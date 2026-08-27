# 鬼影三击剑（TripleStab）

> 技能ID 112 | 级别 A（维持预判） | 可实现性 ✅（直接；"鬼影步状态门禁"与暗属性元素简化掉） | 分析日期 2026-08-22 | 批次 A11

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼影三击剑 | `skill\Swordman\TripleStab.skl [name]` |
| 英文名 | TripleStab（取 skl 文件名；[name2] 实测为英文 `Apparition Shadow Three Sword`） | 同上 [name2] |
| 职业 | 鬼泣（[skill fitness growtype]=2，L17；growtype maximum 仅第 3 位 50） | 同上 |
| 学习��级 | 35（[required level range] 3） | 同上 [required level] |
| 最高等级 | 70 | 同上 [maximum level] |
| 类型 | active（skill class 3）/ 魔法·暗属性（.atk [elemental property]=dark element） | 同上 [type] / .atk 实测 |
| 指令 | ↓↑→ + Z | 同上 [command] / [command key explain] |
| CD | 8000 ms（dungeon 固定） | 同上 [cool time] |
| MP | 60 → 924（Lv1 → Lv70） | 同上 [consume MP] |
| 特殊消耗 | 只能在[鬼影步]已施放的状态下使用（引擎门禁） | 同上 [explain] |
| 前置 | 技能 111（鬼影鞭 GhostSideWind）Lv1 | 同上 [pre required skill]（111 号 skl [name] 实测=鬼影鞭） |
| static data | `2000`（单值；语义未考证——参照同族位移参数惯例推断为三连刺总位移/速度相关 px 量级） | 同上 [static data] |
| 一句话效果 | 迅速向下、上、中三个方向发出刺击并击退敌人（三段连刺，末段大击退） | 同上 [explain] |

**level info（2 列，Lv1 → Lv70 首末值）+ 模板解码**（L21 法，3 向量 ↔ 3 占位符）：

| 占位符 | 向量 | 语义 | Lv1 值 |
|---|---|---|---|
| 刺击下段攻击力 | (-1,0,1.0) | level col0 | 146% → 678%…（Lv70=约 3167） |
| 刺击上段攻击力 | (-1,0,1.0) | **同 col0**（与下段同列） | 同上 |
| 刺击中段攻击力 | (-1,1,1.0) | level col1 | 678%（Lv1） |

即下段/上段共用 col0，中段（末段）独立 col1——与"末段大击退"的 finish atk 分工吻合。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无注册行**（grep `triplestab` 无命中；技能 112 作第 5 参亦无）——F3 引擎内置技能。三方印证：

1. **常量表 → .chr 槽位**：`swordman_header.nut:291` `CUSTOM_ANI_TRIPLESTAB <- 121`；.chr `[etc motion]` 0 基（973 行起）973+121=**1094 行 = `Animation/TripleStab.ani`** ✓ 对位吻合。`[etc attack info]`（1294 行起）：行 1379/#85 = `AttackInfo/TripleStab.atk`、行 1380/#86 = `AttackInfo/TripleStabFinish.atk`。
2. **数据文件**：角色 .ani/.atk/.als/特效全在（§3）；无 passiveobject 条目（`passiveobject.lst` grep 无命中）——伤害判定全在角色侧帧盒，无 PO。
3. **兄弟职业参照**：`atswordman_load_state.nut` / `jg_swordman_load_state.nut` 均无 triplestab 注册（grep 实测）——本技能连参照脚本都没有，纯引擎内置（比 064/008 更彻底：008 还有 jg 版参照）。行为重建只靠 .ani/.atk/.skl 三方。

### 2.2 引擎行为重建（.ani 标记 + .atk + skl 对位）

**施放流程（单动画三窗口，586ms）**：引擎播 `TripleStab.ani`（etc #121，14 帧 586ms），按帧内攻击盒分三段刺击：

| 段 | 帧窗口 | 攻击盒（min/max px，064 口径） | 方向（explain 序） | 倍率列 |
|---|---|---|---|---|
| 刺 1 | **F1-F3**（45ms×3） | x∈[18,188] y∈[-15,30] **z∈[-30,103]**（偏低） | 下段 | col0 |
| 刺 2 | **F7-F9** | x∈[33,115] y∈[-15,30] **z∈[13,114]**（偏高） | 上段 | col0 |
| 刺 3 | **F11-F13** | x∈[61,199] y∈[-15,30] z∈[13,83]（居中、最远） | 中段 | col1 |

**SET FLAG 实测**：F1=**9**、F6=**1**、F7=**9**、F10=**2**、F11=**9**、F12=**65534**。
- flag **9** 在每个刺击窗口首帧出现（F1/F7/F11）——推断=引擎"攻击窗口生效/命中重置"标记（与 008 三段斩的段号 flag 1-5 不同型，具体语义引擎内置未考证）；
- flag **1**/**2**（F6/F10）在 2、3 段窗口**前**一帧——推断=段切换/攻击信息切换标记（1→主 atk、2→finish atk？无脚本佐证）；
- flag **65534**（F12）= 收尾/取消标记（多技能同型，惯例语义）。
- F5 delay=1ms：三段之间的 0 长过渡帧。

**两份 atk 的分工（推断）**：`TripleStab.atk`（#85）用于刺 1/刺 2（damage 反应/push30/lift70/knuckback 3-35）；`TripleStabFinish.atk`（#86，blow/knuckback 3-100/**push400/lift200**）用于刺 3 末段大击退——与 explain"并击退敌人"和模板"中段独立倍率"三点吻合（引擎选用时机未考证，按帧窗口对位属高置信推断）。

**门禁**：只能在鬼影步状态施放——引擎检查鬼影步（技能 18，018-GhostStep）在场 buff；我们的 Buff 查询门面缺失，demo 直放开门禁（§7）。

### 2.3 被动对象 / appendage

**无**（passiveobject.lst 无条目；白名单内无 ap_triplestab 类脚本；伤害全在角色帧盒）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\triplestab.ani`（etc #121） | 14（F0-13） | 586ms（45×13+1+45） | F1=9、F6=1、F7=9、F10=2、F11=9、F12=65534 | **F1-F3 / F7-F9 / F11-F13** | 三段刺击；每帧 2-4 damageBox；仅引 sm_body 图集 |
| 特效 `effect\animation\triplestab\0_normal.ani` | 15 | 675ms | 无 | 无 | 刺光·普通层（.als @F0） |
| 特效 `1_dodge.ani` | 15 | 675ms | 无 | 无 | 刺光·减淡层（.als @F0） |

`.als`（triplestab.ani.als）：`[use animation]` 注册 normal/dodge 两层 + `[none effect add]` F0×2（标记 10000/10001）——**已有节型，AlsParser 全覆盖**（L12/L15）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | TripleStab.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\TripleStab.skl` | ✅ 实测 | 2 列等级数据 + static 2000 |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | 见 §2.1 |
| 常量表 | swordman_header.nut:291 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_TRIPLESTAB=121 |
| 参照脚本 | —（不存在） | `…\pvf\sqr\character\{atswordman,jg_swordman}\`（grep 实测无） | ⛔ 缺失 | 纯引擎内置，无同型参照 |
| .chr 条目 | etc motion #121（行 1094）+ etc attack info #85/#86（行 1379/1380） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 动画 + 两份 atk |
| 角色 .ani | triplestab.ani + triplestab.ani.als | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 帧表 |
| 角色 .atk | triplestab.atk / triplestabfinish.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测（全读） | 刺 1/2 + 末段击退 |
| 特效 .ani | 0_normal.ani / 1_dodge.ani（+ triplestab_ds\ 剑影版目录） | `…\pvf\character\swordman\effect\animation\triplestab\` | ✅ 实测 | 刺光双层 |
| 装备层 | triplestab* ×76 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ 实测（存在性） | avatar 变体图层 |
| 关联取消 | canceltriplestab.skl | `…\pvf\skill\Swordman\canceltriplestab.skl` | ✅ 存在 | 强制-鬼影三击剑（记档） |
| 关联强化 | triplestabex.skl（[feature skill index] 116） | `…\pvf\skill\Swordman\triplestabex.skl` | ✅ 存在 | E 批另行分析 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画（单图集约定，L16） | 必需（共享） | ✅ `sm_body0000.img.bytes` 在库 |
| `Effect/TripleStab/0_normal.img` | sprite_character_swordman_effect_triplestab.NPK | 刺光普通层 | 可选（角色动画本身完整；还原时再提） | ❌ |
| `Effect/TripleStab/1_dodge.img` | 同上 | 刺光减淡层 | 可选 | ❌ |

**缺失 img：必需级 0**、可选级 2 张（同属一个 NPK）。本技能是零必需 img 的轻量技能。

## 5. 实现方案草案

### 内容件清单（全部继承真实基类；数值 DNF 原值 + demo 建议值并列）

1. **`DotNet~/Skills/TripleStabSkill.cs : SkillLogic`**（单动画三窗口帧驱动，同 NormalAttack/008-TripleSlash 草案范式）
   - `CooldownMs = 8000`（DNF 原值直用）；`TotalTimeMs = 600`（动画 586ms 取整）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanTripleStab)` + `ctx.ClearHitTargets()`；不设手动攻击盒——**帧驱动攻击盒**：triplestab.ani 的 F1-F3/F7-F9/F11-F13 盒翻译进 json 后由 LSHitboxComponentSystem 自动激活（releasewavedash 先例，判定帧表扩 1 条）。
   - `OnUpdate` 段间推进（SubState=已发段数 0→3）：
     - 段间 F4-F6（约 180-270ms 窗）与 F8-F10、F13-末：各段窗口结束时 `ctx.ClearHitTargets()`（段间重置，L19 已通）——实现上按帧号守卫：`帧>3 且 SubState==0 → ClearHitTargets(); SubState=1`，F>7 → SubState=2，F>13 → SubState=3。
     - 刺 1/刺 2 用技能本体 `HitReaction`（atk 主版：Damage=55/HitstunMs=500/KnockbackX=30/LaunchY=70）。
   - **刺 3（末段击退）改走一次性 Area**（SkillLogic.HitReaction 单值限制，008/064 同款手法）：F11 帧触发 `ctx.CreateAreaInFront(AreaIds.TripleStabFinish, 1.0)`——
2. **`DotNet~/Areas/TripleStabFinishArea.cs : AreaDefinition`**（BloodBoomArea 一次性 EnterActions 范式）
   - `TotalTimeMs = 200`（F11-F13 活跃窗）、`EnterActions = { MeleeHit }`、`HalfExtents = (0.8, 0.25, 0.5)`（F12-13 盒 x∈[61,199] y∈[-15,30] z∈[13,83] 折算半尺寸）、
   - `HitReaction { Damage = 90, HitstunMs = 700, KnockbackX = 400, LaunchY = 200 }`（finish atk 原值 push400/lift200/blow）；`ViewAnimId = None`（视觉由角色动画+刺光层承担）。
3. **无需新 Action/Buff**（MeleeHit 现成；无异常状态）。
4. 门禁（鬼影步状态）不做（§7）；暗属性元素不带（元素系统缺失）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎内置状态（无注册）+ TripleStab.ani | `TripleStabSkill : SkillLogic` + AnimId |
| 三段帧盒（F1-3/F7-9/F11-13） | json attackBoxes 帧驱动（零代码判定） |
| flag 9 ×3（窗口生效标记） | 帧号 const + SubState 守卫（不进翻译链路） |
| atk 主版/finish 版分工 | 技能 HitReaction（刺1/2）+ Finish Area HitReaction（刺3） |
| 段间多目标重命中 | 段末 `ClearHitTargets()`（L19 已通） |
| 鬼影步状态门禁 | Buff 查询门面缺失 → 不做 |
| .als 刺光双层（normal/dodge @F0） | overlay 翻译直用（AlsParser 已支持） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.TripleStab = 20` + ButtonToSkill 新键 |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanTripleStab = 81`、`TripleStabFxNormal = 82`、`TripleStabFxDodge = 83` |
| json 注册 | `…\LSAnimClipRegistrar.cs` | `RegisterOne` ×1~3（角色 1 必需；刺光 2 可选） |
| 帧判定���表 | LSHitboxComponentSystem | 扩 1 条（triplestab 帧盒） |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 无新增（sm_body 已在库；刺光可选后补） |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 8000 ms | 8000（直用） |
| 总时长 | 586ms（14 帧） | 600 |
| 刺 1（下段） | col0 146%+ / atk 主版 push30 lift70 | 伤害 55/硬直 500/Kb 30/Ly 70 |
| 刺 2（上段） | col0（同列）/ atk 主版 | 同刺 1（数值复用） |
| 刺 3（中段/末段） | col1 678%+ / finish atk push400 lift200 blow | 伤害 90/硬直 700/Kb 400/Ly 200（Area 承担） |
| 段窗口 | F1-F3 / F7-F9 / F11-F13（各 135ms） | 帧驱动直译 |
| 攻击盒 | 三窗口实测值（§2.2 表） | 帧驱动直译（finish Area 半尺寸 (0.8,0.25,0.5)） |
| static 2000 | 语义未考证 | 不用 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| TripleStab.skl | `.skl` 无子命令（2 列 + static 1 值） | 手抄量极小；并入既有缺口 |
| 2 份 .atk | `.atk` 无子命令 | 手抄（每份 ~8 值）；并入既有缺口 |
| triplestab.ani | `[SET FLAG]`（9/1/2/65534） | 按既有约定跳过（段号 const 进技能类，064/008 同构）——非缺口 |
| triplestab.ani.als | `[none effect add]` 已支持（L12/L15） | 无缺口 |
| 0_normal.ani / 1_dodge.ani | 常规节 | 现有 ani 子命令全覆盖 |

本技能翻译缺口：`.skl`/`.atk` 两类既有（计 2 条），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 只能在鬼影步状态施放（引擎门禁） | Buff 查询门面（缺失，R1-A3 记档） | demo 无门禁直放 |
| 暗属性魔法伤害 | 元素属性系统（缺失） | 数值不带属性 |
| flag 9 / 1 / 2 的引擎语义 | 未考证（引擎内置） | 按帧窗口对位实现，不依赖 flag |
| MP 60-924 | MP 系统（延后） | 跳过 |
| 等级缩放（2 列） | 延后 | 固定值 |
| 音效 TWINSWD_HIT_01 | 延后 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. `[static data] 2000` 语义（推断与引擎位移/判定扩展相关——三刺是否有小步前移不可见，.ani 无位移标记）。
2. flag 9/1/2 的精确引擎语义；两份 atk 与三窗口的对位（刺3=finish 为命名+击退数值+模板三重吻合的高置信推断，无脚本实证）。
3. 引擎是否在同段内允许重复命中（.ani 无 resetHit 标记，按单命中处理）。
4. skill class 3 与 1/2 的差异含义（引擎分类号，未考证）���

**新系统级缺口**：无（本技能全部落在已有/已记档能力内）。

**翻译工具缺口**：`.skl`/`.atk`（既有两类）。

**给下轮的经验**：技能 112 是"纯引擎内置且无任何兄弟参照"的极简样本（零 PO、零必需 img、单动画三窗口）——此类技能走读只需 .chr 槽位（973/1294 行基）+ .ani 帧盒 + .atk 三件套；[SET FLAG] 的 **9** 值在多段刺击类 .ani 中复现（本例 ×3），疑为"攻击窗口生效"类引擎标记，后续同型技能可直接按窗口对位不追语义。
