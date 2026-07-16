---
name: et-build
description: ET build, proto export, and server startup. Use when compiling with dotnet build ET.sln, exporting Proto to C#, or starting ET.App.
---

# et-build - ET Build 入口

> 适配 ET9，路径与 ET10 不同。

## 何时使用

- 编译项目（唯一命令：`dotnet build ET.sln`）
- 导出 Proto 文件（`.proto` -> C#）
- 启动服务器

## 不要加载

- 只是改代码、写测试，还没到编译或导出环节
- 只是导出 Luban 配置（用 `et-luban`）

## 默认动作

1. 编译统一使用 `dotnet build ET.sln`，不单独编译包或 IDE 私有方案
2. Model / Hotfix 程序集不能用 IDE 编译，必须走 ET 编译入口

## 优先入口

- 编译：`dotnet build ET.sln`
- Proto 导出：`dotnet Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll`
- 启动服务器：`dotnet ./Bin/ET.App.dll --Console=1`
- UBridge CLI 编译：`dotnet build Packages/cn.etetet.ubridge/DotNet~/ET.UBridge.csproj`

## 按需补读

- `references/et-build-commands.md`：命令、常见排查
