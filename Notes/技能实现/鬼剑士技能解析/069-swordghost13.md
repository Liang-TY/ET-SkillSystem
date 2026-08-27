# 双魂共鸣（swordghost13）

> 技能ID 69 | 级别 B（预判 A 纠偏：主动自 buff 状态技，无攻击判定） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 双魂共鸣 | `skill\Swordman\SwordGhost\swordghost13.skl [name]` |
| 英文名 | swordghost13（skl 文件名；无 [name2]）——引擎别名 **SPIRITCONVERSION（灵魂共鸣，旧名）**：header 常量 `SKILL_SPIRITCONVERSION <- 69`、`CUSTOM_ANI_SPIRITCONVERSION <- 274` 与本技能常量同值并立 | skl 实测 + swordman_header.nut:41/122/154/445 |
| 职业 | 剑影（[skill fitness growtype]=5；JG_SwordMan 目录） | 同上 |
| 学习等级 | 20 | 同上 [required level] |
| 最高等级 | 40（剑影觉醒段上限 10） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1）——**buff 型主动技** | 同上 [type] |
| 指令 | ↑↑ + Space（BUFF 键） | 同上 [command] |
| CD | 10000 ms | 同上 [cool time] 10000 10000 |
| MP | 271 → 2570（Lv1 → Lv40） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 前置 | 技能 123（鬼影剑）Lv1 | 同上 [pre required skill] |
| static data | `15000` = **buff 持续时间 15s**（nut 未读、引擎消费——level property 模板"持续时间 : - 秒"印证，机制推断见 §8） | 同上 |
| 一句话效果 | 施放时两个灵魂在剑鬼身体里共鸣强化身体：增加基本攻击力和技能攻击力（16.5% → 82.5%），持续 15 秒 | 同上 [explain] + level property |

**level property（1 列，Lv1 → Lv40）**：`165→825`，`<float1>%%` 且配置行 `-1 0 0.1` → **col0/10 = 16.5% → 82.5%**
（基本攻击和技能攻击力增加量）。nut 实证：`sq_GetLevelData(obj, 69, 0, level)` 存入 appendage var "ap"。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
172: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "Character/JG_SwordMan/SwordGhost13/SwordGhost13.nut", "SwordGhost13", STATE_SWORD_GHOST_13, SKILL_SWORD_GHOST_13);
3:   IRDSQRCharacter.pushScriptFiles("Character/JG_SwordMan/jg_swordman_common.nut");   //（同文件头部，剑影公共脚本）
```

- 状态名 `SwordGhost13`，STATE_SWORD_GHOST_13=117（=STATE_SPIRITCONVERSION，新旧名同号），SKILL=69。
- nut 目录 `sqr\character\JG_SwordMan\SwordGhost13\`（注意大小写与目录位置：**在 character\JG_SwordMan\ 下，
  不在 swordman\ 下**）。目录内 8 个文件，本技能实际只用 2 个：`swordghost13.nut`（41 行）+
  `ap_soulscoexist.nut`（43 行 appendage）；其余 ap_buff*.nut / ap_swordghost27/28.nut 属同目录其他剑影技（22/24/25/26/27/28 等）。
- **无被动对象**（不创建 24349/无攻击判定体）——纯状态技。

### 2.2 主 nut 逐回调（swordghost13.nut，41 行，全量）

**checkExecutableSkill**：`sq_IsUseSkill(69)` → 推状态 117（标准门禁）。
**checkCommandEnable**：恒真。

**onSetState（施法瞬间，一次性）**：
```
sq_StopMove
播 CUSTOM_ANI_SWORD_GHOST_13_BUFF = 244 → .chr etc motion #244 = Animation/spiritconversion_body.ani（4 帧 1190ms）
bonus = sq_GetLevelData(69, 0, level)                       // 165→825
appendage = sq_AppendAppendage(obj, obj, 69, …, "character/jg_swordman/swordghost13/ap_soulscoexist.nut", false)
appendage.setAppendCauseSkill(BUFF_CAUSE_SKILL, job, 69, level)
appendage.getVar("ap").push_vector(bonus)                   // 增伤数值塞进 appendage 变量
sq_AppendAppendageID(appendage, obj, obj, 69, true)         // 注册进引擎 buff ID 系统
appendage.setEnableIsBuff(true) + setBuffIconImage(150)     // 标记为 buff + 图标 150
// 施法闪光：sq_CreatePooledObject 播 effect/animation/spiritconversion/spiritconversionbuff_00.ani
//           （34 帧 1040ms，aura1.img，带 .als 叠 aura2 层），挂在自身位置
```

**onEndCurrentAni**：回站立 STATE_STAND。**无 onProc/onKeyFrameFlag**（技能本体无过程逻辑）。

### 2.3 appendage（ap_soulscoexist.nut，43 行，全量）

- 注册 `proc`/`prepareDraw`/`onStart`/`drawAppend` 四回调；**proc/onStart 均为空壳**（只做判空）。
- **drawAppend**：buff 存续期间每帧在角色上方绘制 `effect/animation/spiritconversion/buff_loop.ani`
  （17 帧 800ms **循环**，`character/swordman/equipment/growtype/bladespirit_2nd09.img`——二觉光环图），
  位置 y+角色高/5。
- **增伤的执行端不在脚本**：appendage 未设 sq_SetValidTime（时长），未在 proc 里改攻击数值——
  增伤与 15s 时长均由引擎 buff 系统（setEnableIsBuff + AppendAppendageID + var "ap"）结算，pvf 侧不可见（§8 存疑）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/spiritconversion_body.ani`（施法动作，.chr #244） | 4 | 1190ms | 无 | 无 | sm_body 帧号 238/239（与 spiritmove1 姿势帧同图集段） |
| `effect/animation/spiritconversion/spiritconversionbuff_00.ani`（施法闪光） | 34 | 1040ms | 无 | 无 | aura1.img；.als 叠 buff_01（aura2.img）@帧 0 层 10001 |
| `effect/animation/spiritconversion/buff_loop.ani`（存续光环） | 17 | 800ms（loop） | 无 | 无 | bladespirit_2nd09.img，appendage drawAppend 每帧绘制 |
| `character/swordman/animation/spiritconversion.ani`（旧名版本，.chr #274） | 4 | 1190ms | 无 | 无 | 旧版动作（nut 未用），自身带 .als（叠 Buff_00/Buff_01） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordghost13.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SwordGhost\swordghost13.skl` | ✅（215 行） | 技能数据 |
| 注册行 | swordman_load_state.nut:172 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 117 注册 |
| 主 nut | swordghost13.nut | `…\pvf\sqr\character\JG_SwordMan\SwordGhost13\swordghost13.nut` | ✅（41 行） | 施法+挂 buff |
| appendage | ap_soulscoexist.nut | `…\pvf\sqr\character\JG_SwordMan\SwordGhost13\ap_soulscoexist.nut` | ✅（43 行） | 光环绘制（增伤在引擎） |
| 常量 | swordman_header.nut:23/41/122/154/414/445 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | STATE 117 / ANI 244 / 新旧名并立 |
| .chr 条目 | etc motion #244（1217 行）/ #274（1247 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | 新旧施法动画 |
| 角色 .ani | spiritconversion_body.ani / spiritconversion.ani | `…\pvf\character\swordman\animation\` | ✅ | 施法动作 |
| 特效 .ani | spiritconversionbuff_00/01.ani、buff_loop.ani、buff_effect_l_00/01/02、buff_effect_n_03.ani | `…\pvf\character\swordman\effect\animation\spiritconversion\` | ✅ | 施法闪光/存续光环（后 4 个属他技/受击向，本文不消费） |
| .als 边车 | spiritconversionbuff_00.ani.als、spiritconversion.ani.als、buff_effect_l_00.ani.als | 同上 | ✅ | 叠层（[add]/[none effect add]） |
| 角色 .atk / PO | —（无攻击判定） | — | ⛔ 不适用 | 纯状态技 |
| 装备层 | spiritconversion*.ani ×152 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | avatar 变体图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 238/239） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | **必需** | ✅ 已在库 |
| Effect/SpritConversion/aura1.img（注意 DNF 原路径拼写就是 Sprit） | sprite_character_swordman_effect_spritconversion.NPK | 施法闪光主层 | 可选 | ❌ |
| Effect/SpritConversion/aura2.img | 同上 | 施法闪光叠层（.als） | 可选 | ❌ |
| equipment/growtype/bladespirit_2nd09.img | sprite_character_swordman_equipment_growtype.NPK | 存续光环（buff_loop） | 可选（做光环才需要） | ❌ |

缺失 img：必需 0 张、可选 3 张（3 个 NPK）。

## 5. 实现方案草案（🔶：增伤消费端缺——buff 落地为数值写入+视觉）

1. **`DotNet~/Skills/SoulResonanceSkill.cs : SkillLogic`**（TestCooldownSkill 自buff范式 + bloodboom 骨架）
   - `CooldownMs = 10000`；`TotalTimeMs = 1190`（施法动画 4 帧）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSpiritConversionBody)` + `ctx.AddBuffToSelf(BuffIds.SoulResonance)`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Buffs/SoulResonanceBuff.cs : BuffDefinition`**（BurnBuff 同构）
   - `TotalTimeMs = 15000`（static 直译）；`TickTimeMs = 0`。
   - `AddActions = { AddSoulResonanceOn }`、`RemoveActions = { AddSoulResonanceOff }`。
3. **`DotNet~/Actions/AddSoulResonanceOn/OffAction.cs : LSAction`**（ForbidMoveOn/Off 同构成对，~10 行）
   - On：`ctx.AddOwnerNumeric(NumericType.AttackPct, +82)`（demo 固定 Lv 满档 82.5→取整；DNF 原值 col0/10）；
     Off：`-82` 对冲。**NumericType.AttackPct(10033) 真实存在**（`Scripts\Model\Share\NumericType.cs` 实测），
     五层公式 `final=(base+add)×(1+pct)+…` 会重算 Attack final——**写入端全通**。
   - **🔶 降级点**：当前 `MeleeHitAction.Damage` 读固定 `HitReaction.Damage`，**无任何代码消费 Attack 数值**——
     buff 挂上后伤害不会实际变化（纯数值簿记+视觉）。补齐方案（小框架改动）：MeleeHit 伤害改为
     `reaction.Damage × (1 + ownerNumeric[AttackPct]/100)`（LSActionContext 加一个 GetOwnerAttackPct 门面），
     或等伤害公式数值化专题。demo 建议先带门面（约 15 行）��把消费端补上——否则技能等于没效果。
4. **视觉**：施法闪光 = 施法动画 overlay（spiritconversionbuff_00 + als，LSAnimClipRegistrar 注册后自动叠）；
   存续光环 = buff 持续期循环层——**Buff 系统无视觉挂接**（§7 新缺口），简化：跳过存续光环
   （或 demo 里由技能 OnCast 后挂一个 15s 循环 overlay 到单位视图——需手组装，记档）。
5. 需要新增的 Area/Bullet：无。

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SoulResonance = 14` + 新按键（如 V） |
| BuffId | `Runtime\BuffDefinition.cs` | `BuffIds.SoulResonance = 5` |
| ActionId | `Runtime\LSAction.cs` | `ActionIds.AddSoulResonanceOn = 8`、`Off = 9` |
| AnimId | npkparser `AnimConfigRegistry.cs` | `SwordmanSpiritConversionBody = 54`、`SpiritConversionCast = 55`（闪光，可选） |
| json/图集/按键/翻译 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem / DnfConfigTranslation | 施法 json（+可选闪光 json/图集） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000ms | 10000 |
| 施法时长 | 1190ms（4 帧） | 1190 |
| buff 时长 | 15000ms（static） | 15000 |
| 增伤 | col0/10 = 16.5% → 82.5%（基本+技能攻击力） | AttackPct +82（满档直译） |
| buff 图标 | setBuffIconImage(150) | 无 UI，跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `swordghost13.skl` | `.skl` 无子命令 | 手抄（并入既有 skl 子命令建议） |
| `spiritconversion_body.ani` / `spiritconversionbuff_00.ani` / `buff_loop.ani` | 常规节（FRAME/IMAGE/DELAY/LOOP） | **现有 ani 子命令全覆盖**（实测节名仅 LOOP/FRAME/SHADOW） |
| `spiritconversionbuff_00.ani.als` / `spiritconversion.ani.als` | `[use animation]` + `[add]`/`[none effect add]` | **现有 als 子命令全覆盖** |
| `[SHADOW]`（部分 ani） | 已知跳过节（064 已记） | 无需处理 |
| appendage `ap_soulscoexist.nut` | 行为脚本无对应翻译类型 | 不需翻译（逻辑映射为 BuffDefinition，§5 已给） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 增伤由引擎 buff 系统结算（appendage 只存数值+画光环） | **伤害公式未接数值系统**（Attack 数值无消费端——等级数值缩放同类，延后档实证） | LSActionContext 加 GetOwnerAttackPct 门面 + MeleeHit 乘算（~15 行小改）；或 demo 承认纯视觉 |
| 存续期光环（drawAppend 每帧画 buff_loop.ani） | **Buff 视觉挂接缺失**（新缺口）：overlay 机制挂在动画上，不挂在 buff 上 | 跳过存续光环；或手组装 15s 循环 overlay 到单位视图 |
| 15s 时长的引擎消费机制（static 15000 → 引擎 buff 到期） | 机制黑盒（nut 无 ValidTime） | 我们侧用 BuffDefinition.TotalTimeMs=15000 直译，行为等价 |
| buff 图标/UI（setBuffIconImage） | 无 buff UI | 跳过 |
| 新旧双名并存（SPIRITCONVERSION/SwordGhost13） | 非缺口（记档防迷路：查 69 相关常量两条名都要搜） | — |

## 8. 存疑与缺口上报

**未考证项**
1. 引擎如何消费 var "ap" 与 static 15000（增伤公式入口、到期机制）——pvf 无源，行为按 explain+模板值直译。
2. `spiritconversion.ani`（旧名 #274）与 `spiritconversion_body.ani`（#244）新旧两版并存的原因（改名期产物），未考证哪段
   哪些职业用旧版。
3. 目录内 ap_buff*.nut / ap_swordghost27/28.nut 归属（同目录其他剑影技 22/24-28），本技能不消费——后续批次按需读。

**新系统级缺口（§6.3 清单外）**
1. **Buff 视觉挂接**：Buff 存续期间的头像/身上光环/粒子（DNF appendage drawAppend 是通用机制）——
   我们的 Buff 只有逻辑 Tick。建议后续给 BuffDefinition 加 `ViewAnimId`（挂单位 overlay 层，buff 添加/移除时
   创建/销毁），本技能（存续光环）与燃烧/冰冻（状态着色）都会用到。归"缺失"档记名。
2. **增伤类 buff 的消费端**（伤害公式数值化）：AttackPct 数值五层公式已就绪但 MeleeHit 不读——
   与 §6.3"等级数值缩放（延后）"同根。本技能是第一个**必须**消费数值的用例，建议随本技能实现时把
   GetOwnerAttackPct 门面一并落地（否则该技能无可感知效果）。

**翻译工具缺口**：无新增（ani/als 全覆盖；skl 既有缺口）。
