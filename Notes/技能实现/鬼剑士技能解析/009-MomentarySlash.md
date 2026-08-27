# 拔刀斩（MomentarySlash）

> 技能ID 9 | 级别 A | 可实现性 🔶（主干直接可实现；蓄力输入与太刀抓取降级） | 分析日期 2026-08-22 | 批次 A10

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 拔刀斩 | `skill\Swordman\MomentarySlash.skl [name]` |
| 英文名 | MomentarySlash（取 skl 文件名；[name2]="拔刀" 是中文短名） | 同上 [name2] |
| 职业 | 剑魂（[skill fitness growtype]=1，L17；growtype 5 剑影亦可学至上限 50） | 同上 |
| 学习等级 | 35 | 同上 [required level] |
| 最高等级 | 70（growtype1/5 段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ←↓→ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 15000 ms（pvp 18000 / 起手 18000） | 同上 [dungeon][cool time] / [pvp] |
| MP | 110 → 924（Lv1→Lv70，dungeon） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无（消耗无色小晶体 3037×1） | 同上 [consume item] |
| 一句话效果 | 拔刀蓄势后向周围大范围快速斩出强力一击；太刀/光剑精通达标且持对应武器时追加拔刀追击 | 同上 [explain] |

**static data**：dungeon `500 300 225 150 75 0` / pvp `500 30 20 10 5 0`（6 值）。
**level info**：1 列——2628（Lv1）→ 28084（Lv70）（pvp 180→…）。
**level property 模板**：`拔刀斩武器物理攻击力 : <int>%%`，向量 `(-1, 0, 1.0)`（常量 stub；实际数值即 level info 唯一列）。

static data 语义**未考证**（施法侧引擎内置）。强旁证：**首值 500 与角色动画 F0 delay=500ms 精确吻合** → 推断 6 值 = 蓄力五段阈值 + 结束标记（500/300/225/150/75/0，pvp 等比缩至 30/20/10/5），即"按住蓄力、分段强化"系统的参数；`checkExecutableSkill` 进状态前压入的 6 个 0 与之一一对应（各段计数器清零）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
137: IRDSQRCharacter.pushState(0, "character/swordman/badao/badao.nut", "MomentarySlash", 23, 9);
```

- nut 目录名与技能名不一致（**badao** = 拔刀），一切以注册行为准（L2 语义：状态号 23、技能 ID 9）。
- **半引擎内置形态**（F3b 变体）：注册行存在、nut 存在（57 行）但**无 onSetState/onKeyFrameFlag/onProc/onEndCurrentAni 四大回调**——蓄力/出刀时序全在引擎；nut 只提供门禁与命中补丁两类钩子。
- `sqr\character\swordman\` 内引用 momentaryslash 的仅 badao 两文件 + `swordman_header.nut`（`CUSTOM_ANI_MOMENTARYSLASH <- 8`）。

### 2.2 主 nut 逐回调（badao.nut，57 行全读）

**checkExecutableSkill_MomentarySlash**（门禁）：
```
sq_IsUseSkill(9) → 压 6 个 0 进 IntVect（蓄力段计数器）→ flag var 清 0 → 进状态 23
```

**onAfterSetState_MomentarySlash**：
```
若武器子类型 == 2（太刀）：attackInfo 设 upForce 200（浮空加强）
isAtk var 清 0（已出刀标记）
```

**onBeforeAttack_MomentarySlash**（每次命中前）：
```
本技能命中硬直置 0（sq_SetCurrentAttackeHitStunTime(attackInfo, 0)）
若太刀 && 目标可抓取/可定身/非霸体/非装置：
    挂 appendage ap_momentaryslash → sq_HoldAndDelayDie(200,200)（命中即控住）
isAtk 置 1
```

**checkCommandEnable**：恒 true（任意状态可指令）。

引擎侧（未考证，由资源反推）：播 momentaryslash.ani（etc motion #8）→ F0 500ms 蓄势 → 出刀帧引擎施加角色攻击（momentaryslash.atk）+ 生成拔刀波 PO（momentaryslashwave.obj，见 §2.3）。

**蓄力重制版资源组**（现代版蓄力管线，引擎内置择用）：`momentaryslashre_ready.ani`（1 帧 100000ms 无限蓄势站桩）→ `momentaryslash_re.ani`（12 帧 855ms，**F3 flag 1 / F4 flag 2** 真触发标记）→ `momentaryslash_re_last.ani`（2 帧 1000ms 满蓄停顿 + charge1 特效 `[none effect add]` 边车）→ `momentaryslashre_attack.ani`（11 帧 555ms 出刀）。

### 2.3 被动对象与 appendage

**拔刀波 momentaryslashwave.obj**（`passiveobject\character\swordman\momentaryslashwave.obj`，"wave of momentary slash"）：

| 节 | 值 | 说明 |
|---|---|---|
| pass type / piercing | pass all / 1000 | 全穿透 |
| basic motion | `Animation/MomentarySlash/Start.ani` | 3 帧（10000/200/80ms），**F0 有攻击盒** |
| attack info | `AttackInfo/MomentarySlashWave.atk` | 下表 |
| int data | `50` | （生命周期参数，未考证） |
| destroy condition | 无（int data 驱动） | |

**Start.ani F0 攻击盒**（偏移+尺寸）：`-69 -69 -68 220 137 143` → 中心 x≈+0.4 单位、半尺寸 ≈(1.1, 0.69, 0.72) 单位；引 `Character/Swordman/Effect/MomentarySlashEx/New_BigWave.img`（**借用 Ex 系贴图**）+ LINEARDODGE。F0 的 10000ms 为悬停帧（命中窗口由引擎/int data 截断）。另有 `badao.act`（basic ani=startb.ani 视觉版，F0 创建视觉 PO 26184 于 x=75——**26184 在 passiveobject.lst 无命中**，注册处未考证）与 `start.ani.als`（叠 startb 层 z=-10）。

**MomentarySlashWave.atk**：

| 字段 | 值 | → 我们 HitReaction |
|---|---|---|
| absolute damage / damage bonus | 3000 / 300 | 固定伤害基底（demo 折算） |
| attack type | physic（weapon damage apply） | — |
| damage reaction / direction | **down** / hit horizon | 长硬直击倒近似 |
| push aside / lift up | 0 / 0 | 不击退不浮空 |

**角色侧 momentaryslash.atk**（.chr etc attack info，引擎施加）：physic/weapon damage、reaction **none**、push 200 / lift 200、hit horizon、knuck back 3/10。

**ap_momentaryslash.nut**（太刀命中 appendage，61 行全读）——抓取微控完整链：
```
onStart：前方加特效 character/swordman/animation/rorate.ani
proc：t≤550ms → 目标 z 轴加速抬升到 150（sq_GetAccel 450ms）
      zPos≥10 期间 → 按时间旋转目标（每 400ms 转 360°——旋转上抛）
      抬升结束且落回地面 → 强制 STATE_DOWN（250 力度砸地）+ 失效
onEnd：删特效/appendage
```
——即太刀拔刀斩命中 = "卷起→旋转→砸落"的投摔表现（Grab 缺口家族）。

**temporaryhitmomentaryslash.obj**：仅 [name] 一节的空壳 PO（无 motion/attack）——引擎内置专用占位。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\momentaryslash.ani`（.chr etc motion #8） | 12 | 1055 | F6=65534 | 无 | F0=500ms 蓄势（=static col0）；F1-3=25ms 三连快帧（出刀瞬间）；其余 30-150ms 收势。65534 语义未考证（064/022 同款，第三例） |
| momentaryslash.[pvp].ani | 12 | 1180 | 无 | 无 | pvp 变体（不用） |
| momentaryslash_re.ani（蓄力版） | 12 | 855 | F3=1、F4=2 | 无 | 引擎蓄力管线（现代版） |
| momentaryslashre_ready.ani | 1 | 100000 | 无 | 无 | 无限蓄势站桩（超长 DELAY 极端例） |
| momentaryslashre_attack.ani | 11 | 555 | 无 | 无 | 蓄力出刀 |
| momentaryslash_re_last.ani | 2 | 1000 | 无 | 无 | 满蓄停顿（.als 挂 charge1） |
| `passiveobject\...\momentaryslash\start.ani`（拔刀波） | 3 | 10280 | 无 | **F0**（盒见 §2.3） | F0=10000 悬停；New_BigWave.img |
| startb.ani（波视觉版） | 3 | 10200 | 无 | 无 | badao.act 基动画 |

`.als`：start.ani.als（[add] z=-10 叠 startb）、momentaryslash_re_last.ani.als（[none effect add] charge1）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MomentarySlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MomentarySlash.skl` | ✅ 实测 | 等级/CD/MP/static 6 值/1 列 level info |
| 注册行 | swordman_load_state.nut 行 137 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | badao.nut，状态 23，技能 9 |
| 主 nut | badao.nut | `…\pvf\sqr\character\swordman\badao\badao.nut` | ✅ 实测（57 行全读） | 门禁/太刀补丁；**无四大回调（半引擎内置）** |
| appendage | ap_momentaryslash.nut | `…\pvf\sqr\character\swordman\badao\ap_momentaryslash.nut` | ✅ 实测（61 行全读） | 太刀旋转砸落抓取 |
| .chr 条目 | etc motion #8 + etc attack info | `…\pvf\character\swordman\swordman.chr` 行 981 / 1302 | ✅ 实测 | Animation/MomentarySlash.ani；AttackInfo/MomentarySlash.atk |
| 角色 .ani | momentaryslash.ani（+re 系 4 个 + pvp 变体） | `…\pvf\character\swordman\animation\` | ✅ 实测 | 基础 12 帧 1055ms + 蓄力管线资源组 |
| 角色 .atk | momentaryslash.atk | `…\pvf\character\swordman\attackinfo\momentaryslash.atk` | ✅ 实测 | none 反应/push200/lift200 |
| PO 定义 | momentaryslashwave.obj / temporaryhitmomentaryslash.obj | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 拔刀波判定体 / 空壳占位 |
| PO .ani | start.ani / startb.ani（+badao.act） | `…\passiveobject\character\swordman\animation\momentaryslash\` | ✅ 实测 | 波判定+视觉（F0 盒） |
| PO .atk | momentaryslashwave.atk | `…\passiveobject\character\swordman\attackinfo\momentaryslashwave.atk` | ✅ 实测 | absolute 3000+300/down/push0/lift0 |
| 挥砍特效 | drawingsword_*.ani ×8 + charge1/2.ani + circle.ani | `…\pvf\character\swordman\effect\animation\momentaryslash\` | ✅ 实测 | 蓝/红/白/无 ×上/下 弧光层 + 蓄力特效 |
| 装备层 | momentaryslash 系 ×54（coat 层抽样） | `…\pvf\equipment\character\swordman\avatar\coat\*\` | ✅ 实测（存在性） | 换装图层（demo 不需要） |
| 关联强化 | MomentarySlashEx（CUSTOM_ANI_MOMENTARYSLASHEX=98 / atk / momentaryslashexwave.obj / momentaryslashex.act） | 各目录 | ✅ 存在性实测 | E 类批次另行分析 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画（%04d 单图集） | 必需（共享） | ✅ 已在库 |
| `Character/Swordman/Effect/MomentarySlashEx/New_BigWave.img` | sprite_character_swordman_effect_momentaryslashex.NPK | 拔刀波视觉（start.ani） | **必需**（波视觉主表现） | ❌ |
| `Character/Swordman/Effect/MomentarySlash/circle.img` | sprite_character_swordman_effect_momentaryslash.NPK | 圆环特效（circle.ani） | 可选 | ❌ |
| `…/Effect/MomentarySlash/drawingsword_{none,blue,red,white}_{ldodge_under,upper}.img` ×8 | 同上 | 挥砍弧光（按武器/精通选色） | 可选（demo 选 1 色） | ❌ |
| `…/Effect/MomentarySlashEx/ChargeDodge1.img`、`ChargeDodge2.img` | sprite_character_swordman_effect_momentaryslashex.NPK | 蓄力光效（charge1/2.ani） | 可选（蓄力简化后不用） | ❌ |
| rorate.ani 引用 img | 未提取（ap_momentaryslash 特效；太刀线，demo 不做） | — | 不需要 | — |

缺失 img：必需 1 张、可选 2 类（circle + 弧光任选 1~2 张）。**必需级仅 1 张**（New_BigWave）——本技能资源极轻。

## 5. 实现方案草案

**降级决策**：蓄力（按住输入五段强化）与太刀抓取两大机制缺失 → demo 做"固定 500ms 蓄势 + 出刀大范围斩击"基础版（classic 形态）。

### 内容件清单

1. **`DotNet~/Skills/MomentarySlashSkill.cs : SkillLogic`**（BloodBoomSkill 范式：帧号 const + SubState 一次性守卫）
   - `CooldownMs = 15000`；`TotalTimeMs = 1055`（momentaryslash.ani 原时长直用）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanMomentarySlash)` + `ctx.ClearHitTargets()`。
   - `OnUpdate`：
     - t≥575（F4 出刀瞬间：500+25×3+50）且 SubState==0：`ctx.SetAttackHitbox(offset (0.5,0,0.3), half (1.1,0.7,0.7))`（Start.ani F0 盒折算）+ `ctx.CreateAreaInFront(AreaIds.MomentarySlashWave, (FP)5/10)`（波视觉区，无动作纯视觉可选）；SubState=1。
     - t≥725（出刀窗口 150ms 后）且 SubState==1：`ctx.DisableAttackHitbox()`；SubState=2。
   - `OnEnd`：`ctx.DisableAttackHitbox()` + `ctx.PlayDefaultAnim()`。
   - `HitReaction { Damage = 120, HitstunMs = 800, KnockbackX = 0, LaunchY = 0 }`（momentaryslashwave.atk：absolute 3000+300/down/push0/lift0 → demo 折算 + down 用长硬直近似；角色 atk 的 push200/lift200 属引擎侧武器判定，二者取一，见 §7）。
   - `HitActions = { MeleeHit }`。
2. **`DotNet~/Areas/MomentarySlashWaveArea.cs : AreaDefinition`**（可选纯视觉区）
   - `TotalTimeMs = 280`（start.ani 有效尾帧 200+80）、无 Actions、`ViewAnimId = AnimId.MomentarySlashWave`（New_BigWave 大范围斩光）。
   - 若不做此区，可把 New_BigWave json 挂为施法 ani 的手组装 overlay（releasewave 先例）——二选一。
3. **无新增 Buff/Action**（太刀抓取不做）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 23 + momentaryslash.ani | `MomentarySlashSkill` + `AnimId.SwordmanMomentarySlash` |
| F0 500ms 蓄势（static col0） | 动画原帧直用（json 不改——500ms 本来就是帧时长） |
| 蓄力五段（static 300/225/150/75） | 缺按住输入 → 砍掉（固定基础威力） |
| 拔刀波 PO（Start.ani F0 盒） | `SetAttackHitbox`（固定盒路径）+ 视觉区/overlay |
| momentaryslash.atk vs wave.atk 双来源 | 取 wave.atk（判定体权威），角色侧武器判定不做 |
| 太刀 ap_momentaryslash 旋转砸落 | 缺抓取/旋转微控 → 用 `HitReaction.LaunchY` 抬升近似（若演示需要）或直接跳过 |
| 太刀/光剑精通追击 | 缺精通系统 + Buff 查询门面 → 跳过 |
| 蓄力版 re 系动画组 | 一并跳过（随按住输入缺口） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.MomentarySlash = 16` + `ButtonToSkill` case 8（新键，如 B） |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.MomentarySlashWave = 6`（022 已占 4/5；若做视觉区） |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanMomentarySlash = 64`、`MomentarySlashWave = 65`（可选：charge1=66） |
| json 注册 | `LSAnimClipRegistrar.cs` | `RegisterOne` ×1~2（momentaryslash.json / momentaryslash_wave.json） |
| 图集 | `LSAnimResComponentSystem.cs` | `New_BigWave.img.bytes`（必需）+ 可选 circle/drawingsword |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 15000 ms | 15000（直用） |
| 总时长 | 1055 ms（12 帧） | 1055 |
| 蓄势 | F0 500ms | 500（原帧直用） |
| 出刀判定 | Start.ani F0 盒：中心 +0.4 单位、半尺寸 (1.1,0.69,0.72) | SetAttackHitbox offset (0.5,0,0.3) half (1.1,0.7,0.7) |
| 伤害 | wave.atk absolute 3000+bonus300；level col 2628→28084 | 120（固定） |
| 反应 | wave: down/push0/lift0；角色: none/push200/lift200 | Hitstun 800 / Kb 0 / Ly 0 |
| MP | 110→924 | 无 MP 系统，跳过 |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `momentaryslash.ani` / `start.ani` / `startb.ani` | `[SET FLAG] 65534`（按约定跳过）、F0/悬停超长 DELAY（start.ani 10000、re_ready 100000——**本技能不翻译 re 系**则只剩 start.ani F0） | start.ani 若做视觉区需手改 F0=280 总长内合理值；触发帧 const 进技能类（bloodboom 惯例） |
| `start.ani.als` / `momentaryslash_re_last.ani.als` | `[add]`/`[none effect add]` 已支持 | 无缺口 |
| `badao.act` | `.act` 无子命令 | 不依赖（行为手抄进技能类） |
| `momentaryslash.obj` 系（momentaryslashwave/temporaryhit） | `.obj` 无子命令 + `[int data]` 节 | 手工映射（§5 已给）；obj 子命令需覆盖 int data |
| `momentaryslash.atk` / `momentaryslashwave.atk` | `.atk` 无子命令 | 手抄（各 ~10 值） |
| `MomentarySlash.skl` | `.skl` 无子命令（static 6 值 + 1 列 level info） | 手抄 |

结论：常规 .ani/.als 可译；实质缺口 = 超长 DELAY（start.ani）+ `.act`/`.obj`/`.atk`/`.skl` 四类无子命令，计 5 条（与 022 同族）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 按住蓄力五段强化（static 500/300/225/150/75） | **按住输入缺失**（缺口累计已录"超长 DELAY/蓄力"） | 固定 500ms 蓄势瞬发；蓄力版待输入缓冲扩展（按住=持续缓冲可近似：后续可实验"按住期间不 ConsumeBuffer"） |
| 太刀命中旋转砸落（ap_momentaryslash） | 抓取/旋转微控缺失（Grab 家族新形态：非抓到手上，是命中后卷起旋转砸地） | 若要表现：`HitReaction.LaunchY=300` + 长硬直（空中落地即趴，LSFlight 已有"落地动量清零趴住"）；旋转不做 |
| 太刀/光剑精通追加拔刀追击 | 精通/被动技能系统 + Buff 查询门面缺失 | 跳过（explain 明示为条件分支） |
| 角色武器判定 + 波 PO 判定双伤害来源 | 单一 HitReaction 表达不了双来源 | 取波 PO（判定体权威），数值上调 20% 近似总量 |
| flag 65534（F6） | 未考证（第三例） | 忽略（出刀窗口自行按 F4-F7 推算） |
| PO 26184 视觉子对象 | passiveobject.lst 无注册（未考证） | 用 Area 视觉/overlay 替代 |
| 等级/武器精通数值缩放 | 等级缩放延后 | 固定值 |
| 音效（R_DRAWING_SWORD_HIT 等） | 延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①static data 6 值精确语义（"五段蓄力阈值"为推断，仅 col0=500 有帧时长旁证）；②flag 65534；③PO 26184 注册处；④`[int data] 50` 的生命周期语义；⑤引擎如何择用基础版 momentaryslash.ani 与 re 系蓄力管线（推测与版本/精通状态相关）。
- **新缺口上报**：①**按住型输入**（蓄力）——缺口累计已有，本技能是最典型例（F0 蓄势帧 + static 分段 + re 系无限站桩帧三重证据）；②**命中后旋转/砸落微控**（ap_momentaryslash）——比"抓取到手上"轻，可先用 LaunchY+落地趴近似，正式做法需要"受击者姿态程序控制"能力；③`.obj [int data]` 节纳入 obj 子命令设计。
- **给下轮的经验**：badao（拔刀）目录名 ≠ 技能名——**nut 目录与技能名不一致时以注册行为准**再查资源；半引擎内置技能的"蓄力"参数先看 **static data 首值是否=动画 F0 delay**（本例 500=500 强印证）。
