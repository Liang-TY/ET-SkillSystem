# R6-D-地下城定义考察

考察完成。以下为报告。

# 地下城定义考察报告（dungeon/ 目录）

## 1. 顶层结构

```
E:/Projects/cs/dnforigin/pvf源码提取部分/pvf/dungeon/
├── dungeon.lst            ← 全局索引：地下城ID → .dgn 文件路径（头部 `1 Act1/Lorien.dgn`，尾部到 ID 9719）
├── dungeon.kor.str / dungeon.jpn.lst   ← 韩文/日文名字串
├── dungeonsample.dgn      ← 字段模板（含全空示例段）
├── act1..act9/            ← 按大区版本组织的普通地下城（如 act1/lorien.dgn）
├── 95dungeon/ kcontents2/ kcontents3/  ← 95级/韩服内容
├── ancient/ legend/ timegate/ village/ towers/ event/ quest/ special/
├── anton/ anton_awakening/ luke_normal/ luke_riad/  ← 团本
├── advancealtar/ castleofthedead/ darkelftemple/ guilddungeon/ riskdungeon/
├── metrocenter/ poongjintrainingroom/ shonantournament/ southerndale/
├── stormofmetastasis/ tau_kingdom/ title/ tutorial/ villageattack/ warroom/
└── （子目录内共 495 个 .dgn；另有少量 .tbl 难度表、.ani 入场动画、1 个 .tlk）
```

## 2. 典型 .dgn 完整原样引用

### 2.1 `dungeon/act1/lorien.dgn`（洛兰，最简单入门本，全文）

```
#PVF_File

[background pos]
	80

[entering title]
	`Title/Lorien.ani`

[cutscene image]
	`Map/CutScene/Lorien.img`	0

[minimap image]
	`Map/MiniMap/Act1.img`

[worldmap pattern info]
	1	1
	`WorldMap/SelectdungeonSlot/elvengard.img`	0

[minimum required level]
	1

[basis level]
	1

[experience increasing point]
	1.2

[champion]
	0	0	0	0	0

[pathgate object]
	361	362	363	364	365	366	367	368	369	370

[maze info]

[size]
	2	3

[greed]
	`JJEE
 LLMM
 DDGG`

[start map]
	0	0	1	0
[/start map]

[boss map]
	0	2	1	2
[/boss map]

[seal door map index]
	94

[seal door pos]
	1	1

[hit count]
	50	50	50	50	50

[seal door appear rate]
	1

[hell dungeon]
	1

[special passive object item]
	0	-1	3	1000	2000	1004	2000	3074	3000
	1	-1	1	10149508	6000
[/special passive object item]

[event monster]
	1	5000	61726	1	300	80	1
[/event monster]

[monster difficulty bonus]
	5	10	20	30	30
	10	30	60	150	250
	1	1	1	1	1
	1	1	1	1	1
	2	3	4	6	10
	1	1.5	1.5	1.5	3
	1	1.5	2	2.5	3
	0	0	0	0	0
	1	1	2	4	8
	1	2	4	6	8
	2	2	2	2.35	2.6
	0.6	0.8	0.95	0.99	1
	1	1.5	2	4	6
[/monster difficulty bonus]

[name]
	`洛兰`

[explain]
	`    洛兰！ 这个林荫闭日、 祥和安宁的丛林， 曾是人类和精灵的乐园！ ...`
```

（explain 尾部截断显示，原文件为一段完整介绍文字。）

### 2.2 `dungeon/act1/grakkarak.dgn`（格拉卡，有 [map specification] 强制指定，迷宫 6×3，核心迷宫段原样）

```
[size]
	6	3

[greed]
	`AABBNNNNFFMM
 BBNNHHOOAAKK
 AACCAADDFFGG`

[map specification]
	5	0	1030
[/map specification]

[map specification]
	3	2	1031
[/map specification]

[start map]
	1	2	1	0
[/start map]

[boss map]
	5	0	3	2
[/boss map]

[seal door map index]
	97

[seal door pos]
	3	0
```

其余字段与 lorien 同构：最低等级 10 / 基准等级 13 / champion `3 6 9 12 14` / 门对象 `391..400` / 事件怪 `1 10000 61726 1 300 80 1`（61726=黄金哥布林，出现率 10000‱）。

### 2.3 `dungeon/act5/bwanga.dgn`（布万加修炼场——多套布局变体样本；第 1、2 套 [maze info] 原样，全文共 7 套）

```
[maze info]

[size]
	10	1

[greed]
	`BBFFFFFFFFFFFFFFFFEE`

[map specification]
	1	0	16011
...
[map specification]
	8	0	16018
[/map specification]

[boss map specification]
	9	0	16019
[/boss map specification]

[start map]
	0	0
[/start map]

[boss map]
	9	0
[/boss map]

[maze info]

[quest connection]
	0	4002	-1

[size]
	2	1

[greed]
	`BBEE`

[boss map specification]
	1	0	16020
[/boss map specification]
...
```

后续 5 套依次为 `[quest connection] 0 4003..4007 -1/1`，布局从 `BBEE`(2格) 逐步增长到 `BBFFFFFFFFFFFFFFFFEE`(10格)，boss 图 ID 逐套换为 16021/16022/16023/16024，普通房图 ID 16011..16018 复用。

## 3. 字段结构总结

| 分类 | 字段 | 说明 |
|---|---|---|
| 身份/UI | `[name]` `[explain]` `[entering title]`(入场动画.ani) `[cutscene image]` `[minimap image]` `[worldmap pattern info]` `[background pos]` `[use zoom rate]` | 名字、介绍、入场动画、过场图、小底图、世界地图选卡槽 |
| 进入门槛 | `[minimum required level]` `[basis level]` `[recommended level]` `[experience increasing point]`(经验倍率) `[required item]`(如 `10158964 1 1`) `[limit party count]` `[designate dungeon difficulty]` `[prohibit practice]` | **没有“包含 map id 列表”的显式数组**——地图归属靠反向声明（见下） |
| 迷宫布局 | `[maze info]`(可重复多块) → `[size] 列 行` + `[greed]` 形状码网格 + `[map specification] 列 行 全局mapID` + `[boss map specification]` + `[start map] 列 行(可写多对候选)` + `[boss map] 列 行(可写多对候选)` + `[quest connection] 0 任务ID ±1`(按任务进度选变体) | **map id 引用只在这里**：强制指定某格用 map.lst 中的哪张图（实证 1030→`GrakKarak/b1004.map`、1031→`b1005.map`）；未指定的格从本副本地图池随机挑 |
| 传送门 | `[pathgate object]` | 10 个门/墙对象 ID（四方向 × 状态） |
| 深渊 | `[hell dungeon]` `[seal door map index]`(=深渊房全局mapID，如 94→`lorien/hell_94.map`) `[seal door pos] 列 行` `[hit count] 50×5` `[seal door appear rate]` `[escape hell]` | 深渊派对房替换/封印门 |
| 刷怪强度 | `[champion]`(每难度精英怪数) `[monster difficulty bonus]`(13行×5难度矩阵：HP/攻击/防御/经验等系数) `[event monster]`(乱入怪：`出现率‱ 怪物ID ...`，61726 黄金哥布林) `[boss room entrance condition]`(`[hunt monster] 1 50097 1 1`=杀指定怪才开boss门) | **每图刷什么怪/坐标不在 .dgn，而在每张 .map 的 `[monster]` 段**，每行：`怪物ID … x y … [fixed] [normal|boss]` |
| 通关条件 | 默认=击杀 boss 房 `[boss]` 怪；例外用 `[clear condition]`（22 处），如 `[destroy object] 10601 1`(摧毁物体) | |
| 奖励/掉落 | `[special passive object item]`（房内地面生成物权重表，如 `1000/1004/3074`=金币类，`10149508`=道具） `[coin limit]` `[gold card use]` | **装备掉落表不在 .dgn**——在 monster/*.mob 与任务奖励里，.dgn 只管地面拾取物 |
| 疲劳 | 常规 .dgn **无疲劳字段**；仅特殊本有 `[no fatigue]`(塔类,3处) `[fatigue] 4`+`[fatigue result] 0`(warroom 5处) `[tournament round fatigue]` | 常规“每进一间房扣 1 点疲劳”是引擎规则，不落在数据文件 |
| 其他 | `[revision table]`(指向 .tbl 难度表) `[risk dungeon]` `[ancient dungeon]` `[limit inout count]` 塔类专用字段若干 | |

`.map` 内刷怪表实例（`map/lorien/4.map` 普通房 / `b1.map` boss 房）：

```
[monster]
	1	1	0	1025	248	0	1	1	`[fixed]`	`[normal]`
	1	1	0	693	258	0	1	1	`[fixed]`	`[normal]`
	...
[monster]   (b1.map)
	2	1	0	1072	267	0	2	1	`[fixed]`	`[boss]`   ← 怪物ID 2，boss 标记
```

## 4. 地图数量与串联方式

**数量**：一个地下城 = `map/` 下同名目录的一组房间图。实测：洛兰 17 张（s0–s3 起点房 ×4、4–9 普通房 ×6、bn10–12 ×3、b1–b3 boss 房 ×3、hell_94 深渊房 ×1）；格拉卡 33 张；布万加 17 张。命名约定：`s*`=起点房变体、纯数字=普通房、`b*`=boss房、`hell_<地下城mapID>`=深渊房；新版本文件名直接带坐标如 `8153_(0.3)start.map`。全局 ID 登记在 `map/map.lst`（8187 行）；每张 .map 用 `[dungeon] 1` 反向声明归属哪个地下城。

**串联：不是编辑器连线，而是“网格迷宫 + 形状码”**：
- `[size] 列 行` 定义网格，`[greed]` 每格 2 字符 = 该房的“开门形状模板”：`NN`=空格（无房间）、`BB`=起点房（仅右门）、`EE`=终点房（仅左门）、`FF`=左右走廊、其余字母（AA/CC/DD/GG/HH/JJ/KK/MM/OO…）= 带上下门的各种分叉形状。形状决定哪几面墙开门（`[pathgate pos]` 给四门坐标），清完怪门解锁 → 走进相邻格。
- **选图规则**：`[map specification] 列 行 mapID` 强制指定；否则引擎从本副本地图池（`[dungeon]` ID 相同的所有 .map）里，按格子需求（boss 格 ↔ `[type] [boss]` 的图）+ .map 自身 `[greed]` 声明的形状模板做匹配随机挑选。即“形状模板匹配”实现随机选图，房间每次进本可不同。
- **拓扑形态**：既有纯线性（布万加 `BBFFFFFFFFFFFFFFFFEE` 一条直线 10 格），也有树状分叉（格拉卡 6×3 网格含上下岔路）。起点/终点可给多组候选坐标随机二选一（洛兰 start `0 0 1 0`、boss `0 2 1 2`；格拉卡 boss `5 0 3 2` 两个可能位置各绑指定图 1030/1031）。
- **多套 `[maze info]`** = 同一副本多套布局变体，由 `[quest connection] 任务ID` 按主线进度选择（布万加 7 套，随任务房间从 2 格长到 10 格）。

**注意**：本报告与工程既有笔记 `Notes/dnf源码研究/13-地图与战斗场景系统整体架构认知.md`、`Notes/dnf源码研究/原始笔记/R4-M-地图与战斗场景.md` 结论一致并新增实证（格拉卡 boss 双候选绑图、warroom 疲劳字段、[boss room entrance condition] 样本、清关条件样本）。各字母精确的开门位掩码在数据文件中仍不可见（引擎侧逻辑），AA 疑似万能格。