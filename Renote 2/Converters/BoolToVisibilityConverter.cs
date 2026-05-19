using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Renote_2.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool visible = value is bool b && b;
            if (parameter is string invert && invert == "Invert")
                visible = !visible;

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            return false;
        }
    }
}
