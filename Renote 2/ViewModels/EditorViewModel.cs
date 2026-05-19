using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Text;

namespace Renote_2.ViewModels
{
    /// <summary>
    /// ViewModel for individual document/tab state and operations
    /// </summary>
    public partial class EditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string content = string.Empty;

        [ObservableProperty]
        private string title = "Untitled";

        [ObservableProperty]
        private string displayTitle = "Untitled";

        [ObservableProperty]
        private string filePath = string.Empty;

        [ObservableProperty]
        private bool isModified;

        [ObservableProperty]
        private Encoding fileEncoding = Encoding.UTF8;

        [ObservableProperty]
        private int lineCount = 1;

        [ObservableProperty]
        private int columnCount = 1;

        [ObservableProperty]
        private int characterCount;

        private bool _isProgrammaticChange;

        public EditorViewModel()
        {
            UpdateDisplayTitle();
            UpdateStatistics(0);
        }

        public EditorViewModel(string filePath, string content, Encoding encoding)
        {
            _isProgrammaticChange = true;
            FilePath = filePath;
            Content = content;
            Title = System.IO.Path.GetFileName(filePath) ?? "Untitled";
            FileEncoding = encoding;
            IsModified = false;
            UpdateDisplayTitle();
            UpdateStatistics(0);
            _isProgrammaticChange = false;
        }

        partial void OnTitleChanged(string value) => UpdateDisplayTitle();

        partial void OnIsModifiedChanged(bool value) => UpdateDisplayTitle();

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Content) && !_isProgrammaticChange)
            {
                MarkAsModified();
            }
        }

        public void UpdateStatistics(int selectionStart)
        {
            CharacterCount = Content.Length;

            if (CharacterCount == 0)
            {
                LineCount = 1;
                ColumnCount = 1;
                return;
            }

            int caret = selectionStart;
            if (caret < 0)
                caret = 0;
            if (caret > CharacterCount)
                caret = CharacterCount;

            int line = 1;
            int column = 1;

            for (int i = 0; i < caret; i++)
            {
                if (Content[i] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }

            LineCount = Content.Split('\n').Length;
            ColumnCount = column;
        }

        public void MarkAsModified()
        {
            if (!IsModified)
                IsModified = true;
        }

        public void MarkAsSaved() => IsModified = false;

        private void UpdateDisplayTitle()
        {
            DisplayTitle = IsModified ? $"{Title}*" : Title;
        }

        public void Clear()
        {
            _isProgrammaticChange = true;
            Content = string.Empty;
            Title = "Untitled";
            FilePath = string.Empty;
            IsModified = false;
            FileEncoding = Encoding.UTF8;
            UpdateDisplayTitle();
            UpdateStatistics(0);
            _isProgrammaticChange = false;
        }
    }
}
