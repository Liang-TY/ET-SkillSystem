# R6-M-地图布局考察

# DNF 地图布局定义考察报告（pvf/map/ 目录）

## 1. 顶层结构

`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\map\` 共 242 个条目，全部是**目录**（每个目录 = 一个“地图区域/地下城群”），无顶层散文件。

- 命名规律：`{编号}_{英文地名}`（如 `148_prison1`、`95map/`、`towerofdespair/`）或纯地名（`alfhlyra`、`training`、`tutorial`）。带数字前缀的是后期版本地图，会再嵌套一层子目录（`95map/wasteland`、`anton_raid/223_hatchery3`、`metrocenter/hell` 等）。
- 统计：`.map` 文件共 **4521** 个，`.til` 瓦片 **7535** 个，`.ani` 场景动画 **17555** 个。
- 每个区域目录的标准子目录约定：**`tile/`（.til 瓦片+碰撞）+ `animation/`（.ani 场景物）**，部分后期图另有 `action/`、`new_tile/` 等。另有跨图共享资源目录 `map/common/`（如 `mapcover.ani`）。
- 房间文件命名：**`{地图ID}({格子x},{格子y}){用途}.map`**，如 `148_prison1` 下：

```
18601(0,0)n.map
18605(1,0)n.map
18606(2,0)s.map      ← s = 起点房
18627(2,2)b.map      ← b = Boss房
18649(3,2)n.map      ← n = 普通房
```

后缀是描述性的（normal/boss/start/named/hell/gate…，共 481 个地图无坐标后缀，多为城镇/单间图）。**权威的房间类型来自 .map 内部 `[type]`**：全库统计 `[normal]` 3788、`[boss]` 600、`[dummy]` 75、`[cover]` 1。坐标格含义由地下城文件（`dungeon/.../*.dgn` 的 `[maze info]`）定义，见第 3 节。

## 2. 典型文件完整引用

### 2.1 普通战斗房间 `map\148_prison1\18649(3,2)n.map`（2350 字节，完整）

```
#PVF_File

[player number]
	2	8

[pvp start area]
	0	0	0	0
	0	0	0	0
	0	0	0	0

[dungeon]
	148
[/dungeon]

[type]
	`[normal]`

[greed]
	`FF FF`

[tile]
	`Tile/Tile00.til`
	`Tile/Tile00.til`
	`Tile/Tile01.til`
	`Tile/Tile01.til`
[/tile]

[pathgate pos]
	48	240	852	238	1171	476	1209	589

[sound]
	`M_RINGWOOD_PRISON`
	`AMB_PRISON`
[/sound]

[animation]
	`Animation/chain.ani`	`[normal]`	619	55	0
	`Animation/chain.ani`	`[normal]`	217	64	0
	`Animation/jail00.ani`	`[normal]`	218	134	0
	`Animation/crystal3.ani`	`[normal]`	885	312	0
	`Animation/crystal3.ani`	`[normal]`	849	352	0
	`Animation/bottom_obj04.ani`	`[bottom]`	863	174	0
	`Animation/bottom_obj02.ani`	`[bottom]`	505	410	0
	`Animation/bottom_obj01.ani`	`[bottom]`	303	358	0
	`Animation/bottom_obj00.ani`	`[bottom]`	679	173	0
	`Animation/broken_lantern01.ani`	`[bottom]`	580	196	0
	`Animation/bottom_obj03.ani`	`[bottom]`	784	378	0
	`Animation/broken_lantern01.ani`	`[bottom]`	374	337	0
	`Animation/broken_lantern01.ani`	`[bottom]`	242	324	0
	`Animation/crystal0.ani`	`[bottom]`	150	175	0
	`Animation/crystal4.ani`	`[bottom]`	129	193	0
	`Animation/crystal4.ani`	`[bottom]`	627	153	0
	`Animation/crystal4.ani`	`[bottom]`	299	328	0
	`Animation/crystal4.ani`	`[bottom]`	508	276	0
	`Animation/crystal5.ani`	`[bottom]`	144	193	0
	`Animation/crystal1.ani`	`[closeback]`	593	71	0
	`../common/mapcover.ani`	`[close]`	880	0	0
[/animation]

[passive object]
	11254	461	351	0
	11257	101	153	0
	11254	460	155	0
	11257	341	153	0
	11301	807	169	0
	11254	544	155	0
	11254	622	155	0
	11302	907	314	0
	11299	27	344	0
	11306	213	349	0
	11306	305	350	0
	11302	898	352	0
	11254	384	352	0
	11257	141	351	0
	11301	39	367	0
	11326	936	374	0
	11301	882	390	0
[/passive object]

[monster]
	65019	1	0	585	193	0	1	1	`[fixed]`	`[normal]`
	65020	1	0	371	193	0	1	1	`[fixed]`	`[normal]`
	65021	1	0	267	222	0	1	1	`[fixed]`	`[normal]`
	65019	1	0	512	229	0	1	1	`[fixed]`	`[normal]`
	65019	1	0	442	263	0	1	1	`[fixed]`	`[normal]`
	65019	1	0	346	288	0	1	1	`[fixed]`	`[normal]`
[/monster]

[monster specific AI]
	`[normal]`
	`[normal]`
	`[normal]`
	`[normal]`
	`[normal]`
	`[normal]`
[/monster specific AI]

[event monster position]
	405	271	0
	335	224	0
	408	307	0
	274	297	0
[/event monster position]

[map name]
	`PVP 无名`
```

### 2.2 瓦片碰撞文件 `map\148_prison1\tile\tile00.til`（1002 字节，完整）

```
#PVF_File

[IMAGE]
	`Map/Season4/Abnoba/Prison1/Tile/Tile.img`	0

[img pos]
	80

[pass type]
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	2	2	2	2	2	2	2	2	2	2	2	2	2	2
	0	0	0	0	0	0	0	0	0	0	0	0	0	0
	0	0	0	0	0	0	0	0	0	0	0	0	0	0
	（……以下全 0，共 30 行）
```

扩展瓦片 `map\148_prison1\tile\tile_ex_map_18609_00.til`（完整）：

```
#PVF_File

[IMAGE]
	`Map/Season4/Abnoba/Prison1/Tile/Tile_ex.img`	0

[img pos]
	80

[tile type]
	`[extended]`

[pass type]
	0	0	2	2	2	2	2	2	2	2	2	2	2	2
	0	0	2	2	2	2	2	2	2	2	2	2	2	2
	0	0	2	2	2	2	2	2	2	2	2	2	2	2
	0	2	2	2	2	2	2	2	2	2	2	2	2	2
	0	2	2	2	2	2	2	2	2	2	2	2	2	2
	0	2	2	2	2	2	2	2	2	2	2	2	2	2
	0	2	2	2	2	2	2	2	2	2	2	2	2	2
	0	2	2	2	2	2	2	2	2	2	2	2	2	2
	0	0	0	2	2	2	2	2	2	2	2	2	2	2
	0	0	0	0	2	2	2	2	2	2	2	2	2	2
```

### 2.3 城镇地图 `map\alfhlyra\alfhlyra.map`（2553 字节，节选关键段，城镇特有段全引）

```
[town movable area]
	9	183	22	258	31	5
	497	120	85	21	31	1
	2430	184	18	258	31	2
	1004	453	76	25	31	3
	321	442	81	29	31	4
[/town movable area]

[virtual movable area]
	16	184	2423	258
	496	129	88	58
	1003	433	76	36
	320	432	84	26
[/virtual movable area]

[far sight scroll]
	50
[middle sight scroll]
	70
[near sight scroll]
	110

[background animation]
	[ani info]
		[filename]
			`Animation/far00.ani`
		[layer]
			`[distantback]`
		[order]
			`[below]`
	[/ani info]
[/background animation]

[animation]
	`Animation/AlfSeriaHouse.ani`	`[closeback]`	533	40	0
	`../Elvengard/Animation/SeriaLight01.ani`	`[closeback]`	534	138	0
	`../Town/event/npc_move.ani`	`[normal]`	1931	-75	0
[/animation]

[passive object]
	947	-2560	-120	0
	947	-1920	-120	0
	（……每 640px 一个，共 9 个，y=-120 —— 背景循环物件）
[/passive object]

[NPC]
	44	`[left]`	2138	128	0
	26	`[right]`	1971	152	0
	23	`[left]`	321	179	0
	24	`[left]`	1582	180	0
	25	`[left]`	1117	182	0
[/NPC]
```

（该图 `[tile]` 与 `[extended tile]` 各 11 个 .til，一一对应。）

## 3. 字段结构总结

### 3.1 .map 文件（单间布局，中缀格式 `#PVF_File`）

全库 4518+ 个 .map 的段落普查（按出现次数）：`[player number]`/`[pvp start area]`/`[type]` 4518、`[tile]` 4467、`[sound]` 4415、`[pathgate pos]` 4279、`[animation]` 4266、`[dungeon]` 4135、`[monster]` 3886、`[passive object]` 3871、`[background animation]` 3694、`[far/middle/near sight scroll]` 3495/3313/3277、`[extended tile]` 1966、`[special passive object]` 1673、`[NPC]` 1018、`[dungeon start area]` 789、`[town movable area]` 239、`[virtual movable area]` 238 ……（小众段：`[way point]`、`[move layered map]`、`[warp map fixed direction]`、`[customized screen edge]`、`[camera edge exception]`、`[camera edge recovery object]`、`[block path]`、`[absolute start path]`、`[limit player move area]`、`[static player start pos]`、`[phase]`、`[ai character]`、`[map dialog]` 等约 90 种）

按功能归类：

| 功能 | 段落 | 格式与含义 |
|---|---|---|
| 房间归属/类型 | `[dungeon] 148`、`[type] \`[normal]/[boss]/[dummy]/[cover]\`` | 所属地下城 ID、房间类型 |
| 瓦片层 | `[tile]`（多行 .til 路径，从左到右的水平分段）、`[extended tile]`（扩展层，与 `[tile]` 数量一一对应）、`[tile option] \`[upper]\`` | 房间 = N 个 14 列 .til 水平拼接；扩展瓦片可加高/半格地形 |
| 碰撞层 | 在 .til 的 `[pass type]`（见 3.2） | 0=不可通行，2=可走；扩展瓦片同格式 |
| 怪物刷新 | `[monster] id ? ? x y z ? ? \`[fixed]\` \`[normal]\``（x,y,z 为像素坐标，如 `65019 1 0 585 193 0 1 1 \`[fixed]\` \`[normal]\``；Boss 房出现 `\`[boss]\`` 行）、`[monster specific AI]`（与 monster 行一一对应）、`[event monster position]`（x y z 三元组）、`[monster spawn pos]`、`[champion]`、`[monster team]`、`[blood monster]` |
| 出生点/触发区 | `[dungeon start area] x y w h`（如 `155 175 129 127`，进图玩家落点矩形）、`[pvp start area]`（3 行 x y w h）、`[pvp practice start area]`、`[summon start area]`、`[static player start pos]`（按 `[player 1]` 分玩家）、`[tournament start area]` |
| 房间门 | `[pathgate pos]` 8 个数（4 个 x,y 点，定义出/入门区域多边形，如 `48 240 852 238 1171 476 1209 589`）、`[pathgate recognize range] 65 300 65 300 0 0 0 0`、`[pathgate object]`、`[block path]`（`\`[close]\`/\`[open]\`` 序列）、`[absolute start path] \`[left]\``、`[warp map fixed direction] \`[left]\`` |
| 传送/切图 | `[way point] 100 245 1000 245 550 245 …`（巡逻路点）、`[move layered map]`（`[zpos under] -300` 上下层切换）、`[map over move ani]`、`[limit player move area]`（`[player 1] 0 270 5000 45` 限制玩家活动矩形） |
| 摄像机 | `[customized screen edge]`（`[left] 50 [right] 750 [top] 50`，即 800 宽屏幕留边）、`[camera edge exception] 1`、`[camera edge recovery object] 12684 1`、`[far/middle/near sight scroll] 50/70/110`（三层视差滚动速度） |
| 城镇专属 | `[town movable area] x y w h 方向ID 索引`、`[virtual movable area] x y w h`（真可行走矩形）、`[NPC] id \`[left]\` x y z`、`[visible town minimap]`、`[map character move sound]`、`[guild agit entrance info] 833 196 78 61 -2 -2` |
| 表现层 | `[animation] ani路径 层名 x y z`（层名：`[normal]/[bottom]/[closeback]/[close]/[distantback]` 渲染排序）、`[background animation]`（嵌套 `[ani info]`）、`[shadow type] \`[mirror]\``、`[show dust]`、`[background effect] \`[swing]\``、`[sound]`（BGM+环境音）、`[map loading image path] \`Sprite\Interface\...\`.img` |
| 关卡逻辑 | `[phase]`（`[duration] 20000` + `[type] \`named\`` + `[action assign]`）、`[buff]`、`[ai character]`、`[special passive object]`（`49014 1064 164 0 1` + 掉落物 `\`[item]\` 1 3 -1 -1 -1`）、`[map dialog]`（`<apc::26508>` 对白）、`[conditional summon monster]` |

### 3.2 .til 瓦片（碰撞 + 图像引用）

- **格子尺寸**：基础瓦片固定 **14 列 × 30 行**；扩展瓦片（`[tile type] \`[extended]\``）行数可变（本例 10 行）。每格像素尺寸在 pvf 文本中**无显式字段**（未确证，社区常取 80px/格推导自 `[img pos] 80`/`[background pos] 80`，`[customized screen edge]` 证实屏幕为 800 宽）。
- `[pass type]`：0=阻挡、2=可通行（基础图典型为上 12 行可走、下 18 行为 0）。
- `[IMAGE] \`.../Tile.img\` 帧号` + `[img pos] 80`：瓦片贴图来源与对齐基准。

### 3.3 迷宫布局（与 dungeon/ 交叉，解析文件名坐标）

`dungeon\stormofmetastasis\abnoba\prison1.dgn` 的 `[maze info]`（完整引用）：

```
[maze info]

[size]
	5	5

[greed]
	`JJFFFFFFMM
	 KKAAAAAAKK
	 LLFFFFFFOO
	 KKAAAAAAKK
	 DDFFFFFFGG`

[map specification]
	0	0	18601
（…每格一条 `x y 地图ID`，与文件名 `18601(0,0)n.map` 对应…）
	2	2	18627

[start map]
	2	0	2	4
[/start map]

[boss map]
	2	2
[/boss map]
```

`[greed]` 字母表（数据实证，每格 2 字符）：`AA`=空格无房间；单门 `BB`/`JJ`=右、`EE`/`MM`=左、`CC`=上、`II`=下；双门 `FF`=左右、`KK`=上下、`DD`=上右、`GG`=上左；三门 `LL`=上右下、`OO`=上左下、`NN`=左右下、`HH`=上左右；`PP`=四向（据 lorieninside/mirkwood 连通性推证）。门连通为“任一侧标记即开”。`[start map]`/`[boss map]` 给起点/Boss 候选格坐标。城镇侧的 .twn（如 `town\alfhlyra.twn`）用 `[area] 索引 + map路径 + [minimap rect] + [gate]/[dungeon gate]` 把城镇各区地图串成一张大图。

## 4. img 资源引用链

map 树内**不含任何 .img**，贴图统一引用外部图像脚本路径（不区分大小写、以游戏镜像根为基准）：

1. **瓦片贴图**（.til → img）：`[IMAGE] \`Map/Season4/Abnoba/Prison1/Tile/Tile.img\` 帧号`（第二个数字为 img 内帧/场景索引，tile00=0、tile01=1）。
2. **场景物动画**（.ani → img）：每帧 `[IMAGE] \`Map/Season4/Abnoba/Prison1/ani/chain.img\` 帧` + `[IMAGE POS] -8 -272`（贴图偏移）+ `[DELAY] 1200`；如 `map\common\mapcover.ani` 引用 `\`Map/Common/mapcover.img\``。
3. **.map 直接引用**：`[map loading image path] \`Sprite\Interface\TowersStoryImage\Bone.img\``、`[show dust] \`Common/CommonEffect/Animation/SandDust.ani\``。
4. **.dgn/.twn 侧引用**（同体系）：`[cutscene image] \`Map/CutScene/Abnoba.img\` 0`、`[minimap image] \`Map/MiniMap/Act2.img\``、`[worldmap pattern info] \`WorldMap/SelectdungeonSlot/Abnoba.img\` 4`、`[entering title] \`Title/Alfhlyra.ani\``。

即：**map/ 树 = 纯文本布局与逻辑（.map/.til/.ani），所有像素资源通过 `Map/...`、`Sprite/...`、`WorldMap/...` 等虚拟路径指向镜像中的 .img 图像脚本文件**。

### 关键路径索引
- 房间示例：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\map\148_prison1\18649(3,2)n.map`、`18627(2,2)b.map`、`18606(2,0)s.map`
- 城镇示例：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\map\alfhlyra\alfhlyra.map`、`d_alfhlyra.map`
- 瓦片示例：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\map\148_prison1\tile\tile00.til`、`tile_ex_map_18609_00.til`
- 迷宫定义：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\dungeon\stormofmetastasis\abnoba\prison1.dgn`
- 城镇组装：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\town\alfhlyra.twn`