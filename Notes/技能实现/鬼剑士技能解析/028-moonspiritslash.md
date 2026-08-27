# 共鸣 : 鬼灵斩（moonspiritslash）

> 技能ID 28 | 级别 A（维持预判） | 可实现性 ✅（直接；幻鬼位置接力简化为固定身后、运行时 atk 错位采用意图值） | 分析日期 2026-08-22 | 批次 A11

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 共鸣 : 鬼灵斩 | `skill\Swordman\ghostsword\moonspiritslash.skl [name]` |
| 英文名 | moonspiritslash（取 skl 文件名，全小写；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype]=5，L17；ghostsword 族） | 同上 |
| 学习等级 | 35（[required level range] 2） | 同上 [required level] |
| 最高等级 | 60（growtype0/5 段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1）/ 物理 | 同上 [type] / [weapon effect type] |
| 指令 | →←↓→ + Z（[skill command advantage] 20/40） | 同上 [command] |
| CD | 15000 ms | 同上 [cool time] |
| MP | 129 → 1083 | 同上 [consume MP] |
| 特殊消耗 | 消耗道具 3037×1 | 同上 [consume item] |
| 可施放状态 | 0（站立）/ 8（攻击中）/ 14（疑幻鬼共存态，066 同款未考证） | 同上 [executable states] |
| 前置 | 技能 66（共鸣 : 离魂一闪 spiritcrossslash）Lv1 | 同上 [pre required skill] |
| static data | `100`（单值=**斩击范围 %**，模板对位实证） | 同上 + level property |
| 一句话效果 | 剑影与幻鬼一起挥剑，对大范围内敌人造成巨大物理伤害；分离态施放时幻鬼中断当前动作在原地立即施放鬼灵斩 | 同上 [explain] |

**level info（1 列，Lv1 → Lv60）**：col0 鬼灵斩攻击力 6957% → 30357%+（每级 +706 步进）。

**level property 模板解码**（L21 法，2 占位符 ↔ 2 向量）：
- 鬼灵斩攻击力 <int>%% → `(-1,0,1.0)` = level col0；
- 斩击范围 <int>%% → `(0,0,1.0)` = static[0] = **100**（全等级固定 100%，不随级缩放）；
- 模板中间行"[幻鬼斩击攻击力与剑影相同]"为固定文本（无占位符）——剑影与幻鬼两侧伤害同源。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
169: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/moonspiritslash/moonspiritslash.nut", "moonspiritslash", STATE_MOONSPIRITSLASH, SKILL_MOONSPIRITSLASH);
17:  IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);   // F5 共享 PO
```

- `swordman_header.nut`：`STATE_MOONSPIRITSLASH <- 120`（57 行）、`SKILL_MOONSPIRITSLASH <- 28`（157 行）、`CUSTOM_ANI_MOONSPIRITSLASH <- 303`（474 行）。
- .chr etc motion 0 基实测：**303 = `Animation/moonspiritslash.ani`（行 1276）**——对位吻合。
- 两侧判定体 = **F5 unclebang 共享 PO 24349**，写包首 dword：**69**（剑影侧斩击）/ **70**（幻鬼侧斩击）。

### 2.2 主 nut 逐回调（moonspiritslash.nut，129 行全读）

**checkExecutableSkill_moonspiritslash**：state ∈ {0, 8, 14} 才放行（攻击中可接技）→ 子状态 0 进状态 120。

**onSetState_moonspiritslash（仅 subState 0，单段技能）**：
```
SpeedRate = BLADESPIRIT(123) 列0 + 1（鬼人化提速）
播 CUSTOM_ANI_MOONSPIRITSLASH（moonspiritslash.ani，20 帧 1200ms）
创建 PO 24349 写包 dword 69（在自身位置）                    // 剑影侧斩击体
VSObject = getVSObject(obj)                                  // 在场幻鬼查询（id∈{61,62,63,65,66,68,70,74}）
若有幻鬼：  PO 24349 dword 70 @ 幻鬼位置 + 销毁旧幻鬼          // 幻鬼在原地立即斩（分离态分支）
若无幻鬼：  PO 24349 dword 70 @ 自身 x-100（身后 1 单位）      // 合体态：幻鬼在身后出现
```

**onKeyFrameFlag_moonspiritslash（sub0 / flag 10001，F8=480ms）**：
```
als_ani 叠加特效 moonspiritslashaeffect_00.ani @ z=-105，尺寸 100×(static[0]/100)
```

**onEndCurrentAni_moonspiritslash**：回 STATE_STAND。

### 2.3 幻鬼被动对象（共享 PO 24349，dword 69/70，六回调逐文件走读）

**dword 69（剑影侧斩击）**——`setcustomdata.nut` case 69：
- 播共享 .obj 自定义动画 **#85**（= `../../../character/swordman/effect/animation/moonspiritslash/moonspiritslash_attack.ani`——借角色特效目录，20 帧 1200ms、**F8-F11 攻击盒 x∈[-314,644] y∈[-100,200] z∈[-8,175]**，前后横跨 9.6 单位的巨幅斩击盒）；
- 攻击信息 `sq_GetCustomAttackInfo(obj, 41)`——**F5 R2-A10 修正案再实证**：PO 侧 [etc attack info] 0 基（行号-119）**#41 = windspiritvs.atk（运行时实际加载）**，意图位 **#43 = moonspiritslash.atk（code+2 还原）**，两份实测：
  | atk | 关键参数 |
  |---|---|
  | moonspiritslash.atk（意图，采用） | physic/weapon、**down** 反应、push 200 / lift 150、blood 70 |
  | windspiritvs.atk（运行时实际） | physic/weapon、damage 反应、push 200 / lift 150、vs cut、blood 70、knuckback 3-60、**force hitstun 1000** |
- 伤害 = 父角色 `sq_GetBonusRateWithPassive(28, -1, 0, 1.0)` = **level col0**；攻击盒/尺寸按 static[0]=100% 缩放（`sq_SetAttackBoundingBoxSizeRate`）；
- `onendcurrentani.nut` case 69：播完即毁（单相位）。

**dword 70（幻鬼侧斩击，双相位）**——`setcustomdata.nut` case 70 只 `sendStateOnlyPacket(10)`，参数在 `setstate.nut` case 70 现读：
- **state 10（鬼灵斩）**：播自定义动画 **#86**（= `animation/moonspiritslash/moonspiritslashbbody_body.ani`，15 帧 1305ms、**F8-F11 攻击盒 x∈[-297,608] y∈[-100,200] z∈[-27,198]**）；攻击信息同 code 41、伤害同 col0、尺寸同 static[0]；
  - .als 边车 4 层：BEffect02_00@F5/层-1、BEffect01_04@F8(10001)、BEffect01_03@F8(10002)、BEffect01_02@F3(10003)；
  - `onkeyframeflag.nut` case 70/state10/flag 10001（F8）：再叠 `moonspiritslashbeffect01_01.ani`；
  - `onendcurrentani.nut`：state 10 播完 → state 11；
- **state 11（消散）**：播 **#87**（= `moonspiritslashbbody_finish.ani`，19 帧 2720ms，F11=flag 10001 → 叠 disappearback/front 双消散层）；播完销毁 PO。
- **幻鬼锚点**：dword 70 在 getVSObject 的在场幻鬼 id 集内——施放后幻鬼停留在斩击位直至消散（后续 VS 技可接力，R2-A6 幻鬼实体记忆系统依赖）。

**onattack.nut** case 69/70：`GhostSword_Attack_Effect`（命中目标身上随机播放 common/hiteffect 斜闪——纯表现，无异常状态）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\moonspiritslash.ani`（.chr #303，玩家） | 20 | 1200ms（60×20） | **F8=10001** | 无 | 挥剑 pose；仅引 sm_body 图集 |
| PO #85 `effect\animation\moonspiritslash\moonspiritslash_attack.ani`（剑影侧） | 20 | 1200ms | F8=10001 | **F8-F11**（x∈[-314,644]） | 引 `BladeSpiritDot/VengeanceSpirit.img` |
| PO #86 `…unclebang…\moonspiritslash\moonspiritslashbbody_body.ani`（幻鬼侧） | 15 | 1305ms | F8=10001 | **F8-F11**（x∈[-297,608]） | 同上图集；.als 4 层 |
| PO #87 `moonspiritslashbbody_finish.ani` | 19 | 2720ms | F11=10001 | 无 | 消散段（F10 前后多长停帧） |
| 特效 `moonspiritslashaeffect_00.ani`（F8 挂接） | 24 | 1110ms | 无 | 无 | 月灵光柱（BladeSpirit/000.img） |
| 特效 `moonspiritslashbeffect01_01.ani` | 未逐帧 | — | 无 | 无 | 幻鬼斩光（MoonSpiritSlash03.img） |
| 特效 `disappearback/front.ani` | 19×2 | 665ms | 无 | 无 | 消散双面（TeleportVS/Normal.img 等） |

`.als`：玩家 .ani 无边车；PO 侧 bbody_body.ani.als（[use animation]+[none effect add] 4 层，全支持）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | moonspiritslash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\moonspiritslash.skl` | ✅ 实测 | 1 列 + static 100 |
| 注册行 | swordman_load_state.nut:169 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 120 / 技能 28 |
| 常量 | swordman_header.nut:57/157/474 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | STATE/SKILL/ANI |
| 主 nut | moonspiritslash.nut | `…\pvf\sqr\character\swordman\5_ghostsword\moonspiritslash\moonspiritslash.nut` | ✅ 实测（129 行全读） | 单段施法 + 双 PO 创建 + 幻鬼锚点 |
| 共享 PO 回调 | setcustomdata/setstate/onkeyframeflag/onendcurrentani/onattack 的 case 69/70 | `…\pvf\sqr\shared_passive_object\swordman\*.nut` | ✅ 实测 | 两侧斩击全逻辑 |
| 共享 PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ 实测 | etc motion #85/86/87（行 107-109）；etc attack info #41（行 159）/#43（行 161）——**atk 错位 -2 再实证** |
| PO .atk | moonspiritslash.atk（意图）/ windspiritvs.atk（运行时） | `…\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\` | ✅ 实测 | §2.3 双份参数 |
| .chr 条目 | etc motion #303（行 1276）+ etc attack info #125/#126（行 1419/1420） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 玩家动画 + moonspiritslashfar/near.atk（引擎接线未考证，066 同款） |
| 玩家 .ani | moonspiritslash.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 1200ms |
| 玩家 .atk | moonspiritslashfar.atk / moonspiritslashnear.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 双份 down/push200/lift180（far 与 near 仅 blood 倍率差） |
| PO/特效 .ani | moonspiritslash_attack + bbody_body/finish + beffect01_01 + disappearback/front（+族内 aeffect×8、cspirit 系未消费） | 两侧 animation 目录 | ✅ 实测 | §2.4 |
| .als | moonspiritslashbbody_body.ani.als 等 3 个（unclebang 目录） | `…\passiveobject\unclebang_shared_passive_object\swordman\animation\moonspiritslash\` | ✅ 实测 | 全部已支持节型 |
| 幻鬼记忆 | jg_swordman_common.nut:892 getVSObject | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅ 实测 | id 70 在场集 |
| 装备层 | moonspirit* ×228 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ 实测（存在性） | avatar 变体图层 |
| 关联强化 | moonspiritslashex.skl（feature skill index 106） | `…\pvf\skill\Swordman\ghostsword\moonspiritslashex.skl` | ✅ 存在 | E 批另行分析 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 玩家挥剑动画 | 必需（共享） | ✅ |
| `Character/Swordman/Effect/BladeSpiritDot/VengeanceSpirit.img` | sprite_character_swordman_effect_bladespiritdot.NPK | **幻鬼/斩击身体**（#85/#86/#87 三动画全用此图） | **必需** | ❌ |
| `Character/Swordman/Effect/MoonSpiritSlash/MoonSpiritSlash03.img`（beffect01_01 用） | sprite_character_swordman_effect_moonspiritslash.NPK | 幻鬼斩光 | 必需 | ❌ |
| `MoonSpiritSlash01/02/04/05.img`（bbody .als 的 BEffect01_02/03/04、BEffect02_00 系） | 同上 | .als 挂接的斩光组 | 可选（.als 层后补） | ❌ |
| `Character/Swordman/Effect/BladeSpirit/000.img` | sprite_character_swordman_effect_bladespirit.NPK | F8 月灵光柱（aeffect_00） | 可选 | ❌ |
| `Character/Swordman/Effect/TeleportVS/Normal.img`（+LDodge.img） | sprite_character_swordman_effect_teleportvs.NPK | 消散双面 | 可选 | ❌ |
| `MoonSpiritSlash01-05` 变体 `_Dodge.img`、Common/Dust01/02 | 各自路径 NPK | 族内备用层 | 可跳过 | ❌ |

**缺失 img：必需 2 张（VengeanceSpirit + MoonSpiritSlash03，分属 2 个 NPK）、可选 6 张。**

## 5. 实现方案草案（号段：SkillIds 21 / AnimIds 84-87 / AreaIds 11-12，A11 批内顺延）

### 内容件清单

1. **`DotNet~/Skills/MoonSpiritSlashSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发范式）
   - `CooldownMs = 15000`；`TotalTimeMs = 1250`（玩家动画 1200ms）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanMoonSpiritSlash)` + `ctx.ClearHitTargets()`。
   - `OnUpdate` t≥480（F8=10001 同时刻）且 SubState==0：**双区同帧创建**（剑影+幻鬼两侧同时挥剑）：
     - `ctx.CreateAreaInFront(AreaIds.MoonSpiritSlashFront, 1.65)`——剑影侧大斩区（165px=盒中心前移量）；
     - `ctx.CreateAreaInFront(AreaIds.MoonSpiritSlashPhantom, -1.0)`——幻鬼侧（**负距离=身后**，签名 `CreateAreaInFront(int areaId, FP distance)` 沿朝向带符号偏移；无幻鬼锚点门面，简化固定身后——分离态"幻鬼原地斩"变体不做，§7）；
     - `ctx.SetSubState(1)`；同时叠 F8 光柱特效（overlay 或跳过）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。伤害全在 Area（DNF 侧结算方就是 PO 24349）。
2. **`DotNet~/Areas/MoonSpiritSlashFrontArea.cs : AreaDefinition`**（BloodBoomArea 一次性范式；实测字段面：TotalTimeMs/TickTimeMs/HalfExtents/EnterActions/TickActions/HitReaction/ViewAnimId/ViewEndAnimId/ViewBackAnimId——**Area 无 SpawnOffset 字段**（那是 BulletDefinition 的），中心偏移并入 `CreateAreaInFront` 距离）
   - `TotalTimeMs = 720`（盒窗口 F8-F11 + 收势）、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (4.8, 1.5, 0.9)`（#85 盒 x∈[-314,644] y∈[-100,200] z∈[-8,175] 折算：宽 9.58/高 3/深 1.83 单位；盒中心前移 165px≈1.65 单位已并入 §5 的 CreateAreaInFront 距离）；
   - `HitReaction { Damage = 200, HitstunMs = 600, KnockbackX = 200, LaunchY = 150 }`（意图 atk moonspiritslash.atk 原值 down/push200/lift150；damage=col0 6957% demo 折算）；
   - `ViewAnimId = AnimId.MoonSpiritSlashAttack`（moonspiritslash_attack 1200ms 译件）。
3. **`DotNet~/Areas/MoonSpiritSlashPhantomArea.cs : AreaDefinition`**（同构第二区）
   - 同参数（伤害同源 col0——模板"幻鬼斩击攻击力与剑影相同"）；`HalfExtents = (4.5, 1.5, 1.1)`（#86 盒折算，中心前移 1.55 并入距离）；
   - `ViewAnimId = AnimId.MoonSpiritPhantomBody`（bbody_body）；`ViewEndAnimId = AnimId.MoonSpiritPhantomFinish`（bbody_finish 消散——Area 有收尾通道，135 号文档 Bullet 无通道的对照项）。
   - 两区重叠处敌人**吃双份伤害**（DNF 同构：剑影+幻鬼两刀）。
4. **无新增 Action/Buff**（MeleeHit 现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 120 单段 + 双 PO 创建 | `MoonSpiritSlashSkill` OnUpdate 帧触发双 Area |
| PO d69（剑影侧巨盒+col0 伤害） | `MoonSpiritSlashFrontArea`（意图 atk 参数） |
| PO d70（幻鬼侧双相位：斩+消散） | `MoonSpiritSlashPhantomArea`（ViewEndAnimId=消散） |
| PO atk code 41（运行时 windspiritvs） | 采用意图值 moonspiritslash.atk（down/push200/lift150）——本 pvf 错位数据不还原 |
| 分离态幻鬼原地斩（getVSObject 接力） | 幻鬼锚点门面缺失 → 固定身后 -1 单位 |
| BLADESPIRIT(123) 提速 | 精通系统缺失 → 固定 1.0 |
| 斩击范围 static[0]=100% | 固定 100%（不缩放） |
| state 8/14 中施放 | 技能取消体系缺失 → 仅站立 |
| F8 als 光柱 | 无 nut 驱动 overlay → 手组装或跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `…\Runtime\SkillIdAttribute.cs` | `SkillIds.MoonSpiritSlash = 21` + ButtonToSkill 新键 |
| AreaId | `…\Runtime\AreaDefinition.cs` | `MoonSpiritSlashFront = 11`、`MoonSpiritSlashPhantom = 12` |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanMoonSpiritSlash = 84`、`MoonSpiritSlashAttack = 85`、`MoonSpiritPhantomBody = 86`、`MoonSpiritPhantomFinish = 87` |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | 玩家 1 + PO 2 个 json；图集 2 张必需 |
| 按键 | LSOperaComponentSystem | 新键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 15000 ms | 15000（直用） |
| 玩家动画 | 1200ms（F8=480ms 触发） | 直用 |
| 双区触发 | F8（480ms） | 同帧 |
| 每侧伤害 | col0 6957% → 30357% | 每区 200（重叠双吃） |
| 命中反应 | 意图 atk：down / push200 / lift150 / blood70 | Hitstun 600 / Kb 200 / Ly 150 |
| 攻击盒 | #85 x[-314,644]、#86 x[-297,608] | HalfExtents 见 §5 |
| 幻鬼侧位置 | 在场幻鬼位（锚点）/ 合体态 x-100 | 固定身后 -1.0 单位 |
| 消散段 | bbody_finish 2720ms | ViewEndAnimId 直译 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| moonspiritslash.skl | `.skl` 无子命令 | 单列手抄（既有缺口） |
| PO .atk（moonspiritslash / windspiritvs）+ 玩家 far/near .atk ×2 | `.atk` 无子命令 | 手抄 ~4 份（既有缺口） |
| swordman_shared.obj | `.obj` 无子命令 | 手工对位（本档已给 #85/86/87 + #41/#43 映射） |
| 全部 .ani | 常规节（FRAME/DELAY/IMAGE/ATTACK BOX），无规则外节 | **现有 ani 子命令全覆盖** |
| 全部 .als | [use animation]+[none effect add]（已支持） | 无缺口 |

本技能翻译缺口：`.skl`/`.atk`/`.obj` 三类既有（计 3 条），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 分离态：幻鬼中断当前动作原地斩（getVSObject 接力） | **幻鬼锚点实体轻量版**（缺失，R2-A8 记档，F5 全族受益） | 幻鬼区固定身后 -1 单位（合体态行为）；锚点门面立项后回补 |
| 运行时 atk 错位加载 windspiritvs.atk（force hitstun 1000 砸控手感） | 本 pvf mod 数据问题（F5 修正案） | 采用意图 atk（down/push200/lift150）；如要本 pvf 手感换 windspiritvs 参数 |
| state 8/14 中施放 | 技能取消体系（缺失，R1-A3 记档） | 仅站立可放 |
| BLADESPIRIT(123) 动画提速 | 精通/等级缩放（延后） | 固定 1.0 |
| F8 als_ani 光柱（nut 逐层驱动） | 无脚本驱动 overlay 通道（064 已录） | 手组装 overlay（releasewave 先例）或跳过 |
| blood 70 / vs cut 表现 | 无出血数值通道（.atk 表现层） | 跳过（DNF 本体未挂 ACTIVESTATUS） |
| 攻击盒随 static[0] 缩放 | 对象整体缩放（延后） | 固定 100% |
| 音效 R_DARK_SWORD_HIT | 延后 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. 玩家侧 moonspiritslashfar/near.atk（.chr #125/#126）的接线——脚本未调用、玩家 .ani 无攻击盒（066 spiritcrossslashfirst/second 同款"引擎接线不可见"）。
2. state 14（executable states 第 3 项）具体状态。
3. bbody_finish 2720ms 的超长停帧段（L23 同族"待事件"型帧？未逐帧核对 delay 分布）。
4. 幻鬼消散后是否残留锚点供后续 VS 技接力（getVSObject 集含 70，但本 PO 播完即毁——接力窗口=消散前 2720ms，推断）。

**F5 修正案再实证（供主循环）**：本技能 dword 69/70 的 atk code 41 → 运行时 #41=windspiritvs.atk、意图 #43=moonspiritslash.atk——**错位 -2 规律第三次独立验证**（spiritcrossslash/windspiritvs/moonspiritslash/hellslash 四例齐），修正案无需再修。

**新系统级缺口**：无新增（幻鬼锚点已在 R2-A8 记档；本技能是该缺口的又一用户实证）。

**给下轮的经验**：ghostsword 族 d69/70 型"双体同挥"技能（moonspiritslash 等）——两侧 PO 全读 level col0 同源、atk 同 code；实现侧=同帧双 Area + ViewEndAnimId 消散，比 135（弹体）更简单，是 F5 族里最直译的一档。PO 借角色特效目录动画（#85 路径 `../../../character/...`）是共享 .obj 常态，img 推导要按 .ani 内实际路径走（L14 同型）。
