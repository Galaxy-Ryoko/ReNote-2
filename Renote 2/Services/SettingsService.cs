using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Renote_2.Services
{
    /// <summary>
    /// Persists application settings to a JSON file (works for unpackaged WinUI apps).
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _settings;

        public SettingsService()
            : this(null)
        {
        }

        /// <param name="settingsFilePath">Optional path for tests; uses %LocalAppData%\Renote2 when null.</param>
        public SettingsService(string? settingsFilePath)
        {
            if (!string.IsNullOrWhiteSpace(settingsFilePath))
            {
                var directory = Path.GetDirectoryName(settingsFilePath)!;
                Directory.CreateDirectory(directory);
                _settingsFilePath = settingsFilePath;
            }
            else
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Renote2");
                Directory.CreateDirectory(folder);
                _settingsFilePath = Path.Combine(folder, "settings.json");
            }

            _settings = LoadFromDisk();
        }

        public void SaveWindowDimensions(double width, double height, double x, double y)
        {
            _settings.WindowWidth = width;
            _settings.WindowHeight = height;
            _settings.WindowX = x;
            _settings.WindowY = y;
            SaveToDisk();
        }

        public (double width, double height, double x, double y) LoadWindowDimensions() =>
            (_settings.WindowWidth, _settings.WindowHeight, _settings.WindowX, _settings.WindowY);

        public void SaveWordWrapEnabled(bool enabled)
        {
            _settings.WordWrapEnabled = enabled;
            SaveToDisk();
        }

        public bool LoadWordWrapEnabled() => _settings.WordWrapEnabled;

        public void SaveFontSettings(string fontFamily, double fontSize)
        {
            _settings.FontFamily = fontFamily;
            _settings.FontSize = fontSize;
            SaveToDisk();
        }

        public (string fontFamily, double fontSize) LoadFontSettings() =>
            (_settings.FontFamily, _settings.FontSize);

        public void SaveTheme(string theme)
        {
            _settings.Theme = theme;
            SaveToDisk();
        }

        public string LoadTheme() => _settings.Theme;

        public void SaveZoomLevel(double zoomLevel)
        {
            _settings.ZoomLevel = zoomLevel;
            SaveToDisk();
        }

        public double LoadZoomLevel() => _settings.ZoomLevel;

        public void SaveStatusBarVisibility(bool visible)
        {
            _settings.ShowStatusBar = visible;
            SaveToDisk();
        }

        public bool LoadStatusBarVisibility() => _settings.ShowStatusBar;

        public void SaveRecentFiles(List<string> filePaths)
        {
            _settings.RecentFiles = filePaths.Take(10).ToList();
            SaveToDisk();
        }

        public List<string> LoadRecentFiles() => new(_settings.RecentFiles);

        public void AddRecentFile(string filePath)
        {
            var recent = LoadRecentFiles();
            recent.RemoveAll(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            recent.Insert(0, filePath);
            SaveRecentFiles(recent);
        }

        public void ClearAllSettings()
        {
            _settings = new AppSettings();
            SaveToDisk();
        }

        private AppSettings LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                    return new AppSettings();

                var json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        private void SaveToDisk()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // Ignore persistence errors so the editor still runs.
            }
        }

        private sealed class AppSettings
        {
            public double WindowWidth { get; set; } = 1000;
            public double WindowHeight { get; set; } = 600;
            public double WindowX { get; set; } = 100;
            public double WindowY { get; set; } = 100;
            public bool WordWrapEnabled { get; set; } = true;
            public string FontFamily { get; set; } = "Consolas";
            public double FontSize { get; set; } = 12;
            public string Theme { get; set; } = "System";
            public double ZoomLevel { get; set; } = 100;
            public bool ShowStatusBar { get; set; } = true;
            public List<string> RecentFiles { get; set; } = new();
        }
    }
}
