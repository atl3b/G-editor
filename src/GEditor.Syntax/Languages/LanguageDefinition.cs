using System.Collections.Generic;

namespace GEditor.Syntax;

/// <summary>语言定义 — 不可变值对象</summary>
public sealed record LanguageDefinition : ILanguageDefinition
{
    public string LanguageId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlySet<string> FileExtensions { get; init; } = new HashSet<string>();
    public bool SupportsMultiline { get; init; }
}
