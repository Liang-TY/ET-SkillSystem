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
| LSBuff + LSBuffComponent | ✅ | 2026-08-18 | 纯数据实体（Model）；+Stack 叠层 +Removing（存活一帧服务 Route B 标记后回收，同 LSCast.Finished 模式） |
| BuffDef + BuffRegistry | ✅ | 2026-08-18 | 改为 **BuffDefinition 属性类 + [BuffId] 反射注册**（与技能同构，不用 doc 原方案的手动 Register——要解决"何时调"问题）；TotalTimeMs/TickTimeMs/Add·Tick·RemoveActions 虚属性；static readonly 数组零分配 |
| Actions 接口 + 分发器 | ✅ | 2026-08-18 | **LSAction 基类 + [ActionId] + ActionLoader**（ContentLoader&lt;TAttr,TBase&gt; 泛型统一：扫描+守门员，SkillLoader 已薄壳化复用同机制）；LSActionContext readonly struct 门面（owner/source/frameNo + Damage/Hp/Hitstun/ForbidMove/PlayAnim/AddBuff） |
| 内置 Action | ✅ | 2026-08-18 | MeleeHit（原 ApplyHit 硬编码搬家：50 伤害+500 硬直+受击动画）/ FireDamageTick(10) / ForbidMoveOn·Off / AddBurnBuff。参数 const 内嵌，luban 化进表（05 §5 记档） |
| LSBuffComponentSystem | ✅ | 2026-08-18 | ET.Skill（调 BuffLoader/ActionLoader 防循环依赖）；快照迭代（[StaticField] scratch，tick 动作增删安全）；叠层简版（同 buff 再挂=Stack+1 刷新时长不重跑 AddActions） |
| ApplyHit Actions 化 | ✅ | 2026-08-18 | 伤害/硬直/Hurt 动画全搬进 MeleeHitAction；ApplyHit 只剩 防重记录 + NotifyHit + 分发技能 HitActions |
| 燃烧/眩晕 验证 | ✅ | 2026-08-18 | 全通过：J 命中→[Combat] 伤害50 + 燃烧每秒 `[Buff] 燃烧伤害10 HP=...`×3 后自停；K→自挂 Stun（诊断日志实测 ON 帧97→OFF 帧116 = 950ms≈配置 1000ms，期间 WASD 失效）+ CD 拦截照旧。首测"体感 300ms"为误判——OnEnd(300ms) 是技能自身时长，与 buff 无关 |

**实现记录（2026-08-18）**：
1. **配置形态决策**：Buff/Action 与技能同构（属性类 + 特性反射注册 + 守门员），ET10 spell 对照过（骨架同构：实例+配置+时机化效果节点；差异：它行为树节点+序列化参数+状态同步，我们扁平节点+const 参数+帧同步——见会话记录）。
2. **ContentLoader 泛型**：抽自 SkillLoader；以后 equipment/monster 内容直接复用。SkillLoader 变薄壳，调用方 API 零改动。
3. **引用纪律**：内容层（SkillContent）只见 ET.Skill 门面——NumericType.ForbidMove 这类 ET.Model 常量不进内容，收编为 ctx.OwnerForbidMove(bool)。
4. SkillContentLoader 一次 Assembly.Load 三注册（Skill/Buff/Action）。
5. 挂载顺序：Buffer→Skill→Cast→**Buff**→Hitbox（命中挂 Buff 的 JustAdded 在清标记之后）。
6. ET10 有暂缓的记档位：Stack 上限/刷新策略配置化、互斥组/免疫（BuffFlags）、分组覆盖。
