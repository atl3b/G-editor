using GEditor.Core.Selection;
using Xunit;

namespace GEditor.Tests.Selection;

public class ColumnSelectionTests
{
    [Fact]
    public void Empty_ShouldBeEmpty()
    {
        var selection = ColumnSelection.Empty;
        Assert.True(selection.IsEmpty);
    }

    [Fact]
    public void IsNormalized_WhenStartBeforeEnd_ShouldBeTrue()
    {
        var selection = new ColumnSelection(0, 0, 5, 10);
        Assert.True(selection.IsNormalized);
    }

    [Fact]
    public void IsNormalized_WhenStartAfterEnd_ShouldBeFalse()
    {
        var selection = new ColumnSelection(5, 10, 0, 0);
        Assert.False(selection.IsNormalized);
    }

    [Fact]
    public void Normalized_ShouldReorderCoordinates()
    {
        var selection = new ColumnSelection(5, 10, 0, 0).Normalized();
        Assert.Equal(0, selection.StartLine);
        Assert.Equal(0, selection.StartColumn);
        Assert.Equal(5, selection.EndLine);
        Assert.Equal(10, selection.EndColumn);
    }

    [Fact]
    public void GetLineRanges_ShouldTruncateToLineLengths()
    {
        var lines = new List<string>
        {
            "abcdef",
            "ab",
            "abcdefgh",
            ""
        };

        var selection = new ColumnSelection(0, 2, 3, 6).Normalized();
        var ranges = selection.GetLineRanges(lines);

        Assert.Equal(4, ranges.Count);

        // Line 0: 2-6 (全部在范围内)
        Assert.Equal(0, ranges[0].Line);
        Assert.Equal(2, ranges[0].StartColumn);
        Assert.Equal(6, ranges[0].EndColumn);

        // Line 1: 2-2 (只有2个字符，截断到末尾)
        Assert.Equal(1, ranges[1].Line);
        Assert.Equal(2, ranges[1].StartColumn);
        Assert.Equal(2, ranges[1].EndColumn);

        // Line 2: 2-6
        Assert.Equal(2, ranges[2].Line);
        Assert.Equal(2, ranges[2].StartColumn);
        Assert.Equal(6, ranges[2].EndColumn);

        // Line 3: 空行
        Assert.Equal(3, ranges[3].Line);
        Assert.Equal(0, ranges[3].StartColumn);
        Assert.Equal(0, ranges[3].EndColumn);
    }

    [Fact]
    public void LineCount_ShouldReturnCorrectCount()
    {
        var selection = new ColumnSelection(2, 0, 5, 10);
        Assert.Equal(4, selection.LineCount);
    }

    [Fact]
    public void Offset_ShouldShiftCoordinates()
    {
        var selection = new ColumnSelection(1, 2, 3, 4).Offset(1, 2);
        Assert.Equal(2, selection.StartLine);
        Assert.Equal(4, selection.StartColumn);
        Assert.Equal(4, selection.EndLine);
        Assert.Equal(6, selection.EndColumn);
    }

    [Fact]
    public void Normalized_SameLine_ShouldHandleReversedColumns()
    {
        var selection = new ColumnSelection(3, 10, 3, 2).Normalized();
        Assert.Equal(3, selection.StartLine);
        Assert.Equal(2, selection.StartColumn);
        Assert.Equal(3, selection.EndLine);
        Assert.Equal(10, selection.EndColumn);
    }

    [Fact]
    public void GetLineRanges_WhenAllLinesShorterThanStartCol_ShouldReturnEmptyRanges()
    {
        var lines = new List<string> { "ab", "a", "abc" };
        var selection = new ColumnSelection(0, 5, 2, 8).Normalized();
        var ranges = selection.GetLineRanges(lines);

        Assert.Equal(3, ranges.Count);
        Assert.All(ranges, r => Assert.True(r.IsEmpty));
    }

    [Fact]
    public void GetLineRanges_SingleLine_ShouldReturnSingleRange()
    {
        var lines = new List<string> { "hello world" };
        var selection = new ColumnSelection(0, 0, 0, 5).Normalized();
        var ranges = selection.GetLineRanges(lines);

        Assert.Single(ranges);
        Assert.Equal(0, ranges[0].StartColumn);
        Assert.Equal(5, ranges[0].EndColumn);
        Assert.Equal(5, ranges[0].Length);
    }

    [Fact]
    public void GetLineRanges_OutOfBoundLines_ShouldBeSkipped()
    {
        var lines = new List<string> { "line0" };
        // 选区跨越行 0-3，但只有行 0 存在
        var selection = new ColumnSelection(0, 0, 3, 5).Normalized();
        var ranges = selection.GetLineRanges(lines);

        Assert.Single(ranges);
        Assert.Equal(0, ranges[0].Line);
    }
}
