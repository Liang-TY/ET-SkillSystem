# 强化 - 地裂 · 波动剑（NormalWaveEx）

> 技能ID 140 | 级别 E | 可实现性 🔶（伤害增量=纯调档✅，大小增量撞"对象整体缩放"延后档） | 分析日期 2026-08-22 | 批次 E1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 地裂 · 波动剑 | `NormalWaveEx.skl [name]` |
| 英文名 | NormalWaveEx（取 skl 文件名；[name2]=`Earth Crack Wave Sword Upgrade` 为英文——Ex 族普遍例外，L1 反例族） | 同上 |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5；skill class 0 波动系） | 同上 |
| 学习等级 | 50（[required level range] 5） | 同上 [required level] |
| 最高等级 | 10（[growtype maximum level] 各职业 5——单角色实际至多 5 级 TP） | 同上 |
| 类型 | passive（TP 特性技，[feature skill type] 1） | 同上 [type] |
| 指令 | 无（被动，随基础技 20 生效） | 同上 |
| CD / MP | 无 | 同上 |
| 特殊消耗 | TP 点 1/级（[special purchase cost] 1） | 同上 |
| 前置 | 基础技 20（地裂 · 波动剑）≥ Lv5（[pre required skill] `20 5`） | 同上 |
| 一句话效果 | 地裂 · 波动剑攻击力 +8%/级、大小 +5%/级（explain 口径；数据口径见 §2.2） | 同上 [explain ex] |

基础技解析：`020-NormalWave.md`（✅，demo 已实现 WaveSwordSkill + NormalWaveBullet）。本文只写增量，不重复走读。

## 2~7. 强化增量（合并节）

### 2.1 强化改了什么（explain ex 官方口径）

| 项 | 增量 | 备注 |
|---|---|---|
| 波动剑攻击力 | **+8% / TP级** | 伤害公式乘算位 |
| 波动剑大小 | **+5% / TP级**（explain） | 数据实测为 col1 每级 +8（§2.2），文本/数据不一致 |
| pvp | 增量全部为 0（[pvp][special level up] 三行全 0） | pvp 不生效，TP 系设计惯例 |

无新行为、无新段数、无新判定——纯数值放大。

### 2.2 数据侧解码（.skl 实测）

- **[level info]（dungeon 3 列 × 71 行）＝基础技 20 同名表的整表复制**（col0 `225→2366`、col1 `100→159`、col2 `276→2877`，按**基础技等级**索引；E 类通用形态，详 §8 批次结论）。
- **[static data] `250 100 -150`** 与基础技逐值相同（复制）。
- **[special level up]**（TP 每级增量，真正的强化载体）：

| 行 | 目标 | 每级增量 |
|---|---|---|
| `-1 0 % 8` | level col0（固定伤害列，见下勘误） | +8% |
| `-1 1 + 8` | level col1（大小比率 100 基准） | +8（≈+8%/TP） |
| `-1 2 % 8` | level col2（魔攻% 列） | +8% |

- **[level property] 模板向量与基础技完全一致**（`-1 2 / -2 0 / -1 1` → 魔攻% ←col2、+固定 ←col0、大小% ←col1）。⚠ 据此建议勘误 020 文档 §1 的列语义（其写作 col0=魔攻%/col2=固定，与模板向量顺序相反）；两列量级相近，数值结论不受影响。
- **大小增量的文本/数据冲突**：explain 称 +5%/级，special level up 实测 col1 +8/级（100 基准下即 +8%/级）。疑 explain 滞后；demo 建议二选一口径（见 §2.6）。

### 2.3 生效链路（DNF 侧）

- `wave.nut`（74 行全文实测）**无 140 分支**——其唯一强化分支是子状态 100=技能 100（强化冰刃，写包两发 24328）；本技能不走任何脚本。
- `sqr\character\swordman\` 白名单子树 grep `normalwaveex` 零命中。
- 结论：**强化折算全在引擎伤害公式内**（`sq_GetBonusRateWithPassive` 家族按 TP 等级乘算——推断，高置信：jg 参照脚本对基础技取数全用 WithPassive 版，见 142 文档 §2.3）；弹体仍是 normalwave.obj（24328），结构与命中参数不变。

### 2.4 缺口继承与增量影响

| 基础版状态（020） | 增量项 | 我方落点 |
|---|---|---|
| ✅ demo 已实现（WaveSwordSkill/NormalWaveBullet） | 攻击力 +8%/TP | `NormalWaveBullet.HitReaction.Damage` 数值调档，零新机制 |
| 大小随 col1 缩放（020 §7 已列延后） | 大小 +5~8%/TP | **对象整体缩放（延后档，064/016 累计在案）**——Ex 的大小增量同样延后，固定 100% |
| 等级缩放延后 | TP 等级维度 | demo 无 TP 系统：增量以固定档（如 TP=1 档 +8%）写进数值，或留 const |

### 2.5 资源与翻译增量

- **新增 img：0 张**（无自有 .ani/.atk/.obj；图标 `Character/Swordman/Effect/SkillIcon.img` 242/243 为 UI 共用表，基础技侧已记）。
- 翻译缺口：`.skl` 子命令（全局在案）1 条；Ex 特有节 **[special level up] / [special purchase cost] / [growtype maximum level]** 是 skl 子命令设计时的新增输入（TP 增量表必须 dump，否则 E 类 68 技全部手抄）。

### 2.6 demo 实现口径（建议）

| 项 | DNF 原值 | demo 建议 |
|---|---|---|
| 伤害 | 基础伤害 ×(1+0.08×TP) | `Damage = 50 × 1.08`（TP=1 档）或直接写 const 表 |
| 大小 | ×(1+0.05~0.08×TP) | 延后不还原（固定盒 0.5/0.4/0.3）；还原时 HalfExtents 同乘即可（判定可先行，视觉缩放仍延后） |
| TP 消耗 | 1 点/级 | 无 TP 系统：默认习得 1 档 |

## 8. 存疑与缺口上报

1. 大小增量文本（5%）与数据（col1 +8）不一致，疑 explain 滞后——以数据口径 8% 记，主循环定夺。
2. `'%8'` 的精确算法（列值相对百分比 vs 百分点直加）未考证；两口径在 TP≤5 时差异小，demo 不受影响。
3. 020 文档 col0/col2 列语义的模板向量勘误建议（§2.2，供主循环汇总时回改 020）。
4. **E 类通用结构结论（批次级）**：Ex skl = 基础技 level info/static data 整表复制 + [special level up] 每级增量（level 列）或 static 槽覆写/增量；复制表按基础技等级索引。后续 E 批可直接套此解码法，免逐列再证。
