# et-unitybridge — Unity Editor 桥接操作

## 何时使用

- 检测 UnityBridge 连通性、编译状态、PlayMode 状态
- 通过 CLI 操作 Unity Editor（资源/场景/GameObject/Transform/Inspector/Prefab 等）
- 执行 `Compile` / `Refresh` / `RegenProject` 延迟命令
- 排查 UnityBridge 错误

## 何时不加载

- 纯 C# 代码编译（不需要桥接）
- Excel/Luban 导出（用 et-excel / et-luban）
- 纯代码分析（用 et-code）
- 只读协议文档不做实际操作

## 默认操作

1. **CLI 入口：** `dotnet ./Bin/ET.UBridge.dll <命令> [--参数]`
2. **前置 Ping：** 执行任何操作前先 `dotnet ./Bin/ET.UBridge.dll Ping` 确认连通
3. **命令发现：** 用 `dotnet ./Bin/ET.UBridge.dll HostState` 查看所有可用命令
4. **延迟命令：** `Compile`/`Refresh`/`RegenProject` 返回 `{"_deferred":true}` 后，等待 Unity 响应，可能需要 `--timeout 120000`
5. **PlayMode 命令：** `EnterPlay`/`ExitPlay` 在 ET9 中存在域重载 bug，暂时不可用
6. **DLL 缺失：** 如 `Bin/ET.UBridge.dll` 不存在，触发 `et-build` 编译

## Token 节省规则

1. 不需要读全部 handler——用 `HostState` 查命令列表，按需用 `rg` 查具体 handler
2. 不需要读全部 proto——按命令名搜 `UBridge_C_10000.proto`
3. 批量操作用 `BatchExecute`
4. 不要 dump 完整 JSON 响应——只提取关键字段
5. 优先用命令而非 GUI 点击

## 最小流程

```powershell
# 检测连通
dotnet ./Bin/ET.UBridge.dll Ping

# 查看所有可用命令
dotnet ./Bin/ET.UBridge.dll HostState

# 读取控制台日志
dotnet ./Bin/ET.UBridge.dll ConsoleGetLogs --count 10 --logType Error
```

## 不可用的 ET10 命令（未迁移 / 有 bug）

以下 ET10 `cn.etetet.unitybridge` 的命令在 ET9 `cn.etetet.ubridge` 中**不可用**：

| 命令 | 原因 |
|------|------|
| `EnterPlay` / `ExitPlay` | 域重载后 pending 未清理的 bug，待修复 |
| `GameViewGetResolution` / `ListResolutions` / `SetResolution` | 尚未移植 |
| `EditorLog` / `UnityTestRun` | 尚未移植 |
| `QueryHostState` | 已改名为 `HostState` |

使用 `dotnet ./Bin/ET.UBridge.dll HostState` 可查看当前实际可用命令。

## 参考文档

- `references/et-unitybridge-ai-ops.md` — AI 操作模式
- `references/et-unitybridge-cli.md` — CLI 命令参考
- `../README.md` — 完整命令清单与架构说明
