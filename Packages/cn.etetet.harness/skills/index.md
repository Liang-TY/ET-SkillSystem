# ET9 Skills 索引

> 从 ET10 `cn.etetet.harness/skills/index.md` 适配到 ET9。

先读本文件，禁止先完整读取所有 skill 正文。

## 加载策略

1. 先用场景匹配 1 个主 skill；只有跨域任务才叠加其它 skill
2. 先读命中的 `SKILL.md`；需要细节时再读该 skill 的 `references/*.md`
3. 能直接调用现成脚本或 CLI 时，优先现成入口，不重复展开长命令
4. 所有命令必须使用 `pwsh`（PowerShell 7），不要使用 Windows 自带的 `powershell.exe`
5. 涉及 Unity 编辑器内操作时，优先使用 `et-unitybridge`；命中后读取 `Packages/cn.etetet.ubridge/AGENTS.md`
6. 修改或新增 C# 代码时默认遵守"每个类一个文件"

## 任务入口

### 核心开发

- `et-code`
  - 场景：写/改任何 C#（Entity/System 分层、包结构、分析器红线）、创建新包、asmdef/asmref
  - 补读：`skills/et-code/SKILL.md`，细节 `skills/et-code/references/et-code-rules.md`
- `et-async`
  - 场景：`async` / `await` / `ETTask`、`EntityRef` await 安全、并发等待
  - 补读：`skills/et-async/SKILL.md`
- `et-yiui`
  - 场景：UI 面板（YIUI 体系、u_Com/u_Event 绑定、循环列表、.ui.yaml spec 编写）
  - 补读：`skills/et-yiui/SKILL.md`，拼 UI 方案 `Notes/UIBuilder-P1实施方案.md`

### 远程执行（开发机无 Unity）

- `automation` 任务总线
  - 场景：编译、构建、截图、任何需要 Unity 的操作
  - 协议：`automation/PROTOCOL.md`（任务单/结果/提交规范/失败流程/保险丝）
  - 从 AI（Unity 机）配置模板：`automation/worker/`

### Unity 编辑器操作

- `et-unitybridge`
  - 场景：查询 Unity 状态、操作 Editor（资源/场景/GameObject/Transform/Inspector/Prefab/截图）、执行延迟命令、排查桥接错误
  - 补读：`Packages/cn.etetet.ubridge/AGENTS.md`

### 构建与导出

- `et-build`
  - 场景：编译（`dotnet build ET.sln`）、导出 Proto
  - 补读：`skills/et-build/SKILL.md`
- `et-luban` — 待迁移

### 数据与提交

- `et-excel`
  - 场景：创建/编辑/读取 Excel 文件（`.xlsx`、`.csv`、`.tsv`），写入数据、格式化、公式、图表
  - 路由：使用 Claude Code 内置 **xlsx skill** 处理
  - 补读：`skills/et-excel/SKILL.md`
  - 依赖：Python 3.11+、openpyxl、pandas
- `et-git`
  - 场景：提交前检查、整理 `git status` / `git diff`、筛除无关文件、中文提交信息、rebase 同步
  - 补读：`skills/et-git/SKILL.md`

## 组合场景

- 改 ET 代码：`et-code` → 涉及异步叠加 `et-async` → `et-build`
- 做 UI：`et-yiui` → 写 spec → lint → `automation` 下单 build → 看截图迭代
- Unity 编辑器操作：`et-unitybridge`（回退通道）→ 确认就绪后执行
- 读写 Excel：`et-excel`（xlsx skill）
- 准备提交：`et-git`

## xlsx skill 依赖说明

用于 `et-excel` 的 Claude Code 内置 xlsx skill 需要以下环境：

| 依赖 | 版本 | 说明 |
|------|------|------|
| Python | 3.11+ | 运行时 |
| openpyxl | 最新 | Excel 创建/编辑/格式化 |
| pandas | 最新 | 数据分析与批量操作 |

本环境已安装：
- `C:\Users\Liang\AppData\Local\Programs\Python\Python311\python.exe`
- openpyxl ✅
- pandas ✅
