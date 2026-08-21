# DNF 技能实现方案

> 本文档是"从 DNF 源码分析到游戏内实现"的**完整方法论**，以两个技能为实例：
> ① 鬼剑士·浴血之怒（swordman_bloodboom，官方技能）
> ② 波动爆发（releasewave，玩家自制 mod 技能，文件来源 `E:\Projects\cs\dnf-pvf-learn\第四期nut技能波动爆发`）
>
> 新会话必读：§0 系统现状 → §1 文件关联链 → §3 概念映射 → §4/§5 进度与遗留。

---

## 0. 当前技能系统架构现状（新会话必读）

### 0.1 已实现的完整框架

```
┌─── 技能内容层（独立编译管线）──────────────────────────────────────────┐
│   skill/DotNet~/（Unity 永不编译，ET/Skill/Compile 菜单 dotnet build）│
│   ├── Skills/*.cs     技能过程逻辑（SkillLogic 子类，[SkillId] 注册）  │
│   ├── Actions/*.cs    效果节点（LSAction 子类，[ActionId] 注册）       │
│   ├── Buffs/*.cs      Buff 配置（BuffDefinition 子类，[BuffId] 注册）  │
│   ├── Bullets/*.cs    投射物配置（BulletDefinition 子类，[BulletId]）  │
│   └── Areas/*.cs      区域效果配置（AreaDefinition 子类，[AreaId]）    │
│                                                                      │
│   编译产物 ET.SkillContent.dll.bytes → YooAsset → Assembly.Load       │
│   → ContentLoader<TAttr,TBase> 反射注册（守门员：无状态强制）          │
└──────────────────────────────────────────────────────────────────────┘

┌─── 技能框架层（ET.Skill 程序集，热更第 5 件套，Unity/F6 编译）─────────┐
│   skill/Runtime/                                                     │
│   ├── SkillLogic/SkillContext/SkillCastHelper  技能生命周期+门面     │
│   ├── LSAction/LSActionContext                  效果节点基类+门面     │
│   ├── BuffDefinition/BuffLoader                 Buff 配置基类         │
│   ├── BulletDefinition/BulletLoader/LSBulletSystem  投射物            │
│   ├── AreaDefinition/AreaLoader/LSAreaSystem    区域效果              │
│   ├── ContentLoader<TAttr,TBase>                泛型注册器+守门员     │
│   ├── SkillSystemConfig                         全局配置（json 加载） │
│   └── SkillSystemConfigData                     配置数据类            │
└──────────────────────────────────────────────────────────────────────┘

┌─── 逻辑实体层（ET.Model 程序集，skill/Scripts/Model/Share/）───────────┐
│   LSCast/LSCastComponent     施法实例（生命周期状态/Route B 标记）    │
│   LSSkillComponent           技能槽位（冷却/缓冲消费）               │
│   LSHitboxComponent          受击/攻击盒采样（帧驱动/多盒）          │
│   LSCombatComponent          战斗状态（硬直/HurtAnimId/DefaultAnimId）│
│   LSBuff/LSBuffComponent     Buff 实例（Tick/进出检测/叠层）         │
│   LSBullet/LSBulletComponent 投射物（飞行/穿透/碰撞）                │
│   LSArea/LSAreaComponent     区域效果（进出检测/Tick/收尾动画）      │
│   LSInputBufferComponent     输入缓冲（按下沿检测/超时清空）         │
│   LSNumericComponent         数值系统（五层公式）                    │
└──────────────────────────────────────────────────────────────────────┘

┌─── 视图层（lockstep 包）─────────────────────────────────────────────┐
│   LSUnitView + RenderConfig    分层渲染（prefab 21 层 + sortingOrder）│
│   LSSpriteAnimViewComponent    动画帧切换 + 受击闪白 + 加法混合      │
│   LSAnimResComponent           多图集（Atlases/AtlasCenters）        │
│   LSBulletViewComponent        弹视图（渲染自推帧 + prefab 同构）    │
│   LSAreaViewComponent          区域视图（循环/收尾动画切换）         │
│   LSCastViewComponent          表现钩子（Cast/Buff 标记 + HP diff）  │
│   LSUnitViewSystem             朝向翻转（根 GO localScale）         │
└──────────────────────────────────────────────────────────────────────┘
```

### 0.2 五类内容体系

| 内容类型 | 基类 | 标记 | 示例 |
|---|---|---|---|
| 技能 | SkillLogic | [SkillId] | NormalAttack/WaveSword/FireCircle |
| 效果节点 | LSAction | [ActionId] | MeleeHit/FireDamageTick/AddBurnBuff |
| Buff | BuffDefinition | [BuffId] | BurnBuff/StunBuff |
| 投射物 | BulletDefinition | [BulletId] | NormalWaveBullet |
| 区域 | AreaDefinition | [AreaId] | FireCircleArea |

### 0.3 当前按键映射

| 键 | 技能 | 效果 |
|---|---|---|
| J | NormalAttack | 鬼剑士普攻（swordman_attack1 + SetAttackHitbox） |
| K | TestCooldown | 自眩晕 + CD 验证 |
| I | WaveSword | 波动剑投射物（穿透弹） |
| O | FireCircle | 火圈区域效果（持续燃烧） |
| U | BloodBoom | 浴血之怒（自耗 5% HP 的自身中心血爆 + .als 施法特效叠加） |

### 0.4 未实现/延后的 DNF 特性

| 特性 | 状态 | 说明 | 实现条件 |
|---|---|---|---|
| **.als 边车解析** | ✅ 已做（2026-08-21） | 声明式特效叠加（§1.2）：翻译工具 als 子命令 + AnimOverlayConfig + LSAnimOverlayViewComponent | — |
| **.atk 命中反应** | ✅ 已做（2026-08-21） | HitReaction 虚属性（§1.3 方案 C）：SkillLogic/Area/Bullet + MeleeHit 读 ctx.GetSourceHitReaction() | — |
| **霸体帧** | ⏸ 延后 | DAMAGE TYPE=SUPERARMOR，需帧级受击检查 | AnimFrameData 加字段 |
| **屏震/闪屏** | ⏸ 延后 | 开关已有（SkillSystemConfig），效果未实现 | 视图 camera |
| **音效** | ⏸ 延后 | 无音频系统 | 接入音频 |
| **多段命中** | ⏸ 延后 | HitTargets 定时清空（DNF resetHitObjectList） | Area/Bullet 加字段 |
| **浮空/击退** | ⏸ 延后 | 需要 Z 轴物理（位移+落地） | 新系统 |
| **[RGBA] 帧染色** | ⏸ 延后 | 隐形帧/闪白/淡入淡出 | 视图 color |
| **[IMAGE RATE] 缩放** | ⏸ 延后 | 帧缩放/镜像 | 视图 scale |
| **luban 迁移** | ⏸ 延后 | 配置表化（Excel→json→运行时） | 工具链适配 |
| **Area 火圈视觉** | ✅ 已做 | firecircle.json + AT_Up.img | — |

### 0.5 架构关键决策记录

| 决策 | 选择 | 原因 |
|---|---|---|
| 技能内容编译 | DotNet~ csproj 独立 dotnet build | 改数值不触发 Unity 编译；手动菜单（ET/Skill/Compile） |
| ET.Skill 程序集 | 热更第 5 件套（F6 编译） | 引用 ET.Model → 必须热更侧（HybridCLR AOT 不能引用热更） |
| 命中反应参数 | 方案 C（HitReaction 虚属性，static readonly 零 GC） | 每技能独立参数（DNF 同构）；C# 属性是 luban 前置形态；SkillLogic 非 ECS 实体不受分析器管 |
| 内容层引用面 | 只见 SkillContext/LSActionContext 门面 | ET.SkillContent csproj 只引 ET.Skill/TrueSync/Npkparser/Core |
| 受击动画分流 | LSCombatComponent.HurtAnimId | DNF sq_GetDamageAni 同构（每角色自带）；有动画播动画无动画红闪 |
| 朝向翻转 | LSUnitViewSystem 翻根 GO localScale | 所有子层一体镜像；LSSpriteAnimView 不重复处理 |
| 摆位公式 | §2.1 绝对公式 + prefab 中间层运行时自标定 | 消除常数偏移；任意画布尺寸资源自动落对 |
| 视图检测三模式 | Just* 标记/视图侧缓存 diff/逻辑直驱 | LastHitstunTimer 边沿不可用（同一 Unity Update 里逻辑先覆盖） |

---

## 1. DNF 技能文件关联链（核心知识）

一个 DNF 技能由**六层文件**组成，通过 `.chr` 文件和 nut 脚本串联：

```
┌─── ① .skl ────────────────────────────────────────────────────────────┐
│   skill/<职业>/<技能名>.skl                                             │
│   内容：名称/等级/CD/MP消耗/指令/等级属性（伤害%/大小比例/出血参数）      │
│   → 纯数据，无逻辑                                                     │
└────────────────────────────────────────────────────────────────────────┘
         ↓ 被注册到
┌─── ② swordman_load_state.nut ─────────────────────────────────────────┐
│   sqr/character/swordman/swordman_load_state.nut                       │
│   IRDSQRCharacter.pushState(..., "bloodboom.nut", "bloodboom", 229)   │
│   → 状态机注册：技能ID → nut 脚本路径                                   │
└────────────────────────────────────────────────────────────────────────┘
         ↓ 触发执行
┌─── ③ 技能逻辑 nut ────────────────────────────────────────────────────┐
│   sqr/character/swordman/<技能名>/<技能名>.nut                         │
│   核心回调：                                                           │
│   onSetState        → 施法瞬间（扣HP/播动画/播音效）                    │
│   onKeyFrameFlag    → .ani 的 SET FLAG 帧触发（创建爆炸实体）           │
│   onEndCurrentAni   → 动画播完（回待机）                                │
│   onProc            → 每帧逻辑                                         │
│                                                                       │
│   关键 API：                                                           │
│   sq_SetCurrentAnimation(ID)  → 用 ID 查 .chr → 找到 .ani 播放        │
│   sq_SetCurrentAttackInfo(ID) → 用 ID 查 .chr → 找到 .atk 使用         │
│   sq_SendCreatePassiveObjectPacket(ID, ...) → 创建攻击判定体            │
│   sq_GetLevelData(skillID, index, level) → 读 .skl 等级数据            │
└────────────────────────────────────────────────────────────────────────┘
         │                    │                        │
         ↓                    ↓                        ↓
┌─── ④ .chr 文件 ───┐  ┌─── ⑤ .atk 文件 ──┐  ┌─── ⑥ passiveobject ─┐
│ character/swordman/│  │ attackinfo/*.atk  │  │ 独立攻击判定体        │
│ swordman.chr       │  │ 命中反应规则：     │  │ 有自己的 nut 脚本     │
│ 动画注册表+攻击注册 │  │ 击退/浮空/倒地/   │  │ 有自己的 .ani 特效    │
│ ID → 文件路径映射  │  │ 硬直时长/异常注入  │  │ 攻击盒在这里创建      │
│                   │  │                   │  │ (sq_AddAttackBox)    │
│ Animation/xxx.ani │  │                   │  │                     │
│   = 124           │  │                   │  │ 目录：               │
│ AttackInfo/xxx.atk│  │                   │  │ passiveobject/       │
│   = 87            │  │                   │  │   character/swordman/│
└───────────────────┘  └───────────────────┘  │   animation/<技能名>/ │
                                               │   action/<技能名>.act│
                                               └─────────────────────┘
```

### 1.1 资源分层

```
character/swordman/
├── animation/xxx.ani                    ← 角色本体动作（帧数据+damageBox+SET FLAG）
├── animation/xxx.ani.als               ← 边车：声明式特效叠加（§1.2）
├── attackinfo/xxx.atk                   ← 命中反应参数（击退力/浮空力/硬直时长）
├── effect/animation/<技能名>/xxx.ani     ← 视觉特效动画（发光/冲击波/爆炸）
└── swordman.chr                         ← 注册表（ID → .ani/.atk 路径映射）

equipment/character/swordman/avatar/
├── belt/belt_a/xxx.ani                  ← 同一动作的装备图层变体（每层各一份）
├── jacket/jacket_a/xxx.ani
└── ...

passiveobject/character/swordman/
├── animation/<技能名>/xxx.ani            ← 攻击判定体的特效动画
├── animation/<技能名>/xxx.ani.als       ← 特效的特效叠加
└── action/<技能名>.act                   ← 攻击判定体的行为定义

sqr/character/swordman/<技能名>/
├── <技能名>.nut                         ← 技能主逻辑
└── ap_<技能名>.nut                      ← 攻击判定体逻辑
```

### 1.2 .als 边车机制（2026-08-20 实证）

**是什么**：声明式特效叠加表——引擎加载 .ani 时自动读同名 .als，在指定帧/层叠加特效动画，不改 .ani 一个字节。

**bloodboom.ani.als 实际内容**：
```
[use animation]                          ← 注册特效动画（路径+别名）
	`../Effect/Animation/Bloodboom/casting_bloodboom_circle2.ani`
	`casting_bloodboom_circle2`

[add]                                    ← 挂接：(帧号, 层号) + 别名
	3	-2	`casting_bloodboom_casting_back`    ← 帧3、层-2
	-1	-1	`casting_bloodboom_boomback`        ← 全帧、层-1
	3	10001	`casting_bloodboom_casting`       ← 帧3、标记10001
	-1	10002	`casting_bloodboom_boomfront`    ← 全帧
	-1	10003	`casting_bloodboom_circle1`
	-1	10004	`casting_bloodboom_circle2`
```

**两个整数**：`(帧号, 层号/标记)`——帧号 -1 = 全帧生效；层号控制渲染 z 序。

**作用**：播放 bloodboom.ani 时自动叠加 6 个特效——零代码，纯声明式。

**我们的实现方案**（~200 行，三层）：

##### ① 翻译层：DnfConfigTranslation 加 `als` 子命令

**输入**：`.als` 文本文件（与 .ani 同一分节语法）
**输出**：`overlay.json`（叠加配置列表）

```
.als 原始格式：
[use animation]
	`../Effect/Animation/Bloodboom/casting_bloodboom_circle2.ani`
	`casting_bloodboom_circle2`
[add]
	3	-2	`casting_bloodboom_casting_back`
	-1	10001	`casting_bloodboom_casting`
```

```
输出 overlay.json：
{
  "overlays": [
    { "startFrame": 3,  "z": -2,   "effectAni": "casting_bloodboom_casting_back" },
    { "startFrame": -1, "z": -1,   "effectAni": "casting_bloodboom_boomback" },
    { "startFrame": 3,  "z": 10001, "effectAni": "casting_bloodboom_casting" },
    { "startFrame": -1, "z": 10002, "effectAni": "casting_bloodboom_boomfront" }
  ]
}
```

**新增代码（DnfConfigTranslation 项目）**：

| 文件 | 内容 | 行数 |
|---|---|---|
| `Als/AlsModels.cs` | AlsOverlay 数据类（StartFrame/Z/EffectAni 别名）+ AlsDocument | ~25 行 |
| `Als/AlsParser.cs` | 解析 `[use animation]`（注册别名→路径）+ `[add]`（帧/层/别名）| ~50 行 |
| `Als/AlsJsonWriter.cs` | Utf8JsonWriter 输出 overlay.json | ~30 行 |
| `Program.cs` 加 `case "als"` | 子命令分派 | ~3 行 |
| `Als/README.md` | 翻译规则文档 | — |
| **合计** | | **~110 行** |

**解析要点**：
- `[use animation]` 两行值 = (ani 路径, 别名)——存别名→路径映射
- `[add]` 三列值 = (帧号, 层号, 别名)——帧号 -1 = 全帧
- 层号含义：负数 = 渲染 z 序（背后）；正数大值 = SET FLAG 标记号段（DNF 语义混合，先用原值直译，游戏侧按需解释）
- 特效 .ani 的路径是**相对路径**（相对于 .als 所在目录），需转绝对/注册名

**CLI 用法**：
```bash
dotnet run --project DnfConfigTranslation -- als <.als文件> [输出.json]
```

##### ② 数据层：AnimOverlayData + Registry

```csharp
// npkparser/Runtime/（游戏侧数据结构，JsonUtility 反序列化）
[Serializable]
public class AnimOverlayConfig
{
    public AnimOverlayEntry[] overlays;
}

[Serializable]
public class AnimOverlayEntry
{
    public int startFrame;      // -1 = 全帧
    public int z;               // 渲染层号
    public string effectAni;    // 特效动画注册名（→ AnimId 查找）
}

// AnimConfigRegistry 加注册/查询
public static Dictionary<int, AnimOverlayConfig> OverlayConfigs = new();
public static void RegisterOverlay(int animId, AnimOverlayConfig config) { ... }
public static AnimOverlayConfig GetOverlay(int animId) { ... }
```

##### ③ 视图层：LSAnimOverlayViewComponent

```
挂 LSUnitView 下（IUpdate）
→ 监听 AnimId 变化
→ 有 overlay 配置 = 动态创建子 GO + SpriteRenderer（sortingOrder = 装备层段 + z）
→ 每个特效独立帧推进（渲染 deltaTime，同弹视图 AdvanceFrame 模式）
→ startFrame 到达后开始显示；父动画切走/播完 = 销毁全部 overlay GO
```

### 1.3 .atk 命中反应机制

**是什么**：每个技能一份命中反应参数文件——被击者被打后怎么反应。

**典型内容**：
```
[damage bonus]     -15          ← 伤害加成
[attack type]      physic       ← 物理/魔法
[push aside]       30           ← 击退力
[lift up]          75           ← 浮空力
[attack direction] hit down     ← 击退方向
[damage reaction]  damage       ← 反应类型
```

**我们的实现**（方案 C，DNF 同构）：

```csharp
// 参数类（ET.Skill Runtime/，[EnableClass]，非实体不受分析器管）
[EnableClass]
public class HitReaction
{
    public int Damage;
    public int HitstunMs;
    public int KnockbackX;
    public int LaunchY;
}

// SkillLogic 加虚属性（static readonly 预分配，零 GC）
public abstract class SkillLogic
{
    private static readonly HitReaction Default = new() { Damage = 50, HitstunMs = 500 };
    public virtual HitReaction HitReaction => Default;
}

// 每个技能 override 自己的参数（同样 static readonly，零 GC）
public class BloodBoomSkill : SkillLogic
{
    private static readonly HitReaction BloodBoomReaction = new() { Damage = 300, HitstunMs = 1000 };
    public override HitReaction HitReaction => BloodBoomReaction;
}

// MeleeHitAction 从 ctx 读（不再硬编码）
ctx.GetSourceHitReaction().Damage
```

**为什么 ECS 下合法**：SkillLogic 不是 LSEntity/Component/System——是 ET.Skill 程序集里的普通 C# 抽象类，不在分析器管辖名单（只查 ET.Core/Model/Hotfix/ModelView/HotfixView），虚属性+字段完全合法。HitReaction 是纯配置数据，命中瞬间查表即用，不进快照、不参与回滚。

**设计决策**：
- **零 GC**：`static readonly` 预分配共享实例（无状态只读，帧同步安全）
- **luban 前置形态**：C# 虚属性 → 以后迁 luban 只改 getter 为表查询，其余零改动
- **Area/Bullet 同构**：AreaDefinition/BulletDefinition 也加 HitReaction 虚属性（它们的 HitActions 也用 MeleeHit）

---

## 2. 查找分析提取流程

实现一个 DNF 技能的完整步骤：

### Step 1：找到技能文件

```bash
# 1.1 找 .skl（技能数据）
ls pvf/skill/swordman/ | grep <关键词>

# 1.2 找 .ani（角色动作）
ls pvf/character/swordman/animation/ | grep <关键词>

# 1.3 找 nut 脚本（技能逻辑）
ls pvf/sqr/character/swordman/<技能名>/

# 1.4 找被动对象（攻击判定体）
ls pvf/passiveobject/character/swordman/animation/<技能名>/
ls pvf/passiveobject/character/swordman/action/<技能名>.act

# 1.5 找攻击信息（命中反应）
ls pvf/character/swordman/attackinfo/ | grep <关键词>

# 1.6 找 .als（特效叠加边车）
ls pvf/character/swordman/animation/<技能名>.ani.als
```

### Step 2：分析技能数据

```bash
# .skl 关键字段
grep -A2 "name\`\|explain\`\|command\|cool time\|level property" skill/swordman/<技能名>.skl

# .ani 关键数据
grep -c "FRAME" <技能名>.ani        # 帧数
grep -c "ATTACK BOX" <技能名>.ani   # 攻击盒数量（玩家技能通常=0）
grep -c "DAMAGE BOX" <技能名>.ani   # 受击盒数量
grep "SET FLAG" <技能名>.ani        # 攻击触发帧标记

# nut 脚本核心逻辑
cat sqr/character/swordman/<技能名>/<技能名>.nut | head -80

# .atk 命中反应参数
cat pvf/character/swordman/attackinfo/<技能名>.atk
```

### Step 3：确定攻击触发时机

**SET FLAG 是关键**——.ani 里 `[SET FLAG] N` 表示该帧触发 nut 的 `onKeyFrameFlag(obj, N)`。

```bash
# 找 SET FLAG 在哪一帧
grep -B30 "SET FLAG" <技能名>.ani | grep "^\[FRAME" | tail -1
```

### Step 4：确定需要提取的资源

```bash
# 找特效 ani 引用的所有 img
grep -h "\.img\|\.IMG" passiveobject/character/swordman/animation/<技能名>/*.ani \
  | sed 's/.*`\([^`]*\)`.*/\1/' | sort -u

# .als 里引用的特效 ani
grep "use animation" -A1 <技能名>.ani.als | grep "\.ani"

# NPK 名按路径规则：sprite_<路径下划线化>.NPK
```

**认版本**：v2（ARGB）✓ / v4（索引色）✓ / v5（DDS）✗

### Step 5：翻译资源

```bash
# .ani → .json（DnfConfigTranslation）
dotnet run --project DnfConfigTranslation -- ani <.ani路径> <输出.json>

# .als → overlay.json（DnfConfigTranslation，待实现 als 子命令）
dotnet run --project DnfConfigTranslation -- als <.als路径> <输出.json>

# .img → .bytes（直接复制改名）
cp <提取的.img> Bundles/AnimRes/<名称>.img.bytes
```

### Step 6：注册到游戏

```csharp
// AnimId 加常量（npkparser/Runtime/AnimConfigRegistry.cs）
public const int BloodBoomCast = 20;       // 施法动画
public const int BloodBoomExplosion = 21;  // 爆炸特效

// LSAnimClipRegistrar 注册 json
await RegisterOne(resLoader, ".../bloodboom_cast.json", AnimId.BloodBoomCast);

// LSAnimResComponentSystem 加载图集
await BuildAtlas(self, resLoader, ".../BLOODBOOM_BOOMFRONT.img.bytes");
```

### Step 7：实现技能

```csharp
// DotNet~/Skills/BloodBoomSkill.cs
[SkillId(SkillIds.BloodBoom)]
public class BloodBoomSkill : SkillLogic
{
    public override int CooldownMs => 5000;
    public override int TotalTimeMs => 910;
    public override HitReaction HitReaction => new() { Damage = 300, HitstunMs = 1000 };

    public override void OnCast(SkillContext ctx)
    {
        ctx.PlayAnim(AnimId.SwordmanBloodboom);
        ctx.ClearHitTargets();
        ctx.AddNumericToSelf(NumericType.Hp, -HP消耗量);  // HP 自耗
    }

    public override void OnUpdate(SkillContext ctx, int dtMs)
    {
        // SET FLAG 帧 = 爆炸触发帧
        if (ctx.CurrentFrameIndex() >= 22 && ctx.GetSubState() == 0)
        {
            ctx.CreateAreaInFront(AreaIds.BloodBoom, 距离);
            ctx.SetSubState(1);  // 标记已触发
        }
    }
}
```

---

## 3. DNF → 我们系统概念映射表

| DNF 概念 | DNF 文件 | 我们的实现 | 状态 |
|---|---|---|---|
| 技能数据 | .skl（CD/等级/伤害%） | SkillLogic 子类属性 | ✅ |
| 技能逻辑 | nut 脚本（onSetState 等） | OnCast/OnUpdate/OnEnd | ✅ |
| 施法动画 | .ani（角色动作） | AnimId → 注册的 json | ✅ |
| 动画帧数据 | .ani 的 FRAME 节 | AnimFrameData | ✅ |
| 攻击触发帧 | .ani 的 SET FLAG | OnUpdate 检查 CurrentFrameIndex()（帧号 const 进技能类） | ✅ |
| 攻击判定体 | passiveobject | LSArea / LSBullet | ✅ |
| **命中反应** | .atk（击退/浮空/硬直） | **HitReaction 虚属性**（方案 C，SkillLogic/Area/Bullet 三处 + GetSourceHitReaction） | ✅ |
| 攻击盒 | nut 的 sq_AddAttackBox | SetAttackHitbox() / Area HalfExtents | ✅ |
| 出血/中毒 | nut 写入异常参数 | BuffDefinition + TickAction | ✅ |
| **特效叠加** | **.als 边车** | **AnimOverlayConfig + LSAnimOverlayViewComponent** | ✅ |
| **施放门槛（HP）** | checkExecutableSkill + static[0] | SkillLogic.MinCastHpPct（TryCast 前拒绝不进 CD） | ✅ |
| **技能子状态** | setSkillSubState | LSCast.SubState + ctx.GetSubState/SetSubState | ✅ |
| **霸体帧** | .ani DAMAGE TYPE | ⏸ 延后 | ⏸ |
| 视觉特效 | effect/animation/*.ani | ViewAnimId 视图自推帧 | ✅ |
| **屏震/闪屏** | nut sq_SetMyShake | ⏸ 延后（开关已有） | ⏸ |
| 装备图层 | equipment/avatar 每层 .ani | RenderConfig.Layers | ✅ |
| **音效** | nut sq_PlaySound | ⏸ 延后 | ⏸ |
| **[RGBA] 染色** | .ani 帧级 | ⏸ 延后 | ⏸ |
| **[IMAGE RATE]** | .ani 帧级缩放 | ⏸ 延后 | ⏸ |

---

## 4. 实例一：鬼剑士·浴血之怒（swordman_bloodboom）

### 4.1 DNF 原始数据

| 项 | 值 |
|---|---|
| 名称 | 浴血之怒 |
| 等级 | 75（二次觉醒大招） |
| 指令 | →←→ + Z |
| CD | 40 秒（demo 缩短 5 秒） |
| MP | 580~4500 |
| HP 消耗 | 自身 HP %（等级数据 [consume HP]） |
| 动画 | bloodboom.ani 23 帧（帧 0-19: 35ms, 帧 20-22: 70ms）≈ 910ms |
| SET FLAG | 帧 22 = **1**（爆炸触发） |
| 霸体 | 帧 20-22（DAMAGE TYPE=SUPERARMOR） |
| attackBox | **无**（由 passiveobject nut 动态创建） |
| damageBox | 23 帧全有 ✓ |
| .als | **有**（6 个特效叠加：施法/爆炸/圆环，见 §1.2） |
| .atk | passiveobject/character/swordman/action/bloodboom.act |

### 4.2 nut 脚本分析

```java
// onSetState（施法瞬间）：
扣自身 HP = hpMax × 等级消耗%
播动画 sq_SetCurrentAnimation(122)  // bloodboom.ani
播音效 SM_BLOODBOOM_01

// onKeyFrameFlag(obj, 1)（帧 22 SET FLAG=1 触发）：
创建爆炸实体 24370（ap_bloodboom.nut）
写入参数：技能ID/攻击范围/伤害倍率/出血参数
屏震 + 闪屏
播特效动画 finish_bloodboom_finish_floorblood.ani
播音效 SM_BLOODBOOM_02

// onEndCurrentAni（动画播完）：
回待机 STATE_STAND
```

### 4.3 爆炸实体（passiveobject）

```
passiveobject/character/swordman/
├── animation/bloodboom/
│   ├── boom1_bloodboom_boomfront.ani    ← 正面爆炸 11 帧 × 60ms
│   ├── boom1_bloodboom_boomback.ani     ← 背面爆炸
│   ├── boom1_bloodboom_casting.ani      ← 施法蓄力 11 帧
│   ├── finish_bloodboom_shockwave.ani   ← 收尾冲击波 7 帧
│   └── ...（15+ 个特效 + 5 个 .als）
├── action/bloodboom.act
└── sqr/.../ap_bloodboom.nut
```

### 4.4 需要的 img 资源

| img | NPK | 用途 | 状态 |
|---|---|---|---|
| BLOODBOOM_BOOMFRONT.IMG | sprite_character_swordman_effect_bloodboom.NPK | 正面爆炸 | 待提取 |
| BLOODBOOM_SHOCK.IMG | 同上 | 冲击圈 | 待提取 |
| BLOODBOOM_CASTING.IMG | 同上 | 施法蓄力 | 可选 |
| BLOODBOOM_CASTING_BACK.IMG | 同上 | 施法背面 | 可选 |
| BLOODBOOM_FINISH*.IMG | 同上 | 收尾 | 可选 |

### 4.5 实现进度

| 任务 | 状态 | 日期 | 备注 |
|---|---|---|---|
| PVF 源码分析 | ✅ | 2026-08-20 | 完整链路（.skl/.nut/.ani/.als/.act/passiveobject） |
| 角色施法动画翻译 | ✅ | 2026-08-19 | swordman_bloodboom.json 已入库（23 帧 980ms） |
| .als 分析 | ✅ | 2026-08-20 | 6 个特效叠加（§1.2；其中 4 个为空占位动画，实译 2 个） |
| HitReaction 方案 C | ✅ 定案 | 2026-08-20 | C# 虚属性，luban 前置形态 |
| HitReaction 落地 | ✅ | 2026-08-21 | SkillLogic/Area/Bullet 三虚属性 + LSActionContext.GetSourceHitReaction + 4 构造点传参 + MeleeHit 改读 |
| .als 翻译工具 | ✅ | 2026-08-21 | DnfConfigTranslation als 子命令（AlsModels/AlsParser/AlsJsonWriter + Program.cs 分派 + README） |
| AnimOverlayViewComponent | ✅ | 2026-08-21 | 视图层特效叠加（lockstep ModelView/HotfixView，新 GO 挂单位根下，层号直译 sortingOrder） |
| 爆炸特效 .ani 翻译 | ✅ | 2026-08-21 | casting/casting_back/boomfront/boomback 4 个 json 入库（帧数 20/16/10/8） |
| img 提取 | ✅ | 2026-08-21 | 用户从 NPK 提取（8 张全 v2 ARGB）；入库 4 张 .img.bytes |
| BloodBoomSkill 技能类 | ✅ | 2026-08-21 | MinCastHpPct 10% + 扣 5% 上限 HP + 帧22 SubState 触发 + TotalTimeMs 980 + CD 5s（demo） |
| BloodBoomArea 爆炸区域 | ✅ | 2026-08-21 | 8×3×3 单位，EnterActions=MeleeHit(300)+AddBleedBuff，ViewAnimId+ViewBackAnimId 双层爆炸 |
| BleedBuff 出血 Buff | ⬜→✅ | 2026-08-21 | 3 秒每秒 15（BleedDamageTick；与 Burn 同构） |
| U 键映射 | ✅ | 2026-08-21 | LSOperaComponentSystem U→button5 + SkillIds.BloodBoom=5 |
| 验证 | ⬜ | | 待测试机（拉代码 → F6 → ET/Skill/Compile → Play，按 U） |

### 4.6 遗留/未处理

| 问题 | 状态 | 说明 |
|---|---|---|
| 霸体帧（SUPERARMOR） | ⏸ 延后 | AnimFrameData 加 damageType 字段 + 受击检查逻辑 |
| 屏震+闪屏 | ⏸ 延后 | SkillSystemConfig 开关已有，效果未实现 |
| ~~出血 Buff~~ | ✅ 2026-08-21 | BleedBuff : BuffDefinition（3 秒每秒 15，与 Burn 同构） |
| 4 段 vs 8 段多段 | ⏸ 延后 | demo 先做单次大伤害（BloodBoomArea EnterActions 单次） |
| 音效 | ⏸ 延后 | 音频系统未接入（SM_BLOODBOOM_01/02 已考证） |
| 大小比例缩放 | ⏸ 延后 | 等级→Area HalfExtents 缩放，demo 固定值 |
| 施法侧 4 个空占位动画 | ⏭ 跳过 | bloodboom.ani.als 的 boomfront/boomback/circle1/circle2 是纯时间轴占位（本客户端版本无贴图），overlay 注册时别名不映射即跳过 |
| shock/finish 系列特效 | ⏸ 延后 | 依赖 [RGBA] 染色/[IMAGE RATE] 缩放/[IMAGE ROTATE] 旋转（均延后，§0.4）；finish3 有攻击盒数据可后补 |
| 血气旺盛(63)增伤联动 | ⏸ 延后 | ap_bloodboom appendage：对出血敌人攻击力增加——需 Buff 查询门面 + 增伤公式位 |
| 帝血弑天已损 HP 增伤 | ⏸ 延后 | nut：(hpMax-hp)/1% × level 列7 增伤向量——demo 固定伤害 |
| 施放时停止移动 | ⏸ 延后 | DNF sq_StopMove；现有技能（波动剑等）也未禁移动，统一在攻击状态机做 |
| Buff 末跳被到期吞掉 | 🐛 既有系统性 | LSBuffComponentSystem.LSUpdate 到期检查在 Tick 之前并 continue → 3s/1s 配置实际只 tick 2 次（燃烧/出血均如此）。要修把 Tick 段移到到期判断前即可，但会改变既有 Burn/Stun 结算节奏——待定夺 |

#### 4.7 实现落地记录（2026-08-21，as-built）

与 §1.2/§1.3 方案的差异与补充决策：

1. **AnimOverlayEntry 加了 `effectAnimId` 字段**（[NonSerialized]，注册时由 LSAnimClipRegistrar 用 alias→AnimId 字典解析）——翻译工具不知道 AnimId 分配，别名解析放注册侧。
2. **overlay GO 是 `new GameObject` + SpriteRenderer 直接挂单位根 GO 下**（不是 prefab 子层）——跟随位置 + 朝向镜像免费获得，且 sortingOrder 不受 prefab 21 层限制；**层号直译 sortingOrder**（-2→-2、10001→10001，负=身后低于身体层动态值≈0，10001+ 高于一切动态深度）。
3. **SkillLogic 加了 `MinCastHpPct`（FP，默认 0）**：DNF checkExecutableSkill 的 HP 门槛数据驱动化，SkillCastHelper.TryCast 在 CD 检查前拒绝（不进 CD 不建 cast）。
4. **SkillContext 补了 4 个门面**：GetSubState/SetSubState（LSCast.SubState 一直存在但没暴露）、GetCasterHp/GetCasterMaxHp/ConsumeCasterHp（自耗 HP 技能用）。
5. **AreaDefinition 加了 `ViewBackAnimId`**：爆炸前后两层（boomfront 主层 sortingOrder 5 / boomback 背层 4），LSAreaViewComponentSystem 重构为 AdvanceOne 单层函数复用；顺手修了 Update 里 foreach 中 RemoveView 改字典的隐患（收集后统一删）。
6. **伤害/出血参数在 BloodBoomArea.HitReaction（300/1000ms）**，不在技能上——DNF 里伤害方就是被动对象 24370（角色 .atk 无 bloodboom 条目已考证）。
7. **触发帧用 const 帧号**（BoomFrame=22 + SubState 一次性守卫），SET FLAG 不进翻译链路（方案 §3 既有映射：OnUpdate 检查 CurrentFrameIndex）。

---

## 5. 实例二：波动爆发（releasewave，玩家 mod 技能）

> 来源：`E:\Projects\cs\dnf-pvf-learn\第四期nut技能波动爆发`

### 5.1 mod 文件结构

```
第四期nut技能波动爆发/
├── skill/swordman/releasewave.skl          ← 技能数据
├── sqr/character/swordman/
│   └── releasewave/releasewave.nut         ← 技能逻辑
├── character/swordman/
│   ├── animation/releasewavedash_body.ani  ← 角色动作
│   ├── animation/releasewavedash_body.ani.als ← 特效边车
│   ├── attackinfo/releasewave_light.atk    ← 命中反应
│   └── effect/animation/releasewave/       ← 视觉特效
├── passiveobject/unclebang_shared_passive_object/
│   ├── animation/...
│   └── sqr/.../po_swordman_shared.nut
├── equipment/character/swordman/avatar/
│   └── belt/belt_a~h/releasewavedash_body.ani  ← 8 套装备图层
├── 波动爆发.txt                            ← 安装教程（文件关联说明）
└── !  波动爆发重做.NPK                      ← 客户端资源包
```

### 5.2 nut 脚本分析

```java
// onSetState：
播动画 sq_SetCurrentAnimation(124)  // releasewavedash_body.ani
设攻击信息 sq_SetCurrentAttackInfo(87)  // releasewave_light.atk
播特效 locakonhit("releasewaveibackwind.ani", ...)

// onKeyFrameFlag(obj, 10001)：
创建攻击实体 24389

// onEndCurrentAni：
回待机
```

### 5.3 mod 与官方的差异

| 差异点 | 官方 | mod |
|---|---|---|
| SET FLAG | 通常 = 1 | **10001**（mod 自定义号段） |
| 被动对象 ID | 官方分配 | **24389**（自定义，需手动注册） |
| 特效目录 | effect/animation/<技能名>/ | 同（但子目录结构可能不同） |

### 5.4 实现进度

| 任务 | 状态 | 日期 | 备注 |
|---|---|---|---|
| mod 文件分析 | ✅ | 2026-08-20 | 本文 §5 |
| 角色动画翻译 | ⬜ | | releasewavedash_body.ani |
| 特效翻译 | ⬜ | | 多个子目录 |
| 技能类实现 | ⬜ | | |

### 5.5 遗留/未处理

（实现后记录）

---

## 6. 新会话入门指南

### 6.1 必读顺序

```
1. §0 系统现状 → 了解已有什么、缺什么（2 分钟）
2. §1 文件关联链 → 理解 DNF 六层结构（3 分钟）
3. §3 概念映射表 → DNF 概念 → 我们的实现（1 分钟）
4. §4.5 或 §5.4 进度表 → 知道做到哪了（30 秒）
5. §4.6 或 §5.5 遗留清单 → 知道什么没做（30 秒）
6. Packages/cn.etetet.skill/CLAUDE.md → 编写规范
```

### 6.2 关键上下文

| 信息 | 值 |
|---|---|
| PVF 源码位置 | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf` |
| mod 技能源码 | `E:\Projects\cs\dnf-pvf-learn\` |
| 翻译工具 | `E:\Projects\cs\parse-img-ani\DnfConfigTranslation`（dotnet run -- ani <输入> <输出>） |
| 游戏工程 | `E:\Projects\cs\et9lockStepYIUITest` |
| 技能编写规范 | `Packages/cn.etetet.skill/CLAUDE.md` |
| 技能系统架构 | 本文档 §0 |
| 测试机 | 用户手动（拉代码 → F6 → ET/Skill/Compile → Play） |
| 开发机 | 无 Unity（写代码 → 提交 → 测试机拉取验证） |
| 认 img 版本 | v2 ✓ / v4 ✓ / v5 DDS ✗ |
| NPK 命名规则 | `sprite_<路径下划线化>.NPK` |

### 6.3 开工步骤

```
1. 确认进度：读 §4.5/§5.4 → 找到第一个 ⬜ 的任务
2. 按 §2 流程操作（查找 → 分析 → 提取 → 翻译 → 注册 → 实现）
3. 按 skill/CLAUDE.md 规范写代码
4. 更新 §4.5/§5.4 进度表 + §4.6/§5.5 遗留清单
5. 告知用户测试机验证步骤
```
