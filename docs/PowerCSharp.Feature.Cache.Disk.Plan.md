# PowerCSharp.Feature.Cache.Disk — Architecture & Migration Plan

Status: Planning. Source of truth for the port: `one_ahm_webapi` `DiskCacheService` and collaborators, **fully de-branded** (no Sentry, Sitecore, Econfig2, Eshopping, Vehicle, Iris, or any client names).

## Goals

- Ship a production-grade, de-branded disk-backed LRU cache as a standalone pluggable feature.
- Keep the implementation as close as possible to the proven production code, adapted to PowerCSharp style.
- Make the cache run on **both .NET Core and .NET Framework** (`netstandard2.0;net8.0`).
- Reuse the PowerCSharp family (`Core`, `Helpers`) — they exist to be consumed by us and everyone else.

## Target Framework (TFM) policy

Features floor at **`netstandard2.0`** when the capability is runtime-agnostic (I/O, serialization, algorithms). Use **`net8.0`-only** or a dedicated **`PowerCSharp.Feature.AspNetCore.{xyz}`** package only when the feature genuinely needs the ASP.NET Core pipeline (middleware, hosting, MVC, endpoints). Disk caching is runtime-agnostic, so it floors at `netstandard2.0`.

## Package structure (decided)

| Package | TFM | Role |
|---|---|---|
| `PowerCSharp.Feature.Cache.Abstractions` | `netstandard2.0;net8.0`, zero third-party deps | Pure contracts + POCOs + **NoOp floors**: `ICacheStore`, `ICacheService`, `IDiskCacheService`, `CacheFeatureOptions`, NoOp implementations, `CacheResult<T>`, `CacheEntryMetadata`, `CacheFileKind` + static registry. |
| `PowerCSharp.Feature.Cache` | `net8.0` | ASP.NET feature-module layer only: `CacheFeatureModule`, `AddCacheFeature`. References `Cache.Abstractions` + `Features.Abstractions` (the latter is `net8.0` + AspNetCore via `IApplicationBuilder`, so the module stays `net8.0`). |
| `PowerCSharp.Feature.Cache.BitFaster` | `netstandard2.0;net8.0` | In-memory provider. References `Cache.Abstractions` + `BitFaster.Caching` (2.5.2, netstandard2.0-safe) + `Microsoft.Extensions.Options` + DI abstractions. Drops the AspNetCore framework ref. |
| `PowerCSharp.Feature.Cache.Disk` | `netstandard2.0;net8.0` | De-branded production disk LRU cache. References Abstractions + Core + Helpers only. |

Dependency direction: providers (`BitFaster`, `Disk`) reference only `Cache.Abstractions` (+ Core/Helpers for Disk), never the `net8.0` ASP.NET module. Framework consumers reference `Cache.Abstractions` + a provider directly; the module is for ASP.NET hosts.

## Key design decisions

- **No `GetOptions()` in the core service.** The service takes a plain `DiskCacheOptions` POCO via constructor — framework-agnostic and unit-testable.
- **Config binding stays optional and framework-safe.** `AddCacheDisk(IServiceCollection, IConfiguration)` uses `Microsoft.Extensions.DependencyInjection.Abstractions` + `Microsoft.Extensions.Configuration.Abstractions` (both `netstandard2.0`). No ASP.NET Core required.
- **Serializer:** `System.Text.Json` always (already pinned by `Helpers` at 10.0.8; `netstandard2.0`-safe).
- **NoOp floors live in `Cache.Abstractions`** so Framework consumers get a safe default without the ASP.NET module.
- **Background cleanup / LRU eviction:**
  - Core service exposes synchronous primitives `PurgeExpired()` / `EvictToLimit()` (+ async variants) — zero host dependency, run on Framework.
  - Portable self-cleanup on all TFMs via an internal `System.Threading.Timer` gated by options (`EnableBackgroundCleanup`, `CleanupInterval`); service is `IDisposable` to stop it.
  - On `net8.0` (`#if NET8_0_OR_GREATER`) also implement `IHostedService` for host start/stop + graceful shutdown; registration wires it as a hosted service on net8.0 only.
- **Eviction policy (v1 = subset):** port a subset of production semantics for v1 (count cap + TTL expiry); size-cap and advanced metrics can follow. LRU recency tracked via the index.
- **Cross-process locking = file-lock coordination (configurable):** atomic writes (temp file + `File.Move`/replace) so readers never see partial files; per-key cross-process coordination via named `Mutex`/`.lock` sidecar on the write path; single named mutex around the index read-modify-write. Toggle via `EnableCrossProcessLocking` for single-process speed.
- **Index storage = single JSON index file** (matches production), updated atomically.
- **Cache directory default:** derive a cross-platform default under `Path.GetTempPath()` + app/feature name when options don't specify one.
- **`CacheFileKind` = smart-enum value object + static registry.** Each kind is a `static readonly` instance (name, extension, optional subdir); `CacheFileKindRegistry.Register(...)` lets extension packages (e.g. a future `PowerCSharp.Sitecore`) contribute kinds from their registration entry point. Static (not DI) so it works identically on Framework and Core, and is available before any container exists.
- **Diagnostics:** add `CacheEntryMetadata` and `CacheResult<T>` for hit/miss/age/source metadata.
- **Options validation:** guard clauses in the POCO/service (no DataAnnotations/AspNetCore dependency).

## Versioning

Nothing is published yet, so **all Features packages stay at `1.0.0`** (no `1.3.0` bump). `Cache.Abstractions` and `Feature.Cache.Disk` are new at `1.0.0`.

## Work breakdown

1. Create `PowerCSharp.Feature.Cache.Abstractions` (`netstandard2.0;net8.0`); move contracts + `CacheFeatureOptions` + NoOp floors there; repoint `Feature.Cache` (module only) and `Feature.Cache.BitFaster` to it; multi-target BitFaster; keep all versions at `1.0.0`; build/test/pack.
2. Create `PowerCSharp.Feature.Cache.Disk` (`netstandard2.0;net8.0`); port `DiskCacheService` + index/LRU/options de-branded; POCO options; `AddCacheDisk` registration.
3. `CacheFileKind` smart-enum + static registry; `CacheEntryMetadata` + `CacheResult<T>`.
4. Sync `PurgeExpired`/`EvictToLimit` core; portable timer (all TFMs) + `net8.0` `IHostedService` sweeper; atomic writes + file-lock coordination; single JSON index; cross-platform default dir.
5. Tests: roundtrip, eviction (count cap + TTL), expiry, concurrency/locking, file-kind registry.
6. Template integration: `AddCacheDisk` wiring + disk cache samples in `PowerCSharp.CleanArchitecture`.
7. Docs: package READMEs + fold the TFM policy into the Features docs.
