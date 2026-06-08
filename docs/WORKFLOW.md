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
  - **Automatically deleted** after merge to develop

### 📦 Release Branches

- **`release/version-number`**: Release preparation branches
  - Created manually from `develop` when ready for release
  - Used for final testing, documentation updates, version bumping
  - Merged to `main` via Pull Request for production release
  - **Automatically deleted** after merge to main
  - Triggers NuGet package publishing to NuGet.org and GitHub Packages
  - Naming convention: `release/v1.0.0`, `release/v1.1.0`, etc.

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

#### 5. Creating a Release
```bash
# When develop is ready for release
git checkout develop
git pull origin develop

# Create release branch
git checkout -b release/v1.0.0

# Update version numbers, documentation, changelog
# Perform final testing and validation
# Commit release preparation changes
git add .
git commit -m "chore: Prepare v1.0.0 release"
git push -u origin release/v1.0.0

# Create PR from release/v1.0.0 -> main
# Include:
# - Release notes
# - Testing performed
# - Version changelog
```

#### 6. Release to Production
- PR is reviewed and approved
- Merged into main branch
- **CI/CD Pipeline automatically:**
  - Builds and tests the release
  - Packs NuGet packages
  - Publishes to NuGet.org and GitHub Packages
  - Deletes the release branch
- Main branch is tagged with release version

## 📋 Current Branch Structure

```
main                    ← Production releases
├── develop             ← Integration branch
    ├── feature/initial-library-structure ← Current feature work
    └── feature/future-features
└── release/v1.0.0     ← Release preparation (temporary)
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

## � CI/CD Automation

Our GitHub Actions workflow automatically handles builds, tests, releases, and branch cleanup.

### 🚀 Triggers

The CI/CD pipeline runs on:

**Push Events:**
- `main` branch - Build, test, and validate production code
- `develop` branch - Build, test, and validate integration code  
- `release/*` branches - Build, test, and validate release preparation

**Pull Request Events:**
- PRs to `main` - Build, test, and validate production changes
- PRs to `develop` - Build, test, and validate feature integration

### 📦 Automated Jobs

#### Build and Test
- **Runs on**: All pushes to `main`, `develop`, `release/*`
- **Runs on**: All PRs to `main`, `develop`
- **Actions**: Restore dependencies, build solution, run tests with coverage
- **Coverage**: Uploads to Codecov for tracking

#### Feature Branch Cleanup
- **Triggers**: When feature branch is merged to `develop`
- **Action**: Automatically deletes the feature branch
- **Purpose**: Keeps repository clean after feature integration

#### Release Process
- **Triggers**: When `release/*` branch is merged to `main`
- **Actions**:
  1. **Pack**: Creates NuGet packages from release
  2. **Publish**: Publishes to NuGet.org and GitHub Packages
  3. **Cleanup**: Deletes the release branch
- **Result**: Production packages available for download

#### Release Branch Cleanup  
- **Triggers**: When `release/*` branch is merged to `main`
- **Action**: Automatically deletes the release branch
- **Purpose**: Clean up after successful release

### 🎯 Smart Conditions

The workflow uses intelligent conditions to ensure jobs run only when appropriate:

- **Feature deletion**: Only when merged to `develop` (not `main`)
- **Release publishing**: Only when `release/*` merged to `main` (not any `main` push)
- **Branch cleanup**: Error handling prevents workflow failures

### 📋 Workflow Summary

```
Feature Branch → PR to develop → Build/Test → Auto-delete feature branch
     ↓ (when ready for release)
Release Branch → PR to main → Build/Test → Pack/Publish → Auto-delete release branch
```

## �️ Branch Protection Rules

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
3. **Create release branches from develop (not from feature branches)**
4. **Keep feature branches focused and small**
5. **Write clear commit messages**
6. **Update documentation with changes**
7. **Test thoroughly before creating PRs**
8. **Release branches trigger automatic NuGet publishing**
9. **Branches are automatically deleted after successful merges**

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
- **Enhanced CI/CD workflow implemented**
  - Automatic feature branch deletion after merge to develop
  - Automatic release branch deletion after merge to main
  - Smart triggers for pack/publish (only on release/* → main merges)
  - Build/test coverage for main, develop, and release/* branches
  - Comprehensive workflow documentation

� 📋 **Ready for Use:**
- Complete GitFlow workflow with automation
- Release process with automatic NuGet publishing
- Branch cleanup and repository maintenance
- Full CI/CD pipeline with proper triggers

� **Next Steps:**
1. Create first release branch when ready for v0.1.0
2. Test complete workflow with actual release
3. Verify automatic NuGet publishing functionality
4. Validate branch cleanup automation
