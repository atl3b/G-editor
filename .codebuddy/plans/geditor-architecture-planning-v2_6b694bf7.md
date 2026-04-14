---
name: geditor-architecture-planning-v4
overview: G-editor 完整项目规划：涵盖架构设计、UI 与交互设计、项目里程碑及 CodeBuddy 实施路线
todos:
  - id: m0-scaffold
    content: 里程碑 0：项目初始化与仓库规范
    status: completed
  - id: m1-ui-core
    content: 里程碑 1：基础 UI 外壳与文档模型
    status: completed
  - id: m2-fileio
    content: 里程碑 2：文件 IO、编码与换行支持
    status: completed
  - id: m3-editing
    content: 里程碑 3：编辑核心与撤销重做
    status: completed
  - id: m4-search
    content: 里程碑 4：搜索替换 UI 与逻辑
    status: completed
  - id: m5-syntax
    content: 里程碑 5：基础语法高亮
    status: completed
  - id: m6-polish
    content: 里程碑 6：体验打磨与稳定化
    status: completed
  - id: m7-column-mode
    content: 增强功能：列模式（Column Mode / 矩形选区）
    status: completed
  - id: m6-test-p0-lineending
    content: P0: 新建 LineEndingDetectorTests.cs（换行符检测测试）
    status: completed
  - id: m6-test-p0-docmgr
    content: P0: 新建 DocumentManagerTests.cs（文档管理器生命周期测试）
    status: completed
  - id: m6-test-p0-buffer-column
    content: P0: 补充 EditorBuffer 列模式方法测试（GetColumnText/InsertAtColumns/DeleteAtColumns）
    status: completed
  - id: m6-test-p0-replaceall
    content: P0: 补充 SearchService ReplaceAll 执行验证测试
    status: completed
  - id: m6-test-p0-saveas
    content: P0: 补充 TextFileService SaveAs 编码/换行转换测试
    status: completed
  - id: m6-test-p1-valueobjects
    content: P1: 新建 Search 值对象专项测试（SearchMatch/Query/Options）
    status: completed
  - id: m6-test-p1-undoredo-ext
    content: P1: 补充 UndoRedoManager 连续序列 + 大批量命令测试
    status: completed
  - id: m6-test-p1-regex
    content: P1: 补充 SearchService 正则模式 + 特殊字符转义测试
    status: completed
  - id: m6-test-p1-encoding-cn
    content: P1: 补充 EncodingDetector GBK/中文文件检测测试
    status: completed
  - id: m6-test-p1-edgecases
    content: P1: 新建边界/异常测试套件（Unicode、空输入、极端参数）
    status: completed
  - id: m6-feature-wordwrap
    content: 功能: 自动换行（Word Wrap）基础实现
    status: completed
  - id: m6-feature-gotoline
    content: 功能: 跳转到行号（Ctrl+G）
    status: completed
  - id: m6-feature-recentfiles
    content: 功能: 最近打开文件列表
    status: completed
  - id: m6-final-verify
    content: 最终 dotnet build 全量验证 + README 完善
    status: completed
---

# G-editor 项目规划文档

## 1. 项目概述

### 1.1 项目背景

G-editor 是一个从零开发的轻量桌面文本编辑器，产品目标类似 Notepad++。

本项目允许开发者阅读 Notepad++ 开源项目源码来理解功能设计、模块划分和交互方式，但**严禁直接拷贝、逐行翻译、机械改写或高度近似重写其实现代码**。所有设计决策应基于独立思考和原创设计。

项目主协议采用 **MIT License**。允许接受与 MIT 兼容的贡献代码来源，包括 MIT、BSD-2-Clause、BSD-3-Clause、Apache-2.0。**不接受** GPL / LGPL / 其他与 MIT 不兼容许可的代码直接合入项目。所有合入主干的代码统一按 MIT 发布。

### 1.2 技术栈

| 技术 | 选择 |
| --- | --- |
| 编程语言 | C# |
| 运行时 | .NET 8 |
| UI 框架 | WPF |
| 首选平台 | Windows |

### 1.3 解决方案目录结构

```
G-editor/
├── GEditor.sln
├── LICENSE                              # MIT
├── README.md
├── CONTRIBUTING.md
├── .gitignore
│
├── src/
│   ├── GEditor.Core/                    # 核心逻辑库 — 零 UI 依赖
│   ├── GEditor.Syntax/                  # 语法高亮库 — 零 UI 依赖，不引用 Core
│   └── GEditor.App/                     # WPF 应用宿主
│       ├── ViewModels/                  # MVVM ViewModel 层
│       └── Views/                       # XAML 视图层
│
└── tests/
    └── GEditor.Tests/                  # 单元测试项目
```

---

## 2. UI 与交互设计

### 2.1 UI 设计目标

G-editor 的 UI 目标是：**清晰、稳定、低认知负担、高编辑效率**。

具体原则如下：

| 原则 | 说明 |
| --- | --- |
| 可发现性 | 文本编辑器常用能力应在界面上可被发现和操作 |
| 键盘优先 | 鼠标和键盘都支持，但偏重键盘效率，快捷键覆盖常用操作 |
| 最小干扰 | 避免过度装饰性设计，界面元素服务于功能而非展示 |
| MVP 优先 | 优先保证稳定的桌面编辑器外壳，而非复杂视觉效果 |

### 2.2 主窗口布局

主窗口采用经典三段式结构，由上到下分为顶部区域、中央区域和底部区域。

#### 2.2.1 顶部区域

**标题栏**

- 显示应用程序名称 "G-editor"
- 当前活跃文档的文件名（未保存时带 `*` 前缀）
- 窗口控制按钮（最小化、最大化/还原、关闭）

**菜单栏**

- 提供所有核心功能的入口
- 详细菜单结构见 2.3 节

**工具栏**（MVP 可简化或省略）

- 快速访问常用命令的图标按钮
- MVP 阶段可只保留：新建、打开、保存
- 后续可扩展：撤销、重做、查找

#### 2.2.2 中央区域

**标签栏**

- 每个打开的文档对应一个标签
- 标签显示文件名，未命名文档显示 "Untitled N"
- 文档有未保存修改时在标题旁显示 `*`
- 支持鼠标点击切换、右键菜单关闭
- 详见 2.4 节

**编辑区**

- 承载文本编辑的核心区域
- 详见 2.5 节

**侧边区域**（MVP 可不启用）

- 预留用于文件浏览器、大纲视图等
- 架构可预留，MVP 阶段不实现

#### 2.2.3 底部区域

**状态栏**

- 显示当前文档的关键信息
- 详见 2.6 节

### 2.3 菜单与命令系统

#### 2.3.1 菜单结构

菜单栏应包含以下顶级菜单项及其子菜单：

**File（文件）**

| 菜单项 | 说明 |
| --- | --- |
| New（新建） | Ctrl+N，创建空白文档 |
| Open...（打开） | Ctrl+O，打开文件对话框 |
| Save（保存） | Ctrl+S，保存当前文档 |
| Save As...（另存为） | Ctrl+Shift+S，另存为新文件 |
| Close Tab（关闭标签） | Ctrl+W，关闭当前标签 |
| Exit（退出） | Alt+F4，退出应用程序 |

**Edit（编辑）**

| 菜单项 | 说明 |
| --- | --- |
| Undo（撤销） | Ctrl+Z，撤销最近操作 |
| Redo（重做） | Ctrl+Y / Ctrl+Shift+Z，重做撤销的操作 |
| Cut（剪切） | Ctrl+X，剪切选中文本 |
| Copy（复制） | Ctrl+C，复制选中文本 |
| Paste（粘贴） | Ctrl+V，粘贴剪贴板内容 |
| Select All（全选） | Ctrl+A，选中全部内容 |

**Search（搜索）**

| 菜单项 | 说明 |
| --- | --- |
| Find...（查找） | Ctrl+F，打开查找面板 |
| Replace...（替换） | Ctrl+H，打开替换面板 |
| Find Next（查找下一个） | F3，查找下一处 |
| Find Previous（查找上一个） | Shift+F3，查找上一处 |

**View（视图）**

| 菜单项 | 说明 |
| --- | --- |
| Toggle Status Bar（切换状态栏） | 显示/隐藏底部状态栏 |
| Toggle Line Numbers（切换行号） | 显示/隐藏行号列 |
| Column Mode（列模式） | Alt+鼠标拖动进入列编辑模式 |
| Toggle Word Wrap（切换自动换行） | 预留，MVP 暂不实现 |

**Edit（编辑）**

| 菜单项 | 说明 |
| --- | --- |
| Column Mode Insert（列模式插入） | Alt+Shift+I，在多行插入相同文本 |
| Column Mode Delete（列模式删除） | Alt+Shift+D，删除多列文本 |

**Encoding（编码）**

| 菜单项 | 说明 |
| --- | --- |
| Reopen with Encoding（重新打开指定编码） | 以指定编码重新加载文件 |
| Save with Encoding（保存为指定编码） | 以指定编码保存文件 |
| Change Line Ending（更改换行符） | 切换文档的换行符类型 |

**Language（语言）**

| 菜单项 | 说明 |
| --- | --- |
| Plain Text（纯文本） | 无语法高亮 |
| C/C++ | C/C++ 语法高亮 |
| C# | C# 语法高亮 |
| Python | Python 语法高亮（Phase 2 扩展） |
| JSON | JSON 语法高亮 |
| XML | XML 语法高亮 |

#### 2.3.2 命令系统绑定原则

菜单项应优先通过命令系统绑定到 ViewModel，具体原则如下：

| 原则 | 说明 |
| --- | --- |
| 命令绑定 | 菜单项绑定到 ViewModel 中的 `ICommand` 属性，不直接调用方法 |
| ViewModel 隔离 | 核心逻辑写在 ViewModel 或 Core 层，不在 code-behind 中直接编写业务逻辑 |
| UI 层职责 | UI 层通过命令和数据绑定与 Core 层交互，保持 UI 与业务逻辑分离 |
| 快捷键映射 | 快捷键在 InputGesture 中定义，与命令绑定协同工作 |

### 2.4 标签页设计

#### 2.4.1 基本行为

| 特性 | 说明 |
| --- | --- |
| 标签对应 | 每个打开的文档对应一个标签 |
| 标签标题 | 标签默认显示文件名 |
| 未命名文档 | 新建空白文档显示 "Untitled 1"、"Untitled 2" 等序号 |
| 未保存标记 | 文档有未保存修改时，在标题前或末尾显示 `*` 符号 |
| 标签切换 | 鼠标点击标签可切换活跃文档 |
| 标签关闭 | 提供关闭按钮或通过右键菜单关闭 |

#### 2.4.2 扩展功能（后续版本）

| 功能 | 说明 |
| --- | --- |
| 右键菜单 | 关闭当前、关闭其他、关闭所有 |
| 中键关闭 | 鼠标中键点击标签关闭文档 |
| 拖拽重排 | 拖拽标签调整顺序 |
| 标签压缩 | 标签过多时压缩显示或提供滚动 |

### 2.5 编辑区设计

#### 2.5.1 MVP 核心能力

编辑区在 MVP 阶段应实现以下基础能力：

| 能力 | 说明 |
| --- | --- |
| 文本显示与输入 | 支持文本的显示和键盘输入 |
| 光标定位 | 支持鼠标点击和键盘（方向键、Home/End 等）移动光标 |
| 选区 | 支持鼠标拖拽和 Shift+方向键扩大选区，支持双击选行 |
| 当前行高亮 | 可见区域中当前编辑行有背景色高亮 |
| 行号 | 左侧显示行号列，行号右对齐 |
| 基础缩进 | Tab 键插入缩进，Backspace 减少缩进 |
| 基础语法高亮 | 根据文件类型显示关键字、字符串等的高亮颜色 |

#### 2.5.2 MVP 暂不实现的能力

以下功能架构可预留，但 MVP 阶段不实现：

| 功能 | 说明 | 预留方式 |
| --- | --- | --- |
| Minimap | 右侧代码缩略图 | 架构预留 |
| 复杂 gutter 图标 | 断点、书签等 | 架构预留 |
| 代码折叠 | 折叠代码块 | 架构预留 |
| 复杂导航面板 | 符号列表、引用查找等 | 架构预留 |

### 2.6 状态栏设计

#### 2.6.1 MVP 显示内容

状态栏在 MVP 阶段至少显示以下信息：

| 信息 | 说明 | 示例 |
| --- | --- | --- |
| 当前编码 | 文档的字符编码 | UTF-8、GB2312 |
| 当前换行风格 | 文档的换行符类型 | CRLF、LF、CR |
| 光标位置 | 当前光标所在的行号和列号 | Ln 1, Col 1 |
| 语言模式 | 当前文档的语言类型 | Plain Text、C#、JSON |
| 文档修改状态 | 可选，未保存时显示标记 | Modified |

#### 2.6.2 扩展方向

| 方向 | 说明 |
| --- | --- |
| 可点击切换 | 后续可让编码、换行符、语言支持点击切换 |
| 编码菜单 | 状态栏点击显示编码选择下拉菜单 |

### 2.7 查找与替换 UI

#### 2.7.1 交互设计原则

查找与替换功能优先采用轻量、低打断的交互方式：

| 原则 | 说明 |
| --- | --- |
| 低侵入性 | 优先采用顶部内嵌搜索条或非模态面板，避免模态对话框阻断编辑 |
| 快速访问 | Ctrl+F 打开查找，Ctrl+H 打开替换 |
| 持续可见 | 搜索面板打开后保持可见，支持多次查找/替换 |

#### 2.7.2 查找功能

查找面板/条应包含以下元素：

| 元素 | 说明 |
| --- | --- |
| 搜索框 | 输入待查找的文本 |
| 上一处按钮 | 跳转到上一处匹配 |
| 下一处按钮 | 跳转到下一处匹配 |
| 区分大小写 | CheckBox，区分大小写匹配 |

#### 2.7.3 替换功能

替换面板在查找基础上增加以下元素：

| 元素 | 说明 |
| --- | --- |
| 替换输入框 | 输入替换文本 |
| 替换按钮 | 替换当前匹配项 |
| 全部替换按钮 | 替换所有匹配项 |

### 2.8 MVVM 与 UI 分层

#### 2.8.1 分层架构

G-editor 采用 MVVM 模式进行 UI 与业务逻辑的分离：

| 层级 | 职责 | 说明 |
| --- | --- | --- |
| **View** | XAML 布局与视觉结构 | 负责 UI 呈现和数据绑定，不编写业务逻辑 |
| **ViewModel** | 命令、状态映射、交互逻辑 | 消费 Core/Syntax 层服务，处理 UI 状态和用户交互 |
| **Core** | 文档内容、文件 IO、编码处理、编辑操作、搜索 | 纯业务逻辑，零 UI 依赖 |

#### 2.8.2 核心约束

| 约束 | 说明 |
| --- | --- |
| 不在 code-behind 中编写业务逻辑 | 所有业务逻辑通过 ViewModel 或 Core 层实现 |
| ViewModel 不依赖具体 WPF 控件 | ViewModel 通过接口和抽象与 UI 交互，便于测试 |
| App 引用 Core，Core 不依赖 WPF | Core 层保持平台无关性 |

#### 2.8.3 建议的 ViewModel 列表

| ViewModel | 职责 |
| --- | --- |
| **MainWindowViewModel** | 主窗口状态、菜单命令、文档集合管理、活跃文档切换 |
| **DocumentTabViewModel** | 单个标签的状态（文件名、修改状态、关闭命令） |
| **EditorViewModel** | 编辑区状态、光标位置、选区、文本内容绑定 |
| **StatusBarViewModel** | 状态栏数据（编码、换行符、光标位置、语言） |
| **SearchPanelViewModel** | 搜索/替换面板状态、搜索命令、匹配结果 |

### 2.9 MVP UI 范围定义

#### 2.9.1 MVP 必做

| 组件 | 说明 |
| --- | --- |
| 主窗口 | 基础窗口框架，包含标题栏和窗口控制按钮 |
| 菜单栏 | 完整菜单结构，支持键盘快捷键 |
| 多标签 | 支持打开多个文档，标签切换和关闭 |
| 编辑区 | 文本输入、光标、选区、行号、当前行高亮 |
| 状态栏 | 显示编码、换行符、光标位置、语言模式 |
| 基础查找/替换 | 搜索框、上一处/下一处、区分大小写、替换功能 |

#### 2.9.2 MVP 暂不做

| 组件 | 说明 |
| --- | --- |
| 自定义主题系统 | Phase 2 考虑 |
| 插件管理界面 | Phase 3 考虑 |
| 高级设置页 | Phase 2 考虑 |
| 多窗格布局 | Phase 3 考虑 |
| 拖拽标签分组 | Phase 3 考虑 |
| Minimap | Phase 4 考虑 |
| 复杂代码导航面板 | Phase 4 考虑 |

---

## 3. 架构设计

### 3.1 各项目职责说明

#### 3.1.1 GEditor.Core — 核心业务逻辑层

| 职责 | 说明 |
| --- | --- |
| 文档模型 | `Document` 作为聚合根，组合 `EditorBuffer`（纯文本内容）和元信息（编码、换行符、路径） |
| 编辑操作 | 命令模式（`IEditCommand`）+ `UndoRedoManager`，所有编辑通过命令执行，支持撤销/重做 |
| 文件 IO | `TextFileService` 统一读写，委托 `EncodingDetector` 和 `LineEndingDetector` 处理编码/换行 |
| 搜索替换 | `SearchService`（纯只读搜索），替换通过 `IEditCommand` + `UndoRedoManager` 实现 |
| 文档管理 | `DocumentManager` 管理多文档生命周期（创建/打开/关闭/切换） |

**关键约束**：不引用任何 WPF/UI 包，所有对外交互通过接口和事件。

#### 3.1.2 GEditor.Syntax — 语法高亮引擎

| 职责 | 说明 |
| --- | --- |
| Token 分类 | `ISyntaxHighlighter` 接收文本行，输出 `SyntaxToken` 列表 |
| 语言注册 | `ISyntaxHighlighterRegistry` 管理扩展名 → 高亮器的映射 |
| 语言定义 | `ILanguageDefinition` 描述语言元数据，与高亮器分离 |
| 规则驱动 | `RegexBasedHighlighter` 基类 + `HighlightRule` 统一模式 |
| 可扩展 | 新语言只需新增 `Highlighter` 类 + 注册一行，不改引擎代码 |

**关键约束**：不引用 WPF 包和 Core 包，输出纯数据（`SyntaxToken` 列表），UI 层负责着色映射。

#### 3.1.3 GEditor.App — WPF 应用宿主

| 职责 | 说明 |
| --- | --- |
| UI 渲染 | 通过 WPF View 渲染编辑器、标签页、状态栏、搜索面板 |
| MVVM 绑定 | ViewModel 消费 Core/Syntax 层服务，通过数据绑定驱动 UI |
| 依赖注入 | `App.xaml.cs` 配置 DI 容器，ViewModel 通过构造函数注入 |
| 对话框 | `WpfDialogService` 封装文件打开/保存对话框 |

#### 3.1.4 GEditor.Tests — 单元测试

| 职责 | 说明 |
| --- | --- |
| 核心逻辑测试 | Document、EditorBuffer、UndoRedoManager、搜索、文件 IO |
| 高亮测试 | 验证各语言 Token 输出正确性 |
| Mock 友好 | 所有服务通过接口注入，测试时用 Moq 替换 |

---

## 4. 项目里程碑

### 里程碑 0：项目初始化与仓库规范

**目标**：建立项目基础设施和开发规范，确保团队协作和代码质量的基础。

**关键任务**：

1. 创建 GEditor.sln 解决方案文件
2. 创建 4 个项目：GEditor.Core、GEditor.Syntax、GEditor.App、GEditor.Tests
3. 配置项目间引用关系
4. 创建 .gitignore 文件
5. 添加 MIT License 文件
6. 初始化 README.md 和 CONTRIBUTING.md

**主要产出物**：

- 可编译通过的解决方案骨架
- 项目引用关系图
- 基础 .gitignore 配置
- LICENSE 和基础文档

**里程碑入口条件**：无

**里程碑验收标准**：

- `dotnet build` 成功，无编译错误
- 四个项目均可独立编译
- Git 仓库结构符合 .gitignore 规范

---

### 里程碑 1：基础 UI 外壳与文档模型

**目标**：建立 WPF 应用的基础 UI 框架，实现文档数据模型的核心结构，完成主窗口布局。

**关键任务**：

1. **UI 层**：
   - 实现 MainWindow 主窗口骨架（标题栏、菜单栏、编辑区容器、状态栏）
   - 创建基础 ViewModel 结构（MainWindowViewModel、EditorViewModel、StatusBarViewModel）
   - 配置 DI 容器（App.xaml.cs）
   - 实现基础 XAML 布局和绑定

2. **Core 层**：
   - 定义值对象：LineEnding、DocumentEncodingInfo、DocumentLineEndingInfo
   - 实现 EditorBuffer 骨架（行存储 + 基本接口）
   - 实现 Document 聚合根骨架

**主要产出物**：

- MainWindow.xaml 完整布局（菜单栏、标签栏容器、编辑区容器、状态栏）
- MainWindowViewModel.cs（菜单命令绑定框架）
- EditorViewModel.cs（编辑区状态管理框架）
- EditorBuffer.cs（行存储核心）
- Document.cs（文档聚合根）

**里程碑入口条件**：

- 里程碑 0 完成
- Solution 可编译通过

**里程碑验收标准**：

- 应用程序可启动，显示主窗口
- 菜单栏可见（File、Edit、Search、View、Encoding、Language）
- 状态栏可见，显示默认信息
- 新建文档可创建，显示 "Untitled 1" 标签

---

### 里程碑 2：文件 IO、编码与换行支持

**目标**：实现文件的打开、保存功能，支持多种字符编码和换行符处理。

**关键任务**：

1. **UI 层**：
   - 实现文件对话框服务（WpfDialogService）
   - 菜单绑定：New、Open、Save、Save As
   - 状态栏显示当前编码和换行符
   - 编码/换行符切换菜单项

2. **Core 层**：
   - 实现 EncodingDetector（BOM 检测、回退策略）
   - 实现 LineEndingDetector（CRLF/LF/CR 识别）
   - 实现 TextFileService（文件读写）
   - 实现 DocumentManager（多文档生命周期管理）
   - 完成 EditorBuffer 的 Insert/Delete/Replace 方法

**主要产出物**：

- WpfDialogService.cs
- EncodingDetector.cs
- LineEndingDetector.cs
- TextFileService.cs
- DocumentManager.cs
- 完整的文件读写功能

**里程碑入口条件**：

- 里程碑 1 完成
- EditorBuffer 基本方法实现完成

**里程碑验收标准**：

- 可打开任意文本文件
- 可保存/另存为文件
- 编码检测正确（BOM、无 BOM、UTF-8、GB 系列）
- 换行符处理正确（打开保持、切换、保存转换）
- 状态栏正确显示编码和换行符类型

---

### 里程碑 3：编辑核心与撤销重做

**目标**：实现完整的编辑功能，支持撤销和重做操作。

**关键任务**：

1. **Core 层**：
   - 实现 IEditCommand 接口
   - 实现 InsertTextCommand、DeleteTextCommand、ReplaceTextCommand
   - 实现 UndoRedoManager（撤销重做栈）
   - 实现 CompositeEditCommand（复合命令，用于批量替换）

2. **UI 层**：
   - EditorView 绑定键盘输入事件
   - 菜单绑定：Undo、Redo、Cut、Copy、Paste、Select All
   - 工具栏绑定（可选）

**主要产出物**：

- IEditCommand.cs 及命令实现类
- UndoRedoManager.cs
- CompositeEditCommand.cs
- EditorViewModel 完整编辑逻辑
- 菜单项完整功能

**里程碑入口条件**：

- 里程碑 2 完成
- DocumentManager 可用

**里程碑验收标准**：

- 键盘可正常输入文本
- 选中文字后可剪切、复制、粘贴
- Ctrl+Z 可撤销，Ctrl+Y 可重做
- 撤销/重做栈正确维护
- 文档修改状态正确反映

---

### 里程碑 4：搜索替换 UI 与逻辑

**目标**：实现查找和替换功能，提供流畅的搜索体验。

**关键任务**：

1. **Core 层**：
   - 实现 ISearchService 接口
   - 实现 SearchQuery、SearchOptions、SearchMatch、ReplaceOptions、ReplaceResult
   - 实现 SearchService（FindAll、FindNext、FindPrevious、CreateReplaceAllCommand）

2. **UI 层**：
   - 实现 SearchPanelViewModel
   - 实现 SearchPanelView（搜索条或浮动面板）
   - 菜单绑定：Find、Replace、Find Next、Find Previous
   - 快捷键绑定：Ctrl+F、Ctrl+H、F3、Shift+F3

**主要产出物**：

- ISearchService.cs
- SearchService.cs
- SearchPanelViewModel.cs
- SearchPanelView.xaml
- 完整的查找替换功能

**里程碑入口条件**：

- 里程碑 3 完成
- EditorViewModel 光标定位功能可用

**里程碑验收标准**：

- Ctrl+F 打开搜索面板
- 输入搜索词后高亮所有匹配项
- F3/Shift+F3 在匹配项间跳转
- Ctrl+H 打开替换面板
- Replace 替换当前匹配，Replace All 替换所有匹配
- Replace All 支持一次撤销恢复全部

---

### 里程碑 5：基础语法高亮

**目标**：实现语法高亮引擎，为常见语言提供高亮支持。

**关键任务**：

1. **Syntax 层**：
   - 实现 TokenKind 枚举
   - 实现 SyntaxToken、SyntaxHighlightResult 值对象
   - 实现 ISyntaxHighlighter 接口
   - 实现 SyntaxHighlighterRegistry
   - 实现 RegexBasedHighlighter 基类
   - 实现内置语言高亮器：PlainText、C#、JSON、XML

2. **App 层**：
   - App.xaml.cs 中注册高亮器
   - EditorViewModel 集成语法高亮
   - XAML 中定义 TokenKind → 颜色映射（ResourceDictionary）
   - Language 菜单绑定语言切换

**主要产出物**：

- GEditor.Syntax 完整实现
- 4 个内置语言高亮器
- EditorView 语法着色渲染
- Language 菜单完整功能

**里程碑入口条件**：

- 里程碑 4 完成
- Syntax 项目结构建立

**里程碑验收标准**：

- .cs 文件打开后关键字、字符串、注释有颜色区分
- .json 文件键值对有颜色区分
- .xml 文件标签和属性有颜色区分
- .txt 文件无特殊高亮（PlainText）
- Language 菜单切换语言模式有效
- 文件打开时根据扩展名自动选择语言

---

### 里程碑 6：体验打磨与稳定化

**目标**：修复缺陷，提升稳定性，优化用户体验，为正式发布做准备。

**关键任务**：

1. **缺陷修复**：
   - 处理里程碑 1-5 中遗留的缺陷
   - 边界情况处理（大文件、特殊字符、编码冲突等）
   - 异常处理完善

2. **单元测试补全**：
   - 补充里程碑 2-5 中跳过的测试场景
   - 确保核心逻辑覆盖率达到目标

3. **文档完善**：
   - README 补充快速开始指南
   - CONTRIBUTING 补充开发规范
   - 更新架构文档

4. **体验优化**（可选）：
   - 启动性能优化
   - 大文件处理优化
   - 内存使用优化

**主要产出物**：

- 稳定可用的 G-editor v1.0
- 补充的单元测试
- 完善的 README 和 CONTRIBUTING

**里程碑入口条件**：

- 里程碑 0-5 全部完成
- 所有测试通过

**里程碑验收标准**：

- 所有已知缺陷已修复或延期记录
- 核心场景测试通过率 100%
- README 包含安装和使用说明
- 应用程序可正常启动、编辑、保存

---

## 5. 实施路线（面向 CodeBuddy）

以下 8 轮映射到 CodeBuddy 的执行计划，每轮对应特定的目标、里程碑、产出物和前置条件。

### 第 1 轮：初始化 solution 与项目结构

**目标**：建立项目骨架，配置解决方案和多项目结构。

**对应里程碑**：里程碑 0

**关键任务**：

1. 创建 GEditor.sln
2. 创建 src/ 目录结构
3. 创建 GEditor.Core.csproj（GEditor.Core 项目）
4. 创建 GEditor.Syntax.csproj（GEditor.Syntax 项目）
5. 创建 GEditor.App.csproj（GEditor.App 项目，引用 Core 和 Syntax）
6. 创建 tests/ 目录
7. 创建 GEditor.Tests.csproj（引用 Core 和 Syntax）
8. 配置项目间引用关系
9. 创建 .gitignore、LICENSE（MIT）、基础 README.md

**产出物**：

- GEditor.sln
- 4 个项目文件（.csproj）
- 基础项目引用关系
- .gitignore、LICENSE

**前置条件**：无

**进入下一轮条件**：`dotnet build` 成功，无编译错误

---

### 第 2 轮：设计文档模型

**目标**：实现核心数据模型，为 UI 和编辑功能奠定基础。

**对应里程碑**：里程碑 1

**关键任务**：

1. **定义值对象**：
   - LineEnding 枚举（CRLF, LF, CR, Unknown）
   - DocumentEncodingInfo 值对象
   - DocumentLineEndingInfo 值对象
   - DocumentChangedEventArgs

2. **实现 EditorBuffer 骨架**：
   - 行存储（List<string>）
   - GetAllText、SetAllText 方法
   - Insert、Delete、Replace 方法（骨架）
   - Changed 事件

3. **实现 Document 聚合根骨架**：
   - 组合 EditorBuffer + 元信息
   - FilePath、IsDirty、EncodingInfo、LineEndingInfo、DisplayName
   - ExecuteCommand、MarkAsSaved、GetFullText、LoadText

**产出物**：

- src/GEditor.Core/Documents/*.cs
- src/GEditor.Core/Buffer/EditorBuffer.cs

**前置条件**：第 1 轮完成

**进入下一轮条件**：EditorBuffer 和 Document 骨架代码编写完成，测试项目可引用

---

### 第 3 轮：设计编码与换行处理

**目标**：实现文件 IO 相关的编码检测和换行符处理。

**对应里程碑**：里程碑 2

**关键任务**：

1. **实现编码检测**：
   - IEncodingDetector 接口
   - EncodingDetector 实现（BOM 优先 + 启发式回退）

2. **实现换行符检测**：
   - ILineEndingDetector 接口
   - LineEndingDetector 实现

3. **实现文件服务**：
   - ITextFileService 接口
   - TextFileService 实现（Open、Save、SaveAs）

4. **实现文档管理器**：
   - IDocumentManager 接口
   - DocumentManager 实现（CreateNew、OpenAsync、Close、SetActive）

5. **编写单元测试**：
   - EncodingDetectorTests
   - TextFileServiceTests

**产出物**：

- src/GEditor.Core/IO/*.cs
- src/GEditor.Core/Management/*.cs
- tests/GEditor.Tests/IO/*.cs

**前置条件**：第 2 轮完成

**进入下一轮条件**：文件读写功能实现完成，编码检测测试通过

---

### 第 4 轮：设计编辑缓冲与撤销重做

**目标**：实现编辑命令模式和撤销重做系统。

**对应里程碑**：里程碑 3

**关键任务**：

1. **实现编辑命令接口**：
   - IEditCommand 接口

2. **实现具体命令**：
   - InsertTextCommand
   - DeleteTextCommand
   - ReplaceTextCommand

3. **实现撤销重做管理器**：
   - UndoRedoManager（执行、撤销、重做、清空）
   - CanUndo、CanRedo、UndoCount 属性

4. **实现复合命令**：
   - CompositeEditCommand（用于 Replace All）

5. **编写单元测试**：
   - EditorBufferTests（编辑操作）
   - UndoRedoManagerTests
   - CompositeEditCommandTests

**产出物**：

- src/GEditor.Core/Editing/*.cs
- tests/GEditor.Tests/Editing/*.cs

**前置条件**：第 3 轮完成

**进入下一轮条件**：UndoRedoManager 和所有命令类实现完成，测试通过

---

### 第 5 轮：设计主窗口、多标签和状态栏

**目标**：实现 WPF UI 的基础框架，包括主窗口、标签页和状态栏。

**对应里程碑**：里程碑 1

**关键任务**：

1. **配置 DI 容器**：
   - App.xaml.cs 中注册所有服务
   - 配置 ViewModel 注入

2. **实现主窗口布局**：
   - MainWindow.xaml（顶部菜单栏、标签栏容器、编辑区容器、底部状态栏）
   - MainWindow.xaml.cs（无业务逻辑）

3. **实现 ViewModel 结构**：
   - MainWindowViewModel（文档集合、活跃文档、菜单命令）
   - EditorViewModel（文本内容、光标、选区、语法高亮状态）
   - StatusBarViewModel（编码、换行符、光标位置、语言）

4. **实现标签页 UI**：
   - DocumentTabViewModel（文件名、修改标记、关闭命令）
   - TabControl 绑定文档集合

5. **实现状态栏**：
   - StatusBarView.xaml
   - 状态信息绑定

6. **实现基础菜单绑定**：
   - File 菜单（New、Open、Save、Save As、Close Tab、Exit）
   - View 菜单（Toggle Status Bar、Toggle Line Numbers）

**产出物**：

- src/GEditor.App/App.xaml 和 App.xaml.cs
- src/GEditor.App/MainWindow.xaml 和 MainWindow.xaml.cs
- src/GEditor.App/ViewModels/*.cs
- src/GEditor.App/Views/*.xaml

**前置条件**：第 1 轮完成

**进入下一轮条件**：主窗口可启动，菜单可响应，标签页可切换

---

### 第 6 轮：设计搜索与替换

**目标**：实现搜索和替换功能，提供流畅的搜索体验。

**对应里程碑**：里程碑 4

**关键任务**：

1. **实现搜索服务**：
   - ISearchService 接口
   - SearchQuery、SearchOptions、SearchMatch、ReplaceOptions、ReplaceResult 值对象
   - SearchService 实现（FindAll、FindNext、FindPrevious、CountMatches、CreateReplaceAllCommand）

2. **实现搜索面板 UI**：
   - SearchPanelViewModel
   - SearchPanelView.xaml（搜索框、替换框、选项、按钮）

3. **绑定菜单和快捷键**：
   - Search 菜单（Find、Replace、Find Next、Find Previous）
   - Ctrl+F、Ctrl+H、F3、Shift+F3 快捷键

4. **编写单元测试**：
   - SearchServiceTests
   - 替换集成测试

**产出物**：

- src/GEditor.Core/Search/*.cs
- src/GEditor.App/ViewModels/SearchPanelViewModel.cs
- src/GEditor.App/Views/SearchPanelView.xaml
- tests/GEditor.Tests/Search/*.cs

**前置条件**：第 4 轮完成

**进入下一轮条件**：搜索替换功能完整可用，测试通过

---

### 第 7 轮：设计语法高亮扩展架构

**目标**：实现语法高亮引擎，提供多语言支持。

**对应里程碑**：里程碑 5

**关键任务**：

1. **实现 Syntax 项目基础结构**：
   - TokenKind 枚举
   - SyntaxToken、SyntaxHighlightResult 值对象
   - ISyntaxHighlighter 接口
   - ISyntaxHighlighterRegistry 接口和实现

2. **实现高亮器基类**：
   - ILanguageDefinition、LanguageDefinition
   - HighlightRule
   - RegexBasedHighlighter 基类

3. **实现内置语言高亮器**：
   - PlainTextHighlighter
   - CSharpSyntaxHighlighter
   - JsonSyntaxHighlighter
   - XmlSyntaxHighlighter

4. **集成到 App 层**：
   - App.xaml.cs 注册高亮器
   - EditorViewModel 集成高亮逻辑
   - XAML 定义 TokenKind → 颜色映射

5. **绑定 Language 菜单**：
   - Language 菜单项绑定语言切换
   - 文件打开时自动识别语言

6. **编写单元测试**：
   - SyntaxHighlighterRegistryTests
   - CSharpHighlighterTests
   - JsonHighlighterTests
   - XmlHighlighterTests
   - PlainTextHighlighterTests

**产出物**：

- src/GEditor.Syntax/*.cs
- src/GEditor.Syntax/Languages/*.cs
- tests/GEditor.Tests/Syntax/*.cs
- App.xaml 颜色映射资源

**前置条件**：第 6 轮完成

**进入下一轮条件**：语法高亮功能完整，测试通过

---

### 第 8 轮：落地 README / CONTRIBUTING / LICENSE

**目标**：完善项目文档，为正式发布做准备。

**对应里程碑**：里程碑 6

**关键任务**：

1. **完善 README.md**：
   - 项目介绍
   - 功能特性
   - 系统要求
   - 安装指南
   - 快速开始
   - 键盘快捷键
   - 截图（可选）

2. **完善 CONTRIBUTING.md**：
   - 开发环境设置
   - 代码规范
   - 分支管理
   - Pull Request 流程
   - MIT 协议说明

3. **验证 LICENSE**：
   - 确认 LICENSE 文件为 MIT License
   - 所有源文件头部版权声明一致

4. **项目整理**：
   - 清理临时文件和调试代码
   - 确保 .gitignore 正确
   - 最终 `dotnet build` 验证

**产出物**：

- 完整的 README.md
- 完整的 CONTRIBUTING.md
- LICENSE 文件

**前置条件**：第 7 轮完成，所有里程碑功能验收通过

**进入下一轮条件**：文档完整、可读、可用

---

## 6. 核心类 / 接口设计

### 6.1 值对象与枚举

```csharp
// === GEditor.Core/Documents/ ===

/// <summary>换行符类型枚举</summary>
public enum LineEnding
{
    CRLF,   // \r\n (Windows)
    LF,     // \n (Unix/macOS)
    CR,     // \r (Classic Mac)
    Unknown // 尚未检测
}

/// <summary>文档编码元信息 — 值对象</summary>
public sealed class DocumentEncodingInfo
{
    public System.Text.Encoding Encoding { get; init; }      // 检测到的编码
    public bool HasBom { get; init; }                         // 是否带 BOM
    public string DisplayName { get; init; } = string.Empty;  // 状态栏显示名
}

/// <summary>文档换行符元信息 — 值对象</summary>
public sealed class DocumentLineEndingInfo
{
    public LineEnding DetectedLineEnding { get; init; }
    public LineEnding ActiveLineEnding { get; set; }
    public string Sequence => DetectedLineEnding switch
    {
        LineEnding.CRLF => "\r\n",
        LineEnding.LF   => "\n",
        LineEnding.CR   => "\r",
        _               => Environment.NewLine
    };
}

/// <summary>文档变更事件参数</summary>
public sealed class DocumentChangedEventArgs : EventArgs
{
    public int StartLine { get; init; }
    public int EndLine { get; init; }
    public string ChangeType { get; init; } = string.Empty;
}
```

```csharp
// === GEditor.Core/Search/ ===

/// <summary>搜索选项 — 值对象</summary>
public sealed class SearchOptions
{
    public bool MatchCase { get; init; }
    public bool WholeWord { get; init; }
    public bool UseRegex { get; init; }

    public static SearchOptions Default => new();
    public static SearchOptions CaseSensitive => new() { MatchCase = true };
}

/// <summary>搜索条件 — 值对象</summary>
public sealed class SearchQuery
{
    public string Pattern { get; init; } = string.Empty;
    public SearchOptions Options { get; init; } = new();

    public static SearchQuery Create(string pattern, SearchOptions? options = null)
        => new() { Pattern = pattern, Options = options ?? SearchOptions.Default };
}

/// <summary>搜索匹配结果 — 值对象</summary>
public sealed class SearchMatch : IEquatable<SearchMatch>
{
    public int Line { get; init; }
    public int Column { get; init; }
    public int Length { get; init; }
    public string MatchedText { get; init; } = string.Empty;
    public string LineText { get; init; } = string.Empty;
}
```

```csharp
// === GEditor.Syntax/ ===

/// <summary>Token 语义类型枚举</summary>
public enum TokenKind
{
    None, Keyword, String, Comment, Number, Identifier,
    Operator, Delimiter, Type, Attribute, Preprocessor, PlainText
}

/// <summary>语法高亮 Token — 值对象</summary>
public sealed class SyntaxToken : IEquatable<SyntaxToken>
{
    public TokenKind Kind { get; init; }
    public int StartColumn { get; init; }
    public int Length { get; init; }
    public string Text { get; init; } = string.Empty;
    public int LineNumber { get; init; }
}
```

### 6.2 核心接口

```csharp
// === GEditor.Core/Buffer/EditorBuffer.cs ===

public sealed class EditorBuffer
{
    private readonly List<string> _lines = new();
    public int LineCount => _lines.Count;
    public string this[int index] => _lines[index];
    public IReadOnlyList<string> Lines => _lines.AsReadOnly();

    public event EventHandler<DocumentChangedEventArgs>? Changed;

    public string GetAllText(string lineEnding);
    public void SetAllText(string text);
    public string GetRange(int startLine, int startCol, int endLine, int endCol);
    public (int newLine, int newCol) Insert(int line, int column, string text);
    public (int newLine, int newCol) Delete(int line, int column, int length);
    public (int newLine, int newCol) Replace(int line, int column, int length, string newText);
    public int GetLineLength(int line);
}
```

```csharp
// === GEditor.Core/Documents/Document.cs ===

public sealed class Document : IDisposable
{
    public string FilePath { get; set; } = string.Empty;
    public bool IsNew { get; }
    public bool IsDirty { get; private set; }
    public DocumentEncodingInfo EncodingInfo { get; set; }
    public DocumentLineEndingInfo LineEndingInfo { get; set; }
    public string DisplayName { get; }
    public EditorBuffer Buffer { get; }

    public event EventHandler<DocumentChangedEventArgs>? Changed;

    void ExecuteCommand(IEditCommand command);
    void MarkAsSaved();
    string GetFullText();
    void LoadText(string text);
    public void Dispose();
}
```

```csharp
// === GEditor.Core/Editing/IEditCommand.cs ===

public interface IEditCommand
{
    void Execute(EditorBuffer buffer);
    void Undo(EditorBuffer buffer);
    string Description { get; }
}

public sealed class UndoRedoManager
{
    public bool CanUndo { get; }
    public bool CanRedo { get; }
    public int UndoCount { get; }

    void Execute(IEditCommand command);
    void Undo();
    void Redo();
    void Clear();
}

public sealed class CompositeEditCommand : IEditCommand
{
    public string Description { get; }
    public CompositeEditCommand(string description, IEnumerable<IEditCommand> commands);
    public void Execute(EditorBuffer buffer);
    public void Undo(EditorBuffer buffer);
}
```

```csharp
// === GEditor.Core/IO/ ===

public interface IEncodingDetector
{
    DocumentEncodingInfo Detect(string filePath);
    DocumentEncodingInfo Detect(byte[] fileBytes);
}

public interface ILineEndingDetector
{
    LineEnding Detect(string text);
}

public interface ITextFileService
{
    Document Open(string filePath);
    void Save(Document document);
    void SaveAs(Document document, string newFilePath, System.Text.Encoding? encoding = null, LineEnding? lineEnding = null);
}
```

```csharp
// === GEditor.Core/Search/ISearchService.cs ===

public interface ISearchService
{
    IReadOnlyList<SearchMatch> FindAll(EditorBuffer buffer, SearchQuery query);
    SearchMatch? FindNext(EditorBuffer buffer, SearchQuery query, int fromLine, int fromColumn);
    SearchMatch? FindPrevious(EditorBuffer buffer, SearchQuery query, int fromLine, int fromColumn);
    int CountMatches(EditorBuffer buffer, SearchQuery query);
    IEditCommand CreateReplaceAllCommand(EditorBuffer buffer, SearchQuery query, string replacement);
}
```

```csharp
// === GEditor.Core/Management/IDocumentManager.cs ===

public interface IDocumentManager
{
    Document? ActiveDocument { get; }
    IReadOnlyList<Document> Documents { get; }

    Document CreateNew();
    Task<Document> OpenAsync(string filePath);
    void Close(Document document);
    void SetActive(Document document);
}
```

```csharp
// === GEditor.Syntax/ ===

public interface ISyntaxHighlighter
{
    string LanguageName { get; }
    IReadOnlySet<string> SupportedExtensions { get; }
    IReadOnlyList<SyntaxToken> HighlightLine(string lineText, int lineNumber);
    SyntaxHighlightResult HighlightDocument(IReadOnlyList<string> lines);
}

public interface ISyntaxHighlighterRegistry
{
    IReadOnlyList<ISyntaxHighlighter> Highlighters { get; }
    IReadOnlyList<ILanguageDefinition> Languages { get; }
    void Register(ISyntaxHighlighter highlighter);
    ISyntaxHighlighter? GetHighlighterByExtension(string fileExtension);
    ISyntaxHighlighter? GetHighlighterByLanguage(string languageName);
    bool IsSupported(string fileExtension);
}
```

---

## 7. 架构约束清单

| 约束 | 说明 |
| --- | --- |
| Core 不依赖 WPF | `GEditor.Core.csproj` 不引用任何 `Microsoft.*.Wpf` 包 |
| Syntax 不依赖 WPF | `GEditor.Syntax.csproj` 不引用任何 WPF 包 |
| Syntax 不依赖 Core | Syntax 是独立库，不引用 `GEditor.Core` |
| App 引用 Core + Syntax | `GEditor.App.csproj` 添加 ProjectReference |
| Tests 引用 Core + Syntax | `GEditor.Tests.csproj` 添加 ProjectReference |
| 事件驱动 | Document/EditorBuffer 通过事件通知变更，ViewModel 订阅 |
| 依赖注入 | App.xaml.cs 配置 DI，ViewModel 构造函数注入接口 |
| 命令模式 | 所有编辑通过 IEditCommand 执行，支持撤销/重做 |
| 搜索只读 | `ISearchService` 不直接修改 Buffer，替换通过 IEditCommand |
| 替换原子性 | "Replace All" 使用 `CompositeEditCommand` 打包 |
| Token 不含颜色 | `SyntaxToken` 和 `TokenKind` 不包含任何颜色/样式信息 |
| ViewModel 不写业务逻辑 | 所有业务逻辑在 Core 层，ViewModel 仅做状态映射 |
| MVP 优先 | 复杂功能（Minimap、代码折叠等）架构预留，MVP 不实现 |

---

## 8. MIT 合规边界说明

### 8.1 允许的行为

- 阅读 Notepad++ 源码理解功能设计和交互方式
- 基于理解独立设计和实现类似功能
- 参考其他 MIT/BSD/Apache 许可的开源项目
- 接受这些协议许可的代码贡献

### 8.2 禁止的行为

- 直接拷贝 Notepad++ 源码
- 逐行翻译 Notepad++ 实现
- 机械改写变量名但不改结构
- 高度近似重写（保留相同结构和逻辑）
- 合并 GPL/LGPL 等不兼容许可的代码

### 8.3 合规建议

| 做法 | 说明 |
| --- | --- |
| 独立设计 | 先理解需求，再独立设计实现方案 |
| 多源参考 | 参考多个开源项目而非单一来源 |
| 重写注释 | 如需参考注释逻辑，务必用自己语言重写 |
| 代码审查 | 合入代码前进行合规审查 |

---

## 9. 附录

### 9.1 键盘快捷键参考

| 功能 | 快捷键 |
| --- | --- |
| 新建 | Ctrl+N |
| 打开 | Ctrl+O |
| 保存 | Ctrl+S |
| 另存为 | Ctrl+Shift+S |
| 关闭标签 | Ctrl+W |
| 退出 | Alt+F4 |
| 撤销 | Ctrl+Z |
| 重做 | Ctrl+Y |
| 剪切 | Ctrl+X |
| 复制 | Ctrl+C |
| 粘贴 | Ctrl+V |
| 全选 | Ctrl+A |
| 查找 | Ctrl+F |
| 替换 | Ctrl+H |
| 查找下一个 | F3 |
| 查找上一个 | Shift+F3 |

### 9.2 技术栈版本要求

| 技术 | 最低版本 | 推荐版本 |
| --- | --- | --- |
| .NET SDK | .NET 8.0 | .NET 8.0 LTS |
| C# | C# 12 | C# 12 |
| MSBuild | 17.0+ | 最新稳定版 |
| Windows | Windows 10 | Windows 11 |

### 9.3 项目文件结构总览

```
G-editor/
├── GEditor.sln
├── LICENSE                              # MIT
├── README.md
├── CONTRIBUTING.md
├── .gitignore
│
├── src/
│   ├── GEditor.Core/
│   │   ├── GEditor.Core.csproj
│   │   ├── Documents/
│   │   ├── Buffer/
│   │   ├── Editing/
│   │   ├── IO/
│   │   ├── Search/
│   │   └── Management/
│   │
│   ├── GEditor.Syntax/
│   │   ├── GEditor.Syntax.csproj
│   │   └── Languages/
│   │
│   └── GEditor.App/
│       ├── GEditor.App.csproj
│       ├── App.xaml / App.xaml.cs
│       ├── MainWindow.xaml / .xaml.cs
│       ├── ViewModels/
│       ├── Views/
│       ├── Converters/
│       ├── Services/
│       └── Helpers/
│
└── tests/
    └── GEditor.Tests/
        ├── GEditor.Tests.csproj
        ├── Documents/
        ├── Buffer/
        ├── Editing/
        ├── IO/
        ├── Search/
        ├── Syntax/
        └── Selection/  ← [新增] 列模式测试
```

---

## 10. 当前状态总览（2026-04-14 更新）

### 10.1 里程碑完成状态

| 里程碑 | 名称 | 状态 | 完成日期 |
|--------|------|------|----------|
| M0 | 项目初始化与仓库规范 | **已完成** | 初始 |
| M1 | 基础 UI 外壳与文档模型 | **已完成** | 初始 |
| M2 | 文件 IO、编码与换行支持 | **已完成** | 初始 |
| M3 | 编辑核心与撤销重做 | **已完成** | 初始 |
| M4 | 搜索替换 UI 与逻辑 | **已完成** | 初始 |
| M5 | 基础语法高亮 | **已完成** | 初始 |
| M6 | 体验打磨与稳定化 | **已完成** | 2026-04-14 |
| M7+ | **列模式（增强功能）** | **已完成** | 2026-04-14 |

### 10.2 项目统计

**代码规模：**
- 业务逻辑 .cs 文件：~55 个
- 总代码行数：~5000+ 行
- 测试文件：18 个

**分层统计：**
- GEditor.Core: 26 个源文件（Documents、Buffer、Editing、IO、Search、Management、Selection）
- GEditor.Syntax: 14 个源文件（基础设施 + 4 种语言高亮器）
- GEditor.App: 15 个源文件（ViewModels、Controls、Views、Services、Converters）

### 10.3 已完成的增强功能：列模式 (Column Mode)

> 该功能不在原始 v3 计划中，作为独立需求实现。

#### 功能范围
- 矩形选区（Alt+鼠标拖动）创建 ColumnSelection
- 列模式复制/剪切/粘贴（剪贴板按行拼接）
- 列模式插入（Alt+Shift+I）/删除（Alt+Shift+D）
- 半透明蓝色 Adorner 渲染矩形选区高亮
- 状态栏 "[列模式]" 指示
- 全部操作通过 CompositeEditCommand 支持原子撤销

#### 新增文件清单
| 文件 | 层级 | 说明 |
|------|------|------|
| `src/GEditor.Core/Selection/SelectionMode.cs` | Core | 选区模式枚举 Stream/Column |
| `src/GEditor.Core/Selection/ColumnSelection.cs` | Core | 矩形选区值对象（record struct） |
| `src/GEditor.Core/Selection/IColumnEditOperation.cs` | Core | 列编辑操作接口 |
| `src/GEditor.Core/Editing/ColumnInsertCommand.cs` | Core | 多行同时插入命令 |
| `src/GEditor.Core/Editing/ColumnDeleteCommand.cs` | Core | 矩形选区删除命令 |
| `src/GEditor.Core/Editing/ColumnReplaceCommand.cs` | Core | 先删后插替换命令 |
| `src/GEditor.App/Controls/ColumnSelectionAdorner.cs` | App | WPF Adorner 绘制半透明矩形 |
| `tests/GEditor.Tests/Selection/ColumnSelectionTests.cs` | Tests | 列选区数据模型测试 |
| `tests/GEditor.Tests/Editing/ColumnCommandTests.cs` | Tests | 列命令单元测试 |
| `tests/GEditor.Tests/Editing/ColumnEditModeTests.cs` | Tests | 列编辑集成测试 |

### 10.4 测试覆盖差距分析（2026-04-14 更新）

以下按模块列出**有源码但缺少/不足测试**的文件，以及**完全缺失的核心功能测试**。

#### 10.4.1 缺失测试的 Core 层源文件

| 源文件 | 模块 | 现有测试状态 | 需补充 |
|--------|------|-------------|--------|
| `LineEndingDetector.cs` | IO | **无独立测试** | 新建 `LineEndingDetectorTests.cs`：CRLF/LF/CR/Unknown/Mixed 检测 |
| `DocumentManager.cs` | Management | **无独立测试** | 新建 `DocumentManagerTests.cs`：CreateNew/OpenAsync/Close/SetActive 生命周期 |
| `DocumentEncodingInfo.cs` | Documents | **无独立测试**（仅间接覆盖） | 值对象构造、默认值验证 |
| `DocumentLineEndingInfo.cs` | Documents | **无独立测试**（仅间接覆盖） | Sequence 属性、ActiveLineEnding 切换 |
| `SearchMatch.cs` / `SearchQuery.cs` / `SearchOptions.cs` / `ReplaceOptions.cs` / `ReplaceResult.cs` | Search | **无值对象专项测试** | 边界构造、Equals/GetHashCode |
| `SelectionMode.cs` | Selection | **无独立测试** | 枚举值完整性 |

#### 10.4.2 测试不充分的已有测试文件

| 测试文件 | 覆盖范围 | 缺失的关键场景 |
|----------|---------|----------------|
| `EditorBufferTests.cs` | SetAllText/Insert/Delete/Replace/GetRange 基础操作 | **列模式方法未测**：`GetColumnText()` / `InsertAtColumns()` / `DeleteAtColumns()`；边界：空 buffer、超大文本、Unicode 多字节字符、行首/行尾插入删除 |
| `DocumentTests.cs` | 基本生命周期 + Undo/Redo | **缺**：FilePath 变更后 DisplayName 更新、EncodingInfo/LineEndingInfo 设置传播、多文档场景下事件隔离 |
| `UndoRedoManagerTests.cs` | 基本 Undo/Redo 栈操作 | **缺**：Redo 后再 Undo 的连续性、Clear 后重新操作、大量命令（1000+）栈稳定性 |
| `CompositeEditCommandTests.cs` | 基本组合执行/撤销 | **缺**：嵌套 CompositeCommand、空子命令列表与单命令的边界 |
| `SearchServiceTests.cs` | FindAll/FindNext/FindPrevious/CountMatches | **缺**：正则模式（UseRegex）、跨行匹配行为、空 buffer、特殊正则字符转义、ReplaceAll 命令执行验证 |
| `EncodingDetectorTests.cs` | BOM 检测 + 空数据回退 | **缺**：UTF-8 无 BOM 与 GBK 中文检测区分、null 输入、大文件的启发式 null-byte 检测 |
| `TextFileServiceTests.cs` | Open/Save 基础流程 | **缺**：SaveAs 编码转换、SaveAs 换行符转换、Open 不存在文件异常、文件锁定场景 |
| `ColumnSelectionTests.cs` | Normalized/GetLineRanges/Offset | 覆盖较好，可补充：反向选区（End < Start）极端坐标、负数坐标容错 |
| `ColumnCommandTests.cs` | 列插入/删除/替换/撤销 | 覆盖较好，可补充：单行选区、全空行选区、超长文本列操作 |

#### 10.4.3 完全缺失的测试类别

| 缺失测试类别 | 说明 | 建议优先级 |
|--------------|------|-----------|
| **IO 集成测试** | LineEndingDetector + TextFileService 联合端到端测试 | P0 |
| **Management 集成测试** | DocumentManager + TextFileService 完整工作流 | P0 |
| **Search 替换集成测试** | SearchService.CreateReplaceAllCommand 执行后的实际 Buffer 验证 | P1 |
| **边界/异常测试套件** | 空字符串、null、负数索引、超大输入等统一收集 | P1 |
| **Unicode/多语言测试** | CJK 字符、Emoji、组合字符在 Insert/Delete/Search 中的正确性 | P2 |
| **性能基准测试** | 大文件（10000+ 行）的 EditorBuffer 操作耗时 | P3 |

#### 10.4.4 测试覆盖统计

| 模块 | 源文件数 | 有测试文件数 | 覆盖率(估算) |
|------|---------|------------|-------------|
| Buffer | 1 | 1 | ~70% (缺列模式方法) |
| Documents | 5 | 1 | ~40% (缺值对象) |
| Editing | 9 | 5 | ~75% (缺 Insert/Delete/ReplaceTextCommand 独立测试) |
| IO | 6 | 2 | ~50% (缺 LineEndingDetector) |
| Search | 7 | 1 | ~60% (缺值对象) |
| Management | 2 | 0 | **0%** |
| Selection | 3 | 1 | ~80% |
| Syntax | 14 | 6 | ~85% |
| App | 15 | 0 | **0%** (ViewModels 依赖 WPF，可测性有限) |
| **合计** | **62** | **17** | **~55%** |

---

### 10.5 缺失核心功能清单

以下功能在当前计划中**未被规划或规划不足**，但作为一个可用文本编辑器应当具备：

#### 10.5.1 高优先级（影响基本可用性）

| 功能 | 说明 | 当前状态 | 建议 |
|------|------|---------|------|
| **自动换行（Word Wrap）** | 编辑区长行自动折行显示 | 计划中标注"MVP 暂不实现" | M8 中实现基础版本 |
| **Tab 键缩进增强** | Tab 插入空格 vs 制表符选择、Shift+Tab 减少缩进 | 基础 Tab 已实现 | 增强：支持 Tab 大小配置 |
| **括号匹配高亮** | 光标在括号附近时高亮对应括号 | 未实现 | 可作为 M8 一部分 |
| **关闭前保存提示** | 已在 MainWindowViewModel 实现 | 已有 | 需要测试验证 |

#### 10.5.2 中优先级（提升编辑体验）

| 功能 | 说明 | 当前状态 | 建议 |
|------|------|---------|------|
| **缩进指引线** | 编辑区显示垂直缩进参考线 | 未实现 | M9 规划 |
| **迷你地图（Minimap）** | 右侧代码缩略图 | Phase 4 规划 | 保持远期 |
| **多步撤销/重做分组** | 连续输入视为一个撤销单元 | 未实现 | M9 规划 |
| **跳转到行号（Ctrl+G）** | 对话框输入行号跳转 | 未实现 | M8 规划 |
| **最近打开文件列表** | File 菜单显示最近打开的 N 个文件 | 未实现 | M8 规划 |

#### 10.5.3 低优先级（锦上添花）

| 功能 | 说明 | 建议 |
|------|------|------|
| 多窗口/分屏编辑 | Phase 3 | 远期 |
| 主题系统（亮色/暗色） | Phase 2 | 远期 |
| 插件架构 | Phase 3 | 远期 |
| 宏录制/回放 | 类似 Notepad++ Macro | 远期 |

---

### 10.6 M6 剩余工作项（更新）

以下工作尚未完全收尾：

#### 第一阶段：测试补全（P0 - 必须完成）

- [x] **P0** ✅ 新建 `tests/GEditor.Tests/IO/LineEndingDetectorTests.cs`
  - CRLF 文本 → CRLF、LF 文本 → LF、CR 文本 → CR
  - 空字符串 → Unknown、混合换行符 → 主导类型
  - 纯文本无换行 → Unknown
- [x] **P0** ✅ 新建 `tests/GEditor.Tests/Management/DocumentManagerTests.cs`
  - CreateNew 创建文档并设为 Active
  - OpenAsync 打开文件并加入 Documents 集合
  - Close 从集合移除并 Dispose，Active 自动切换
  - SetActive 切换活跃文档，抛异常当文档不在管理中
  - Close 最后文档时 Active 变为 null
- [x] **P0** ✅ 补充 `EditorBufferTests.cs` — 列模式方法测试
  - GetColumnText：单行/多行/空行/越界选区
  - InsertAtColumns：多位置插入、空文本、越界行
  - DeleteAtColumns：多范围删除、返回被删文本、越界范围
- [x] **P0** ✅ 补充 `SearchServiceTests.cs` — ReplaceAll 执行验证
  - CreateReplaceAllCommand 返回的命令 Execute 后 Buffer 内容正确
  - ReplaceAll 支持一次 Undo 恢复全部
- [x] **P0** ✅ 补充 `TextFileServiceTests.cs` — SaveAs 编码和换行转换

#### 第二阶段：测试完善（P1 - 应当完成）

- [x] **P1** ✅ 新建 `tests/GEditor.Tests/Search/SearchValueObjectTests.cs` — SearchMatch/Query/Options 值对象验证
- [x] **P1** ✅ 补充 `UndoRedoManagerTests.cs` — 连续 Undo/Redo/Clear 序列、大批量命令（1000+）
- [x] **P1** ✅ 补充 `SearchServiceTests.cs` — UseRegex 正则模式、特殊字符转义
- [x] **P1** ✅ 补充 `EncodingDetectorTests.cs` — GBK/中文文件检测、UTF-16 无BOM启发式
- [x] **P1** ✅ 新建 `tests/GEditor.Tests/EdgeCases/EdgeCaseTests.cs` — Unicode多字节字符、空输入、极端参数

#### 第三阶段：功能完善

- [x] 自动换行（Word Wrap）基础实现
- [x] 跳转到行号（Ctrl+G）
- [x] 最近打开文件列表
- [x] README 补充快速开始指南和截图
- [x] 最终 `dotnet build` 全量验证 + 全量测试通过
