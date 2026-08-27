# 崩山裂地斩（OutRageBreak）

> 技能ID 81 | 级别 B | 可实现性 🔶（跳跃捶地主干可表达：前跳位移 + 落地攻击盒 + 冲击波/岩浆双 Area；跳跃 z 抛物线、按方向键调距离、Rage 态直发岩浆为降级点） | 分析日期 2026-08-22 | 批次 B5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 崩山裂地斩 | `skill\Swordman\OutRageBreak.skl` [name] |
| 英文名 | OutRageBreak（取 skl 文件名；[name2] 实测 `Outrage Break`） | 同上 [name2] |
| 职业 | 狂战士（[skill fitness growtype]=3，L17） | 同上 |
| 学习等级 | 45 | 同上 [required level] |
| 最高等级 | 70（三觉前档 50） | 同上 [maximum level] / [growtype maximum level] 第 4 位=50 |
| 类型 | 主动（active，skill class 2） | 同上 [type] |
| 指令 | ↑→→ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 40000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 0（[consume MP] 0 0——不耗 MP） | 同上 |
| 特殊消耗 | 无色小晶块 ×2（[consume item] 3037 2 2）；施放扣 HP（见下） | 同上 |
| 可施放状态 | 状态 8（普攻）/ 245（FatalBlood） | 同上 [executable states] |
| 前置 | 技能 103 Lv1（具体技能未考证；explain 限定"血之狂暴态"= 状态/技能 232 RAGE，header 行 64/92 实证） | 同上 [pre required skill] |
| static data | `130 150 500 6 100 200 400 50 100`（9 槽，见 §8） | 同上 [dungeon][static data] |
| 一句话效果 | 召唤血气之剑跃起猛力捶击地面，大范围冲击波浮空敌人，地面喷出炙热岩浆多段追打浮空目标；仅血之狂暴态可用，施放扣 HP；按方向键控制跳跃距离 | 同上 [explain] |

**level property（4 列，模板 4 行 4 向量，L21 读法）**：HP消减量 = col3（`-1 3 1.0`，860→11360 固定值型）；
捶击物理攻击力 = col0（`-1 0 1.0`，630→4677%）；冲击波物理攻击力 = col1（`-2 1 1.0`，2958→21820）；
岩浆物理攻击力 = col2（`-2 2 1.0`，3108→22911）。四列语义全部由模板行文本直读（本技能无未考证列）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 130（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/outbreak/outbreak.nut", "OutBreak", 45 , 81 );
// swordman_header.nut 行 236/237（实测）：CUSTOM_ANI_OUTRAGEBREAKREADY <- 66，CUSTOM_ANI_OUTRAGEBREAKSLASH <- 67
// 行 64/65：STATE_SWORDMAN_RAGE <- 232，STATE_SWORDMAN_BLOODSNATCH <- 233
```

⚠ nut 目录名是 `outbreak\` 而非技能名（nut 目录名 ≠ 技能名又一例）；状态号 **45** ≠ 技能 ID **81**（L2 又一实证）。

### 2.2 主 nut 逐回调（outbreak.nut，244 行，无混淆痕迹）

- **checkExecutableSkill_OutRageBreak（门禁）**：`sq_IsUseSkill(81)` 时——
  若当前状态是 232（血之狂暴 RAGE）或 233（鲜血暴掠 BLOODSNATCH）：**跳过跳跃，直接**
  `sq_SendCreatePassiveObjectPacket(20044, 23, 100, 1, 0)`（岩浆 PO，subType 23 快速版）；
  否则 → 切状态 45 子状态 0（正常跳跃流程）。
- **onSetState（子状态 0，起跳准备）**：设攻击速度；扣 HP = `sq_GetLevelData(81, 3, level)`（= col3，HP消减量）。
- **onSetState（子状态 10，跳跃斩）**：
  叠加两层剑气特效（`sq_AddStateLayerAnimation` ← 1sword_slash_ldodge/none.ani，effect 层）；
  记录起点 x/y；读 static data 决定跳跃距离——
  `static[5]=200` 默认距离 / 顺方向按住 → `static[6]=400` / 逆方向按住 → `static[7]=50`；
  上/下按住 → `static[8]=100` 为 y 偏移（leny）；`static[1]=150` 为最大跳高 maxZ；
  播动画 67（OutRageBreakSlash）。
- **onKeyFrameFlag（子状态 0，flag 1 @Ready.ani F1=560ms）**：切状态 45 子状态 10（进入跳跃）。
- **onProc（子状态 10，每帧）**：帧 <5（DROP_FRAME=5）期间——水平 x/y 匀速插值
  （`sq_GetUniformVelocity`，时长 = 动画 F0-F4 delaySum=600ms），**z 走抛物线**
  （`getQuadraticFunction`：顶点 maxZ=150 落在行程中点）；撞墙（`isMovablePos` 假）则按剩余量缩短行程。
  帧 ≥5 → 切子状态 1（落地）。
- **onAfterSetState（子状态 1）**：`sq_SetCurrentTimeByFrame(animation, 5)`——落地段从 F5 续播；
  叠加落地剑气两层（sword_slash_ldodge1/none1.ani）。
- **冲击波/岩浆 PO 的创建点在引擎侧**（nut 全文无 20043/20044 常规创建调用；由引擎状态 45 按
  落地时点实例化——参照 087"半内置"定性；subType 23 的直发分支是唯一脚本可见创建）。

### 2.3 被动对象 / appendage

| PO（passiveobject.lst 实测） | .obj 结构 | .atk 关键值 |
|---|---|---|
| **20043 OutRageBreakFloor**（冲击波地板） | [layer] bottom / pass all 1000 / [basic motion] floor.ani（6 帧 3300ms，F0/F1 攻击盒 `-220,-65,-41 → 420×130×94` 偏移+尺寸）/ [etc motion] floor_over / floor_blood（各 3300ms 纯视觉）/ [attack info] OutRageBreakFloor.atk | physic / **down 击倒 / lift up 200 / blow / no blood 50 1.0**（pvp lift 450） |
| **20044 OutRageBreakBloodExp**（岩浆喷发） | [layer] bottom / pass all 1000 / [basic motion] bloodexp_damage.ani（**1 帧 760ms**，F0 攻击盒 `-220,-65,-41 → 440×130×212`，高柱状判定）/ [attack info] OutRageBreakBloodExp.atk / on end of animation 播完即毁 | physic / **down / lift up 200 / blow / no blood 20 1.0** |

- 两个 .obj 的 [name] 均为 `银光落刃的飞溅`——**复用银光落刃（EarthQuake）的地板飞溅 PO 家族**（L14 跨技能复用又一例）。
- 岩浆的多次喷发（bloodexp1/bloodexp2 双相视觉 + glow，各 480/240ms）由引擎按落地时点随机排布
  （bloodexp_damage.ani 是单帧长驻判定；_ds 变体为剑影专用，本技能不用）。
- **ap_outbreak.nut（55 行）**：仅记录施法者起点坐标、校验 state==45 的空壳 appendage（旧版位置回滚残留，
  proc 内 setCurrentPos 已被注释）——无机制意义。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\outragebreakready.ani（槽 66） | 2 | 620ms（560+60） | **F1=1（@560ms）**→切跳跃 | 无 | 起跳准备 |
| character\…\outragebreakslash.ani（槽 67） | 9 | 1980ms（120/120/60/120/180/60/60/60/**1200**） | 无 | **F5/F6/F7**：min/max `-10 -20 -2 188 40 124`（≈1.98×0.6×1.26 单位前伸盒） | F0-F4=跳跃段；F5 起落地捶击；F8=1200ms 蓄留帧（站桩收势，L23 同族） |
| PO floor.ani | 6 | 3300ms | 无 | F0/F1 | 冲击波判定（前 2 帧） |
| PO floor_over / floor_blood.ani | 6/7 | 3300ms | 无 | 无 | 地板余韵/血渍视觉相位 |
| PO bloodexp_damage.ani | 1 | 760ms | 无 | F0 | 岩浆判定（单帧长驻） |
| PO bloodexp1/2_ldodge/none.ani | 各 7 | 480ms | 无 | 无 | 岩浆两相视觉 |
| PO bloodexp_glow.ani | 3 | 240ms | 无 | 无 | 岩浆辉光 |
| effect\…\outragebreak\（11 个 .ani） | — | — | — | — | 剑气四组（ready/slash/1slash/impact）+ stone（引擎/脚本层叠加，§2.2） |

`.als` 边车：**无**（角色��� PO 两侧 animation 目录 ls 实测）；PO 与 effect 目录各有 particle 子目录（.ptl，L5 统一处理）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | OutRageBreak.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\OutRageBreak.skl` | ✅（260 行） | 4 列等级数据 |
| 注册行 | load_state 行 130（状态 45/技能 81） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 236/237/64/65 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 动画 66/67；状态 232/233 |
| 主 nut | outbreak.nut | `…\pvf\sqr\character\swordman\outbreak\outbreak.nut` | ✅（244 行） | 门禁/跳跃位移/HP 扣除 |
| ap nut | ap_outbreak.nut | 同目录 | ✅（55 行空壳） | 旧版位置回滚残留 |
| .chr 条目 | etc motion #66/#67（行 1039/1040）+ etc attack #OutRageBreak（行 1351） | `…\pvf\character\swordman\swordman.chr` | ✅ | Ready/Slash.ani + atk |
| 角色 .ani | outragebreakready / outragebreakslash（+ [pvp] 变体） | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | outragebreak.atk | `…\pvf\character\swordman\attackinfo\outragebreak.atk` | ✅ | physic/down/push0/**lift100**/hit down/武器伤害适用 1 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 无边车 | 剑气走 sq_AddStateLayerAnimation（脚本层） |
| PO lst | 20043/20044 行（11219-11222） | `…\pvf\passiveobject\passiveobject.lst` | ✅ | ID→obj 映射 |
| PO 定义 | outragebreakfloor.obj / outragebreakbloodexp.obj（+_ds ×2） | `…\pvf\passiveobject\character\swordman\` | ✅ | §2.3 |
| PO .ani | outragebreak\ 11 个（+outragebreak_ds\） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | §2.4 |
| PO .atk | outragebreakfloor.atk / outragebreakbloodexp.atk（+_ds ×2） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | down/lift200 |
| 施法特效 | outragebreak\ 11 .ani + particle | `…\pvf\character\swordman\effect\animation\outragebreak\` | ✅ | 剑气/石屑（脚本层引用） |
| 装备层 | *outragebreak*.ani ×152 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`——全部 img 位于 `Character/Swordman/Effect/OutRageBreak/`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色 Ready/Slash 动画 | 必需（共享） | ✅ 已在库 |
| outragebreak_floor.img | sprite_character_swordman_effect_outragebreak.NPK | 冲击波地板（floor.ani + effect 引用） | **必需** | ❌ |
| outragebreak_bloodsexp_1_ldodge.img / _1_none.img | 同上 | 岩浆相 1 视觉 | **必需** | ❌ |
| outragebreak_bloodsexp_2_ldodge.img / _2_none.img | 同上 | 岩浆相 2 视觉 | **必需** | ❌ |
| outragebreak_bloodsword_ldodge.img / _none.img | 同上 | 血气之剑（跳跃段剑气） | 可选 | ❌ |
| outragebreak_bloodsexp_glow.img | 同上 | 岩浆辉光 | 可选 | ❌ |

缺失 img：必需级 5 张 + 可选级 3 张，全部同属一个 NPK（一次提取全覆盖）。img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/OutRageBreakSkill.cs : SkillLogic`**（同 ReleaseWaveSkill 位移范式 + BloodBoomSkill 帧触发范式）
   - `CooldownMs = 40000`（DNF 原值直用，demo 可缩 10000）；`TotalTimeMs = 2600`（Ready 620 + Slash 1980）。
   - `MinCastHpPct`：DNF 前置"血之狂暴态"无对应（无 Buff 查询门面），demo 不设门槛，扣 HP 走 `ctx.ConsumeCasterHp(固定值)`（col3 原值 860，demo 建议 5% MaxHp 同 bloodboom 惯例）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanOutRageBreakReady)` + SubState=0。
   - OnUpdate（ElapsedMs + SubState 守卫）：
     - `≥560 && SubState==0`：切跳跃段——`ctx.PlayAnim(AnimId.SwordmanOutRageBreakSlash)` +
       `ctx.MoveCasterForward(2.0单位, 600ms)`（static[6]=400px 直用；**简化：直线位移替代抛物线，z 抛物线见 §7**）；SubState=1。
     - `≥1160 && SubState==1`（落地 = 560+600ms）：落地捶击——本体现 `ctx.SetAttackHitbox(前偏 0.9, 半尺寸 (1.0,0.3,0.6))`（F5-F7 盒折算）+ `HitReaction`（捶击， outragebreak.atk 原值）；
       `ctx.CreateAreaInFront(AreaIds.OutRageBreakFloor, 0)`（冲击波区，以落地点为中心）+ `ctx.CreateAreaInFront(AreaIds.OutRageBreakBloodExp, 0)`（岩浆区，同点）；SubState=2。
   - OnEnd：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/OutRageBreakFloorArea.cs : AreaDefinition`**（冲击波，同 ReleaseWaveArea 一次性爆发范式）
   - `TotalTimeMs=330`（floor.ani 前两判定帧时长）、`EnterActions={MeleeHit}`、
     `HalfExtents=(3.1,1.0,0.7)`（PO 盒 offset -220 + size 420 → 中心偏后 0.2、宽 4.2 单位 ÷100，demo 取中心对称 3.1）、
     `HitReaction{Damage=180, HitstunMs=800, KnockbackX=0, LaunchY=200}`（OutRageBreakFloor.atk down/lift200 → 浮空击倒）；
     `ViewAnimId=AnimId.OutRageBreakFloor`（floor.ani）+ `ViewEndAnimId=AnimId.OutRageBreakFloorOver`（floor_over 余韵）。
3. **`DotNet~/Areas/OutRageBreakBloodExpArea.cs : AreaDefinition`**（岩浆多段追打，同 FireCircleArea Tick 范式）
   - `TotalTimeMs=760`（bloodexp_damage 单帧时长）、`TickTimeMs=250`、`EnterActions={MeleeHit}`、`TickActions={MeleeHit}`（≈3 次追打，DNF 引擎排布多次喷发的简化——**同段定时多段，L19 第二档：Area Tick 无去重，天然多段**）、
     `HalfExtents=(2.2,0.65,1.5)`（bloodexp 盒 440×130×212 折算，高柱）、
     `HitReaction{Damage=60, HitstunMs=400, KnockbackX=0, LaunchY=200}`（bloodexp.atk 同 down/lift200——保持浮空连打手感）；
     `ViewAnimId=AnimId.OutRageBreakBloodExp1` + `ViewBackAnimId=AnimId.OutRageBreakBloodExpGlow`（辉光背层）。
4. **无需新 Buff/Action**（MeleeHit 现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 45 三子状态（Ready→Jump→Land） | SkillLogic SubState 0→1→2（bloodboom 同构） |
| 跳跃抛物线（getQuadraticFunction maxZ=150） | 简化为水平 `MoveCasterForward`（z 主动位移缺失，§7） |
| 按方向键调距离（static[5/6/7/8]） | 无技能中方向输入读取（R1-A3 缺口）——demo 固定前跳 400px 档 |
| F5-F7 落地攻击盒（角色 .atk） | 技能 `SetAttackHitbox` + `HitReaction`（帧驱动盒，releasewave 同构） |
| PO 20043 冲击波（bottom/穿透） | `OutRageBreakFloorArea`（一次性 Enter 结算） |
| PO 20044 岩浆（单帧 760ms 判定 + 引擎多次喷发） | `OutRageBreakBloodExpArea`（Tick 多段近似） |
| sq_AddStateLayerAnimation 剑气四层 | 无脚本层特效通道——视觉并入 Area ViewAnimId / 角色动画本体（跳过独立剑气层，§7） |
| 血之狂暴态门槛（232/233 直发岩浆） | 无 Buff 查询门面（R1-A3）——demo 统一走完整跳跃流程 |
| HP 消耗（col3） | `ctx.ConsumeCasterHp`（bloodboom 同构） |

### 注册点清单（草案号段，B5 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.OutRageBreak=27` + ButtonToSkill 新键 |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | SwordmanOutRageBreakReady=132、SwordmanOutRageBreakSlash=133、OutRageBreakFloor=134、OutRageBreakFloorOver=135、OutRageBreakBloodExp1=136、OutRageBreakBloodExp2=137、OutRageBreakBloodExpGlow=138 |
| AreaId | `AreaDefinition.cs` | OutRageBreakFloor=30、OutRageBreakBloodExp=31 |
| json / 图集 | LSAnimClipRegistrar / BuildAtlas | json ×7；img 必需 5 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000ms | 40000（直用；演示可改 10000） |
| 总时长 | Ready 620 + Slash 1980 = 2600ms | 2600 |
| 跳跃段 | F0-F4 600ms，行程 200/400/50px（方向键选择），z 顶点 150px | 前跳 4.0 单位 / 600ms 直线 |
| 捶击（本体） | col0 630~4677%；atk：down/push0/lift100 | 伤害 100/硬直 600/浮 100 |
| 冲击波 | col1 2958~21820；atk：down/lift200/blow | 伤害 180/硬直 800/浮 200 |
| 岩浆 | col2 3108~22911；atk：down/lift200 | 60/250ms tick ×3（≈总 180） |
| HP 消耗 | col3 860~11360（固定值） | 5% MaxHp（ConsumeCasterHp） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 角色/PO 全部 .ani（17 个） | 节面常规（F8 超长 DELAY 1200ms 属 L23 已记档家族——落地收势帧，消费侧约定"末帧超长=待机"即可） | 现有 ani 子命令全覆盖 |
| OutRageBreak.skl | `.skl` 无子命令（既有缺口） | 4 列模板行语义全部自明，手抄零负担 |
| 3 个 .atk（角色 1 + PO 2，+_ds 不计） | `.atk` 无子命令（既有） | 手抄 ~8 值/文件可接受 |
| 2 个 .obj | `.obj` 无子命令（既有） | 手工映射 2 Area（§5）；L9 相位建模建议不变 |
| particle 子目录 .ptl | 无子命令（L5 既有） | 跳过，特效 ani 替代 |

无新节。计 4 条既有缺口。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 跳跃 z 抛物线（顶点 150px，行进 600ms） | **施法者 z 主动位移**（R3-A15 已记档，流星落同款） | 水平 MoveCasterForward 直线位移；跳跃观感用 Slash 动画本体帧自带的高度差近似（资源帧本身画了跃起姿态） |
| 按方向键控制跳跃距离/上下偏移（static[5-8]） | 技能中方向输入读取（R1-A3 缺口） | demo 固定前跳 400px 档（最大距离） |
| 血之狂暴态才可用 + Rage/BloodSnatch 态直发岩浆（subType 23 快速版） | Buff/状态查询门面（R1-A3）+ 技能取消体系（R1-A4） | 无门槛直接施放；只做完整跳跃流程 |
| 岩浆喷发的引擎随机排布（bloodexp1/2 相位 ×多次） | 位置类确定性随机（R2-A10）+ 无引擎编排可见 | 单区 Tick 多段（250ms×3）近似"岩浆持续追打浮空敌人" |
| 剑气四层 sq_AddStateLayerAnimation（脚本层特效） | 无脚本层叠加通道（.als overlay 不适用于脚本层） | 跳过独立剑气层（角色动画+PO 视觉已够）；如需可手组装 overlay（releasewave 先例） |
| F8 1200ms 收势蓄留帧 | L23 超长 DELAY 族（事件推进语义） | TotalTimeMs 兜底 2600ms，末帧超长当站桩处理 |
| 屏震/音效（FLAMES_HIT/R_SQUARESWDC_HIT） | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 槽 0/2/3/4（130/500/6/100）语义——static[3]=6 疑为岩浆喷发次数（explain"岩浆攻击周围浮空的敌人"的多段数），无脚本消费方佐证。
2. 引擎侧创建 20043/20044 的确切时点与岩浆喷发排布逻辑（半内置编排不可见，见 §2.2 尾注）。
3. subType 23 的语义（20044 的写包参数第二位，疑为岩浆快速版子类型，引擎消费）。
4. [pvp] 变体 outragebreakslash.[pvp].ani 未读（demo 不做 pvp）。

**系统级缺口**：无新缺口（z 主动位移/方向输入/状态查询均为已记档缺口的再次实证：z 位移第 2 例、技能中方向输入第 3 例）。

**给下轮的经验**：狂战系老技能的 nut 目录常与技能名无关（本例 outbreak\），**先按技能 ID 在 load_state 第 5 参反查注册行**比按目录名猜更省事；PO 20043/20044 同族 .obj 的 [name] 复用"银光落刃的飞溅"，判定参数在各 obj 的 [attack info] 里，角色侧仅一个 atk。
