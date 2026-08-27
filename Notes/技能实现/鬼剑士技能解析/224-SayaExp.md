# 强化-冰霜之萨亚（SayaExp）

> 技能ID 224 | 级别 E（TP 强化技，**萨亚的真 TP**） | 可实现性 ✅（=基础版 036 ✅；增量=领域时长 +1s/级 的 TotalTimeMs 调档 + 光环半径覆写档可选——零新机制） | 分析日期 2026-08-22 | 批次 E6

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 冰霜之萨亚 | `SayaExp.skl [name]` |
| 英文名 | SayaExp（skl 文件名；[name2]=`Summon Saya UpGrade`） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype]=2）；[growtype maximum level] `0 0 5 0 0 5`——鬼泣/剑影各 5 | 同上 |
| 学习等级 | 65（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；单角色至多 5 级） | 同上 |
| TP 消耗 | 2/级 | 同上 [special purchase cost] |
| 前置 | 技能 36（冰霜之萨亚）Lv1 | 同上 [pre required skill] `36 1` |
| 类型 | passive（[feature skill type] 1；skill class 3 召唤系） | 同上 |
| 一句话效果 | 萨亚领域持续时间 +1 秒/级（TP5 = 10s→15s） | 同上 [explain ex] |
| 基础技 | 36 冰霜之萨亚（`036-Saya.md` ✅）；基础 skl [feature skill index]=224 双向链接（实测） | 两 skl 实测 |

> **与 96（冰晶之萨亚，SayaEx.skl，active 二觉替换技）划清**（E4 已纠偏，本批任务再确认）：PO `SayaEx.obj`（20063）/`SayaExIce.obj`（20064，passiveobject.lst:11258-11260 实测）属技能 96；**224 沿用基础 Saya.obj（20013）不变**，无新增 PO。

## 2. 强化增量（对照 036-Saya.md）

### 2.1 数据侧（E 类通用解码法 + static 覆写变体）

- [level info]（71 行 ×5 列，末行 `3136 10000 744 169 6947`）与基础**逐字节相同**；[level property] 6 向量逐值相同（时长/间隔/魔攻/冰冻率/冰冻时长/冰冻Lv）。
- **[static data] 覆写变体（本批 4 例之一，单槽）**：
  - dungeon：基础 `450 250 70 1000` → Ex `450 350 70 1000`——**static[1] 光环半径 250→350（+100px）**，explain 未提及半径变化（164 Khazan 350 同款存疑：引擎 TP 态换用 Ex static 则半径 +40%，倾向面板数据不采纳，无直证）。
  - pvp：Ex `450 250 70 2000`——半径不覆写（pvp 不强化通例），但**间隔 1000→2000**（基础 skl 无 pvp static 节，Ex 新增；pvp 降频 2s/跳，顺带补记）。
- [special level up]（dungeon，**仅 1 行**——本批最简）：`-1 1 % 10` → **col1 持续时间 +10%/级**。
  - col1 基础恒 10000ms → +1000ms/级 = **+1 秒/级，与 explain"持续时间+1秒"完美对位**——`%` 型增量作用于时间列的直读样本（E 批首例）。pvp 无该节。
- 脚本消费：萨亚族纯引擎内置（036 §2.1 无 nut 无注册），TP 更无脚本面。

### 2.2 增量明细

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 持续时间 +1s/级（10s→15s @TP5） | col1 +10%/级 | SayaZone `TotalTimeMs = 10000×(1+0.1×TP)`——✅ 纯调档 |
| 2 | （存疑）光环半径 250→350 | static[1] 覆写 | `HalfExtents 2.5→3.5`（可选采纳；explain 未提，建议不采纳） |

伤害/冰冻概率/时长/等级、伤害间隔（1s）均不随 TP 变。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SayaExp.skl（226 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SayaExp.skl` | ✅ 全节实测 | 镜像表 + static 覆写 + TP 增量 |
| 基础 skl | Saya.skl（[feature skill index] 224） | 同目录 | ✅ 实测比对 | 双向链接 |
| 脚本 | —（纯引擎内置，036 已证无 nut） | `…\sqr\character\swordman\` | ⛔ 无 | TP 消费在引擎 |
| PO 定义 | Saya.obj（20013，**沿用基础不变**） | `…\passiveobject\character\swordman\saya.obj` | ✅（036 已析） | 六相位+attack info |
| 易混 PO | SayaEx.obj（20063）/SayaExIce.obj（20064） | 同目录 | ✅ lst 实测（**属技能 96，非本文**） | 防混记档 |
| 基础技文档 | 036-Saya.md | 本目录 | ✅ | 继承源（SayaZone Area 草案） |

## 4. 资源需求

**0 新 img / 0 新文件**（224 无自有视觉；萨亚六相位+冰蓝染色随基础档 036 §4 的必需 3）。

## 5. 实现方案草案（增量落地）

零新内容件/注册点，并入 036 §5 草案：

| 参数 | 基础版（036 草案） | TP 并入（建议 TP5 定值） |
|---|---|---|
| 领域时长 | TotalTimeMs 10000 | **15000**（+1000×TP 直译） |
| 伤害间隔/每跳伤害/冰冻 | 1000ms / 40 / 9% | 不变 |
| 光环半径 | HalfExtents 2.5 | 不变（350 覆写档不采纳） |

时长延长 = Tick 跳数 10→15，总伤自然 +50%——TP 的实际收益即"多 5 跳"。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| SayaExp.skl | `.skl` 无子命令（5 列镜像 + static 覆写 + 1 行增量） | 手抄可行；并入 skl 子命令缺口 |

翻译缺口计 1 条（.skl 类型）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 持续时间 +1s/级 | 无缺口 | TotalTimeMs 调档 |
| 光环半径疑 250→350 | 未考证（Khazan 同款） | 不采纳 |
| 冰冻 Lv 对抗 / 水属性 / 读条/消耗 | 随基础档（036 §7） | 同 036 |

## 8. 存疑与缺口上报

**未考证项**
1. static[1] 250→350 覆写的引擎消费（半径 +40% vs 面板数据——explain 未提半径，与 164 KhazanEx static[1] 250→350 **数值完全相同**，疑同族模板残留）。
2. pvp static 间隔 2000（Ex 新增 pvp 节）是否 pvp 实际采用（demo 无 pvp）。

**新缺口**：无。翻译工具：`.skl` 子命令（常驻）。
