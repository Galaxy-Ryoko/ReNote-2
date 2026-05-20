using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace Renote_2.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<EditorViewModel> documents = new();

        [ObservableProperty]
        private EditorViewModel? activeDocument;

        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private bool isWordWrapEnabled = true;

        [ObservableProperty]
        private double zoomLevel = 100;

        [ObservableProperty]
        private double editorFontSize = 12.0;

        [ObservableProperty]
        private double baseFontSize = 12.0;

        [ObservableProperty]
        private string fontFamily = "Consolas";

        [ObservableProperty]
        private bool isStatusBarVisible = true;

        [ObservableProperty]
        private string? searchText;

        [ObservableProperty]
        private string? replaceText;

        [ObservableProperty]
        private bool isSearchCaseSensitive;

        [ObservableProperty]
        private bool isSearchWholeWord;

        [ObservableProperty]
        private string statusBarMessage = "Ready";

        public MainViewModel()
        {
            CreateNewDocument();
        }

        partial void OnZoomLevelChanged(double value) => EditorFontSize = BaseFontSize * (value / 100.0);

        [RelayCommand]
        public void CreateNewDocument()
        {
            var newDoc = new EditorViewModel();
            Documents.Add(newDoc);
            ActiveDocument = newDoc;
            SelectedTabIndex = Documents.Count - 1;
            StatusBarMessage = "New document created";
        }

        public void RemoveDocument(EditorViewModel document)
        {
            Documents.Remove(document);

            if (Documents.Count == 0)
            {
                CreateNewDocument();
            }
            else if (ActiveDocument == document)
            {
                int index = Math.Min(SelectedTabIndex, Documents.Count - 1);
                if (index < 0)
                    index = 0;
                ActiveDocument = Documents[index];
                SelectedTabIndex = index;
            }

            StatusBarMessage = "Document closed";
        }

        public void OnTabSelectionChanged(int newIndex)
        {
            if (newIndex >= 0 && newIndex < Documents.Count)
            {
                ActiveDocument = Documents[newIndex];
                SelectedTabIndex = newIndex;
            }
        }

        [RelayCommand]
        public void ToggleWordWrap()
        {
            IsWordWrapEnabled = !IsWordWrapEnabled;
            StatusBarMessage = $"Word wrap {(IsWordWrapEnabled ? "enabled" : "disabled")}";
        }

        [RelayCommand]
        public void ZoomIn()
        {
            if (ZoomLevel < 400)
            {
                ZoomLevel += 10;
                StatusBarMessage = $"Zoom: {ZoomLevel}%";
            }
        }

        [RelayCommand]
        public void ZoomOut()
        {
            if (ZoomLevel > 50)
            {
                ZoomLevel -= 10;
                StatusBarMessage = $"Zoom: {ZoomLevel}%";
            }
        }

        [RelayCommand]
        public void ResetZoom()
        {
            ZoomLevel = 100;
            StatusBarMessage = "Zoom reset to 100%";
        }

        [RelayCommand]
        public void NextTab()
        {
            if (Documents.Count > 1)
            {
                int nextIndex = (SelectedTabIndex + 1) % Documents.Count;
                OnTabSelectionChanged(nextIndex);
            }
        }

        [RelayCommand]
        public void PreviousTab()
        {
            if (Documents.Count > 1)
            {
                int prevIndex = SelectedTabIndex - 1;
                if (prevIndex < 0)
                    prevIndex = Documents.Count - 1;
                OnTabSelectionChanged(prevIndex);
            }
        }

        [RelayCommand]
        public void ClearSearch()
        {
            SearchText = null;
            ReplaceText = null;
        }

        [RelayCommand]
        public void SetFont(string fontFamily)
        {
            FontFamily = fontFamily;
            StatusBarMessage = $"Font changed to {fontFamily}";
        }

        public void ChangeFontSize(double newFontSize)
        {
            BaseFontSize = newFontSize;
            EditorFontSize = newFontSize * (ZoomLevel / 100.0);
            StatusBarMessage = $"Font size changed to {newFontSize}pt";
        }
    }
}
