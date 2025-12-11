using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using Microsoft.Win32.SafeHandles;

namespace ConFormat;

/// <summary>
/// Provides a Windows-specific bar context.
/// </summary>
[SupportedOSPlatform("Windows5.1.2600")]
public class WindowsBarContext : BarContext
{
    private bool _disposed;
    private readonly SafeFileHandle _wnd;
    private readonly CONSOLE_MODE _originalMode;

    /// <summary>
    /// Initializes an instance of <see cref="WindowsBarContext"/>.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns false).</param>
    /// <param name="interval">Update interval.</param>
    /// <exception cref="Win32Exception">Thrown for internal Win32 errors.</exception>
    public WindowsBarContext(TextWriter output, Func<bool> redirectedFunc, Func<int> widthFunc, TimeSpan interval) : base(output, redirectedFunc, widthFunc, interval)
    {
        WindowsBarUtility.Enter(out _wnd, out _originalMode);
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
            WindowsBarUtility.Exit(_wnd, _originalMode);
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
