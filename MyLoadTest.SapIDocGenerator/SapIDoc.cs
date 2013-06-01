using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MyLoadTest.SapIDocGenerator
{
    /// <summary>
    /// The SapIDoc class contains the Segment/Field values. It can output as XML or as a flat-text SapIDoc file.
    ///
    /// To learn more about IDocs, it is suggested that you read:
    /// * http://www.saptraininghub.com/all-about-idocdefinition-architecture-implementation/
    /// As of Release 4.6, the IDoc interface supports XML for outbound and inbound processing.
    /// </summary>
    public sealed class SapIDoc
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SapIDoc"/> class.
        /// </summary>
        /// <param name="definition">
        ///     The definition of IDoc.
        /// </param>
        /// <param name="flatTextIdoc">
        ///     The contents of a flat-text IDoc file.
        /// </param>
        public SapIDoc(SapIDocDefinition definition, string flatTextIdoc)
        {
            DebugLog.Write("========== New IDoc created ==========");

            this.Definition = definition;
            this.ControlRecord = new Dictionary<string, string>();
            this.Segments = new List<Dictionary<string, string>>();

            Parse(flatTextIdoc);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The IDoc Basic Type (from IDOCTYP field in Control Record)
        /// </summary>
        public string Type
        {
            get;
            private set;
        }

        /// <summary>
        /// The IDoc number (from DOCNUM field in Control Record)
        /// </summary>
        public string Number
        {
            get;
            private set;
        }

        /// <summary>
        /// IDoc Control Record for Interface to External System
        /// The IDoc Control Record is always the first line in the IDoc file.
        /// </summary>
        public Dictionary<string, string> ControlRecord
        {
            get;
            private set;
        }

        /// <summary>
        /// The Segments of the IDoc.
        /// Note that the each Segment also contains the fields from the Data Record.
        /// </summary>
        public List<Dictionary<string, string>> Segments
        {
            get;
            private set;
        }

        /// <summary>
        /// The SapIDocDefinition contains information about segment properties and field positions. It is needed in order
        /// to interpret the flat-text IDoc file.
        /// </summary>
        public SapIDocDefinition Definition
        {
            get;
            private set;
        }

        /////// <summary>
        /////// The XSD (exported from WE60) contains enough information to validate the field lengths and values in an
        /////// IDoc if it is in XML format.
        /////// Note: This property must be set before checking the .IsValid property.
        /////// </summary>
        ////public XElement Xsd
        ////{
        ////    // TODO: it would be better to have a LoadXsd() method, as the XSD must be modified before being used for validation.
        ////    get;
        ////    private set;
        ////}

        #endregion

        #region Public Methods

        /// <summary>
        /// An XML representation of the IDoc.
        /// Note that it would be possible to do Schema validation of this XML with no changes to the XSD, but this
        /// feature has been deferred.
        /// </summary>
        /// <returns></returns>
        public XElement GetXml()
        {
            DebugLog.Write("Building XML output...");

            var idoc = new XElement("IDOC", new XAttribute("BEGIN", "1"));
            //// Add the Control Record fields
            var seg = new XElement(ControlRecord["TABNAM"], new XAttribute("SEGMENT", "1"));
            foreach (var field in ControlRecord)
            {
                seg.Add(new XElement(field.Key, field.Value));
            }

            idoc.Add(seg);

            // Add each segment (in order)
            foreach (var segment in Segments)
            {
                seg = new XElement(segment["SEGNAM"], new XAttribute("SEGMENT", "1"));
                foreach (var field in segment)
                {
                    seg.Add(new XElement(field.Key, field.Value));
                }

                idoc.Add(seg);
            }

            var xml = new XElement(Type, idoc);

            DebugLog.Write("XML IDoc:\n{0}", xml.ToString());
            return xml;
        }

        /// <summary>
        /// Returns true if the IDoc segments and fields are valid according to the XSD. This includes field lengths,
        /// enumerated values,
        /// Note: Getting this property will cause an exception to be throw if it is called before the Xsd property
        /// has been set.
        /// Note: This is quite an expensive property to call, as it will need to generate an XML representation of
        /// the IDoc before checking it with the XSD.
        ///
        /// NOTES:
        /// From OxygenXml...
        /// Warning (http://www.w3.org/TR/xml-schema-1, oxygen-maxOccursLimit) oxygen-maxOccursLimit: In the declaration of 'element', the value of 'maxOccurs' is '999999', it will be considered 'unbounded' for validation because large values for maxOccurs are not supported by Xerces.
        /// Warning (http://www.w3.org/TR/xml-schema-1, oxygen-maxOccursLimit) oxygen-maxOccursLimit: In the declaration of 'element', the value of 'maxOccurs' is '999999', it will be considered 'unbounded' for validation because large values for maxOccurs are not supported by Xerces.
        /// Warning (http://www.w3.org/TR/xml-schema-1, oxygen-maxOccursLimit) oxygen-maxOccursLimit: In the declaration of 'element', the value of 'maxOccurs' is '999999', it will be considered 'unbounded' for validation because large values for maxOccurs are not supported by Xerces
        ///
        /// Note: may have to manually add the fields from the Data Record to each Segment definition in the XSD, so that it can properly validate (and so the IDoc is easier to generate).
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            throw new NotImplementedException("This will be implemented in a future version of the IDoc Generator.");
        }

        /// <summary>
        ///
        /// It does the following
        ///  - adds quotes around lines (preserving indenting)
        ///  - escapes quote for attributes
        ///  - adds a length attribute for each field
        ///  - adds a comment with the Segment/Field description
        ///  - adds a comment for enumerated fields with allowed values (and their descriptions)
        ///
        /// Example output:
        /// &quot;&lt;ZISUPODMAS_BAPIZBUSMASSENDEM01&gt;&quot;
        /// &quot;    &lt;IDOC BEGIN=\&quot;1\&quot;&gt;&quot;
        /// &quot;        &lt;EDI_DC40 SEGMENT=\&quot;1\&quot;&gt;&quot;                    // IDoc Control Record for Interface to External System
        /// &quot;            &lt;TABNAM length=\&quot;10\&quot;&gt;EDI_DC40&lt;/TABNAM&gt;&quot; // Name of Table Structure
        /// &quot;            &lt;MANDT length=\&quot;3\&quot;&gt;MAN&lt;/MANDT&gt;&quot;         // Client
        /// &quot;            &lt;DIRECT length=\&quot;1\&quot;&gt;1&lt;/DIRECT&gt;&quot;         // Direction [1 = Outbound, 2 = Inbound]
        ///
        /// </summary>
        /// <returns>Text that is ready to be used by a function in VuGen.</returns>
        public string GetVuGenActionContents()
        {
            DebugLog.Write("Building VuGen output...");

            var idoc = new StringBuilder();

            //// Start building the IDoc string
            idoc.AppendFormat(
                "    // Create an IDoc file from XML input. Note that values can be parameterized.\n" +
                    "    idoc_create(\"IDocParam\",\n" +
                    "        \"<{0}>\"\n" +
                    "        \"    <IDOC BEGIN=\\\"1\\\">\"\n",
                this.Type);

            // Add the IDoc Control Record fields
            var segmentName = ControlRecord["TABNAM"];
            var segmentDescription = Definition.ControlRecord.Description;
            idoc.AppendFormat(
                "        \"        <{0} SEGMENT=\\\"1\\\">\" // {1}\n", segmentName, segmentDescription);
            foreach (var field in ControlRecord)
            {
                var fieldDescription = Definition.ControlRecord[field.Key].Description;
                var fieldLength = Definition.ControlRecord[field.Key].Length;
                idoc.AppendFormat(
                    "        \"            <{0} length=\\\"{1}\\\">{2}</{3}>\" // {4}\n",
                    field.Key,
                    fieldLength.ToString(CultureInfo.InvariantCulture),
                    field.Value,
                    field.Key,
                    fieldDescription);
            }

            idoc.AppendFormat("        \"        </{0}>\"\n", segmentName);

            // Add each IDoc segment (in order)
            foreach (var segment in Segments)
            {
                segmentName = segment["SEGNAM"];
                segmentDescription = Definition.Segments[segmentName].Description;
                idoc.AppendFormat(
                    "        \"        <{0} SEGMENT=\\\"1\\\">\" // {1}\n",
                    segmentName,
                    segmentDescription);
                foreach (var field in segment)
                {
                    // The field Description could be in either the Data Segment definition or the Segment definition.
                    // Note that this code assumes that field names are never the same in the Data Record and the Segment Data.
                    string fieldDescription;
                    int fieldLength;
                    if (Definition.DataRecord.Contains(field.Key))
                    {
                        fieldDescription = Definition.DataRecord[field.Key].Description;
                        fieldLength = Definition.DataRecord[field.Key].Length;
                    }
                    else
                    {
                        fieldDescription = Definition.Segments[segmentName][field.Key].Description;
                        fieldLength = Definition.Segments[segmentName][field.Key].Length;
                    }

                    idoc.AppendFormat(
                        "        \"            <{0} length=\\\"{1}\\\">{2}</{3}>\" // {4}\n",
                        field.Key,
                        fieldLength.ToString(CultureInfo.InvariantCulture),
                        field.Value,
                        field.Key,
                        fieldDescription);
                }

                idoc.AppendFormat("        \"        </{0}>\"\n", segmentName);
            }

            // Add the last part of the IDoc string
            idoc.AppendFormat(
                "        \"    </IDOC>\"\n" +
                    "        \"</{0}>\");\n",
                this.Type);

            // TODO: The license should be added from a file or from a registry key, so that it is not hard-coded for all users.
            const string License = "    // The license key must be set before idoc_create() can be called.\n" +
                "    idoc_set_license(\n" +
                "        \"<license>\"\n" +
                "        \"    <name>Joe User</name>\"\n" +
                "        \"    <company>BigCo</company>\"\n" +
                "        \"    <email>joe.user@example.com</email>\"\n" +
                "        \"    <key>ANUAA-ADHHB-BS7VU-MVH45-9ZG3B-U3PUQ</key>\"\n" +
                "        \"    <expires>2013-10-01</expires>\"\n" +
                "        \"</license>\");\n";

            // TODO: put error message code here in case DLL is not found.
            const string Dll = "    // The IDoc DLL must be loaded before any idoc_ functions are called.\n" +
                "    int rc = lr_load_dll(\"idoc.dll\");\n" +
                "    if (rc != 0) {\n" +
                "        lr_error_message(\"Problem loading idoc.dll\");\n" +
                "        lr_abort();\n" +
                "    }\n";

            // This is the Action.c file
            // Note special escaping for curly braces due to string.Format replacement parameters.
            var action = string.Format(
                "Action()\n" +
                    "{{\n" +
                    "{0}" +
                    "\n" +
                    "{1}" +
                    "\n" +
                    "{2}" +
                    "\n" +
                    "    return 0;\n" +
                    "}}",
                Dll,
                License,
                idoc);

            DebugLog.Write("VuGen output:{0}{1}", Environment.NewLine, action);
            return action;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Extracts field data from each Segment in the input IDoc.
        /// </summary>
        /// <param name="flatTextIdoc">
        ///     The flat-text IDoc file.
        /// </param>
        private void Parse(string flatTextIdoc)
        {
            DebugLog.Write("Parsing flat text input IDoc:\n{0}", flatTextIdoc);

            // Each Record/Segment will be on a new line.
            // "RemoveEmptyEntries" handles case of blank rows due to "\r\n"
            var rows = flatTextIdoc.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Length < 2)
            {
                throw new SapIDocException("An IDoc cannot have less than 2 segments.");
            }

            // The first row is always the Control Record. All other rows are Segments, with a Data Record at the start.
            DebugLog.Write("Processing Control Record...");
            foreach (var field in Definition.ControlRecord)
            {
                var value = rows[0].Substring(field.StartPosition, field.Length).TrimEnd();
                DebugLog.Write("{0}={1}", field.Name, value);
                this.ControlRecord.Add(field.Name, value);

                switch (field.Name)
                {
                    case "IDOCTYP":
                        this.Type = value;
                        break;

                    case "DOCNUM":
                        this.Number = value;
                        break;
                }
            }

            // Process all the Segments
            for (var i = 1; i < rows.Length; i++)
            {
                // Process the fields from the Data Record (except the SDATA field)
                var currentSegment = new Dictionary<string, string>();
                string segmentName = null;
                foreach (var field in Definition.DataRecord)
                {
                    if (field.Name == "SDATA")
                    {
                        break; // this is the last field in the Data Record section of the Segment.
                    }

                    var value = rows[i].Substring(field.StartPosition, field.Length).TrimEnd();
                    //// TODO: can throw an OutOfBounds Exception if it is the wrong IDoc.
                    if (field.Name == "SEGNAM")
                    {
                        segmentName = value;
                        DebugLog.Write("Starting Segment \"{0}\"...", segmentName);
                    }

                    DebugLog.Write("{0}={1}", field.Name, value);
                    currentSegment.Add(field.Name, value);
                }

                if (segmentName == null)
                {
                    throw new SapIDocException("Segment is not found.");
                }

                // Process the fields from the specified Segment.
                foreach (var field in Definition.Segments[segmentName])
                {
                    // TODO: can throw a KeyNotFoundException if it is the wrong IDoc.
                    var val = rows[i].Substring(field.StartPosition, field.Length).TrimEnd();
                    DebugLog.Write("{0}={1}", field.Name, val);
                    currentSegment.Add(field.Name, val);
                }

                Segments.Add(currentSegment);
            }
        }

        #endregion
    }
}