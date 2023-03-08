namespace ConFormat;

/// <summary>
/// Provides configuration for table output using Unicode characters.
/// </summary>
/// <param name="InsertPad">If true, add padding between separators and edges of inner content.</param>
public record UnicodeTableBuilderOptions(bool InsertPad = true) : TableBuilderOptions(InsertPad)
{
    /// <inheritdoc />
    public override char RowSeparator => '─';

    /// <inheritdoc />
    public override char RowColumnSeparator => '┼';

    /// <inheritdoc />
    public override char TopColumnSeparator => '┯';

    /// <inheritdoc />
    public override char BottomColumnSeparator => '┷';

    /// <inheritdoc />
    public override char ColumnSeparator => '│';

    /// <inheritdoc />
    public override string LeftBorder => "┃";

    /// <inheritdoc />
    public override string LeftTopBorder => "┏";

    /// <inheritdoc />
    public override string LeftRowSeparatorBorder => "┠";

    /// <inheritdoc />
    public override string LeftBottomBorder => "┗";

    /// <inheritdoc />
    public override string RightBorder => "┃";

    /// <inheritdoc />
    public override string RightTopBorder => "┓";

    /// <inheritdoc />
    public override string RightRowSeparatorBorder => "┨";

    /// <inheritdoc />
    public override string RightBottomBorder => "┛";
}
