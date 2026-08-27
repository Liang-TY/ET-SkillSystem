# 魔煞血陨（swordman_bloodmarble）

> 技能ID 230 | 级别 A | 可实现性 🔶（吸取-投掷-爆炸主体可直译；按住蓄力/再按加速/球体抛物弹道需简化） | 分析日期 2026-08-22 | 批次 A16

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 魔煞血陨 | `skill\Swordman\swordman_bloodmarble.skl [name]` |
| 英文名 | swordman_bloodmarble（取 skl 文件名） | 同上 |
| 职业 | 狂战士（帝血弑天）二觉 80 级技 | [second growtype maximum level] 索引 7=30（系列枚举推断，见 239 §1）+ 血气系设定 |
| 学习等级 | 80 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30，索引 7） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | active（skill class 3） | 同上 [type] |
| 指令 | ↓→→ + Z（MP 优惠 50%/50%） | 同上 [command] / [skill command advantage] |
| CD | 45000 ms | 同上 [cool time] |
| MP | 800 → 6000 | 同上 [consume MP] |
| 特殊消耗 | 道具 3037 × 5 | 同上 [consume item] |
| 可施放状态 | 8/0/14（攻击中可取消） | 同上 [executable states] |
| 一句话效果 | 施法后头顶凝聚魂血球，按住技能键从周围敌人身上吸取血气（球体变大、敌人被定身）；松开/超时后血球向前方砸落（落地小爆→血气大爆炸，爆炸威力按蓄力大小分三档）；下落中再按技能键加速坠落 | 同上 [explain] + nut/PO 走读 |

**level info（5 列）与 static data（写包对位实证）**：

| 项 | 源 | 系数 | Lv1 值 | 语义 |
|---|---|---|---|---|
| 吸取血气时间上限 `<float1>`秒 | static[0]（向量 (0,0,0.001)） | 0.001 | 1500 → **1.5s** | 蓄力上限（超时+300ms 强制投掷） |
| 大小比例上限 `<int>`%% | static[1]（向量 (1,1,1.0)） | 1.0 | 200 → **200%** | 球体 100%→200% 线性长大 |
| 血气吸收攻击力 `<int>` | level col0（-2 源） | 1.0 | 1097 | 蓄力期吸取脉冲伤害 |
| 血凝珠多段攻击力 `<int>` | level col1（-2 源） | 1.0 | 8536 | 投掷命中伤害 |
| 第 1 阶段爆炸 `<int>` | level col2 | 1.0 | 14640 | 球 100-110% 时 |
| 第 2 阶段爆炸 `<int>` | level col3 | 1.0 | 32539 | 中档蓄力 |
| 第 3 阶段爆炸 `<int>` | level col4 | 1.0 | 42296 | 满蓄（200%） |

Lv70 值：1097→8780 / 8536→68301 / 14640→117127 / 32539→260290 / 42296→338376。
**-2 源再次实证**（L21：-2=level 列引用，与 022/066 同）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
24: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/bloodmarble/bloodmarble.nut", "swordman_bloodmarble", 230, 230);
 8-13: pushPassiveObj(share_po_swordman_24370.nut, 24370) + 六回调
```

- 角色施法动画：`sq_SetCurrentAnimation(128)` → .chr etc motion **#128 = `Animation/BloodMarbleExtractReady_Body.ani`**（1101 行实测）。
- 判定主体 = 共享 PO **24370 case 230**（F7/L20）：etc motion **#2** `BloodMarble/bigblood_bigblood_dodge.ani`（魂血球）、**#3** `MiniExplosionNormal.ani`（落地小爆）、**#4** `BloodMarbleExplosion_finish_front.ani`（大爆炸）、**#67** `BloodMarbleExtract_Normal.ani`（吸取脉冲）；atk **#3** `BloodMarbleChunkBase.atk`（蓄力期球体）、**#4** `BloodmarbleThrowAttack.atk`（投掷）、**#5** `BloodMarbleExplosion.atk`（大爆）、**#44** `BloodMarbleExtract.atk`（吸取）——0 基直读。
- 另有 `ap_bloodmarble.nut`（207 行，mod 混淆已还原大意）：挂在被吸取敌人身上的 appendage——drawAppend 画"血凝珠串"（ballhead+balltail1-4 共 12 段，正弦路径从敌人飞向施法者，随机 300-1000ms 一轮）；proc 给施法者动画叠红色 LINEARDODGE 脉冲。纯视觉。

### 2.2 主 nut 逐回调（bloodmarble.nut，71 行，无混淆）

- **checkExecutableSkill**：遍历自己名下 24370 PO——找到 skill 230 / subType 1 / 未加速 / state 11（下落中）→ `SendChangeSkillEffectPacket(word 1)`＝**再按下落中技能键＝加速坠落**。正常路径：`sq_IsEnterSkillLastKeyUnits(230)`（记录按住）→ 进状态 230。
- **onSetState**：`sq_StopMove` + 音效 SM_BLOODMARBLE + 播 #128（12 帧 600ms）+ 攻速静态。
- **onEndCurrentAni**（施法动画播完＝召唤点在动画末尾而非 flag）：
  写包 9 dword：230 / 1 / **col1(8536)** / **static[0](1500)** / col0(1097) / col2 / col3 / col4 / **static[1](200)**
  → `sq_SendCreatePassiveObjectPacket(24370, 0, 0, 0, 300)`——**球出生在头顶 z=300**；回 STAND（角色即自由，后续全由球 PO 驱动）。

### 2.3 共享 PO 24370 case 230（subType 1 状态机 10→11→12→13）

**setcustomdata**：atk#4 power ← col1；var = [吸取上限, 0, 0, col0, col2, col3, col4]；rate = [100, 200, (200-100)/10=10]；bool(0)=false（加速标记）→ state 10。

| state | 进入条件 | 动画/atk | 行为 |
|---|---|---|---|
| **10 蓄力吸取** | 出生 | etc#2 球体（10 帧 600ms 循环）+ atk#3（push50） | `setTimeEvent(0, 600ms, 循环)`＋50ms 私有计时器；procappend：**施法者松开技能键（`!isDownSkillLastKey`）或超时（上限+300ms）→ state 11**；球体图像/图层按 100→200% 线性长大（50ms 拍刷新）；timer0 每 600ms 在球位置创建 subType 2 吸取脉冲 PO |
| **11 投掷下落** | 松键/超时 | etc#2 + atk#4（lift50） | 图像+**攻击盒**按当前大小缩放；timer1=200ms 一次 resetHitObjectList（投掷多段可重复命中）；**`sq_SetMoveParticle("particle/bloodmarblemove.ptl", 0, -50)` + 前向速度 100**＝抛物下坠；procappend：z≤0 → state 12；（再按键 → onChangeSkillEffect：换粒子速度 500＝**加速坠落**） |
| **12 落地小爆** | z≤0 | etc#3（10 帧 660ms，F0 flag1） | miniexplosionround 视觉（按大小缩放）；震屏 10/150；播完→13 |
| **13 血气大爆炸** | 12 播完 | etc#4（15 帧 990ms，**F7 flag1**→震 60/200+黑闪）+ **atk#5**（down/**lift 600**） | **伤害按蓄力大小分档**：damageIndex = 3+uniform(1..3, 大小-100, 200-110) → var[4]/[5]/[6]＝第1/2/3阶段（14640/32539/42296）；攻击盒 F7 起暴涨至 x[-166,170] y[-99,114] z[-41,490]；finish_floor 视觉；播完销毁 |

**subType 2（吸取脉冲 PO，每 600ms 一个）**：etc#67（12 帧 1350ms，F1 flag2、F9/F10 flag1）+ atk#44（0 推 0 浮 knuck-1＝**纯吸取不动敌人**）+ 震 2/100。**onAttack 命中敌人时**：可抓取判定 → 挂 ap_bloodmarble（血珠串视觉）+ **`sq_HoldAndDelayDie`（吸取期定身）**；播完销毁。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/BloodMarbleExtractReady_Body.ani`（施法#128） | 12 | 600ms（均 50ms） | F4=0（无消费） | F4 空 | 召唤点=动画末（onEndCurrentAni） |
| PO etc#2 bigblood_bigblood_dodge.ani（球体） | 10 | 600ms | 无 | 全帧 `-87 -49 -7 171 98 171`（蓄力期判定随缩放） | .als 1 个 |
| PO etc#67 BloodMarbleExtract_Normal.ani（吸取脉冲） | 12 | 1350ms | F1=2、F9/F10=1 | F1-6 渐涨至 x[-282,268]（大范围吸取） | .als 1 个 |
| PO etc#3 MiniExplosionNormal.ani（落地小爆） | 10 | 660ms | F0=1 | 无（判定窗=atk 配套） | miniexplosionround.ani 配套层 |
| PO etc#4 BloodMarbleExplosion_finish_front.ani（大爆炸） | 15 | 990ms | **F7=1**（震+黑闪） | F6-10 涨至 x[-166,170] z[-41,490] | .als 多层（back/middle/floor/addeffect1-10/addblood1-6） |
| beed/ ballhead+balltail1-4.ani | — | — | — | — | ap 血珠串素材（脚本直绘） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_bloodmarble.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_bloodmarble.skl` | ✅ 实测 | 5 列 + static 2 槽（§1 全解码） |
| 注册行 | swordman_load_state.nut 行 24 / 8-13 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 230 + PO 24370 |
| 主 nut | bloodmarble.nut | `…\pvf\sqr\character\swordman\bloodmarble\bloodmarble.nut` | ✅ 实测（71 行，无混淆） | 施法/加速分流/写包 |
| 血珠 ap | ap_bloodmarble.nut | `…\pvf\sqr\character\swordman\bloodmarble\ap_bloodmarble.nut` | ✅ 实测（207 行，混淆已还原大意） | 吸取定身 + 血珠串/红染视觉 |
| PO 回调 | share_obj/swordman/ case 230 | `…\pvf\sqr\common_object\share_obj\swordman\{setcustomdata:84,setstate:52,procappend:6,onendcurrentani:17,else:13/185/538/828}.nut` | ✅ 实测 | 蓄力/投掷/落地/爆炸全状态机 |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ 实测 | etc #2/#3/#4/#67、atk #3/#4/#5/#44 对位 |
| PO atk ×5 | BloodMarbleChunkBase / BloodmarbleThrowAttack / BloodMarbleExplosion / BloodMarbleExtract / bloodmarblemoveexplosion（未见解引，疑备用） | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ 实测 | 蓄力/投掷/大爆/吸取命中反应 |
| .chr 条目 | etc motion #128 | `…\pvf\character\swordman\swordman.chr` 1101 行 | ✅ 实测 | `Animation/BloodMarbleExtractReady_Body.ani` |
| 角色 .ani | BloodMarbleExtractReady_Body.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 12 帧 600ms |
| 角色 .atk | —（判定全在 PO） | `…\pvf\character\swordman\attackinfo\` | — | — |
| PO .ani | bloodmarble/ 55 个（bigblood/bloodmarbleextract/throwattack/miniexplosion/bloodmarbleexplosion_finish 系 + beed/ 6 个） | `…\script_sqr_nut_qq506807329\swordman\animation\bloodmarble\` | ✅ 实测 | 全部演出视觉（.als 8 个） |
| 特效 .ani | —（本技角色侧 effect 目录无独立 blodmarble 目录，视觉全在 PO 侧） | `…\pvf\character\swordman\effect\animation\` | ✅ 实测（无目录） | — |
| 装备层 | 未查 | `…\pvf\equipment\...` | 未查 | sm_body 单图集（L16） |

## 4. 资源需求

img 集中在 `character/swordman/effect/BloodMarble/`（大小写两种路径写法并存，NPK 推导按小写主路径：sprite_character_swordman_effect_bloodmarble.NPK）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `…/BloodMarble/05_bloodagg_D.img`（+ `_N`） | sprite_character_swordman_effect_bloodmarble.NPK | **魂血球本体** | **必需** | ❌ |
| `…/BloodMarble/08boom_front.img`（+ `08boom_floor.img`、`08_middle.img`） | 同上 | **血气大爆炸** | **必需**（front；floor/middle 可选） | ❌ |
| `…/BloodMarble/01floor_N.img`（+ `01floor_D.img`） | 同上 | 球底光圈/落地小爆 | 建议 | ❌ |
| `…/BloodMarble/07_throwblood_N.img`（+ `07_throwblood_floor.img`、`07blooddraw_D.img`） | 同上 | 投掷血线 | 可选 | ❌ |
| `…/BloodMarble/05_bloodagg_N.img`、`06_bloodSPIT_D/N.img`、`addboom.img`、`addboom2.img`、`hit_floor.img` | 同上 | 吸取/加层 | 可选 | ❌ |
| `Character/Swordman/Effect/BloodBoom/bloodboom_finish2.img` | sprite_character_swordman_effect_bloodboom.NPK | 借图（bloodboom 系在库 4 张但**此张不在**） | 可选 | ❌ |
| `Character/Fighter/Effect/EnergyField/boom.img`、`ATTimeSlash/timeslash_circle.img`、`Hellbenter/energy.img`、`HundredSword/sword_light_tail.img`、`MeteorSword/meteorsword_circle.img` | 跨职业/跨技 5 个 NPK | .als 借图（L14 常态） | 可选 | ❌ |
| sm_body0000.img | （已入库） | 角色施法动画 | 必需（共享） | ✅ |

缺失 img：必需 2 张（05_bloodagg_D、08boom_front）+建议 3 张+可选 15 张（含跨 NPK 借图 6 张）。

## 5. 实现方案草案

**结构映射**：施法（600ms）→ 头顶球（蓄力固定满档 1.5s + 吸取区 Tick 600ms×2）→ 投掷（简化为前方固定点延迟坠落）→ 落地小爆区 → 大爆区（第三档伤害）。

### 内容件清单

1. **`DotNet~/Skills/BloodMarbleSkill.cs : SkillLogic`**（SubState 时间轴；**蓄力瞬发满档**——R3-A15 惯例）
   - `CooldownMs = 45000`；`TotalTimeMs = 4200`（600 施法 + 1500 满蓄吸取 + 500 飞行 + 660 小爆 + 990 大爆，紧排）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanBloodMarbleReady)` + `ctx.ClearHitTargets()`。
   - `OnUpdate`（SubState 推进，600ms 后 `PlayDefaultAnim` 解控、技能空转计时）：
     - t≥600：`ctx.CreateArea(AreaIds.BloodMarbleDrain, 施法者位)`（吸取区）；SubState=1。
     - t≥2100（满蓄完）：记录落点＝前方 2.5 单位（DNF 球前抛约 2-3 单位落点，**位置随机/精确落点未考证**）；SubState=2。
     - t≥2600（飞行完）：`ctx.CreateArea(AreaIds.BloodMarbleHit, 落点)`（落地小爆）；SubState=3。
     - t≥3260：`ctx.CreateArea(AreaIds.BloodMarbleBoom, 落点)`（大爆炸，**固定第三档**）；SubState=4。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/BloodMarbleDrainArea.cs : AreaDefinition`**（FireCircleArea Tick 范式）
   - `TotalTimeMs = 1500`、`TickTimeMs = 600`（2 拍）、`TickActions = { MeleeHit }`；
   - `HalfExtents = (2.8, 0.9, 1.0)`（etc#67 吸取盒 F4 极值 x[-282,268] 折算）；
   - `HitReaction { Damage = 30, HitstunMs = 400, KnockbackX = 0, LaunchY = 0 }`（atk#44 原值 0/0/knuck-1＝吸取不推动；定身见 §7）；
   - `ViewAnimId = AnimId.BloodMarbleBall`（球体循环——球视觉挂吸取区中心，出生即在场）。
3. **`DotNet~/Areas/BloodMarbleHitArea.cs : AreaDefinition`**（落地小爆＋投掷命中合一）
   - `TotalTimeMs = 660`、`EnterActions = { MeleeHit }`、`HalfExtents = (1.5, 0.6, 1.0)`；
   - `HitReaction { Damage = 120, HitstunMs = 500, KnockbackX = 0, LaunchY = 50 }`（atk#4 lift50＋小爆 down 折中）；
   - `ViewAnimId = AnimId.BloodMarbleMiniExplosion`。
4. **`DotNet~/Areas/BloodMarbleBoomArea.cs : AreaDefinition`**（血气大爆炸）
   - `TotalTimeMs = 500`（etc#4 F7 判定起爆窗）、`EnterActions = { MeleeHit }`、`HalfExtents = (1.7, 1.0, 2.5)`（F8 盒 x[-166,170] z 高 490 折算）；
   - `HitReaction { Damage = 400, HitstunMs = 1000, KnockbackX = 0, LaunchY = 600 }`（atk#5 原值 down+lift600——大浮空击倒）；
   - `ViewAnimId = AnimId.BloodMarbleBoom`（990ms 视觉）。
5. **无新增 Buff/Action**。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 施法动画末召唤（onEndCurrentAni 写包） | OnUpdate t=600 触发（动画 600ms 播完点对齐） |
| 按住蓄力（球 100→200%，三档爆炸） | 按住输入缺失 → **固定满��� 1.5s + 第三档**（R3-A15 共性） |
| timer0 600ms 吸取脉冲 PO | 吸取区 Tick 600ms（Tick 无去重同构） |
| 吸取定身（sq_HoldAndDelayDie） | 定身微控缺失（021 §7 同族）→ 用短 Hitstun 近似 |
| 球抛物下坠（move particle + 再按加速×5） | 弹道/加速交互缺失 → 固定 500ms 延迟 + 前方 2.5 单位落点 |
| 落地小爆→大爆炸（etc#3→#4） | 两个 Area 顺序创建（L9） |
| 三档爆炸伤害 | 固定第三档 |
| 血珠串/红染 ap 视觉 | 脚本直绘无翻译源 → 球体+爆炸主视觉承担（064 §8-2 同族） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.BloodMarble = 25` |
| AreaId | `Runtime\AreaDefinition.cs` | `BloodMarbleDrain = 21`、`BloodMarbleHit = 22`、`BloodMarbleBoom = 23` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanBloodMarbleReady = 114`、`BloodMarbleBall = 115`、`BloodMarbleMiniExplosion = 116`、`BloodMarbleBoom = 117` |
| json 注册 | `LSAnimClipRegistrar.cs` | 角色 1 + PO 3（含 .als overlay 翻译） |
| 图集 | `LSAnimResComponentSystem.cs` | 05_bloodagg_D、08boom_front（必需两张） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000 ms | 45000（直用） |
| 施法 | 600ms（动画末召唤） | t=600 触发 |
| 蓄力 | 按住 0~1.5s（+300ms 宽限），球 100→200% | 固定 1.5s 满档 |
| 吸取 | 600ms/脉冲 × 2-3，1097%；定身 | Tick 600ms，Damage 30/拍，Hitstun 400 |
| 投掷 | 球飞行 + 命中 8536%（lift50），200ms 后可重命中 | 并入落地小爆区 Damage 120 |
| 落地小爆 | 660ms；down/push100/lift300（配套 atk#2 疑用） | Damage 120 / Ly 50 |
| 大爆炸 | 第 1/2/3 档 14640/32539/42296%；down/lift600；盒 x±166 | 第三档 Damage 400 / Ly 600 |
| 全程 | 600+≤1800+飞行+660+990 ≈ 4-5s | 4200ms |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_bloodmarble.skl | `.skl` 无子命令 | 本档已全解码（5 列+static 2 槽） |
| 5 份 .atk（#3/#4/#5/#44/+备用 #2） | `.atk` 无子命令 | 手抄可接受；`[knuck back] -1`（负＝禁推）语义记入 atk 立项输入 |
| 24370 obj | `.obj` 无子命令 | #2/#3/#4/#67、#3/#4/#5/#44 对位表已给 |
| bloodmarble 各 .ani/.als（8 个 .als） | `[use animation]`/`[add]` 常规 | ani/als 子命令全覆盖 |
| `particle/bloodmarblemove.ptl` | **.ptl 无子命令 + 移动粒子是行为不是视觉** | 弹道由 Area 编排替代（§5）；ptl 记 L5 通案 |
| ap_bloodmarble.nut 血珠串/红染 | 脚本直绘（无翻译源） | 跳过（064 §8-2 引擎特效无声明式来源族） |
| beed/ballhead 等 6 个 .ani | 常规节 | 可译但暂无消费（直绘专用），不提取 |

结论：`.skl`/`.atk`/`.obj` 族共性 3 条 + .ptl（L5 通案）；ani/als 无新节缺口。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 按住技能键蓄力（球体大小＝伤害档位） | 按住输入缺失（R3-A15 四技共性） | 固定满档 1.5s + 第三档 |
| 下落中再按加速 ×5 | 技能二段交互门面缺失（239 §8）+ 按住缺失 | 不做（固定时序） |
| 球体抛物弹道（move particle，z 物理） | Bullet 无 z 弹道/移动粒子（施法者 z 主动位移姊妹项） | 固定延迟+固定落点（前方 2.5 单位） |
| 吸取定身（HoldAndDelayDie） | 定身/抓取微控缺失（021 §7 同族） | 短 Hitstun 400 近似 |
| 投掷多段 200ms 重置 | 同段定时多段 Area Tick 可表达（L19） | 并入小爆区单次（伤害直加折算） |
| 球体/爆炸图像+攻击盒三重缩放 | 对象整体缩放延后 | 固定 100%（第三档伤害直给） |
| 血珠串（敌人→施法者飞行珠）+ 角色红染 | 位置随机+直绘无源 | 跳过（球体主视觉已足够） |
| 震屏（60/200 大爆）/黑闪 | 屏震/闪屏延后 | 跳过 |
| 攻击中取消施放 | 技能取消体系缺失 | 站立施法 |
| 音效 SM_BLOODMARBLE | 音频延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①球体投掷的水平落点（move particle 前向速度 100 的飞行距离——demo 取前方 2.5 单位是估计值）；②atk#2 `bloodmarblemoveexplosion.atk` 的消费者（obj/回调均未见引用，疑备用或引擎侧）；③施法动画 F4 flag 0 的语义（flag 0 无消费者惯例）；④pvp level info 未全读；⑤落地小爆的判定参数（etc#3 无攻击盒，疑用 state 12 切换时的当前 atk#4 残留或引擎默认——demo 并入小爆区自定参数）。
- **缺口累计引用**：按住蓄力（R3-A15 共性，本批 230 是"蓄力档位型"样本）；技能二段交互（239 §8，本技=加速降落变体）；Bullet z 弹道（R3-A15 流星落姊妹）。
- **给下轮的经验**：230 是 24370 族里"onEndCurrentAni 召唤"型（施法动画无 SET FLAG，召唤点=动画播完）——读不到 flag 时先查 onEndCurrentAni；ChangeSkillEffect(word) 是 PO 侧"技能外按键交互"的第二种载体（与 addSetStatePacket 并列），`else.nut:828` 是其总入口。
