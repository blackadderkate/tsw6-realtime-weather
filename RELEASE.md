# Release Process

This document describes how to create releases of TSW6 Realtime Weather.

## Build System

The project uses GitHub Actions to automatically build artifacts on every push to `main`. The built packages are available as workflow artifacts.

### Automatic Builds

**Every push to `main` automatically creates a build artifact.**

- GitHub Actions builds the AOT executable
- Creates a packaged ZIP file
- Uploads as a workflow artifact (retained for 90 days)
- Available in the Actions tab under the workflow run

No action needed - just push to main:
```bash
git push origin main
```

## Creating a Release

Releases are created manually from the build artifacts:

### Steps

1. **Push your changes to main**
   ```bash
   git add .
   git commit -m "feat: your changes"
   git push origin main
   ```

2. **Wait for the build to complete**
   - Go to Actions tab on GitHub
   - Wait for the "Build" workflow to finish
   - Verify it succeeded (green checkmark)

3. **Download the artifact**
   - Click on the completed workflow run
   - Scroll to "Artifacts" section
   - Download the `tsw6-realtime-weather-*-windows-x64.zip` file

4. **Create a GitHub Release**
   - Go to Releases → "Draft a new release"
   - Create a new tag (e.g., `v1.0.0`)
   - Add a release title (e.g., "Version 1.0.0")
   - Describe the changes in the release notes
   - Upload the downloaded ZIP file
   - Check "Set as the latest release"
   - Click "Publish release"

### Version Naming

Follow semantic versioning:
- `v1.0.0` - Major release (breaking changes)
- `v1.1.0` - Minor release (new features, backward compatible)
- `v1.0.1` - Patch release (bug fixes)

## Manual Workflow Trigger

You can manually trigger a build without pushing:

1. Go to **Actions** → **Build**
2. Click **Run workflow**
3. Select the branch to build
4. Click **Run workflow**
5. Download artifacts from the completed run

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
Copy-Item ./config.json ./release/
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
