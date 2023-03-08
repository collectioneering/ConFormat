using System.Text;

namespace ConFormat;

/// <summary>
/// Content filler using text and switching between 1-3 appended periods for every <see cref="IContentFiller.Fill"/> call.
/// </summary>
public class EllipsisSuffixContentFiller : IContentFiller
{
    /// <summary>
    /// Creates an instance of <see cref="EllipsisSuffixContentFiller"/>.
    /// </summary>
    /// <param name="message">Base message.</param>
    /// <param name="initialI">Initial index (0 = one period, 1 = two periods, 2 = three periods).</param>
    /// <returns>Content instance.</returns>
    public static EllipsisSuffixContentFiller Create(string message, int initialI)
    {
        return new EllipsisSuffixContentFiller(message, initialI);
    }

    /// <summary>
    /// Base message.
    /// </summary>
    public readonly string Message;
    private readonly string[] _messages;
    private int _i;

    /// <summary>
    /// Initializes an instance of <see cref="EllipsisSuffixContentFiller"/>.
    /// </summary>
    /// <param name="message">Base message.</param>
    /// <param name="initialI">Initial index (0 = one period, 1 = two periods, 2 = three periods).</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public EllipsisSuffixContentFiller(string message, int initialI)
    {
        if (initialI < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialI));
        }
        Message = message;
        _messages = new[] { $"{message}.", $"{message}..", $"{message}..." };
        _i = initialI % 3;
    }

    /// <inheritdoc />
    public void Fill(StringBuilder stringBuilder, int width)
    {
        StringFillUtil.FillLeft(_messages[_i], stringBuilder, width);
        _i = (_i + 1) % 3;
    }
}
