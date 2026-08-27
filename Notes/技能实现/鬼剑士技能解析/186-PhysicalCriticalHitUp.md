# 物理暴击（PhysicalCriticalHitUp）

> 技能ID 186 | 级别 C | 可实现性 ⛔（暴击系统缺失） | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 物理暴击 | `skill\Swordman\PhysicalCriticalHitUp.skl [name]` |
| 英文名 | PhysicalCriticalHitUp（取 skl 文件名；[name2]="Physical Critical Hit" 为 DNF 官方英文名） | 同上 [name2] 实测 |
| 职业 | 全系共通（[skill fitness growtype] 0-5；图标在 `Character/Common/SkillIcon.img` 共通表 #34/#35，L1 惯例佐证） | 同上 |
| 学习等级 | 20（[required level range] 3） | 同上 [required level] |
| 最高等级 | 20（各觉醒段上限 10） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | passive（skill class 4） | 同上 [type] / [skill class] |
| 指令 / CD / MP | 无（纯被动） | 同上（无相应节） |
| 一句话效果 | 攻击敌人时增加自身物理暴击率 +1%/级（Lv1 +1% → Lv20 +20%） | 同上 [explain] + [level property] |

**level property（1 列）**：模板 `增加物理暴击率 : <float1>%%`，向量 `(-1, 0, 0.1)` → col0 × 0.1 = 百分比。
[level info]（列数头=1）：Lv1=10 → Lv20=200，每级 +10 → **+1.0% → +20.0%（+1%/级）**，列语义零存疑。

### 暴击三件套对照（186 / 188 / 189 同构模板）

| 项 | 186 物理暴击 | 188 魔法暴击 | 189 魔法背击 |
|---|---|---|---|
| skl 文件 | `PhysicalCriticalHitUp.skl` | `MagicalCriticalHitUp.skl` | `MagicalBackAttackCritical.skl` |
| 效果 | 物理暴击率 + | 魔法暴击率 + | **从背后攻击时**魔法暴击率 + |
| Lv1 → Lv20 | +1% → +20% | +1% → +20% | +1.5% → +30% |
| 学习等级 / 最高等级 | 20 / 20 | 20 / 20 | 20 / 20 |
| growtype | 0-5 共通 | 0-5 共通 | 0-5 共通 |
| skill class | 4 | 4 | 4 |
| 图标（Common 表） | #34/#35 | #38/#39 | #40/#41 |
| 脚本/nut/appendage/PO | 全无（实测见 §2.1） | 全无（实测） | 全无（实测） |
| 可实现性 | ⛔ | ⛔ | ⛔（另需背击方向判定） |

## 2. 技能逻辑走读

### 2.1 注册与文件链（全查实测，无一处命中）

- `swordman_load_state.nut`：grep `physicalcritical` 无命中（无 pushState）；
- `sqr\character\swordman\` 全树（含 `passive_skill_swordman.nut`、`appendage\`、`beidong\`）：无命中；
- `sqr\character\JG_SwordMan\`：无命中；
- `passiveobject\character\swordman\`（animation/action/attackinfo 各 ls）：无 physicalcritical 相关文件；
- `character\swordman\animation\` / `effect\animation\`：无专属动画/特效（ls 实测）。

**结论（F3 引擎内置，且是最纯形态）**：pvf 侧只有一个 .skl。引擎读到技能后把 col0 直读进
change-status 系统（`CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_HIT_RATE` 一类引擎常量——
209 鬼夜在 `passive_skill_swordman.nut` 的挂载方式可作旁证：`addParameter(CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_HIT_RATE, …)`），
参与引擎伤害管线的暴击 roll。无任何专属资源、无任何脚本逻辑。

### 2.2 机制归纳

```
学得即常驻：物理暴击率 += col0 × 0.1%（Lv1 +1% → Lv20 +20%）
（无触发、无持续、无消耗——面板被动）
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | PhysicalCriticalHitUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\PhysicalCriticalHitUp.skl` | ✅ 实测 | 全部技能数据（唯一文件） |
| lst 条目 | swordmanskill.lst 155-156 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 186 → 本 skl |
| 注册行 / 主 nut / appendage / PO | —（不存在） | `…\pvf\sqr\character\swordman\` 等（全树 grep 实测） | ⛔ 缺失（引擎内置） | 属性进引擎 change-status |
| 角色 .ani / .atk / 特效 | —（不存在） | `…\pvf\character\swordman\animation\` 等 | ⛔ 缺失 | 无专属资源 |
| 图标 | SkillIcon.img #34/#35 | `Character/Common/SkillIcon.img`（共通表） | ✅ 实测（路径） | 我方无 UI 消费，不做 |

## 4. 资源需求

**零资源需求**（无 img/ani/als/atk/obj）——与 175-JumpPowerUp 同为"仅 1 个 .skl"的最短文件链形态。

## 5. 实现方案草案

⛔ 级免写。记**前提**：暴击系统落地（攻击方暴击率数值 + 伤害 roll 注入点 + 暴击伤害倍率）——
见 §8 上报。届时形态：`NumericType` 加 `PhysicalCritRate = 1008`（+子键），
本技能退化为"学得时 `AddNumeric` 挂 col0 值"的一条数据行，无内容件开发量。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| PhysicalCriticalHitUp.skl | `.skl` 尚无子命令（1 列 level info） | 手抄 1 个数即可；`.skl` 子命令为全局已知缺口（064 起多次记档） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 物理暴击率 +1%~20% | **缺失：暴击系统**（§6.3 清单外新缺口，见 §8）——我方伤害 = HitReaction.Damage 固定值直扣（MeleeHitAction.cs 实测），无暴击 roll、无暴击率数值位、无暴击伤害倍率 | 暴击系统立项前不实现；立项后本技能零成本接入 |

## 8. 存疑与缺口上报

**未考证项**：无（数据列语义全明；引擎内部公式位不在 pvf 可考范围）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **暴击系统**（本批首撞并集中实证）：`MeleeHitAction`/`SkillContext.CheckHit`/`NumericType`（实测止于 1006，
   无任何 Crit 键位）三处确认——无暴击率数值位、无伤害 roll 注入点、无暴击伤害倍率。
   消费方已累计：186/188/189（本批）+ 91 斩铁式（暴击触发前提）+ 209 鬼夜（挂载实证但无消费）+ 119（剑影同类）。
   建议与"属性数值无伤害消费链"（176 §8）合并为**伤害公式立项**一并补齐——
   落地形态：MeleeHit 改读 source NumericType（攻击力 + 暴击率 roll → 暴击倍率），一次改动全族解锁。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
