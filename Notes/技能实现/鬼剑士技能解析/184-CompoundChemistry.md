# 炼金（CompoundChemistry）

> 技能ID 184 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 炼金 | .skl `[name]` |
| 英文名 | CompoundChemistry | skl 文件名（`[name2]`="The Chemistry"） |
| 职业 | 全职业可学，上限统一 2 级（[growtype maximum level]=2 2 2 2 2 2） | .skl |
| 学习等级 / 最高等级 | 1（range 10）/ 10 | .skl |
| 类型 | `[passive]`，[skill class] 4 | .skl `[type]` |
| 一句话效果 | 进行物品合成时使用的技能 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\CompoundChemistry.skl`（46 行）

## 存档说明
老一代"生产系"四件套（181-184）的"炼金"入口：物品合成的化学分支（药水/材料类配方），等级=熟练度档位。注意与 **193-Alchemy（链金术，副职"炼金术师"）** 是两代不同系统：184 是全职业低上限的通用生产入口，193 是 20 级后的专职副职（做恢复/Buff 药水）。零战斗内容：无注册（load_state/passive_skill 均无命中）、无脚本。不实现的原因：依赖物品合成/材料/配方经济系统，demo 无物品概念。

## 8. 一句话结论
⛔ 不实现/远期：生产合成系统入口技能，无物品合成系统则整体无意义。
