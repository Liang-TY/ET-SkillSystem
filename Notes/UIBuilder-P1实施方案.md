# UIBuilder P1 实施方案

> 日期：2026-08-24 ｜ 状态：待确认
> 配套文档：`automation/PROTOCOL.md`（任务总线协议）、`CLAUDE.md`（项目规范入口）

## 0. 背景与已确认决策

| 决策 | 内容 |
|---|---|
| UI 体系 | 全部 YIUI；UILSLogin/UILSLobby/UILSRoom 冻结；cn.etetet.ui 停用 |
| 像素风 | 暂不做，spec 预留 style/image 字段 |
| 贴图 | P1 不上贴图，用 Unity 自带控件外观量产 panel，贴图 pass 后补 |
| 真源 | `*.ui.yaml` spec 是唯一真源，prefab 全量重建，不增量合并 |
| 布局 | 自动布局为主、锚点预设为辅、样式 token 预留 |
| 通道 | Unity CLI（com.unity.pipeline，[CliCommand]）为主；ubridge 文件桥保留为回退 |
| 拓扑 | git 任务总线为主链路（主 AI 开发机下单 / 从 AI Unity 机施工）；UU端口映射+SSH 为调试旁路 |
| 从 AI | 无头模式（watcher 触发 `claude -p`，低配模型）；规范见 PROTOCOL.md |

## 1. 总体架构

```
开发机（主AI，无Unity）                     Unity 机（从AI + Builder）
──────────────────────                    ──────────────────────────
真源: *.ui.yaml + 手写System/Component      常驻: Unity编辑器(-automated) + watcher脚本
离线: lint（schema/命名/资源/绑定一致性）      触发: automation/tasks 有待办 → claude -p 无头会话
下单: git push 任务单 ──────────────▶      执行: unity command yiui_build_panel ...
收图: pull results（BuildResult+PNG）◀──── 产出: prefab + YIUIGen代码 + 截图 → wip-{id} 分支
合并: 主AI审查后 merge wip → 功能分支
```

## 2. P1 范围

**做**：spec schema v1、Builder 包（cn.etetet.uibuilder）、4 个 CliCommand、预览截图、任务总线与从 AI 配套、LtyTestPanel 等价性验收、新面板量产验证。

**不做**：贴图引用生效、D 表（数据绑定）、多 View 拆分、ExportSpec 反向导出、样式 token 生效、Play 模式验证、Tailscale（如需再加）。

## 3. spec schema v1

文件：`Packages/cn.etetet.<pkg>/Assets/GameRes/YIUI/<Pkg>/<Panel>.ui.yaml`
prefab 默认输出：`<spec目录>/Prefabs/<Panel>.prefab`（可用 `panel.prefabPath` 覆盖）。
手写代码归属：`Scripts/ModelView/Client/YIUIComponent/<Pkg>/` 与 `Scripts/HotfixView/Client/YIUISystem/<Pkg>/`（现有约定不变，YIUIGen 生成路径不变）。

### 3.1 panel 段

| 字段 | 类型 | 默认 | 说明 |
|---|---|---|---|
| name | string | 必填 | 面板名，如 `SkillPanel`。= prefab 名 = 组件名前缀 |
| pkg | string | 必填 | YIUI 包名（目录与生成代码归属） |
| layer | enum | Panel | Top/Tips/Popup/Panel/Scene/Bottom（EPanelLayer） |
| cacheSeconds | int | 0 | TimeCache 秒数，0=不缓存 |
| blockBg | bool | true | 自动带 UIBlockBG 子节点 |
| stackOption | enum | VisibleTween | EPanelStackOption |
| priority | int | 0 | |
| prefabPath | string | 按默认规则推导 | 覆盖输出路径 |
| codePackage | string | 空 | 代码生成的目标 UPM 包名（cn.etetet.\<x\> 的 x）。**空 = YIUI 按 prefab 所在位置自动推导（推荐）**。注意与 pkg（YIUI 资源包名）是两个概念，传错会把生成代码写进无关包 |

### 3.2 nodes 段（节点树）

```yaml
nodes:
  - name: string        # 必填。u_Com 开头 → 注册 CDE 表 C 表；否则纯布局节点
    type: enum          # 见 3.3 控件类型表
    bind:               # 可选，覆盖默认绑定组件
      component: string # 如 RectTransform / Text / Button / LoopVerticalScrollRect
    place: {...}        # 定位（见 3.4）；不写 = 铺满父节点
    layout: {...}       # 作为容器的自动布局（见 3.5）；作用于子节点排列
    props: {...}        # 控件属性，封闭集合（见 3.3）
    image: string       # 预留（贴图 pass 启用），v1 忽略并 warning
    style: string       # 预留（样式 token）
    children: [...]     # 子节点递归
```

规则：
- 节点名树内唯一（lint 检查）。
- `u_Com*` 的绑定名 = 节点名；绑定组件类型 = type 的默认绑定（表 3.3），可用 `bind.component` 覆盖。
- 事件名建议 `u_Event*` 前缀（lint 检查）。

### 3.3 控件类型表（v1 封闭集合）

| type | 构造来源 | 默认绑定组件 | props（封闭集合，括号内默认值） |
|---|---|---|---|
| node | 代码直建 RectTransform | RectTransform | （无） |
| text | YIUI 模板 YIUIText_NoRaycast | Text | text(""), fontSize(24), color(#FFFFFFFF), alignment(UpperLeft..LowerRight, MiddleCenter), raycast(false), bestFit(false) |
| tmp | YIUI 模板 YIUIText (TMP) | TextMeshProUGUI | text(""), fontSize(24), color, alignment |
| image | YIUI 模板 YIUIImage_NoRaycast | Image | color(#FFFFFFFF), imageType(Simple), raycast(false), preserveAspect(false), fillAmount(1), fillMethod(Horizontal) |
| button | YIUIButton_NoText 模板 + Builder 补齐 Button(targetGraphic+ColorTint) + 自建 legacy Text 标签（模板本身无 Button——YIUI 走 EventBind 体系；TMP 标签为 LiberationSans SDF 无中文字形，CJK TMP 字体待字体方案确定后接入） | Button | text(""), fontSize(24), interactable(true), color |
| input | DefaultControls.InputField | InputField | text(""), placeholder("请输入..."), contentType(Standard), lineType(SingleLine) |
| toggle | DefaultControls.Toggle | Toggle | isOn(false), label("Toggle") |
| slider | DefaultControls.Slider | Slider | value(0), minValue(0), maxValue(1) |
| dropdown | DefaultControls.Dropdown | Dropdown | options([字符串数组]) |
| scroll_view | DefaultControls.ScrollView | ScrollRect | horizontal(true), vertical(true), movementType(Elastic) |
| loop_scroll_v | 模板 LoopScrollVertical | LoopVerticalScrollRect | **item(必填)**, reverse(false)。item 仅校验/文档用——YIUI 体系下列表项由手写 System 经 typeof(ItemComponent) 运行时绑定，prefab 无序列化槽位；间距由 item 尺寸/运行时决定 |
| loop_scroll_h | 模板 LoopScrollHorizontal | LoopHorizontalScrollRect | 同上 |
| prefab | 实例化指定 prefab | （按 bind.component 指定） | **path(必填，项目内路径)** |
| block | UIBlock 组件节点 | 无 | color |

模板查找目录（沿用 ubridge 现有机制，`UBridgeControlHandlers.cs` 的 `s_TemplateDirs`）：
`Packages/cn.etetet.yiuiloopscrollrectasync/Editor/TemplatePrefabs`、`Packages/cn.etetet.yiuiframework/Editor/TemplatePrefabs/YIUI`。
扩展新类型 = 加模板 prefab + 在 ControlFactory 注册表加一行。

### 3.4 place 段（锚点定位）

| 字段 | 默认 | 说明 |
|---|---|---|
| anchor | center | 预设词：center / top / bottom / left / right / top_left / top_right / bottom_left / bottom_right / stretch(=full) / top_stretch / bottom_stretch / left_stretch / right_stretch |
| offset | [0,0] | 九宫格点锚：相对锚点位置（部分拉伸预设的点轴忽略 offset） |
| margins | [0,0,0,0] | [左,上,右,下]：stretch 轴取对应两边内缩；部分拉伸预设的贴边轴取对应一边作内缩 |
| size | 按类型默认 | 显式尺寸；button 默认 [160,48]，text 默认 [200,40]，其余默认 [100,100] |
| pivot | [0.5,0.5] | |
| rotation | 0 | z 轴角度 |
| scale | [1,1] | |

根节点下第一层子节点若不写 place，默认 `anchor: stretch, margins 全 0`（铺满，与现有面板习惯一致）。

### 3.5 layout 段（自动布局）

| 字段 | 默认 | 说明 |
|---|---|---|
| type | 必填 | vertical / horizontal / grid |
| spacing | 0 | float 或 [x,y] |
| padding | [0,0,0,0] | [左,右,上,下] |
| childAlignment | UpperLeft | TextAnchor 枚举名 |
| controlChildSize | true | |
| childForceExpand | false | |
| cellSize | — | grid 必填 [w,h] |
| constraint | Flexible | Flexible / FixedColumnCount / FixedRowCount |
| constraintCount | 1 | |

子节点在 layout 容器下**不写 place**（写了 lint 报 warning：会被 LayoutGroup 覆盖）。此时 Builder 自动：居中锚点 + 类型默认尺寸 + 挂 `LayoutElement(preferredWidth/Height=尺寸)`，让 LayoutGroup 取到正确 preferred（否则子节点被压成近 0 尺寸）。

### 3.6 events 段（E 表）

```yaml
events:
  - name: u_EventClose   # 事件名
    sync: false          # 默认 false = TaskEvent（异步）
    params: []           # EUIEventParamType: Bool/Int/Long/Float/String/Object/ParamVo
    target: u_ComBtnClose # 挂载目标节点名，必须存在于节点树
    trigger: Click       # Click / ClickDown / ClickUp，默认 Click
```

### 3.7 完整示例（无贴图量产风格）

```yaml
# Packages/cn.etetet.lockstep/Assets/GameRes/YIUI/Skill/SkillPanel.ui.yaml
panel:
  name: SkillPanel
  pkg: Skill
  layer: Panel
  cacheSeconds: 10

nodes:
  - name: Title
    type: text
    place: { anchor: top, offset: [0, -40], size: [400, 60] }
    props: { text: 技能, fontSize: 36 }

  - name: u_ComBtnRoot
    type: node
    place: { anchor: stretch, margins: [20, 20, 200, 20] }
    layout: { type: vertical, spacing: 12, padding: [10, 10, 10, 10] }
    children:
      - { name: u_ComBtnSkill1, type: button, props: { text: 上挑 } }
      - { name: u_ComBtnSkill2, type: button, props: { text: 三段斩 } }
      - { name: u_ComBtnSkill3, type: button, props: { text: 裂波斩 } }

  - name: u_ComLoopSkillList
    type: loop_scroll_v
    place: { anchor: stretch, margins: [220, 80, 20, 80] }
    props:
      item: Packages/cn.etetet.lockstep/Assets/GameRes/YIUI/ScrollTest/TestScrollItem.prefab

  - name: u_ComBtnClose
    type: button
    place: { anchor: top_right, offset: [-40, -40], size: [64, 64] }
    props: { text: X }

events:
  - { name: u_EventClose, target: u_ComBtnClose, trigger: Click }
```

### 3.8 离线 lint 规则（开发机，dotnet CLI）

1. YAML 语法 + schema 完整性（必填/枚举合法/类型匹配）
2. props 属性名 ∉ 该 type 封闭集合 → 错误（防编造字段）
3. 节点名唯一、events.target 存在于节点树
4. `u_Com*`/`u_Event*` 命名规范
5. 引用的资源路径（item/path/prefabPath）文件存在（仓库内直接查）
6. 绑定一致性：spec C/E 表 vs 手写 System 代码中引用的 `u_Com*/u_Event*` 双向核对
7. 子节点 place 与父节点 layout 并存 → warning（place 会被 LayoutGroup 覆盖；place 与 layout 同节点并存是合法用法：容器自身定位 + 子节点布局）

## 4. Builder 包设计

### 4.1 目录

```
cn.etetet.uibuilder/
  package.json                     # 依赖 cn.etetet.yiuiframework
  Editor/
    UIBuilder.Editor.asmdef        # 引用: ET 框架 Editor 程序集；对 YIUI Editor 反射调用（同 ubridge 现状）
    Core/
      SpecModel.cs                 # POCO: PanelSpec/NodeSpec/PlaceSpec/LayoutSpec/EventSpec
      SpecLoader.cs                # YamlDotNet 反序列化 + schema 校验（错误收集，不遇错即弃）
      BuildPipeline.cs             # 编排（见 4.2）
      BuildResult.cs               # 结构化结果
    Build/
      PanelAssembler.cs            # 面板骨架（逻辑迁移自 UBridgeYIUICreatePanelHandler 系列）
      ControlFactory.cs            # 类型注册表 Dictionary<string, Func<NodeSpec, GameObject>>
      LayoutApplier.cs             # place/layout → RectTransform/LayoutGroup（迁移自 UBridgeRect/LayoutHandlers）
      PropConfigurator.cs          # 每 type 一张属性表（封闭集合）
      CDEBinder.cs                 # C/E 表写入 + 事件组件挂载（迁移自 UBridgeCDEHandlers）
      CodeGenTrigger.cs            # 反射 UICreateModule.CreatePackages(cde, true, false, pkg)
    Preview/
      PreviewRenderer.cs           # 离屏场景渲染 → PNG
    Commands/
      UIBuilderCliCommands.cs      # [CliCommand] 注册
    Bridge/
      UBridgeUIBuilderHandlers.cs  # ubridge 薄壳（回退通道）
    Menu/
      UIBuilderMenu.cs             # 人工菜单入口
  Plugins/
    YamlDotNet/                    # YamlDotNet.dll（netstandard2.1，随包内置，免 NuGetForUnity 依赖）
```

### 4.2 BuildPipeline 主流程

```csharp
public static BuildResult Build(string specPath, bool runCodeGen = true, bool runPreview = true)
{
    // ① SpecLoader.Load：解析 + 校验（收集全部错误后一次性返回）
    // ② PanelAssembler.CreateSkeleton：内存中建根节点 + UIBlockBG + CDE 表（不碰磁盘）
    // ③ 递归 BuildNode：ControlFactory 建 → LayoutApplier 定位 → PropConfigurator 配属性 →（u_Com* → CDEBinder）
    // ④ events → CDEBinder 写 E 表 + 挂事件组件
    // ⑤ CodeGenTrigger：生成 YIUIGen 代码（已存在的手写 partial 不覆盖，YIUI 工具自身行为）
    // ⑥ 全部成功 → PrefabUtility.SaveAsPrefabAsset 落盘（失败则丢弃内存对象，磁盘零污染）
    // ⑦ AssetDatabase.SaveAssets + Refresh → 等待编译 → CompilerMessage 收集错误
    // ⑧ PreviewRenderer.Capture 截图
}
```

### 4.3 CliCommand 清单

| 命令 | 参数 | 行为 |
|---|---|---|
| `yiui_build_panel` | --spec \<path\> --json | 全量构建，返回 BuildResult JSON |
| `yiui_build_all` | --dir \<specDir\> | 批量构建目录下全部 spec（量产用） |
| `yiui_preview_panel` | --prefab \<path\> --width 1920 --height 1080 --out \<dir\> | 预览截图，返回 PNG 路径 |
| `yiui_compile_check` | --timeout 300 | Refresh → 轮询 isCompiling → 返回 CompilerMessage[]（file/line/message/type） |
| `yiui_list_types` | | 输出全部 type + props 封闭集合 JSON（AI 自查用） |
| `yiui_export_spec` | --prefab \<path\> | **v2**：prefab → spec 反向导出 |

BuildResult JSON：

```json
{
  "ok": true, "spec": "…", "prefab": "…",
  "genFiles": ["…"], "preview": "…png",
  "warnings": ["…"],
  "errors": [ { "file": "…", "line": 18, "type": "Error", "message": "…" } ]
}
```

### 4.4 预览实现要点

新建临时空场景 → Canvas（与 YIUIRoot 同配置：ScreenSpaceOverlay + ScaleWithScreenSize 1920×1080 match width）→ 实例化 prefab → `Canvas.ForceUpdateCanvases()` → 离屏相机渲染 RenderTexture → ReadPixels → PNG 写 `automation/results/` 或 `--out` 指定目录 → 恢复原场景。不依赖 GameView 状态，可任意分辨率。

### 4.5 环境与依赖

- YamlDotNet：dll 内置于包（netstandard2.1）。
- com.unity.pipeline：Unity 机项目内安装（`unity pipeline install`），编辑器以 `-automated` 启动。
- Unity CLI（beta）：`$env:UNITY_CLI_CHANNEL='beta'; irm https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1 | iex`。
- 兼容性已核：pipeline 要求 Unity 6.0 LTS+，本项目 6000.0.25f1 ✓。

## 5. 实施步骤

| 步骤 | 内容 | 完成定义 |
|---|---|---|
| S1 ✅ | uibuilder 包骨架 + SpecModel/SpecLoader（含全部 lint 规则 3.8.1-5） | 坏 spec 返回完整错误清单 |
| S2 ✅ | PanelAssembler + ControlFactory + LayoutApplier + PropConfigurator | 示例 3.7 构建出正确 GameObject 树（场景内人工验证） |
| S3 ✅ | CDEBinder + CodeGenTrigger（迁移 ubridge 逻辑） | 构建后 YIUIGen 文件生成、C/E 表正确（SkillPanel 实测：6 C 表 + 1 E 表 + 面板选项全部精确映射） |
| S4 | PreviewRenderer | 任意 prefab 出 PNG |
| S5 | CliCommand + ubridge 薄壳 + Menu 三入口 | `unity command yiui_build_panel` 跑通；ubridge 文件桥同样跑通 |
| S6 | Unity 机环境（CLI/pipeline/-automated/watcher/从AI配置） | 一条 Ping 任务全链路回环 |
| S7 | 验收（见 §6） | 全部通过 |

S1-S5 在 Unity 机开发（Builder 是 Editor 代码），开发机可并行写 lint CLI、协议文件、skills（本次已交付）。

## 6. P1 验收标准

1. **等价性**：为现有 `LtyTestPanel` 写 spec（描述其现状），构建产物与手工 prefab 等价——层级一致、C 表条目 `u_ComLoopScrollHorizontal→RectTransform` 一致、YIUIGen 生成文件与现有 diff 无实质差异。
2. **增量迭代**：改 spec（加一个按钮 + 一个事件）→ rebuild → diff 符合预期，手写 System 不被覆盖。
3. **失败路径**：写错属性名 / 引用不存在节点 → 构建失败返回结构化错误；原 prefab 不被破坏。
4. **编译闭环**：手写代码故意留错 → build 结果含结构化编译错误清单（file/line/message）。
5. **预览**：截图 PNG 布局正确，1920×1080。
6. **量产指标**：一个 10 节点新面板，从写 spec 到"可编译+截图在手"≤ 5 分钟（Unity 机上单次 build ≤ 60 秒）。
7. **回退通道**：pipeline 不可用时，ubridge 文件桥仍能完成 build。

## 7. 风险与对策

| 风险 | 对策 |
|---|---|
| pipeline 为 0.1.0 beta（token 失效/命令名漂移） | ubridge 文件桥保留为主回退；命令层薄壳隔离，切换成本一行 |
| YamlDotNet 依赖 | dll 随包内置；极端情况 schema 同构切换 JSON（解析层独立） |
| 编辑器弹窗/焦点问题 | 编辑器 `-automated` 启动；watcher 超时上浮 |
| YIUI 代码生成模板随版本变化 | CodeGenTrigger 只反射调 `UICreateModule.CreatePackages`，不复制模板逻辑 |
| 编译反馈慢（远端） | 离线 lint 前置拦截大部分；compile 结构化回传 |
| 人工改 prefab 与 spec 分叉 | 约定 spec 唯一真源 + v2 ExportSpec 导回；build 覆盖前提示 |

## 8. P2/P3 展望（不在本方案内）

- P2：贴图 pass（image 字段生效、素材库约定、批量回填）���样式 token、`asset_gen`（ComfyUI worker）、`run_tests`/`build_player` 通用 CI 任务。
- P3：Tailscale 交互模式、多 View 拆分、D 表支持、Play 冒烟（需先解决 bytes 模式 Play 与桥接共存）。
