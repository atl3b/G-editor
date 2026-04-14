using GEditor.Core.Buffer;
using GEditor.Core.Editing;
using Xunit;

namespace GEditor.Tests.Editing;

public class UndoRedoManagerTests
{
    [Fact]
    public void Execute_PushesToUndoStack()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 5, " world"), buffer);

        Assert.Equal(1, manager.UndoCount);
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);
        Assert.Equal("hello world", buffer[0]);
    }

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 5, " world"), buffer);
        manager.Undo(buffer);

        Assert.Equal("hello", buffer[0]);
        Assert.False(manager.CanUndo);
        Assert.True(manager.CanRedo);
    }

    [Fact]
    public void Redo_ReappliesCommand()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 5, " world"), buffer);
        manager.Undo(buffer);
        manager.Redo(buffer);

        Assert.Equal("hello world", buffer[0]);
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void Undo_WhenEmpty_DoesNothing()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        var manager = new UndoRedoManager();

        manager.Undo(buffer); // Should not throw

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void Redo_WhenEmpty_DoesNothing()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        var manager = new UndoRedoManager();

        manager.Redo(buffer); // Should not throw

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void Execute_NewAfterUndo_ClearsRedoStack()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 5, " world"), buffer);
        manager.Undo(buffer);
        Assert.True(manager.CanRedo);

        manager.Execute(new InsertTextCommand(0, 5, " earth"), buffer);
        Assert.False(manager.CanRedo);
        Assert.Equal("hello earth", buffer[0]);
    }

    [Fact]
    public void Clear_EmptiesBothStacks()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 5, " world"), buffer);
        manager.Undo(buffer);
        manager.Clear();

        Assert.False(manager.CanUndo);
        Assert.False(manager.CanRedo);
        Assert.Equal(0, manager.UndoCount);
    }

    [Fact]
    public void MultipleOperations_UndoRedoSequence()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 3, "d"), buffer);  // "abcd"
        manager.Execute(new InsertTextCommand(0, 4, "e"), buffer);  // "abcde"

        Assert.Equal("abcde", buffer[0]);

        manager.Undo(buffer); // "abcd"
        Assert.Equal("abcd", buffer[0]);

        manager.Undo(buffer); // "abc"
        Assert.Equal("abc", buffer[0]);

        manager.Redo(buffer); // "abcd"
        Assert.Equal("abcd", buffer[0]);
    }

    #region 连续 Undo/Redo 序列 + 大批量命令

    [Fact]
    public void ContinuousUndoRedo_FullCycleRestoresFinalState()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("");
        var manager = new UndoRedoManager();

        // 执行 5 次连续插入
        for (int i = 0; i < 5; i++)
            manager.Execute(new InsertTextCommand(0, i, ((char)('a' + i)).ToString()), buffer);

        Assert.Equal("abcde", buffer[0]);

        // 全部撤销
        for (int i = 0; i < 5; i++)
            manager.Undo(buffer);
        Assert.Equal("", buffer[0]);
        Assert.False(manager.CanUndo);
        Assert.True(manager.CanRedo);
        Assert.Equal(0, manager.UndoCount); // Undo 栈已空，全部在 Redo 栈中

        // 全部重做
        for (int i = 0; i < 5; i++)
            manager.Redo(buffer);
        Assert.Equal("abcde", buffer[0]);
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void UndoThenRedoThenUndoAgain_MaintainsCorrectness()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("start");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 5, " A"), buffer);  // "start A"
        manager.Execute(new InsertTextCommand(0, 7, " B"), buffer);  // "start A B"

        manager.Undo(buffer);  // "start A"
        Assert.Equal("start A", buffer[0]);

        manager.Redo(buffer);   // "start A B"
        Assert.Equal("start A B", buffer[0]);

        manager.Undo(buffer);  // "start A"
        Assert.Equal("start A", buffer[0]);

        manager.Undo(buffer);  // "start"
        Assert.Equal("start", buffer[0]);
    }

    [Fact]
    public void LargeBatchCommands_1000Inserts_AllUndoCorrectly()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("");
        var manager = new UndoRedoManager();

        const int count = 1000;
        for (int i = 0; i < count; i++)
            manager.Execute(new InsertTextCommand(0, i, "X"), buffer);

        Assert.Equal(count, manager.UndoCount);
        Assert.Equal(new string('X', count), buffer[0]);

        // 撤销全部
        for (int i = 0; i < count; i++)
            manager.Undo(buffer);

        Assert.Equal("", buffer[0]);
        Assert.False(manager.CanUndo);
    }

    [Fact]
    public void LargeBatchCommands_1000MixedOps_StableStack()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world");
        var manager = new UndoRedoManager();

        const int count = 1000;
        for (int i = 0; i < count; i++)
        {
            if (i % 2 == 0)
                manager.Execute(new InsertTextCommand(0, 0, "A"), buffer);
            else
                manager.Execute(new DeleteTextCommand(0, 0, 1, "A"), buffer);
        }

        Assert.Equal(count, manager.UndoCount);
        Assert.True(manager.CanUndo);
        Assert.False(manager.CanRedo);

        // 撤销一半
        for (int i = 0; i < count / 2; i++)
            manager.Undo(buffer);

        Assert.Equal(count / 2, manager.UndoCount);
        Assert.True(manager.CanUndo);
        Assert.True(manager.CanRedo);
    }

    [Fact]
    public void ClearAfterManyOps_ResetsCompletely()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("initial");
        var manager = new UndoRedoManager();

        for (int i = 0; i < 50; i++)
            manager.Execute(new InsertTextCommand(0, buffer[0].Length, i.ToString()), buffer);

        Assert.True(manager.CanUndo);
        manager.Clear();
        Assert.False(manager.CanUndo);
        Assert.False(manager.CanRedo);
        Assert.Equal(0, manager.UndoCount);

        // Clear 后可重新操作
        manager.Execute(new InsertTextCommand(0, buffer[0].Length, " new"), buffer);
        Assert.Equal(1, manager.UndoCount); // UndoCount == 1
    }

    [Fact]
    public void InterleavedExecuteAndUndo_MaintainsConsistentState()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("ab");
        var manager = new UndoRedoManager();

        manager.Execute(new InsertTextCommand(0, 2, "c"), buffer); // "abc"
        manager.Undo(buffer);                                       // "ab"
        manager.Execute(new InsertTextCommand(0, 2, "X"), buffer);  // "abX"
        manager.Execute(new DeleteTextCommand(0, 1, 1, "b"), buffer);    // "aX"
        manager.Undo(buffer);                                        // "abX"
        manager.Redo(buffer);                                         // "aX"

        Assert.Equal("aX", buffer[0]);
        Assert.True(manager.CanUndo);
    }

    #endregion
}
