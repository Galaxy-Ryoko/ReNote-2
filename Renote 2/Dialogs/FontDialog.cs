using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;

namespace Renote_2.Dialogs
{
    /// <summary>
    /// Font settings data model
    /// </summary>
    public class FontSettings
    {
        public string FontFamily { get; set; } = "Consolas";
        public double FontSize { get; set; } = 12;
    }

    public sealed class FontDialog : ContentDialog
    {
        private static readonly double[] FontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        private ComboBox FontComboBox;
        private ComboBox FontSizeComboBox;
        private TextBlock PreviewTextBlock;

        public string SelectedFontFamily { get; private set; } = "Consolas";
        public double SelectedFontSize { get; private set; } = 12;

        public FontDialog()
        {
            Title = "Font Settings";
            PrimaryButtonText = "Apply";
            SecondaryButtonText = "Cancel";
            DefaultButton = ContentDialogButton.Primary;

            // Create the UI
            var grid = new Grid
            {
                Padding = new Thickness(0, 16, 0, 0)
            };

            // Set up row definitions
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Set up column definitions
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Font Family label
            var fontLabel = new TextBlock
            {
                Text = "Font Family:",
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(fontLabel, 0);
            Grid.SetColumn(fontLabel, 0);
            grid.Children.Add(fontLabel);

            // Font Family ComboBox
            FontComboBox = new ComboBox
            {
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(12, 0, 0, 0)
            };
            FontComboBox.SelectionChanged += FontComboBox_SelectionChanged;
            Grid.SetRow(FontComboBox, 0);
            Grid.SetColumn(FontComboBox, 1);
            grid.Children.Add(FontComboBox);

            // Font Size label
            var sizeLabel = new TextBlock
            {
                Text = "Font Size:",
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            Grid.SetRow(sizeLabel, 1);
            Grid.SetColumn(sizeLabel, 0);
            grid.Children.Add(sizeLabel);

            // Font Size ComboBox
            FontSizeComboBox = new ComboBox
            {
                Width = 80,
                Margin = new Thickness(12, 12, 0, 0)
            };
            FontSizeComboBox.SelectionChanged += FontSizeComboBox_SelectionChanged;

            var sizePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };
            sizePanel.Children.Add(FontSizeComboBox);
            sizePanel.Children.Add(new TextBlock { Text = "pt", VerticalAlignment = VerticalAlignment.Center });

            Grid.SetRow(sizePanel, 1);
            Grid.SetColumn(sizePanel, 1);
            grid.Children.Add(sizePanel);

            // Preview label
            var previewLabel = new TextBlock
            {
                Text = "Preview:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(previewLabel, 2);
            Grid.SetColumnSpan(previewLabel, 2);
            grid.Children.Add(previewLabel);

            // Preview TextBlock
            PreviewTextBlock = new TextBlock
            {
                Text = "The quick brown fox jumps over the lazy dog ABCDEFG 0123456789",
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.Wrap,
                Height = 80,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(PreviewTextBlock, 3);
            Grid.SetColumnSpan(PreviewTextBlock, 2);
            grid.Children.Add(PreviewTextBlock);

            Content = grid;

            // Initialize font list
            InitializeFonts();
            InitializeFontSizes();
        }

        private void InitializeFonts()
        {
            var fonts = GetSystemFonts();
            foreach (var font in fonts)
            {
                FontComboBox.Items.Add(font);
            }

            var index = FontComboBox.Items.Cast<string>().ToList().IndexOf(SelectedFontFamily);
            if (index >= 0)
                FontComboBox.SelectedIndex = index;
            else if (FontComboBox.Items.Count > 0)
                FontComboBox.SelectedIndex = 0;
        }

        private void InitializeFontSizes()
        {
            foreach (var size in FontSizes)
            {
                FontSizeComboBox.Items.Add(size.ToString());
            }

            var sizeIndex = FontSizes.ToList().IndexOf(SelectedFontSize);
            if (sizeIndex >= 0)
                FontSizeComboBox.SelectedIndex = sizeIndex;
            else
                FontSizeComboBox.SelectedIndex = 4; // Default to 12
        }

        private static List<string> GetSystemFonts()
        {
            var fonts = new List<string>
            {
                "Consolas",
                "Courier New",
                "Courier",
                "Lucida Console",
                "Monaco",
                "Monospace",
                "Segoe UI Mono",
                "Source Code Pro",
                "Fira Code",
                "JetBrains Mono",
                "DejaVu Sans Mono",
                "Menlo",
                "Ubuntu Mono",
                "Liberation Mono",
                "Inconsolata",
                "Cascadia Code",
                "Cascadia Mono",
                "IBM Plex Mono",
                "Comic Sans MS",
                "Arial",
                "Verdana",
                "Georgia",
                "Times New Roman",
                "Trebuchet MS",
                "Segoe UI",
            };

            fonts.Sort();
            return fonts.Distinct().ToList();
        }

        public void SetCurrentFont(string fontFamily, double fontSize)
        {
            SelectedFontFamily = fontFamily;
            SelectedFontSize = fontSize;

            if (FontComboBox.Items.Count > 0)
            {
                var index = FontComboBox.Items.Cast<string>().ToList().IndexOf(fontFamily);
                if (index >= 0)
                    FontComboBox.SelectedIndex = index;

                var sizeIndex = FontSizes.ToList().IndexOf(fontSize);
                if (sizeIndex >= 0)
                    FontSizeComboBox.SelectedIndex = sizeIndex;
            }

            UpdatePreview();
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontComboBox.SelectedItem is string font)
            {
                SelectedFontFamily = font;
                UpdatePreview();
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is string sizeStr && double.TryParse(sizeStr, out var size))
            {
                SelectedFontSize = size;
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            PreviewTextBlock.FontFamily = new FontFamily(SelectedFontFamily);
            PreviewTextBlock.FontSize = SelectedFontSize;
        }
    }
}
