# 邪光斩（GrandWave）

> 技能ID 50 | 级别 A | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 邪光斩（[name2] 同名） | `skill\Swordman\GrandWave.skl [name]/[name2]` |
| 英文名 | GrandWave（skl 文件名） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4；wave 系；"修罗邪光斩"强化技名旁证） | 同上 |
| 学习等级 | 20 | 同上 [required level] |
| 最高等级 | 70（六系各 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 0，老技能） | 同上 [type] |
| 指令 | →→ + Z | 同上 [command] |
| CD | 10000 ms（[auto cooltime apply] 0——蓄力技 CD 时机特殊，见 §7） | 同上 [dungeon][cool time] |
| MP | 65 → 616 | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `350` = **多段攻击间隔 350ms**（PO 实证 `sq_GetIntData(parentChr, 50, 0)` 作 setTimeEvent 间隔） | 同上 + setcustomdata.nut |
| 一句话效果 | 挥剑向前方发出巨大剑气，对命中的敌人多段魔法攻击 | 同上 [explain] |

**level property（2 列，Lv1 → Lv70）**：`658→7484`、`492→3228`。
nut 实证：col0 **一读两用**——`sq_GetPowerWithPassive(50,-1,0,…)` 作攻���力%（魔法），`sq_GetLevelData(50,0,…)` 作
PO 突进目标距离 px（两处都读 col 0，setcustomdata.nut case 11 实测）；col1（492→3228）本技能 nut 无消费，
**语义未考证**。模板：魔法攻击力 `<int>` / 射程 `<int>px` / 多段攻击间隔 `<float2>秒`（=static 350→0.35s）。

**关联技能（记档）**：51 修罗邪光斩（GrandWaveCharge.skl，**passive**，Lv20 起、最高 20）——习得后可蓄力，
满蓄发射强化波（§2.4）；141 前后 GrandWaveEx 为 TP 强化（另批）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
143: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/attack/grandwave.nut", "GrandWave", 27, 50);
18:  IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);
```

- **状态名 `GrandWave`、状态号 27、技能号 50**（老技能惯例：状态号≠技能号各一套）。
- nut = `sqr\character\swordman\attack\grandwave.nut`，**仅 25 行，只含施法门禁**（`checkExecutableSkill_GrandWave`：
  `sq_IsUseSkill(50)` → 推状态 27 子状态 [1,0]；`checkCommandEnable` 恒真）。**onSetState/onProc/onKeyFrameFlag 等
  回调在白名单内全部不存在**（grep 实测）——施法侧行为（播动画/创建 PO 的时机与 id）编译在引擎内，
  属 F3 引擎内置形态的变体：**有注册行但 nut 只有门禁壳**（"半引擎内置"）。
- 判定体共用 PO 24349，本技能用子 id **11**（基础波）/ 12（满蓄波）/ 13/14（光属性变体，见 §2.3）。

### 2.2 引擎内置施法行为（推断 + PO 数据反推）

施法瞬间（引擎状态 27）：
- 播施法动画：**pvf 无对应 .chr 条目**（swordman.chr 全文 grep 无 grandwave，实测）——引擎内置状态使用自身动画表，
  推断为通用挥剑动画（.chr [etc motion] #8 = `Animation/Wave.ani`，9 帧含 F0 delay 10000ms 蓄势帧 + F7 flag 65534 取消标记，
  与波动剑族共用；**未考证**）；
- 挥剑特效（引擎按惯例播放，无脚本引用）：`effect/animation/grandwave.ani`（7 帧 400ms，GrandWaveBlade.img）；
- 创建 PO 24349 写包 dword **11**（未蓄力版；蓄力版满蓄时 dword 12，蓄力过程循环播 grandwaveoncharge1/2.ani
  ——14 帧循环 910ms，OnCharge.img；满蓄爆发播 grandwavefullcharge1/2.ani 6 帧 200ms，FullCharge.img。全部实测存在）。

### 2.3 被动对象（24349 子 id 11/12/13/14，`sqr\shared_passive_object\swordman\`）

**setcustomdata case 11（基础波，实测）**：
- 动画 = [etc motion] 槽 8 = `../../character/swordman/animation/grandwave/grandwave_light_grandwave2.ani`
  （解析到 `passiveobject\character\swordman\animation\grandwave\grandwave_light_grandwave2.ani`：8 帧 × 40ms
  **循环**，无攻击盒；img = `Character/Swordman/Effect/GrandWave.img`）；
- 攻击信息 = 对象表 6 = `attackInfo/grandwave.atk`；伤害 = `sq_GetPowerWithPassive(50,-1,0)`（col0）；
- **多段**：`setTimeEvent(0, static[0]=350ms)` → 每 350ms resetHitObjectList（同 126 机制）；
- 存起点/目标：目标 = 起点 + col0 px（var grandWaveMove）。

**case 12（修罗邪光斩满蓄波）**：动画槽 9（grandwavefullcharge_light_grandwavefullcharge2.ani）；atk 7
（grandwavefullcharge.atk）；尺寸 = 技能 51 static[1]=**200%**（图像+攻击盒三重同步缩放）；
多段间隔 = 51 static[3]=**300ms**；射程 = col0 × 51 static[2]=**150%**。
**case 13/14**：light（光属性）变体——动画槽 10/11（grandwave_light/ 子目录同名系）、atk 8/9
（grandwave_light.atk / grandwavefullcharge_light.atk，[elemental property] light element），其余同 11/12。
触发条件（谁发 13/14）未考证——推断与习得/属性转换状态有关。

**procappend case 11（PO 行为核心，实测）**：
```
distance = col0 × 0.3                                  // 实际前进距离 = 射程列的 30%
每帧：x = sq_GetUniformVelocity(起点, 目标(col0), currentT, 3000)   // 3000ms 内匀速爬向全射程
越 过 起 点±(col0×0.3) → sq_SendDestroyPacketPassiveObject          // 走到 30% 射程即销毁
```
即：**剑气以 col0/3000 (px/ms) 的慢速向前爬行，走到 30% 射程处消散**——Lv1 约 197px/900ms。
存活 ~900ms ÷ 350ms 多段间隔 → **每个敌人最多约 3 跳**（"多段攻击"解释）。case 12 同构但 moveTime=51static[2]×15=2250ms。
命中反应见 §2.5。onendcurrentani：播完即毁。

### 2.4 蓄力（修罗邪光斩 51，被动强化，记档）

GrandWaveCharge.skl：static `10 200 150 300`（蓄气上限/尺寸 200%/射程加成 150%/满蓄多段间隔 300ms），
col0 40→（满蓄增加魔攻）。习得后按住技能键蓄力（oncharge 循环特效），满蓄发射 case 12 强化波；
蓄气时间上限 static[0]=10（显示 <float1> 秒，具体换算未考证）。demo 可整体跳过（§7）。

### 2.5 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| （角色施法）`Animation/Wave.ani`（推断，.chr etc #8） | 9 | 10500ms（F0 蓄势 10000） | F7=65534 | 未查 | 引擎内置状态的通用波动剑施法动画，**归属未考证** |
| `effect/animation/grandwave.ani`（挥剑特效） | 7 | 400ms | 无 | 无 | GrandWaveBlade.img |
| `effect/animation/grandwaveoncharge1/2.ani`（蓄力循环） | 14 | 910ms（loop） | 无 | 无 | OnCharge.img |
| `effect/animation/grandwavefullcharge1/2.ani`（满蓄爆发） | 6 | 200ms | 无 | 无 | FullCharge.img |
| `passiveobject\character\swordman\animation\grandwave\grandwave_light_grandwave2.ani`（PO 波体） | 8 | 320ms（loop） | 无 | **无**（同族 grandwave_light_grandwave1.ani 带全帧盒，PO 用的 2 号无盒——碰撞来源未考证，见 §8） | GrandWave.img |

命中反应（`passiveobject\unclebang_shared_passive_object\swordman\attackInfo\grandwave.atk`，实测）：
魔法 / damage reaction=damage / **push aside 300 / lift up 200** / attack direction=hit horizon / hit info=**blow** /
no blood 50 1.0 / 音效 WAVE_HIT。fullcharge 版仅多 [knuck back] 1；light 版仅元素不同（数值同）。
→ HitReaction：Damage=col0 结算、HitstunMs 常规、KnockbackX=300、LaunchY=200。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GrandWave.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GrandWave.skl` | ✅（252 行） | 技能数据 |
| 关联 .skl | GrandWaveCharge.skl（51）/ GrandWaveEx.skl | 同目录 | ✅ | 蓄力被动 / TP 强化（另批） |
| 注册行 | swordman_load_state.nut:143 / :18 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 27 注册 + PO 24349 |
| 主 nut | grandwave.nut | `…\pvf\sqr\character\swordman\attack\grandwave.nut` | ✅（25 行，**仅门禁壳**） | checkExecutableSkill 只此一职 |
| 施法回调 | —（不存在） | `…\pvf\sqr\character\swordman\` 全树 grep 无 onSetState_GrandWave | ⛔ | **引擎内置**（§2.2 推断） |
| PO 壳+逻辑 | po_swordman_shared.nut + swordman/setcustomdata.nut(case 11-14)/procappend.nut(case 11-14)/ontimeevent.nut | `…\pvf\sqr\shared_passive_object\…` | ✅ | 波体行为全量（mod 脚本补全了引擎缺位） |
| PO 定义 | swordman_shared.obj（etc motion 槽 8-11） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅ | 动画映射 |
| .chr 条目 | **无 grandwave 条目**（全文 grep 实测） | `…\pvf\character\swordman\swordman.chr` | ⛔ | 施法动画引擎内置 |
| 角色 .ani/.atk | **无**（animation/ 与 attackinfo/ 下 grep 无 grandwave，实测） | `…\pvf\character\swordman\` | ⛔ | 同上 |
| PO .atk | attackInfo/grandwave.atk / grandwavefullcharge.atk / grandwave_light.atk / grandwavefullcharge_light.atk（对象表 6/7/8/9） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\attackInfo\` | ✅ | 命中反应 |
| PO .ani | grandwave/、grandwave_light/ 两子目录共 20 文件（含 _light 变体、grandwave1/2 等） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | 波体视觉（loop） |
| 特效 .ani | grandwave.ani、grandwaveoncharge1/2.ani、grandwavefullcharge1/2.ani（effect/animation 根目录）+ _ds 系（剑影变体） | `…\pvf\character\swordman\effect\animation\` | ✅ | 挥剑/蓄力/满蓄特效（引擎按惯例播放） |
| .als 边车 | grandwave_light_grandwave2.ani.als 等（PO 动画同名边车 ×3） | 同 PO 动画目录 | ✅（未逐个细读） | 波体叠层 |
| 装备层 | grandwave*.ani ×0（avatar 下无，实测 find 计数 0） | `…\pvf\equipment\character\swordman\avatar\` | ⛔ | 老技能引擎动画，无换装层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（施法动画帧——若采用 Wave.ani，帧号未提取） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | **必需** | ✅ 已在库 |
| GrandWave.img（Effect 根） | sprite_character_swordman_effect.NPK | 波体主视觉（PO loop 动画） | **必需** | ❌ |
| GrandWaveBlade.img（Effect 根） | sprite_character_swordman_effect.NPK | 挥剑特效 | 可选 | ❌ |
| GrandWaveOnCharge.img（Effect 根） | sprite_character_swordman_effect.NPK | 蓄力循环特效 | 可选（做蓄力才需要） | ❌ |
| GrandWaveFullCharge.img（Effect 根） | sprite_character_swordman_effect.NPK | 满蓄爆发特效 | 可选（同上） | ❌ |
| PO .als 叠层引用的 img（未逐一提取清单） | 待定 | 波体叠层 | 可选 | ❌ |

缺失 img：必需 1 张、可选 3+ 张（**全部同属 sprite_character_swordman_effect.NPK，一次提取全覆盖**）。
注意 Effect 根目录直挂的 img（无子目录）→ NPK 名推导为 `sprite_character_swordman_effect.NPK`。

## 5. 实现方案草案（🔶：多段→单段/手工多段 + 慢速弹体）

1. **`DotNet~/Skills/GrandWaveSkill.cs : SkillLogic`**（WaveSwordSkill 投射物范式改慢速弹）
   - `CooldownMs = 10000`；`TotalTimeMs = 350`（施法挥手；Wave.ani 实际时长含 10000ms 蓄势帧，demo 用短动作或
     复用现有 swordman_attack1，见 §7）。
   - `OnCast`：`ctx.PlayAnim(施法动画)` + `ctx.CreateBullet(BulletIds.GrandWave)`。
2. **`DotNet~/Bullets/GrandWaveBullet.cs : BulletDefinition`**
   - `Speed = (FP)22/100`（DNF 波速 = col0 px / 3000ms → Lv1 658px/3s ≈ 0.22 单位/…换算：658px=6.58 单位/3s≈2.2 单位/s；
     demo 建议 2~3 单位/s 的"慢速爬行波"）；
   - `TotalTimeMs = 900`（存活 = 走到 30% 射程 ≈ 900ms；射程由寿命×速度表达）；
   - `DestroyOnHit = false`（穿透多目标）+ `HitActions = { MeleeHit }`；
   - `HalfExtents`：PO 无盒（§8 存疑）——参考同族 grandwave_light_grandwave1.ani 全帧盒与视觉尺寸取
     (1.5,0.5,1.5)；`SpawnOffset = (0.8, 0, 0)`；`ViewGrounded = true`；`ViewAnimId = AnimId.GrandWaveWave`（PO loop 动画）。
   - HitReaction：`{Damage=130, HitstunMs=600, KnockbackX=300, LaunchY=200}`（grandwave.atk 原值直译）。
   - **多段（🔶）**：DNF 每 350ms 对同一敌人再结算。现有 Bullet 的 HitTargets 去重=单跳。简化选项：
     a) 单跳大伤害（推荐 demo）；b) 等"多段命中"框架项（Area/Bullet 加 ResetHitIntervalMs——已在 §6.3 延后档记名）。
3. 蓄力（修罗邪光斩 51）：demo 跳过（无按住蓄力输入 + [auto cooltime apply] 0 的 CD 时机语义，见 §7）。
4. 需要新增的 Action/Buff/Area：无。

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.GrandWave = 13` + 新按键（如 N） |
| BulletId | `Runtime\BulletDefinition.cs` | `BulletIds.GrandWave = 3` |
| AnimId | npkparser `AnimConfigRegistry.cs` | `GrandWaveWave = 53`（波体 loop）；可选 `GrandWaveBlade` 等 |
| json/图集/按键/翻译 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem / DnfConfigTranslation | 波体 json + GrandWave.img.bytes + effect 图集 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000ms（auto cooltime apply 0） | 10000 |
| 波速 | col0 px / 3000ms（Lv1 658px → ≈0.22px/ms） | 2.5 单位/s |
| 波寿命 | 走到 30% 射程（Lv1 ≈900ms） | 900ms（射程≈2.3 单位） |
| 多段 | 每 350ms reset，~3 跳，每跳 col0%（658%+） | 单跳 130（或 3×45） |
| HitReaction | grandwave.atk：damage/push 300/lift 200/blow | Damage 130/Hitstun 600/Kb 300/Ly 200 |
| 满蓄版（跳过记档） | 尺寸 200%/射程 150%/间隔 300ms/魔攻+40 起 | — |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `GrandWave.skl` / `GrandWaveCharge.skl` | `.skl` 无子命令 | 手抄（并入既有 skl 子命令建议） |
| 4 个 PO `.atk` | `.atk` 无子命令 | 手抄 |
| `grandwave_light_grandwave2.ani`（PO loop） | 常规节 | 现有 ani 子命令全覆盖 |
| PO .als 边车 ×3（grandwave 系） | `[use animation]`/`[add]`（未逐个核对，抽样同族） | 现有 als 子命令预期覆盖；**提取时逐个跑一遍核对**（本文未逐文件验证） |
| `swordman_shared.obj` | `.obj` 无子命令 | 不需直译（槽 8-11 映射已手抄） |
| （蓄力侧）引擎按惯例播放的 3 组特效 ani | 常规节 | ani 子命令可译；但**无脚本引用者**（引擎硬编码挂接）——翻译后需手动挂视图（064-GoreCross "特效缺源"同条） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 施法状态引擎内置（无 onSetState/无 .chr 条目） | 行为缺声明式来源（F3 引擎内置同族） | 以 PO 数据反推的时序重建（本文 §2.2-2.3 已给全量参数）；施法动画用现有 swordman 动作占位 |
| 多段命中（每 350ms reset ~3 跳） | 多段命中（延后档） | 单跳；或 Bullet 加 ResetHitIntervalMs（框架小改，记档） |
| 蓄力（修罗邪光斩 51）：按住蓄力/满蓄强化波/蓄力特效循环 | 无按住蓄力输入 + CD 时机（auto cooltime apply 0）特殊 | demo 全跳过（基础瞬发版）；蓄力输入缓冲属输入系统扩展 |
| light 变体（case 13/14 光属性） | 元素属性系统（缺失档） | 跳过（atk 数值相同，纯属性差异） |
| 波体碰撞盒：PO 用动画无攻击盒（grandwave2.ani），同族 1 号才有 | 判定来源未考证（§8） | 自定 HalfExtents (1.5,0.5,1.5)（视觉尺寸对齐） |
| col0 一读两用（伤害% + 射程 px） | 非缺口（记档防误读） | demo 伤害/射程分开给值 |
| 音效 WAVE_HIT | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. 引擎状态 27 的施法动画归属（推断 Wave.ani，.chr etc motion #8）与创建 PO 的确切帧号/时机。
2. case 13/14（light 变体）的触发条件（谁写包 13/14）——推断与元素转换/被动状态相关，未找到写包方。
3. PO 波体碰撞来源：所用动画无攻击盒但同族 1 号动画有——可能引擎按 atk/默认盒处理，未考证。
4. col1（492→3228）语义：本技能 nut 无消费，可能为引擎侧（老技能数据冗余常见）。
5. [auto cooltime apply] 0 的精确 CD 语义（蓄力技按松开/发射起算？）未考证——demo 按施放即 CD 处理。

**新系统级缺口（§6.3 清单外）**
1. **慢速爬行多段弹体**（波速 < 角色移速、存活期内反复结算）：现有 Bullet 可表达慢速+穿透，但"存活期内对同一目标
   周期性再结算"需要 ResetHitIntervalMs 字段——与 §6.3 已列"多段命中（HitTargets 重置）"同源，本技能是
   Bullet 侧的第一个实证用例（此前 firecircle 走 Area Tick 天然多段）。建议该框架项落地时 Bullet/Area 一并加。
2. **引擎惯例特效无引用者**（挥剑/蓄力/满蓄 3 组 ani 无任何脚本/边车引用）：与 064"引擎内置绘制特效"同一缺口，
   再添一例——建议主循环汇总时统一立"特效缺源"清单。

**翻译工具缺口**：无新增节级缺口（skl/atk/obj 为既有已知；PO .als 抽样为常规节）。
