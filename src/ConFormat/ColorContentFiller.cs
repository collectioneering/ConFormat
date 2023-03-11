using System.Text;

namespace ConFormat;

/// <summary>
/// Content filler that applies color to output.
/// </summary>
public class ColorContentFiller : IContentFiller
{
    private const string Reset = "\u001b[0m";

    private static readonly Dictionary<ConsoleColor, string> s_sequences = new()
    {
        /*{Color.Black, "\u001b[30;1m"},*/
        { ConsoleColor.Red, "\u001b[31;1m" },
        { ConsoleColor.Green, "\u001b[32;1m" },
        { ConsoleColor.Yellow, "\u001b[33;1m" },
        { ConsoleColor.Blue, "\u001b[34;1m" },
        { ConsoleColor.Magenta, "\u001b[35;1m" },
        { ConsoleColor.Cyan, "\u001b[36;1m" },
        { ConsoleColor.White, "\u001b[37;1m" },
    };
    /// <summary>
    /// Color.
    /// </summary>
    public ConsoleColor Color;

    /// <summary>
    /// Inner content.
    /// </summary>
    public IContentFiller Content;

    /// <summary>
    /// Creates an instance of <see cref="ColorContentFiller"/>.
    /// </summary>
    /// <param name="initialColor">Initial color.</param>
    /// <param name="initialContent">Initial inner content.</param>
    /// <returns>Content instance.</returns>
    public static ColorContentFiller Create(ConsoleColor initialColor, IContentFiller initialContent)
    {
        return new ColorContentFiller(initialColor, initialContent);
    }

    /// <summary>
    /// Initializes an instance of <see cref="ColorContentFiller"/>.
    /// </summary>
    /// <param name="initialColor">Initial color.</param>
    /// <param name="initialContent">Initial inner content.</param>
    public ColorContentFiller(ConsoleColor initialColor, IContentFiller initialContent)
    {
        Color = initialColor;
        Content = initialContent;
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width, int scrollIndex = 0)
    {
        if (width < 1)
        {
            return;
        }
        if (s_sequences.TryGetValue(Color, out string? sequence))
        {
            stringBuilder.Append(sequence);
        }
        Content.Fill(stringBuilder, width, scrollIndex);
        stringBuilder.Append(Reset);
    }
}
