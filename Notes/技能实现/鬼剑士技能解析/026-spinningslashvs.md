# 幻鬼 : 回天（spinningslashvs）

> 技能ID 26 | 级别 A | 可实现性 🔶（幻鬼实体接力与剑术中即时施放两大族级缺口，主干两段攻击可完整表达） | 分析日期 2026-08-22 | 批次 A13

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻鬼 : 回天 | `skill\Swordman\ghostsword\spinningslashvs.skl [name]` |
| 英文名 | spinningslashvs（取 skl 文件名；本 skl 无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype]=5，L17；ghostsword 族） | 同上 |
| 学习等级 | 40 | 同上 [required level] |
| 最高等级 | 60（growtype0/5 段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] / [skill class] |
| 指令 | ↓↓→ + Z（指令施法 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 20000 ms（pvp 30000 + 开场 CD 30000） | 同上 [cool time]（dungeon/pvp 两节） |
| MP | 170 → 1428 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 1（consume item 3037 1 1） | 同上 |
| 可施放状态 | 0/8/14/126/127/136/71/45/170/20/22（站立/普攻/受击/六剑术族等——nut 只放行 0/8/14 切状态，其余走"即时幻鬼"路径，见 §2.2） | 同上 [executable states] |
| 前置 | 技能 137（speedslashvs）Lv1 | 同上 [pre required skill] |
| static data | `100 150 0 0 100 600 400 80`（dungeon/pvp 同值） | 同上 [static data] |
| 一句话效果 | 幻鬼发动回旋斩击，对周围敌人造成巨大物理伤害；施放剑术技能过程中使用时，无施放动作立即出现幻鬼攻击敌人 | 同上 [explain] |

**static data 语义**（以共享 PO 脚本消费印证，见 §2.3）：`[0]`=判定/图像缩放率 100%、`[1]`=终结段前移距离 150px、`[2]`=追加回旋开关（0=关；>0 时 state10 后接 12/13 两个追加回旋）、`[3]`=追尾 darkwind PO（id 64）开关（0=关）；`[4..7]=100 600 400 80` 未考证。

**level info（2 列，Lv1 → Lv60）**：col0 回旋斩攻击力 3208→25668、col1 终结一击攻击力 4812→38502（列语义由 PO 脚本 state10=col0 / state11=col1 实证，非推断）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
165: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/spinningslashvs/spinningslashvs.nut", "spinningslashvs", STATE_SPINNINGSLASHVS, SKILL_SPINNINGSLASHVS);
```

- `swordman_header.nut`：`STATE_SPINNINGSLASHVS <- 121`（53 行）、`SKILL_SPINNINGSLASHVS <- 26`（158 行）、`CUSTOM_ANI_SPINNINGSLASHVS <- 300`（471 行）。
- .chr etc motion 0 基 #300 = `Animation/spinningslashvs.ani`（1273 行，实测对位）。
- 攻击判定体 = **F5 unclebang 共享 PO 24349**，写包首 dword = **63**（`passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` + `sqr\shared_passive_object\swordman\*.nut` 六回调分派）。

### 2.2 主 nut 逐回调（spinningslashvs.nut，115 行全读）

- `checkExecutableSkill`（**双路径是本技能的灵魂**）：
  - 状态 0/8/14 → 正常路径：切状态 121 subState 0（有施放动作）。
  - 状态 ∈ {SPIRITMOVE/SPEEDSLASH/GHOSTPIERCE/WHITEGHOSTSLASH/GHOSTDECOLLATION/SWARDDANCEBS}（六剑术进行中）→ **不切状态**（return false，本体动画不中断）：`getVSObject(obj)` 取在场幻鬼实体（= 共享 PO 24349 中 id∈{61,62,63,65,66,68,70,74} 者，jg_swordman_common.nut:897 实测）——在场则在幻鬼位置创建 PO 24349（dword 63）并销毁旧实体（**幻鬼位置接力**）；不在场则在前方 70px 创建。这就是 explain 的"无施放动作立即出现幻鬼"。
- `onSetState` subState 0：播 `CUSTOM_ANI_SPINNINGSLASHVS`（spinningslashvs.ani，4 帧 400ms，无 flag/无攻击盒）；动画速度 = 1 + 剑术精通（SKILL_BLADESPIRIT=123）col0 加速；随后创建 PO 24349（dword 63），出生点规则同上。
- `onEndCurrentAni`：回 STATE_STAND。
- 施法侧不设攻击信息、不创建其它对象——全部伤害在幻鬼 PO。

### 2.3 共享 PO 24349 / id 63（幻鬼本体，F5 链路）

`setcustomdata.nut:893` case 63 → `sendStateOnlyPacket(10)`；`setstate.nut:426` case 63 四个子状态：

| 子状态 | 动画（etc motion 0 基实测） | atk code | 伤害列 | 行为 |
|---|---|---|---|---|
| 10 回旋 | #72 `vsspinningslashatt1_body.ani`（14 帧 870ms） | 36 | col0 | 攻速同剑术精通；判定盒 ×static[0]；若 static[3]>0 再造 PO id 64（darkwind 追尾层，本 pvf 关） |
| 11 终结 | #73 `vsspinningslashfinish_body.ani`（24 帧 3170ms） | 37 | col1 | flag 10001 时 **PO 前移 static[1]=150px**（`sq_GetDistancePos` 瞬移）+ 终结特效 + 屏震 5/200 |
| 12/13 追加回旋 | #74/#75 `_body_add1/_add2.ani`（各 870ms） | 36 | col0 | 仅 static[2]>0 时进入（本 pvf 关）；12→13→11 |

- `onEndCurrentAni`（id 63）：state10 末 → static[2]>0 ? state12 : state11；state11 末 → 销毁；state12→13→11。
- `onkeyframeflag`（id 63）：state10/12/13 的 flag 10001 → `als_ani` 叠加特效 `vsspinningslashatt1_05.ani` + `_01.ani`（后者 ×sizeRate）+ 屏震；state11 的 10001 → 前移 + `vsspinningslashfinish_00/09/06.ani` 三层特效 + 屏震；state11 的 10002 → 消散特效 `disappearback/front.ani`。
- `onattack`/`procappend` 无 case 63（无命中附加状态）。

**atk 错位（VS 族 -2 规律在 id 63 再实证，重要）**：脚本 atk code 36/37 按当前表 0 基直读 = `speedslashvs5.atk` / `spinningslashvs1.atk`；按名称意图配对应为 `spinningslashvs1.atk`（回旋）/ `spinningslashvs2.atk`（终结）——意图位比 code 高 2，与 R2-A10（066 §2.3）实证的 VS 族 -2 规律一致，且把该规律的适用范围从 id 66-74 扩到 **id 63**（VS 块起点提前）。三份 atk 实测：

| atk | 关键参数 | 语义 |
|---|---|---|
| spinningslashvs1.atk（意图·回旋） | physic/weapon、damage 反应、push 100、lift 80、hit horizon、blood 0 2.0、knuck back 2 | 平推型回旋（**建议采用**） |
| spinningslashvs2.atk（意图·终结） | physic/weapon、**down** 反应、push 220、**lift 350**、blood 70 3.0、hit down/front、knuck 3 150、**force hit stun 1000**、vs opposite cut | 击飞终结 |
| speedslashvs5.atk（运行时·回旋实际） | physic/weapon、down 反应、push 200、**lift -3000（砸落）**、bounce 1/bounce up 300、blood 30 2.0 | 本 pvf 实际手感（砸地控）——若追求还原 pvf 实况采用此份 |

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\spinningslashvs.ani`（角色施法，.chr #300） | 4 | 400ms（40/60/60/240） | 无 | 无 | 仅引 `sm_body%04d.img`（已入库）；本体只是摆pose，攻击全在 PO |
| `…unclebang…\animation\spinningslashvs\vsspinningslashatt1_body.ani`（PO 回旋） | 14 | 870ms | F4=10001、F5=10002、F8=10003（10002/10003 无脚本消费者） | F5/F6：`-250 -60 0 550 120 150` | 引 VengeanceSpirit.img（幻鬼身体）；.als 三层 none-effect 叠加 |
| `…vsspinningslashfinish_body.ani`（PO 终结） | 24 | 3170ms | F0=10001、F16=10002 | F0/F1/F2：`-320 -70 0 680 140 150` | 无 .als；特效由 als_ani 动态叠 |
| `…vsspinningslashatt1_body_add1/_add2.ani`（追加回旋 ×2） | 14 | 870ms | 同 att1_body | F5/F6 同值 | 本 pvf static[2]=0 不启用 |
| `…vsspinningslashatt1_05/_01.ani`、`vsspinningslashfinish_00/09/06.ani`、`disappearback/front.ani`（特效层） | — | — | — | — | 见 §4 img 表；`_05.ani`/`finish_06.ani` 自带 .als 子层 |

攻击盒读法（PO"偏移+尺寸"或 min/max 口径，064 同构）：回旋 F5 ≈ x∈[-250,550] y∈[-60,120] z∈[0,150]（800×180×150px，中心前偏 150px）；终结 F0 ≈ x∈[-320,680]（1000px 宽，中心前偏 180px）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | spinningslashvs.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\spinningslashvs.skl` | ✅ 实测 | CD/MP/static 8 值/2 列 level info |
| 注册行 | swordman_load_state.nut 行 165 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 5_ghostsword\spinningslashvs\spinningslashvs.nut，状态 121，技能 26 |
| 主 nut | spinningslashvs.nut | `…\pvf\sqr\character\swordman\5_ghostsword\spinningslashvs\spinningslashvs.nut` | ✅ 实测（115 行全读） | 双路径施放 + 创建 PO 24349/63 |
| 共享 PO 回调 | setcustomdata/setstate/onkeyframeflag/onendcurrentani 的 case 63 | `…\pvf\sqr\shared_passive_object\swordman\*.nut` | ✅ 实测 | 幻鬼四子状态（回旋/终结/追加×2） |
| 共享 PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ 实测 | etc motion #72-75、etc attack info 表（错位见 §2.3） |
| PO atk | spinningslashvs1/2/3.atk、speedslashvs5.atk | `…\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\` | ✅ 实测（3/4 份细读；spinningslashvs3 未读——本技能无引用） | 回旋/终结命中参数 |
| .chr 条目 | etc motion #300 | `…\pvf\character\swordman\swordman.chr` 行 1273 | ✅ 实测 | Animation/spinningslashvs.ani |
| 角色 .ani | spinningslashvs.ani | `…\pvf\character\swordman\animation\spinningslashvs.ani` | ✅ 实测 | 4 帧 400ms |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 无本技条目 | 伤害全在 PO（L3 常态） |
| PO .ani/.als | vsspinningslashatt1/finish_body(+_add1/2) ×4 + .als ×3、特效 att1_00-09/finish_00-09、disappearback/front、att1_05/finish_06 .als | `…\passiveobject\unclebang_shared_passive_object\swordman\animation\spinningslashvs\` | ✅ 实测（33 文件目录 ls） | 幻鬼四相位 + 特效组 |
| 未启用资源 | spinbody\ 子目录、darkwind\ 子目录 | 同上 | ✅ 存在（spinbody 为 att1 系副本；darkwind 属 id 64 关闭分支） | static 开关关闭，不需要 |
| 装备层 | spinningslashvs 系 ×76 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,…}\*\` | ✅ 实测（find 计数 76） | 换装图层（只查存在性） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/BladeSpiritDot/VengeanceSpirit.img` | sprite_character_swordman_effect_bladespiritdot.NPK | 幻鬼实体身体（回旋/终结 body 主层） | **必需** | ❌ |
| `…BladeSpiritDot/VengeanceSpirit_Dodge.img` | 同上 | 幻鬼加法混合层（族共享，全 VS 系复用） | 可选 | ❌ |
| `…Effect/SpinningSlashVS/slash1.img` | sprite_character_swordman_effect_spinningslashvs.NPK | 回旋斩特效（att1_01） | **必需** | ❌ |
| `…SpinningSlashVS/slash2_normal1.img` | 同上 | 终结大斩（finish_06，×sizeRate） | **必需** | ❌ |
| `…SpinningSlashVS/redeye2.img` | 同上 | 终结红眼光效（finish_00） | **必需** | ❌ |
| `…SpinningSlashVS/redeye1.img / rock.img / slash2_dodge.img / slash2_normal2.img` | 同上 | 备用/变体层 | 可选 | ❌ |
| `Character/Swordman/Effect/TeleportVS/Normal.img`、`…/LDodge.img` | sprite_character_swordman_effect_teleportvs.NPK | 幻鬼出现/消散（att1_05、disappearfront/back） | 可选（无它则幻鬼瞬现瞬灭） | ❌ |
| `Character/Fighter/Effect/JumpSuplexDustBack/Front.img`、`Character/DemonicLancer/Effect/HuntersMoveset/DustC.img` | 跨职业 sprite NPK（L14 跨目录复用常态） | 落地扬尘 | 可选 | ❌ |
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色帧（已入库） | 必需（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` |

**结论**：必需 4 张（VengeanceSpirit + slash1 + slash2_normal1 + redeye2），分属 2 个 NPK；可选 8 张。AnimConfigRegistry 无本技能相关 AnimId（实测 grep）。

## 5. 实现方案草案

- **内容件清单**（继承真实基类；两相位 → 两 Area，064 同构决策）：
  - `SpinningSlashVSSkill : SkillLogic`（同 BloodBoomSkill 帧触发 + SubState 守卫范式）：`CooldownMs=20000`、`TotalTimeMs=1000`（角色动画仅 400ms，但需活到 870ms 发起终结 Area）。`OnCast`：`ctx.PlayAnim(AnimId.SwordmanSpinningSlashVS)` + `ctx.ClearHitTargets()` + `ctx.CreateAreaInFront(AreaIds.SpiningSlashVSSpin, 70/100=0.7)`（幻鬼出生 = 施法者前方 70px≈0.7 单位）。`OnUpdate`：`ctx.GetElapsedMs() >= 870` 且 SubState 守卫 → `ctx.CreateAreaInFront(AreaIds.SpinningSlashVSFinish, 0.7 + 1.5)`（终结 Area 补上 PO 前移 150px≈1.5 单位）。`OnEnd`：`ctx.PlayDefaultAnim()`。
  - `SpinningSlashVSSpinArea : AreaDefinition`（同 BloodBoomArea 一次性结算范式）：`TotalTimeMs=870`、`TickTimeMs=0`、`EnterActions={MeleeHit}`、`HalfExtents=(4.0,0.9,0.75)`（F5 盒 800×180×150px ÷100，中心前偏 1.5 由创建距离承担）、`HitReaction{Damage=120, HitstunMs=500, KnockbackX=100, LaunchY=80}`（spinningslashvs1.atk 意图值；若还原 pvf 实况改用 speedslashvs5.atk 的砸落：LaunchY=-3000→demo -60、KnockbackX=200）、`ViewAnimId=AnimId.SpiningSlashVSAttBody`（vsspinningslashatt1_body.json + 其 .als overlay）。
  - `SpinningSlashVSFinishArea : AreaDefinition`：`TotalTimeMs=1500`（原 ani 3170ms 大半是消散尾帧，判定盒只活 450ms）、`EnterActions={MeleeHit}`、`HalfExtents=(5.0,1.05,0.75)`（F0 盒 ÷100）、`HitReaction{Damage=150, HitstunMs=1000, KnockbackX=220, LaunchY=350}`（spinningslashvs2.atk，force hit stun 1000 直用）、`ViewAnimId=AnimId.SpinningSlashVSFinishBody`、特效 overlay 手组装 finish_00/09/06 三层（releasewave 手组装先例）。
  - 无新 Action/Buff（MeleeHit 现成）。
- **概念映射**：PO 24349/dword63 两相位 → 两个 Area 顺序创建；幻鬼出生 70px/前移 150px → `CreateAreaInFront` 距离参数；als_ani 动态特效 → Area overlay（LSAnimOverlayUtil）；atk 意图值 → HitReaction。
- **注册点**：SkillIds 加 `SpinningSlashVS=19`；AnimIds 加 `SwordmanSpinningSlashVS=77`、`SpinningSlashVSAttBody=78`、`SpinningSlashVSFinishBody=79`（+特效层 80-83 预留）；AreaIds 从 9 起（`Spin=9`、`Finish=10`）；LSAnimClipRegistrar 注册 3-4 个 json；BuildAtlas 增 bladespiritdot/spinningslashvs 两图集；按键映射新键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms | 20000（直用） |
| 施法动画 | 4 帧 400ms | TotalTimeMs 1000（兜终结 Area 触发） |
| 回旋判定 | F5/F6 盒 800×180×150px，PO 出生 70px | Area 半尺寸 (4.0,0.9,0.75)，出生 0.7 单位 |
| 回旋命中 | atk1：damage/push100/lift80（col0 3208=320.8% 武器） | 伤 120/硬直 500/Kb 100/Ly 80 |
| 终结判定 | F0-F2 盒 1000×210×150px，前移 150px | Area 半尺寸 (5.0,1.05,0.75)，创建距离 2.2 单位 |
| 终结命中 | atk2：down/push220/lift350/硬直1000（col1 4812=481.2%） | 伤 150/硬直 1000/Kb 220/Ly 350 |
| 终结触发时点 | PO state10 动画末（870ms） | OnUpdate 870ms |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| spinningslashvs.ani 等 6 份角色/PO ani（实测节名枚举） | `[INTERPOLATION]`（17 处）不在翻译规则表，README 未识别清单亦未提及 | 整节跳过无碍（帧插值是渲染平滑，帧动画直出即可）；建议 README 未识别节清单补记 |
| 同上 | `[SHADOW]`（6 处）、`[PLAY SOUND]`、`[SET FLAG]`、`[DAMAGE TYPE]`、`[IMAGE RATE]`（42 处） | 均已在已知跳过清单（[SHADOW] 轮间经验已记档）；[IMAGE RATE] ×sizeRate 缩放是本技能视觉主参数——归游戏侧"对象整体缩放"延后档 |
| 3 份 .als（att1_body/_add1/_add2）+ 2 份特效 .als | 仅 `[use animation]`/`[none effect add]` | 现有 als 子命令全覆盖（L12），无缺口 |
| PO .atk ×4 / swordman_shared.obj / .skl | `.atk`/`.obj`/`.skl` 无子命令 | 既有缺口（轮间经验已记）；本技能手抄可行 |

**结论**：ani/als 全部可被现有子命令翻译；新记 1 条（[INTERPOLATION]）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 幻鬼实体在场位置接力（getVSObject → 在幻鬼处重建并销毁旧体） | **幻鬼实体记忆/锚点**（缺失，R2-A6/A8 已上报） | 固定出生在前方 0.7 单位；族级系统立项后回补 |
| 六剑术施放中"无施放动作即时幻鬼攻击" | **技能取消体系**（缺失，064 已上报）+ 上条 | demo 只做常规施放路径 |
| 攻速随剑术精通（BLADESPIRIT col0）提升 | 属性数值消费链缺失（R1-A4 最重缺口） | 固定 1.0 速度（Area/动画时长即原值） |
| 判定盒/图像 ×static[0] 缩放 | 对象整体缩放（延后） | 固定 100% |
| 终结段 PO 前移 150px 瞬移 | 无（`CreateAreaInFront` 距离参数即可表达） | §5 已用创建距离表达 |
| 追加回旋（static[2]>0）/darkwind 追尾（static[3]>0） | 无缺口（数据开关，本 pvf 均 0） | 不实现，注释保留开关语义 |
| 屏震 5/200、消散特效、音效 | 屏震/音效延后 | 跳过 |
| VS 族 atk 错位（运行时加载 speedslashvs5=砸落，意图=平推） | 非缺口（pvf mod 瑕疵，066 同族） | 采用意图值 atk1/atk2（§5）；追求 pvf 实况换数值即可 |

## 8. 存疑与缺口上报

- **未考证**：static[4..7]=`100 600 400 80` 语义（无脚本消费可证）；att1_body F5=10002/F8=10003 两个 flag 的消费者（疑引擎内置/废弃）；spinningslashvs3.atk 的归属（本技能无引用）；`illusionslashwaveparticle_ds.als` 类同目录资源与本技能无关项未逐一排查。
- **族级结论修正（供主循环回填轮间经验 F5）**：VS 族 atk -2 错位的适用范围从"id 66-74"扩到 **id 63**（spinningslashvs 脚本 code 36/37 → 名称位 37/38，实证同 066）；建议表述改为"VS 块（id 63 起）"。getVSObject 认定的幻鬼实体 id 集实测 = {61,62,63,65,66,68,70,74}（jg_swordman_common.nut:902-905）。
- **翻译工具缺口**：`[INTERPOLATION]` 节（本批首记，建议并入未识别���清单）。
- **系统级缺口复证**：幻鬼实体记忆/锚点（26 为第 N 例）、技能取消体系（剑术中即时施放路径整族依赖）。
