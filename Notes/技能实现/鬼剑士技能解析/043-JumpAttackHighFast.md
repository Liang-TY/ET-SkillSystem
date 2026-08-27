# 草上飞（JumpAttackHighFast）

> 技能ID 43 | 级别 C（**纠偏：实为 D 类**——DNF 未实装的占位技能，explain 自述"保留中"） | 可实现性 ⛔（原版无行为可移植） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 草上飞 | `JumpAttackHighFast.skl [name]`（[name2] 同为中文`草上飞`，L1 惯例） |
| 英文名 | JumpAttackHighFast（取 skl 文件名） | 同上 |
| 职业 | 全系共通（growtype 0-5；上限 1） | skl [skill fitness growtype] / [growtype maximum level] |
| 学习等级 | 14 | skl [required level] |
| 最高等级 | 1 | skl [maximum level] |
| 类型 | passive（skill class 1） | skl [type] / [skill class] |
| explain | **`保留中`**（唯一说明文字——官方明示未开放） | skl [explain] |
| static data | `4 30 10 100`（四值，无 explain 佐证，语义全部**未考证**） | skl |
| level info / level property | **无此二节**（连数值表都没配——纯占位实锤） | skl 实测 |
| 一句话效果 | （不可用）跳跃攻击强化类被动的占位文件，未实装 | §2 走读归纳 |

## 2. 技能逻辑走读（极简）

- `swordman_load_state.nut`：无注册（grep jumpattackhighfast 无命中）；
- `passive_skill_swordman.nut`：无 case 43；
- `sqr\character\swordman\jump\` 目录（跳跃相关 nut 唯一所在地）：仅 `swordman_jump.nut`，
  grep `HighFast|JUMPATTACK` 零命中——**跳跃系统侧也无消费点**；
- 白名单全树无 JumpAttackHighFast 命名资源（ani/atk/obj/effect 均查）。

结论：skl 是**数据占位文件**（有头无身：explain"保留中"+ static 四值+无 level 表）。
文件名暗示设计意图为"跳跃攻击强化（高度/速度向）"，配合 static `4 30 10 100`（**推断**为
次数/高度/速度/比率类参数，无任何佐证）。国服正式服该技能同样长期未开放。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | JumpAttackHighFast.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\JumpAttackHighFast.skl` | ✅ | 占位数据（static 4 值） |
| 注册行 / 被动注册 | — | load_state / passive_skill_swordman.nut | ⛔ 均无 | — |
| 主 nut / jump 目录 | — | `…\sqr\character\swordman\jump\`（仅 swordman_jump.nut，grep 无命中） | ⛔ 无消费点 | — |
| .ani / .atk / PO / 特效 | — | 各资源目录 | ⛔ 无专属 | — |
| 图标 | SkillIcon.img #86/87 | `Character/Swordman/Effect/SkillIcon.img` | ✅（路径） | 不做 UI |

## 4. 资源需求

必需级缺失 img = **0**（无任何资源文件）。

## 5~7. 实现判定（⛔ 级一段话）

无可移植行为：原版自身未实装（explain"保留中"+ 无 level 表 + 无脚本 + 无资源 + 无消费点，
五重实证）。**不实现、不建议立项**。若未来跳跃系统落地且想自创"跳跃攻击强化"被动，
static `4 30 10 100` 可作设计参数的占位参考（语义未考证，仅文件名方向性提示）。
跳跃系统缺口本身在案（R1-A2，175/017 等已上报）。

## 8. 存疑与缺口上报

- 未考证：static 四值语义（无佐证，全部存疑）。
- 分类纠偏：C → **D**（非战斗/不可用——与手册 §4 D 类"一段话存档：做什么用、为何不实现"对齐；
  本文为 C 批内唯一 D 级处理，返回值注明）。
- 新缺口 / 翻译缺口：均无（.skl 全局已知项，无新内容）。
