# 破极兵刃（OverDrive）

> 技能ID 38 | 级别 B（武器增益 buff） | 可实现性 ⛔（核心=武器攻击力/物理暴击率增益，属性数值消费链缺失档第 5 实证——buff 挂载可通、增伤端全卡死；光环视觉壳可先行，Khazan/远古记忆同队列） | 分析日期 2026-08-22 | 批次 B2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 破极兵刃 | `skill\Swordman\OverDrive.skl` [name] |
| 英文名 | OverDrive（取 skl 文件名；[name2]="The Overdrive"） | 同上 [name2] 实测 |
| 职业 | 剑魂（[skill fitness growtype]=1；升级上限剑魂/剑影各 20 级） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 30 | 同上 [maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | →→ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | **5000 ms（dungeon）/ 40000 ms（pvp）**—— dungeon 短 CD 是"战斗中反复开启"型增益 | 同上 [dungeon/pvp][cool time] |
| MP | 140 → 1680 | 同上 [consume MP] |
| 读条 | 500 ms（casting time） | 同上 |
| 特殊消耗 | 无（副作用：武器耐久消耗加速 col1——我们无耐久系统，跳过） | 同上 [explain] |
| static data | 无 | 同上实测 |
| 一句话效果 | 武器突破耐久极限 600 秒：增加武器攻击力和物理暴击率，但加快耐久消耗 | 同上 [explain] |

**level property 模板解码（L21 向量法，4 列全明）**（Lv1 → Lv30，dungeon）：

| # | 模板项 | 向量 | Lv1 | Lv30 |
|---|---|---|---|---|
| 1 | 持续时间 | -1,0,0.001 | **600s**（600000 恒定） | 600s |
| 2 | 加快耐久度消耗速度 | -1,1,1.0 | 274% | **-63%（负值！）** |
| 3 | 增加武器攻击力 | -1,2,1.0 | **67** | **1336** |
| 4 | 增加物理爆击率 | -1,3,0.1 | 7.6% | 41.3% |

pvp 表：持续 20s、耐久 300→-53%、攻击力 21→909、物暴 5%→35.5%。

⚠ col1 耐久增速随等级递减并转负（Lv21 起 0、高等级 -63%）——负耐久增速=减速？疑 **mod 数据修正痕迹**
（原版应为全程正值递增），记档存疑。col2 攻击力中段亦有回落（Lv44 起 944→1336 重爬）——跳档同象。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**鬼剑士本尊引擎内置（F3）**：load_state 无注册（grep `overdrive` 零命中）；`sqr\character\swordman\` 全树零命中
（swordman 效果目录无 overdrive 子目录——视觉与 ats 共用，见 §2.4）。

**同型完整参照脚本（F3③，剑魔 ats 版 146）**：`atswordman_load_state.nut:89`：
`IRDSQRCharacter.pushState(10, "character/atswordman/1_swordmaster/overdrive/overdrive.nut", "Overdrive", 146, 146)`——
118+61 行，本批参照质量最高的一个（几乎无混淆）。

### 2.2 参照实现逐回调（atswordman overdrive.nut + ap_overdrive.nut 全读）

```
checkExecutableSkill_Overdrive：sq_IsUseSkill(146) → 切状态 146 子 0
onSetState 子 0：停移动；播施法动画（ani 81；魔剑变体 313）；读条 500ms 折算施法速度
onEnterFrame 帧 4（施法动画中段）：★ 挂 ap_overdrive.nut
      sq_SetValidTime(col0 = 600000ms = 600s) + setBuffIconImage(56)
onEndCurrentAni：回待机 state 0
★ ap_overdrive onStart（buff 生效瞬间）：
  atk  = sq_GetLevelData(146, 2, level)   // 武器攻击力列（ats 布局）
  crit = sq_GetLevelData(146, 1, level)   // 物暴列
  sq_AddChangeStatus("Overdrive", type **15**, false, atk)     // ← 属性键 15 = 攻击力
  sq_AddChangeStatus("Overdrive", type **50**, false, crit×0.1) // ← 属性键 50 = 物理暴击率
  sq_AddEffectFront("character/swordman/effect/animation/atoverdrive/overdrive_bufftop.ani")
  sq_AddEffectBack ("…/overdrive_buffbot.ani")                 // buff 期间常驻光环前后层
onVaildTimeEnd / onEnd：删除前后光环层（buff 到期回收）
```

——**DNF 属性 buff 的标准解剖样本**：`sq_AddChangeStatus(键, 值)` 写属性 + 常驻光环 ani + 到期回收三件套。
伤害端完全由引擎属性系统结算（武器攻击力进武器伤害公式、暴击进 roll）——**这两条消费链我们全缺**（§5）。

（注：ats 版 col1/col2 与剑魂版 [level property] 列序不同（ats：col1=物暴 col2=攻击力；剑魂：col1=耐久 col2=攻击力 col3=物暴）——
两版 skl 各自的模板直读成立；键 15/50 的语义两版一致。）

### 2.3 引擎内置行为重建（剑魂版，推断）

```
施放（→→+Space，读条 500ms，CD 5s）：
  扣 MP；播施法动画（引擎内置姿态——.chr 无 overdrive 条目，疑 Summon2/Attack3 共用，未考证）
  播 overdrive_on_eff.ani（12f 960ms 施放特效）
  帧 4 附近挂 appendage（600s）：武器攻击力 +col2、物暴 +col3、耐久消耗 ×(1+col1/100)
  光环：bufftop/buffbot 前后层常驻（800ms 循环，OverDrive_01.img）
  再施放：刷新/重挂（DNF buff 惯例，引擎内置——未考证）
```

### 2.4 动画与特效

| 件 | 帧数/时长 | 引用 img | 说明 |
|---|---|---|---|
| `effect\animation\atoverdrive\overdrive_on_eff.ani` | 12f 960ms | `Character/Swordman/Effect/ATOverDrive/OverDrive_00.img` | 施放瞬间特效 |
| `overdrive_bufftop.ani` | 8f 800ms（**F? SET FLAG 1**） | `OverDrive_01.img` | buff 常驻前层（ats 脚本引用 swordman 树路径） |
| `overdrive_buffbot.ani` | 7f 800ms | `OverDrive_01.img` | buff 常驻背层 |
| 施法姿态（剑魂版） | 未考证（.chr 无条目） | — | 引擎默认（疑共用动作） |

⚠ 目录名 `atoverdrive` 挂在 **swordman** 效果树下但带 at 前缀——ats 系技能共用剑士视觉树的实例（076 atfrenzy 的
loop 光环同挂 swordman 树）。`.als` 边车：无。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | OverDrive.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\OverDrive.skl` | ✅ 实测（230 行） | 数值（4 列全明） |
| 注册行 | —（鬼剑士无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | F2 三招全查 |
| 参照 nut | overdrive.nut + ap_overdrive.nut | `…\pvf\sqr\character\atswordman\1_swordmaster\overdrive\` | ✅ 实测（118+61 行） | ats 版完整实现 |
| .chr 条目 | —（无） | `…\pvf\character\swordman\swordman.chr`（grep 无） | ⛔ 无 | 姿态引擎默认 |
| 特效 ani | overdrive_on_eff/bufftop/buffbot.ani | `…\pvf\character\swordman\effect\animation\atoverdrive\` | ✅ 实测（3 文件） | §2.4 |
| 特效 img | OverDrive_00.img / OverDrive_01.img | `Character/Swordman/Effect/ATOverDrive/`（ani 引用实证） | ✅ 引用链实证 | 施放/光环 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（grep overdrive 无） | ⛔ 无 | 无攻击 |
| .als | — | atoverdrive 目录 ls | ⛔ 无边车 | — |
| 增益 appendage（剑魂版） | （引擎内置，pvf 无实体） | `…\pvf\appendage\`（无路径不检索） | 未考证 | 攻/暴增益载体 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（施法姿态帧） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | 必需 | ✅ 已在库 |
| OverDrive_00.img | sprite_character_swordman_effect_atoverdrive.NPK | 施放特效 12 帧 | 必需（视觉还原） | ❌ 未入库 |
| OverDrive_01.img | 同上 | 常驻光环前后层 | 必需（视觉还原） | ❌ 未入库 |

缺失 img：必需 2 张（同一 NPK 一次提取）。⛔ 期间挂起。

## 5. 实现方案草案

**⛔ 暂缓（增益消费链）**——与 Khazan（025）/远古记忆/不屈意志同队列（R1-A4 起属性数值无伤害消费链，本技能第 5 实证）：

| DNF 机制 | 我们的现状（代码实测） | 阻断点 |
|---|---|---|
| 武器攻击力 +67→1336（sq_AddChangeStatus 键 15） | `NumericType.Attack(1003)` 键存在但 **MeleeHit 只读固定 `HitReaction.Damage`**（MeleeHitAction.cs:13 实测）——Attack 值没人读 | **属性数值无伤害消费链**（最重缺口）：Attack 键挂上也不改任何伤害 |
| 物理暴击率 +7.6%→41.3%（键 50） | 无暴击概念（无 roll、无倍伤） | 同上 + 暴击子系统（新） |
| 耐久消耗加速 | 无武器耐久系统 | 跳过（副作用本来就不做） |
| 600s 常驻 + 到期回收 | ✅ BuffDefinition TotalTimeMs + RemoveActions | 无 |
| 常驻光环前后层 | Buff 视觉挂接（R1-A5 缺失档；076/084 本批同撞） | 见简化 |

**可先��落地的部分（视觉壳 + 数值占位）**：
- `OverDriveSkill : SkillLogic`——CD 5000（dungeon 直用）、TotalTimeMs=500（读条跳过）；
  `OnCast` `ctx.PlayAnim(施法姿态)` + `ctx.AddBuffToSelf(BuffIds.OverDriveBuff)`。
- `OverDriveBuff : BuffDefinition`——`TotalTimeMs=600000`（col0 直用；demo 可缩 30s）、
  AddActions={��� on_eff 特效 + 挂 bufftop/buffbot 光环}、RemoveActions={撤光环}；
  数值面 `AddActions={AddOwnerNumeric(NumericType.Attack, +1336)}` **可挂但零消费**（诚实占位，等消费链）。
- 注册点：`SkillIds.OverDrive = 32`；`AnimId.OverDriveOnEff = 149`、`OverDriveBuffTop = 150`、
  `OverDriveBuffBot = 151`；`BuffIds.OverDriveBuff = 19` 预留。
- 光环简化：常驻层撞 Buff 视觉挂接——用 Area 视图不可行（非场地）；近似方案=on_eff 单播 + 周期重播（084 同款）。

**关键数值表（若实现）**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 5000ms（dungeon） | 5000 直用 |
| 持续 | 600s（恒定） | demo 30s（便于验证开关） |
| 武器攻击力 | +67→1336（col2） | +100（占位，等消费链） |
| 物暴 | +7.6%→41.3%（col3） | 砍（无暴击系统） |
| 耐久加速 | 274%→-63%（col1，疑 mod 数据） | 跳过（无耐久） |
| 读条 | 500ms | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `OverDrive.skl` | `.skl` 无子命令（4 列 × 30 行） | 手抄 4 值全明；`skl` 子命令同前议 |
| bufftop.ani | `[SET FLAG] 1`（光环层内） | 既有约定整节跳过；非缺口 |
| 三个特效 ani | 常规节（FRAME/DELAY/IMAGE/LOOP） | **全部可译**（实测无规则外表） |

结论：ani 资源全部可被现有 ani 子命令翻译；实质缺口 `.skl` 子命令（1 条，重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 武器攻击力增益（核心） | **缺失档：属性数值消费链**（第 5 实证——远古记忆/不屈意志/流心:狂/Khazan 之后） | ⛔ 主因；等 Attack 键进伤害公式（一次投资全 buff 族解锁） |
| 物理暴击率 | 同上 + 暴击子系统 | 砍 |
| 耐久消耗加速 | 无耐久系统（延后类） | 跳过（副作用不还原反而省事） |
| 600s 长驻 + 反复开启刷新 | 无（TotalTimeMs + 重挂刷新） | 直译 |
| 常驻光环（前后层循环） | **缺失档：Buff 视觉挂接**（R1-A5；本批 076/084 同撞——三技能打包立项的依据） | 单播/周期重播近似 |
| 读条 500ms | 读条系统（延后档） | 跳过 |
| MP 140-1680 | MP 系统（延后档） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. col1 耐久增速高等级转负（Lv21+）——疑 mod 数据修正（原版应恒正递增）；col2 攻击力中段回落跳档同象。
2. 剑魂版施法姿态（.chr 无条目；疑 Summon2/Attack3 共用，未考证）。
3. 剑魂版增益 appendage 实体（pvf\appendage 无路径）。
4. 再施放行为（刷新时长 or 重挂叠加——DNF 惯例刷新，无脚本佐证）。
5. ats 与剑魂版列布局差异下键 15/50 的取数列（两版模板各自直读成立，交叉处未逐值对拍）。

**新系统级缺口（消费方增补）**
1. **属性数值无伤害消费链第 5 实证**——本技能是最纯样本（全部效果=两个属性键）：建议 00-总览 将
   "NumericType.Attack 进 MeleeHit 结算"列为 buff 族解锁的第一杠杆（改动点集中在 MeleeHitAction 一处）。
2. **Buff 视觉挂接第 3 消费方**（076/084/038 本批三例打包）：`sq_AddEffectFront/Back` 常驻层模式
   （挂/撤两阶段）是 DNF buff 视觉的标准形态，建议 BuffDefinition 加 ViewAnimId/ViewBackAnimId 虚属性
   （AreaDefinition 同构，框架改动小）。

**翻译工具缺口**：`.skl` 子命令（重复印证）；无新节。

**给下轮的经验**：剑魂武器增益系（破极兵刃/里鬼剑术等）的属性写入全走 `sq_AddChangeStatus(键 15/50)`
（ats 参照可读性最好）；视觉挂在 `effect\animation\at*` 前缀目录但物理上在 swordman 树——ats 系与剑士系
共享特效树的实例，查资源别被 at 前缀骗去 atswordman 树找。
