using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MyLoadTest.SapIDocGenerator.UI.Converters
{
    public sealed class ComputedHeightConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            #region Argument Check

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (targetType != typeof(double))
            {
                throw new ArgumentException(null, "targetType");
            }

            #endregion

            var result = 0d;
            for (var index = 0; index < values.Length; index++)
            {
                var value = values[index];

                double castValue;
                if (value is int)
                {
                    castValue = (int)value;
                }
                else if (value is double)
                {
                    castValue = (double)value;
                }
                else if (value is Thickness)
                {
                    var thickness = (Thickness)value;
                    castValue = thickness.Top + thickness.Bottom;
                }
                else
                {
                    throw new ArgumentException(@"Invalid value type.", "values");
                }

                result += index == 0 ? castValue : -castValue;
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}