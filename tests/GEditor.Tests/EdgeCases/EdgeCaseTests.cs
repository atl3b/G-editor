using GEditor.Core.Buffer;
using GEditor.Core.Documents;
using GEditor.Core.Editing;
using GEditor.Core.Search;
using Xunit;

namespace GEditor.Tests.EdgeCases;

/// <summary>
/// 边界/异常测试套件：Unicode 多字节字符、空输入、极端参数
/// 覆盖：EditorBuffer / Document / SearchService / UndoRedoManager 的边界场景
/// </summary>
public class EdgeCaseTests
{
    #region Unicode 多字节字符

    [Fact]
    public void Buffer_InsertCJK_MaintainsCorrectContent()
    {
        // EditorBuffer 列索引基于 C# 字符数，非字节偏移
        var buffer = new EditorBuffer();
        buffer.SetAllText("初始文本"); // 4 个中文字符，Length = 4
        buffer.Insert(0, 4, "中文追加");

        Assert.Equal("初始文本中文追加", buffer[0]);
    }

    [Fact]
    public void Buffer_DeleteCJK_DeletesCorrectNumberOfChars()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("你好世界");
        // 删除 2 个中文字符（"世界"）
        buffer.Delete(0, 2, 2);

        Assert.Equal("你好", buffer[0]);
    }

    [Fact]
    public void Buffer_Emoji_InsertAndRetrieve()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("start ");
        buffer.Insert(0, 6, "🎉🎊");

        Assert.Equal("start 🎉🎊", buffer[0]);
    }

    [Fact]
    public void Buffer_ReplaceWithEmoji_ContentPreserved()
    {
        // "replace this part here" → replace first 22 chars entirely with emoji text
        var buffer = new EditorBuffer();
        buffer.SetAllText("replace this part here");
        buffer.Replace(0, 0, 22, "\U0001F525emoji here");

        Assert.Equal("\U0001F525emoji here", buffer[0]);
    }

    [Fact]
    public void Buffer_MixedScript_MultipleLines()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("English 日本語\n한국어 العربية\n🌍 emoji line");

        Assert.Equal(3, buffer.LineCount);
        Assert.Contains("日本語", buffer[0]);
        Assert.Contains("한국어", buffer[1]);
        Assert.Contains("🌍", buffer[2]);
    }

    [Fact]
    public void Search_FindCJK_FindsCorrectMatch()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("这是一个测试，测试搜索功能是否正常");

        var service = new SearchService();
        var query = SearchQuery.Create("测试");
        var matches = service.FindAll(buffer, query);

        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void Search_FindEmoji_FindsMatch()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello 🎉 world 🎉 end");

        var service = new SearchService();
        var query = SearchQuery.Create("🎉");
        var matches = service.FindAll(buffer, query);

        Assert.Equal(2, matches.Count);
    }

    #endregion

    #region 空输入 / null 边界

    [Fact]
    public void Buffer_SetAllText_EmptyString_CreatesSingleEmptyLine()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("");

        Assert.Equal(1, buffer.LineCount);
        Assert.Equal("", buffer[0]);
    }

    [Fact]
    public void Buffer_GetAllText_EmptyBuffer_ReturnsEmpty()
    {
        var buffer = new EditorBuffer();

        Assert.Equal("", buffer.GetAllText("\n"));
    }

    [Fact]
    public void Buffer_LineCount_ZeroForDefault()
    {
        var buffer = new EditorBuffer();

        Assert.Equal(0, buffer.LineCount);
    }

    [Fact]
    public void Buffer_InsertAtPositionZeroIntoEmpty_Works()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("");
        buffer.Insert(0, 0, "hello");

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void Search_NullBuffer_DoesNotThrow()
    {
        var service = new SearchService();
        var matches = service.FindAll(null!, SearchQuery.Create("test"));

        Assert.Empty(matches);
    }

    [Fact]
    public void Search_EmptyPattern_DoesNotThrow()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("some text");
        var service = new SearchService();

        var matches = service.FindAll(buffer, SearchQuery.Create(""));
        Assert.Empty(matches);

        var count = service.CountMatches(buffer, SearchQuery.Create(""));
        Assert.Equal(0, count);
    }

    [Fact]
    public void Search_CountMatches_NullBuffer_ReturnsZero()
    {
        var service = new SearchService();
        var count = service.CountMatches(null!, SearchQuery.Create("test"));

        Assert.Equal(0, count);
    }

    #endregion

    #region 极端参数

    [Fact]
    public void Buffer_InsertAtEndOfLine_Appends()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abc"); // length = 3
        buffer.Insert(0, 3, "def");

        Assert.Equal("abcdef", buffer[0]);
    }

    [Fact]
    public void Buffer_InsertEmptyString_NoChange()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        buffer.Insert(0, 2, "");

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void Buffer_DeleteZeroLength_NoChange()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("hello");
        buffer.Delete(0, 1, 0);

        Assert.Equal("hello", buffer[0]);
    }

    [Fact]
    public void Buffer_ReplaceSameLengthText_SwapsContent()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef");
        buffer.Replace(0, 0, 3, "xyz");

        Assert.Equal("xyzdef", buffer[0]);
    }

    [Fact]
    public void Buffer_ReplaceWithEmpty_Deletes()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("abcdef");
        buffer.Replace(0, 0, 3, "");

        Assert.Equal("def", buffer[0]);
    }

    [Fact]
    public void Buffer_SingleLine_GetRangeEntireLine()
    {
        // "only one line" 长度为 13 个字符
        var buffer = new EditorBuffer();
        buffer.SetAllText("only one line");
        var result = buffer.GetRange(0, 0, 0, 13);

        Assert.Equal("only one line", result);
    }

    [Fact]
    public void Buffer_MultiLine_SetAndGetConsistent()
    {
        var text = "line1\nline2\nline3";
        var buffer = new EditorBuffer();
        buffer.SetAllText(text);

        Assert.Equal(text, buffer.GetAllText("\n"));
        Assert.Equal(3, buffer.LineCount);
    }

    [Fact]
    public void UndoRedo_ExecuteNullCommand_DoesNotThrowOnGuardedCall()
    {
        var buffer = new EditorBuffer();
        buffer.SetAllText("test");
        var manager = new UndoRedoManager();

        // Execute 内部有 ArgumentNullException.ThrowIfNull
        Assert.Throws<ArgumentNullException>(() => manager.Execute(null!, buffer));
    }

    [Fact]
    public void DocumentEncodingInfo_DefaultValues_AreSensible()
    {
        var info = new DocumentEncodingInfo();

        Assert.NotNull(info.Encoding);
        Assert.False(info.HasBom);
        Assert.NotEmpty(info.DisplayName);
    }

    [Fact]
    public void DocumentLineEndingInfo_Default_IsUnknown()
    {
        var info = new DocumentLineEndingInfo();

        Assert.Equal(LineEnding.Unknown, info.DetectedLineEnding);
        Assert.Equal(LineEnding.Unknown, info.ActiveLineEnding);
    }

    [Fact]
    public void DocumentLineEndingInfo_Sequence_ForEachType()
    {
        foreach (var ending in new[] { LineEnding.CRLF, LineEnding.LF, LineEnding.CR })
        {
            var info = new DocumentLineEndingInfo { DetectedLineEnding = ending, ActiveLineEnding = ending };
            Assert.NotEmpty(info.Sequence);
        }
    }

    #endregion

    #region 大文本压力（轻量级）

    [Fact]
    public void Buffer_LargeSingleLine_HandlesThousandsOfChars()
    {
        var longText = new string('x', 10000);
        var buffer = new EditorBuffer();
        buffer.SetAllText(longText);

        Assert.Equal(10000, buffer.GetLineLength(0));
        buffer.Insert(0, 5000, "MARKER");
        Assert.Contains("MARKER", buffer[0]);
        Assert.Equal(10006, buffer.GetLineLength(0)); // 10000 + 6

        buffer.Delete(0, 5000, 6);
        Assert.DoesNotContain("MARKER", buffer[0]);
        Assert.Equal(10000, buffer.GetLineLength(0));
    }

    [Fact]
    public void Buffer_LargeMultiLine_HandlesThousandsOfLines()
    {
        var lines = Enumerable.Range(0, 1000).Select(i => $"Line {i}: some content").ToArray();
        var text = string.Join("\n", lines);
        var buffer = new EditorBuffer();
        buffer.SetAllText(text);

        Assert.Equal(1000, buffer.LineCount);
        Assert.Equal("Line 999: some content", buffer[999]);
    }

    [Fact]
    public void Search_LargeBuffer_PerformanceDoesNotHang()
    {
        var lines = Enumerable.Range(0, 500).Select(i => $"Line {i} with pattern and other words").ToArray();
        var buffer = new EditorBuffer();
        buffer.SetAllText(string.Join("\n", lines));

        var service = new SearchService();
        var query = SearchQuery.Create("pattern");
        var matches = service.FindAll(buffer, query);

        Assert.Equal(500, matches.Count);
    }

    #endregion
}
