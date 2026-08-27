# 强化-猛龙断空斩（RapidMoveSlashEx）

> 技能ID 152 | 级别 E（TP 强化被动） | 可实现性 🔶（=基础版 072 🔶——中段/终段攻击力 +10%/级为常数倍率，直接并入 demo Damage；无新行为） | 分析日期 2026-08-22 | 批次 E5

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 猛龙断空斩 | `RapidMoveSlashEx.skl [name]` |
| 英文�� | RapidMoveSlashEx（skl 文件名；[name2]=`Dragon Air Break Upgrade`） | 同上 |
| 职业 | 剑魂（[growtype maximum level] `0 5 0 0 0 0`——本批唯一单职业档；[skill fitness growtype]=1） | 同上 |
| 学习等级 | 65（[required level range] 5）；前置：技能 72 猛龙断空斩 Lv1 | 同上 |
| 最高等级 | 10（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 |
| 类型 | passive（skill class 1；[feature skill type] 1） | 同上 [type] |
| 一句话效果 | 猛龙断空斩的攻击力增加：每级 +10%（中段/终段两路） | 同上 [explain ex] |
| 基础技文档 | 072-RapidMoveSlash.md（🔶 多段前冲连斩主干可做） | 本目录 |

## 2. 强化增量（对照 072-RapidMoveSlash.md）

### 2.1 数据表形态：整表镜像 + static 裁尾

- [level info] 2 列 × 69 行逐值镜像（python diff 实测 diff=0；col0 265→1953 中段、col1 529→3906 终段——072 §1 同值）。
- static：dungeon `4 1 2000 -5000 300 -1000`（基础 7 值裁掉尾 0）/ pvp `3 1 2000 -5000 400 -1000`——除裁尾外逐值同（含 pvp 段距 400）。
- [special level up]（dungeon）：`-1 0 % 10` + `-1 1 % 10`（两路攻击力各 +10%/级）；pvp 两行全 0。

### 2.2 增量明细

| 增量项 | 数据源 | 每 TP 级 | TP10 |
|---|---|---|---|
| 中段攻击力（col0） | `-1 0 % 10` | +10% | ×2 |
| 终段攻击力（col1） | `-1 1 % 10` | +10% | ×2 |
| 斩击次数（static[0]=4）/段距（static[4]=300） | — 无增量 | 不变 | 4 次 / 300px |

### 2.3 增量性质判定：纯数值档（本批最简）

两路常数倍率、无行为/几何维度——R7-E2 总量表"纯数值档"标准件。

### 2.4 资源增量

**0 新增**（图标 SkillIcon.img 槽 248/249）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | RapidMoveSlashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\RapidMoveSlashEx.skl` | ✅（243 行全读） | 镜像表 + TP 增量 |
| lst 条目 | ID 152 | `…\pvf\skill\swordmanskill.lst` 391-392 行 | ✅ | — |
| 反向链接 | RapidMoveSlash.skl [feature skill index]=152 | 同目录 | ✅ 实测 | 双向互指 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（grep rapidmoveslashex 零命中） | ⛔ | 引擎消费 |
| 基础技文档 | 072-RapidMoveSlash.md | 本目录 | ✅ | 数值/缺口继承源 |

## 4. 实现方案增量（并入 072 §5 草案）

072 的 `RapidMoveSlashSkill`（段机）+ `RapidMoveSlashFinalArea` 原样适用，TP 只改两处 Damage：

| 项 | 基础 demo 值 | TP10 并入后 |
|---|---|---|
| 冲斩段伤害 | 100 | 100 ×2 = **200** |
| 终结段伤害 | 150 | 150 ×2 = **300** |

无 TP 系统期间直接按满级定值（或维持基础值，实现期决策）；无新内容件/注册点。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| RapidMoveSlashEx.skl | `.skl` 无子命令（2 列窄表，手抄 4 值即全） | 随 skl 子命令（常驻） |

翻译缺口 1 条（.skl，常驻）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 两段 +10%/级 | 无缺口（常数倍率） | 直译 |
| pvp 不强化 | 无 PVP 分流 | 不做 |
| （基础档既有：段间方向追加/霸体/光剑感电等） | 随基础档 072 §7 | 见该档 |

## 8. 存疑与缺口上报

**未考证项**
1. Ex static 裁尾（基础 7 值 `… -1000 0` → Ex 6 值 `… -1000`）——疑面板表裁掉引擎专用尾参 0（无语义差异），未考证。

**缺口上报**：无新系统级缺口。

**给轮间经验**：072 §8-1 基础档未解 static 五值（1/2000/-5000/-1000）本档无新证据（TP 不触及位移参数）——冲阵物理参数仍属引擎黑箱。
