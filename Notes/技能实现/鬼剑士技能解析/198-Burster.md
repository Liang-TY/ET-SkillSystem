# 极限突破（Burster）

> 技能ID 198 | 级别 B | 可实现性 ⛔（技能本体=三项全部撞缺失档：①跨技能 CD 覆写/重置门面 ②移速/攻速/施放速度数值零消费 ③攻击力增减无伤害消费链——施法演出与标记 Buff 可做，但技能将空转；跨技能 CD 门面落地后可翻 🔶） | 分析日期 2026-08-22 | 批次 B5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极限突破 | `skill\Swordman\Burster.skl` [name] |
| 英文名 | Burster（取 skl 文件名；[name2] 实测 `Buster`） | 同上 [name2] |
| 职业 | 全职业共通（[skill fitness growtype] 0-5 六系全列；skill class 0 通用技能） | 同上 |
| 学习等级 | 60 | 同上 [required level] |
| 最高等级 | 10（各系上限 5） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动 buff（active） | 同上 [type] |
| 指令 | —（[command] 空，技能窗/快捷栏施放） | 同上 |
| CD | 180000 ms（pvp 起手 600000） | 同上 [cool time] / [start cool time] |
| 施法时间 | 1000 ms（读条） | 同上 [casting time] |
| MP | 1000 → 1500 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×5 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14（普攻/站立/受击?14） | 同上 [executable states] |
| static data | **31 个技能 ID**：`85 86 87 88 95 96 97 98 99 100 101 102 44 75 112 22 62 9 23 79 60 57 72 103 82 74 73 81 58 31 169`（排除/重置表，见 §2.2） | 同上 [static data] |
| 一句话效果 | 10~28 秒内 Lv30 以下技能 CD 消失（实际=覆写为 1000ms；抓取技等 31 技排除），攻速+20%/施放+20%/移速+10%，攻击力 -50% | 同上 [explain] + 脚本实证 |

**level property（6 列，模板 5 行 5 向量，全部自明）**：有效时间 = col0×0.001 = **10→28 秒**（唯一成长列）；
攻速增加率 = col1×0.1 = **20%**（恒 200）；移速增加率 = col2×0.1 = **10%**（恒 100）；
施放速度增加率 = col3×0.1 = **20%**（恒 200）；攻击力减少 = col4 = **50%**（恒）。
col5 = **1000**（恒，无模板行——脚本实证 = CD 覆写值 1000ms，见 §2.2）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**swordman_load_state.nut 无注册**——本技能是跨职业通用技能，注册在公共表（C2 定点读取实证）：

```
// sqr/character/common_load_state.nut 行 4-14（实测）
IRDSQRCharacter.pushScriptFiles("character/common/burster/common_state_onsetstate.nut");  // +onproc/scrollpos/onendstate/ontimeevent 共 5 件
IRDSQRCharacter.pushState(0, "Character/Common/Burster/Burster.nut", "Burster", STATE_BURSTER, -1);   // ×11（job 0-10 全职业）
// sqr/character/common/common_header.nut 行 4/8（实测）：STATE_BURSTER <- 198，SKILL_BURSTER <- 255
```

⚠ 常量表：状态号 = **198**（与剑士侧技能 ID 同数巧合）；脚本侧技能号用 **SKILL_BURSTER=255**（跨职业通用索引，
与 swordman.lst 的 198 的映射关系由引擎解析——未考证）。
⚠ ⚠ **mod 状态复用发现**：`common_state_onsetstate.nut` 等五件（被 pushScriptFiles 注入）里的
`onSetState_Burster` 是**普雷（Prey）副本飞行系统**（翅膀/疾风视觉、z 轴物理 sq_ZStop、900 速冲刺、
OBJECT_MESSAGE_INVINCIBLE 无敌、PO 160003/160014）——**同名状态被 mod 复用为飞行状态机**（C6 形态④变体：
不改原文件，用后注入的脚本文件覆盖同名回调）。原版 CD 增益逻辑完整幸存于 `bf_burster.nut`（韩文注释原版）。

### 2.2 主脚本逐段（bf_burster.nut，265 行，韩文注释原版 + ap_common_burster.nut 193 行）

- **checkExecutableSkill_Burster**：`sq_IsUseSkill(SKILL_BURSTER)` → 切 STATE_BURSTER。
- **checkCommandEnable_Burster**：死亡之塔/绝望之塔禁用；攻击态中仅 pvp 特例可取消。
- **onSetState_Burster**：`sq_StopMove`；播**通用 buff 动画**（`sq_GetBuffAni`——无专属角色动画）；
  攻击信息设 CUSTOM_ATTACK_INFO_RESONANCE（占位）；**动画速度按施法时间缩放**（读条 1000ms 对齐）；
  创建施法特效 ×2（buster_start_back_light3 前后层，Character/Common/Animation/BusterMode/）；
  黑屏闪（150 alpha，读条时长）；开始施法条。
- **onEndCurrentAni_Burster**：屏震 3/250；删旧 appendage → 挂 `ap_common_burster.nut`（有效时间 = col0）；
  **ChangeStatus appendage 三参数注入**：攻速 +col1、移速 +col2、施放 +col3；回 STAND（**施放后立即自由行动**）。
- **startSkillCoolTime(obj, skillIndex, …)（引擎 CD 钩子，核心机制）**：
  burster appendage 有效 且 `isEnableBursterSkill(skillIndex)` 为真 → **返回 col5=1000 作为该技能 CD**——
  "Lv30 以下技能 CD 消失"的真身 = **CD 覆写为 1000ms**（引擎按"Lv30 以下"预筛 + 脚本排除表双过滤，分工推断见 §8）。
- **isEnableBursterSkill**：遍历 static data 31 个 ID，命中 → 不享受 CD 覆写
  （韩文注释"금지된 스킬"=禁用技能表；含 85/86/87/88 觉醒大技、60 鬼影闪、82 卡洛等——觉醒技不参与 CD 消失）。
- **ap_common_burster.nut**：
  - onStart：**burster 自身 + 31 排除表技能的 CD 一次性清零**（`sq_SetStartCoolTime(chr, 0, intVec)`——发动瞬间白送一轮无 CD）；
    注册"眼部光谱"特效（ocular spectrum，蓝色 RGBA 20,80,200 渐隐）；有效期内持续黑屏暗角；
  - onAttackParent：**有效期内每次命中**在命中点生成 buster_hit_back_normal1/front_dodge 双层命中特效；
  - onEnd：清光谱特效；
  - onStartMap：进图时若 buff 仍在，补暗角闪屏。

### 2.3 被动对象 / appendage

无 PO。appendage 单件：`ap_common_burster.nut`（上述）。**col4（攻击力 -50%）在两份脚本中均无消费方**——
推断引擎侧 damage rate 调整（bf 内有"버스터모드에서 데미지율을 조정합니다"空注释段，功能未脚本化）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | 备注 |
|---|---|---|---|
| character\common\animation\BusterMode\ 47 个 .ani（含 4 个 .als） | — | — | 通用特效库：start 系（back_light1-3/front_circle/cross/lens1-6/light/rainbow）���loop 系（back/front + _normal）、hit 系（back_normal1-2/front_dodge）、finish 系（back_blackhole/circle1-5/exp1-5/line1-4 ×14） |
| 施法动画 | — | — | `sq_GetBuffAni` 通用 buff 动画（无专属角色动画，avatar 层 0 件实证） |

`.als` 边车：BusterMode 目录 4 个（hit_back_normal1 / loop_back_normal / loop_front_normal / finish_front_exp4）——通用 overlay。
角色侧（swordman）**无任何 burster 专属动画/atk/PO**（animation/attackinfo/effect/passiveobject 四处 ls 实测零命中）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Burster.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Burster.skl` | ✅（106 行） | 6 列 + 31 ID static |
| 注册行 | common_load_state.nut 行 10-20（STATE_BURSTER=198/技能 -1，job 0-10） | `…\pvf\sqr\character\common_load_state.nut` | ✅（C2 定点读取） | §2.1 |
| 常量 | common\common_header.nut 行 4/8 | `…\pvf\sqr\character\common\common_header.nut` | ✅（同上） | STATE 198 / SKILL 255 |
| 主 nut | burster.nut | `…\pvf\sqr\character\common\burster\burster.nut` | ✅（133 行） | **实为普雷飞行视觉库**（pushState 指向它，内容被复用） |
| buff 库 | bf_burster.nut | 同目录 | ✅（265 行，韩文原版） | CD 覆写/状态机/ChangeStatus（§2.2） |
| ap nut | ap_common_burster.nut | 同目录 | ✅（193 行） | buff 主体/CD 清零/命中特效 |
| mod 注入 ×5 | common_state_onsetstate/onproc/scrollpos/onendstate/ontimeevent.nut | 同目录 | ✅（242/158/52/11/58 行） | **普雷飞行状态机**（同名回调覆盖，§2.1） |
| 角色 .ani/.atk/PO | —（无） | swordman 各层目录 | ⛔ 零命中 | 通用 buff 动画 + 通用特效库 |
| 特效库 | BusterMode\ 47 .ani + 4 .als | `…\pvf\character\common\animation\BusterMode\` | ✅（引用链定点） | §2.4 |
| 装备层 | —（0 件） | `…\pvf\equipment\character\swordman\avatar\` | ✅（find 零命中） | 通用动画无换装层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| buster_front_cross.img / buster_back_light1/2.img | sprite_character_common_bustermode.NPK | 施法十字/背光（onSetState 双层） | **必需** | ❌ |
| buster_loop_normal / loop_back / loop_front.img | 同上 | 循环态视觉（脚本中部分被注释，als 在用） | 可选 | ❌ |
| buster_hit_normal / buster_hit_dodge.img | 同上 | 命中特效（onAttackParent 双层） | 可选 | ❌ |
| buster_finish_exp1-4 / finish_circle ×2 / finish_line.img | 同上 | 终结视觉（本 pvf 未见脚本引用——疑普雷系统用） | 可选 | ❌ |
| buster_front_lens / front_rainbow / frontlight1 / front_circle.img | 同上 | 施法辅助层 | 可选 | ❌ |

缺失 img：必需级 3 张 + 可选级 16 张（同一 NPK 一次提取全覆盖）。img 版本红线（v2/v4 可用/v5 不可）由提取时把关。
（角色动画 = 通用 buff 动画复用现有图集，无新增需求。）

## 5. 实现方案草案（⛔ 级：仅记架构占位，主体待门面）

当前框架下技能的所有效果端均无消费方，做出来是"空壳演出"。给出**门面落地后**的实现形态（先行记档）：

1. **`DotNet~/Skills/BursterSkill.cs : SkillLogic`**（形态就绪后）
   - `CooldownMs=180000`；`TotalTimeMs=1000`（读条即完，无站桩）。
   - OnCast：`ctx.PlayAnim(通用施法动画)` + `ctx.AddBuffToSelf(BuffIds.Burster)`。
2. **`DotNet~/Buffs/BursterBuff.cs : BuffDefinition`**（`TotalTimeMs=10000~28000`，纯标记）
   - 前提门面 A：**SkillContext.ResetSkillCooldown(skillId)**（R3-A14 已上报，本技能需要其"覆写"变体——
     buff 有效期内 TryCast 的 CD 检查按 1000ms 计，即 `GetSkillCooldown(skillId)` 门面 + 条件覆写）；
   - 前提门面 B：NumericType.MoveSpeed/AttackSpeed 消费链（R2-A7 姊妹，攻速/施放速度系统当前整体不存在）；
   - 前提门面 C：伤害端数值消费链（R1-A4——攻击力 -50% 的落点）。
   - 视觉：暗角/光谱跳过（闪屏延后档）；命中特效需攻击出手钩子（本批 082 上报）。
3. **31 技能排除表**：若门面 A 落地，排除表硬编码在 BursterBuff 内（排除清单照抄 static data）。

### 关键数值表

| 项 | DNF 原值 | demo 建议值（门面后） |
|---|---|---|
| CD | 180000ms | 180000 |
| 持续 | 10→28s（col0） | 10s（Lv1 档） |
| CD 覆写 | 1000ms（col5，31 技排除） | 1000ms 直用 |
| 攻速/施放/移速 | +20%/+20%/+10%（恒） | 待消费链 |
| 攻击力 | -50%（恒，引擎消费） | 待消费链 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| Burster.skl（6 列 + 31 ID static） | `.skl` 无子命令（既有）——static 是**技能 ID 数组**（新形态：前例均为数值参数） | skl 子命令立项时 static data 按原始数组 dump 即可（消费侧自行解释） |
| BusterMode\ 47 .ani + 4 .als | 节面常规（[use animation]/[add]/[none effect add] 均已支持） | 现有 ani/als 子命令全覆盖 |
| bf_burster.nut 韩文注释 | 非翻译问题 | — |

计 1 条既有缺口（.skl），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **CD 覆写 1000ms（buff 期内 Lv30 以下技能）** | **跨技能 CD 覆写门面**（R3-A14"跨技能 CD 重置"的强化版：条件性持续覆写，且需排除表过滤） | ⛔ 主体；门面落地前不做 |
| 发动瞬间 31+1 技 CD 清零 | 同上门面（一次性重置子集） | 同上 |
| 攻速+20%/施放+20% | 攻速/施放速度系统不存在（动画速度无门面） | 砍 |
| 移速+10% | NumericType.Speed 移动端零消费（R2-A7） | 砍 |
| 攻击力 -50% | 属性数值无伤害消费链（R1-A4） | 砍 |
| 施法动画速度=读条缩放 | 无动画速度门面（086 同报） | 固定速度 |
| 有效期暗角/光谱/命中特效 | 闪屏延后 + Buff 视觉挂接（R1-A5）+ 攻击出手钩子（082 上报） | 砍 |
| 塔副本禁用 | 无副本类型查询 | 无副本系统，天然不适用 |

## 8. 存疑与缺口上报

**未考证项**
1. "Lv30 以下"等级过滤的实现位置——引擎侧预筛（推断）与脚本 31 ID 排除表的分工边界；31 表含多个 >Lv30 技能（85-88 觉醒/60 鬼影闪/82 卡洛），疑"排除表"同时承担"发动时 CD 清零名单"双职（onStart 把 31 表全部清零——觉醒大技 CD 白送一轮？语义存疑，可能是 mod 调参残留或原版即如此）。
2. SKILL_BURSTER=255 与 swordman 技能 ID 198 的映射（引擎按职业解析通用技能索引——常见机制但未见实证）。
3. col4 攻击力 -50% 的消费方（脚本无，推断引擎 damage rate）。
4. pushState 指向 burster.nut（普雷视觉库）而 buff 状态回调在 bf_burster.nut——同名 `onSetState_Burster` 两处定义（bf 与 common_state_onsetstate），后者 pushScriptFiles 注入更晚应覆盖前者（普雷版生效？）——若如此 **buff 施法演出也被普雷版劫持**，实际线上表现未考证（本档按 bf 原版记录）。

**系统级缺口（主循环汇总）**
1. **跨技能 CD 覆写门面**（buff 期内他技能 CD 按 X ms 计 + 排除表）：R3-A14"跨技能 CD 重置"的姊妹需求，建议合并立项（LSSkillComponent 已有 CD 存储，覆写=查询时条件替换，实现代价低）。
2. **攻速/施放速度系统**（独立于移速消费链的又一数值消费缺口）：082/085/086/198 四技共撞，建议 00-总览单列。
3. **通用技能（skill class 0）注册面**：我们的 SkillId 体系按职业单列，通用技能（全职业一份逻辑）尚无先例——若做通用技能需注册面决策（单 SkillId 共用即可，记档备查）。

**给下轮的经验**：**通用技能（skill class 0）不在 swordman_load_state 里**——注册在 `sqr\character\common_load_state.nut`（C2 定点读取），行为脚本在 `sqr\character\common\<技能名>\`；查这类技能先 ls common 目录。**同名状态回调被 pushScriptFiles 注入的文件覆盖**（本例普雷飞行 vs 原 buff）是 C6 形态④的新变体（不改原文件、后注入覆盖），遇到"回调内容与技能语义完全对不上"时查 common_load_state 的 pushScriptFiles 列表。
