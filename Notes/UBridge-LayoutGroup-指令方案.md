# UBridge LayoutGroup 及常用 UI 组件指令方案

> 目标：补齐拼 UI 时常用的布局及样式指令，模式与 RectTransform 一致（直接读/写 C# API 属性，不走 Inspector 通道）。

---

## 一、LayoutGroup 指令（3 组 × 1 读 + 1 写）✅ 已实现

### 组件类型

| 类型 | 对应 Unity 类 | 特有属性 |
|------|-------------|---------|
| `HorizontalLayoutGroup` | `UnityEngine.UI.HorizontalLayoutGroup` | 横向排列子对象 |
| `VerticalLayoutGroup` | `UnityEngine.UI.VerticalLayoutGroup` | 纵向排列子对象 |
| `GridLayoutGroup` | `UnityEngine.UI.GridLayoutGroup` | 网格排列，多 `cellSize`/`constraint` |

### 公共属性（Horizontal + Vertical 共享，Grid 少了 spacing，多了 cellSize）

| 属性 | 类型 | 说明 |
|------|------|------|
| PaddingLeft/Right/Top/Bottom | int | 内边距 |
| Spacing | float | 子对象间距（H/V 用此字段） |
| SpacingX/Y | float | 子对象间距（Grid 用此字段） |
| ChildAlignment | int | 对齐方式（TextAnchor 枚举转 int） |
| ReverseArrangement | bool | 是否反向排列（Grid 无此属性） |
| ControlChildWidth/Height | bool | 是否控制子对象宽/高（Grid 无此属性） |
| ChildForceExpandWidth/Height | bool | 是否强制扩展子对象宽/高（Grid 无此属性） |
| CellSizeX/Y (Grid 专属) | float | 单元格大小 |
| Constraint (Grid 专属) | int | 固定行数/列数（Flexible=0, FixedColumnCount=1, FixedRowCount=2） |
| ConstraintCount (Grid 专属) | int | 固定数量 |
| StartCorner (Grid 专属) | int | 起始角（UpperLeft=0, UpperRight=1, LowerLeft=2, LowerRight=3） |
| StartAxis (Grid 专属) | int | 起始轴（Horizontal=0, Vertical=1） |

### 已实现指令

**读：**

| 命令 | 参数 | 返回 |
|------|------|------|
| `LayoutGet` | `--instanceId` | 当前挂载的 LayoutGroup 类型 + 全部属性 |

**写（一个指令涵盖所有类型，按参数区分）：**

| 命令 | 关键参数 | 设什么 |
|------|---------|--------|
| `LayoutSet` | `--instanceId`, `--paddingL/R/T/B`, `--spacing`, `--spacingX/Y`, `--alignment`, `--cellSizeX/Y`, `--constraint` 等 | 匹配组件类型后逐属性赋值 |

**增/删：**

| 操作 | 已有指令 |
|------|---------|
| 增 | `InspectorAddComponent --type "UnityEngine.UI.HorizontalLayoutGroup, UnityEngine.UI"` |
| 删 | `InspectorRemoveComponent --type "HorizontalLayoutGroup"` |

### 实现文件

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `Packages/cn.etetet.ubridge/Proto/UBridgeLayout_C_55000.proto` | LayoutGetRequest/Response + LayoutSetRequest/Response |
| 生成 C# | `Packages/cn.etetet.proto/CodeMode/Model/ClientServer/UBridgeLayout_C_55000.cs` | Opcode 55001-55004 |
| Handler | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeLayoutHandlers.cs` | UBridgeLayoutGetHandler + UBridgeLayoutSetHandler |
| CLI | `Packages/cn.etetet.ubridge/DotNet~/Program.cs` | 18 个新参数 + LayoutGet/LayoutSet switch cases |
| 注册 | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeEditorHost.cs` | RegisterHandler × 2 |

### 测试结果 ✅

| 测试 | HorizontalLayoutGroup | VerticalLayoutGroup | GridLayoutGroup |
|------|:---:|:---:|:---:|
| InspectorAddComponent | ✅ | ✅ | ✅ |
| LayoutGet（默认值） | ✅ | ✅ | ✅ |
| LayoutSet | ✅ | ✅ | ✅ |
| LayoutGet（验证修改） | ✅ | ✅ | ✅ |
| InspectorRemoveComponent | ✅ | ✅ | ✅ |
| LayoutGet（确认删除，Error=3） | ✅ | ✅ | — |

### 使用示例

```bash
# 创建 GameObject
dotnet Bin/ET.UBridge.dll GameObjectCreate --name "MyLayout"

# 添加 HorizontalLayoutGroup（注意：UI 组件需 assembly-qualified name）
dotnet Bin/ET.UBridge.dll InspectorAddComponent --instanceId <id> --type "UnityEngine.UI.HorizontalLayoutGroup, UnityEngine.UI"

# 读默认值
dotnet Bin/ET.UBridge.dll LayoutGet --instanceId <id>

# 修改属性（H/V 用 --spacing；Grid 用 --spacingX --spacingY）
dotnet Bin/ET.UBridge.dll LayoutSet --instanceId <id> --spacing 10 --paddingL 5 --paddingR 5 --paddingT 5 --paddingB 5 --alignment 4

# Grid 专属参数
dotnet Bin/ET.UBridge.dll LayoutSet --instanceId <id> --spacingX 5 --spacingY 10 --cellSizeX 200 --cellSizeY 80 --constraint 1 --constraintCount 3

# 删除组件
dotnet Bin/ET.UBridge.dll InspectorRemoveComponent --instanceId <id> --type "HorizontalLayoutGroup"

# 销毁 GameObject
dotnet Bin/ET.UBridge.dll GameObjectDestroy --instanceId <id>
```

---

## 二、ContentSizeFitter 指令 ✅ 已实现

**用途：** 让 Content 根据子对象自动调整宽高。LoopScroll 的 Content 必配 `VerticalFit=Preferred`（纵向贴紧子 Item），否则 ScrollRect 不知道可滚动范围。

| 属性 | 类型 | 说明 |
|------|------|------|
| HorizontalFit | int | Unconstrained=0, MinSize=1, PreferredSize=2 |
| VerticalFit | int | 同上 |

### 已实现指令

| 命令 | 参数 | 说明 |
|------|------|------|
| `FitterGet` | `--instanceId` | 返回 HorizontalFit / VerticalFit |
| `FitterSet` | `--instanceId`, `--hFit`, `--vFit` | 设置适配模式 |

### 实现文件

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `Packages/cn.etetet.ubridge/Proto/UBridgeFitter_C_56000.proto` | FitterGet/Set Request/Response, opcode 56001-56004 |
| Handler | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeFitterHandlers.cs` | FitterGet + FitterSet Handler |
| CLI | `Packages/cn.etetet.ubridge/DotNet~/Program.cs` | `--hFit`/`--vFit` 参数 + 2 个 switch case |
| 注册 | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeEditorHost.cs` | RegisterHandler × 2 |

### 测试结果 ✅

| 操作 | 结果 |
|------|------|
| InspectorAddComponent | ✅ |
| FitterGet（默认值） | ✅ hFit=0(Unconstrained), vFit=0(Unconstrained) |
| FitterSet（LoopScroll 标配） | ✅ hFit=0, vFit=2(PreferredSize) |
| FitterGet（验证修改） | ✅ vFit: 0→2 |
| InspectorRemoveComponent | ✅ |
| FitterGet（确认删除） | ✅ Error=3 "No ContentSizeFitter" |

### 使用示例

```bash
# 查 Content 的 Fitter 当前值
dotnet Bin/ET.UBridge.dll FitterGet --instanceId <Content的InstanceId>

# 设纵向 Preferred（LoopScroll 必配）
dotnet Bin/ET.UBridge.dll FitterSet --instanceId <id> --hFit 0 --vFit 2
```

---

## 三、LayoutElement 指令 ✅ 已实现

| 属性 | 类型 | 说明 |
|------|------|------|
| MinWidth/Height | float | 最小宽高（默认 -1 = 不参与计算） |
| PreferredWidth/Height | float | 偏好宽高（默认 -1） |
| FlexibleWidth/Height | float | 弹性宽高（默认 -1） |
| IgnoreLayout | bool | 是否忽略布局 |
| LayoutPriority | int | 布局优先级（默认 1） |

### 已实现指令

| 命令 | 参数 | 说明 |
|------|------|------|
| `ElementGet` | `--instanceId` | 返回全部 LayoutElement 属性 |
| `ElementSet` | `--instanceId`, `--minW/H`, `--prefW/H`, `--flexW/H`, `--ignoreLayout`, `--layoutPriority` | 设置布局元素 |

### 实现文件

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `Packages/cn.etetet.ubridge/Proto/UBridgeElement_C_57000.proto` | ElementGet/Set Request/Response, opcode 57001-57004 |
| Handler | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeElementHandlers.cs` | ElementGet + ElementSet Handler |
| CLI | `Packages/cn.etetet.ubridge/DotNet~/Program.cs` | `--minW/H`, `--prefW/H`, `--flexW/H`, `--ignoreLayout`, `--layoutPriority` + 2 switch cases |
| 注册 | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeEditorHost.cs` | RegisterHandler × 2 |

### 测试结果 ✅

| 操作 | 结果 |
|------|------|
| InspectorAddComponent | ✅ |
| ElementGet（默认值） | ✅ min/pref/flex=-1, ignoreLayout=false, priority=1 |
| ElementSet | ✅ min=100x50, pref=200x100, flex=1x1 |
| ElementGet（验证修改） | ✅ 全部一致 |
| InspectorRemoveComponent | ✅ |
| ElementGet（确认删除） | ✅ Error=3 "No LayoutElement" |

### 使用示例

```bash
dotnet Bin/ET.UBridge.dll ElementGet --instanceId <id>
dotnet Bin/ET.UBridge.dll ElementSet --instanceId <id> --minW 100 --minH 50 --prefW 200 --prefH 100 --flexW 1 --flexH 1
```

---

## 四、Image 渲染属性指令 ✅ 已实现

`AddControl` 能创建 Image，`ImageGet`/`ImageSet` 调整外观：

| 属性 | 类型 | 说明 |
|------|------|------|
| Sprite | string | 精灵路径（AssetDatabase 加载） |
| ColorR/G/B/A | double | RGBA 颜色（0-1） |
| ImageType | int | Simple=0, Sliced=1, Tiled=2, Filled=3 |
| FillAmount | double | 填充比例（Type=Filled 时生效） |
| FillMethod | int | 填充方向（Horizontal=0, Vertical=1, Radial90=2, Radial180=3, Radial360=4） |
| RaycastTarget | bool | 是否响应射线 |
| PreserveAspect | bool | 是否保持宽高比 |

### 已实现指令

| 命令 | 参数 | 说明 |
|------|------|------|
| `ImageGet` | `--instanceId` | 返回 Image 全部属性 |
| `ImageSet` | `--instanceId`, `--colorR/G/B/A`, `--sprite`, `--imageType`, `--fillAmount` 等 | 设置渲染属性 |

### 实现文件

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `Packages/cn.etetet.ubridge/Proto/UBridgeImage_C_58000.proto` | ImageGet/Set Request/Response, opcode 58001-58004 |
| Handler | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeImageHandlers.cs` | ImageGet + ImageSet Handler |
| CLI | `Packages/cn.etetet.ubridge/DotNet~/Program.cs` | `--colorR/G/B/A`, `--sprite`, `--imageType`, `--fillAmount`, `--fillMethod`, `--raycastTarget`, `--preserveAspect` |

### 测试结果 ✅

| 操作 | 结果 |
|------|------|
| AddControl --type Image | ✅ |
| ImageGet（默认值） | ✅ color=(1,1,1,1), type=Simple, fill=1, raycast=true |
| ImageSet | ✅ color=(1,0,0,0.5), fill=0.8, raycast=false, preserveAspect=true |
| ImageGet（验证修改） | ✅ 全部一致 |
| InspectorRemoveComponent | ✅ |
| ImageGet（确认删除） | ✅ Error=3 "No Image" |

### 使用示例

```bash
dotnet Bin/ET.UBridge.dll ImageGet --instanceId <id>
dotnet Bin/ET.UBridge.dll ImageSet --instanceId <id> --colorR 1 --colorG 0 --colorB 0 --colorA 0.5 --fillAmount 0.8 --raycastTarget false
```

---

## 五、Text 渲染属性指令 ✅ 已实现

| 属性 | 类型 | 说明 |
|------|------|------|
| Text | string | 文本内容 |
| FontSize | int | 字号 |
| FontStyle | int | Normal=0, Bold=1, Italic=2, BoldAndItalic=3 |
| Alignment | int | TextAnchor 枚举（同 LayoutGroup） |
| ColorR/G/B/A | double | RGBA 颜色（0-1） |
| BestFit | bool | 自适应字号 |
| RaycastTarget | bool | 是否响应射线 |

### 已实现指令

| 命令 | 参数 | 说明 |
|------|------|------|
| `TextGet` | `--instanceId` | 返回 Text 全部属性 |
| `TextSet` | `--instanceId`, `--text`, `--fontSize`, `--fontStyle`, `--alignment`, `--colorR/G/B/A`, `--bestFit`, `--raycastTarget` | 设置文本 |

### 实现文件

| 层 | 文件 | 说明 |
|----|------|------|
| Proto | `Packages/cn.etetet.ubridge/Proto/UBridgeText_C_59000.proto` | TextGet/Set Request/Response, opcode 59001-59004 |
| Handler | `Packages/cn.etetet.ubridge/Scripts/Editor/UBridgeTextHandlers.cs` | TextGet + TextSet Handler |
| CLI | `Packages/cn.etetet.ubridge/DotNet~/Program.cs` | `--text`, `--fontSize`, `--fontStyle`, `--alignment`, `--colorR/G/B/A`, `--bestFit`, `--raycastTarget` |

### 测试结果 ✅

| 操作 | 结果 |
|------|------|
| AddControl --type Text | ✅ |
| TextGet（默认值） | ✅ "New Text", size=14, style=Normal, color=(0.2,0.2,0.2,1) |
| TextSet | ✅ "Hello World", size=24, Bold, color=(0,0.5,1,1) |
| TextGet（验证修改） | ✅ 全部一致 |
| InspectorRemoveComponent | ✅ |
| TextGet（确认删除） | ✅ Error=3 "No Text" |

### 使用示例

```bash
dotnet Bin/ET.UBridge.dll TextGet --instanceId <id>
dotnet Bin/ET.UBridge.dll TextSet --instanceId <id> --text "Hello World" --fontSize 24 --fontStyle 1 --colorR 0 --colorG 0.5 --colorB 1 --colorA 1
```

---

## 六、YIUI 专有脚本（已有/缺失）✅ 已完成

| 脚本 | 当前状态 |
|------|---------|
| `UIBindCDETable` | ✅ 通过 YIUI Create Panel/Common/View 创建 + CDE 绑定指令 |
| `UIBlock` | ✅ `InspectorAddComponent --type "YIUIFramework.UIBlock, ET.YIUIFramework"` + `InspectorRemoveComponent --type "UIBlock"`。无自定义属性（纯射线阻挡层），无需专用读写指令 |
| `YIUIClickEffect` | ⏸️ 暂不实现。通过 `InspectorAddComponent`/`InspectorRemoveComponent` + `InspectorGetProperties`/`InspectorSetProperty` 已覆盖 |
| `UIEventBind*` | ✅ `YIUIAttachEvent` 挂载。**已修复**：支持 `--triggerType ClickDown/ClickUp/Click` + Task 异步事件类型映射 |

### EventBind 修复详情（2026-07-27）

**改动**：
- `Proto/UBridgeYIUI_C_54000.proto`：`YIUIAttachEventRequest` 加 `string EventTriggerType = 94`
- `Scripts/Editor/UBridgeCDEHandlers.cs`：重写 `GetBindComponentType(paramTypes, isTaskEvent, triggerType)`，三维映射表
- `DotNet~/Program.cs`：加 `--triggerType` CLI 参数

**映射表**：

| triggerType | Sync | Async（Task） |
|-------------|------|:---:|
| `Click`（默认） | UIEventBindClick | UITaskEventBindClick |
| `Click` + Int 参数 | UIEventBindClickInt | UITaskEventBindClickInt |
| `Click` + String 参数 | UIEventBindClickString | UITaskEventBindClickString |
| `Click` + Object 参数 | UIEventBindClickPointerEventData | UITaskEventBindClickPointerEventData |
| `ClickDown` | UIEventBindClickDown | — |
| `ClickUp` | UIEventBindClickUp | — |

**CLI 用法**：
```bash
# 按下事件
YIUIBindEvent --path "..." --name u_EventDown --type Sync
YIUIAttachEvent --path "..." --targetName Btn_Test --name u_EventDown --triggerType ClickDown

# 异步点击
YIUIBindEvent --path "..." --name u_EventAsyncClick --type Async
YIUIAttachEvent --path "..." --targetName Btn_Test --name u_EventAsyncClick --triggerType Click
```

---

## 七、实现优先级

| 优先级 | 组 | 状态 |
|--------|----|------|
| **P1** | LayoutGroup（1 读 + 1 写） | ✅ 已完成 |
| **P2** | ContentSizeFitter | ✅ 已完成 |
| **P3** | LayoutElement | ✅ 已完成 |
| **P4** | Image / Text 渲染属性 | ✅ 已完成 |

---

## 八、实现模式（跟 RectTransform 一样）

**Proto：** 1 组 Request/Response pairs，字段编号从 90 起（RpcId=90, Error=91, Message=92, 数据 93+）

**Handler：**
```csharp
var lg = EditorUtility.InstanceIDToObject(id) as GameObject;
var layout = lg?.GetComponent<HorizontalLayoutGroup>(); // 或 Vertical/Grid
if (!layout) { resp.Error = 3; ... }
resp.PaddingLeft = layout.padding.left;
resp.Spacing = layout.spacing;
// ... 写同理，直接赋值
```

**CLI：** 添加 `--spacing` / `--paddingL` 等参数

**Proto 生成：**
```bash
dotnet Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll
```

---

## 九、关键上下文（新会话快速上手）

### 参考模板

- Handler 写法：参考 `UBridgeRectHandlers.cs`（`Scripts/Editor/` 下，跟 RectTransform 完全一致的模式）
- Proto 生成流程：`UBridge_C_50000.proto` 等 → `dotnet Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll` → 生成 C# 代码（Client/Server/ClientServer 三份拷贝）
- CLI 写法：参考 `DotNet~/Program.cs` 中 RectTransform 的参数解析 + switch case
- 注册 Handler：在 `UBridgeEditorHost.cs` 的 `EnsureInitialized()` 中加 `RegisterHandler`

### 程序集分布

- Proto：`Packages/cn.etetet.ubridge/Proto/` → 命名规则 `{Name}_{C|S}_{OpcodeStart}.proto`
- 生成 C#：`Packages/cn.etetet.proto/CodeMode/Model/ClientServer/`（自动生成，勿手动改）
- Handler：`Packages/cn.etetet.ubridge/Scripts/Editor/`（asmref → ET.Editor）
- CLI：`Packages/cn.etetet.ubridge/DotNet~/Program.cs`

### 常用 proto 消息模板

```proto
// ResponseType LayoutGetResponse
message LayoutGetRequest // IRequest
{
    int32 RpcId = 90;
    int32 InstanceId = 91;
}
message LayoutGetResponse // IResponse
{
    int32 RpcId = 90;
    int32 Error = 91;
    string Message = 92;
    string Type = 93;
    int32 PaddingLeft = 94;
    // ...
}
```

### 构建与测试

```bash
dotnet build ET.sln
# CLI 测试（需要 Unity Editor 打开）
dotnet Bin/ET.UBridge.dll LayoutGet --instanceId <id>
dotnet Bin/ET.UBridge.dll LayoutSet --instanceId <id> --spacing 10
```

### 已知坑（详见 README.md § 已知坑）

- Proto 生成后需新建 `.csproj` 条目（或 Unity 刷新自动生成）
- Static 字段需 `[StaticField]`
- ET.Model 的 defineConstraints 已加 `IS_COMPILING || UNITY_EDITOR`
- Play 模式 FileNotFound 已通过 `playModeStateChanged` 修复
- **UI 组件（UnityEngine.UI）需 assembly-qualified name**：`"UnityEngine.UI.Xxx, UnityEngine.UI"`。InspectorAddComponent 的类型解析只尝试 `name` → `name, UnityEngine` → `UnityEngine.name, UnityEngine`，不覆盖 `UnityEngine.UI` 命名空间
- **InspectorAddComponent 和 InspectorRemoveComponent 的 proto 字段不同**：前者用 `TypeName`（field 95），后者用 `ComponentName`（field 95）。CLI 已拆分两个 switch case 分别构造 payload
- **BSON float 截断（TruncationException）**：MongoDB.Bson 库将 JSON 数字一律当 double 解析，`RepresentationConverter.ToSingle` 转换 float 时检测到精度损失即抛异常。非二进制精确值（0.8、0.3 等）会触发，整数和 0.5 这种值不触发。**修复**：proto 中所有 float 字段改为 double，Handler 中 `(float)r.Xxx` 显式截断

### 本次实现额外修复

- **拆分 InspectorAddComponent / InspectorRemoveComponent CLI**：之前共享同一 payload 同时发送 `TypeName` 和 `ComponentName`，但 proto 各只定义一个字段，BSON 严格模式反序列化失败
- **新增 CLI 支持**：`GameObjectCreate`、`GameObjectDestroy`、`GameObjectFind` 三个命令（Handler 早已注册，但 CLI switch case 缺失，导致无法传参）
- **Proto float → double**：5 个 proto（Layout/Fitter/Element/Image/Text）的浮点字段全部改为 double，根除 BSON float 截断异常

---

## 十、下一步

- [x] P2: ContentSizeFitter 指令（FitterGet + FitterSet）
- [x] P3: LayoutElement 指令（ElementGet + ElementSet）
- [x] P4: Image / Text 渲染属性指令
