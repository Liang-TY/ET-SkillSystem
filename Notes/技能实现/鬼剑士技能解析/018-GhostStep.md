# 鬼影步（GhostStep）

> 技能ID 18 | 级别 B（**预判 A 纠偏：非位移攻击技，是自增益 buff 技**） | 可实现性 ⛔（三主干全撞系统缺口，深简化壳见 §5） | 分析日期 2026-08-22 | 批次 A7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼影步 | `skill\Swordman\GhostStep.skl` [name] |
| 英文名 | GhostStep（取 skl 文件名；[name2] 实测为英文别名 `Shadow pace`，本 pvf 少见的例外） | 同上 [name2] |
| 职业 | 鬼泣（[skill fitness growtype]=2，L17 映射） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 30（growtype 上限：仅鬼泣 20） | 同上 [maximum level] / [growtype maximum level] `0 0 20 0 0 0` |
| 类型 | 主动（active，skill class 3） | 同上 [type] |
| 指令 | ↑↓ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 5000 ms（固定） | 同上 [dungeon][cool time] `5000 5000` |
| MP | 240 → 816（Lv1→30） | 同上 [dungeon][consume MP] |
| 施法时间 | 500 ms（[casting time]，Buff 类标准吟唱条） | 同上 |
| static data | `500 300`（推断：500=施法时间对应值；**300=前冲无敌时间 300ms**——explain 明言"前冲的一定时间段内会进入无敌状态"，数值语义无 nut 佐证） | 同上 [static data] |
| 一句话效果 | 借鬼神之力提升移动速度（持续 600s），前冲起跑 300ms 内无敌；普攻/前冲攻击转为暗属性魔法攻击；与冥炎卡洛（82）共存时普攻变卡洛暗炎连击 | 同上 [basic explain]/[explain] + 走读（§2.2） |

**level property（11 列，Lv1 → Lv30 首末值）**：`600000(恒)`、`1000(恒)`、`110→700`、`294→1258`、`338→1447`、`397→1698`、`441→1887`、`368→1573`、`0(恒)`、`0(恒)`、`100→690`。

**已实证列**（来自 `swordman_throw.nut` case 18，§2.2）：
- **col0 = 600000 = 持续时间 ms（600s）**——`time = sq_GetLevelData(18, 0, level); appendage.sq_SetValidTime(time)` 直读；
- **col10 = 100→690 = 增益值**——`bonus = sq_GetLevelData(18, 10, level)` 存入 appendage 变量 `ap`（消费方为引擎内置，见 §2.3）。
其余列（col1 恒 1000、col2-7 成长列、col8/9 恒 0）语义未考证（ap_ghoststep.nut 为空壳，全部由引擎消费；按 DNF 惯例 col2-7 疑为移动速度/前冲攻击力/卡洛加成等万分率向量，**推断**）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无 pushState 注册**（grep `ghoststep` 无命中）——鬼影步是老一代 buff 技能，施法走**引擎内置 throw（施法）状态 13**（load_state 行 117：`pushState(..., "Character/swordman/swordman_throw.nut", "swordman_throw", 13, -1)`），挂 buff 的逻辑写在共享的 throw 回调里。

**appendage 无独立行为脚本**：`appendage\ap_ghoststep.nut` 全文 4 行，`sq_AddFunctionName` 空注册（无任何回调）——buff 效果（移速/无敌窗口/属性转换）**全部引擎数据驱动**，pvf 侧零逻辑。

```
// sqr/character/swordman/swordman_throw.nut（共享 throw 状态回调，case 18 分支）
case 18:
    if (已挂 ap_ghoststep) 移除旧的;                     // 重复施放刷新
    time   = sq_GetLevelData(18, 0,  skill_level);       // col0 = 600000ms 持续
    bonus  = sq_GetLevelData(18, 10, skill_level);       // col10 = 增益值
    appendage = sq_AppendAppendage(obj, obj, 18, true, "character/swordman/appendage/ap_ghoststep.nut", true);
    appendage.sq_SetValidTime(time);                     // 600s 有效期
    appendage.getVar("ap").push_vector(bonus);           // 增益值存变量
```

### 2.2 主流程（引擎内置 + throw 回调拼合还原）

1. **施法**：按 ↑↓+Space → 进 throw 状态 13，播 `Throw1.ani`/`Throw2.ani`（.chr `[throw motion 1-1]/[1-2]`，实测 6 帧 300ms / 11 帧 600ms，sm_body 图集），施法条 500ms。
2. **挂 buff**（onAfterSetState_swordman_throw case 18）：移除旧 ap_ghoststep → 挂新 appendage（600s，ap=增益值）。
3. **buff 生效（引擎内置，数据驱动）**：
   - 移动速度提升（consume col 系列之一 + ap 变量）；
   - 前冲（Dash 状态）起跑 **300ms 无敌窗口**（static data[1]，OBJECT_MESSAGE_INVINCIBLE 同族机制）；
   - 普攻/前冲攻击 → **暗属性魔法攻击**（explain 明言；元素转换引擎内置）；
   - **卡洛联动**（`kalla.nut` onProcCon + `swordman_common.nut` procAppend STATE_ATTACK 分支，均实测）：ap_kalla 与 ap_ghoststep 同时在身 → 普攻直接转投**卡洛状态 44**（暗炎剑连击，技能 82 冥炎卡洛的攻击形态）——"使用卡洛可增加普攻和前冲攻击攻击力"的脚本实证。
4. **到期**：appendage 600s 失效 → 一切效果消失（ghostend 视觉）。

### 2.3 appendage / 前冲攻击资产

- `ap_ghoststep.nut`：空壳（§2.1）。
- 前冲攻击专档（引擎在 buff 期内切换 dash attack）：
  - `animation\GhostStepSlashReady.ani` / `GhostStepSlashMove.ani`（.chr etc motion 槽 #18/#19，实测行 991/992）+ `attackinfo\GhostStepSlash.atk`（etc attack info #21）；
  - 特效 `effect\animation\ghoststepslash\`：slash1/slash2（k-light-1/2.img）、skull（k-soul.img）、move（k-speed.img）、dust（借 HardAttackCharge\dust.img，L14 跨目录复用又一例）。
- buff 视觉 `effect\animation\ghoststep\`：appear1/2（施放现身 6 帧 540ms）、stay1/2（循环鬼影 8 帧 720ms **LOOP**）、disappear1/2、ghost1/2/3（invincible-process.img——**无敌窗口专用视觉**）、ghostend（invincible-resolutive.img）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\swordman\animation\Throw1.ani | 6 | 300ms | 无 | 无 | 施法动作 1（sm_body 图集） |
| character\swordman\animation\Throw2.ani | 11 | 600ms | 无 | 无 | 施法动作 2（sm_body） |
| character\swordman\animation\GhostStepSlashReady.ani / Move.ani | 未逐帧 | — | 未提取 | 未提取 | 前冲攻击动作（本次未走读，buff 期 dash attack 档） |
| effect\...\ghoststep\appear1.ani | 6 | 540ms | 无 | 无 | 现身特效（ghost-body.img） |
| effect\...\ghoststep\stay1.ani | 8 | 720ms | 无 | 无 | 循环鬼影（LOOP=1） |
| effect\...\ghoststep\ghostend.ani | 6 | 720ms | 无 | 无 | 到期收尾 |

`.als` 边车：throw1/throw2/ghoststep 系列均无（ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GhostStep.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GhostStep.skl` | ✅ | 技能数据（CD/施法/11 列等级数据/static） |
| 注册行 | 无独立注册（throw 状态 13 共享） | `…\pvf\sqr\character\swordman_load_state.nut` 行 117 | ✅ | throw 状态注册（技能 ID -1 不绑定） |
| 主逻辑 | swordman_throw.nut case 18 | `…\pvf\sqr\character\swordman\swordman_throw.nut` 行 30-41 | ✅ | 挂 appendage（§2.1 代码块） |
| appendage | ap_ghoststep.nut | `…\pvf\sqr\character\swordman\appendage\ap_ghoststep.nut` | ✅（空壳 4 行） | 效果全引擎内置 |
| 联动 | kalla.nut（ap 检测→状态 44） | `…\pvf\sqr\character\swordman\kalla\kalla.nut` 行 44-75 | ✅ | 卡洛普攻变身判定 |
| 联动 | swordman_common.nut（STATE_ATTACK 分支） | `…\pvf\sqr\character\swordman\swordman_common.nut` 行 248-258 | ✅ | 普攻转卡洛状态 44 |
| .chr 条目 | throw motion 1-1/1-2（行 928-932）；etc motion #18/#19（行 991/992）；etc attack info #21（行 1315） | `…\pvf\character\swordman\swordman.chr` | ✅ | Throw1/2.ani、GhostStepSlashReady/Move.ani、GhostStepSlash.atk |
| 角色 .ani | Throw1/Throw2.ani、GhostStepSlashReady/Move.ani | `…\pvf\character\swordman\animation\` | ✅ | 施法 + 前冲攻击动作 |
| 角色 .atk | ghoststepslash.atk | `…\pvf\character\swordman\attackinfo\` | ✅（未细读） | 前冲攻击命中反应 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失 | — |
| 特效 .ani | ghoststep\*.ani ×12、ghoststepslash\*.ani ×5 | `…\pvf\character\swordman\effect\animation\` | ✅ | buff 视觉全家桶 |
| 装备层 | throw*.ani ×232（含各 avatar 层） | `…\pvf\equipment\character\swordman\avatar\` | ✅（find 计数） | 施法动作换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法/前冲动作图集（Throw/Slash 均 sm_body%04d） | 必需（共享） | ✅ 已在库 |
| Character\Swordman\Effect\GhostStep\ghost-body.img | sprite_character_swordman_effect_ghoststep.NPK | 现身/循环鬼影/斩击主体 | 必需（buff 视觉） | ❌ |
| Character\Swordman\Effect\GhostStep\ghost-light.img | 同上 | 鬼影发光层 | 必需 | ❌ |
| Character\Swordman\Effect\GhostStep\invincible-process.img | 同上 | **无敌窗口视觉**（前冲残影） | 可选（无敌做不了则弃） | ❌ |
| Character\Swordman\Effect\GhostStep\invincible-resolutive.img | 同上 | 到期收尾 | 可选 | ❌ |
| Character\Swordman\Effect\GhostStep\k-light-1/2.img、k-soul.img、k-speed.img | 同上 | 前冲攻击斩击特效 | 可选 | ❌ |
| Character\Swordman\Effect\HardAttackCharge\dust.img | sprite_character_swordman_effect_hardattackcharge.NPK | 斩击尘土（L14 跨技能借图） | 可选 | ❌ |

缺失 img：必需级 2 张（ghost-body/ghost-light，同 NPK 一次提取全覆盖），可选级 5 张。

## 5. 实现方案草案（⛔ 级，深简化"演示壳"方案——立项前不做）

> 三主干（移速/前冲无敌/暗属性转换）**全部**撞系统缺口（§7），本节只给"buff 时序 + 视觉"演示壳：

- **内容件**（全部继承真实基类）：
  - `GhostStepSkill : SkillLogic`——OnCast：`ctx.PlayAnim(AnimId.SwordmanThrow)`（throw1）+ `ctx.AddBuffToSelf(BuffIds.GhostStep)`；`CooldownMs=5000`、`TotalTimeMs=300`（施法动画 300ms，播完 OnEnd 回默认动画——**无施放后僵直机制，见 §7**）。
  - `GhostStepBuff : BuffDefinition`——`TotalTimeMs=600000`、`AddActions={AddGhostStepSpeedAction}`、`RemoveActions={RemoveGhostStepSpeedAction}`（数值增减对偶，BleedBuff 同族范式）。
  - `AddGhostStepSpeedAction / RemoveGhostStepSpeedAction : LSAction`——`ctx.AddOwnerNumeric(NumericType.SpeedPct, +10 / -10)`（demo 建议值；DNF 原值 col 系未考证）。**前提：移动系统消费 Speed（现缺口，§7-1）**。
- **概念映射**：throw 状态 13 → SkillLogic 施法；ap_ghoststep 有效期 → BuffDefinition.TotalTimeMs；ap 变量增益 → NumericType.SpeedPct；invincible-process 特效 → 无敌窗口（缺失，跳过）。
- **注册点**：SkillIds.GhostStep=15、BuffIds.GhostStep=7、AnimIds 59-62（Throw/Appear/Stay/Disappear）；json 注册 throw1 + ghoststep 特效 3 个；图集 ghoststep NPK 2 张必需；按键 ButtonToSkill case 8。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 5000ms | 5000（直用） |
| 施法 | 500ms 吟唱 + Throw1 300ms | 300（无吟唱条系统） |
| 持续 | 600000ms（col0 实证） | 60000（10 倍缩短，演示可见到期） |
| 移速增益 | col 系未考证（DNF 惯例万分率） | SpeedPct +10%（数值可写，消费缺口修后生效） |
| 前冲无敌 | 300ms（static[1]，推断） | 不做（无敌帧缺口） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| Throw1/Throw2.ani、ghoststep\*.ani、ghoststepslash\*.ani | 节面常规（FRAME/IMAGE/DELAY/DAMAGE BOX），本次未发现规则外节 | **现有 ani 子命令全覆盖**（[LOOP] 已支持） |
| GhostStep.skl | `.skl` 无子命令（11 列 level info + static data） | 并入既有 `.skl` 子命令缺口（轮间经验已记档） |
| ghoststepslash.atk | `.atk` 无子命令 | 并入既有 `.atk` 缺口 |

本技能无 .als、无 .ptl、无 .obj——翻译面干净，计 2 条既有缺口。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 移动速度提升 | **属性数值消费链缺失（新证：Speed 侧）**——NumericType.Speed/SpeedAdd/SpeedPct 五层公式框架存在，但 `LSInputComponentSystem` 移动计算硬编码 6 单位/s（实测行 62 `input.V * 6 * 50 / 1000`），**无任何代码读 Speed** | 补移动消费（一行改速度源）后 SpeedPct 即活；与 R1-A4 伤害侧缺口同族，建议并入"属性数值消费链"统一立项 |
| 前冲 300ms 无敌（static[1]） | **无敌帧**（缺失档，R1-A5 已记档；且我们无前冲状态机，"前冲起跑窗口"无从谈起） | 跳过；invincible-process.img 视觉一并弃 |
| 普攻/前冲攻击转暗属性魔法 | **元素属性系统**（缺失档）+ 无伤害类型通道 | 跳过 |
| 与卡洛（82）联动：普攻变卡洛暗炎（状态 44） | Buff 查询门面（缺失）+ 技能取消体系（缺失）+ 卡洛本体未实现 | 跳过（卡洛是独立技能，E 批另行分析） |
| 重复施放刷新时长 | Buff 叠层简版已有"同 Buff 再挂=刷新时长"（BuffDefinition 语义） | ✅ 天然支持，无需处理 |
| buff 期鬼影视觉跟随 | **Buff 视觉挂接**（缺失，R1-A5 drawAppend 光环类同档）——BuffDefinition 无视图通道 | 深简化：施法瞬间一次性 overlay（appear 动画）替代循环跟随层 |

## 8. 存疑与缺口上报

**未考证项**
1. level property col1-9 语义（col1 恒 1000、col2-7 成长、col8/9 恒 0）——ap_ghoststep 空壳、无 nut 消费佐证。
2. static data `500 300` 精确语义（300=前冲无敌时间为高置信推断，500 未解）。
3. ghoststepslash.atk 具体命中参数（未细读——前冲攻击是 buff 期衍生档，独立于本技能主干）。
4. throw 状态实际播 Throw1 还是 Throw2（引擎按施法类型选择，未考证）。

**新系统级缺口（§6.3 清单外）**
1. **移动速度属性消费链缺失**（本批新证，归属 R1-A4"属性数值无伤害消费链"的姊妹项）：NumericType.Speed 五层公式在库但移动计算硬编码。建议总览汇总时把 R1-A4 那条扩写为"属性数值无消费链（伤���端 + 移动端两例）"。

**给下轮的经验**：鬼剑士自增益类技能（buff/形态）先查 `sqr\character\swordman\swordman_throw.nut` 的 switch(getThrowIndex()) ——引擎 throw 状态 13 是所有"吟唱挂 buff"技能的公共入口（47 波动印记/82 卡洛/18 鬼影步同槽），挂表逻辑全在这一个文件里；appendage 目录下的空壳 nut = 效果引擎数据驱动的直接判据。
