# 血魔 · 弑天（swordman_bloodriven）

> 技能ID 231 | 级别 A | 可实现性 🔶（五段子状态机主干可直译：变身→蓄力→爆发眩晕→冲刺五段→终结爆炸；按住蓄力输入缺失（R3-A15 共性）、蓄力缩放/屏震闪屏延后） | 分析日期 2026-08-22 | 批次 A18

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 血魔 · 弑天 | `skill\Swordman\swordman_bloodriven.skl` [name] |
| 英文名 | swordman_bloodriven（取 skl 文件名；本 skl 无 [name2]，实测） | 同上 |
| 职业 | 狂战士（85 级二觉大招；[second growtype maximum level] 第 8 位=30，与 241/244/246 三技槽位互证：idx4=剑魂/idx5=鬼泣/idx7=狂战/idx9=阿修罗 0 基槽位） | 同上 + 技能名常识 |
| 学习等级 | 85 | 同上 [required level] |
| 最高等级 | 40（二觉档上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 3） | 同上 [type] |
| 指令 | ↓↑→→ + Z（指令施法 MP 优惠 50%/50% 档） | 同上 [command] / [skill command advantage] |
| CD | 180000 ms（pvp 起手 CD 600000） | 同上 [cool time] / [pvp][start cool time] |
| MP | 2500 → 5000（Lv1→40） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×10 | 同上 [consume item] 3037 10 10 |
| 可施放状态 | 8 / 0 / 14 | 同上 [executable states] |
| static data | `500 150 300 200`（pvp `500 150 400 200`） | 同上 [dungeon][static data] |
| 一句话效果 | 化身为血魔暴走突击前方敌人并引发血气爆炸；被血球命中的敌人受伤并爆炸；化身瞬间使周围敌人眩晕 | 同上 [explain] |

**level property（11 列，Lv1 → Lv40 首末值，13 向量与模板 13 个占位符一一对应，实测对位）**：

| 列/槽 | 模板变量 | Lv1 值 | 向量 | 说明 |
|---|---|---|---|---|
| static[0] | 蓄力时间上限 | 500×0.001=0.5 秒 | (0,0,0.001) | nut substate 1 记录 `sq_GetIntData(231,0)`=500ms 印证 |
| static[1] | 最大蓄力时大小比例 | 150% | (1,1,1.0) | PO setImageRate 1.0~1.5 |
| static[2] | 最大蓄力时突击距离 | 300 x轴 | (2,2,1.0) | uniform(300,300)=恒 300px |
| static[3] | 最大蓄力时眩晕范围 | 200 px | (3,3,1.0) | uniform(120,200) |
| col9+col0 | 血魔突击攻击力 | 24852% + 32759 | (-1,9)/(-2,0) | substate 3 印证 |
| col1 | 突击多段攻击次数上限 | 5 | (-1,1) | timeEvent 计数=col1-1 印证 |
| col10+col2 | 血气爆炸攻击力 | 54360% + 53717 | (-1,10)/(-2,2) | substate 4 写包印证 |
| col5 | 血球攻击力 | 9491 | (-2,5) | substate 0/1 印证 |
| col6 | 眩晕时间 | 2000×0.001=2.0 秒 | (-1,6,0.001) | flag 处理器 2000ms 印证 |
| col7 | 眩晕Lv | 102 | (-1,7) | setActiveStatus 印证 |
| col8 | 眩晕几率 | 100% | (-1,8) | 同上 |

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 27（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/bloodriven/bloodriven.nut", "swordman_bloodriven", 231, 231);
// swordman_header.nut 行 63/91/293-297/478-479（实测）：STATE/SKILL_SWORDMAN_BLOODRIVEN <- 231
//   CUSTOM_ANI_BLOODRIVENCAST<-123 / CHARGE<-124 / CHANGE<-125 / JUMP<-126 / FINISH<-127
//   CUSTOM_ATTACK_BLOODRIVENMULTIHIT<-87 / CHARGEHIT<-88
```

F7 标准结构（状态号=技能号）：完整 nut 管演出（243 行，C3 mod 混淆变量名但逻辑完整可读）+ 共享 PO 24370 只承担终结爆炸一击（L20）。

### 2.2 主 nut 逐回调（bloodriven.nut，243 行）

**五段子状态机**（onSetState 按 substate 分派，全部实测）：

| substate | 动画（.chr 槽） | 攻击信息 | 语义 |
|---|---|---|---|
| 0 | 123 Cast（17 帧 1000ms） | atk 87（power=col5 血球） | 起手变身；记录按键尾跟踪 `sq_IsEnterSkillLastKeyUnits`；屏震 1 |
| 1 | 124 Charge（6 帧 480ms 循环） | atk 87（col5） | **按住蓄力**：记录蓄力起点时间与上限（static[0]=500ms），动画循环 |
| 2 | 125 Change（8 帧 640ms） | —（无角色判定） | **变身爆发**：按 charge 比例 t=elapsed/limit 计算 var[0]=大小(100→150%)、var[1]=距离(300 恒)、var[2]=眩晕范围(120→200px)；屏震 4 |
| 3 | 126 Jump（6 帧 380ms） | atk 88（col9+col0 突击） | **血魔冲刺**：记录起点 x；timeEvent0 间隔=动画总长/(col1-1)、次数=col1-1 → 每 tick resetHitObjectList（**同段定时多段 5 段**） |
| 4 | 127 Finish（10 帧 820ms） | — | **终结**：由 flag 触发 PO 爆炸（见下） |

- **onProc substate 3（冲刺位移）**：`sq_GetDistancePos(起点, 方向, uniform(0, 距离300, 当前帧时间, 动画总长))` 匀速插值 + `isMovablePos` 撞墙止损（撞墙时扣除剩余距离）——releasewave 冲刺同构（01§5.6-2）。
- **onProcCon**：substate 0 动画播完时——仍按住技能键 → substate 1（蓄力）；否则 → substate 2（charge 参数 0/1=无蓄力）。substate 1——蓄力满 500ms 或松键 → substate 2（携带实际 elapsed/limit）；动画播完仍在按 → 循环 substate 1。
- **onEndCurrentAni**：2→3→4→STAND 线性推进。
- **onKeyFrameFlag**：
  - substate 2 flag 1（Change F2 @160ms 实测）：**化身瞬间眩晕 AoE**——遍历所有敌方 ACTIVE 对象，`距离 ≤ var[2]（120~200px）且 |Δz|≤300` → `sq_sendSetActiveStatusPacket(STUN, 几率col8=100, Lv col7, 时长 col6=2000ms)`。
  - substate 4 flag 1（Finish F0 @0ms 实测）：屏震 8 + 黑屏闪屏（delaySum(1,9)）+ 写包 `(231, 爆炸% col10, 爆炸平伤 col2, 大小 var[0])` → `sq_SendCreatePassiveObjectPacket(24370, 0, 0, 0, 0)` 于自身位置。
- **onTimeEvent 0**：`resetHitObjectList()`（冲刺 5 段的段间重置，间隔 380/4=95ms）。

### 2.3 被动对象 / appendage

**共享 PO 24370 case 231**（`sqr\common_object\share_obj\swordman\setcustomdata.nut:72`，实测）：
```
atk ← custom 1 = BloodRivenExplosion.atk；anim ← custom 1 = BloodRivenExplosion.ani（10 帧 820ms，F0-F3 攻击盒）
bonusRate ← col10；power ← col2；尺寸率 ← var[0]/100（1.0~1.5，图像+攻击盒三重缩放）
```
播完即毁（onendcurrentani case 231）。**本技能无 appendage**（`sqr\character\swordman\` 无 ap_bloodriven，实测目录仅 1 个 nut）。
爆炸视觉 = bloodrivenexplosion.ani 的 .als 挂 21 层 [none effect add]（blood/boomglow/boomline/hurricane/dust 系列，实测）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\bloodrivencast.ani（槽123） | 17 | 1000ms | F11=1(@520ms) | **F11-F16**（血球判定，atk87） | .als 挂 6 层（cast 系发光） |
| bloodrivencharge.ani（124） | 6 | 480ms | F0=1(@0) | **F0-F5**（全帧，atk87） | .als 挂 7 层（loop 系光环） |
| bloodrivenchange.ani（125） | 8 | 640ms | **F2=1(@160ms)→眩晕 AoE** | 无 | .als 挂 10 层（change 系爆发） |
| bloodrivenjump.ani（126） | 6 | 380ms | F3=1(@210ms)（无 nut 处理） | **F0-F5**（全帧，atk88 突击多段） | 冲刺动作 |
| bloodrivenfinish.ani（127） | 10 | 820ms | **F0=1(@0ms)→爆炸 PO** | 无 | .als 无（bloodrivenfinish.ani 实测无边车） |
| PO bloodrivenexplosion.ani | 10 | 820ms | 无 | **F0-F3** | .als 挂 21 层爆炸合成 |

**atk 实测**：
- `character\swordman\attackinfo\BloodRivenMultiHit.atk`（atk87）：physic / damage 反应 / **push 250 / lift 200** / hit horizon / knuck back 1。
- `BloodRivenChargeHit.atk`（atk88）：physic / damage / push 50 / lift 0 / hit horizon——冲刺贴地推。
- PO `BloodRivenExplosion.atk`（custom 1）：physic / **down 击倒** / push 200 / lift 150 / hit horizon。

全链时长：1000（起手）+480（蓄力上限）+640（爆发）+380（冲刺）+820（终结）≈ **3.3 秒**（不含按住延长）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_bloodriven.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_bloodriven.skl` | ✅（277 行） | 技能数据（11 列 + static 4 槽） |
| 注册行 | load_state 行 27（231/231） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 63/91/293-297/478-479 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画 123-127/攻击 87/88 |
| 主 nut | bloodriven.nut | `…\pvf\sqr\character\swordman\bloodriven\bloodriven.nut` | ✅（243 行） | 五段子状态机 |
| ap nut | —（不存在） | 同目录（仅 1 个 nut） | ⛔ 无 | 无 appendage |
| 共享 PO | share_po_swordman_24370 六回调 case 231 | `…\pvf\sqr\common_object\share_obj\swordman\setcustomdata.nut` 等 | ✅（L20） | 终结爆炸 |
| .chr 条目 | etc motion #123-127（行 1096-1100）；etc attack info #87/88（行 1381/1382） | `…\pvf\character\swordman\swordman.chr` | ✅ | 五动画 + 两攻击 |
| 角色 .ani/.als | bloodrivencast/charge/change/jump/finish.ani（cast/charge/change 有 .als） | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | BloodRivenMultiHit.atk / BloodRivenChargeHit.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | §2.4 |
| PO .obj | qq506807329new_swordman_24370.obj etc motion#1 / etc attack#1 | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | BloodRivenExplosion.ani/.atk（0 基直读，L20） |
| PO .ani/.als | bloodrivenexplosion.ani + .als + boom 系列 21 个 | `…\passiveobject\script_sqr_nut_qq506807329\swordman\animation\bloodriven\` | ✅（22 文件） | 爆炸视觉 |
| PO .atk | BloodRivenExplosion.atk | `…\passiveobject\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ | 爆炸命中 |
| 施法特效 | cast/change/loop 系列 32 文件 | `…\pvf\character\swordman\effect\animation\BloodRiven\` | ✅ | .als 引用层 |
| 装备层 | *bloodriven*.ani ×359 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |

## 4. 资源需求

img → NPK 规则：`sprite_<img路径下划线化>.NPK`（01§2 Step 4）。本技能 img 全部集中在两个 NPK 族（一次提取全覆盖）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色五动画图集 | 必需（共享） | ✅ 已在库 |
| riven_circle.img / riven_circle_dodge.img | sprite_character_swordman_effect_bloodriven.NPK | 施法/蓄力光环（cast.ani + charge.als） | 必需 | ❌ |
| riven_sbody_normal.img | 同上 | 变身形态贴图（change.ani） | 必需 | ❌ |
| riven_jump01.img | 同上 | 冲刺残影（jump.ani） | 必需 | ❌ |
| riven_cast_normal / cast_dodge / castglow / changeglow / glowall / plusglow / bottomblood（7 张） | 同上 | .als 施法/爆发叠加层 | 可选 | ❌ |
| riven_backblood / bblood / boomglow / boomline / dust_dodge / glowall / hurricane00（7 张，PO 爆炸 .als 层） | 同上 | 终结爆炸合成 | 必需（爆炸主视觉） | ❌ |

缺失 img：必需级 5 张（skin 外）+ 可选级 7 张，同属 `sprite_character_swordman_effect_bloodriven.NPK` 一族（每 img 实际一个 NPK 文件，按 01§2 规则逐张提取）。img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案

### 内容件清单（全部继承真实基类；数值 DNF 原值 + demo 建议值并列）

1. **`DotNet~/Skills/BloodRivenSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发 + ReleaseWaveSkill 纯函数位移两范式合成）
   - `CooldownMs=180000`（demo 建议 30000）；`TotalTimeMs=3300`（五段动画全链 3320ms 收尾）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanBloodRivenCast)` + `ctx.ClearHitTargets()`；无蓄力输入（R3-A15 共性缺口）——**demo 瞬发取满蓄力档**（大小 150%/眩晕 200px，SubState 直接从"变身"推进）。
   - OnUpdate（ElapsedMs + SubState 推进，五段一次性守卫）：
     - `≥1000 && SubState==0`：播 `AnimId.SwordmanBloodRivenChange`；`≥1160`（Change 160ms 处）`ctx.CreateArea(AreaIds.BloodRivenStun, ctx.GetTargetPosition())` 眩晕区（变身点=施放快照，无位移误差）+ SetSubState(1)；
     - `≥1640 && SubState==1`：播 `AnimId.SwordmanBloodRivenJump` + SetSubState(2)；血球阶段（Cast/Charge 的 atk87 判定）demo 并入冲刺段（见 §7 简化）；
     - 冲刺段（1640-2020ms）：`ctx.MoveCasterForward(300px÷100÷380ms×dtMs)` 纯函数匀速（releasewave §5.6-2 同构）；每 95ms `ctx.ClearHitTargets()`（**同段定时多段=ClearHitTargets 定时调用，L19 已通**）+ 冲刺盒 `ctx.SetAttackHitbox(前偏0.8, (0.9,0.4,0.8))`（atk88 原值 push50 贴地推）；
     - `≥2020 && SubState==2`：播 `AnimId.SwordmanBloodRivenFinish` + `ctx.DisableAttackHitbox()` + `ctx.CreateAreaInFront(AreaIds.BloodRivenExplosion, 0)`（冲刺终点爆炸，CreateAreaInFront 用实时位置）+ SetSubState(3)。
   - OnEnd：`ctx.PlayDefaultAnim()`。
   - `HitReaction{Damage=60, HitstunMs=400, KnockbackX=50, LaunchY=0}`（atk88 突击贴地推原值）。
2. **`DotNet~/Areas/BloodRivenStunArea.cs : AreaDefinition`**（变身眩晕，同 FireCircleArea 范式）
   - `TotalTimeMs=200`、`TickTimeMs=0`、`HalfExtents=(2.0,0.5,2.0)`（眩晕范围 200px÷100）、`EnterActions={MeleeHit}`、
     `HitReaction{Damage=10, HitstunMs=200, KnockbackX=0, LaunchY=0, ProcBuffId=BuffIds.BloodDemonStun, ProcChance=100}`——眩晕走 ProcBuffId 通道（FreezeBuff 先例，L6）。
3. **`DotNet~/Areas/BloodRivenExplosionArea.cs : AreaDefinition`**（终结血气爆炸，同 BloodBoomArea 一次性爆发范式）
   - `TotalTimeMs=820`（explosion.ani 全长）、`EnterActions={MeleeHit}`、`HalfExtents=(1.8,0.6,1.8)`（F0 盒折算）、
     `HitReaction{Damage=250, HitstunMs=800, KnockbackX=200, LaunchY=150}`（BloodRivenExplosion.atk 原值 down/push200/lift150——击倒表现同 064 相位2）；
     `ViewAnimId=AnimId.BloodRivenExplosion`。
4. **`DotNet~/Buffs/BloodDemonStunBuff.cs : BuffDefinition`**（2 秒眩晕，复制 StunBuff 改时长）
   - `TotalTimeMs=2000`（DNF col6 原值）、AddActions={ForbidMoveOn} / RemoveActions={ForbidMoveOff}——与 StunBuff 同构零新机制。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 231 五子状态 + 动画 123-127 | `BloodRivenSkill` + 4 个 AnimId（Charge 蓄力段 demo 砍） |
| flag 触发（眩晕/爆炸） | OnUpdate ElapsedMs 时刻 + SubState 守卫 |
| 蓄力 uniform 插值（大小/距离/范围） | 缺按住蓄力输入 → 固定满档（§7） |
| 冲刺 onProc 匀速插值 + 撞墙止损 | `MoveCasterForward` 纯函数；撞墙检测延后（无地图碰撞） |
| timeEvent resetHitObjectList 5 段 | `ctx.ClearHitTargets()` 每 95ms 定时调用（已落地门面） |
| 眩晕 setActiveStatusPacket | `HitReaction.ProcBuffId=BloodDemonStun`（L6 通道） |
| PO 24370 爆炸 | `BloodRivenExplosionArea`（CreateAreaInFront 于冲刺终点） |
| 屏震/闪屏/音效 | 延后跳过 |

### 注册点清单（草案号段，A18 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.BloodRiven=23` + ButtonToSkill 新键 |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | SwordmanBloodRivenCast=104、…Change=105、…Jump=106、…Finish=107、BloodRivenExplosion=108（Charge=109 预留，蓄力版后补） |
| AreaId | `Packages\cn.etetet.skill\Runtime\AreaDefinition.cs` | BloodRivenStun=16、BloodRivenExplosion=17 |
| BuffId | `Packages\cn.etetet.skill\Runtime\BuffDefinition.cs` | BloodDemonStun=10 |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×4~5 + img 必需 5 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 180000ms | 30000 |
| 总时长 | ≈3320ms（五段） | 3300 |
| 眩晕触发 | Change F2 @160ms（累计 1160ms） | ElapsedMs 1160 |
| 眩晕 | 几率100%/Lv102/2000ms/范围 120-200px | StunBuff 2000ms/范围 2.0 单位 |
| 冲刺 | 300px/380ms 匀速，撞墙止损 | 3 单位/380ms 匀速（无撞墙） |
| 突击多段 | 5 段（间隔95ms），24852%+32759，push50 | 60 伤害×5 段 |
| 血气爆炸 | 54360%+53717，down/push200/lift150，尺寸×1.5 | 250/硬直800/推200/浮150 |
| 血球（Cast/Charge 判定） | col5 9491，push250/lift200 | 并入冲刺段（§7） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 8 个角色 .ani + PO 爆炸 .ani | 节面常规（FRAME/DELAY/IMAGE/RGBA/GRAPHIC EFFECT 均已支持，L15） | 现有 ani 子命令全覆盖 |
| bloodrivencast/charge/change.ani.als + explosion.ani.als | `[none effect add]`（已支持） | 现有 als 子命令全覆盖 |
| boom_riven_boomline1/2.ani、boom_riven_dust_dodge.ani | **`[FLIP TYPE]`（值 `HORIZON`，帧级水平镜像）——新节首见** | 建议 ani 子命令加 flipType 字段（帧级 mirror）；消费侧视图 scale.x=-1 即可，代价低 |
| swordman_bloodriven.skl（11 列 + static 4） | `.skl` 无子命令（既有缺口） | 手抄 13 向量可接受；并入既有缺口 |
| 3 个 .atk（MultiHit/ChargeHit/Explosion） | `.atk` 无子命令（既有缺口） | 手抄每文件 ~8 值 |
| 24370 .obj | `.obj` 无子命令（既有缺口） | 本技能仅用 etc#1 一对，手工映射无成本 |

计 2 条既有缺口 + 1 条新节（[FLIP TYPE]）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 按住蓄力（0-500ms 比例缩放大小/眩晕范围/距离） | **按住蓄力输入缺失**（R3-A15 四技共性；输入缓冲只有按下沿） | demo 瞬发满档（150%/200px）；升级路径=输入缓冲加"按住时长"通道 |
| 血球阶段（Cast F11-16/Charge F0-5 的 atk87 判定，被血球命中者爆炸） | 可表达（帧盒+HitReaction）但与冲刺段重叠、叙事冗余 | demo 砍血球段：判定统一进冲刺（atk88），爆炸只保留终结一次 |
| PO 尺寸随蓄力缩放（图像+攻击盒三重） | 对象整体缩放（延后，IMAGE RATE 同族） | 固定满档尺寸 |
| 冲刺撞墙止损（isMovablePos） | 无地图碰撞（延后） | 不检测，位移恒定 |
| 屏震（4 段递增至 8）/黑屏闪屏/音效 | 屏震闪屏音效（延后） | 跳过 |
| 变身形态贴图切换（riven_sbody） | 已可表达（.ani 图集帧） | 直译即可 |

## 8. 存疑与缺口上报

**未考证项**
1. Charge 段 F0 flag 1 / Cast 段 F11 flag 1 无 nut 处理器——推断为引擎侧音效/特效标记。
2. 血球"命中后发生爆炸"的二次爆炸实现（atk87 命中是否有引擎侧连锁，无脚本可查）——demo 已砍该段，无影响。
3. 装备层 359 个 .ani 仅 find 计数未逐一核对（含 5 动画 × 多 avatar 层）。

**新缺口上报（主循环汇总）**
1. **[FLIP TYPE] 翻译节**（本批 231/241/244/246 四技共 45 处，帧级水平镜像）：ani 子命令加 flipType 字段 + 视图 scale.x 翻转——低成本高覆盖（大量 PO 特效 ani 用它做左右变体）。
2. **按住蓄力输入**（本技能 + R3-A15 已记档四技再实证）：建议输入缓冲扩展 held 时长通道，一次立项多技能受益。

**给下轮的经验**：`swordman_` 85 二觉系（231/241/244/246）与 60 级系（233/238）同为 F7 结构，但 85 级四技的共享 PO 24370 用法更重（写包 subType 分模式/多状态推进）——分析时先读 `share_obj\swordman\setcustomdata.nut` 的 case <技能号> 拿 subType 分发表，再按 setstate/onendcurrentani 追状态推进链；`qq506807329new_swordman_24370.obj` 的 etc motion/attack 表是**全二觉系共用动画总表**（0 基直读），拿到即可整族查表。
