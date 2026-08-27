# 精工（CompoundCraft）

> 技能ID 181 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 精工 | .skl `[name]` |
| 英文名 | CompoundCraft | skl 文件名（`[name2]`="The Craft"） |
| 职业 | 全职业可学，各职业上限不同（[growtype maximum level]=2 4 4 2 2 2） | .skl |
| 学习等级 / 最高等级 | 1（range 10）/ 10 | .skl |
| 类型 | `[passive]`，[skill class] 4 | .skl `[type]` |
| CD / MP / 数值 | 均无（[static data] 空） | .skl |
| 一句话效果 | 进行物品合成时使用的技能 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\CompoundCraft.skl`（46 行）

## 存档说明
老一代"生产系"四件套（181 精工 / 182 织造 / 183 机械 / 184 炼金）之一：合成玩法的系统入口，等级=熟练度档位，决定可合成的配方等级。零战斗内容：无注册（`swordman_load_state.nut`/`passive_skill_swordman.nut` 均无命中）、无 nut、无资源。不实现���原因：依赖物品合成/材料/背包/配方经济系统——demo 无物品概念，整族无落点。注意与 191-194"副职四件"（附魔师/控偶师/炼金术师/分解师）区分：181-184 是更早的通用生产系统（各职业以不同上限参与），191-194 是满级后的专职副职系统，两代并存于本 pvf。

## 8. 一句话结论
⛔ 不实现/远期：生产合成系统入口技能，无物品合成系统则整体无意义。
