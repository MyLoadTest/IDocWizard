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

static const regex g_parameterRegex("\\{idoc\\:([^:}]*)\\:([^:}]*)\\}", regex_constants::icase);
static const regex g_parameterNameRegex("^[a-zA-Z_][a-zA-Z_0-9]*$");

namespace
{
    // Returns true if the "Parameter substitution" checkbox is selected in the VuGen script Runtime Settings.
    // Use it like this:
    //      if (is_param_log_enabled()) { lr_output_message("my message about params"); }
    bool is_param_log_enabled()
    {
        unsigned int log_options = lr_get_debug_message();
        return (log_options & LR_MSG_CLASS_PARAMETERS) != 0;
    }

    // Returns true if the "Advanced trace" checkbox is selected in the VuGen script Runtime Settings.
    // Use it like this:
    //      if (is_trace_log_enabled()) { lr_output_message("my message about trace"); }
    bool is_trace_log_enabled()
    {
        unsigned int log_options = lr_get_debug_message();
        return (log_options & LR_MSG_CLASS_FULL_TRACE) != 0;
    }

    bool ensure_valid_license()
    {
        if (!g_licenseValid)
        {
            lr_error_message(
                "[%s] ERROR: Valid license is not specified. Call idoc_set_license() first.",
                __FUNCTION__);
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

// TODO (will do this last, as it is non-essential functionality.
IDOCREPLAYDLL_API BOOL idoc_set_license(const LPCSTR licenseXml)
{
    g_licenseValid = false;

    if (licenseXml == NULL)
    {
        lr_error_message("[%s] License cannot be NULL.", __FUNCTION__);
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
        lr_output_message("[%s] License key is valid. Expiry date: %TODO", __FUNCTION__);
    }

    // The license key functionality has been implemented using the SoftActivate SDK. Example code for C++ can be
    // found at http://www.softactivate.com/

    // Before checking the license key, you will have to define the license key template.
    // The template used for checking must match the template that was used to generate the key. This template can be
    // found in the the Keygen VS Project.

    return g_licenseValid;
}

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

IDOCREPLAYDLL_API LPCSTR idoc_eval_string(const LPCSTR parameterizedString)
{
    if (!ensure_valid_license())
    {
        return parameterizedString;
    }

    if (parameterizedString == NULL)
    {
        lr_error_message("[%s] ERROR: Parameterized string cannot be NULL.", __FUNCTION__);
        return parameterizedString;
    }

    // TODO: move this check so that it only throws an error when parameterizedString contains something that looks like an IDoc parameter (i.e. {IDoc:xxx:yyy}
    // We don't want an error to be raised when idoc_create() is called with standard LoadRunner parameters.
    if (g_idocParamInputFilePath.empty())
    {
    //    lr_error_message("[%s] ERROR: Input file is not selected. (Call idoc_select_input_file first.)", __FUNCTION__);
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

IDOCREPLAYDLL_API int idoc_count_element(const LPCSTR elementName)
{
    if (elementName == NULL)
    {
        lr_error_message("[%s] ERROR: Element name cannot be NULL.", __FUNCTION__);
        return 0;
    }

    if (!ensure_valid_license())
    {
        return 0;
    }

    if (g_idocParamInputFilePath.empty())
    {
        lr_error_message("[%s] ERROR: Input file is not selected. (Call idoc_select_input_file first.)", __FUNCTION__);
        return 0;
    }

    int result = count_element(g_idocParamInputFile.root(), std::string(elementName));
    return result;
}

IDOCREPLAYDLL_API LPCSTR idoc_create_direct(const LPCSTR idocXml)
{
    if (idocXml == NULL)
    {
        lr_error_message("[%s] ERROR: IDoc XML cannot be NULL.", __FUNCTION__);
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
        lr_error_message("[%s] ERROR: The specified IDoc XML document is empty.", __FUNCTION__);
        return NULL;
    }

    const xpath_node_set segmentNodeSet = doc.root().select_nodes("//IDOC/*[@SEGMENT='1']");
    if (segmentNodeSet.empty())
    {
        lr_error_message("[%s] ERROR: The specified IDoc XML is not a valid IDoc.", __FUNCTION__);
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
                "[%s] ERROR: The specified IDoc XML contains segment '%s' without fields.",
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
                    "[%s] ERROR: The specified IDoc XML contains field '%s:%s' without length or with invalid one.",
                    __FUNCTION__,
                    segmentNode.name(),
                    fieldNode.name());
                return NULL;
            }

            string fieldText(fieldNode.text().as_string());
            if (fieldText.length() > length)
            {
                lr_error_message(
                    "[%s] ERROR: The specified IDoc XML contains field '%s:%s' which actual length (%u) is greater"
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

IDOCREPLAYDLL_API BOOL idoc_create(const LPCSTR parameterName, const LPCSTR idocXml)
{
    if (parameterName == NULL)
    {
        lr_error_message("[%s] ERROR: Parameter name cannot be NULL.", __FUNCTION__);
        return FALSE;
    }

    cmatch dummyMatch;
    const bool parameterValid = regex_match(parameterName, dummyMatch, g_parameterNameRegex);
    if (!parameterValid)
    {
        lr_error_message("[%s] ERROR: Parameter name is invalid.", __FUNCTION__);
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

IDOCREPLAYDLL_API void idoc_free_memory()
{
    g_allocatedStrings.clear();
}

/// Saves a string (containing an IDoc) to the file system.
/// @param the file name (including path) to save the IDoc to.
/// @param the string containing the IDoc.
/// @return true if the file was saved successfully.
IDOCREPLAYDLL_API BOOL idoc_save(LPCSTR filePath, LPCSTR idoc_string)
{
	// any errors should print an error using lr_error_message() and return FALSE;
	// lr_eval_string() should be called on idoc_string, as it may contain LoadRunner {parameters}.

	return TRUE;
}