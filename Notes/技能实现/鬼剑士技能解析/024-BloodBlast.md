# 怒气爆发（BloodBlast）

> 技能ID 24 | 级别 A | 可实现性 ✅（多段用 Area Tick 直接表达，无需新机制） | 分析日期 2026-08-22 | 批次 A8

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 怒气爆发 | `skill\Swordman\BloodBlast.skl [name]` |
| 英文名 | BloodBlast（skl 文件名；[name2]=`Raising Fury` 为官方英文名，本表按惯例取文件名） | 同上实测 |
| 职业 | 狂战士（[skill fitness growtype]=3；血气系） | 同上 |
| 学习等级 | 30 | 同上 [required level] |
| 最高等级 | 70（各觉醒段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 2） | 同上 [type] |
| 指令 | ↓↑ + Z（MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 16000 ms（pvp 起始 CD 15000） | 同上 [dungeon][cool time] / [pvp][start cool time] |
| MP | 110 → 924（Lv1 → Lv70） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无色小晶块 ×1（[consume item] 3037） | 同上 |
| 前置 | 技能 76（Frenzy 血之狂暴/暴走）Lv3 | 同上 [pre required skill] |
| 屏震 | [shake screen] 2 10000 | 同上 |
| static data | `4 450 450 700 1`（**无 nut 消费者，语义未考证**；推断：4=段数、450/450=多段间隔 ms、700=判定半径 px、1=中心双倍段标志） | 同上（推断标注） |
| 一句话效果 | 向自身周围爆发怒气：敌人受物理伤害并浮空；爆发产生冲击波 4 段攻击，中心敌人可受 8 段 | 同上 [explain] |

**level property（3 列，Lv1 → Lv70）**：`645→5259`、`500→143`（递减）、`861→7014`。
列语义未考证（引擎内置消费，L21 无法适用）；推断：col0=每段攻击力 ‰、col1=多段间隔（随级缩短 500→143ms）��col2=中心段攻击力或总量。

**重要澄清（L2）**：load_state 里 `wave.nut "WaveSword" 24 -1` 的 24 是**状态号**（波动剑系共用状态），与技能 ID 24（本技能）纯属巧合——
本技能与波动剑系无关（实测 wave.nut 无 bloodblast 分支）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能无任何 pushState 注册**（load_state 全文件 grep `bloodblast` 无命中）、**无 nut 目录**（`sqr\character\swordman\` 全树 grep 无命中）、
**角色 .chr / animation / attackinfo 无 blastblood 条目**（三处 grep 实测，attackinfo 目录仅 grabblastblood 系=技能 31 嗜魂封魔斩）。
→ **全引擎内置（F3 形态）**：施法侧时序在客户端引擎内，pvf 只留数据文件。角色表现也由 PO 层承担（见 §2.3，老技能形态）。

数据侧文件链（全部实测存在）：

```
passiveobject.lst（定点读取）:
  20009 → Character/Swordman/BlastBlood.obj        （怒气爆发 主要）
  20010 → Character/Swordman/BlastBloodSub.obj     （怒气爆发 隐藏）
  20031 → Character/Swordman/BlastBloodPreSub.obj  （怒气爆发 隐藏·前段）
  20120/20121/20122 → BlastBloodOrigin/PreSubOrigin/SubOrigin.obj（变体，未细读）
  20102/20103/20104 → BlastBloodEx/SubEx/PreSubEx.obj（TP 强化版，E 批课）
```

### 2.2 施法时序（引擎内置，数据反推——050-GrandWave 反推法）

由 PO 动画/atk 数据侧反推（无脚本佐证，推断标注）：

```
onSetState：扣 MP（+无色×1）→ 自身中心创建 BlastBloodPreSub（前段，400ms，lift 400 先手浮空）
  → 血柱升起：BlastBlood（主，2200ms）+ BlastBloodSub（隐藏第二判定体，2160ms）同期运作
  → 引擎按 static 间隔定时多段结算（4 段；中心双倍 → 8 段）
  → 施法者本体：无独立角色动画（.chr 无条目），姿势由 PreSub 的 b-01/b-02-d 贴图层绘制
收尾：PO 播完自毁；角色回待机
```

### 2.3 被动对象（三件 .obj 实测）

| .obj（PO ID） | 结构 | 动画/判定 | 攻击信息 |
|---|---|---|---|
| **BlastBloodPreSub**（20031，"怒气爆发 隐藏"） | basic=presub_back.ani + etc motion presub_front.ani | presub_back 7f/400ms，攻击盒 **F1@100-F5@350**（渐大：`-42 -18 -22 82 36 38` → `-127 -24 -64 254 48 116`，min/max 口径——地面爆发圈） | **BlastBloodPreSub.atk：down 反应 / push 0 / lift 400**（先手浮空） |
| **BlastBlood**（20009，"怒气爆发 主要"） | basic=BlastBlood1.ani + etc motion：BlastBlood2/3/Floor1/Floor2/Floor3（**etc 无 attack info 配对 → 纯视觉层**，L13） | BlastBlood1 15f/2200ms，攻击盒 **F10@1000-F13@1400**：`-41 -77 0 80 54 330`（血柱：x半 0.6、z 高 3.3 单位） | BlastBlood.atk：**down / push 0 / lift 0** / hit lift up / blow（多段原地连打不推不浮） |
| **BlastBloodSub**（20010，"怒气爆发 隐藏"） | basic=BlastBloodSub.ani（无 etc） | 5f/2160ms（F0 停留 1120ms 后爆发），攻击盒 **F1@1120-F3@1360**：`-73 -50 0 146 100 330`（更宽血柱） | BlastBloodSub.atk：down / push 0 / lift 0（同主爆） |

三 .obj 均 pass all / piercing 1000（全穿透多目标）。

**多段机制**：explain 明言 4 段（外圈）/8 段（中心）；static[0]=4 与间隔 450/450 为仅有的数据线索（引擎定时结算，无脚本）。
中心 8 段的构成未考证——推断为中心区域被主+隐藏两个判定体（或双份计时）同时覆盖，叠加成倍。

**视觉分层**：BlastBlood1.ani.als 用标准 `[use animation]`×8 + `[add]`（blood1~blood8 血柱分别挂 F0/层 18~13）；
blastbloodhit1.ani.als 挂 BlastBloodHit2（命中火花）；floor1-3 为地面扩散层。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| presub_back.ani | 7 | 400ms | — | F1@100-F5@350（渐大爆圈） | 地面前段 |
| presub_front.ani | 7 | 400ms | — | 无 | 前景纯视觉 |
| BlastBlood1.ani | 15 | 2200ms | — | F10@1000-F13@1400（血柱盒） | .als 血柱 8 层 |
| BlastBloodSub.ani | 5 | 2160ms | — | F1@1120-F3@1360（宽血柱盒） | 隐藏第二判定 |
| BlastBlood2/3.ani | 15/18 | 1600/1900ms | — | 无 | 主 .obj etc 视觉层 |
| BlastBloodFloor1~3.ani | — | — | — | 无 | 地面扩散视觉层 |
| BlastBloodHit1/2.ani | — | — | — | 无 | 命中火花（.als 挂接） |
| BlastBlood/ 子目录（blood1~8.ani、blood_floor_*.ani、floor_over.ani 等 14 个） | — | — | — | — | .als 引用的血柱/地面单元 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodBlast.skl（253 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodBlast.skl` | ✅ | 技能数据（3 列/static 5 值/无色消耗） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | 见 §2.1 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（全树 grep 实测无） | ⛔ 缺失 | 逻辑在引擎 |
| PO 定义 | BlastBlood.obj / BlastBloodSub.obj / BlastBloodPreSub.obj | `…\pvf\passiveobject\character\swordman\` | ✅ | 三件判定体结构 |
| PO 注册 | passiveobject.lst:11152/11154/11196 区段（ID 20009/20010/20031 实测对表） | `…\pvf\passiveobject\passiveobject.lst` | ✅ | PO ID → .obj 路径 |
| PO .atk | BlastBlood.atk / BlastBloodSub.atk / BlastBloodPreSub.atk | `…\passiveobject\character\swordman\attackinfo\` | ✅ | 三段命中反应 |
| PO .ani | blastblood1/2/3/sub/presub_back/presub_front/floor1-3/hit1-2.ani + BlastBlood\ 子目录 | `…\passiveobject\character\swordman\animation\` | ✅ | 血柱全套 |
| .als | blastblood1.ani.als、blastbloodhit1.ani.als、blastbloodhit1_ds.ani.als（+pvp 变体） | 同上 | ✅ | 血柱 8 层挂接/命中火花 |
| 角色 .ani/.atk/.chr | —（无） | `…\character\swordman\`（三处 grep 实测无 blastblood 条目） | ⛔ 缺失 | 施法表现由 PO 层 b-01/b-02-d 姿势层承担 |
| 装备层 | —（0 个） | `…\equipment\character\swordman\avatar\`（find 实测 0） | ⛔ 无 | 印证"角色本体无动作" |
| 关联 | Frenzy.skl（技能 76 前置）、BlastBloodEx 系（20102-20104，TP 强化） | `…\skill\Swordman\` | ✅ 存在 | 前置/强化（另行分析） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （角色动画） | — | — | — | 角色本体无动作（PO 姿势层替代），无 sm_body 新帧需求 |
| BlastBlood/b-01.img、b-02-d.img | sprite_character_swordman_effect_blastblood.NPK | PreSub 施法者姿势层（前/背） | **必需**（无它则施法者无表现） | ❌ |
| BlastBlood/blood-b.img、blood-back.img、blood-d1.img、blood-d2.img、bloodred.img | 同上 | 血柱主体（presub/sub/主爆引用） | **必需** | ❌ |
| BlastBlood/blood-front.img、blood_floor.img、blood_floor_back/_dodge/_front.img、BlastBloodHit.img | 同上 | 前景血柱/地面扩散/命中火花 | 可选 | ❌ |

缺失 img：**必需级 7 张**（同一 NPK `sprite_character_swordman_effect_blastblood` 一次提取全覆盖）、可选级 6 张。

## 5. 实现方案草案

- **内容件清单**（全部现有机制；BloodBoomSkill/AREA 同族先例直接套）：
  - `BloodBlastSkill : SkillLogic`（SkillIds.BloodBlast=17）：
    - `CooldownMs=16000`（demo 可缩 8000）；`TotalTimeMs=400`（角色侧无动作——只做前段姿势触发后立即交棒给 Area，技能本体极短）。
    - `OnCast`：`ctx.ClearHitTargets()`；动画：无 DNF 施法动画 → 复用现有 `AnimId.SwordmanAttack2`（近身挥击姿势近似，或干脆不切动画）；
      创建三个区域：`ctx.CreateArea(AreaIds.BloodBlastPre, 自身位置)` + `ctx.CreateArea(AreaIds.BloodBlast, 自身位置)` + `ctx.CreateArea(AreaIds.BloodBlastCore, 自身位置)`（外圈+内圈同心）。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - `BloodBlastPreArea : AreaDefinition`（前段，BlastBloodPreSub 同构）：`TotalTimeMs=400`、`TickTimeMs=0`、
    `EnterActions={MeleeHit}`、`HalfExtents=(2.0,0.5,1.0)`（presub F5 盒 x[-127,254]/z[-64,116] 折算）、
    `HitReaction{Damage=60, HitstunMs=500, KnockbackX=0, LaunchY=400}`（PreSub.atk lift400 直用——先手浮空）、
    `ViewAnimId=AnimId.BlastBloodPre`（presub 视觉+b-01 姿势层）。
  - `BloodBlastArea : AreaDefinition`（外圈 4 段，BlastBoomArea 范式 + Tick）：
    `TotalTimeMs=1800`、**`TickTimeMs=450`**（static[1] 推断值直用）、`TickActions={MeleeHit}`、
    `HalfExtents=(3.5,1.0,1.65)`（主血柱盒 + static[3]=700px 半径推断折中）、
    `HitReaction{Damage=80, HitstunMs=500, KnockbackX=0, LaunchY=0}`（主 .atk：0 推 0 浮，down 靠硬直表现）、
    `ViewAnimId=AnimId.BlastBlood1`（血柱）、`ViewEndAnimId` 可选。
  - `BloodBlastCoreArea : AreaDefinition`（内圈 4 段，中心双倍的关键）：参数同外圈、`HalfExtents=(1.2,0.5,1.65)`（Sub 盒宽柱缩小）、
    伤害略高（col2 口径）。**中心单位同时在外圈+内圈 → 每 Tick 双份命中 = 外 4 段/中心 8 段**（Area Tick 无命中去重，LSAreaSystem.LSUpdate 实测：TickActions 对 InsideUnits 每 Tick 全量重跑）。
  - **无需**新 Buff/Action/Bullet。
- **概念映射**：引擎内置多段计时 → 双同心 Area 的 `TickTimeMs`；BlastBloodPreSub 先手浮空 → 短命 PreArea 的 LaunchY=400；
  "中心 8 段" → 内外双 Area 叠加命中；血柱视觉 → ViewAnimId 三层；MP/无色消耗 → 跳过（延后档）。
- **注册点**：SkillIds.BloodBlast=17 + ButtonToSkill 新键；AnimIds `BlastBloodPre=68、BlastBlood1=69、BlastBloodCore=70`（hit/floor 可选不占号）；
  LSAnimClipRegistrar ×3；BuildAtlas 加 `sprite_character_swordman_effect_blastblood` 系图集；LSOperaComponentSystem 新键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 16000ms | 8000（手感演示） |
| 前段浮空 | PreSub.atk：lift 400 / push 0 | LaunchY 400 直用 |
| 外圈段数/间隔 | static[0]=4 段、间隔 450ms（推断） | TickTimeMs 450 × TotalTime 1800 |
| 中心段数 | 8 段（explain） | 内外双 Area 同参叠加 |
| 每段伤害 | col0 645‰（推断=每段） | 外圈 80、内圈 90 |
| 多段间隔成长 | col1 500→143ms（推断） | 固定 450 |
| 判定半径 | static[3]=700px（推断） | 外圈半 3.5 单位、内圈半 1.2 单位 |
| 段间反应 | down/push0/lift0（原地连打） | Hitstun 500 + LaunchY 0（第 2 段起可选 LaunchY 150 维持浮空，见 §7） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BloodBlast.skl | `.skl` 无子命令（3 列 + 5 值 static + [consume item]/[shake screen] 节） | 手抄可行；skl 子命令时把 consume item 一并纳入（无色消耗族技能多） |
| BlastBlood*.atk ×3 | `.atk` 无子命令（本组结构简单：down/0/lift 400） | 手抄 |
| BlastBlood*.obj ×3 | `.obj` 无子命令（etc 无 atk 配对=纯视觉层，L13 判据本组再实证） | 手工映射（§5 已给） |
| blastblood1.ani.als / hit1.ani.als | 全部为已支持节（[use animation]/[add]） | 无缺口 |
| blastblood*.ani `[SET FLAG]` | 无（本组动画无 flag） | — |

结论：.ani/.als 全部可被现有子命令翻译；实质缺口 = `.skl`/`.atk`/`.obj` 三子命令，计 3 条（常驻）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 多段间敌人保持浮空连打（hit lift up + down 反应的 juggle 语义） | 我们的 Tick 命中 LaunchY=0 不会刷新浮空——首段 PreSub 抬起后中途会落地 | 可选：每段 LaunchY 150 反复小抬（LSFlight 重打刷新语义支持）；或接受落地连打（手感差异：敌人贴地受身） |
| 血气狂暴（Frenzy 76）状态下的增伤/HP 消耗替代（DNF 血气系惯例，本 skl 未列 HP 消耗，联动未考证） | Buff 查询门面 + 属性数值伤害消费链（R1-A4 已上报最重缺口） | 跳过（基础版 MP 消耗本就延后） |
| 无色小晶块消耗 | 道具系统缺失 | 跳过 |
| 屏震 [shake screen] 2 10000 | 屏震延后 | 跳过 |
| 中心 8 段的精确构成（主+隐藏判定体叠加 or 引擎双计时） | 引擎内置不可考 | 双同心 Area 叠加（数学等效：4+4=8） |
| 血柱随等级缩放/间隔随级缩短 | 对象整体缩放延后 + 等级缩放延后 | 固定值 |
| BlastBloodOrigin/PreSubOrigin/SubOrigin 变体（20120-20122） | 用途未考证（疑血魔变身/觉醒变体） | 不做；留 E/觉醒批次考证 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 5 值与 3 列 level property 的精确语义（引擎内置消费，全部标推断）。
2. 中心 8 段的引擎实现构成（§7 已给等效方案）。
3. BlastBloodOrigin 系（20120-20122）与 `_ds` 变体的触发条件。
4. 血气狂暴（76）习得后是否有 HP 替代消耗/增伤（DNF 惯例有，本 pvf 未证）。

**新系统级缺口（§6.3 清单外）**
- 本技能无新缺口（多段落地方式 = Area Tick 无去重实测成立，反例修正了"多段命中=延后"的一刀切认知：**区域型同段定时多段我们已可表达**——建议主循环把 §6.3"延后"档的"多段命中（HitTargets 重置）"细分为"投射物同段重置=延后 / 区域 Tick=已有"，与 L19 的"段间已落地"并列记档）。

**翻译工具缺口（并入主循环汇总）**：`.skl`/`.atk`/`.obj` 三子命令（常驻 3 条，无新增节缺口）。
