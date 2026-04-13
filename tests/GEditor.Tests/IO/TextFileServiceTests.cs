using GEditor.Core.Documents;
using GEditor.Core.IO;
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
}
