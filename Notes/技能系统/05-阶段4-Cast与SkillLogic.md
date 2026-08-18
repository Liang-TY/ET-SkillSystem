# 阶段 4：Cast 与 SkillLogic（技能有生命周期）

> 把阶段 3 的"按住就攻击"升级为"按一下放一个技能"——技能有 OnCast/OnUpdate/OnEnd 生命周期、冷却、施法者/目标位置。

---

## 0. 速查

- **目标**：实现 `LSCast`（一次技能施放实例，存所有运行时状态）+ `SkillLogic`（无状态基类）+ `LSCastComponent`（容器）+ `LSSkillComponent`（技能槽位/冷却/施法驱动）。
- **前提**：阶段 1-3。
- **验证**：玩家按攻击键 → 创建一个 LSCast → OnCast 激活攻击盒 → 几帧后 OnEnd 关闭 → 冷却中不能再放。

---

## ⚠️ 关键决策：技能代码放哪、怎么编译（2026-08-18 定案）

> 一次到位，不分开发期/发布期。背景（为什么不是"技能放 Hotfix"）：
> 1. **ET 程序集规则**：Hotfix 禁属性/禁非 const 字段/非静态类须 [EnableClass]——技能类要写 `CooldownMs => 500` 配置属性，在 Hotfix 写不了
> 2. **用户工作流要求**：改技能数值不能触发 Unity 自动编译；要独立 csproj 手动触发编译，DLL 改名 .bytes 走资源加载（loader 模式）
> 3. **HybridCLR 铁律**：AOT 程序集不能引用热更程序集（ET.Model 等 4 件套）→ 引用了 ET.Model 类型的 ET.Skill 框架必须自己也是热更程序集

### 最终架构（三块）

```
① ET.Skill 框架 = 第 5 个热更程序集（Unity/F6 编译，asmdef）
   skill/Runtime/ET.Skill.asmdef（热更约束同 ET.Model；引用 Model/LSEntity/Core/TrueSync/Aabb/Npkparser/**MemoryPack**——实体生成代码的继承链碰 IMemoryPackable，漏了会 CS0012）
   内容：SkillLogic（无状态基类）/ SkillIdAttribute+SkillIds / SkillLoader（RegisterAssembly 反射注册单例）
        / SkillContext（readonly struct——值传递零 GC，技能只见门面 API 不见实体类型）
        / LSCastSystem + LSCastComponentSystem（生命周期机制必须在这层：SkillContext.RestartCurrentSkill
          需要它们，放 ET.Hotfix 会循环依赖）
   loader 包配套：AssemblyTool.DllNames 加 "ET.Skill"；CodeLoader 加载 ET.Skill.dll.bytes（Model 后 Hotfix 前）

② ET.SkillContent 内容 = 独立 dotnet csproj（Unity 永不编译）
   skill/DotNet~/ET.SkillContent.csproj + Skills/*.cs（NormalAttack/TestCooldownSkill，属性随便写）
   引用（混合 HintPath）：ET.Skill ← Temp/Bin/Debug（F6 产物）；ET.Core/TrueSync/Npkparser ← Library/ScriptAssemblies
   工作流：改技能 → 菜单 ET/Skill/Compile（不绑快捷键）→ dotnet build → 拷 .dll.bytes/.pdb.bytes
        → skill/Bundles/SkillContent/ → YooAsset 收集。改框架 → 先 F6 再菜单。

③ 运行时加载链
   CodeLoader 载热更 5 件套 → SkillContentLoader（skill 包 HotfixView，room.Init 前、LSAnimClipRegistrar 同时机）
   载 ET.SkillContent.dll.bytes → Assembly.Load → SkillLoader.RegisterAssembly（扫 [SkillId]）
```

### 关键事实备忘（踩过/验证过的坑）

- ET 的分析器管辖名单（AnalyzeAssembly）按程序集名字面匹配：ET.Core/Model/Hotfix/ModelView/HotfixView——**ET.Skill 不在名单**，字段属性随便写，[EnableClass] 也不用加
- ET.Model 等 4 件套在 editor 下 Unity 正常编译进内存，但**进 Play 时 AssemblyEditor 把文件从 ScriptAssemblies 删掉**（ETModelLoadFromBytes.json 存在时），运行时��� Bundles/Code/*.bytes 加载 → **csproj 引用热更 DLL 只能用 Temp/Bin/Debug（F6 产物），不能用 ScriptAssemblies**
- [EnableClass] 是纯分析器标记，不进 Generator，零生成零 GC；真正的 GC 点是 new SkillContext——改 readonly struct 消除（24 字节栈拷贝）
- AOT 不能引用热更，但**热更引用热更随便**（SkillContent 引用 ET.Skill/ET.Model 合法）；dotnet 编译 ≠ AOT
- SkillLogic 无状态 = 帧同步回滚硬要求（实例不进快照），与程序集无关；运行时状态全在 LSCast 实体

> **规范文档**：新程序集脱离分析器管辖 ≠ 随便写。程序集拓扑、通用铁律（禁 UnityEngine/禁 float/禁实例字段...）、技能模板与 API 表、编译流程、新技能接入清单——全部固化在 **`Packages/cn.etetet.skill/CLAUDE.md`**（包内 CLAUDE.md 会话自动注入）。无状态纪律由 SkillLoader.RegisterAssembly 运行时守门员机器强制。

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

> **状态说明**：2026-08-17 按旧方案（技能放 Hotfix/Skills）写完了第一版全部逻辑代码（未在 Unity 验证）。
> 2026-08-18 用户指出 ET 规则违反（Hotfix 禁属性/字段/普通类）+ 提出独立编译要求 → 架构定案（见上节）。
> 逻辑设计全部保留，按新架构**迁移**（等用户确认后执行）。

| 任务 | 状态 | 日期 | 备注 |
|------|------|------|------|
| LSCast + LSCastComponent + LSSkillComponent（实体） | ✅ | 2026-08-17 | 纯数据实体，留 skill/Scripts/Model/Share（进 ET.Model）；Just\* 标记/TargetIds/SubState/Phase；TotalTimeMs 创建时从 SkillLogic 拷入 |
| LSSkillComponentSystem（Hotfix 侧） | ✅ | 2026-08-18 | LSUpdate（Route B 清标记 + 冷却递减 + 缓冲消费）；TryCast 委托 SkillCastHelper（见下） |
| SkillLogic + SkillContext + SkillLoader + [SkillId] | ✅ | 2026-08-18 | 迁 skill/Runtime/（ET.Skill asmdef）；**SkillContext = readonly struct**（private 字段纯门面 + GetElapsedMs/GetCasterId/GetTargetPosition）；SkillLoader = RegisterAssembly 反射注册 + **守门员** |
| LSCastSystem + LSCastComponentSystem + SkillCastHelper | ✅ | 2026-08-18 | 迁 ET.Skill；EndNow 直写 Cooldowns（不走 Hotfix 扩展避免循环依赖）；**SkillCastHelper.TryCast** 单一实现（Hotfix 门禁入口 + SkillContext.RestartCurrentSkill 连段路径共用，普通静态类无 [EntitySystemOf] 避免生成器冲突） |
| NormalAttack + TestCooldownSkill | ✅ | 2026-08-18 | 迁 skill/DotNet~/Skills/；生命周期签名改 (SkillContext ctx[, int dtMs])——技能不摸 LSCast |
| ET.Skill.asmdef + 热更件套接线 | ✅ | 2026-08-18 | 热更约束同 ET.Model；AssemblyTool.DllNames + AssemblyEditor.DllNames + CodeLoader 三分支加载 + CodeTypes 数组（Model 后 Hotfix 前） |
| ET.SkillContent.csproj + Editor 菜单 + Bundles/SkillContent | ✅ | 2026-08-18 | netstandard2.1 混合 HintPath（ET.Skill←Temp/Bin/Debug，其余←ScriptAssemblies）；菜单 ET/Skill/Compile（检查 F6 前置→dotnet build→拷 .bytes） |
| SkillContentLoader（运行时加载注册） | ✅ | 2026-08-18 | HotfixView，LSSceneChangeStart 接线（room.Init 前，AnimRegistrar 同点）；resLoader 载 bytes → Assembly.Load → RegisterAssembly |
| YooAsset 收集 | ✅ | 2026-08-18 | AssetBundleCollectorSetting 加 SkillContent 组（CollectPath=skill/Bundles/SkillContent，抄 AnimRes 结构；CollectorGUID 为手造 32hex——若 YooAsset 编辑器报 GUID 问题在 UI 里重建该条目即可） |
| 包规范文档 | ✅ | 2026-08-18 | `Packages/cn.etetet.skill/CLAUDE.md`——程序集拓扑/通用铁律/技能模板与 API 表/编译流程/接入清单（新会话自动读） |
| Unity 验证（J 手感不变 + K CD 日志） | ✅ | 2026-08-18 | 全通过：两技能运行时注册（`[Skill] 注册技能 1/2`）；J 膝踢/扣血/连段与 3.5 一致；K 施放→OnEnd(Elapsed=300ms)→CD 拦截(剩余1400ms)→2s 后恢复。**独立编译管线全链路跑通**（F6→ET/Skill/Compile→YooAsset→Assembly.Load→反射注册→守门员） |

**测试机验证清单（2026-08-18 迁移完成后）**：
1. 拉代码，Unity 打开等编译过
2. F6（热更 5 件套 → Temp/Bin/Debug + Bundles/Code 出 ET.Skill.dll.bytes）
3. 菜单 ET/Skill/Compile（应打日志 `[SkillCompile] 完成` + `[Skill] 注册技能 1/2` 在 Play 时）
4. Play：J 膝踢手感与 3.5 一致；K 的 CD 日志；无报错无 hash fail
5. 若 YooAsset 收集报错（SkillContent 组 GUID），在 YooAsset 收集器 UI 里删掉该条重新添加 Packages/cn.etetet.skill/Bundles/SkillContent 即可

**迁移踩坑记录（2026-08-18，测试机首编报错修的）**：
0. **按下沿检测写死 Button==1**：K（Button=2）永远不触发——改成 `Button != 0 && LastButton == 0`（0→非0 都算按下沿，多按键通吃）。
1. **生成代码程序集引用**：`[EntitySystemOf]` 生成的 .g.cs 继承链碰 `IMemoryPackable`（ET.MemoryPack）→ ET.Skill.asmdef 必须引用 ET.MemoryPack（CS0012）。
2. **扩展方法的程序集边界**：ET.Skill 用不了 ET.Hotfix 里的扩展方法（不能反向引用）——`LSAnimComponent.Play`、`LSNumericComponent.Get/Add`、`ClearHitTargets` 全是 Hotfix 扩展（CS1061/CS1929）。修法：**LSNumericComponentSystem 整体挪进 skill/Runtime/**（五层公式不能复制；Hotfix 调用方经 ET.Skill 引用照常可用）；**PlayAnim 内联属性赋值**（LSAnimComponent 全 public 属性；与 LSAnimComponentSystem.Play 同步，改时两处一起）；ClearHitTargets 直摸 HitTargets.Clear()。
3. 经验：往 ET.Skill 加代码时，先查要调的扩展方法定义在哪个程序集——Hotfix 的扩展一律不可用（内联或把该 System 挪进来）。

**实现记录（2026-08-17，逻辑层，迁移后依然成立）**：
1. 攻击状态机搬家：阶段3.5 写在 LSHitboxComponentSystem 的起手/取消/结束逻辑移入 Cast 框架；Hitbox 只剩物理职责（盒采样/碰撞/结算/命中回写 cast）。
2. 攻击盒双路径：帧驱动（判定帧=有 attackBoxes 的帧，NormalAttack 用）+ 固定盒（SkillContext.SetAttackHitbox）；hitbox 只在"动画=Attack1"时重采样帧盒，其他动画不动列表。
3. Route B 标记清理由 LSSkillComponentSystem 开头做（先于 Hitbox/Cast）——修掉"hitbox 先设 cast 后清"的同帧覆灭问题；Finished cast 下帧 Dispose。
4. CD 验证技能 TestCooldownSkill（K 键 CD 2000ms 纯 Log）；NormalAttack CooldownMs=0（DNF 普攻无真 CD）。
5. 取消窗口常量 CancelFrame=3（kneekick 收招帧）；RestartCurrentSkill = 结束当前 + 重施（连段）。
6. 待办：伤害50/硬直500ms 硬编码（Actions 层 + 表化后消除）；Bullet/Area/Buff API 阶段5/6 加。

## 5. 未来演进决策（2026-08-18，记档）

**Actions 效果层（阶段 5 引入）**：参考 `E:\Projects\cs\et7.2skillsystemlession\ET-SkillSystem`（skillSystemLession 分支）的 Actions 模式——
- 技能 = 配置行（actions 列表组合原子节点）；节点 = 无状态 handler 类（`[Actions(type)]` + 反射注册单例，与 SkillLoader 同机制）
- 分工：**过程逻辑（连段/取消/多段）留 SkillLogic 类；时机效果（命中扣血/硬直/击退/加Buff）走 Actions 表组合**——DNF 本来就是 .skl 配置 + nut 脚本双轨
- 节点库放 ET.SkillContent DLL（独立编译管线已支持）；attack/buff 表届时用 luban 或 json（见下）
- ET10 `E:\Projects\cs\etmaster\ET\Packages\cn.etetet.spell`（图形编辑器+多态节点）远期参考其分层，帧同步下不照搬 Unity SerializeReference

**luban 迁移专题（延后，内容规模化时做）**：
- 现状：本项目无 luban——cn.etetet.excel 是 ET 自带简易导出器（EPPlus+Roslyn，表头即 schema）；战斗配置全走手写 json + 注册表（AnimConfigRegistry 模式）
- etmaster 有完整设施：cn.etetet.yiuiluban（Luban.dll，**net10.0**）+ cn.etetet.config（luban.conf 汇总各包 schema + LubanGen.ps1 + 生成基类 [ConfigProcess]/ResolveRef）
- 迁移成本：约半天~一天独立工程（net10 工具适配或源码重编 net8、ET10 基类适配、生成流程接入）；与技能系统解耦
- 决策：**先不迁**。阶段 4-6 用 json 注册表跑通；怪物 AI/装备内容规模化时（约阶段 7 前后）单独做 luban 专题，届时 attack/buff/monster 表 luban 化（json→表平移，注册表读法不变）
