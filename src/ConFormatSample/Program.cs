using System.Diagnostics;
using ConFormat;

using (var bar = BarContext.Create(Console.Out, false, () => Console.IsOutputRedirected, () => Console.BufferWidth, TimeSpan.FromSeconds(0.05)))
{
    Stopwatch sw = new();
    sw.Start();
    var content = TimedDownloadPrefabContentFiller.Create("Downloading...");
    const int n = 16;
    bar.Write(content);
    for (int i = 0; i < n; i++)
    {
        await Task.Delay(TimeSpan.FromSeconds(0.1));
        content.SetProgress((float)(i + 1) / n);
        content.SetDuration(sw.Elapsed);
        bar.Update(content);
    }
    content.SetProgress(1);
    content.SetDuration(sw.Elapsed);
    sw.Stop();
    bar.Write(content);
    await Task.Delay(TimeSpan.FromSeconds(1));
    bar.Clear();
}

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

record Content(string Id, string Size, string Color);
