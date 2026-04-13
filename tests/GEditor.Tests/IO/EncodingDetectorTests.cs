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
}
