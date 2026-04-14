using GEditor.Core.Buffer;
using GEditor.Core.Selection;
using Xunit;

namespace GEditor.Tests.Editing;

public class ColumnEditModeTests
{
    [Fact]
    public void InsertAtColumns_ShouldInsertAtMultiplePositions()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("a\nb\nc");

        var positions = new List<(int line, int column)>
        {
            (0, 0),
            (1, 0),
            (2, 0)
        };

        buffer.InsertAtColumns(positions, "X");

        var lines = buffer.Lines;
        Assert.Equal("Xa", lines[0]);
        Assert.Equal("Xb", lines[1]);
        Assert.Equal("Xc", lines[2]);
    }

    [Fact]
    public void InsertAtColumns_ShouldHandleEmptyText()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("a\nb\nc");

        var positions = new List<(int line, int column)>
        {
            (0, 0),
            (1, 0)
        };

        buffer.InsertAtColumns(positions, "");

        var lines = buffer.Lines;
        Assert.Equal("a", lines[0]);
        Assert.Equal("b", lines[1]);
        Assert.Equal("c", lines[2]);
    }

    [Fact]
    public void DeleteAtColumns_ShouldDeleteMultipleColumnRanges()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\nghijkl");

        var ranges = new List<(int line, int column, int length)>
        {
            (0, 1, 2),  // 删除第0行的列1-2 ("bc")
            (1, 2, 2)   // 删除第1行的列2-3 ("ij")
        };

        buffer.DeleteAtColumns(ranges);

        var lines = buffer.Lines;
        Assert.Equal("adef", lines[0]);
        Assert.Equal("ghkl", lines[1]);
    }

    [Fact]
    public void DeleteAtColumns_ShouldReturnDeletedTexts()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\nghijkl");

        var ranges = new List<(int line, int column, int length)>
        {
            (0, 1, 3),
            (1, 2, 2)
        };

        var deleted = buffer.DeleteAtColumns(ranges);

        Assert.Equal(2, deleted.Length);
        Assert.Equal("bcd", deleted[0]);
        Assert.Equal("ij", deleted[1]);
    }

    [Fact]
    public void DeleteAtColumns_ShouldHandleOutOfBoundsColumns()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("ab\nabc");

        var ranges = new List<(int line, int column, int length)>
        {
            (0, 1, 10),  // 超出第0行的长度
            (1, 2, 5)    // 超出第1行的长度
        };

        buffer.DeleteAtColumns(ranges);

        var lines = buffer.Lines;
        Assert.Equal("a", lines[0]);
        Assert.Equal("ab", lines[1]);
    }

    [Fact]
    public void GetColumnText_ShouldReturnEmptyStringsForEmptySelections()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc\ndef");

        var selection = new ColumnSelection(0, 1, 0, 1);  // 空选区
        var texts = buffer.GetColumnText(selection);

        Assert.Single(texts);
        Assert.Equal("", texts[0]);
    }

    [Fact]
    public void GetColumnText_ShouldHandleEmptyLines()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc\n\ndef");

        var selection = new ColumnSelection(0, 0, 2, 3).Normalized();
        var texts = buffer.GetColumnText(selection);

        Assert.Equal(3, texts.Length);
        Assert.Equal("abc", texts[0]);
        Assert.Equal("", texts[1]);  // 空行
        Assert.Equal("def", texts[2]);
    }
}
