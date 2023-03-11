using System.Diagnostics;
using System.Text;

namespace ConFormat;

/// <summary>
/// Provides default creation for <see cref="ScrollingContentFiller{TContent}"/>.
/// </summary>
public class ScrollingContentFiller
{
    /// <summary>
    /// Creates an instance of <see cref="ScrollingContentFiller{TContent}"/>.
    /// </summary>
    /// <param name="interval">Update interval.</param>
    /// <param name="initialContent"></param>
    /// <typeparam name="TContent">Initial inner content.</typeparam>
    /// <returns>Content instance.</returns>
    public static ScrollingContentFiller<TContent> Create<TContent>(TimeSpan interval, TContent initialContent)
        where TContent : IContentFiller
    {
        return new ScrollingContentFiller<TContent>(interval, initialContent);
    }
}

/// <summary>
/// Content filler that applies scroll to output.
/// </summary>
/// <typeparam name="TContent">Inner content type.</typeparam>
public struct ScrollingContentFiller<TContent> : IContentFiller where TContent : IContentFiller
{
    /// <summary>
    /// Inner content.
    /// </summary>
    public TContent Content;

    /// <summary>
    /// Stopwatch.
    /// </summary>
    public readonly Stopwatch Stopwatch;

    /// <summary>
    /// Update interval.
    /// </summary>
    public readonly TimeSpan Interval;

    /// <summary>
    /// Active scroll index.
    /// </summary>
    public int ScrollIndex;

    /// <summary>
    /// Initializes an instance of <see cref="ScrollingContentFiller{TContent}"/>.
    /// </summary>
    /// <param name="interval">Update interval.</param>
    /// <param name="content">Initial inner content.</param>
    public ScrollingContentFiller(TimeSpan interval, TContent content)
    {
        Stopwatch = new Stopwatch();
        Interval = interval;
        Content = content;
        ScrollIndex = 0;
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width, int scrollIndex = 0)
    {
        if (!Stopwatch.IsRunning)
        {
            Stopwatch.Start();
        }
        else
        {
            if (Stopwatch.Elapsed >= Interval)
            {
                Stopwatch.Restart();
                ScrollIndex++;
            }
        }
        Content.Fill(stringBuilder, width, ScrollIndex);
    }
}
