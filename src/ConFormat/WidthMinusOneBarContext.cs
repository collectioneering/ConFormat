namespace ConFormat;

/// <summary>
/// Default fallback implementation that avoids spillage by using one less than the available buffer width.
/// </summary>
public class WidthMinusOneBarContext : BarContext
{
    private bool _disposed;

    /// <inheritdoc />
    protected override int AvailableWidth => base.AvailableWidth - 1;

    /// <summary>
    /// Initializes an instance of <see cref="WidthMinusOneBarContext"/>.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns true).</param>
    /// <param name="interval">Update interval.</param>
    public WidthMinusOneBarContext(TextWriter output, Func<bool> redirectedFunc, Func<int> widthFunc, TimeSpan interval) : base(output, redirectedFunc, widthFunc, interval)
    {
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
    }
}
