namespace GEditor.Core.Editing;

/// <summary>
/// 复合编辑命令 — 将多个子命令打包为单个原子操作。
/// Execute 按顺序执行所有子命令，Undo 按逆序撤销所有子命令。
/// </summary>
public sealed class CompositeEditCommand : IEditCommand
{
    public string Description { get; }
    private readonly IReadOnlyList<IEditCommand> _commands;

    public CompositeEditCommand(string description, IEnumerable<IEditCommand> commands)
    {
        Description = description;
        _commands = commands.ToList().AsReadOnly();
    }

    public void Execute(Buffer.EditorBuffer buffer)
    {
        foreach (var cmd in _commands)
            cmd.Execute(buffer);
    }

    public void Undo(Buffer.EditorBuffer buffer)
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
            _commands[i].Undo(buffer);
    }
}
