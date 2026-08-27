# 上挑（UpperSlash）

> 技能ID 46 | 级别 A | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 上挑 | `UpperSlash.skl [name]` |
| 英文名 | UpperSlash（本 pvf 少见的 [name2]=`Upper Slash` 真英文；文档名仍取 skl 文件名） | 同上 [name2] |
| 职业 | 鬼剑士共通（growtype 0-5，Lv1 起手技） | 同上 |
| 学习等级 | 1 | 同上 [required level] |
| 最高等级 | 50 | 同上 [maximum level]（level info 实际 50 档，口径一致） |
| 类型 | active（skill class 1）；[auto cooltime apply] 1 | 同上 |
| 指令 | Z | 同上 [command] / [command key explain] |
| CD | 2000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 6 → 40（Lv1 → Lv50） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 武器效果类型 | physical | 同上 |
| static data | `100 -250 100 -50`（语义未考证，引擎内置消费） | 同上 [static data] |
| 一句话效果 | 从下段往上挑的攻击，使敌人浮空；攻击全程有霸体判定 | 同上 [explain] |

**level property（4 列，Lv1 → Lv50）**：col0 `120→840`、col1 `350→1167`、col2 `100(恒)`、col3 `30→210`。
**四列里三列有脚本实证**（剑鬼版 `attack.nut onSetState_upperslash_swordman`，见 §2.2）：
- col0 = 攻击倍率%（`sq_GetBonusRateWithPassive(46,-1,0,…)`）；
- col1 = **浮空力**（`sq_GetLevelData(46,1,…)` → `sq_SetCurrentAttacknUpForce`）；
- col3 = 固定物攻加成（`sq_GetPowerWithPassive(46,-1,3,…)` → `sq_SetCurrentAttackPower`）；
- col2 恒 100 未考证（推断为浮空力显示比率的基准值，模板 `浮空力比率 : <float1>%%`）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**基础版（技能 46）在 `swordman_load_state.nut` 中无注册行**（实测；唯一命中是 149 行被**注释掉的**剑鬼版注册）：

```
//IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/attack/attack.nut", "upperslash_swordman", STATE_UPPERSLASH_BLADESPIRIT, -1);
```

但**函数实体仍在白名单内**：`sqr\character\swordman\attack\attack.nut` 340-407 行保存着 `onSetState_upperslash_swordman / onEndCurrentAni_upperslash_swordman / onAttack_upperslash_swordman` 完整函数组（剑鬼版实现，机制同构基础版），是本技能行为重建的权威参照。

角色侧常量（`swordman_header.nut`）：`CUSTOM_ANI_UP_ATTACK <- 202`（基础版动作）、`CUSTOM_ANI_UPPERSLASHAFTER <- 16`（追加段）、`CUSTOM_ATTACK_INFO_UPPERSLASH_BLADESPIRIT <- 148`（剑鬼版攻击信息）。白名单 grep 实证：前两者除定义外**无任何 nut 引用**——基础版引擎内置消费。

### 2.2 引擎内置状态行为重建（.ani 标记 + .atk + attack.nut 函数组三方印证）

**onSetState（施法瞬间）——剑鬼版函数组逐行实证**（`attack.nut onSetState_upperslash_swordman`）：

```
子状态 0：
  sq_SetCurrentAnimation(272)                          // 剑鬼版动画；基础版引擎播 etc 槽 202 的 up_Attack.ani
  sq_SetStaticSpeedInfo(攻击速度…)                     // 攻速加成
  sq_SetCurrentAttackInfo(148)                         // 剑鬼版；基础版用 etc atk 槽 11 UpperSlash.atk
  damage      = sq_GetBonusRateWithPassive(46, -1, 0)  // skl col0 攻击倍率%
  damageBonus = sq_GetPowerWithPassive(46, -1, 3)      // skl col3 固定物攻 → sq_SetCurrentAttackPower
  upForce     = sq_GetLevelData(46, 1)                 // skl col1 浮空力
  sq_SetCurrentAttacknBackForce(pAttack, 100)          // 后拉力 100（挑空时向后拖）
  sq_SetCurrentAttacknUpForce(pAttack, upForce)        // 上挑力 = col1（350→1167）
  sq_SetCurrentAttackDirection(pAttack, ATTACK_DIRECTION_UP)   // 方向=向上击飞
  sq_StopMove()
onEndCurrentAni → STATE_STAND（回待机）
onAttack → isSwordSaber 时播放剑鬼剑气特效（视觉分支）
```

**关键机制结论**：上挑的浮空不靠 .atk 数值，而是**脚本把 skl col1 写进攻击信息的 UpForce + 方向 UP**——我们侧精确对应 `HitReaction.LaunchY`（+KbX 拿 BackForce=100）。

**基础版帧标记（up_Attack.ani 实测，etc 槽 202 = .chr 1175 行 `Animation/up_Attack.ani`）**

| 帧号 | 累计时间 | 标记/数据 | 语义 |
|---|---|---|---|
| F0-F8 | 全 9 帧 | **DAMAGE TYPE = SUPERARMOR** | 全程霸体（explain"攻击时有霸体判定"实证） |
| F2 | 100ms | **flag 1** + **ATTACK BOX** `-32 -13 0 233 26 141` | 命中帧：挑击判定盒（min/max 口径：x∈[-32,233] 前伸 2.3 单位、z 至 141 高位——上挑盒形） |
| F3 | 150ms | **ATTACK BOX** `15 -13 1 178 26 189` | 第二判定帧（盒前移收窄、z 至 189 更高） |
| F6 | 450ms | **flag 65534** | 取消窗口（GoreCross 同型惯例） |

**.als 边车（up_attack.ani.als 实测，基础版唯一边车）**：

```
[use animation]
	`../Effect/Animation/upperslash1.ani`	`sub2`    // 上挑刀光（4 帧 200ms）
[none effect add]
	2	1000	`sub2`                     // 帧 2、层 1000 挂刀光——与 F2 命中帧精确同步
```

（`[none effect add]` 在**官方文件**出现——此前只在 mod 文件（releasewave）见过，工具已同等支持，无缺口。）

**追加攻击段（UpperSlashAfter，引擎内置，触发条件未考证）**：`upperslashafter.ani`（etc 槽 16，5 帧 500ms，无盒无 flag）+ `UpperSlashAfter.atk`（etc atk 槽 20：物理/down 反应/hit lift up，与主段同向）+ 特效 `upperslash2.ani`（5 帧 250ms）。按 DNF 玩法惯例推断为**上挑动作中再次输入技能指令追加一击**（同样击飞方向）；基础版引擎内置无法取证，实现按此推断建模。

### 2.3 被动对象 / appendage

无（passiveobject 白名单内无 upperslash 相关 .obj；剑鬼版剑气特效走 onAttack 即时动画，无判定体）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/up_attack.ani`（角色） | 9（F0-8） | 550ms（F0-7=50，F8=150） | F2=1，F6=65534 | F2、F3（见 §2.2 数值） | **全帧 SUPERARMOR**；含 `[SPECTRUM]` 残影配置（term150/life100/白 80%/LINEARDODGE）；仅引 `sm_body%04d.img` |
| `character/swordman/animation/upperslashafter.ani`（追加段） | 5 | 500ms（5×100） | 无 | 无 | 追加一击动作 |
| `character/swordman/effect/animation/upperslash1.ani`（刀光） | 4 | 200ms | 无 | 无 | `upperslash.img`；经 .als 在 F2 挂层 1000 |
| `character/swordman/effect/animation/upperslash2.ani`（追加段刀光） | 5 | 250ms | 无 | 无 | `upperslash.img`；挂接关系引擎内置（无引用者，推断伴追加段） |
| `character/swordman/effect/animation/atupperslash\*`（Lv95 被动强化系） | — | — | — | — | 含 lv95passive 子目录，属强化批，本文记档不展开 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | UpperSlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\UpperSlash.skl` | ✅ 实测 | 技能数据（CD2000/4 列等级数据） |
| lst 条目 | ID 46 | `…\pvf\skill\swordmanskill.lst` 13-14 行 | ✅ 实测 | — |
| 注册行 | —（无 pushState；149 行剑鬼版注册被注释） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ **缺失（引擎内置状态）** | 见 §2.1 |
| 参照 nut | attack.nut 340-407 行函数组 | `…\pvf\sqr\character\swordman\attack\attack.nut` | ✅ 实测 | upperslash_swordman 三回调（剑鬼版） |
| 常量 | swordman_header.nut 186/372/539 行 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_UP_ATTACK=202 等 |
| .chr 条目 | etc motion #202（up_Attack.ani，1175 行）+ #16（UpperSlashAfter.ani，989 行）；etc attack info #11（1305 行）+ #20（1314 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 动作与攻击信息映射 |
| 角色 .ani | up_attack.ani / upperslashafter.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 9 帧 550ms / 5 帧 500ms |
| .als | up_attack.ani.als | `…\pvf\character\swordman\animation\up_attack.ani.als` | ✅ 实测 | F2 挂刀光 upperslash1 |
| 角色 .atk | upperslash.atk / upperslashafter.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 物理/down 反应/hit lift up/hit wav R_SLESSSWDA_HIT（浮空力由脚本注入，不在 .atk） |
| PO 定义 | —（无） | `…\pvf\passiveobject\character\swordman\` | ⛔ 无 | — |
| 刀光特效 | upperslash1/2.ani | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | 挑击弧光 |
| 装备层 | up_attack.ani ×76 / upperslashafter.ani ×76 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,coat,face,hair,neck,pants,shoes}\*\` | ✅ 实测（find 计数各 76） | 各 avatar 变体图层（只查存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img（帧索引集） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色/追加段动画图集 | 必需（共享） | ✅ `sm_body0000.img.bytes` 已在库 |
| upperslash.img | sprite_character_swordman_effect.NPK（img 直接位于 Effect\ 下） | 上挑刀光（upperslash1/2.ani 共用，帧索引区分） | 可选（建议做——上挑动作本体平淡，刀光是辨识度主体；且 .als 自动叠加已支持） | ❌ 未入库 |

必需 img **0 张**；可选 1 张。

## 5. 实现方案草案

- **内容件清单**（零新机制，浮空是已有强项）：
  - `DotNet~/Skills/UpperSlashSkill.cs : SkillLogic`——同 `BloodBoomSkill` 范式 + `ReleaseWaveSkill` 的反应参数法：
    - `CooldownMs = 2000`（原值直用）；`TotalTimeMs = 550`（动画 9 帧；若做追加段窗口可延到 1000，见下）。
    - `HitReaction`（static readonly）：`Damage=70 / HitstunMs=500 / KnockbackX=100 / LaunchY=350`——剑鬼版 BackForce 100 / UpForce col1（Lv1=350）实证值直用；浮空手感即 `LaunchY` 驱动 LSFlight（releasewave 同构）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanUpAttack)` + `ctx.ClearHitTargets()`。**攻击盒走帧驱动**——up_attack.json 自带 F2/F3 attackBox（翻译后 LSHitboxComponentSystem 自动激活，判定帧=有盒帧，releasewave F1/F2 同构），**不用 SetAttackHitbox**。
    - `OnUpdate`（追加段，可选做）：F2 起 `ctx.PeekBufferedButton()==本技能键` → `ctx.ConsumeBuffer()` + `ctx.PlayAnim(AnimId.SwordmanUpperSlashAfter)` + `ctx.ClearHitTargets()` + `ctx.SetSubState(2)`（追加段自身无 .ani 盒 → `ctx.SetAttackHitbox` 固定盒一次，参照 HardAttack 草案）。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - **不需要 Area/Bullet/Buff/新 Action**。
- **概念映射**：引擎状态 + up_Attack.ani → SkillLogic + AnimId；脚本 UpForce/BackForce/DIRECTION_UP → `HitReaction.LaunchY/KnockbackX`；.ani F2/F3 攻击盒 → 帧驱动盒（json）；.als 刀光 → `AnimOverlayConfig` 自动叠加（.als 子命令现成）；SUPERARMOR 全帧 → 延后（§7）；SPECTRUM 残影 → 延后。
- **注册点清单**：

  | 什么 | 在哪 | 增量 |
  |---|---|---|
  | SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.UpperSlash = 12` + `ButtonToSkill` case 8 |
  | AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanUpAttack=49`、`SwordmanUpperSlashAfter=50`、`UpperslashFx=51`（overlay 别名映射用） |
  | json 注册 | `…\LSAnimClipRegistrar.cs` | `RegisterOne` ×2 + `als` 翻译的 `up_attack_overlay.json`（别名 sub2→UpperslashFx） |
  | 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 无必增；可选 `upperslash.img.bytes` |
  | 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 8 |

- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 2000ms | 2000 |
| 总时长 | 550ms（9 帧）；追加段再 500ms | 550（追加窗口关闭时点） |
| 命中帧 | F2-F3（100-200ms，帧驱动盒） | 直用 json 盒 |
| 伤害 | col0 120% + col3 30 固定（Lv1） | MeleeHit 固定 70 |
| 浮空力 | col1 = 350（Lv1）→ 1167（Lv50），BackForce 100，方向 UP | LaunchY 350 / Kb 100 |
| 霸体 | 全 9 帧 SUPERARMOR | 跳过（§7） |
| 追加段伤害 | 未考证（atk 同构主段，hit lift up） | Damage 50 / 同浮空参数 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `UpperSlash.skl` | `.skl` 尚无子命令（4 列 level info + static data） | 手抄可行（列语义本档已实证 3/4）；随批量化提级 |
| `upperslash.atk` / `upperslashafter.atk` | `.atk` 尚无子命令 | 手抄 ~5 值可接受 |
| `up_attack.ani` | `[SPECTRUM]` 节（含 SPECTRUM TERM/LIFE TIME/COLOR/EFFECT 子节）**不在翻译规则表**——本批新见节 | **新缺口**：武器残影轨迹配置（term150/life100/白80%/LINEARDODGE）。暂跳过（工具按未识别节丢弃即可）；若要还原，需 AnimClipData 加 trail 字段 + 视图层拖尾渲染——消费侧大件，先记档 |
| `up_attack.ani` | `[DAMAGE TYPE] SUPERARMOR`（全 9 帧） | 按既有约定整节跳过（01§0.4 霸体帧延后）；**消费缺口**：AnimFrameData 加 damageType 字段的既有提案再次获得数据源 |
| `up_attack.ani.als` | `[use animation]` + `[none effect add]` | ✅ 均已支持（官方文件首见 [none effect add]，解析无缺口） |
| `upperslash1/2.ani` | `[SHADOW]`（值 0） | 整节跳过无碍（GoreCross 先例） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 攻击全程霸体（explain 明示 + .ani 全帧 SUPERARMOR） | 霸体帧（延后档） | demo 无霸体：施放中可被打断（受击系统接管）；后续按 01§0.4 提案补 damageType 字段后还原 |
| 浮空力随等级增长 350→1167 | 等级数值缩放（延后） | 固定 LaunchY 350（Lv1 值） |
| 追加攻击段（触发条件引擎内置未考证） | 无声明式数据源 | 按"动画中再按技能键"推断建模（输入缓冲 + SubState 现成）；不做也不影响主体 |
| SPECTRUM 武器残影 | 残影系统（延后档新项） | 跳过（纯视觉） |
| 剑鬼版攻速加成（sq_SetStaticSpeedInfo） | 无攻速属性系统 | 忽略 |
| 音效（R_SLESSSWDA_HIT） | 延后（无音频系统） | 跳过 |
| MP 消耗 | 延后 | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. 追加攻击段（UpperSlashAfter）的触发条件与数值（引擎内置；按玩法惯例推断"动画中再输入"）。
2. `[static data] 100 -250 100 -50` 语义（4 值，或与霸体/浮空修正相关）。
3. skl col2 恒 100 的语义（推断浮空力显示比率基准）。
4. upperslash2.ani 与追加段的挂接关系（无引用者可查，推断伴生）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **武器残影/轨迹（[SPECTRUM] 节）**：本批新见的节类型，配置完整（间隔/寿命/颜色/混合模式）。跨技能可能出现（凡带刀光的技能都可能有）；建议在 01§0.4 表增补"残影"行（延后档），翻译侧 README 未识别节清单补记。
2. 官方文件使用 `[none effect add]`（此前以为仅 mod 用）——**不是缺口**，但更新 L 间经验：als 解析对官方边车同样会遇到该节名。

**给下轮的经验**：上挑的浮空机制（col1 UpForce + ATTACK_DIRECTION_UP + BackForce 100）全在 attack.nut 的剑鬼函数组里——**attack.nut 是老技能行为的隐藏参照库**（47 行还有按武器类型分支的动画切换），查老技能先把 attack.nut 的函数名列表扫一遍。基础版动作不叫 upperslash.ani 而是 **up_Attack.ani**（etc 槽 202）——按技能名找不到动画时，用 CUSTOM_ANI_* 常量值去 .chr 对槽位。
