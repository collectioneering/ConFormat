using System.Diagnostics;
using System.Text;

namespace ConFormat;

/// <summary>
/// Provides a context to manage a status bar.
/// </summary>
public abstract class MultiBarContext<TKey> : IDisposable where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Width available to the bar.
    /// </summary>
    protected virtual int AvailableWidth => _redirectedFunc() ? 40 : _widthFunc();

    private readonly object _lock = new();
    private readonly TextWriter _output;
    private readonly Func<bool> _redirectedFunc;
    private readonly Func<int> _widthFunc;
    private readonly Func<int> _heightFunc;
    private readonly int _initialRow;
    private readonly TimeSpan _interval;
    private readonly Dictionary<TKey, int> _keysToIndices = new();
    private readonly List<BarEntry> _entries = [];
    private readonly BarState _clearBar = new(false);

    private record BarEntry(BarState State, Stopwatch Stopwatch, TKey Key);

    /// <summary>
    /// Initializes an instance of <see cref="BarContext"/>.
    /// </summary>
    /// <param name="output">Output writer.</param>
    /// <param name="redirectedFunc">Function that returns true if the output is redirected.</param>
    /// <param name="widthFunc">Function that returns the terminal width (only needs to return a valid value when <paramref name="redirectedFunc"/> returns true).</param>
    /// <param name="heightFunc">TODO</param>
    /// <param name="initialRowFunc">TODO</param>
    /// <param name="interval">Update interval.</param>
    protected MultiBarContext(
        TextWriter output,
        Func<bool> redirectedFunc,
        Func<int> widthFunc,
        Func<int> heightFunc,
        Func<int> initialRowFunc,
        TimeSpan interval)
    {
        _interval = interval;
        _output = output;
        _redirectedFunc = redirectedFunc;
        _widthFunc = widthFunc;
        _heightFunc = heightFunc;
        _initialRow = initialRowFunc();
    }

    /// <summary>
    /// Allocates a new bar.
    /// </summary>
    /// <param name="barKey">Bar key.</param>
    public void Allocate(TKey barKey)
    {
        lock (_lock)
        {
            GetOrCreate(barKey);
        }
    }

    /// <summary>
    /// Removes the specified bar.
    /// </summary>
    /// <param name="barKey">Bar key.</param>
    public void Remove(TKey barKey)
    {
        lock (_lock)
        {
            if (!_keysToIndices.TryGetValue(barKey, out int value))
            {
                return;
            }
            _entries.RemoveAt(value);
            for (int i = value; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                _keysToIndices[entry.Key] = i;
                MoveTo(i);
                entry.State.Redraw(_output);
            }
            var blank = new BlankContentFiller();
            DrawInternal(_clearBar, _entries.Count, ref blank, 0);
        }
    }

    private bool TryGet(TKey barKey, out KeyValuePair<int, BarEntry> value)
    {
        if (_keysToIndices.TryGetValue(barKey, out int index))
        {
            value = new KeyValuePair<int, BarEntry>(index, _entries[index]);
            return true;
        }
        value = default;
        return false;
    }

    private KeyValuePair<int, BarEntry> GetOrCreate(TKey barKey)
    {
        if (_keysToIndices.TryGetValue(barKey, out int index))
        {
            return new KeyValuePair<int, BarEntry>(index, _entries[index]);
        }
        int addIndex = _entries.Count;
        var entry = CreateAndStartNewBarEntry(barKey);
        _entries.Add(entry);
        _keysToIndices[barKey] = addIndex;
        return new KeyValuePair<int, BarEntry>(addIndex, entry);
    }

    private static BarEntry CreateAndStartNewBarEntry(TKey barKey)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        return new BarEntry(new BarState(true), stopwatch, barKey);
    }

    private void MoveTo(int index)
    {
        _output.Write($"\u001b[H\u001b[{_initialRow + index + 1};1H");
    }

    /// <summary>
    /// Writes the specified content immediately.
    /// </summary>
    /// <param name="barKey">Bar key.</param>
    /// <param name="contentFiller">Content.</param>
    /// <param name="scrollIndex">Current scroll index for the content.</param>
    /// <typeparam name="T">Content filler type.</typeparam>
    public void Write<T>(TKey barKey, ref T contentFiller, int scrollIndex = 0) where T : IContentFiller
    {
        lock (_lock)
        {
            if (TryGet(barKey, out var value))
            {
                DrawInternal(value.Value, value.Key, ref contentFiller, scrollIndex);
            }
        }
    }

    /// <summary>
    /// Updates with the specified content if the bar is due for an update based on the configured update interval.
    /// </summary>
    /// <param name="barKey">Bar key.</param>
    /// <param name="contentFiller">Content.</param>
    /// <param name="scrollIndex">Current scroll index for the content.</param>
    /// <typeparam name="T">Content filler type.</typeparam>
    public void Update<T>(TKey barKey, ref T contentFiller, int scrollIndex = 0) where T : IContentFiller
    {
        lock (_lock)
        {
            if (!TryGet(barKey, out var value))
            {
                return;
            }
            if (value.Value.Stopwatch.Elapsed < _interval)
            {
                return;
            }
            DrawInternal(value.Value, value.Key, ref contentFiller, scrollIndex);
        }
    }

    /// <summary>
    /// Clears the bar output.
    /// </summary>
    /// <param name="barKey">Bar key.</param>
    public void Clear(TKey barKey)
    {
        lock (_lock)
        {
            if (!TryGet(barKey, out var value))
            {
                return;
            }
            var blank = new BlankContentFiller();
            DrawInternal(value.Value, value.Key, ref blank, 0);
        }
    }

    /// <summary>
    /// Clears all bar outputs.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void ClearAll()
    {
        lock (_lock)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var blank = new BlankContentFiller();
                DrawInternal(_clearBar, i, ref blank, 0);
            }
            MoveTo(0);
        }
    }

    private void DrawInternal<T>(BarEntry entry, int index, ref T contentFiller, int scrollIndex) where T : IContentFiller
    {
        MoveTo(index);
        DrawInternal(entry.State, index, ref contentFiller, scrollIndex);
    }

    private void DrawInternal<T>(BarState barState, int index, ref T contentFiller, int scrollIndex) where T : IContentFiller
    {
        MoveTo(index);
        barState.Draw(ref contentFiller, scrollIndex, AvailableWidth, _output);
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
    /// <param name="heightFunc">TODO</param>
    /// <param name="initialRowFunc">TODO</param>
    /// <param name="interval">Update interval.</param>
    /// <returns></returns>
    public static MultiBarContext<TKey> Create(
        TextWriter output,
        bool forceFallback,
        Func<bool> redirectedFunc,
        Func<int> widthFunc,
        Func<int> heightFunc,
        Func<int> initialRowFunc,
        TimeSpan interval = default)
    {
        if (interval == default)
        {
            interval = BarContext.DefaultInterval;
        }
        if (!forceFallback && OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
        {
            return new WindowsMultiBarContext<TKey>(output, redirectedFunc, widthFunc, heightFunc, initialRowFunc, interval);
        }
        return new WidthMinusOneMultiBarContext<TKey>(output, redirectedFunc, widthFunc, heightFunc, initialRowFunc, interval);
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
        lock (_lock)
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
