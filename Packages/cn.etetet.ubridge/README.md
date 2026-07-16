# cn.etetet.ubridge — Unity Editor 文件 IPC 桥接

> 从 ET10 `cn.etetet.unitybridge` v2.0.0 迁移到 ET9，裁剪为极简版。
> 源项目：`D:\Projects\et10initLockStepYIUI-UITest\Packages\cn.etetet.unitybridge`

## 是什么

通过**文件系统**实现 CLI 进程 ↔ Unity Editor 之间的 RPC 通信。
CLI 写 JSON 文件到 `Temp/UnityBridge/requests/`，Editor 每 200ms 轮询处理，响应写回 `Temp/UnityBridge/responses/`。
无网络、无 HTTP、无管道。`File.Move` 做原子锁，MongoDB BSON 做序列化。

**当前状态：50 个命令可用（+3 延迟引擎就绪 +2 EnterPlay/ExitPlay 域重载 bug 待修），13 个 Handler 文件，~3000 行代码。**

## 目录结构

```
cn.etetet.ubridge/
├── Proto/
│   └── UBridge_C_10000.proto          # 所有命令的 proto 消息定义
├── Scripts/
│   ├── Model/Share/                    # → ET.Model 程序集
│   │   ├── UBridgeStorage.cs           # 文件 IO + 原子锁 + BSON 序列化
│   │   ├── UBridgeCommands.cs          # 请求/响应信封 + Handler 接口
│   │   └── UBridgeErrorCode.cs         # 错误码常量
│   └── ModelView/Client/               # → ET.ModelView 程序集
│       ├── UBridgeEditorHost.cs        # [InitializeOnLoad] 轮询 + 分发
│       ├── UBridgePingHandler.cs
│       ├── UBridgeConsoleGetLogsHandler.cs
│       ├── UBridgeScreenshotCaptureHandler.cs
│       ├── UBridgeMenuItemExecuteHandler.cs
│       ├── UBridgeSceneHandlers.cs           # Scene ×5
│       ├── UBridgeSelectionHandlers.cs      # Selection ×5
│       ├── UBridgeAssetHandlers.cs          # Asset ×5
│       ├── UBridgeGameObjectHandlers.cs     # GameObject ×7
│       ├── UBridgeTransformHandlers.cs      # Transform ×8
│       ├── UBridgePrefabHandlers.cs         # Prefab ×6
│       ├── UBridgeInspectorHandlers.cs      # Inspector ×8
│       ├── UBridgeEditorHandlers.cs         # Editor 控制 ×5
│       ├── UBridgeDeferredRuntime.cs        # 延迟命令引擎
│       └── UBridgeDeferredHandlers.cs       # 延迟命令 ×5
└── DotNet~/                            # CLI 控制台应用
    ├── ET.UBridge.csproj               # net8.0，引用 ET.Core.csproj
    └── Program.cs                      # 入口 + 参数解析 + 请求/响应轮询
```

## 已实现的命令

### 系统（6）

| 命令 | 用途 | 示例 |
|------|------|------|
| `Ping` | 连通检测 + Editor 状态 | `Ping` |
| `ConsoleGetLogs` | 读取控制台日志 | `ConsoleGetLogs --count 50 --logType Error` |
| `ScreenshotCapture` | 截取 Game View | `ScreenshotCapture --format png --allowEditMode true` |
| `MenuItemExecute` | 执行 Unity 菜单项 | `MenuItemExecute --menuPath "File/Save"` |
| `HostState` | 返回 Editor 状态 + 命令列表 | `HostState` |
| `BatchExecute` | 批量执行子命令 | `BatchExecute` |

### 场景（5）

| 命令 | 用途 |
|------|------|
| `SceneGetHierarchy` | 获取层级树 |
| `SceneGetActive` | 当前激活场景 |
| `SceneLoad` | 加载场景 |
| `SceneSave` | 保存场景 |
| `SceneNew` | 新建场景 |

### 选中（5）

| 命令 | 用途 |
|------|------|
| `SelectionGet` | 获取选中 |
| `SelectionSet` | 设置选中 |
| `SelectionAdd` | 添加到选中 |
| `SelectionRemove` | 从选中移除 |
| `SelectionClear` | 清除选中 |

### 资源（5）

| 命令 | 用途 |
|------|------|
| `AssetSearch` | 搜索资源 |
| `AssetFind` | 查找资源 |
| `AssetGetPath` | GUID→路径 |
| `AssetLoad` | 加载资源 |
| `AssetReadText` | 读取文本资源 |
| `AssetImport` | 导入资源（延迟） |
| `AssetRefresh` | 刷新资源（延迟） |

### GameObject（7）

| 命令 | 用途 |
|------|------|
| `GameObjectCreate` | 创建 |
| `GameObjectDestroy` | 销毁 |
| `GameObjectFind` | 查找 |
| `GameObjectGetInfo` | 获取信息 |
| `GameObjectRename` | 重命名 |
| `GameObjectDuplicate` | 复制 |
| `GameObjectSetActive` | 设置激活 |

### Transform（8）

| 命令 | 用途 |
|------|------|
| `TransformGet` | 获取 |
| `TransformSetPosition` | 设位置 |
| `TransformSetRotation` | 设旋转 |
| `TransformSetScale` | 设缩放 |
| `TransformSetParent` | 设父节点 |
| `TransformSetSiblingIndex` | 设同级索引 |
| `TransformLookAt` | 朝向 |
| `TransformReset` | 重置 |

### Prefab（6）

| 命令 | 用途 |
|------|------|
| `PrefabInstantiate` | 实例化到场景 |
| `PrefabSave` | 保存为 Prefab 资产 |
| `PrefabApply` | 应用覆盖 |
| `PrefabUnpack` | 解包 |
| `PrefabGetInfo` | 获取 Prefab 状态 |
| `PrefabGetHierarchy` | 遍历层级树 |

### Inspector（8）

| 命令 | 用途 |
|------|------|
| `InspectorGetComponents` | 列出组件 |
| `InspectorGetProperties` | 遍历所有属性 |
| `InspectorGetProperty` | 按名查属性 |
| `InspectorFindProperty` | 关键词搜索 |
| `InspectorSetProperty` | 设置属性值 |
| `InspectorSetProperties` | 批量设置 |
| `InspectorAddComponent` | 添加组件 |
| `InspectorRemoveComponent` | 移除组件 |

### Editor 控制（5）

| 命令 | 用途 |
|------|------|
| `Reload` | PlayMode 中重载热更 DLL |
| `EditorUndo` | 撤销 |
| `EditorRedo` | 重做 |
| `EditorPause` | 暂停/恢复 |
| `EditorGetState` | 快照编辑器状态 |

### 延迟命令（5，⚠️ EnterPlay/ExitPlay 域重载 bug 待修）

| 命令 | 用途 | 状态 |
|------|------|------|
| `Compile` | 编译脚本 | ⚠️ 引擎就绪 |
| `Refresh` | 刷新资源 | ⚠️ 引擎就绪 |
| `RegenProject` | 重生成项目文件 | ⚠️ 引擎就绪 |
| `EnterPlay` | 进入 PlayMode | ❌ 域重载 bug |
| `ExitPlay` | 退出 PlayMode | ❌ 域重载 bug |

> 所有命令前缀：`dotnet Bin/ET.UBridge.dll <命令名> [--参数]`

## 添加新命令（步骤模板）

### 1. 加 Proto 消息

在 `Proto/UBridge_C_10000.proto` 末尾追加 Request + Response message。

```protobuf
// ResponseType XxxResponse
message XxxRequest // IRequest
{
    int32 RpcId = 1;
    // 你的参数...
}

message XxxResponse // IResponse
{
    int32 RpcId = 1;
    int32 Error = 2;
    string Message = 3;
    // 你的返回值...
}
```

[USER: 请在文档末尾添加规则]

当修改 proto 文件中的请求结构体（Request message）时，如有需要，必须同步更新 DotNet~/Program.cs 中该命令的 payloadJson 构造代码，确保 CLI 发送的参数与 proto 定义一致。

### 2. 运行 Proto 生成

```bash
dotnet Packages\cn.etetet.proto\DotNet~\Exe\ET.Proto2CS.dll
```

### 3. 写 Handler

在 `Scripts/ModelView/Client/` 新建 `UBridgeXxxHandler.cs`：

```csharp
using UnityEditor;
// ... 其他 using

namespace ET
{
    public static class UBridgeXxxHandler
    {
        public static string Handle(string payloadJson)
        {
            XxxRequest request = UBridgeJsonHelper.FromJson<XxxRequest>(payloadJson);
            XxxResponse response = XxxResponse.Create();

            // 业务逻辑...

            return UBridgeJsonHelper.ToJson(response);
        }
    }
}
```

规则：
- 类必须是 `public static`，方法签名必须是 `public static string Handle(string payloadJson)`
- Proto 类型用 `XxxRequest.Create()` / `XxxResponse.Create()`，**禁止 `new`**
- 返回值用 `UBridgeJsonHelper.ToJson(response)` 序列化
- 同步命令直接返回；延迟命令暂不支持（需移植 Deferred 机制）

### 4. 注册 Handler

在 `UBridgeEditorHost.cs` 的 `EnsureInitialized()` 中加一行：

```csharp
RegisterHandler("Xxx", UBridgeXxxHandler.Handle);
```

### 5. 更新 CLI

在 `DotNet~/Program.cs` 中：

**a)** 加参数变量（如有新参数）：

```csharp
string xxxParam = "default";
```

**b)** 加参数解析：

```csharp
case "--xxxParam" when i + 1 < args.Length: xxxParam = args[++i]; break;
```

**c)** 加 payload 构造（switch 中）：

```csharp
case "Xxx":
    payloadJson = $"{{\"_t\":\"ET.XxxRequest\",\"RpcId\":1,\"Param1\":\"{xxxParam}\"}}";
    break;
```

### 6. 编译 + 测试

```bash
dotnet build ET.sln -c Debug
dotnet Bin/ET.UBridge.dll Xxx --param1 value
```

## 架构核心

```
CLI (Program.cs)                          Unity Editor
    │                                         │
    │  写 {rpcId}.json → requests/  ──→    UBridgeEditorHost
    │                                       每 200ms File.Move → processing/（原子锁）
    │                                       解析 JSON → 查 Handler 表 → 调用 Handle()
    │  ← 读 responses/{rpcId}.json ──      写响应 JSON
    │
    ▼
  stdout（JSON）
```

**关键组件：**

| 组件 | 文件 | 职责 |
|------|------|------|
| 原子锁 | `UBridgeStorage.cs:TryTakeNextRequest()` | `File.Move` 从 `requests/` → `processing/`，只有一个进程能成功 |
| 原子写入 | `UBridgeStorage.cs:WriteTextAtomic()` | 先写 `.tmp`，再 Move 到目标，避免半写入 |
| BSON 序列化 | `UBridgeStorage.cs:UBridgeJsonHelper` | 封装的 `MongoHelper.ToJson/FromJson`，带 `_t` 类型判别器 |
| 命令分发 | `UBridgeEditorHost.cs` | `Dictionary<string, Func<string, string>>` 存 Handler，按 Command 字段路由 |

## 已知坑（14 条，按文件/场景查询）

| 场景 | 报错特征 | 解决 |
|------|----------|------|
| 加新 Handler 后 DotNet 构建报 CS0246 | 类型找不到 | 先运行 proto 生成（`dotnet ...Exe/ET.Proto2CS.dll`） |
| 加新 Handler 后 Unity 报 CS0103 | `当前上下文中不存在名称` | Unity 打开刷新 → 生成 .meta → Rider 重载项目 |
| 加新 Handler 后 Unity 报 CS0246 | 找不到 proto 类型 | 检查新包是否在 `packages-lock.json` 中，手动添加 `cn.etetet.xxx` 条目 |
| Editor 端编译时 `File.Move` 报 CS1501 | `No overload for method 'Move' takes 3 arguments` | .NET Framework 4.7.1 只有两参数，用 `File.Delete` + `File.Move` 替代 |
| `[InitializeOnLoad]` 时报 NRE | `MongoRegister.Init() → CodeTypes.Instance 为 null` | 延迟 `MongoRegister.Init()` 到首次 `OnUpdate`；或直接移除调用，ET 框架已自调 |
| 静态字段报 ET0015 | `Static字段声明 xxx 需要标记标签` | 加 `[StaticField]` 特性（需 `using` 对应的 namespace） |
| 静态字段报 ET0004 | `Hotfix程序集中 不允许声明非Const字段` | 代码移到 `Scripts/ModelView/Client/`（ET.ModelView 程序集），不要放在 HotfixView |
| Proto 类型报 ET0031 | `禁止使用new构造ET.Xxx类型的对象` | 用 `Xxx.Create()` 代替 `new Xxx()` |
| Editor 端 `Object` 歧义报 CS0029 | `Cannot implicitly convert type 'UnityEngine.Object' to 'ET.Object'` | 显式写 `UnityEngine.Object` 全名 |
| DotNet 端 `[StaticField]` 报 CS0246 | `未能找到类型或命名空间名"StaticField"` | 在 `.csproj` 加 `UBRIDGE_CLI` 常量，代码中用 `#if !UBRIDGE_CLI` / `#endif` 包裹 |
| DotNet 端报 CS0246 找不到 Proto 类型 | CLI 项目没引用 ET.Model | CLI 不要引用 Proto 类型，用字符串拼 JSON（`$"{{\"_t\":\"ET.XxxRequest\",...}}"`) |
| DotNet 顶层语句报 CS8803 | `顶级语句必须位于命名空间之前` | 文件必须以 `return await ...` 开头，或用传统 `Main` 方法（推荐） |
| `dotnet build ET.sln` 后 DLL 不存在 | 新项目没加进 .sln | 在 `ET.sln` 中添加项目定义 + 构建配置 + NestedProjects 映射 |
| CLI 参数被误当命令名 | 实际执行了 `--count` 而不是 `ConsoleGetLogs` | `args[0]` 是命令名，带 `--` 前缀的参数需显式传命令名 |
| Proto 单行格式 → CS1514/CS1513 | 128 错误，生成 C# 缺少 `{` | **`{` 必须独占一行**，每字段一行，参考 `LoginOuter_C_1000.proto` 格式 |
| Proto 缺少 IRequest 标记 | 48 错误，类型未实现接口 | 每个 Request/Response message 必须有 `// IRequest`/`// IResponse` + `// ResponseType` |
| `Object` 歧义（多个 Handler） | CS0311/CS0029 | `using UObject = UnityEngine.Object;` 别名统一替换 |
| `new BridgeXxx { ... }` → ET0031 | 初始化器未匹配正则 | 逐属性赋值：`Create()` + `info.A = ...` |
| Unity 6 API 差异 | `EditorSceneManager`/`GetScenePathByBuildIndex` 不存在 | 加 `UnityEditor.SceneManagement`；用 `EditorBuildSettings.scenes[i].path` |
| CLI default case 参数丢失 | `--name` 解析但未进入 JSON | default case 动态拼接已设置参数 |



## 待移植命令（ET10 参考）

源包共 68 个命令，分为以下类别。标注难度：

| 类别 | 难度 | 说明 |
|------|------|------|
| 系统（Ping 等 6 个） | 🟢 | ✅ 6/6 已实现 |
| BatchExecute | 🟡 | ✅ 已实现 |
| 资源 Asset（7 个） | 🟢 | ✅ 7/7 已实现 |
| 场景 Scene（5 个） | 🟢 | ✅ 5/5 已实现 |
| 选中 Selection（5 个） | 🟢 | ✅ 5/5 已实现 |
| GameObject（7 个） | 🟢 | ✅ 7/7 已实现 |
| Transform（8 个） | 🟢 | ✅ 8/8 已实现 |
| Inspector（8 个） | 🟡 | ✅ 8/8 已实现 |
| Prefab（6 个） | 🟡 | ✅ 6/6 已实现 |
| Editor 同步（5 个） | 🟢 | ✅ 5/5 已实现 |
| 延迟命令（5 个） | 🔴 | ⚠️ 3 引擎就绪 + 2 EnterPlay/ExitPlay 域重载 bug 待修 |
| BatchExecute | 🟡 | 待移植 |
| 生命周期剩余（AssetImport/Refresh 等 2 个） | 🔴 | 待移植 |
| GVResolution/EditorLog/UnityTestRun 等 | 🟢 | 待移植 |

> **参考文件：** `Notes/ET10-UnityBridge命令清单.md`
> **源包位置：** `D:\Projects\et10initLockStepYIUI-UITest\Packages\cn.etetet.unitybridge`