using GEditor.Core.Buffer;
using GEditor.Core.Editing;
using GEditor.Core.Selection;
using Xunit;

namespace GEditor.Tests.Editing;

public class ColumnCommandTests
{
    [Fact]
    public void ColumnInsertCommand_ShouldInsertTextAtMultiplePositions()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("line1\nline2\nline3");

        var positions = new List<(int line, int column)>
        {
            (0, 0),  // 开头
            (1, 0),  // 开头
            (2, 0)   // 开头
        };

        var command = new ColumnInsertCommand(positions, ">>");
        command.Execute(buffer);

        var lines = buffer.Lines;
        Assert.Equal(">>line1", lines[0]);
        Assert.Equal(">>line2", lines[1]);
        Assert.Equal(">>line3", lines[2]);
    }

    [Fact]
    public void ColumnInsertCommand_ShouldInsertAtSpecificColumns()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc\ndef\nghi");

        var positions = new List<(int line, int column)>
        {
            (0, 1),  // 第2个字符后
            (1, 1),
            (2, 1)
        };

        var command = new ColumnInsertCommand(positions, "X");
        command.Execute(buffer);

        var lines = buffer.Lines;
        Assert.Equal("aXbc", lines[0]);
        Assert.Equal("dXef", lines[1]);
        Assert.Equal("gXhi", lines[2]);
    }

    [Fact]
    public void ColumnDeleteCommand_ShouldDeleteColumnRange()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\n123456\nghijkl");

        // 选区 [1, 4) 表示列1-3（不包括4）
        var selection = new ColumnSelection(0, 1, 2, 4).Normalized();

        // 调试：检查GetLineRanges
        var ranges = selection.GetLineRanges(buffer.Lines);
        Assert.Equal(3, ranges.Count);

        // Line 0: 列1到列4 -> "bcd" (3个字符)
        Assert.Equal(0, ranges[0].Line);
        Assert.Equal(1, ranges[0].StartColumn);
        Assert.Equal(4, ranges[0].EndColumn);

        // Line 1: 列1到列4 -> "234" (3个字符)
        Assert.Equal(1, ranges[1].Line);
        Assert.Equal(1, ranges[1].StartColumn);
        Assert.Equal(4, ranges[1].EndColumn);

        // Line 2: 列1到列4 -> "hij" (3个字符)
        Assert.Equal(2, ranges[2].Line);
        Assert.Equal(1, ranges[2].StartColumn);
        Assert.Equal(4, ranges[2].EndColumn);

        var command = new ColumnDeleteCommand(selection);
        command.Execute(buffer);

        var lines = buffer.Lines;
        Assert.Equal("aef", lines[0]);  // 删除了 bcd
        Assert.Equal("156", lines[1]);  // 删除了 234
        Assert.Equal("gkl", lines[2]);  // 删除了 hij
    }

    [Fact]
    public void ColumnDeleteCommand_ShouldHandleShortLines()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("ab\nabc\nabcd");

        // 选区 [1, 2) 只选中第2个字符
        var selection = new ColumnSelection(0, 1, 2, 2).Normalized();
        var command = new ColumnDeleteCommand(selection);
        command.Execute(buffer);

        var lines = buffer.Lines;
        Assert.Equal("a", lines[0]);     // 删除了 b
        Assert.Equal("ac", lines[1]);    // 删除了 b
        Assert.Equal("acd", lines[2]);   // 删除了 b
    }

    [Fact]
    public void ColumnDeleteCommand_ShouldSupportUndo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\n123456");

        // 只测试第1行的删除：从列1开始删2个字符 -> "23"
        var ranges = new List<(int line, int column, int length)>
        {
            (1, 1, 2)  // 第1行，列1开始，长度2
        };

        var deleted = buffer.DeleteAtColumns(ranges);
        Assert.Single(deleted);
        Assert.Equal("23", deleted[0]);

        var linesAfterDelete = buffer.Lines;
        Assert.Equal("abcdef", linesAfterDelete[0]);
        Assert.Equal("1456", linesAfterDelete[1]);

        // 恢复
        buffer.InsertAtColumns(new[] { (1, 1) }, "23");
        Assert.Equal("abcdef", buffer.Lines[0]);
        Assert.Equal("123456", buffer.Lines[1]);
    }

    [Fact]
    public void ColumnInsertCommand_ShouldSupportUndo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("line1\nline2\nline3");

        var positions = new List<(int line, int column)>
        {
            (0, 0),
            (1, 0),
            (2, 0)
        };

        var command = new ColumnInsertCommand(positions, ">>");
        command.Execute(buffer);

        var linesAfterInsert = buffer.Lines;
        Assert.Equal(">>line1", linesAfterInsert[0]);

        command.Undo(buffer);
        var linesAfterUndo = buffer.Lines;
        Assert.Equal("line1", linesAfterUndo[0]);
        Assert.Equal("line2", linesAfterUndo[1]);
        Assert.Equal("line3", linesAfterUndo[2]);
    }

    [Fact]
    public void GetColumnText_ShouldReturnSelectedColumnTexts()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\n123456\nghijkl");

        // 选区 [1, 3) 表示列1-2（不包括3）
        var selection = new ColumnSelection(0, 1, 2, 3).Normalized();
        var texts = buffer.GetColumnText(selection);

        Assert.Equal(3, texts.Length);
        Assert.Equal("bc", texts[0]);
        Assert.Equal("23", texts[1]);
        Assert.Equal("hi", texts[2]);
    }

    [Fact]
    public void GetColumnText_ShouldHandleSingleLineSelection()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef");

        // 选区 [1, 3) 表示列1-2（不包括3）
        var selection = new ColumnSelection(0, 1, 0, 3);
        var texts = buffer.GetColumnText(selection);

        Assert.Single(texts);
        Assert.Equal("bc", texts[0]);
    }

    #region ColumnReplaceCommand Tests

    [Fact]
    public void ColumnReplaceCommand_ShouldDeleteAndInsert()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\n123456\nghijkl");

        // 替换列 1-4 为 "XX"
        var selection = new ColumnSelection(0, 1, 2, 4).Normalized();
        var command = new ColumnReplaceCommand(selection, "XX");
        command.Execute(buffer);

        var lines = buffer.Lines;
        Assert.Equal("aXXef", lines[0]);
        Assert.Equal("1XX56", lines[1]);
        Assert.Equal("gXXkl", lines[2]);
    }

    [Fact]
    public void ColumnReplaceCommand_ShouldSupportUndo()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\n123456");

        var selection = new ColumnSelection(0, 1, 1, 4).Normalized();
        var command = new ColumnReplaceCommand(selection, "X");
        command.Execute(buffer);

        // 验证替换后结果
        Assert.Equal("aXef", buffer.Lines[0]);
        Assert.Equal("1X56", buffer.Lines[1]);

        command.Undo(buffer);

        // 撤销后恢复原始内容
        Assert.Equal("abcdef", buffer.Lines[0]);
        Assert.Equal("123456", buffer.Lines[1]);
    }

    [Fact]
    public void ColumnReplaceCommand_WithEmptyNewText_ShouldJustDelete()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef\n123456");

        var selection = new ColumnSelection(0, 2, 0, 5).Normalized();
        var command = new ColumnReplaceCommand(selection, "");
        command.Execute(buffer);

        Assert.Equal("abf", buffer.Lines[0]);

        command.Undo(buffer);
        Assert.Equal("abcdef", buffer.Lines[0]);
    }

    [Fact]
    public void ColumnReplaceCommand_WithLongerReplacement_ShouldExpand()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc\ndef");

        var selection = new ColumnSelection(0, 1, 1, 2).Normalized();
        var command = new ColumnReplaceCommand(selection, "XXXX");
        command.Execute(buffer);

        Assert.Equal("aXXXXc", buffer.Lines[0]);
        Assert.Equal("dXXXXf", buffer.Lines[1]);

        command.Undo(buffer);
        Assert.Equal("abc", buffer.Lines[0]);
        Assert.Equal("def", buffer.Lines[1]);
    }

    #endregion
}
