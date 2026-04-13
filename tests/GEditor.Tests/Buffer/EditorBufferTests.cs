using GEditor.Core.Buffer;
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
}
