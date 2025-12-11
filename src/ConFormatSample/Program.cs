using System.Diagnostics;
using ConFormat;
using ConFormatSample;

// Demo is a verb
DemoQR();
await DemoDownloadBarAsync();
DemoTable();
await Task.Delay(TimeSpan.FromSeconds(2));
await DemoMultiDownloadBarAsync();

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

async Task DownloadRandomChainAsync(MultiBarContext<string> bar, int n, double mean, double range)
{
    for (int i = 0; i < n; i++)
    {
        await DownloadEntryAsync(bar, Guid.NewGuid().ToString("N"), Random.Shared.NextDouble() * range + (mean - range * 0.5));
    }
}

async Task DownloadEntryAsync(MultiBarContext<string> bar, string key, double duration)
{
    bar.Allocate(key);
    try
    {
        Stopwatch sw = new();
        sw.Start();
        var content = TimedDownloadPrefabContentFiller.Create($"Downloading {key}...");
        const int n = 64;
        double div = duration / n;
        bar.Write(key, ref content);
        for (int i = 0; i < n; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(div));
            content.SetProgress((float)(i + 1) / n);
            content.SetDuration(sw.Elapsed);
            bar.Write(key, ref content);
        }
        content.SetProgress(1);
        content.SetDuration(sw.Elapsed);
        sw.Stop();
        bar.Clear(key);
    }
    finally
    {
        bar.Remove(key);
    }
}

async Task DemoMultiDownloadBarAsync()
{
    using (var bar = MultiBarContext<string>.Create(
               Console.Out,
               false,
               static () => Console.IsOutputRedirected,
               static () => Console.BufferWidth,
               static () => Console.WindowHeight,
               Console.CursorTop - Console.WindowTop,
               TimeSpan.FromSeconds(0.05)))
    {
        await Task.WhenAll(
            DownloadRandomChainAsync(bar, 5, 2, 0.3),
            DownloadRandomChainAsync(bar, 5, 2, 0.3),
            DownloadRandomChainAsync(bar, 3, 2, 0.3),
            DownloadRandomChainAsync(bar, 3, 2, 0.3),
            DownloadRandomChainAsync(bar, 3, 2, 0.3));
    }
    await Task.Delay(TimeSpan.FromSeconds(1));
    using (var bar = MultiBarContext<string>.Create(
               Console.Out,
               false,
               static () => Console.IsOutputRedirected,
               static () => Console.BufferWidth,
               static () => Console.WindowHeight,
               Console.CursorTop - Console.WindowTop,
               TimeSpan.FromSeconds(0.05)))
    {
        var content = new StringContentFiller("f", ContentAlignment.Left);
        bar.Allocate("a");
        bar.Allocate("b");
        bar.Allocate("c");
        bar.Write("a", ref content);
        bar.Write("b", ref content);
        bar.Write("c", ref content);
        await Task.Delay(TimeSpan.FromSeconds(1));
        bar.ClearAll();
    }
    using (var bar = MultiBarContext<string>.Create(
               Console.Out,
               false,
               static () => Console.IsOutputRedirected,
               static () => Console.BufferWidth,
               static () => Console.WindowHeight,
               Console.CursorTop - Console.WindowTop,
               TimeSpan.FromSeconds(0.05)))
    {
        for (int i = 0; i < 20; i++)
        {
            var content = new StringContentFiller($"f{i}", ContentAlignment.Left);
            bar.Allocate($"v{i}");
            bar.Write($"v{i}", ref content);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
        }
        bar.ClearAll();
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
