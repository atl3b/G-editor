namespace GEditor.Core.Editing;

/// <summary>插入文本命令</summary>
public sealed class InsertTextCommand : IEditCommand
{
    public string Description { get; }
    private readonly int _line;
    private readonly int _column;
    private readonly string _text;

    public InsertTextCommand(int line, int column, string text)
    {
        _line = line;
        _column = column;
        _text = text ?? string.Empty;
        Description = $"Insert '{_text}' at ({_line}, {_column})";
    }

    public void Execute(Buffer.EditorBuffer buffer)
        => buffer.Insert(_line, _column, _text);

    public void Undo(Buffer.EditorBuffer buffer)
        => buffer.Delete(_line, _column, Buffer.EditorBuffer.GetBufferLength(_text));
}
