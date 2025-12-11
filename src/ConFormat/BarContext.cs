using System.Diagnostics;
using System.Text;

namespace ConFormat;

/// <summary>
/// Provides a context to manage a status bar.
/// </summary>
public abstract class BarContext : IDisposable
{
    /// <summary>
    /// Default update interval.
    /// </summary>
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(0.05f);

    /// <summary>
    /// Width available to the bar.
    /// </summary>
    protected virtual int AvailableWidth => _redirectedFunc() ? 40 : _widthFunc();

    private readonly TextWriter _output;
    private readonly Func<bool> _redirectedFunc;
    private readonly Func<int> _widthFunc;
    private readonly TimeSpan _interval;
    private readonly Stopwatch _stopwatch;
    private readonly BarState _state;
    private bool _active;

    /// <summary>
    /// Initializes an instance of <see cref="BarContext"/>.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns true).</param>
    /// <param name="interval">Update interval.</param>
    protected BarContext(TextWriter output, Func<bool> redirectedFunc, Func<int> widthFunc, TimeSpan interval)
    {
        _interval = interval;
        _output = output;
        _redirectedFunc = redirectedFunc;
        _widthFunc = widthFunc;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
        _state = new BarState(false);
    }

    /// <summary>
    /// Writes the specified content immediately.
    /// </summary>
    /// <param name="contentFiller">Content.</param>
    /// <param name="scrollIndex">Current scroll index for the content.</param>
    /// <typeparam name="T">Content filler type.</typeparam>
    public void Write<T>(ref T contentFiller, int scrollIndex = 0) where T : IContentFiller
    {
        _active = true;
        DrawInternal(ref contentFiller, scrollIndex);
    }

    /// <summary>
    /// Updates with the specified content if the bar is due for an update based on the configured update interval.
    /// </summary>
    /// <param name="contentFiller">Content.</param>
    /// <param name="scrollIndex">Current scroll index for the content.</param>
    /// <typeparam name="T">Content filler type.</typeparam>
    public void Update<T>(ref T contentFiller, int scrollIndex = 0) where T : IContentFiller
    {
        _active = true;
        if (_stopwatch.Elapsed < _interval)
        {
            return;
        }
        DrawInternal(ref contentFiller, scrollIndex);
    }

    /// <summary>
    /// Clears the bar output.
    /// </summary>
    public void Clear()
    {
        _active = true;
        var blank = new BlankContentFiller();
        DrawInternal(ref blank, 0);
        _output.Write('\r');
    }

    /// <summary>
    /// Terminates the output with a newline.
    /// </summary>
    public void End()
    {
        if (!_active)
        {
            return;
        }
        _active = false;
        _state.EndLine(_output);
    }

    private void DrawInternal<T>(ref T contentFiller, int scrollIndex) where T : IContentFiller
    {
        _stopwatch.Restart();
        _state.Draw(ref contentFiller, scrollIndex, AvailableWidth, _output);
    }

    internal struct BlankContentFiller : IContentFiller
    {
        public void Fill(StringBuilder stringBuilder, int width, int scrollIndex = 0)
        {
            StringFillUtil.PadRemaining(stringBuilder, width);
        }
    }

    /// <summary>
    /// Creates a default context applicable to the current operating system.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="forceFallback">If true, force fallback to a simple implementation.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns true).</param>
    /// <param name="interval">Update interval.</param>
    /// <returns></returns>
    public static BarContext Create(TextWriter output, bool forceFallback, Func<bool> redirectedFunc, Func<int> widthFunc, TimeSpan interval = default)
    {
        if (interval == default)
        {
            interval = DefaultInterval;
        }
        if (!forceFallback && OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
        {
            return new WindowsBarContext(output, redirectedFunc, widthFunc, interval);
        }
        return new WidthMinusOneBarContext(output, redirectedFunc, widthFunc, interval);
    }

    /// <summary>
    /// Disposes this instance.
    /// </summary>
    /// <param name="disposing">True if called from a normal <see cref="IDisposable.Dispose"/> call.</param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
