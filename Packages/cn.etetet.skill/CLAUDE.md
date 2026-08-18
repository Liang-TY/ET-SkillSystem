# cn.etetet.skill — 帧同步技能系统 CLAUDE.md

> **新会话必读**：本包涉及帧同步确定性 + 回滚安全 + 热更程序集拓扑，规则多且部分脱离 ET 分析器管辖（机器不管，靠本文档 + 运行时守门员强制）。写代码前先对号入座。

## 包概述

技能系统包。四块组成，**归属四个不同程序集**，写代码前先搞清楚目标文件该放哪：

```
Packages/cn.etetet.skill/
├── Scripts/Model/Share/     → ET.Model（热更）    实体：LSCast/LSCastComponent/LSSkillComponent
│                                                   + LSNumeric/LSHitbox/LSCombat/LSInputBuffer
├── Scripts/Hotfix/Share/    → ET.Hotfix（热更）    System：LSSkillComponentSystem/LSHitboxComponentSystem/
│                                                   LSCombatComponentSystem/LSInputBufferComponentSystem
├── Runtime/                 → ET.Skill（热更，F6） 技能框架：SkillLogic/SkillContext/SkillLoader/
│                                                   [SkillId]/LSCastSystem/LSCastComponentSystem
├── DotNet~/                 → ET.SkillContent      游戏内容（独立 dotnet 编译，Unity 永不编译）
│   ├── Skills/*.cs                                技能（过程逻辑）
│   ├── Actions/*.cs                               效果节点（伤害/硬直/挂Buff...，组合用）
│   ├── Buffs/*.cs                                 Buff 配置
│   └── Bullets/*.cs                               投射物配置（速度/射程/碰撞盒/HitActions）
├── Bundles/SkillContent/    → 编译输出 ET.SkillContent.dll.bytes（YooAsset 收集）
└── Editor/SkillCompileTool  → 菜单 ET/Skill/Compile
```

## 程序集拓扑与依赖铁律

```
ET.SkillContent（内容，dotnet） ──→ ET.Skill（框架，热更/F6）──→ ET.Model（实体，热更）
        │                              │                          └→ ET.LSEntity/Core/TrueSync/Aabb/Npkparser
        └→ ET.Core/TrueSync/Npkparser（非热，ScriptAssemblies）
ET.Hotfix ──→ ET.Skill（禁止反向：ET.Skill 不得引用 ET.Hotfix，会循环依赖）
```

- **ET.Skill 引用了 ET.Model → 它必须是热更程序集**（HybridCLR：AOT 不能引用热更）。已加入 loader 的 DllNames（第 5 件套），CodeLoader 在 Model 后 Hotfix 前加载
- **分析器不管 ET.Skill / ET.SkillContent**（管辖名单只有 ET.Core/Model/Hotfix/ModelView/HotfixView）——字段属性语法上随便写，但见下方规范，**语义约束依然全部有效**

## 通用铁律（帧同步 + 回滚，适用于本包所有代码）

| 铁律 | 原因 |
|---|---|
| **逻辑类禁止实例字段**（SkillLogic 子类/SkillContext 是 struct 除外） | 逻辑实例不进快照，有字段=回滚后状态丢失=desync。运行时状态只存 LSCast/实体 |
| **禁 UnityEngine**（Time/Random/Object/GameObject...） | 逻辑层确定性。需要"随机"用 ctx 提供的确定性随机（未来 API），需要时间用 dtMs 参数 |
| **禁 float/double 数学** | 只用 FP/TSVector/TSMath；int 常量运算可以 |
| **禁 System.Random/DateTime/Guid/Environment** | 非确定性源 |
| **禁 lambda/delegate/闭包当数据存** | 不可序列化；回调一律走 override 虚方法或 int id 配置驱动 |
| **禁反射/IO/网络**（SkillLoader 注册扫描除外） | 逻辑层纯计算 |
| 静态字段标 `[StaticField]` | 域重载/热重载语义一致 |
| AABB 构造走 `new AABB{Min=,Max=}`（Id 恒 0） | 静态计数器是 desync 雷（见 AABB.cs） |

## 技能编写规范（ET.SkillContent，`DotNet~/Skills/`）

**一个技能 = 一个文件 = 一个类**，模板：

```csharp
namespace ET
{
    /// <summary>技能一句话描述。数值出处/设计依据写这里。</summary>
    [SkillId(SkillIds.XxxYyy)]        // 常量先加进 Runtime/SkillIdAttribute.cs 的 SkillIds 表
    public class XxxYyy : SkillLogic
    {
        private const int SomeFrame = 3;    // 配置常量用 const（唯一允许的实例字段）

        public override int CooldownMs => 0;        // CD；普攻类=0（动画+取消窗口门禁）
        public override int TotalTimeMs => 360;     // 总时长，到时自动 OnEnd；0=自己控制
        // public override bool ManualCooldown => true;  // 多段技能 OnEnd 才起 CD（默认 false）

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };  // 命中效果组合
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)  { ... }   // 起手（播动画/清命中表）
        public override void OnUpdate(SkillContext ctx, int dtMs) { ... }  // 每帧（取消窗口判定等）
        public override void OnEnd(SkillContext ctx)   { ... }   // 收招（关盒/回默认动画）
    }
}
```

**只能通过 SkillContext 操作世界**（门面）：

| 需求 | API |
|---|---|
| 播动画 | `ctx.PlayAnim(AnimId.X)` / `ctx.PlayDefaultAnim()` |
| 攻击盒（无帧数据技能） | `ctx.SetAttackHitbox(offset, halfExtents)` / `ctx.DisableAttackHitbox()` |
| 输入缓冲（连段取消） | `ctx.PeekBufferedButton()` / `ctx.RestartCurrentSkill()` |
| 数值 | `ctx.AddNumeric(target, NumericType.X, v)` / `ctx.GetNumeric(...)` |
| Buff | `ctx.AddBuff(target, BuffIds.X)` / `ctx.AddBuffToSelf(BuffIds.X)` |
| 查询 | `ctx.GetEnemies()`（共享缓冲勿持有）/ `ctx.CheckHit(a, b)` / `ctx.GetCasterId()` / `ctx.CurrentFrameIndex()` |

有动画判定帧数据的技能（.ani 翻译的）**不用 SetAttackHitbox**——帧驱动自动激活（判定帧=有 attackBoxes 的帧）。

## Action 效果节点规范（`DotNet~/Actions/`，阶段5+）

效果（伤害/硬直/挂Buff/数值增减...）= 无状态节点，**组合**进技能的 `HitActions` 或 Buff 的 `*Actions`：

```csharp
[ActionId(ActionIds.XxxYyy)]   // 常量加进 Runtime/LSAction.cs 的 ActionIds 表
public class XxxYxxAction : LSAction
{
    private const int Value = 10;   // 参数第一版 const 内嵌；luban 化时进表
    public override void Run(LSActionContext ctx) => ctx.DamageOwner(Value);
}
```

LSActionContext 门面（owner=效果作用单位/buff宿主，source=施加者）：`DamageOwner/GetOwnerHp/SetOwnerHitstun/OwnerForbidMove/PlayOwnerAnim/AddBuffToOwner/AddOwnerNumeric/FrameNo/GetOwnerId/GetSourceId`。**不要**在内容层碰 ET.Model 类型（如 NumericType）——要什么数值操作就在 LSActionContext 加门面方法。

## Buff 配置规范（`DotNet~/Buffs/`，阶段5+）

```csharp
[BuffId(BuffIds.Xxx)]           // 常量加进 Runtime/BuffDefinition.cs 的 BuffIds 表
public class XxxBuff : BuffDefinition
{
    public override int TotalTimeMs => 3000;      // 0=永久
    public override int TickTimeMs => 1000;       // 0=不 Tick
    private static readonly int[] AddActionsArr = { ... };   // 添加/每次Tick/移除时执行
    public override int[] AddActions => AddActionsArr;
}
```

叠层简版：同 Buff 再挂 = Stack+1 + 刷新时长（不重跑 AddActions）。Buff 复杂流程（如"受击触发"）不塞配置——那是 SkillLogic/后续系统的职责。

## 投射物配置规范（`DotNet~/Bullets/`，阶段6+）

```csharp
[BulletId(BulletIds.Xxx)]       // 常量加进 Runtime/BulletDefinition.cs 的 BulletIds 表
public class XxxBullet : BulletDefinition
{
    public override FP Speed => 15;               // 单位/秒
    public override int TotalTimeMs => 1500;      // 寿命（射程 = Speed × 时长）
    public override bool DestroyOnHit => false;   // false=穿透（HitTargets 去重）
    public override TSVector HalfExtents => new((FP)5/10, (FP)4/10, (FP)3/10);  // AABB 半尺寸
    public override int[] HitActions => ...;      // 命中效果（节点组合，同技能/Buff）
    public override int ViewAnimId => AnimId.X;   // 视图动画（弹的表现帧视图层自推，逻辑零动画状态）
}
```

发射：技能 OnCast 里 `ctx.CreateBullet(BulletIds.Xxx)`（出生=身前 0.8，方向=施法者朝向）。碰撞=弹 AABB vs 单位受击盒（多盒），不打施法者。区域效果 LSArea 延后（无资源，见 07）。

**运行时守门员**：`ContentLoader.RegisterAssembly`（技能/Buff/Action/Bullet 四类同机制）反射检查内容类实例字段，非 const 字段存在 → 拒绝注册 + 报错。想存状态 → 加到 LSCast/LSBuff/LSBullet 实体（Model/Share）。

## 编译流程（谁编译、怎么触发）

| 改了什么 | 怎么编译 |
|---|---|
| 技能内容（DotNet~/Skills） | Unity 菜单 **ET/Skill/Compile**（dotnet build → .bytes → Bundles/SkillContent） |
| 框架（Runtime/）或实体/System | **F6**（热更 5 件套重编）→ 再 ET/Skill/Compile |
| 规则：本机（开发机）无 Unity | 提交代码 → 测试机拉取 → Unity 验证 |

## 新技能接入清单

1. 动画：.ani → `DnfConfigTranslation`（E:\Projects\cs\parse-img-ani）转 json → 放 `lockstep/Bundles/AnimRes/` → npkparser 加 AnimId 常量 → LSAnimClipRegistrar 注册
2. `SkillIds` 加常量 → `SkillIds.ButtonToSkill` 加按键映射（新按键要改 LSOperaComponentSystem 采集）
3. `DotNet~/Skills/XxxYyy.cs` 按模板写
4. ET/Skill/Compile → Play 验证

## 文档与决策记录

- 阶段执行手册：`Notes/技能系统/00-总览与阶段划分.md`（当前进度）+ 各阶段文档
- 架构定案（独立编译管线/热更拓扑/踩坑记录）：`Notes/技能系统/05-阶段4-Cast与SkillLogic.md` §关键决策
- 未来演进（Actions 效果层/luban 迁移，均延后记档）：`Notes/技能系统/05-阶段4-Cast与SkillLogic.md` §5
