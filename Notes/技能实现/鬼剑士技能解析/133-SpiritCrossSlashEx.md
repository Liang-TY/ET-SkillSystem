# 强化 - 共鸣 : 离魂一闪（SpiritCrossSlashEx）

> 技能ID 133 | 级别 E | 可实现性 ✅（基础技 066 本身 ✅；增量 = 纯攻击力乘区，零新机制零新资源） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 共鸣 : 离魂一闪 | `skill\Swordman\SpiritCrossSlashEx.skl` [name] |
| 英文名 | SpiritCrossSlashEx（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype] 全列；[growtype maximum level] `5 0 0 0 0 5` → TP 上限 5） | 同上 |
| 学习等级 | 55（[required level range] 5）；前置 66 共鸣:离魂一闪 Lv1 | 同上 |
| 最高等级 | [maximum level] 7（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· skill class 5（新式 TP）/ 物理 | 同上 |
| TP 消耗 | 2 点/级 | 同上 |
| 一句话增量 | 共鸣:离魂一闪攻击力 +10%/Lv（突进斩/交叉共鸣两列） | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 新式结构（byte 级实证）

python diff 实证：[static data]（`400 0`）与 [level info]（71 行 × 2 列：col0 突进斩 1626→13010、col1 交叉共鸣 3794→30357）与 `ghostsword\spiritcrossslash.skl` 逐字节相同（dungeon/pvp 两节均同）。[special level up] 空节——增量引擎内部 ×(1+0.1N)（同 083 分型）。

**skl 结构异常记档**：本文件含**两个 [dungeon] 节**（首个为空壳，第二个携带 static/level info；pvp 节的 level info 为空）——mod 编辑或官方数据迁移痕迹，引擎取有数据的一节；skl 子命令实现时需兼容"同名节重复、取非空"。

level property（模板对位）：
- 突进斩击/交叉共鸣攻击力 ← (-1,0)/(-1,1) = col0/col1（副本表）；
- 剑影和幻鬼突进距离 ← (0,0,1.0) = static[0]=**400px**（不变）；
- 特殊功能·普通状态施放时删除幻鬼突进、前方立即生成幻鬼并发动交叉共鸣 ← (1,1,1.0) = static[1]=**0（关）**——TP 不开此功能。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `spiritcrossslashex` 0 命中；无 PO/ani/atk/appendage。基础技 PO 24349 id68 取数 `sq_GetBonusRateWithPassive(66,-1,0/1)`（066 文档 §2.3 实证）——TP 加成引擎层并入两列。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SpiritCrossSlashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SpiritCrossSlashEx.skl` | ✅（269 行，双 [dungeon] 异常） | 副本 + 引擎增量 |
| 基础技文档 | 066-spiritcrossslash.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | ghostsword\spiritcrossslash.skl | 同 skl 树 | ✅ | diff 对拍相同（两节均同） |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无新增（复用 066 全链：PO 24349 id68） | 纯数据被动 |

## 4. 资源需求

无自有资源（图标位 616/617）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 066 一并落地）

- **零新增内容件**。066 草案两件各接 `TpLevel`（0-5 常量）：
  - `SpiritCrossPhantom`（前突弹，col0 路）：`Damage = 90 × (1 + 0.10 × TpLevel)`；
  - `SpiritCrossCrossArea`（共鸣区，col1 路）：`Damage = 180 × (1 + 0.10 × TpLevel)`。
  - 突进距离 400px（弹速/时长）、命中反应（意图 atk 平砍 / second.atk 浮空击倒）、CD 12000 不变。
- **概念映射**：引擎 ×(1+0.1N) → TpLevel 乘区 ×2；TP 学习系统缺失（R6-C1）→ 常量。
- **关键数值表**：TP0-5 两列同乘 ×1.0 → ×1.5。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| SpiritCrossSlashEx.skl | `.skl` 无子命令；**同名节重复（[dungeon] ×2）结构首见** | skl 子命令实现需兼容"同名节取非空一份"；增量同 083 分型处理 |

本技能翻译缺口 1 类（.skl，含重复节兼容项）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |
| 分离态特殊功能（static[1] 开关） | 本 pvf=0 关闭 + Buff 查询门面缺失（066 §7 已录） | 不做（与基础技侧一致） |

## 8. 存疑与缺口上报

- **未考证**：双 [dungeon] 节的成因（mod 编辑痕迹推断）；新式 TP ×(1+0.1N) 公式为推断（同 083）。
- 无新增系统级缺口（基础技 066 的缺口清单——VS 族 atk 错位/幻鬼三相位拆件等——TP 侧全部不触）。
