using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyLoadTest
{
    public sealed class IniFile : IEnumerable<IniFileSection>
    {
        #region Constants and Fields

        internal static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

        private const string SectionGroupName = "name";
        private const char KeyValueSeparator = '=';

        private static readonly Regex SectionRegex = new Regex(
            string.Format(CultureInfo.InvariantCulture, @"^ \s* \[ (?<{0}>[^\]]+) \] \s* $", SectionGroupName),
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Singleline);

        private readonly IniFileSectionCollectionInternal _sections;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IniFile"/> class.
        /// </summary>
        public IniFile()
        {
            _sections = new IniFileSectionCollectionInternal();
        }

        #endregion

        #region Public Properties

        public int Count
        {
            [DebuggerNonUserCode]
            get
            {
                return _sections.Count;
            }
        }

        public IniFileSection this[string name]
        {
            get
            {
                return GetOrAddSection(name);
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}. Count = {1}",
                GetType().Name,
                this.Count);
        }

        public void Clear()
        {
            _sections.Clear();
        }

        public void Load(TextReader reader)
        {
            #region Argument Check

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            #endregion

            Clear();

            IniFileSection currentSection = null;
            var isPreviousLineEmpty = false;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var sectionMatch = SectionRegex.Match(line);
                if (sectionMatch.Success)
                {
                    var sectionName = sectionMatch.Groups[SectionGroupName].Value;

                    var section = _sections.Contains(sectionName)
                        ? _sections[sectionName]
                        : new IniFileSection(this, sectionName, isPreviousLineEmpty);

                    _sections.Add(section);
                    currentSection = section;
                    continue;
                }

                line = line.TrimStart();
                isPreviousLineEmpty = string.IsNullOrEmpty(line);

                if (isPreviousLineEmpty)
                {
                    continue;
                }

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
                    currentSection = new IniFileSection(this, string.Empty, false);
                    _sections.Add(currentSection);
                }

                currentSection.Add(key, value);
            }
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

            Load(new StreamReader(stream));  // MUST NOT dispose of StreamReader to prevent disposing of the stream
        }

        public void LoadFromFile(string filePath)
        {
            #region Argument Check

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    @"The value can be neither empty or whitespace-only string nor null.",
                    "filePath");
            }

            #endregion

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Load(stream);
            }
        }

        public void Save(TextWriter writer)
        {
            #region Argument Check

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            #endregion

            var insertSeparatorBetweenSections = false;
            foreach (var section in _sections)
            {
                if (insertSeparatorBetweenSections && section.IsSeparatedFromPrevious)
                {
                    writer.WriteLine();
                }

                insertSeparatorBetweenSections = true;

                if (!string.IsNullOrEmpty(section.Name))
                {
                    writer.WriteLine("[{0}]", section.Name);
                }

                foreach (var pair in section)
                {
                    writer.Write(pair.Key);
                    if (pair.Value != null)
                    {
                        writer.Write('=');
                        writer.Write(pair.Value);
                    }

                    writer.WriteLine();
                }
            }

            writer.Flush();
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

            Save(new StreamWriter(stream));  // MUST NOT dispose of StreamWriter to prevent disposing of the stream
        }

        public void SaveToFile(string filePath)
        {
            #region Argument Check

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    @"The value can be neither empty or whitespace-only string nor null.",
                    "filePath");
            }

            #endregion

            using (var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                Save(stream);
            }
        }

        public IniFileSection AddSection(string name)
        {
            #region Argument Check

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (_sections.Contains(name))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The section '{0}' already exists.",
                        name),
                    "name");
            }

            #endregion

            return AddSectionInternal(name);
        }

        public IniFileSection GetSection(string name)
        {
            return GetSectionInternal(name, false);
        }

        public IniFileSection GetOrAddSection(string name)
        {
            return GetSectionInternal(name, true);
        }

        public bool RemoveSection(string name)
        {
            #region Argument Check

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            #endregion

            if (!_sections.Contains(name))
            {
                return false;
            }

            var section = _sections[name];
            _sections.Remove(name);
            section.Owner = null;
            return true;
        }

        #endregion

        #region IEnumerable<IniFileSection> Members

        public IEnumerator<IniFileSection> GetEnumerator()
        {
            return _sections.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private Methods

        private IniFileSection AddSectionInternal(string name)
        {
            var result = new IniFileSection(this, name, true);
            _sections.Add(result);
            return result;
        }

        private IniFileSection GetSectionInternal(string name, bool createIfNotExists)
        {
            #region Argument Check

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            #endregion

            return _sections.Contains(name)
                ? _sections[name]
                : (createIfNotExists ? AddSectionInternal(name) : null);
        }

        #endregion

        #region IniFileSectionCollectionInternal Class

        private sealed class IniFileSectionCollectionInternal : KeyedCollection<string, IniFileSection>
        {
            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="IniFileSectionCollectionInternal"/> class.
            /// </summary>
            internal IniFileSectionCollectionInternal()
                : base(NameComparer)
            {
                // Nothing to do
            }

            #endregion

            #region Protected Methods

            protected override string GetKeyForItem(IniFileSection item)
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