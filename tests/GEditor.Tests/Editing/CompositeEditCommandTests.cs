using GEditor.Core.Buffer;
using GEditor.Core.Editing;
using Xunit;

namespace GEditor.Tests.Editing;

public class CompositeEditCommandTests
{
    [Fact]
    public void Execute_ExecutesAllCommandsInOrder()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc");

        var command = new CompositeEditCommand("multi", new IEditCommand[]
        {
            new InsertTextCommand(0, 3, "d"),
            new InsertTextCommand(0, 4, "e"),
            new InsertTextCommand(0, 5, "f"),
        });

        command.Execute(buffer);

        Assert.Equal("abcdef", buffer[0]);
    }

    [Fact]
    public void Undo_UndoesCommandsInReverseOrder()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc");

        var command = new CompositeEditCommand("multi", new IEditCommand[]
        {
            new InsertTextCommand(0, 3, "d"),
            new InsertTextCommand(0, 4, "e"),
            new InsertTextCommand(0, 5, "f"),
        });

        command.Execute(buffer);
        Assert.Equal("abcdef", buffer[0]);

        command.Undo(buffer);
        Assert.Equal("abc", buffer[0]);
    }

    [Fact]
    public void Execute_EmptyCommands_DoesNothing()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc");

        var command = new CompositeEditCommand("empty", Array.Empty<IEditCommand>());

        command.Execute(buffer);
        Assert.Equal("abc", buffer[0]);

        command.Undo(buffer);
        Assert.Equal("abc", buffer[0]);
    }

    [Fact]
    public void Description_ReturnsProvidedDescription()
    {
        var command = new CompositeEditCommand("test desc", Array.Empty<IEditCommand>());
        Assert.Equal("test desc", command.Description);
    }

    [Fact]
    public void CompositeCommand_InUndoRedoManager_WorksAsSingleUnit()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello world hello world");
        var manager = new UndoRedoManager();

        // Replace All: "hello" → "hi" (2 occurrences)
        var command = new CompositeEditCommand("Replace All", new IEditCommand[]
        {
            new ReplaceTextCommand(0, 12, 5, "hello", "hi"),  // second occurrence first (from back)
            new ReplaceTextCommand(0, 0, 5, "hello", "hi"),    // first occurrence
        });

        manager.Execute(command, buffer);
        Assert.Equal("hi world hi world", buffer[0]);

        // Single undo should restore both
        manager.Undo(buffer);
        Assert.Equal("hello world hello world", buffer[0]);
    }
}
