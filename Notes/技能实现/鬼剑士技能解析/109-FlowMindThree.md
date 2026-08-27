# 流心 : 升（FlowMindThree）

> 技能ID 109 | 级别 A | 可实现性 🔶（跳跃表现降级） | 分析日期 2026-08-22 | 批次 A9

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 流心 : 升 | `skill\Swordman\FlowMindThree.skl` [name] |
| 英文名 | FlowMindThree（取 skl 文件名；本例 [name2] 为英文 "Flow Heart : Upper Slash"，仍按规范用文件名） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1，L17 映射） | 同上 |
| 学习等级 | 30（前置：流心 105 Lv1） | 同上 [required level] / [pre required skill] |
| 最高等级 | 70（剑魂段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | （流心动作中）Z——只能从流心派生状态（刺/跃/架势/特定状态）中按技能键发动 | 同上 [command] `{6=(SKILL)}` + [command key explain] + `swordman_common.nut` 钩子实测 |
| CD | 无 [cool time] 节；[auto cooltime apply] 0。等级表 col1 疑似随级递减冷却（Lv1=10000ms → Lv70=5ms，**推断**，见 §2.2 佐证） | 同上 [dungeon][level info] + `[cooltime level info] 1` |
| MP | 50 → 490（Lv1→Lv70） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 一句话效果 | 向上跳跃的同时对敌人发出上挑攻击；击中浮空敌人增加攻击力（+120%）；光剑/太刀可 2 段多段攻击；巨剑减少跳跃力 | 同上 [explain] + [level property] 三元组解码 |

**level property 解码**（模板 3 个 `<int>` ↔ 3 行向量，L21 法）：

| 模板占位 | 向量 | 解读 | 值 |
|---|---|---|---|
| 普通攻击力 `<int>`% | `(-1, 0, 1.0)` | level info col0（随级） | 480% → 3944%（Lv1→Lv70） |
| 光剑/太刀多段次数 `<int>`次 | `(5, 5, 1.0)` | static[5]=2（首值非 -1 → static 槽） | 2 次 |
| 击中浮空敌人增伤率 `<int>`% | `(7, 7, 1.0)` | static[7]=120 | +120% |

static data 全列 `180 350 45 1 0 2 1 120 60 220`——除上表实证的 [5]/[7] 外，其余列（疑似跳跃力 180/350/220 三档与武器相关）引擎内部消费，**未考证**。
level info col1（10000→5 递减）：**推断为随等级缩短的冷却**——依据：①`[cooltime level info] 1` 节声明 col1 为冷却列；②`flowmindtwo.nut` 有 `sq_GetSkill(obj, 109).isInCoolTime()` 冷却检查（无冷却则此检查无意义）；③同族 107/108 的 col1 同构递减（FlowMindOne 5000→103、FlowMindTwo 6500→负值）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无 pushState**（按名、按技能号 109/110/111 反查均无命中，实测）。流心系注册行只有两条：

```
139: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/flowmind/flowmindonefallstate.nut", "FlowMindOneFallState", STATE_FLOW_MIND_ONE_FALL_STATE, 105);
142: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/weaponmaster/flowmind/flowmind.nut", "FlowMind", 61, 105);
```

- 状态 61 = 流心架势（枢纽，技能 105）；`STATE_FLOW_MIND_ONE_FALL_STATE = 147`（`swordman_header.nut:81` 实测）。
- **状态 62/63/64 = 流心:刺/跃/升（技能 107/108/109）由引擎内置注册**，pvf 无注册行——`swordman_common.nut` 钩子与 `flowmindtwo.nut` 的 `sq_AddSetStatePacket(63/64, …)` 直接按状态号切换，回调名 `onSetState_FlowMindTwo` / `onAfterSetState_swordman_flowmindthree`（引擎注册的状态名）可佐证状态名存在而 pvf 不注册。
- ⚠ **与 064-GoreCross 文档的记档冲突**：064 将 `mystate == 64` 读作"十字斩状态"。本批实测同一函数（`procAppend_Flowmind_Comminterrupt`）里 `UseSkillState(obj, 109, 64, …)` 明确把技能 109 切进状态 64——**状态 64 应为流心:升**，十字斩的引擎状态号需重查（064 的"状态号=技能号 64"是推断，此处记档供主循环裁定）。

### 2.2 施法入口（swordman_common.nut · procAppend_Flowmind_Comminterrupt，mod 改写区）

每帧钩子（growtype>1 直接 return，剑魂专属）。`mystate` 取值：0 站立 / 6 跳跃 / 8 普攻 / 29? / 61 流心架势 / 62 刺 / 63 跃 / 64 升。语义逐条：

```
z==0 且 (站立/14/63)        → EnableSoften(107, 62) + 按令切刺（地面接刺）
mystate==29 且当前动画>300ms → 同上
空中(z>30, 跳跃 400-11000ms) → EnableSoften(107, 147) 空中刺
mystate==64（升中）          → 允许取消进空中刺 147
(站立/普攻/14/62/64/29/147)  → EnableSoften(108, 63) 接跃
武器==钝器/短剑/巨剑(0/3):   → mystate∈{63,29} 或 (62 且动画>370ms) 时按 Z → UseSkillState(109, 64, [0,6,61]) 切升
其他武器:                    → mystate∈{63,62,29} 按 Z → 切升（条件更宽）
技能 109 isSealFunction()(On/Off 设置开): → 站立/普攻/14/63/62/29 可直接切升
```

另一入口（`weaponmaster\flowmind\flowmindtwo.nut` onProc，跃中）：

```
若挂有 ap_liuxing（流心:狂 buff，技能 110 产物）且按技能键且 109 不在冷却
  → sq_AddSetStatePacket(64, [0])   // 从跃中取消进升（狂 buff 解锁）
```

mod 注入版（`sqr\character\swordman\flowmind\flowmindthree.nut`，onAfterSetState_swordman_flowmindthree）：

```
进入状态 64 substate 0 时：
  若挂 ap_stateoflimit（技能 248 被动）→ 霸体 sq_SetSuperArmorUntilTime(等级数据)
  若 datas[1]==0（从跃取消进来的入口向量）→ 重推状态 64 向量 [1]（转 substate 1，推断=跳过起手直接攻击段；mod 逻辑，语义未完全考证）
```

### 2.3 引擎内置状态 64 行为重建（.ani + .atk + 常量三方印证，F3 走读法）

无角色主 nut（`weaponmaster\flowmind\flowmindthree.nut` 仅 4 行空壳，实测）。重建：

- **onSetState（推断）**：播 `FlowMindThreeReady.ani`（.chr etc motion 槽 112，`CUSTOM_ANI_FLOWMINDTHREEREADY <- 112` 常量互证）——2 帧 160ms 起跳预备；随后引擎按 static data 跳跃力把角色抛起（z 位移，**无脚本可证，未考证**）。
- **攻击段**：播 `FlowMindThreeAttack.ani`（槽 113，`CUSTOM_ANI_FLOWMINDTHREEATTACK <- 113`）；设攻击信息 `sq_SetCurrentAttackInfo(78)`（.chr etc attack info 槽 78 = `AttackInfo/FlowMindThreeAttack.atk` 实测）；伤害倍率 = level col0（480%+）；浮空敌人额外 ×(1+120%)（引擎内部判定）。
- **帧触发**：attack.ani 的 F0 自带攻击盒（见 §2.4），F0 delay=10000ms 为悬停帧（引擎在命中/落地/状态切换时推进，非真实播放 10 秒）；光剑/太刀 2 段 = 引擎在命中后重置命中表再结算一次（对应我们 L19 的段间重置语义）。
- **onEndCurrentAni（推断）**：回待机/流心架势。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\flowmindthreeready.ani` | 2 | 160ms（80×2） | 无 | 无 | 图集帧 199/200；受击盒全覆盖 |
| `character\swordman\animation\flowmindthreeattack.ani` | 3 | 名义 10021ms（**F0=10000 悬停帧**+20+1），实际由引擎推进 | 无 | **F0**：`-26 -15 -14 / 102 30 145`（min/max，x 前伸约 1 单位） | 图集帧 59/203/203 |
| 特效 `effect\animation\flowmindthree\effect_dodge.ani`（5 帧）+ `line_dodge.ani`（6 帧） | — | — | — | — | 经 attack.ani.als 挂接（见 §2.5） |

### 2.5 .als 边车（flowmindthreeattack.ani.als，实测全文）

```
[use animation]
	`../Effect/Animation/FlowMindThree/effect_dodge.ani`  `effect`
	`../Effect/Animation/FlowMindThree/line_dodge.ani`    `line`
[create draw only object]
	0	`effect`	0	1	0
[create draw only object]
	0	`line`	0	1	0
```

帧 0 创建两个 draw-only 特效对象（上挑光效 + 刃线），偏移 (0,1,0)。**`[create draw only object]` 节（非 follow parent 变体）不在 als 翻译规则表**——缺口累计在案的 `[create draw only object follow parent]`（R1-A4 里鬼）的同族变体，本批新增实证（见 §6）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMindThree.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMindThree.skl` | ✅ | 技能数据（10 列 static + 2 列等级表） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ **缺失（引擎内置状态 64）** | 见 §2.1，另记 064 文档冲突 |
| 枢纽 nut | flowmind.nut | `…\pvf\sqr\character\swordman\weaponmaster\flowmind\flowmind.nut` | ✅（43 行） | 流心架势 onProc（内含 110 触发逻辑，见 110 文档） |
| 施法钩子 | swordman_common.nut | `…\pvf\sqr\character\swordman\swordman_common.nut` | ✅（mod 改写区 C3） | procAppend_Flowmind_Comminterrupt 升入口（§2.2） |
| 主 nut | flowmindthree.nut ×2 | `…\swordman\weaponmaster\flowmind\flowmindthree.nut`（4 行空壳）/ `…\swordman\flowmind\flowmindthree.nut`（29 行 mod 钩子） | ✅ 实测 | 本体逻辑在引擎；mod 补霸体/子状态钩子 |
| 跃中取消 | flowmindtwo.nut | `…\swordman\weaponmaster\flowmind\flowmindtwo.nut` | ✅（202 行） | 跃 onProc 检查狂 buff+109 冷却 → 切状态 64（§2.2） |
| 状态常量 | swordman_header.nut | `…\pvf\sqr\character\swordman\swordman_header.nut` 274-283 行 | ✅ | CUSTOM_ANI_FLOWMINDTHREEREADY=112 / THREEATTACK=113 |
| .chr 条目 | etc motion #112/#113 + etc attack info #78 | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | FlowMindThreeReady.ani / FlowMindThreeAttack.ani / FlowMindThreeAttack.atk |
| 角色 .ani | flowmindthreeready.ani / flowmindthreeattack.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 见 §2.4 |
| .atk | flowmindthreeattack.atk | `…\pvf\character\swordman\attackinfo\flowmindthreeattack.atk` | ✅ 实测 | 物理武器伤害 / **damage reaction=down 击倒** / hit horizon / ignore weight / 无 push/lift / 音效 R_BEAMSWDA_HIT |
| .als | flowmindthreeattack.ani.als | `…\pvf\character\swordman\animation\` | ✅ 实测 | `[create draw only object]` ×2（§2.5） |
| 特效 .ani | effect_dodge.ani / line_dodge.ani | `…\pvf\character\swordman\effect\animation\flowmindthree\` | ✅ 实测 | 上挑光效/刃线（_ds 为剑影变体，跳过） |
| 被动对象 | — | `…\pvf\passiveobject\character\swordman\animation\`（grep 无） | ⛔ 无（武器判定型，无 PO） | — |
| 装备层 | flowmindthreeready.ani / flowmindthreeattack.ani 各 76 份 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,…}\` | ✅ 实测（find 计数 76+76） | 只查存在性 |
| 关联强化 | FlowMindThreeEx.skl（技能 221，TP） | `…\pvf\skill\Swordman\` | ✅ 存在 | E 类批次另行分析 |
| 关联被动 | cancelflowmind.skl / swordman_stateoflimit.skl（248） | `…\pvf\skill\Swordman\` | ✅ 存在 | 取消被动/状态限定（mod），本文记档 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集（%04d 单图集；帧 10/11/12/59/199/200/203） | 必需（共享） | ✅ 已在库（L16） |
| effect_dodge.img | sprite_character_swordman_effect_flowmindthree.NPK | 上挑主光效（5 帧） | **必需**（攻击段唯一视觉） | ❌ 未入库 |
| line_dodge.img | 同上 | 刃线拖尾（6 帧） | **必需** | ❌ 未入库 |

缺失 img：必需级 2 张（同一 NPK，一次提取全覆盖）。v2/v4 由提取时把关。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/FlowMindThreeSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发范式；独立施放版）
   - `CooldownMs = 5000`（DNF 原值随级 10000→5ms 递减，demo 取固定中值）；`TotalTimeMs = 800`（ready 160ms + attack 段 demo 640ms；原版 F0 悬停帧由引擎推进，我们取固定时长）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanFlowMindThreeReady)`；SubState=0。
   - `OnUpdate`：`ElapsedMs >= 160 && SubState == 0` → 切 `AnimId.SwordmanFlowMindThreeAttack` + SubState=1（起手→攻击段；升的"跳起"以动画表现，z 位移砍掉，见 §7）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - 攻击判定走帧驱动：attack json 的 F0 attackBox（判定帧表加该 AnimId，见注册点）。命中表跨段防重：OnCast 里 `ctx.ClearHitTargets()`（光剑 2 段多段不做，demo 单段）。
   - `HitReaction{Damage=110, HitstunMs=800, LaunchY=120}`（DNF atk：down 击倒反应、无 push/lift；击倒手感按 064 先例 = 小浮空 + 长硬直。伤害原值 level col0 = 480% 武器基数，demo 固定）。
2. **无需新 Area/Bullet/Buff/Action**——全走帧驱动武器判定 + MeleeHit。
3. 特效两层走 overlay：为 `SwordmanFlowMindThreeAttack` 手工注册 AnimOverlayConfig（entry：startFrame=0，z=+2/-1 之类，effectAnimId=两个新 AnimId）——复用 .als 机制的既有管线（RegisterOverlay），绕开 `[create draw only object]` 翻译缺口（releasewave 手组装先例）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 64（流心:升） | `FlowMindThreeSkill : SkillLogic` + 两个 AnimId |
| ready→attack 两段动画 | OnUpdate ElapsedMs 切段 + SubState 守卫 |
| 流心派生入口（common 钩子/跃中取消） | demo 简化为独立按键直发（技能取消体系缺失，§7） |
| F0 攻击盒（.ani 帧驱动） | json attackBoxes + LSHitboxComponentSystem.IsAttackDrivenAnim 白名单 |
| atk down 反应 | HitReaction（LaunchY+长硬直近似击倒） |
| .als [create draw only object] | 手工注册 overlay 配置（不依赖翻译） |
| 跳跃（static 跳跃力抛起） | 砍掉（跳跃系统缺失）——仅播动画 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.FlowMindThree = 15` + ButtonToSkill case 7（新键） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanFlowMindThreeReady = 59`、`SwordmanFlowMindThreeAttack = 60`、`FlowMindThreeEffect = 61`（effect_dodge）、`FlowMindThreeLine = 62`（line_dodge） |
| 判定帧表 | `Packages\cn.etetet.skill\Scripts\Hotfix\Share\LSHitboxComponentSystem.cs` | `IsAttackDrivenAnim` 加 `AnimId.SwordmanFlowMindThreeAttack` |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | RegisterOne ×4 + attack 动画 RegisterOverlay（手工 overlay） |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | `effect_dodge.img.bytes`、`line_dodge.img.bytes` |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 7 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000→5ms（随级递减，推断） | 5000 固定 |
| 总时长 | ready 160ms + attack 引擎推进（F0 悬停） | 800 |
| 伤害 | level col0：480%→3944% 武器魔法/物攻基数 | MeleeHit 固定 110 |
| 击倒反应 | atk：down / 无 push / 无 lift | LaunchY 120 + Hitstun 800 |
| 攻击盒 | F0 `-26,-15,-14 / 102,30,145`（min/max 像素） | 帧驱动直用（换算 1.28×0.45×1.59 单位） |
| 浮空增伤 | static[7]=+120% | 砍掉（无浮空状态查询） |
| 光剑/太刀 2 段 | static[5]=2 | 单段 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| flowmindthreeattack.ani | F0 `[DELAY] 10000` 超长悬停帧 | 已知缺口（jump.ani 同类）：翻译钳制或约定手改，本批新增实例 |
| flowmindthreeattack.ani.als | **`[create draw only object]` 节**（帧/别名/xyz 偏移，值结构似 [add]） | 新实证：与已记档的 `[create draw only object follow parent]` 同族——建议 AlsParser 按同一解析器支持两节名，输出 overlay 条目（startFrame/别名/z 用层段约定） |
| FlowMindThree.skl | `.skl` 无子命令（10 列 static + 2 列等级表 + [cooltime level info]） | 本技能手抄可行；随批量化提级（既有记档） |
| flowmindthreeattack.atk | `.atk` 无子命令 | 手抄 6 值（既有记档） |
| 特效 .ani ×2 | `[SHADOW]`（既有记档） | 跳过 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 向上跳跃 + 空中上挑（z 轴抛起） | **跳跃系统缺失**（R1-A2 累计；LSFlight 只有受击物理） | 地面版：只播起手+上挑动画，攻击盒照常帧驱动；视觉由特效承担"上升感" |
| 只能从流心派生状态发动（架势/刺/跃中取消） | **技能取消体系缺失**（064 累计）+ 状态前置缺失（R1-A1 累计） | 独立按键直发；流心连招等取消体系立项 |
| 浮空敌人增伤 +120% | 无目标浮空状态查询门面 | 砍掉（固定伤害） |
| 光剑/太刀 2 段多段 | 段内二次结算可用 ClearHitTargets 表达（L19），但伤害值差异化需 HitReaction 切换 | demo 单段 |
| 巨剑减少跳跃力 / 武器差异行为 | 换装/武器切换缺失 | 全部砍掉 |
| 蓄力版（On/Off 设置 isSealFunction） | 无技能 On/Off 设置系统 | 忽略 |
| 霸体（stateoflimit 248 联动，mod） | 霸体帧延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①static data 列 0-4/6/8/9 语义（跳跃力档位推断）；②level col1=CD 的读法为三技能族交叉推断，无引擎代码直证；③引擎状态 64 的跳跃/多段/浮空增伤实现细节（无脚本）；④mod 钩子"datas[1]==0 → 重推状态 64 [1]"的完整语义；⑤状态 29 是什么状态。
- **记档冲突上报（重要）**：`mystate == 64` 在 064-GoreCross 被读作十字斩状态、本批实证应為流心:升状态——建议主循环复核 064 的引擎状态号结论（十字斩可能另有状态号，064 的"状态号=技能 ID"推断不成立）。
- **新系统级缺口**：无新增（跳跃/取消/状态前置/浮空查询均已在案）。
- **翻译工具缺口**：`[create draw only object]` 节（新实证变体，建议与 follow parent 变体一并支持）；超长 DELAY 新实例。
- **给下轮的经验**：流心系四技能（105/107/108/109）+110 全部引擎内置（状态 61-64 引擎注册），pvf 只剩钩子与数据；走读入口固定三板斧：`swordman_common.nut` procAppend 钩子 → `weaponmaster\flowmind\flowmind.nut`（枢纽+狂触发）→ `swordman_header.nut` 274-283 行 CUSTOM_ANI 常量表对 .chr 槽位。
