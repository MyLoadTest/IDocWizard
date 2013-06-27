using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MyLoadTest.SapIDocGenerator.UI.Converters
{
    public class BooleanToValueConverter<T> : IValueConverter
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BooleanToValueConverter{T}"/> class.
        /// </summary>
        public BooleanToValueConverter()
        {
            this.TrueValue = default(T);
            this.FalseValue = default(T);
        }

        #endregion

        #region Public Properties

        public T TrueValue
        {
            get;
            set;
        }

        public T FalseValue
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
                throw new ArgumentException(string.Format(@"The value must be {0}.", typeof(bool).Name), "value");
            }

            #endregion

            return (bool)value ? this.TrueValue : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region Argument Check

            if (!(value is T))
            {
                throw new ArgumentException(
                    string.Format(@"The value must be {0}.", typeof(T).Name),
                    "value");
            }

            #endregion

            return EqualityComparer<T>.Default.Equals((T)value, this.TrueValue);
        }

        #endregion
    }
}