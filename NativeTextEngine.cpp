#include <windows.h>
#include <string>
#include <vector>
#include <regex>

extern "C" {

struct MappedFile {
    HANDLE hFile = nullptr, hMap = nullptr;
    LPVOID view = nullptr;
    std::wstring utf16Cache;
};

__declspec(dllexport) void* NTE_OpenMappedFile(const wchar_t* path, long long* outLength, int* outEncoding) {
    auto* mf = new MappedFile();
    mf->hFile = CreateFileW(path, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (mf->hFile == INVALID_HANDLE_VALUE) { delete mf; return nullptr; }

    LARGE_INTEGER size; GetFileSizeEx(mf->hFile, &size);
    *outLength = size.QuadPart;

    mf->hMap = CreateFileMappingW(mf->hFile, nullptr, PAGE_READONLY, 0, 0, nullptr);
    mf->view = MapViewOfFile(mf->hMap, FILE_MAP_READ, 0, 0, 0);

    // BOM sniff
    auto* bytes = static_cast<unsigned char*>(mf->view);
    if (size.QuadPart >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) *outEncoding = 1;
    else if (size.QuadPart >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) *outEncoding = 2;
    else *outEncoding = 0; // assume UTF-8 no BOM; fallback ANSI handled in managed layer

    int wlen = MultiByteToWideChar(CP_UTF8, 0, reinterpret_cast<char*>(bytes), (int)size.QuadPart, nullptr, 0);
    mf->utf16Cache.resize(wlen);
    MultiByteToWideChar(CP_UTF8, 0, reinterpret_cast<char*>(bytes), (int)size.QuadPart, mf->utf16Cache.data(), wlen);

    return mf;
}

__declspec(dllexport) const wchar_t* NTE_GetTextPointer(void* handle, int* outLen) {
    auto* mf = static_cast<MappedFile*>(handle);
    *outLen = static_cast<int>(mf->utf16Cache.size());
    return mf->utf16Cache.c_str();
}

__declspec(dllexport) int NTE_RegexSearch(void* handle, const wchar_t* pattern, bool caseSensitive,
                                           int* matchOffsets, int maxMatches) {
    auto* mf = static_cast<MappedFile*>(handle);
    auto flags = std::regex::ECMAScript | (caseSensitive ? std::regex::flag_type(0) : std::regex::icase);
    try {
        std::wregex re(pattern, flags);
        int count = 0;
        auto begin = std::wsregex_iterator(mf->utf16Cache.begin(), mf->utf16Cache.end(), re);
        auto end = std::wsregex_iterator();
        for (auto it = begin; it != end && count < maxMatches; ++it, ++count)
            matchOffsets[count] = static_cast<int>(it->position());
        return count;
    } catch (...) { return 0; }
}

__declspec(dllexport) void NTE_CloseFile(void* handle) {
    auto* mf = static_cast<MappedFile*>(handle);
    if (mf->view) UnmapViewOfFile(mf->view);
    if (mf->hMap) CloseHandle(mf->hMap);
    if (mf->hFile) CloseHandle(mf->hFile);
    delete mf;
}

} // extern "C"