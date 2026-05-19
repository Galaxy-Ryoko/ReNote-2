using Renote_2.ViewModels;
using System.Text;
using Xunit;

namespace Renote_2.Tests.ViewModels;

public class EditorViewModelTests
{
    [Fact]
    public void NewDocument_HasUntitledDisplayTitle()
    {
        var doc = new EditorViewModel();
        Assert.Equal("Untitled", doc.DisplayTitle);
        Assert.False(doc.IsModified);
    }

    [Fact]
    public void ContentChange_SetsModifiedAndUpdatesDisplayTitle()
    {
        var doc = new EditorViewModel();
        doc.Content = "Hello";

        Assert.True(doc.IsModified);
        Assert.Equal("Untitled*", doc.DisplayTitle);
    }

    [Fact]
    public void MarkAsSaved_ClearsModifiedIndicator()
    {
        var doc = new EditorViewModel { Content = "x" };
        doc.MarkAsSaved();

        Assert.False(doc.IsModified);
        Assert.Equal("Untitled", doc.DisplayTitle);
    }

    [Fact]
    public void UpdateStatistics_ReportsCaretLineAndColumn()
    {
        var doc = new EditorViewModel { Content = "ab\ncd" };
        doc.UpdateStatistics(4);

        Assert.Equal(2, doc.LineCount);
        Assert.Equal(2, doc.ColumnCount);
        Assert.Equal(5, doc.CharacterCount);
    }

    [Fact]
    public void LoadedDocument_UsesFileNameAsTitle()
    {
        var doc = new EditorViewModel(@"C:\notes\readme.txt", "content", Encoding.UTF8);

        Assert.Equal("readme.txt", doc.Title);
        Assert.Equal("readme.txt", doc.DisplayTitle);
        Assert.False(doc.IsModified);
    }
}
