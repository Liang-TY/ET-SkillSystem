# 鬼影鞭（GhostSideWind）

> 技能ID 111 | 级别 A | 可实现性 ✅（直接，基础直发版） | 分析日期 2026-08-22 | 批次 A9

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼影鞭 | `skill\Swordman\GhostSideWind.skl` [name] |
| 英文名 | GhostSideWind（取 skl 文件名；[name2]="Apparition Shadow Strap"） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype]=2，L17 映射；暗属性/鬼影系佐证） | 同上 |
| 学习等级 | 30（前置：鬼影步 18 Lv1） | 同上 |
| 最高等级 | 70（鬼泣段上限 50） | 同上 |
| 类型 | 主动（active，skill class 3） | 同上 [type] |
| 指令 | →←→ + Z；**只能在 [鬼影步] 已施放状态下使用** | 同上 [command] + [basic explain] |
| CD | 8000ms | 同上 [dungeon][cool time] |
| MP | 55 → 700（Lv1→Lv70） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 一句话效果 | 挥鞭横扫中距离敌人（2 击，最远约 2.95 单位），把远处的敌人拉到身前 | 同上 [explain] + 攻击盒实测 |

**level property 解码**（3 个 `<int>` ↔ 3 行向量，L21 法）：

| 模板占位 | 向量 | 解读 | 值 |
|---|---|---|---|
| 第1击攻击力 `<int>`% | `(-1, 0, 1.0)` | level col0 | 761% → 9066% |
| 第2击攻击力 `<int>`% | `(-1, 1, 1.0)` | level col1 | 1347% → 16027% |
| 攻击范围 `<int>`% | `(2, 2, 1.0)` | static[2]=100 | 100% |

static data `50 2500 100`（pvp `100 1500 100`）：[2]=范围% 已实证；[0]/[1]（50/2500，pvp 减半至 100/1500）**推断为拉拽参数**（拉拽保持时间/拉拽力，pvp 减弱佐证），引擎消费，未考证。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**引擎内置技能（F3 完整形态���**：`swordman_load_state.nut` 按名/按号 111 均无 pushState（实测）；`sqr\character\swordman\` 白名单全树无 ghostsidewind 引用（实测）；`atswordman_load_state.nut` / `demonicswordman_load_state.nut` 亦无同名注册（参照脚本二路皆空）。前置技能鬼影步（18）的 appendage `ap_ghoststep.nut` 实测为空壳（仅空 sq_AddFunctionName）——**鬼影步状态与鬼影鞭状态逻辑均在引擎内**，pvf 只提供数据 + 动画 + 命中参数。

常量佐证：`swordman_header.nut:290` `CUSTOM_ANI_GHOSTSIDEWIND <- 120` ↔ `.chr` etc motion 槽 120 实测一致。

### 2.2 引擎行为重建（.ani + .atk 两方印证）

- **onSetState（推断）**：鬼影步状态中输入 →←→+Z → 切引擎内置鬼影鞭状态：播 `GhostSideWind.ani`（.chr 槽 120，8 帧 760ms）；设攻击信息 `sq_SetCurrentAttackInfo(84)`（.chr etc attack info 槽 84 实测 = `AttackInfo/GhostSideWind.atk`）；扣 MP、进 CD 8000。
- **命中窗口（帧驱动，.ani 实测攻击盒）**：

| 帧 | 累计时间 | 攻击盒（min/max 像素） | 前伸 | 备注 |
|---|---|---|---|---|
| F2 | 360ms | `13,-15,15 / 274,30,77` | 2.74 单位 | 挥出；PLAY SOUND MENGRYOUNG_ATK |
| F3 | 440ms | `13,-15,15 / 295,30,73` | **2.95（最远）** | **SET FLAG 1** |
| F4 | 520ms | `13,-15,15 / 261,30,53` | 2.61 | 收鞭中 |
| F5 | 600ms | `13,-15,15 / 136,30,62` | 1.36 | 近身段 |
| F6 | 680ms | — | — | **flag 65534**（惯例取消/命中标记，语义未考证，同 064 F14） |

- **两击结构**：skl 双伤害列（第1击/第2击）+ F2-F5 连续攻击盒 + F3 flag 1——**推断**引擎在 F3 处重置命中表并切换到第 2 伤害列（F2-F3=第1击远端拉拽，F4-F5=第2击近身鞭击）；无脚本可证，标未考证。
- **拉拽**：`ghostsidewind.atk` 实测 `[push aside] -200` + `[lift up] 50`——**负 push = 向攻击者方向位移（拉到身前）**，正是 explain"把远距离的敌人拉到身前"的数据源。
- **onEndCurrentAni（推断）**：回待机（或回鬼影步状态）。

### 2.3 特效

`character\swordman\effect\animation\ghostsidewind\`：`00_sword_normal.ani`（8 帧，普通混合鞭刃）、`01_sword_dodge.ani`（8 帧，加法混合辉光层）——**无 .als 边车、无脚本引用**（实测），引擎在挥鞭时绘制（同 064 gorecross slash1-4"引擎内置绘制无声明式来源"形态）。img：`Character/Swordman/Effect/GhostSideWind/00_sword_normal.img` / `01_sword_dodge.img`。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GhostSideWind.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GhostSideWind.skl` | ✅ | 技能数据 |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置状态） | §2.1 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（白名单 grep 无） | ⛔ 缺失 | 逻辑在引擎 |
| 前置 appendage | ap_ghoststep.nut | `…\pvf\sqr\character\swordman\appendage\ap_ghoststep.nut` | ✅（空壳） | 鬼影步状态（引擎） |
| 状态常量 | swordman_header.nut:290 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | CUSTOM_ANI_GHOSTSIDEWIND=120 |
| .chr 条目 | etc motion #120 + etc attack info #84 | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | Animation/GhostSideWind.ani；AttackInfo/GhostSideWind.atk |
| 角色 .ani | ghostsidewind.ani | `…\pvf\character\swordman\animation\ghostsidewind.ani` | ✅ 实测 | 8 帧 760ms，F2-F5 攻击盒，flag 1/65534 |
| .atk | ghostsidewind.atk | `…\pvf\character\swordman\attackinfo\ghostsidewind.atk` | ✅ 实测 | 魔法/暗属性/反应 none/hit down/hit direction back/ignore weight/**push -200/lift 50**/[hit info] 10 1.0（未考证） |
| .als | —（无） | `…\pvf\character\swordman\animation\`（ls 实测） | ⛔ 无边车 | 特效引擎绘制 |
| 特效 .ani | 00_sword_normal.ani / 01_sword_dodge.ani | `…\pvf\character\swordman\effect\animation\ghostsidewind\` | ✅ 实测 | 鞭刃双层视觉（_ds 剑影变体跳过） |
| 被动对象 | — | `…\pvf\passiveobject\character\swordman\animation\`（grep 无 ghostside） | ⛔ 无（武器判定型） | — |
| 装备层 | ghostsidewind.ani ×76 | `…\pvf\equipment\character\swordman\avatar\` | ✅ 实测（find 计数 76） | 只查存在性 |
| 关联强化 | GhostSideWindEx.skl（技能 115，即 skl [feature skill index] 115） | `…\pvf\skill\Swordman\` | ✅ 存在 | E 类批次另行分析 |
| 关联取消 | cancelghostsidewind.skl | `…\pvf\skill\Swordman\` | ✅ 存在 | 强制-鬼影鞭（记档） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集（帧 10/11/12/199） | 必需（共享） | ✅ 已在库（L16） |
| 00_sword_normal.img | sprite_character_swordman_effect_ghostsidewind.NPK | 鞭刃主视觉（8 帧） | **必需**（角色 .ani 只有身体姿势，鞭刃全在特效层） | ❌ 未入库 |
| 01_sword_dodge.img | 同上 | 鞭刃辉光层（8 帧，LINEARDODGE 同类混合） | **必需** | ❌ 未入库 |

缺失 img：必需级 2 张（同一 NPK，一次提取全覆盖）。

## 5. 实现方案草案

### 内容件清单（零新机制；拉拽用既有击退物理的负值语义）

1. **`DotNet~/Skills/GhostSideWindSkill.cs : SkillLogic`**（帧驱动范式，同 NormalAttack/SwordmanReleaseWaveDash）
   - `CooldownMs = 8000`（DNF 原值直用）；`TotalTimeMs = 760`（动画全长）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanGhostSideWind)` + `ctx.ClearHitTargets()`。
   - `OnUpdate`（两击分段，L19 段间重置）：`CurrentFrameIndex() > 3 && GetSubState() == 0` → `ctx.ClearHitTargets()` + `SetSubState(1)`（F3 之后重置命中表 → F4-F5 第二击可再中；两击伤害同值，见 §7）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - `HitReaction{Damage = 100, HitstunMs = 600, KnockbackX = -200, LaunchY = 50}`——**KnockbackX 负值即拉拽**：`LSActionContext.LaunchOwner`（`Runtime\LSAction.cs:106` 实测）速度 = dir × (knockbackX/100) × 2.5，负值翻转方向 → 目标以 5 单位/s **朝攻击者滑行**，贴地摩擦减速停在身前；lift 50 → 垂直 1 单位/s 微浮。DNF 原值直译，无需改框架。
   - 攻击判定全帧驱动：F2-F5 攻击盒由 json attackBoxes + `IsAttackDrivenAnim` 白名单驱动（`LSHitboxComponentSystem.SampleBox` 面左镜像已有），最远 2.95 单位 = 鞭子射程直接成立。
2. **无需新 Area/Bullet/Buff/Action**——MeleeHit 全覆盖（伤害+硬直+受击动画+拉拽）。
3. **鞭刃视觉**：为 `AnimId.SwordmanGhostSideWind` 手工注册 AnimOverlayConfig（两条 entry：startFrame=2，z=2 → GhostSideWindSword；z=3 → GhostSideWindSwordDodge）——releasewave 手组装 overlay 先例，绕开"引擎绘制无声明式来源"缺口。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎内置鬼影鞭状态 | `GhostSideWindSkill : SkillLogic` + AnimId |
| .chr 槽 120 动画 | `AnimId.SwordmanGhostSideWind`（swordman_ghostsidewind.json） |
| F2-F5 武器攻击盒（帧驱动） | json attackBoxes + IsAttackDrivenAnim 白名单 |
| atk push -200 拉拽 | `HitReaction.KnockbackX = -200` → LaunchOwner 负值反向（**既有物理直接支持**） |
| 第1击/第2击双伤害列 | 段间 ClearHitTargets（两击均可中）+ 单一 HitReaction（伤害合一，简化） |
| 鬼影步状态前置 | 砍掉：独立按键直发 |
| 引擎绘制鞭刃特效 | 手工注册 overlay（两层） |
| 暗属性 | 砍掉（元素系统缺失） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.GhostSideWind = 17` + ButtonToSkill case 9 |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanGhostSideWind = 65`、`GhostSideWindSword = 66`、`GhostSideWindSwordDodge = 67` |
| 判定帧表 | `Packages\cn.etetet.skill\Scripts\Hotfix\Share\LSHitboxComponentSystem.cs` | `IsAttackDrivenAnim` 加 `AnimId.SwordmanGhostSideWind` |
| json/图集 | `…\LSAnimClipRegistrar.cs` / `…\LSAnimResComponentSystem.cs` | swordman_ghostsidewind.json + 00_sword_normal/01_sword_dodge.img.bytes + 手工 RegisterOverlay |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 9 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 8000ms | 8000（直用） |
| 总时长 | 760ms（8 帧：200+80×7） | 760 |
| 第1击伤害 | level col0：761%→9066% | 100（与第2击合一） |
| 第2击伤害 | level col1：1347%→16027% | 同上（单一 HitReaction 简化） |
| 拉拽 | push aside **-200** | KnockbackX -200 直用（拉向攻击者 5 单位/s） |
| 微浮 | lift up 50 | LaunchY 50 直用 |
| 射程 | 攻击盒 F3 最远 295px = 2.95 单位 | 帧驱动直用 |
| 硬直 | atk 未配 damage reaction 硬直值 | HitstunMs 600 |
| 攻击范围缩放 | static[2]=100%（随级 Ex 才变） | 100% 固定 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ghostsidewind.ani | 常规节（FRAME/IMAGE/DELAY/DAMAGE BOX/ATTACK BOX/SET FLAG/PLAY SOUND） | `ani` 子命令全覆盖（SET FLAG/PLAY SOUND 按约定跳过） |
| 00/01_sword.ani | 常规节 + [SHADOW] | 覆盖（SHADOW 既有记档跳过） |
| GhostSideWind.skl | `.skl` 无子命令（3 列 static + 2 列等级表） | 手抄 6 值可行（既有记档） |
| ghostsidewind.atk | `.atk` 无子命令；`[hit info]`（空``×2 + `10 1.0`）语义未考证 | 手抄（push/lift/direction 6 值）；[hit info] 建议加 `atk` 子命令时一并考证（疑似多段命中参数，与双伤害列相关） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 只能在鬼影步状态中发动 | 状态前置缺失（R1-A1 累计）+ 鬼影步本身引擎内置未分析 | 独立直发（基础版） |
| 第1击/第2击两列不同伤害 | 单 HitReaction 表达不了段间切值（064 同类记档） | 两击同伤害；段间 ClearHitTargets 保住"两击都中"的结构 |
| 拉拽把敌人精确放到身前固定位 | 我们的负击退=初速度+摩擦滑行（无"到位即停"） | 摩擦自然停在近身，手感近似（参数可调：KnockbackX 大→拉得近） |
| 暗属性伤害 | 元素属性系统缺失 | 跳过 |
| flag 65534（F6） | 语义未考证（惯例取消/命中标记） | 忽略 |
| 引擎绘制鞭刃双层特效 | 无声明式来源（064 累计缺口） | 手工注册 overlay（本档 §5 已给配置） |
| 音效 MENGRYOUNG_ATK / SQUARESWDA_HIT_01 | 音频延后 | 跳过（已考证记档） |

## 8. 存疑与缺口上报

- **未考证**：①两击分段与 F3 flag 1 的关系（推断=命中表重置+伤害列切换）；②static[0]=50/static[1]=2500（pvp 100/1500）的拉拽语义；③`[hit info]` 节 `10 1.0` 含义（疑似多段命中参数）；④鬼影步（18）状态的引擎行为（前置技能，另行分析批次）。
- **新系统级缺口**：无新增（拉拽已被负值击退物理覆盖——**这是本批最重要的正面结论**：DNF 负 push 语义与 LaunchOwner 方向乘法天然兼容，无需新机制）。
- **翻译工具缺口**：无新增节（.skl/.atk 既有项）；`[hit info]` 建议随 atk 子命令立项时考证。
- **给下轮的经验**：中程"拉拽/钩锁"类技能（鬼影鞭/其他 grab 变体）先查 .atk `[push aside]` 是否负值——负值=拉，我们 HitReaction.KnockbackX 负值直译即得，不要新建机制；鞭/锁类视觉通常无 .als（引擎绘制），统一走手工 overlay 挂在角色动画上。
