# R2-F：DNF「按键 → 技能触发」链路研究笔记

> 第2轮 Agent F 原始笔记。任务：断点①——按键→技能触发链路 + 键位配置 + MP/CD 归属。
> 仓库里只有 pvf 数据 + 反提取的函数声明（language.dof.*.md，来源为客户端引擎的 Squirrel 绑定声明），**没有引擎 C++ 源码**；引擎内部行为是根据声明 + pvf 数据 + 脚本调用惯例推断的，均标注证据。
> 已综合进：04-按键到伤害全链路-总结.md

---

## 一、完整链路：按键盘 A 键 → `sq_IsEnterSkill(8)` 返回真 → 技能施放

DNF 的输入分 **4 层**：物理键 → 逻辑热键(OPTION_HOTKEY) → 技能定位（快捷栏槽位 或 .skl 指令序列）→ 脚本回调。逐环节：

| # | 环节 | 做什么 | 在哪实现 | 证据 |
|---|------|--------|----------|------|
| 1 | 物理键按下 | OS/输入层捕获 VK 键码 | 客户端引擎（C++，无源码） | `pvf\clientonly\hotkeysystem.co` 第四列就是 **Windows VK 码**（27=ESC、38=↑、65='A'、88='X'、90='Z'、32=Space、120=F9…） |
| 2 | 物理键→逻辑热键 | VK 码映射为 OPTION_HOTKEY_* 逻辑索引（如"技能快捷键1"=20），按 system/character/dungeon/quickchat 四类分组、分左右手 | 引擎读 **pvf 默认表** + 用户自定义覆盖 | 默认表：`pvf\clientonly\hotkeysystem.co`（明文）；**用户自定义不在 pvf 内**：官服存服务器账号数据、私服存 `game/cfg` + MySQL（WebSearch 证实） |
| 3a | 快捷栏路径：逻辑热键→技能ID | "技能快捷键1~7"（20~25,66）和"扩展技能1~7"（26~31,93）各自对应**该角色快捷栏对应槽位里放的技能**。槽位内容 = 角色数据（拖图标设置），**服务端角色 DB 保存，不在 pvf 内**（pvf 全目录无 default-quickslot/slot 赋值文件）。引擎的 CNRDSkillManager 维护槽位：`getSlotIndex(skillIndex)`/`getQuickSlotNumber`/`getEmptySlot`/`addAllKeyCommand(skillTree)`（把技能���上所有技能的按键指令注册进按键检查器） | 引擎 + 服务端角色数据 | `知识库\资源nut函数声明\language.dof.CNRDSkillManager.md` |
| 3b | 指令路径：按键序列匹配 | 每个技能在 .skl 里定义 `[command]`（如上挑=Z、三段斩=→+Z、崩山击=↓↓+X），指令里的 (SKILL)/(ATTACK)/(LEFT) 等记号由 `rdarkeyindex.dat` 定义索引；`CNRDCommandChecker`（角色持有，`obj.getCommandChecker()`）记录最近按键序列并匹配 | .skl 数据 + 引擎匹配器 | `pvf\skill\rdarkeyindex.dat`；`pvf\skill\swordman\upperslash.skl`（`[command] {6=}(SKILL){/command}`，`[command key explain] 操作指令 : Z`）；`pvf\skill\swordman\tripleslash.skl`；`language.dof.CNRDCommandChecker.md` |
| 4 | 技能键"被按下"登记 | 两条路殊途同归：匹配成功后引擎登记"技能 X 的键已按下"（内部状态），供 `sq_IsEnterSkill(X)` 查询；同时触发脚本回调 | 引擎 | `sq_IsEnterSkill` 声明："判断技能施放后，是否又按下了技能，返回 0只按了一次/1施放后又按了一次"，脚本统一用 `!= -1` 判定"按过"——`language.dof.character.md:1207`；实例 `pvf\sqr\common.nut:1429` |
| 5 | 脚本门禁①：`checkCommandEnable_技能名(obj)` | 引擎在技能键按下时回调，返回 false 则指令不被受理（也控制技能图标亮/灭；配合 setSkillCommandEnable / flushCommandEnable / sq_SetAllCommandEnable） | 技能 .nut | `知识库\11-常用函数.md`；实例 `sqr\character\swordman\5_ghostsword\speedslash\speedslash.nut` |
| 6 | 脚本门禁②：`checkExecutableSkill_技能名(obj)` | 尝试执行：脚本调用 `obj.sq_IsUseSkill(SKILL_ID)`——**引擎在此做 CD/MP/密封/消耗品校验，通过则扣 MP、进 CD，返回 true** | 引擎施放路径（由脚本调用触发） | speedslash.nut：`isUse = obj.sq_IsUseSkill(SKILL_SPEEDSLASH); if(isUse){ obj.sq_AddSetStatePacket(STATE_SPEEDSLASH, …) }` |
| 7 | 进入技能状态 | 脚本 `obj.sq_AddSetStatePacket(STATE_X, STATE_PRIORITY_USER, true)` →（经网络包同步给队友客户端）→ `onSetState_技能名` 播动画、出判定 | 脚本 + 引擎状态机 | 03-状态系统.md；speedslash.nut |

**关键结论**："物理键→技能ID"的映射是引擎做好的，脚本只按技能 ID 查询/消费。映射拆成两张表：① 物理键→逻辑热键（默认在 hotkeysystem.co，用户自定义在服务器 DB）；② 逻辑热键槽位→技能 ID（角色快捷栏数据，服务器端，**不在 pvf**）。此外每个技能还有 .skl `[command]` 指令串作为第二触发路径。

## 二、OPTION_HOTKEY_* 常量全表

定义两份（内容一致）：`pvf\sqr\dnf_enum_header.nut`（1342-1376 行起）、`language.dof.header.md:2346-2520`（含韩文注释和默认键注释）。

```squirrel
OPTION_HOTKEY__UNDEFINED      <- -1
// 移动/基本操作
OPTION_HOTKEY_MOVE_UP         <- 0   // (Up arrow)
OPTION_HOTKEY_MOVE_LEFT       <- 1   // (Left arrow)
OPTION_HOTKEY_MOVE_DOWN       <- 2   // (Down arrow)
OPTION_HOTKEY_MOVE_RIGHT      <- 3   // (Right arrow)
OPTION_HOTKEY_ATTACK          <- 4   // (X) 普通攻击
OPTION_HOTKEY_JUMP            <- 5   // (C) 跳跃
OPTION_HOTKEY_SKILL           <- 6   // (Z) 技能键
OPTION_HOTKEY_SKILL2          <- 7   // (Space) buff用技能2
OPTION_HOTKEY_CREATURE_SKILL  <- 8   // (V) 宠物技能
// 窗口/界面
OPTION_HOTKEY_STATUS_WINDOW        <- 9   // (M)
OPTION_HOTKEY_SKILL_WINDOW         <- 10  // (K)
OPTION_HOTKEY_ITEM_INVENTORY      <- 11  // (I)
OPTION_HOTKEY_OPTION_WINDOW       <- 12  // (O)
OPTION_HOTKEY_NORMAL_QUEST_WINDOW <- 13  // (Q)
OPTION_HOTKEY_AVATAR_INVENTORY    <- 14  // (U)
OPTION_HOTKEY_CERASHOP            <- 15  // (T)
OPTION_HOTKEY_MINIMAP             <- 16  // (N)
OPTION_HOTKEY_CREATURE_WINDOW     <- 17  // (Y)
OPTION_HOTKEY_TOOLTIP_            <- 18  // (R)
OPTION_HOTKEY_EPIC_QUEST_WINDOW   <- 19  // (W)
// 技能快捷栏
OPTION_HOTKEY_QUICK_SKILL1..6     <- 20..25  // A S D F G H
OPTION_HOTKEY_EXSKILL1..6         <- 26..31  // F1-F6
// 物品快捷栏
OPTION_HOTKEY_ITEM_QUICKSLOT1..6  <- 32..37  // 1-6
// 功能键
OPTION_HOTKEY_TOGGLE_ITEM_NAME_IN_DUNGEON    <- 38  // ctrl
OPTION_HOTKEY_HIDE_MAIN_HUD                  <- 39  // Tab
OPTION_HOTKEY_TOGGLE_TITLE_ANIMATION         <- 40  // E
OPTION_HOTKEY_TOGGLE_SKILL_INFORMATION       <- 41  // F7
OPTION_HOTKEY_PAUSE_IN_TOWER                 <- 42
OPTION_HOTKEY_CAPTURE_MOVING_PICTURE         <- 43  // Pause
OPTION_HOTKEY_MENU_MY_INFO/COMMUNITY/CONTENTS/SERVICE <- 44/45/46/47  // 7 8 9 0
OPTION_HOTKEY_MENU_SYSTEM__CLOSE_ALL_WINDOW  <- 48  // Esc
OPTION_HOTKEY_PVP                    <- 49  // P
OPTION_HOTKEY_RECOMMEND_USER         <- 50  // [
OPTION_HOTKEY_PARTY_MATCHING         <- 51  // ]
OPTION_HOTKEY_FRIEND                 <- 52  // L
OPTION_HOTKEY_GUILD                  <- 53  // ;
OPTION_HOTKEY_MEMBER                 <- 54  // '
OPTION_HOTKEY_BLACKLIST              <- 55
OPTION_HOTKEY_PVP_BUDDY              <- 56
OPTION_HOTKEY_WAR_AREA_LIST          <- 57  // ,
OPTION_HOTKEY_AUCTION_WINDOW         <- 58  // B
OPTION_HOTKEY_GOBLIN_PAD             <- 59
OPTION_HOTKEY_HOTKEY_SETTING_WINDOW  <- 60
OPTION_HOTKEY_WAR_AREA_INFORMATION   <- 61  // End
OPTION_HOTKEY_HELLMODE_INFORMATION   <- 62  // 废弃
OPTION_HOTKEY_FAVOR_CHECK_WINDOW     <- 63  // 废弃
OPTION_HOTKEY_EXPERT_JOB             <- 64
OPTION_HOTKEY_EMOTION_EXPRESSION     <- 65
OPTION_HOTKEY_EVENT                  <- 66  // shift
OPTION_HOTKEY_PVP_MISSION            <- 67
OPTION_HOTKEY_PVP_RECORD             <- 68
OPTION_HOTKEY_QUICK_CHAT_0..9        <- 69..78
OPTION_HOTKEY_TOGGLE_ITEMINFO_COMPARE<- 79  // F8
OPTION_HOTKEY_TITLEBOOK              <- 80
OPTION_HOTKEY_THIS_DUNGEON           <- 81
OPTION_HOTKEY_ANOTHER_DUNGEON        <- 82
OPTION_HOTKEY_RETURN_TO_TOWN         <- 83
OPTION_HOTKEY_MERCENARY_SYSTEM       <- 84
OPTION_HOTKEY_ITEM_DICTIONARY        <- 85
OPTION_HOTKEY_QUICK_PARTY_REGISTER   <- 86
// 子键类型（sq_IsKeyDown 第二参数）
ENUM_SUBKEY_TYPE_ALL                 <- 7
```

配套的另一套枚举 **E_COMMAND**（普攻/跳跃等"基础动作指令"，`sq_IsEnterCommand` 用，定义在 `avenger_header.nut:152-159` 与 `12-常量定义.md:204-215`）：

```squirrel
E_ATTACK_COMMAND  <- 0   // 攻击键(X)
E_JUMP_COMMAND    <- 1   // 跳跃键(C)
E_DASH_COMMANDS_1 <- 2   // 前冲(右)
E_DASH_COMMANDS_2 <- 3   // 前冲(左)
E_CREATURE_COMMAND<- 4   // 宠物键
E_BUFF_COMMAND    <- 5   // buff键
E_SKILL_COMMAND   <- 6   // 技能键Z
E_COMMAND_COUNT   <- 7   // "没有这个，用这个会掉线"
```

`pvf\skill\rdarkeyindex.dat`（明文）——.skl 指令记号的索引表（注意与 OPTION_HOTKEY 编号不同，独立一套）：
`0=UP 1=DOWN 2=LEFT 3=RIGHT 4=SKILL(Z) 5=ATTACK(X) 6=JUMP(C) 7=CREATURE(V) 8=BUFF(Space)`

## 三、sq_IsEnterSkill 与 sq_IsUseSkill 语义对照

| | `obj.sq_IsEnterSkill(skillIndex)` | `obj.sq_IsUseSkill(skillIndex)` |
|---|---|---|
| 语义 | 查询"该技能的键/指令被按下"（施放前后都能查，典型用于施放中再按键的连段/取消检测） | **尝试使用技能**：走引擎完整施放校验 |
| 返回 | -1=没按；0=只按了一次；1=施放后又按了一次；脚本惯例 `!= -1` | true=成功（MP已扣/CD已启动）；false=CD中/MP不足/被密封等 |
| 副作用 | **无**（纯查询，不进CD不扣MP） | **有**：扣 MP、启动 CD。原话"使用后会使技能进入CD，没有这个函数技能无CD" |
| 长按/短按 | 本身不区分；**蓄力(长按)**要配 `obj.sq_IsEnterSkillLastKeyUnits(skillIndex)`（在 sq_IsUseSkill 成功后立刻调用），实例 thief/rogue/adesphantom.nut:11 | 不处理长按 |
| 典型用法 | 在 onProc_/appendage 里检测"玩家又按了这个技能键"来打断/追加上一阶段，或强制取消类技能 | 在 checkExecutableSkill_xxx 里做最终放行 |

标准组合拳（强制中断/柔化，`atfighter/appendage/ap_atfighter_comminterrupt.nut:33-53`，乱码注释为韩文）：
```squirrel
function SetSkillState(obj, skillindex, state, Arr) {
    local iEnterSkill = obj.sq_IsEnterSkill(skillindex); // 查询按键（无副作用）
    if (iEnterSkill == -1) return false;
    if (obj.sq_GetState() == state) return false;
    if (obj.sq_IsUseSkill(skillindex)) {                  // 判定+进CD+扣消耗
        obj.sq_AddSetStatePacket(state, STATE_PRIORITY_USER, true);
        return true;
    }
}
```

相关 API（声明见 language.dof.globalFunction.md / .character.md / .CNRDSkill.md）：
- `sq_IsKeyDown(index, subKeyType)` / `sq_IsKeyUp(...)` — 读**逻辑热键**当前按下/松开
- `sq_IsEnterCommand(obj, keynum)` / `sq_SetKeyxEnable(obj, keynum, bool)` — 读/使能 **E_COMMAND 基础指令**（连段检测用）
- `obj.setSkillCommandEnable(skillIndex, bool)` / `obj.sq_IsCommandEnable(skillId)` / `obj.flushCommandEnable()` / `sq_SetAllCommandEnable(obj,bool)` — 技能级开关（图标亮灭、是否受理按键）
- `skill.isInCoolTime()` / `skill.getCoolTime(obj,-1)` / `obj.startSkillCoolTime(id,lv,-1)` / `skill.resetCurrentCoolTime()` — CD 查询/手动启动/重置
- `skill.getSpendMp(obj, skillLevel)` — 引擎算好的 MP 消耗
- `obj.sq_IsEnterSkillLastKeyUnits(id)` — 蓄力

## 四、技能栏/键位配置文件盘点（pvf 内实测）

| 文件 | 内容 | 格式 |
|---|---|---|
| `pvf\clientonly\hotkeysystem.co` | **默认键位表**（核心发现）。每条 [key] 块 4 行：①显示名 ②OPTION_HOTKEY 逻辑索引 ③类别+左右手 ④默认 VK 码。例：`技能快捷键 1 / 20 / dungeon / left / 65('A')`；`普通攻击 4 dungeon left 88('X')`；`技能 6 dungeon left 90('Z')`；`技能快捷键 7 66 dungeon left 18(Alt)`；`扩展技能快捷键 1-6 26-31 right 81/87/69/82/84/89(F/P/E/R/T/Y)`（此表为国服私服默认，与 header.md 里韩服注释默认值略有出入） | 明文 |
| `pvf\clientonly\hotkeysystemforcreator.co` | 创造者(Creator)职业的同款默认键位表 | 明文 |
| `pvf\clientonly\joypadmapping.co` | 手柄映射：按产品名把手柄按键映射到键盘键 | 明文 |
| `pvf\skill\rdarkeyindex.dat` | .skl [command] 指令记号索引表（UP/DOWN/LEFT/RIGHT/SKILL/ATTACK/JUMP/CREATURE/BUFF = 0-8） | 明文 |
| `pvf\skill\*.lst`（如 swordmanskill.lst） | **技能 ID ↔ .skl 文件映射表**。成对出现：`46\n`Swordman/UpperSlash.skl``（46=上挑，8=三段斩，1=格挡）。skilllist.lst 是 11 个职业 lst 的索引 | 明文 |
| 各 .skl 的 [command] 段 | 每技能施放指令串，记号引用 rdarkeyindex.dat，连接符 `{8=&}`(按住) / `{8=`,`}`(顺序)。配套 [command key explain](说明文本)、[command customizing](0/1 是否允许玩家开关指令) | 明文 |
| `pvf\clientonly\cancelskilllist.co` | 按职业列出"可强制取消(普攻中取消)"的技能ID列表 | 明文 |
| `pvf\clientonly\commonskilllist.co` | 全职业通用技能ID | 明文 |
| `pvf\clientonly\skilltree\*_sp.co / *_tp.co` | 技能树UI的SP/TP页数据 | 明文 |
| `pvf\etc\newbieskillselect.etc` | 新手自动学技能表 | 明文 |

**明确不在 pvf 内的（如实标注）**：
- 玩家自定义键位（物理键↔逻辑热键的覆盖）：官服=服务器账号数据（游戏内 ESC→快捷键设置）；私服=服务端 game/cfg + MySQL 表。pvf 只有**默认值**。
- 快捷栏槽位↔技能ID 的赋值（哪个技能放A键）：**未找到任何 pvf 文件**，属角色数据（服务器 DB，客户端 UI 拖拽设置后上传）。引擎侧通过 CNRDSkillManager.getSlotIndex/getQuickSlotNumber 等读取。
- `pvf\n_string.lst`：只是字符串表索引，与键位无关。`pvf\data\`、`pvf\ui\` 下无键位文件。
- 引擎 C++ 源码：本仓库没有，输入匹配细节为推断。

## 五、MP 扣除和 CD 检查是谁做的

**结论：都是引擎做的（在 sq_IsUseSkill 的施放路径里），脚本只有数据提供和微调钩子。**

- MP 消耗数值：数据驱动，来自 .skl 的 `[dungeon][consume MP] 6 40`（基础值+成长，upperslash.skl）；引擎用 `skill.getSpendMp(obj, level)` 计算最终值。
- 脚本微调钩子：`useSkill_before_职业名(obj, skillIndex, consumeMp, consumeItem, oldSkillMpRate)`——**引擎把算好的 consumeMp 传进来**，脚本可 `obj.setSkillMpRate(skillIndex, newRate)` 改倍率，useSkill_after_* 恢复。确认"引擎算消耗"。
- CD：.skl 里 `[cool time] 2000 2000` + `[auto cooltime apply] 1`（施放后自动进CD）。sq_IsUseSkill 内部检查 isInCoolTime，通过才放行并启动 CD。脚本侧 `skill.isInCoolTime()` 只是**查询**，`obj.startSkillCoolTime(id, lv, -1)` 是**手动补启动**（特殊技能用），`resetCurrentCoolTime()` 重置。
- 推论：脚本不调 sq_IsUseSkill 就没 CD——校验与扣费完全依赖引擎在该函数里的实现。

## 六、普通攻击（X 键）触发链

普攻**不是技能**，没有技能ID/CD/MP，走引擎原生基础状态机：
1. X 键 → OPTION_HOTKEY_ATTACK(4) → 引擎识别为 E_ATTACK_COMMAND(0) 基础指令；
2. 引擎直接把角色置入 `STATE_ATTACK <- 8`（引擎原生状态；基础状态全套：STAND=0、DAMAGE=3、DIE=5、JUMP=6、JUMP_ATTACK=7、ATTACK=8、THROW=13、DASH=14、DASH_ATTACK=15、BUFF=17…；状态ID与技能ID是两个独立编号空间，如技能8=三段斩≠状态8=普攻）；
3. 每职业用 pushState 给状态 8 挂脚本处理器，第5参数 skillId=-1 证明它不属于任何技能：`swordman_load_state.nut:120` → `pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/attack/attack.nut", "swordman_attack", 8, -1)`；
4. attack.nut 只做**职业化定制**：连段子状态推进、换动画。连招下一段的按键检测用命令层API（attack.nut:16-22）：
```squirrel
sq_SetKeyxEnable(obj, E_ATTACK_COMMAND, true);        // 先使能该指令监听
if (sq_IsEnterCommand(obj, E_ATTACK_COMMAND)) {       // 玩家又按了X
    obj.sq_IntVectPush(3);                             // 推进到子状态3
    obj.sq_AddSetStatePacket(8, STATE_PRIORITY_IGNORE_FORCE, true); // 重进状态8
}
```
5. 普攻中**取消进技能**：cancelskilllist.co 列出各职业可从普攻取消施放的技能ID；setEnableCancelSkill_职业名 回调里 setSkillCommandEnable。
6. 旁证——AI 陪跑角色也走同一输入层：`pvf\aicharacter\arad_aic\2013sao\3ghost_bremen\key\*.key` 用 `[input] `x` 30 / `time` 30` 这种"虚拟按键+按住时长"脚本驱动，说明引擎的战斗输入统一抽象为按键事件流。

## 七、给我们 2D 格斗技能系统的可复刻分层（浓缩）

1. **InputConfig 层**：物理键码 → 逻辑动作名（等价 OPTION_HOTKEY），默认表打进包、用户自定义存存档/服务器；预生成"技能槽1~N"逻辑键。
2. **CommandMatcher 层**：等价 CNRDCommandChecker——环形缓冲记录最近输入序列，两类匹配：槽位键直接命中 → 技能ID；指令串（→→+X 语法树来自技能配置数据，等价 .skl [command]+rdarkeyindex）序列匹配 → 技能ID。
3. **CastGate 层**（引擎，进 TryUseSkill(skillId)）：CD 检查→MP/资源检查→密封/禁用检查→通过则扣MP、启CD、置"已按下"标记。
4. **Script 层**：每技能 OnCommandEnable（可否受理/图标）与 OnTryExecute（调 TryUseSkill 后发状态包）双回调；状态机 OnSetState/OnProc/OnEndAni 跑演出与判定。
5. **普攻独立通道**：X 键 → 基础状态(无技能ID/CD/MP)，连段推进用 IsEnterCommand(ATTACK) 在状态脚本内自轮转；"普攻取消"用可取消技能白名单。

Sources:
- [游戏设置-快捷键设置 - DNF官网](http://dnf.qq.com/book2011/gdc/10024/10041/31.shtml)
- [系统设置-新手引导 - DNF官网](https://dnf.qq.com/act/a20121026gbook/sys.html)
- [DNF单机键位设置无法保存修复教程（game/cfg + 数据库）](https://dnf.arad.ink/thread-3138-1-1.html)
