---
name: et-yiui
description: YIUI framework conventions for this project. Use when creating/modifying UI panels, writing panel Components/Systems, using bindings (u_Com/u_Event), loop scroll lists, or authoring .ui.yaml specs.
---

# et-yiui - YIUI UI 规范入口

> 本项目 UI 唯一体系为 YIUI（cn.etetet.yiuiframework v3.1.4 + yiuiinvoke + yiuiyooassets + yiuiloopscrollrectasync）。
> 决策：全部 YIUI；UILSLogin/UILSLobby/UILSRoom 已冻结；cn.etetet.ui 停用——**新 UI 一律 YIUI，禁止再用 ET 原生 UI 方式新建界面**。

## 何时使用

- 新建/修改 UI 面板（spec 或手写代码）
- 使用 CDE 绑定（u_Com\*/u_Event\*）、数据绑定、事件
- 循环列表（LoopScrollRect）
- 跨面板/跨模块调用（yiuiinvoke）

## 面板的三层文件（以 LoginPanel 为例）

| 文件 | 目录 | 可否手改 |
|---|---|---|
| `LoginPanelComponentGen.cs` | `Scripts/ModelView/Client/YIUIGen/Login/` | **禁止**（YIUI 工具生成） |
| `LoginPanelComponentSystemGen.cs` | `Scripts/HotfixView/Client/YIUIGen/Login/` | **禁止**（YIUI 工具生成） |
| `LoginPanelComponent.cs` | `Scripts/ModelView/Client/YIUIComponent/Login/` | 手写（自定义字段放这） |
| `LoginPanelComponentSystem.cs` | `Scripts/HotfixView/Client/YIUISystem/Login/` | 手写（逻辑放这） |

## 命名约定

- 绑定字段：`u_Com*`（C 表组件）、`u_Event*`（E 表事件）；绑定名 = prefab 节点名
- 组件类：`<面板名>Component`；常量 `PkgName`/`ResName` 由 Gen 生成，引用资源一律用常量不硬编码字符串
- spec 文件：`Assets/GameRes/YIUI/<Pkg>/<面板名>.ui.yaml`；prefab 在 `<Pkg>/Prefabs/`

## 面板生命周期（手写 System 中可用的钩子）

| 接口/方法 | 签名 | 用途 |
|---|---|---|
| Awake | `IAwake` | 组件初始化 |
| YIUIBind | `IYIUIBind` | 绑定后（Gen 的 UIBind 自动调用，勿手写重名） |
| YIUIInitialize | `IYIUIInitialize` | 面板初始化（建子组件、列表） |
| YIUIOpen | `async ETTask<bool> YIUIOpen(...)` | 打开（异步加载/刷新数据），return false 中断 |
| Destroy | `IDestroy` | 销毁清理 |

打开面板：`await root.OpenPanelAsync<T>()`（YIUIRootComponent 扩展，支持最多 P5 传参）；关闭走 PanelOption/层级管理，勿直接 Destroy。

## 循环列表（YIUILoopScrollChild）

```csharp
// 初始化（YIUIInitialize）
var scrollRect = self.u_ComLoopScrollHorizontal.GetComponent<LoopHorizontalScrollRect>() as LoopScrollRect;
self.m_Loop = self.AddChild<YIUILoopScrollChild, LoopScrollRect, Type, string>(
    scrollRect, typeof(ItemComponent), "u_EventSelect");

// 刷新（YIUIOpen）
self.Loop.SetDataRefresh(list, defaultIndex).NoContext();

// 渲染/点击：EntitySystem 泛型方法（owner, item, data, index, select）
```

## spec 编写（拼 UI 的正道）

完整定义见 `Notes/UIBuilder-P1实施方案.md` §3（schema v1：panel/nodes/place/layout/props/events）。
要点：spec 是唯一真源；`u_Com*` 自动进 C 表；初期无贴图（type+props 搭结构）；改 UI = 改 spec 重建，**不直接改 prefab**。

## 跨模块通信

面板间/模块间调用走 yiuiinvoke（`YIUIInvoke` 特性 + handler），不互相引用面板类型，保持解耦。

## 红线

- YIUIGen 生成文件禁改；prefab 由 Builder 从 spec 生成，禁手工改（人工调整走 v2 ExportSpec 导回）
- **`#region YIUIEvent开始/结束` 标记禁删**（YIUI 区域合并依赖它；删了代码生成报错）
- 帧同步相关数值/逻辑不进 UI 层；UI 只做展示与表现
- ET 原生 UI（UIComponent/AUIEvent/UILSxxx）不再扩展

## rebuild 安全性边界（改 spec 重建对手写代码的影响）

| 文件 | rebuild 行为 |
|---|---|
| YIUIGen/*（Gen） | 全量重写（禁手改，无损失） |
| YIUIComponent/*（partial） | 已存在则跳过（ExistSkip），永不覆盖 |
| YIUISystem/*（partial） | 区域增量合并：YIUIEvent region 内已有事件 stub 保留（内写逻辑安全）、新事件追加 region 尾；**region 外代码永不触碰** |

依据：`TemplateEngine.RegionCheckReplace`（默认 cover=false 只增不删）。
