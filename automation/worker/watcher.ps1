#!/usr/bin/env pwsh
# UI Worker watcher —— 轮询 automation/tasks，发现待办任务时唤醒无头 claude 会话
# 用途：Unity 机常驻。只做两件事：看有没有活、喊人干活。不解析任务内容。
# 详见 automation/PROTOCOL.md

param(
    # 项目根目录（Unity 机上改成实际路径）
    [string]$ProjectRoot = "E:\Projects\cs\et9lockStepYIUITest",
    # 扫描间隔（秒）
    [int]$IntervalSec = 15,
    # 从 AI 模型（任务确定性高，用低配模型省成本）
    [string]$Model = "haiku"
)

$tasksDir   = Join-Path $ProjectRoot "automation\tasks"
$resultsDir = Join-Path $ProjectRoot "automation\results"
$lockFile   = Join-Path $ProjectRoot "automation\worker\.worker.lock"

Write-Host "[watcher] 启动 root=$ProjectRoot interval=${IntervalSec}s model=$Model"

while ($true) {
    try {
        # 1) 锁检查：上一个 worker 进程还活着就跳过
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
            # 2) 找待办：tasks/ 里没有对应 result 文件的任务
            $pending = @()
            if (Test-Path $tasksDir) {
                $pending = Get-ChildItem $tasksDir -Filter *.yaml -ErrorAction SilentlyContinue | Where-Object {
                    -not (Test-Path (Join-Path $resultsDir ($_.BaseName + ".result.yaml")))
                }
            }

            if ($pending.Count -gt 0) {
                Write-Host ("[watcher] {0} 个待办任务（{1}），唤醒 worker..." -f $pending.Count, (($pending | ForEach-Object BaseName) -join ","))
                $proc = Start-Process -FilePath "claude" `
                    -ArgumentList @("-p", "按 ui-worker skill 处理 automation/tasks 中的待办任务", "--model", $Model) `
                    -WorkingDirectory $ProjectRoot `
                    -PassThru -WindowStyle Hidden
                Set-Content -Path $lockFile -Value $proc.Id
                Write-Host ("[watcher] worker 已启动 pid={0}" -f $proc.Id)
            }
        }
    }
    catch {
        Write-Host "[watcher] 异常: $_"
    }
    Start-Sleep -Seconds $IntervalSec
}
