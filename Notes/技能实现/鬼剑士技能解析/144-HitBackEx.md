# 强化-逆转反击（HitBackEx）

> 技能ID 144 | 级别 E | 可实现性 ⛔（随基础技触发链缺失；反击攻击力 +10%/级为纯数值，基础技降级方案落地时增量直落） | 分析日期 2026-08-22 | 批次 E3

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 逆转反击 | `HitBackEx.skl [name]` |
| 英文名 | HitBackEx（skl 文件名；[name2]=`Reverse attack from the backside Upgrade`） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1；[growtype maximum level] `0 5 0 0 0 5`——剑魂/剑影各 5 级） | 同上 |
| 学习等级 | 55（**前置：技能 7 逆转反击 Lv5**，[pre required skill] `7 5`） | 同上 |
| 最高等级 | 10（TP 10 级） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 2 | 同上 |
| 类型 | passive（skill class 1） | 同上 |
| 一句话效果 | 反击攻击力 +10%/级（10 级 +100%）；被保护或发动反击时不受到伤害 | 同上 [explain ex] |
| 基础技 | 7 逆转反击（`007-HitBack.md`，⛔）；基础 skl [feature skill index]=144 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 007-HitBack.md）

### 2.1 TP 数据表解码（L21 向量法）

- static data：dungeon **`1 100 30000 7000`** / pvp `0 100 30000 7000`。
  - static[2]=30000：level property 向量 (2,2,×0.001) → **保护冷却时间 30 秒**（基础技 explain"每30秒一次保护"的数据面——注意基础 skl static 仅 `0`，**保护 CD 的数据实际挂在 Ex 表**）；
  - static[0]：dungeon 1 / pvp 0——疑"保护/反击免伤开关"（explain ex"被保护或发动反击时不会受到伤害"，pvp 不免伤），**推断**；
  - static[1]=100 / static[3]=7000 语义未考证（7000 与基础技满级 CD 同值，疑反击后 CD）。
- [level info] 2 列 ×70 行 = **基础表逐值复制**（`500 1260` → `500 6581`；col0=500 恒定同基础未解语义）。
- [special level up]（dungeon）：**`-1 1 % 10` = col1 反击攻击力 +10%/TP 级**；pvp `0`（PVP 无增量）。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 反击攻击力 +10%/级 | [special level up] col1 | 纯倍率：HitReaction.Damage ×(1+0.1×TP)，基础技可做时 ✅ 零成本 |
| 2 | 保护/反击免伤 | static[0]（推断） | 我们本无减伤公式，语义天然近似成立（受击端不结算） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | HitBackEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\HitBackEx.skl` | ✅ | TP 数据 |
| 基础 skl | HitBack.skl（[feature skill index] 144） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（基础档 F3 已证引擎内置；passive_skill case 表无 144，实测） | ⛔ | TP 消费在引擎 |
| 基础技文档 | 007-HitBack.md | 本目录 | ✅ | 触发链三缺口论证引用 |

## 4. 资源需求

TP 被动零资源（图标 SkillIcon.img #238/239，不做 UI）。缺失 img：**0**（反击动作/命中沿用基础技链，007 档已证资源完备）。

## 5. 实现方案草案（增量落地）

**随基础技 ⛔ 暂缓**——背击检测 + 受击触发窗口 + 受击管线钩子三缺口（007 §5/§8）是主因，与本 TP 无关。若基础技按 007 降级方案（普通背袭反击技）实现，本增量 = 反击 HitReaction.Damage 加一个 TP 倍率系数（配置常量注入），无新内容件。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| HitBackEx.skl | `.skl` 无子命令（含 **[special level up]** 节——E 类 TP 增量表，本批首见新节） | 手抄可行；`skl` 子命令立项时 **[special level up] 四元组（源/列/单位/步进）必须进输出** |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 反击触发链（背击→窗口→Z） | 随基础技 ⛔（007 档三缺口） | 等"受击反击族"立项 |
| 攻击力 +10%/级 | 无缺口（数值层） | 倍率常数 |
| 保护 CD 30s | 随触发链（窗口的冷却面） | 同上 |

## 8. 存疑与缺口上报

**未考证项**
1. static[0]=1/0 免伤开关推断；static[1]=100、static[3]=7000 语义。
2. Ex 的 [level info] 与基础表的引擎取用优先级（TP 习得后引擎读哪张表，未考证）。

**新缺口**：无新增系统级缺口。翻译工具：`.skl` 子命令（重复印证）；**[special level up] 新节上报（E 批共性，主循环汇总）**。
