# 强化 - 流心：升（FlowMindThreeEx）

> 技能ID 221 | 级别 E | 可实现性 🔶（增量本身零新机制；整体随基础技 109 的 🔶 前提） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 流心：升 | `skill\Swordman\FlowMindThreeEx.skl` [name] |
| 英文名 | FlowMindThreeEx（取 skl 文件名；[name2] 实测 `流心 升 Upgrade`） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1；[growtype maximum level] `0 5 0 0 0 0` → TP 上限 5） | 同上 |
| 学习等级 | 55（[required level range] 5）；前置 109 流心:升 Lv5 | 同上 |
| 最高等级 | [maximum level] 10（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· skill class 1（老式 TP） | 同上 |
| TP 消耗 | 2 点/级 | 同上 [special purchase cost] |
| 一句话增量 | 流心:升攻击力 +10%/Lv | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 增量声明（byte 级实证）

python diff 实证：[static data]（10 值 `180 350 45 1 0 2 1 120 60 220`）与 [level info]（71 行，col0 480→3944 / col1 10000→5）与 `FlowMindThree.skl` 逐字节相同。增量仅一行：

| special level up 行 | 解读 | 对账 |
|---|---|---|
| `-1 0 % 10` | level col0（攻击力列）每 TP 级 +10 | explain ex "攻击力增加率 10%" ✓ |

**不受 TP 影响（实证：无对应行）**：static[5]（光剑太刀 2 段多段）、static[7]（浮空增伤 +120%）、跳跃力档位、col1（CD）——TP 纯加攻击力一路。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `flowmindthreeex` 0 命中；无任何脚本/资源。基础技 109 走引擎内置状态 64（F6），TP 结算在引擎层（`sq_GetBonusRateWithPassive(109, …)` 族取数时并入）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMindThreeEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMindThreeEx.skl` | ✅（233 行） | 副本 + 增量声明 |
| 基础技文档 | 109-FlowMindThree.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | FlowMindThree.skl | 同 skl 目录 | ✅ | diff 对拍相同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无 | 纯数据被动（F6 族） |

## 4. 资源需求

无自有资源（图标位 318/319）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 109 一并落地）

- **零新增内容件**。109 草案的 `FlowMindThreeSkill` 加 `TpLevel`（0-5 常量）：
  - `HitReaction.Damage = 基准 × (1 + 0.10 × TpLevel)`（atk down 击倒反应、LaunchY 120 等不变）。
  - 浮空增伤 +120% 与光剑太刀 2 段本就按基础技简化跳过（109 §7），TP 不触及。
- **概念映射**：[special level up] 单行 → TpLevel 乘区；TP 学习系统缺失（R6-C1）→ 常量。
- **关键数值表**：TP0-5 攻击力 ×1.0 → ×1.5。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| FlowMindThreeEx.skl | `.skl` 无子命令；[special level up]（本批 219 首记） | 同 219 处理 |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |
| 引擎内置状态 64 的 TP 结算 | F6 全内置 | 无需还原（数值侧已覆盖） |

## 8. 存疑与缺口上报

- **未考证**：无新增（三技族 TP 结构一致，219/220 已覆盖族规律）；pvp 侧无 special level up 行，推断 TP 不影响 pvp 数值。
- **给 109 的回填**：109 文档 §1 level property 解码表与 TP 增量完全兼容（TP 只加 col0），无需修正。
