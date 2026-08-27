# 强化 - 鬼影闪（GhostStepSlashEx）

> 技能ID 226 | 级别 E | 可实现性 🔶（增量 = 攻击力 +10%/级 纯数值乘区，可满级预折算；整体随基础技 060 的 🔶 前提——TP1 的"僵直大幅减少"随基础技禁锢简化一并空转） | 分析日期 2026-08-22 | 批次 E7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 鬼影闪 | `skill\Swordman\GhostStepSlashEx.skl` [name] |
| 英文名 | GhostStepSlashEx（取 skl 文件名；[name2] 实测 `鬼影闪 UpGrade` 为中文别名，L1） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype]=2；[growtype maximum level] `0 0 5 0 0 0` → TP 上限 **5**） | 同上 |
| 学习等级 | 65（[required level range] 5）；前置 60 鬼影闪 Lv1 | 同上 |
| 最高等级 | [maximum level] 10（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· skill class 3（随基础技）——**老式声明型 TP**（[special level up] 显式四元组，R7-E4 两形态之"老式"） | 同上 |
| TP 消耗 | 2 点/级（[special purchase cost] 2） | 同上 |
| 一句话增量 | TP1：命中僵直时间大幅减少（引擎内置，无数据行）；**TP2 起：攻击力 +10%/级** | 同上 [explain ex] |

**双向链**：基础技 `GhostStepSlash.skl` [feature skill index] = **226** ✓（与 Ex 的 [pre required skill] 60 互指，E3-①规律第 11 例）。

## 2. 强化增量（TP 表）

### 2.1 副本 + 增量声明（byte 级实证）

python diff 实证（R7-E4 标准法）：[static data]（`300`）、[level info] dungeon（70 行 × 1 列，5692→46318）、[level info] pvp（70 行，205→1670）与 `GhostStepSlash.skl` **全部逐字节相同**。增量：

| special level up 行 | 解读 | 对账 |
|---|---|---|
| （dungeon）`-1 0 % 10` | **level col0（魔法攻击力列）每 TP 级 +10%**（% 型步长，E2 两格式之 `%`） | explain ex "2级开始攻击力增加量 : 10%%" ✓ |
| （pvp）无增量行 | pvp 不受 TP 影响 | E2"pvp 增量恒 0"规律 ✓ |

**不受 TP 影响**：static[0]=300 移动距离、CD、禁锢体系全部。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单六目录 grep `ghoststepslashex` **0 命中**（实测）；无 PO/ani/atk/appendage（判定链全在基础技 060 已查：PO 20026 / GhostStepSlash.atk / k-light.img）。TP 结算在引擎 feature 体系（R7-E3 ②第三批复证），消费入口疑为引擎 `sq_GetPowerWithPassive` 族对 col0 的折算——**增量起算级未考证**：explain 明示"2 级开始"，数据行无偏移信息（TP5 到底是 +40% 还是 +50% 未定，demo 按 explain 口径取 ×(1+0.1×(N-1))）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GhostStepSlashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GhostStepSlashEx.skl` | ✅（235 行） | 副本 + 增量声明 |
| 基础技文档 | 060-GhostStepSlash.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点（Area/Buff 草案在彼） |
| 基础技 .skl | GhostStepSlash.skl | 同 skl 目录 | ✅ | diff 对拍三节全同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无新增（复用 060 全链） | 纯数据被动 |

## 4. 资源需求

无自有资源（图标位 314/315；[skill preloading image] 空节）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 060 一并落地）

- **零新增内容件**。060 草案 `GhostStepSlashArea.HitReaction.Damage=110`（固定值）→ TP 增量 = **Damage 常量乘区**：`Damage = 110 × (1 + 0.1 × max(0, TpLevel-1))`（TP0-5：110/110/121/132/143/154——TP1 仅僵直收益，explain 口径）。
- **TP1 僵直减少项空转说���**：原版"僵直"在禁锢体系（060 已把命中反应简化为 HitstunMs=0 + HoldBuff）——僵直已被基础技简化抹平，TP1 的收益在 demo 中无对应物，不补偿（与 228 的"印记叠加项随基础技简化空转"同款处理）。
- **概念映射**：level col0 ×(1+0.1N) → Damage 乘区；TP 学习系统缺失（R6-C1）→ 常量 TpLevel（060 草案同款）。

**关键数值表**：

| TP 级 | 0 | 1 | 2 | 3 | 4 | 5 |
|---|---|---|---|---|---|---|
| 攻击力乘区 | ×1.0 | ×1.0（仅僵直收益） | ×1.1 | ×1.2 | ×1.3 | ×1.4 |
| demo Damage | 110 | 110 | 121 | 132 | 143 | 154 |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| GhostStepSlashEx.skl | `.skl` 无子命令；[special level up] `%` 型单行（E3 已记档格式之一） | skl 子命令纳入 [special level up] 四元组（% 与 + 两型，E1 已记） |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| TP1 命中僵直大幅减少 | 僵直在禁锢体系内（引擎内置），基础技已简化 | 不做（空转，§5） |
| 攻击力 +10%/级（TP2 起） | 属性数值无伤害消费链（R1-A4）——但我们 demo 伤害本就是常量 | Damage 常量乘区直译（零新机制） |
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |

## 8. 存疑与缺口上报

- **未考证**：①增量起算级（explain"2 级开始" vs 数据行无偏移——引擎折算公式 ×(1+0.1N) 还是 ×(1+0.1(N-1)) 无脚本旁证，demo 按 explain）；②TP1 僵直减少的引擎实现（无数据行，疑引擎硬编码常量）。
- **给收尾总览**：本技能是"**Ex 后缀 = 真 TP**"的常规样本（与 95/97/99/101 的"Ex = 二觉替换主动技"形成对照，判定法=看 [type] 与 [feature skill index] 指向，见 95 文档 §8）。
