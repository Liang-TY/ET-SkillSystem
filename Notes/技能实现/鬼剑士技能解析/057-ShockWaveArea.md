# 邪光波动阵（ShockWaveArea）

> 技能ID 57 | 级别 B（预分类；实为 A 类主动攻击·持续阵，见 §8 纠偏） | 可实现性 🔶（阵本体多段+爆炸可完整表达；仅"扩散型伤害链"需简化） | 分析日期 2026-08-22 | 批次 B4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 邪光波动阵 | `skill\Swordman\ShockWaveArea.skl [name]` |
| 英文名 | ShockWaveArea（取 skl 文件名；[name2]="Demon light array"） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype] 4；波动系） | 同上 |
| 学习等级 | 40（**前置：技能 52 Lv1** = 挫折意志系） | 同上 [required level] / [pre required skill] |
| 最高等级 | 70（六系各 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type]/[skill class] |
| 指令 | ↑↓↓ + Z | 同上 [command] / [command key explain] |
| CD | 20000 ms | 同上 [dungeon][cool time] |
| MP | 180 → 1512 | 同上 [dungeon][consume MP] |
| 读条 | casting time 400 ms | 同上 |
| 消耗品 | 无色小晶块 ×1（[consume item] 3037 1 1） | 同上 |
| static data | `80 2000 7 7 0`——[2]=**多段攻击次数 7**（模板 (2,2,1.0) 实证）；[0]=80/[1]=2000/[3]=7/[4]=0 语义未考证（[1] 疑阵持续/多段间隔相关） | 同上 [static data] + [level property] 向量 |
| 一句话效果 | 捶击地面产生圆形波动阵，阵内所有敌人受多段魔法伤害+邪光伤害并击飞；对受邪光伤害敌人**周围的敌人**再造成扩散型魔法伤害 | 同上 [explain] |

**level property（5 列，Lv1 → Lv70）**：捶击物理攻击力 col0=`439→…`；多段魔法攻击力 col1=`338→…`；邪光魔法攻击力 col2=`676→…`；扩散型魔法攻击力 col3=`5268→…`（源 -2，固定伤害形态）；**波动阵范围 col4=`400→514px`**；多段攻击次数=static[2]=7 次（全等级恒定）。模板向量 6 条全部可解（L21 法），无未考证列。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
108: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/Standalonewave/Standalonewave.nut", "StandAloneWave", 34, 57);
111: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/shockwavearea/shockwavearea.nut", "swordman_shockwavearea", 31, -1);
```

⚠ 状态网解读（L2 语义，实测三方印证）：
- **状态 31 = swordman_shockwavearea**，第 5 参 -1（不绑定技能）——本技能的捶地施法状态，**回调在引擎内**（shockwavearea.nut 只有 53 行，无 onSetState）。
- 行 108 的状态 34 挂的是**技能 57**（L2：第 5 参才是技能 ID）——即施放技能 57 会被引擎先送入状态 34（StandAloneWave 名下），而 standalonewave.nut 的 `onProcCon` 检测到技能 57 输入后 `sq_AddSetStatePacket(31)` 转入状态 31。**31/34 两个 nut 均为"阵三连"的互跳代理壳**（详见 062 文档 §2.2，本技能侧只需知道：实际捶地行为在引擎内置状态 31 中）。
- 阵三连代理网：31(邪光阵) ⇄ 34(无双波) → 13 THROW(不动明王阵)——三个阿修罗阵技互相技能取消的输入链全在这两个代理 nut 里。

### 2.2 引擎内置施法行为重建（动画 + PO 数据反推）

```
读条 400ms → ShockWaveAreaCast.ani（2 帧 510ms，纯姿态，无攻击盒）
→ ShockWaveAreaSmash.ani（4 帧 425ms）：
   F0 携 3 个攻击盒（实测 86/-20/0/119/40/107 等三组，武器捶地判定）
   → 捶击物理伤害 col0（角色侧 .atk 槽 15 = AttackInfo/ShockWaveArea.atk：physic/push100/lift100）
   → 创建 PO 20021（邪光波动阵本体）
→ onCreateObject_swordman_shockwavearea（shockwavearea.nut 实测）：
   阵 PO(碰撞索引 20021) 一出现 → 施法者立刻回 STATE_STAND（捶完即解控，实测代码）
```

### 2.3 被动对象：邪光波动阵 PO 20021（三相位，L9 结构）

`passiveobject\character\swordman\shockwavearea.obj`（passiveobject.lst:11175 实测 ID **20021**；shockwavearea.nut 的 onCreateObject 以 20021 反向印证）：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [basic motion] | `Animation/ShockWaveAreaHold1.ani`（5 帧，wave-explode.img） | 相位 1：阵持续多段判定 |
| [attack info] | `AttackInfo/ShockWaveArea.atk`（PO 侧） | magic / **down / push 200 / lift 200** / etc（多段+邪光共用） |
| [etc motion] | Hold2.ani（1 帧）、Explosion.ani（5 帧） | 相位 2/3：聚气收束 → 爆炸 |
| [etc attack info] | `AttackInfo/ShockWaveAreaExplosion.atk` | magic / **down / push 200 / lift 200**（爆炸击飞） |
| [pass type]/[piercing power] | pass all / 1000 | 全穿透多目标 |

相位-数值对应（推断，引擎内置无脚本可证）：相位 1 多段×7 次、每跳 col1；收束/爆炸承担 col2（邪光）击飞；col3（扩散型）在爆炸时对**受邪光伤害敌人周围**的额外敌人结算（扩散链，§7）。
范围 col4=400px（Lv1）→ Area 半尺寸约 4.0 单位。

**mod 附加 PO（记档不还原）**：`shockwavearea1.obj`（lst 18661 行 ID **249952**，mod 号段）+ `action/shockwavearea1.act`（TRIGGER：帧 0 检查 300px 内 CHARACTER → SET ACTION STAND）；`shockwavearea111.obj`（**未注册 passiveobject.lst**，孤儿文件，basic motion 借 Vajra 不动明王闪电 ani）。两者是 mod 对扩散链的补丁痕迹。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\ShockWaveAreaCast.ani`（.chr etc #12） | 2 | 510ms（85/425） | 无 | 无 | 起手 |
| `ShockWaveAreaSmash.ani`（.chr etc #13） | 4 | 425ms（85×3/170） | 无 | **F0 ×3**（武器捶地盒） | 捶击 |
| `passiveobject\...\animation\shockwaveareahold1.ani`（PO 相位1） | 5 | 未逐帧加总 | 无 | 无（判定走 atk/引擎） | wave-explode.img |
| `shockwaveareahold2.ani`（PO 相位2） | 1 | — | 无 | 无 | 聚气收束帧 |
| `shockwaveareaexplosion.ani`（PO 相位3） | 5 | 未逐帧加总 | 无 | 无 | 爆炸 |
| `effect\animation\shockwavearea\{cast,smash,area}.ani` | — | — | 无 | 无 | 引擎惯例特效（wave-opening/wave-u/wave-d.img，无脚本引用者） |

角色侧 .atk：`attackinfo\ShockWaveArea.atk`（[etc attack info] 槽 15 实测）：physic / damage / **push 100 / lift 100**——捶击物理那一下。
PO 侧 atk 数值见 §2.3。`.als` 边车：**两侧均无**（ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ShockWaveArea.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ShockWaveArea.skl` | ✅ | 数据（5 列全明） |
| 注册行 | load_state:108/111 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 34(挂57)/31(-1) 代理网 |
| 主 nut | shockwavearea.nut | `…\pvf\sqr\character\swordman\shockwavearea\shockwavearea.nut` | ✅（53 行，无施法回调） | 阵三连互跳代理 + PO20021 出现即回站立 |
| 代理 nut | standalonewave.nut | `…\pvf\sqr\character\swordman\standalonewave\standalonewave.nut` | ✅（67 行） | 技能 57 输入 → 转状态 31（§2.1） |
| .chr 条目 | etc #12/#13（Cast/Smash.ani）+ etc attack #15 | `…\pvf\character\swordman\swordman.chr` 985/986/1309 行 | ✅ 实测 | 施法动画 + 捶击 atk |
| 角色 .ani | ShockWaveAreaCast/Smash.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | ShockWaveArea.atk | `…\pvf\character\swordman\attackinfo\ShockWaveArea.atk` | ✅ | 捶击物理（push100/lift100） |
| PO 定义 | shockwavearea.obj（20021） | `…\pvf\passiveobject\character\swordman\shockwavearea.obj` | ✅ | 三相位阵 |
| PO 定义(mod) | shockwavearea1.obj（249952）/ shockwavearea111.obj（未注册） | 同目录 | ⚠ mod 残留 | 扩散链补丁（不还原） |
| PO .act | shockwavearea1.act | `…\pvf\passiveobject\character\swordman\action\shockwavearea1.act` | ✅（mod 用） | 300px 距离触发 SET STAND |
| PO .atk | shockwavearea.atk / shockwaveareaexplosion.atk（+ _ds ×2） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | down/push200/lift200 ×2 |
| PO .ani | shockwaveareahold1/2、shockwaveareaexplosion、shockwavearea1（空占位） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | 阵视觉三相位 |
| 特效 .ani | effect\animation\shockwavearea\{cast,smash,area}.ani | `…\pvf\character\swordman\effect\animation\shockwavearea\` | ✅ | 引擎惯例特效（无引用者） |
| .als | —（无） | 两侧 animation 目录 | ⛔ 无 | — |
| 装备层 | ShockWaveArea*（未查） | `…\pvf\equipment\character\swordman\avatar\` | 未考证 | 老一代引擎动画惯例无换装层（050 同推） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动画帧 | 必需（共享） | ✅ 已在库 |
| wave-explode.img（`Character/Swordman/Effect/ShockWaveArea/`） | sprite_character_swordman_effect_shockwavearea.NPK | 阵三相位主视觉（hold1/hold2/explosion 共用） | **必需** | ❌ 未入库 |
| wave-opening.img（同目录） | 同上 | 起手特效 | 可选 | ❌ |
| wave-u.img（同目录） | 同上 | 捶地特效 | 可选 | ❌ |
| wave-d.img（同目录） | 同上 | 阵面特效 | 可选 | ❌ |

缺失 img：必需 1、可选 3，**全部同一 NPK**（一次提取全覆盖）。

## 5. 实现方案草案

1. **`DotNet~/Skills/ShockWaveAreaSkill.cs : SkillLogic`**（BloodBoomSkill 帧触发 + FireCircleArea 持续区范式组合）
   - `CooldownMs = 20000`；`TotalTimeMs = 1100`（Cast 510 + Smash 425 + 余量；阵是独立 Area 不占技能时长——PO 出现即解控，DNF 实证）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanShockWaveAreaCast)`；`ctx.ClearHitTargets()`。
   - `OnUpdate`（SubState 守卫）：510ms 处切 `PlayAnim(AnimId.SwordmanShockWaveAreaSmash)`；Smash F0（≈595ms）一次性：`ctx.SetAttackHitbox(前偏 1.0, 半尺寸 (0.5,0.3,0.8))` + `HitActions={MeleeHit}` 捶击物理（技能 HitReaction：Damage=60/HitstunMs=500/KnockbackX=100/LaunchY=100，角色 atk 原值直译）+ `ctx.CreateArea(AreaIds.ShockWaveZone, 0)`（自身中心阵）+ `ctx.DisableAttackHitbox()`；随后 `ctx.PlayDefaultAnim()`（对齐"PO 出现即回站立"）。
2. **`DotNet~/Areas/ShockWaveZoneArea.cs : AreaDefinition`**（FireCircleArea 范式：TickActions 循环伤害）
   - `TotalTimeMs = 2000`（static[1] 疑阵持续 2s，demo 取 2000）、`TickTimeMs = 280`（2000ms÷7 跳 ≈ 285ms，DNF 7 次多段均匀化）、`HalfExtents = (4.0, 0.5, 4.0)`（col4=400px）、`TickActions = { MeleeHit }`、`ViewAnimId = AnimId.ShockWaveHold`（hold1 循环）、`ViewEndAnimId = AnimId.ShockWaveExplosion`。
   - Tick 用 `HitReaction{Damage=45, HitstunMs=400, KnockbackX=200, LaunchY=200}`（PO shockwavearea.atk 原值）——7 跳 ×45 ≈ 315 总伤（col1 338% 折算档）。
3. **`DotNet~/Areas/ShockWaveBurstArea.cs : AreaDefinition`**（阵末爆炸，技能 OnUpdate 定时创建或复用 Zone 的收尾动画——两案：独立 Area 语义更准）
   - `TotalTimeMs = 400`、`TickTimeMs=0`、`EnterActions={MeleeHit}`、`HalfExtents=(4.5,0.6,4.5)`、`HitReaction{Damage=110, HitstunMs=800, KnockbackX=200, LaunchY=200}`（explosion.atk：down/push200/lift200 → 长硬直+双 200 表达击倒手感）、`ViewAnimId=AnimId.ShockWaveExplosion`。
   - 由 Zone 无法在到期时"换 HitReaction 再打一发"（Area 到期只有 ExitActions/收尾动画），故爆炸独立成 Area、由技能 `OnUpdate` 在 `ElapsedMs >= 2700` 时创建（SubState 守卫；时间驱动，L10 惯例）。
   - **扩散型 col3（5268 固定值）简化**：并入爆炸伤害（Damage=110+60）或跳过（§7）。
4. 需要新增的 Action/Buff/Bullet：无。

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.ShockWaveArea = 28` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.ShockWaveZone = 30`、`ShockWaveBurst = 31` |
| AnimId | npkparser `AnimConfigRegistry.cs` | `SwordmanShockWaveAreaCast=135`、`…Smash=136`、`ShockWaveHold=137`（PO hold1）、`ShockWaveExplosion=138`（PO explosion）；可选 139（area.ani 特效） |
| json/图集/按键 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem | 4 个 json + wave-explode.img.bytes（+可选 3 张） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms | 20000 直用 |
| 读条 | 400ms | 跳过（瞬发） |
| 捶击物理 | col0=439%；atk push100/lift100 | Damage 60 / Kb 100 / Ly 100 |
| 阵范围 | col4=400px（Lv1→514） | HalfExtents 4.0 |
| 多段 | 7 次 × col1=338% | 7 Tick × 45 |
| 阵持续 | static[1]=2000（推断） | 2000ms |
| 爆炸 | explosion.atk down/200/200；col2=676%（+col3 5268 扩散） | Damage 110 / 硬直 800 / Kb 200 / Ly 200 |
| 施法总长 | Cast 510 + Smash 425 | TotalTimeMs 1100（阵独立） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `ShockWaveArea.skl` | `.skl` 无子命令 | 手抄 5 列 + static（本技能列义全明，L21 直解） |
| 角色/PO `.atk` ×3 | `.atk` 无子命令 | 手抄（每文件 ~8 值） |
| `shockwavearea.obj` | `.obj` 无子命令 | 不需直译（三相位已手抄为 Area 编排） |
| `shockwavearea1.act` | `.act` 无子命令（mod 残留文件） | 不翻译不还原 |
| 各 .ani | 常规节（无 .als、无 RGBA/特殊节） | 现有 ani 子命令全覆盖 |

结论：动画资源全部可译；缺口为 `.skl`/`.atk`/`.obj`/`.act` 既有四类，无新增节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 扩散型魔法伤害（对受邪光伤敌人**周围**敌人再结算） | **缺失档：目标位置读取门面**（R4-A18 记档） | 并入爆炸 Area 一并结算（同范围全覆盖）；视觉无差异，数值略高 |
| 同段定时多段（阵内 7 跳） | **已通**（L19 三档之中档：Area Tick 无去重天然多段） | 直译 7×Tick |
| 读条 400ms | 延后档（无读条系统） | 瞬发 |
| 阵三连技能取消网（31⇄34→13 互跳代理） | **缺失档：技能取消体系**（064 首报，本技能是第 N 例——且是首个"代理壳 nut"形态） | 不还原；各阵独立施放 |
| 引擎惯例特效 cast/smash/area.ani（无引用者） | 延后（特效缺源，064 同条） | hold1/explosion 已够表现；要还原走手组装 overlay |
| 霸体/屏震（引擎内置捶地惯例如有） | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static[0]=80 / static[1]=2000 / static[3]=7 / static[4]=0 的引擎消费语义（[1] 推断阵持续 2s）。
2. 相位-列对应（col2 邪光 vs col3 扩散挂哪个相位）无脚本可证，按"多段=col1、爆炸=col2+col3 并算"简化。
3. ShockWaveAreaCast/Smash 的装备层变体是否存在（avatar 未查）。
4. 状态 34 挂技能 57 的引擎语义（施放 57 先入 34 再跳 31）——从代理代码反推，未见引擎侧直接证据。

**纠偏**：预分类 B → **实为 A**（active 且主体是攻击判定：捶击 + 阵多段 + 爆炸）。B 类分析深度已超额完成。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **代理壳 nut 形态**（本批首证）：阵三连的 load_state 注册行/状态号与技能归属错位（34 挂 57）、真实施法逻辑在引擎、nut 只做互跳代理——老技能查归属时 pushState 第 5 参也不可靠，需与 header 常量 + .chr/PO 资源三方印证。建议补进 _轮间经验 F 系。
2. 目标位置读取门面第 2 用例（扩散链）——R4-A18 已记，本技能再证。

**翻译工具缺口**：无新增（.skl/.atk/.obj/.act 既有）。
