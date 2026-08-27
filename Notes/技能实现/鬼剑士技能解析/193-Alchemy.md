# 链金术（Alchemy）

> 技能ID 193 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 链金术（skl 原文用字；常用译名"炼金术"） | .skl `[name]` |
| 英文名 | Alchemy | skl 文件名（`[name2]`="Alchemy"） |
| 职业 | 全职业（副职向，[skill fitness growtype] 0-5） | .skl |
| 学习等级 / 最高等级 | 20 / 20 | .skl |
| 类型 | `[passive]`，[skill class] 4 | .skl `[type]` |
| 一句话效果 | 可以制作具有恢复或 Buff 效果的药水，还可以制作具有特殊效果的道具；熟练度提升解锁更多种类 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Alchemy.skl`（41 行）

## 存档说明
副职四件（191-194）之"炼金术师"：制作消耗品（恢复药、Buff 药、特殊道具如神秘世界喇叭类）的生产入口。与 184-CompoundChemistry（通用生产"炼金"）是两代系统，本技能是满级专职版。不实现的原因：依赖材料/配方/熟练度生产系统+消耗品道具使用系统（吃药动作、道具栏、道具 CD）——demo 无物品概念，连 Buff 药的"使用"动作都没有载体。无 load_state/passive_skill 注册（grep `alchemy` 无命中）、无脚本。

## 8. 一句话结论
⛔ 不实现/远期：炼金术师生产入口，依赖生产+消耗品道具双系统缺失。
