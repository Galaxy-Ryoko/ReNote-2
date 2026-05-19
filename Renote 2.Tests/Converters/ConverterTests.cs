using Microsoft.UI.Xaml;
using Renote_2.Converters;
using Xunit;

namespace Renote_2.Tests.Converters;

public class ConverterTests
{
    [Theory]
    [InlineData(true, TextWrapping.Wrap)]
    [InlineData(false, TextWrapping.NoWrap)]
    public void BoolToTextWrappingConverter_Converts(bool input, TextWrapping expected)
    {
        var converter = new BoolToTextWrappingConverter();
        var result = converter.Convert(input, typeof(TextWrapping), null, string.Empty);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, Visibility.Visible)]
    [InlineData(false, Visibility.Collapsed)]
    public void BoolToVisibilityConverter_Converts(bool input, Visibility expected)
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(input, typeof(Visibility), null, string.Empty);
        Assert.Equal(expected, result);
    }
}
