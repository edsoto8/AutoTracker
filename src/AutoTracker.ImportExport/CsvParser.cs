using System.Text;

namespace AutoTracker.ImportExport;

public static class CsvParser
{
    public static IEnumerable<string[]> ReadRows(string content)
    {
        var normalized = content.ReplaceLineEndings("\n");
        var record = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in normalized)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                record.Append(c);
            }
            else if (c == '\n' && !inQuotes)
            {
                var line = record.ToString();
                record.Clear();
                if (!string.IsNullOrWhiteSpace(line))
                    yield return ParseLine(line);
            }
            else
            {
                record.Append(c);
            }
        }

        var remaining = record.ToString();
        if (!string.IsNullOrWhiteSpace(remaining))
            yield return ParseLine(remaining);
    }

    private static string[] ParseLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                { sb.Append('"'); i++; }
                else if (c == '"')
                { inQuotes = false; }
                else
                { sb.Append(c); }
            }
            else
            {
                if (c == '"') { inQuotes = true; }
                else if (c == ',') { fields.Add(sb.ToString()); sb.Clear(); }
                else { sb.Append(c); }
            }
        }
        fields.Add(sb.ToString());
        return fields.ToArray();
    }

    public static string Escape(string? value)
    {
        if (value is null) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
