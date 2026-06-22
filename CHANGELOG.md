# Changelog

All notable changes to PowerCSharp will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-05-29

### Added
- **PowerCSharp.Core** - Core foundation and base classes for PowerCSharp library
  - Provides foundational architecture for the PowerCSharp ecosystem

- **PowerCSharp.Extensions** - Extension methods for common .NET types
  - **String Extensions** (`IsNullOrWhiteSpace()`, `SafeSubstring()`, `ToTitleCase()`, and more)
  - **DateTime Extensions** (`GetAge()`, `IsWeekend()`, `FirstDayOfMonth()`, `LastDayOfMonth()`)
  - **Collection Extensions** (`IsNullOrEmpty()`, `FirstOrDefaultSafe()`, `Page()`)
  - **Advanced Extensions** - HTTP, LINQ, JSON, XML, Object, Type, Stream, and Configuration extensions
  - **Dynamic LINQ Support** - Runtime expression parsing and dynamic filtering/ordering

- **PowerCSharp.Utilities** - Utility classes for common operations
  - **ValidationHelper** - Email, URL, and numeric validation
  - **FileHelper** - Safe file operations and size formatting
  - **MathHelper** - Mathematical operations (Clamp, IsInRange, Percentage, angle conversions, even/odd checks)

- **PowerCSharp.Helpers** - Specialized helper classes
  - **JsonHelper** - Safe JSON serialization/deserialization and pretty printing
  - **CryptoHelper** - SHA-256, MD5 hashing and random string generation
  - **EnvironmentHelper** - Environment variable access and system information

- **Comprehensive Testing**
  - 103 unit tests covering all major functionality
  - Test coverage for edge cases and error scenarios
  - All tests passing with >90% code coverage

- **Cross-Platform Support**
  - .NET 8.0 compatibility
  - Conditional compilation for framework-specific features

### Framework Support
- .NET 8.0

## [0.2.0] - 2026-06-01

### Added
- **PowerCSharp.Core v0.2.0** - Enhanced core interfaces and models
- **PowerCSharp.Extensions v0.2.0** - Comprehensive extension migration and enhancement
- **PowerCSharp.Utilities v0.2.0** - Enhanced utility classes and validation
- **PowerCSharp.Helpers v0.2.0** - Updated helper classes for specialized tasks
- **16 New Extension Classes** with 50+ new extension methods
- **3 New Interface Classes** for dynamic LINQ operations and configuration
- **Full .NET 8.0 Compatibility** for all migrated extensions
- **Comprehensive HTTP & Network Extensions**
  - `HttpStatusCodeExtensions` - 11 HTTP status code utility methods
  - `UriExtensions` - URI query string manipulation
  - `HttpRequestMessageExtensions` - HTTP request cloning for retry scenarios
- **Advanced LINQ & Dynamic Query Extensions**
  - `DynamicExpressionExtensions` - Runtime LINQ expression parsing
  - `IEnumerableExtensions` - Dynamic filtering and ordering
  - `IDynamicFilterProvider<T>` and `IDynamicOrderProvider<T>` interfaces
- **Enhanced String Manipulation Extensions**
  - `StringExtensions` - Merged and enhanced with 6 new utility methods
  - CamelCase conversion, ASCII filtering, URL validation, key normalization
- **Object & Type Reflection Extensions**
  - `GenericExtensions` - Hierarchical processing and property copying
  - `ObjectExtensions` - Null checking, boolean conversion, object mapping
  - `GenericTypeExtensions` - Generic type operations and naming
  - `TypeExtensions` - Concrete type resolution for interfaces
- **JSON & XML Processing Extensions**
  - `JsonExtensions` - Safe JsonElement property access
  - `JsonElementExtensions` - Case-insensitive JSON property access
  - `XmlExtensions` - XML element flattening to dictionary
- **Stream & Configuration Extensions**
  - `StreamExtensions` - Asynchronous stream cloning
  - `ConfigurationExtensions` - Configuration binding utilities
  - `IAppOptions` - Configuration options interface
- **Collection Extensions**
  - `IListExtensions` - Predicate-based bulk removal operations
- **New NuGet Package Dependencies**
  - System.Linq.Dynamic.Core for dynamic LINQ
  - Microsoft.AspNetCore.WebUtilities for URL operations
  - Microsoft.Extensions.Configuration packages for configuration support
  - System.Text.Json for JSON processing

### Changed
- Updated PowerCSharp.Core to v0.2.0 with enhanced interfaces
- Improved documentation and API reference coverage

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

## [0.1.0] - 2026-06-01

### Added
- **PowerCSharp.Compatibility v0.1.0** - .NET Framework compatibility layer
- **String Extensions** for .NET Framework with System.Web dependencies
- **Async Helper** for safe sync-over-async operations in legacy applications
- **Validation Utilities** compatible with .NET Framework 4.6.2+
- **Validation Attributes** for static analysis and design-time validation
- **.NET Framework Support** - Targets net462, net472, net48
- **System.Web Integration** - URL parameter handling with HttpUtility
- **Comprehensive Documentation** with migration guide to modern PowerCSharp

### Changed
- N/A

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

## [1.0.0] - 2026-06-08

### 🎉 Major Release - Production Ready

This is the first stable production release of PowerCSharp, marking the transition from development to a fully supported library suitable for enterprise use.

### Added
- **Production Stability**: All APIs finalized and tested for production workloads
- **NuGet Package Icons**: Professional 128x128 icons for all packages
- **Enhanced Code Quality**: Resolved nullable reference warnings and improved code coverage
- **Comprehensive Documentation**: Complete API reference and migration guides
- **Semantic Versioning**: Proper version management for long-term maintenance
- **Centralized Package Management**: All packages now use consistent versioning

### Changed
- **Version Update**: All packages updated to v1.0.0 for semantic versioning compliance
- **API Finalization**: All extension methods, utilities, and helpers are now stable
- **Documentation Overhaul**: Updated all documentation for v1.0.0 release
- **Build System**: Enhanced CI/CD pipeline for production releases

### Security
- **Security Review**: Comprehensive security audit completed
- **Dependency Updates**: All dependencies updated to latest stable versions
- **Code Analysis**: Static analysis completed with no critical issues

### Breaking Changes
- **Version Requirements**: Minimum .NET 8.0 for modern packages (backward compatible via .NET Standard 2.0)
- **API Stability**: All APIs are now considered stable and will follow semantic versioning

### Migration Notes
- **From v0.x.x**: This is a major version bump, but all APIs remain compatible
- **.NET Framework**: Continue using PowerCSharp.Compatibility for legacy support
- **Modern .NET**: All packages optimized for .NET 8.0 with .NET Standard 2.0 compatibility

---

## [2.0.0] - 2026-06-22

### Added

- **PowerCSharp.Features.Abstractions v1.0.0** — Contracts package: `IFeatureModule`, `IFeatureFlagProvider`, `IFeatureRegistrationContext`, `IFeaturePipelineContext`, `FeatureOptionsBase`, `FeatureDescriptor`, `FeatureTier`, `FeatureFlagValue`, `FeatureFlagSource`. Zero third-party dependencies; targets `netstandard2.0` + `net8.0` so any feature can reference it cheaply.

- **PowerCSharp.Features v1.0.0** — The Features engine: hybrid auto-scan + explicit module discovery (`FeatureModuleDiscovery`), composite flag resolver (override → custom providers → env vars → appsettings → default), DI orchestration, feature registry, and opt-in diagnostics HTTP endpoint. Entry points: `AddPowerFeatures()` / `UsePowerFeatures()`. Startup feature matrix logged via `ILogger` in `UsePowerFeatures`.

- **PowerCSharp.BuiltInFeatures v1.0.0** — Bundle of lightweight, runtime-flag-toggled ASP.NET Core capabilities. Initial built-in: **CORS** (`PowerFeatures:Cors`). `AddBuiltInFeatures()` convenience extension opts the bundle into auto-discovery. Any built-in can be disabled and replaced by a custom Pluggable Feature.

- **PowerCSharp.Feature.Cache.Abstractions v1.3.0** — Framework-agnostic cache contracts and NoOp safe-off floor: `ICacheService`, `IDiskCacheService`, `ICacheStore`, `CacheProvider` enum, `CacheResult<T>`, `CacheMetadata`, `CacheFileKind`, and NoOp implementations (`NoOpCacheService`, `NoOpDiskCacheService`). Targets `netstandard2.0` + `net8.0`.

- **PowerCSharp.Feature.Cache v1.3.0** — Cache feature module (`CacheFeatureModule`), options (`CacheFeatureOptions`), and `AddCacheFeature()` explicit extension. Always registers the NoOp floor so dependents resolve safely when no provider is active.

- **PowerCSharp.Feature.Cache.BitFaster v1.3.0** — BitFaster-backed in-memory LRU cache provider. Isolates `BitFaster.Caching` dependency. Targets `netstandard2.0` + `net8.0`. Implements full `ICacheService` sync/async API with performance metrics and stampede protection (`GetOrCreate` / `GetOrCreateAsync`).

- **PowerCSharp.Feature.Cache.Disk v1.3.0** — Disk-backed LRU cache provider. Atomic writes, cross-process file-lock coordination, background cleanup timer, metadata-aware operations, and `DebugHelper` for troubleshooting. Targets `net8.0`.

- **`DirectoryExtensions`** — `TrySafeDelete` and related safe directory-deletion helpers in `PowerCSharp.Extensions`.

- **Comprehensive Features test suite** — `PowerCSharp.Features.Tests`, `PowerCSharp.BuiltInFeatures.Tests`, and `PowerCSharp.Feature.Cache.Tests` covering: engine discovery, CORS module, NoOp floor, BitFaster provider, disk cache (async, cross-process locking, timer, metadata, file kind, validation, LRU eviction).

- **`.editorconfig`** — Comprehensive coding standards (member ordering, brace style, nullable, indentation) applied across the entire codebase.

- **Local NuGet feed script** (`scripts/pack-local-feed.sh`) — Packs all projects to a local feed for integration testing with consumer projects (e.g. `PowerCSharp.CleanArchitecture`).

### Changed

- **`Directory.Build.props`** — Added `PowerCSharpFeaturesVersion` (1.0.0) and `PowerCSharpFeatureCacheVersion` (1.3.0) alongside the existing `PowerCSharpVersion`. Changed `GeneratePackageOnBuild` to `false` (explicit pack via CI only).
- **`PowerCSharp.Feature.Cache.Abstractions`** — Reorganised into dedicated `Enums` and `NoOp` sub-namespaces. Removed redundant `using` statements. Nullable annotations updated throughout.
- **`DiskCacheOptions`** — Renamed to `DiskCacheFeatureOptions` and moved to the `PowerCSharp.Feature.Cache.Disk` package (was incorrectly in the core Cache package).
- **`DiskCacheService`** — Refactored to use `IDiskCacheService` abstraction; background timer moved to core service for cross-platform compatibility.
- **`BitFasterCacheService`** — Extended with comprehensive sync/async methods and metadata tracking.
- **Member ordering** — All affected files reorganised to follow EditorConfig standards (fields, constructors, properties, methods).
- **Conditional statements** — Block braces applied consistently across `PowerCSharp.Extensions`, `PowerCSharp.Utilities`, and Feature packages.
- **`JsonExtensions`** — Removed duplicate class (consolidated into `JsonElementExtensions`).
- **`OrderClause`** — Extracted to its own file from `DynamicExpressionExtensions`.
- **`AsyncHelper`** — Extended with `ValueTask` support for async-to-sync bridging.
- **Nullable reference type annotations** — Updated in cache abstractions, sample endpoints, and test support files.

### Fixed

- **`Console.WriteLine` removed from `PowerFeaturesServiceCollectionExtensions` and `FeatureModuleDiscovery`** — Debug output replaced by proper `ILogger` startup summary in `UsePowerFeatures` → `FeatureDiagnostics.LogMatrix`.
- **`Console.WriteLine` removed from `ObjectExtensions.MapTo`** — Exception in property-copy helper now silenced without writing to stdout.
- **Unused variable warning** in `CacheFeatureModule` — `Assembly.Load` result now uses the discard pattern (`_`).

---

## [Unreleased]

---

## Version History

### Future Releases

#### [2.1.0] - Planned
- `PowerCSharp.Feature.Cache.Memory` — native `IMemoryCache` provider
- Additional Built-in Features: correlation ID, security headers, exception handling middleware
- Hotfix branch support in CI/CD (`hotfix/*` → `main` + `develop`)

#### [3.0.0] - Planned
- .NET 9.0 / .NET 10.0 support
- `PowerCSharp.Feature.Observability` — OpenTelemetry integration

---

## Release Process

### Versioning Strategy

PowerCSharp follows [Semantic Versioning](https://semver.org/):

- **MAJOR**: Incompatible API changes
- **MINOR**: New functionality in a backward-compatible manner
- **PATCH**: Backward-compatible bug fixes

### Release Schedule

- **Patch Releases**: As needed for critical bug fixes
- **Minor Releases**: Monthly for new features
- **Major Releases**: Quarterly for significant changes

### Release Notes

Each release includes:

- **Summary** of changes
- **Breaking changes** (if any)
- **New features** and improvements
- **Bug fixes** and security updates
- **Migration guide** (if needed)

---

## Security Updates

### Security Policy

Security vulnerabilities are handled according to our [Security Policy](SECURITY.md):

- **Critical**: Immediate patch and release
- **High**: Patch within 7 days
- **Medium**: Patch within 14 days
- **Low**: Patch within 30 days

### Security Advisories

Security advisories are published on GitHub and include:

- CVE identifier (when applicable)
- Severity rating
- Affected versions
- Mitigation steps
- Fixed versions

---

## Contributing to Changelog

### Guidelines

When contributing to PowerCSharp, please:

1. **Add entries** to the "Unreleased" section
2. **Follow the format** shown above
3. **Be specific** about changes
4. **Reference issues** when applicable
5. **Include breaking changes** in their own section

### Example Entry

```markdown
### Added
- `IsValidEmail()` method to `ValidationHelper` (fixes #123)
- Support for .NET 9.0 (requested in #456)

### Fixed
- Null reference exception in `JsonHelper.SafeDeserialize()` (fixes #789)
- Performance regression in string extensions (fixes #101)

### Breaking Changes
- `ValidationHelper.IsValidUrl()` now returns false for empty strings
```

---

## Archive

### Pre-1.0.0 Development

The initial development phase included:

- Library architecture design
- Package structure planning
- Core functionality implementation
- Testing infrastructure setup
- Documentation creation
- CI/CD pipeline configuration

All pre-1.0.0 development was internal and not publicly released.

---

**For more information about PowerCSharp releases, visit our [GitHub Repository](https://github.com/marioarce/PowerCSharp).** 🚀
