# 鬼斩（HardAttack）

> 技能ID 5 | 级别 A | 可实现性 ✅ | 分析日期 2026-08-22 | 批次 A1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼斩 | `HardAttack.skl [name]` |
| 英文名 | HardAttack（取 skl 文件名；本 pvf 的 [name2]=`鬼斩` 是中文，L1 实证再添一例） | 同上 |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5，Lv1 起手技） | 同上 |
| 学习等级 | 1 | 同上 [required level] |
| 最高等级 | [maximum level] 原始值 700（口径未考证；[level info] 实际 70 档，GoreCross 同现象） | 同上 |
| 类型 | active（skill class 3） | 同上 [type] |
| 指令 | ↑(按住状态下)+Z | 同上 [command] / [command key explain] |
| CD | 6000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 17 → 196（Lv1 → Lv70） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 武器效果类型 | magical（魔法系） | 同上 [weapon effect type] |
| static data | `100`（语义未考证，引擎内置消费） | 同上 [static data] |
| 一句话效果 | 把鬼神召唤到武器上，向前方敌人击出强大威力的暗属性斩击（击倒型） | 同上 [explain] |

**level property（2 列，Lv1 → Lv70）**：col0 `390 → 3686`、col1 `65 → 614`。
- **col0 = 攻击倍率%实证**：剑鬼参照脚本 `jg_swordman/hardattack/hardattack.nut` 的 `onSetState_ghostslash` 写着 `sq_GetBonusRateWithPassive(5, -1, 0, 1.0)`——技能号 5、列号 0，即本列。
- col1 语义未考证（推断为固定魔攻加成，同型技 UpperSlash 的列 3 在其脚本中即固定伤害加成 `sq_GetPowerWithPassive`）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**基础版（技能 5）在 `swordman_load_state.nut` 中无注册行**（全文件实测，本技能属最老一���：角色侧逻辑编译在客户端引擎内，pvf 只提供数据文件，走读法见 `_轮间经验.md` F3）。

**剑鬼（SWORD_GHOST）同型参照注册**（同文件 150-151 行，机制同构的现代 nut 化实现）：

```
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/jg_swordman/hardattack/hardattack.nut", "hardattack_swordman", 20, -1);
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/jg_swordman/hardattack/hardattack.nut", "ghostslash", STATE_HARDATTACK_BLADESPIRIT, -1);
```

引擎内置的直接证据：`sqr\character\swordman\swordman_header.nut:171` 定义 `CUSTOM_ANI_HARDATTACK <- 1`，白名单内 grep 实证该常量**除定义外无任何 nut 引用**（仅剑影版常量 273/168 被 jg 脚本使用）——动画/攻击信息由引擎按 .chr 槽位直取。

### 2.2 引擎内置状态行为重建（.ani 标记 + .atk 数据 + 剑鬼脚本三方印证）

**onSetState（施法瞬间）——推断**
- 播 `hardattack.ani`（.chr etc motion 槽 1，974 行）；
- 扣 MP；设角色攻击信息 `hardattack.atk`（etc attack info 槽 0）。

**帧标记（hardattack.ani 实测）**

| 帧号 | 累计时间 | 标记 | 推断语义 |
|---|---|---|---|
| F6 | 300ms | **flag 65534** | 取消窗口（0xFFFE，GoreCross 同型惯例；推断本刀命中段起点也在此时点附近——挥砍蓄势完成） |
| F11 | 550ms | **flag 65534** | 第二取消窗口（后段可取消回待机） |

- **无攻击盒���无 1/2 号命中 flag**：命中判定完全引擎施加（老技能武器判定），pvf 无从取证精确命中帧；从动画节奏（F5-F11 挥砍主体）推断命中窗口 ≈ 300~550ms。
- 全 18 帧有 damageBox（受击盒完整）。

**剑鬼脚本版逐回调（参照实证，`character/jg_swordman/hardattack/hardattack.nut` 102 行实测）**
- `onSetState_ghostslash` 子状态 0：播 `CUSTOM_ANI_HARDATTACK_BLADESPIRIT(273)`；按"剑气"技能等级设置动画速度率；`sq_SetCurrentAttackInfo(168)`；**`damage = sq_GetBonusRateWithPassive(5, -1, 0, 1.0)`——用技能 5 的列 0 作攻击倍率**（基础版数值复用实证）；`sq_StopMove`。
- `onEndCurrentAni_ghostslash`：回 `STATE_STAND`（老技能标准收尾）。
- `onKeyFrameFlag`（10001 → 创建 PO 24349）整段**被注释**——旧机制废弃痕迹。
- `onAfterSetState_hardattack_swordman`：短剑（isSwordSaber）时改走剑影状态 105——职业分支先例。

**onEndCurrentAni（原版推断）**：回待机 state 0。

### 2.3 被动对象 / appendage

**基础版无被动对象**（passiveobject 白名单内无 hardattack 相关 .obj/.act——只有蓄力变体，见下）。

**蓄力变体（相邻技能，另批分析）**：lst 第 180 行有 `HardAttackCharge.skl`（蓄力鬼斩）；其资源已在本技能目录树内：
- 角色 `hardattackchargeafter.ani`（etc 槽 17，11 帧 650ms）+ `HardAttackChargeAfter.atk`（etc atk 槽 21，与 hardattack.atk 同构：魔法/暗属性/down/push300/lift300）；
- PO `hardattackchargeafter.obj`（name=卡赞）：etc motion 6 个（khazan/saya/bremen 各 down/up）+ etc attack info 3 个（Khazan/Saya/Bremen.atk，魔法/暗属性，down/push300/lift200/hit horizon）——蓄力满档召唤三鬼神追击的多相位 PO（L9 结构）；
- 特效 `hardattackfullcharge1/2.ani`、`hardattackoncharge1/2.ani`、`hardattackchargeafterdust.ani` + PO 动画目录 `hardattackcharge\`（khazan/saya/bremen + bladepantom 系列）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/hardattack.ani`（角色） | 18（F0-17） | 950ms（F0-2=50，F3-4=25，F5-16=50，F17=150） | F6=65534，F11=65534 | **无**（引擎施加武器判定） | 每帧 damageBox；仅引 `sm_body%04d.img` |
| `character/swordman/effect/animation/hardattack1.ani`（刀光1） | 8 | 550ms | 无 | 无 | `HardAttackBlade1.img`；引擎内置绘制（无 .als 无脚本引用，grep 实证） |
| `character/swordman/effect/animation/hardattack2.ani`（刀光2） | 8 | 550ms | 无 | 无 | `HardAttackBlade2.img`；同上 |

`.als` 边车：**无**（animation 目录 ls 实证；`hardattack_bladespirit.ani.als` 为剑影版，非本技能）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | HardAttack.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\HardAttack.skl` | ✅ 实测 | 技能数据（CD6000/MP17-196/2列等级数据） |
| lst 条目 | ID 5 | `…\pvf\skill\swordmanskill.lst` 11-12 行 | ✅ 实测 | — |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ **缺失（引擎内置状态）** | 见 §2.1 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（grep hardattack 无 nut） | ⛔ 缺失 | 参照剑鬼版 `…\pvf\sqr\character\jg_swordman\hardattack\hardattack.nut`（102 行实测） |
| .chr 条目 | etc motion #1 + etc attack info #0 | `…\pvf\character\swordman\swordman.chr` 974 / 1294 行 | ✅ 实测 | Animation/HardAttack.ani；AttackInfo/HardAttack.atk |
| 角色 .ani | hardattack.ani | `…\pvf\character\swordman\animation\hardattack.ani` | ✅ 实测 | 18 帧 950ms，flag 65534×2 |
| 角色 .atk | hardattack.atk | `…\pvf\character\swordman\attackinfo\hardattack.atk` | ✅ 实测 | 魔法/暗属性/down/push300/lift300/hit down |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失（本技能无边车） | — |
| PO 定义 | —（基础版无） | `…\pvf\passiveobject\character\swordman\` | ⛔ 无（仅蓄力变体 hardattackchargeafter.obj） | — |
| 刀光特效 | hardattack1/2.ani | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | 引擎绘制刀光（无引用者） |
| 装备层 | hardattack.ani ×76 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,coat,face,hair,neck,pants,shoes}\*\` | ✅ 实测（find 计数 76） | 各 avatar 变体图层（只查存在性） |
| 关联强化 | HardAttackCharge.skl（蓄力鬼斩） | `…\pvf\skill\Swordman\HardAttackCharge.skl` | ✅ 存在（lst 180 行） | 另批分析；其资源清单见 §2.3 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img（帧索引集） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色挥砍动画图集（%04d 解析为单图集） | 必需（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` 已在库 |
| HardAttackBlade1.img | sprite_character_swordman_effect.NPK（img 直接位于 Effect\ 下） | 刀光特效 1（hardattack1.ani 8 帧） | 可选（建议做，暗属性刀光是本技能辨识度主体） | ❌ 未入库 |
| HardAttackBlade2.img | 同上 | 刀光特效 2（hardattack2.ani 8 帧） | 可选 | ❌ 未入库 |

**结论**：必需 img **0 张**（角色动画图集已入库）；可选 2 张同属一个 NPK。
img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案

- **内容件清单**（零新机制）：
  - `DotNet~/Skills/HardAttackSkill.cs : SkillLogic`——同 `BloodBoomSkill` 范式（帧号 const + SubState 一次性守卫）：
    - `CooldownMs = 6000`（DNF 原值直用；demo 嫌长可 3000）；`TotalTimeMs = 950`（动画 18 帧总时长）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanHardAttack)` + `ctx.ClearHitTargets()`。
    - `OnUpdate`：F6（300ms，取消窗口同点）`ctx.SetAttackHitbox(offset(0.9,0,0.8), half(0.8,0.3,0.6))`（引擎无攻击盒数据 → 固定盒路径，尺寸参照 GoreCross 两刀惯例）+ `HitActions={MeleeHit}`；F12（600ms）`ctx.DisableAttackHitbox()`（单段命中即可，ClearHitTargets 保证只结算一次）。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
    - `HitReaction`（static readonly）：`Damage=110 / HitstunMs=600 / KnockbackX=300 / LaunchY=300`——hardattack.atk 原值 push300/lift300 + down 反应（击倒型，releasewave as-built §5.6-3 同构手感）。
  - **不需要 Area/Bullet/Buff/新 Action**（单段武器斩）。
- **概念映射**：引擎状态（技能 5）+ hardattack.ani → `HardAttackSkill + AnimId`；引擎施加武器判定 → `SetAttackHitbox` 固定盒（GoreCross 同法）；.atk down/push/lift → `HitReaction`；刀光 hardattack1/2 → 手组装 overlay（releasewave 先例）或跳过（引擎内置绘制无声明式来源，GoreCross §8 已上报同类）。
- **注册点清单**：

  | 什么 | 在哪 | 增量 |
  |---|---|---|
  | SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.HardAttack = 11` + `ButtonToSkill` case 7 |
  | AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanHardAttack=49`（+可选 `HardAttackFx1=50`/`HardAttackFx2=51`，接现有 48 之后顺延） |
  | json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×1（+可选刀光 2 个） |
  | 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 无必增（sm_body 已在）；可选时加 `HardAttackBlade1/2.img.bytes` |
  | 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 7 |

- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 6000ms | 6000（或 demo 3000） |
| 总时长 | 950ms（18 帧） | 950 |
| 命中窗口 | 引擎内置未考证（推断 F6-F11 ≈ 300-550ms） | F6 起盒、F12 关盒 |
| 伤害 | skl col0 = 390%（Lv1，武器魔攻%）+ col1 65（推断固定值） | MeleeHit 固定 110 |
| 命中反应 | hardattack.atk：down / push 300 / lift 300 / hit down | Damage110/Hitstun600/Kb300/Ly300 |
| 暗属性 | [elemental property] dark element | 无元素系统 → 忽略（§7） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `HardAttack.skl` | `.skl` 尚无子命令（2 列 level info + static data） | 手抄可行；随 241 技能批量化提级（GoreCross 已上报同类） |
| `hardattack.atk` | `.atk` 尚无子命令 | 手抄 ~6 值可接受 |
| `hardattack1/2.ani` | `[SHADOW]`（值 0）不在翻译规则表 | 整节跳过无碍（GoreCross 先例），README 未识别节清单补记 |
| `hardattack.ani`（角色） | `[SET FLAG]`（65534×2） | 按既有约定跳过（触发帧 const 进技能类）——非缺口 |
| `up_attack.ani.als` 类 | —（本技能无 .als） | 本技能 .ani/.als 资源全部可被现有 ani 子命令翻译 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 暗属性魔法伤害（elemental property + attack type magic） | 元素属性系统（缺失档） | 中立物理伤害，数值等价；技能图标/刀光染暗色补偿辨识度 |
| 命中帧引擎内置（无 .ani 攻击盒、无命中 flag） | 无声明式数据源（老技能通病） | 固定盒 + 帧号 const（GoreCross 同法）；窗口取 F6-F11 |
| 刀光 hardattack1/2 引擎内置绘制（无 .als、无脚本引用） | 延后（无声明式翻译源，GoreCross §8-2 同类） | 先跳过；还原时手组装 overlay（releasewave 先例） |
| 65534 取消窗口（F6/F11 可取消接后续动作） | 技能取消体系缺失（GoreCross §8-1 已上报） | 忽略（技能完整播完）；已有 RestartCurrentSkill 可表达同技能连发 |
| 音效（atk [hit wav] 未在本 .atk 出现；挥砍声引擎内置） | 延后（无音频系统） | 跳过 |
| 等级缩放（col0 390%→3686%） | 延后（等级缩放） | demo 固定值 |
| MP 消耗 | 延后（无 MP 系统） | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. 引擎内置命中帧窗口（无数据源，推断 300-550ms）。
2. `[static data] 100` 语义。
3. skl col1（65→614）语义（推断固定魔攻加成）。
4. [maximum level]=700 的口径（[level info] 实际 70 档）。

**新系统级缺口（§6.3 清单外，主循环汇总）**：无新增（元素属性/技能取消/引擎绘制特效均已被 GoreCross 上报在案；本技能再次实证"老技能引擎内置"走读法 F3 有效——常量定义但无 nut 引用即为其特征）。

**给下轮的经验**：老一代 Lv1-5 起手技（鬼斩/上挑/连突刺/银光落刃）全部引擎内置，但**剑鬼区（load_state 149-153 行）注册了同型 nut 化参照**（hardattack/tripleslash/upperslash），且 attack.nut 内还藏着 upperslash_swordman 函数组——查老技能先扫这两处。蓄力变体资源（hardattackchargeafter.obj 三鬼神 PO）混在本技能目录树内，分析 HardAttackCharge 时直接复用 §2.3 清单。
