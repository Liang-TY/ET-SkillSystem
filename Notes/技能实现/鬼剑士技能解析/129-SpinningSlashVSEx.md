# 强化 - 幻鬼 : 回天（SpinningSlashVSEx）

> 技能ID 129 | 级别 E | 可实现性 🔶（增量本身 ✅ 级——两列攻击力乘区零新机制；整体随基础技 026 的 🔶 前提） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 幻鬼 : 回天 | `skill\Swordman\SpinningSlashVSEx.skl` [name] |
| 英文名 | SpinningSlashVSEx（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype] 全列；[growtype maximum level] `5 0 0 0 0 5` → TP 上限 5） | 同上 |
| 学习等级 | 55（[required level range] 5）；前置 26 幻鬼:回天 Lv1 | 同上 |
| 最高等级 | [maximum level] 7（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· skill class 5（新式 TP）/ 物理 | 同上 |
| TP 消耗 | 2 点/级 | 同上 |
| 一句话增量 | 幻鬼:回天攻击力 +10%/Lv（回旋/终结两列） | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 新式结构（byte 级实证）

python diff 实证：[static data]（8 值 `100 150 0 0 100 600 400 80`）与 [level info]（71 行 × 2 列：col0 回旋 3208→25668、col1 终结 4812→38502）与 `ghostsword\spinningslashvs.skl` 逐字节相同。[special level up] 空节——增量引擎内部 ×(1+0.1N)（同 083 分型；explain ex 只说"攻击力"，两列伤害同乘，高置信）。

level property（10 占位符模板对位，L21 法——**注意这些行全是 static/level 副本的显示映射，不是 TP 增量**）：

| 模板行 | 向量 | 值 | 说明 |
|---|---|---|---|
| 回旋斩攻击力 / 终结斩击攻击力 | (-1,0)/(-1,1) | col0/col1 | 副本表 |
| 斩击范围 | (0,0,1.0) | static[0]=100 | 不变 |
| 终结斩击突进距离 | (1,1,1.0) | static[1]=**150px** | 不变 |
| 回旋斩次数增加 | (2,2,1.0) | static[2]=**0（关）** | **TP 不开追加回旋** |
| 变更幻鬼生成位置/黑色风暴 | (3,3,1.0) | static[3]=**0（关）** | **TP 不开黑色风暴** |
| 黑暗风暴大小/吸附范围/吸附时间/攻击间隔 | (4,4)/(5,5)/(6,6,0.001)/(7,7,0.001) | 100%/600px/0.4s/0.08s | 备用参数（static[3]=0 时不启用） |

——static[4..7]（100 600 400 80）语义就此全解码（026 文档 §8"未考证"回填）：黑色风暴子系统的四参数，开关 static[3]=0 关闭。**TP 只加攻击力，四个特殊功能开关全保持关闭。**

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `spinningslashvsex` 0 命中；无 PO/ani/atk/appendage。基础技 PO 24349 id63 两相位取数 `sq_GetBonusRateWithPassive(26, -1, 0/1, 1.0)`（026 文档 §2.3 实证 state10=col0/state11=col1）——TP 加成引擎层并入两列。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SpinningSlashVSEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SpinningSlashVSEx.skl` | ✅（278 行） | 副本 + 引擎增量 |
| 基础技文档 | 026-spinningslashvs.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | ghostsword\spinningslashvs.skl | 同 skl 树 | ✅ | diff 对拍相同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无新增（复用 026 全链：PO 24349 id63） | 纯数据被动 |

## 4. 资源需求

无自有资源（图标位 624/625；黑色风暴资源因 static[3]=0 不需要）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 026 一并落地）

- **零新增内容件**。026 草案的两 Area（Spin/Finish）各接 `TpLevel`（0-5 常量）：
  - `SpinningSlashVSSpinArea.Damage = 120 × (1 + 0.10 × TpLevel)`；`FinishArea.Damage = 150 × (1 + 0.10 × TpLevel)`（对位 col0/col1 两列）。
  - 攻击盒（800/1000px 宽）、终结前移 150px、命中反应（意图 atk1 平推/atk2 击飞）、CD 20000 不变。
- **概念映射**：引擎 ×(1+0.1N) → TpLevel 乘区 ×2（两 Area）；TP 学习系统缺失（R6-C1）→ 常量。
- **关键数值表**：TP0-5 两列同乘 ×1.0 → ×1.5。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| SpinningSlashVSEx.skl | `.skl` 无子命令（新式 TP，同 083 分型） | 同 083 处理 |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |
| 黑暗风暴/追加回旋（TP 模板罗列但开关 0） | 数据关闭（本 pvf 不启用），非缺口 | 不实现（与基础技侧一致，026 §7） |
| 六剑术中即时施放的 TP 结算 | 技能取消体系缺失（基础技侧已砍） | 常规路径乘区即可 |

## 8. 存疑与缺口上报

- **未考证**：TP 是否对"黑色风暴"参数另有隐藏增量（开关全 0 无法从本 pvf 证伪——explain ex 仅述攻击力，按无处理）。
- **给 026 的回填**：026 文档 §8"static[4..7]=`100 600 400 80` 未考证"→ 本批经 Ex 副本 level property 模板对位全解码：**static[4]=风暴大小 100%、static[5]=吸附范围 600px、static[6]=吸附时间 400ms、static[7]=攻击间隔 80ms**（黑色风暴子系统参数，static[3]=0 关闭态）。
