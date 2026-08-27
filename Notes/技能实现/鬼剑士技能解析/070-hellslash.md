# 共鸣 : 聚渊（hellslash）

> 技能ID 70 | 级别 B | 可实现性 🔶（幻鬼突进斩+灵魂刀聚怪+双终结三段全可表达；追加终结段被本 pvf static data 关闭、幻鬼实体记忆可绕过、atk -2 错位按 F5 规则换算） | 分析日期 2026-08-22 | 批次 B6

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 共鸣 : 聚渊 | `skill\Swordman\ghostsword\hellslash.skl [name]` |
| 英文名 | hellslash（取 skl 文件名；[name2] 节无） | 同上 |
| 职业 | 剑影·夜刀神（[skill fitness growtype] 空档；[second growtype maximum level] 第 11/12 位=50/50 → growtype 5 二觉档，F5 族三方互证；explain 通篇 幻鬼/夜刀神） | 同上 |
| 学习等级 | 70（二觉技档） | 同上 [required level] |
| 最高等级 | 60（二觉档上限 50） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | active（skill class 1，物理） | 同上 [type] / [weapon effect type] |
| 指令 | ↑→→ + Z（指令 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 50000 ms（固定） | 同上 [cool time] |
| MP | 686 → 1440（Lv1→Lv60） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶体 3037×3 | 同上 [consume item] |
| 可执行状态 | `[executable states] 0 8 14`（站立/普攻/状态 14） | 同上 [executable states] |
| static data | `100 400 100 0`（L21 读法对位模板行，见下） | 同上 [dungeon][static data] |
| 一句话效果 | 幻鬼向前突进斩击并封印定身敌人，夜刀神掷出灵魂刀把敌人吸聚到一处，随后与瞬移到身后的幻鬼一同发动双终结斩 | 同上 [explain] |

**level property（3 列 level info + 4 行 static 向量；模板 9 行实测）**：

| 参数 | 源（L21） | 值 | 语义 |
|---|---|---|---|
| 幻鬼移动斩击攻击力 | level col0 | `2628% → 21031%`（Lv1→60） | dword 74 state10 伤害 |
| 幻鬼终结攻击力 | level col1 | `10515% → 84125%` | dword 74 state11/12 伤害 |
| 夜刀神终结攻击力 | level col2 | `13144% → 105156%` | dword 72/73 伤害 |
| 幻鬼移动斩击攻击范围 | static[0] | 100% | dword 74 攻击盒倍率 |
| 灵魂刀吸取范围 | static[1] | **400px** | dword 71 吸取半径；**另被 case 72/73 挪用作攻击盒倍率（×4.0）** |
| 夜刀神和幻鬼终结攻击范围 | static[2] | 100% | dword 74 state11/12 攻击盒倍率 |
| 追加斩击开关 | static[3] | **0 = 关** | onEndCurrentAni 分支门（本 pvf 关闭追加段，见 §2.2） |

## 2. 技能逻辑走读

### 2.1 注册与文件链（F5 链路，R1-A5 建立 + 本档四 dword 全实证）

```
// sqr/character/swordman_load_state.nut 行 170（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/hellslash/hellslash.nut", "hellslash", STATE_HELLSLASH, SKILL_HELLSLASH);
// swordman_header.nut：STATE_HELLSLASH=118（行 58）、SKILL_HELLSLASH=70（行 155）
// 行 17：pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349) —— unclebang 共享 PO
```

- 主 nut：`sqr\character\swordman\5_ghostsword\hellslash\hellslash.nut`（177 行，**无混淆干净原版**）+ `ap_hellslash.nut`（83 行，命中定身 appendage）。
- PO 24349：`passiveobject.lst:29375` → `passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj`；
  六回调在 `sqr\shared_passive_object\swordman\*.nut`，按写包首 dword 分派——**本技能用 71/72/73/74 四个分支**（setcustomdata 行 966-1024 全部实测）。
- 常量（header.nut 实测）：`CUSTOM_ANI_HELLSLASH=304`、`CUSTOM_ANI_HELLSLASH_ADD=305`（.chr etc motion 304/305 = `Animation/hellslash.ani` / `hellslash_add.ani`，行 1277/1278）；
  `CUSTOM_ATTACK_INFO_HELLSLASH=166`、`_ADD=167`（.chr etc attack info 166/167 = `AttackInfo/hellslash.atk` / `hellslash_add.atk`，行 1460/1461）。
- 关联被动 `SKILL_BLADESPIRIT=123`（剑影刀魂）：其 level col0/col1 作为 **动画速度倍率**（1+rate）注入全部演出——攻速联动，我们侧无消费（§7）。

### 2.2 主 nut 逐回调（hellslash.nut，177 行实测）

- **onSetState subState 0**（施法）：
  1. `sq_SetCurrentAnimation(304)`（hellslash.ani 24 帧 3214ms）+ 攻速倍率；
  2. **写包 dword 72** → `sq_SendCreatePassiveObjectPacket(24349, 0, 0, 0, 0)`——夜刀神本体终结 PO（出生在自身位）；
  3. `getVSObject(obj)` 判幻鬼实体（F5 的 VS 在场记忆）：**在场则先销毁它**、不在场也照样——两种分支都**写包 dword 74** → `CreatePassiveObject(24349, 0, -100, 0, 0)`（幻鬼 PO 出生在身后 100px）。⚠ 即幻鬼实体记忆对本技能**非必需**（与 48 冥夜鬼天杀同为低依赖样本，R3-A14 对照）；
  4. 记录 `lastTime = 动画总时长`（3214ms）→ 供 PO 定身 appendage 用。
- **onKeyFrameFlag subState 0**：
  - flag **10002**（.ani F5 @ 839ms）：写包 dword **71** → `CreatePassiveObject(24349, 0, +300, 0, 0)`——**灵魂刀**在前方 300px 落地，开始聚怪；
  - flag **10004**（.ani F13 @ 2349ms）：`als_ani` 叠 `hellslash_ttack_00.ani` 斩击光效。
- **onEndCurrentAni subState 0**：`sq_GetIntData(SKILL_HELLSLASH, 3)`（static[3] 追加开关）>0 → 切 subState 1（追加终结段）；**本 pvf = 0 → 直接回站立（追加段为死配置）**。subState 1（未启用）：播 305 动画、叠 6 层封印符 `sworddancebs02effect_01~06`（借 SwordDanceBS 资源）、写包 dword 73（追加终结 PO，挑空 300/300）。
- **checkExecutableSkill/checkCommandEnable**：常规（站立/普攻/14 态可放）。

### 2.3 共享 PO 24370→24349 六回调 · dword 71/72/73/74（全部实测）

**dword 74 幻鬼**（setstate.nut:728 状态机 10→11→12）：
- **state 10 突进斩**：动画 etc motion **88** = `animation/hellslash/vsmove/vsattack01_00.ani`（22 帧 1440ms，F1-F18 攻击盒从贴身推进到 +850px——**突进多段判定**）；攻击信息 `sq_GetCustomAttackInfo(obj, 42)`（**VS 族 -2 规则**：运行时实载 etc attack info **40** = `suddenstrikevs.atk`，F5 R2-A10/R3-A13 换算，本技能 dword 71-74 全在 63-74 区段）；伤害 = level col0 + 攻击盒倍率 static[0]；叠地效 `vsattack01_floor_00.ani`。
- **onattack state 10 命中**：`sq_AppendAppendage(damager, …, "ap_hellslash.nut")` + `sq_SetValidTime(lastTime=3214ms)`——**命中定身**（ap 详见下）。
- **onendcurrentani state 10 → 11**。
- **state 11 幻鬼终结**：动画 etc motion **90** = `vsfinish/vsattack02_body.ani`（39 帧 3361ms，F6-F8 攻击盒 216×120×300）；攻击信息 43（→实载 41 = `windspiritvs.atk`）；伤害 col1、盒倍率 static[2]；**`setCurrentPos(parentChr.x - 350px 反向)` + 方向翻转——幻鬼瞬移到施法者背后**；onkeyframeflag F6=10001 音效、F32=10002 消散光效（disappear front/back）。
- state 12（追加终结，static[3]>0 才到）：动画 93 = `vs_add_attack02_body.ani`、atk 44（→42）、伤害 col1。
- **onendcurrentani state 11/12**：销毁。

**dword 71 灵魂刀**（setcustomdata:966 + procappend:384）：
- 出生：动画 etc motion **89** = `soulsword/soulsword_01.ani`（24 帧 1460ms，**无攻击盒**）+ 前景层 `soulswordforeground_00.ani`；
- 出生瞬间扫描：400px（static[1]）内最后一个敌方 → 记其坐标为 `hellslashAttract` 锚点；
- **procappend 每帧**：锚点 400px 内所有敌方 → `sq_GetAccel(锚点, 敌坐标, 200)` 逐帧拉向锚点——**聚怪**（同冥祭之沼的拉拽族）。

**dword 72 夜刀神本体终结**（setcustomdata:982）：动画 etc motion **91** = `…/effect/animation/hellslash/hellslash_attack.ani`（24 帧 3214ms，与角色动画同拍；**F13-F18 攻击盒 450×120×316**）；攻击信息 = **角色侧** `sq_GetCustomAttackInfo(parentChr, 166)` = `hellslash.atk`；伤害 col2；盒倍率 static[1]（400% → 实际判定 1800px 宽，覆盖整个吸取区）；onkeyframeflag F13(10004)：屏震 8/300 + 白闪。

**dword 73 追加终结**（setcustomdata:999，本 pvf 死配置）：动画 92 = `hellslash_add_attack.ani`；atk 167 = `hellslash_add.atk`；伤害 col2；`BackForce 300 / UpForce 300 / Direction UP`（挑空）。

**ap_hellslash.nut**（命中定身，83 行实测）：
- `proc`：强制 parent（被命中敌人）进 `STATE_HOLD`（每帧重发）——**封印定身**；
- `onEnd`：若仍处 HOLD → `STATE_DOWN`（DOWN_PARAM_TYPE_FORCE 100/150）——**定身结束强制倒地**；
- 附加的 CustomDamageType/不可抓取 代码被 mod 注释（存疑项）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/hellslash.ani`（角色 #304） | 24 | 3214ms | F1=10001、**F5=10002（灵魂刀）**@839ms、F8=10003、**F13=10004（斩击光+终结判定窗起点）**@2349ms | 无 | sm_body；`.als` 叠 HellSlash_00~09 十层（z -2/10001-10008） |
| `hellslash_add.ani`（#305，死配置） | 11 | 1045ms | F1/F2 | 无 | `.als` 叠封印符 add_09~21 十三层 |
| `…unclebang…\hellslash\vsmove\vsattack01_00.ani`（幻鬼突进） | 22 | 1440ms | 无 | **F1-F18**（盒 x 0→850px 推进，y 偏 -150 高 300） | Energy04.img；.als 存在 |
| `…\soulsword\soulsword_01.ani`（灵魂刀） | 24 | 1460ms | 无 | 无（纯聚怪体） | 借 `SpeedSlashUpper/en01.img`；.als 前景层 |
| `…\vsfinish\vsattack02_body.ani`（幻鬼终结） | 39 | 3361ms | F6=10001、F32=10002 | **F6-F8**（216×120×300） | 借 `BladeSpiritDot/VengeanceSpirit.img` |
| `effect/animation/hellslash/hellslash_attack.ani`（夜刀神终结，dword 72 视觉） | 24 | 3214ms | 同角色动画四 flag | **F13-F18**（450×120×316） | Attack02.img；盒倍率 ×4 后 1800×480×1264 |
| `effect/animation/hellslash/hellslash_add_attack.ani`（dword 73，死配置） | 11 | 1045ms | F1/F2 | F0-F2 | — |

**atk 数据**（vs 族 -2 换算后实载三份 + 角色两份）：

| 文件 | 用于 | 关键值 | → HitReaction |
|---|---|---|---|
| `…unclebang…\attackinfo\suddenstrikevs.atk`（表 40） | 幻鬼突进斩 | **down**/push20/**lift -2000**/hit down/inner/force stun 1000/ignore weight | Damage=col0、Hitstun 1000、Kb 20、Ly 0（-2000 为砸地语义） |
| `…\windspiritvs.atk`（表 41） | 幻鬼终结 | damage/push200/lift150/hit horizon/vs cut/force stun 1000 | Damage=col1、Hitstun 1000、Kb 200、Ly 150 |
| `…\spiritcrossslash.atk`（表 42，state12 用，死配置） | — | 未读（同族） | — |
| `character/…\attackinfo\hellslash.atk`（chr #166） | 夜刀神终结 | **down**/push30/lift90/hit down/blood100/knuck -1/ignore weight | Damage=col2、Hitstun 800、Kb 30、Ly 90 |
| `hellslash_add.atk`（#167，死配置） | 追加终结 | 同上结构 | — |
| `…\hellslash1/2/3.atk`（表 44/45/46） | **-2 规则下成孤儿**（无请求可达） | 1=轻反应、2/3=push0 | 记档（意图配对本，运行时按规则不加载） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | hellslash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\hellslash.skl` | ✅ | 3 列伤害 + static 4 槽 |
| 注册行 | load_state 行 170（状态 118/技能 70） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 主 nut | hellslash.nut | `…\pvf\sqr\character\swordman\5_ghostsword\hellslash\hellslash.nut` | ✅（177 行） | 施法编排 |
| ap nut | ap_hellslash.nut | 同上目录 | ✅（83 行） | 命中定身+结束倒地 |
| PO 回调 | 六 nut dword 71/72/73/74 | `…\pvf\sqr\shared_passive_object\swordman\` | ✅（五处 case 全读） | 三段判定+聚怪 |
| PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\` | ✅ | etc motion 88-93 / etc attack info 40-46 |
| PO .atk | suddenstrikevs / windspiritvs / hellslash1-3.atk | `…unclebang…\swordman\attackinfo\` | ✅ | §2.4 表 |
| .chr 条目 | etc motion #304/#305（行 1277/1278）+ etc attack info #166/#167（行 1460/1461） | `…\pvf\character\swordman\swordman.chr` | ✅ | 动画/攻击表 |
| 角色 .ani | hellslash.ani(+.als)、hellslash_add.ani(+.als) | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | hellslash.atk / hellslash_add.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 夜刀神终结 |
| PO .ani | vsattack01_00 / soulsword_01(+fg) / vsattack02_body / vs_add_attack02 等（vsmove/soulsword/vsfinish 三子目录 + 消散层） | `…\passiveobject\unclebang_shared_passive_object\swordman\animation\hellslash\` | ✅ | 幻鬼三段视觉 |
| 特效 .ani | hellslash_attack / hellslash_add_attack / hellslash_ttack_00 / talisman\sworddancebs02effect_01-06 | `…\pvf\character\swordman\effect\animation\hellslash\` | ✅ | 终结斩/封印符 |
| .act | hellslash1.act / hellslashmove.act | `…\pvf\passiveobject\character\swordman\action\` | ✅（**原始数据层镜像**，见 §8） | 数据驱动行为表（PULL APPENDAGE 50/50/1000 等） |
| 镜像动画树 | `…\passiveobject\character\swordman\animation\hellslash\`（soulsword/vsmove/vsfinish） | 同路径 | ✅（存在） | .act 引用对象 |
| 装备层 | coat 层 hellslash ×27 | `…\pvf\equipment\character\swordman\avatar\coat\` | ✅（计数） | 换装图层 |

## 4. 资源需求

两目录全量 img 扫描实测（19 + 29 种）。**跨技能借图严重**（BladeSpirit/BladeSpiritDot/SpeedSlashUpper/SpritCrossSlash/TeleportVS/SwordDanceBS/CommonEffect，L14 常态）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画 | 必需（共享） | ✅ 已在库 |
| `Character/Swordman/Effect/HellSlash/Attack02.img` | sprite_character_swordman_effect_hellslash.NPK | 夜刀神终结斩（dword 72 视觉，16 个 ani 共用） | **必需** | ❌ |
| `…/HellSlash/Energy04.img` | 同上 | 幻鬼突进斩主视觉 | **必需** | ❌ |
| `Character/Swordman/Effect/BladeSpiritDot/VengeanceSpirit.img` | sprite_character_swordman_effect_bladespiritdot.NPK | 幻鬼终结（借图） | **必需** | ❌ |
| `Character/Swordman/Effect/SpeedSlashUpper/en01.img` | sprite_character_swordman_effect_speedslashupper.NPK | 灵魂刀（借图） | **必需** | ❌ |
| HellSlash/Attack01/03-07、Energy01-08、SoulSword、SwordLight、Talisman01/02 | sprite_character_swordman_effect_hellslash.NPK | als 十层光效/封印符/变体 | 可选 | ❌ |
| BladeSpirit/001-004、BSSpeedSlash01/04、VSAttackG、SwordDanceBS/06 | 各自 sprite_…_NPK | 角色 .als 叠层（借图） | 可选 | ❌ |
| VengeanceSpirit_Dodge、LDodge、Normal、Slash02、redeye1、VSAttackD/J、VSGround、Circle | 各自 NPK | PO .als/变体层（借图） | 可选 | ❌ |

缺失 img：必需 4 张（分属 4 个 NPK——跨 NPK 提取注意）、可选 ≈ 30 张。

## 5. 实现方案草案

**结构映射**：DNF"四 PO 分工" → 我们"一 Bullet（幻鬼突进）+ 两 Area（灵魂刀聚怪 / 幻鬼终结）+ 技能 OnUpdate 定时终结 Area（夜刀神）"。

### 内容件清单

1. **`DotNet~/Skills/HellSlashSkill.cs : SkillLogic`**（BloodBoomSkill 帧触发范式）
   - `CooldownMs = 50000`（demo 建议 30000）；`TotalTimeMs = 3300`（角色动画 3214）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanHellSlash)` + `ctx.CreateBullet(BulletIds.HellSlashGhostDash)`（幻鬼突进 = 弹体，从身后 100px 起飞——SpawnOffset 直译）。
   - `OnUpdate`：
     - `GetElapsedMs() ≥ 839 && SubState==0`（flag 10002 同拍）：`ctx.CreateArea(AreaIds.HellSlashSoulSword, 施法者前方 3.0 单位)`——灵魂刀落位开始聚怪 + SubState=1；
     - `GetElapsedMs() ≥ 1440 && SubState==1`（幻鬼动画播完→瞬移）：`ctx.CreateArea(AreaIds.HellSlashGhostFinish, 施法者背后 3.5 单位)`（位置 = GetTargetPosition 反向偏移，方向读施法朝向）+ SubState=2；
     - `GetElapsedMs() ≥ 2349 && SubState==2`（flag 10004 同拍）：`ctx.CreateAreaInFront(AreaIds.HellSlashMainStrike, 0)` 夜刀神终结斩 + SubState=3。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Bullets/HellSlashGhostDash.cs : BulletDefinition`**（NormalWaveBullet 穿透范式改）
   - `Speed = 6`（850px/1.18s ≈ 6 单位/s）、`TotalTimeMs = 1180`（F1-F18 判定窗）、`HalfExtents = (4.2, 1.5, 1.5)`（F6 盒 842×300×300 折算）、`DestroyOnHit = false`（穿透多目标）；
   - `HitReaction { Damage = 60, HitstunMs = 1000, KnockbackX = 20, LaunchY = 0, ProcBuffId = BuffIds.Stun, ProcChance = 100 }`——suddenstrikevs.atk（down/push20/砸地/force stun 1000）+ **命中定身 = Stun 概率挂载**（L6 链路；DNF 的 HOLD→DOWN 两段式简化为单段定身，§7）；
   - `ViewAnimId = AnimId.HellSlashGhostDash`（vsattack01_00）。
3. **`DotNet~/Areas/HellSlashSoulSwordArea.cs : AreaDefinition`**（灵魂刀聚怪区，FireCircleArea Tick 范式）
   - `TotalTimeMs = 1460`、`TickTimeMs = 400`、`HalfExtents = (4.0, 1.0, 4.0)`（400px 半径）、
     `TickActions = { MeleeHit }`、`HitReaction { Damage = 0, HitstunMs = 0, KnockbackX = -60, LaunchY = 0 }`（负击退=拉向施法者，L22；DNF 拉向锚点坐标，见 §7）；
   - `ViewAnimId = AnimId.HellSlashSoulSword`。
4. **`DotNet~/Areas/HellSlashMainStrikeArea.cs : AreaDefinition`**（夜刀神终结，ReleaseWaveArea 一次性爆发范式）
   - `TotalTimeMs = 870`（F13-F18 六帧 80ms）、`EnterActions = { MeleeHit }`、`HalfExtents = (2.25, 0.6, 1.6)`（450×120×316 折算）；
   - `HitReaction { Damage = 250, HitstunMs = 800, KnockbackX = 30, LaunchY = 90 }`（hellslash.atk 原值 down/push30/lift90；Damage = col2 13144% demo 折算）；
   - `ViewAnimId = AnimId.HellSlashMainStrike`（hellslash_attack.ani）。
5. **`DotNet~/Areas/HellSlashGhostFinishArea.cs : AreaDefinition`**（幻鬼终结）
   - `TotalTimeMs = 240`、`EnterActions = { MeleeHit }`、`HalfExtents = (1.1, 0.6, 1.5)`（216×120×300）；
   - `HitReaction { Damage = 180, HitstunMs = 1000, KnockbackX = 200, LaunchY = 150 }`（windspiritvs.atk 原值）；
   - `ViewAnimId = AnimId.HellSlashGhostFinish`（vsattack02_body）。
6. **无新增 Action/Buff**（Stun/MeleeHit 现成；追加段 static[3]=0 死配置不实现）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 118 + hellslash.ani | `HellSlashSkill` + `AnimId.SwordmanHellSlash` |
| dword 74 state10 突进多段判定 | `HellSlashGhostDash : BulletDefinition`（穿透弹） |
| ap_hellslash（HOLD→DOWN） | `HitReaction.ProcBuffId=Stun`（单段定身近似） |
| dword 71 灵魂刀逐帧拉拽 | Tick 区负 `KnockbackX`（与 247 同构） |
| dword 72 夜刀神终结（F13-18 判定） | OnUpdate 定时 `CreateAreaInFront` |
| dword 74 state11 幻鬼瞬移背后终结 | OnUpdate 定时 `CreateArea`（反向偏移） |
| dword 73 追加终结 | 死配置（static[3]=0），跳过 |
| 刀魂 123 攻速联动 | 无攻速系统，跳过 |
| VS 实体在场消耗 | 低依赖（两分支同行为），跳过 |

### 注册点清单（B6 批号段）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.HellSlash = 28` + ButtonToSkill 新键 |
| BulletId | `Runtime\BulletDefinition.cs` | `BulletIds.HellSlashGhostDash = 6` |
| AreaId | `Runtime\AreaDefinition.cs` | `HellSlashSoulSword = 32`、`HellSlashMainStrike = 33`、`HellSlashGhostFinish = 34` |
| AnimId | `AnimConfigRegistry.cs` | SwordmanHellSlash=142、SwordmanHellSlashAdd=143、MainStrike=144、GhostDash=145、SoulSword=146、GhostFinish=147 |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×6（含 2 个 .als overlay）；img 必需 4 张（**跨 4 个 NPK**） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 50000 ms | 30000 |
| 施法动画 | 24 帧 3214ms | TotalTimeMs 3300 |
| 灵魂刀落点/范围 | +300px / 吸取半径 400px | 前方 3.0 单位 / HalfExtents (4.0,1.0,4.0) |
| 幻鬼突进 | 850px/1180ms、col0 2628%→21031% | Speed 6、Damage 60 |
| 突进命中反应 | down/push20/砸地/定身 3214ms→倒地 | Stun 挂载 + Kb 20/Ly 0/硬直 1000 |
| 夜刀神终结 | F13(2349ms) 起、col2 13144%→105156%、盒 ×4（1800px 宽） | 2349ms 触发、Damage 250、盒 (2.25,0.6,1.6)（demo 不放大 4 倍） |
| 幻鬼终结 | 动画 1440ms 后瞬移背后 350px、col1 10515%→84125% | 1440ms 触发、背后 3.5 单位、Damage 180 |
| 终结反应 | hellslash.atk down/push30/lift90；windspiritvs push200/lift150 | 见 §5.4/§5.5 |
| 追加段 | static[3]=0（关） | 不实现 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 全部主链 .ani（角色 2 + PO 3 + 特效 2） | 节面常规（FRAME/DELAY/IMAGE/SET FLAG/ATTACK BOX/DAMAGE BOX/GRAPHIC EFFECT） | **现有 ani 子命令全覆盖** |
| 角色 .als ×2 | `[none effect add]`（L12 已支持）、多 `[use animation]` 块 | 无缺口 |
| PO .als ×7（soulsword/vsattack/vsfinish 等） | 同上 | 无缺口 |
| `.atk` ×7 | `.atk` 无子命令；**[force hit stun time]**（suddenstrikevs/windspiritvs 1000ms） | 已在 R2-A8 记档清单；手抄 6 值×7 可接受 |
| `.obj`（swordman_shared） | `.obj` 无子命令（etc 表 94/47 项，F5 共用） | L9 相位建模建议不变 |
| `.skl` | `.skl` 无子命令 | 手抄（3 列+4 槽，量小） |
| `.act` ×2 | **`.act` 无子命令**（TRIGGER/BEHAVIOR 帧驱动行为表：PULL APPENDAGE、CREATE PASSIVEOBJECT 等） | 见 §8 新发现；本技能主链不走 .act（运行时真身是 unclebang 回调），暂不立项 |

结论：.ani/.als 全覆盖；实质缺口 = `.atk`/`.obj`/`.skl` 既有三项 + `.act` 新记档一项。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 追加终结段（subState 1 + dword 73） | **本 pvf static[3]=0 已关闭**（非我方缺口） | 不实现；开关上线时补 OnEnd 分支即可 |
| 幻鬼实体在场判定/消耗（getVSObject） | 幻鬼实体记忆（R2-A6 缺口） | 两分支行为相同——直接无视，零损失 |
| 命中 HOLD 定身 3.2s → 强制倒地两段式 | 目标状态控制（STATE_HOLD/DOWN 无对应；R2-A8 目标状态查询门面姊妹项） | 单段 Stun（ForbidMove 1s）近似，手感偏轻 |
| 灵魂刀吸取拉向"锚点坐标"（出生时最后一个敌人的位置） | 位置类锚点记忆（目标位置读取门面 R4-A18 姊妹） | 拉向施法者（负 KnockbackX 径向），方向近似 |
| 夜刀神/幻鬼终结攻击盒 ×4（1800px 宽横扫） | —（数值面） | demo 用原始盒 450px 不放大（×4 是吸取区联动，视觉过宽） |
| 刀魂 123 攻速倍率（全部动画提速） | 攻速系统缺失 | 固定速度 |
| 突进判定 18 帧推进盒 | Bullet 无逐帧变盒（固定 HalfExtents） | 取中段盒 (4.2,1.5,1.5) 近似 |
| 屏震 8/300 + 白闪（dword 72 F13） | 屏震/闪屏延后 | 跳过 |
| 音效 R_SM_HELLSLASH_VS_02 等 | 音频延后 | 跳过 |
| vs cut 表现（windspiritvs 专属节） | 表现层 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. **VS 族 atk -2 错位第 5 次实证**：dword 74 请求 42/43/44 → 按 F5 规则实载 40/41/42（suddenstrikevs/windspiritvs/spiritcrossslash）；`hellslash1/2/3.atk`（表 44-46，命名意图配对）在 -2 规则下无请求可达、成孤儿。若 -2 规则不适用本例，则突进/终结反应应改读 hellslash1（轻反应）/hellslash2/3（push0）——**两套读法伤害数字不变、只差命中反应手感**，实现期以实测手感定夺。
2. `hellslash_body.ani`（角色 animation 目录第三份变体）无脚本引用——用途未考证（疑旧版/装备层基样）。
3. `ap_hellslash.nut` 中被注释的 CustomDamageType/不可抓取 段——原版行为还是 mod 屠刀，未考证。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **`.act` 数据驱动行为层新发现**：`passiveobject\character\swordman\action\hellslash1.act / hellslashmove.act` + **镜像动画树** `…\animation\hellslash\`（soulsword/vsmove/vsfinish 三子目录与 unclebang 树同名并存）。.act 格式 = `[TRIGGER](帧号+范围检查) → [BEHAVIOR]（PULL APPENDAGE 50/50/1000、CREATE PASSIVEOBJECT 260168/260169 等）`——**一份不走 nut 的声明式 PO 行为定义**，与 unclebang 共享 PO 链路并存的"原始数据层"。运行时孰为真身未考证（load_state 注册实证指向 unclebang；.act+镜像树疑引擎对 260168/260169 这类内置 PO 的数据源）。建议：① `.act` 子命令记入翻译工具缺口清单；② 后续撞到 `action\*.act` 有引用者的技能时优先解此疑。
2. **持续拉拽族第三例**（247 冥祭之沼 / 70 聚渊 / R2-A9 鬼影鞭）：DNF 三种实现（逐帧 setPos / sq_GetAccel / 命中负 push）——我们统一用负 KnockbackX tick 近似，建议拉拽族立项时对表。

**给下轮的经验**：ghostsword 族（5_ghostsword 目录）**主 nut 均为干净原版**（C6④ 不适用），直接读；四 dword 编排 = "onSetState 建本体 PO + flag 建功能 PO + onEnd 分支追加段"；static data 尾槽常是追加段开关（0=关）——读到开关为 0 时追加分支按死配置处理，不必深挖。
