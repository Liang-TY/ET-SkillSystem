# UnityBridge CLI 参考 (ET9)

## CLI 入口

```bash
dotnet ./Bin/ET.UBridge.dll <命令名> [--参数]
```

## 根目录

默认 `Temp/UnityBridge`，可通过环境变量 `ET_UNITY_BRIDGE_ROOT` 覆盖。

目录结构：
```
Temp/UnityBridge/
├── requests/      # CLI 写 JSON 请求
├── responses/     # Editor 写 JSON 响应
├── processing/    # 处理中（File.Move 原子锁）
├── deadletter/    # 失败请求
└── state/
    └── pending.json  # 延迟命令状态（域重载安全）
```

## 通用参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `--timeout` | 15000 | 超时毫秒数 |
| `--waitMs` | 100 | 轮询间隔毫秒 |

## 常用命令

| 命令 | 用途 | 示例 |
|------|------|------|
| `Ping` | 连通检测 | `Ping` |
| `HostState` | 返回状态 + 命令列表 | `HostState` |
| `ConsoleGetLogs` | 读控制台日志 | `ConsoleGetLogs --count 20 --logType Error` |
| `Compile` | 编译脚本（延迟） | `Compile --timeout 180000` |
| `Refresh` | 刷新资源（延迟） | `Refresh --timeout 60000` |
| `RegenProject` | 重生成项目文件（延迟） | `RegenProject --timeout 60000` |
| `ScreenshotCapture` | 截图 | `ScreenshotCapture --format png --allowEditMode true` |
| `SceneNew` | 新建场景 | `SceneNew` |
| `GameObjectCreate` | 创建对象 | `GameObjectCreate --name Cube` |
| `AssetSearch` | 搜索资源 | `AssetSearch --filter t:Scene` |

## 延迟命令处理

延迟命令（Compile/Refresh/RegenProject）返回 `{"_deferred":true}` 表示已接受。CLI 应继续轮询直到收到最终响应。

> ⚠️ `EnterPlay`/`ExitPlay` 在 ET9 中存在域重载后 pending 未清理的 bug，暂时不可用。

## 不可用命令

`EnterPlay`/`ExitPlay`（域重载 bug）、`GameView*`（未迁移）、`EditorLog`/`UnityTestRun`（未迁移）。

用 `dotnet ./Bin/ET.UBridge.dll HostState` 查看当前可用命令。

## 常见错误

| 错误码 | 说明 | 处理 |
|--------|------|------|
| 3 | 命令未找到 / 参数无效 | 检查命令名拼写，用 `HostState` 确认 |
| 7 | 不在 PlayMode | EnterPlay 先启动 |
| 8 | Handler 执行失败 | 查看 Unity Console 日志 |
| Timeout | 超时 | 检查 Unity 是否运行、Host 是否启动 |

## 响应解读

- `Error == 0` 且 `Message == null` → 成功
- `Error != 0` → 失败，查看 `Message`
- `{"_deferred":true}` → 延迟命令已接受，等待最终响应
