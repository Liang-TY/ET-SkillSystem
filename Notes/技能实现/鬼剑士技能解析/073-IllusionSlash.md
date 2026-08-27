# 幻影剑舞（IllusionSlash）

> 技能ID 73 | 级别 A（预分类 A，维持） | 可实现性 🔶（主干多段连斩+剑气可表达；武器差异/精通联动/连打增段倚缺口简化） | 分析日期 2026-08-22 | 批次 A13

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻影剑舞 | `skill\Swordman\IllusionSlash.skl [name]` |
| 英文名 | IllusionSlash（取 skl 文件名；[name2]="Shadow swordfly" 为本 pvf 少见的英文名） | 同上 [name2] |
| 职业 | 剑魂（[skill fitness growtype]=1，L17） | 同上 |
| 学习等级 | 45（剑魂 45 级大技） | 同上 [required level] |
| 最高等级 | 70（growtype 段 1/5=50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] / [skill class] |
| 指令 | →↓→ + Z（指令施法 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 45000 ms（pvp 40000 + 开场 40000） | 同上 [cool time] |
| MP | 360 → 3024（dungeon） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无色小晶块 2 | 同上 [consume item] |
| 可施放状态 | 仅 8（普攻中） | 同上 [executable states] |
| 前置 | 技能 9（MomentarySlash 三段斩）Lv1 | 同上 [pre required skill] |
| 施法屏震 | [shake screen] 3 300（引擎数据） | 同上 |
| feature skill index | 217（×2，语义未考证，疑指向武器精通关联技） | 同上 |
| static data | `5 10 12 10 12 10 12 10 12 8 10 10 12 500 800 30 4 5 0 7 1000 -500 7 1000 -250 20 24 20 24`（29 值；pvp 版个别槽不同） | 同上 [static data] |
| 一句话效果 | 原地迅猛连续斩击，最后一击发出凌厉剑气多重伤害；斩击次数/速度/终结效果随武器与精通变化；可按上下方向键控制剑气方向、连按攻击键增加次数与威力 | 同上 [explain] |

**level info（3 列，Lv1 → Lv70）** + **level property 向量解码（L21 法，模板行实证）**：
- col0 = 斩击物理攻击力 464→3715（向量 `(-1,0,1.0)`）
- col1 = 剑气物理攻击力 min 546→5937（`(-1,1,1.0)`）
- col2 = 剑气物理攻击力 max 774→6194（`(-1,2,1.0)`）
- 基本斩击次数 = static[1]~static[2] = **10~12 次**（`(1,1)/(2,2)`）
- 剑气多段攻击数 = static[16]~static[17] = **4~5 段**（`(16,16)/(17,17)`）
- [太刀精通]/[光剑精通] Lv7 时斩击次数 = static[25]~static[26] = **20~24 次**
- 使用太刀斩击攻击力减少率 = static[21]×0.1 = **-50%**（`(21,21,0.1)`）；光剑 = static[24]×0.1 = **-25%**（`(24,24,0.1)`）
- 其余 static 槽（0/3-15/18-20/22-23/27-28：含 500/800/1000/-500/-250/7/30 等）未考证——疑为剑气飞行距离/位移偏移/方向键倾角/钝器冲击波开关等引擎参数（pvp 版 [3]/[4]=20/24、[18]=600、[24]/[25]=500/-300 与 dungeon 不同的差异佐证"武器/模式相关槽"判断）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（**引擎内置，F3b 形态**）

```
95:  IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/IllusionSlash/IllusionSlash.nut", "swordman_illusionSlash", 40, -1);
135: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/IllusionSlash/IllusionSlash.nut", "swordman_illusionSlash", 40, 73);
```

同 nut 双注册（95 行不绑技能、135 行绑技能 73），状态号 40。
**但 illusionslash.nut（127 行全读）是 mod 混淆壳**（C3 同类实证）：
- 变量名全混淆（`ey9CPt10LIj_DiIut0ExteD` 等）+ 引用 mod 资源路径 `passiveobject/script_sqr_nut_qq506807329/...`；
- 其中 `checkExecutableSkill_IllusionSlash`（106-120 行）是**本技唯一真实逻辑**：`sq_IsUseSkill(73)` → 进状态 40 subState 0；
- 其余 `onAfterSetState/onProc/onTimeEvent/onCreateObject_swordman_illusionSlash` 全部服务 **技能 248（stateoflimit，mod 自加技）**：挂 `ap_stateoflimit.nut` appendage 时按 450ms 定时生成 24370 打击体（dword 248/4）、武器子类==2（短剑）时把 20037 碰撞体替换为终结体（dword 248/5/6）——与本技无关，仅寄生在同名状态回调里。
- **无 onSetState/onKeyFrameFlag/onEndCurrentAni**——幻影剑舞本体时序**引擎内置**（2008 年代老技能，同 064 GoreCross 形态）；atswordman/jg_swordman 两参照系均无同名注册（实测 grep），走数据三方印证（§2.2-2.4）。

### 2.2 引擎内置时序重建（.ani 标记 + PO 数据印证）

- **onSetState（推断）**：播 `IllusionSlash1.ani` 起，引擎按 static 的斩击次数轮转 4 套斩击动画。
- **斩击循环**：`IllusionSlash1-4.ani`（各 7 帧 300ms）轮转——10~12 次 ≈ 3 轮（4+4+4）；精通档 20~24 次 ≈ 6 轮。每轮/每次斩击产生 `illusionslashmelee1-4.obj` 判定体（创建时机引擎内置，推断每次斩击一刀一个、或每轮一个——**未考证**）。
- **IllusionSlash1.ani F1=flag 1**：唯一 script 侧可见 flag（无消费者可查，疑首刀 PO 创建/音效）；`IllusionSlashFinal.ani F5/F8=65534`（取消标记，064 同款，语义未考证）。
- **上挑/下砸**：角色特效 `effect\animation\illusionslash\upper.ani`（4 帧 150ms，自带攻击盒 F0/F1）配 `IllusionSlashUpper.atk`（lift 300 击飞）；`smash.ani`（2 帧 160ms）配 `IllusionSlashSmash.atk`（lift 50 压地）——置于舞蹈内/终结起手（引擎接线未考证，动画名与 atk 名精确对应）。
- **终结剑气**：`IllusionSlashFinal.ani`（10 帧 940ms）→ 生成 `illusionslashwave.obj`（剑气飞行体，pass all 穿透）+ `illusionslashwave2.obj`（同动画异攻击力二连波）+ `illusionslashsub.obj`（地面冲击波层，[bottom]）。
- **onEndCurrentAni（推断）**：Final 播完回待机。

### 2.3 被动对象（.obj 五件实测）

| .obj | 结构 | atk | 说明 |
|---|---|---|---|
| `illusionslashmelee1-4.obj`（+`1ds-4ds` 剑影变体 ×4） | 单相位：basic motion `IllusionSlashMelee1-4.ani`（各 6 帧 700ms，攻击盒 F2/F3，如 F2=`-55 -35 2 219 70 135`≈274×105×133px）+ `[on end of animation]` 自毁 | `AttackInfo/IllusionSlashMelee.atk`：damage/push30/lift30/cut+blood 40/hit wav R_SQUARESWDC_HIT | 每刀斩击判定（穿透 1000） |
| `illusionslashwave.obj` | basic `IllusionSlashWave.ani`（3 帧 90ms，攻击盒 F0-F2=`-70 -40 1 225 80 194`）+ etc：WaveBottom1/2 + **mod 追加 stateoflimit 残影 ani**；[name]=`作者剑圣60`（mod 水印实证） | `IllusionSlashWave.atk`：damage/push200/lift30/blood 40/hit horizon；etc attack info=mod `stateoflimitillusionslashwave.atk` | 剑气飞行体（速度引擎内置未考证） |
| `illusionslashwave2.obj` | 同 basic motion，etc 仅 WaveBottom1/2（**无 mod 痕迹，原版形态**） | `IllusionSlashWave2.atk`：damage/push200/lift30 | 二连波第二道 |
| `illusionslashsub.obj` | basic `IllusionSlashSub/1_shockwave_dodge.ani`，[layer]=[bottom] | `IllusionSlashSub.atk`：**down**/push100/**lift250**/blow/no blood | 地面冲击波（压地判定） |

PO 行为无独立 nut（`ap_illusionslash*` 不存在，白名单实测）——全引擎内置。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\IllusionSlash1.ani`（.chr #51） | 7 | 300ms（6×50+尾帧0） | **F1=1** | 无 | 仅引 sm_body 图集；斩击动作 1 |
| `IllusionSlash2/3/4.ani`（#52/53/54） | 7 | 300ms | 无 | 无 | 斩击动作 2-4 |
| `IllusionSlashFinal.ani`（#55） | 10 | 940ms（5×50+80×3+**400**） | F5=65534、F8=65534 | 无 | 末帧 400ms 收势（L23 前摇型超长 delay 的反面：收势型） |
| `…effect\animation\illusionslash\upper.ani` | 4 | 150ms | 无 | F0/F1 | slash.img；配 Upper.atk |
| `…smash.ani` | 2 | 160ms | 无 | 无 | shot.img；配 Smash.atk |
| PO `illusionslashmelee1-4.ani`（+ .als ×4） | 6 | 700ms | 无 | F2/F3 | 斩击判定体；.als 为 **mod stateoflimit 叠层**（引用 ani 在 pvf 内缺失，跳过） |
| PO `illusionslashwave.ani`（+ .als）、`wavebottom1/2`、`wavehit`、`waveparticle` | 3 | 90/180ms | 无 | F0-F2 | 剑气本体 + 底层 + 命中特效 |
| PO `illusionslashmelee10/20/30/40.ani` | — | — | — | — | **mod 重皮版**（全引 STATEOFLIMIT.IMG；无对应 .obj，经 .als/引擎引用）——非原版资源 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | IllusionSlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\IllusionSlash.skl` | ✅ 实测（290 行） | CD/MP/static 29 值/3 列 level info/level property 模板 11 向量 |
| 注册行 | swordman_load_state.nut 行 95/135 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 40 双注册（-1/73） |
| 主 nut | illusionslash.nut | `…\pvf\sqr\character\swordman\IllusionSlash\illusionslash.nut` | ✅ 实测（127 行全读） | **mod 混淆壳**：本技仅 checkExecutableSkill 真实；余为技能 248 寄生代码 |
| .chr 条目 | etc motion #51-55（1024-1028 行）+ etc attack info #47/48（1341/1342 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 五动画 + Upper/Smash 两 atk |
| 角色 .ani | IllusionSlash1-4/Final.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 300ms×4 + 940ms |
| 角色 .atk | IllusionSlashUpper.atk / IllusionSlashSmash.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 击飞/压地 |
| 角色 .als | —（无） | 同上目录 | ⛔ 实测不存在 | — |
| 角色特效 | upper.ani / smash.ani | `…\character\swordman\effect\animation\illusionslash\`（另 `_ds` 剑影版目录） | ✅ 实测 | 上挑/下砸视觉 |
| PO 定义 | illusionslashmelee1-4(+ds).obj、wave.obj、wave2.obj、waveds.obj、sub.obj | `…\passiveobject\character\swordman\` | ✅ 实测 | 判定体五类（.obj 无子命令，结构手工读） |
| PO .ani/.als | melee1-4(+10/20/30/40)、wave、wave2(_ds)、wavebottom1/2、wavehit(_ds)、waveparticle(_ds)、sub\ 子目录 | `…\passiveobject\character\swordman\animation\` | ✅ 实测（目录 ls + 抽读） | 判定体动画与特效 |
| PO .atk | IllusionSlashMelee.atk、Wave.atk、Wave2.atk、Sub.atk（+_ds ×4、stateoflimit×1） | `…\passiveobject\character\swordman\attackinfo\` | ✅ 实测（4 份细读） | 各段命中 |
| 装备层 | IllusionSlash 系 ×380 | `…\pvf\equipment\character\swordman\avatar\{…}\*\` | ✅ 实测（find 计数 380） | 换装图层（只查存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/IllusionSlash/slash.img` | sprite_character_swordman_effect_illusionslash.NPK | 斩击刀光（melee PO 主体 + upper） | **必需** | ❌ |
| `…IllusionSlash/shot-front.img`、`shot-back.img` | 同上 | 剑气本体/底层（wave/wavebottom） | **必需** | ❌ |
| `…IllusionSlash/damage.img`、`particle.img` | 同上 | 剑气命中/粒子（wavehit/particle） | 可选 | ❌ |
| `…IllusionSlash/finish/1_shockwave_dodge.img` ~ `5_light_dodge.img`（5 张，含文件名带空格的 `2_ground_dodge .img`） | sprite_character_swordman_effect_illusionslash_finish.NPK | 终结地面冲击波（sub PO 五层） | **必需**（终结视觉主体） | ❌ |
| `Character/DemonicSwordsman/Effect/IllusionSlash/particle.img`、`shot-front.img` | sprite_character_demomicswordsman_effect_illusionslash.NPK（L14 跨职业复用） | _ds 剑影版变体 | 可选（demo 不做剑影版） | ❌ |
| `Character/Fighter/Effect/EarthBreak/floor.img` | sprite_character_fighter_effect_earthbreak.NPK | sub 地裂尘土 | 可选 | ❌ |
| `CHARACTER/SWORDMAN/EFFECT/STATEOFLIMIT/STATE_OF_LIMIT_ILLUSION.IMG` | mod 资源（stateoflimit 包） | mod 重皮 melee10-40 | 跳过（非原版） | — |
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色帧 | 必需（共享） | ✅ 已入库 |

**结论**：必需 8 张（slash + shot-front/back + finish 五张），分属 2 个 NPK；可选 8 张。AnimConfigRegistry 无相关 AnimId（实测 grep）。

## 5. 实现方案草案

- **内容件清单**（多段连段 L19 已通 + 剑气弹体，全部先例内）：
  - `IllusionSlashSkill : SkillLogic`（同三段斩/里鬼连段子状态机范式，L19 段间重置）：`CooldownMs=45000`、`TotalTimeMs=4600`（12 刀×300 + Final 940 + 余量）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanIllusionSlash1)`、`ctx.ClearHitTargets()`、SubState=0（刀计数）。
    - `OnUpdate`：每刀切点（300ms 网格）→ `ctx.PlayAnim(轮转 1→2→3→4)` + `ctx.SetAttackHitbox(前偏 1.1, 半尺寸 (1.4,0.5,0.7))`（melee F2 盒折算）+ 本刀 Hit（HitReaction：伤 60/硬直 250/Kb 30/Ly 30，IllusionSlashMelee.atk 原值——**快斩短硬直**）+ `ctx.ClearHitTargets()`（段间重置）+ `ctx.DisableAttackHitbox()`；第 10 刀改为 Upper 参数（Ly 300 击飞，IllusionSlashUpper.atk）。
    - 末刀后（3600ms）：`ctx.PlayAnim(AnimId.SwordmanIllusionSlashFinal)` + `ctx.CreateAreaInFront(AreaIds.IllusionSlashSmash, 1.0)`（下砸压地，Smash.atk：伤 100/硬直 500/Kb 30/Ly 50）+ SubState=1。
    - Final F0 后 ≈ 250ms：`ctx.CreateBullet(BulletIds.IllusionSlashWave)` ×2 道（第二道延迟 200ms，对应 wave/wave2 二连）。
    - 连打增段（可选增强）：`ctx.PeekBufferedButton()==攻击键` 时刀计数上限 +6（输入缓冲已落地）；武器差异不做（§7）。
  - `IllusionSlashSmashArea : AreaDefinition`：`TotalTimeMs=160`、`EnterActions={MeleeHit}`、`HalfExtents=(1.5,0.6,1.0)`、`ViewAnimId=AnimId.IllusionSlashSmash`（smash.ani json）。
  - `IllusionSlashWaveBullet : BulletDefinition`（复制 `NormalWaveBullet` 改参数）：`Speed=10`（原值未考证，取弹体默认）、`TotalTimeMs=1500`、`DestroyOnHit=false`、`HalfExtents=(1.5,0.6,1.0)`（wave 盒 295×120×194px 折算）、`HitActions={MeleeHit}`、`HitReaction{Damage=120, HitstunMs=400, KnockbackX=200, LaunchY=30}`（Wave.atk 原值）、`ViewAnimId=AnimId.IllusionSlashWave`。剑气 4~5 段多段（DNF 原版）→ demo 简化单段/目标（L19 穿透重置档：Bullet 需 ResetHitIntervalMs 字段——延后）。
  - 终结冲击波 `IllusionSlashSubArea`（可选）：`TotalTimeMs=300`、`HalfExtents=(2.0,0.8,1.0)`、HitReaction down/push100/lift250、`ViewAnimId=sub\1_shockwave_dodge`。
  - 无新 Action/Buff。
- **概念映射**：引擎轮转斩击 → OnUpdate 时间网格 + PlayAnim 轮转（SubState=刀计数）；melee PO → SetAttackHitbox 帧驱动（01§5.6-6 冲刺盒同构）；wave PO → BulletDefinition（WaveSword 直系同类——本技即"剑魂版波动剑风暴"）；65534 → 忽略。
- **注册点**：SkillIds 加 `IllusionSlash=21`；AnimIds 加 `SwordmanIllusionSlash1=91`、`…2/3/4=92/93/94`、`SwordmanIllusionSlashFinal=95`、`IllusionSlashUpper=96`、`IllusionSlashSmash=97`、`IllusionSlashWave=98`；AreaIds `IllusionSlashSmash=12`（Sub 可选 13）；BulletIds `IllusionSlashWave=5`；json 注册 ×8+；BuildAtlas illusionslash(+finish) 图集；新按键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000ms | 45000（直用） |
| 斩击次数 | 基本 10~12（static[1]/[2]）；精通 20~24（static[25]/[26]） | 12 固定（连打 +6 可选） |
| 每刀节奏 | 300ms/刀（动画实测） | 300ms 网格 |
| 斩击命中 | Melee.atk：damage/push30/lift30（col0 464=46.4%/刀） | 伤 60/硬直 250/Kb 30/Ly 30 |
| 上挑 | Upper.atk：push30/**lift300**/hit lift up | 第 10 刀 Ly 300 |
| 下砸 | Smash.atk：push30/lift50/hit down | 伤 100/硬直 500/Kb 30/Ly 50 |
| 剑气 | Wave.atk：push200/lift30；col1-2 546~774（54.6%~77.4%）；多段 4~5 | 弹体伤 120×2 道；穿透单段 |
| 冲击波 | Sub.atk：down/push100/lift250 | 可选：伤 80/Kb 100/Ly 250 |
| 总时长 | 12×300+940 ≈ 4540ms | TotalTimeMs 4600 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| IllusionSlash1-4/Final.ani 等 5+8 份（实测节名枚举） | `[INTERPOLATION]`（4 处）、`[SHADOW]`（3 处）、`[SET FLAG]`/`[DAMAGE TYPE]`/`[IMAGE RATE]`（2 处） | 已知跳过清单；[INTERPOLATION] 为 026 首记新缺口，建议 README 补记 |
| 同上 | `[GRAPHIC EFFECT]`（13 处）、`[LOOP]`、`[RGBA]` | 已支持（L15/README），非缺口 |
| `illusionslashmelee1-4.ani.als` | 引用的 `StateofLimit/…` 特效 ani **在 pvf 内不存在**（mod 寄生 .als） | 翻译时按缺文件处理（als 子命令会报路径缺失——验证工具行为）；游戏侧跳过该 overlay 即可 |
| `IllusionSlash.skl` | `.skl` 无子命令；本文件 29 值 static + 3 列 level info + 11 向量 | 手抄可行（§1 已全解码）；批量化时建议 skl 子命令 |
| 5 份 `.obj` + 8 份 `.atk` | `.obj`/`.atk` 无子命令 | 既有缺口；本技能手抄可行 |

**结论**：ani/als 主体可译；新记 1 类风险（.als 引用缺失文件——mod 寄生行）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 武器类型差异化（短剑/太刀/巨剑/光剑/钝器五分支：次数/间隔/剑气有无/冲击波） | **武器类型差异化**（缺失，R2-A6 流心:刺首撞） | demo 固定"太刀/光剑型"（12 刀 + 双波）；钝器冲击波可后补 |
| 武器精通 Lv7 联动（次数翻倍 + 减伤） | 被动系统 + 属性数值消费链（缺失） | 固定基本档 12 刀 |
| 连按攻击键增加斩击次数/剑气威力 | 输入缓冲已有（PeekBufferedButton）——**无缺口** | §5 可选增强（+6 刀） |
| 上下方向键控制剑气方向 | **技能中方向输入读取**（缺失，R1-A3 已上报） | 剑气固定向前 |
| 剑气 4~5 段多段（每目标多次） | 穿透重置多段（L19 档三：Bullet 加 ResetHitIntervalMs 字段） | 单段/目标；两道波=两次命中 |
| 施法屏震 [shake screen] 3 300 | 屏震延后 | 跳过 |
| mod 重皮资源（STATEOFLIMIT.IMG、melee10-40、stateoflimit atk/als） | 非缺口（mod 产物，非原版） | 全部跳过，按原版资源走 |
| 引擎内置时序（melee PO 创建节奏） | 无脚本可证（**未考证**） | §5 用 300ms 网格近似（与动画时长自洽） |

## 8. 存疑与缺口上报

- **未考证**：①melee PO 的创建节奏（每刀/每轮一个，引擎内置）；②static 29 值中 16 个未解槽（§1 列举，含 500/800/1000/-500/-250）；③剑气飞行速度/射程（引擎内置，.obj 无数据）；④feature skill index 217 的指向；⑤flag 1（IllusionSlash1 F1）与 65534（Final F5/F8）消费者；⑥`_ds` 系（DemonicSwordsman 前缀 img + ds obj/atk）确认为剑影版变体（064 同构），非本职业技能本体。
- **新缺口上报**：**.als 引用缺失文件**（melee1-4.ani.als 引用 StateofLimit 路径 ani，pvf 内不存在）——建议 DnfConfigTranslation 的 als 子命令对缺文件 use-animation 行降级为告警+跳过而非报错，游戏侧 overlay 按别名缺失处理（L15 消费侧已有"别名不映射即跳过"惯例，工具侧需对齐）。
- **系统级缺口复证**：武器类型差异化（第二例，流心:刺后）；技能中方向输入读取（第二例）；穿透重置多段（Bullet 字段）。
- **给下轮**：`IllusionSlash\illusionslash.nut` 是 mod 混淆壳的**完整样本**（C3 新增实例：不只 attack/flowmind，老技能 nut 也可能整文件被 mod 寄生）——load_state 有注册但 nut 无本技回调时，先扫文件内 qq506807329/stateoflimit 痕迹再下"引擎内置"结论。
