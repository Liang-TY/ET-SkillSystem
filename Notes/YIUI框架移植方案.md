# YIUI 框架移植方案

## 概述

从 `D:\Projects\yiui-et9\YIUI` 将 YIUI 框架（`cn.etetet.yiuiframework` v3.1.4）移植到当前项目 `D:\Projects\et9lockStepYIUITest`。

**核心原则：** 每一步操作后验证，出现问题先解决再继续。标记 👤 的步骤需要人工确认后才能执行。

---

## 依赖链与步骤顺序

```
yiuiinvoke (独立，仅依赖 ET.Core ── 已有)
   ↓
Sirenix Odin Inspector (插件，无其他依赖)
   ↓
DOTween (插件，无其他依赖)
   ↓
yiuiframework (依赖以上全部 + TextMeshPro)
```

按此依赖链从底层到上层依次复制，确保每步完成后 Unity 可以正常编译，避免中间状态出现大量编译错误。

---

## 前置分析总结

| 检查项 | 状态 |
|--------|------|
| `cn.etetet.core` 3.0.3 | ✅ 已有 |
| `cn.etetet.sourcegenerator` 3.0.1 | ✅ 已有 |
| `cn.etetet.yooassets` 2.3.6 | ✅ 已有 |
| `cn.etetet.referencecollector` 3.0.0 | ✅ 已有 |
| `cn.etetet.yiuiinvoke` | ❌ 缺失（需复制） |
| Sirenix Odin Inspector | ❌ 缺失（从 YIUI 项目 Plugins 复制） |
| DOTween | ❌ 缺失（从 YIUI 项目 Plugins 复制） |
| `Unity.TextMeshPro` 程序集 | ⚠️ Assets 中有 TMP 资源，但 manifest.json 无单独条目 |
| ET9 兼容性 | ✅ YIUI 含完整 ET9 条件编译适配层 |

---

## 步骤一：复制 cn.etetet.yiuiinvoke 包

> **依赖**：仅 `cn.etetet.core`（已有），无其他依赖，安全先复制。

### 1.1 操作

```powershell
robocopy "D:\Projects\yiui-et9\YIUI\Packages\cn.etetet.yiuiinvoke" `
         "D:\Projects\et9lockStepYIUITest\Packages\cn.etetet.yiuiinvoke" /E /NP /NFL /NDL
```

### 1.2 验证

- [ ] 确认目标目录 `Packages\cn.etetet.yiuiinvoke\package.json` 存在
- [ ] 确认 `package.json` 中 `"name": "cn.etetet.yiuiinvoke"`，`"version": "4.0.3"`
- [ ] 确认有 `Scripts/Core/Share/` 子目录，包含 `.asmref` 文件

### 1.3 预期问题

| 问题 | 可能性 | 处理方式 |
|------|--------|----------|
| Unity 重新编译报错 | 低 | yiuiinvoke 仅依赖 `cn.etetet.core`（已有），使用 `.asmref` 引用 `ET.Core`，无独立 asmdef，编译应该无问题 |

---

## 步骤二：复制 Sirenix Odin Inspector 插件

> **依赖**：无。这是第三方插件，yiuiframework 依赖它的 DLL，所以插件必须先就位。

### 2.1 操作

```powershell
# 先创建目标 Plugins 目录（如果不存在）
New-Item -ItemType Directory -Force -Path "D:\Projects\et9lockStepYIUITest\Assets\Plugins"

# 复制整个 Sirenix 目录（包含 .meta 文件）
robocopy "D:\Projects\yiui-et9\YIUI\Assets\Plugins\Sirenix" `
         "D:\Projects\et9lockStepYIUITest\Assets\Plugins\Sirenix" /E /NP /NFL /NDL
```

**复制内容包含（约 14MB）：**
- `Assemblies/` — 所有 DLL（Editor + Runtime + AOT + NoEditor 变体）
- `Assemblies/link.xml` × 2 — IL2CPP 裁剪保护
- `Odin Inspector/Assets/Editor/` — Odin Editor 资源（Shader、图标、配置）
- `Demos/` — 示例包（可跳过，不影响运行）
- `Readme.txt`

### 2.2 验证

- [ ] 确认 `Assets\Plugins\Sirenix\Assemblies\Sirenix.OdinInspector.Attributes.dll` 存在
- [ ] 确认 `Assets\Plugins\Sirenix\Assemblies\Sirenix.OdinInspector.Editor.dll` 存在（Editor 专用）
- [ ] 确认 `Assets\Plugins\Sirenix\Assemblies\Sirenix.Serialization.dll` 存在

### 2.3 预期问题

| 问题 | 可能性 | 处理方式 |
|------|--------|----------|
| DLL 平台设置不对导致运行时错误 | 低 | `.meta` 文件已经配置好平台过滤，直接复制即可 |
| `link.xml` 和项目已有的冲突 | 低 | 两份 `link.xml`（`Assemblies/` 和 `Assemblies/AOT/`）都是 Odin 专属的，不会冲突 |
| 👤 Odin 弹出 License 窗口 | 可能 | YIUI 项目使用的可能是已激活的版本，复制到新项目后可能需要重新激活。Odin 提供免费试用 |

---

## 步骤三：复制 DOTween 插件

> **依赖**：无。这也是第三方插件，yiuiframework 依赖它做窗口动画。

### 3.1 操作

```powershell
# 复制整个 Demigiant 目录
robocopy "D:\Projects\yiui-et9\YIUI\Assets\Plugins\Demigiant" `
         "D:\Projects\et9lockStepYIUITest\Assets\Plugins\Demigiant" /E /NP /NFL /NDL

# 复制 DOTween 全局设置
New-Item -ItemType Directory -Force -Path "D:\Projects\et9lockStepYIUITest\Assets\Resources"
robocopy "D:\Projects\yiui-et9\YIUI\Assets\Resources" `
         "D:\Projects\et9lockStepYIUITest\Assets\Resources" `
         "DOTweenSettings.asset" "DOTweenSettings.asset.meta" /NP /NFL /NDL
```

**复制内容包含（约 815KB）：**
- `DOTween/DOTween.dll` — 运行时（174KB），所有平台
- `DOTween/Editor/DOTweenEditor.dll` — Editor 专用（64KB）
- `DOTween/Modules/*.cs` — Unity 集成源码（UI、Physics、Sprite 等扩展）
- `DOTween/Modules/DOTween.Modules.asmdef` — 模块程序集定义

### 3.2 验证

- [ ] 确认 `Assets\Plugins\Demigiant\DOTween\DOTween.dll` 存在
- [ ] 确认 `Assets\Resources\DOTweenSettings.asset` 存在

### 3.3 预期问题

| 问题 | 可能性 | 处理方式 |
|------|--------|----------|
| DOTween.Modules.asmdef 引用缺失 | 中 | Modules 源码依赖 Unity 内置模块，如报错可暂时删除 Modules 文件夹或修复 asmdef 引用 |
| DOTweenSettings.asset 路径冲突 | 低 | 目标项目 `Assets/Resources/` 只有 `BuildinFileManifest.asset`，不冲突 |

---

## 步骤四：复制 cn.etetet.yiuiframework 包

> **依赖**：yiuiinvoke（步骤一）、Sirenix（步骤二）、DOTween（步骤三）、TextMeshPro（步骤六检查）。此时前三个依赖已就位。

### 4.1 操作

```powershell
robocopy "D:\Projects\yiui-et9\YIUI\Packages\cn.etetet.yiuiframework" `
         "D:\Projects\et9lockStepYIUITest\Packages\cn.etetet.yiuiframework" /E /NP /NFL /NDL
```

### 4.2 验证

- [ ] 确认 `Packages\cn.etetet.yiuiframework\package.json` 存在（version: 3.1.4）
- [ ] 确认 `Runtime\ET.YIUIFramework.asmdef` 存在
- [ ] 确认 `Editor\ET.YIUIFramework.Editor.asmdef` 存在
- [ ] 确认 `YIUI.BindSourceGenerator.dll` 和 `.meta` 存在（Roslyn Analyzer）

### 4.3 预期问题

| 问题 | 可能性 | 处理方式 |
|------|--------|----------|
| Unity 编译报错：缺少 `Sirenix.OdinInspector` 命名空间 | ❌ 不应出现 | 步骤二已完成 |
| Unity 编译报错：缺少 `DG.Tweening` 命名空间 | ❌ 不应出现 | 步骤三已完成 |
| Unity 编译报错：缺少 `cn.etetet.yiuiinvoke` 类型 | ❌ 不应出现 | 步骤一已完成 |
| Unity 编译报错：缺少 `Unity.TextMeshPro` 引用 | 🟡 可能 | 步骤六处理 |
| DOTween.Modules.asmdef 编译报错 | 🟡 可能 | 步骤三的遗留问题，此处统一处理 |

---

## 步骤五：复制 YIUI 运行时资源

> **依赖**：yiuiframework 包（步骤四）已就位。

### 5.1 操作

```powershell
# 复制 YIUI 包内置的运行时资源（关闭按钮预制体等）
robocopy "D:\Projects\yiui-et9\YIUI\Packages\cn.etetet.yiuiframework\Assets\GameRes" `
         "D:\Projects\et9lockStepYIUITest\Assets\GameRes" /E /NP /NFL /NDL

# 复制 YIUI 项目级 GameRes（YIUISettings 配置、通用 UI 源文件）
robocopy "D:\Projects\yiui-et9\YIUI\Assets\GameRes\YIUI" `
         "D:\Projects\et9lockStepYIUITest\Assets\GameRes\YIUI" /E /NP /NFL /NDL
```

### 5.2 验证

- [ ] 确认 `Assets\GameRes\YIUI\YIUI\Prefabs\Close\YIUIClose_Black.prefab` 存在
- [ ] 确认 `Assets\GameRes\YIUI\YIUISettings\YIUIConstAsset.txt` 存在
- [ ] 确认 `Assets\GameRes\YIUI\YIUISettings\YIUIAtlasData.asset` 存在

### 5.3 👤 手动步骤：配置 YIUIConstAsset

YIUI 项目使用 Odin JSON 序列化将配置保存为 `.txt`，需要在 Unity Editor 中重新配置以匹配当前项目路径：

1. 在 Unity 中选中 `Assets/GameRes/YIUI/YIUISettings/YIUIConstAsset.txt`
2. 检查以下配置项是否与当前项目匹配：
   - **项目命名空间** — 当前项目默认命名空间是什么？
   - **资源加载路径** — YooAssets 资源路径配置
   - **UI 根节点** — 是否需要调整

---

## 步骤六：检查并修复 TextMeshPro 引用

> **前置**：步骤四完成后 Unity 编译如果报 TMP 缺失则执行此步骤。

### 6.1 分析

YIUI 框架多个 asmdef 引用了 `Unity.TextMeshPro`：
- `ET.YIUIFramework.asmdef`（Runtime）
- `ET.YIUIFramework.Editor.asmdef`（Editor）

当前项目 `manifest.json` 中没有 `com.unity.textmeshpro` 条目，但 `Assets/TextMesh Pro/` 目录下有 TMP 资源。TMP 在 Unity 2022.3 中可能随 uGUI 包隐式引入。

### 6.2 操作

1. 在 Unity Package Manager 中确认 `TextMeshPro` 是否已安装
2. 如果缺失：通过 `Window → Package Manager → Unity Registry` 安装 `TextMeshPro`
3. 或者：在 `manifest.json` 中添加 `"com.unity.textmeshpro": "3.0.0"` 或更高版本

### 6.3 验证

- [ ] Unity 编译不再报告 `Unity.TextMeshPro` 引用缺失错误
- [ ] 👤 如果 TMP 安装后弹出 "TMP Importer" 窗口，点击 "Import TMP Essentials"

---

## 👤 步骤七：Unity 完整编译验证

### 7.1 操作

1. 打开 Unity 编辑器
2. 等待所有脚本编译完成
3. 检查 Console 窗口
4. 验证 YIUI 菜单项出现在正确位置

### 7.2 验证清单

- [x] Console 无编译 Error
- [x] **ET → YIUI 自动化工具** 菜单项存在（点击可打开 AutoTool 窗口）
- [x] **Assets 右键 → YIUI** 子菜单存在（Create UIPanel、Create UIView 等）
- [x] **GameObject 右键 → YIUI** 子菜单存在（Button、Text (TMP)、Image 等）
- [ ] 🔧 **Odin AOT 重新生成检查：** 当前项目删除了 YIUI 源项目生成的 `Sirenix.Serialization.AOTGenerated.dll`（内部引用了 `ET.YIUIFramework`）。在 IL2CPP 构建前（iOS/WebGL），需要通过 Odin 菜单 `Tools → Odin Inspector → Static AOT Generation` 重新生成，否则序列化可能失败。Editor 和 Mono 构建不受影响。

### 7.3 验证结果

**注意：** YIUI 没有传统的 `ET/YIUI` 多级菜单栏（与文档预期不同），它的菜单项分布在：

| 位置 | 菜单路径 | 功能 |
|------|----------|------|
| 顶部菜单栏 | `ET → YIUI 自动化工具` | 打开 YIUI AutoTool 主窗口 |
| Project 右键 | `Assets → YIUI → Create UIPanel / UIView / ...` | 创建 UI 资源和代码 |
| Hierarchy 右键 | `GameObject → YIUI → Button / Text (TMP) / ...` | 快速添加 UI 控件 |

全部菜单项已验证正常显示。

---

## 👤 步骤八：创建 YIUI 登录面板并接入登录流程（完整流程）

YIUI 使用 **YooAssets** 作为资源加载后端。整个流程分为 6 个阶段：

---

### 8.1 阶段一：在 Unity 中创建 YIUI 登录面板 Prefab

#### 8.1.1 操作

1. 在 Project 面板，导航到 `Packages/cn.etetet.lockstep/Assets/GameRes/YIUI/` 目录
2. 右键空白处 → **`YIUI → Create UIPanel`**
3. 在弹出的命名对话框中输入 `Login`
4. 自动生成以下文件：

| 生成的文件 | 路径 | 说明 |
|-----------|------|------|
| 预制体 | `Assets/GameRes/YIUI/Login/Prefabs/LoginPanel.prefab` | Unity 预制体 |
| Component | `Scripts/ModelView/Client/YIUIComponent/Login/LoginPanelComponent.cs` | 数据模型（AOT 层） |
| ComponentGen | `Scripts/ModelView/Client/YIUIGen/Login/LoginPanelComponentGen.cs` | 自动生成的绑定代码 |
| System | `Scripts/HotfixView/Client/YIUISystem/Login/LoginPanelComponentSystem.cs` | 逻辑代码（HotfixView 层） |
| SystemGen | `Scripts/HotfixView/Client/YIUIGen/Login/LoginPanelComponentSystemGen.cs` | 自动生成的系统代码 |

> **注意：** YIUI 的文件生成位置由 `YIUIConstAsset.txt` 中的 `UIProjectPackageResPath` 控制，模板是 `Assets/../Packages/cn.etetet.{0}/Assets/GameRes/YIUI`，`{0}` 为当前包名 `lockstep`。

#### 8.1.2 验证

- [x] 所有 6 个文件均已生成（Component / ComponentGen / System / SystemGen / Prefab / .meta）

---

### 8.2 阶段二：在 Prefab 上添加 UI 控件并配置绑定

#### 8.2.1 控件说明

YIUI 通过 **ComponentTable**（组件表）和 **EventTable**（事件表）来绑定 UI 控件和代码：

| 表类型 | 用途 | 示例 |
|--------|------|------|
| **ComponentTable** | 绑定 UI 控件引用 | `InputField` → `u_ComInput` |
| **EventTable** | 绑定 UI 事件 | `Button.onClick` → `u_EventClick1` |
| **DataTable** | 绑定数据驱动 | `Text.text` 绑定到某个字符串变量 |
| **CDETable** | 组件-数据-事件综合表 | Panel 的子组件和子 View 管理 |

#### 8.2.2 操作步骤

1. 在 Hierarchy 中打开 `LoginPanel.prefab`
2. YIUI 的 Panel 预制体自带 `YIUIChild` + `YIUIPanelComponent` + `YIUIWindowComponent`，不要删除
3. 添加 UI 控件：
   - 添加一个 **`InputField (TMP)`** 用于输入账号（也可用普通 `InputField`）
   - 添加一个 **`Button`** 用于触发登录
4. 将控件注册到绑定表：
   - 选中 Prefab 根节点，在 Inspector 中找到 **YIUI ComponentTable** 或 **自动绑定助手**
   - 将 InputField 拖入 ComponentTable，命名为 `u_ComInput`
   - 将 Button 的 `onClick` 事件拖入 EventTable，命名为 `u_EventClick1`
5. 点击 **YIUI AutoTool → 重新生成代码**，更新 `*Gen.cs` 绑定代码

#### 8.2.3 验证

- [x] Prefab 中可见 InputField 和 Button
- [x] `LoginPanelComponentGen.cs` 中包含 `u_ComInput` 和 `u_EventClick1` 字段
- [x] `LoginPanelComponentSystemGen.cs` 的 `UIBind()` 方法中包含绑定逻辑

---

### 8.3 阶段三：配置 YooAssets 收集 YIUI 资源（⚠️ 关键步骤）

#### 8.3.1 原理

YIUI 通过以下链路加载 Panel 预制体：
```
OpenPanelAsync<LoginPanelComponent>()
  → YIUIFactory.InstantiateGameObjectAsync(scene, pkgName="Login", resName="LoginPanel")
    → YIUILoadComponent.LoadAssetAsync<GameObject>("Login", "LoginPanel")
      → YIUILoadDI.LoadAssetAsyncFunc("Login", "LoginPanel", typeof(GameObject))
        → YIUIYooAssetsLoadComponent.LoadAssetAsync(arg2="LoginPanel", type=GameObject)
          → YooAssets.GetPackage("DefaultPackage").LoadAssetAsync("LoginPanel", ...)
```

关键点：
- `pkgName`（"Login"）在 YooAssets 实现中**被忽略**
- `resName`（"LoginPanel"）直接作为 YooAssets 的 location 地址
- YooAssets 使用 `AddressByFileName` 规则，地址即为文件名（不含路径和扩展名）

#### 8.3.2 当前问题

当前项目的 YooAssets 收集器配置（`Packages/cn.etetet.lockstep/Settings/AssetBundleCollectorSetting.asset`）**不包含** YIUI 资源路径。现有收集器只覆盖了 `Packages/cn.etetet.lockstep/Bundles/UI`，而 YIUI 资源在 `Packages/cn.etetet.lockstep/Assets/GameRes/YIUI/`。

**不加这一个步骤，运行时必然报错：找不到 "LoginPanel" 资源。**

#### 8.3.3 👤 操作（二选一）

**方式一：Unity Editor 手动操作（推荐）**

1. 打开菜单 **`Window → YIUI AutoTool`** 或 **`YooAsset → Asset Bundle Collector`**
2. 在 `DefaultPackage` 下添加一个新的 Collector Group：
   - **Group Name:** `YIUI`
   - **Collect Path:** `Packages/cn.etetet.lockstep/Assets/GameRes/YIUI`
   - **Collector Type:** `Main Asset Collector`
   - **Address Rule:** `AddressByFileName`
   - **Pack Rule:** `PackSeparately`
   - **Filter Rule:** `CollectPrefab`
3. 保存

**方式二：直接编辑配置文件**

编辑 `Packages/cn.etetet.lockstep/Settings/AssetBundleCollectorSetting.asset`，在 `Groups` 列表末尾添加：

```yaml
    - GroupName: YIUI
      GroupDesc: 
      AssetTags: 
      ActiveRuleName: EnableGroup
      Collectors:
      - CollectPath: Packages/cn.etetet.lockstep/Assets/GameRes/YIUI
        CollectorGUID: (自动生成)
        CollectorType: 0
        AddressRuleName: AddressByFileName
        PackRuleName: PackSeparately
        FilterRuleName: CollectPrefab
        AssetTags: 
        UserData: 
```

#### 8.3.4 验证

- [ ] YooAssets Collector 窗口中可以看到 `YIUI` 组，包含 `LoginPanel.prefab`
- [ ] 或者在 `AssetBundleCollectorSetting.asset` 中能找到 `YIUI` Group

---

### 8.4 阶段四：实现登录逻辑（代码改动）

#### 8.4.1 改动 1：初始化 YIUI 框架

**文件：** `EntryEvent3_InitClient.cs`

在 `AppStartInitFinish` 发布前，添加 YIUI 初始化：

```csharp
// YIUI 初始化
root.AddComponent<YIUIMgrComponent>();
await root.GetComponent<YIUIMgrComponent>().Initialize();
```

> **说明：** `YIUIMgrComponent.Awake()` 会自动创建 `YIUIRootComponent`；`Initialize()` 会绑定 YooAssets 加载函数、加载常量配置、初始化绑定表。

#### 8.4.2 改动 2：替换登录面板创建

**文件：** `AppStartInitFinish_CreateUILSLogin.cs`

```csharp
// 旧: await UIHelper.Create(root, UIType.UILSLogin, UILayer.Mid);
// 新: 
await root.YIUIRoot().OpenPanelAsync<LoginPanelComponent>();
```

#### 8.4.3 改动 3：替换登录面板关闭

**文件：** `LoginFinish_RemoveLoginUI.cs`

```csharp
// 旧: await UIHelper.Remove(scene, UIType.UILSLogin);
// 新:
await scene.YIUIMgr().ClosePanelAsync<LoginPanelComponent>();
```

#### 8.4.4 改动 4：按钮点击 → 登录逻辑

**文件：** `YIUISystem/Login/LoginPanelComponentSystem.cs`

```csharp
[YIUIInvoke(LoginPanelComponent.OnEventClick1Invoke)]
private static async ETTask OnEventClick1Invoke(this LoginPanelComponent self)
{
    GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
    string account = self.u_ComInput.text;
    if (string.IsNullOrEmpty(account)) account = "TestPlayer";

    LoginHelper.Login(self.Root(), globalComponent.GlobalConfig.Address,
        account, "123456").NoContext();  // 密码硬编码

    await ETTask.CompletedTask;
}
```

#### 8.4.5 说明

- 密码硬编码为 `"123456"`，单机测试够用
- 旧登录代码（`UILSLoginEvent`、`UILoginComponentSystem` 等）**保留不变**，如需回滚可恢复
- `LoginPanelComponent` 中的 `u_ComInput` 和 `u_EventClick1` 由 YIUI 自动绑定工具在 `UIBind()` 中自动关联

---

### 8.5 阶段五：编译热更 DLL + YooAssets 构建 + 运行测试

**关键：ET + HybridCLR 的 Scripts 代码不会被 Unity 自动编译！**

`Scripts/HotfixView/Client/` 的代码改动后，运行时加载的仍然是旧的 `Packages/cn.etetet.loader/Bundles/Code/ET.HotfixView.dll.bytes`。

> 修改 Scripts 代码后必须：**`F6` 编译** → **`F7` 重载**（菜单 `ET → Loader → Compile` / `Reload`）

#### 8.5.1 👤 操作

1. **YooAssets 构建资源包：**
   - 打开 `YooAsset → Asset Bundle Builder`
   - 选择 `DefaultPackage`
   - 点击 **Build** 构建资源包
   - 等待构建完成（输出到 `Bundles/` 目录）

2. **运行游戏：**
   - 点击 Unity Play 按钮
   - 观察 YIUI 登录面板是否正常显示
   - 在 InputField 中输入账号（或留空使用默认 "TestPlayer"）
   - 点击按钮登录

#### 8.5.2 验证

- [ ] YooAssets 构建成功，`Bundles/` 目录生成
- [ ] 游戏启动后 YIUI 登录面板显示
- [ ] InputField 可输入文字
- [ ] 点击按钮后进入登录流程
- [ ] 登录成功后 Panel 自动关闭，进入 Lobby
- [ ] Console 无运行时错误

#### 8.5.3 可能遇到的问题

| 问题 | 原因 | 解决方案 |
|------|------|----------|
| `YooAsset 加载资源包失败: DefaultPackage` | YooAssets 未初始化或包名不匹配 | 检查 `YIUIYooAssetsLoadComponent.Initialize("DefaultPackage")` 与构建包名一致 |
| 找不到 "LoginPanel" 资源 | YooAssets 收集器未包含 YIUI 路径（阶段三遗漏） | 重新执行阶段三，确保 `Assets/GameRes/YIUI/` 被收集 |
| YIUI 面板空白 | Prefab 中 UI 控件未正确配置 | 检查 ComponentTable 和 EventTable 绑定 |
| 点击按钮无反应 | EventTable 中 `u_EventClick1` 未绑定到 Button 的 onClick | 在 Prefab 中重新绑定 |
| `YIUIMgrComponent` 找不到 | EntryEvent3_InitClient 未添加初始化 | 检查改动 1 |
| Editor 模式加载正常但打包后失败 | YooAssets 的 Build 未包含新资源 | 重新构建 YooAssets 包 |

---

### 8.6 阶段六：Editor 模拟模式说明

如果 YooAssets 配置为 **Editor Simulate Mode**（编辑器模拟模式），Unity 中直接 Play 时 YooAssets 会直接从 Assets 目录加载资源，无需预先 Build。此时只要 YooAssets Collector 配置正确（阶段三），Play 模式就能正常找到资源。

检查当前项目的 YooAssets 运行模式：
- `YooAsset → Asset Bundle Builder → Play Mode` 或
- `Packages/cn.etetet.loader` 中的 YooAssets 初始化代码

如果是 **Offline/Host Play Mode**，则必须先 Build（阶段五）。

---

## 风险项汇总

| 风险 | 等级 | 缓解措施 |
|------|------|----------|
| Odin 许可证问题 | 🟡 | 检查 YIUI 源项目的 Odin 激活状态；如弹窗可试用或购买 |
| TextMeshPro 程序集缺失 | 🟡 | 通过 Package Manager 安装 |
| `cn.etetet.ui`（简单 UI）与 YIUI 功能重叠 | 🟢 | 命名空间不同，可共存；lockstep 对 `cn.etetet.ui` 的依赖保持不动 |
| HybridCLR 热更兼容性 | 🟡 | YIUI 的 HotfixView 代码需要通过 HybridCLR 热更，步骤八之后需单独测试热更流程 |
| YIUIConstAsset 配置不匹配 | 🟡 | 需要根据当前项目实际路径修改配置 |

---

## 执行日志

> 此表格在执行过程中逐项填写。

| 步骤 | 状态 | 开始时间 | 完成时间 | 备注 |
|------|------|----------|----------|------|
| 一：yiuiinvoke | ✅ 完成 | 17:30 | 17:31 | 66 files, 10 dirs, 106.5 KB, 全部验证通过 |
| 二：Sirenix | ✅ 完成 | 17:36 | 17:36 | 137 files, 18 dirs, 13.19 MB, 全部验证通过 |
| 三：DOTween | ✅ 完成 | 17:46 | 17:46 | 48 files, 723 KB + DOTweenSettings.asset, 全部验证通过 |
| 四：yiuiframework | ✅ 完成 | 17:47 | 17:47 | 1295 files, 219 dirs, 493 .cs, 1.68 MB, 全部验证通过 |
| 五：运行时资源 | ✅ 完成 | 17:56 | 17:56 | 27 files, 75 KB, 全部验证通过 |
| 六：TMP 检查 | ✅ 跳过 | - | - | 编译通过，TMP 引用已由 UpdateScriptsReferences 解决 |
| 七：Unity 编译 | ✅ 完成 | - | - | 编译通过无 Error，YIUI 菜单项全部验证正常 |
| 八：功能验证 | ✅ 完成 | - | - | YIUI LoginPanel 已接入登录流程并成功运行，相机问题已解决 |

---

## 遇到问题及解决方案

> 执行过程中动态记录。

| 步骤 | 问题描述 | 解决方案 | 状态 |
|------|----------|----------|------|
| 一 | `EventSystem` 缺少 `partial` 修饰符 → CS0260 编译错误 | YIUI 项目的 `cn.etetet.core` 将 `EventSystem` 改为 `partial class`，当前项目未改。修改 `Packages\cn.etetet.core\Scripts\Core\Share\World\EventSystem\EventSystem.cs` 第7行，`public class` → `public partial class` | ✅ 已解决 |
| 二 | `Sirenix.Serialization.AOTGenerated.dll` 加载失败：`Unable to resolve reference 'ET.YIUIFramework'` | **原因：** 该 DLL 位于 `Assets/Plugins/Sirenix/Assemblies/AOT/`，是 YIUI 源项目中通过 Odin 菜单 `Tools → Odin Inspector → Static AOT Generation` 生成的 IL2CPP AOT 预编译序列化代码。Odin 扫描源项目中所有带 `[OdinSerialize]` 的类型（包括 `ET.YIUIFramework` 中的 `YIUIConstAsset`、各类 `UIDataBind*` 等），生成序列化代码时将 `ET.YIUIFramework` 作为编译依赖嵌入 DLL 元数据。复制到当前项目时 yiuiframework 尚未就位，Unity 检测到缺失引用拒绝加载。**为何复制 yiuiframework 后不自动修复：** AOTGenerated.dll 是二进制编译产物，不是源代码。它对 `ET.YIUIFramework` 的引用是编译时写入 DLL 元数据的强引用，类似一个 `.exe` 依赖特定版本的 `.dll`。把 yiuiframework 复制过来只是把被引用的程序集放到了项目中，但 AOTGenerated.dll 本身是另一个项目编译出来的，它内部的引用指向的是源项目的程序集标识，不会自动重新指向新的。必须删除旧的 DLL 让 Odin 在当前项目中重新运行 AOT Generation，生成引用当前项目 `ET.YIUIFramework` 的新 DLL。Editor 和 Mono 构建不受影响 | ✅ 已解决 |
| 四 | `Unity.EditorCoroutines.Editor` 命名空间缺失 → CS0234 | 当前项目 manifest.json 缺少 `com.unity.editorcoroutines` 包。添加 `"com.unity.editorcoroutines": "1.0.1"` 到 manifest.json | ✅ 已解决 |
| 四 | `EPanelLayer` 等 YIUI 类型找不到 → CS0246 | YIUI 的 `packagegit.json` 声明了 `ScriptsReferences`（ModelView/HotfixView 需引用 `ET.YIUIFramework`），但 ET 构建系统未合并。在 Unity 中执行菜单 `ET → Loader → UpdateScriptsReferences` 即可自动合并所有包的引用 | ✅ 已解决 |
| 四 | `YIUIAtlasModule` 找不到 → CS0103 | 该类定义在 `cn.etetet.yiuiyooassets` 包中（YIUI 的 YooAssets 桥接层），当前项目缺失。从 YIUI 源项目复制了 `cn.etetet.yiuiyooassets` (v3.1.0, 64 files, 36 KB) | ✅ 已解决 |
| 八 | ET Scripts 代码改动后运行不生效，仍显示旧登录面板 | ET + HybridCLR 的 `Scripts/` 代码不会被 Unity 自动编译，运行时加载的是预编译的 `.dll.bytes`。修改后必须 `F6`（编译）→ `F7`（重载） | ✅ 已解决 |
| 八 | 运行时 YooAssets 报错 `The location is invalid: YIUIConstAsset` / `YIUIAtlasData` | YooAssets 收集器不包含 YIUI 资源路径。修复：1) 在 `AssetBundleCollectorSetting.asset` 中添加 YIUI 组，包含 3 个收集器（Settings 配置、框架内置预制体、lockstep 包面板）；2) `EnableAddressable: 0` → `1`，新增 `SupportExtensionless: 1`，与 YIUI 源项目对齐 | ✅ 已解决 |
| 八 | YIUI 面板已创建但 Game 窗口看不到 UI 内容；手动切换相机 Render Type 后可见但有蓝色背景 | **URP 相机渲染问题。** YIUI 源项目的 `Init.unity` 场景中预置了 YIUIRoot PrefabInstance，其 UICamera 被设为 Overlay 模式并已加入 MainCamera 的 URP Camera Stack。当前项目场景没有这个预配置，InitRoot 从 YooAssets 动态加载后相机不在 Stack 中，URP 不渲染。修复：在 Init.unity 场景中手动拖入 YIUIRoot.prefab，设相机 Render Type 为 Overlay，加入 MainCamera 的 Camera Stack | ✅ 已解决 |
