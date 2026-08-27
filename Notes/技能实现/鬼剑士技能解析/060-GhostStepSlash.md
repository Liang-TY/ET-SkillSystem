# 鬼影闪（GhostStepSlash）

> 技能ID 60 | 级别 A | 可实现性 🔶（基础版=独立前冲刺斩+禁锢；"鬼影步无敌中才可发动"依赖前置状态门，绕过） | 分析日期 2026-08-22 | 批次 A12

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼影闪 | `skill\Swordman\GhostStepSlash.skl [name]` |
| 英文名 | GhostStepSlash（skl 文件名；本例 [name2]="Shadow dodge" 恰为英文） | 同上 [name2] |
| 职业 | 鬼泣（[skill fitness growtype]=2；L17 映射 2=鬼泣） | 同上 |
| 学习等级 | 40（前置：技能 112 TripleStab 鬼影三击剑 Lv1——注：explain 说的施放载体是[鬼影步]，前置表却是 112） | 同上 [required level] / [pre required skill] |
| 最高等级 | 50（鬼泣段上限 30） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 3，魔法武器效果） | 同上 [type] / [skill class] / [weapon effect type] |
| 指令 | ←→→ + Z（指令 MP 优惠 20%/40%） | 同上 [command] / [skill command advantage] |
| CD | 20000 ms（pvp 起手 CD 30000） | 同上 [cool time] / [pvp][start cool time] |
| MP | 200 → 1680（Lv1→Lv67） | 同上 [consume MP] |
| 特殊消耗 | 无（消耗品 3037×1 为无色小晶块惯例） | 同上 [consume item] |
| static data | `300`（= 移动距离 300px，见 level property 第 2 向量 `0 0 1.0` 直取 static 槽 0，L21 解码法） | 同上 [static data] + [level property] |
| 一句话效果 | 极速前移一段距离并斩击敌人；被斩者进入禁锢，禁锢结束时附加暗属性伤害；只能在鬼影步前冲无敌时使用 | 同上 [explain] |

**level property（2 列模板，Lv1 → Lv67）**：魔法攻击力 = level col0：`5692 → 46318`（向量 `-1 0 1.0`）；移动距离 = static[0]：`300px` 恒定（向量 `0 0 1.0`）。
[feature skill index] 226 = GhostStepSlashEx（TP 强化版，E 类批另行分析）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**`swordman_load_state.nut` 无本技能注册行**（grep `ghoststepslash` 无命中；按状态号 33/技能号 60 双查均无）。**老一代引擎内置技能**（F3 形态），
状态号 **33** 的直接证据来自 mod 中断系统：

```
// sqr/character/swordman/appendage/ap_swordman_comminterrupt.nut（case 2 鬼泣区，skill 252 习得后生效的"强制中断"）
EnableSoften(obj, 60, 33); //鬼影??
SetSkillState(obj, 60, 33, [0]);
```

- ⚠ 勘误线索：`_轮间经验.md` L2 记"50=鬼影闪"——本文件实证 **技能 60 的状态号是 33**；状态 50 属于技能 87（Blache，`SetSkillState(obj, 87, 50, [0])` 同文件 case 2 区）。建议主循环修正 L2。
- 原版施放门（"只能在鬼影步前冲无敌时"）为引擎内置：鬼影步 = **技能 18**（lst:181-182 `Swordman/GhostStep.skl`；已另有 `018-GhostStep.md` 完整分析——引擎 throw 状态 13 + ap_ghoststep buff + **前冲 300ms 无敌窗口** static[1]），ap_ghoststep.nut 为空壳（实测）。"前冲无敌中按鬼影闪"的输入检测全部在引擎里。
- 动画常量：`swordman_header.nut:188-189` `CUSTOM_ANI_GHOSTSTEPSLASHREADY <- 18`、`CUSTOM_ANI_GHOSTSTEPSLASHMOVE <- 19`。

### 2.2 引擎内置状态行为重建（.ani 标记 + .obj 数据 + atk 三方印证；无 nut 可读，标注推断）

**onSetState（推断）**：播 `GhostStepSlashReady.ani`（.chr etc motion 槽 18，实测 991 行）→ 完毕切 `GhostStepSlashMove.ani`（槽 19，992 行）并在 move 期间高速前移 300px（static[0]）；
设角色攻击信息 etc attack info 槽 21 = `AttackInfo/GhostStepSlash.atk`（实测 1315 行）。
引擎在前冲起点/沿途创建判定 PO 20026（推断——PO 出生点无脚本可证）。

**帧触发**：两个角色 .ani 均**无 SET FLAG、无 ATTACK BOX**（实测帧表见 2.4）——命中完全由 PO 承担，时机未考证（推断=move 开始帧）。

**onEndCurrentAni（推断）**：回待机 state 0。

**无敌帧缺口**：本技能的"施放载体"本身就是鬼影步的无敌前冲（explain 明示）；独立重建时该门不存在——撞"状态前置型技能"+"无敌帧"两个已记档缺口（见 §7/§8）。

### 2.3 被动对象（ghoststepslash.obj，ID 20026，passiveobject.lst:11185 实测）

| .obj 节 | 值 | 说明 |
|---|---|---|
| [name] | `鬼影闪` | |
| [pass type] / [piercing power] | pass all / 1000 | 全穿透（一次扫过路径上所有敌人） |
| [basic motion] | `Animation/GhostStepSlash1.ani` | 6 帧 640ms；**F0 有攻击盒** `-153 -50 15 505 100 95` → min/max 口径 x∈[-153,505] y∈[-50,100] z∈[15,95]（≈6.6×1.5×0.8 单位的横带，覆盖整条前冲路径） |
| [attack info] | `AttackInfo/GhostStepSlash.atk` | 见下 |
| [add object effect] | `Animation/GhostStepSlash2.ani` @层 1 | 纯视觉叠加层（同为 k-light.img 帧 0-5，无攻击盒） |
| [object destroy condition] | on end of animation | 播完即毁（640ms） |

- **无 PO 行为 nut**（`ap_ghoststepslash.nut` 不存在，appendage/ 目录 grep 实测）——PO 行为引擎内置。
- 变体：`GhostStepSlash_DS.obj`（ID 20090，暗帝/剑影系变体）、`ActionObject/Monster/Magneus/GhostStepSlash*.obj`（怪物用，与本技能无关）。
- **禁锢**：两个 .atk 的 damage reaction 均为 `none`（无受击反应）——被斩者不吃硬直动画、直接进入引擎内置"禁锢"状态，禁锢到期时附加暗属性伤害（explain；引擎行为，无脚本/数据可读，时长与伤害值**未考证**）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/ghoststepslashready.ani` | 2 | 240ms（160+80） | 无 | 无 | 前冲预备姿势；仅引 sm_body%04d.img |
| `character/swordman/animation/ghoststepslashmove.ani` | 3 | 400ms（80+80+240） | 无 | 无 | 前冲斩击本体；仅引 sm_body |
| `passiveobject/.../ghoststepslash1.ani`（PO 主层） | 6 | 640ms（240+80×5） | 无 | **F0**（数值见 2.3） | 引 `Effect/GhostStep/k-light.img` 帧 0-5 |
| `passiveobject/.../ghoststepslash2.ani`（PO 叠加层） | 6 | 640ms | 无 | 无 | 同 k-light.img |
| `effect/animation/ghoststepslash/dust.ani` | 5 | 450ms | — | — | 引 `Effect/HardAttackCharge/dust.img`（跨目录复用，L14） |
| `effect/animation/ghoststepslash/move.ani` | 3 | 400ms | — | — | 引 `k-speed.img`（实测） |
| `effect/animation/ghoststepslash/skull.ani` | 7 | 1240ms | — | — | 引 `k-soul.img`（鬼影骷髅残影） |
| `effect/animation/ghoststepslash/slash1.ani` / `slash2.ani` | 2 / 1 | 200 / 200ms | — | — | 引 `k-light-1/-2.img`（斩击弧光） |

`.als` 边车：本技能全部文件**均无**（角色/被动对象/特效三侧 ls 实测）。
`[pvp]` 变体：ghoststepslash1.[pvp].ani / ghoststepslash1_ds.[pvp].ani 存在（PvP 专用节奏，demo 不用）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GhostStepSlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GhostStepSlash.skl` | ✅ | 技能数据（CD/MP/2 列等级数据/static 300） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置状态 33） | 唯一入口证据 = ap_swordman_comminterrupt.nut 的 SetSkillState(60, 33) |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（grep ghoststepslash 仅命中 header 常量） | ⛔ 缺失 | 角色逻辑在引擎 |
| PO 定义 | ghoststepslash.obj（+_ds） | `…\pvf\passiveobject\character\swordman\ghoststepslash.obj` | ✅ 实测 | 判定 PO（ID 20026） |
| PO 行为 nut | —（不存在） | `…\pvf\sqr\character\swordman\appendage\`（ap_ghoststepslash 不存在；ap_ghoststep 为空壳） | ⛔ 缺失 | 引擎内置 |
| .chr 条目 | etc motion #18/#19 + etc attack info #21 | `…\pvf\character\swordman\swordman.chr` 991/992/1315 行 | ✅ 实测 | GhostStepSlashReady/Move.ani；GhostStepSlash.atk |
| 角色 .ani | ghoststepslashready/move.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 帧表见 2.4 |
| 角色 .atk | GhostStepSlash.atk | `…\pvf\character\swordman\attackinfo\GhostStepSlash.atk` | ✅ 实测 | magic/暗属性/reaction none/push0/lift0/hit horizon/blood 10/1.0 |
| PO .atk | GhostStepSlash.atk | `…\pvf\passiveobject\character\swordman\attackinfo\GhostStepSlash.atk` | ✅ 实测 | magic/reaction none/push0/lift0/hit horizon/cut/blood 0/1.0 |
| PO .ani | ghoststepslash1/2.ani（+_ds、[pvp]×2） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 判定+视觉 |
| 特效 .ani | dust/move/skull/slash1/slash2.ani | `…\pvf\character\swordman\effect\animation\ghoststepslash\` | ✅ 实测 | 引擎绘制弧光/残影（无引用者） |
| 装备层 | ghoststepslashready/move.ani | `…\pvf\equipment\character\swordman\avatar\`（belt_a 实测 2 件；全变体未遍历） | ✅ 实测 | 换装图层（demo 不需要） |
| 关联强化 | GhostStepSlashEx.skl（技能 226） | `…\pvf\skill\Swordman\GhostStepSlashEx.skl` | ✅ 实测 | E 类批次另行分析 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集（%04d 单图集，L16） | 必需（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` |
| GhostStep/k-light.img | sprite_character_swordman_effect_ghoststep.NPK | PO 判定层视觉（帧 0-5） | **必需** | ❌ |
| GhostStep/k-light-1.img / k-light-2.img | 同上 | 斩击弧光 slash1/slash2 | 可选 | ❌ |
| GhostStep/k-soul.img | 同上 | 骷髅残影 skull.ani | 可选（招牌视觉，建议保留） | ❌ |
| GhostStep/k-speed.img | 同上 | 前冲速度线 move.ani | 可选 | ❌ |
| HardAttackCharge/dust.img | sprite_character_swordman_effect_hardattackcharge.NPK | 落地尘土 dust.ani | 可选 | ❌ |

缺失 img：必需级 1 张（k-light.img），可选级 4 张，分属 2 个 NPK。img 版本红线（v2/v4 可/v5 不可）由提取时把关。

## 5. 实现方案草案

### 内容件清单（继承真实基类；数值 DNF 原值 + demo 建议值并列）

1. **`DotNet~/Skills/GhostStepSlashSkill.cs : SkillLogic`**（ReleaseWaveSkill 位移范式 + BloodBoomSkill SubState 守卫范式）
   - `CooldownMs = 20000`（DNF 原值直用）；`TotalTimeMs = 700`（ready 240 + move 400 + 余量 60）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanGhostStepSlashReady)` + `ctx.ClearHitTargets()`；SubState=0。
   - `OnUpdate` 两段推进：`CurrentFrameIndex() >= 2`（ready 播完）且 SubState==0 → `ctx.PlayAnim(AnimId.SwordmanGhostStepSlashMove)`、`ctx.CreateAreaInFront(AreaIds.GhostStepSlash, FP.Zero)`（判定区放在自身位置，盒覆盖前方路径）、SubState=1；
     SubState==1 期间按 ReleaseWave 纯函数位移：`MoveCasterForward(3单位 × dt/400ms)`（300px→3 单位，前冲集中在 move 期 400ms）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - 无敌帧：不做（缺口，见 §7）——demo 版即"可被打断的快速位移斩"。
2. **`DotNet~/Areas/GhostStepSlashArea.cs : AreaDefinition`**（BloodBoomArea 范式）
   - `TotalTimeMs=640`（PO 动画时长）、`TickTimeMs=0`、`EnterActions={MeleeHit, AddHoldBuff}`、
     `HalfExtents=(33/10, 75/100, 40/100)`（PO F0 盒 x[-153,505] 的半宽≈3.3、y 半深 0.75、z 高 0.8；中心偏前由 CreateAreaInFront 距离调整，demo 取 `distance=(FP)17/10` 让盒前缘贴 5 单位处）、
     `HitReaction{Damage=110, HitstunMs=0, KnockbackX=0, LaunchY=0}`（原值 push0/lift0/reaction none：不打断姿态，禁锢接管）、
     `ViewAnimId=AnimId.GhostStepSlashPo`（ghoststepslash1 层）+ `ViewBackAnimId=AnimId.GhostStepSlashPo2`（ghoststepslash2 叠加层，boomback 同构用法）。
3. **`DotNet~/Buffs/HoldBuff.cs : BuffDefinition`**（复制 FreezeBuff：ForbidMoveOn/Off 定身）
   - `TotalTimeMs=1500`（DNF 禁锢时长未考证，demo 取 1.5s）、`AddActions={ForbidMoveOn}`、
     `RemoveActions={ActionIds.HoldEndDarkBurst}`——"禁锢结束附加暗属性伤害"直接落在 Buff 移除时点。
4. **`DotNet~/Actions/AddHoldBuffAction.cs : LSAction`**（AddBleedBuffAction 同构，~10 行）+ **`Actions/HoldEndDarkBurstAction.cs : LSAction`**（MeleeHitAction 精简版：`ctx.DamageOwner(60)` 固定暗属性伤害，demo 值）。
   - 或更省：禁锵用 `HitReaction.ProcBuffId=BuffIds.Hold + ProcChance=100`（MonsterIceBreath 先例），仅 HoldEndDarkBurst 需要新建。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 33 + ready/move.ani | `GhostStepSlashSkill` + 两段 PlayAnim（帧号 const） |
| static[0]=300px 前冲 | `MoveCasterForward` 纯函数位移（ReleaseWave 同构，px÷100=单位） |
| 判定 PO 20026（pass all + 大横带盒） | `GhostStepSlashArea`（一次 Enter 结算=单次横扫） |
| 禁锢（reaction none + hold） | `HoldBuff`（FreezeBuff 同构定身）+ HitstunMs=0 |
| 禁锢结束暗属性伤害 | HoldBuff.RemoveActions = HoldEndDarkBurstAction |
| 鬼影步无敌门（引擎） | 不做（前置状态门缺口；demo 独立施放） |
| effect 弧光 slash1/slash2（引擎绘制） | Area `ViewAnimId` 双层 + 可选 overlay 手组装 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.GhostStepSlash = 19` + `ButtonToSkill` case（新键） |
| AreaId | 同包 `Runtime/AreaDefinition.cs` | `AreaIds.GhostStepSlash = 9`（A12 段） |
| BuffId | 同包 `Runtime/BuffDefinition.cs` | `BuffIds.Hold = 9` |
| ActionId | 同包 `Runtime/LSAction.cs` | `ActionIds.AddHoldBuff = 12`、`ActionIds.HoldEndDarkBurst = 13` |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanGhostStepSlashReady=77`、`SwordmanGhostStepSlashMove=78`、`GhostStepSlashPo=79`、`GhostStepSlashPo2=80`（可选 skull=81） |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×4-5 |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 新增 `k-light.img.bytes`（必需）；可选 k-soul/k-speed/k-light-1/2/dust |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms（pvp 起手 30000） | 20000 直用（或演示缩短 8000） |
| 总时长 | ready 240 + move 400 = 640ms | 700 |
| 前冲距离 | static[0]=300px | 3 单位 / 400ms 匀速 |
| 伤害 | 魔法攻击力 5692%（Lv1）→ 46318%（Lv67），武器魔法基数结算 | MeleeHit 固定 110 |
| 命中反应 | 两侧 .atk 均 reaction none/push0/lift0（禁锢替代受击） | HitstunMs=0/Kb0/Ly0 + HoldBuff 100% |
| 禁锢时长 | 未考证（引擎内置） | 1500ms |
| 禁锢结束暗伤 | 未考证（引擎内置，暗属性） | 固定 60 |
| 判定盒 | PO F0 x[-153,505] y[-50,100] z[15,95] | HalfExtents (3.3, 0.75, 0.8)，前偏 1.7 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 本技能全部 .ani（角色 2 + PO 2 + 特效 5） | `[SHADOW]`（PO/特效 ani，72 处族）；`[IMAGE ROTATE]`（ghoststepslash1/2.ani PO 层）；`[IMAGE RATE]`/`[INTERPOLATION]`（slash1/slash2 特效）；`[DAMAGE TYPE]`（无——本技能角色 ani 无霸体帧） | [SHADOW]/[IMAGE RATE]/[IMAGE ROTATE] 均为已记档跳过节（视觉延后档）；**[INTERPOLATION]（帧插值开关）未在任何记档清单中——本批上报**，暂跳过无碍逻辑 |
| [GRAPHIC EFFECT]（slash1/slash2 = LINEARDODGE） | **已支持**（L15：graphicEffect 字段，AnimClipData 已消费） | 非缺口 |
| `.atk` ×2 / `.obj` / `.skl` | `.atk`/`.obj`/`.skl` 尚无子命令 | 本技能手抄 ~10 个数值可行；随批量化提级（既有记档） |
| 空占位帧 | ghoststepslash1/2.ani 每帧 `[IMAGE]` 均有实路径 | 无缺口 |

结论：**.ani 资源全部可被现有 ani 子命令翻译**；无 .als；实质缺口 = `.skl`/`.atk`/`.obj` 三类无子命令（累计项，非本技能新增）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 只能在鬼影步前冲无敌中发动 | **前置状态门**（R1-A1 记档"状态前置型技能"）+ **无敌帧**（R1-A5 记档 UNBREAKABLE） | demo 独立施放（普通主动技）；待跳跃/状态机立项后回补 |
| 禁锢状态（无受击反应的定身 + 到期暗伤） | 无引擎 hold——但 Buff 定身机制已有（FreezeBuff 先例） | HoldBuff（ForbidMove 定身 1.5s）+ RemoveActions 到期暗伤，手感接近 |
| 刺击伤害按魔法攻击力%结算 | 属性数值无伤害消费链（R1-A4 最重缺口） | MeleeHit 固定值（demo 惯例） |
| 暗属性伤害 | 元素属性系统缺失档 | 固定值直伤，忽略属性 |
| 弧光/骷髅/速度线特效（引擎绘制） | 延后（无声明式来源，064 同类） | k-light 双层做主视觉；k-soul/k-speed 作可选 overlay |
| 音效（R_SQUARESWDC_HIT） | 延后（无音频） | 跳过 |
| 移动撞墙止损 | 延后（无地图碰撞） | 超界不管（与 ReleaseWave 同现状） |

## 8. 存疑与缺口上报

**未考证项**
1. 状态 33 的引擎内部流程（PO 创建时机/是否随角色推进/命中次数——F0 单盒推断为单次横扫结算）。
2. 禁锢的时长与到期暗伤的数值来源（引擎内置，skl 2 列 level info 均为攻击力%与距离，无禁锢参数）。
3. [pre required skill] 112（TripleStab 鬼影三击剑）与 explain 所述[鬼影步]（技能 18，见 `018-GhostStep.md`）的对应关系（前置表指向 112 而非 18，本 pvf 的 ID 表或与官版有出入）。
4. effect/ghoststepslash 各特效 ani 与动画帧的精确挂接关系（无 .als、无引用者）。
5. `L2 状态 50=鬼影闪` 记载与实测冲突（本文件实证 33 才是鬼影闪状态、50=Blache 87）——建议主循环修正。

**新系统级缺口上报**：无新增（前置状态门/无敌帧/属性消费链均已在缺口累计中）。

**翻译工具缺口**：`[INTERPOLATION]` 节（帧插值，本批 4 技能 16 个 ani 出现）不在 README 规则表与跳过清单中——建议列入"未识别节跳过"清单；其余（[SHADOW]/.skl/.atk/.obj）为既有记档。
