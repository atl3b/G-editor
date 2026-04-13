namespace GEditor.Core.Editing;

/// <summary>编辑命令接口 — 命令模式</summary>
public interface IEditCommand
{
    /// <summary>在指定缓冲区上执行命令</summary>
    void Execute(Buffer.EditorBuffer buffer);

    /// <summary>撤销命令</summary>
    void Undo(Buffer.EditorBuffer buffer);

    /// <summary>命令描述（用于 UI 显示）</summary>
    string Description { get; }
}
