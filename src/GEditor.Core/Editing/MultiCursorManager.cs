namespace GEditor.Core.Editing;

/// <summary>
/// 多光标位置
/// </summary>
public readonly record struct CursorPosition(int Line, int Column);

/// <summary>
/// 多光标管理器 - 管理多个光标位置的编辑操作
/// </summary>
public class MultiCursorManager
{
    private readonly List<CursorPosition> _cursors = new();
    private bool _isActive;

    /// <summary>
    /// 是否处于多光标模式
    /// </summary>
    public bool IsActive => _isActive && _cursors.Count > 1;

    /// <summary>
    /// 所有光标位置（只读）
    /// </summary>
    public IReadOnlyList<CursorPosition> Cursors => _cursors.AsReadOnly();

    /// <summary>
    /// 主光标（最后添加的或当前活动的）
    /// </summary>
    public CursorPosition? PrimaryCursor => _cursors.Count > 0 ? _cursors[^1] : null;

    /// <summary>
    /// 光标数量
    /// </summary>
    public int Count => _cursors.Count;

    /// <summary>
    /// 光标变化事件
    /// </summary>
    public event Action? CursorsChanged;

    /// <summary>
    /// 添加光标位置
    /// </summary>
    public void AddCursor(int line, int column)
    {
        var newPos = new CursorPosition(line, column);
        
        // 检查是否已存在相同位置
        if (_cursors.Contains(newPos))
            return;

        _cursors.Add(newPos);
        _isActive = true;
        CursorsChanged?.Invoke();
    }

    /// <summary>
    /// 设置主光标位置（清除其他光标，进入单光标模式）
    /// </summary>
    public void SetPrimaryCursor(int line, int column)
    {
        _cursors.Clear();
        _cursors.Add(new CursorPosition(line, column));
        _isActive = false;
        CursorsChanged?.Invoke();
    }

    /// <summary>
    /// 移除指定位置的光标
    /// </summary>
    public bool RemoveCursorAt(int line, int column)
    {
        var pos = new CursorPosition(line, column);
        bool removed = _cursors.Remove(pos);
        
        if (removed)
        {
            if (_cursors.Count <= 1)
                _isActive = false;
            CursorsChanged?.Invoke();
        }
        
        return removed;
    }

    /// <summary>
    /// 清除所有辅助光标，只保留最后一个作为主光标
    /// </summary>
    public void ClearSecondaryCursors()
    {
        while (_cursors.Count > 1)
        {
            _cursors.RemoveAt(0);
        }
        _isActive = false;
        CursorsChanged?.Invoke();
    }

    /// <summary>
    /// 清除所有光标
    /// </summary>
    public void ClearAll()
    {
        _cursors.Clear();
        _isActive = false;
        CursorsChanged?.Invoke();
    }

    /// <summary>
    /// 检查指定位置是否有光标
    /// </summary>
    public bool HasCursorAt(int line, int column)
    {
        return _cursors.Contains(new CursorPosition(line, column));
    }

    /// <summary>
    /// 获取所有光标的插入位置列表（用于列模式插入）
    /// </summary>
    public List<(int line, int column)> GetInsertPositions()
    {
        return _cursors.Select(c => (c.Line, c.Column)).ToList();
    }

    /// <summary>
    /// 更新所有光标的位置偏移（在插入/删除文本后调用）
    /// </summary>
    /// <param name="line">变更发生的行</param>
    /// <param name="column">变更发生的列</param>
    /// <param name="insertedLength">插入的字符数（负数表示删除）</param>
    public void UpdatePositionsAfterEdit(int line, int column, int insertedLength)
    {
        for (int i = 0; i < _cursors.Count; i++)
        {
            var cursor = _cursors[i];
            
            // 变更点之后的光标需要更新位置
            if (cursor.Line > line || (cursor.Line == line && cursor.Column >= column))
            {
                int newLine = cursor.Line;
                int newCol = cursor.Column;

                if (cursor.Line == line)
                {
                    newCol += insertedLength;
                    // 处理列溢出
                    if (newCol < 0) newCol = 0;
                }
                
                _cursors[i] = new CursorPosition(newLine, newCol);
            }
        }
        
        CursorsChanged?.Invoke();
    }
}
