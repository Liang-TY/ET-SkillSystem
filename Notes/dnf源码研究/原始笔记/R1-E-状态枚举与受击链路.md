# R1-E：DNF 受击状态机与 STATE 全表提取笔记

> 第1轮 Agent E 原始笔记（B 的缺口补查）。任务：读《12-常量定义》《10-攻击系统》+ 01 受击相关节，补齐状态枚举表和受击链路。
> 重要更正：**STATE_* 完整枚举表不在 12-常量定义.md 里**（该文件只有 5 组常量）。全表实际位于同目录 `常量定义.txt`（118-195 行）和 `资源nut函数声明\language.dof.header.md`（203-354 行）。两者内容一致，header 版注释更全。
> 已综合进：01-技能生命周期与状态系统-总结.md

---

## 一、STATE_* 完整枚举表（0-75）

来源：`nut知识库\常量定义.txt:120-195`、`nut知识库\资源nut函数声明\language.dof.header.md:203-354`

| 编号 | 常量 | 含义（注释原文） |
|---|---|---|
| 0 | STATE_STAND | 站立状态，待机（韩文原文含"移动"） |
| 1 | STATE_SIT | 坐下（普通状态下按键） |
| 2 | STATE_SIT_FOREVER | 永久坐下。注意：header 中文注释标"状态-倒地"是**错位误译**，韩文 앉아있기=持续坐着 |
| **3** | **STATE_DAMAGE** | **挨打/受伤状态**。受击硬直走这个 |
| **4** | **STATE_DOWN** | **倒地状态**。注意：header 中文注释标"状态-浮空"，`sq_GetDownState` 的注释也是"浮空状态编号"——**DNF 里浮空和倒地共用此状态，靠子编号区分** |
| **5** | **STATE_DIE** | **死亡状态** |
| 6 | STATE_JUMP | 跳跃 |
| 7 | STATE_JUMP_ATTACK | 跳跃攻击 |
| 8 | STATE_ATTACK | 攻击 |
| 9 | STATE_HOLD | 抓取状态/持续禁锢（无法行动，被抓方所处状态） |
| 10 | STATE_SUMMONSTART | 召唤开始 |
| 11 | STATE_SUMMONEND | 召唤结束 |
| 12 | STATE_UNSUMMON | 反召唤消失 |
| 13 | STATE_THROW | 投掷 |
| 14 | STATE_DASH | 冲刺 |
| 15 | STATE_DASH_ATTACK | 冲刺攻击 |
| 16 | STATE_GET_ITEM | 获取物品 |
| 17 | STATE_BUFF | 增益状态 |
| **18** | **STATE_QUICK_STANDING** | **快速起身**（header 注释"从蹲伏起身"） |
| 19 | STATE_CROUCH | 下蹲 |
| 20 | STATE_CROUCH_ATTACK | 下蹲攻击 |
| 21 | STATE_LOW_KICK | 低踢/下段踢 |
| 22 | STATE_TRY_GRAB | 尝试抓取 |
| 23 | STATE_SUPLEX | 过肩摔 |
| 24 | STATE_JUMP_SUPLEX | 跳跃过肩摔 |
| 25 | STATE_JUMP_SUPLEX_LARIAT | 跳跃过肩摔追加套索踢 |
| 26 | STATE_MOUNT_TRY | 尝试骑乘 |
| 27 | STATE_MOUNT | 骑乘 |
| 28 | STATE_STOMP | 踩踏 |
| 29 | STATE_CLOSE_PUNCH | 近身拳击 |
| 30 | STATE_LIFT_UPPER | 升龙拳/上勾拳（**典型的上挑浮空技状态**） |
| 31 | STATE_GRAB_EXPLOSION | 抓取爆炸 |
| 32 | STATE_VIRTUAL_ATTACK | 虚拟攻击（普攻派生特殊攻击时的虚拟 STATE） |
| 33-73 | （职业技能状态） | 肩撞(33)、旋风腿(34)、闪电之舞(38)、升龙拳(50)、举起(53)、地面踢击(58)…均为格斗家系技能状态 |
| 74 | STATE_INHERIT_START | 继承开始（无注释） |
| 75 | STATE_MAX | 最大值 |

**关于 19+ 编号的说明（推断）**：`常量定义.txt:592-632` 另有一张"虚拟攻击状态"表（19-57，全圣职者技能），编号与主表 19-57 重叠。可断定：**0-18 是全职业通用状态，19 起是各职业自定义技能状态，每个职业一套**（编号可撞）。STATE_SPECIAL 在 03 文档被引用但全库无定义（**缺口**）。

### 状态优先级（补全注释版）

来源：`03-状态系统.md:46-50`、`常量定义.txt:586-590`（12-常量定义.md 的版本残缺）

| 值 | 常量 | 注释 |
|---|---|---|
| 0 | STATE_PRIORITY_AUTO | 根据当前状态自动决定（攻击取消、施法→触发等） |
| 1 | STATE_PRIORITY_USER | 用户输入决定（技能、攻击等） |
| 2 | STATE_PRIORITY_HALF_FORCE | 半强制，可被某些技能中断 |
| 3 | STATE_PRIORITY_FORCE | **强制优先级（倒地、伤害判定、投掷、举起等）——受击相关状态用这档** |
| 4 | STATE_PRIORITY_IGNORE_FORCE | 无视强制（过肩摔时的移动抓取等） |

## 二、受击链路拼图：命中→僵直→浮空→落地→倒地→起身

核心结论：**DNF 的受击反应是"攻击方配置驱动"**——所有受击反应参数都挂在攻击方的 AttackInfo 上，命中后由引擎（C++ 层，非脚本）应用到受击方。

### 第 0 步：攻击方在 AttackInfo 上配置"受击反应六件套"

来源：`10-攻击系统.md:176-251`、`常量定义.txt:934-955`、`language.dof.globalFunction.md`

```squirrel
sq_SetCurrentAttackeDamageAct(attackInfo, damageAct);   // 受击反馈模式（决定进哪个状态）
sq_SetCurrentAttacknUpForce(attackInfo, upForce);       // 浮空力（"致使浮空的高度"）
sq_SetCurrentAttacknBackForce(attackInfo, backForce);   // 击退力（推开距离）
sq_SetCurrentAttackeHitStunTime(attackInfo, stunTime);  // 击中后强制僵直时间(毫秒)
```

**DAMAGEACT 受击动作枚举**（常量定义.txt:934-939，globalFunction.md:1466-1476 有逐值注释）：
- 0 = DAMAGEACT_NONE 无受击反馈
- 1 = DAMAGEACT_DAMAGE 僵直（硬直受击）
- 2 = DAMAGEACT_DOWN 倒下
- 3 = DAMAGEACT_DAMAGE_EXCEPT_HUMAN 除人形护甲外僵直

**KNOCK_BACK_TYPE 击退类型枚举**（941-947）：
- 0 NORMAL 普通 / 1 KNOCK_BACK 长距离击退 / 2 SHORT_KNOCK_BACK 短距离击退 / **3 PIXEL_WITHOUT_DAMAGE_TIME 无受击时间的像素级击退（站立时受击不会倒地）** / -1 NOT_BACK 无击退

**HIT_DIRECTION 受击方向枚举**（949-955）：0 AUTO / 1 FRONT / 2 BACK / 3 OUTER 外侧 / 4 INNER 内侧

（KNOCK_BACK_TYPE 与 HIT_DIRECTION 的 setter 函数文档未收录——**缺口**）

### 命中 → 僵直（STATE_DAMAGE=3）
damageAct=1 时受击方进入硬直受击：播 `obj.sq_GetDamageAni(index)`（04-动画系统.md:113-118，按 index 区分多套受击动画）。僵直时长 = hitStunTime × 僵直率，可被 `sq_SetAttackInfoHitDelayRateDamager`（globalFunction.md:2199）调整，受击方属性 `CHANGE_STATUS_TYPE_HIT_RECOVERY <- 34 // 硬直率`（12-常量定义.md:162）参与计算（公式文档未给——**缺口**）。

### 上挑 → 浮空（STATE_DOWN=4 的浮空子态）
upForce>0 把受击者打上天。**浮空与倒地同用 STATE_DOWN(4)，靠子编号区分**：`sq_GetDownState(obj)` 注释为"获取浮空状态编号"（globalFunction.md:716-721）——DOWN 状态内部有多个子阶段（推测：浮空上升/浮空下落/落地弹跳/躺地，未证实，**缺口**：子编号取值表未收录）。

### 落地 → 倒地 → 起身
- 落地动画：`obj.sq_GetDownAni()`（04-动画系统.md:120-125）
- 躺地循环：`obj.sq_GetOverturnAni()` 循环倒地动画——倒地后躺在地上的持续动画，**此时受击判定仍存在**（DNF 可追打倒地目标）
- **快速起身**：STATE_QUICK_STANDING=18 + SKILL_QUICK_STANDING=190
- **普通起身的触发 API/状态转换文档完全没写**（**缺口**：推测由引擎在倒地动画播完后自动回 STATE_STAND，无脚本参与，未经证实）
- 倒地期间的受击（追打/弹跳）规则未收录（**缺口**）

### 死亡（STATE_DIE=5）
受击方脚本可通过 `onSetState_(obj, state==STATE_DIE, ...)` 响应；`reset_职业名` 在死亡复活/进房时执行。Appendage 可在 `onSetHp` 里拦截 HP 防死（hp<=0 时改 hp=1，06-附加对象.md:244-263）。

### 受击方可用的拦截/响应钩子（脚本层）
| 钩子 | 时机 | 来源 |
|---|---|---|
| `addSetStatePacket_职业名(obj, state, datas)` 返回 -1 | 任意状态切换前（含被击入 DAMAGE/DOWN），可驳回 | 03:87-107 |
| `onSetState_ / onAfterSetState_ / onEndState_` | 状态切换时/后/结束 | 01:43-75 |
| `onDamageParent(appendage, attacker, boundingBox, isStuck)` | 挂在受击者身上的 Appendage：被攻击时 | 06:180-194 |
| `onApplyHpDamage(appendage, newHpDamage, attacker)` | 被打时**修改伤害数值**（组队不运行） | 06:213-238 |
| `getImmuneTypeDamageRate(appendage, damageRate, attacker)` | 修改伤害率（减伤） | 06:265-283 |
| `onSetHp(appendage, hp, attacker)` | 拦截 HP 设置 | 06:244-263 |
| `sendSetHpPacket_职业名(obj, hp, sendInstant)` | 拦截 HP 变化包 | 03:209-227 |
| `setActiveStatus_职业名(obj, activeStatus, power)` 返回 0 拒绝 | 异常状态施加时 | 03:142-157 |

**没有独立的 onDamaged/onHpChanged 事件函数**——01 文件全目录已核查，受击侧响应全部走 Appendage 钩子或状态切换钩子。

## 三、攻击类型→受击反应的映射机制

**结论：映射不走 ATTACKTYPE，走 AttackInfo 的独立字段。**

- ATTACKTYPE（物理 0/魔法 1/绝对 2，10:154-160）只决定伤害计算方式，与受击反应无关。
- 受击反应完全由攻击方逐字段配置：damageAct（僵直/倒地二选一）+ upForce（浮空高度）+ backForce/knockBackType（击退）+ hitStunTime（僵直时长）。
- 即"上挑→浮空"= damageAct=1 或 2 + 高 upForce（对应 STATE_LIFT_UPPER=30 这类技能状态）；"重击→倒地"= DAMAGEACT_DOWN=2；"拉扯"= knockBackType。
- ATTACK_DIRECTION（上 0/中 1/下 2 段，常量存在）是否影响受击反应或对空/对地命中（**缺口**）。
- 攻击属性（ENUM_ELEMENT）只影响属性伤害与异常附加，不影响受击动作。

## 四、多段命中与时序

- `obj.sq_SetMaxHitCounterPerObject(maxHit)`：同一攻击对单目标的最大命中次数，达上限不再命中（10:309-316）。
- 命中时序钩子（攻击方侧，每次命中触发）：`onBeforeAttack_` → `onAttack_` → `onAfterAttack_`（10:5-39）；onAttack 可返回受击对象，"是写抓取技能的基础"。
- **同一攻击多段命中的间隔规则（攻速/帧间隔如何控制 hit interval）文档未收录**（**缺口**）。
- 绕过攻击盒直接发命中：`sq_SendHitObjectPacket(obj, target, x, y, z)`、`sq_SendHitObjectPacketWithNoStuck`，或 `sq_getNewAttackInfoPacket()` 构造完整包（含 hitStunTimeAttackerDamager/upForce/backForce/knockBackType 全部受击参数）后 `sq_SendHitObjectPacketByAttackInfo(sourceObj, targetObj, ap)`（10:255-293，字段定义见 language.dof.AttackInfoPacket.md 全文）。

## 五、附带收获：12-常量定义.md 的两处残缺

1. ACTIVESTATUS 表在 03-状态系统.md:185-205 比在 12 里多出 3 项：**ACTIVESTATUS_HOLD=16 束缚、ACTIVESTATUS_ARMOR_BREAK=17 护甲破甲、ACTIVESTATUS_MAX=18**（12 的表止于 15，是残缺版）。
2. 控制型异常（眩晕 3/冰冻 1/石化 7/睡眠 8）本质是"站着不动的强控受击"，经 `sq_sendSetActiveStatusPacket(targetObj, obj, 类型, 概率, 等级, 强制false, 时间)` 或 `sq_SetChangeStatusIntoAttackInfo(attackInfo, 0, 类型, 概率, 等级, 时间, 伤害)` 施加（03:159-179、10:187-218）。

## 缺口清单（文档确实没有，勿编造）

1. STATE_DOWN(4) 的子编号取值表（sq_GetDownState 返回值的含义）
2. 普通起身（非 Quick Standing）的状态转换触发机制
3. 倒地追打/落地弹跳的判定规则
4. KNOCK_BACK_TYPE、HIT_DIRECTION 的 setter 函数签名
5. 多段命中的间隔规则
6. STATE_SPECIAL 的定义值
7. "命中后引擎将受击方切进 STATE_DAMAGE/STATE_DOWN"这一步的引擎侧代码（只能从 damageAct API 反推）
8. hitStunTime 与硬直率(HIT_RECOVERY)的具体合成公式
