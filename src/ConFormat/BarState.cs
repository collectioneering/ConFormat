using System.Text;

namespace ConFormat;

internal class BarState
{
    private readonly bool _persistPreviousDrawState;
    private readonly StringBuilder _stringBuilder = new();

    public BarState(bool persistPreviousDrawState)
    {
        _persistPreviousDrawState = persistPreviousDrawState;
    }

    public void Redraw(TextWriter output)
    {
        if (!_persistPreviousDrawState)
        {
            throw new NotSupportedException($"Cannot call {nameof(Redraw)} when this {nameof(BarState)} instance is not set to persist previous draw state");
        }
        DrawLine(_stringBuilder, output);
    }

    public void Draw<T>(ref T contentFiller, int scrollIndex, int availableWidth, TextWriter output) where T : IContentFiller
    {
        _stringBuilder.Clear();
        int widthRemaining = availableWidth;
        _stringBuilder.Append('\r');
        if (widthRemaining > 0)
        {
            contentFiller.Fill(_stringBuilder, widthRemaining, scrollIndex);
        }
        DrawLine(_stringBuilder, output);
        if (!_persistPreviousDrawState)
        {
            _stringBuilder.Clear();
        }
    }

    /// <summary>
    /// Draws the content of the specified <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="stringBuilder"><see cref="StringBuilder"/> with the content to write.</param>
    /// <param name="output">Output <see cref="TextWriter"/>.</param>
    protected virtual void DrawLine(StringBuilder stringBuilder, TextWriter output)
    {
        foreach (var chunk in stringBuilder.GetChunks())
        {
            output.Write(chunk.Span);
        }
    }

    /// <summary>
    /// Applies a newline to the output writer.
    /// <param name="output">Output <see cref="TextWriter"/>.</param>
    /// </summary>
    public virtual void EndLine(TextWriter output)
    {
        output.WriteLine();
    }
}
