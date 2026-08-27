# 强化 - 鬼印珠（ReflectGuardExp）

> 技能ID 228 | 级别 E | 可实现性 🔶（增量 = 多段次数 +3/Lv，撞 Bullet 多段缺口族——Area Tick 路线可表达；整体随基础技 002 的 🔶 前提） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 鬼印珠 | `skill\Swordman\ReflectGuardExp.skl` [name] |
| 英文名 | ReflectGuardExp（取 skl 文件名；[name2] 实测 `Ghost gate return Upgrade`） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4；[growtype maximum level] `0 0 0 0 3 0` → TP 上限 **3**） | 同上 |
| 学习等级 | 55（[required level range] 5）；前置 2 鬼印珠 Lv5 | 同上 |
| 最高等级 | [maximum level] 5（实际可学 3） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· **skill class 0**（老式 TP） | 同上 |
| TP 消耗 | 3 点/级（[special purchase cost] 3——本批最贵） | 同上 |
| 一句话增量 | 鬼印珠多段攻击次数 +3 次/Lv（**不加攻击力**） | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 增量声明（byte 级实证）

python diff 实证：[static data]（4 值 `250 150 10 120`）与 [level info]（71 行 × 9 列）与 `ReflectGuard.skl` 逐字节相同。增量：

| special level up 行 | 解读 | 对账 |
|---|---|---|
| （dungeon）`2 2 + 3` | **static[2]（基准 10）每 TP 级 +3** | explain ex "多段攻击次数增加 : 3次" ✓ |
| （pvp）`1 1 - 0`、`2 2 + 0` | pvp 侧增量为 0 | pvp 不受 TP 影响 ✓ |

**关键解码（回填基础技 002）**：special level up 唯一增量行指向 **static[2]**，且 explain 明言增量是"多段攻击次数"→ **基础技 static[2]=10 即鬼印珠多段攻击次数基准（同敌最多结算 10 跳）**——002 文档"static[2]/[3] 未考证"中的 static[2] 就此解码（高置信：声明式数据 + 文案对账双证）。static[3]=120 仍未考证。

**不受 TP 影响**：9 列攻击力/时长/大小/爆炸全部（TP 纯加段数——攻击力×段数的整体 DPS 提升约 +30%/Lv @满段）。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `reflectguardexp` 0 命中；无 PO/ani/atk/appendage（珠体 PO 20033/爆炸 20049 均基础技资产，002 文档已全查）。TP 结算引擎内部（珠体多段循环的 MaxHitCounter 上限读 static[2]+增量，推断）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ReflectGuardExp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ReflectGuardExp.skl` | ✅（253 行） | 副本 + 增量声明 |
| 基础技文档 | 002-ReflectGuard.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | ReflectGuard.skl | 同 skl 目录 | ✅ | diff 对拍相同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无新增（复用 002 的 PO 20033/20049 链） | 纯数据被动 |

## 4. 资源需求

无自有资源（图标位 268/269；[shake screen]/[durability decrease rate] 为 skl 元数据）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 002 一并落地）

- **零新增内容件**。002 草案的珠体若走 **Area Tick 路线**（002 §5 简化二选一之二）：`TickTimeMs=150` 保持，`TotalTimeMs = 跳数 × 150`，其中跳数 = `10 + 3 × TpLevel`（TP0-3：10/13/16/19 跳）——**多段次数 TP 增量在该路线下是纯配置乘区，直接可表达**。
- 若走 **Bullet 单跳简化路线**（002 §5 简化①）：TP 增量无处安放（Bullet 多段 ResetHitIntervalMs 缺口未落地）——**建议基础技实现时直接选 Area Tick 路线**，TP 增量随之免费获得。
- **概念映射**：static[2]+3N → Area 总时长 = (10+3N)×150ms（tick 间隔不变）；TP 学习系统缺失（R6-C1）→ 常量。
- **关键数值表**：

| TP 级 | 0 | 1 | 2 | 3 |
|---|---|---|---|---|
| 多段次数 | 10 | 13 | 16 | 19 |
| 珠体活跃时长（间隔 150ms） | 1500ms | 1950ms | 2400ms | 2850ms |

（注意：珠体存续时长另有 col0=3s 独立列，TP 不改；满段 TP3 时 19 跳 × 150ms = 2850ms 仍在 3s 内自洽。）

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| ReflectGuardExp.skl | `.skl` 无子命令；[special level up]（本批 219 首记；本例含 pvp 双行 0 增量变体） | 同 219 处理；skl 子命令输出需保留 pvp/dungeon 两套 special level up |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 珠体同敌多段（0.15s × 10 跳，TP +3 跳/级） | Bullet 侧多段 ResetHitIntervalMs（R1-A5 首记、002 第 2 例） | 基础技选 Area Tick 路线（L19 三档之"Area Tick 可表达"档），TP 增量=时长配置 |
| 印记系统放大的段数叠加（n 印 +N 跳 × TP +3N 跳） | 技能资源标记系统缺失（002 §8 首报） | n 恒 0（基础技同款简化），TP 增量独立保留 |
| TP 学习（3 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |

## 8. 存疑与缺口上报

- **未考证**：static[2] 多段次数与"时长/间隔"换算的引擎精确规则（10 跳 × 150ms = 1.5s < 存续 3s——多段上限先到还是时长先到？推断上限先到）；static[3]=120 语义（002 遗留，本批无新证）。
- **给 002 的回填**：002 文档 §1 static data 行可补注——**static[2]=10 = 多段攻击次数基准**（本批 special level up + explain 对账双证），"static[2]/[3] 未考证"修正为"static[3] 未考证"。
