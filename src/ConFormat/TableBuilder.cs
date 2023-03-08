using System.Buffers;
using System.Text;
using EA;

namespace ConFormat;

/// <summary>
/// Provides structured table output.
/// </summary>
public partial class TableBuilder
{
    private readonly TableBuilderOptions _options;

    private readonly record struct TableBuilderCreationEntry(string Label, int Length);

    internal readonly record struct TableBuilderEntry(string Label, int Length, ColumnFlags Flags);

    internal readonly TableBuilderEntry[] _entries;
    private readonly StringBuilder _stringBuilder;

    /// <summary>
    /// Initializes an instance of <see cref="TableBuilder"/>.
    /// </summary>
    /// <param name="options">Options to configure with.</param>
    /// <param name="entries">Pairs of column label and width.</param>
    public TableBuilder(TableBuilderOptions options, params (string Label, int Length)[] entries) : this(options, entries.Select(v => new TableBuilderCreationEntry(v.Label, v.Length)).ToArray())
    {
    }

    private TableBuilder(TableBuilderOptions options, params TableBuilderCreationEntry[] entries)
    {
        if (entries.Length == 0) throw new ArgumentException("Cannot use empty table entries", nameof(entries));
        _options = options;
        if (EastAsianWidth.GetWidth(_options.RowSeparator) != 1)
        {
            throw new ArgumentException("Row separator must have width 1");
        }
        if (EastAsianWidth.GetWidth(_options.TopRowSeparator) != 1)
        {
            throw new ArgumentException("Top row separator must have width 1");
        }
        if (EastAsianWidth.GetWidth(_options.BottomRowSeparator) != 1)
        {
            throw new ArgumentException("Bottom row separator must have width 1");
        }
        string?[] tmp = ArrayPool<string?>.Shared.Rent(4);
        try
        {
            var tmpSpan = tmp.AsSpan(0, 4);
            tmpSpan[0] = options.LeftBorder;
            tmpSpan[1] = options.LeftBottomBorder;
            tmpSpan[2] = options.LeftTopBorder;
            tmpSpan[3] = options.LeftRowSeparatorBorder;
            EnsureAllConsistent(tmpSpan, nameof(options), "left border");
            tmpSpan[0] = options.RightBorder;
            tmpSpan[1] = options.RightBottomBorder;
            tmpSpan[2] = options.RightTopBorder;
            tmpSpan[3] = options.RightRowSeparatorBorder;
            EnsureAllConsistent(tmpSpan, nameof(options), "right border");
        }
        finally
        {
            ArrayPool<string?>.Shared.Return(tmp);
        }
        _entries = new TableBuilderEntry[entries.Length];
        for (int i = 0; i < entries.Length; i++)
        {
            TableBuilderCreationEntry e = entries[i];
            ColumnFlags flags = ColumnFlags.None;
            if (i == 0)
            {
                flags |= ColumnFlags.Left;
            }
            if (i + 1 == entries.Length)
            {
                flags |= ColumnFlags.Right;
            }
            _entries[i] = new TableBuilderEntry(e.Label, Math.Max(e.Length, e.Label.Length), flags);
        }
        _stringBuilder = new StringBuilder();
    }

    private void EnsureAllConsistent(ReadOnlySpan<string?> elements, string paramName, string groupName)
    {
        if (elements.Length == 0)
        {
            return;
        }
        if (elements[0] is { } str)
        {
            int expectedWidth = EastAsianWidth.GetWidth(str);
            for (int i = 1; i < elements.Length; i++)
            {
                if (elements[i] is not { } str2)
                {
                    throw new ArgumentException($"Inconsistent configuration, all {groupName} elements must be null or all must be non-null", paramName);
                }
                if (EastAsianWidth.GetWidth(str2) != expectedWidth)
                {
                    throw new ArgumentException($"Inconsistent configuration, all {groupName} elements must have the same width", paramName);
                }
            }
        }
        else
        {
            for (int i = 1; i < elements.Length; i++)
            {
                if (elements[i] is { })
                {
                    throw new ArgumentException($"Inconsistent configuration, all {groupName} elements must be null or all must be non-null", paramName);
                }
            }
        }
    }

    /// <summary>
    /// Emits the title row to the provided writer.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    public void EmitTitleRow(TextWriter textWriter)
    {
        EmitLineRow(textWriter, SpecialLine.Top);
        foreach (var t in _entries)
        {
            Write(textWriter, t, t.Label);
        }
        textWriter.WriteLine();
        EmitLineRow(textWriter, SpecialLine.RowSeparator);
    }

    /// <summary>
    /// Emits the final row to the provided writer.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    public void EmitEndRow(TextWriter textWriter)
    {
        EmitLineRow(textWriter, SpecialLine.Bottom);
    }

    private void EmitLineRow(TextWriter textWriter, SpecialLine specialLine)
    {
        foreach (var tbe in _entries)
        {
            _stringBuilder.Clear();
            PopulatePreContent(in tbe, specialLine);
                    switch (specialLine)
                    {
                        case SpecialLine.None:
                            _stringBuilder.Append(_options.RowSeparator, tbe.Length);
                            break;
                        case SpecialLine.Top:
                            _stringBuilder.Append(_options.TopRowSeparator, tbe.Length);
                            break;
                        case SpecialLine.RowSeparator:
                            _stringBuilder.Append(_options.RowSeparator, tbe.Length);
                            break;
                        case SpecialLine.Bottom:
                            _stringBuilder.Append(_options.BottomRowSeparator, tbe.Length);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null);
                    }
            PopulatePostContent(in tbe, specialLine);
            WriteCurrentStringBuilderContent(textWriter);
        }
        textWriter.WriteLine();
    }

    /// <summary>
    /// Creates an emitter for one content row.
    /// </summary>
    /// <param name="autoEndLine">If true, automatically emit newline after final element in row.</param>
    /// <returns>Row emitter valid for one row.</returns>
    public TableBuilderRowEmitter CreateRowEmitter(bool autoEndLine = true) => new(this, autoEndLine);

    internal void Write(TextWriter textWriter, in TableBuilderEntry tbe, string value)
    {
        _stringBuilder.Clear();
        PopulatePreContent(in tbe, SpecialLine.None);
        StringFillUtil.FillLeft(value, _stringBuilder, tbe.Length);
        PopulatePostContent(in tbe, SpecialLine.None);
        WriteCurrentStringBuilderContent(textWriter);
    }

    private void PopulatePreContent(in TableBuilderEntry tbe, SpecialLine specialLine)
    {
        if ((tbe.Flags & ColumnFlags.Left) != 0)
        {
            string? value = specialLine switch
            {
                SpecialLine.None => _options.LeftBorder,
                SpecialLine.Top => _options.LeftTopBorder,
                SpecialLine.RowSeparator => _options.LeftRowSeparatorBorder,
                SpecialLine.Bottom => _options.LeftBottomBorder,
                _ => throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null)
            };
            if (value != null)
            {
                _stringBuilder.Append(value);
                if (_options.InsertPad)
                {
                    switch (specialLine)
                    {
                        case SpecialLine.None:
                            _stringBuilder.Append(' ');
                            break;
                        case SpecialLine.Top:
                            _stringBuilder.Append(_options.TopRowSeparator);
                            break;
                        case SpecialLine.RowSeparator:
                            _stringBuilder.Append(_options.RowSeparator);
                            break;
                        case SpecialLine.Bottom:
                            _stringBuilder.Append(_options.BottomRowSeparator);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null);
                    }
                }
            }
        }
    }

    private void PopulatePostContent(in TableBuilderEntry tbe, SpecialLine specialLine)
    {
        if ((tbe.Flags & ColumnFlags.Right) != 0)
        {
            string? value = specialLine switch
            {
                SpecialLine.None => _options.RightBorder,
                SpecialLine.Top => _options.RightTopBorder,
                SpecialLine.RowSeparator => _options.RightRowSeparatorBorder,
                SpecialLine.Bottom => _options.RightBottomBorder,
                _ => throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null)
            };
            if (value != null)
            {
                if (_options.InsertPad)
                {
                    switch (specialLine)
                    {
                        case SpecialLine.None:
                            _stringBuilder.Append(' ');
                            break;
                        case SpecialLine.Top:
                            _stringBuilder.Append(_options.TopRowSeparator);
                            break;
                        case SpecialLine.RowSeparator:
                            _stringBuilder.Append(_options.RowSeparator);
                            break;
                        case SpecialLine.Bottom:
                            _stringBuilder.Append(_options.BottomRowSeparator);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null);
                    }
                }
                _stringBuilder.Append(value);
            }
        }
        else
        {
            if (_options.InsertPad)
            {
                if (specialLine != SpecialLine.None)
                {
                    switch (specialLine)
                    {
                        case SpecialLine.None:
                            _stringBuilder.Append(' ');
                            break;
                        case SpecialLine.Top:
                            _stringBuilder.Append(_options.TopRowSeparator);
                            break;
                        case SpecialLine.RowSeparator:
                            _stringBuilder.Append(_options.RowSeparator);
                            break;
                        case SpecialLine.Bottom:
                            _stringBuilder.Append(_options.BottomRowSeparator);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null);
                    }
                }
                else
                {
                    _stringBuilder.Append(' ');
                }
            }
            switch (specialLine)
            {
                case SpecialLine.None:
                    _stringBuilder.Append(_options.ColumnSeparator);
                    break;
                case SpecialLine.Top:
                    _stringBuilder.Append(_options.TopColumnSeparator);
                    break;
                case SpecialLine.RowSeparator:
                    _stringBuilder.Append(_options.RowColumnSeparator);
                    break;
                case SpecialLine.Bottom:
                    _stringBuilder.Append(_options.BottomColumnSeparator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null);
            }
            if (_options.InsertPad)
            {
                if (specialLine != SpecialLine.None)
                {
                    switch (specialLine)
                    {
                        case SpecialLine.None:
                            _stringBuilder.Append(' ');
                            break;
                        case SpecialLine.Top:
                            _stringBuilder.Append(_options.TopRowSeparator);
                            break;
                        case SpecialLine.RowSeparator:
                            _stringBuilder.Append(_options.RowSeparator);
                            break;
                        case SpecialLine.Bottom:
                            _stringBuilder.Append(_options.BottomRowSeparator);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(specialLine), specialLine, null);
                    }
                }
                else
                {
                    _stringBuilder.Append(' ');
                }
            }
        }
    }

    private void WriteCurrentStringBuilderContent(TextWriter textWriter)
    {
        try
        {
            foreach (var chunk in _stringBuilder.GetChunks())
            {
                textWriter.Write(chunk.Span);
            }
        }
        finally
        {
            _stringBuilder.Clear();
        }
    }


    [Flags]
    internal enum ColumnFlags
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1
    }

    private enum SpecialLine
    {
        None,
        Top,
        RowSeparator,
        Bottom
    }
}
