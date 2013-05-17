using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator
{
    /// <summary>
    /// A segment is a collection of fields, with a segment name.
    ///
    /// Note: fields must be added to the segment in the correct order.
    ///
    /// Documentation for the KeyedCollection abstract base class: http://msdn.microsoft.com/en-us/library/ms132438.aspx
    /// Documentation for the Tuple class: http://msdn.microsoft.com/en-us/library/system.tuple.aspx
    /// </summary>
    public class SapIDocSegment : KeyedCollection<string, SapIDocField>
    {
        #region fields

        private string _segmentName;
        private string _segmentDescription;

        #endregion

        #region properties

        /// <summary>
        /// The Segment name e.g. "EDI_DC40".
        /// Note that this property can only be set when an instance of the class is created.
        /// </summary>
        public string Name
        {
            get
            {
                return _segmentName;
            }
            private set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new SapIDocDefinitionException("Segment name cannot be empty.");
                }
                DebugLog.Write("Segment name: {0}", value);
                _segmentName = value;
            }
        }

        /// <summary>
        /// The Segment description type e.g. "IDoc Control Record for Interface to External System".
        /// Note that this property can only be set when an instance of the class is created.
        /// </summary>
        public string Description
        {
            get
            {
                return _segmentDescription;
            }
            private set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new SapIDocDefinitionException("Segment description cannot be empty.");
                    // This might cause problems if they haven't bothered to enter a description.
                }
                DebugLog.Write("Segment description: {0}", value);
                _segmentDescription = value;
            }
        }

        #endregion

        #region constructor

        /// <summary>
        /// If you use this constructor, you will have to call Add() for each field that is added to the IDoc.
        /// </summary>
        /// <param name="name">The Name of the Segment</param>
        /// <param name="description">The Description of the Segment</param>
        public SapIDocSegment(string name, string description)
        {
            DebugLog.Write(
                "New Segment created without any Field objects. Fields must be added to the Segment by calling the Add() method.");
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Create a new Segment from an List<> of fields.
        /// Using this constructor means that it is not necessary to call Add() for each Field.
        /// </summary>
        /// <param name="name">The Name of the Segment</param>
        /// <param name="description">The Description of the Segment</param>
        /// <param name="fields">A List<> of Fields</param>
        public SapIDocSegment(string name, string description, List<SapIDocField> fields)
        {
            DebugLog.Write("New Segment created from List<> of {0} Fields", fields.Count);
            Name = name;
            Description = description;
            foreach (SapIDocField field in fields)
            {
                this.Add(field);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Temporary method to print the fields in the input string (which is an IDoc segment/line from a text IDoc file.
        /// </summary>
        /// <param name="input"></param>
        public void PrintFields(string input)
        {
            foreach (SapIDocField field in this)
            {
                // TODO: http://stackoverflow.com/questions/644017/net-format-a-string-with-fixed-spaces
                Console.WriteLine("{0, 10}: {1}", field.Name, input.Substring(field.Position - 1, field.Length));
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// This is used by ...
        /// Note: When inheriting from KeyedCollection, this is the only method that absolutely must be overridden,
        /// because without it the KeyedCollection cannot extract the keys from the items.
        /// </summary>
        /// <param name="field"></param>
        /// <returns>the fieldname as a string</returns>
        protected override string GetKeyForItem(SapIDocField field)
        {
            // The key is the field's name.
            return field.Name;
        }

        #endregion
    }
}