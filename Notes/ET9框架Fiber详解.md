# ET9 框架 Fiber 详解

## 一、Fiber 是什么

Fiber 是 ET9 框架中的**轻量级执行上下文**，类似 Erlang 的轻量级进程概念。每个 Fiber 拥有：

- 独立的 Entity 树（通过 Root Scene 持有）
- 独立的 EntitySystem（事件分发）
- 独立的 Mailboxes（Actor 消息收发）
- 独立的 ThreadSynchronizationContext（线程同步上下文）
- 独立的 Logger

简单理解：**一个 Fiber 就是一个隔离的"小世界"**，有自己的实体、组件、事件系统，不与其它 Fiber 共享任何状态，只能通过 Actor 消息通信。

### 核心源码（`Fiber.cs`）

```csharp
public class Fiber: IDisposable
{
    // 线程静态变量，每个线程访问自己的 Fiber 实例
    [ThreadStatic]
    public static Fiber Instance;

    public int Id;
    public int Zone;
    public Scene Root { get; }          // 该 Fiber 的根场景实体
    public int Process;                 // 所属进程ID
    public EntitySystem EntitySystem;   // 独立的事件系统
    public Mailboxes Mailboxes;         // 独立的消息邮箱
    public ThreadSynchronizationContext ThreadSynchronizationContext;

    internal Fiber(int id, int zone, int sceneType, string name)
    {
        this.Id = id;
        this.Zone = zone;
        this.EntitySystem = new EntitySystem();
        this.Mailboxes = new Mailboxes();
        this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
        this.Root = new Scene(this, id, 1, sceneType, name);  // 创建根场景
    }

    internal void Update()
    {
        this.EntitySystem.Publish(new UpdateEvent());  // 发布 Update 事件给所有实体
    }

    internal void LateUpdate()
    {
        this.EntitySystem.Publish(new LateUpdateEvent());
        this.ThreadSynchronizationContext.Update();     // 处理线程上下文中的异步任务
    }
}
```

---

## 二、Fiber 有什么用

### 1. 逻辑隔离

每个 Fiber 内部是一个完全独立的 ECS 世界，Entity、Component、System 都在 Fiber 内闭环运行，不会与其它 Fiber 产生数据竞争。

### 2. 并发执行

多个 Fiber 可以并行运行在不同的线程上，充分利用多核 CPU。

### 3. 生命周期管理

Fiber 可以动态创建和销毁，非常适合"按需创建、用完即弃"的场景（如游戏房间）。

### 4. 安全通信

Fiber 之间通过 Actor 消息机制通信，不允许直接引用其它 Fiber 的对象，从架构层面杜绝了多线程竞争问题。

---

## 三、怎么用

### 创建 Fiber

通过 `FiberManager.Instance.Create()` 创建：

```csharp
// 指定 ID 创建（用于配置表中的固定 ID）
await FiberManager.Instance.Create(
    SchedulerType.ThreadPool,    // 调度类型
    fiberId,                     // Fiber ID
    zone,                        // 区服
    sceneType,                   // 场景类型
    "name"                       // 名称
);

// 自动生成 ID 创建
int fiberId = await FiberManager.Instance.Create(
    SchedulerType.ThreadPool,
    zone,
    SceneType.RoomRoot,
    "RoomRoot"
);
```

### 销毁 Fiber

```csharp
await FiberManager.Instance.Remove(fiberId);
```

注意：Remove 会在 Fiber 所属的线程上执行销毁，避免线程竞争。

### 初始化 Fiber

创建 Fiber 后，框架会根据 `sceneType` 自动分发到对应的初始化处理器：

```csharp
// 框架内部调用
await EventSystem.Instance.Invoke<FiberInit, ETTask>(
    sceneType, new FiberInit() { Fiber = fiber }
);
```

开发者通过 `[Invoke(SceneType.XXX)]` 注册初始化处理器：

```csharp
[Invoke(SceneType.Match)]  // 当 sceneType == Match 时触发
public class FiberInit_Match : AInvokeHandler<FiberInit, ETTask>
{
    public override async ETTask Handle(FiberInit fiberInit)
    {
        Scene root = fiberInit.Fiber.Root;
        // 初始化这个 Fiber 的组件
        root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
        root.AddComponent<TimerComponent>();
        root.AddComponent<MatchComponent>();
        // ...
    }
}
```

### 获取当前 Fiber

```csharp
// 在 Entity 上获取所属 Fiber
Fiber fiber = entity.Fiber();

// 或者在线程上下文中直接访问
Fiber current = Fiber.Instance;
```

---

## 四、三种调度模式

FiberManager 支持三种调度策略，由 `SchedulerType` 枚举定义：

### 1. Main — 主线程调度

```
┌─────────── 主线程（Unity 主循环）────────────┐
│ Fiber A: Update → LateUpdate                 │
│ Fiber B: Update → LateUpdate                 │
│ Fiber C: Update → LateUpdate                 │
│ （轮询执行，所有 Fiber 共享主线程）             │
└──────────────────────────────────────────────┘
```

- 所有 Fiber 在主线程上轮询执行
- 适用于 Unity Editor、WebGL 等不支持多线程的平台
- 客户端默认使用此调度

### 2. Thread — 独占线程调度

```
┌──────────┐  ┌──────────┐  ┌──────────┐
│ Thread 1  │  │ Thread 2  │  │ Thread 3  │
│ Fiber A   │  │ Fiber B   │  │ Fiber C   │
│ 独占循环   │  │ 独占循环   │  │ 独占循环   │
└──────────┘  └──────────┘  └──────────┘
```

- 每个 Fiber 独占一个 OS 线程
- 线程内部循环调用 `Update()` + `LateUpdate()` + `Sleep(1ms)`
- 适用于需要独占 CPU 的场景（如独立的网络线程）

### 3. ThreadPool — 线程池调度

```
┌───────────────────────────────────────────┐
│            线程池（CPU 核心数个线程）          │
│  Thread 1 ─┐                               │
│  Thread 2 ─┤─ 共享处理队列                   │
│  Thread 3 ─┤   [Fiber A, Fiber B, Fiber C,  │
│  Thread 4 ─┘    Fiber D, Fiber E, ...]      │
└───────────────────────────────────────────┘
```

- 创建 CPU 核心数个线程
- 所有 Fiber 放入共享队列，线程按需取用处理
- 适用于大量 Fiber 并发的场景（如服务器上的游戏房间）

### 平台差异

```csharp
// FiberManager.Awake() 中的平台判断
#if (ENABLE_VIEW && UNITY_EDITOR) || UNITY_WEBGL
    // Unity 编辑器 或 WebGL：所有调度都走主线程
    this.schedulers[(int)SchedulerType.Thread] = this.mainThreadScheduler;
    this.schedulers[(int)SchedulerType.ThreadPool] = this.mainThreadScheduler;
#else
    // 独立服务端：Thread 和 ThreadPool 使用真正的多线程
    this.schedulers[(int)SchedulerType.Thread] = new ThreadScheduler(this);
    this.schedulers[(int)SchedulerType.ThreadPool] = new ThreadPoolScheduler(this);
#endif
```

---

## 五、Fiber 与 Entity/Scene 的关系

```
Fiber
  │
  └── Root (Scene)                    ← 每个 Fiber 恰好一个 Root Scene
        │
        ├── Component (MailBoxComponent, TimerComponent, ...)
        │
        ├── Child Entity (Room, MatchComponent, ...)
        │     ├── Component
        │     └── Child Entity
        │           └── ...
        │
        └── Child Entity
              └── ...
```

- **Fiber** 持有 **Root Scene**
- **Scene** 是特殊的 **Entity**，实现了 `IScene` 接口，持有 `Fiber` 引用
- 所有 Entity 通过 `IScene` 向上找到所属 Fiber
- 同一个 Fiber 内的 Entity 可以直接访问彼此，跨 Fiber 只能通过消息

```csharp
// Entity 获取所属 Fiber 的方式
public static Fiber Fiber(this Entity entity)
{
    return entity.IScene.Fiber;
}
```

---

## 六、Fiber 用在哪些地方

### 框架核心 Fiber

| SceneType | 用途 | 调度方式 |
|-----------|------|----------|
| Main | 主 Fiber，入口 | Main（主线程） |
| NetInner | 内部网络通信 | ThreadPool |
| NetClient | 客户端网络连接 | Main / Thread |

### 业务 Fiber（以帧同步包为例）

| SceneType | 用途 | 调度方式 |
|-----------|------|----------|
| Gate | 网关服务，管理玩家连接 | ThreadPool |
| Match | 匹配服务 | ThreadPool |
| Map | 地图服务，管理房间分配 | ThreadPool |
| RoomRoot | 帧同步房间，每场战斗一个 | ThreadPool |
| LockStepClient | 客户端帧同步逻辑 | Main（主线程） |

---

## 七、客户端使用示例

### 启动流程

```
Unity MonoBehaviour (Init.cs)
  │
  ├── Start()
  │     └── 创建 FiberManager 单例
  │     └── CodeLoader.Start() → Entry.Start()
  │           └── FiberManager.Create(SchedulerType.Main, SceneType.Main, ...)
  │                 └── FiberInit_Main 触发
  │                       └── 发布 EntryEvent1 → EntryEvent2 → EntryEvent3
  │                             └── 各包注册自己的初始化逻辑
  │
  ├── Update()      → FiberManager.Instance.Update()      → 驱动所有 Main 调度的 Fiber
  └── LateUpdate()  → FiberManager.Instance.LateUpdate()  → 驱动所有 Main 调度的 Fiber
```

客户端 `Init.cs`（Unity MonoBehaviour）：

```csharp
public class Init: MonoBehaviour
{
    private async ETTask StartAsync()
    {
        // 1. 创建全局单例
        World.Instance.AddSingleton<TimeInfo>();
        World.Instance.AddSingleton<FiberManager>();  // ← FiberManager 在这里创建

        // 2. 加载代码，调用 Entry.Start()
        World.Instance.AddSingleton<CodeLoader>().Start();  // → Entry.Start()
    }

    private void Update()
    {
        TimeInfo.Instance.Update();
        FiberManager.Instance.Update();      // ← 每帧驱动所有主线程 Fiber
    }

    private void LateUpdate()
    {
        FiberManager.Instance.LateUpdate();  // ← 每帧驱动 LateUpdate
    }
}
```

`Entry.cs` 中创建 Main Fiber：

```csharp
public static class Entry
{
    private static async ETTask StartAsync()
    {
        // 注册各种全局单例...
        World.Instance.AddSingleton<OpcodeType>();
        World.Instance.AddSingleton<MessageQueue>();
        // ...

        // 创建主 Fiber
        await FiberManager.Instance.Create(SchedulerType.Main, SceneType.Main, 0, SceneType.Main, "");
    }
}
```

`FiberInit_Main.cs` — Main Fiber 的初始化：

```csharp
[Invoke(SceneType.Main)]
public class FiberInit_Main : AInvokeHandler<FiberInit, ETTask>
{
    public override async ETTask Handle(FiberInit fiberInit)
    {
        Scene root = fiberInit.Fiber.Root;
        // 根据 Options 中的 SceneName 确定 SceneType
        int sceneType = SceneTypeSingleton.Instance.GetSceneType(Options.Instance.SceneName);
        root.SceneType = sceneType;

        // 依次发布三个初始化事件，各业务包可以订阅
        await EventSystem.Instance.PublishAsync(root, new EntryEvent1());
        await EventSystem.Instance.PublishAsync(root, new EntryEvent2());
        await EventSystem.Instance.PublishAsync(root, new EntryEvent3());
    }
}
```

帧同步包客户端订阅初始化事件（`EntryEvent3_InitClient.cs`）：

```csharp
[Event(SceneType.LockStep)]
public class EntryEvent3_InitClient : AEvent<Scene, EntryEvent3>
{
    protected override async ETTask Run(Scene root, EntryEvent3 args)
    {
        root.AddComponent<GlobalComponent>();
        root.AddComponent<UIComponent>();
        root.AddComponent<PlayerComponent>();
        // ...
    }
}
```

### 客户端特点

- 客户端只有 **Main Fiber**，运行在 Unity 主线程
- 在 Editor/WebGL 下，即使指定 Thread/ThreadPool 调度，也会降级为主线程
- 客户端的 Fiber 驱动由 Unity 的 `MonoBehaviour.Update()` 触发

---

## 八、服务端使用示例

### 启动流程

```
Server Init.cs
  │
  ├── Start()
  │     └── 创建 FiberManager 单例
  │     └── CodeLoader.Start() → Entry.Start()
  │           └── 创建 Main Fiber (SchedulerType.Main)
  │                 └── FiberInit_Main
  │                       └── EntryEvent2 (服务端初始化)
  │                             └── 读取配置，批量创建业务 Fiber
  │
  ├── Update()      → FiberManager.Instance.Update()
  └── LateUpdate()  → FiberManager.Instance.LateUpdate()
```

服务端 `EntryEvent2_InitServer.cs`：

```csharp
[Event(SceneType.LockStep)]
public class EntryEvent2_InitServer : AEvent<Scene, EntryEvent2>
{
    protected override async ETTask Run(Scene root, EntryEvent2 args)
    {
        // 读取当前进程应该启动哪些场景
        var scenes = StartSceneConfigCategory.Instance.GetByProcess(process);
        foreach (StartSceneConfig startConfig in scenes)
        {
            int sceneType = SceneTypeSingleton.Instance.GetSceneType(startConfig.SceneType);
            // 创建业务 Fiber（Match、Map 等），使用线程池调度
            await FiberManager.Instance.Create(
                SchedulerType.ThreadPool,
                startConfig.Id,
                startConfig.Zone,
                sceneType,
                startConfig.Name
            );
        }
    }
}
```

各业务 Fiber 的初始化处理器：

```csharp
// 匹配服务 Fiber
[Invoke(SceneType.Match)]
public class FiberInit_Match : AInvokeHandler<FiberInit, ETTask>
{
    public override async ETTask Handle(FiberInit fiberInit)
    {
        Scene root = fiberInit.Fiber.Root;
        root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
        root.AddComponent<TimerComponent>();
        root.AddComponent<MessageSender>();
        root.AddComponent<MatchComponent>();       // ← 匹配逻辑组件
    }
}

// 地图服务 Fiber
[Invoke(SceneType.Map)]
public class FiberInit_Map : AInvokeHandler<FiberInit, ETTask>
{
    public override async ETTask Handle(FiberInit fiberInit)
    {
        Scene root = fiberInit.Fiber.Root;
        root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
        root.AddComponent<TimerComponent>();
        root.AddComponent<RoomManagerComponent>();  // ← 房间管理组件
        root.AddComponent<MessageSender>();
    }
}

// 房间 Fiber（每场战斗动态创建）
[Invoke(SceneType.RoomRoot)]
public class FiberInit_RoomRoot : AInvokeHandler<FiberInit, ETTask>
{
    public override async ETTask Handle(FiberInit fiberInit)
    {
        Scene root = fiberInit.Fiber.Root;
        root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
        root.AddComponent<TimerComponent>();
        root.AddComponent<MessageSender>();
        root.AddComponent<MessageLocationSenderComponent>();
    }
}
```

### 动态创建房间 Fiber

战斗房间是在匹配成功后动态创建的（`Match2Map_GetRoomHandler.cs`）：

```csharp
// Map Fiber 收到创建房间请求
Fiber fiber = root.Fiber();
int fiberId = await FiberManager.Instance.Create(
    SchedulerType.ThreadPool,   // 线程池调度
    fiber.Zone,
    SceneType.RoomRoot,         // 房间场景类型
    "RoomRoot"
);
// 返回 ActorId，后续通过消息与这个房间通信
ActorId roomRootActorId = new(fiber.Process, fiberId);
```

### 服务端特点

- 服务端使用 **ThreadPool** 调度，充分利用多核
- Match、Map 等服务是配置表中**预定义**的 Fiber，启动时创建
- Room 是**动态创建**的 Fiber，每场战斗一个，战斗结束即销毁
- 同一进程内可以有多个 Fiber 并行运行
- Fiber 之间通过 Actor 消息通信（`MessageSender.Call` / `Send`）

---

## 九、Fiber 之间的通信

Fiber 之间**不允许直接引用对方的对象**（`FiberManager.Get` 是 internal 方法），只能通过 Actor 消息机制通信：

```csharp
// Fiber A 向 Fiber B 发送请求并等待响应
Map2Match_GetRoom response = await root.GetComponent<MessageSender>().Call(
    targetActorId,       // 目标 Fiber 中某个实体的地址
    request              // 请求消息
) as Map2Match_GetRoom;

// Fiber A 向 Fiber B 发送单向消息（不等响应）
root.GetComponent<MessageLocationSenderComponent>()
    .Get(LocationType.GateSession)
    .Send(playerId, message);
```

ActorId 的结构：

```csharp
public struct ActorId
{
    public int Process;    // 进程 ID
    public int FiberId;    // Fiber ID
    public long InstanceId;// 实体实例 ID
}
```

ET 框架根据 Process 和 FiberId 判断：
- **同进程同 Fiber** → 直接调用
- **同进程不同 Fiber** → 线程间消息传递
- **不同进程** → 网络传输

---

## 十、Fiber 与进程、线程的关系

```
┌─────────────────────────────── OS 进程 ──────────────────────────────┐
│                                                                      │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  │
│  │ Fiber A  │  │ Fiber B  │  │ Fiber C  │  │ Fiber D  │  │ Fiber E  │  │
│  │ (Main)   │  │ (Match)  │  │ (Map)    │  │ (Room1)  │  │ (Room2)  │  │
│  │          │  │          │  │          │  │          │  │          │  │
│  │ 主线程    │  │ 线程池    │  │ 线程池    │  │ 线程池    │  │ 线程池    │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │
│       │              │              │              │              │       │
│       │         Actor 消息通信（同进程内不走网络）                         │
│       │              │              │              │              │       │
│  ┌────┴──────────────┴──────────────┴──────────────┴──────────────┘  │
│  │                    线程池（CPU 核心数个线程）                         │
│  └───────────────────────────────────────────────────────────────────┘
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
         │ 网络传输
         ▼
┌─────────────────────────────── 另一个 OS 进程 ──────────────────────┐
│  ...                                                                │
└─────────────────────────────────────────────────────────────────────┘
```

| 概念 | 范围 | 生命周期 | 通信方式 | 数量级 |
|------|------|----------|----------|--------|
| **进程** | OS 进程 | 长期运行 | 网络 | 少量固定 |
| **Fiber** | 逻辑隔离单元 | 动态创建/销毁 | Actor 消息 | 可达数千 |
| **线程** | 执行单元 | 由调度器管理 | 共享内存（框架封装） | CPU 核心数 |

核心设计：Fiber 提供了**逻辑级别的隔离**，同时通过线程池复用物理线程，避免了"一个房间一个线程"的资源浪费。
