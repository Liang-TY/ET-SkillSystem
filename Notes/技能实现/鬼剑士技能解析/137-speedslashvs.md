# 幻鬼 : 连斩（speedslashvs）

> 技能ID 137 | 级别 A | 可实现性 ✅（基础五段连斩直接可实现；无动作施放联动降级） | 分析日期 2026-08-22 | 批次 A8

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻鬼 : 连斩 | `skill\Swordman\ghostsword\speedslashvs.skl [name]` |
| 英文名 | speedslashvs（skl 文件名；无 [name2] 节） | 同上实测 |
| 职业 | 剑影（[skill fitness growtype]=5） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 60 | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ↑↑ + Z | 同上 [command] / [command key explain] |
| CD | 8000 ms（pvp 12000） | 同上 [cool time] |
| MP | 60 → 600（Lv1 → Lv60） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 前置 | 技能 135（oneslashvs 幻鬼：一闪VS）Lv1 | 同上 [pre required skill] |
| 可施放状态 | 0/8/14（常规）+ 111 鬼步/112 鬼连斩/115 鬼连牙/119 白鬼/122 断头/127 剑舞（六鬼技状态中**无施放动作施放**）+ 45/71/170/20/22（未考证） | 同上 [executable states] + header 常量对表 |
| static data | `100 0`：**nut 实证** col0=攻击盒/图像缩放 100%；col1=终结变体触发位（基础版=0 → 走 state11→12 主线；>0 时插一段 loopbody_add，疑由强化版 SpeedSlashVSEx 置 1） | skl + 共享 PO 回调 |
| 一句话效果 | 幻鬼快速连续斩击前方敌人造成物理伤害（4 斩 + 1 记砸地终结）；施放剑术技能过程中使用时可无施放动作立即出现幻鬼攻击 | 同上 [explain] |

**level property（4 列，Lv1 → Lv60）**：`632→5060`、`844→6748`、`1052→8436`、`1688→13328`。
nut 实证：col0/1/2/3 = 第 1/2/3/4 斩攻击力 ‰（`sq_GetBonusRateWithPassive(SKILL_SPEEDSLASHVS, -1, col, 1.0)`）；
**第 5 击（砸地终结）复用 col3**。

**等级外联动（攻速）**：动画速度 × `SpeedRate = 1 + 鬼影剑(123)列0`（setstate 回调头统一计算）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
164: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/speedslashvs/speedslashvs.nut", "speedslashvs", STATE_SPEEDSLASHVS, SKILL_SPEEDSLASHVS);
     // STATE_SPEEDSLASHVS=116 / SKILL_SPEEDSLASHVS=137（header 实测）
18:  IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);   // 共享判定 PO（F5）
```

判定体与视觉全部在共享 PO 24349（写包 dword **62**）——幻鬼本体演出（ready/loop/end/消失）与 5 段命中都在 PO 侧；
**角色本体只播一段 440ms 的"施放幻鬼"手势**，然后回待机，幻鬼独立打完全程。

### 2.2 主 nut 逐回调（speedslashvs.nut，116 行，实测通读）

**checkExecutableSkill_speedslashvs（双路径）**：
- 状态 0/8/14 → push 子状态 `[0]` 进 STATE_SPEEDSLASHVS（常规：播角色手势动画）。
- 状态 ∈ {111 鬼步, 112 鬼连斩, 115 鬼连牙, 119 白鬼, 122 断头, 127 剑舞} → **不切状态**（无施放动作）：
  `getVSObject(obj)` 取幻鬼实体 → 写 dword 62，`sq_SendCreatePassiveObjectPacketPos(24349, VSObject.x/y, 0)` 在幻鬼当前位置出 PO 并
  `sq_SendDestroyPacketPassiveObject(VSObject)` 销毁幻鬼；无幻鬼时在身前 70px 出 PO。

**onSetState（子状态 0，唯一）**：播 CUSTOM_ANI_SPEEDSLASHVS=299（`Animation/speedslashvs.ani`，.chr 1272 行对表）×SpeedRate；
随后**与上面同款**的 PO 创建逻辑（幻鬼位或身前 70px）。

**onEndCurrentAni（子状态 0）**：回 STATE_STAND。

### 2.3 共享 PO 24349（dword 62）——五状态幻鬼连斩机（实测）

`setcustomdata` case 62 仅 `sendStateOnlyPacket(10)` 转发；真身在 `setstate.nut` case 62（362-452 行）：

| PO 状态 | 动画（customAnimation 索引→文件） | 时长 | 攻击盒（min/max，px） |
|---|---|---|---|
| 10（幻鬼现身） | 67 → `speedslashvs_readybody.ani`（4f） | 260ms | 无（.als 挂 ReadyBody_01 前景层） |
| 11（第一轮连斩） | 68 → `speedslashvs_speedslashvs_loopbody.ani`（10f） | 560ms | F0-F2：x[-143,344] y[-35,70] z[0,160] |
| 12（第二轮连斩+收势） | 69 → `speedslashvs_endbody.ani`（8f） | 740ms | F1-F2：x[-200,433] z[0,160]；F5-F7：x[-96,329] z[0,266] |
| 13（残身消失+终结击） | 70 → `speedslashvsis_body.ani`（29f） | 3470ms | F3-F4：x[-149,463] y[-44,88] z[-8,170] |
| 14（终结变体插段，col1>0 才走） | 71 → `speedslashvs_speedslashvs_loopbody_add.ani`（10f） | 560ms | 同 11 |

**onKeyFrameFlag（命中编排，逐段切换攻击信息 + 段间 resetHitObjectList）**：

| PO 状态 | flag（帧@时刻） | 动作 | 伤害列 / atk |
|---|---|---|---|
| 11 | 10001（F0@0ms） | 第 1 斩：atk[32]=speedslashvs1.atk + 特效 speedslasheffect_01/dust01 | col0 |
| 11 | 10002（F5@240ms） | **resetHitObjectList** + 第 2 斩：atk[33]=speedslashvs2.atk + effect_02/dust02 | col1 |
| 12 | 10001（F1@40ms） | 第 3 斩：atk[34]=speedslashvs3.atk + effect_03/dust03 | col2 |
| 12 | 10002（F5@460ms） | **reset** + 第 4 斩：atk[35]=speedslashvs4.atk + effect_04/dust04 | col3 |
| 13 | 10001（F3@180ms） | 第 5 击（砸地终结）：atk[36]=**speedslashvs5.atk**（down/**lift -3000**/push200/bounce）+ 音效 + effect_05/dust05 | col3 复用 |
| 13 | 10002（F13@1330ms） | 消失特效（disappearfront/back，无命中） | — |
| 14 | 10001/10002 | 同 12 的两斩（atk34/35，col2/col3）——强化变体插段 | col2/col3 |

**onEndCurrentAni（状态推进）**：10→11；11→（int1>0 ? 14 : 12）；14→12；12→13；13→销毁。
即基础版主线 = 现身 260ms → 第一轮 560ms（斩1/斩2）→ 第二轮 740ms（斩3/斩4）→ 残身 3470ms（终结击后淡出）。

**从施放起算的命中时刻**（PO 创建=施放瞬间）：斩1@260ms、斩2@500ms、斩3@860ms、斩4@1280ms、终结@1740ms。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/speedslashvs.ani`（角色手势） | 9 | 440ms | F2@140ms=10001 | 无 | 无 .als 边车（实测目录）；播完角色回待机，幻鬼继续 |
| `…\unclebang…\animation\speedslashvs\speedslashvs_readybody.ani` | 4 | 260ms | — | 无 | .als `[none effect add]` 挂 ReadyBody_01 @z10002 |
| `…\speedslashvs_speedslashvs_loopbody.ani` | 10 | 560ms | F0=10001、F5=10002 | F0-F2（见上表） | 第一轮连斩 |
| `…\speedslashvs_endbody.ani` | 8 | 740ms | F1=10001、F5=10002 | F1-F2、F5-F7 | 第二轮+收势 |
| `…\speedslashvsis_body.ani` | 29 | 3470ms | F3=10001、F13=10002 | F3-F4 | 终结击后长淡出（末帧 300ms） |
| `…\speedslashvs_speedslashvs_loopbody_add.ani` | 10 | 560ms | F0/F5 | 同 loopbody | 终结变体（基础版不走） |
| 特效族 | speedslasheffect_01~05 / dust01~05 / disappear 系 | — | — | — | 斩击剑气+尘土+消失 |

**命中反应五连（atk 实测）**：

| atk | 反应 | push | lift | 特记 |
|---|---|---|---|---|
| speedslashvs1 | damage | 50 | 135 | vs opposite cut |
| speedslashvs2 | damage | 50 | 135 | hit down |
| speedslashvs3 | damage | 50 | 120 | hit down |
| speedslashvs4 | **down** | 75 | **350** | 第 4 斩击倒浮空 |
| speedslashvs5 | **down** | 200 | **-3000**（砸地） | bounce 1 + bounce up lift 300（落地弹跳） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | speedslashvs.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\speedslashvs.skl` | ✅ | 4 列攻击力/static 100 0 |
| 注册行 | swordman_load_state.nut:164 / :18 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 116 + PO 24349 |
| 主 nut | speedslashvs.nut（116 行） | `…\pvf\sqr\character\swordman\5_ghostsword\speedslashvs\speedslashvs.nut` | ✅ | 双路径施放（常规/无动作） |
| 常量表 | swordman_header.nut:470 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | CUSTOM_ANI_SPEEDSLASHVS=299 |
| .chr 条目 | etc motion 299（1272 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | Animation/speedslashvs.ani |
| 角色 .ani | speedslashvs.ani（无边车） | `…\pvf\character\swordman\animation\` | ✅ | 440ms 手势 |
| 共享 PO 回调 | setcustomdata.nut:889 / setstate.nut:362-452 / onkeyframeflag.nut:492-612 / onendcurrentani.nut:416-459 | `…\pvf\sqr\shared_passive_object\swordman\` | ✅ | dword 62 全部分支 |
| 共享 PO 定义 | swordman_shared.obj | `…\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ | motion[67-71]、atk[32-36] 索引对表 |
| PO .atk | speedslashvs1~5.atk | `…\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\` | ✅ | 五段命中反应 |
| PO .ani | speedslashvs/ 目录 40+ 文件（含 5 个 .als） | `…\passiveobject\unclebang_shared_passive_object\swordman\animation\speedslashvs\` | ✅ | 幻鬼演出全套 |
| 幻鬼实体 | getVSObject 相关（跨 nut 共用函数） | `…\pvf\sqr\character\JG_SwordMan\jg_swordman_common.nut`（定义定位） | ✅（函数族存在） | 幻鬼对象创建/定位/销毁 |
| 装备层 | speedslashvs 相关 76 个文件 | `…\pvf\equipment\character\swordman\avatar\` | ✅（只查存在性） | avatar 变体图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色手势动画 | 必需（共享） | ✅ 在库 |
| SpeedSlashVS/Slash01~04.img、SSVSIS_fluid.img | sprite_character_swordman_effect_speedslashvs.NPK | 四斩剑气+残身流体 | **必需**（幻鬼斩击主视觉） | ❌ |
| BladeSpiritDot/VengeanceSpirit.img、VengeanceSpirit_Dodge.img | sprite_character_swordman_effect_bladespiritdot.NPK | 幻鬼本体（readybody/loopbody 身形） | **必需**（幻鬼=本技能视觉主体） | ❌ |
| SpeedSlashVS 相关 dust/disappear 系引用的 Common/CommonEffect/Dust/Dust01.img 等 | sprite_common_commoneffect_dust.NPK | 尘土 | 可选 | ❌ |
| SpinningSlashVS/slash2_normal1~2.img、TeleportVS/{LDodge,Normal,Smoke01,Smoke02}.img、WindSpiritVS/ddd0000.img | 各自路径 sprite NPK | 消失/传送系特效（跨技复用，L14 常态） | 可选 | ❌ |
| Character/Mage/Effect/BroomSpin/G03.img | sprite_character_mage_effect_broomspin.NPK | 跨职业借用（法师扫帚旋贴图） | 可选 | ❌ |

缺失 img：必需级约 7 张（Slash01-04+fluid+VengeanceSpirit×2）、可选级约 8 张。
注：幻鬼身形若嫌多图集，最小 demo 可只保留 Slash 系剑气 + 用 readybody/loopbody 的部分帧。

## 5. 实现方案草案

- **内容件清单**：
  - `SpeedSlashVsSkill : SkillLogic`（SkillIds.SpeedSlashVs=16；时间线驱动 + 段间 ClearHitTargets——L19 段间多段已落地）：
    - `CooldownMs=8000`；`TotalTimeMs=2100`（活跃命中窗到 1740ms+缓冲；3470ms 淡出尾巴截断）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSpeedSlashVs)`、`ctx.ClearHitTargets()`；elapsed≥440 后 `ctx.PlayDefaultAnim()`（SubState 守卫，角色先回待机，"幻鬼独立作战"观感）。
    - `OnUpdate` 时间线（elapsed 相对施放，帧号 const + SubState 一次性守卫，同 127-speedslash 草案模式）：
      - @260 斩1：`SetAttackHitbox(前偏1.9, (2.44,0.53,0.8))`（loopbody F0-F2 盒折算）+ SubState=1；
      - @500 斩2：`ClearHitTargets()`（段间重置）；
      - @860 斩3：`ClearHitTargets()` + 换盒（endbody F1-F2 → 前偏1.2、(3.17,0.6,0.8)）；
      - @1280 斩4：`ClearHitTargets()`；
      - @1740 终结击：`ClearHitTargets()` + `DisableAttackHitbox()` + `ctx.CreateAreaInFront(AreaIds.SpeedSlashVsFinish, 1.6)`（砸地终结区，独立 HitReaction）；
      - @2090 `DisableAttackHitbox()`（兜底）。
    - 技能 `HitReaction{Damage=100, HitstunMs=500, KnockbackX=50, LaunchY=135}`（斩1-4 统一，atk1-3 原值口径）；`HitActions={MeleeHit}`。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - `SpeedSlashVsFinishArea : AreaDefinition`（AreaIds 新增，同 ReleaseWaveArea 一次性范式）：`TotalTimeMs=400`、`TickTimeMs=0`、
    `EnterActions={MeleeHit}`、`HalfExtents=(3.1,0.7,0.85)`（is_body F3-F4 盒折算）、
    `HitReaction{Damage=180, HitstunMs=800, KnockbackX=200, LaunchY=0}`（speedslashvs5.atk：down/push200/lift-3000 砸地——LaunchY 取 0 落地，弹跳 bounce 跳过）、`ViewAnimId=AnimId.SpeedSlashVsFinishFx`（speedslasheffect_05 视觉）。
  - **无需**新 Buff/Action/Bullet。
- **概念映射**：PO 24349 dword62 五状态机 → 技能 OnUpdate 时间线（五次固定盒命中 + 段间 ClearHitTargets）+ 终结 Area；
  resetHitObjectList → `ctx.ClearHitTargets()`（L19）；无动作施放/VSObject → 不做（§7）；幻鬼演出动画 → Area/技能 ViewAnim（弹体视图自推帧同款，逻辑零动画状态）。
- **注册点**：SkillIds.SpeedSlashVs=16 + ButtonToSkill 新键；AnimIds `SwordmanSpeedSlashVs=63、SpeedSlashVsLoop=64、SpeedSlashVsEnd=65、SpeedSlashVsIs=66、SpeedSlashVsFinishFx=67`（readybody=可选并入 63 段）；
  LSAnimClipRegistrar ×5；BuildAtlas 加 Slash01-04/VengeanceSpirit 图集；LSOperaComponentSystem 新键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 8000ms | 8000（直用） |
| 总时长 | PO 主线 260+560+740=1560ms + 残身淡出 3470ms | 2100（截淡出尾） |
| 斩1-4 伤害 | col0-3：632/844/1052/1688‰（Lv1） | 统一 MeleeHit 100 |
| 斩1-4 反应 | push50/lift135、135、120、350(击倒) | 统一 {Kb50, Ly135}（第4击的差异由终结区补足） |
| 终结击 | col3 复用 1688‰；down/push200/lift-3000/bounce300 | {Damage180, Hitstun800, Kb200, Ly0} |
| 判定盒 | loopbody x[-143,344] / endbody x[-200,433] / is x[-149,463] | 前偏 1.9/1.2/1.6，半尺寸见 §5 |
| 命中时刻 | 260/500/860/1280/1740ms | 直用（时间线 const） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| speedslashvs.skl | `.skl` 无子命令（4 列） | 手抄可行；批量化时加子命令（常驻缺口） |
| speedslashvs1~5.atk | `.atk` 无子命令；speedslashvs5 含 `[bounce]`/`[bounce up lift up]`（落地弹跳）与负 lift | 手抄；atk 子命令设计时纳入 bounce 字段（消费侧需 LSFlight 落地反弹，暂跳过） |
| speedslashvs_readybody.ani.als 等 5 个 .als | `[none effect add]` **已支持**（AlsParser 兼容，L12）——非缺口 | — |
| speedslashvsis_body.ani | `[SET FLAG]`/`[PLAY SOUND]` 跳过 | 触发时刻进时间线 const——非缺口 |
| swordman_shared.obj | `.obj` 无子命令 | 手工映射（F5 族常驻缺口） |

结论：.ani/.als 全部可被现有子命令翻译；实质缺口 = `.skl`/`.atk`/`.obj` 三子命令 + atk 的 bounce 字段记档，计 4 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 剑术技能施放中"无施放动作"立即出幻鬼（六鬼技状态联动 + VSObject 定位/销毁） | 技能取消体系（缺失档，R1 上报）+ 幻鬼实体体系缺失（本技能首个明确依赖 VSObject 的样本） | demo 只做常规路径；PO 出生固定身前 0.7 单位（DNF 无幻鬼时同为 70px） |
| 幻鬼实体（VSObject）作为可定位/可销毁/可替换的独立对象 | 无召唤物/独立实体概念（§6.3 缺失：召唤物独立 AI——此处是非 AI 的纯定位锚点，轻量） | 若后续做幻鬼体系，建议先落"锚点实体"（无 AI、仅位置），F5 族多技能受益 |
| 第 4 斩 down+lift350 → 终结击 lift-3000 砸地 + bounce 弹跳 | LSFlight 无落地反弹（bounce）；技能级 HitReaction 单值（五段不同反应） | 斩1-4 统一反应；终结击独立 Area（064 多相位定案同构）；弹跳跳过 |
| 攻速联动 ×SpeedRate | 无 ctx 动画速度门面 | 固定 1.0 |
| 音效/消失特效（disappearfront/back） | 音频缺失/特效可选 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static col1>0 的终结变体（state 14 插段）的开启条件——疑 SpeedSlashVSEx（技能 318，E 批）置位，未读该 skl 证实。
2. `[executable states]` 中 45/71/170/20/22 号状态归属（与 136 同款疑点，族内共性）。
3. getVSObject 幻鬼实体的创建者（疑 spiritmove/oneslashvs 等前置技，属 126/135 技能的课）。

**新系统级缺口（§6.3 清单外）**
1. **幻鬼锚点实体（VSObject 轻量版）**：F5 族"幻鬼技能"共享一个可定位的幻鬼对象（创建→各技在其位置出判定→销毁/替换）。非 AI、非独立战斗，仅位置锚点——建议以极简 LSAnchorEntity 立项，收益覆盖 5_ghostsword 全族。
2. （沿用上报）跨技能取消体系：本技能"六鬼技状态中无动作施放"是 R1 已上报缺口的又一实证，且**不需位移、只需绕过在技门禁 + 位置接力**，可作为该缺口的最小落地样板。

**翻译工具缺口（并入主循环汇总）**：`.skl`/`.atk`/`.obj` 子命令（常驻 3 条）；atk `[bounce]`/`[bounce up lift up]` 字段记档（消费侧同步）。
