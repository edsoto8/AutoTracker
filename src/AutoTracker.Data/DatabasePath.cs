namespace AutoTracker.Data;

public static class DatabasePath
{
    public static string GetConnectionString()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".autotracker");
        Directory.CreateDirectory(folder);
        return $"Data Source={Path.Combine(folder, "autotracker.db")}";
    }
}
