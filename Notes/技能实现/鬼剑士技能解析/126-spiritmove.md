# 鬼步（spiritmove）

> 技能ID 126 | 级别 B（预判 A 纠偏：位移+姿势状态技） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼步 | `skill\Swordman\ghostsword\spiritmove.skl [name]` |
| 英文名 | spiritmove（skl 文件名；本 skl 无 [name2] 节） | 同上实测 |
| 职业 | 剑影（[skill fitness growtype]=5） | 同上 |
| 学习等级 | 15 | 同上 [required level] |
| 最高等级 | 60（各觉醒段上限 50） | 同上 |
| 类型 | active（skill class 1） | 同上 |
| 指令 | ←→ + Space（BUFF 键） | 同上 [command] |
| CD | 6000 ms（pvp 10000 + 起手 CD 10000） | 同上 [cool time] / [pvp] |
| MP | 20 → 210 | 同上 [consume MP] |
| 前置 | 技能 123（鬼影剑）Lv1 | 同上 [pre required skill] |
| static data | `400 120 3 300`：突进距离 400px / 攻击范围 120% / 多段攻击次数 3 / 无敌时间 300ms（nut 实证取用，见 §2） | 同上 [static data] + spiritmove.nut |
| 一句话效果 | 施放摆出准备姿势，再按 Z/X/技能键以肉眼无法追踪的速度前突，攻击路径上敌人（3 段多段伤害 + 300ms 无敌）；姿势下按剑术技能键可发动该技能的特殊终结 | 同上 [explain] |

**level property（1 列，Lv1 → Lv60）**：`254→2038`（+26/级）= 多段攻击力 `<int>%%`（PO 实证 `sq_GetBonusRateWithPassive(SKILL_SPIRITMOVE, -1, 0, …)`）。
模板其余三行对应 static：突进距离/攻击范围/多段次数/无敌时间（<float2> 秒 = 0.3s）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
154: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/spiritmove/spiritmove.nut", "spiritmove", STATE_SPIRITMOVE, SKILL_SPIRITMOVE);
18:  IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);
162: IRDSQRCharacter.pushState(…, "…/returnspiritmove/returnspiritmove.nut", "returnspiritmove", STATE_RETURNSPIRITMOVE, SKILL_RETURNSPIRITMOVE);   // 关联技 128 回返鬼步（另批）
```

- STATE_SPIRITMOVE=111 / SKILL_SPIRITMOVE=126（swordman_header.nut:42/148 实测）。
- 判定体共用 PO 24349，本技能用子 id **41**（与 speedslash 子状态 2 同一 PO）。
- 攻速联动同 speedslash（鬼影剑 123 SpeedRate）。

### 2.2 主 nut 逐回调（spiritmove.nut，155 行）

**onSetState**
- 子状态 0（准备姿势）：播 CUSTOM_ANI_SPIRITMOVE1=275（spiritmove1.ani，**末帧 delay 1760ms 的持剑姿势**）；
  `sq_StopMove`。
- 子状态 1（突进）：播 CUSTOM_ANI_SPIRITMOVE2=276（spiritmove2.ani）；
  **无敌**：`sq_SendMessage(OBJECT_MESSAGE_UNBREAKABLE, 1)`，`sq_PostDelayedMessage(…, 0, lastTime=static[3]=300ms)` 恢复（pvp 不生效）；
  计算目标 x = 现位置 + static[0]=**400px** 存 var spiritMove；
  写包 dword **41** → `CreatePassiveObject(24349, 0, 300, 0, 0)`（PO 出生在身前 300px）。

**onProc（子状态 1）**：`sq_GetUniformVelocity(起点, 目标, currentT, 500)` + `sq_MoveToNearMovablePos` ——
**500ms 内匀速前突 400px**（撞墙可止）。与 ReleaseWaveSkill 的纯函数位移同构（§5 直接复用范式）。

**onProcCon（每帧条件）**
- 子状态 0（姿势中的输入窗口）：
  - `setSkillCommandEnable(126)` + `sq_IsEnterSkill(126)`（再按本技能键）→ 切子状态 1（突进）；
  - 按住攻击键（OPTION_HOTKEY_ATTACK）→ 子状态 1；
  - 按住技能键（OPTION_HOTKEY_SKILL）→ 子状态 1；
  - 按住跳跃键 → 回站立（**姿势取消**；explain 的"按 C 取消"是另一键位映射）；
  - `SpiritMoveContact(obj)`——**剑术技能联动**（见 §2.3）。
- 子状态 1：`whiteGhostSlshContact(obj)`（白鬼一闪取消，同 127 文档 §7，跳过）。

**onEndCurrentAni**
- 子状态 0 →（动画播完=超时未按键）→ 子状态 1（**explain"一定时间内不按键自动发动"的实现：姿势动画总长 2000ms 走完自动突进**）。
- 子状态 1 → 回站立。

### 2.3 剑术技能联动（SpiritMoveContact，jg_swordman_common.nut:737 实测）

姿势期间对 5 个剑术技能各调一对：
- `GhostSwordCommandEnable(技能ID, 状态)`：不在 CD 且 MP 足 → `setSkillCommandEnable(该技能, true)`（图标点亮）；
- `GhostSwordSetState(技能ID, [子状态], 状态)`：`sq_IsEnterSkill(该技能)` 时**手动扣 MP + `startSkillCoolTime`**，
  然后把角色直接切进该技能的**终结子状态**：
  - 鬼连斩(127) → 子状态 [2]；鬼连牙(GHOSTPIERCE) → [2]；白鬼一闪(WHITEGHOSTSLASH) → [2]；
    鬼魂切(GHOSTDECOLLATION) → [2]；裂魂乱舞(SWORDDANCEBS) → [4]。

即：**姿势下按剑术键 = 跳过该技能本体、直接执行"鬼步突进 + 该技能终结动作"**（speedslash 子状态 2 的 contact 动画
+ 突进 + PO41 鬼步多段伤害；其他技能同构）。鬼步命中伤害全部走 PO41（鬼步列 0），"额外适用剑术技能攻击力"
的数据通道未见（与 127 文档 §8 同一存疑）。

### 2.4 被动对象（24349 子 id 41）

setcustomdata（`sqr\shared_passive_object\swordman\setcustomdata.nut` case 41，实测）：
- 动画 = [etc motion] 槽 50 = `character/swordman/effect/animation/spiritmove/spiritmovedasheffect_00.ani`
  （15 帧 × 60ms = 900ms，**每帧攻击盒 `-200 -60 0 400 120 200`**，携带 SpiritMove02.img 可见突进残像，
  且带 .als 边车叠 5 层烟/终结特效，§3）；
- 攻击信息 = 对象表 30 = `attackInfo/spiritmove.atk`（§2.5 数值）；
- 伤害 = `sq_GetBonusRateWithPassive(126, -1, 0)`（列 0，254%+）；
- 攻击盒缩放 = static[1]=120%；
- **多段**：`setTimeEvent(0, 动画总时长/HitCount, …)`，HitCount = static[2]=**3** → 每 300ms
  `resetHitObjectList()`（ontimeevent.nut case 0 实测）→ 一个敌人最多吃 3 跳；`sq_SetMaxHitCounterPerObject(3)`。
- onattack：命中挂剑影命中特效（GhostSword_Attack_Effect，纯视觉）；onendcurrentani：播完即毁。

### 2.5 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/spiritmove1.ani`（姿势，槽 275） | 8 | 2000ms（末帧 1760ms） | 无 | 无 | 末帧长 delay=姿势保持；播完=自动突进 |
| `character/swordman/animation/spiritmove2.ani`（突进，槽 276） | 7 | 365ms | 无 | 无 | 判定在 PO |
| `effect/animation/spiritmove/spiritmovedasheffect_00.ani`（PO 槽 50） | 15 | 900ms | 无 | **全 15 帧**（`-200 -60 0 400 120 200`） | 突进残像主层 + .als 叠 5 层 |
| 同目录 spiritmovedashsmoke_00/01/02.ani、spiritmovefinisheffect_00.ani、spiritmovefinishsmoke_00.ani | 未逐帧 | — | — | — | .als 注册的叠加层（Smoke×3、终结光、终结烟） |

命中反应（`passiveobject\unclebang_shared_passive_object\swordman\attackInfo\spiritmove.atk`，实测）：
物理 / **damage reaction=none**（无受击硬直反应类型）/ push aside **100** / lift up **200** / attack direction=hit horizon /
hit info=**blow**（吹飞）/ no blood / knuck back -1 / 音效 R_SWD_HIT。
→ 我们的 HitReaction：Damage=列0 结算、HitstunMs≈0（none 反应）、KnockbackX=100、LaunchY=200。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | spiritmove.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\spiritmove.skl` | ✅ | 技能数据 |
| 注册行 | swordman_load_state.nut:154 / :18 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态注册 + PO 24349 |
| 主 nut | spiritmove.nut | `…\pvf\sqr\character\swordman\5_ghostsword\spiritmove\spiritmove.nut` | ✅（155 行） | 姿势/突进两子状态 |
| 常量 | swordman_header.nut:42/148/446-447 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | STATE 111 / SKILL 126 / ANI 275-276 |
| 联动公共 | jg_swordman_common.nut:737 SpiritMoveContact / :757 GhostSwordCommandEnable / :777 GhostSwordSetState | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅ | 剑术联动（load_state:3 引入） |
| PO 壳+逻辑 | po_swordman_shared.nut + swordman/setcustomdata.nut(case 41)/ontimeevent.nut(case 0)/onattack/onendcurrentani | `…\pvf\sqr\shared_passive_object\…` | ✅ | 多段弹体逻辑 |
| PO 定义 | swordman_shared.obj（etc motion 槽 50） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ | 动画映射 |
| .chr 条目 | etc motion #275/#276（1248-1249 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | 动画映射 |
| 角色 .ani | spiritmove1/2.ani | `…\pvf\character\swordman\animation\` | ✅ | 姿势/突进动作 |
| 角色 .atk | —（无 spiritmove.atk，攻击信息在对象表 30） | `…\pvf\character\swordman\attackinfo\`（实测无） | ⛔ | 命中参数走 PO（L3 惯例） |
| PO .atk | attackInfo/spiritmove.atk | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\attackInfo\spiritmove.atk` | ✅ | 突进多段命中反应 |
| PO/特效 .ani | spiritmovedasheffect_00.ani + 5 个叠加层 | `…\pvf\character\swordman\effect\animation\spiritmove\` | ✅ | 突进视觉 |
| .als 边车 | spiritmovedasheffect_00.ani.als | 同上 | ✅ | 叠 Smoke×3/终结光/终结烟 5 层（[none effect add]） |
| 装备层 | spiritmove*.ani ×152 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | avatar 变体图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 132/238-241） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色 姿势/突进动画 | **必需** | ✅ 已在库 |
| SpiritMove/SpiritMove02.img | sprite_character_swordman_effect_spiritmove.NPK | 突进残像主层（PO 视觉） | **必需**（突进段核心视觉） | ❌ |
| SpiritMove/SpiritMove01.img | 同上 | DashSmoke_00 叠层 | 可选 | ❌ |
| SpiritMove/Smoke01.img、Smoke02.img | 同上 | DashSmoke_01/02 叠层 | 可选 | ❌ |
| SpiritMove/Smoke03.img | 同上 | 终结烟（finishsmoke） | 可选 | ❌ |
| GhostSlashBS/GhostSlashBS05.img | sprite_character_swordman_effect_ghostslashbs.NPK | 终结光（finisheffect，与 127 共用） | 可选 | ❌ |

缺失 img：必需 1 张、可选 5 张（2 个 NPK）。

## 5. 实现方案草案（🔶：姿势输入窗口 + 位移 + 单段简化）

1. **`DotNet~/Skills/SpiritMoveSkill.cs : SkillLogic`**（ReleaseWaveSkill 位移范式 + 输入缓冲）
   - `CooldownMs = 6000`；姿势段 `TotalTimeMs` 拆两段管理：施放 → 姿势（SubState=0，最长 2000ms）→ 突进（365ms）。
     实现：`TotalTimeMs = 2400`（姿势 2000 + 突进 365 + 余量），全程 SubState 推进：
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSpiritMove1)`（姿势）；SubState=0。
   - `OnUpdate`：
     - SubState 0（姿势窗口 2000ms）：`ctx.PeekBufferedButton()` 非空（攻击键 J=button1 / 本技能键——
       现有输入缓冲已能表达"再按 Z/X/技能键"）→ `ctx.ConsumeBuffer()` + 切 SubState=1；
       跳跃键取消：现有按钮表无跳跃（§7 简化）；姿势到时（ElapsedMs≥2000）自动切 SubState=1。
     - SubState 1（突进）：`ctx.PlayAnim(AnimId.SwordmanSpiritMove2)`（一次性，用 SubState=2 守卫）；
       位移 = `ctx.MoveCasterForward(4 × min(Δt,500)/500)`（400px=4 单位，ReleaseWaveSkill.OnUpdate 纯函数同构，
       不存起点回滚安全）；判定：突进期间 `ctx.SetAttackHitbox(前偏 1.5, 半尺寸 (2.0,0.6,1.0))`
       （PO 盒 `-200 -60 0 400 120 200` 换算）+ `HitActions={MeleeHit}`。
   - `OnEnd`：`ctx.PlayDefaultAnim()` + `ctx.DisableAttackHitbox()`。
   - **多段简化（🔶 核心降级点）**：DNF 为 900ms 内 3 跳（每 300ms resetHitObjectList）。现有系统多段命中在延后档
     （§6.3）。demo 简化为**突进路径单次命中**（HitTargets 去重天然单次）；如坚持多段，可把突进拆成
     3 个时窗各 `ctx.ClearHitTargets()`（等价手工 reset——技巧上可行：OnUpdate 里每 300ms 清一次命中表，
     无需框架改动，建议实现时直接采用，多段即恢复）。
2. **无敌 300ms**：无无敌系统（§7 记档，新缺口）；简化跳过。
3. **剑术联动**（姿势下按其他剑术技能→其终结段）：跨技能状态跳转 + 手动扣 MP/起 CD 语义。
   demo 建议：姿势窗口只响应"任意已缓冲按键 → 突进"（即全部按普通鬼步处理），联动等技能取消体系。
   若只做鬼连斩一档联动：姿势窗口读 button==鬼连斩键 → `cast.EndNow` + 立即 `SkillCastHelper.TryCast(鬼连斩)`
   进入其"contact 段"（需给 SpeedSlashSkill 预留 SubState 直入接口——可行但建议缓做）。
4. 需要新增的 Action/Buff/Bullet/Area：无。

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SpiritMove = 12` + 新按键（如 B） |
| AnimId | `Runtime\AnimConfigRegistry.cs`（npkparser） | `SwordmanSpiritMove1 = 50`、`SwordmanSpiritMove2 = 51`、`SpiritMoveDash = 52`（PO 残像层） |
| json/图集/按键/翻译 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem / DnfConfigTranslation | spiritmove1/2 + spiritmovedasheffect(+als) json；SpiritMove 图集 1 个 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 6000ms | 6000 |
| 姿势窗口 | 2000ms（姿势动画总长） | 2000 |
| 突进 | 400px / 500ms 匀速 | 4 单位 / 500ms |
| 无敌 | 300ms（UNBREAKABLE） | 跳过（无系统） |
| 多段 | 3 跳 × 每 300ms（列0 254%+ 每跳） | 单跳 120 或 3×60（ClearHitTargets 手工多段） |
| HitReaction | spiritmove.atk：none 反应/push 100/lift 200/blow | Damage 120/HitstunMs 0/Kb 100/Ly 200 |
| 突进判定盒 | PO 全帧 `-200 -60 0 400 120 200` | 前偏 1.5、半尺寸 (2.0,0.6,1.0) |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `spiritmove.skl` | `.skl` 无子命令（static 四元组含义靠 nut 佐证） | 手抄；并入 skl 子命令建议 |
| PO `spiritmove.atk` | `.atk` 无子命令 | 手抄 |
| `spiritmovedasheffect_00.ani`（PO） | 常规节（LOOP/FRAME/IMAGE/DELAY/ATTACK BOX） | 现有 ani 子命令全覆盖 |
| `spiritmovedasheffect_00.ani.als` | `[use animation]` + `[none effect add]` ×5 | **现有 als 子命令全覆盖** |
| `swordman_shared.obj` | `.obj` 无子命令 | 本技能不需直译（槽 50 映射已手抄进本文档） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 突进 3 跳多段（每 300ms resetHitObjectList） | 多段命中（延后档） | 单跳；或技能内每 300ms `ClearHitTargets()` 手工多段（零框架改动，推荐） |
| 300ms 无敌（UNBREAKABLE 消息） | **无敌帧系统缺失（新缺口）** | 跳过；需要时给 LSCast/LSCombat 加 InvulnerableTimer 门面（受击系统检查） |
| 姿势下跳跃键取消 | 按钮表无跳跃键（输入面窄） | 不做（姿势到时自动突进已兜底） |
| 剑术联动（5 技能各自终结段 + 手动扣 MP/起 CD） | 技能取消体系缺失（064 已上报）+ Buff 查询门面 | demo 不做联动；姿势窗口任意按键→普通突进 |
| 再按 Z/X/技能键的细分输入（三键位分别判定） | 输入缓冲单槽位（PeekBufferedButton 只有一个值） | 已可表达（任一键缓冲即触发），键位细分差异忽略 |
| PO 残像 5 层叠加（.als） | overlay 机制已有（区域/单位均支持） | 翻译 als 后挂突进段视图（或 Area overlay，releasewave 先例） |
| 攻速联动（鬼影剑 123 加速动画） | 延后档（同 127） | 固定 1.0 |

## 8. 存疑与缺口上报

**未考证项**
1. 姿势段`setSkillCommandEnable` 对 5 个剑术技能图观点亮（UI 层行为），我们无技能图标系统——视为纯 UI，忽略。
2. `sq_SetMaxHitCounterPerObject(3)` 与 setTimeEvent 的组合细节（每敌最多 3 跳的封顶由谁执行）——推断 timeEvent
   reset 即全部来源，未逐行考证引擎侧。
3. 回返鬼步（returnspiritmove，技能 128）与鬼步的关系（另有 pushState，见 §2.1 行 162）——另批分析。

**新系统级缺口（§6.3 清单外）**
1. **无敌帧（UNBREAKABLE）**：`sq_SendMessage(OBJECT_MESSAGE_UNBREAKABLE, 1)` + 定时恢复——突进/闪避类技能通用。
   建议：LSCast 或 LSCombat 加 `InvulnerableTimer`（进快照），受击入口检查；数据面给 SkillLogic 加虚属性
   `InvincibleMs`（DNF static 直译）。归"缺失"档，多个位移技都会撞上。
2. **姿势/蓄力类输入窗口的通用化**：本技能（2000ms 姿势等键）与邪光斩蓄力（050 文档）同构——
   "施放态可被后续输入改写子状态"。现有框架可表达（SubState + PeekBufferedButton），但建议在
   SkillLogic 层沉淀一个 `WaitInputWindow` 惯例（文档级约定即可，无需改框架）。

**翻译工具缺口**：无新增（ani/als 全覆盖；skl/atk 为既有已知缺口）。
