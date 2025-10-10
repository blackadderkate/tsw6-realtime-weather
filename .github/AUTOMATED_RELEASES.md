# Automated Release System

This project uses GitHub Actions to automatically build and release the application.

## How It Works

### 1. **Development Builds** (Automatic on every push to `main`)
- **Trigger**: Every push to the `main` branch
- **Release Tag**: `dev-latest` (updated automatically)
- **Type**: Pre-release
- **Purpose**: Bleeding-edge builds for testing and development

When you push to `main`, GitHub Actions will:
1. Build the AOT compiled executable
2. Create a ZIP package with the executable, config, and documentation
3. Update the `dev-latest` release with the new build
4. Mark it as a pre-release

### 2. **Stable Releases** (Manual version tags)
- **Trigger**: Pushing a version tag (e.g., `v1.0.0`)
- **Type**: Full release with auto-generated release notes
- **Purpose**: Official stable releases for end users

To create a stable release:
```bash
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions will:
1. Build the AOT compiled executable
2. Create a ZIP package
3. Create a new release with the version tag
4. Auto-generate release notes from commits since the last tag
5. Mark it as the latest release

### 3. **Pull Request Builds**
- **Trigger**: Pull requests to `main`
- **Purpose**: Verify that PRs compile successfully
- No releases are created, but artifacts are available for testing

## Release Contents

Every release includes:
- `tsw6-realtime-weather.exe` - Native AOT compiled executable (~14.5MB)
- `config.yaml` - Configuration template
- `README.md` - Full documentation
- `QUICKSTART.txt` - Quick start guide

## Versioning

Follow semantic versioning:
- `v1.0.0` - Major release (breaking changes)
- `v1.1.0` - Minor release (new features, backward compatible)
- `v1.0.1` - Patch release (bug fixes)

## Manual Workflow Trigger

You can also manually trigger a build:
1. Go to Actions → Build and Release
2. Click "Run workflow"
3. Select the branch
4. Click "Run workflow"

## Development Workflow

### Regular Development
```bash
# Make changes, commit
git add .
git commit -m "feat: add new feature"
git push origin main
# ✅ Automatic dev-latest release created
```

### Creating a Stable Release
```bash
# Update version references if needed
# Make sure everything is committed and pushed

# Create and push a tag
git tag v1.0.0
git push origin v1.0.0
# ✅ Automatic stable release created
```

## Downloading Releases

### For End Users
- Go to the [Releases](https://github.com/GarethLowe/tsw6-realtime-weather/releases) page
- Download the latest stable release (without "pre-release" label)

### For Testers/Developers
- Download the `dev-latest` pre-release for the absolute latest version
- Note: Development builds may be less stable

## Troubleshooting

### Build Fails
- Check the Actions tab for error logs
- Common issues:
  - Compilation errors (fix code and push again)
  - Missing dependencies (check .csproj)
  - .NET version mismatch (currently using .NET 9.0)

### Release Not Created
- For stable releases: Make sure you pushed the tag (`git push origin v1.0.0`)
- For dev builds: Check that you pushed to `main` (not a different branch)
- Verify the Actions workflow completed successfully

### Old dev-latest Release
The `dev-latest` tag is automatically moved to the latest commit on `main`. If you don't see your changes:
- Wait for the Actions workflow to complete (check Actions tab)
- Refresh the releases page
- The commit SHA in the release notes should match your latest commit
