# 鬼食（DisjointItemDark）

> 技能ID 42 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼食 | .skl `[name]` |
| 英文名 | DisjointItemDark | skl 文件名（`[name2]` 也是中文"鬼食"） |
| 职业 | 鬼泣向强化（名称/图标判据；[skill fitness growtype] 全 0-5 可学） | .skl |
| 学习等级 / 最高等级 | 22（range 4）/ 5 | .skl `[required level]`/`[maximum level]` |
| 类型 | `[passive]`，[skill class] 4 | .skl `[type]` |
| 数值（推断） | static=50 250；`[level property]` 单列"出现机率 : <float1>%%"向量 (0,1,0.1) → 约 5% 起（static[0]×0.1，L21 读法，系数细节未考证） | .skl |
| 一句话效果 | 分解道具时，可以增加暗属性小晶块的出现机率 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DisjointItemDark.skl`（53 行）

## 存档说明
**179-DisjointItem（物品分解）** 的鬼泣向产出强化：不新增分解能力本身，只改分解的掉落表——提高"暗属性小晶块"（无色小晶块的暗属性变体，附魔/生产材料）的出现率。依赖：物品分解产出系统（掉落表+材料经济+元素属性晶块体系），我们 demo 无物品/背包/经济系统，分解都没有，更谈不上掉落概率修正。实测：`swordman_load_state.nut` 无注册、`passive_skill_swordman.nut` 无 `disjoint` 命中——纯引擎分解管线消费，无任何脚本/动画/战斗资源。与 194（开启分解商店）同属分解师经济技族。

## 8. 一句话结论
⛔ 不实现/远期：挂在"物品分解产出"经济系统上的概率被动，系统不存在则技能整体空转。
