using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace UM.UI.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// true → Visible, false → Collapsed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
                return b ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
