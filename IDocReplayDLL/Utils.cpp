#include "stdafx.h"

#include "Utils.h"

namespace Utils
{
    bool FileExists(const LPCSTR filePath)
    {
        if (filePath == NULL)
        {
            return false;
        }

        const DWORD attributes = ::GetFileAttributesA(filePath);
        return attributes != INVALID_FILE_ATTRIBUTES && (attributes & FILE_ATTRIBUTE_DIRECTORY) == 0;
    }
}