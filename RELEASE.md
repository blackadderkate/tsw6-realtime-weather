# Release Process

This document describes how to create releases of TSW6 Realtime Weather.

## Automated Release System

The project uses GitHub Actions for continuous deployment with two types of releases:

### 1. Development Builds (Automatic)

**Every push to `main` automatically creates a development build.**

- **Tag**: `dev-latest` (automatically updated)
- **Type**: Pre-release
- **Purpose**: Testing and development
- **Contains**: Latest changes from the main branch

No action needed - just push to main:
```bash
git push origin main
```

The workflow will automatically:
- Build the AOT executable
- Package with config and documentation
- Update the `dev-latest` release

### 2. Stable Releases (Tag-based)

**Create stable releases by pushing version tags.**

1. **Prepare for release**
   - Ensure all changes are committed and pushed to main
   - Update version information if needed in documentation
   - Verify the `dev-latest` build is working correctly

2. **Create and push a version tag**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. **GitHub Actions automatically:**
   - Builds with AOT compilation
   - Creates release package with:
     - Native Windows x64 executable (~14.5MB)
     - Configuration template (config.yaml)
     - Documentation (README.md)
     - Quick start guide (QUICKSTART.txt)
   - Creates a GitHub Release with ZIP file
   - Generates release notes from commits
   - Marks as the latest stable release

4. **Review and publish**
   - Release is automatically published
   - Edit release notes if needed
   - Verify the build succeeded

## Manual Workflow Trigger

You can manually trigger a build without pushing:

1. Go to **Actions** → **Build and Release**
2. Click **Run workflow**
3. Select the branch to build
4. Click **Run workflow**
5. Artifacts will be available in the workflow run

## Manual Build (Local)

To build a release package locally:

```powershell
# Clean previous builds
Remove-Item -Recurse -Force ./publish, ./release -ErrorAction SilentlyContinue

# Build AOT release
dotnet publish -c Release -r win-x64 --self-contained -p:PublishAot=true -o ./publish

# Create package directory
New-Item -ItemType Directory -Force -Path ./release

# Copy files
Copy-Item ./publish/tsw6-realtime-weather.exe ./release/
Copy-Item ./config.yaml ./release/
Copy-Item ./README.md ./release/

# Create ZIP
Compress-Archive -Path ./release/* -DestinationPath "./tsw6-realtime-weather-v1.0.0-windows-x64.zip"
```

## Version Numbering

Follow [Semantic Versioning](https://semver.org/):
- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality
- **PATCH** version for backwards-compatible bug fixes

Examples:
- `v1.0.0` - Initial release
- `v1.1.0` - Add new weather features
- `v1.1.1` - Fix weather API bug
- `v2.0.0` - Breaking configuration changes

## Pre-release Versions

For beta or RC releases, use pre-release tags:
```bash
git tag v1.1.0-beta.1
git tag v1.1.0-rc.1
```

GitHub Actions will mark these as pre-releases automatically.

## Troubleshooting

### Build fails on GitHub Actions
- Check the Actions log for specific errors
- Verify the .NET SDK version (9.0.x) is available
- Ensure all dependencies are properly referenced

### Release not created
- Verify you pushed a tag starting with 'v'
- Check that GitHub Actions has write permissions
- Review the workflow file syntax

### AOT compilation errors
- Ensure all JSON serialization uses source generators
- Verify no reflection-based code in hot paths
- Check for trimming warnings in local build
