using System.Text;
using ConFormat;

public struct TimedDownloadPrefabContentFiller : IContentFiller
{
    public static TimedDownloadPrefabContentFiller Create(string initialName)
    {
        return new TimedDownloadPrefabContentFiller(initialName);
    }

    public BorderContentFiller<SplitContentFiller<StringContentFiller, FixedSplitContentFiller<StringContentFiller, FixedSplitContentFiller<ColorContentFiller<ProgressContentFiller>, StringContentFiller>>>> Content;

    public TimedDownloadPrefabContentFiller(string initialName)
    {
        Content = BorderContentFiller.Create("[", "]",
            SplitContentFiller.Create("|", 0.25f, 0.75f,
                StringContentFiller.Create(initialName, ContentAlignment.Left),
                FixedSplitContentFiller.Create("|", 7, 0,
                    StringContentFiller.Create("0.0s", ContentAlignment.Right),
                    FixedSplitContentFiller.Create("|", 6, 1,
                        ColorContentFiller.Create(ConsoleColor.Green, ProgressContentFiller.Create()),
                        StringContentFiller.Create("0.0%", ContentAlignment.Right)))));
    }

    public void SetName(string name)
    {
        Content.Content.ContentLeft = new StringContentFiller(name, ContentAlignment.Left);
    }

    public void SetDuration(TimeSpan duration)
    {
        Content.Content.ContentRight.ContentLeft = new StringContentFiller($"{duration.TotalSeconds:F1}s", ContentAlignment.Right);
    }

    public void SetProgress(float progress)
    {
        progress = Math.Clamp(progress, 0.0f, 1.0f);
        Content.Content.ContentRight.ContentRight.ContentLeft.Content.Progress = progress;
        Content.Content.ContentRight.ContentRight.ContentRight = new StringContentFiller($"{100.0f * progress:F1}%", ContentAlignment.Right);
    }

    public void Fill(StringBuilder stringBuilder, int width)
    {
        Content.Fill(stringBuilder, width);
    }
}
