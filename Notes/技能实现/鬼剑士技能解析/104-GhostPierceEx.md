# 强化 - 鬼连牙（GhostPierceEx）

> 技能ID 104 | 级别 E（TP 强化技） | 可实现性 🔶（=基础版 136 🔶；增量=单列攻击力 +10%/级，纯数值档） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 鬼连牙 | `skill\Swordman\GhostPierceEx.skl [name]` |
| 英文名 | GhostPierceEx（skl 文件名；无 [name2] 节——剑影族 Ex 通例） | 同上 |
| 职业 | 剑影（[skill fitness growtype] 0-5，但 [growtype maximum level] `5 0 0 0 0 5` 仅 0/5 位有值） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 |
| 最高等级 | 7（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 [special purchase cost] |
| 类型 | `[passive]` + [feature skill type] 1；skill class 5（剑影鬼技族）；[auto cooltime apply] 1 | 同上 |
| 前置 | 技能 136（鬼连牙）**Lv3**（本批唯一 3 级前置，其余多为 Lv5） | 同上 [pre required skill] |
| 一句话效果 | 增加鬼连牙的刺击攻击力（+10%/级） | 同上 [explain ex] |
| 基础技文档 | `136-ghostpierce.md`（🔶 基础刺击 ✅，拉拽/鬼步联动降级） | 本目录 |

## 2. 强化增量

### 2.1 数据源与消费链（实测）

- **镜像表**：[level info] 单列 `3862 → 30902` 与基础 136 攻击力列**逐值一致**（Lv1-70 全表）；[static data] `100 200` 同基础（col0=攻击盒/图像缩放 100%、col1=受击僵直倍率 200%，136 文档 nut 实证语义）。
- **消费链**：TP 通例三路实测零脚本引用（名字 grep / load_state / passive_skill）。基础技取数点 = 共享 PO case 45/46 的 `sq_GetBonusRateWithPassive(SKILL_GHOSTPIERCE, -1, 0, 1.0)`——**单列同时喂常规刺击（dword45）与鬼步终结刺击（dword46，2 倍尺寸）两路**，TP +10% 一并放大两路伤害。
- 反向链接：`ghostsword\ghostpierce.skl [feature skill index] 104`（实测）。
- pvp 段保留 [special level up] `+10%`——**决斗场也强化**（剑影族 TP 通例，与老一代技能相反）。

### 2.2 增量明细

| 增量项 | special level up 行 | 每 TP 级 | TP7 满级 | 说明 |
|---|---|---|---|---|
| 刺击攻击力（col0：3862→30902‰） | `-1 0 \`%\` 10` | +10% | **+70%** | 同时作用于常规段与鬼步终结段 |
| 刺击范围（static[0]=100%） | — 无增量行 | 不变 | 不变 | 仅面板显示 |
| 敌人僵直率（static[1]=200%） | — 无增量行 | 不变 | 不变 | 仅面板显示（136 文档实证僵直倍率语义） |

### 2.3 增量性质判定：纯数值档

单列伤害系数；范围/僵直率 static 无 TP 增量行——**强化只加攻击力，不动判定几何与控制时长**。拉拽、鬼步联动等基础版 🔶 降级点全部与 TP 无关。

### 2.4 资源增量

0 新 img / 0 新资源文件。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GhostPierceEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GhostPierceEx.skl` | ✅ 实测（260 行全读） | 镜像表 + TP 增量 |
| lst 条目 | ID 104 | `…\pvf\skill\swordmanskill.lst` 314-315 行 | ✅ 实测 | — |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无（实测） | 引擎折叠 |
| 取数点 | 共享 PO setcustomdata case 45/46 | `…\pvf\sqr\shared_passive_object\swordman\setcustomdata.nut` | ✅（136 文档实证 + 本批对位复核） | col0 单列双路消费 |
| 反向链接 | ghostpierce.skl [feature skill index] | `…\pvf\skill\Swordman\ghostsword\ghostpierce.skl` | ✅ 实测 =104 | — |
| 基础技文档 | 136-ghostpierce.md | 本目录 | ✅ | 继承源 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注册点。** 136 草案 `GhostPierceSkill.HitReaction.Damage = 120` 按 TP 缩放：
`Damage = 120 × (1 + 0.10 × TPLv)`（TP7 满级 = 204）；HitstunMs 800 / Kb 50 / Ly 150 / 判定盒全部不变。鬼步终结段（若随取消体系后续落地）同乘。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `GhostPierceEx.skl` | `.skl` 无子命令；[special level up] | 随 skl 子命令（本批统一设计输入） |

翻译缺口计 1 条（.skl 类型）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 攻击力 +10%/级 | 技能等级/TP 系统缺失（延后档） | 固定 TP0 基线或满级 204 |
| pvp 也强化 | 无 PVP 分流 | 不做（demo 全值统一） |
| 拉拽/鬼步联动（基础版降级点） | 与 TP 无关 | 沿用 136 §7 |

## 8. 存疑与缺口上报

**未考证项**
1. 面板显示的"刺击范围 100% / 僵直率 200%"是否会在高 TP 下被引擎另行放大（无增量行，倾向否）。
2. [auto cooltime apply] 1 的语义（镜像基础技字段，未考证，全族共性）。

**缺口上报**：无新系统级缺口、无新翻译节。
