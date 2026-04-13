namespace GEditor.Core.Editing;

/// <summary>删除文本命令</summary>
public sealed class DeleteTextCommand : IEditCommand
{
    public string Description { get; }
    private readonly int _line;
    private readonly int _column;
    private readonly string _deletedText;

    public DeleteTextCommand(int line, int column, int length, string deletedText)
    {
        _line = line;
        _column = column;
        _deletedText = deletedText ?? string.Empty;
        Description = $"Delete {_deletedText.Length} chars at ({_line}, {_column})";
    }

    public void Execute(Buffer.EditorBuffer buffer)
        => buffer.Delete(_line, _column, Buffer.EditorBuffer.GetBufferLength(_deletedText));

    public void Undo(Buffer.EditorBuffer buffer)
        => buffer.Insert(_line, _column, _deletedText);
}
