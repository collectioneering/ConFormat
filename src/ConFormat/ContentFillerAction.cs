namespace ConFormat;

/// <summary>
/// Represents an action to take on an <see cref="IContentFiller"/>.
/// </summary>
/// <typeparam name="T">Content filler type.</typeparam>
public delegate void ContentFillerAction<T>(ref T contentFiller) where T : IContentFiller;
