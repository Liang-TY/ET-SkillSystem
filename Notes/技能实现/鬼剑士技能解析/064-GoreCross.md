# 十字斩（GoreCross）

> 技能ID 64 | 级别 A | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 试点批次（单技能试规范）

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 十字斩 | `GoreCross.skl [name]` |
| 英文名 | Gore Cross | `GoreCross.skl [name2]` |
| 职业 | 鬼剑士共通（六系可学）；「强力追击」分支限狂战士/狱血魔神（=狂战士系） | skl `[skill fitness growtype] 0-5` + explain 文本 |
| 学习等级 | 15 | skl `[required level]` |
| 最高等级 | 70（各觉醒段上限 50） | skl `[maximum level]` / `[growtype maximum level]` |
| 类型 | active（skill class 2） | skl `[type]` |
| 指令 | ←→ + Z（指令施放 MP 优惠 10%/20%档） | skl `[command]` / `[skill command advantage]` |
| CD | 3000 ms（固定，不随等级变化） | skl `[dungeon][cool time] 3000 3000` |
| MP | 17 → 170（Lv1 → Lv70） | skl `[dungeon][consume MP]` |
| 特殊消耗 | 习得[血气旺盛]（技能 63，被动 Lv1）后：MP 消耗 **由 HP 消耗替代**，且十字斩出血机率 +40% | `BloodyVigorous.skl [explain]`（血气旺盛 static data = 400，语义未考证） |
| static data | `150 100 100`（dungeon；语义未考证，引擎内置消费） | skl `[static data]` |
| 一句话效果 | 挥武器十字两连斩，并在身前召唤「血之十字」自地面升起攻击敌人（高机率出血）；狂战士系在召唤瞬间再按技能键可发动击退型强力追击 | skl `[explain]` |

**level property（12 列，Lv1 → Lv70 首末值）**：`90→1561`、`149→1398`、`100→151`、`17→123`、`10000(恒)`、`43→403`、`8→651`、`35→323`、`0(恒)`、`0(恒)`、`167→1571`、`42→362`。
列语义**未考证**（本技能逻辑为引擎内置，无 nut 调用 `sq_GetLevelData` 可读）。旁证：剑影同型技 `atswordman/gorecross.skl` 为 8 列且其 nut 用列 0/2/4/6 作四段攻击倍率——本技能 12 列布局与之不同。仅 `col4=10000` 按 DNF 惯例推断为出血机率万分率（=100%）。TP 强化版 `GoreCrossEx.skl`（技能 141）同 12 列、col8/9 恒 100，其 explain 声明效果为"攻击力+8%/Lv、十字大小+8%/Lv、出血时间+1s/Lv"，仅作语义旁证。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无注册行**（全文件 72 条 pushState 逐一核对，grep `gorecross|gore|, 64` 均无命中）。十字斩属最老一代技能：**角色侧逻辑编译在客户端引擎内**，pvf 只提供数据文件。

> ⚠ **状态号勘误（主循环裁定 2026-08-22，R2-A9 上报）**：原文推断"状态号与技能号同为 64（老技能惯例）"有误——
> `swordman_common.nut:65/74` 实测 `UseSkillState(obj, 109, 64, [0,6,61])` 明确把技能 109（流心:升）切进状态 64，
> **状态 64 = 流心:升**（流心家族 61=架势/62=刺/63=跃/64=升）。下方 `mystate == 64` 代码块注释"处于十字斩状态"
> 应读作"处于流心:升状态，允许取消进流心:刺"。十字斩的真实引擎状态号未考证；
> 但"引擎内置"结论不受影响（无注册行 + 无 nut 两事实独立成立）。

引擎内置的直接证据：

```
// sqr/character/swordman/swordman_common.nut（procAppend_Flowmind_Comminterrupt，中断/连携钩子）
if (mystate == 64) {                       // 处于十字斩状态
    EnableSoften(obj, 107, 147);           // 允许触发 流心·壹（技能 107）取消
    local enterskill = SetSkillState(obj, 107, 147, [0]);
    ...
}
if (mystate == 0 || ... || mystate == 64 || ...)  // 站立/攻击/十字斩等状态可接 流心·贰（技能 108）
    { EnableSoften(obj, 108, 63); SetSkillState(obj, 108, 63, [...]); }
```

```
// sqr/character/swordman/swordman_header.nut
CUSTOM_ANI_GORECROSS <- 25        // = swordman.chr [etc motion] 段第 26 项（实测 998 行 GoreCross.ani，973 行起 0 计数）
```

**同型脚本参照**（行为重建的权威依据）：剑影 `atswordman_load_state.nut:90` 注册了同名技能的完整脚本实现——
`IRDSQRCharacter.pushState(10, "character/atswordman/gorecross/gorecross.nut", "Gorecross", 94, 94)`。
剑影版与狂战士原版机制同构（两段挥砍 + 帧 2 召唤十字 PO + 按键触发追击子状态），其 nut 可逐回调对照阅读（见 2.2）。

### 2.2 引擎内置状态行为重建（以 .ani 标记 + .obj 数据 + 剑影脚本版三方印证）

**onSetState（施法瞬间）——推断**
- 播 `gorecross.ani`（.chr etc motion 槽 25）
- 扣消耗：MP（或血气旺盛习得时 HP）
- 设角色攻击信息：`gorecross1.atk`（第一刀）

**帧触发（gorecross.ani 实测 SET FLAG）**
| 帧号 | 累计时间 | 标记 | 推断语义 |
|---|---|---|---|
| F7 | 350ms | **flag 1** | 第一刀（横斩）命中：`gorecross1.atk`（damage 反应/push 100/lift 100/hit horizon） |
| F14 | 1040ms | **flag 65534** + PLAY SOUND `GORECROSS_ATK3` | 特殊标记（DNF 惯例 0xFFFE = 取消窗口/命中标记，语义未考证） |
| F15 | 1070ms | **flag 2** | 第二刀（纵斩）命中：`gorecross2.atk`（damage/push 100/**lift 200/hit lift up 击飞**）；同帧召唤血之十字 PO（推断） |

**剑影脚本版逐回调（参照实证，`character/atswordman/gorecross/gorecross.nut`）**
- `onSetState` 子状态 0：播挥砍动画、设攻击信息 8、按 skl 列 0 算攻击倍率（`sq_GetBonusRateWithPassive(94,-1,0,…)`）；`sq_StopMove`。
- `onEnterFrame` 帧 2：`sq_SendCreatePassiveObjectPacket(24383, 0, 80, 0, 60)`——在身前 80px、高 60px 处召唤 PO，写入（技能ID, 子类型 0, PO 伤害倍率=列 2）。
- `onProcCon` 帧 ≥3 起：若按住攻击键或技能键尾（`sq_IsKeyDown(OPTION_HOTKEY_ATTACK)` / `isDownSkillLastKey()`）→ 置 var 标记（=追击开关）。
- `onEndCurrentAni` 子状态 0：标记已置 → 切子状态 1（追击段：播收势动画、攻击信息 9、倍率列 4，帧 2 再召唤 PO 子类型 1、倍率列 6）；未置 → 回待机（state 0）。
- 即"召唤瞬间按技能键"实际是**召唤帧起至动画播完前的输入窗口**，剑影版收敛为二段式（本体/追击）子状态机。

**onEndCurrentAni（原版推断）**：回待机 state 0（老技能标准收尾）。

### 2.3 被动对象：血之十字（gorecross.obj，完整实测）

`passiveobject/character/swordman/gorecross.obj`（狂战士版；`gorecross_ds.obj` 为剑影专用变体，数据同构）：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [floating height] | 1 | 悬浮高度 |
| [pass type] / [piercing power] | pass all / 1000 | 全穿透（可打多目标） |
| [basic motion] | `Animation/GoreCross1.ani` | 相位 1：十字闪光升起（4 帧 320ms，F1/F2 有攻击盒） |
| [attack info] | `AttackInfo/GoreCross.atk` | 相位 1 命中：damage 反应/push 30/lift 30/cut+blood 60 |
| [add object effect] | `Animation/GoreCross2.ani` @层 -1 | 叠加视觉层（无攻击盒） |
| [etc motion] | `GoreCross3.ani`、`GoreCross4.ani` | 相位 2：三联十字爆发（4 帧 320ms，末帧 RGBA 淡出）；相位 3：纯淡出（RGBA 64→0） |
| [etc attack info] | `AttackInfo/GoreCrossAdd.atk` | 相位 2 命中：**down 击倒反应**/push 250/lift 300 |
| [object destroy condition] | on end of animation | 播完即毁 |

另有 `gorecrossmercilessness.atk`（同目录，**无脚本/无 .obj 引用**——引擎内置专用）：down 击倒/hit direction front/push 120/lift 300/cut+blood 80。按 explain 与命名（mercilessness=无情）判定为**狂战士系"强力追击"的命中参数**。

被动对象**无独立行为 nut**（白名单内 grep `gore` 仅命中 swordman_header.nut 常量定义；`ap_gorecross.nut` 不存在）——PO 行为同样引擎内置，数据全在 .obj/.atk。

**攻击盒实测**（gorecross1.ani F1/F2，被动对象「偏移+尺寸」格式，与 01§5.5 记档一致）：
- F1：`-83 -25 -96 142 50 193`
- F2：`-68 -25 -68 130 50 133`
按 min/max 口径解读 F1 ≈ x∈[-83,142] y∈[-25,50] z∈[-96,193] → 判定约 2.25×0.75×2.89 单位（÷100，含中轴覆盖身前）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/gorecross.ani`（角色） | 29（F0-28） | 1330ms（F0-8=50，F9=280 蓄，F10=80，F11-13=50，F14-20=30，F21-28=20） | F7=1，F14=65534(+音效 GORECROSS_ATK3)，F15=2 | **无**（引擎按 flag 施加武器判定） | 每帧 2-4 个 damageBox（皮肤/护甲层）；仅引 `sm_body%04d.img` |
| `passiveobject/.../gorecross1.ani`（PO 相位1） | 4 | 320ms（40/140/70/70） | 无 | F1、F2 | GRAPHIC EFFECT `LINEARDODGE`（线性减淡） |
| `passiveobject/.../gorecross2.ani`（PO 叠加层） | 4 | 320ms | 无 | 无 | F0 引 `gorecross_cross.img` 帧10 |
| `passiveobject/.../gorecross3.ani`（PO 相位2） | 4 | 320ms | 无 | 无 | F3 RGBA(255,255,255,128) 半透明 |
| `passiveobject/.../gorecross4.ani`（PO 相位3） | 4 | 320ms | 无 | 无 | RGBA 64→0 渐隐 + LINEARDODGE |
| `character/swordman/effect/animation/gorecross/slash1-4.ani`（挥砍弧光） | 12/13/5/7 | 未逐帧统计 | 无 | 无 | 引擎内置绘制的武器特效（无 .als 边车、无脚本引用，白名单内 grep 实证） |

`.als` 边车：**本技能全部文件均无**（character 与 passiveobject 两侧 animation 目录 ls 实证）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GoreCross.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GoreCross.skl` | ✅ 实测（259 行） | 技能数据（CD/MP/12列等级数据） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ **缺失（引擎内置状态 64）** | 见 §2.1，另有 swordman_common.nut mystate==64 钩子 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（grep gore 全树无） | ⛔ 缺失 | 角色逻辑在引擎；参照剑影版 `…\pvf\sqr\character\atswordman\gorecross\gorecross.nut`（165 行实测） |
| ap nut | —（不存在） | `…\pvf\passiveobject\character\swordman\` | ⛔ 缺失 | PO 行为引擎内置，数据在 .obj |
| .chr 条目 | etc motion #25 + etc attack info #23/#24 | `…\pvf\character\swordman\swordman.chr` 998 / 1317 / 1318 行 | ✅ 实测 | Animation/GoreCross.ani；AttackInfo/GoreCross1.atk、GoreCross2.atk |
| 角色 .ani | gorecross.ani | `…\pvf\character\swordman\animation\gorecross.ani` | ✅ 实测 | 29 帧 1330ms，flag 1/65534/2 |
| 角色 .atk | gorecross1.atk / gorecross2.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 两刀挥砍命中反应 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失（本技能无边车） | — |
| PO 定义 | gorecross.obj（+gorecross_ds.obj 剑影变体） | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 血之十字 PO 结构 |
| PO .ani | gorecross1-4.ani（+_ds ×4；awakening 子目录 4 个为剑影觉醒版） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 十字视觉三相位 |
| PO .atk | gorecross.atk / gorecrossadd.atk / gorecrossmercilessness.atk（+gorecross_ds.atk / gorecrossadd_ds.atk） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | PO 相位1 / 相位2 / 强力追击 |
| 挥砍特效 | slash1-4.ani（+gorecross_ds/ 剑影版） | `…\pvf\character\swordman\effect\animation\gorecross\` | ✅ 实测 | 引擎绘制弧光（无引用者） |
| 装备层 | gorecross.ani ×76 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,coat,face,hair,neck,pants,shoes}\*\` | ✅ 实测（find 计数 76） | 各 avatar 变体图层（只查存在性） |
| 关联被动 | BloodyVigorous.skl（技能 63） | `…\pvf\skill\Swordman\BloodyVigorous.skl` | ✅ 实测 | 血气旺盛：MP→HP + 出血率+40%（逻辑亦引擎内置，passive_skill_swordman.nut 无此技能） |
| 关联强化 | GoreCrossEx.skl（技能 141，TP） | `…\pvf\skill\Swordman\GoreCrossEx.skl` | ✅ 实测 | E 类批次另行分析 |
| 关联取消 | cancelgorecross.skl | `…\pvf\skill\Swordman\cancelgorecross.skl` | ✅ 实测（不在 lst 241 条内） | 强制-十字斩（本文仅记档） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集（%04d 解析为单图集，帧索引 10/201/142/143…） | 必需（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` 已在库 |
| gorecross_obj_cross_ldodge.img | sprite_character_swordman_effect_gorecross.NPK | PO 相位 1 十字闪光主层 | **必需** | ❌ 未入库 |
| gorecross_obj_3cross.img | 同上 | PO 相位 2 三联十字爆发 | **必需** | ❌ 未入库 |
| gorecross_cross.img | 同上 | PO 叠加层 F0 + 弧光 slash2 | 可选 | ❌ 未入库 |
| gorecross_obj_cross_none.img | 同上 | PO 叠加层主体 | 可选 | ❌ 未入库 |
| gorecross_obj_3cross_dodge.img | 同上 | PO 相位 3 渐隐层 | 可选 | ❌ 未入库 |
| gorecross_slash.img | 同上 | 挥砍弧光 1（slash1.ani，12 帧） | 可选 | ❌ 未入库 |
| gorecross_3cross.img | 同上 | 弧光 3（slash3.ani） | 可选 | ❌ 未入库 |
| gorecross_3slash_dodge.img | 同上 | 弧光 4（slash4.ani） | 可选 | ❌ 未入库 |
| （avatar 各层 sm_coat%02d%02da.img 等） | sprite_character_swordman_equipment_avatar_<层>.NPK | 装备换装图层 | 可选（demo 单层 sm_body 即可） | ❌ 未入库（不需要） |

缺失 img：必需级 2 张、可选级 6 张，同属一个 NPK（一次提取全覆盖）。img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案

### 内容件清单（全部继承真实基类；数值 DNF 原值 + demo 建议值并列）

1. **`DotNet~/Skills/GoreCrossSkill.cs : SkillLogic`**（同 BloodBoomSkill 范式：帧号 const + SubState 一次性守卫）
   - `CooldownMs = 3000`（DNF 原值 3000 直用）；`TotalTimeMs = 1400`（角色动画 1330ms；PO 相位2 触发点 = F15(1070ms)+320ms = 1390ms，须留出 10ms 余量——DNF 中 PO 是独立对象不受角色动画时长约束，这里用技能时长兜住）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanGoreCross)` + `ctx.ClearHitTargets()`；血气旺盛向简化：`ctx.ConsumeCasterHp(固定值)`（可选，默认不做）。
   - `OnUpdate` 触发编排（LSCast.SubState 单值推进：0 起 → 1=第一刀已发 → 2=第二刀+召唤已发 → 3=追加追击标记 → 4=相位2爆发已发；追击与爆发互不排斥，故 3 与 4 用 `SubState % 10`/位段或两个独立跃迁均可，实现时二选一）：
     - F7（flag 1）：`ctx.SetAttackHitbox(前偏 ~0.9, 半尺寸 (0.7,0.3,0.6))` + `HitActions = {MeleeHit}` → 第一刀（HitReaction A：demo 伤害 80/硬直 500/push 100/lift 100，原值见 .atk）；发完 `ctx.DisableAttackHitbox()`，SubState=1。
     - F15（flag 2）：第二刀 `SetAttackHitbox`（HitReaction B：demo 伤害 90/硬直 600/push 100/**lift 200 击飞**）+ `ctx.CreateAreaInFront(AreaIds.GoreCross, ~0.9单位)` 召唤血之十字（DNF PO 出生 80px≈0.8 单位）；SubState=2。
     - F15 起至 `TotalTimeMs` 前为**追击输入窗口**：`ctx.PeekBufferedButton() == <本技能键>` → `ctx.ConsumeBuffer()` + 置追击标记 + `ctx.CreateAreaInFront(AreaIds.GoreCrossFinish, 0.9)`（强力追击区，HitReaction D）；追击无独立角色动画（未考证），追击区自带视觉即表现（见 §7）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/GoreCrossArea.cs : AreaDefinition`**（同 BloodBoomArea 范式：EnterActions 单次结算）
   - **PO 两相位 → 两个 Area 顺序创建**（定案）：单 Area 的 TickActions 共用同一份 HitReaction，表达不了"相位1 轻推+出血、相位2 击倒"的参数切换；也不可用 ExitActions 打相位2（Exit 语义=离开/消失，≠ DNF 的击倒时点）。故相位1/相位2 各一个 Area，由技能 `OnUpdate` 按帧/时间分别创建：
   - **`GoreCrossArea`（相位1）**：`TotalTimeMs=320`、`TickTimeMs=0`、`EnterActions={MeleeHit, AddBleedBuff}`、`HalfExtents=(1.1,0.4,1.4)`（PO 攻击盒 F1 折算：x[-83,142]/y[-25,50]/z[-96,193] 的半尺寸）、`HitReaction{Damage=120, HitstunMs=500, KnockbackX=30, LaunchY=30, ProcBuffId=BuffIds.Bleed, ProcChance=100}`、`ViewAnimId=AnimId.GoreCrossFlash`（gorecross1 层）+ `ViewBackAnimId=AnimId.GoreCrossCross`（gorecross2 叠加层，同构 boomback 用法）。
   - **`GoreCrossBurstArea`（相位2/三联爆发）**：技能 `OnUpdate` 按**时间**驱动（`ctx.GetElapsedMs() >= 1390`，SubState 守卫；帧号到不了——1390ms 已越出角色动画末帧）`CreateAreaInFront`；`TotalTimeMs=320`、`EnterActions={MeleeHit}`、`HalfExtents=(1.4,0.5,1.7)`（爆发加大）、`HitReaction{Damage=150, HitstunMs=800, KnockbackX=250, LaunchY=300}`（lift300+长硬直=DNF down 手感，releasewave as-built §5.6-3 同构）、`ViewAnimId=AnimId.GoreCross3Cross`、`ViewEndAnimId=AnimId.GoreCross3CrossFade`（gorecross4 渐隐）。
   - **`GoreCrossFinishArea`（强力追击）**：`EnterActions={MeleeHit}`、`HitReaction{Damage=250, HitstunMs=800, KnockbackX=120, LaunchY=300}`（gorecrossmercilessness.atk 原值 push120/lift300/down）；视图复用 `AnimId.GoreCross3Cross`（或 3cross 加大帧，无专属动画——引擎版追击动画未考证）。
   - 出血 Buff 复用现有 `BuffIds.Bleed`（BleedBuff：3 秒每秒 15；DNF 原值列 5/6 未考证，demo 用现值）。
3. **需要新增的 Action**：无（MeleeHit + AddBleedBuff 全现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 64 + gorecross.ani | `GoreCrossSkill : SkillLogic` + `AnimId.SwordmanGoreCross` |
| flag 1/2 帧触发 | `OnUpdate` 帧号 const（7/15）+ SubState 守卫（bloodboom §4.7-7 同构） |
| 角色侧两刀 .atk | 技能 `HitReaction` + `SetAttackHitbox`（.ani 无攻击盒 → 固定盒路径） |
| 血之十字 PO | 两个 `AreaDefinition` 顺序创建（相位1/相位2） |
| PO 出血（skl col4 几率） | `HitReaction.ProcBuffId/ProcChance` + `LSRng`（已落地） |
| 追击输入窗口 | `PeekBufferedButton/ConsumeBuffer` + SubState=3（剑影版 onProcCon 同构） |
| 血气旺盛 MP→HP | 无 MP 系统：简化为可选 `ConsumeCasterHp` 或忽略（延后） |
| 引擎绘制弧光 slash1-4 | 无 .als：跳过或手组装 overlay（releasewave 手组装先例；先跳过） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.GoreCross = 11` + `ButtonToSkill` case 7（新键，如 N） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanGoreCross=49`、`GoreCrossFlash=50`（gorecross1）、`GoreCrossCross=51`（gorecross2）、`GoreCross3Cross=52`（gorecross3）、`GoreCross3CrossFade=53`（gorecross4）（段值接现有 48 之后顺延） |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×5（swordman_gorecross.json 等） |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 新增 `gorecross_obj_cross_ldodge.img.bytes`、`gorecross_obj_3cross.img.bytes`（必需两张；可选四张一并） |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 7 |
| 翻译 | DnfConfigTranslation ani 子命令 | 角色 1 个 + PO 4 个 json（现有规则全覆盖，见 §6） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 3000ms | 3000（直用） |
| 总时长 | 角色 1330ms（29 帧）；PO 相位2 落在 1390ms | 1400（兜住 PO 相位2） |
| 第一刀 | atk1：damage/push100/lift100；倍率 skl col0（90%+） | 伤害 80/硬直 500/Kb 100/Ly 100 |
| 第二刀 | atk2：damage/push100/lift200 击飞 | 伤害 90/硬直 600/Kb 100/Ly 200 |
| PO 相位1 | gorecross.atk：damage/push30/lift30；出血几率 col4=100% | 伤害 120/Kb 30/Ly 30 + Bleed 100% |
| PO 相位2 | gorecrossadd.atk：down/push250/lift300 | 伤害 150/Kb 250/Ly 300/硬直 800 |
| 强力追击 | mercilessness：down/push120/lift300/blood80 | 伤害 250/Kb 120/Ly 300/硬直 800 |
| PO 出生位 | 剑影版 80px/高60 → 0.8 单位 | CreateAreaInFront 0.9 |
| 攻击盒（角色刀） | 引擎施加，无数据 | 半尺寸 (0.7,0.3,0.6)，前偏 0.9 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `GoreCross.skl` | `.skl` 尚无子命令（12 列 level info + static data） | 本技能手抄 5 组数值可行；建议后续加 `skl` 子命令（至少 dump level info 矩阵 + 头部字段），241 技能批量化时收益大 |
| 5 个 `.atk`（gorecross1/2 + PO 3 个） | `.atk` 尚无子命令 | 本技能手抄（每文件 ~8 值）可接受；随批量化提级 |
| `gorecross.obj` | `.obj` 尚无子命令（basic/etc motion 多相位 + add object effect 层结构） | 无需直译：PO 结构手工映射为 Area 编排（本档 §5 已给）；如后续 PO 类技能多，可加 `obj` 子命令输出相位序列 JSON |
| `passiveobject/.../gorecross1-4.ani` | `[SHADOW]`（值 0）不在翻译规则表 | 整节跳过无碍（PO 无影子），建议 README 未识别节清单补记 |
| 同上 gorecross1/4.ani | `[GRAPHIC EFFECT]` `LINEARDODGE`（线性减淡混合） | 暂跳过（README 已声明整节跳过）；若要还原图集辉光质感，需 AnimFrameData 加 blend 字段 + 视图 renderer.blendMode——**消费侧缺口，先记档** |
| `gorecross.ani`（角色） | `[SET FLAG]`（1/65534/2）、`[PLAY SOUND]` | 按既有约定跳过（触发帧 const 进技能类，bloodboom §4.7-7 同构）——非缺口 |
| PO 各 .ani 的 F0 空路径 `[IMAGE]`（路径为空串） | 现有规则可处理（path="" 空白帧） | 验证输出 json 的空 path 帧渲染为空白帧即可，无需改工具 |

结论：**.ani/.als 资源全部可被现有 ani 子命令翻译**（本技能无 .als）；实质缺口为 `.skl`/`.atk`/`.obj` 三类无子命令 + `[GRAPHIC EFFECT]` 消费缺口，计 4 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 挥砍弧光特效 slash1-4（引擎内置绘制，无 .als 边车、无脚本引用） | 延后（无声明式翻译源） | demo 先跳过（角色动画本身含挥刀动作）；还原时可手组装 overlay（releasewave 8 层手组装先例） |
| 血气旺盛（63）MP→HP 替代 + 出血率+40% | 延后（MP 消耗在延后清单；被动技能系统不存在；Buff 查询门面缺失——bloodboom §4.6 已记同类） | demo 固定 100% 出血（等于被动满档）；HP 消耗可选做成 `ConsumeCasterHp(常数)`（同 BloodBoomSkill 用法） |
| 追击输入窗口的精确边界（引擎内置，剑影版=召唤帧至动画末） | 已有（输入缓冲 PeekBufferedButton/ConsumeBuffer 落地） | 窗口 = F15 至 TotalTimeMs；追击动画独立表现不可得 → 追击区自带视觉（三联十字加大）替代 |
| 强制-十字斩（cancelgorecross）跨技能取消 | **缺失（新缺口，见 §8）** | 不实现；现有 TryCast 硬直门禁 + 输入缓冲近似"被打后立放" |
| flag 65534 取消/命中标记语义 | 未考证（引擎内置） | 忽略 |
| PO 多相位（basic→etc motion 序列，每相位独立 atk） | 框架无多阶段 Area（不算缺失，编排可表达） | 两个 Area 按帧序创建（§5）；若嫌 Area 数量多，可后续给 AreaDefinition 加 PhaseActions（不急） |
| LINEARDODGE 线性减淡混合 | 延后（帧混合模式无数据通道） | 直出原始帧（视觉略实，可接受） |
| 等级数值缩放（col0-11 十二列） | 延后（等级缩放在延后清单） | demo 固定值（§5 数值表） |
| 出血参数按 skl 列 5/6 缩放 | 列语义未考证 | 用现有 BleedBuff 预设（3s/15 每秒） |
| 音效（GORECROSS_ATK3 / R_GORECROSS_HIT / R_SQUARESWDC_HIT） | 延后（无音频系统） | 跳过（已考证记录在案） |

## 8. 存疑与缺口上报

**未考证项**
1. `.skl` 12 列 level property 逐列语义（引擎内置消费，无 nut 佐证；仅 col4=10000 出血几率有惯例旁证）——241 技能普遍性问题，建议主循环在总览里统一记档。
2. `[static data] 150 100 100`（dungeon）语义。
3. flag 65534（F14）语义（推断为取消窗口/命中标记）。
4. 引擎内置版追击段的专属动画（若存在）——剑影版有独立收势动画，狂战士版未考证；demo 以追击区视觉替代。
5. 挥砍弧光 slash1-4 与两刀/PO 相位的精确挂接关系（无引用者可查）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **技能取消体系**（强制-XX 被动 + 65534 标记 + 中断连携如 swordman_common 的流心钩子）：现有系统仅同技能 RestartCurrentSkill 连段取消，无"技能 A 施放中取消进技能 B"的能力。鬼剑士有全套 Cancel 被动（cancelgorecross.skl 等不在 241 清单内，但流心类连携都倚在 common nut 钩子上），若做连招手感会反复撞到。
2. **引擎内置绘制特效无声明式来源**：老技能（本技能首批实证）的武器弧光不走 .als、不走 nut，纯引擎硬编码——翻译管线够不着。建议：手组装 overlay 惯例化（命名约定 + 注册辅助函数），或在文档层统一记"特效缺源"。
3. **PO 多相位 .obj 结构**：basic→etc motion 序列 + 每相位 atk 是 DNF 被动对象通用形态（本技能首个完整样本），跨技能复用度高——建议 DnfConfigTranslation 立项 `obj` 子命令时直接按"相位序列"建模（motion/atk 配对数组），游戏侧可映射为 Area 编排或未来 PhaseActions。

**翻译工具缺口（并入主循环汇总）**：`.skl` 子命令、`.atk` 子命令、`.obj` 子命令、`[GRAPHIC EFFECT]` 消费侧通道（详见 §6，计 4 条）。
