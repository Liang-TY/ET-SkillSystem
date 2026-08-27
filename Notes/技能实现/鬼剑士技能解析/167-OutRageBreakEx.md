# 强化-崩山裂地斩（OutRageBreakEx）

> 技能ID 167 | 级别 E（TP 强化技） | 可实现性 🔶（=基础版 081 🔶 跳跃 z 位移/方向键缺口；TP 增量全数值——三路伤害 +10%/级、HP 消耗 -10%/级，零新增缺口） | 分析日期 2026-08-22 | 批次 E6

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 崩山裂地斩 | `OutRageBreakEx.skl [name]` |
| 英文名 | OutRageBreakEx（skl 文件名；[name2]=`Outrage Break Upgrade`） | 同上 |
| 职业 | 狂战士（[skill fitness growtype]=3） | 同上 |
| 学习等级 | 65（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；[growtype maximum level] `0 0 0 5 0 0`） | 同上 |
| TP 消耗 | 2/级 | 同上 [special purchase cost] |
| 前置 | 技能 81（崩山裂地斩）Lv1 | 同上 [pre required skill] `81 1` |
| 类型 | passive（[feature skill type] 1；skill class 2） | 同上 |
| 一句话效果 | 攻击力 +10%/级、HP 消耗量 -10%/级（explain 两口径；数据侧实为**四路**增量，见 §2） | 同上 [explain ex] |
| 基础技 | 81 崩山裂地斩（`081-OutRageBreak.md` 🔶）；基础 skl [feature skill index]=167 双向链接（实测） | 两 skl 实测 |

## 2. 强化增量（对照 081-OutRageBreak.md）

### 2.1 数据侧（E 类通用解码法）

- [level info]（70 行 ×4 列，末行 `4677 21820 22911 11360`）、[static data] dungeon `130 150 500 6 100 200 400 50 100` 与 pvp `130 150 0 5 100` 两节、[level property] 4 向量——**全部与基础 skl 逐字节相同**（python 比对实测；顺带为 081 补记 pvp static 五值）。
- [special level up]（dungeon，4 行）：

| 行 | 目标 | 每 TP 级增量 |
|---|---|---|
| `-1 0 % 10` | col0 捶击物理攻击力 | +10% |
| `-1 1 % 10` | col1 冲击波攻击力 | +10% |
| `-1 2 % 10` | col2 岩浆（碎片）攻击力 | +10% |
| `-1 3 % -10` | col3 HP 消减量 | **-10%（`%` 负步长——首见格式：与 164 KhazanEx 的 `-` 减法格式是达成同一语义的两种写法）** |

- pvp 无 [special level up] 节（不强化，通例）。
- explain 只列"攻击力/HP 消耗"两条，数据实为三路攻击力 + HP 四路——explain 简化口径（对 081 的三段伤害结构是旁证：col0/1/2 = 捶击/冲击波/岩浆三路独立成长）。
- 脚本消费：`outbreak\outbreak.nut` 定点 grep `167`/`OutRageBreakEx` 零命中（实测）——TP 折算在引擎。

### 2.2 增量明细

| # | 增量 | 落我们侧 |
|---|---|---|
| 1 | 捶击 +10%/级 | 本体 HitReaction.Damage ×(1+0.1×TP)——✅ |
| 2 | 冲击波 +10%/级 | OutRageBreakFloorArea Damage ×(1+0.1×TP)——✅ |
| 3 | 岩浆 +10%/级 | OutRageBreakBloodExpArea 每 Tick ×(1+0.1×TP)——✅ |
| 4 | HP 消耗 -10%/级 | ConsumeCasterHp ×(1-0.1×TP)（081 demo 建议改按 col3 原值 860 起算时同步乘）——✅ |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | OutRageBreakEx.skl（262 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\OutRageBreakEx.skl` | ✅ 全节实测 | 镜像表 + TP 增量 |
| 基础 skl | OutRageBreak.skl（[feature skill index] 167） | 同目录 | ✅ 实测比对 | 双向链接 |
| 脚本 | —（无 167 分支） | `…\pvf\sqr\character\swordman\outbreak\outbreak.nut` | ⛔ 零命中（实测） | TP 消费在引擎 |
| 基础技文档 | 081-OutRageBreak.md | 本目录 | ✅ | 继承源（跳跃+双 Area 草案） |

## 4. 资源需求

**0 新 img / 0 新文件**（TP 不换动画不换 PO；随基础档 081 §4 的必需 5 + 可选 3）。

## 5. 实现方案草案（增量落地）

零新内容件/注册点，并入 081 §5 草案：

| 参数 | 基础版（081 草案） | TP 并入（建议 TP5 定值） |
|---|---|---|
| 捶击伤害 | 100 | ×1.5 → 150 |
| 冲击波伤害 | 180 | ×1.5 → 270 |
| 岩浆每 Tick | 60 ×3 | ×1.5 → 90 ×3 |
| HP 消耗 | 5% MaxHp（col3 原值 860 折算） | ×(1-0.1×TP)，TP5=×0.5 → 2.5% MaxHp |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| OutRageBreakEx.skl | `.skl` 无子命令（4 行增量，含 `%` 负步长） | skl 子命令的 [special level up] 步长需兼容"格式符 + 负值"组合（`%`/`+`/`-` 三格式 ×正负号） |

翻译缺口计 1 条（.skl 类型；负步长为 skl 子命令新设计输入）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 三路攻击力 +10%/级、HP -10%/级 | 无缺口 | 常数倍率直译 |
| 跳跃抛物线/方向键调距/血之狂暴门禁 | 随基础档缺口（z 位移第 2 例/方向输入/状态查询） | 同 081 §7 |
| pvp 不强化 | 无 PVP 分流 | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. col3 衰减下限：TP5 累计 -50%，引擎是否对 HP 消耗设最低值（≥0 显然，但是否有保底占比）未证。
2. explain 未提三路攻击力分别成长（只写"攻击力"）——按数据三路同倍率处理，无实际差异。

**新缺口**：无。翻译工具：`.skl` 子命令（`%` 负步长格式首见，E2 `-` 格式并列第三种写法——[special level up] 步长格式族 `%`/`+`/`-` × 正负号）。
