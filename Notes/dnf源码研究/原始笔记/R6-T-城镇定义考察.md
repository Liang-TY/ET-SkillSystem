# R6-T-城镇定义考察

# 城镇定义考察报告（pvf/town/）

## 1. town/ 顶层结构

共 **43 个条目**：

| 类别 | 数量 | 说明 |
|---|---|---|
| `.twn` 文件 | 38 | 城镇定义（含 2 个 hendonmyre 变体：`hendonmyre.twn` / `hendonmyre.jpn.twn`） |
| 子目录 | 2 | `origin/`（仅 1 个文件 `38_underfoot.twn`）、`title/`（36 个 `.ani` 城镇入场标题动画） |
| 清单/字符串 | 3 | `town.lst`（城镇 ID→文件映射，38 项）、`town.jpn.lst`（日服版，34 项）、`town.kor.str`（空文件，2 字节） |

`.twn` 文件都很小（275~1615 字节），说明城镇定义只是“壳”，实际场景数据在 `map/` 目录的 `.map` 文件里。`town.lst` 片段（ID 即游戏中城镇编号，选角色界面对应此表）：

```
1
`Elvengard.twn`
2
`Underfoot.twn`
3
`Sewer.twn`
...
36
`origin/38_underfoot.twn`
```

## 2. 典型城镇文件原样引用

### 2.1 elvengard.twn（艾尔文防线，545 字节，3 个区域）

```
#PVF_File

[entering title]
	`Title/Elvengard.ani`

[cutscene image]
	`Map/CutScene/Elvengard.img`	0

[dungeon what must be cleared]
	-1

[area]
	0
	`Elvengard/Elvengard.map`
	`[minimap rect]`	372	242	438	268
	`[/minimap rect]`
	`[normal]`
[/area]

[area]
	1
	`Elvengard/Gate.map`
	`[minimap rect]`	399	230	412	239
	`[/minimap rect]`
	`[gate]`	474	234
[/area]

[area]
	2
	`Elvengard/D_Elvengard.map`
	`[minimap rect]`	444	242	473	267
	`[/minimap rect]`
	`[dungeon gate]`	1
[/area]

[name]
	`艾尔文防线`
```

### 2.2 underfoot.twn（暗黑城，1247 字节，13 个区域）

```
#PVF_File

[entering title]
	`Title/Underfoot.ani`

[cutscene image]
	`Map/CutScene/Underfoot_Town.img`	0

[dungeon what must be cleared]
	-1

[area]
	0
	`Town/Underfoot/Gate_Underfoot.map`
	`[gate]`	500	200
[/area]

[area]
	1
	`Town/Underfoot/Underfoot_GoSilverCrown.map`
	`[normal]`
[/area]

[area]
	2
	`Town/Underfoot/Underfoot_Event.map`
	`[normal]`
[/area]

[area]
	4
	`Town/Underfoot/Underfoot_NPCplaza.map`
	`[normal]`
[/area]

[area]
	5
	`Town/Underfoot/Underfoot_DT_Or_Icewall.map`
	`[dungeon gate]`	50
[/area]

[area]
	9
	`Town/Underfoot/Underfoot_DeathTower.map`
	`[dungeon gate]`	100
[/area]

[area]
	12
	`origin/town/05_hendonmyre/ximan.map`
	`[dungeon gate]`	102
[/area]

[name]
	`暗黑城`
```

（area 3/6/7/8/10/11 同构，为省篇幅省略，原始文件 92 行完整无缺。hendonmyre.twn 共 1615 字节、12 个 area，额外字段见下文第 3 节。）

## 3. 城镇定义字段结构（两层：.twn + .map）

### 3.1 .twn 层（城镇骨架 = 区域列表 + 门槛）

| 字段 | 含义 |
|---|---|
| `[entering title]` | 入场标题动画（`town/title/*.ani`） |
| `[cutscene image]` + 数字 | 进城过场图 + 播放标记 |
| `[dungeon what must be cleared]` | 进城前置需通关的 dungeon ID，`-1` = 无 |
| `[only server parsing dungeon what must be cleared]` | 仅服务端校验的前置（hendonmyre 为 `2`） |
| `[area]` N + `.map` 路径 | 区域编号 + 场景文件（路径相对 `pvf/map/`） |
| `[minimap rect]` x1 y1 x2 y2 | 该区域在大地图（世界图）上的小地图矩形 |
| `[normal]` / `[gate]` x y / `[dungeon gate]` N | 区域类型：普通城区 / 城镇间传送门（传送触发点坐标）/ 地下城入口（N 为 `dungeon.lst` 中的 dungeon ID） |
| `[name]` | 本地化名称 |
| `[limit level]` | 进入等级限制（hendonmyre 为 `3`） |

注意：城镇间移动和进地下城**不靠坐标传送门硬编码在 .twn**，而是“area 类型标记 + .map 内部通路”组合。

### 3.2 .map 层（单场景实体，城镇与地下城共用格式）

以 `map/Town/Underfoot/underfoot_npcplaza.map`（NPC 广场）为例，关键字段原样引用：

```
[town movable area]
	882	354	120	30	2	2
	1329	204	20	80	2	12
[/town movable area]

[virtual movable area]
	20	170	1320	170
	894	327	100	40
[/virtual movable area]

[sound]
	`M_UNDERFOOT1`
[/sound]

[NPC]
	64	`[right]`	150	145	0
	7	`[left]`	360	145	0
	6	`[left]`	770	145	0
	12	`[right]`	980	147	0
	149	`[left]`	570	145	0
	444	`[right]`	1170	145	0
[/NPC]
```

各字段（综合 elvengard.map / npcplaza.map / gate.map 归纳）：

- **通路**：`[town movable area]` = 矩形 `x y w h` + 两个链接参数（末尾数字对应同城镇内目标 area 编号，如 npcplaza 的 `2 12` 指向 area 12），即“走到此矩形→切换到相邻 area”；`[virtual movable area]` = 纯几何可走区（不含切换逻辑）。
- **NPC 布点**：`[NPC]` = `NPC_ID(引用 npc 脚本) 朝向 x y z`。NPC 不在 .twn 而在 .map。
- **出生点**：城镇 .map 复用了 PVP 字段 `[pvp start area]` / `[pvp practice start area]`（矩形出生区）+ `[player number]`。城内落点也由此类矩形承担。
- **传送门/出口**：对应 .twn 中 area 的 `[gate] x y`（去其他城镇的世界图传送点）和 `[dungeon gate] N`；个别 .map ���有专属扩展，如 gate.map 末尾：

```
[guild agit entrance info]
	833	196	78	61	-2	-2
[/guild agit entrance info]
```

- **BGM/音效**：`[sound]`（BGM 字符串如 `M_UNDERFOOT1`、环境音如 `AMB_FOREST_01`，可多行）。
- **氛围/表现**：`[animation]`（装饰动画：文件、图层 `[bottom]/[normal]/[closeback]/[distantback]`、x y z）、`[background animation]`（远/中景滚动层）、`[tile]`（地砖 .til，多行）、`[far/middle/near sight scroll]`（视差滚动速度）、`[background pos]`、`[shadow type]`、`[passive object]`（场景被动物件 `ID x y z`）。
- `[map name]` 为编辑器残留（多为 `PVP 无名`），非运行时名称。

## 4. 城镇 vs 地下城（纯数据层面）

对比例子：`dungeon/act1/lorien.dgn`（洛兰，96 行，即 elvengard `[dungeon gate] 1` 指向的 dungeon）：

```
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
...
[hit count]
	50	50	50	50	50
...
[hell dungeon]
	1
...
[event monster]
	1	5000	61726	1	300	80	1
[/event monster]

[monster difficulty bonus]
	5	10	20	30	30
	...
[/monster difficulty bonus]
```

纯数据层面区别：

| 维度 | 城镇 .twn | 地下城 .dgn |
|---|---|---|
| 拓扑 | **固定**：显式 `[area]` 列表，每 area 固定绑 1 个 .map | **随机拼图**：`[size] 2 3` + `[greed]` 字母网格（2 字符=1 格，JJ/EE/LL/MM/DD/GG 共 6 房间），`[start map]`/`[boss map]` 给候选格坐标（起点顶行、Boss 底行） |
| 地图来源 | 每 area 一个指定 .map 文件 | 房间从 `map/<dungeon名>/` 池子里随机抽（lorien 目录含候选 `4.map~9.map`、Boss 候选 `b1/b2/b3.map`、起点候选 `s0~s3.map`、地狱房 `hell_94.map`）——推断依据目录命名，字母池映射规则未见显式文件 |
| 门槛字段 | `[dungeon what must be cleared]`、`[limit level]` | `[minimum required level]`、`[basis level]`、`[experience increasing point]`（经验倍率） |
| 战斗数值 | 无 | `[champion]`、`[monster difficulty bonus]`（5 难度列的怪物成长表）、`[event monster]`、`[hit count]`、`[seal door...]`、`[hell dungeon]`、`[special passive object item]`、`[pathgate object]` |
| 世界图 | `[minimap rect]`（城镇在大地图上的贴图区） | `[worldmap pattern info]`（选图界面槽位图）+ `[minimap image]` |
| 描述文本 | 仅 `[name]` | `[name]` + `[explain]`（副本介绍长文本） |

一句话：**.twn 是“区域路由表”（固定场景 + 传送门 + 前置门槛），.dgn 是“房间生成器配置”（随机迷宫 + 难度/怪物/掉落数值）**；两者共用的 .map 才是承载 NPC 布点、通路矩形、BGM、氛围动画的场景实体层。

**相关绝对路径**：
- `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\town\town.lst`
- `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\town\elvengard.twn` / `hendonmyre.twn` / `underfoot.twn`
- `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\map\Elvengard\elvengard.map` / `gate.map`
- `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\map\Town\Underfoot\underfoot_npcplaza.map`
- `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\dungeon\dungeon.lst`、`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\dungeon\act1\lorien.dgn`
- `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\map\lorien\`（房间候选池）