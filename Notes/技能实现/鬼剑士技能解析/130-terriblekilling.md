# 鬼隐 · 夜奈落（terriblekilling）

> 技能ID 130 | 级别 A | 可实现性 🔶（演出主干可还原；无敌/吸怪/结束位移三缺口降级） | 分析日期 2026-08-22 | 批次 A19

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼隐 · 夜奈落 | `skill\Swordman\ghostsword\terriblekilling.skl [name]` |
| 英文名 | terriblekilling（取 skl 文件名，全小写） | 同上 |
| 职业 | 剑影二次觉醒（夜见罗刹）——`[skill fitness growtype]` 为空、`[skill fitness second growtype] = 2`（剑影二觉专属） | 同上 |
| 学习等级 | 85（二觉大招） | 同上 `[required level]` |
| 最高等级 | 40（二觉段上限 30，索引 11） | 同上 `[maximum level]` / `[second growtype maximum level]` |
| 类型 | active（skill class 1） | 同上 `[type]` |
| 指令 | ↓↑→→ + Z（指令施法 MP 优惠 50%） | 同上 `[command]` / `[skill command advantage]` |
| CD | 180000 ms（3 分钟） | 同上 `[cool time]` |
| MP | 2329 → 4658 | 同上 `[consume MP]` |
| 特殊消耗 | 无色小晶体类道具 3037 × 10 | 同上 `[consume item]` |
| 可施放状态 | 0（站立）/ 8（攻击）/ 14（疑幻鬼共存态，未考证，与 054 同款存疑） | 同上 `[executable states]` |
| 一句话效果 | 夜见罗刹与幻鬼爆发鬼气笼罩世界（黑幕演出），幻鬼下劈+双方连续三段斩击范围内敌人并**强制定身控制**，终结交叉斩解除控制击倒（"[永堕黑暗吧！]"） | 同上 `[explain]` + 走读 |

**static data**（dungeon）：`600 -50 -20 10 400 100`——六值在白名单内**无任何脚本消费者**（主 nut 与共享 PO case 60 均不读；吸怪用硬编码 ±150、回撤用硬编码 -150）。语义未考证，疑引擎内置消费或残留。

**level info**（4 列，Lv1 → Lv68）：col0 第一次下劈 18411 → 312990；col1 第二次横斩 18411 → 312990；col2 第三次横斩 18411 → 312990（**前三列恒等**）；col3 终结交叉斩 67507 → 1147630（≈ 前段 3.67 倍）。
**level property**（4 占位符）：四向量 `(-1, 0..3, 1.0)` 全部指向 level 列 0-3（L21 规则，与 PO 回调 `sq_GetBonusRateWithPassive(130,-1,col,1.0)` 消费完全对位）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
161: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/terriblekilling/terriblekilling.nut", "terriblekilling", STATE_TERRIBLEKILLING, SKILL_TERRIBLEKILLING);
 18: IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);   // F5 共享判定 PO（全族共用）
```

`swordman_header.nut` 常量（实测行号）：`STATE_TERRIBLEKILLING <- 126`（50 行）、`SKILL_TERRIBLEKILLING <- 130`（163 行）、
`CUSTOM_ANI_TERRIBLEKILLING1 <- 296` / `2 <- 297`（467/468 行）、`CUSTOM_ATTACK_INFO_TERRIBLEKILLING1..4 <- 162..165`（553-556 行）。

- 角色 .chr etc motion 0 基 #296/#297（1269/1270 行）= `Animation/terriblekilling1.ani` / `terriblekilling2.ani`（对位吻合）。
- 攻击信息 #162-165（1456-1459 行）= `AttackInfo/terriblekilling1..4.atk`——**经 parentChr 的 .chr 表直读，0 基无错位**（F5 的 -2 错位只发生在经 PO 自身 .obj [etc attack info] 表取数的场合，本技能不经过该路径，见 §8 给主循环的边界说明）。
- 伤害主体 = F5 unclebang 共享 PO 24349，写包首 dword = **60**。
- 同槽复用：JG NPC 剑鬼 22 的二觉演出复用同一状态 126/技能 130（`STATE_SWORD_GHOST_22 <- 126`、`SKILL_SWORD_GHOST_22 <- 130`），消费 etc motion #238/#239（`terriblekillingstartbs_body.ani`/`terriblekilling_body.ani`）与攻击信息 #131-135（`terriblekilling.atk`/`terriblekilling1..3.atk`/`terriblekillingfinish.atk`）——玩家技能流程不经过这一套（白名单内无其脚本消费者，常量悬空）。

### 2.2 主 nut 逐回调（terriblekilling.nut，156 行全读）

**checkCommandEnable**：恒 true。

**checkExecutableSkill**：`sq_IsUseSkill(130)` → 推子状态 0 进状态 126（仅状态 0/8/14 可放，非全状态取消型）。

**onSetState**：
- subState 0（起手，600ms）：播音效 `R_SM_TERRIBLEKILLING`；播 `terriblekilling1.ani`；**`OBJECT_MESSAGE_UNBREAKABLE` 开**，并按动画总时长 600ms 后自动关（`sq_PostDelayedMessage`）——全程无敌由消息对实现。`sq_StopMove`。
- subState 1（主演出，3090ms）：播 `terriblekilling2.ani`；UNBREAKABLE 同法覆盖全时长；`isMyControlObject` 时写包 dword **60** 并 `sq_SendCreatePassiveObjectPacket(24349, 0, 0, 0, 0)`——PO 在子状态切入瞬间创建于自身原点（非帧触发）。

**onKeyFrameFlag**（按子状态分流）：
- subState 0（terriblekilling1.ani 实测 flag：F4=10001、F6=10002、F8=10003）：
  - 10001：`als_ani` 播 `effect/animation/terriblekilling/startblacksmokeground_00.ani`（黑烟地面，自带 .als 链 `_01`）；
  - 10002：播 `startgroundbs_00.ani`（罗刹登场地面层）；F8 的 10003 无消费者。
- subState 1（terriblekilling2.ani 实测 flag：F5=10001、F13=10002、F17=10003、F25=10004、F39=10005、F42=10006、F48=10007）：
  - **10001（F5，子状态起 300ms）——吸怪**：遍历碰撞管理器全部对象，`isEnemy && OBJECTTYPE_ACTIVE` → `object.setCurrentPos(sq_GetDistancePos(自身x, 朝向, sq_getRandom(-150,150)), 自身y, 0)`——**把全场敌人硬传送到自身前后 ±150px、贴地**（演出语义：黑暗中敌人被拽入斩击场）；同帧叠加三层特效 `ghostbgloop_00.ani`（幻鬼背景循环，als 链 01-04）、`attack_00.ani`（攻击主特效母时间轴，als 链 attack_01-16）、`screen_03.ani`（全屏暗幕，als 链 screen_00-15）。
  - 10002/10003/10004/10005：**角色侧无消费者**——其中 10002-10004 是 `terriblekilling2.ani.als` 的 marker 门（BSMove 夜见罗刹剪影层按标记逐段现身，见 §2.4）；F39 的 10005 悬空。
  - 10006（F42，与 PO 终结斩同时刻）：`sq_SetMyShake(30, 300)` 屏震。
  - 10007（F48）：六个收尾消散层 `vsmoveend_01/_01_white/_00/_blood` + `disappearback/front`（PO 动画目录，x=150 偏移）。

**onEndCurrentAni**：
- subState 0 → 推子状态 1（顺序演出）。
- subState 1 → 回 STATE_STAND；`sq_MoveToNearMovablePos(…, -150 …)` **传送回撤 150px**；随后**强制转身**（LEFT↔RIGHT）——"斩穿后背身收刀"的演出收尾。

### 2.3 幻鬼被动对象（共享 PO 24349，case 60，四段连击主体）

| 回调 | case 60 行为 |
|---|---|
| setcustomdata | 播 `getCustomAnimation(63)` = `character/swordman/effect/animation/terriblekilling/terriblekilling_attack.ani`（.obj etc motion 0 基 #63 直读对位）——**51 帧空占位动画**（IMAGE 全空，L7 型），纯时间轴+判定盒载体，与角色 2 号动画**逐帧同步**（同 51 帧 3090ms） |
| onkeyframeflag | 四段切换：flag 10001→`CUSTOM_ATTACK_INFO_TERRIBLEKILLING1`+col0；10002→atk2+col1+`resetHitObjectList`；10003→atk3+col2+reset；10004→atk4+col3+reset。**注意 10001 分支无 break 直落 10002**：第一段攻击信息被立刻覆盖为 atk2/col1——因 skl 数据 col0=col1=col2 恒等，数值无损，仅丢失 atk1 的 `ignore super armor` 位（sworddancebs case 59 同款 fall-through 写法，疑原作者笔误的族特征） |
| onattack | `GhostSword_Attack_Effect`（jg_swordman_common.nut:800，对受害者随机贴斜斩命中特效，纯视觉）+ **挂 `ap_terriblekilling.nut` appendage，有效期 900ms**（每段命中刷新） |
| onendcurrentani | 播完即销毁（PO 生命周期 3090ms） |
| setstate / procappend / ontimeevent | **无 case 60 分支**（无逐帧行为——下劈/斩击全部由动画 flag 驱动） |

**PO 动画判定**（terriblekilling_attack.ani，攻击盒「偏移+尺寸」单行 `-800 -800 -800 1600 1600 1600`）：
四个攻击窗与角色动画同步——F5-F7（flag 10001，子状态起 300-480ms）、F17-F19（10002，1000-1210ms）、F28-F31（10003，1570-1810ms）、F42-F44（10004，2440-2620ms）。盒体按偏移+尺寸口径为边长 1600px（16 单位）的立方级巨型判定（另一种 min/max 口径则 -800~+1600 前倾）——远超 ±150px 吸怪范围的"安全网"式判定。

**强控 appendage（ap_terriblekilling.nut，84 行全读，挂受害者 900ms）**：
- onStart：`sq_SetCustomDamageType(parent, true, 1)`（切换受击表现类型）+ `sq_SetGrabable(false)`；若受害者处于眩晕则强制解除（细节语义未考证）。
- proc：每帧 `STATE_HOLD`（Z_ACCEL_TYPE_CONST）——**硬定身**（explain"攻击时可以强制控制敌人"的实现）。
- onEnd：还原伤害类型/可抓；若仍处 HOLD → 强制 `STATE_DOWN`（DOWN_PARAM_TYPE_FORCE + 方向 + 100/150 参数）——**控制解除时击倒倒地**（explain"发动终结攻击时解除控制"）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG / marker | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/terriblekilling1.ani`（角色起手） | 12 | 600ms | F4=10001（黑烟层）、F6=10002（罗刹登场层）、F8=10003（悬空） | 无 | **无 DAMAGE BOX**（配合 UNBREAKABLE 全程无敌）；.als 挂 TerribleKillingStartBS_00 @marker 10001 |
| `character/swordman/animation/terriblekilling2.ani`（角色主演出） | 51 | 3090ms（42×60+100+2×30+2×90+3×20+170；非 60 帧位于 F14/15/16/17/21/22/23/38/50） | F5=10001（**吸怪+三特效层**）、F13=10002/F17=10003/F25=10004（.als marker 门）、F39=10005（悬空）、F42=10006（屏震）、F48=10007（消散层） | 无 | 无 DAMAGE BOX；.als 挂 BSMove_00-03（marker 10001-10004，夜见罗刹剪影四段现身）+ VSMove_00-05（z -6..-1 常驻背景，帧 0/0/3/10/28/0，幻鬼移动剪影） |
| `…/effect/animation/terriblekilling/terriblekilling_attack.ani`（PO 时间轴） | 51 | 3090ms（延迟分布与角色 2 号动画**逐帧全同**——非 60 帧位置一一对应，帧锁双胞胎） | F5=10001、F17=10002、F28=10003、F42=10004（四段命中切换，绝对施法后 900/1600/2170/3040ms） | **F5-F7 / F17-F19 / F28-F31 / F42-F44**（`-800 -800 -800 1600 1600 1600`） | **空占位**（IMAGE 全空）——视觉由角色侧 attack_00/screen_03 链承担 |
| attack_00.ani（攻击特效母轴）+ als 链 attack_01-16 | 未逐帧统计 | — | als marker 10001-10016 × 帧 0/1/5/21/22 | 无 | 16 层子特效（血雾/刀疤/闪光/白斩），circle.img 打底 |
| screen_03.ani（全屏暗幕）+ als 链 screen_00-15 | 未逐帧 | — | als marker -12..-3 + 10001-10003 × 帧 35/36 | 无 | "世界笼罩黑暗"演出层 |
| ghostbgloop_00.ani + als 链 01-04 | 未逐帧 | — | als z -4..-1 | 无 | 幻鬼背景循环（地面/球体/坠落背景） |
| vsmoveend_01 / _01_white / _00 / _blood / disappearback / disappearfront | 未逐帧 | — | — | 无 | 收尾消散六层（F48=10007 触发） |

**无消费者动画记档**（白名单内大小写不敏感全查无命中）：`blackbgstart_00`、`vsappear_00-01`、`ghostbgstart_00-04`、`ground_00-05`（ground_01 自带 .als 链）、PO 目录 `startvsa_body(+als)/startvsb_body(+als)/startvsa_00/startvsb_00-01/startgroundvs_00/endvs_body/endvs_body_white`——疑官方原版/Mod 重制版并存残留；`erjue_effect_front/back` 被 `JG_SwordMan/swordghost22/ap_erjue.nut`（NPC 二觉光环）借用，不在本技能流程。

`.als` 边车共 12 个（角色 2 + 特效 6 + PO 2 + 未消费 terriblekilling_body/startbs_body 2），节全部为 `[use animation]` + `[none effect add]`，现有 als 子命令可译。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | terriblekilling.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\terriblekilling.skl` | ✅ 实测 | 等级/CD/MP/static 6 值/level 4 列 |
| 注册行 | swordman_load_state.nut 行 161 / 18 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 126 + PO 24349（F5 共用） |
| 主 nut | terriblekilling.nut | `…\pvf\sqr\character\swordman\5_ghostsword\terriblekilling\terriblekilling.nut` | ✅ 实测（156 行全读） | 两子状态演出 + 吸怪 + 无敌 + PO 创建 |
| 强控 appendage | ap_terriblekilling.nut | `…\5_ghostsword\terriblekilling\ap_terriblekilling.nut` | ✅ 实测（84 行全读） | 受害者 hold/禁抓/结束击倒 |
| 共享 PO 回调 | onkeyframeflag.nut case 60（442-477 行）、onattack.nut（121-130 行）、setcustomdata.nut（881-886 行）、onendcurrentani.nut（394-399 行） | `…\pvf\sqr\shared_passive_object\swordman\` | ✅ 实测 | 四段命中切换 + 强控挂载 |
| 共享 PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ 实测 | etc motion #63 = terriblekilling_attack.ani |
| .chr 条目 | etc motion #296/#297（1269/1270 行）+ #238/#239（1211/1212 行，JG 复用）；etc attack info #162-165（1456-1459 行）+ #131-135（1425-1429 行，JG 复用） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 动画/攻击信息两套对位 |
| 角色 .ani | terriblekilling1.ani / terriblekilling2.ani（+ 各自 .als；+ JG 复用 terriblekilling_body / terriblekillingstartbs_body + .als） | `…\pvf\character\swordman\animation\` | ✅ 实测 | 600ms 起手 + 3090ms 主演出 |
| 角色 .atk | terriblekilling1..4.atk（玩家流程）+ terriblekilling.atk / terriblekillingfinish.atk（JG 套，玩家流程不消费） | `…\pvf\character\swordman\attackinfo\` | ✅ 实测（6 文件全读） | 四段命中反应 |
| PO .ani | terriblekilling_attack.ani（角色特效树借用） | `…\pvf\character\swordman\effect\animation\terriblekilling\` | ✅ 实测 | 空占位时间轴+判定盒 |
| 特效 .ani/.als | attack_00(+als,01-16) / screen_03(+als,00-15) / ghostbgloop_00(+als,01-04) / startblacksmokeground_00(+als,01) / startgroundbs_00 / terriblekillingstartbs_00 / bsmove_00-03 / vsmove_00-05 等（66 文件目录实测） | 同上目录 | ✅ 实测 | 黑幕+双剪影+16 层攻击特效 |
| PO 收尾 .ani | vsmoveend_00/01/01_white/blood、disappearback/front | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\animation\terriblekilling\` | ✅ 实测 | 收尾消散六层 |
| 命中特效函数 | GhostSword_Attack_Effect | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut:800` | ✅ 实测（C2 定点读取） | 受害者随机斜斩贴图（common/hiteffect，白名单外，跳过） |
| 装备层 | terriblekilling1/2/startbs_body/body.ani ×4 组 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,coat,…}\*\` | ✅ 实测（find 命中，未计数全量） | 各 avatar 变体图层（只查存在性） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。角色动画仅引 `sm_body%04d.img` 模板（L16，已入库，零需求）。

| img（按 NPK 分组） | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| startbsa / startbsb / starta | sprite_character_swordman_effect_terriblekilling.NPK | 起手：罗刹登场 + 黑烟地面 | **必需** | ❌ |
| ghostground / ghostspherea / ghostfallingbg | 同上 | 幻鬼背景循环三层 | **必需** | ❌ |
| blooda2 / bloodb2 / bloodc2 / blooda1 / bloode1 / flare / slashwhite / scarcross / scarc / bloodmistc / scarb / bloodmistb / scara / bloodmista（14 张，attack_01-16 链） | 同上 | 四段斩击的血雾/刀疤/闪光主视觉 | **必需** | ❌ |
| lastslash / dustb / spark / bloodd2 / bloodd1 / bgdisappear / dusta / shadow / slashforce / atmosphere（10 张，screen 链） | 同上 | 全屏暗幕+终结斩演出 | **必需** | ❌ |
| disappearbs / appearbs（bsmove 链）+ appearvs / disappearvs（vsmove 链）+ bloode / bloodf（vsmoveend） | 同上 | 双剪影现身/消散（罗刹+幻鬼） | 可选（演出完整性建议提取） | ❌ |
| vengeancespirit.img / vengeancespirit_dodge.img | sprite_character_swordman_effect_bladespiritdot.NPK | 幻鬼实体剪影（vsmove_05/vsmoveend_01/01_white） | 可选 | ❌（054 同款共缺） |
| bsattackc.img / vsattackh.img | sprite_character_swordman_effect_spritcrossslash.NPK | 罗刹/幻鬼斩击剪影（bsmove_03/vsmove_04） | 可选 | ❌ |
| normal.img / ldodge.img | sprite_character_swordman_effect_teleportvs.NPK | 传送消散（disappearback/front） | 可选 | ❌（054 同款共缺） |
| common/commoneffect/glow/circle.img | sprite_common_commoneffect_glow.NPK | 特效链通用圆环打底 | **必需** | ✅ `Bundles\AnimRes\circle.img.bytes` 已在库 |

**缺失 img：必需级 30 张（同属 terriblekilling 主 NPK，一次提取全覆盖）+ 可选级 12 张（4 个跨技能 NPK，与 054/066 共享）= 42 张缺失 / 43 张总需求。** img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案

**结构映射**：双段角色动画（600+3090ms）+ 时间驱动四段 Area（对应 PO 四攻击窗）+ HoldBuff（强控）。

### 内容件清单

1. **`DotNet~/Skills/TerribleKillingSkill.cs : SkillLogic`**（BloodBoomSkill SubState 编排 + 064 时间驱动范式）
   - `CooldownMs = 180000`；`TotalTimeMs = 3700`（600 起手 + 3090 主演出）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanTerribleKilling1)` + `ctx.ClearHitTargets()`；SubState=0。
   - `OnUpdate` 时间驱动（SubState 单值推进，全部用 `ctx.GetElapsedMs()`，回滚安全）：
     - 600ms（SubState 0→1）：`ctx.PlayAnim(AnimId.SwordmanTerribleKilling2)`（起手→主演出，黑幕/罗刹层由该动画 .als overlay 自动叠）。
     - 900ms（SubState 1→2）：**吸怪简化点**（原版把全场敌人传送到 ±1.5 单位内——位移他人门面缺失，§7）：跳过传送，直接靠超大判定盒覆盖；SubState 推进。
     - 900 / 1600 / 2170ms（SubState 2→3→4）：`ctx.CreateArea(AreaIds.TerribleKillingSlash, 施法者位置)` ×3——**同 AreaId 三次创建**，每次新建 Area 即自带全新命中去重（对应 resetHitObjectList 的段间重置，L19 连段段间已通档）。
     - 3040ms（SubState 4→5）：`ctx.CreateArea(AreaIds.TerribleKillingFinish, 施法者位置)`（终结交叉斩，down 反应）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。（原版结束"回撤 150px + 转身"——施法者位置写入/朝向翻转门面缺失，§7 简化：原地收刀。）
2. **`DotNet~/Areas/TerribleKillingSlashArea.cs : AreaDefinition`**（BloodBoomArea 范式，EnterActions 单次结算）
   - `TotalTimeMs = 200`（单攻击窗 180-240ms 近似）、`TickTimeMs = 0`、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (3, 1, 3)`（DNF 盒 16 单位级安全网，demo 按吸怪范围 ±1.5 + 余量取 3 单位半径）；
   - `HitReaction { Damage = 150, HitstunMs = 500, KnockbackX = 150, LaunchY = 300, ProcBuffId = BuffIds.Hold, ProcChance = 100 }`（atk1-3：damage 反应/push 150/lift 300；强控 900ms→Hold 挂 900ms 每段刷新）；
   - `ViewAnimId = AnimId.TerribleKillingAttack`（attack_00 母轴 + 16 层 als overlay 自表视觉）。
3. **`DotNet~/Areas/TerribleKillingFinishArea.cs : AreaDefinition`**
   - 同构；`TotalTimeMs = 200`；`HitReaction { Damage = 550, HitstunMs = 800, KnockbackX = 150, LaunchY = 300 }`（atk4：**down 击倒**/push 150/lift 300/blood 0；伤害 = 前段 ×3.67 col3 比例）；`ViewAnimId` 复用 TerribleKillingAttack（或 screen 链终结层）。
4. **`DotNet~/Buffs/HoldBuff.cs : BuffDefinition`**（复制 StunBuff 改时长）
   - `TotalTimeMs = 900`；`AddActions = { ForbidMoveOn }`、`RemoveActions = { ForbidMoveOff }`——DNF 每帧 STATE_HOLD 的硬定身简化为移动禁止（目标状态查询/他人状态强制缺失，§7）；结束击倒（onEnd STATE_DOWN）省略——击倒手感由终结斩 LaunchY 承担。
5. **无新增 Action**（MeleeHit/ForbidMoveOn/Off 全现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 126 + terriblekilling1/2.ani 双段 | `TerribleKillingSkill` SubState 时间驱动 + 两个 AnimId |
| UNBREAKABLE 消息对（全程无敌） | 无敌帧缺口（R1-A5）——跳过；注：我们受击本就不打断施法（受击-施法互斥缺口反向兜底），演出不被打断的体验近似成立 |
| 共享 PO 24349 dword 60（空占位时间轴 + 四 flag 攻击窗） | 四个时间点 CreateArea（同 AreaId×3 + 终结 1）——PO 判定盒→Area HalfExtents |
| atk1-4 + level col0-3 | 三份 Slash Area HitReaction（col0-2 恒等合一）+ Finish Area（col3） |
| 吸怪（全场敌人传送 ±150px） | 位移他人门面缺失（R2-A8）→ 超大 HalfExtents 直接覆盖，敌人不聚拢 |
| ap_terriblekilling 强控 900ms | `HitReaction.ProcBuffId = Hold`（MonsterIceBreath 概率挂 Buff 先例，此处 100%） |
| 结束回撤 150 + 转身 | 施法者位置写入门面缺失（R2-A10 族）→ 跳过 |
| .als 双剪影/暗幕/16 层特效链 | AnimOverlayConfig + LSAnimOverlayViewComponent（bloodboom/releasewave 同构，三层 als 直接翻译注册） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.TerribleKilling = 23` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `TerribleKillingSlash = 16`、`TerribleKillingFinish = 17` |
| BuffId | `Runtime\BuffDefinition.cs` | `Hold = 10` |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanTerribleKilling1 = 104`、`SwordmanTerribleKilling2 = 105`、`TerribleKillingAttack = 106`（attack_00 主轴）、`TerribleKillingScreen = 107`、`TerribleKillingGhostBg = 108`、`TerribleKillingStartBS = 109`、`TerribleKillingVsMoveEnd = 110`（可选）；attack_01-16 / screen_00-15 / bsmove / vsmove 等 overlay 子层逐一注册（bloodboom §4.7-1 别名机制），号段 111 起预留 |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | 角色 2 + PO 1 + 特效母轴 4 + overlay 子层批量 |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | terriblekilling 主 NPK 30 张（必需）+ 跨技能 4 NPK 12 张（可选）+ circle（已在） |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 180000 ms | 180000（直用；demo 可临时调短验证） |
| 总时长 | 600 + 3090 = 3690ms（角色/PO 同步） | 3700 |
| 四段命中时刻（施法后） | 900 / 1600 / 2170 / 3040ms（PO flag @300/1000/1570/2440 + 600 起手） | 直用 |
| 前三段伤害 | level col0-2 = 18411%→312990%（恒等） | MeleeHit 150 ×3 |
| 终结伤害 | col3 = 67507%→1147630%（3.67×） | 550 |
| 前段命中反应 | atk1-3：damage/push 150/lift 300/hit down/blood 70×6.0 | Hitstun 500 / Kb 150 / Ly 300 |
| 终结命中反应 | atk4：**down**/push 150/lift 300/bs x cut/blood 0 | Hitstun 800 / Kb 150 / Ly 300（down 手感） |
| 强控 | appendage 900ms 每帧 HOLD + 结束强制 DOWN | HoldBuff 900ms ForbidMove（击倒并入终结斩） |
| 判定盒 | ±800px 立方级（16 单位） | HalfExtents (3, 1, 3)（覆盖吸怪 ±1.5 范围） |
| 吸怪 | 全场敌人传送 ±150px | 跳过（缺口） |
| 无敌 | UNBREAKABLE 全程 3690ms | 跳过（缺口） |
| MP / 无色 | 2329-4658 / 3037×10 | 无消耗系统，跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| attack_06.ani / attack_07.ani | **`[OPERATION]`（值 1）——新见节名**，不在 ani 子命令规则表（全局节，位于 SHADOW 与 FRAME MAX 之间） | 整节跳过无碍（疑动画整体混合/操作模式开关）；建议 README 未识别节清单补记 |
| terriblekilling.skl | `.skl` 无子命令 | 手抄（4 列 level + static 6 值，量小） |
| terriblekilling1..4.atk（+JG 套 2 文件） | `.atk` 无子命令 | 手抄（每文件 ~8 值）；atk 立项时 `ignore super armor`/`knuck back`/`[hit info] bs x cut`/`[hit wav]` 字段需纳入（族缺口已录） |
| swordman_shared.obj | `.obj` 无子命令 | 手工对位（本档已给 #63/#162-165 全部对位结论） |
| 60 个运行时 .ani | `[SHADOW]`（已知跳过族） | 非新缺口 |
| terriblekilling_body.ani.als（未消费文件） | 引用 `ground0_01.ani`——实际文件名 `ground_01.ani`，**失配引用** | R3-A13 已录".als 引用缺失文件降级"族；本文件无消费者，不影响 |
| 12 个 .als | `[use animation]`/`[none effect add]` 均已支持 | 无缺口 |

结论：.ani/.als 资源可被现有子命令翻译（**唯一新节 `[OPERATION]`**）；实质缺口 = 新节 1 条 + `.skl`/`.atk`/`.obj` 族共性 3 条 + als 失配引用 1 条（未消费），计 5 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 全程无敌 3690ms（UNBREAKABLE 消息对） | **无敌帧缺失**（R1-A5 已录；本技能是"全时长无敌"的最重用户） | 跳过；受击不打断施法的现状使演出连续性近似成立（伤害仍会吃到，二觉大招 demo 可接受） |
| 吸怪：全场 ACTIVE 敌人硬传送 ±150px | **位移他人门面缺失**（R2-A8 拉拽/牵引族——本技能是"群体硬传送"变体，比拉拽更激进） | 超大 HalfExtents 直接覆盖原站位敌人；表现差异：敌人不聚拢、斩击场不收拢 |
| 结束回撤 150px + 强制转身 | 施法者位置写入/朝向翻转门面缺失（R2-A10 位置门面族） | 原地收刀回待机 |
| 强制定身（每帧 STATE_HOLD + damageType 定制 + 禁抓 + 结束击倒） | 目标状态查询/他人状态强制缺失（R2-A8 族）；受击表现定制无通道 | HoldBuff（ForbidMove 900ms）近似"定身"；击倒由终结斩 LaunchY 承担 |
| 屏震 30/300、音效 R_SM_TERRIBLEKILLING、闪屏 | 延后档（无音频/屏震） | 跳过 |
| PO 16 单位立方判定 | 无（我们用 HalfExtents 表达，只是数值选择） | demo 取 3 单位半径 |
| 等级数值缩放（col0-3） | 延后档 | demo 固定值 |
| 双剪影多层 overlay（BSMove 4 + VSMove 6 + 16 层攻击链 + 15 层暗幕链） | 无框架缺口（.als overlay 已落地）；**工作量在资源翻译注册量**（约 45 个 ani json） | 先做主链（角色 2 + attack 链 + screen 主层），剪影层分批补 |
| JG NPC 剑鬼 22 复用同状态/技能号的二觉演出 | 无关（NPC 侧，本 pvf 常量悬空无消费者） | 不实现，记档防混淆 |

## 8. 存疑与缺口上报

**未考证项**
1. static data `600 -50 -20 10 400 100` 六值语义（无脚本消费者；600/400/100 与演出参数的对应关系无佐证）。
2. 可施放状态 14 的语义（与 054 同款存疑，幻鬼共存态推断）。
3. PO 动画攻击盒口径：`-800…1600` 按"偏移+尺寸"读为 16 单位立方、按 min/max 读为前倾 24 单位——两种口径差 8 单位，不影响 demo 取值（都远超需要）。
4. 角色动画 F8=10003 / F39=10005 悬空 flag（无 nut/als 消费者）。
5. 无消费者动画群（blackbgstart/vsappear/ghostbgstart_01-04/ground_00-05/PO 目录 8 个）的归属（官方原版 vs Mod 重制版并存残留，推断）。
6. case 60 flag 10001 无 break 直落 10002 的原意（数值无损因 col 恒等设计，但 ignore super armor 位在运行时丢失——实测运行时第一段用的是 atk2 参数）。

**给主循环的边界结论（并入轮间经验）**
1. **F5 -2 错位边界补充**：错位仅发生在"经 swordman_shared.obj 自身 [etc attack info] 表取数"（`sq_GetCustomAttackInfo(obj, …)`）；本技能走 `sq_GetCustomAttackInfo(parentChr, …)`（角色 .chr 表），0 基直读**无错位**（#162-165 与 CUSTOM_ATTACK_INFO_TERRIBLEKILLING1-4 逐一对位实证）。后续 F5 族读 atk 先看取数对象是 obj 还是 parentChr。
2. **技能槽复用形态新样本**：JG NPC 剑鬼 22 与玩家共用状态号 126/技能号 130（header 两套常量同值），各自动画（#238/239 vs #296/297）与攻击信息（#131-135 vs #162-165）分列——查老技能资源时"同号两套常量"要分辨消费方，防止串读。
3. **als marker 门控再实证**：terriblekilling2.ani 的 flag 10002-10004 无 nut 消费者、但被同名 .als 的 marker 值（10001-10004）消费——**判定"flag 悬空"前先查同名 .als 的 marker 列**（054 只见无消费者案例，本档是 marker 门控正样本）。
4. **全程无敌（UNBREAKABLE 消息对 + sq_PostDelayedMessage 定时关）**：与位移技的瞬段无敌不同，这是"按动画时长包络"的第二种用法，无敌帧立项时的第二种数据形态。
5. 新翻译节 `[OPERATION]`（attack_06/07.ani，值 1）——建议并入未识别节清单。
