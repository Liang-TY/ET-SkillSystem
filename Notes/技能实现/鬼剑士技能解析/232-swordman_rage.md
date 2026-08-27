# 暴怒狂斩（swordman_rage）

> 技能ID 232 | 级别 B（预分类；实为 A 类多段连招攻击技，见 §8 纠偏） | 可实现性 🔶（六段连招全链可表达；连打加速/跳跃取消/血之狂暴门槛需简化） | 分析日期 2026-08-22 | 批次 B4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 暴怒狂斩 | `skill\swordman\swordman_rage.skl [name]` |
| 英文名 | swordman_rage（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 狂战士（[skill fitness growtype] 3；`swordman_` 前缀二觉系——F7） | 同上 |
| 学习等级 | 35（**前置：技能 76 Lv5 = Frenzy.skl 血之狂暴**，lst 实测） | 同上 [required level] / [pre required skill] |
| 最高等级 | 60 | 同上 [maximum level] |
| 类型 | active（skill class 2） | 同上 [type]/[skill class] |
| 指令 | ←↑→ + Z | 同上 [command] / [command key explain] |
| CD | 20000 ms | 同上 [dungeon][cool time] |
| MP | 113 → 1117 | 同上 [dungeon][consume MP] |
| 读条 | 无（skl 无 casting time 节） | 同上 |
| 消耗品 | 无色小晶块 ×1 | 同上 [consume item] |
| 施放状态门禁 | [executable states] **8 0 14** + explain"只能在[血之狂暴]状态下使用"（血之狂暴=buff 在场校验，引擎/门禁壳侧） | 同上 [executable states] + explain |
| static data | `4 30`——[0]=**乱砍次数 4**、[1]=**每次按键增速 30%**（模板 (0,0)/(1,1) 实证） | 同上 [static data] + [level property] 向量 |
| 一句话效果 | 仅血之狂暴状态可用、可按跳跃键中断：武器下劈使敌浮空 → 二刀流乱砍（每次挥砍空中留剑痕）→ 最后与剑痕一起撕裂敌人 | 同上 [explain] |

**level property（3 列，Lv1 → Lv60）**：冲击波攻击力 col0=`2670→…`；乱砍攻击力 col1=`1108→…`；最后一击攻击力 col2=`4450→…`。（模板 5 项：col0/col1/col2 走 level，次数与增速走 static——L21 法全明，零未考证���。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
30: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/rage/rage.nut", "swordman_rage", 232, 232);
 6: IRDSQRCharacter.pushPassiveObj("common_object/share_obj/share_po_swordman_24370.nut", 24370);   // 共享打击 PO（L20）
```

标准 F7 结构：**完整 nut 管演出 + 共享 PO 24370 承担乱砍/冲击波判定**。nut 相对路径 `sqr\character\swordman\rage\rage.nut`（233 行，mod 变量混淆但逻辑全清）。

### 2.2 主 nut 逐回调（rage.nut 全量走读）

**`checkExecutableSkill_swordman_rage`**：`sq_IsUseSkill(232)` → 推状态 232 子状态 [0]。
**`checkCommandEnable`**：站立恒可；攻击态按 `sq_IsCommandEnable(232)`。

**`onSetState_swordman_rage`（按子状态 0-5 分支）**：
- **sub 0（下劈）**：`sq_StopMove`；读 static[1]=30、static[0]=4；var 向量 [0]=0（已按计数）、[1]=**5**（加速按键上限）、[2]=100、[3]=100+30×5=**250**（速度区间）、[4]=**4**（剩余乱砍次数）；播动画 **129**（RageStartBody_body.ani，.chr 实测）；创建特效 `rage/start/ragestartcast_00.ani`。
- **sub 1-4（乱砍四式）**：播动画 **130-133**（RageLoop01-04Body_body.ani）；每式入场 `vector[4] -= 1`；写包 `(232, substate, col1 乱砍攻击力, 速度系数)` → `sq_SendCreatePassiveObjectPacket(24370,0,0,0,0)`；同帧挂两组 draw-only：`rage/loop/0N/rageloop0Nshadow_00.ani`（残影）+ `rageloop0Ntraceloop_0X.ani`（**剑痕**，`sq_moveWithParent` 跟随，≤8 个存 var("aniobj")）。
- **sub 5（终结撕裂）**：播动画 **134**（RageEndBody_body.ani）+ `sq_SetCurrentAttackInfo(89)`（**角色侧** .atk 槽 89 = Rage_atk3.atk，实测）+ `sq_SetAttackPowerWithPassive(232, state, -1, 2, 1.0)`（col2）。

**`onKeyFrameFlag_swordman_rage`**：
- sub 0 **flag 1**（F5，300ms）：写包 `(232, 0, col0 冲击波攻击力, 速度系数)` → 创建 PO 24370 + `sq_SetMyShake(3,100)` + 特效 `ragestartfront_00.ani`。
- sub 5 **flag 2**（F2，481ms）：`RemoveAllAni(obj)`（清全部剑痕/残影）+ `sq_SetMyShake(3,100)`。

**`onProcCon_swordman_rage`（逐帧输入）**：
- 跳跃键使能 → 按下 → **`AddSetStatePacket(STATE_STAND)`（跳跃中断，explain 明示）**；
- 技能键/攻击键使能 → 按下一次且计数<5：计数+1，速度 = uniform(100→250, 计数, 5) → `sq_SendChangeSkillEffectPacket` → `onChangeSkillEffect` 里 `sq_SetStaticSpeedInfo(…, DEFAULT×速度/100, …)`（**全局播放提速 100%→250%**）。

**`onEndCurrentAni_swordman_rage`（状态机推进）**：
```
sub 0           → sub 1
sub 5           → STATE_STAND（收招）
sub 1-3（剩余次数>0）→ sub+1；sub 4（剩余>0）→ 回 sub 1（循环乱砍）
剩余次数 = 0     → sub 5（终结）
```
即 0→1→2→3→4→5 单圈（static[0]=4 恰好每式一次；若次数更大则 1-4 循环多圈）。

### 2.3 被动对象：共享 PO 24370 case 232（`sqr\common_object\share_obj\swordman\setcustomdata.nut:117` 实测）

```
读包首 dword subType（施法者的子状态 0-4）：
  subType > 0（乱砍式 1-4）：绘制层改 BOTTOM；attack = 自定义表 7（RageLoopLast.atk）；
                             动画 = 自定义表 5+subType（RageLoop01/02/03/04Slash_00/03/00/03.ani）
  subType = 0（下劈冲击波）：attack = 自定义表 6（Rage_atk1.atk）；动画 = 表 5（RageStartBottom_01.ani）
  随后：sq_SetCurrentAttackPower(包内 col0/col1)；动画 setSpeedRate(包内速度系数 100-250%)
```

24370 自定义表（`passiveobject\script_sqr_nut_qq506807329\swordman\qq506807329new_swordman_24370.obj` 实测，0 基——L20/R4-A17 已四验）：
- [etc motion] 5-9 = Rage/RageStartBottom_01.ani、RageLoop01Slash_00.ani、RageLoop02Slash_03.ani、RageLoop03Slash_00.ani、RageLoop04Slash_03.ani；
- [etc attack info] 6 = Rage_atk1.atk（下劈浮空）、7 = RageLoopLast.atk（乱砍）。
（atk 文件实体在 `…\script_sqr_nut_qq506807329\swordman\AttackInfo\`：rage_atk1.atk / ragelooplast.atk——obj 引用名 `RageLoopLast.atk` 与磁盘名 `ragelooplast.atk` 大小写失配，引擎大小写不敏感，已知翻译缺口"obj 引用失配"再证。）

**sub 5 终结不走 PO**：角色侧 atk 89（Rage_atk3.atk）+ 动画 F2-F5 攻击盒（实测：x∈[-80,401] y∈[-45,90] z∈[-4,269]——4.8×2.7 单位大范围前向盒）。剑痕撕裂视觉 = RageEndBody .als 在 F2 帧（层 -17..-2）叠 15 层 TraceEnd/Slash 特效（实测边车）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `RageStartBody_body.ani`（.chr etc #129） | 11 | 691ms（F0-8=60，F9=150，F10=1） | **F5=1**（300ms 下劈冲击波） | 无（判定在 PO） | .als 无；引 sm_body |
| `RageLoop01-04Body_body.ani`（#130-133） | 5×4 | 260ms/式（52×5） | 无 | 无（判定在 PO） | 各带 .als：`[none effect add]` 层 -1/10001 叠 `_00/_02` 换装层（Frenzy 剑血图层） |
| `RageEndBody_body.ani`（#134） | 7 | 1105ms（150/310/21/43/64/129/388） | **F2=2**（481ms 清痕+震屏） | **F2-F5 ×1**（-80,-45,-4,401,90,269） | .als：F0 叠 _00/_02；**F2 叠 15 层**（EndBack/Front×3/Slash×4 + 4 式 TraceEnd×8，层 -17..-2） |
| `ragestartbody/rageendbody_{00,02}.ani`（换装叠层） | — | — | — | — | Frenzy/sword_blood_upper/under.img（血剑层） |
| PO `rage/ragestartbottom_01.ani`（冲击波） | — | — | 无 | 无（atk 判定） | 24370 表 5 |
| PO `rage/rageloop0Nslash_XX.ani`（乱砍特效 ×16 + .als ×3） | — | — | 无 | 无（atk 判定） | 24370 表 6-9 |
| 特效 `rage/start|loop|end/*`（cast/front×3、shadow×4、trace loop/end×16、End 系×10） | — | — | 无 | 无 | Cast/Dust/Attack01-03/Trace.img |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_rage.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\swordman\swordman_rage.skl` | ✅ | 数据（3 列 + static 2 值全明） |
| 注册行 | load_state:30/6 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 232 + PO 24370 |
| 主 nut | rage.nut | `…\pvf\sqr\character\swordman\rage\rage.nut` | ✅（233 行全走读） | §2.2 全部演出/状态机 |
| PO 壳 | share_po_swordman_24370.nut + setcustomdata.nut case 232 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅ | §2.3 |
| PO 定义 | qq506807329new_swordman_24370.obj（表 5-9 / 6-7） | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | 乱砍/冲击波动画与 atk 映射 |
| PO .atk | rage_atk1.atk / ragelooplast.atk | `…\script_sqr_nut_qq506807329\swordman\AttackInfo\` | ✅ | 下劈浮空 / 乱砍 |
| .chr 条目 | etc #129-134（六式动画）+ etc attack #89 | `…\pvf\character\swordman\swordman.chr` 1102-1107/1383 行 | ✅ 实测 | §2.4 |
| 角色 .ani | RageStart/Loop01-04/EndBody_body.ani + _00/_02 变体 ×6 | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | Rage_atk3.atk | `…\pvf\character\swordman\attackinfo\Rage_atk3.atk` | ✅ | 终结撕裂 |
| .als | 5 个（Loop×4 + End） | 同 animation 目录 | ✅ 实测 | 血剑层 + 剑痕撕裂 15 层 |
| 特效 .ani | effect\animation\rage\{start,loop,end}\ 40+ 文件（含 .als ×3） | `…\pvf\character\swordman\effect\animation\rage\` | ✅ | 残影/剑痕/终结 |
| PO 特效 .ani | passiveobject\animation\rage\ 23 文件（slash 系 + .als ×3） | `…\pvf\passiveobject\character\swordman\animation\rage\` | ✅ | 乱砍判定体视觉 |
| 装备层 | 未查 | `…\pvf\equipment\character\swordman\avatar\` | 未考证 | 换装由 _00/_02 叠层覆盖 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 六式角色动画帧 | 必需（共享） | ✅ 已在库 |
| Cast.img（`Character/Swordman/Effect/Rage/`） | sprite_character_swordman_effect_rage.NPK | 下劈起手特效 | 可选 | ❌ |
| Dust.img / Attack01.img / Attack02.img / Attack03.img | 同上 | 下劈尘土/终结撕裂系 | **必需**（Attack02/03 终结视觉）/可选（Dust/01） | ❌ |
| Trace.img | 同上 | **剑痕全系**（loop/end ×16 + als 叠层）——本技能标志性视觉 | **必需** | ❌ |
| sword_blood_upper.img / sword_blood_under.img（`Effect/Frenzy/`） | sprite_character_swordman_effect_frenzy.NPK | _00/_02 血剑换装叠层（als 驱动） | 可选（血之狂暴氛围） | ❌ |

缺失 img：必需 3（Attack02/Attack03/Trace）、可选 5；分属 2 个 NPK（Rage 主 NPK 一次覆盖 6 张）。

## 5. 实现方案草案

1. **`DotNet~/Skills/SwordmanRageSkill.cs : SkillLogic`**（GoreCrossSkill 多段编排范式 + gorecross 三 Area 结构同类）
   - `CooldownMs = 20000`；`TotalTimeMs = 3400`（691 + 4×260 + 1105 + 余量；乱砍提速版会缩短——demo 固定速）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanRageStart)`（#129）；SubState=0。
   - `OnUpdate` 帧驱动编排（帧号 const + SubState 一次性守卫，bloodboom §4.7-7 同构）：
     - **F5（300ms）**：`ctx.CreateArea(AreaIds.RageSlam, 0)`（下劈冲击波区，自身中心略前）；SubState=1。
     - **动画切替**（用 GetElapsedMs 时间驱动，动画帧号驱动亦可）：951ms 起每 260ms 依次 `PlayAnim(Loop01-04)`（#130-133），每式 `ctx.CreateAreaInFront(AreaIds.RageSlash, 1.0)`（乱砍区，**每次新建**——L19 段间多段已通：每式独立 Area 天然重新可命中，无需 ClearHitTargets）；末式后 SubState=5。
     - **终结**（≈2031ms）：`PlayAnim(AnimId.SwordmanRageEnd)`（#134）；F2 等时点（+481ms）`ctx.SetAttackHitbox(前偏 1.6, 半尺寸 (2.4,0.7,1.4))`（F2 盒 x[-80,401] 折算）+ `HitActions={MeleeHit}` + 终结 HitReaction；+500ms `ctx.DisableAttackHitbox()`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/RageSlamArea.cs : AreaDefinition`**（下劈冲击波 = PO sub0）
   - `TotalTimeMs=400`、`EnterActions={MeleeHit}`、`HalfExtents=(2.0,0.5,1.5)`、`HitReaction{Damage=90, HitstunMs=400, KnockbackX=0, LaunchY=80}`（rage_atk1.atk 原值：**lift 80 浮空**/down/bounce）、`ViewAnimId=AnimId.RageStartBottom`（PO 表 5）。
3. **`DotNet~/Areas/RageSlashArea.cs : AreaDefinition`**（乱砍式 = PO sub1-4，四式共用配置、四次独立创建）
   - `TotalTimeMs=260`、`EnterActions={MeleeHit}`、`HalfExtents=(1.8,0.6,1.4)`、`HitReaction{Damage=40, HitstunMs=250, KnockbackX=0, LaunchY=20}`（ragelooplast.atk 原值：damage/push0/lift20/cut+blood40）、`ViewAnimId=四式 PO 特效各自 AnimId`（每式给不同 slash 视觉，四连观感）。
4. **终结**：技能本体 HitReaction = `{Damage=150, HitstunMs=800, KnockbackX=150, LaunchY=60}`（Rage_atk3.atk 原值：down/push150/lift60/blood80/ignore weight）。剑痕视觉：Area overlay 或技能侧手组装（releasewave 先例）；**最小版直接用 RageEndBody .als 的 15 层叠效**（als→overlay 管线现成，F2 起自动叠）。
5. 需要新增的 Action/Buff/Bullet：无。

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SwordmanRage = 32` + 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.RageSlam = 37`、`RageSlash = 38` |
| AnimId | npkparser `AnimConfigRegistry.cs` | `SwordmanRageStart=159`、`…Loop01-04=160-163`、`…End=164`、`RageStartBottom=165`、`RageSlash01-04=166-169`（PO 特效；159 起避开本批 075 的 153-158） |
| json/图集/按键 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem | 11 个 json + Rage/Frenzy 图集 |
| .als | 翻译后经 overlay 注册（别名→AnimId 由注册侧解析，§1.2 as-built） | Loop×4 + End 边车 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms | 20000 直用 |
| 下劈冲击波 | col0=2670%；rage_atk1：down/**lift80**/bounce/blood20 | Damage 90 / Ly 80（浮空起跳）/硬直 400 |
| 乱砍×4 | col1=1108%/式；ragelooplast：damage/lift20/cut+blood40 | 4 式 × Damage 40 / Ly 20 |
| 提速 | 每按+30%（上限 5 次=250%），全程动画/PO 同步加速 | 固定 100%（§7） |
| 终结 | col2=4450%；Rage_atk3：down/push150/lift60/blood80/ignore weight | Damage 150 / Kb 150 / Ly 60 / 硬直 800 |
| 终结判定盒 | F2-F5：x[-80,401] z[-4,269]（≈4.8×2.7 单位） | SetAttackHitbox 前偏 1.6 / 半尺寸 (2.4,0.7,1.4) |
| 乱砍间隔 | Loop 动画 260ms/式 | 260ms |
| 施法门槛 | 血之狂暴（Frenzy buff 在场）+ 状态 8/0/14 | 跳过（demo 直放） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `swordman_rage.skl` | `.skl` 无子命令 | 手抄 3 列 + static 2 值（全明） |
| `.atk` ×3（rage_atk1/ragelooplast/Rage_atk3） | `.atk` 无子命令 | 手抄（各 ~10 值） |
| 24370 `.obj` | `.obj` 无子命令 | 不需直译（case 232 槽位已手抄） |
| obj atk 引用名失配（`RageLoopLast.atk` vs 磁盘 `ragelooplast.atk`） | 引用大小写失配（已知翻译缺口再证） | 提取时按磁盘名对齐 |
| 5 个角色 `.als` + 3 个 PO `.als` + 3 个特效 `.als` | `[none effect add]`/`[use animation]`/`[add]` 均已支持（L12/R2-A9） | 现有 als 子命令覆盖——**本技能是 .als 层叠的重度用户**（End 边车 15 层），翻译后需验证层号 -17..-2 的 sortingOrder 表现 |
| 各 .ani | 常规节（GRAPHIC EFFECT 已支持 L15） | 现有 ani 子命令覆盖 |

结论：资源全部可被现有 ani/als 子命令翻译；实质缺口 `.skl`/`.atk`/`.obj`（既有）+ obj 引用失配（既有记档再证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 连按技能/攻击键提速 100%→250%（角色动画+PO 同步） | **缺失档：技能二段交互门面**（R4-A16）+ 无 ctx 动画变速门面（LSAnimComponent.Speed 存在但 SkillContext 无暴露） | 固定 100%；要还原时给 SkillContext 加 `SetAnimSpeed(FP)` 小门面（顺手项，上报） |
| 跳跃键中断技能 | **缺失档：跳跃系统**（R1-A2）+ 技能自结束门面 ctx.EndCast()（R1-A3 记档） | 不做中断（技能时长短，可接受） |
| 血之狂暴状态门槛 | **缺失档：自身 Buff 查询门面**（R4-A18） | demo 直放 |
| 剑痕随角色移动（sq_moveWithParent 跟随）且 ≤8 个累积 | Area overlay 无"跟随施法者"（R4-A17 缺口——但本技能施放者基本定点，影响小） | 剑痕留在挥砍点（更贴近"空中留痕"设定）；累积上限自然由 Area 寿命控制 |
| 乱砍四式 PO 判定与角色动画同速 | 已可表达（每式独立 Area，参数一致） | 直译 |
| bounce（弹地）/ignore weight（无视体重浮空） | .atk 的 HitReaction 外字段（atk 子命令立项字段清单既有建议） | 以 lift 80 + 长硬直近似 |
| mod 变量混淆（rage.nut 全文乱名） | C3 已知形态；逻辑可读（本文全量走读） | 无碍 |

## 8. 存疑与缺口上报

**未考证项**
1. 终结 F2-F5 的 4 个攻击盒是否有参数差异（实测同一坐标组，推断同盒持续 4 帧）。
2. 乱砍循环多圈的入口（static[0]>4 时 1-4 循环——本 pvf 恒 4，无从实证多圈表现）。
3. `sq_SendChangeSkillEffectPacket` 的提速是否影响 PO 攻击间隔（setSpeedRate 实证动画提速；判定节奏未证）。
4. 装备层（avatar）rage 动画变体是否存在（未查；_00/_02 叠层已覆盖主要视觉）。

**纠偏**：预分类 B → **实为 A**（多段连招攻击技；F7"主 nut 厚壳多子状态机（231 可直译）"同档——本技能 233 行全脚本，是 F7 厚壳档又一完整样本，且是**首个可全量直译的 F7 连招技**）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **SkillContext 动画变速门面**（`SetAnimSpeed`）：LSAnimComponent.Speed 字段已存在（PlayAnimUtil 置 1），只差 SkillContext 暴露——提速类技能（本技能 100→250%、R4-A16 的 230 加速）共用，属小改，建议优先排期。
2. 连打加速与"再按触发"合并为"技能内多次消费输入"统一设计（R4-A16 已记，本技能为"计数型"第三形态：239/242/230 是分支型、62 是加速型、232 是计数加速型）。

**翻译工具缺口**：obj 引用大小写失配再证（既有）；`.als` 15 层深叠（-17..-2 层号）为现有管线最深样本，建议实现期专项验证 sortingOrder 表现。
