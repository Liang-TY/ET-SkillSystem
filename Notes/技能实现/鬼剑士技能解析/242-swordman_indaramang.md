# 天雷·波动剑（swordman_indaramang）

> 技能ID 242 | 级别 A | 可实现性 🔶（标记-多段-引爆主体可直译；"再按引爆"与感电状态需简化） | 分析日期 2026-08-22 | 批次 A16

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 天雷 · 波动剑 | `skill\Swordman\swordman_indaramang.skl [name]` |
| 英文名 | swordman_indaramang（取 skl 文件名；indaramang＝帝释天/Indra，雷神） | 同上 |
| 职业 | 阿修罗（天帝）二觉 75 级技——波动剑族的高阶版 | [second growtype maximum level] 索引 9=30（系列枚举推断，见 239 §1）+ 波动印联动 + 光属性设定 |
| 学习等级 | 75 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30，索引 9） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | active（skill class 2） | 同上 [type] |
| 指令 | →←→ + Z（MP 优惠 50%/50%） | 同上 [command] / [skill command advantage] |
| CD | 40000 ms | 同上 [cool time] |
| MP | 580 → 4500 | 同上 [consume MP] |
| 特殊消耗 | 道具 3037 × 3 | 同上 [consume item] |
| 可施放状态 | 8/0/14（攻击中可取消） | 同上 [executable states] |
| 一句话效果 | 向前挥出波动剑，标记前方范围内全部敌人并各挂一枚光属性波动珠；波动珠每 0.5s 电击一次（高几率感电），持续 5s 后（或再按技能键）在每名敌人身上引发雷爆击浮 | 同上 [explain] + nut/PO 走读 |

**level info（8 列，Lv1，[level property] 全 `-1` 源直读 + 写包对位实证）**：

| 列 | 占位符 | 系数 | Lv1 值 | 语义 |
|---|---|---|---|---|
| col0 | 波动珠攻击力 `<int>`%% | 1.0 | 2288% | 写包第 1 dword → atk#27（每拍电击） |
| col1 | 波动珠持续时间 `<float1>`秒 | 0.001 | 5000 → 5.0s | 写包第 9 dword → timer1 自动引爆 |
| col2 | 多段攻击间隔 `<float1>`秒 | 0.001 | 500 → 0.5s | 写包第 8 dword → timer0 电击拍 |
| col3 | 爆炸攻击力 `<int>`%% | 1.0 | 7529% | 写包第 10 dword → 爆炸 PO atk#28 |
| col4 | 感电几率 `<int>`%% | 1.0 | 100% | 感电 4 参之一 |
| col5 | 感电Lv | 1.0 | 65 | 同上 |
| col6 | 感电持续时间 | 0.001 | 1000 → 1s | 同上 |
| col7 | 感电攻击力 `<int>` | 1.0 | 1143 | 同上（ACTIVESTATUS_LIGHTNING） |

Lv70 值：18309 / 5000 / 500 / 60239 / 100 / 120 / 1000 / 18117。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
72: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/indaramang/indaramang.nut", "swordman_indaramang", 242, 242);
 8-13: pushPassiveObj(share_po_swordman_24370.nut, 24370) + 六回调 sq_RunScript
```

- 判定主体 = 共享 PO **24370 case 242**（F7/L20 结构）：etc motion **#41** `Indaramang/explosion.ani`、atk **#27** `indaramangline.atk`（电击）/ **#28** `Indaramang.atk`（爆炸），0 基直读。
- 角色施法动画：`sq_SetCurrentAnimation(160)` → .chr etc motion **#160 = `Animation/Indaramang.ani`**（1133 行实测），带 `.als`（charge@F0/10002 + attack@F8/10001 两层）。
- **波动剑族对照（F1）**：老波动剑（20/21/22/24）共用 `wave\wave.nut` + 弹体 24328；本技**不走 wave.nut**，独立 nut + 共享 PO 24370——但保留族特征：`sqx_WaveMarkPush(obj, 1, 1)`（若挂有 `ap_wavemark` 波动印 appendage 则推 1 枚波动印，与 021 §2.2 的强化版联动同源）。

### 2.2 主 nut 逐回调（indaramang.nut，91 行；onKeyFrameFlag 段混淆已还原）

- **checkExecutableSkill**：已有在场 PO（24370/242/subType1）→ 若其 state≠10 发 state 10（**引爆**）后 return false（不耗 CD 不重施法）＝"再按技能键引爆"。无 PO 时：若有波动印 appendage → `sqx_WaveMarkPush`；进状态 242。
- **onSetState**：`sq_StopMove`；播 #160 Indaramang.ani（16 帧 1280ms）；攻速静态信息。
- **onKeyFrameFlag(obj, 1)**（**F8=640ms** 触发）：
  1. 黑闪（帧 8-15 总时长，alpha 80）；
  2. 写包 10 dword：242 / 1 / col0 / 感电4参(col4/5/6/7) / col2 / col1 / col3；
  3. `sq_SendCreatePassiveObjectPacket(24370, 0, 300, 0, 0)`——PO 出生在**身前 300px**；
  4. 在同位置放池化 `attack_wave.ani`（地面波动视觉）。
- **onEndCurrentAni**：回 STAND。

### 2.3 共享 PO 24370 case 242（标记-电击-引爆状态机）

**subType 1（波动珠母体，出生即标记）**——`setcustomdata.nut:366`：
```
atk ← #27 indaramangline.atk + col0 倍率 + ACTIVESTATUS_LIGHTNING(感电4参)
timer0 = col2(500ms, 循环)   // 电击拍
timer1 = col1(5000ms, 1次)   // 自动引爆
标记：枚举对象管理器敌人，|dx|≤300 |dy|≤90 |dz|≤50（以 PO 位为准）
      → 逐个 sq_AppendAppendage(ap_indaramang)（身上画电球）+ 压入目标表
```
- **timer0（500ms 电击拍，else.nut:709）**：对目标表内存活者逐个 `sq_SendHitObjectPacket`（atk#27：光魔 2288%/拍 + 感电判定；damage reaction **none**＝无硬直纯伤害）。
- **state 10（引爆——再按或 5s 到点，setstate.nut:8）**：对目标表存活者逐个——在其位置（z+身高/2）创建 subType 2 PO（写包：242/2/col3/感电4参/sizeRate=身高/130）；摘除电球 appendage；清空目标表。
- **procappend（94）**：目标表空（全灭）→ 母体自毁。
- **onendcurrentani（92）**：subType1 播完即毁（母体无战斗动画，纯逻辑体）。

**subType 2（雷爆，逐敌生成）**——setcustomdata 尾段：
```
anim ← etc#41 explosion.ani（12 帧 960ms，F1 flag1，攻击盒 F1-2）
atk ← #28 Indaramang.atk + col3 倍率 + 感电4参；图像/判定盒按 sizeRate 缩放
```
播完销毁。**"2 个以上波动珠相互连接"** 的电弧是纯视觉（`electric_line*.ani`，effect 目录，无判定）。

**ap_indaramang.nut（93 行，挂在敌人身上的 appendage）**：drawAppend 画电球（start→loop 两态，按目标身高/90 缩放、悬于身高一半处），proc 使电球跟随目标位置，onEnd 清理——即"波动珠跟随敌人"的视觉载体。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/Indaramang.ani`（施法） | 16 | 1280ms（均 80ms） | **F8=1（召唤）** | 无 | .als：charge@F0/10002、attack@F8/10001 |
| PO etc#41 explosion.ani（雷爆） | 12 | 960ms（均 80ms） | F1=1 | F1-2 | 无 .als；判定窗 160ms |
| effect attack_wave.ani（地面波） | 8 | 640ms | 无 | 无 | 池化 draw-only |
| effect loop_electric_ball.ani（电球循环） | 15 | 1200ms | 无 | 无 | ap 挂目标身上 |
| effect charge/attack/electric_line*/start_electric_ball* | — | — | — | — | 施法/连线/电球开场视觉（.als 2 个） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_indaramang.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_indaramang.skl` | ✅ 实测 | 8 列等级数据（§1 全解码） |
| 注册行 | swordman_load_state.nut 行 72 / 8-13 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 242 + PO 24370 |
| 主 nut | indaramang.nut | `…\pvf\sqr\character\swordman\indaramang\indaramang.nut` | ✅ 实测（91 行） | 施法/引爆分流/写包 |
| 电球 ap | ap_indaramang.nut | `…\pvf\sqr\character\swordman\indaramang\ap_indaramang.nut` | ✅ 实测（93 行） | 目标身上电球视觉 |
| PO 回调 | share_obj/swordman/ case 242 | `…\pvf\sqr\common_object\share_obj\swordman\{setcustomdata:366,setstate:8,procappend:94,onendcurrentani:92,else:709}.nut` | ✅ 实测 | 标记/电击拍/引爆 |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ 实测 | etc #41 / atk #27/#28 对位 |
| PO atk ×2 | indaramangline.atk / Indaramang.atk | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ 实测 | 电击（none 反应）/ 爆炸（down+push250+lift100） |
| .chr 条目 | etc motion #160 | `…\pvf\character\swordman\swordman.chr` 1133 行 | ✅ 实测 | `Animation/Indaramang.ani` |
| 角色 .ani | Indaramang.ani + .als | `…\pvf\character\swordman\animation\` | ✅ 实测 | 16 帧 1280ms |
| 角色 .atk | —（判定全在 PO） | `…\pvf\character\swordman\attackinfo\` | — | — |
| PO .ani | Indaramang/explosion.ani | `…\script_sqr_nut_qq506807329\swordman\animation\Indaramang\` | ✅ 实测 | 雷爆 |
| PO .ani 镜像 | indaramang/ | `…\pvf\passiveobject\character\swordman\animation\indaramang\` | ✅ 实测 | 官方部署位副本 |
| 特效 .ani | indaramang/ 12 个（attack/attack_wave/charge/electric_line×3/loop_electric_ball/loop_electric_wave/start_electric_ball×2）+ .als ×2 | `…\pvf\character\swordman\effect\animation\indaramang\` | ✅ 实测 | 全部演出视觉 |
| 装备层 | 未查 | `…\pvf\equipment\...` | 未查 | sm_body 单图集（L16） |

## 4. 资源需求

视觉集中在 `Character/Swordman/Effect/Indaramang/`（NPK：sprite_character_swordman_effect_indaramang.NPK）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `…/Indaramang/attack.img` | 同上 | 施法 .als attack 层（F8+） | **必需** | ❌ |
| `…/Indaramang/charge.img` | 同上 | 施法 .als charge 层（F0 起蓄电） | **必需** | ❌ |
| `…/Indaramang/attack_wave.img` | 同上 | 召唤地面波 | **必需** | ❌ |
| `…/Indaramang/electric_ball.img` | 同上 | 波动珠循环（目标身上） | **必需** | ❌ |
| `…/Indaramang/explosion.img` | 同上 | 雷爆 | **必需** | ❌ |
| `…/Indaramang/electric_wave.img` | 同上 | 电球下环波 | 可选 | ❌ |
| `…/Indaramang/electric_line.img`、`electric_line_start.img` | 同上 | 多珠连线电弧 | 可选 | ❌ |
| `…/Indaramang/electric_ball_start.img`、`electric_ball_start_normal.img` | 同上 | 电球开场 | 可选 | ❌ |
| sm_body0000.img | （已入库） | 角色施法动画 | 必需（共享） | ✅ |

缺失 img：必需 5 张、可选 5 张——一个 NPK 全覆盖。（AnimRes 现有 `explosionelectric02.img.bytes` 是他技资源，非本技。）

## 5. 实现方案草案

**结构映射**：施法（1280ms，F8=640ms）→ 前方 3 单位"波动珠区"（Tick 500ms × 10 拍）→ 引爆区（down/浮空终结）。

### 内容件清单

1. **`DotNet~/Skills/IndaramangSkill.cs : SkillLogic`**
   - `CooldownMs = 40000`；`TotalTimeMs = 7300`（1280 施法 + 5000 珠期 + 960 爆炸——技能托管计时器方案，同 239 §5-(b)：1280ms 后 `PlayDefaultAnim` 解控，技能空转驱动终结）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanIndaramang)`（.als 两层自动叠加）+ `ctx.ClearHitTargets()`。
   - `OnUpdate`：F8（640ms）`ctx.CreateAreaInFront(AreaIds.IndaramangOrb, 3.0)`（PO 出生 300px=3 单位）+ SubState=1；t≥5640ms（珠期完）`ctx.CreateAreaInFront(AreaIds.IndaramangBlast, 3.0)` + SubState=2。
   - **再按引爆**：不做（同 239 §8 缺口），固定 5s 自动引爆。
2. **`DotNet~/Areas/IndaramangOrbArea.cs : AreaDefinition`**（FireCircleArea Tick 范式）
   - `TotalTimeMs = 5000`、**`TickTimeMs = 500`**（10 拍）、`TickActions = { MeleeHit, AddShockBuffAction(新) }`；
   - `HalfExtents = (3.0, 0.9, 0.5)`（DNF 标记盒 |dx|≤300/|dy|≤90/|dz|≤50 折算）；
   - `HitReaction { Damage = 60, HitstunMs = 0, KnockbackX = 0, LaunchY = 0, ProcBuffId = BuffIds.Shock, ProcChance = 100 }`（atk#27 damage reaction **none**＝无硬直——HitstunMs 0 直译；感电 100% 走 ProcBuffId，L6 链路）；
   - `ViewAnimId = AnimId.IndaramangElectricBall`（电球循环动画挂在区中心——"逐敌电球"简化为"区中心单球"，§7）。
3. **`DotNet~/Areas/IndaramangBlastArea.cs : AreaDefinition`**
   - `TotalTimeMs = 300`（explosion.ani 判定窗 F1-2=160ms+余量）、`EnterActions = { MeleeHit }`、`HalfExtents = (3.0, 0.9, 1.2)`（爆炸覆盖标记区）；
   - `HitReaction { Damage = 260, HitstunMs = 800, KnockbackX = 250, LaunchY = 100 }`（atk#28 原值 down+push250+lift100+hit lift up——浮空击倒）；
   - `ViewAnimId = AnimId.IndaramangExplosion`（960ms 视觉播完自隐）。
4. **`DotNet~/Buffs/ShockBuff.cs : BuffDefinition`**（感电，BurnBuff 同构 ~15 行）：`DurationMs = 1000`、`TickTimeMs = 250`、TickActions = { ShockDamageTick（复制 BleedDamageTickAction 改名，Damage = 1143/4） }。
   - 感电 Lv65 无消费面（等级缩放延后）→ 忽略。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 施法 F8 flag1 召唤 | OnUpdate 帧 const 8 + SubState |
| 母体 PO"标记表 + 500ms 定点电击" | `IndaramangOrbArea` Tick 500ms（Tick 无去重=每拍全命中，语义同构） |
| 标记表固定敌人（后进场不吃珠） | Area 是"位置判定"非"名单判定"——后进场者也会被电（差异，§7） |
| 逐敌雷爆 PO（atk#28） | 单一引爆区覆盖标记区（判定等价；视觉差异） |
| ACTIVESTATUS_LIGHTNING 感电 | `ProcBuffId+ProcChance` + ShockBuff（L6 标准链路） |
| 再按引爆 | 不做（技能二段交互缺口，239 §8 同族） |
| 波动印 push（ap_wavemark 联动） | Buff 查询门面缺失 → 跳过 |
| 黑闪 | 闪屏延后 → 跳过 |
| 光属性 | 元素系统缺失 → 忽略 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.Indaramang = 24` |
| AreaId | `Runtime\AreaDefinition.cs` | `IndaramangOrb = 19`、`IndaramangBlast = 20` |
| BuffId | `Runtime\BuffDefinition.cs` | `BuffIds.Shock = 10` |
| ActionId | `Runtime\LSAction` | `ShockDamageTick = 13`（复制 BleedDamageTick） |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanIndaramang = 110`、`IndaramangElectricBall = 111`、`IndaramangExplosion = 112`（可选 attack_wave=113） |
| json 注册 | `LSAnimClipRegistrar.cs` | 角色 1（含 .als overlay）+ 特效 2~3 |
| 图集 | `LSAnimResComponentSystem.cs` | attack/charge/attack_wave/electric_ball/explosion 五张 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000 ms | 40000（直用） |
| 施法 | 1280ms（F8=640ms 召唤） | 帧 8 触发 |
| 电击 | 500ms/拍 × 10 拍（5s），2288% 光魔，无硬直 | Tick 500ms，Damage 60/拍，Hitstun 0 |
| 感电 | 100% / Lv65 / 1s / 1143 攻击力 | ShockBuff 100% / 1s / 4×286 |
| 标记范围 | PO 位（前 300px）±300×90×50 | HalfExtents (3.0, 0.9, 0.5) @前 3 单位 |
| 爆炸 | 7529%；down/push250/lift100 | Damage 260 / Hitstun 800 / Kb 250 / Ly 100 |
| 爆炸判定窗 | explosion.ani F1-2（160ms） | 300ms |
| 全程 | 1280 + 5000 + 960 ≈ 7.2s | 7300ms 托管 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_indaramang.skl | `.skl` 无子命令 | 本档 8 列已全解码；批量收益归 skl 立项 |
| 2 份 .atk（#27/#28） | `.atk` 无子命令 | 手抄可接受；`[damage reaction] none`（无硬直伤害）语义需在 atk 立项时保留——HitReaction.HitstunMs=0 源头 |
| 24370 obj | `.obj` 无子命令 | 本档 #41/#27/#28 对位表已给 |
| 各 .ani/.als（含 electric_line_start 等 2 个 .als） | `[use animation]`/`[add]` 常规 | ani/als 子命令全覆盖，无新节 |
| ap_indaramang.nut 的 drawAppend 电球 | 脚本直绘（非 .als 可译） | 游戏侧用区中心 ViewAnimId 替代（§5）；**"挂在敌人身上的视觉 appendage"无翻译问题、是表达方式差异** |

结论：`.skl`/`.atk`/`.obj` 族共性 3 条；ani/als 无新节缺口。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 再按技能键提前引爆（不耗 CD） | 技能二段交互门面缺失（239 §8 新记缺口） | 固定 5s 自动引爆 |
| 标记表＝施放瞬间名单（后进场敌人不被电/不被爆） | Area 是位置型非名单型（框架语义差异，非系统缺口） | 接受差异：后进场者也被电（对玩家略有利，手感无碍） |
| 感电（LIGHTNING 状态：受击附加雷伤） | ProcBuffId 链路已有（L6）；感电"受击触发加伤"精确语义无消费面 | 降级为时间型 dot（ShockBuff 250ms/跳） |
| 逐敌雷爆 + 电球逐敌跟随 | 多实例视觉（每敌一个 PO/appendage） | 单区中心球 + 单爆炸区（判定等价，视觉减配） |
| 多珠电弧连线（electric_line） | 纯视觉层 | 跳过或后补 overlay |
| 波动印 push 联动 | Buff 查询门面缺失（021 §7 同族） | 跳过 |
| 电球按目标身高缩放（/90、/130） | 对象整体缩放延后 | 固定 100% |
| 黑闪 | 闪屏延后 | 跳过 |
| 攻击中取消施放（executable states） | 技能取消体系缺失 | 站立施法 |
| 音效（无专属，未考证） | 音频延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①"一定数量的敌人"（explain）是否有珠数上限——标记代码只按范围枚举未见上限，疑 explain 描述旧版或上限在引擎侧；②timer0 `setTimeEvent(0,500,0,false)` 第 4 参 false 的重置语义（239 为 true）——按 else.nut 行为（每拍全表命中）不影响本档结论；③pvp level info 未全读；④感电 4 参在爆炸（subType2）重复写入是否叠加——两 PO 各自攻击信息独立，判定为各自独立概率。
- **缺口累计引用**：技能二段交互门面（本批 239 首报，本技第二例）；受击-状态联动加伤（感电精确语义，属性数值无伤害消费链 R1-A4 姊妹）。
- **给下轮的经验**：24370 case 的"标记名单型 PO"（出生枚举压 obj_vector + timer 定点 hit）与"圆形区域枚举型"（239 timer0）是共享 PO 两种命中范式，读 setcustomdata 开头 50 行即可分辨；`sq_SetChangeStatusIntoAttackInfo` 的 LIGHTNING 4 参顺序＝概率/等级/时长/攻击力，与 FREEZE 同构（L6）。
