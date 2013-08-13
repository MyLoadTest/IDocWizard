#include "stdafx.h"

#include "IDocReplayDLL.h"
#include "Utils.h"
#include "lrun.h" // LoadRunner functions (lr_)
#include "pugixml.hpp"  // http://code.google.com/p/pugixml/

#include <stdio.h>
#include <iostream>
#include <sstream>
#include <vector>
#include <string>
#include <regex>

using namespace pugi;
using namespace std;

bool g_licenseValid = false;  // Global variable. Note that this variable will retain its value for the lifetime of the
                              // DLL (it does not need to be declared "static").

// The input file contains XML values that can be extracted by idoc_eval_string().
std::string g_idocParamInputFilePath;
pugi::xml_document g_idocParamInputFile;

std::vector<std::string> g_allocatedStrings;

// IDoc parameters have the format: {IDoc:SegmentName:FieldName}
// Parameter names must contain only alphanumeric characters and underscores.
static const regex g_parameterRegex("\\{idoc\\:([^:}]*)\\:([^:}]*)\\}", regex_constants::icase);
static const regex g_parameterNameRegex("^[a-zA-Z_][a-zA-Z_0-9]*$");

// This section contains functions that are used internally by the DLL.
// These functions cannot be called from a VuGen script.
namespace
{
    /// @brief Determines if "parameter substitution" logging is enabled
    /// @details Runtime Settings > General > Log: Is the "parameter substitution" checkbox ticked?
    ///     Note that logging levels may be changed at runtime.
    /// @return true if the "parameter substitution" checkbox is ticked.
    /// @example
    ///     if (is_param_log_enabled()) { lr_output_message("my message about params"); }
    bool is_param_log_enabled()
    {
        unsigned int log_options = lr_get_debug_message();
        return (log_options & LR_MSG_CLASS_PARAMETERS) != 0;
    }

    /// @brief Determines if "advanced trace" logging is enabled
    /// @details Runtime Settings > General > Log: Is the "advanced trace" checkbox ticked?
    ///     Note that logging levels may be changed at runtime.
    /// @return true if the "advanced trace" checkbox is ticked.
    /// @example
    ///     if (is_trace_log_enabled()) { lr_output_message("my message about trace"); }
    bool is_trace_log_enabled()
    {
        unsigned int log_options = lr_get_debug_message();
        return (log_options & LR_MSG_CLASS_FULL_TRACE) != 0;
    }

    /// @brief Determines if the current license is valid
    /// @details Before any functions in idoc.dll are called, a license key must be set using the
    ///     idoc_set_license function.
    /// @return true if the current license key is valid
    bool ensure_valid_license()
    {
        if (!g_licenseValid)
        {
            lr_error_message("Valid license is not specified. Call idoc_set_license() first.");
            return false;
        }

        return true;
    }

    const char_t* get_license_property_value(const xml_node& rootNode, const char_t* propertyName)
    {
         const char_t* result = rootNode.child(propertyName).text().as_string();
         return result == NULL ? "" : result;
    }

    int count_element(const xml_node& node, const string& elementName)
    {
        if (elementName.empty())
        {
            return 0;
        }

        int result = 0;

        if (elementName.compare(node.name()) == 0)
        {
            result++;
        }

        for (xml_object_range<xml_node_iterator>::const_iterator it = node.children().begin();
            it != node.children().end();
            ++it)
        {
            result += count_element(*it, elementName);
        }

        return result;
    }

    const xml_node get_node_value(const xml_node& rootNode, const string& nodeName)
    {
        string query(".//");
        query += nodeName;

        return rootNode.select_single_node(query.c_str()).node();
    }
}

/// @brief Sets the license key to use for idoc.dll 
/// @details The idoc_create function will not work unless the license key has been set and is 
///     valid. If the license key is invalid, an error message will be written to the Replay log.
/// @param a string containing a license in XML format
/// @example
///     idoc_set_license(
///         "<license>"
///         "    <name>Joe User</name>"
///         "    <company>BigCo</company>"
///         "    <email>joe.user@example.com</email>"
///         "    <key>ANUAA-ADHHB-BS7VU-MVH45-9ZG3B-U3PUQ</key>"
///         "    <expires>2013-10-01</expires>"
///         "</license>");
/// @todo Add the license key functionality using the SoftActivate API. Note: I will do this last,
///     as it is non-essential functionality.
IDOCREPLAYDLL_API BOOL idoc_set_license(const LPCSTR licenseXml)
{
    g_licenseValid = false;

    if (licenseXml == NULL)
    {
        lr_error_message("[%s] License key cannot be NULL.", __FUNCTION__);
        return g_licenseValid;
    }

    if (is_trace_log_enabled())
    {
        lr_output_message("[%s] Checking license key:\n%s", __FUNCTION__, licenseXml);
    }

    using namespace pugi;

    xml_document doc;
    const xml_parse_result parseResult = doc.load(licenseXml);
    if (!parseResult)
    {
        lr_error_message("[%s] Invalid license XML (%s).", __FUNCTION__, parseResult.description());
        return g_licenseValid;
    }

    const xml_node& root = doc.child("license");
    const std::string name(get_license_property_value(root, "name"));
    const std::string company(get_license_property_value(root, "company"));
    const std::string email(get_license_property_value(root, "email"));
    const std::string key(get_license_property_value(root, "key"));
    const std::string expires(get_license_property_value(root, "expires"));

    // TODO: check that the license is valid. If it is valid, set license_valid to true;

    g_licenseValid = true;

    // TODO: add trace messages for "expired license" and "invalid license" (i.e. they have tampered with the fields)
    if (g_licenseValid && is_trace_log_enabled())
    {
        lr_output_message("License key is valid. Expiry date: %TODO");
    }

    // The license key functionality has been implemented using the SoftActivate SDK. Example code for C++ can be
    // found at http://www.softactivate.com/

    // Before checking the license key, you will have to define the license key template.
    // The template used for checking must match the template that was used to generate the key. This template can be
    // found in the the Keygen VS Project.

    return g_licenseValid;
}

/// @brief Select the input file from the Repository
/// @param the file name (including path) of the IDoc to use as input for any IDoc parameters 
/// @return true if the file exists and was selected successfully
IDOCREPLAYDLL_API BOOL idoc_select_input_file(const LPCSTR filePath)
{
    g_idocParamInputFilePath.erase();
    g_idocParamInputFile.reset();

    if (!ensure_valid_license())
    {
        return FALSE;
    }

    if (filePath == NULL)
    {
        lr_error_message("[%s] File path cannot be NULL.", __FUNCTION__);
        return FALSE;
    }

    if (!Utils::FileExists(filePath))
    {
        lr_error_message("[%s] File \"%s\" is not found.", __FUNCTION__, filePath);
        return FALSE;
    }

    using namespace pugi;

    const xml_parse_result parseResult = g_idocParamInputFile.load_file(filePath);
    if (!parseResult)
    {
        lr_error_message("[%s] Invalid input file XML (%s).", __FUNCTION__, parseResult.description());
        return FALSE;
    }

    g_idocParamInputFilePath = filePath;
    return TRUE;
}

/// @brief Replaces any IDoc parameters with a the specified value from the input IDoc. IDoc 
///     parameters should be in the format {IDoc:SegmentName:FieldName}
///     Note you must specify the input IDoc to use by first calling idoc_select_input_file
/// @param a new string with the IDoc parameters replaced with their values
/// @return a new string with the IDoc parameters replaced
IDOCREPLAYDLL_API LPCSTR idoc_eval_string(const LPCSTR parameterizedString)
{
    if (!ensure_valid_license())
    {
        return parameterizedString;
    }

    if (parameterizedString == NULL)
    {
        lr_error_message("Parameterized string cannot be empty.");
        return parameterizedString;
    }

    // TODO: move this check so that it only throws an error when parameterizedString contains something that looks like an IDoc parameter (i.e. {IDoc:xxx:yyy}
    // We don't want an error to be raised when idoc_create() is called with standard LoadRunner parameters.
    if (g_idocParamInputFilePath.empty())
    {
    //    lr_error_message("Input file is not selected. (Call idoc_select_input_file first.)");
        return parameterizedString;
    }

    g_allocatedStrings.push_back(std::string());

    string& result = g_allocatedStrings.back();

    const char* currentString = parameterizedString;

    cmatch matchResults;
    while (regex_search(currentString, matchResults, g_parameterRegex) && matchResults.size() == 3)
    {
        const cmatch::difference_type offset = matchResults.position();
        if (offset > 0)
        {
            result.append(currentString, offset);
            currentString += offset;
        }

        const sub_match<const char*> entireMatch = matchResults[0];
        const sub_match<const char*> segmentMatch = matchResults[1];
        const sub_match<const char*> fieldMatch = matchResults[2];

        const string segment(segmentMatch.first, segmentMatch.length());
        const string field(fieldMatch.first, fieldMatch.length());

        const xml_node segmentNode = get_node_value(g_idocParamInputFile.root(), segment);
        const xml_node fieldNode = get_node_value(segmentNode, field);

        if (segmentNode.empty() || fieldNode.empty())
        {
            result.append(currentString, entireMatch.length());
            currentString += entireMatch.length();

            if (is_param_log_enabled())
            {
                lr_output_message(
                    "Warning: [IDoc] Unable to find IDoc parameter '%s:%s'.",
                    segment.c_str(),
                    field.c_str());
            }

            continue;
        }

        const char_t* value = fieldNode.text().as_string();
        result.append(value);

        if (is_param_log_enabled())
        {
            lr_output_message(
                "[IDoc] Parameter substitution: '%s:%s' => '%s'.",
                segment.c_str(),
                field.c_str(),
                value);
        }

        currentString += entireMatch.length();
    }

    result.append(currentString);

    return result.c_str();
}

/// TODO
/// Note you must specify the input IDoc to use by first calling idoc_select_input_file
/// @param TODO
/// @return the number of occurrances of the specified element in the input IDoc
IDOCREPLAYDLL_API int idoc_count_element(const LPCSTR elementName)
{
    if (elementName == NULL)
    {
        lr_error_message("[%s] Element name cannot be NULL.", __FUNCTION__);
        return 0;
    }

    if (!ensure_valid_license())
    {
        return 0;
    }

    if (g_idocParamInputFilePath.empty())
    {
        lr_error_message("[%s] Input file is not selected. (Call idoc_select_input_file first.)", __FUNCTION__);
        return 0;
    }

    int result = count_element(g_idocParamInputFile.root(), std::string(elementName));
    return result;
}

/// @brief Creates an IDoc file based on an XML template and returns it as a string
/// @param a string containing the IDoc's XML template
/// @return a string containing a new flat-text IDoc
IDOCREPLAYDLL_API LPCSTR idoc_create_direct(const LPCSTR idocXml)
{
    if (idocXml == NULL)
    {
        lr_error_message("[%s] IDoc XML cannot be NULL.", __FUNCTION__);
        return FALSE;
    }

    if (!ensure_valid_license())
    {
        return NULL;
    }

    const LPCSTR idocXmlPartiallyProcessed = idoc_eval_string(idocXml);
    const char* idocXmlProcessed = lr_eval_string(idocXmlPartiallyProcessed);
    const char* idocXmlFinal = idocXmlProcessed == NULL ? idocXmlPartiallyProcessed : idocXmlProcessed;

    xml_document doc;
    doc.load(idocXmlFinal);
    if (doc.empty())
    {
        lr_error_message("[%s] The specified IDoc XML document is empty.", __FUNCTION__);
        return NULL;
    }

    const xpath_node_set segmentNodeSet = doc.root().select_nodes("//IDOC/*[@SEGMENT='1']");
    if (segmentNodeSet.empty())
    {
        lr_error_message("[%s] The specified IDoc XML is not a valid IDoc.", __FUNCTION__);
        return NULL;
    }

    stringstream resultStream;

    for (xpath_node_set::const_iterator segmentIterator = segmentNodeSet.begin();
        segmentIterator != segmentNodeSet.end();
        ++segmentIterator)
    {
        const xml_node segmentNode = segmentIterator->node();
        const xpath_node_set fieldNodeSet = segmentNode.select_nodes("./*");
        if (fieldNodeSet.empty())
        {
            lr_error_message(
                "[%s] The specified IDoc XML contains segment '%s' without fields.",
                __FUNCTION__,
                segmentNode.name());
            return NULL;
        }

        for (xpath_node_set::const_iterator fieldIterator = fieldNodeSet.begin();
            fieldIterator != fieldNodeSet.end();
            ++fieldIterator)
        {
            static const unsigned int InvalidLength = 0xFFFFFFFF;

            const xml_node fieldNode = fieldIterator->node();
            const unsigned int length = fieldNode.attribute("length").as_uint(InvalidLength);
            if (length == InvalidLength)
            {
                lr_error_message(
                    "[%s]: The specified IDoc XML contains field '%s:%s' without length or with invalid one.",
                    __FUNCTION__,
                    segmentNode.name(),
                    fieldNode.name());
                return NULL;
            }

            string fieldText(fieldNode.text().as_string());
            if (fieldText.length() > length)
            {
                lr_error_message(
                    "[%s] The specified IDoc XML contains field '%s:%s' which actual length (%u) is greater"
                        " than declared (%u).",
                    __FUNCTION__,
                    segmentNode.name(),
                    fieldNode.name(),
                    fieldText.length(),
                    length);
                return NULL;
            }

            const size_t padCount = length - fieldText.length();
            if (padCount > 0)
            {
                fieldText.append(padCount, ' ');
            }

            resultStream << fieldText;
        }

        resultStream << endl;
    }

    g_allocatedStrings.push_back(resultStream.str());
    const string& resultingDocument = g_allocatedStrings.back();

    return resultingDocument.c_str();
}

/// @brief Creates an IDoc file based on an XML template and saves it to a {parameter}
/// @param the name of the LoadRuner {parameter} to save the IDoc to
/// @param a string containing the IDoc's XML template
/// @return true if the IDoc was created successfully
IDOCREPLAYDLL_API BOOL idoc_create(const LPCSTR parameterName, const LPCSTR idocXml)
{
    if (parameterName == NULL)
    {
        lr_error_message("[%s] Parameter name cannot be NULL.", __FUNCTION__);
        return FALSE;
    }

    cmatch dummyMatch;
    const bool parameterValid = regex_match(parameterName, dummyMatch, g_parameterNameRegex);
    if (!parameterValid)
    {
        lr_error_message("[%s] Parameter name is invalid.", __FUNCTION__);
        return FALSE;
    }

    LPCSTR idoc = idoc_create_direct(idocXml);
    if (idoc == NULL)
    {
        return FALSE;
    }

    lr_save_string(idoc, parameterName);
    return TRUE;
}

/// @brief Creates an IDoc file based on an XML template that contains XPath expressions and saves
///     it to a {parameter}
/// @param the name of the LoadRuner {parameter} to save the IDoc to
/// @param a string containing the IDoc's XML template. This may contain XPath expressions
/// @return TRUE if the IDoc was created successfully; otherwise, FALSE.
/// @todo I haven't quite figured out how this should work, but it may be needed to handle the
///     situation where the output IDoc has a variable number of segments.
IDOCREPLAYDLL_API BOOL idoc_create_xpath(const LPCSTR parameterName, const LPCSTR idocXml)
{
    return TRUE;
}

/// @brief Frees the memory used by the library
IDOCREPLAYDLL_API void idoc_free_memory()
{
    g_allocatedStrings.clear();
}

/// @brief Saves a string (containing an IDoc) to the file system.
/// @param the file name (including path) to save the IDoc to.
/// @param the string containing the IDoc.
/// @return TRUE if the file was saved successfully; otherwise, FALSE.
IDOCREPLAYDLL_API BOOL idoc_save(LPCSTR filePath, LPCSTR idocString)
{
    if (filePath == NULL)
    {
        lr_error_message("[%s] File path cannot be NULL.", __FUNCTION__);
        return FALSE;
    }

    if (idocString == NULL)
    {
        lr_error_message("[%s] IDoc string cannot be NULL.", __FUNCTION__);
        return FALSE;
    }

    // any errors should print an error using lr_error_message() and return FALSE;
    // lr_eval_string() should be called on idoc_string, as it may contain LoadRunner {parameters}.

    return TRUE;
}