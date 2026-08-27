# 幻鬼：大回天（windspiritvs）

> 技能ID 59 | 级别 A | 可实现性 🔶（三段突进斩主体可直译；幻鬼实体接力/剑术中无动作施放需简化） | 分析日期 2026-08-22 | 批次 A16

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻鬼 : 大回天 | `skill\Swordman\ghostsword\windspiritvs.skl [name]` |
| 英文名 | windspiritvs（取 skl 文件名） | 同上 |
| 职业 | 剑影（夜见主宰）二觉 75 级技——幻鬼 VS 族 | [second growtype maximum level] 索引 11=30（系列枚举推断，见 239 §1）+ ghostsword 目录 + VS 族链路 |
| 学习等级 | 75 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30，索引 11） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | active（skill class 1，physical） | 同上 [type] / [weapon effect type] |
| 指令 | ↓↑→ + Z（MP 优惠 20%/40%） | 同上 [command] / [skill command advantage] |
| CD | 40000 ms（pvp 20000） | 同上 [cool time] |
| MP | 568 → 4402 | 同上 [consume MP] |
| 特殊消耗 | 道具 3037 × 3 | 同上 [consume item] |
| 可施放状态 | 0/8/14/126/127/136/71/45/170/20/22（站立系 + 六个剑术技能态） | 同上 [executable states] + nut |
| 一句话效果 | 施法后幻鬼在身前现身，向前方旋转突进三段连斩（每段多段物理伤害并把敌人聚在 X 轴上）；在剑术技能施放中使用时无施法动作直接从幻鬼位置出招 | 同上 [explain] + nut/PO 走读 |

**level info（1 列）与 static data（本 pvf 最干净的 L21 实证样本）**：

| 项 | 源 | Lv1 值 | 语义（setstate 回调直读实证） |
|---|---|---|---|
| 旋转斩击攻击力 `<int>`%% | level col0（向量 (-1,0,1.0)） | **12620%** | `parentChr.sq_GetBonusRateWithPassive(59, -1, 0, 1.0)` → 三段共用 |
| 斩击范围 `<int>`%% | static[0]（向量 (0,0,1.0)） | 100 | 攻击盒缩放 `sq_GetIntData(parentChr, 59, 0)/100` |
| 突进距离 `<int>`px | static[1]（向量 (1,1,1.0)） | **140px** | 两段突进 `sq_MoveToNearMovablePos` 距离 |
| 反方向生成幻鬼/吸附 | static[2]（向量 (2,2,1.0)） | 0（关） | >0 时：PO 生成于**施法者前方 400px 且背向**，第三段命中把敌人拉向施法者（200ms ap） |

Lv70 col0 = 100962%。pvp static 多一位（`100 100 140 0`，首位疑 pvp 攻击盒修正，未考证）。
**速度联动**：三段动画与突进均乘 `SpeedRate`——技能 123（BLADESPIRIT，剑影被动）col0/col1 的攻速加成（1+x%）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
167: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/windspiritvs/windspiritvs.nut", "windspiritvs", STATE_WINDSPIRITVS, SKILL_WINDSPIRITVS);
      STATE_WINDSPIRITVS = 125（header:55）、SKILL_WINDSPIRITVS = 59（header:162）
（PO 24349 共享壳注册见 F5：po_swordman_shared.nut 按写包首 dword 分派到 sqr\shared_passive_object\swordman\ 六回调）
```

- 施法动画：`CUSTOM_ANI_ONESLASHVS = 298`（header:469）→ .chr etc motion **#298 = `Animation/oneslashvs.ani`**（1271 行实测）——**复用 135 一闪的施法动画**（8 帧 420ms，F4 flag 10001 属一闪逻辑，本技无消费者）。
- 攻击体 = 共享 PO **24349 分派 case 66**（F5 链路）：动画 `getCustomAnimation(79/80/81)` → swordman_shared.obj etc motion **#79/#80/#81 = windspiritvs{abody_body, bbody_body, c_body}.ani**（0 基直读正常）。
- **atk 配对（F5 -2 错位第四实证）**：回调代码 `sq_GetCustomAttackInfo(obj, 39)` 三次（三段同 atk 索引）。表值 #39 = `spinningslashvs3.atk`；按 F5 规则（VS 族 63-74 一律 -2）**实载 #37 = `spinningslashvs1.atk`**；语义预期（文件名对应）为 #41 `windspiritvs.atk`。三说并列记档，demo 数值取 windspiritvs.atk 设计值、参数量级三者同档（见 §5）。

### 2.2 主 nut 逐回调（windspiritvs.nut，116 行，无混淆）

- **checkExecutableSkill**（三分支）：
  1. 状态 0/8/14（站立系）→ 进状态 125（正常施法）；
  2. 状态 111(SPIRITMOVE)/SPEEDSLASH/GHOSTPIERCE/WHITEGHOSTSLASH/GHOSTDECOLLATION/SWORDDANCEBS（**六个剑术技能态**）→ **不进状态机直接创建 PO**（＝"无施放动作立即出现幻鬼"）：有在场幻鬼（`getVSObject`，F5：24349 在场判定）→ 在幻鬼位置创建并**销毁幻鬼**（接力+消耗）；无幻鬼 → 身前 70px 创建。写包仅 1 dword（分派号 66）；
  3. 其余状态不可用。
- **onSetState**（subState 0）：播 #298 oneslashvs.ani（速度乘 BLADESPIRIT 加成）；随后与分支 2 相同的 PO 创建逻辑（**施法动画与幻鬼攻击并行**——角色摆一闪姿势，幻鬼在前方开打）。
- **onEndCurrentAni**：回 STAND。

### 2.3 共享 PO 24349 case 66（幻鬼三段状态机 10→11→12→destroy）

参数全部从 parentChr 直读（不经写包）：col0 伤害 / static[0] 盒缩放 / static[1] 突进 / static[2] 反向开关。

| state | 动画（etc#） | 时长 | 命中 | 关键行为 |
|---|---|---|---|---|
| 10 A 段 | #79 windspiritvsabody_body（12 帧 649ms） | 649ms | atk 索引 39（F5 换算，§2.1），攻击盒 F8-10：`-175 -84 -1 455 168 242`（x[-175,280]） | F2 flag10001→aeffect_11 视觉；**F10 flag10004→向前突进 140px**（`sq_MoveToNearMovablePos`）；**static[2]>0 时本段出生点改为施法者前方 400px 且方向取反**；播完→11 |
| 11 B 段 | #80 windspiritvsbbody_body（8 帧 370ms） | 370ms | 同 atk，攻击盒 F0-2：x[-208,292] | F0→beffect_00 视觉；**F6 flag10003→再突进 140px**；播完→12 |
| 12 C 段 | #81 windspiritvsc_body（29 帧 2990ms） | 2990ms | 同 atk，攻击盒 F0-2：x[-206,344]；F22 后无盒 | F0→ceffect_00；F22→disappearback（幻鬼消散）；**命中敌人时 static[2]>0 → 挂 ap_windspiritvs（200ms）把敌人 `sq_MoveToNearMovablePos` 拉向施法者**（"向剑影吸附"）；播完销毁 |

onattack 另统一调 `Vs_Attack_Effect`（VS 族共用命中特效）。三段攻击信息逐段重设——段间命中表重置（每段独立可命中，L19 段间多段已通）。

**ap_windspiritvs.nut**（51 行）：onStart 执行一次"把 parent（敌人）拉到 source（施法者）前方 100px 可移动位"——吸附的实现体；200ms 自动失效。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/oneslashvs.ani`（施法复用#298） | 8 | 420ms | F4=10001（一闪遗留，无本技消费者） | 无 | .als 见 135 文档 |
| PO #79 windspiritvsabody_body.ani | 12 | 649ms | F2=10001、F8=10002、F9=10003、**F10=10004（突进）** | F8-10 | .als：none effect add ×3 + **create draw only object ×2**（烟/现身） |
| PO #80 windspiritvsbbody_body.ani | 8 | 370ms | F0=10001、F5=10002、**F6=10003（突进）** | F0-2 | .als 同构 |
| PO #81 windspiritvsc_body.ani | 29 | 2990ms（均 120ms） | F0=10001、F22=10002（消散） | F0-2 | 长收势段 |
| PO 特效 aeffect_00-11 / beffect_00-06 / ceffect_00-04 / movesmoke / vsappear_00-01 / disappearback | — | — | — | — | 三段配套视觉（.als 6 个） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | windspiritvs.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\windspiritvs.skl` | ✅ 实测 | 1 列 + static 3 槽（§1 全解码） |
| 注册行 | swordman_load_state.nut 行 167 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 125 / 技能 59 |
| 主 nut | windspiritvs.nut | `…\pvf\sqr\character\swordman\5_ghostsword\windspiritvs\windspiritvs.nut` | ✅ 实测（116 行） | 施法分流/幻鬼接力/PO 创建 |
| 吸附 ap | ap_windspiritvs.nut | `…\5_ghostsword\windspiritvs\ap_windspiritvs.nut` | ✅ 实测（51 行） | 敌人拉向施法者（200ms） |
| PO 回调 | shared_passive_object/swordman/ case 66 | `…\pvf\sqr\shared_passive_object\swordman\{setcustomdata:934,setstate:569,onendcurrentani:508,onkeyframeflag:726,onattack:145}.nut` | ✅ 实测 | 三段状态机/突进/吸附 |
| PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\` | ✅ 实测 | etc #79-81 / atk 表（F5 -2 换算） |
| PO atk | windspiritvs.atk（#41 语义）/ spinningslashvs1.atk（#37 按规则实载） | `…\unclebang_shared_passive_object\swordman\attackinfo\` | ✅ 实测 | 三段命中参数 |
| .chr 条目 | etc motion #298 | `…\pvf\character\swordman\swordman.chr` 1271 行 | ✅ 实测 | `Animation/oneslashvs.ani` |
| 角色 .ani | oneslashvs.ani（复用） | `…\pvf\character\swordman\animation\` | ✅ 实测 | 8 帧 420ms |
| 角色 .atk | —（判定全在 PO） | `…\pvf\character\swordman\attackinfo\` | — | — |
| PO .ani | windspiritvs/ 55 个（abody/bbody/c 系 + aeffect/beffect/ceffect + vsappear/movesmoke/disappear） | `…\unclebang_shared_passive_object\swordman\animation\windspiritvs\` | ✅ 实测 | 三段全套视觉（.als 6 个） |
| 特效 .ani | —（视觉全在 PO 侧 unclebang 目录） | `…\pvf\character\swordman\effect\animation\windspiritvs\` | ✅ 实测（无目录） | — |
| 装备层 | 未查 | `…\pvf\equipment\...` | 未查 | sm_body 单图集（L16） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/WindSpiritVS/00001.img`、`00002.img`、`00003.img`、`ddd0000.img` | sprite_character_swordman_effect_windspiritvs.NPK | 三段斩击主视觉 | **必需**（至少 00001） | ❌ |
| `Character/Swordman/Effect/BladeSpiritDot/VengeanceSpirit.img`（+`_Dodge`） | sprite_character_swordman_effect_bladespiritdot.NPK | **幻鬼实体贴图**（VS 族共用） | **必需**（幻鬼本体） | ❌ |
| `Character/Swordman/Effect/SpinningSlashVS/slash1.img`、`slash2_normal1.img`、`slash2_dodge.img`、`redeye1.img` | sprite_character_swordman_effect_spinningslashvs.NPK | 旋转斩风特效（借自 26 旋转斩） | 可选 | ❌ |
| `Character/Swordman/Effect/TeleportVS/Normal.img`、`LDodge.img` | sprite_character_swordman_effect_teleportvs.NPK | 幻鬼现身/消散 | 可选 | ❌ |
| `Character/Mage/Effect/BroomSpin/G03.img` | sprite_character_mage_effect_broomspin.NPK | 跨职业借图（L14 常态） | 可选 | ❌ |
| sm_body0000.img | （已入库） | 角色施法动画 | 必需（共享） | ✅ |

缺失 img：必需 5 张、可选 8 张，跨 5 个 NPK。

## 5. 实现方案草案

**结构映射**：施法（420ms 一闪姿势）→ 身前 0.7 单位幻鬼三段 Area（A 649ms 突进 → B 370ms 突进 → C 收势），每段独立命中。

### 内容件清单

1. **`DotNet~/Skills/WindSpiritVsSkill.cs : SkillLogic`**
   - `CooldownMs = 40000`；`TotalTimeMs = 4500`（420 施法 + 649 + 370 + 2990 三段——技能托管：420ms 后 `PlayDefaultAnim` 解控，空转驱动段序）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanOneslashVs)`（复用 135 一闪已注册动画）+ `ctx.ClearHitTargets()`。
   - `OnUpdate`（SubState 推进，t 相对施法）：
     - t=0：`ctx.CreateAreaInFront(AreaIds.WindSpiritVsA, 0.7)`（DNF 70px）；SubState=1。
     - t≥1069（A 完）：`ctx.CreateAreaInFront(AreaIds.WindSpiritVsB, 1.4)`（A 末已突进 140px≈1.4 单位——段中心前移）；SubState=2。
     - t≥1439：`ctx.CreateAreaInFront(AreaIds.WindSpiritVsC, 2.8)`（再突进后）；SubState=3。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - **剑术中无动作施放/幻鬼接力**：不做（§7）。
2. **`DotNet~/Areas/WindSpiritVsArea.cs : AreaDefinition`**（一个类三实例或三个配置——FireCircleArea 派生范式）
   - A：`TotalTimeMs = 350`（F8-10 判定窗 3 帧近似）、`EnterActions = { MeleeHit }`、`HalfExtents = (2.3, 0.8, 1.2)`（盒 x[-175,280] 半宽折算，中心前偏 0.5）、`HitReaction { Damage = 130, HitstunMs = 400, KnockbackX = 100, LaunchY = 80 }`（spinningslashvs1.atk 原值 push100/lift80/hit horizen）、`ViewAnimId = AnimId.WindSpiritVsA`。
   - B：`TotalTimeMs = 160`、`HalfExtents = (2.5, 0.8, 1.2)`、同 HitReaction、`ViewAnimId = AnimId.WindSpiritVsB`。
   - C：`TotalTimeMs = 360`（F0-2 判定窗）、`HalfExtents = (2.8, 0.8, 1.2)`、`HitReaction { Damage = 130, HitstunMs = 500, KnockbackX = 200, LaunchY = 150 }`（windspiritvs.atk 设计值 push200/lift150/hit horizon——终段横扫）、`ViewAnimId = AnimId.WindSpiritVsC`（2990ms 长视觉播完自隐）。
   - 三段各自独立 Area＝段间命中重置天然成立（L19）。
3. **无新增 Buff/Action**（吸附开关 static[2]=0，不做）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 125 + oneslashvs 施法动画 | `WindSpiritVsSkill` + 复用 AnimId（135 已注册） |
| PO 24349 case 66 三段状态机 | 三个 Area 顺序创建（L9 多相位惯例） |
| 段内 flag 突进（MoveToNearMovablePos 140px×2） | 段中心逐段前移（Area 出生位硬编码 0.7→1.4→2.8）——**位移他人门面不缺**（是 PO 自己动，非推人），用创建位表达 |
| 三段同 atk 逐段重设（段间重命中） | 三 Area 各自 EnterActions（天然重命中） |
| BLADESPIRIT 攻速加成（动画/突进提速） | 被动查询缺失 → 固定 1.0 |
| 剑术技能态无动作施放 + 幻鬼位置接力（消耗幻鬼） | 技能取消体系 + 幻鬼实体记忆双缺（F5/R2-A6 已记）→ 只做站立施法、固定身前 0.7 单位 |
| static[2] 反向生成+吸附（默认关） | 数据为 0，直接不做（若开：反向出生位 + 位移他人门面缺口 R2-A8） |
| Vs_Attack_Effect 共用命中特效 | 无命中特效通道（帧级）→ 跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.WindSpiritVs = 26` |
| AreaId | `Runtime\AreaDefinition.cs` | `WindSpiritVsA = 24`、`WindSpiritVsB = 25`、`WindSpiritVsC = 26` |
| AnimId | `AnimConfigRegistry.cs` | `WindSpiritVsA = 118`、`WindSpiritVsB = 119`、`WindSpiritVsC = 120`（幻鬼三段，含 .als overlay） |
| json 注册 | `LSAnimClipRegistrar.cs` | PO 3 个（.als 翻译随行） |
| 图集 | `LSAnimResComponentSystem.cs` | WindSpiritVS 00001 + VengeanceSpirit（必需） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000 ms | 40000（直用） |
| 施法 | oneslashvs 420ms（并行） | 420ms |
| 三段时长 | 649/370/2990 ms（×BLADESPIRIT 速度） | Area 350/160/360 判定 + C 视觉 2990 |
| 三段伤害 | 各 12620%（col0 ×3 段） | 130 × 3 |
| 命中反应 | 实载 push100/lift80（#37）；设计 push200/lift150/horizon（#41） | A/B 100/80、C 200/150 |
| 突进 | 140px × 2 段（F10/F6 flag） | 段中心 0.7/1.4/2.8 单位 |
| 攻击盒 | x[-175,280] / [-208,292] / [-206,344] ×100% 缩放 | HalfExtents 2.3/2.5/2.8 × (0.8, 1.2) |
| 吸附 | static[2]=0 关（开启时 200ms 拉向施法者） | 不做 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| windspiritvs.skl | `.skl` 无子命令 | 本档 1 列+static 3 槽已全解码 |
| 2 份候选 .atk | `.atk` 无子命令 | 手抄；`[hit horizen]`（spinningslashvs1 拼写变体）与 `[hit horizon]`（windspiritvs）需 atk 立项时归一 |
| swordman_shared.obj | `.obj` 无子命令 | #79-81/atk 表已给（F5 -2 换算） |
| windspiritvs{abody,bbody,c}_body.ani.als | `[none effect add]`（已支持，L12） | 无缺口 |
| 同上 abody .als | **`[create draw only object]`**——参数形如 `帧号 / 别名 / 0 1 0`（无 follow parent 后缀变体，R1-A4 已记后缀变体，本例是无后缀变体） | AlsParser 按 [add] 同构扩展（R1-A4 建议重申：需按帧号创建一次性子动画） |
| 各 .ani | 常规节 | ani 子命令全覆盖 |

结论：`.skl`/`.atk`/`.obj` 族共性 3 条 + `[create draw only object]` 无后缀变体 1 条（R1-A4 缺口的补充样本）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 剑术技能施放中无动作出招（六状态直连 PO） | 技能取消体系缺失（064/R2-A9 族） | 只做站立施法 |
| 从在场幻鬼位置出招并消耗幻鬼（getVSObject） | 幻鬼实体记忆/传送缺失（R2-A6 F5 主缺口） | 固定身前 0.7 单位 |
| 段内突进（PO 自身位移 140px×2） | 无"区域中途位移"——用段中心前移表达（等价判定，视觉略跳） | 段位硬编码 §5 |
| BLADESPIRIT（123）攻速联动 | 跨技能 level/static 查询门面缺失（R3-A11）+ 被动系统缺失 | 固定 1.0 |
| 吸附开关（static[2]>0 时拉敌人） | 位移他人门面缺失（R2-A8）；本 pvf 数据为 0 关 | 不做 |
| 三段攻击盒随 static[0] 缩放 | 对象整体缩放延后 | 固定 100% |
| 幻鬼命中特效 Vs_Attack_Effect | 无命中帧特效通道 | 跳过（Area 主视觉承担） |
| C 段 2990ms 长收势 | 无（视图播完自隐，判定窗只在前 360ms） | 直译 |
| 音效 | 音频延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①atk 实载三说（#39 表值 spinningslashvs3 / F5 规则 #37 spinningslashvs1 / 语义 #41 windspiritvs）——按 F5 规则采信 #37，demo 数值取三者同档无碍；②pvp static 首位 `100` 的语义；③BLADESPIRIT（123）col0/col1 具体百分值（未读 123.skl——属该技能批次）；④A 段 F8 的 flag 10002 / F9 的 10003 无 case 66 消费者（疑残留或引擎层）。
- **缺口累计引用**：幻鬼实体记忆/传送（F5 全族主缺口，本技为"接力+消耗"第二形态样本——066 是"从幻鬼位置接力"、本技多了销毁幻鬼）；技能取消体系（六剑术态直连）；`[create draw only object]` 无后缀变体（R1-A4 补充样本）。
- **给下轮的���验**：**本 pvf 最简 L21 样本**——windspiritvs.skl 的 4 向量与 static data（100/140/0）逐字对上（含 -1 源与 0/1/2 源混用），可作讲解例；24349 的 setstate 回调**直接从 parentChr 读 level/static**（不经写包传参），与 24370 的写包流相反——读 F5 族参数先看 setstate 有没有 parentChr 调用；**case 66 的 atk -2 错位是第四实证**（63/65/66 三连），F5 规则可信度已足够写进总览。
