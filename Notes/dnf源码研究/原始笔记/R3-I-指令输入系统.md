# R3-I：DNF 指令输入系统（↓→+Z 出招）提取笔记

> 第3轮 Agent I 原始笔记。任务：.skl [command] 语法全集 + CNRDCommandChecker 匹配机制 + 冲突/超时规则 + 方向状态输入。
> 资料根：E:\Projects\cs\dnforigin\（pvf\ = pvf源码提取部分\pvf\，知识库\ = 部分前人的文档\nut知识库\）
> 已综合进：07-连招与取消系统-总结.md

---

## 一、.skl `[command]` 语法全集

### 1.1 记号系统：两层编号 + 二进制 opcode

**花括号里的数字不是键编号，是 PVF 二进制的记录类型号（opcode）**。168 论坛漫游 skl 二进制翻译帖给出的原始格式：

```
[@05][command]        ; 段头=opcode05
[@06](RIGHT) [@08], [@06](RIGHT) [@08], [@06](SKILL)   ; 爆头一击 →→+Z
[@05][skill command advantage]
[@02]10   ; 冷却-10‰
[@02]20   ; MP消耗-20‰
```

`[@06]`=键记号、`[@08]`=连接符——与明文 pvf\skill\*.skl 里的 `{6=`(RIGHT)`}`、`{8=`,`}` 一一对应。即：

| 明文记号 | 二进制 | 含义 |
|---|---|---|
| `{6=`(键名)`}` | `[@06]` | **一个按键记号**。键名查 pvf\skill\rdarkeyindex.dat |
| `{8=`,`}` | `[@08]` `,` | **顺序连接符**：前一键松开后再按下一键（先后） |
| `{8=&}` | `[@08]` `&` | **同按连接符**：前一键**保持按住**的同时按下一键（同时/按住） |

`rdarkeyindex.dat`（"索引号→键记号"映射表，自己的条目就写成 `{6=`(UP)`}`）：

| 索引 | 记号 | 键 | 引擎内常量（12-常量定义.md E_COMMAND） |
|---|---|---|---|
| 0 | `(UP)` | ↑ | —（E_COMMAND 里无 UP/DOWN，方向另有编号） |
| 1 | `(DOWN)` | ↓ | — |
| 2 | `(LEFT)` | ← | E_DASH_COMMANDS_2=3 |
| 3 | `(RIGHT)` | → | E_DASH_COMMANDS_1=2 |
| 4 | `(SKILL)` | Z 技能键 | E_SKILL_COMMAND=6 |
| 5 | `(ATTACK)` | X 攻击键 | E_ATTACK_COMMAND=0 |
| 6 | `(JUMP)` | C 跳跃键 | E_JUMP_COMMAND=1 |
| 7 | `(CREATURE)` | V 宠物键 | E_CREATURE_COMMAND=4 |
| 8 | `(BUFF)` | Space buff键 | E_BUFF_COMMAND=5 |

注意存在**三套编号并存**（易混）：rdarkeyindex（.skl 数据用）、E_COMMAND（检查器内部命令类型）、OPTION_HOTKEY（键位设置索引：UP=0 LEFT=1 DOWN=2 RIGHT=3 ATTACK=4 JUMP=5 SKILL=6 SKILL2/Space=7 CREATURE=8，另含 QUICK_SKILL1~6=A S D F G H 技能栏快捷键）。

### 1.2 语法全集（对全 pvf\skill 树 273 个含 [command] 的 .skl 做穷举统计���

全部出现过的记号只有 10 种键 × 2 种连接符，无其他语法变体：

| 记号 | 全库出现次数 | 说明 |
|---|---|---|
| `{8=`,`}` | 3094 | 顺序连接，占绝对主流 |
| `(RIGHT)` | 1337 | 方向键（最多，因→→系指令多） |
| `(SKILL)` | 909 | Z |
| `(DOWN)` | 749 | ↓ |
| `(UP)` | 654 | ↑ |
| `(LEFT)` | 389 | ← |
| `(BUFF)` | 264 | Space（buff/被动开关类技能收尾键） |
| `(JUMP)` | 46 | C |
| `(ATTACK)` | 46 | X |
| `{8=&}` | **35** | 同按连接，仅 35 处，**只出现在"方向键 & 动作键"两键组合里**（枚举全部 35 例：backstep ↓&C×11 职业、suplex →&Z、damagelowkick ↓&Z、singlekick ↑&Z、liftshot →&Z、robotrx78 ↓&Z、hardattack ↑&Z、tripleslash →&Z、thrust →&Z、ducking 三兄弟 →/↓/↑&X 等），explain 一律写"(按住状态下)" |
| `(CREATURE)` | **0** | V 键在 rdarkeyindex 有定义但**没有任何技能指令使用** |

结构性规律（硬统计）：
- **指令长度 1~5 个键**（1键21例 / 2键16 / 3键65 / 4键78 / 5键43，另有 8 个空段）。
- **收尾键永远是动作键，从不以方向键结尾**：SKILL 445 次、BUFF 141、JUMP 27、ATTACK 25。
- `&` 从不用于方向键与方向键之间；最长 5 键指令（↑↑↓↓+Z 类）全用 `,` 连接。

`[command]` 相关的段一共有 5 种（全库段头统计）：

| 段 | 出现次数 | 含义 |
|---|---|---|
| `[command]` / `[/command]` | 273 | 指令本体（"初始手搓键位"） |
| `[command key explain]` | 247 | 纯显示文本（`操作指令 : →→ + Z`），含"（跳跃状态下）"等状态前缀说明 |
| `[skill command advantage]` | 147 | 手搓增益两值：**冷却减少‰、MP消耗减少‰**（爆头 10/20 = CD-1%、MP-2%；血气爆发 50/50 = 各-5%） |
| `[command customizing]` | 135 个文件 | 值全部为 0（见三.3，语义缺口） |
| `[executable states]` | 800 个文件 | 该技能可从哪些**状态**发起（状态号表），与指令正交——是"能不能放"的状态门，不是输入序列 |

### 1.3 状态条件（跳跃中/蹲下中/前冲中）**不写在 [command] 里**

`[command]` 只编码裸键序列；"（跳跃状态下）Z""（前冲攻击中)X""（被攻击中)Z""（倒地状态下)C""(回旋踢动作中)方向键"这些前缀只出现在 `[command key explain]` 显示文本中。实际门控走三条路：
1. **引擎侧默认行为**：银光落刃 ashenfork.skl 的 [command] 只有 `{6=`(SKILL)`}`，无 [executable states]，全 pvf\sqr 也无任何 ashfork 脚本——"空中才可 Z"是引擎对该技能的默认可施放状态处理。
2. **[executable states] 数据门**：burster.skl `8 0 14`（攻击/站立/前冲状态可发动——这就是"取消"机制的数据面）。
3. **脚本轮询门**：pvf\sqr\character\swordman\jump\swordman_jump.nut——在 onProcCon_SwordmanJump 里检查 obj.getState()==6（跳跃态）+ 武器类型 + !sq_IsKeyDown(OPTION_HOTKEY_ATTACK)（X 未按住），然后 obj.setSkillCommandEnable(105,true) 放行指令、sq_IsEnterSkill(105)!=-1 查询指令是否完成，手动 startSkillCoolTime、sq_IsUseSkill 消费、sq_AddSetStatePacket 切状态。

### 1.4 实例解读（18 例，覆盖全部语法形态）

| # | 技能（文件） | [command] 记号序列 | 解读 |
|---|---|---|---|
| 1 | 上挑 swordman\upperslash.skl | `(SKILL)` | 单键 Z |
| 2 | 三段斩 swordman\tripleslash.skl | `(RIGHT)` `&` `(SKILL)` | **→按住不松+按Z**（`&` 只夹在两键中间；顺序是先按住→再按Z） |
| 3 | 后跳 swordman\backstep.skl | `(DOWN)` `&` `(JUMP)` | ↓按住+C，全职业通用的 11 份拷贝同款 |
| 4 | 里鬼剑术 swordman\hardattack.skl | `(UP)` `&` `(SKILL)` | ↑按住+Z |
| 5 | 十字斩 swordman\gorecross.skl | `(LEFT)`,`(RIGHT)`,`(SKILL)` | ←→+Z：先←后→（各自按下即可，无需按住）再Z |
| 6 | 崩山击 swordman\hopsmash.skl | `(RIGHT)`,`(DOWN)`,`(SKILL)` | →↓+Z |
| 7 | 爆炎波动剑 swordman\firewave.skl | `(LEFT)`,`(DOWN)`,`(RIGHT)`,`(SKILL)` | ←↓→+Z 三方向序 |
| 8 | 嗜魂之手 swordman\grabblastblood.skl | `(RIGHT)`,`(RIGHT)`,`(SKILL)` | **→→+Z：同一方向键两次**（双击的表达法=重复记号） |
| 9 | 破军升龙击 swordman\chargecrash.skl | `(LEFT)`,`(RIGHT)`,`(RIGHT)`,`(SKILL)` | ←→→+Z（与十字斩 ←→+Z 前缀重叠，见 2.3） |
| 10 | 邪神怖拉修 swordman\blache.skl | `(UP)`,`(UP)`,`(DOWN)`,`(DOWN)`,`(SKILL)` | ↑↑↓↓+Z，最长 5 键觉醒技 |
| 11 | 冰霜之萨亚 swordman\saya.skl | `(RIGHT)`,`(RIGHT)`,`(BUFF)` | →→+**Space**：以 BUFF 键收尾（阵/被动开关类常用） |
| 12 | 银光落刃 swordman\ashenfork.skl | `(SKILL)` | explain"(跳跃状态下)Z"——状态门在引擎/脚本，指令本体就是 Z |
| 13 | 后跳斩 swordman\backstepcutter.skl | `(ATTACK)` | "(后跳动作中)X"——单 X，状态门另管 |
| 14 | 三连突刺 swordman\dashattackmultihit.skl | `(ATTACK)` | "(前冲攻击中)X" |
| 15 | 复仇反击 gunner\countershoot.skl | `(SKILL)` | "(被攻击中)Z"，其 [static data] 有"被攻击后发动时间1000ms"——被击窗口是数据参数 |
| 16 | 浮空截击 gunner\airraid.skl | `(SKILL)` | explain 是"(回旋踢动作中)**方向键**"——收尾键甚至不是本指令的键，实际检测全在引擎，`(SKILL)` 只是占位注册 |
| 17 | 快速起身 fighter\quickstanding.skl | `(JUMP)` | "(倒地状态下)C" |
| 18 | 俯冲直拳 priest\duckingstraight.skl | `(RIGHT)` `&` `(ATTACK)` | "(俯冲动作中)→按住+X"——状态前缀+& 组合并用；同系俯冲腹击=↓&X、俯冲翔拳=↑&X |

补充：**空 [command] 段**也存在（burster、weaponcombo、flowmind 流心等 8 例）= 该技能无独立搓招指令，靠其他技能/状态进入。

## 二、匹配机制

### 2.1 CNRDCommandChecker 全部公开 API

知识库\资源nut函数声明\language.dof.CNRDCommandChecker.md 全文仅两个方法：

```squirrel
class CNRDCommandChecker {
    function setAIMode(bool) {}        // 设置AI模式（AI接管/玩家输入切换）
    function commandListReset() {}     // 重置按键列表（清空输入缓冲）
}
```

关联 API（language.dof.character.md / globalFunction.md / CNRDSkillManager.md）：

| API | 所属 | 作用 |
|---|---|---|
| getCommandChecker() | 角色对象 | 取检查器 |
| setCommandChecker(cmdChecker) | 角色 | 挂载检查器 |
| initCommandChecker(cmdChecker, bool) | 角色 | bool=true 初始化 / false 重置 |
| setSkillCommandEnable(skillIndex, bool) | 角色 | 开关单个技能的指令（UI 可用性+指令放行） |
| sq_IsEnterSkill(skillIndex) | 角色 | **轮询某技能的指令是否已完成**，返回 -1=无 |
| sq_IsEnterSkillLastKeyUnits(skillIndex) | 角色 | 末键按住蓄力相关 |
| sq_IsUseSkill(skillIndex) | 角色 | 消费技能使用，触发进 CD（"没有这个函数技能无CD"） |
| sq_IsCommandEnable(skillId) | 角色 | 查询某技能指令当前是否启用 |
| sq_IsKeyDown(OPTION_HOTKEY_x, ENUM_SUBKEY_TYPE_ALL) | 全局 | **绕过指令系统直接读原始按键状态** |
| sq_SetAllCommandEnable(obj, bool) | 全局 | 一键开关全角色技能指令（变身/演出时用） |
| sq_SetEnableKeyInputType(cmdChecker, 0, bool, 1) | 全局 | 键盘输入总开关（后两参数语义未文档化） |
| setCommandChecker / addAllKeyCommand(skillTree) / setParent(chr) | CNRDSkillManager | 把技能树全部指令注册进检查器（见三） |
| skill.setCommandEnable(bool) / skill.isInCoolTime() | CNRDSkill | 单技能指令开关/CD 查询 |

脚本侧事件钩子：flushCommandEnable_职业名(obj)（刷新技能亮灭）、procDash_职业名(obj)（前冲状态每帧）、每技能 checkCommandEnable_技能名(obj)（引擎回调问"此刻该技能指令可否触发"，661 处实现）、checkExecutableSkill_技能名(obj)。

### 2.2 序列完成的判定与超时 —— **缺口**

- **顺序严格**：指令是按键事件流的**顺序匹配**（`,` 语义即先后），方向键乱序不成立。`&` 要求按后一键瞬间前一键仍处于按下状态（"按住状态下"）。
- **超时**：引擎 C++（CNRDCommandChecker 实现）无公开源码——GitHub 搜索 CNRDCommandChecker/rdarkeyindex 零命中。输入缓冲的清空时机可确认两处：commandListReset() 显式清空、initCommandChecker(checker,false) 重置；**序列匹配的时间窗数值（社区体感约零点几秒~1 秒级，超过后前缀作废）无权威出处——缺口**。
- 脚本能做的"主动操作"：拿到 checker 后只有切 AI 模式和清缓冲两条；**逐键查询/注入没有公开 API**。脚本的常规姿势是**轮询 sq_IsEnterSkill(id)** 等引擎告诉你"这个技能的指令刚完成了"。

### 2.3 前缀冲突裁决 —— 数据面已确认，运行时仲裁为缺口

数据面事实（skill fitness growtype 字段）：
- **设计期去重**：完全相同的指令 (→→+Z) 允许存在于 嗜魂之手/邪光斩/冥炎剑 三个技能，因为分属狂战(growtype3)/阿修罗(4)/鬼泣(2)，**同一角色不可能同时持有**——冲突靠互斥解决，不靠运行时。
- **真前缀重叠仍存在**：剑魂同时持有 十字斩←→+Z 与 破军升龙击←→→+Z。玩家实机输入 ←→→Z 出破军、←→Z 出十字斩 ⇒ 推断匹配器对每个候选序列独立记录进度，**收到不匹配预期的键即重置该序列**（←→ 后再来的 → 使十字斩进度失效、破军进度+1），完成即触发。三段斩(→&Z) 与 邪光斩(→→+Z) 也不冲突：`&` 语义（Z 按下时 → 必须仍按住）天然区分"按住单→"与"点两下→"。**"最长匹配等待/立即触发"的精确仲裁顺序无源码——缺口**。

## 三、指令 ↔ 技能绑定

注册链路（唯一一处完整调用样本：pvf\sqr\character\creatormage\mousecontrol_lib.nut L14-56，缔造者用脚本接管其他角色的按键）：

```
chr.getCommandChecker()                    // 取检查器
checker.setAIMode(bool) / commandListReset()
sq_SetEnableKeyInputType(checker,0,true,1) // 开键盘输入
chr.initCommandChecker(checker, aiMode)    // 初始化/重置
chr.setCommandChecker(checker)
skillMgr = chr.getSkillManager()
skillMgr.setParent(chr)
skillMgr.setCommandChecker(checker)        // 技能管理器 ↔ 检查器 连接
skillTree = chr.getCurrentSkillTree()
skillMgr.addAllKeyCommand(skillTree)       // ★ 遍历技能树，把每个已学技能 .skl 的 [command] 注册进检查器
chr.flushCommandEnable()                   // 刷新（触发 flushCommandEnable_职业名 钩子）
```

正常玩家路径中这套由引擎 C++ 自动完成（全 sqr 树里 addAllKeyCommand 只出现在缔造者脚本），脚本的介入点是**动态开关**：setSkillCommandEnable(id,bool) / skill.setCommandEnable(bool) / sq_SetAllCommandEnable(obj,bool)（变身锁指令、buff 持有期间才放行某技能等）。

**[command customizing]**：全库 135 个文件带此段、**值全部为 0**，且集中在"基础连招型"技能（后跳/快速起身/里鬼/三段斩/银光落刃/蓝拳俯冲系/刺客旋刃系/缔造者全技能…）。游戏内对应"技能指令设置（使用指令开关/变更指令）"功能（官网/百度经验可证该 UI 存在），但 **0/1 的精确语义（0=禁止玩家自定义？还是 0=默认未自定义？）无任何权威文档，且本库无 1 值样本可比对——缺口**。

**[skill command advantage] 两值 = 指令施放(手搓)时 冷却减少‰、MP消耗减少‰**（168 论坛二进制注释：`10 //冷却-10‰=1.0％`、`20 //MP消耗-20‰=2.0％`；社区"手搓入门指北"证实手搓减 CD 机制与等级相关分层）。快捷键（技能栏 A~H）施放吃不到此增益——这是"为什么要做指令输入"的数值侧动机。

## 四、方向类状态输入（蹲下/前冲）

结论：**方向键事件进的是同一个按键流（所以 →→ 才能被技能指令匹配到），但"蹲下/前冲"这两个状态本身是引擎独立检测的，不经过技能指令匹配器**。

证据链：
- 状态号是引擎枚举：STATE_CROUCH=19（蹲下，按住↓）、STATE_DASH=14（前冲）、STATE_DASH_ATTACK=15、STATE_QUICK_STANDING=18、STATE_FAST_DASH=59 等（language.dof.header.md L204-322）。没有任何脚本创建 STATE_DASH/STATE_CROUCH——都是引擎状态机按方向键流（含双击判定）自己进入。
- E_COMMAND 枚举把左右方向键列为 E_DASH_COMMANDS_1/2（右=2、左=3）——方向键与 X/C/Z 同属检查器可见的命令类型，佐证单一按键流。
- 脚本只能**事后响应**：procDash_职业名(obj)（知识库\01 L142"前冲被动"）在 mage_common.nut L12、atswordman_common.nut L590 等的实现只做**脚步声/特效**（if state!=14 return + 按动画 flag 播声），完全不参与触发。
- "前冲中才能放"的技能用状态查询表达：atmage 各技能 `if(state == STATE_ATTACK || STATE_STAND || STATE_DASH)`，fastdash checkExecutableSkill 里 state==0||14。
- 双击方向的窗口同样无源码——**缺口**。

## 五、给我们的启示：指令输入系统最小组成件（客观陈述 DNF 侧）

1. **逻辑键层**：把物理键位映射到固定逻辑键（方向×4 + 攻击/跳跃/技能/Buff/宠物），玩家可改键不动逻辑。三套编号要分清，DNF 自己就有三套。
2. **按键事件流（带按下/按住/抬起）**：单一输入流，同时供"技能指令匹配"和"引擎状态机（蹲/冲/跳）"消费；脚本还能旁路直读（sq_IsKeyDown）。
3. **指令描述 DSL**（.skl [command]）：键记号 + 两种连接符（顺序`,` / 同按`&`），规则：长度 1~5、必须以动作键收尾、方向只做前缀。数据驱动、策划可改。
4. **序列匹配器（CommandChecker）**：对每角色持有"已学技能指令集"，逐事件推进各序列进度，不匹配即重置，完成即触发；暴露 reset/AI 开关。
5. **注册/解绑通道**：技能树 → 匹配器批量注册（addAllKeyCommand），运行期可整批开关或逐技能开关（变身/演出/条件态用）。
6. **触发后的裁决层**：指令完成 ≠ 施放。还要过：技能是否已学、状态门（[executable states] / checkCommandEnable_ 钩子）、CD/MP、霸体打断规则；通过则 sq_IsUseSkill（进 CD）+ 切状态。
7. **状态条件外置**：跳跃中/前冲中/被击中等条件不进指令 DSL，由"数据状态表 + 引擎默认 + 脚本轮询"三层分担——DSL 保持极简是 DNF 的明确取舍。
8. **指令施放激励**（手搓增益：CD/MP 千分率减免）+ 技能栏快���键并行通道（两通道并存，快捷键不吃增益）。
9. **每帧刷新钩子**（flushCommandEnable）驱动 UI 技能图标亮灭。

**缺口清单**（引擎 C++ 侧无公开源码/文档）：序列匹配超时窗口数值；前缀重叠时"等待更长序列 vs 立即触发较短序列"的精确仲裁；[command customizing] 0/1 精确语义；sq_SetEnableKeyInputType 后两个参数；双击方向触发前冲的时间窗。

**主要文件出处**：
- 键表：pvf\skill\rdarkeyindex.dat
- 指令实例：pvf\skill\swordman\*.skl、pvf\skill\mage\*.skl 等各职业目录
- 脚本门控样本：pvf\sqr\character\swordman\jump\swordman_jump.nut、pvf\sqr\character\atfighter\fastdash\fastdash.nut、chainbrake.nut（checkCommandEnable 钩子）
- 注册链路：pvf\sqr\character\creatormage\mousecontrol_lib.nut
- API：知识库\资源nut函数声明\language.dof.{CNRDCommandChecker,CNRDSkillManager,character,globalFunction,header}.md、知识库\{01,02,12}.md

**外部来源**（opcode 与手搓增益佐证）：
- [168遊戲論壇：dnf单库一键端（#44 漫游技能pvf翻译，[@06]/[@08]/手搓增益‰注释）](http://www.168gamesf.com/thread-391753-2-1.html)
- [COLG：手搓入门指北（手搓减冷却机制）](https://bbs.colg.cn/thread-8556847-1-1.html)
- [百度经验：dnf技能释放指令设置（游戏内变更指令 UI）](https://jingyan.baidu.com/article/6fb756ecb0c57f651858fbdd.html)
- [Reddit r/DFO：skill commands（指令施放与装备联动）](https://www.reddit.com/r/DFO/comments/1cnl6gu/new_to_dfo_wondering_about_skill_commands/)
