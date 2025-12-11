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
public class WindowsMultiBarContext<T> : MultiBarContext<T> where T : IEquatable<T>
{
    private bool _disposed;
    private SafeFileHandle _wnd;
    private CONSOLE_MODE _originalMode;

    /// <summary>
    /// Initializes an instance of <see cref="WindowsBarContext"/>.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns false).</param>
    /// <param name="heightFunc">Function that returns the terminal viewport height (only needs to return a valid value when <paramref name="redirectedFunc"/> returns false).</param>
    /// <param name="initialRow">The 0-indexed row of the cursor to start with.</param>
    /// <param name="interval">Update interval.</param>
    /// <exception cref="Win32Exception">Thrown for internal Win32 errors.</exception>
    public WindowsMultiBarContext(TextWriter output, Func<bool> redirectedFunc,
        Func<int> widthFunc,
        Func<int> heightFunc,
        int initialRow,
        TimeSpan interval)
        : base(output, redirectedFunc, widthFunc, heightFunc, initialRow, interval)
    {
        WindowsBarUtility.Enter(out _wnd, out _originalMode);
    }

    /// <inheritdoc />
    protected override void AdvanceLine()
    {
        WindowsBarUtility.Exit(_wnd, _originalMode);
        base.AdvanceLine();
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
