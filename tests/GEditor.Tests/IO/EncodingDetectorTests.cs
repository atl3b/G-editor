using System.Text;
using GEditor.Core.Documents;
using GEditor.Core.IO;
using Xunit;

namespace GEditor.Tests.IO;

public class EncodingDetectorTests
{
    private readonly EncodingDetector _detector = new();

    [Fact]
    public void Detect_EmptyBytes_ReturnsDefaultUtf8()
    {
        var result = _detector.Detect(Array.Empty<byte>());

        Assert.Equal("UTF-8", result.DisplayName);
        Assert.False(result.HasBom);
    }

    [Fact]
    public void Detect_Utf8Bom_ReturnsUtf8WithBom()
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("hello")).ToArray();
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8 with BOM", result.DisplayName);
        Assert.True(result.HasBom);
    }

    [Fact]
    public void Detect_Utf16LeBom_ReturnsUtf16Le()
    {
        var bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("hello")).ToArray();
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-16 LE", result.DisplayName);
        Assert.True(result.HasBom);
    }

    [Fact]
    public void Detect_Utf16BeBom_ReturnsUtf16Be()
    {
        var bytes = Encoding.BigEndianUnicode.GetPreamble().Concat(Encoding.BigEndianUnicode.GetBytes("hello")).ToArray();
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-16 BE", result.DisplayName);
        Assert.True(result.HasBom);
    }

    [Fact]
    public void Detect_PlainAscii_ReturnsUtf8NoBom()
    {
        var bytes = Encoding.ASCII.GetBytes("hello world");
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8", result.DisplayName);
        Assert.False(result.HasBom);
    }

    [Fact]
    public void Detect_NullBytes_ThrowsOrReturnsDefault()
    {
        var result = _detector.Detect((byte[]?)null);

        Assert.NotNull(result);
        Assert.Equal("UTF-8", result.DisplayName);
    }

    #region GBK / 中文文件检测

    [Fact]
    public void Detect_ChineseUtf8NoBom_ReturnsUtf8()
    {
        var text = "你好，世界！这是中文测试文件。";
        var bytes = Encoding.UTF8.GetBytes(text);
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8", result.DisplayName);
        Assert.False(result.HasBom);
        Assert.IsType<UTF8Encoding>(result.Encoding);
    }

    [Fact]
    public void Detect_ChineseWithPunctuationUtf8_ReturnsUtf8()
    {
        var text = "函数定义：public void Main() { /* 注释 */ }";
        var bytes = Encoding.UTF8.GetBytes(text);
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8", result.DisplayName);
        // 验证可以用此编码正确解码
        var decoded = result.Encoding.GetString(bytes);
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Detect_MixedChineseAndEnglishUtf8_ReturnsUtf8()
    {
        var text = "Hello 世界! 123 数字。";
        var bytes = Encoding.UTF8.GetBytes(text);
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8", result.DisplayName);
        var roundtrip = result.Encoding.GetString(bytes);
        Assert.Equal(text, roundtrip);
    }

    [Fact]
    public void Detect_LongChineseTextUtf8_ReturnsUtf8Consistently()
    {
        // 较长的中文文本，确保启发式不会误判为 UTF-16
        var sb = new StringBuilder();
        for (int i = 0; i < 100; i++)
            sb.AppendLine($"第{i + 1}行：这是一个包含中文字符的测试文本，包含标点符号：\uff0c\u3002\uff01\uff1f\uff1b\u201c\u201d\u2018\u2019\uff08\uff09");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8", result.DisplayName);
        Assert.False(result.HasBom);
    }

    [Fact]
    public void Detect_ChineseUtf8WithBom_ReturnsUtf8WithBom()
    {
        var text = "中文内容";
        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(text);
        var bytes = preamble.Concat(content).ToArray();

        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8 with BOM", result.DisplayName);
        Assert.True(result.HasBom);
        var decoded = result.Encoding.GetString(bytes.AsSpan(preamble.Length));
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Detect_Utf16LeNoBom_WithNullBytesAtOddPositions_Detected()
    {
        // UTF-16 LE 无 BOM 但有 null 字节在奇数位置
        var text = "Hello 中文测试";
        var bytes = Encoding.Unicode.GetBytes(text); // UTF-16 LE

        var result = _detector.Detect(bytes);

        // 奇数位有 null → 检测为 UTF-16 LE
        Assert.Equal("UTF-16 LE", result.DisplayName);
        var decoded = result.Encoding.GetString(bytes);
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Detect_Utf16BeNoBom_WithNullBytesAtEvenPositions_Detected()
    {
        // UTF-16 BE 无 BOM 但有 null 字节在偶数位置
        var text = "Hello 中文测试";
        var bytes = Encoding.BigEndianUnicode.GetBytes(text);

        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-16 BE", result.DisplayName);
        var decoded = result.Encoding.GetString(bytes);
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Detect_JapaneseKoreanUtf8_ReturnsUtf8()
    {
        // 日文和韩文也是有效的 UTF-8 多字节序列
        var text = "こんにちは 한국어 你好"; // 日文 韩文 中文
        var bytes = Encoding.UTF8.GetBytes(text);
        var result = _detector.Detect(bytes);

        Assert.Equal("UTF-8", result.DisplayName);
        var roundtrip = result.Encoding.GetString(bytes);
        Assert.Equal(text, roundtrip);
    }

    [Fact]
    public void Detect_EmptyArray_NotSameAsNull()
    {
        var emptyResult = _detector.Detect(Array.Empty<byte>());
        var nullResult = _detector.Detect((byte[]?)null);

        // 都应返回合理的默认值，不崩溃
        Assert.NotNull(emptyResult);
        Assert.NotNull(nullResult);
        Assert.Equal(emptyResult.DisplayName, nullResult.DisplayName);
    }

    #endregion
}
