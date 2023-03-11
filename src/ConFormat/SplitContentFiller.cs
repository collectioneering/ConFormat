using System.Text;
using EA;

namespace ConFormat;

/// <summary>
/// Provides default creation for <see cref="SplitContentFiller{TContentLeft,TContentRight}"/>.
/// </summary>
public static class SplitContentFiller
{
    /// <summary>
    /// Creates an instance of <see cref="SplitContentFiller{TContentLeft,TContentRight}"/>.
    /// </summary>
    /// <param name="separator">Separator.</param>
    /// <param name="weightLeft">Weight for left content.</param>
    /// <param name="weightRight">Weight for right content.</param>
    /// <param name="initialContentLeft">Initial left content.</param>
    /// <param name="initialContentRight">Initial right content.</param>
    /// <typeparam name="TContentLeft">Left content type.</typeparam>
    /// <typeparam name="TContentRight">Right content type.</typeparam>
    /// <returns>Content instance.</returns>
    public static SplitContentFiller<TContentLeft, TContentRight> Create<TContentLeft, TContentRight>(string separator, float weightLeft, float weightRight, TContentLeft initialContentLeft, TContentRight initialContentRight)
        where TContentLeft : IContentFiller where TContentRight : IContentFiller
    {
        return new SplitContentFiller<TContentLeft, TContentRight>(separator, weightLeft, weightRight, initialContentLeft, initialContentRight);
    }
}

/// <summary>
/// Content filler that uses two contained content fillers, with relative weight-based sizing.
/// </summary>
/// <typeparam name="TContentLeft">Left content type.</typeparam>
/// <typeparam name="TContentRight">Right content type.</typeparam>
public struct SplitContentFiller<TContentLeft, TContentRight> : IContentFiller where TContentLeft : IContentFiller where TContentRight : IContentFiller
{
    /// <summary>
    /// Separator.
    /// </summary>
    public readonly string Separator;
    private readonly float _weightLeft;
    private readonly float _weightRight;

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
    /// Initializes an instance of <see cref="SplitContentFiller{TContentLeft,TContentRight}"/>.
    /// </summary>
    /// <param name="separator">Separator.</param>
    /// <param name="weightLeft">Weight for left content.</param>
    /// <param name="weightRight">Weight for right content.</param>
    /// <param name="initialContentLeft">Initial left content.</param>
    /// <param name="initialContentRight">Initial right content.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public SplitContentFiller(string separator, float weightLeft, float weightRight, TContentLeft initialContentLeft, TContentRight initialContentRight)
    {
        if (weightLeft < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightLeft));
        }
        if (weightRight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weightRight));
        }
        Separator = separator;
        _weightLeft = weightLeft;
        _weightRight = weightRight;
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
        float totalWeight = _weightLeft + _weightRight;
        int widthLeft;
        int widthRight;
        if (totalWeight <= float.Epsilon)
        {
            widthLeft = remainingWidth >> 1;
            widthRight = remainingWidth - widthLeft;
        }
        else
        {
            if (_weightLeft <= float.Epsilon)
            {
                widthLeft = 0;
                widthRight = remainingWidth;
            }
            else
            {
                if (_weightRight <= float.Epsilon)
                {
                    widthRight = 0;
                }
                else
                {
                    widthRight = Math.Clamp((int)Math.Round((double)_weightRight * remainingWidth / totalWeight), 0, remainingWidth);
                }
                widthLeft = remainingWidth - widthRight;
            }
        }
        ContentLeft.Fill(stringBuilder, widthLeft);
        stringBuilder.Append(Separator);
        ContentRight.Fill(stringBuilder, widthRight);
    }
}
