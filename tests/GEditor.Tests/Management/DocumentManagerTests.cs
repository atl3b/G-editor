using GEditor.Core.Documents;
using GEditor.Core.IO;
using GEditor.Core.Management;
using Moq;
using Xunit;

namespace GEditor.Tests.Management;

public class DocumentManagerTests
{
    private readonly Mock<ITextFileService> _fileServiceMock;
    private readonly DocumentManager _manager;

    public DocumentManagerTests()
    {
        _fileServiceMock = new Mock<ITextFileService>();
        _manager = new DocumentManager(_fileServiceMock.Object);
    }

    #region CreateNew

    [Fact]
    public void CreateNew_CreatesDocumentAndSetsActive()
    {
        var doc = _manager.CreateNew();

        Assert.NotNull(doc);
        Assert.Same(doc, _manager.ActiveDocument);
        Assert.Single(_manager.Documents);
        Assert.True(doc.IsNew);
    }

    [Fact]
    public void CreateNew_MultipleDocuments_AllInCollection()
    {
        var doc1 = _manager.CreateNew();
        var doc2 = _manager.CreateNew();
        var doc3 = _manager.CreateNew();

        Assert.Equal(3, _manager.Documents.Count);
        Assert.Contains(doc1, _manager.Documents);
        Assert.Contains(doc2, _manager.Documents);
        Assert.Contains(doc3, _manager.Documents);
        // Last created is active
        Assert.Same(doc3, _manager.ActiveDocument);
    }

    #endregion

    #region OpenAsync

    [Fact]
    public async Task OpenAsync_OpensFileAndAddsToCollection()
    {
        var expectedDoc = new Document("C:\\test.txt");
        expectedDoc.LoadText("hello world");
        _fileServiceMock.Setup(s => s.Open("C:\\test.txt")).Returns(expectedDoc);

        var doc = await _manager.OpenAsync("C:\\test.txt");

        Assert.NotNull(doc);
        Assert.Same(expectedDoc, doc);
        Assert.Same(doc, _manager.ActiveDocument);
        Assert.Single(_manager.Documents);
        _fileServiceMock.Verify(s => s.Open("C:\\test.txt"), Times.Once);
    }

    [Fact]
    public async Task OpenAsync_AfterCreateNew_BothInCollection()
    {
        _manager.CreateNew();

        var openedDoc = new Document("C:\\open.txt");
        openedDoc.LoadText("opened content");
        _fileServiceMock.Setup(s => s.Open("C:\\open.txt")).Returns(openedDoc);

        var doc = await _manager.OpenAsync("C:\\open.txt");

        Assert.Equal(2, _manager.Documents.Count);
        Assert.Same(doc, _manager.ActiveDocument);
    }

    #endregion

    #region Close

    [Fact]
    public void Close_RemovesDocumentFromCollection()
    {
        var doc = _manager.CreateNew();
        _manager.Close(doc);

        Assert.Empty(_manager.Documents);
    }

    [Fact]
    public void Close_ActiveDocument_SwitchesToNext()
    {
        var doc1 = _manager.CreateNew();
        var doc2 = _manager.CreateNew();

        // Close the active document (doc2)
        _manager.Close(doc2);

        Assert.Single(_manager.Documents);
        Assert.DoesNotContain(doc2, _manager.Documents);
        // Should switch to remaining document
        Assert.Same(doc1, _manager.ActiveDocument);
    }

    [Fact]
    public void Close_NonActiveDocument_KeepsActive()
    {
        var doc1 = _manager.CreateNew();
        var doc2 = _manager.CreateNew();

        // Close non-active (doc1), keep doc2 as active
        _manager.Close(doc1);

        Assert.Single(_manager.Documents);
        Assert.Same(doc2, _manager.ActiveDocument);
    }

    [Fact]
    public void Close_LastDocument_ActiveBecomesNull()
    {
        var doc = _manager.CreateNew();
        _manager.Close(doc);

        Assert.Null(_manager.ActiveDocument);
    }

    [Fact]
    public void Close_MultipleDocuments_CorrectSwitch()
    {
        var doc1 = _manager.CreateNew();
        var doc2 = _manager.CreateNew();
        var doc3 = _manager.CreateNew();

        // Close middle document (doc2)
        _manager.Close(doc2);

        Assert.Equal(2, _manager.Documents.Count);
        Assert.Same(doc3, _manager.ActiveDocument); // Should switch to next available
    }

    [Fact]
    public void Close_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _manager.Close(null!));
    }

    #endregion

    #region SetActive

    [Fact]
    public void SetActive_ValidDocument_SwitchesActive()
    {
        var doc1 = _manager.CreateNew();
        var doc2 = _manager.CreateNew();

        _manager.SetActive(doc1);

        Assert.Same(doc1, _manager.ActiveDocument);
    }

    [Fact]
    public void SetActive_NotManaged_ThrowsArgumentException()
    {
        var externalDoc = new Document();

        Assert.Throws<ArgumentException>(() => _manager.SetActive(externalDoc));
    }

    [Fact]
    public void SetActive_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _manager.SetActive(null!));
    }

    [Fact]
    public void SetActive_ClosedDocument_ThrowsArgumentException()
    {
        var doc = _manager.CreateNew();
        _manager.Close(doc);

        Assert.Throws<ArgumentException>(() => _manager.SetActive(doc));
    }

    #endregion

    #region Documents 集合完整性

    [Fact]
    public void Documents_ReturnsReadOnlyList()
    {
        _manager.CreateNew();
        _manager.CreateNew();

        var docs = _manager.Documents;
        Assert.IsAssignableFrom<System.Collections.Generic.IReadOnlyList<Document>>(docs);
        Assert.Equal(2, docs.Count);
    }

    [Fact]
    public void ActiveDocument_InitiallyNull()
    {
        Assert.Null(_manager.ActiveDocument);
    }

    #endregion
}
