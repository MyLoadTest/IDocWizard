using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MyLoadTest.SapIDocGenerator.UI.Converters
{
    public sealed class BooleanToVisibilityConverter : BooleanToValueConverter<Visibility>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BooleanToVisibilityConverter"/> class.
        /// </summary>
        public BooleanToVisibilityConverter()
        {
            this.TrueValue = Visibility.Visible;
            this.FalseValue = Visibility.Collapsed;
        }

        #endregion
    }
}