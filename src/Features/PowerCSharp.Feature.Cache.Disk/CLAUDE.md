# CLAUDE.md — PowerCSharp.Feature.Cache.Disk

> Scope: this project only (`DiskCacheService`, `DiskCacheIndex`, `DiskCacheIndexEntry`,
> `DiskCacheBackgroundService`, `DiskCacheFeatureOptions`). Read alongside `src/Features/CLAUDE.md`
> and the root `CLAUDE.md`.

## Sensitivity level: high

This is the one package in the repository doing real cross-process concurrency control and
non-transactional file I/O. Bugs here are silent-corruption or silent-deadlock bugs, not
compile-time or even easily-reproducible-in-a-unit-test bugs. Any change to locking, atomic write
ordering, or index serialization needs a `<risk>` block in your response and, ideally, a
multi-process manual test, not just the existing unit test suite.

## 1. What This Package Actually Does (grounded in `DiskCacheService.cs`)

- Stores each cache value as its own JSON file under `_rootDirectory` (default
  `%TEMP%/powercsharp-disk-cache`, overridable via `DiskCacheFeatureOptions.DirectoryPath`), named
  by a hash of the key (`HashFileName`), plus a single `index.json` mapping keys → 
  `DiskCacheIndexEntry` (file path, created/last-accessed/expires timestamps).
- **In-process locking** is two-layered:
  - `_indexLock` (a plain `object` + `lock`) guards all in-memory mutation of `_index.Entries`.
  - Per-key `SemaphoreSlim`s (`_keyLocks`, `ConcurrentDictionary<string, SemaphoreSlim>`) serialize
    concurrent `Get`/`Set`/`Remove` calls for the *same key within this process*.
- **Cross-process locking** is opt-in via `DiskCacheFeatureOptions.EnableCrossProcessLocking`,
  implemented with named, global `Mutex`es:
  - One index mutex per root directory: `Global\PowerCSharp_DiskCache_Index_{hash}`.
  - One mutex per key: `Global\PowerCSharp_DiskCache_Key_{hash}`, created lazily and cached in
    `_keyMutexes`, **never removed** for the lifetime of the service (see Hazard 1 below).
  - `WithIndexLock` / `WithKeyLock` wrap the corresponding critical section; both are no-ops if
    `EnableCrossProcessLocking` is `false` — in that mode, only the in-process locks apply, and
    multiple *processes* sharing a directory can race.
- **Writes are atomic** at the OS level: value writes go to `<file>.tmp` then `File.Move` to the
  final path (never write-in-place); `SaveIndex()` follows the identical temp-then-move pattern for
  `index.json`. This is what makes a torn read of a `.json` value or of the index itself
  structurally impossible under normal OS semantics — do not "simplify" this back to a direct
  `File.WriteAllText` on the real path.
- **Eviction** is LRU by `LastAccessedUtc`, enforced in `EvictIfNeeded()` (inline, on every
  `SetAsync`) and `EvictToLimit()` (callable directly). **TTL** expiry is checked lazily on read
  (`IsExpired`) and swept proactively by `PurgeExpiredAsync`, invoked on a `System.Threading.Timer`
  when `EnableBackgroundCleanup` is `true` (constructor, `StartCleanupTimer`).
- `DiskCacheBackgroundService` (net8.0 only, `#if NET8_0_OR_GREATER`) is an `IHostedService`
  wrapper for host lifecycle logging only — the actual timer lives in `DiskCacheService` itself and
  runs on every target framework, including `netstandard2.0` hosts with no `IHostedService` concept.

## 2. Known Hazards — Do Not Touch Without Addressing These

1. **`_keyMutexes` grows unbounded.** Every distinct cache key ever used with cross-process locking
   enabled creates a `Mutex` that lives until `Dispose()`. A cache with high key cardinality and
   `EnableCrossProcessLocking = true` will leak OS mutex handles for the life of the process. If
   you're asked to fix this, the fix needs its own eviction policy for `_keyMutexes` — coordinated
   with, but distinct from, the LRU eviction of cache *entries* in §1, since a mutex must never be
   disposed while another thread/process could still be waiting on it.
2. **Mutex hash collisions.** Mutex names are derived from `key.GetHashCode():X8` /
   `_rootDirectory.GetHashCode():X8` — a 32-bit hash. Two distinct keys colliding on this hash will
   silently share a lock (harmless — over-serializes, doesn't corrupt) but two distinct
   **root directories** colliding would cross-serialize unrelated caches. Low probability, non-zero;
   do not "optimize" this to a shorter hash.
3. **`GetOrCreateAsync` factory runs inside the per-key semaphore**, not inside the cross-process
   mutex. Under `EnableCrossProcessLocking = true` with multiple processes, two processes can both
   run the factory concurrently for a cold key before either writes — last writer wins, no
   duplicate-work prevention across processes by design. Do not describe this method as
   preventing cross-process duplicate computation; it only prevents duplicate computation
   in-process.
4. **`Global\` mutex namespace requires appropriate OS privileges** in some locked-down Windows
   environments (e.g. certain container/service-account configurations) and has no direct
   equivalent semantics on non-Windows platforms beyond what .NET's `Mutex` emulates. If you extend
   this feature to a new OS target, re-validate this mechanism specifically — do not assume named
   `Mutex` behaves identically across platforms just because it compiles under `netstandard2.0`.
5. **No corruption-detection on partial writes from a hard process kill mid-`File.Move`.** The
   temp-then-move pattern protects against torn writes, not against a crash between deleting the
   old file and completing the move (`SetAsync`: `File.Delete(filePath)` then `File.Move(tempPath,
   filePath)` are two separate syscalls). A crash in that narrow window loses the entry (the old
   file is gone, the new one didn't land) — this degrades to a cache miss on next read, which is
   safe for a cache, but should not be described as fully atomic across a crash boundary.

## 3. If You Are Asked to Modify This Package

- Reproduce hazards with a **multi-process** test harness (e.g. two console apps or two xunit test
  processes pointed at the same `DirectoryPath`) before claiming a fix works — the existing
  `tests/PowerCSharp.Feature.Cache.Tests` suite runs in a single process and cannot exercise cross-
  process contention.
- Never remove the temp-file-then-move pattern for either cache value files or `index.json`.
- Never make `WithIndexLock`/`WithKeyLock` a no-op path change without preserving the existing
  `EnableCrossProcessLocking` gate — some consumers deliberately run with it `false` for
  single-process performance.
- Call out any change to mutex lifetime, naming, or scope explicitly as `<risk>` per the root
  `CLAUDE.md` Section 5 — this is exactly the class of change that passes single-process unit tests
  while breaking multi-process production behavior.
