# 银光落刃（AshenFork）

> 技能ID 16 | 级别 A | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 银光落刃 | `AshenFork.skl [name]` |
| 英文名 | AshenFork（skl 文件名；[name2]=`Ashen Fork`） | 同上 [name2] |
| 职业 | 鬼剑士共通（growtype 0-5；巨剑精通多段限剑魂系） | 同上 |
| 学习等级 | 5 | 同上 [required level] |
| 最高等级 | 70 | 同上 [maximum level]（level info 实际 70 档） |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | (跳跃状态下) Z | 同上 [command] / [command key explain] |
| CD | 4000 ms（pvp 5000） | 同上 [dungeon][cool time] |
| MP | 10 → 120（Lv1 → Lv70） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 武器效果类型 | physical | 同上 |
| static data | `50 10 50 100`（pvp `50 10 50 75`；语义未考证——**推断为跳跃高度阈值/伤害分档**：跳得越高伤害越强、过阈值出冲击波） | 同上 [static data] |
| 预加载资源 | `[skill preloading image] JumpAttackHold.img`（下落蓄能光效贴图，官方预加载声明） | 同上 |
| 一句话效果 | 跳跃中向下方敌人强力刺击（越高越强）；达到一定高度施放出现冲击波；剑魂巨剑精通 Lv3+ 可多段攻击 | 同上 [explain] |

**level property（3 列，Lv1 → Lv70，模板三占位齐一一对应）**：col0 `185→1752`、col1 `185→1752`、col2 `100→219`。
- col0 = 物理攻击力%（模板 `<int>%%`）——刺击本体倍率；
- col1 = **冲击波物理攻击力**（模板第二个 `<int>`，固定值型）；
- col2 = 冲击波大小比率%（模板 `<int>%%`）——col1/col2 与 ashenforksub PO 的存在互证，置信较高（无 nut 参照，标推断）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**`swordman_load_state.nut` 中无注册行**；白名单内无 ashenfork 相关 nut（`sqr\character\swordman\jump\swordman_jump.nut` grep ashen/fork/jumpattack **零命中**——跳跃状态机的跳转也引擎内置）。本技能与连突刺同为**无 nut 参照**的引擎内置技（F3 特征），行为重建靠 .chr/.ani/.atk/.obj 数据。

相邻注册：`swordman_jump.nut`（状态 6，SwordmanJump）管理跳跃/下落——银光落刃即从该状态按 Z 触发（推断），.chr `[jumpattack motion]` 提供下落刺击动作。

### 2.2 引擎内置状态行为重建（.chr/.ani/.atk/.obj 数据印证）

**施法瞬间（推断）**
- 跳跃状态中按 Z → 扣 MP、起 CD，角色进入下落刺击：播 `JumpAttack.ani`（.chr [jumpattack motion]，与普通跳跃攻击共用动作），叠放下落光效 `jumpattackhold.ani`（skl 预加载声明的贴图）；
- 设攻击信息 `AshenFork.atk`（etc atk 槽 10）——注意与普通跳攻 `JumpAttack.atk`（[jumpattack info]，damage bonus 10）**同动作不同 atk**：银光版无 bonus、击倒反应相同。

**下落刺击判定（jumpattack.ani 实测）**

| 帧号 | 累计时间 | 攻击盒（min/max 口径） | 语义 |
|---|---|---|---|
| F2 | 150ms | `13 -13 -11 88 26 167` | **窄高刺击盒**：x∈[13,88]（前伸 0.88 单位）、z∈[-11,167]（**自高空贯到地面**）——下落刺击判定形 |

（引擎在下落期间循环/停驻该判定帧直到落地——推断；.ani 本体仅 6 帧 300ms。）

**落地冲击波（ashenforksub.obj 完整实测）——本技能核心判定体**：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [name] | `银光落刃的飞溅` | 官方命名 |
| [layer] / [floating height] | **bottom** / 1 | 贴地层 |
| [pass type] / [piercing power] | pass all / 1000 | 全穿透（横扫全场敌人） |
| [basic motion] | `Animation/AshenForkSub.ani` | 6 帧 330ms；**攻击盒 F0-F3**（偏移+尺寸口径，对称环状）：F0/F1 ±94×33 / F2 ±110×37 / F3 ±110×40（x 半幅 0.94→1.10 单位渐扩）——判定窗 0-220ms |
| [etc motion] | AshenForkSubDust.ani / AshenForkSubFlash.ani | 尘土（11 帧 537ms）+ 白闪（2 帧 200ms）视觉层 |
| [attack info] | `AttackInfo/AshenForkSub.atk` | 物理/**down 击倒**/**lift 300**/no blood/hit wav R_SHOCKWAVE_HIT |

冲击波以落点为中心、贴地对称扩散（环盒渐扩），击倒浮空（lift 300 + down）。伤害 = skl col1（冲击波攻击力），大小 = col2 比例（推断：缩放 PO 盒与图像——对象整体缩放延后档）。

**高度机制（推断）**：跳得越高 col0 倍率越高（static data 分档）；未达高度只有刺击、达到高度才出冲击波（explain 明示）。

**巨剑精通多段（剑魂条件分支，explain 明示）**：`jumpattackmultislash1/2.ani`（etc 槽 23/24）+ 特效目录 `jumpattackmulti\`（katana under/upper + effect 共 9 个 ani）——习得巨剑精通 Lv3+ 时落地变多段斩。demo 跳过（被动条件分支 + 多段命中均延后/缺失档）。

**收尾（推断）**：落地动画播完回待机。

### 2.3 命中反应（.atk 实测）

- `ashenfork.atk`（下落刺击本体）：物理/no element/**down**/push 270 / lift 180/hit down。
- `ashenforksub.atk`（冲击波）：物理/**down**/**lift 300**（无 push 值）/no blood 20 1.0/R_SHOCKWAVE_HIT——冲击波主打**原地击倒浮空**不推走。
- 参照 `jumpattack.atk`（普通跳攻）：damage bonus 10/down/push 270 / lift 180（银光版除无 bonus 外同构）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/jumpattack.ani`（下落刺击） | 6（F0-5） | 300ms（100/50/50/40/30/30） | 无 | **F2**（窄高贯地盒） | 与普通跳攻共用动作（.chr [jumpattack motion] + etc 槽 180 重复引用）；仅引 `sm_body%04d.img` |
| `character/swordman/effect/animation/jumpattackhold.ani`（下落光效） | 2 | 200ms | 无 | 无 | `JumpAttackHold.img`（skl 预加载声明互证） |
| `passiveobject/.../ashenforksub.ani`（冲击波环） | 6 | 330ms（55×6） | 无 | **F0-F3**（±94→±110 渐扩） | `Common/CommonEffect/EarthQuakeRing.img` |
| `passiveobject/.../ashenforksubdust.ani`（尘土层） | 11 | 537ms | 无 | 无 | `Common/CommonEffect/EarthQuake.img` |
| `passiveobject/.../ashenforksubflash.ani`（白闪层） | 2 | 200ms | 无 | 无 | `Monster/Common/MonsterDieFlash.img`（**借用怪物死亡白闪贴图**） |
| `character/swordman/animation/jumpattackmultislash1/2.ani`（巨剑精通多段） | 未提取 | — | — | — | etc 槽 23/24；条件分支，demo 跳过 |

`.als` 边车：**无**（两侧 animation 目录 ls 实证；`ashenfork_ds\*` 与 `*_ds` 系列均为剑影变体，非本技能）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | AshenFork.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\AshenFork.skl` | ✅ 实测 | 技能数据（CD4000/3 列/预加载声明） |
| lst 条目 | ID 16 | `…\pvf\skill\swordmanskill.lst` 61-62 行 | ✅ 实测 | — |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ **缺失（引擎内置状态）** | — |
| 跳跃状态 nut | swordman_jump.nut（零命中佐证） | `…\pvf\sqr\character\swordman\jump\swordman_jump.nut` | ✅ 实测（无本技能引用） | 跳跃状态机（触发源推断） |
| 常量 | —（无 CUSTOM_ANI_ASHENFORK） | `…\pvf\sqr\character\swordman\swordman_header.nut` | ⛔ 无常量（共用 [jumpattack motion] 无需 etc 槽） | 唯一不走 etc motion 的本批技能 |
| .chr 条目 | [jumpattack motion]（923 行）+ etc motion #180（1153 行重复）；etc attack info #10（1304 行）+ [jumpattack info]（1282 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | JumpAttack.ani / AshenFork.atk / JumpAttack.atk 映射 |
| 角色 .ani | jumpattack.ani | `…\pvf\character\swordman\animation\jumpattack.ani` | ✅ 实测 | 6 帧 300ms，F2 贯地盒 |
| 角色 .atk | ashenfork.atk / jumpattack.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 银光刺击 / 普通跳攻（同动作不同参数） |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失（本技能无边车） | — |
| PO 定义 | ashenforksub.obj（+ashenforksub_ds.obj 剑影变体） | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 冲击波（§2.2 表） |
| PO .ani | ashenforksub.ani / ashenforksubdust.ani / ashenforksubflash.ani（+ ashensub.[pvp].ani 变体） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 冲击波三层视觉 |
| PO .atk | ashenforksub.atk（+ashenforksub_ds.atk） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | 冲击波命中（down/lift300） |
| 下落光效 | jumpattackhold.ani | `…\pvf\character\swordman\effect\animation\jumpattackhold.ani` | ✅ 实测 | 落刃蓄能光（2 帧） |
| 多段分支资源 | jumpattackmultislash1/2.ani + effect\jumpattackmulti\*9 | 两侧 animation 目录 | ✅ 实测（存在） | 巨剑精通条件分支，记档跳过 |
| 装备层 | jumpattack.ani ×86 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,coat,face,hair,neck,pants,shoes}\*\` | ✅ 实测（find 计数 86，含 [pvp] 变体） | 各 avatar 变体图层（只查存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img（帧索引集） | sprite_character_swordman_equipment_avatar_skin.NPK | 下落刺击动画图集 | 必需（共享） | ✅ `sm_body0000.img.bytes` 已在库 |
| Common/CommonEffect/EarthQuakeRing.img | sprite_common_commoneffect.NPK | 冲击波环主视觉（ashenforksub.ani 6 帧） | **必需**（冲击波是本技能核心表现，Area ViewAnimId 靠它） | ❌ 未入库 |
| Common/CommonEffect/EarthQuake.img | 同上 | 冲击波尘土层（ashenforksubdust.ani 11 帧） | 必需（建议，主视觉伴生层） | ❌ 未入库 |
| Monster/Common/MonsterDieFlash.img | sprite_monster_common.NPK | 冲击波白闪层（ashenforksubflash.ani 2 帧） | 可选 | ❌ 未入库 |
| Character/Swordman/Effect/JumpAttackHold.img | sprite_character_swordman_effect.NPK | 下落蓄能光效（jumpattackhold.ani 2 帧） | 可选 | ❌ 未入库 |

必需 img **2 张**（跨 1 个 NPK：sprite_common_commoneffect）+ 可选 2 张。注意冲击波贴图在 **Common 通用资源树**——本批首见"非 swordman 目录资源"，提取清单别漏。

## 5. 实现方案草案

- **前置说明（重要简化）**：demo 无跳跃/空中状态系统（§7/§8），本草案做**地面直放版**——原地落刃下砸 + 落点冲击波，保留核心手感（贯地刺击盒 + 贴地击倒冲击波）。跳跃版等空中状态系统落地后把 OnCast 换到跳跃输入分支即可（草案已预留）。
- **内容件清单**：
  - `DotNet~/Skills/AshenForkSkill.cs : SkillLogic`——同 `BloodBoomSkill` 范式：
    - `CooldownMs = 4000`（原值直用）；`TotalTimeMs = 550`（刺击 300ms + 落地缓冲 250ms）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanJumpAttack)` + `ctx.ClearHitTargets()`（空中版改：跳跃态按 Z 才可 TryCast——等状态机）。
    - **刺击盒走帧驱动**——jumpattack.json 自带 F2 贯地盒（z 至 1.67 单位），帧驱动自动激活。
    - `OnUpdate`：SubState 守卫，elapsed ≥ 300ms（落地时点）`ctx.CreateAreaInFront(AreaIds.AshenFork, FP.Zero)`（冲击波以落点为中心）。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
    - `HitReaction`（static readonly，刺击段）：`Damage=70 / HitstunMs=500 / KnockbackX=270 / LaunchY=180`（ashenfork.atk 原值）。
  - `DotNet~/Areas/AshenForkArea.cs : AreaDefinition`——同 `ReleaseWaveArea` 范式（一次性爆发）：
    - `TotalTimeMs=330`（PO 动画 6 帧）、`TickTimeMs=0`、`EnterActions={MeleeHit}`、
    - `HalfExtents=(1.1, 0.35, 0.2)`（PO 盒 ±110×37×17 折算，贴地扁盒）；
    - `HitReaction{Damage=90, HitstunMs=800, KnockbackX=0, LaunchY=300}`——ashenforksub.atk 原值：无 push、lift 300 + down = 原地击倒浮空（与 releasewave 击退型对照，本技能是**纯浮空不推走**型）；
    - `ViewAnimId=AnimId.AshenForkSubRing`（EarthQuakeRing）+ `ViewBackAnimId=AnimId.AshenForkSubDust`（尘土背层，boomback 同构用法）。
  - **不需要新 Action/Buff/Bullet**。
- **概念映射**：引擎跳跃态触发 → 地面直放简化（TryCast 门禁留接口）；jumpattack.ani F2 盒 → 帧驱动盒；ashenforksub PO → `AreaDefinition`（单相位，无需 L9 多 Area 编排）；skl col1/col2 → Area 伤害/大小（大小缩放延后）；高度分档 → 简化固定值。
- **注册点清单**：

  | 什么 | 在哪 | 增量 |
  |---|---|---|
  | SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.AshenFork = 14` + `ButtonToSkill` case 10 |
  | AreaId | `Packages\cn.etetet.skill\Runtime\AreaDefinition.cs` | `AreaIds.AshenFork = 4`（接现有 3 之后） |
  | AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanJumpAttack=49`、`AshenForkSubRing=50`、`AshenForkSubDust=51`（+可选 `AshenForkSubFlash=52`、`JumpAttackHold=53`） |
  | json 注册 | `…\LSAnimClipRegistrar.cs` | `RegisterOne` ×3~5 |
  | 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | `EarthQuakeRing.img.bytes` + `EarthQuake.img.bytes`（必需两张；可选两张一并） |
  | 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 10 |

- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 4000ms | 4000 |
| 总时长 | 刺击 300ms（动作 6 帧，空中悬停实际更长）+ 落地 | 550 |
| 刺击判定 | F2 贯地盒（z 至 167px=1.67 单位） | 直用 json 盒 |
| 刺击伤害 | col0 185%（Lv1）×高度分档（static data 未考证） | MeleeHit 固定 70 |
| 刺击反应 | down/push270/lift180 | Kb270/Ly180/Hitstun500 |
| 冲击波伤害 | col1 185（Lv1，固定值型） | Area Damage 90 |
| 冲击波反应 | down/**lift 300**/无 push | Ly300/Hitstun800/Kb0 |
| 冲击波范围 | ±110px（col2 100% 基准） | HalfExtents 1.1 单位 |
| 冲击波时点 | 落地帧（引擎） | elapsed 300ms |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `AshenFork.skl` | `.skl` 尚无子命令（3 列 level info + static data + **[skill preloading image] 节**） | 手抄可行；`[skill preloading image]` 建议随 skl 子命令一并 dump（资源预载清单有提取价值） |
| `ashenfork.atk` / `ashenforksub.atk` / `jumpattack.atk` | `.atk` 尚无子命令 | 手抄（每文件 ~8 值） |
| `ashenforksub.obj` | `.obj` 尚无子命令 | **单相位 + 双视觉附层**（etc motion 无配对 etc attack info——纯视觉，不产生判定切换，与 GoreCross 多相位不同）；手映射为 1 Area + ViewBackAnimId 即可，比 GoreCross 简单 |
| PO 三个 .ani | `[SHADOW]`（值 0） | 整节跳过无碍（GoreCross 先例） |
| `jumpattackhold.ani` 等 | 无其他新节 | 常规 ani 子命令全覆盖 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 跳跃状态中施放（前置状态门禁） | **跳跃/空中状态系统缺失（新缺口，§8）** | 地面直放版（§5）；跳跃版留接口后补 |
| 跳得越高伤害越强 + 高度阈值出冲击波（static data 分档） | 无空中高度量（依赖跳跃系统）；等级缩放延后 | 固定伤害 + 恒出冲击波 |
| 冲击波大小随 col2 缩放（PO 盒/图像同步缩放） | 对象整体缩放（延后，GoreCross/IceWave 同类） | 固定 100% 基准值 |
| 巨剑精通 Lv3+ 多段斩（jumpattackmultislash1/2 + 9 特效） | 被动条件分支无系统（Buff 查询门面缺失）+ 多段命中延后 | 跳过 |
| 刺击帧下落中持续判定（引擎循环停驻 F2） | 帧驱动盒=单帧窗口；空中悬停时长未知 | 地面版一次判定即可；跳跃版落地前用 Area 常驻或循环盒替代（后补） |
| 白闪层借 Monster 贴图 | 无（跨目录资源正常提取） | 照常提取 sprite_monster_common.NPK |
| 音效（R_SHOCKWAVE_HIT） | 延后（无音频系统） | 跳过 |
| MP 消耗 | 延后 | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. static data `50 10 50 100` 语义（推断高度阈值/伤害分档；pvp 第 4 值 75≠100 佐证"分档参数"而非纯标记）。
2. col0/col1/col2 精确语义（模板对位推断，无 nut 佐证；col1/col2 与 PO 存在互证）。
3. 下落中判定帧的循环/停驻机制（引擎内置；.ani 仅 6 帧 300ms 表达不了长下落）。
4. 冲击波触发时点（推断=落地帧；无帧标记可查）。
5. jumpattack.ani 被 [jumpattack motion] 与 etc 槽 180 **双重引用**的分工（推断：前者普通跳攻、后者银光落刃状态各自取用同一动作）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **跳跃/空中状态系统**：银光落刃（跳跃中施放）与连突刺（前冲中施放）同批撞上"状态前置型技能"——当前 LSCast 只能从地面待机施放。补齐需要：跳跃输入/竖直位移/空中施法门禁/落地事件。本批第二次实证，建议 01§0.4 表增补（与连突刺上报合并为一条）。
2. **对象整体缩放**：col2 冲击波大小%再证（GoreCross/IceWave 已报，累计 3 例——DnfConfigTranslation 立项时一并考虑 imageSizeRate 字段）。

**给下轮的经验**：冲击波型 PO 贴图在 `Common\CommonEffect\`（地震环/尘土是**通用资源**，很多落击系技能会复用——提取一次多技能受益）；PO 白闪借 Monster 贴图说明**跨目录复用是常态**，img 清单要按 .ani 里的完整路径推 NPK，不能按技能目录猜。`.obj` 的 etc motion 若无 etc attack info 配对则是纯视觉层（不产生 L9 多相位问题）。
