# cn.etetet.lockstep — 帧同步包 CLAUDE.md

## 包概述

ET 框架的帧同步（LockStep）业务包，实现完整的帧同步游戏流程：匹配 → 进房 → 帧同步逻辑 → 视图表现。

## 目录结构

```
cn.etetet.lockstep/
├── Editor/
│   └── LockStepEditor.cs          # 菜单项：ET/LockStep/Init
├── Scripts/
│   ├── Model/                     # 数据模型（AOT）
│   │   ├── Share/                 # 客户端+服务端共用
│   │   │   ├── LSUnit.cs          # 帧同步单位实体（Position, Rotation, Forward）
│   │   │   ├── LSUnitComponent.cs # LSUnit 容器组件��挂在 LSWorld 上
│   │   │   ├── LSConstValue.cs    # 常量：UpdateInterval=50ms, MatchCount=1
│   │   │   ├── LSInput.cs         # 输入结构体（V: TSVector2, Button: int）
│   │   │   ├── LSInputComponent.cs# 输入组件（挂在 LSUnit 上），实现 ILSUpdate
│   │   │   ├── OneFrameInputs.cs  # 一帧所有玩家输入集合 Dictionary<long, LSInput>
│   │   │   ├── Room.cs            # 核心场景容器（IScene）
│   │   │   ├── FrameBuffer.cs     # 环形帧缓冲（存储输入、快照、哈希）
│   │   │   ├── Replay.cs          # 回放数据（单位信息 + 帧输入 + 快照）
│   │   │   ├── FixedTimeCounter.cs# 帧时间计数器（支持动态调整间隔）
│   │   │   ├── SceneType.cs       # 场景类型枚举（RoomRoot/Match/Map 等）
│   │   │   ├── PackageType.cs     # 包类型 ID = 11
│   │   │   └── IRoomMessage.cs    # 房间消息接口
│   │   ├── Client/
│   │   │   ├── LSClientUpdater.cs # 客户端帧更新器（输入收集+预测+发送）
│   │   │   ├── LSReplayUpdater.cs # 回放更新器
│   │   │   └── ...
│   │   └── Server/                # 服务端模型（PlayerRoom、RoomServer 等）
│   ├── Hotfix/                    # 逻辑实现（热更）
│   │   ├── Share/
│   │   │   ├── LSInputComponentSystem.cs   # 输入处理：V * 6 * 50 / 1000 移动
│   │   │   ├── LSUnitFactory.cs            # 工厂：创建 LSUnit + 添加 LSInputComponent
│   │   │   ├── RoomSystem.cs               # Room 核心逻辑：Init/Update/Save/Record
│   │   │   └── EntryEvent1_InitShare.cs    # 初始化事件
│   │   ├── Client/
│   │   │   ├── LSClientUpdaterSystem.cs    # 客户端帧循环：预测→更新→发送
│   │   │   ├── LSReplayUpdaterSystem.cs    # 回放帧循环
│   │   │   ├── EnterMapHelper.cs           # 进入地图
│   │   │   └── ...Handler.cs               # 各种消息处理器
│   │   └── Server/                # 服务端逻辑
│   ├── ModelView/                 # 视图模型（AOT，可引用 Unity）
│   │   └── Client/
│   │       ├── LSUnitView.cs          # 单位视图（GameObject, Transform, 插值数据）
│   │       ├── LSUnitViewComponent.cs # 视图容器（挂在 Room 上）
│   │       ├── LSAnimatorComponent.cs # 3D Animator 组件（当前用 Unity Animator）
│   │       ├── LSOperaComponent.cs    # 操作组件（WASD 输入）
│   │       ├── LSCameraComponent.cs   # 摄像机跟随
│   │       └── UI/                    # UI 组件定义
│   └── HotfixView/                # 视图逻辑（热更）
│       └── Client/
│           ├── LSUnitViewSystem.cs            # 视图更新：位置插值 + 动画切换
│           ├── LSUnitViewComponentSystem.cs   # 视图初始化：加载 3D prefab
│           ├── LSAnimatorComponentSystem.cs   # Animator 控制
│           ├── LSOperaComponentSystem.cs      # WASD 输入 → LSClientUpdater.Input
│           ├── LSCameraComponentSystem.cs     # 摄像机跟随
│           └── UI/                            # UI 逻辑
└── DotNet~/                      # 服务器端独立运行入口
```

## 核心类详解

### Room（帧同步房间）

```csharp
Room : Entity, IScene, IUpdate
├── Fiber              // 所在 Fiber
├── LSWorld            // 帧同步世界（Child）- 所有 LSEntity 的容器
├── FrameBuffer        // 环形帧缓冲
├── FixedTimeCounter   // 帧时间计数器
├── PlayerIds          // 玩家 ID 列表
├── PredictionFrame    // 客户端预测帧号
├── AuthorityFrame     // 权威帧号（服务器确认）
├── Replay             // 回放数据
└── IsReplay           // 是否回放模式
```

**核心方法** `RoomSystem.Update(OneFrameInputs)`：
1. 将输入分发到各 LSUnit 的 LSInputComponent
2. 保存当前帧 LSWorld 快照（MemoryPack 序列化）
3. 调用 `LSWorld.Update()` → 驱动所有 LSUpdate

### LSWorld（帧同步世界）

```csharp
LSWorld : Entity, IScene
├── Frame: int         // 当前帧号
├── Random: TSRandom   // 确定性随机
├── updater: LSUpdater // 内部更新器
└── idGenerator: long  // ID 生成器
```

- `LSWorld.Update()` 调用 `LSUpdater.Update()` 遍历所有 ILSUpdate 实体
- 支持完整的 MemoryPack 序列化/反序列化（用于快照回滚）

### 帧同步实体体系

```
LSWorld
└── LSUnitComponent (Component)
    └── LSUnit (Child) - 逻辑单位
        ├── Position: TSVector
        ├── Rotation: TSQuaternion
        ├── Forward: TSVector (计算属性)
        └── LSInputComponent (Component) - 输入
            └── LSInput { V: TSVector2, Button: int }
```

### LSInputComponentSystem — 移动逻辑

```csharp
LSUpdate:
    TSVector2 v2 = LSInput.V * 6 * 50 / 1000;  // 速度6 * 帧间隔50ms / 1000
    unit.Position += new TSVector(v2.x, 0, v2.y);
    unit.Forward = unit.Position - oldPos;
```

### 视图层体系

```
Room (IScene)
├── LSClientUpdater (Component) - 客户端帧驱动
├── LSOperaComponent (Component) - WASD 输入
├── LSUnitViewComponent (Component) - 视图容器
│   └── LSUnitView (Child) - 单位视图
│       ├── GameObject + Transform
│       ├── LSAnimatorComponent (Component) - 3D Animator
│       ├── Position/Rotation (插值目标)
│       └── totalTime/t (插值进度)
└── LSCameraComponent (Component) - 摄像机
```

### 视图更新流程

1. **LSOperaComponentSystem.Update**：读取 WASD → 设置 `LSClientUpdater.Input.V`
2. **LSClientUpdaterSystem.Update**：
   - 检查时间是否到下一帧
   - 最多预测 5 帧
   - 获取/预测帧输入 → `Room.Update(inputs)` → 发送输入到服务器
3. **LSUnitViewSystem.Update**：
   - 读取 LSUnit 的逻辑位置
   - Lerp 插值平滑移动
   - 根据输入 V 是否为 zero 切换动画（Speed 参数）

### LSUnitViewComponentSystem.InitAsync — 创建视图

```
加载 Unit.prefab (3D Skeleton) → Instantiate → 创建 LSUnitView
→ 添加 LSAnimatorComponent (Unity Animator)
```

## 帧同步参数

| 参数 | 值 | 说明 |
|------|-----|------|
| UpdateInterval | 50ms | 逻辑帧间隔 |
| FrameCountPerSecond | 20 | 每秒逻辑帧数 |
| MatchCount | 1 | 匹配人数（1=单机测试） |
| SaveLSWorldFrameCount | 1200 | 每 60 秒存一次快照 |
| 预测上限 | 5 帧 | 客户端最多预测 5 帧 |
| 移动速度 | 6 | `V * 6 * 50 / 1000` |

## 当前状态

### 已实现
- 完整的帧同步框架：匹配 → 进房 → 帧同步逻辑
- 客户端预测 + 回滚
- 服务器权威帧广播 + 哈希校验
- 回放系统（变速播放）
- 3D Skeleton 角色渲染（Animator）
- WASD 移动 + 动画切换

### 待实现（2D 帧动画 Demo）
- 2D 精灵动画系统（替换 3D Animator）
- IMG 文件解析（NpkApi 移植）
- LSAnimComponent（帧同步确定性动画）
- LSSpriteAnimViewComponent（2D 视图渲染）
- 动画配置加载（stay.json / move.json）

## 注意事项

- **LSEntity 必须用 MemoryPackable 标记**，否则快照序列化会遗漏字段
- **LSUpdate 中不能使用任何非确定性操作**（如 System.Random、DateTime、float）
- **视图层不参与帧同步**，只读取逻辑层状态做表现，可以自由使用 Unity API
- **Room.IScene 就是 Room 自身**，视图层通过 `(entity.IScene as Room)` 获取 Room
- **LSUnitView 用 EntityRef\<LSUnit\>** 持有逻辑层引用，弱引用避免阻止 GC
- **DotNet~** 目录下的代码是服务器独立运行版本，与 Unity 客户端共享 Model/Hotfix/Share
