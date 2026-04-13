using GEditor.Core.Documents;
using GEditor.Core.IO;

namespace GEditor.Core.Management;

/// <summary>文档管理器 — 管理多文档生命周期</summary>
public sealed class DocumentManager : IDocumentManager
{
    private readonly List<Document> _documents = new();
    private readonly ITextFileService _fileService;

    public Document? ActiveDocument { get; private set; }
    public IReadOnlyList<Document> Documents => _documents.AsReadOnly();

    public DocumentManager(ITextFileService fileService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    public Document CreateNew()
    {
        var document = new Document();
        _documents.Add(document);
        ActiveDocument = document;
        return document;
    }

    public Task<Document> OpenAsync(string filePath)
    {
        var document = _fileService.Open(filePath);
        _documents.Add(document);
        ActiveDocument = document;
        return Task.FromResult(document);
    }

    public void Close(Document document)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));

        _documents.Remove(document);
        document.Dispose();

        if (ActiveDocument == document)
            ActiveDocument = _documents.FirstOrDefault();
    }

    public void SetActive(Document document)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));
        if (!_documents.Contains(document))
            throw new ArgumentException("Document is not managed by this manager.");

        ActiveDocument = document;
    }
}
