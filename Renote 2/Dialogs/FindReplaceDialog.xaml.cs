namespace Renote_2.Dialogs
{
    /// <summary>
    /// Find and Replace dialog data model
    /// </summary>
    public class FindReplaceData
    {
        public string SearchText { get; set; } = string.Empty;
        public string ReplaceText { get; set; } = string.Empty;
        public bool IsMatchCase { get; set; }
        public bool IsWholeWord { get; set; }

        /// <summary>
        /// Dialog result hint: "Single" or "ReplaceAll"
        /// </summary>
        public string? Tag { get; set; }

        public bool IsReplaceAll => Tag == "ReplaceAll";
    }
}
