namespace ConFormat;

/// <summary>
/// Default fallback implementation that avoids spillage by using one less than the available buffer width.
/// </summary>
public class WidthMinusOneMultiBarContext<TKey> : MultiBarContext<TKey> where TKey : IEquatable<TKey>
{
    private bool _disposed;

    /// <inheritdoc />
    protected override int AvailableWidth => base.AvailableWidth - 1;

    /// <summary>
    /// Initializes an instance of <see cref="WidthMinusOneBarContext"/>.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns false).</param>
    /// <param name="heightFunc">Function that returns the terminal viewport height (only needs to return a valid value when <paramref name="redirectedFunc"/> returns false).</param>
    /// <param name="initialRow">The 0-indexed row of the cursor to start with.</param>
    /// <param name="interval">Update interval.</param>
    public WidthMinusOneMultiBarContext(
        TextWriter output,
        Func<bool> redirectedFunc,
        Func<int> widthFunc,
        Func<int> heightFunc,
        int initialRow,
        TimeSpan interval)
        : base(output, redirectedFunc, widthFunc, heightFunc, initialRow, interval)
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
