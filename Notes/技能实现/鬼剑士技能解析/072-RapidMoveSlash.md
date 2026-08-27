# 猛龙断空斩（RapidMoveSlash）

> 技能ID 72 | 级别 A | 可实现性 🔶（主干=多段前冲连斩+终结浮空上斩，段间连段已通；追加方向操作/光剑感电/全程霸体/普攻中取消依赖缺口，简化绕过） | 分析日期 2026-08-22 | 批次 A12

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 猛龙断空斩 | `skill\Swordman\RapidMoveSlash.skl [name]` |
| 英文名 | RapidMoveSlash（skl 文件名；[name2]="Dragon Air Break"） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1；L17 映射 1=剑魂） | 同上 |
| 学习等级 | 40（前置：技能 8 TripleSlash 三段斩 Lv5） | 同上 [required level] / [pre required skill] |
| 最高等级 | 70（剑魂段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1，物理武器效果） | 同上 [type] / [skill class] / [weapon effect type] |
| 指令 | ↑→→ + Z（指令 MP 优惠 20%/40%） | 同上 [command] |
| CD | 20000 ms（pvp 30000、pvp 起手 30000）；**[auto cooltime apply]=0 → CD 从技能结束起算**（对应我们 ManualCooldown=true） | 同上 [cool time] / [auto cooltime apply] |
| MP | 25 → 420（Lv1→Lv70） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 可执行状态 | `[executable states] 8`——**普攻状态（state 8）中可施放**（取消普攻接猛龙） | 同上 [executable states] |
| static data | `4 1 2000 -5000 300 -1000 0`（7 值；pvp 版同序但第 5 值 300→400） | 同上 [static data] |
| 一句话效果 | 向前快速移动并���击敌人，可上下左右移动着连斩，追加操作指定后续方向；最后一击为浮空单手上斩；光剑+光剑精通可附感电 | 同上 [explain] |

**level property（3 列模板，Lv1 → Lv70）**：斩击次数上限 = static[0] = **4 次**（向量 `0 0 1.0`，L21 解码法）；后续攻击物理攻击力 = col0：`265% → 1953%`；最后一击物理攻击力 = col1：`529% → 3906%`。
static 其余值语义**未考证**（引擎消费）；旁证：pvp 版仅 static[4] 由 300→400，结合 explain"移动斩击"推断 **static[4]=每段冲斩距离 px（300，pvp 400）**。
[feature skill index] 152 = RapidMoveSlashEx.skl（TP 强化版，E 类批另行分析）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
93:  IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/RapidMoveSlash/RapidMoveSlash.nut", "swordman_rapidmoveslash", 39, -1);
132: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/RapidMoveSlash/RapidMoveSlash.nut", "swordman_rapidmoveslash", 39, 72);
```

状态号 **39**，技能 72（132 行绑定；93 行为不绑定技能的同名注册）。主 nut `sqr\character\swordman\rapidmoveslash\RapidMoveSlash.nut` 仅 53 行且被 mod 作者混淆（C3 同款随机变量名）——**半引擎内置（F3b）**：多段冲阵/方向追加/终结上斩的核心状态机在引擎里，pvf 只剩门禁壳。

### 2.2 主 nut 逐回调（rapidmoveslash.nut，53 行实测）

- `checkExecutableSkill_RapidMoveSlash(obj)`：`sq_IsUseSkill(72)` → IntVect 写 [0,1] → `sq_AddSetStatePacket(39, STATE_PRIORITY_USER, true)`——进入状态 39。
- `checkCommandEnable_RapidMoveSlash(obj)`：恒 true（指令可用性交引擎；skl 的 [executable states] 8 允许普攻中接）。
- `onAfterSetState_swordman_rapidmoveslash(obj, skillId, ?, vector)`：若 vector[0]==1 且挂有 `ap_stateoflimit` appendage（mod"状态边界"系统）→ 写包（248, 3, 248 技能列3 倍率, 动画总 delay）创建 **PO 24370**（mod 共享打击 PO，L20）——**纯 mod 注入**（原版无此逻辑），与技能本体无关，记档不实现。
- 其余回调（onSetState/onProc/onKeyFrameFlag/onEndCurrentAni）**全部不存在**——冲阵节奏由引擎按 .ani 与 static data 驱动（推断，无脚本可证）。

### 2.3 引擎行为重建（.ani + .atk + static data 三方印证；推断标注）

- 节奏（推断）：每次斩击 = `RapidMoveSlashReady1/2.ani`（160ms 起手）→ `RapidMoveSlashMove1.ani`（500ms 冲斩，攻击信息=atk1）；第 4 段（最后一击）换 `Move2.ani`（500ms，攻击信息=atk2 浮空上斩）。Ready1=首段起手、Ready2=后续段起手（DNF 惯例双 ready，未考证）。
- 冲斩位移：每段约 300px（static[4] 推断），全程 SUPERARMOR（move1/move2 **全帧霸体**，实测帧表）。
- 段间输入窗口："开始斩击后可通过追加操作指定后续攻击方向"——引擎在段间读方向键决定下一段冲向（上/下/左/右，explain"可以上下左右移动着斩击"）。
- 光剑感电：习得光剑精通一定等级 + 光剑系武器时终结技附感电（explain；条件判定与 ACTIVESTATUS_LIGHTNING 注入在引擎/被动侧，pvf 无脚本）。
- move1 F1 有 `SET FLAG 65534`（取消/命中标记，语义未考证，064 同款）。

### 2.4 被动对象

**无本技能专属 PO**（passiveobject.lst 与 passiveobject\character\swordman\ 均无 rapidmoveslash；命中走角色 .atk 45/46 + 引擎施加的武器判定）。唯一 PO 引用是 mod 注入的共享 24370（§2.2，不实现）。

### 2.5 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/rapidmoveslashready1.ani` | 2 | 160ms | 无 | 无 | 首段起手；sm_body |
| `rapidmoveslashready2.ani` | 2 | 160ms | 无 | 无 | 后续段起手（推断） |
| `rapidmoveslashmove1.ani` | 5 | 500ms | F1=65534 | 无 | **全帧 SUPERARMOR**；冲斩本体 |
| `rapidmoveslashmove2.ani` | 5 | 500ms | 无 | 无 | **全帧 SUPERARMOR**；终结上斩 |
| `effect/.../rapidmoveslash/move1.ani` | 4 | 240ms | 无 | — | 冲斩风特效 |
| `move2.ani` | 4 | 500ms | 无 | — | 终结斩风 |
| `slash1.ani` / `slash2.ani` | 4 / 4 | 400 / 400ms | 无 | — | 斩击弧光 |
| `lightready1-x / lightready2-x`（x=1-6） | 各 2 | 各 160ms | 无 | — | **光剑专用**起手特效 ×12（武器差异视觉） |
| `lightmove1-x / lightmove2-x`（x=1-6） | 各 5 | 各 500ms | 无 | — | **光剑专用**冲斩特效 ×12 |
| `rmatka_eff_00-03` / `rmatkb_eff_00-05` | 5-11 | 500-680ms | 无 | — | 残影拖尾（a/b 两组） |
| `smoke_eff_00.ani`（+ .als） | 8 | 520ms | 无 | — | 烟尘；.als 叠加 smoke_eff_01.ani（[none effect add] 层 -1） |
| `dust.ani` | 5 | 450ms | 无 | — | 引 HardAttackCharge/dust.img |

`.als` 边车：仅 effect 侧 `smoke_eff_00.ani.als`（实测）；角色 .ani 无边车。
`[pvp]` 变体：rapidmoveslashmove1/2.[pvp].ani 存在（PvP 专用节奏）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | RapidMoveSlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\RapidMoveSlash.skl` | ✅ | 技能数据 |
| 注册行 | swordman_load_state.nut 93/132 行 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 39 绑定技能 72 |
| 主 nut | RapidMoveSlash.nut | `…\pvf\sqr\character\swordman\rapidmoveslash\RapidMoveSlash.nut` | ✅（53 行，mod 混淆壳） | 门禁 + mod 注入；核心引擎内置 |
| .chr 条目 | etc motion #47-50 + etc attack info #45/#46 | `…\pvf\character\swordman\swordman.chr` 1020-1023 / 1339-1340 行 | ✅ | Ready1/2、Move1/2.ani；RapidMoveSlash1/2.atk |
| 角色 .ani | rapidmoveslashready1/2、move1/2.ani（+[pvp]×2） | `…\pvf\character\swordman\animation\` | ✅ | 帧表见 2.5 |
| 角色 .atk | RapidMoveSlash1.atk / RapidMoveSlash2.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 冲斩/终结命中反应 |
| PO 定义 | —（不存在） | `…\pvf\passiveobject\` | ⛔ 无专属 PO | 命中走角色 atk + 引擎武器判定 |
| 特效 .ani | move1/2、slash1/2、light×24、rmatka/b、smoke×2、dust | `…\pvf\character\swordman\effect\animation\rapidmoveslash\` | ✅ 实测（42 个 .ani + 1 .als） | 引擎绘制特效（无引用者） |
| 装备层 | rapidmoveslashready1/2、move1/2.ani | `…\pvf\equipment\character\swordman\avatar\`（belt_a 实测 4 件） | ✅ | 换装图层（demo 不需要） |
| 关联强化 | RapidMoveSlashEx.skl（技能 152） | `…\pvf\skill\Swordman\RapidMoveSlashEx.skl` | ✅ 实测 | E 类批次另行分析 |
| 关联被动 | 光剑精通（感电条件） | `…\pvf\skill\Swordman\`（LightSwordMastery 系，未定位） | 未考证 | 光剑感电门 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集 | 必需（共享） | ✅ 已在库 |
| RapidMoveSlash/move1.img 等（move/slash 系 img） | sprite_character_swordman_effect_rapidmoveslash.NPK | 冲斩/弧光特效 | 可选 | ❌ |
| RapidMoveSlash/rmatka/b 系 img | 同上 | 残影拖尾 | 可选 | ❌ |
| RapidMoveSlash/Add_Floor / Add_Line / Add_Smoke / face / m-02 / s-01 / s-02.img | 同上 | 烟尘/速度线/面部特效 | 可选 | ❌ |
| WeaponCombo/sword_light_*.img ×5（glow_under/upper、motionblur_under、light_under/upper） | sprite_character_swordman_effect_weaponcombo.NPK | **光剑**光效（light×24 特效的图源） | 可选（demo 不做光剑分支） | ❌ |
| HardAttackCharge/dust.img | sprite_character_swordman_effect_hardattackcharge.NPK | 尘土 | 可选 | ❌ |
| TripleSlash/05.img | sprite_character_swordman_effect_tripleslash.NPK | 跨目录复用特效（L14 常态） | 可选 | ❌ |
| （smoke_eff_00.ani 某帧 IMAGE 为空路径） | — | 空占位帧 | — | — |

缺失 img：必需级 **0**（角色动画单图集已在库，本技能判定不依赖特效）；可选级 10+ 张，分属 4 个 NPK。demo 可零 img 起步（纯角色动画+固定盒），特效后补。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/RapidMoveSlashSkill.cs : SkillLogic`**（ReleaseWave 位移范式 + SubState 段机）
   - `CooldownMs = 20000`（DNF 原值）；**`ManualCooldown = true`**（skl [auto cooltime apply]=0 → CD 自技能结束起算，DNF 实证映射）；`TotalTimeMs = 3300`（自管理：3×(ready 160+move 500) + 终结 660 + 余量）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanRapidMoveSlashReady1)`、`ctx.ClearHitTargets()`、SubState=0。
   - `OnUpdate` 段机（每段：起手帧满 → PlayAnim(Move) + `ctx.SetAttackHitbox(前偏1.2, 半尺寸(1.5,5/10,8/10))`（引擎施加无数据，按冲斩盒惯例取值）+ 段内位移 `MoveCasterForward(3单位×dt/500ms)`（static[4]=300px 推断）；段末 `ctx.DisableAttackHitbox()` + `ctx.ClearHitTargets()`（**段间重置=L19 已通**）→ 下一段 PlayAnim(Ready2/Move1)；
     末段（第 4 段）PlayAnim(Move2) 且不设固定盒、改 `ctx.CreateAreaInFront(AreaIds.RapidMoveSlashFinal, FP.Zero)`（终结上斩区，终结反应走 Area）。
   - `HitReaction`（技能级=冲斩段）：`{Damage=100, HitstunMs=500, KnockbackX=200, LaunchY=0}`（atk1 原值 push200/hit down）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - 段间方向追加：**不做**（技能中方向输入读取缺口，R1-A3 已记档）——4 段固定前冲。
2. **`DotNet~/Areas/RapidMoveSlashFinalArea.cs : AreaDefinition`**（BloodBoomArea 范式）
   - `TotalTimeMs=400`、`EnterActions={MeleeHit}`、`HalfExtents=(12/10,5/10,10/10)`（终结斩近身盒）、
     `HitReaction{Damage=150, HitstunMs=800, KnockbackX=100, LaunchY=400}`（atk2 原值 down 反应/push100/**lift400=hit lift up 浮空上斩**，releasewave as-built §5.6-3 击飞同构）、`ViewAnimId=AnimId.RapidMoveSlashMove2Effect`（可选）。
3. 无需新 Buff/Action（MeleeHit 现成；感电不做）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 39 + Ready1/2→Move1/2 循环 | SubState 段机 + 每段 PlayAnim（帧号 const） |
| 每段 300px 冲斩（static[4]） | `MoveCasterForward` 纯函数位移（ReleaseWave 同构） |
| 角色无攻击盒、引擎施加武器判定 | `SetAttackHitbox` 固定盒（NormalAttack 同构） |
| 段间重新可命中（引擎 resetHitObjectList） | 段末 `ClearHitTargets()`（L19：段间多段已通） |
| 终结 atk2 浮空上斩 | `RapidMoveSlashFinalArea` + HitReaction LaunchY=400（LSFlight 抛物线） |
| [auto cooltime apply]=0 | `ManualCooldown=true` |
| 段间方向追加 | 缺口（技能中方向输入），固定前冲替代 |
| 光剑感电 | 缺口（元素状态+被动门+武器类型差异化），跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `…\Runtime\SkillIdAttribute.cs` | `SkillIds.RapidMoveSlash = 20` + 按键 case |
| AreaId | `…\Runtime\AreaDefinition.cs` | `AreaIds.RapidMoveSlashFinal = 10`（A12 段） |
| AnimId | `…\npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanRapidMoveSlashReady1=82`、`Ready2=83`、`Move1=84`、`Move2=85`（特效可选 86+：slash1/move1/rmatk 等） |
| json/图集/按键 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem | 角色 4 json；图集零新增（sm_body 已在库）；新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms（结束���算） | 20000 + ManualCooldown |
| 段数 | static[0]=4（斩击次数上限） | 4（3 冲斩 + 1 终结） |
| 每段节奏 | ready 160 + move 500（推断） | 直用 |
| 每段位移 | static[4]=300px（pvp 400，推断） | 3 单位 / 500ms 匀速 |
| 冲斩伤害 | col0 265%→1953%（物理武器基数） | MeleeHit 固定 100 |
| 冲斩反应 | atk1：damage/push200/hit down/cut/blood15 | Damage100/Hitstun500/Kb200/Ly0 |
| 终结伤害 | col1 529%→3906% | 150 |
| 终结反应 | atk2：down/push100/lift400/hit lift up | Kb100/Ly400/Hitstun800 |
| 霸体 | move 全帧 SUPERARMOR | 不做（延后） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| rapidmoveslashmove1.ani | `[SET FLAG]`（F1=65534）、`[DAMAGE TYPE] SUPERARMOR`（全帧） | 既有约定跳过（触发帧 const 进技能类）；霸体帧若立项需 AnimFrameData 加 damageType（既有记档） |
| effect 侧 42 个 .ani | `[SHADOW]`；`[LOOP]`（move1.ani 循环）；`[IMAGE ROTATE]`（rmatkb_eff_02 等残影层）；`[INTERPOLATION]`（move2 等 16 处族） | [SHADOW]/[IMAGE ROTATE] 记档跳过；[LOOP] 已支持；**[INTERPOLATION] 不在任何清单——本批上报**（060 §8 同条） |
| light* 特效 | `[GRAPHIC EFFECT]`（LINEARDODGE，52 文件族） | **已支持**（L15），非缺口 |
| smoke_eff_00.ani.als | [use animation] + [none effect add] | **均已支持**（README 规则表），无缺口 |
| `.skl` / `.atk` ×2 | 尚无子命令 | 手抄 ~15 值可行；累计记档 |
| （72-effect 扫描见 1 帧 IMAGE 空路径） | 空占位帧 | 现有规则可处理（path=""） |

结论：**.ani/.als 全部可被现有子命令翻译**；无本技能新增翻译缺口。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 段间上下左右追加方向 | **技能中方向输入读取**（R1-A3 缺口累计） | 4 段固定前冲（手感差异：原版可折返连斩） |
| 普攻状态 8 中可施放（[executable states]） | 在技中不能施放（SkillCastHelper 门禁）+ 技能取消体系缺失（064 上报） | 独立施放 |
| 全程霸体（move1/2 全帧 SUPERARMOR） | 霸体帧延后（§6.3） | 不做（冲斩中可被打断，可用"快速位移"补偿手感） |
| 光剑精通≥等级 + 光剑 → 终结感电 | 被动技能系统缺失 + 武器类型差异化（R2-A6）+ 元素状态 | 跳过；后续可加 ElectrocutionBuff（BurnBuff 同构 tick 伤）作常驻简化 |
| 冲斩伤害按物理%结算 | 属性数值无伤害消费链（R1-A4） | MeleeHit 固定值 |
| 残影/烟尘/光剑特效（引擎绘制） | 延后 | 角色动画自带动作，特效后补 overlay |
| flag 65534 | 未考证 | 忽略 |
| 音效（JAKYEOL_HIT） | 延后 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 除 [0]=4、[4]=300（推断）外 5 值语义（1/2000/-5000/-1000/0；引擎消费无脚本）。
2. Ready1/Ready2 与 Move1/Move2 的确切编排（首段/后续段/终结的分配为 DNF 惯例推断）；每段是否重摆 ready。
3. 感电的概率/等级/时长参数（在引擎/被动侧，skl 无列）。
4. 光剑精通技能 skl 的确切文件（未定位）。
5. mod onAfterSetState 的 vector[0]==1 来源路径（mod 内部，不实现）。

**新系统级缺口上报**：无新增（方向输入/技能取消/霸体/武器差异/属性消费链均在缺口累计中；感电若做简化版走 BurnBuff 同构，无需新机制）���

**翻译工具缺口**：无新增。
