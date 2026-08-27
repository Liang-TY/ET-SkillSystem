# 强化 - 幻鬼 : 一闪（OneSlashVSEx）

> 技能ID 113 | 级别 E（TP 强化技） | 可实现性 🔶（=基础版 135 🔶；增量=单列攻击力 +10%/级，纯数值档） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 幻鬼 : 一闪 | `skill\Swordman\OneSlashVSEx.skl [name]` |
| 英文名 | OneSlashVSEx（skl 文件名；无 [name2] 节） | 同上 |
| 职业 | 剑影（[growtype maximum level] `5 0 0 0 0 5` 仅 0/5 位有值） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 |
| 最高等级 | 7（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 |
| 类型 | `[passive]` + [feature skill type] 1；skill class 5；[auto cooltime apply] 1 | 同上 |
| 前置 | 技能 135（幻鬼 : 一闪）Lv5 | 同上 [pre required skill] |
| 一句话效果 | 增加幻鬼一闪的突进攻击力（+10%/级） | 同上 [explain ex] |
| 基础技文档 | `135-Oneslashvs.md`（🔶 弹体直译可，取消/幻鬼记忆降级） | 本目录 |

## 2. 强化增量

### 2.1 数据源与消费链（实测）

- **镜像表**：[level info] 单列 `2131 → 17055`（Lv1→70）与基础 135 攻击力列**逐值一致**；[static data] dungeon `450` / pvp `350` = 基础突进距离 px（PO setstate 实证消费），TP 不触及。
- **消费链**：TP 通例三路实测零脚本引用。基础技取数点 = 共享 PO `setstate.nut case 61/state 10` 的 `sq_GetBonusRateWithPassive(SKILL_ONESLASHVS, -1, 0, 1.0)`——TP +10% 由引擎在该访问器内折叠。
- 反向链接：`ghostsword\oneslashvs.skl [feature skill index] 113`（实测）。
- pvp 段保留 [special level up] `+10%`（剑影族 TP 也强化决斗场）。
- 本 skl 出现空节 `[skill explain icon]`（无内容）——工具按空节忽略即可。

### 2.2 增量明细

| 增量项 | special level up 行 | 每 TP 级 | TP7 满级 | 说明 |
|---|---|---|---|---|
| 一闪攻击力（col0：2131→17055‰） | `-1 0 \`%\` 10` | +10% | **+70%** | Bullet 伤害 |
| 突进距离（static 450px） | — | 不变 | 不变 | 100ms 匀速突进节奏不变 |
| 命中反应（push75/lift200） | —（在 .atk） | 不变 | 不变 | atk 不随 TP 缩放 |

### 2.3 增量性质判定：纯数值档

基础版 135 草案已把伤害落在 `OneSlashVsGhostBullet.HitReaction.Damage`——TP 增量就是这一个数的乘法。"剑术中无动作瞬发 / 幻鬼位置接力"两个基础降级点与 TP 无关。

### 2.4 资源增量

0 新 img / 0 新资源文件（幻鬼视觉共用基础技 PO 动画族）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | OneSlashVSEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\OneSlashVSEx.skl` | ✅ 实测（254 行全读） | 镜像表 + TP 增量 |
| lst 条目 | ID 113 | `…\pvf\skill\swordmanskill.lst` 316-317 行 | ✅ 实测 | — |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无（实测） | 引擎折叠 |
| 取数点 | 共享 PO setstate.nut case 61 | `…\pvf\sqr\shared_passive_object\swordman\setstate.nut` | ✅（135 文档实证 + 本批对位复核） | col0 消费 |
| 反向链接 | oneslashvs.skl [feature skill index] | `…\pvf\skill\Swordman\ghostsword\oneslashvs.skl` | ✅ 实测 =113 | — |
| 基础技文档 | 135-Oneslashvs.md | 本目录 | ✅ | 继承源 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注���点。** 135 草案 `OneSlashVsGhostBullet.HitReaction.Damage = 120` → `120 × (1 + 0.10 × TPLv)`（TP7 满级 = 204）；Speed 45 / TotalTimeMs 100 / 穿透 / Kb75 / Ly200 全部不变。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `OneSlashVSEx.skl` | `.skl` 无子命令；空节 [skill explain icon] | 空节忽略；随 skl 子命令 |

翻译缺口计 1 条（.skl 类型）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 攻击力 +10%/级 | 技能等级/TP 系统缺失（延后档） | TP0 基线或满级 204 |
| 消散段视觉（Bullet 无 ViewEndAnimId） | 框架小项（135 §8 已记） | 沿用基础版简化 |

## 8. 存疑与缺口上报

**未考证项**：无新增（基础版 4 项存疑全部沿用，TP 侧无新疑点）。

**缺口上报**：无。
