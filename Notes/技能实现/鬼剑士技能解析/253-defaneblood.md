# 狂气涌动（defaneblood）

> 技能ID 253 | 级别 B（预判 A 纠偏：无攻击逻辑，纯自体生存 BUFF） | 可实现性 🔶（HP 下限钳制用 Tick 补血近似） | 分析日期 2026-08-22 | 批次 A15

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 狂气涌动 | `skill\Swordman\defaneblood.skl [name]` |
| 英文名 | defaneblood（取 skl 文件名，全小写） | 同上 |
| 职业 | 狂战士（血气系二觉技；血气防御罩/狂气文案） | 同上 [explain] + 常识 |
| 学习等级 | 65 | 同上 [required level] |
| 最高等级 | 15（二觉段上限 15） | 同上 [maximum level] / [second growtype maximum level]（索引 7 = 15） |
| 类型 | active（skill class 1）——**但行为是瞬发自 BUFF，无状态无动画** | 同上 [type] + nut 走读 |
| 指令 | Space（BUFF 键） | 同上 [command] |
| CD | 45000 ms（skl）；**实际生效 CD = level col2 = 45s→26s 随等级递减**（startSkillCoolTime 覆写，见 §2.2） | 同上 + defaneblood.nut |
| MP | 450（固定） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 可施放状态 | 0-299 全状态（被击状态 3/4/5/9 由 checkCommandEnable 拦截——"除被击状态以外任何状态可用"） | 同上 [executable states] + nut |
| 一句话效果 | 10 秒内 HP 不会被击落到门槛（默认 50%）以下；触发吸收时驱散全部异常状态并获得全异常抗性 +1000，同时生成血气防御罩视觉 | 同上 [explain] + ap 走读 |

**static data**（dungeon）：`10000` = 持续时间 ms（level property 向量 `(0,0,0.001)` 对位）。

**level info**（3 列 × 20 行）：col0 = 50（恒定，未减少的 HP 比率%）、col1 = 3000（恒定，**模板未消费**——疑触发门槛或护盾参数，无脚本引用，未考证）、col2 = 45000→26000（技能冷却 ms）。

**level property**（3 占位符）：持续时间 `(0,0,0.001)`=static[0]=10 秒；技能冷却时间 `(-1,2,0.001)`=col2；未减少的 HP 值比率 `(-1,0,1.0)`=col0=50%。三向量与 nut 消费（`sq_GetLevelData(…,0/2,lv)`、`sq_GetIntData(…,0)`）完全吻合。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
38: IRDSQRCharacter.pushState(0, "character/swordman/defaneblood/defaneblood.nut", "defaneblood", -1, SKILL_DEFANEBLOOD);
```

- `swordman_header.nut:113 SKILL_DEFANEBLOOD <- 253`。
- **状态号 = -1（无引擎状态）**：全 nut 无 onSetState/onKeyFrameFlag/onEndCurrentAni——技能本体只是"扣 CD + 挂 appendage"的瞬时函数（L2 状态号语义的 -1 变体新样本：**技能绑定存在但状态号 -1 = 不进状态机**）。
- **无角色动画/无 atk**：`swordman.chr` grep defaneblood 0 命中、attackinfo 无对应文件（实测）——视觉全在 appendage 层。

### 2.2 主 nut 逐回调（defaneblood.nut，59 行全读）

**checkCommandEnable**：state ∈ {3, 4, 5, 9}（被击/倒地族状态）→ false，其余 true。

**checkExecutableSkill**（施放 = 瞬时，无状态切换）：
```
挂 ap_defaneblood.nut 到自己（BUFF，图标 96，setEnableIsBuff）
maxHp = level col0 (50)          // HP 门槛 %
time  = static[0] (10000)        // 持续 10s
nowHp = 当前 HP%
门槛锁定值 = (nowHp < 50) ? nowHp : 50      // 施放时已低于 50% → 锁当前值
appendage.sq_SetValidTime(10000)
```

**startSkillCoolTime**：CD 覆写 = level col2（45s→26s）——"冷却时间减少效果不适用该技能"的实现基础（不吃外部 CDR，吃自身等级递减）。

### 2.3 appendage（ap_defaneblood.nut，151 行全读）——技能全部机制在此

| 回调 | 行为 |
|---|---|
| `onSetHp`（核心） | 结算 HP 前钩子：若新 HP ≤ 门槛值（门槛% × MaxHp）→ **钳制为门槛值**（吸收全部超额伤害）；带三豁免：攻击带 `isIgnoreDieHard_` 标记 / 中继超时 / 非战斗状态。吸收成功置标志位 true |
| `proc`（每帧） | 标志位 true（护盾已触发）时：value0 = 1000 → `CHANGE_STATUS_TYPE_ACTIVESTATUS_TOLERANCE_ALL +1000`（全异常免疫）；并**驱散** LIGHTNING/STONE/POISON/BURN/BLEEDING 五种已有异常；另写物理防御 +0（空操作） |
| `onDamageParent`（受击时） | 标志位 true → `createFrontDefaneBlood`（swordman_common.nut:96）：`sq_AddDrawOnlyAniFromParent` 播 berserkhit_a.ani（血气防御罩受击闪现，跟随角色） |
| `drawAppend`（渲染层） | BUFF 期间在角色身上叠播 `character/swordman/animation/berserk/berserk.ani`（狂气光环循环） |

**机制归纳**：10s 内 HP 只能降到一个下限（min[施放时 HP%, 50%]）；一旦有伤害被下限吸收 → 立刻驱散全部异常 + 免疫新异常（抗性 1000）+ 受击闪光特效。光环全程显示。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/berserk/berserk.ani`（狂气光环） | 5 | 500ms（LOOP 循环） | 无 | 无 | drawAppend 驱动，引 Berserk.img；无 .als |
| `character/swordman/animation/berserk/berserkhit_a.ani`（受击护罩） | 11 | 880ms | 无 | 无 | 引 BerserkHit.img/BerserkHitGlow.img；.als 挂 berserkhit_b.ani @帧0 层10001 |
| berserkhit_b.ani | — | — | — | — | 仅作 berserkhit_a 的叠加层 |
| effect/animation/berserk/ 同名 4 文件 | — | — | — | — | effect 侧镜像副本（本技能 nut 引用的是 animation/ 路径，此副本疑他处使用，未考证） |

节名枚举实测：三个 .ani 仅 FRAME MAX/FRAMExxx/LOOP/SHADOW——全常规（SHADOW 已记档跳过）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | defaneblood.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\defaneblood.skl` | ✅ 实测 | 等级/CD/MP/static/3 列 level |
| 注册行 | swordman_load_state.nut 行 38 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 -1（无状态机），技能 253 |
| 主 nut | defaneblood.nut | `…\pvf\sqr\character\swordman\defaneblood\defaneblood.nut` | ✅ 实测（59 行） | 瞬时挂 BUFF + CD 覆写 |
| appendage | ap_defaneblood.nut | `…\defaneblood\ap_defaneblood.nut` | ✅ 实测（151 行） | HP 钳制/异常驱散/视觉 |
| 公共函数 | createFrontDefaneBlood（swordman_common.nut:96） | `…\pvf\sqr\character\swordman\swordman_common.nut` | ✅ 实测 | 护罩受击特效 |
| .chr 条目 | —（0 命中） | `…\pvf\character\swordman\swordman.chr` | ⛔ 无（纯 BUFF 无动画） | — |
| 角色 .ani | berserk.ani / berserkhit_a/b.ani | `…\pvf\character\swordman\animation\berserk\` | ✅ 实测 | 光环 + 受击护罩（animation 目录，非 effect） |
| .als | berserkhit_a.ani.als | 同上 | ✅ 实测 | `[add]` 常规 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 无 | 无命中判定 |
| 特效镜像 | berserk 4 文件 | `…\pvf\character\swordman\effect\animation\berserk\` | ✅ 实测 | 副本，引用者未考证 |
| 装备层 | —（不需要） | `…\pvf\equipment\...` | 未查 | 无角色动作 → 无 avatar 图层需求 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 角色本体（BUFF 不换动画） | — | ✅ |
| `Character/Swordman/Effect/Berserk/Berserk.img` | sprite_character_swordman_effect_berserk.NPK | 狂气光环（berserk.ani） | **必需** | ❌ |
| `…/Berserk/BerserkHit.img` | 同上 | 护罩受击主体 | 可选 | ❌ |
| `…/Berserk/BerserkHitGlow.img` | 同上 | 护罩辉光 | 可选 | ❌ |

缺失 img：必需 1、可选 2，同一 NPK 一次提取。AnimRes 实测全未入库。

## 5. 实现方案草案

**结构映射**：瞬时技能 → 自体 Buff；HP 下限钳制 → Tick 补血近似（伤害管线钩子缺失，§7/§8）。

### 内容件清单

1. **`DotNet~/Skills/DefaneBloodSkill.cs : SkillLogic`**（WaveSwordSkill 极简范式）
   - `CooldownMs = 45000`（DNF 等级递减 45→26s 属等级缩放延后，demo 取 Lv1 值）；`TotalTimeMs = 100`（瞬时技能短占位——无自结束门面 ctx.EndCast()，R1-A3 已录）。
   - `OnCast`：`ctx.AddBuffToSelf(BuffIds.DefaneBlood)`（不播动画、不清命中表——无判定）。
2. **`DotNet~/Buffs/DefaneBloodBuff.cs : BuffDefinition`**（BleedBuff/BurnBuff 同构）
   - `TotalTimeMs = 10000`（static[0]）；`TickTimeMs = 100`；`TickActions = { HealToFloorAction }`。
3. **`DotNet~/Actions/HealToFloorAction.cs : LSAction`**（BleedDamageTickAction 范式，~15 行）
   - `if (ctx.GetOwnerHp() < Floor) ctx.AddOwnerNumeric(NumericType.Hp, Floor - hp)`（补回下限）；
   - **前置**：`LSActionContext.GetOwnerMaxHp()` 门面缺失（百分比下限需要 MaxHp，§8 上报，框架层 3 行小增量）；实现前临时方案：Action 内嵌绝对值门槛（如 HP < 500 → 补到 500，demo 可用）。
4. **无 Area/Bullet**（纯自体技能）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| checkExecutableSkill 挂 appendage | `AddBuffToSelf`（BuffDefinition） |
| onSetHp HP 钳制 | Tick 补血近似（100ms 内拉回下限；一击致死仍死——§7 差异） |
| 门槛锁定 min(施放时 HP%, 50%) | Buff 参数化缺失（const 内嵌）→ demo 固定 50% |
| 异常驱散 + 抗性 1000 | 无异常抗性/驱散系统 → 不做（§7） |
| startSkillCoolTime CD 覆写 | CooldownMs 固定值（等级缩放延后） |
| drawAppend 光环 / 受击护罩特效 | Buff 视觉挂接缺口（R1-A5）→ 跳过 |
| 被击状态禁用（3/4/5/9） | 已有：TryCast 硬直门禁（HitstunTimer > 0 拒绝）✓ 天然对齐 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.DefaneBlood = 20` |
| BuffId | `Runtime\BuffDefinition.cs` | `BuffIds.DefaneBlood = 9` |
| ActionId | `Runtime\LSAction.cs` | `ActionIds.HealToFloor = 12` |
| AnimId | `AnimConfigRegistry.cs` | `BerserkAura = 79`（可选——Buff 视觉通道落地后用） |
| 框架 | `LSActionContext.cs` | （建议）加 `GetOwnerMaxHp()` 门面 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45s（Lv1）→ 26s（Lv20） | 45000（直用 Lv1 值） |
| 持续 | 10s（static[0]=10000） | 10000（直用） |
| HP 下限 | level col0 = 50%（施放时更低则锁当前） | 固定 50% |
| 异常抗性 | +1000（触发后） | 不做 |
| 光环动画 | berserk.ani 500ms 循环 | 跳过（视觉通道缺失） |
| 受击护罩 | berserkhit_a.ani 880ms | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| defaneblood.skl | `.skl` 无子命令 | 手抄（3 列 + static 1 值） |
| berserk.ani / berserkhit_a/b.ani | 仅常规节 + [SHADOW]（已记档） | **全部可被现有 ani 子命令翻译** |
| berserkhit_a.ani.als | `[use animation]`/`[add]` 均支持 | 无缺口 |
| （无 .atk/.obj 参与本技能） | — | — |

结论：缺口仅 `.skl` 子命令 1 条（族共性）；.ani/.als 全通。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **受击伤害管线 HP 下限钳制**（onSetHp 改写结算值） | **新缺口：受击伤害修改钩子**——Buff 无"改伤害/钳 HP"注入点（Buff 只有 Add/Tick/Remove 三时机） | Tick 补血近似：命中后 100ms 内拉回下限；**差异**：一击致死仍死、HP 条会闪降一下 |
| 施放时已低于 50% → 锁当前值 | Buff 参数动态化缺失（const 内嵌） | 固定 50%（高于此场景无差异） |
| 异常驱散 + 全异常抗性 +1000 | 无异常抗性/驱散系统（异常本身只有 4 种 Buff） | 不做（demo 异常面小，影响有限） |
| 吸收触发时受击护罩特效（berserkhit_a） | 无"受击瞬间"Buff 事件钩子 | 跳过（或并入 Tick 首跳显示，视觉近似） |
| drawAppend 常驻光环 | Buff 视觉挂接缺口（R1-A5 已录） | 跳过 |
| CD 等级递减 45→26s | 等级缩放延后 | 固定 45s |
| MP 450 | 无 MP 系统 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①level col1 = 3000（恒定，无消费者——疑异常触发门槛或护盾参数）；②effect/animation/berserk 镜像副本的引用者；③被拦截状态 3/4/5/9 的精确语义（被击/倒地族，按 0=站立/8=攻击惯例推断）；④BUFF_CAUSE_SKILL/setAppendCauseSkill 追溯链（我们的 Buff 无来源技能记录，无消费场景，不构成缺口）。
- **新系统级缺口上报：受击伤害管线钩子（HP 下限钳制/伤害改写类 Buff）**——`onSetHp` 类"结算前改写"在我们侧无注入点（DamageOwner 直通 NumericSystem）。同类技能预判：狂战系不死/减伤被动、霸体减伤都会撞。建议二档处理：短期 Buff Tick 近似（本档方案）；中期在 LSActionContext.DamageOwner 或 NumericSystem 挂"Buff 改写链"。
- **小门面**：`LSActionContext.GetOwnerMaxHp()`（百分比型 Buff/Action 通用）。
- **新样本**：pushState 状态号 -1 + 技能 ID 绑定（瞬时技能不进状态机的注册形态）——L2 状态号语义补此变体。
- **给下轮的经验**：`defaneblood` 这类"纯 BUFF 瞬时技"识别特征 = pushState 状态号 -1 + 无 .chr 条目 + 无角色 .ani——全机制在 ap_*.nut 的 onSetHp/proc/drawAppend 三回调，直接读 appendage 即全貌，不必找状态机。
