# 强化 - 流心：刺（FlowMindOneEx）

> 技能ID 219 | 级别 E | 可实现性 🔶（增量本身零新机制——一行乘区；整体随基础技 107 的 🔶 前提） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 流心：刺 | `skill\Swordman\FlowMindOneEx.skl` [name] |
| 英文名 | FlowMindOneEx（取 skl 文件名；[name2] 实测 `流心 冲 Upgrade`） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1；[growtype maximum level] `0 5 0 0 0 0` → 剑魂 TP 上限 **5**） | 同上 |
| 学习等级 | 55（[required level range] 5）；前置 107 流心:刺 Lv5（[pre required skill] `107 5`） | 同上 |
| 最高等级 | [maximum level] 10（growtype 实际可学 5） | 同上 |
| 类型 | passive（[type]）· 特性技（[feature skill type] 1）· skill class 1（老式 TP） | 同上 |
| TP 消耗 | 2 点/级（[special purchase cost] 2） | 同上 |
| 一句话增量 | 流心:刺 攻击力 +10%/Lv；对异常状态敌人增伤 +3%/Lv（20% → 35%@TP5） | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 skl 结构：基础技数据副本 + 增量声明（byte 级实证）

python diff 实证：本 skl 的 `[dungeon][static data]`（13 值）与 `[level info]`（71 行）与 `FlowMindOne.skl` **逐字节相同**（pvp 侧亦同）——Ex skl 不携带"强化后数值"，只携带副本 + 增量声明。

增量声明 = `[explain ex]` + `[special level up]`（四元组：源 / 索引 / 格式 / 每级增量）：

| special level up 行 | 解读 | 与 explain ex 对账 |
|---|---|---|
| `-1 0 % 10` | level col0（攻击力列）每 TP 级 +10 | "技能每增加Lv1时， 攻击力增加率 : 10%" ✓ |
| `12 12 + 30` | static[12]（异常增伤基准 200 = 20%）每 TP 级 +30 → +3% | "技能每增加Lv1时状态异常攻击力增加率 : 3%" ✓ |

**不受 TP 影响的列（实证：special level up 无对应行）**：col1（CD）、col2/col3（钝器眩晕概率/等级）、static[2]/[5]（多段次数 2/4）、static[6]（太刀光剑段威力 70%）、static[7]（眩晕时长 3s）、static[8]（巨剑短剑捶击 450%）——TP 只加攻击力与异常增伤两路。

### 2.2 引擎消费（全内置，零脚本）

- load_state 无注册（TP 技能无状态）；sqr 白名单全树 grep `flowmindoneex` **0 命中**；无 PO / ani / atk / appendage。
- 基础技取数走 `sq_GetBonusRateWithPassive(107, 147, 0, 1.5)` 族（107 文档 §2.3 实测）——WithPassive 后缀即引擎把 TP 加成并入取值，TP 结算全在引擎层，pvf 无任何可读消费代码。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMindOneEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMindOneEx.skl` | ✅（231 行） | 副本 + 增量声明 |
| 基础技文档 | 107-FlowMindOne.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点（§5 草案在其上叠加） |
| 基础技 .skl | FlowMindOne.skl | 同 skl 目录 | ✅ | 副本对拍源（diff 实证相同） |
| 注册行 / 主nut / PO / .ani / .atk | — | — | ⛔ 无 | 纯数据被动，全引擎内置 |

## 4. 资源需求

无自有资源（仅 SkillIcon.img 图标位 320/321，不入资源链）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 107 一并落地）

- **零新增内容件**。107 草案（107 文档 §5）的 `FlowMindOneSkill` 加一个 `TpLevel`（int，demo 常量 0-5，帧同步安全）：
  - `HitReaction.Damage = 基准 × (1 + 0.10 × TpLevel)`（追加下斩段 Damage 同乘）。
  - 眩晕概率 33%/时长 3s/多段 2 次按基础技原值**不变**（§2.1 实证 TP 不改这些）。
  - 异常增伤 +3%/Lv：撞**属性数值无伤害消费链**（R1-A4）——基础技侧本就未实现（107 §7），随其一并跳过。
- **概念映射**：[special level up] 四元组 → `TpLevel` 常量乘区/加值；TP 学习系统（特性点购买/技能等级）缺失——与"技能等级系统"（R6-C1，35 封印解除同族）合并记档，demo 以常量表达。
- **注册点**：无新 SkillId/AnimId/img（TP 非可施放技，不进按键表）。
- **关键数值表**：

| TP 级 | 0 | 1 | 2 | 3 | 4 | 5 |
|---|---|---|---|---|---|---|
| 攻击力乘区 | ×1.0 | ×1.1 | ×1.2 | ×1.3 | ×1.4 | ×1.5 |
| 异常增伤 | 20% | 23% | 26% | 29% | 32% | 35% |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| FlowMindOneEx.skl | `.skl` 无子命令；**[special level up] 节首见**（四元组 源/索引/格式/增量） | skl 子命令设计输入：副本节（level info/static data 与基础技重复）可跳过，**[special level up] 必须输出**——它是 TP 增量的唯一声明处 |

本技能翻译缺口 1 类（.skl，含新节 [special level up]）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| TP 学习（特性点购买，55 级起每 5 级 1 点） | 技能等级系统缺失（R6-C1） | demo 常量 TpLevel |
| 异常状态敌人增伤 +3%/Lv | 增伤消费链（R1-A4）+ 目标状态查询（R2-A8） | 跳过（基础技同款未实现） |
| 武器差异分支下的 TP 结算 | 武器类型差异化缺失（107 §8 首撞） | 单档乘区（不分武器） |
| 空中段 1.5 倍率与 TP 的叠加次序 | 引擎内部，无脚本可证 | 未考证（§8） |

## 8. 存疑与缺口上报

- **未考证**：TP 乘区与空中段 1.5 倍率的叠加次序（乘法交换无差，仅记）；col1（CD）不受 TP 影响为 special level up 反证（无该行）——高置信但非引擎直证。
- **给轮间经验（E 批总规律，本批 9 例 diff 实证）**：**Ex/Exp 型 TP skl = 基础技 [dungeon]/[pvp] 数据逐字节副本 + 增量声明（老式 skill class 0/1 在 [special level up]，explain ex 为文案权威）；消费全引擎内置（基础技脚本 sq_GetBonusRateWithPassive 族取数时并入），无 nut/PO/ani/atk/appendage**。分析 TP 技能只需：读自己 skl 的 special level up + explain ex，diff 对拍基础技确认副本，零走读工作量。
