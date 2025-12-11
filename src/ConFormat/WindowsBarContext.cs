using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;

namespace ConFormat;

/// <summary>
/// Provides a Windows-specific bar context.
/// </summary>
[SupportedOSPlatform("Windows5.1.2600")]
public class WindowsBarContext : BarContext
{
    private bool _disposed;
    private readonly SafeHandle _wnd;
    private readonly CONSOLE_MODE _originalMode;

    /// <summary>
    /// Initializes an instance of <see cref="WindowsBarContext"/>.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns true).</param>
    /// <param name="interval">Update interval.</param>
    /// <exception cref="Win32Exception">Thrown for internal Win32 errors.</exception>
    public WindowsBarContext(TextWriter output, Func<bool> redirectedFunc, Func<int> widthFunc, TimeSpan interval) : base(output, redirectedFunc, widthFunc, interval)
    {
        _wnd = PInvoke.CreateFile("CONOUT$",
            (uint)(GENERIC_ACCESS_RIGHTS.GENERIC_READ | GENERIC_ACCESS_RIGHTS.GENERIC_WRITE),
            FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            default,
            null);
        if (!PInvoke.GetConsoleMode(_wnd, out _originalMode))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        var modMode = _originalMode & ~CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT;
        if (!PInvoke.SetConsoleMode(_wnd, modMode))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        try
        {
            if (!PInvoke.SetConsoleMode(_wnd, _originalMode))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        finally
        {
            if (disposing)
            {
                _wnd.Dispose();
            }
        }
    }
}
