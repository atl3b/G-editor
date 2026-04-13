using GEditor.Core.Buffer;
using GEditor.Core.Editing;
using Xunit;

namespace GEditor.Tests.Editing;

/// <summary>
/// CRLF 场景下的命令往返测试，确保 Undo/Redo 正确处理 \r\n
/// </summary>
public class CrlfCommandTests
{
    #region GetBufferLength Tests

    [Theory]
    [InlineData("", 0)]
    [InlineData("hello", 5)]
    [InlineData("hello\r\nworld", 11)] // \r\n counted as 1
    [InlineData("\r\n", 1)]              // single CRLF
    [InlineData("a\r\nb\r\nc", 5)]      // two CRLFs
    [InlineData("line1\r\nline2\r\nline3", 17)]
    [InlineData("a\nb\nc", 5)]          // LF only
    [InlineData("a\rb\rc", 5)]          // CR only
    [InlineData("mixed\r\nlf\ncr\r", 12)] // mixed endings
    public void GetBufferLength_ReturnsCorrectLength(string text, int expected)
    {
        Assert.Equal(expected, EditorBuffer.GetBufferLength(text));
    }

    [Fact]
    public void GetBufferLength_NullText_ReturnsZero()
    {
        Assert.Equal(0, EditorBuffer.GetBufferLength(null!));
    }

    #endregion

    #region InsertTextCommand CRLF Tests

    [Fact]
    public void InsertTextCommand_Undo_WithSingleLine_RestoresOriginal()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("line1\r\nline2");
        var manager = new UndoRedoManager();

        // Insert single-line text at end of "line1"
        manager.Execute(new InsertTextCommand(0, 5, " inserted"), buffer);

        Assert.Equal(2, buffer.LineCount);
        Assert.Equal("line1 inserted", buffer[0]);
        Assert.Equal("line2", buffer[1]);

        // Undo should restore original
        manager.Undo(buffer);

        Assert.Equal(2, buffer.LineCount);
        Assert.Equal("line1", buffer[0]);
        Assert.Equal("line2", buffer[1]);
    }

    [Fact]
    public void InsertTextCommand_InsertCrlf_ThenUndo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("a");
        var manager = new UndoRedoManager();

        // Insert CRLF at end: creates new line
        manager.Execute(new InsertTextCommand(0, 1, "\r\n"), buffer);

        Assert.Equal(2, buffer.LineCount);
        Assert.Equal("a", buffer[0]);
        Assert.Equal("", buffer[1]);

        // Undo: should remove the empty line
        manager.Undo(buffer);

        Assert.Equal(1, buffer.LineCount);
        Assert.Equal("a", buffer[0]);
    }

    #endregion

    #region ReplaceTextCommand CRLF Tests

    [Fact]
    public void ReplaceTextCommand_SingleLine_ExecuteUndoRedo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");
        var manager = new UndoRedoManager();

        // Replace "world" with "earth"
        manager.Execute(new ReplaceTextCommand(0, 6, 5, "world", "earth"), buffer);

        Assert.Equal("hello earth", buffer[0]);

        manager.Undo(buffer);

        Assert.Equal("hello world", buffer[0]);

        manager.Redo(buffer);

        Assert.Equal("hello earth", buffer[0]);
    }

    [Fact]
    public void ReplaceTextCommand_MultiLine_ExecuteUndoRedo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("line1\r\nline2\r\nline3");
        var manager = new UndoRedoManager();

        // Replace "line2" with "newLine"
        manager.Execute(new ReplaceTextCommand(1, 0, 5, "line2", "newLine"), buffer);

        Assert.Equal(3, buffer.LineCount);
        Assert.Equal("line1", buffer[0]);
        Assert.Equal("newLine", buffer[1]);
        Assert.Equal("line3", buffer[2]);

        manager.Undo(buffer);

        Assert.Equal("line1", buffer[0]);
        Assert.Equal("line2", buffer[1]);
        Assert.Equal("line3", buffer[2]);
    }

    #endregion

    #region DeleteTextCommand CRLF Tests

    [Fact]
    public void DeleteTextCommand_SingleLine_ExecuteUndoRedo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");
        var manager = new UndoRedoManager();

        // Delete "world"
        manager.Execute(new DeleteTextCommand(0, 6, 5, "world"), buffer);

        Assert.Equal("hello ", buffer[0]);

        manager.Undo(buffer);

        Assert.Equal("hello world", buffer[0]);
    }

    [Fact]
    public void DeleteTextCommand_MultiLine_ExecuteUndoRedo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("line1\r\nline2\r\nline3");
        var manager = new UndoRedoManager();

        // Delete "line2" (including its newline)
        // Note: buffer length of "line2\r\n" = 5 + 1 = 6
        manager.Execute(new DeleteTextCommand(1, 0, 6, "line2\r\n"), buffer);

        Assert.Equal(2, buffer.LineCount);
        Assert.Equal("line1", buffer[0]);
        Assert.Equal("line3", buffer[1]);

        manager.Undo(buffer);

        Assert.Equal(3, buffer.LineCount);
        Assert.Equal("line1", buffer[0]);
        Assert.Equal("line2", buffer[1]);
        Assert.Equal("line3", buffer[2]);
    }

    #endregion
}
