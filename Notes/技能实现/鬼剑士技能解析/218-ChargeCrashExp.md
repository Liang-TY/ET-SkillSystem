# 强化-破军升龙击（ChargeCrashExp）

> 技能ID 218 | 级别 E | 可实现性 ✅（肩击/上斩各 +10%/级为两段 Damage 常数倍率；下捶蓄气上限随武器系统缺失砍，同基础档） | 分析日期 2026-08-22 | 批次 E3

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 破军升龙击 | `ChargeCrashExp.skl [name]` |
| 英文名 | ChargeCrashExp（skl 文件名；[name2]=`Charge Crash Upgrade`） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1；[growtype maximum level] `0 5 0 0 0 0`——仅剑魂 5 级，E3 批唯一无双职业档） | 同上 |
| 学习等级 | 55（**前置：技能 68 破军升龙击 Lv5**，[pre required skill] `68 5`） | 同上 |
| 最高等级 | 10（TP 10 级） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 2 | 同上 |
| 类型 | passive（skill class 1；[weapon effect type] `[physical]`） | 同上 |
| 一句话效果 | 肩击/上斩攻击力各 +10%/级 | 同上 [explain ex] |
| 基础技 | 68 破军升龙击（`068-ChargeCrash.md`，✅）；基础 skl [feature skill index]=218 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 068-ChargeCrash.md）

### 2.1 TP 数据表解码（L21 向量法，3 向量全明）

- static data `700 -1500 300 400`（pvp `700 -1500 300 600`，仅 [3] 差异）= 基础同值。
- [level info] 2 列 ×70 行 = **基础表逐值复制**（`288 432` → `2344 3516`，实测逐值一致）。
- [special level up]：**`-1 0 % 10`（col0 肩击 +10%/级）+ `-1 1 % 10`（col1 上斩 +10%/级）**。⚠ 本 skl 结构变体：[special level up] 块位于 [dungeon] 块**外**（[/dungeon] 与 [pvp] 之间），无 pvp 对应块——skl 子命令解析需容错此位置。
- [level property] 三向量：(-1,0,×1.0) 肩击、(-1,1,×1.0) 上斩、**(3,3,×0.001) → static[3]=400×0.001 = 0.4s 下捶击蓄气时间上限**——补解基础档未解的 static[3]（400）：**下捶蓄气上限 0.4 秒**（不随 TP 变；pvp 600→0.6s）。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 肩击攻击力 +10%/级 | [special level up] col0 | 冲撞段 HitReaction.Damage ×(1+0.1×TP)——✅ 常数倍率 |
| 2 | 上斩攻击力 +10%/级 | [special level up] col1 | `ChargeCrashUpperArea` Damage ×(1+0.1×TP)——✅ 同上 |
| 3 | （列名勘误）col0/col1 命名"肩击/上斩"确认基础档推断（col0=冲撞段、col1=上挑段） | 模板 | 无实现动作 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ChargeCrashExp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ChargeCrashExp.skl` | ✅ | TP 数据 |
| 基础 skl | ChargeCrash.skl（[feature skill index] 218） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（基础档 chargecrash.nut 仅 mod 钩子壳；passive_skill case 表无 218，实测） | ⛔ | TP 消费在引擎 |
| 基础技文档 | 068-ChargeCrash.md | 本目录 | ✅ | 三段式/撞敌停驻草案引用 |

## 4. 资源需求

TP 被动零新增（up-slash/dash 等必需 4 张归基础档未入库）。缺失 img：**0**（增量自身）。

## 5. 实现方案草案（增量落地）

基础档 068 §5 的 `ChargeCrashSkill`（冲撞帧盒 + 撞敌停驻）+ `ChargeCrashUpperArea`（lift 400 上挑区）原样适用，本 TP 只改两处：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 冲撞段伤害 | col0 288‰ ×(1+0.1×TP) | 90 ×1.5 = 135（TP5 档） |
| 上挑段伤害 | col1 432‰ ×(1+0.1×TP) | 120 ×1.5 = 180（TP5 档） |
| 下捶蓄气上限 | static[3] = 0.4s（本档补解） | 不做（下捶段随武器系统缺失砍，同基础档） |

无新内容件/注册点；TP 等级配置常量注入。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ChargeCrashExp.skl | `.skl` 无子命令（2 列 + [special level up] 2 行；**[special level up] 块位于 [dungeon] 外**——结构变体） | 手抄可行；skl 子命令解析需兼容该块两种挂载位置 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 下捶段（蓄气 0.4s 上限 + 砸地） | 武器/精通系统缺失（随基础档） | 砍（static[3]=0.4s 已解出记档，武器系统落地后取值） |
| 两段攻击力 +10%/级 | 无缺口 | 常数倍率 |
| （基础档既有：65534 取消窗口/撞墙/超越极限联动） | 随基础档 | 见 068 §7 |

## 8. 存疑与缺口上报

**未考证项**
1. [special level up] 挂载位置变体（[dungeon] 外）是否影响引擎语义（疑纯排版，未考证）。
2. static[0]/[1]/[2]（700/-1500/300）语义仍随基础档未解（本档仅补解 [3]=400 蓄气上限）。

**新缺口**：无新增系统级缺口。**基础档补解 1 条**（static[3] = 下捶蓄气上限 0.4s；col0/col1 命名确认——主循环回填 068 时引用本档 §2.1）。翻译工具：`.skl` 子命令（重复印证）；[special level up] 节结构变体（E 批共性上报）。
