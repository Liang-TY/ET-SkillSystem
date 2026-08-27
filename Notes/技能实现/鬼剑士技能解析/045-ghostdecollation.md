# 冥灵断魂斩（ghostdecollation）

> 技能ID 45 | 级别 A | 可实现性 🔶（主路径两段斩可直接表达；鬼步特殊功能倚"技能取消体系"缺口） | 分析日期 2026-08-22 | 批次 A13

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冥灵断魂斩 | `skill\Swordman\ghostsword\ghostdecollation.skl [name]` |
| 英文名 | ghostdecollation（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype]=5，L17；ghostsword 族） | 同上 |
| 学习等级 | 45 | 同上 [required level] |
| 最高等级 | 60（growtype0/5 段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] / [skill class] |
| 指令 | ↓↑ + Z（指令施法 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 45000 ms（pvp 同 45000 + 开场 45000） | 同上 [cool time] |
| MP | 299 → 2511（pvp 149→1255） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无色小晶块 2 | 同上 [consume item] |
| 可施放状态 | 0/8/14；另经鬼步准备姿势特殊入口进 subState 2（§2.2） | 同上 [executable states] + jg_swordman_common.nut 实测 |
| 前置 | 技能 71（whiteghostslash）Lv1 | 同上 [pre required skill] |
| static data | `100 0` | 同上 [static data] |
| 一句话效果 | 聚灵魂之力把身边敌人向后推并大幅挥剑造成巨大物理伤害；在[鬼步]准备姿势下按技能键，鬼步终结动作变更为本技终结动作，并对命中敌人额外适用本技攻击力 | 同上 [explain] |

**static data 语义**（nut 消费印证）：`[0]`=特效/判定缩放率 100%、`[1]`=使用后重置鬼步 CD 的开关（0=关）。
**level info（1 列，Lv1 → Lv60）**：攻击力 18941→151533（PO 50/51 共用 `sq_GetBonusRateWithPassive(45,-1,0,1.0)` 实证）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
158: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/ghostdecollation/ghostdecollation.nut", "ghostdecollation", STATE_GHOSTDECOLLATION, SKILL_GHOSTDECOLLATION);
```

- `swordman_header.nut`：`STATE_GHOSTDECOLLATION <- 122`（47 行）、`SKILL_GHOSTDECOLLATION <- 45`（159 行）、`CUSTOM_ANI_GHOSTDECOLLATION1/2/CONTACT <- 286/287/288`（457-459 行）、`CUSTOM_ATTACK_INFO_GHOSTDECOLLATION <- 155`（546 行）。
- .chr etc motion 0 基 #286/287/288 = `ghostdecollation1/2/_contact.ani`（1259-1261 行，实测对位）；etc attack info #155 = `AttackInfo/ghostdecollation.atk`（1449 行）。另 #127/128 = `ghostdecollationready/attack.atk`（1421/1422 行）。
- 判定体 = F5 共享 PO 24349，首 dword ∈ {41, 50, 51}（按阶段）。

### 2.2 主 nut 逐回调（ghostdecollation.nut，243 行全读）

四子状态机（0→1→待机；鬼步特殊入口 2→3→待机）：

- `checkExecutableSkill`：常规路径 → 状态 122 subState 0。
- **subState 2 的入口在 jg_swordman_common.nut**：`SpiritMoveContact()`（733 行起）——鬼步（SPIRITMOVE）准备姿势中，五个剑术技能（含本技）按键即 `GhostSwordSetState` 进各自 contact 子状态并扣 MP/起 CD（本技 → subState 2）。这即 explain 的"[鬼步]准备姿势下按技能键"。
- `onSetState`：
  - **0 蓄势**：播 #286 `ghostdecollation1.ani`（17 帧 860ms，攻速随剑术精通 col0）；动画 F4/F5 自带攻击盒（引擎挂 `ghostdecollationready.atk`——nut 未显式设，高置信推断，§8）= "把身边敌人向后推"的第一击。
  - **1 大斩**：播 #287 `ghostdecollation2.ani`（11 帧 610ms）+ 音效；创建 PO 24349 **dword 50**（本体位置 0,0,0）= 主伤害段；`als_ani` 叠加 `attackeffect_01.ani`（×static[0]）。
  - **2 鬼步终结变体**：播 #288 `ghostdecollation_contact.ani`（6 帧 360ms，攻速 col1）；计算 xDistance = 当前位 + 鬼步距离（SPIRITMOVE static[0]=400px）；创建 PO 24349 **dword 41**（前方 300px）= 鬼步突进判定段；static[1]>0 时重置鬼步 CD（本 pvf 0=关）。
  - **3 变体大斩**：播 #287（攻速 col1）+ 双特效（attackeffect_01 + attackb_03）；创建 PO 24349 **dword 51**（0,0,0）= 主伤害段（2 倍尺寸）。
- `onKeyFrameFlag`：subState 0 F0=10001 音效；subState 1/3 F1=10002 闪屏（80/20/60/38 白）+ 屏震 17/220；subState 1 F2=10003 叠加 `attackb_03.ani`。
- `onProc` subState 2：向 xDistance **匀速突进 500ms**（`sq_GetUniformVelocity` + `sq_MoveToNearMovablePos` 撞墙止损）。
- `onProcCon` subState 1/3 帧 ≥3：`whiteGhostSlshContact(obj)`——白鬼之斩（71）按键则取消进该技能（MP/CD 检查在其内部）——**连携取消窗口**。
- `onEndCurrentAni`：0→1、1→待机、2→3、3→待机。

### 2.3 共享 PO 24349 / dword 41、50、51（setcustomdata.nut 实测）

| dword | 动画（etc motion 0 基） | atk | 伤害/参数 |
|---|---|---|---|
| 41 鬼步突进段 | #50 `…effect\animation\spiritmove\spiritmovedasheffect_00.ani` | `sq_GetCustomAttackInfo(obj,30)` → #30 = `attackInfo/spiritmove.atk`（**0 基直读即名称位，非 VS 族无错位**） | 伤害 = SPIRITMOVE col0（254@Lv1，鬼步自身攻击力）；尺寸 ×SPIRITMOVE static[1]=120%；**多段**：`setTimeEvent(0, delay/HitCount)` + `sq_SetMaxHitCounterPerObject(HitCount)`，HitCount = SPIRITMOVE static[2]=3 段 |
| 50 主伤害段（常规） | #55 `…effect\animation\ghostdecollation\ghostdecollation_attack.ani`（**空图占位**，视觉靠 als_ani 特效） | `sq_GetCustomAttackInfo(parentChr, 155)` → 角色 .chr #155 = `ghostdecollation.atk` | 伤害 = 本技 col0；尺寸 ×static[0]=100% |
| 51 主伤害段（鬼步变体） | 同 #55，速度 ×SpeedRateEx | 同 #155 | 伤害同 col0；**尺寸 ×2.0**（`local size = 2.0`，双倍判定+视觉） |

三份 atk 实测：

| atk | 关键参数 | 用途 |
|---|---|---|
| `character\swordman\attackinfo\ghostdecollationready.atk`（#127） | physic、weapon apply **0**、damage 反应、push 150、lift 100、knuck 3 100、force hit stun 1000、hit down | 蓄势推离第一击（引擎接线推断） |
| `character\swordman\attackinfo\ghostdecollation.atk`（#155，PO 50/51 用） | physic/weapon、**down** 反应、push 250、**lift 400**、blood 100 4.0、hit down/front、hit wav R_DARK_SWORD_HIT | 主伤害大斩 |
| `…unclebang…\attackinfo\spiritmove.atk`（PO #30，dword 41 用） | physic/weapon、**none** 反应、push 100、lift 200、hit horizon、**blow** hit info、no blood、knuck -1 | 突进段轻推 |

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\ghostdecollation1.ani`（#286 蓄势） | 17 | 860ms | F0=10001（音效） | F4/F5：`-68 -20 0 214 40 122`（min/max：282×60×122px，中心前偏 73px） | 攻速 ×剑术精通；.als 4 层特效（ghostdecollationready_00-03） |
| `ghostdecollation2.ani`（#287 大斩） | 11 | 610ms | F0=10001、F1=10002、F2=10003 | 无（伤害在 PO 50/51） | 无 .als（实测不存在） |
| `ghostdecollation_contact.ani`（#288 突进） | 6 | 360ms | 无 | 无（突进伤害在 PO 41） | 含 **[SPECTRUM] 武器残影节**（缺口记档） |
| `…effect\animation\ghostdecollation\ghostdecollation_attack.ani`（PO #55） | — | — | — | — | **空图占位**（L7），视觉=attackeffect_01/attackb_03 动态叠加 |
| `…attackeffect_01.ani` / `attackb_03.ani`（主斩特效） | — | — | — | — | DustD.img / DustC.img；attackeffect_01 自带 .als |
| `…effect\animation\spiritmove\spiritmovedasheffect_00.ani`（PO #50 突进特效） | — | — | — | — | SpiritMove02.img；ghostdecollation_ready 系 body 变体 ×4（.chr #233/234/248/249）无脚本引用者（§8） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ghostdecollation.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\ghostdecollation.skl` | ✅ 实测 | CD/MP/static/1 列 level info |
| 注册行 | swordman_load_state.nut 行 158 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 5_ghostsword\ghostdecollation\，状态 122，技能 45 |
| 主 nut | ghostdecollation.nut | `…\pvf\sqr\character\swordman\5_ghostsword\ghostdecollation\ghostdecollation.nut` | ✅ 实测（243 行全读） | 四子状态机 + 突进 + 连携 |
| 鬼步入口 | jg_swordman_common.nut（SpiritMoveContact/GhostSwordCommandEnable/GhostSwordSetState） | `…\pvf\sqr\character\JG_SwordMan\jg_swordman_common.nut` 733-800 行 | ✅ 实测 | subState 2 入口（C2 定点读） |
| 连携函数 | whiteGhostSlshContact（846 行起） | 同上 | ✅ 实测 | subState 1/3 帧起白鬼之斩取消窗口 |
| 共享 PO 回调 | setcustomdata case 41/50/51 | `…\pvf\sqr\shared_passive_object\swordman\setcustomdata.nut` 601/748/765 行 | ✅ 实测 | 三判定体参数 |
| 共享 PO 定义 | swordman_shared.obj | `…\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ 实测 | etc motion #50/#55、etc attack info #30（直读无错位） |
| 共享 PO atk | spiritmove.atk | `…\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\spiritmove.atk` | ✅ 实测 | 突进段命中 |
| .chr 条目 | etc motion #286/287/288（1259-1261 行）+ etc attack info #127/128/155（1421/1422/1449 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 三动画 + 三 atk |
| 角色 .ani | ghostdecollation1/2/_contact.ani + ghostdecollation1.ani.als | `…\pvf\character\swordman\animation\` | ✅ 实测 | 860/610/360ms |
| 角色 .atk | ghostdecollation.atk / ghostdecollationready.atk / ghostdecollationattack.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 主斩/蓄势推离/（attack.atk 引用者未考证） |
| PO 视觉 | ghostdecollation_attack.ani、attackeffect_00-10、attackb_00-03、attackc_00、ghostdecollationready_00-03、hiteffect\ | `…\character\swordman\effect\animation\ghostdecollation\` | ✅ 实测（目录 ls 25 文件） | 特效组 |
| 突进特效 | spiritmovedasheffect_00.ani | `…\character\swordman\effect\animation\spiritmove\` | ✅ 实测 | PO 41 视觉（与 126-spiritmove 共享） |
| 装备层 | ghostdecollation 系 ×532 | `…\pvf\equipment\character\swordman\avatar\{…}\*\` | ✅ 实测（find 计数 532） | 换装图层（只查存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/GhostDecollation/DustD.img` | sprite_character_swordman_effect_ghostdecollation.NPK | 主斩特效（attackeffect_01） | **必需** | ❌ |
| `…GhostDecollation/DustC.img` | 同上 | 大斩追加特效（attackb_03） | **必需** | ❌ |
| `…GhostDecollation/SwordTrailA.img / Flare.img / MistReady.img / SwordFlame.img` | 同上 | 蓄势段 .als 四层 | **必需**（视觉主体） | ❌ |
| `Character/Swordman/Effect/SpiritMove/SpiritMove02.img` | sprite_character_swordman_effect_spiritmove.NPK | 鬼步变体突进特效（与 126 共享） | 可选（鬼步变体用） | ❌ |
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色帧 | 必需（共享） | ✅ 已入库 |

**结论**：必需 6 张同属 `sprite_character_swordman_effect_ghostdecollation.NPK`（一次提取全覆盖）+ 可选 1 张。AnimConfigRegistry 无相关 AnimId（实测 grep）。

## 5. 实现方案草案

- **内容件清单**（常规路径 0→1 先行，鬼步变体 2→3 为可选增强）：
  - `GhostDecollationSkill : SkillLogic`（同 GoreCrossSkill 连段子状态机范式）：`CooldownMs=45000`、`TotalTimeMs=1600`（860+610=1470 + 余量）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanGhostDecollation1)` + `ctx.ClearHitTargets()`；SubState=0。
    - `OnUpdate`：蓄势段帧 4（≈300ms，F0-3 各 60ms）→ `ctx.SetAttackHitbox(前偏 0.7, 半尺寸 (1.4,0.3,0.6))` + 蓄势推击（HitReaction A）→ F6 后 `ctx.DisableAttackHitbox()`；动画末（860ms）SubState→1：`ctx.PlayAnim(AnimId.SwordmanGhostDecollation2)` + `ctx.CreateAreaInFront(AreaIds.GhostDecollationSlash, 0)`（主斩 Area 中心=本体）。
    - 鬼步变体（SubState 2/3，demo 可后置）：入口需技能取消体系（§7）——落地时为：OnCast 即 `ctx.MoveCasterForward` 匀速 4 单位/500ms（releasewave 纯函数位移范式）+ 突进段 Bullet/Area + 末段 2 倍尺寸主斩 Area。
  - `GhostDecollationSlashArea : AreaDefinition`：`TotalTimeMs=610`、`EnterActions={MeleeHit}`、`HalfExtents=(2.5,1.0,1.2)`（主斩以本体为中心，参考鬼斩类近身盒）、`HitReaction{Damage=280, HitstunMs=800, KnockbackX=250, LaunchY=400}`（ghostdecollation.atk 原值 push250/lift400/down）、`ViewAnimId=AnimId.GhostDecollationAttackEffect`（attackeffect_01 特效 json；ghostdecollation_attack.ani 是空占位跳过）。
  - 蓄势推击复用技能本体 `SetAttackHitbox` + `HitReaction`（Damage 60/硬直 1000/Kb 150/Ly 100，ghostdecollationready.atk 原值）——同 swordman_attack1 普攻范式。
  - 无新 Action/Buff。
- **概念映射**：subState 链 → LSCast.SubState；PO 50/51 → 一个 Area（2 倍尺寸变体用 HalfExtents 翻倍即表达）；PO 41 多段（3 段 setTimeEvent）→ 突进 Bullet `DestroyOnHit=false` + 同段定时多段（L19 三档之"Area/Bullet 加字段"档，demo 先单段）；flashScreen/屏震延后。
- **注册点**：SkillIds 加 `GhostDecollation=20`；AnimIds 加 `SwordmanGhostDecollation1=84`、`SwordmanGhostDecollation2=85`、`GhostDecollationAttackEffect=86`、`GhostDecollationReadyOv=87-90`（四层）；AreaIds `GhostDecollationSlash=11`；json 注册 + BuildAtlas ghostdecollation 图集 + 新按键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000ms | 45000（直用） |
| 总时长 | 0(860)→1(610) = 1470ms | TotalTimeMs 1600 |
| 蓄势推击 | atk ready：damage/push150/lift100/硬直1000/盒 282×60×122px | 伤 60/硬直 1000/Kb 150/Ly 100，盒前偏 0.7 |
| 主斩（PO 50） | atk：down/push250/lift400/blood100，col0=18941（万分率=189.4%） | 伤 280/硬直 800/Kb 250/Ly 400 |
| 鬼步变体突进 | 400px/500ms 匀速 + PO41 3 段（spiritmove.atk push100/lift200/none） | 4 单位/500ms；突进段伤 80×3 段（后置） |
| 变体主斩（PO 51） | 同主斩 ×2 尺寸 | HalfExtents ×2，伤同主斩（后置） |
| 白鬼取消窗口 | subState1/3 帧≥3 按键取消进 71 | 不实现（取消体系缺失） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `ghostdecollation_contact.ani` | **[SPECTRUM]/[SPECTRUM TERM]/[SPECTRUM LIFE TIME]/[SPECTRUM EFFECT]/[SPECTRUM COLOR]**（武器残影五节全套） | 既有缺口（R1-A1 上挑首见已记档）；整节跳过，残影不做 |
| 8 份涉及 ani（实测节名枚举） | `[INTERPOLATION]`（1 处）、`[SHADOW]`（4 处）、`[PLAY SOUND]`/`[SET FLAG]`/`[DAMAGE TYPE]` | 均已知跳过（[INTERPOLATION] 本批 026 首记）；无碍 |
| 同上 | `[GRAPHIC EFFECT]`（31 处） | **已支持**（L15，graphicEffect 字段），非缺口 |
| ghostdecollation1.ani.als | `[use animation]`/`[none effect add]` | 现有 als 子命令全覆盖 |
| .skl / .atk ×3 / swordman_shared.obj | 无子命令 | 既有缺口；本技能手抄可行 |

**结论**：ani/als 全部可译；新增实记 [SPECTRUM] 出现于本技能 contact 动画（此前记档在 046 上挑，此处为第二例）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 鬼步准备姿势特殊入口（subState 2：突进+变体终结） | **技能取消体系**（缺失，064 已上报；F6 流心系同族依赖） | demo 只做常规路径 0→1；变体路径等取消体系立项 |
| 白鬼之斩连携取消窗口（subState1/3 帧≥3） | 同上 | 不实现 |
| PO 41 突进段同段 3 段多段（setTimeEvent+MaxHitCounter） | 同段定时多段（L19 档二：Bullet/Area 加字段） | 变体后置时一并做；或先单段 |
| 攻速随剑术精通 col0/col1 | 属性数值消费链（缺失） | 固定 1.0 |
| 蓄势段 .als 四层 + attackeffect 特效 ×static[0] 缩放 | 对象整体缩放（延后） | 固定 100% |
| 突进撞墙止损（sq_MoveToNearMovablePos） | 撞墙检测（延后） | 不做（releasewave 同款简化） |
| 闪屏 + 屏震 17/220 | 延后 | 跳过 |
| 鬼步 CD 重置（static[1]>0） | 本 pvf 关（=0） | 不实现 |

## 8. 存疑与缺口上报

- **未考证**：①蓄势段 F4/F5 攻击盒配 `ghostdecollationready.atk` 的引擎接线（nut 未显式设攻击信息，名称与时机高度吻合，标高置信推断）；②`ghostdecollationattack.atk`（#128）与 `ghostdecollationready_body/attack_body/ex_*` 四动画（.chr #233/234/248/249）的引用者——5_ghostsword/JG_SwordMan 全 grep 无命中，疑引擎内置分支（鬼步自身状态用）或废弃资源；③level info 万分率的具体结算基数。
- **系统级缺口复证**：技能取消体系（本技能 + spinningslashvs 双双撞上，5_ghostsword 整族的入口机制）；同段定时多段（PO 41）。
- **翻译工具缺口复证**：[SPECTRUM] 五节（ghostdecollation_contact.ani，第二例）；[INTERPOLATION]（026 首记同款）。
- **给下轮**：ghostsword 族剑术六技（speedslash/ghostpierce/whiteghostslash/ghostdecollation/sworddancebs/spiritmove）的 contact 子状态全部挂在 `JG_SwordMan\jg_swordman_common.nut` 的 SpiritMoveContact/whiteGhostSlshSwordContact 两钩子上（733-800 行实测）——做族级取消/连携时从这两处入手。
