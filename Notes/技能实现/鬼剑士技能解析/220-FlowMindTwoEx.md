# 强化 - 流心：跃（FlowMindTwoEx）

> 技能ID 220 | 级别 E | 可实现性 🔶（增量本身零新机制；整体随基础技 108 的 🔶 前提） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 流心：跃 | `skill\Swordman\FlowMindTwoEx.skl` [name] |
| 英文名 | FlowMindTwoEx（取 skl 文件名；[name2] 实测 `流心 快 Upgrade`） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1；[growtype maximum level] `0 5 0 0 0 0` → TP 上限 5） | 同上 |
| 学习等级 | 55（[required level range] 5）；前置 108 流心:跃 Lv5 | 同上 |
| 最高等级 | [maximum level] 10（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· skill class 1（老式 TP） | 同上 |
| TP 消耗 | 2 点/级 | 同上 [special purchase cost] |
| 一句话增量 | 流心:跃攻击力 +10%/Lv（两列同加） | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 增量声明（byte 级实证）

python diff 实证：[static data]（14 值）与 [level info]（71 行）与 `FlowMindTwo.skl` 逐字节相同。增量：

| special level up 行 | 解读 | 对账 |
|---|---|---|
| `-1 0 % 10` | level col0（主攻击力列，755→6278）每 TP 级 +10 | explain ex "攻击力增加率 10%" ✓ |
| `-1 3 % 10` | **level col3（108 文档遗留未考证列，疑追加砍击/冲击波攻击力）每 TP 级 +10** | explain ex 未单列——TP 对两列同加，**旁证 col3 是第二路伤害列** |

**不受 TP 影响**：col1（CD）、col2、static 14 值（跳跃物理参数组/冲击波参数）——TP 纯加两路攻击力。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `flowmindtwoex` 0 命中；无 PO/ani/atk/appendage。基础技数据经引擎（流心系状态 63 引擎内置，F6）结算时并入。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMindTwoEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMindTwoEx.skl` | ✅（241 行） | 副本 + 增量声明 |
| 基础技文档 | 108-FlowMindTwo.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | FlowMindTwo.skl | 同 skl 目录 | ✅ | diff 对拍相同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无 | 纯数据被动（引擎内置族 F6） |

## 4. 资源需求

无自有资源（图标位 322/323）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 108 一并落地）

- **零新增内容件**。108 草案的 `FlowMindTwoSkill` 加 `TpLevel`（0-5 常量）：
  - 砍击段 `Damage = 基准 × (1 + 0.10 × TpLevel)`；钝器冲击波 Area（可选二期件）若实现则 Damage 同乘（对位 col3 路的猜测实现——col3 语义未考证，乘区不区分列归属，实现侧无风险）。
  - CD/跳跃参数/命中反应（atk push50/down）不变。
- **概念映射**：[special level up] 两行 → TpLevel 乘区；TP 学习系统缺失（R6-C1 同族）→ 常量。
- **关键数值表**：TP0-5 攻击力 ×1.0 → ×1.5（两列同步）。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| FlowMindTwoEx.skl | `.skl` 无子命令；[special level up]（本批 219 首记） | 同 219：副本节可跳过、special level up 必须输出 |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |
| col3 列的 TP 加成落点 | col3 语义未考证（108 §8 遗留） | 乘区统一打到砍击段 Damage（不细分列） |
| 引擎内置状态 63 的 TP 结算 | F6 族全内置，无脚本可证 | 无需还原（数值侧已覆盖） |

## 8. 存疑与缺口上报

- **未考证**：col3 精确语义（本批 TP 两列同加旁证其为伤害列——108 文档"疑追加砍击/冲击波攻击力"的推断得到间接支持，建议主循环回填 108 §1 col2/col3 备注）；TP 是否影响 pvp 侧数值（pvp 无 special level up 行，推断不影响）。
- **给 108 的回填**：108 文档 §1 "col2/col3 未考证"可补注——TP special level up 对 col0/col3 同加 10%，col3 为第二伤害列（高置信）。
