# UBridge YIUI 指令分析 — 通过 CLI 创建 UI Panel

> 分析手动创建 YIUI Panel 的完整过程，按拼 UI 的自然流程排列。每步评估能否做成 UBridge 命令。

---

## 总体流程图

```
步骤一：创建 Prefab → 步骤二：添加控件 → 步骤三：调整布局
    → 步骤四：CDE Table 绑定控件 + 创建事件
    → 步骤五：控件挂载 EventBind 组件 → 步骤六：保存 Prefab
    → 步骤七：生成代码
```

---

## 步骤一：创建 Panel Prefab

### 人工操作

`在 Assets/GameRes/YIUI/xxx/Prefabs/ 文件夹右键 → YIUI → Create UIPanel`

结果：在该文件夹下生成 `YIUIPanel.prefab`（带 `UIBindCDETable`、`UIBlockBG` 子对象）。

### 代码路径

**文件：** `Packages/cn.etetet.yiuiframework/Editor/MenuItem/MenuItemYIUIPanel.cs`

```csharp
// 1. 检查路径
var path = AssetDatabase.GetAssetPath(Selection.activeObject);
if (!path.Contains(YIUIConstHelper.Const.UIProjectResPath)) return error;

// 2. 创建 Panel GameObject
var panelObject = new GameObject();
panelObject.AddComponent<RectTransform>();
panelObject.AddComponent<CanvasRenderer>();
var cdeTable = panelObject.AddComponent<UIBindCDETable>();
cdeTable.UICodeType = EUICodeType.Panel;
cdeTable.PanelOption |= EPanelOption.TimeCache;

// 3. 添加 UIBlockBG 子对象
var bg = new GameObject("UIBlockBG") + RectTransform + CanvasRenderer + UIBlock;
bg.transform.SetParent(panelObject.transform);

// 4. 保存
PrefabUtility.SaveAsPrefabAsset(panelObject, savePath);
Object.DestroyImmediate(panelObject);
```

### 涉及 API

| API | 来源 | 可访问性 |
|-----|------|----------|
| `UIBindCDETable` | YIUIFramework (Runtime) | ✅ public |
| `EUICodeType.Panel` | YIUIFramework (Runtime) | ✅ public |
| `EPanelOption.TimeCache` | YIUIFramework (Runtime) | ✅ public |
| `PrefabUtility.SaveAsPrefabAsset` | UnityEditor | ✅ public |
| `UIBlock` | YIUIFramework (Runtime) | ✅ public |

### 拟实现命令

| 命令 | 参数 | 逻辑 |
|------|------|------|
| `YIUICreatePanel` | path, name | 创建 Panel GameObject → 配置 CDE Table → 添加 UIBlockBG → `SaveAsPrefabAsset` |
| `PrefabLoadForEdit` | prefabPath | `LoadPrefabContents` → 返回根 InstanceId（供后续步骤修改） |
| `PrefabSaveModified` | instanceId | `SaveAsPrefabAsset` → `UnloadPrefabContents` |

---

## 步骤二：添加 UI 控件

### 人工操作

Hierarchy 中选中 Panel → 右键 → `UI → Text / Image / Button / InputField / Toggle / Dropdown / ScrollView`。

### 核心 API

Unity 提供 `UnityEngine.UI.DefaultControls` 工厂类（Runtime 层），一行代码创建完整控件（含所有子对象层级）：

```csharp
using UnityEngine.UI;
var res = new DefaultControls.Resources();

var text   = DefaultControls.CreateText(res);
var image  = DefaultControls.CreateImage(res);
var button = DefaultControls.CreateButton(res);       // 含 Image + 子 Text
var input  = DefaultControls.CreateInputField(res);    // 含子 Text + Placeholder
var toggle = DefaultControls.CreateToggle(res);       // 含 Background + Checkmark + Label
var dropdown = DefaultControls.CreateDropdown(res);    // 含 Template + Scrollbar
var scrollView = DefaultControls.CreateScrollView(res); // 含 Viewport + Content
```

创建后只需 `SetParent(parentTransform)` + 重命名。

### 涉及 API

| API | 来源 | 可访问性 |
|-----|------|----------|
| `DefaultControls.CreateXxx()` | UnityEngine.UI (Runtime) | ✅ public |
| `PrefabUtility.LoadPrefabContents` | UnityEditor | ✅ public |
| `PrefabUtility.SaveAsPrefabAsset` | UnityEditor | ✅ public |

### 拟实现命令

| 命令 | 参数 | 逻辑 |
|------|------|------|
| `YIUIAddControl` | parentInstanceId, name, controlType | 调 `DefaultControls.CreateXxx()` → `SetParent` → 重命名 |

`controlType`：Text / Image / Button / Toggle / InputField / Dropdown / ScrollView。

---

## 步骤三：调整控件布局（RectTransform）

### 人工操作

选中控件 → Inspector → RectTransform 区域 → 设置锚点（anchors）、位置（pos）、大小（width/height）、轴心（pivot）。

### 核心 API

RectTransform 全部 public，6 个属性一个命令对一个：

| 属性 | API | 用途 |
|------|-----|------|
| Anchor | `anchorMin`, `anchorMax` | 锚点位置（0~1），控制拉伸和对齐 |
| Size | `sizeDelta` | 相对锚点的宽高，锚点分离时可用负值 |
| Position | `anchoredPosition` | 相对锚点的偏移 |
| Pivot | `pivot` | 轴心（0~1），旋转缩放中心 |
| Rotation | `localRotation` | 欧拉角度旋转 |
| Scale | `localScale` | 缩放 |

### 涉及 API

| API | 来源 | 可访问性 |
|-----|------|----------|
| `RectTransform.anchorMin/Max` | UnityEngine | ✅ public |
| `RectTransform.anchoredPosition` | UnityEngine | ✅ public |
| `RectTransform.sizeDelta` | UnityEngine | ✅ public |
| `RectTransform.pivot` | UnityEngine | ✅ public |

### 拟实现命令

**写操作（6 个）：**

| 命令 | 参数 | 设什么 |
|------|------|--------|
| `RectSetAnchor` | instanceId, minX/Y, maxX/Y | anchorMin, anchorMax |
| `RectSetSize` | instanceId, width, height | sizeDelta |
| `RectSetPos` | instanceId, x, y | anchoredPosition |
| `RectSetPivot` | instanceId, x, y | pivot |
| `RectSetRotation` | instanceId, x, y, z | localRotation |
| `RectSetScale` | instanceId, x, y, z | localScale |

**读操作（1 个通用命令）：**

| 命令 | 参数 | 返回 |
|------|------|------|
| `RectGet` | instanceId | 全部 RectTransform 属性（anchorMin/Max, sizeDelta, anchoredPosition, pivot, localRotation, localScale）+ width/height 计算值 |

读命令返回一个 `RectGetResponse`，包含 18 个字段，一次调用获取全部状态，用于验证写操作是否生效：

```json
{
  "AnchorMin": {"X":0,"Y":1}, "AnchorMax": {"X":1,"Y":1},
  "SizeDelta": {"X":200,"Y":50}, "AnchoredPosition": {"X":0,"Y":0},
  "Pivot": {"X":0.5,"Y":0.5},
  "LocalRotation": {"X":0,"Y":0,"Z":0},
  "LocalScale": {"X":1,"Y":1,"Z":1}
}
```

**示例：** Text 设为"顶部居中，全宽，高 50，贴顶"：

```bash
RectSetAnchor --id 12345 --minX 0 --minY 1 --maxX 1 --maxY 1
RectSetPos --id 12345 --x 0 --y 0
RectSetSize --id 12345 --w 0 --h 50
```

---

## 步骤四：CDE Table 绑定控件 + 创建事件

### 人工操作

双击 prefab → Hierarchy 选中根节点 → Inspector 中 `YIUI CDE Table` 面板：
- **C（Component）：** 把子控件拖入面板，命名（如 `u_ComInput`）
- **E（Event）：** 选择事件类型 + 参数，添加事件（如 `u_EventClick1`）

### 代码路径

**文件：** `Runtime/Core/YIUIBind/Code/Component/UIBindComponentTable_Editor.cs`

```csharp
// 控件绑定
cdeTable.ComponentTable.EditorAddComponent(component, "u_ComInput");
```

**文件：** `Runtime/Core/YIUIBind/Code/Event/UIBindEventTable_Editor.cs`

```csharp
// 事件创建
cdeTable.EventTable.EditorAddEvent(
    EUITaskEventType.Async,       // Task(异步) 或 Sync(同步)
    "u_EventClick1",              // 事件名
    new List<EUIEventParamType>() // 参数类型，0参=空列表
);
```

### 涉及 API

| API | 来源 | 可访问性 |
|-----|------|----------|
| `EditorAddComponent(Component, string)` | Editor | ✅ public（`#if UNITY_EDITOR`） |
| `EditorAddEvent(EUITaskEventType, string, List)` | Editor | ✅ public（`#if UNITY_EDITOR`） |
| `ComponentTable.AllBindDic` | Runtime | ✅ public（`IReadOnlyDictionary<string, Component>`） |
| `EventTable.EventDic` | Runtime | ✅ public（`IReadOnlyDictionary<string, UIEventBase>`） |

### 拟实现命令

**写操作（2 个）：**

| 命令 | 参数 | 逻辑 |
|------|------|------|
| `YIUIBindComponent` | prefabPath, controlInstanceId, bindName | 加载 prefab → 取 Component → `EditorAddComponent` → 保存 |
| `YIUIBindEvent` | prefabPath, eventName, eventType, paramTypes | 加载 prefab → 取 CDE Table → `EditorAddEvent` → 保存 |

**读操作（2 个）：**

| 命令 | 参数 | 返回 |
|------|------|------|
| `YIUIGetBindings` | prefabPath | `ComponentTable.AllBindDic` 所有条目（名称 + 组件类型名） |
| `YIUIGetEvents` | prefabPath | `EventTable.EventDic` 所有条目（名称 + 事件类型 + 参数类型列表） |

读命令用于验证写操作。`AllBindDic` 和 `EventDic` 都是 public `IReadOnlyDictionary`，直接遍历即可。

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| prefabPath | string | prefab 路径 |
| bindName | string | 绑定名，如 `"u_ComInput"` |
| eventName | string | 事件名，如 `"u_EventClick1"` |
| eventType | string | `"Sync"` 或 `"Task"`（异步） |
| paramTypes | string | 逗号分隔类型，如 `"Int,String"`，无参留空 |
| controlInstanceId | int | 子控件的 InstanceId |

---

## 步骤五：控件挂载 EventBind 组件

### 人工操作

CDE Table 创建事件后 → Hierarchy 选中按钮 → Inspector → Add Component → `UITaskEventBindClick`（异步）或 `UIEventBindClick`（同步）→ 下拉选择事件。

### 核心 API

```csharp
// 异步点击事件
var bind = buttonObject.AddComponent<UITaskEventBindClick>();
bind.EventName = "u_EventClick1";

// 同步点击事件
var bind = buttonObject.AddComponent<UIEventBindClick>();
bind.EventName = "u_EventClick1";
```

### 涉及 API

| API | 来源 | 可访问性 |
|-----|------|----------|
| `UITaskEventBindClick` | YIUIFramework (Runtime) | ✅ public |
| `UIEventBindClick` | YIUIFramework (Runtime) | ✅ public |
| `UIEventBind.EventName` (setter) | YIUIFramework (Runtime) | ✅ public |

### 拟实现命令

| 命令 | 参数 | 逻辑 |
|------|------|------|
| `YIUIAttachEvent` | prefabPath, targetInstanceId, eventName, eventType | 加载 prefab → 按类型 `AddComponent<UIEventBindClick>` 或 `UITaskEventBindClick` → 设 EventName → 保存 |

---

## 步骤六：保存 Prefab

`PrefabSaveModified`（步骤一已定义）覆盖此步骤，无需单独命令。

---

## 步骤七：生成代码

### 人工操作

选中 prefab → Inspector 中 `YIUI CDE Table` 面板 → 点击 **"Packages生成"** → 等待 → 检查 Console。

### 代码路径

**文件：** `Runtime/Core/YIUIBind/Code/CDE/UIBindCDETable_Editor.cs`

```csharp
[Button("Packages生成", 50)]
internal void CreatePackagesUICode()
{
    // 反射调用 UICreateModule.CreatePackages(cdeTable, false, false, packageName)
    InvokeTargetMethod(CreateModuleType, "CreatePackages", this, false, false, m_PackagesName);
    AssetDatabase.Refresh();
}
```

`CreateModuleType` = `YIUIFramework.Editor.UICreateModule`（Editor 程序集，反射调用）。

### UBridge 实现方式

```csharp
var t = Type.GetType("YIUIFramework.Editor.UICreateModule, ET.YIUIFramework.Editor");
t.GetMethod("CreatePackages").Invoke(null, new object[] { cdeTable, false, false, packageName });
AssetDatabase.Refresh();
```

### 生成产物

- `Scripts/ModelView/Client/YIUIComponent/{Name}/{Name}PanelComponent.cs`
- `Scripts/ModelView/Client/YIUIGen/{Name}/{Name}PanelComponentGen.cs`
- `Scripts/HotfixView/Client/YIUISystem/{Name}/{Name}PanelComponentSystem.cs`
- `Scripts/HotfixView/Client/YIUIGen/{Name}/{Name}PanelComponentSystemGen.cs`

### 拟实现命令

| 命令 | 参数 | 逻辑 |
|------|------|------|
| `YIUIGenerateCode` | prefabPath, packageName | 加载 prefab → `GetComponent<UIBindCDETable>` → 反射调 `UICreateModule.CreatePackages` → `AssetDatabase.Refresh()` |

---

## 命令汇总

| 步骤 | 命令 | 数量 |
|------|------|------|
| 一：创建 Prefab | `YIUICreatePanel`, `PrefabLoadForEdit`, `PrefabSaveModified` | 3 |
| 二：添加控件 | `YIUIAddControl` | 1 |
| 三：调整布局 | `RectGet`, `RectSetAnchor/Size/Pos/Pivot/Rotation/Scale` | 7 |
| 四：CDE 绑定 | `YIUIBindComponent`, `YIUIBindEvent`, `YIUIGetBindings`, `YIUIGetEvents` | 4 |
| 五：挂载 EventBind | `YIUIAttachEvent` | 1 |
| 六：保存 Prefab | 已由 `PrefabSaveModified` 覆盖 | 0 |
| 七：生成代码 | `YIUIGenerateCode` | 1 |
| **合计** | | **17** |

## 实施计划

| 阶段 | 内容 | 状态 |
|------|------|------|
| 1 | 注册 `YIUICreatePanel` + `PrefabLoadForEdit/SaveModified` | 待定 |
| 2 | 注册 `YIUIAddControl`（依赖 `DefaultControls`） | 待定 |
| 3 | 注册 `RectGet` + `RectSetAnchor/Size/Pos/Pivot/Rotation/Scale`（7 个） | 待定 |
| 4 | 注册 `YIUIBindComponent/Event` + `YIUIGetBindings/Events`（4 个） | 待定 |
| 5 | 注册 `YIUIAttachEvent` | 待定 |
| 6 | 注册 `YIUIGenerateCode`（反射 `UICreateModule`） | 待定 |
| 7 | CLI 支持 | 待定 |

全部 API public 或可反射调用，无阻塞。

---

## 附录：可独立实现的指令（按优先级）

部分指令**不依赖完整工作流**，可以先单独实现和测试，作为其他步骤的基础设施。

### 优先级 1：RectTransform 指令（7 个：1 读 + 6 写）— ✅ 已完成

**依赖：** 无。只需要场景中有带 `RectTransform` 的 GameObject。

**实现文件：**

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `UBridge_C_10000.proto` L1286-1400 | 7 组 Request/Response（14 个 message） |
| 生成 C# | `cn.etetet.proto/CodeMode/Model/Client/UBridge_C_10000.cs` | Proto2CS 生成 |
| Handler | `Scripts/ModelView/Client/UBridgeRectHandlers.cs` | `RectHelper` + 7 个 `static class Handler` |
| 注册 | `UBridgeEditorHost.cs` L117-123 | 7 个 `RegisterHandler` |
| CLI | `DotNet~/Program.cs` L125-144 | 参数解析 + switch case |

- `RectGet`：读全部属性，返回 `RectGetResponse`（18 个字段）
- `RectSetAnchor/Size/Pos/Pivot/Rotation/Scale`：6 个写操作，模式相同：

```csharp
// RectGet 示例
var rt = go.GetComponent<RectTransform>();
resp.AnchorMin = V2(rt.anchorMin); resp.AnchorMax = V2(rt.anchorMax);
resp.SizeDelta = V2(rt.sizeDelta); resp.AnchoredPosition = V2(rt.anchoredPosition);
...

// RectSetAnchor 示例
rt.anchorMin = new Vector2(r.MinX, r.MinY);
rt.anchorMax = new Vector2(r.MaxX, r.MaxY);
```

**测试步骤：**

```bash
# 1. 在 Unity 场景中创建一个 UI Image（右键 Hierarchy → UI → Image）
# 2. 选中这个 Image，记下 InstanceId（用 SelectionGet 获取）
dotnet Bin/ET.UBridge.dll SelectionGet

# 3. 先读取初始值
dotnet Bin/ET.UBridge.dll RectGet --instanceId 12345

# 4. 修改
dotnet Bin/ET.UBridge.dll RectSetAnchor --instanceId 12345 --minX 0 --minY 0 --maxX 1 --maxY 1
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId 12345 --rectWidth 200 --rectHeight 100

# 5. 再次读取验证
dotnet Bin/ET.UBridge.dll RectGet --instanceId 12345
# 对比步骤3的输出，确认 Anchor 和 Size 已变化
```

**验证标准：** `RectGet` 返回的值与 `RectSet` 设置的值一致。

---

### 优先级 2：YIUICreatePanel — ✅ 已完成

**依赖：** 无。完全独立的创建逻辑。

**实现文件：**

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `UBridge_C_10000.proto` L1402-1442 | YIUICreatePanel + PrefabLoadForEdit + PrefabSaveModified |
| 生成 C# | `cn.etetet.proto/CodeMode/Model/Client/UBridge_C_10000.cs` | Proto2CS 生成 |
| Handler | `Scripts/ModelView/Client/UBridgeYIUIPanelHandlers.cs` | 3 个 Handler（CreatePanel + LoadForEdit + SaveModified） |
| 注册 | `UBridgeEditorHost.cs` L125-127 | 3 个 `RegisterHandler` |
| CLI | `DotNet~/Program.cs` L114-123 | 3 个命令的 switch case |

**测试步骤：**

```bash
dotnet Bin/ET.UBridge.dll YIUICreatePanel --path "Assets/GameRes/YIUI/Test/Prefabs" --name TestPanel

# 在 Unity Project 窗口确认 TestPanel.prefab 已创建
# 选中 prefab，检查 Inspector 中有 UIBindCDETable 组件
```

**验证标准：** Project 窗口出现 `.prefab` 文件，Inspector 有 CDE Table 组件。

---

### 优先级 3：PrefabLoadForEdit + PrefabSaveModified — ✅ 已完成

**依赖：** 无。基础设施命令，加载/保存任意 prefab。

**实现文件：**

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `UBridge_C_10000.proto` L1418-1442 | PrefabLoadForEdit + PrefabSaveModified 两组 Req/Resp |
| 生成 C# | `cn.etetet.proto/CodeMode/Model/Client/UBridge_C_10000.cs` | Proto2CS 生成 |
| Handler | `Scripts/ModelView/Client/UBridgeYIUIPanelHandlers.cs` L56-89 | PrefabLoadForEdit + PrefabSaveModified |
| 注册 | `UBridgeEditorHost.cs` L126-127 | 2 个 `RegisterHandler` |
| CLI | `DotNet~/Program.cs` L120-125 | 两个独立 case（Save 需要 --instanceId） |

**实现细节：**
- `PrefabLoadForEdit`：`PrefabUtility.LoadPrefabContents(path)` → 返回根 GameObject InstanceId
- `PrefabSaveModified`：`PrefabUtility.SaveAsPrefabAsset(gameObject, path)` + `PrefabUtility.UnloadPrefabContents(go)` + `AssetDatabase.Refresh()`

**注意事项：** `PrefabSaveModified` 的 CLI 需要同时传 `--instanceId`（LoadForEdit 返回的根节点 ID）和 `--path`（prefab 路径），LoadForEdit 和 SaveModified 不能共享同一个 fall-through case。

**测试步骤：**

```bash
# 1. 创建测试 prefab（或用 YIUICreatePanel 创建）
# 2. 加载
dotnet Bin/ET.UBridge.dll PrefabLoadForEdit --path "Assets/GameRes/YIUI/Test/Prefabs/TestPanel.prefab"
# 返回根 InstanceId，如 -12345

# 3. 修改（用现有 Transform 命令或在 Unity 中手动改）
# 4. 保存
dotnet Bin/ET.UBridge.dll PrefabSaveModified --instanceId -12345 --path "Assets/GameRes/YIUI/Test/Prefabs/TestPanel.prefab"

# 5. 在 Unity 中验证修改已保存
```

**验证标准：** 加载 → 修改 → 保存后，prefab 内容在 Unity 中已更新。

---

### 优先级 4：AddControl（标准 Unity UI 控件）— ✅ 已完成 + YIUIAddControl TO DO

**依赖：** 需要一个父节点（场景 GameObject 或已加载的 prefab 根节点）。

**实现文件：**

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `UBridge_C_10000.proto` L1464-1488 | AddControlRequest/Response + YIUIAddControlRequest/Response（两条指令） |
| 生成 C# | `cn.etetet.proto/CodeMode/Model/Client/UBridge_C_10000.cs` | Proto2CS 生成 |
| Handler | `Scripts/ModelView/Client/UBridgeControlHandlers.cs` | AddControl（switch DefaultControls.CreateXxx） + YIUIAddControlHandler（TODO） |
| 注册 | `UBridgeEditorHost.cs` L129-130 | 2 个 RegisterHandler |
| CLI | `DotNet~/Program.cs` | --parentId 参数 + AddControl case |

**实现细节：**
- `AddControl`：通过 `UnityEngine.UI.DefaultControls` 创建 11 种标准控件（Button/Text/Image/RawImage/InputField/Toggle/Slider/ScrollView/Dropdown/Scrollbar/Panel），然后 SetParent + SetName
- `YIUIAddControl`：Handler 留 TODO，后续克隆 `TemplatePrefabs/YIUI/` 下的模板 prefab

**测试步骤：**

```bash
# 1. 加载 prefab（优先级 3）
dotnet Bin/ET.UBridge.dll PrefabLoadForEdit --path "Assets/GameRes/YIUI/Test/Prefabs/TestPanel.prefb"
# 返回根 InstanceId = -12345

# 2. 添加 Button
dotnet Bin/ET.UBridge.dll YIUIAddControl --parentId -12345 --name MyButton --type Button

# 3. 添加 InputField
dotnet Bin/ET.UBridge.dll YIUIAddControl --parentId -12345 --name MyInput --type InputField

# 4. 保存
dotnet Bin/ET.UBridge.dll PrefabSaveModified --id -12345 -path ".../TestPanel.prefab"

# 5. 在 Unity 中打开 prefab，验证子对象已创建
```

**验证标准：** Prefab 在 Unity 中打开后，Hierarchy 下能看到新创建的控件。

---

### 优先级 5：CDE Table 绑定（5 个：2 读 + 3 写）— ✅ 已完成

**依赖：** 需要一个已有 CDE Table 的 prefab（YIUICreatePanel 创建）+ 子控件（AddControl 添加）。

**实现文件：**

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `UBridge_C_10000.proto` | 5 组 Req/Resp + YIUIBindingInfo + YIUIEventItem |
| 生成 C# | `cn.etetet.proto/CodeMode/Model/Client/UBridge_C_10000.cs` | Proto2CS 生成 |
| Handler | `Scripts/ModelView/Client/UBridgeCDEHandlers.cs` | CDEHelper + 5 个 Handler |
| 注册 | `UBridgeEditorHost.cs` L131-136 | 5 个 RegisterHandler |
| CLI | `DotNet~/Program.cs` | --controlName/--paramTypes/--targetName + 5 个 case |

**实现细节：**
- 所有命令独立操作 prefab 文件：`LoadPrefabContents` → 操作 CDE Table → `SaveAsPrefabAsset` → `UnloadPrefabContents`
- `YIUIGetBindings`：遍历 `ComponentTable.AllBindDic`，返回绑定名+组件类型+组件名
- `YIUIGetEvents`：遍历 `EventTable.EventDic`，返回事件名+Sync/Async+参数类型
- `YIUIBindComponent`：通过 `--controlName` 查找子对象，优先匹配 Unity UI 组件（typeof 集合），调 `EditorAddComponent`
- `YIUIBindEvent`：调 `EventTable.EditorAddEvent(eventType, eventName, paramTypes)`
- `YIUIAttachEvent`：根据事件参数类型匹配 UIEventBind* 组件，`AddComponent` + 反射设 `m_EventName`；重复挂载会先移除旧的

**已测试：** All 5 commands pass.

```bash
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../TestPanel.prefab" --controlName MyButton --name u_ComMyButton
dotnet Bin/ET.UBridge.dll YIUIBindEvent --path "Assets/.../TestPanel.prefab" --name u_EventClick --type Sync --paramTypes ""
dotnet Bin/ET.UBridge.dll YIUIAttachEvent --path "Assets/.../TestPanel.prefab" --targetName MyButton --name u_EventClick
```

---

### 优先级 6：YIUIGenerateCode — ✅ 已完成

**依赖：** prefab 须已配置好 CDE Table（优先级 5 完成）。

**实现文件：**

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `UBridge_C_10000.proto` | YIUIGenerateCodeRequest/Response |
| 生成 C# | `cn.etetet.proto/CodeMode/Model/Client/UBridge_C_10000.cs` | Proto2CS 生成 |
| Handler | `Scripts/ModelView/Client/UBridgeCDEHandlers.cs` | 反射调用 `UICreateModule.CreatePackages` |
| 注册 | `UBridgeEditorHost.cs` L137 | RegisterHandler |
| CLI | `DotNet~/Program.cs` | case YIUIGenerateCode（--path=prefab, --name=package） |

**实现细节：**
- 必须用 `AssetDatabase.LoadAssetAtPath` 加载（不能使用 LoadPrefabContents），因为 `UICreateModule` 内部检查 `IsPartOfPrefabAsset`
- 反射加载 `ET.YIUIFramework.Editor` 程序集调用 `YIUIFramework.Editor.UICreateModule.CreatePackages`

**测试步骤：**

```bash
dotnet Bin/ET.UBridge.dll YIUIGenerateCode --prefabPath "Assets/.../TestPanel.prefab" --packageName lockstep

# 读取控制台检查编译错误
dotnet Bin/ET.UBridge.dll ConsoleGetLogs --count 10 --logType Error
```

**验证标准：** 4 个 `.cs` 文件生成在正确路径，Unity Console 无编译错误。

---

### 实现顺序图

```
优先级1: RectTransform(6)  ──→  可在实现后立即测试
优先级2: YIUICreatePanel       ──→  可立即测试
优先级3: PrefabLoad/Save       ──→  可立即测试
         ↓
优先级4: YIUIAddControl        ──→  需 1+3
         ↓
优先级5: CDE 绑定(3)           ──→  需 1+2+3+4
         ↓
优先级6: YIUIGenerateCode      ──→  需 1~5 全部
