# 破军升龙击（ChargeCrash）

> 技能ID 68 | 级别 A | 可实现性 ✅（冲刺+上挑主干全表达，ReleaseWave 先例直接套） | 分析日期 2026-08-22 | 批次 A8

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 破军升龙击 | `skill\Swordman\ChargeCrash.skl [name]` |
| 英文名 | ChargeCrash（skl 文件名；[name2]=`Charge Crash` 为官方英文名带空格，本表按惯例取文件名） | 同上实测 |
| 职业 | 剑魂（[skill fitness growtype]=1） | 同上 |
| 学习等级 | 30 | 同上 [required level] |
| 最高等级 | 70（各觉醒段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1）；`[auto cooltime apply] 1`（施放即进 CD——我们 TryCast 默认行为同构） | 同上 [type] |
| 指令 | ←→→ + Z（MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 10000 ms（pvp 13000） | 同上 [dungeon][cool time] |
| MP | 70 → 700（Lv1 → Lv70） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 关联强化 | [feature skill index] 218 = ChargeCrashExp.skl（强化-破军升龙击，TP 版）——追加下捶的精通门槛由该体系控制 | 同上 + lst 对表 |
| static data | `700 -1500 300 400`（pvp `700 -1500 300 600`）——**无 nut 消费者，语义未考证**；推断 [0]=冲撞速度/距离、[1]=减速/终止参数、[2]/[3]=上挑/收尾力参数（pvp 只差 [3]） | 同上（推断标注） |
| 一句话效果 | 肩冲撞击退敌人并顺势单手上斩浮空；习得巨剑/钝器精通并持对应武器时追加蓄气下捶 | 同上 [explain] |

**level property（2 列，Lv1 → Lv70）**：`288→2344`、`432→3516`。
列语义未考证（引擎内置消费）；推断 col0=冲撞攻击力 ‰、col1=上挑攻击力 ‰（与两段 .atk 一一对应）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
90: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/ChargeCrash/ChargeCrash.nut", "swordman_chargecrash", 37, -1);
    // 第4参状态号 37（老一代小号状态）、第5参技能 ID -1（不绑定特定技能，L2 语义）
```

**F3b 半引擎内置**：注册行存在，但 `chargecrash.nut` 仅 25 行且被 mod 作者混淆改写（变量名全乱码，C3 同类）——
不含任何状态回调（onSetState/onKeyFrameFlag/onProc 均无），只剩一个 mod 注入钩子：

```
// onAfterSetState_swordman_chargecrash（混淆还原，唯一逻辑）：
if (子状态 datas[0]==1 && datas[1]==0) 且挂有 appendage "ap_stateoflimit"（超越极限，技能 248 = swordman_stateoflimit.skl，lst:454 实测对表）:
    写包(248, 2, sq_GetBonusRateWithPassive(248, state, col2, 1.0))
    → sq_SendCreatePassiveObjectPacket(24370, 0, 30, -1, 0)
    // 24370 = 全鬼剑共享打击 PO（L20）；超越极限被动在冲撞瞬间附加一记额外打击
```

**核心三段式（冲撞→上挑→收尾）在引擎内置状态 37 里**，pvf 只留数据文件——施法时序按 050-GrandWave 的"数据侧反推法"重建（§2.2）。

### 2.2 引擎状态 37 行为重建（.ani 标记 + .atk + PO 数据三方印证）

**第一段·冲撞（ChargeCrashDash.ani，450ms）**
- 播 .chr etc motion 43（header `CUSTOM_ANI_CHARGECRASHDASH <- 43` ↔ .chr 1016 行对表实测）。
- 冲刺位移：引擎按 static（推断 [0]/[1]：初速 700、减速 -1500）推进，**撞到敌人即停驻转段**（DNF 破军核心手感；撞敌停驻为游戏行为常识推断，无脚本佐证）。
- 命中：F0-F3 帧自带攻击盒（`-10 -20 8 105 40 68`，min/max：x[-10,105] y[-20,40] z[8,68]——贴身窄长撞区），
  用 `chargecrashdash.atk`：damage 反应 / **push 200** / hit lift up 方向 / blow / knuck back 2。
- F3 flag 65534：0xFFFE 特殊标记（064 记档：取消窗口/命中标记惯例，语义未考证）。

**第二段·上挑（ChargeCrashUpper.ani，340ms）**
- 冲撞结束（撞敌或滑完）切入，.chr etc motion 44。
- 命中：F2/F3 攻击盒（F2 `-5 -15 22 160 30 128`、F3 `-30 -15 39 160 30 140`——从贴身挥到身前大片），
  用 `chargecrashupper.atk`：**down 反应 / lift 400 / hit down**——浮空挑起的数据源。
- 上挑弧光特效 `effect\animation\chargecrash\up-slash.ani`（引擎绘制，无脚本引用者）。

**第三段·收尾/下捶（ChargeCrashFinish.ani，640ms，条件段）**
- .chr etc motion 45；F0 flag 65534；**无攻击盒**——命中走 PO：`ChargeCrashSub.obj`（PO 20030，passiveobject.lst:11194 实测），
  名为"**银光落刃的飞溅**"（复用银光落刃的溅射表现）：basic `Animation/ChargeCrash/damage-back.ani` + etc `damage-front.ani`，
  命中 `ChargeCrashSub.atk`：**down / lift 300** / blow。
- 触发条件：习得巨剑/钝器精通并持对应武器（explain + feature 218 强化体系）——demo 无武器系统，跳过（§7）。
- `chargecrashfinish.atk`（down/push300/**lift -300** 砸地）亦在本段 .chr 注册——下捶把浮空敌人砸回地面。

**mod 附加（超越极限联动）**：挂 ap_stateoflimit 时冲撞瞬间额外 PO 24370 打击（写包 248/2/col2 倍率）——被动联动，demo 跳过。

### 2.3 被动对象（ChargeCrashSub.obj，20030，实测）

| .obj 节 | 值 | 说明 |
|---|---|---|
| [name] | `银光落刃的飞溅` | 复用银光落刃（016 批）的溅射 PO |
| [floating height] / [pass type] / [piercing power] | 1 / pass all / 1000 | 全穿透多目标 |
| [basic motion] | `Animation/ChargeCrash/damage-back.ani`（+ .als） | 背层溅射 |
| [etc motion] | `Animation/ChargeCrash/damage-front.ani` | 前景层（无 etc attack info 配对 → 纯视觉，L13） |
| [attack info] | `AttackInfo/ChargeCrashSub.atk` | down / lift 300 / blow |

同目录 `chargecrash\` PO 动画族：damage-back1/2.ani、choppinghammersubback/front.ani（下捶溅射，借圣职者 ChoppingHammer 贴图）、fire_dodge1-3.ani（火花）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/ChargeCrashDash.ani` | 5 | 450ms（50/50/50/50/150） | F3=65534 | **F0-F3**：`-10 -20 8 105 40 68` | 帧 0-3 每帧 1 盒；末帧 150ms 滑行 |
| `character/swordman/animation/ChargeCrashUpper.ani` | 5 | 340ms（30/30/60/60/160） | 无 | **F2/F3**：`-5 -15 22 160 30 128`、`-30 -15 39 160 30 140` | 上挑判定在中段两帧 |
| `character/swordman/animation/ChargeCrashFinish.ani` | 8 | 640ms（80×8 均分） | F0=65534 | 无 | 下捶姿势（命中走 PO） |
| `…/ChargeCrashDash.[pvp].ani`、`ChargeCrashUpper.[pvp].ani` | — | — | — | — | pvp 变体存在（未细读） |
| `passiveobject\…\animation\chargecrash\damage-back.ani`（+als/.pvp）等 11 个 | — | — | — | — | PO 溅射/下捶/火花视觉 |
| `character/swordman\effect\animation\chargecrash\`：charge/dash/down-slash/dustdash/dustdashlast/up-slash.ani | — | — | — | — | 蓄气/冲刺尾焰/下捶/灰尘/上挑弧光（引擎绘制，无引用者） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ChargeCrash.skl（258 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ChargeCrash.skl` | ✅ | 技能数据（2 列/static 4 值/auto CD） |
| 注册行 | swordman_load_state.nut:90 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 37 注册 |
| 主 nut | chargecrash.nut（25 行，mod 混淆） | `…\pvf\sqr\character\swordman\ChargeCrash\chargecrash.nut` | ✅ 存在但为薄壳 | 仅超越极限(248)联动钩子；核心逻辑引擎内置（F3b） |
| 常量表 | swordman_header.nut:213-215 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | CUSTOM_ANI 43/44/45（+EX 99-101） |
| .chr 条目 | etc motion 43-45（1016-1018 行）；etc attack info（1336-1338 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | Dash/Upper/Finish.ani + 三 .atk |
| 角色 .ani | ChargeCrashDash/Upper/Finish.ani（+2 个 pvp 变体） | `…\pvf\character\swordman\animation\` | ✅ | 三段动作（帧表见 §2.4） |
| 角色 .atk | chargecrashdash/upper/finish.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 三段命中反应 |
| PO 定义 | ChargeCrashSub.obj（20030）/ ChargeCrashSub_DS.obj（20091） | `…\pvf\passiveobject\character\swordman\` | ✅ | 下捶溅射 PO（.obj 名"银光落刃的飞溅"） |
| PO .atk | ChargeCrashSub.atk | `…\passiveobject\character\swordman\attackinfo\` | ✅ | down/lift 300 |
| PO .ani | chargecrash\ 目录 11 个（damage-back + .als 等） | `…\passiveobject\character\swordman\animation\chargecrash\` | ✅ | 溅射/下捶/火花视觉 |
| 角色特效 | charge/dash/down-slash/dustdash/dustdashlast/up-slash.ani | `…\pvf\character\swordman\effect\animation\chargecrash\` | ✅ | 引擎绘制弧光/尘（无脚本引用者） |
| 装备层 | chargecrash 相关 456 个文件 | `…\pvf\equipment\character\swordman\avatar\` | ✅（只查存在性） | avatar 变体图层 |
| 关联 | ChargeCrashExp.skl（218，强化）/ ChargeCrashEx.skl / swordman_stateoflimit.skl（248，二觉被动） | `…\skill\Swordman\` | ✅ 存在 | 强化/被动联动（另行分析） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色三段动作图集 | 必需（共享） | ✅ 在库（L16） |
| ChargeCrash/up-slash.img | sprite_character_swordman_effect_chargecrash.NPK | 上挑弧光（主视觉记忆点） | **必需** | ❌ |
| ChargeCrash/dash.img、dustdash.img、dustdashlast.img | 同上 | 冲刺尾焰/灰尘 | **必需**（冲刺感主体） | ❌ |
| ChargeCrash/charge.img、down-slash.img | 同上 | 蓄气/下捶 | 可选（下捶 demo 不做） | ❌ |
| ChargeCrash/damage-front.img、fire_dodge.img、sub_dodge.img | 同上 | PO 溅射/火花 | 可选 | ❌ |
| Priest/Effect/ChoppingHammer/subback.img、subfront.img | sprite_character_priest_effect_choppinghammer.NPK | 下捶溅射（跨职业借贴图，L14 常态） | 可选 | ❌ |

缺失 img：**必需级 4 张**（同一 NPK 一次提取）、可选级 6 张（跨 2 个 NPK）。

## 5. 实现方案草案

- **内容件清单**（ReleaseWaveSkill 冲刺先例 + 064 多相位 Area 定案，全部现有机制）：
  - `ChargeCrashSkill : SkillLogic`（SkillIds.ChargeCrash=18）：
    - `CooldownMs=10000`（demo 可缩 5000）；`TotalTimeMs=790`（冲撞 450 + 上挑 340）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanChargeCrashDash)`、`ctx.ClearHitTargets()`。
    - `OnUpdate`（SubState：0=冲撞中，1=冲撞完/已撞，2=上挑已发）：
      - 冲刺位移（纯函数，ReleaseWave 同构）：`t=min(elapsed,450)`，位移 = t/450 × 3.5 单位按帧差增量 `ctx.MoveCasterForward(...)`。
      - **撞敌停驻**（本技能机制重点）：SubState=0 时每 tick 遍历 `ctx.GetEnemies()` + `ctx.CheckHit(caster, enemy)`，
        任一命中 → 提前结束冲刺（SubState=1，剩余位移不再执行）+ `ctx.PlayAnim(AnimId.SwordmanChargeCrashUpper)`——
        即"撞到就停下接上挑"，全部用现有门面表达（无新机制）。
      - elapsed≥450 且 SubState=0 → 滑行到底，同样切上挑（SubState=1）。
      - 上挑命中：技能级 HitReaction 是单值、与冲撞段不同 → 冲撞段吃帧驱动盒 + 技能 HitReaction；上挑段在 Upper F2 时刻
        （elapsed≈450+100=550）`ctx.CreateAreaInFront(AreaIds.ChargeCrashUpper, 0.8)` 用独立反应（064 F15 召唤区同构）。
    - `HitReaction{Damage=90, HitstunMs=500, KnockbackX=200, LaunchY=0}`（chargecrashdash.atk：push200 直用，无浮空）；
      `HitActions={MeleeHit}`——冲撞 F0-F3 帧盒命中自动走此反应（帧驱动判定帧表，releasewavedash 先例：翻译后 json 自带 attackBox）。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - `ChargeCrashUpperArea : AreaDefinition`（AreaIds 新增）：`TotalTimeMs=200`、`TickTimeMs=0`、`EnterActions={MeleeHit}`、
    `HalfExtents=(0.83,0.4,0.55)`（Upper F3 盒 x[-30,160]/z[39,140] 折算）、
    `HitReaction{Damage=120, HitstunMs=800, KnockbackX=0, LaunchY=400}`（upper.atk：**lift 400 浮空挑起直用**，down 反应以 800ms 硬直近似）、
    `ViewAnimId=AnimId.ChargeCrashUpSlash`（up-slash.ani 弧光）。
  - **无需**新 Buff/Action/Bullet。下捶/超越极限联动不做（§7）。
- **概念映射**：引擎状态 37 三段 → 技能 OnUpdate 时间线 + SubState；撞敌停驻 → CheckHit 提前转段；
  冲撞帧盒 → 翻译 json 帧驱动自动判定；上挑 .atk → Area 独立 HitReaction（LaunchY 400）；
  65534 标记 → 忽略（取消窗口延后）；static 冲刺参数 → 常数 3.5 单位/450ms（原值语义未考证，取观感等效）。
- **注册点**：SkillIds.ChargeCrash=18 + ButtonToSkill 新键；AnimIds `SwordmanChargeCrashDash=71、SwordmanChargeCrashUpper=72、ChargeCrashUpSlash=73`（Finish=74 可选不占必需号）；
  LSAnimClipRegistrar ×3；BuildAtlas 加 chargecrash 图集 4 张必需 img；LSOperaComponentSystem 新键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000ms | 5000（手感演示） |
| 总时长 | 冲撞 450 + 上挑 340 = 790ms（收尾 640 为条件段） | 790 |
| 冲刺距离/时长 | static 推断（700/-1500 观感等效） | 3.5 单位 / 450ms 匀速 |
| 冲撞伤害/反应 | col0 288‰；push 200 / 无浮空 / blow | Damage 90 / Kb 200 / Hitstun 500 |
| 冲撞判定盒 | F0-F3：x[-10,105] y[-20,40] z[8,68] | 帧驱动直用（json 攻击盒） |
| 上挑伤害/反应 | col1 432‰；down / **lift 400** | Damage 120 / LaunchY 400 / Hitstun 800 |
| 上挑判定盒 | F2/F3：x[-30,160] z[39,140] | Area 半尺寸 (0.83,0.4,0.55)，前偏 0.8 |
| 下捶（条件段） | finish.atk：down/push300/lift -300 砸地 | 不做（武器系统缺失） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ChargeCrash.skl | `.skl` 无子命令（2 列 + static 4 值 + [feature skill index]/[auto cooltime apply] 节） | 手抄可行；skl 子命令时纳入 feature index 字段（强化技关联族多） |
| chargecrashdash/upper/finish.atk + ChargeCrashSub.atk | `.atk` 无子命令；dash.atk 含 `[knuck back] 2`、`[no blood] 30 1.5`；finish 含负 lift | 手抄（每文件 ≤8 值） |
| ChargeCrashSub.obj | `.obj` 无子命令（etc 无 atk 配对=纯视觉层再实证） | 手工映射（§5 已给） |
| ChargeCrashDash/Finish.ani `[SET FLAG] 65534` | 按既有约定跳过 | 触发帧 const 进技能类——非缺口 |
| damage-back.ani.als | 常规 [use animation]/[add] | 无缺口 |

结论：.ani/.als 全部可译；实质缺口 = `.skl`/`.atk`/`.obj` 三子命令，计 3 条（常驻，无本技能新增节缺口）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 巨剑/钝器精通 + 武器类型 → 追加蓄气下捶（Finish 640ms + PO 20030 + finish.atk 砸地） | 武器/精通系统缺失（§6.3 缺失档：换装/武器切换） | demo 只做 冲撞→上挑 两段；下捶资源已在 §4 记档，武器系统落地后补 |
| 超越极限(248)被动：冲撞瞬间附加 PO 24370 打击 | 属性数值无伤害消费链（R1-A4 最重缺口）+ 被动技能系统 | 跳过 |
| flag 65534 取消窗口（冲撞末帧/收尾首帧） | 技能取消体系（缺失档） | 忽略；现有硬直门禁 + 输入缓冲近似 |
| 冲刺撞墙止损（sq_MoveToNearMovablePos 族语义） | 撞墙检测延后（§6.3 延后档；ReleaseWave 同记） | 无地图碰撞，滑完全程 |
| 撞敌"推着走"细节（撞区命中即停 vs 推离后再停） | 无连续推挤物理（命中即 MeleeHit 结算 + LaunchOwner 推离） | 撞即停+击退 200，观感等效 |
| 蓄气特效 charge.ani（长按蓄力） | 蓄力输入缺失（IceWave 蓄力版同类） | 跳过（charge.ani 为下捶蓄气，随下捶一起砍） |
| 屏震/音效（CHASERP_HIT 等） | 屏震/音频延后 | 跳过 |
| 冲刺方向输入微调 | 技能中方向输入读取（R1-A3 已上报） | 固定朝向 |

## 8. 存疑与缺口上报

**未考证项**
1. static data `700 -1500 300 400` 逐值语义（无 nut 消费者；pvp 仅 [3] 600≠400，暗示 [3] 与浮空/收尾力相关）。
2. 2 列 level property 与冲撞/上挑的对应关系（col0/col1 推断）。
3. flag 65534 语义（两处：Dash F3、Finish F0）。
4. 撞敌停驻的精确判定（撞区首帧命中即停 vs 全程命中均可停）——demo 按"任意命中即停"实现，观感等效。
5. chargecrash.nut mod 钩子中 `sq_GetBonusRateWithPassive(248, state, 2, …)` 的 state 实参（混淆变量，疑为 37）；主循环已确认 248=超越极限，与本钩子 ap_stateoflimit 语境吻合。

**新系统级缺口（§6.3 清单外）**
- 无新缺口。撞敌停驻用 GetEnemies+CheckHit 轮询表达成立（成本可接受：每帧 O(场单位数)，demo 规模无碍）。
  若后续冲刺类技能密集，可考虑给框架加"接触检测助手"（一次性 API），暂不立项。

**翻译工具缺口（并入主循环汇总）**：`.skl`/`.atk`/`.obj` 三子命令（常驻 3 条，无新增节缺口）。
