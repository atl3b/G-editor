namespace GEditor.Core.Documents;

/// <summary>文档换行符元信息 — 值对象</summary>
public sealed class DocumentLineEndingInfo
{
    public LineEnding DetectedLineEnding { get; init; } = LineEnding.Unknown;

    private LineEnding _activeLineEnding = LineEnding.Unknown;
    public LineEnding ActiveLineEnding
    {
        get => _activeLineEnding;
        set => _activeLineEnding = value;
    }

    /// <summary>当前活跃换行符序列</summary>
    public string Sequence => ActiveLineEnding switch
    {
        LineEnding.CRLF => "\r\n",
        LineEnding.LF   => "\n",
        LineEnding.CR   => "\r",
        _               => Environment.NewLine
    };

    public static DocumentLineEndingInfo Default => new();
}
