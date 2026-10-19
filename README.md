# ReNote 2

A modern note-taking application for Windows, built with WinUI 3 and .NET 8.

## Download

Get the latest build from the [Releases page](https://github.com/Galaxy-Ryoko/ReNote-2/releases/latest). Choose the ZIP for your processor (x86, x64 or ARM64), extract it, and run `Renote 2.exe`. No installation is required.

## Requirements

- Windows 10 (version 19041 or later) or Windows 11

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
./build.ps1 -Platform x64
```

Packages are written to the `dist/` folder. See [DISTRIBUTION.md](DISTRIBUTION.md) for more details.
