# 强化-死亡抗拒（DieHardEx）

> 技能ID 148 | 级别 E | 可实现性 🔶（两条增量——恢复量 +10%/级、HP 门槛 +3%/级——零成本落地；力量增益随消费链砍） | 分析日期 2026-08-22 | 批次 E3

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 死亡抗拒 | `DieHardEx.skl [name]` |
| 英文名 | DieHardEx（skl 文件名；[name2]=`Die Hard Upgrade`） | 同上 |
| 职业 | 狂战士（[skill fitness growtype]=3；[growtype maximum level] `0 0 0 5 0 0`——仅狂战 5 级） | 同上 |
| 学习等级 | 55（**前置：技能 34 死亡抗拒 Lv5**，[pre required skill] `34 5`） | 同上 |
| 最高等级 | 10（TP 10 级） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 2 | 同上 |
| 类型 | passive（skill class 2） | 同上 |
| 一句话效果 | HP 恢复量 +10%/级；可施放 HP 比率 +3%/级（25%→55%）；力量增益 +10%/级 | 同上 [explain ex] |
| 基础技 | 34 死亡抗拒（`034-DieHard.md`，🔶 本族最可落地）；基础 skl [feature skill index]=148 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 034-DieHard.md）

### 2.1 TP 数据表解码（L21 向量法，7 向量全明）

- static data `25 300` = 基础同值（HP 门槛 25% / 恢复 tick 300ms）——门槛由 [special level up] 动态加算。
- [level info] 6 列 ×70 行 = **基础表逐值复制**（`504 2500 3744 266 21579 34` 起）。
- [special level up]（dungeon，三行）：
  - `-1 0 % 10` → **col0 HP 恢复量 +10%/级**；
  - `0 0 + 3` → **static[0] HP 门槛 +3/级（百分点加算）**：25% → 25+3×TP，TP10 = **55%**；
  - `-1 5 % 10` → **col5 力量 +10%/级**。
  - pvp 三行全 0（PVP 无增量）。

**基础文档勘误补全（本档产出）**：Ex 的 [level property] 模板比 034 文档解码多出"增加力量"行——向量 (-1,5,×1.0) → **col5 = 增加力量（34 → 2006）**。基础档 §1 表只解了 6 行漏此列（基础 [explain]"增加力量、物防和硬直"本有提示）。全列修正后：col1=恢复持续 2500ms、col0=恢复量 504、col5=力量 34、col2=物防 3744、col3=硬直 266、col4=效果持续 21579、static[0]=门槛 25。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | HP 恢复量 +10%/级 | [special level up] col0 | HealTick 每跳 ×(1+0.1×TP)——✅ 零成本 |
| 2 | 可施放 HP 门槛 +3%/级（25→55%） | [special level up] static[0] | `MinCastHpPct = 25 + 3×TP`——✅ 零成本（现成属性直译） |
| 3 | 力量增益 +10%/级 | [special level up] col5 | ⛔ 属性消费链（力量→伤害公式无）——砍 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DieHardEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DieHardEx.skl` | ✅ | TP 数据 |
| 基础 skl | DieHard.skl（[feature skill index] 148） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（passive_skill case 表无 148，实测） | ⛔ | TP 消费在引擎 |
| 基础技文档 | 034-DieHard.md | 本目录 | ✅ | DieHardSkill/DieHardRegenBuff 草案引用 |

## 4. 资源需求

TP 被动零新增（恢复特效 PinchHpRegen.img 归基础档：必需 1 张未入库）。缺失 img：**0**（增量自身）。

## 5. 实现方案草案（增量落地）

基础档 034 §5 三件套（DieHardSkill / DieHardRegenBuff / HealTickAction）原样适用，本 TP 增量只改两处配置：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| HP 门槛 | 25 + 3×TP %（TP10=55%） | MinCastHpPct=40（TP4 档演示） |
| 恢复总量 | col0 504 ×(1+0.1×TP) | 500 × 1.4 = 700（8 跳 × 88，TP4 档） |
| 力量 | col5 34×(1+0.1×TP) | 砍（消费链缺失） |

无新内容件/注册点；TP 等级以配置常量注入。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| DieHardEx.skl | `.skl` 无子命令（含 [special level up] 的 **static 加算型（`0 0 + 3`）** 变体） | 手抄可行；skl 子命令纳入 TP 增量表时须支持"源=0（static 槽）+ 绝对加算"两种语义（L21 的源值语义在 TP 表第二次实证） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 力量 +10%/级 | 属性消费链（R1-A4 队列） | 砍 |
| 门槛随 TP 上升（55% 可发动） | 无缺口 | MinCastHpPct 直译 |
| TP 等级成长本身 | TP/技能等级系统（R6-C1 记档） | 配置常量注入 |

## 8. 存疑与缺口上报

**未考证项**
1. Ex 表与基础表引擎取用优先级（本批 Ex 表为逐值复制，���槛加算走 [special level up]，无冲突）。
2. 基础档 §8 的既有存疑（tick 间隔推断、中段回落跳档）延续适用。

**新缺口**：无新增系统级缺口。**基础档勘误 1 条（col5=力量 漏列补全，主循环回填 034 文档时引用本档 §2.1）**。翻译工具：`.skl` 子命令（重复印证）；[special level up] 节 static 加算变体（E 批共性上报）。
