using System.Diagnostics;
using ConFormat;

// Demo is a verb
DemoQR();
await DemoDownloadBarAsync();
DemoTable();

void DemoQR()
{
    const int qrVersion = 5;
    Console.Clear();
    if (!QRWriter.TryWriteFromBinary(Console.Out, "https://youtu.be/uK_DXptM7pA"u8, QRECCLevel.Low, maxVersion: qrVersion))
    {
        Console.Error.WriteLine("Failed to encode QR code as bytes");
    }
    Console.SetCursorPosition(40, 0);
    if (!QRWriter.TryWriteFromText(Console.Out, "https://youtu.be/VQqO20pVhpk"u8, QRECCLevel.Low, maxVersion: qrVersion))
    {
        Console.Error.WriteLine("Failed to encode text QR code");
    }
    Console.SetCursorPosition(0, 14);
}

async Task DemoDownloadBarAsync()
{
    using (var bar = BarContext.Create(Console.Out, false, () => Console.IsOutputRedirected, () => Console.BufferWidth, TimeSpan.FromSeconds(0.05)))
    {
        Stopwatch sw = new();
        sw.Start();
        var content = TimedDownloadPrefabContentFiller.Create("Retrieving metadata for \"【MV】Killer neuron / 佐高陵平 feat. 藍月なくる\"");
        const int n = 64;
        bar.Write(ref content);
        for (int i = 0; i < n; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.05));
            content.SetProgress((float)(i + 1) / n);
            content.SetDuration(sw.Elapsed);
            bar.Write(ref content);
        }
        content.SetProgress(1);
        content.SetDuration(sw.Elapsed);
        sw.Stop();
        bar.Write(ref content);
        await Task.Delay(TimeSpan.FromSeconds(1));
        bar.Clear();
    }
}

void DemoTable()
{
    var tableBuilder = new TableBuilder(new UnicodeTableBuilderOptions(),
        ("ID", 10),
        ("Size", 7),
        ("Color", 15));
    Content[] contents =
    {
        new("X1", "Large", "Blue"), //
        new("X2", "Medium", "Green"), //
        new("X3", "Super Big Ultra", "Orange"), //
        new("X4", "Small", "Yellow"), //
    };
    tableBuilder.Emit(Console.Out, contents, v => (v.Id, v.Size, v.Color));
}

record Content(string Id, string Size, string Color);
