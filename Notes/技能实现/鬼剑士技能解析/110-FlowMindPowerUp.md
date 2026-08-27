# 流心 : 狂（FlowMindPowerUp）

> 技能ID 110 | 级别 B（预分类 A，**纠偏**：增益状态技，无独立攻击行为） | 可实现性 🔶（buff 本体可做，增伤消费卡死） | 分析日期 2026-08-22 | 批次 A9

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 流心 : 狂 | `skill\Swordman\FlowMindPowerUp.skl` [name] |
| 英文名 | FlowMindPowerUp（取 skl 文件名；[name2]="Flow Heart : Strong"） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1） | 同上 |
| 学习等级 | 30（前置：流心 105 Lv1） | 同上 |
| 最高等级 | 20（剑魂段上限 10） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active）——**实际为增益状态技**（指令类型 BUFF） | 同上 [type] / [command] `{6=(BUFF)}` |
| 指令 | （流心动作中）space（BUFF 键，`flowmind.nut` 实测读 `OPTION_HOTKEY_SKILL2`） | 同上 [command key explain] + 代码实证 |
| CD | 5000ms（pvp 10000ms） | 同上 [cool time] |
| MP | 20 → 168（Lv1→Lv20） | 同上 [consume MP] |
| 特殊消耗 | 维持 MP 10.0→60.0（持续耗蓝） | 同上 [maintain MP] |
| 一句话效果 | 流心架势中蓄气 0.3s 后，一段时间内 [流心:刺/跃/升] 攻击力 +30%~92%（按等级），并解锁"跃中按 Z 取消进升" | 同上 [explain] + 代码走读（§2.2） |

**level property 解码**（模板 3 int + 2 float ↔ 5 行向量，L21/L8 法）：

| 模板占位 | 向量 | 解读 | 值 |
|---|---|---|---|
| 刺攻击力增加率 `<int>`% | `(-1, 0, 1.0)` | level col0 | 30% → 92% |
| 跃攻击力增加率 `<int>`% | `(-1, 1, 1.0)` | level col1 | 30% → 92%（三列同值） |
| 升攻击力增加率 `<int>`% | `(-1, 2, 1.0)` | level col2 | 30% → 92% |
| 蓄气时间 `<float1>`秒 | `(0, 0, 0.001)` | static[0]=300 × 0.001 | 0.3 秒 |
| 效果持续上限 `<float1>`秒 | `(1, 1, 0.001)` | static[1]=0（pvp=10000） | 地下城 ∞ / pvp 10 秒 |

static data：地下城 `300 0`、pvp `300 10000`——全列被 level property 覆盖，**无未解码列**（本技能 static 解码完整度高）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无 pushState、无独立状态、无角色动画、无 .atk、无被动对象**（全部实测：load_state 按名/按号 110 无命中；`character\swordman\animation\` 无 flowmindpowerup 文件；attackinfo 无同名；passiveobject 无）。本技能是**挂在流心架势状态（61，技能 105）onProc 上的引擎内置增益**：

```
142: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/weaponmaster/flowmind/flowmind.nut", "FlowMind", 61, 105);
```

### 2.2 施法与效果链（flowmind.nut onProc_FlowMind 实测全文归纳）

```
onProc_FlowMind（流心架势中每帧）：
  fmjt = sq_GetIntData(obj, 110, 1)        // 技能 110 static[1] = 0（地下城）
  若按住 OPTION_HOTKEY_SKILL2（space/BUFF 键）：
    移除已有 ap_liuxing → 重新挂 ap_liuxing appendage
    appendagegw.sq_SetValidTime(fmjt)      // 有效期 0 = 无限
```

- **触发**：架势中按住 space（蓄气 0.3s 的门槛由引擎/CD 门禁处理，脚本仅按键即挂）。
- **效果载体**：appendage `character/swordman/weaponmaster/flowmind/ap_liuxing.nut`——实测 4 行空壳（只注册 onEnd/isEnd 回调名 "BAOJI"，本文件无实现，同 L20 ap_bloodboom 空壳形态）。**增伤数值（技能 110 level col0/1/2）的消费完全在引擎内部**：白名单内无任何脚本读取这三列；引擎在结算刺/跃/升伤害时检测该 appendage 存在并放大。
- **交叉实证（消费方之一）**：`weaponmaster\flowmind\flowmindtwo.nut` onProc_FlowMindTwo——跃（状态 63）中若挂 ap_liuxing 且按技能键且 109 不在冷却 → `sq_AddSetStatePacket(64, [0])` 切流心:升。即狂 buff 同时是"跃中取消进升"的解锁开关。
- CD 5000：由引擎技能系统按 [cool time] 应用（激活即进 CD）。

### 2.3 特效

`character\swordman\effect\animation\flowmindpowerup\`：`charge_dodge.ani`（6 帧 **LOOP**，蓄力循环光）、`flash_dodge.ani`（11 帧，蓄满闪光）、`flow_mind_power_up_hit_effect.ani`（4 帧，命中特效——**无脚本引用**，推断为引擎在被增益技能命中时绘制）。角色本体无专用动画（保持架势 stay）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMindPowerUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMindPowerUp.skl` | ✅ | 技能数据（本批 static 解码最完整） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（挂在 105 枢纽状态上） | §2.1 |
| 触发 nut | flowmind.nut | `…\pvf\sqr\character\swordman\weaponmaster\flowmind\flowmind.nut` | ✅（43 行） | 架势 onProc：space 按住挂 ap_liuxing（§2.2） |
| appendage | ap_liuxing.nut | `…\swordman\weaponmaster\flowmind\ap_liuxing.nut` | ✅（4 行空壳） | 增益标记本体（效果引擎内部） |
| 消费方 | flowmindtwo.nut | `…\swordman\weaponmaster\flowmind\flowmindtwo.nut` | ✅ | 跃中检测 buff → 解锁取消进升 |
| .chr 条目 | — | `…\pvf\character\swordman\swordman.chr` | ⛔ 无（无角色动画/攻击信息） | — |
| 角色 .ani | — | `…\pvf\character\swordman\animation\` | ⛔ 无专用动画 | 保持架势动画 |
| .atk | — | `…\pvf\character\swordman\attackinfo\` | ⛔ 无（增益技） | — |
| .als | — | 同上 | ⛔ 无 | — |
| 特效 .ani | charge_dodge / flash_dodge / flow_mind_power_up_hit_effect | `…\pvf\character\swordman\effect\animation\flowmindpowerup\` | ✅ 实测 | 蓄力/闪光/命中特效（_ds 剑影变体跳过） |
| 装备层 | — | `…\pvf\equipment\character\swordman\avatar\` | ⛔ 无（find 计数 0） | 无角色动画故无图层 |
| 同族 | FlowMind.skl(105)/FlowMindOne(107)/Two(108)/Three(109) | `…\pvf\skill\Swordman\` | ✅ 存在 | 枢纽与被增益技能（109 本批另档） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| charge_dodge.img | sprite_character_swordman_effect_flowmindpowerup.NPK | 蓄力循环光（buff 存续期视觉） | **必需** | ❌ 未入库 |
| flash_dodge.img | 同上 | 蓄满闪光 | 可选 | ❌ 未入库 |
| flow_mind_power_up_hit_effect.img | 同上 | 增益命中特效（引擎时机） | 可选（消费端缺失，先不提） | ❌ 未入库 |

缺失 img：必需级 1 张（同一 NPK）。

## 5. 实现方案草案

**先说结论（哪半能做、哪半卡死）**：
- ✅ **能做**：施放（独立按键直发，砍掉架势前置）→ 给自己挂 `FlowMindRage` Buff + 蓄力循环视觉（视觉挂接用"纯视觉 Area"绕开 BuffDefinition 无视图通道的缺口，见下）+ CD 5000 门禁。
- ⛔ **卡死**：①**增伤消费**——`MeleeHitAction` 只读来源 `HitReaction.Damage` 固定值（`DotNet~\Actions\MeleeHitAction.cs:14` 实证），NumericType 五层公式存在但伤害端不查询，"刺/跃/升 +30%~92%"无处生效（缺口累计在案"属性数值无伤害消费链"，R1-A4）；②**解锁消费**——SkillContext 无 HasBuff 查询门面（只有 AddBuff/AddBuffToSelf），"跃中按 Z 取消进升"的检测无从写起（"Buff 查询门面"缺口在案），且跃/升本身也依赖取消体系。
- 即：**buff 本体与视觉是现成的，全部两条效果消费链都卡系统级缺口**——demo 只能做"空转 buff"（挂载可见、数值无效），真正完工依赖伤害公式接数值系统（建议先于单个增益技立项，缺口已累计）。

### 内容件清单

1. **`DotNet~/Skills/FlowMindPowerUpSkill.cs : SkillLogic`**
   - `CooldownMs = 5000`（DNF 原值直用）；`TotalTimeMs = 500`（无角色动画，短施法）。
   - `OnCast`：`ctx.AddBuffToSelf(BuffIds.FlowMindRage)` + `ctx.CreateAreaInFront(AreaIds.FlowMindRageAura, FP.Zero)`（distance=0 即落在自身位置——SkillContext 无位置 getter，用此门面；纯视觉区，见下）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`（施法期间沿用当前动画即可）。
2. **`DotNet~/Buffs/FlowMindRageBuff.cs : BuffDefinition`**（同 StunBuff 配置形态）
   - `TotalTimeMs = 10000`（DNF 地下城原值 ∞、pvp 10000——取 pvp 有限值，避免永久 buff 无移除路径）；`TickTimeMs = 0`；无 Actions（数值消费缺失，空挂）。
3. **`DotNet~/Areas/FlowMindRageAura.cs : AreaDefinition`**（FireCircleArea 视觉范式）
   - `TotalTimeMs = 10000`（与 buff 同寿）、`TickTimeMs = 0`、无 Enter/ExitActions、`HalfExtents` 极小（无判定，纯视觉锚点）、`ViewAnimId = AnimId.FlowMindPowerUpCharge`（charge_dodge 循环）——**Buff 视觉挂接缺口的既定绕法**（R1-A5 缺口记档同源）。
4. **无需新 Action**（无任何效果节点可写——正是卡死的体现）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| ap_liuxing appendage（无限时） | `FlowMindRageBuff`（10s 有限时近似） |
| 引擎读 110 level col0/1/2 放大刺/跃/升伤害 | ⛔ 无对应（MeleeHit 不读数值）——等伤害公式接 NumericType |
| 跃中检测 buff 解锁取消 | ⛔ 无 HasBuff 门面 + 无取消体系 |
| space 按住蓄气 0.3s | 无按住输入（缓冲为按下沿）→ 直发近似 |
| charge_dodge 蓄力循环视觉 | 纯视觉 Area（ViewAnimId 循环） |
| 维持 MP | 无 MP 系统，跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.FlowMindPowerUp = 16` + ButtonToSkill case 8 |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `FlowMindPowerUpCharge = 63`（charge_dodge）、`FlowMindPowerUpFlash = 64`（flash_dodge，可选注册） |
| BuffId | `Packages\cn.etetet.skill\Runtime\BuffDefinition.cs` | `BuffIds.FlowMindRage = 7` |
| AreaId | `Packages\cn.etetet.skill\Runtime\AreaDefinition.cs` | `AreaIds.FlowMindRageAura = 4` |
| json/图集 | `…\LSAnimClipRegistrar.cs` / `…\LSAnimResComponentSystem.cs` | charge_dodge.json + charge_dodge.img.bytes（必需）；flash 可选 |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 8 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 5000ms | 5000（直用） |
| 蓄气时间 | 0.3s（static[0]=300） | 0（无按住输入，直发） |
| 增伤 | 刺/跃/升 +30%→92%（level col0/1/2） | 无效果（消费链缺失，空挂） |
| 持续 | ∞（地下城）/ 10s（pvp，static[1]） | 10000ms |
| 维持 MP | 10→60/s | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| FlowMindPowerUp.skl | `.skl` 无子命令（2 列 static + 3 列等级表 + maintain MP） | 手抄 6 值可行（既有记档） |
| charge_dodge.ani / flash_dodge.ani | 仅常规节（FRAME MAX/FRAMExxx/LOOP/SHADOW） | `ani` 子命令全覆盖；LOOP/SHADOW 均已支持/记档，无新缺口 |

本技能翻译面干净：**全部资源可被现有 ani 子命令翻译**，缺口仅 .skl 既有项。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 增益后刺/跃/升伤害 +30%~92% | **属性数值无伤害消费链**（缺失档，R1-A4 累计——本批最重卡点） | buff 空挂（挂载/时长/视觉正确）；数值生效等 MeleeHit 接 NumericType 伤害公式（系统投资，建议优先立项） |
| 跃中按 Z 取消进升（buff 解锁） | Buff 查询门面缺失（R1-A5/064 累计）+ 技能取消体系缺失 | 跳过（流心连招体系整体延后） |
| 只能在流心架势中发动 | 状态前置缺失（R1-A1 累计） | 独立按键直发 |
| 按住 space 蓄气 0.3s | 输入缓冲只有按下沿，无按住语义 | 直发 |
| 维持 MP 10-60/s | MP 系统延后 | 跳过 |
| 地下城持续 ∞ | 永久 buff 无外部移除路径（LSBuff 0=永久靠外部移除，无驱散 API） | 取 pvp 10s 有限时 |

## 8. 存疑与缺口上报

- **未考证**：①引擎在何处/如何读 110 level 三列放大伤害（引擎内部，白名单无脚本可证——结论由"ap_liuxing 空壳 + 全树无消费脚本"反推）；②CD 起算时机（推断=激活即 CD）；③flow_mind_power_up_hit_effect.ani 的绘制时机（推断为增益命中时引擎绘制）。
- **新系统级缺口**：无新增（伤害消费链/Buff 查询门面/取消体系/状态前置/永久 buff 驱散均已在案）。
- **翻译工具缺口**：无新增（.skl 既有项）。
- **给下轮的经验**：本技能是"buff 半成品"的标准样本——遇到增益类技能先查三件事：appendage nut 是否空壳（空壳=效果在引擎）、伤害端是否有消费脚本、SkillContext 是否有查询门面；三者都空就直接按 §5"能做/卡死"两段写。纯视觉 Area（无 Actions + ViewAnimId 循环）是 Buff 视觉的现成绕法。
