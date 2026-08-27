# 极 · 神剑术 (瞬斩)（flashcut）

> 技能ID 236 | 级别 A | 可实现性 🔶（标记列表直发/按住演出降级；判定主干可完整直译） | 分析日期 2026-08-22 | 批次 A17

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极 · 神剑术 (瞬斩) | `skill\Swordman\swordman_flashcut.skl [name]` |
| 英文名 | flashcut（取 skl 文件名去 swordman_ 前缀，系列惯例同 234/235） | 同上 |
| 职业 | 剑魂 · 剑神（极·神剑术系列第三式；skl 特征与 234/235 完全一致：[skill fitness second growtype]=2、[second growtype maximum level] 槽 3=30） | 同上 + 234/235 交叉 |
| 学习等级 | 80 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↓↑→ + Space（[command] 机读序列 DOWN,UP,RIGHT,SKILL；[command key explain] 原文写"←↓→ + Space"，两处不一致，以机读为准）；指令施法 MP 优惠 20%/40% 档 | 同上 [command] / [command key explain] / [skill command advantage] |
| CD | 50000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 800 → 6000（Lv1→70 列） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块（道具 3037）×5 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14（普攻/站立/受击系可接） | 同上 [executable states] |
| 一句话效果 | 拔刀演出后携飞剑急速前斩 370px，被飞剑命中的敌人被定住标记；收势时对全部被标敌人每 100ms 一道剑气连打 6 次，最后以强力一击收尾 | 同上 [explain] + PO 走读 |

**level property（5 列，模板逐占位对位，向量全为 `(-1,0..4,1.0)` level 直读）**：
col0 瞬斩距离 **370**（x 轴 px，恒定）、col1 驭剑术攻击力 **9716% → 48153%**（Lv1→Lv40）、
col2 剑气打击攻击力 **1570% → 7784%**、col3 剑气攻击次数 **6**（恒定）、col4 剑气最后一击攻击力 **20381% → 101024%**。

**与系列共性缺口修正**：本技能**没有蓄力段**——234/235 的"按住蓄力→demo 瞬发"共性缺口在 236 不存在；
236 是固定时间线演出（Ready 520ms → Atk 360ms → End 1020ms，全程无输入分支）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 46（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/flashcut/flashcut.nut", "swordman_flashcut", 236, 236);
// 行 8-13：共享打击 PO 24370（share_po_swordman_24370.nut + common_object/share_obj/swordman/ 六回调，L20/F7 同 234/235）
```

- `swordman_header.nut`：`STATE_SWORDMAN_FLASHCUT/SKILL_SWORDMAN_FLASHCUT <- 236`（行 68/96）、
  `CUSTOM_ANI_SWORDMAN_FLASHCUT{READY,ATK,END}_BODY <- 150/151/152`（行 320-322）、
  `CUSTOM_ATTACK_SWORDMAN_FLASHCUTDELAYATTACK{,FINISH} <- 93/94`（行 484/485）。
- .chr 对位（0 基，实测吻合）：etc motion #150/151/152 = 1123-1125 行 `FlashCutReady/Atk/End_Body.ani`（行号-973 ✓）；
  etc attack info #93/94 = 1387/1388 行 `FlashCutDelayAttack.atk` / `FlashCutDelayAttackFinish.atk`（行号-1294 ✓）。
  ⚠ #93/94 常量在 sqr 树内**无消费者**（grep 实测仅 header 定义行）——角色侧这两份 .atk 疑引擎残留，
  真正生效的同名 .atk 在 PO 侧目录（见 §2.3）。
- 主 nut 212 行，变量名被 mod 作者混淆（C3 族），语义已人工还原（下文）。

### 2.2 主 nut 逐回调（flashcut.nut，三子状态固定时间线）

**checkExecutableSkill**：`sq_IsUseSkill(236)` → (subState 0) 进状态 236。
**checkCommandEnable**：STAND 直接可；STATE_ATTACK 内走 `sq_IsCommandEnable(236)`。

**onSetState**（每子状态先 `sq_StopMove`，末尾统一 `sq_SetStaticSpeedInfo(攻速)`——攻速静态化，延后档）：

| 子状态 | 动画（.chr 槽） | 行为 |
|---|---|---|
| 0 预备 | #150 Ready_Body（9 帧 520ms） | 清 aniobj 列表；播动画；**黑屏开启**：flashScreen(start=Ready 前 6 帧累计 400ms, 99990ms, 0→130, 黑, BOTTOM)——即 t≈400ms 起画面压黑，直到 End 段收掉 |
| 1 前斩 | #151 Atk_Body（3 帧 360ms） | 震 8/150；挂父特效 `flashcutsmoke_eff_00.ani`(0,-1,0)；**写包创建 PO 24370**（见下）；建 draw-only 背景条 `flashcutbg_01.ani`(z+70) + pooled `flashcutbg_02.ani`（存 aniobj）；存向量 (起点x, col0=370 前斩距离) |
| 2 收势 | #152 End_Body（18 帧 1020ms） | 仅播动画 |

subState 1 的写包顺序（对照 level property 列）：
`[236, col1 驭剑术%, col2 剑气%, col4 最后一击%, col3 次数]` → `sq_SendCreatePassiveObjectPacket(24370, 0, 0, 0, 0)`（角色原位出生，飞剑判定随盒覆盖前斩路径）。

**onProc**（subState 1，前斩位移）：
```
x = 起点 + 方向 × uniform(0 → 370, 当前时间, 动画前 2 帧时长和 160ms)   // 370px/160ms 急速前冲
isMovablePos 撞墙检测 → 撞墙清向量止损（延后档：无地图碰撞）
```

**onProcCon**（纯视觉）：把 aniobj 里的两条背景条钉在屏幕右缘/屏幕 Y——镜头锁定的演出血条，跳过。

**onKeyFrameFlag**：
- subState 0 flag 1（Ready F3，t≈280ms）：震 8/100。
- subState 1 flag 1（Atk F2，t≈160ms 入段）：震 15/150。
- subState 2 flag 1（End F10，t≈460ms 入段 ≈ 全程 t1340）：**RemoveAllFlash（收黑屏）** + 白闪(0,50,300,240) +
  找到自己的 PO 24370（id 236）→ `addSetStatePacket(11)`——**剑气连打启动**。

**onEndCurrentAni**：子状态 0→1→2→STAND 顺序推进。
**onEndState**：离开状态 236 → RemoveAllFlash（防黑屏残留）。

### 2.3 共享 PO 24370（mod 版，case 236 两相位）

| 相位 | 触发 | 行为 | 动画 / 攻击信息（mod obj 0 基直读，本批实测复验） |
|---|---|---|---|
| state 10 飞剑 | 创建即（写包后 addSetStatePacket(10)） | 播飞剑动画（600ms，**F0 攻击盒** `x[-46,510] y[-70,140] z[-10,160]`，偏移格式÷100≈前伸 5.1×1.4×1.7 单位条带，覆盖前斩路径）；**onAttack 每次命中**：可抓/可_hold 敌人 → 移除旧 ap_flashcut → 重挂 `ap_flashcut.nut`（空壳，仅存活校验——hold 标记）+ `sq_HoldAndDelayDie(400, 300)`（定身钉住）→ push 进标记列表（obj_vector） | etc motion **#28** `FlashCut/FlashCutCut.ani`（IMAGE 空路径纯判定占位，LINEARDODGE）；atk **#14** `FlashCutCut.atk`：physic/weapon、reaction damage、cut、blood 1.0/10、**无 push/lift**、音 R_PUNTO_HIT |
| state 11 剑气连打 | 角色收势 End F10 flag 1（t≈1340） | `setTimeEvent(0, 100ms, ∞, repeat)`；**每拍**：计数(var0=col3) 递减；计数耗尽那一拍先 `removeAllAp`（解除全部定身）并换 atk56；**对全部标记敌人逐个 `sq_SendHitObjectPacket`**（无视位置直发，敌身高中点）；标记列表为空→销毁；最后一击发完→销毁；每次命中在敌位置随机生成 `fcfinish01-05_eff_00.ani` 命中视觉 | atk **#55** `FlashCutDelayAttack.atk`：physic/weapon、reaction **none**（纯伤害拍，无受击反应）、hit direction down；atk **#56** `FlashCutDelayAttackFinish.atk`：physic/weapon、reaction **down**、push **400**、lift **300**、no blood 100 |

procappend：state 10 期间父对象（角色）死亡/消失 → 销毁 PO。
onDestroy（else.nut case 236）：销毁时 removeAllAp（兜底解除定身）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/flashcutready_body.ani`（#150） | 9 | 520ms（50/50/120/60/60/60/40/40/40） | **F3=1**（震 8/100） | 无 | .als 挂 8 层 ready 特效（帧 0-3 起，层 -2..10006）；仅引 sm_body |
| `flashcutatk_body.ani`（#151） | 3 | 360ms（40/120/200） | **F2=1**（震 15/150） | 无 | .als 挂 4 层 atk 特效（全帧 0 起，层 -3..10001）；前斩位移窗 = 前 2 帧 160ms |
| `flashcutend_body.ani`（#152） | 18 | 1020ms（80 + 40×8 + 60×9 + 80/120） | **F10=1**（t≈460 入段：收黑屏/白闪/PO state 11） | 无 | .als 挂 8 层（帧 8/9/10 起，含 Smoke_01 @帧9 层-4） |
| PO `…qq506807329/…/FlashCut/flashcutcut.ani`（etc motion #28） | 2 | 600ms（300/300） | 无 | **F0**：`-46 -70 -10 510 140 160`（min/max 口径 x[-46,510]） | IMAGE 空路径占位（L7）+ LINEARDODGE（L15 已支持）+ [SHADOW] |
| effect/animation/flashcut/ 52 个（ready×8/atk×4/end×8+smoke×3/fcfinish01-05/bg×2） | 2-11 | — | 无 | 无 | 全 draw-only 视觉层；fcfinish/smoke/bg 带 .als ×8 |

`.als` 边车：3 个角色动画全有（[none effect add] 变体，L12 已支持）+ effect 目录 8 个。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_flashcut.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_flashcut.skl` | ✅（247 行） | 5 列等级数据 |
| 注册行 | load_state 行 46（236/236）+ 行 8-13（PO 24370） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 236 + 共享 PO |
| 常量 | swordman_header.nut 行 68/96/320-322/484-485 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画/攻击槽位 |
| 主 nut | flashcut.nut（212 行，混淆已还原） | `…\pvf\sqr\character\swordman\flashcut\flashcut.nut` | ✅ | 三子状态时间线 |
| ap nut | ap_flashcut.nut（18 行空壳） | `…\flashcut\ap_flashcut.nut` | ✅ | hold 标记 appendage（仅存活校验） |
| PO 回调 | common_object/share_obj/swordman/{setcustomdata,setstate,procappend,onendcurrentani,else}.nut 的 case 236 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅ | 飞剑标记 + 100ms 剑气节拍器 |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | etc motion #28、atk #14/#55/#56 对位（本批第三次独立复验 0 基无错位） |
| PO atk | FlashCutCut.atk / FlashCutDelayAttack.atk / FlashCutDelayAttackFinish.atk | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ | 飞剑 / 剑气每击 / 最后一击 |
| .chr 条目 | etc motion #150-152（1123-1125 行）+ etc attack info #93/94（1387/1388 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | 三动画 + 两份无消费者 atk |
| 角色 .ani | flashcut{ready,atk,end}_body.ani + .als ×3 | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | FlashCutDelayAttack.atk / FlashCutDelayAttackFinish.atk | `…\pvf\character\swordman\attackinfo\` | ✅（无脚本消费者） | 与 PO 侧同名文件内容一致（DelayAttack 实测相同；疑引擎残留双份） |
| PO .ani | flashcutcut.ani | `…\script_sqr_nut_qq506807329\swordman\animation\FlashCut\` | ✅ | 飞剑判定占位 |
| 特效 .ani | flashcut/ 52 个 + .als ×11 | `…\pvf\character\swordman\effect\animation\flashcut\` | ✅ | 全视觉层 |
| 装备层 | *flashcut* ×228 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层（只查存在性） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。跨目录借图是常态（L14）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 三段角色动画 | 必需（共享） | ✅ |
| `Character/Swordman/Effect/FlashCut/LD_SwordA.img` | sprite_character_swordman_effect_flashcut.NPK | **飞剑/斩击主视觉**（atk_eff 4 层 + ready/end 各 4 层引用，102 处） | **必需** | ❌ |
| `…/FlashCut/LD_Start.img` | 同上 | 预备段光效（44 处） | **必需**（ready 演出主体） | ❌ |
| `…/FlashCut/LD_FinishEff_A/B/C.img` | 同上 | 收势/剑气终结光效（25/15/30 处） | **必需** | ❌ |
| `…/FlashCut/LD_SwordB.img`、`LD_Circle.img` | 同上 | 剑光变体/圆环 | 可选 | ❌ |
| `Character/Swordman/Effect/MeteorSword/meteorsword_exp_particle.img` | sprite_character_swordman_effect_meteorsword.NPK | **剑气命中粒子视觉**（fcfinish×5 + smoke 引用 56 处——同系列 235 借用，NPK 共享） | **必需**（剑气命中视觉主体） | ❌ |
| `Character/Fighter/Effect/ATFinalExtremeStrike/BackGround.img` | sprite_character_fighter_effect_atfinalextremestrike.NPK | 黑屏期背景条（flashcutbg_02，**跨职业借图**） | 可选（黑屏演出砍掉后可不做） | ❌ |
| `Character/Thief/Effect/SwordDance/sworddance_hitend_{front,back}dust.img` | sprite_character_thief_effect_sworddance.NPK ×2 | 命中尘土（**跨职业借图**） | 可选 | ❌ |
| `Character/Swordman/Effect/ATGigaBlade/giga_smoke.img` | sprite_character_swordman_effect_atgigablade.NPK | 烟雾 | 可选 | ❌ |

缺失 img：**必需 6 张**（本族 NPK 5 张 + meteorsword 1 张）、可选 7 张（跨职业 3 NPK + 本族 4 张）。
img 版本红线（v2/v4 可 / v5 不可）由提取时把关。

## 5. 实现方案草案

**结构映射**：飞剑条带 = 前斩时创建的宽判定 Area（静态盖住 0→370px 前斩路径）；
标记+定身 = 飞剑命中硬直 400ms 钉位（234 §5 同款近似）；剑气连打 = Tick 100ms 无去重 Area ×6 + 终结单发 Area（atk55/atk56 反应不同，按 064 多相位定案拆两 Area）。

### 内容件清单

1. **`DotNet~/Skills/FlashCutSkill.cs : SkillLogic`**（BloodBoom SubState + ReleaseWave 纯函数位移范式）
   - `CooldownMs = 50000`（DNF 原值直用）；`TotalTimeMs = 2040`（Ready 520 + Atk 360 + End 1020 中段 + 剑气 7 拍收尾）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanFlashCutReady)` + `ctx.ClearHitTargets()`；记录起点（SubState=0）。
   - `OnUpdate`（时间驱动，SubState 单值推进）：
     - t ≥ 520：`ctx.PlayAnim(AnimId.SwordmanFlashCutAtk)` + `ctx.CreateAreaInFront(AreaIds.FlashCutDash, (FP)23/10)`（条带中心 = 前斩路径中点 232px≈2.3 单位，静态盖 0→370px）；SubState=1。
     - 520 ≤ t < 680（前 2 帧 160ms）：`ctx.MoveCasterForward(370px 换算 3.7 单位 × dt/160)`（ReleaseWave §5.6-2 纯函数增量同构；无墙直用）。
     - t ≥ 880：`ctx.PlayAnim(AnimId.SwordmanFlashCutEnd)`；SubState=2。
     - t ≥ 1340（End F10）：`ctx.CreateArea(AreaIds.FlashCutQi, 前斩路径中点)`——剑气区对被钉住的敌人连打；SubState=3。
     - t ≥ 1940（6 拍完）：`ctx.CreateArea(AreaIds.FlashCutQiFinish, 同点)`——终结击；SubState=4。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/FlashCutDashArea.cs : AreaDefinition`**（BloodBoomArea 单次范式）
   - `TotalTimeMs = 360`（飞剑 PO 600ms 内判定窗收窄到前斩段）、`TickTimeMs = 0`、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (28/10, 105/100, 85/100)`（PO F0 盒 x[-46,510] 半宽 278px、y 半 105px、z 半 85px 折算）；
   - `HitReaction { Damage = 120, HitstunMs = 400, KnockbackX = 0, LaunchY = 0 }`（atk14 无 push/lift；HoldAndDelayDie 400 定身 → 长硬直钉位，234 §5 同款）；
   - `ViewAnimId = AnimId.FlashCutSwordA`（atk_eff_00 主层，飞剑光带）。
3. **`DotNet~/Areas/FlashCutQiArea.cs : AreaDefinition`**（FireCircle Tick + Tick 无去重范式，L19/R2-A8 档）
   - `TotalTimeMs = 600`、**`TickTimeMs = 100`**、`TickActions = { MeleeHit }`（6 拍独立结算）；
   - `HalfExtents = (22/10, 1, 85/100)`（盖前斩路径）；
   - `HitReaction { Damage = 40, HitstunMs = 250, KnockbackX = 0, LaunchY = 0 }`（atk55 reaction none → 小硬直保钉位）；
   - `ViewAnimId = AnimId.FlashCutFinish`（fcfinish 命中视觉循环层）。
4. **`DotNet~/Areas/FlashCutQiFinishArea.cs : AreaDefinition`**（终结）
   - `TotalTimeMs = 300`、`EnterActions = { MeleeHit }`；
   - `HitReaction { Damage = 200, HitstunMs = 800, KnockbackX = 400, LaunchY = 300 }`（atk56 原值：down/push400/lift300）；
   - `ViewAnimId = AnimId.FlashCutFinish`（复用，或换 FinishEff_C 加大层）。
5. **无新增 Buff/Action**（ap_flashcut 空壳不建；MeleeHit 复用）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 236 三子状态时间线 | SubState 时间编排（无输入分支，直译零损耗） |
| 飞剑 PO 24370 state 10（F0 宽盒 + 随行） | 静态宽 Area（盖前斩路径；DNF PO 出生在角色位、盒前伸 510px——路径条带等价） |
| onAttack 标记 + HoldAndDelayDie(400) 定身 | 飞剑命中 HitstunMs 400 钉位（234 §5"硬直钉位≈标记集合"同款） |
| setTimeEvent 100ms × N 对标记列表直发 | Tick 无去重 Area ×6（L19/R2-A8 档） |
| 最后一击 atk56（down/push400/lift300） | 第二个单发 Area |
| onProc 370px/160ms 前冲 + 撞墙止损 | MoveCasterForward 纯函数增量（撞墙延后，无墙直用） |
| 黑屏 99990 + 白闪 + 震 8~15 + 背景条钉屏幕 | 闪屏/屏震（延后档）→ 全跳过，不影响判定 |
| removeAllAp 解除定身 | 硬直自然到期（等价） |
| 攻速静态化（sq_SetStaticSpeedInfo） | 动画速度门面未暴露（延后）→ 固定速度 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.FlashCut = 23` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `FlashCutDash = 16`、`FlashCutQi = 17`、`FlashCutQiFinish = 18` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanFlashCutReady = 104`、`SwordmanFlashCutAtk = 105`、`SwordmanFlashCutEnd = 106`、`FlashCutSwordA = 107`、`FlashCutFinish = 108`（可选：smoke/bg 109+） |
| json 注册 | `LSAnimClipRegistrar.cs` | 角色 3 个 + .als overlay（ready/atk/end 三套）+ 特效 2~3 个 |
| 图集 | `LSAnimResComponentSystem.cs` | LD_SwordA/LD_Start/LD_FinishEff_A-C/meteorsword_exp_particle（必需 6 张） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 50000 ms | 50000（直用） |
| 总时长 | Ready 520 + Atk 360 + End 1020 = 1900（角色）；剑气再延 700 | 2040（兜住剑气收尾） |
| 前斩 | 370px / 160ms（Atk 前 2 帧） | 3.7 单位 / 160ms 匀速 |
| 飞剑判定 | PO F0 盒 x[-46,510] y[-70,140] z[-10,160]，600ms | Area 半 (2.8,1.05,0.85) 中心前 2.3 |
| 飞剑伤害 | col1 9716%（atk14：damage/cut/blood，无 push/lift） | Damage 120 / Hitstun 400（定身钉位） |
| 剑气每拍 | col2 1570%，100ms × 6，atk55 reaction none | Tick 100ms × 6，Damage 40 / Hitstun 250 |
| 剑气最后一击 | col4 20381%，atk56 down/push400/lift300 | Damage 200 / Hitstun 800 / Kb 400 / Ly 300 |
| 剑气命中视觉 | fcfinish01-05 随机 @敌位置 | Area ViewAnimId 单层循环（随机 5 选 1 简化为 1） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_flashcut.skl | `.skl` 无子命令 | 手抄 5 列（量小可接受）；skl 子命令立项既有 |
| 3 份 PO .atk + 2 份角色 .atk | `.atk` 无子命令 | 手抄（每份 ~6 值）；`[attack enemy]`/`[weapon damage apply]`/`[hit wav]` 字段并入 atk 立项输入 |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | 本档已给 #28/#14/#55/#56 对位表，手工映射 |
| `flashcutbg_02.ani` | **`[OPERATION] 1`**（新节名，全 pvf 首见——引擎背景条操作模式标记，语义未考证） | 建议整节跳过 + README 未识别节清单补记；黑屏演出本就延后，非阻塞 |
| 各 .ani 的 [SHADOW]/[GRAPHIC EFFECT] | 已知族（L15 GRAPHIC EFFECT 已支持） | 无新缺口 |
| .als 的 [none effect add] | 已支持（L12） | 无缺口 |

结论：缺口 = `.skl`/`.atk`/`.obj` 族共性 3 条 + **新节 `[OPERATION]`** 1 条，计 4 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 标记列表逐敌直发（无视位置 100ms 定点连打） | 无标记集合实体（234 同款：位置语义 Area） | 硬直钉位 + 同位置 Tick Area（敌人不动零差异；多段 atk 无 push 不推出区，比 234 更贴合） |
| HoldAndDelayDie 定身 400/300 + 空壳 ap | 无 hold 微控（缺失档，021 §8 已记） | HitstunMs 400 近似 |
| 黑屏→白闪演出 + 背景条钉屏幕缘 | 闪屏/屏震（延后）+ 屏幕锚定视觉无通道 | 全跳过（判定零损失，演出感降级明显——拔刀斩的"世界静止"感丢失） |
| 前斩撞墙止损（isMovablePos） | 无地图碰撞（延后） | 无墙环境直用 |
| fcfinish 5 选 1 随机命中视觉 | 位置随机缺口（R2-A10）姊妹项 + 无必要 | 单层循环视觉替代 |
| 攻速静态化 | 动画速度门面（延后） | 固定速度 |
| 音效（R_PUNTO_HIT/R_FLASHCUT_HIT） | 无音频（延后） | 跳过 |
| 混淆源码 | mod 污染（C3 族） | 本档还原语义为实现依据 |

## 8. 存疑与缺口上报

- **未考证**：①`[OPERATION]` 节语义（仅 flashcutbg_02.ani 一处）；②角色侧 .atk #93/94 的消费者（grep 无命中，疑引擎残留——与 PO 侧同名同内容双份）；③黑屏 start=Ready 前 6 帧累计 400ms 的读法（getDelaySum(0,5) 上界含否，±60ms 量级不影响重建）；④level info 70 行 vs 上限 40（超出段数据未用）。
- **系列修正**：极·神剑术系列并非全有蓄力段——236 是**固定时间线**技能（无按住检测），234/235 的"蓄力瞬发"共性缺口不适用本技能；后续系列技能（如有）先看 onSetState 有无 charge 子状态再套共性。
- **F7 结构第三次复验**：mod obj 的 [etc motion]/[etc attack info] 0 基直读在本技能再次吻合（#28/#14/#55/#56），与 234（#19/#9-11）、235（#20-27/#12-13）三方互证——F7 族该表可放心直查。
- **给下轮的经验**：剑神 24370 族"终结伤害拆两个自定义 atk 槽（连打 atk + 终结 atk）"是常见形态（234 的 9/10、236 的 55/56）——重建时终结拍必须单独 Area（反应参数不同）。
