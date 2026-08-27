# 后跳斩（BackStepCutter）

> 技能ID 49 | 级别 B（状态前置派生攻击） | 可实现性 🔶（简化直发版全部现有件可表达；原版"后跳中按 X"前置依赖状态前置型缺口，169-BACKSTEP 已记） | 分析日期 2026-08-22 | 批次 B2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 后跳斩 | `skill\Swordman\BackStepCutter.skl` [name] |
| 英文名 | BackStepCutter（取 skl 文件名；[name2]="Back Step Cutter" 是真英文） | 同上 [name2] 实测 |
| 职业 | 共通/剑魂/剑影可学（[growtype maximum level] `1 1 0 0 0 1`：鬼泣/狂战/阿修罗 0 级；[skill fitness growtype]=1） | 同上 |
| 学习等级 | 15 | 同上 [required level] |
| 最高等级 | 1（一次性） | 同上 [maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | **（后跳动作中）X**——攻击键派生，非独立技能键 | 同上 [command] `{6=(ATTACK)}` / [command key explain] |
| CD | 2000 ms | 同上 [dungeon][cool time] |
| MP | 3 → 4（仅 1 级两段值） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `100`（1 值，语义未考证；疑攻击倍率或位移参数） | 同上 [static data] |
| level property | **无**（max level 1，无 [level info]） | 同上实测 |
| 一句话效果 | 施放[后跳]的同时向敌人发出斩击（但无法使用[空中连斩]） | 同上 [explain] |

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能无独立注册行**（load_state grep `backstepcutter` 零命中）。它与后跳（169，状态 7）共用同一条注册：

```
114: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/BackStep/BackStep.nut", "swordman_backstep", 7, -1);
```

- 状态号 7 = 后跳（L2 老技能状态号表）；第 5 参 -1 = 不绑定特定技能。
- **技能 49 进入状态 7 的直接证据**：`backstep.nut`（mod 混淆壳，C6④）的 `onAfterSetState_swordman_backstep` 读
  `sq_GetVectorData(datas, 0) == 49` 才走分支——即引擎在后跳斩施放时把状态 7 连同 vector[0]=49（技能 ID）一起推入。
  （169-BACKSTEP §2.2 曾把 49 读作"技能 248 的编排序列号"，本文按"vector[0]=技能 ID"解读更自然——后跳本体 vec 为其他值不触发该分支；存疑见 §8。）
- mod 分支行为（非原版，记档不还原）：若挂有 ap_stateoflimit（剑神二觉被动，技能 248 `swordman_stateoflimit.skl`，lst 实测在册）→
  写包(248, 1, bonusRate) → 在自身位置创建共享 PO 24370（L20）追加打击。

**原版后跳斩逻辑完全引擎内置**（F3）：白名单内 grep `backstepcutter` 于 `sqr\character\swordman\` 全树、
attack.nut 隐藏函数组（F4 套路）、jg_swordman/atswordman 树——全部零命中。

### 2.2 引擎内置行为重建（.ani 标记 + 注册链 + 169 互证）

```
处于后跳（状态 7）动画期间按 X：
  技能 49 可用判定（CD 2000）→ 后跳斩
  斩击动作复用后跳动画 backjump.ani（无独立 backstepcutter.ani——animation 目录 ls 实测）
  F5（帧起 ~200ms，40×5 前摇后）：SET FLAG 1 —— 武器攻击判定窗口（推断：引擎按 flag 施加武器判定，
      同 gorecross 无 ATTACK BOX 先例；命中参数无专属 .atk，引擎默认武器结算）
  F6（380ms）：SET FLAG 2 —— 收招/落地事件（未考证）
  后跳位移本身（-600px/s 水平小跳）已在状态 7 内由引擎处理（169 已析）
  动画播完回待机 state 0
```

**无专属 .atk**：`attackinfo\` ls grep `back|jump` 仅 hitback/jumpattack/jumpattack_bladespirit——后跳斩命中参数引擎内置。
旁证可用：`frenzy1.atk`（damage -19/push 30/lift 30/damage 反应）——其动画 Frenzy1.ani 与 backjump.ani
**共用同一批 sm_body 帧 188-193**（实测两侧 python 提取），同帧资产的最近命中参数参照。

### 2.3 被动对象 / appendage

- `ap_backstep.nut`（63 行）：空脚手架（169 已读全文，无逻辑）。
- 二觉联动特效（mod 侧）：`passiveobject\character\swordman\animation\state_of_limit_backstep_00/01/02.ani`
  （5f/5f/3f 各 150ms；00/01 引 particle.img、02 引 state_of_limit_backstep.img；01 有 .als 双层叠加）——
  属技能 248 的追加打击视觉，本技能不依赖。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 受击盒 | 备注 |
|---|---|---|---|---|---|---|
| `character\swordman\animation\backjump.ani`（.chr etc 槽 203 = CUSTOM_ANI_BACK_JUMP，header:373 实证） | 7（F0-6） | 380ms（40×5/80/100，169 实测） | F0-F4=0，**F5=1，F6=2** | 无（引擎施加武器判定） | **0/7 帧（全程无敌帧，169 实测）** | 仅引 sm_body 帧模板（188-193） |
| `back_jump_start/run/end.ani`（etc 槽 245/246/247 = CUSTOM_ANI_SWORD_BACK_JUMP_*，header:415-417 实证） | 3+2+1 | 225/300/50ms | 无 | 无 | 未验 | 疑前冲后跳/分段变体（169 §8 已记疑点） |

`.als` 边车：无（animation 目录 ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BackStepCutter.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BackStepCutter.skl` | ✅ 实测（72 行） | 数值（CD/指令） |
| 注册行 | load_state:114（共用） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 7（后跳系共用） |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（全树 grep 零命中） | ⛔ 缺失（引擎内置） | 角色逻辑在引擎 |
| 后跳状态 nut | backstep.nut + ap_backstep.nut | `…\pvf\sqr\character\swordman\backstep\` | ✅ 实测（mod 混淆壳 + 空壳） | 169 已析；vec[0]==49 分支为本技能存在性佐证 |
| .chr 条目 | backjump.ani = etc 槽 203；back_jump_* = 245/246/247 | `…\pvf\character\swordman\swordman.chr` 1176/1218-1220 行 + header:373/415-417 | ✅ 实测 | 动画注册 |
| 角色 .ani | backjump.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 角色 .atk | —（无专属） | `…\pvf\character\swordman\attackinfo\`（ls grep 实测） | ⛔ 无 | 引擎默认武器结算；frenzy1.atk 可作参照 |
| .als | — | animation 目录 | ⛔ 无边车 | — |
| 二觉联动 PO 特效 | state_of_limit_backstep_00/01/02.ani（+01.als） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 技能 248 追加打击视觉（非本技能核心） |
| 装备层 | backjump.ani ×N | `…\pvf\equipment\character\swordman\avatar\` | ✅（169 已验 belt_a/coat_a 命中） | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 188-193） | sprite_character_swordman_equipment_avatar_skin.NPK | 后跳斩动作 7 帧 | 必需 | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` 已在库（L16） |
| state_of_limit_backstep.img / particle.img | sprite_passiveobject_character_swordman_animation 系 | 二觉联动打击视觉 | 可选（mod/248 内容，不提取） | ❌ 未入库（不提取） |

缺失 img：**0**（必需级零缺口；可选级 2 张属技能 248 内容不提取）。

## 5. 实现方案草案

**简化直发版（🔶）**——explain 原文即"施放[后跳]的同时向敌人发出斩击"：直发版=后跳位移+斩击，与原版行为高度重合，仅缺"必须处于后跳中按 X"的输入前置：

- **内容件清单**：
  - `DotNet~/Skills/BackStepCutterSkill.cs : SkillLogic`——同 `ReleaseWaveSkill` 位移范式 + 帧触发守卫：
    - `CooldownMs = 2000`（DNF 原值直用）；`TotalTimeMs = 380`（backjump.ani 直用）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanBackJump)`（**复用 169 预留的 104 号**，动画同一份）+ `ctx.ClearHitTargets()`。
    - `OnUpdate`：① 位移纯函数（169 §5 同构）——`min(ElapsedMs,380)/380 × 2.28 单位` 帧差增量 `ctx.MoveCasterForward(-增量)`（负值=背向）；
      ② `CurrentFrameIndex() >= 5 && GetSubState()==0`（对应 F5 flag1）→ `ctx.SetAttackHitbox(前偏 0.8, 半尺寸 (0.7,0.3,0.6))` +
      命中走 `HitActions={MeleeHit}`（参照 frenzy1.atk：Damage 50/HitstunMs 500/KnockbackX 30/LaunchY 30）+ `ctx.SetSubState(1)`；
      ③ `CurrentFrameIndex() >= 6` → `ctx.DisableAttackHitbox()`。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - 无需新 Area/Buff/Bullet/Action（MeleeHit 现成）。
- **概念映射**：状态 7 内按 X → 直发按键触发（状态前置型缺口，见 §7）；引擎武器判定帧 → `SetAttackHitbox` 固定盒
  （gorecross 同惯例：.ani 无攻击盒的引擎判定技能走固定盒路径）；引擎默认命中 → frenzy1.atk 参照值。
- **注册点**：`SkillIds.BackStepCutter = 27`（ButtonToSkill 新键）；AnimId 复用 `SwordmanBackJump = 104`（169 预留，零新增）；
  LSAnimClipRegistrar/BuildAtlas 零增量（sm_body 已在库）。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 2000ms | 2000 直用 |
| 总时长 | 380ms（7 帧） | 380 直用 |
| 位移 | 后跳 -600px/s（169 level col1，推断） | 6 单位/s 背向 × 380ms ≈ 2.28 单位 |
| 斩击触发 | F5（~280ms）flag 1 | 帧 5 + SubState 守卫 |
| 命中参数 | 引擎默认（无 .atk；frenzy1.atk 参照 -19/push30/lift30） | Damage 50/Hitstun 500/Kb 30/Ly 30 |
| 无敌 | 全程 380ms（0 受击盒帧，169 实测） | 依赖 169 §8 受击盒空帧语义修正（~3 行使能项） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `BackStepCutter.skl` | `.skl` 无子命令（本技能仅 3 个数值，手抄零成本） | `skl` 子命令同前议（重复印证） |
| `backjump.ani` | `[SET FLAG]`（0×5/1/2） | 既有约定整节跳过（触发帧 const 进技能类）——非缺口 |
| state_of_limit_backstep_01.als | `[add]` 双层叠加 | 现有 als 子命令可译（不提取，记档即可） |

结论：**ani/als 资源全部可被现有子命令翻译**；实质缺口仅 `.skl` 子命令（1 条，重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 后跳动作中按 X 派生（状态前置） | **缺失档：状态前置型技能**（R1-A1 已记，后跳中施放同族；技能取消体系姊妹项） | 直发版：按技能键=后跳位移+斩击一体（行为≈原版，仅输入前置不同） |
| 全程无敌帧 | 受击盒空帧语义（169 §8 已给 ~3 行使能修正，非系统级） | 未修正前发"位移躲判定"版；修正后升格 |
| 小跳弧线（z 轴） | 跳跃系统（缺失档，R1-A2） | 平移版后跳（169 同款简化） |
| 武器命中参数（引擎内置） | 无 .atk 数据源 | frenzy1.atk 参照值（同帧资产旁证） |
| mod 二觉联动打击（248） | mod 内容 + 跨技能取消体系 | 不实现 |
| "无法使用空中连斩"互斥 | 技能互斥/空中连斩不存在 | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. `[static data] 100` 语义（攻击倍率/位移参数猜测）。
2. F5 flag1/F6 flag2 的精确语义（推断 flag1=武器判定窗口；169 读作落地/收招事件）。
3. `backstep.nut` vec[0]==49 的两种读法（本文=技能 ID 49；169=技能 248 序列号 49）——行为等价（都触发 248 联动），归属不同；按 169 §2.2 与本文 §2.1 并读。
4. back_jump_start/run/end 三件套与本技能的挂接关系（169 §8 已记）。
5. 引擎内置武器判定的实际命中盒尺寸（无数据源，demo 固定盒）。

**新系统级缺口**：无新增（状态前置型/受击盒空帧语义/跳跃系统均已在档；本技能是"状态前置型缺口"的**第 3 例消费方**——连突刺/银光落刃之后）。

**翻译工具缺口**：`.skl` 子命令（重复印证，无新增节）。

**给下轮的经验**：后跳系技能（169 后跳 / 49 后跳斩 / 后跳取消类）共用 backjump.ani（etc 槽 203）与状态 7——查这族先读 169-BACKSTEP.md，动画/无敌帧/位移结论可全部复用；后跳斩的斩击帧就是同一份动画的 F5/F6。
