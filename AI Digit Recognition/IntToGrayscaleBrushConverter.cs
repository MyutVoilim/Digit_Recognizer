﻿using System;
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && intValue >= 0 && intValue <= 255)
            {
                byte byteValue = (byte)intValue;
                return new SolidColorBrush(Color.FromRgb(byteValue, byteValue, byteValue));
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
