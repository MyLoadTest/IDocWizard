using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public sealed class SapIDocSchema
    {
        #region Fields

        private readonly XmlSchemaSet _schemas = new XmlSchemaSet();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SapIDocSchema"/> class
        ///     using an XSD that has been exported from SAP.
        /// </summary>
        /// <param name="xsd">
        ///     The root element of the XSD schema.
        /// </param>
        public SapIDocSchema(XElement xsd)
        {
            Logger.DebugFormat("Creating new IDoc schema from: {0}", xsd);

            using (var xsdReader = new StringReader(xsd.ToString()))
            {
                using (var schemaDocument = XmlReader.Create(xsdReader))
                {
                    _schemas.Add(string.Empty, schemaDocument);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates an IDoc schema from the specified XSD file that was exported from SAP.
        /// </summary>
        /// <param name="path">
        ///     The XSD filename.
        /// </param>
        /// <returns>
        ///     Returns a new <see cref="SapIDocSchema"/> object.
        /// </returns>
        public static SapIDocSchema LoadFromFile(string path)
        {
            Logger.DebugFormat("Entered {0}('{1}')", MethodBase.GetCurrentMethod().GetQualifiedName(), path);

            XElement definition;
            try
            {
                definition = XElement.Load(path);
            }
            catch (Exception ex)
            {
                var message = string.Format("Error loading XSD schema from '{0}'.", path);
                throw new SapIDocException(message, ex);
            }

            return new SapIDocSchema(definition);
        }

        /// <summary>
        ///     Checks whether the IDoc XML conforms to the definition in the XSD.
        /// </summary>
        /// <param name="idoc">
        ///     The root XML element of the IDoc to check.
        /// </param>
        /// <exception cref="SapIDocException">
        ///     Validation has failed.
        /// </exception>
        public void Validate(XElement idoc)
        {
            // Schema validation is much simpler if you use an XDocument instead of an XElement.
            var doc = new XDocument(idoc);

            doc.Validate(
                _schemas,
                (sender, e) =>
                {
                    var message = string.Format("The IDoc XML validation has failed: {0}", e.Message);
                    throw new SapIDocException(message);
                });
        }

        #endregion
    }
}