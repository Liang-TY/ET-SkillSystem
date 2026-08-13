# Half B 执行手册：帧动画驱动 + 怪物创建（消费图集 → 动起来）

> 本文档是**执行手册**（playbook），接在 Half A 之后。
> 配套文档（同文件夹）：`DNF风格2D美术资源与帧动画系统方案.md`（主方案，下称"主文档"）、`HalfA-运行时图集管线.md`（前序）、`参考-npkparser包说明.md`。
> 范围：消费 Half A 产出的 sprite 字典，驱动动画（逻辑层推进帧 + 视图层换帧），并创建一个怪物 unit 原地循环走。
> **联合验收 VT：进战斗场景，看到一个怪物原地循环播放 move.json 的走动画。**

---

## 0. 会话恢复速查（新 agent 先读这段）

- **前提（硬性）**：Half A 已完成并通过 **V2**（图集管线产出正确的 `Sprites` 字典 + `move.json` 已注册为 `AnimId.Walk` + `stay.json` 为 `AnimId.Idle`）。看 `HalfA-运行时图集管线.md` §8 确认全绿。**没过 V2 先回去做 Half A。**
- **Half B 目标**：① 逻辑层 `LSAnimComponent` 按 clip 的 per-frame delay 推进帧（确定性）；② 视图层 `LSSpriteAnimViewComponent` 读逻辑帧索引、diff 换 sprite；③ 创建一个怪物 unit `Play(Walk)`；④ 玩家也挂新系统 `Play(Idle)`、删掉 Mecanim。
- **当前状态**：Half A 完成后，玩家有 2D prefab + SpriteRenderer，但没动画（Half A 的临时静态 sprite 已在 T7 删掉）；怪物还没创建；`LSAnimatorComponent`（Mecanim）还在。**Half B 还没开始实现。**
- **下一步**：按 §5 流程执行；VT 验证要用户进 Unity 手动跑（§7）。
- **新 agent 操作流程**：详 §10。简版 = 确认 Half A 过 V2 → 读本文档 → 读主文档 §4.1-4.3（代码骨架）/ §9（架构）/ §13（状态机）→ 读 §11 真实代码 → 按 §5 执行 → 更新 §8 → 遇问题记 §9 → 到 VT 找用户测。

---

## 1. Half B 的范围

**做（IN scope）：**
- 逻辑层 `LSAnimComponent`（帧状态）+ `LSAnimComponentSystem`（LSUpdate 按 ms 累加器推进帧，余数保留无漂移）。
- 视图层 `LSSpriteAnimViewComponent`（diff 缓存）+ System（读逻辑 FrameIndex，变了就换 SpriteRenderer.sprite + imagePos 偏移）。
- `LSUnitFactory.Init` 给玩家加 `LSAnimComponent` + `Play(Idle)`（**LSInputComponent 保留不动**）。
- `RoomSystem.Init` 玩家循环之后建一个**测试怪物** LSUnit（固定位置、不进 PlayerIds、无输入），`Play(Walk)`。
- `LSUnitViewComponentSystem.InitAsync` 遍历所有 unit（不只玩家），把 `LSAnimatorComponent` 换成 `LSSpriteAnimViewComponent`。
- 删 `LSUnitViewSystem.Update` 里的 `SetFloatValue("Speed",...)` 那行；删 `LSAnimatorComponent` + System。

**不做（OUT of scope，以后再做）：**
- 怪物 AI / 行为状态机（巡逻/追击/攻击）——测试怪物只是 `Play(Walk)` 写死。
- 碰撞 / 伤害盒（damageBox/attackBox 采样）。
- 玩家"移动时切 Walk"（Walk-on-move）——测试期玩家一直 Idle，能接受。
- 怪物移动（寻路）——原地循环走，不移动位置。

---

## 2. 方案 + 关键决策

**方案**：两层分离（主文档 §9）。逻辑层只存 `AnimId/FrameIndex/FrameTick`，按 clip 的 delay 推进帧，确定性、会回滚；视图层零模拟，只 diff 帧索引、变了就换 sprite。怪物和玩家共用同一套组件，区别只在 `Play` 的动作不同。

**关键决策（守好，别违反）：**

| # | 决策 | 原因 |
|---|------|------|
| 1 | 帧状态在逻辑层 `LSAnimComponent`（`[MemoryPackable]` + `ISerializeToEntity`） | 决定第几帧 → 决定伤害盒 → 影响胜负，必须确定性 + 快照回滚 |
| 2 | `LSUpdate` 用 FP 累加器 + while 推进 + 保留余数 | 无漂移；只有确定性的一次性量化抖动（所有客户端一致） |
| 3 | 视图层只 diff `LastAnimId/LastFrameIndex`，没变不碰 SpriteRenderer | 零模拟、省开销 |
| 4 | 翻转沿用 `LSUnitViewSystem` 已有的 `localScale.x`，**别再设 `SpriteRenderer.flipX`** | 两者会抵消 |
| 5 | 玩家的 `LSInputComponent` 和输入/控制逻辑**一行不动** | 只加 `LSAnimComponent`，WASD 照旧 |
| 6 | 怪物不进 `PlayerIds`、不加 `LSInputComponent` | 不是玩家：不吃输入、相机不跟、不被输入分发 |
| 7 | `LSAnimComponentSystem.LSUpdate` 对未注册 clip 走 null 守卫 | clip 在视图层 init 注册，极端早 tick 可能还没注册，守卫防爆 |
| 8 | 怪物用 `AddChild<LSUnit>()` 自动分配 id（确定性，同一创建点所有客户端一致） | 不和玩家 id 冲突；以后正式刷怪改服务端分配 id |

---

## 3. 起点：Half A 交过来的状态 + 要改的现有代码

### 3.1 Half A 交过来的状态（前提）
- `LSAnimResComponent`：持有 `Atlas`（单张图集）+ `Sprites`（`Dictionary<int,Sprite>`），`GetSprite(imgIndex)` 可用。
- `AnimConfigRegistry`：`AnimId.Idle=stay.json`、`AnimId.Walk=move.json` 已注册。
- `LSUnitViewComponentSystem.InitAsync`：已加载 `Unit2D.prefab`、已赋 `SpriteRenderer`（Half A 的 T2）；但还遍历 `PlayerIds`、还 `AddComponent<LSAnimatorComponent>()`。
- 玩家有 2D prefab + SpriteRenderer，但没动画（临时静态 sprite 在 T7 已删）。

### 3.2 当前 `LSUnitFactory.cs`（要改：给玩家加 LSAnimComponent）

```csharp
public static LSUnit Init(LSWorld lsWorld, LockStepUnitInfo unitInfo)
{
    LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
    LSUnit lsUnit = lsUnitComponent.AddChildWithId<LSUnit>(unitInfo.PlayerId);
    lsUnit.Position = unitInfo.Position;
    lsUnit.Rotation = unitInfo.Rotation;
    lsUnit.AddComponent<LSInputComponent>();   // ← 玩家输入，保留不动
    return lsUnit;
}
```

### 3.3 当前 `RoomSystem.cs` Init 玩家循环（要改：循环后加怪物）

```csharp
for (int i = 0; i < unitInfos.Count; ++i)
{
    LockStepUnitInfo unitInfo = unitInfos[i];
    LSUnitFactory.Init(lsWorld, unitInfo);
    self.PlayerIds.Add(unitInfo.PlayerId);
}
// ← Half B 在这之后加怪物
```

### 3.4 当前 `LSUnitViewSystem.cs` Update 里的旧行（要删）

```csharp
// TODO: 改成你的 2D 帧动画逻辑
self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", isMoving ? speed : 0);  // ← Half B 删
```

---

## 4. 任务清单（接 Half A 的 T7，从 T8 起；完成一项勾一项、§8 记日期）

- [ ] **T8** 建 `LSAnimComponent.cs`（`Packages/cn.etetet.lockstep/Scripts/Model/Share/LSAnim/`）：`[ComponentOf(typeof(LSUnit))] [MemoryPackable] : LSEntity, ILSUpdate, IAwake, ISerializeToEntity`，字段 `AnimId/FrameIndex/FrameTick(FP)/Speed(FP)/IsLoop/IsFinished`（代码见 §6.1）。
- [ ] **T9** 建 `LSAnimComponentSystem.cs`（`Hotfix/Share/LSAnim/`）：`Awake`→`Play(Idle)`；`LSUpdate` 累加器推进帧；`Play(animId)` null 守卫 + 重置；`GetCurrentFrame()`（代码见 §6.2）。
- [ ] **T10** 改 `LSUnitFactory.Init`：`lsUnit.AddComponent<LSInputComponent>()` 之后加 `var anim = lsUnit.AddComponent<LSAnimComponent>(); anim.Play(AnimId.Idle);`（玩家：Idle）。
- [ ] **T11** 改 `RoomSystem.Init`：玩家循环之后建测试怪物 `LSUnit monster = lsUnitComponent.AddChild<LSUnit>(); monster.Position = 固定可见位置; monster.AddComponent<LSAnimComponent>().Play(AnimId.Walk);`（代码见 §6.4）。
- [ ] **T12** 建 `LSSpriteAnimViewComponent.cs`（`ModelView/Client/LSAnim/`）：`[ComponentOf(typeof(LSUnitView))] : Entity, IAwake, IUpdate, ILSRollback`，字段 `SpriteRenderer/LastAnimId=-1/LastFrameIndex=-1`（代码见 §6.5）。
- [ ] **T13** 建 `LSSpriteAnimViewComponentSystem.cs`（`HotfixView/Client/LSAnim/`）：`Awake` 从父 LSUnitView 拿 SpriteRenderer；`Update` diff 换帧 + imagePos 偏移；`LSRollback` 重置缓存（代码见 §6.6）。
- [ ] **T14** 改 `LSUnitViewComponentSystem.InitAsync`：循环从 `foreach playerId` 改成遍历 `lsUnitComponent.Children`；`AddComponent<LSAnimatorComponent>()` 改成 `AddComponent<LSSpriteAnimViewComponent>()`（prefab 路径 + SpriteRenderer 赋值沿用 Half A，不动）（代码见 §6.7）。
- [ ] **T15** 改 `LSUnitViewSystem.Update`：删掉 `SetFloatValue("Speed",...)` 那行 + 它的 TODO 注释。
- [ ] **T16** 删 `LSAnimatorComponent.cs`（ModelView/Client）+ `LSAnimatorComponentSystem.cs`（HotfixView/Client）。
- [ ] **VT** 联合验证：用户进 Unity Play，看到怪物原地循环走 move.json（§7）。

---

## 5. 执行流程（按顺序）

```
T8  LSAnimComponent（逻辑帧状态）
T9  LSAnimComponentSystem（推进帧）       ── 逻辑层就绪，可单测 FrameIndex 是否推进
   ↓
T10 LSUnitFactory 加 LSAnimComponent（玩家 Idle）
T11 RoomSystem.Init 加怪物（Walk）        ── 逻辑层完成：两个 unit 都有动画状态
   ↓
T12 LSSpriteAnimViewComponent（视图 diff 缓存）
T13 LSSpriteAnimViewComponentSystem（换帧）
T14 LSUnitViewComponentSystem 换组件 + 遍历所有 unit
T15 删 SetFloatValue 行 ──→ 【VT：进场景看到怪物循环走 move.json】
   ↓
T16 删 LSAnimatorComponent（清理）→ Half B 收工
```

> T8/T9 可先于视图层做，逻辑层独立。T14 是接合点：视图层一旦换成 LSSpriteAnimViewComponent 并遍历所有 unit，怪物就会出现并动起来。

---

## 6. 关键代码参考

### 6.1 LSAnimComponent（逻辑 Model/Share）

```csharp
// Packages/cn.etetet.lockstep/Scripts/Model/Share/LSAnim/LSAnimComponent.cs
using MemoryPack;
using TrueSync;
namespace ET
{
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSAnimComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        public int AnimId;            // AnimId.Idle=1, Walk=2, ...
        public int FrameIndex;        // 当前帧
        public FP FrameTick;          // 累积毫秒（FP 定点数）
        public FP Speed = FP.One;     // 播放倍率
        public bool IsLoop = true;
        public bool IsFinished;       // 非循环动画是否播完
    }
}
```

### 6.2 LSAnimComponentSystem（逻辑 Hotfix/Share，推进帧）

```csharp
// Packages/cn.etetet.lockstep/Scripts/Hotfix/Share/LSAnim/LSAnimComponentSystem.cs
namespace ET
{
    [EntitySystemOf(typeof(LSAnimComponent))]
    [LSEntitySystemOf(typeof(LSAnimComponent))]
    public static partial class LSAnimComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAnimComponent self) => self.Play(AnimId.Idle);

        [LSEntitySystem]
        private static void LSUpdate(this LSAnimComponent self)
        {
            if (self.IsFinished) return;
            AnimClipData clip = AnimConfigRegistry.Get(self.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return;   // null 守卫

            self.FrameTick += (FP)LSConstValue.UpdateInterval * self.Speed; // 余数保留 → 无漂移
            int delay = clip.frames[self.FrameIndex].delay;
            if (delay <= 0) delay = LSConstValue.UpdateInterval;

            while (!self.IsFinished && self.FrameTick >= (FP)delay)
            {
                self.FrameTick -= (FP)delay;
                if (++self.FrameIndex >= clip.frames.Length)
                {
                    if (self.IsLoop) self.FrameIndex = 0;
                    else { self.FrameIndex = clip.frames.Length - 1; self.IsFinished = true; break; }
                }
                delay = clip.frames[self.FrameIndex].delay;
                if (delay <= 0) delay = LSConstValue.UpdateInterval;
            }
        }

        public static void Play(this LSAnimComponent self, int animId)
        {
            AnimClipData clip = AnimConfigRegistry.Get(animId);
            if (clip == null) { self.AnimId = animId; return; }   // 未注册先记 id，等注册了再播
            self.AnimId = animId; self.FrameIndex = 0; self.FrameTick = FP.Zero;
            self.Speed = FP.One; self.IsLoop = clip.loop; self.IsFinished = false;
        }

        public static AnimFrameData GetCurrentFrame(this LSAnimComponent self)
        {
            AnimClipData clip = AnimConfigRegistry.Get(self.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return default;
            return clip.frames[self.FrameIndex];
        }
    }
}
```

### 6.3 LSUnitFactory 改动（玩家加 LSAnimComponent + Idle）

```csharp
public static LSUnit Init(LSWorld lsWorld, LockStepUnitInfo unitInfo)
{
    LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
    LSUnit lsUnit = lsUnitComponent.AddChildWithId<LSUnit>(unitInfo.PlayerId);
    lsUnit.Position = unitInfo.Position;
    lsUnit.Rotation = unitInfo.Rotation;
    lsUnit.AddComponent<LSInputComponent>();          // 玩家输入，保留不动
    lsUnit.AddComponent<LSAnimComponent>();           // ← Half B 加：Awake 自动 Play(Idle)
    return lsUnit;
}
```

### 6.4 RoomSystem.Init 改动（玩家循环后建测试怪物）

```csharp
for (int i = 0; i < unitInfos.Count; ++i)
{
    LockStepUnitInfo unitInfo = unitInfos[i];
    LSUnitFactory.Init(lsWorld, unitInfo);
    self.PlayerIds.Add(unitInfo.PlayerId);
}

// —— Half B 测试桩：建一个怪物原地循环走 move.json ——
LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
LSUnit monster = lsUnitComponent.AddChild<LSUnit>();                 // 自动 id，确定性
monster.Position = new TSVector(3, 0, 0);                            // 固定可见位置，按需调
monster.AddComponent<LSAnimComponent>().Play(AnimId.Walk);           // 怪物：走（move.json）
// 注意：不 add LSInputComponent、不 Add 到 PlayerIds
```

> 正式刷怪（配置驱动、服务端分配 id）以后再做；这是测试桩。

### 6.5 LSSpriteAnimViewComponent（视图 ModelView/Client）

```csharp
// Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSAnim/LSSpriteAnimViewComponent.cs
using UnityEngine;
namespace ET.Client
{
    [ComponentOf(typeof(LSUnitView))]
    public class LSSpriteAnimViewComponent : Entity, IAwake, IUpdate, ILSRollback
    {
        public SpriteRenderer SpriteRenderer;
        public int LastAnimId = -1;
        public int LastFrameIndex = -1;
    }
}
```

### 6.6 LSSpriteAnimViewComponentSystem（视图 HotfixView/Client，diff 换帧）

```csharp
// Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSAnim/LSSpriteAnimViewComponentSystem.cs
using UnityEngine;
namespace ET.Client
{
    [EntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [LSEntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [FriendOf(typeof(LSSpriteAnimViewComponent))]
    public static partial class LSSpriteAnimViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSSpriteAnimViewComponent self)
            => self.SpriteRenderer = self.GetParent<LSUnitView>().SpriteRenderer;

        [EntitySystem]
        private static void Update(this LSSpriteAnimViewComponent self)
        {
            if (self.SpriteRenderer == null) return;
            LSUnitView view = self.GetParent<LSUnitView>();
            LSAnimComponent anim = view.Unit?.GetComponent<LSAnimComponent>();
            if (anim == null) return;
            if (anim.AnimId == self.LastAnimId && anim.FrameIndex == self.LastFrameIndex) return; // diff

            AnimFrameData frame = anim.GetCurrentFrame();
            LSAnimResComponent res = (self.IScene as Room)?.GetComponent<LSAnimResComponent>();
            Sprite sprite = res?.GetSprite(frame.image.index);
            if (sprite == null) return;

            self.SpriteRenderer.sprite = sprite;
            self.SpriteRenderer.transform.localPosition =
                new Vector3(frame.imagePos.x / 100f, frame.imagePos.y / 100f, 0f);   // 100ppu
            self.LastAnimId = anim.AnimId;
            self.LastFrameIndex = anim.FrameIndex;
        }

        [LSEntitySystem]
        private static void LSRollback(this LSSpriteAnimViewComponent self)
        { self.LastAnimId = -1; self.LastFrameIndex = -1; }   // 逻辑快照已恢复，强制下次重同步
    }
}
```

### 6.7 LSUnitViewComponentSystem.InitAsync 改动（遍历所有 unit + 换组件）

```csharp
// 沿用 Half A：animRes.InitAsync() 在最前面已建好图集（不改）
// 改两处：循环遍历所有 unit；AddComponent 换成 LSSpriteAnimViewComponent
foreach (var kv in lsUnitComponent.Children)                 // ← 原 foreach (long playerId in room.PlayerIds)
{
    LSUnit lsUnit = (LSUnit)kv.Value;
    // Half A 已改：加载 Unit2D.prefab、实例化、赋 SpriteRenderer（这些不动）
    GameObject prefab = await room.GetComponent<ResourcesLoaderComponent>()
        .LoadAssetAsync<GameObject>("Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab");
    GameObject unitGo = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
    unitGo.transform.position = lsUnit.Position.ToVector();

    LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
    lsUnitView.SpriteRenderer = unitGo.GetComponentInChildren<SpriteRenderer>();      // Half A
    lsUnitView.AddComponent<LSSpriteAnimViewComponent>();   // ← 原 AddComponent<LSAnimatorComponent>()
}
```

> 注意 `Children` 迭代时不要在循环体里往 `lsUnitComponent` 加东西（这里加的是 LSUnitView，挂在 LSUnitViewComponent 下，安全）。

---

## 7. 联合验证 VT（需要你进 Unity 手动跑）

**前提**：Half A 的 V2 已通过（图集本身渲染正确）。V2 没过先回 Half A。

**步骤：**
1. 打开战斗场景，Play。
2. 看：场景里**玩家**显示 stay.json 的 Idle 帧（可能随 WASD 移动，但帧不切——测试期不接 Walk-on-move，正常）。
3. 看：玩家旁边（位置 3,0,0 附近）**有一个怪物**，显示 BantuAmazones，**在原地循环走**（move.json 的 6 帧 9-14 来回切换）。

**通过判据**：怪物可见 + 持续循环切换 walk 的帧（不是静止一帧）。

**排查表：**

| 现象 | 大概率原因 | 查哪 |
|------|-----------|------|
| 怪物完全看不到 | 位置出屏 / prefab 没登记 AssetBundleCollector / SpriteRenderer 没赋值 | 调 monster.Position；查 Collector；查 T14 赋值 |
| 怪物可见但静止一帧不切 | LSAnimComponent 没推进 / clip 没注册成 Walk | 临时在 LSUpdate 打印 FrameIndex 看涨不涨；查 AnimConfigRegistry.Get(Walk) 是否 null |
| 怪物显示但图错乱/颠倒 | 这是 Half A 的图集问题，V2 应该已挡住 | 回 Half A 复查 V2 |
| 玩家也不见 | 同上 prefab/SpriteRenderer 问题；或 LSAnimComponent 没挂 | 查 T10 |
| 报错找不到 LSAnimatorComponent | T15/T16 删它后某处还引用 | 全局搜 LSAnimatorComponent 残留 |

**通过后**：T16 删 Mecanim 清理 → Half B 收工，§8 全绿。整个 A+B 闭环达成：怪物创建 + 循环走动画。

---

## 8. 实施进度（agent 每次更新）

| 任务 | 状态 | 日期 | 备注 |
|------|------|------|------|
| 前提：Half A V2 通过 | 🟡 待测 | | 与 VT 一起在 Unity 里测 |
| T8 LSAnimComponent | ✅ 完成 | 2026-08-11 | Model/Share/LSAnim/ |
| T9 LSAnimComponentSystem | ✅ 完成 | 2026-08-11 | LSUpdate FP 累加器推进帧 |
| T10 LSUnitFactory 加 LSAnimComponent | ✅ 完成 | 2026-08-11 | 玩家 Idle（保留 LSInputComponent） |
| T11 RoomSystem 加怪物 | ✅ 完成 | 2026-08-11 | 固定位置 (3,0,0)，Play(Walk)，不进 PlayerIds |
| T12 LSSpriteAnimViewComponent | ✅ 完成 | 2026-08-11 | ModelView/Client/LSAnim/ |
| T13 LSSpriteAnimViewComponentSystem | ✅ 完成 | 2026-08-11 | diff 换帧 + imagePos 偏移 + LSRollback |
| T14 LSUnitViewComponentSystem 换组件 | ✅ 完成 | 2026-08-11 | 遍历所有 unit + 换 LSSpriteAnimViewComponent + 删 T6 临时代码 |
| T15 删 SetFloatValue 行 | ✅ 完成 | 2026-08-11 | 连 LSInput/isMoving 临时变量一起删 |
| T16 删 LSAnimatorComponent | ✅ 完成 | 2026-08-11 | git rm 2 .cs + 2 .meta |
| VT 验证通过 | 🟡 待测 | | 等用户进 Unity Play |

状态：⬜未开始 / 🟡进行中 / ✅完成 / ⚠️阻塞

---

## 9. 问题记录（遇到就加）

| # | 问题 | 原因 | 对策 | 状态 |
|---|------|------|------|------|
| | | | | |

---

## 10. 新会话 / 新 agent 操作指引（playbook）

**开场，按顺序：**

1. **先确认前提**：打开 `HalfA-运行时图集管线.md` §8，确认 Half A 全绿 + V2 通过。没过 → 先做 Half A，别提前做 B。
2. **读本文件夹文档**：本文档 → 主文档 §4.1-4.3（代码骨架）/ §9（两层架构）/ §13（状态机，Half B 暂不用，了解即可）。
3. **读真实代码**（§11，读真的别读副本）：`LSUnitFactory.cs`、`RoomSystem.cs`、`LSUnitViewComponentSystem.cs`、`LSUnitViewSystem.cs`、`LSUnitView.cs`、`LSAnimatorComponent.cs`+System（要删的那套）。
4. **看 §8 进度**，从下一个 ⬜ 接着做。
5. **按 §5 + §6 执行**。每完成一项：§8 改 ✅ + 日期。遇问题加 §9，阻塞改 ⚠️。
6. **到 VT**：本文档没法自测，**告诉用户去 Unity Play**（§7），等反馈再继续/清理。

**铁律：**

- ✏️ 只改真实文件（§11），别改本文件夹副本。
- 🔁 代码以实际文件为准，动手前先读。
- 🚧 **不碰玩家输入/控制**（`LSInputComponent`、`LSInputComponentSystem`、`RoomSystem.Update` 输入分发、相机跟随一行不动）。
- 🚧 不越界：不做 AI/碰撞/Walk-on-move/寻路（怪物原地走 move.json 就行）。
- 🧱 守 §2 的 8 条决策（尤其帧状态在逻辑层、null 守卫、翻转走 scale.x、怪物不进 PlayerIds）。
- 🗑 T16 删 LSAnimatorComponent 前先 T15 删引用，保证编译链不断。

**完成判据**：VT 通过（怪物原地循环走 move.json）+ §8 全绿 + T16 清理完。

---

## 11. 引用

**本文件夹（参考文档，只读）：**
- `DNF风格2D美术资源与帧动画系统方案.md` — 主方案（§4.1-4.3 代码、§9 架构、§13 状态机）
- `HalfA-运行时图集管线.md` — 前序（确认 V2 通过）
- `参考-npkparser包说明.md` — 解析器/AnimClipData/AnimConfigRegistry API

**要读/改的真实代码文件：**
| 文件 | 操作 |
|------|------|
| `Packages/cn.etetet.lockstep/Scripts/Hotfix/Share/LSUnitFactory.cs` | 改（T10：加 LSAnimComponent） |
| `Packages/cn.etetet.lockstep/Scripts/Hotfix/Share/RoomSystem.cs` | 改（T11：加怪物） |
| `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSUnitViewComponentSystem.cs` | 改（T14：遍历所有 unit + 换组件） |
| `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSUnitViewSystem.cs` | 改（T15：删 SetFloatValue 行） |
| `Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSUnitView.cs` | 读（SpriteRenderer 字段、Unit 引用） |
| `Packages/cn.etetet.lockstep/Scripts/Model/Share/LSAnim/LSAnimComponent.cs` | 新建（T8） |
| `Packages/cn.etetet.lockstep/Scripts/Hotfix/Share/LSAnim/LSAnimComponentSystem.cs` | 新建（T9） |
| `Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSAnim/LSSpriteAnimViewComponent.cs` | 新建（T12） |
| `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSAnim/LSSpriteAnimViewComponentSystem.cs` | 新建（T13） |
| `Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSAnimatorComponent.cs` | 删（T16） |
| `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSAnimatorComponentSystem.cs` | 删（T16） |
