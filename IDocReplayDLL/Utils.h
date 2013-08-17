#pragma once

#include <windows.h>

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

    bool DeleteFile(const LPCSTR filePath)
    {
        if (!Utils::FileExists(filePath))
        {
            return true;
        }

        if (!::SetFileAttributesA(filePath, FILE_ATTRIBUTE_NORMAL))
        {
            return false;
        }

        return !!::DeleteFileA(filePath);
    }
}