using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI
{
    public static class ControlItem
    {
        #region Public Methods

        public static ControlItem<T> Create<T>(T value, string text)
        {
            return new ControlItem<T>(value, text);
        }

        public static ControlItem<T> Create<T>(T value)
        {
            return new ControlItem<T>(value);
        }

        #endregion
    }
}