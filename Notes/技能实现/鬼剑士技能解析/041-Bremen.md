# 侵蚀普戾蒙（Bremen）

> 技能ID 41 | 级别 B（召唤/光环减益） | 可实现性 ⛔（异常状态抗性 + 魔防两数值均无系统承载；阵本体与视觉可完整表达，结构与 Khazan 同族） | 分析日期 2026-08-22 | 批次 B1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 侵蚀普戾蒙 | `skill\Swordman\Bremen.skl` [name] |
| 英文名 | Bremen（取 skl 文件名；[name2]="The Bremen of Muzzy eyes"） | 同上 [name2] 实测 |
| 职业 | 鬼泣（[skill fitness growtype] **仅 2**；growtype maximum level 六系均 20） | 同上 |
| 学习等级 | 15（**前置：技能 25 刀魂之卡赞 Lv1**，[pre required skill] `25 1` 实测） | 同上 [required level] / [pre required skill] |
| 最高等级 | 50 | 同上 [maximum level] |
| 类型 | active（**skill class 3 = 召唤/增益类**） | 同上 [type]/[skill class] |
| 指令 | ↓↓ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 10000 ms（pvp 20000） | 同上 [dungeon][cool time] |
| MP | 40 → 644（Lv1→Lv50） | 同上 [dungeon][consume MP] |
| 读条 | casting time 400 ms | 同上 [casting time] |
| 特殊消耗 | 无 | 同上 |
| static data | `300 250`（static[1]=250 由 po_bremen.nut 实证为**光环半径 px**；static[0]=300 疑召唤落点前偏移 px——比卡赞前 50px，未考证） | 同上 + po_bremen.nut |
| 一句话效果 | 在前方召唤鬼神普戾蒙，领域内**敌人**减少异常状态抗性和魔防，持续 20~54.5 秒；重复施放则旧的消失、重新在前方召唤 | 同上 [explain] |

**level property 模板解码（3 列全明，L21 向量法）**：
- 持续时间 = col1 × 0.001 = **20s → 54.5s**（Lv1→Lv50）
- 减少异常状态抗性 = col0 = **2 → 140**
- 减少魔防 = col2 = **900 → 114158**

（dungeon 表 50 级完整；中段第 51 行起 45500 旁有数值回落重排段，与 Khazan 同款分段现象，未考证。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无 pushState**（引擎内置召唤，F3）。load_state 仅一行 PO 行为脚本（125 行）：

```
125: IRDSQRCharacter.pushScriptFiles("character/swordman/ghostrelease/po_bremen.nut");
```

被召唤物：`passiveobject\character\swordman\bremen.obj` = **PO 20014**（passiveobject.lst:11161 实测——与卡赞 20012/萨亚 20013 连号）。

### 2.2 引擎内置行为重建

```
施放（读条 400ms）：
  播召唤姿态（Summon1.ani 150ms / Summon2.ani 600ms，与卡赞共用）
  在前方（static[0] 疑 300px）创建 PO 20014（普戾蒙领域）
  领域存续 col1（20~54.5s）：周期性对半径 250px 内的敌人挂"异常抗性 -col0 / 魔防 -col2"减益 appendage
  （appendage 实体在 pvf\appendage 大树内，未考证——030 同例）
  领域动画相位：出现 → 常驻 → 消失（与卡赞同构六相位）
  重复施放：旧 PO 销毁重召（explain 明示）
```

**与卡赞的机制差异**：目标侧相反（卡赞 buff 我方 / 普戾蒙 debuff 敌方）+ 数值侧不同（力量智力 vs 异常抗性魔防）+ 时长随等级成长（卡赞恒 120s）。**结构完全同族**。

### 2.3 被动对象与 mod 附加层

**bremen.obj（PO 20014，纯视觉 + 引擎光环）**——结构与 khazan.obj 逐节一致：

| .obj 节 | 值 |
|---|---|
| [floating height] | 0（贴地） |
| [pass type]/[piercing power] | pass all / 1000 |
| [etc motion] | Bremen1.ani、Bremen2.ani（鬼神本体双层循环）→ BremenAreaAppear1/2.ani → BremenAreaStay.ani（循环）→ BremenAreaDisappear.ani |
| [name] | `普戾蒙` |

无 basic motion、无 attack info——纯视觉（L13 判定同卡赞）。

**po_bremen.nut（38 行，mod 附加攻击层，与 po_khazan 对称）**：`onCreat_Bremen` 攻击力 = sq_GetPowerWithPassive(技能 10 GHOSTRELEASE, col2)、`onProc_Bremen` 每技能 10 col3 间隔对半径 `sq_GetIntData(41,1)`=250px 内敌人发 no-stuck 打击包。原版无攻击，mod 层记档不还原。

### 2.4 动画关键帧表（全部实测）

| 动画 | 帧数 | 总时长 | 循环 | 引用 img | 备注 |
|---|---|---|---|---|---|
| `passiveobject\...\animation\bremen1.ani` | 8 | 880ms | ✅ | GhostBremen1.img | 鬼神本体层 1 |
| `bremen2.ani` | 8 | 880ms | ✅ | GhostBremen2.img | 本体层 2 |
| `bremenareaappear1.ani` / `2` | 13 | 1040ms | ❌ | SummonArea.img | 领域出现——**染色 `94 106 29 255`（毒绿色调，RGBA 已支持）** |
| `bremenareastay.ani` | 4 | 720ms | ✅ | SummonArea.img | 常驻（同绿色调） |
| `bremenareadisappear.ani` | 4 | 320ms | ❌ | SummonArea.img | 消失 |
| 施法侧共用 | summon1/2.ani | 150/600ms | ❌ | sm_body（帧 75-89） | 与卡赞/全体召唤共用 |

帧数/时长与卡赞逐项相同（diff 实测：仅 RGBA 染色值差异 + img 文件名差异）——**同一 SummonArea.img 两种染色** = 两阵视觉区分的全部秘密。

`.als` 边车：无（两侧 animation 目录 ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Bremen.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Bremen.skl` | ✅ 实测（239 行） | 数值（3 列全明）+ 前置 25 |
| 注册行 | load_state:125 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 仅 pushScriptFiles po_bremen.nut |
| PO 行为 nut | po_bremen.nut | `…\pvf\sqr\character\swordman\ghostrelease\po_bremen.nut` | ✅ 实测（38 行） | mod 攻击层（技能 10 驱动） |
| PO 注册 | passiveobject.lst:11160-11161 | `…\pvf\passiveobject\passiveobject.lst` | ✅ 实测 | Bremen.obj = 20014 |
| PO 定义 | bremen.obj | `…\pvf\passiveobject\character\swordman\bremen.obj` | ✅ 实测 | §2.3 纯视觉 6 相位 |
| PO .atk | —（原版无） | `…\passiveobject\character\swordman\attackinfo\`（grep bremen 命中 scream_bremen_effect.atk——zigadvent 技能联动攻击，非本技能；hardattackchargeafterbremen.atk 同理） | ⛔ 原版无 | 减益领域无攻击 |
| PO .ani | bremen×6 | `…\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 角色 .chr / .ani | [throw motion 2-1/2-2]（chr 934-938 行）Summon1/2.ani | `…\pvf\character\swordman\` | ✅ 实测 | 共用召唤姿态 |
| 减益 appendage | （未知） | `…\pvf\appendage\`（大树无路径不检索） | 未考证 | 异常抗性/魔防 debuff 载体 |
| .als | — | 两侧 animation 目录 | ⛔ 无 | — |
| 预载 img | [skill preloading image] | Bremen.skl 内 | ✅ 实测 | GhostBremen1/2.img + SummonArea.img |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | 必需 | ✅ 已在库 |
| GhostBremen1.img / GhostBremen2.img | sprite_character_swordman_effect.NPK（Effect 根目录推导） | 鬼神本体双层 | 必需（视觉还原） | ❌ 未入库 |
| SummonArea.img | 同上（**与卡赞共用一张**） | 领域阵（绿色调由 RGBA 染色实现） | 必需（视觉还原） | ❌ 未入库 |

缺失 img：必需 3 张（其中 SummonArea 与卡赞共享，鬼泣两阵合计 5 张、同一 NPK 一次提取）。

## 5. 实现方案草案

**⛔ 暂缓（核心减益）**——与 030/025 同族判定，但缺口构成略不同：

1. **异常状态抗性系统缺失**（缺失档）：抗性 -2~-140 无处安放——我们无抗性数值键、无"异常状态命中概率对抗抗性"的公式（LSRng 只做 ProcChance 直判）。这比"属性消费链"更深一层：即便有数值键，现有 Buff 挂载是固定概率，不查抗性。
2. **魔防/防御数值无消费链**（缺失档，R1-A4 已记）：魔防 -900~-114158 无键位（NumericType.Defense 存在但伤害端零消费）。
3. **队伍/阵营判定**（缺失档，R1-A3）：目标为敌方——**单人 demo 期 GetEnemies()（除自己全部）语义恰好近似正确**，此项实际不阻断 demo；多人/队友场景才阻断。

**可先行落地的部分（与卡赞共用的视觉壳）**：
- `BremenSkill : SkillLogic`——CD 10000、TotalTimeMs=600；`OnCast` PlayAnim(召唤姿态) + `CreateAreaInFront(AreaIds.BremenZone, 3.0)`（static[0]=300 推断）。
- `BremenZone : AreaDefinition`——`TotalTimeMs=20000`（Lv1 值）、`HalfExtents=(2.5,0.5,2.5)`、`TickTimeMs=1000`、`ViewAnimId=GhostBremen+绿色领域 Stay`、`ViewEndAnimId=Disappear`；TickActions 留空（等抗性/魔防系统）。
- 减益落地时：`BuffIds.BremenDebuff = 11` 预留（TickActions=AddBuff，Burn 同构；数值进未来 STR/INT/RES/DEF 键位）。
- 注册点：`SkillIds.Bremen = 28`；`AnimIds 124-130`（GhostBremen1/2、AreaAppear1/2、AreaStay、AreaDisappear——**注意与卡赞是 6 个不同 .ani 文件**（RGBA 不同），各翻译各注册，不能共用 json）；`AreaIds.BremenZone = 19`。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000ms | 10000 直用 |
| 领域时长 | 20s → 54.5s | 20s（Lv1 直用） |
| 光环半径 | 250px（static[1]） | 2.5 单位 |
| 召唤落点 | 前 300px（static[0] 推断） | CreateAreaInFront 3.0 |
| 异常抗性减 | 2 → 140 | 等抗性系统 |
| 魔防减 | 900 → 114158 | 等防御消费链 |
| 读条 | 400ms | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Bremen.skl` | `.skl` 无子命令（含 [pre required skill]/[steel learning skill] 节） | 手抄 3 值；`skl` 子命令同前议（前置技能关系建议进输出） |
| bremen×6.ani | `[SHADOW]`（规则表外） | 跳过无碍；RGBA 染色已支持 |
| bremen.obj | `.obj` 无子命令 | 与卡赞同：手工映射 Area 三态 |

结论：动画资源全部可被现有 ani 子命令翻译（RGBA 染色是本技能视觉的关键区分，链路已通）；实质缺口 `.skl`/`.obj`（重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 敌方减异常抗性（核心） | **缺失档：异常状态抗性系统**（数值键 + 概率对抗公式两层缺） | ⛔ 主因；与异常 Buff 体系（冰冻/出血等已有 Buff 的抗性判定）合并立项 |
| 敌方减魔防（核心） | **缺失档：属性数值消费链**（Defense 键存在但零消费） | 同 030/025 队列 |
| 敌方目标选择 | 阵营判定缺失，但 demo 期 GetEnemies 语义近似 | 单人 demo 不阻断；多人时补 |
| 前置技能（卡赞 Lv1） | 无技能树/前置系统 | demo 直接可学（跳过校验） |
| 20~54.5s 长驻 + 重复替换 | 无（Area TotalTimeMs + 重建） | 直译 |
| 读条/MP | 延后档 | 跳过 |
| mod 攻击层（po_bremen） | mod 内容 + mod 技能 10 | 不实现 |

## 8. 存疑与缺口上报

**未考证项**
1. static[0]=300 落点语义（与卡赞 250 的差异推断为召唤距离）。
2. 减益 appendage 实体与刷新规则（大树未检索）。
3. 光环刷新间隔（引擎内置；demo 取 1s）。
4. level info 中段回落重排（45500 段）规则——与 Khazan 同款现象。
5. scream_bremen_*.ani/atk（zigadvent 技能的普戾蒙联动，16116 号 PO）与本技能的关系（另一技能的增幅消费，未展开）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **异常状态抗性系统**：本技能首次以"减抗性"形态撞上（030 撞的是元素抗性）——需要 ①抗性数值键 ②Buff 挂载概率与抗性的对抗公式（现 ProcChance 直判）。所有"增/减异常抗性"技能（鬼泣系多个光环）与"免疫异常"装备词条共用。
2. **技能前置树**（[pre required skill]）：本技能前置卡赞 Lv1——首个带前置关系的主动技样本；demo 期跳过校验即可，记档供技能树立项参考。

**翻译工具缺口**：`.skl`/`.obj` 子命令（重复印证）；`[skill preloading image]` 节（第 3 例，建议工具忽略+记档，同 025）。

**给下轮的经验**：鬼泣鬼神族第二样本完全验证 025 的结构模板（引擎召唤 + .obj 纯视觉六相位 + SummonArea.img 共用 + RGBA 染色分色系 + pushScriptFiles mod 攻击层 + 连号 PO）。萨亚（20013 冰阵）/罗刹/卡洛按此直查即可；**两阵共用 img 但 .ani 不能共用**（RGBA 差异在 .ani 帧数据里）。
