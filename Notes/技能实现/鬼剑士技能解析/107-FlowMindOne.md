# 流心 : 刺（FlowMindOne）

> 技能ID 107 | 级别 A | 可实现性 🔶（地面突刺基本版可直接；空中版依赖跳跃系统缺口；武器差异需简化） | 分析日期 2026-08-22 | 批次 A6

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 流心 : 刺 | `skill\Swordman\FlowMindOne.skl [name]` |
| 英文名 | FlowMindOne（skl 文件名；[name2] 实测 `Flow Heart : Stab`） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1） | 同上 |
| 学习等级 | 20（前置：105 流心 Lv1） | 同上 [pre required skill] |
| 最高等级 | 70（growtype maximum：剑魂位 50） | 同上 |
| 类型 | active（skill class 1） | 同上 |
| 指令 | （流心 动作中）X（`{6=(ATTACK)}`；mod 钩子另允许站立/走路直接进） | 同上 [command] + common.nut |
| CD | **无 [cool time] 节**；[auto cooltime apply] 0 + [cooltime level info] 1 → **推断 CD=等级表列 1**（5000ms→103ms 随等级递减，见下） | 同上 + 推断 |
| MP | 30-280 | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data（dungeon 13 值） | `300 50 2 1 1 2 70 3000 450 300 -1500 650 200`；已解出（见 §2.4）：[2]=基本多段次数 2、[5]=太刀光剑段数基准 2（×2=4 段）、[6]=太刀光剑多段威力 70%、[7]=钝器眩晕时长 3000ms、[8]=巨剑短剑追加捶击 450%、[12]=异常增伤 200×0.1=20%；其余（300/50/1/1/300/-1500/650/200）为引擎状态 62 内部消费，**未考证** | 同上 [static data] + level property 解码 |
| 一句话效果 | 突刺前方敌人（多段 2 次）；钝器附加眩晕、太刀/光剑加多段、巨剑/短剑按 Z 追加下斩；另有空中落下突刺形态 | 同上 [explain] |

**level property（4 列 + 9 向量，Lv1→Lv70）**：col0 = 217→1676（普通攻击力 %，空中段 nut 实证 `sq_GetBonusRateWithPassive(107, 状态, 0, 1.5)` 读列 0）；col1 = 5000→103（递减，按 [cooltime level info] 1 推断为 CD ms，**未确证**）；col2 = 330→1454（×0.1 = 钝器眩晕机率 33%→145%）；col3 = 22→160（眩晕 Lv，每级+2 实测吻合）。
向量解码法（L21 细化，本技能三例实证）：`(-1, colIdx, scale)` → 等级列×系数；`(slotIdx, slotIdx, scale)` → static[slotIdx]×系数——`(7,7,0.001)`→3000ms=3 秒、`(6,6,1.0)`→70%、`(12,12,0.1)`→20% 与模板文案逐条对上。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**地面形态（状态 62）无任何注册、无专属状态 nut**——引擎内置状态（load_state 72 条逐一核对；`sqr\character\swordman\weaponmaster\flowmind\flowmindone.nut` 28 行仅有 `onProc_FlowMindOne` 取消钩子，非状态主体）。行为重建靠 .ani 标记 + .atk + 同族参照（F3 走读法）。

**空中落下形态（状态 147）有完整脚本**：

```
139: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/flowmind/flowmindonefallstate.nut", "FlowMindOneFallState", STATE_FLOW_MIND_ONE_FALL_STATE, 105);
```

（第 5 参写 105 而非 107——mod 疑点：该状态只被 107 的入口逻辑使用，记 §8。）

**入口（双轨，均为 mod 钩子）**：
- `swordman_common.nut` procAppend_Flowmind_Comminterrupt：地面 z==0 且处于状态 0/14/63（站立/走路/跃收招）→ `SetSkillState(obj, 107, 62, [0,61])`；状态 29（里鬼）300ms 后同；空中 z>30 且跳跃 400-11000ms → 进 147（并 `startSkillCoolTime(107)`）；状态 64（升）中 → 147。
- `jump\swordman_jump.nut` enableFlowMindOneFallState：跳跃中（state 6）武器非 2（非钝器，推断）且未按攻击键 → 流心指令 + `sq_IsUseSkill(107)` → 进 147。两轨功能重叠（mod 叠加，C3 同款现象）。

### 2.2 地面形态（状态 62，引擎内置——按 .ani/.atk/同族重建）

- 施法瞬间（推断）：播 `FlowMindOne.ani`（.chr etc motion #106）、设攻击信息 73（`AttackInfo/FlowMindOne.atk`，.chr etc attack info #73 实测）、按 col0 与武器类型结算。
- 攻击盒（FlowMindOne.ani 实测，min/max 格式）：F3 `35 -19 14 152 38 60` ~ F7 同量级 → 突刺长盒 x∈[35,152]（前伸约 1.5 单位）。
- 多段：ATK 帧 F3-F7 共 5 帧按"基本多段次数 2"分组重置命中（引擎 resetHitObjectList 语义，推断）。
- 武器差异（skl 模板+static 解码）：钝器=命中 33% 概率 3 秒眩晕（col2/col3/static[7]）；太刀/光剑=4 段×70% 威力；巨剑/短剑=Z 键追加下斩（`FlowMindOneAdd.ani` 1330ms + atk 74，触发窗口引擎侧未考证）。
- 流心:狂 窗口内（ap_liuxing 在挂）可 JUMP 键取消进 63（跃）、SKILL2 取消进 64（升）（`weaponmaster\flowmind\flowmindone.nut` onProc 实测）。
- 走读参照：同目录 `flowmindtwo.nut`（跃，202 行）是同族唯一全 nut 化状态脚本，结构可逐回调对照。

### 2.3 空中落下形态（状态 147，flowmindonefallstate.nut 84 行完整走读）

- `onSetState` sub0：停移动；创建特效（pooled `effect\animation\flowmindone\fallstate_one_leap.ani` 于当前位置 + follow-parent `fallstate_one_start/end.ani`，引擎 var 持有句柄）；设 x 移动 750 速度/1200 加速（`sq_SetStaticMoveInfo(0,750,750,true,1200,true)`）、z 速度 -750→-1200（`sq_SetZVelocity`）；播 `CUSTOM_ANI_FLOW_MIND_ONE_FALL_STATE`（=176 → `animation/flowmindonefallstate.ani`）；**设攻击信息 73**（同地面）+ 攻击倍率 = col0 × **1.5**（`sq_GetBonusRateWithPassive(107, 147, 0, 1.5)`）；攻击速度 1.5 倍。
- `onProc` sub0：`z<=0`（落地）→ 切 sub1（销毁箭头特效、播 `CUSTOM_ANI_FLOW_MIND_ONE_FALL_STATE_END`=188，200ms）。
- `onEndCurrentAni`：回 STATE_STAND。
- `onEndState`：切状态时清理 start/end 特效句柄。
- mod 附加：入场时若挂"极·神剑术(248)"appendage → 授霸体至 248 列 1 时长（`flowmind\flowmindone.nut` 混淆版 onAfterSetState，与 backstep/chargecrash 同款 mod 手笔）。
- 攻击盒（flowmindonefallstate.ani 实测）：**全部 6 帧常驻** `13 -13 -11 88 26 167`——下落纵列盒（z 上限 167px），F5 delay=3000ms 为悬停帧（落地前持续判定）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\FlowMindOne.ani`（etc #106；小写 flowmindone.ani 为同一内容副本，diff 实证） | 9 | 720ms（全 80ms） | 无 | F3-F7 | 突刺主段；仅引 sm_body%04d.img；.als 挂 4 层（见 §3） |
| `character\swordman\animation\flowmindoneadd.ani`（etc #107） | 19 | 1330ms（全 70ms） | 无 | F4-F11 | 巨剑/短剑追加下斩；仅引 sm_body |
| `character\swordman\animation\flowmindonefallstate.ani`（etc #176） | 6 | 名义 3270ms（F5=3000 悬停） | 无 | F0-F5 全帧 | 空中下落刺；.als 挂 4 层 |
| `character\swordman\animation\flowmindonefallstateend.ani`（etc #188） | 1 | 200ms | 无 | 无 | 落地收势 |
| `character\swordman\animation\flowmindonefallone.ani` / `falltwo.ani`（etc #177/#178） | 未逐帧 | — | — | — | 引擎备用变体（177 有 .als 挂 FlowMindTwo attack2 层），本链路未消费，未深读 |
| `character\swordman\effect\animation\flowmindone\*.ani`（特效层） | dash_dodge 7帧480ms / move_shadow_normal 4帧320ms / short_heavy_add_dodge 6帧480ms / fallstate_one_leap 5帧400ms / fallstate_one_start 2帧160ms / fallstate_one_end 3帧240ms | — | — | 无 | 突刺残影/阴影/追加闪光/下落三件套 |
| `character\swordman\effect\animation\flowmindtwo\*.ani`（被空中段 .als 借用） | attack1_normal/dodge 5帧400ms、fallstate_two_air 4帧320ms、fallstate_two_leap 5帧400ms | — | — | 无 | 空中段 .als 叠层来源 |

`.als` 边车（4 个，实测）：
- `flowmindone.ani.als`：`dash_dodge`@F2 z10000 + `move_shadow_normal`@F0 z-1 + `shadow_dodge1`@F0 z-2 + `shadow_dodge2`@F0 z-3（move_shadow_dodge1/2 引 `Effect\DashAttackMultiHit\thrust_under/upper.img`，跨目录复用）。
- `flowmindoneadd.ani.als`：`add_dodge`@F4 z10000。
- `flowmindonefallstate.ani.als`：attack1_normal z10000 / attack1_dodge z10001 / fallstate_two_air z10003 / fallstate_two_leap z10004（全 F0）。
- `flowmindonefallone.ani.als`：attack2_dodge1/2（本链路未消费）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMindOne.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMindOne.skl` | ✅（240 行） | 4 列等级数据 + 13 static |
| 注册行 | —（地面 62 无注册） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | 空中 147 有：139 行（第 5 参 105，mod 疑点） |
| 主 nut（空中） | flowmindonefallstate.nut | `…\pvf\sqr\character\swordman\flowmind\flowmindonefallstate.nut` | ✅（84 行） | 空中段全逻辑 |
| 钩子 nut | flowmindone.nut ×2 | `…\swordman\weaponmaster\flowmind\flowmindone.nut`（28 行，取消钩子）/ `…\swordman\flowmind\flowmindone.nut`（19 行，混淆 mod 霸体） | ✅ | 见 §2.2/2.3 |
| 入口钩子 | swordman_common.nut / swordman_jump.nut | `…\sqr\character\swordman\` | ✅ | 62/147 双轨入口 |
| .chr 条目 | etc motion #106/#107/#176/#177/#178/#188；etc attack info #73/#74 | `…\pvf\character\swordman\swordman.chr`（1079/1080/1149-1161/1367/1368 行） | ✅ | 动画与 atk 映射 |
| 角色 .ani | FlowMindOne / flowmindoneadd / flowmindonefallstate(+end) 等 | `…\pvf\character\swordman\animation\` | ✅ | 见 §2.4 |
| 角色 .atk | flowmindone.atk / flowmindoneadd.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 见下 |
| 特效 .ani/.als | flowmindone\（6+3 个 .als）/ flowmindtwo\（借用 4 个） | `…\pvf\character\swordman\effect\animation\` | ✅ | 突刺残影/下落特效 |
| 装备层 | flowmindone*.ani 等流心系 | `…\pvf\equipment\character\swordman\avatar\belt\belt_a\`（ls 含 flowmindone/oneadd/fallone/fallstate/fallstateend/falltwo/jumpattack） | ✅ 存在 | 换装图层 |
| 关联技能 | 105 FlowMind / 108 FlowMindTwo / 109 FlowMindThree / 110 FlowMindPowerUp / 248 极·神剑术 | `…\pvf\skill\Swordman\` | ✅ | 前置/连携/霸体 mod |

**.atk 实测**：`flowmindone.atk`：physic / weapon damage apply 1 / damage reaction=damage / attack direction=hit horizon / hit info scale `-1 1.3`（武器伤害 1.3 倍基准）/ hit wav R_BEAMSWDA_HIT，**无 push/lift**（突刺不击退不浮空）。`flowmindoneadd.atk`：同构，scale `-1 1.5`，wav JAKYEOL_HIT。

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 突刺/追加/下落角色帧 | 必需（共享） | ✅ 已入库 |
| Character/Swordman/Effect/FlowMindOne/dash_dodge.img | sprite_character_swordman_effect_flowmindone.NPK | 突刺残影（.als F2） | 可选（地面版视觉主层，建议提取） | ❌ |
| …/FlowMindOne/move_shadow_normal.img | 同上 | 突刺阴影 | 可选 | ❌ |
| …/FlowMindOne/short_heavy_add_dodge.img | 同上 | 追加下斩闪光 | 可选 | ❌ |
| …/DashAttackMultiHit/thrust_under.img、thrust_upper.img | sprite_character_swordman_effect_dashattackmultihit.NPK | 阴影层 2/3（跨目录复用） | 可选 | ❌ |
| …/FlowMindOne/fallstate_one_leap.img、fallstate_one.img | sprite_character_swordman_effect_flowmindone.NPK | 空中段 nut 特效（leap/start/end 共用 fallstate_one.img） | 可选（空中版时升必需） | ❌ |
| …/FlowMindTwo/attack1_normal.img、attack1_dodge.img、fallstate_two_air.img、fallstate_two_leap.img | sprite_character_swordman_effect_flowmindtwo.NPK | 空中段 .als 四叠层 | 可选（同上） | ❌ |

**缺失 img：必需 0 张、可选 11 张（地面 5 + 空中 6），分属 3 个 NPK。**

## 5. 实现方案草案（号段：SkillIds 16 / AnimIds 61-69 / BuffIds 7，ActionIds 未新增）

### 内容件清单

1. **`DotNet~/Skills/FlowMindOneSkill.cs : SkillLogic`**（同 NormalAttack/BloodBoom 范式；帧号 const + SubState 一次性守卫）
   - `CooldownMs = 5000`（DNF 列 1 推断原值 Lv1 5000ms；demo 固定）；`TotalTimeMs = 0`（自管——突刺 720ms + 可选追加 1330ms 两段）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanFlowMindOne)`；`ctx.ClearHitTargets()`；SubState=0。
   - `OnUpdate`（L19 段间多段：段内帧驱动命中 + 段间 ClearHitTargets）：
     - 段 0（突刺，720ms）：攻击盒走翻译 json 帧数据（F3-F7 自带 attackBox → LSHitboxComponentSystem 帧驱动自动激活，无需 SetAttackHitbox）；在累计 480ms（≈F6）处 `ctx.ClearHitTargets()` 一次 → 两段命中（对应"基本多段 2 次"）。全段过后 SubState=1。
     - 段 1（追加窗口，720-1100ms）：`ctx.PeekBufferedButton()==<Z键/技能键>` → 消费 + `ctx.PlayAnim(AnimId.SwordmanFlowMindOneAdd)` + `ctx.ClearHitTargets()` + SubState=2（追加段帧驱动盒 F4-F11 自动生效，动画 1330ms 播完自收）。
     - 段 2 结束（ElapsedMs ≥ 720+1330）→ `OnEnd`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - HitReaction（atk 73 原值无 push/lift；倍率 col0 217%→1676%）：`Damage=100, HitstunMs=500, KnockbackX=0, LaunchY=0`（demo 建议值）。
   - 钝器眩晕（demo 可选）：`ProcBuffId=BuffIds.FlowMindStun, ProcChance=33`（L6 链路，MonsterIceBreath 先例）。
2. **`DotNet~/Buffs/FlowMindStunBuff.cs : BuffDefinition`**（StunBuff 同构）：`TotalTimeMs=3000`（static[7] 原值）、`AddActions={ForbidMoveOn}`、`RemoveActions={ForbidMoveOff}`。
3. **无新 Action**（MeleeHit/ForbidMove 现成）；空中版（167 之前）不做：依赖跳跃系统（R1-A2 已记档缺口），落地后补。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 62 + FlowMindOne.ani | `FlowMindOneSkill` + `AnimId.SwordmanFlowMindOne`（帧驱动攻击盒） |
| 基本多段 2 次（引擎分组重置） | 段中点 `ctx.ClearHitTargets()`（L19） |
| atk 73（1.3 武器倍率/damage/hit horizon） | `HitReaction` 虚属性（Damage 固定值惯例） |
| Z 追加下斩（巨剑/短剑，引擎触发） | SubState=1 输入窗口 + `PlayAnim` 切段（单 cast 内编排，无需跨技能取消） |
| 钝器眩晕（col2 概率/static[7] 时长） | `ProcBuffId/ProcChance` + `FlowMindStunBuff` |
| 空中 147（z 物理/下落盒/1.5 倍率） | ⛔ 跳跃系统缺口——后补 |
| .als 4 层（残影/阴影） | AnimOverlayConfig 视图自动叠加（现有链路） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.FlowMindOne = 16` + ButtonToSkill 新键（如 M） |
| BuffId | `…\Runtime\BuffDefinition.cs` | `BuffIds.FlowMindStun = 7` |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanFlowMindOne=61`、`SwordmanFlowMindOneAdd=62`、`FlowMindOneDashDodge=63`、`FlowMindOneShadowNormal=64`、`FlowMindOneAddDodge=65`；空中段预留 66-69（fallstate/fallstateend/leap/start） |
| json/图集 | LSAnimClipRegistrar / LSAnimResComponentSystem.BuildAtlas | 2 个角色 json + 3 个特效 json + overlay json；图集 3 张（可选 5 张） |
| 按键 | LSOperaComponentSystem | 新键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 无 [cool time]；列 1 推断 5000→103ms 递减 | 5000 固定 |
| 突刺时长/命中帧 | 720ms / F3-F7（5 帧 80ms） | 帧数据直用，段间 480ms 重置一次 |
| 攻击力 | col0 217%→1676%（武器%×1.3） | Damage 100 |
| 多段 | 2 次（太刀光剑 4 次×70%） | 2 次（固定，武器差异不做） |
| 追加下斩 | FlowMindOneAdd 1330ms、威力 450%（static[8]）、atk scale 1.5 | Damage 160、窗口 720-1100ms |
| 眩晕 | 33%→145%（col2×0.1）/ 3s（static[7]）/ Lv22→160（col3） | 33% / 3s / 等级不做 |
| 硬直/击退/浮空 | atk 无 push/lift（damage/hit horizon） | 0 / 0 / 0 |
| 空中段 | 落下 z 速 -750→-1200、x 速 750、倍率 col0×1.5、全帧纵列盒 | ⛔ 后补 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| FlowMindOne.skl | `.skl` 无子命令（4 列 level info + 13 static + 9 向量） | 手抄可行；批量化建议 skl 子命令（累计缺口） |
| flowmindone.atk / flowmindoneadd.atk | `.atk` 无子命令 | 每文件 ~7 值手抄（累计缺口） |
| flowmindonefallstate.ani | F5 `[DELAY] 3000` 悬停帧 | 翻译钳制/手改（同 jump.ani 前例） |
| 各 .als | `[add]` 全帧/帧层号 | 现有 als 子命令全覆盖（含跨目录 use animation 引用，注册侧做别名→AnimId） |

**本技能翻译缺口 2 类（.skl、.atk）；ani/als 全覆盖。**

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 空中落下突刺（z 物理/落地切段/1.5 倍率） | **缺失：跳跃系统**（R1-A2 已记档，本技能再+1例） | demo 只做地面版；跳跃系统落地后按 §5 补 |
| 武器类型差异（钝器眩晕/太刀光剑 4 段/巨剑短剑追加） | **缺失（新缺口）：武器类型概念**——仓库无武器 subType 数据通道 | demo 固定"基本 2 段 + Z 追加"；眩晕按固定 33% 演示 |
| 经流心架势/其他技能取消进入 | 缺失：技能取消体系（105 文档 §8 已补证） | 独立施放（或并入 105 合并技能） |
| 流心:狂 暴击窗口取消进 63/64 | 缺失：暴击消费链（R1-A4） | 跳过 |
| 攻击速度 1.5 倍（空中段） | 延后：动画速度通道（AnimComponent.Speed 已有，但技能无门面） | 空中版后补时用帧表直译即可 |
| 极·神剑术霸体授护（mod） | 延后：霸体帧 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：col1 精确语义（CD 推断依据仅 [cooltime level info] 1 节名）；static 13 值中 7 个（300/50/1/1/300/-1500/650/200）；引擎状态 62 的多段重置精确帧；追加下斩的触发窗口边界；fallone/falltwo 变体用途；flowmindtwo.nut 的加载机制（状态 63 无注册但有完整脚本，推断引擎按状态名绑定同目录 nut）。
- **新系统级缺口**：**武器类型差异化**（107 首撞）——同一技能按 weaponSubType 分支完全不同效果（眩晕/多段/追加），引擎与 common 钩子遍布此分支；我们无武器类型数据通道。归 §6.3 缺失档，建议与"武器系统"一并立项（影响全部剑魂武器技）。
- **mod 疑点**：状态 147 注册行第 5 参为 105（应为 107）；`flowmind\flowmindone.nut` 为混淆 mod（与 backstep/chargecrash 同手笔，绑 248 霸体）；入口双轨（common.nut 与 jump.nut 重叠）。
- **给轮间经验（L21 细化）**：level property 向量三元组新解：`(-1, 列号, 系数)` = 等级列×系数；`(槽号, 槽号, 系数)` = static[槽]×系数。107 三例实证（(7,7,0.001)→3s、(6,6,1.0)→70%、(12,12,0.1)→20%）；021-IceWave 的 [-1,4,0.1] 按此应读"列4×0.1"而非"基准4+0.1/级"，建议主循环回补。
