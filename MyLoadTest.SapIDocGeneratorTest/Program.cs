//// copyright Stuart Moncrieff
//// Blah blah blah

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyLoadTest.CommandLine;
using MyLoadTest.SapIDocGenerator;

namespace MyLoadTest.SapIDocGeneratorTest
{
    /// <summary>
    /// This program converts a flat-text IDoc file (and it's C-Header definition) into an XML-formatted IDoc and a VuGen Action.c file.
    ///
    /// To run the program:
    ///     test --idoc=".\testdata\idoc1.txt" --header=".\testdata\idoc1.h"
    ///
    /// </summary>
    internal static class Program
    {
        #region Fields

        private const string SourceFile = "Action.c"; // Action.c file (output)

        private static string _idocFile; // Flat-text IDoc file (input)
        private static string _headerFile; // Exported C-Header definition of the IDoc (input)
        private static string _xmlFile; // IDoc in XML format (output)
        private static bool _showHelp;

        #endregion

        #region Application Entry Point

        private static void Main(string[] args)
        {
            var p = new OptionSet
            {
                { "idoc=", "the flat-text IDoc file", v => _idocFile = v }, // Mandatory
                { "header=", "the exported C-header for the IDoc (from WE60)", v => _headerFile = v }, // Mandatory
                { "?|help", "display this help and exit", v => _showHelp = true }, // Optional
            };

            // Process the command-line arguments
            List<string> options;
            try
            {
                options = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("test: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `test --help' for more information.");
                return;
            }

            foreach (var option in options)
            {
                Console.WriteLine("Option: {0}", option);
            }

            // Command line option "--help"
            if (_showHelp)
            {
                ShowHelp(p);
                return;
            }

            // Check that the mandatory input arguments were specified.
            if (string.IsNullOrEmpty(_idocFile) || string.IsNullOrEmpty(_headerFile))
            {
                Console.WriteLine("You did not specify all the arguments. Try `test --help' for more information.");
                return;
            }

            // Check that both of the input files exist.
            if (!File.Exists(_idocFile) || !File.Exists(_headerFile))
            {
                Console.WriteLine("One of the input files does not exist. Please double check the file name and path.");
                return;
            }

            // Read the IDoc and IDoc Definition
            Console.WriteLine("Reading IDoc Definition from C-header file: {0}", _headerFile);
            var def = SapIDocDefinition.LoadHeader(_headerFile);
            Console.WriteLine("Reading IDoc data text file: {0}", _idocFile);
            var idocContents = File.ReadAllText(_idocFile);

            var idoc = new SapIDoc(def, idocContents);

            //// ==========================================
            //// Example for spec:

            ////// Import IDoc Definition from C header file (*.h)
            ////string headerFileName = "example.h";
            ////SapIDocDefinition def = SapIDocDefinition.LoadHeader(headerFileName);
            ////Console.WriteLine("IDoc type: {0}", def.Name);

            ////// Import IDoc file
            ////string idocFileName = "example.txt";
            ////string idocTxt = File.ReadAllText(idocFileName); ;
            ////IDoc idoc = new IDoc(def, idocTxt);
            ////Console.WriteLine("IDoc Number: {0}", idoc.Number);

            ////// Output Action.c file
            ////Console.WriteLine("Action.c: {0}", idoc.GetVuGenActionContents());

            //// ==========================================

            ////// Import IDoc Definition from C header file (*.h)
            ////string headerFileName = "example.h";
            ////SapIDocDefinition def = SapIDocDefinition.LoadHeader(headerFileName);

            ////// Bulk Import IDoc files and save them as XML
            ////foreach (string file in files) {
            ////    string idocTxt = File.ReadAllText(idocFileName); ;
            ////    IDoc idoc = new IDoc(def, idocTxt);
            ////    Console.WriteLine("IDoc Number: {0}", idoc.Number);
            ////    string path = repositoryPath + "\\" + idoc.Type + "\\" + idoc.Number + ".xml";
            ////    using (StreamWriter writer = new StreamWriter(filepath)) {
            ////        writer.WriteLine(idoc.GetXml.ToString());
            ////    }
            ////}

            // Output Action.c file
            var generatedActions = idoc.GetVuGenActionContents();
            Console.WriteLine("Action.c: {0}", generatedActions.MainActionContents);

            // Save IDoc for input parameter
            Console.WriteLine("IDoc XML to save for Parameter input: {0}", idoc.GetXml());

            // Create output files
            _xmlFile = idoc.Number + ".xml";
            Console.WriteLine("Creating XML IDoc output file: {0}", _xmlFile);
            File.WriteAllText(_xmlFile, idoc.GetXml().ToString());
            Console.WriteLine("Creating VuGen output file: {0}", SourceFile);
            File.WriteAllText(SourceFile, generatedActions.MainActionContents);
            ////Console.WriteLine(idoc.GetXml().ToString());
            ////Console.WriteLine(idoc.GetVuGenActionContents());

            Console.Write("finished. Press any key to continue . . . ");
            Console.ReadKey(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Print usage instructions for the Test program
        /// </summary>
        /// <param name="p">The OptionSet that is used by the command-line program.</param>
        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine();
            Console.WriteLine("This test program converts a flat-text IDoc file (and it's C-Header definition) into an XML-formatted IDoc and a VuGen Action.c file.");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Example: test --idoc=\"idoc.txt\" --header=\"idoc.h\"");
            Console.WriteLine();
            Console.WriteLine("Please report any bugs to stuart@myloadtest.com");
        }

        #endregion
    }
}