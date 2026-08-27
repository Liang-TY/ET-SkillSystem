# 强化 - 鬼步（SpiritMoveEx）

> 技能ID 134 | 级别 E（TP 强化技） | 可实现性 🔶（=基础版 126 🔶；增量=多段攻击力单列 +10%/级，纯数值档——注意其放大面横跨所有"鬼步联动终结段"） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 鬼步 | `skill\Swordman\SpiritMoveEx.skl [name]` |
| 英文名 | SpiritMoveEx（skl 文件名；无 [name2] 节） | 同上 |
| 职业 | 剑影（[growtype maximum level] `5 0 0 0 0 5`） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 |
| 最高等级 | 7（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 |
| 类型 | `[passive]` + [feature skill type] 1；skill class 5；[auto cooltime apply] 1 | 同上 |
| 前置 | 技能 126（鬼步）Lv5 | 同上 [pre required skill] |
| 一句话效果 | 增加鬼步突进多段攻击的攻击力（+10%/级） | 同上 [explain ex] |
| 基础技文档 | `126-spiritmove.md`（🔶 姿势窗口+位移可表达，无敌帧/联动延后） | 本目录 |

## 2. 强化增量

### 2.1 数据源与消费链（实测）

- **镜像表**：[level info] 单列 `254 → 2038`（Lv1→70）与基础 126 多段攻击力列**逐值一致**；[static data] `400 120 3 300`（突进距离/攻击范围%/段数/无敌时间）同基础，TP 不触及。
- **消费链**：TP 通例三路实测零脚本引用。基础技取数点 = 共享 PO setcustomdata case 41 的 `sq_GetBonusRateWithPassive(SKILL_SPIRITMOVE, -1, 0)`。
- 反向链接：`ghostsword\spiritmove.skl [feature skill index] 134`（实测）。
- pvp 段保留 [special level up] `+10%`（剑影族决斗场也强化）。

### 2.2 增量明细

| 增量项 | special level up 行 | 每 TP 级 | TP7 满级 | 说明 |
|---|---|---|---|---|
| 多段攻击力（col0：254→2038‰） | `-1 0 \`%\` 10` | +10% | **+70%** | 3 跳每跳同乘 |
| 突进距离 400px / 范围 120% / 段数 3 / 无敌 0.3s | — 无增量行 | 全部不变 | 不变 | 手感参数全不动 |

### 2.3 增量性质判定：纯数值档（但放大面横跨整族）

关键结构性事实（126/127/136 三份基础文档交叉实证）：**鬼步 PO（dword 41）的 col0 也是所有"鬼步联动终结段"的伤害源**——鬼连斩(127)/鬼连牙(136)/白鬼一闪(119)/断头台(122 断头)/裂魂乱舞(剑舞) 在鬼步姿势下按技能键触发的终结段伤害均读 `126 列 0`。因此 **SpiritMoveEx 的 +10%/级同时放大：鬼步本体 3 跳 + 全族联动终结段伤害**（而 122-speedslashex 只放大鬼连斩本体三斩、不影响联动段——两技作用面互补不重叠）。

### 2.4 资源增量

0 新 img / 0 新资源文件。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SpiritMoveEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SpiritMoveEx.skl` | ✅ 实测（267 行全读） | 镜像表 + TP 增量 |
| lst 条目 | ID 134 | `…\pvf\skill\swordmanskill.lst` 320-321 行 | ✅ 实测 | — |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无（实测） | 引擎折叠 |
| 取数点 | 共享 PO setcustomdata.nut case 41 | `…\pvf\sqr\shared_passive_object\swordman\setcustomdata.nut` | ✅（126 文档实证 + 本批对位复核） | col0 消费（本体+联动段） |
| 反向链接 | spiritmove.skl [feature skill index] | `…\pvf\skill\Swordman\ghostsword\spiritmove.skl` | ✅ 实测 =134 | — |
| 基础技文档 | 126-spiritmove.md | 本目录 | ✅ | 继承源 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注册点。** 126 草案鬼步段每跳伤害（单跳 120 或 3×60 手工多段）按 TP 缩放：`每跳 = 基线 × (1 + 0.10 × TPLv)`（TP7 满级 ×1.7）。若联动终结段（等技能取消体系）后续落地，其伤害同乘本系数（§2.3）。无敌 300ms / 位移 400px/500ms 不变（无敌帧本身仍是无敌帧系统缺口，与 TP 无关）。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `SpiritMoveEx.skl` | `.skl` 无子命令；[special level up] | 随 skl 子命令 |

翻译缺口计 1 条（.skl 类型）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 多段攻击力 +10%/级 | 技能等级/TP 系统缺失（延后档） | TP0 基线或满级 ×1.7 |
| 无敌 300ms | 无敌帧系统缺失（126 §8 已记，与 TP 无关） | 沿用基础版跳过 |
| 联动段放大 | 技能取消体系缺失（联动段本身未落地） | 联动落地时把系数一并带入 |

## 8. 存疑与缺口上报

**未考证项**：无新增（基础版 3 项存疑沿用）。

**缺口上报**：无新缺口；记录"TP 放大面横跨鬼步本体+全族联动段"的族级事实（总览可引）。
