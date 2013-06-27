using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI.Converters
{
    public sealed class BooleanToStringConverter : BooleanToValueConverter<string>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BooleanToStringConverter"/> class.
        /// </summary>
        public BooleanToStringConverter()
        {
            this.TrueValue = bool.TrueString;
            this.FalseValue = bool.FalseString;
        }

        #endregion
    }
}