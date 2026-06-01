# PowerCSharp GitFlow Workflow

## Branching Strategy

We follow the GitFlow branching convention for professional development:

### 🌳 Main Branches

- **`main`**: Production-ready releases
  - Only contains stable, tested code
  - Tags are created for releases (v1.0.0, v1.1.0, etc.)
  - Protected branch - requires PR for changes

- **`develop`**: Integration branch for features
  - Contains the latest developed features
  - All feature branches merge into develop
  - Serves as the base for new feature branches

### 🚀 Feature Branches

- **`feature/feature-name`**: New features and improvements
  - Created from `develop`
  - Merged back to `develop` via Pull Request
  - Naming convention: `feature/description-of-feature`

### 🔄 Workflow Process

#### 1. Creating a New Feature
```bash
# Start from develop branch
git checkout develop
git pull origin develop

# Create feature branch
git checkout -b feature/your-feature-name
```

#### 2. Developing the Feature
- Make your changes on the feature branch
- Commit frequently with descriptive messages
- Test your changes thoroughly

#### 3. Creating Pull Request
```bash
# Push feature branch
git push -u origin feature/your-feature-name

# Create PR from feature/your-feature-name -> develop
# Include:
# - Clear description of changes
# - Testing performed
# - Any breaking changes
```

#### 4. Merging to Develop
- PR is reviewed and approved
- Merged into develop branch
- Feature branch is deleted

#### 5. Releasing to Main
```bash
# When develop is ready for release
git checkout main
git pull origin main
git merge develop
git tag v1.0.0
git push origin main --tags
```

## 📋 Current Branch Structure

```
main                    ← Production releases
├── develop             ← Integration branch
    ├── feature/initial-library-structure ← Current feature work
    └── feature/future-features
```

## 🎯 Commit Message Convention

We use conventional commits for clarity:

- `feat:` New features
- `fix:` Bug fixes
- `docs:` Documentation changes
- `style:` Code formatting (no functional changes)
- `refactor:` Code refactoring
- `test:` Adding or updating tests
- `chore:` Maintenance tasks

### Examples
```bash
feat: Add string extension methods for validation
fix: Resolve null reference exception in JsonHelper
docs: Update README with installation instructions
refactor: Optimize regex performance with GeneratedRegexAttribute
```

## 🛡️ Branch Protection Rules

### Main Branch
- Require pull request reviews
- Require status checks to pass (build, tests)
- Require up-to-date branches before merging
- Include administrators

### Develop Branch
- Require pull request reviews
- Require status checks to pass
- Include administrators

## 🚨 Important Notes

1. **Never commit directly to main**
2. **Always create feature branches from develop**
3. **Keep feature branches focused and small**
4. **Write clear commit messages**
5. **Update documentation with changes**
6. **Test thoroughly before creating PRs**

## 🔄 Current Workflow Status

✅ **Completed:**
- Initial library structure implemented
- All projects created and configured
- Comprehensive tests and samples
- CI/CD pipeline set up with GitHub Packages support
- Interface reorganization completed (v0.1.0 preparation)
  - All interfaces moved to PowerCSharp.Core
  - Namespace structure: `PowerCSharp.Core.Interfaces.Extensions.*`
  - Documentation updated to reflect new architecture
  - Project references and using statements updated

� **In Progress:**
- Ready for v0.1.0 release preparation
- Final testing and validation

📋 **Next Steps:**
1. Create v0.1.0 release on GitHub
2. Trigger CI/CD pipeline for package publishing
3. Verify packages appear on both NuGet.org and GitHub Packages
4. Update documentation with release notes
