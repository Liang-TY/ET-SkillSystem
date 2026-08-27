# 强化 - 上挑（UpperSlashEx）

> 技能ID 215 | 级别 E（TP 强化技） | 可实现性 🔶（=基础版 046 🔶；增量纯伤害系数，并入固定伤害常量，等级缩放延后档） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 上挑 | `skill\Swordman\UpperSlashEx.skl [name]` |
| 英文名 | UpperSlashEx（skl 文件名；[name2]="Upper Slash Upgrade"） | 同上 |
| 职业 | 鬼剑士全系（[skill fitness growtype] 0-5） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；[growtype maximum level] `5 5 5 5 5 5`） | 同上 |
| TP 消耗 | 1/级 | 同上 [special purchase cost] |
| 类型 | `[passive]` + [feature skill type] 1；[weapon effect type] physical（随基础技） | 同上 |
| 前置 | 技能 46（上挑）Lv5 | 同上 [pre required skill] |
| 指令 | Z（镜像基础技显示） | 同上 [command] |
| 一句话效果 | 提升上挑的攻击力（+8%/级）；浮空力不受强化 | 同上 [explain ex] + 数据实情 |
| 基础技文档 | `046-UpperSlash.md`（🔶） | 本目录 |

## 2. 强化增量

### 2.1 数据源与消费链（实测，含本批最直接的折叠证据）

- **镜像表**：[level info] 4 列（`120 350 100 30` → `840 1167 100 210`）与基础技 046 四列解码**逐值一致**；[static data] `100 -250 100 -50` 同基础。
- **消费链零脚本引用**（TP 通例三路实测：名字 grep 零命中 / load_state 无 pushState / passive_skill 无 case）。
- **折叠证据（结构性，本批最有力的一条）**：基础技消费函数组 `attack.nut onSetState_upperslash_swordman`（046 文档实证）取数方式为
  `sq_GetBonusRateWithPassive(46, -1, 0)`（col0 攻击倍率）与 `sq_GetPowerWithPassive(46, -1, 3)`（col3 固定物攻）——
  **与 UpperSlashEx [special level up] 的两条增量行（col0/col3 各 +8%）精确对位**。"WithPassive" 访问器族即引擎折叠 TP 增量的取数点，全 pvf 脚本零感知。
- 反向链接：`UpperSlash.skl [feature skill index] 215`（实测）。

### 2.2 增量明细

| 增量项 | special level up 行 | 每 TP 级 | TP10 满级 | 基础文档对位 |
|---|---|---|---|---|
| 攻击倍率（col0：120→840%） | `-1 0 \`%\` 8` | +8% | **+80%** | §2.2 damage = sq_GetBonusRateWithPassive |
| 固定物攻（col3：30→210） | `-1 3 \`%\` 8` | +8% | **+80%** | §2.2 damageBonus = sq_GetPowerWithPassive |
| 浮空力（col1：350→1167） | — 无增量行 | **不变** | 不变 | explain 只说"攻击力增加"——数据实情：TP 不动浮空手感 |
| col2（恒 100，语义未考证） | — | 不变 | 不变 | 沿用 046 存疑 3 |

pvp 段无 [special level up]——决斗场不强化。

### 2.3 增量性质判定：纯数值档（只加伤害、不动浮空）

对实现而言是**本批最干净的增量**：基础版 046 草案的 `HitReaction{Damage=70, LaunchY=350}` 中只有 Damage 一个数随 TP 缩放，LaunchY/盒/帧全部不动。

### 2.4 资源增量

0 新 img / 0 新资源文件。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | UpperSlashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\UpperSlashEx.skl` | ✅ 实测（195 行全读） | 镜像表 + TP 增量 |
| lst 条目 | ID 215 | `…\pvf\skill\swordmanskill.lst` 312-313 行 | ✅ 实测 | — |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无（实测） | 引擎折叠 |
| 折叠消费证据 | attack.nut 340-407 行函数组 | `…\pvf\sqr\character\swordman\attack\attack.nut` | ✅（046 文档实证 + 本批对位复核） | col0/col3 取数点 |
| 反向链接 | UpperSlash.skl [feature skill index] | `…\pvf\skill\Swordman\UpperSlash.skl` | ✅ 实测 =215 | — |
| 基础技文档 | 046-UpperSlash.md | 本目录 | ✅ | 继承源 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注册点。** 046 草案 `UpperSlashSkill` 的 Damage 常量按 TP 缩放：

| 参数 | 基础版（046 草案） | TP 并入 |
|---|---|---|
| Damage | 70（固定） | `70 × (1 + 0.08 × TPLv)`；无 TP 系统 demo：保持 70（TP0 基线）或直接取 TP10 满级 **126** |
| LaunchY / KnockbackX / 帧盒 / CD | 350 / 100 / F2-F3 / 2000ms | 全部不变 |

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `UpperSlashEx.skl` | `.skl` 无子命令；[special level up] 四元组 | 随 skl 子命令（本批统一设计输入） |

翻译缺口计 1 条（.skl 类型）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 攻击力 +8%/级 | 技能等级/TP 系统缺失（延后档"等级数值缩放"） | 固定满级值或 TP0 基线 |
| pvp 不强化 | 无 PVP 分流 | 不做 |
| 浮空力不强化（数据实情） | 无 | 反而省事——demo 手感与基础版完全一致 |

## 8. 存疑与缺口上报

**未考证项**
1. pvp 段无增量行是否等价"决斗场不强化"（结构性推断：special level up 仅 dungeon 节出现）。
2. [growtype maximum level] `5×6` 与 max 10 的分段语义（全批共性存疑）。
3. col2 恒 100 语义沿用 046 存疑（TP 亦不触及）。

**缺口上报**：无新系统级缺口；本技能是"TP 折叠进 WithPassive 访问器"的最佳教学样本（§2.1），建议总览引用。
