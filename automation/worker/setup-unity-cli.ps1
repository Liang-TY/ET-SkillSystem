#!/usr/bin/env pwsh
# Unity CLI 一次性安装脚本（Unity 机上运行）
# 作用：安装 unity 二进制（beta 通道）到当前用户 PATH，并验证可用。
# 说明：CLI 是机器级工具、不进仓库；本脚本只是把官方安装命令封装一下，
#       重复运行安全（官方安装器自带升级/覆盖逻辑）。
# 对应文档：automation/worker/README.md §1

$ErrorActionPreference = 'Stop'

Write-Host "[setup] 安装 Unity CLI (beta 通道) ..."

$env:UNITY_CLI_CHANNEL = 'beta'
Invoke-RestMethod https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1 | Invoke-Expression

# 新开 shell 才有 PATH，这里直接探测常见安装位置做验证
$unityCmd = Get-Command unity -ErrorAction SilentlyContinue
if (-not $unityCmd) {
    $candidates = @(
        "$env:LOCALAPPDATA\UnityCLI\unity.exe",
        "$env:USERPROFILE\.unity-cli\unity.exe",
        "$env:ProgramFiles\Unity CLI\unity.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) { $unityCmd = $c; break }
    }
}

if ($unityCmd) {
    Write-Host "[setup] 完成: $unityCmd"
    & $unityCmd --version
    Write-Host ""
    Write-Host "[setup] 下一步（新开终端后 unity 命令全局可用）："
    Write-Host "  1. cd <项目根>"
    Write-Host "  2. unity pipeline list    # 确认项目已启用 pipeline（manifest 已含 com.unity.pipeline）"
    Write-Host "  3. 用 -automated 打开编辑器: unity open <项目根> -- -automated"
    Write-Host "  4. 编辑器开着时运行: unity command   # 应列出命令清单"
} else {
    Write-Warning "[setup] 安装脚本已执行，但未在 PATH/常见位置找到 unity。请新开一个终端运行 'unity --version' 验证。"
}
