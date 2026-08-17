# R2-H：DNF 技能原型拆解（弹道 / 抓取 / 召唤·变身）

> 第2轮 Agent H 原始笔记。任务：深读三个新原型复杂技能。
> 承接第一轮（三段斩/血气爆发/血爆）。★ = 本轮新增 API。
> 总判断：**DNF 老技能（技能号小，如 50 崩山击波、31 噬魂之手）的角色侧状态机在 C++ 引擎里，.nut 只挂回调钩子；新技能（技能号 130+/220+）整条状态机在 .nut 里写**。
> 已综合进：05-复杂技能案例-总结.md

---

## A. 弹道类 —— 鬼剑波动剑系

### A1. 主样本：冰波动剑（技能 100，state 24 WaveSword → PO 24328）

**注册**（swordman_load_state.nut L16-17）：
```squirrel
IRDSQRCharacter.pushState(0, "character/Swordman/wave/wave.nut", "WaveSword", 24, -1);
IRDSQRCharacter.pushPassiveObj("character/Swordman/wave/po_wavecut.nut", 24328);
```

**时间线**：
1. **触发**：玩家按技能键 → 引擎（state 24 为老状态，C++ 内建）播放挥手动画。脚本侧只在 `onKeyFrameFlag_WaveSword(obj, flagIndex)` 动画打帧标记处接管。
2. **弹道创建（一次造两条，y 错开 ±15）**：
   - `if (!obj.isMyControlObject()) return`——**只在本地控制端发包创建**，其它客户端由同步包生成（PO 只有一份权威来源）。
   - 取值：`atk = obj.sq_GetPowerWithPassive(100, 24, 3, -1, 1.0)`（伤害列3）、`atk2`（列7，爆炸段伤害）、`count`（列1，段数）、冰冻 proc/level/time（列4/5/6，proc 除以10成概率）。
   - **传参协议**：`obj.sq_StartWrite()` → 依次 WriteDword（atk, 125(id), count, currIdx, dist=75, size=100, yOff(±15), maxT=3000, freezeProc, freezeLv, freezeTime, atk2）→ `obj.sq_SendCreatePassiveObjectPacket(24328, 0, 75, 1, 0)`（PO编号, skill槽, x偏移, y偏移, z偏移）。**写入顺序 = PO 侧读取顺序，纯位置协议**。
3. **PO 出生**（po_wavecut.nut `setCustomData_po_wavecut(obj, receiveData)`）：
   - `receiveData.readDword()` 按序读回全部参数；
   - `obj.setCurrentAnimation(ani)`（经 `obj.getVar().GetAnimationMap("", "passiveobject/.../icewave.ani")` 缓存取动画）；
   - **攻击盒安装**：`local attackInfo = sq_GetCustomAttackInfo(obj, 103); sq_SetCurrentAttackInfo(obj, attackInfo);` 然后两步取引用改伤害：`attackInfo = sq_GetCurrentAttackInfo(obj); sq_SetCurrentAttackPower(attackInfo, attackBonusRate);`
   - `sq_SetAttackBoundingBoxSizeRate(currentAni, sizeRate, sizeRate, sizeRate)`——攻击盒随 size 参数缩放（视觉缩放与判定缩放同步）；
   - `sq_SetChangeStatusIntoAttackInfo(attackInfo, 0, ACTIVESTATUS_FREEZE, proc, level, time)` ★——**把异常状态直接注进攻击信息，命中即判定冰冻**（枚举：FREEZE=1, BLEEDING=11, STUN=3…见 dnf_enum_header.nut L1180+）；
   - 参数转存 `obj.getVar("var").push_vector(...)`，供 procAppend 用；两枚 flag（0=已分生、1=已爆炸）。
   - 另挂一个纯视觉 pooledObj（随机 6 选 1 的 .ani，setSpeedRate/setImageRateFromOriginal）存进 getVar("effectObj")。
4. **飞行 = 分段接力（DNF 地波弹道的真面目：不是移动体，是链式生灭）**（procAppend_po_wavecut）：
   - `currentT = sq_GetCurrentTime(currentAni)`；`currentT >= 10 && flag0==0` → 置 flag0=1，若 maxCount > currCount：仅 isMyControlObject() 端，在 `sq_GetDistancePos(obj.getXPos(), dir, dist)` 处用 **PO 侧发包 API**（与角色侧不同名）：`sq_BinaryStartWrite() / sq_BinaryWriteDword(...) / sq_SendCreatePassiveObjectPacketPos(obj, 24328, 0, posX, y+yOff, z)` 生下一段（currCount+1）。
   - `currentT >= maxT(3000) && flag1==0` → 终结：播放爆炸特效，**`sq_createAttackObjectWithPath(obj, "....ani", "....atk", false, atk2, size, 0, 0, 37)` ★ 一次性攻击对象**（直接给 ani+atk 路径+倍率，引擎代管生灭，适合爆炸帧），关掉视觉 pooledObj（effect.setValid(false)），`sq_SendDestroyPacketPassiveObject(obj)` 自毁。
   - `destroy_po_wavecut(obj)`：兜底清理 effectObj——**PO 销毁钩子里必须回收 pooled 视觉对象，否则特效悬空**。
5. **命中反应**（onAttack_po_wavecut(obj, damager, ...)）：
   - 前置过滤：`!sq_IsHoldable(obj, damager) || !sq_IsGrabable(obj, damager) || sq_IsSuperArmor(damager)` 则不控场；
   - 命中即挂 ap_wavehold.nut appendage（若未挂）并 **`sq_HoldAndDelayDie(damager, obj, true, false, false, 200, 200, ENUM_DIRECTION_NEUTRAL, masterAppendage)`**——目标定身 200ms（波动剑"软控"表现）+ `masterAppendage.sq_SetValidTime(2000)`。
6. **伤害本体**：不在 onAttack 里算——由 attackInfo（含注入的 power/状态）随引擎碰撞流程结算；onAttack 只追加控制类效果。

### A2. 辅样本：崩山击波（grand wave，技能 50，PO id 11/12/13，真·平移弹道）

在 shared_passive_object/swordman/ 的分发式 PO 脚本（setcustomdata.nut / procappend.nut / ontimeevent.nut / onattack.nut 按 obj.getVar("id") 分发）：
- **创建参数**：`obj.getCustomAnimation(8/9/10)` + `sq_GetCustomAttackInfo(obj, 6/7/8)`，伤害 `parentChr.sq_GetPowerWithPassive(50, -1, 0, -1, 1.0)` → sq_SetCurrentAttackPower。
- **飞行**（procappend id 11/12/13）：出发时记 grandWaveMove = [x0, xTarget]（xTarget = sq_GetDistancePos(x, dir, distance)，distance 取技能等级数据）；每帧 `x = sq_GetUniformVelocity(x0, xTarget, currentT, 3000)` → `sq_setCurrentAxisPos(obj, 0, x)`——**匀速插值位移而非物理速度**，出界即销毁。三档 id 对应 原版/强化(TP加距离加攻速)/二段 变体。
- **多段命中**：创建时 `obj.setTimeEvent(0, attackTerm, 0, false)`（attackTerm=技能列0）；onTimeEvent case 0：每 tick `obj.resetHitObjectList()` ★——**清空已命中名单，让穿透弹道对同一目标按间隔反复结算**（全库用 364 次，多段 PO 标配）。case 1 定时自毁，case 2 定时撒随机子 PO。
- **onAttack**：按 id 追加命中效果（火波 id19 挂燃烧 appendage 4000ms；冰波 id22 挂冰冻，时长取 sq_GetIntData(parentChr, 100, 0, lv)）。
- **父引用**：`parentChr = sq_ObjectToSQRCharacter(obj.getTopCharacter())`——PO 随时回溯主人取技能面板；`chrState = parentChr.sq_GetState()` 可做"主人进入 X 状态则弹道消失"类联动。

### A 小结：弹道生命周期管理模式
- **三种弹道形态**：①链式生灭（wavecut：每段独立 PO，10ms 接力，天然支持分段属性变化）；②插值平移（grandwave：单 PO 匀速走，出界销毁）；③引擎托管一次性攻击对象（sq_createAttackObjectWithPath，爆炸帧专用）。
- **协议模板**：sq_StartWrite/Write[Dword|Word|Bool] → sq_SendCreatePassiveObjectPacket[Pos] ↔ setCustomData_po_* 里 readDword()。角色侧叫 obj.sq_*，PO 侧叫 sq_Binary*。
- **只有 isMyControlObject() 端发包/切状态**；**所有本地资源（pooledObj）在 destroy 钩子回收**。
- 命中伤害走 attackInfo 数据（创建时配好 power/受击反应/异常状态），onAttack 只做追加控制与特效。

**文件清单（A）**：
- sqr/character/swordman/wave/{wave,po_wavecut}.nut、ap_wavehold.nut
- sqr/shared_passive_object/swordman/{setcustomdata,procappend,ontimeevent,onattack}.nut
- sqr/character/swordman/attack/grandwave.nut（仅 checkExecutable，主体引擎）
- 注册：swordman_load_state.nut

## B. 抓取类 —— 过肩摔族 + 噬魂之手

知识库疑问的答案：**onAttack "返回受击对象"在脚本层的等价物 = 把 damager push_obj_vector 存起来 + 挂 hold appendage**。受害者状态即 STATE_HOLD = 9（dnf_enum_header.nut L153，确认；另有 STATE_HOLD_UP=53、STATE_JUMP_SUPLEX=24/25 专用变体）。经典过肩摔（state 23）本体在 C++；全脚本抓取看以下���个。

### B1. 主样本：飓风超级过肩摔（fighter，技能/状态 239）

**时间线**（hurricanesupersuplex.nut，变量被混淆但逻辑完整）：
1. **入场**：checkExecutableSkill → sq_IsUseSkill(239) → sq_IntVectPush(0) + sq_AddSetStatePacket(239, USER, true)。
2. **substate 0 抓取判定帧**：sq_SetCurrentAnimation(155); sq_SetCurrentAttackInfo(87);（攻击盒 87 = 判定用）
3. **onAttack（命中）**（L103-131，抓取成立三查）：
   ```squirrel
   if (!damager.isObjectType(OBJECTTYPE_ACTIVE)) return;   // 只抓 Active（怪/角色）
   if (sq_IsGrabable(obj, damager) && sq_IsHoldable(obj, damager) && !sq_IsFixture(damager)) {
       obj.getVar().push_obj_vector(damager);              // ★受害者登记（可多个）
       local ap = CNSquirrelAppendage.sq_AppendAppendage(damager, obj, 239, true,
                    "character/fighter/hurricanesupersuplex/ap_hurricanesupersuplex.nut", true);
       sq_HoldAndDelayDie(damager, obj, true, true, false, 0, 0, ENUM_DIRECTION_NEUTRAL, ap);
       ap.setState(1或2, null);                            // 通知受害者侧同步状态
   }
   ```
   - **sq_IsGrabable / sq_IsHoldable / !sq_IsFixture ★ 是可抓三查**：可抓取/可定身/非固定物（BOSS 通常 fixture → 不可抓）。
   - **sq_HoldAndDelayDie(victim, grabber, bool×3, x, y, direction, appendage) ★**：把 victim 打入 HOLD(9) 并绑定 grabber；第 6/7 参在 wavecut 里传 (200,200)、这里传 (0,0)（疑似定身时长/死亡延迟参数）；**appendage 作为"锁"传入，appendage 失效即松手**——双人绑定的锚点。
   - substate0 首次命中置 bool 标记"抓到了"。
4. **分支**：onEndCurrentAni case0：getBool(0)==true → substate1（上挑，ani156/atk88，倍率 sq_GetBonusRateWithPassive(239,239,0,1.0)）；没抓到 → STATE_STAND（**抓空即落回站立，不进入后续演出**）。
5. **substate 2 腾空**（自管 Z 轴）：
   ```squirrel
   local t = sq_GetCurrentTime(ani);           // 0..500ms
   z = sq_GetAccel(0, 270, t, 500, true);      // 加速升到 270px
   sq_setCurrentAxisPos(obj, 2, z);            // ★按轴写位置，2=Z
   ```
   全屏背景特效：屏宽 getFieldXPos(800 + 2*CFG_SCREEN_WIDTH_OFFSETX, ...)、sq_CreatePooledObject + ENUM_DRAWLAYER_BOTTOM。500ms 到顶 → substate4。
6. **substate 4 空中抓取帧**（ani159）：把所有受害者 appendage setState(3, null)——受害者 setCustomRotate(true, sq_ToRadian(180)) 倒转 + 抬到 Z + sq_GetObjectHeight(victim)（头顶）。**关键帧 flag 2/3 还会把受害者 XY 钉在抓取者身后 -45px**（onKeyFrameFlag L141-155，方向/朝向各翻一次符号）。
7. **substate 3 下砸**（100ms 起跳缓冲后）：sq_GetUniformVelocity(zTop, 0, t, 800) 匀速砸地，受害者 Z 跟随 + 身高抬升防入地；obj.sq_GetStateTimer() 计时（**state 计时器，重进状态归零**）。落地 → substate5。
8. **substate 5 结算**：sq_StartWrite(239, 1, bonusRate) → sq_SendCreatePassiveObjectPacket(24373, 0, 0, 0, 0) 落点爆炸 PO（脚本为空壳 share_po_fighter_24373.nut，引擎结算伤害）→ 播完 → STAND。
9. **受害者侧同步机（ap_hurricanesupersuplex.nut）**——双人同步的核心：
   - appendage.sq_AddFunctionName("onChangeState"/"proc"/"onEnd", ...)；append.getParent()=受害者、getSource()=抓取者。
   - onChangeState：state1=记当前 XYZ，100ms 内 lerp 到抓取者前方 50px（sq_GetUniformVelocity 三轴）；state2=Z 从当前升到 600（300ms）；state3=倒转+抬头顶。
   - proc：**source.getState() != 239 || sourceSubState == 5 → append.setValid(false)**——主人技能中断/进入终结段，锁立即失效（防悬挂）。
   - onEnd：setCustomRotate(false, 0) 复原 + **sq_SimpleMoveToNearMovablePos(victim, 5000) ★ 把受害者挪到最近可站立点（防扔进墙里）**。

### B2. 副样本 1：肘击连投（fighter elbowthrow，技能 237）—— 抓取技的"投出"参数

- 循环抓（substate0 抓 → 1 肘击循环 moveCount 次 → 2 终结）：受害者每帧被钉在抓取者身上的**动画关键帧驱动偏移表**（onKeyFrameFlag flag 100~106 → pos = [dx, dy, facing]，受害者沿身体轨迹被甩）。"抓取者的动画帧直接编排受害者挂点"模式。
- **投出结算**（flag 110 → sq_SendChangeSkillEffectPacket(obj, 237) → onChangeSkillEffect，**跨进程效果包，把"此刻投掷参数"广播出去在本地执行**）：
  ```squirrel
  CNSquirrelAppendage.sq_RemoveAppendage(victim, ".../ap_elbowthrow.nut");  // 先解锁
  sq_MoveToNearMovablePos(victim, ...);                                     // 防卡墙
  // 仅第一只：发 24373 爆炸 PO（带 victim 的 group/uniqueId 定位 + 倍率）
  // victim 端：
  local gIntV = sq_GetGlobalIntVector();
  sq_IntVectorPush(gIntV, sq_GetOppositeDirection(victim.getDirection())); // 反向
  sq_IntVectorPush(gIntV, 0); sq_IntVectorPush(gIntV, 1);
  sq_IntVectorPush(gIntV, power /*按住下键=1600 否则 900*/);
  sq_IntVectorPush(gIntV, 100);
  sq_AddSetStatePacketActiveObject(victim, STATE_DOWN, gIntV, STATE_PRIORITY_FORCE);
  victim.flushSetStatePacket();
  ```
  ★ **被抓方最终以 STATE_DOWN(4) + intVector(方向, ?, ?, 抛出力, ?) 被强制切状态**——投技的受击表现不走 damageAct，直接由抓取方下发 DOWN 状态参数。`sq_AddSetStatePacketActiveObject` + `flushSetStatePacket` 是**对 Monster/ActiveObject 切状态的标准通道**（受害者是怪不是角色，没有 sq_ 那套）。
- **方向修正输入**：按住前/后/上/下决定下一肘位移 (±170, ±25)——抓取技的方向操控全在 onEndCurrentAni 里读 sq_IsKeyDown(OPTION_HOTKEY_MOVE_*)。
- **多目标与去重**：getVar("bool1") 记"遇到过不可抓目标"；onAttack 里若三查失败 → 置 bool1，动画结束后走 substate6（落空收招，ani149 + flag1 时切 atk85 补一段伤害）。伤害补发用 sq_SetCurrentAttackInfo(85)+BonusRate 在收招帧临时换攻击盒。

### B3. 副样本 2：噬魂之手 GRABHAND（swordman state 26）—— **不可抓取分支**（老技能，钩子式）

grabhand.nut onAttack_GRABHAND：只在 `!sq_IsHoldable || !sq_IsGrabable || sq_IsFixture`（目标是霸体/不可抓）时动作：
- 播放"抓空爆血"特效（createGrabBloodHandunGrabEffect，pooledObj + sq_moveWithParent 挂身上）；
- 存 damager → sq_AddSetStatePacket(26, [1, skill, -1]) 进落空子状态；
- **onAfterSetState_GRABHAND：sq_SendHitObjectPacket(obj, dama, 0, 0, 0) ★ 手动对目标补一发命中结算**（不可抓 ≠ 免疫，直接按攻击盒结算伤害，跳过抓取演出）。
- onEndCurrentAni_GRABHAND：sq_SendMessage(obj, OBJECT_MESSAGE_INVINCIBLE, 1, 0) + sq_PostDelayedMessage(obj, OBJECT_MESSAGE_INVINCIBLE, 0, 0, 500) ★——**抓取者收招 500ms 无敌**（消息 1 开 / 延迟 500ms 后发 0 关）。
- onEndState：sq_SetCustomDamageType(obj, false, 0)（配合 skill 117 被动，抓取期间改伤害类型，退出还原）。

### B 小结：抓取的双人状态同步模式
1. **判定**：onAttack + 三查（Grabable/Holdable/!Fixture）+ OBJECTTYPE_ACTIVE；成立 → victim 入 obj_vector + 挂 appendage（appendage 即"锁"，source=抓取者 parent=受害者）+ sq_HoldAndDelayDie 打入 STATE_HOLD。
2. **同步**：appendage 是跑在受害者身上的小状态机，appendage.setState(n) 由抓取者驱动；位置同步 = 每帧 sq_GetUniformVelocity/sq_GetAccel 插值 + sq_setCurrentAxisPos（受害者）或 setCurrentPos（跟随点表）。抓取者状态机中断 → appendage setValid(false) 自动松绑。
3. **结算时机**：伤害不在抓取瞬间，而在**动画关键帧/最终段**（flag 或终结 substate）经 PO 或 sq_SendHitObjectPacket 结算；投出 = 移除 appendage + victim 强制 STATE_DOWN(intVector[反向,…,抛力,…]) + sq_MoveToNearMovablePos 防卡墙 + 无敌帧保护收招。
4. 抓空（判定框碰到人但三查失败）→ 落空子状态 + 立即补伤。

**文件清单（B）**：
- sqr/character/fighter/hurricanesupersuplex/{hurricanesupersuplex,ap_hurricanesupersuplex}.nut
- sqr/character/fighter/elbowthrow/{elbowthrow,ap_elbowthrow}.nut
- sqr/character/swordman/grabhand/grabhand.nut（不可抓分支）
- sqr/character/fighter/suplexcyclone/suplexcyclone.nut（抓取技中开输入窗转别的技能：sq_GetSkill(x).isInCoolTime() + setSkillCommandEnable + sq_IsEnterSkill + startSkillCoolTime 手动开冷却后直接 AddSetStatePacket 切别的技能状态——技能取消的通用模板）
- 空壳：common_object/share_obj/share_po_fighter_24373.nut（引擎结算）

## C. 召唤 / 变身类

### C1. 主样本：召唤克鲁塔（mage，技能 136 → monster 50680731）—— 全链路可读

**时间线**：
1. **入场与重复施放分流**（summonkruta.nut checkExecutable）——"同一技能键按情境换功能"：
   - 若 getMyMonsterObject_Character_*(obj, 50680731, "…/ap_summonkruta.nut") ★ 找到**自己**的存活召唤物（遍历 obj.getObjectManager().getCollisionObject(i)，条件：OBJECTTYPE_MONSTER + !obj.isEnemy(object)（**友方判定**）+ getCollisionObjectIndex()==50680731 + 挂着该 appendage 且 appendage 里存的 sqrChr 是自己 → 多人各召唤各的互不干扰）：不走 sq_IsUseSkill，直接 sq_IntVectPush(2) + sq_AddSetStatePacket(136) 进**召回/传送分支**。
   - 若 onGetMyPassiveObject_My(obj, 24372, 136, 1) ★（按 PO 编号+skill+subType 过滤自己的 PO）找到标记 PO → 正在召唤中 → 播放"无法使用"警告（startCantUseSkillWarning / sq_AddMessage(414) / sq_PlaySound("WZ_NOMANA")）返回 false。
   - 否则正常 sq_IsUseSkill(136) → substate0。
2. **substate 0 读条**：obj.sq_GetThrowChargeAni(2)（★通用投掷/施法状态动画表）+ sq_GetCastTime(obj,136,lv) → sq_StartDrawCastGauge(obj, time, true) ★ 读条 UI；施法速度 sq_SetStaticSpeedInfo(CAST_SPEED)；按读条/动画时长比换算真实进入下一阶段的时间存 var（**帧同步下用动画 delaySum 比率折算**）；onProcCon 里 sq_GetStateTimer() >= 换算值 → substate1。
3. **substate 1 生成实体**（双包并发）：
   ```squirrel
   if (obj.sq_IsMyControlObject()) {
       obj.sq_StartWrite(); obj.sq_WriteDword(136); obj.sq_WriteDword(1);
       sq_SendCreatePassiveObjectPacketPos(obj, 24372, 0, 前方100px, y, 1111 /*z=魔法信标*/);
       SummonMonsterPacket(obj, 50680731, obj.getTeam(), summonLevel, summonTime,
                           100, 0, 0, obj.getDirection(), 0);
   }
   ```
   - **SummonMonsterPacket(施法者, 怪物图鉴ID, 队伍, 等级, 存活ms, x, y, z, 朝向, aiType) ★ 是召唤核心**：召唤物不是 PO 也不是新角色，是**引擎级 Monster（ActiveObject）**，按队伍字段进友方阵营。等级/时长直接来自技能表（sq_GetLevelData(136, 0/3, lv)）。
   - **标记 PO 24372 = 出生握手信标**：z=1111 是暗号，share_po_mage_24372 的 procAppend（common_object/share_obj/mage/procappend.nut L428+）：出生后扫到 index==50680731 的怪 → 向施法者 sq_SendChangeSkillEffectPacket(chr, 136) 回执（带怪的 sq_GetGroup/sq_GetUniqueId）→ 自毁。**为什么这么绕：SummonMonsterPacket 是异步的，脚本拿不到直接引用，用 PO 轮询 + 技能效果包���执拿到新对象的 group/uniqueId 句柄**。
4. **接生**（mage_common.nut onChangeSkillEffect_Mage case 136）★——sq_SendChangeSkillEffectPacket/onChangeSkillEffect_<StateName> 是**任意时机触发远端脚本逻辑的通用 RPC 通道**：
   ```squirrel
   local monObj = sq_GetCNRDObjectToActiveObject(sq_GetObject(obj, group, uniqueId)); // ★三元组寻址
   monObj.setMapFollowParent(obj); monObj.setMapFollowType(1);   // ★跨房间跟随主人
   local ap = CNSquirrelAppendage.sq_AppendAppendage(monObj, monObj, 136, true, "…/ap_summonkruta.nut", false);
   CNSquirrelAppendage.sq_Append(ap, monObj, monObj, false);
   setStartInfo_appendage_mage_summonkruta(obj, ap);            // 初始化AI计时器
   ```
5. **AI = 挂在召唤物身上的 appendage**（ap_summonkruta.nut）：
   - setStartInfo：建 **timer_vector**（push_timer_vector / setParameter(间隔ms,-1) / resetInstant(0) / isOnEvent(timer) / setEventTerm ★）——每种攻击一个可调间隔计时器（等级≥5 解锁攻击2、≥10 解锁攻击3/大招，间隔实时按主人技能等级刷新）。
   - proc：怪 STATE_STAND 且到点 → sq_FindTarget(parentObj, -40, 240, 40, 40) ★ 索敌 → sq_getRandom(0,100)>40 选技能分支 → **sq_AddSetStatePacketActiveObject(monster, STATE_ATTACK, [attackIdx], USER) 命令怪攻击**；攻击中按帧号（sq_GetAnimationFrameIndex）/getStateTimer().Get() 驱动连段子状态。
   - onSourceKeyFrameFlag（**appendage 能吃宿主动画关键帧** ★）：flag101~105 → sq_SetCurrentAttackBonusRate(sq_GetCurrentAttackInfo(monster), sq_GetBonusRateWithPassive(主人, 136, 136, col, 1.0))——**召唤物的每次攻击倍率实时取自主人面板**；flag106 发子 PO；flag111 画地裂特效。
   - **寿命**：主人 getState()==STATE_DIE 或 appendage 失效 → onEnd 里 parentObj.sendDestroyPacket(true) ★ 召唤物自毁（另外 SummonMonsterPacket 的 summonTime 由引擎到期杀）；sqrChr 引用存在 appendage var 里。
6. **召回分支（substate2/3）**：把怪 sq_MoveToNearMovablePos 挪到面前、播传送特效、sq_AddSetStatePacketActiveObject(mon, 8, [4], FORCE) 让怪立即出招。**配套��生技能 137（repeatedsmashofrage）：单独技能键，checkExecutable 先找自己的怪，找到才可用，向怪发攻击指令**——召唤体系的"指挥技能"就是这样叠出来的。

### C2. 变身样本：贝亚娜变身（mage avatar，技能 245 / STATE_MAGE_AVATAR）—— 持续形态维持

- **一键双态**（avatar.nut checkExecutable）：挂着 ap_avatar.nut（变身中）且 getVar("skill").getBool(0)==false → 按键 = substate1 **解除变身**；否则正常 sq_IsUseSkill 施放变身。
- **变身生效**：动画结束 sq_BinaryWriteWord(1); sq_SendChangeSkillEffectPacket(obj, 245) → mage_common.nut case245：
  - **属性 buff appendage**：ap_avatar_icon.nut 挂自己 + masterAppendage.sq_SetValidTime(time) ★（时长=等级数据，buff 图标/倒计时载体）+ setAppendCauseSkill(BUFF_CAUSE_SKILL, job, skill, level) ★（buff 来源登记）；
  - **sq_AddChangeStatus("changeStatus", obj, obj, 0, 37, false, 0) + addParameter(CHANGE_STATUS_TYPE_*, false, value)** ★——通用属性修改通道（HP_MAX/物防魔防/攻速移速/异常抗性/物魔攻，值经 sq_GetAbilityConvertRate(obj, CONVERT_TABLE_TYPE_HP) 换算）；
  - **形态 appendage**：ap_avatar.nut（无时限）。
- **形态维持**（ap_avatar.nut proc，每帧）：
  - obj.setSkillCommandEnable(83, false) ★——**变身期间封掉技能 83（技能栏"替换"的实现：不是换栏位，是开关技能可用性）**；
  - 监视 buff：ap_avatar_icon 消失（倒计时结束）且处于 STAND/ATTACK/DASH → sq_AddSetStatePacket(STATE_MAGE_RETURNTOBM, ...) 强制变回；
  - sq_SetCustomDamageType(obj, true, 1) ★ 持续改伤害类型；sq_IsInBattle() 为假立即失效（出图自动解除）。
  - onStart/onEnd 管理 sq_AddOcularSpectrum ★（残影滤镜）与循环音效 sq_PlaySound("…", SOUND_ID)/stopSound；drawAppend 钩子自绘光环（appendage 的**渲染钩子**，跟宿主绘制管线走 x/y/isFlip）。
- **变身态专属动作**：setCurrentAnimation_mage_avatar(obj, name) 在别的技能脚本里被调用来替换动画（变身换全套动作资源的落点）。

### C 小结：召唤/变身的实体生成模式
- 召唤 = **引擎 Monster（ActiveObject）+ 标记 PO 握手 + appendage 寄生 AI**：SummonMonsterPacket（等级/队伍/寿命一站式）→ PO 轮询回执拿 group/uniqueId → onChangeSkillEffect 接生（挂 AI appendage + setMapFollowParent 跟随）→ AI 用 timer_vector + sq_FindTarget + sq_AddSetStatePacketActiveObject 驱动，伤害倍率每击实时取主人面板 → appendage 失效/主人死亡 = sendDestroyPacket 自毁。
- 变身 = **双 appendage 架构**：icon appendage（带 sq_SetValidTime，管倒计时+ChangeStatus 属性）+ 形态 appendage（管技能禁用/伤害类型/演出，���视前者到期强制还原）。"技能替换"不换 UI 栏位，用 setSkillCommandEnable(false) 屏蔽 + 专属动画映射函数。
- 单键多行为：checkExecutable 按"目标实体是否存在"分流 substate（召唤/召回/拒绝三态）。

**文件清单（C）**：
- sqr/character/mage/summonkruta/{summonkruta,ap_summonkruta,repeatedsmashofrage}.nut
- sqr/character/mage/mage_common.nut（onChangeSkillEffect_Mage 接生逻辑，L983+）
- sqr/common_object/share_obj/mage/procappend.nut（标记 PO 握手）
- sqr/common_object/run_script/{my_monster_object,my_passive_object}.nut（找自己召唤物/PO 的工具函数）
- sqr/character/mage/avatar/{avatar,ap_avatar}.nut（变身）
- 注册：mage_load_state.nut

## D.（顺手）浮空参数实战值

- **受击反应四件套**（attackInfo 上配置，碰撞结算时引擎读取）：
  - sq_SetCurrentAttacknUpForce(attackInfo, n) ★ 浮空力；sq_SetCurrentAttacknBackForce(attackInfo, n) ★ 击退力；sq_SetCurrentAttackeHitStunTime(attackInfo, ms) ★ 硬直；sq_SetCurrentAttackeDamageAct(attackInfo, n) ★ 受击表现（DAMAGEACT_NONE=0 / DAMAGE=1 / DOWN=2 / DAMAGE_EXCEPT_HUMAN=3，dnf_enum_header.nut L974-977）。
  - 实战数值（atgunner/killpoint.nut L351-354、suppressingfire.nut L138-140）：**普通连段命中 upForce=10, backForce=3（轻硬直）；终结段 DamageAct=2(DOWN), upForce=300, backForce=300（大浮空击飞）**。多段持续 PO 常配 sq_SetCurrentAttackeHitStunTime(attackInfo, 0)（po_bloodcastle_*.nut）防止每跳都硬直。
  - 可读取回滚：sq_GetCurrentAttackeDamageAct(attackInfo) 先存后改、onAfterDamage 还原（atmage/firepillar.nut L351-381，施法中把来犯攻击的 DamageAct 临时置 NONE = 变相霸体）。
- **本体上挑类**（剑魂上挑/格斗上勾拳）是引擎老技能，upForce 存 .atk 二进制，.nut 不出现；脚本侧启动器看 atmage/windstrike/wind_strike.nut：upForce = obj.sq_GetLevelData(2)（技能表列），sq_SetCurrentAttacknUpForce(attackInfo, upForce) 与 power/bonusRate 同点配置。
- **程序化抛体轨迹**（自己算 Z）：上升 sq_GetAccel(0, 270, t, 500, true)（加速到 270px/500ms）；下落 sq_GetUniformVelocity(zTop, 0, t, 800)（匀速 800ms 砸地）；投出目标 = STATE_DOWN intVector（反向, 0, 1, 抛力900/1600, 100）。

## 共性总结（对 ET9 移植最有用的几条）

1. **"对象 + 包 + 钩子"三件套**：一切远程/异步行为 = Write* 序列化 → Send*Packet（Create/Destroy/ChangeSkillEffect/SetState/HitObject/Message）→ 对端具名钩子（setCustomData_po_* / onChangeSkillEffect_* / onChangeState_appendage_*）读包执行。且**只有 isMyControlObject() 端发权威包**——这天然就是帧同步的"指令-表现"分层（PO 创建参数包 ≈ 一次广播指令）。
2. **appendage 是万能副作用容器**：debuff/控制（hold 锁）、召唤 AI、变身形态、buff 属性（ChangeStatus 通道）全是 appendage；parent(宿主)/source(施与者) 双向引用 + sq_SetValidTime 时限 + proc/onChangeState/onSourceKeyFrameFlag/onEnd/drawAppend 钩子 + 自带 timer_vector。移植时值得做一个等价 ECS 组件。
3. **状态机分型**：老技能=引擎内置状态+脚本关键帧钩子；新技能=纯脚本子状态机（setSkillSubState + onSetState switch + onEndCurrentAni/onProc/onKeyFrameFlag 推进，sq_GetStateTimer() 计时，结束 AddSetStatePacket(STATE_STAND,...) 归位）。**动画关键帧 flag 是最重要的节拍器**（出伤害窗/特效/受害者挂点全靠它）。
4. **伤害结算与受击表现解耦**：伤害=attackInfo 数据（power/bonusRate/element/异常状态注入/受击四件套）随碰撞走；脚本 onAttack 只做"追加"（控制/特效/挂 debuff/登记抓取）。抓取与投掷则完全绕过 damageAct，用状态指令直接操纵受害者（STATE_HOLD / STATE_DOWN(intVector)）。
5. **对象寻址三元组**：sq_GetGroup(obj)+sq_GetUniqueId(obj) → sq_GetObject(mgr, g, u)，配合 getCollisionObjectIndex()（资源 ID）与 isEnemy()（阵营判定）——跨实体引用全部走它，绝不经手裸指针/名字。
6. **清理责任到钩子**：pooledObj 在 destroy_po_* 回收；appendage 在 onEnd 复原姿态（rotate/damageType/spectrum）+ 防卡墙挪位；召唤物在 appendage 失效时 sendDestroyPacket。任何"给对象挂的东西"都有对应卸载钩子——帧同步下泄漏=不同步，这一点要当硬规范。

**说明**：原任务 B 提的 suplex(state 23) 与知识库"onAttack 返回受击对象"的引擎内建抓取未在 .nut 层出现（C++ 实现，squirrel 层等价物即 B 节的 obj_vector+appendage 模式）；所选三个新原型文件中 hurricanesupersuplex/elbowthrow 局部变量名被官方混淆（逻辑与 API 完全可读），summonkruta/avatar/grabhand/wavecut 全部原生可读。
