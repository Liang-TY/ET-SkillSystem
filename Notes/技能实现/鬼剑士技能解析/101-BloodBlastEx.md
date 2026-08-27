# 血气爆发（BloodBlastEx）

> 技能ID 101 | 级别 E（**预分类纠偏：非 TP——[type] active、skill class 2，狂战士二觉替换型主动技**（怒气爆发的 70 级进化版）） | 可实现性 🔶（固定时长"双柱多段喷射 + 收尾爆"可按 024 双同心 Area Tick 同构直译；**按住持续喷射（channel 5s）与嗜血联动**撞按住输入+Buff 查询双缺口→固定档降级） | 分析日期 2026-08-22 | 批次 E7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 血气爆发 | `skill\Swordman\BloodBlastEx.skl` [name] |
| 英文名 | BloodBlastEx（取 skl 文件名；[name2] 实测 `Burst Fury`） | 同上 |
| 职业 | 狂战士二觉（[second growtype maximum level] 12 槽**第 6/7 位=30** = 狂战，R6-C4；[skill fitness growtype] 空） | 同上 |
| 学习等级 | 70（[required level range] 2）；前置 24 怒气爆发 Lv5 | 同上 |
| 最高等级 | 50（二觉档实际 30） | 同上 |
| 类型 | active（skill class 2）/ 物理 | 同上 |
| 指令 | →←↑→ + Z（MP 优惠 50%/50%） | 同上 |
| CD | 50000 ms（pvp 起手 600000） | 同上 |
| MP | 1000 → 2800 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×2 | 同上 [consume item] |
| 屏震 | [shake screen] 2 10000（延后） | 同上 |
| static data | `0 100 50 700 5000 250`——[3]=700 判定半径 px（与基础 static[3]=700 同值互证）、[4]=5000 喷射时间上限 ms、[5]=250 多段间隔 ms（**两向量实证 (4,4,0.001)/(5,5,0.001)**）；[0]=0/[1]=100/[2]=50 无向量引用（未考证） | 同上 + level property |
| 一句话效果 | 自身周围持续爆发血气喷射：物理伤害+浮空；按住技能键持续喷射（上限 5s），松开或到时停止；嗜血后施放可不按住自动持续；喷射位置血之狂暴吸 HP 量增加 | 同上 [explain] |

**level property 模板解码（4 列 + 5 向量，L21 法全解，Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 物理攻击力 | (-2,0,1.0) | col0 = 765 → 8658 |
| 冲击波攻击力 | (-2,2,1.0) | col2 = 2870 → 32463 |
| 多段攻击间隔 | **(5,5,0.001)** → static[5] | **0.25 s 恒定** |
| 每秒 HP 减少量 | (-1,3,1.0) | col3 = 100 → 1325（**施法者自耗**——血气系 HP 代价） |
| 喷射时间上限 | **(4,4,0.001)** → static[4] | **5.0 s 恒定** |

pvp 表同构减档（col0=150→1998 等）。

**与基础 024 怒气爆发对照**：4 段/3 秒雨（static `4 450 450 700 1`）→ **channel 5s/0.25s 间隔（20 跳）+ 每秒 HP 自耗 + 嗜血/血之狂暴双联动**；先手浮空 lift 400→**100**（PreSub.atk 实测）；伤害结构 col0 主柱 + col2 冲击波（基础 3 列语义推断表就此获得同族对拍）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（纯引擎内置，F3——024 同型）

- load_state 无注册（grep blastbloodex/bloodblastex 无命中）；`sqr\character\swordman\` 无本技 nut/appendage；无 .chr/角色动画条目（024 同款"角色本体无动作、PO 层承担表现"）。
- 被动对象 ×3（passiveobject.lst 实测 11334-11339 行）：**PO 20103 = BlastBloodEx.obj**（主血柱）、**PO 20104 = BlastBloodSubEx.obj**（隐藏第二判定）、**PO 20105 = BlastBloodPreSubEx.obj**（前段）。
- ⚠ 勘误回填 024：024 §2.1 记"20102/20103/20104 → BlastBloodEx/SubEx/PreSubEx"——实测 **20101=FireWaveBigEx、20102=FireWaveBigSubEx、20103/20104/20105 才是 BlastBloodEx 三件**（偏移 1 位）。

### 2.2 引擎内置行为重建（数据反推，推断标注）

```
按下技能键 → 创建 PO 20105（前段，lift 100 先手浮空——基础版 lift 400 减半）
  → 主喷射：PO 20103（BlastBlood1Ex.ani 2200ms，F11-F14 攻击盒）+ PO 20104（Sub，复用基础动画，F1-F3 盒）
  按住期间：引擎循环 PO 动画（2200/2160ms 相位）并以 static[5]=250ms 对范围内敌人多段结算
    col0%（主柱）/中心双份（主+Sub 叠加，024"中心 8 段"同构）+ 冲击波 col2%
    施法者每秒 HP -col3（100~1325）
  松开 / 5s 上限 → BlastBloodExEnd.ani（1200ms 收尾爆，etc 层挂 end_blood/small_blood_end）
嗜血(23) Buff 在身：施放后不按住也持续喷射（引擎查询）
喷射位置：血之狂暴(76) 吸 HP 量增加的光环（引擎内置）
```

### 2.3 被动对象（三个 .obj 完整实测）

| .obj（PO ID） | 结构与动画 | .atk 关键值 |
|---|---|---|
| **20103 BlastBloodEx**（主血柱） | basic `BlastBlood1Ex.ani`（15 帧 2200ms，**F11-F14 攻击盒** `-104 -77 0` + `203 68 249` → x[-1.04,0.99] z[0,2.49] 宽柱——基础版盒 x[-0.41,0.39] z[0,3.3] 窄高柱，Ex 更宽扁）；etc：**复用基础** BlastBlood2/Floor1/Floor2.ani + **新增** BlastBloodEx/sub.ani（18 帧 1800ms，有自有图 sub.img——基础 Sub 无图）+ sub_dodge1/2 + BlastBloodExEnd.ani（12 帧 1200ms 收尾） | `BlastBloodEx.atk`：physic / **down** / push 0 / lift 0 / hit lift up / blow（基础主爆同参数族） |
| **20104 BlastBloodSubEx**（隐藏第二判定） | basic **复用基础** `BlastBloodSub.ani`（5 帧 2160ms，F0 停 1120ms → F1-F3 盒 `-73 -50 0`+`146 100 330`） | `BlastBloodSubEx.atk`：physic / down / 0 / 0（同基础） |
| **20105 BlastBloodPreSubEx**（前段） | basic/etc **复用基础** `BlastBloodPreSub_back/front.ani`（400ms，渐大爆圈盒） | `BlastBloodPreSubEx.atk`：physic / down / push 0 / **lift 100**（基础 PreSub lift 400——先手浮空减半） |

三 .obj 均 pass all / piercing 1000。

### 2.4 动画关键帧表（Ex 新增件实测；复用件见 024 §2.4）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img |
|---|---|---|---|---|---|
| BlastBlood1Ex.ani（主柱判定） | 15 | 2200ms | 无 | **F11-F14**（§2.3 宽柱盒；F14 delay 800） | **无 img**（视觉全在 .als） |
| BlastBlood1Ex.ani.als | — | — | — | — | 挂 BlastBloodEx/ex_blood1-8（八血柱）+ blood_floor 三层 + sub（12 项 [use animation]） |
| BlastBloodEx/sub.ani（隐藏柱视觉） | 18 | 1800ms | 无 | 无 | `Effect/BlastBloodEx/sub.img`（基础 Sub 无图的升级） |
| BlastBloodExEnd.ani（收尾爆） | 12 | 1200ms | 无 | 无 | blood-d1.img + .als 挂 end_blood_floor/end_blood/small_blood_end1-3 |
| blastbloodex\ 目录 | ex_blood1-8、small_blood_end1-3、blood_floor×3、end_blood×2、sub_dodge1/2 等 18 文件 | — | — | — | 八柱/收尾视觉全套 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodBlastEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodBlastEx.skl` | ✅（229 行） | 4 列全解 |
| 注册行 | —（无 pushState） | `…\sqr\character\swordman_load_state.nut` | ⛔ 无 | 引擎内置（F3） |
| 主 nut / appendage | —（不存在；appendage 目录 grep 0 命中） | `…\sqr\character\swordman\` | ⛔ 无 | 逻辑在引擎 |
| PO 注册 | passiveobject.lst:11334-11339 | `…\passiveobject\passiveobject.lst` | ✅ 实测 | 20103/20104/20105 |
| PO 定义 | BlastBloodEx.obj / BlastBloodSubEx.obj / BlastBloodPreSubEx.obj | `…\passiveobject\character\swordman\` | ✅ 实测 | §2.3 |
| PO .atk | blastbloodex.atk / blastbloodsubex.atk / blastbloodpresubex.atk | `…\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | §2.3 |
| PO .ani/.als | BlastBlood1Ex.ani(+.als)、BlastBloodExEnd.ani(+.als)、blastbloodex\ ×18、复用基础 BlastBloodSub/PreSub 系 | `…\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 角色 .ani/.atk/.chr | —（无，PO 层承担��� | `…\character\swordman\` | ⛔ | 024 同款 |
| 基础技文档 | 024-BloodBlast.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 结构对照基准 |
| 关联 TP | BloodBlastExp.skl（166，E6 批） | `…\skill\Swordman\` | ✅ 存在 | 见 §8 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `…/Effect/BlastBloodEx/ex_blood1-8.img`（八血柱） | sprite_character_swordman_effect_blastbloodex.NPK | 主柱 .als 八层（ex_blood*.ani 逐柱引用） | **必需**（8 张） | ❌ |
| `…/BlastBloodEx/blood_floor.img`、`blood_floor_back.img`、`blood_floor_front.img` | 同上 | 地面血泊三层 | **必需** | ❌ |
| `…/BlastBloodEx/sub.img` | 同上 | 隐藏柱视觉（本技新增） | **必需** | ❌ |
| `…/BlastBloodEx/end_blood.img`、`small_blood.img` | 同上 | 收尾爆/小血花 | 可选 | ❌ |
| `…/BlastBloodEx/blood_red_back.img`、`blood_red_front.img`、`sub_dodge.img` | 同上 | .als 叠层 | 可选 | ❌ |
| （复用 024 必需）BlastBlood/b-01、b-02-d、blood 系 | sprite_character_swordman_effect_blastblood.NPK | 前段姿势层/基础柱复用件 | 必需（共享，024 已列） | ❌ |
| sm_body0000.img | （已入库） | —（角色无动作） | — | ✅ |

**缺失 img：本技新增必需 11 张、可选 5 张——全部同一个新 NPK（effect_blastbloodex）一次提取；另共享 024 的 effect_blastblood NPK。**

## 5. 实现方案草案（号段：SkillIds 38 / AnimIds 197-199 / AreaIds 46-48，E7 批内顺延）

### 内容件清单（固定时长版——channel 降级）

1. **`DotNet~/Skills/BurstFurySkill.cs : SkillLogic`**（024 BloodBlastSkill 直改 + HP 自耗）：
   - `CooldownMs=50000`；`TotalTimeMs=5600`（前段 400 + 喷射 5000 + 收尾 200——固定 5s 满档，§7）。
   - `OnCast`：`ctx.ClearHitTargets()` + 三区同帧：`ctx.CreateArea(AreaIds.BurstFuryPre, 自身)` + `ctx.CreateArea(AreaIds.BurstFury, 自身)` + `ctx.CreateArea(AreaIds.BurstFuryCore, 自身)`（024 三区同构）；动画复用 `AnimId.SwordmanAttack2` 近似（024 同款，角色无专属动作）。
   - `OnUpdate`：喷射期每 1000ms `ctx.ConsumeCasterHp(20)`（col3 100/秒 demo 折算；BloodBoom 的 MinCastHpPct/ConsumeCasterHp 先例——**按秒分期扣**是本技新用法）；t≥5400 → `ctx.CreateArea(AreaIds.BurstFuryEnd, 自身)`（收尾爆）。
2. **`BurstFuryPreArea : AreaDefinition`**（前段）：`TotalTimeMs=400`、`EnterActions={MeleeHit}`、`HalfExtents=(2.0,0.5,1.0)`、`HitReaction{Damage=50, HitstunMs=400, KnockbackX=0, LaunchY=100}`（**PreSubEx.atk lift 100 直用**——Ex 减半值）、`ViewAnimId=AnimId.BlastBloodPre`（复用 024）。
3. **`BurstFuryArea : AreaDefinition`**（主柱多段）：`TotalTimeMs=5000`、**`TickTimeMs=250`**（static[5]）、`TickActions={MeleeHit}`、`HalfExtents=(3.5,1.0,1.25)`（主柱宽盒 F11 x[-1.04,0.99] + static[3]=700px 半径折中，024 同参）、`HitReaction{Damage=30, HitstunMs=500, KnockbackX=0, LaunchY=0}`、`ViewAnimId=AnimId.BurstFuryMain`（BlastBlood1Ex json，视觉层由 .als 翻译产物叠加）。
4. **`BurstFuryCoreArea : AreaDefinition`**（中心双份/冲击波列）：同参同心小盒 `HalfExtents=(1.2,0.5,1.25)`（Sub 盒）、`TickActions={MeleeHit}`、`HitReaction{Damage=55, ...}`（col2 冲击波 2870% 折算并入中心双份——外 20 跳/中心 40 跳近似）。
5. **`BurstFuryEndArea : AreaDefinition`**（收尾）：`TotalTimeMs=1200`、`EnterActions={MeleeHit}`、`HalfExtents=(3.0,0.8,1.5)`、`HitReaction{Damage=120, HitstunMs=700, KnockbackX=0, LaunchY=0}`、`ViewAnimId=AnimId.BurstFuryEnd`。
6. **嗜血/血之狂暴联动不做**（§7）；HP 自耗可用 `MinCastHpPct` 保底（血气系惯例）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| channel 按住 5s（松开即停） | **固定 5s 满档**（按住输入缺失，R3-A15 族第 9 例）；按住落地后改 OnUpdate 读输入截断 |
| 每秒 HP 自耗 col3 | ConsumeCasterHp 按 1s 分期（BloodBoom 先例的时间维扩展） |
| 主柱/Sub 双判定体 + 250ms 多段 | 双同心 Area Tick（024 直构，L19"同段定时"档） |
| 先手浮空 lift 100 | PreArea.LaunchY=100 直译 |
| 嗜血(23) 后自动持续喷射 | Buff 查询门面（R4-B18）+ 按住双撞 → 不做 |
| 喷射位置血之狂暴吸 HP 增益光环 | 属性消费链（R1-A4）+ 阵营/光环 → 不做 |
| 冲击波 col2 | 并入中心区 Damage（双列三区结构无法四区，取近似） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.BurstFury = 38` + 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `BurstFuryPre/Core/End = 46/47/48`（主区复用 024 BloodBlast 参数亦可） |
| AnimId | `AnimConfigRegistry.cs` | `BurstFuryMain = 197`、`BurstFuryEnd = 198`、`BurstFurySub = 199`（可选） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×2-3；effect_blastbloodex 图集 1 个（11 张） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 50000 ms | 50000 直用 |
| 喷射时长 | 按住 0~5000ms（channel） | 固定 5000 |
| 多段间隔 | static[5] = 250 ms | Tick 250（20 跳） |
| 主柱每跳 | col0 765%→8658% | 30 |
| 中心双份 | 主+Sub 叠加（024 同构）+col2 冲击波 2870% | 内圈 55 |
| 先手浮空 | PreSubEx.atk lift **100**（基础 400） | LaunchY 100 |
| 每秒 HP 自耗 | col3 100→1325 | 20/s × 5s |
| 收尾爆 | BlastBloodExEnd 1200ms（无独立 atk——复用主 .atk 族，推断） | Damage 120 |
| 判定半径 | static[3]=700 px | 外圈半 3.5 单位 |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| BloodBlastEx.skl | `.skl` 无子命令 | 手抄（4 列全解） |
| BlastBloodEx/SubEx/PreSubEx.obj ×3 | `.obj` 无子命令（复用件+新增件混排的 etc 表） | 手工映射四 Area（§2.3 已给） |
| blastbloodex/subex/presubex.atk ×3 | `.atk` 无子命令 | 手抄各 ~7 值 |
| BlastBlood1Ex.ani.als / BlastBloodExEnd.ani.als | [use animation]×12 / ×5（大体量但结构常规） | als 子命令全覆盖（024 同族先例） |
| BlastBlood1Ex.ani | 无 img 帧（判定载体，L7 族）+ F14 800ms 长帧（非 10s 级，不需钳制） | 直译无碍 |

本技能翻译缺口 3 类（.skl/.obj/.atk）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| **按住持续喷射/松开停止（channel）** | 按住输入缺失（R3-A15 族第 9 例；009/97 同族） | 固定 5s 满档（损失"松开提前停"的操作节奏；CD 50s 下影响可控） |
| 嗜血(23) Buff→自动持续喷射 | Buff 查询门面（R4-B18）+ 按住双撞 | 不做（联动收益归零） |
| 喷射位置血之狂暴吸 HP 增益（光环） | 属性消费链（R1-A4）+ 阵营判定（R1-A3） | 不做 |
| 每秒 HP 自耗 | ConsumeCasterHp 已有（BloodBoom）但为一次性语义 | OnUpdate 按 1s 分期调用（零新机制，时间维用法） |
| 多段间浮空维持（hit lift up） | 024 同款（首段抬起后落地） | 可选每跳 LaunchY 150 小抬（LSFlight 重打刷新） |
| 冲击波独立列 col2 | 双同心区已占 | 并入中心区数值（近似） |

## 8. 存疑与缺口上报

- **未考证**：①static[0]=0/[1]=100/[2]=50 语义（疑光环参数/HP 吸收增量）；②喷射期 PO 动画的循环规则（2200/2160ms 相位 vs 250ms 结算的配合——引擎内部）；③收尾爆有无独立 atk（obj 未列，疑复用主 .atk）；④col2"冲击波"的判定载体（主柱？独立波？无第三 PO——疑 Sub 柱即"冲击波"载体，两义并读存疑）；⑤嗜血联动窗口。
- **新旧 TP 并存关系结论（本批专项④）**：**101 与 166（BloodBlastExp，E6 批）不是两代 TP**——101 是 [type] active 二觉替换主动技（前置 24 Lv5、二觉 30 档）；怒气爆发的 TP 是 166。**铁证：基础技 BloodBlast.skl [feature skill index] = 166**。
- **给 024 的回填（勘误）**：024 §2.1 的 PO 号段表偏移 1 位——实测 **20103=BlastBloodEx、20104=BlastBloodSubEx、20105=BlastBloodPreSubEx**（20101/20102 属 FireWaveEx）；024 引用的"TP 强化版（E 批课）"实为本技（二觉替换技）资产；024 §1 三列语义推断表（col0 每段/col1 间隔缩短/col2 中心）可借本技 4 列表交叉印证——col1"间隔 500→143 递减"与本技恒定 250ms 不同构，024 推断维持待考。
