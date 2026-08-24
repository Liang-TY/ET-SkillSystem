# Unity 机从 AI 安装与配置

> 本目录是**模板仓库**。以下步骤在 Unity 机上执行一次。

## 0. 前置

- Unity 编辑器可正常打开本项目（6000.0.25f1）
- PowerShell 7（`pwsh`）
- 已装 Claude Code CLI（`claude` 可用）

## 1. 安装 Unity CLI（beta 通道）

一条命令（封装好的脚本，重复运行安全）：

```powershell
pwsh -File automation/worker/setup-unity-cli.ps1
```

或手动执行官方安装命令：

```powershell
$env:UNITY_CLI_CHANNEL='beta'; irm https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1 | iex
unity --version
```

## 2. 项目内启用 pipeline

`com.unity.pipeline` 已写入 `Packages/manifest.json`（0.5.0-exp.1，Unity 6.0 LTS+ 可用），**仓库拉取后打开工程即自动安装**。验证：

```powershell
cd <项目根>
unity pipeline list     # 应显示本项目已启用
```

之后编辑器用 `-automated` 参数启动（自动处理弹窗）：

```powershell
unity open <项目根> -- -automated
```

验证连通：编辑器开着时执行 `unity command`，应列出编辑器暴露的命令清单（含后续注册的 yiui_* 命令）。

## 3. 部署从 AI skill

```powershell
# 模板 → Unity 机本地 skill 目录
Copy-Item automation/worker/ui-worker.SKILL.md .claude/skills/ui-worker/SKILL.md
```

## 4. 收紧从 AI 权限

Unity 机项目目录 `.claude/settings.local.json`（不进 git，本机生效）：

```json
{
  "permissions": {
    "allow": [
      "Bash(git *)",
      "Bash(unity *)",
      "Bash(pwsh *)",
      "Edit(automation/results/**)",
      "Read(**)",
      "Glob(**)",
      "Grep(**)"
    ]
  }
}
```

原理：无头模式（`claude -p`）下白名单外的一切操作直接拒绝，无人工确认环节——这就是"从 AI 改不了代码"的强制层。

> 想在 Unity 机开正常开发会话时：临时改此文件放开权限，或直接用 `--dangerously-skip-permissions` 自担风险。

## 5. 角色声明

Unity 机用户级 `~/.claude/CLAUDE.md`（不存在则创建）：

```
本机角色=从AI(ui-worker)。本机运行 Unity 编辑器。
一切行为遵守 automation/PROTOCOL.md 与 ui-worker skill 白名单，只消费 automation/tasks/。
```

## 6. 启动 watcher

```powershell
# 前台试跑
pwsh -File automation/worker/watcher.ps1

# 常驻：注册为计划任务（登录时启动）
$action  = New-ScheduledTaskAction -Execute "pwsh" -Argument "-File <项目根>\automation\worker\watcher.ps1"
$trigger = New-ScheduledTaskTrigger -AtLogOn
Register-ScheduledTask -TaskName "UIWorkerWatcher" -Action $action -Trigger $trigger
```

注意把脚本内 `$ProjectRoot` 改成本机实际路径。

## 7. 全链路自检

1. 在任意机器上向 `automation/tasks/` 提交 `0001.yaml`（type: env_check）并 push。
2. Unity 机 watcher 15 秒内唤醒从 AI。
3. 开发机 pull `automation/results/0001.result.yaml`，应看到 `status: done` 及编辑器/pipeline 状态。
4. 若失败，按 result 的 message 排查（常见：编辑器未开、pipeline 未装、权限白名单路径不对）。

## 已知坑（排障记录）

| 症状 | 根因 | 修复 |
|---|---|---|
| watcher 唤醒后 .worker.lock 长期占着、无 result | `Start-Process "claude"` 解析到 npm shim 的 claude.ps1，.ps1 关联记事本 → 假进程永不退出 | 真 claude.exe 前插 PATH；仓库侧 watcher 已参数化 `$ClaudeExe`（默认 claude.cmd） |
| worker 被拉起但秒退、无任何输出 | 网关 `ANTHROPIC_*` 环境变量只在交互 profile，**计划任务上下文没有** | env 并入 `~/.claude/settings.json` 的 env 块（上下文无关）；用 `pwsh -NoProfile` 模拟验证 |
