# Changelog

All notable changes to PowerCSharp will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial library structure with four main packages
- Core string manipulation and validation extensions
- Extension methods for collections and DateTime operations
- Utility classes for validation and file operations
- Helper classes for JSON, cryptography, and environment operations
- Comprehensive unit tests for all packages
- Console and web application samples
- CI/CD pipeline with GitHub Actions
- Documentation and workflow guidelines

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

## [1.0.0] - 2026-05-29

### Added
- **PowerCSharp.Core** - Core string manipulation and validation extensions
  - `IsNullOrWhiteSpace()` extension method
  - `ToTitleCase()` string formatting
  - `SafeSubstring()` safe string extraction
  - Email validation utilities
  - String sanitization methods

- **PowerCSharp.Extensions** - Extension methods for common .NET types
  - Collection extensions (`IsNullOrEmpty()`, `FirstOrDefaultSafe()`, `Page()`)
  - DateTime extensions (`GetAge()`, `IsWeekend()`, `FirstDayOfMonth()`, `LastDayOfMonth()`)
  - LINQ enhancements and utilities

- **PowerCSharp.Utilities** - Utility classes for common operations
  - `ValidationHelper` for email, URL, and numeric validation
  - `FileHelper` for safe file operations and size formatting
  - `MathHelper` for mathematical calculations and conversions

- **PowerCSharp.Helpers** - Specialized helper classes
  - `JsonHelper` for safe JSON serialization/deserialization
  - `CryptoHelper` for SHA-256, MD5, and random string generation
  - `EnvironmentHelper` for environment variable access

- **Testing Infrastructure**
  - Comprehensive unit tests for all packages
  - Test coverage for edge cases and error scenarios
  - Integration tests for complex scenarios

- **Samples and Documentation**
  - Console application demonstrating all features
  - Web application sample for ASP.NET Core integration
  - Comprehensive README with usage examples
  - API documentation with XML comments

- **Development Tools**
  - GitHub Actions CI/CD pipeline
  - Automated testing and build processes
  - Code quality checks and formatting
  - Dependency management and security scanning

### Framework Support
- .NET 8.0
- .NET Standard 2.0 (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

### Documentation
- Complete API documentation
- Usage examples for all major features
- Contributing guidelines
- Security policy
- Code of conduct

---

## Version History

### Future Releases

#### [1.1.0] - Planned
- Additional string manipulation methods
- Enhanced cryptographic utilities
- Performance optimizations
- More validation helpers

#### [1.2.0] - Planned
- Async extension methods
- Caching utilities
- Logging helpers
- Configuration extensions

#### [2.0.0] - Planned (Breaking Changes)
- .NET 9.0 support
- Updated API design
- Removed deprecated methods
- Enhanced performance

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
