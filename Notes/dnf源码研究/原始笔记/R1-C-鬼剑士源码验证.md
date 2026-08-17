# R1-C：DNF 技能脚本源码验证笔记（男鬼剑 swordman / 男法师 mage）

> 第1轮 Agent C 原始笔记。任务：拿真实 .nut 源码验证知识库（二手资料）。
> 验证环境：`pvf/sqr/`。所有 .nut 均为**明文 Squirrel 源码**（od -c 抽查确认）。
> 已综合进：01-技能生命周期与状态系统-总结.md

---

## 一、目录探查

`pvf/sqr/character/` 下实际存在：**swordman（男鬼剑）+ swordman_load_state.nut**、**mage（男法师）+ mage_load_state.nut**（无需用女版替代）；另有 atswordman/atmage/atswordman_3rd、fighter/atfighter、gunner/atgunner、priest/atpriest、thief、new_mage/new_atmage、demonicswordman（黑暗武士）、creatormage（缔造者）、jg_swordman（剑鬼）。

加载链（`sqr/loadstate.nut`）：
```squirrel
sq_RunScript("dnf_enum_header.nut");
sq_RunScript("common.nut");
...
sq_RunScript("Character/swordman_load_state.nut");
sq_RunScript("Character/mage_load_state.nut");
```
`dnf_enum_header.nut:1208` 定义 `ENUM_CHARACTERJOB_SWORDMAN <- 0`。

## 二、load_state.nut 结构（swordman_load_state.nut，173 行）

文件本体不含任何技能逻辑，只做四类注册：
1. `IRDSQRCharacter.pushScriptFiles(path)` —— 加载 header/common/被动技能脚本
2. `IRDSQRCharacter.pushPassiveObj(path, 对象ID)` —— 注册被动物体
3. `sq_RunScript(path)` —— 拉起共享被动物体的分发脚本
4. `IRDSQRCharacter.pushState(职业枚举, 脚本路径, 状态名, 状态ID, 技能ID)` —— 状态机注册，主体

`pushState` 五参数的真实含义（用例反推，交叉验证多处）：
```squirrel
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/outbreak/outbreak.nut", "OutBreak", 45, 81);
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/bloodboom/bloodboom.nut", "swordman_bloodboom", 229, 229);
IRDSQRCharacter.pushState(0, "character/swordman/wave/wave.nut", "WaveSword", 24, -1);
```
- 第 4 参是**状态 ID**（脚本里 `sq_AddSetStatePacket(45, ...)` 用数字引用），第 5 参是**技能 ID**（-1 表示非技能状态）。
- **状态 ID 和技能 ID 是两套编号**（OutBreak：state 45 / skill 81）。
- 存在**同名重复注册**（rapidmoveslash 两次：`(39,-1)` 与 `(39,72)`）——同一状态绑定不同技能 ID 各注册一次，优先级规则**待确认**。
- `mage_load_state.nut`（112 行）结构完全同构。

## 三、技能深读

### 技能 A：三段斩·剑影版（tripleslashbs）

文件：`sqr/character/jg_swordman/swordghost_effect/tripleslash.nut`（249 行，明文干净版）。���一文件注册了**两个状态**（`tripleslash_swordman` 和 `tripleslashbs`），是"一个文件多状态"的实例。

**1. 函数结构**

| 函数 | 职责 |
|---|---|
| `onAfterSetState_tripleslash_swordman` | 原版三段斩（状态22）切入后，若 `sq_GetSkillLevel(123)>0`（学了鬼剑术被动）则转发进剑影版状态 |
| `onSetState_tripleslashbs` | 每段进入：设动画/攻击信息/伤害；记段数；算位移目标；按左右键改朝向；锁攻速 |
| `onEndCurrentAni_tripleslashbs` | 当前段动画播完 → 回 STATE_STAND（各分支全一样，冗余写法） |
| `onProcCon_tripleslashbs` | 条件逻辑：帧号门槛后开"追加输入窗"，检测再按技能键进下一段 |
| `onProc_tripleslashbs` | 每帧位移：匀速插值 + 碰撞安全移动；调特效 |
| `onEndState_tripleslashbs` | 离开本状态时恢复移动参数、启动技能冷却 |

**2. 执行时序的关键 API 序列**

```
按键 → 引擎调 checkExecutableSkill_*
  → sq_AddSetStatePacket(state, priority, true)   // 切状态走"网络包"，非本地直接调用
onSetState：
  obj.setSkillSubState(subState)                  // 官方子状态���制
  obj.sq_StopMove()
  obj.sq_SetCurrentAnimation(CUSTOM_ANI_TRIPLESLASH_BLADESPIRIT1)   // 常量=动画表索引
  obj.sq_SetCurrentAttackInfo(CUSTOM_ATTACK_INFO_TRIPLESLASH_BLADESPIRIT1)
  obj.sq_GetBonusRateWithPassive(8,-1,0,1.0) → obj.sq_SetCurrentAttackBonusRate(damage)  // 百分比伤害
  obj.sq_GetPowerWithPassive(8,-1,3,-1,1.0) → obj.sq_SetCurrentAttackPower(damageBonus)  // 固定伤害
  obj.sq_SetStaticSpeedInfo(SPEED_TYPE_ATTACK_SPEED, ..., 1.0, 1.0)  // 锁定攻速类型
每帧：
  onProcCon（帧判断+输入窗） / onProc（位移）
  命中由引擎按 attackInfo 的逐帧攻击盒自动完成 → 命中回调 onAttack_<state>(obj, damager, boundingBox, isStuck)
onEndCurrentAni → 回 STAND
```

伤害取值不在脚本里写死，而是 `sq_GetBonusRateWithPassive(技能ID8, -1, 数据索引, 倍率)` 按（技能ID，数据索引）从技能数值表取——第 2、4 参 `-1` 的确切语义**待确认**（推断为"无被动技能来源/默认"）。

**3. "第几帧"判定的真实写法（两种）**

a) **轮询帧号**（tripleslash.nut:139-147）：
```squirrel
local pAni = obj.getCurrentAnimation();
local frmIndex = pAni.GetCurrentFrameIndex();
if (frmIndex >= 3) {           // 低级版：第3帧起开放连段输入
    obj.setSkillCommandEnable(8, true);
    local tripleSlash = obj.sq_IsEnterSkill(8);
```
等价 API 三个变体并存：`pAni.GetCurrentFrameIndex()`、`obj.sq_GetCurrentFrameIndex(pAni)`（outbreak.nut:170）、`sq_GetAnimationFrameIndex(pAni)`（attack.nut:9）。

b) **时间-帧换算**（tripleslash.nut:203-209）：
```squirrel
local currentT = sq_GetCurrentTime(pAni);
local xAccel = sq_GetUniformVelocity(sq_GetXPos(obj), xDistance, currentT, 200);  // 200ms内匀速到目标x
sq_MoveToNearMovablePos(obj, xAccel, sq_GetYPos(obj), 0, ..., 20, -1, 3);        // 带碰撞检测的移动
```

**连段输入窗机制**：`setSkillCommandEnable(8, true)` 开窗 → `sq_IsEnterSkill(8)` 检测技能键再按 → 满足则 `sq_IntVectClear(); sq_IntVectPush(substateV); sq_AddSetStatePacket(...)` 带参切下一段。方向修正直接读键盘：`sq_IsKeyDown(OPTION_HOTKEY_MOVE_LEFT, ENUM_SUBKEY_TYPE_ALL)` → `obj.sq_SetDirection(ENUM_DIRECTION_LEFT)`。

**4. 无状态 vs 有状态**
脚本函数本身**完全无状态**（引擎每次回调传入 obj），状态挂两处：
- 官方机制：`obj.setSkillSubState(n)` / `obj.getSkillSubState()`
- 通用变量槽：`obj.getVar("slashcount").setInt(0,...)`、`obj.getVar("slashmove").clear_vector()/push_vector(x)`——`getVar(名字)` 得到命名容器，内可放 vector / obj_vector / timer_vector / AnimationMap

**5. 实战细节（知识库难覆盖）**
- 冷却启动写在 `onEndState` 而非施放时：`obj.startSkillCoolTime(8, 1, -1)`，且用 `newState != STATE_TRIPLESLASH_BLADESPIRIT` 保证"真离开本技能"才计时。
- `onEndCurrentAni` 开头必查 `if (!obj.isMyControlObject()) return;`——非属主客户端直接跳过切状态逻辑（多人同步：属主端发状态包，其余端只播表现）。
- 五个子状态的 `onEndCurrentAni`/`onProc` 分支代码完全复制粘贴——官方脚本工程化程度低，读时别假设有抽象。

### 技能 B：血气爆发 OutBreak（+ bloodboom 佐证）

文件：`sqr/character/swordman/outbreak/outbreak.nut`（244 行）。

**1. 函数结构**

| 函数 | 职责 |
|---|---|
| 文件头 `ENUM_OUTRAGEBREAK_STATE_DROP_FRAME <- 5` | 全局常量：下落判定帧号 |
| `checkExecutableSkill_OutRageBreak(obj)` | 可施放检查钩子：`sq_IsUseSkill(81)` 判按键；若正处 233/232 状态（其他血气技中）改为瞬发 `sq_SendCreatePassiveObjectPacket(20044,23,100,1,0)` |
| `checkCommandEnable_OutRageBreak(obj)` | 命令可用性（直接 return true） |
| `onSetState_OutBreak` | 子状态0：扣血；子状态1：叠图层动画；子状态10：按方向键组合算冲刺距离向量 |
| `onAfterSetState_OutBreak` | 手动把子状态 push 进 getVar()（老式子状态管理）；state==1 时跳帧 |
| `onKeyFrameFlag_OutBreak` | 关键帧标记回调：state 0 的标记帧 → 发包切子状态10 |
| `onProc_OutBreak` | 子状态10 前 5 帧抛物线冲���；frmIndex >= 5 后切子状态1 |
| `getQuadraticFunction(obj,x,b,c)` | 自写二次函数算 Z 轴跳跃高度 |

**2. 关键机制实录**
- **HP 消耗**（血气系真实写法，outbreak.nut:73-74）：
```squirrel
local SKILLHP = sq_GetLevelData(obj,81, 3, sq_GetSkillLevel(obj, 81))
obj.setHp(obj.getHp() - SKILLHP, null, true)
```
- **施放中再按变瞬发**：检查当前 `obj.sq_GetState()`，处于特定技能状态时不再切状态而是直接生成被动物体——技能间联动的真实做法。
- **关键帧标记回调**（第三种帧机制，引擎在 .ani 中标记的帧触发）：
```squirrel
function onKeyFrameFlag_OutBreak(obj,flagIndex) {
    local state = obj.getVar().get_vector(0);
    if (state == 0) { ... obj.sq_AddSetStatePacket(45, STATE_PRIORITY_USER, true); }
```
标记的具体含义由 flagIndex 区分（bloodboom.nut:53 `if (v7cJR8fQve0 == 1)` 在标记1处：发被动物体创建包 + `sq_SetMyShake(obj, 8, 300)` 震屏 + `sq_flashScreen(...)` 闪屏 + 地面血泊特效）。标记数据本身编在二进制 .ani 里，脚本侧只收 flagIndex。
- **跳帧播放**：`obj.sq_SetCurrentTimeByFrame(animation, ENUM_OUTRAGEBREAK_STATE_DROP_FRAME)`——把动画时间直接设到第 5 帧，配合 `pAni.getDelaySum(0, 5)`（帧区间总时长）做时间归一。
- **撞墙缩短冲刺**（outbreak.nut:211-225）：`obj.isMovablePos(dstX, ...)` 失败时，把本次越过的 offset 从剩余总距离 totalLen 里扣掉——冲刺技能遇墙"吃掉剩余距离"的真实实现。
- **被动物体跨端传参**（bloodboom.nut:56-74，最典型的模式）：
```squirrel
obj.sq_StartWrite();
obj.sq_WriteDword(229);
obj.sq_WriteDword(obj.sq_GetIntData(229, 1));
...（8个参数）
obj.sq_SendCreatePassiveObjectPacket(24370, 0, 0, 0, 60);   // 尾参是 x,y,z 偏移
```
对端在 `setCustomData_po_*(obj, receiveData)` 里 `receiveData.readDword()` **按写入顺序**读回——"技能参数从角色端序列化传给被动物体"的网络同步机制。
- **特效创建**（bloodboom.nut:80-88）：`sq_CreateAnimation("", "path.ani")`（首参空串）→ `addLayerAnimation(1, 子动画, true)` 叠层 → `sq_CreatePooledObject(ani, true)`（对象池）→ `setCurrentPos/setCurrentDirection` → `sq_SetEnumDrawLayer(pooledObj, ENUM_DRAWLAYER_BOTTOM)` → `sq_AddObject(obj, pooledObj, OBJECTTYPE_DRAWONLY, false)`。
- **appendage（buff/附加体）**：`CNSquirrelAppendage.sq_AppendAppendage(damager, obj, 229, true, ".../ap_bloodboom.nut", true)`；ap 脚本内 `appendage.sq_AddFunctionName("proc","proc_appendage_OutBreak")` 显式注册回调；`ap_outbreak.nut:25` 的 proc 里 `parentObj.getState() != 45` 即 `appendage.setValid(false)` 自杀——附加体生命周期绑定施���者状态。

## 四、对照纠偏：真实代码 vs "知识库风格 API"

**一致的部分**
- `sq_` 前缀确实是大头，但实际是**两套并存**：全局函数 `sq_GetSkillLevel / sq_IsKeyDown / sq_GetUniformVelocity / sq_GetDistancePos / sq_MoveToNearMovablePos / sq_SetCurrentAttackInfo / sq_CreatePooledObject...`，以及绑定在对象上的原生方法 `obj.sq_XXX`：`obj.sq_SetCurrentAnimation / sq_AddSetStatePacket / sq_GetVectorData / sq_IsEnterSkill / sq_GetBonusRateWithPassive / sq_SetStaticSpeedInfo...`。知识库若只写全局函数会漏掉 obj 方法这一半。
- `on*/setState` 命名风格确认，后缀必须等于 pushState 注册的状态名。另有 `checkExecutableSkill_<状态名>` / `checkCommandEnable_<状态名>`——**grep 全库无任何脚本调用方，确认为引擎按函数名约定直接调用的钩子**。

**知识库不太可能覆盖的真实模式**
1. **切状态走网络包**：一切状态迁移用 `obj.sq_AddSetStatePacket(stateID, STATE_PRIORITY_*, bool)`，且回调里大量 `if (!obj.isMyControlObject()) return;`——"逻辑只在属主端跑、表现全端跑"。
2. **命中判定本体在引擎**：脚本只做 `sq_SetCurrentAttackInfo(CUSTOM_ATTACK_INFO_常量)` + 设伤害，攻击盒按帧配置在 attackInfo 数据里，引擎逐帧扫盒，命中才回调 `onAttack_`。技能脚本里看不到判定循环。
3. **常量索引表**：`CUSTOM_ANI_TRIPLESLASH_BLADESPIRIT1 <- 262`（swordman_header.nut:432）、`CUSTOM_ATTACK_INFO_TRIPLESLASH_BLADESPIRIT1 <- 142`（:533）——动画/攻击信息用纯数字索引引用角色资产清单，脚本通常不写 ani 路径；只有特效才写明文路径。
4. **伤害双轨**：`sq_SetCurrentAttackBonusRate`（百分比）与 `sq_SetCurrentAttackPower`（固定值）总是成对设置；数值经 `sq_GetBonusRateWithPassive / sq_GetPowerWithPassive / sq_GetLevelData / sq_GetIntData` 从技能表按（技能ID，索引）现取。
5. **技能 ID 全程硬编码数字**：`sq_IsEnterSkill(8)`、`sq_GetSkillLevel(123)`、`sq_IsUseSkill(81)`——可读性差但真实。
6. **本源码包混有私服改档**：`common_object/share_obj/swordman/*.nut` 函数名带 QQ 号签名、局部变量混淆（`QqpuHn5C49sl`）；而 `tripleslash.nut`、`outbreak.nut`、`bloodboom.nut`（bloodboom 局部变量被换名但函数名干净）是可读版。
7. **被动物体钩子绑定约定**：`<hook>_po_<注册文件名去 po_ 前缀>`——`po_wavecut.nut` → `onAttack_po_wavecut`。
8. **同文件多状态、多技能共用一状态文件**：tripleslash.nut 注册两个状态；hardattack.nut 注册两个。
9. 定时器双轨：`obj.setTimeEvent(id, interval, count, repeat)` → `onTimeEvent_` 回调；或 `getVar().push_timer_vector()` 手动管理。

**明确标"待确认"项**：`sq_GetBonusRateWithPassive` 第 2 参与 `sq_GetPowerWithPassive` 第 4 参的 `-1` 语义；pushState 重复注册同名状态的覆盖规则；空 share_po 文件与混淆钩子名的绑定机制（疑为私服引擎改动）；`sq_SetMoveDirection(dir, ENUM_DIRECTION_NEUTRAL)` 第 2 参作用。

## 核心文件路径索引
- 总入口 `pvf/sqr/loadstate.nut`
- `pvf/sqr/character/swordman_load_state.nut` / `mage_load_state.nut`
- `pvf/sqr/character/jg_swordman/swordghost_effect/tripleslash.nut`
- `pvf/sqr/character/swordman/outbreak/outbreak.nut` + `ap_outbreak.nut`
- `pvf/sqr/character/swordman/bloodboom/bloodboom.nut`
- `pvf/sqr/character/swordman/attack/attack.nut`（普攻连击/onAttack 实例）
- `pvf/sqr/character/swordman/swordman_header.nut`（CUSTOM_ANI/CUSTOM_ATTACK_INFO 常量表）
- `pvf/sqr/shared_passive_object/po_swordman_shared.nut` + `swordman/*.nut`（PO hook 拆分模板）
- `pvf/sqr/common_object/share_obj/swordman/*.nut`（私服混淆版，引用需谨慎）
