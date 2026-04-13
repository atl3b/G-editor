using GEditor.Core.Documents;

namespace GEditor.Core.IO;

/// <summary>换行符检测器接口</summary>
public interface ILineEndingDetector
{
    /// <summary>从文本内容中检测主导换行符类型</summary>
    LineEnding Detect(string text);
}
