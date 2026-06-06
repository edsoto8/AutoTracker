using AutoTracker.ImportExport;

namespace AutoTracker.ImportExport.Tests;

public class CsvParserTests
{
    [Fact]
    public void ReadRows_SimpleRows_ParsesCorrectly()
    {
        const string csv = "a,b,c\n1,2,3\n4,5,6";
        var rows = CsvParser.ReadRows(csv).ToList();

        Assert.Equal(3, rows.Count);
        Assert.Equal(["a", "b", "c"], rows[0]);
        Assert.Equal(["1", "2", "3"], rows[1]);
    }

    [Fact]
    public void ReadRows_QuotedFieldWithNewline_TreatedAsSingleRecord()
    {
        const string csv = "Id,Notes\n1,\"line one\nline two\"\n2,normal";
        var rows = CsvParser.ReadRows(csv).ToList();

        Assert.Equal(3, rows.Count);
        Assert.Equal("line one\nline two", rows[1][1]);
    }

    [Fact]
    public void ReadRows_QuotedFieldWithComma_ParsesCorrectly()
    {
        const string csv = "Id,Name\n1,\"Smith, Jr.\"\n2,Jones";
        var rows = CsvParser.ReadRows(csv).ToList();

        Assert.Equal(3, rows.Count);
        Assert.Equal("Smith, Jr.", rows[1][1]);
    }

    [Fact]
    public void ReadRows_EscapedQuoteInsideField_ParsesCorrectly()
    {
        const string csv = "Id,Notes\n1,\"say \"\"hello\"\"\"\n2,plain";
        var rows = CsvParser.ReadRows(csv).ToList();

        Assert.Equal(3, rows.Count);
        Assert.Equal("say \"hello\"", rows[1][1]);
    }

    [Fact]
    public void ReadRows_EmptyLines_Skipped()
    {
        const string csv = "a,b\n\n1,2\n\n3,4\n";
        var rows = CsvParser.ReadRows(csv).ToList();

        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public void Escape_ValueWithComma_Quoted()
    {
        Assert.Equal("\"a,b\"", CsvParser.Escape("a,b"));
    }

    [Fact]
    public void Escape_ValueWithNewline_Quoted()
    {
        Assert.Equal("\"a\nb\"", CsvParser.Escape("a\nb"));
    }

    [Fact]
    public void Escape_NullValue_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, CsvParser.Escape(null));
    }

    [Fact]
    public void Escape_PlainValue_Unquoted()
    {
        Assert.Equal("hello", CsvParser.Escape("hello"));
    }
}
