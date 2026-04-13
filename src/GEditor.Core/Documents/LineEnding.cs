namespace GEditor.Core.Documents;

/// <summary>换行符类型枚举</summary>
public enum LineEnding
{
    CRLF,   // \r\n (Windows)
    LF,     // \n (Unix/macOS)
    CR,     // \r (Classic Mac)
    Unknown // 尚未检测
}
