# 强化 - 冥灵断魂斩（GhostDecollationEx）

> 技能ID 83 | 级别 E | 可实现性 🔶（增量本身 ✅ 级——纯攻击力乘区零新机制；整体随基础技 045 的 🔶 前提，鬼步特殊功能链不变） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 冥灵断魂斩 | `skill\Swordman\GhostDecollationEx.skl` [name] |
| 英文名 | GhostDecollationEx（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype] `0 1 2 3 4 5` 全列；[growtype maximum level] `5 0 0 0 0 5` → 共通/剑影 TP 上限 **5**） | 同上 |
| 学习等级 | 55（[required level range] 5）；前置 45 冥灵断魂斩 Lv1 | 同上 [pre required skill] |
| 最高等级 | [maximum level] 7（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· **skill class 5（新式 TP）** / 物理 | 同上 |
| TP 消耗 | 2 点/级 | 同上 [special purchase cost] |
| CD 元数据 | [auto cooltime apply] 1 + [dungeon][cool time] 45000/45000（与基础技 CD 相同的副本；[consume MP] 299/2511 同） | 同上 |
| 一句话增量 | 冥灵断魂斩攻击力 +10%/Lv | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 新式结构（byte 级实证）

python diff 实证：[static data]（`100 0`）与 [level info]（71 行 × 1 列，col0 18941→151533）与 `ghostsword\ghostdecollation.skl` 逐字节相同——**新式 TP（skill class 5）与老式的差别：[special level up] 为空节，增量无声明式数据，全部由引擎按 explain ex（攻击力 +10%/Lv）内部结算**（×(1+0.1N)）。

level property（模板对位，L21 法）：
- 断魂斩攻击力 ← `(-1,0,1.0)` = level col0（副本表）；
- 斩击范围 ← `(0,0,1.0)` = static[0]=**100**（不变）；
- 特殊功能·鬼步连接重置鬼步 CD ← `(1,1,1.0)` = static[1]=**0（关）**——TP 不开此功能。

**不受 TP 影响**：斩击范围 100%、鬼步 CD 重置开关（0=关）——TP 纯加攻击力。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `decollationex` **0 命中**；无 PO/ani/atk/appendage。基础技 PO 24349 取数走 `sq_GetBonusRateWithPassive(45, -1, 0, 1.0)`（045 文档 §2.3 实测）——WithPassive 后缀即引擎把 TP 加成并入，TP 结算全在引擎层。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GhostDecollationEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GhostDecollationEx.skl` | ✅（267 行） | 副本 + 引擎增量 |
| 基础技文档 | 045-ghostdecollation.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | ghostsword\ghostdecollation.skl | 同 skl 树 | ✅ | diff 对拍相同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无新增（复用 045 全链） | 纯数据被动 |

## 4. 资源需求

无自有资源（图标位 626/627）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 045 一并落地）

- **零新增内容件**。045 草案的 `GhostDecollationSkill` 加 `TpLevel`（0-5 常量）：
  - 主斩/蓄势推击 Damage 同乘 `×(1 + 0.10 × TpLevel)`（PO 50/51 两形态共用 col0，一刀乘区全覆盖；鬼步变体 sub2/3 后置实现时同乘）。
  - 斩击范围 100%、命中反应（down/push250/lift400）、CD 45000 不变。
- **概念映射**：引擎 ×(1+0.1N) → TpLevel 乘区；TP 学习系统缺失（R6-C1）→ 常量。
- **关键数值表**：TP0-5 攻击力 ×1.0/1.1/1.2/1.3/1.4/1.5（col0 18941% 基准 → ×1.5 = 28412%）。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| GhostDecollationEx.skl | `.skl` 无子命令；新式 TP 无 [special level up] 数据（空节）——增量只在 explain ex 文本 | skl 子命令设计输入：class 5 型 TP 的增量建议按"explain ex 解析 + 固定 ×0.1/级"约定，不依赖数据节 |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |
| 鬼步特殊功能链的 TP 结算（sub2/3 路径） | 技能取消体系缺失（045 §7，基础技侧已砍） | 常规路径乘区即可；变体路径随基础技后补 |
| [auto cooltime apply] + CD/MP 副本 | 引擎语义未考证（疑与基础技 CD 联动/仅 UI） | 忽略（基础技 CD 直用） |

## 8. 存疑与缺口上报

- **未考证**：新式 TP 的 ×(1+0.1N) 精确公式为 explain 文案 + 引擎行为推断（无声明式数据可证）；[auto cooltime apply] 在 TP 技能上的语义。
- **给轮间经验（新式 TP 分型）**：**skill class 5 型 TP（ghostsword 剑影族 5 例）＝副本 + 空 [special level up] + explain ex 文案增量（攻击力 +10%/Lv；个别附 Lv1 固定加成），消费同走 sq_GetBonusRateWithPassive**——与老式（class 0/1，[special level up] 声明增量）二分。
