# 极炎 · 裂波剑（FireWaveEx）

> 技能ID 99 | 级别 E（**预分类纠偏：非 TP——[type] active、skill class 1，阿修罗二觉替换型主动技**（爆炎·波动剑的 70 级进化版）） | 可实现性 ✅（022 同族扩展：主波 Area + 火焰地带 Tick Area（FireCircle 范式）+ 爆炸 Area 三段时序全可表达；灼伤 = BurnBuff 复制 BleedBuff；stuck 吸附并入长硬直——022 同款处理） | 分析日期 2026-08-22 | 批次 E7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极炎 · 裂波剑 | `skill\Swordman\FireWaveEx.skl` [name] |
| 英文名 | FireWaveEx（取 skl 文件名；[name2] 实测 `Super Blust Fire Wave Sword`） | 同上 |
| 职业 | 阿修罗二觉（[second growtype maximum level] 12 槽**第 8/9 位=30** = 阿修罗，R6-C4；[skill fitness growtype] 空） | 同上 |
| 学习等级 | 70（[required level range] 2）；前置 22 爆炎·波动剑 Lv1 | 同上 |
| 最高等级 | 50（二觉档实际 30） | 同上 |
| 类型 | active（skill class 1）/ 魔法 | 同上 |
| 指令 | →←↑→ + Z（MP 优惠 50%/50%） | 同上 |
| CD | 50000 ms（pvp 起手 600000） | 同上 |
| MP | 800 → 1680 | 同上 [consume MP] |
| 读条 | 400 ms | 同上 [casting time] |
| 特殊消耗 | 无色小晶块 ×2 | 同上 [consume item] |
| 屏震 | [shake screen] 2 400（延后） | 同上 |
| static data | `500 4000 300 100 250`——[1]=4000 火焰地带持续 ms、[2]=300 地带多段间隔 ms（**两向量实证 (1,1,0.001)/(2,2,0.001)**）；[0]=500/[3]=100/[4]=250 无向量引用（疑弹速/bead 参数，未考证） | 同上 + level property |
| 一句话效果 | 挥剑生成前方火焰地带：范围内多段火伤 → 剧烈爆炸，概率灼伤；施放时可生成波动印 | 同上 [explain] |

**level property 模板解码（11 列 + 10 向量，L21 法；Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 魔法攻击力 | (-2,0,1.0) | col0 = 5166 → 58432 |
| 爆炸魔法攻击力 | (-2,1,1.0) | col1 = 8607 → 97386 |
| 火焰地带攻击力 | (-2,2,1.0) | col2 = 1476 → 16695 |
| 火焰地带持续时间 | **(1,1,0.001)** → static[1] | **4.0 s 恒定** |
| 火焰地带多段攻击间隔 | **(2,2,0.001)** → static[2] | **0.3 s 恒定** |
| 灼伤机率 | (-1,3,0.1) | col3 = 1000 → **100% 恒定** |
| 灼伤Lv | (-1,4,1.0) | col4 = Lv72 → Lv170 |
| 灼伤持续时间 | (-1,5,0.001) | col5 = 3000 → **3.0 s 恒定** |
| 灼伤攻击力 | **(-5,6,1.0)** | col6 = 941 → 10604（**源 -5 新见**，本批首例） |
| 灼伤周边攻击力 | **(-5,7,1.0)** | col7 = 1214 → 13687（源 -5 第 2 例） |

未引用列：col8=1、col9=1000、col10=50（恒定三列，无向量——疑灼伤 tick 间隔/扩散参数，未考证）。pvp 表同构减档（col0=215→2435 等，static 同）。

**与基础 022 爆炎·波动剑对照**：形状从"前推火浪+终点爆炸"两段 → "**火焰地带（4s 多段）+ 爆炸**"，新增**灼伤子系统**（100% 上火、独立两列攻击力——基础版明确不灼烧，022 §2.1 已证）；主波动画全复用基础 fire-front/back/fire_dodge（视觉直系继承，096 预载互证同款）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（F1 波动剑族 + 引擎内置）

- load_state 无本技注册；`wave\wave.nut` 的 `onKeyFrameFlag_WaveSword` 只有 IceWaveEx（100）分支（022 已录），FireWaveEx 同样无脚本分支——施法走族共用 WaveSword 状态 24（`wave.ani`，.chr etc motion #9）+ engine 择技。波动印联动（ap_wavemark 推印）族内通用。
- 被动对象 ×2（passiveobject.lst 实测 11330-11333 行）：**PO 20101 = FireWaveBigEx.obj**（主火浪）、**PO 20102 = FireWaveBigSubEx.obj**（爆炸+火焰地带）。
- 白名单 grep `firewaveex`（sqr 侧）0 命中——行为引擎内置（022 同型）。

### 2.2 引擎内置行为重建（数据反推，推断标注）

```
施放（读条 400ms，wave.ani 族共用姿态）：
  → 创建主火浪 PO 20101（FireWaveBigEx）：
    basic fire-front.ani + etc fire-back/fire_dodge/fire_normal（基础火浪动画全套复用）
    推进命中：col0% 火伤（atk：damage/stuck -1000 吸附/knuck back 1）
  → 触地/终点：创建 PO 20102（FireWaveBigSubEx）：
    爆炸：blast-front.ani（基础爆炸主层复用）+ blast-back-ex.ani（新背层）→ col1% 爆炸火伤
    火焰地带：ex_flame_floor（地面火海视觉）+ ex_bead_fire（火珠）+ 
      ex_bead_fire_attack_box.ani（纯判定帧：10 帧×70ms 全帧盒 offset(-152,-17,-22)+size(300,50,200)
      → x[-1.52,1.48] z[-0.22,1.78] 单位贴地宽盒）
    引擎按 static[2]=300ms 对地带内敌人多段结算 col2%（4s/0.3s ≈ 13 跳）
    灼伤：SubEx.atk [active status] burn 7 零占位——运行时按 col3(100%)/col4(Lv)/col5(3s) 注入
      灼伤 DoT 数值 = col6（941~10604）/扩散 col7（1214~13687）
```

### 2.3 被动对象（两个 .obj 完整实测）

**PO 20101 firewavebigex.obj（主火浪）**：

| .obj 节 | 值 |
|---|---|
| [basic motion] | `Animation/FireWaveBig/fire-front.ani`（**基础主层复用**，490ms） |
| [etc motion] | fire-back.ani / fire_dodge.ani / fire_normal.ani（基础火浪三层全复用，022 §2.4 已读帧表） |
| [attack info] | `AttackInfo/FireWaveBigEx.atk`：magic / fire / **damage 反应** / **stuck -1000**（吸附定身）/ knuck back 1 / blow / no blood 50 1.9 |
| 其余 | **[width] 0 0**（基础版无此节值差异记录）/ pass all / piercing 1000 |

**PO 20102 firewavebigsubex.obj（爆炸+火焰地带）**：

| .obj 节 | 值 |
|---|---|
| [basic motion] | `Animation/FireWaveBig/blast-front.ani`（基础爆炸主层复用，9 帧 630ms） |
| [etc motion] 十层 | ex_sub_normal/dodge（爆炸近层）、blast-back-ex（新背层 420ms）、ex_flame_floor_normal/dodge（火海底层）、ex_bead_fire_normal/dodge/dodge_rot（火珠三层）、ex_flame_dodge（焰舌）、**ex_bead_fire_attack_box.ani（纯判定帧：无 img，10 帧×70ms 全帧同盒——L7 空占位判定族的地带版）** |
| [attack info] | `AttackInfo/FireWaveBigSubEx.atk`：magic / fire / **none 反应** / **stuck -1000** / **[active status] burn `0 0 0 0 0 0 0`（7 零占位，运行时注入）** |

### 2.4 动画关键帧表（firewavebig\ 目录 Ex 新增件实测；基础件帧表见 022 §2.4）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img |
|---|---|---|---|---|---|
| ex_bead_fire_attack_box.ani（地带判定） | 10 | 700ms | 无 | **全 10 帧** `-152 -17 -22` + `300 50 200` | **无 img（纯判定）** |
| ex_flame_floor_dodge.ani（火海） | 10 | 700ms 循环 | 无 | 无 | `Effect/FireWave/ex_flame_floor_dodge.img` |
| ex_bead_fire_dodge.ani（火珠） | 10 | 700ms 循环 | 无 | 无 | `ex_bead_fire_dodge.img` |
| ex_flame_dodge.ani（焰舌） | 9 | 560ms | 无 | 无 | `ex_flame_dodge.img` |
| blast-back-ex.ani（爆炸背层） | 6 | 420ms | 无 | 无 | blast-back.img（基础图复用） |
| （复用基础件）fire-front/fire-back/fire_dodge/blast-front | — | 490/420/770/630ms | — | ATKBOX.ani（扩张盒，022 已读） | FireWave/*.img |
| .als | ex_bead_fire_dodge.ani.als | — | — | — | 叠层（022 已记） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FireWaveEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FireWaveEx.skl` | ✅（249 行） | 11 列全解 + 二觉归属 |
| 注册行 | —（族共用行 16/17，本技无独立分支） | `…\sqr\character\swordman_load_state.nut` | ⛔（WaveSword 24/-1） | F1 族 |
| 主 nut | wave.nut（无本技分支） | `…\sqr\character\swordman\wave\wave.nut` | ✅（022 已全读） | 族共用 |
| PO 注册 | passiveobject.lst:11330-11333 | `…\passiveobject\passiveobject.lst` | ✅ 实测 | 20101 / 20102 |
| PO 定义 | firewavebigex.obj / firewavebigsubex.obj | `…\passiveobject\character\swordman\` | ✅ 实测 | §2.3 |
| PO .atk | firewavebigex.atk / firewavebigsubex.atk | `…\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | §2.3 |
| PO .ani | firewavebig\ ex_* ×10（含 .als ×1）+ 复用基础件 ×6 | `…\passiveobject\character\swordman\animation\firewavebig\` | ✅ 实测 | §2.4 |
| 角色 .ani | wave.ani（族共用） | `…\character\swordman\animation\` | ✅（022 已读） | 施法姿态 |
| 角色 .atk | —（无） | `…\character\swordman\attackinfo\` | ⛔ | 命中在 PO 表（L3） |
| skl 预载 | FireWaveFloor.img + Common/Fire.img + FireLight.img | [skill preloading image] | ✅（与 022 同清单） | 预载（翻译记档节） |
| 基础技文档 | 022-FireWave.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 对照基准 |
| 关联 TP | FireWaveExp.skl（212，E6 批） | `…\skill\Swordman\` | ✅ 存在 | 见 §8 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （复用 022 必需）flame_dodge / fire-front / blast-front | sprite_character_swordman_effect_firewave.NPK 等三 NPK | 主浪/爆炸（022 已列必需 3 张） | 必需（共享） | ❌ |
| `…/Effect/FireWave/ex_flame_floor_dodge.img` | sprite_character_swordman_effect_firewave.NPK（**与基础同 NPK**） | 火海底层 | **必需** | ❌ |
| `…/FireWave/ex_flame_floor_normal.img` | 同上 | 火海常态层 | **必需** | ❌ |
| `…/FireWave/ex_bead_fire_dodge.img` | 同上 | 火珠 | **必需** | ❌ |
| `…/FireWave/ex_bead_fire_normal.img` | 同上 | 火珠常态层 | **必需** | ❌ |
| `…/FireWave/ex_flame_dodge.img` | 同上 | 焰舌 | **必需** | ❌ |
| `…/FireWave/ex_sub_dodge.img`、`ex_sub_normal.img` | 同上 | 爆炸近层 | 可选 | ❌ |
| `…/FireWave/ex_bead_fire_dodge_rot.img` | 同上 | 旋转火珠（dodge_rot.ani） | 可选 | ❌ |
| Common/CommonEffect/Fire.img、FireLight.img | sprite_common_commoneffect.NPK | 预载/火光（022 已记） | 可选 | ❌ |

**缺失 img：本技新增必需 5 张、可选 3 张——全部落在基础 022 的 firewave NPK 内（一次提取全覆盖）；跨 NPK 0 张新增。**

## 5. 实现方案草案（号段：SkillIds 37 / AnimIds 194-196 / AreaIds 43-45 / BuffIds 18，E7 批内顺延）

### 内容件清单

1. **`DotNet~/Skills/SuperFireWaveSkill.cs : SkillLogic`**（022 FireWaveSkill 三区时序直扩）：
   - `CooldownMs=50000`；`TotalTimeMs=1700`（读条 400 + 主浪 763 + 余量；地带/爆炸区独立存续）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanWaveCast)`（族共用 wave.ani，F0 手改 400——022 同款）+ `ctx.ClearHitTargets()`。
   - `OnUpdate`：t≥400 → `ctx.CreateAreaInFront(AreaIds.SuperFireWave, 1)`（主浪区）；t≥1163 → `ctx.CreateAreaInFront(AreaIds.FireFloorZone, 2)` + `ctx.CreateAreaInFront(AreaIds.SuperFireWaveExplosion, 2)`（地带+爆炸同点，可同帧）。
2. **`DotNet~/Areas/SuperFireWaveArea.cs : AreaDefinition`**（主浪，022 FireWaveArea 直改）：
   - `TotalTimeMs=770`、`EnterActions={MeleeHit}`、`HalfExtents=(2.0,0.32,1.35)`（ATKBOX F2 同基础）；
   - `HitReaction{Damage=110, HitstunMs=900, KnockbackX=0, LaunchY=0}`（atk damage/stuck -1000/knuck back 1——stuck 并入长硬直，022 爆炸同款；Damage=col0 5166% 折算）；
   - `ViewAnimId=AnimId.FireWaveFront`（复用基础 json）。
3. **`DotNet~/Areas/FireFloorZoneArea.cs : AreaDefinition`**（火焰地带——本技核心增量）：
   - `TotalTimeMs=4000`（static[1]）、**`TickTimeMs=300`**（static[2]，13 跳）、`TickActions={MeleeHit, AddBurnBuff}`（FireCircleSkill Tick 范式直用）；
   - `HalfExtents=(1.5,0.35,1.0)`（ex_bead_fire_attack_box 盒 x[-1.52,1.48]/y[-0.17,0.33]/z 折算）；
   - `HitReaction{Damage=35, HitstunMs=300, KnockbackX=0, LaunchY=0, ProcBuffId=BuffIds.Burn, ProcChance=100}`（col2 1476%/跳 + 灼伤 100% 直译）；
   - `ViewAnimId=AnimId.FireFloor`（ex_flame_floor_dodge 700ms 循环）。
4. **`DotNet~/Areas/SuperFireWaveExplosionArea.cs : AreaDefinition`**（爆炸）：`TotalTimeMs=630`、`EnterActions={MeleeHit}`、`HalfExtents=(1.2,0.4,1.2)`（022 爆炸区同参）、`HitReaction{Damage=180, HitstunMs=900, KnockbackX=0, LaunchY=0}`（atk none/stuck；Damage=col1 8607%）、`ViewAnimId=AnimId.FireWaveBlastFront`（复用）。
5. **`DotNet~/Buffs/BurnBuff.cs : BuffDefinition`**（**复制 BleedBuff 改名**）：`TotalTimeMs=3000`（col5）、`TickTimeMs=1000`、`TickActions={BurnTick}`，每跳 25（col6 941% 折算；"灼伤周边攻击力"col7 的扩散语义不做，记 §7）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| WaveSword 状态 + wave.ani | 族共用 PlayAnim（022 同款） |
| 主浪 PO 20101（基础动画复用 + stuck） | SuperFireWaveArea（022 直改） |
| 火焰地带（4s/0.3s 多段 + 地带盒） | **FireFloorZoneArea Tick**（FireCircle 先例；地带盒 ex_bead_fire_attack_box 直读） |
| 爆炸 PO 20102 basic（blast-front 复用） | SuperFireWaveExplosionArea |
| 灼伤（100%/Lv72-170/3s/独立两列攻击力） | BurnBuff（BleedBuff 同构）——异常 Buff 挂法 L6 链路 |
| stuck -1000（吸附定身） | HitstunMs 长硬直近似（022 先例） |
| 波动印联动 | Buff 查询门面缺失 → 跳过（F1 族通用） |
| 火属性 | 元素系统缺失 → 无属性（惯例） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SuperFireWave = 37` + 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `SuperFireWave = 43`、`FireFloorZone = 44`、`SuperFireWaveExplosion = 45` |
| BuffId | `Runtime\BuffDefinition.cs` | `Burn = 18` |
| AnimId | `AnimConfigRegistry.cs` | `FireFloor = 194`（+可选 `FireBead = 195`、`FlameDodgeEx = 196`——视图单层够用则免） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×1-3；firewave NPK 补提 5 张 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD / 读条 | 50000 / 400 ms | 直用 |
| 主浪伤害 | col0 5166%→58432% | 110 |
| 主浪反应 | damage / stuck -1000 | Hitstun 900 / Kb 0 / Ly 0 |
| 地带 | 4s × 0.3s ≈ 13 跳，col2 1476%→16695%/跳 | Tick 300ms × Damage 35 |
| 地带盒 | x[-1.52,1.48] y[-0.17,0.33]（贴地） | HalfExtents (1.5,0.35,1.0) |
| 爆炸 | col1 8607%→97386% | 180 |
| 灼伤 | 100% / Lv72→170 / 3s / col6 941→10604 每跳 | BurnBuff 3s×1s×25 |
| 灼伤扩散 | col7 1214→13687（周边） | 不做（§7） |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| FireWaveEx.skl | `.skl` 无子命令 | 手抄（已全解）；**[special level up] 之外的 E 类新输入：[skill fitness second growtype]/[second growtype maximum level] 12 槽**（R6-C4 已解，skl 子命令一并纳入） |
| firewavebigex.obj / firewavebigsubex.obj | `.obj` 无子命令 | 手工映射三 Area（§2.3 已给全表；etc 十层的"纯判定帧混视觉层"结构是 obj 子命令设计输入） |
| firewavebigex.atk / firewavebigsubex.atk | `.atk` 无子命令；**[active status] burn 7 零占位**（运行时注入型） | 手抄；atk 子命令 burn 建模沿用 044 curse 7 参输入 + "占位全零=运行时注入"注记 |
| ex_bead_fire_attack_box.ani | 无 img 纯判定帧（L7 族） | ani 子命令现状可译（帧表无 IMAGE 也输出）；确认不因缺图报错 |
| wave.ani | `[DELAY] 10000` F0 | 022 同款（手改 400） |
| .als / 其余 .ani | 常规节 | ✅ 全覆盖 |

本技能翻译缺口 4 类（.skl/.obj/.atk/超长 DELAY——均既有记档的复证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 灼伤 Lv72→170（等级对抗） | 状态等级(Lv)系统缺失（R6-C4 已记） | 直判命中（FreezeBuff 同款） |
| 灼伤"周边攻击力"（col7 扩散） | 目标位置读取/AoE-from-target 缺失（R4-B18） | 不做（BurnBuff 单体 DoT；col7 忽略） |
| stuck -1000 吸附 | 无 hold/stuck 微控（022 同族） | HitstunMs 长硬直近似 |
| 灼伤概率/Lv 随级成长 | 概率 100% 恒定无需 roll；Lv 直判 | 无损 |
| 火属性 | 元素系统缺失 | 无属性直伤 |
| 屏震/无色×2 | 延后 | 跳过 |

（主干三段时序、多段 Tick、异常 Buff 全部落在已有机制内——**本批 8 技中唯一 ✅ 档**。）

## 8. 存疑与缺口上报

- **未考证**：①static[0]=500/[3]=100/[4]=250 语义（疑弹速/火珠数量与半径）；②col8=1/col9=1000/col10=50 无向量引用（疑灼伤 tick 间隔 1s=col9？）；③**L21 源 -5** 新见（灼伤两列）——与已知 -1/-2（level 列）、-4/-6（未解）并列，疑 level 列引用变体，收尾统一；④主浪→地带/爆炸的触发时点（触地 vs 终点，022 同款悬案）；⑤灼伤扩散的引擎实现。
- **新旧 TP 并存关系结论（本批专项③）**：**99 与 212（FireWaveExp，E6 批）不是两代 TP**——99 是 [type] active 二觉替换主动技（前置 22 Lv1、二觉 30 档）；爆炎波动剑的 TP 是 212。**铁证：基础技 FireWave.skl [feature skill index] = 212**。
- **给 022 的回填**：022 §2.4 记"firewavebig 目录仅 ex_bead_fire_dodge.ani.als（Ex 系，不在基础版链路）"——Ex 系实为 10 文件族（本批 §2.4），全部属本技（99）链路；022 基础版结论不变。
