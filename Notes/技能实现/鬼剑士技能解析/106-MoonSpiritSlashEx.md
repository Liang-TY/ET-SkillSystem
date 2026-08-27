# 强化 - 共鸣 : 鬼灵斩（MoonSpiritSlashEx）

> 技能ID 106 | 级别 E | 可实现性 ✅（基础技 028 本身 ✅；增量 = 纯攻击力乘区，零新机制零新资源） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 共鸣 : 鬼灵斩 | `skill\Swordman\MoonSpiritSlashEx.skl` [name] |
| 英文名 | MoonSpiritSlashEx（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype] 全列；[growtype maximum level] `5 0 0 0 0 5` → TP 上限 5） | 同上 |
| 学习等级 | 55（[required level range] 5）；**前置 137（speedslashvs 幻鬼:疾影斩）Lv1——非基础技 28，数据异常见 §8** | 同上 [pre required skill] |
| 最高等级 | [maximum level] 7（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· skill class 5（新式 TP）/ 物理 | 同上 |
| TP 消耗 | 2 点/级 | 同上 |
| 一句话增量 | 共鸣:鬼灵斩攻击力 +10%/Lv（剑影/幻鬼两侧同源同加） | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 新式结构（byte 级实证）

python diff 实证：[static data]（单值 `100`）与 [level info]（71 行 × 1 列，col0 6957→30357+，步进 +706）与 `ghostsword\moonspiritslash.skl` 逐字节相同。[special level up] 空节——增量引擎内部 ×(1+0.1N)（同 083 分型）。

level property（模板对位）：
- 鬼灵斩攻击力 ← `(-1,0,1.0)` = level col0；
- 斩击范围 ← `(0,0,1.0)` = static[0]=**100**（不变）；
- 模板固定文本"[幻鬼斩击攻击力与剑影相同]"——两侧同源，TP 乘区天然覆盖两刀。

**不受 TP 影响**：斩击范围 100%。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `moonspiritslashex` 0 命中（028 文档 §3 记的"关联强化"路径笔误——实际文件在 `skill\Swordman\` 根，非 ghostsword 子目录）；无 PO/ani/atk/appendage。基础技 PO 24349 dword 69/70 两侧均 `sq_GetBonusRateWithPassive(28,-1,0,1.0)` 取数（028 文档 §2.3 实测）——TP 加成在引擎层并入两侧。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MoonSpiritSlashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MoonSpiritSlashEx.skl` | ✅（254 行） | 副本 + 引擎增量 |
| 基础技文档 | 028-moonspiritslash.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | ghostsword\moonspiritslash.skl | 同 skl 树 | ✅ | diff 对拍相同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无新增（复用 028 全链：PO 24349 d69/70） | 纯数据被动 |

## 4. 资源需求

无自有资源（图标位 622/623）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 028 一并落地）

- **零新增内容件**。028 草案的 `MoonSpiritSlashSkill` 加 `TpLevel`（0-5 常量）：
  - 双区（`MoonSpiritSlashFrontArea` / `PhantomArea`）Damage 同乘 `×(1 + 0.10 × TpLevel)`——两侧伤害同源 col0，一个乘区全覆盖（重叠双吃 = DNF 同构）。
  - HalfExtents/命中反应（意图 atk down/push200/lift150）/CD 15000 不变。
- **概念映射**：引擎 ×(1+0.1N) → TpLevel 乘区；TP 学习系统缺失（R6-C1）→ 常量。
- **关键数值表**：TP0-5 每侧攻击力 ×1.0 → ×1.5（col0 6957% 基准 → ×1.5）。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| MoonSpiritSlashEx.skl | `.skl` 无子命令（新式 TP，同 083 分型） | 同 083 处理 |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行��� | 缺口/困难 | 简化建议 |
|---|---|---|
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |
| 幻鬼位置接力的 TP 结算 | 幻鬼锚点缺失（基础技侧已简化固定身后，028 §7） | 与 TP 无关，不触 |

## 8. 存疑与缺口上报

- **数据异常（mod 疑点）**：[pre required skill] = **137（speedslashvs）** 而非基础技 28——官方 TP 应前置基础技本体；同批 138（WhiteGhostSlashEx pre=126）同型异常。学习链数据疑被 mod 改动，实现侧按"前置 28"理解即可（无功能影响）。
- **给 028 的回填**：028 文档 §3"关联强化"行的路径 `skill\Swordman\ghostsword\moonspiritslashex.skl` 有误——实测文件在 `skill\Swordman\MoonSpiritSlashEx.skl`（根目录，lst:360 实证）。
- 未考证：新式 TP ×(1+0.1N) 公式为推断（同 083）。
