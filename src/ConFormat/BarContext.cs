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
    private readonly StringBuilder _stringBuilder;
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
        _stringBuilder = new StringBuilder();
    }

    /// <summary>
    /// Writes the specified content immediately.
    /// </summary>
    /// <param name="contentFiller">Content.</param>
    /// <typeparam name="T">Content filler type.</typeparam>
    public void Write<T>(T contentFiller) where T : IContentFiller
    {
        _active = true;
        DrawInternal(contentFiller);
    }

    /// <summary>
    /// Updates with the specified content if the bar is due for an update based on the configured upate interval.
    /// </summary>
    /// <param name="contentFiller">Content.</param>
    /// <typeparam name="T">Content filler type.</typeparam>
    public void Update<T>(T contentFiller) where T : IContentFiller
    {
        _active = true;
        if (_stopwatch.Elapsed < _interval)
        {
            return;
        }
        DrawInternal(contentFiller);
    }

    /// <summary>
    /// Clears the bar output.
    /// </summary>
    public void Clear()
    {
        _active = true;
        DrawInternal(new BlankContentFiller());
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
        EndLine();
    }

    private void DrawInternal<T>(T contentFiller) where T : IContentFiller
    {
        _stopwatch.Restart();
        _stringBuilder.Clear();
        int widthRemaining = AvailableWidth;
        _stringBuilder.Append('\r');
        if (widthRemaining > 0)
        {
            contentFiller.Fill(_stringBuilder, widthRemaining);
        }
        DrawLine(_stringBuilder);
        _stringBuilder.Clear();
    }

    internal struct BlankContentFiller : IContentFiller
    {
        public void Fill(StringBuilder stringBuilder, int width)
        {
            StringFillUtil.PadRemaining(stringBuilder, width);
        }
    }

    /// <summary>
    /// Draws the content of the specified <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="stringBuilder"><see cref="StringBuilder"/> with the content to write.</param>
    protected virtual void DrawLine(StringBuilder stringBuilder)
    {
        foreach (var chunk in stringBuilder.GetChunks())
        {
            _output.Write(chunk.Span);
        }
    }

    /// <summary>
    /// Applies a newline to the output writer.
    /// </summary>
    protected virtual void EndLine()
    {
        _output.WriteLine();
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
