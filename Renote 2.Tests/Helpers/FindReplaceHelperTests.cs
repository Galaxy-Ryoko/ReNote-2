using Renote_2.Helpers;
using Xunit;

namespace Renote_2.Tests.Helpers;

public class FindReplaceHelperTests
{
    [Fact]
    public void FindNext_ReturnsIndex_WhenTextExists()
    {
        int index = FindReplaceHelper.FindNext("hello world", "world", 0, matchCase: true, wholeWord: true);
        Assert.Equal(6, index);
    }

    [Fact]
    public void FindNext_IsCaseInsensitive_WhenConfigured()
    {
        int index = FindReplaceHelper.FindNext("Hello", "hello", 0, matchCase: false, wholeWord: true);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindNext_ReturnsNegativeOne_WhenNotFound()
    {
        int index = FindReplaceHelper.FindNext("abc", "xyz", 0, matchCase: true, wholeWord: false);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindNext_RespectsWholeWord()
    {
        int index = FindReplaceHelper.FindNext("foobar foo", "foo", 0, matchCase: true, wholeWord: true);
        Assert.Equal(7, index);
    }

    [Fact]
    public void FindNext_ReturnsNegativeOne_ForEmptySearch()
    {
        int index = FindReplaceHelper.FindNext("text", "", 0, matchCase: true, wholeWord: false);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void ApplyReplace_InsertsReplacementText()
    {
        string result = FindReplaceHelper.ApplyReplace("abcdef", 2, 2, "ZZ");
        Assert.Equal("abZZef", result);
    }
}
