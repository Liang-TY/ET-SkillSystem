# 强化 - 鬼斩（HardAttackEx）

> 技能ID 142 | 级别 E | 可实现性 ✅（纯数值调档，零新机制） | 分析日期 2026-08-22 | 批次 E1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 鬼斩 | `HardAttackEx.skl [name]` |
| 英文名 | HardAttackEx（取 skl 文件名；[name2]=`Devil Slash Upgrade` 英文） | 同上 |
| 职业 | 鬼剑士共通（fitness 0-5；skill class 3） | 同上 |
| 学习等级 | 50（range 5） | 同上 |
| 最高等级 | 10（growtype 上限 5） | 同上 |
| 类型 | passive（TP 特性技） | 同上 |
| 指令 / CD / MP | 无 | 同上 |
| 特殊消耗 | TP 点 1/级 | 同上 |
| 前置 | 基础技 5（鬼斩）≥ Lv5 | 同上 [pre required skill] `5 5` |
| 一句话效果 | 鬼斩攻击力 +8%/级（本批最纯的单一数值强化） | 同上 [explain ex] |

基础技解析：`005-HardAttack.md`（✅，单段斩+固定盒草案已成）。

## 2~7. 强化增量（合并节）

### 2.1 强化改了什么

| 项 | 增量 |
|---|---|
| 鬼斩攻击力 | **+8%/级**（explain 与 special level up 双向印证，仅此一项） |

无大小/段数/异常/行为变化。pvp special level up 全 0（pvp 不生效，同族惯例）。

### 2.2 数据侧解码

- [level info] 2 列 × 71 行＝基础技 5 表逐值复制（col0 `390→3686`、col1 `65→614`——与 005 文档 Lv1/Lv70 首末值完全一致）；[static data] `100` 同复制。
- **[special level up]** 仅 2 行：`-1 0 % 8`（col0 攻击倍率%）、`-1 1 % 8`（col1 固定加成）——两列同率 +8%，对应 explain"攻击力 +8%"。
- [level property] 模板 `魔法攻击力 : <int>%% + <int>`（向量 −1,0 / −2,1）与基础技同构，col0=倍率%（jg 参照脚本 `sq_GetBonusRateWithPassive(5,-1,0,1.0)` 实证）、col1=固定值。

### 2.3 生效链路

- `sqr\character\swordman\` 子树 grep `hardattackex` 零命中（实测）——基础版引擎内置（005 F3），Ex 同为引擎折算。
- **WithPassive 折算点的直接旁证（本批最清晰）**：jg 参照脚本 `hardattack.nut` 对技能 5 的取数全部走 `sq_GetBonusRateWithPassive(5,…)` / `sq_GetPowerWithPassive(5,…)`——"WithPassive"后缀家族即引擎按已习得 Ex 等级叠算 TP 增量的���口（高置信推断，E 类通用机制）。

### 2.4 缺口继承与增量影响

| 基础版状态（005 ✅） | 增量项 | 我方落点 |
|---|---|---|
| 单段武器斩，固定盒+HitReaction（草案零新机制） | 攻击力 +8%/TP | `HitReaction.Damage = 110 × (1+0.08×TP)`——一个乘数，完事 |
| 等级缩放延后 | TP 维度 | 同 140：固定 TP=1 档写 const |

无任何新增缺口；基础版的既有简化（暗属性/取消窗口/刀光特效）原样继承。

### 2.5 资源与翻译增量

- 新增 img：**0 张**（图标 SkillIcon.img 226/227，UI 共用）。
- 翻译缺口：`.skl` 子命令 1 条（全局在案，无新增节类型）。

### 2.6 demo 实现口径

| 项 | DNF 原值 | demo 建议 |
|---|---|---|
| 伤害 | col0 390%（Lv1）×(1+0.08×TP) | Damage = 110 × 1.08 ≈ 119（TP=1 档） |
| 其余（CD/时长/命中反应/盒） | 不变 | 005 草案原值直用 |

## 8. 存疑与缺口上报

1. `'%8'` 精确算法（百分点 vs 相对值）未考证（批次共性存疑，demo 不受影响）。
2. 无其他存疑——本技能是"E 类=数值调档"的最简样本，可作后续 E 批快速通道的判定基准。
