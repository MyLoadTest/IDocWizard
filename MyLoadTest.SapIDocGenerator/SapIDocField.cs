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
    public class SapIDocField
    {
        #region private fields

        private String _fieldName;
        private String _fieldDescription;
        private int _startPos;
        private int _length;
        // TODO: make these match the names in the definition.
        // add more field properties.
        // NOTE: do this after I can parse the input file.

        #endregion

        #region properties

        /// <summary>
        /// The field name
        /// </summary>
        public string Name
        {
            get
            {
                return _fieldName;
            }
            private set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new SapIDocDefinitionException("Field name cannot be empty.");
                }
                DebugLog.Write("Field name: {0}", value);
                _fieldName = value;
            }
        }

        /// <summary>
        /// The field description
        /// </summary>
        public string Description
        {
            get
            {
                return _fieldDescription;
            }
            private set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new SapIDocDefinitionException("Field description cannot be empty.");
                }
                DebugLog.Write("Field description: {0}", value);
                _fieldDescription = value;
            }
        }

        /// <summary>
        /// The starting position of the field i.e. the byte/character offset.
        /// Note tht fields never start at 0, as the segment name takes up the first x characters.
        /// </summary>
        public int Position
        {
            get
            {
                return _startPos;
            }
            private set
            {
                if (value < 0)
                {
                    string msg = String.Format("Field starting position cannot be negative. Value: {0}", value);
                    throw new SapIDocDefinitionException(msg);
                }
                DebugLog.Write("Field starting position: {0}", value);
                _startPos = value;
            }
        }

        /// <summary>
        /// The length of the field in bytes/characters. Length is measured from the field's starting position.
        /// </summary>
        public int Length
        {
            get
            {
                return _length;
            }
            private set
            {
                if (value < 1)
                {
                    string msg = String.Format("Field length position cannot be 0 or negative. Value: {0}", value);
                    throw new SapIDocDefinitionException(msg);
                }
                DebugLog.Write("Field length position: {0}", value);
                _length = value;
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// The no-args constructor is not allowed
        /// </summary>
        private SapIDocField()
        {
        }

        /// <summary>
        /// Field constructor.
        ///
        /// When a Field object is created, the field definition must be specified.
        /// </summary>
        /// <param name="fieldName">The name of the Field</param>
        /// <param name="fieldDescription">The description of the Field</param>
        /// <param name="startPos">The starting position of the field. Note that this may have be to be calculated from the lengths of the preceeding fields.</param>
        /// <param name="length">The length of the Field</param>
        public SapIDocField(string fieldName, string fieldDescription, int startPos, int length)
        {
            Name = fieldName;
            Description = fieldDescription;
            Position = startPos;
            Length = length;
        }

        // TODO:
        /// <summary>
        /// Note that the length of the Field is calculated from the length of the enum values (which should all be the same length).
        /// Note: I don't really need this anymore, as it is not possible to create an IDoc definition from the XSD, only to verify an IDoc XML.
        /// </summary>
        /// <param name="fieldName">The name of the Field</param>
        /// <param name="fieldDescription">The description of the Field</param>
        /// <param name="startPos">The starting position of the field. Note that this may have be to be calculated from the lengths of the preceeding fields.</param>
        /// <param name="length">The length of the Field</param>
        /// <param name="enumValues"></param>
        public SapIDocField(string fieldName, int startPos, Dictionary<string, string> enumValues)
        {
            // TODO: can have IsEnum property that checks if _enumValues is null.
            // FUCk! each enum value has a different description.
        }

        #endregion

        // Note: not sure if I need any methods for this object, as it is really a data object.
        // TODO: might want a method to extract field values.
    }
}