# Watcher 重启与首单联调 Runbook（给执行的 AI 读）

> 你在 **Unity 机**上。目标：用 watcher v2 重启从 AI 体系，并**手动首跑一次**把挂着的
> 0004 任务处理掉（手动跑能看见无头会话的真实输出，顺便当全链路验证）。
> 前置：`automation/worker/SETUP-FOR-AI.md` 的部署已完成过一次。
> 全程不改仓库文件；遇到与本文不符的现场 → 停下报告。

## 1. 更新到 v2

```powershell
$ROOT = "D:\Projects\et9lockStepYIUITest"
cd $ROOT
git pull --rebase
Copy-Item automation\worker\ui-worker.SKILL.md .claude\skills\ui-worker\SKILL.md -Force
Select-String -Path automation\worker\watcher.ps1 -Pattern "v2"        # 确认拿到新版
```

## 2. 模型冒烟（决定 watcher 用哪个模型）

```powershell
claude --version
claude -p "回复OK" --model haiku        # 限时等 60 秒
# 失败 → 再试：claude -p "回复OK" --model sonnet
# 再失败 → 停，报告"模型/中转不可用"，等人类处理
```

记下可用的模型名，后面两处用它（下称 `$M`，取值 haiku 或 sonnet）。

## 3. 停旧 watcher

```powershell
Stop-ScheduledTask -TaskName "UIWorkerWatcher" -ErrorAction SilentlyContinue
# 前台有 watcher 窗口的话，人工关掉那个窗口
```

## 4. 重启（两个形态都开）

```powershell
# 4a. 常驻（隐藏，登录自启）
Unregister-ScheduledTask -TaskName "UIWorkerWatcher" -Confirm:$false -ErrorAction SilentlyContinue
$action  = New-ScheduledTaskAction -Execute "pwsh" `
          -Argument "-File $ROOT\automation\worker\watcher.ps1 -ProjectRoot $ROOT -Model $M"
Register-ScheduledTask -TaskName "UIWorkerWatcher" -Action $action -Trigger (New-ScheduledTaskTrigger -AtLogOn)
Start-ScheduledTask -TaskName "UIWorkerWatcher"

# 4b. 可见窗口（给人类看心跳，调试期开着）
Start-Process pwsh -ArgumentList "-NoExit","-File","$ROOT\automation\worker\watcher.ps1","-ProjectRoot",$ROOT,"-Model",$M
```

自验：`Get-ScheduledTask -TaskName "UIWorkerWatcher" | Select-Object State` 应为 Running；
新开的 pwsh 窗口应打印 `[watcher] v2 启动 ...`。

## 5. 手动首跑 0004（关键验证，前台可见输出）

> 0004 还挂在总线上。下面这条和 watcher 唤醒的命令**内容一致**，只是你在前台跑、能看见全过程。
> 若权限拦截这条命令，请人类代跑并贴回输出。

```powershell
claude -p "按 ui-worker skill 处理 origin/automation 总线上的待办任务：0004。先 git fetch origin automation，任务内容用 git show origin/automation:automation/tasks/0004.yaml 读取。" --model $M
```

观察它是否：读单 → 执行 env_check → 写结果 → worktree 提交推送 automation 分支。
**它卡住/报错/权限被拒的原文就是排查依据**，完整贴回。

## 6. 验证结果

```powershell
git fetch origin automation
git ls-tree -r --name-only origin/automation -- automation/results
# 应出现 automation/results/0004.result.yaml
```

watcher 窗口此时若打印"发现待办: 0004"——说明它也要拉起 worker；同任务被你手动完成后
（result 已存在）worker 会发现无待办直接退出，属正常，不冲突。

## 7. 完成报告（输出给用户）

```
[ ] v2 已部署（git log -1 短哈希）
[ ] 可用模型：haiku / sonnet（另一个的报错原文）
[ ] UIWorkerWatcher：Running
[ ] 可见 watcher 窗口：已开
[ ] 0004 手动首跑：成功/失败（失败贴原文）
[ ] 0004.result.yaml 已上 automation 分支：是/否
```

## 禁止事项

- 禁改仓库文件；禁删 region；禁碰 automation/tasks/
- watcher 以外的 claude 进程不要杀（可能是在跑的 worker）
- 模型两个都不可用时**不要**自行改用其它未验证模型，停下报告
