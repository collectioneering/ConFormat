using System.Text;
using EA;

namespace ConFormat;

/// <summary>
/// Provides default creation for <see cref="BorderContentFiller{TContent}"/>.
/// </summary>
public static class BorderContentFiller
{
    /// <summary>
    /// Creates an instance of <see cref="BorderContentFiller{TContent}"/>.
    /// </summary>
    /// <param name="left">Left border.</param>
    /// <param name="right">Right content.</param>
    /// <param name="initialContent">Initial inner content.</param>
    /// <typeparam name="TContent">Inner content type.</typeparam>
    /// <returns>Content instance.</returns>
    public static BorderContentFiller<TContent> Create<TContent>(string left, string right, TContent initialContent)
        where TContent : IContentFiller
    {
        return new BorderContentFiller<TContent>(left, right, initialContent);
    }
}

/// <summary>
/// Content filler that provides left and right borders.
/// </summary>
/// <typeparam name="TContent">Inner content type.</typeparam>
public struct BorderContentFiller<TContent> : IContentFiller where TContent : IContentFiller
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
    public TContent Content;

    /// <summary>
    /// Initializes an instance of <see cref="BorderContentFiller{TContent}"/>.
    /// </summary>
    /// <param name="left">Left border.</param>
    /// <param name="right">Right border.</param>
    /// <param name="initialContent"></param>
    public BorderContentFiller(string left, string right, TContent initialContent)
    {
        Left = left;
        Right = right;
        _borderWidth = EastAsianWidth.GetWidth(Left) + EastAsianWidth.GetWidth(Right);
        Content = initialContent;
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width)
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
