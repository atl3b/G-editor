using GEditor.Core.Documents;

namespace GEditor.Core.IO;

/// <summary>编码检测器接口</summary>
public interface IEncodingDetector
{
    /// <summary>检测文件编码，优先 BOM → 启发式 → 回退默认</summary>
    DocumentEncodingInfo Detect(string filePath);

    /// <summary>从字节数组检测编码</summary>
    DocumentEncodingInfo Detect(byte[] fileBytes);
}
