#include "stdafx.h"

#include "..\IDocReplayDLL\IDocReplayDLL.h"
#include <iostream>
#include <fstream>
#include <streambuf>

#include <string>
#include <regex>

namespace
{
    struct IDocMemoryManager
    {
        IDocMemoryManager() {};
        virtual ~IDocMemoryManager() { idoc_free_memory(); }
    };

    const bool ReadFileText(const char* filePath, std::string& fileText)
    {
        using namespace std;

        fileText.erase();

        const ifstream fileStream(filePath);
        if (fileStream.bad())
        {
            return false;
        }

        istreambuf_iterator<char> it(fileStream.rdbuf());
        fileText.assign(it, istreambuf_iterator<char>());

        return true;
    }
}

static const char* g_testInputFilePath = "IDocInput.xml";
static const char* g_testOutputFilePath = "IDocOutput.txt";

int _tmain()
{
    using namespace std;

    IDocMemoryManager idocMemoryManager;

    const bool setLicenseResult = !!idoc_set_license(
        "<license>"
        "    <name>Joe User</name>"
        "    <company>BigCo</company>"
        "    <email>joe.user@example.com</email>"
        "    <key>ANUAA-ADHHB-BS7VU-MVH45-9ZG3B-U3PUQ</key>"
        "    <expires>2013-10-01</expires>"
        "</license>");

    if (!setLicenseResult)
    {
        wcout << L"*** Error setting license." << endl;
        return 1;
    }

    const bool selectInputFileResult = !!idoc_select_input_file(g_testInputFilePath);
    if (!selectInputFileResult)
    {
        wcout << L"*** Error setting input file." << endl;
        return 2;
    }

    const int docNumCount = idoc_count_element("DOCNUM");
    if (docNumCount != 2)
    {
        wcout << L"*** idoc_count_element() test has failed." << endl;
        return 3;
    }

    const int rootNodeCount = idoc_count_element("ZISUPOD_ZBAPIPUBLISH02");
    if (rootNodeCount != 1)
    {
        wcout << L"*** idoc_count_element() test has failed." << endl;
        return 4;
    }

    LPCSTR evalResult = idoc_eval_string(
        "abcdef{IDoc:EDI_DC40_U:MANDT}ghijkl{idoC:Z2BP_HEADER000:DOCNUM}mno{IDoc:XXX:YYY}p");
    const string strEvalResult(evalResult ? evalResult : "");
    if (strEvalResult.compare("abcdef100ghijkl0000000062041994mno{IDoc:XXX:YYY}p") != 0)
    {
        wcout << L"*** idoc_eval_string() test has failed." << endl;
        return 5;
    }

    string idocInputFileText, idocOutputFileText;
    if (!ReadFileText(g_testInputFilePath, idocInputFileText))
    {
        wcout << L"*** Failed to read test file '" << g_testInputFilePath << "'." << endl;
        return -1;
    }
    if (!ReadFileText(g_testOutputFilePath, idocOutputFileText))
    {
        wcout << L"*** Failed to read test file '" << g_testOutputFilePath << "'." << endl;
        return -1;
    }

    const LPCSTR createDirectResult = idoc_create_direct(idocInputFileText.c_str());
    if (createDirectResult == NULL || idocOutputFileText.compare(createDirectResult) != 0)
    {
        wcout << L"*** idoc_create_direct() test has failed." << endl;
        return 6;
    }

    wcout << L"* Test passed." << endl << endl;
    return 0;
}