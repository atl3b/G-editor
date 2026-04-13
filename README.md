# G-editor

一款从零构建的轻量级桌面文本编辑器，使用 C# / .NET 8 / WPF 技术栈开发。

项目受 **Notepad++** 启发，但不是其 fork，也不复用其源码。允许阅读 Notepad++ 源码理解设计思想，但禁止复制或近似改写其实现。

所有代码均为原创或来自与 MIT 兼容的许可证。

## 项目目标

- **轻量可扩展**：模块化架构，核心逻辑与 UI 层严格分离，便于测试和迭代
- **渐进式开发**：以 MVP 为起点，逐步扩展功能
- **开发者友好**：清晰的分层设计，核心层不依赖 UI 框架，可独立测试
- **正确处理文本**：对编码检测、换行符保留、撤销/重做原子性等基础能力做到可靠

## MVP 功能范围

| 功能 | 说明 |
|------|------|
| 文件操作 | 新建、打开、保存、另存为 |
| 多标签页 | 多文档管理、标签切换 |
| 编辑能力 | 撤销/重做（命令模式 + UndoRedoManager）、行级文本缓冲区 |
| 查找与替换 | 大小写敏感、全字匹配、正则表达式、Replace All（原子复合命令，一次 Undo 恢复全部） |
| 编码处理 | BOM 优先检测 → 启发式 → 回退默认；支持 UTF-8/UTF-16/GB2312/GBK |
| 换行符 | 打开时自动检测（CRLF / LF / CR），保存时保留原风格，可手动切换 |
| 语法高亮 | C#、JSON、XML、Plain Text（规则驱动 + 注册中心，可扩展） |
| 状态栏 | 光标位置、编码、换行符类型、语言模式 |

## 快速开始

### 系统要求

- Windows 10 或更高版本
- .NET 8 SDK

### 构建项目

```bash
# 克隆仓库
git clone https://github.com/atl3b/G-editor.git
cd G-editor

# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行测试
dotnet test

# 运行应用程序
dotnet run --project src/GEditor.App/GEditor.App.csproj
```

### 键盘快捷键

| 功能 | 快捷键 |
|------|--------|
| 新建 | Ctrl+N |
| 打开 | Ctrl+O |
| 保存 | Ctrl+S |
| 另存为 | Ctrl+Shift+S |
| 关闭标签 | Ctrl+W |
| 撤销 | Ctrl+Z |
| 重做 | Ctrl+Y |
| 查找 | Ctrl+F |
| 替换 | Ctrl+H |
| 查找下一个 | F3 |
| 查找上一个 | Shift+F3 |

## 技术栈

| 层级 | 技术 |
|------|------|
| 语言 | C# 12 / .NET 8 |
| UI 框架 | WPF |
| 架构模式 | MVVM + 分层架构 |
| 依赖注入 | Microsoft.Extensions.DependencyInjection |
| 单元测试 | xUnit |
| 构建工具 | .NET CLI |

## 项目结构

```
G-editor/
├── src/
│   ├── GEditor.Core/        # 核心业务逻辑（文档模型、编辑命令、文件 IO、搜索替换）
│   ├── GEditor.Syntax/      # 语法高亮引擎（规则驱动，零 UI 依赖，不引用 Core）
│   └── GEditor.App/         # WPF 应用宿主（MVVM、DI、View/ViewModel）
└── tests/
    └── GEditor.Tests/       # 单元测试
```

**架构分层原则：**

- `GEditor.Core` — 不引用任何 WPF 包，所有对外交互通过接口和事件
- `GEditor.Syntax` — 不引用 WPF 包和 Core 包，输出纯数据（`SyntaxToken` 列表），UI 层负责着色映射
- `GEditor.App` — 引用 Core + Syntax，通过 MVVM 绑定驱动 UI

## 文本编码与换行策略

### 编码

- 编辑器内部统一使用 .NET 字符串的原生编码 UTF-16
- **打开文件**：BOM 优先（UTF-8 BOM、UTF-16 LE/BE）→ 启发式检测 → 回退系统默认编码
- **保存文件**：默认保持打开时的编码；可通过 UI 或"另存为"指定编码

### 换行符

- 打开文件时自动检测主导换行风格（CRLF / LF / CR）
- 编辑过程中内部统一处理，不丢失换行符信息
- 保存时默认保留原文件换行风格，可通过菜单手动切换

## 架构设计

### 核心模块

- **Document** - 文档聚合根，管理文档元数据（路径、编码、换行符）和内容缓冲区
- **EditorBuffer** - 行级文本缓冲区，负责文本存储和编辑操作
- **UndoRedoManager** - 撤销/重做管理器，支持命令模式
- **TextFileService** - 文件读写服务，处理编码检测和换行符转换
- **SearchService** - 搜索服务，支持正则表达式和 Replace All
- **SyntaxHighlighterRegistry** - 语法高亮注册中心，管理多种语言的高亮器

## 开发方式

本项目采用 **vibe coding** 方式推进，通过 CodeBuddy 辅助协作开发。

## 许可证

本项目采用 [MIT License](LICENSE) 发布。

## 代码来源与许可证兼容性

- 本项目所有代码均为原创或来自与 MIT 兼容的许可证（MIT、BSD-2-Clause、BSD-3-Clause、Apache-2.0）
- 允许参考开源项目（包括 Notepad++）的设计思想和架构理念，但严禁直接拷贝或近似改写其源码
- 所有合入主干的代码统一按 MIT 许可证发布
- 如提交第三方代码，需在 PR 中声明来源和许可证

## 贡献

请参阅 [CONTRIBUTING.md](CONTRIBUTING.md) 了解贡献流程与规范。

## 项目状态

**MVP 阶段** — 核心功能已完成，包括文件操作、编辑能力、查找替换、编码处理、语法高亮。测试覆盖率达到目标。
