using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator.UI
{
    public sealed class ControlItem<T> : IEquatable<ControlItem<T>>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ControlItem{T}"/> class.
        /// </summary>
        public ControlItem(T value, string text)
        {
            this.Value = value;
            this.Text = text ?? (ReferenceEquals(value, null) ? string.Empty : (value.ToString() ?? string.Empty));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ControlItem{T}"/> class.
        /// </summary>
        public ControlItem(T value)
            : this(value, null)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public T Value
        {
            get;
            private set;
        }

        public string Text
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            return Equals(obj as ControlItem<T>);
        }

        public override int GetHashCode()
        {
            return ReferenceEquals(this.Value, null) ? 0 : this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Text;
        }

        #endregion

        #region IEquatable<ControlItem<T>> Members

        public bool Equals(ControlItem<T> other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(this.Value, other.Value);
        }

        #endregion
    }
}