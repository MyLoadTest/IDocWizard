#pragma once

#include "windows.h"

#ifdef IDOCREPLAYDLL_EXPORTS
#define IDOCREPLAYDLL_API extern "C" __declspec(dllexport)
#else
#define IDOCREPLAYDLL_API extern "C" __declspec(dllimport)
#endif

IDOCREPLAYDLL_API BOOL idoc_set_license(const LPCSTR licenseXml);

IDOCREPLAYDLL_API BOOL idoc_select_input_file(const LPCSTR filePath);

IDOCREPLAYDLL_API LPCSTR idoc_eval_string(const LPCSTR parameterizedString);

IDOCREPLAYDLL_API int idoc_count_element(const LPCSTR elementName);

IDOCREPLAYDLL_API LPCSTR idoc_create_direct(const LPCSTR idocXml);

IDOCREPLAYDLL_API BOOL idoc_create(const LPCSTR parameterName, const LPCSTR idocXml);

IDOCREPLAYDLL_API BOOL idoc_create_xpath(const LPCSTR parameterName, const LPCSTR idocXml);

IDOCREPLAYDLL_API void idoc_free_memory();

IDOCREPLAYDLL_API BOOL idoc_save(LPCSTR filePath, LPCSTR idocString);