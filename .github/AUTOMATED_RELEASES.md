# Automated Build System

This project uses GitHub Actions to automatically build the application on every push.

## How It Works

### Automatic Builds
- **Trigger**: Every push to `main` or pull request
- **Output**: ZIP package with executable and documentation
- **Storage**: Workflow artifacts (retained for 90 days)
- **Purpose**: Always have a fresh build ready to release

When you push to `main`, GitHub Actions will:
1. Build the AOT compiled executable
2. Create a ZIP package with the executable, config, and documentation
3. Upload as a workflow artifact
4. Artifact is available for download from the Actions tab

## Creating Releases

Releases are created **manually** from the build artifacts:

1. **Push changes to main**
   ```bash
   git add .
   git commit -m "your changes"
   git push origin main
   ```

2. **Wait for build to complete**
   - Check Actions tab for green checkmark
   - Workflow typically completes in 2-5 minutes

3. **Download the artifact**
   - Click on the workflow run
   - Find "Artifacts" section at the bottom
   - Download the ZIP file

4. **Create a GitHub Release**
   - Go to Releases → Draft a new release
   - Create a tag (e.g., `v1.0.0`)
   - Write release notes
   - Upload the downloaded ZIP file
   - Publish the release

## Artifact Contents

Every build artifact includes:
- `tsw6-realtime-weather.exe` - Native AOT compiled executable (~14.5MB)
- `config.json` - Configuration template
- `README.md` - Full documentation
- `QUICKSTART.txt` - Quick start guide

## Versioning

Follow semantic versioning for releases:
- `v1.0.0` - Major release (breaking changes)
- `v1.1.0` - Minor release (new features, backward compatible)
- `v1.0.1` - Patch release (bug fixes)

## Manual Workflow Trigger

You can manually trigger a build:
1. Go to Actions → Build
2. Click "Run workflow"
3. Select the branch
4. Click "Run workflow"
5. Download artifact when complete

## Troubleshooting

### Build Fails
- Check the Actions tab for error logs
- Common issues:
  - Compilation errors (fix code and push again)
  - Missing dependencies (check .csproj)
  - .NET version mismatch (currently using .NET 9.0)

### Can't Find Artifacts
- Make sure the workflow completed successfully (green checkmark)
- Artifacts appear at the bottom of the workflow run page
- Artifacts are deleted after 90 days

### Build Takes Too Long
- AOT compilation typically takes 2-5 minutes
- Check the workflow logs for any hanging processes
- Cancel and re-run if stuck
