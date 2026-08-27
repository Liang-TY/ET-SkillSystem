# 血气之刃（BloodSword）

> 技能ID 103 | 级别 A | 可实现性 ✅（直接，基础版：HP 消耗门面/帧驱动攻击盒/Area Tick 多段爆炸全现成；按后方向键原地刺击与霸体降级） | 分析日期 2026-08-22 | 批次 A12

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 血气之刃 | `skill\Swordman\BloodSword.skl [name]` |
| 英文名 | BloodSword（skl 文件名；[name2]="Blood Sword"） | 同上 |
| 职业 | 狂战士（[skill fitness growtype]=3；L17 映射 3=狂战士） | 同上 |
| 学习等级 | 40（前置：技能 23 Sacrifice Lv1） | 同上 [required level] / [pre required skill] |
| 最高等级 | 50（狂战段上限 30） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 2，物理武器效果；血气系） | 同上 |
| 指令 | ←←→ + Z（指令 MP 优惠 20%/40%） | 同上 [command] |
| CD | 20000 ms（auto cooltime apply=1 → 施放即起算）；pvp 起手 20000 | 同上 [cool time] / [auto cooltime apply] |
| MP | **0**（[consume MP] 0 0——本技能不耗 MP） | 同上 [dungeon][consume MP] |
| 特殊消耗 | **HP 消耗量 = level col2：187 → 3763**（Lv1→Lv67，随级成长） | 同上 [level property] 第 3 向量 `-1 2 1.0` |
| static data | `500 -1000 170 60 250 100`（6 值，语义未考证；爆炸范围=static[5]=100% 已证） | 同上 [static data] + level property 第 4 向量 `5 5 1.0` |
| 一句话效果 | 前方喷血气生成血刃，抓取血刃强威力刺击；血刃刺入后爆炸；施放时按后方向键可原地刺击 | 同上 [explain] |

**level property（4 列模板，Lv1 → Lv67）**：刺击攻击力 = col0：`1170 → 9522`；爆炸攻击力 = col1：`5310 → 43215`；HP 消耗量 = col2：`187 → 3763`；爆炸范围 = static[5] = **100%**（向量 `5 5 1.0`，L21 解码法：源 5 → static 槽 5）。
[feature skill index] 168 = BloodSwordEx.skl（TP 强化版，E 类批另行分析）；另有 cancelbloodsword.skl（强制取消，不在 241 清单）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
86: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/BloodSword/BloodSword.nut", "swordman_bloodsword", 60, -1);
```

状态号 **60**（不绑定技能 ID 的通用注册；技能 103 → 状态 60 的映射在引擎侧，另见下方 mod 中断系统直接证据）。

### 2.2 主 nut 逐回调（BloodSword.nut，49 行实测，mod 混淆壳 + 引擎本体）

mod 中断系统里的调用（ap_swordman_comminterrupt.nut case 3 狂战区，嗜血封魔斩 toggle 时）：

```
EnableSoften(obj, 103, 60);
SetSkillState(obj, 103, 60, [102]); //血气之刃——技能 103 → 状态 60 实证
```

nut 现存回调（**均为 mod 注入，C3 混淆**；原版流程引擎内置）：
- `onSetState_swordman_bloodsword(obj, skillId, vector, ?)`：vector[0]==103 分支 → 创建**装备系 PO 48312**（ElementalBusterExpBodyFire，写包 2000+朝向）+ 攻速×1.5 + var bool(0)=false；else 分支 → bool(0)=true + **攻速×12.0**（mod 提速改造）。两分支均为 mod 附加（48312 是装备特效对象，与血气之刃本体无关）。
- `onProcCon`：bool(0)==false 且当前帧≥11 → 对 PO 48312 发 `sq_SendHitObjectPacketWithNoStuck`（mod 提前引爆逻辑）。
- `onAttack`：命中时 bool(0)=true 终止上述循环。
- `onCreateObject_swordman_bloodsword(obj, createObject)`：新建对象为 PO 且 collision index==**20066（BloodSwordExplosion）**→ `AddSetStatePacket(STATE_STAND)`——**爆炸 PO 出现后角色回待机**（原版时序的关键旁证）。

**原版引擎流程重建（.ani 标记 + .obj 数据 + atk 印证；推断标注）**：
1. 施法：扣 HP（col2），播 `BloodSwordMake.ani`（380ms，全帧霸体，无攻击盒——喷血成刃）。
2. 成刃完毕切 `BloodSwordCharge.ani`（1045ms）：抓刃前刺。**F5 SET FLAG 1**（刺击起手标记，推断）；**F6-F13 有攻击盒**（角色 .ani 实测，见 2.4——判定即帧驱动，非 PO）；攻击信息 = etc attack info #72 `BloodSword.atk`。
3. 刺中敌人 → 血刃留在敌身体上 → 创建爆炸 PO **20066**（`BloodSwordExplosion.obj`）→ 角色回待机（onCreateObject 旁证）。
4. 按后方向键 → 原地刺击变体（引擎输入分支，无脚本）。

### 2.3 被动对象（BloodSwordExplosion.obj，ID 20066，passiveobject.lst:11265 实测）

| .obj 节 | 值 | 说明 |
|---|---|---|
| [basic motion] | `Animation/BloodSwordExplosion/explosion_28.ani` | 19 帧 1000ms；F0 SET FLAG 1；**F0-F4 有攻击盒**（F0 `-116 -67 -80 232 134 174`、F1-4 `-160 -67 -104 320 134 235` → min/max 口径 F1 x∈[-160,320] y∈[-67,134] z∈[-104,235]≈3.2×2×3.4 单位大爆炸） |
| [attack info] | `AttackInfo/BloodSwordExplosion.atk` | physic/damage bonus 100/damage/push50/hit info blow/blood 0.5/100 |
| [int data] | `3  300` | **推断=3 次×300ms 多段结算**（爆炸 [int data] 是 R2-A10 记档的翻译缺口节；F0-F4 盒活动期 290ms 与 3×300 组合语义未完全考证） |
| [object destroy condition] | on end of animation | 1000ms 后销毁 |
| .als | `explosion_28.ani.als` | **29 层叠加**（[use animation] 注册 Explosion_00-27/29 全套 + [none effect add] 帧全 0、层 -1~10028）——多层同帧爆炸视觉 |

变体：`BloodSwordExplosion_DS.obj`（20079）、`BloodSwordExplosion1.obj`（130020，basic motion=exp.ani）、`bloodswordexplosionboom.obj`（**未在 passiveobject.lst 注册**——引擎内置专用，同 gorecrossmercilessness 情形）。exp.ani.als = exp_dodge.ani @层 10。
无 PO 行为 nut（appendage/ 无 ap_bloodsword——引擎内置）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/bloodswordmake.ani` | 18 | 380ms | 无 | 无 | **全帧 SUPERARMOR**；.als 叠 make/make_dodge 特效 |
| `bloodswordcharge.ani` | 17 | 1045ms | **F5=1** | **F6-F13**（F6 `-3 -20 47 154 39 45`；F7/F8 `82 -20/-21 40 133 43/41 50`；F9-F13 `83 -19 40 137/147 39/40 45`） | F0-8 SUPERARMOR；刺击判定为**帧驱动角色盒**（DNF 罕见实装数据）；.als 叠 4 特效 + [create draw only object] |
| `passiveobject/.../explosion_28.ani` | 19 | 1000ms | F0=1 | F0-F4（数值见 2.3） | + .als 29 层 |
| `effect/.../bloodsword/make.ani` / `make_dodge.ani` | — | — | — | — | 成刃特效（.als 挂层 10000/10001） |
| `charge.ani` / `charge_body.ani` / `charge_dodge.ani` / `dust.ani` | — | — | — | — | 刺击特效（.als 挂层 10000-10002；dust 走 [create draw only object] 帧 8） |

攻击盒解读（角色 .ani min/max 口径）：刺击盒是一条 **x 前伸 154→215px、z 高度带 40-50px 的水平窄条**（F6 起点近身、F7 起前移到 82-133px——刺击推进的盒演化）；demo 折算盒深约 2.1 单位、高 0.5 单位。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodSword.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodSword.skl` | ✅ | 技能数据（MP0/HP 消耗/3 列+范围） |
| 注册行 | swordman_load_state.nut 86 行 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 60 |
| 主 nut | BloodSword.nut | `…\pvf\sqr\character\swordman\bloodsword\BloodSword.nut` | ✅（49 行，mod 混淆+注入） | 门禁/回待机旁证；核心引擎内置 |
| .chr 条目 | etc motion #102/#103 + etc attack info #72 | `…\pvf\character\swordman\swordman.chr` 1075/1076/1366 行 | ✅ | Make/Charge.ani；BloodSword.atk |
| 角色 .ani | bloodswordmake.ani（+.als）、bloodswordcharge.ani（+.als、+[pvp]×2） | `…\pvf\character\swordman\animation\` | ✅ | 帧表见 2.4 |
| 角色 .atk | BloodSword.atk | `…\pvf\character\swordman\attackinfo\BloodSword.atk` | ✅ | 刺击命中（pvp 分支 push100/lift60） |
| PO 定义 | bloodswordexplosion.obj（+1/_ds/boom） | `…\pvf\passiveobject\character\swordman\` | ✅ | 爆炸 PO 20066（boom 未注册） |
| PO .atk | bloodswordexplosion.atk（+1） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 爆炸命中 |
| PO .ani | explosion_28.ani（+.als）、explosion_00-29.ani（+[pvp]×30）、exp.ani（+.als）、exp_dodge.ani | `…\pvf\passiveobject\character\swordman\animation\bloodswordexplosion\` | ✅（66 文件） | 爆炸视觉族 |
| 特效 .ani | make/make_dodge/charge/charge_body/charge_dodge/dust.ani | `…\pvf\character\swordman\effect\animation\bloodsword\` | ✅ | .als 挂接的施法特效 |
| 装备层 | bloodswordmake/charge.ani | `…\pvf\equipment\character\swordman\avatar\`（belt_a 实测 2 件） | ✅ | 换装图层（demo 不需要） |
| 关联强化 | BloodSwordEx.skl（168）/ cancelbloodsword.skl | `…\pvf\skill\Swordman\` | ✅ 实测 | E 类批/取消被动（记档） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集 | 必需（共享） | ✅ 已在库 |
| BloodSword/sword_normal.img / sword_dodge.img | sprite_character_swordman_effect_bloodsword.NPK | **血刃本体**（刺击主视觉） | **必需** | ❌ |
| BloodSword/BloodB.img / exp_normal.img / exp_dodge.img | 同上 | 爆炸 PO 视觉 | **必需** | ❌ |
| BloodSword/body_dodge.img | 同上 | 施法体光 | 可选 | ❌ |
| Guard/guard_attack_none.img | sprite_character_swordman_effect_guard.NPK | charge_dodge 特效跨目录引用（L14） | 可选 | ❌ |
| （explosion 族 29 个 .ani 与 charge/make .als 引用上表 img；有 1 帧 IMAGE 空路径占位） | — | 空占位帧 | — | — |

缺失 img：必需级 4 张（同 1 个 NPK，一次提取全覆盖）、可选级 2 张。img 版本红线由提取时把关。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/BloodSwordSkill.cs : SkillLogic`**（BloodBoomSkill HP 消耗范式 + ReleaseWave 帧盒范式）
   - `CooldownMs = 20000`（原值直用）；`TotalTimeMs = 1600`（make 380 + charge 1045 + 余量；爆炸 Area 独立存活不受此限）。
   - `MinCastHpPct = 5`（DNF 无明示 HP 门槛，血气系惯例取安全值）；`OnCast`：`ctx.PlayAnim(AnimId.SwordmanBloodSwordMake)` + `ctx.ClearHitTargets()` + `ctx.ConsumeCasterHp(固定 200)`（DNF col2 原值 Lv1=187，demo 取 200 或 maxHp 3%）。
   - `OnUpdate` 段机：SubState==0 且 make 动画播完（帧≥17）→ `PlayAnim(AnimId.SwordmanBloodSwordCharge)`、SubState=1（刺击判定 F6-F13 走 json 帧盒自动激活——同 ReleaseWave SwordmanReleaseWaveDash 先例，LSHitboxComponentSystem 判定帧表加 `SwordmanBloodSwordCharge`）；
     SubState==1 且 charge 帧至 F13（约 845ms，刺击收势）→ `ctx.CreateAreaInFront(AreaIds.BloodSwordExplosion, (FP)8/10)`（爆炸中心=刺击落点，DNF 刺入点推断）+ `ctx.DisableAttackHitbox()`（若帧盒已自动收则免）、SubState=2。
   - `HitReaction`（技能级=刺击）：`{Damage=120, HitstunMs=500, KnockbackX=30, LaunchY=30}`（BloodSword.atk 原值 push30/lift30/hit horizon/blow）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - 按后方向键原地刺击：**不做**（技能中方向输入缺口，R1-A3 记档）——demo 固定前冲刺。
2. **`DotNet~/Areas/BloodSwordExplosionArea.cs : AreaDefinition`**（FireCircleArea Tick 范式 + BloodBoomArea 视觉范式）
   - `TotalTimeMs=900`、**`TickTimeMs=300`（→ 3 次 Tick=[int data] "3 300" 多段直译，R2-A8 同段定时多段=Area Tick 可表达）**、`TickActions={MeleeHit}`、
     `HalfExtents=(24/10, 1, 17/10)`（PO F1 盒 x[-160,320]/y[-67,134]/z[-104,235] 折半）、
     `HitReaction{Damage=130, HitstunMs=600, KnockbackX=50, LaunchY=0}`（PO atk 原值 damage bonus 100/push50/blow）、
     `ViewAnimId=AnimId.BloodSwordExplosionPo`（explosion_28 译 json；其 .als 29 层 demo 只注册 2-3 层：BloodB/exp_normal/exp_dodge）。
3. 无需新 Buff/Action（MeleeHit/帧盒/Tick 全现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| HP 消耗（MP=0） | `ConsumeCasterHp` + `MinCastHpPct`（BloodBoomSkill 同构，已有门面） |
| make→charge 两相位 | SubState 段机 + 两 PlayAnim |
| 刺击判定 F6-F13（角色 .ani 帧盒） | json attackBoxes 帧驱动（ReleaseWave as-built §5.6-6 同构） |
| 刺中→爆炸（命中触发） | 简化为**时间驱动**（charge F13 时点 CreateArea）——SkillLogic 无命中回调，行为差异见 §7 |
| 爆炸 PO 20066 + [int data] 3×300 | Area TickTimeMs=300 × TotalTimeMs=900（多段直译） |
| 爆炸 29 层 .als | overlay 注册 2-3 层（视觉降级） |
| 按后方向键原地刺 | 缺口（技能中方向输入），跳过 |
| 爆炸中心=刺入点 | `CreateAreaInFront(0.8)` 固定前偏 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `…\Runtime\SkillIdAttribute.cs` | `SkillIds.BloodSword = 21` + 按键 case |
| AreaId | `…\Runtime\AreaDefinition.cs` | `AreaIds.BloodSwordExplosion = 11`（A12 段） |
| AnimId | `…\npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanBloodSwordMake=86`、`SwordmanBloodSwordCharge=87`、`BloodSwordExplosionPo=88`（特效层可选 89-91） |
| 帧盒判定表 | `…\lockstep\...\LSHitboxComponentSystem.cs` | 判定帧表加 `SwordmanBloodSwordCharge`（F6-F13 自动激活） |
| json/图集/按键 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem | 3+ json；图集 4 张必需 img；新按键 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms（施放即起算） | 20000 |
| HP 消耗 | col2 187（Lv1）→ 3763 | 固定 200（或 maxHp 3%） |
| 施法门槛 | 无明示 | MinCastHpPct 5 |
| 总时长 | make 380 + charge 1045 = 1425ms | 1600 |
| 刺击伤害 | col0 1170%→9522% | 120 |
| 刺击反应 | atk：damage/push30/lift30/hit horizon/blow | Damage120/Hitstun500/Kb30/Ly30 |
| 爆炸伤害×3 段 | col1 5310%→43215%（PO damage bonus 100） | 每段 130 ×3 tick/300ms |
| 爆炸反应 | PO atk：damage/push50/blow | Kb50/Hitstun600 |
| 爆炸盒 | F1 x[-160,320] y[-67,134] z[-104,235] | HalfExtents (2.4, 1.0, 1.7) |
| 刺击盒 | F6-F13 x 至 154-215px 水平窄条 | json 帧盒直译（不手折算） |
| 霸体 | make 全帧+charge F0-8 | 不做（延后） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| bloodswordexplosion.obj `[int data] 3 300` | **[int data] 节**（R2-A10 已记档的 .obj 缺口字段） | obj 子命令立项时纳入 intData 数组；当前手抄（本技能已抄入 §2.3） |
| bloodswordcharge.ani.als `[create draw only object]` | **无 follow 后缀变体**（R2-A10 再实证，本技能第 3 例） | als 子命令按 [add] 同构支持（帧号 + 别名 + 层参数），游戏侧作"仅绘制对象"处理 |
| 角色/PO .ani 的 `[SET FLAG]`（charge F5、explosion F0）/`[DAMAGE TYPE] SUPERARMOR` | 既有约定跳过 | 触发帧 const 进技能类 |
| `.skl` / `.atk` ×2 / `.obj` ×4 | 尚无子命令 | 手抄可行；累计记档 |
| explosion_28.ani 的 `[IMAGE RATE]`、charge/make 的 `[PLAY SOUND]`、特效层 `[INTERPOLATION]`/`[RGBA]` | [IMAGE RATE] 延后记档；[PLAY SOUND] 约定跳过；[INTERPOLATION] 不在任何清单（060 §8 上报）；[RGBA] 已支持 | 除 [INTERPOLATION] 上报外无新缺口 |
| explosion_28.ani.als 29 组 [use animation]+[none effect add] | **均已支持**（规则表内），仅量大 | 批量翻译无碍；游戏侧按需注册子集 |

结论：实质缺口 2 条（[int data] 字段、[create draw only object] 无后缀变体——均为**既有记档的第 N 次实证**，非新增类型）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 刺中敌人才炸（命中触发爆炸） | SkillLogic 无命中回调（无"刺击命中"事件面） | 时间驱动：charge F13 固定时点起爆（打空也爆，行为差异：原版空刺不爆） |
| 按后方向键原地刺击 | 技能中方向输入读取（R1-A3 缺口） | 固定前冲刺击 |
| make/charge 前段霸体 | 霸体帧延后 | 不做 |
| 爆炸 29 层视觉 | 无碍（overlay 可表达，量的问题） | 注册 2-3 层主视觉 |
| 刺击/爆炸按物理%结算 | 属性数值无伤害消费链（R1-A4） | MeleeHit 固定值 |
| 攻速 ×12（mod 注入） | —（mod 行为不还原） | 忽略 mod 壳逻辑 |
| 音效 | 延后 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 6 值中 [0]=500、[2]=170、[4]=250 的语义（[5]=100=爆炸范围%已证；引擎消费无脚本——170 疑似与 slashofboom PO 出生 170px 同为"身前判定距离"惯例）。
2. [int data] `3 300` 的精确语义（推断 3 次×300ms；与 F0-F4 盒活动期 290ms 的组合方式未完全对上）。
3. 引擎刺击命中→爆炸的准确触发条件（刺中任意敌人即爆 vs 刺击动画结束即爆——onCreateObject 只证"爆炸出现后回待机"）。
4. explosion_00-29 全族与 28 号的选用规则（.obj 固定 28；其余 29 个疑似按爆炸范围/等级选档，未考证）。
5. bloodswordexplosionboom.obj（未注册）的引擎用途。

**新系统级缺口上报**：**SkillLogic 命中事件面**（"我的攻击盒命中了谁"回调/查询——血气之刃"刺中才爆"是首个明确依赖此语义的 A 类样本；现有只有 Area/Bullet 内部结算，技能本体拿不到命中结果）。建议与 ctx.EndCast() 一并考虑门面设计。

**翻译工具缺口**：[int data]、[create draw only object] 无后缀变体（均为既有记档追加实证）。
