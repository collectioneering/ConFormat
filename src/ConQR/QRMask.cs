namespace ConQR;

/// <summary>
/// The mask pattern used in a QR Code symbol.
/// </summary>
/// <remarks>
/// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L70
/// </remarks>
public enum QRMask
{
    /// <summary>
    /// Mask pattern 0.
    /// </summary>
    Mask0 = 0,

    /// <summary>
    /// Mask pattern 1.
    /// </summary>
    Mask1 = 1,

    /// <summary>
    /// Mask pattern 2.
    /// </summary>
    Mask2 = 2,

    /// <summary>
    /// Mask pattern 3.
    /// </summary>
    Mask3 = 3,

    /// <summary>
    /// Mask pattern 4.
    /// </summary>
    Mask4 = 4,

    /// <summary>
    /// Mask pattern 5.
    /// </summary>
    Mask5 = 5,

    /// <summary>
    /// Mask pattern 6.
    /// </summary>
    Mask6 = 6,

    /// <summary>
    /// Mask pattern 7.
    /// </summary>
    Mask7 = 7,

    /// <summary>
    /// A special value to tell the QR Code encoder to automatically select an appropriate mask pattern.
    /// </summary>
    Auto = ~0
}
