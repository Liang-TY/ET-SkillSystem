# 冥夜鬼天杀（ultimatecrossslash）

> 技能ID 48 | 级别 A | 可实现性 🔶（主干可实现：蓄势 → 幻鬼现身 → 突进交叉斩大范围击倒；黑屏/无敌帧/强控/终结多层特效降级） | 分析日期 2026-08-22 | 批次 A14

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冥夜鬼天杀 | `skill\Swordman\ghostsword\ultimatecrossslash.skl` [name] |
| 英文名 | ultimatecrossslash（取 skl 文件名；本 skl 无 [name2] 节，实测） | 同上 |
| 职业 | 剑影（夜刀神）（[second growtype maximum level] 第 11/12 位=30/30 → growtype 5 剑影一觉/二觉档，87/233 两技能三方互证；幻鬼/夜刀神=F5 族常识） | 同上 |
| 学习等级 | 50（一觉主动；前置：技能 28 Lv1——ghostsword 族，未考证具体名） | 同上 [required level] / [pre required skill] |
| 最高等级 | 40（二觉后上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1，物理） | 同上 [type] / [weapon effect type] |
| 指令 | ↑↑↓↓ + Z（指令施法 MP 优惠 50%/50% 档） | 同上 [command] / [skill command advantage] |
| CD | 145000 ms（固定） | 同上 [cool time] |
| MP | 998 → 8383（Lv1→40） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×5（[consume item] 3037 5 5） | 同上 |
| 可施放状态 | 0 / 8 / 14 | 同上 [executable states] |
| static data | `0 800`（slot0=强控开关 **0=关**、slot1=控制范围 800px——level property 模板直接印证，见下） | 同上 [static data] |
| 一句话效果 | 夜刀神凝聚灵魂之力，周围陷入黑暗，幻鬼在前方出现，二者交叉斩击造成巨大物理伤害 | 同上 [explain] |

**level property（2 列 + 2 static；Lv1 → Lv40 首末值）**：col0 交叉斩攻击力 `12680→172468%`、
col1 交叉斩范围 `100→110%`（Lv6 起 +10%）；强控开关 = static[0]=**0（本 pvf 关闭）**、控制范围 = static[1]=800px。
**Lv 里程碑**（explain）：Lv3 施放时**无敌**、Lv6 范围 +10%、Lv9 攻击力 +10%。

## 2. 技能逻辑走读

### 2.1 注册与文件链（F5 族链路直查，L 经验 F5 全数命中）

```
// sqr/character/swordman_load_state.nut 行 160（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/ultimatecrossslash/ultimatecrossslash.nut", "ultimatecrossslash", STATE_ULTIMATECROSSSLASH, SKILL_ULTIMATECROSSSLASH);
// 行 18：pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349)   ← unclebang 共享 PO（F5）
// swordman_header.nut 行 49/160/465/466/552（实测）：STATE_ULTIMATECROSSSLASH <- 123，SKILL_ULTIMATECROSSSLASH <- 48
//   CUSTOM_ANI_ULTIMATECROSSSLASH1 <- 294 / 2 <- 295；CUSTOM_ATTACK_INFO_ULTIMATECROSSSLASH <- 161
//   SKILL_BLADESPIRIT <- 123（幻鬼之力被动：施法动画加速源）
```

状态号 **123** ≠ 技能 ID **48**。伤害体 = F5 共享 PO **24349**（`sqr\shared_passive_object\swordman\*.nut` 六回调 + 
`passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj`），按写包首 dword 分流：本技能用 **57/58** 两个 id。

### 2.2 主 nut 逐回调（ultimatecrossslash.nut，176 行；F5 族标准结构，未混淆）

**onSetState 子状态 0（蓄势，anim 294 = ultimatecrossslash1.ani，18 帧 1290ms）**：
```
播 R_SM_ULTIMATECROSSSLASH 音效；动画速度 = 1 + BladeSpirit(123) 被动 col0（幻鬼之力加速）
flashScreen(黑, 500/1700/500, α100)                    // 周围陷入黑暗
if 技能等级 ≥ 3: UNBREAKABLE 消息开，动画播完关         // Lv3 无敌（无敌帧缺口 R1-A5）
trigger_data = static[0] = 0                            // 强控开关（本 pvf 关）
if trigger_data > 0: 800px 内所有敌人挂 ap_ultimatecrossslash（有效期 1000ms）—— 强制控制
```

**onKeyFrameFlag 子状态 0，flag 10001（实测 F9，800ms）**：
```
写包（58）→ 创建 PO 24349 于身前 200px                 // 幻鬼现身（纯视觉体）
```

**onSetState 子状态 1（交叉斩，anim 295 = ultimatecrossslash2.ani，10 帧 730ms）**：
```
动画速度同上；sq_MoveToNearMovablePos(前移至多 150px)   // 突进（撞墙收敛）
写包（57）→ 创建 PO 24349 于身前 0（跟随施法者）        // 交叉斩伤害体
等级 ≥ 3: UNBREAKABLE 再开一段
```

**onKeyFrameFlag 子状态 1**：flag 10001（F0）播音效；flag 10002（F2，100ms）`als_ani` 播 light_01 特效；
flag 10003（F4，280ms）连播 endattack_03 / endbackground_01 / endfloor_00 / endforeground_06（均按范围 col1 缩放）+ 屏震 30/300。

**onEndCurrentAni**：子状态 0 → 子状态 1；子状态 1 → 回 STAND。

**全时序（帧表实测）**：0-1290ms 蓄势（800ms 幻鬼现身）→ 1290ms 突进 150px + 伤害体创建 →
**伤害窗口 1410-1840ms**（PO 动画 F4-F7 攻击盒激活段）→ 2020ms 结束。

### 2.3 被动对象 / appendage

**共享 PO 24349 分支（`sqr\shared_passive_object\swordman\setcustomdata.nut` 实测）**：
- **case 58**（幻鬼现身，蓄势段 800ms 创建于 +200px）��播 customAnimation 61 = 
  `passiveobject\unclebang_shared_passive_object\swordman\animation\ultimatecrossslash\vsultimatecrossslashappear_body.ani`
  （7 帧 660ms，**无攻击信息 = 纯视觉**，L13）；带 .als 挂 front/back 多层。
- **case 57**（交叉斩伤害体，随施法者）：播 customAnimation 60 =
  `character\swordman\effect\animation\ultimatecrossslash\ultimatecrossslash_attack.ani`
  （10 帧 730ms，与角色 anim 295 同构同旗标）；攻击信息 = `sq_GetCustomAttackInfo(施法者, 161)` →
  **借用角色 .chr etc attack info #161 = ultimatecrossslash.atk**；伤害倍率 = col0；攻击盒按 col1 缩放；
  `sq_moveWithParent` 跟随施法者。
- `onattack.nut` case 57：命中播 `GhostSword_Attack_Effect`（F5 族共享命中特效函数）。

**PO 动画攻击盒实测（ultimatecrossslash_attack.ani，偏移+尺寸口径）**：F4-F7 = 偏移(-800,-800,-800) 尺寸(1600,1600,1600)
——**以跟随点为中心 16×16×16 单位的巨型立方判定**（范围% 缩放 100~110），即"以自身为中心的大范围交叉斩"。

**ap_ultimatecrossslash.nut（77 行，强控 appendage——本 pvf 未启用）**：
- onStart：`sq_SetCustomDamageType(受控者, true, 1)`（自定义受击类型=无常规受击演出）+ `sq_SetGrabable(false)`；
  若已眩晕则强制清除并短暂锁状态切换；
- proc（每帧）：`addSetStatePacket(STATE_HOLD, 强制)`——把敌人钉在 HOLD 状态（**目标状态控制流**，031/ap_bloodsnatch 同族）；
- onEnd：恢复可抓性；若仍处 HOLD → 强制切回 STAND。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\ultimatecrossslash1.ani（槽294，蓄势） | 18 | 1290ms（F0-9=80，F10=120，F11=100，F12-14=40，F15-17=50） | **F9=10001**（800ms，幻鬼现身） | 无 | 有 .als（cast_00-03/05 + CastBack drawonly） |
| character\…\ultimatecrossslash2.ani（槽295，交叉斩） | 10 | 730ms（20/20/60/20/160/90×5） | F0=10001、F2=10002（100ms）、F4=10003（280ms） | 无 | 有 .als（endattack_00-08 九层链 @F3） |
| effect\…\ultimatecrossslash_attack.ani（PO 槽60，伤害体） | 10 | 730ms（同构 295） | F2=10001/F4=10002/F8=10003 | **F4-F7：-800,-800,-800,1600,1600,1600** | 16 单位立方判定 |
| shared\…\vsultimatecrossslashappear_body.ani（PO 槽61，幻鬼） | 7 | 660ms | 无 | 无 | 纯视觉 + .als |
| 同目录 vs*endattack/appear 系列 30+ 个 | — | 470~3280ms | 部分有 | endattack_body 有（-400,-150,-106,860,300,539） | 族共享视觉池（本技能直接引用者见 §4） |

ultimatecrossslash.atk（.chr #161，实测）：physic / damage reaction **down** / push aside **40** / lift up **30** /
**ignore weight 1** / blood 150×3.0 / knuck back -1 / 音效 R_FLASHCUT_HIT。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ultimatecrossslash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\ultimatecrossslash.skl` | ✅ | 技能数据（2 列 + static 2 槽） |
| 注册行 | load_state 行 160（状态 123/技能 48）；行 18（PO 24349） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 49/146/160/465/466/552 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画 294/295/攻击信息 161/BladeSpirit 123 |
| 主 nut | ultimatecrossslash.nut（176 行） | `…\pvf\sqr\character\swordman\5_ghostsword\ultimatecrossslash\ultimatecrossslash.nut` | ✅ | §2.2 |
| ap nut | ap_ultimatecrossslash.nut（77 行） | `…\pvf\sqr\character\swordman\5_ghostsword\ultimatecrossslash\ap_ultimatecrossslash.nut` | ✅ | 强控（本 pvf 未启用） |
| 共享 PO 回调 | setcustomdata.nut（case 57/58）、onattack.nut（case 57） | `…\pvf\sqr\shared_passive_object\swordman\` | ✅ | F5 链路 |
| 共享 PO 定义 | swordman_shared.obj（etc motion #60/#61） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ | §2.3 |
| .chr 条目 | etc motion #294/#295；etc attack info #161（槽位号按 [etc motion] 0 基计数实测对名核对，未记行号） | `…\pvf\character\swordman\swordman.chr` | ✅ | ultimatecrossslash1/2.ani + .atk |
| 角色 .ani | ultimatecrossslash1/2.ani（另 cast_body/endattack_body.ani 在库无本技能引用者，见 §8） | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .als | ultimatecrossslash1/2.ani.als（+cast_body/endattack_body 各 1） | 同上 | ✅ | 施法/终结多层挂接 |
| 角色 .atk | ultimatecrossslash.atk | `…\pvf\character\swordman\attackinfo\ultimatecrossslash.atk` | ✅ | down/push40/lift30 |
| 特效 .ani | 施法侧 cast_00-05/EndAttack_00-08 + effect/ 子目录（light/endbackground/endfloor/endforeground 等） | `…\pvf\character\swordman\effect\animation\ultimatecrossslash\` | ✅ | §2.2 als_ani 引用 5 个 + .als 链 |
| PO 视觉 | vs* 系列 30+ 个 + 4 个 .als | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\animation\ultimatecrossslash\` | ✅ | 幻鬼现身/终结（族共享池） |
| 装备层 | 未查（时间预算；本技能 50 级觉醒，avatar 层存在概率高——**未考证**） | `…\pvf\equipment\character\swordman\avatar\` | 未考证 | — |

## 4. 资源需求

跨目录借图多（L14 常态）；幻鬼族视觉池跨技能复用（BladeSpirit/SpiritMove/TeleportVS 系）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色两段动画 | 必需（共享） | ✅ 已在库 |
| Attack.img | sprite_character_swordman_effect_ultimatecrossslash.NPK | 交叉斩主判定视觉（ultimatecrossslash_attack.ani） | 必需 | ❌ |
| A01/A02、B01、Back01/Back1/Back2 | 同上 | 斩击光弧层（cast/endattack 系） | 必需（可选 2-3 张） | ❌ |
| EndNomal01/02、EndDodge01/02、Ligth01 | 同上 | 终结爆发/光柱 | 必需（可减张） | ❌ |
| Smoke01/03/04/05 | 同上 | 烟尘层 | 可选 | ❌ |
| Last01B、Last02、sword.img | 同上 | PO 侧终结/刀身（vs* 系引用） | 可选 | ❌ |
| 000/002/003/004/00704.img | sprite_character_swordman_effect_bladespirit.NPK | 幻鬼现身（BladeSpirit 系，cast 系引用） | 必需（幻鬼本体） | ❌ |
| VengeanceSpirit(_Dodge).img | sprite_character_swordman_effect_bladespiritdot.NPK | PO 侧幻鬼 | 可选 | ❌ |
| SpiritMove01、Smoke03 | sprite_character_swordman_effect_spiritmove.NPK | 移形烟雾 | 可选 | ❌ |
| Mist.img | sprite_character_swordman_effect_returnspiritmove.NPK | 归魂雾 | 可选 | ❌ |
| LDodge、Normal、Smoke02 | sprite_character_swordman_effect_teleportvs.NPK | 传送闪光 | 可选 | ❌ |
| Circle.img；Smoke11A/B/C | sprite_common_commoneffect_glow / _smoke.NPK | 通用光圈/烟（Common 借图） | 可选 | ❌ |

缺失 img：必需级约 10 张（3 个 NPK）、可选级约 13 张（6 个 NPK）——**F5 族技能共享视觉池，族内一次提取多技能复用**。img 版本红线由提取时把关。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/UltimateCrossSlashSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发 + 二段子状态范式）
   - `CooldownMs=30000`（DNF 原值 145000，demo 缩短）；`TotalTimeMs=2020`（1290+730）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanUltimateCrossSlash1)` + `ctx.ClearHitTargets()`。
   - OnUpdate（GetElapsedMs + SubState 推进）：
     - `≥800 && SubState==0`：`ctx.SetSubState(1)`——幻鬼现身标记（视觉走技能本体 overlay 或前置 Area，见下）；
     - `≥1290 && SubState<=1`：`ctx.PlayAnim(AnimId.SwordmanUltimateCrossSlash2)`；`ctx.MoveCasterForward(1.5)`（突进 150px 一次性，DNF 帧内收敛）；`ctx.CreateAreaInFront(AreaIds.UltimateCrossSlash, 0.8)`（伤害区随创建点固定——DNF 是 moveWithParent 跟随，突进一次性位移后位置等效）；`ctx.SetSubState(2)`；
     - `≥1570 && SubState==2`：终结表现时刻（屏震档，跳过）`ctx.SetSubState(3)`。
   - OnEnd：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/UltimateCrossSlashArea.cs : AreaDefinition`**（交叉斩，同 ReleaseWaveArea 一次性爆发范式）
   - `TotalTimeMs=730`、`EnterActions={MeleeHit}`、
     `HalfExtents=(8.0,8.0,8.0)`（PO 盒 1600 立方折算 ÷200——16 单位全宽即 ±8；demo 可收窄到 (5,2,5) 防误伤全屏）、
     `HitReaction{Damage=300, HitstunMs=800, KnockbackX=40, LaunchY=30}`（atk 161 原值 down/push40/lift30）；
   - `ViewAnimId=AnimId.UltimateCrossSlashAttack`（attack.ani 主层）+ overlay 手组装 EndNomal/光弧层（.als 翻译件）。
   - 幻鬼现身视觉：`AnimId.UltimateCrossSlashGhostAppear` 以技能本体 overlay（.als 翻译链 startFrame=800ms 对应帧）挂接，
     或最简版并入 Area ViewBackAnimId——demo 二选一。
3. **无需新 Buff/Action**（强控本 pvf 未启用；无敌帧缺失跳过）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 123 两子状态（anim 294/295） | `UltimateCrossSlashSkill` 两段 PlayAnim + ElapsedMs 驱动 |
| PO 24349 dword 58（幻鬼纯视觉） | 技能 overlay / Area 背层视觉 |
| PO 24349 dword 57（moveWithParent 伤害体） | `CreateAreaInFront`（位移一次性后等效）+ 大 HalfExtents |
| atk 161（down/push40/lift30/ignore weight） | Area `HitReaction`（ignore weight 无消费：跳过） |
| 范围% col1（100-110） | 固定 HalfExtents（等级缩放延后） |
| BladeSpirit 被动加速动画 | 跳过（动画速度门面未暴露） |
| Lv3 无敌（UNBREAKABLE） | 无敌帧缺失（R1-A5）——跳过 |
| 强控 appendage（static[0]=0 未启用） | 若启用需目标状态控制（缺失档）；StunBuff 近似备选 |
| 黑屏闪屏/屏震/镜头 | 延后——跳过 |

### 注册点清单（草案号段，A14 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.UltimateCrossSlash=20` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanUCS1=86、SwordmanUCS2=87、UltimateCrossSlashAttack=88、UltimateCrossSlashGhostAppear=89、UltimateCrossSlashLight=90、UltimateCrossSlashEndAttack=91 |
| AreaId | `AreaDefinition.cs` | UltimateCrossSlash=12 |
| json / 图集 | LSAnimClipRegistrar / BuildAtlas | json ×4~6 + overlay ×2；img 必需 ~10 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 145000ms | 30000 |
| 总时长 | 2020ms（1290+730） | 2020 |
| 幻鬼现身 | F9 = 800ms，身前 200px | ElapsedMs 800（overlay） |
| 突进 | 150px（撞墙收敛） | MoveCasterForward 1.5 一次 |
| 伤害窗口 | PO F4-F7 = 1410-1840ms | Area 创建即 EnterActions 单次 |
| 判定 | 16×16×16 单位立方 ×110% | (5,2,5)（demo 收窄）或 (8,8,8) |
| 伤害 | col0 12680%（atk down/push40/lift30） | 300/硬直 800/推 40/浮 30 |
| 里程碑 | Lv3 无敌/Lv6 +10% 范围/Lv9 +10% 攻 | 跳过（无等级系统） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ultimatecrossslash1/2.ani.als（及 PO 侧 4 个 .als） | `[use animation]`+`[none effect add]`+`[create draw only object]`（CastBack_00，无 follow-parent 变体） | 前两者已支持；[create draw only object] **已记档缺口**（R1-A4/R2-A10，本技能第 4 实证）——建议 als 子命令按 [add] 同构输出 |
| ultimatecrossslash.skl（2 列）+ ultimatecrossslash.atk | `.skl`/`.atk` 无子命令（atk 含 [ignore weight]/[knuck back] HitReaction 外字段） | 手抄量小；[ignore weight] 并入 atk 子命令字段设计清单（R2-A8 记档再+1） |
| swordman_shared.obj | `.obj` 无子命令 | F5 族共用一份，并入既有缺口（L9） |
| ani 层面 | 节面常规（实测无规则外新节） | 现有 ani 子命令全覆盖 |

计 4 条既有缺口（.skl/.atk/.obj/[create draw only object]），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 伤害体跟随施法者（moveWithParent） | Area 无跟随宿主能力（记档：Area 跟随） | 突进改一次性位移后创建，位置等效 |
| 16 单位立方判定 | 无（可表达，HalfExtents 直配） | demo 收窄防全屏误伤 |
| Lv3 施放无敌 | 无敌帧（缺失，R1-A5） | 跳过 |
| 强控 800px 内敌人 1s（HOLD 钉定） | 目标状态控制（缺失档；**本 pvf static[0]=0 未启用**） | 跳过；若做近似用 StunBuff |
| BladeSpirit 被动加速动画 | 动画速度门面 + 被动等级查询（延后） | 固定速度 |
| 黑屏闪屏/屏震/音效 | 闪屏/屏震/音频（延后） | 跳过 |
| 范围/攻击力等级缩放（Lv6/9） | 等级数值缩放（延后） | 固定值 |
| 终结多层特效（endattack 00-08 九层链 + als_ani 5 处） | .als 翻译已通；img 量大 | 主层 + 2 子层先做，其余渐进 |

## 8. 存疑与缺口上报

**未考证项**
1. `ultimatecrossslashcast_body.ani` / `ultimatecrossslashendattack_body.ani`（角色 animation 目录、带 .als）的引用者——本技能 nut 只用 294/295；疑 terriblekilling（剑影二觉 49?）或引擎备用，未查。
2. PO 槽 62（vsultimatecrossslashendattack_body，34 帧 3280ms 带攻击盒）的创建者 = setcustomdata case 59——本技能不创建，属族内其他技能（记档不展开）。
3. 前置技能 28 的名称（ghostsword 族，未读 skl）。
4. 装备层 avatar .ani 未查（存在概率高，未实测）。
5. case 57/58 的动画槽与 swordman_shared.obj etc motion 的 0 基配对（#60/#61）按 F5 规则直读——VS 族 -2 错位规则（R2-A10）只涉 atk 配对，motion 正常，本技能 atk 借用角色 .chr #161 不经 obj 配对，不受错位影响（已核）。

**系统级缺口（非新增，实证补充）**
- **Area 跟随宿主单位**（moveWithParent）：F5 族伤害体普遍跟随施法者，本技能 + 一闪/幻鬼步族都会撞——建议记入 §6.3 缺口清单（一次性位移可绕过，但"真跟随"需 LSArea 增加 FollowOwnerId）。
- 幻鬼实体在场判定（F5 R2-A6 已记档）：本技能幻鬼是临时演出体不依赖在场记忆——**F5 族内首个无 VSObject 依赖的技能**，可作为 F5 族首个低依赖直译样本（下轮经验）。
- 无敌帧：第 5 实证（R1-A5 起）。
- 翻译工具 [create draw only object]：第 4 实证。

**给下轮的经验**：5_ghostsword 目录的技能全走 F5 直查链（load_state 行 160 一族 + setcustomdata 按 dword 分流），
**先读 setcustomdata 的 case 分支即可拿到伤害体三要素（动画槽→obj etc motion、atk 借用方、缩放参数）**，不必扫 obj 全表；
本技能证明 F5 族也有"纯演出 PO（case 58 无 atk）+ 伤害 PO（case 57）"分工，读 case 有无 `sq_SetCurrentAttackInfo` 即可分类。
