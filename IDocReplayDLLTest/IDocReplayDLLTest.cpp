#include "stdafx.h"

#include "..\IDocReplayDLL\IDocReplayDLL.h"
#include "..\IDocReplayDLL\Utils.h"
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

        ifstream fileStream(filePath);
        if (fileStream.bad())
        {
            return false;
        }

        istreambuf_iterator<char> it(fileStream.rdbuf());
        fileText.assign(it, istreambuf_iterator<char>());

        fileStream.close();

        return true;
    }
}

static const char* g_testInputFilePath = "IDocInput.xml";
static const char* g_testOutputFilePath = "IDocOutput.txt";
static const char* g_testSaveMethodFilePath = "TestSave.txt";

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
        wcout << L"*** Error setting the license." << endl;
        return 1;
    }

    const bool selectInputFileResult = !!idoc_select_input_file(g_testInputFilePath);
    if (!selectInputFileResult)
    {
        wcout << L"*** Error setting the input file." << endl;
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
        wcout << L"*** Failed to read the test file '" << g_testInputFilePath << "'." << endl;
        return -1;
    }
    if (!ReadFileText(g_testOutputFilePath, idocOutputFileText))
    {
        wcout << L"*** Failed to read the test file '" << g_testOutputFilePath << "'." << endl;
        return -1;
    }

    const LPCSTR createDirectResult = idoc_create_direct(idocInputFileText.c_str());
    if (createDirectResult == NULL || idocOutputFileText.compare(createDirectResult) != 0)
    {
        wcout << L"*** idoc_create_direct() test has failed." << endl;
        return 6;
    }

    if (!Utils::DeleteFile(g_testSaveMethodFilePath))
    {
        wcout << L"*** Unable to delete the test output file '" << g_testSaveMethodFilePath << "'." << endl;
        return -1;
    }

    const string testStringValue("1-{Test}-2");
    const bool saveResult = !!idoc_save(g_testSaveMethodFilePath, testStringValue.c_str());
    if (!saveResult)
    {
        wcout << L"*** idoc_save() test has failed." << endl;
        return 7;
    }

    string actualStringValue;
    if (!ReadFileText(g_testSaveMethodFilePath, actualStringValue))
    {
        wcout << L"*** Failed to read the test output file '" << g_testSaveMethodFilePath << "'." << endl;
        return -1;
    }

    if (actualStringValue.compare(testStringValue.c_str()) != 0)
    {
        wcout << L"*** idoc_save() test has failed." << endl;
        return 7;
    }

    wcout << L"* Test passed." << endl << endl;
    return 0;
}