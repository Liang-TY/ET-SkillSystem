# 死亡抗拒（DieHard）

> 技能ID 34 | 级别 B（濒死自愈 buff） | 可实现性 🔶（**本批 6 技能中最可落地**：HP 门槛 + 恢复 Buff + 视觉全部现有件可表达；仅物防/硬直增益因数值消费链空转需砍） | 分析日期 2026-08-22 | 批次 B2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 死亡抗拒 | `skill\Swordman\DieHard.skl` [name] |
| 英文名 | DieHard（取 skl 文件名；[name2]="Die Hard"） | 同上 [name2] 实测 |
| 职业 | 狂战士（[skill fitness growtype]=3；升级上限仅狂战 20 级） | 同上 |
| 学习等级 | 20 | 同上 [required level] |
| 最高等级 | 50 | 同上 [maximum level] |
| 类型 | active（skill class 2） | 同上 [type] |
| 指令 | ↑↑ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 20000 ms | 同上 [dungeon][cool time] |
| MP | 40 → 448 | 同上 [consume MP] |
| 读条 | 300 ms（casting time） | 同上 |
| 特殊消耗 | 无 | 同上 |
| static data | `25 300`（2 值：**static[0]=25 已由模板向量直证**=可发动 HP 比率 25%；static[1]=300 疑=恢复 tick 间隔 ms——推断，pvp 表 150 与之互证） | 同上 + §1 模板 |
| 一句话效果 | 仅当自身 HP 极低（≤25%）时可施放：缓慢恢复大量 HP，并增加物防和硬直，持续一段时间 | 同上 [explain] |

**level property 模板解码（L21 向量法，6 模板 5 列全明 + 1 static 引用）**（Lv1 → 表末 dungeon）：

| # | 模板项 | 向量 | Lv1 | 表末 |
|---|---|---|---|---|
| 1 | HP恢复的持续时间 | -1,**1**,0.001 | 2.5s（2500 恒定） | 2.5s |
| 2 | HP恢复量 | -1,0,1.0 | 504 | 71323 |
| 3 | 增加物防 | -1,2,1.0 | 3744 | 54782 |
| 4 | 增加硬直 | -1,3,1.0 | 266 | 2006 |
| 5 | 增加效果的持续时间 | -1,4,0.001 | 21.579s | 94.053s |
| 6 | **可发动技能的HP比率** | **0,0,1.0（源=0 → static data 槽 0！）** | 25%（恒定） | 25% |

pvp 表：col1=5000（5s 恢复）、物防大减（480 起）、效果 7.7s、static `25 150`。

⚠ 表中段（约 Lv39-49 区段）出现 HP 恢复量回落再爬升（33852→35856→38004 且 col2 回落 35856→33328）——分段/跳档规则未考证（025-Khazan 同象）。

**批注（预判纠偏）**：批次提示曾把本技能归入"受击伤害管线钩���"消费方——实测**不撞**：死亡抗拒是"施放门槛 + 主动恢复"，HP 下限钳制（真不死系）另有技能；本技能唯一管线依赖是防御增益消费（可砍，见 §7）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**引擎内置（F3）**：load_state 无注册（grep `diehard` 零命中）；`sqr\character\swordman\` 全树、
atswordman/jg_swordman/demonicswordman/avenger/common 五个 load_state 全查零命中——**连参照脚本都没有**
（ats 版无同名技）。数据面只有 .skl + 特效 ani 两件。

### 2.2 引擎内置行为重建（.skl 数据 + buff 类标准形态推断）

```
可用判定：HP ≤ 25%（static[0]）才可施放——引擎 checkExecutableSkill 门槛（我们 MinCastHpPct 同构位）
施放（↑↑+Space，读条 300ms，CD 20s）：
  扣 MP
  播 buff motion（.chr [buff motion]=Summon2.ani 600ms——skill class 2 buff 类标准姿态，推断）
  播 pinchhpregen.ani（18f 860ms 特效，见 §2.3）
  挂"死亡抗拒"增益 appendage（引擎内置，pvf 无实体——持续 col4=21.6~94.1s）：
    ① HP 恢复：col1（2.5s）内恢复总量 col0（504→71323）
       —— tick 间隔疑 static[1]=300ms（2.5s/300ms ≈ 8.3 跳；pvp static[1]=150 与 5s/150ms≈33 跳互证，推断）
    ② 物防 +col2（3744→54782）
    ③ 硬直 +col3（266→2006）
  增益到期自动移除
```

### 2.3 特效（唯一资源件）

`character\swordman\effect\animation\pinchhpregen.ani`——18 帧 860ms，引
`Character/Swordman/Effect/PinchHpRegen.img`（无 SET FLAG/攻击盒）。
⚠ skl [skill preloading image] 写的是 `PinchHpRegen1.img`/`PinchHpRegen2.img`——**与 ani 实际引用的
`PinchHpRegen.img` 命名不一致**（预载清单疑旧版残留，记档非缺口）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 受击盒 | 备注 |
|---|---|---|---|---|---|---|
| pinchhpregen.ani（特效） | 18 | 860ms | 无 | 无 | — | PinchHpRegen.img；6 处 [SHADOW] 类节 |
| Summon2.ani（施法姿态，共用） | 12 | 600ms | F9=65534 | 无 | — | 025-Khazan 已验（sm_body 帧 78-89） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DieHard.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DieHard.skl` | ✅ 实测（253 行） | 数值（5 列全明 + static 门槛） |
| 注册行 | —（无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | 五个 load_state 全查 |
| 主 nut | —（不存在，无参照） | `…\pvf\sqr\character\swordman\` + ats/jg 全查 | ⛔ 缺失 | 引擎内置 |
| 施法姿态 | Summon2.ani（[buff motion] 共用） | `…\pvf\character\swordman\swordman.chr` 949-950 行 | ✅ 共用（推断挂接） | buff 类标准姿态 |
| 特效 ani | pinchhpregen.ani | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | §2.3 |
| 特效 img | PinchHpRegen.img | （NPK 内，未提取） | ✅ ani 引用实证 | 恢复特效 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（grep diehard 无） | ⛔ 无 | 无攻击 |
| .als | — | 两侧 animation 目录 | ⛔ 无边车 | — |
| 预载清单 | PinchHpRegen1/2.img | DieHard.skl [skill preloading image] | ⚠ 命名与 ani 引用不一致 | 记档 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 78-89 施法姿态） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | 必需 | ✅ 已在库 |
| PinchHpRegen.img | sprite_character_swordman_effect.NPK（img 直属 Effect 根，025 同规则推导） | 恢复特效 18 帧 | 必需（视觉还原） | ❌ 未入库 |

缺失 img：必需 1 张。技能 🔶 期间仅此一张待提取。

## 5. 实现方案草案

**🔶 简化可实现（增益削减版）**——主干三件（HP 门槛/恢复/持续壳）全部现有件：

- **内容件清单**：
  - `DotNet~/Skills/DieHardSkill.cs : SkillLogic`：
    - `CooldownMs = 20000`（DNF 原值直用）；`TotalTimeMs = 600`（Summon2 施法姿态）；**`MinCastHpPct = 25`**
      （static[0] 直译——TryCast 前拒绝、不进 CD，SkillLogic.MinCastHpPct 现成，BloodBoom 先例）。
    - `OnCast`：`ctx.PlayAnim(施法姿态 AnimId)` + `ctx.AddBuffToSelf(BuffIds.DieHardRegen)`。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - `DotNet~/Buffs/DieHardRegenBuff.cs : BuffDefinition`——**恢复载体**：
    - `TotalTimeMs = 2500`（col1 直用：恢复期 2.5s）；`TickTimeMs = 300`（static[1] 推断值）；
      `TickActions = { ActionIds.HealTick }`——约 8 跳 × 每跳 63（=col0 504 ÷ 8，Lv1 折算）。
  - `DotNet~/Actions/HealTickAction.cs : LSAction`——新 Action ~10 行（**BleedDamageTickAction 同构改加号**：
    `ctx.AddOwnerNumeric` 不行（内容层不碰 NumericType）→ 需 LSActionContext 加 `HealOwner(int)` 门面
    （`DamageOwner` 镜像，~3 行）——框架层微增量，先例充分）。
  - **物防/硬直增益（砍）**：NumericType.Defense(1004) 键存在但零消费（R1-A4 姊妹实证 NumericType.Speed），
    硬直无键——挂了也是空转；增益壳 `DieHardBuff`（TotalTimeMs=21600 直译 col4）可留作占位。
  - 特效：pinchhpregen 翻译 json 注册后由 buff AddActions 触发单播（或技能 OnCast 播）——overlay/自播两路均可。
- **概念映射**：HP≤25% 门槛 → `MinCastHpPct`（checkExecutableSkill 同构，01§3 已映射）；appendage 恢复 →
  `BuffDefinition.TickActions`；读条 300ms → 跳过（延后档）。
- **注册点**：`SkillIds.DieHard = 29`；`AnimId.PinchHpRegen = 138`；`BuffIds.DieHardRegen = 14`、
  `DieHardBuff = 15`（占位）；`ActionIds.HealTick = 14`。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms | 20000 直用 |
| HP 门槛 | ≤25%（static[0]） | MinCastHpPct=25 直用 |
| 施法 | 读条 300ms + 姿态 600ms | 600（读条跳过） |
| 恢复期 | 2500ms（col1） | 2500 直用 |
| 恢复总量 | 504→71323（col0） | demo 固定 500（8 跳 × 63） |
| tick 间隔 | 300ms（static[1]，推断） | 300 |
| 物防/硬直 | +3744→54782 / +266→2006 | **砍**（消费链缺失，空转） |
| 增益持续 | 21.6s→94.1s（col4） | 恢复期即效果期（简化合并 2.5s）或 20s 占位 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `DieHard.skl` | `.skl` 无子命令（5 列 × 70 行 + static） | 手抄 5 值全明；`skl` 子命令同前议 |
| `pinchhpregen.ani` | `[SHADOW]` 等节（实测节名均为常规+SHADOW） | 常规节可译；SHADOW 跳过无碍（已记档） |
| 预载清单 | `[skill preloading image]` 命名与 ani 引用不一致 | 工具忽略即可（016 已记档，本例再证"预载清单不可作为 img 清单依据"） |

结论：ani 资源全部可被现有 ani 子命令翻译；实质缺口 `.skl` 子命令（1 条，重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| HP ≤ 25% 才可施放 | **无缺口**（MinCastHpPct 现成） | 直译 |
| 2.5s 缓慢恢复 HP | **无缺口**（Buff Tick + 新 HealTick ~10 行） | 直译 |
| 增加物防 | **缺失档：属性数值消费链**（Defense 键在、伤害端零消费——NumericType.Speed 姊妹实证） | 砍（无防御公式的 demo 里空转） |
| 增加硬直 | 同上 + 无键位 | 砍 |
| 读条 300ms | 读条系统（延后档） | 跳过 |
| MP 40-448 | MP 系统（延后档） | 跳过 |
| 增益 21.6~94.1s 长持续 | 无（TotalTimeMs 可配） | 若砍增益则与恢复期合并 |
| 濒死演出/音效 | 音频（延后档） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static[1]=300 =恢复 tick 间隔（推断；pvp static[1]=150 互证但不排他）。
2. 增益 appendage 实体（pvf\appendage 大树无路径）。
3. level info 中段回落跳档规则（025 同象）。
4. 施法姿态 = Summon2（buff motion 共用，推断——无专属施法动画实测）。
5. 预载清单 PinchHpRegen1/2.img 与 ani 引用 PinchHpRegen.img 的关系（疑版本残留）。

**新系统级缺口**：无新增（物防增益归既有"属性数值消费链"；本技能**不是**受击伤害管线钩子消费方——预判纠偏已记 §1 批注）。
框架层微增量备忘：`LSActionContext.HealOwner(int)` 门面（治疗类 Action 的公共前置，Burn/Bleed 镜像 ~3 行）。

**翻译工具缺口**：`.skl` 子命令（重复印证）；预载清单命名失配记档（工具侧无需动作）。

**给下轮的经验**：狂战"濒死系"技能（死亡抗拒/血气唤醒等）的施放门槛都在 **static data 首值**（本技能 25%=MinCastHpPct 直译）——level property 里向量源为 **0**（非 -1/-2）的模板行就是读 static[0]，L21 规则的最干净实证样本。
