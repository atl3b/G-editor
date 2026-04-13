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
}
