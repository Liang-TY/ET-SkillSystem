# YIUI LoopScrollRect 无限循环列表（异步）

> YIUI 框架的无限循环列表控件，通过复用 Item 实现高性能滚动。
> 文档：https://lib9kmxvq7k.feishu.cn/wiki/HPbwwkhsKi9aDik5VEXcqPhDnIh

## 提供的模板

Hierarchy 右键 → **YIUI → LoopScroll**，6 种模板可选：

| 模板 | 方向 | 说明 |
|------|------|------|
| `LoopScrollVertical` | ↓ 纵向 | 标准纵向循环列表 |
| `LoopScrollVerticalReverse` | ↑ 纵向反向 | 从底部开始 |
| `LoopScrollVerticalGroup` | ↓ 纵向分组 | 多列 Grid |
| `LoopScrollHorizontal` | → 横向 | 标准横向循环列表 |
| `LoopScrollHorizontalReverse` | ← 横向反向 | 从右侧开始 |
| `LoopScrollHorizontalGroup` | → 横向分组 | 多行 Grid |

每个模板都是完整预制体，自带 `LoopScrollRect` 组件 + `RectTransform` + `Image`（遮罩）+ `Viewport/Content` 层级。

---

## 在 Panel 中添加列表

### 方式一：Unity 手动操作

1. 双击 Panel prefab 进入编辑
2. Hierarchy 选中根节点 → 右键 → **YIUI → LoopScroll → Vertical**（或 Horizontal）
3. 调整位置大小（RectTransform 的 anchor、sizeDelta 等）
4. CDE Table 中绑定刚创建的 LoopScroll 对象，命名如 `u_ComLoopScrollVertical`

### 方式二：UBridge CLI（未来支持）

```
// TODO: 等 YIUIAddControl 完善后可用
dotnet Bin/ET.UBridge.dll YIUIAddControl --parentId <rootId> --type LoopScrollVertical
```

---

## 创建列表 Item

> Item 是 **Common 组件**（`EUICodeType.Common`），不是 Panel。
> 与 Panel 的区别：无 `TimeCache`、无全屏 RectTransform、无 `UIBlockBG` 子对象。

### 第 1 步：创建 Item Prefab（Common）

**UBridge 方式（待实现）：**

```bash
# TODO: YIUICreateCommon 指令尚未实现，目前需手动创建
dotnet Bin/ET.UBridge.dll YIUICreateCommon --path "Assets/GameRes/YIUI/YourModule/Prefabs" --name YourItem
```

**手动方式：** 在 Project 面板，右键目标文件夹 → **YIUI → Create UICommon**。

### 第 2 步：给 Item 添加控件

```bash
# 加载 Item prefab
dotnet Bin/ET.UBridge.dll PrefabLoadForEdit --path "Assets/.../YourItem.prefab"
# → RootInstanceId

# 添加需要的 UI 控件（自动加前缀）
dotnet Bin/ET.UBridge.dll AddControl --parentId <root> --name Title --type Text
dotnet Bin/ET.UBridge.dll AddControl --parentId <root> --name Icon --type Image

# 保存
dotnet Bin/ET.UBridge.dll PrefabSaveModified --instanceId <root> --path "Assets/.../YourItem.prefab"
```

### 第 3 步：CDE 绑定

```bash
# 绑定控件到 CDE Table
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../YourItem.prefab" --controlName Txt_Title --name u_DataTitle
dotnet Bin/ET.UBridge.dll YIUIBindComponent --path "Assets/.../YourItem.prefab" --controlName Img_Icon --name u_DataIcon

# 创建点击事件（如需）
dotnet Bin/ET.UBridge.dll YIUIBindEvent --path "Assets/.../YourItem.prefab" --name u_EventSelect --type Sync --paramTypes ""
dotnet Bin/ET.UBridge.dll YIUIAttachEvent --path "Assets/.../YourItem.prefab" --targetName Txt_Title --name u_EventSelect
```

### 第 4 步：生成代码

```bash
dotnet Bin/ET.UBridge.dll YIUIGenerateCode --path "Assets/.../YourItem.prefab" --name <packageName>
```

---

## 运行时配置

### Panel 侧（包含列表的界面）

在 Panel 的 `YIUIInitialize` 中创建 `YIUILoopScrollChild`：

```csharp
[EntitySystem]
private static void YIUIInitialize(this YourPanelComponent self)
{
    self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type, string>(
        self.u_ComLoopScrollVertical,          // CDE 绑定的 LoopScrollRect 对象
        typeof(YourItemComponent),             // Item 的 Component 类型
        "u_EventSelect"                        // Item 被点击时触发的事件名（可选）
    );
}
```

在 `YIUIOpen` 中设置数据：

```csharp
[EntitySystem]
private static async ETTask<bool> YIUIOpen(this YourPanelComponent self)
{
    await ETTask.CompletedTask;

    var dataList = new List<YourDataType>();
    // ... 填充数据 ...

    self.Loop.ClearSelect();
    self.Loop.SetDataRefresh(dataList, 0).NoContext();
    return true;
}
```

### 数据渲染回调

```csharp
// 每帧渲染 Item 时回调
[EntitySystem]
private static void YIUILoopRenderer(this YourPanelComponent self, YourItemComponent item, YourDataType data, int index, bool select)
{
    item.u_DataTitle.SetValue(data.Title);
    item.u_DataIcon.SetValue(data.Icon);
}
```

### Item 点击回调（可选）

```csharp
[EntitySystem]
private static void YIUILoopOnClick(this YourPanelComponent self, YourItemComponent item, YourDataType data, int index, bool select)
{
    item.u_DataSelect.SetValue(select);
    Log.Debug($"Item {index} clicked, selected={select}");
}
```

---

## 关键概念

| 概念 | 说明 |
|------|------|
| `YIUILoopScrollChild` | 列表管理器 Entity，负责 Item 池、数据刷新、渲染调度 |
| `IYIUILoopScrollPrefabAsyncSource` | Item prefab 的异步加载接口（`YIUILoopScrollChild` 已实现） |
| `IYIUILoopScrollDataSource` | 数据刷新接口（`SetDataRefresh`） |
| `YIUILoopRenderer` | 渲染回调，在 `ProvideData` 时被调用，更新 Item UI |
| `YIUILoopOnClick` | 点击回调，Item 被点击时触发 |
| `LoopScrollRect` | Unity UI 组件，负责滚动逻辑、对象复用 |

**数据流向：**

```
SetDataRefresh(IList data, 0)
    → LoopScrollRect 滚动
    → 从 Item 池取/创建 Item
    → ProvideData(transform, index)
    → YIUILoopRenderer(item, data, index, select)
    → Item 的 UI 更新
```

---

## UBridge 待实现

| 指令 | 说明 |
|------|------|
| `YIUICreateCommon` | 创建 Common 类型预制体（当前需手动：右键 → YIUI → Create UICommon） |
| `YIUIAddControl` | 添加 YIUI 模板控件（LoopScrollVertical 等），当前需手动 |

---

## 程序集分布

| 层 | 路径 | 编译到 |
|----|------|--------|
| Runtime | `Runtime/*.cs` + `Runtime/Extend/*.cs` | `ET.YIUIFramework` |
| Editor | `Editor/*.cs` + `Editor/MenuItem/*.cs` + `Editor/TemplatePrefabs/` | `ET.YIUIFramework.Editor` |
| ModelView | `Scripts/ModelView/Client/LoopScroll/` + `Event/` | `ET.ModelView` |
| HotfixView | `Scripts/HotfixView/Client/LoopScroll/` | `ET.HotfixView` |
