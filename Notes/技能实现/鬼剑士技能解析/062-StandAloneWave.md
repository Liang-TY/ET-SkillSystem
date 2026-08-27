# 无双波（StandAloneWave）

> 技能ID 62 | 级别 B（预分类；实为 A 类主动攻击·吸附控场，见 §8 纠偏） | 可实现性 🔶（吸附可用负 KnockbackX 零新机制表达；连打加速/按吸附数增伤/物防增益需简化） | 分析日期 2026-08-22 | 批次 B4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 无双波 | `skill\Swordman\StandAloneWave.skl [name]` |
| 英文名 | StandAloneWave（取 skl 文件名；[name2]="Peerless Wave"） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype] 4） | 同上 |
| 学习等级 | 35（**前置：技能 52 Lv1** = 挫折意志系） | 同上 [required level] / [pre required skill] |
| 最高等级 | 70（六系各 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type]/[skill class] |
| 指令 | ↓↑ + Z | 同上 [command] / [command key explain] |
| CD | 20000 ms | 同上 [dungeon][cool time] |
| MP | 140 → 1176 | 同上 [dungeon][consume MP] |
| 读条 | casting time 300 ms | 同上 |
| 消耗品 | 无色小晶块 ×1 | 同上 [consume item] |
| 施放状态门禁 | [executable states] **8**（实测——仅普攻状态中可用；explain 另要求[杀意波动]施放中 + [挫折意志]状态，引擎侧校验） | 同上 [executable states] + explain |
| static data | `300 100 5 300 5 3 3 4 4 3 480 140 80 200 1 0`——[0]=300（启动所需最少时间×0.001=0.3s）、[4]=5（攻击力上限所需吸附总数）；其余 14 值引擎消费未考证 | 同上 [static data] + [level property] 向量 |
| 一句话效果 | 自身周围生成强力吸附波吸住周围敌人（需杀意波动+挫折意志）；连按操作键加快波动速度；吸附 1 名以上敌人时出现冲击波；施放后增加自身物防，吸附敌人越多伤害越大；对领主/稀有/精英/深渊/霸体类伤害更大 | 同上 [explain] |

**level property（4 列，Lv1 → Lv70）**：魔法攻击力 col0=`800→…`；冲击波魔法攻击力 col1=`2880→…`；每吸附 1 名敌人时增加攻击力 col2=`576→…`；攻击力上限吸附总数=static[4]=**5**；增加物防 col3=`3936→…`；启动所需最少时间=static[0]=**0.3s**。（模板 6 项全明，L21 法；col 语义无一未考证。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
108: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/Standalonewave/Standalonewave.nut", "StandAloneWave", 34, 57);
```

⚠ **归属错位（本技能核心迷雾，实测澄清）**：状态 34 虽名为 StandAloneWave，但第 5 参（技能 ID，L2）挂的是 **57（邪光波动阵）**——施放技能 57 被引擎先送入状态 34。无双波（62）自己**没有绑定注册行**，它的施放由代理网转入状态 34（见 §2.2）。nut 目录 `standalonewave\` 与技能同名纯属巧合陷阱（023-Sacrifice 同款双陷阱）���

### 2.2 主 nut 逐回调（standalonewave.nut，67 行，全量实测——纯代理，无施法逻辑）

**`onProcCon_StandAloneWave(obj)`**（处于状态 34 时逐帧）：
```
若技能 74(不动明王阵) 不在 CD：启用其指令 → 若输入 → 写包 8 参
   (throwState=0, throwType=0, throwIndex=74, chargeTime=0, shootTime=3400, animIdx=1, …)
   → sq_AddSetStatePacket(13, …)   // 进 THROW 状态放不动明王阵
若技能 57(邪光波动阵) 不在 CD：启用其指令 → 若输入 → 进状态 31（捶地）
```
**`onProc_StandAloneWave(obj)`**（isBattleMode 下）：
```
若 SKILL_WAVESPINAREA(74) 不在 CD 且输入：手动 startSkillCoolTime(74) +
   写包 11 参（shootTime = sq_GetIntData(74,0)，chargeSpeed 1000/1000，personalCastRange=-1）
   → 进 THROW 状态 13
```
（对照：shockwavearea.nut 的 onProcCon 是镜像——在状态 31 里检测技能 62 输入 → 进状态 34。**31⇄34 互跳 + 34→13**，即"阵三连"技能取消网。）

**结论**：无双波本体的施法行为（Ready→吸附→冲击波）**全在引擎内置状态 34 中**，nut 只是它与其他两阵的互跳代理。

### 2.3 被动对象：无双波 PO 20027

`passiveobject\character\swordman\standalonewave.obj`（passiveobject.lst:11187 实测 ID **20027**）：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [basic motion] | `Animation/StandAloneWave.ani`（9 帧，`light/mu-explode.img`） | 吸附波本体视觉 |
| [attack info] | `AttackInfo/StandAloneWave.atk`（PO 侧） | magic/light element / **down / push 500 / lift 200 / blow**（冲击波击飞） |
| [object destroy condition] | on end of animation | 播完即毁（单相位 PO，L13 无 etc 序列） |

攻击盒实况：**基础版 StandAloneWave.ani 无攻击盒；`standalonewave_light\standalonewave.ani`（光属性变体，9 帧）携带 18 个攻击盒**（实测计数）——判定盒在 light 变体里（与 wavespinarea.obj 直接引用 `_Light` 目录动画同规律，光属性版本才是"真身"）。

角色侧 .atk：`attackinfo\StandAloneWave.atk`（[etc attack info] 槽 22 实测）：**down / push 200 / lift 200**（引擎内置状态 34 施放中使用的角色侧命中）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `StandAloneWaveReady.ani`（.chr etc #20） | **1** | **3000ms** | 无 | 无 | 蓄势悬停帧（L23 同型：超长 DELAY=事件推进——吸附期姿态，引擎按吸附结束/输入推进） |
| `StandAloneWaveBasic.ani`（.chr etc #21） | 5 | 760ms | 无 | 无 | 基础收势 |
| `StandAloneWaveStrong.ani`（.chr etc #22） | 5 | 1030ms | 无 | 无 | 强化收势（推断：吸附≥1 敌出冲击波时用） |
| `passiveobject\...\animation\standalonewave.ani`（PO 20027） | 9 | 未逐帧加总 | 无 | 无（判定在 atk/引擎） | mu-explode.img（light 子目录） |
| `standalonewave_light\standalonewave.ani`（光变体） | 9 | 未逐帧加总 | 无 | **×18** | 判定盒载体 |
| `effect\animation\standalonewave\{blast1,blast2,charge1,charge2,floor,slash1,slash2}.ani` | — | — | 无 | 无 | 引擎惯例特效（mu-* / BlackHole 借图，§4） |

`.als` 边车：**两侧均无**（ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | StandAloneWave.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\StandAloneWave.skl` | ✅ | 数据（4 列全明） |
| 注册行 | load_state:108 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅（但第 5 参挂的是技能 57，§2.1） | 状态 34（归属错位） |
| 主 nut | standalonewave.nut | `…\pvf\sqr\character\swordman\standalonewave\standalonewave.nut` | ✅（67 行纯代理） | 阵三连互跳；无双波本体引擎内置 |
| 镜像代理 | shockwavearea.nut | `…\pvf\sqr\character\swordman\shockwavearea\shockwavearea.nut` | ✅ | 状态 31 内检测技能 62 → 进状态 34 |
| .chr 条目 | etc #20/#21/#22（Ready/Basic/Strong.ani）+ etc attack #22 | `…\pvf\character\swordman\swordman.chr` 993/994/995/1316 行 | ✅ 实测 | 三段施法动画 + 角色侧 atk |
| 角色 .ani | StandAloneWaveReady/Basic/Strong.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | StandAloneWave.atk | `…\pvf\character\swordman\attackinfo\StandAloneWave.atk` | ✅ | down/200/200 |
| PO 定义 | standalonewave.obj（20027） | `…\pvf\passiveobject\character\swordman\standalonewave.obj` | ✅ | 单相位吸附波 |
| PO .atk | standalonewave.atk | `…\pvf\passiveobject\character\swordman\attackinfo\standalonewave.atk` | ✅ | down/push500/lift200/blow |
| PO .ani | standalonewave.ani + standalonewave_light\（判定盒变体） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | §2.4 |
| 特效 .ani | standalonewave\ 目录 7 个 | `…\pvf\character\swordman\effect\animation\standalonewave\` | ✅ | 引擎惯例特效 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 无 | — |
| 装备层 | 未查 | `…\pvf\equipment\character\swordman\avatar\` | 未考证 | 老一代引擎动画惯例 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动画帧 | 必需（共享） | ✅ 已在库 |
| light/mu-explode.img（`Character/Swordman/Effect/StandAloneWave/light/`） | sprite_character_swordman_effect_standalonewave_light.NPK | PO 吸附波主视觉 | **必需** | ❌ 未入库 |
| mu-explode-1.img / mu-Asura-explode.img | sprite_character_swordman_effect_standalonewave.NPK | blast1/blast2 冲击波特效 | 可选 | ❌ |
| mu-energy.img / mu-Asura.img | 同上 | charge1/charge2 蓄能特效 | 可选 | ❌ |
| basis_swing.img / mu-swing.img | 同上 | slash1/slash2 挥波特效 | 可选 | ❌ |
| bottom.img（`Character/Mage/Effect/BlackHole/`，floor.ani 借用） | sprite_character_mage_effect_blackhole.NPK | 地面旋涡（**跨职业借图，L14 同例**） | 可选 | ❌ |

缺失 img：必需 1、可选 6（分属 3 个 NPK；主视觉一个 NPK 即可开跑）。

## 5. 实现方案草案

1. **`DotNet~/Skills/StandAloneWaveSkill.cs : SkillLogic`**（BloodBoomSkill 自身中心范式 + FireCircle 持续区）
   - `CooldownMs = 20000`；`TotalTimeMs = 3800`（启动 0.3s + 吸附 3s（Ready 悬停帧实测 3000ms）+ 收势 0.5s）。
   - `OnCast`：`ctx.PlayAnim(AnimId.StandAloneWaveReady)`（1 帧 3000ms 悬停，翻译时把 DELAY 钳到 3000 见 §6）+ `ctx.CreateArea(AreaIds.StandAloneWaveZone, 0)`（自身中心吸附区）。
   - `OnUpdate`：3300ms 处切 `PlayAnim(AnimId.StandAloneWaveStrong)`（≥1 敌被吸时）/否则 Basic——判定：`ctx.GetEnemies()` 逐个 `CheckHit` 自身攻击盒（Area 内有敌近似），SubState 记录；吸附结束帧（≈3300ms）`CreateArea(AreaIds.StandAloneWaveBlast, 0)` 冲击波。
2. **`DotNet~/Areas/StandAloneWaveZoneArea.cs : AreaDefinition`**（吸附区）
   - `TotalTimeMs = 3000`、`TickTimeMs = 300`、`HalfExtents = (3.0, 0.5, 3.0)`（吸附半径，static 无直读值按视觉 300px 量级取）、`TickActions = { MeleeHit }`；
   - **吸附 = 每 Tick 负击退**：`HitReaction{Damage=30, HitstunMs=200, KnockbackX=-150, LaunchY=0}`——负 KnockbackX 经 LaunchOwner 天然反向=**拉向施法者**（L22 鬼影鞭实证，零新机制）；低伤害小硬直模拟"吸住"。
3. **`DotNet~/Areas/StandAloneWaveBlastArea.cs : AreaDefinition`**（冲击波）
   - `TotalTimeMs = 400`、`EnterActions={MeleeHit}`、`HalfExtents=(3.5,0.6,3.5)`、`HitReaction{Damage=160, HitstunMs=800, KnockbackX=500, LaunchY=200}`（PO standalonewave.atk 原值：down/push500/lift200/blow）、`ViewAnimId=AnimId.StandAloneWaveBlast`（blast1/2 视觉）。
4. 需要新增的 Action/Buff/Bullet：无。
   - 每吸附 1 名增伤（col2×N，上限 5 名）与物防+col3：**简化跳过**（§7）。

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.StandAloneWave = 29` + 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.StandAloneWaveZone = 32`、`StandAloneWaveBlast = 33` |
| AnimId | npkparser `AnimConfigRegistry.cs` | `StandAloneWaveReady=140`、`…Basic=141`、`…Strong=142`、`StandAloneWavePo=143`（PO 波视觉）、`…Blast=144`（blast1） |
| json/图集/按键 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem | 5 个 json + mu-explode 系 img |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms | 20000 直用 |
| 启动最少时间 | static[0]=0.3s | 300ms |
| 吸附持续 | Ready 悬停帧 3000ms（实测） | Area 3000ms / Tick 300ms × 10 跳 |
| 吸附力 | 引擎内置（无数据） | Kb -150/Tick（往自身拉） |
| 波动基础魔攻 | col0=800% | Tick 30 × 10 |
| 冲击波魔攻 | col1=2880%（+col2×吸附数≤5） | 单发 160 |
| 冲击波命中 | PO atk：down/push500/lift200/blow | Kb 500 / Ly 200 / 硬直 800 |
| 物防增加 | col3=3936+ | 跳过（无消费链） |
| 施放门槛 | 杀意波动���+挫折意志+状态8 | 跳过（demo 直放） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `StandAloneWave.skl` | `.skl` 无子命令 | 手抄 4 列 + static 2 值 |
| 角色/PO `.atk` ×2 | `.atk` 无子命令 | 手抄 |
| `standalonewave.obj` | `.obj` 无子命令 | 不需直译（单相位已手抄） |
| `StandAloneWaveReady.ani` | **单帧 DELAY=3000ms 超长悬停**（L23 同族：事件推进帧） | 翻译工具钳制/消费侧约定（既有缺口再证）；demo 侧 Area 时长独立控制，不受影响 |
| 各 .ani | 常规节 | 现有 ani 子命令全覆盖 |

结论：动画资源全部可译；缺口 = `.skl`/`.atk`/`.obj`（既有）+ 超长 DELAY（既有记档再证），无新增。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 吸附（持续把周围敌人吸到身边） | **已通**（L22：负 KnockbackX=拉拽，LaunchOwner 天然反向） | Tick 负击退直译；无"抓定身"手感差异用短硬直补 |
| 连续按操作键加快波动速度 | **缺失档：技能二段交互门面**（R4-A16 三技共撞）+ 无动画变速 ctx 门面 | 固定速度 |
| 吸附人数越多伤害越大（col2×N≤5） | **缺失档：目标枚举/计数门面**（R3-A11 记档） | 固定伤害（或 Area 内单位数在 OnUpdate 手动数——GetEnemies+CheckHit 可近似，但帧同步成本高，不建议 demo 做） |
| 物防+col3（自身增益） | **缺失档：属性数值消费链**（Defense 零消费，NumericType.Speed 姊妹实证） | 跳过 |
| 领主/稀有/精英/深渊/霸体类增伤 | 缺失档：怪物品级/霸体属性系统 | 跳过 |
| 施放门槛（杀意波动 buff 在场 + 挫折意志 + 仅状态8） | **缺失档：自身 Buff 查询门面**（R4-A18）+ 技能取消体系（状态门禁） | demo 直放 |
| 吸住敌人不可逃跑（hold 定身语义） | 部分：无 hold 微控（021 同条） | 短硬直+拉拽组合近似 |
| 音效/屏震 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. 吸附半径精确值（static 16 值中无直读；按 PO 视觉/BlackHole floor 特效量级取 300px）。
2. Basic vs Strong 两版收势动画的切换条件（推断：吸附≥1 → Strong 出冲击波）。
3. static 其余 14 值语义（引擎消费）。
4. 无双波被吸入时的"波动持续掉血"节奏（引擎内置，无数据）——demo 用 Tick 近似。

**纠偏**：预分类 B → **实为 A**（主体=吸附控场+冲击波伤害）。B 类深度已完成。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **pushState 第 5 参也失准的实证**（状态 34 名为 StandAloneWave 却挂技能 57；技能 62 无绑定行）——归属判定需"第 5 参 + header 常量 + 资源名"三方印证（与 057 §8 同条，合并记档）。
2. 连打加速（二段交互 + 动画变速门面）——R4-A16 已记，本技能是"加速"形态（239/242/230 是再按触发形态），实现时合并评估。

**翻译工具缺口**：超长 DELAY 再证（既有）；无新增。
