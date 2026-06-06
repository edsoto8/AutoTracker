namespace AutoTracker.ImportExport;

public class ImportResult
{
    public int Imported { get; set; }
    public List<string> Skipped { get; set; } = [];

    public int SkippedCount => Skipped.Count;
    public int Total => Imported + SkippedCount;
}
