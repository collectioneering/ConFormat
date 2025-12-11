using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ConQR;

/// <summary>
/// Provides utilities for working with QR data.
/// </summary>
public static class QR
{
    private const int ReedSolomonDegreeMax = 30;

    private const int PenaltyN1 = 3;
    private const int PenaltyN2 = 3;
    private const int PenaltyN3 = 40;
    private const int PenaltyN4 = 10;
    private static ReadOnlySpan<byte> AlphanumericCharset => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:"u8;

    private static ReadOnlySpan<sbyte> EccCodewordsPerBlock => new sbyte[]
    {
        // Version: (note that index 0 is for padding, and is set to an illegal value)
        //0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40    Error correction level
        -1, 7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, // Low
        -1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, // Medium
        -1, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, // Quartile
        -1, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, // High
    };

    private static ReadOnlySpan<sbyte> NumErrorCorrectionBlocks => new sbyte[]
    {
        // Version: (note that index 0 is for padding, and is set to an illegal value)
        //0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40    Error correction level
        -1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8, 8, 9, 9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25, // Low
        -1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49, // Medium
        -1, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68, // Quartile
        -1, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81, // High
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static sbyte GetEccCodewordsPerBlock(int i, int j)
    {
        return EccCodewordsPerBlock[i * 41 + j];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static sbyte GetNumErrorCorrectionBlocks(int i, int j)
    {
        return NumErrorCorrectionBlocks[i * 41 + j];
    }

    /// <summary>
    /// The minimum version number supported in the QR Code Model 2 standard.
    /// </summary>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L132
    /// </remarks>
    public const int MinimumVersion = 1;

    /// <summary>
    /// The maximum version number supported in the QR Code Model 2 standard.
    /// </summary>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L133
    /// </remarks>
    public const int MaximumVersion = 40;

    /// <summary>
    /// Calculates the number of bytes needed to store any QR Code up to and including the given version number.
    /// </summary>
    /// <param name="version">Maximal version number to consider.</param>
    /// <returns>Number of bytes needed to store any QR Code up to and including the given version number.</returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L139
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="version"/> is outside the valid range.</exception>
    public static int GetBufferLengthForVersion(int version)
    {
        ThrowForInvalidVersion(version);
        return ((version * 4 + 17) * (version * 4 + 17) + 7) / 8 + 1;
    }

    /// <summary>
    /// Encodes the given UTF-8-encoded text string to a QR Code.
    /// </summary>
    /// <param name="text">UTF-8-encoded text to encode.</param>
    /// <param name="qrCode">QR code buffer to populate (buffer reference is truncated to match output length).</param>
    /// <param name="eccLevel">The target ECC level to apply.</param>
    /// <param name="minVersion">Minimum allowed version.</param>
    /// <param name="maxVersion">Maximum allowed version.</param>
    /// <param name="mask">The mask pattern to apply.</param>
    /// <param name="boostEccLevel">If true, then the ECC level of the result may be higher than <paramref name="eccLevel"/> if it can be done without increasing the version.</param>
    /// <returns>
    /// Returns true if successful, or false if the data is too long to fit
    /// in any version in the given range at the given ECC level.
    /// </returns>
    /// <remarks>
    /// If successful, the resulting QR Code may use numeric, alphanumeric, or byte mode to encode the text.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L132
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minVersion"/> or <paramref name="maxVersion"/> are outside the valid range.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="minVersion"/> is larger than <paramref name="maxVersion"/>,
    /// or if <paramref name="qrCode"/> is shorter than the required buffer length for version <paramref name="maxVersion"/>.
    /// </exception>
    public static bool TryEncodeText(scoped ReadOnlySpan<byte> text, ref Span<byte> qrCode, QRECCLevel eccLevel = QRECCLevel.Medium, int minVersion = MinimumVersion, int maxVersion = MaximumVersion, QRMask mask = QRMask.Auto, bool boostEccLevel = true)
    {
        ThrowForInvalidVersion(minVersion);
        ThrowForInvalidVersion(maxVersion);
        if (minVersion > maxVersion)
        {
            throw new ArgumentException($"Minimum version {minVersion} cannot be larger than maximum version {maxVersion}.", nameof(minVersion));
        }
        int bufLength = GetBufferLengthForVersion(maxVersion);
        if (qrCode.Length < bufLength)
        {
            throw new ArgumentException($"Buffer ({qrCode.Length} bytes) is shorter than required buffer size ({bufLength} bytes)", nameof(qrCode));
        }
        if (text.Length == 0)
        {
            return EncodeSegmentsAdvancedInternal(ReadOnlySpan<QRSegment>.Empty, Span<byte>.Empty, ref qrCode, eccLevel, minVersion, maxVersion, mask, boostEccLevel);
        }
        QRSegment segment = default;
        Span<byte> tmp = stackalloc byte[bufLength];
        if (IsNumeric(text))
        {
            if (CalculateSegmentBufferSize(QRMode.Numeric, text.Length) > bufLength)
            {
                return false;
            }
            segment = MakeNumeric(text, tmp);
        }
        else if (IsAlphanumeric(text))
        {
            if (CalculateSegmentBufferSize(QRMode.Alphanumeric, text.Length) > bufLength)
            {
                return false;
            }
            segment = MakeAlphanumeric(text, tmp);
        }
        else
        {
            if (text.Length > bufLength)
            {
                return false;
            }
            text.CopyTo(tmp);
            segment.Mode = QRMode.Byte;
            segment.BitLength = CalculateSegmentBitLength(segment.Mode, text.Length);
            if (segment.BitLength == int.MaxValue)
            {
                return false;
            }
            segment.NumChars = text.Length;
        }
        segment.DataOffset = 0;
        return EncodeSegmentsAdvancedInternal(stackalloc QRSegment[1] { segment }, tmp, ref qrCode, eccLevel, minVersion, maxVersion, mask, boostEccLevel);
    }

    /// <summary>
    /// Encodes the given binary data to a QR Code.
    /// </summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="qrCode">QR code buffer to populate (buffer reference is truncated to match output length).</param>
    /// <param name="eccLevel">The target ECC level to apply.</param>
    /// <param name="minVersion">Minimum allowed version.</param>
    /// <param name="maxVersion">Maximum allowed version.</param>
    /// <param name="mask">The mask pattern to apply.</param>
    /// <param name="boostEccLevel">If true, then the ECC level of the result may be higher than <paramref name="eccLevel"/> if it can be done without increasing the version.</param>
    /// <returns>
    /// Returns true if successful, or false if the data is too long to fit
    /// in any version in the given range at the given ECC level.
    /// </returns>
    /// <remarks>
    /// If successful, the resulting QR Code will use byte mode to encode the data.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L170
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minVersion"/> or <paramref name="maxVersion"/> are outside the valid range.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="minVersion"/> is larger than <paramref name="maxVersion"/>,
    /// or if <paramref name="qrCode"/> is shorter than the required buffer length for version <paramref name="maxVersion"/>.
    /// </exception>
    public static bool TryEncodeBinary(scoped ReadOnlySpan<byte> data, ref Span<byte> qrCode, QRECCLevel eccLevel = QRECCLevel.Medium, int minVersion = MinimumVersion, int maxVersion = MaximumVersion, QRMask mask = QRMask.Auto, bool boostEccLevel = true)
    {
        ThrowForInvalidVersion(minVersion);
        ThrowForInvalidVersion(maxVersion);
        if (minVersion > maxVersion)
        {
            throw new ArgumentException($"Minimum version {minVersion} cannot be larger than maximum version {maxVersion}.", nameof(minVersion));
        }
        int bufLength = GetBufferLengthForVersion(maxVersion);
        if (qrCode.Length < bufLength)
        {
            throw new ArgumentException($"Buffer ({qrCode.Length} bytes) is shorter than required buffer size ({bufLength} bytes)", nameof(qrCode));
        }
        if (data.Length == 0)
        {
            return EncodeSegmentsAdvancedInternal(ReadOnlySpan<QRSegment>.Empty, Span<byte>.Empty, ref qrCode, eccLevel, minVersion, maxVersion, mask, boostEccLevel);
        }
        if (data.Length > bufLength)
        {
            return false;
        }
        QRSegment segment = default;
        segment.Mode = QRMode.Byte;
        segment.BitLength = CalculateSegmentBitLength(segment.Mode, data.Length);
        if (segment.BitLength == int.MaxValue)
        {
            return false;
        }
        segment.NumChars = data.Length;
        segment.DataOffset = 0;
        Span<byte> tmp = stackalloc byte[bufLength];
        data.CopyTo(tmp);
        return EncodeSegmentsAdvancedInternal(stackalloc QRSegment[1] { segment }, tmp, ref qrCode, eccLevel, minVersion, maxVersion, mask, boostEccLevel);
    }

    /// <summary>
    /// Encodes the given segments to a QR Code.
    /// </summary>
    /// <param name="segments">Segments to encode to a QR code.</param>
    /// <param name="segmentData">Source buffer for raw contents of <paramref name="segments"/>.</param>
    /// <param name="qrCode">QR code buffer to populate (buffer reference is truncated to match output length).</param>
    /// <param name="eccLevel">The target ECC level to apply.</param>
    /// <param name="minVersion">Minimum allowed version.</param>
    /// <param name="maxVersion">Maximum allowed version.</param>
    /// <param name="mask">The mask pattern to apply.</param>
    /// <param name="boostEccLevel">If true, then the ECC level of the result may be higher than <paramref name="eccLevel"/> if it can be done without increasing the version.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="minVersion"/> is larger than <paramref name="maxVersion"/>,
    /// or if <paramref name="qrCode"/> is shorter than the required buffer length for version <paramref name="maxVersion"/>.
    /// </exception>
    public static bool EncodeSegmentsAdvanced(scoped ReadOnlySpan<QRSegment> segments, scoped ReadOnlySpan<byte> segmentData, ref Span<byte> qrCode, QRECCLevel eccLevel = QRECCLevel.Medium, int minVersion = MinimumVersion, int maxVersion = MaximumVersion, QRMask mask = QRMask.Auto, bool boostEccLevel = true)
    {
        ThrowForInvalidVersion(minVersion);
        ThrowForInvalidVersion(maxVersion);
        if (minVersion > maxVersion)
        {
            throw new ArgumentException($"Minimum version {minVersion} cannot be larger than maximum version {maxVersion}.", nameof(minVersion));
        }
        int bufLength = GetBufferLengthForVersion(maxVersion);
        if (qrCode.Length < bufLength)
        {
            throw new ArgumentException($"Buffer ({qrCode.Length} bytes) is shorter than required buffer size ({bufLength} bytes)", nameof(qrCode));
        }
        if (segmentData.Length > bufLength)
        {
            return false;
        }
        Span<byte> tmp = stackalloc byte[bufLength];
        segmentData.CopyTo(tmp);
        return EncodeSegmentsAdvancedInternal(segments, tmp, ref qrCode, eccLevel, minVersion, maxVersion, mask, boostEccLevel);
    }

    private static bool EncodeSegmentsAdvancedInternal(scoped ReadOnlySpan<QRSegment> segments, scoped Span<byte> segmentAndTempBuffer, ref Span<byte> qrCode, QRECCLevel eccLevel, int minVersion, int maxVersion, QRMask mask, bool boostEccLevel)
    {
        if (segments.Length == 0)
        {
            throw new ArgumentException("Cannot encode empty segment array");
        }
        Debug.Assert(MinimumVersion <= minVersion && minVersion <= maxVersion && maxVersion <= MaximumVersion);
        Debug.Assert(0 <= (int)eccLevel && (int)eccLevel <= 3 && -1 <= (int)mask && (int)mask <= 7);
        // Find the minimal version number to use
        int version, dataUsedBits;
        for (version = minVersion;; version++)
        {
            int dataCapacityBits2 = GetDataCodewords(version, eccLevel) * 8; // Number of data bits available
            dataUsedBits = GetTotalBits(segments, version);
            if (dataUsedBits != int.MaxValue && dataUsedBits <= dataCapacityBits2)
            {
                // This version number is found to be suitable
                break;
            }
            if (version >= maxVersion)
            {
                // All versions in the range could not fit the given data
                return false;
            }
        }
        // Increase the error correction level while the data still fits in the current version number
        for (int i = (int)QRECCLevel.Medium; i <= (int)QRECCLevel.High; i++)
        {
            if (boostEccLevel && dataUsedBits <= GetDataCodewords(version, (QRECCLevel)i) * 8)
            {
                eccLevel = (QRECCLevel)i;
            }
        }
        // Concatenate all segments to create the data bit string
        qrCode = qrCode[..GetBufferLengthForVersion(version)];
        qrCode.Clear();
        int bitLength = 0;
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            AppendBitsToBuffer((uint)segment.Mode, 4, qrCode, ref bitLength);
            AppendBitsToBuffer((uint)segment.NumChars, GetCharCountBits(segment.Mode, version), qrCode, ref bitLength);
            for (int j = 0; j < segment.BitLength; j++)
            {
                if (segment.DataOffset is not { } dataOffset)
                {
                    throw new InvalidDataException();
                }
                int bit = (segmentAndTempBuffer[dataOffset + (j >> 3)] >> (7 - (j & 7))) & 1;
                AppendBitsToBuffer((uint)bit, 1, qrCode, ref bitLength);
            }
        }
        Debug.Assert(bitLength == dataUsedBits);
        // Add terminator and pad up to a byte if applicable
        int dataCapacityBits = GetDataCodewords(version, eccLevel) * 8;
        Debug.Assert(bitLength <= dataCapacityBits);
        int terminatorBits = dataCapacityBits - bitLength;
        if (terminatorBits > 4)
        {
            terminatorBits = 4;
        }
        AppendBitsToBuffer(0, terminatorBits, qrCode, ref bitLength);
        AppendBitsToBuffer(0, (8 - bitLength % 8) % 8, qrCode, ref bitLength);
        Debug.Assert(bitLength % 8 == 0);
        // Pad with alternating bytes until data capacity is reached
        for (byte padByte = 0xEC; bitLength < dataCapacityBits; padByte ^= 0xEC ^ 0x11)
        {
            AppendBitsToBuffer(padByte, 8, qrCode, ref bitLength);
        }
        // Compute ECC, draw modules
        AddECCAndInterleave(qrCode, version, eccLevel, segmentAndTempBuffer);
        InitializeFunctionModules(version, qrCode);
        DrawCodewords(segmentAndTempBuffer, GetRawDataModules(version) / 8, qrCode);
        DrawLightFunctionModules(qrCode, version);
        InitializeFunctionModules(version, segmentAndTempBuffer);
        // Do masking
        if (mask == QRMask.Auto)
        {
            // Automatically choose best mask
            int minPenalty = int.MaxValue;
            for (int i = 0; i < 8; i++)
            {
                var msk = (QRMask)i;
                ApplyMask(segmentAndTempBuffer, qrCode, msk);
                DrawFormatBits(eccLevel, msk, qrCode);
                int penalty = GetPenaltyScore(qrCode);
                if (penalty < minPenalty)
                {
                    mask = msk;
                    minPenalty = penalty;
                }
                ApplyMask(segmentAndTempBuffer, qrCode, msk); // Undoes the mask due to XOR
            }
        }
        Debug.Assert(0 <= (int)mask && (int)mask <= 7);
        ApplyMask(segmentAndTempBuffer, qrCode, mask); // Apply the final choice of mask
        DrawFormatBits(eccLevel, mask, qrCode); // Overwrite old format bits
        return true;
    }

    /// <summary>
    /// Appends error correction bytes to each block of the given data array, then interleaves
    /// bytes from the blocks and stores them in the result array.
    /// </summary>
    /// <param name="data">Input buffer.</param>
    /// <param name="version">Version number.</param>
    /// <param name="eccLevel">ECC level.</param>
    /// <param name="result">Result buffer.</param>
    /// <remarks>
    /// data[0 : dataLen] contains the input data. data[dataLen : rawCodewords] is used as a temporary work area
    /// and will be clobbered by this function. The final answer is stored in result[0 : rawCodewords].
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L297
    /// </remarks>
    private static void AddECCAndInterleave(ReadOnlySpan<byte> data, int version, QRECCLevel eccLevel, Span<byte> result)
    {
        Debug.Assert(0 <= (int)eccLevel && (int)eccLevel < 4 && MinimumVersion <= version && version <= MaximumVersion);
        int numBlocks = GetNumErrorCorrectionBlocks((int)eccLevel, version);
        int blockEccLen = GetEccCodewordsPerBlock((int)eccLevel, version);
        int rawCodewords = GetRawDataModules(version) / 8;
        int dataLen = GetDataCodewords(version, eccLevel);
        int numShortBlocks = numBlocks - rawCodewords % numBlocks;
        int shortBlockDataLen = rawCodewords / numBlocks - blockEccLen;
        // Split dat into blocks, calculate ECC< and interleave (not concatenate) the bytes into a single sequence
        Span<byte> rsdiv = stackalloc byte[ReedSolomonDegreeMax];
        ReedSolomonComputeDivisor(blockEccLen, rsdiv);
        ReadOnlySpan<byte> dat = data;
        Span<byte> ecc = stackalloc byte[ReedSolomonDegreeMax]; // Temporary storage
        for (int i = 0; i < numBlocks; i++)
        {
            int datLen = shortBlockDataLen + (i < numShortBlocks ? 0 : 1);
            ReedSolomonComputeRemainder(dat, datLen, rsdiv, blockEccLen, ecc);
            for (int j = 0, k = i; j < datLen; j++, k += numBlocks)
            {
                // Copy data
                if (j == shortBlockDataLen)
                {
                    k -= numShortBlocks;
                }
                result[k] = dat[j];
            }
            for (int j = 0, k = dataLen + i; j < blockEccLen; j++, k += numBlocks)
            {
                // Copy ECC
                result[k] = ecc[j];
            }
            dat = dat[datLen..];
        }
    }

    /// <summary>
    /// Computes a Reed-Solomon ECC generator polynomial for the given degree, storing in result[0 : degree].
    /// </summary>
    /// <param name="degree">Degree.</param>
    /// <param name="result">Result.</param>
    /// <remarks>
    /// This could be implemented as a lookup table over all possible parameter values, instead of as an algorithm.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L361
    /// </remarks>
    private static void ReedSolomonComputeDivisor(int degree, Span<byte> result)
    {
        Debug.Assert(1 <= degree && degree <= ReedSolomonDegreeMax);
        // Polynomial coefficients are stored from highest to lowest power, excluding the leading term which is always 1.
        // For example the polynomial x^3 + 255x^2 + 8x + 93 is stored as the uint8 array {255, 8, 93}.
        result[..degree].Clear();
        result[degree - 1] = 1; // Start off with the monomial x^0
        // Compute the product polynomial (x - r^0) * (x - r^1) * (x - r^2) * ... * (x - r^{degree-1}),
        // drop the highest monomial term which is always 1x^degree.
        // Note that r = 0x02, which is a generator element of this field GF(2^8/0x11D).
        byte root = 1;
        for (int i = 0; i < degree; i++)
        {
            // Multiply the current product by (x - r^i)
            for (int j = 0; j < degree; j++)
            {
                result[j] = ReedSolomonMultiply(result[j], root);
                if (j + 1 < degree)
                {
                    result[j] ^= result[j + 1];
                }
            }
            root = ReedSolomonMultiply(root, 0x02);
        }
    }

    /// <summary>
    /// Computes the Reed-Solomon error correction codeword for the given data and divisor polynomials.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="dataLen"></param>
    /// <param name="generator"></param>
    /// <param name="degree"></param>
    /// <param name="result"></param>
    /// <remarks>
    /// The remainder when data[0 : dataLen] is divided by divisor[0 : degree] is stored in result[0 : degree].
    /// All polynomials are in big endian, and the generator has an implicit leading 1 term.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L387
    /// </remarks>
    private static void ReedSolomonComputeRemainder(ReadOnlySpan<byte> data, int dataLen, ReadOnlySpan<byte> generator, int degree, Span<byte> result)
    {
        Debug.Assert(1 <= degree && degree <= ReedSolomonDegreeMax);
        result[..degree].Clear();
        for (int i = 0; i < dataLen; i++)
        {
            // Polynomial division
            byte factor = (byte)(data[i] ^ result[0]);
            // memmove
            for (int j = 0; j < degree - 1; j++)
            {
                result[j] = result[j + 1];
            }
            result[degree - 1] = 0;
            for (int j = 0; j < degree; j++)
                result[j] ^= ReedSolomonMultiply(generator[j], factor);
        }
    }

    /// <summary>
    /// Gets the product of the two given field elements modulo GF(2^8/0x11D).
    /// </summary>
    /// <param name="x">X.</param>
    /// <param name="y">Y.</param>
    /// <returns>The product of the two given field elements modulo GF(2^8/0x11D).</returns>
    /// <remarks>
    /// All inputs are valid. This could be implemented as a 256*256 lookup table.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L405
    /// </remarks>
    private static byte ReedSolomonMultiply(byte x, byte y)
    {
        // Russian peasant multiplication
        byte z = 0;
        for (int i = 7; i >= 0; i--)
        {
            z = (byte)((z << 1) ^ ((z >> 7) * 0x11D));
            z ^= (byte)(((y >> i) & 1) * x);
        }
        return z;
    }

    /// <summary>
    /// Clears the given QR Code grid with light modules for the given version's size, then marks every function module as dark.
    /// </summary>
    /// <param name="version">Version.</param>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#421
    /// </remarks>
    private static void InitializeFunctionModules(int version, Span<byte> qrCode)
    {
        // Initialize QR Code
        int qrSize = version * 4 + 17;
        qrCode[..((qrSize * qrSize + 7) / 8 + 1)].Clear();
        qrCode[0] = (byte)qrSize;
        // Fill horizontal and vertical timing patterns
        FillRectangle(6, 0, 1, qrSize, qrCode);
        FillRectangle(0, 6, qrSize, 1, qrCode);
        // Fill 3 finder patterns (all corners except bottom right) and format bits
        FillRectangle(0, 0, 9, 9, qrCode);
        FillRectangle(qrSize - 8, 0, 8, 9, qrCode);
        FillRectangle(0, qrSize - 8, 9, 8, qrCode);
        // Fill numerous alignment patterns
        Span<byte> alignPatPos = stackalloc byte[7];
        int numAlign = GetAlignmentPatternPositions(version, alignPatPos);
        for (int i = 0; i < numAlign; i++)
        {
            for (int j = 0; j < numAlign; j++)
            {
                // Don't draw on the three finder corners
                if (!((i == 0 && j == 0) || (i == 0 && j == numAlign - 1) || (i == numAlign - 1 && j == 0)))
                {
                    FillRectangle(alignPatPos[i] - 2, alignPatPos[j] - 2, 5, 5, qrCode);
                }
            }
        }
        // Fill version blocks
        if (version >= 7)
        {
            FillRectangle(qrSize - 11, 0, 3, 6, qrCode);
            FillRectangle(0, qrSize - 11, 6, 3, qrCode);
        }
    }

    /// <summary>
    /// Draws light function modules and possibly some dark modules onto the given QR Code, without changing non-function modules.
    /// </summary>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <param name="version">Version.</param>
    /// <remarks>
    /// This does not draw the format bits. This requires all function modules to be previously
    /// marked dark (namely by <see cref="InitializeFunctionModules"/>), because this may skip redrawing dark function modules.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L458
    /// </remarks>
    private static void DrawLightFunctionModules(Span<byte> qrCode, int version)
    {
        // Draw horizontal and vertical timing patterns
        int qrSize = GetSize(qrCode);
        for (int i = 7; i < qrSize - 7; i += 2)
        {
            SetModuleBounded(qrCode, 6, i, false);
            SetModuleBounded(qrCode, i, 6, false);
        }
        // Draw 3 finder patterns (all corners except bottom right; overwrites some timing modules)
        for (int dy = -4; dy <= 4; dy++)
        {
            for (int dx = -4; dx <= 4; dx++)
            {
                int dist = Math.Abs(dx);
                if (Math.Abs(dy) > dist)
                {
                    dist = Math.Abs(dy);
                }
                if (dist == 2 || dist == 4)
                {
                    SetModuleUnbounded(qrCode, 3 + dx, 3 + dy, false);
                    SetModuleUnbounded(qrCode, qrSize - 4 + dx, 3 + dy, false);
                    SetModuleUnbounded(qrCode, 3 + dx, qrSize - 4 + dy, false);
                }
            }
        }
        // Draw numerous alignment patterns
        Span<byte> alignPatPos = stackalloc byte[7];
        int numAlign = GetAlignmentPatternPositions(version, alignPatPos);
        for (int i = 0; i < numAlign; i++)
        {
            for (int j = 0; j < numAlign; j++)
            {
                if ((i == 0 && j == 0) || (i == 0 && j == numAlign - 1) || (i == numAlign - 1 && j == 0))
                {
                    continue; // Don't draw on the three finder corners
                }
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                        SetModuleBounded(qrCode, alignPatPos[i] + dx, alignPatPos[j] + dy, dx == 0 && dy == 0);
                }
            }
        }
        // Draw version blocks
        if (version >= 7)
        {
            // Calculate error correction code and pack bits
            int rem = version; // version is uint6, in the range [7, 40]
            for (int i = 0; i < 12; i++)
                rem = (rem << 1) ^ ((rem >> 11) * 0x1F25);
            int bits = version << 12 | rem; // uint18
            Debug.Assert(bits >> 18 == 0);

            // Draw two copies
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int k = qrSize - 11 + j;
                    SetModuleBounded(qrCode, k, i, (bits & 1) != 0);
                    SetModuleBounded(qrCode, i, k, (bits & 1) != 0);
                    bits >>= 1;
                }
            }
        }
    }

    private static ReadOnlySpan<byte> DrawFormatBitsTable => new byte[] { 1, 0, 3, 2 };

    /// <summary>
    /// Draws two copies of the format bits (with its own error correction code) based on the given mask and error correction level.
    /// </summary>
    /// <param name="eccLevel">ECC level.</param>
    /// <param name="mask">Mask.</param>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <remarks>
    /// This always draws all modules of
    /// the format bits, unlike <see cref="DrawLightFunctionModules"/> which might skip dark modules.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L519
    /// </remarks>
    private static void DrawFormatBits(QRECCLevel eccLevel, QRMask mask, Span<byte> qrCode)
    {
        // Calculate error correction code and pack bits
        Debug.Assert(0 <= (int)mask && (int)mask <= 7);
        int data = DrawFormatBitsTable[(int)eccLevel] << 3 | (int)mask; // eccLevel is uint2, mask is uint3
        int rem = data;
        for (int i = 0; i < 10; i++)
            rem = (rem << 1) ^ ((rem >> 9) * 0x537);
        int bits = (data << 10 | rem) ^ 0x5412; // uint15
        Debug.Assert(bits >> 15 == 0);

        // Draw first copy
        for (int i = 0; i <= 5; i++)
            SetModuleBounded(qrCode, 8, i, GetBit(bits, i));
        SetModuleBounded(qrCode, 8, 7, GetBit(bits, 6));
        SetModuleBounded(qrCode, 8, 8, GetBit(bits, 7));
        SetModuleBounded(qrCode, 7, 8, GetBit(bits, 8));
        for (int i = 9; i < 15; i++)
            SetModuleBounded(qrCode, 14 - i, 8, GetBit(bits, i));

        // Draw second copy
        int qrSize = GetSize(qrCode);
        for (int i = 0; i < 8; i++)
            SetModuleBounded(qrCode, qrSize - 1 - i, 8, GetBit(bits, i));
        for (int i = 8; i < 15; i++)
            SetModuleBounded(qrCode, 8, qrSize - 15 + i, GetBit(bits, i));
        SetModuleBounded(qrCode, 8, qrSize - 8, true); // Always dark
    }

    /// <summary>
    /// Calculates and stores an ascending list of positions of alignment patterns for this version number.
    /// </summary>
    /// <param name="version">Version number.</param>
    /// <param name="result">Result buffer.</param>
    /// <returns>The length of the list (in the range [0,7]).</returns>
    /// <remarks>
    /// Each position is in the range [0,177), and are used on both the x and y axes.
    /// This could be implemented as lookup table of 40 variable-length lists of unsigned bytes.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L553
    /// </remarks>
    private static int GetAlignmentPatternPositions(int version, Span<byte> result)
    {
        if (version == 1)
        {
            return 0;
        }
        int numAlign = version / 7 + 2;
        int step = version == 32 ? 26 : (version * 4 + numAlign * 2 + 1) / (numAlign * 2 - 2) * 2;
        for (int i = numAlign - 1, pos = version * 4 + 10; i >= 1; i--, pos -= step)
        {
            result[i] = (byte)pos;
        }
        result[0] = 6;
        return numAlign;
    }

    /// <summary>
    /// Sets every module in the range [left : left + width] * [top : top + height] to dark.
    /// </summary>
    /// <param name="left">Left.</param>
    /// <param name="top">Top.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L567
    /// </remarks>
    private static void FillRectangle(int left, int top, int width, int height, Span<byte> qrCode)
    {
        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                SetModuleBounded(qrCode, left + dx, top + dy, true);
            }
        }
    }

    /// <summary>
    /// Draws the raw codewords (including data and ECC) onto the given QR Code.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="dataLen">Data length.</param>
    /// <param name="qrcode">QR Code buffer.</param>
    /// <remarks>
    /// This requires the initial state of the QR Code to be dark at function modules and light at codeword modules (including unused remainder bits).
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L580
    /// </remarks>
    private static void DrawCodewords(ReadOnlySpan<byte> data, int dataLen, Span<byte> qrcode)
    {
        int qrSize = GetSize(qrcode);
        int i = 0; // Bit index into the data
        // Do the funny zigzag scan
        for (int right = qrSize - 1; right >= 1; right -= 2)
        {
            // Index of right column in each column pair
            if (right == 6)
            {
                right = 5;
            }
            for (int vert = 0; vert < qrSize; vert++)
            {
                // Vertical counter
                for (int j = 0; j < 2; j++)
                {
                    int x = right - j; // Actual x coordinate
                    bool upward = ((right + 1) & 2) == 0;
                    int y = upward ? qrSize - 1 - vert : vert; // Actual y coordinate
                    if (!GetModuleBounded(qrcode, x, y) && i < dataLen * 8)
                    {
                        bool dark = GetBit(data[i >> 3], 7 - (i & 7));
                        SetModuleBounded(qrcode, x, y, dark);
                        i++;
                    }
                    // If this QR Code has any remainder bits (0 to 7), they were assigned as
                    // 0 / false / light by the constructor and are left unchanged by this method
                }
            }
        }
        Debug.Assert(i == dataLen * 8);
    }

    /// <summary>
    /// XORs the codeword modules in this QR Code with the given mask pattern and given pattern of function modules.
    /// </summary>
    /// <param name="functionModules">Function modules.</param>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <param name="mask">Mask.</param>
    /// <remarks>
    /// The codeword bits must be drawn
    /// before masking. Due to the arithmetic of XOR, calling <see cref="ApplyMask"/> with
    /// the same mask value a second time will undo the mask. A final well-formed
    /// QR Code needs exactly one (not zero, two, etc.) mask applied.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L611
    /// </remarks>
    private static void ApplyMask(ReadOnlySpan<byte> functionModules, Span<byte> qrCode, QRMask mask)
    {
        Debug.Assert(0 <= (int)mask && (int)mask <= 7); // Disallows QRMask.Auto
        int qrSize = GetSize(qrCode);
        for (int y = 0; y < qrSize; y++)
        {
            for (int x = 0; x < qrSize; x++)
            {
                if (GetModuleBounded(functionModules, x, y))
                {
                    continue;
                }
                bool invert;
                switch ((int)mask)
                {
                    case 0:
                        invert = (x + y) % 2 == 0;
                        break;
                    case 1:
                        invert = y % 2 == 0;
                        break;
                    case 2:
                        invert = x % 3 == 0;
                        break;
                    case 3:
                        invert = (x + y) % 3 == 0;
                        break;
                    case 4:
                        invert = (x / 3 + y / 2) % 2 == 0;
                        break;
                    case 5:
                        invert = x * y % 2 + x * y % 3 == 0;
                        break;
                    case 6:
                        invert = (x * y % 2 + x * y % 3) % 2 == 0;
                        break;
                    case 7:
                        invert = ((x + y) % 2 + x * y % 3) % 2 == 0;
                        break;
                    default:
                        Debug.Assert(false);
                        return;
                }
                bool val = GetModuleBounded(qrCode, x, y);
                SetModuleBounded(qrCode, x, y, val ^ invert);
            }
        }
    }

    /// <summary>
    /// Calculates and returns the penalty score based on state of the given QR Code's current modules.
    /// </summary>
    /// <param name="qrcode"></param>
    /// <returns></returns>
    /// <remarks>
    /// This is used by the automatic mask choice algorithm to find the mask pattern that yields the lowest score.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L639
    /// </remarks>
    private static int GetPenaltyScore(ReadOnlySpan<byte> qrcode)
    {
        int qrSize = GetSize(qrcode);
        int result = 0;
        Span<int> runHistory = stackalloc int[7];
        // Adjacent modules in row having same color, and finder-like patterns
        for (int y = 0; y < qrSize; y++)
        {
            bool runColor = false;
            int runX = 0;
            runHistory[0] = 0;
            for (int x = 0; x < qrSize; x++)
            {
                if (GetModuleBounded(qrcode, x, y) == runColor)
                {
                    runX++;
                    if (runX == 5)
                    {
                        result += PenaltyN1;
                    }
                    else if (runX > 5)
                    {
                        result++;
                    }
                }
                else
                {
                    FinderPenaltyAddHistory(runX, runHistory, qrSize);
                    if (!runColor)
                    {
                        result += FinderPenaltyCountPatterns(runHistory, qrSize) * PenaltyN3;
                    }
                    runColor = GetModuleBounded(qrcode, x, y);
                    runX = 1;
                }
            }
            result += FinderPenaltyTerminateAndCount(runColor, runX, runHistory, qrSize) * PenaltyN3;
        }
        // Adjacent modules in column having same color, and finder-like patterns
        for (int x = 0; x < qrSize; x++)
        {
            bool runColor = false;
            int runY = 0;
            runHistory[0] = 0;
            for (int y = 0; y < qrSize; y++)
            {
                if (GetModuleBounded(qrcode, x, y) == runColor)
                {
                    runY++;
                    if (runY == 5)
                    {
                        result += PenaltyN1;
                    }
                    else if (runY > 5)
                    {
                        result++;
                    }
                }
                else
                {
                    FinderPenaltyAddHistory(runY, runHistory, qrSize);
                    if (!runColor)
                    {
                        result += FinderPenaltyCountPatterns(runHistory, qrSize) * PenaltyN3;
                    }
                    runColor = GetModuleBounded(qrcode, x, y);
                    runY = 1;
                }
            }
            result += FinderPenaltyTerminateAndCount(runColor, runY, runHistory, qrSize) * PenaltyN3;
        }
        // 2*2 blocks of modules having same color
        for (int y = 0; y < qrSize - 1; y++)
        {
            for (int x = 0; x < qrSize - 1; x++)
            {
                bool color = GetModuleBounded(qrcode, x, y);
                if (color == GetModuleBounded(qrcode, x + 1, y) &&
                    color == GetModuleBounded(qrcode, x, y + 1) &&
                    color == GetModuleBounded(qrcode, x + 1, y + 1))
                {
                    result += PenaltyN2;
                }
            }
        }
        // Balance of dark and light modules
        int dark = 0;
        for (int y = 0; y < qrSize; y++)
        {
            for (int x = 0; x < qrSize; x++)
            {
                if (GetModuleBounded(qrcode, x, y))
                {
                    dark++;
                }
            }
        }
        int total = qrSize * qrSize; // Note that size is odd, so dark/total != 1/2
        // Compute the smallest integer k >= 0 such that (45 - 5k)% <= dark / total <= (55 + 5k)%
        int k = (Math.Abs(dark * 20 - total * 10) + total - 1) / total - 1;
        Debug.Assert(0 <= k && k <= 9);
        result += k * PenaltyN4;
        Debug.Assert(0 <= result && result <= 2568888L); // Non-tight upper bound based on default values of PenaltyN1, ..., N4
        return result;
    }

    /// <remarks>
    /// Can only be called immediately after a light run is added, and
    /// returns either 0, 1, or 2. A helper function for <see cref="GetPenaltyScore"/>.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L719
    /// </remarks>
    private static int FinderPenaltyCountPatterns(ReadOnlySpan<int> runHistory, int qrSize)
    {
        int n = runHistory[1];
        Debug.Assert(n <= qrSize * 3);
        bool core = n > 0 && runHistory[2] == n && runHistory[3] == n * 3 && runHistory[4] == n && runHistory[5] == n;
        // The maximum QR Code size is 177, hence the dark run length n <= 177.
        // Arithmetic is promoted to int, so n * 4 will not overflow.
        return (core && runHistory[0] >= n * 4 && runHistory[6] >= n ? 1 : 0) + (core && runHistory[6] >= n * 4 && runHistory[0] >= n ? 1 : 0);
    }

    /// <remarks>
    /// Must be called at the end of a line (row or column) of modules. A helper function for <see cref="GetPenaltyScore"/>.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L731
    /// </remarks>
    private static int FinderPenaltyTerminateAndCount(bool currentRunColor, int currentRunLength, Span<int> runHistory, int qrSize)
    {
        if (currentRunColor)
        {
            // Terminate dark run
            FinderPenaltyAddHistory(currentRunLength, runHistory, qrSize);
            currentRunLength = 0;
        }
        currentRunLength += qrSize; // Add light border to final run
        FinderPenaltyAddHistory(currentRunLength, runHistory, qrSize);
        return FinderPenaltyCountPatterns(runHistory, qrSize);
    }

    /// <summary>
    /// Pushes the given value to the front and drops the last value.
    /// </summary>
    /// <param name="currentRunLength"></param>
    /// <param name="runHistory"></param>
    /// <param name="qrSize"></param>
    /// <remarks>
    /// A helper function for <see cref="GetPenaltyScore"/>.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L743
    /// </remarks>
    private static void FinderPenaltyAddHistory(int currentRunLength, Span<int> runHistory, int qrSize)
    {
        if (runHistory[0] == 0)
        {
            currentRunLength += qrSize; // Add light border to initial run
        }
        // memmove
        for (int i = 5; i >= 0; i--)
        {
            runHistory[i + 1] = runHistory[i];
        }
        runHistory[0] = currentRunLength;
    }

    /// <summary>
    /// Gets the side length of the given QR Code, assuming that encoding succeeded.
    /// </summary>
    /// <param name="qrCode"></param>
    /// <returns></returns>
    /// <remarks>
    /// The result is in the range [21, 177]. Note that the length of the array buffer
    /// is related to the side length - every <paramref name="qrCode"/> must have length at least
    /// <see cref="GetBufferLengthForVersion"/>.
    /// </remarks>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L755
    /// </remarks>
    public static int GetSize(ReadOnlySpan<byte> qrCode)
    {
        int result = qrCode[0];
        Debug.Assert(MinimumVersion * 4 + 17 <= result && result <= MaximumVersion * 4 + 17);
        return result;
    }

    /// <summary>
    /// Gets the color of the module (pixel) at the given coordinates, which is false for light or true for dark.
    /// </summary>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>True for dark, false for light.</returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L765
    /// </remarks>
    public static bool GetModule(ReadOnlySpan<byte> qrCode, int x, int y)
    {
        int qrSize = qrCode[0];
        return 0 <= x && x < qrSize && 0 <= y && y < qrSize && GetModuleBounded(qrCode, x, y);
    }

    /// <summary>
    /// Gets the color of the module at the given coordinates, which must be in bounds.
    /// </summary>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <returns>True if module is dark.</returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L782
    /// </remarks>
    public static bool GetModuleBounded(ReadOnlySpan<byte> qrCode, int x, int y)
    {
        int qrSize = qrCode[0];
        Debug.Assert(21 <= qrSize && qrSize <= 177 && 0 <= x && x < qrSize && 0 <= y && y < qrSize);
        int index = y * qrSize + x;
        return GetBit(qrCode[(index >> 3) + 1], index & 7);
    }

    /// <summary>
    /// Sets the color of the module at the given coordinates, which must be in bounds.
    /// </summary>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="isDark">True if module is to be dark.</param>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L782
    /// </remarks>
    private static void SetModuleBounded(Span<byte> qrCode, int x, int y, bool isDark)
    {
        int qrSize = qrCode[0];
        Debug.Assert(21 <= qrSize && qrSize <= 177 && 0 <= x && x < qrSize && 0 <= y && y < qrSize);
        int index = y * qrSize + x;
        int bitIndex = index & 7;
        int byteIndex = (index >> 3) + 1;
        if (isDark)
        {
            qrCode[byteIndex] |= (byte)(1 << bitIndex);
        }
        else
        {
            qrCode[byteIndex] &= (byte)((1 << bitIndex) ^ 0xFF);
        }
    }


    /// <summary>
    /// Sets the color of the module at the given coordinates, doing nothing if out of bounds.
    /// </summary>
    /// <param name="qrCode">QR Code buffer.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="isDark">True if module is to be dark.</param>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L796
    /// </remarks>
    private static void SetModuleUnbounded(Span<byte> qrCode, int x, int y, bool isDark)
    {
        int qrSize = qrCode[0];
        if (0 <= x && x < qrSize && 0 <= y && y < qrSize)
        {
            SetModuleBounded(qrCode, x, y, isDark);
        }
    }

    /// <summary>
    /// Gets whether the i'th bit of <paramref name="x"/> is set to 1 when x &gt;= 0 and 0 &lt;= i &lt;= 14.
    /// </summary>
    /// <param name="x">X.</param>
    /// <param name="i">I.</param>
    /// <returns>True if i'th bit of <paramref name="x"/> is set to 1 when x &gt;= 0 and 0 &lt;= i &lt;= 14.</returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L804
    /// </remarks>
    private static bool GetBit(int x, int i)
    {
        return ((x >> i) & 1) != 0;
    }


    /// <summary>
    /// Tests whether the given string can be encoded as a segment in numeric mode.
    /// </summary>
    /// <param name="text">Text to evaluate.</param>
    /// <returns>True if the given string can be encoded as a segment in numeric mode.</returns>
    /// <remarks>
    /// A string is encodable iff each character is in the range 0 to 9.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L310
    /// </remarks>
    private static bool IsNumeric(ReadOnlySpan<byte> text)
    {
        if (text.Length == 0)
        {
            throw new ArgumentException("Cannot check an empty source buffer", nameof(text));
        }
        for (; !text.IsEmpty; text = text[1..])
        {
            byte c = text[0];
            if (c < '0' || c > '9')
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Tests whether the given string can be encoded as a segment in alphanumeric mode.
    /// </summary>
    /// <param name="text">Text to evaluate.</param>
    /// <returns>True if the given string can be encoded as a segment in alphanumeric mode.</returns>
    /// <remarks>
    /// A string is encodable iff each character is in the following set: 0 to 9, A to Z
    /// (uppercase only), space, dollar, percent, asterisk, plus, hyphen, period, slash, colon.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L318
    /// </remarks>
    private static bool IsAlphanumeric(ReadOnlySpan<byte> text)
    {
        if (text.Length == 0)
        {
            throw new ArgumentException("Cannot check an empty source buffer", nameof(text));
        }
        for (; !text.IsEmpty; text = text[1..])
        {
            if (!AlphanumericCharset.Contains(text[0]))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns the number of bytes needed for the data buffer of a segment containing the given number of characters using the given mode.
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="characters"></param>
    /// <returns>The number of bytes, or <see cref="Int32.MaxValue">Int32.MaxValue</see> on failure.</returns>
    /// <remarks>
    /// All valid results are in the range [0, 4096].
    ///  It is okay for the user to allocate more bytes for the buffer than needed.
    /// For byte mode, numChars measures the number of bytes, not Unicode code points.
    /// For ECI mode, numChars must be 0, and the worst-case number of bytes is returned.
    /// An actual ECI segment can have shorter data. For non-ECI modes, the result is exact.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L332
    /// </remarks>
    private static int CalculateSegmentBufferSize(QRMode mode, int characters)
    {
        int temp = CalculateSegmentBitLength(mode, characters);
        if (temp == int.MaxValue)
        {
            return int.MaxValue;
        }
        Debug.Assert(0 <= temp && temp <= short.MaxValue);
        return (temp + 7) / 8;
    }

    /// <summary>
    /// Gets a segment representing the given binary data encoded in byte mode.
    /// </summary>
    /// <param name="data">Input buffer.</param>
    /// <param name="buf">Buffer to encode to.</param>
    /// <returns>Created segment info.</returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L340
    /// </remarks>
    public static QRSegment MakeBytes(ReadOnlySpan<byte> data, Span<byte> buf)
    {
        if (data.Length == 0)
        {
            throw new ArgumentException("Cannot encode an empty source buffer", nameof(data));
        }
        QRSegment result;
        result.Mode = QRMode.Byte;
        result.BitLength = CalculateSegmentBitLength(result.Mode, data.Length);
        Debug.Assert(result.BitLength != int.MaxValue);
        result.NumChars = data.Length;
        if (data.Length > 0)
        {
            data.CopyTo(buf);
        }
        result.DataOffset = 0;
        return result;
    }

    /// <summary>
    /// Gets a segment representing the given string of decimal digits encoded in numeric mode.
    /// </summary>
    /// <param name="digits">Input string of decimal digits (ASCII / UTF-8).</param>
    /// <param name="buf">Buffer to encode to.</param>
    /// <returns>Created segment info.</returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L346
    /// </remarks>
    public static QRSegment MakeNumeric(ReadOnlySpan<byte> digits, Span<byte> buf)
    {
        if (digits.Length == 0)
        {
            throw new ArgumentException("Cannot encode an empty source buffer", nameof(digits));
        }
        QRSegment result = default;
        result.Mode = QRMode.Numeric;
        int bitLen = CalculateSegmentBitLength(result.Mode, digits.Length);
        Debug.Assert(bitLen != int.MaxValue);
        result.NumChars = digits.Length;
        if (bitLen > 0)
        {
            buf[..((bitLen + 7) / 8)].Clear();
        }
        result.BitLength = 0;
        uint accumData = 0;
        int accumCount = 0;
        for (; !digits.IsEmpty; digits = digits[1..])
        {
            byte c = digits[0];
            Debug.Assert('0' <= c && c <= '9');
            accumData = accumData * 10 + (uint)(c - '0');
            accumCount++;
            if (accumCount == 3)
            {
                AppendBitsToBuffer(accumData, 10, buf, ref result.BitLength);
                accumData = 0;
                accumCount = 0;
            }
        }
        if (accumCount > 0)
        {
            // 1 or 2 digits remaining
            AppendBitsToBuffer(accumData, accumCount * 3 + 1, buf, ref result.BitLength);
        }
        Debug.Assert(result.BitLength == bitLen);
        result.DataOffset = 0;
        return result;
    }

    /// <summary>
    /// Gets a segment representing the given text string encoded in alphanumeric mode.
    /// </summary>
    /// <param name="text">Input string of alphanumeric text.</param>
    /// <param name="buf">Buffer to encode to.</param>
    /// <returns>Created segment info.</returns>
    /// <remarks>
    /// The characters allowed are: 0 to 9, A to Z (uppercase only), space,
    /// dollar, percent, asterisk, plus, hyphen, period, slash, colon.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L354
    /// </remarks>
    public static QRSegment MakeAlphanumeric(ReadOnlySpan<byte> text, Span<byte> buf)
    {
        if (text.Length == 0)
        {
            throw new ArgumentException("Cannot encode an empty source buffer", nameof(text));
        }
        QRSegment result = default;
        result.Mode = QRMode.Alphanumeric;
        int bitLen = CalculateSegmentBitLength(result.Mode, text.Length);
        Debug.Assert(bitLen != int.MaxValue);
        result.NumChars = text.Length;
        if (bitLen > 0)
        {
            buf[..((bitLen + 7) / 8)].Clear();
        }
        result.BitLength = 0;
        uint accumData = 0;
        int accumCount = 0;
        for (; !text.IsEmpty; text = text[1..])
        {
            int idx = AlphanumericCharset.IndexOf(text[0]);
            Debug.Assert(idx != -1);
            accumData = accumData * 45 + (uint)idx;
            accumCount++;
            if (accumCount == 2)
            {
                AppendBitsToBuffer(accumData, 11, buf, ref result.BitLength);
                accumData = 0;
                accumCount = 0;
            }
        }
        if (accumCount > 0)
        {
            // 1 character remaining
            AppendBitsToBuffer(accumData, 6, buf, ref result.BitLength);
        }
        Debug.Assert(result.BitLength == bitLen);
        result.DataOffset = 0;
        return result;
    }

    /// <summary>
    /// Gets a segment representing an Extended Channel Interpretation (ECI) designator with the given assignment value.
    /// </summary>
    /// <param name="assignmentValue">Assignment value to encode.</param>
    /// <param name="buf">Buffer to encode to.</param>
    /// <returns>Created segment info.</returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.h?ts=4#L361
    /// </remarks>
    public static QRSegment MakeECI(int assignmentValue, Span<byte> buf)
    {
        if (assignmentValue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(assignmentValue));
        }
        QRSegment result = default;
        result.BitLength = 0;
        if (assignmentValue < 1 << 7)
        {
            buf[..1].Clear();
            AppendBitsToBuffer((uint)assignmentValue, 8, buf, ref result.BitLength);
        }
        else if (assignmentValue < 1 << 14)
        {
            buf[..2].Clear();
            AppendBitsToBuffer(2, 2, buf, ref result.BitLength);
            AppendBitsToBuffer((uint)assignmentValue, 14, buf, ref result.BitLength);
        }
        else if (assignmentValue < 1000000)
        {
            buf[..3].Clear();
            AppendBitsToBuffer(6, 3, buf, ref result.BitLength);
            AppendBitsToBuffer((uint)(assignmentValue >> 10), 11, buf, ref result.BitLength);
            AppendBitsToBuffer((uint)(assignmentValue & 0x3FF), 10, buf, ref result.BitLength);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(assignmentValue));
        }
        result.Mode = QRMode.ECI;
        result.NumChars = 0;
        result.DataOffset = 0;
        return result;
    }

    /// <summary>
    /// Calculates the number of bits needed to encode the given segments at the given version.
    /// </summary>
    /// <param name="segments">Segments.</param>
    /// <param name="version">Version number.</param>
    /// <returns>
    /// A non-negative number if successful,
    /// or <see cref="Int32.MaxValue">Int32.MaxValue</see> if a segment has too many characters to fit in its length field,
    /// or the total bits exceeds <see cref="Int16.MaxValue">Int16.MaxValue</see>.
    /// </returns>
    /// <remarks>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L991
    /// </remarks>
    private static int GetTotalBits(ReadOnlySpan<QRSegment> segments, int version)
    {
        if (segments.Length == 0)
        {
            throw new ArgumentException("Cannot check an empty source buffer", nameof(segments));
        }
        int result = 0;
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            int numChars = segment.NumChars;
            int bitLength = segment.BitLength;
            Debug.Assert(0 <= numChars && numChars <= short.MaxValue);
            Debug.Assert(0 <= bitLength && bitLength <= short.MaxValue);
            int ccBits = GetCharCountBits(segment.Mode, version);
            Debug.Assert(0 <= ccBits && ccBits <= 16);
            if (numChars >= 1 << ccBits)
            {
                // The segment's length doesn't fit the field's bit width
                return int.MaxValue;
            }
            result += 4 + ccBits + bitLength;
            if (result > short.MaxValue)
            {
                return int.MaxValue;
            }
        }
        Debug.Assert(0 <= result);
        return result;
    }

    /// <summary>
    /// Gets the bit width of the character count field for a segment in the given mode
    /// in a QR Code at the given version number.
    /// </summary>
    /// <param name="mode">Mode.</param>
    /// <param name="version">Version number.</param>
    /// <returns>The bit width of the character count field for a segment.</returns>
    /// <remarks>
    /// The result is in the range [0, 16].
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L1014
    /// </remarks>
    private static int GetCharCountBits(QRMode mode, int version)
    {
        Debug.Assert(MinimumVersion <= version && version <= MaximumVersion);
        int i = (version + 7) / 17;
        return mode switch
        {
            QRMode.Numeric => CharCountBitsNumeric[i],
            QRMode.Alphanumeric => CharCountBitsAlphanumeric[i],
            QRMode.Byte => CharCountBitsByte[i],
            QRMode.Kanji => CharCountBitsKanji[i],
            QRMode.ECI => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    private static ReadOnlySpan<byte> CharCountBitsNumeric => new byte[] { 10, 12, 14 };
    private static ReadOnlySpan<byte> CharCountBitsAlphanumeric => new byte[] { 9, 11, 13 };
    private static ReadOnlySpan<byte> CharCountBitsByte => new byte[] { 8, 16, 16 };
    private static ReadOnlySpan<byte> CharCountBitsKanji => new byte[] { 8, 10, 12 };

    /// <summary>
    /// Gets the number of 8-bit codewords that can be used for storing data (not ECC),
    /// for the given version number and error correction level.
    /// </summary>
    /// <param name="version">Version number.</param>
    /// <param name="eccLevel">Error correction level.</param>
    /// <returns>The number of 8-bit codewords that can be used for storing data (not ECC).</returns>
    /// <remarks>
    /// The result is in the range [9, 2956].
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L330
    /// </remarks>
    private static int GetDataCodewords(int version, QRECCLevel eccLevel)
    {
        int v = version, e = (int)eccLevel;
        Debug.Assert(0 <= e && e < 4);
        return GetRawDataModules(v) / 8 - GetEccCodewordsPerBlock(e, v) * GetNumErrorCorrectionBlocks(e, v);
    }

    /// <summary>
    /// Returns the number of data bits that can be stored in a QR Code of the given version number, after
    /// all function modules are excluded.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    /// <remarks>
    /// This includes remainder bits, so it might not be a multiple of 8.
    /// The result is in the range [208, 29648]. This could be implemented as a 40-entry lookup table.
    /// <br/>
    /// https://github.com/nayuki/QR-Code-generator/blob/2643e824eb15064662e6c4d99b010740275a0be1/c/qrcodegen.c?ts=4#L342
    /// </remarks>
    private static int GetRawDataModules(int version)
    {
        Debug.Assert(MinimumVersion <= version && version <= MaximumVersion);
        int result = (16 * version + 128) * version + 64;
        if (version >= 2)
        {
            int numAlign = version / 7 + 2;
            result -= (25 * numAlign - 10) * numAlign - 55;
            if (version >= 7)
            {
                result -= 36;
            }
        }
        Debug.Assert(208 <= result && result <= 29648);
        return result;
    }

    /// <summary>
    ///  Returns the number of data bits needed to represent a segment
    /// containing the given number of characters using the given mode.
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="characters"></param>
    /// <returns>
    /// The number of data bits needed to represent a segment
    /// containing the given number of characters using the given mode,
    /// or <see cref="Int32.MaxValue">Int32.MaxValue</see> on failure.
    /// </returns>
    /// <remarks>
    /// All valid results are in the range [0, <see cref="Int16.MaxValue">Int16.MaxValue</see>].
    /// For byte mode, numChars measures the number of bytes, not Unicode code points.
    /// For ECI mode, numChars must be 0, and the worst-case number of bits is returned.
    /// An actual ECI segment can have shorter data. For non-ECI modes, the result is exact.
    /// </remarks>
    private static int CalculateSegmentBitLength(QRMode mode, int characters)
    {
        if (characters > short.MaxValue)
        {
            return int.MaxValue;
        }
        int result = mode switch
        {
            QRMode.Numeric => (characters * 10 + 2) / 3, // ceil(10 / 3 * n)
            QRMode.Alphanumeric => (characters * 11 + 1) / 2, // ceil(11 / 2 * n)
            QRMode.Byte => characters * 8,
            QRMode.Kanji => characters * 13,
            QRMode.ECI when characters == 0 => 3 * 8,
            _ => int.MaxValue
        };
        Debug.Assert(result >= 0);
        if (result > short.MaxValue)
        {
            return int.MaxValue;
        }
        return result;
    }

    private static void AppendBitsToBuffer(uint value, int numBits, Span<byte> buffer, ref int bitLength)
    {
        Debug.Assert(0 <= numBits && numBits <= 16 && value >> numBits == 0);
        for (int i = numBits - 1; i >= 0; i--, bitLength++)
        {
            buffer[bitLength >> 3] |= (byte)(((value >> i) & 1) << (7 - (bitLength & 7)));
        }
    }

    private static void ThrowForInvalidVersion(int version, [CallerArgumentExpression("version")] string? paramName = null)
    {
        if (version is < MinimumVersion or > MaximumVersion)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Version number {version} is outside the range of valid versions [{MinimumVersion}, {MaximumVersion}].");
        }
    }
}
