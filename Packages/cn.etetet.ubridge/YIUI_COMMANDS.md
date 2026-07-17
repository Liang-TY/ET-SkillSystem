# YIUI 指令文档

> 通过 UBridge CLI 完成 YIUI Panel 创建 → 添加控件 → 布局 → CDE 绑定 → 代码生成的完整自动化流程

---

## 指令总览

| 步骤 | 命令 | 数量 | 说明 |
|------|------|------|------|
| 创建 Prefab | `YIUICreatePanel`, `PrefabLoadForEdit`, `PrefabSaveModified` | 3 | 创建/加载/保存 prefab |
| 添加控件 | `AddControl` | 1 | 创建 Unity 标准 UI 控件 |
| 调整布局 | `RectGet`, `RectSetAnchor/Size/Pos/Pivot/Rotation/Scale` | 7 | 读写 RectTransform |
| CDE 绑定 | `YIUIGetBindings`, `YIUIGetEvents`, `YIUIBindComponent`, `YIUIBindEvent` | 4 | 读/写 CDE Table |
| 挂载事件 | `YIUIAttachEvent` | 1 | 将事件挂载到控件 |
| 生成代码 | `YIUIGenerateCode` | 1 | 反射调用 UICreateModule 生成 .cs |
| **合计** | | **17** | |

---

## 一、创建 Prefab（3 条）

### YIUICreatePanel

创建 YIUI Panel 预制体。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体保存文件夹路径（须在 `GameRes/YIUI` 下） |
| `--name` | string | ✅ | Panel 名称 |

```bash
dotnet Bin/ET.UBridge.dll YIUICreatePanel --path "Assets/GameRes/YIUI/YIUI/Prefabs" --name AuthPanel
```

**行为：** 创建 Panel GameObject → 添加 `UIBindCDETable` + `CanvasRenderer` + `UIBlockBG` → `PrefabUtility.SaveAsPrefabAsset`。

**返回：** `PrefabPath`

---

### PrefabLoadForEdit

加载预制体到预览场景，返回根节点 InstanceId（供后续 `AddControl`、`RectSet*` 使用父节点/控件 ID）。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |

```bash
dotnet Bin/ET.UBridge.dll PrefabLoadForEdit --path "Assets/GameRes/YIUI/YIUI/Prefabs/AuthPanel.prefab"
```

**返回：** `RootInstanceId`

---

### PrefabSaveModified

保存已加载的预制体内容。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--instanceId` | int | ✅ | LoadForEdit 返回的根节点 ID |
| `--path` | string | ✅ | 预制体路径 |

```bash
dotnet Bin/ET.UBridge.dll PrefabSaveModified --instanceId 46100 --path "Assets/GameRes/YIUI/YIUI/Prefabs/AuthPanel.prefab"
```

---

## 二、添加控件（1 条）

### AddControl

通过 `UnityEngine.UI.DefaultControls` 创建标准 Unity UI 控件，添加到指定父节点。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--parentId` | int | ✅ | 父节点 InstanceId |
| `--name` | string | ❌ | 控件显示名 |
| `--type` | string | ✅ | 控件类型（见下表） |

**支持的 `--type` 值：**

| type | 创建控件 | 包含子对象 |
|------|----------|-----------|
| `Button` | Button | + Image + 子 Text |
| `Text` | Text (Legacy) | — |
| `Image` | Image | — |
| `RawImage` | RawImage | — |
| `InputField` | InputField | + Placeholder + Text |
| `Toggle` | Toggle | + Background + Checkmark + Label |
| `Slider` | Slider | + Background + Fill + Handle |
| `ScrollView` | ScrollView | + Viewport + Content |
| `Dropdown` | Dropdown | + Template + Scrollbar |
| `Scrollbar` | Scrollbar | + Sliding Area + Handle |
| `Panel` | Image（全屏拉伸） | — |

```bash
# 先加载预制体获取 RootInstanceId
dotnet Bin/ET.UBridge.dll PrefabLoadForEdit --path "Assets/.../AuthPanel.prefab"
# → RootInstanceId = 46100

# 添加控件
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name BtnLogin --type Button
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name InputAccount --type InputField
```

**返回：** 新控件的 `InstanceId`

---

## 三、调整布局（7 条：1 读 + 6 写）

### RectGet（读）

读取指定 GameObject 的 RectTransform 全部属性（18 个字段）。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--instanceId` | int | ✅ | GameObject InstanceId |

```bash
dotnet Bin/ET.UBridge.dll RectGet --instanceId -17032
```

**返回字段：** `AnchorMinX/Y`, `AnchorMaxX/Y`, `SizeDeltaX/Y`, `AnchoredPosX/Y`, `PivotX/Y`, `LocalRotX/Y/Z`, `LocalScaleX/Y/Z`

---

### RectSetAnchor / RectSetSize / RectSetPos / RectSetPivot / RectSetRotation / RectSetScale（写）

分别设置 RectTransform 的 6 个属性组。

| 命令 | 参数 | 对应属性 |
|------|------|----------|
| `RectSetAnchor` | `--instanceId`, `--minX`, `--minY`, `--maxX`, `--maxY` | `anchorMin`, `anchorMax` |
| `RectSetSize` | `--instanceId`, `--rectWidth`, `--rectHeight` | `sizeDelta` |
| `RectSetPos` | `--instanceId`, `--posX`, `--posY` | `anchoredPosition` |
| `RectSetPivot` | `--instanceId`, `--pivotX`, `--pivotY` | `pivot` |
| `RectSetRotation` | `--instanceId`, `--rotX`, `--rotY`, `--rotZ` | `localRotation` |
| `RectSetScale` | `--instanceId`, `--scaleX`, `--scaleY`, `--scaleZ` | `localScale` |

```bash
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId -17032 --rectWidth 200 --rectHeight 40
dotnet Bin/ET.UBridge.dll RectSetPos --instanceId -17032 --posX 0 --posY -110
```

**注意：** InstanceId 仅在 LoadForEdit → SaveModified 之间有效。保存后 ID 失效。

---

## 四、CDE Table 绑定（4 条：2 读 + 2 写）

所有 CDE 命令直接操作 `.prefab` 文件，内部自动 `LoadPrefabContents → 操作 → SaveAsPrefabAsset → UnloadPrefabContents`。

### YIUIGetBindings（读）

读取 CDE Table 中所有组件绑定。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |

```bash
dotnet Bin/ET.UBridge.dll YIUIGetBindings --path "Assets/.../AuthPanel.prefab"
```

**返回：** `Bindings[]` 每项含 `Name`（绑定名）、`ComponentType`、`ComponentName`

---

### YIUIGetEvents（读）

读取 CDE EventTable 中所有事件定义。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |

```bash
dotnet Bin/ET.UBridge.dll YIUIGetEvents --path "Assets/.../AuthPanel.prefab"
```

**返回：** `Events[]` 每项含 `EventName`、`EventType`（Sync/Async）、`ParamTypes`

---

### YIUIBindComponent（写）

将子控件绑定到 CDE ComponentTable。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |
| `--controlName` | string | ✅ | 子控件 GameObject 名称（非 InstanceId） |
| `--name` | string | ✅ | 绑定名，如 `u_ComBtnLogin` |

```bash
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName BtnLogin --name u_ComBtnLogin
```

**组件匹配优先级：** 优先匹配 Unity UI 组件（`typeof(Button)`, `typeof(Text)`, `typeof(Image)` 等集合），找不到则 fallback 第一个非 Transform 组件。

---

### YIUIBindEvent（写）

在 CDE EventTable 中创建事件定义。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |
| `--name` | string | ✅ | 事件名，如 `u_EventClickLogin` |
| `--type` | string | ❌ | `Sync`（同步）或 `Async`（异步），默认 Async |
| `--paramTypes` | string | ❌ | 逗号分隔参数类型：`Bool`, `Int`, `String`, `Float`, `Object`, `ParamVo`，空字符串表示无参 |

```bash
dotnet Bin/ET.UBridge.dll YIUIBindEvent --path "Assets/.../AuthPanel.prefab" --name u_EventClickLogin --type Sync --paramTypes ""
```

---

## 五、挂载事件（1 条）

### YIUIAttachEvent

将事件挂载到指定控件（添加 `UIEventBindClick` 等组件并设置 EventName）。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |
| `--targetName` | string | ✅ | 目标控件 GameObject 名称 |
| `--name` | string | ✅ | 事件名（须已在 EventTable 中存在） |

```bash
dotnet Bin/ET.UBridge.dll YIUIAttachEvent --path "Assets/.../AuthPanel.prefab" --targetName BtnLogin --name u_EventClickLogin
```

**重复挂载处理：** 如目标已有同名事件的 `UIEventBind*` 组件，先移除旧的再挂载新的。

**EventBind 类型映射（按事件首参数）：**

| 首参数 | 挂载组件 |
|--------|---------|
| 无参 | `UIEventBindClick` |
| `Int` | `UIEventBindClickInt` |
| `String` | `UIEventBindClickString` |
| `Object` / `ParamVo` | `UIEventBindClickPointerEventData` |
| `Bool` / `Float` 等 | `UIEventBindClick`（fallback） |

---

## 六、生成代码（1 条）

### YIUIGenerateCode

反射调用 `YIUIFramework.Editor.UICreateModule.CreatePackages`，根据 CDE Table 绑定生成 4 个 `.cs` 文件。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |
| `--name` | string | ✅ | 目标包名（如 `lockstep`） |

```bash
dotnet Bin/ET.UBridge.dll YIUIGenerateCode --path "Assets/.../AuthPanel.prefab" --name lockstep
```

**生成产物：**
- `Packages/{pkg}/Scripts/ModelView/Client/YIUIComponent/YIUI/{Name}Component.cs`
- `Packages/{pkg}/Scripts/ModelView/Client/YIUIGen/YIUI/{Name}ComponentGen.cs`
- `Packages/{pkg}/Scripts/HotfixView/Client/YIUISystem/YIUI/{Name}ComponentSystem.cs`
- `Packages/{pkg}/Scripts/HotfixView/Client/YIUIGen/YIUI/{Name}ComponentSystemGen.cs`

---

## 完整工作流示例

```bash
# 1. 创建预制体
dotnet Bin/ET.UBridge.dll YIUICreatePanel --path "Assets/GameRes/YIUI/YIUI/Prefabs" --name AuthPanel

# 2. 加载预制体
dotnet Bin/ET.UBridge.dll PrefabLoadForEdit --path "Assets/.../AuthPanel.prefab"
# → RootInstanceId = 46100

# 3. 添加控件
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name BtnLogin --type Button
# → InstanceId = -17032
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name InputAccount --type InputField
# → InstanceId = -17056
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name InputPassword --type InputField
# → InstanceId = -17092

# 4. 调整布局
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId -17056 --rectWidth 300 --rectHeight 30
dotnet Bin/ET.UBridge.dll RectSetPos --instanceId -17056 --posX 0 --posY -20
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId -17092 --rectWidth 300 --rectHeight 30
dotnet Bin/ET.UBridge.dll RectSetPos --instanceId -17092 --posX 0 --posY -60
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId -17032 --rectWidth 200 --rectHeight 40
dotnet Bin/ET.UBridge.dll RectSetPos --instanceId -17032 --posX 0 --posY -110

# 5. 保存
dotnet Bin/ET.UBridge.dll PrefabSaveModified --instanceId 46100 --path "Assets/.../AuthPanel.prefab"

# 6. CDE 绑定
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName BtnLogin --name u_ComBtnLogin
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName InputAccount --name u_ComInputAccount
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName InputPassword --name u_ComInputPassword
dotnet Bin/ET.UBridge.dll YIUIBindEvent --path "Assets/.../AuthPanel.prefab" --name u_EventClickLogin --type Sync --paramTypes ""
dotnet Bin/ET.UBridge.dll YIUIAttachEvent --path "Assets/.../AuthPanel.prefab" --targetName BtnLogin --name u_EventClickLogin

# 7. 生成代码
dotnet Bin/ET.UBridge.dll YIUIGenerateCode --path "Assets/.../AuthPanel.prefab" --name lockstep
```

---

## TODO

| 命令 | 状态 | 说明 |
|------|------|------|
| `YIUIAddControl` | ❌ 未实现 | 克隆 `TemplatePrefabs/YIUI/` 下的 YIUI 模板 prefab 创建控件，Handler 仅返回 TODO 错误 |
