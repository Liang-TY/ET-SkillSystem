# ET 框架包机制 — 以 cn.etetet.npk 为示例

## 一、什么是包（Package）

ET 框架使用 Unity 包管理器（UPM）组织代码。每个包是一个独立的功能模块，放在 `Packages/` 目录下，通过 `package.json` 声明身份和依赖。

### 包的层级

```
┌─────────────────────────────────────────┐
│  业务层   lockstep, login, console      │
│          依赖多个框架包，含具体游戏逻辑    │
├─────────────────────────────────────────┤
│  框架层   netinner, proto, ui, yooassets │
│          依赖核心包，提供通用功能模块      │
├─────────────────────────────────────────┤
│  基础库层 truesync, lsentity, npk        │
│          依赖极少，独立性强，纯工具/算法   │
├─────────────────────────────────────────┤
│  核心层   core, sourcegenerator          │
│          无外部依赖，所有包的根基          │
└─────────────────────────────────────────┘
```

**原则：上层依赖下层，下层不能依赖上层。**

---

## 二、包的目录结构

ET 框架的包分两类：

### A. 基础库包（如 truesync、npk）

只有纯 C# 工具代码，不依赖 Unity 引擎，不使用 Entity/Component/System 模式。

```
cn.etetet.xxx/
├── package.json              # Unity 包描述
├── packagegit.json           # Git 元数据
├── Ignore.ET.Xxx.asmdef     # 占位 asmdef（防止 Unity 报错）
├── Runtime/
│   ├── ET.Xxx.asmdef        # 程序集定义
│   ├── SomeType.cs           # 源文件
│   └── ...
└── DotNet~/                  # 服务端独立编译
    └── ET.Xxx.csproj
```

### B. 业务包（如 lockstep）

使用 ET 的 ECS 模式，代码分层放置，有 Unity 依赖。

```
cn.etetet.xxx/
├── package.json
├── Runtime/
│   ├── Model/ET.Model.asmdef
│   ├── Hotfix/ET.Hotfix.asmdef
│   ├── ModelView/ET.ModelView.asmdef
│   └── HotfixView/ET.HotfixView.asmdef
├── Scripts/                  # 实际源码（通过 asmref 指向 asmdef）
│   ├── Model/Share/
│   │   └── AssemblyReference.asmref  → { "reference": "ET.Model" }
│   ├── Hotfix/Share/
│   │   └── AssemblyReference.asmref  → { "reference": "ET.Hotfix" }
│   ├── ModelView/Client/
│   │   └── AssemblyReference.asmref  → { "reference": "ET.ModelView" }
│   └── HotfixView/Client/
│       └── AssemblyReference.asmref  → { "reference": "ET.HotfixView" }
├── DotNet~/                  # 服务端独立编译
│   ├── Model/ET.Model.csproj
│   ├── Hotfix/ET.Hotfix.csproj
│   └── App/ET.App.csproj
└── Editor/                   # Unity 编辑器扩展
```

### 分层含义

| 层 | 内容 | 可引用 | 编译方式 |
|----|------|--------|---------|
| Model | Entity/Component 定义、数据结构 | Model | AOT |
| Hotfix | System 逻辑实现 | Model + Hotfix | 热更新 |
| ModelView | Unity 相关的 Component 定义 | Model + Unity | AOT |
| HotfixView | Unity 相关的 System 实现 | 全部 | 热更新 |

---

## 三、关键文件详解

### 3.1 package.json

Unity 包的身份证明，声明名称、版本、依赖的其他包。

```json
{
  "name": "cn.etetet.npk",
  "displayName": "ET.Npk",
  "version": "1.0.0",
  "unity": "2022.3",
  "description": "NPK/IMG 精灵图解析 + 动画配置",
  "dependencies": {
    "cn.etetet.core": "3.0.3"
  }
}
```

- `name`：全小写，以 `cn.etetet.` 开头
- `dependencies`：Unity 包级别的依赖，确保被依赖的包先加载

### 3.2 Runtime/Xxx.asmdef

Unity 程序集定义，决定源码编译成哪个 DLL、引用哪些其他程序集。

```json
{
  "name": "ET.Npk",
  "rootNamespace": "",
  "references": [
    "ET.Core",
    "ET.SourceGeneratorAttribute"
  ],
  "defineConstraints": [],
  "autoReferenced": true
}
```

- `references`：**程序集级别**的引用（不是包级别），即需要使用哪个 DLL 的类型
- `defineConstraints`：`["INITED"]` 表示需要 INITED 宏才编译（业务包常用），基础库留空
- `autoReferenced`：true 表示其他程序集自动引用此程序集

### 3.3 Ignore.Xxx.asmdef

防止 Unity 在某些情况下报错的占位文件：

```json
{
  "name": "Ignore.ET.Npk",
  "defineConstraints": ["IGNORE"]
}
```

`IGNORE` 宏永远不会定义，所以这个程序集永远不会编译。它只是占位用。

### 3.4 DotNet~/ET.Xxx.csproj

服务端独立编译的项目文件。与服务端共享同一份 Runtime 源码。

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>ET</RootNamespace>
    <LangVersion>12</LangVersion>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">
    <DefineConstants>DOTNET</DefineConstants>
    <OutputPath>$(SolutionDir)Bin</OutputPath>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="..\Runtime\**\*.cs">
      <Link>%(RecursiveDir)%(FileName)%(Extension)</Link>
    </Compile>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..." />
  </ItemGroup>
</Project>
```

关键点：
- `<Compile Include="..\Runtime\**\*.cs">`：共享 Runtime 下的源文件
- `<OutputPath>$(SolutionDir)Bin</OutputPath>`：输出到统一 Bin 目录
- `<DefineConstants>DOTNET</DefineConstants>`：标记为服务端编译

---

## 四、如何创建一个新包 — cn.etetet.npk 实战

### 需求

将 NPK/IMG 精灵图解析和动画配置从 lockstep 包中独立出来，作为基础库包供多个业务包复用。

### 步骤

#### 1. 创建包目录和文件

```
Packages/cn.etetet.npk/
├── package.json
├── Ignore.ET.Npk.asmdef
├── Runtime/
│   ├── ET.Npk.asmdef
│   ├── NpkSprite.cs          # 精灵数据类
│   ├── NpkImgParser.cs       # IMG 二进制解析器
│   ├── AnimClipData.cs       # 动画配置数据结构
│   └── AnimConfigRegistry.cs # 配置注册表 + AnimId 常量
└── DotNet~/
    └── ET.Npk.csproj
```

#### 2. 编写源码

所有源码放在 `Runtime/` 下，使用 `namespace ET`，标注 `[EnableClass]`。

#### 3. 在其他包中引用

在 lockstep 包中做两件事：

**a) package.json 添加包依赖（Unity 层面）**
```json
"dependencies": {
  "cn.etetet.npk": "1.0.0"
}
```

**b) asmdef 添加程序集引用（编译层面）**

需要用到 NPK 类型的程序集都要加：
```json
"references": [
  "ET.Npk",
  ...
]
```

**c) DotNet~ csproj 添加项目引用（服务端编译）**
```xml
<ProjectReference Include="..\..\..\cn.etetet.npk\DotNet~\ET.Npk.csproj" />
```

#### 4. 注册到 ET.sln

在 `ET.sln` 中添加新项目条目，使 `dotnet build ET.sln` 能编译它。

#### 5. 编译验证

```bash
dotnet build ET.sln
```

---

## 五、测试方法

资源加载测试在 lockstep 包的视图层（HotfixView）中进行：

1. `LSAnimResComponent` 挂载在 Room 上，`InitAsync` 时加载资源
2. 通过 `NpkImgParser.Parse()` 解析 IMG 二进制 → `NpkSprite[]`
3. 通过 `JsonUtility.FromJson<AnimClipData>()` 解析 JSON 配置
4. 通过 `AnimConfigRegistry.Register()` 注册动画配置
5. 运行时在 Unity Console 中观察 `[LSAnimRes]` 日志输出

### 预期日志输出

```
[LSAnimRes] IMG loaded, size: xxxxx bytes
[LSAnimRes] Parsed xx sprites from IMG
[LSAnimRes] Created xx Unity Sprites
[LSAnimRes] Idle config registered: 1 frames, loop=False, totalDuration=80ms
[LSAnimRes] Walk config registered: 6 frames, loop=True, totalDuration=400ms
```
