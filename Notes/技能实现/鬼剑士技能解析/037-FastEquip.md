# 疾影手（FastEquip）

> 技能ID 37 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 疾影手 | .skl `[name]` |
| 英文名 | FastEquip | skl 文件名（`[name2]`="The Quick Exchange of Weapon"） |
| 职业 | 剑魂（主）；剑影亦可 1 级 | .skl `[growtype maximum level]`=0 1 0 0 0 1（L17 映射；[explain] 自称"剑魂特有技能"） |
| 学习等级 / 最高等级 | — / 1 | .skl |
| 类型 | `[passive]`，[skill class] 1 | .skl `[type]` |
| CD / MP | 无（效果内含"武器切换冷却减少"） | .skl |
| 数值（推断，L21 读法） | CD 减少量 50%；物理暴击率 +10%（static[0]=100×0.1）；物理攻击力 +10%（static[1]=100×0.1）；Buff 持续 10 秒（static[2]=10000×0.001） | .skl `[static data]`=100 100 10000 + `[level property]` 四行 + `[level info]` 1/50 |
| 一句话效果 | 可迅速切换武器；切换**其它系列**武器时获得攻击力+暴击率 Buff（持续一段时间），切换**同系列**武器只减少切换冷却 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FastEquip.skl`（59 行）

## 存档说明
剑魂的快速换武被动：把换武器从"回城式慢操作"变成战斗中即时动作，并用"跨系列换武给增益、同系列换武只减 CD"的规则奖励频繁换武玩法。它依赖三层系统：①换装/武器切换事件（§6.3 缺失档）——整个技能的触发器；②武器系列（weaponSubType）判定（R2-A6"武器类型差异化"缺口）；③攻击力/暴击率数值消费链（"属性数值无伤害消费链"最重缺口——Buff 挂得上但伤害端不读）。实测：`swordman_load_state.nut` 无注册、`passive_skill_swordman.nut` 无 `fastequip` 命中——效果全由引擎换装管线消费，本 pvf 无独立脚本。二觉强化版为 **124-MysticEquip（神影手）**（有完整 appendage 实现，见该文档）。

## 8. 一句话结论
⛔ 不实现/远期：触发器（换武器系统）+ 消费端（攻击力/暴击属性链）双缺失；与 124 合并到"武器系统"立项时一并设计。
