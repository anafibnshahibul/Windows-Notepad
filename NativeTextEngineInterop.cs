using System.Runtime.InteropServices;
using System.Text;

namespace AdvancedNotepad.App;

public static class NativeTextEngineInterop
{
    private const string Dll = "NativeTextEngine.dll";

    [DllImport(Dll, CharSet = CharSet.Unicode)]
    private static extern IntPtr NTE_OpenMappedFile(string path, out long length, out int detectedEncoding);

    [DllImport(Dll)]
    private static extern IntPtr NTE_GetTextPointer(IntPtr handle, out int utf16Length);

    [DllImport(Dll)]
    private static extern void NTE_CloseFile(IntPtr handle);

    [DllImport(Dll, CharSet = CharSet.Unicode)]
    private static extern int NTE_RegexSearch(IntPtr handle, string pattern, bool caseSensitive,
        [Out] int[] matchOffsets, int maxMatches);

    public static (string content, Encoding enc) ReadLargeFile(string path)
    {
        var handle = NTE_OpenMappedFile(path, out long length, out int encFlag);
        try
        {
            var ptr = NTE_GetTextPointer(handle, out int utf16Len);
            string content = Marshal.PtrToStringUni(ptr, utf16Len);
            Encoding enc = encFlag switch
            {
                0 => new UTF8Encoding(false),
                1 => new UTF8Encoding(true),
                2 => Encoding.Unicode,
                _ => Encoding.GetEncoding(1252)
            };
            return (content, enc);
        }
        finally { NTE_CloseFile(handle); }
    }

    public static int[] RegexSearch(string path, string pattern, bool caseSensitive)
    {
        var handle = NTE_OpenMappedFile(path, out _, out _);
        try
        {
            var buffer = new int[10_000];
            int count = NTE_RegexSearch(handle, pattern, caseSensitive, buffer, buffer.Length);
            return buffer.Take(count).ToArray();
        }
        finally { NTE_CloseFile(handle); }
    }
}