# 物品分解（DisjointItem）

> 技能ID 179 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 物品分解 | .skl `[name]` |
| 英文名 | DisjointItem | skl 文件名（`[name2]`="Disjoin Item"） |
| 职业 | 全职业通用（[skill fitness growtype] 0-5） | .skl |
| 学习等级 / 最高等级 | 1 / 10 | .skl `[required level]`/`[maximum level]` |
| 类型 | `[passive]`，[skill class] 4 | .skl `[type]` |
| CD / MP / 数值 | 均无（[static data] 空） | .skl |
| 一句话效果 | 进行物品分解时使用的技能 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DisjointItem.skl`（43 行）

## 存档说明
分解师副职的入门主技能（"物品分解"）：打开分解 UI 把装备拆成小晶块等材料的系统入口——等级（熟练度）提升分解产物的档位。它本身零战斗内容：无 CD、无消耗、无 static data、无注册（`swordman_load_state.nut` 无注册、`passive_skill_swordman.nut` 无 `disjoint` 命中）、无 nut/动画/判定——纯 UI/经济系统开关，等级数值全由分解管线（诺顿 NPC/分解机）消费。不实现的原因：依赖物品/背包/材料产出经济系统，我们 demo 是纯战斗锁帧 demo，无物品概念。关联技族：**042-DisjointItemDark（鬼食，分解产出概率强化）**、**194-OpenDisjointShop（开启分解商店，分解师摆摊）**。

## 8. 一句话结论
⛔ 不实现/远期：分解师经济系统入口技能，无物品/背包/经济系统则整体无意义。
