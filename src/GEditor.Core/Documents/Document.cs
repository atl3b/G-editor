using GEditor.Core.Buffer;
using GEditor.Core.Editing;

namespace GEditor.Core.Documents;

/// <summary>
/// 文档聚合根：组合 EditorBuffer + 编码信息 + 换行符信息 + 文件路径。
/// </summary>
public sealed class Document : IDisposable
{
    public string FilePath { get; set; } = string.Empty;
    public bool IsNew { get; }
    public bool IsDirty { get; private set; }
    public DocumentEncodingInfo EncodingInfo { get; set; } = DocumentEncodingInfo.Default;
    public DocumentLineEndingInfo LineEndingInfo { get; set; } = DocumentLineEndingInfo.Default;
    public EditorBuffer Buffer { get; }
    public UndoRedoManager UndoRedoManager { get; }

    public string DisplayName
    {
        get
        {
            if (IsNew) return "Untitled";
            return Path.GetFileName(FilePath);
        }
    }

    public event EventHandler<DocumentChangedEventArgs>? Changed;

    public Document() : this(string.Empty) { }

    public Document(string filePath)
    {
        FilePath = filePath;
        IsNew = string.IsNullOrEmpty(filePath);
        Buffer = new EditorBuffer();
        UndoRedoManager = new UndoRedoManager();
        Buffer.SetAllText(string.Empty); // Ensure one empty line
        Buffer.Changed += OnBufferChanged;
    }

    private void OnBufferChanged(object? sender, DocumentChangedEventArgs e)
    {
        IsDirty = true;
        Changed?.Invoke(this, e);
    }

    /// <summary>通过 UndoRedoManager 执行编辑命令</summary>
    public void ExecuteCommand(IEditCommand command)
    {
        UndoRedoManager.Execute(command, Buffer);
    }

    /// <summary>撤销</summary>
    public void Undo() => UndoRedoManager.Undo(Buffer);

    /// <summary>重做</summary>
    public void Redo() => UndoRedoManager.Redo(Buffer);

    /// <summary>标记为已保存</summary>
    public void MarkAsSaved() => IsDirty = false;

    /// <summary>获取全文（用于保存）</summary>
    public string GetFullText() => Buffer.GetAllText(LineEndingInfo.Sequence);

    /// <summary>设置全文（用于加载）</summary>
    public void LoadText(string text)
    {
        Buffer.SetAllText(text);
        IsDirty = false;
        UndoRedoManager.Clear();
    }

    public void Dispose()
    {
        Buffer.Changed -= OnBufferChanged;
    }
}
