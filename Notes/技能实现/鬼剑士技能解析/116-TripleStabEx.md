# 强化 - 鬼影三击剑（TripleStabEx）

> 技能ID 116 | 级别 E（TP 强化技） | 可实现性 ✅（=基础版 112 ✅；两列攻击力各 +10%/级，纯数值档） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 鬼影三击剑 | `skill\Swordman\TripleStabEx.skl [name]` |
| 英文名 | TripleStabEx（skl 文件名；[name2]="三击剑 UpGrade"——中文名2） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype] 2；[growtype maximum level] `0 0 5 0 0 0`） | 同上 |
| 学习等级 | 55（[required level range] 5） | 同上 |
| 最高等级 | 10（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 |
| 类型 | `[passive]` + [feature skill type] 1；skill class 3 | 同上 |
| 前置 | 技能 112（鬼影三击剑）Lv5 | 同上 [pre required skill] |
| 一句话效果 | 增加三击剑三段刺击的攻击力（+10%/级） | 同上 [explain ex] |
| 基础技文档 | `112-TripleStab.md`（✅ 直接，帧驱动三窗口） | 本目录 |

## 2. 强化增量

### 2.1 数据源与消费链（实测）

- **镜像表**：[level info] 2 列（`146 678` → `1730 8074`）与基础 TripleStab.skl **逐值一致**（本批实测基础 skl 首/末档对表；顺带修正 112 文档对 Lv70 的估算——见 §8 回填）。
- [static data] dungeon `2000`（pvp `1500`）同基础（语义未���证，沿用 112 存疑 1）。
- **消费链**：TP 通例三路实测零脚本引用；基础技纯引擎内置（F3，连参照脚本都无），取数在引擎内直接读 skl 列——TP 增量同样引擎折叠。
- [level property] 模板解码（L21）：下段←col0、上段←**col0（与下段同列，镜像基础技模板）**、中段←col1——TP 两条增量行正好覆盖两列。
- 反向链接：`TripleStab.skl [feature skill index] 116`（实测）。
- pvp 段**无** [special level up]——决斗场不强化。

### 2.2 增量明细

| 增量项 | special level up 行 | 每 TP 级 | TP10 满级 | 说明 |
|---|---|---|---|---|
| 刺击下段/上段（col0：146→1730‰） | `-1 0 \`%\` 10` | +10% | **+100%** | 刺 1/刺 2（两段共用列） |
| 刺击中段/末段（col1：678→8074‰） | `-1 1 \`%\` 10` | +10% | **+100%** | 刺 3（finish atk 大击退段） |
| static 2000 | — | 不变 | 不变 | 语义未考证 |

### 2.3 增量性质判定：纯数值档

两列伤害系数；三段帧窗口、两份 atk 反应分工（主版/finish 版 push400）全部不动。基础版 ✅ 结论无增量压力。

### 2.4 资源增量

0 新 img / 0 新资源文件。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | TripleStabEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\TripleStabEx.skl` | ✅ 实测（235 行全读） | 镜像表 + TP 增量 |
| lst 条目 | ID 116 | `…\pvf\skill\swordmanskill.lst` 328-329 行 | ✅ 实测 | — |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无（实测） | 引擎折叠（基础技亦引擎内置） |
| 镜像对照 | TripleStab.skl level info 首末档 | `…\pvf\skill\Swordman\TripleStab.skl` | ✅ 实测（146 678 → 1730 8074） | §8 回填依据 |
| 反向链接 | TripleStab.skl [feature skill index] | 同上 | ✅ 实测 =116 | — |
| 基础技文档 | 112-TripleStab.md | 本目录 | ✅ | 继承源 + 回填对象 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注册点。** 112 草案：

| 参数 | 基础版 | TP 并入（满级 TP10 定值 ×2.0） |
|---|---|---|
| 刺 1/刺 2 Damage | 55（技能 HitReaction） | **110** |
| 刺 3 Finish Area Damage | 90 | **180** |
| Kb 30/400、Ly 70/200、帧窗口 | 不变 | 不变 |

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `TripleStabEx.skl` | `.skl` 无子命令；[special level up] | 随 skl 子命令 |

翻译缺口计 1 条（.skl 类型）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 两列攻击力 +10%/级 | 技能等级/TP 系统缺失（延后档） | TP0 基线或满级 ×2.0 |
| 鬼影步门禁/暗属性（基础版简化点） | 与 TP 无关 | 沿用 112 §7 |

## 8. 存疑与缺口上报

**未考证项**
1. static `2000`（pvp 1500）语义沿用 112 存疑 1。
2. [growtype maximum level] `0 0 5 0 0 0` 与 max 10 分段语义（全批共性）。

**回填 112 文档**：其 §1 记"col0 Lv70=约 3167"为外推误差——**实测基础 skl 与 Ex 镜像双证 Lv70 = col0 1730 / col1 8074**（112 文档的等级区间值估算按此修正）。

**缺口上报**：无新系统级缺口。
