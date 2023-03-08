namespace ConFormat;

/// <summary>
/// Describes how a segment's data bits are interpreted.
/// </summary>
/// <remarks>
/// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L89
/// </remarks>
public enum QRMode
{
    /// <summary>
    /// Numeric encoding.
    /// </summary>
    /// <remarks>
    /// 10 bits per 3 digits.
    /// </remarks>
    Numeric = 0b0001,

    /// <summary>
    /// Alphanumeric encoding.
    /// </summary>
    /// <remarks>
    /// 11 bits per 2 characters.
    /// </remarks>
    Alphanumeric = 0b0010,

    /// <summary>
    /// Byte encoding.
    /// </summary>
    /// <remarks>
    /// 8 bits per character.
    /// </remarks>
    Byte = 0b0100,

    /// <summary>
    /// Kanji encoding.
    /// </summary>
    /// <remarks>
    /// 13 bits per character.
    /// </remarks>
    Kanji = 0b1000,

    /// <summary>
    /// Extended Channel Interpretation.
    /// </summary>
    /// <remarks>
    /// Select alternate character set or encoding.
    /// </remarks>
    ECI = 0b0111
}
