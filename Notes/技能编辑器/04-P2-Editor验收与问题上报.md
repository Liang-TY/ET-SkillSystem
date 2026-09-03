# P2 Editor：验收与问题上报

> 本文是 Editor 开始编码后的验收门槛和暂停规则。设计细节见
> 02-P2-Editor详细设计.md，AI/Unity CLI 契约见 03-P2-Editor-AI自动化接口.md。
> 未通过 P2 验收前，不进入 P3 Play 模式 IMGUI。

## 1. 验收原则

- 以 JSON 参数源、运行时行为和 Editor 预览三者可追溯为准，不以“窗口能打开”作为完成。
- 每个结论都要有可重复步骤和证据：命令输出、日志、JSON 哈希、截图或录屏。
- 预览允许资源占位，但必须明确标记缺失原因；不能把占位当成资源已验证。
- 发现影响数据语义、程序集边界或运行时一致性的情况，立即暂停对应阶段并上报。

## 2. 分阶段门槛

| 阶段 | 进入条件 | 必须交付 | 通过条件 |
|---|---|---|---|
| Step 0 依赖探针 | P1 参数加载和独立校验通过 | UI Toolkit、PreviewRenderUtility、动画资源和 Pipeline 探针记录 | 程序集可编译，资源取得路径和释放行为明确 |
| Step 1 纵向切片 | 探针无 A 类问题 | HardAttack 的打开、单字段编辑、保存、Reload、Undo/Redo | 改前后哈希和 DTO 值可解释，错误不会写盘 |
| Step 2 Skill 完整编辑 | Step 1 通过 | Phase、盒体、SpawnEvent、HitEvent、引用编辑 | 时间投影、命令历史和校验结果稳定 |
| Step 3 真实预览 | 动画映射探针通过 | SpriteRenderer、中心点、镜像、ViewOffset、辅助盒层 | 同一 AnimId/帧与运行时位置一致，缺失资源可诊断 |
| Step 4 全资产回归 | Step 2/3 通过 | 五类资产浏览、依赖跳转、CLI 回归报告 | 代表性技能和全量目录无未解释 Error |

## 3. 功能验收清单

### 3.1 窗口和生命周期

- 菜单可打开窗口；关闭、重开和域重载不报错。
- PreviewRenderUtility、RenderTexture、临时 GameObject、材质和 Sprite 引用均在 OnDisable/Dispose
  释放；无隐藏场景残留。
- 左侧能浏览和搜索 22 Skill、4 Bullet、14 Area、4 Buff、7 Action，显示整数 ID、名称、文件和诊断状态。

### 3.2 编辑和持久化

- 打开 HardAttack，分别修改基础字段、Phase、ManualBox、SpawnEvent、HitEvent 和一个整数引用。
- 每次修改设置 dirty；取消/Reload 时明确提示丢弃 dirty，不静默覆盖。
- Undo/Redo 对字段编辑、列表增删、复制、删除和时间轴拖动有效；连续输入/拖动合并为一个历史步骤。
- 保存前校验；Error 阻止保存，Warning 进入结果但不阻止带占位预览。
- 保存采用原子替换；保存失败保留旧文件和 dirty 状态。
- 保存后 Reload，值、列表顺序和格式化 JSON 可重现；文件顶层 id 不因重命名改变。

### 3.3 时间和事件

- 时间轴同时显示 Cast 全局时间、Phase 起点和 Phase 局部时间。
- 定时事件使用 previousTime < atMs <= currentTime；atMs=0 在 Cast 创建或 Phase 进入时执行。
- 显示并校验 PhaseEnter、PhaseEnd、Input/untilMs、phase 重入和 Cast-wide 事件。
- 校验 untilMs >= atMs、Phase 索引、duration/cancel 边界、nextPhase 目标和事件必填 ID。
- HitPolicy 至少能显示并保存 FirstHitInCast、FirstHitInPhase、OncePerTargetInPhase、
  EveryResolvedHit；没有动画时间点的 HitEvent 不伪造 atMs。

### 3.4 预览一致性

- 角色和 Bullet/Area 使用真实 SpriteRenderer 或明确的线框/品红占位，不用近似 GUI 图片替代。
- 同一 AnimId 和帧的 delay、中心点、像素密度、镜像、sorting、ViewOffset 与运行时一致。
- 坐标检查器能展示像素值、单位换算、Y/Z 对调、中心点、偏移、镜像和最终预览坐标。
- 攻击盒、受击盒、Area 盒、位移轨迹、当前帧和事件标记能开关并与时间选择同步。
- 预览不创建 LSCast/LSWorld，不执行碰撞、伤害、物理、联网、回滚或真实 Buff/Action。

### 3.5 AI/Unity CLI

- unity command 能列出技能命令，并能返回单个结构化 JSON 结果。
- list/get/validate 对五类资产和整数 ID 工作；缺失 ID、坏 JSON、重复 ID 有稳定错误码。
- patch 默认 dryRun；dryRun 不改变文件和全局 Loader。
- save 必须经过校验，支持 expectedHash 冲突检测和原子写入；冲突时不覆盖、不丢弃。
- open/preview/regression 与窗口调用同一核心服务；不得依赖鼠标坐标或当前 UI 布局。
- 输出目录和写入目录均受限；不能通过 CLI 执行任意代码或写工程外文件。

## 4. 代表性回归矩阵

先执行以下小集合，再执行全量目录扫描。每项记录“加载、时间轴、预览、保存/Reload、CLI”结果。

| 技能 | 重点覆盖 |
|---|---|
| HardAttack | 单 Phase、ManualBox、基础字段和最小纵向切片 |
| NormalAttack | 普通/空中分支、帧盒只读 |
| HopSmash | 多段 Phase、落地/状态前置 |
| ChargeCrash | movement、stopOnHit、末端事件 |
| GoreCross | Input 窗口、untilMs、跨帧命中 |
| GrandWave | Bullet 引用和投射物视图 |
| BloodBlast | Area 引用、多段 Tick 和多个视图事件 |
| MonsterIceBreath | 怪物技能、Bullet、ProcBuff |

资产级最少覆盖：一个 Bullet、一个 Area、一个 Buff、一个 Action 的打开、整数引用跳转、
字段修改、校验、保存和 Reload。随后扫描全部 22/4/14/4/7 文件，记录坏文件和缺失引用。

## 5. 证据格式

每个通过的步骤至少保存：

~~~text
evidence/
  <date>-<step>-commands.json       CLI 输入和结构化输出
  <date>-<step>-before-after.json   变更前后 DTO 或哈希
  <date>-<step>-preview.png         代表性时间点截图
  <date>-<step>-notes.md            Unity 版本、资源路径和已知占位
~~~

Unity/F6、MemoryPack 和实际 Play 行为回归结果必须单独标记；独立 Loader 的
loaderValid=true、referenceErrors=0 只能证明参数目录和引用可加载，不能替代 Unity/F6 或行为证据。

## 6. 问题分级和处理

### A 类：必须先汇报并暂停

以下情况不能用临时 hack 绕过：

1. ET.Skill.Editor 编译需要反向依赖 ET.Hotfix、ET.ModelView，或改变现有程序集拓扑。
2. AnimRes/NPK/Sprite 无法取得，或中心点、切片、像素密度、镜像、sorting 与运行时不一致。
3. AnimId 到 ani.bytes/overlay 的映射只能复制一份易漂移清单，必须决定共享清单归属。
4. 当前 SkillParams 无法表达已有技能行为：多次 Phase 重入、输入窗口、特殊抓取、帧盒切换等。
5. 必须改变 LSCast 快照、MemoryPackOrder、SkillContext 公共契约、回滚语义或全局事件语义。
6. 发现 atMs、Phase 局部时间、事件重复规则或 HitPolicy 与现有代码/技能测试矛盾。
7. 用户要求回写 NPK/动画资源、在 Editor 执行真实战斗、提前实现 Play IMGUI，或扩展 P2 边界。
8. 需要批量删除旧字段、覆盖未保存内容、改变 JSON 身份规则或扩大 CLI 写入/执行权限。

A 类上报后，停止受影响模块及其后续依赖模块；不继续堆 UI。由主 AI 汇报现象、最小复现、
影响、可选方案和需要用户确认的选择，确认后再修改文档或代码。

### B 类：记录后可继续

单个 Sprite 缺失、坏文件、列表为空、控件样式、滚动/缩放体验、缓存性能和非语义性 null
防护属于 B 类。应显示诊断或占位，登记问题编号和后续动作，不改变时间/坐标/数据契约。

### C 类：普通实现错误

明确的空引用、越界、重复绘制、RenderTexture 未释放等，可在当前阶段修复；修复后补最小
回归证据，不必暂停整体规划。

## 7. 上报模板

在 进度与问题记录.md 增加一条记录，至少填写：

~~~text
ISSUE-NNN
阶段/日期：
类别：A / B / C
现象：
最小复现：
预期与实际：
影响模块和既定决策：
可选方案：
需要确认的问题：
临时措施（如有）：
证据路径：
状态：待确认 / 处理中 / 已解决 / 暂缓
~~~

A 类记录必须保留用户确认结论和方案变更前后的文档差异。B/C 类记录必须写清是否影响
验收项，避免“已修复”与“已验证”混用。

## 8. P2 完成和交接

只有同时满足以下条件才可把 P2 标为完成：

- Step 0 到 Step 4 门槛全部通过；
- 代表性回归矩阵和全量目录扫描有证据；
- CLI 命令契约、路径限制、冲突和 dryRun 行为有证据；
- 仍为占位或尚未验证的 AnimRes、特殊技能和运行时行为已明确列出；
- 进度与问题记录.md 无未处理的 A 类问题，或每项都有用户确认的暂缓结论；
- P1-迁移对照表.md 已补齐到可追溯状态。

P2 完成后才创建 P3 Play IMGUI 任务；P4 的帧盒回写、nextSkill 转派和新机制模板另行立项。
