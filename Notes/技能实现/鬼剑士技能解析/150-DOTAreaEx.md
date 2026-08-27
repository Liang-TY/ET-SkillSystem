# 强化-杀意波动（DOTAreaEx）

> 技能ID 150 | 级别 E | 可实现性 🔶（每秒伤害 +50%/级在 Tick 常数体系零成本；减攻范围/比率两路随消费���砍） | 分析日期 2026-08-22 | 批次 E3

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 杀意波动 | `DOTAreaEx.skl [name]` |
| 英文名 | DOTAreaEx（skl 文件名；[name2]=`Rage Wave Upgrade`） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4；[growtype maximum level] `0 0 0 0 5 0`——仅阿修罗 5 级） | 同上 |
| 学习等级 | 55（**前置：技能 52 杀意波动 Lv5**，[pre required skill] `52 5`） | 同上 |
| 最高等级 | 10（TP 10 级） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 1（E3 批最便宜） | 同上 |
| 类型 | passive（skill class 0） | 同上 |
| 一句话效果 | 每秒波动伤害 +50%/级；攻击力减少范围 +40px/级；攻击力减少比率 +3%/级 | 同上 [explain ex] |
| 基础技 | 52 杀意波动（`052-DOTArea.md`，🔶）；基础 skl [feature skill index]=150 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 052-DOTArea.md）

### 2.1 TP 数据表解码（L21 向量法，6 向量全明）

- static data **`50 25`**（基础 `50 10`）——基础档两值本就未解（疑高度限/MP 判定），TP 后 static[1] 10→25 同未解，记档。
- [level info] 6 列 ×40 行 = **基础表逐值复制**（`624 300 5 5 300 5` 起）。
- [special level up]（dungeon，三行）：
  - `-1 0 % 50` → **col0 每秒波动伤害 +50%/级**（E3 批最大单增量：TP10 = ×6）；
  - `-1 4 + 40` → **col4 攻击力减少范围 +40px/级**（300 → 700px）；
  - `-1 5 + 3` → **col5 攻击力减少比率 +3%/级**（5% → 35%）。
  - pvp 后两行 0（PVP 仅伤害增量，减攻不生效）。

**语义注意**：explain ex 称"施放时追加减攻效果"，但基础表 col4/col5 已有恒定值（300/5）——疑基础引擎版即带减攻（TP 前不可见/为 0 级），TP 起放大；或文本滞后。数据面按"两列基础有值 + TP 逐级加算"记。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 每秒波动伤害 +50%/级 | [special level up] col0 | TickActions MeleeHit Damage ×(1+0.5×TP)——✅ 零成本（固定值天然"无视防御"语义不变） |
| 2 | 减攻范围 300→700px | col4 加算 | ⛔ 属性消费链（敌方攻击力无键无消费）——砍 |
| 3 | 减攻比率 5→35% | col5 加算 | ⛔ 同上——砍 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DOTAreaEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DOTAreaEx.skl` | ✅ | TP 数据（含 [skill preloading image] DOTArea/DOTAreaDamage 两行，同基础档） |
| 基础 skl | DOTArea.skl（[feature skill index] 150） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（基础档已证纯引擎 toggle；passive_skill case 表无 150，实测） | ⛔ | TP 消费在引擎 |
| 基础技文档 | 052-DOTArea.md | 本目录 | ✅ | DOTAreaAura/FireCircle 范式引用 |

## 4. 资源需求

TP 被动零新增（DOTAreaDamage.img/EarthQuakeRing.img 归基础档：必需 1 + 可选 1 未入库）。缺失 img：**0**（增量自身）。

## 5. 实现方案草案（增量落地）

基础档 052 §5 的 `DOTAreaAura : AreaDefinition`（Tick 1000ms + MeleeHit）原样适用，本 TP 只改一处：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 每秒伤害 | col0 ×0.1 ×(1+0.5×TP)（Lv1 基础 62.4；TP10 ×6 = 374.4） | 25 ×(1+0.5×TP)（TP2 档 = 50/秒） |
| 减攻范围/比率 | 300+40×TP px / 5+3×T % | 砍 |

无新内容件/注册点。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| DOTAreaEx.skl | `.skl` 无子命令（6 列 + [special level up] 3 行 + [skill preloading image]） | 手抄可行；skl 子命令纳入 TP 增量表；预载清单照旧忽略+记档 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 敌方攻击力 -5~-35%（300~700px 内） | 属性消费链（R1-A4 队列） | 砍 |
| toggle/光环跟随/暴击光环 | 随基础档（Area 跟随 + Buff 查询门面缺口） | 同基础档 10s 驻场简化 |
| 伤害 +50%/级 | 无缺口 | Tick Damage 常数倍率 |

## 8. 存疑与缺口上报

**未考证项**
1. static `50 25` 两值语义（基础档存疑延续，TP 后 static[1] 变 25）。
2. "减攻为 TP 追加效果"与基础表 col4/col5 已有值的矛盾（疑基础版 0 级隐藏，未考证）。

**新缺口**：无新增系统级缺口。翻译工具：`.skl` 子命令（重复印证）；[special level up] 节 + [skill preloading image]（E 批共性记档）。
