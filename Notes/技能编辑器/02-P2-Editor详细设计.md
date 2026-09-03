# P2 Editor 详细设计

> 本文是 Editor 编码基线。总纲只保留路线；AI 自动化命令见 `03-P2-Editor-AI自动化接口.md`，
> 验收和暂停规则见 `04-P2-Editor验收与问题上报.md`。

## 1. 目标和边界

Editor 用 UI Toolkit 在 Unity 内编辑 SkillParams JSON，并提供确定性的动画、特效和盒体预览。

首版必须做到：

- 浏览、搜索和打开 Skill/Bullet/Area/Buff/Action
- 编辑字段、列表、时间窗口和整数引用
- Undo/Redo、dirty、保存、Reload、校验和外部修改检测
- 自绘时间轴、时间选择和预览控制
- 真实 SpriteRenderer 预览；资源缺失时有明确占位
- 预览不执行真实技能、物理、命中、回滚或联机

首版不做：

- Play 模式 IMGUI（P3）
- NPK 或动画帧盒写回（P4）
- 任意 C# 脚本编辑
- 通过运行时 LSCast 模拟战斗

## 2. 核心原则

1. JSON 是唯一可编辑源；Luban 生成类和运行时只读模型不由窗口直接修改。
2. 编辑状态、预览状态和运行时状态分离；窗口不创建或驱动 LSCast、LSWorld、LSUnit。
3. View 不直接散改 DTO；所有修改走 Session 的命令入口。
4. 时间换算、坐标换算和校验各有唯一实现，不能在不同 View 中复制公式。
5. 缺失资源和非法引用必须可见；不自动把错误 ID 改成 0。
6. 先做 HardAttack 纵向切片，再扩展全部面板和资源类型。

## 3. 分层和依赖

~~~text
SkillEditorWindow ─┐
Unity Pipeline CLI ├─> SkillEditorOperations
Batchmode Adapter ─┘       -> SkillEditorSession
                                -> DocumentStore
                                -> CommandHistory
                                -> ValidationService
                                -> AssetCatalog / IdCatalog
                                -> SkillTimelineProjection
                                -> PreviewController

Inspector / Timeline / Preview
  -> Session.Execute(command)
  -> 文档变化
      -> Validation
      -> View 刷新
      -> Preview 重新采样
~~~

Editor 程序集可以引用 ET.Skill、Npkparser、Unity UI Toolkit 和经探针确认的 Unity Pipeline，但不得反向引用
ET.Hotfix 或 ET.ModelView。Operations 是三个入口共用的应用层门面；CLI/Batchmode 只做参数适配和结果序列化，
不能复制 DocumentStore、Validation、Preview 或保存逻辑。

## 4. 文件结构

~~~text
Packages/cn.etetet.skill/Editor/SkillEditor/
  SkillEditorWindow.cs
  SkillEditorOperations.cs
  SkillEditorSession.cs
  SkillEditorDocument.cs
  SkillEditorDocumentStore.cs
  SkillEditorHistory.cs
  SkillEditorCommands.cs
  SkillEditorValidation.cs
  SkillTimelineProjection.cs
  SkillTimelineElement.cs
  SkillInspectorView.cs
  ContentInspectorView.cs
  SkillAnimCatalog.cs
  SkillPreviewController.cs
  SkillCoordinateMapper.cs
  SkillGizmoOverlay.cs
  SkillEditorCliCommands.cs
  SkillEditorCliModels.cs
  SkillEditorBatchmode.cs
~~~

已有的 `SkillEditorDocument.cs` 和 `SkillTimelineElement.cs` 是基础草稿；正式实现可重构，但不能改变
JSON 格式和本文的依赖边界。

## 5. 功能模块

| 模块 | 职责 | 关键约束 |
|---|---|---|
| Window | 菜单、布局、生命周期、面板路由 | OnDisable 统一释放资源 |
| Operations | list/get/validate/patch/save/preview/regression 应用门面 | UI/CLI/Batchmode 共用，不保存隐式全局脏状态 |
| Session | 当前文档、选择、时间、朝向、dirty、诊断 | 不保存静态旧引用 |
| DocumentStore | DTO 解析、快照、原子保存、Reload、外部变更 | 保存失败保留 dirty |
| History | Do/Undo/Redo、连续输入合并 | 不为每个字符/鼠标移动建历史 |
| Validation | 结构、范围、引用、动画和迁移约束 | 对 DTO 做无副作用校验 |
| AssetCatalog | 五类内容目录、坏文件、重复 ID | 文件名不参与寻址 |
| TimelineProjection | Phase 起点、局部/全局/帧时间 | 唯一时间算法 |
| Timeline | 泳道、事件、播放头、选择和拖动 | 通过命令修改，不直接改 DTO |
| Skill Inspector | Skill 基础字段、Phase、Reaction、Box、事件 | 显式强类型控件 |
| Content Inspector | Bullet/Area/Buff/Action 全字段 | 引用显示 id-name，保存整数 |
| AnimCatalog | AnimId、ani.bytes、overlay、缺失状态 | 不按文件名猜 ID |
| Preview | 播放、暂停、逐帧、朝向、RenderTexture | 不执行真实战斗 |
| Mapper/Gizmo | 坐标、盒体、轨迹、诊断标记 | 与运行时共用纯规则 |
| CLI/Batchmode Adapter | Unity CLI 和固定无头入口 | 只解析请求/序列化结果，见 03 |

## 6. 窗口布局和操作

### 左侧资产浏览器

五个分类页签：Skill、Bullet、Area、Buff、Action。支持按整数 ID、名称和文件路径搜索；列表显示错误、缺失
引用和 dirty 标记。选择资产只改变 Session，不直接写盘。

### 中央预览区

- RenderTexture 嵌入 UI Toolkit
- 播放/暂停、速度、时间拖动、上一帧/下一帧
- 左右朝向
- 攻击盒、受击盒、Area、轨迹和坐标轴开关
- 资源缺失时显示品红或线框占位、AnimId、路径和原因

### 下方时间轴

- Cast 全局刻度和 Phase 泳道
- 每段 animId、持续时间和帧边界
- ManualBox 窗口
- SpawnEvent 菱形
- HitEvent 所属 phase 的命中泳道
- 位移区间、转移箭头和播放头
- 50ms 吸附、缩放、横向平移、选择、复制、删除

HitEvent 没有固定的动画时间点时，只显示其命中触发语义，不伪造一个毫秒位置。

### 右侧 Inspector

按选择对象显示：

- Skill：基础字段、Phase、Movement、HitReaction、ManualBox、SpawnEvent、HitEvent、HitActions
- Bullet：速度、寿命、碰撞盒、偏移、视图动画、命中反应和动作
- Area：盒体、持续时间、Tick、进入/退出动作、视图动画
- Buff：持续时间、Tick、叠层、刷新规则和 Action 列表
- Action：kind、value、intervalMs、buffId

## 7. 核心接口形状

~~~csharp
internal sealed class SkillEditorSession : IDisposable
{
    public SkillEditorDocument Document { get; private set; }
    public SkillEditorSelection Selection { get; }
    public SkillPreviewState Preview { get; }
    public SkillEditorHistory History { get; }
    public IReadOnlyList<SkillEditorIssue> Issues { get; }
    public bool IsDirty { get; }

    public bool TryOpen(SkillEditorAsset asset, out string error);
    public void Execute(ISkillEditorCommand command);
    public bool TrySave(out string error);
    public bool TryReload(out string error);
    public void Validate();
    public void Dispose();
}

internal interface ISkillEditorCommand
{
    string Description { get; }
    void Do(SkillEditorSession session);
    void Undo(SkillEditorSession session);
}
~~~

初版命令历史可以保存前后 JSON 快照；确认行为正确后再优化为字段差异。文本输入在失焦或提交时形成一次
命令，时间轴连续拖动在 pointer-up 时形成一次命令。

## 8. 时间、预览和坐标管道

### 时间投影

`SkillTimelineProjection` 统一输出 phase 起点、phase 局部时间、cast 全局时间、动画帧区间和事件位置。
运行时和 Editor 都遵循 `previous < atMs <= current`；atMs=0、PhaseEnter、Input/untilMs 和 phase 重入
必须在时间轴上有明确标记。

### 预览采样

预览控制器按全局时间确定当前 phase，再按帧 delay 采样 AnimClipData。delay <= 0 时使用 50ms。SpawnEvent
到点只创建预览实例并显示持续时间、方向、轨迹和 ViewOffset，不执行伤害或碰撞。

### 坐标转换

所有盒体和精灵位置经过同一个纯转换管道：

~~~text
DNF 像素 min/max
  -> /100 游戏单位
  -> Y/Z 对调
  -> imagePos + frame center
  -> ViewOffset
  -> 面向左镜像 X
  -> Preview 世界坐标
  -> 相机/Canvas 坐标
~~~

辅助层绘制原点、地面线、攻击盒、受击盒、Area 盒、轨迹、当前帧和选中高亮。变换检查器逐步显示中间值，
最后一步必须能对应预览中的 Sprite 或盒体。

## 9. 动画资源适配

现有资源主要是 `*.ani.bytes`，另有少量动画/overlay `*.json`；AnimId 到地址、切片和 overlay 别名的关系
主要写在运行时 `LSAnimClipRegistrar`，不是资源文件顶层字段。

因此 `SkillAnimCatalog` 不得只扫描 JSON，也不得按文件名猜 AnimId。第 0 步先验证：

1. 是否能在不破坏程序集拓扑的情况下复用注册流程。
2. 如果不能，是否抽取 Editor/运行时共用的纯映射清单。
3. 切片动画、overlay 别名和 Sprite 中心点是否与运行时完全一致。

如果需要修改运行时资源协议或抽取共享清单，按 `04-P2-Editor验收与问题上报.md` 的 A 类规则暂停。

## 10. 实现顺序

### Step 0：依赖探针

验证 UI Toolkit、Newtonsoft、PreviewRenderUtility、RenderTexture、AnimClipData 和动画映射。只验证，不顺手改
运行时语义。

### Step 1：HardAttack 纵向切片

实现 Operations、Window、Session、DocumentStore、History、Validation、空 Preview 接口和最小时间轴；
完成打开、修改 cooldownMs、保存、Reload、校验、Undo/Redo，并让 CLI 的 list/get/validate 调用相同服务。

### Step 2：Skill 编辑完整化

加入 Phase、ManualBox、SpawnEvent、HitEvent 的选择、增删、复制、拖动和引用校验；所有修改走命令。
同时实现 patch dryRun、expectedHash、原子保存和 UI dirty 冲突检测。

### Step 3：真实预览

先完成稳定占位和时间采样，再接 SpriteRenderer、NPK、overlay、ViewOffset、镜像和辅助盒体。

### Step 4：全资产和回归

开放 Bullet/Area/Buff/Action，补依赖跳转、全量目录校验、preview/regression CLI 和代表性技能回归。
完成 P2 后才能进入 P3。
