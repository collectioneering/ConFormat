using System.Text;

namespace ConFormat;

/// <summary>
/// Content filler for text.
/// </summary>
public class StringContentFiller : IContentFiller
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
    public string Content
    {
        get => _content;
        set
        {
            _content = value ?? throw new InvalidOperationException();
            _contentLength = StringFillUtil.ComputeLength(_content);
        }
    }

    /// <summary>
    /// Text alignment.
    /// </summary>
    public ContentAlignment Alignment { get; set; }

    private int _contentLength;
    private string _content;

    /// <summary>
    /// Initializes an instance of <see cref="StringContentFiller"/>.
    /// </summary>
    /// <param name="content">Content.</param>
    /// <param name="alignment">Text content.</param>
    public StringContentFiller(string content, ContentAlignment alignment)
    {
        _content = content;
        Alignment = alignment;
        _contentLength = StringFillUtil.ComputeLength(content);
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width, int scrollIndex = 0)
    {
        switch (Alignment)
        {
            case ContentAlignment.Left:
                if (width < 1)
                {
                    break;
                }
                if (_contentLength >= width)
                {
                    const int gap = 2;
                    uint contentLength = (uint)_contentLength;
                    uint targetWidth = contentLength + gap;
                    uint offset = (uint)scrollIndex % targetWidth;
                    uint remainingDraw = targetWidth - offset;
                    if (remainingDraw < width)
                    {
                        StringFillUtil.FillLeft(Content, stringBuilder, (int)remainingDraw, scrollIndex);
                        StringFillUtil.FillLeft(Content, stringBuilder, (int)(width - remainingDraw));
                    }
                    else
                    {
                        StringFillUtil.FillLeft(Content, stringBuilder, width, (int)offset);
                    }
                }
                else
                {
                    StringFillUtil.FillLeft(Content, stringBuilder, width);
                }
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
