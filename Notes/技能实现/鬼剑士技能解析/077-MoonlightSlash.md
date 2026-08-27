# 月光斩（MoonlightSlash）

> 技能ID 77 | 级别 A | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 月光斩 | `MoonlightSlash.skl [name]` |
| 英文名 | MoonlightSlash（取 skl 文件名；[name2] 实测恰为英文 `Moonlight Slash`，本 pvf 少见的例外） | skl [name2] |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5 全可学，成长上限各 50；经典 DNF 里为鬼泣专属，本 pvf 已放开） | skl |
| 学习等级 | 15 | skl `[required level]` |
| 最高等级 | 70 | skl `[maximum level]` |
| 类型 | active（skill class 3） | skl `[type]` |
| 指令 | ↑→ + Z（指令施放 MP 优惠 10%/20% 档） | skl `[command]` / `[skill command advantage]` |
| CD | 4000 ms（pvp 5000 ms） | skl |
| MP | 17 → 182（Lv1 → Lv70） | skl `[dungeon][consume MP]` |
| 特殊消耗 | 无 | skl |
| 武器效果属性 | magical（魔法系；.atk 均为 dark element 暗属性） | skl `[weapon effect type]` + .atk 实测 |
| static data | `200 -500 100 -50 100`（5 值；语义未考证，推断与三段位移/方向控制相关——见 §2.2） | skl |
| 一句话效果 | 向前方发出暗属性月形斩击，追加单手上斩；按左右方向键控制移动距离；若已学[满月斩]（技能 80）则再追加双手上斩 | skl `[explain]` |

**level property（6 列，Lv1：`288 312 336 72 78 84` → Lv70：`2722 2948 3174 680 738 794`）**：
列布局推断 = (月形斩% , 单手上斩% , 双手上斩% , 月形固定值 , 单手固定值 , 双手固定值)。
依据：pvp 表 Lv1 `44 48 53 4 5 5` 恰为 3 个百分数 + 3 个小固定值的形态（dungeon 同构放大）；
模板 `月形斩击魔法攻击力 : <int>%% +<int>` ×3 组。标**推断**（高置信，无 nut 直接印证书剑侧本体）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无注册行**（grep `moonlight` 无命中）——老一代引擎内置技能（F3）。
三方印证：

① `swordman_header.nut` 常量：`CUSTOM_ANI_MOONLIGHTSLASH1 <- 61`、`CUSTOM_ANI_MOONLIGHTSLASH2 <- 62`、`CUSTOM_ANI_MOONLIGHTSLASHFULL <- 65`；
  `swordman.chr [etc motion]` 槽位实测对应（973+61=1034 行 `Animation/MoonlightSlash1.ani` ✓）；
  `[etc attack info]` 槽 54/55/56（1294+54=1348 行起 MoonlightSlash1/2/Full.atk）。

② `passiveobject\character\swordman\animation\moonlightslash\` 存在 95 级强化视觉（moonexplosion_00/01/02.ani，img 引用 `Effect/MoonlightSlashFull/95lv/`）
  + `attackinfo\moonlightslashdemoniccrownexp.atk`（恶魔皇冠系强化命中参数）——归属强化/二觉被动，非本体三段判定。

③ **兄弟职业同型脚本**：剑影 `atswordman_load_state.nut:171` 注册了重制版
  `pushState(10, "character/atswordman/2_vegabond/moonlightslash/moonlightslash.nut", "Moonlightslash", 226, 226)`
  ——495 行完整 nut 化实现（C2 定点读取实证）。与本体机制同构点：**子状态链自动推进（0→1→…，onEndCurrentAni 切下一段）**、
  **onProcCon 早期子状态内读左右方向键改写朝向/移动**、**onTimeEvent 周期 resetHitObjectList（动画时长/18 × 18 次 = 同段多目标多轮命中）**、
  每子状态独立动画 + 独立攻击信息 + 独立倍率列（sq_GetBonusRateWithPassive(226,-1,col,…)）。
  剑影版是 8 子状态的华丽重制（前冲 350px、腾空 z=200、多组屏震闪屏），本体为 2+1 段的朴素版。

### 2.2 引擎内置状态行为重建（书剑士本体，按 .ani/.atk/skl + 剑影版同构推断）

- **onSetState**：播 `MoonlightSlash1.ani`（槽 61），设攻击信息 `MoonlightSlash1.atk`，倍率取 skl 列 0/3。
- **段 1 月形斩**（620ms，攻击盒 F2-F3）：挥出暗属性月牙（视觉 = 引擎绘制 `effect/animation/moonlightslash1.ani`，引用 `slash-1.img`）；
  **onEndCurrentAni → 自动切子状态 2**（DNF 月光斩为自动连段，无需按键确认——剑影版 onEndCurrentAni 链式推进同构）。
- **段 2 单手上斩**（620ms，攻击盒 F0-F1）：`MoonlightSlash2.ani` + atk2（hit lift up 浮空）；视觉 `moonlightslash2.ani`（slash-2.img）。
- **段 3 双手上斩·满月**（550ms，攻击盒 F1-F5 连续 5 帧）：仅当已学[满月斩]（技能 80）；
  `MoonlightSlashFull.ani` + atkFull；视觉 `moonlightslashfull.ani`（fullmoon.img）。
- **方向控制**：`onProcCon` 早期子状态读 `OPTION_HOTKEY_MOVE_LEFT/RIGHT` 改写朝向变量（剑影版实证）；配合 skl static data
  `200 -500 100 -50 100`（推断：各段位移 px 与方向修正，精确语义未考证）。
- **多段命中**：同一段动画内多次 resetHitObjectList（剑影版 timeEvent 每 动画时长/18 ms 重置一次）——本体各段的命中重置节奏未考证，
  本体 .ani 无 SET FLAG（见 §2.4），推断由引擎按时器或攻击帧自动处理。

### 2.3 被动对象（强化视觉，非本体）

`passiveobject\character\swordman\animation\moonlightslash\`：
- `moonexplosion_00.ani`（25 帧 1660ms，`BackMoonN2.img`）+ `.als`（[none effect add] 挂 `_01`@帧0/z10001、`_02`@帧6/z10002）
- `moonexplosion_01.ani`（9 帧 580ms，`MoonCrackL.img`）、`moonexplosion_02.ani`（6 帧 380ms，`4thStonePiece.img`）
- PO 命中参数 `moonlightslashdemoniccrownexp.atk`：magic/dark/down/push 120/lift 300/hit wav `R_LIGHTSHOT_HIT`。

无对应 .obj/.act 实测（目录无 moonlight 条目）——PO 行为引擎内置。**归属判定**：img 路径 `Effect/MoonlightSlashFull/95lv/`
与 atk 名（demoniccrown=恶魔皇冠，鬼泣二觉被动）指向 95 级强化版/满月斩强化，归 E 批（MoonlightSlashEx=298、MoonlightSlashFull=80）参考，本体 demo 可跳过。

### 2.4 动画关键帧表（角色侧实测）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒（min/max px） | 备注 |
|---|---|---|---|---|---|
| moonlightslash1.ani | 5 | 620ms（80×4+300） | **无** | F2/F3：`-2 -15 25 170 30 120`（约 1.7×0.45×0.95 单位） | 末帧 300ms 收招；另有 [pvp] 变体 |
| moonlightslash2.ani | 5 | 620ms | 无 | F0/F1：`-19 -15 25 147 30 170` | |
| moonlightslashfull.ani | 9 | 550ms（50×8+150） | 无 | F1-F5：`-81 -30 0 237 60 174`（约 3.2×0.9×1.74 单位，满月大范围） | |
| 特效 moonlightslash1/2.ani（effect 目录） | 5/5 | 400ms | 无 | 无 | slash-1.img / slash-2.img，引擎绘制月牙斩光 |
| 特效 moonlightslashfull.ani | 7 | 350ms | 无 | 无 | fullmoon.img |

伤害盒每帧 1 个；图像全为 `sm_body%04d.img`（已入库）。`.als` 边车：角色侧无（实测）；PO 侧 1 个（见 §2.3）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MoonlightSlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MoonlightSlash.skl` | ✅（250 行） | 技能数据（6 列等级数据） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | 见 §2.1 |
| 常量表 | swordman_header.nut:231-235 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 槽位 61/62/65 |
| 参照脚本 | moonlightslash.nut | `…\pvf\sqr\character\atswordman\2_vegabond\moonlightslash\moonlightslash.nut` | ✅（495 行，C2 定点读） | 剑影重制版（技能 226），机制同构参照 |
| .chr 条目 | etc motion #61/#62/#65 + etc attack info #54/55/56 | `…\pvf\character\swordman\swordman.chr:1034/1035/1038/1348-1350` | ✅ 实测 | 三段动画与攻击信息 |
| 角色 .ani | moonlightslash1.ani / 2.ani / full.ani（+1 个 [pvp] 变体） | `…\pvf\character\swordman\animation\` | ✅ | 三段动作 |
| 角色 .atk | moonlightslash1/2/full.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 三段命中反应 |
| 特效 .ani | moonlightslash1/2/full.ani | `…\pvf\character\swordman\effect\animation\` | ✅ | 月牙斩光（引擎绘制，无脚本引用者） |
| PO .ani | moonexplosion_00/01/02.ani（+_00.als） | `…\pvf\passiveobject\character\swordman\animation\moonlightslash\` | ✅ | 95 级强化视觉（归强化系） |
| PO .atk | moonlightslashdemoniccrownexp.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 强化命中参数（恶魔皇冠系） |
| 装备层 | moonlight* ×228 | `…\pvf\equipment\character\swordman\avatar\{…}\` | ✅（find 计数） | avatar 变体（只查存在性） |
| 关联技能 | MoonlightSlashFull.skl（80，满月斩）、MoonlightSlashEx.skl（298，TP 强化） | `…\pvf\skill\Swordman\` | ✅ 存在 | 段 3 解锁前置 / E 批另行分析 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img（三段角色动画） | sprite_character_swordman_equipment_avatar_skin.NPK | 动作图集 | 必需（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` |
| slash-1.img | sprite_character_swordman_effect.NPK | 段 1 月牙斩光 | **必需**（还原核心视觉——DNF 本体月牙全靠此层） | ❌ |
| slash-2.img | sprite_character_swordman_effect.NPK | 段 2 斩光 | **必需**（同上） | ❌ |
| fullmoon.img | sprite_character_swordman_effect.NPK | 段 3 满月视觉 | **必需**（同上） | ❌ |
| BackMoonN2.img / MoonCrackL.img / 4thStonePiece.img | sprite_character_swordman_effect_moonlightslashfull_95lv.NPK | 95 级强化爆炸视觉 | 可选（归强化/E 批） | ❌ |

缺失 img：必需 3 张（同一 NPK：sprite_character_swordman_effect）+ 可选 3 张（另一 NPK）。
注：slash-1/2、fullmoon 与 `Character/Swordman/Effect/` 直下其他技能特效（gorecross 等）同 NPK，主循环批量提取时可一并处理。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/MoonlightSlashSkill.cs : SkillLogic`**（单 cast 内三段子状态机，同 WeaponComboSkill 草案模式）
   - `CooldownMs = 4000`（DNF 原值直用）；`TotalTimeMs = 0`（手动段控）；三段时长 620+620+550=1790ms。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanMoonlightSlash1)`；`ctx.ClearHitTargets()`；段表起点记录（SubState=1）。
   - `OnUpdate`（`GetElapsedMs()` + `SetSubState` 守卫）：
     - ≥620ms 且 SubState==1 → 段2：`PlayAnim(MoonlightSlash2)` + `ClearHitTargets()` + SubState=2；
     - ≥1240ms 且 SubState==2 → 段3（demo 直开——"已学满月斩"门禁无被动技能系统，简化常开）：
       `PlayAnim(MoonlightSlashFull)` + `ClearHitTargets()` + SubState=3。
   - 攻击盒：三段 json 均自带 attackBoxes（帧驱动，无需 SetAttackHitbox）；判定帧表扩 3 条目（releasewavedash 先例）。
   - 每段 HitReaction 独立（静态 readonly ×3，.atk 原值见数值表）。
   - 方向控制（左右键改向/调位移）：SkillContext 无按键状态门面（PeekBufferedButton 仅 button id）——**简化**：
     段 1 期间固定小前移 `ctx.MoveCasterForward(每帧增量)`（累计 ~0.6 单位，对应 static data 200px 量级/100）或完全跳过位移；
     手感差异：不能反身月牙，可接受。
   - `OnEnd`：`PlayDefaultAnim()`。
2. **月牙视觉**：三张特效 ani（moonlightslash1/2/full）译 json 注册 AnimId，作为技能 `PlayAnim` 同帧的 overlay。
   角色动画无 .als——手组装 overlay（releasewave 8 层手组装先例）：段开始时叠加对应特效（层 z=5 量级）。
   若接受降级可先跳过（角色挥砍动作本身完整）。
3. **无需**新 Area/Bullet/Buff/Action（无 PO 依赖：本体判定全在角色帧盒）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎内置状态 77 + 三子状态 | `MoonlightSlashSkill` 单类三段 SubState（剑影版 onEndCurrentAni 链式推进同构） |
| 三 .ani 帧盒 | 帧驱动攻击盒（json attackBoxes 直译） |
| 三 .atk（暗属性魔法反应） | 三份 static readonly HitReaction（暗属性元素系统缺失——数值不带属性，见 §7） |
| skl 6 列（%+固定×3 段） | demo 固定伤害（等级缩放延后） |
| 左右键方向控制 | 无按键状态门面（新输入面）→ 固定前移/跳过 |
| 满月斩（80）习得门禁 | 无被动技能系统 → demo 常开第三段 |
| 月牙特效（引擎绘制） | 特效 ani 译 json + 手组装 overlay |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.MoonlightSlash = 12` + ButtonToSkill 新键（如 M） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanMoonlightSlash1/2/Full = 53/54/55` + 特效 `MoonlightSlashFx1/2/Full = 56/57/58`（接 52 顺延，以 WeaponCombo 先占 49-52 计） |
| json 注册 | `…\LSAnimClipRegistrar.cs` | `RegisterOne` ×6 |
| 帧判定帧表 | LSHitboxComponentSystem | 扩 3 条目 |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | `slash-1.img.bytes`、`slash-2.img.bytes`、`fullmoon.img.bytes` |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 4000 ms | 4000（直用） |
| 总时长 | 620+620+550 = 1790 ms | 直用 |
| 段1 伤害 | 列0/3：288% + 72（Lv1，魔法） | 70 |
| 段2 伤害 | 列1/4：312% + 78 | 80 |
| 段3 伤害 | 列2/5：336% + 84 | 100 |
| 段1 反应 | atk1：damage 反应 / lift 300 / hit down | HitReaction{70, 500, Kb 0, Ly 300} |
| 段2 反应 | atk2：**down** / lift 300 / hit lift up（浮空） | {80, 600, Kb 0, Ly 300} |
| 段3 反应 | atkFull：**down** / lift 300 / push 100 / hit lift up | {100, 800, Kb 100, Ly 300} |
| 攻击盒 | F2-F3 / F0-F1 / F1-F5（见 §2.4） | 帧驱动直译 |
| 段1 前移 | static data 推断 ~200px | 0.6 单位/段1（可选） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| MoonlightSlash.skl | `.skl` 无子命令 | 手抄 6 列×70 级量大——skl 子命令收益高（同 064 建议） |
| 3 个角色 .atk | `.atk` 无子命令 | 3 文件 × 5 值手抄可接受 |
| moonexplosion_00.ani.als | 无缺口（[use animation]/[none effect add] 均已支持） | — |
| 全部 .ani | 仅常规节（FRAME/IMAGE/DELAY/DAMAGE BOX/ATTACK BOX），无 SET FLAG/无 RGBA | 现有 ani 子命令全覆盖 |

结论：**.ani/.als 资源全部可被现有子命令翻译**；实质缺口仅 .skl/.atk 两类（全局已知）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 暗属性魔法伤害 | 元素属性系统缺失 | 数值不带属性；"魔法"仅体现在伤害标尺（demo 皆固定值） |
| 左右方向键控制移动距离/反身 | SkillContext 无按键状态门面（新输入面，输入系统扩展位） | 段1 固定小前移或无位移；不做反身 |
| 满月斩（80）习得才解锁段 3 | 被动技能系统缺失 | demo 常开三段（或留 bool 开关） |
| 同段内多轮命中重置（剑影版 timeEvent 每 1/18 动画时长） | 定时自动重置延后；段间 ClearHitTargets 已落地 | 每段单次命中（满月段 F1-F5 连续帧盒天然单目标单次） |
| 月牙斩光引擎绘制（无 .als 声明） | 无声明式来源（064 缺口② 同源） | 特效 ani 手组装 overlay（本档已给方案）；最低配跳过 |
| 屏震/闪屏（剑影版大量 sq_SetMyShake/sq_flashScreen） | 延后档 | 跳过 |
| 95 级强化 PO 视觉 | 归强化系（E 批） | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. static data `200 -500 100 -50 100` 逐值语义（位移/方向修正推断）。
2. 本体（非剑影版）三段是否需要输入确认推进——按 DNF 常识与剑影版链式结构判为**自动连段**，未实证。
3. 本体各段内的命中重置节奏（引擎内置；剑影版 18 等分 timeEvent 为旁证）。
4. 6 列布局 (3 百分比 + 3 固定值) 的推断（pvp 表形态印证，无 nut 直接印证）。
5. 月牙特效与各段的精确挂接帧（引擎绘制无引用可查）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **技能中读方向键/按键状态**：月光斩（方向控制）与剑影版参照（onProcCon 读键）都依赖"施放中读按键状态"。
   现有输入面只有按钮按下沿缓冲（PeekBufferedButton 单值）。若要做"方向控制位移/蓄力/按住强化"一族技能，
   需要 SkillContext 暴露按键状态门面（方向/按住类），建议归档：输入系统扩展位（与 067 §8 缺口① 同源，可合并立项）。

**翻译工具缺口**：无新增（.skl/.atk 为全局已知项）。
