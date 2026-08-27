# 共鸣：离魂一闪（spiritcrossslash）

> 技能ID 66 | 级别 A | 可实现性 ✅（直接；幻鬼视觉连续性/精通提速简化） | 分析日期 2026-08-22 | 批次 A10

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 共鸣 ： 离魂一闪 | `skill\Swordman\ghostsword\spiritcrossslash.skl [name]` |
| 英文名 | spiritcrossslash（取 skl 文件名，全小写；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype]=5，L17；ghostsword 族） | 同上 |
| 学习等级 | 30 | 同上 [required level] |
| 最高等级 | 60（growtype0/5 段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ←→→ + Z（指令施法 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 12000 ms | 同上 [cool time] |
| MP | 61 → 640（dungeon；pvp 30→320） | 同上 [consume MP] |
| 可施放状态 | 0（站立）/ 8（攻击中）/ 14（未考证，疑幻鬼共存态） | 同上 [executable states] |
| 前置 | 幻鬼剑术 BLADESPIRIT（123）Lv1 | 同上 [pre required skill] + `swordman_header.nut:146 SKILL_BLADESPIRIT <- 123` |
| 一句话效果 | 幻鬼（分身）快速前突斩击，随后与剑影交叉换位再斩，两段物理伤害；分离态施放时幻鬼中断当前动作立即发动交叉斩 | 同上 [explain] |

**static data**：dungeon `400 0` / pvp `400 0`（两值，脚本直读已证）：
- col0 = **400**：幻鬼与剑影的突进距离（px）；
- col1 = **0**：特殊功能开关——"普通状态下施放时删除幻鬼突进、前方立即生成幻鬼并发动交叉共鸣"（level property 模板明示 [0 关 1 开]；本 pvf 两端都是 0=关）。

**level info（2 列，Lv1 → Lv60）**：col0 突进斩击攻击力 1626→13010、col1 交叉共鸣攻击力 3794→30357。

**level property 模板**（4 占位符）：突进斩击攻击力/交叉共鸣攻击力/突进距离 px/特殊功能开关——向量 `(-1,0,1.0)`、`(-1,1,1.0)`、`(0,0,1.0)`、`(1,1,1.0)`。
**实证**：后两向量按 L21 规则（≥0=static 槽）指向 static[0]=400（距离）与 static[1]=0（开关）**与脚本 `sq_GetIntData(66, 0/1)` 消费完全吻合**；前两向量是常量 stub（-1,0/-1,1），真实数值走 level info 两列（022-FireWave 已证 -2 才是 level 列引用——本模板未用，属 mod 模板笔误，不影响解读）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
168: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/spiritcrossslash/spiritcrossslash.nut", "spiritcrossslash", STATE_SPIRITCROSSSLASH, SKILL_SPIRITCROSSSLASH);
```

- `swordman_header.nut`：`STATE_SPIRITCROSSSLASH <- 109`（56 行）、`SKILL_SPIRITCROSSSLASH <- 66`（147 行）、`CUSTOM_ANI_SPIRITCROSSSLASH1 <- 301` / `2 <- 302`（472-473 行）。
- .chr etc motion 0 基索引实测：**301 = `Animation/spiritcrossslash1.ani`（行 1274）、302 = `spiritcrossslash2.ani`（行 1275）**——与常量精确对位。
- 幻鬼 = **F5 unclebang 共享 PO 24349**，写包首 dword = **68**（`passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` 定义 + `sqr\shared_passive_object\swordman\*.nut` 六回调分派）。

### 2.2 主 nut 逐回调（spiritcrossslash.nut，189 行全读）

**checkExecutableSkill**：state ∈ {0, 8, 14} 才放行（攻击中可取消接技——见 §7 取消体系缺口）→ 子状态 0 进状态 109。

**onSetState**（subState 0/1 两段）：
```
速度系数 = BLADESPIRIT(123) level data col0/col1 + 1（幻鬼剑术提速，前置精通）
subState 0（幻鬼突进段）：
    播 CUSTOM_ANI_SPIRITCROSSSLASH1（spiritcrossslash1.ani，提速系数）
    sq_StopMove；创建 PO 24349（写包 68）——若场上已有幻鬼对象(VSObject)先销毁再建
subState 1（交叉共鸣段）：
    播 CUSTOM_ANI_SPIRITCROSSSLASH2（spiritcrossslash2.ani）
    记录 xDistance = 施法者 x + static col0(400px)（前突目标点）
```

**onKeyFrameFlag**（SET FLAG 10001）：
```
subState 0、F5(300ms)：als_ani 叠加幻鬼出发特效（vsstarta_01.ani @0ms + vsattackb1_00.ani @300ms）
subState 1、F0(0ms)：叠加 4 层剑影侧特效（bsattacka1_01 / bsattackb1_01 / spritcrossslashbsstartmove /
    crossspark_00，各延迟 130ms）+ 屏震 5/200
```

**onProc**（subState 1）：`sq_GetUniformVelocity(当前x, xDistance, t, 50)` → 50ms 内匀速前突 400px（极速换位斩）。

**onProcCon**（subState 0）：static col1 > 0 时**立即**切 subState 1（删除幻鬼突进段——本 pvf 开关关闭，不走）。

**onEndCurrentAni**：subState 0 → 切 1；subState 1 → 回站立。

### 2.3 幻鬼被动对象（共享 PO 24349，id=68 三相位）

`sqr\shared_passive_object\swordman\` 六回调（本技能走 setstate/onkeyframeflag/onendcurrentani/procappend 的 case 68）：

| 相位 | 动画（etc motion 0 基索引） | 行为 | 伤害 |
|---|---|---|---|
| state 10 突进斩 | **#82** `animation/spiritcrossslash/vsreadya_body.ani`（6 帧 720ms） | 匀速前突 400px/1000ms（static col1>0 时改为瞬移至前方 400px）；结束→state 11 | `sq_GetBonusRateWithPassive(66,-1,0)` = **level col0**；攻击信息 `sq_GetCustomAttackInfo(obj, 40)` |
| state 11 交叉斩 | **#83** `vsattacka_body.ani`（5 帧 250ms） | **转向 180°**，反方向 50ms 疾进 400px（与剑影交叉）；结束→state 12 | **level col1**；攻击信息同 40 |
| state 12 消散 | **#84** `vsattackb_body.ani`（16 帧 2200ms，F9 flag 10001） | 叠 vsattacka1_03 特效；F9 叠 disappearback/front 消散层；播完销毁 | 无 |

**攻击信息表 40 的指向（本 pvf 实证，重要）**：`swordman_shared.obj` [etc attack info] 0 基直读 #40 = `suddenstrikevs.atk`；而按名称匹配应为 #42 `spiritcrossslash.atk`。**VS 族（id 66-74）的 atk code 全部比名称匹配位低 2**（windspiritvs 39→41、moonspiritslash 41→43、hellslash 42/43/44→44/45/46 三例交叉印证），而波动族（id 15-17）code=直读（icewave/firewave 对位吻合）——**本 pvf 该表被 mod 插入过条目，VS 族回调代码是按旧表写的，运行时实际加载 suddenstrikevs.atk**。两份 atk 实测：

| atk | 关键参数 | 用途 |
|---|---|---|
| spiritcrossslash.atk（意图值） | physic/weapon damage、reaction **damage**、cut+blood 70、无 push/lift | 平砍型（我们采用） |
| suddenstrikevs.atk（运行时实际） | physic/weapon、reaction **down**、push 20、**lift -2000**（砸落）、ignore weight、hit down/inner、force hitstun 1000、knuck back -1 | 本 pvf 实际手感（砸地控） |

**玩家侧 atk**（.chr etc attack info #123/124，脚本未显式调用、引擎接线未考证）：
- `spiritcrossslashfirst.atk`：physic/weapon、damage 反应、cut+blood 70、knuck back 3/10、lift 200、push 0；
- `spiritcrossslashsecond.atk`：physic/weapon、**down** 反应、cut+blood 70、**lift 360**、push 0。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\spiritcrossslash1.ani`（玩家段1，.chr #301） | 14 | 840（60×14） | **F5=10001**、F13=10002 | 无 | 10001=幻鬼出发特效；10002 无脚本消费者（引擎/未考证）；仅引 sm_body 图集 |
| `spiritcrossslash2.ani`（玩家段2，#302） | 8 | 400（60×4+40×4） | **F0=10001** | 无 | 4 层剑影特效 + 屏震 |
| vsreadya_body.ani（幻鬼段1） | 6 | 720 | 无 | 无（判定在攻击信息） | 引 `Effect/BladeSpiritDot/VengeanceSpirit.img` |
| vsattacka_body.ani（幻鬼段2） | 5 | 250 | 无 | 无 | 同上 |
| vsattackb_body.ani（幻鬼段3） | 16 | 2200 | F9=10001（消散层） | 无 | 同上 |

`.als` 边车（unclebang animation\spiritcrossslash\ 目录 6 个）：vsreadya_body.ani.als（**[create draw only object]** 节首见变体——无 "follow parent" 后缀，缺口累计已录 R1-A4 家族）、vsstarta_01/vsattackb1_00/vsattacka1_03（[none effect add] 叠层）等，全部常规可译。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | spiritcrossslash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\spiritcrossslash.skl` | ✅ 实测 | 等级/CD/MP/static/2 列 level info/level property 4 向量 |
| 注册行 | swordman_load_state.nut 行 168 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 5_ghostsword\spiritcrossslash\spiritcrossslash.nut，状态 109，技能 66 |
| 主 nut | spiritcrossslash.nut | `…\pvf\sqr\character\swordman\5_ghostsword\spiritcrossslash\spiritcrossslash.nut` | ✅ 实测（189 行全读） | 玩家侧两段式 |
| 共享 PO 回调 | setcustomdata/setstate/onkeyframeflag/onendcurrentani/procappend 的 case 68 | `…\pvf\sqr\shared_passive_object\swordman\*.nut` | ✅ 实测 | 幻鬼三相位 |
| 共享 PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ 实测 | etc motion #82/83/84、etc attack info #40（错位见 §2.3） |
| 共享 PO atk | spiritcrossslash.atk / suddenstrikevs.atk | `…\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\` | ✅ 实测 | 意图值 / 运行时实际 |
| .chr 条目 | etc motion #301/302 + etc attack info #123/124 | `…\pvf\character\swordman\swordman.chr` 行 1274/1275 / 1416-1418 | ✅ 实测 | 玩家两段动画 + first/second/spiritcrossslash 三 atk |
| 玩家 .ani | spiritcrossslash1/2.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 840/400ms |
| 玩家 .atk | spiritcrossslashfirst.atk / spiritcrossslashsecond.atk / spiritcrossslash.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 引擎接线未考证（§8） |
| 幻鬼 .ani | vsreadya_body / vsattacka_body / vsattackb_body（+特效层 vs* ×27） | `…\passiveobject\unclebang_shared_passive_object\swordman\animation\spiritcrossslash\` | ✅ 实测 | 三相位 + 特效组 |
| 剑影特效 | bsattacka1_01 / bsattackb1_01 / spritcrossslashbsstartmove / crossspark_00（+同族 ×18） | `…\pvf\character\swordman\effect\animation\spiritcrossslash\` | ✅ 实测 | subState 1 四层叠加 |
| 装备层 | spiritcrossslash 系 ×18（coat 层抽样） | `…\pvf\equipment\character\swordman\avatar\coat\*\` | ✅ 实测（存在性） | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 玩家两段动画 | 必需（共享） | ✅ |
| `Character/Swordman/Effect/BladeSpiritDot/VengeanceSpirit.img` | sprite_character_swordman_effect_bladespiritdot.NPK | **幻鬼身体**（三相位全部） | **必需** | ❌ |
| `Character/Swordman/Effect/SpritCrossSlash/VSAttackA.img`、`VSAttackG.img`、`VSAttackH.img` | sprite_character_swordman_effect_spritcrossslash.NPK | 幻鬼消散段特效（vsattacka1_03/disappear） | 可选 | ❌ |
| `…/SpritCrossSlash/BSAttackA.img`、`BSAttackC.img` | 同上 | 剑影侧斩击特效 | 可选 | ❌ |
| `…/SpritCrossSlash/SparkA.img`、`SparkB.img` | 同上 | 交叉火花（crossspark_00 等） | 可选 | ❌ |
| `Character/Swordman/Effect/TeleportVS/LDodge.img`、`Normal.img` | sprite_character_swordman_effect_teleportvs.NPK | 出发/消散传送特效（vsstarta/vsappear 系） | 可选 | ❌ |

缺失 img：必需 1 张（VengeanceSpirit）、可选 8 张（3 个 NPK）。幻鬼视觉最小集=必需 1 张即可成体。

## 5. 实现方案草案

**结构映射**：幻鬼= 前突穿透弹（Bullet，带幻鬼视觉）；交叉共鸣 = 玩家 50ms 前突 + 前方 4 单位处共鸣区（Area，兼幻鬼回斩视觉）。

### 内容件清单

1. **`DotNet~/Skills/SpiritCrossSlashSkill.cs : SkillLogic`**（ReleaseWaveSkill 位移范式 + BloodBoom SubState 编排）
   - `CooldownMs = 12000`；`TotalTimeMs = 1240`（段1 840 + 段2 400）。
   - `OnCast`（SubState=0）：`ctx.PlayAnim(AnimId.SwordmanSpiritCrossSlash1)` + `ctx.ClearHitTargets()` + `ctx.CreateBullet(BulletIds.SpiritCrossPhantom)`（幻鬼出发：出生=身前 0.8，Speed=4 单位/s，寿命 1000ms → 前突 4 单位=400px ✓）。
   - `OnUpdate`：
     - t≥840 且 SubState==0：`ctx.PlayAnim(AnimId.SwordmanSpiritCrossSlash2)`；`ctx.ClearHitTargets()`（段间重置，L19）；SubState=1。
     - SubState==1 且 t<890（50ms 窗口）：`ctx.MoveCasterForward(4 × dtMs/50)`（匀速前突 400px，纯函数增量，ReleaseWaveSkill 同构）。
     - t≥890 且 SubState==1：`ctx.CreateAreaInFront(AreaIds.SpiritCrossCross, (FP)4)`（共鸣区落在前突终点=幻鬼换位点）；SubState=2。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Bullets/SpiritCrossPhantom.cs : BulletDefinition`**（NormalWaveBullet 范式）
   - `Speed = 4`（400px/1000ms）、`TotalTimeMs = 1000`、`DestroyOnHit = false`（穿透）、`HalfExtents = (0.5, 0.3, 0.5)`（幻鬼斩击宽度，推断值）、`ViewAnimId = AnimId.SpiritCrossPhantomDash`（vsreadya_body 720ms，播完停末帧）；
   - `HitReaction { Damage = 90, HitstunMs = 500, KnockbackX = 0, LaunchY = 0 }`（意图 atk spiritcrossslash.atk：damage 反应无 push/lift；damage=level col0 demo 折算）；
   - `HitActions = { MeleeHit }`。
3. **`DotNet~/Areas/SpiritCrossCrossArea.cs : AreaDefinition`**（BloodBoomArea 范式）
   - `TotalTimeMs = 650`（共鸣斩短窗）、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (1.2, 0.5, 1.2)`（交叉斩范围无盒数据，按特效宽度推断）；
   - `HitReaction { Damage = 180, HitstunMs = 800, KnockbackX = 0, LaunchY = 360 }`（spiritcrossslashsecond.atk：down/lift360/push0——交叉共鸣的浮空击倒；damage=level col1 demo 折算）；
   - `ViewAnimId = AnimId.SpiritCrossPhantomSlash`（vsattacka_body 回斩视觉）+ 可选 `ViewEndAnimId = AnimId.SpiritCrossSpark`（crossspark 收尾）。
4. **无新增 Buff/Action**（两段伤害全走 MeleeHit + 各自 HitReaction）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 109 两子状态 | SubState 0/1 + TotalTimeMs 编排 |
| 幻鬼 PO 24349 id68 相位10（前突+col0 伤害） | `SpiritCrossPhantom` 弹（穿透+视觉+第一段伤害） |
| 相位11（反向 50ms 疾进+col1 伤害） | 玩家 `MoveCasterForward` 前突 + `SpiritCrossCrossArea`（伤害与回斩视觉合一） |
| 相位12（消散） | Area 视图播完自删（disappear 层可选） |
| BLADESPIRIT(123) 提速 | 精通系统缺失 → 固定 1.0 速度（§7） |
| static col1 特殊功能（跳过幻鬼突进） | 开关=0 关闭 + 需"场上幻鬼状态查询"（Buff 查询门面缺失）→ 不做 |
| als_ani 四层剑影特效 | 无 nut 驱动 overlay 通道 → v1 跳过/手组装（releasewave 先例，§7） |
| 可从攻击态(8)施放 | 技能取消体系缺失 → 仅站立可放（§7） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SpiritCrossSlash = 18` + `ButtonToSkill` case 10（新键，如逗号键） |
| BulletId | `Runtime\BulletDefinition.cs` | `BulletIds.SpiritCrossPhantom = 4` |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.SpiritCrossCross = 8` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanSpiritCrossSlash1 = 71`、`SwordmanSpiritCrossSlash2 = 72`、`SpiritCrossPhantomDash = 73`、`SpiritCrossPhantomSlash = 74`、`SpiritCrossSpark = 75`（可选） |
| json 注册 | `LSAnimClipRegistrar.cs` | `RegisterOne` ×4~5（玩家两段 + 幻鬼两段 [+spark]） |
| 图集 | `LSAnimResComponentSystem.cs` | `VengeanceSpirit.img.bytes`（必需；可选 +SpritCrossSlash/TeleportVS 系） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 12000 ms | 12000（直用） |
| 段1 时长 | spiritcrossslash1.ani 840ms（14×60） | 840 |
| 幻鬼前突 | 400px / 1000ms 匀速（static col0） | 弹 Speed 4 单位/s × 1000ms |
| 段1 伤害 | level col0 1626→13010 | 90（固定） |
| 段1 反应 | spiritcrossslash.atk：damage/cut+blood70/无 push-lift | Hitstun 500 / Kb 0 / Ly 0 |
| 玩家前突 | 400px / **50ms**（onProc uniform） | MoveCasterForward 4 单位 / 50ms |
| 段2 伤害 | level col1 3794→30357 | 180（固定） |
| 段2 反应 | second.atk：down / lift 360 / push 0 | Hitstun 800 / Ly 360 / Kb 0 |
| 总时长 | 840+400 = 1240ms | 1240 |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 玩家/幻鬼 .ani（spiritcrossslash1/2、vsreadya_body、vsattacka_body、vsattackb_body） | 常规节 + [SET FLAG]（约定跳过，帧号 const 进技能/弹配置） | **全部可被现有 ani 子命令翻译** |
| `vsreadya_body.ani.als` | **`[create draw only object]`**（无 "follow parent" 后缀的变体节） | 缺口累计已录该家族（R1-A4 里鬼为 `[create draw only object follow parent]`）——本例补记无后缀变体；建议 als 子命令按 [add] 同构支持（帧号+别名+可选层号） |
| 其余 .als（vsstarta_01/vsattackb1_00/vsattacka1_03/vsattackb1_00 等） | `[use animation]`/`[none effect add]` 已支持 | 无缺口 |
| `spiritcrossslash.atk` ×3 + `suddenstrikevs.atk` | `.atk` 无子命令 | 手抄（意图 atk 优先，§5 已选） |
| `swordman_shared.obj` | `.obj` 无子命令（etc motion/atk 长表） | 手工对位（本档 §2.3 已给 0 基索引表 + VS 族错位警告） |
| `spiritcrossslash.skl` | `.skl` 无子命令 | 手抄（2 列 + static 2 值） |

结论：.ani/.als 侧实质缺口仅 `[create draw only object]` 变体 1 条；另有 `.atk`/`.obj`/`.skl` 三类无子命令（族共性），计 4 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 幻鬼三相位连续体（一个 PO 依次换动画/换向/消散） | 弹只有单动画单方向；反向疾进无法表达 | 拆两件：前突弹（相位10）+ 共鸣区视觉（相位11/12 合并）；幻鬼"转身回斩"的连续感由 Area 视图接力（vsattacka_body），损失幻鬼回程的独立移动表现 |
| als_ani 四层剑影特效（nut 逐层时序驱动） | 无脚本驱动 overlay 通道（引擎特效无声明式来源，064 已录） | v1 跳过；还原时手组装 overlay（releasewave 8 层先例——`rw_burst_overlay` 同构可挂 spiritcrossslash2） |
| BLADESPIRIT(123) 动画提速（前置精通联动） | 被动/精通系统 + level data 查询缺失 | 固定 1.0（不做提速） |
| static col1 特殊功能（分离态立即交叉斩） | 需查询场上幻鬼对象状态（Buff 查询门面缺失） | 不做（本 pvf 开关亦为 0） |
| state 8/14 中断施放（攻击中/幻鬼态接技） | 技能取消体系缺失（R1-A3/064 已录） | 仅站立可放 |
| 屏震 5/200 | 延后 | 跳过 |
| 运行时 atk 错位加载 suddenstrikevs.atk（砸落控） | 本 pvf mod 数据问题（§2.3） | 采用意图 atk（spiritcrossslash.atk 平砍型）；如想要本 pvf 手感可换 down/lift-2000 参数 |
| cut+blood 70 出血表现 | 无出血数值通道（HitReaction 无出血参数，.atk [blood] 表现层） | 跳过（BleedBuff 不挂——DNF 本体也未挂 ACTIVESTATUS） |
| 等级缩放 | 延后 | 固定值 |

## 8. 存疑与缺口上报

- **未考证**：①玩家侧 first/second.atk 的接线（脚本未调用、.ani 无攻击盒——引擎按 etc attack info 槽位施加的通路不可见；demo 已按"幻鬼承担两段伤害"建模，与 level property 模板"突进斩击/交叉共鸣"两攻击力列吻合）；②state 14（executable states 第 3 项）具体状态；③vsattackb_body F9 flag 10002 之外 spiritcrossslash1.ani F13=10002 的消费者。
- **新缺口上报（F5 修正案，主循环请并入轮间经验）**：**unclebang `swordman_shared.obj` 的 [etc attack info] 在本 pvf 存在 VS 族（id 66-74）索引错位 -2**——运行时按 0 基直读（如 spiritcrossslash 实际加载 suddenstrikevs.atk），而 [etc motion] 0 基直读正常（#82/83/84 对位吻合、windspiritvs/moonspiritslash 三例交叉印证）。F5 原结论"[etc motion]/[etc attack info] 0 基索引"需补注：**motion 恒直读；atk 在本 pvf 的波动族（id≤17）直读、VS 族（id≥66）错位 -2（mod 插表所致），走读该族时按"code+2 = 名称匹配位"还原意图值**。
- **给下轮的经验**：ghostsword 族 VS 系技能（windspiritvs/spiritcrossslash/moonspiritslash/hellslash/ultimatecrossslash…）幻鬼全走共享 PO 24349 + id 66-74 分支——`setstate.nut` 按 id 找相位、`swordman_shared.obj` 行 22 起为 motion 0 基表（`行号-22=索引`）；玩家侧动画常量 300+ 段直接对 .chr 行 973 起的 etc motion（`行号-973=索引`）。level property 向量 **-2 = level info 列引用**（022 双列模板 + 本档 static 对位双印证，L21 补充）。
