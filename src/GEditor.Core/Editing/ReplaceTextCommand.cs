namespace GEditor.Core.Editing;

/// <summary>替换文本命令</summary>
public sealed class ReplaceTextCommand : IEditCommand
{
    public string Description { get; }
    private readonly int _line;
    private readonly int _column;
    private readonly string _oldText;
    private readonly string _newText;

    public ReplaceTextCommand(int line, int column, int length, string oldText, string newText)
    {
        _line = line;
        _column = column;
        _oldText = oldText ?? string.Empty;
        _newText = newText ?? string.Empty;
        Description = $"Replace '{_oldText}' with '{_newText}' at ({_line}, {_column})";
    }

    public void Execute(Buffer.EditorBuffer buffer)
        => buffer.Replace(_line, _column, Buffer.EditorBuffer.GetBufferLength(_oldText), _newText);

    public void Undo(Buffer.EditorBuffer buffer)
        => buffer.Replace(_line, _column, Buffer.EditorBuffer.GetBufferLength(_newText), _oldText);
}
