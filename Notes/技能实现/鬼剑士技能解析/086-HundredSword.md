# 极·鬼剑术（暴风式）（HundredSword）

> 技能ID 86 | 级别 B | 可实现性 🔶（深简化"剑阵吸引区+逐段斩+终结上挑"可表达主干：聚怪用负击退近似、终结 550 浮空直译；24 剑自动环绕演出/聚怪 hold 拖拽/连打加速终结为降级点） | 分析日期 2026-08-22 | 批次 B5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极·鬼剑术（暴风式） | `skill\Swordman\HundredSword.skl` [name] |
| 英文名 | HundredSword（取 skl 文件名；[name2] 实测 `Super Ghost Fencing， Storm Style`） | 同上 [name2] |
| 职业 | 剑魂（50 级一觉主动，skill class 1；[skill fitness second growtype] 1 2 + 鬼剑术系常识） | 同上 |
| 学习等级 | 50 | 同上 [required level] |
| 最高等级 | 70（一觉/二觉档 50，[second growtype maximum level] 第 3/4 位） | 同上 |
| 类型 | 主动（active） | 同上 [type] |
| 指令 | ↑↑↓↓ + Z | 同上 [command] |
| CD | 145000 ms；[league ban] 1 | 同上 [cool time] |
| MP | 980 → 8232 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×5 | 同上 [consume item] |
| 可施放状态 | 状态 8（普攻中可放） | 同上 [executable states] |
| static data | `150 35 250 20 3 6 9 3000`（8 槽，部分有 nut 印证见 §8） | 同上 [dungeon][static data] |
| 一句话效果 | 抛出 24 把剑（12 浮空+12 插地成剑阵）造成高僵直伤害；施放者持 24 剑在剑阵中连段攻击，受伤敌人被吸入剑阵中心；全部剑消失后以上斩一击终结 | 同上 [explain] |

**level property（6 列，模板 3 行 3 向量 + 3 列无模板）**：剑插地物理攻击力 = col0（970→9207）；
剑阵物理攻击力 = col1（646→6140）；完结技能物理攻击力 = col2（3877→36826）。
col3（Lv6 起 30→62）/col4（Lv9 起 6）/col5（Lv9 起 10000→40500，+500/级）无模板行——
按 explain 里程碑推断：col3 ↔ 3 级"一定时间无敌"（30→6.2s?×0.1 未证）、col4 ↔ 6 级"暴击率增加"（6%）、
col5 ↔ 9 级"斩铁式等级加成"（10000=100%?）——**均为推断**。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 97/99（实测）
IRDSQRCharacter.pushState(..., "character/swordman/HundredSword/HundredSword.nut", "swordman_hundredsword", 47, 86);
IRDSQRCharacter.pushState(..., "character/swordman/HundredSword/HundredSwordHit.nut", "swordman_hundredswordhit", 48, 86);
// swordman_header.nut 行 245-256（实测）：CUSTOM_ANI_HUNDREDSWORDREADY <- 75 … CUSTOM_ANI_HUNDREDSWORDFINISH <- 86
// passiveobject.lst（实测）：20051 = Character/Swordman/HundredSwordHit.obj（行 11235-11236）
```

三态结构：**47 暴风式主体 → 48 终结蓄势 → 49 终结上斩**（49 无注册行，引擎内置态，ap_hundredsword 反推实证）。
状态号 47/48 ≠ 技能 ID 86（L2 又一实证）。

### 2.2 主 nut 逐回调（hundredsword.nut 54 行 + hundredswordhit.nut 36 行，均干净无混淆）

**hundredsword.nut（状态 47）**：
- `onAfterSetState`：
  - 存 `speedRate` = (0, 5, 150, 800)——动画速度从 **150% 加速到 800%** 的插值参数（共 5 步）；
  - **聚怪抓取**：扫描全部敌人（对象管理器遍历），条件 = 敌对 + 可伤 + **可抓取（IsHoldable/IsGrabable）** +
    非固定物 + 距离判定（距身前 150px 中心 **x±240 / y±75 / z≤300**）→ 逐个挂 `ap_hundredsword.nut`
    appendage + `sq_HoldAndDelayDie`（抓取定身），写入向量（敌人原 x/y/z、剑阵中心 x/y、计数 0、步长 20）。
- `onEndState`：离开状态 47/48（即技能结束）→ 移除全部 ap_hundredsword。

**hundredswordhit.nut（状态 48，终结蓄势）**：
- `onAfterSetState`：按 speedRate 当前插值设动画速度（初始 150%）。
- `onProcCon`：每帧开放 攻击键/技能 86 指令；**每按一次 → time+1（0→5 步进）→ 动画速度 150%→800% 递增**——
  **连打加速终结**（连按攻击键把终结演出越打越快）。
- `onEndState`：同上清理。

**引擎内置部分（不可见，反推）**：状态 47 播 Ready.ani（25 帧 3 秒——抛 24 剑起手）后进入
**自动环绕连段**：MoveReady1-5（2 帧 250ms）+ MoveSlash1-5（1 帧 60ms 闪斩）五组循环——施放者在剑阵中
自动瞬移连斩（explain"施放者用这 24 把剑攻击周围敌人"）；剑阵伤害由 PO 20051 反复结算（见 2.3）；
24 剑的浮空/插地/飞散视觉 = passiveobject\animation\hundredsword\ 的 swordair/swordland/swordspread 系；
结束后切状态 48 →（连打耗尽/时序到）→ 状态 49 播 Finish.ani 终结上斩（角色 atk + PO finish 系视觉）。

### 2.3 被动对象 / appendage

**① PO 20051 HundredSwordHit（剑阵伤害体，.obj 实测）**：
- name `觉醒技暴风式` / bottom 层 / pass all / piercing 1000；
- [basic motion] HundredSwordHit.ani（2 帧 100ms，F0 攻击盒）+ [etc motion] HitLayer/Disappear1/Disappear2；
- [attack info] HundredSwordHit.atk：physic / damage 反应 / push 30 / lift 100 / **hit direction inner（向心）** /
  **ignore weight 1** / knuck back 3 10 / hit info 表外空值两行——**剑阵每击把敌人往中心吸**。

**② ap_hundredsword.nut（61 行，聚怪 hold 核心，逻辑全在脚本）**：
- `proc`（每帧）：施放者在状态 47/48（剑阵期）→ 敌人（appendage parent）被**拖向剑阵中心**：
  x/y 从原位向中心按 `sq_GetUniformVelocity(原位, 中心, time/20)` 插值；z 从原值压到 **70**（浮空定高）；
  施放者进入状态 49 且帧 >1 → 解除（终结击飞）；其他状态 → 直接失效。
- `onDamageParent`（敌人每次被 PO 打）：若攻击者位置恰在剑阵中心（x/y 相等）→ **推进拖拽计数 time+1（≤20）**——
  即"**每被剑阵打一次就被吸进一步**"（伤害驱动的逐步聚怪，DNF 版以伤代吸）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\hundredswordready.ani（槽 75） | 25 | **3000ms** | 无 | 无 | 抛起手（24 剑出场） |
| character\…\hundredswordmoveready1-5.ani（槽 76-80） | 各 2 | 各 250ms | 无 | 无 | 五组瞬移预备 |
| character\…\hundredswordmoveslash1-5.ani（槽 81-85） | 各 1 | 各 50-60ms | 无 | 无 | 五组闪斩（单帧姿态） |
| character\…\hundredswordfinish.ani（槽 86） | 6 | 770ms | 无 | **3 帧攻击盒**（F2-F4，实测有盒） | 终结上斩 |
| PO hundredswordhit.ani | 2 | 100ms | 无 | F0 | 剑阵单次伤害闪现 |
| PO hundredswordhitlayer / disappear1/2.ani | — | — | — | 无 | 叠层/剑消失视觉 |
| passiveobject\…\hundredsword\ 47 个 .ani | — | — | — | — | swordair1/2+glow（浮空剑）、swordland1/2+glow1/2（插地剑）、swordspread1/2、dash/dust（瞬移尘）、ready/slash/flash1-3/light1-3/lighttail1-4（连段特效）、finish 系 13 个（终结） |

`.als` 边车：**无**（角色与 PO 两侧实测）。角色动画仅引 sm_body（L16 ✓）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | HundredSword.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\HundredSword.skl` | ✅（266 行） | 6 列数据 |
| 注册行 | load_state 行 97/99（状态 47/48/技能 86） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 245-256 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 动画 75-86 |
| 主 nut ×2 | hundredsword.nut / hundredswordhit.nut | `…\pvf\sqr\character\swordman\hundredsword\` | ✅（54/36 行） | 聚怪抓取/连打加速 |
| ap nut | ap_hundredsword.nut | 同目录 | ✅（61 行） | 逐击聚怪 hold（§2.3②） |
| .chr 条目 | etc motion #75-86（行 1048-1059）+ etc attack #HundredSwordFinish（行 1356） | `…\pvf\character\swordman\swordman.chr` | ✅ | 12 动画 + 终结 atk |
| 角色 .ani | hundredsword 系 12 个 | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | hundredswordfinish.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | physic/**down/push300/lift550 击飞**/cut+blood50 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 无边车 | — |
| PO lst | 20051（行 11235-11236） | `…\pvf\passiveobject\passiveobject.lst` | ✅ | ID→obj |
| PO 定义 | hundredswordhit.obj | `…\pvf\passiveobject\character\swordman\` | ✅ | §2.3① |
| PO .ani | hundredswordhit 系 4 个 + hundredsword\ 47 个 | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | 剑阵/终结视觉 |
| PO .atk | hundredswordhit.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 向心吸附（§2.3①） |
| 施法特效 | hundredsword\ 47 个（与 PO 目录同名同内容，双份） | `…\pvf\character\swordman\effect\animation\hundredsword\` | ✅ | §2.4 末行 |
| 装备层 | *hundredsword*.ani ×912 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层（12 动作 ×76 变体） |

## 4. 资源需求

⚠ 本技能 img 引用含 **`%02d` 模板图集**（同 sm_body%04d 家族）：`sword%02d.img`（24 把剑个体）、
`weapon00-03%02d.img + weapon05%02d.img`（按武器类型分五族的武器贴图）——实际 img 数按动画帧引用展开。

| img（模板） | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色 12 动画 | 必需（共享） | ✅ 已在库 |
| sword%02d.img（24 张） | sprite_character_swordman_effect_hundredsword.NPK | 24 把剑本体 | **必需** | ❌ |
| slash.img / dash.img | 同上 | 连段斩/瞬移 | **必需** | ❌ |
| finish.img / finish_slash.img / finish_slash_circle.img | 同上 | 终结上斩 | **必需** | ❌ |
| floor_glow.img / floor_circle.img | 同上 | 剑阵地板 | **必需** | ❌ |
| cast.img / exp1 / exp2 / light_exp / light1-3 / lighttail1-4 / sword_light / sword_light_tail / sword_back_glow / swordair 系 / finish 系粒子（flash1-3/finishcircle1-4/finishflash/finishfloor/finishparticle1-2/finishtail1-4/dust） | 同上 | 全程视觉 | 可选 | ❌ |
| weapon00%02d/01/02/03/05%02d.img（五族模板） | 同上 | 施放者持剑挥砍（分武器类型） | 可选 | ❌ |
| supergirl_change_glows.img | sprite_character_mage_effect_bellatrix.NPK（**跨职业借图**，L14） | 换装光 | 可选 | ❌ |
| dust.img | sprite_character_swordman_effect_hardattackcharge.NPK（跨技能借图） | 瞬移尘 | 可选 | ❌ |

缺失 img：必需级 6 项（其中 sword%02d 展开为 24 张，合计 **29 张**）+ 可选级 ~30 张（主 NPK 一次提取全覆盖）。
img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案（深简化"吸力剑阵 + 三段斩 + 终结上挑"）

### 内容件清单

1. **`DotNet~/Skills/HundredSwordSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发范式；三态编排放单技能 SubState）
   - `CooldownMs=145000`（demo 缩 30000）；`TotalTimeMs=6000`（Ready 3000 + 剑阵 2000 + 终结 770 压缩为 5770，取整 6000）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanHundredSwordReady)`（25 帧起手，视图播完停末帧）+ `ctx.SetSubState(0)`。
   - OnUpdate（ElapsedMs + SubState 守卫）：
     - `≥3000 && SubState==0`：**剑阵区** `ctx.CreateAreaInFront(AreaIds.HundredSwordStorm, 1.5)`（150px 中心，
       static[0] 印证）+ 施放者开始连段表现（`ctx.PlayAnim(AnimId.SwordmanHundredSwordMoveSlash1)` 每段 60ms ×5 循环——
       **简化：不瞬移，站桩连斩**；MoveReady/瞬移砍掉见 §7）；`SetSubState(1)`。
     - `≥5000 && SubState==1`：终结 `ctx.PlayAnim(AnimId.SwordmanHundredSwordFinish)` +
       `ctx.CreateAreaInFront(AreaIds.HundredSwordFinish, 1.5)`（上挑终结区）；`SetSubState(2)`。
   - OnEnd：`ctx.PlayDefaultAnim()`。
   - **插地伤害**（col0 剑插地）：并入剑阵区首次 Enter 结算（见下）。
2. **`DotNet~/Areas/HundredSwordStormArea.cs : AreaDefinition`**（剑阵：吸附+持续伤害，同 FireCircleArea Tick 范式）
   - `TotalTimeMs=2000`、`TickTimeMs=200`（10 段）、`EnterActions={MeleeHit}`、`TickActions={MeleeHit}`；
   - `HalfExtents=(2.4,0.75,3.0)`（聚怪判定 x±240/y±75/z≤300 直译 ÷100）；
   - 首击 `HitReaction{Damage=90, HitstunMs=700, KnockbackX=0, LaunchY=100}`（插地 col0 + Hit.atk push30/lift100，
     高僵直）——**Tick 段改 `HitReaction{Damage=35, HitstunMs=250, KnockbackX=-30, LaunchY=70}`：
     负击退 = 向心吸附**（L22；对应 hit direction inner + ap 逐击拖拽的双重近似，L24 方向差异已知）；
     单 Area 双段 HitReaction 切换无通道（R4-A17 已记档）——**实现取舍：统一用 Tick 参数（-30/70），
     插地首击并入同参数**，伤害差并入终结（§7）；
   - `ViewAnimId=AnimId.HundredSwordStormFloor`（floor_glow 循环）+ overlay 手组装 swordair/swordland 双层（可选）。
3. **`DotNet~/Areas/HundredSwordFinishArea.cs : AreaDefinition`**（终结上挑）
   - `TotalTimeMs=770`（Finish.ani 直用）、`EnterActions={MeleeHit}`、
     `HalfExtents=(1.8,0.6,1.6)`（F2-F4 攻击盒折算）；
   - `HitReaction{Damage=350, HitstunMs=1000, KnockbackX=300, LaunchY=550}`（Finish.atk 原值 down/push300/**lift550**——
     全库最大浮空值，L24 击退方向差异注意）；
   - `ViewAnimId=AnimId.HundredSwordFinishSlash` + `ViewEndAnimId=AnimId.HundredSwordFinishCircle`。
4. **无需新 Buff/Action/Bullet**（聚怪用负击退；hold 拖拽简化掉）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 47/48/49 三态（引擎） | 单 SkillLogic 三段 SubState（时间驱动，087 同构） |
| 24 剑（12 浮空+12 插地） | 剑阵 Area 视觉层（sword%02d 图集展开，数量做视觉减配可 12→8） |
| ap_hundredsword 逐击拖拽聚怪 + z 压 70 | 负 KnockbackX 吸附近似（L22/L24）；z 压制/hold 缺失（抓取系统 R2-A7 同族） |
| PO 20051 反复伤害（hit direction inner） | StormArea Tick 多段 + 负击退（L19 第二档） |
| MoveReady/MoveSlash ×5 自动瞬移连段 | 站桩连斩动画循环（瞬移/目标跳跃砍掉，§7） |
| 连打加速终结（150%→800%） | 无动画速度控制门面——砍掉（固定速度播） |
| 终结 Finish.atk（lift550） | FinishArea HitReaction 直译 |
| 3 级无敌/6 级暴击/9 级斩铁加成（col3-5） | 无敌帧系统缺失（R1-A5）+ 暴击系统无 + 跨技能等级联动无 | 全砍 |

### 注册点清单（草案号段，B5 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.HundredSword=31` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanHundredSwordReady=161、SwordmanHundredSwordMoveSlash=162、SwordmanHundredSwordFinish=163、HundredSwordStormFloor=164、HundredSwordSwordAir=165、HundredSwordSwordLand=166、HundredSwordFinishSlash=167、HundredSwordFinishCircle=168 |
| AreaId | `AreaDefinition.cs` | HundredSwordStorm=34、HundredSwordFinish=35 |
| json / 图集 | LSAnimClipRegistrar / BuildAtlas | json ×8；img 必需 29 张（sword%02d 模板展开） |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 145000ms | 145000（演示可缩 30000） |
| 起手 | Ready 25 帧 3000ms | 3000（直译） |
| 剑阵 | col1 646~6140% 多段；atk push30/lift100/inner | 35 × 10 段（200ms）/吸附 -30/浮 70 |
| 插地首击 | col0 970~9207%（高僵直） | 90/硬直 700（并入首段近似） |
| 终结 | col2 3877~36826%；atk down/push300/lift550 | 350/硬直 1000/击退 300/浮空 550 |
| 总时长 | 3000+剑阵(引擎)+770 ≈ 6-8s | 6000（剑阵压 2000） |
| 聚怪判定 | 中心前 150px，x±240/y±75/z≤300 | Area 2.4×0.75×3.0 @前 1.5 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| HundredSword.skl（6 列 + 8 static 槽） | `.skl` 无子命令（既有） | 3 模板列自明；col3-5 + static 部分槽未考证 |
| 2 个 .atk（角色 Finish + PO Hit） | `.atk` 无子命令（既有）；PO 的 **[hit info] 空值行**（`` `` -1 5.0）与 **[ignore weight]/[knuck back 3 10]** 为表外字段 | 手抄；[ignore weight]/[knuck back] 已在 atk 子命令字段设计清单（R2-A8 系），空值行解析需容错 |
| hundredswordhit.obj（etc motion 3 项） | `.obj` 无子命令（既有） | 手工映射 2 Area（§5） |
| 全部 .ani（~60 个） | 节面常规 | 现有 ani 子命令全覆盖 |
| sword%02d / weapon0X%02d.img 模板 | 非 ani 节问题——**模板图集展开**（同 sm_body%04d，L16 惯例） | 提取侧按帧引用展开（24+5 族）；翻译工具无需改 |

计 2 条既有缺口 + 1 条解析容错建议（atk 空值行），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **逐击拖拽聚怪**（ap_hundredsword：每次被剑阵打→向中心插值一步 + z 压 70 + hold） | 位移他人门面（R2-A8）+ 抓取 hold（R2-A7 同族）+ 逐击状态推进（受击管线钩子 R3-A15 姊妹） | 负 KnockbackX 径向吸附（L22/L24 已知差异：径向≠朝心，扇形误差可接受） |
| 施放者瞬移五连斩（MoveReady/MoveSlash） | 目标位置读取/瞬移门面（R4-A18 目标位置读取同族）+ 技能中方向输入（R1-A3） | 站桩连斩循环动画（视觉弱化，判定不减） |
| 连打加速终结（150%→800% 动画速度） | 无动画播放速度门面 | 固定速度；连打交互并入"提前触发终结"（PeekBufferedButton 单次） |
| 单 Area 双段 HitReaction（插地重击 vs 吸附 tick） | 技能内分段 HitReaction 切换（R4-A17 已记档） | 统一 Tick 参数（§5.2 取舍） |
| 24 剑独立环绕演出（引擎编排） | 长时序演出 PO（087 同族） | 剑阵 Area 视觉层（数量减配 8-12 把） |
| 3 级无敌（col3） | 无敌帧（R1-A5 UNBREAKABLE 缺失） | 砍掉 |
| 6 级暴击（col4）/9 级斩铁联动（col5） | 暴击系统/跨技能等级联动（跨技能 level data 查询 R3-A11 同族） | 砍掉 |
| 音效/屏震/联赛禁用 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. col3/col4/col5 与三个里程碑（无敌/暴击/斩铁加成）的对应及换算（无模板行，推断）。
2. static data 槽语义：static[0]=150 有 nut 印证（剑阵中心距）；static[1]=35/static[2]=250/static[3]=20（疑 y 范围/时长/步长——ap 里硬编码 240/75/300/20/700 与 static 值相近但**不完全一致**，疑 mod 调参或两套并存）；static[4-6]=3/6/9（里程碑等级）；static[7]=3000（Ready 时长?）——部分推断。
3. 状态 47 内 MoveReady/MoveSlash 五连段的推进条件（自动/按键驱动）与 PO 20051 的创建节奏——引擎侧不可见。
4. 状态 49 的注册与行为（ap 反推 = 终结上斩态；无注册行、无 nut，纯引擎）。
5. effect 与 passiveobject 两侧 hundredsword 目录 47 文件同名同内容双份——分工未考证（疑引擎按侧取用）。

**系统级缺口**
- 无新缺口（聚怪/瞬移/连打加速/无敌均已记档；本技能是"位移他人门面"（R2-A8）在**持续吸附**场景的第 2 实证，建议 00-总览与拉拽类合并统计）。

**给下轮的经验**：一觉大技的"多状态"结构（47/48/49）只看注册行会漏态——**appendage 里的 `sourceObj.getState()` 分支是补全状态表的最佳线索**（本技能状态 49 即由此反推）。聚怪/吸力类技能先看 PO atk 的 `hit direction inner`——吸附语义在 atk 里就有，不必读 appendage 也能定性。
