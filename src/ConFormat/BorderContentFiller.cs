using System.Text;
using EA;

namespace ConFormat;

/// <summary>
/// Content filler that provides left and right borders.
/// </summary>
public class BorderContentFiller : IContentFiller
{
    /// <summary>
    /// Left border.
    /// </summary>
    public readonly string Left;

    /// <summary>
    /// Right border.
    /// </summary>
    public readonly string Right;
    private readonly int _borderWidth;

    /// <summary>
    /// Inner content.
    /// </summary>
    public IContentFiller Content;

    /// <summary>
    /// Creates an instance of <see cref="BorderContentFiller"/>.
    /// </summary>
    /// <param name="left">Left border.</param>
    /// <param name="right">Right content.</param>
    /// <param name="initialContent">Initial inner content.</param>
    /// <returns>Content instance.</returns>
    public static BorderContentFiller Create(string left, string right, IContentFiller initialContent)
    {
        return new BorderContentFiller(left, right, initialContent);
    }
    /// <summary>
    /// Initializes an instance of <see cref="BorderContentFiller"/>.
    /// </summary>
    /// <param name="left">Left border.</param>
    /// <param name="right">Right border.</param>
    /// <param name="initialContent"></param>
    public BorderContentFiller(string left, string right, IContentFiller initialContent)
    {
        Left = left;
        Right = right;
        _borderWidth = EastAsianWidth.GetWidth(Left) + EastAsianWidth.GetWidth(Right);
        Content = initialContent;
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width, int scrollIndex = 0)
    {
        if (width < 1)
        {
            return;
        }
        if (_borderWidth > width)
        {
            StringFillUtil.PadRemaining(stringBuilder, width);
            return;
        }
        stringBuilder.Append(Left);
        Content.Fill(stringBuilder, width - _borderWidth);
        stringBuilder.Append(Right);
    }
}
