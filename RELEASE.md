# Release Process

This document describes how to create a new release of TSW6 Realtime Weather.

## Automated Release (Recommended)

The project uses GitHub Actions to automatically build and package releases.

### Steps

1. **Update version information** (if applicable)
   - Update any version strings in the code or documentation
   - Commit and push changes to main branch

2. **Create and push a version tag**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. **GitHub Actions will automatically:**
   - Build the project with AOT compilation
   - Create a release package with:
     - Native Windows x64 executable
     - Configuration template (config.yaml)
     - Documentation (README.md)
     - Quick start guide (QUICKSTART.txt)
   - Create a GitHub Release with the ZIP file attached
   - Generate release notes

4. **Review the release**
   - Go to the GitHub Releases page
   - Verify the build succeeded
   - Edit release notes if needed
   - Publish if it was created as draft

## Manual Release

You can also trigger a release manually without creating a tag:

1. Go to the **Actions** tab on GitHub
2. Select **Build and Release** workflow
3. Click **Run workflow**
4. The build will run and create an artifact (but not a release)
5. Download the artifact from the workflow run

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
