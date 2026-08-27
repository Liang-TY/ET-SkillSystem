# 自动格挡（AutoGuard）

> 技能ID 3 | 级别 B（被动触发型防御 buff——格挡的自动版） | 可实现性 ⛔（触发判定撞受击伤害管线钩子【第 6 消费方】+ 受击侧方向判定；与 001-Guard 同管线同队列，本技能提供最完整的参照脚本链） | 分析日期 2026-08-22 | 批次 B2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 自动格挡 | `skill\Swordman\AutoGuard.skl` [name] |
| 英文名 | AutoGuard（取 skl 文件名；[name2]="Auto Guard"） | 同上 [name2] 实测 |
| 职业 | 剑魂（[skill fitness growtype]=1；升级上限剑魂/剑影各 10 级） | 同上 |
| 学习等级 | 25（前置：[格挡] Lv1——[pre required skill] `1 1` 实证） | 同上 |
| 最高等级 | 20 | 同上 [maximum level] |
| 类型 | active（skill class 1）；[auto cooltime apply] 1 | 同上 [type] |
| 指令 | ↓↑ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 30000 ms | 同上 [dungeon][cool time] |
| MP | 60 → 420 | 同上 [consume MP] |
| 读条 | 700 ms（casting time） | 同上 |
| 特殊消耗 | 无 | 同上 |
| static data | dungeon 无；pvp `500`（1 值，语义未考证） | 同上 |
| 一句话效果 | 被击中前一秒以剑士本能自动施放[格挡]：受击时概率触发格挡，效果持续 120 秒 | 同上 [explain] |

**level property 模板解码（L21 向量法，2 列全明）**：

| # | 模板项 | 向量 | Lv1 | Lv20 |
|---|---|---|---|---|
| 1 | 施放机率 | -1,0,1.0 | **5%** | **100%**（每 5% 一档） |
| 2 | 持续时间 | -1,1,0.001 | 120s（120000 恒定） | 120s |

pvp 表：机率 2%→40%、持续 20s、static 500。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**鬼剑士本尊引擎内置（F3）**：load_state 无注册（grep `autoguard` 零命中）；`sqr\character\swordman\` 全树零命中
（attack.nut 隐藏函数组 F4 套路亦无）。

**同型完整参照脚本（F3③，剑魔 ats 版）**：`atswordman_load_state.nut:88`：
`IRDSQRCharacter.pushState(10, "character/atswordman/1_swordmaster/autoguard/autoguard.nut", "Autoguard", 126, 126)`——
**本批走读深度最高的参照**（189+24 行，mod 混淆可读），001-Guard 分析时已发现该文件但未细读，本文补全。

### 2.2 参照实现逐回调（atswordman autoguard.nut + ap_autoguard.nut 全读）

```
checkExecutableSkill_Autoguard：sq_IsUseSkill(126) → 切状态 126 子 0
onSetState 子 0（施放/读条）：播施法动画（ani 74；魔剑变体 231）；读条 700ms（sq_GetCastTime）折算攻速
onSetState 子 1（读条完）：★ 挂 ap_autoguard.nut，sq_SetValidTime(col1 = 120000ms = 120s)
      + setEnableIsBuff(true) + setBuffIconImage(57)——"自动格挡待机"buff
onEndCurrentAni：子 0 → 子 1；子 1 → 回待机 state 0
★ ap_autoguard.nut（触发判定核心，仅 24 行）——appendage 挂 onDamageParent 回调：
onDamageParent(appendage, attacker, ...)（受击结算管线内、伤害落地前）：
  prob = sq_GetLevelData(126, 0, level)（= col0 施放机率 5~100%）
  if (sq_getRandom(0,99) > prob) return            // roll 失败 → 伤害照常结算
  parent.getVar("Autoguard").setBool(0, true)      // roll 成功 → 置自动格挡标记
  if (IsFrontOf(parent, attacker)) 记攻击者方向 else 记反方向   // 受击侧方向判定，供转身面向攻击者
★ proSkill_ATSwordman_Autoguard（每拍钩子）：
  if (var "Autoguard" 标记已置) → 清标记 + 切子 2
onSetState 子 2（自动格挡瞬间）：
  sq_SetDirection(记录的攻击者方向)（转身面向攻击）
  播格挡动画（ani 76；魔剑变体 230）
  sq_SendMessage(OBJECT_MESSAGE_SUPERARMOR, 1, 0)（格挡期间霸体）
  ★ 挂 ap_guard.nut（=001-Guard 剖析过的格挡伤害吸收 appendage，技能 85 版）
    ——即"自动格挡成功 = 免费施放一次格挡"（explain 原文"施放自身已学的[格挡]技能"）
onEndState 子 2：SUPERARMOR 关 + 移除 ap_guard appendage
```

**与 001-Guard 的关系**：格挡（1）= 玩家按住主动姿态；自动格挡（3）= buff 常驻 120s，**受击瞬间概率自动转
入格挡状态**并复用格挡的伤害吸收 appendage + 被击退/反击窗口全套（001 §2.2）。两者的核心逻辑同一管线（受击钩子）。

**触发特效**（swordman 侧）：`effect\animation\autoguard_ldodge.ani` + `autoguard_none.ani`
（各 8f 800ms；ldodge=加亮层/none=普通层——触发瞬间闪光，引擎随格挡成功绘制）；
ats 版另有 `atautoguard\autoguard_eff.ani`（6f 480ms）。

### 2.3 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| autoguard_ldodge.ani（effect 根） | 8 | 800ms | 无 | 无 | autoguard_ldodge.img（加亮层） |
| autoguard_none.ani（effect 根） | 8 | 800ms | 无 | 无 | autoguard_none.img（普通层） |
| ats autoguard_eff.ani | 6 | 480ms | 无 | 无 | AutoGuard.img（ats 施放特效） |
| Guard.ani / HitBack.ani（触发后格挡姿态，001 已析） | 2 / 9 | 事件悬停 / 800ms | — | — | 001 §2.3 |
| 施法姿态 | Summon2.ani（[buff motion]，推断） | 12 | 600ms | — | — |

`.als` 边车：无（两侧 ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | AutoGuard.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\AutoGuard.skl` | ✅ 实测（139 行） | 数值（2 列全明） |
| 注册行 | —（鬼剑士无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | F2 三招全查 |
| 参照 nut | autoguard.nut + ap_autoguard.nut | `…\pvf\sqr\character\atswordman\1_swordmaster\autoguard\` | ✅ 实测（189+24 行） | ats 版完整实现（C2 定点读） |
| 格挡 appendage | ap_guard.nut | `…\pvf\sqr\character\atswordman\guard\` | ✅（001 已析 43 行） | 触发后复用的伤害吸收 |
| .chr 条目 | —（无 autoguard 条目） | `…\pvf\character\swordman\swordman.chr`（grep 无） | ⛔ 无 | 姿态/触发动画走引擎/共用 |
| 特效 ani | autoguard_ldodge.ani / autoguard_none.ani | `…\pvf\character\swordman\effect\animation\`（根） | ✅ 实测 | 触发闪光双层 |
| ats 特效 | atautoguard\autoguard_eff.ani | `…\pvf\character\swordman\effect\animation\atautoguard\` | ✅ 实测 | ats 施放特效 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（grep autoguard 无） | ⛔ 无 | 无攻击 |
| .als | — | 两侧 animation 目录 | ⛔ 无边车 | — |
| 前置 | Guard.skl（技能 1） | `…\pvf\skill\Swordman\Guard.skl` | ✅（001 已析） | [pre required skill] 依赖 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 123/124 格挡姿态、施法姿态） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动作 | 必需 | ✅ 已在库 |
| autoguard_ldodge.img | sprite_character_swordman_effect.NPK（img 直属 Effect 根） | 触发闪光加亮层 8 帧 | 必需（视觉还原） | ❌ 未入库 |
| autoguard_none.img | 同上 | 触发闪光普通层 | 必需（视觉还原） | ❌ 未入库 |
| AutoGuard.img（ats） | sprite_character_swordman_effect_atautoguard.NPK | ats 施放特效 | 可选 | ❌ 未入库 |
| guard_attack_ldodge/none.img（格挡冲击波，001 已记） | sprite_character_swordman_effect_guard.NPK | 触发后冲击波 | 可选（与 001 共用） | ❌ 未入库 |

缺失 img：必需 2 张、可选 3 张。⛔ 期间挂起（与 001-Guard 资源可合并提取）。

## 5. 实现方案草案

**⛔ 暂缓（触发判定）**——复用 001-Guard §5 的管线使能前提，本技能在其上加"概率触发层"：

| DNF 机制 | 我们的现状（代码实测） | 阻断点 |
|---|---|---|
| 受击前概率判定（ap_autoguard.onDamageParent + sq_getRandom） | `ApplyHit` 受击方零钩子——**连 roll 的插入点都没有** | **受击伤害管线钩子**（第 6 消费方；比格挡的"伤害修正"更前置——需要"受击前回调"而非"受击中修正"） |
| 转身面向攻击者（IsFrontOf + 记方向） | 无受击侧方向判定（001 §8-1 同缺口） | 随管线一并（钩子参数需带攻���者引用） |
| 自动转入格挡态 + 复用 ap_guard 吸收 | 同 001-Guard 全套阻断 | 管线落地后=001 的技能内封装 |
| 120s 待机 buff + 图标 | ✅ BuffDefinition + AddBuffToSelf 可挂（LSRng 概率面现成） | 无（壳可先做） |
| 霸体消息（SUPERARMOR） | 霸体帧（延后档） | 格挡瞬间免打断简化 |

**使能路径（与 001 共用）**：`LSCombatComponent.OnIncomingHit(attacker, ref HitReaction)` 管线落地后：
- `AutoGuardSkill : SkillLogic`——CD 30000、TotalTimeMs=700（读条跳过则 600）；
  `OnCast` `ctx.AddBuffToSelf(BuffIds.AutoGuardReady)`。
- `AutoGuardReadyBuff : BuffDefinition`——TotalTimeMs=120000（col1 直用）、AddActions={播施法特效}。
- 管线钩子内：宿主持 AutoGuardReady buff → `LSRng.Roll(5~100%)` 成功 → 伤害按格挡吸收%
  （复用 001 的 Guard 吸收参数）+ 硬直豁免 + 播 autoguard 闪光 + 消费一层（或转 600ms 反击窗口）。
- 注册点：`SkillIds.AutoGuard = 31`；`AnimId.AutoGuardLdodge = 147`、`AutoGuardNone = 148`；
  `BuffIds.AutoGuardReady = 18` 预留；格挡冲击波 Area 复用 001 预留 `AreaIds.GuardWave = 16`。

**关键数值表（若实现）**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000ms | 30000 直用 |
| 待机时长 | 120s（col1 恒定） | 120 直用（demo 可 30s） |
| 触发机率 | 5%→100%（col0） | 50%（固定，演示明显） |
| 触发后 | = 格挡（001 §5 数值：吸收 40→80%/击退/600ms 反击窗） | 复用 001 表 |
| 读条 | 700ms | 跳过 |
| 触发闪光 | 8f 800ms 双层 | 直译 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `AutoGuard.skl` | `.skl` 无子命令（2 列小表） | 手抄 2 值全明；`skl` 子命令同前议 |
| autoguard_ldodge/none.ani | `[SHADOW]` 类（实测常规节+SHADOW） | 常规可译；SHADOW 跳过（已记档） |
| Guard.ani（触发后） | 双 10000ms 事件悬停帧（001 已记，L23 族） | 钳制/约定手改（重复印证） |

结论：ani 资源全部可被现有 ani 子命令翻译；实质缺口 `.skl` 子命令 + 超长 DELAY（均重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 受击前概率触发（核心） | **缺失档：受击伤害管线钩子**（第 6 消费方；且需"受击前"回调位） | ⛔ 主因；与 001 同队列等管线 |
| 转身面向攻击者 | 受击侧方向判定（001 §8-1） | 随管线 |
| 触发后=格挡全套（吸收/击退/反击窗/冲击波） | 同 001-Guard 全套 | 管线落地后复用 001 方案 |
| 格挡瞬间霸体 | 霸体帧（延后档） | 硬直豁免近似 |
| 120s 待机 buff | ✅ 可表达 | 直译 |
| 前置[格挡]Lv1 | 技能前置系统（缺失档） | 跳过（无技能树） |
| 读条 700ms | 读条系统（延后档） | 跳过 |
| MP 60-420 | MP 系统（延后档） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. pvp `[static data] 500` 语义（dungeon 无 static）。
2. 鬼剑士版触发特效与 ats 版差异（引擎内置绘制，无脚本佐证挂接）。
3. 施法姿态 = Summon2（推断）。
4. 触发后是否完整复用格挡的被击退表现（ats 版子 2 未见 HitBack 移动调用——疑引擎按格挡态默认处理）。
5. [feature skill index] 145 的用途（特征技能索引，未考证）。

**新系统级缺口（消费方增补）**
1. **受击伤害管线钩子第 6 消费方**，且提出新设计约束：需要 **"受击前"回调位**（roll 在伤害落地前，
   决定"这次伤害是否按格挡处理"）——与 001 的"受击中修正"、Keiga 的"受击计数"三个插入深度不同，
   管线立项时建议按 onIncomingHit（前）/onDamaged（中）/onHitCounted（后）三段设计。
2. 复用 001-Guard 全部记档（受击侧方向判定/按住输入等），无新增翻译缺口。

**翻译工具缺口**：`.skl` 子命令 + 超长 DELAY（均重复印证）。

**给下轮的经验**：防御触发族（自动格挡/不屈意志/各职业受击触发被动）的判定核心都在 **appendage 的
onDamageParent 回调**（受击管线注入点的另一形态——与 getImmuneTypeDamageRate（修正）不同，
onDamageParent 是**事件回调可改流程**）——查这族先搜 `sq_AddFunctionName("onDamageParent"` 模式。
