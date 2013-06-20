using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest
{
    public sealed class ValueHolder<T>
    {
        #region Public Properties

        public T Value
        {
            get;
            set;
        }

        #endregion
    }
}