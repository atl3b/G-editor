using GEditor.Core.Search;
using Xunit;

namespace GEditor.Tests.Search;

/// <summary>
/// Search 值对象专项测试：SearchMatch / SearchQuery / SearchOptions / ReplaceOptions / ReplaceResult
/// 覆盖：构造、默认值、Equals/GetHashCode、静态工厂方法、边界值
/// </summary>
public class SearchValueObjectTests
{
    #region SearchOptions

    [Fact]
    public void SearchOptions_Default_AllFlagsFalse()
    {
        var options = SearchOptions.Default;

        Assert.False(options.MatchCase);
        Assert.False(options.WholeWord);
        Assert.False(options.UseRegex);
    }

    [Fact]
    public void SearchOptions_CaseSensitive_OnlyMatchCaseTrue()
    {
        var options = SearchOptions.CaseSensitive;

        Assert.True(options.MatchCase);
        Assert.False(options.WholeWord);
        Assert.False(options.UseRegex);
    }

    [Fact]
    public void SearchOptions_WholeWordOnly_OnlyWholeWordTrue()
    {
        var options = SearchOptions.WholeWordOnly;

        Assert.False(options.MatchCase);
        Assert.True(options.WholeWord);
        Assert.False(options.UseRegex);
    }

    [Fact]
    public void SearchOptions_CustomInit_AllFlagsSet()
    {
        var options = new SearchOptions
        {
            MatchCase = true,
            WholeWord = true,
            UseRegex = true
        };

        Assert.True(options.MatchCase);
        Assert.True(options.WholeWord);
        Assert.True(options.UseRegex);
    }

    #endregion

    #region SearchQuery

    [Fact]
    public void Query_Default_HasEmptyPatternAndDefaultOptions()
    {
        // 注意：SearchOptions 未重写 Equals，需逐属性断言
        var query = new SearchQuery();

        Assert.Equal(string.Empty, query.Pattern);
        Assert.False(query.Options.MatchCase);
        Assert.False(query.Options.WholeWord);
        Assert.False(query.Options.UseRegex);
    }

    [Fact]
    public void Query_Create_SetsPatternAndOptions()
    {
        var query = SearchQuery.Create("hello", SearchOptions.CaseSensitive);

        Assert.Equal("hello", query.Pattern);
        Assert.True(query.Options.MatchCase);
    }

    [Fact]
    public void Query_Create_NullOptions_UsesDefault()
    {
        var query = SearchQuery.Create("test", null);

        Assert.Equal("test", query.Pattern);
        // null options 回退为默认值（新实例，非同一引用）
        Assert.False(query.Options.MatchCase);
        Assert.False(query.Options.WholeWord);
        Assert.False(query.Options.UseRegex);
    }

    [Fact]
    public void Query_Init_SetsAllProperties()
    {
        var options = new SearchOptions { UseRegex = true };
        var query = new SearchQuery
        {
            Pattern = @"\d+",
            Options = options
        };

        Assert.Equal(@"\d+", query.Pattern);
        Assert.True(query.Options.UseRegex);
    }

    #endregion

    #region SearchMatch - Construction & Properties

    [Fact]
    public void Match_Default_HasZeroValuesAndEmptyStrings()
    {
        var match = new SearchMatch();

        Assert.Equal(0, match.Line);
        Assert.Equal(0, match.Column);
        Assert.Equal(0, match.Length);
        Assert.Equal(string.Empty, match.MatchedText);
        Assert.Equal(string.Empty, match.LineText);
    }

    [Fact]
    public void Match_Init_SetsAllProperties()
    {
        var match = new SearchMatch
        {
            Line = 5,
            Column = 10,
            Length = 7,
            MatchedText = "pattern",
            LineText = "some line with pattern inside"
        };

        Assert.Equal(5, match.Line);
        Assert.Equal(10, match.Column);
        Assert.Equal(7, match.Length);
        Assert.Equal("pattern", match.MatchedText);
        Assert.Equal("some line with pattern inside", match.LineText);
    }

    [Fact]
    public void Match_NegativeLine_AcceptedAsIs()
    {
        // 值对象不验证范围，调用方负责
        var match = new SearchMatch { Line = -1 };
        Assert.Equal(-1, match.Line);
    }

    #endregion

    #region SearchMatch - Equals / GetHashCode

    [Fact]
    public void Match_Equals_SameValues_ReturnsTrue()
    {
        var a = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };
        var b = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Match_Equals_DifferentLine_ReturnsFalse()
    {
        var a = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };
        var b = new SearchMatch { Line = 9, Column = 2, Length = 3, MatchedText = "abc" };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Match_Equals_DifferentColumn_ReturnsFalse()
    {
        var a = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };
        var b = new SearchMatch { Line = 1, Column = 9, Length = 3, MatchedText = "abc" };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Match_Equals_DifferentLength_ReturnsFalse()
    {
        var a = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };
        var b = new SearchMatch { Line = 1, Column = 2, Length = 9, MatchedText = "abc" };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Match_Equals_DifferentMatchedText_ReturnsFalse()
    {
        var a = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };
        var b = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "xyz" };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Match_Equals_IgnoresLineText()
    {
        // LineText 不参与相等性比较（按当前实现）
        var a = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc", LineText = "line A" };
        var b = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc", LineText = "line B" };

        Assert.Equal(a, b); // 相等因为核心字段相同
    }

    [Fact]
    public void Match_Equals_Null_ReturnsFalse()
    {
        var match = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };

        Assert.False(match.Equals(null));
        Assert.False(match.Equals((object?)null));
    }

    [Fact]
    public void Match_Equals_SameInstance_ReturnsTrue()
    {
        var match = new SearchMatch { Line = 1, Column = 2, Length = 3, MatchedText = "abc" };

        Assert.True(match.Equals(match));
    }

    [Fact]
    public void Match_GetHashCode_ConsistentAcrossCalls()
    {
        var match = new SearchMatch { Line = 42, Column = 7, Length = 5, MatchedText = "test" };

        int hash1 = match.GetHashCode();
        int hash2 = match.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    #endregion

    #region ReplaceOptions

    [Fact]
    public void ReplaceOptions_Default_EmptyReplacementText()
    {
        var options = new ReplaceOptions();

        Assert.Equal(string.Empty, options.ReplacementText);
    }

    [Fact]
    public void ReplaceOptions_Init_SetsReplacementText()
    {
        var options = new ReplaceOptions { ReplacementText = "new value" };

        Assert.Equal("new value", options.ReplacementText);
    }

    #endregion

    #region ReplaceResult

    [Fact]
    public void ReplaceResult_Default_ZeroReplacedCount()
    {
        var result = new ReplaceResult();

        Assert.Equal(0, result.ReplacedCount);
    }

    [Fact]
    public void ReplaceResult_Init_SetsCount()
    {
        var result = new ReplaceResult { ReplacedCount = 5 };

        Assert.Equal(5, result.ReplacedCount);
    }

    [Fact]
    public void ReplaceResult_NegativeCount_AcceptedAsIs()
    {
        // 值对象不验证范围，但正常使用不应出现负数
        var result = new ReplaceResult { ReplacedCount = -1 };
        Assert.Equal(-1, result.ReplacedCount);
    }

    #endregion
}
