# 幻鬼 : 一闪（Oneslashvs）

> 技能ID 135 | 级别 A | 可实现性 🔶（幻鬼突进弹体可直接；"无施放动作取消施放"与"幻鬼位置记忆"两变体依赖技能取消/召唤物系统，简化掉） | 分析日期 2026-08-22 | 批次 A6

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻鬼 : 一闪 | `skill\Swordman\ghostsword\oneslashvs.skl [name]` |
| 英文名 | Oneslashvs（skl 文件名；无 [name2] 节） | 同上 |
| 职业 | 剑影（[skill fitness growtype]=5，L17） | 同上 |
| 学习等级 | 20（前置：123 鬼人化 Lv1） | 同上 [pre required skill] |
| 最高等级 | 60（growtype maximum：共通 50 / 剑影 50） | 同上 |
| 类型 | active（skill class 1）/ 物理系（[weapon effect type]=physical） | 同上 |
| 指令 | →↓→ + Z（[skill command advantage] 10/20：指令施放 MP 优惠） | 同上 [command] |
| CD | 6000 ms（地下城）/ 12000 ms（pvp） | 同上 [cool time] |
| MP | 32-336（pvp 16-168） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 可施放状态 | `0 8 14 126 127 136 71 45 170 20 22`——站立/普攻/走路 + 鬼步(126)/疾影斩?(127)/幻鬼贯穿(136)/白鬼一闪(71)/幻鬼断头台(45)/幻鬼剑舞(170)/20/22 等剑术技能中（=**剑术技中可用，且无施放动作**） | 同上 [executable states] |
| static data | `450`（dungeon，单值=幻鬼突进距离 px；pvp 350）——PO 脚本 `sq_GetIntData(135, 0)` 实证 | 同上 + setstate.nut |
| 一句话效果 | 幻鬼快速向前突进，攻击路径上的敌人；若已有幻鬼在场则从幻鬼位置发动并替换之；剑术技能施放中用则角色无动作、幻鬼瞬发 | 同上 [explain] + 走读 |

**level property**：仅 1 列：dungeon 2131→17055（Lv1→60，攻击力 %，PO 脚本 `sq_GetBonusRateWithPassive(SKILL_ONESLASHVS, -1, 0, 1.0)` 读列 0 实证）；pvp 51→202。无 [level property] 模板节（纯数值列）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
163: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/oneslashvs/oneslashvs.nut", "oneslashvs", STATE_ONESLASHVS, SKILL_ONESLASHVS);
17:  IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);
```

（`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\sqr\character\swordman_load_state.nut` 实测；状态 114 / 技能 135；PO 24349 = F5 共享 PO，全 ghostsword 族共用。）

文件链（F5 链路直查）：主 nut → 写包首 dword **61** → PO 24349 → `sqr\shared_passive_object\swordman\setcustomdata.nut` case 61 → `setstate.nut` case 61/state 10-11 → 对象定义 `passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj`（etc motion/attack info **0 基**索引；passiveobject.lst:29375 定位实测）。

### 2.2 主 nut 逐回调（oneslashvs.nut，118 行，实测）

- `checkCommandEnable_oneslashvs`：恒 true（任何状态可接收指令）。
- `checkExecutableSkill_oneslashvs`（施放分流，两路）：
  - **常规路**（处于状态 0/8/14）：切状态 STATE_ONESLASHVS（角色播 420ms 施法动作）。
  - **剑术中取消路**（处于 SPIRITMOVE/SPEEDSLASH/GHOSTPIERCE/WHITEGHOSTSLASH/GHOSTDECOLLATION/SWORDDANCEBS 等状态）：**不切状态**（角色无施放动作），直接：若有在场幻鬼（VSObject）→ 在幻鬼位置创建 PO 24349（写 dword 61）并**销毁旧幻鬼**；无幻鬼 → 在自身位置创建。函数返回 false（"技能未进入状态"但幻鬼攻击已发出）——[explain] "无施放动作立即出现幻鬼攻击" 的实现。
- `onSetState_oneslashvs` sub0（常规路）：停移动；播 `CUSTOM_ANI_ONESLASHVS`（=298 → `Animation/oneslashvs.ani`，攻速按 123 鬼人化列 0/1 的 SpeedRate 加速）；`als_ani` 创建两个跟随特效（`unclebang_shared_passive_object\swordman\animation\oneslashvs\start_01.ani` + `vsappear_01.ani`）；随后同样执行"有幻鬼→幻鬼位置/无→自身"的 PO 61 创建+旧幻鬼销毁。
- `onEndCurrentAni`：回 STATE_STAND。
- 角色 .ani 的 F4 flag 10001：主 nut 无 onKeyFrameFlag 消费（引擎或无作用，未考证）。

### 2.3 被动对象（PO 24349 / id 61，共享回调逐文件走读）

- `setcustomdata.nut` case 61：立即 `sendStateOnlyPacket(10)`（无数据装载——伤害/距离在 setstate 现读父角色等级数据）。
- `setstate.nut` case 61：
  - **state 10（突进）**：播共享 .obj 自定义动画 **65**（= `animation\oneslashvs\vsattack_body.ani`，实测 1 帧 100ms、F0 自带攻击盒）；攻速按鬼人化 SpeedRate；设攻击信息 `sq_GetCustomAttackInfo(obj, 31)`（= 共享 .obj etc attack info **31** → `attackInfo\oneslashvs.atk`，实测映射）；伤害 = 父角色 `sq_GetBonusRateWithPassive(135, -1, 0, 1.0)`（列 0）；目标 x = 当前 x + 450px（static[0]）存 var；播音效 R_SM_ONESLASH_VS。
  - **state 11（消散）**：播自定义动画 **66**（= `vsend_body.ani`，19 帧 1780ms）。
- `procappend.nut` case 61/state 10：`sq_GetUniformVelocity(x, 目标x, currentT, 100)` ——**100ms 匀速突进 450px**（≈4.5 单位，45 单位/s）。
- `onendcurrentani.nut` case 61：state 10 播完 → state 11；state 11 播完 → 销毁 PO。
- `onkeyframeflag.nut` case 61/state 11/flag 10001（vsend_body F13≈1300ms）：`als_ani` 创建消散双特效 `disappearback.ani` / `disappearfront.ani`。
- vsattack_body 的 .als（4 层）：VSAttack_00（Slash.img 斩光）z10002 / VSAttack_01（MistA.img 雾）z10001 / VSAttack_02（SpeedLine.img 速度线）z-1 + 借 `vsend_body.ani.als` 五层（vsend_00-04 + vsattackground_00 地面层 z-2）。
- **幻鬼记忆**：`jg_swordman\jg_swordman_common.nut:892 getVSObject` = 遍历自己名下 PO 24349，var id ∈ {61,62,63,65,66,68,70,74} 者视为"在场幻鬼"——一闪的 id 61 也在列，即**一闪后幻鬼停留在突进终点直至消散**（后续 VS 技从该位置接力）。
- 同目录 `ap_oneslashvs.nut`（79 行）：onStartMap/proc 时创建 PO 24349 dword **50** 的"幻鬼待机体"——**白名单内无任何引用者**（全 sqr 检索实测），判定为私有服遗留死码，记档不采信。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\oneslashvs.ani`（.chr etc motion #298） | 8 | 420ms（60/60/30/30/60×4） | F4=10001（无脚本消费） | 无 | 角色施法pose；仅引 sm_body%04d.img |
| `character\swordman\animation\oneslashvsready_body.ani`（etc #222） | 8 | 420ms | F4=**1** | 无 | 同构变体，白名单内无引用者（引擎备用，未考证） |
| PO `vsattack_body.ani`（共享 .obj etc motion #65） | 1 | 100ms | 无 | F0：`-80 -35 0 179 70 140` | **突进判定体**（min/max 读法 x∈[-80,179] y∈[-35,70] z∈[0,140]，按 064 口径；"偏移+尺寸"读法存疑记 §8） |
| PO `vsend_body.ani`（etc #66） | 19 | 1780ms | F13=10001 | 无 | 消散段（F12=500ms 停帧）；.als 挂 5 层 |
| PO `start_01.ani` / `vsappear_01.ani` | 10/18 | 600/900ms | 无 | 无 | 角色施法时 als_ani 创建（现身闪光，.als 各借 TeleportVS/VSAppear_00 层） |
| PO `disappearback/front.ani` | 19/19 | 665ms | 无 | 无 | 消散双特效（flag 10001 触发） |
| PO `empty.ani`（etc #64） | 1 | 50ms | 无 | 无 | 空占位 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | oneslashvs.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\oneslashvs.skl` | ✅（264 行） | 1 列等级数据 + static 450 |
| 注册行 | swordman_load_state.nut:163 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 114 / 技能 135 |
| 主 nut | oneslashvs.nut | `…\pvf\sqr\character\swordman\5_ghostsword\oneslashvs\oneslashvs.nut` | ✅（118 行） | 施放分流 + PO 61 创建 |
| ap nut | ap_oneslashvs.nut | 同目录 | ✅（79 行，**无引用者**） | 死码（幻鬼待机体 50，不采信） |
| PO 回调 | setcustomdata/setstate/procappend/onendcurrentani/onkeyframeflag.nut | `…\pvf\sqr\shared_passive_object\swordman\` | ✅（case 61 逐段走读） | 幻鬼突进/消散全逻辑 |
| PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ | 共享动画/攻击表（etc motion #64-66 / etc attack info #31，0 基实测） |
| PO .atk | oneslashvs.atk | `…\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\oneslashvs.atk` | ✅ | 见下 |
| PO 动画 | oneslashvs\*.ani（30 个）+ 6 个 .als | `…\passiveobject\unclebang_shared_passive_object\swordman\animation\oneslashvs\` | ✅ | 幻鬼现身/突进/消散 |
| 幻鬼记忆 | jg_swordman_common.nut:892 getVSObject | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅ | VSObject 判定（id ∈ {61,…}） |
| .chr 条目 | etc motion #222/#298 | `…\pvf\character\swordman\swordman.chr` 1195/1271 行 | ✅ | oneslashvsready_body/oneslashvs.ani |
| 角色 .ani | oneslashvs.ani / oneslashvsready_body.ani | `…\pvf\character\swordman\animation\` | ✅ | 施法 pose |
| 装备层 | — | `…\pvf\equipment\character\swordman\avatar\`（belt_a ls 无 oneslash 系） | ⛔ 不存在 | 幻鬼非角色图层（贴图走 Effect） |

**PO .atk 实测**（oneslashvs.atk）：physical / weapon damage apply 1 / damage reaction=damage / **vs opposite cut**（对向斩断）/ blood 30-3.0（pvp 0.5）/ attack direction=hit horizen / **lift up 200 / push aside 75**（打浮空）/ hit wav R_DARK_SWORD_HIT。

## 4. 资源需求

img 全部来自 `Character/Swordman/Effect/...`（PO 动画内引用实测），NPK 规则 `sprite_<路径下划线化>.NPK`：

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色 pose 帧 | 必需（共享） | ✅ |
| Effect/BladeSpiritDot/VengeanceSpirit.img | sprite_character_swordman_effect_bladespiritdot.NPK | **幻鬼身体**（vsattack_body + vsend_body 共用） | **必需** | ❌ |
| Effect/OneSlashVS/Slash.img | sprite_character_swordman_effect_oneslashvs.NPK | 突进斩光（vsattack_00/vsend_00/vsstart_00） | **必需** | ❌ |
| Effect/OneSlashVS/MistA.img | 同上 | 突进雾（vsattack_01） | **必需** | ❌ |
| Effect/OneSlashVS/SpeedLine.img | 同上 | 速度线（vsattack_02/vsend_04） | **必需** | ❌ |
| Effect/OneSlashVS/MistB.img、MistC.img | 同上 | 消散雾（vsend_01/02） | 可选 | ❌ |
| Effect/SpritCrossSlash/VSAttackB.img | sprite_character_swordman_effect_spritcrossslash.NPK | 消散斩光（vsend_03） | 可选 | ❌ |
| Effect/OneSlashVS/GroundA.img、GroundB.img | sprite_character_swordman_effect_oneslashvs.NPK | 地面痕（vsattackground_00/vsendground_01，.als z-2 层） | 可选 | ❌ |
| Effect/SpritCrossSlash/VSAttackA.img | sprite_character_swordman_effect_spritcrossslash.NPK | 现身闪光（start_01） | 可选 | ❌ |
| Effect/TeleportVS/Normal.img、LDodge.img | sprite_character_swordman_effect_teleportvs.NPK | 传送残影（vsappear_01/disappearback/front） | 可选 | ❌ |
| Effect/BladeSpiritDot/VengeanceSpirit_Dodge.img | sprite_character_swordman_effect_bladespiritdot.NPK | 白色变体（vsend_body_white，本链路未消费） | 可跳过 | ❌ |

**缺失 img：必需 4 张、可选 8 张，分属 4 个 NPK。**

## 5. 实现方案草案（号段：SkillIds 17 / AnimIds 70-76 / BulletIds 3，Buff-Action 未新增）

### 内容件清单

1. **`DotNet~/Skills/OneSlashVsSkill.cs : SkillLogic`**（同 WaveSwordSkill 范式）
   - `CooldownMs = 6000`（DNF 原值直用）；`TotalTimeMs = 420`（oneslashvs.ani）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanOneSlashVs)`；`ctx.CreateBullet(BulletIds.OneSlashVsGhost)`（出生=施法者身前；"从在场幻鬼位置发动"简化掉，见 §7）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。伤害在 Bullet.HitReaction（DNF 侧结算方就是 PO 24349）。
2. **`DotNet~/Bullets/OneSlashVsGhostBullet.cs : BulletDefinition`**（同 NormalWaveBullet 范式）
   - `Speed = 45`（450px/100ms → 4.5 单位/0.1s）；`TotalTimeMs = 100`（突进 100ms）；`DestroyOnHit = false`（穿透路径上全部敌人）；`HalfExtents = (1.3, 0.5, 0.7)`（atk 盒 x[-80,179]/y[-35,70]/z[0,140] min/max 折算半尺寸，§8 存疑注）；`SpawnOffset = (0.5, 0.5, 0)`（盒中心前移 0.5 单位 + 半高贴地）；`ViewGrounded = false`（幻鬼立姿）。
   - `HitActions = { MeleeHit }`；`HitReaction = { Damage = 120, HitstunMs = 600, KnockbackX = 75, LaunchY = 200 }`（atk 原值 push75/lift200 直用，LaunchOwner 链路已落地）。
   - `ViewAnimId = AnimId.OneSlashVsGhostBody`（幻鬼身体帧 + vsattack 三层 .als overlay——bullet 视图 prefab 同构现有弹视图）。
   - **消散视觉简化**：BulletDefinition 无 ViewEndAnimId（AreaDefinition 有）——100ms 后弹体直接消失，消散特效（vsend 段 1780ms）砍掉或后续给 Bullet 补 ViewEndAnimId 小扩展（记 §8，非系统级）。
3. **无新 Action/Buff**（MeleeHit 现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 施法 pose 420ms + PO 创建 | `OneSlashVsSkill.OnCast`（PlayAnim + CreateBullet） |
| PO 24349/dword 61 突进判定体 | `OneSlashVsGhostBullet : BulletDefinition`（F5 共享 PO 族的通用替代表达） |
| procappend 100ms 匀速 450px | Bullet.Speed × TotalTimeMs（配置即得） |
| atk 31（push75/lift200/穿透） | `HitReaction` + `DestroyOnHit=false` |
| setstate 现读父角色列 0 伤害 | Bullet.HitReaction.Damage 固定值（demo 惯例） |
| vsattack .als 三层特效 | 弹视图 overlay（LSAnimOverlayUtil 共享助手，releasewave 先例） |
| 在场幻鬼位置接力（VSObject） | ⛔ 召唤物实体记忆——简化为固定从自身身前发射 |
| 剑术中无动作瞬发（checkExecutableSkill 取消路） | ⛔ 技能取消体系——只做独立施放 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `…\Runtime\SkillIdAttribute.cs` | `SkillIds.OneSlashVs = 17` + ButtonToSkill 新键（如 B） |
| BulletId | `…\Runtime\BulletDefinition.cs` | `BulletIds.OneSlashVsGhost = 3` |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanOneSlashVs=70`、`OneSlashVsGhostBody=71`（vsattack_body）、`OneSlashVsGhostEnd=72`（vsend_body，预留给 ViewEndAnimId 扩展）、`OneSlashVsStart=73`、`OneSlashVsAppear=74`、`OneSlashVsDisappearBack=75`、`OneSlashVsDisappearFront=76` |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | 1 角色 + 4 必需特效 json；图集 4 张（必需） |
| 按键 | LSOperaComponentSystem | 新键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 6000 ms | 6000 |
| 角色施法 | 420ms（F4=240ms 无脚本消费） | 帧数据直用 |
| 幻鬼突进 | 450px / 100ms（static[0]/procappend） | Speed 45 单位/s × 100ms |
| 攻击力 | 列 0：2131%→17055%（Lv1→60） | Damage 120 |
| 命中反应 | push 75 / lift 200 / damage / vs opposite cut / blood 30 | Kb 75 / Ly 200 / Hitstun 600 |
| 穿透 | 共享 .obj pass all + piercing 1000 | DestroyOnHit=false |
| 攻速加成 | 鬼人化(123)列 0/1 SpeedRate | 不做（等级缩放延后） |
| 消散段 | vsend_body 1780ms + flag10001 双特效 | 砍掉（ViewEndAnimId 后补） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| oneslashvs.skl | `.skl` 无子命令 | 单列数值手抄无碍（累计缺口） |
| PO oneslashvs.atk | `.atk` 无子命令 | ~12 值手抄（累计缺口） |
| swordman_shared.obj | `.obj` 无子命令 | 本技能只取 etc motion 65/66 + attack info 31 三行映射，手工对照即可；PO 类技能持续增多，建议按 064 §8 提议立项 obj 子命令（相位序列建模） |
| PO 各 .als | `[none effect add]` 变体节 | **已支持**（AlsParser 同等解析，L12/01§5.6-7 实证），非缺口 |
| 全部 .ani | 常规节（LOOP/FRAME/IMAGE/DELAY） | 现有 ani 子命令全覆盖 |

**本技能翻译缺口 3 类（.skl、.atk、.obj）；ani/als 全覆盖。**

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 从在场幻鬼位置发动 + 接力记忆（VSObject） | **缺失：召唤物实体记忆**（§6.3"召唤物独立 AI"的轻量子集——只需位置，不需 AI） | 固定从自身身前发射；幻鬼实体系统立项时（剑影族整体依赖）回补 |
| 剑术技能中无施放动作瞬发 | **缺失：技能取消体系**（064 首报，本批 105/107 连续第三撞） | 只做独立施放版 |
| 消散段视觉（1780ms vsend + 双特效） | 框架小项：BulletDefinition 无 ViewEndAnimId（Area 有） | 弹到时直接消失；或给 Bullet 加 ViewEndAnimId（~10 行，同 Area 先例） |
| 鬼人化攻速缩放（SpeedRate） | 延后：等级数值缩放 | 固定 1.0 |
| blood/vs opposite cut 表现 | 延后：受击表现细节 | MeleeHit 标准受击链 |
| 音效 R_SM_ONESLASH_VS / R_DARK_SWORD_HIT | 延后：无音频系统 | 跳过（已考证记录） |

## 8. 存疑与缺口上报

- **未考证**：vsattack_body 攻击盒 6 数值的口径（min/max vs 偏移+尺寸——064 按 min/max、01§5.5 按 PO 中心对称，两说并存；本表按 min/max 采用，提取时以游戏内目测校准）；oneslashvs.ani F4 flag 10001 的消费方（引擎）；oneslashvsready_body.ani（etc #222，flag=1 变体）用途；executable states 中 20/22 等个别状态号归属。
- **框架小缺口**：**BulletDefinition 无收尾动画通道**（ViewEndAnimId）——弹体型 PO 的通用需求（消散/爆裂收尾），建议随下一弹技能一并补（非系统级）。
- **死码记档**：ap_oneslashvs.nut（幻鬼待机体 dword 50）无引用者——若后续技能（鬼步 126 等）需要"幻鬼常驻"，其真实创建链路需另查（本批未发现）。
- **F5 链路再验证**：本技能完整走通"首 dword 分派 → shared_passive_object 六回调 → 共享 .obj 0 基索引"链路，F5 结论无需修正；补充一点：setcustomdata case 61 不装载数据、全部参数在 setstate 现读父角色（各 VS 技写法不一，读时以 case 块为准）。
