using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Renote_2.Helpers
{
    /// <summary>
    /// Helper class for displaying common dialogs
    /// </summary>
    public class DialogHelper
    {
        /// <summary>
        /// Shows a confirmation dialog and returns user's choice
        /// </summary>
        public static async Task<bool> ShowConfirmationDialogAsync(
            XamlRoot xamlRoot,
            string title,
            string message,
            string primaryButtonText = "Yes",
            string secondaryButtonText = "No")
        {
            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        /// <summary>
        /// Shows a save confirmation dialog for unsaved changes
        /// </summary>
        public static async Task<SaveChangesResult> ShowSaveChangesDialogAsync(
            XamlRoot xamlRoot,
            string fileName)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = "Unsaved Changes",
                Content = $"Do you want to save changes to '{fileName}'?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            return result switch
            {
                ContentDialogResult.Primary => SaveChangesResult.Save,
                ContentDialogResult.Secondary => SaveChangesResult.DontSave,
                _ => SaveChangesResult.Cancel
            };
        }

        /// <summary>
        /// Shows an error dialog
        /// </summary>
        public static async Task ShowErrorDialogAsync(
            XamlRoot xamlRoot,
            string title,
            string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows an info dialog
        /// </summary>
        public static async Task ShowInfoDialogAsync(
            XamlRoot xamlRoot,
            string title,
            string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a text input dialog
        /// </summary>
        public static async Task<string?> ShowTextInputDialogAsync(
            XamlRoot xamlRoot,
            string title,
            string message,
            string defaultText = "")
        {
            var textBox = new TextBox
            {
                Text = defaultText,
                Height = 40,
                PlaceholderText = "Enter text..."
            };

            var dialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = title,
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = message, Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10) },
                        textBox
                    }
                },
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary ? textBox.Text : null;
        }
    }

    /// <summary>
    /// Enum for save changes dialog result
    /// </summary>
    public enum SaveChangesResult
    {
        Save,
        DontSave,
        Cancel
    }
}
