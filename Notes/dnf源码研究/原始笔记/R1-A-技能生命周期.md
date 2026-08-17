# R1-A：DNF 技能系统提取笔记（来源：nut知识库 01/02 两篇文档）

> 第1轮 Agent A 原始笔记。任务：读知识库《01-角色事件函数》+《02-技能系统》。
> 出处标记：〔01〕= 01-角色事件函数.md；〔02〕= 02-技能系统.md。函数签名照抄文档；文档未解释的参数只列名不臆测。
> 已综合进：01-技能生命周期与状态系统-总结.md

---

## 一、角色/技能生命周期事件函数全表

### 1.1 技能施放入口（〔02〕"技能使用相关"/"Buff 技能开发"/"完整技能示例"）

| 函数签名（照抄） | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `useSkill_before_职业名(obj, skillIndex, consumeMp, consumeItem, oldSkillMpRate)` | "在技能使用前执行" | "可以修改技能消耗等"，例：算出新 MP 消耗率后 `obj.setSkillMpRate(skillIndex, newMpRate)` | `oldSkillMpRate` 为原始 MP 消耗率（示例用它恢复原值）；其余参数文档未逐个解释。返回 `true` |
| `useSkill_after_职业名(obj, skillIndex, consumeMp, consumeItem, oldSkillMpRate)` | "使用技能后" | "恢复原始MP消耗率"：`obj.setSkillMpRate(skillIndex, oldSkillMpRate.tofloat())` | 同上 |
| `checkExecutableSkill_*(obj)`（例 `checkExecutableSkill_BloodBoom(obj)`、`checkExecutableSkill_BuffZy(obj)`） | 技能可执行性检查（文档未写明由谁在何时调用） | 调 `obj.sq_IsUseSkill(SKILL_ID)` 判定可用（该调用使技能进 CD），然后 `obj.sq_AddSetStatePacket(STATE_XXX, STATE_PRIORITY_USER, false)` 切状态并返回 `true` | `obj`：角色对象 |
| `checkCommandEnable_*(obj)`（例 `checkCommandEnable_BloodBoom(obj)`） | 技能按键开关（文档未写明调用时机） | 取 `obj.sq_GetState()` 判断当前状态（如 `STATE_STAND`）下能否出招，返回 true/false | 同上 |

### 1.2 状态机事件（〔01〕"状态设置相关"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `onSetState_技能名(obj, state, datas, isResetTimer)` | "设定状态时执行的函数。`obj.sq_AddSetStatePacket` 函数执行后会执行这个函数" | 进入状态时的初始化：`sq_StopMove()`、`sq_SetCurrentAnimation()`、`sq_SetStaticSpeedInfo()`（〔02〕血爆/Buff 示例）；例中 `if(state == STATE_DIE) obj.sq_RemoveSkillLoad(SKILL_ID)` | `state`：状态编号；`datas`：随状态包传的数据；`isResetTimer`：文档未解释 |
| `onAfterSetState_技能名(obj, state, datas, isResetTimer)` | "设置状态后" | "状态设置后的处理" | 同上 |
| `onEndState_技能名(obj, new_state)` | "状态结束时" | 清理；示例用 `if(new_state != STATE_XXX)` 判断是否真是离开本状态 | `new_state`：即将切入的新状态 |
| `addSetStatePacket_职业名(obj, state, datas)` | "增加设置状态包时" | "可以驳回状态，且可以再次设置别的状态"；`return -1` 表示驳回状态 | 同 onSetState 前两参 |

### 1.3 状态持续/每帧处理（〔01〕"状态持续处理"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `onProc_技能名(obj)` | "在人物处于技能状态过程中会自动执行"（"这是很重要的函数"）；标题注明"**组队双方运行**" | 帧轮询判定/创建特效（血爆示例在第 20 帧创建 PO） | `obj` |
| `onProcCon_技能名(obj)` | "状态持续过程中执行"，标题注明"**仅自己运行**" | "仅自己运行的处理逻辑" | `obj` |
| `procAppend_职业名(obj)` | "被动处理" | 被动处理逻辑；示例 `return 1`（也见 `return 0`） | `obj` |
| `procDash_职业名(obj)` | "前冲被动" | 前冲时的处理 | `obj` |
| `procSkill_职业名(obj)` | "被动技能处理" | 被动技能处理逻辑 | `obj` |

### 1.4 被动技能（〔01〕"被动技能相关"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `ProcPassiveSkill_职业名(obj, skill_index, skill_level)` | "当角色学习被动技能时触发" | `skill_level > 0` 时 `CNSquirrelAppendage.sq_AppendAppendage(obj, obj, skill_index, false, "路径/ap_xxx.nut", true)` 创建附加对象；返回 `true` | `skill_index`/`skill_level`：技能编号/等级 |
| `onUseSkillPassiveSkill_职业名(obj, skillIndex, skillLevel)` | "使用被动技能时" | "处理逻辑"（文档未展开） | 同上 |

### 1.5 攻击事件（〔01〕"攻击相关事件"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `onAttack_技能名(obj, damager, boundingBox)` | "攻击到时"——"攻击敌人后执行的函数" | "可以返回受到伤害的对象。**这是写抓取技能的基础**" | `damager`："受到攻击的对象"；`boundingBox`："攻击框" |
| `onBeforeAttack_技能名(obj, damager, boundingBox)` | "攻击前" | "攻击前的处理" | 同上 |
| `onAfterAttack_技能名(obj, damager, boundingBox)` | "攻击后" | "攻击后的处理"；示例 `return 1`（也见 `return 0`） | 同上 |

### 1.6 动画事件（〔01〕"动画相关"；〔02〕血爆示例）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `onEndCurrentAni_技能名(obj)` | "当前动画结束时" | 技能收尾：重置标记、`obj.sq_AddSetStatePacket(STATE_STAND, STATE_PRIORITY_USER, false)` 回站立；Buff 技能在此时创建 Appendage | `obj` |

### 1.7 地图/副本生命周期（〔01〕"地图相关事件"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `onStartDungeon_职业名(obj)` | "进入副本时" | 初始化（示例内用 `sq_IsMyCharacter(obj)` 判断是否本人角色） | `obj` |
| `onStartMap_职业名(obj)` | "进入 map 房间时" | 进房间处理 | `obj` |
| `onEndMap_技能名(obj)` | "结束当前 map 时" | 清理（示例内用 `obj.sq_IsMyControlObject()` 判断） | `obj` |

### 1.8 定时事件（〔01〕"定时事件"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `onTimeEvent_技能名(obj, timeEventIndex, timeEventCount)` | "定时时钟事件" | 按 `timeEventIndex` 分发处理。返回值文档有两处描述：正文"返回 `true` 运行成功，返回 `false` 运行不成功会多运行几次"；代码注释"`return true; // true 表示回调中断`"（两说矛盾） | `timeEventIndex`/`timeEventCount`：定时器索引/计数（文档未细释）。定时器的注册方式文档未讲 |

### 1.9 技能效果/控制/重置（〔01〕"技能效果相关"/"技能控制相关"/"重置相关"；〔02〕"技能控制"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `onChangeSkillEffect_职业名(obj, skillIndex, reciveData)` | "接收技能效果包"（两文件均有此函数） | 从包读数据：`local data = reciveData.readDword();`；〔02〕示例中调 `skill.resetCurrentCoolTime()`、`skill.setSealActiveFunction(true)` | `reciveData`：可 `readDword()` 的数据包 |
| `flushCommandEnable_职业名(obj)` | "刷新技能按键状态时" | "这里可以设置技能亮不亮"：`sq_SetAllCommandEnable(obj, false)` / `obj.setSkillCommandEnable(SKILL_ID, true)`；返回 `S_FLOW_NORMAL` 或 `S_FLOW_RETURN` | `obj` |
| `setEnableCancelSkill_职业名(obj, isEnable)` | 标题："普攻强制施放技能"（具体时机未讲） | `obj.setSkillCommandEnable(SKILL_ID, isEnable)`；示例先判 `obj.isMyControlObject()` | `isEnable`：启用与否 |
| `reset_职业名(obj)` | "死亡复活、进入下一个房间时执行" | 重置清理 | `obj` |
| `resetDungeonStart_职业名(obj, moduleType, resetReason, isDeadTower, isResetSkillUserCount)` | "副本中重置状态时。城镇中也会运行，相当于进入副本回到城镇都会运行" | 重置处理；示例返回 `1`（也见 `return -1`） | 四个参数仅列名，文档未解释含义 |

### 1.10 Throw/道具（〔02〕"Throw 状态相关"/"道具使用"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `changeThrowState_职业名(obj, throwState)` | "得到13状态下的子状态"（STATE_THROW=13） | `throwState == 3 && obj.getThrowIndex() == SKILL_ID` 时：`obj.sq_IntVectClear(); obj.sq_IntVectPush(throwState); obj.sq_AddSetStatePacket(STATE_THROW, STATE_PRIORITY_USER, true)` | `throwState`：投掷子状态编号 |
| `isUsableItem_职业名(obj, itemIndex)` | "是否可以使用道具" | 挂了某 AP 时禁用恢复药（`sq_IsItemRecover(itemIndex)`）或指定道具 | `itemIndex`：道具编号 |

### 1.11 被动物体（PO/特效）事件（〔02〕"特效文件（po_BloodBoom.nut）"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `setCustomData_po_BloodBoomFinish(obj, reciveData)` | PO 被创建、收到自定义数据包时（"接收自定义数据（函数名po_BloodBoomFinish必须与特效列表中的名称一致）"） | `reciveData.readDword()` 读伤害 → `sq_SetCurrentAttackPower(attackinfo, damage)` → `obj.isMyControlObject()` 时用全局 IntVector 发状态包 | `reciveData`：数据包 |
| `setState_po_BloodBoomFinish(obj, state, datas)` | PO 状态设置时 | 按 `state` 设子状态与动画（`obj.getCustomAnimation(0)`） | `state`/`datas` |
| `onEndCurrentAni_po_BloodBoomFinish(obj)` | PO 动画结束 | `sq_SendDestroyPacketPassiveObject(obj)` 销毁特效 | `obj` |

### 1.12 Appendage（附加对象/Buff 载体）事件（〔02〕"Buff 技能 AP 文件结构"）

| 函数签名 | 触发时机 | 典型用途 | 参数含义 |
|---|---|---|---|
| `sq_AddFunctionName(appendage)` | AP 文件加载/注册时（文档未明说时机） | `appendage.sq_AddFunctionName("onStart", "onStart_appendage_BuffZy")` 注册事件回调 | `appendage` |
| `onStart_appendage_BuffZy(appendage)` | 注册名 "onStart"（AP 生效开始） | `appendage.getParent()` 取宿主 → 读 `getVar("buffdata")` 向量 → `sq_getChangeStatus`/`sq_AddChangeStatusAppendageID` 修改属性 | `appendage` |

---

## 二、技能从触发到结束的完整流程

文档没有一处把全流程按时序串讲；以下流程由〔02〕"完整技能示例：血爆技能" + 两文件散落的机制拼装，每步标注依据。

1. **按键检测/图标可用性**
   - 图标亮灭：`flushCommandEnable_*`（非战斗时 `sq_SetAllCommandEnable(obj, false)`）〔01/02〕
   - 按键开关：`checkCommandEnable_*(obj)` 按 `obj.sq_GetState()`（如 `STATE_STAND`）判定当前状态可否出招〔02〕
   - 脚本内判键：`if(obj.sq_IsEnterSkill(SKILL_ID) != -1)` "判断是否按下技能键"〔02〕
2. **可执行性检查 + 消耗判定 + 进 CD**
   - `checkExecutableSkill_*(obj)` 调 `obj.sq_IsUseSkill(SKILL_ID)`；文档强调："**`sq_IsUseSkill` 使用后会使技能进入CD，没有这个函数技能无CD**"〔02〕
   - 消耗修改窗口：`useSkill_before_*` → `useSkill_after_*`〔02〕
   - 手动 CD 控制：`obj.startSkillCoolTime(SKILL_ID, skillLevel, -1)`；查询 `skill.isInCoolTime()`；重置 `skill.resetCurrentCoolTime()`〔02〕
3. **状态切换（走网络包）**
   - `obj.sq_AddSetStatePacket(STATE_BLOODBOOM, STATE_PRIORITY_USER, false)`；中途可被 `addSetStatePacket_*` 驳回（返回 -1）或改设其他状态〔01〕；随后 `onAfterSetState_*`〔01〕。技能状态需先注册：`IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "…/BloodBoom.nut", "BloodBoom", STATE_BLOODBOOM, SKILL_BLOODBOOM)`〔02〕
4. **动画与速度设置（onSetState 内）**
   - `obj.sq_StopMove()` → `obj.sq_SetCurrentAnimation(CUSTOM_ANI_STAGE_BLOODBOOM)` → `obj.sq_SetStaticSpeedInfo(SPEED_TYPE_CAST_SPEED, …)`（施法速度影响动画）〔02〕
5. **判定帧（onProc 每帧轮询动画帧号）**
   - `onProc_*`：`local currentAni = obj.getCurrentAnimation(); local currentAniIndex = currentAni.GetCurrentFrameIndex();`，`if(currentAniIndex == 20 && Hit == 0)` 时才放特效；用 `obj.getVar().setInt(FLAG_HIT, Hit + 1)` 防重复〔02〕
6. **伤害计算与传递**
   - 算伤：`obj.sq_GetBonusRateWithPassive(SKILL_ID, STATE_ID, 0, 1.0)`（百分比）/ `obj.sq_GetPowerWithPassive(SKILL_ID, STATE_ID, 1, 0, 1.0)`（固伤，参数4"如果只有固伤写-1"）〔02〕
   - 本体攻击设伤：`obj.sq_SetCurrentAttackBonusRate(rate)` / `obj.sq_SetCurrentAttackPower(power)`〔02〕
   - 经 PO（被动物体）结算的路线（血爆）：`obj.sq_StartWrite(); obj.sq_WriteDword(power);` 写包 → `obj.sq_SendCreatePassiveObjectPacket(24320, 0, 0, 0, z)` 创建 PO → PO 端 `setCustomData_po_*` 里 `reciveData.readDword()` 读伤 → `sq_GetCurrentAttackInfo(obj)` + `sq_SetCurrentAttackPower(attackinfo, damage)` 设伤〔02〕
7. **命中回调**
   - `onBeforeAttack_*` → `onAttack_*(obj, damager, boundingBox)` → `onAfterAttack_*`〔01〕
8. **结束**
   - `onEndCurrentAni_*`：重置标记，`obj.sq_AddSetStatePacket(STATE_STAND, STATE_PRIORITY_USER, false)` 回站立〔02〕；离开状态时 `onEndState_*(obj, new_state)` 清理〔01〕
9. **取消/打断相关**
   - `setEnableCancelSkill_*(obj, isEnable)`："普攻强制施放技能"〔02〕；`addSetStatePacket_*` 可驳回/改写状态切换〔01〕。受击打断机制文档完全未提
10. **Buff 类技能的变体流程**
    - `checkExecutableSkill_*` → `onSetState_*`（播动画）→ `onEndCurrentAni_*` 中创建 Appendage：`CNSquirrelAppendage.sq_AppendAppendage(...)` → `appendage.setAppendCauseSkill(BUFF_CAUSE_SKILL, sq_getJob(obj), SKILL_BUFFZY, skillLevel)`（左下角 Buff 图标）→ `appendage.sq_SetValidTime(Time)`（毫秒持续）→ 数据存 `appendage.getVar("buffdata")` 向量 → `CNSquirrelAppendage.sq_AppendAppendageID(appendage, obj, obj, APID_BUFF_ZY, true)`（"true 不能带过图"）→ 回 `STATE_STAND`；AP 的 `onStart_*` 读向量并经 `sq_AddChangeStatusAppendageID`/`addParameter` 改属性〔02〕

---

## 三、技能系统"基座"提供的 API 清单

### 3.1 状态机
- `obj.sq_AddSetStatePacket(state, priority, isResetTimer)`；`obj.sq_GetState()`；`obj.sq_StopMove()`；`obj.sq_RemoveSkillLoad(SKILL_ID)`
- 附带数据向量（角色）：`obj.sq_IntVectClear()`、`obj.sq_IntVectPush(v)`
- PO 状态包：`obj.addSetStatePacket(PASSIVEOBJ_SUB_STATE_0, pIntVec, STATE_PRIORITY_AUTO, false, "")`；全局向量 `sq_GetGlobalIntVector()`、`sq_IntVectorClear(pIntVec)`、`sq_IntVectorPush(pIntVec, 0)`
- 注册：`IRDSQRCharacter.pushState(职业枚举, "nut路径", "名字", STATE_ID, SKILL_ID)`；`IRDSQRCharacter.pushPassiveObj("nut路径", 编号)`

### 3.2 动画控制
- `obj.sq_SetCurrentAnimation(CUSTOM_ANI_XXX)`；`obj.getCurrentAnimation()`；`currentAni.GetCurrentFrameIndex()`
- `obj.sq_SetStaticSpeedInfo(SPEED_TYPE_CAST_SPEED, …)`
- PO：`obj.getCustomAnimation(0)`（"0是[etc motion]下面的第一个"）、`obj.setCurrentAnimation(ani)`
- 结束事件 `onEndCurrentAni_*`

### 3.3 攻击/伤害
- 算伤：`obj.sq_GetBonusRateWithPassive(技能编号, 状态编号, 动态数据第几个, 比率)`（"1.0表示100%"）；`obj.sq_GetPowerWithPassive(技能编号, 状态编号, 固伤动态数据第几个, 百分比动态数据第几个或-1, 比率)`
- 设伤：`obj.sq_SetCurrentAttackBonusRate(rate)`；`obj.sq_SetCurrentAttackPower(power)`；`sq_SetCurrentAttackPower(attackinfo, damage)`（PO）
- 命中事件三连（onBeforeAttack/onAttack/onAfterAttack，带 `boundingBox` 攻击框参数）

### 3.4 特效/被动物体（PO）创建与销毁
- `obj.sq_SendCreatePassiveObjectPacket(编号, 0, 0, 0, z)`（例：`24320` 与 `38001`；"能挂载nut的特效编号要在24300到24400之间"）
- `sq_SendDestroyPacketPassiveObject(obj)`
- PO 事件：`setCustomData_po_*` / `setState_po_*` / `onEndCurrentAni_po_*`

### 3.5 数据传输/变量容器
- 写包：`obj.sq_StartWrite()`；`obj.sq_WriteDword(v)`；读包：`reciveData.readDword()`
- 通用变量：`obj.getVar().setInt(name, v)` / `obj.getVar().getInt(name)`；命名容器向量：`obj.getVar("state").clear_vector()/push_vector(v)/set_vector(i, v)/get_vector(i)`
- 技能效果包接收：`onChangeSkillEffect_*(obj, skillIndex, reciveData)`

### 3.6 技能数值/等级数据
- `sq_GetSkill(obj, SKILL_ID)`；`sq_GetSkillLevel(obj, SKILL_ID)`
- 动态（按等级）数据：`obj.sq_GetLevelData(SKILL_ID, index, skillLevel)`，index"动态数据第几个（从0开始）"
- 静态数据：`obj.sq_GetIntData(SKILL_ID, index)`

### 3.7 CD/密封/按键
- `obj.startSkillCoolTime(SKILL_ID, skillLevel, -1)`；`skill.resetCurrentCoolTime()`；`skill.isInCoolTime()`
- `skill.setSealActiveFunction(true)`；`skill.setSealFunction(true)`；`skill.isSealFunction()`；skl 文件 `[seal enable] 1`
- `obj.sq_IsEnterSkill(SKILL_ID)`；`obj.sq_IsUseSkill(SKILL_ID)`（触发 CD）；`obj.setSkillMpRate(skillIndex, rate)`；`obj.setSkillCommandEnable(SKILL_ID, isEnable)`；`sq_SetAllCommandEnable(obj, false)`

### 3.8 Appendage（Buff/属性修改）
- `CNSquirrelAppendage.sq_AppendAppendage(obj, obj, skill_index, false, "ap路径", true)`；`CNSquirrelAppendage.sq_AppendAppendageID(appendage, obj, obj, APID, true)`；`CNSquirrelAppendage.sq_IsAppendAppendage(obj, "ap路径")`；`obj.GetSquirrelAppendage("ap路径")` + `appendage.isValid()`
- `appendage.sq_SetValidTime(毫秒)`；`appendage.setAppendCauseSkill(BUFF_CAUSE_SKILL, job, skillId, level)`；`appendage.getVar("名")`
- 属性修改：`appendage.sq_getChangeStatus("名")`；`appendage.sq_AddChangeStatusAppendageID(obj, obj, 0, CHANGE_STATUS_TYPE_ATTACK_SPEED, false, 数值, APID_COMMON)`（"false为数值，true为百分比"）；`change_appendage.clearParameter()`；`change_appendage.addParameter(类型, false, 值)`

### 3.9 目标/环境查询
- `sq_IsMyCharacter(obj)`；`obj.sq_IsMyControlObject()` / `obj.isMyControlObject()`；`obj.isInBattle()`；`obj.getObjectHeight()`；`sq_getJob(obj)`；`obj.getThrowIndex()`；`sq_IsItemRecover(itemIndex)`
- 目标查找/搜索类 API：**两篇文档均未出现任何目标搜索函数**

---

## 四、技能运行时状态存放在哪

1. **脚本层是函数集合，不是对象实例**：技能逻辑是 `.nut` 文件内按命名约定（`事件名_技能名`）组织的一组 Squirrel 全局函数，由 `IRDSQRCharacter.pushState(...)` 注册；全部示例中没有构造函数、成员字段、`this` 状态。
2. **运行时状态全部挂在引擎管理的对象上**，载体是"通用变量容器"而非强类型字段：角色对象 `obj.getVar()`（无名槽）与 `obj.getVar("state")`（命名容器 + vector）；Appendage `appendage.getVar("buffdata")`；PO `obj.getVar("state")`。
3. **"当前处于哪个阶段"由三样东西共同表达**：状态机 `obj.sq_GetState()` + getVar 标记位 + 动画帧号（时间基准外化为帧进度）。PO 阶段额外用子状态常量。
4. **一次性数据经数据包传给 PO**（sq_StartWrite/WriteDword → setCustomData_po_* 读）。
5. **技能等级相关的静态配置在 skl 数据文件**，经 `sq_GetLevelData`/`sq_GetIntData` 按下标读取。

## 五、判定帧/攻击时机机制

- **机制 = 脚本在 onProc 里按帧轮询动画帧号 + getVar 标记防重入**（血爆 onProc_BloodBoom 关键行）：
  ```squirrel
  local currentAni = obj.getCurrentAnimation();
  local currentAniIndex = currentAni.GetCurrentFrameIndex();
  local Hit = obj.getVar().getInt(FLAG_HIT);
  if(currentAniIndex == 20 && Hit == 0)
  ```
- 文档中**没有**"动画事件/关键帧事件（animation event）"机制的描述；"第几帧出判定"完全由脚本 `if(frameIndex == N && 标记 == 0)` 表达。
- 帧号来自 `ani.GetCurrentFrameIndex()`（动画对象方法，无 sq_ 前缀）；`sq_GetCurrentFrameIndex` 这个名字在两篇文档中**未出现**。
- 攻击框：唯一出现处是攻击事件参数 `boundingBox`；攻击盒的**创建/配置 API 两篇文档均未涉及**。

## 六、知识库没讲清/明显缺失的点

1. **事件路由机制未讲**：函数名后缀与 pushState 第 3 参的关系、引擎如何按后缀找到函数。
2. **`checkExecutableSkill_*` / `checkCommandEnable_*` 未列入 01 的总表**，准确调用时机/返回值语义/与 useSkill_before、sq_IsUseSkill 的先后关系均未讲。
3. **onProc/onProcCon 调用频率与同步模型**：每帧还是每逻辑帧；"组队双方/仅自己"确切含义未定义；状态包由谁广播未讲。
4. **攻击盒（判定框）机制完全缺失**（10-攻击系统.md 应有）。
5. **`onAttack` "返回受击对象"（抓取技基础）的具体语义未展开**。
6. **`onTimeEvent_*` 的注册方式缺失**；返回值两处描述自相矛盾。
7. **`onSetState_*` 的 `datas` 参数读取 API 缺失**。
8. **STATE_PRIORITY_* / S_FLOW_* 等常量含义未解释**��
9. **动画资源绑定链缺失**：CUSTOM_ANI_* 常量在哪定义、.ani 如何关联、getCustomAnimation 的"[etc motion]"数据源结构。
10. **CD 起算点**：sq_IsUseSkill 使技能进 CD——从按键瞬间还是状态结束起算？与 startSkillCoolTime 的关系未讲。
11. **打断/受击/无敌**：技能被受击硬直打断、后摇取消、柔化/强制取消体系——全部缺失。
12. **PO 事件函数不全 + 反常规则未解释**（"子状态不能为 1、应从 10 开始"）。
13. **Appendage 生命周期不全**（只注册演示了 onStart）。
14. **API 命名不一致**：`obj.sq_IsMyControlObject()` vs `obj.isMyControlObject()` 等多处。
15. **目标查找/范围搜索 API 完全缺失**（可能在 11/16 篇）。
16. **`useSkill_before/after` 的 `consumeItem` 机制未展开**；resetDungeonStart_* 四参数含义未讲。
17. **完整时序从未被文档串讲**（本笔记第二节是拼装的，各事件相对顺序需源码确认）。
