namespace GEditor.Syntax;

/// <summary>
/// 语言定义接口 — 描述一种语言的元数据和扩展名映射。
/// </summary>
public interface ILanguageDefinition
{
    string LanguageId { get; }
    string DisplayName { get; }
    IReadOnlySet<string> FileExtensions { get; }
    bool SupportsMultiline { get; }
}
