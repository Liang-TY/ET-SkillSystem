# 鬼连牙（ghostpierce）

> 技能ID 136 | 级别 A | 可实现性 🔶（基础刺击路径 ✅，拉拽/鬼步联动降级） | 分析日期 2026-08-22 | 批次 A8

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼连牙 | `skill\Swordman\ghostsword\ghostpierce.skl [name]` |
| 英文名 | ghostpierce（skl 文件名；无 [name2] 节） | 同上实测 |
| 职业 | 剑影（[skill fitness growtype]=5；5_ghostsword 目录） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 60（各觉醒段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | →←→ + Z（指令施放 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 8000 ms（pvp 10000） | 同上 [cool time] |
| MP | 46 → 483（Lv1 → Lv60） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 前置 | 技能 127（鬼连斩 speedslash）Lv1 | 同上 [pre required skill] |
| 可施放状态 | 0（站立）/ 8（攻击）/ 14（跳？） | 同上 [executable states] |
| static data | `100 200`：**nut 实证** col0=攻击盒/图像缩放 100%、col1=受击僵直倍率 200%（÷100=2.0 传入 `sq_SetAttackInfoHitDelayRateDamager`） | skl + ghostpierce.nut/共享 PO case45 |
| 一句话效果 | 聚灵魂之力于剑上强力刺击：命中者僵直并被拉到剑影前方，可推开霸体敌人，对倒地/浮空者强制拉到面前；鬼步准备姿势下按技能键发动特殊功能 | 同上 [explain] |

**level property（1 列，Lv1 → Lv60）**：`3862 → 30902`（每级 +392，近似线性）。
nut 实证：col0 = 刺击攻击力 ‰——共享 PO case 45/46 均 `sq_GetBonusRateWithPassive(SKILL_GHOSTPIERCE, -1, 0, 1.0)`，单列同时喂常规刺击与终结刺击。

**等级外联动（攻速）**：动画速度 × `SpeedRate = 1 + 鬼影剑(123)列0`（常规段）/ `SpeedRateEx = 1 + 列1`（终结段），onSetState 实测（同 127-speedslash 族）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
156: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/ghostpierce/ghostpierce.nut", "ghostpierce", STATE_GHOSTPIERCE, SKILL_GHOSTPIERCE);
     // swordman_header.nut: STATE_GHOSTPIERCE <- 115 / SKILL_GHOSTPIERCE <- 136（第4参状态号115、第5参技能ID136，L2 语义）
18:  IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);   // 共享判定 PO（F5，全 5_ghostsword 族共用）
```

判定体走 **F5 unclebang 链路**：写包首 dword（本技能 41/45/46）→ `sqr\shared_passive_object\swordman\*.nut` 六回调分派 →
对象定义 `passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj`（etc motion / etc attack info **0 基索引**）。

### 2.2 主 nut 逐回调（ghostpierce.nut，216 行，实测通读）

**checkExecutableSkill_ghostpierce**：`sq_IsUseSkill(SKILL_GHOSTPIERCE)` → push 子状态 `[0]` 进 STATE_GHOSTPIERCE（常规施放路径）。

**onSetState（子状态 0/1/2/3 四相，setSkillSubState）**：
| 子状态 | 动画（CUSTOM_ANI_*） | 动作 | PO 24349 写包 |
|---|---|---|---|
| 0（起势） | GHOSTPIERCE1=280（ghostpierce1.ani） | 音效 R_SM_GHOST_PIERCE_01_2；sq_StopMove；速度×SpeedRate | — |
| 1（刺击） | GHOSTPIERCE2=281（ghostpierce2.ani） | 速度×SpeedRate | 写 **45** → CreatePO(0,0,0) |
| 2（鬼步终结·贴身收拢） | GHOSTPIERCE_CONTACT=282（ghostpierce_contact.ani） | 速度×SpeedRateEx；计算 xDistance=鬼步(126) int0 的方向距离点 | 写 **41** → CreatePO(300,0,0) |
| 3（终结刺击） | GHOSTPIERCE2=281 复用 | 速度×SpeedRateEx | 写 **46** → CreatePO(0,0,0) |

**onEndCurrentAni（状态机推进）**：0→1；1→STATE_STAND；2→3；3→STATE_STAND。
即常规路径 = 0→1→待机（一次刺击）；鬼步特殊功能路径 = 2→3（贴身收拢→终结刺击），**子状态 2 的入口不在本 nut**——由
`JG_SwordMan\jg_swordman_common.nut:748 SpiritMoveContact` 的 `GhostSwordSetState(obj, SKILL_GHOSTPIERCE, [2], STATE_GHOSTPIERCE)` 在鬼步接触窗按下技能键时切入（实测）。

**onKeyFrameFlag（子状态 1/3，flag 10001 = ghostpierce2.ani F2@60ms）**：音效 R_SM_GHOST_PIERCE_02 + `sq_SetMyShake(12,120)` 屏震 +
`als_ani` 播特效 `character/swordman/effect/animation/ghostpierce/ghostpierceattack_08.ani`（z=60，随 col0 缩放、随 SpeedRate 加速）。

**onProc（子状态 2）**：`sq_GetUniformVelocity(当前X, xDistance, t, 500)` —— 500ms 匀速冲向鬼步距离点（`sq_MoveToNearMovablePos` 带碰撞止损）。

**onProcCon（子状态 1/3，帧≥6）**：`whiteGhostSlshContact(obj)`（jg_swordman_common.nut:846 实测）——开启白鬼连斩(119)技能指令并允许按键切入 = **跨技能取消窗口**（吃技能取消体系缺口）。

### 2.3 共享 PO 24349 回调（dword 41/45/46，实测）

| dword | setcustomdata 分支 | 动画/攻击信息 | 命中行为 |
|---|---|---|---|
| 45（常规刺击） | case 45 | motion[53]=`effect/animation/ghostpierce/ghostpierce_attack.ani`；atk=角色 `attackinfo\ghostpierce.atk`（CUSTOM_ATTACK_INFO_GHOSTPIERCE=153，.chr etc attack info 第 153 项实测对表） | 伤害=col0；受击僵直×col1/100=**2.0**；盒/图像缩放 col0/100=1.0 |
| 46（终结刺击） | case 46 | 同上（速度×SpeedRateEx） | 同上，但 **size=2.0**（判定/视觉加倍） |
| 41（鬼步终结·幻影穿刺） | case 41 | motion[50]=`effect/animation/spiritmove/spiritmovedasheffect_00.ani`；atk[30]=`unclebang…\attackinfo\spiritmove.atk` | 伤害=鬼步(126) col0；**多段**：`setTimeEvent(0, 动画总时长/HitCount)` + `sq_SetMaxHitCounterPerObject(HitCount)`，HitCount=鬼步 int2 |

**onAttack（case 45/46，每次命中，onattack.nut:84-91 实测）**：
```
在受击者身上播 hit_02.ani（挂其 Z 中心）
CNSquirrelAppendage.sq_AppendAppendage(damager, parentChr, …, "…/ghostpierce/ap_ghostpierce.nut", true)
```

**ap_ghostpierce.nut（拉拽 appendage，50 行实测）**：`onStart` 把宿主（受击者）
`sq_MoveToNearMovablePos` 到 **source（施法者）朝向前方 200px** ——即"拉到剑影前方"的瞬移拉拽；proc 仅做存活校验。

**onEndCurrentAni（case 41/45/46）**：动画播完即销毁 PO。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/ghostpierce1.ani`（角色·起势） | 1 | 359ms | — | 无 | .als：`[create draw only object]` F0 挂 Cast_06 |
| `character/swordman/animation/ghostpierce2.ani`（角色·刺击） | 11 | 468ms | **F2@60ms=10001** | 无（判定在 PO） | .als：AttackDust_03@F2、EndSmoke_01@F9（均 `[create draw only object]`） |
| `character/swordman/animation/ghostpierce_contact.ani`（角色·鬼步终结收拢） | 6 | 360ms | — | 无 | |
| `character/swordman/effect/animation/ghostpierce/ghostpierce_attack.ani`（PO 判定层，dword45/46 用） | 11 | 468ms | F2@60ms=10001 | **F2@60/F3@100/F4@140**：`-68 -35 0 325 70 140`（min/max：x[-68,325] y[-35,70] z[0,140] → 前伸 3.25 单位、高 1.4） | 角色动画与 PO 判定层逐帧时间轴完全同步（同一节奏两份） |
| ghostpierce 特效族（effect\animation\ghostpierce\） | cast_00~06 / attackdust_00~03 / endsmoke_00~03 / hit_00~02 / ghostpierceattack_00~08 | — | — | — | hit_02=命中挂件；ghostpierceattack_08=flag10001 主特效 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ghostpierce.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\ghostpierce.skl` | ✅ | 技能数据（1 列攻击力/static 100 200） |
| 注册行 | swordman_load_state.nut:156 / :18 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 115 注册 + 共享 PO 24349 注册 |
| 主 nut | ghostpierce.nut（216 行） | `…\pvf\sqr\character\swordman\5_ghostsword\ghostpierce\ghostpierce.nut` | ✅ | 子状态机 0→1（常规）/2→3（鬼步特例） |
| 拉拽 appendage | ap_ghostpierce.nut（50 行） | `…\pvf\sqr\character\swordman\5_ghostsword\ghostpierce\ap_ghostpierce.nut` | ✅ | 命中者瞬移到施法者前方 200px |
| 常量表 | swordman_header.nut:451-453/544 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | CUSTOM_ANI 280/281/282；CUSTOM_ATTACK_INFO_GHOSTPIERCE=153 |
| .chr 条目 | etc motion 280-282（1253-1255 行）；etc attack info 120 与 153（1414/1447 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | ghostpierce1/2/contact.ani；ghostpierce.atk ×2 |
| 角色 .ani | ghostpierce1/2/contact.ani（+ghostpierce1.ani.als、ghostpierce2.ani.als） | `…\pvf\character\swordman\animation\` | ✅ | 角色动作（无攻击盒） |
| 角色 .atk | ghostpierce.atk | `…\pvf\character\swordman\attackinfo\ghostpierce.atk` | ✅ | 刺击命中参数（PO 借用，见 §2.3） |
| 共享 PO 回调 | setcustomdata/setstate/onendcurrentani/onkeyframeflag/onattack.nut | `…\pvf\sqr\shared_passive_object\swordman\` | ✅ | dword 41/45/46 分支（setcustomdata:601/653/674、onattack:84-91 等） |
| 共享 PO 定义 | swordman_shared.obj（etc motion 94 项 / etc attack info 47 项） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ | motion[50]/[53]、atk[30] 索引对表 |
| PO .atk | spiritmove.atk | `…\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\spiritmove.atk` | ✅ | dword41（鬼步幻影穿刺）命中参数 |
| 特效 .ani | ghostpierce/ 全族 30+ 个 | `…\pvf\character\swordman\effect\animation\ghostpierce\` | ✅ | 刺击/尘/命中特效 |
| PO 特效 | spiritmovedasheffect_00.ani | `…\pvf\character\swordman\effect\animation\spiritmove\` | ✅ | dword41 幻影冲刺视觉（借用鬼步特效） |
| 装备层 | ghostpierce 相关 456 个文件 | `…\pvf\equipment\character\swordman\avatar\` | ✅（只查存在性） | 各 avatar 变体图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集（%04d 单图集） | 必需（共享） | ✅ sm_body0000.img.bytes 在库（L16） |
| GhostPierce/Slash.img、Cast.img、black.img、wiggle2.img、01/03/04.img、AttackBlack.img、DustADodge.img、Mist.img | sprite_character_swordman_effect_ghostpierce.NPK | 刺击剑气/蓄势/命中特效 | 可选（最小 demo 可仅角色动作） | ❌ |
| Common/CommonEffect/Dust/Dust02.img、Common/CommonEffect/Glow/Circle.img | sprite_common_commoneffect_dust / …_glow.NPK | 尘/光圈（跨目录复用，L14 常态） | 可选 | ❌ |

缺失 img：**必需级 0 张**（角色动作全走 sm_body 已入库图集）；可选级 10 张（+2 张 Common 复用）。
img 版本红线（v2/v4 可/v5 不可）由用户提取时把关。

## 5. 实现方案草案

- **内容件清单**（继承真实基类，均为现有机制）：
  - `GhostPierceSkill : SkillLogic`（SkillIds.GhostPierce=15；同 127-speedslash 草案的"PO 判定帧数据进技能类 const"思路）：
    - `CooldownMs=8000`（DNF 原值直用）；`TotalTimeMs=860`（起势 359ms + 刺击 468ms + 余量；鬼步特例段 demo 不做，见 §7）。
    - `OnCast`：`ctx.PlayAnim(AnimId.GhostPierce1)`、`ctx.ClearHitTargets()`。
    - `OnUpdate`（SubState 推进，bloodboom §4.7-7 同构）：elapsed≥359 且 SubState=0 → `ctx.PlayAnim(AnimId.GhostPierce2)`、SubState=1；
      elapsed≥419（=359+60，PO 判定层 F2）且 SubState=1 → `ctx.SetAttackHitbox(前偏1.3, 半尺寸(1.97,0.53,0.7))`（PO 盒 x[-68,325]/y/z 折算）+ SubState=2；
      elapsed≥539（判定层 F5）且 SubState=2 → `ctx.DisableAttackHitbox()` + SubState=3。
    - `HitReaction{Damage=120, HitstunMs=800, KnockbackX=50, LaunchY=150}`（atk 原值：push50/lift150/**force hit stun 800 直用**——"僵直"卖点就靠这 800ms）；`HitActions={MeleeHit}`。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - **无需**新 Area/Buff/Bullet/Action（拉拽简化掉，见 §7）。
- **概念映射**：PO 24349 dword45 判定 → 技能固定盒（SetAttackHitbox + 帧时点 const）；ap_ghostpierce 拉拽 → 简化为 800ms 强硬直（无位移）；flag10001 特效 → 可选 overlay/视图特效；
  SpeedRate 攻速联动 → 不做（动画 Speed 字段存在但无 ctx 门面，简化记档）。
- **注册点**：SkillIds 加 `GhostPierce=15` + ButtonToSkill 新键；AnimConfigRegistry 加 `GhostPierce1=59、GhostPierce2=60`（GhostPierceContact=61、GhostPierceAttack=62 可选）；
  LSAnimClipRegistrar 注册 ghostpierce1/2 json；BuildAtlas 无必需新增（sm_body 已在）；demo 按键映射在 LSOperaComponentSystem。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 8000ms | 8000（直用） |
| 总时长 | 起势 359 + 刺击 468 = 827ms | 860 |
| 刺击伤害 | col0：3862‰ → 30902‰（Lv1→60） | MeleeHit 固定 120 |
| 受击僵直 | force hit stun 800ms ×2.0 倍率（=1600ms 实效，hitDelay rate 语义未全证） | HitstunMs 800 |
| 击退/浮空 | push 50 / lift 150 | KnockbackX 50 / LaunchY 150 |
| 判定盒 | PO F2-F4：x[-68,325] y[-35,70] z[0,140] | 前偏 1.3、半尺寸 (1.97,0.53,0.7) |
| 拉拽 | 命中者瞬移至施法者前方 200px | 不做（800ms 硬直近似"被钉住"） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ghostpierce.skl | `.skl` 无子命令（1 列 level info + static） | 手抄可行；随批量化加 `skl` 子命令（常驻缺口） |
| ghostpierce.atk / spiritmove.atk | `.atk` 无子命令；且含 `[force hit stun time]`、`[ignore super armor]`、`[knuck back]`、`[hit wav]` 等 HitReaction 之外的参数 | 手抄；`atk` 子命令立项时把 force-stun/ignore-superarmor 纳入字段（本技能僵直 800 是核心手感） |
| swordman_shared.obj | `.obj` 无子命令 | 本技能手工映射（§5 已给），随 F5 族整体立项 |
| ghostpierce1.ani.als / ghostpierce2.ani.als | **`[create draw only object]`**（F0/F2/F9 挂特效，值结构=帧号+别名+3 参）——R1-A4 已记档缺口，本批再实证 2 处 | als 子命令按 `[add]` 同构支持（帧号/层/别名 + 独立绘制物标志） |
| ghostpierce2.ani `[SET FLAG]`/`[PLAY SOUND]` | 既有约定跳过 | 触发帧 const 进技能类——非缺口 |

结论：.ani 本体全部可译；实质缺口 = `.skl`/`.atk`/`.obj` 三子命令 + `[create draw only object]`，计 4 条（3 条常驻 + 1 条本族加重）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 命中拉拽：受击者瞬移到施法者前 200px（ap_ghostpierce） | **位移他人门面缺失（新缺口，§8）**——现有只有 LaunchOwner（沿施法者→受击者方向推离），无"拉近/牵引" | 用 800ms 强硬直表现"被钉在剑尖"；如需精确拉拽，给 SkillContext/LSActionContext 加 MoveUnitTowards 门面（改动小） |
| 鬼步准备姿势按技能键 → 子状态 2/3（贴身收拢+2 倍尺寸终结刺击） | 技能取消体系/状态门禁扩展（缺失档，R1 已上报） | demo 只做常规路径 0→1；鬼步特例待跨技能取消立项 |
| onProcCon 帧≥6 开启白鬼连斩取消窗 | 同上（跨技能取消） | 跳过 |
| 攻击倒地/浮空敌人时强制拉到面前 | 目标状态查询门面缺失（浮空/倒地判定，新缺口记档） | 跳过（统一反应） |
| 推开霸体敌人（atk ignore super armor 1） | 霸体帧系统延后（§6.3 延后档） | 无霸体概念时自然生效（都推得动），无需处理 |
| 攻速联动（鬼影剑 123 列 0/1 × 动画速度） | 无 ctx 动画速度门面（anim.Speed 字段已有） | demo 固定 1.0；后续加 ctx.SetAnimSpeed 即可 |
| 屏震 sq_SetMyShake(12,120) | 屏震延后（§6.3） | 跳过 |
| 音效 ×3 | 音频系统缺失 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. ghostpierce2.ani 刺击段受击僵直的实效值：atk force-stun 800 与 hitDelay rate 2.0 的叠加语义（推断 1600ms，未全证）。
2. dword 41 多段的 HitCount 实值（鬼步 126 的 int2 列，本批未读 spiritmove.skl——属 126 技能的课）。
3. speedslashvs 等 [executable states] 中 45/71/170/20/22 号状态归属（族内共性，未逐一考证）。
4. `ghostpiercecast/attack/end_body.ani`（.chr 223-225 槽）的引用者——疑 JG_SwordMan（剑鬼）版本专用，未证。

**新系统级缺口（§6.3 清单外）**
1. **位移他人门面（拉拽/牵引）**：ap_ghostpierce 的"瞬移到施法者前方 200px"无法用现有 API 表达（LaunchOwner 只有推离方向）。建议 SkillContext/LSActionContext 增加 MoveUnitTowards(target, 方向, 距离) 或 SetUnitPosition。幻影闪/吸扯类技能会复用。
2. **目标状态查询**（受击者是否浮空/倒地）：本技能"攻击倒地或浮空敌人时强制拉到面前"与后续 juggle 类技能都需要；建议 LSHitbox/LSFlight 暴露 IsAirborne 查询门面。

**翻译工具缺口（并入主循环汇总）**：`.skl`/`.atk`/`.obj` 子命令（常驻）；`[create draw only object]` als 节（本批 2 处再实证）；atk 字段 `[force hit stun time]`/`[ignore super armor]`。
