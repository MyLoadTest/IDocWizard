using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyLoadTest
{
    public sealed class IniFile
    {
        #region Constants and Fields

        private const string SectionGroupName = "name";
        private const char KeyValueSeparator = '=';

        private static readonly Regex SectionRegex = new Regex(
            string.Format(CultureInfo.InvariantCulture, @"^ \s* \[ (?<{0}>[^\]]+) \] \s* $", SectionGroupName),
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Singleline);

        private readonly IniFileSectionCollectionInternal _sectionCollection;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IniFile"/> class.
        /// </summary>
        public IniFile()
        {
            _sectionCollection = new IniFileSectionCollectionInternal();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IniFile"/> class using the specified file.
        /// </summary>
        public IniFile(string filePath)
            : this()
        {
            using (var stream = File.OpenRead(filePath))
            {
                Load(stream);
            }
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            _sectionCollection.Clear();
        }

        public void Load(Stream stream)
        {
            #region Argument Check

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException(@"The stream is not readable.", "stream");
            }

            #endregion

            Clear();

            using (var streamReader = new StreamReader(stream))
            {
                IniFileSectionInternal currentSection = null;

                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var sectionMatch = SectionRegex.Match(line);
                    if (sectionMatch.Success)
                    {
                        var sectionName = sectionMatch.Groups[SectionGroupName].Value;

                        var section = _sectionCollection.Contains(sectionName)
                            ? _sectionCollection[sectionName]
                            : new IniFileSectionInternal(sectionName);

                        _sectionCollection.Add(section);
                        currentSection = section;
                        continue;
                    }

                    line = line.TrimStart();

                    string key;
                    string value;

                    var separatorIndex = line.IndexOf(KeyValueSeparator);
                    if (separatorIndex < 0)
                    {
                        key = line.TrimEnd();
                        value = null;
                    }
                    else
                    {
                        key = line.Substring(0, separatorIndex).TrimEnd();
                        value = line.Substring(separatorIndex + 1, line.Length - separatorIndex - 1);
                    }

                    if (currentSection == null)
                    {
                        currentSection = new IniFileSectionInternal(string.Empty);
                        _sectionCollection.Add(currentSection);
                    }

                    currentSection.Add(new KeyValuePair<string, string>(key, value));
                }
            }
        }

        public void Save(Stream stream)
        {
            #region Argument Check

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException(@"The stream is not writeable.", "stream");
            }

            #endregion

            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        #endregion

        #region IniFileSectionInternal Class

        private sealed class IniFileSectionInternal : KeyedCollection<string, KeyValuePair<string, string>>
        {
            #region Constructors

            public IniFileSectionInternal(string name)
                : base(StringComparer.OrdinalIgnoreCase)
            {
                #region Argument Check

                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                #endregion

                this.Name = name;
            }

            #endregion

            #region Public Properties

            public string Name
            {
                get;
                private set;
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

        #region IniFileSectionCollectionInternal Class

        private sealed class IniFileSectionCollectionInternal : KeyedCollection<string, IniFileSectionInternal>
        {
            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="IniFileSectionCollectionInternal"/> class.
            /// </summary>
            public IniFileSectionCollectionInternal()
                : base(StringComparer.OrdinalIgnoreCase)
            {
                // Nothing to do
            }

            #endregion

            #region Protected Methods

            protected override string GetKeyForItem(IniFileSectionInternal item)
            {
                #region Argument Check

                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                #endregion

                return item.Name;
            }

            #endregion
        }

        #endregion
    }
}