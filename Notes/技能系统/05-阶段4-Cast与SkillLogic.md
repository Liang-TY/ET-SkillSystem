# 阶段 4：Cast 与 SkillLogic（技能有生命周期）

> 把阶段 3 的"按住就攻击"升级为"按一下放一个技能"——技能有 OnCast/OnUpdate/OnEnd 生命周期、冷却、施法者/目标位置。

---

## 0. 速查

- **目标**：实现 `LSCast`（一次技能施放实例，存所有运行时状态）+ `SkillLogic`（无状态基类）+ `LSCastComponent`（容器）+ `LSSkillComponent`（技能槽位/冷却/施法驱动）。
- **前提**：阶段 1-3。
- **验证**：玩家按攻击键 → 创建一个 LSCast → OnCast 激活攻击盒 → 几帧后 OnEnd 关闭 → 冷却中不能再放。

---

## ⚠️ 关键决策：技能代码放哪、怎么编译

> **项目命名**：不叫"SkillDefs"——因为不只技能，还有装备/怪物数据。叫 **GameContent**（所有可热更的游戏内容的家）。早期全 C# 一个 DLL，详见 `Notes/dnf源码研究/配置方案选型与DNF翻译管线.md` §11。

技能编译有两种模式（demo / 开发期用第一种，不搞 DotNet~ DLL）：

| | 开发期（现在用） | 发布期（以后联机热更才用） |
|---|---|---|
| 技能 .cs 放哪 | `skill/Scripts/Hotfix/Share/Skills/*.cs` | `skill/Skills/*.cs` → DotNet~ 编译成 DLL |
| 谁编译 | **Unity 自动编译**（asmref → ET.Hotfix 程序集） | `dotnet build` → HybridCLR 加载 |
| 能断点 | ✅ 正常打 | ❌ 要配 HybridCLR |
| 你现在需要吗 | ✅ **就用这个** | ❌ 不碰 |

**技能写法**：在 `skill/Scripts/Hotfix/Share/Skills/` 下建普通 .cs，继承 `SkillLogic`，打 `[SkillId(n)]` 特性。Unity 自动编译，跟写 LSAnimComponent 没区别。SkillLoader 扫描 `[SkillId]` 自动注册。

以后联机热更时，把 `Skills/` 目录迁到 DotNet~ 编译管线——代码本身不用改（SkillLogic 子类还是那些），只是换个编译方式。

---

## 1. 核心组件

### 1.1 LSCast（skill 包 Model/Share）

一次技能施放，**所有状态存这里**，SkillLogic 无状态。

```csharp
[ChildOf(typeof(LSCastComponent))]
[MemoryPackable]
public partial class LSCast : LSEntity, ILSUpdate, IAwake<int>, ISerializeToEntity
{
    [MemoryPackOrder(0)] public int SkillId;
    [MemoryPackOrder(1)] public long CasterId;
    [MemoryPackOrder(2)] public int ElapsedMs;
    [MemoryPackOrder(3)] public bool Finished;
    [MemoryPackOrder(4)] public TSVector TargetPosition;
    [MemoryPackOrder(5)] public int TotalTimeMs;
    [MemoryPackOrder(6)] public List<long> TargetIds;  // 已命中目标
    // 技能专用状态（各技能按需）
    [MemoryPackOrder(7)] public int Phase;
    // 连段子状态（DNF setSkillSubState 机制，三段斩实证）
    [MemoryPackOrder(11)] public int SubState;

    // --- Route B 状态标记（视图层轮询 diff 用）---
    [MemoryPackOrder(8)] public bool JustStarted;    // 本帧刚 OnCast
    [MemoryPackOrder(9)] public bool JustHit;        // 本帧刚命中（TargetIds 变化）
    [MemoryPackOrder(10)] public bool JustFinished;  // 本帧刚结束

    [MemoryPackIgnore] private SkillLogic _logic;  // 回滚后 null，自动重建
    public SkillLogic GetLogic() => _logic ??= SkillLoader.Create(SkillId);
}
```

### Route B 标记的设/清规则

在 `LSCastSystem.LSUpdate` 里：
- **开头**清上一帧标记：`self.JustStarted = self.JustHit = self.JustFinished = false;`
- **OnCast 调用后**：`self.JustStarted = true;`
- **命中检测后**（TargetIds 新增了）：`self.JustHit = true;`
- **结束时**：`self.JustFinished = true;`

视图层（阶段 7）IUpdate 读这些标记，true 就播表现（起手特效/命中火花/清理）。回滚后从快照恢复，视图重新 diff，不会重放历史。

### 1.2 SkillLogic + ISkillLogic（无状态）

```csharp
public interface ISkillLogic {
    int CooldownMs { get; }
    void OnCast(SkillContext ctx, LSCast cast);
    void OnUpdate(SkillContext ctx, LSCast cast, int dtMs);
    void OnEnd(SkillContext ctx, LSCast cast);
}

public abstract class SkillLogic : ISkillLogic {
    public virtual int CooldownMs => 0;
    public virtual int TotalTimeMs => 0;
    public virtual void OnCast(SkillContext ctx, LSCast cast) { }
    public virtual void OnUpdate(SkillContext ctx, LSCast cast, int dtMs) { }
    public virtual void OnEnd(SkillContext ctx, LSCast cast) { }
}
```

### 1.3 SkillContext（技能 API）

SkillContext 封装了技能逻辑里用到的所有操作。SkillLogic 的 OnCast/OnUpdate/OnEnd 通过它访问世界、创建实体、改数值、设碰撞框。

```csharp
public class SkillContext
{
    public readonly LSWorld World;
    public readonly LSUnit Caster;
    public readonly LSCast Cast;

    public SkillContext(LSWorld world, LSUnit caster, LSCast cast)
    {
        World = world; Caster = caster; Cast = cast;
    }

    // ---- 创建实体 ----
    public LSCast CreateCast(int skillId, TSVector targetPos);       // 触发子技能
    public LSBullet CreateBullet(                                     // 发射弹道
        TSVector origin, TSVector direction, FP speed,
        LSShapeData shape, int bulletConfigId);
    public LSArea CreateArea(                                         // 创建区域效果（配置驱动，无 lambda）
        LSShapeData shape, int durationMs, int tickIntervalMs,
        int[] enterActionIds, int[] tickActionIds, int[] exitActionIds);

    // ---- Buff ----
    public void AddBuff(LSUnit target, int buffId);
    public void RemoveBuff(LSUnit target, int buffId);

    // ---- 数值（纯 int key） ----
    public void AddNumeric(LSUnit target, int numericKey, FP value);
    public FP GetNumeric(LSUnit target, int numericKey);

    // ---- 碰撞框 ----
    public void SetAttackHitbox(TSVector offset, TSVector halfExtents);  // 激活攻击盒
    public void DisableAttackHitbox();                                    // 关攻击盒

    // ---- 移动 ----
    public void MoveCasterTo(TSVector position);
    public void MoveUnitTo(LSUnit unit, TSVector position);

    // ---- 技能替换（变身换技能用）----
    public void ReplaceSkill(int slot, int newSkillId);
    public void RestoreSkill(int slot);

    // ---- 查询 ----
    public List<LSUnit> GetEnemies();                     // 获取所有敌方单位
    public List<LSUnit> GetEnemiesInShape(LSShapeData shape);  // 形状内的敌人
    public bool CheckHit(LSUnit attacker, LSUnit target);      // 攻击盒 vs 受击盒相交检测
}
```

### 1.4 LSCastSystem（Hotfix/Share，逻辑驱动）

```csharp
[LSEntitySystem]
private static void LSUpdate(this LSCast self) {
    // Route B：清上一帧标记
    self.JustStarted = self.JustHit = self.JustFinished = false;

    if (self.Finished) return;
    self.ElapsedMs += LSConstValue.UpdateInterval;
    // 委托 SkillLogic.OnUpdate
    // 命中时设 self.JustHit = true
    // 自动结束检查（TotalTimeMs）→ 设 self.JustFinished = true; self.Finished = true;
}
```

### 1.5 LSSkillComponent + 施放

玩家身上挂 `LSSkillComponent`（技能槽位 + 冷却计时）。按键 → `TryCast(skillId, targetPos)` → 检查冷却 → 创建 LSCast → 设 `cast.JustStarted = true`。

### 1.6 SkillLoader（工厂 + 注册）

扫描 `[SkillId(n)]` 特性的 SkillLogic 子类（在 ET.Hotfix 程序集里），注册工厂。

---

## 2. 写一个技能：完整举例

**文件位置**：`Packages/cn.etetet.skill/Scripts/Hotfix/Share/Skills/NormalAttack.cs`

```csharp
using TrueSync;

namespace ET
{
    [SkillId(1)]  // SkillIds.NormalAttack = 1
    public class NormalAttack : SkillLogic
    {
        public override int CooldownMs => 500;   // 0.5 秒冷却
        public override int TotalTimeMs => 300;   // 技能持续 0.3 秒后自动结束

        public override void OnCast(SkillContext ctx, LSCast cast)
        {
            // 施法瞬间：激活攻击盒（在玩家面前）
            // offset：攻击盒相对玩家的偏移；halfExtents：攻击盒半尺寸
            ctx.SetAttackHitbox(new TSVector(2, 0, 0), new TSVector(1, 1, 1));
        }

        public override void OnEnd(SkillContext ctx, LSCast cast)
        {
            // 收招：关攻击盒
            ctx.DisableAttackHitbox();
        }
    }
}
```

**写完后**：
1. Unity 自动编译（跟 LSAnimComponent 一样，asmref 编进 ET.Hotfix）。
2. SkillLoader.Init() 扫描到 `[SkillId(1)]` → 注册工厂。
3. 玩家按键 → `LSSkillComponent.TryCast(1, targetPos)` → 创建 LSCast → OnCast（激活攻击盒）→ 300ms 后 OnEnd（关攻击盒）→ 冷却 500ms。

**再举个复杂点的：冲撞**

```csharp
// Packages/cn.etetet.skill/Scripts/Hotfix/Share/Skills/ChargeSkill.cs
[SkillId(1001)]
public class ChargeSkill : SkillLogic
{
    public override int CooldownMs => 5000;
    public override int TotalTimeMs => 2000;  // 2 秒超时自动结束

    public override void OnCast(SkillContext ctx, LSCast cast)
    {
        // 算冲撞目标位置，状态存 cast（不存 this，因为 SkillLogic 无状态）
        TSVector dir = cast.TargetPosition - ctx.Caster.Position;
        if (dir.Length() > 800) dir = dir.normalized * 800;
        cast.SavedPos = ctx.Caster.Position + dir;
        ctx.SetAttackHitbox(new TSVector(0, 0, 5), new TSVector(5, 5, 10));
    }

    public override void OnUpdate(SkillContext ctx, LSCast cast, int dtMs)
    {
        // 每帧往前冲，碰到敌人扣血
        LSUnit caster = ctx.Caster;
        TSVector toTarget = cast.SavedPos - caster.Position;
        FP moveAmount = (FP)30 * (FP)dtMs / 1000;

        if (moveAmount >= toTarget.Length())
        {
            caster.Position = cast.SavedPos;  // 到了
            // 不主动结束，等 TotalTimeMs 超时或 OnEnd
        }
        else
        {
            caster.Position += toTarget.normalized * moveAmount;
        }

        // 碰撞扣血
        foreach (LSUnit enemy in ctx.GetEnemies())
        {
            if (cast.TargetIds.Contains(enemy.Id)) continue;  // 防重
            if (ctx.CheckHit(caster, enemy))
            {
                ctx.AddNumeric(enemy, NumericType.Hp, -(FP)100);
                cast.TargetIds.Add(enemy.Id);
            }
        }
    }

    public override void OnEnd(SkillContext ctx, LSCast cast)
        => ctx.DisableAttackHitbox();
}
```

**两个技能文件都是普通 .cs，放 `Scripts/Hotfix/Share/Skills/`，Unity 自动编译。不需要 DotNet~、不需要 DLL、能断点。**

---

## 3. 回滚兼容关键

- SkillLogic **必须无状态**（回滚后 LSCast 的 _logic 重建，状态从 LSCast 的 [MemoryPackable] 字段恢复）。
- **不用 lambda/delegate**（不可序列化）。
- 区域效果/Buff 的回调用 `int[] actionIds` 配置驱动（阶段 5/6）。
- **CD 双机制**（DNF 实证）：默认 TryCast 成功时自动进 CD（.skl [auto cooltime apply]）；多段技能支持延迟到 OnEnd 手动起（=startSkillCoolTime，三段斩实证在 onEndState 才 startSkillCoolTime）。LSSkillComponent 加 `ManualCooldown` 开关——默认 false（TryCast 即 CD），多段技能设 true（OnEnd 才 CD）。

---

## 4. 进度记录

| 任务 | 状态 | 日期 | 备注 |
|------|------|------|------|
| LSCast + LSCastComponent | ⬜ | | |
| SkillLogic + SkillContext | ⬜ | | |
| LSCastSystem（逻辑驱动 + Route B 标记） | ⬜ | | |
| LSSkillComponent（槽位/冷却） | ⬜ | | |
| SkillLoader（扫描 [SkillId] 注册） | ⬜ | | |
| NormalAttack 技能 + 验证 | ⬜ | | 放 Scripts/Hotfix/Share/Skills/ |
