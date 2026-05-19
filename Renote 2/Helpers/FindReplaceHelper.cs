using System;

namespace Renote_2.Helpers
{
    /// <summary>
    /// Text search and replace helpers for the editor.
    /// </summary>
    public static class FindReplaceHelper
    {
        public static int FindNext(string content, string searchText, int startIndex, bool matchCase, bool wholeWord)
        {
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(content))
                return -1;

            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int index = startIndex;

            while (index < content.Length)
            {
                int found = content.IndexOf(searchText, index, comparison);
                if (found < 0)
                    return -1;

                if (!wholeWord || IsWholeWordMatch(content, found, searchText.Length))
                    return found;

                index = found + 1;
            }

            return -1;
        }

        public static string ApplyReplace(string content, int index, int length, string replaceText)
        {
            return content.Remove(index, length).Insert(index, replaceText);
        }

        private static bool IsWholeWordMatch(string content, int index, int length)
        {
            bool startOk = index == 0 || !char.IsLetterOrDigit(content[index - 1]);
            int endIndex = index + length;
            bool endOk = endIndex >= content.Length || !char.IsLetterOrDigit(content[endIndex]);
            return startOk && endOk;
        }
    }
}
