using GEditor.Core.Documents;
using GEditor.Core.IO;
using Xunit;

namespace GEditor.Tests.IO;

public class LineEndingDetectorTests
{
    private readonly LineEndingDetector _detector = new();

    #region CRLF 检测

    [Fact]
    public void Detect_CrlfText_ReturnsCRLF()
    {
        Assert.Equal(LineEnding.CRLF, _detector.Detect("line1\r\nline2\r\nline3"));
    }

    [Fact]
    public void Detect_SingleCrlf_ReturnsCRLF()
    {
        Assert.Equal(LineEnding.CRLF, _detector.Detect("hello\r\nworld"));
    }

    [Fact]
    public void Detect_OnlyCrlf_ReturnsCRLF()
    {
        Assert.Equal(LineEnding.CRLF, _detector.Detect("\r\n"));
    }

    #endregion

    #region LF 检测

    [Fact]
    public void Detect_LfText_ReturnsLF()
    {
        Assert.Equal(LineEnding.LF, _detector.Detect("line1\nline2\nline3"));
    }

    [Fact]
    public void Detect_SingleLf_ReturnsLF()
    {
        Assert.Equal(LineEnding.LF, _detector.Detect("hello\nworld"));
    }

    #endregion

    #region CR 检测

    [Fact]
    public void Detect_CrText_ReturnsCR()
    {
        Assert.Equal(LineEnding.CR, _detector.Detect("line1\rline2\rline3"));
    }

    [Fact]
    public void Detect_SingleCr_ReturnsCR()
    {
        Assert.Equal(LineEnding.CR, _detector.Detect("hello\rworld"));
    }

    #endregion

    #region Unknown / 空输入

    [Fact]
    public void Detect_EmptyString_ReturnsUnknown()
    {
        Assert.Equal(LineEnding.Unknown, _detector.Detect(""));
    }

    [Fact]
    public void Detect_NullString_ReturnsUnknown()
    {
        Assert.Equal(LineEnding.Unknown, _detector.Detect(null!));
    }

    [Fact]
    public void Detect_PlainTextNoNewlines_ReturnsUnknown()
    {
        Assert.Equal(LineEnding.Unknown, _detector.Detect("hello world"));
    }

    #endregion

    #region 混合换行符（主导类型判定）

    [Fact]
    public void Detect_MixedCrlfDominant_ReturnsCRLF()
    {
        // 2 CRLF + 1 LF → CRLF 主导
        string text = "a\r\nb\r\nc\nd";
        Assert.Equal(LineEnding.CRLF, _detector.Detect(text));
    }

    [Fact]
    public void Detect_MixedLfDominant_ReturnsLF()
    {
        // 多于 CRLF 数量的 LF → LF 主导
        string text = "a\nb\nc\nd\r\ne";
        Assert.Equal(LineEnding.LF, _detector.Detect(text));
    }

    [Fact]
    public void Detect_MixedCrAndLf_LfWinsWhenEqualOrMore()
    {
        // CR 和 LF 各一个，LF >= CR → LF
        string text = "a\rb\nc";
        Assert.Equal(LineEnding.LF, _detector.Detect(text));
    }

    [Fact]
    public void Detect_AllThreeTypes_ReturnsDominant()
    {
        // 2 CRLF, 2 LF, 1 CR → CRLF >= LF 且 CRLF >= CR
        string text = "a\r\nb\r\nc\nd\ne\rf";
        Assert.Equal(LineEnding.CRLF, _detector.Detect(text));
    }

    #endregion
}
