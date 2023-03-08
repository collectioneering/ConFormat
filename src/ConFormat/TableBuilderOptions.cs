namespace ConFormat;

/// <summary>
/// Provides configuration for a <see cref="TableBuilder"/>.
/// </summary>
/// <param name="InsertPad">If true, add padding between separators and edges of inner content.</param>
public record TableBuilderOptions(bool InsertPad = true)
{
    /// <summary>
    /// Row separator.
    /// </summary>
    public virtual char RowSeparator => '-';

    /// <summary>
    /// Row separator.
    /// </summary>
    public virtual char TopRowSeparator => '-';

    /// <summary>
    /// Row separator.
    /// </summary>
    public virtual char BottomRowSeparator => '-';

    /// <summary>
    /// Row / column separator.
    /// </summary>
    public virtual char RowColumnSeparator => '-';

    /// <summary>
    /// Top column separator.
    /// </summary>
    public virtual char TopColumnSeparator => '-';

    /// <summary>
    /// Bottom column separator.
    /// </summary>
    public virtual char BottomColumnSeparator => '-';

    /// <summary>
    /// Column separator.
    /// </summary>
    public virtual char ColumnSeparator => '|';

    /// <summary>
    /// Left border.
    /// </summary>
    public virtual string? LeftBorder => null;

    /// <summary>
    /// Left top border.
    /// </summary>
    public virtual string? LeftTopBorder => null;

    /// <summary>
    /// Left row separator border.
    /// </summary>
    public virtual string? LeftRowSeparatorBorder => null;

    /// <summary>
    /// Left bottom border.
    /// </summary>
    public virtual string? LeftBottomBorder => null;

    /// <summary>
    /// Right border.
    /// </summary>
    public virtual string? RightBorder => null;

    /// <summary>
    /// Right top border.
    /// </summary>
    public virtual string? RightTopBorder => null;

    /// <summary>
    /// Right row separator border.
    /// </summary>
    public virtual string? RightRowSeparatorBorder => null;

    /// <summary>
    /// Right bottom border.
    /// </summary>
    public virtual string? RightBottomBorder => null;
}
