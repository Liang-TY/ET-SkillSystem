# 强化 - 格挡（GuardEx）

> 技能ID 163 | 级别 E（TP 强化技） | 可实现性 ⛔（=基础版 001 ⛔；三路增量全部落在被"受击伤害管线钩子"阻断的吸收机制上，强化随基础技生死） | 分析日期 2026-08-22 | 批次 E2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 格挡 | `skill\Swordman\GuardEx.skl [name]` |
| 英文名 | GuardEx（skl 文件名；[name2]="Guard Upgrade"） | 同上 [name2] 实测 |
| 职业 | 鬼剑士全系（[skill fitness growtype] 0-5） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 [required level] |
| 最高等级 | 3（TP；[growtype maximum level] `1 1 1 1 1 1`，分段语义未考证） | 同上 [maximum level] |
| TP 消耗 | 2/级（[special purchase cost]） | 同上 |
| 类型 | `[passive]` + [feature skill type] 1（TP 强化技，非可施放被动） | 同上 [type] |
| 前置 | 技能 1（格挡）Lv5（[pre required skill]） | 同上 |
| 指令 | 无独立指令（[command] 为基础技 ↓↓+X 的镜像显示） | 同上 [command] |
| 一句话效果 | 提升格挡的物理/魔法伤害吸收率与冲击波限制发动时间 | 同上 [explain ex] |
| 基础技文档 | `001-Guard.md`（⛔ 受击伤害管线钩子，本批缺口第 4 消费方） | 本目录 |

## 2. 强化增量

### 2.1 数据源与消费链（三路实测）

- **镜像表**：GuardEx 的 [level info]（5 列 × 70 档）与 [static data]（`500 400 100 400 3000`）= 基础技 Guard 表**逐值镜像**（首档 `200 200 40 15 10` / 末档 `700 0 80 80 100` 与 001 文档解码一致）——用途为 TP 面板显示插值，不新增数值。
- **增量定义全部在 [special level up]**（行格式：`源 索引 格式串 步长`；源 -1/-2=level info 列、正值=static data 槽，L21 同族读法）。
- **零脚本消费（TP 通例，本批十技一次实测）**：① `sqr\character\swordman\` + `JG_SwordMan\` 递归 grep 全部 Ex 名 → 零命中；② `swordman_load_state.nut` 无 pushState（名/ID 双查）；③ `passive_skill_swordman.nut` 无 case 163/164/215/104/113/114/134/122/115/116。**TP 增量由引擎在取数访问器族（`sq_GetLevelData` / `sq_Get*WithPassive`）内折叠**，全 pvf 脚本零感知——结构性证据见 215-UpperSlashEx §2.1（增量列与脚本取数列精确对位）。
- **反向链接**：基础技 `Guard.skl` `[feature skill index] 163`（实测）。

### 2.2 增量明细（[explain ex] + special level up 交叉印证）

| 增量项 | special level up 行 | 每 TP 级 | TP3 满级 | explain 对应 |
|---|---|---|---|---|
| 物理吸收率（基础 col2：40→80%） | `-1 2 \`%\` 5` | +5% | **+15%**（55→95%） | "物理吸收伤害值增加量 : 5%" |
| 魔法吸收率（基础 col3：15→80%） | `-1 3 \`%\` 10` | +10% | **+30%**（45→110%，是否封顶 100 未考证） | "魔法吸收伤害值增加量 : 10%" |
| 冲击波限制发动时间（static[1]=400，单位未考证） | `1 1 \`%\` 10` | +10 | 430 | "冲击波限制发动时间增加量 : 10%" |
| 被击退时间（col1） | — 无增量行 | 不变 | 不变 | — |

pvp 段增量行全 0（`-1 0 \`%\` 0` / `-1 3 \`+\` 0`）——**决斗场不强化**（老一代技能 TP 通例，见批次总评）。

### 2.3 增量性质判定：纯数值档

三路增量（物理%/魔法%/冲击波窗口）全是乘加系数，**无新行为、无新判定、无新资源**。但三者的宿主数值全部是基础版被 ⛔ 阻断的"正面吸收 + 冲击波触发"机制本身——强化在管线落地前无独立实现物。

### 2.4 资源增量

**0 新 img / 0 新 .ani / 0 新 .atk / 0 新 .obj**（attackinfo 目录 grep Ex 名零命中、passiveobject 无对应 .obj，实测）——TP 技纯数据件；图标在共用 `SkillIcon.img` 槽 284/285（UI 层，不构成提取需求）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GuardEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GuardEx.skl` | ✅ 实测（175 行全读） | 镜像表 + TP 增量定义 |
| lst 条目 | ID 163 | `…\pvf\skill\swordmanskill.lst` 308-309 行 | ✅ 实测 | — |
| 注册行 | — | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无（TP 引擎内置） | 名/ID 双查零命中 |
| 被动注册 | — | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ⛔ 无 case | 实测 |
| 消费点 | 引擎取数访问器族（折叠 TP） | —（引擎 C++ 侧） | 引擎内置 | §2.1 |
| 反向链接 | Guard.skl [feature skill index] | `…\pvf\skill\Swordman\Guard.skl` | ✅ 实测 =163 | 基础↔强化互指 |
| 基础技文档 | 001-Guard.md | 本目录 | ✅ | 数值/缺口继承源 |

## 4. 实现方案增量（并入基础技草案）

**零新内容件、零新注册点**（无 SkillId/AnimId/Area/Buff 需求）。基础版按 001 §5 落地（受击管线钩子立项后）时，TP 增量即参数层：

| 参数 | 基础版（001 草案） | TP 增量并入后（建议直用 TP3 满级定值，无 TP 系统时不做区分） |
|---|---|---|
| 物理吸收 | 70%（固定） | 70% + 15% = **85%** |
| 魔法吸收 | 70% | 70% + 30% = **100%**（钳顶） |
| 冲击波窗口 | guardwave 判定窗 | +10%×3（单位未考证，直译按比例放大窗口时长） |

demo 建议：无 TP/技能等级系统（R6-C1 记档缺口）期间**保持基础版数值不动**；TP 系统立项后按 `base + step × TPLv` 直译——两行公式即成。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `GuardEx.skl` | `.skl` 无子命令（常驻缺口）；TP 特有节 [special level up] / [special purchase cost] / [feature skill type] / [growtype maximum level] | 随 `skl` 子命令立项一并设计：special level up 四元组（源/索引/格式/步长）应译成结构化增量字段——**本批 10 份 Ex 的核心设计输入** |
| （无 .ani/.als/.atk/.obj） | — | 无缺口 |

翻译缺口计 1 条（.skl 类型），无新节缺口（全部并入 skl 子命令设计面）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| TP 逐级 +5%/+10% 吸收 | 技能等级/TP 系统缺失（R6-C1"技能等级系统"族）；吸收本体在**缺失档：受击伤害管线钩子** | 管线落地���整体 ⛔；落地后先固定满级值 |
| 冲击波窗口 +10%/级 | 同上 + 窗口单位未考证 | 满级 430 一次定值 |
| 决斗场不强化 | 无 PVP 分流系统 | 不做（demo 无 PVP） |
| MP 仅开始扣一次（基础版行为） | MP 系统延后 | 沿用基础版结论 |

## 8. 存疑与缺口上报

**未考证项**
1. 冲击波限制发动时间 static[1]=400 的单位与引擎消费方式（ms / 10ms 档）。
2. 魔法吸收 TP3 时 110% 是否被引擎封顶 100%。
3. [growtype maximum level] `1 1 1 1 1 1` 与 [maximum level] 3 的分段累进语义（疑按觉醒段位累加，全批共性存疑）。
4. 增量是否只在"正面格挡"内生效（随基础版 IsFrontOf 未考证项同命运）。

**缺口上报**：无新系统级缺口（TP 数值消费与基础版共用"受击管线钩子"+"技能等级系统"两条既有缺口；TP 增益形态作为"技能等级系统"缺口的第 3 类消费方记档——前两类：35 封印解除 +1 级 / 174 基础精通伤害链）。

**翻译工具缺口**：`.skl` 子命令（常驻）；[special level up] 节设计输入（本批新见，随 skl 子命令）。
