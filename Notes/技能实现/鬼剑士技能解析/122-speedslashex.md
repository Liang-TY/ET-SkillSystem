# 强化 - 鬼连斩（speedslashex）

> 技能ID 122 | 级别 E（TP 强化技） | 可实现性 ✅（=基础版 127 ✅；三斩各 +10%/级纯数值档；注意与另一强化技 118 上挑段并存不冲突） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 鬼连斩 | `skill\Swordman\speedslashex.skl [name]`（本技 skl 文件名全小写，与基础技 speedslash 同风格） |
| 英文名 | speedslashex（skl 文件名；无 [name2] 节） | 同上 |
| 职业 | 剑影（[growtype maximum level] `5 0 0 0 0 5`） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 |
| 最高等级 | 7（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 |
| 类型 | `[passive]` + [feature skill type] 1；skill class 5；[auto cooltime apply] 1 | 同上 |
| 前置 | 技能 127（鬼连斩）Lv5 | 同上 [pre required skill] |
| 一句话效果 | 增加鬼连斩三次斩击的攻击力（各 +10%/级） | 同上 [explain ex] |
| 基础技文档 | `127-speedslash.md`（✅ 基础三连斩直接可实现） | 本目录 |
| 同族区分 | **另一强化技 118（SKILL_SPEEDSLASHUPPER，追加第 4 段上挑）是 SP 技能不是 TP**——122 只加本体三斩数值，两者并存不冲突 | 127 文档 §2.2/§7 |

## 2. 强化增量

### 2.1 数据源与消费链（实测）

- **镜像表**：[level info] 3 列（`516 619 929` → `4104 4966 7484`）与基础 127 三列**逐值一致**；[static data] dungeon `100` 同基础（斩击范围%，nut 实证作攻击盒缩放）。
- **消费链**：TP 通例三路实测零脚本引用。基础技取数点 = 共享 PO onkeyframeflag id42 三次切 atk 时的 `sq_GetBonusRateWithPassive(SKILL_SPEEDSLASH, -1, col, 1.0)`（col=0/1/2 ↔ F3/F9/F13 三段）——三条增量行精确对位。
- 反向链接：`ghostsword\speedslash.skl [feature skill index] 122`（实测）。
- pvp 段保留 3 条 [special level up] `+10%`；pvp [static data] `100 100` 双值与基础单值 100 不同（疑冗余，记档）。

### 2.2 增量明细

| 增量项 | special level up 行 | 每 TP 级 | TP7 满级 | 说明 |
|---|---|---|---|---|
| 第 1 斩（col0：516→4104‰） | `-1 0 \`%\` 10` | +10% | **+70%** | F3 段 |
| 第 2 斩（col1：619→4966‰） | `-1 1 \`%\` 10` | +10% | **+70%** | F9 段 |
| 第 3 斩（col2：929→7484‰） | `-1 2 \`%\` 10` | +10% | **+70%** | F13 段 |
| 斩击范围（static=100%） | — | 不变 | 不变 | |

### 2.3 增量性质判定：纯数值档（作用面=本体三斩）

- TP 只放大本体三连斩；118 的上挑段（子状态 1/3）是独立 SP 强化技，伤害不走 127 的三列（其 skl 不在本批）——**122 不影响上挑段**。
- 鬼步联动终结段（dword 41）伤害读**鬼步 126 列 0**而非 127 的列——**122 也不影响联动段**（那部分由 134-SpiritMoveEx 放大）。作用面三分工清晰：122=本体三斩 / 118=追加段 / 134=鬼步+联动段。

### 2.4 资源增量

0 新 img / 0 新资源文件。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | speedslashex.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\speedslashex.skl` | ✅ 实测（263 行全读） | 镜像表 + TP 增量 |
| lst 条目 | ID 122 | `…\pvf\skill\swordmanskill.lst` 324-325 行 | ✅ 实测 | — |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无（实测） | 引擎折叠 |
| 取数点 | 共享 PO onkeyframeflag.nut id 42 | `…\pvf\sqr\shared_passive_object\swordman\onkeyframeflag.nut` | ✅（127 文档实证 + 本批对位复核） | 三段切 atk 取列 |
| 反向链接 | speedslash.skl [feature skill index] | `…\pvf\skill\Swordman\ghostsword\speedslash.skl` | ✅ 实测 =122 | — |
| 基础技文档 | 127-speedslash.md | 本目录 | ✅ | 继承源 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注册点。** 127 草案 `SpeedSlashSkill` 三段 Damage（80/95/140）按 TP 缩放：`各段 = 基线 × (1 + 0.10 × TPLv)`（TP7 满级 = 136/162/238）；帧号 const 3/9/13、段间 ClearHitTargets、盒、CD 5000 全部不变。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `speedslashex.skl` | `.skl` 无子命令；[special level up] | 随 skl 子命令 |

翻译缺口计 1 条（.skl 类型）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 三斩各 +10%/级 | 技能等级/TP 系统缺失（延后档） | TP0 基线或满级 ×1.7 |
| pvp static 双值 `100 100` | 数据冗余疑（未考证） | 忽略 |
| 上挑段/联动段（非本技作用面） | 属 118 / 134 | 见 §2.3 三分工 |

## 8. 存疑与缺口上报

**未考证项**
1. pvp [static data] `100 100` 双值语义（dungeon 单值 100；疑冗余或 pvp 专属第二参数）。
2. 118 上挑段的伤害列归属（其 skl 不在本批，作用面结论由 127 文档结构性事实推出）。

**缺口上报**：无新系统级缺口；记录"三强化技作用面三分工"族级事实（122 本体 / 118 追加段 / 134 联动段，总览可引）。
