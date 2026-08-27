# 神影手（MysticEquip）

> 技能ID 124 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 神影手 | .skl `[name]` |
| 英文名 | MysticEquip | skl 文件名（`[name2]` 空） |
| 职业 | 剑魂 75 级二觉被动 | .skl `[required level]`=75 + `[second growtype maximum level]` 12 槽第 4 槽=1（R6-C4 成对排布=剑魂二觉）；且脚本 gate `sq_getGrowType(obj)==1`（剑魂，L17） |
| 学习等级 / 最高等级 | 75 / 1 | .skl |
| 类型 | `[passive]`，[skill class] 1 | .skl `[type]` |
| 数值（推断） | [dungeon] static=20000（Buff 20 秒）；level info 三列 3650/150/150 → 换武冲击波攻击力 3650（%）、攻击速度 +15%、移动速度 +15%（×0.1，L21 读法） | .skl |
| 一句话效果 | 掌握疾影手的奥义：切换武器时在前方发动冲击波并获得[疾影手]Buff；无法在被击时使用，不满足条件时退化为普通[疾影手] | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MysticEquip.skl`（67 行）

## 存档说明
**037-FastEquip（疾影手）** 的二觉强化：把"换武器"从纯增益动作升级为带攻击判定的动作——换武瞬间①前方冲击波（写包创建 PO 224302，dword 序列 25/施法者objId/power，威力 `sq_GetPowerWithPassive`）②挂攻速+移速 Buff（20 秒）。本批 D1 中唯一有完整脚本链的系统技，实测：`sqr\character\swordman\passive_skill_swordman.nut:147` `procSkill_MysticEquip`（等级>0 且剑魂时挂常驻 appendage `Character/Swordman/MysticEquip/MysticEquip.nut`，87 行）；appendage `proc` 每帧比对 `getWeaponSubType()`——**武器类型变化即触发** Buff_MysticEquip（CHANGE_STATUS_TYPE_ATTACK_SPEED/MOVE_SPEED）+ 冲击波 PO。不实现的原因：触发器是"换武器"（§6.3 缺失档"换装/武器切换"），无换装系统则 appendage 永不触发；且"无法在被击时使用"还撞受击状态判定（受击-施法互斥缺口姊妹项）。远期若武器系统立项：冲击波可用 Area+MeleeHit 直译、Buff 可用 BuffDefinition 直译（对照 BloodBoom 范式），增量极小。

## 8. 一句话结论
⛔ 不实现/远期：机制本体（冲击波+Buff）在我们框架内可直译，但触发源=武器切换系统（缺失档）——与 037 合并到"武器系统"立项。
