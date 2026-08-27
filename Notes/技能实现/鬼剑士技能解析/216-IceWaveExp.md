# 强化-冰刃·波动剑（IceWaveExp）

> 技能ID 216 | 级别 E | 可实现性 ✅（冰柱数量 +3/级 = N 发弹体循环创建、攻击力 +15%/级 = 常数倍率，零新机制） | 分析日期 2026-08-22 | 批次 E3

> **与 E5 批 100 IceWaveEx（极冰·裂波剑，active）区分**：wave.nut/po_wavecut 的"强化版分支"（子状态 100 / 写包 id 125，
> 读技能 100 的数据）属 100 号主动技；**216 无任何脚本分支**（`wave\` 定点 grep `216`/`IceWaveExp` 零命中，实测）——TP 消费在引擎。

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 冰刃 · 波动剑 | `IceWaveExp.skl [name]` |
| 英文名 | IceWaveExp（skl 文件名；[name2]=`冰刃波动剑 Upgrade`） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4；[growtype maximum level] `0 0 0 0 1 0`——仅阿修罗） | 同上 |
| 学习等级 | 55（**前置：技能 21 冰刃·波动剑 Lv5**，[pre required skill] `21 5`） | 同上 |
| 最高等级 | **3**（TP 3 级封顶——E3 批唯一非 10 级者） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 4 | 同上 |
| 类型 | passive（skill class 0；[skill under cooltime effect] 空节随基础） | 同上 |
| 一句话效果 | 冰柱生成数量 +3/级（7→16 @TP3）；攻击力 +15%/级（+45% @TP3） | 同上 [explain ex] |
| 基础技 | 21 冰刃·波动剑（`021-IceWave.md`，✅ 示范文档）；基础 skl [feature skill index]=216 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 021-IceWave.md）

### 2.1 TP 数据表解码（L21 向量法，5 向量全明）

- static data `100 300`（pvp `100 400`）= 基础同值（语义基础档未展开，延续）。
- [level info] 7 列 ×70 行 = **基础表逐值复制**（`200 7 105 714 220 31 1800` 起，实测逐值一致）。
- [level property] 五向量与基础 skl 逐值相同：(-2,3,×1.0) 魔攻=col3、(-1,1,×1.0) 数量=col1、(-1,4,×0.1) 冰冻机率=col4、(-1,6,×0.001) 冰冻时长=col6、(-1,5,×1.0) 冰冻Lv=col5。
- [special level up]（dungeon）：**`-1 1 + 3` = col1 冰柱数量 +3/级（绝对加算）；`-1 3 % 15` = col3 攻击力 +15%/级**；pvp 无增量。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 冰柱数量 +3/级（7 → 16 @TP3） | [special level up] col1 加算 | `OnCast` 循环 `ctx.CreateBullet` ×N（y 偏移铺开或间隔连发），多弹各自独立命中去重——✅ 零新机制 |
| 2 | 攻击力 +15%/级 | [special level up] col3 | 每发 Damage ×(1+0.15×TP)——✅ 常数倍率 |

冰冻概率/时长/等级（col4/5/6）**不随 TP 变**。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | IceWaveExp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\IceWaveExp.skl` | ✅ | TP 数据（含 [skill preloading image] ×5，与基础 skl 同清单） |
| 基础 skl | IceWave.skl（[feature skill index] 216） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无 216 分支） | `…\sqr\character\swordman\wave\wave.nut`（只读技能 100——E5 批对象） | ⛔（对 216 而言） | TP 消费在引擎 |
| 基础技文档 | 021-IceWave.md | 本目录 | ✅ | 格式标准 + IceWaveSkill/Bullet 草案引用 |

## 4. 资源需求

TP 被动零新增（冰波视觉 icewaveex\1-6.ani 系 img 归基础档可选级未提取；空占位 icewave.ani 无需求——021 §4）。缺失 img：**0**（增量自身）。

## 5. 实现方案草案（增量落地）

基础档 021 §5 的 `IceWaveBullet : BulletDefinition`（穿透 + ProcBuffId 冰冻）原样适用：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 冰柱数量 | col1 = 7 + 3×TP（TP3 = 16） | 1 + TP（TP3 = 4 发，y 偏移 ±0.3 铺开——demo 观感折中） |
| 每发伤害 | col3 魔攻 ×(1+0.15×TP) | 固定 40 ×(1+0.15×TP)（TP3 = 58） |
| 冰冻概率/时长 | col4×0.1 / col6 ms（不随 TP） | 25% / 3.5s（021 demo 值不变） |

实现形态：`IceWaveSkill.OnCast` 按 `TpLevel` 循环 N 次 `ctx.CreateBullet(BulletIds.IceWave, y 偏移)`；无新内容件/注册点。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| IceWaveExp.skl | `.skl` 无子命令（7 列 + [special level up] 2 行 + [skill preloading image] ×5） | 手抄可行；skl 子命令纳入 TP 增量表（加算 + 百分比两种 [special level up] 形态都在本技出现） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 16 发冰柱多段齐射 | 无缺口（多弹创建 + 各自去重天然支持；基础档"多段延后"结论仅指单弹对同目标反复结算——本技是多弹非多段，不撞） | demo 折中 4 发 |
| 攻击力 +15%/级 | 无缺口 | 常数倍率 |
| 判定盒随大小档缩放 / hold 微控 / 蓄力版 | 随基础档（对象缩放延后 / hold 缺失 / 蓄力输入缺失） | 同 021 §7 |

## 8. 存疑与缺口上报

**示范文档勘误（任务指定核验，按 L21 修正记档）**：021-IceWave.md §5 关键数值表的"基准 + 成长"读法与 L21 向量法有出入，按 L21 修正：

| 021 §5 原文 | L21 修正（本档实测基础 skl + Ex skl 同表互证） |
|---|---|
| "冰冻概率 列3：基准 4% + 0.1%/级（推断）" | **col4 ×0.1 直读**：Lv1 = 220 → **22.0%**（随基础技能等级从表取值；"4% 基准"无表源） |
| "冰冻时长 列5：基准 5s + 1.0s/级（推断）" | **col6 ×0.001 直读**：Lv1 = 1800ms → **1.8s**（同上） |
| "小冰柱数量 列1：基准 1 + 1/级" | **col1 直读**：Lv1 = **7 个**（非 1） |

021 §2.2 的列号引用（prob=列4/10、lv=列5、time=列6）本来就对，仅 §5 的"基准值"行废弃；§5 demo 建议值（25%/3.5s/数量 1）作为**演示常数**仍然成立，只是不再声称源自表基准。主循环回填 021 时引用本表。

**未考证项**：static `100 300` 语义（基础档延续）；Ex 表与基础表引擎取用优先级（逐值复制，无冲突）。

**新缺口**：无新增系统级缺口。翻译工具：`.skl` 子命令（重复印证）；[special level up] 节（E 批共性上报）。
