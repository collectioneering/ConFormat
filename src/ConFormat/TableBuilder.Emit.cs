namespace ConFormat;

public partial class TableBuilder
{
    /// <summary>
    /// Delegate for retrieving a 1-column output for a given object.
    /// </summary>
    /// <typeparam name="T">item type.</typeparam>
    public delegate ValueTuple<string> Column1Delegate<in T>(T item);

    /// <summary>
    /// Delegate for retrieving a 2-column output for a given object.
    /// </summary>
    /// <typeparam name="T">item type.</typeparam>
    public delegate ValueTuple<string, string> Column2Delegate<in T>(T item);

    /// <summary>
    /// Delegate for retrieving a 3-column output for a given object.
    /// </summary>
    /// <typeparam name="T">item type.</typeparam>
    public delegate ValueTuple<string, string, string> Column3Delegate<in T>(T item);

    /// <summary>
    /// Delegate for retrieving a 4-column output for a given object.
    /// </summary>
    /// <typeparam name="T">item type.</typeparam>
    public delegate ValueTuple<string, string, string, string> Column4Delegate<in T>(T item);

    /// <summary>
    /// Delegate for retrieving a 5-column output for a given object.
    /// </summary>
    /// <typeparam name="T">item type.</typeparam>
    public delegate ValueTuple<string, string, string, string, string> Column5Delegate<in T>(T item);

    /// <summary>
    /// Delegate for retrieving a 6-column output for a given object.
    /// </summary>
    /// <typeparam name="T">item type.</typeparam>
    public delegate ValueTuple<string, string, string, string, string, string> Column6Delegate<in T>(T item);

    /// <summary>
    /// Delegate for retrieving a 7-column output for a given object.
    /// </summary>
    /// <typeparam name="T">item type.</typeparam>
    public delegate ValueTuple<string, string, string, string, string, string, string> Column7Delegate<in T>(T item);

    /// <summary>
    /// Emits a 1-column table output.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="enumerable">Elements.</param>
    /// <param name="elementDelegate">Delegate to handle elementDelegate.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if this table is not configured for 1-column output.</exception>
    public void Emit<T>(TextWriter textWriter, IEnumerable<T> enumerable, Column1Delegate<T> elementDelegate)
    {
        if (_entries.Length != 1)
        {
            throw new InvalidOperationException("Element function does not match this instance's number of columns");
        }
        EmitTitleRow(textWriter);
        foreach (var item in enumerable)
        {
            var row = CreateRowEmitter();
            var elementTuple = elementDelegate(item);
            row.Emit(textWriter, elementTuple.Item1);
        }
        EmitEndRow(textWriter);
    }

    /// <summary>
    /// Emits a 2-column table output.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="enumerable">Elements.</param>
    /// <param name="elementDelegate">Delegate to handle elementDelegate.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if this table is not configured for 2-column output.</exception>
    public void Emit<T>(TextWriter textWriter, IEnumerable<T> enumerable, Column2Delegate<T> elementDelegate)
    {
        if (_entries.Length != 2)
        {
            throw new InvalidOperationException("Element function does not match this instance's number of columns");
        }
        EmitTitleRow(textWriter);
        foreach (var item in enumerable)
        {
            var row = CreateRowEmitter();
            var elementTuple = elementDelegate(item);
            row.Emit(textWriter, elementTuple.Item1);
            row.Emit(textWriter, elementTuple.Item2);
        }
        EmitEndRow(textWriter);
    }

    /// <summary>
    /// Emits a 3-column table output.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="enumerable">Elements.</param>
    /// <param name="elementDelegate">Delegate to handle elementDelegate.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if this table is not configured for 3-column output.</exception>
    public void Emit<T>(TextWriter textWriter, IEnumerable<T> enumerable, Column3Delegate<T> elementDelegate)
    {
        if (_entries.Length != 3)
        {
            throw new InvalidOperationException("Element function does not match this instance's number of columns");
        }
        EmitTitleRow(textWriter);
        foreach (var item in enumerable)
        {
            var row = CreateRowEmitter();
            var elementTuple = elementDelegate(item);
            row.Emit(textWriter, elementTuple.Item1);
            row.Emit(textWriter, elementTuple.Item2);
            row.Emit(textWriter, elementTuple.Item3);
        }
        EmitEndRow(textWriter);
    }

    /// <summary>
    /// Emits a 4-column table output.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="enumerable">Elements.</param>
    /// <param name="elementDelegate">Delegate to handle elementDelegate.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if this table is not configured for 4-column output.</exception>
    public void Emit<T>(TextWriter textWriter, IEnumerable<T> enumerable, Column4Delegate<T> elementDelegate)
    {
        if (_entries.Length != 4)
        {
            throw new InvalidOperationException("Element function does not match this instance's number of columns");
        }
        EmitTitleRow(textWriter);
        foreach (var item in enumerable)
        {
            var row = CreateRowEmitter();
            var elementTuple = elementDelegate(item);
            row.Emit(textWriter, elementTuple.Item1);
            row.Emit(textWriter, elementTuple.Item2);
            row.Emit(textWriter, elementTuple.Item3);
            row.Emit(textWriter, elementTuple.Item4);
        }
        EmitEndRow(textWriter);
    }

    /// <summary>
    /// Emits a 5-column table output.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="enumerable">Elements.</param>
    /// <param name="elementDelegate">Delegate to handle elementDelegate.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if this table is not configured for 5-column output.</exception>
    public void Emit<T>(TextWriter textWriter, IEnumerable<T> enumerable, Column5Delegate<T> elementDelegate)
    {
        if (_entries.Length != 5)
        {
            throw new InvalidOperationException("Element function does not match this instance's number of columns");
        }
        EmitTitleRow(textWriter);
        foreach (var item in enumerable)
        {
            var row = CreateRowEmitter();
            var elementTuple = elementDelegate(item);
            row.Emit(textWriter, elementTuple.Item1);
            row.Emit(textWriter, elementTuple.Item2);
            row.Emit(textWriter, elementTuple.Item3);
            row.Emit(textWriter, elementTuple.Item4);
            row.Emit(textWriter, elementTuple.Item5);
        }
        EmitEndRow(textWriter);
    }

    /// <summary>
    /// Emits a 6-column table output.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="enumerable">Elements.</param>
    /// <param name="elementDelegate">Delegate to handle elementDelegate.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if this table is not configured for 6-column output.</exception>
    public void Emit<T>(TextWriter textWriter, IEnumerable<T> enumerable, Column6Delegate<T> elementDelegate)
    {
        if (_entries.Length != 6)
        {
            throw new InvalidOperationException("Element function does not match this instance's number of columns");
        }
        EmitTitleRow(textWriter);
        foreach (var item in enumerable)
        {
            var row = CreateRowEmitter();
            var elementTuple = elementDelegate(item);
            row.Emit(textWriter, elementTuple.Item1);
            row.Emit(textWriter, elementTuple.Item2);
            row.Emit(textWriter, elementTuple.Item3);
            row.Emit(textWriter, elementTuple.Item4);
            row.Emit(textWriter, elementTuple.Item5);
            row.Emit(textWriter, elementTuple.Item6);
        }
        EmitEndRow(textWriter);
    }

    /// <summary>
    /// Emits a 7-column table output.
    /// </summary>
    /// <param name="textWriter">Writer to write to.</param>
    /// <param name="enumerable">Elements.</param>
    /// <param name="elementDelegate">Delegate to handle elementDelegate.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if this table is not configured for 7-column output.</exception>
    public void Emit<T>(TextWriter textWriter, IEnumerable<T> enumerable, Column7Delegate<T> elementDelegate)
    {
        if (_entries.Length != 7)
        {
            throw new InvalidOperationException("Element function does not match this instance's number of columns");
        }
        EmitTitleRow(textWriter);
        foreach (var item in enumerable)
        {
            var row = CreateRowEmitter();
            var elementTuple = elementDelegate(item);
            row.Emit(textWriter, elementTuple.Item1);
            row.Emit(textWriter, elementTuple.Item2);
            row.Emit(textWriter, elementTuple.Item3);
            row.Emit(textWriter, elementTuple.Item4);
            row.Emit(textWriter, elementTuple.Item5);
            row.Emit(textWriter, elementTuple.Item6);
            row.Emit(textWriter, elementTuple.Item7);
        }
        EmitEndRow(textWriter);
    }
}
