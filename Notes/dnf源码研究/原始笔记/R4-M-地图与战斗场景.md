# R4-M：DNF 地图/战斗场景系统架构（研究原始笔记）

> 第4轮（补充轮）Agent M 原始笔记。任务：三层目录关系/进图流程/怪物生成管线/房间流转/难度缩放。
> 已综合进：13-地图与战斗场景系统整体架构认知.md

---

## 0. 总览：数据规模

| 类型 | 数量 | 索引文件 |
|---|---|---|
| 副本 .dgn | 495 个（lst 收录 480） | `dungeon/dungeon.lst` |
| 地图 .map | 4521 个（lst 收录 4093） | `map/map.lst` |
| 怪物 .mob | 3689 个（lst 收录 3402） | `monster/monster.lst` |
| APC/AI角色 .aic | 842 个 | `aicharacter/aicharacter.lst` |
| 帧级行为脚本 .act | 全 pvf 共 52137 个 | 无总 lst，由 .mob 逐个引用 |
| AI 决策树 .ai | monster 下 10006 个 | 同上 |
| 被动对象 .obj | 16028 个 | `passiveobject/passiveobject.lst` |
| NPC 402 / 城镇 .twn 38 / worldmap .wdm 40+ | — | `npc/npc.lst` 等 |

所有文件均为 `#PVF_File` 明文格式。

## 1. 层级关系：worldmap → dungeon → maze(房间网格) → map

**stagemap 不是关卡层**（全目录只有 1 个 advancealtar.stm，是特殊模式的选层 UI）。真正的层级：

```
region/  阿拉德大陆分区(.rgn) —— 聚合若干城镇 [towns] 28 2 3 4 5 18
  └─ town/ 城镇(.twn) —— 每个 [area] 引用一个 .map（城镇街道也用同款地图格式）
        └─ [dungeon gate] N        ← 从城镇此区域可进的副本ID
worldmap/ 世界地图(.wdm) —— [dungeon] 21 22 23... 列出该地区可选副本ID（选图UI数据）
  └─ dungeon/ 副本(.dgn) —— 定义 [maze info] 房间网格、难度、缩放、事件怪
        └─ [greed] 迷宫网格（每格2字符=房间形状模板）
              └─ map/ 房间(.map) —— 每格房间一个实例：地形tile、刷怪表、门、出生点、装饰
```

- `map.lst` 的 ID 全局唯一；.map 内 `[dungeon] 7` 反向声明归属副本；`hell_97.map`/`b1001.map`(boss)/`s1001.map`(start) 为同一房间的变体。
- 新版地图文件名直接编码迷宫坐标：`map/144_redcrystal_forest/8153_(0.3)start.map`。

### .dgn 字段全解（样本：dungeon/act1/grakkarak.dgn 格拉卡）
- 元信息：`[name]` `[entering title]`(进图标题动画) `[cutscene image]` `[minimap image]` `[worldmap pattern info] 1 5`(挂世界图哪个槽)
- 门槛：`[minimum required level] 10` `[basis level] 13`（怪物等级基准）`[limit party count]`
- 迷宫：`[maze info]` → `[size] 6 3`(列×行) + `[greed]`(房间形状网格) + `[map specification] 列 行 地图ID`（强制指定某房间用哪张图；缺省按形状模板从本副本 .map 池随机挑）+ `[boss map specification]` + `[start map] 列 行` + `[boss map] 列 行`
- **多个 [maze info] 块 = 多套布局变体**，由 `[quest connection] 0 4002 -1`（任务进度）选择——bwanga.dgn 有 7 套，随主线任务房间逐渐增多
- 门：`[pathgate object] 391..400`（10 个门/墙对象 ID）
- 深渊：`[hell dungeon] 1` `[seal door map index] 97`(专用map→hell_97.map) `[seal door pos] 3 0` `[hit count] 50×5`
- 事件怪：`[event monster] 1 10000 61726 1 300 80 1` → 61726=黄金哥布林（随机乱入），.map 的 `[event monster position]` 给它留刷点
- 难度：`[monster difficulty bonus]` 13行×5列矩阵、`[champion] 3 6 9 12 14`（每难度精英怪数量）
- 其他：`[boss room entrance condition] [hunt monster] 4 50073 1 1...`（杀指定怪才开门进Boss房）、`[clear condition] [clear map] 15051 1`、`[named monster map pos]`、`[use zoom rate] 1.0`

## 2. 进图流程（数据层面）

```
城镇街道(.twn [area]) ──走到 [dungeon gate] 8 按X──► 弹出该门对应副本
   →（或世界地图 .wdm [dungeon] 列表点选）
   → dungeon.lst[7] = Act1/GrakKarak.dgn
   → 客户端展示 [cutscene]/[entering title]，选难度(普通/冒险/勇士/王者/英雄)
   → 引擎按难度+任务状态选定 [maze info] 变体 → 生成迷宫网格
   → 逐格：按 greed 形状码 + 房间类型([normal]/[boss]/start) 匹配 [dungeon]=7 的 .map 实例化
   → 玩家落在 [start map] 格的 [dungeon start area] 矩形内
   → 清怪 → 门解锁 → 走门 → 相邻格房间实例化（新房间+新刷怪表）→ … → Boss房 → 通关
```

### 迷宫 greed 码（两级使用）
- `.dgn [greed]`：size 6 3 → 3 行字符串、每格 2 字符。**NN=空格（无房间）**；其余字母=房间"形状模板"（决定四面哪边有门）。
- `.map [greed]`：声明本地图实现的是哪个形状模板，一格一字符、宽房间多屏多码（`FF FF`=2屏）。引擎做模板匹配选图。实测格拉卡迷宫码（AA BB CC DD FF GG HH KK MM OO）与地图池吻合（迷宫格(5,2)=GG ↔ 唯一的 `1010.map GG GG`）；**AA 疑似万能格**（迷宫大量 AA 但地图池无 AA 图）。
- 已实证：`BB`=起点房(仅右门)、`EE`=终点房(仅左门)、`FF`=左右走廊、`NN`=空。每个字母的精确开门位掩码=引擎侧——**缺口**。

## 3. 地图文件结构（.map）

样本：`map/grakkarak/1002.map`（普通房）、`b1002.map`（Boss变体）、`hell_97.map`（深渊房）、`8153_(0.3)start.map`（新版）。字段：

**地形/碰撞（横版格斗核心）**
- `[tile]`：每屏一个 `.til` 引用。`.til` = `[IMAGE]`贴图 + `[pass type]` **32行×14列的碰撞网格**，取值：`0`=实心不可穿(地面/墙)、`1`=可从下方穿越的平台、`2`=可通行区。**地形=贴图与碰撞格一一对应，全数据驱动**。
- `[extended tile]`：装饰层扩展；`[greed]`：形状模板；`[pathgate pos]`：4 个门位坐标（左/右/上/下）。

**表现**
- `[far/middle/near sight scroll]` 56/90/110——**三层视差**滚动速率；`[background animation]`（distantback/middleback 层）；`[animation]` 前景装饰（ani+层+坐标）；`[sound]` BGM+环境音；新版 `[map loading image path]`、`[opening bgm] 'M_XXX_INTRO' 17500`。

**出生点**
- `[dungeon start area] x y w h`（玩家落点矩形，仅 start 变体有效）；`[player number] 2 8`；`[pvp start area]`；`[apc random point] 150`。

**刷怪表（每房一份，进房即刷）**
- `[monster]` 每行：`怪物ID flag 等级Override x y ? n n [fixed] [normal]`（例 `5001 0 12 966 197 0 1 1`；Boss 行 `15 1 0 262 247 0 2 1 [fixed] [boss]`）。**刷点模式只见 [fixed] 固定点，无波次字段——怪全部进房生成，波次/Boss阶段由 .act 触发器和脚本层实现**。
- `[monster specific AI]`：AI 难度标签；`[monster team]` 敌我编队权重；`[monster spawn pos]`（死亡之塔随机刷点）。
- `[NPC] 1000 [left] 0 0 0`；`[ai character] 600 473 255 0 [monster] [normal] 0 0`——**本房间内刷 APC**。

**可破坏物/掉落/陷阱**
- `[passive object] objID x y ?`——249/250=大/小草丛、230=树（可破坏遮蔽物）
- `[special passive object]`：`221 Barrel.obj / 222 TreasureBox.obj` + 子标签 `[item]`(地上物) / `[trap] 2411 1 2 5 -1`(陷阱伤害区)；深渊房 `30519 SealedGateOfHellDungeon3.obj` + `[hellparty]` 掉落表（物品ID 权重 次数）。

## 4. 房间流转与门机制

- 门对象按 **10 槽位**配置在 .dgn `[pathgate object]`（格拉卡 391-400）：`左门Normal/左门Boss/左墙/右Normal/右Boss/右墙/上Normal/上Boss/下Normal/下Boss`。房间实例化时引擎按迷宫拓扑在 `[pathgate pos]` 位置放门或封墙。
- 门 `.obj` 内含 4 段动画：常驻/Close/**Open**/Light——**清完房 → 门播 Open 动画解锁**；开门判定在引擎（怪物全灭），ACT 脚本可感知对象销毁（`[ON DESTROY OBJECT]`）。
- 复杂流转：`[boss room entrance condition] [hunt monster]`（条件门）、`[clear condition] [clear map]`（自定义通关）、.act 的 `[SET ACTION]/[CHANGE AI]/[TELEPORT]`、sqr/map/anton.nut 按 dungeonIndex+mapIndex 挂自定义机制。

## 5. 怪物生成管线（配置 → 生成 → 行为驱动）

### 5.1 配置：monster.lst → .mob
`.mob` 字段：
- 属性框架：`[ability category] '[HP MAX]' * 70`（系数×等级基础表）、`[level] min max`、16 种 `[xx resistance]`、`[move/attack/cast speed]`、`[hit recovery]`、`[weight]`
- AI 参数：`[sight] [warlike] [attack delay] [targeting nearest] [targeting time term] [think term] [intelligence] [vision] [keep range distance] [is named monster]`
- 分类：`[category] [human][goblin][melee combat]...`（技能加成判定用）
- 掉落：`[item] 1062 50`（itemID 权重）、`[common champion drop item]`（精英掉落）
- 战斗数据：`[attack kind]`、`[attack info]`→每技能一个 `.atk`（命中反应定义；**判定框不在 .atk，在 .ani 帧**）
- 装备/形象：`[equipment]`、`[face image]`（怪物卡片头像）

### 5.2 行为驱动：两代体系并存
**旧式（Act1 老怪）**：.mob 直接引用动作动画 `[waiting motion]/[move motion]/[damage motion]/[attack motion] Attack1.ani`，攻击配 .atk；行为=引擎默认 AI（sight/warlike 字段驱动的追击+普攻），action.ai 是空壳。

**新式（95版起，5万+ .act）**：
- .mob 引用**状态机**：`[waiting action] Action/Stay.act`、`[attack action] Action/Attack_0..12.act`、`[cooltime]`（每技能CD）
- `.act`（帧级行为脚本）：
  - `[MOTION]`：`[BASE ANI]` 主动画 + `[SUB ANI]` 分层子动画 + `[SOUND] 音效 帧号`
  - `[TRIGGER]`：`[FRAME] 起 止`(帧窗口)/`[LIMIT] n`(限次)/`[WHICH] [ME]|[ALL MONSTER TEAM]|[PLAYER]`/`[CHECKUP]` 条件/`[DO BEHAVIOR] [ME] 行为序号`
  - `[BEHAVIOR]`：`[ATTACK] n`、`[CREATE PASSIVEOBJECT]`、`[SUMMON MONSTER][INDEX][LEVEL][POS]`、`[TELEPORT]`、`[SET ACTION][NOW]`（切状态）、`[CHANGE AI]`、`[MOVE ME]`、`[FLASH SCREEN]`、`[SHAKING]`、`[SAY SPEECH]`、`[DESTROY]`、`[SET DAMAGE BOX][ON/OFF]`
- **判定框在动画里**：.ani 每帧可挂 [ATTACK BOX]/[DAMAGE BOX]，ACT 的 FRAME 触发与 .ani 帧一一对应。

### 5.3 AI 决策树：.ai 文件（明文）
`.mob [ai pattern]` 按 5 个难度各列 4 个文件：`Event.ai / DestinationSelect.ai / Action.ai / MoveMethod.ai`。
- `Action.ai` = 嵌套 `[think]` 决策树：条件函数带参（`is target in attack area() 150.0 130.0 100.0 100.0`、`is the skill in cooltime() 9`、`get random()`），叶子 `[return] 技能序号`（对应 [attack action] 列表序号）
- MoveMethod（怎么走）/DestinationSelect（去哪）/Event（事件反应），简单怪全部 `[return] 0/-1`

### 5.4 aicharacter/（APC）与 monster/ 的关系
- APC = **用玩家体系做的 AI 角色**（陪跑队友/敌对NPC/Boss化人形怪）。`.aic` = 玩家 Build：`[skill] 技能ID 等级`、`[equipment]`、`[quick skill]/[quick item]` + AI 参数 + 4 件套 ai 文件。
- **关键差异**：APC 的 Action.ai 返回**字符串=按键宏名**，由 `[key stream]` 映射到 `key/*.key` 录制的**虚拟输入序列**（`[input] 'target current direction' 100 / 'a' 30`）——**APC 是"回放玩家操作"驱动，怪物是"技能序号→.act"驱动**。
- 放置：.map `[ai character]` 行直接刷；arad_aic/2013sao 为剧情/联动 APC 变体库。

### 5.5 sqr/ 脚本层
- 副本钩子：`onStartDungeon_职业名`/`onStartMap_职业名`/`onEndMap_*`（sqr/apjh/onstartdungeon.nut、onstartmap.nut）、`onDungeonClearMonsterEvent`（清怪检测）。
- `sqr/map/`：dungeon.nut(总入口) + anton.nut/watchtower/tayberrs——按 `sq_GetDungeonByStage→dungeonIndex + sq_GetMapIndex→mapIndex` 给特定房间挂 appendage 实现自定义图机制。
- `sqr/monster/`：28 个怪物用 appendage（ap_set_superarmor/ap_set_max_hp/rage/ap_rage(狂暴UI)/ap_attack…）——.mob 无法表达的运行时逻辑全走这层。

## 6. 难度与缩放
- **5 档难度**（普通/冒险/勇士/王者/英雄），数据处处按 5 列：.dgn `[champion]` 5 值、`[hit count]` 5 值、`[monster difficulty bonus]` 13行×5列；.mob `[ai pattern]` 恰好 `[easy][medium][hard][ultimate][hero]` 5 组——**难度同时改属性数值和 AI 决策树文件**。
- `[monster difficulty bonus]`（格拉卡）13 行（行1 5 10 20 30 30 疑似等级加值；行2 经验加值；行5-13 对应好战性/视野/回避率/血量/攻速/移速/抗性/伤害/防御/硬直等），与全局表 monsterapcdifficultybonus.tbl 的 11 组属性对应（本 pvf 无该 tbl，行序映射=部分缺口；外部佐证：无笙博客、dnf.arad.ink）。
- 等级基础表：`monster/commonmonsterbaseparameter.tbl` / `bossmonsterbaseparameter.tbl`——按 1~140+ 等级给血量/攻防基数（Boss 表约为普通表 2.5 倍），.mob 的 `[ability category]` 系数乘上去。
- 精英/绿名（champion）：引擎按 `[champion] 3 6 9 12 14` 从普通怪里随机升级 N 只，吃 [common champion elemental property]/[common champion drop item]。
- 布局缩放：多 [maze info]（任务态变体）；`[ancient dungeon]`、`[designate dungeon difficulty]` 锁难度。

## 7. region / town / worldmap 简述
- `region/arad.rgn`：`[towns] 28 2 3 4 5 18`——纯分组。
- `town/hendonmyre.twn`：N 个 `[area]`，每区一个 .map（街道地形与副本同格式）+ `[dungeon gate] 8`（该区可进的副本门）+ `[gate]`（区域间传送点）。
- `worldmap/behemoth.wdm`：背景图 + `[dungeon] 21 22...`（本图可点副本，对应 .dgn 的 [worldmap pattern info]）+ 深渊入场券。
- `stagemap/`：仅特殊模式选层 UI。

## 8. 关键文件路径清单
**索引**：dungeon/dungeon.lst、map/map.lst、monster/monster.lst、aicharacter/aicharacter.lst、passiveobject/passiveobject.lst
**教学样本（实证）**：
- 副本：dungeon/act1/grakkarak.dgn（最小完整例）、act9/trombe.dgn（双迷宫变体）、act5/bwanga.dgn（7变体）、dungeonsample.dgn（空模板=字段清单）
- 地图：map/grakkarak/{1002,b1002,hell_97}.map、144_redcrystal_forest/8153_(0.3)start.map、deadtower/007f.map（随机刷点+APC）
- 地形：map/grakkarak/tile/forestover.til（碰撞网格）、150_spider_kingom/tile/tile03.til（平台型 pass=1）
- 怪物：monster/goblin/hillgab.mob（旧式）、tau/shauta.mob（旧式Boss）、95monster/newmonsters/daybreak/mammon/mammon.mob（新式全套）
- ACT：mammon/action/attack_0.act（简单）、daybreak/molech/action/attack_1.act（复杂多触发器）、goblin/action_goblin/{summon,teleport}.act
- APC：aicharacter/swordman/kagemaru/{kagemaru.aic,key/a.key}、arad_aic/2013sao/aganzo/aganzo.aic
- 门：passiveobject.lst(391-400) + 95object/mapobject/pathgate/.../normal_d.obj
- 脚本：sqr/loadstate.nut、sqr/apjh/{onstartdungeon,onstartmap}.nut、sqr/map/{dungeon,anton}.nut、sqr/monster/、知识库 14-ACT脚本说明.md

## 9. 缺口清单
1. greed 形状码→开门方向的精确映射表（已实证 NN/BB/EE/FF，其余推断；AA 疑似通配待证）。
2. [monster difficulty bonus] 13 行的逐行属性名（有外部 11 组属性佐证，行序映射未定）。
3. 清房→开门的引擎判定细节与门对象 10 槽中"墙"与 -1 的完整规则。
4. .map [monster] 行第 2/6/7/8 列与 [monster specific AI] 标签的精确语义。
5. [special passive object item]/[event monster] 各数值列含义。
6. commonmonsterbaseparameter.tbl 23 列逐列属性名。
7. stagemap .stm 的 [slot type] 数值含义（非核心）。

Sources: [无笙博客 - 台服DNF的pvf如何调整全局怪物属性](https://blog.aicq.icu/archives/adjusting-monster-attributes-in-taiwan-DNF-pvf), [DNF单机论坛 - 怪物血量修改](https://dnf.arad.ink/thread-3768-1-1.html)
