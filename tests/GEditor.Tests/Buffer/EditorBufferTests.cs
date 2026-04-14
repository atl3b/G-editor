using GEditor.Core.Buffer;
using GEditor.Core.Selection;
using Xunit;

namespace GEditor.Tests.Buffer;

public class EditorBufferTests
{
    [Fact]
    public void SetAllText_SingleLine_SetsCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        Assert.Equal(1, buffer.LineCount);
        Assert.Equal("hello world", buffer[0]);
    }

    [Fact]
    public void SetAllText_MultipleLines_SplitsCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("line1\r\nline2\r\nline3");

        Assert.Equal(3, buffer.LineCount);
        Assert.Equal("line1", buffer[0]);
        Assert.Equal("line2", buffer[1]);
        Assert.Equal("line3", buffer[2]);
    }

    [Fact]
    public void SetAllText_EmptyText_CreatesOneEmptyLine()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("");

        Assert.Equal(1, buffer.LineCount);
        Assert.Equal("", buffer[0]);
    }

    [Fact]
    public void SetAllText_LfNewlines_SplitsCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("a\nb\nc");

        Assert.Equal(3, buffer.LineCount);
        Assert.Equal("a", buffer[0]);
        Assert.Equal("b", buffer[1]);
        Assert.Equal("c", buffer[2]);
    }

    [Fact]
    public void GetAllText_JoinsWithLineEnding()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello\nworld");

        Assert.Equal("hello\r\nworld", buffer.GetAllText("\r\n"));
        Assert.Equal("hello\nworld", buffer.GetAllText("\n"));
    }

    [Fact]
    public void Insert_SingleLine_InsertsCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        var (newLine, newCol) = buffer.Insert(0, 5, " beautiful");

        Assert.Equal(0, newLine);
        Assert.Equal(15, newCol); // column(5) + text.Length(10) = 15
        Assert.Equal("hello beautiful world", buffer[0]);
    }

    [Fact]
    public void Insert_MultiLine_SplitsCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("helloworld");

        var (newLine, newCol) = buffer.Insert(0, 5, "\nnew");

        Assert.Equal(1, newLine);
        Assert.Equal(3, newCol);
        Assert.Equal(2, buffer.LineCount);
        Assert.Equal("hello", buffer[0]);
        Assert.Equal("newworld", buffer[1]);
    }

    [Fact]
    public void Insert_EmptyText_DoesNothing()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        var (newLine, newCol) = buffer.Insert(0, 2, "");

        Assert.Equal(0, newLine);
        Assert.Equal(2, newCol);
        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void Delete_SingleLine_DeletesCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        buffer.Delete(0, 5, 1);

        Assert.Equal("helloworld", buffer[0]);
    }

    [Fact]
    public void Delete_CrossLine_DeletesCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello\nworld");

        buffer.Delete(0, 5, 5); // delete "\n" and part of "world"

        Assert.Equal(1, buffer.LineCount);
        Assert.Equal("hellod", buffer[0]);
    }

    [Fact]
    public void Delete_ZeroLength_DoesNothing()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        buffer.Delete(0, 2, 0);

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void Replace_ReplacesCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        buffer.Replace(0, 6, 5, "earth");

        Assert.Equal("hello earth", buffer[0]);
    }

    [Fact]
    public void GetLineLength_ReturnsCorrectLength()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello\nworld");

        Assert.Equal(5, buffer.GetLineLength(0));
        Assert.Equal(5, buffer.GetLineLength(1));
    }

    [Fact]
    public void Changed_Event_FiresOnInsert()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        int fired = 0;
        buffer.Changed += (_, _) => fired++;

        buffer.Insert(0, 5, " world");

        Assert.Equal(1, fired);
    }

    [Fact]
    public void Changed_Event_FiresOnDelete()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        int fired = 0;
        buffer.Changed += (_, _) => fired++;

        buffer.Delete(0, 0, 2);

        Assert.Equal(1, fired);
    }

    [Fact]
    public void Lines_ReturnsReadOnlyList()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("a\nb\nc");

        Assert.Equal(3, buffer.Lines.Count);
        Assert.Equal("a", buffer.Lines[0]);
        Assert.Equal("b", buffer.Lines[1]);
        Assert.Equal("c", buffer.Lines[2]);
    }

    #region GetBufferLength Tests

    [Theory]
    [InlineData("", 0)]
    [InlineData("hello", 5)]
    [InlineData("hello\r\nworld", 11)] // \r\n counted as 1
    [InlineData("\r\n", 1)]             // single CRLF
    [InlineData("a\r\nb\r\nc", 5)]     // two CRLFs
    [InlineData("line1\r\nline2\r\nline3", 17)]
    [InlineData("a\nb\nc", 5)]         // LF only
    [InlineData("a\rb\rc", 5)]         // CR only
    public void GetBufferLength_PlainText_ReturnsCorrectLength(string text, int expected)
    {
        Assert.Equal(expected, EditorBuffer.GetBufferLength(text));
    }

    [Fact]
    public void GetBufferLength_NullText_ReturnsZero()
    {
        Assert.Equal(0, EditorBuffer.GetBufferLength(null!));
    }

    [Fact]
    public void GetBufferLength_MixedLineEndings_CountsCorrectly()
    {
        // Mixed: \r\n (2 chars) should count as 1, \r (1 char) as 1, \n (1 char) as 1
        string mixed = "a\r\nb\nc\rd";
        // a(1) + \r\n(1) + b(1) + \n(1) + c(1) + \r(1) + d(1) = 7
        Assert.Equal(7, EditorBuffer.GetBufferLength(mixed));
    }

    #endregion

    #region GetColumnText Tests

    [Fact]
    public void GetColumnText_SingleLine_ReturnsCorrectText()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        var selection = new ColumnSelection(0, 0, 0, 5);
        var result = buffer.GetColumnText(selection);

        Assert.Single(result);
        Assert.Equal("hello", result[0]);
    }

    [Fact]
    public void GetColumnText_MultipleLines_ReturnsEachLineSelection()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\nghijkl\nmnopqr");

        // Select columns 2-5 on all 3 lines
        var selection = new ColumnSelection(0, 2, 2, 5);
        var result = buffer.GetColumnText(selection);

        Assert.Equal(3, result.Length);
        Assert.Equal("cde", result[0]);
        Assert.Equal("ijk", result[1]);
        Assert.Equal("opq", result[2]);
    }

    [Fact]
    public void GetColumnText_EmptyLine_ReturnsEmptyString()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc\n\nxyz");

        var selection = new ColumnSelection(0, 1, 2, 3);
        var result = buffer.GetColumnText(selection);

        Assert.Equal(3, result.Length);
        Assert.Equal("bc", result[0]);
        Assert.Equal("", result[1]); // Empty line - column range beyond line length
        Assert.Equal("yz", result[2]);
    }

    [Fact]
    public void GetColumnText_OutOfRangeSelection_ReturnsClamped()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hi");

        // Select beyond line length - should clamp to available text
        var selection = new ColumnSelection(0, 0, 0, 100);
        var result = buffer.GetColumnText(selection);

        Assert.Single(result);
        Assert.Equal("hi", result[0]);
    }

    [Fact]
    public void GetColumnText_ReversedCoordinates_WorksViaNormalized()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        // Reversed: End < Start
        var selection = new ColumnSelection(0, 5, 0, 0);
        var result = buffer.GetColumnText(selection);

        Assert.Single(result);
        Assert.Equal("hello", result[0]);
    }

    #endregion

    #region InsertAtColumns Tests

    [Fact]
    public void InsertAtColumns_SinglePosition_InsertsCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        buffer.InsertAtColumns(new[] { (line: 0, column: 5) }, " ");

        Assert.Equal("hello  world", buffer[0]);
    }

    [Fact]
    public void InsertAtColumns_MultiplePositions_InsertsAtEachPosition()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc\ndef\nghi");

        buffer.InsertAtColumns(new[]
        {
            (line: 0, column: 1),
            (line: 1, column: 1),
            (line: 2, column: 1)
        }, ">");

        Assert.Equal("a>bc", buffer[0]);
        Assert.Equal("d>ef", buffer[1]);
        Assert.Equal("g>hi", buffer[2]);
    }

    [Fact]
    public void InsertAtColumns_EmptyText_DoesNothing()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        buffer.InsertAtColumns(new[] { (line: 0, column: 2) }, "");

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void InsertAtColumns_EmptyPositions_DoesNothing()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        buffer.InsertAtColumns(Array.Empty<(int, int)>(), "x");

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void InsertAtColumns_OutOfBoundsLine_SkipsGracefully()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        // Line 99 doesn't exist - should skip without exception
        buffer.InsertAtColumns(new[]
        {
            (line: 0, column: 2),
            (line: 99, column: 0)
        }, "x");

        Assert.Equal("hexllo", buffer[0]); // Only line 0 was modified
    }

    [Fact]
    public void InsertAtColumns_ColumnBeyondLineLength_ClampsToEnd()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hi"); // Length 2

        // Column 10 is beyond line length → should clamp to end
        buffer.InsertAtColumns(new[] { (line: 0, column: 10) }, "!");

        Assert.Equal("hi!", buffer[0]);
    }

    [Fact]
    public void InsertAtColumns_FiresChangedEvent()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        int fired = 0;
        buffer.Changed += (_, _) => fired++;

        buffer.InsertAtColumns(new[] { (line: 0, column: 2) }, "x");

        Assert.Equal(1, fired);
    }

    #endregion

    #region DeleteAtColumns Tests

    [Fact]
    public void DeleteAtColumns_SingleRange_DeletesCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");

        var deleted = buffer.DeleteAtColumns(new[] { (line: 0, column: 0, length: 5) });

        Assert.Single(deleted);
        Assert.Equal("hello", deleted[0]);
        Assert.Equal(" world", buffer[0]);
    }

    [Fact]
    public void DeleteAtColumns_MultipleRanges_DeletesFromEachLine()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\nghijkl\nmnopqr");

        var deleted = buffer.DeleteAtColumns(new[]
        {
            (line: 0, column: 2, length: 2),
            (line: 1, column: 2, length: 2),
            (line: 2, column: 2, length: 2)
        });

        Assert.Equal(3, deleted.Length);
        Assert.Equal("cd", deleted[0]);
        Assert.Equal("ij", deleted[1]);
        Assert.Equal("op", deleted[2]);

        Assert.Equal("abef", buffer[0]);
        Assert.Equal("ghkl", buffer[1]);
        Assert.Equal("mnqr", buffer[2]);
    }

    [Fact]
    public void DeleteAtColumns_ReturnsDeletedTextInOriginalOrder()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("aaa\nbbb\nccc");

        var deleted = buffer.DeleteAtColumns(new[]
        {
            (line: 0, column: 0, length: 1), // "a"
            (line: 1, column: 0, length: 1), // "b"
            (line: 2, column: 0, length: 1)  // "c"
        });

        Assert.Equal(3, deleted.Length);
        Assert.Equal("a", deleted[0]);
        Assert.Equal("b", deleted[1]);
        Assert.Equal("c", deleted[2]);
    }

    [Fact]
    public void DeleteAtColumns_EmptyRanges_ReturnsEmptyArray()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        var deleted = buffer.DeleteAtColumns(Array.Empty<(int, int, int)>());

        Assert.Empty(deleted);
        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void DeleteAtColumns_ZeroLength_SkipsRange()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        var deleted = buffer.DeleteAtColumns(new[] { (line: 0, column: 1, length: 0) });

        Assert.Empty(deleted);
        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void DeleteAtColumns_OutOfBoundsLine_SkipsGracefully()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        var deleted = buffer.DeleteAtColumns(new[]
        {
            (line: 0, column: 0, length: 2), // Valid
            (line: 99, column: 0, length: 2)  // Invalid line
        });

        Assert.Single(deleted);
        Assert.Equal("he", deleted[0]);
        Assert.Equal("llo", buffer[0]);
    }

    [Fact]
    public void DeleteAtColumns_FiresChangedEvent()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");

        int fired = 0;
        buffer.Changed += (_, _) => fired++;

        buffer.DeleteAtColumns(new[] { (line: 0, column: 0, length: 2) });

        Assert.Equal(1, fired);
    }

    #endregion
}
