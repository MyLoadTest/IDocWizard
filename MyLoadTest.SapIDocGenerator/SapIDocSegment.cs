using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator
{
    /// <summary>
    ///     Represents an IDoc segment which is a collection of fields, with a segment name.
    ///     NOTE: Fields must be added to the segment in the correct order.
    /// </summary>
    public sealed class SapIDocSegment : KeyedCollection<string, SapIDocField>
    {
        #region Constructors

        /// <summary>
        /// If you use this constructor, you will have to call Add() for each field that is added to the IDoc.
        /// </summary>
        /// <param name="name">The Name of the Segment</param>
        /// <param name="description">The Description of the Segment</param>
        public SapIDocSegment(string name, string description)
        {
            Logger.Debug("Creating new IDoc segment");
            Logger.DebugFormat("  Segment name: '{0}'", name);
            Logger.DebugFormat("  Segment description: '{0}'", description);

            #region Argument Check

            if (name.IsNullOrEmpty())
            {
                throw new ArgumentException("Segment name cannot be empty.", "name");
            }

            if (description.IsNullOrEmpty())
            {
                throw new ArgumentException("Segment description cannot be empty.", "description");
            }

            #endregion

            this.Name = name;
            this.Description = description;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The Segment name e.g. "EDI_DC40".
        /// Note that this property can only be set when an instance of the class is created.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The Segment description type e.g. "IDoc Control Record for Interface to External System".
        /// Note that this property can only be set when an instance of the class is created.
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Temporary method to print the fields in the input string (which is an IDoc segment/line from a text IDoc file.
        /// </summary>
        /// <param name="input"></param>
        public void PrintFields(string input)
        {
            foreach (var field in this)
            {
                // TODO: http://stackoverflow.com/questions/644017/net-format-a-string-with-fixed-spaces
                Console.WriteLine("{0, 10}: {1}", field.Name, input.Substring(field.StartPosition - 1, field.Length));
            }
        }

        #endregion

        #region Protected Methods

        protected override string GetKeyForItem(SapIDocField field)
        {
            return field == null ? null : field.Name;
        }

        protected override void InsertItem(int index, SapIDocField item)
        {
            #region Argument Check

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            #endregion

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, SapIDocField item)
        {
            #region Argument Check

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            #endregion

            base.SetItem(index, item);
        }

        #endregion
    }
}