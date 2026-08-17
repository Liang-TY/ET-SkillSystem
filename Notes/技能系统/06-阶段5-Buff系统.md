# 阶段 5：Buff 系统（配置驱动）

> Buff = 有生命周期的效果（燃烧 Tick 伤害、眩晕禁移动、加速...）。用 BuffDef 配置 + Actions 驱动，不用每个 Buff 写一个类。

---

## 0. 速查

- **目标**：`LSBuff`（实例）+ `LSBuffComponent`（容器）+ `BuffDef`/`BuffRegistry`（配置）+ Actions 系统（效果执行）。
- **前提**：阶段 1（数值组件）+ 阶段 4（Cast，命中时施加 Buff）。
- **验证**：给怪物施加"燃烧"Buff → 每 1 秒扣 10 血、持续 3 秒后消失。

---

## 1. 核心

### 1.1 LSBuff（实例）

```csharp
[ChildOf(typeof(LSBuffComponent))]
[MemoryPackable]
public partial class LSBuff : LSEntity, IAwake<int>, ISerializeToEntity {
    [MemoryPackOrder(0)] public int ConfigId;
    [MemoryPackOrder(1)] public long SourceId;
    [MemoryPackOrder(2)] public int RemainingMs;
    [MemoryPackOrder(3)] public int TickTimer;

    // --- Route B 状态标记（视图层轮询 diff 用）---
    [MemoryPackOrder(4)] public bool JustAdded;    // 本帧刚添加
    [MemoryPackOrder(5)] public bool JustRemoved;  // 本帧刚移除
}
```

### Route B 标记规则
- AddBuff 时设 `JustAdded = true`。
- 移除时设 `JustRemoved = true`（移除前，在 Dispose 之前设）。
- LSBuffComponentSystem.LSUpdate 开头清上一帧标记。
- 视图层 IUpdate 读 JustAdded/JustRemoved 播 Buff 图标/特效。

### 1.2 BuffDef（配置）+ BuffRegistry（注册表）

```csharp
[MemoryPackable]
public struct BuffDef {
    public int TotalTime;       // ms
    public int TickTime;        // ms, 0=不 Tick
    public int[] AddActions;    // 添加时执行
    public int[] TickActions;   // Tick 时执行
    public int[] RemoveActions; // 移除时执行
}
```

### 1.3 Actions 系统（框架接口，SkillDefs 实现）

```csharp
public interface ILSAction { void Run(LSUnit owner, int[] param, LSActionRunType runType); }

// 实现示例（SkillDefs）：
[LSAction(LSActionType.NumericChange)]  // 改数值（加减速、禁移动...）
[LSAction(LSActionType.Damage)]         // 扣血
[LSAction(LSActionType.AddBuff)]        // 施加子 Buff
```

### 1.4 LSBuffComponentSystem（Hotfix/Share，倒计时 + Tick + 移除）

每帧：RemainingMs -= dt；到时执行 RemoveActions 并移除；TickTimer 满 → 执行 TickActions。

### 1.5 BuffDef 注册（SkillDefs 启动时）

```csharp
BuffRegistry.Register(BuffIds.Burn, new BuffDef {
    TotalTime = 3000, TickTime = 1000,
    TickActions = new[] { ActionIds.FireDamage },
});
BuffRegistry.Register(BuffIds.Stun, new BuffDef {
    TotalTime = 1000,
    AddActions = new[] { ActionIds.ForbidMoveOn },
    RemoveActions = new[] { ActionIds.ForbidMoveOff },
});
```

---

## 2. 验证

- 怪物挂 Burn Buff → Console 每 1 秒打 `怪物 HP 减 10`，3 秒后 Buff 消失、不再扣。
- 玩家挂 Stun Buff → 1 秒内 WASD 无效（ForbidMove > 0），1 秒后恢复。

---

## 3. 关键

- Buff 效果统一通过 Actions（NumericChange/Damage/AddBuff...），不在 Buff 类里写逻辑。
- 多来源叠加天然支持（两个 Burn 独立实例，各自倒计时）。
- LSBuffComponent 用预分配 `_toRemove` 列表避免每帧 GC。

---

## 4. 进度记录

| 任务 | 状态 | 日期 | 备注 |
|------|------|------|------|
| LSBuff + LSBuffComponent | ⬜ | | |
| BuffDef + BuffRegistry | ⬜ | | |
| Actions 接口 + 分发器 | ⬜ | | |
| 内置 Action（NumericChange/Damage/AddBuff） | ⬜ | | |
| LSBuffComponentSystem | ⬜ | | |
| 燃烧/眩晕 验证 | ⬜ | | |
