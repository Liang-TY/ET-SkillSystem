# S6 配置 Runbook（给执行配置的 AI 读）

> 你（读本文件的 AI）在 **Unity 机**上，任务是完成“从 AI 常驻体系”的一次性部署。
> 文档主人和验收方：主 AI（开发机）。完成后向用户报告，由主 AI 下第一张任务单联调。
> 人类只在需要批准权限时出现。全程在本机操作，**不改仓库任何文件**。

## 0. 前置自检（有任何一条失败 → 停下报告，不要继续）

```powershell
# 本机工程根（下文以 $ROOT 指代）
$ROOT = "D:\Projects\et9lockStepYIUITest"
cd $ROOT

git status --short          # 应干净；不干净先停下报告
git pull --rebase           # 确认拿到最新（含本文件）
claude --version            # Claude Code CLI 必须可用
unity --version             # 应显示 1.0.0-beta.6
unity command yiui_list_types   # 编辑器须开着且 pipeline 连通；应返回类型清单
```

## 1. 部署从 AI skill

```powershell
New-Item -ItemType Directory -Force -Path "$ROOT\.claude\skills\ui-worker"
Copy-Item "$ROOT\automation\worker\ui-worker.SKILL.md" "$ROOT\.claude\skills\ui-worker\SKILL.md"
```

自验：`Get-Content "$ROOT\.claude\skills\ui-worker\SKILL.md" -TotalCount 3` 应看到 frontmatter（name: ui-worker）。

## 2. 角色声明（~/.claude/CLAUDE.md）

**先看有没有现存文件**（存在则把下面内容追加到开头，不要覆盖原内容）：

```powershell
$p = "$env:USERPROFILE\.claude\CLAUDE.md"
$role = @"

本机角色=从AI(ui-worker)。本机运行 Unity 编辑器。
一切行为遵守 automation/PROTOCOL.md 与 ui-worker skill 白名单，只消费 automation/tasks/。
"@
if (Test-Path $p) { $role + "`n" + (Get-Content $p -Raw) | Set-Content $p -Encoding utf8 }
else { New-Item -ItemType Directory -Force -Path (Split-Path $p) | Out-Null; $role | Set-Content $p -Encoding utf8 }
```

## 3. 注册 watcher 常驻（注意路径用 D 盘实际值，勿用脚本内默认的 E 盘）

```powershell
$action  = New-ScheduledTaskAction -Execute "pwsh" `
          -Argument "-File $ROOT\automation\worker\watcher.ps1 -ProjectRoot $ROOT"
$trigger = New-ScheduledTaskTrigger -AtLogOn
Register-ScheduledTask -TaskName "UIWorkerWatcher" -Action $action -Trigger $trigger -Force
Start-ScheduledTask -TaskName "UIWorkerWatcher"
```

自验：

```powershell
Get-ScheduledTask -TaskName "UIWorkerWatcher"        # State 应为 Running
Get-ScheduledTaskInfo -TaskName "UIWorkerWatcher"    # LastTaskResult 应为 0 或 267009(运行中)
```

如需前台调试可另开终端跑 `pwsh -File $ROOT\automation\worker\watcher.ps1 -ProjectRoot $ROOT`，确认每 15 秒打印心跳且无异常。

## 4. 权限白名单（**必须最后做**——写完之后你自己的会话也被收窄）

写到 `.claude/settings.local.json`。**先读现有内容**：已存在则把 `permissions.allow` 数组合并去重（保留原有其它字段），不存在则创建整份：

```json
{
  "permissions": {
    "allow": [
      "Bash(git *)",
      "Bash(unity *)",
      "Bash(pwsh *)",
      "Write(automation/results/**)",
      "Read(**)",
      "Glob(**)",
      "Grep(**)"
    ]
  }
}
```

用 pwsh 写（Set-Content），**不要用你自己的 Write 工具**（写完白名单后 Write 只放行 automation/results，本文件不在其内）。

注意：无头模式（`claude -p`）下白名单外一律拒绝。之后想在 Unity 机开正常开发会话，人工临时改此文件即可。

## 5. 完成报告（向用户口头汇报，另在会话里输出以下清单）

```
[ ] 前置自检 5 项全过（列出 unity/claude 版本号）
[ ] skill 已部署：.claude/skills/ui-worker/SKILL.md
[ ] 角色声明已写入 ~/.claude/CLAUDE.md（新增/追加）
[ ] 计划任务 UIWorkerWatcher 注册并 Running（$ROOT 路径正确）
[ ] settings.local.json 白名单已合并写入
[ ] 本机编辑器状态：开着 / pipeline 端口可达
```

报告后停下。**不要自行下任务单测试**——第一张正式任务单（0004 env_check）由主 AI 在开发机下发，那才是 S6 联调。

## 禁止事项

- 禁改仓库内任何文件（含 watcher.ps1 的默认路径——用启动参数覆盖，不改文件）
- 禁覆盖已有配置文件（settings.local.json / ~/.claude/CLAUDE.md 只合并追加）
- 禁动任务总线目录（automation/tasks/ 只读）
- 遇到任何与本文不符的现场（路径不同、文件已存在且内容冲突、计划任务已存在）→ 停下报告，不自作主张
