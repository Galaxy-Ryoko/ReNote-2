using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Renote_2.Dialogs;
using Renote_2.Helpers;
using Renote_2.Services;
using Renote_2.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;

namespace Renote_2
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();
        private readonly FileService _fileService = new();
        private readonly SettingsService _settingsService = new();
        private readonly Dictionary<EditorViewModel, TextBox> _editorBoxes = new();
        private bool _isFullScreen;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Renote 2 - Text Editor";
            RootGrid.DataContext = _viewModel;

            var (width, height, x, y) = _settingsService.LoadWindowDimensions();
            if (AppWindow != null)
            {
                try
                {
                    AppWindow.MoveAndResize(new Windows.Graphics.RectInt32((int)x, (int)y, (int)width, (int)height));
                }
                catch { }
            }

            var (fontFamily, fontSize) = _settingsService.LoadFontSettings();
            _viewModel.FontFamily = fontFamily;
            _viewModel.IsWordWrapEnabled = _settingsService.LoadWordWrapEnabled();
            _viewModel.ZoomLevel = _settingsService.LoadZoomLevel();
            _viewModel.IsStatusBarVisible = _settingsService.LoadStatusBarVisibility();
            _viewModel.EditorFontSize = fontSize * (_viewModel.ZoomLevel / 100.0);

            PopulateRecentFilesMenu();
        }

        private XamlRoot DialogRoot => Content.XamlRoot;

        #region Editor access

        private TextBox? GetActiveEditor()
        {
            if (_viewModel.ActiveDocument != null &&
                _editorBoxes.TryGetValue(_viewModel.ActiveDocument, out var editor))
            {
                return editor;
            }

            return null;
        }

        private static EditorViewModel? GetDocumentFromTab(object? tab)
        {
            if (tab is TabViewItem tabViewItem)
                return tabViewItem.DataContext as EditorViewModel;
            return tab as EditorViewModel;
        }

        private void EditorTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is EditorViewModel document)
                _editorBoxes[document] = textBox;
        }

        private void EditorTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is EditorViewModel document)
                _editorBoxes.Remove(document);
        }

        private void EditorTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is EditorViewModel document)
                document.UpdateStatistics(textBox.SelectionStart);
        }

        private void EditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is EditorViewModel document)
                document.UpdateStatistics(textBox.SelectionStart);
        }

        private void ApplyEditorFocusAction(Action<TextBox> action)
        {
            var editor = GetActiveEditor();
            if (editor == null)
            {
                _viewModel.StatusBarMessage = "No active editor";
                return;
            }

            editor.Focus(FocusState.Programmatic);
            action(editor);
        }

        #endregion

        #region Keyboard shortcuts

        private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var ctrl = IsKeyDown(VirtualKey.Control);

            switch (e.Key)
            {
                case VirtualKey.N when ctrl && !IsKeyDown(VirtualKey.Shift):
                    OnNewClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.O when ctrl:
                    OnOpenClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.S when ctrl && !IsKeyDown(VirtualKey.Shift):
                    OnSaveClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.S when ctrl && IsKeyDown(VirtualKey.Shift):
                    OnSaveAsClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.T when ctrl:
                    OnNewTabClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.W when ctrl:
                    OnCloseTabClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.Tab when ctrl && !IsKeyDown(VirtualKey.Shift):
                    OnNextTabClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.Tab when ctrl && IsKeyDown(VirtualKey.Shift):
                    OnPrevTabClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.F when ctrl:
                    OnFindClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.H when ctrl:
                    OnReplaceClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.F3:
                    OnFindNextClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.F5:
                    OnInsertTimeClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.F11:
                    OnToggleFullScreenClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.Add when ctrl:
                case (VirtualKey)187 when ctrl:
                    OnZoomInClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.Subtract when ctrl:
                case (VirtualKey)189 when ctrl:
                    OnZoomOutClick(sender, e);
                    e.Handled = true;
                    break;
                case VirtualKey.Number0 when ctrl:
                case VirtualKey.NumberPad0 when ctrl:
                    OnResetZoomClick(sender, e);
                    e.Handled = true;
                    break;
            }
        }

        private static bool IsKeyDown(VirtualKey key)
        {
            var state = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(key);
            return (state & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;
        }

        #endregion

        #region File menu

        private void OnNewClick(object sender, RoutedEventArgs e) =>
            _viewModel.CreateNewDocumentCommand.Execute(null);

        private void OnNewWindowClick(object sender, RoutedEventArgs e)
        {
            var newWindow = new MainWindow();
            newWindow.Activate();
        }

        private async void OnOpenClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var (file, encoding) = await _fileService.OpenFileAsync(this);
                if (file == null)
                    return;

                var content = await _fileService.ReadFileAsync(file, encoding);
                var doc = new EditorViewModel(file.Path, content, encoding);
                _viewModel.Documents.Add(doc);
                _viewModel.ActiveDocument = doc;
                _viewModel.SelectedTabIndex = _viewModel.Documents.Count - 1;

                _settingsService.AddRecentFile(file.Path);
                PopulateRecentFilesMenu();
                _viewModel.StatusBarMessage = $"Opened: {file.Name}";
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorDialogAsync(DialogRoot, "Error Opening File", ex.Message);
            }
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e) =>
            await SaveCurrentDocumentAsync();

        private async Task SaveCurrentDocumentAsync()
        {
            if (_viewModel.ActiveDocument == null)
                return;

            if (string.IsNullOrEmpty(_viewModel.ActiveDocument.FilePath))
            {
                await SaveAsCurrentDocumentAsync();
                return;
            }

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(_viewModel.ActiveDocument.FilePath);
                await _fileService.WriteFileAsync(file, _viewModel.ActiveDocument.Content, _viewModel.ActiveDocument.FileEncoding);
                _viewModel.ActiveDocument.MarkAsSaved();
                _viewModel.StatusBarMessage = $"Saved: {_viewModel.ActiveDocument.Title}";
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorDialogAsync(DialogRoot, "Error Saving File", ex.Message);
            }
        }

        private async void OnSaveAsClick(object sender, RoutedEventArgs e) =>
            await SaveAsCurrentDocumentAsync();

        private async Task SaveAsCurrentDocumentAsync()
        {
            if (_viewModel.ActiveDocument == null)
                return;

            try
            {
                var file = await _fileService.SaveAsFileAsync(this);
                if (file == null)
                    return;

                await _fileService.WriteFileAsync(file, _viewModel.ActiveDocument.Content, _viewModel.ActiveDocument.FileEncoding);
                _viewModel.ActiveDocument.FilePath = file.Path;
                _viewModel.ActiveDocument.Title = file.Name;
                _viewModel.ActiveDocument.MarkAsSaved();

                _settingsService.AddRecentFile(file.Path);
                PopulateRecentFilesMenu();
                _viewModel.StatusBarMessage = $"Saved as: {file.Name}";
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorDialogAsync(DialogRoot, "Error Saving File", ex.Message);
            }
        }

        private async void OnSaveAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var doc in _viewModel.Documents.Where(d => d.IsModified).ToList())
            {
                _viewModel.ActiveDocument = doc;
                await SaveCurrentDocumentAsync();
            }
        }

        private async void OnPrintClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ActiveDocument == null)
                return;

            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), $"renote2_{Guid.NewGuid():N}.txt");
                await File.WriteAllTextAsync(tempPath, _viewModel.ActiveDocument.Content);
                var file = await StorageFile.GetFileFromPathAsync(tempPath);
                await Launcher.LaunchFileAsync(file);
                _viewModel.StatusBarMessage = "Opened file in default app for printing";
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorDialogAsync(DialogRoot, "Print", ex.Message);
            }
        }

        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            if (!await TryCloseAllDocumentsAsync())
                return;

            SaveWindowSettings();
            Close();
        }

        #endregion

        #region Edit menu

        private void OnUndoClick(object sender, RoutedEventArgs e)
        {
            ApplyEditorFocusAction(_ =>
            {
                KeyboardShortcutHelper.SendCtrlChord(VirtualKey.Z);
                _viewModel.StatusBarMessage = "Undo";
            });
        }

        private void OnRedoClick(object sender, RoutedEventArgs e)
        {
            ApplyEditorFocusAction(_ =>
            {
                KeyboardShortcutHelper.SendCtrlChord(VirtualKey.Y);
                _viewModel.StatusBarMessage = "Redo";
            });
        }

        private void OnCutClick(object sender, RoutedEventArgs e)
        {
            ApplyEditorFocusAction(editor =>
            {
                if (editor.SelectionLength > 0)
                {
                    var data = new DataPackage();
                    data.SetText(editor.SelectedText);
                    Clipboard.SetContent(data);
                    editor.SelectedText = string.Empty;
                }
                _viewModel.StatusBarMessage = "Cut";
            });
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            ApplyEditorFocusAction(editor =>
            {
                if (editor.SelectionLength > 0)
                {
                    var data = new DataPackage();
                    data.SetText(editor.SelectedText);
                    Clipboard.SetContent(data);
                }
                _viewModel.StatusBarMessage = "Copy";
            });
        }

        private async void OnPasteClick(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveEditor();
            if (editor == null)
                return;

            editor.Focus(FocusState.Programmatic);
            var package = Clipboard.GetContent();
            if (package?.Contains(StandardDataFormats.Text) != true)
                return;

            var text = await package.GetTextAsync();
            if (string.IsNullOrEmpty(text))
                return;

            editor.SelectedText = text;
            _viewModel.StatusBarMessage = "Paste";
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            ApplyEditorFocusAction(editor =>
            {
                if (editor.SelectionLength > 0)
                    editor.SelectedText = string.Empty;
                else if (editor.SelectionStart < editor.Text.Length)
                    editor.Text = editor.Text.Remove(editor.SelectionStart, 1);

                _viewModel.StatusBarMessage = "Delete";
            });
        }

        private async void OnFindClick(object sender, RoutedEventArgs e) =>
            await ShowFindReplaceDialogAsync(FindReplaceDialogMode.Find);

        private void OnFindNextClick(object sender, RoutedEventArgs e) =>
            PerformFindNext(wrapAround: true);

        private async void OnReplaceClick(object sender, RoutedEventArgs e) =>
            await ShowFindReplaceDialogAsync(FindReplaceDialogMode.Replace);

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            ApplyEditorFocusAction(editor =>
            {
                editor.SelectAll();
                _viewModel.StatusBarMessage = "Select all";
            });
        }

        private void OnInsertTimeClick(object sender, RoutedEventArgs e)
        {
            var stamp = DateTime.Now.ToString("g");
            ApplyEditorFocusAction(editor =>
            {
                int start = editor.SelectionStart;
                editor.Text = editor.Text.Insert(start, stamp);
                editor.SelectionStart = start + stamp.Length;
                _viewModel.StatusBarMessage = $"Inserted: {stamp}";
            });
        }

        #endregion

        #region Find / replace

        private async Task ShowFindReplaceDialogAsync(FindReplaceDialogMode mode)
        {
            var initial = new FindReplaceData
            {
                SearchText = _viewModel.SearchText ?? string.Empty,
                ReplaceText = _viewModel.ReplaceText ?? string.Empty,
                IsMatchCase = _viewModel.IsSearchCaseSensitive,
                IsWholeWord = _viewModel.IsSearchWholeWord
            };

            var result = await FindReplaceDialogHelper.ShowAsync(DialogRoot, initial, mode);
            if (result == null)
                return;

            _viewModel.SearchText = result.SearchText;
            _viewModel.ReplaceText = result.ReplaceText;
            _viewModel.IsSearchCaseSensitive = result.IsMatchCase;
            _viewModel.IsSearchWholeWord = result.IsWholeWord;

            if (mode == FindReplaceDialogMode.Find)
            {
                PerformFindNext(wrapAround: true);
                return;
            }

            if (result.IsReplaceAll)
                PerformReplaceAll();
            else
                PerformReplaceOne();
        }

        private void PerformFindNext(bool wrapAround)
        {
            if (_viewModel.ActiveDocument == null || string.IsNullOrEmpty(_viewModel.SearchText))
            {
                _viewModel.StatusBarMessage = "Enter search text";
                return;
            }

            var editor = GetActiveEditor();
            if (editor == null)
                return;

            string content = _viewModel.ActiveDocument.Content;
            int start = editor.SelectionStart + Math.Max(editor.SelectionLength, 1);

            int found = FindReplaceHelper.FindNext(
                content,
                _viewModel.SearchText,
                start,
                _viewModel.IsSearchCaseSensitive,
                _viewModel.IsSearchWholeWord);

            if (found < 0 && wrapAround)
            {
                found = FindReplaceHelper.FindNext(
                    content,
                    _viewModel.SearchText,
                    0,
                    _viewModel.IsSearchCaseSensitive,
                    _viewModel.IsSearchWholeWord);
            }

            if (found < 0)
            {
                _viewModel.StatusBarMessage = "No matches found";
                return;
            }

            editor.Select(found, _viewModel.SearchText.Length);
            _viewModel.StatusBarMessage = "Match found";
        }

        private void PerformReplaceOne()
        {
            if (_viewModel.ActiveDocument == null || string.IsNullOrEmpty(_viewModel.SearchText))
                return;

            var editor = GetActiveEditor();
            if (editor == null)
                return;

            int found = FindReplaceHelper.FindNext(
                _viewModel.ActiveDocument.Content,
                _viewModel.SearchText,
                editor.SelectionStart,
                _viewModel.IsSearchCaseSensitive,
                _viewModel.IsSearchWholeWord);

            if (found < 0)
            {
                _viewModel.StatusBarMessage = "No matches found";
                return;
            }

            string replaceText = _viewModel.ReplaceText ?? string.Empty;
            editor.Select(found, _viewModel.SearchText.Length);
            editor.SelectedText = replaceText;
            _viewModel.StatusBarMessage = "Replaced";
        }

        private void PerformReplaceAll()
        {
            if (_viewModel.ActiveDocument == null || string.IsNullOrEmpty(_viewModel.SearchText))
                return;

            var doc = _viewModel.ActiveDocument;
            int count = 0;
            int index = 0;
            string replaceText = _viewModel.ReplaceText ?? string.Empty;

            while (true)
            {
                int found = FindReplaceHelper.FindNext(
                    doc.Content,
                    _viewModel.SearchText,
                    index,
                    _viewModel.IsSearchCaseSensitive,
                    _viewModel.IsSearchWholeWord);

                if (found < 0)
                    break;

                doc.Content = FindReplaceHelper.ApplyReplace(doc.Content, found, _viewModel.SearchText.Length, replaceText);
                index = found + replaceText.Length;
                count++;
            }

            _viewModel.StatusBarMessage = count > 0 ? $"Replaced {count} occurrence(s)" : "No matches found";
        }

        #endregion

        #region View menu

        private void OnToggleStatusBarClick(object sender, RoutedEventArgs e)
        {
            _viewModel.IsStatusBarVisible = !_viewModel.IsStatusBarVisible;
            _settingsService.SaveStatusBarVisibility(_viewModel.IsStatusBarVisible);
            _viewModel.StatusBarMessage = _viewModel.IsStatusBarVisible ? "Status bar shown" : "Status bar hidden";
        }

        private void OnZoomInClick(object sender, RoutedEventArgs e) =>
            _viewModel.ZoomInCommand.Execute(null);

        private void OnZoomOutClick(object sender, RoutedEventArgs e) =>
            _viewModel.ZoomOutCommand.Execute(null);

        private void OnResetZoomClick(object sender, RoutedEventArgs e) =>
            _viewModel.ResetZoomCommand.Execute(null);

        private void OnToggleWordWrapClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleWordWrapCommand.Execute(null);
            _settingsService.SaveWordWrapEnabled(_viewModel.IsWordWrapEnabled);
        }

        private void OnToggleFullScreenClick(object sender, RoutedEventArgs e)
        {
            if (AppWindow == null)
                return;

            if (!_isFullScreen)
            {
                AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                _isFullScreen = true;
                _viewModel.StatusBarMessage = "Full screen";
            }
            else
            {
                AppWindow.SetPresenter(AppWindowPresenterKind.Default);
                _isFullScreen = false;
                _viewModel.StatusBarMessage = "Exited full screen";
            }
        }

        #endregion

        #region Window / tabs

        private void OnNewTabClick(object sender, RoutedEventArgs e) =>
            _viewModel.CreateNewDocumentCommand.Execute(null);

        private async void OnCloseTabClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ActiveDocument != null)
                await TryCloseDocumentAsync(_viewModel.ActiveDocument);
        }

        private async void OnCloseAllTabsClick(object sender, RoutedEventArgs e) =>
            await TryCloseAllDocumentsAsync();

        private void OnNextTabClick(object sender, RoutedEventArgs e) =>
            _viewModel.NextTabCommand.Execute(null);

        private void OnPrevTabClick(object sender, RoutedEventArgs e) =>
            _viewModel.PreviousTabCommand.Execute(null);

        private void OnAddTabButtonClick(TabView sender, object args) =>
            _viewModel.CreateNewDocumentCommand.Execute(null);

        private async void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            var document = GetDocumentFromTab(args.Tab);
            if (document == null)
                return;

            if (!await TryCloseDocumentAsync(document) && !_viewModel.Documents.Contains(document))
            {
                int index = Math.Min(_viewModel.SelectedTabIndex, _viewModel.Documents.Count);
                _viewModel.Documents.Insert(index, document);
                _viewModel.ActiveDocument = document;
                _viewModel.SelectedTabIndex = index;
            }
        }

        #endregion

        #region Help / recent files

        private async void OnAboutClick(object sender, RoutedEventArgs e)
        {
            const string aboutMessage = "Renote 2\n\nA lightweight, modern text editor for Windows 10 and Windows 11\n\nVersion 1.0\nBuilt with WinUI 3";
            await DialogHelper.ShowInfoDialogAsync(DialogRoot, "About Renote 2", aboutMessage);
        }

        private void PopulateRecentFilesMenu()
        {
            RecentFilesMenu.Items.Clear();

            var recentFiles = _settingsService.LoadRecentFiles();
            if (recentFiles.Count == 0)
            {
                var emptyItem = new MenuFlyoutItem { Text = "(No recent files)", IsEnabled = false };
                RecentFilesMenu.Items.Add(emptyItem);
                return;
            }

            foreach (var filePath in recentFiles)
            {
                var item = new MenuFlyoutItem { Text = Path.GetFileName(filePath), Tag = filePath };
                item.Click += RecentFileItem_Click;
                RecentFilesMenu.Items.Add(item);
            }
        }

        private async void RecentFileItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuFlyoutItem item || item.Tag is not string filePath)
                return;

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);
                var encoding = await _fileService.DetectEncodingAsync(file);
                var content = await _fileService.ReadFileAsync(file, encoding);

                var doc = new EditorViewModel(file.Path, content, encoding);
                _viewModel.Documents.Add(doc);
                _viewModel.ActiveDocument = doc;
                _viewModel.SelectedTabIndex = _viewModel.Documents.Count - 1;
                _viewModel.StatusBarMessage = $"Opened: {file.Name}";
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorDialogAsync(DialogRoot, "Error Opening Recent File", ex.Message);
            }
        }

        #endregion

        #region Document close / settings

        private async Task<bool> TryCloseDocumentAsync(EditorViewModel document)
        {
            if (document.IsModified)
            {
                var result = await DialogHelper.ShowSaveChangesDialogAsync(DialogRoot, document.Title);
                if (result == SaveChangesResult.Cancel)
                    return false;

                if (result == SaveChangesResult.Save)
                {
                    _viewModel.ActiveDocument = document;
                    await SaveCurrentDocumentAsync();
                    if (document.IsModified)
                        return false;
                }
            }

            _editorBoxes.Remove(document);
            _viewModel.RemoveDocument(document);
            return true;
        }

        private async Task<bool> TryCloseAllDocumentsAsync()
        {
            foreach (var doc in _viewModel.Documents.ToList())
            {
                if (!await TryCloseDocumentAsync(doc))
                    return false;
            }

            return true;
        }

        private void SaveWindowSettings()
        {
            if (AppWindow != null)
            {
                var size = AppWindow.Size;
                var pos = AppWindow.Position;
                _settingsService.SaveWindowDimensions(size.Width, size.Height, pos.X, pos.Y);
            }

            _settingsService.SaveWordWrapEnabled(_viewModel.IsWordWrapEnabled);
            _settingsService.SaveZoomLevel(_viewModel.ZoomLevel);
            _settingsService.SaveStatusBarVisibility(_viewModel.IsStatusBarVisible);
            _settingsService.SaveFontSettings(_viewModel.FontFamily, _viewModel.EditorFontSize);
        }

        #endregion
    }
}
