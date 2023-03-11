using System.Diagnostics;
using System.Text;

namespace ConFormat;

/// <summary>
/// Content filler that applies scroll to output.
/// </summary>
public class ScrollingContentFiller : IContentFiller
{
    /// <summary>
    /// Inner content.
    /// </summary>
    public IContentFiller Content;

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
    /// Creates an instance of <see cref="ScrollingContentFiller"/>.
    /// </summary>
    /// <param name="interval">Update interval.</param>
    /// <param name="initialContent"></param>
    /// <returns>Content instance.</returns>
    public static ScrollingContentFiller Create(TimeSpan interval, IContentFiller initialContent)
    {
        return new ScrollingContentFiller(interval, initialContent);
    }

    /// <summary>
    /// Initializes an instance of <see cref="ScrollingContentFiller"/>.
    /// </summary>
    /// <param name="interval">Update interval.</param>
    /// <param name="content">Initial inner content.</param>
    public ScrollingContentFiller(TimeSpan interval, IContentFiller content)
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
