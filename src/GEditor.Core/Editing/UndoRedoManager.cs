namespace GEditor.Core.Editing;

/// <summary>撤销/重做管理器</summary>
public sealed class UndoRedoManager
{
    private readonly Stack<IEditCommand> _undoStack = new();
    private readonly Stack<IEditCommand> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public int UndoCount => _undoStack.Count;

    /// <summary>执行命令（由 Document 注入 buffer 后调用）</summary>
    public void Execute(IEditCommand command, Buffer.EditorBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(command);
        command.Execute(buffer);
        _undoStack.Push(command);
        _redoStack.Clear();
    }

    /// <summary>撤销最近一次命令</summary>
    public void Undo(Buffer.EditorBuffer buffer)
    {
        if (_undoStack.Count == 0) return;
        var command = _undoStack.Pop();
        command.Undo(buffer);
        _redoStack.Push(command);
    }

    /// <summary>重做最近一次撤销</summary>
    public void Redo(Buffer.EditorBuffer buffer)
    {
        if (_redoStack.Count == 0) return;
        var command = _redoStack.Pop();
        command.Execute(buffer);
        _undoStack.Push(command);
    }

    /// <summary>清空历史</summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
