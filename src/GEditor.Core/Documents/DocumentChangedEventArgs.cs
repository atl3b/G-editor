namespace GEditor.Core.Documents;

/// <summary>文档变更事件参数</summary>
public sealed class DocumentChangedEventArgs : EventArgs
{
    public int StartLine { get; init; }
    public int EndLine { get; init; }
    public string ChangeType { get; init; } = string.Empty;   // "insert" / "delete" / "replace"
}
