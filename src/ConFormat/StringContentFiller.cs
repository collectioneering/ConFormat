using System.Text;

namespace ConFormat;

/// <summary>
/// Content filler for text.
/// </summary>
public readonly struct StringContentFiller : IContentFiller
{
    /// <summary>
    /// Creates an instance of <see cref="StringContentFiller"/>.
    /// </summary>
    /// <param name="content">Content.</param>
    /// <param name="alignment">Text alignment.</param>
    /// <returns>Content instance.</returns>
    public static StringContentFiller Create(string content, ContentAlignment alignment)
    {
        return new StringContentFiller(content, alignment);
    }

    /// <summary>
    /// Content.
    /// </summary>
    public readonly string Content;

    /// <summary>
    /// Text alignment.
    /// </summary>
    public readonly ContentAlignment Alignment;

    /// <summary>
    /// Initializes an instance of <see cref="StringContentFiller"/>.
    /// </summary>
    /// <param name="content">Content.</param>
    /// <param name="alignment">Text content.</param>
    public StringContentFiller(string content, ContentAlignment alignment)
    {
        Content = content;
        Alignment = alignment;
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width)
    {
        switch (Alignment)
        {
            case ContentAlignment.Left:
                StringFillUtil.FillLeft(Content, stringBuilder, width);
                break;
            case ContentAlignment.Right:
                StringFillUtil.FillRight(Content, stringBuilder, width);
                break;
            default:
                StringFillUtil.PadRemaining(stringBuilder, width);
                break;
        }
    }
}
