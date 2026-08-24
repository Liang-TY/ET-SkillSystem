# 0006 未回传诊断 Runbook（给执行的 AI 读）

> 你在 **Unity 机**上。背景：主 AI 01:57 下单 0006（build TestManyPanel），02:06 验收时
> 无 result、无 wip-0006 分支 = **自动触发链未动作**。0004 曾手动跑通，故断点大概率在
> watcher 自动触发环节。你的任务：定位断点 → 修复 → 让 0006 真正完成 → 输出结论。
> 人类会把你的结论转发给主 AI。禁改仓库文件、禁动任务单、禁杀非 watcher 的 claude 进程。

## 0. 前置

```powershell
$ROOT = "D:\Projects\et9lockStepYIUITest"
cd $ROOT
git pull --rebase
```

## 1. 分叉定位（一分钟出结论）

```powershell
Get-Item "$ROOT\automation\worker\.last-spawn" | Select-Object LastWriteTime
Get-ScheduledTask -TaskName "UIWorkerWatcher" | Select-Object State
```

- **A. `.last-spawn` 不存在或早于 01:57** → watcher 从未拉起 worker，跳 §2；
- **B. `.last-spawn` 晚于 01:57 且在反复更新** → watcher 在拉起但 worker 秒退，跳 §3；
- 计划任务 State 不是 Running → 先查这个（服务被停了），修复 = Start-ScheduledTask。

## 2. 分支 A：watcher 没识别到任务

复现它的检测逻辑（在工程根手动执行）：

```powershell
git fetch origin automation
git ls-tree -r --name-only origin/automation -- automation/tasks     # 应有 0006.yaml
git ls-tree -r --name-only origin/automation -- automation/results   # 不应有 0006.result.yaml
```

- `fetch` 报错 / 无输出（PATH、凭据、网络）→ **这就是根因**：watcher 的 `2>$null` 把错误吞了，
  读的是陈旧 origin/automation。修复后在**新开的 pwsh**（模拟计划任务上下文）里重跑这三条确认通；
- 三条都对、能看出 0006 待办，但 watcher 就是不拉 → 前台起一个 watcher 调试窗口观察一轮：

```powershell
Start-Process pwsh -ArgumentList "-NoExit","-File","$ROOT\automation\worker\watcher.ps1","-ProjectRoot",$ROOT
```

记录它打印了什么（启动行？发现待办？异常？），然后停掉计划任务避免双实例抢锁，调试完恢复。

## 3. 分支 B：worker 秒退（重点嫌疑：模型）

前台手跑与 watcher 完全相同的命令，**等满 6 分钟**（build 是重活）：

```powershell
claude -p "按 ui-worker skill 处理 origin/automation 总线上的待办任务：0006。先 git fetch origin automation，任务内容用 git show origin/automation:automation/tasks/0006.yaml 读取。" --model haiku
```

- 若它正常完成（拉 dev 分支 pull、`unity command yiui_build_panel --spec Packages/cn.etetet.lockstep/Assets/GameRes/YIUI/TestMany/TestManyPanel.ui.yaml`、
  wip-0006 产物分支、结果+PNG 上 automation）→ 问题只在"无头+watcher 上下文"，对比差异（如 cwd、PATH）；
- 若报 `unrecognized_model` 类硬错 → 换 `--model sonnet` 重试一次；成功则把计划任务改用 sonnet：

```powershell
Unregister-ScheduledTask -TaskName "UIWorkerWatcher" -Confirm:$false
$action = New-ScheduledTaskAction -Execute "pwsh" -Argument "-File $ROOT\automation\worker\watcher.ps1 -ProjectRoot $ROOT -Model sonnet"
Register-ScheduledTask -TaskName "UIWorkerWatcher" -Action $action -Trigger (New-ScheduledTaskTrigger -AtLogOn)
Start-ScheduledTask -TaskName "UIWorkerWatcher"
```

- 其它报错 → 原文完整记入结论。

## 4. 收尾验证

```powershell
git fetch origin automation
git ls-tree -r --name-only origin/automation -- automation/results   # 应出现 0006.result.yaml
git ls-remote --heads origin | Select-String wip-0006                 # 应有产物分支
```

注：你手动跑完 0006 后，watcher 若也拉起一个 worker，它会发现无待办直接退出——正常。

## 5. 结论报告（原样输出给用户，用户转发主 AI）

```
[断点] A(watcher未识别) / B(worker秒退) / 计划任务停了 / 其它
[根因] 一句话（含关键报错原文）
[修复] 做了什么（模型换没换、任务重注册没有、PATH 处理）
[0006] 已完成(wip分支+result都在) / 未完成(卡在哪一步)
[耗时] 0006 从下单到回传实际分钟数（若已回传）
[遗留] 需要主 AI 处理的事项
```
