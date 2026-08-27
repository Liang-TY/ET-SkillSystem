# 冥祭之沼（swordman_tombstoneswamp）

> 技能ID 247 | 级别 B | 可实现性 🔶（吸附沼泽+延时爆炸主干可表达；"再按引爆"二段交互、施放时前后方向键设置、霸体降级） | 分析日期 2026-08-22 | 批次 B6

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冥祭之沼 | `skill\Swordman\swordman_tombstoneswamp.skl [name]` |
| 英文名 | swordman_tombstoneswamp（取 skl 文件名；[name2] 节无） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype] 空档；[second growtype maximum level] 第 5/6 位=30/30 → growtype 2 一觉/二觉档，087-Blache 同判据；墓碑/冥界主题互证） | 同上 |
| 学习等级 | 70（二觉技档） | 同上 [required level] |
| 最高等级 | 40 | 同上 [maximum level] |
| 类型 | active（skill class 3） | 同上 [type] |
| 指令 | →←↑→ + Z（指令 MP 优惠 50%/50% 档） | 同上 [command] / [skill command advantage] |
| CD | 40000 ms | 同上 [cool time] |
| MP | 822 → 1726（Lv1→Lv40） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶体 3037×2 | 同上 [consume item] |
| 可执行状态 | `[executable states] 8 0 14 32 20 42 65 13 33 50 237 238 240 241`——除普攻/站立外，可在 12 个**其他技能状态中**施放（鬼魂释放 131 的取消体系，见 §2.2） | 同上 [executable states] |
| static data | 无 | 同上 |
| 一句话效果 | 在自身周围升起墓碑开启冥界之门，持续吸附周围敌人；一段时间后或再按技能键引发暗属性爆炸 | 同上 [explain] |

**level property（6 列，Lv1 → Lv40，dungeon；向量全为 `-1 <列> <系数>` = 纯 level 列，L21 读法）**：

| 列 | 值域 | 语义（模板行对位） |
|---|---|---|
| col0 | 380（恒） | 吸附敌人的范围 380px |
| col1 | 5000（恒） | 吸附敌人持续时间（系数 0.001 → 5.0 秒） |
| col2 | 2（pvp 1） | 吸附强度 Lv（每帧拉动步长，见 §2.3） |
| col3 | 19997 → 159988（pvp 163→1311） | 爆炸攻击力（万分率 %） |
| col4 | 100（恒） | 前方设置 +100px（施放时按→） |
| col5 | -100（恒） | 后方设置 -100px（施放时按←） |

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 68（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/tombstoneswamp/tombstoneswamp.nut", "swordman_tombstoneswamp", 247, 247);
// swordman_header.nut 行 79/107：STATE_SWORDMAN_TOMBSTONESWAMP <- 247，SKILL_SWORDMAN_TOMBSTONESWAMP <- 247
```

F7 二觉系标准结构（完整 nut 管演出 + 共享 PO **24370** 承担判定，R4-A17 薄壳档）。⚠ 主 nut 被 mod 作者
`compilestring(XOR)` 混淆（C6 形态④），本档已用 python 逐字节解码还原（方法：`chr(a^b)` 连接 + 字面量保序），
还原结果与明文函数（checkExecutableSkill/addObject）风格一致，可信。

**PO 24370 链路**（L20）：`passiveobject.lst:9` → `script_sqr_nut_qq506807329/swordman/qq506807329new_swordman_24370.obj`，
六回调在 `sqr\common_object\share_obj\swordman\*.nut`（按写包首 dword 247 分派）。

### 2.2 主 nut 逐回调（tombstoneswamp.nut，105 行：41 行明文 + 4 段混淆已解码）

- **checkExecutableSkill_swordman_tombstoneswamp**（施放前置钩子，明文）：
  1. 枚举自己的 24370 对象：若存在 var("skill")==247 的沼泽——
     - 其状态 ≠ `PASSIVEOBJ_SUB_STATE_1(=11, header.nut:604 实测)` → **拒绝施放**（起幕/爆炸期不可再按）；
     - 其状态 == 11（吸附循环期）→ `magicBall.getVar().set_vector(1, 0)`（**把 var 槽 1 = 剩余吸附时间清零 → 下一帧 procappend 判超时 → 立即进入爆炸链**）+ 拒绝施放。**这就是"再按技能键引爆"的实现**。
  2. 无在场沼泽时正常施放判定：若当前处于 11 个鬼魂释放取消状态（32/20/42/65/13/33/50/237/238/240/241——237/238/240/241 = SLASHOFBOOM/SLASHOFHELL/BLADEPHANTOMEX/ZIGADVENT，header.nut:69-73 实测）
     → **不进技能状态**，直接 `addObject_swordman_tombstoneswamp(obj, getGhostSoulRelease_Area_Distance(obj,state), y+1, 0)`
     （落点 = 鬼魂释放技能 131 的 static data 按当前状态查出的偏移位置，`passive_skill_swordman.nut:196` 实测）；
     否则 → `sq_AddSetStatePacket(STATE_SWORDMAN_TOMBSTONESWAMP)` 进状态。
- **onSetState**（解码）：`sq_StopMove` → `sq_SetCurrentAnimation(169)`（.chr etc motion #169 = `Animation/TombStoneSwamp_Body.ani`，实测行 1142）→ 播音效 `SM_TOMBSTONE` → `sq_SetStaticSpeedInfo(ATTACK_SPEED…)`（施法动画吃攻速）。
- **onKeyFrameFlag**（解码）：
  - flag **1**（.ani F4）：读 4 列等级数据写包 `[247, col0 吸附范围, col1 吸附时长, col2 吸附强度, sq_GetBonusRateWithPassive(247,247,col3) 爆炸倍率]`；按住←/→ 时偏移取 col5(-100)/col4(+100)；
    `sq_SendCreatePassiveObjectPacket(24370, 0, sq_GetDistancePos(0, direction, offset), 0, 0)`——**沼泽 PO 出生点 = 施法者位置 ± 前后偏移**。
  - flag **2**（.ani F6）：`sq_SetMyShake(obj, 5, 200)` 屏震。
- **onEndCurrentAni**（解码）：回 `STATE_STAND`（施法 900ms 结束即自由，沼泽 PO 自治存活）。
- **checkCommandEnable**（明文+解码 Block 0 同文重复）：站立恒真；普攻态 `sq_IsCommandEnable(247)`。

### 2.3 被动对象 24370 · case 247（六回调实测走读）

- **setcustomdata**：存 var `[col0 吸附范围, col1 吸附时长, col2 吸附强度]`；`sq_SetCurrentAttackBonusRate(sq_GetCustomAttackInfo(obj,45), 爆炸倍率)`；→ 状态 10。
- **setstate**（状态机 10→11→12→13）：
  - **10 起幕**：主动画 `character/swordman/effect/animation/tombstoneswamp/groundstart_00.ani`（2 帧 720ms，bottom 层）；
    `sq_CreateDrawOnlyObject` ×3 个 `tombstonestart_03.ani` 墓碑，偏移 (0,-60) / (-150,+50) / (+150,+50)——**三块墓品环绕**。
  - **11 吸附循环**：主动画 `groundloop_00.ani`（1 帧 960ms LOOP）；三墓碑切 `tombstoneloop_02.ani`。
  - **12 收场**：主动画 `groundend_00.ani`（1 帧 360ms）。
  - **13 爆炸**：层级切 normal；动画 = 24370 obj **etc motion #68** = `Animation/TombStoneSwamp/Explosion_02.ani`（16 帧 960ms，F0-F4 五帧攻击盒）；
    攻击信息 = **etc attack info #45** = `AttackInfo/TombStoneSwampExplosion.atk`；再叠 `tombstoneenda_01` / `tombstoneendb_01` 两个纯视觉层、三墓碑切 `tombstoneendc_00.ani`；`sq_SetMyShake(15, 240)`。
- **procappend**：状态 <12 且对象年龄 ≤ col1(5000ms) 时 `setRangeObjectXPos(obj, 380px, NEUTRAL, 0, 400, 0, col2)`——
  **吸附实现**（`sqr/common_object/run_script/range_object.nut:74` 实测）：枚举 380px 半径内敌方（z 限 400、可移动），
  每帧把敌人 x/y 各向沼泽中心拉 **col2=2px**（含 isMovablePos 撞墙检查）。年龄超时 → 状态 12。
- **onendcurrentani**：状态 ≠13 → 状态+1 顺延；状态 13 → 销毁。（**再按引爆** = 外部把 var[1] 清零 → procappend 超时短路 → 同一条 12→13 链。）
- **destroy**：`RemoveAllAni(obj)`。

**PO 全周期**：出生 → 起幕 720ms（三墓碑）→ 吸附循环至对象年龄 5000ms（L10 注意：吸附计时从出生起算，与起幕/循环两态并行）→ 360ms 收场 → 960ms 爆炸(F0-F4 判定) → 销毁，共 ≈ **6320ms**（再按引爆 = 任意时刻清零计时器短路进入尾部链）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/tombstoneswamp_body.ani`（角色，.chr #169） | 15 | 900ms（60×15） | F4=**1**（建沼泽）、F6=**2**（屏震） | 无（判定在 PO） | 全帧 DAMAGE BOX + **DAMAGE TYPE SUPERARMOR（施法全程霸体）**；sm_body 图集 |
| 同名 `.ani.als` | — | — | — | — | `[none effect add]`×2：TombStoneSwamp_00(F0,z10002)/01(F5,z10001) 挥手起势光效 |
| `effect/animation/tombstoneswamp/groundstart_00.ani`（PO 起幕，bottom） | 2 | 720ms | 无 | 无 | MagicCircle.img；.als 叠 GroundStart_01~06 六层（z -6..-1） |
| `groundloop_00.ani`（PO 循环） | 1 | 960ms（LOOP） | 无 | 无 | 同上；.als 存在 |
| `groundend_00.ani` | 1 | 360ms | 无 | 无 | 同上 |
| `tombstonestart_03.ani`（三墓碑起幕，draw-only ×3） | 12 | 720ms | 无 | 无 | TombStone.img；.als 存在 |
| `tombstoneloop_02.ani`（三墓碑循环） | 16 | 960ms | 无 | 无 | 同上 |
| `tombstoneenda_01.ani` / `endb_01.ani`（爆炸视觉层） | 8 | 480ms | 无 | 无 | Stone.img；.als 各一 |
| `tombstoneendc_00.ani`（三墓碑崩碎） | 7 | 420ms | 无 | 无 | Beam.img |
| `…24370\Animation\TombStoneSwamp\explosion_02.ani`（PO 爆炸判定层） | 16 | 960ms | 无 | **F0-F4** | 盒见下；GRAPHIC EFFECT；.als 叠 Explosion_00/01/03/04/05 五层（z -3..-1、10001/10002） |

**explosion_02.ani 攻击盒**（偏移+尺寸，DNF 像素）：F0 `-284 -120 -156 587 240 462`，F1 `-309 -120 -128 630 240 469`，F2-F4 ≈ `-270 -120 -110 570 240 462`。
折算半尺寸 ≈ **(2.9, 1.2, 2.3) 单位**、中心前偏 ≈ 0（爆炸以沼泽为中心向前覆盖）。

**TombStoneSwampExplosion.atk**（`…24370\AttackInfo\tombstoneswampexplosion.atk` 实测）：

| 字段 | 值 | → 我们 HitReaction |
|---|---|---|
| attack type / elemental | magic / **dark element** | 无属性直伤（元素系统缺失） |
| damage reaction / direction | **down** / front | HitstunMs 900 + 击倒表现 |
| push aside / lift up | 120 / **450** | KnockbackX=120 / LaunchY=450（大浮空） |
| damage bonus | 空（倍率由写包 col3 注入） | Damage=固定值 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_tombstoneswamp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_tombstoneswamp.skl` | ✅（257 行） | 6 列等级数据/CD/MP |
| 注册行 | load_state 行 68（247/247） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 247 |
| 主 nut | tombstoneswamp.nut | `…\pvf\sqr\character\swordman\tombstoneswamp\tombstoneswamp.nut` | ✅（105 行，4 段 XOR 已解码） | 施法/落点/再按引爆 |
| ap nut | —（无） | `…\pvf\sqr\character\swordman\tombstoneswamp\`（仅 1 文件） | ⛔ 不存在 | PO 逻辑在共享回调 |
| PO 回调 | setcustomdata/setstate/procappend/onendcurrentani/else.nut case 247 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅（各 1 处） | 沼泽状态机/吸附/爆炸 |
| 吸附函数 | range_object.nut | `…\pvf\sqr\common_object\run_script\range_object.nut` | ✅（setRangeObjectXPos:74） | 逐帧拉拽实现 |
| PO 定义 | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅（C2 定点读） | etc motion #68 / etc attack info #45 |
| PO 爆炸动画 | explosion_02.ani(+.als) | `…\passiveobject\script_sqr_nut_qq506807329\swordman\Animation\TombStoneSwamp\` | ✅ | 爆炸判定层 |
| PO .atk | tombstoneswampexplosion.atk | `…\passiveobject\script_sqr_nut_qq506807329\swordman\AttackInfo\` | ✅ | down/push120/lift450/暗 |
| .chr 条目 | etc motion #169（行 1142） | `…\pvf\character\swordman\swordman.chr` | ✅ | TombStoneSwamp_Body.ani |
| 角色 .ani | tombstoneswamp_body.ani(+.als) | `…\pvf\character\swordman\animation\` | ✅ | 15 帧 900ms，F4/F6 flag |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\`（grep 无） | ⛔ 不存在 | 伤害全在 PO |
| 特效 .ani | groundstart/loop/end、tombstonestart/loop/enda/b/c 等 50 文件 | `…\pvf\character\swordman\effect\animation\tombstoneswamp\` | ✅ | 沼泽/墓碑视觉（含 _01~_06 变体与 explosionground 系列） |
| 关联系统 | ghostsoulrelease（技能 131） | `…\pvf\sqr\character\swordman\ghostsoulrelease\` | ✅（存在） | 取消施放体系（本技能是其用户之一） |
| 装备层 | coat 层 tombstoneswamp ×9 | `…\pvf\equipment\character\swordman\avatar\coat\` | ✅（计数） | 换装图层 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`。主链 7 张 + 变体 11 张（全目录扫描实测，含 2 张跨目录借用）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动画 | 必需（共享） | ✅ 已在库 |
| `Character/Swordman/Effect/TombStoneSwamp/MagicCircle.img` | sprite_character_swordman_effect_tombstoneswamp.NPK | 沼泽地面法阵（start/loop/end 三态） | **必需** | ❌ |
| `…/TombStoneSwamp/TombStone.img` | 同上 | 三墓碑（起幕+循环） | **必需** | ❌ |
| `…/TombStoneSwamp/Explosion.img` | 同上 | 爆炸主视觉（含 als 五层） | **必需** | ❌ |
| `…/TombStoneSwamp/ReadyA.img` / `ReadyB.img` | 同上 | 角色施法 als 光效 | **必需** | ❌ |
| `…/TombStoneSwamp/Stone.img` | 同上 | 爆炸碎岩层 a/b | 可选 | ❌ |
| `…/TombStoneSwamp/Beam.img` | 同上 | 墓碑崩碎光柱 | 可选 | ❌ |
| BlackHole / BlueFlame / ExplosionFrag / GroundANormal~ELDodge(6) / HalfSphere / TombStoneEffect.img | 同上 | _01~_06 变体与 explosionground/tombstonelocation 系列（本档主链未用） | 可选 | ❌ |
| `Character/DemonicSwordsman/Effect/TimeBreak/Shock.img`、`Monster/CollosseumofEmpire/Mortuus/GreenHeart_ExplosionA05.img` | 跨目录借用（L14 常态） | 变体系引用 | 可选 | ❌ |

缺失 img：必需级 5 张（同 NPK 一次提取全覆盖）、可选级 13 张。角色侧无新 img（sm_body 已在库）。

## 5. 实现方案草案

**结构映射**：DNF"沼泽 PO 自治 7s" → 我们"施法 900ms 技能 + 两个自治 Area（吸附区 5s → 爆炸区）由技能 OnUpdate 定时接续创建"。

### 内容件清单

1. **`DotNet~/Skills/TombStoneSwampSkill.cs : SkillLogic`**（BloodBoomSkill 帧触发范式 + 定时接续）
   - `CooldownMs = 40000`（DNF 原值；demo 建议 20000）；`TotalTimeMs = 5900`（施法 900 + 吸附 5000）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanTombStoneSwampBody)`（15 帧自带 als 光效）。
   - `OnUpdate`：
     - F4（≈240ms，SubState==0）：`ctx.CreateArea(AreaIds.TombStoneSwamp, ctx.GetTargetPosition())`（沼泽区在施放点生根；前后偏移 v1 砍掉，见 §7）、SubState=1；
     - `GetElapsedMs() ≥ 5600 && SubState==1`：`ctx.CreateArea(AreaIds.TombStoneSwampExplosion, ctx.GetTargetPosition())`（沼泽 F4≈240ms 出生 + 年龄 5000 + 收场 360ms；爆炸 Area 自治存活至 6560ms，超出技能时长无碍）、SubState=2；
     - （v2 二段交互：吸附期内 `ctx.PeekBufferedButton()==<本技能键>` → 提前建爆炸区——见 §8 缺口①）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/TombStoneSwampArea.cs : AreaDefinition`**（吸附区，FireCircleArea Tick 范式 + 负 KnockbackX 拉拽）
   - `TotalTimeMs = 5720`（720 起幕+5000 循环）、`TickTimeMs = 400`、`TickActions = { MeleeHit }`；
   - `HalfExtents = (3.8, 1.0, 3.8)`（380px **半径**口径——procappend `sq_GetDistance ≤ 380` 实测为半径，比 087 的 ÷2 口径更宽，本档采信半径直译）；
   - `HitReaction { Damage = 0, HitstunMs = 0, KnockbackX = -80, LaunchY = 0 }`——**吸附 = 逐 tick 负向击退拉向施法者**（L22 鬼影鞭同构；DNF 原版 2px/帧 ≈ 80px/400ms，方向以施法者为中心近似沼泽中心，见 §7）；
   - `ViewAnimId = AnimId.TombStoneSwampGroundStart`（起幕 720ms 后视觉停在 LOOP 帧由 json 天然承担）；
     墓碑三层视觉简化为单层（Area 无多偏移视图，§7）。
3. **`DotNet~/Areas/TombStoneSwampExplosionArea.cs : AreaDefinition`**（爆炸，ReleaseWaveArea 一次性爆发范式）
   - `TotalTimeMs = 960`（explosion_02 时长）、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (2.9, 1.2, 2.3)`（F0 盒 587×240×462 折算）；
   - `HitReaction { Damage = 320, HitstunMs = 900, KnockbackX = 120, LaunchY = 450 }`（atk 原值 push120/lift450/down；Damage=col3 19997% 的 demo 折算）；
   - `ViewAnimId = AnimId.TombStoneSwampExplosion` + `ViewEndAnimId` 可空（爆炸即终）。
4. **无新增 Action/Buff/Bullet**（MeleeHit 现成；诅咒类状态本技能没有——比 044 干净）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 247 + tombstoneswamp_body.ani | `TombStoneSwampSkill` + `AnimId.SwordmanTombStoneSwampBody` |
| flag 1 建 PO（±100px 前后偏移） | F4 `CreateArea(GetTargetPosition())`（偏移砍掉） |
| 沼泽 PO 状态机 10→11→12→13 | 吸附 Area（5720ms）→ 定时接续爆炸 Area |
| setRangeObjectXPos 逐帧 2px 拉拽 | Tick 区负 `KnockbackX`（L22 拉拽链路） |
| 再按技能键 = var[1] 清零短路 | 技能二段交互门面（缺口，v1 砍掉） |
| TombStoneSwampExplosion.atk | 爆炸 Area `HitReaction` |
| 鬼魂释放 12 状态内施放 + 落点偏移 | 技能取消体系（缺口，砍掉——站立施放即可） |
| DAMAGE TYPE SUPERARMOR 全程 | 霸体帧（延后档） |

### 注册点清单（B6 批号段）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.TombStoneSwamp = 27` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `TombStoneSwamp = 30`、`TombStoneSwampExplosion = 31` |
| AnimId | `AnimConfigRegistry.cs` | SwordmanTombStoneSwampBody=132、GroundStart=133、GroundLoop=134、GroundEnd=135、StoneStart=136、StoneLoop=137、StoneEndC=138、Explosion=139、ReadyA/ReadyB=140/141（als 层） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×6~10；img 必需 5 张 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000 ms | 20000 |
| 施法动画 | 15 帧 900ms（吃攻速） | 900（TotalTimeMs 前段） |
| 沼泽吸附范围 | 380px 半径 | HalfExtents (3.8,1.0,3.8) |
| 吸附时长/强度 | 5000ms / 每帧 2px | 5720ms / 每 400ms 拉 80px |
| 爆炸延迟 | 沼泽出生（施法 F4≈240ms）后 5000+360ms | ElapsedMs 5600 |
| 爆炸伤害 | col3 19997%→159988%（Lv1→40） | 320 固定 |
| 爆炸反应 | down / push120 / lift450 / 暗属性 | 硬直 900 / Kb 120 / Ly 450 |
| 爆炸判定 | 盒 587×240×462px（F0-F4，300ms 窗口） | HalfExtents (2.9,1.2,2.3)、EnterActions 单次 |
| 前后设置 | ±100px（←/→ 键） | 砍掉（缺口 R1-A3） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 全部主链 .ani（body/ground×3/tombstone×4/explosion_02） | 节面常规（FRAME/DELAY/IMAGE/LOOP/ATTACK BOX/DAMAGE BOX） | **现有 ani 子命令全覆盖** |
| tombstoneswamp_body.ani | `[DAMAGE TYPE] SUPERARMOR`（整节跳过已知） | 霸体帧归延后档，无需工具改动 |
| groundstart_00.ani | `[RGBA]`（已支持）、`[PLAY SOUND]`（整节跳过） | 无缺口 |
| explosion_02.ani | `[GRAPHIC EFFECT]`（L15 已支持）、`[PLAY SOUND]` | 无缺口 |
| 全目录 .ani 变体 | `[SHADOW]`（记档）、`[IMAGE RATE]`（延后档） | 已在缺口累计，无新增 |
| `.atk`（tombstoneswampexplosion） | `.atk` 无子命令 | 手抄 6 值可接受；批量化并入 atk 子命令立项 |
| `.obj`（24370） | `.obj` 无子命令 | 已有 L20/L9 结论，手工映射 Area 编排 |
| `.skl` | `.skl` 无子命令（6 列矩阵小，手抄无压力） | 并入既有缺口 |
| tombstoneswamp.nut | mod XOR compilestring 混淆 | 非翻译对象；**解码法已在本档 §2.2 留档**（python 逐 `(a^b)` 保序拼接） |

结论：.ani/.als 资源全部可被现有子命令翻译；实质缺口 = `.atk`/`.obj`/`.skl` 三类既有项，无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 再按技能键提前引爆 | **技能二段交互门面**（R4-A16 已记档，本技能第 4 例） | v1 自然 5s 超时爆炸；v2 沿用 PeekBufferedButton 窗口方案（但 CD 40s 内 TryCast 被拒，需 ManualCooldown+窗口内自消费，实现期与 239/242/230 统一决策） |
| 吸附期(PO 状态 11)外不可引爆 | 同上（二段交互的状态门禁细节） | 简化为技能全程可提前引爆（DNF 起幕 720ms 内不可） |
| 施放时按←/→ 在前后 100px 设置沼泽 | **技能中方向输入读取**（R1-A3 已记档） | 沼泽固定生根在施放点 |
| 鬼魂释放 12 状态内取消施放 + 状态相关落点 | 技能取消体系（缺失） | 不实现（站立施放） |
| 吸附方向 = 拉向沼泽中心 | 我们 KnockbackX 方向 = 以施法者为中心径向（L24 语义差异；沼泽≈施放点，偏移小） | demo 可接受；沼泽偏移功能上线时一并校 |
| 三墓碑三偏移 draw-only 视觉 | Area 单 ViewAnimId（无多偏移子层） | 墓碑层并入沼泽主视觉 json 或砍掉一层 |
| 施法全程霸体（SUPERARMOR） | 霸体帧延后 | 跳过（受击照常） |
| 爆炸暗属性 | 元素系统缺失 | 无属性直伤 |
| 屏震 5/200、15/240 | 屏震延后 | 跳过 |
| 施法动画吃攻速（sq_SetStaticSpeedInfo） | 攻速系统缺失 | 固定 900ms |
| 音效 SM_TOMBSTONE | 音频延后 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. `getGhostSoulRelease_Area_Distance` 的 4 个落点档（131 的 static 0-3 槽）具体数值——依赖技能 131 的 skl static data，本档未展开（属鬼魂释放体系，E/C 类批次分析 131 时补）。
2. `groundloop_00` 系 `_01~_06` 变体、`explosionground_00`、`tombstonelocation_tombstone_a/b/c` 的运行时选择逻辑（共享回调未引用，疑 mod 遗留/引擎备用）——按可选资源记。
3. `[skill fitness second growtype] 1 2` 的语义（推断为"一觉/二觉两阶段均可学"，与 [second growtype maximum level] 两位=30/30 自洽，无独立佐证）。

**新系统级缺口（§6.3 清单外/追加实证）**
1. **技能二段交互门面**第 4 例实证（R4-A16 三例之外）：DNF 的实现形态值得记档——**不是**施法中按键，而是 checkExecutableSkill 在 CD 期拦截再按 → 直接改写**已存在 PO 的内部变量**（var[1]=0 短路计时器）。我们的对应物 = "技能对已创建 Area 的运行时控制门面"（提前引爆/销毁），比通用二段交互更具体，可与 Area 接续创建（087 §8）合并立项。
2. **吸附（拉拽）语义确认**：L22 只证了"命中瞬间拉近"；本技能是**持续逐帧拉拽**（非命中驱动）——用 Tick 区负 KnockbackX 可近似，但每 tick 会施加一次击退物理（含落地摩擦），DNF 是直接改坐标（isMovablePos 检查）。若手感差异大，需 LSFlight 侧"持续牵引"模式。记档供拉拽族（鬼影鞭/聚渊/冥祭之沼三例）统一评估。
3. **mod XOR 混淆 nut 的通用解码法**（工具向）：`compilestring((0xA^B).tochar()+…+"literal"+…)` 形态可用 python `chr(a^b)` 保序拼接完整还原（本档 4 段全部成功，含 junk 变量名但 API 调用可读）。建议记入轮间经验供后续 C6④ 案例 batch 解码。

**给下轮的经验**：二觉 swamp/area 类技能 = "主 nut 薄壳 + 24370 case N 状态机（10 起/11 循环/12 收/13 爆炸）+ setRangeObjectXPos 拉拽"三件套；主 nut 混淆先解码再读（§2.2 方法）；吸附参数 = 写包 dword 顺序对照 level property 列（F7 解码法再实证）。
