# Contributing to PowerCSharp

Thank you for your interest in contributing to PowerCSharp! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, or JetBrains Rider)

### Setting Up Your Development Environment

1. **Fork the Repository**
   ```bash
   # Fork the repository on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/PowerCSharp.git
   cd PowerCSharp
   ```

2. **Add Upstream Remote**
   ```bash
   git remote add upstream https://github.com/marioarce/PowerCSharp.git
   ```

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Solution**
   ```bash
   dotnet build
   ```

5. **Run Tests**
   ```bash
   dotnet test
   ```

## 📋 Development Workflow

### Branch Strategy

We follow [GitFlow](docs/WORKFLOW.md) branching conventions:

- `main` - Production-ready releases
- `develop` - Integration branch for features
- `feature/feature-name` - New features and improvements
- `hotfix/issue-description` - Critical bug fixes

### Creating a Feature Branch

```bash
# Ensure you're on the latest develop branch
git checkout develop
git pull upstream develop

# Create your feature branch
git checkout -b feature/your-feature-name
```

### Making Changes

1. **Code Style**: Follow the existing code style and conventions
2. **Documentation**: Update XML documentation for public APIs
3. **Tests**: Add unit tests for new functionality
4. **Build**: Ensure the solution builds without warnings

### Commit Guidelines

We use [Conventional Commits](https://www.conventionalcommits.org/) specification:

- `feat:` New features
- `fix:` Bug fixes
- `docs:` Documentation changes
- `style:` Code formatting (no functional changes)
- `refactor:` Code refactoring
- `test:` Adding or updating tests
- `chore:` Maintenance tasks

**Examples:**
```bash
feat: Add string extension for email validation
fix: Resolve null reference exception in JsonHelper
docs: Update README with installation instructions
test: Add unit tests for DateTimeExtensions
```

### Submitting Changes

1. **Push Your Branch**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create Pull Request**
   - Target: `develop` branch
   - Include clear description
   - Reference any related issues
   - Include testing performed

3. **Code Review**
   - Address reviewer feedback
   - Keep PR focused and small
   - Ensure all CI checks pass

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/PowerCSharp.Core.Tests
```

### Test Guidelines

- Write unit tests for all public APIs
- Use descriptive test names
- Follow Arrange-Act-Assert pattern
- Mock external dependencies
- Achieve good code coverage (>80%)

## 📝 Code Standards

### C# Coding Guidelines

- Use PascalCase for public members
- Use camelCase for private members
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods small and focused
- Use async/await for asynchronous operations

### Example Code Style

```csharp
/// <summary>
/// Validates if the specified string is a valid email address.
/// </summary>
/// <param name="email">The email string to validate.</param>
/// <returns>True if the email is valid; otherwise, false.</returns>
public static bool IsValidEmail(this string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return false;
    
    try
    {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
    }
    catch
    {
        return false;
    }
}
```

## 📦 Package Guidelines

### Adding New Packages

1. **Create New Project** in appropriate folder (`src/`)
2. **Update Solution** to include new project
3. **Add Tests** project
4. **Update Documentation** in README
5. **Create Sample** if needed

### Package Structure

```
src/PowerCSharp.NewPackage/
├── PowerCSharp.NewPackage.csproj
├── Extensions/
├── Helpers/
└── Utilities/

tests/PowerCSharp.NewPackage.Tests/
├── PowerCSharp.NewPackage.Tests.csproj
└── Tests/
```

## 🐛 Bug Reports

### Reporting Bugs

1. **Search existing issues** first
2. **Use the bug report template**
3. **Provide detailed information**:
   - Version of PowerCSharp
   - .NET version
   - Operating system
   - Reproduction steps
   - Expected vs actual behavior
   - Stack trace (if applicable)

## 💡 Feature Requests

### Requesting Features

1. **Search existing issues** first
2. **Use the feature request template**
3. **Provide clear description** of the feature
4. **Explain the use case** and benefit
5. **Consider implementation** suggestions

## 📖 Documentation

### Documentation Types

- **API Documentation**: XML comments in code
- **README.md**: Project overview and quick start
- **docs/**: Detailed guides and references
- **Samples/**: Working examples
- **CHANGELOG.md**: Version history

### Writing Documentation

- Use clear, concise language
- Include code examples
- Provide context and use cases
- Keep documentation up-to-date

## 🔧 Development Tools

### Recommended Extensions

- **Visual Studio**: 
  - Code Analysis
  - Git Tools
  - NuGet Package Manager

- **VS Code**:
  - C# Dev Kit
  - GitLens
  - .NET Runtime Install Tool

### Build Commands

```bash
# Build solution
dotnet build

# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack
```

## 🤝 Community

### Getting Help

- **GitHub Issues**: Report bugs and request features
- **GitHub Discussions**: Ask questions and share ideas
- **Email**: support@marioarce.dev

### Code of Conduct

Please read our [Code of Conduct](CODE_OF_CONDUCT.md) to understand our community expectations.

## 📄 License

By contributing to PowerCSharp, you agree that your contributions will be licensed under the [MIT License](LICENSE).

## 🙏 Thank You

We appreciate all contributions to PowerCSharp! Whether you're reporting a bug, fixing an issue, or suggesting a new feature, your help makes this project better for everyone.

---

**Happy Coding!** 🚀
