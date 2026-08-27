# 极 · 鬼剑术 (斩铁式)（DefenseDownOnCritical）

> 技能ID 91 | 级别 C | 可实现性 ⛔（暴击判定源缺失——触发前提不存在） | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极 · 鬼剑术 (斩铁式) | `skill\Swordman\DefenseDownOnCritical.skl [name]` |
| 英文名 | DefenseDownOnCritical（取 skl 文件名；[name2]="Super Ghost Fencing， ZT Style"） | 同上 |
| 职业 | 剑魂二觉被动（[second growtype maximum level] 槽 3-4 非零 = 剑魂段；名称即官方剑魂二觉被动） | 同上 |
| 学习等级 | 48 | 同上 [required level] |
| 最高等级 | 50（二觉段上限 30×2 槽） | 同上 |
| 类型 | passive（skill class 1）——预判"像主动技"已验 [type]=passive 证伪，为纯数值+触发被动 | 同上 [type] |
| 指令 / CD / MP | 无（常驻被动） | 同上 |
| static data | dungeon `10 10000`——static[0]=最大重叠 10；static[1]=10000 = 持续 10s；pvp `3 7000`（叠 3 层/7s） | 同上 [static data] |
| 一句话效果 | 增加攻击力；敌人受到自身暴击伤害时降低其物理防御力，可叠加 10 层持续 10 秒 | 同上 [explain] |

**level property（2 列，Lv1 → Lv50）**：模板
`最大重叠数<int>`←static[0]=10、`持续时间<float1>秒`←static[1]=10000×0.001=**10s 恒定**、
`物防降低量<int>`←col0、`基本攻击力和技能攻击力增加<float1>%%`←col1×0.1。
- col0 物防降低：**1003 → 17214**（每层；Lv1 即四位数，mod 数值风）；
- col1 攻击力：100→10.0% → 560→56.0%。
pvp 单列（col0 211→7492；无攻击力列）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全查实测，无一处命中）

grep `defensedown`：`swordman_load_state.nut`、`sqr\character\swordman\` 全树
（`passive_skill_swordman.nut` 无 case 91）、`sqr\character\JG_SwordMan\`、
`passiveobject\character\swordman\`、`character\swordman\effect\animation\`——全部无命中。

**结论（F3 引擎内置）**：本技能是"引擎暴击管线的挂钩件"——引擎伤害结算判定本次命中为暴击时，
向受击者写入"物防 -col0"appendage（10 层 ×10s）。触发源（暴击判定）与效果端（防御参与伤害公式）
都在引擎内，pvf 侧只有 .skl 数据。

### 2.2 机制归纳

```
学得即常驻：
  自身基本/技能攻击力 + col1×0.1%（10% → 56%）
  自身伤害打出暴击 → 目标物防 - col0（每层 1003~17214）
    叠层上限 10（static[0]），每层持续 10s（static[1]）
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DefenseDownOnCritical.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DefenseDownOnCritical.skl` | ✅ 实测 | 2 列 + static 2 槽 |
| lst 条目 | swordmanskill.lst 273-274 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 91 → 本 skl |
| 注册行 / 主 nut / appendage / PO / 动画 / 特效 | —（全查无，实测） | 同 186 §3 各路径 | ⛔ 缺失（引擎内置） | 暴击挂钩在引擎 |
| 图标 | SkillIcon.img #200/#201 | `Character/Swordman/Effect/SkillIcon.img` | ✅ 实测（路径） | 无 UI 消费 |
| 通用暴击 appendage（参照） | critical_hit\41/42.apd | `…\pvf\appendage\critical_hit\` | ✅ 实测（非本技能——物品向暴击 buff，佐证引擎暴击 change-status 体系存在） | 数据形态参照 |

## 4. 资源需求

**零资源需求**（无专属视觉文件）。

## 5. 实现方案草案

⛔ 暂缓——双缺失：
1. **暴击系统**（186 §8：我方无暴击 roll——本技能的触发前提"打出暴击"根本不会发生；
   MeleeHitAction.cs 实测只走 Damage/Hitstun/Knockback/ProcBuff 固定管线）；
2. **防御数值消费链**（NumericType.Defense=1004 存在，但 MeleeHit 伤害不读任何 NumericType——
   属性消费链缺口的防御侧姊妹实证）。

届时形态（暴击系统落地后零障碍）：暴击命中 → `ctx.AddBuff(target, BuffIds.Shredded)`，
`ShreddedBuff : BuffDefinition`（Stack 10 × 10s，AddActions 走 `AddNumeric(DefenseAdd, -col0)`）——
Buff 叠层简版天然同构（176 §5 同款）；完全复用现有 Buff 体系，缺的只是两端管线。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| DefenseDownOnCritical.skl | `.skl` 无子命令（2 列×50 级 + static 2 槽） | 手抄 4 值；全局已知缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 暴击触发（触发源） | **缺失：暴击系统**（186 §8；本技能是最强实证——被动挂在"暴击事件"上，无暴击则整技空转） | 随暴击系统立项 |
| 物防 -1003~-17214 ×10 层 | **缺失：防御参与伤害公式**（属性消费链姊妹缺口；NumericType.Defense 零消费） | 同上（伤害公式立项一并） |
| 攻击力 +10%~56% | **缺失：属性伤害消费链**（第 9 实证） | 同上 |
| 10 层×10s 叠层 | 无缺口（BuffDefinition 叠层已通） | 直用 |

## 8. 存疑与缺口上报

**未考证项**
1. 叠层刷新规则（每层独立 10s 计时 or 整体刷新——引擎 appendage 常规为每层独立，未考证）。
2. col0 数值与引擎物防面板的量纲（1003 起步为 mod 通胀数值，直译需缩放）。

**新系统级缺口**：无新上报（暴击系统并入 186 §8——本技能作为"暴击事件钩子"消费方列入清单；
**防御零消费**并入属性消费链记档，作防御侧首个实证）。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
