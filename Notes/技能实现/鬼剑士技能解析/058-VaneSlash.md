# 裂波斩（VaneSlash）

> 技能ID 58 | 级别 A | 可实现性 🔶（抓取判定简化） | 分析日期 2026-08-22 | 批次 A2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 裂波斩 | `skill\Swordman\VaneSlash.skl [name]` |
| 英文名 | VaneSlash（取 skl 文件名；[name2]="Wave breaks" 亦为英文，属例外） | 同上 [name2] 实测 |
| 职业 | 阿修罗系（wave 系；explain 提及波动刻印联动） | 同上 [explain] + 系常识 |
| 学习等级 | 10 | 同上 [required level] |
| 最高等级 | 70（各觉醒段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 0） | 同上 [type] / [skill class] |
| 指令 | →↑ + Z（指令施放 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 8000 ms（PvP 6000 ms） | 同上 [dungeon][cool time] / [pvp][cool time] |
| 施放时间 | 400 ms（casting time，蓄条——我方无此机制，见 §7） | 同上 [casting time] |
| MP | 20 → 310 | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 一句话效果 | 强力上斩抓取前方敌人并施放裂波波轮，裂波对被抓取及周围敌人多段攻击并击飞；上斩/下斩只伤被抓取者，裂波伤全部 | 同上 [explain] |

**level property（4 列，Lv1 → Lv70）**：`95→898`、`143→1346`、`114→1077`、`36→337`。
模板行（实测）：

```
上斩魔法攻击力 : <int>%%
下斩魔法攻击力 : <int>%%
裂波魔法攻击力 : <int>%% + <int>
裂波多段攻击次数 : <int>次
```

向量行 `(-1,0) (-1,1) (-1,2) (-2,3) (1,1)` 与占位符顺序对照 → **col0=上斩%、col1=下斩%、col2=裂波%、
col3=裂波固伤加值**（模板行推断，高置信）；**多段次数 = static data 第 2 值 `3`**（向量 (1,1) 指向
static 槽——推断，同型证据见 065-HopSmash 的 static[8]/[9] 恰等于模板"2~3 次"）。
**static data = `500 3`**：500 语义未考证（推断=波轮维持时长基准 ms），3=裂波多段次数。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无注册行**（grep `vaneslash` / `, 58` 均无命中；72 条 pushState 逐一核对）。
裂波斩属最老一代技能：**角色侧状态逻辑在客户端引擎内**，pvf 只提供数据文件；裂波波轮是数据驱动的被动对象（.obj）。

```
// sqr/character/swordman/swordman_header.nut
CUSTOM_ANI_VANESLASHTRY <- 14      // = swordman.chr [etc motion] 第 15 项（987 行，实测）
CUSTOM_ANI_VANESLASH    <- 15      // = 同上第 16 项（988 行）
```

```
// character/swordman/swordman.chr [etc attack info]（0 计数自 HardAttack.atk）
#16 `AttackInfo/VaneSlashTry.atk`     （1310 行）
#17 `AttackInfo/VaneSlashUpper.atk`   （1311 行）
#18 `AttackInfo/VaneSlashSmash.atk`   （1312 行）
```

兄弟职业脚本无同型实现（atswordman/demonicswordman/common load_state grep 均无命中）。

### 2.2 引擎内置行为重建（.ani 标记 + .obj/.atk 数据 + explain 三方印证）

**四段时序（推断，语义由 .atk 反应参数锚定）**：

| 段 | 触发 | 动画 | 命中参数 | 语义 |
|---|---|---|---|---|
| ① 上斩起手 | 施放（0.4s 蓄条后） | `vaneslashtry.ani` 5 帧 350ms，**F2/F3 攻击盒** `0,-25,31~117,50,126` / `-24,-25,51~101,50,125`（min/max → 前方 ~1.2 单位） | VaneSlashTry.atk：magic / **none 反应** / push 0 / lift 0 / **hit horizon** | 抓取判定：命中即把敌人卷入（无击退无浮空——为抓取让位） |
| ② 上斩击飞 | 抓取命中后 | 同上动画尾段（推断） | VaneSlashUpper.atk：magic / none / 0 / 0 / **hit lift up** + weapon damage apply | 把被抓敌人向上挑起（浮空由引擎抓取系统驱动，lift 值 0 佐证） |
| ③ 裂波波轮 | 挑起瞬间创建 PO | 角色 `vaneslash.ani` 7 帧 1490ms（F2=140ms，F5 flag 65534，**F6 delay 1000ms 长停帧**=维持波轮）；PO `VaneSlash.ani`（见 2.3） | VaneSlash.atk（PO）：magic / damage 反应 / push 100 / lift 50 / blow / no blood 50 | 波轮贴着被抓敌人旋转多段打击（3 段，static[1]） |
| ④ 下斩收尾 | 波轮结束 | PO `VaneSlashNormal.ani` + 角色下斩 | VaneSlashSmash.atk（角色，magic/none/0/0/hit down）+ VaneSlashFinal.atk（PO）：magic / **down 击倒** / push 500 / lift 200 / blow | 把敌人砸飞出去（击倒+大击退） |

**角色无敌帧推断**：`vaneslash.ani` 全 7 帧 **0 个受击盒**（实测）——旋转期间角色不可被命中
（DNF 裂波斩确有无敌窗，但"全程无敌"存疑，标推断）。
F5 flag 65534 同 GoreCross F14（取消窗口/命中标记，语义未考证）。

**联动**：explain"若已学[波动刻印]则产生 1 个刻印"——需 Buff 查询门面（缺失），跳过（§7）。

### 2.3 被动对象：裂波波轮（vaneslash.obj，完整实测）

`passiveobject/character/swordman/vaneslash.obj`（`vaneslash_ds.obj` 为剑影专用变体，数据同构）：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [floating height] | 1 | 悬浮高度 |
| [pass type] / [piercing power] | pass all / 1000 | 全穿透 |
| [basic motion] | `Animation/VaneSlash.ani` | 相位 1：波轮旋转（20 帧，名义 11330ms 含 F13 的 10000ms 悬停帧；F5-F14 每帧 **2 个攻击盒**，F15-F19 无盒淡出） |
| [attack info] | `AttackInfo/VaneSlash.atk` | 相位 1 命中：damage/push100/lift50/blow |
| [etc motion] | `Animation/VaneSlashNormal.ani` | 相位 2：终结爆发（4 帧 280ms，IMAGE RATE 缩放） |
| [etc attack info] | `AttackInfo/VaneSlashFinal.atk` | 相位 2 命中：down/push500/lift200/blow |
| [object destroy condition] | **无此节**（与 064 gorecross.obj 不同） | 波轮寿命由引擎状态驱动销毁（推断：跟随角色施放状态结束） |

**波轮攻击盒实测**（F5-F14 恒定 2 盒，min/max 口径）：`-76,-35,-23~212,70,66` 与 `-128,-35,-68~179,70,58`
→ 主盒约 x∈[-76,212]（2.88 单位宽）× z∈[-23,66]，副盒偏后 x∈[-128,179]——覆盖角色前后的大范围旋轮。
动画用 `Character/Swordman/Effect/VaneSlash/lighting.img`，全帧 LINEARDODGE。

**多段实现（推断）**：DNF 引擎对 PO 定时重置命中表实现多段；段数 3（static[1]），与 10 个带盒帧的
关系未考证（可能每帧一跳共 ~10 跳，static 3 为对单目标段数——**未考证**，demo 建议 3 段）。

特效层（引擎绘制，`effect/animation/vaneslash/`，无引用者）：
`dust.ani`（6 帧 420ms，up-dust.img，起手尘土）、`upper.ani`（5 帧 350ms，up.img，上斩光）、
`smash.ani`（8 帧 630ms，up.img + down.img，下斩光）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/vaneslashtry.ani` | 5 | 350ms | 无 | **F2/F3** | 受击盒 1/帧 |
| `character/swordman/animation/vaneslashtry.[pvp].ani` | — | — | — | — | PvP 变体（存在，未读） |
| `character/swordman/animation/vaneslash.ani` | 7 | 1490ms（F6 停 1000ms） | F5=65534 | 无（引擎施加） | **全程 0 受击盒**（推断无敌帧） |
| `passiveobject/.../animation/VaneSlash.ani` | 20 | 名义 11330ms（F13 停 10000ms） | 无 | **F5-F14 每帧 2 盒** | 波轮主相位；LINEARDODGE |
| `passiveobject/.../animation/VaneSlashNormal.ani` | 4 | 280ms | 无 | 无（引擎按 etc attack info 施加） | 终结爆发；IMAGE RATE |
| `effect/animation/vaneslash/`（dust/upper/smash） | 5-8 | 350-630ms | 无 | 无 | 引擎绘制特效 |

`.als` 边车：**本技能全部文件均无**（两侧 animation 目录 ls 实证）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | VaneSlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\VaneSlash.skl` | ✅ 实测 | 技能数据 |
| lst 条目 | swordmanskill.lst 67-68 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 58 → 本 skl |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | — |
| 常量表 | swordman_header.nut 184-185 行 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_VANESLASHTRY/VANESLASH = 14/15 |
| .chr 条目 | etc motion #14/#15（987/988 行）+ etc attack info #16/#17/#18（1310-1312 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 动画 + 三段命中参数 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（grep vaneslash 仅中 header 常量） | ⛔ 缺失 | 角色逻辑在引擎 |
| ap nut | —（不存在） | `…\pvf\passiveobject\character\swordman\` | ⛔ 缺失 | PO 行为引擎内置，数据在 .obj |
| 角色 .ani | vaneslashtry.ani / vaneslash.ani（+ [pvp] 变体） | `…\pvf\character\swordman\animation\` | ✅ 实测 | 见 §2.4 |
| 角色 .atk | vaneslashtry.atk / vaneslashupper.atk / vaneslashsmash.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 上斩抓取/挑起/下斩 |
| PO 定义 | vaneslash.obj（+vaneslash_ds.obj 剑影变体） | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 波轮结构 |
| PO .ani | VaneSlash.ani / VaneSlashNormal.ani（+_ds ×2；vaneslash_light\vaneslash.ani 光属性变体，引用 `EFFECT/VANESLASH/LIGHT/LIGHTING.IMG`，无引用者） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 波轮两相位 |
| PO .atk | vaneslash.atk / vaneslashfinal.atk（+_ds ×2） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | 多段/终结命中 |
| 特效 .ani | dust.ani / upper.ani / smash.ani（+vaneslash_ds\） | `…\pvf\character\swordman\effect\animation\vaneslash\` | ✅ 实测 | 引擎绘制特效 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失 | — |
| 装备层 | vaneslash.ani ×76 / vaneslashtry.ani ×76 | `…\pvf\equipment\character\swordman\avatar\` | ✅ 实测（find 计数） | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画帧 | 必需（共享） | ✅ 已在库 |
| lighting.img | sprite_character_swordman_effect_vaneslash.NPK | 波轮主相位（PO 20 帧全用它） | **必需** | ❌ |
| lighting-normal.img | 同上 | 波轮终结爆发 | **必需** | ❌ |
| up.img | 同上 | 上斩/下斩光（upper.ani + smash.ani） | 可选 | ❌ |
| down.img | 同上 | 下斩光（smash.ani） | 可选 | ❌ |
| up-dust.img | 同上 | 起手尘土（dust.ani） | 可选 | ❌ |
| LIGHT/LIGHTING.IMG（光属性变体） | sprite_character_swordman_effect_vaneslash_light.NPK（推断） | 光属性版波轮（无引用者） | 可选（不做） | ❌ |

缺失 img：必需 2、可选 3（+1 光属性变体不做），同属主 NPK 一次提取全覆盖。

## 5. 实现方案草案

### 内容件清单（继承真实基类；数值 DNF 原值 + demo 建议值并列）

1. **`DotNet~/Skills/VaneSlashSkill.cs : SkillLogic`**（同 BloodBoomSkill 范式：帧号/时间 const + SubState 一次性守卫）
   - `CooldownMs = 8000`（DNF 原值直用）；`TotalTimeMs = 2000`（try 350 + spin 1490 ≈ 1840，留余量；DNF 蓄条 400ms 砍掉=瞬发）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanVaneSlashTry)` + `ctx.ClearHitTargets()`。
   - `OnUpdate` 编排（SubState 0→1→2）：
     - 0→1（帧 2-3，帧驱动攻击盒由 vaneslashtry json 自带）：上斩命中 → `HitReaction A`（demo：Damage 70/HitstunMs 600/Kb 0/**Ly 180**——DNF lift 值为 0 是因为抓取系统代劳浮空，我方用 LaunchY 替代"挑起"）。
     - t=350ms → SubState=1：`ctx.PlayAnim(AnimId.SwordmanVaneSlash)`（旋转，引擎版无敌帧不还原）+ `ctx.CreateAreaInFront(AreaIds.VaneSlash, 0.8)`（波轮创建在身前——DNF 波轮以被抓敌人为中心，我方简化为前方固定点）。
     - t≈1600ms → SubState=2：`ctx.CreateAreaInFront(AreaIds.VaneSlashFinal, 0.8)`（终结爆发）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/VaneSlashArea.cs : AreaDefinition`**（波轮多段——同 FireCircleArea 范式：TickActions 循环结算）
   - `TotalTimeMs = 1250`、`TickTimeMs = 350`（Tick 于 350/700/1050ms = **3 次** ≈ static[1]=3 段；
     350ms 起跳与 PO VaneSlash.ani 的 F0-F4 无盒暖机段 5×70ms=350ms 精确对齐）；`EnterActions = null`；
   - `TickActions = { MeleeHit }`；`HalfExtents = (1.4, 0.5, 0.45)`（PO 主盒 x∈[-76,212] 折算半宽≈1.44）；
   - `HitReaction { Damage = 45, HitstunMs = 500, KnockbackX = 100, LaunchY = 50 }`（vaneslash.atk 原值 push100/lift50）；
   - `ViewAnimId = AnimId.VaneSlashWheel`（VaneSlash.ani json；F13 悬停帧 10000ms 翻译时改小，否则视图播不完）。
3. **`DotNet~/Areas/VaneSlashFinalArea.cs : AreaDefinition`**（终结爆发——同 BloodBoomArea 单次结算）
   - `TotalTimeMs = 280`、`TickTimeMs = 0`、`EnterActions = { MeleeHit }`、`HalfExtents = (1.2, 0.5, 0.5)`；
   - `HitReaction { Damage = 120, HitstunMs = 800, KnockbackX = 500, LaunchY = 200 }`（vaneslashfinal.atk 原值：down/push500/lift200）；
   - `ViewAnimId = AnimId.VaneSlashNormal`（VaneSlashNormal.ani json）。
4. 无需新 Action / Buff（MeleeHit 现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 58 + try/spin 两动画 | `VaneSlashSkill` + 两个 AnimId（PlayAnim 时序切换） |
| 抓取判定（try atk hit horizon / upper atk hit lift up） | 简化为普通命中 + `HitReaction.LaunchY`（无抓取系统，见 §7） |
| 波轮 PO（.obj 多相位） | 两个 `AreaDefinition` 顺序创建（064 GoreCross 同构定案） |
| PO 多段（引擎命中表重置） | Area `TickActions`（FireCircle 先例） |
| 终结击倒 | `VaneSlashFinalArea.HitReaction`（KnockbackX/LaunchY + 长硬直） |
| 蓄条 400ms | 砍掉（瞬发） |
| 波动刻印联动 | 跳过（Buff 查询门面缺失） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.VaneSlash = 12`（顺延；与并行批次冲突时以实现时统一分配为准）+ `ButtonToSkill` case 8（新键） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanVaneSlashTry=54`、`SwordmanVaneSlash=55`、`VaneSlashWheel=56`、`VaneSlashNormal=57`（接 48 段后顺延，含 064 提案的 49-53，冲突时统一调） |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×4 |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | `lighting.img.bytes`、`lighting-normal.img.bytes`（必需两张） |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 8 |
| 翻译 | DnfConfigTranslation ani 子命令 | 角色 2 + PO 2 json |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 8000ms | 8000（直用） |
| 总时长 | 蓄 400 + try 350 + spin 1490 ≈ 2240ms | 2000（瞬发） |
| 上斩 | try.atk：none/0/0/horizon；倍率 col0 95% | 伤害 70/硬直 600/Ly 180（挑起替代抓取） |
| 波轮每段 | PO atk：damage/push100/lift50；倍率 col2 114% | 伤害 45/硬直 500/Kb 100/Ly 50 ×3 Tick |
| 终结 | final.atk：down/push500/lift200 | 伤害 120/硬直 800/Kb 500/Ly 200 |
| 多段次数 | static[1]=3（推断） | TickTimeMs 350 ×3（350ms 暖机对齐 PO 无盒帧） |
| 波轮范围 | PO 盒 x∈[-76,212] | HalfExtents (1.4, 0.5, 0.45) + 前偏 0.8 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `VaneSlash.skl` | `.skl` 尚无子命令（4 列 level info + static） | 手抄 4 组数值可行；随批量化加 `skl` 子命令（同 064 记档） |
| 5 个 `.atk`（角色 3 + PO 2） | `.atk` 尚无子命令；`[knuck back] -1`、`[no blood]`、`[hit wav]` 字段无对应概念 | 手抄每文件 ~8 值；knuck back/no blood 呈现层字段暂跳过 |
| `vaneslash.obj` | `.obj` 尚无子命令 | 手工映射为 Area 编排（本文 §5 已给）；随 PO 类技能批量化提级 `obj` 子命令（064 同议） |
| PO `VaneSlash.ani` / 特效 ani | `[SHADOW]`（跳过无碍）、`[GRAPHIC EFFECT]` `LINEARDODGE` | SHADOW 补记 README 未识别清单；LINEARDODGE 为消费侧缺口（同 064 记档：需帧 blend 字段） |
| PO `VaneSlashNormal.ani` | `[IMAGE RATE]` 缩放 | 整节跳过（延后档）；终结爆发直出原始帧 |
| PO `VaneSlash.ani` F13 `[DELAY] 10000` 悬停帧 | 翻译会直译 10000ms | **翻译后手改 json**（改 ~500ms）或工具加"超长 delay 钳制"选项；Area 视图依赖动画播完 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 抓取判定（可抓霸体/防御敌人，卷入波轮；对不可抓敌人上斩/下斩无伤害） | **缺失：抓取/投掷 Grab 系**（§6.3 已列，本技能实证） | 上斩=普通命中+LaunchY 挑起；波轮对全体敌人结算（DNF 本就如此）；对霸体敌人差异不还原（无霸体系统） |
| 波轮以被抓敌人为中心旋转 | 无目标绑定原语（Area 固定点） | 固定在施法者前方 0.8 单位（视觉差异小） |
| 旋转期间角色无敌（0 受击盒，推断） | 无无敌帧机制（也未列档） | 不还原（受击如常）；如需，可临时清 CurrentHurtBoxes（新门面，记档） |
| 蓄条 400ms + 施放中不可移动 | 无 cast 条/移动锁（延后） | 瞬发 + TotalTimeMs 门禁 |
| 多段 3 次（引擎命中表重置） | 多段命中在延后档 | Area TickActions 表达（已有，不算缺口） |
| 波动刻印 +1 联动 | Buff 查询门面（缺失） | 跳过 |
| LINEARDODGE / IMAGE RATE | 延后档 | 直出原始帧 |
| 音效（YEOLPA_WIND_HIT / R_SLESSSWDA_HIT） | 延后档（无音频） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. col3（36→337）= 裂波固伤加值的读法（模板行推断；另一候选=多段次数，与 static[1]=3 冲突，取前者）。
2. static data `500` 语义（推断=波轮维持时长基准）。
3. 波轮实际段数（static 3 vs 10 个带盒帧；引擎命中表重置周���无数据）。
4. 上斩(②)与起手(①)在 try 动画内的分界（无脚本，两 atk 无帧号锚点；推断 ①=F2-F3、②=引擎在抓取命中瞬间施加）。
5. 角色旋转全程 0 受击盒是否等于无敌帧（推断）。
6. `vaneslash_light\vaneslash.ani`（光属性变体，引用 `LIGHT/LIGHTING.IMG`）的引用者未考证（推断=元素属性切换系统的产物，与 releasewave §5.5"雷神之息"同类）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **抓取系统的实证补充**：老技能（本技能）的抓取判定零脚本零数据——pvf 侧只有 atk 的 `hit horizon`/`hit lift up` 方向标记，判定与卷入逻辑全在引擎。若后续要还原"上斩抓取"手感，需自建：命中→目标锁定→跟随位移→释放，工程量不小；建议维持 §6.3 缺失档结论，各抓取技统一走"LaunchY 挑起"简化。
2. **超长 DELAY 悬停帧的翻译处理**：PO/角色 ani 中 delay=10000 的帧（本技能波轮 F13、jump.ani F7/F14）直译后动画永不播完——工具侧建议加钳制或文档约定手改（与 017 §8 同源，一并汇总）。

**翻译工具缺口**：`.skl` 子命令、`.atk` 子命令、`.obj` 子命令、`[GRAPHIC EFFECT]` 消费通道、超长 DELAY 处理（计 5 条；.ani 主体全部可译）。
