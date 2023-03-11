using System.Runtime.InteropServices;
using System.Text;
using EA;

namespace ConFormat;

internal static class StringFillUtil
{
    public static int ComputeLength(string content)
    {
        return EastAsianWidth.GetWidth(content);
    }

    public static void FillLeft(string content, StringBuilder stringBuilder, int width, int scrollIndex = 0)
    {
        if (width < 1)
        {
            return;
        }
        using var enumerator = content.EnumerateRunes();
        if (!enumerator.MoveNext())
        {
            return;
        }
        var prevRune = enumerator.Current;
        Span<char> buf = stackalloc char[2];
        while (enumerator.MoveNext())
        {
            int prevRuneWidth = EastAsianWidth.GetWidth(prevRune.Value);
            if (scrollIndex > 0)
            {
                if (prevRuneWidth >= scrollIndex)
                {
                    scrollIndex = 0;
                }
                else
                {
                    scrollIndex -= prevRuneWidth;
                }
                prevRune = enumerator.Current;
                continue;
            }
            if (prevRuneWidth + 1 > width)
            {
                stringBuilder.Append('…');
                PadRemaining(stringBuilder, width - 1);
                return;
            }
            prevRune.EncodeToUtf16(buf);
            stringBuilder.Append(buf);
            buf.Clear();
            width -= prevRuneWidth;
            prevRune = enumerator.Current;
        }
        if (scrollIndex > 0)
        {
            PadRemaining(stringBuilder, width);
            return;
        }
        int lastRuneWidth = EastAsianWidth.GetWidth(prevRune.Value);
        if (lastRuneWidth > width)
        {
            stringBuilder.Append('…');
            PadRemaining(stringBuilder, width - 1);
            return;
        }
        prevRune.EncodeToUtf16(buf);
        stringBuilder.Append(buf);
        PadRemaining(stringBuilder, width - lastRuneWidth);
    }

    public static void FillRight(string content, StringBuilder stringBuilder, int width)
    {
        if (width < 1)
        {
            return;
        }
        const int spanCount = 256;
        if (content.Length <= spanCount)
        {
            // it is impossible to drive without a license!
            // runes each take at least one utf16 code unit, so using length as max theoretical works
            Span<Rune> runes = stackalloc Rune[spanCount];
            int i = 0;
            foreach (var rune in content.EnumerateRunes())
            {
                runes[i++] = rune;
            }
            FillRight(runes[..i], stringBuilder, width);
        }
        else
        {
            List<Rune> list = new(spanCount * 2);
            list.AddRange(content.EnumerateRunes());
            FillRight(CollectionsMarshal.AsSpan(list), stringBuilder, width);
        }
    }

    private static void FillRight(Span<Rune> span, StringBuilder stringBuilder, int width)
    {
        if (width < 1)
        {
            return;
        }
        if (span.Length == 0)
        {
            PadRemaining(stringBuilder, width);
            return;
        }
        int availableWidth = width;
        int startPos;
        for (startPos = span.Length - 1; startPos >= 1; startPos--)
        {
            int runeWidth = EastAsianWidth.GetWidth(span[startPos].Value);
            if (availableWidth < runeWidth + 1)
            {
                break;
            }
            availableWidth -= runeWidth;
        }
        if (startPos == 0)
        {
            int firstRuneWidth = EastAsianWidth.GetWidth(span[0].Value);
            if (firstRuneWidth > availableWidth)
            {
                startPos++;
                stringBuilder.Append('…');
                PadRemaining(stringBuilder, availableWidth - 1);
            }
            else
            {
                availableWidth -= firstRuneWidth;
                PadRemaining(stringBuilder, availableWidth);
            }
        }
        else
        {
            startPos++;
            if (availableWidth > 0)
            {
                stringBuilder.Append('…');
                PadRemaining(stringBuilder, availableWidth - 1);
            }
        }
        Span<char> buf = stackalloc char[2];
        for (int i = startPos; i < span.Length; i++)
        {
            span[i].EncodeToUtf16(buf);
            stringBuilder.Append(buf);
            buf.Clear();
        }
    }

    internal static void PadRemaining(StringBuilder stringBuilder, int remaining, char c = ' ')
    {
        stringBuilder.Append(c, remaining);
    }
}
