# 冰晶之萨亚（SayaEx）

> 技能ID 96 | 级别 E（**预分类纠偏：文件名以 Ex 结尾但不是 TP 强化被动——[type] active、skill class 3 召唤类，鬼泣二觉替换型主动技**，本文按 B 类深度走读） | 可实现性 🔶（领域主干直接可做（036 同构）；冰之结晶子系统撞"LSActionContext 无 CreateArea"缺口，离散结晶降级为第二区叠加） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冰晶之萨亚 | `skill\Swordman\SayaEx.skl` [name] |
| 英文名 | SayaEx（取 skl 文件名；[name2] 实测 `Saya of Crystal`） | 同上 |
| 职业 | 鬼泣二觉（[skill fitness second growtype] `1 2`；[second growtype maximum level] 12 槽第 4/5 位 = **鬼泣 30 级**，R6-C4 职业判定捷径；[skill fitness growtype] 空 = 一觉树无此技） | 同上 |
| 学习等级 | 60（[required level range] 2）；前置 36 冰霜之萨亚 Lv1 | 同上 |
| 最高等级 | 50（[maximum level]；二觉档实际 30） | 同上 |
| 类型 | active（**skill class 3 = 召唤类**，与基础萨亚同型）/ 魔法 | 同上 [type]/[skill class]/[weapon effect type] |
| 指令 | →←↓→ + Z（[skill command advantage] 50/50） | 同上 |
| CD | 40000 ms | 同上 [cool time] |
| MP | 800 → 2240 | 同上 [consume MP] |
| 读条 | 100 ms | 同上 [casting time] |
| 特殊消耗 | 道具 3037 ×1（无色小晶块，同基础萨亚） | 同上 [consume item] |
| static data | `600 350 70 1000 60 1000`——[0]=600 召唤落点前偏（推断，同族 036=450）；[1]=350 领域半径（level property 无该引用、按族解读）；[3]=1000 伤害间隔（**(3,3,0.001)** 实证）；[2]=70/[4]=60/[5]=1000 未考证（[4]/[5] 疑结晶参数） | 同上 + level property |
| 一句话效果 | 召唤冰晶版萨亚：领域内敌人每 1 秒受冰属性魔法伤害、概率冰冻；**并周期性生成冰之结晶攻击敌人**（基础萨亚没有的增量），持续 10 秒 | 同上 [explain] |

**level property 模板解码（9 列 + 10 向量，L21 法全解，Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 持续时间 | (-1,1,0.001) | col1 = 10000 → **10 s 恒定** |
| 伤害间隔 | **(3,3,0.001)** → static[3] | 1000 → **1 s 恒定** |
| 魔法攻击力 | (-1,0,1.0) | col0 = 322 → 3631 % |
| 冰冻机率 | (-1,2,0.1) | col2 = 100→2550 → **10% → 255%** |
| 冰冻持续时间 | (-1,4,0.001) | col4 = 1500→24064 → 1.5 → 24.1 s |
| 冰冻Lv | (-1,3,1.0) | col3 = Lv62 → Lv160 |
| **冰之结晶生成数量** | (-1,5,1.0) | col5 = **10（恒定）** |
| **冰之结晶魔法攻击力** | (-1,6,1.0) | col6 = **319 → 3602 %** |
| 冰之结晶再生成时间 | (-1,7,0.001) | col7 = **500 ms 恒定** |
| 冰之结晶射程 | (-1,8,1.0) | col8 = **150 px 恒定** |

与基础萨亚（036）对照：领域半径 250→**350**、落点 450→**600**、冰冻概率曲线大幅上抬（Lv1 9%→10%、满级 74%→255%）、新增结晶子系统（10 个/0.5s 再生/319% 独立伤害/150px 射程）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（纯引擎内置，F3——比基础萨亚更"数据化"）

- load_state **无 pushState/pushScriptFiles**（grep sayaex 无命中）；`sqr\character\swordman\` 无本技 nut/appendage；PO 创建无脚本（grep 20063/20064 白名单 0 命中）——**全部行为由引擎按 skl+obj 数据驱动**（036 同型第三例）。
- 被召唤物 ×2（passiveobject.lst 实测 11258-11261 行）：
  - **PO 20063 = `Character/Swordman/SayaEx.obj`**（冰晶萨亚领域）；
  - **PO 20064 = `Character/Swordman/SayaExIce.obj`**（冰之结晶）。
- 施法姿态：.chr 无 sayaex 条目（grep 实测）——共用召唤族 throw motion（Summon1/2.ani，036 §2.2 同款，推断）。
- [skill preloading image]：GhostSaya1/2 + SummonArea 三张（与基础萨亚同图，视觉直系继承的声明级证据）。

### 2.2 引擎内置行为重建

```
施放（读条 100ms，共用召唤姿态）：
  播召唤姿态 → 消耗 MP + 无色×1
  在前方 static[0]=600px 创建 PO 20063（冰晶萨亚领域）
领域存续 col1 = 10s（SayaEx.obj 六相位：出现→常驻→消失，同族结构）：
  每 static[3]=1000ms：对半径 static[1]=350px 内敌人
    → 冰属性魔法伤害 col0%（322%~3631%）
    → 概率冰冻 col2×0.1（10%~255%），持续 col4（1.5~24.1s），冰冻 Lv col3
    （命中参数 = Saya.atk——与基础萨亚同一份 .atk，见 §2.3）
冰之结晶子系统（每 static? 周期，数据见 col5/7/8）：
  每 col7=500ms 生成 1 个 PO 20064（上限/总量疑 col5=10，未考证）
  结晶四相位（SayaExIce.obj）：Ready(2000ms 待机) → Start(840ms 发射) → Stay(100000ms 悬停=待事件，L23 型) → End(560ms 消散)
  结晶命中：SayaExIce.atk（魔法/冰/无反应/无击退——见 §2.3），伤害 col6（319%~3602%），射程 col8=150px
  （结晶的移动/寻敌规则引擎内部，无脚本可读——未考证，疑 150px 内寻敌或直线弹道）
重复施放：旧 PO 销毁重召（鬼神族共用语义）
```

### 2.3 被动对象（两个 .obj 完整实测）

**PO 20063 sayaex.obj（领域）**：

| .obj 节 | 值 |
|---|---|
| [attack info] | **`AttackInfo/Saya.atk`——与基础萨亚完全同一份**（magic/water/damage reaction none/push 30/lift 10/[active status] freeze 运行时写入，036 §2.3 已读） |
| [etc motion] 六相位 | `SayaEx/Saya1.ani`、`SayaEx/Saya2.ani`（本体双层 10 帧 1100ms 循环）→ `SayaAreaAppear1/2.ani`（**借基础萨亚父目录资产**，出现）→ `SayaEx/SayaAreaStay.ani`（4 帧 720ms 常驻）→ `SayaAreaDisappear.ani`（借父目录，消失） |
| 其余 | floating height 0 / pass all / piercing 1000（同族） |

**PO 20064 sayaexice.obj（冰之结晶）**：

| .obj 节 | 值 |
|---|---|
| [basic motion] | `SayaEx/Ready.ani`（2 帧 2000ms 待机） |
| [etc motion] | `Start.ani`（12 帧 840ms 发射）→ `Stay.ani`（1 帧 **100000ms 悬停**，L23 待事件型）→ `End.ani`（8 帧 560ms 消散） |
| [attack info] | `AttackInfo/SayaExIce.atk` 实测：**magic / water element / damage reaction none / hit horizon / 无 push 无 lift** / wav P_SAYA_ICE_HIT（结晶轻跳不打断） |
| 其余 | width 1 1 / floating height 1 / pass all / piercing 1000 |

### 2.4 动画关键帧表（sayaex\ 子目录 23 文件，抽关键件实测）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img |
|---|---|---|---|---|---|
| saya1.ani / saya2.ani（领域本体双层，循环） | 10 | 1100ms | 无 | 无 | GhostSaya1/2.img |
| sayaareastay.ani（领域常驻，循环） | 4 | 720ms | 无 | 无 | SummonArea.img |
| ready.ani（结晶待机，basic motion） | 2 | 2000ms | 无 | 无 | SayaEx/ReadyDodge.img |
| start.ani（结晶发射） | 12 | 840ms | 无 | 无 | SayaEx/IceDodge.img |
| stay.ani（结晶悬停） | 1 | **100000ms** | 无 | 无 | SayaEx/IceDodge.img |
| end.ani（结晶消散） | 8 | 560ms | 无 | 无 | SayaEx/IceDodge.img |
| .als 叠层 ×5（实测全读） | — | — | — | — | ready→ReadyNormal@z-10；saya1→backnormal@-20+backdodge@-10+wingdodge@+10；sayaareastay→bottomdodge1/2/3@-10/-20/-30；start→startbodynormal@-10+startbodydodge@-5（F5 起）；stay→staybodynormal@-10 |

（判定全部走 .atk + 引擎半径/结晶碰撞，.ani 无攻击盒——036 同型。）

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SayaEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SayaEx.skl` | ✅（243 行） | 9 列全解 + 二觉归属 |
| 注册行 | —（无 pushState/pushScriptFiles） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无 | 纯引擎召唤（F3） |
| 主 nut | —（不存在） | `…\sqr\character\swordman\`（grep 0 命中） | ⛔ 无 | 引擎内置 |
| PO 注册 | passiveobject.lst:11258-11261 | `…\pvf\passiveobject\passiveobject.lst` | ✅ 实测 | 20063 领域 / 20064 结晶 |
| PO 定义 | sayaex.obj / sayaexice.obj | `…\passiveobject\character\swordman\` | ✅ 实测 | §2.3 |
| PO .atk | sayaexice.atk（+ 复用 saya.atk） | `…\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | §2.3 |
| PO .ani/.als | sayaex\ ×23（含 .als ×5） | `…\passiveobject\character\swordman\animation\sayaex\` | ✅ 实测 | §2.4 |
| 借用资产 | SayaAreaAppear1/2.ani、SayaAreaDisappear.ani（父目录）、Summon1/2.ani（施法姿态） | `…\passiveobject\character\swordman\animation\` / `…\character\swordman\animation\` | ✅ 存在 | 领域相位/施法共用 |
| .chr 条目 | —（无专属） | `…\character\swordman\swordman.chr` | ⛔ 无 | 姿态共用（推断同 036） |
| 联动标记 | ap_scream_sayaex_friend.nut（38 行空壳，buff 图标 158） | `…\sqr\character\swordman\zigadvent\` | ✅ 实测 | 241 吉格降临嘶吼召唤"冰晶萨亚"的标记 appendage（另有 ap_scream_saya_friend 对应基础版）——非本技逻辑 |
| 基础技文档 | 036-Saya.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 结构对照基准 |
| 装备层 | —（共用姿态） | `…\equipment\character\swordman\avatar\` | 未查 | 同 036 结论 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（召唤姿态帧） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法姿态（共用） | 必需（共享） | ✅ |
| `Character/Swordman/Effect/GhostSaya1.img` / `GhostSaya2.img` | sprite_character_swordman_effect.NPK（036 同款推导） | 冰晶萨亚本体双层（**与 036 共享**） | 必需 | ❌ |
| `Character/Swordman/Effect/SummonArea.img` | 同上（三阵一图，RGBA 分色——冰晶版疑沿用冰蓝，未逐帧验染色值） | 领域阵 | 必需 | ❌ |
| `Character/Swordman/Effect/SayaEx/ReadyDodge.img` | sprite_character_swordman_effect_sayaex.NPK | 结晶待机体 | **必需** | ❌ |
| `…/SayaEx/IceDodge.img` | 同上 | 结晶发射/悬停/消散体（三相位共用） | **必需** | ❌ |
| `…/SayaEx/ReadyNormal.img`、`IceNormal.img` | 同上 | .als 加法混合层（normal 系） | 可选 | ❌ |
| `…/SayaEx/back.img`、`bottomDodge.img`、`wingDodge.img` | 同上 | 本体/领域 .als 叠层 | 可选 | ❌ |

**缺失 img：必需 5 张（3 张与 036 共享 + 2 张本技新增，分属 2 个 NPK）、可选 5 张（同一 NPK）。**

## 5. 实现方案草案（号段：SkillIds 34 / AnimIds 178-186 / AreaIds 39-40，E4 批内顺延；撞号无妨 L18）

### 内容件清单

1. **`DotNet~/Skills/SayaExSkill.cs : SkillLogic`**（同 036 SayaSkill 范式）：`CooldownMs=40000`、`TotalTimeMs=600`；`OnCast`：`ctx.PlayAnim(AnimId.SwordmanSummon2)`（共用姿态）+ **双区同帧创建**：`ctx.CreateAreaInFront(AreaIds.SayaExZone, (FP)6)`（static[0]=600px）+ `ctx.CreateAreaInFront(AreaIds.SayaExCrystalZone, (FP)6)`（结晶区与领域同心）。
2. **`DotNet~/Areas/SayaExZone.cs : AreaDefinition`**（036 SayaZone 直改）：
   - `TotalTimeMs=10000`（col1）、`TickTimeMs=1000`（static[3]）、`TickActions={MeleeHit}`、`HalfExtents=(3.5,0.5,3.5)`（static[1]=350px）；
   - `HitReaction{Damage=40, HitstunMs=0, KnockbackX=30, LaunchY=10, ProcBuffId=BuffIds.Freeze, ProcChance=10}`（Saya.atk 直译 + Lv1 冰冻概率 10%；冰冻时长要贴原值则新 `SayaExFreezeBuff : BuffDefinition`（1500ms，Freeze 同构））；
   - `ViewAnimId=AnimId.SayaExAreaStay`、`ViewBackAnimId=AnimId.SayaExGhost1/2`（本体双层——三层视图不够时本体并为单层）。
3. **`DotNet~/Areas/SayaExCrystalZone.cs : AreaDefinition`**（**冰之结晶的降级载体**——离散结晶见 §7 缺口）：
   - `TotalTimeMs=10000`、`TickTimeMs=500`（col7 再生成）、`TickActions={MeleeHit}`、`HalfExtents=(1.5,0.5,1.5)`（col8 射程 150px）；
   - `HitReaction{Damage=35, HitstunMs=0, KnockbackX=0, LaunchY=0}`（SayaExIce.atk 无反应无位移直译；damage=col6 319% demo 折算）；
   - `ViewAnimId=AnimId.SayaExIceStart`（start.ani 840ms 循环——结晶个体演出降级为区域循环视觉）。
4. 重复施放替换：同 CasterId 两区先删后建（036 惯例）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| PO 20063 领域（tick 伤害+冰冻） | `SayaExZone`（036 直构） |
| PO 20064 冰之结晶（10 个离散实体、Ready→Start→Stay→End 生命周期） | `SayaExCrystalZone` 双区叠加降级（数值/节奏保留，个体性丢失）——**LSActionContext 无 CreateArea（R6-C5）挡住"区生区"**，离散化待该缺口 |
| 结晶寻敌/弹道（引擎内部） | 未考证 → 固定同心区 |
| 二觉替换语义（学习后是否替换基础萨亚槽位） | 未考证 → demo 两技并存（不做自动替换） |
| 冰属性/冰冻 Lv/读条/无色/前置 | 同 036 处理（跳过/直判/忽略） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SayaEx = 34` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `SayaExZone = 39`、`SayaExCrystalZone = 40` |
| AnimId | `AnimConfigRegistry.cs` | `SayaExGhost1/2=178/179`、`SayaExAreaStay=180`、`SayaExIceReady=181`、`SayaExIceStart=182`、`SayaExIceStay=183`、`SayaExIceEnd=184`（+叠层 185-186 预留） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | 6-8 个 json；图集 2 张（effect + effect_sayaex） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000 ms | 40000 直用 |
| 领域时长/间隔 | 10 s / 1 s | 直用 |
| 领域每跳 | col0 322%→3631% 魔攻 | 40/跳 |
| 领域半径/落点 | 350 px / 前 600 px | 3.5 单位 / 6 单位 |
| 冰冻概率/时长 | 10%→255% / 1.5→24.1 s | 10% / FreezeBuff 预设或 1.5s 变体 |
| 结晶伤害/节奏 | col6 319%→3602% / 每 0.5s | 35/跳（0.5s tick） |
| 结晶射程 | 150 px | HalfExtents 1.5 |
| 结晶命中反应 | SayaExIce.atk：none/无 push/lift | HitstunMs 0 / 0 / 0 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| SayaEx.skl | `.skl` 无子命令（含 [skill preloading image]/[consume item]/[second growtype maximum level] 12 槽） | 手抄 12 值；skl 子命令同前议（[skill preloading image] 016 已记档、12 槽二觉判定 R6-C4 已解） |
| sayaex.obj / sayaexice.obj | `.obj` 无子命令 | 手工映射双 Area（本档 §2.3 已给全相位表） |
| sayaexice.atk（+ 复用 saya.atk） | `.atk` 无子命令 | 手抄 5 值 |
| stay.ani | `[DELAY] 100000` 超长悬停帧 | L23 既有缺口：翻译钳制/约定手改 |
| 全部 .ani/.als | [use animation]/[add] 已支持；无攻击盒/无 flag | **现有 ani/als 子命令全覆盖** |

本技能翻译缺口 4 类（.skl/.obj/.atk/超长 DELAY）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 冰之结晶 = 独立实体 ×10（四相位生命周期 + 0.5s 再生 + 射程内寻敌） | **LSActionContext 无世界查询/CreateArea（R6-C5，第 3 消费方）** + 目标位置读取（R4-B18）；Area 自身无法派生子判定体 | 双区叠加：结晶伤害并入 0.5s tick 的同心小区域，个体演出降级为 start.ani 循环视觉——数值与节奏完整，离散感损失 |
| 结晶移动/寻敌规则 | 引擎内部不可读（未考证） | 同心固定区（不做寻敌） |
| 领域本体三层视图（本体双层+阵） | Area 视图通道现两层（ViewAnimId+ViewBackAnimId） | 本体并单层（036 同款简化） |
| 冰属性/冰冻 Lv | 元素系统/抗性对抗缺失 | 同 036（跳过/直判） |
| 二觉学习替换语义 | 技能等级系统缺失（R6-C1） | 两技并存 |
| stay.ani 100s 悬停 | L23 既有缺口 | 翻译钳制 |
| 读条/MP/无色消耗 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①static[2]=70、[4]=60、[5]=1000 语义（[4]/[5] 疑结晶存在时长/生成节奏参数，无消费证据）；②结晶"数量 10"= 并发上限还是总量（10s/0.5s=20 个生成周期 > 10，若每 0.5s 一个则 10 疑并发上限）；③结晶寻敌/弹道规则（引擎内部）；④领域常驻层染色值（疑冰蓝同 036，未逐帧验 RGBA）；⑤Appear1/2 双版本选择逻辑（036 同款遗留）；⑥学习后是否替换基础萨亚技能槽（二觉替换语义）。
- **缺口复证/新增消费方**：**LSActionContext 无 CreateArea（R6-C5）第 3 消费方**——"区域派生周期性子判定体"形态（结晶/弹幕阵类通用），建议该缺口立项时把本技能列入受益清单；目标位置读取门面（R4-B18）第 2 消费方（若做结晶寻敌）。
- **预分类纠偏上报（主循环记账）**：**文件名 Ex 结尾 ≠ TP 强化被动**——96 是 [type] active 的二觉替换型主动技（同目录另有 224-SayaExp 才是"强化-冰霜之萨亚"TP 被动，归其他批次）。后续 E 批遇 Ex 名先验 [type]。
- **给轮间经验**：鬼神族 PO 连号表增补：**20063 SayaEx / 20064 SayaExIce**（036 记的 20011-20016 区间之外的新成员，11258-11261 行）；吉格降临（241）嘶吼联动对基础/冰晶萨亚各有一个空壳标记 appendage（ap_scream_saya[_ex]_friend.nut，仅 buff 图标）——241 全链走读时从这两个标记入手。
