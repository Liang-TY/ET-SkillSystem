# 三段斩（TripleSlash）

> 技能ID 8 | 级别 A（维持预判） | 可实现性 ✅（直接，含明确标注的实现手法借用） | 分析日期 2026-08-22 | 批次 A3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 三段斩 | `skill\Swordman\TripleSlash.skl [name]` |
| 英文名 | TripleSlash（取 skl 文件名；[name2]="断空斩" 又是中文别名，佐证轮间经验 L1） | 同上 [name2] 实测 |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5 六系可学）；强化-三段斩（TP 143）与太刀/光剑精通的 5/7 连斩扩展面向剑魂 | 同上 + TripleSlashEx.skl explain |
| 学习等级 | 15 | 同上 [required level] |
| 最高等级 | 70（各觉醒段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | →(按住状态下)+Z | 同上 [command] / [command key explain] |
| CD | 6000 ms（dungeon 固定；pvp 8000）；**连斩中断或结束后才起算**（[auto cooltime apply] 0） | 同上 [dungeon][cool time] / [auto cooltime apply] / explain |
| MP | 12 → 120（Lv1 → Lv70） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `600 -1000`（dungeon；语义未考证——600 疑与引擎版每段前冲速度/距离相关，参照脚本用的是硬编码 200px/段，见 §2.2） | 同上 [static data] |
| 一句话效果 | 滑动前冲的同时向前方敌人发出 3 连斩；连按技能指令续写下一段；强化后 5 连斩 | 同上 [explain] |

**level property（6 列，Lv1 → Lv70 首末值）**：`158→1478`、`198→2216`、`198→2216`、`47→443`、`138→1551`、`158→1773`。
列语义有**参照脚本直接印证**（jg_swordman 版按列取数，见 §2.2）：
col0/col3 = 第 1、2 段（5 连时第 1-3 段）物理攻击力 %/固定值；col1/col4 = 3 连的第 3 段（5 连的第 4 段）；col2/col5 = 5 连的第 5 段。
（[level property] 的 16 条向量与模板两组 <int> 占位逐一对应：3 连斩组用 col0/col3、col0/col3、col1/col4，5 连斩组用 col0/col3 ×3、col1/col4、col2/col5——与脚本取数完全一致。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无基础版注册行**（全文件 grep `tripleslash` 仅 2 处，均为剑影系变体）：

```
152: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/jg_swordman/swordghost_effect/tripleslash.nut", "tripleslash_swordman", 22, -1);
153: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/jg_swordman/swordghost_effect/tripleslash.nut", "tripleslashbs", STATE_TRIPLESLASH_BLADESPIRIT, -1);
```

- 三段斩属最老一代技能：**角色侧状态 22 的逻辑编译在客户端引擎内**，pvf 只提供数据文件（与 064-GoreCross 状态 64 同型）。
- 行 152 把引擎状态 22 **重绑定**到剑影（jg_swordman）"剑鬼特效"脚本；行 153 注册剑影分身版状态 138。该脚本（249 行）用技能 8 的等级数据完整重写了三段斩机制——**是本技能行为重建的权威参照**（F3 走读法第③条"兄弟职业同型脚本"，本次为 jg_swordman 而非 atswordman）。
- 引擎内置旁证：`sqr\character\swordman\swordman_header.nut` 常量 `CUSTOM_ANI_TRIPLESLASH1~5 <- 3/4/5/6/7`（实测与 .chr [etc motion] 第 973 行起 0 计数的第 3~7 项一一对应）；`swordman_common.nut:53/89` 的流心连携钩子枚举了普攻/十字斩等老状态号（未直接引用 22，状态 22 逻辑全在引擎）。

### 2.2 参照脚本逐回调（`sqr\character\jg_swordman\swordghost_effect\tripleslash.nut`，249 行实测）

**onSetState（每段进入瞬间，subState 0~4 = 第 1~5 段）**
- 播对应段动画（剑影版复用 3 套动画循环；**引擎基础版用 TripleSlash1~5.ani 各一段一套**，.ani 帧表见 §2.4）、设对应段攻击信息、`sq_StopMove`；
- 伤害取数（技能 8 等级数据）：段 1/2 → `sq_GetBonusRateWithPassive(8,-1,0)` + `sq_GetPowerWithPassive(8,-1,3)`；段 3（无强化 143）→ 列 1/列 4；有强化 143 时段 3 → 列 0/列 3、段 4 → 列 1/列 4、段 5 → 列 2/列 5；
- **方向输入**：按住左/右方向键则 `sq_SetDirection` 翻转面向（每段可反向斩）；
- **前冲目标**：记录 `当前X + 200px` 为本段终点（`slashmove`）。

**onProc（每帧）**
- 各段统一：`sq_GetUniformVelocity(当前X, 段终点X, 当前段已历时, 200)` → 匀速前冲 **200px / 200ms**（即 1000px/s），用 `sq_MoveToNearMovablePos` 带撞墙绕行；
- 剑影版另有 `setSwordGhost28Effect` 幻影特效（我们无此系统，忽略）。

**onProcCon（每帧条件，连按续段核心）**
- 无强化（level(143)<1）＝3 连：段号 ≤2 且已斩数 <3 时启用技能指令；**动画帧 ≥3（约 210ms）起**检测 `sq_IsEnterSkill(8)`（连按 →+Z）→ 立即切下一段（段号 +1，>2 回绕 0——动画按段号循环）；
- 有强化（143>0）＝5 连：同理，帧 ≥5 起，段号 0~4 循环；
- **段动画播完仍未续段 → 回待机 STATE_STAND**。

**onEndCurrentAni**
- 各段动画播完的兜底回待机（正常续段在 onProcCon 已切走，到不了这里）。

**onEndState（离开状态 22 时）**
- 恢复移动参数；**`startSkillCoolTime(8, 1, -1)`——此刻才起 CD**（与 skl [auto cooltime apply] 0、explain"中断或结束连斩后开始计算冷却时间"三方吻合）。

### 2.3 被动对象 / appendage

**无**。伤害判定走角色侧攻击信息（引擎按 .ani SET FLAG 帧施加武器判定，.ani 无攻击盒，同 064 情况）；白名单内无 `ap_tripleslash` 类脚本。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\tripleslash1.ani`（角色·段1） | 5 | 580ms（70×4+300） | **F0=1**、F3=65534 | 无（引擎施加武器判定） | 收招帧 F4 停 300ms＝续段/取消窗口 |
| `tripleslash2.ani`（段2） | 5 | 580ms | **F0=2** | 无 | 同构 |
| `tripleslash3.ani`（段3） | 5 | 580ms | **F0=3** | 无 | 3 连终段（引擎改用 TripleSlash3Down.atk，推断见 §3） |
| `tripleslash4.ani`（段4） | 5 | 580ms | **F0=4** | 无 | 5 连专用（强化 143） |
| `tripleslash5.ani`（段5） | 5 | 580ms | **F0=5** | 无 | 5 连终段（击飞收尾，atk 实测见 §5） |
| `character\swordman\effect\animation\tripleslash\slash1~5.ani` | 各 5 | 各 350ms | 无 | 无 | 挥砍弧光（引擎内置绘制、无脚本引用——064 §8.2 同型）；F0 空帧，引 `Effect/TripleSlash/01~05.img` |
| 同目录 `move1~2.ani` | 各 5 | 各 350ms | 无 | 无 | 前冲扬尘，引 `Effect/TripleSlash/move.img` |

F0 的 SET FLAG 值 1~5 即段号：引擎按 flag 值选用对应段攻击信息（.chr [etc attack info] 第 2~6 项 = TripleSlash1~5.atk、第 7 项 = TripleSlash3Down.atk，实测见 §3）。每帧含 2~4 个 damageBox（皮肤/护甲层）；仅引 `sm_body%04d.img`（帧号 2-6/189-193/195-198/11-15/200-203，全部落在已入库的单图集内，见 §4）。
`.als` 边车：基础版 5 个 .ani **均无**（实测 ls）；剑影版 tripleslash_bladespirit1~3.ani 有（本技能不消费）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | TripleSlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\TripleSlash.skl` | ✅ 实测 | 技能数据（CD/MP/6 列等级数据） |
| 注册行 | —（基础版无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut:152-153` | ⛔ 基础版缺失（引擎内置状态 22）；152/153 为剑影变体 | 见 §2.1 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（白名单内无 tripleslash 目录） | ⛔ 缺失 | 参照：`…\pvf\sqr\character\jg_swordman\swordghost_effect\tripleslash.nut`（249 行实测，注册行直接指向、C2 定点读取） |
| .chr 条目 | etc motion #3~#7 + etc attack info #2~#7 | `…\pvf\character\swordman\swordman.chr` 976-980 / 1296-1301 行 | ✅ 实测 | Animation/TripleSlash1~5.ani；AttackInfo/TripleSlash1~5.atk + TripleSlash3Down.atk |
| 角色 .ani | tripleslash1~5.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 帧表 |
| 角色 .atk | tripleslash1~5.atk + tripleslash3down.atk（共 6） | `…\pvf\character\swordman\attackinfo\` | ✅ 实测（全读） | §5 数值表；3down = 3 连终段击倒变体（命名+DNF 手感推断，引擎选用时机未考证） |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失（基础版无边车） | — |
| 挥砍特效 | slash1~5.ani + move1~2.ani（+tripleslash_ds\ 剑影版） | `…\pvf\character\swordman\effect\animation\tripleslash\` | ✅ 实测 | 引擎绘制弧光/扬尘（无引用者） |
| 装备层 | tripleslash*.ani ×608 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ 实测（只查存在性） | 各 avatar 变体图层 |
| 关联强化 | TripleSlashEx.skl（技能 143，lst 实测） | `…\pvf\skill\Swordman\TripleSlashEx.skl` | ✅ 实测（explain 已读） | 强化-三段斩：3 连→5 连（剑魂精通再→7 连）；**参照脚本 5 连开关即查 143** |
| 关联取消 | canceltripleslash.skl | `…\pvf\skill\Swordman\canceltripleslash.skl` | ✅ 实测 | 强制-三段斩（不在本批清单，记档） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img（帧号 2-6/189-193/195-198/11-15/200-203 → 单图集 sm_body0000.img 内帧索引） | sprite_character_swordman_equipment_avatar_skin.NPK | 5 段角色动画皮肤层 | **必需**（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` 已在库（bloodboom 帧号 78+ 已实证单图集约定） |
| Effect/TripleSlash/01.img ~ 05.img | sprite_character_swordman_effect_tripleslash.NPK | 5 段挥砍弧光 | 可选（角色动画本身含挥刀动作；还原时再提） | ❌ 未入库 |
| Effect/TripleSlash/move.img | 同上 | 前冲扬尘 | 可选 | ❌ 未入库 |
| （avatar 各层 sm_coat%02d%02da.img 等 ×608） | sprite_character_swordman_equipment_avatar_<层>.NPK | 装备换装图层 | 可选（demo 单层 sm_body 即可） | ❌ 未入库（不需要） |

缺失 img：必需级 **0**、可选级 6 张（同属一个 NPK，一次提取全覆盖）。img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案

### 内容件清单（全部继承真实基类；数值 DNF 原值 + demo 建议值并列）

1. **`DotNet~/Skills/TripleSlashSkill.cs : SkillLogic`**（连段状态机＝LSCast.SubState + 输入缓冲，NormalAttack 连段取消 + ReleaseWave 纯函数位移两先例合成）
   - `CooldownMs = 6000`（DNF 原值直用）；**`ManualCooldown = true`**（DNF"连斩结束才起 CD"同构——SkillLogic 已有该开关，实测存在）；`TotalTimeMs = 0`（连段长度随输入变化，自己控制）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanTripleSlash1)` + `ctx.ClearHitTargets()` + `ctx.SetAttackHitbox(前偏 0.8, 半尺寸 (0.7,0.35,0.6))`（.ani 无攻击盒 → 固定盒路径，NormalAttack 同构）+ `ctx.SetSubState(0)`。
   - `OnUpdate`（SubState 位段：十位=段起点累计 ms、个位=段号 0-4——gorecross 草案同款打包惯例）：
     - 段内前冲（ReleaseWave 纯函数同构）：`t = ElapsedMs - 段起点`，`t < 200` 时 `ctx.MoveCasterForward(2单位 × dtMs / 200)`（DNF 200px/200ms）；
     - 续段窗口（`段内 t ≥ 210ms` 即 .ani F3 起）：`ctx.PeekBufferedButton() == <本技能键>` → `ctx.ConsumeBuffer()` + 段号+1（≤2；5 连扩展时 ≤4）→ `PlayAnim(对应段 AnimId)` + `ClearHitTargets()` + 重设攻击盒 + 更新 SubState 位段；
     - 段动画播完（t ≥ 580ms）无续段输入 → 结束施放：借 `ctx.RestartCurrentSkill()` ���现（其内部 `EndNow` 结束本 cast，随后 `TryCast` 因 ManualCooldown 下 CD 已起而必然拒绝——确定性成立；语义借用见 §8 缺口上报）。
   - `OnEnd`：`ctx.DisableAttackHitbox()` + `ctx.PlayDefaultAnim()`。
   - **段间 HitReaction 切换**：SkillLogic.HitReaction 单值（LSHitboxComponentSystem.cs:110-124 实测按 `logic.HitActions/HitReaction` 固定取用）→ 段 1/2 用技能本体 `HitReaction{Damage=60, HitstunMs=500, KnockbackX=200, LaunchY=120}`（atk1/2 原值）；**段 3（终段击倒）改走一次性 Area**（见下）——AreaDefinition.HitReaction 独立，064 多相位同款手法。
2. **`DotNet~/Areas/TripleSlashFinishArea.cs : AreaDefinition`**（终段击倒，BloodBoomArea 一次性 EnterActions 范式）
   - `OnUpdate` 在切段 3 时 `ctx.CreateAreaInFront(AreaIds.TripleSlashFinish, 0.9)`；`TotalTimeMs=280`（终段活跃帧窗口）、`TickTimeMs=0`、`EnterActions={MeleeHit}`、`HalfExtents=(0.8,0.4,0.7)`、`HitReaction{Damage=90, HitstunMs=800, KnockbackX=300, LaunchY=200}`（tripleslash3down.atk 原值 down/push300/lift200）；`ViewAnimId=None`（视觉由角色动画承担）。
3. **需要新增的 Action**：无（MeleeHit 现成）。
4. **强化 143（5 连）/精通 7 连**：demo 不做（无 TP/精通系统），段数上限做成 const 便于后补。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 22 + TripleSlashN.ani | `TripleSlashSkill : SkillLogic` + 段号→AnimId 表 |
| onSetState 段循环 + onProcCon 连按（帧≥3 起） | `OnUpdate` 段内时间窗 + `PeekBufferedButton/ConsumeBuffer`（NormalAttack CancelFrame 同构） |
| onProc 匀速前冲 200px/200ms | `MoveCasterForward` 纯函数插值（ReleaseWaveSkill.OnUpdate 同构） |
| onEndState startSkillCoolTime | `ManualCooldown=true`（OnEnd 后起 CD） |
| 每段独立 .atk | 段 1/2 技能 HitReaction；终段 Area.HitReaction（064 多相位→多 Area 同款） |
| 段间可反向（方向键翻转） | 无技能中读方向输入门面 → 简化不做（§7） |
| 撞墙绕行 sq_MoveToNearMovablePos | 无地图碰撞（延后清单） → 直线位移 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.TripleSlash = 11` + `ButtonToSkill` case 7（新键） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanTripleSlash1~5`（段值接现有 48 之后顺延，与 064 草案等未实现技能统筹分配） |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×5（swordman_tripleslash1~5.json） |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 无新增（sm_body0000 已在库；特效图集后补） |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 7 |
| 翻译 | DnfConfigTranslation ani 子命令 | 5 个角色 json（现有规则全覆盖，见 §6） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 6000ms（段末起算；pvp 8000） | 6000（直用，ManualCooldown） |
| 每段时长 | 580ms（F0-F3 ×70 + F4 ×300 收招） | 580（直用） |
| 续段窗口 | F3 起（≈210ms）至段末 | 210~580ms |
| 每段前冲 | 参照脚本 200px / 200ms（=2 单位，匀速） | 2 单位 / 200ms |
| 段 1/2 命中 | atk1/2：damage 反应/push 200/lift 120 | 伤害 60/硬直 500/Kb 200/Ly 120 |
| 段 3（3 连终段） | 3down.atk：down/push 300/lift 200 | 伤害 90/硬直 800/Kb 300/Ly 200（终段 Area） |
| 5 连段 4/5 | atk4：damage/push 200/lift 120；atk5：down/push 100/lift 0 + hit lift up 击飞 | （后补强化时）90/600/Kb 100/Ly 0 |
| 伤害公式 | skl col0-2 % + col3-5 固定（Lv1：158%+47 等） | 每段固定值（demo 惯例） |
| 攻击盒 | 引擎施加（无数据） | 前偏 0.8，半尺寸 (0.7,0.35,0.6) |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `TripleSlash.skl` | `.skl` 尚无子命令（6 列 level info + static data） | 本技能手抄 6 组数值可行；建议后续加 `skl` 子命令（064 已提同议） |
| 6 个 `.atk`（tripleslash1~5 + 3down） | `.atk` 尚无子命令 | 手抄（每文件 ~7 值）可接受；随批量化提级 |
| tripleslash1~5.ani（角色） | `[SET FLAG]`（段号 1~5 + 65534） | 按既有约定跳过（段号/取消帧 const 进技能类——064 同构），非缺口 |
| slash1~5.ani / move1~2.ani（特效） | `[SHADOW]`（值 0）、`[GRAPHIC EFFECT]` | SHADOW 整节跳过无碍（064 已记）；**GRAPHIC EFFECT 实测已支持**（AniParser.cs:140 解析为 graphicEffect 1-4、游戏 json 已见 `graphicEffect:1` 字段，README"未识别节"清单滞后）——非缺口 |
| F0 空路径 `[IMAGE]` | 现有规则可处理（path="" 空白帧） | 无需改工具 |

结论：**.ani 资源全部可被现有 ani 子命令翻译**（本技能无 .als）；实质缺口为 `.skl`/`.atk` 无子命令，计 2 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 段间按方向键反向斩（onSetState sq_SetDirection） | 技能中读方向输入无门面（输入缓冲只存技能键；延后档） | demo 全程保持面向；后续 LSOpera 采集方向缓冲后补 |
| 撞墙绕行（sq_MoveToNearMovablePos） | 地图碰撞（延后档） | 直线位移，穿墙按 demo 全技能惯例 |
| 续段即时切段的精确时机（参照脚本输入帧立即切） | 已有（PeekBufferedButton 每帧轮询）——但段起点记忆需 SubState 位段打包（打包是 064 草案既有惯例，非缺口） | 直用位段打包；或"下段仍等本段 580ms 播完"再简化（手感略拖） |
| 施放中结束 cast 无专用门面 | **新缺口**：`TotalTimeMs=0` 的技能无法主动结束施放（SkillContext 仅 RestartCurrentSkill 内含 EndNow） | 借 `RestartCurrentSkill()` 收尾（ManualCooldown>0 时其内部 TryCast 必被 CD 拒绝，净效果=干净结束，确定性成立）；建议后续补 `ctx.EndCast()` 一行门面 |
| 5 连/7 连（TP 143 + 精通） | TP/精通系统（延后档） | 段数上限 const=3，后补 |
| 挥砍弧光 slash1-5 + 扬尘 move1-2（引擎内置绘制，无声明式来源） | 064 §8.2 同型（延后档） | demo 跳过；还原时手组装 overlay（releasewave 先例） |
| MP 消耗 12-120 | MP 系统（延后档） | 跳过 |
| 等级数值缩放（col0-5 六列） | 等级缩放（延后档） | demo 固定值（§5 数值表） |
| 音效 R_SQUARESWDC_HIT | 音频（延后档） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. `[static data] 600 -1000` 语义（600 疑为引擎版前冲速度/距离参数；参照脚本硬编码 200px/段，两者关系未考证）。
2. 引擎基础版的段动画与 .atk 精确映射（TripleSlash3Down 的启用时机＝"3 连终段"为命名+手感推断；.ani F0 flag=段号与 .chr etc attack info #2~#7 的对应为编号对位推断）。
3. 基础版每段前冲距离是否与参照脚本一致（参照=200px/200ms）。
4. flag 65534（tripleslash1.ani F3）语义（推断为取消窗口标记，064 同款）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **技能自结束门面**：`TotalTimeMs=0` 的自控时长技能无 `ctx.EndCast()`——连段/蓄力类技能都会撞到（本技能借 RestartCurrentSkill 规避，语义借用）。建议 SkillContext 补一行门面（实现＝暴露 LSCast.EndNow）。
2. **技能中方向输入读取**：DNF 大量位移技支持施放中改向（本技能每段反向）；输入缓冲仅存技能键。若做"位移技转向手感"需 LSOpera 采集方向按下沿 + ctx 门面。归延后档候选。

**翻译工具缺口（并入主循环汇总）**：`.skl` 子命令、`.atk` 子命令（计 2 条，与 064 重复印证）。
