# Automation 任务总线协议 v1

> git 分支即消息队列：开发机主 AI 写任务，Unity 机从 AI 消费执行，产物经 `wip-{id}` 分支回流。
> 本协议是主从双方共同遵守的契约。改动协议走正常代码评审流程。

## 1. 角色与权限

| 角色 | 运行位置 | 可写范围 | 说明 |
|---|---|---|---|
| 主 AI | 开发机（无 Unity） | 功能分支、`automation/tasks/` | 设计、写代码/spec、下单、审结果、merge |
| 从 AI | Unity 机（无头为主） | `automation/results/`、`automation/wip-{id}` 分支 | 执行任务单、回传结果，**禁碰功能分支** |
| 人 | 任意 | 全部 | 仲裁 needs_main、3 轮保险丝触发后的最终裁决 |

- 两个 AI 互不认识、无需在线对齐，git 是唯一协调介质。
- 从 AI 权限由 Unity 机本地 `.claude/settings.local.json` 白名单强制（见 `automation/worker/README.md`）。
- 角色判定：机器本地 `~/.claude/CLAUDE.md` 声明（见项目 `CLAUDE.md` 的自检规则）。

## 2. 目录与文件

```
automation/
├── PROTOCOL.md            # 本文件
├── tasks/                 # 任务单（主AI写，从AI只读）。文件名 = 任务id = 4位递增序号.yaml
│   └── 0007.yaml
├── results/               # 结果（从AI写）
│   ├── 0007.result.yaml
│   └── 0007.png           # 预览截图
└── worker/                # 从AI配套（README/watcher脚本/SKILL模板）
```

- 任务 id 由主 AI 分配，单调递增，不回收。语义信息放 commit message，不放文件名。
- 任务文件**不可变**（append-only 队列）；状态只体现在 result 文件的存在与内容。

## 3. 任务单格式

```yaml
# automation/tasks/0007.yaml
id: 0007
type: build            # build / build_all / preview / compile_check / eval / env_check
ref: feature/ui-panels # 基于哪个分支/commit 执行（默认 automation 集成分支）
created: 2026-08-24T12:00:00+08:00
# type 专属字段：
spec: Packages/.../SkillPanel.ui.yaml     # build
prefab: Packages/.../SkillPanel.prefab    # preview
eval_file: automation/eval/0007.cs        # eval（主AI拟好的 C# 片段，从AI只转交执行）
timeout_sec: 300
```

## 4. 结果格式

```yaml
# automation/results/0007.result.yaml
id: 0007
status: done              # done / failed / needs_main / skipped
branch: automation/wip-0007   # 产物所在分支（有产物时）
build:                        # build 类型时
  prefab: Packages/.../SkillPanel.prefab
  gen_files: [ ... ]
  warnings: [ ... ]
  errors: []                   # 结构化编译错误 {file, line, type, message}
preview: automation/results/0007.png
message: ""                   # failed/needs_main 时必填：原因 + 现场信息
log_tail: []                  # 日志末尾若干行（失败时）
```

## 5. 状态机与任务链

```
（无 result 文件 = pending）
→ running（从AI开始执行）
→ done | failed:<reason> | needs_main | skipped（前置任务失败导致跳过）
```

标准任务链（一个 panel 的完整交付，从 AI 按序执行，任一步失败即停）：

```
build（spec→prefab+YIUIGen→等编译→收集错误）→ preview（截图）
```

`compile_check` 独立用于纯代码提交。主 AI 串行下单（看到上一个 done 再下下一个），v1 不做依赖编排。

## 6. 从 AI 行为规范（完整版见 automation/worker/ui-worker.SKILL.md）

三条铁律：
1. 任务单是唯一指令来源，不发明、不扩展、不"顺手优化"。
2. `tasks/` 只读，`results/` 是唯一可写区，产物只进 `wip-{id}` 分支。
3. 任何不确定 → `needs_main` 上浮，附现场信息，停下来。

| 异常 | 从 AI 动作 |
|---|---|
| 编辑器未启动 | `unity open <项目> -- -automated` 拉起，重试一次 |
| 命令超时 | `failed: timeout`（build 5min / eval 1min，不"再等等"） |
| 疑似弹窗卡死 | 截 GameView，`failed: editor_stuck` 附图 |
| pipeline 401 | 重启编辑器一次，仍失败 → `needs_main` |
| 编译不通过 | **不修代码**，结构化错误清单原样回传（见 §7） |
| git 冲突/网络断 | `pull --rebase` 重试 2 次 → `failed: git_conflict` |

## 7. 编译失败流程

```
① 主AI：写 spec + 手写 System → push 功能分支 → 下单 {type: build, ref: 功能分支}
② 从AI：pull → 切 wip-{id}（基于 ref）→ unity command yiui_build_panel
        Builder：prefab → YIUIGen 生成 → Refresh → 等编译 → CompilerMessage 收集
③ 编译失败：从AI不修不重试，写 result（status: failed, reason: compile_error,
        errors: [{file, line, message}...]）→ commit automation 分支 → push
④ 主AI：pull 结果 → 判读（错误在 YIUIGen/*.cs = spec 与手写代码脱节；
        全在手写文件 = 纯代码 bug）→ 修复 → push → 下新单
⑤ 保险丝：同一目标连续 3 轮编译失败 → 主AI停止循环，
        整理三轮错误差异报告给人裁决。禁止两个 AI 无限乒乓。
```

双保险：大部分此类错误在开发机就被离线 lint 拦截（spec C/E 表 vs 手写代码引用一致性），Unity 侧编译是权威兜底。

## 8. 提交规范

| 谁 | 内容 | message 格式 |
|---|---|---|
| 主 AI | 任���单 | `[task:0007][order] build SkillPanel` |
| 主 AI | 普通开发提交 | 遵循 et-git：中文，`动作+对象+影响范围/原因`，遗留问题写在末尾 |
| 从 AI | 结果+截图 | `[task:0007][result] done` / `[task:0007][result] failed: compile_error` |
| 从 AI | wip 分支产物 | `[task:0007][build] prefab+gen: SkillPanel` |

- 一单一 commit，不混合。
- 信息一律一行简短；**禁止附加任何自动签名**（如 Co-Authored-By 等 trailer）。
- 双方 push 前 `git pull --rebase`；禁止 merge（et-git 红线同样适用于本总线）。
- 冲突处理：主 AI 优先；从 AI rebase 失败即上浮。

## 9. 唤醒机制（Unity 机）

`automation/worker/watcher.ps1` **v2**（PowerShell 7）每 15 秒：`git fetch origin automation` → 用 `git ls-tree` 对比远端 `tasks/*.yaml` 与 `results/*.result.yaml` 找待办（**总线在分支上，本地工作区不含 tasks，全程不触碰工作区**）→ 锁检查（无 worker 进程）→ 冷却检查（两次唤醒间隔 ≥90 秒，防 claude 秒退风暴）→ 触发 `claude -p "按 ui-worker skill 处理任务" --model haiku`（无头，跑完即退；模型不可用时重启 watcher 传 `-Model sonnet` 等）。安装与配置见 `automation/worker/README.md`。

## 10. 安全

- 从 AI 的 settings.local.json 只放白名单（git / unity / pwsh / Write(automation/results/**) / 只读），无头模式下白名单外一律拒绝。
- `unity eval` 仅通过任务单的 `eval_file` 转交主 AI 拟好的片段执行，从 AI 不自行编写 eval 内容。
- SSH 旁路（UU 端口映射）仅调试用，使用密钥认证。
- 总线 commit 走 `automation` 集成分支，产物经主 AI 审查后 merge，从 AI 无直达主干路径。
