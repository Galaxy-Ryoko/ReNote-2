using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Renote_2.Dialogs;
using System;
using System.Threading.Tasks;

namespace Renote_2.Helpers
{
    public enum FindReplaceDialogMode
    {
        Find,
        Replace
    }

    /// <summary>
    /// Find and replace dialog using ContentDialog.
    /// </summary>
    public static class FindReplaceDialogHelper
    {
        public static async Task<FindReplaceData?> ShowAsync(
            XamlRoot xamlRoot,
            FindReplaceData initial,
            FindReplaceDialogMode mode)
        {
            var findBox = new TextBox
            {
                Text = initial.SearchText,
                PlaceholderText = "Find what",
                Margin = new Thickness(0, 0, 0, 8)
            };

            var replaceBox = new TextBox
            {
                Text = initial.ReplaceText,
                PlaceholderText = "Replace with",
                Margin = new Thickness(0, 0, 0, 8),
                Visibility = mode == FindReplaceDialogMode.Replace ? Visibility.Visible : Visibility.Collapsed
            };

            var matchCase = new CheckBox { Content = "Match case", IsChecked = initial.IsMatchCase };
            var wholeWord = new CheckBox { Content = "Match whole word only", IsChecked = initial.IsWholeWord };

            var panel = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock { Text = "Find what:" },
                    findBox
                }
            };

            if (mode == FindReplaceDialogMode.Replace)
            {
                panel.Children.Add(new TextBlock { Text = "Replace with:" });
                panel.Children.Add(replaceBox);
            }

            panel.Children.Add(matchCase);
            panel.Children.Add(wholeWord);

            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = mode == FindReplaceDialogMode.Find ? "Find" : "Replace",
                Content = panel,
                PrimaryButtonText = mode == FindReplaceDialogMode.Find ? "Find Next" : "Replace",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            if (mode == FindReplaceDialogMode.Replace)
                dialog.SecondaryButtonText = "Replace All";

            var result = await dialog.ShowAsync().AsTask();

            if (result == ContentDialogResult.None)
                return null;

            var data = new FindReplaceData
            {
                SearchText = findBox.Text,
                ReplaceText = replaceBox.Text,
                IsMatchCase = matchCase.IsChecked == true,
                IsWholeWord = wholeWord.IsChecked == true
            };

            data.Tag = result == ContentDialogResult.Secondary ? "ReplaceAll" : "Single";
            return data;
        }
    }
}
