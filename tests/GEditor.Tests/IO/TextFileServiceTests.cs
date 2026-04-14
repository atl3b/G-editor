using GEditor.Core.Documents;
using GEditor.Core.IO;
using System.Text;
using Xunit;

namespace GEditor.Tests.IO;

public class TextFileServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly TextFileService _service;

    public TextFileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "GEditorTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _service = new TextFileService(new EncodingDetector(), new LineEndingDetector());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Open_Utf8File_ReadsCorrectly()
    {
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "hello\nworld", System.Text.Encoding.UTF8);

        var doc = _service.Open(filePath);

        Assert.Equal(2, doc.Buffer.LineCount);
        Assert.Equal("hello", doc.Buffer[0]);
        Assert.Equal("world", doc.Buffer[1]);
        Assert.Equal(filePath, doc.FilePath);
        Assert.False(doc.IsDirty);
    }

    [Fact]
    public void Open_DetectsCrlf()
    {
        var filePath = Path.Combine(_tempDir, "crlf.txt");
        File.WriteAllText(filePath, "line1\r\nline2\r\nline3");

        var doc = _service.Open(filePath);

        Assert.Equal(3, doc.Buffer.LineCount);
        Assert.Equal(LineEnding.CRLF, doc.LineEndingInfo.DetectedLineEnding);
    }

    [Fact]
    public void Open_DetectsLf()
    {
        var filePath = Path.Combine(_tempDir, "lf.txt");
        File.WriteAllText(filePath, "line1\nline2\nline3");

        var doc = _service.Open(filePath);

        Assert.Equal(3, doc.Buffer.LineCount);
        Assert.Equal(LineEnding.LF, doc.LineEndingInfo.DetectedLineEnding);
    }

    [Fact]
    public void Save_PreservesContent()
    {
        var filePath = Path.Combine(_tempDir, "save.txt");
        var doc = new Document(filePath);
        doc.LoadText("hello\nworld");

        _service.Save(doc);

        var content = File.ReadAllText(filePath);
        Assert.Contains("hello", content);
        Assert.Contains("world", content);
    }

    [Fact]
    public void Save_MarksDocumentAsSaved()
    {
        var filePath = Path.Combine(_tempDir, "saved.txt");
        var doc = new Document(filePath);
        doc.LoadText("test");
        doc.ExecuteCommand(new Core.Editing.InsertTextCommand(0, 4, " data"));

        Assert.True(doc.IsDirty);
        _service.Save(doc);
        Assert.False(doc.IsDirty);
    }

    #region SaveAs 编码转换测试

    [Fact]
    public void SaveAs_WithDifferentEncoding_SavesInNewEncoding()
    {
        // Create original file with UTF-8 content
        var originalPath = Path.Combine(_tempDir, "original.txt");
        File.WriteAllText(originalPath, "hello world", Encoding.UTF8);

        var doc = _service.Open(originalPath);

        // SaveAs with a different encoding (UTF-16)
        var newPath = Path.Combine(_tempDir, "converted.txt");
        var utf16 = Encoding.Unicode; // UTF-16
        _service.SaveAs(doc, newPath, encoding: utf16);

        // Read back as UTF-16 to verify
        var bytes = File.ReadAllBytes(newPath);
        var decoded = utf16.GetString(bytes);

        // UTF-16 has BOM prefix; trim it for content comparison
        Assert.Equal("hello world", decoded.Trim('\uFEFF'));
        Assert.Equal(utf16, doc.EncodingInfo.Encoding);
    }

    [Fact]
    public void SaveAs_PreservesOriginalFile()
    {
        var originalPath = Path.Combine(_tempDir, "original.txt");
        File.WriteAllText(originalPath, "original content");

        var doc = _service.Open(originalPath);

        var newPath = Path.Combine(_tempDir, "copy.txt");
        _service.SaveAs(doc, newPath);

        // Original file should still exist and have same content
        Assert.True(File.Exists(originalPath));
        Assert.Equal("original content", File.ReadAllText(originalPath));
    }

    #endregion

    #region SaveAs 换行符转换测试

    [Fact]
    public void SaveAs_WithLineEnding_ConvertsLineEndings()
    {
        var filePath = Path.Combine(_tempDir, "lf_file.txt");
        File.WriteAllText(filePath, "line1\nline2\nline3", Encoding.UTF8);

        var doc = _service.Open(filePath);
        Assert.Equal(LineEnding.LF, doc.LineEndingInfo.DetectedLineEnding);

        var newPath = Path.Combine(_tempDir, "crlf_file.txt");
        _service.SaveAs(doc, newPath, lineEnding: LineEnding.CRLF);

        // Read raw bytes to check line ending conversion
        var rawContent = File.ReadAllText(newPath, Encoding.UTF8);
        Assert.Contains("\r\n", rawContent);
        // Verify no standalone LF (all \n are part of \r\n)
        int crlfCount = CountOccurrences(rawContent, "\r\n");
        int lfCount = CountOccurrences(rawContent, "\n");
        Assert.Equal(crlfCount, lfCount);

        // Document's ActiveLineEnding should be updated
        Assert.Equal(LineEnding.CRLF, doc.LineEndingInfo.ActiveLineEnding);
    }

    [Fact]
    public void SaveAs_LfToCr_ConvertsCorrectly()
    {
        var filePath = Path.Combine(_tempDir, "lf_source.txt");
        File.WriteAllText(filePath, "a\nb\nc", Encoding.UTF8);

        var doc = _service.Open(filePath);
        var newPath = Path.Combine(_tempDir, "cr_target.txt");

        _service.SaveAs(doc, newPath, lineEnding: LineEnding.CR);

        var rawContent = File.ReadAllText(newPath, Encoding.UTF8);
        Assert.Contains("\r", rawContent);
        Assert.DoesNotContain("\n", rawContent); // CR only
        Assert.Equal(LineEnding.CR, doc.LineEndingInfo.ActiveLineEnding);
    }

    [Fact]
    public void SaveAs_CrlfToLf_ConvertsCorrectly()
    {
        var filePath = Path.Combine(_tempDir, "crlf_source.txt");
        File.WriteAllText(filePath, "a\r\nb\r\nc", Encoding.UTF8);

        var doc = _service.Open(filePath);
        var newPath = Path.Combine(_tempDir, "lf_target.txt");

        _service.SaveAs(doc, newPath, lineEnding: LineEnding.LF);

        var rawContent = File.ReadAllText(newPath, Encoding.UTF8);
        Assert.All(rawContent.Split('\n'), line => Assert.DoesNotContain("\r", line));
        Assert.Equal(LineEnding.LF, doc.LineEndingInfo.ActiveLineEnding);
    }

    #endregion

    #region SaveAs 综合测试

    [Fact]
    public void SaveAs_UpdatesFilePath()
    {
        var originalPath = Path.Combine(_tempDir, "orig.txt");
        File.WriteAllText(originalPath, "test");

        var doc = _service.Open(originalPath);
        var newPath = Path.Combine(_tempDir, "newname.txt");

        _service.SaveAs(doc, newPath);

        Assert.Equal(newPath, doc.FilePath);
    }

    [Fact]
    public void SaveAs_MarksAsNotDirty()
    {
        var originalPath = Path.Combine(_tempDir, "dirty.txt");
        File.WriteAllText(originalPath, "initial");

        var doc = _service.Open(originalPath);
        doc.ExecuteCommand(new Core.Editing.InsertTextCommand(0, 7, " modified"));
        Assert.True(doc.IsDirty);

        var newPath = Path.Combine(_tempDir, "saved_clean.txt");
        _service.SaveAs(doc, newPath);

        Assert.False(doc.IsDirty);
    }

    #endregion

    private static int CountOccurrences(string text, string pattern)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
            return 0;

        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
