using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using Microsoft.Win32.SafeHandles;

namespace ConFormat;

[SupportedOSPlatform("Windows5.1.2600")]
internal static class WindowsBarUtility
{
    public static void Enter(out SafeFileHandle wnd, out CONSOLE_MODE originalMode)
    {
        wnd = PInvoke.CreateFile("CONOUT$",
            (uint)(GENERIC_ACCESS_RIGHTS.GENERIC_READ | GENERIC_ACCESS_RIGHTS.GENERIC_WRITE),
            FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            default,
            null);
        if (!PInvoke.GetConsoleMode(wnd, out originalMode))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        var modMode = originalMode & ~CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT;
        if (!PInvoke.SetConsoleMode(wnd, modMode))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public static void Exit(SafeHandle wnd, CONSOLE_MODE originalMode)
    {
        if (!PInvoke.SetConsoleMode(wnd, originalMode))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
