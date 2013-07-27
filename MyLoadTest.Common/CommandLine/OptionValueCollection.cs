using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MyLoadTest.CommandLine
{
    public class OptionValueCollection : IList, IList<string>
    {
        #region Constants and Fields

        private readonly List<string> _values = new List<string>();
        private readonly OptionContext _optionContext;

        #endregion

        #region Constructors

        internal OptionValueCollection(OptionContext optionContext)
        {
            #region Argument Check

            if (optionContext == null)
            {
                throw new ArgumentNullException("optionContext");
            }

            #endregion

            _optionContext = optionContext;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Join(", ", _values.ToArray());
        }

        #endregion

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_values).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)_values).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)_values).SyncRoot;
            }
        }

        #endregion

        #region ICollection<T>

        public void Add(string item)
        {
            _values.Add(item);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(string item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return _values.Remove(item);
        }

        public int Count
        {
            get
            {
                return _values.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable<T>

        public IEnumerator<string> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        #endregion

        #region IList

        int IList.Add(object value)
        {
            return ((IList)_values).Add(value);
        }

        bool IList.Contains(object value)
        {
            return ((IList)_values).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_values).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ((IList)_values).Insert(index, value);
        }

        void IList.Remove(object value)
        {
            ((IList)_values).Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            ((IList)_values).RemoveAt(index);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                ((IList)_values)[index] = value;
            }
        }

        #endregion

        #region IList<T>

        public int IndexOf(string item)
        {
            return _values.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            _values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _values.RemoveAt(index);
        }

        private void AssertValid(int index)
        {
            if (_optionContext.Option == null)
            {
                throw new InvalidOperationException("OptionContext.Option is null.");
            }

            if (index >= _optionContext.Option.MaxValueCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (_optionContext.Option.OptionValueType == OptionValueType.Required && index >= _values.Count)
            {
                throw new OptionException(
                    string.Format(
                        _optionContext.OptionSet.MessageLocalizer("Missing required value for option '{0}'."),
                        _optionContext.OptionName),
                    _optionContext.OptionName);
            }
        }

        public string this[int index]
        {
            get
            {
                AssertValid(index);
                return index >= _values.Count ? null : _values[index];
            }

            set
            {
                _values[index] = value;
            }
        }

        #endregion
    }
}