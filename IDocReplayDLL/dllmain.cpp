#include "stdafx.h"

#include "IDocReplayDLL.h"

BOOL APIENTRY DllMain(HMODULE /*hModule*/, DWORD dwReason, LPVOID /*lpReserved*/)
{
    switch (dwReason)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        idoc_free_memory();
        break;

    case DLL_PROCESS_DETACH:
        break;
    }

    return TRUE;
}