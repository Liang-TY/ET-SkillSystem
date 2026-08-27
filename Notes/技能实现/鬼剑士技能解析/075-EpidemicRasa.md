# 瘟疫之罗煞（EpidemicRasa）

> 技能ID 75 | 级别 B（召唤/减益光环） | 可实现性 ⛔（攻速/物防/硬直减益均无系统承载 + 失明无系统；附身结构与中毒/出血异常可先行，结构与 Bremen/Khazan 鬼神族同族） | 分析日期 2026-08-22 | 批次 B4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 瘟疫之罗煞 | `skill\Swordman\EpidemicRasa.skl [name]` |
| 英文名 | EpidemicRasa（取 skl 文件名；[name2]="Rhasa of Epidemic"） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype] 2） | 同上 |
| 学习等级 | 35（**前置：技能 36 Lv1**） | 同上 [required level] / [pre required skill] |
| 最高等级 | 70（六系各 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（**skill class 3 = 召唤/增益类**） | 同上 [type]/[skill class] |
| 指令 | ↑→ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 30000 ms | 同上 [dungeon][cool time] |
| MP | 80 → 280 | 同上 [dungeon][consume MP] |
| 读条 | casting time 500 ms | 同上 |
| 消耗品 | 无色小晶块 ×1 | 同上 [consume item] |
| static data | `450 1000 100 100 100 80 250 4 3`——[6]=250（**召唤范围 = static[6]×2 = 500px**，模板 (6,6,2.0) 实证）；[0]=450 疑召唤落点前偏移（与 Khazan 250/Bremen 300 同位参数，未考证）；其余未考证 | 同上 [static data] + [level property] 向量 |
| 一句话效果 | 召唤瘟疫鬼神附到敌人身上：减攻速/物防/硬直，并附加中毒、失明、流血三选一异常状态；单个敌人最多同时附 3 个鬼神 | 同上 [explain] |

**level property（20 列，Lv1 代表值，L21 法解码）**：领域持续 col0×0.001=**20s**；召唤范围 500px（static）；攻速减 col2=5、物防减 col3=776、硬直减 col4=15（量级 Lv70 约 200/6250/60）；**中毒**：机率 col6×0.1=100%（本 pvf 恒定）、Lv col7=37、持续 col8×0.001=10s、攻击力 col9=1200（源 -3，L21 未解档）；**失明**：机率 col10×0.1=100%、Lv col11=37、持续 col12×0.001=5s、视野范围 col13=40px、命中减 col14=1%；**出血**：机率 col15×0.1=100%、Lv col16=37、持续 col17×0.001=10s、攻击力 col18=120（源 -4）；**鬼神附身持续 col19×0.001=10s**。
（col1=10000 无模板引用——疑附身时长冗余列或引擎消费，未考证；col5=2000 同。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无 pushState**（引擎内置召唤，F3；与 025-Khazan/041-Bremen 同族模板）。load_state 只有一行**被注释掉的** PO 行为脚本注册（实测 126 行）：

```
126: //IRDSQRCharacter.pushScriptFiles("character/swordman/ghostrelease/po_epidemicrasa.nut");
```

（po_epidemicrasa.nut 在 `ghostrelease\` 目录**不存在**——实测目录仅 ap_ghostrelease/po_bremen/po_khazan 三文件。mod 作者注释掉了不存在的脚本，原版即引擎内置。）

被召唤 PO（passiveobject.lst 实测）：
- **EpidemicRasa.obj = 20042**（鬼神本体/附身体，lst:11217 行前值实测）
- **EpidemicRasaCreater.obj = 20040**（领域生成器，lst:11213 行前值实测）

### 2.2 引擎内置行为重建

```
施放（读条 500ms）：
  播召唤姿态（与 Khazan/Bremen 共用 Summon1/2.ani——041 已证同族共用）
  在前方（static[0] 疑 450px）创建领域生成器 PO 20040：
    EpidemicRasaAreaAppear1/2.ani（13 帧，SummonArea.img，鬼泣鬼神族共用领域底）
    → AreaStay.ani（4 帧循环，领域持续 col0=20s）→ AreaDisappear.ani（4 帧）
    同时显现鬼神视觉（rasa.ani / rasa_ldodge.ani，8 帧循环）
  领域在 500px 范围内寻找敌人（召唤范围 static[6]×2）：
    对命中者创建附身鬼神 PO 20042（epidemicrasa.atk：damage reaction=none/push0/lift0
      ——非伤害命中，专用于"附上"判定；stay.ani 6 帧 ×6 攻击盒 = 附身判定帧）
  附身（col19=10s）：目标减攻速 col2/物防 col3/硬直 col4 +
    附加中毒/失明/流血三选一（各 100% 机率、Lv37、持续/攻击力见 §1）
    （减益与异常的具体写入=引擎内置 appendage，pvf\appendage 大树内未检索——041 同例）
  单个敌人最多同时 3 个鬼神（explain 明示；3 层叠加语义引擎内置）
```

### 2.3 被动对象（两 PO，全实测）

**EpidemicRasa.obj（20042，附身鬼神）**：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [width] | 0 0 | 无宽度判定 |
| [basic motion] | `Animation/EpidemicRasa/startup.ani`（12 帧，s-01.img） | 相位 1：鬼神显现 |
| [etc motion] | startup_ldodge、stay.ani（6 帧）、stay_ldodge、grab.ani（9 帧）、grab_ldodge、grab_stay.ani（6 帧）、grab_stay_ldodge | 显现→常驻→**抓附（grab）→附身常驻**（ldodge=线性减淡叠层变体） |
| [attack info] | `AttackInfo/EpidemicRasa.atk` | magic / **damage reaction=none / push 0 / lift 0 / blow**（附身判定专用，无位移无反应） |
| [name]（文件末尾） | `邪光斩` | ⚠ **mod 污染痕迹**（C6：obj 被改/复用时残留异名） |

**EpidemicRasaCreater.obj（20040，领域生成器）**：basic=EpidemicRasaAreaAppear1.ani；etc=Appear2 / Stay / Disappear / rasa.ani / rasa_ldodge.ani；attack=`AttackInfo/EpidemicRasaCreater.atk`（**down**/push0/lift0——领域初次展开的推挤反应）。[name] 同样残留 `邪光斩`（mod 污染）。

攻击盒：stay.ani 6 帧 ×6 盒（附身判定帧）；其余均无盒。

### 2.4 动画关键帧表（全部实测）

| 动画 | 帧数 | 循环 | 引用 img | 备注 |
|---|---|---|---|---|
| `epidemicrasaareaappear1.ani` / `2` | 13 | ❌ | **SummonArea.img** | 领域出现（与 Khazan/Bremen 共用一张图，041 已证） |
| `epidemicrasaareastay.ani` | 4 | ✅ | SummonArea.img | 常驻 |
| `epidemicrasaareadisappear.ani` | 4 | ❌ | SummonArea.img | 消失 |
| `rasa.ani` / `rasa_ldodge.ani` | 8 | ✅ | `EpidemicRasa/rasa.img` | 鬼神本体（ldodge=减淡层） |
| `startup.ani` / `startup_ldodge.ani` | 12 | ❌ | `EpidemicRasa/s-01.img` | 显现 |
| `stay.ani` / `stay_ldodge.ani` | 6 | ✅（推断） | s-01.img | **常驻 ×6 攻击盒 = 附身判定** |
| `grab.ani` / `grab_ldodge.ani` | 9 | ❌ | s-01.img | 抓附动作 |
| `grab_stay.ani` / `grab_stay_ldodge.ani` | 6 | ✅ | s-01.img | 附身常驻 |
| 施法侧共用 | summon1/2.ani（sm_body 帧 75-89） | ❌ | sm_body | 与 Khazan/Bremen 共用（041 实测） |

`.als` 边车：**无**（两侧 animation 目录 ls 实测）。RGBA/染色：appear/stay 系与 Bremen 同款 SummonArea 染色待逐帧确认（Bremen 是绿 94/106/29——本技能未提取，存疑）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | EpidemicRasa.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\EpidemicRasa.skl` | ✅ | 数据（20 列 + static） |
| 注册行 | load_state:126（**注释态**） | `…\pvf\sqr\character\swordman_load_state.nut` | ⚠ 注释行 | po_epidemicrasa.nut 不存在——原版即引擎内置 |
| PO 行为 nut | —（不存在） | `…\pvf\sqr\character\swordman\ghostrelease\po_epidemicrasa.nut` | ⛔ 缺失 | 引擎内置（F3） |
| PO 注册 | passiveobject.lst:11213/11217 | `…\pvf\passiveobject\passiveobject.lst` | ✅ | 20040/20042 |
| PO 定义 | epidemicrasa.obj / epidemicrasacreater.obj | `…\pvf\passiveobject\character\swordman\` | ✅（末尾 [name] 均 mod 污染"邪光斩"） | §2.3 |
| PO .atk | epidemicrasa.atk / epidemicrasacreater.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 附身判定（none/0/0）/ 领域展开（down/0/0） |
| PO .ani | animation\epidemicrasa\ 14 文件 | `…\pvf\passiveobject\character\swordman\animation\epidemicrasa\` | ✅ | §2.4 |
| 角色 .chr/.ani/.atk | —（无专属；共用 Summon1/2.ani） | `…\pvf\character\swordman\` | ⚠ 共用（041 已证） | 召唤姿态 |
| 减益 appendage | （未知） | `…\pvf\appendage\`（大树无路径不检索） | 未考证 | 攻速/物防/硬直/异常载体 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 无 | — |
| 预载 img | [skill preloading image] | EpidemicRasa.skl 内 | ✅ 实测 | ⚠ 列的是 GhostBremen1/2 + SummonArea（**Bremen 的清单**，mod 换皮残留——真用图 rasa.img/s-01.img 未列入，记档） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | 必需（共享） | ✅ 已在库 |
| SummonArea.img | sprite_character_swordman_effect.NPK（Effect 根） | 领域底（**与 Khazan/Bremen 共用一张**） | 必需（视觉还原） | ❌ 未入库 |
| rasa.img（`Character/Swordman/Effect/EpidemicRasa/`） | sprite_character_swordman_effect_epidemicrasa.NPK | 鬼神本体 | 必需 | ❌ |
| s-01.img（同目录） | 同上 | 显现/常驻/抓附 | 必需 | ❌ |

缺失 img：必需 3 张（其中 SummonArea 与鬼泣鬼神族共享——B4 批内 075 独需 rasa/s-01 两张新图）。

## 5. 实现方案草案

**⛔ 暂缓（核心减益）**——与 041-Bremen 同族判定，缺口构成：
1. **攻速/物防/硬直三减益无消费链**（缺失档，R1-A4 最重缺口；硬直 reduction 更是全新数值键——现 HitstunMs 是命中参数不是属性）。
2. **失明系统缺失**（视野范围/命中率双概念均无）。
3. 队伍/阵营判定（R1-A3）——demo 期 GetEnemies() 近似正确，不阻断。

**可先行落地的部分（附身结构 + 异常状态）**：
- `EpidemicRasaSkill : SkillLogic`——CD 30000、TotalTimeMs=600（召唤姿态）；`OnCast`：`ctx.PlayAnim(召唤姿态)` + `ctx.CreateAreaInFront(AreaIds.RasaField, 4.5)`（static[0]=450 推断）。
- `RasaFieldArea : AreaDefinition`——`TotalTimeMs=20000`（col0 Lv1）、`HalfExtents=(5.0,0.5,5.0)`（范围 500px）、`TickTimeMs=1000`、`TickActions={ AddRasaGhostBuff }`、`ViewAnimId=AnimId.RasaFieldStay`、`ViewEndAnimId=AnimId.RasaFieldDisappear`。
- `RasaGhostBuff : BuffDefinition`——**附身鬼神 = 目标身上的 Buff**（DNF 的 grab_stay 常驻语义直译）：`TotalTimeMs=10000`（col19）、`TickTimeMs=1000`、`TickActions={ PoisonDamageTick }`（三选一异常简化为中毒先落地）；叠层=同 Buff 再挂 Stack+1（**BuffDefinition 叠层简版现成**——"单敌最多 3 鬼" = Stack 上限 3，恰好是叠层机制的第一个需求样本，框架注释已预留 Stack 上限位）。
- `PoisonDamageTickAction : LSAction` + `AddRasaGhostBuffAction : LSAction`——BleedDamageTick/AddBleedBuff 同构新写（~10 行/个）。
- 失明/出血分支与三减益：等数值消费链 + 失明系统落地后回填。
- 注册点：`SkillIds.EpidemicRasa = 31`；`AreaIds.RasaField = 36`；`BuffIds.RasaGhost = 15`；`ActionIds.PoisonDamageTick = 14`、`AddRasaGhostBuff = 15`；`AnimIds 153-158`（AreaAppear/Stay/Disappear、Rasa、Startup、GrabStay）。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000ms | 30000 直用 |
| 领域持续 | col0×0.001 = 20s | 20000 |
| 附身持续 | col19×0.001 = 10s | Buff 10000 |
| 召唤范围 | static[6]×2 = 500px | HalfExtents 5.0 |
| 落点 | static[0]=450（推断前偏移） | CreateAreaInFront 4.5 |
| 中毒 | 100%/Lv37/10s/攻 1200 每？ | PoisonBuff：10s 每 1s 12（Bleed 同构） |
| 失明（⛔） | 5s/视野40px/命中-1% | 等系统 |
| 三减益（⛔） | 攻速5+/物防776+/硬直15+ | 等消费链 |
| 单敌上限 | 3 鬼 | Buff Stack 上限 3 |

## 6. 翻译工���适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `EpidemicRasa.skl` | `.skl` 无子命令（20 列——**本批最宽表**，超 023 的 18 列） | 手抄关键 10 值（§5 已列）；skl 子命令矩阵输出设计的最宽样本 |
| 两个 `.obj` | `.obj` 无子命令（且末尾 [name] 被 mod 污染） | 手工映射（§2.3 已给）；mod 残留记档 |
| 两个 PO `.atk` | `.atk` 无子命令 | 手抄 |
| 14 个 PO `.ani` | 常规节（无 .als、无 RGBA 实测样本——染色待确认见 §2.4） | 现有 ani 子命令覆盖 |

结论：动画资源全部可译；实质缺口 `.skl`/`.obj`/`.atk`（既有三类）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 减攻速/物防/硬直（核心） | **缺失档：属性数值消费链**（硬直 reduction 为全新数值键） | ⛔ 主因之一；与 041/030/025 队列合并等消费链 |
| 失明（视野 40px + 命中 -1%） | **缺失档：失明系统**（视野/命中双概念） | ⛔ 主因之二；视觉/命中系统均无 |
| 中毒/失明/流血三选一 | 中毒=PoisonBuff 新写（Bleed 同构零新机制）；三选一=LSRng 分流（已有） | 先落地"恒中毒"版 |
| 单敌最多 3 鬼叠加 | BuffDefinition 叠层简版现成（Stack+刷新）；**Stack 上限**字段是预留位未实现 | 叠层机制立项时以本技能为第一用例 |
| 领域找敌+附身（grab 语义） | 无独立系统——**Area Tick + AddBuff 直译**（grab 只是视觉） | RasaFieldArea Tick 挂 Buff |
| 读条/MP/无色消耗 | 延后档 | 跳过 |
| mod 污染（obj [name]=邪光斩、skl 预载清单错列 Bremen 图） | C6 已知形态 | 按幸存数据重建（本文即） |

## 8. 存疑与缺口上报

**未考证项**
1. static[0]=450 落点语义（与 Khazan 250/Bremen 300 同位参数族）。
2. 减益/异常 appendage 实体（appendage 大树未检索）与"三选一"分流时机（附身时 or 每次 Tick）。
3. appear/stay 的 SummonArea 染色值（Bremen 绿色系已证；本技能未提取帧数据确认）。
4. col1=10000 / col5=2000 冗余列语义。
5. EpidemicRasaEx（lst 396 行）强化版另行分析（E 类批次）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **失明系统**（视野裁剪 + 命中率）首个明确需求样本——与冰冻/眩晕等已有 Buff 不同，需要视觉与命中两层；建议并入异常状态体系立项评估。
2. **Buff Stack 上限**：BuffDefinition 注释预留位（叠层简版只加不计上限）——本技能"单敌最多 3 鬼"是第一个真实需求，实现时给 BuffDefinition 加 MaxStack 即可（小改，记档）。
3. 鬼神族第三样本（Khazan/Bremen/Rasa）继续验证 041 模板：连号 PO（20040/20042）、SummonArea 共用、召唤姿态共用、引擎内置——**四样本后该模板可固化为 F 系结论**（萨亚 20013/卡洛同查）。

**翻译工具缺口**：`.skl` 20 列宽表（既有建议的最宽新证）；`.obj` mod 污染（[name] 残留）记档。
