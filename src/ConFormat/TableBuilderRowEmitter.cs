namespace ConFormat;

/// <summary>
/// Provides output for a row in a <see cref="TableBuilder"/>.
/// </summary>
public struct TableBuilderRowEmitter
{
    private readonly TableBuilder _builder;
    private readonly bool _autoEndLine;
    private ReadOnlyMemory<TableBuilder.TableBuilderEntry> _rem;

    internal TableBuilderRowEmitter(TableBuilder builder, bool autoEndLine)
    {
        _builder = builder;
        _autoEndLine = autoEndLine;
        _rem = builder._entries;
    }

    /// <summary>
    /// Emits an element for the row to the provided writer.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="value">Value to emit.</param>
    public void Emit(TextWriter textWriter, string value)
    {
        if (_rem.Length == 0)
        {
            throw new InvalidOperationException();
        }
        _builder.Write(textWriter, in _rem.Span[0], value);
        _rem = _rem[1..];
        if (_rem.Length == 0 && _autoEndLine)
        {
            textWriter.WriteLine();
        }
    }
}
