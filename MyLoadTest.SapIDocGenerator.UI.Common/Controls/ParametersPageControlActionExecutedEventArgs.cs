using System;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI.Controls
{
    public sealed class ParametersPageControlActionExecutedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParametersPageControlActionExecutedEventArgs"/> class.
        /// </summary>
        internal ParametersPageControlActionExecutedEventArgs(bool replaced)
        {
            this.Replaced = replaced;
        }

        #endregion

        #region Public Properties

        public bool Replaced
        {
            get;
            private set;
        }

        #endregion
    }
}