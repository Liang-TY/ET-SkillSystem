# 裂魂乱舞（sworddancebs）

> 技能ID 170 | 级别 A | 可实现性 🔶（普通线判定/位移/分段伤害可完整直译；鬼步接力与白鬼斩取消两特殊功能撞技能取消体系缺口） | 分析日期 2026-08-22 | 批次 A17

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 裂魂乱舞 | `skill\Swordman\ghostsword\sworddancebs.skl [name]` |
| 英文名 | sworddancebs（取 skl 文件名） | 同上 |
| 职业 | 剑影 · 夜刀神（二觉 Lv80；[second growtype maximum level] 槽 11=30 → growtype 5=剑影二觉档；ghostsword 目录 + 鬼步联动常识） | 同上 + L17 |
| 学习等级 | 80 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↑↓→ + Z；指令施法 MP 优惠 20%/40% 档 | 同上 [command] / [command key explain] / [skill command advantage] |
| CD | 45000 ms（固定） | 同上 [cool time] |
| MP | 823 → 6172 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块（道具 3037）×5 | 同上 [consume item] |
| 可施放状态 | 0 / 8 / 14 | 同上 [executable states] |
| 武器特效 | physical | 同上 [weapon effect type] |
| 一句话效果 | 前冲三连斩（可被反向键刹车），接大范围旋转乱舞多段 + 终结斩击；在[鬼步]终结窗口按技能键可从"接触段"直接接力发动（终结加倍尺寸）；旋转末期可取消进[白鬼斩] | 同上 [explain] + nut 走读 |

**static data（dungeon）**：`100 500 800 0` = 斩击范围 **100%**（static[0]，判定盒三轴缩放）/ 斩击移动速度 **500**（static[1]）/ 旋转斩击移动速度 **800**（static[2]）/ 删除第 1、2、3 次斩击开关 **0=关**（static[3]）。
**level property（5 列 level + 4 列 static 引用）**：第1次斩击 col0 **2658%**、第2次 col1 **7974%**、第3次 col2 **10632%**、旋转斩击 col3 **6645%**（每拍）、终结斩击 col4 **18606%**；斩击范围/两档移动速度/删除开关四参取 static 槽（向量 `(0,0)(1,1)(2,2)(3,3)`——L21 正源=static 槽读法实证）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（F5 族标准链路）

```
// sqr/character/swordman_load_state.nut 行 159（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/sworddancebs/sworddancebs.nut", "sworddancebs", STATE_SWORDDANCEBS, SKILL_SWORDDANCEBS);
// = (…, 127, 170)：状态号 127 ≠ 技能号 170（L2 老世代号段）
// 共享 PO 24349 的注册在 unclebang 链（F5 已记档，本技能写包 dword 41/52-56 分流）
```

- `swordman_header.nut`：`STATE_SWORDDANCEBS <- 127`（行 48）、`SKILL_SWORDDANCEBS <- 170`（行 164）、`CUSTOM_ANI_SWORDDANCEBS{1,2,3,4}_ <- 289-292` + `CUSTOM_ANI_SWORDDANCEBS_CONTACT <- 293`（行 460-464）、`CUSTOM_ATTACK_INFO_SWORDDANCEBS{1,2,3,4,5} <- 156-160`（行 547-551）。
- .chr 对位（0 基实测吻合）：etc motion #289-293 = 1262-1266 行 `sworddancebs{1,2,3,4}.ani` + `sworddancebs_contact.ani`（行号-973 ✓）；etc attack info #156-160 = 1450-1454 行 `AttackInfo/sworddancebs{1,2,3,4,5}.atk`（行号-1294 ✓）。
  ⚠ .chr 另有第二组同名系条目：#240-243（1213-1216 行 `sworddancebs01-04body_body.ani`）与 #136-139（1430-1433 行 `sworddancebs{1,2,3}+finish.atk`）——**本 nut 不使用**（消费者未考证，疑引擎旧版残留），勿混。
- 主 nut 472 行**未混淆**（可读，同 245）。

### 2.2 主 nut 逐回调（sworddancebs.nut，六子状态）

全段动画速度乘 **SpeedRate/SpeedRateEx**（来自被动技能 123[剑影精通] col0/col1，`1+值`——被动速度加成，延后档）。

**onSetState（每子状态）**：

| 子状态 | 动画 | 行为 | PO 24349 写包 |
|---|---|---|---|
| 0 斩击1 | #289（7 帧 420ms） | — | dword **52**（PO 播 sworddancebs1_attack，atk=sworddancebs1.atk ×col0，盒随 static[0] 缩放） |
| 1 斩击2 | #290（7 帧 360ms） | — | dword **53**（atk2 ×col1） |
| 2 斩击3 | #291（9 帧 440ms） | — | dword **54**（atk3 ×col2） |
| 3 旋转乱舞 | #292（29 帧 1395ms） | als 播 04effecta_48/04effectb_13 旋转特效两层（尺寸 ×static[0]%） | dword **55**（PO 播 sworddancebs4_attack，**moveWithParent 跟随角色**，盒 ×1.0） |
| 4 鬼步接触（特殊功能） | #293 contact（6 帧 360ms） | 存 spiritMove 终点（前方 SKILL_SPIRITMOVE static[0] 距离） | dword **41**（**前方 300px** 出生；PO=spiritmovedasheffect，atk=共享 obj atk30 spiritmove.atk ×SPIRITMOVE col0，多段=timeEvent(delay/HitCount)+MaxHitCounter） |
| 5 特殊功能终结 | #292 再播（SpeedRateEx） | 同 3 的 als 特效 | dword **56**（同 55 但**盒 ×2.0 双倍尺寸**） |

**onEndCurrentAni**：0→1→2→3→STAND（普通线）；4→5→STAND（特殊功能线）。

**onProc（速度位移——冲刺与刹车）**：
- 子状态 0/1：body 动画帧到 **flag 10001**（斩1=F3/斩2=F1）→ `sq_SetVelocity(x, ±static[1]=500)`（面朝方向冲刺；**按住反方向键则速度置 0=刹车**）；帧到 **flag 10004**（斩1=F6/斩2=F5）→ 速度归零。
- 子状态 3：帧到 **flag 10002**（F8）→ 速度 ±static[2]=**800**（旋转推进）；**flag 10008**（F16）→ 归零。
- 子状态 4：500ms 匀速插值冲向 spiritMove 终点（sq_MoveToNearMovablePos 撞墙安全）。

**onKeyFrameFlag（视觉/反馈，10001-10009 号段）**：斩1 F4=10002 播 01effect02 特效、F5=10003 震 2/80；斩2 F2/F3 同构；斩3 F0=10001 播 03effect、F3=10002 震 5/100；旋转段 F5=10001 震 8/60、F9=10003 白闪、F15=10007 震 3/60、F16=10008 白闪、F17=10009 震 10/200、F14=10006 音 R_SWORDDANCE_BS_ATK；**10004/10005/10006/10009 四旗是 PO 侧伤害节拍**（见 §2.3）。

**onProcCon**：
- 子状态 0：static[3] > 0（删除前三斩开关，本 skl=0 关）→ 直接跳子状态 3。
- 子状态 3/5：帧 ≥18 → `whiteGhostSlshContact(obj)`——若[白鬼斩]（技能 71）特殊功能已学且 MP/CD 允许 → **旋转末期可取消进白鬼斩**（jg_swordman_common.nut:846 实测）。

**外部接力入口（F5 族 VS 接力体系）**：
- `SpiritMoveContact`（jg_swordman_common.nut:738，鬼步终结窗口调用）：`GhostSwordSetState(SKILL_SWORDDANCEBS, [4], STATE_SWORDDANCEBS)`——**鬼步中按本技能键 → 直接进子状态 4**（接触段起手，跳过前三斩）——即 explain 的"[鬼步]的终结动作变更为[裂魂乱舞]终结动作"。
- `whiteGhostSlshSwordContact`（白鬼斩终结窗口，行 871）：可从白鬼斩接回本技能**子状态 0**（普通起手）。

### 2.3 共享 PO 24349（unclebang，case 52-56/41）

**判定结构（setcustomdata + onkeyframeflag 实测）**：

| case | 动画（shared obj etc motion 0 基） | 攻击信息 | 伤害 |
|---|---|---|---|
| 52/53/54 三连斩 | **#56/57/58** = effect/…/sworddancebs/sworddancebs{1,2,3}_attack.ani | 角色 .chr **#156/157/158** sworddancebs{1,2,3}.atk | col0/col1/col2（一次性，无重置） |
| 55 旋转 | **#59** = sworddancebs4_attack.ani（**moveWithParent 跟随角色**） | **逐拍设置**：flag 10004(F10) 设 atk159——fallthrough——10005(F12) **resetHitObjectList**+atk159；10006(F14) 重置+atk159；10009(F17) 重置+**atk160** | 前三拍 col3 旋转%、末拍 col4 终结% |
| 56 特殊终结 | 同 #59（盒 ×2.0） | 同 55 节拍（同 flags） | 同（SpeedRateEx 加速） |
| 41 鬼步接触 | **#50** = effect/…/spiritmove/spiritmovedasheffect_00.ani | **共享 obj 自有 atk #30** spiritmove.atk | SPIRITMOVE col0（鬼步伤害）；timeEvent(delay/HitCount) + setMaxHitCounterPerObject(HitCount) 计数多段 |

onattack：case 41/52-56 全部 → `GhostSword_Attack_Effect`（jg_swordman_common.nut:800——通用随机命中斩痕视觉，common/hiteffect 资源）。onendcurrentani：动画播完销毁。

**关键点**：三连斩 PO 是"独立判定体"（出生在角色位、判定盒前伸 3-4 单位）；旋转 PO **跟随角色**（角色 800px/s 推进中判定随之走）；多段节奏 = **每拍 resetHitObjectList**（L19 段间重命中档）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG（号段 10001-10009） | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `sworddancebs1.ani`（#289，角色） | 7 | 420ms | F3=10001(速度起)、F4=10002(特效)、F5=10003(震)、F6=10004(速度停) | 无（判定在 PO） | .als 挂 01effecta 链 |
| `sworddancebs2.ani`（#290） | 7 | 360ms | F1=10001、F2=10002、F3=10003、F5=10004 | 无 | .als 挂 02effect 链 |
| `sworddancebs3.ani`（#291） | 9 | 440ms | F0=10001(特效)、F3=10002(震) | 无 | .als 挂 03body/03effect 链 |
| `sworddancebs4.ani`（#292） | 29 | 1395ms | F5=10001、F8=10002(速度起)、F9=10003、**F10=10004/F12=10005/F14=10006（PO 三拍）**、F15=10007、F16=10008(速度停)、**F17=10009（PO 终结拍）** | 无（PO 侧生效） | .als 挂 04effectA/B 首层 |
| `sworddancebs_contact.ani`（#293） | 6 | 360ms | 无 | 无 | 特殊功能起手 |
| PO `sworddancebs1_attack.ani` | 7 | 420ms | 同 body（同步镜像） | **F5/F6**：`-76,-50,-2,370,100,221` | IMAGE 空壳（L7）——视觉在角色层 |
| PO `sworddancebs2_attack.ani` | 7 | 360ms | 同上 | **F3-F5**：`-170,-50,0,400,100,221` | 同上 |
| PO `sworddancebs3_attack.ani` | 9 | 440ms | 同上 | **F3-F5**：`-100,-50,0,400,100,180` | 同上 |
| PO `sworddancebs4_attack.ani` | 29 | 1395ms | 同 body | **F10-F19 ×10**：`-250,-50,0,500,100,300`（5×1×3 单位旋转区） | 同上 |
| effect/…/sworddancebs/ 92 个（01effect/02effect/03body/03effect/04effectA/B 链） | — | — | — | — | 壳层+子层两段式（同 245 结构） |

atk 关键值（实测）：sworddancebs1/2 = physic/damage/push **50**/lift **150**/hit down；3 = physic/**down**/push 50/lift 90；4（旋转拍）= physic/**down**/push **150**/lift 70/hit down；5（终结）= physic/**down**/push **250**/lift **180**/**hit lift up**；PO spiritmove.atk（case 41）= physic/**none**/push 100/lift 200/**blow**/knuck back -1。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ghostsword/sworddancebs.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\sworddancebs.skl` | ✅（204 行） | 5 列 + 4 static 槽 |
| 注册行 | load_state 行 159（127/170） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 127 |
| 常量 | swordman_header.nut 行 48/164/460-464/547-551 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画/攻击槽位 |
| 主 nut | sworddancebs.nut（472 行，**未混淆**） | `…\pvf\sqr\character\swordman\5_ghostsword\sworddancebs\sworddancebs.nut` | ✅ | 六子状态机 |
| PO 回调 | shared_passive_object/swordman/{setcustomdata,onkeyframeflag,onattack,onendcurrentani}.nut 的 case 41/52-56 | `…\pvf\sqr\shared_passive_object\swordman\` | ✅ | F5 六回调（本技能无 procappend/ontimeevent 分支） |
| PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\` | ✅ | etc motion #50/#56-59、atk #30 对位（0 基直读实测吻合；**非 VS 族 63-74 段，无 -2 错位**） |
| PO atk | attackInfo/spiritmove.atk | `…\unclebang_shared_passive_object\swordman\attackInfo\` | ✅ | case 41 鬼步接触 |
| .chr 条目 | etc motion #289-293（1262-1266 行）+ etc attack info #156-160（1450-1454 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | 五动画 + 五 atk（另组 #240-243/#136-139 未用） |
| 角色 .ani | sworddancebs{1,2,3,4}.ani + .als ×4、sworddancebs_contact.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | sworddancebs{1,2,3,4,5}.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | §2.3 |
| PO .ani | effect/animation/sworddancebs/sworddancebs{1,2,3,4}_attack.ani | 同 effect 目录 | ✅ | 判定体（空壳镜像） |
| 特效 .ani | sworddancebs/ 92 个 + .als ×6 | `…\pvf\character\swordman\effect\animation\sworddancebs\` | ✅ | 壳层链结构 |
| 关联函数 | jg_swordman_common.nut:738/772/800/846（SpiritMoveContact/GhostSwordSetState/GhostSword_Attack_Effect/whiteGhostSlshContact） | `…\pvf\sqr\character\JG_SwordMan\jg_swordman_common.nut` | ✅ | 接力/取消/命中视觉 |
| 装备层 | *sworddancebs* ×684 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层（存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 五段角色动画 | 必需（共享） | ✅ |
| `…/SwordDanceBS/{01,02,03,04,05,06}.img` | sprite_character_swordman_effect_sworddancebs.NPK | **斩击/旋转剑光全套**（06.img 89 处为主视觉） | **必需** | ❌ |
| `…/SpeedSlashUpper/en01.img`、`en03.img` | sprite_character_swordman_effect_speedslashupper.NPK | 剑气拖尾（229/179 处——**跨技能借用**，F5 族常态） | **必需** | ❌ |
| `…/SpeedSlashBS/BSSpeedSlash04.img` | sprite_character_swordman_effect_speedslashbs.NPK | **旋转主视觉**（220 处——跨技能借用） | **必需** | ❌ |
| `…/SpeedSlashUpper/en02.img`、`en04.img` | 同 Upper NPK | 拖尾变体 | 可选 | ❌ |
| `…/SpeedSlashBS/BSSpeedSlash01.img`、`BSSpeedSlash03.img` | 同 BS NPK | 旋转变体 | 可选 | ❌ |
| `…/BladeSpirit/{001,002,003}.img` | sprite_character_swordman_effect_bladespirit.NPK | 剑影精通精灵层 | 可选 | ❌ |

缺失 img：**必需 10 张（3 个 NPK：本族 1 + 跨技能 Upper/BS 各 1）**、可选 5 张。跨技能三 NPK 均为 F5 族共享库（speedslash/白鬼斩等同族技能复用，提取一次多技受益）。
img 版本红线（v2/v4 可 / v5 不可）由提取时把关。

## 5. 实现方案草案

**结构映射**：三连斩 = 三次"前冲 + 前方单发 Area"（每 Area 自带 HitReaction——**绕开 245 的分段反应切换缺口**，PO 化判定天然适配 Area）；旋转 = 逐拍 `CreateAreaInFront(0)` 于角色当前位置（DNF PO 跟随角色 ≈ 逐拍以施法者位建区——角色 800px/s 推进中每拍落一区）；前冲 = MoveCasterForward 纯函数增量。
**砍掉**：鬼步接力（子状态 4/5）与白鬼斩取消——技能取消体系缺口（已有记档）；反向键刹车——方向输入缺失。

### 内容件清单

1. **`DotNet~/Skills/SwordDanceBsSkill.cs : SkillLogic`**（SubState 时间编排 + ReleaseWave 位移范式）
   - `CooldownMs = 45000`；`TotalTimeMs = 2620`（420+360+440+1395 普通线）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSwordDanceBs1)` + `ctx.ClearHitTargets()`；SubState=0。
   - `OnUpdate`（时间驱动，段推进 + 冲刺 + 建区）：
     - 段0（0~420）：F3 起冲刺——t∈[180,420] 窗口 `ctx.MoveCasterForward(5 单位/s × dt/1000)`（static[1]=500px/s）；t≈300（PO F5 盒时刻）`ctx.CreateAreaInFront(AreaIds.SwordDanceSlash1, (FP)15/10)`；t≥420：换动画 2 + ClearHitTargets；SubState=1。
     - 段1（420~780）：F1 起冲刺 t∈[460,720]；t≈600 建 Slash2 区；t≥780：换动画 3 + ClearHitTargets；SubState=2。
     - 段2（780~1220）：无位移（DNF 段2 无速度分支）；t≈1000（PO F3 盒）建 Slash3 区；t≥1220：换动画 4 + ClearHitTargets；SubState=3。
     - 段3 旋转（1220~2615）：F8 起推进 t∈[1350,1670]（800px/s=8 单位/s）；**三拍** t≈1400/1550/1800 各 `ctx.CreateAreaInFront(AreaIds.SwordDanceSpin, FP.Zero)` + ClearHitTargets（旋转拍，DNF F12/F14）；**终结拍** t≈2050（F17）`CreateAreaInFront(AreaIds.SwordDanceFinish, FP.Zero)`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/SwordDanceSlash{1,2,3}Area.cs : AreaDefinition`**（三连斩，BloodBoom 单次范式）
   - `TotalTimeMs = 200`、`EnterActions = { MeleeHit }`；
   - Slash1：`HalfExtents = (22/10, 75/100, 11/10)`（PO F5 盒 x[-76,370] 半宽 223px/前伸 370px 折算，中心前 1.5 单位）、`HitReaction { Damage = 70, HitstunMs = 500, KnockbackX = 50, LaunchY = 150 }`（atk1 原值）；
   - Slash2：盒 x[-170,400] → 半 (28/10, 75/100, 11/10) 前 1.2；Damage 90（col1 7974%）反应同 atk2；
   - Slash3：盒 x[-100,400] → 同构；Damage 120（col2 10632%）；atk3=down → HitstunMs 700。
3. **`DotNet~/Areas/SwordDanceSpinArea.cs : AreaDefinition`**（旋转拍 ×3 次复用）
   - `TotalTimeMs = 150`、`EnterActions = { MeleeHit }`、`HalfExtents = (37/10, 75/100, 15/10)`（PO 盒 x[-250,500] z[0,300]）；
   - `HitReaction { Damage = 100, HitstunMs = 700, KnockbackX = 150, LaunchY = 70 }`（atk4 原值 down/push150/lift70；col3 6645%/拍）。
4. **`DotNet~/Areas/SwordDanceFinishArea.cs : AreaDefinition`**（终结拍）
   - 同盒、`HitReaction { Damage = 200, HitstunMs = 900, KnockbackX = 250, LaunchY = 180 }`（atk5 原值 down/push250/lift180/lift up；col4 18606%）。
5. **无新增 Buff/Action**。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 127 六子状态（普通线 0-3） | SubState 时间编排直译 |
| PO 24349 case 52/53/54 三连斩判定体 | 三个单发 Area（每 Area 独立 HitReaction——**本结构天然适配我们的 Area 体系**，无 245 的分段反应缺口） |
| 旋转 PO moveWithParent 跟随 + 每拍 resetHitObjectList | 逐拍 CreateAreaInFront(0)（施法者位建区 ≈ 跟随；拍间 ClearHitTargets = L19 段间重命中） |
| sq_SetVelocity 500/800 冲刺 + 反向键刹车 | MoveCasterForward 增量（刹车砍——方向输入缺失 R1-A3） |
| 剑影精通 SpeedRate 加速 | 动画速度门面未暴露（延后）→ 固定 1.0 |
| static[3] 删除前三斩开关 | 可表达（SubState 起点 3）——demo 不做开关，记档 |
| 鬼步接力（子状态 4/5 + PO 41 鬼步伤害 + 盒 ×2） | **技能取消体系缺失**（064/R1-A4 记档；F5 族 VS 接力第 N 实证）→ 不做 |
| 白鬼斩取消（旋转末期） | 同上缺口 → 不做 |
| GhostSword_Attack_Effect 随机命中斩痕 | 位置随机缺口（R2-A10）→ Area ViewAnimId 单层替代 |
| als 特效链（01-04effect 系 + 尺寸缩放） | .als overlay 翻译可通；尺寸缩放（IMAGE RATE 族）延后 → 固定尺寸 |
| 震 2~10 / 白闪 / 音效 | 延后 → 跳过 |
| PO 盒三轴 ×static[0] 缩放 | 对象整体缩放（延后，累计多例）→ demo 100% |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SwordDanceBs = 26` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `SwordDanceSlash1 = 24`、`Slash2 = 25`、`Slash3 = 26`、`SwordDanceSpin = 27`、`SwordDanceFinish = 28` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanSwordDanceBs1 = 126`、`Bs2 = 127`、`Bs3 = 128`、`Bs4 = 129`、`Contact = 130`（可选）、剑光层 131+（可选） |
| json 注册 | `LSAnimClipRegistrar.cs` | 角色 4~5 个 + .als overlay 链 |
| 图集 | `LSAnimResComponentSystem.cs` | 必需 10 张（3 NPK） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000 ms | 45000（直用） |
| 总时长 | 2615ms（420+360+440+1395 普通线） | 2620 |
| 冲刺1/2 | 500px/s，F3→F6 / F1→F5 | 5 单位/s 同窗口 |
| 旋转推进 | 800px/s，F8→F16 | 8 单位/s 同窗口 |
| 斩击1 | col0 2658%；atk1 push50/lift150；盒 x[-76,370] | Damage 70 / Hitstun 500 / Kb 50 / Ly 150 |
| 斩击2 | col1 7974%；atk2 同参；盒 x[-170,400] | Damage 90 / 同反应 |
| 斩击3 | col2 10632%；atk3 down/push50/lift90 | Damage 120 / Hitstun 700 |
| 旋转拍 ×3 | col3 6645%/拍；atk4 down/push150/lift70；盒 5×1×3 单位 | Damage 100/拍 / Hitstun 700 / Kb 150 / Ly 70 |
| 终结拍 | col4 18606%；atk5 down/push250/lift180/lift up | Damage 200 / Hitstun 900 / Kb 250 / Ly 180 |
| 鬼步接触（特殊） | SPIRITMOVE col0；atk=spiritmove.atk none/push100/lift200/blow | 不做（取消体系缺口） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ghostsword/sworddancebs.skl | `.skl` 无子命令 | 手抄 5 列 + 4 static 槽（static 槽语义本档已考） |
| 6 份 .atk（角色 5 + PO spiritmove） | `.atk` 无子命令 | 手抄；`[blow]`/`[knuck back]` 并入 atk 立项输入 |
| swordman_shared.obj | `.obj` 无子命令 | 本档已给 #50/#56-59/#atk30 对位表 |
| 各 .ani/.als | 节名常规（[none effect add]/[SHADOW]/[LOOP]） | **无新节缺口**；PO _attack 空壳（L7）照常翻译（IMAGE 空 path 帧渲染空白） |
| als_ani 尺寸缩放参数（100×sizeRate） | 运行时缩放（非文件内容） | 对象整体缩放缺口（延后档）——非翻译问题 |

结论：缺口 = `.skl`/`.atk`/`.obj` 族共性 3 条，无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 鬼步中按技能键接力（子状态 4 接触段→5 双倍终结） | **技能取消体系缺失**（064 首报；F5 族 VS 接力第 4 实证——SpiritMoveContact 五技同窗） | 不做（普通线完整）；接力版等取消体系立项 |
| 旋转末期取消进白鬼斩（71） | 同上缺口 | 不做 |
| 冲刺反向键刹车（按反方向速度置 0） | 技能中方向输入读取缺失（R1-A3） | 固定全速冲刺 |
| 旋转判定体跟随角色（moveWithParent） | Area 无跟随门面（243 §8 同款新缺口） | **逐拍以施法者位建区**近似（拍间角色位移 ≤1.6 单位，判定断续可接受；比 243 主云的跟随更贴合） |
| 剑影精通（123）全段动画加速 | 动画速度门面未暴露（延后）+ 跨技能 level 查询门面（R3-A11） | 固定 1.0 |
| static[0] 判定盒三轴缩放 / als 特效尺寸 | 对象整体缩放（延后） | 固定 100% |
| 特殊终结盒 ×2.0 | 同上 | 不做（特殊线整体砍） |
| 随机命中斩痕（GhostSword_Attack_Effect） | 位置随机缺口（R2-A10） | 单层视觉替代 |
| 震 2~10 / 白闪 / R_SWORDDANCE_BS_ATK | 延后 | 跳过 |
| 冲刺撞墙安全（MoveToNearMovablePos） | 无地图碰撞（延后） | 无墙直用 |

## 8. 存疑与缺口上报

**未考证项**
1. .chr 第二组同名条目（#240-243 body_body 系 ani + #136-139 finish 系 atk）的消费者（本 nut 不用；疑引擎旧版或 mod 残留）。
2. case 41 的 timeEvent(0, delay/HitCount, 0, false) 无 ontimeevent 分支（count=0+false 语义下疑似仅靠 setMaxHitCounterPerObject 限次——鬼步（126）文档可互证，未深究）。
3. pvp 段 level info 为空（本技能无 PVP 数据）。
4. subState 4 的 xDistance 用 SKILL_SPIRITMOVE static[0]（鬼步位移距离）——具体值在 spiritmove.skl（本档未读，属技能 126 范畴）。

**系统级缺口（非新增，实证补充）**
1. **技能取消体系第五实证**：F5 族 `SpiritMoveContact`（jg_swordman_common.nut:738）把五技（speedslash/ghostpierce/whiteghostslash/ghostdecollation/sworddancebs）统一挂进鬼步终结接力窗——取消体系立项时应把"鬼步接力窗"作为 F5 族的标准接入口一并设计（与 R2-A6 记录的 VSObject 在场记忆同族）。
2. **Area 跟随门面**第二实证（旋转 PO moveWithParent；243 主云第一实证）——本档给出"逐拍建区"近似法可作过渡范式。
3. **分段 HitReaction 的两条出路对照**（本批内部结论）：PO 化判定（本技能/064）→ 每 Area 独立反应，天然适配；角色帧驱动判定（245）→ 单 HitReaction 折中。总览归档时建议合并成一条"连段技判定载体选择"指引。

**给下轮的经验**：F5 族（24349）写包 dword 已知值累计：41=鬼步接触（spiritmove 系）、42/43=speedslash 系、45/46=ghostpierce、47-51=其他 ghostsword 技、52-56=**sworddancebs 专用**、57=ultimatecrossslash、60+=VS 族（63-74 有 -2 错位）。sworddancebs 的 `_attack.ani` 空壳与角色 body ani **帧表完全同步**（同 delay 同 flag）——判定镜像可只翻译一份当两份用。
