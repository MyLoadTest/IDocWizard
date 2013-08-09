IDoc Wizard
===========
The IDoc Wizard allows performance testers to easily generate a large number of SAP IDoc files using 
VuGen/LoadRunner. It will help testers run load/volume tests by allowing them to:
*   Create an IDoc Template script to use for bulk generation of IDocs.
*   Parameterise fields within the IDoc Template to be replaced with standard LoadRunner parameters
    (datetime, unique numbers, values from CSV files, etc.).
*   Parameterise fields within the IDoc Template to be replaced with values from other IDocs. E.g. 
    when bulk generating “Confirm Purchase Order” IDocs, the generated IDocs must contain the order
    number from the "Purchase Order" IDocs that have already been created by the SAP system.
*   Manage the input IDocs that will be used for Parameter source data.

Product Page: http://www.myloadtest.com/tools/idoc-wizard/

GitHub Project: https://github.com/StuartMoncrieff/IDocWizard/

High-Level Design
-----------------
There are three main parts to the IDoc Wizard:

1.  Replay DLL (C++)
    *   Contains functions that are called by the VuGen script at runtime
    *   Replaces parameterised values in the IDoc Template
    *   If an input IDoc was used to provide the parameter values, it moves it to a "used" 
        directory (so duplicates will not be created).
    *   Converts the IDoc Template XML into a flat-text IDoc file
    *   Saves the generated IDoc file
2.  Generator DLL (C#)
    *   Is used by VuGen during script development
    *   Generates a VuGen script from an input IDoc (and its field definition file)
    *   Converts IDocs to XML, so they can be easily used as source data for IDoc Parameters.
3.  GUI + Data Management (C#)
    *   Is used with/by VuGen during script development
    *   User interface for generating a VuGen script from an input IDoc
    *   User interface for replacing IDoc fields in the Template with Parameters
    *   User interface for importing IDocs to be used as source data for Parameters

Visual Studio Projects
----------------------

The following Visual Studio projects create the various components of the IDoc Wizard:

*   IDocReplayDLL
*   IDocReplayDLLTest
*   KeyGen
*   MyLoadTest.Common
*   MyLoadTest.SapIDocGenerator
*   MyLoadTest.SapIDocGenerator.UI.Addin
*   MyLoadTest.SapIDocGenerator.UI.Common
*   MyLoadTest.SapIDocGeneratorTest

LoadRunner Functions
--------------------

The following functions may be called from a VuGen script:

*   idoc_set_license

        /// Sets the license key to use. The idoc_create function will not work unless the license
        /// key has been set and is valid.
        /// @param a string containing a license in XML format
        /// @return true if the license if valid
        BOOL idoc_set_license(const LPCSTR licenseXml)
        
*   idoc_select_input_file

        /// Select the input file from the Repository
        /// @param the file name (including path) of the IDoc to use as input for any IDoc
        /// parameters 
        /// @return true if the file exists and was selected successfully
        BOOL idoc_select_input_file(const LPCSTR filePath)

*   idoc_create

        /// Creates an IDoc file based on an XML template and saves it to a {parameter}
        /// @param the name of the LoadRuner {parameter} to save the IDoc to
        /// @param a string containing the IDoc's XML template
        /// @return true if the IDoc was created successfully
        BOOL idoc_create(const LPCSTR parameterName, const LPCSTR idocXml)

*   idoc_create_direct

        /// Creates an IDoc file based on an XML template and returns it as a string
        /// @param a string containing the IDoc's XML template
        /// @return a string containing a new flat-text IDoc
        LPCSTR idoc_create_direct(const LPCSTR idocXml)

*   idoc_eval_string
        
        /// Replaces any IDoc parameters with a the specified value from the input IDoc.
        /// IDoc parameters should be in the format {IDoc:SegmentName:FieldName}
        /// Note you must specify the input IDoc to use by first calling idoc_select_input_file
        /// @param a new string with the IDoc parameters replaced with their values
        /// @return a new string with the IDoc parameters replaced
        LPCSTR idoc_eval_string(const LPCSTR parameterizedString)

*   idoc_count_element

        /// TODO
        /// Note you must specify the input IDoc to use by first calling idoc_select_input_file
        /// @param TODO
        /// @return the number of occurrances of the specified element in the input IDoc
        int idoc_count_element(const LPCSTR elementName)

*   idoc_save

        /// Saves a string (containing an IDoc) to the file system.
        /// @param the file name (including path) to save the IDoc to.
        /// @param the string containing the IDoc.
        /// @return true if the file was saved successfully.
        BOOL idoc_save(LPCSTR filePath, LPCSTR idoc_string)

*   idoc_free_memory

        // Frees the memory used by ??
        void idoc_free_memory()

The IDoc Format
---------------
TODO


