using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MyLoadTest
{
    public sealed class IniFileSection : IEnumerable<KeyValuePair<string, string>>
    {
        #region Constants and Fields

        private readonly IniFileSectionInternal _internal;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IniFileSection"/> class.
        /// </summary>
        internal IniFileSection(IniFile owner, string name, bool isSeparatedFromPrevious)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            #endregion

            _internal = new IniFileSectionInternal();
            this.Owner = owner;
            this.Name = name;
            this.IsSeparatedFromPrevious = isSeparatedFromPrevious;
        }

        #endregion

        #region Public Properties

        public IniFile Owner
        {
            get;
            internal set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Count
        {
            [DebuggerNonUserCode]
            get
            {
                return _internal.Count;
            }
        }

        public string this[string key]
        {
            get
            {
                #region Argument Check

                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                #endregion

                EnsureValid();

                return _internal.Contains(key) ? _internal[key].Value : null;
            }

            set
            {
                AddOrUpdate(key, value);
            }
        }

        #endregion

        #region Internal Properties

        internal bool IsSeparatedFromPrevious
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}. Name = {1}, Count = {2}",
                GetType().Name,
                this.Name,
                this.Count);
        }

        public void Add(string key, string value)
        {
            #region Argument Check

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            #endregion

            EnsureValid();

            _internal.Add(new KeyValuePair<string, string>(key, value));
        }

        public void AddOrUpdate(string key, string value)
        {
            #region Argument Check

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            #endregion

            EnsureValid();

            if (!_internal.Contains(key))
            {
                _internal.Add(new KeyValuePair<string, string>(key, value));
                return;
            }

            var oldValue = _internal[key].Value;
            var index = _internal.IndexOf(new KeyValuePair<string, string>(key, oldValue));
            if (index < 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Unable to find a pair with the key '{0}' though it has to exist.",
                        key));
            }

            _internal[index] = new KeyValuePair<string, string>(key, value);
        }

        public bool Contains(string key)
        {
            #region Argument Check

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            #endregion

            EnsureValid();

            return _internal.Contains(key);
        }

        public void Remove(string key)
        {
            #region Argument Check

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            #endregion

            EnsureValid();

            _internal.Remove(key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,string>> Members

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private Methods

        private void EnsureValid()
        {
            if (this.Owner == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The section '{0}' does not belong to any {1}.",
                        this.Name,
                        typeof(IniFile).Name));
            }
        }

        #endregion

        #region IniFileSectionInternal Class

        private sealed class IniFileSectionInternal : KeyedCollection<string, KeyValuePair<string, string>>
        {
            #region Constructors

            internal IniFileSectionInternal()
                : base(IniFile.NameComparer)
            {
                // Nothing to do
            }

            #endregion

            #region Protected Methods

            protected override string GetKeyForItem(KeyValuePair<string, string> item)
            {
                return item.Key;
            }

            #endregion
        }

        #endregion
    }
}