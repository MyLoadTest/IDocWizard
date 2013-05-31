using System;
using System.Collections.Generic;
using System.Linq;

namespace MyLoadTest.SapIDocGenerator
{
    /// <summary>
    /// Field definition
    ///  - field name
    ///  - starting position
    ///  - length
    ///  - TODO: other field properties
    /// </summary>
    public sealed class SapIDocField
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SapIDocField"/> class.
        /// </summary>
        /// <param name="name">
        ///     The name of the IDoc field.
        /// </param>
        /// <param name="description">
        ///     The description of the IDoc field.
        /// </param>
        /// <param name="startPosition">
        ///     The starting position of the IDoc field.
        ///     Note that this may have to be calculated from the lengths of the preceding fields.
        /// </param>
        /// <param name="length">
        ///     The length of the IDoc field.
        /// </param>
        public SapIDocField(string name, string description, int startPosition, int length)
        {
            DebugLog.Write("Creating new IDoc field");
            DebugLog.Write("  Field name: '{0}'", name);
            DebugLog.Write("  Field description: '{0}'", description);
            DebugLog.Write("  Field starting position: {0}", startPosition);
            DebugLog.Write("  Field length: {0}", length);

            #region Argument Check

            if (name.IsNullOrEmpty())
            {
                throw new ArgumentException("Field name cannot be empty.", "name");
            }

            if (description.IsNullOrEmpty())
            {
                throw new ArgumentException("Field description cannot be empty.", "description");
            }

            if (startPosition < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "startPosition",
                    startPosition,
                    "Field starting position cannot be negative.");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "length",
                    length,
                    "Field length must be positive.");
            }

            #endregion

            this.Name = name;
            this.Description = description;
            this.Position = startPosition;
            this.Length = length;
        }

        #endregion

        #region Public Properties

        // TODO: make these match the names in the definition.
        // add more field properties.
        // NOTE: do this after I can parse the input file.

        /// <summary>
        /// The field name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The field description
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// The starting position of the field i.e. the byte/character offset.
        /// Note tht fields never start at 0, as the segment name takes up the first x characters.
        /// </summary>
        public int Position
        {
            get;
            private set;
        }

        /// <summary>
        /// The length of the field in bytes/characters. Length is measured from the field's starting position.
        /// </summary>
        public int Length
        {
            get;
            private set;
        }

        #endregion

        // Note: not sure if I need any methods for this object, as it is really a data object.
        // TODO: might want a method to extract field values.
    }
}