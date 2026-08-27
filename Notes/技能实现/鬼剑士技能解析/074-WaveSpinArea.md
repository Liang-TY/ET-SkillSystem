# 不动明王阵（WaveSpinArea）

> 技能ID 74 | 级别 B（预分类；实为 A 类主动攻击·持续阵，见 §8 纠偏） | 可实现性 🔶（焰珠旋转多段+终结爆炸主干可表达；连按加速/波动印资源系统需简化） | 分析日期 2026-08-22 | 批次 B4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 不动明王阵 | `skill\Swordman\WaveSpinArea.skl [name]` |
| 英文名 | WaveSpinArea（取 skl 文件名；[name2]="Acalanatha array"） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype] 4） | 同上 |
| 学习等级 | 45（**前置：技能 2 Lv1**，lst 2=ReflectGuard——归属存疑，explain 明示真门槛是"波动印出现后"） | 同上 [required level] / [pre required skill] |
| 最高等级 | 70（六系各 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type]/[skill class] |
| 指令 | ↑↓↑ + Z | 同上 [command] / [command key explain] |
| CD | 45000 ms | 同上 [dungeon][cool time] |
| MP | 400 → 3360 | 同上 [dungeon][consume MP] |
| 读条 | casting time 500 ms | 同上 |
| 消耗品 | 无色小晶块 ×2 | 同上 [consume item] |
| 施放状态门禁 | [executable states] 0 8 14（站立/普攻/14）；另有"波动印出现后才能施放"（引擎侧资源门槛） | 同上 [executable states] + explain |
| static data | `4000 900 7 170 350 300`——[0]=**旋转时间 4000ms**（模板 (0,0,0.001) 实证=4s）、[4]=**波动阵范围 350px**（模板 (4,4,1.0) 实证）；[1]=900/[2]=7/[3]=170/[5]=300 语义未考证（[2] 疑焰珠数量） | 同上 [static data] + [level property] 向量 |
| 一句话效果 | 生成使敌人浮空的不动明王阵，释放旋转于阵周边的焰珠攻击敌人（仅波动印出现后可放）；连按攻击键提高焰珠转速，焰珠旋转一定次数后消失并引发魔法爆炸；波动印越多焰珠越多；施放期间不会被敌人抓取 | 同上 [explain] |

**level property（2 列，Lv1 → Lv70）**：焰珠魔法攻击力 col0=`922→…`；爆炸魔法攻击力 col1=`4004→…`；旋转时间=static[0]=4s；波动阵范围=static[4]=350px。（模板 4 项全明：两列走 level（源 -2）、两值走 static 槽，L21 法直解。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无 pushState 注册行**（grep wavespin 实测仅代理代码引用）。施放链路（实测三方拼接）：

```
// 入口 1：处于状态 34（无双波）时——standalonewave.nut onProcCon（8 参版，shootTime=3400 字面量）
   技能 74 输入 → 写包(throwState=0, throwType=0, throwIndex=74, chargeTime=0,
                     shootTime=3400, animIdx=1, speedType 4/4, …) → sq_AddSetStatePacket(13)
// 入口 2：battle mode 下——standalonewave.nut onProc（11 参完整版）
   技能 74 输入 → 手动 startSkillCoolTime(74) → shootTime = sq_GetIntData(74,0)=4000
                     → chargeSpeedValue/shootSpeedValue=1000, personalCastRange=-1 → 状态 13
// 入口 3：处于状态 31（邪光波动阵）时——shockwavearea.nut onProcCon 同款 8 参版
```

即：**不动明王阵经引擎通用 THROW 状态 13（`swordman_throw.nut`）施放**——throwIndex=74、蓄力/施放速度参数 1000/1000、活动期 4000ms（=旋转时间）。`swordman_throw.nut` 内**无 case 74**（实测：只有 23/47/82/18/222）→ 状态 13 内对 74 的行为（焰珠生成/旋转/爆炸）**引擎内置**，资源与数值落在本节 PO 与 .skl。

### 2.2 引擎内置施法行为重建（THROW 机制 + PO 反推）

```
施放（读条 500ms）→ 进 THROW 状态 13（throwIndex=74）：
  施放者定身于阵中（THROW 态锁移动，"施放者不会被抓取"=霸体/免抓，延后档）
  创建阵视觉（effect: create.ani/createnormal 系 + circle 系三层圆环）
  生成焰珠（WaveSpinArea PO 20038；波动印越多珠越多——引擎按波动印记数分支，§7）
  旋转期 4000ms（static[0]）：焰珠绕阵周边旋转，触敌 → WaveSpinArea.atk（damage/push0/lift250/blow 浮空）
    连按攻击键 → 转速提高（代理参数 shootSpeedValue 1000 基准，引擎加速；无脚本细节）
  旋转期满 → 焰珠消失 + 引发魔法爆炸（WaveSpinAreaBomb.atk：down/push300/lift300）
    角色终结姿态 WaveSpinAreaBomb.ani（15 帧 1230ms，.chr etc #56）
  THROW 态结束 → 回站立
```

### 2.3 被动对象：不动明王阵 PO 20038（多相位，L9 结构）

`passiveobject\character\swordman\wavespinarea.obj`（passiveobject.lst:11209 实测 ID **20038**；**引用的全是 `WaveSpinArea_Light/` 光属性子目录动画**，实测）：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [basic motion] | `Animation/WaveSpinArea_Light/WaveSpinArea.ani`（1 帧，bead.img） | 相位 1：焰珠本体（视觉锚点帧） |
| [etc motion] | Dodge1.ani（3 帧）、Hot.ani（1 帧）、Hit.ani（3 帧）、Dodge1Hit.ani、HotHit.ani | 后续相位：旋转/受击表现（Hit 系 3 帧带攻击盒 ×3） |
| [attack info] | `AttackInfo/WaveSpinArea.atk` | magic/light / **damage / push 0 / lift 250 / blow**（焰珠命中=浮空） |
| [etc attack info] | `AttackInfo/WaveSpinAreaBomb.atk` | magic / **down / push 300 / lift 300 / blow**（终结爆炸） |

**判定盒**：`wavespinareahit.ani` 3 帧 ×3 攻击盒（实测计数）；bomb1/2/3.ani 分别 4/3/3 盒——多相位盒都在动画里（爆炸波及范围由 bomb 系动画盒给出）。
**爆炸视觉**：`wavespinareabomb1/2/3.ani`（12/11/11 帧，1/2/3-dodge.img + bead_disappear_normal.img；各自带 `.als` 层叠 bombdodge，实测）+ `wavespinareabombwave.ani`（8 帧，**借 Priest/BlueDragon/exp_nova.img**，L14 跨职业借图）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\WaveSpinAreaBomb.ani`（.chr etc #56） | 15 | 1230ms | 无 | 无 | 终结姿态（仅引 sm_body） |
| `passiveobject\...\animation\wavespinarea_light\wavespinarea.ani`（PO 相位1） | 1 | — | 无 | 无 | bead.img 锚点帧 |
| `wavespinareadodge1.ani`（PO 相位2） | 3 | 未逐帧加总 | 无 | 无 | bead-dodge.img |
| `wavespinareahit.ani`（PO 相位3） | 3 | 同上 | 无 | **×3** | bead.img（命中判定盒） |
| `wavespinareabomb1/2/3.ani`（爆炸，+.als ×3） | 12/11/11 | 未逐帧加总 | 无 | **4/3/3** | 1/2/3-dodge.img 系 |
| `wavespinareabombwave.ani` | 8 | 同上 | 无 | 无 | BlueDragon 借图 |
| `effect\animation\wavespinarea\` 15 文件（create/circle/hold 系，create.ani 与 holdfloor.ani 带 .als） | — | — | 无 | 无 | 阵/珠/地面标记视觉（mg-circle、target-effect 系） |

`.als` 边车：PO 侧 bomb1/2/3 ×3 + wavespinarea_light 目录同名 ×3（结构 `[use animation]`+`[add]` 常规，实测抽样）；角色侧无。

## 3. 关联文件清单（每行实测���

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | WaveSpinArea.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\WaveSpinArea.skl` | ✅ | 数据（2 列 + static 双键值全明） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无（经代理进 THROW 13） | 引擎内置（F3 变体） |
| THROW 载体 | swordman_throw.nut | `…\pvf\sqr\character\swordman\swordman_throw.nut` | ✅（无 case 74，仅确认载体） | 状态 13 通用蓄力/施放 |
| 代理 nut | standalonewave.nut / shockwavearea.nut | `…\pvf\sqr\character\swordman\{standalonewave,shockwavearea}\` | ✅ | 技能 74 输入 → 状态 13 写包（§2.1） |
| .chr 条目 | etc #56（WaveSpinAreaBomb.ani） | `…\pvf\character\swordman\swordman.chr` 1029 行 | ✅ 实测 | 终结姿态 |
| 角色 .ani | WaveSpinAreaBomb.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | —（无，attackinfo grep wavespin 无角色侧） | `…\pvf\character\swordman\attackinfo\` | ⛔ 无 | 判定全在 PO |
| PO 定义 | wavespinarea.obj（20038）+ _light/_ds 变体 | `…\pvf\passiveobject\character\swordman\` | ✅ | §2.3 |
| PO .atk | wavespinarea.atk / wavespinareabomb.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | lift250 / down+300+300 |
| PO .ani | wavespinarea.ani、dodge1/2、hit、hot、bomb1-3、bombwave��bombdodge、hothit、dodge1hit（+ light/ds 子目录） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | 阵/珠/爆炸视觉 |
| 特效 .ani | effect\animation\wavespinarea\ 15 文件（create/circle/hold 系） | `…\pvf\character\swordman\effect\animation\wavespinarea\` | ✅ | 阵视觉（含 2 个 .als） |
| .als | bomb 系 ×3 + light 目录 ×3 + create/holdfloor ×2 | 上述目录 | ✅（抽样常规） | 层叠特效 |
| 装备层 | 未查 | `…\pvf\equipment\character\swordman\avatar\` | 未考证 | 同族惯例无 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色/终结动画帧 | 必需（共享） | ✅ 已在库 |
| bead.img（`Character/Swordman/Effect/WaveSpinArea/`） | sprite_character_swordman_effect_wavespinarea.NPK | 焰珠本体（PO 锚点 + hit 帧） | **必需** | ❌ |
| 1-dodge / 2-dodge / 3-dodge.img | 同上 | 爆炸 1/2/3 段 | **必需**（爆炸视觉） | ❌ |
| bead_disappear_normal.img | 同上 | 珠消散 | 可选 | ❌ |
| bead-dodge.img / effect.img（dodge2 用） | 同上 | 旋转残影 | 可选 | ❌ |
| mg-circle / mg-circle-back / mg-circle-front.img | 同上 | 阵三环 | 可选（阵氛围） | ❌ |
| action.img / action_normal.img | 同上 | create 特效 | 可选 | ❌ |
| target-effect / -dodge / -light.img、target-selet.img、bead_sub_dodge.img | 同上 | hold 地面标记系 | 可选 | ❌ |
| exp_nova.img（`Character/Priest/Effect/BlueDragon/`，bombwave 借用） | sprite_character_priest_effect_bluedragon.NPK | 爆炸波（跨职业借图，L14） | 可选 | ❌ |

缺失 img：必需 4、可选 12；除借图 1 张外**全部同一 NPK**（一次提取全覆盖）。

## 5. 实现方案草案

1. **`DotNet~/Skills/WaveSpinAreaSkill.cs : SkillLogic`**（FireCircleSkill 持续区 + 自身中心范式）
   - `CooldownMs = 45000`；`TotalTimeMs = 5300`（旋转 4000 + 爆炸收势 1230）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanWaveSpinHold)`（无专门施法姿态动画——用 hold 特效层做主体，角色播通用施法姿态或 Idle；§7）；`ctx.CreateArea(AreaIds.WaveSpinOrbZone, 0)`（自身中心）；`ctx.ClearHitTargets()`。
   - `OnUpdate`（SubState 守卫）：4000ms 处 `CreateArea(AreaIds.WaveSpinBomb, 0)` 终结爆炸 + `ctx.PlayAnim(AnimId.SwordmanWaveSpinBomb)`（15 帧终结姿态 1230ms）。
2. **`DotNet~/Areas/WaveSpinOrbZoneArea.cs : AreaDefinition`**（焰珠旋转区 = 同段定时多段，L19 中档）
   - `TotalTimeMs = 4000`（static[0] 原值）、`TickTimeMs = 400`（10 跳；"旋转一定次数后消失"的节奏近似）、`HalfExtents = (3.5, 0.5, 3.5)`（static[4]=350px）、`TickActions = { MeleeHit }`；
   - `HitReaction{Damage=35, HitstunMs=300, KnockbackX=0, LaunchY=250}`（wavespinarea.atk 原值：push0/lift250/blow=持续挑空）；
   - `ViewAnimId = AnimId.WaveSpinOrb`（bead 锚点）+ 阵视觉层走 Area overlay（circle 三环，手组装——releasewave 8 层先例）。
3. **`DotNet~/Areas/WaveSpinBombArea.cs : AreaDefinition`**（终结爆炸）
   - `TotalTimeMs = 500`、`EnterActions={MeleeHit}`、`HalfExtents=(3.8,0.6,3.8)`、`HitReaction{Damage=180, HitstunMs=800, KnockbackX=300, LaunchY=300}`（bomb.atk 原值：down/push300/lift300）、`ViewAnimId=AnimId.WaveSpinBomb1`（12 帧爆炸）+ `ViewEndAnimId=AnimId.WaveSpinBombWave`（BlueDragon 波，可选）。
4. 需要新增的 Action/Buff/Bullet：无。
   - 连按加速 / 波动印珠数 / 免抓取：**简化跳过**（§7）。

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.WaveSpinArea = 30` + 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.WaveSpinOrbZone = 34`、`WaveSpinBomb = 35` |
| AnimId | npkparser `AnimConfigRegistry.cs` | `SwordmanWaveSpinBomb=145`（角色姿态）、`WaveSpinOrb=146`、`WaveSpinCircle=147`、`WaveSpinBomb1=148`、`WaveSpinBombWave=149`；可选 hold 系 150-152 |
| json/图集/按键 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem | 5+ json + wavespinarea NPK 图集 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000ms | 45000 直用 |
| 旋转时间 | static[0]=4000ms | 4000 / Tick 400ms × 10 |
| 阵范围 | static[4]=350px | HalfExtents 3.5 |
| 焰珠攻击 | col0=922%；atk push0/lift250/blow | Tick 35 / Ly 250（持续挑空） |
| 爆炸攻击 | col1=4004%；bomb.atk down/300/300 | 单发 180 / Kb 300 / Ly 300 / 硬直 800 |
| 焰珠数量 | 波动印数分支（引擎内置，无数据） | 固定 1 区（视觉 3 珠可用 overlay 层表现） |
| 施放总长 | THROW 4000 + Bomb 1230 | TotalTimeMs 5300 |
| 读条 | 500ms | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `WaveSpinArea.skl` | `.skl` 无子命令 | 手抄 2 列 + static 2 键值（全明） |
| PO `.atk` ×2 | `.atk` 无子命令 | 手抄 |
| `wavespinarea.obj` | `.obj` 无子命令 | 不需直译（相位已手抄） |
| 各 .ani（15 特效 + 12 PO） | 常规节（含 RGBA/GRAPHIC EFFECT 均已支持，L15） | 现有 ani 子命令全覆盖 |
| 8 个 `.als` | `[use animation]`/`[add]` 常规（抽样实测） | 现有 als 子命令覆盖 |

结论：动画/边车资源全部可被现有 ani/als 子命令翻译；实质缺口仅 `.skl`/`.atk`/`.obj`（既有三类）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 连按攻击键提高焰珠转速 | **缺失档：技能二段交互门面**（R4-A16）+ 动画变速 ctx 门面缺失 | 固定转速 |
| 波动印数量 → 焰珠数量 | **缺失档：跨技能资源系统（波动印记数）**——无任何资源/印记系统 | 固定 3 珠视觉 + 单判定区 |
| 施放期间不被抓取（免抓） | 延后档（霸体/无敌帧族，且 demo 无抓取怪） | 跳过 |
| 焰珠"绕阵旋转"的环形轨迹判定 | 现有 Area=固定 AABB（无环形/旋转判定，LSShapeData 记档） | 圆形区 Tick 多段近似（视觉上珠旋转用 overlay 动画表现，判定为整个圆域） |
| THROW 蓄力态（施放者定身演出） | 已可表达（技能时长内不位移即可；无专门施法姿态动画——§2.4 实测只有终结姿态） | 角色播 Idle/通用施法姿态 + 特效层 |
| 读条 500ms / MP / 无色消耗 | 延后档 | 跳过 |
| 音效/屏震 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static[1]=900 / [2]=7 / [3]=170 / [5]=300 语义（[2]=7 疑焰珠基础数量或旋转圈数，无脚本可证）。
2. 代理写包两版 shootTime 不一致（8 参版字面量 3400 vs onProc 版 static[0]=4000）的原因——推断 8 参版为旧版残留。
3. 施法姿态动画归属（THROW 态 animIdx=1 对应哪个 .ani，引擎内部表未考证）。
4. [pre required skill] 2 1 与 explain"波动印门槛"的关系（lst 2=ReflectGuard，归属未细究）。

**纠偏**：预分类 B → **实为 A**（主体=焰珠多段+爆炸攻击）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **环形/旋转轨迹判定**（LSShapeData）：焰珠绕圈是"旋转判定体"首个明确用例——现以圆形 AABB Tick 近似；若后续职业技能（旋转类）多，建议 ShapeData 立项时补"环形轨迹"形态。
2. 波动印资源系统（印记计数 → 技能分支）：阿修罗系（波动印家族）通用依赖，首个实证用例；建议并入"跨技能数值查询门面"（R3-A11）合并评估。

**翻译工具缺口**：无新增（.skl/.atk/.obj 既有）。
