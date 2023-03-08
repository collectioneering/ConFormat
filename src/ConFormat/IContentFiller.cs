using System.Text;

namespace ConFormat;

/// <summary>
/// Represents an item that can append content to a <see cref="StringBuilder"/> with exactly the specified width.
/// </summary>
public interface IContentFiller
{
    /// <summary>
    /// Appends content to the supplied <see cref="StringBuilder"/> with exactly the specified width.
    /// </summary>
    /// <param name="stringBuilder"><see cref="StringBuilder"/> to append to.</param>
    /// <param name="width">Width to fill.</param>
    void Fill(StringBuilder stringBuilder, int width);
}
