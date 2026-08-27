# 幻鬼 : 奈落（suddenstrikevs）

> 技能ID 54 | 级别 A | 可实现性 🔶（索敌/延时判定窗小缺口可绕） | 分析日期 2026-08-22 | 批次 A15

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻鬼 ： 奈落 | `skill\Swordman\ghostsword\suddenstrikevs.skl [name]` |
| 英文名 | suddenstrikevs（取 skl 文件名，全小写） | 同上 |
| 职业 | 剑影 · 夜刀神（二觉；ghostsword/VS 族，basic explain"夜刀神挥剑"） | 同上 + 目录线索 |
| 学习等级 | 60 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30） | 同上 [maximum level] / [second growtype maximum level]（索引 10/11 = 30） |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ↓↑↓ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 20000 ms | 同上 [cool time] |
| MP | 334 → 935 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶体类道具 3037 × 2 | 同上 [consume item] |
| 可施放状态 | 0/8/14 等长表（106 个状态号）——任意动作中可发（见 §2.2 两分支） | 同上 [executable states] |
| 一句话效果 | 夜刀神挥剑，幻鬼瞬移到范围内最强敌人上方 200px，下劈砸落造成巨大物理伤害（命中强制控制默认关） | 同上 [explain] + 共享 PO 走读 |

**static data**（dungeon）：`800 100 0`——三值全部被共享 PO 回调直读实证：
- col0 = **800**：以剑影为中心的索敌范围 px（`sq_FindFirstTarget(-400, 400, 800, 500)`）；
- col1 = **100**：下劈范围%（攻击盒与特效统一缩放率）；
- col2 = **0**：幻鬼下劈命中时强制控制敌人开关（[0 关 1 开]，本 pvf 关闭）。

**level info**（1 列，Lv1 → Lv68）：col0 下劈攻击力 10573 → 84585（%）。
**level property**（4 占位符）：下劈攻击力 `(-1,0,1.0)`=level col0；索敌范围 `(0,0,1.0)`=static[0]；下劈范围 `(1,1,1.0)`=static[1]；强制控制 `(2,2,1.0)`=static[2]——四向量与 PO 回调消费完全吻合（L21 规则对位实证）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
166: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/suddenstrikevs/suddenstrikevs.nut", "suddenstrikevs", STATE_SUDDENSTRIKEVS, SKILL_SUDDENSTRIKEVS);
 18: IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);   // F5 共享判定 PO
```

- `swordman_header.nut`：`STATE_SUDDENSTRIKEVS <- 124`（54 行）、`SKILL_SUDDENSTRIKEVS <- 54`（161 行）、
  `CUSTOM_ANI_SPEEDSLASHVS <- 299`（470 行）。
- 施法动画 **复用"幻鬼 : 迅"的手势**：.chr 1272 行 `Animation/speedslashvs.ani` = etc motion 0 基 #299（1272-973，与常量对位）。奈落无专属施法动画（explain"无施放动作立即出现幻鬼"）。
- 幻鬼本体 = **F5 unclebang 共享 PO 24349**，写包首 dword = **65**（`sqr\shared_passive_object\swordman\*.nut` 六回调分派）。

### 2.2 主 nut 逐回调（suddenstrikevs.nut，106 行全读）

**checkCommandEnable**：恒 true（任意状态可出指令）。

**checkExecutableSkill**（两分支，"无施放动作"的实现）：
- 当前 state ∈ {0 站立, 8 攻击, 14（疑幻鬼共存态，未考证）} → 推子状态 0 进入状态 124（有施法手势）；
- **其余任意状态**（动作中取消施放）→ 不进状态、不播动画：直接写包 dword 65 创建 PO 24349，并先销毁在场幻鬼实体（`getVSObject` + `sq_SendDestroyPacketPassiveObject`）。

**onSetState**（subState 0，唯一）：
- 速度系数 = BLADESPIRIT（幻鬼剑术 123）level col0/col1 + 1（前置精通提速；精通系统缺失 → 固定 1.0）；
- 播 `CUSTOM_ANI_SPEEDSLASHVS`（speedslashvs.ani 440ms，F2@140ms flag 10001 无脚本消费者——137-speedslashvs.md 实测）×SpeedRate；
- `sq_StopMove`；创建 PO 24349（dword 65）——在场幻鬼实体先销毁再建（无则直接建）。

**onEndCurrentAni**：subState 0 → 回 STATE_STAND。

### 2.3 幻鬼被动对象（共享 PO 24349，case 65 两相位）

| 相位 | 动画（etc motion 0 基，**直读实测**） | 行为 | 伤害 |
|---|---|---|---|
| state 10 出现 | **#77** `animation/suddenstrikevs/suddenstrikevs_start_body.ani`（6 帧 480ms，F0=10001 闪屏、F1=10002 无消费者） | 索敌 800px 取最强敌人：命中→ PO 传送到 (敌.x, 敌.y+1, **z=200**)（敌人正上方）；未命中→ 施法者前方 200px、z=200；叠 targetfloor_00.ani 地面标记（缩放 static[1]） | 无 |
| state 11 下劈 | **#78** `animation/suddenstrikevs/suddenstrikevs_end_body.ani`（26 帧 2940ms，F0=10001 特效+音+震、F19=10002 消散层、F20-F25 RGBA 渐隐） | procappend：z 从 200 匀速降到 0，**100ms** 内砸落；F0-F2 攻击盒 `-200 -50 0 / 400 100 400`（对称 ±2 单位宽 × 高 4）；播完销毁 | `sq_GetBonusRateWithPassive(54,-1,0)` = **level col0**；攻击信息 `sq_GetCustomAttackInfo(obj, 38)`（错位见下） |

**onattack**（state 11 命中时）：static[2] > 0（本 pvf =0 关）→ 给受害者挂 `ap_suddenstrikevs.nut` 2800ms：定身（STATE_HOLD 每帧强制）+ 禁抓 + 结束时强制 STATE_DOWN 击倒 + 免伤类型切换（`sq_SetCustomDamageType`）——**本 pvf 数据下不生效**，仅记录机制。另 `Vs_Attack_Effect` 通用命中特效。

**⚔ atk 错位 -2 反向核验（本批重要产出，066 修正案闭环）**：
case 65 state 11 取攻击信息 **code 38**。`swordman_shared.obj` [etc attack info] 0 基直读 #38 = **spinningslashvs2.atk**（运行时实载）；按名称匹配奈落应在 #40 = **suddenstrikevs.atk**——code+2=40，**-2 错位规则对 case 65 同样成立**。边界修正：错位族不止 id 66-74，至少下探到 **case 65**；case 63（spinningslash，code 36/37）亦在错位区（36 直读=speedslashvs5.atk），而 case 61（code 31）与 case 62（speedslash，code 32-36，137 文档名称全对位）直读正常——**错位起点在 case 62/63 之间**（精确插入点未考证）。两份 atk 实测：

| atk | 关键参数 | 定位 |
|---|---|---|
| suddenstrikevs.atk（意图） | physic/weapon、reaction **down**、push 20、**lift -2000（负值=砸落）**、ignore weight、hit down/inner、blood 100×3.0、knuck back -1、force hit stun 1000 | 本档采用（文档草案以意图值建模） |
| spinningslashvs2.atk（运行时实载） | physic/weapon、down、push 220、lift 350、vs opposite cut、blood 70、hit direction front、knuck back 3/150、force hit stun 1000、pvp 分节 | 本 pvf 实际手感 |

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/speedslashvs.ani`（施法手势，复用） | 9 | 440ms | F2@140=10001（无消费者） | 无 | 137-speedslashvs.md 实测，无边车 |
| `…\unclebang…\animation\suddenstrikevs\suddenstrikevs_start_body.ani` | 6 | 480ms（6×80） | F0=10001（闪屏，state 10 消费）、F1=10002（无消费者） | 无 | 引 VengeanceSpirit.img 帧 97-105，IMAGE POS(-238,-331) |
| `…\suddenstrikevs_end_body.ani` | 26 | 2940ms（80+19×120+4×80+180+80） | F0=10001（劈落三层特效+音 SUDDENSTRIKE_VS_SWISH+震 10/300）、F19=10002（消散层） | **F0-F2**（`-200 -50 0 400 100 400`） | F20-F25 RGBA(204→0) 渐隐；F24 有 [INTERPOLATION] 1 |
| targetfloor_00.ani / attackfloor_00.ani / attackback_00.ani / attackfront_00.ani / disappearback.ani / disappearfront.ani | 未逐帧 | — | — | — | als_ani 驱动的特效层（nut 时序） |
| suddenstrikevs_hold.ani / suddenstrikevs_white.ani | — | — | — | — | 白名单内无引用者（强制控制关闭的备用视觉，未考证） |

`.als` 边车（同目录 6 个）：start/end_body 与 attackfloor/attackback/attackfront/targetfloor——节全部为 `[use animation]` + `[none effect add]`，现有 als 子命令可译。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | suddenstrikevs.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\suddenstrikevs.skl` | ✅ 实测 | 等级/CD/MP/static 3 值/level 1 列 |
| 注册行 | swordman_load_state.nut 行 166 / 18 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 124 + PO 24349（F5 共用） |
| 主 nut | suddenstrikevs.nut | `…\pvf\sqr\character\swordman\5_ghostsword\suddenstrikevs\suddenstrikevs.nut` | ✅ 实测（106 行全读） | 施法侧两分支 |
| 强控 appendage | ap_suddenstrikevs.nut | `…\5_ghostsword\suddenstrikevs\ap_suddenstrikevs.nut` | ✅ 实测（83 行） | 受害者 hold/down（static[2]=0 关闭） |
| 共享 PO 回调 | setcustomdata/setstate/onkeyframeflag/procappend/onendcurrentani/onattack 的 case 65 | `…\pvf\sqr\shared_passive_object\swordman\*.nut` | ✅ 实测 | 幻鬼两相位 |
| 共享 PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ 实测 | etc motion #77/78 直读对位；etc attack info #38（错位见 §2.3） |
| 共享 PO atk | suddenstrikevs.atk / spinningslashvs2.atk | `…\unclebang_shared_passive_object\swordman\attackinfo\` | ✅ 实测 | 意图 / 运行时实载 |
| .chr 条目 | etc motion #299（1272 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | Animation/speedslashvs.ani（复用） |
| 角色 .ani | speedslashvs.ani | `…\pvf\character\swordman\animation\` | ✅（137 文档） | 440ms 手势；奈落无专属施法动画 |
| PO .ani | suddenstrikevs_start_body / end_body（+特效层 ×30） | `…\passiveobject\unclebang_shared_passive_object\swordman\animation\suddenstrikevs\` | ✅ 实测 | 幻鬼本体 + 特效组 |
| .als | 同目录 6 个 | 同上 | ✅ 实测 | 常规可译 |
| 装备层 | 未查 | `…\pvf\equipment\character\swordman\avatar\` | 未查 | 施法动画为复用手势（137 已查该族），本技能不单独产生装备层需求（推断） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 施法手势 | 必需（共享） | ✅ |
| `Character/Swordman/Effect/BladeSpiritDot/VengeanceSpirit.img` | sprite_character_swordman_effect_bladespiritdot.NPK | 幻鬼身体（start/end/hold 三 ani 全用） | **必需** | ❌ |
| `Character/Swordman/Effect/SuddenStrikeVS/Attack01.img` | sprite_character_swordman_effect_suddenstrikevs.NPK | 下劈劈落特效（attackfloor/attackback） | **必需** | ❌ |
| `…/SuddenStrikeVS/StartLight03.img` | 同上 | attackfront 前景光 | 可选 | ❌ |
| `Common/CommonEffect/Glow/CircleRing.img` | sprite_common_commoneffect_glow.NPK | 目标地面标记环 | 可选 | ❌ |
| `…/TeleportVS/Normal.img`、`LDodge.img` | sprite_character_swordman_effect_teleportvs.NPK | 幻鬼消散传送层 | 可选 | ❌ |
| `…/BladeSpiritDot/VengeanceSpirit_Dodge.img` | sprite_character_swordman_effect_bladespiritdot.NPK | suddenstrikevs_white.ani（无引用者） | 可选（存疑） | ❌ |

缺失 img：**必需 2 张、可选 5 张**（4 个 NPK；AnimRes 目录 grep 实测全未入库）。与 066-spiritcrossslash 共享 VengeanceSpirit/TeleportVS 两 NPK——F5 族一次提取多技能受益。

## 5. 实现方案草案

**结构映射**：施法手势（复用迅的动画）+ 时间驱动的"目标位置延时下劈区"（Area）。

### 内容件清单

1. **`DotNet~/Skills/SuddenStrikeVsSkill.cs : SkillLogic`**（BloodBoomSkill SubState 编排 + 064 时间驱动范式）
   - `CooldownMs = 20000`；`TotalTimeMs = 600`（施法手势 440ms + 余量；下劈区是独立 Area 不受技能时长约束）���
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSpeedSlashVs)`（复用 137 草案的 speedslashvs 动画）+ `ctx.ClearHitTargets()`；**索敌**：`ctx.GetEnemies()` 中取与施法者距离 ≤ 8 单位（800px）且 HP 最高者（"最强敌人"；敌方 HP 读取走 `ctx.GetNumeric(unit, NumericType.Hp)`，见 §8 门面注记），`ctx.SetSubState(目标 unitId)`（SubState 存 unitId——进快照回滚安全；无敌则 0）。
   - `OnUpdate`：`GetElapsedMs() >= 480`（幻鬼出现窗 480ms）且未触发时：按 SubState 里的 unitId 在 `GetEnemies()` 重查该单位**当前位置**（跟随目标），`ctx.CreateArea(AreaIds.SuddenStrike, 位置)`；无匹配则取身前 2 单位；置已触发标记（SubState 取负或另用位段）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/SuddenStrikeArea.cs : AreaDefinition`**（BloodBoomArea 范式）
   - `TotalTimeMs = 320`（end_body F0-F2 判定窗近似）、`TickTimeMs = 0`、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (2.0, 0.5, 2.0)`（攻击盒 x±200/y±50/z0-400 折算，取横向与纵深半尺寸）；
   - `HitReaction { Damage = 400, HitstunMs = 1000, KnockbackX = 20, LaunchY = -2000 }`（意图 atk suddenstrikevs.atk：down/push20/lift-2000 砸落/force hit stun 1000；**LaunchY 负值天然产生向下初速度**——LaunchOwner 直通，L22 负 push 同理）；
   - `ViewAnimId = AnimId.SuddenStrikeGhostEnd`（end_body 视觉，含渐隐）+ `ViewBackAnimId = AnimId.SuddenStrikeAttackBack`（attackback 层，可选）。目标标记环（targetfloor）与闪屏跳过/延后。
3. **无新增 Buff/Action**（强制控制 static[2]=0 关闭，不做 hold 机制）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 124 + speedslashvs.ani | `SuddenStrikeVsSkill` + 复用 AnimId（137 草案） |
| 任意动作中取消施放（无手势分支） | 技能取消体系缺失 → 仅站立可放（§7） |
| 幻鬼 PO 24349 dword 65 相位 10（索敌传送+标记） | 技能 OnCast 索敌 + SubState 存目标 id；标记环视觉跳过 |
| 相位 11（z 200→0 砸落 100ms + F0-F2 判定） | 480ms 后于目标位置 CreateArea（砸落视觉并入 Area ViewAnimId） |
| 攻击信息 code 38（运行时错位 spinningslashvs2.atk） | 采用意图 atk suddenstrikevs.atk 参数（066 同款裁定） |
| static[2] 强制控制（本 pvf 关） | 不做（数据即关） |
| BLADESPIRIT 提速 | 精通系统缺失 → 固定 1.0 |
| 在场幻鬼实体销毁重建（getVSObject） | 幻鬼锚点实体缺口（R2-A8 已报）→ 不做，每次独立索敌 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SuddenStrikeVs = 19` + 按键映射 |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.SuddenStrike = 9` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanSpeedSlashVs`（复用 137 草案号）、`SuddenStrikeGhostEnd = 77`、`SuddenStrikeAttackBack = 78`（可选） |
| json 注册 | `LSAnimClipRegistrar.cs` | speedslashvs（137 共用）+ suddenstrikevs_end_body 等 ×2 |
| 图集 | `LSAnimResComponentSystem.cs` | VengeanceSpirit.img.bytes、Attack01.img.bytes（必需） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000 ms | 20000（直用） |
| 施法手势 | speedslashvs.ani 440ms | 440（复用） |
| 索敌范围 | static[0]=800px | 8 单位 |
| 幻鬼出现窗 | start_body 480ms | 480（延时创建判定区） |
| 砸落 | z 200→0 / 100ms | 并入 Area 出现瞬间（视觉自表） |
| 判定盒 | end_body F0-F2：x±200 y±50 z0-400 | HalfExtents (2.0, 0.5, 2.0) |
| 伤害 | level col0 = 10573%→84585% | 400（固定，二觉级大伤害） |
| 命中反应 | 意图 atk：down/push 20/lift -2000/force hitstun 1000 | Hitstun 1000 / Kb 20 / Ly -2000（砸落） |
| 强制控制 | static[2]=0（关） | 不做 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| suddenstrikevs.skl | `.skl` 无子命令 | 手抄（1 列 + static 3 值，量小） |
| suddenstrikevs.atk / spinningslashvs2.atk | `.atk` 无子命令 | 手抄（意图 atk 优先）；atk 立项时纳入 `force hit stun time`/`ignore weight`/`knuck back` 字段（R2-A8 已录同族） |
| swordman_shared.obj | `.obj` 无子命令 | 手工对位（F5 族常驻缺口）；**本档已给错位修正**（§2.3） |
| suddenstrikevs_end_body.ani | **`[INTERPOLATION]`**（F24，值 1）——**新见节名**，不在工具规则表 | 帧间插值开关（视觉平滑），整节跳过无碍；建议 README 未识别节清单补记 |
| 各 .ani 的 [SHADOW]/[SET FLAG]/[PLAY SOUND]/[DAMAGE TYPE] | 已知跳过族 | 非新缺口（SHADOW 已记档） |
| 6 个 .als | `[use animation]`/`[none effect add]` 均已支持 | 无缺口 |

结论：.ani/.als 资源可被现有子命令翻译；实质缺口 `.skl`/`.atk`/`.obj`（族共性）+ 新节 `[INTERPOLATION]` 1 条，计 4 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 任意动作中无施放动作瞬发（106 个可施放状态） | 技能取消体系缺失（064/R1-A3 已录） | 仅站立可放；取消态瞬发不做 |
| "最强敌人"索敌（HP 比较） | `GetEnemies` 已有；敌方 HP 需 `GetNumeric(unit, key)`（门面存在但需 NumericType 常量，内容层纪律上建议加 GetUnitHp 薄门面，§8） | demo 取距离内 HP 最高；实现期加 10 行门面即可 |
| 幻鬼 480ms 出现窗后延时下劈 | Area 无"激活延迟"字段——用技能 OnUpdate 时间驱动绕过（本档方案，零框架改动） | 480ms 后 CreateArea（已在 §5） |
| 目标位置跨帧保持 | LSCast 无坐标存储门面 | SubState 存目标 unitId，OnUpdate 重查当前位置（回滚安全，§5） |
| 命中强制控制 hold（static[2]=1 时：定身+禁抓+结束击倒） | 数据关闭 + 抓取/微控缺失档 | 不做 |
| BLADESPIRIT(123) 动画提速 | 精通系统缺失 | 固定 1.0 |
| 闪屏（黑闪 500/2000/500）/屏震 10,300/音效 | 延后 | 跳过 |
| 在场幻鬼实体销毁重建 | 幻鬼锚点实体缺口（R2-A8） | 不做，独立索敌（表现差异：无幻鬼站位记忆） |
| 运行时实载 spinningslashvs2.atk（错位） | 本 pvf mod 数据问题 | 采用意图 atk（066 裁定同款） |

## 8. 存疑与缺口上报

- **未考证**：①state 14（可施放状态之一）具体语义；②start_body F1=10002、speedslashvs.ani F2=10001 无消费者（引擎侧或残留）；③suddenstrikevs_hold.ani / suddenstrikevs_white.ani 的引用者（白名单内无命中，疑备用/引擎侧）；④错位插入点精确位置（case 62/63 之间，未逐 code 验证）。
- **F5 -2 错位修正案反向核验（给主循环并入轮间经验）**：case 65（id 54 奈落）code 38 → 运行时实载 **spinningslashvs2.atk**、意图 #40 suddenstrikevs.atk，-2 规则成立且**族下界由 66 下探到 65**；case 63（spinningslash）code 36/37 亦错位；case 61/62 直读正常——修正 F5 表述为"**VS 族错位覆盖 case 63-74（至少）**，61/62 及波动族直读"。
- **新翻译节**：`[INTERPOLATION]`（suddenstrikevs_end_body.ani F24）——首次出现，建议并入累计清单。
- **建议小门面**：`SkillContext.GetUnitHp(LSUnit)`（敌方 HP 读取，索敌类技能通用）。
- **给下轮的经验**：ghostsword 族 VS 系"强制控制开关"都在 static data 末列（本技能 static[2]、windspiritvs static[2] 同构），读 skl 时先看 static 再找 onattack 挂 appendage 的分支；索敌型 PO 的目标查找全在 setstate state 10（sq_FindFirstTarget 四参），索敌范围/前移距离都来自 static。
