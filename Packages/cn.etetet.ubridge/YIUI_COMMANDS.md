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
| 清空 CDE | `YIUIClearBindings` | 1 | 清空 C/E 表（反射） |
| 生成代码 | `YIUIGenerateCode` | 1 | 反射调用 UICreateModule 生成 .cs |
| **合计** | | **18** | |

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
**创建后自动按类型加前缀重命名**，供 `YIUIBindComponent` 后续精确绑定。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--parentId` | int | ✅ | 父节点 InstanceId |
| `--name` | string | ❌ | 控件名称（自动加前缀，见下表） |
| `--type` | string | ✅ | 控件类型（见下表） |

**自动命名规则：** 按下划线拆分 `--name` → 首段等于该类型前缀则保留原名，否则加 `前缀_`。

| type | 前缀 | `--name Login` → | `--name Btn_Login` → | 包含子对象 |
|------|------|-------------------|----------------------|-----------|
| `Button` | `Btn` | `Btn_Login` | `Btn_Login` | + Image + 子 Text |
| `Text` | `Txt` | `Txt_Login` | `Txt_Login` | — |
| `Image` | `Img` | `Img_Login` | `Img_Login` | — |
| `RawImage` | `RawImg` | `RawImg_Login` | `RawImg_Login` | — |
| `InputField` | `Input` | `Input_Login` | `Input_Login` | + Placeholder + Text |
| `Toggle` | `Tog` | `Tog_Login` | `Tog_Login` | + Background + Checkmark + Label |
| `Slider` | `Sld` | `Sld_Login` | `Sld_Login` | + Background + Fill + Handle |
| `ScrollView` | `Scroll` | `Scroll_Login` | `Scroll_Login` | + Viewport + Content |
| `Dropdown` | `Drop` | `Drop_Login` | `Drop_Login` | + Template + Scrollbar |
| `Scrollbar` | `Bar` | `Bar_Login` | `Bar_Login` | + Sliding Area + Handle |
| `Panel` | `Panel` | `Panel_Login` | `Panel_Login` | Image（全屏拉伸） |

```bash
# 添加控件（自动加前缀）
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name Login --type Button
# → 实际命名: Btn_Login
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name Account --type InputField
# → 实际命名: Input_Account
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
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName Btn_Login --name u_ComBtnLogin
```

**组件解析规则（两级）：**

1. **前缀精确匹配：** 按 `_` 拆分 `controlName` 取首段 → 查前缀映射表 → `GetComponent<目标类型>()`

| 前缀 | 目标组件 |
|------|---------|
| `Btn` | `Button` |
| `Txt` | `Text` |
| `Img` | `Image` |
| `RawImg` | `RawImage` |
| `Input` | `InputField` |
| `Tog` | `Toggle` |
| `Sld` | `Slider` |
| `Scroll` | `ScrollRect` |
| `Drop` | `Dropdown` |
| `Bar` | `Scrollbar` |
| `Panel` | `Image` |

2. **默认 fallback：** 前缀未命中 → 遍历所有 Component，跳过 `Transform`/`CanvasRenderer`，取第一个剩余组件。

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

## 五、清空 CDE 表（1 条）

### YIUIClearBindings

清空 CDE Table 的 C 表（组件绑定）或 E 表（事件定义），按 `--type` 区分。

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `--path` | string | ✅ | 预制体路径 |
| `--type` | string | ✅ | `C`（组件表）/ `E`（事件表）/ `All`（全部） |

```bash
dotnet Bin/ET.UBridge.dll YIUIClearBindings --path "Assets/.../TestPanel.prefab" --type C
# → "ComponentTable cleared (2 entries)"

dotnet Bin/ET.UBridge.dll YIUIClearBindings --path "Assets/.../TestPanel.prefab" --type All
# → "Cleared: ComponentTable (2) + EventTable (1)"
```

**实现：** 反射清 `UIBindComponentTable.m_AllBindPair` + 调 `AutoCheck()`；反射清 `UIBindEventTable.m_EventDic`。

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

# 3. 添加控件（自动加前缀）
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name Login --type Button
# → 实际命名: Btn_Login, InstanceId = -17032
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name Account --type InputField
# → 实际命名: Input_Account, InstanceId = -17056
dotnet Bin/ET.UBridge.dll AddControl --parentId 46100 --name Password --type InputField
# → 实际命名: Input_Password, InstanceId = -17092

# 4. 调整布局
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId -17056 --rectWidth 300 --rectHeight 30
dotnet Bin/ET.UBridge.dll RectSetPos --instanceId -17056 --posX 0 --posY -20
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId -17092 --rectWidth 300 --rectHeight 30
dotnet Bin/ET.UBridge.dll RectSetPos --instanceId -17092 --posX 0 --posY -60
dotnet Bin/ET.UBridge.dll RectSetSize --instanceId -17032 --rectWidth 200 --rectHeight 40
dotnet Bin/ET.UBridge.dll RectSetPos --instanceId -17032 --posX 0 --posY -110

# 5. 保存
dotnet Bin/ET.UBridge.dll PrefabSaveModified --instanceId 46100 --path "Assets/.../AuthPanel.prefab"

# 6. CDE 绑定（controlName 用前缀名）
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName Btn_Login --name u_ComBtnLogin
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName Input_Account --name u_ComInputAccount
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../AuthPanel.prefab" --controlName Input_Password --name u_ComInputPassword
dotnet Bin/ET.UBridge.dll YIUIBindEvent --path "Assets/.../AuthPanel.prefab" --name u_EventClickLogin --type Sync --paramTypes ""
dotnet Bin/ET.UBridge.dll YIUIAttachEvent --path "Assets/.../AuthPanel.prefab" --targetName Btn_Login --name u_EventClickLogin

# 7. 生成代码
dotnet Bin/ET.UBridge.dll YIUIGenerateCode --path "Assets/.../AuthPanel.prefab" --name lockstep
```

---

## TODO

| 命令 | 状态 | 说明 |
|------|------|------|
| `YIUIAddControl` | ❌ 未实现 | 克隆 `TemplatePrefabs/YIUI/` 下的 YIUI 模板 prefab 创建控件，Handler 仅返回 TODO 错误 |
