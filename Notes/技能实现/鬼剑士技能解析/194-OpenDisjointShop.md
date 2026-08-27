# 开启分解商店（OpenDisjointShop）

> 技能ID 194 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 开启分解商店 | .skl `[name]` |
| 英文名 | OpenDisjointShop | skl 文件名（`[name2]`="Open Disjoint Shop"） |
| 职业 | 全职业（副职向，[skill fitness growtype] 0-5） | .skl |
| 学习等级 / 最高等级 | 20 / 20 | .skl |
| 类型 | `[passive]`，[skill class] 4 | .skl `[type]` |
| 一句话效果 | 开启分解商店为其他玩家提供分解服务收取手续费，也能分解自身装备；熟练度升级分解机（诺顿）、分解机有耐久需修理、专属产出 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\OpenDisjointShop.skl`（42 行）

## 存档说明
副职四件（191-194）之"分解师"的商业形态：在城镇摆摊开店，**其他玩家**把装备丢进你的分解机付费分解，你收手续费+涨熟练度；还带分解机升级（NPC 诺顿交互）与耐久修理小系统。依赖：多玩家交互/网络摆摊+经济货币+NPC 交互+耐久系统——这是 15 个 D 类技能里离战斗 demo 最远的一个：单机锁帧战斗 demo 连"其他玩家"都不存在。无 load_state/passive_skill 注册、无脚本。关联：179（分解主技）、42（鬼食产出强化）。

## 8. 一句话结论
⛔ 不实现（本批最远期）：多玩家经济摆摊系统，与单机战斗 demo 的目标域完全不相交。
