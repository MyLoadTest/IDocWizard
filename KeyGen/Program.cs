namespace MyLoadTest.IDoc
{
    using NDesk.Options;
    using SoftActivate.Licensing;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    /// The IDoc Keygen creates a license key for use by the IDoc Generator Replay DLL.
    /// 
    /// The program should be invoked like this:
    /// 
    /// C:\>keygen --name="Joe User" --company="BigCo" --email="joe.user@example.com" --expiry="2013-10-01"
    /// 
    /// A license key is an XML string that looks like this:
    /// <license>
    ///     <name>Joe User</name>
    ///     <company>BigCo</company>
    ///     <email>joe.user@example.com</email>
    ///     <key></key>
    ///     <expires>2013-10-01</expires>
    /// </license>
    /// 
    /// Note that the license key is not hardware-locked. The only thing preventing someone sharing their license key
    /// is that their name and company is embedded in the license key. If any of the fields in the license key (name, 
    /// company, etc) are modified, then they key will be invalid. Changing the date on the computer would be a work-
    /// around for the expiry date restriction.
    /// 
    /// The license key will appear in every VuGen script that uses the IDoc Generator. The idoc_set_license() function
    /// must be called before any IDocs can be generated. e.g.
    /// Action()
    /// {
    ///     idoc_set_license("<license>"
    ///                      "    <name>Joe User</name>"
    ///                      "    <company>BigCo</company>"
    ///                      "    <email>joe.user@example.com</email>"
    ///                      "    <key></key>"
    ///                      "    <expires>2013-10-01</expires>"
    ///                      "</license>");
    ///                      
    ///     idoc_generate("<ZISUPODMAS_BAPIZBUSMASSENDEM01">
    ///                   "    <IDOC BEGIN=\"1\">
    ///                   "        <EDI_DC40 SEGMENT=\"1\">                    // IDoc Control Record
    ///                   "            <TABNAM length=\"8\">EDI_DC40</TABNAM>" // Name of Table Structure
    ///                   "            <MANDT length="3">123</MANDT>"          // Client
    ///                   "            <DOCNUM length="7">1234567</DOCNUM>"    // IDoc number
    ///                   "        </EDI_DC40>"
    ///                   "    </IDOC>"
    ///                   "</ZISUPODMAS_BAPIZBUSMASSENDEM01>");
    /// }
    /// 
    /// This program uses the following components:
    ///  - NDesk Options for managing command-line arguments. MIT/X11 license. http://www.ndesk.org/Options
    ///  - SoftActivate Licensing SDK. Commercial license. http://www.softactivate.com
    /// </summary>
    class Program
    {
        #region private fields
        private static DateTime expiryDate;
        private static XElement licenseXml;

        private static string name = null;
        private static string email = null;
        private static string company = null;
        private static string expires = null;
        private static bool showHelp = false;
        #endregion

        #region main entrypoint
        public static void Main(string[] args) {

            var p = new OptionSet() {
               	{ "name=", "the name of the person requesting the license", v => name = v }, // Mandatory
               	{ "email=", "the email address of the person requesting the license",  v => email = v }, // Mandatory
               	{ "company=", "the company name of the person requesting the license", v => company = v }, // Mandatory
               	{ "expiry=", "the license expiry date in yyyy-mm-dd format", v => expires = v }, // Mandatory
               	{ "?|help", "display this help and exit", v => showHelp = true }, // Optional
            };

            // Process the command-line arguments
            List<string> options;
            try {
                options = p.Parse(args);
            }
            catch (OptionException e) {
                System.Console.Write("keygen: ");
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine("Try `keygen --help' for more information.");
                return;
            }

            // Command line option "--help"
            if (showHelp == true) {
                ShowHelp(p);
                return;
            }

            // Check that the mandatory input arguments were specified.
            if ( (name == null) ||
                 (email == null) ||
                 (company == null) ||
                 (expires == null)) {
                Console.WriteLine("You did not specify all the arguments. Try `keygen --help' for more information.");
                return;
            }

            /* ---------- PROCESS EXPIRY DATE ---------- */

            // Parse the input date.
            try {
                expiryDate = DateTime.ParseExact(expires, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch {
                Console.WriteLine("Invalid expiry date format. Date must be in yyyy-mm-dd format.");
                return;
            }

            // If the expiry date is in the past, then throw an error.
            DateTime currentDate = DateTime.Now;
            if (expiryDate.CompareTo(currentDate) == -1) { // -1 is earlier
                Console.WriteLine("Expiry date cannot be in the past.");
                return;
            }

            /* ---------- PRIVATE KEY ---------- */
            string privateKeyFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\private.key";
            if (File.Exists(privateKeyFile) == false) {
                Console.WriteLine("Could not find \"private.key\". This file is needed to generate a license key.");
                return;
            }
            string privateKey = File.ReadAllText(privateKeyFile);

            /* ---------- CREATE LICENSE TEMPLATE ---------- */
            // This is mostly a cut-and-paste from \SoftActivate Licensing SDK Standard\samples\CS\KeyGenCS
            // Note that the template will have to be duplicated in the C++ code that checks the key.

            // This is a demo key. Replace this key with your purchased key.
            SDKRegistration.SetLicenseKey("SLDMX-3GK7N-NRZKM-5SECA-TV25K");

            // The license key template specifies what the license key will look like, what data will it contain and 
            // how secure the license key will be.
            KeyTemplate tmpl = new KeyTemplate();
            tmpl.NumberOfGroups = 6;
            tmpl.CharactersPerGroup = 5;
            tmpl.Encoding = LicenseKeyEncoding.Base32X;
            tmpl.GroupSeparator = "-";
            tmpl.DataSize = 30; // the license keys will hold 30 bits of data
            tmpl.SignatureSize = 120; // 120 bits = ((characters per group * number of groups) * bits per character) - 
                                      //data size ; 120 = ((6 * 5) * 5) - 30
            tmpl.ValidationDataSize = 100 * 8; // If the field size (as specified in the template) is smaller than the 
                                               // data size, the data is truncated to fit the field.

            // Set the private key used to generate the license keys. This is previously obtained by a call to 
            // LicenseKeyTemplate.GenerateSigningKeyPair().
            tmpl.SetPrivateKey(privateKey); // TODO: replace demo key "w9Oz5xMOwkYv" in file with real key.
            tmpl.SetPublicKey("doxzyMNu7n46whM="); // needed to check that the license generated successfully.

            // Define the data portion of the license key. Keep in mind that the field names are not included in the 
            // license keys. Only the field values are included.
            // Add a 14-bit field specifying the product expiration date, between 1/1/2010 and 12/31/2041
            tmpl.AddDataField("ExpirationDate",            // field name
                              LicenseKeyFieldType.Integer, // field is regarded as an integer
                              14, // field is 14 bits long (4 bits for month, 5 bits for day, 5 bits for year)
                              0  // field starts at position 0 in license key data bits
                              );

            tmpl.AddValidationField("Name", LicenseKeyFieldType.String, 40 * 8, 0);
            tmpl.AddValidationField("Company", LicenseKeyFieldType.String, 20 * 8, 40);
            tmpl.AddValidationField("Email", LicenseKeyFieldType.String, 40 * 8, 80);

            // create a key generator based on the template above
            KeyGenerator keyGen = new KeyGenerator(tmpl);

            /* ---------- ADD LICENSE PROPERTIES AND GENERATE KEY ---------- */

            // add an expiration date for this license key
            int packedDate = PackDate(expiryDate);
            keyGen.SetKeyData("ExpirationDate", packedDate);

            keyGen.SetValidationData("Name", name);
            keyGen.SetValidationData("Company", company);
            keyGen.SetValidationData("Email", email);

            // now generate the license key
            string licenseKey = keyGen.GenerateKey();

            /* ---------- CHECK THE GENERATED KEY ---------- */

            // validate the generated key. This sequence of code is also used in the actual product to validate the 
            // entered license key
            KeyValidator keyVal = new KeyValidator(tmpl);
            keyVal.SetKey(licenseKey);

            keyVal.SetValidationData("Name", name); // the key will not be valid if you set a different user name than the one you have set at key generation
            keyVal.SetValidationData("Company", company);
            keyVal.SetValidationData("Email", email);

            if (keyVal.IsKeyValid()) {
                Console.WriteLine("\nThe generated license key is: " + licenseKey);

                // now read the expiration date from the license key
                int expirationDate = keyVal.QueryIntKeyData("ExpirationDate");

                Console.WriteLine("The expiration date is " + UnpackDate(expirationDate).ToString("MMMM dd, yyyy"));

                licenseXml = new XElement("license",
                                 new XElement("name", name),
                                 new XElement("company", company),
                                 new XElement("email", email),
                                 new XElement("key", licenseKey),
                                 new XElement("expires", expires)
                             );
                Console.WriteLine(licenseXml.ToString());
            }
            else {
                Console.WriteLine("Error creating/validating license key !\n");
            }




            return;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Print usage instructions for the KeyGen
        /// </summary>
        /// <param name="p">The OptionSet that is used by the command-line program.</param>
        private static void ShowHelp(OptionSet p) {
            DateTime dateInOneMonth = DateTime.Now.AddMonths(1);

            Console.WriteLine();
            Console.WriteLine("This keygen creates a license key for the IDoc Generator");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Example: keygen --name=\"Joe User\" --company=\"BigCo\" --email=\"joe.user@example.com\" expiry=\"{0:yyyy-MM-dd}\"", dateInOneMonth);
            Console.WriteLine();
            Console.WriteLine("This license key generator is © copyright MyLoadTest Pty Ltd, 2013."); // Try ALT-0169 for the copyright symbol
            Console.WriteLine("Please report any bugs to stuart@myloadtest.com");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        static int PackDate(DateTime date) {
            return ((date.Month - 1) << 10) | ((date.Day - 1) << 5) | (date.Year - 2010);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packedDate"></param>
        /// <returns></returns>
        static DateTime UnpackDate(int packedDate) {
            return new DateTime(2010 + (packedDate & 0x1F), 1 + (packedDate >> 10), 1 + ((packedDate >> 5) & 0x1F));
        }
        #endregion
    }
}


/*
 * DETERMINING LICENSE KEY SIZE
 * 
 * The BASE32X format stores 5 bits for each license key character (excluding separators), so the license key 
 * 3XTZWJ-V5N4JB-MXDBAA-K9CYK7-2XUWSU (5 groups of 6) can store 180 bits.
 * 

*/