# 强化-邪光斩（GrandWaveEx）

> 技能ID 211 | 级别 E | 可实现性 🔶（攻击力 +10%/级零成本；随基础技多段/慢速弹体简化档） | 分析日期 2026-08-22 | 批次 E3

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 邪光斩（[name2]=`邪光斩 Upgrade`） | `GrandWaveEx.skl [name]/[name2]` |
| 英文名 | GrandWaveEx（skl 文件名） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4；[growtype maximum level] `0 0 0 0 5 0`——仅阿修罗 5 级） | 同上 |
| 学习等级 | 55（**前置：技能 50 邪光斩 Lv5**，[pre required skill] `50 5`） | 同上 |
| 最高等级 | 10（TP 10 级） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 2 | 同上 |
| 类型 | passive（skill class 0；[auto cooltime apply] 0 随基础） | 同上 |
| 指令 | →→+Z（模板复制） | 同上 |
| 一句话效果 | 邪光斩攻击力 +10%/级 | 同上 [explain ex] |
| 基础技 | 50 邪光斩（`050-GrandWave.md`，🔶）；基础 skl [feature skill index]=211 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 050-GrandWave.md）

### 2.1 TP 数据表解码（L21 向量法）

- static data `350` = 基础同值（多段攻击间隔 350ms，PO setTimeEvent 实证值）。
- [level info] 2 列 ×70 行 = **基础表逐值复制**（`658 492` → `1610 5637`，实测逐值一致）。
- [special level up]（dungeon）：**`-1 1 % 10` = col1（魔法攻击力显示列）+10%/TP 级**；pvp 无增量（pvp 表尾行小段回落同基础档 pvp 现象）。
- [level property] 三向量与基础 skl 逐值相同：(-2,1,×1.0) 魔攻=col1、(-1,0,×0.3) 射程=col0×0.3、(0,0,×0.001) 间隔=static[0]。

**基础文档勘误（本档产出）**：050 §1"col0 `658→7484`、col1 `492→3228`"与 skl 实测不符——dungeon 表实测 **col0 658→1610、col1 492→5637**（`7484`/`3228` 两数在 GrandWave.skl 全文 grep 零命中，疑手抄串行）。同时按模板证读：**魔攻显示列 = col1（492→5637）、射程 = col0×0.3（197→483px）**——与基础档"col0 一读两用（nut 读 col0 作攻击力% 与距离）"的 nut 证读并存张力（模板列与 nut 列不一致，疑 sq_GetPowerWithPassive 的列位与显示模板列位错位 1，未考证）。**本 TP 增量按模板作用于 col1**，不受该张力影响。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 攻击力 +10%/级 | [special level up] col1 | 波体每跳/单跳 Damage ×(1+0.1×TP)——✅ 配置倍率 |

仅此一条。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GrandWaveEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GrandWaveEx.skl` | ✅ | TP 数据 |
| 基础 skl | GrandWave.skl（[feature skill index] 211） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（基础档 grandwave.nut 仅 25 行门禁壳；passive_skill case 表无 211，实测） | ⛔ | TP 消费在引擎 |
| 基础技文档 | 050-GrandWave.md | 本目录 | ✅ | 慢速爬行波/PO case 11 走读引用 |

## 4. 资源需求

TP 被动零新增（GrandWave.img 必需 1 张 + 蓄力系可选归基础档）。缺失 img：**0**（增量自身）。

## 5. 实现方案草案（增量落地）

基础档 050 §5 的 `GrandWaveBullet : BulletDefinition`（慢速穿透弹）原样适用，本 TP 只改伤害：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 每跳伤害 | col1 魔攻%（Lv1 492 → ×0.1 显示 49.2%…按引擎结算） | 单跳 130 ×(1+0.1×TP)（TP5 档 195；若做 3 跳多段则 45×倍率×3） |
| 射程/波速/间隔 | col0×0.3 / col0÷3000 / 350ms | 不随 TP 变，同基础档 |

无新内容件/注册点；TP 等级配置常量注入。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| GrandWaveEx.skl | `.skl` 无子命令（2 列 + [special level up] 1 行 + [auto cooltime apply] 节） | 手抄可行；skl 子命令同前议 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 多段命中（350ms reset ~3 跳） | 随基础档（Bullet ResetHitIntervalMs 框架项） | 单跳倍率放大 |
| 蓄力版（51 修罗邪光斩） | 随基础档（蓄力输入缺失） | 跳过 |
| 模板列/nut 列错位 1（§2.1） | 未考证 | 记档（影响基础档回填，不影响 TP 增量） |

## 8. 存疑与缺口上报

**未考证项**
1. 模板证读（魔攻=col1）与基础档 nut 证读（power=col0）的列位差——两者并存的真实引擎语义（疑 nut 的 `-1,0` 索引与模板 `-2,1` 索引相差 1 或语义不同）。
2. Ex 表与基础表引擎取用优先级（本批为逐值复制，无冲突）。

**基础档勘误 1 条**（050 §1 表末值 7484/3228 → 实测 1610/5637，主循环回填 050 时引用本档 §2.1）。**新缺口**：无新增系统级缺口。翻译工具：`.skl` 子命令（重复印证）；[special level up] 节（E 批共性上报）。
