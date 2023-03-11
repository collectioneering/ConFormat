using System.Globalization;

namespace ConFormat;

/// <summary>
/// Provides utility for writing QR code content to a <see cref="TextWriter"/>.
/// </summary>
public static class QRWriter
{
    /// <summary>
    /// Attempts to generate and write a QR code from UTF-8-encoded text.
    /// </summary>
    /// <param name="writer">Target writer.</param>
    /// <param name="text">UTF-8-encoded text.</param>
    /// <param name="eccLevel">The target ECC level to apply.</param>
    /// <param name="minVersion">Minimum allowed version.</param>
    /// <param name="maxVersion">Maximum allowed version.</param>
    /// <param name="mask">The mask pattern to apply.</param>
    /// <param name="boostEccLevel">If true, then the ECC level of the result may be higher than <paramref name="eccLevel"/> if it can be done without increasing the version.</param>
    /// <returns>True if successfully generated.</returns>
    public static bool TryWriteFromText(TextWriter writer, ReadOnlySpan<byte> text, QRECCLevel eccLevel = QRECCLevel.Medium, int minVersion = QR.MinimumVersion, int maxVersion = QR.MaximumVersion, QRMask mask = QRMask.Auto, bool boostEccLevel = true)
    {
        Span<byte> qr = stackalloc byte[QR.GetBufferLengthForVersion(QR.MaximumVersion)];
        if (QR.TryEncodeText(text, ref qr, eccLevel, minVersion, maxVersion, mask, boostEccLevel))
        {
            Write(writer, qr);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to generate and write a QR code from a binary buffer.
    /// </summary>
    /// <param name="writer">Target writer.</param>
    /// <param name="binary">Binary buffer.</param>
    /// <param name="eccLevel">The target ECC level to apply.</param>
    /// <param name="minVersion">Minimum allowed version.</param>
    /// <param name="maxVersion">Maximum allowed version.</param>
    /// <param name="mask">The mask pattern to apply.</param>
    /// <param name="boostEccLevel">If true, then the ECC level of the result may be higher than <paramref name="eccLevel"/> if it can be done without increasing the version.</param>
    /// <returns>True if successfully generated.</returns>
    public static bool TryWriteFromBinary(TextWriter writer, ReadOnlySpan<byte> binary, QRECCLevel eccLevel = QRECCLevel.Medium, int minVersion = QR.MinimumVersion, int maxVersion = QR.MaximumVersion, QRMask mask = QRMask.Auto, bool boostEccLevel = true)
    {
        Span<byte> qr = stackalloc byte[QR.GetBufferLengthForVersion(QR.MaximumVersion)];
        if (QR.TryEncodeBinary(binary, ref qr, eccLevel, minVersion, maxVersion, mask, boostEccLevel))
        {
            Write(writer, qr);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Writes QR code.
    /// </summary>
    /// <param name="writer">Target writer.</param>
    /// <param name="qr">QR code content.</param>
    public static void Write(TextWriter writer, ReadOnlySpan<byte> qr)
    {
        int side = QR.GetSize(qr);
        int hh = (side + 1) >> 1;
        for (int i = 0; i < hh; i++)
        {
            WriteRow(writer, qr, i * 2, side, side);
        }
    }

    private static void WriteRow(TextWriter writer, ReadOnlySpan<byte> qr, int y, int w, int h)
    {
        if (y + 1 == h)
        {
            for (int i = 0; i < w; i++)
            {
                writer.Write(QR.GetModuleBounded(qr, i, y) ? "\u001b[38;5;232m▀" : "\u001b[38;5;231m▀"); // fg
            }
        }
        else
        {
            writer.Write("\u001b[38;5;231m\u001b[48;5;232m"); // fg,bg
            for (int i = 0; i < w; i++)
            {
                writer.Write(Blocks[(QR.GetModuleBounded(qr, i, y) ? 1 : 0) | (QR.GetModuleBounded(qr, i, y + 1) ? 2 : 0)]);
            }
        }
        writer.Write("\u001b[0m\u001b[");
        WritePlain(writer, w);
        writer.Write("D\u001b[1B");
    }

    private static void WritePlain(TextWriter textWriter, int value)
    {
        Span<char> tSpan = stackalloc char[IntBitLength];
        if (value.TryFormat(tSpan, out int tSpanLength, provider: CultureInfo.InvariantCulture))
        {
            textWriter.Write(tSpan[..tSpanLength]);
        }
        else
        {
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));
        }
    }

    // -2147483648
    private const int IntBitLength = 11;

    private static ReadOnlySpan<char> Blocks => "█▄▀ ";
}
