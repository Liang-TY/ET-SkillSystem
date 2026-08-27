# 强化-波动爆发（ReleaseWaveEx）

> 技能ID 210 | 级别 E | 可实现性 ✅（基础技是我们游戏唯一已实现的强化对象；攻击力 +10%/级为 Damage 常数倍率） | 分析日期 2026-08-22 | 批次 E3

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 波动爆发 | `ReleaseWaveEx.skl [name]` |
| 英文名 | ReleaseWaveEx（skl 文件名；[name2]=`Release Wave Upgrade`） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4；[growtype maximum level] `0 0 0 0 5 0`——仅阿修罗 5 级） | 同上 |
| 学习等级 | 55（**前置：技能 32 波动爆发 Lv5**，[pre required skill] `32 5`） | 同上 |
| 最高等级 | 10（TP 10 级） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 2 | 同上 |
| 类型 | passive（skill class 0） | 同上 |
| 指令 | （skl 带 [command] ↑↑+Z——从基础技模板复制，被动强化无独立指令语义） | 同上 |
| 一句话效果 | 波动爆发攻击力 +10%/级 | 同上 [explain ex] |
| 基础技 | 32 波动爆发（`032-ReleaseWave.md`，✅ **我们游戏已实现** ReleaseWaveSkill）；基础 skl [feature skill index]=210 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 032-ReleaseWave.md）

### 2.1 TP 数据表解码（L21 向量法）

- static data `600`（pvp 800）= 基础同值（施放后僵直，基础档 01§5.5 已记档）。
- [level info] 1 列 ×70 行 = **基础表逐值复制**（`4004` → `32024`，逐值一致）。
- [special level up]（dungeon）：**`-1 0 % 10` = col0 攻击力 +10%/TP 级**（TP10 = ×2）；pvp 无该节。
- [level property] 向量 (-2,0,×1.0)：魔攻显示 = col0 直读——与基础 skl 单列结构一致。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 攻击力 +10%/级 | [special level up] col0 | 冲刺/爆发两段 Damage ×(1+0.1×TP)——✅ 配置倍率 |

仅此一条——E3 批最干净的 TP 强化。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ReleaseWaveEx.skl | `E:\Projects\cs\dnfororigin\pvf源码提取部分\pvf\skill\Swordman\ReleaseWaveEx.skl` | ✅ | TP 数据（含 [skill preloading image] ReleaseWave1/2 两行，同基础档） |
| 基础 skl | ReleaseWave.skl（[feature skill index] 210） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（基础档已证引擎内置；passive_skill case 表无 210，实测） | ⛔ | TP 消费在引擎 |
| 我方实现 | ReleaseWaveSkill.cs（as-built） | `Packages\cn.etetet.skill\DotNet~\Skills\` | ✅ | 增量落点（032 §5 三方对照） |

## 4. 资源需求

TP 被动零新增（ReleaseWave1/2.img 归基础档可选级 2 张未入库；主视觉 RELEASEWAVE3 已在库）。缺失 img：**0**（增量自身）。

## 5. 实现方案草案（增量落地）

在我们已实现的 `ReleaseWaveSkill`（冲刺 80 / 爆发 150 固定伤害，032 §5 对照表）上加 TP 倍率：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 冲刺段伤害 | 固定 80 ×(1+0.1×TP) | 80 × 1.5 = 120（TP5 档） |
| 爆发段伤害 | 固定 150 ×(1+0.1×TP) | 150 × 1.5 = 225（TP5 档） |

实现形态：技能配置加 `TpLevel` 字段（int，默认 0），两段 HitReaction.Damage 构造时乘倍率——**零框架改动**。TP 等级成长（随 TP 技能页投入变化）依赖 TP/技能等级系统（R6-C1 记档），demo 以常量注入即可。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ReleaseWaveEx.skl | `.skl` 无子命令（1 列 + [special level up] 1 行 + [skill preloading image]） | 手抄 1 值 + 1 增量行；skl 子命令同前议 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 攻击力随 TP 成长 | TP/技能等级系统（R6-C1 记档） | 配置常量注入 |
| （基础技侧既有简化：无位移版可选/击飞方向语义/CD 递减/波动印） | 随基础档 | 见 032 §5/§7 |

## 8. 存疑与缺口上报

**未考证项**：无新增（单列单增量全明；基础档 032 §8 的 CD 递减公式等存疑延续适用）。

**新缺口**：无新增系统级缺口。翻译工具：`.skl` 子命令（重复印证）；[special level up] 节 + [skill preloading image]（E 批共性记档）。
