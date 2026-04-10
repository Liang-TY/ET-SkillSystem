# 怪物副本与 Scene、Map、Room 的关系

## 一、Scene、Map、Room、副本分别是什么

先看现有的实体层次结构：

```
┌─── Fiber (RoomRoot) ───────────────────────────────────────────┐
│                                                                 │
│  Root Scene (SceneType.RoomRoot)                                │
│    └── 组件: MailBox, Timer, MessageSender ...                  │
│    └── 组件: Room                                               │
│          ├── 属性: FrameBuffer, PredictionFrame, AuthorityFrame │
│          ├── 子: LSWorld (SceneType.LockStepServer)             │
│          │     └── 组件: LSUnitComponent                        │
│          │           ├── 子: LSUnit(玩家A)                      │
│          │           │     └── 组件: LSInputComponent           │
│          │           ├── 子: LSUnit(玩家B)                      │
│          │           │     └── 组件: LSInputComponent           │
│          │           └── (怪物也在这里)                          │
│          └── 组件: LSServerUpdater / LSClientUpdater            │
└─────────────────────────────────────────────────────────────────┘
```

| 概念 | 本质 | 职责 |
|------|------|------|
| **Scene** | Fiber 的根实体，每个 Fiber 有且仅有一个 Root Scene | 提供实体树根节点 |
| **Map** | 一个 SceneType 的 Fiber（SceneType.Map） | 管理房间的创建和分配 |
| **Room** | 挂在 Root Scene 上的 Component | 帧同步逻辑（帧缓冲、回滚） |
| **LSWorld** | Room 的 Child Entity | 逻辑世界的所有实体（玩家、怪物、技能） |
| **副本** | 就是 Room 这个整体 | 战斗场景 = Room + LSWorld + 怪物 LSUnit |

关键事实：

- **Room 不是 Fiber 的根**，它是 `root.AddComponent<Room>()` 挂到 Root Scene 上的组件
- **副本不是一个独立的东西，它就是 Room**
- Room 里的 LSWorld 就是副本的逻辑世界
- 怪物和玩家都是 LSWorld 里的 LSUnit，区别只是怪物挂 AI 组件，玩家挂 InputComponent

## 二、帧同步中怪物的同步方式

核心原则：**怪物没有任何数据通过网络同步，所有客户端各自本地算出完全一样的结果。**

网络只传输玩家的输入（按键、摇杆方向、技能按钮），怪物的一切行为全部由本地确定性模拟产生。

### 怪物位置 — 确定性AI驱动

```
每帧逻辑更新：
  怪物AI读取：
    - 怪物当前状态（巡逻/追击/攻击）
    - 所有玩家的位置（本地已有）
    - 仇恨列表（确定性计算的）

  AI决策（纯函数，确定性）：
    if (距离最近的玩家 < 5米) → 追击该玩家
    if (距离最近的玩家 < 1.5米) → 停下来准备攻击
    else → 继续巡逻路线

  移动计算（定点数）：
    怪物.Position += 方向 * 速度 * deltaTime
```

### 怪物血量 — 确定性伤害计算

```
帧N：玩家B按下攻击键 → 输入: {PlayerB: Button=攻击}

所有客户端各自本地执行：
  1. 检查：玩家B是否在攻击范围内？ → 是（所有人算出一样的距离）
  2. 检查：攻击CD是否好了？ → 是（所有人从同一帧开始计时）
  3. 计算伤害：damage = 攻击力 * 技能倍率 - 防御力 * 减伤系数（纯数学，定点数）
  4. 怪物.HP -= damage
  → 所有客户端的怪物HP剩余完全一致
```

### 帧同步的要求

| 要求 | 说明 |
|------|------|
| **确定性浮点** | 不能用 `float`/`double`，必须用定点数（如 TrueSync） |
| **确定性随机** | 用固定种子的伪随机数生成器 |
| **逻辑完全相同** | 不能有 `if (isLocalPlayer)` 这种分支影响逻辑 |
| **不依赖帧率** | 逻辑用固定时间步长（如50ms），不能用 `Time.deltaTime` |
| **不依赖外部状态** | 不能查数据库、读文件、调系统API |

## 三、怪物副本的实现方案

### 1. 定义怪物 LSUnit

```csharp
// Model/Share/LSMonsterUnit.cs
[ChildOf(typeof(LSMonsterComponent))]
[MemoryPackable]
public partial class LSMonsterUnit : LSEntity, IAwake, ISerializeToEntity
{
    public int ConfigId;            // 配置表ID
    public TSVector Position;
    public TSQuaternion Rotation;
    public long Hp;
    public long MaxHp;
    public int SkillCooldown;       // 技能冷却（帧数）
    public long TargetPlayerId;     // 仇恨目标
}
```

### 2. 定义怪物 AI 组件

```csharp
// Model/Share/LSMonsterAIComponent.cs
[ComponentOf(typeof(LSMonsterUnit))]
[MemoryPackable]
public partial class LSMonsterAIComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
{
    public int State;               // 0=巡逻, 1=追击, 2=攻击
}
```

### 3. 定义怪物容器组件

```csharp
// Model/Share/LSMonsterComponent.cs
[ComponentOf(typeof(LSWorld))]
[MemoryPackable]
public partial class LSMonsterComponent : LSEntity, IAwake, ISerializeToEntity
{
}
```

### 4. 怪物工厂

```csharp
// Hotfix/Share/LSMonsterFactory.cs
public static partial class LSMonsterFactory
{
    public static LSMonsterUnit Init(LSWorld lsWorld, LockStepMonsterInfo monsterInfo)
    {
        LSMonsterComponent monsterComp = lsWorld.GetComponent<LSMonsterComponent>();
        LSMonsterUnit monster = monsterComp.AddChildWithId<LSMonsterUnit>(monsterInfo.MonsterId);

        monster.ConfigId = monsterInfo.ConfigId;
        monster.Position = monsterInfo.Position;
        monster.Rotation = monsterInfo.Rotation;

        // 从配置表读取属性
        MonsterConfig config = MonsterConfigCategory.Instance.Get(monsterInfo.ConfigId);
        monster.Hp = config.Hp;
        monster.MaxHp = config.Hp;

        monster.AddComponent<LSMonsterAIComponent>();
        return monster;
    }
}
```

### 5. 怪物 AI 逻辑（确定性）

```csharp
// Hotfix/Share/LSMonsterAIComponentSystem.cs
[LSEntitySystem]
private static void LSUpdate(this LSMonsterAIComponent self)
{
    LSMonsterUnit monster = self.GetParent<LSMonsterUnit>();

    if (monster.Hp <= 0)
        return;

    // 技能CD递减
    if (monster.SkillCooldown > 0)
        monster.SkillCooldown--;

    // 获取所有玩家
    LSUnitComponent playerUnits = monster.LSWorld().GetComponent<LSUnitComponent>();

    // 找最近的玩家（确定性，因为所有人位置相同）
    long nearestId = 0;
    TSVector nearestPos = TSVector.zero;
    long minDist = long.MaxValue;
    foreach (LSUnit player in playerUnits.Children.Values)
    {
        long dist = TSVector.DistanceSquared(monster.Position, player.Position);
        if (dist < minDist)
        {
            minDist = dist;
            nearestId = player.Id;
            nearestPos = player.Position;
        }
    }

    MonsterConfig config = MonsterConfigCategory.Instance.Get(monster.ConfigId);

    if (minDist <= config.AttackRange * config.AttackRange)
    {
        // 在攻击范围内 → 攻击
        if (monster.SkillCooldown <= 0)
        {
            // 对目标造成伤害（确定性）
            LSUnit target = playerUnits.GetChild<LSUnit>(nearestId);
            target.Hp -= config.AttackPower;  // 定点数运算
            monster.SkillCooldown = config.SkillCooldownFrames;
        }
    }
    else if (minDist <= config.ChaseRange * config.ChaseRange)
    {
        // 在追击范围内 → 追击
        TSVector dir = nearestPos - monster.Position;
        dir = dir.normalized;
        monster.Position += dir * config.MoveSpeed * 50 / 1000;  // 定点数移动
        monster.Rotation = TSQuaternion.LookRotation(dir, TSVector.up);
    }
    // 否则保持巡逻/待机
}
```

### 6. 修改 Room.Init — 加入怪物创建

```csharp
public static void Init(this Room self,
    List<LockStepUnitInfo> unitInfos,
    List<LockStepMonsterInfo> monsterInfos,
    long startTime, int frame = -1)
{
    self.StartTime = startTime;
    self.AuthorityFrame = frame;
    self.PredictionFrame = frame;
    self.FrameBuffer = new FrameBuffer(frame);
    self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);

    LSWorld lsWorld = self.LSWorld;
    lsWorld.Frame = frame + 1;

    // 创建玩家
    lsWorld.AddComponent<LSUnitComponent>();
    for (int i = 0; i < unitInfos.Count; ++i)
    {
        LSUnitFactory.Init(lsWorld, unitInfos[i]);
        self.PlayerIds.Add(unitInfos[i].PlayerId);
    }

    // ★ 创建怪物
    lsWorld.AddComponent<LSMonsterComponent>();
    for (int i = 0; i < monsterInfos.Count; ++i)
    {
        LSMonsterFactory.Init(lsWorld, monsterInfos[i]);
    }
}
```

### 7. 协议定义

```protobuf
// Proto/LockStepOuter_C_11001.proto 中新增

message LockStepMonsterInfo
{
    int64 MonsterId = 1;           // 怪物唯一ID
    int32 ConfigId = 2;            // 配置表ID
    TrueSync.TSVector Position = 3;
    TrueSync.TSQuaternion Rotation = 4;
}

message Room2C_Start
{
    int64 StartTime = 1;
    repeated LockStepUnitInfo UnitInfo = 2;
    repeated LockStepMonsterInfo MonsterInfo = 3;  // ★ 新增怪物列表
}
```

### 8. 服务端生成怪物（C2Room_ChangeSceneFinishHandler）

```csharp
// 所有玩家加载完毕后
Room2C_Start room2CStart = Room2C_Start.Create();
room2CStart.StartTime = TimeInfo.Instance.ServerFrameTime();

// 创建玩家单位（不变）
foreach (RoomPlayer rp in roomServerComponent.Children.Values)
{
    LockStepUnitInfo unitInfo = LockStepUnitInfo.Create();
    unitInfo.PlayerId = rp.Id;
    unitInfo.Position = new TSVector(20, 0, -10);
    unitInfo.Rotation = TSQuaternion.identity;
    room2CStart.UnitInfo.Add(unitInfo);
}

// ★ 根据副本配置创建怪物
int dungeonId = 1001;  // 副本配置ID
DungeonConfig dungeonConfig = DungeonConfigCategory.Instance.Get(dungeonId);
foreach (DungeonMonsterConfig mc in dungeonConfig.Monsters)
{
    LockStepMonsterInfo monsterInfo = LockStepMonsterInfo.Create();
    monsterInfo.MonsterId = IdGenerater.Instance.GenerateId();
    monsterInfo.ConfigId = mc.ConfigId;
    monsterInfo.Position = mc.Position;
    monsterInfo.Rotation = TSQuaternion.identity;
    room2CStart.MonsterInfo.Add(monsterInfo);
}

// 初始化 Room（包含玩家和怪物）
room.Init(room2CStart.UnitInfo, room2CStart.MonsterInfo, room2CStart.StartTime);
room.AddComponent<LSServerUpdater>();

RoomMessageHelper.BroadCast(room, room2CStart);
```

### 9. 客户端创建怪物视图

在 `LSUnitViewComponentSystem.InitAsync` 中增加怪物视图的创建：

```csharp
public static async ETTask InitAsync(this LSUnitViewComponent self)
{
    Room room = self.Room();
    LSWorld lsWorld = room.LSWorld;
    Scene root = self.Root();

    // 创建玩家视图（不变）
    LSUnitComponent playerComp = lsWorld.GetComponent<LSUnitComponent>();
    foreach (LSUnit playerUnit in playerComp.Children.Values)
    {
        // ... 原有的玩家 GameObject 创建逻辑
    }

    // ★ 创建怪物视图
    LSMonsterComponent monsterComp = lsWorld.GetComponent<LSMonsterComponent>();
    foreach (LSMonsterUnit monster in monsterComp.Children.Values)
    {
        MonsterConfig config = MonsterConfigCategory.Instance.Get(monster.ConfigId);
        string assetsName = $"Packages/cn.etetet.demores/Bundles/Unit/{config.PrefabName}.prefab";
        GameObject bundleGameObject = await room.GetComponent<ResourcesLoaderComponent>()
            .LoadAssetAsync<GameObject>(assetsName);
        GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");

        GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();
        GameObject monsterGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
        monsterGo.transform.position = monster.Position.ToVector();

        LSUnitView monsterView = self.AddChildWithId<LSUnitView, GameObject>(monster.Id, monsterGo);
        monsterView.AddComponent<LSAnimatorComponent>();
    }
}
```

### 10. 通关判定（确定性）

在 `LSWorld` 的 `LSUpdate` 末尾，或单独做一个 `LSDungeonComponent`：

```csharp
[LSEntitySystem]
private static void LSUpdate(this LSDungeonComponent self)
{
    LSMonsterComponent monsterComp = self.LSWorld().GetComponent<LSMonsterComponent>();

    bool allDead = true;
    foreach (LSMonsterUnit monster in monsterComp.Children.Values)
    {
        if (monster.Hp > 0)
        {
            allDead = false;
            break;
        }
    }

    if (allDead)
    {
        // 所有怪物死亡 → 通关（在所有客户端同一帧触发）
        self.DungeonClear = true;
    }
}
```

## 四、完整的客户端-服务端流程

```
玩家组队 → 选择副本 → 匹配（Match Fiber）
                          │
                          ▼
                    Map Fiber 创建 RoomRoot Fiber
                          │
                          ▼
                  RoomRoot Fiber 初始化
                  ┌───────────────────────────────┐
                  │ Root Scene                      │
                  │   └── Room (Component)          │
                  │         ├── LSServerUpdater     │
                  │         └── LSWorld             │
                  │               ├── LSUnitComponent (玩家)  │
                  │               │     ├── LSUnit(玩家A) + LSInputComponent │
                  │               │     └── LSUnit(玩家B) + LSInputComponent │
                  │               ├── LSMonsterComponent (怪物) │
                  │               │     ├── LSMonsterUnit(怪物1) + LSMonsterAIComponent │
                  │               │     ├── LSMonsterUnit(怪物2) + LSMonsterAIComponent │
                  │               │     └── LSMonsterUnit(怪物3) + LSMonsterAIComponent │
                  │               └── LSDungeonComponent (通关判定) │
                  └───────────────────────────────┘
                          │
                          ▼
                  每帧循环：
                  服务端：收集玩家输入 → 广播 OneFrameInputs → room.Update()
                  客户端：发送输入 → 收到 OneFrameInputs → room.Update()

                  room.Update() 驱动 LSWorld：
                    1. 所有 LSUnit 的 LSUpdate（玩家移动）
                    2. 所有 LSMonsterUnit 的 LSUpdate（怪物AI → 移动/攻击）
                    3. 所有伤害计算、HP扣减
                    4. LSDungeonComponent 检查通关条件
```

## 五、DNF 副本房间与 ET Room 的关系

DNF 的"房间"和 ET 的 `Room` 是**两个不同的概念**，只是中文撞名了：

| DNF 概念 | 含义 | 对应 ET 中的什么 |
|----------|------|-----------------|
| **副本**（Dungeon） | 整个地下城，从头打到尾 | **Room** — 一个帧同步游戏会话 |
| **房间**（Map1/Map2/...） | 副本内的一个个关卡区域 | LSWorld 内的逻辑分区，不是独立的实体 |

### 一个副本的完整流程

```
玩家进入副本
  → 创建一个 Room（帧同步会话）
  → Room 内一个 LSWorld
  → LSWorld 内一个 LSDungeonComponent 管理关卡进度

Room 1（第一关）:
  LSWorld 内: 玩家 LSUnit + 怪物A + 怪物B
  → 打完所有怪物 → LSDungeonComponent 检测通关
  → 移除旧怪物 → 加载新地图配置 → 生成新怪物

Room 2（第二关）:
  LSWorld 内: 同一批玩家 LSUnit + 怪物C + 怪物D
  → 玩家 HP/Buff/技能冷却 全部保留（同一个 LSWorld）
  → 打完 → 继续...

Boss Room:
  LSWorld 内: 同一批玩家 + Boss
  → 击杀 Boss → 副本结束 → 销毁 Room
```

**整个过程只有一个 Room、一个 LSWorld**。DNF 的"换房间"只是 LSWorld 内部的状态切换——清理旧怪物、生成新怪物、切换地图。

### 为什么不需要多个 LSWorld

```
┌─ Room（整个副本）──────────────────────────────────────┐
│  LSWorld                                                │
│  ├── LSUnitComponent                                    │
│  │   └── LSUnit(玩家) ← HP/Buff/CD 始终保留             │
│  ├── LSProjectileComponent ← 弹道跨关卡时清空           │
│  ├── LSAreaComponent ← 区域跨关卡时清空                 │
│  ├── LSBuffComponent(per unit)                          │
│  └── LSDungeonComponent  ← 管理"当前在哪个关卡"         │
│       ├── CurrentRoomIndex: int   // 第几关              │
│       ├── RoomConfigs[]           // 每关配置            │
│       └── LSUpdate: 检查通关条件 → 切换关卡              │
└─────────────────────────────────────────────────────────┘
```

### 切换关卡时的逻辑

```csharp
// LSDungeonComponentSystem.LSUpdate
private static void OnRoomClear(LSDungeonComponent self)
{
    // 1. 清理当前关卡的实体
    self.LSWorld().GetComponent<LSProjectileComponent>().DisposeAll();
    self.LSWorld().GetComponent<LSAreaComponent>().DisposeAll();
    // 移除所有怪物（不移除玩家）

    // 2. 推进关卡
    self.CurrentRoomIndex++;

    if (self.CurrentRoomIndex >= self.RoomConfigs.Count)
    {
        // 副本通关
        self.DungeonClear = true;
        return;
    }

    // 3. 生成新关卡的怪物
    var roomConfig = self.RoomConfigs[self.CurrentRoomIndex];
    foreach (var monsterInfo in roomConfig.Monsters)
    {
        LSMonsterFactory.Init(self.LSWorld(), monsterInfo);
    }
}
```

帧缓冲、预测回滚机制不受影响——快照会自然记录 LSWorld 从"有旧怪物"到"有新怪物"的状态变化。

### Room 代码注释说的"多个 LSWorld"是什么场景

Room.cs 源码注释：`// LSWorld做成child，可以有多个lsWorld，比如守望先锋有两个`

守望先锋这类游戏：一张大地图上同时存在多个独立的逻辑区域（比如攻防两端各自有独立模拟），每个区域是独立的 LSWorld，各自有自己的帧同步循环。

DNF 不需要这种设计。DNF 的关卡是**串行的**（打完一间进下一间），不是**并行的**。

### 总结

```
DNF 副本 = ET Room（一个帧同步会话）
DNF 房间/地图 = LSWorld 内的一个关卡状态（LSDungeonComponent 管理）
切换地图 = 清除旧怪物 + 生成新怪物（同一个 LSWorld）
玩家状态 = 自然保留（同一个 LSWorld 里的同一个 LSUnit）
```

---

## 六、核心结论

- **副本就是 Room**，不需要额外的 Scene 或 Fiber
- **怪物���是 LSWorld 里的 LSUnit**，和玩家在同一个逻辑世界里
- 怪物挂 AI 组件（`LSMonsterAIComponent`），玩家挂输入组件（`LSInputComponent`），这是唯一的区别
- 怪物的 AI、移动、伤害计算、通关判定全部在 LSWorld 内确定性执行
- 怪物不需要网络同步任何状态，因为所有客户端跑同一份代码、同一份输入，结果完全一致
- 副本配置（刷什么怪、刷多少、什么位置）由配置表决定，在 `Room.Init` 时读入 LSWorld
