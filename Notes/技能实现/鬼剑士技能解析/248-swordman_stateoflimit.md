# 极·神剑术（swordman_stateoflimit）

> 技能ID 248 | 级别 C（二觉被动，剑魂） | 可实现性 🔶（分半：数值半 ⛔——增伤 col24 撞属性消费链；联动半 ✅/🔶——七个宿主技的追加打击 PO 全部落在共享 PO 24370，数据链完整可直译，gate 需烘焙；霸体撞延后档） | 分析日期 2026-08-22 | 批次 C5

**本技能是 109/234/235/049/068/072/073 七份既有文档反复引用的"stateoflimit 联动"本体**——此前各文档把消费钩子判为"mod 注入、记档不还原"；本批走读本体后结论修正：**248 是 lst 在册技能，其 explain 声明的七项联动与七处消费钩子一一对应**（本 pvf 的 mod 作者用"标记 appendage + 宿主技钩子"实��了官方二觉被动的全部机制）。回链见 §8。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极 · 神剑术 | `skill\swordman\swordman_stateoflimit.skl` [name] |
| 英文名 | swordman_stateoflimit（取 skl 文件名；无 [name2]；消费端动画/目录名 stateoflimit） | 同上 |
| 职业 | 剑魂·剑神（二觉被动；注册行 gate `growtype==1`，L17 映射 1=剑魂） | `passive_skill_swordman.nut:7` |
| 学习等级 | 75 | skl [required level] |
| 最高等级 | 35（[second growtype maximum level] 第 3 槽 = 30） | skl [maximum level] / [second growtype maximum level] |
| 类型 | 被动（[passive]，skill class 1） | skl [type] |
| 指令 | —（纯被动） | — |
| 特殊消耗 | 无（[durability decrease rate] 60——武器耐久加速，无耐久系统跳过） | skl 实测 |
| 一句话效果 | 剑术极致化：全技能增伤；流心系施放得布万加霸体；后跳斩出西岚剑气；猛龙断空斩出巴恩飓风；破军升龙击出阿甘左暴风式；幻影剑舞触发索德罗斯幻影；破空斩可触发极限之十字斩；流星落追加流星之天剑（按武器分支异常） | skl [explain] 全文 |

**level property（24 项全明，25 列 level info，L21 向量法；Lv1 → Lv35）**：

| # | 模板项 | 列 | Lv1 | Lv35 | 消费端实证 |
|---|---|---|---|---|---|
| 1 | 基本攻击力和技能攻击力增加 | col24×0.1 | 14% | 80% | 无脚本消费者（引擎） |
| 2 | 西岚的剑气攻击力 | col0 | 1737% | 5455% | backstep.nut（后跳斩） |
| 3 | 布万加的霸体持续时间 | col1×0.001 | 1.0s | 4.4s | flowmind one/two/three.nut |
| 4 | 暴风式冲击波攻击力 | col2 | 3861% | 12123% | chargecrash.nut |
| 5 | 飓风攻击力 | col3 | 3861% | 12123% | rapidmoveslash.nut |
| 6 | 幻影斩击攻击力 | col4 | 2872% | 9015% | illusionslash.nut（450ms 时件） |
| 7 | 幻影剑气攻击力 | col5 | 3861% | 12123% | illusionslash.nut（改写 20037 倍率） |
| 8 | [幻影剑舞]钝器最后一击攻击力 | col6 | 3861% | 12123% | illusionslash.nut（钝器替换终结） |
| 9 | 流星之天剑攻击力 | col7 | 3861% | 12123% | ap_meteorsword_stateoflimit |
| 10 | 流星之天剑投掷数量上限 | col8 | 3 | 3（恒定） | 同上（timer 间隔换算） |
| 11 | 流星之天剑控制持续时间 | col9×0.001 | 1.25s | 1.25s（恒定） | 同上 |
| 12 | [巨剑]流星之天剑追加攻击力 | col10 | 387% | 2801% | 同上 case 3 |
| 13 | [短剑]束缚几率/持续时间 | col11 / col12×0.001 | 57% / 1.0s | 91% / 4.4s | 同上 case 0 → ACTIVESTATUS_HOLD |
| 14 | [钝器]眩晕几率/持续时间/地面冲击波 | col13 / col14×0.001 / col15 | 57% / 1.0s / 2587% | 91% / 4.4s / 8122% | 同上 case 2 → ACTIVESTATUS_STUN |
| 15 | [太刀]刺伤几率/持续时间/攻击力 | col16 / col17×0.001 / col18 | 57% / 1.0s / 1247 | 91% / 4.4s / 4035 | 同上 case 1 → ACTIVESTATUS_BLEEDING |
| 16 | [光剑]感电几率/持续时间/攻击力/地面冲击波 | col19 / col20×0.001 / col21 / col22 | 57% / 1.0s / 1247 / 2018% | 91% / 4.4s / 4035 / 6336% | 同上 case 5 → ACTIVESTATUS_LIGHTNING（脚本实写 col16-18，与 col19-21 数值恒等，等效复用，见 §2.2-6） |

**col23（240→920）无任何模板项与脚本引用**——按 explain"极限之十字斩"疑为其倍率列，但 234 的十字斩实测用 234 自身列（§2.2-7），col23 判**未考证/疑 mod 残留**。

## 2. 技能逻辑走读

### 2.1 注册与挂载（标记型 appendage）

```
// sqr\character\swordman\passive_skill_swordman.nut case 248（引擎回调 ProcPassiveSkill_Swordman）
append = "character/swordman/appendage/ap_stateoflimit.nut"
if (skill_level > 0 && sq_getGrowType(obj) == 1)      // 剑魂
    sq_AppendAppendage(obj, obj, 248, false, append, true)
else
    sq_RemoveAppendage(obj, append)
```

**appendage 本体是空壳**（`…\swordman\appendage\ap_stateoflimit.nut`，7 行，仅空 sq_AddFunctionName；`…\swordman\beidong\` 下另有同名副本，proc/onStart 也全为空桩）。即 ap_stateoflimit **只当"已习得"标记**，全部行为在消费端按 `sq_IsAppendAppendage(obj, "…ap_stateoflimit.nut")` 字符串判定——与 171 的 ap_buff_171 同构（标记型二觉被动双实证）。

### 2.2 七个消费端（白名单 grep `stateoflimit` 全命中集，逐个实读）

**1）流心系霸体（布万加）**——`swordman\flowmind\flowmindone.nut:13` / `flowmindtwo.nut:8` / `flowmindthree.nut:13`（⚠ 此目录为 mod 混淆树，C3）：

```
进入流心状态（子状态 0）且挂标记：
  sq_SetSuperArmorUntilTime(obj, sq_GetLevelData(248, 1, level))   // col1 原值直用 = 1000→4400ms 霸体
```

（109 文档已引用本行；flowmindthree 另有 datas[1]==0 重推状态 64 的 mod 逻辑，与本技能无关。）

**2）后跳斩→西岚剑气**——`swordman\backstep\backstep.nut:12`（进入状态 49 时）：

```
写包(248, subType=1, sq_GetBonusRateWithPassive(248, state, 0, 1.0))   // col0 剑气倍率
sq_SendCreatePassiveObjectPacketPos(24370, 0, 自身X, 自身Y-1, 0)       // 脚下生成共享 PO
```

**3）破军升龙击→阿甘左暴风式**——`swordman\chargecrash\chargecrash.nut:12`（子状态 1/0 冲撞起手）：

```
写包(248, subType=2, col2 倍率)；sq_SendCreatePassiveObjectPacket(24370, 0, 30, -1, 0)   // 身前 30px
```

**4）猛龙断空斩→巴恩飓风**——`swordman\rapidmoveslash\rapidmoveslash.nut:11`（进入状态 39/子状态 1）：

```
写包(248, subType=3, col3 倍率, sq_GetDelaySum())                      // 第 4 参=技能动画总时长
sq_SendCreatePassiveObjectPacket(24370, 0, 0, 0, 0)                    // 生成于自身位置
```

**5）幻影剑舞→索德罗斯幻影**——`swordman\illusionslash\illusionslash.nut:8/27/71`（onAfterSetState 起 700ms 循环时件 + 450ms 单次时件 + onProc 武器分支）：

```
时件 0（每 700ms）：sq_AddDrawOnlyAniFromParent(state_of_limit_illusion_1~4.ani 随机)   // 幻影视觉
时件 1（450ms）：写包(248, subType=4, col4 倍率) → PO 24370 @自身                       // 幻影斩击打击
onProc：追踪幻影剑舞本体 PO 20037——
  钝器(weaponSubType==2)：销毁 20037，写包(248, subType=5, col6) → PO 24370 @前 180px（暴风式终结替换）
  其他武器：改写 20037 动画(自定义槽 2)+攻击信息(槽 0)，倍率=col5（幻影剑气强化）
```

**6）流星落→流星之天剑**——`swordman\meteorsword\meteorsword.nut:397-402`（subState 4 落剑期挂 `ap_meteorsword_stateoflimit.nut`）：

```
ap onStart：timer 间隔 = 235 col6 × 235 col7 / 248 col8（按投掷上限 3 把摊节奏）
ap proc（仅 state 235 且时件在场）：按攻击键(X)且踩 timer 节拍 →
  读方向输入 ±120px 偏移落点；
  写包(248, subType=6, weaponSubType, 映射型[1,3,4,2,-1,5][weaponSubType],
        武器分支参数——短剑0: col11/col12；太刀1: col16/17/18；钝器2: col13/14/15；
        巨剑3: 无(倍率+=col10)；光剑5: col16/17/18+col22,
        基础倍率 col7（经 235 列名取数）, 控制时长 col9)
  → PO 24370 @落点, z=1000（自天而降）
⚠ 光剑支实写 col16-18（模板标太刀刺伤列）——col16-18 与 col19-21 每级数值恒等（57/1000/1247→91/4400/4035），行为等效，判数据冗余复用。
```

**7）破空斩→极限之十字斩**——`swordman\swordofmind\swordofmind.nut:145`（创建 234 主 PO 时）：

```
写包尾附 sq_WriteBool(挂有 ap_stateoflimit)      // 掌握标志随 234 主 PO 下发
→ PO case 234 state 11 爆炸段：掌握=true 时找 HP 最高标记敌人传送到其位置，播 cross_eff.ani
  （atk #11 SwordOfMindThirdPhase.atk，down/lift150——倍率用 234 自身列，详见 234-swordofmind.md §state 11）
```

### 2.3 共享 PO 24370 的 case 248 分派（`sqr\common_object\share_obj\swordman\`，L20/F7 链路）

**setcustomdata.nut:650** 按 subType 装配（动画/攻击信息全部命中 `passiveobject.lst:9-10` 指向的 mod obj `…\qq506807329new_swordman_24370.obj`，0 基直读）：

| subType | 动画（etc motion 槽） | 攻击信息（etc attack info 槽） | 装配要点 |
|---|---|---|---|
| 1 西岚剑气 | `stateoflimit\state_of_limit_backstep_01.ani`（直读路径） | 48 `StateOfLimitSwordWave.atk` | 倍率 col0；`sq_SetMoveParticle("particle/stateoflimit.ptl", 700)` **飞行剑气**（粒子速度 700） |
| 2 暴风式 | `state_of_limit_crash_05.ani` | 49 `StateOfLimitSwordLastEffect.atk` | 倍率 col2 |
| 3 飓风 | （setstate 装，见下） | 50 `StateOfLimitSpin.atk` | 存出生点+delaySum → setState 10 |
| 4 幻影斩击 | `illusionslash\state_of_limit_illusion_wind_meele_0X.ani`（随机 0-3） | 51 `StateOfLimitShadow.atk` | 倍率 col4；移粒子 |
| 5 幻影剑气终结 | `illusionslashsub\1_shockwave_dodge.ani`（底层）+ 4 个 draw-only（`2_ground_dodge .ani`⚠文件名含空格实测在册、3_sword、4_attackt、5_light）+ 粒子 | 52 `IllusionSlashSub.atk` | 倍率 col6（钝器终结） |
| 6 流星之天剑 | `meteorsword\bigswordboom<weaponSubType>_sword_inner.ani` | 53 `StateOfLimitMeteorSword.atk` | 按 weaponSubType 写异常：0=HOLD(col11/12)、1=BLEEDING(col16/17/18)、2=STUN(col13/14,+col15 冲击波)、5=LIGHTNING(col16/17/18,+col22 冲击波)；末两参=倍率 col7、强锁硬直 col9 |
| 7 落剑冲击波 | 钝器=`chargecrash\damage-back.ani` / 光剑=`meteorsword\lightswordshockwave.ani` | 54 `StateOfLimitMeteorSwordShockWave.atk` | 由 subType 6 落地时二段创建（procappend:210） |

**行为侧**：procappend.nut:210——subType 3 飓风 state 10：`sq_GetAccel` 从出生点**追施法者当前位置**移动 delaySum ms，到点 setState 11；subType 6 流星剑：z 从 1000 匀速降到 0（300ms）→ 换 `bigswordboom<N>_sword.ani` 爆发 + 钝器/光剑补 subType 7 冲击波。setstate.nut:607——飓风 state10=`state_of_limit_dragon_02.ani`、state11=`state_of_limit_dragon_vanish_02.ani`。onendcurrentani.nut:250——subType 2/3/5/6/7 播完即毁（1/4 走移粒子生命周期，未细读）。

### 2.4 判定参数实测（PO .atk 逐文件）

| atk | 反应 | push/lift | 备注 |
|---|---|---|---|
| StateOfLimitSwordWave | damage | 300/200 | blow、hit horizon、no blood 50@1.0、front、音效 R_DARK_KNIGHT_HIT |
| StateOfLimitSwordLastEffect | down | 300/200 | — |
| StateOfLimitSpin | none | 300/200 | 无受击反应（持续牵引型） |
| StateOfLimitShadow | damage | 0/200 | hit lift up（浮空） |
| IllusionSlashSub | down | 100/250 | — |
| StateOfLimitMeteorSword | down | 20/50 | — |
| StateOfLimitMeteorSwordShockWave | down | —/300 | — |

全部 physic + weapon damage apply + no element。

### 2.5 关键帧表（PO 动画，C1 法提取）

| 动画 | 帧数 | 总时长 | 攻击盒（min/max 口径，连续帧） |
|---|---|---|---|
| state_of_limit_backstep_01.ani | 5 | 150ms | F0-F4 `-51,-25,-6 / 92,59,156`（全帧恒定） |
| state_of_limit_crash_05.ani | 7 | 600ms | F0-F3 `-55,-20,0 / 190,40,250` |
| state_of_limit_dragon_02.ani | 8 | 480ms | F0-F7 `-55,-50,1 / 131,100,155` |
| meteorsword\bigswordboom1_sword_inner.ani 等 ×5 | 4 | 未逐帧 | 每武器一把 |
| lightningpower 系（非本技能，250 用） | — | — | — |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_stateoflimit.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\swordman\swordman_stateoflimit.skl` | ✅ 实测（164 行） | 24 项数值全明 |
| 注册行 | passive_skill_swordman.nut:5-14 | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | case 248 挂标记 |
| 标记 appendage | ap_stateoflimit.nut ×2 | `…\swordman\appendage\`（7 行空壳）/ `…\swordman\beidong\`（空桩） | ✅ 实测 | 掌握标记 |
| 常量 | SKILL_SWORDMAN_STATEOFLIMIT <- 248 | `…\swordman\swordman_header.nut:108` | ✅ 实测 | — |
| 消费端 ×7 | flowmindone/two/three、backstep、chargecrash、rapidmoveslash、illusionslash、meteorsword、swordofmind 各 .nut | `…\sqr\character\swordman\` 各子目录 | ✅ 实测（§2.2 行号） | 全部为混淆 mod 层（变量名乱码，C6④） |
| 流星 ap | ap_meteorsword_stateoflimit.nut | `…\swordman\meteorsword\` | ✅ 实测（~110 行） | X 键投剑节奏器 |
| PO 回调 | setcustomdata/setstate/procappend/onendcurrentani.nut | `…\sqr\common_object\share_obj\swordman\` | ✅ 实测 | case 248 subType 1-7 |
| PO obj | qq506807329new_swordman_24370.obj | `…\passiveobject\script_sqr_nut_qq506807329\swordman\`（passiveobject.lst:9-10 定位） | ✅ 实测 | etc motion 74 槽 / etc attack info 59 槽，0 基无错位（F7 第 5 实证） |
| PO .atk ×7 | stateoflimit*.atk / illusionslashsub.atk | `…\script_sqr_nut_qq506807329\swordman\AttackInfo\` | ✅ 实测 | §2.4 |
| PO 动画 | stateoflimit\*.ani（含 illusionslash\、meteorsword\ 子目录、illusionslashsub\） | `…\script_sqr_nut_qq506807329\swordman\Animation\` | ✅ 实测（40+ 文件） | 七类打击视觉 |
| 粒子 | stateoflimit.ptl / illusionslashmelee.ptl / illusionslashsub.ptl | `…\script_sqr_nut_qq506807329\swordman\` 引用 | ✅ 引用实证 | L5 统一跳过 |
| 图标 | SkillIcon.img 428/429 | skl [icon] | ✅ 引用实证 | demo 不需要 |

## 4. 资源需求

img 推导 NPK：`sprite_<路径下划线化>.NPK`；关键 ani 的 IMAGE 引用已逐文件提取。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| Character/Swordman/Effect/StateofLimit/state_of_limit_Crash.img | sprite_character_swordman_effect_stateoflimit.NPK | 暴风式（subType 2） | **必需** | ❌ |
| Character/Swordman/Effect/StateofLimit/state_of_limit_dragon.img | 同上 | 飓风移动/消散（subType 3） | **必需** | ❌ |
| Character/Swordman/Effect/IllusionSlash/slash_shadow.img | sprite_character_swordman_effect_illusionslash.NPK | 幻影斩击（subType 4） | **必需** | ❌ |
| Character/Swordman/Effect/IllusionSlash/particle.img | 同上 | 西岚剑气（subType 1 主层） | **必需** | ❌ |
| Character/Swordman/Effect/MeteorSword/meteorsword_swords_whole.img | sprite_character_swordman_effect_meteorsword.NPK | 流星之天剑 ×5 武器（subType 6） | **必需** | ❌ |
| Monster/Spider/SpiderBossSpark.img | sprite_monster_spider.NPK | 光剑落剑冲击波（subType 7；**跨 Monster 树复用**，L14 常态） | 可选 | ❌ |
| Character/Swordman/Effect/IllusionSlash/finish/1_shockwave_dodge.img | sprite_character_swordman_effect_illusionslash_finish.NPK | 钝器终结（subType 5 主层；4 个 draw-only 同目录另需 finish 系 img，未逐一提链） | 可选 | ❌ |

缺失 img：必需 5 张（三个 NPK 一次提取）+ 可选 2+ 张。v2/v4 由提取时把关。

## 5. 实现方案草案（联动半；数值半 ⛔ 见 §7）

**核心思路**：本技能自身无 SkillLogic（无施放）；联动效果做进**七个宿主技能**，gate 用"掌握标志"烘焙（demo 常开或角色挂 `StateOfLimitBuff` 后硬编码查询）。

- **内容件清单**（全部继承真实基类；分两档）：
  1. `StateOfLimitBuff : BuffDefinition`——`TotalTimeMs=0`（永久）+ `AddActions={AddOwnerNumeric(NumericType.Attack, +80)}`（占位零消费，诚实标注）。demo 以"游戏开局自动挂载"或首技施放时挂载替代"习得事件"（被动技能系统缺失）。
  2. 七个宿主技增量（各自文档已有主实现，此处只列追加项）：
     - 流心 刺/跃/升 SkillLogic OnCast：追加霸体——撞"霸体帧"延后档，跳过（§7）。
     - 后跳斩 SkillLogic：命中帧追加 `ctx.CreateArea(AreaIds.SilanSwordQi, 0)`——`SilanSwordQiArea : AreaDefinition`（同 BloodBoomArea 范式）：`TotalTimeMs=150`、`EnterActions={MeleeHit}`、`HitReaction{Damage=按 col0 demo 120, HitstunMs=500, KnockbackX=300, LaunchY=200}`（atk48 原值）、`HalfExtents=(0.7,0.4,0.8)`（盒 143×84×162px 折算）、`ViewAnimId=AnimId.StateOfLimitBackstep`。
     - 破军升龙击：冲撞帧追加 `GanZuoStormArea`（atk49 down/300/200，盒 245×60×250px → 1.2,0.3,1.25）。
     - 猛龙断空斩：`BaenTornadoArea`——飓风跟随段简化为**冲撞路径上的 stationary Area**（Area 跟随施法者 R4-B17 缺口；或逐拍建区过渡范式，243 先例）。
     - 幻影剑舞：施放后 450ms 追加 `SoderosPhantomArea`（atk51 lift200 浮空）+ 700ms 周期幻影视觉（overlay 重播，084 先例）；钝器终结替换为 `IllusionSubFinishArea`（atk52 down/100/250）。
     - 流星落：落剑期 X 键追加投剑——`MeteorSwordFallBullet : BulletDefinition`（z=10 高点创建、300ms 垂直落下 → `ViewEndAnimId` 撞缺口，落地帧改 Area 爆发 atk53）+ 钝器/光剑二段 `MeteorShockArea`（atk54 lift300）。武器分支异常走 `HitReaction.ProcBuffId`（HOLD 无对应 Buff，STUN 有 `BuffIds.Stun`，BLEEDING 有 `BuffIds.Bleed`，LIGHTNING 见 250 文档新增 LightningBuff）。
     - 破空斩：爆炸段 gate 查询掌握标志（demo 常开）→ 追加十字斩 Area（atk #11 数据在 234 文档已记）。
- **概念映射**：标记 appendage → `StateOfLimitBuff`；宿主钩子 `sq_IsAppendAppendage` → 宿主 SkillLogic 内 gate（烘焙常量）；PO 24370 subType → 七个 AreaDefinition/BulletDefinition；`sq_SetMoveParticle(700)` 飞行剑气 → BulletDefinition（速度 7px/ms≈0.07 单位/ms，demo 0.5 单位/100ms 同 NormalWaveBullet 档）。
- **注册点**：`BuffIds.StateOfLimitBuff = 18`；`AreaIds` 自 39 顺延 5 个；`AnimIds` 自 178 顺延（Backstep01=178、Crash05=179、Dragon02=180、WindMeele=181、MeteorInner=182 五档武器可共用一套）。
- **关键数值表（demo 建议值）**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 霸体时长 | col1 = 1000→4400ms | 跳过（延后档） |
| 剑气倍率 | col0 = 1737→5455% | MeleeHit 120 |
| 暴风式/飓风/幻影倍率 | col2/3/5/6 = 3861→12123% | 130 |
| 幻影斩击 | col4 = 2872→9015% | 110 |
| 流星剑 | col7 = 3861→12123%（+col10 巨剑 387→2801%） | 150 |
| 异常几率/时长 | 57→91% / 1.0→4.4s | 50% / 2s（Stun/Bleed 现值对齐） |
| 增伤 | col24×0.1 = 14→80% | 占位 +80 零消费 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_stateoflimit.skl | `.skl` 无子命令（25 列 × 35 行 + 24 向量） | 本文档已手抄全明；`skl` 子命令同前议 |
| 7 个 .atk | `.atk` 无子命令 | 手抄 7×~6 值（§2.4 已全列）；批量化提级 |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | 本批已用 python 直读槽位；`obj` 子命令按"相位序列"建模建议不变（064 首议） |
| stateoflimit 各 .ani | 仅 [SHADOW]（6 文件实测枚举，节名全集=FRAME/LOOP/DELAY/IMAGE/ATTACK BOX/SHADOW） | 常规节全可译；SHADOW 既有跳过 |
| 4 个 .ani.als（backstep_01/crash_05/dragon_vanish_02/bigswordboom*_inner） | 预期为 [use animation]/[add] 常规（未逐字节核对——073 文档记过 stateoflimit 叠层 als 有"引用 ani 缺失"降级案例） | 常规可译；缺失引用按 R3-A13 降级惯例 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 全技能增伤 14%→80%（col24） | **缺失档：属性数值消费链** | ⛔ 数值半主因；NumericType 占位 |
| 流心系霸体 1.0→4.4s | 霸体帧（延后档） | 跳过 |
| 飓风追施法者移动 delaySum ms | Area 跟随施法者（R4-B17 缺口邻近） | stationary Area 或逐拍建区（243 过渡范式） |
| 流星剑 z=1000 落下 + 方向键控落点 | 施法者 z 主动位移姊妹缺口（R3-A15）+ 技能中方向输入（R1-A3） | 落点固定前偏；Bullet 斜落简化为原地 Area 爆发 |
| X 键节奏投剑（3 把，timer 门控） | 二段交互门面（R4-B16，第 5 例） | 单次追加投剑（1 把，固定延迟） |
| 掌握判定（appendage 查询） | 自身 Buff 查询门面（R4-B18）+ 被动技能系统 | 烘焙常开/角色常驻 Buff 硬编码 |
| 武器分支异常（束缚/刺伤/感电） | HOLD 无对应 Buff；武器差异化缺失（R2-A6） | 钝器→Stun、太刀→Bleed 现成；短剑/光剑砍 |
| 移粒子剑气（速度 700 飞行） | 粒子系统（L5） | BulletDefinition 直译飞行 |

## 8. 存疑与缺口上报

**未考证项**：①col23（240→920）无引用（疑极限之十字斩倍率残留）；②光剑支写 col16-18 与模板列名错位（数值恒等已证等效，命名归属未证）；③subType 1/4 的销毁路径（onend 清单未含，移粒子生命周期未细读）；④col8=3 投掷上限在 ap 里如何参与 timer 摊算（读了换算式 `235col6×235col7/248col8`，语义为节奏间隔，未逐参验证）；⑤[second growtype maximum level] 槽位与职业的映射表（3=剑神/9=天帝/11=夜见罗刹，本批三技能反推）。

**对既有文档的回链修正（主循环裁定）**：
- 049/068/072/073 四文档把本技能的消费钩子判为"纯 mod 注入、非原版、不还原"——**建议改为"mod 实现的官方二觉被动联动，还原优先级随 248 联动半立项提升"**：钩子行为与 248 的 explain 七项一一对应，不是 mod 杜撰技。073 文档 §8 的"mod 混淆壳完整样本"结论不变（nut 确为混淆层）。
- 109 文档 §2.2"若挂 ap_stateoflimit → 霸体"即本技能消费端 1，数值 col1=1000→4400ms 现已对表（原文"等级数据"未给值）。
- 235 文档 §"追加 ap"即消费端 6，投剑参数本批已全解（§2.2-6）。
- 234 文档 §state 11 即消费端 7 的掌握 bool，链路闭合。

**新系统级缺口**：无新缺口（霸体/跟随/二段交互/自身 Buff 查询/属性消费链全部在案）；本批贡献是**二觉被动标准形态定案**——"标记 appendage + 宿主技钩子 + 共享 PO 24370 承载"，171/248 双实证，后续 C 类二觉被动（C2 批 254 唤醒等）可按此链路直查。

**给下轮的经验**：查"某被动影响某主动技"别只看被动自身——grep **appendage 路径字符串**（`sq_IsAppendAppendage`）在 `sqr\character\swordman\` 全子树的命中集就是消费端全集；24370 的 case 248 是本 pvf 里最大的单技能分派表（7 subType），同类还有 case 85/234/235，读 setcustomdata 按 dword 顺序对写包端即可零错位还原。
