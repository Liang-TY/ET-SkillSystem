# R1-B：DNF 状态系统提取笔记（来源：nut知识库/03-状态系统.md）

> 第1轮 Agent B 原始笔记。任务：读知识库《03-状态系统》。
> 结论先行：该文档实际内容是"状态 API 层"（设置/拦截/查询状态 + 异常状态枚举），**不含状态全表和受击状态机**——相关各节如实标注文档只讲到哪一步，缺口汇总在第六节。
> 已综合进：01-技能生命周期与状态系统-总结.md（缺口部分由 R1-E 补齐）

---

## 一、DNF 角色状态全表

**文档没有提供状态全表。** 文档中按名字出现过的状态仅限代码示例中零星几个：

| 状态名（文档原文） | 行为特征 | 出处 |
|---|---|---|
| `STATE_DIE`（死亡） | 示例：进入该状态时执行 `obj.sq_RemoveSkillLoad(SKILL_ID)` | L14-16 |
| `STATE_JUMP`（跳跃） | 示例中作为"当前状态"被查询：`obj.getState() == STATE_JUMP` | L97 |
| `STATE_JUMP_ATTACK`（跳跃攻击） | 示例中作为"想切入的新状态"，在跳跃状态下可被拦截驳回 | L97-102 |
| `STATE_SPECIAL`（特殊状态） | 处于该状态时 `setActiveStatus_*` 返回 0，**拒绝被附加异常状态** | L149-153 |
| `STATE_OTHER`、`STATE_XXX` | 纯占位符，非真实常量 | L34、L101 |

文档通过 getter 注释**间接暗示了状态是数字编号**（L116-122）：
- "获取 **8状态**攻击状态下的攻击ID" → `getAttackIndex()` —— 8 号状态 = 攻击状态
- "获取 **13状态**使用的技能ID / 13状态下的子状态" → `getThrowIndex()` / `getThrowState()`
- "获取 **17状态**使用的技能ID" → `getBuffSkillIndex()` —— 17 号状态 = Buff状态

## 二、受击状态机细节

**这是文档最大的空白：完全没有受击状态机内容。** 逐项核对：
- 普通受击/浮空/倒地的触发条件：未提及。全文没有 launch、down、倒地、起身、硬直（hitstun）等字样。
- 浮空→落地→倒地→起身链路及时长：未提及。
- 浮空追击/倒地追击：未提及。
- 受击无敌帧/防无限连机制：未提及。

与"受击/被控"沾边的仅有两条间接线索（不可过度解读）：
1. 状态优先级注释（L49）：`STATE_PRIORITY_FORCE <- 3 // 被迫去做的事情（坠落，伤害，死亡，拦截等）一般给敌人发送控制状态时`——受击（伤害）、坠落、死亡属于"强制"优先级 3。
2. 异常状态枚举（L186-205）中的控制类：眩晕/冰冻/石化/睡眠/束缚/混乱等。异常状态施加带概率、等级、时间参数，且受方可通过 `setActiveStatus_*` 拒绝。

## 三、状态切换规则

### 3.1 状态优先级（L46-51，原文照录含注释）

```squirrel
STATE_PRIORITY_AUTO <- 0           // 根据时间和条件自动完成的一切（攻击结束，发射->战场等）特效一般用这个
STATE_PRIORITY_USER <- 1           // 根据用户的command发布的内容（技能，攻击等）一般是使用技能的时候
STATE_PRIORITY_HALF_FORCE <- 2     // 想要离开，但都比强制要低（翻过来等）
STATE_PRIORITY_FORCE <- 3          // 被迫去做的事情（坠落，伤害，死亡，拦截等）一般给敌人发送控制状态时
STATE_PRIORITY_IGNORE_FORCE <- 4   // 强制变更无视的东西（用手术刀抓住对方等）一般是抓取敌人的时候
```

数值越高优先级越高是常理推断，文档未明说数值大小与胜败判定的关系。

### 3.2 状态拦截（L87-107）
`addSetStatePacket_职业名(obj, state, datas)`：可**驳回**状态，并可改为设置别的状态。示例：当前为 STATE_JUMP 时，若请求切入 STATE_JUMP_ATTACK，则改发 STATE_OTHER（附带数据 4）并 `return -1` 驳回原状态；`return 0`（或其他值）表示放行。

### 3.3 状态结束钩子（L29-39）
`onEndState_技能名(obj, new_state)`：状态结束时触发，通过判断 `new_state != STATE_XXX` 决定是否做清理——旧状态能感知自己被什么新状态替换。

### 3.4 状态切换时动画怎么切
**本文档未提及。**

## 四、状态与技能的关系

1. **技能脚本按状态钩子组织**：`onSetState_技能名`、`onAfterSetState_技能名`、`onEndState_技能名`（每个技能脚本可挂自己的状态进入/离开逻辑）。`onSetState_*` 在 `obj.sq_AddSetStatePacket` 执行后被触发。
2. **死亡清技能蓄力**：进入 STATE_DIE 时示例调用 `obj.sq_RemoveSkillLoad(SKILL_ID)`。
3. 优先级注释间接表明：技能/攻击发布属于 STATE_PRIORITY_USER（1）。

**哪些状态下能施放技能/普攻、施法中被打是否打断——文档均未提及。**

## 五、状态相关 API 函数清单（签名照抄）

### 状态回调（脚本侧钩子）
| 签名 | 说明（文档原话） |
|---|---|
| `onSetState_技能名(obj, state, datas, isResetTimer)` | 设定状态时执行的函数 |
| `onAfterSetState_技能名(obj, state, datas, isResetTimer)` | 状态设置后的处理 |
| `onEndState_技能名(obj, new_state)` | 状态结束时 |
| `addSetStatePacket_职业名(obj, state, datas)` | 增加设置状态包时；可驳回���return -1 驳回 |
| `setActiveStatus_职业名(obj, activeStatus, power)` | 设置异常状态时；返回 0 拒绝，返回 1 允许 |
| `sendSetHpPacket_职业名(obj, hp, sendInstant)` | 发送设置HP包，可拦截 HP 变化 |
| `sendSetMpPacket_职业名(obj, mp, sendInstant)` | 发送设置MP包 |
| `reset_职业名(obj)` | 重置刷新时；死亡复活、进入下一个房间时执行 |
| `resetDungeonStart_职业名(obj, moduleType, resetReason, isDeadTower, isResetSkillUserCount)` | 副本中重置状态时 |

### 设置/发送状态
| 调用 | 说明（出处） |
|---|---|
| `obj.sq_AddSetStatePacket(STATE_ID, STATE_PRIORITY_USER, false)` | 发送状态，不传值（L60）；末参 true=传递数据 |
| `obj.sq_IntVectClear()` / `obj.sq_IntVectPush(value)` … 再 `sq_AddSetStatePacket(..., true)` | 发送状态并传递值（L66-69） |
| `sq_GetGlobalIntVector()`; `sq_IntVectorClear(pIntVec)`; `sq_IntVectorPush(pIntVec, v)`; `obj.sendStateOnlyPacket(STATE_ID, pIntVec)` | 特效中发送状态包（L77-80） |
| `obj.sq_GetVectorData(datas, 0)` | 在 onSetState_* 里读取传入数据（L133-134） |

注意：L101 拦截示例中写的是小写 `obj.sq_addSetStatePacket(...)`，与 L9/L60 的 `sq_AddSetStatePacket` 大小写不一致（文档原样如此）。

### 查询状态（L116-122，注释照抄）
```squirrel
local state = obj.sq_GetState();                // 获取当前状态
local subState = obj.getSkillSubState();        // 获取技能子状态
local throwIndex = obj.getThrowIndex();         // 获取13状态使用的技能ID
local throwState = obj.getThrowState();         // 获取13状态下的子状态
local attackIndex = obj.getAttackIndex();       // 获取8状态攻击状态下的攻击ID
local buffSkillIndex = obj.getBuffSkillIndex(); // 获取17状态使用的技能ID
```
另：全局形式 `sq_GetState(obj)` 出现于 L149。文档中不存在字面名为 `setState`/`getState` 的 API。

### 异常状态
| 调用 | 说明（出处） |
|---|---|
| `sq_sendSetActiveStatusPacket(targetObj, obj, ACTIVESTATUS_STUN, prob.tofloat(), level, false, time)` | 直接发送异常状态包；参数：被附加对象，源对象，异常类型，概率，等级，强制false，时间（L163-168） |
| `sq_SetChangeStatusIntoAttackInfo(attackInfo, 0, ACTIVESTATUS_BLEEDING, bleedingrate, bleedinglevel, bleedingtime, bleedingdamage)` | 在攻击信息中设置异常状态（L174-178）——注意注释列 6 个参数但调用有 7 个实参，第 2 个实参 0 未被注释解释 |

异常状态枚举（L186-205，照抄）：
```squirrel
ACTIVESTATUS_SLOW <- 0            // 减速
ACTIVESTATUS_FREEZE <- 1          // 冰冻
ACTIVESTATUS_POISON <- 2          // 中毒
ACTIVESTATUS_STUN <- 3            // 眩晕
ACTIVESTATUS_CURSE <- 4           // 诅咒
ACTIVESTATUS_BLIND <- 5           // 失明
ACTIVESTATUS_LIGHTNING <- 6       // 感电
ACTIVESTATUS_STONE <- 7           // 石化
ACTIVESTATUS_SLEEP <- 8           // 睡眠
ACTIVESTATUS_BURN <- 9            // 燃烧
ACTIVESTATUS_WEAPON_BREAK <- 10   // 武器破甲
ACTIVESTATUS_BLEEDING <- 11       // 出血
ACTIVESTATUS_HASTE <- 12          // 加速
ACTIVESTATUS_BLESS <- 13          // 祝福
ACTIVESTATUS_ELEMENT <- 14        // 元素
ACTIVESTATUS_CONFUSE <- 15        // 混乱
ACTIVESTATUS_HOLD <- 16           // 束缚
ACTIVESTATUS_ARMOR_BREAK <- 17    // 护甲破甲
ACTIVESTATUS_MAX <- 18
```
（12/13/14 是增益型，与控制���混在同一枚举——文档事实。）

### 其他状态周边
- `obj.sq_RemoveSkillLoad(SKILL_ID)`；`CNSquirrelAppendage.sq_IsAppendAppendage(obj, "路径/ap_xxx.nut")`；`obj.getHp()`

## 六、知识库没讲清/明显缺失的点

1. **没有状态 ID 枚举表**。状态是数字（8=攻击、13=投掷、17=Buff），完整编号-名称对照缺失。文档自称"12-常量定义.md 有常量定义"可查。
2. **受击状态机整体缺失**：硬直、浮空触发、落地倒地起身链路、追击规则、无敌帧、防无限连保护——全部只字未提。
3. **状态优先级的裁决机制未讲**：高能否打断低、同优先级怎么处理、IGNORE_FORCE(4) 与 FORCE(3) 的精确语义未说明。
4. **状态与技能的兼容矩阵缺失**。
5. **状态→动画绑定缺失**。
6. **isResetTimer 参数语义未解释**。
7. **datas 的写入读取不对称**（sq_IntVectPush vs sq_GetVectorData vs sq_IntVectorPush 两套 API）。
8. `sq_addSetStatePacket`（小写）与 `sq_AddSetStatePacket`（大写）大小写不一致。
9. `sq_SetChangeStatusIntoAttackInfo` 注释 6 参调用 7 实参。
10. `sq_sendSetActiveStatusPacket` 第 6 参"强制false"只给了 false 示例。
11. `STATE_OTHER`/`STATE_XXX` 是示例占位符，勿当真实状态名引用。

**可能藏着缺失信息的邻近文档**：01、02、10-攻击系统、12-常量定义、07-属性状态，以及 `资源nut函数声明\language.dof.character.md` 等函数声明文件。

**给主会话的提示**：如果目标是学受击链路，这份 03 文档信息量接近于零，只有三点可用：①受击类状态=FORCE 优先级 3；②控制类异常状态枚举；③异常状态可被受方按当前状态拒绝。状态全表和打击链路需从邻近文档另行提取（→ 已由 R1-E 完成）。
