# 强化-嗜魂之手（GrabBlastBloodExp）

> 技能ID 165 | 级别 E | 可实现性 ⛔（随基础技抓取系统缺失；四列 +10%/级为纯数值，基础技按"定身连招"深简化落地时增量直落） | 分析日期 2026-08-22 | 批次 E3

> **与 E7 批 102 GrabBlastBloodEx（灭魂之手，active）是两个不同技能**：165 = 技能 31 的 TP 强化（本档）；
> 102 = 复用 GRABHAND 状态 26 的独立主动技（grabhand.nut 的 `skill==102` 失败分支属它，不属本档）。
> 基础档 031 §2.1"技能 102（嗜魂之手 TP 强化版）"的表述按 lst 对表**纠偏**：102 非 TP 强化，165 才是。

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 嗜魂之手 | `GrabBlastBloodExp.skl [name]` |
| 英文名 | GrabBlastBloodExp（skl 文件名；[name2]=`Bloodlust Upgrade`——与基础技 [name2] 相同） | 同上 |
| 职业 | 狂战士（[skill fitness growtype]=3；[growtype maximum level] `0 0 0 5 0 0`——仅狂战 5 级） | 同上 |
| 学习等级 | 55（**前置：技能 31 嗜魂之手 Lv5**，[pre required skill] `31 5`） | 同上 |
| 最高等级 | 10（TP 10 级） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 2 | 同上 |
| 类型 | passive（skill class 2） | 同上 |
| 一句话效果 | 嗜魂之手攻击力（抓取段/喷发段/对出血段）与力量上升值各 +10%/级 | 同上 [explain ex] |
| 基础技 | 31 嗜魂之手（`031-GrabBlastBlood.md`，⛔）；基础 skl [feature skill index]=165 双向链接证 | 两 skl 实测 |
| 沿用基础节 | [shake screen] 2 400 / [durability decrease rate] 20 / [command] →→+Z / [steel learning skill] `1 50 200 4`（TP 体系节，语义未考证） | 同上 |

## 2. 强化增量（对照 031-GrabBlastBlood.md）

### 2.1 TP 数据表解码（L21 向量法）

- static data `1200 500 600 4` = 基础同值（pvp `1200 500 600 2`，仅 [3] 差异同基础）。
- [level info] 6 列 ×70 行 **不是基础表复制**（重要差异，见 §8）：Ex 起 `220 314 10000 46 264 20000` vs 基础 `352 502 30000 46 528 30000`——攻击力列（col0/1/4）整体更低、力量时长（col2/col5）30000ms→10000/20000ms。时长列仍恒定不随级变。
- [special level up]（dungeon，四行）：`-1 0 % 10`（col0 抓取/吸血段）、`-1 1 % 10`（col1 喷发段）、`-1 3 % 10`（col3 力量值）、`-1 4 % 10`（col4 对出血段）——**四列各 +10%/级**；col2/col5 时长列不加。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 抓取段/喷发段/对出血段攻击��� +10%/级 | [special level up] col0/1/4 | 纯倍率：各段 Damage ×(1+0.1×TP)——基础技可做时 ✅ 零成本 |
| 2 | 力量上升值 +10%/级 | col3 | ⛔ 属性消费链——砍 |
| 3 | 对出血敌人增伤/延时 | col4/col5 | ⛔ Buff 查询门面（R1-A3）——随基础档跳过 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GrabBlastBloodExp.skl | `E:\Projects\cs\dnfororigin\pvf源码提取部分\pvf\skill\Swordman\GrabBlastBloodExp.skl` | ✅ | TP 数据 |
| 基础 skl | GrabBlastBlood.skl（[feature skill index] 165） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\grabhand\`（定点 grep `165`/`GrabBlastBloodExp` 零命中，实测）；passive_skill case 表无 165 | ⛔ | TP 消费在引擎；grabhand.nut 的 skill==102 分支属灭魂之手 |
| 基础技文档 | 031-GrabBlastBlood.md | 本目录 | ✅ | 抓取三子系统拆解引用 |

## 4. 资源需求

TP 被动零新增（Grab.ani/喷发 PO/img 族归基础档：必需 4 张未入库）。缺失 img：**0**（增量自身）。

## 5. 实现方案草案（增量落地）

随基础技 ⛔ 暂缓（抓取/目标控制系统三子系统，031 §5 表格为立项依据）。若基础技按 031 §5"定身连招"深简化实现（GrabHoldBuff 定身 + 终结喷发 Area），本增量 = 两段 HitReaction.Damage ×(1+0.1×TP) 配置倍率，无新内容件；力量增量砍。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| GrabBlastBloodExp.skl | `.skl` 无子命令（6 列 + [special level up] 4 行 + [steel learning skill]/[shake screen] 节） | 手抄可行；skl 子命令纳入 TP 增量表与 [steel learning skill]（TP 体系节，语义未考证） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 抓取演出/可抓性判定 | 基础档 ⛔（Grab 系） | 031 定身近似 |
| 力量 +10%/级 | 属性消费链（R1-A4） | 砍 |
| 对出血敌人增伤 | Buff 查询门面（R1-A3） | 跳过 |
| Ex 表与基础表数值差异（攻击力低/时长短） | 未考证（§8） | 实现取基础表为准（基础技引擎消费主表） |

## 8. 存疑与缺口上报

**未考证项**
1. **Ex 表不是基础表复制**（col0 220 vs 基础 352 @Lv1；col2/5 10000/20000 vs 30000/30000）——独立平衡表或旧版残留，引擎取用优先级未知。基础档 031 §1"col2=30000 恒定"的基础表读法不受影响（本档实测基础表确为 30000 恒定）。
2. [steel learning skill] `1 50 200 4` 语义（基础 007 档同节未解，延续）。

**新缺口**：无新增系统级缺口。**基础档纠偏 1 条**（102 ≠ TP 强化，主循环回填 031 时引用本档头部说明）。翻译工具：`.skl` 子命令（重复印证）；[special level up] 节（E 批共性上报）。
