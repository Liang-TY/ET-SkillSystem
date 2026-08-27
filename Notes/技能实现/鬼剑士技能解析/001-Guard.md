# 格挡（Guard）

> 技能ID 1 | 级别 B | 可实现性 ⛔（核心"吸收正面伤害"依赖受击伤害管线钩子，R3-A15 已记缺口的第 4 个消费方） | 分析日期 2026-08-22 | 批次 B1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 格挡 | `skill\Swordman\Guard.skl` [name] |
| 英文名 | Guard（取 skl 文件名；[name2]="The Guard"） | 同上 [name2] 实测 |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5；其他职业树另有 atpriest/demonicswordman 同名 skl，实测 find） | 同上 |
| 学习等级 | 5 | 同上 [required level] |
| 最高等级 | 10（[maximum level]；[growtype maximum level] 各系 5——觉醒后段续表 48 行另计） | 同上 |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ↓↓ + X | 同上 [command] / [command key explain] |
| CD | 2000 ms（dungeon，固定） | 同上 [dungeon][cool time] |
| MP | 3 → 4（Lv1→Lv10；**仅格挡开始时消耗一次**，explain 明示） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `500 400 100 400 3000`（5 值，语义未考证） | 同上 [dungeon][static data] |
| 一句话效果 | 按住技能键保持格挡姿态，吸收来自**前方**的攻击伤害（物理/魔法%）；格挡中被击会被击退一定距离 | 同上 [explain] |

**level property 模板解码（L21 向量法，3 行对位）**：
- 吸收物理伤害 = col2：**40% → 80%**（Lv1→Lv10，觉醒后段恒 80）
- 吸收魔法伤害 = col3：**15% → 80%**
- 被击退时间 = col1 × 0.001：**0.2s → 0s**（等级越高被打退越短）
- col0（200→700）与 col4（10→100）**语义未考证**（无模板行对应，疑分别为格挡持续相关时长与冲击波/反击参数——见 §8）。

level info 共 5 列 48 行（Lv1-10 + 觉醒后段 38 行）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**鬼剑士本尊无注册**——`swordman_load_state.nut` grep `guard` 零命中；`sqr\character\swordman\` 递归 grep 无 guard 状态 nut；attack.nut 隐藏函数组（F4 套路）亦无。原版格挡逻辑**引擎内置**（老一代技能，F3）。

**同型完整参照脚本（F3③）**：剑影版注册于 `atswordman_load_state.nut:93`：

```
IRDSQRCharacter.pushState(10, "character/atswordman/guard/guard.nut", "Guard", 85, 85);
```

`sqr\character\atswordman\guard\guard.nut`（201 行，mod 混淆但可读）+ `ap_guard.nut`——行为同构，逐回调对照见 §2.2。另 `atswordman\1_swordmaster\autoguard\autoguard.nut`（自动格挡，技能 126/状态 126）为同族被动版。

### 2.2 参照实现逐回调（atswordman guard.nut，剑影版 201 行实读）

```
checkExecutableSkill_Guard：sq_IsUseSkill(85) 可用 → 切状态 85 子状态 0
onSetState 子状态 0（起手格挡）：
  播 SW_GUARD 音效；sq_StopMove；播格挡姿态动画（ats 版 ani 15/216/390 按魔剑/戴缪斯变体）
onSetState 子状态 1（保持格挡，被击后）：
  若刚被格挡命中（ATGuard 标记）：sq_SetMoveDirection(背向) + sq_SetStaticMoveInfo(0,100,100)（被击退移动）
  挂 ap_guard.nut appendage（伤害吸收，见下）
onAfterDamage_Guard（被击结算钩子，核心）：
  处于子状态 1 时，若 攻击者在正面(IsFrontOf) 且伤害类型为 sendState 3/4/9：
    挂 ap_counterslash.nut appendage（sq_SetValidTime(600)——600ms 反击窗口）
    置 ATGuard=true、Counterslash=true，向背面移动，重入子状态 1
onProcCon_Guard：子状态 1 中按住技能键且过半程 → 可重新进入格挡循环
onEndCurrentAni：子状态 0 播完 → 子状态 1；子状态 1 播完 → 回待机 state 0
onEndState：离开状态 85 时移除 ap_guard appendage
```

**ap_guard.nut（伤害吸收实现，核心）**——appendage 挂 `getImmuneTypeDamageRate` 回调：

```
getImmuneTypeDamageRate(appendage, damage, attacker)：
  攻击类型 == 1(魔法) → rate = sq_GetLevelData(85, 1, level)   // 魔法吸收列
  攻击类型 == 0(物理) → rate = sq_GetLevelData(85, 0, level)   // 物理吸收列
  return damage - rate%
  ——引擎在伤害结算管线里调该回调，格挡者按 % 削减所受伤害
```

（注：ats 版读自己 skl 的 col0/col1；鬼剑士版模板对位是 col2/col3——两版列布局不同，模板直读各自成立，见 §1。）

**被击退表现**：`hitback.ani`（.chr [etc motion] 槽 2 = HitBack.ani，9 帧 800ms）**全部 9 帧 [DAMAGE TYPE] SUPERARMOR**（实测 grep×9）——被击退期间霸体不可打断。

**格挡冲击波 PO**：`passiveobject\character\swordman\guardwave.obj`（**PO 20040**，passiveobject.lst:11213 实测）——[name] `格挡的冲击波`：basic motion GuardWave.ani（8 帧 560ms，**F0-F6 全有攻击盒**）+ attack info `guardwave.atk`（物理/damage 反应/push 100/lift 100/no blood 40）+ 叠加尘土层 GuardWaveDust.ani。创建时机引擎内置未考证（按命名与参数推断：格挡成功瞬间在身前产生冲击波推开/伤害敌人）。

### 2.3 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 受击盒 | 备注 |
|---|---|---|---|---|---|---|
| `character\swordman\animation\guard.ani`（.chr etc 槽 0，CUSTOM_ANI_GUARD=0 实证） | 2 | 10000+10000（**事件悬停帧**） | 无 | 无 | 2/2 | 按住期间停在姿态帧（引擎按按键事件推进，L23 族）；帧 123/124 |
| `character\swordman\animation\hitback.ani`（etc 槽 2） | 9 | 800ms（100+50×7+400） | 无 | 无 | 9/9 | **全程 SUPERARMOR**；被击退表现 |
| `passiveobject\...\animation\guardwave.ani`（PO） | 8 | 560ms | 无 | F0-F6 | — | 冲击波判定帧（引 guard_attack_ldodge.img） |
| `passiveobject\...\animation\guardwavedust.ani` | 8 | 560ms | 无 | 无 | — | 尘土叠加层（guard_attack_none.img） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Guard.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Guard.skl` | ✅ 实测（160 行） | 数值（吸收%/击退时间） |
| 注册行 | —（鬼剑士无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | F2 三招全查 |
| 参照 nut | guard.nut + ap_guard.nut | `…\pvf\sqr\character\atswordman\guard\` | ✅ 实测（201+43 行） | 剑影版完整实现（C2 定点读） |
| 同族 | autoguard.nut（自动格挡） | `…\pvf\sqr\character\atswordman\1_swordmaster\autoguard\` | ✅ 存在（未细读） | 技能 126 被动版（技能 3 AutoGuard 的同族） |
| .chr 条目 | etc 槽 0 = Guard.ani / 槽 2 = HitBack.ani | `…\pvf\character\swordman\swordman.chr` 973/975 行 | ✅ 实测 | 格挡姿态 + 被击退 |
| 角色 .ani | guard.ani / hitback.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.3 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（grep guard 无） | ⛔ 无 | 格挡无攻击 |
| PO 定义 | guardwave.obj | `…\pvf\passiveobject\character\swordman\guardwave.obj` | ✅ 实测 | 冲击波 PO |
| PO .atk | guardwave.atk | `…\pvf\passiveobject\character\swordman\attackinfo\guardwave.atk` | ✅ 实测 | push100/lift100/物理 |
| PO .ani | guardwave.ani / guardwavedust.ani | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.3 |
| .als | — | 两侧 animation 目录 | ⛔ 无边车 | — |
| 装备层 | guard.ani ×N | `…\pvf\equipment\character\swordman\avatar\{belt,coat,…}\` | ✅ 实测 | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 123/124 格挡姿态、51-59 被击退） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动作 | 必需 | ✅ 已在库 |
| guard_attack_ldodge.img | sprite_character_swordman_effect_guard.NPK（路径 `Character/Swordman/Effect/Guard/` 下划线化推导） | 冲击波主层 8 帧 | 必需（若做冲击波） | ❌ 未入库 |
| guard_attack_none.img | 同上 | 尘土叠加层 | 可选 | ❌ 未入库 |

缺失 img：必需 1 张、可选 1 张（同一 NPK 一次提取）。

## 5. 实现方案草案

**⛔ 暂缓（核心机制）**——按现状只有"姿态动画 + 按键消费"能落地，伤害吸收/正面判定/被击退三件核心全部撞管线：

| DNF 机制 | 我们的现状（代码实测） | 阻断点 |
|---|---|---|
| 正面攻击判定（IsFrontOf） | `ApplyHit`（LSHitboxComponentSystem:102-126）直接跑**攻击方** HitActions，**无受击方任何钩子** | 无受击侧回调 |
| 伤害按 % 吸收（ap_guard getImmuneTypeDamageRate） | `MeleeHit` 读 `HitReaction.Damage` 直扣——无伤害修正注入点 | 同上（**受击伤害管线钩子**缺口，R3-A15 已记，本技能第 4 消费方） |
| 被格挡击退 + 600ms 反击窗口 | 击退语义在攻击方 HitReaction.KnockbackX；格挡者"被击但免硬直+自退"无法表达 | 同上 + 硬直豁免 |
| 按住技能键保持格挡 | 输入只有按下沿缓冲（PeekBufferedButton），无按住检测 | 按住输入缺失 |
| 格挡冲击波（PO 20040） | `GuardWaveArea : AreaDefinition` 可直配（EnterActions=MeleeHit，push100/lift100，ViewAnimId 双层） | **可表达**——只等格挡判定事件触发它 |

**使能前提（一次管线投资，多方受益）**：在 `ApplyHit` 前插受击方门面（如 `LSCombatComponent.OnIncomingHit(attacker, ref HitReaction)` 或 BuffDefinition 级 onIncomingHit 回调）——血爆系"HP 下限钳制"（R3-A15 上报）、不屈意志施法免打断（R1-A4）、本技能伤害吸收、后跳无敌帧判定可同管线解决。落地后本技能映射：`GuardSkill : SkillLogic`（子状态 0 姿态/1 持续）+ 按住简化为固定时长窗口（如 1.5s）+ 受击钩子里做"攻击者在正面 → Damage×(1-吸收%)、硬直 0、自退 KnockbackX、反击窗口 600ms（输入缓冲已有）+ 生成 GuardWaveArea"。

**注册点预留**：`SkillIds.Guard = 25`；`AnimId.SwordmanGuard = 108`、`GuardWave = 109`、`GuardWaveDust = 110`；`AreaIds.GuardWave = 16`。

**关键数值表（若实现）**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 2000ms | 2000 直用 |
| 物理吸收 | 40→80%（col2） | 70%（固定） |
| 魔法吸收 | 15→80%（col3） | 70%（无元素区分时同物理） |
| 被击退时间 | 0.2→0s（col1） | 0.15s 位移 1 单位 |
| 反击窗口 | 600ms（ats 版 sq_SetValidTime） | 600 直用 |
| 冲击波 | guardwave.atk：push100/lift100/damage 反应 | HitReaction{Damage=30, HitstunMs=300, KnockbackX=100, LaunchY=100} |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Guard.skl` | `.skl` 无子命令（5 列 level info + 5 值 static data） | 手抄 3 值（模板全明）；`skl` 子命令同前议 |
| `guard.ani` | **双 10000ms 事件悬停帧**（L23 族——按住保持语义） | 钳制/约定手改（缺口已记档）；消费侧用技能时长兜底 |
| `hitback.ani` | `[DAMAGE TYPE] SUPERARMOR` ×9（整节跳过，README 明示） | 霸体帧延后档既有记档 |
| `guardwave.ani / guardwavedust.ani` | `[SHADOW]`（规则表外） | 整节跳过无碍（已记 README 补记建议）；其余常规节 |

结论：PO 动画可被现有 ani 子命令翻译；实质缺口 `.skl` 子命令 + 超长 DELAY（均重复印证），无新节缺口。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 吸收正面伤害 % | **缺失档：受击伤害管线钩子**（R3-A15 第 4 消费方） | ⛔ 主因；管线落地前不做 |
| 按住保持格挡 | 按住输入（延后档） | 固定时长窗口替代 |
| 被击退 + 霸体（hitback 全程 SUPERARMOR） | 霸体帧（延后档）+ 受击侧控制 | 管线落地时一并 |
| 格挡冲击波 | 无（AreaDefinition 可表达） | 事件触发即可 |
| 反击窗口（counterslash 600ms） | 输入缓冲已有 ✅ | 直用 |
| MP 仅开始扣一次 | MP 系统（延后档） | 跳过 |
| 自动格挡（技能 3）/强制-格挡联动 | 被动技能系统 + 技能取消体系（缺失档） | 不实现 |

## 8. 存疑与缺口上报

**未考证项**
1. 鬼剑士版 col0（200→700）与 col4（10→100）语义（无模板行；col4 疑自动格挡联动或冲击波参数）。
2. `[static data] 500 400 100 400 3000` 五值语义。
3. guardwave.obj 的创建时机（推断为格挡成功瞬间，无脚本/数据佐证）。
4. 鬼剑士本尊格挡的引擎状态号（ats 版为 85，鬼剑士版未考证；老技能状态号小数字惯例）。
5. 鬼剑士版是否也有 counterslash 式自动反击（ats 版实证 600ms；鬼剑士原版疑为"格挡后手动取消"——未考证）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **受击侧方向判定**（IsFrontOf 等价）：即便伤害管线钩子落地，"仅正面格挡"还需攻击者→受击者朝向判定 API（受击钩子的参数需带攻击者引用——建议随管线一并设计）。
2. **按住输入检测**（isDownSkillLastKey 等价）：格挡（持续姿态）/蓄力技（R3-A15 已记共性"按住蓄力输入缺失"）共同依赖——建议在 LSInputBufferComponent 加按住状态位（帧同步安全：输入本身就是指令流的一部分）。
3. （缺口追加记档）**受击伤害管线钩子**第 4 消费方（前 3：HP 下限钳制/施法免打断/后跳无敌帧判定）——建议在 00-总览 将其列为优先立项候选（多技能解锁的杠杆点）。

**翻译工具缺口**：`.skl` 子命令 + 超长 DELAY（均重复印证，无新增）。

**给下轮的经验**：格挡这类"防御姿态"技能的关键证据在 **appendage 的 getImmuneTypeDamageRate 回调**（伤害管线注入点的 DNF 原生形态）——同族技能（自动格挡/不屈意志/剑魂觉醒防御）都该先找 appendage 回调而不是状态 nut。atswordman\demonicswordman 各有同名 guard 实现可参照（F3③）。
