# 幽魂之布雷德（swordman_bladephantom）

> 技能ID 239 | 级别 A | 可实现性 🔶（主体可直译；"再按即最后一击"与僵直倍率需简化） | 分析日期 2026-08-22 | 批次 A16

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幽魂之布雷德 | `skill\Swordman\swordman_bladephantom.skl [name]` |
| 英文名 | swordman_bladephantom（取 skl 文件名） | 同上 |
| 职业 | 鬼泣（黑暗君主）二觉 75 级技 | [second growtype maximum level] 索引 5=30（与 235 索引 2/3=剑魂线交叉印证的系列枚举推断）+ atk 全带 `dark element` + 幽魂/暗属性召唤设定 |
| 学习等级 | 75 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30，索引 5） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | active（skill class 3） | 同上 [type] |
| 指令 | →←→ + Z（指令施法 MP 优惠 50%/50% 档） | 同上 [command] / [skill command advantage] |
| CD | 40000 ms | 同上 [cool time] |
| MP | 580 → 4500 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块（道具 3037）× 3 | 同上 [consume item] |
| 可施放状态 | 8/0/14/32/20/42/65/13/33/50/237/238/240/241（14 态，攻击/前冲/其他二觉技中可取消接续） | 同上 [executable states] + nut checkExecutableSkill |
| 一句话效果 | 施法后在身前召唤幽魂布雷德的残影，残影每 80ms 对召唤阵范围内敌人造成暗属性魔法伤害（高僵直）；持续 6 秒（总时长 8s 的 3/4）后或再按技能键时，发动"范围斩击→最后斩击"两段收尾 | 同上 [explain] + nut/PO 走读 |

**level info（7 列，Lv1）与列语义（[level property] 全 `-1` 源直读 + nut 写包对位，L21/F7 规则）**：

| 列 | 占位符 | 系数 | Lv1 值 | 语义（写包顺序对位实证） |
|---|---|---|---|---|
| col0 | 幽魂攻击间隔 `<float2>`秒 | 0.001 | 80 → **0.08 秒** | setcustomdata 写包末位 → PO timer0 间隔 |
| col1 | 幽魂魔法攻击力 `<int>`%% | 1.0 | 249% | 写包第 1 dword → atk#18 bonus |
| col2 | 范围斩击攻击力 `<int>`%% | 1.0 | 2791% | 写包第 2 dword → subType3 var[0] → atk#19 |
| col3 | 最后的斩击攻击力 `<int>`%% | 1.0 | 15948% | 写包第 3 dword → subType3 var[1] → atk#20 |
| col4 | 持续时间 `<float1>`秒 | 0.001 | 8000 → **8.0 秒** | 写包第 4 dword → PO 总寿命（战斗期=×3/4=6s） |
| col5 | 召唤阵范围 `<int>`px | 1.0 | 350 | 写包第 5 dword → 幽魂命中 x 范围（×3/4=262px）与图像缩放基准 |
| col6 | 僵直率 `<int>`%% | 1.0 | 200 → **200%** | 写包第 6 dword → `sq_SetAttackInfoForceHitStunTime` |

Lv70 值：80 / 249→1544 / 2791→16958 / 15948→96850 / 8000 / 350 / 200→1580（col6 随等级升到 1580）。
另由 RangePx/350 与基准 Px=240、yPx=70 算出 **SizeRate = 350/240×100 ≈ 145%**、yPos = 70×1.45 ≈ 101（写包第 7/8 dword，缩放与 y 半径基准）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
59: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/bladephantom/bladephantom.nut", "swordman_bladephantom", 239, 239);
 8: IRDSQRCharacter.pushPassiveObj("common_object/share_obj/share_po_swordman_24370.nut", 24370);   // 共享 PO（L20/F7）
 9-13: sq_RunScript("common_object/share_obj/swordman/{setcustomdata,setstate,procappend,onendcurrentani,else}.nut")
```

- `swordman_header.nut`：`STATE_SWORDMAN_BLADEPHANTOM <- 239`（71 行）、`CUSTOM_ANI_SWORDMAN_GRAB <- 10`（180 行）、`PASSIVEOBJ_SUB_STATE_0..3 = 10..13`（603-606 行）。
- `.chr` 对位：etc motion **#10 = `Animation/Grab.ani`**（通用抓取/召唤姿势动画复用为施法动作，973 行起 0 基，实测 983 行）。
- 判定/演出全部在共享 PO **24370 case 239**（mod obj `qq506807329new_swordman_24370.obj`，etc motion/atk **0 基直读**）：
  - etc motion **#31** `BladePhantom/mist_start.ani`（出场）、**#32** `mist_loop.ani`（战斗循环）、**#33** `mist_end.ani`（收尾）、**#34** `slash1.ani`（范围斩击）、**#35** `finish_circle.ani`（最后斩击）
  - etc attack info **#18** `bladephantomghostattack.atk`（幽魂攻击）、**#19** `BladePhantomSlash.atk`（范围斩击）、**#20** `BladePhantomFinish.atk`（最后斩击）
- F7 结构完全成立：完整 nut 管演出 + 共享 PO 24370 承担远程判定；写包 dword 顺序与 level property 列一一对应（§1 表）。

### 2.2 主 nut 逐回调（bladephantom.nut，147 行；onKeyFrameFlag 段被 mod 混淆，已还原）

- **checkExecutableSkill**：
  1. 先查自己是否已有在场 PO（`onGetMyPassiveObject_My(obj, 24370, 239, 1)`）——**在场且处于 state 11（战斗循环）→ 发 state 12（最后一击）后 return false**（不进 CD 不重新施法）＝"再按技能键立即发动最后一击"。
  2. 无在场 PO：若当前处于 32/20/42/65/13/33/50/237/238/240/241 等 11 个取消态 → **不进状态机直接 addObject**（无施放动作召唤，取 `getGhostSoulRelease_Area_Distance`（鬼魂解放系公共函数，237/238 系）的距离位）；否则进状态 239 正常施法。
- **addObject_swordman_bladephantom**（召唤写包，10 个 dword）：技能 239 / subType 1 / col1 / col2 / col3 / col4 / col5 / col6 / SizeRate / yPos / col0 → `sq_SendCreatePassiveObjectPacket(24370, 0, 125, 0, 0)`——**出生在身前 125px**；播音效 SM_PHANTOMBLADE。
- **onSetState**：`sq_StopMove`；播 `CUSTOM_ANI_SWORDMAN_GRAB`（#10 Grab.ani，17 帧 640ms）；按动画 0-7 帧总延时画施法条；攻速静态信息（不随攻速变化）。
- **onKeyFrameFlag(obj, 100)**（Grab.ani **F8=320ms flag 100** 触发）：与 addObject 完全相同的写包+创建（mod 混淆段逐句还原与 addObject 一字不差）。即正常施法路径的召唤点。
- **onEndCurrentAni**：回 STATE_STAND。
- **onEndState**：离开状态时收施法条。

### 2.3 共享 PO 24370 case 239（判定与演出主体）

**subType 1（幽魂残影，状态机 10→11→12→13）**——`setstate.nut:156` 起：

| state | 动画/视觉 | 行为 |
|---|---|---|
| 10 出场 | etc#31 mist_start（9 帧 720ms）+ 池化 draw-only `magic_circle_start.ani` + `slash_before.ani`（全部按 SizeRate 缩放） | `setTimeEvent(1, 持续×3/4, 1次)`——6000ms 后自动进 12；动画播完(onendcurrentani)→11 |
| 11 战斗循环 | etc#32 mist_loop（12 帧 960ms）+ `magic_circle_loop.ani` 池化 | `sq_SetCurrentAttackInfo(atk#18)` + **`sq_SetAttackInfoForceHitStunTime(僵直率200)`**；`setTimeEvent(0, 80ms, 循环)`＝幽魂攻击拍；`setTimeEvent(2, slash_before总延时, 循环)`＝周期斩击视觉+音效 |
| 12 终结入口 | — | 移除全部计时器；**再创建一个 24370 PO（subType 3）**，写包：239/3/僵直率/col2/col3/SizeRate |
| 13 收尾 | etc#33 mist_end + `magic_circle_end.ani`；RemoveAllFlash（解除黑屏）+ 黑闪淡出 130 | 播完销毁 |

**timer0（80ms 幽魂攻击拍，else.nut:654 onUpdateTimeEvent）**——本技能最有价值的机制样本：
```
x 半径 = 召唤阵范围 350 × 3/4 = 262px；y 半径 = yPos(≈101) × 2/3 ≈ 46px；z 半径 300
① 视觉：在范围内随机点放一个池化 ghost_dash.ani（幽魂掠过残影，按来向定朝向）
② 判定：手动枚举对象管理器全部敌人，|dx|≤262 && |dy|≤46 && |dz|≤300 且可受击
   → sq_SendHitObjectPacket（用当前攻击信息 atk#18 = 249% 暗魔法 + 强制僵直 200%）
```
即**幽魂攻击不是攻击盒，是 80ms 一拍的"圆形区域手动枚举"**——与我们 Area 的 Tick 语义完全同构。

**subType 3（终结二连斩 PO，state 10→11）**：
- state 10：etc#34 slash1（12 帧 600ms，**F0/F3/F7/F8 四个 flag 1，攻击盒全帧**）+ atk#19（范围斩击 2791%+强制僵直）；onKeyFrameFlag flag1 → `resetHitObjectList()` + 震屏 8/50——**4 段多段斩**；播完→11。
- state 11：etc#35 finish_circle（28 帧 1680ms，**F16 flag 2** → 震 8/400 + 白闪；攻击盒 F19-21）+ atk#20（最后斩击 15948%，**down+push 200**）；z+60、动画提速 115%；播完 → 通知父 PO（subType1）进 state 13，自己销毁。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/Grab.ani`（施法，复用#10） | 17 | 640ms（均 40ms） | **F8=100（召唤）**、F15=65534 | F7-15（抓取技遗留，本技能不消费） | .als 无 |
| PO etc#31 mist_start.ani | 9 | 720ms | 无 | 无 | .als 挂 mist_dodge_start@F8/10001 |
| PO etc#32 mist_loop.ani | 12 | 960ms | 无 | 无 | .als 挂 mist_dodge_loop@F0/10001；判定走 timer0 手动枚举 |
| PO etc#33 mist_end.ani | — | — | 无 | 无 | 收尾（未逐帧） |
| PO etc#34 slash1.ani（范围斩击） | 12 | 600ms | **F0/F3/F7/F8=1**（×4 resetHit） | 全 12 帧：`-243 -50 -29 481 100 338`（偏移+尺寸→x[-243,238] z[-29,309]≈前向大范围） | .als 挂 slash3@-2/slash2@-1/slash_ghost4@F2 等 5 层 |
| PO etc#35 finish_circle.ani（最后斩击） | 28 | 1680ms | **F16=2**（震+白闪） | F19-21（同上盒） | .als 挂 finish_line1-6 + finish_circle1/2 + finish_ghost1-6 十余层 |
| 池化视觉 magic_circle_start/loop/end.ani | 9/—/— | 720ms | 无 | 无 | 魔法阵三层；slash_before.ani 借 `WaveSpinArea/mg-circle-front.img` |
| 池化视觉 ghost_dash.ani | 25 | 1500ms | 无 | 无 | 每 80ms 随机位置一枚（幽魂掠过） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_bladephantom.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_bladephantom.skl` | ✅ 实测 | 7 列等级数据（§1 全解码） |
| 注册行 | swordman_load_state.nut 行 59 / 8-13 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 239 + PO 24370 六回调 |
| 主 nut | bladephantom.nut | `…\pvf\sqr\character\swordman\bladephantom\bladephantom.nut` | ✅ 实测（147 行，onKeyFrameFlag 段混淆已还原） | 施法/取消态分流/召唤写包 |
| PO 回调 | share_obj/swordman/ 五个 nut 的 case 239 | `…\pvf\sqr\common_object\share_obj\swordman\{setcustomdata:262,setstate:156,onendcurrentani:51,else:218,654}.nut` | ✅ 实测 | 幽魂状态机/timer 攻击/终结二连 |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ 实测 | etc motion #31-35 / atk #18-20 对位 |
| PO atk ×3 | bladephantomghostattack.atk / BladePhantomSlash.atk / BladePhantomFinish.atk | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ 实测 | 幽魂攻击/范围斩击/最后斩击 |
| .chr 条目 | etc motion #10 | `…\pvf\character\swordman\swordman.chr` 983 行 | ✅ 实测 | `Animation/Grab.ani` |
| 角色 .ani | Grab.ani（复用） | `…\pvf\character\swordman\animation\Grab.ani` | ✅ 实测 | 17 帧 640ms，F8 flag 100 |
| 角色 .atk | —（无专属） | `…\pvf\character\swordman\attackinfo\` | —（判定全在 PO） | — |
| PO .ani | mist_start/loop/end、slash1-3、slash_before、finish_circle、magic_circle_*、ghost_dash 等 60+ | `…\script_sqr_nut_qq506807329\swordman\animation\bladephantom\` | ✅ 实测 | 全部演出视觉（含 10 个 .als） |
| PO .ani 镜像 | bladephantom/ 同名目录 | `…\pvf\passiveobject\character\swordman\animation\bladephantom\` | ✅ 实测 | 官方部署位副本（mist/magicsquar 系） |
| 特效 .ani | bladephantom/ 20 个 | `…\pvf\character\swordman\effect\animation\bladephantom\` | ✅ 实测 | finish/ghost_smoke 系视觉层 |
| 装备层 | 未查 | `…\pvf\equipment\...` | 未查 | sm_body 单图集（L16） |

## 4. 资源需求

img 推导 NPK：`sprite_<img所在路径下划线化>.NPK`——本技能视觉全部集中在 `Character/Swordman/Effect/BladePhantom/`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `…/BladePhantom/mist.img` | sprite_character_swordman_effect_bladephantom.NPK | 幽魂残影本体（start/loop/end 三态共用） | **必需** | ❌ |
| `…/BladePhantom/magiccircle.img` | 同上 | 召唤魔法阵三层 | **必需** | ❌ |
| `…/BladePhantom/1normal_finish.img` | 同上 | 范围斩击 slash1/3 | **必需** | ❌ |
| `…/BladePhantom/finish_circle.img` | 同上 | 最后斩击主视觉 | **必需** | ❌ |
| `…/BladePhantom/2normal_finish.img` | 同上 | 范围斩击 .als 层 slash2 | 可选 | ❌ |
| `…/BladePhantom/finish_line1-3.img` | 同上 | 最后斩击 .als 线光层 | 可选 | ❌ |
| `…/BladePhantom/ghost_smoke_start.img` | 同上 | 幽魂掠过 ghost_dash | 可选（判定核心视觉，建议升必需） | ❌ |
| `…/BladePhantom/finish_boom.img` | 同上 | 收尾爆闪 | 可选 | ❌ |
| `…/BladePhantom/{ghost_attack,ghost_sword,ghost_smoke_end,normal_ghost,smoke1,smoke2,lsat_finish,ghost_start_attackmove}.img` | 同上 | 其余 .als/特效层 | 可选 | ❌ |
| `Character/Swordman/Effect/WaveSpinArea/mg-circle-front.img` | sprite_character_swordman_effect_wavespinarea.NPK | slash_before（跨目录借图，L14 常态） | 可选 | ❌ |
| sm_body0000.img | （已入库） | 角色/Grab 施法动画 | 必需（共享） | ✅ |

缺失 img：必需 4 张（+建议 1 张）、可选 13 张——主视觉一个 NPK 全覆盖。v2/v4 由提取时把关。

## 5. 实现方案草案

**结构映射**：施法（640ms Grab）→ 幽魂持续区（Tick 80ms × 75 拍，6s）→ 范围斩击区（多段）→ 最后斩击区（down 击倒）。

### 内容件清单

1. **`DotNet~/Skills/BladePhantomSkill.cs : SkillLogic`**（SubState 时间编排）
   - `CooldownMs = 40000`；`TotalTimeMs = 700`（施法 640ms；幽魂区/终结区是独立 Area，不受技能时长约束——同 BloodBoom"技能短、区域长"形态）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanGrab)` + `ctx.ClearHitTargets()`；音效跳过。
   - `OnUpdate`：`ctx.CurrentFrameIndex() >= 8 && GetSubState()==0` → `ctx.CreateAreaInFront(AreaIds.BladePhantomGhost, 1.25)`（出生 125px≈1.25 单位）+ `SetSubState(1)`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - **再按即终结**：DNF 由 checkExecutableSkill 在技能 CD 外拦截（不耗 CD）。我们的 TryCast 会进 CD——简化为不做（§7）；若要支持，需"在场 Area 查询+输入直连"门面（新缺口，见 §8）。
2. **`DotNet~/Areas/BladePhantomGhostArea.cs : AreaDefinition`**（FireCircleArea Tick 范式）
   - `TotalTimeMs = 6000`、**`TickTimeMs = 80`**（75 拍）、`TickActions = { MeleeHit }`；
   - `HalfExtents = (2.6, 0.5, 1.0)`（DNF x±262px、y±46px、z±300——z 折算取薄值，我们无立体散布）；
   - `HitReaction { Damage = 40, HitstunMs = 1000, KnockbackX = 3, LaunchY = 0 }`（atk#18 原值 knuck back 3≈0；**僵直率 200% → HitstunMs 加倍表达**：DNF 基准僵直 ×2）；
   - `ViewAnimId = AnimId.BladePhantomMistLoop`（mist_loop 循环）。
   - **Tick 结束自动终结**：Area 无"到期回调创建新 Area"的能力——终结区由技能 OnUpdate 按总时间轴驱动（见下）；DNF 的 timer1=6000ms 语义用技能侧时间轴对齐（技能 640ms + Area6000ms → 终结区在 6640ms 落点，误差<一帧，可接受）。
3. **`DotNet~/Areas/BladePhantomSlashArea.cs : AreaDefinition`**（范围斩击）
   - `TotalTimeMs = 600`、`TickTimeMs = 150`（DNF 4 段 flag 均布 → 4 拍 Tick）、`TickActions = { MeleeHit }`；
   - `HalfExtents = (2.4, 0.5, 1.5)`（slash1 攻击盒 x[-243,238] 折算半宽）；
   - `HitReaction { Damage = 90, HitstunMs = 1200, KnockbackX = 3, LaunchY = 0 }`（atk#19：damage 反应+强制僵直）；
   - `ViewAnimId = AnimId.BladePhantomSlash1`。
   - ⚠ 但技能 TotalTimeMs 只有 700ms，6640ms 的终结触发**超出技能生命周期**——两案：(a) 技能 `TotalTimeMs = 7000` 但 640ms 后即 `PlayDefaultAnim` 解控（技能继续空转到 7000ms 驱动终结区，等于"托管计时器"）；(b) 新增"Area 到期动作"（ExitActions ≠ 此语义，见 064 §5 同辨）。**demo 取 (a)**，零新机制。
4. **`DotNet~/Areas/BladePhantomFinishArea.cs : AreaDefinition`**（最后斩击）
   - `TotalTimeMs = 400`（F19-21 判定窗 3 帧 60ms + 余量）、`EnterActions = { MeleeHit }`、`HalfExtents = (2.4, 0.5, 1.5)`；
   - `HitReaction { Damage = 300, HitstunMs = 800, KnockbackX = 200, LaunchY = 100 }`（atk#20 原值 down+push200——击倒用长硬直+击退表达，releasewave as-built 同构）；
   - `ViewAnimId = AnimId.BladePhantomFinishCircle`（1680ms 长视觉，Area 判定窗先结束、视图播完自隐——LSAreaViewComponent 现行为一致）。
5. **无新增 Buff/Action**（MeleeHit 现成；暗属性/感电类无）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 239 + Grab.ani F8 flag100 | `BladePhantomSkill` OnUpdate 帧 const 8 + SubState |
| 幽魂 PO timer0 手动圆形枚举（80ms） | `BladePhantomGhostArea` TickTimeMs=80（Tick 无去重＝每拍全场命中，语义同构） |
| 僵直率 200%（ForceHitStunTime） | `HitReaction.HitstunMs` 直接加倍（无倍率通道，数值直译） |
| subType3 二连斩（atk#19→#20） | 两个 Area 顺序创建（L9 多相位惯例） |
| 再按技能键提前终结 | 不做（§7/§8） |
| 11 个取消态直接召唤 | 技能取消体系缺失 → 只做站立施法 |
| SizeRate 图像/判定盒缩放 | 对象整体缩放（延后）→ 固定 100% |
| 黑屏长闪（500/99990） | 闪屏延后 → 跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.BladePhantom = 23` |
| AreaId | `Runtime\AreaDefinition.cs` | `BladePhantomGhost = 16`、`BladePhantomSlash = 17`、`BladePhantomFinish = 18` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanGrab = 104`、`BladePhantomMistLoop = 105`、`BladePhantomSlash1 = 106`、`BladePhantomFinishCircle = 107`、`BladePhantomMagicCircle = 108`（可选 ghost_dash=109） |
| json 注册 | `LSAnimClipRegistrar.cs` | Grab 1 + PO 4~5 个（含翻译 .als 的 overlay） |
| 图集 | `LSAnimResComponentSystem.cs` | mist.img、magiccircle.img、1normal_finish.img、finish_circle.img（+可选 ghost_smoke_start.img） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000 ms | 40000（直用） |
| 施法 | Grab.ani 640ms，F8(320ms) 召唤 | 帧 8 触发 |
| 幽魂攻击 | 80ms/拍 × 75 拍（6000ms），249% 暗魔 | Tick 80ms，Damage 40/拍 |
| 幽魂范围 | x±262px y±46px z±300 | HalfExtents (2.6, 0.5, 1.0) |
| 僵直率 | 200%（强制僵直） | HitstunMs 1000（基准加倍） |
| 范围斩击 | 2791% × 4 段（flag 4 次 resetHit） | 90 × Tick150ms × 4 |
| 最后斩击 | 15948%，down，push 200 | Damage 300 / Hitstun 800 / Kb 200 |
| 召唤位置 | 身前 125px | CreateAreaInFront 1.25 单位 |
| 全程 | 640 + 6000 + 600 + 1680 ≈ 8.9s（收尾视觉重叠） | 技能托管 7000ms |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_bladephantom.skl | `.skl` 无子命令 | 本档已全解码（7 列×写包对位），无需子命令也行；批量收益归 skl 立项 |
| 3 份 .atk（#18/19/20） | `.atk` 无子命令 | 手抄 3×~8 值可接受；`[knuck back]` 字段并入 atk 立项输入 |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | 本档已给 #31-35/#18-20 对位表 |
| bladephantom 各 .ani | 常规节（FRAME/DELAY/IMAGE/ATTACK BOX/SET FLAG） | `ani` 子命令全覆盖 |
| 10 个 .als（slash1/finish_circle/ghost_dash/mist_* 等） | `[use animation]`/`[add]` 常规 | `als` 子命令全覆盖（slash1 挂 5 层、finish_circle 挂 10+ 层——overlay 数量多但结构常规） |
| finish_circle.ani 等 | `[GRAPHIC EFFECT]`（LINEARDODGE 系） | 已支持（L15，graphicEffect 字段）——非缺口 |

结论：`.skl`/`.atk`/`.obj` 族共性 3 条；ani/als 无新节缺口。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 再按技能键立即终结幽魂（不耗 CD） | **在场对象查询 + 技能外输入直连门面缺失**（新缺口，同族：技能取消体系） | 不做：固定 6s 后自动终结（时间轴等价，损失操作深度） |
| 11 个取消态无施法动作召唤 | 技能取消体系缺失（064 上报族） | 只做站立施法 |
| 僵直率 200%（倍率型强制僵直） | HitReaction 无倍率通道（不算系统缺口，数值可直译） | HitstunMs 直接给加倍值 |
| 范围斩击 4 段多段（resetHitObjectList） | 段间多段已通（L19）；本档用 Area Tick 表达 | Tick 150ms × 4（同段定时多档） |
| 幽魂 80ms 一拍全场枚举（无攻击盒） | Area Tick 无去重正好同构（L19/R2-A8） | 直译 |
| 暗属性伤害 | 元素属性系统缺失 | 忽略属性，只留伤害 |
| SizeRate 145% 图像/判定盒三重缩放 | 对象整体缩放（延后，已 3+ 例） | 固定 100%（视觉略小，可后调 img 帧本身） |
| 黑屏长闪 + 震屏 + 白闪 | 屏震/闪屏延后 | 跳过 |
| 幽魂随机掠过视觉（每拍随机点 ghost_dash） | 位置随机未暴露（R2-A10）+ 独立多实例视觉 | 幽魂区单循环动画替代（mist_loop 本体即主视觉，掠过层可省） |
| 音效 SM_PHANTOMBLADE / PHANTOMBLADE_GHOST | 音频延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①`getGhostSoulRelease_Area_Distance`（鬼魂解放 237/238 系公共函数）的返回距离值——只影响取消态召唤位置，demo 不做取消态；②mist_end/magic_circle_loop 等次要 ani 未逐帧（仅核心 6 个做了帧表）；③pvp level info 300+ 行只采样未全读；④Lv70 col6=1580 的僵直率读法（写包对位是确定的，等级成长曲线未逐级核对）。
- **新系统级缺口上报**：**"技能 CD 外重按触发在场实体阶段推进"门面**（DNF checkExecutableSkill 拦截模式：不耗 CD、驱动已存在 PO 的状态机）——本技能/炸弹类/蓄力引爆类都会撞；与技能取消体系（064）同族但不同点（前者是"同一技能的第二段交互"，后者是跨技能打断）。可作为"技能二段交互"缺口单独记档。
- **给下轮的经验**：24370 的 `else.nut:654`（onUpdateTimeEvent）是共享 PO 计时器逻辑总入口——凡 24370 case 技能有 setTimeEvent 的，攻击/阶段判定全在这读；`sq_SetAttackInfoForceHitStunTime` 是僵直倍率型技能的通用表达（僵直率列直传），HitReaction 数值化时直接乘基准。
