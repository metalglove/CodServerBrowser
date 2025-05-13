using System.Runtime.InteropServices;

namespace CodServerBrowser.Core.Services;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct Map_t
{
    public IntPtr NameAddress;
    public int Id;
    public int Unk;
}
