# UnityBridge AI 操作模式 (ET9)

## 核心原则

1. **先读后写**：写操作前先 `Ping` 确认状态
2. **验证写操作**：写后检查 `Error==0`
3. **延迟命令等待**：`Compile`/`Refresh` 等返回 `{"_deferred":true}` 后需等待
4. **超时设置**：延迟命令 `--timeout 120000`，同步命令默认 15s

## 命令发现

```bash
# 查看所有可用命令
dotnet ./Bin/ET.UBridge.dll HostState

# 搜索特定命令
rg "class UBridgeXxxHandler" ./Packages/cn.etetet.ubridge/Scripts/ModelView/Client/
```

## 任务路由表

| 任务类别 | 命令族 | 说明 |
|----------|--------|------|
| 连通检测 | `Ping` | 首选，返回编译/PlayMode/Unity版本 |
| 命令发现 | `HostState` | 返回完整命令列表 |
| 编译/刷新 | `Compile`, `Refresh`, `RegenProject` | 延迟命令，需 `--timeout 120000` |
| 控制台 | `ConsoleGetLogs` | `--count 20 --logType Error` |
| 资源 | `AssetSearch/Find/GetPath/Load/ReadText/Import/Refresh` | Import/Refresh 为延迟命令 |
| 场景 | `SceneGetHierarchy/GetActive/Load/Save/New` | |
| 选中 | `SelectionGet/Set/Add/Remove/Clear` | |
| GameObject | `GameObjectCreate/Destroy/Find/GetInfo/Rename/Duplicate/SetActive` | |
| Transform | `TransformGet/SetPosition/Rotation/Scale/Parent/Sibling/LookAt/Reset` | |
| Inspector | `InspectorGetComponents/Properties/Property/FindProperty/SetProperty/SetProperties/AddComponent/RemoveComponent` | |
| Prefab | `PrefabInstantiate/Save/Apply/Unpack/GetInfo/GetHierarchy` | |
| 截图 | `ScreenshotCapture` | `--format png --allowEditMode true` |
| 撤销/重做 | `EditorUndo`, `EditorRedo` | |
| 暂停 | `EditorPause` | |
| 编辑器状态 | `EditorGetState` | |
| 菜单执行 | `MenuItemExecute` | `--menuPath "File/Save"` |
| 批量操作 | `BatchExecute` | 多个子命令一次执行 |
| 热更重载 | `Reload` | 需 PlayMode |

## 操作模式

### 读取状态

```bash
dotnet ./Bin/ET.UBridge.dll Ping
dotnet ./Bin/ET.UBridge.dll EditorGetState
```

### 延迟命令

```bash
# 编译（等待编译完成）
dotnet ./Bin/ET.UBridge.dll Compile --timeout 180000

# 刷新资源
dotnet ./Bin/ET.UBridge.dll Refresh --timeout 60000
```

### 创建 + 修改

```bash
# 创建 Cube
dotnet ./Bin/ET.UBridge.dll GameObjectCreate --name MyCube

# 修改位置（获取 InstanceId 后）
dotnet ./Bin/ET.UBridge.dll TransformSetPosition --instanceId -1234 --local true
```

### 截图

```bash
dotnet ./Bin/ET.UBridge.dll ScreenshotCapture --format png --allowEditMode true
```

## 常见错误处理

| 错误 | 含义 | 处理 |
|------|------|------|
| `CommandNotFound (3)` | 命令名错误 | `HostState` 查正确名称 |
| `NotInPlayMode (7)` | 需 PlayMode | 忽略或先 EnterPlay |
| `HandlerFail (8)` | Handler 异常 | 读 Unity Console 日志 |
| Timeout | 超时 | Editor 可能离线，`Ping` 确认 |
| `Already compiling` | 编译中 | 等待后再试 |
| `{"_deferred":true}` | 不是错误 | 延迟命令的确认响应 |

## 输出要求

- 总结结果，不要 dump 完整 JSON
- 关键字段只提取需要的部分
- 写操作后提供验证方式
- 不要降级为 GUI 点击（除非用户明确要求）
