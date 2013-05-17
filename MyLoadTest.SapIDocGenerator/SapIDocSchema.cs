using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace MyLoadTest.SapIDocGenerator
{
    /// <summary>
    /// NOTE: it is probably better to add the schema to the IDoc, and give it an IsValid() method.
    /// Having a separate class for the schema will not work because I am going to have to additional elements for the
    /// fields in the Data Record to each segment.
    /// </summary>
    public class SapIDocSchema
    {
        #region fields

        private XmlSchemaSet schemas = new XmlSchemaSet();

        #endregion

        #region properties

        #endregion

        #region constructors

        /// <summary>
        /// The no-args constructor is not allowed.
        /// </summary>
        private SapIDocSchema()
        {
        }

        /// <summary>
        /// Constructor to create a new SapIDocSchema from an XSD that has been exported from SAP.
        /// </summary>
        /// <param name="xsd"></param>
        public SapIDocSchema(XElement xsd)
        {
            DebugLog.Write("New SapIDocSchema created from:\n{0}", xsd.ToString());
            schemas.Add("", XmlReader.Create(new StringReader(xsd.ToString())));
        }

        #endregion

        #region public methods

        /// <summary>
        /// Create an IDoc Schema from an XSD file that has been exported from SAP.
        /// </summary>
        /// <param name="path">The XSD filename</param>
        /// <returns>Returns a new SapIDocSchema object.</returns>
        public static SapIDocSchema Load(string path)
        {
            DebugLog.Write("Calling SapIDocSchema.Load({0})", path);
            // Read file contents
            XElement definition;
            try
            {
                definition = XElement.Load(path);
            }
            catch (Exception e)
            {
                string msg = String.Format("Problem reading file {0}", path);
                throw new SapIDocDefinitionException(msg, e);
            }
            return new SapIDocSchema(definition);
        }

        /// <summary>
        /// Checks whether the IDoc XML conforms to the definition in the XSD.
        /// </summary>
        /// <param name="idoc">The IDoc to check.</param>
        /// <returns>Returns true if the IDoc validates successfully, otherwise throws an SapIDocDefinitionException (yes,
        /// this is a bit of a WTF).</returns>
        public bool Validate(XElement idoc)
        {
            // Schema validation is much simpler if you use an XDocument instead of an XElement.
            XDocument doc = new XDocument(idoc);

            // Note that the second argument is a ValidationEventHandler(Object sender, ValidationEventArgs e)
            doc.Validate(
                schemas,
                (o, e) =>
                {
                    string msg = String.Format("The IDoc XML did not validate.\n{0}", e.Message);
                    throw new SapIDocDefinitionException(msg);
                });

            return true;
        }

        #endregion
    }
}