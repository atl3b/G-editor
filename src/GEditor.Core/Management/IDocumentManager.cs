using GEditor.Core.Documents;

namespace GEditor.Core.Management;

/// <summary>文档管理器接口</summary>
public interface IDocumentManager
{
    /// <summary>当前活跃文档</summary>
    Document? ActiveDocument { get; }

    /// <summary>所有打开的文档</summary>
    IReadOnlyList<Document> Documents { get; }

    /// <summary>创建新空白文档</summary>
    Document CreateNew();

    /// <summary>打开文件</summary>
    Task<Document> OpenAsync(string filePath);

    /// <summary>关闭指定文档</summary>
    void Close(Document document);

    /// <summary>切换活跃文档</summary>
    void SetActive(Document document);
}
