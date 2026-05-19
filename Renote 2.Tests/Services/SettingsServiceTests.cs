using Renote_2.Services;
using System;
using System.IO;
using Xunit;

namespace Renote_2.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _settingsPath;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _settingsPath = Path.Combine(Path.GetTempPath(), "Renote2Tests", Guid.NewGuid().ToString("N"), "settings.json");
        _service = new SettingsService(_settingsPath);
    }

    [Fact]
    public void Constructor_DoesNotThrow_ForUnpackagedPath()
    {
        Assert.True(File.Exists(_settingsPath) || Directory.Exists(Path.GetDirectoryName(_settingsPath)!));
    }

    [Fact]
    public void SaveAndLoadWindowDimensions_PersistValues()
    {
        _service.SaveWindowDimensions(1200, 800, 50, 60);
        var service2 = new SettingsService(_settingsPath);

        var (width, height, x, y) = service2.LoadWindowDimensions();

        Assert.Equal(1200, width);
        Assert.Equal(800, height);
        Assert.Equal(50, x);
        Assert.Equal(60, y);
    }

    [Fact]
    public void SaveAndLoadWordWrap_PersistValues()
    {
        _service.SaveWordWrapEnabled(false);
        var service2 = new SettingsService(_settingsPath);
        Assert.False(service2.LoadWordWrapEnabled());
    }

    [Fact]
    public void AddRecentFile_KeepsMostRecentFirst_AndLimitsToTen()
    {
        for (int i = 0; i < 12; i++)
            _service.AddRecentFile($@"C:\file{i}.txt");

        var recent = _service.LoadRecentFiles();
        Assert.Equal(10, recent.Count);
        Assert.Equal(@"C:\file11.txt", recent[0]);
        Assert.Equal(@"C:\file2.txt", recent[^1]);
    }

    [Fact]
    public void AddRecentFile_MovesDuplicateToFront()
    {
        _service.AddRecentFile(@"C:\a.txt");
        _service.AddRecentFile(@"C:\b.txt");
        _service.AddRecentFile(@"C:\a.txt");

        var recent = _service.LoadRecentFiles();
        Assert.Equal(@"C:\a.txt", recent[0]);
        Assert.Single(recent, f => f.Equals(@"C:\a.txt", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_settingsPath))
                File.Delete(_settingsPath);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
