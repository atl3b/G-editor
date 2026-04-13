namespace GEditor.Core.Documents;

/// <summary>文档编码元信息 — 值对象</summary>
public sealed class DocumentEncodingInfo
{
    public System.Text.Encoding Encoding { get; init; } = System.Text.Encoding.UTF8;
    public bool HasBom { get; init; }
    public string DisplayName { get; init; } = "UTF-8";

    public static DocumentEncodingInfo Default => new();
}
