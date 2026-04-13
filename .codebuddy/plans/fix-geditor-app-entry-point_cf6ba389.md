---
name: fix-geditor-app-entry-point
overview: 为 GEditor.App WPF 项目创建缺失的入口文件（App.xaml、App.xaml.cs、MainWindow.xaml、MainWindow.xaml.cs），修复 CS5001 编译错误。
todos:
  - id: create-app-xaml
    content: 创建 App.xaml 和 App.xaml.cs，定义 WPF 应用入口
    status: completed
  - id: create-mainwindow
    content: 创建 MainWindow.xaml 和 MainWindow.xaml.cs，定义主窗口
    status: completed
  - id: verify-build
    content: 验证 dotnet run --project src/GEditor.App/GEditor.App.csproj 能成功编译运行
    status: completed
    dependencies:
      - create-app-xaml
      - create-mainwindow
---

## Product Overview

修复 GEditor.App WPF 项目编译错误 CS5001（缺少静态 Main 入口点），使 `dotnet run --project src/GEditor.App/GEditor.App.csproj` 能成功编译并运行一个空白主窗口。

## Core Features

- 在 `src/GEditor.App/` 下创建最小 WPF 启动文件集（App.xaml + MainWindow.xaml 及其代码隐藏文件）
- 利用 WPF SDK 自动生成 Main 入口点机制，无需手写 Program.cs
- 确保项目引用的 GEditor.Core 和 GEditor.Syntax 依赖正常加载

## Tech Stack

- 框架: WPF (.NET 8.0, `Microsoft.NET.Sdk` + `<UseWPF>true</UseWPF>`)
- 语言: C# 12

## Implementation Approach

**策略**: 创建最小 WPF 文件集，利用 SDK 内置的自动 Main 生成机制。

当 `.csproj` 使用 `<UseWPF>true</UseWPF>` 时，WPF 构建管线会检测到 `ApplicationDefinition` 构建动作的文件（默认 `App.xaml`），自动生成包含 `Main` 方法的入口点代码。因此只需创建 4 个标准 WPF 文件：

1. `App.xaml` — 声明为 `ApplicationDefinition`（默认行为），设置 `StartupUri` 指向 `MainWindow.xaml`
2. `App.xaml.cs` — `App` 类，继承 `Application`
3. `MainWindow.xaml` — 主窗口 XAML，设置窗口标题和基础属性
4. `MainWindow.xaml.cs` — `MainWindow` 类，继承 `Window`

**关键决策**: 不创建 `Program.cs`，遵循 WPF SDK 约定让编译器自动生成入口点，保持与项目架构一致。RootNamespace 为 `GEditor.App`，文件中使用 `GEditor.App` 命名空间。

## Implementation Notes

- `App.xaml` 的 `x:Class` 必须与 `RootNamespace` 一致（`GEditor.App.App`），确保代码隐藏类正确关联
- `MainWindow.xaml` 的 `StartupUri` 使用相对路径 `MainWindow.xaml`
- 保持最小化实现，不引入 DI 容器配置等逻辑，仅为 MVP 阶段提供可运行的窗口宿主

## Directory Structure

```
src/GEditor.App/
├── GEditor.App.csproj   # [EXISTING] 项目文件，无需修改
├── App.xaml             # [NEW] WPF 应用定义文件，设置 StartupUri="MainWindow.xaml"
├── App.xaml.cs          # [NEW] App 类代码隐藏，继承 Application
├── MainWindow.xaml      # [NEW] 主窗口 XAML，设置标题和基础尺寸
└── MainWindow.xaml.cs   # [NEW] MainWindow 类代码隐藏，继承 Window
```