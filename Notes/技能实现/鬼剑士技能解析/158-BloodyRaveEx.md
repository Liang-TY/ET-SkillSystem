# 强化-嗜魂封魔斩（BloodyRaveEx）

> 技能ID 158 | 级别 E（TP 强化技） | 可实现性 ⛔（=基础版 079 ⛔ 位移他人+抓取双缺口；TP 增量全为数值/吸附速度调档，无新增缺口） | 分析日期 2026-08-22 | 批次 E6

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 嗜魂封魔斩 | `BloodyRaveEx.skl [name]` |
| 英文名 | BloodyRaveEx（skl 文件名；[name2]=`Bloody Rave Upgrade`） | 同上 |
| 职业 | 狂战士（[skill fitness growtype]=3，L17） | 同上 |
| 学习等级 | 65（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；[growtype maximum level] `0 0 0 5 0 0`——狂战单角色至多 5 级） | 同上 |
| TP 消耗 | 2/级 | 同上 [special purchase cost] |
| 前置 | 技能 79（嗜魂封魔斩）**Lv1**（本批 10 技前置全为 Lv1 档，E1-E4 批多见 Lv5 档） | 同上 [pre required skill] `79 1` |
| 类型 | passive（[feature skill type] 1；skill class 2 随基础） | 同上 |
| 一句话效果 | 吸附速度 +10%/级、攻击力 +10%/级、**按住期间每秒 HP 消耗 +10%/级**（代价面同步上升） | 同上 [explain ex] |
| 基础技 | 79 嗜魂封魔斩（`079-BloodyRave.md` ⛔）；基础 skl [feature skill index]=158 双向链接（实测） | 两 skl 实测 |

## 2. 强化增量（对照 079-BloodyRave.md）

### 2.1 数据侧（L21 向量法 + E 类通用解码法）

- [level info]（71 行）与基础 skl **逐字节相同**（python 全表比对，实测）；[static data] dungeon/pvp 两节也逐值相同——纯镜像，无覆写。
- [level property] 8 向量与基础逐值相同（含源 **-4** 的 col5 出血攻击力——079 首见未解源，延续记档）。
- [special level up]（dungeon，5 行）：

| 行 | 目标 | 每 TP 级增量 |
|---|---|---|
| `2 2 % 10` | **static[2]=180** | +10%（吸附速度参数 1） |
| `3 3 % 10` | **static[3]=55** | +10%（吸附速度参数 2） |
| `-1 0 % 10` | level col0（物理攻击力 2314%→…） | +10% |
| `-1 1 % 10` | level col1（最高物理攻击力 10428%→…） | +10% |
| `-1 6 % 10` | level col6（每秒 HP 消减 2.3→…） | **+10%（消耗量增加，explain 明示）** |

- pvp 段 5 行全 0（决斗场不强化，老一代 TP 通例）。
- 脚本消费：`bloodyrave\bloodyrave.nut` 定点 grep `158`/`BloodyRaveEx` 零命中（实测）——TP 折算全在引擎（079 本就 F3b 半内置）。

### 2.2 增量明细

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 吸附速度 +10%/级（static[2]/[3] 双槽同增） | [special level up] static 行 | 吸附=位移他人（缺失档）——增量随缺口空转 |
| 2 | 攻击力 +10%/级（col0/col1 两列） | [special level up] level 行 | 终结区 Damage ×(1+0.1×TP)——✅ 常数倍率 |
| 3 | 每秒 HP 消耗 +10%/级 | col6 行 | ConsumeCasterHp 数值 ×(1+0.1×TP)——✅ |

**回填 079（§8 存疑 1）**：static[2]=180 / static[3]=55 应为**吸附速度双槽**（TP 行与 explain"吸附速度"对位）——原推断"250=拉拽速度量级"的修正候选：速度参数在 [2]/[3]，[5]=250 语义仍不明。吸附速度参数成对出现与 213 无双波（[11]/[12] 双槽）同构。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodyRaveEx.skl（266 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodyRaveEx.skl` | ✅ 全节实测 | 镜像表 + TP 增量 |
| 基础 skl | BloodyRave.skl（[feature skill index] 158） | 同目录 | ✅ 实测比对 | 双向链接 |
| 脚本 | —（无 158 分支） | `…\pvf\sqr\character\swordman\bloodyrave\bloodyrave.nut` | ⛔ 零命中（实测） | TP 消费在引擎 |
| 基础技文档 | 079-BloodyRave.md | 本目录 | ✅ | 继承源（⛔ 定身近似草案） |

## 4. 资源需求

**0 新 img / 0 新文件**（TP 不换动画不换 PO；视觉随基础档 079 §4 的 7 必需 + 9 可选）。

## 5. 实现方案草案（增量落地）

零新内容件/注册点。基础版若按 079 §5"定身+斩击"近似立项后并入：

| 参数 | 基础版（079 草案） | TP 并入（建议 TP5 满级定值） |
|---|---|---|
| 终结斩伤害 | 固定 260（col0-col1 折中） | ×(1+0.1×TP)，TP5=×1.5 → 390 |
| 吸附窗 HP 消耗 | 共扣 5 | ×(1+0.1×TP)，TP5=7.5 |
| 吸附速度 | HoldBuff 800ms 假吸附 | 无落点（位移他人缺失），维持 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BloodyRaveEx.skl | `.skl` 无子命令（7 列镜像表 + [special level up] 5 行 + static 12 值 ×2） | 手抄可行；[special level up] 增量表纳入 skl 子命令（E 批共性） |

翻译缺口计 1 条（.skl 类型）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 吸附速度 +10%/级 | **缺失档：位移他人门面**（随基础 ⛔） | 随基础版队列 |
| 攻击力/HP 消耗 +10%/级 | 无缺口 | 常数倍率直译 |
| pvp 不强化 | 无 PVP 分流 | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. static[2]=180 / static[3]=55 的精确语义（速度/加速度/频率——仅知 TP 与 explain 的"吸附速度"对位；两槽为何需同 +10% 未证）。
2. col6 +10% 与 167 OutRageBreakEx 的 col3 **-10%** 方向相反——% 步长符号按字面（本技消耗增加、167 消耗减少），引擎对"消耗列"是否存在统一反向处理未证（本例 explain 与数据同向，倾向字面直读）。

**回填**：079 §8 存疑 1 的 static 槽语义修正候选（见 §2.2）。
**新缺口**：无。翻译工具：`.skl` 子命令（常驻）。
