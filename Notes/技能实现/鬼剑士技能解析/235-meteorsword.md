# 极 · 神剑术 (流星落)（meteorsword）

> 技能ID 235 | 级别 A | 可实现性 🔶（升空/标记移动/随机落点/蓄力均需简化） | 分析日期 2026-08-22 | 批次 A15

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极 · 神剑术 (流星落) | `skill\Swordman\swordman_meteorsword.skl [name]` |
| 英文名 | meteorsword（skl 存剑 swordman_meteorsword） | 同上 |
| 职业 | 剑魂 · 剑神（二觉 85 系；与 234 破空斩同系列） | 同上 + 常识 |
| 学习等级 | 70 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30） | 同上 [maximum level] / [second growtype maximum level]（索引 2/3 = 30） |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ↓→→ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 35000 ms | 同上 [cool time] |
| MP | 420 → 1200 | 同上 [consume MP] |
| 特殊消耗 | 道具 3037 × 1 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14（攻击中可取消接技） | 同上 [executable states] |
| 一句话效果 | 蓄力后跃起（原地引发冲击波），空中用方向键移动落剑标记 1 秒，随后 38 把流星剑每 50ms 一把随机砸落，最后角色随剑落地震地重击 | 同上 [explain] + nut 走读 |

**static data**：无（dungeon 只有 level info）。
**level info（15 列，Lv1）**：col0 蓄气上限 500(=0.5s)、col1 最大蓄力增伤 25%、col2 跳跃冲击波 1036%、col3 标记移动速度 LV3、col4 标记时间上限 1000(=1s)、col5 流星剑攻击力 222%、col6 数量 38 把、col7 生成间隔 50(=0.05s)、col8/9 生成范围 x±100/y±30、col10 地面冲击波 2072%、col11-14（太刀）出血 31%/Lv82/2.0s/525、col14 出血攻击力（向量源 **-4**，同 234）。
**level property**（15 占位符）：`(-1,N,系数)` 直读 + 末列出血攻击力 `(-4,14,1.0)`——L21 规则对位一致。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
43: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/meteorsword/meteorsword.nut", "swordman_meteorsword", 235, 235);
 8: IRDSQRCharacter.pushPassiveObj("common_object/share_obj/share_po_swordman_24370.nut", 24370);   // 同 234：mod 版共享 PO
```

- `swordman_header.nut`：`STATE_SWORDMAN_METEORSWORD <- 235`（67 行）、`CUSTOM_ANI_SWORDMAN_METEORSWORD{START,CHARGE,CHARGEFINISH,UP,DROP,EXPLOSION} <- 144-149`（314-319 行）、`CUSTOM_ATTACK_SWORDMAN_METEORSWORDEXPLOSION <- 92`（483 行）。
- .chr 对位（0 基，实测全吻合）：etc motion #144-149 = 1117-1122 行（MeteorSwordStart/Charge/ChargeFinish/Up/Drop/Explosion.ani，行号-973 ✓）；etc attack info #92 = 1386 行 `AttackInfo/MeteorSwordExplosion.atk`（行号-1294 ✓）。
- 落剑 PO = 24370 case 235（`common_object/share_obj/swordman/*.nut`，同 234；动画在 mod obj [etc motion] **#20-26** dropsword1-7 随机、**#27** up_meteorsword_shock1；atk **#12** MeteorSword.atk / **#13** MeteorSwordSub.atk——0 基直读无错位）。
- 主 nut 459 行，mod 混淆（同 234），已人工还原。

### 2.2 主 nut 逐回调（meteorsword.nut，六子状态状态机）

**checkExecutableSkill**：使用 → (subState 0, weaponSubType) 进状态 235（weaponSubType==1 太刀 → 落剑带出血）。

| subState | 动画 | 行为 |
|---|---|---|
| 0 起跳预备 | #144 Start（3 帧 210ms） | **sq_SetSuperArmor(0) 霸体**；记录武器类型；播完→1 |
| 1 蓄力 | #145 Charge（10 帧 600ms） | 计时器上限 col0=0.5s；**松开技能键** → 3（带蓄力系数 0→25% 线性）；计时满 → 2 |
| 2 蓄力满收势 | #146 ChargeFinish（5 帧 300ms） | 播完→3（满蓄力） |
| 3 跃起+标记 | #147 Up（**1 帧 500ms 悬停帧**，flag 1 无脚本消费者） | **sq_ZStop**；z 0→1000 匀速 **150ms** 升空；起跳瞬间在原地创建跳跃冲击波 PO（24370 subType 1：atk13 MeteorSwordSub，col2 攻击力）；150ms 后在起跳点放 meteorsword_target.ani 地面标记；**方向键移动标记**（±col3=3px/帧，可移动位检查）最长 col4=1s → 4 |
| 4 落剑雨 | #148 Drop（**1 帧 500ms 悬停帧**） | 黑闪长亮(50,99990)；相机跟拍标记点（300ms 推移）；150ms 后装填 createInfo 并 **setTimeEvent(0, 50ms, 38 次)**——每拍在标记点 ±col8/±col9 随机位置创建落剑 PO（24370 subType 2：z=1000 出生、下坠至 0、atk12 MeteorSword.atk col5 攻击力、太刀时写出血 4 参、动画 dropsword1-7 随机）；38 把发完后相机拉回角色（300ms）→ 发 ChangeSkillEffect(1) → 角色瞬移到标记点，z 1000→0 落下 150ms → 5 |
| 5 落地重击 | #149 Explosion（11 帧 660ms，**F0-F5 六帧攻击盒**） | 攻击信息 **#92 MeteorSwordExplosion.atk**，倍率 col10 ×(1+蓄力)；解除黑闪/霸体；震 6/200 + 白闪；播完→站立 |

**掌握[极·神剑术]追加**（subState 4 落剑期）：挂 `ap_meteorsword_stateoflimit.nut`（技能 248 极·神剑术的数据），其间再创建 dword 248,6 的追加剑组（本批不展开，属技能 248 联动）。

### 2.3 落剑被动对象（PO 24370 case 235）

| subType | 动画（mod obj 直读） | 行为 | atk |
|---|---|---|---|
| 1 跳跃冲击波 | #27 `up_meteorsword_shock1.ani`（7 帧 420ms） | 起跳点原地爆 | **#13 MeteorSwordSub.atk**：physic/weapon、**down、lift 300**、blow、no blood 20——把周围敌人炸飞 |
| 2 流星剑 | #20-26 `dropsword1-7_meteorsword_swords.ani` **随机 7 选 1**（13 帧 1030ms；.als 挂 6 层：crack/shock/drop/drop_dust/circle/circle2） | 出生 z=1000，procappend 按动画 F0 时长匀速下坠到 0，落地判定，播完销毁 | **#12 MeteorSword.atk**：physic/weapon、**down**、push 20、lift 50、hit down、blow、no blood 50；太刀时附 BLEEDING 4 参 |

（`passiveobject\character\swordman\animation\meteorsword\` 另有一套同名 dropsword 镜像目录——官方原版部署位，mod obj 引用的是 script_sqr_nut 路径，两套同构。）

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| MeteorSwordStart.ani（#144） | 3 | 210ms | 无 | 无 | — |
| MeteorSwordCharge.ani（#145） | 10 | 600ms | 无 | 无 | .als 挂 charge 层 @帧0 层10001 |
| MeteorSwordChargeFinish.ani（#146） | 5 | 300ms | 无 | 无 | .als 挂 charge+fullcharge 两层 |
| MeteorSwordUp.ani（#147） | **1** | 500ms | flag 1（无消费者） | 无 | **单帧悬停**：z 升降全由 onProc 驱动，动画只是姿态 |
| MeteorSwordDrop.ani（#148） | **1** | 500ms | flag 1（同上） | 无 | 同上 |
| MeteorSwordExplosion.ani（#149） | 11 | 660ms | 无 | **F0-F5**（x -240~+240、z 0~344，大范围） | .als 挂 dust1/dust2/stone/particle8 等多层 |
| PO：dropsword1_meteorsword_swords.ani ×7 套 | 13 | 1030ms | 无 | 无（判定在 atk） | 每套带 6 层 .als |
| PO：up_meteorsword_shock1.ani | 7 | 420ms | 无 | 无 | — |
| effect/animation/meteorsword/ 27 个（charge/dash/target/upeffect/start_dust/exp_* 等） | — | — | — | — | 全 draw-only 视觉层；节名常规 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_meteorsword.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_meteorsword.skl` | ✅ 实测 | 15 列等级数据 |
| 注册行 | swordman_load_state.nut 行 43 / 8-13 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 235 + PO 24370 |
| 主 nut | meteorsword.nut | `…\pvf\sqr\character\swordman\meteorsword\meteorsword.nut` | ✅ 实测（459 行，混淆已还原） | 六子状态状态机 |
| 追加 ap | ap_meteorsword_stateoflimit.nut | `…\meteorsword\ap_meteorsword_stateoflimit.nut` | ✅ 实测（引用层面） | 极·神剑术追加剑组（技能 248 联动） |
| PO 回调 | common_object/share_obj/swordman/{setcustomdata,procappend,onendcurrentani}.nut 的 case 235 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅ 实测 | 冲击波/落剑两 subType |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ 实测 | etc motion #20-26/#27、atk #12/#13 对位 |
| PO atk | MeteorSword.atk / MeteorSwordSub.atk | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ 实测 | 落剑 / 跳跃冲击波 |
| .chr 条目 | etc motion #144-149（1117-1122 行）+ etc attack info #92（1386 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 六动画 + 落地 atk |
| 角色 .ani | meteorsword{start,charge,chargefinish,up,drop,explosion}.ani（磁盘名小写）+ .als ×5 | `…\pvf\character\swordman\animation\` | ✅ 实测 | 见 §2.4 |
| 角色 .atk | MeteorSwordExplosion.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 落地重击（down/push350/lift200） |
| PO 落剑 .ani | MeteorSword/dropsword1-7 七套（swords/drop/dust/shock/circle/circle2/crack）+ up_meteorsword_shock1 | `…\passiveobject\script_sqr_nut_qq506807329\swordman\animation\MeteorSword\` | ✅ 实测 | 落剑视觉七变体 |
| PO 落剑镜像 | meteorsword/ 同构一套 | `…\passiveobject\character\swordman\animation\meteorsword\` | ✅ 实测 | 官方部署位副本 |
| 特效 .ani | meteorsword/ 27 个 | `…\pvf\character\swordman\effect\animation\meteorsword\` | ✅ 实测 | 蓄力/标记/爆地全视觉 |
| 装备层 | 未查 | `…\pvf\equipment\...` | 未查 | sm_body 单图集（L16） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 六段角色动画 | 必需（共享） | ✅ |
| `…/MeteorSword/meteorsword_swords.img` | sprite_character_swordman_effect_meteorsword.NPK | **流星剑本体**（七变体共用） | **必需** | ❌ |
| `…/MeteorSword/meteorsword_up.img`、`meteorsword_start_dust.img` | 同上 | 跃起冲击/尘土 | 可选 | ❌ |
| `…/MeteorSword/meteorsword_circle.img`、`meteorsword_crack.img`、`meteorsword_drop_dust.img`、`meteorsword_drop.img` | 同上 | 落剑地面层（.als 六层） | 可选 | ❌ |
| `…/MeteorSword/meteorsword_charge.img`、`meteorsword_fullcharge.img`、`meteorsword_dash.img` | 同上 | 蓄力/冲刺 | 可选 | ❌ |
| `…/MeteorSword/meteorsword_exp_main.img`、`meteorsword_exp_dust.img`、`meteorsword_exp_floor.img`、`meteorsword_exp_line.img`、`meteorsword_stone.img`、`meteorsword_exp_particle.img` | 同上 | 落地重击全套 | **必需**（至少 exp_main） | ❌ |
| `character/swordman/effect/meteorsword/meteorsword_target.img`（小写路径变体） | sprite_character_swordman_effect_meteorsword.NPK（同库推断） | 目标地面标记 | 可选 | ❌ |
| `Character/Fighter/Effect/EarthBreak/floor.img`、`Character/Priest/Effect/BlueDragon/exp_nova.img`、`Character/Fighter/Effect/EnergyField/boom.img` | 跨职业三个 NPK | 跨目录借用（L14 常态） | 可选 | ❌ |

缺失 img：**必需 2 张（swords + exp_main）、可选 15+3 张**——主视觉集中在一个 NPK。

## 5. 实现方案草案

**结构映射**：跳跃冲击波 = 施放点小 Area；38 把落剑 = 目标点大 Area 的 **50ms Tick × 38**（L19/R2-A8 同段定时多档直接表达）；落地重击 = 大 HalfExtents 终结 Area。

### 内容件清单

1. **`DotNet~/Skills/MeteorSwordSkill.cs : SkillLogic`**（SubState 时间编排，长技）
   - `CooldownMs = 35000`；`TotalTimeMs = 3500`（Start 210 + 蓄力砍 0 + 标记窗 1000 + 落剑雨 38×50=1900 + 落地 660，紧缩后 ~3500）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanMeteorSwordStart)` + `ctx.ClearHitTargets()` + `ctx.CreateAreaInFront(AreaIds.MeteorSwordJumpShock, FP.Zero)`（起跳冲击波，DNF 在起跳瞬间于原地爆）。
   - `OnUpdate`（SubState 单值推进）：
     - t ≥ 210：`ctx.PlayAnim(AnimId.SwordmanMeteorSwordCharge)`（蓄力动画占位播一遍，600ms——按住蓄力不做）；SubState=1。
     - t ≥ 1210（标记窗结束）：在标记点 `ctx.CreateArea(AreaIds.MeteorSwordRain, 标记点)`；标记点 = 施放者前方 2 单位（**方向键移动标记简化为固定点**，§7）；`ctx.PlayAnim(AnimId.SwordmanMeteorSwordDrop)`；SubState=2。
     - t ≥ 1210+1900+150（剑雨 38 拍 + 相机余量）：角色位移到标记点（`MoveCasterForward` 前方 2 单位已在标记点上方语义——demo 角色未升空，落地简化为原地）；`ctx.PlayAnim(AnimId.SwordmanMeteorSwordExplosion)` + `ctx.CreateArea(AreaIds.MeteorSwordGround, 标记点)`；SubState=3。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/MeteorSwordJumpShockArea.cs`**（BloodBoomArea 范式）
   - `TotalTimeMs = 420`、`EnterActions = { MeleeHit }`、`HalfExtents = (1.2, 0.5, 1.0)`；
   - `HitReaction { Damage = 120, HitstunMs = 600, KnockbackX = 0, LaunchY = 300 }`（MeteorSwordSub.atk：down/lift300/blow——炸飞）；`ViewAnimId = AnimId.MeteorSwordShock`（up_meteorsword_shock1）。
3. **`DotNet~/Areas/MeteorSwordRainArea.cs`**（FireCircleArea Tick 范式 + Tick 无去重）
   - `TotalTimeMs = 1900`、**`TickTimeMs = 50`**（38 拍）、`TickActions = { MeleeHit }`；
   - `HalfExtents = (1.0, 0.3, 1.0)`（落点 x±100/y±30 折算——窄雨带）；
   - `HitReaction { Damage = 30, HitstunMs = 250, KnockbackX = 20, LaunchY = 50, ProcBuffId = BuffIds.Bleed, ProcChance = 31 }`（MeteorSword.atk 原值 + 太刀出血 31% 直接给——武器分支缺失的补偿性近似，demo 可关）；
   - `ViewAnimId = AnimId.MeteorSwordSwords`（dropsword swords 视觉循环）。
4. **`DotNet~/Areas/MeteorSwordGroundArea.cs`**（终结）
   - `TotalTimeMs = 360`（F0-F5 判定窗）、`EnterActions = { MeleeHit }`、`HalfExtents = (2.4, 0.5, 1.7)`（F3 盒 x[-240,240] 折算）；
   - `HitReaction { Damage = 250, HitstunMs = 800, KnockbackX = 350, LaunchY = 200 }`（MeteorSwordExplosion.atk 原值）；
   - `ViewAnimId = AnimId.MeteorSwordExpMain`、可选 `ViewEndAnimId`（exp_floor 渐隐）。
5. **无新增 Buff/Action**。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 六子状态状态机（起跳/蓄力/标记/落剑/落地） | SubState 时间编排 + 三个 Area |
| 跃起 z 0→1000→0（sq_ZStop + onProc） | **不做**（施法者 z 主动位移缺失——跳跃系统缺口，R1-A2）；角色原地，Up/Drop 悬停帧动画贴地播 |
| 方向键移动标记（3px/帧、1s 上限、可移动位检查） | 技能中方向输入缺失（R1-A3）→ 固定前方 2 单位 |
| 38 把随机落剑（±100/±30、50ms 间隔、独立 PO 下落） | 一个雨区 Area Tick 50ms × 38（判定等价；**视觉差异**：单循环动画 vs 38 把独立落剑——随机落点缺口见 §7） |
| 蓄力 0.5s / +25% | 按住输入缺失 → 瞬发，增伤 0 |
| 跳跃冲击波 / 落地重击（.atk #13 / #92） | 两个单次 Area（原值直译） |
| 相机跟拍（getScrollBasisPos） | 无跟随相机 → 跳过（角色不离开屏幕中央即无需求） |
| 霸体 + 黑闪 | 缺口/延后 → 跳过 |
| 追加剑组（极·神剑术 248 联动） | 被动缺失 → 不做 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.MeteorSword = 22` |
| AreaId | `Runtime\AreaDefinition.cs` | `MeteorSwordJumpShock = 12`、`MeteorSwordRain = 13`、`MeteorSwordGround = 14` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanMeteorSwordStart = 86`、`Charge = 87`、`Drop = 88`、`Explosion = 89`、`MeteorSwordShock = 90`、`MeteorSwordSwords = 91`、`MeteorSwordExpMain = 92`（可选 Up=93） |
| json 注册 | `LSAnimClipRegistrar.cs` | 角色 4~6 个 + PO 特效 3 个 |
| 图集 | `LSAnimResComponentSystem.cs` | meteorsword_swords.img、meteorsword_exp_main.img（必需） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 35000 ms | 35000（直用） |
| 蓄力 | 上限 0.5s，+0~25% | 不做（0） |
| 升空 | z 0→1000 / 150ms；落下 1000→0 / 150ms | 不升空 |
| 标记窗 | 1s，方向键 3px/帧 | 固定前方 2 单位 |
| 跳跃冲击波 | col2 1036%；MeteorSwordSub：down/lift300/blow | Damage 120 / Hitstun 600 / Ly 300 |
| 流星剑 | 38 把 × 50ms，col5 222%；MeteorSword：down/push20/lift50 | Tick 50ms × 38，30/拍 / Kb 20 / Ly 50 |
| 流星剑出血（太刀） | 31% / Lv82 / 2.0s / 525 | ProcBuffId=Bleed 31%（可选） |
| 落地重击 | col10 2072%；#92：down/push350/lift200；盒 x±240 | Damage 250 / Hitstun 800 / Kb 350 / Ly 200 |
| 总时长 | 起跳到落地 ≈ 210+600+1000+1900+660 ≈ 4.4s（不含蓄力变化） | 3500（紧缩） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_meteorsword.skl | `.skl` 无子命令 | 手抄 15 列（列多，skl 子命令高收益样本） |
| 3 份 .atk（#92 + PO #12/#13） | `.atk` 无子命令 | 手抄；`blow`/`no blood` 字段并入 atk 立项输入（R2-A8 已录 blow） |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | 本档已给 #12/#13/#20-26/#27 对位表 |
| 各 .ani / .als | 全部 [use animation]/[add]/常规节 | **无缺口**（Up/Drop 单帧 500ms 属正常时长，非 L23 事件悬停型） |
| [SHADOW] 等 | 已知族 | 无新缺口 |

结论：缺口 `.skl`/`.atk`/`.obj` 族共性 3 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 角色升空 z=1000 悬停再落下（sq_ZStop + 主动 z 位移） | **施法者 z 主动位移缺失**（跳跃系统缺口 R1-A2 的姊妹项——LSFlight 只有受击物理） | 不升空：角色原地，落剑/爆地判定照常，仅损失"从天而降"演出（Up/Drop 悬停帧贴地播） |
| 方向键移动落剑标记（1s 窗口） | 技能中方向输入读取缺失（R1-A3） | 固定前方 2 单位 |
| 38 把剑独立随机落点（±100/±30） | 位置类确定性随机缺口（R2-A10 已录，044 墓碑同族） | 单雨区 Tick 表达判定（等价）；视觉若要 38 把散布需 LSRng 落点面 |
| 蓄力按住 + 25% 增伤 | 按住输入缺失 | 瞬发（charge=0） |
| 相机跟拍标记点/拉回 | 无跟随相机（锁步 demo 固定视角） | 跳过 |
| 霸体（sq_SetSuperArmor） | 无敌帧/霸体缺口（R1-A5） | 跳过 |
| 黑屏长闪（99990ms 直到落地） | 闪屏延后 | 跳过 |
| 追加剑组（极·神剑术 248） | 被动/掌握系统缺失 | 不做 |
| 太刀出血组 | 武器类型差异化缺失（R2-A6） | demo 可固定挂 31% Bleed 近似（§5 已给开关位） |
| 乱码混淆源码 | mod 污染（C3 族） | 本档还原语义为实现依据 |

## 8. 存疑与缺口上报

- **未考证**：①Up/Drop 单帧 ani 的 flag 1 消费者（无 onKeyFrameFlag 函数，疑引擎/残留）；②`character/swordman/effect/meteorsword/meteorsword_target.img` 小写路径与 `Effect/MeteorSword/` 大写路径是否同库（NPK 推导按同库处理）；③360 张 pvp level info（本档只做 dungeon 列）；④镜像 dropsword 两套目录的内容差异（未逐文件 diff，同构推断）。
- **缺口累计引用**：本技能是"施法者位置读取/位置随机"缺口（R2-A10，044-TombStoneRain §8）与跳跃系统缺口（R1-A2）的**双撞案例**——落剑类技能的标准简化范式见 §5（雨区 Tick），可供总览归档。
- **给下轮的经验**：meteorsword 是"长演出技能"样本：六子状态全部由 **onProc/onProcCon 时间驱动 + var("state") 内部相位**实现，没有一帧 SET FLAG 依赖——重建时按时间轴编排即可；24370 的 case 235 两 subType 分别对应"跳跃冲击波/落剑"，读 setcustomdata.nut 144-170 行即全部参数。
