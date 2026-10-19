# ReNote 2 Build Script
# Builds self-contained releases for distribution

param(
    [ValidateSet("x86", "x64", "ARM64", "all")]
    [string]$Platform = "x64",
    [string]$Configuration = "Release",
    [switch]$Clean,
    [switch]$NoPublish,
    [switch]$CompressedOnly
)

$projectPath = "Renote 2/Renote 2.csproj"
$outputBase = "dist"
$frameworkVersion = "net8.0-windows10.0.19041.0"

function Write-Header {
    param([string]$Message)
    Write-Host "`n$('=' * 70)" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "$('=' * 70)`n" -ForegroundColor Cyan
}

function Build-Platform {
    param([string]$Plat)
    Write-Host "Building for platform: $Plat" -ForegroundColor Green
    $publishDir = "Renote 2/bin/$Configuration/$frameworkVersion/win-$Plat/publish"

    if (-not $NoPublish) {
        if ($Clean) {
            Write-Host "Cleaning previous build..." -ForegroundColor Yellow
            Remove-Item -Path "Renote 2/bin" -Recurse -Force -ErrorAction SilentlyContinue
        }
        Write-Host "Publishing $Plat build..." -ForegroundColor Yellow
        dotnet publish $projectPath -c $Configuration -p:Platform=$Plat --self-contained -f $frameworkVersion
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed for platform $Plat" -ForegroundColor Red
            return $false
        }
    }

    $packageName = "ReNote2-$Plat"
    $packageDir = Join-Path $outputBase $packageName
    $zipFile = Join-Path $outputBase "$packageName.zip"
    Write-Host "Creating distribution package..." -ForegroundColor Yellow
    Remove-Item -Path $packageDir -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $zipFile -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
    Copy-Item -Path "$publishDir/*" -Destination $packageDir -Recurse -Force

    # Remove unnecessary files to reduce file count
    Write-Host "Optimizing package..." -ForegroundColor Yellow
    Get-ChildItem -Path $packageDir -Recurse -Include "*.dbg", "*.pdb", "*.chk" -Force | Remove-Item -Force -ErrorAction SilentlyContinue
    # Remove .NET host files that are not needed
    Remove-Item -Path "$packageDir/dotnet.exe" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$packageDir/dotnet.dll" -Force -ErrorAction SilentlyContinue

    # Count files after optimization
    $fileCount = (Get-ChildItem -Path $packageDir -Recurse -File | Measure-Object).Count
    $dirCount = (Get-ChildItem -Path $packageDir -Recurse -Directory | Measure-Object).Count

    Compress-Archive -Path $packageDir -DestinationPath $zipFile -Force
    $fileSize = (Get-Item $zipFile).Length / 1MB
    Write-Host "[OK] Package created: $zipFile ($([math]::Round($fileSize, 2)) MB)" -ForegroundColor Green
    Write-Host "     Files: $fileCount | Directories: $dirCount" -ForegroundColor Cyan
    return $true
}

Write-Header "ReNote 2 Build and Distribution Script"
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: .NET CLI not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}
Write-Host "Using .NET: $dotnetVersion" -ForegroundColor Green
New-Item -ItemType Directory -Path $outputBase -Force | Out-Null
$platforms = if ($Platform -eq "all") { @("x86", "x64", "ARM64") } else { @($Platform) }
$allSuccess = $true
foreach ($plat in $platforms) {
    if (-not (Build-Platform $plat)) {
        $allSuccess = $false
    }
}
Write-Header "Build Complete"
if ($allSuccess) {
    Write-Host "[SUCCESS] All builds completed successfully!" -ForegroundColor Green
    Write-Host "`nDistribution packages are ready in '$outputBase/' directory" -ForegroundColor Green
    Write-Host "`nTo create a GitHub release:" -ForegroundColor Cyan
    Write-Host "  1. git tag -a v1.0.0 -m 'Release v1.0.0'" -ForegroundColor Gray
    Write-Host "  2. git push origin v1.0.0" -ForegroundColor Gray
    Write-Host "`nGitHub Actions will automatically build and upload to releases!" -ForegroundColor Cyan
} else {
    Write-Host "[FAILED] Build failed" -ForegroundColor Red
    exit 1
}
