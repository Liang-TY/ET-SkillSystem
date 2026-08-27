# 强化-血气之刃（BloodSwordEx）

> 技能ID 168 | 级别 E（TP 强化技） | 可实现性 ✅（=基础版 103 ✅；增量纯数值两路倍率，零新机制零新资源） | 分析日期 2026-08-22 | 批次 E6

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 血气之刃 | `BloodSwordEx.skl [name]` |
| 英文名 | BloodSwordEx（skl 文件名；[name2]=`Blood Sword Upgrade`） | 同上 |
| 职业 | 狂战士（[skill fitness growtype]=3）；**[growtype maximum level] `0 0 0 5 0 5`——第 6 位开放剑影(growtype5) TP**（本批唯一跨职业开放 TP 档） | 同上 |
| 学习等级 | 65（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；单角色至多 5 级） | 同上 |
| TP 消耗 | 2/级 | 同上 [special purchase cost] |
| 前置 | 技能 103（血气之刃）Lv1 | 同上 [pre required skill] `103 1` |
| 类型 | passive（[feature skill type] 1；skill class 2） | 同上 |
| 一句话效果 | 刺击与爆炸攻击力各 +10%/级（explain 单条"攻击力 +10%"，数据两路同增） | 同上 [explain ex] |
| 基础技 | 103 血气之刃（`103-BloodSword.md` ✅）；基础 skl [feature skill index]=168 双向链接（实测） | 两 skl 实测 |

## 2. 强化增量（对照 103-BloodSword.md）

### 2.1 数据侧（E 类通用解码法）

- [level info]（71 行 ×3 列，末行 `9522 43215 3763`）、[static data] dungeon/pvp 两节 `500 -1000 170 60 250 100`、[level property] 4 向量（刺击/爆炸/HP 消耗/爆炸范围 static[5]）——**全部与基础 skl 逐字节相同**（python 比对实测）。
- [special level up]（dungeon，2 行）：`-1 0 % 10`（col0 刺击攻击力 +10%/级）、`-1 1 % 10`（col1 爆炸攻击力 +10%/级）。pvp 无该节（不强化）。
- 脚本消费：`BloodSword\bloodsword.nut` 定点 grep `168`/`BloodSwordEx` 零命中（实测）——TP 折算在引擎（103 的 nut 本为 mod 混淆壳，TP 更无脚本面）。
- HP 消耗（col2）与爆炸范围（static[5]=100%）**不随 TP 变**。

### 2.2 增量明细

| # | 增量 | 落我们侧 |
|---|---|---|
| 1 | 刺击 +10%/级 | 技能级 HitReaction.Damage ×(1+0.1×TP)——✅ |
| 2 | 爆炸 +10%/级 | BloodSwordExplosionArea 每 Tick ×(1+0.1×TP)——✅ |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodSwordEx.skl（258 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodSwordEx.skl` | ✅ 全节实测 | 镜像表 + TP 增量 |
| 基础 skl | BloodSword.skl（[feature skill index] 168） | 同目录 | ✅ 实测比对 | 双向链接 |
| 脚本 | —（无 168 分支） | `…\pvf\sqr\character\swordman\BloodSword\bloodsword.nut` | ⛔ 零命中（实测） | TP 消费在引擎 |
| 基础技文档 | 103-BloodSword.md | 本目录 | ✅ | 继承源（帧盒+爆炸 Area 草案） |

## 4. 资源需求

**0 新 img / 0 新文件**（随基础档 103 §4 的必需 4 + 可选 2）。

## 5. 实现方案草案（增量落地）

零新内容件/注册点，并入 103 §5 草案：

| 参数 | 基础版（103 草案） | TP 并入（建议 TP5 定值） |
|---|---|---|
| 刺击伤害 | 120 | ×1.5 → 180 |
| 爆炸每 Tick | 130 ×3 | ×1.5 → 195 ×3 |
| HP 消耗 | 固定 200（col2 不随 TP） | 不变 |
| 爆炸盒 | HalfExtents (2.4,1.0,1.7)（static[5]=100% 不随 TP） | 不变 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BloodSwordEx.skl | `.skl` 无子命令（镜像表 + 2 行增量） | 手抄可行；并入 skl 子命令缺口 |

翻译缺口计 1 条（.skl 类型）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 两路攻击力 +10%/级 | 无缺口 | 常数倍率 |
| "刺中才爆"时间驱动近似 / 后方向键原地刺 / 霸体 | 随基础档（103 §7） | 同 103 |
| 剑影侧 TP 投点 | 职业系统无对应 | 不做（主修狂战口径） |

## 8. 存疑与缺口上报

**未考证项**
1. [growtype maximum level] 第 6 位=5 开放剑影 TP，但 [skill fitness growtype]=3（主归属狂战）——剑影(growtype5)角色能否实际投点本 TP 未证（疑 mod 配置或剑影复用血气系技能的遗留档）。

**新缺口**：无。翻译工具：`.skl` 子命令（常驻）。
