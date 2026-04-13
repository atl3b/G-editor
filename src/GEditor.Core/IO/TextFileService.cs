using System.Text;
using GEditor.Core.Documents;

namespace GEditor.Core.IO;

/// <summary>文件读写服务</summary>
public sealed class TextFileService : ITextFileService
{
    private readonly IEncodingDetector _encodingDetector;
    private readonly ILineEndingDetector _lineEndingDetector;

    public TextFileService(IEncodingDetector encodingDetector, ILineEndingDetector lineEndingDetector)
    {
        _encodingDetector = encodingDetector ?? throw new ArgumentNullException(nameof(encodingDetector));
        _lineEndingDetector = lineEndingDetector ?? throw new ArgumentNullException(nameof(lineEndingDetector));
    }

    public Document Open(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        var bytes = File.ReadAllBytes(filePath);
        var encodingInfo = _encodingDetector.Detect(bytes);

        // Remove BOM for decoding
        int preambleLength = encodingInfo.Encoding.GetPreamble().Length;
        string content = encodingInfo.Encoding.GetString(bytes, preambleLength, bytes.Length - preambleLength);

        var lineEnding = _lineEndingDetector.Detect(content);

        var document = new Document(filePath)
        {
            EncodingInfo = encodingInfo,
            LineEndingInfo = new DocumentLineEndingInfo
            {
                DetectedLineEnding = lineEnding,
                ActiveLineEnding = lineEnding != LineEnding.Unknown ? lineEnding : LineEnding.LF
            }
        };
        document.LoadText(content);

        return document;
    }

    public void Save(Document document)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));

        var text = document.GetFullText();
        var encoding = document.EncodingInfo.Encoding;
        var preamble = encoding.GetPreamble();

        using var stream = new FileStream(document.FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        if (preamble.Length > 0)
            stream.Write(preamble, 0, preamble.Length);

        var textBytes = encoding.GetBytes(text);
        stream.Write(textBytes, 0, textBytes.Length);

        document.MarkAsSaved();
    }

    public void SaveAs(Document document, string newFilePath, Encoding? encoding = null, LineEnding? lineEnding = null)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));

        string oldText = document.GetFullText();
        var oldEncoding = document.EncodingInfo.Encoding;

        if (encoding != null || lineEnding != null)
        {
            if (encoding != null)
                document.EncodingInfo = new DocumentEncodingInfo
                {
                    Encoding = encoding,
                    HasBom = encoding.GetPreamble().Length > 0,
                    DisplayName = encoding.EncodingName
                };

            if (lineEnding != null && lineEnding.Value != LineEnding.Unknown)
                document.LineEndingInfo = new DocumentLineEndingInfo
                {
                    DetectedLineEnding = document.LineEndingInfo.DetectedLineEnding,
                    ActiveLineEnding = lineEnding.Value
                };
        }

        document.FilePath = newFilePath;

        var text = document.GetFullText();
        var enc = document.EncodingInfo.Encoding;
        var preamble = enc.GetPreamble();

        using var stream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        if (preamble.Length > 0)
            stream.Write(preamble, 0, preamble.Length);

        var textBytes = enc.GetBytes(text);
        stream.Write(textBytes, 0, textBytes.Length);

        document.MarkAsSaved();
    }
}
