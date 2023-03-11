using System.Text;
using EA;

namespace ConFormat;

/// <summary>
/// Provides default creation for <see cref="FixedSplitContentFiller{TContentLeft,TContentRight}"/>.
/// </summary>
public static class FixedSplitContentFiller
{
    /// <summary>
    /// Creates an instance of <see cref="FixedSplitContentFiller{TContentLeft,TContentRight}"/>.
    /// </summary>
    /// <param name="separator">Separator.</param>
    /// <param name="fixedWidth">Fixed width for element that uses it.</param>
    /// <param name="fixedWidthIndex">Element index for content that has fixed width.</param>
    /// <param name="initialContentLeft">Initial left content.</param>
    /// <param name="initialContentRight">Initial right content.</param>
    /// <typeparam name="TContentLeft">Left content type.</typeparam>
    /// <typeparam name="TContentRight">Right content type.</typeparam>
    /// <returns>Content instance.</returns>
    public static FixedSplitContentFiller<TContentLeft, TContentRight> Create<TContentLeft, TContentRight>(string separator, int fixedWidth, int fixedWidthIndex, TContentLeft initialContentLeft, TContentRight initialContentRight)
        where TContentLeft : IContentFiller where TContentRight : IContentFiller
    {
        return new FixedSplitContentFiller<TContentLeft, TContentRight>(separator, fixedWidth, fixedWidthIndex, initialContentLeft, initialContentRight);
    }
}

/// <summary>
/// Content filler that uses two contained content fillers, with a fixed width for one of the elements.
/// </summary>
/// <typeparam name="TContentLeft">Left content type.</typeparam>
/// <typeparam name="TContentRight">Right content type.</typeparam>
public struct FixedSplitContentFiller<TContentLeft, TContentRight> : IContentFiller where TContentLeft : IContentFiller where TContentRight : IContentFiller
{
    /// <summary>
    /// Separator.
    /// </summary>
    public readonly string Separator;
    private readonly int _fixedWidth;
    private readonly int _fixedWidthIndex;

    /// <summary>
    /// Left content.
    /// </summary>
    public TContentLeft ContentLeft;

    /// <summary>
    /// Right content.
    /// </summary>
    public TContentRight ContentRight;
    private readonly int _separatorWidth;

    /// <summary>
    /// Initializes an instance of <see cref="FixedSplitContentFiller{TContentLeft,TContentRight}"/>.
    /// </summary>
    /// <param name="separator">Separator.</param>
    /// <param name="fixedWidth">Fixed width for element that uses it.</param>
    /// <param name="fixedWidthIndex">Element index for content that has fixed width.</param>
    /// <param name="initialContentLeft">Initial left content.</param>
    /// <param name="initialContentRight">Initial right content.</param>
    /// <returns>Content instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for invalid <paramref name="fixedWidthIndex"/> (must be 0 or 1) or <paramref name="fixedWidth"/> (must be &gt;=0).</exception>
    public FixedSplitContentFiller(string separator, int fixedWidth, int fixedWidthIndex, TContentLeft initialContentLeft, TContentRight initialContentRight)
    {
        if (fixedWidth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fixedWidth));
        }
        if (fixedWidthIndex < 0 || fixedWidthIndex > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(fixedWidthIndex));
        }
        Separator = separator;
        _fixedWidth = fixedWidth;
        _fixedWidthIndex = fixedWidthIndex;
        ContentLeft = initialContentLeft;
        ContentRight = initialContentRight;
        _separatorWidth = EastAsianWidth.GetWidth(Separator);
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width, int scrollIndex = 0)
    {
        if (width < 1)
        {
            return;
        }
        if (width <= _separatorWidth)
        {
            StringFillUtil.PadRemaining(stringBuilder, width);
            return;
        }
        int remainingWidth = width - _separatorWidth;
        if (remainingWidth <= _fixedWidth)
        {
            switch (_fixedWidthIndex)
            {
                case 0:
                    ContentLeft.Fill(stringBuilder, width);
                    break;
                case 1:
                    ContentRight.Fill(stringBuilder, width);
                    break;
                default:
                    StringFillUtil.PadRemaining(stringBuilder, width);
                    break;
            }
            return;
        }
        switch (_fixedWidthIndex)
        {
            case 0:
                ContentLeft.Fill(stringBuilder, _fixedWidth);
                stringBuilder.Append(Separator);
                ContentRight.Fill(stringBuilder, remainingWidth - _fixedWidth);
                break;
            case 1:
                ContentLeft.Fill(stringBuilder, remainingWidth - _fixedWidth);
                stringBuilder.Append(Separator);
                ContentRight.Fill(stringBuilder, _fixedWidth);
                break;
            default:
                StringFillUtil.PadRemaining(stringBuilder, width);
                break;
        }
    }
}
