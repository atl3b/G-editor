# Contributing to G-editor

感谢你对 G-editor 的关注。本文档描述了参与本项目贡献的基本流程与规范，请在提交贡献前仔细阅读。

---

## 📖 目录

- [欢迎贡献](#欢迎贡献)
- [Issue / PR 基本流程](#issue--pr-基本流程)
- [代码来源声明要求](#代码来源声明要求)
- [许可证兼容性要求](#许可证兼容性要求)
- [关于 Notepad++ 的参考边界](#关于-notepad-的参考边界)
- [开发环境搭建](#开发环境搭建)
- [项目架构概览](#项目架构概览)
- [编码规范](#编码规范)
- [测试规范](#测试规范)
- [提交规范建议](#提交规范建议)
- [PR 描述应包含的内容](#pr-描述应包含的内容)
- [许可证不确定时的处理](#许可证不确定时的处理)
- [常见任务指引](#常见任务指引)
- [联系方式](#联系方式)

---

## 欢迎贡献

G-editor 是一个从零构建的轻量级桌面文本编辑器，接受所有形式的贡献，包括但不限于：

- 🐛 **Bug 修复**
- ✨ **新功能开发**
- 📚 **文档完善**
- ✅ **代码审查与测试补充**
- 🏗️ **架构优化建议**

提交贡献即表示你同意遵守本文档中的所有规范。

---

## Issue / PR 基本流程

1. **提交 Issue** — 在着手开发前，先创建 Issue 描述问题或功能需求，等待维护者确认，避免重复劳动。
2. **认领任务** — 在 Issue 中留言表明意向，维护者确认后可开始工作。
3. **创建分支** — 从 `main` 分支创建特性分支，命名建议：`feature/<简要描述>` 或 `fix/<issue编号>-<简要描述>`。
4. **开发与测试** — 编写代码并通过所有现有测试（`dotnet test`），新增功能应附带对应测试用例。
5. **提交 PR** — 填写完整的 PR 描述（见下方要求），等待审查。
6. **修改与合入** — 根据审查意见迭代修改，最终由维护者合入主干。

---

## 代码来源声明要求

G-editor 是**从零开发**的项目。提交 PR 时，必须在 PR 描述中明确说明：

- 代码是否为**完全原创**。
- 若参考了外部项目、文章、库的思路，需注明**具体来源**。
- 若直接引用或改写自第三方代码，需注明**原始许可协议**，且该协议必须在本项目的可接受范围内（见下方许可证兼容性要求）。

---

## 许可证兼容性要求

G-editor 采用 **MIT License** 发布，所有合入主干的代码统一以 MIT 许可发布。

可接受的代码来源许可：

| 许可证 | 是否可合入 |
|---|---|
| MIT | 可以 |
| BSD-2-Clause | 可以 |
| BSD-3-Clause | 可以 |
| Apache-2.0 | 可以 |

**明确拒绝的许可类型：**

| 许可证 | 是否可合入 |
|---|---|
| GPL (任何版本) | 不可以 |
| LGPL (任何版本) | 不可以 |
| AGPL (任何版本) | 不可以 |
| 其他与 MIT 不兼容的许可 | 不可以 |

如有不确定的许可类型，**先提 Issue 讨论，不要直接提交 PR**。

---

## 关于 Notepad++ 的参考边界

G-editor 的设计灵感来源于 Notepad++，但我们**不是其 fork，也不复用其源码**。

阅读 Notepad++ 源码以理解其架构与设计思路是被鼓励的，但必须遵守以下边界：

### 允许

- 学习 Notepad++ 的**设计思想、架构模式、交互理念**
- 借鉴其**功能规划思路**（如多标签页、语法高亮、正则搜索等功能特性）
- 参考其**算法思路**后以完全独立的方式重新实现

### 禁止

- 直接拷贝 Notepad++ 的源码或代码片段到本项目
- 将 Notepad++ 源码**逐行翻译**为 C# 后提交
- 对 Notepad++ 的实现进行**高度近似重写**（即仅做语言翻译或变量改名）
- 引入任何违反其 GPL 许可证的衍生方式

### 不清晰时

如果你不确定某段代码是否越过了参考与复制的边界，**先开 Issue 与维护者沟通**，确认后再提交。这比事后回退更高效。

---

## 开发环境搭建

### 环境要求

| 项目 | 要求 / 推荐 |
|---|---|
| SDK | .NET 8 SDK ([下载](https://dotnet.microsoft.com/download/dotnet/8.0)) |
| 语言 | C# 12 |
| IDE | Visual Studio 2022 / JetBrains Rider / VS Code + C# Dev Kit |
| 操作系统 | Windows 10+ (WPF 依赖) |
| 测试框架 | xUnit |
| Mock 框架 | Moq |
| Git | 最新版本 |

### 快速启动

```bash
# 克隆仓库
git clone https://github.com/atl3b/G-editor.git
cd G-editor

# 还原 NuGet 依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行全部测试
dotnet test

# 运行应用程序
dotnet run --project src/GEditor.App/GEditor.App.csproj
```

### 推荐的 VS Code 扩展

- C# Dev Kit — IntelliSense、调试、测试运行器
- .NET Install Tool — 确保 .NET SDK 已安装
- EditorConfig — 保持代码风格一致

### IDE 配置建议

项目根目录包含 `.editorconfig` 文件，定义了统一的编码风格。请确保你的 IDE 启用 EditorConfig 支持：

- **VS Code**: 安装 `EditorConfig` 扩展后自动生效
- **Visual Studio 2022**: 内置支持，开箱即用
- **Rider**: 内置支持，开箱即用

---

## 项目架构概览

在贡献代码前，请理解项目的分层架构：

```
┌─────────────────────────────────────────┐
│            GEditor.App (WPF)             │  UI 层
│    引用: Core + Syntax                   │
├─────────────────────────────────────────┤
│            GEditor.Core                  │  核心层（无 WPF 依赖）
│    不引用: WPF, Syntax                    │
├─────────────────────────────────────────┤
│          GEditor.Syntax                  │  语法层（纯计算）
│    不引用: WPF, Core                      │
└─────────────────────────────────────────┘
         ↕ 测试                          ↕
┌─────────────────────────────────────────┐
│           GEditor.Tests                 │  单元测试
└─────────────────────────────────────────┘
```

**各层职责与约束：**

| 层 | 职责 | 可引用 | 禁止引用 |
|---|---|---|---|
| **Core** | 文档模型、编辑命令、文件 IO、搜索、选择模型 | System.*, Microsoft.* (非 UI) | WPF, Syntax, App |
| **Syntax** | 语法高亮引擎，输出纯数据 | System* (仅基础库) | WPF, Core, App |
| **App** | WPF 视图、MVVM ViewModel、DI 组装 | Core, Syntax | （顶层） |
| **Tests** | 单元测试 | Core, Syntax, xUnit, Moq | App (除非测试 VM) |

**关键原则：**
- Core 层的所有对外交互通过**接口和事件**，不依赖任何 UI 框架
- Syntax 层输出纯数据 (`SyntaxToken` 列表)，UI 层负责着色映射
- 添加新功能时，优先将逻辑放在 Core 或 Syntax 层，App 层只做组装和绑定

### Core 层模块说明

| 模块 | 路径 | 说明 |
|---|---|---|
| Buffer | `Core/Buffer/` | 行级文本缓冲区 (`EditorBuffer`) |
| Documents | `Core/Documents/` | 文档聚合根、编码/换行符元数据 |
| Editing | `Core/Editing/` | 编辑命令接口及实现（命令模式） |
| Search | `Core/Search/` | 搜索服务、搜索选项与结果模型 |
| IO | `Core/IO/` | 文件读写、编码检测、换行符检测 |
| Management | `Core/Management/` | 多文档管理器 |
| Selection | `Core/Selection/` | 选择模式（普通/列选择） |

### Syntax 层扩展方式

添加新语言高亮只需两步：
1. 创建实现 `ILanguageDefinition` 的类，定义语言的 `HighlightRule` 列表
2. 通过 `SyntaxHighlighterRegistry.Register()` 注册

无需修改已有代码。

---

## 编码规范

### C# 风格

项目使用 `.editorconfig` 统一管理编码风格，主要规则如下：

```ini
# 缩进：4 个空格（禁止 Tab）
indent_style = space
indent_size = 4

# 文件结尾保留空行
insert_final_newline = true

# 字符集
charset = utf-8
```

### 命名约定

| 类型 | 约定 | 示例 |
|---|---|---|
| 公共类/接口 | PascalCase | `EditorBuffer`, `IEditCommand` |
| 公共方法 | PascalCase | `InsertText()`, `GetLine()` |
| 公共属性 | PascalCase | `LineCount`, `FilePath` |
| 私有字段 | _camelCase | `_lines`, `_commands` |
| 参数/局部变量 | camelCase | `lineNumber`, `text` |
| 常量 | PascalCase | `MaxUndoSteps`, `DefaultEncoding` |
| 接口 | I 前缀 + PascalCase | `ISearchService`, `ITextFileService` |
| 事件 | PascalCase | `ContentChanged`, `TextChanged` |

### 代码组织原则

1. **单一职责** — 每个类只做一件事
2. **依赖倒置** — 模块间通过接口通信，不直接依赖具体实现
3. **最小可见性** — 字段和方法默认为 `private`，只在需要时扩大访问级别
4. **无魔法数字** — 使用具名常量或 `enum`
5. **XML 文档注释** — 公共 API 应包含 `<summary>` 注释

### 示例：添加新的编辑命令

```csharp
/// <summary>
/// 自定义文本转换命令示例
/// </summary>
public class TransformTextCommand : IEditCommand
{
    private readonly string _originalText;
    private readonly string _transformedText;
    
    public TransformTextCommand(string original, Func<string, string> transform)
    {
        _originalText = original;
        _transformedText = transform(original);
    }
    
    public void Execute()
    {
        // 应用转换...
    }
    
    public void Undo()
    {
        // 恢复原始内容...
    }
}
```

---

## 测试规范

### 测试框架配置

- **测试框架**: xUnit
- **Mock 库**: Moq
- **断言风格**: Fluent Assertions 可选（如引入）

### 测试文件组织

测试文件按被测模块组织，结构与源码镜像对应：

```
tests/
└── GEditor.Tests/
    ├── Buffer/          # EditorBuffer 相关测试
    ├── Documents/       # Document 模型测试
    ├── Editing/         # 编辑命令测试
    ├── Search/          # 搜索服务测试
    ├── IO/              # 文件 IO 和编码检测测试
    ├── Selection/       # 选择模型测试
    └── Syntax/          # 语法高亮测试
```

### 测试命名约定

采用 **UnitOfWork_Scenario_ExpectedBehavior** 模式：

```csharp
public class EditorBufferTests
{
    [Fact]
    public void InsertText_AtPosition_TextInsertedCorrectly() { }
    
    [Fact]
    public void DeleteText_WithValidRange_TextRemovedAndReturned() { }
    
    [Theory]
    [InlineData("hello", "world", "helloworld")]
    [InlineData("", "test", "test")]
    public void InsertText_AppendToEnd_ConcatenatesStrings(
        string original, string toInsert, string expected) { }
}
```

### 测试覆盖率目标

| 层 | 目标覆盖率 |
|---|---|
| Core - Buffer | >= 90% |
| Core - Editing | >= 90% |
| Core - Search | >= 85% |
| Core - IO | >= 85% |
| Core - Documents | >= 80% |
| Core - Selection | >= 80% |
| Syntax | >= 80% |

### 编写测试的原则

1. **独立性** — 测试之间不应有执行顺序依赖
2. **可读性** — 测试名应清楚表达测试意图，减少注释需求
3. **隔离性** — 使用真实对象优先于 Mock，仅在必要时 Mock 外部依赖
4. **覆盖边界** — 特别注意空输入、边界值、异常路径

---

## 提交规范建议

提交信息采用 **Conventional Commits** 格式：

```
<type>(<scope>): <简短描述>

<可选的详细说明>
```

**type** 取值：

| 类型 | 用途 |
|---|---|
| `feat` | 新功能 |
| `fix` | Bug 修复 |
| `refactor` | 重构（不改变行为） |
| `test` | 测试相关 |
| `docs` | 文档更新 |
| `chore` | 构建、工具、配置等杂项 |
| `perf` | 性能优化 |

**scope** 取值（常用）：

| scope | 对应模块 |
|---|---|
| `buffer` | `GEditor.Core.Buffer` |
| `document` | `GEditor.Core.Documents` |
| `editing` | `GEditor.Core.Editing` |
| `search` | `GEditor.Core.Search` |
| `io` | `GEditor.Core.IO` |
| `selection` | `GEditor.Core.Selection` |
| `syntax` | `GEditor.Syntax` |
| `app` | `GEditor.App` |
| `vm` | `GEditor.App.ViewModels` |
| `ui` | `GEditor.App.Controls` / Views |

**示例：**

```
feat(search): add whole-word and case-sensitive search support

Implemented SearchOptions.WholeWord and SearchOptions.CaseSensitive properties.
Updated SearchService to apply these filters during matching.
Added corresponding unit tests.
```

```
fix(io): correct BOM detection for UTF-8 files

The previous implementation failed to detect UTF-8 BOM when file started with
null bytes. Fixed by checking BOM bytes before content heuristics.
```

```
test(buffer): add unit tests for EditorBuffer insert and delete operations

Covers normal cases, boundary conditions (empty buffer, single line),
and edge cases (insert at position 0, delete entire content).
```

---

## PR 描述应包含的内容

提交 Pull Request 时，PR 描述应包含以下信息：

1. **变更摘要** — 这个 PR 做了什么，一句话概括。
2. **关联 Issue** — `Fixes #<编号>` 或 `Closes #<编号>`，无关联 Issue 时说明原因。
3. **代码来源声明** — 明确标注代码是否原创，如非原创须注明来源及许可。
4. **变更详情** — 主要修改了哪些模块，为什么这样改。
5. **测试说明** — 新增/修改了哪些测试，如何验证。
6. **自检清单**：

   - [ ] 本地通过全部测试（`dotnet test`）
   - [ ] 无新增编译警告（`dotnet build -warnaserror`）
   - [ ] 提交信息符合 Conventional Commits 规范
   - [ ] 已确认代码来源符合许可证要求
   - [ ] 若参考了 Notepad++，确认未越界（见参考边界说明）
   - [ ] 新增公共 API 包含 XML 文档注释

---

## 许可证不确定时的处理

如果你在贡献过程中遇到以下情况：

- 不确定某段第三方代码的许可证是否兼容 MIT
- 不确定自己的实现与参考项目（包括 Notepad++）之间的边界
- 发现项目中可能存在许可证不兼容的代码

**请先提 Issue 讨论说明情况，等待维护者确认后再提交 PR。**

在问题确认前，不要将有争议的代码直接合入主分支。维护者会在 Issue 中提供明确指引。

---

## 常见任务指引

### 添加新的语法高亮语言

1. 在 `src/GEditor.Syntax/Languages/` 下新建 `XxxSyntaxHighlighter.cs`
2. 实现 `ILanguageDefinition` 定义高亮规则
3. 继承 `RegexBasedHighlighter`（如果适用）
4. 在 `App.xaml.cs` 的 DI 注册中注册新高亮器
5. 在 `tests/GEditor.Tests/Syntax/` 下添加对应测试
6. 更新 README.md 功能表

### 添加新的编辑命令

1. 在 `src/GEditor.Core/Editing/` 下新建 `XxxCommand.cs`
2. 实现 `IEditCommand` 接口（Execute / Undo）
3. 在 `tests/GEditor.Tests/Editing/` 下添加命令测试
4. 如需在 UndoRedoManager 中使用，确保命令正确入栈/出栈行为

### 添加新功能到状态栏

1. 在 `StatusBarViewModel.cs` 中添加新属性
2. 更新 `MainWindow.xaml` 中的状态栏绑定
3. 确保属性变更时触发 `PropertyChanged` 事件

### 修复 Core 层 Bug

1. 先编写复现 Bug 的测试用例（应先失败 ✗）
2. 修复代码使测试通过 ✓
3. 确认未破坏其他测试（`dotnet test` 全绿）

---

## 联系方式

如有疑问，可通过 GitHub Issue 与维护者沟通。

---

再次感谢你的贡献。每一份 PR 都在让 G-editor 变得更好。🎉
