# G-editor

一款从零构建的轻量级桌面文本编辑器，使用 **C# / .NET 8 / WPF** 技术栈开发。

项目受 **Notepad++** 启发，但不是其 fork，也不复用其源码。允许阅读 Notepad++ 源码理解设计思想，但禁止复制或近似改写其实现。

所有代码均为原创或来自与 MIT 兼容的许可证。

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## ✨ 特性

### 核心功能

| 功能 | 说明 |
|------|------|
| 📄 文件操作 | 新建、打开、保存、另存为 |
| 📑 多标签页 | 多文档管理、标签切换、标签关闭 |
| ✏️ 编辑能力 | 插入、删除、替换文本；撤销/重做（命令模式 + UndoRedoManager） |
| 🔍 查找与替换 | 大小写敏感、全字匹配、正则表达式、Replace All（原子复合命令，一次 Undo 恢复全部） |
| 🔄 列编辑模式 | 块选择（矩形选择）、列插入、列删除、列替换 |
| 📝 编码处理 | BOM 优先检测 → 启发式 → 回退默认；支持 UTF-8/UTF-16/GB2312/GBK |
| ↩️ 换行符处理 | 打开时自动检测（CRLF / LF / CR），保存时保留原风格，可手动切换 |
| 🎨 语法高亮 | C#、JSON、XML、Plain Text（规则驱动 + 注册中心，可扩展） |
| 📊 状态栏 | 光标位置、编码、换行符类型、语言模式 |
| 🔀 自动换行 | 视图菜单切换，长行自动折行显示 |
| 🔢 跳转到行号 | Ctrl+G 快捷键，对话框输入目标行号 |
| 📂 最近文件 | 文件菜单记录最近打开的 10 个文件，自动持久化 |

### 键盘快捷键

| 功能 | 快捷键 |
|------|--------|
| 新建 | `Ctrl+N` |
| 打开 | `Ctrl+O` |
| 保存 | `Ctrl+S` |
| 另存为 | `Ctrl+Shift+S` |
| 关闭标签 | `Ctrl+W` |
| 撤销 | `Ctrl+Z` |
| 重做 | `Ctrl+Y` |
| 查找 | `Ctrl+F` |
| 替换 | `Ctrl+H` |
| 查找下一个 | `F3` |
| 查找上一个 | `Shift+F3` |
| 跳转到行号 | `Ctrl+G` |
| 列模式切换 | `Alt+M` |
| 列模式插入 | `Alt+Shift+I` |
| 列模式删除 | `Alt+Shift+D` |

## 🚀 快速开始

### 系统要求

- **操作系统**: Windows 10 或更高版本
- **运行时**: .NET 8 Runtime（运行已编译程序）或 .NET 8 SDK（开发构建）

### 构建与运行

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

### 发布独立应用

```bash
# 发布为单文件独立应用（无需安装 .NET Runtime）
dotnet publish src/GEditor.App/GEditor.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

输出路径：`src/GEditor.App/bin/Release/net8.0/win-x64/publish/`

## 🏗️ 技术栈

| 层级 | 技术 | 说明 |
|------|------|------|
| 语言 | C# 12 / .NET 8 | 最新 LTS 版本 |
| UI 框架 | WPF | Windows 原生 UI，支持硬件加速渲染 |
| 架构模式 | MVVM + 分层架构 | 视图与逻辑分离 |
| 依赖注入 | Microsoft.Extensions.DependencyInjection | 轻量级 DI 容器 |
| 单元测试 | xUnit + Moq | 测试框架 + Mock 库 |
| 构建工具 | .NET CLI / MSBuild | 跨平台构建 |

## 📁 项目结构

```
G-editor/
├── src/
│   ├── GEditor.Core/              # 核心业务逻辑层（零 UI 依赖）
│   │   ├── Buffer/                #   行级文本缓冲区 (EditorBuffer)
│   │   ├── Documents/             #   文档模型与元数据 (Document, LineEnding)
│   │   ├── Editing/               #   编辑命令系统（命令模式）
│   │   │   ├── IEditCommand.cs        #     命令接口
│   │   │   ├── InsertTextCommand.cs   #     插入命令
│   │   │   ├── DeleteTextCommand.cs   #     删除命令
│   │   │   ├── ReplaceTextCommand.cs  #     替换命令
│   │   │   ├── CompositeEditCommand.cs #    复合命令（原子操作组）
│   │   │   ├── ColumnInsertCommand.cs #     列插入命令
│   │   │   ├── ColumnDeleteCommand.cs #     列删除命令
│   │   │   ├── ColumnReplaceCommand.cs #    列替换命令
│   │   │   └── UndoRedoManager.cs     #     撤销/重做管理器
│   │   ├── Search/                #   搜索与替换服务
│   │   ├── IO/                    #   文件读写、编码检测、换行符检测
│   │   ├── Management/            #   文档管理器（多文档协调）
│   │   └── Selection/             #   选择模型（含列/块选择支持）
│   │
│   ├── GEditor.Syntax/            # 语法高亮引擎（零 UI 零 Core 依赖）
│   │   ├── ISyntaxHighlighter.cs      #   高亮器接口
│   │   ├── SyntaxHighlighterRegistry.cs # 高亮器注册中心
│   │   ├── SyntaxToken.cs / TokenKind.cs # 词法标记定义
│   │   └── Languages/                 #   具体语言实现
│   │       ├── ILanguageDefinition.cs  #     语言定义接口
│   │       ├── RegexBasedHighlighter.cs #   基于正则的高亮器基类
│   │       ├── CSharpSyntaxHighlighter.cs
│   │       ├── JsonSyntaxHighlighter.cs
│   │       ├── XmlSyntaxHighlighter.cs
│   │       └── PlainTextHighlighter.cs
│   │
│   └── GEditor.App/               # WPF 应用宿主
│       ├── App.xaml / App.xaml.cs         #   应用入口与 DI 配置
│       ├── MainWindow.xaml               #   主窗口定义
│       ├── Controls/                     #   自定义控件
│       │   ├── SyntaxHighlightingTextBox.cs  #     语法高亮文本框
│       │   └── ColumnSelectionAdorner.cs      #     列选择装饰器
│       ├── ViewModels/                   #   MVVM 视图模型
│       │   ├── ViewModelBase.cs          #     VM 基类 (INotifyPropertyChanged)
│       │   ├── MainWindowViewModel.cs    #     主窗口 VM
│       │   ├── EditorViewModel.cs        #     编辑区 VM
│       │   ├── DocumentTabViewModel.cs   #     标签页 VM
│       │   ├── SearchPanelViewModel.cs   #     搜索面板 VM
│       │   ├── StatusBarViewModel.cs     #     状态栏 VM
│       │   └── RelayCommand.cs           #     命令绑定
│       ├── Converters/                   #   值转换器
│       ├── Services/                     #   应用层服务
│       └── Views/                        #   视图（如有额外窗口）
│           ├── SearchPanelView.xaml        #     查找/替换面板
│           └── GoToLineDialog.xaml         #     跳转到行号对话框
│
└── tests/
    └── GEditor.Tests/              # 单元测试
        ├── Buffer/                     #   EditorBuffer 测试
        ├── Documents/                  #   Document 模型测试
        ├── Editing/                    #   编辑命令测试
        │   ├── ColumnCommandTests.cs       #   列编辑命令测试
        │   └── ColumnEditModeTests.cs      #   列编辑模式测试
        ├── Search/                     #   搜索服务测试
        ├── IO/                         #   文件 IO 与编码检测测试
        │   ├── EncodingDetectorTests.cs
        │   ├── TextFileServiceTests.cs
        │   └── LineEndingDetectorTests.cs
        ├── Management/                  #   文档管理器测试
        │   └── DocumentManagerTests.cs
        ├── Selection/                  #   选择模型测试
        ├── EdgeCases/                  #   边界/异常测试套件
        └── Syntax/                     #   语法高亮测试
```

### 架构分层原则

```
┌─────────────────────────────────────────┐
│            GEditor.App (WPF)             │  ← UI 层：MVVM 绑定、用户交互
│    引用: Core + Syntax                   │
├─────────────────────────────────────────┤
│            GEditor.Core                  │  ← 核心层：业务逻辑、数据模型
│    不引用: WPF, Syntax                    │
├─────────────────────────────────────────┤
│          GEditor.Syntax                  │  ← 语法层：高亮引擎（纯计算）
│    不引用: WPF, Core                      │
└─────────────────────────────────────────┘
```

**关键约束：**
- **Core → 无 WPF 依赖** — 所有对外交互通过接口和事件，可独立于 UI 测试
- **Syntax → 零外部依赖** — 输出纯数据 (`SyntaxToken` 列表)，UI 层负责着色映射
- **App → 组装层** — 引用 Core + Syntax，通过 MVVM 绑定驱动 UI，配置 DI 容器

## 📐 架构设计详解

### 命令模式（编辑系统核心）

编辑系统采用 **Command Pattern** 实现，所有编辑操作都封装为 `IEditCommand` 对象：

```
用户输入 → EditCommand.Execute() → Buffer 修改 → Command 入栈 UndoRedoManager
撤销时   → Command.Undo()       → Buffer 回滚 → Command 移出栈
```

**命令类型：**
- **基础命令**: `InsertTextCommand`, `DeleteTextCommand`, `ReplaceTextCommand`
- **复合命令**: `CompositeEditCommand` — 将多个命令打包为原子操作（如 Replace All）
- **列编辑命令**: `ColumnInsertCommand`, `ColumnDeleteCommand`, `ColumnReplaceCommand` — 块区域批量操作

### 文档模型

```
Document（聚合根）
├── 元数据
│   ├── FilePath — 文件路径
│   ├── EncodingInfo — 编码信息（BOM、检测方式）
│   └── LineEndingInfo — 换行符信息（类型、检测结果）
├── 内容
│   └── EditorBuffer — 行级文本缓冲区（按行存储字符串列表）
└── 事件
    └── ContentChanged — 内容变更通知
```

### 语法高亮扩展机制

添加新语言高亮只需两步：
1. 创建 `ILanguageDefinition` 定义语言的 `HighlightRule` 列表
2. 通过 `SyntaxHighlighterRegistry.Register()` 注册

无需修改任何已有代码。

## 💡 设计理念

1. **轻量可扩展** — 模块化架构，核心逻辑与 UI 层严格分离
2. **渐进式开发** — 以 MVP 为起点，逐步扩展功能
3. **开发者友好** — 清晰的分层设计，核心层不依赖 UI 框架，可独立单元测试
4. **正确处理文本** — 对编码检测、换行符保留、撤销/重做原子性等基础能力做到可靠
5. **开放可定制** — 语法高亮通过注册中心扩展，编辑命令可自由组合

## 📝 文本编码与换行策略

### 编码处理

- 内部统一使用 .NET 字符串的原生编码 **UTF-16**
- **打开文件**：BOM 优先（UTF-8 BOM、UTF-16 LE/BE）→ 启发式检测 → 回退系统默认编码
- **保存文件**：默认保持打开时的编码；可通过"另存为"指定编码

支持的编码：UTF-8、UTF-8 BOM、UTF-16 LE、UTF-16 BE、GB2312、GBK、系统默认编码

### 换行符处理

- 打开文件时自动检测主导换行风格（CRLF / LF / CR）
- 编辑过程中内部统一处理，不丢失换行符信息
- 保存时默认保留原文件换行风格，可通过菜单手动切换

## 🛠️ 开发方式

本项目采用 **vibe coding** 方式推进，通过 CodeBuddy 辅助协作开发。

## 📄 许可证

本项目采用 [MIT License](LICENSE) 发布。

## 🔐 代码来源与许可证兼容性

- 本项目所有代码均为原创或来自与 MIT 兼容的许可证（MIT、BSD-2-Clause、BSD-3-Clause、Apache-2.0）
- 允许参考开源项目（包括 Notepad++）的设计思想和架构理念，但严禁直接拷贝或近似改写其源码
- 所有合入主干的代码统一按 MIT 许可证发布
- 如提交第三方代码，需在 PR 中声明来源和许可证

## 🤝 贡献

请参阅 [CONTRIBUTING.md](CONTRIBUTING.md) 了解贡献流程与规范。

## 📊 项目状态

**MVP 阶段** — 核心功能已完成并持续迭代中。

**已实现功能：**
- [x] 文件操作（新建、打开、保存、另存为）
- [x] 多标签页管理
- [x] 编辑能力（插入/删除/替换、撤销/重做）
- [x] 查找与替换（正则、大小写、全字匹配、Replace All）
- [x] 列编辑模式（块选择、列插入/删除/替换）
- [x] 编码检测与转换
- [x] 换行符检测与保留
- [x] 语法高亮（C#、JSON、XML、纯文本）
- [x] 状态栏显示
- [x] 自动换行（Word Wrap）
- [x] 跳转到行号（Ctrl+G）
- [x] 最近打开文件列表（自动持久化）

**计划中的功能：**
- [ ] 行号显示（侧边 gutter 行号）
- [ ] 自动缩进增强
- [ ] 括号匹配高亮
- [ ] 多级撤销粒度控制
- [ ] 更多语言的高亮支持（Python、Java 等）
- [ ] 主题切换（暗色模式等）
- [ ] 插件系统
