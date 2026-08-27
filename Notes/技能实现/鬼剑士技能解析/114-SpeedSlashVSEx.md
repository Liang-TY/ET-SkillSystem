# 强化 - 幻鬼 : 连斩（SpeedSlashVSEx）

> 技能ID 114 | 级别 E（TP 强化技） | 可实现性 ✅（=基础版 137 ✅；四斩各 +10%/级纯数值——"追加斩击"开关在本 pvf 为 0 值死配置，不构成增量） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 幻鬼 : 连斩 | `skill\Swordman\SpeedSlashVSEx.skl [name]` |
| 英文名 | SpeedSlashVSEx（skl 文件名；无 [name2] 节） | 同上 |
| 职业 | 剑影（[growtype maximum level] `5 0 0 0 0 5`） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 |
| 最高等级 | 7（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 |
| 类型 | `[passive]` + [feature skill type] 1；skill class 5；[auto cooltime apply] 1 | 同上 |
| 前置 | 技能 137（幻鬼 : 连斩）**Lv3** | 同上 [pre required skill] |
| 一句话效果 | 增加幻鬼连斩四段斩击的攻击力（各 +10%/级） | 同上 [explain ex] |
| 基础技文档 | `137-speedslashvs.md`（✅ 五段连斩直接可实现） | 本目录 |

## 2. 强化增量

### 2.1 数据源与消费链（实测）

- **镜像表**：[level info] 4 列（`632 844 1052 1688` → `5060 6748 8436 13328`）与基础 137 四列**逐值一致**；[static data] `100 0` 同基础（实测 speedslashvs.skl dungeon static = `100 0`）。
- **消费链**：TP 通例三路实测零脚本引用。基础技取数点 = 共享 PO onkeyframeflag 五次切 atk 时的 `sq_GetBonusRateWithPassive(SKILL_SPEEDSLASHVS, -1, col, 1.0)`（col=0/1/2/3；第 5 击砸地复用 col3）——TP 四条增量行与四列精确对位。
- 反向链接：`ghostsword\speedslashvs.skl [feature skill index] 114`（实测）。
- pvp 段保留 4 条 [special level up] `+10%`。

### 2.2 增量明细

| 增量项 | special level up 行 | 每 TP 级 | TP7 满级 | 说明 |
|---|---|---|---|---|
| 第 1 斩（col0） | `-1 0 \`%\` 10` | +10% | **+70%** | |
| 第 2 斩（col1） | `-1 1 \`%\` 10` | +10% | **+70%** | |
| 第 3 斩（col2） | `-1 2 \`%\` 10` | +10% | **+70%** | |
| 第 4 斩 + 砸地终结（col3 复用） | `-1 3 \`%\` 10` | +10% | **+70%** | 终结击同乘 |
| 斩击范围（static[0]=100%） | — | 不变 | 不变 | |
| **第 1/2 斩次数增加（static[1]）** | **无增量行** | **不变 = 0** | **0（关）** | 见下 |

### 2.3 增量性质判定：纯数值档；"追加斩击"开关死配置（重要回填）

[level property] 模板含一行 `第1次、第2次斩击次数增加 : <int> [0为关，1为开]` ← 向量 `(1, 1, 1.0)` = **static[1] = 0**，且四条 TP 增量行均不触及该槽——**本 pvf 中该开关恒为关**（与 F5 族"static 尾槽=追加段开关常被 0 值死配置关闭"完全同款，R5-B6）。

**回填 137 文档存疑项 1**：其记载"state 14 终结变体插段（int1>0 触发）疑由强化版（记为技能 318）置 1"——实测：
1. 强化版真实 ID = **114**（非 318）；
2. 其 static[1]=0 且 TP 增量不写该槽 → **共享 PO state 14（loopbody_add 插段）在本 pvf 永不触发，判死配置**，基础版"state10→11→(14)→12→13"主线按无 14 分支实现即可（137 文档 §2.3 本就如此实现，结论不变、依据补齐）。

### 2.4 资源增量

0 新 img / 0 新资源文件。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SpeedSlashVSEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SpeedSlashVSEx.skl` | ✅ 实测（269 行全读） | 镜像表 + TP 增量 + 死配置开关 |
| lst 条目 | ID 114 | `…\pvf\skill\swordmanskill.lst` 318-319 行 | ✅ 实测 | — |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无（实测） | 引擎折叠 |
| 取数点 | 共享 PO onkeyframeflag.nut case 62 | `…\pvf\sqr\shared_passive_object\swordman\onkeyframeflag.nut` | ✅（137 文档实证 + 本批对位复核） | 五段切 atk 取列 |
| 死配置对照 | speedslashvs.skl static `100 0` | `…\pvf\skill\Swordman\ghostsword\speedslashvs.skl` | ✅ 实测 | §2.3 |
| 反向链接 | speedslashvs.skl [feature skill index] | 同上 | ✅ 实测 =114 | — |
| 基础技文档 | 137-speedslashvs.md | 本目录 | ✅ | 继承源 + 回填对象 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注册点。** 137 草案 `SpeedSlashVsSkill` 时间线（斩1-4 统一 Damage 100 + 终结区 180）按 TP 缩放：
统一档 `Damage = 100 × (1 + 0.10 × TPLv)`（TP7 满级 = 170）、终结区 `180 × 1.7 = 306`；命中时刻 260/500/860/1280/1740ms、段间 ClearHitTargets、盒尺寸全部不变。**不实现 state 14 追加段**（死配置，见 §2.3）。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `SpeedSlashVSEx.skl` | `.skl` 无子命令；[special level up]；模板含开关行 `[0为关，1为开]` | 随 skl 子命令——level property 模板串可含开关语义文本，翻译时按占位符数对位即可 |

翻译缺口计 1 条（.skl 类型）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 四斩各 +10%/级 | 技能等级/TP 系统缺失（延后档） | TP0 基线或满级统一 ×1.7 |
| （数据模型中的）第 1/2 斩次数 +1 | 本 pvf 死配置（static=0 无 TP 行），无需实现 | 不做；若未来开服数据开启，则撞"段结构切换"（帧时刻表重排），届时再评估 |
| pvp 也强化 | 无 PVP 分流 | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. state 14 死配置结论基于双证据（static[1]=0 + TP 无行触及），引擎侧是否另有开关消费未直证——置信高，留档。
2. [growtype maximum level] `5 0 0 0 0 5` 与 max 7 的分段语义（全批共性）。

**缺口上报**：无新系统级缺口；**回填 137 存疑项 1**（强化版 ID 318 → 114；state 14 判死配置）。
