using GEditor.Core.Documents;

namespace GEditor.Core.IO;

/// <summary>文件读写服务接口</summary>
public interface ITextFileService
{
    /// <summary>打开文件：检测编码+换行符 → 加载内容 → 返回 Document</summary>
    Document Open(string filePath);

    /// <summary>保存文件：按 Document 的编码和换行符写入磁盘</summary>
    void Save(Document document);

    /// <summary>另存为：指定新路径，按指定编码和换行符写入</summary>
    void SaveAs(Document document, string newFilePath, System.Text.Encoding? encoding = null, LineEnding? lineEnding = null);
}
