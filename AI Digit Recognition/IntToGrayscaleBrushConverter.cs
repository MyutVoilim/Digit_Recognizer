using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace AI_Digit_Recognition
{
    /// <summary>
    /// Converts an integer value to a SolidColorBrush of a grayscale color.
    /// The integer value determines the intensity of gray, ranging from 0 (black) to 255 (white).
    /// </summary>
    public class IntToGrayscaleBrushConverter : IValueConverter
    {

        /// <summary>
        /// Converts an integer value to a SolidColorBrush representing a shade of gray.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <param name="targetType">The type of the binding target property. This parameter is not used.</param>
        /// <param name="parameter">Optional parameter to use for conversion. This parameter is not used.</param>
        /// <param name="culture">The culture to use in the converter. This parameter is not used.</param>
        /// <returns>A SolidColorBrush representing the specified shade of gray, or DependencyProperty.UnsetValue if the conversion is not possible.</returns>

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && intValue >= 0 && intValue <= 255)
            {
                byte byteValue = (byte)intValue;
                return new SolidColorBrush(Color.FromRgb(byteValue, byteValue, byteValue));
            }
            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converts a SolidColorBrush back to an integer value.
        /// This method is not implemented and will throw NotImplementedException if used.
        /// </summary>
        /// <param name="value">The SolidColorBrush to convert back.</param>
        /// <param name="targetType">The type to which to convert the SolidColorBrush. This parameter is not used.</param>
        /// <param name="parameter">Optional parameter to use for conversion. This parameter is not used.</param>
        /// <param name="culture">The culture to use in the converter. This parameter is not used.</param>
        /// <returns>Throws NotImplementedException.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
