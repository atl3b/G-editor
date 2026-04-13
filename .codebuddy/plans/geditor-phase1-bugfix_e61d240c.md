---
name: geditor-phase1-bugfix
overview: 修复 G-editor Phase 1 骨架代码中的 Bug：TextFileService.Save 缺少 MarkAsSaved 调用、InsertTextCommand/ReplaceTextCommand 的 Undo 在 CRLF 场景下计算长度错误、UndoRedoManager 存在危险的 null! 死代码路径。
todos:
  - id: fix-textfileservice
    content: 修复 TextFileService.Save() 添加缺失的 MarkAsSaved() 调用
    status: completed
  - id: fix-crlf-buffer-length
    content: 在 EditorBuffer 新增 GetBufferLength 静态方法，并修复 InsertTextCommand/ReplaceTextCommand/DeleteTextCommand 的 CRLF 长度计算
    status: completed
    dependencies:
      - fix-textfileservice
  - id: remove-dead-code
    content: 移除 UndoRedoManager 中无调用方的 Execute(IEditCommand) 死代码重载
    status: completed
  - id: add-crlf-tests
    content: 新增 CRLF 场景的命令 Undo/Redo 往返测试及 GetBufferLength 单元测试
    status: completed
    dependencies:
      - fix-crlf-buffer-length
  - id: run-tests
    content: 运行全量测试验证所有修复正确性
    status: completed
    dependencies:
      - add-crlf-tests
      - fix-textfileservice
      - remove-dead-code
---

## 用户需求

对 G-editor 项目当前代码中的 4 个已诊断问题进行修复，并补充相应的测试覆盖。

## 核心问题

1. **Bug 1 (Critical)**: `TextFileService.Save()` 写入文件后缺少 `document.MarkAsSaved()` 调用，导致 `Save_MarksDocumentAsSaved` 测试失败
2. **Bug 2 (Important)**: `InsertTextCommand.Undo` 使用 `_text.Length` 调用 `buffer.Delete`，当文本包含 `\r\n` 时长度多算（每个 `\r\n` 多算 1 字符），导致 Undo 多删字符
3. **Bug 3 (Important)**: `ReplaceTextCommand` 的 `Execute` 和 `Undo` 方法中，`_oldText.Length` / `_newText.Length` 同样受 CRLF 影响
4. **Bug 4 (Minor)**: `UndoRedoManager.Execute(IEditCommand)` 死代码重载，调用 `command.Execute(null!)` 存在安全隐患，无任何调用方

## 修复范围

- 修复 4 个 Bug，涉及 5 个源文件和 1-2 个测试文件
- 新增 `EditorBuffer.GetBufferLength()` 静态工具方法解决 CRLF 长度计算问题
- 同步修复 `DeleteTextCommand.Execute` 中的同类 CRLF 问题
- 补充 CRLF 场景的 Undo/Redo 测试用例

## 技术栈

- C# 12 / .NET 8
- 测试框架: xUnit
- 项目架构: GEditor.Core（核心逻辑层）+ GEditor.Tests（测试层）

## 实现方案

### Bug 1 修复: TextFileService.Save() 缺少 MarkAsSaved()

在 `Save()` 方法末尾（第 60 行之后）添加 `document.MarkAsSaved()` 调用，与 `SaveAs()` 第 100 行保持一致。这是单行修复。

### Bug 2/3 修复: CRLF 长度计算统一方案

**根因分析**: `EditorBuffer.Delete` 内部以 `remaining -= (available + 1)` 跨行推进，将每个行分隔符算作 1 个单位。但 `_text.Length` 中 `\r\n` 占 2 个字符。每个 `\r\n` 序列在字符串长度和缓冲区长度之间产生 1 的偏差。

**统一方案**: 在 `EditorBuffer` 中新增静态工具方法 `GetBufferLength(string text)`，计算文本在缓冲区中的等效长度:

```
bufferLength = text.Length - count("\r\n" occurrences in text)
```

此方法为 O(n) 简单遍历，性能影响可忽略。

**修复点**:

- `InsertTextCommand.Undo`: `_text.Length` -> `EditorBuffer.GetBufferLength(_text)`
- `ReplaceTextCommand.Execute`: `_oldText.Length` -> `EditorBuffer.GetBufferLength(_oldText)`
- `ReplaceTextCommand.Undo`: `_newText.Length` -> `EditorBuffer.GetBufferLength(_newText)`
- `DeleteTextCommand.Execute`: `_deletedText.Length` -> `EditorBuffer.GetBufferLength(_deletedText)` (同类问题，一并修复)

### Bug 4 修复: 移除死代码

直接删除 `UndoRedoManager.Execute(IEditCommand command)` 重载（第 14-19 行）。经全项目搜索确认无任何调用方，删除安全。

### 测试补充

- 为 `EditorBuffer.GetBufferLength` 添加单元测试（纯文本、含 LF、含 CRLF、含 CR 混合场景）
- 为 `InsertTextCommand` 添加 CRLF 场景的 Execute+Undo 往返测试
- 为 `ReplaceTextCommand` 添加 CRLF 场景的 Execute+Undo 往返测试
- 为 `DeleteTextCommand` 添加 CRLF 场景的 Execute+Undo 往返测试

## 实现注意

- `GetBufferLength` 作为 `EditorBuffer` 的 `public static` 方法，与 buffer 内部表示逻辑内聚
- 修复仅涉及长度计算，不改变任何方法的公开签名或行为语义
- `Save_MarksDocumentAsSaved` 测试修复后将自动通过，无需修改测试代码
- 所有修改保持向后兼容

## 目录结构

```
src/GEditor.Core/
├── Buffer/
│   └── EditorBuffer.cs               # [MODIFY] 新增 GetBufferLength 静态方法
├── Editing/
│   ├── InsertTextCommand.cs          # [MODIFY] Undo 使用 GetBufferLength 替代 _text.Length
│   ├── ReplaceTextCommand.cs         # [MODIFY] Execute/Undo 使用 GetBufferLength 替换长度
│   ├── DeleteTextCommand.cs          # [MODIFY] Execute 使用 GetBufferLength 替换长度
│   └── UndoRedoManager.cs            # [MODIFY] 删除死代码 Execute(IEditCommand) 重载
├── IO/
│   └── TextFileService.cs            # [MODIFY] Save() 末尾添加 MarkAsSaved()
tests/GEditor.Tests/
├── Editing/
│   ├── CrlfCommandTests.cs           # [NEW] CRLF 场景命令往返测试
└── Buffer/
    └── EditorBufferTests.cs          # [NEW/MODIFY] GetBufferLength 测试（检查是否已有此文件）
```

## Agent Extensions

- **code-explorer**
- Purpose: 验证 `DeleteTextCommand` 以外是否还有其他命令类使用 `.Length` 进行缓冲区长度计算
- Expected outcome: 确认所有受影响的命令类均已纳入修复范围