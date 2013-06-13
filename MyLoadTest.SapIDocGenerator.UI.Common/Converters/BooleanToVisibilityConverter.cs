using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MyLoadTest.SapIDocGenerator.UI.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BooleanToVisibilityConverter"/> class.
        /// </summary>
        public BooleanToVisibilityConverter()
        {
            this.TrueVisibility = Visibility.Visible;
            this.FalseVisibility = Visibility.Collapsed;
        }

        #endregion

        #region Public Properties

        public Visibility TrueVisibility
        {
            get;
            set;
        }

        public Visibility FalseVisibility
        {
            get;
            set;
        }

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region Argument Check

            if (!(value is bool))
            {
                throw new ArgumentException(@"The value must be Boolean.", "value");
            }

            #endregion

            return (bool)value ? this.TrueVisibility : this.FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region Argument Check

            if (!(value is Visibility))
            {
                throw new ArgumentException(
                    string.Format(@"The value must be {0}.", typeof(Visibility).Name),
                    "value");
            }

            #endregion

            return (Visibility)value == this.TrueVisibility;
        }

        #endregion
    }
}