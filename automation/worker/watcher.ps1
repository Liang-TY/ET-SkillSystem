#!/usr/bin/env pwsh
# UI Worker watcher v2 —— 任务总线 = origin/automation 分支（本地工作区是 dev 分支，
# 不包含 tasks），因此通过 git ls-tree 读远端总线状态，不触碰工作区。
# 发现待办 → 冷却检查（防 claude 秒退导致 15 秒重拉起风暴）→ 唤醒无头 claude。
# 前台调试：pwsh -File watcher.ps1 -ProjectRoot <工程根> [-Model haiku]
# 详见 automation/PROTOCOL.md

param(
    # 项目根目录（Unity 机上改成实际路径，或启动时传参）
    [string]$ProjectRoot = "E:\Projects\cs\et9lockStepYIUITest",
    # 扫描间隔（秒）
    [int]$IntervalSec = 15,
    # 两次唤醒的最小间隔（秒）：claude 启动即退（模型不可用/网络断）时不至于风暴
    [int]$CooldownSec = 90,
    # 从 AI 模型（任务确定性高，用低配模型省成本；不可用时换 sonnet 等）
    [string]$Model = "haiku",
    # worker 可执行入口：npm 安装的 Windows 机器上是 claude.cmd（裸 "claude" 可能解析到 .ps1 shim 被记事本打开）
    [string]$ClaudeExe = "claude.cmd"
)

$lockFile  = Join-Path $ProjectRoot "automation\worker\.worker.lock"
$stampFile = Join-Path $ProjectRoot "automation\worker\.last-spawn"

Write-Host "[watcher] v2 启动 root=$ProjectRoot interval=${IntervalSec}s cooldown=${CooldownSec}s model=$Model"

function Get-BusFiles([string]$sub) {
    # 返回 origin/automation 上 automation/<sub>/ 下的文件名（仅名字）
    $out = git -C $ProjectRoot ls-tree -r --name-only "origin/automation" -- "automation/$sub" 2>$null
    if (-not $out) { return @() }
    return @($out | ForEach-Object { Split-Path $_ -Leaf })
}

while ($true) {
    try {
        # 1) 上一个 worker 进程还活着吗
        $running = $false
        if (Test-Path $lockFile) {
            $lockedPid = Get-Content $lockFile -ErrorAction SilentlyContinue
            $lockedPidInt = 0
            if ($lockedPid -and [int]::TryParse($lockedPid, [ref]$lockedPidInt)) {
                if (Get-Process -Id $lockedPidInt -ErrorAction SilentlyContinue) { $running = $true }
            }
            if (-not $running) { Remove-Item $lockFile -ErrorAction SilentlyContinue }  # 陈旧锁清理
        }

        if (-not $running) {
            # 2) 冷却检查
            if (Test-Path $stampFile) {
                $last = (Get-Item $stampFile).LastWriteTime
                if (((Get-Date) - $last).TotalSeconds -lt $CooldownSec) {
                    Start-Sleep -Seconds $IntervalSec
                    continue
                }
            }

            # 3) 同步远端总线，找待办：tasks/*.yaml 无对应 results/*.result.yaml
            git -C $ProjectRoot fetch origin automation 2>$null | Out-Null
            $tasks   = Get-BusFiles "tasks"   | Where-Object { $_ -like "*.yaml" }
            $results = Get-BusFiles "results" | ForEach-Object { $_ -replace '\.result\.yaml$', '' }
            $pending = @($tasks | Where-Object {
                $base = $_ -replace '\.yaml$', ''
                $results -notcontains $base
            })

            if ($pending.Count -gt 0) {
                $ids = ($pending | ForEach-Object { $_ -replace '\.yaml$', '' }) -join ", "
                Write-Host ("[watcher] {0} 待办: {1} → 唤醒 worker (model={2})" -f (Get-Date -Format HH:mm:ss), $ids, $Model)
                $proc = Start-Process -FilePath $ClaudeExe `
                    -ArgumentList @(
                        "-p",
                        "处理 origin/automation 总线上的待办任务：$ids。先 git fetch origin automation，任务内容用 git show origin/automation:automation/tasks/<id>.yaml 读取；执行规范以仓库当前版本的 automation/worker/ui-worker.SKILL.md 为准（直接读取该文件，勿依赖本地副本）。",
                        "--model", $Model) `
                    -WorkingDirectory $ProjectRoot `
                    -PassThru -WindowStyle Hidden
                Set-Content -Path $lockFile -Value $proc.Id
                Set-Content -Path $stampFile -Value (Get-Date -Format o)
                Write-Host ("[watcher] worker 已启动 pid={0}" -f $proc.Id)
            }
        }
    }
    catch {
        Write-Host "[watcher] 异常: $_"
    }
    Start-Sleep -Seconds $IntervalSec
}
