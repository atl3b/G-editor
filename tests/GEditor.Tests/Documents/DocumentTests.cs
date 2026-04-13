using GEditor.Core.Documents;
using GEditor.Core.Editing;
using Xunit;

namespace GEditor.Tests.Documents;

public class DocumentTests
{
    [Fact]
    public void NewDocument_IsNewAndUntitled()
    {
        var doc = new Document();

        Assert.True(doc.IsNew);
        Assert.Equal("Untitled", doc.DisplayName);
        Assert.False(doc.IsDirty);
    }

    [Fact]
    public void NewDocument_HasEmptyBuffer()
    {
        var doc = new Document();

        Assert.Equal(1, doc.Buffer.LineCount);
        Assert.Equal("", doc.Buffer[0]);
    }

    [Fact]
    public void Document_WithFilePath_IsNotNew()
    {
        var doc = new Document("/path/to/file.cs");

        Assert.False(doc.IsNew);
        Assert.Equal("file.cs", doc.DisplayName);
    }

    [Fact]
    public void LoadText_SetsBufferAndClearsDirty()
    {
        var doc = new Document();
        doc.LoadText("hello\nworld");

        Assert.Equal(2, doc.Buffer.LineCount);
        Assert.Equal("hello", doc.Buffer[0]);
        Assert.False(doc.IsDirty);
    }

    [Fact]
    public void ExecuteCommand_SetsDirty()
    {
        var doc = new Document();
        doc.LoadText("hello");

        doc.ExecuteCommand(new InsertTextCommand(0, 5, " world"));

        Assert.True(doc.IsDirty);
        Assert.Equal("hello world", doc.Buffer[0]);
    }

    [Fact]
    public void MarkAsSaved_ClearsDirtyFlag()
    {
        var doc = new Document();
        doc.LoadText("hello");
        doc.ExecuteCommand(new InsertTextCommand(0, 5, " world"));

        Assert.True(doc.IsDirty);
        doc.MarkAsSaved();
        Assert.False(doc.IsDirty);
    }

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var doc = new Document();
        doc.LoadText("hello");

        doc.ExecuteCommand(new InsertTextCommand(0, 5, " world"));
        Assert.Equal("hello world", doc.Buffer[0]);

        doc.Undo();
        Assert.Equal("hello", doc.Buffer[0]);
    }

    [Fact]
    public void Redo_ReappliesCommand()
    {
        var doc = new Document();
        doc.LoadText("hello");

        doc.ExecuteCommand(new InsertTextCommand(0, 5, " world"));
        doc.Undo();
        doc.Redo();

        Assert.Equal("hello world", doc.Buffer[0]);
    }

    [Fact]
    public void GetFullText_UsesActiveLineEnding()
    {
        var doc = new Document();
        doc.LoadText("hello\nworld");

        var text = doc.GetFullText();
        // Default line ending is from environment
        Assert.Contains("hello", text);
        Assert.Contains("world", text);
    }

    [Fact]
    public void Changed_Event_FiresOnCommandExecution()
    {
        var doc = new Document();
        doc.LoadText("hello");

        int fired = 0;
        doc.Changed += (_, _) => fired++;

        doc.ExecuteCommand(new InsertTextCommand(0, 5, " world"));

        Assert.Equal(1, fired);
    }

    [Fact]
    public void LoadText_ClearsUndoHistory()
    {
        var doc = new Document();
        doc.LoadText("hello");
        doc.ExecuteCommand(new InsertTextCommand(0, 5, " world"));
        Assert.True(doc.UndoRedoManager.CanUndo);

        doc.LoadText("brand new content");
        Assert.False(doc.UndoRedoManager.CanUndo);
    }

    [Fact]
    public void Dispose_UnsubscribesFromBufferEvents()
    {
        var doc = new Document();
        doc.LoadText("hello");
        doc.Dispose();

        int fired = 0;
        doc.Changed += (_, _) => fired++;

        // This should not fire event since disposed
        doc.Buffer.SetAllText("test");
        Assert.Equal(0, fired);
    }
}
