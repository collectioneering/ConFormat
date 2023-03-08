namespace ConFormat;

/// <summary>
/// A segment of character/binary/control data in a QR Code symbol.
/// </summary>
/// <remarks>
/// The mid-level way to create a segment is to take the payload data
/// and call a factory function such as <see cref="QR.MakeNumeric"/>.
/// The low-level way to create a segment is to custom-make the bit buffer
/// and initialize a <see cref="QRSegment"/> struct with appropriate values.
/// Even in the most favorable conditions, a QR Code can only hold 7089 characters of data.
/// Any segment longer than this is meaningless for the purpose of generating QR Codes.
/// Moreover, the maximum allowed bit length is 32767 because
/// the largest QR Code (version 40) has 31329 modules.
/// <br/>
/// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L109
/// </remarks>
public struct QRSegment
{
    /// <summary>
    /// The mode indicator of this segment.
    /// </summary>
    public QRMode Mode;

    /// <summary>
    /// The length of this segment's unencoded data.
    /// </summary>
    /// <remarks>
    /// Measured in characters for numeric/alphanumeric/kanji mode, bytes for byte mode, and 0 for ECI mode.
    /// Always zero or positive. Not the same as the data's bit length.
    /// </remarks>
    public int NumChars;

    /// <summary>
    /// The byte offset in the source buffer to the data bits of this segment, packed in bitwise big endian.
    /// </summary>
    /// <remarks>
    /// Can be null if the bit length is zero.
    /// </remarks>
    public int? DataOffset;

    /// <summary>
    /// The number of valid data bits used in the buffer.
    /// </summary>
    /// <remarks>
    /// Requires 0 &lt;= bitLength &lt;= 32767, and bitLength &lt;= (capacity of data array) * 8.
    /// The character count (numChars) must agree with the mode and the bit buffer length.
    /// </remarks>
    public int BitLength;
}
