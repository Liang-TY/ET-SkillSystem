# 魂破斩（whiteghostslash）

> 技能ID 71 | 级别 A（维持预判） | 可实现性 🔶（正常施放下劈主干直接可实现；[鬼步]特殊功能链与追加下劈依赖技能取消体系/跨技能数据，简化掉——且本 pvf 开关数据本就关闭追加下劈） | 分析日期 2026-08-22 | 批次 A11

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 魂破斩 | `skill\Swordman\ghostsword\whiteghostslash.skl [name]` |
| 英文名 | whiteghostslash（取 skl 文件名，全小写；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype]=5，L17；ghostsword 族） | 同上 |
| 学习等级 | 35（[required level range] 2） | 同上 [required level] |
| 最高等级 | 60（growtype0/5 段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1）/ 物理 | 同上 [type] / [weapon effect type] |
| 指令 | ↑↓ + Z（[skill command advantage] 20/40） | 同上 [command] |
| CD | 12000 ms | 同上 [cool time] |
| MP | 129 → 1083 | 同上 [consume MP] |
| 特殊消耗 | 消耗道具 3037×1 | 同上 [consume item] |
| 可施放状态 | 0（站立）/ 8（攻击中）/ 14（疑幻鬼共存态，066 同款未考证） | 同上 [executable states] |
| 前置 | 技能 136（幻鬼 : 贯穿 ghostpierce）Lv1 | 同上 [pre required skill] |
| static data | `100 0 130 190 0`（5 值，模板对位全解码见下） | 同上 + level property |
| 一句话效果 | 聚集魂魄之力下劈前方敌人造成巨大物理伤害；在[鬼步]准备姿势下按技能键发动特殊功能（鬼步终结动作变更为魂破斩终结动作，对命中敌人额外适用魂破斩攻击力） | 同上 [explain] |

**level info（1 列，Lv1 → Lv60）**：col0 下劈攻击力 6213% → 约 28800%+（每级 +630 步进）。

**level property 模板解码**（L21 法，6 占位符 ↔ 6 向量，**全对位、零推断**）：

| 占位符 | 向量 | 语义 | 值 |
|---|---|---|---|
| 下劈攻击力 % | (-1,0,1.0) | level col0 | 6213% → |
| 剑气大小 % | (0,0,1.0) | static[0] | 100（全级固定） |
| 追加下劈 [0 关 1 开] | (1,1,1.0) | static[1] | **0（关）** |
| 追加下劈剑气大小 % | (2,2,1.0) | static[2] | 130（备用） |
| 追加下劈速度 | (3,3,1.0) | static[3] | 190（×2.9 播速备用） |
| [鬼步]和剑术技互断 [0 关 1 开] | (4,4,1.0) | static[4] | **0（关）** |

即本 pvf 数据把"追加下劈"与"强制互断"两开关都配为关——sub4 与取消联动在本 pvf 不触发（§2.2）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
157: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/whiteghostslash/whiteghostslash.nut", "whiteghostslash", STATE_WHITEGHOSTSLASH, SKILL_WHITEGHOSTSLASH);
```

- `swordman_header.nut`：`STATE_WHITEGHOSTSLASH <- 119`（46 行）、`SKILL_WHITEGHOSTSLASH <- 71`（156 行）、`CUSTOM_ANI_WHITEGHOSTSLASH1 <- 283` / `2 <- 284` / `CONTACT <- 285`（454-456 行）、`CUSTOM_ATTACK_INFO_WHITEGHOSTSLASH <- 154`（545 行）。
- .chr 0 基实测对位：etc motion #283/#284/#285 = `whiteghostslash1/2/_contact.ani`（行 1256-1258）；etc attack info #154 = `AttackInfo/whiteghostslash.atk`（行 1448）；另有 #121 = `whiteghostslashattack.atk`（行 1415，引擎接线未考证）。
- 判定体 = **F5 共享 PO 24349**，写包首 dword：**47**（正常下劈）/ **41**（鬼步接续版）/ **48**（接续后 ×2 大斩）/ **49**（追加下劈，本 pvf 不触发）。

### 2.2 主 nut 逐回调（whiteghostslash.nut，274 行全读——五子状态状态机）

**checkExecutableSkill**：`sq_IsUseSkill` → 子状态 0 进状态 119（无状态过滤——实际由引擎门禁 executable states）。

**onSetState（按 subState）**——速度统一按 BLADESPIRIT(123) 鬼人化列 0/1 提速（SpeedRate/SpeedRateEx）：

```
sub0（起手）: 播 whiteghostslash1.ani（320ms 举剑聚魂）；.als 挂 WhiteGhostSlashCast_00@F0
sub1（下劈）: 播 whiteghostslash2.ani（510ms）；创建 PO 24349 dword 47 @自身位置
             （=幻鬼下劈判定体，动画 #54=whiteghostslash_attack.ani、
               攻击信息=父角色 CUSTOM_ATTACK_INFO 154=whiteghostslash.atk、伤害=col0、尺寸 static[0]）
sub2（鬼步接续·突进段，入口见下）: 播 whiteghostslash_contact.ani（360ms）；
             xDistance = 面向 × 鬼步(126) static[0]=400px；创建 PO dword 41 @x+300
sub3（接续·下劈）: 播 whiteghostslash2.ani ×SpeedRateEx；创建 PO dword 48（同 47 但尺寸 ×2.0）
sub4（追加下劈）: 播 whiteghostslash2.ani ×(1+static[3]=2.9)；创建 PO dword 49（尺寸 static[2]=130%）
```

**onKeyFrameFlag（sub1/3/4 各自的 flag 10001，即 whiteghostslash2 的 F0）**：
```
播音效 WHITEGHOST_SLASH + 屏震 12/180
叠 3 层特效（尺寸 ×sizeRate，sub1/3 用 static[0]、sub4 用 static[2]）：
  attackdustfront_08.ani（Hon05.img）、attackfloor_01.ani（Hon07.img）、whiteghostslashattack_06.ani（Hon03.img）
```

**onProc（sub2）**：`sq_GetUniformVelocity(x, xDistance, t, 500)` → **500ms 匀速突进 400px**（`sq_MoveToNearMovablePos` 带撞墙绕行）。

**onProcCon（sub1/3/4）**：`whiteGhostSlshSwordContact(obj)`（jg_swordman_common.nut:871）——static[4]>0 时开启与鬼步/疾影斩/幻鬼贯穿/幻鬼断头台/幻鬼剑舞的互相强制中断（**本 pvf=0，全关**）。

**onEndCurrentAni（段链自动推进）**：
```
sub0 → sub1（自动，无需按键）
sub1 → static[1]>0 ? sub4 : 回站立（本 pvf=0 → 回站立）
sub2 → sub3（自动）
sub3 → static[1]>0 ? sub4 : 回站立
sub4 → 回站立
```

**sub2 的入口（特殊功能，实证）**：`JG_SwordMan\jg_swordman_common.nut:737 SpiritMoveContact(obj)`——鬼步(126)的接触/准备窗口钩子里：
```
GhostSwordSetState(obj, SKILL_WHITEGHOSTSLASH, [2], STATE_WHITEGHOSTSLASH)   // 按魂破斩键 → 直接进 sub2
```
即"在[鬼步]准备姿势下按技能键"= 鬼步状态内按键 → 以子状态 2 进入魂破斩（突进+下劈链），角色无常规起手。

### 2.3 幻鬼被动对象（共享 PO 24349，dword 47/41/48/49）

| dword | 动画（.obj etc motion 0 基） | 攻击信息 | 伤害 | 尺寸/速度 | 消亡 |
|---|---|---|---|---|---|
| **47** | **#54** = `../../../character/swordman/effect/animation/whiteghostslash/whiteghostslash_attack.ani`（9 帧 510ms，**F0-F4 攻击盒 x∈[-60,350] y∈[-40,80] z∈[0,280]**，F0=flag 10001） | **父角色侧** `sq_GetCustomAttackInfo(parentChr, 154)` = whiteghostslash.atk（不走 PO 表，无错位问题） | col0 | static[0]=100% ×SpeedRate | 播完即毁 |
| **41** | **#50** = `../../../character/swordman/effect/animation/spiritmove/spiritmovedasheffect_00.ani`（借鬼步特效） | **PO 侧** code 30 = `attackInfo/spiritmove.atk`（波动族段，无错位） | **鬼步(126) col0**（跨技能数据） | 鬼步 static[1]=120%；**多段**：`setTimeEvent(delay/HitCount)` + `sq_SetMaxHitCounterPerObject(鬼步 static[2]=3)`——**3 段命中** | 播完即毁 |
| **48** | #54 同上 | 同 47 | col0 | **size 2.0 × static[0]**（双倍剑气）×SpeedRateEx | 播完即毁 |
| **49** | #54 同上 | 同 47 | col0 | static[2]=130% × 播速 2.9 | 播完即毁 |

`onattack.nut` case 41/47/48/49：`GhostSword_Attack_Effect`（命中随机斜闪，纯表现）。

**whiteghostslash.atk 实测**（父角色侧 #154，d47/48/49 共用）：physic/weapon、**down 击倒**、push 100 / **lift 108**、cut、blood 100-4.0、**bounce 1 / ignore weight 1**、hit direction front、hit wav R_SWD_HIT。
**whiteghostslashattack.atk**（#121，参数与上几乎全同、[hit info]=etc）——与 #227 `whiteghostslashattack_body.ani` 配对（`CUSTOM_ANI_SWORD_GHOST_15_HUN_PO_ATTACK=227`，JG 剑鬼引擎侧变体，本 pvf 玩家路径不消费）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\whiteghostslash1.ani`（.chr #283，起手） | 4 | 320ms（60/160/60/40） | 无 | 无 | 举剑聚魂；.als 挂 Cast_00@F0 |
| `whiteghostslash2.ani`（#284，下劈） | 9 | 510ms（80+40×7+150） | **F0=10001** | 无 | 角色无盒——判定在 PO #54 |
| `whiteghostslash_contact.ani`（#285，鬼步接续突进） | 6 | 360ms（60×6） | 无 | 无 | **含 [SPECTRUM] 节**（武器残影，R1-A1 上挑首见同款，翻译工具缺口在案）；突进动画 360ms < 突进时长 500ms（onProc 按时间推进） |
| PO #54 `whiteghostslash_attack.ani`（幻鬼下劈判定+视觉） | 9 | 510ms | F0=10001 | **F0-F4**（x∈[-60,350] y∈[-40,80] z∈[0,280]） | 引 `BladeSpiritDot/VengeanceSpirit.img`（幻鬼身体） |
| 特效 `attackdustfront_08.ani` / `attackfloor_01.ani` / `whiteghostslashattack_06.ani` | — | — | 无 | 无 | flag 10001 三层（Hon05/Hon07/Hon03.img） |

`.als`：whiteghostslash1.ani.als（Cast_00@F0）+ attackdustfront_08/attackfloor_01/whiteghostslashattack_06 三个特效 .als（全 [use animation]+[none effect add] 已支持）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | whiteghostslash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\whiteghostslash.skl` | ✅ 实测 | 1 列 + static 5 值（全解码） |
| 注册行 | swordman_load_state.nut:157 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 119 / 技能 71 |
| 常量 | swordman_header.nut:46/156/454-456/545 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | STATE/SKILL/ANI×3/ATTACK_INFO |
| 主 nut | whiteghostslash.nut | `…\pvf\sqr\character\swordman\5_ghostsword\whiteghostslash\whiteghostslash.nut` | ✅ 实测（274 行全读） | 五子状态机 |
| 共享 PO 回调 | setcustomdata/onendcurrentani/onattack 的 case 41/47/48/49 | `…\pvf\sqr\shared_passive_object\swordman\*.nut` | ✅ 实测 | 四型幻鬼斩击体 |
| 共享 PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ 实测 | etc motion #50（行 72）/#54（行 76）；etc attack info #30（行 149，spiritmove.atk） |
| 鬼步接续入口 | jg_swordman_common.nut:737 SpiritMoveContact | `…\pvf\sqr\character\JG_SwordMan\jg_swordman_common.nut` | ✅ 实测 | sub2 进入点 + 取消联动开关 |
| 跨技能数据 | spiritmove.skl（鬼步 126）static `400 120 3 300` | `…\pvf\skill\Swordman\ghostsword\spiritmove.skl` | ✅ 实测 | sub2 突进 400px / dword41 尺寸 120% / 3 段命中 |
| .chr 条目 | etc motion #283/284/285 + #226/227（JG 变体）+ etc attack info #121/#154 | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 动画与 atk 槽位全对位 |
| 玩家 .ani | whiteghostslash1/2/_contact.ani（+cast/attack_body JG 变体×2 + .als×2） | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 帧表 |
| 玩家 .atk | whiteghostslash.atk / whiteghostslashattack.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | #154 主用 / #121 JG 变体 |
| PO/特效 .ani | whiteghostslash_attack + attackdustfront_00-12 + attackfloor_00-02 + whiteghostslashattack_00-08 + cast_00（34 个） | `…\pvf\character\swordman\effect\animation\whiteghostslash\` | ✅ 实测（ls） | 幻鬼斩击与多层特效 |
| 借用 PO .ani | spiritmovedasheffect_00.ani | `…\pvf\character\swordman\effect\animation\spiritmove\` | ✅ 存在 | dword 41 视觉（鬼步资产） |
| 装备层 | whiteghost* ×380 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ 实测（存在性） | avatar 变体图层 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 玩家三段动画 | 必需（共享） | ✅ |
| `Character/Swordman/Effect/BladeSpiritDot/VengeanceSpirit.img` | sprite_character_swordman_effect_bladespiritdot.NPK | **幻鬼下劈判定体视觉**（whiteghostslash_attack.ani 唯一图源） | **必需** | ❌ |
| `Character/Swordman/Effect/WhiteGhostSlash/Hon05.img` | sprite_character_swordman_effect_whiteghostslash.NPK | 下劈正面尘（attackdustfront_08） | 必需 | ❌ |
| `…/WhiteGhostSlash/Hon07.img` | 同上 | 地面层（attackfloor_01） | 必需 | ❌ |
| `…/WhiteGhostSlash/Hon03.img` | 同上 | 剑气主层（whiteghostslashattack_06） | 必需 | ❌ |
| `…/WhiteGhostSlash/Dodge.img`、`Hon00-02/04/06/08-10.img`、`GhostPierce/DustADodge.img`、`Common/CommonEffect/Dust/Dust01.img`、`Glow/Circle.img` | 各自路径 NPK | 其余特效层/JG 变体层 | 可选 | ❌ |
| `Effect/SpiritMove/*`（dword 41 借用层） | sprite_character_swordman_effect_spiritmove.NPK | 鬼步接续版视觉（特殊功能砍掉则不需要） | 可跳过 | ❌ |

**缺失 img：必需 4 张（2 个 NPK）、可选 10+ 张。**

## 5. 实现方案草案（号段：SkillIds 22 / AnimIds 88-91 / AreaIds 13，A11 批内顺延）

### 内容件清单（正常施放主干 sub0→sub1；特殊功能链见 §7 简化）

1. **`DotNet~/Skills/WhiteGhostSlashSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发 + 008 段链范式）
   - `CooldownMs = 12000`；`TotalTimeMs = 830`（起手 320 + 下劈 510）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanWhiteGhostSlash1)`（起手）+ SubState=0。
   - `OnUpdate` t≥320 且 SubState==0：`ctx.PlayAnim(AnimId.SwordmanWhiteGhostSlash2)` + `ctx.ClearHitTargets()` + `ctx.CreateAreaInFront(AreaIds.WhiteGhostSlash, 1.45)` + SubState=1（**t=320 即下劈动画 F0=PO 盒首帧，时序与 DNF 完全一致**——PO 在 sub1 进入瞬间创建、其动画 F0 自带盒）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`（static[1]=0 → 无追加下劈，直落站立）。
2. **`DotNet~/Areas/WhiteGhostSlashArea.cs : AreaDefinition`**（BloodBoomArea 一次性范式）
   - `TotalTimeMs = 260`（盒窗口 F0-F4 ≈ 5×40-60ms + 余量）、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (2.05, 0.6, 1.4)`（PO 盒 x∈[-60,350] y∈[-40,80] z∈[0,280] 折算：宽 4.1/高 1.2/深 2.8 单位；**AreaDefinition 实测无 SpawnOffset 字段**——盒中心前移 145px≈1.45 单位已并入 `CreateAreaInFront` 距离）；
   - `HitReaction { Damage = 180, HitstunMs = 700, KnockbackX = 100, LaunchY = 108 }`（whiteghostslash.atk 原值 down/push100/lift108；damage=col0 6213% demo 折算；bounce/ignore weight 为 .atk 表现字段，HitReaction 外——R2-A8 已记档随 atk 子命��一并建模）；
   - `ViewAnimId = AnimId.WhiteGhostSlashAttack`（whiteghostslash_attack 译件，VengeanceSpirit 幻鬼下劈视觉）；可选 `ViewBackAnimId` 挂 attackfloor_01 地面层。
3. **无需新 Action/Buff**（MeleeHit 现成；无异常状态）。
4. 特殊功能（sub2 鬼步接续突进 + sub3 双倍斩 + sub4 追加下劈）与取消联动（whiteGhostSlshSwordContact）：**不做**——依赖技能取消体系 + 跨技能数据读（鬼步 static/level），见 §7。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 119 sub0→sub1 自动段链 | OnUpdate 时间轴 + PlayAnim 切换（SubState 守卫） |
| PO d47（幻鬼下劈判定，父角色 atk #154） | `WhiteGhostSlashArea`（HitReaction 直译） |
| sub1 flag 10001 三层特效 + 屏震 | Area overlay 手组装（releasewave 先例）+ 屏震跳过 |
| static[1]/[4] 开关（追加下劈/互断） | 本 pvf 数据=0 → 不实现；框架侧留 bool 位后补 |
| sub2 鬼步接续（SpiritMoveContact 入口） | **技能取消体系缺失** → 不做（064/R1-A3 已录） |
| dword 41 跨技能读鬼步数据（伤害/尺寸/3 段命中） | 无跨技能 level data 查询 → 随 sub2 一并砍 |
| BLADESPIRIT 提速 | 固定 1.0（精通缺失） |
| [SPECTRUM] 武器残影（contact.ani） | 翻译缺口在案（R1-A1）→ 特殊功能不做则不涉及 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `…\Runtime\SkillIdAttribute.cs` | `SkillIds.WhiteGhostSlash = 22` + ButtonToSkill 新键 |
| AreaId | `…\Runtime\AreaDefinition.cs` | `WhiteGhostSlash = 13` |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanWhiteGhostSlash1 = 88`、`SwordmanWhiteGhostSlash2 = 89`、`WhiteGhostSlashAttack = 90`、`WhiteGhostSlashFx = 91`（whiteghostslashattack_06，可选） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | 玩家 2 + PO 1 个 json；图集 4 张必需 |
| 按键 | LSOperaComponentSystem | 新键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 12000 ms | 12000（直用） |
| 起手 | 320ms（whiteghostslash1） | 直用 |
| 下劈 | 510ms（whiteghostslash2，F0 即判定） | 直用；PO 区 260ms 活跃窗 |
| 伤害 | col0 6213% → 约 28800% | 180（固定） |
| 命中反应 | down / push100 / lift108 / bounce1 / ignore weight1 / blood100 | Hitstun 700 / Kb 100 / Ly 108 |
| 攻击盒 | PO F0-F4 x∈[-60,350] y∈[-40,80] z∈[0,280] | HalfExtents (2.05,0.6,1.4) + 前偏 1.45 |
| （特殊功能）突进 | 鬼步 static[0]=400px / 500ms 匀速 | 不做 |
| （特殊功能）d41 | 鬼步伤害 / 120% / 3 段命中 | 不做 |
| （特殊功能）d48/d49 | col0 / ×2.0 尺寸 / 130% 尺寸 ×2.9 速 | 不做（static[1]=0） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| whiteghostslash.skl | `.skl` 无子命令 | 手抄（1 列 + static 5 值，模板全解码已给） |
| whiteghostslash.atk / whiteghostslashattack.atk + PO spiritmove.atk（d41 用） | `.atk` 无子命令；**[bounce]/[ignore weight]** 为 HitReaction 外字段 | 手抄；bounce/ignore weight 并入 R2-A8 已记档的 atk 子命令字段设计输入 |
| `whiteghostslash_contact.ani` | **[SPECTRUM]** 节（武器残影配置） | 既有缺口（R1-A1 上挑首见）；特殊功能不做则本文件可整体跳过 |
| swordman_shared.obj | `.obj` 无子命令 | 手工对位（#50/#54/#30 已给） |
| 其余 .ani（whiteghostslash1/2、whiteghostslash_attack、特效系） | 常规节 | **现有 ani 子命令全覆盖** |
| 全部 .als | [use animation]+[none effect add]（已支持） | 无缺口 |

本技能翻译缺口：`.skl`/`.atk`/`.obj` 三类既有 + [SPECTRUM] 既有（计 4 条，无新增）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| [鬼步]准备姿势按技能键 → sub2 突进+sub3 双倍斩+sub4 追加下劈链 | **技能取消体系**（缺失，064/R1-A3 记档）+ **跨技能 level/static data 查询门面**（新记 §8）+ **同段定时多段**（d41 3 段——L19 三档中 Bullet 需字段落） | 只做正常施放（sub0→sub1 下劈）；特殊功能整链不做（本 pvf static[1]/[4]=0，追加下劈与互断本就关闭） |
| d48 双倍尺寸 / d49 2.9 倍速 | IMAGE RATE 缩放与播速缩放（延后） | 随特殊功能一并不做 |
| whiteGhostSlshSwordContact 互断开关 | 同技能取消体系 | 不做（本 pvf=0） |
| bounce/ignore weight 命中表现 | HitReaction 外字段（R2-A8 记档） | 跳过（击倒反应已由 down/lift108 表达） |
| sub1 flag 10001 三层特效 + 屏震 12/180 | 无脚本驱动 overlay + 屏震（延后） | Area overlay 手组装 1-2 层；屏震跳过 |
| BLADESPIRIT 提速 / static[2]/[3] 缩放 | 等级缩放（延后） | 固定 1.0/100% |
| 攻击盒 static[0] 缩放 | 对象整体缩放（延后） | 固定 100% |
| 音效 WHITEGHOST_SLASH / R_SWD_HIT | 延后 | 跳过 |
| state 8/14 中施放 | 技能取消体系 | 仅站立 |

## 8. 存疑与缺口上报

**未考证项**
1. `whiteghostslashattack.atk`（#121）与 `whiteghostslashattack_body.ani`（#227）、`cast_body`（#226）的接线——`CUSTOM_ANI_SWORD_GHOST_15/16_HUN_PO/HE_JI_YUE_LIN` 常量组（JG 剑鬼引擎侧，白名单无脚本消费者），推断为剑鬼 NPC 版引擎变体，非本 pvf 玩家路径。
2. sub2 的实际输入窗口边界（SpiritMoveContact 的调用时机归 126-鬼步文档域，本档只记入口）。
3. whiteghostslash2.ani 的 F8 收势帧 150ms 是否为取消窗口（flag 65534 未出现于此动画，推断为固定收势）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **跨技能 level/static data 查询门面**：dword 41 在本技能运行时读**鬼步(126)**的等级数据与 static（伤害/尺寸/多段数）——DNF 的"接续技借用源技数据"模式。我们侧 SkillLogic 拿不到其他技能的配置；若后续做接续类技能（流心系/鬼步系一族），需要 `ctx.GetOtherSkillData(skillId)` 类门面或把借用值硬编码。与"技能取消体系"同域，建议合并立项评估。

**F5 链路补充**：whiteghostslash 的 PO atk 走**父角色侧** `sq_GetCustomAttackInfo(parentChr, 154)`（不是 PO 表）——F5 族内 atk 取数存在两种写法（PO 表 code / 父角色 chr 槽位），后续走读先看 setcustomdata 的取数对象再查表，别默认都走 PO 表。

**给下轮的经验**：ghostsword 族"剑术技接幻鬼技"的入口全在 `JG_SwordMan\jg_swordman_common.nut` 的 `SpiritMoveContact`（鬼步窗口五连 Enable/SetState 表）与各技 onProcCon 的 Contact 函数（static[4] 开关）——做接续类技能走读时先读这张表（737/749/871 行一带），五技（speedslash/ghostpierce/whiteghostslash/ghostdecollation/sworddancebs）的接续子状态号一次拿全。
