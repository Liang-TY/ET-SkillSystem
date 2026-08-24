# ET-SkillSystem 项目规范（AI 会话入口）

基于 ET9 帧同步的 2D 像素网游（借鉴 DNF）。Unity 6000.0.25f1，UI 框架 YIUI。

## 角色自检（新会话第一件事）

1. 读 `~/.claude/CLAUDE.md` 的角色声明；不存在则自检：
   - 本机无法运行 `unity` CLI 且无工程 `Library/` → 本会话为**主 AI**（开发机，无 Unity）
   - 本机有 Unity 环境 → 按 `automation/PROTOCOL.md` 从 AI 规范行事，或询问用户确认
2. 主 AI 职责：设计、写代码/spec、离线 lint、下任务单、审查结果、merge 产物。
   **本机无 Unity：任何需要编译/构建/看运行效果的操作，一律走 `automation/` 任务总线，不要假设能本地做。**

## 规范路由（先读索引，再按需读 skill 正文）

统一入口：`Packages/cn.etetet.harness/skills/index.md`

- 写/改 C# → `et-code`；涉及 async/ETTask/EntityRef → 叠加 `et-async`
- YIUI / UI 面板 / spec → `et-yiui`；拼 UI 方案 → `Notes/UIBuilder-P1实施方案.md`
- 提交 → `et-git`；Unity 编辑器操作（回退通道）→ `et-unitybridge`
- 命令一律 `pwsh`（PowerShell 7）；每个类一个文件

## ET 分析器红线（写码时前置遵守；编译期由分析器兜底）

| 红线 | 规则 |
|---|---|
| Entity 纯数据 | ET0006 不声明方法；ET0010 不声明委托 |
| 跨 Entity 引用 | ET0020 必须 `EntityRef<T>`，不能直接持有 |
| 友元访问 | ET0002 访问他人内部必须 `[FriendOf]` |
| 父子关系 | ET0001/0007 `AddChild/AddComponent` 需对应 `[ChildOf]/[ComponentOf]` |
| 帧同步数值 | ET0023 LSEntity 禁 float/double（用 TSMath） |
| 异步纪律 | ET0008/0009 ETTask 必须 await 或 `.Coroutine()`；ET0016 await 后查 CancelToken |
| 静态字段 | ET0015 必须标 `[StaticField]` |
| 分层 | ET0032 Model 只放 Entity；ET0005 Hotfix 只放静态类；ET0022 Server 不引用 ET.Client |
| 热更安全 | ET0004 Hotfix 无非 const 实例字段 |
| 实例化 | ET0031 `[DisableNew]` 类走工厂/对象池 |

完整 32 条：`Notes/ET框架分析器规则详解.md`

## 提交规范

- **一行简短**：中文一句话（`动作 + 对象 + 影响范围/原因`），不写正文清单；确有遗留问题才在末尾加 `遗留问题：...`
- **禁止附加任何自动签名**（如 Co-Authored-By 等 trailer）
- **禁止 `git pull`**（只用 `--rebase`）、**禁止 merge**
- 自动化任务相关：`[task:id][order|result] ...`，见 `automation/PROTOCOL.md`

## UI 体系决策

- **全部 YIUI**。UILSLogin/UILSLobby/UILSRoom 冻结不再加功能；cn.etetet.ui 包停用
- UI 拼装：spec（`*.ui.yaml`，唯一真源）→ UIBuilder（cn.etetet.uibuilder）→ prefab + YIUIGen 代码
- 初期无贴图（自带控件外观量产），贴图 pass 后补；像素风暂缓
- YIUIGen 生成文件禁止手改；手写逻辑放 YIUISystem/YIUIComponent

## 任务总线（主 AI 日常驱动 Unity 侧执行的方式）

协议：`automation/PROTOCOL.md`。要点：任务单进 `automation/tasks/`，从 AI（Unity 机）消费并回传结果到 `automation/results/`，产物在 `automation/wip-{id}` 分支等主 AI merge。同一目标连续 3 轮编译失败 → 停止循环报告用户。
