# Distribution Guide - ReNote 2

This document explains how to build and distribute ReNote 2 as a standalone application for friends and public release via GitHub.

## Quick Start

### Local Build (Manual)

**Requirements:**
- .NET 8 SDK installed
- PowerShell 5.0+ or PowerShell Core

**Build for x64 (recommended):**
```powershell
./build.ps1 -Platform x64
```

**Build for all platforms (x86, x64, ARM64):**
```powershell
./build.ps1 -Platform all
```

**Available Options:**
```powershell
./build.ps1 -Platform x64                    # Build x64 only
./build.ps1 -Platform x64 -Clean             # Clean and rebuild
./build.ps1 -Platform all -Configuration Release
```

Built packages are placed in the `dist/` directory as ZIP files.

### GitHub Actions Automated Build

**To trigger an automated build and release on GitHub:**

1. **Create a version tag:**
   ```bash
   git tag -a v1.0.0 -m "Release version 1.0.0"
   git push origin v1.0.0
   ```

2. **GitHub Actions will automatically:**
   - Build for all platforms (x86, x64, ARM64)
   - Create distribution packages
   - Upload to GitHub Releases

3. **Verify the release:**
   - Go to your repository's Releases page
   - Check that all platform packages are attached

## Distribution

### Sharing with Friends

1. **Local Distribution:**
   - Run `./build.ps1 -Platform x64`
   - Share the `dist/ReNote2-x64.zip` file
   - Friends extract and run `Renote 2.exe`

2. **Via GitHub Releases:**
   - Create a release tag (see above)
   - GitHub Actions automatically builds and uploads
   - Friends download from the Releases page

### What's Included

Each distribution package contains:
- `Renote 2.exe` - Main application executable
- Windows App SDK runtime (self-contained)
- All required dependencies
- No installation required - just extract and run

### System Requirements

- **Windows 10** (version 19041 or later) or **Windows 11**
- **x64, x86, or ARM64** processor architecture
- **No additional software** required (runtime is bundled)

## Version Management

### Update Version for Release

Edit `Renote 2/Renote 2.csproj` before building:

```xml
<Version>1.0.1</Version>
<FileVersion>1.0.1.0</FileVersion>
<InformationalVersion>1.0.1</InformationalVersion>
```

Then create release tag:
```bash
git tag -a v1.0.1 -m "Release v1.0.1"
git push origin v1.0.1
```

## Build Troubleshooting

**Error: "dotnet: The term 'dotnet' is not recognized"**
- Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
- Restart PowerShell

**Error: "Build failed"**
- Run with `-Clean` flag to reset build cache
- Check that all NuGet dependencies are available

**Large Package Size**
- Distribution packages are ~100-150 MB because they include the bundled Windows App SDK runtime
- This allows users to run the app without additional installation

## GitHub Actions Setup

The `.github/workflows/build-release.yml` workflow:
- Triggers on git tags matching `v*` (e.g., `v1.0.0`, `v1.0.1`)
- Builds all three platform configurations
- Creates ZIP packages for each platform
- Automatically uploads to GitHub Releases
- Can also be triggered manually via "Actions" tab in GitHub

**Workflow Status:**
Check the "Actions" tab on your GitHub repository to view build logs and status.

## File Structure

```
ReNote 2/
├── build.ps1                          # Local build script
├── .github/workflows/
│   └── build-release.yml              # GitHub Actions workflow
├── Renote 2/
│   ├── Renote 2.csproj               # Project file (contains version)
│   ├── App.xaml.cs
│   ├── MainWindow.xaml.cs
│   └── ... (source files)
└── dist/                              # Output directory (created by build)
	├── ReNote2-x64.zip               # Distribution packages
	├── ReNote2-x86.zip
	└── ReNote2-ARM64.zip
```

## Support & Issues

For issues with the build or distribution:
1. Check build logs: `./build.ps1 -Platform x64` output
2. Verify .NET 8 SDK is installed: `dotnet --version`
3. Check GitHub Actions logs: Repository → Actions → Latest workflow run

## License & Attribution

Personal project for friends. Refer to LICENSE file (if applicable) for distribution rights.
