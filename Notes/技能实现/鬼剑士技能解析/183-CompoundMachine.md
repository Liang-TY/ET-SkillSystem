# 机械（CompoundMachine）

> 技能ID 183 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 机械 | .skl `[name]` |
| 英文名 | CompoundMachine | skl 文件名（`[name2]`="The Machine"） |
| 职业 | 理论全职业字段，但 [growtype maximum level]=0 0 0 0 0 0——**任何职业上限都是 0 级=当前不可学习** | .skl 实测 |
| 学习等级 / 最高等级 | 1（range 10）/ 10（与全 0 的职业上限矛盾——死配置痕迹） | .skl |
| 类型 | `[passive]`，[skill class] 4 | .skl `[type]` |
| 一句话效果 | 进行机械组装时使用的技能 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\CompoundMachine.skl`（46 行）

## 存档说明
生产系四件套的"机械组装"入口。特别之处：`[growtype maximum level]` 六槽全 0，即在本 pvf 中**没有任何职业能学到它**（maximum level 10 与职业上限 0 并存=被运营关闭的配置，同 R6-C2"极速成长公会经验无落点"一类死数据）。与 182（织造，字段全空）一起说明这代生产系统在本 pvf 已半废弃。不实现的原因：①物品合成/生产系统不存在（同 181）；②原版本服也已将其关闭。

## 8. 一句话结论
⛔ 不实现：生产系统入口+原版已用全 0 职业上限关闭的死配置技能。
