# 鬼斩 : 炼狱（swordman_slashofhell）

> 技能ID 238 | 级别 A | 可实现性 🔶（主干可实现：前斩 + 冥界刀刃延迟僵直区 + 延迟爆炸；黑暗闪屏/头顶刀刃视觉等表现降级） | 分析日期 2026-08-22 | 批次 A14

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼斩 : 炼狱 | `skill\Swordman\swordman_slashofhell.skl` [name] |
| 英文名 | swordman_slashofhell（取 skl 文件名；本 skl 无 [name2] 节，实测） | 同上 |
| 职业 | 鬼泣（[second growtype maximum level] 第 5/6 位=30/30 → growtype 2 鬼泣一觉/二觉档，87/233 两技能三方互证；鬼斩系=鬼泣常识） | 同上 |
| 学习等级 | 60（前置：技能 237 Lv1——鬼斩系强化，未考证具体名） | 同上 [required level] / [pre required skill] |
| 最高等级 | 40（二觉后上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ←↓→ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 30000 ms（固定；pvp 起手 CD 200000） | 同上 [dungeon][cool time] / [pvp][start cool time] |
| MP | 400 → 1120（Lv1→40） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×1（[consume item] 3037 1 1） | 同上 |
| 可施放状态 | 8 / 0 / 14 | 同上 [executable states] |
| static data | **无 [static data] 节**（实测） | 同上 |
| 一句话效果 | 劈开大地召唤冥界刀刃，暗属性魔法伤害；被刀刃击中的敌人强制僵直一段时间 | 同上 [explain] |

**level property（5 列，Lv1 → Lv40 首末值）**：col0 斩击% `2414→19319`、col1 冥界刀刃% `3523→28188`、col2 爆炸% `6278→50225`、
col3 僵直时间 `2000ms（恒，×0.001=2.0 秒）`、col4 攻击次数上限 `2 次（恒）`（pvp 版 500ms/1 次）。
（向量 5 条全为 -1 源 level 列，与模板 5 行逐行对应，实测无歧义。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 56（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/slashofhell/slashofhell.nut", "swordman_slashofhell", 238, 238);
// swordman_header.nut 行 70/98/325/486（实测）：STATE/SKILL_SWORDMAN_SLASHOFHELL <- 238
//   CUSTOM_ANI_SWORDMAN_SLASHOFHELL <- 155；CUSTOM_ATTACK_SWORDMAN_SLASHOFHELL <- 95
```

单子状态、单段技能；伤害分三层：角色挥斩（atk 95）→ 冥界刀刃多段僵直（共享 PO 24370，引擎内置）→ 延迟爆点（ap 帧表 + ChangeSkillEffect）。

### 2.2 主 nut 逐回调（slashofhell.nut，95 行；mod 混淆变量名 C3 同族）

- **onSetState**：`sq_StopMove` → 播动画 155（8 帧 810ms）→ 设攻击信息 95 + 倍率 col0（斩击%）→
  `sq_flashScreen(黑, 100/800/100, α153)`（**周围陷入黑暗**，闪屏延后档）→ 攻速静态缩放（延后）。
- **onKeyFrameFlag flag=1**（实测 F1，390ms 处）：
  ```
  写包（238, col1 冥界刀刃%, col2 爆炸%, col3 僵直 2000, col4 次数 2）
  → sq_SendCreatePassiveObjectPacket(24370, 0, 370, 0, 0)     // 身前 370px 共享打击 PO（L20）
  ```
  —— 冥界刀刃的判定/僵直/2 次上限计数全部由共享 PO 引擎侧按写包执行（pvf 无对应 nut/obj，073 类老 PO 无数据文件可查）。
- **getScrollBasisPos**（镜头基准位回调）：帧 0 期间镜头沿面向加速前推 300px（`sq_SetCameraScrollPosition`）——
  施法瞬间镜头前拉的演出（延后档：无镜头控制）。
- **onEndCurrentAni**：回 STAND。

### 2.3 被动对象 / appendage

**ap_slashofhell.nut（92 行，僵直受害者的"头顶刀刃+延迟爆点"管理 appendage；由共享 PO 引擎侧命中后挂载——本 nut 内无挂载方）**：
- **onStart**：按受害者身高缩放加载 `slashofhell_debuff_roof_dodge.ani`（÷180 比例 + setImageRate）；
- **drawAppend**（每帧渲染回调）：把刀刃动画画在受害者头顶（`sq_AnimationProc + sq_drawCurrentFrame`，y 上移身高/2-10）——
  **Buff 视觉挂接缺口的又一实例（R1-A5 记档；本技能头顶刀刃是标志性视觉）**；
- **proc**：appendage 向量表按 4 元组轮询（起始时刻, 延迟, 目标 group, 目标 unique）；到点 →
  `sq_SendChangeSkillEffectPacket(受害者, 238)`（触发受害者身上的延迟爆点演出，引擎侧执行）并清零该组；全部处理完 → 失效。

**冥界刀刃/爆点的视觉层**在 mod 注入目录 `passiveobject\script_sqr_nut_qq506807329\swordman\animation\slashofhell\`
（C2 定点读取；qq506807329 水印目录，C3 同源）：`slashofhell_att_eff_1~27.ani` + 镜像 `_r_1~27` + `boom_dodge/normal` +
`debuff_roof_dodge/normal`，共 57 个 .ani + 3 个 .als（27/27/boom 三层链，[none effect add] 全帧挂接）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒（min/max 口径） | 备注 |
|---|---|---|---|---|---|
| character\…\slashofhell.ani（槽155） | 8 | 810ms（F0=320，F1-7=70） | **F1=1**（创建刀刃 PO） | F2：x∈[8,153] z∈[-9,100]；F3：x∈[-2,213] z∈[-9,215]；F4：x∈[-7,240] z∈[-13,212] | 挥斩三帧判定 |
| effect\…\slashofhell_casting_1.ani | 6 | — | 无 | 无 | 施法暗幕（zig_start_casting.img） |
| casting_2 / casting_3 / casting_4 | 8/8/10 | — | 无 | 无 | 斩击弧光×2 + 刀光（.als 挂接，见下） |
| mod 目录 att_eff 系 54 个 + boom + roof | — | — | 无 | 无 | 刀刃雨视觉/爆点/头顶刀刃（纯视觉） |

`.als` 边车：**角色动画有**（slashofhell.ani.als，实测）——`[none effect add]` casting_1@F2/层10001、casting_4@F0/层10002，
**`[create draw only object]`** casting_2 (3,0,2,0)、casting_3 (3,0,3,0)（R1-A4/R2-A10 已记档的该节无 follow-parent 变体，第 3 实证）。
mod 目录另有 3 个 .als（boom_dodge/normal、debuff_roof_dodge 挂 normal 版）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_slashofhell.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_slashofhell.skl` | ✅ | 技能数据（5 列） |
| 注册行 | load_state 行 56（238/238） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 70/98/325/486 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画 155/攻击信息 95 |
| 主 nut | slashofhell.nut（95 行） | `…\pvf\sqr\character\swordman\slashofhell\slashofhell.nut` | ✅ | §2.2 |
| ap nut | ap_slashofhell.nut（92 行） | `…\pvf\sqr\character\swordman\slashofhell\ap_slashofhell.nut` | ✅ | 头顶刀刃 + 延迟爆点帧表 |
| 共享 PO | share_po_swordman_24370.nut 注册行 8 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅（引擎内置，L20） | 冥界刀刃伤害体 |
| .chr 条目 | etc motion #155（行 1128）；etc attack info #95（行 1389） | `…\pvf\character\swordman\swordman.chr` | ✅ | SlashOfHell.ani/.atk |
| 角色 .ani | slashofhell.ani | `…\pvf\character\swordman\animation\slashofhell.ani` | ✅ | 8 帧 810ms |
| 角色 .als | slashofhell.ani.als | 同上 | ✅ | 暗幕/弧光/刀光挂接 |
| 角色 .atk | slashofhell.atk | `…\pvf\character\swordman\attackinfo\slashofhell.atk` | ✅ | magic/暗/down/推150/浮200 |
| 特效 .ani | slashofhell_casting_1~4 | `…\pvf\character\swordman\effect\animation\slashofhell\` | ✅ | 施法视觉 |
| 刀刃视觉 | att_eff 系 54 个 + boom/roof 4 个 + 3 .als | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\animation\slashofhell\`（C2 定点） | ✅ | 冥界刀刃雨/爆点/头顶刀刃 |
| 装备层 | *slashofhell*.ani ×76 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |

slashofhell.atk 实测关键值：magic / **暗属性** / damage reaction **down** / push aside 150 / lift up 200 / blow / no blood 20 / R_SHOCKWAVE_HIT。

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画 | 必需（共享） | ✅ 已在库 |
| slash_dodge.img / slash_dodge2.img | sprite_character_swordman_effect_slashofhell.NPK | 施法弧光（casting_2/3） | 必需 | ❌ |
| sw_dodge.img | 同上 | 刀光（casting_4） | 必需 | ❌ |
| zig_start_casting.img | sprite_character_swordman_effect_zig.NPK | 暗幕（casting_1，Effect/zig 子目录） | 可选（黑屏闪屏延后则弱化） | ❌ |
| sw_att_normal / sw_att_front_dodge / sw_att_back_dodge（3 张） | sprite_character_swordman_effect_slashofhell.NPK | 刀刃雨主体（att_eff 系 72 处引用） | 必需（简版可选 1 张） | ❌ |
| floor_normal / floor_dodge | 同上 | 地面暗蚀（28 处引用） | 可选 | ❌ |
| sw_boom_dodge.img | 同上 | 刀刃爆点 | 必需 | ❌ |
| boom_normal / boom_dodge | 同上 | 终结爆炸 | 必需 | ❌ |
| debuff_roof_normal / debuff_roof_dodge | 同上 | 头顶刀刃（僵直标记） | 可选（有 StunBuff 红闪即可） | ❌ |
| zig_start_floor.img | sprite_character_swordman_effect_zig.NPK | 地面暗蚀底层 | 可选 | ❌ |

缺失 img：必需级 9 张（slashofhell NPK 一次提取全覆盖）、可选级 5 张（+zig NPK）。img 版本红线由提取时把关。

## 5. 实现方案草案

### 内容件清单（全部继承真实基类；数值 DNF 原值 + demo 建议值并列）

1. **`DotNet~/Skills/SlashOfHellSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发范式）
   - `CooldownMs=30000`、`TotalTimeMs=810`（动画全长）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanSlashOfHell)` + `ctx.ClearHitTargets()`（json 自带 F2-F4 攻击盒 → 帧驱动自动激活，无需 SetAttackHitbox）。
   - OnUpdate：`ctx.GetElapsedMs() >= 390 && GetSubState()==0`（F1 旗标时刻）→
     `ctx.CreateAreaInFront(AreaIds.SlashOfHellBlade, 3.7)`（身前 370px）+ `ctx.SetSubState(1)`；
     挥斩本体伤害走技能 `HitReaction`（MeleeHit 在帧盒命中时结算）。
   - OnEnd：`ctx.PlayDefaultAnim()`。
   - `HitReaction{Damage=90, HitstunMs=500, KnockbackX=150, LaunchY=200}`（atk 95 原值 down/push150/lift200 → 击倒手感同 releasewave 范式）。
2. **`DotNet~/Areas/SlashOfHellBladeArea.cs : AreaDefinition`**（冥界刀刃：持续僵直区，同 FireCircleArea Tick 范式）
   - `TotalTimeMs=1200`（覆盖刀刃雨视觉节奏；DNF 僵直 2000ms 由 Buff 承担见下）、`TickTimeMs=550`（≈2 tick，对应"攻击次数上限 2 次"）、
     `HalfExtents=(1.8,0.5,1.8)`（370px 前方刀刃落区）、`EnterActions={MeleeHit, AddStunBuff}`、`TickActions={MeleeHit, AddStunBuff}`；
   - `HitReaction{Damage=70, HitstunMs=400, KnockbackX=0, LaunchY=0, ProcBuffId=BuffIds.Stun, ProcChance=100}`——
     **强制僵直 2 秒 = 现成 StunBuff 直译**（StunBuff 现值 1s 定身，实现期把该场景 Buff 时长调 2000ms 或新建 HellHoldBuff）；
   - `ViewAnimId=AnimId.SlashOfHellBlade`（att_eff 选层手组装循环）。
   - 多段上限 2 次的精确计数（DNF col4）：TickActions 无次数上限参数（L19 同段定时档）——550ms×2 tick 近似 2 次命中，差值可接受。
3. **`DotNet~/Areas/SlashOfHellBoomArea.cs : AreaDefinition`**（延迟爆点，同 FireCircleEnd 收尾范式）
   - 刀刃区 OnEnd/到时后由技能 OnUpdate 在 `ElapsedMs >= 1590`（390+1200）`CreateAreaInFront(AreaIds.SlashOfHellBoom, 3.7)` 二段创建；
     `TotalTimeMs=400`、`EnterActions={MeleeHit}`、`HitReaction{Damage=150, HitstunMs=800, KnockbackX=150, LaunchY=200}`（col2 爆炸段）、
     `ViewAnimId=AnimId.SlashOfHellBoom`（boom_normal 层）。
   - DNF 的"每个受害者各自延迟爆点"（ap 帧表逐目标计时）统一简化为区域整体到时一次爆（视觉差异：无逐人错峰）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 238 + slashofhell.ani | `SlashOfHellSkill` + `AnimId.SwordmanSlashOfHell` |
| F1 旗标 → 共享 PO 24370@370px | `OnUpdate` 时刻触发 + `CreateAreaInFront(3.7)`（L20 引擎 PO → Area 重建） |
| 挥斩帧盒 F2-F4 + atk 95 | json 帧驱动攻击盒 + 技能 `HitReaction` |
| 冥界刀刃强制僵直 2s/上限 2 次 | `Area Tick + StunBuff`（ProcBuffId 通道，L6 同构） |
| 延迟爆点（ap 逐目标计时） | 第二个 Area 按总时序创建（简化） |
| 黑暗闪屏 / 镜头前拉 | 延后（跳过） |
| 头顶刀刃视觉（drawAppend） | 延后（StunBuff 受击表现替代） |

### 注册点清单（草案号段，A14 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.SlashOfHell=22` + ButtonToSkill 新键 |
| AreaId | `Packages\cn.etetet.skill\Runtime\AreaDefinition.cs` | SlashOfHellBlade=14、SlashOfHellBoom=15 |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | SwordmanSlashOfHell=99、SlashOfHellBlade=100、SlashOfHellBoom=101、（可选 casting_1/4 overlay=102/103） |
| json | `…\LSAnim\LSAnimClipRegistrar.cs` | RegisterOne ×3~5 |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 必需 7 张（sw_att_normal 等） |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000ms | 30000（直用） |
| 总时长 | 810ms（8 帧） | 810 |
| 挥斩触发 | F2-F4（390-600ms） | json 帧驱动 |
| 刀刃区创建 | F1=390ms，身前 370px | ElapsedMs 390 + 3.7 单位 |
| 斩击伤害 | col0 2414%（atk down/push150/lift200/暗） | 90/硬直 500/推 150/浮 200 |
| 刀刃伤害 | col1 3523% × 2 次 | 70×2 tick |
| 强制僵直 | col3 2000ms | StunBuff 2000ms |
| 爆炸伤害 | col2 6278% | 150/硬直 800 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| slashofhell.ani.als | `[create draw only object]`（无 follow-parent 变体，值 3/`casting_2`/0/2/0） | **已记档缺口**（R1-A4 里鬼首报、R2-A10 再证，本技能第 3 实证）——建议 als 子命令按 [add] 同构输出独立 overlay 条目（startFrame+独立播放标记） |
| slashofhell.ani 的 [SET FLAG]/[PLAY SOUND]、atk 的 [force hit stun time] 等 | 整节跳过惯例 / .atk 无子命令 | 触发帧 const 进技能类（惯例）；.atk 手抄并入既有缺口 |
| swordman_slashofhell.skl（5 列） | `.skl` 无子命令 | 手抄 5 值可接受；并入既有缺口 |
| mod 目录 57 个 .ani + 3 .als | 节面常规（[none effect add] 已支持） | 现有 ani/als 子命令全覆盖 |

计 3 条既有缺口（.skl/.atk/[create draw only object]），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 冥界刀刃判定/2 次计数（共享 PO 24370 引擎侧） | 引擎内置无数据（L20） | Area Tick×2 近似（§5） |
| 逐受害者延迟爆点（ap 4 元组计时） | 无逐目标延迟任务队列 | 区域整体到时一次爆 |
| 头顶刀刃视觉（drawAppend 随身高缩放） | Buff 视觉挂接（缺失，R1-A5） | StunBuff 表现替代；后续补 Buff 视图通道 |
| 黑暗闪屏（flashScreen α153） | 闪屏（延后） | 跳过 |
| 镜头前拉 300px（getScrollBasisPos） | 无镜头控制（延后） | 跳过 |
| 攻速缩放 | 动画速度门面（延后） | 固定速度 |
| 暗属性伤害 | 元素属性系统（缺失） | 无属性直伤 |
| 精确"2 次上限"参数化 | AreaDefinition 无 Tick 次数上限字段（延后档，L19） | TickTimeMs×TotalTimeMs 凑 2 tick |

## 8. 存疑与缺口上报

**未考证项**
1. 共享 PO 24370 对写包 (238, col1, col2, 2000, 2) 的判定盒尺寸/命中间隔（引擎内置，pvf 无数据）。
2. ap_slashofhell 的挂载方（本 nut 无挂载代码；推断共享 PO 引擎侧命中后挂载）与 4 元组延迟的语义（逐目标爆点间隔值未读到初始写入处）。
3. 前置技能 237 的名称与作用（未读该 skl）。
4. [create draw only object] 首参 `3` 的语义（R1-A4 记档为"类型/ID"，精确含义未解）。

**系统级缺口（非新增，实证补充）**
- Buff 视觉挂接（drawAppend）：第 3 实证（R1-A5 光环类、本技能头顶刀刃类）——按宿主属性（身高）缩放的 Buff 附属视觉，建议 BuffDefinition 视图通道立项时纳入"缩放锚点"参数。
- 翻译工具 `[create draw only object]`：第 3 实证，建议随 als 子命令补齐（§6）。

**给下轮的经验**：`swordman_` 前缀的 60 级二觉系技能（233/238 同代）都是"完整 nut + 共享 PO 24370 承担远程判定"的两层结构——
角色 nut 只管动画/输入/镜头，伤害细节在写包参数里；分析时先把写包 dword 顺序对照 level property 列即可还原全部数值。
