# 强化-不动明王阵（WaveSpinAreaEx）

> 技能ID 214 | 级别 E（TP 强化技） | 可实现性 🔶（=基础版 074 🔶 连按加速/波动印珠数缺口；增量两路攻击力 +10%/级纯数值，零新增缺口） | 分析日期 2026-08-22 | 批次 E6

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 不动明王阵 | `WaveSpinAreaEx.skl [name]` |
| 英文名 | WaveSpinAreaEx（skl 文件名；[name2]=`不动明王阵 Upgrade`——中文别名） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4） | 同上 |
| 学习等级 | 65（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；[growtype maximum level] `0 0 0 0 5 0`） | 同上 |
| TP 消耗 | 2/级 | 同上 [special purchase cost] |
| 前置 | 技能 74（不动明王阵）Lv1 | 同上 [pre required skill] `74 1` |
| 类型 | passive（[feature skill type] 1；skill class 0） | 同上 |
| 一句话效果 | 焰珠与爆炸攻击力各 +10%/级（explain 单条"攻击力 +10%"，数据两路同增） | 同上 [explain ex] |
| 基础技 | 74 不动明王阵（`074-WaveSpinArea.md` 🔶）；基础 skl [feature skill index]=214 双向链接（实测） | 两 skl 实测 |

## 2. 强化增量（对照 074-WaveSpinArea.md）

### 2.1 数据侧（E 类通用解码法）

- [level info]（71 行 ×2 列，末行 `7396 32024`）、[static data] `4000 900 7 170 350 300`、[level property] 4 向量（-2 0 焰珠 / -2 1 爆炸 / 0 0 0.001 旋转时间 / 4 4 范围）——**全部与基础 skl 逐字节相同**（python 比对实测）。
- [special level up]（dungeon，2 行）：`-1 0 % 10`（col0 焰珠魔法攻击力 +10%/级）、`-1 1 % 10`（col1 爆炸伤害 +10%/级）。无 pvp 增量节。
- 旋转时间（static[0]=4000ms）与波动阵范围（static[4]=350px）**不随 TP 变**。
- 脚本消费：基础技无 pushState（经代理进 THROW 状态 13，074 §2.1）；load_state 与代理 nut（standalonewave/shockwavearea）无 214 引用（本批定点 grep standalonewave 目录零命中；074 已证 THROW 载体无 case 74）——TP 消费在引擎。

### 2.2 增量明细

| # | 增量 | 落我们侧 |
|---|---|---|
| 1 | 焰珠攻击力 +10%/级 | WaveSpinOrbZoneArea 每 Tick ×(1+0.1×TP)——✅ |
| 2 | 爆炸伤害 +10%/级 | WaveSpinBombArea Damage ×(1+0.1×TP)——✅ |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | WaveSpinAreaEx.skl（255 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\WaveSpinAreaEx.skl` | ✅ 全节实测 | 镜像表 + TP 增量 |
| 基础 skl | WaveSpinArea.skl（[feature skill index] 214） | 同目录 | ✅ 实测比对 | 双向链接 |
| 脚本 | —（无 214 分支） | `…\pvf\sqr\character\swordman\{standalonewave,shockwavearea}\`（代理 nut）+ swordman_throw.nut | ⛔ 零引用（实测+074 结论） | TP 消费在引擎 |
| 基础技文档 | 074-WaveSpinArea.md | 本目录 | ✅ | 继承源（焰珠区+爆炸区草案） |

## 4. 资源需求

**0 新 img / 0 新文件**（随基础档 074 §4 的必需 4 + 可选 12）。

## 5. 实现方案草案（增量落地）

零新内容件/注册点，并入 074 §5 草案：

| 参数 | 基础版（074 草案） | TP 并入（建议 TP5 定值） |
|---|---|---|
| 焰珠每 Tick | 35 ×10 | ×1.5 → 52 ×10 |
| 终结爆炸 | 180 | ×1.5 → 270 |
| 旋转时间/范围 | 4000ms / 350px（不随 TP） | 不变 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| WaveSpinAreaEx.skl | `.skl` 无子命令（2 列镜像 + 2 行增量） | 手抄可行；并入 skl 子命令缺口 |

翻译缺口计 1 条（.skl 类型）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 两路攻击力 +10%/级 | 无缺口 | 常数倍率 |
| 连按加速 / 波动印珠数 / 环形轨迹判定 | 随基础档缺口（074 §7） | 同 074 |

## 8. 存疑与缺口上报

**未考证项**
1. 本 skl **无 pvp [static data] 节**（dungeon 单节）——pvp 段引擎复用 dungeon 值还是缺省，未证（其余 9 技多有 pvp static）。

**新缺口**：无。翻译工具：`.skl` 子命令（常驻）。
