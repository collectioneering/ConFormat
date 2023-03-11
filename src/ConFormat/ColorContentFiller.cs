using System.Text;

namespace ConFormat;

/// <summary>
/// Provides default creation for <see cref="ColorContentFiller{TContent}"/>.
/// </summary>
public static class ColorContentFiller
{
    internal const string Reset = "\u001b[0m";

    internal static readonly Dictionary<ConsoleColor, string> s_sequences = new()
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
    /// Creates an instance of <see cref="ColorContentFiller{TContent}"/>.
    /// </summary>
    /// <param name="initialColor">Initial color.</param>
    /// <param name="initialContent">Initial inner content.</param>
    /// <typeparam name="TContent">Inner content type.</typeparam>
    /// <returns>Content instance.</returns>
    public static ColorContentFiller<TContent> Create<TContent>(ConsoleColor initialColor, TContent initialContent)
        where TContent : IContentFiller
    {
        return new ColorContentFiller<TContent>(initialColor, initialContent);
    }
}

/// <summary>
/// Content filler that applies color to output.
/// </summary>
/// <typeparam name="TContent">Inner content type.</typeparam>
public struct ColorContentFiller<TContent> : IContentFiller where TContent : IContentFiller
{
    /// <summary>
    /// Color.
    /// </summary>
    public ConsoleColor Color;

    /// <summary>
    /// Inner content.
    /// </summary>
    public TContent Content;

    /// <summary>
    /// Initializes an instance of <see cref="ColorContentFiller{TContent}"/>.
    /// </summary>
    /// <param name="initialColor">Initial color.</param>
    /// <param name="initialContent">Initial inner content.</param>
    public ColorContentFiller(ConsoleColor initialColor, TContent initialContent)
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
        if (ColorContentFiller.s_sequences.TryGetValue(Color, out string? sequence))
        {
            stringBuilder.Append(sequence);
        }
        Content.Fill(stringBuilder, width, scrollIndex);
        stringBuilder.Append(ColorContentFiller.Reset);
    }
}
