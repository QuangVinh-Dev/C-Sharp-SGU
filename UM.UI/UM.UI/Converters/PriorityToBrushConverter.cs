using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace UM.UI.Converters
{
    /// <summary>
    /// Converts a priority string ("Cao", "Trung bình", "Thấp") to a colored brush.
    /// </summary>
    public class PriorityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var priority = value as string;
            return priority switch
            {
                "Cao" => new SolidColorBrush(ColorHelper.FromArgb(255, 220, 53, 69)),      // Red
                "Trung bình" => new SolidColorBrush(ColorHelper.FromArgb(255, 255, 165, 0)), // Orange
                "Thấp" => new SolidColorBrush(ColorHelper.FromArgb(255, 40, 167, 69)),       // Green
                _ => new SolidColorBrush(ColorHelper.FromArgb(255, 108, 117, 125)),           // Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
