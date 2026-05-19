using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;

namespace Renote_2.Converters
{
    /// <summary>
    /// Converts a boolean value to TextWrapping enumeration
    /// </summary>
    public class BoolToTextWrappingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
            return TextWrapping.NoWrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is TextWrapping wrapping)
            {
                return wrapping == TextWrapping.Wrap;
            }
            return false;
        }
    }
}
