# R3-J：DNF 普攻连段 + 取消系统提取笔记

> 第3轮 Agent J 原始笔记。任务：attack.nut 完整读 + cancelskilllist + 取消窗口 + 柔化 + 技能接技能。
> 前提说明：该 PVF 为改版服（含 qq506807329 水印 MOD 函数、私加技能如鬼剑 252"体术逆改"），但**引擎回调契约、cancelskilllist.co、柔化(171)机制均为原版架构**，已标注哪些是 MOD 产物。
> 已综合进：07-连招与取消系统-总结.md

---

## 一、普攻连段完整机制（男鬼剑 attack.nut 实读）

### 1.1 架构：引擎原生推进 + 脚本仅做"例外"

`sqr/character/swordman/attack/attack.nut`（399行）里**没有任何 substate 0/1/2 的推进代码**——普攻三连击完全由引擎 C++ 原生驱动（STATE_ATTACK=8），脚本只通过一组**引擎每帧/每状态回调**注入例外：

| 引擎回调（定义于各职业 *_common.nut / jg_swordman_common.nut） | 作用 |
|---|---|
| getAttackAni_*(obj, index) | 返回第 index 段普攻动画（默认 obj.sq_GetAttackAni(index) → attack1/2/3.ani） |
| getDefaultAttackInfo_*(obj, index) | 返回第 index 段攻击信息（→ attack1/2/3.atk） |
| getAttackCancelStartFrame_*(obj, index) | **第 index 段动画播放到���几帧后，按 X 才会推进到下一段**（知识库原文："得到设置攻击时在哪一帧按下键时可以进入下一个子状态"） |
| getAttackCancelStartFrameSize_*(obj) | 连段总数（默认读引擎原生 sq_GetAttackCancelStartFrameSize()） |
| onSetState_*attack / onAfterSetState / onEndCurrentAni / onProcCon / onKeyFrameFlag | 状态进入/结束/每帧条件/动画帧事件钩子 |

关键实证（jg_swordman_common.nut:148）：
```squirrel
function getAttackCancelStartFrameSize_Swordman(obj) {
	local maxAttackNumber = obj.sq_GetAttackCancelStartFrameSize(); // 引擎原生连段数
	if(isSwordSaber(obj)) return 2;   // 短剑精通(技能123>0)时引擎只推进 0→1→2
	else return obj.sq_GetAttackCancelStartFrameSize(); // 普通情况=引擎数据(三段)
}
```
即：**引擎按 cancelStartFrame 表推进 0→1→2，到 2 之后不管**，所以短剑流的"第4击"必须脚本自己接。

### 1.2 连段推进：子状态号、切换条件、超时重置

- 子状态即 datas[0]：obj.setSkillSubState(subState)。男鬼剑基础三段 = **0/1/2**；MOD 的短剑流扩展出 **3（第4击·剑气四连）**、**4（上挑·短剑变体）**。
- 推进条���（引擎原生，第 N 段）：`当前动画帧索引 ≥ getAttackCancelStartFrame(N)` 且检测到 `E_ATTACK_COMMAND(=0, X键)` 重按 → 引擎直接切 8 状态子状态 N+1，动画 Rewind。
- 脚本侧同款模板（attack.nut:11-24，短剑第3击→第4击）：
```squirrel
case 2:
  if(isSwordSaber(obj)) {
    if(frameIndex >= 5) {                          // 第3击动画第5帧起开放
      sq_SetKeyxEnable(obj, E_ATTACK_COMMAND, true); // 使能X键监听
      if(sq_IsEnterCommand(obj, E_ATTACK_COMMAND)) { // 检测X重按
        obj.sq_IntVectClear();
        obj.sq_IntVectPush(3);                      // 子状态=3
        obj.sq_AddSetStatePacket(8, STATE_PRIORITY_IGNORE_FORCE, true); // 重进STATE_ATTACK
      }
    }
  }
```
- **连段窗口 = [cancelStartFrame 帧时刻, 本段动画播完]**。超时不按 → onEndCurrentAni_*attack 里 sq_AddSetStatePacket(STATE_STAND, ...) 回站立，子状态自然归零，**下一击从第一击重来**。没有独立的"连段计时器"，窗口就是动画剩余时长本身。
- **第4击多段命中**：onKeyFrameFlag_swordman_attack 中 `substate==3 && flagIndex==10001 → obj.resetHitObjectList()`——动画帧标记触发"已打对象清表"，实现单段动画内多次判定。

### 1.3 每段动画/攻击盒/伤害怎么设

- **动画与攻击盒是一体的**：character\swordman\animation\attack1.ani 逐帧定义 [IMAGE][DELAY][DAMAGE BOX x1 y1 z1 x2 y2 z2]。**每一帧都有自己的伤害框**（DNF 特色：判定随帧移动）。
- **攻击信息按段分离**：attackinfo\attack1.atk / attack2.atk / attack3.atk，段间差异即"手感"：

| 段 | damage bonus | damage reaction | push aside | lift up | attack direction |
|---|---|---|---|---|---|
| attack1 | -15% | damage(普通受击) | 30 | 75 | hit down |
| attack2 | — | damage | 30 | 90 | hit horizon |
| attack3 | +20% | **down(击倒)** | 40 | **300** | **hit lift up(挑飞)** |

第三击击飞、前两击推走——收尾段的浮空值就是连招起手感的来源。

- **特殊武器替换**（attack.nut onSetState case 3/4 + weaponcombo 函数）：sq_SetCurrentAnimation(CUSTOM_ANI_ATTACK_BLADESPIRIT4)、sq_SetCurrentAttackInfo(CUSTOM_ATTACK_INFO_ATTACK_BLADESPIRIT)、sq_SetStaticSpeedInfo(ATTACK_SPEED, ..., 1.0, 1.0) 挂攻速；伤害挂被动 sq_GetBonusRateWithPassive(46, -1, 0, 1.0) + applyBasicAttackUp(attackInfo, 8)（普攻强化被动174）。钝器 MOD 还用 sq_Rewind(obj.sq_GetCurrentAni()) 直接热换动画。
- **getAttackAni 钩子做"技能伪装普攻"**（jg_swordman_common.nut:115）：
```squirrel
function getAttackAni_Swordman(obj, index) {
  if(isSwordSaber(obj)) {
    if(obj.getCurrentSkillIndex() == 46 && index == 2)     // 施放上挑(46)时
      return obj.sq_GetCustomAni(CUSTOM_ANI_UPPERSLASH_BLADESPIRIT); // 第3击位换成短剑上挑变体
```

### 1.4 移动普攻 / 原地普攻 / 前冲攻击(state 15) / 跳攻(state 7)

- 原地/移动中按 X 走同一 STATE_ATTACK=8：onSetState_*attack 第一行 obj.sq_StopMove() 截停；攻击中的前滑是引擎按攻击数据推的（.chr [move speed] 850 / [attack speed] 850），脚本不插手（女格斗 MOD 第5击例外：onProc 里用 sq_GetUniformVelocity 手写位移+Z轴抛物线）。
- **前冲攻击 = STATE_DASH_ATTACK(15)**：双击→(STATE_DASH=14) 中按 X。引擎用独立的 getDashAttackAni_* / getDashAttackInfo_* 回调。dashattack.ani 10帧（100/80/50/100/60/60/60/60/30/末帧ms），dashattack.atk：+40% 伤害、push 50 / lift 80、水平击退——数值明显高于普攻第一击，判定框从第0帧就存在（跑动中出伤）。
- 跳攻 = STATE_JUMP_ATTACK(7)，getJumpAttackAni_*（jumpattack.ani 6帧）。
- 攻速对以上全部生效（SPEED_TYPE_ATTACK_SPEED），动画按速度缩放，连段窗口随之缩短。

### 1.5 方向变体普攻怎么触发

男鬼剑普攻本体**没有**"按上+X"变体；方向变体全部走**技能 command 系统**：
- ↓+X → STATE_CROUCH_ATTACK(20) 下蹲攻击（引擎原生）；
- ↓+C（`{6=(DOWN)}{8=&}{6=(JUMP)}`）→ 后跳（技能169），后跳动作中 X → 后跳斩（技能49，command `{6=(ATTACK)}`）；backstep.nut 中后跳状态收到 datas[0]==49 即挂后跳斩子分支。
- 三段斩(8)：`{6=(RIGHT)}{8=&}{6=(SKILL)}` = 按住→+Z；鬼斩(5)：按住↑+Z；裂波斩(58)：→↑+Z。**"按住方向+Z"就是 DNF 的方向变体技，全部是 command 声明，引擎匹配。**

### 1.6 hitstop / 命中反馈

- **受击方**：.atk 的 [damage reaction] + sq_SetCurrentAttackeHitStunTime（击晕/僵直时间）、sq_SetAttackInfoHitDelayRateDamager（击中僵直率）、攻击包字段 ap.hitStunTimeAttackerDamager（**同时冻结攻受双方的僵直时间字段**，这是最接近 hitstop 的机制）。.chr [hit recovery] 600.0 决定自身被击硬直恢复。
- **攻击方**：普攻命中无脚本顿帧（attack.nut 的 onAttack_* 只挂特效）。屏幕反馈是技能级的：sq_SetMyShake(obj, 1, 100)（震屏100ms）、sq_flashScreen(...)，仅在格斗家 MOD 连击/必杀类技能的 onKeyFrameFlag 里出现。**结论：普攻的"打击感"靠受击僵直+每帧判定框+攻速节奏，不靠攻击方停帧。**

## 二、取消系统全景

### 2.1 clientonly\cancelskilllist.co（完整读取，245行）

格式：纯客户端白名单文件，[cancel skill] 下按 [character job]（大类）+ 反引号行（转职名，none=未转职/通用，extype=外传）+ 技能ID列表（TAB分隔）。语义：**列表内技能的指令在普攻等可取消状态中被点亮，可直接施放并打断当前普攻动作**（即"强制-XXX"的免费化全表）。

剑士（[swordman]）全表（ID→名已用 skill\swordmanskill.lst 互证）：

| 转职 | 可取消技能 |
|---|---|
| 通用 none | 8三段斩 5鬼斩 1格挡 46上挑 58裂波斩 68破军升龙击 24怒气爆发 2鬼印珠 103血气之刃 105流心 111鬼影鞭 112鬼影三击剑 169后跳 |
| weaponmaster(剑魂) | 通用 + 98 97 |
| soulbringer(鬼泣) | 通用 + 77 96 95 |
| berserker(红眼) | 通用 + 31 65崩山击 64 101 102 |
| asura(阿修罗) | **20 21 22（波动剑系）** + 通用 + 99 100 |

法师对照（[mage]/[at mage]）：女法通用 = 1天击 17龙牙 21落花掌 23 16 15 13 25多重射击 11 169后跳；各转职在通用基线上追加本系（元素111/113、魔道102/101/104…、战斗法94/98/123…）。规律：**通用层 = 低级小技能 + 后跳；转职层 = 本系核心输出技**。

### 2.2 取消窗口开在哪几帧

- 白名单技能的点亮由**引擎**按 cancelskilllist.co 处理，PVF 文本里查不到逐帧条件；剑士 attack.nut **没有任何 setSkillCommandEnable 调用**——原生取消不需要脚本参与。
- 脚本能证明的窗口行为：女枪 cancel\atgunnercancel.nut 的 onProc_* **每帧无条件** setSkillCommandEnable(40/53/76/77/51, true) → 窗口期内是"持续点亮"而非单帧脉冲。
- 合理推断（有旁证）：普攻取消窗口与 attackCancelStartFrame 同步开放（约动画中段，第一击≈第5帧/250ms 起）至动画结束，即"出完刀（判定帧过了）就能接技能，收招全程可取消"。引擎回调 setEnableCancelSkill_* 的 isEnable 参数即窗口开/关时被引擎调用。

### 2.3 setEnableCancelSkill_<职业> 谁调用

**全 sqr/ 无任何脚本调用方**（grep 证实），调用方是**引擎 C++**——在普攻取消窗口开启时以 isEnable=true 回调，脚本借此把**白名单之外（更新/MOD新增）的技能**也点亮。各职业实现在 <职业>_common.nut（swordman_common.nut:153 只做 obj.setSkillCommandEnable(229..247, true) 一串；atfighter_common.nut:2 点亮 220-230——恰好都是柔化表里那批技能，即"新技能默认也能被普攻取消"）。知识库 02:65 明确定性："setEnableCancelSkill_* - 普攻强制施放技能"。

### 2.4 柔化/强制中断（技能→技能取消）—— ap_atfighter_comminterrupt.nut 全解

**是什么**：女格斗**柔化肌肉（技能171，atfighterskill.lst 互证）**被动，学到后由 passive_skill_atfighter.nut:44 **常驻挂 appendage**，其 proc 每帧执行 → **任何非排除状态中，按已点亮技能的指令就直接 AddSetStatePacket 切进该技能状态，立即打断当前技能**（含普攻）。

三件套模板（swordman_common.nut:110 同款拷贝）：
```squirrel
function EnableSoften(obj, skillindex, state) {
  if (obj.sq_GetState() == state) return false;   // 已在该状态则不点亮（防自打断）
  obj.setSkillCommandEnable(skillindex, true);    // 点亮技能指令（图标亮起）
  return true;
}
function SetSkillState(obj, skillindex, state, Arr) {
  local iEnterSkill = obj.sq_IsEnterSkill(skillindex);   // 玩家输入了该技能指令?
  if (iEnterSkill == -1) return false;
  if (obj.sq_GetState() == state) return false;
  if (obj.sq_IsUseSkill(skillindex)) {             // 检查并扣除CD/MP（没有此调用则无CD）
    obj.sq_IntVectClear();
    foreach(sub in Arr) obj.sq_IntVectPush(sub);   // 预填子状态向量
    obj.sq_AddSetStatePacket(state, STATE_PRIORITY_USER, true); // 强切状态=打断
    return true;
  }
}
```

**状态黑名单**（哪些状态不许柔化）：mystate == 0站立 || 3受伤 || 4倒地 || 5死亡 || 9抓取 || 16拾取（swordman 版额外排除 7跳攻/236/235/25等）。**即站立待机本来就能放技能无需柔化，被控状态不该被取消。**

**覆盖技能**（EnableSoften+SetSkillState 成对，技能ID→状态ID→子状态数组）：通用层 上挑5→30、下段踢6→21、前踢9→22、鹰踏17→28、疾风追击108→59、瞬步2→19、旋风腿86→52、铁山靠13→63、崩拳58→34；再按 sq_getGrowType 分转职各挂 12-14 个（气功 111/15/90/117/220/120/221/67/222…，散打 80/4/1/83/124/122/19/68/82/223-225，街霸 3/106/76/77/105/119/123/226-228，柔道 49/87/81/54/18/89/88/63/118/121/229-231）。

**与 cancelskilllist 的区别**：
| | cancelskilllist.co（普攻取消） | comminterrupt appendage（柔化） |
|---|---|---|
| 打断对象 | 普攻（state 8 等） | **任意技能状态**（黑名单外全状态） |
| 数据源 | 引擎读客户端白名单 | **脚本逐帧轮询**（proc） |
| 窗口 | 引擎控制（普攻中后段） | **全动画任意帧**（含前摇！只要不在黑名单状态） |
| 代价 | 无 | 学被动（占技能点/挂永久appendage） |
| 附带 | 无 | 女枪版在普攻/蹲攻/坐等状态取消时触发 stylish sq_SetMyShake(8,50) 计层奖励 |

男鬼剑同款 MOD 技能：**252 体术逆改**（skill\swordman\swordman_comminterrupt.skl："施放技能时，可强制中断当前技能施放其它技能"，passive_skill_swordman.nut:28 挂 ap_swordman_comminterrupt.nut，覆盖鬼斩5→20、崩山65→36、裂波58→32、三段8→22（限Z轴=0且非跳）等 + 各转职派生，另排除 236/235/7/25 状态）。

### 2.5 后摇取消机制

- 柔化/体术逆改的 SetSkillState 在 appendage proc 里**每帧检查**，所以取消发生在任意帧——**不等 onEndCurrentAni**，直接 sq_AddSetStatePacket(新状态, USER, true) 把当前状态顶掉（USER 优先级高于状态自然结束的 AUTO，低于 FORCE/IGNORE_FORCE，因此被击/抓取仍能打断你的取消）。
- "谁允许这么干"= 常驻 appendage 的全局 proc + setSkillCommandEnable 强制点亮（绕过引擎"当前状态不可用该技能"的限制）+ sq_AddSetStatePacket 的状态机顶替。**不需要目标状态脚本配合**——三行式通用模板，任何职业抄走就能加柔化。
- 技能内部的后摇派生（非取消）则走 onKeyFrameFlag/onProcCon：如 attack.nut 被注释的 SwordGhost 代码——动画帧标记 123 命中时直接 AddSetStatePacket(STATE_SWORD_GHOST_1, USER)。

## 三、状态条件技 & 技能后派生实例

### 3.1 [command] 格式解码（.skl 文本，引擎匹配）
`{槽位=记号}` 序列：槽6=前置条件（方向键**按住**/所处状态隐含）、槽8=触发键；记号 (SKILL)=Z、(ATTACK)=X、(JUMP)=C、(UP/DOWN/LEFT/RIGHT)=方向、`&`=同时按、`,`=先后序列接续。

- **银光落刃 ashenfork（技能16）**：`{6=`(SKILL)`}`，说明"跳跃状态下 Z"——**状态条件技**：仅当处于 STATE_JUMP(6) 时 Z 才触发。ashenfork.skl 的 [command key explain]："跳跃状态中向下方敌人发出强力刺击；跳跃的高度越高攻击力越强"（static data 50 10 50 100 含落点/冲击波参数），配套 attackinfo\ashenfork.atk（damage reaction=down，push 270/lift 180，方向 hit down——落地砸击型）。**sqr/ 下没有 ashenfork.nut**：跳跃态的推进/落点判定由引擎原生处理，PVF 只出数据。
- 后跳斩49：`{6=`(ATTACK)`}`=后跳动作中 X；三段斩8：`{6=(RIGHT)}{8=&}{6=(SKILL)}`=按住→+Z；后跳169：`{6=(DOWN)}{8=&}{6=(JUMP)}`=↓+C。

### 3.2 脚本版状态条件技/派生（跳跃中检测，swordman\jump\swordman_jump.nut）
```squirrel
function enableFlowMindOneFallState(obj) {
  if (state == 6 && 武器!=钝器 && !sq_IsKeyDown(OPTION_HOTKEY_ATTACK, ...)) { // 跳跃态且没按X
    obj.setSkillCommandEnable(105, true);       // 点亮流心(105)
    if (obj.sq_IsEnterSkill(105) != -1 && !skill.isInCoolTime()) {
      obj.startSkillCoolTime(105, ...);
      if (obj.sq_IsUseSkill(107)) {              // 子技能流心:跃可用
        obj.sq_IntVectPush(0);
        obj.sq_AddSetStatePacket(STATE_FLOW_MIND_ONE_FALL_STATE, USER, true); // 空中派生
      }
    }
  }
}
```

### 3.3 完整"技能后派生"实例：流心系（swordman_common.nut:1 procAppend_Flowmind_Comminterrupt，由 sqr\equipment\equipment_swordman.nut:41 procAppend_Swordman 每帧驱动）

剑魂流心(105)是**多状态派生中枢**——按当前状态决定 107流心:跃/108流心:升/109流心:狂 落点：
- 地面(0)/前冲(14)/流心收招(63) → 跃62；
- ��跳(29) 须 动画时长>300ms 后才可派生（**后摇门槛实例**）；
- 空中 ZPos>30 且 跳跃动画>400ms → 空中跃147，且要求 X 键未按住；
- 流心态(62)持钝/短剑时按 Z 须 动画>370ms → 狂64（UseSkillState 直接检测 sq_IsKeyDown(OPTION_HOTKEY_SKILL) 而非指令序列）。

这就是"上挑后接派生"类需求的官方答案：**外层每帧 proc + 状态白名单 + 时间/高度门槛 + setSkillCommandEnable 点亮 + AddSetStatePacket 切换**。

## 四、连招手感���键规则总结（数值量级）

**普攻三连时序（男鬼剑，攻速850基准、SPEED 1.0）**

| 段 | 动画帧数 | 帧延时 | 总时长 | cancelStartFrame(窗口起点) | 窗口宽度(估) |
|---|---|---|---|---|---|
| attack1 | 10 | 9×50ms+末帧150ms | **600ms** | 引擎数据(≈5帧/**250ms**) | ~350ms |
| attack2 | 11 | 10×50+150 | **650ms** | 引擎数据(≈5-6帧) | ~400ms |
| attack3 | 9 | 8×50+150 | **550ms** | 短剑脚本=第5帧(250ms) | ~300ms |

- 脚本侧观察值：毒蛇格斗家=每段第3帧(150ms)；血契女法=5/6/6帧；短剑流3→4击门槛=第5帧。**量级：普攻每段 0.5-0.65s，输入窗口从动画中段(~250ms)开到段末，攻速提升等比压缩全部。**
- **输入缓冲**：DNF 无现代意义上的预输入缓冲——sq_IsEnterCommand 是当帧判定（sq_SetKeyxEnable 先使能再查询的写法="这一帧才监听X"）；按早了(未到 cancelStartFrame)或按晚了(动画结束回 STAND)都丢弃。柔化/流心用 appendage/proc 每帧轮询等价于"全动画缓冲"。
- **取消层级**（从松到紧）：柔化(任意技能帧，黑名单外全状态) > 普攻取消进白名单技能(普攻中后段) > 普攻段间推进(cancelStartFrame 后重按X) > 动画自然结束。
- **优先级铁律**：玩家取消用 STATE_PRIORITY_USER；被击/倒地/抓取用 FORCE/IGNORE_FORCE——所以任何连招都能被打断，手感由受击方 [hit recovery] 600 与攻方僵直字段（hitStunTimeAttackerDamager/HitDelayRateDamager）共同决定，攻方普攻不停帧。
- **可抄的三件套**：EnableSoften / SetSkillState / UseSkillState（约30行）+ 常驻 appendage + 每帧 proc = 通用"任意状态取消"系统；getAttackCancelStartFrame(Size)_* 回调 = 通用"连段窗口"系统；.ani 逐帧 DAMAGE BOX + .atk 段位 reaction 差异 = 打击感数据层。

**主要文件出处**：
- sqr/character/swordman/attack/attack.nut（普攻钩子/短剑第4击/上挑变体）
- sqr/character/jg_swordman/jg_swordman_common.nut（getAttackAni/getAttackCancelStartFrameSize 等）
- sqr/character/swordman/swordman_common.nut（setEnableCancelSkill_Swordman、流心派生、三件套）
- sqr/character/atfighter/appendage/ap_atfighter_comminterrupt.nut + passive_skill_atfighter.nut（柔化）
- sqr/character/swordman/appendage/ap_swordman_comminterrupt.nut（MOD 体术逆改）
- clientonly/cancelskilllist.co、skill/swordman/*.skl、character/swordman/{swordman.chr, animation/attack*.ani, attackinfo/*.atk}
- 知识库/{02,03,04,10,12}.md、常量定义.txt
