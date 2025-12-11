namespace ConQR;

/// <summary>
/// The error correction level in a QR Code symbol.
/// </summary>
/// <remarks>
/// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L57
/// </remarks>
public enum QRECCLevel
{
    /// <summary>
    /// The QR Code can tolerate about 7% erroneous codewords.
    /// </summary>
    Low = 0,

    /// <summary>
    /// The QR Code can tolerate about 15% erroneous codewords.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// The QR Code can tolerate about 25% erroneous codewords.
    /// </summary>
    Quartile = 2,

    /// <summary>
    /// The QR Code can tolerate about 30% erroneous codewords.
    /// </summary>
    High = 3
}
