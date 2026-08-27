# 残影之凯贾（Keiga）

> 技能ID 84 | 级别 B（减伤护体光环） | 可实现性 ⛔（物理/魔法减伤撞受击伤害管线钩子【第 5 消费方】+ 回避率无机制 + 吸收次数需受击计数；视觉壳与吸收计数逻辑可先行） | 分析日期 2026-08-22 | 批次 B2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 残影之凯贾 | `skill\Swordman\Keiga.skl` [name] |
| 英文名 | Keiga（取 skl 文件名；[name2]="Keiga"） | 同上 [name2] 实测 |
| 职业 | 鬼泣（[skill fitness growtype]=2；升级上限鬼泣/剑影各 10 级） | 同上 |
| 学习等级 | 20 | 同上 [required level] |
| 最高等级 | 20 | 同上 [maximum level] |
| 类型 | active（**skill class 3 = 召唤/增益类**） | 同上 [type]/[skill class] |
| 指令 | ↓→ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 30000 ms | 同上 [dungeon][cool time] |
| MP | 120 → 616 | 同上 [consume MP] |
| 读条 | 800 ms（casting time） | 同上 |
| 特殊消耗 | 无 | 同上 |
| static data | `500`（1 值，语义未考证；疑触发检查间隔或残影位参数） | 同上 |
| 一句话效果 | 召唤鬼神凯贾残影环绕自身：减少受到的物理/魔法伤害并增加回避率；期间[鬼影闪]增伤；吸收一定次数伤害或超时后消失 | 同上 [explain] |

**level property 模板解码（L21 向量法，6 列全明——本批解码最干净的一张表）**（Lv1 → Lv20，dungeon）：

| # | 模板项 | 向量 | Lv1 | Lv20 |
|---|---|---|---|---|
| 1 | 物理伤害减少比率 | -1,0,1.0 | **15%** | **47%** |
| 2 | 魔法伤害减少比率 | -1,1,1.0 | **15%** | **47%** |
| 3 | 增加回避率 | -1,2,0.1 | 0.5% | 10% |
| 4 | 持续时间 | -1,3,0.001 | **600s**（600000 恒定） | 600s |
| 5 | 吸收伤害次数上限 | -1,4,1.0 | 30 次 | 68 次 |
| 6 | [鬼影闪]附加伤害 | -1,5,1.0 | 180% | 370% |

pvp 表：持续 30s、减伤 8→23%/8→23%、回避同、吸收 13→32 次、鬼影闪 90→185%。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**引擎内置（F3）**：load_state 无注册（grep `keiga` 零命中）；`sqr\character\swordman\` 全树、atswordman/jg_swordman
全查零命中——无参照脚本（鬼泣鬼神召唤族惯例，025-Khazan 同型：召唤+光环全引擎）。
与 Khazan 不同点：Khazan 是**场地领域**（PO 20012），Keiga 是**跟随自身的护体残影**——无 passiveobject
（passiveobject 根/animation/action 三处 ls grep `keiga` 零命中），残影视觉全靠 `effect\animation\keiga\`。

### 2.2 引擎内置行为重建（.skl + 特效结构推断）

```
施放（↓→+Space，读条 800ms，CD 30s）：
  扣 MP；播召唤姿态（[throw motion 2-1/2-2]=Summon1/2，推断同 Khazan）
  播残影出现特效（appear1-4.ani 12f 1080ms）
  挂"凯贾"护体 appendage（引擎内置，持续 600s）：
    ① 物理/魔法伤害 -15%~47%（受击结算管线内减免——getImmuneTypeDamageRate 族，001-Guard 同位）
    ② 回避率 +0.5%~10%（受击命中判定前 roll——引擎侧）
    ③ [鬼影闪]（技能 50）伤害 +180%~370%（跨技能增伤查询）
    ④ 吸收计数：受击 col4 次（30~68）后残影破碎（break 特效 + 移除 appendage）
    ⑤ 600s 超时同样移除（disappear 特效）
  常驻视觉：loop1-4.ani（6f 540ms 循环——上下两道残影环绕）
```

### 2.3 特效结构（16 个 ani 实测——残影四态 × 上下/明暗四变体）

| 组 | 文件 | 帧/时长 | 引用 img | 说明 |
|---|---|---|---|---|
| 出现 | appear1.ani | 12f 1080ms | **GhostStep/ghost-body.img** + k_down_n.img | 残影显形（1=下·普通层） |
| 出现 | appear2.ani | 12f 1080ms | ghost-light.img + k_down_d.img | （2=下·加亮层） |
| 出现 | appear3/4.ani | 12f 1080ms | k_up_n / k_up_d | （3/4=上·两变体） |
| 常驻 | loop1-4.ani | 6f 540ms | k_down_n / k_down_d / k_up_n / k_up_d | **循环**——上下残影各两层 |
| 破碎 | break1-4.ani | 5f 450ms | 同名四 img | 吸收次数耗尽 |
| 消失 | disappear1-4.ani | 6f 450ms | 同名四 img | 超时自然消散 |

——命名规律（实测 img 对位）：`k_up/down` × `_n/d` = 上身/下身 × 普通层(n)/加亮层(d，LINEARDODGE 类混色疑)。
**全部 img 与鬼影步（18-GhostStep）共用 `Character/Swordman/Effect/GhostStep/` 图集**（凯贾残影=鬼影系视觉）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| appear1-4.ani | 12 | 1080ms | 无 | 无 | ghost-body/light + k_* |
| loop1-4.ani | 6 | 540ms 循环 | 无 | 无 | 常驻光环 |
| break1-4.ani | 5 | 450ms | 无 | 无 | 破碎 |
| disappear1-4.ani | 6 | 450ms | 无 | 无 | 消散 |
| Summon1/2.ani（施法姿态共用） | 3/12 | 150/600ms | Summon2 F9=65534 | — | 025-Khazan 已验 |

`.als` 边车：keiga 目录无（ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Keiga.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Keiga.skl` | ✅ 实测（144 行） | 数值（6 列全明） |
| 注册行 | —（无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | F2 三招全查 |
| 主 nut | —（不存在，无参照） | `…\pvf\sqr\character\swordman\` + ats/jg 全查 | ⛔ 缺失 | 引擎内置 |
| 施法姿态 | Summon1/2.ani（[throw motion 2-1/2-2] 共用） | `…\pvf\character\swordman\swordman.chr` 934-938 行 | ✅ 共用（推断挂接） | 召唤姿态 |
| 特效 ani | keiga\appear1-4/loop1-4/break1-4/disappear1-4 | `…\pvf\character\swordman\effect\animation\keiga\` | ✅ 实测（16 文件） | §2.3 四态 |
| 特效 img | k_up_n/k_up_d/k_down_n/k_down_d + ghost-body/ghost-light | `Character/Swordman/Effect/GhostStep/`（ani 引用实证） | ✅ 引用链实证 | 残影视觉（GhostStep 共用） |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（grep keiga 无） | ⛔ 无 | 无攻击 |
| PO 定义 | — | `…\pvf\passiveobject\character\swordman\`（三处 ls） | ⛔ 无 | 残影非 PO（引擎随体绘制） |
| .als | — | keiga 目录 ls | ⛔ 无边车 | — |
| 增益 appendage | （引擎内置，pvf 无实体） | `…\pvf\appendage\`（无路径不检索） | 未考证 | 减伤/回避载体 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89 召唤姿态） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | 必需 | ✅ 已在库 |
| k_down_n.img / k_down_d.img / k_up_n.img / k_up_d.img | sprite_character_swordman_effect_ghoststep.NPK | 残影四态视觉（**与 18-GhostStep 同 NPK——两技能一次提取全覆盖**） | 必需（视觉还原） | ❌ 未入库 |
| ghost-body.img / ghost-light.img | 同上 | 出现阶段本体/辉光 | 可选 | ❌ 未入库 |

缺失 img：必需 4 张、可选 2 张（同一 NPK）。⛔ 期间挂起。

## 5. 实现方案草案

**⛔ 暂缓（核心减伤）**——受击伤害管线钩子第 5 消费方（前 4：HP 下限钳制/施法免打断/后跳无敌帧判定/格挡吸收，001-Guard §5 汇总）：

| DNF 机制 | 我们的现状（代码实测） | 阻断点 |
|---|---|---|
| 物理/魔法伤害 -15%~47% | `ApplyHit`（LSHitboxComponentSystem）攻击方直跑 HitActions，受击方零钩子；MeleeHit 直扣 Damage | **受击伤害管线钩子**（001 §5"使能前提"一次投资多方受益——管线落地后本技能=按 % 削减 Damage） |
| 回避率 +0.5%~10% | 无命中 roll 环节（受击=盒相交即中） | 同管线（命中前 roll）+ 新回避机制 |
| 吸收 30~68 次后破碎 | LSBuffComponent 无受击事件 | 同管线（onIncomingHit 计数）+ Buff 受击回调 |
| [鬼影闪]+180%~370% | MeleeHit 固定 Damage；跨技能增伤无查询 | 属性数值消费链（R1-A4）+ 跨技能门面 |
| 600s 常驻 + 超时消散 | ✅ BuffDefinition TotalTimeMs + RemoveActions | 无 |

**可先行落地的部分（视觉壳 + 计数占位）**：
- `KeigaSkill : SkillLogic`——CD 30000、TotalTimeMs=600（读条 800 跳过，Summon2 姿态）；
  `OnCast` `ctx.PlayAnim(召唤姿态)` + `ctx.AddBuffToSelf(BuffIds.KeigaAura)`。
- `KeigaAuraBuff : BuffDefinition`——`TotalTimeMs=600000`（col3 直用）、AddActions={播 appear 特效 + 挂 loop 光环}、
  RemoveActions={播 break/disappear 特效}；⚠ loop 常驻光环撞 **Buff 视觉挂接**缺口（R1-A5）——
  简化：appear 单播 + 周期性重播（TickTimeMs=5000 重触发 appear 短版）近似"环绕感"。
- 吸收计数占位：等管线钩子（计数本技能是天然验证用例）。
- 注册点：`SkillIds.Keiga = 30`；`AnimId.KeigaAppearDn=139 / LoopDn=140 / BreakDn=141 / DisappearDn=142 /
  AppearUp=143 / LoopUp=144 / BreakUp=145 / DisappearUp=146`（上下×四态，d/n 双层先做 n 层）；`BuffIds.KeigaAura = 16` 预留。

**关键数值表（若实现）**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000ms | 30000 直用 |
| 持续 | 600s（恒定） | demo 缩 60s（600s 太长不便验证） |
| 物理减伤 | 15%→47% | 40%（固定，Lv20 档） |
| 魔法减伤 | 15%→47% | 40%（无元素区分时同物理） |
| 回避 | 0.5%→10% | **砍**（无机制） |
| 吸收次数 | 30→68 次 | 10 次（demo 缩短验证破碎） |
| 鬼影闪增伤 | 180%→370% | **砍**（消费链） |
| 读条 | 800ms | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Keiga.skl` | `.skl` 无子命令（6 列全明） | 手抄 6 值；`skl` 子命令同前议 |
| keiga\*.ani ×16 | `[SHADOW]` 类节（实测各 ani 均为 FRAME/DELAY/IMAGE 常规节） | 常规可译；SHADOW 跳过（已记档） |

结论：**16 个 ani 全部可被现有 ani 子命令翻译**（含循环 LOOP 节）；实质缺口 `.skl` 子命令（1 条，重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 物理减伤 % | **缺失档：受击伤害管线钩子**（第 5 消费方） | ⛔ 主因；管线落地即直译 |
| 魔法减伤 % | 同上 + 无元素区分 | 与物理同值 |
| 回避率 | 同上（命中 roll） | 砍 |
| 吸收次数→破碎 | 同上（受击计数回调） | 管线落地一并；demo 可先做纯计时版 |
| 鬼影闪增伤 | 属性数值消费链 + 跨技能门面（缺失档） | 砍 |
| 600s 常驻 | 无 | 直译（demo 缩短） |
| 残影常驻光环（loop 循环） | **缺失档：Buff 视觉挂接**（R1-A5；Frenzy/OverDrive 本批同撞） | appear 单播/周期重播近似 |
| 读条 800ms | 读条系统（延后档） | 跳过 |
| MP 120-616 | MP 系统（延后档） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. `[static data] 500` 语义。
2. 增益 appendage 实体（pvf\appendage 无路径）。
3. 残影四变体（k_up/down × n/d）的挂接层序与混色（d 层疑 LINEARDODGE——未逐帧验 GRAPHIC EFFECT 节）。
4. 施法姿态 = Summon1/2（推断，同 Khazan 惯例）。
5. 吸收次数的精确计数口径（每次受击=1 次？伤害次数还是命中次数——引擎内置不可考）。

**新系统级缺口（消费方增补）**
1. **受击伤害管线钩子第 5 消费方**（Keiga 减伤+回避+吸收计数三效合一，是该管线最完整的验证用例——
   001-Guard §5 建议的管线设计应把"受击计数回调"一并纳入）。
2. Buff 视觉挂接 +1 消费方（076/038 本批互证）。

**翻译工具缺口**：`.skl` 子命令（重复印证）；无新节。

**给下轮的经验**：鬼泣"鬼神护体"系（凯贾/鬼影步残影）视觉全在 `effect\animation\keiga\` + **GhostStep 图集共用**——
做 18-GhostStep 时一并提取 k_*/ghost-* img 即本技能视觉零成本；残影类四态（appear/loop/break/disappear）×上下×明暗的命名规律可直接套同族。
