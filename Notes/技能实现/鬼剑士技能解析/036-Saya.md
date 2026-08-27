# 冰霜之萨亚（Saya）

> 技能ID 36 | 级别 B（鬼神召唤/攻击领域） | 可实现性 ✅（直接——鬼神召唤族首个可完整落地样本：周期伤害 + 概率冰冻 + 六相位视觉全部有先例；读条/无色消耗/冰冻等级跳过） | 分析日期 2026-08-22 | 批次 B3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冰霜之萨亚 | `skill\Swordman\Saya.skl` [name] |
| 英文名 | Saya（取 skl 文件名；[name2]="Summon Saya"） | 同上 [name2] 实测 |
| 职业 | 鬼泣（[skill fitness growtype]=**仅 2**） | 同上 |
| 学习等级 | 30（**前置：侵蚀普戾蒙 41 Lv3**，[pre required skill] `41 3` 实测——卡赞→普戾蒙→萨亚 前置链） | 同上 [required level]/[pre required skill] |
| 最高等级 | 70（鬼泣/剑影档 50：`0 0 50 0 0 50`） | 同上 [maximum level]/[growtype maximum level] |
| 类型 | active（**skill class 3 = 召唤类**） | 同上 [type]/[skill class] |
| 指令 | →→ + Space（BUFF 键） | 同上 [command] |
| CD | 20000 ms（pvp 30000，pvp 另有 start cool time 10000） | 同上 [cool time] |
| MP | 150 → 1260 | 同上 [consume MP] |
| 读条 | casting time 100 ms（dungeon；pvp 500） | 同上 [casting time] |
| 特殊消耗 | **[consume item] `3037 1 1`——施放消耗道具 3037 ×1**（无色小晶块类） | 同上 [consume item] |
| static data | `450 250 70 1000`（static[1]=250 光环半径、static[3]=1000 伤害间隔——由 level property 向量实证；static[0]=450 召唤落点前偏移；static[2]=70 未考证） | 同上 [static data] + level property |
| 一句话效果 | 召唤萨亚：领域内敌人每 1 秒受冰属性魔法伤害，概率冰冻，持续 10 秒 | 同上 [explain] |

**level property 模板解码（5 列，L21 向量法全解，Lv1→Lv70 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 持续时间 | (-1, 1, ×0.001) | col1 = 10000 → **10 s 恒定** |
| 伤害间隔 | **(3, 3, ×0.001)** → static[3] | 1000 → **1 s 恒定** |
| 魔法攻击力 | (-1, 0, ×1.0) | col0 = **386 → 3136 %**（每跳伤害倍率） |
| 冰冻机率 | (-1, 2, ×0.1) | col2 = 90→744 → **9% → 74.4%** |
| 冰冻持续时间 | (-1, 4, ×0.001) | col4 = **1500 → 6947 ms**（1.5s → 6.9s） |
| 冰冻Lv | (-1, 3, ×1.0) | col3 = **Lv31 → Lv169**（对抗敌人冰冻抗性的等级，我们无对抗系统→跳过） |

## 2. 技能逻辑走读

### 2.1 注册与文件链

**纯引擎内置召唤（比卡赞/普戾蒙更纯——连 mod 攻击层都没有）**：

- load_state **无 pushState、无 pushScriptFiles**（grep saya 无命中；124-126 行的 ghostrelease pushScriptFiles 只有 khazan/bremen，epidemicrasa 被注释）；
- `sqr\character\swordman\` 无 saya 目录；appendage 无 ap_saya。

被召唤物：`passiveobject\character\swordman\saya.obj` = **PO 20013**（passiveobject.lst:11158-11159 实测——20012 卡赞 / **20013 萨亚** / 20014 普戾蒙 连号，025/041 结论第三次印证）。

### 2.2 引擎内置行为重建

```
施放（读条 100ms）：
  播召唤姿态（Summon1.ani 150ms / Summon2.ani 600ms 共用，与卡赞/普戾蒙同款）
  消耗 MP + 道具 3037×1
  在前方 static[0]=450px 创建 PO 20013（萨亚领域）
领域存续 col1 = 10s：
  每 static[3]=1000ms：对半径 static[1]=250px 内敌人
    → 魔法伤害 col0%（386%~3136% 冰属性）
    → 概率冰冻 col2×0.1（9%~74.4%），持续 col4（1.5~6.9s），冰冻 Lv col3
    （命中参数 = Saya.atk；冰冻三参由 .atk [active status] freeze 运行时写入——
     .atk 静态值 `0 0 0` 为占位，引擎按 skl 列填充，L6 同构）
  领域动画相位：出现 → 常驻（循环）→ 消失
重复施放：旧 PO 销毁重召（鬼神族共用语义，explain 家族同款）
```

**与卡赞/普戾蒙的机制差异**（家族三���本对照）：

| | 卡赞 20012 | 萨亚 20013 | 普戾蒙 20014 |
|---|---|---|---|
| 目标 | 我方 buff | **敌方攻击** | 敌方 debuff |
| .obj 攻击信息 | 无（纯视觉） | **有（Saya.atk）** | 无（纯视觉） |
| mod 攻击层 | po_khazan（技能10） | **无** | po_bremen（技能10） |
| 落点 static[0] | 250 | **450** | 300 |
| 领域半径 static[1] | 250 | **250** | 250 |

——萨亚是鬼神族中唯一"攻击信息在 .obj 本体"的阵：**周期伤害全部可从 .obj/.atk/.skl 三方直读，零未考证**。

### 2.3 被动对象（saya.obj，完整实测）

| .obj 节 | 值 |
|---|---|
| [floating height] | 0（贴地） |
| [pass type] / [piercing power] | pass all / 1000（全穿透多目标） |
| **[attack info]** | `AttackInfo/Saya.atk`（**本族唯一带攻击信息的 .obj**） |
| [etc motion] | Saya1.ani、Saya2.ani（鬼神本体双层循环 8 帧 880ms）→ SayaAreaAppear1/2.ani（出现 13 帧 1040ms ×2）→ SayaAreaStay.ani（常驻 4 帧 720ms 循环）→ SayaAreaDisappear.ani（消失 4 帧 320ms） |
| [name] | `萨亚` |

**Saya.atk 实测**（`passiveobject\character\swordman\attackinfo\saya.atk`）：

```
[attack type] magic / [elemental property] water element（冰属性——无元素系统，跳过）
[damage reaction] none（tick 不打断）
[push aside] 30 / [lift up] 10（轻微位移）
[attack direction] hit horizon
[active status] freeze 0 0 0（占位，运行时写概率/等级/时长）
[hit info] blow / [no blood] 10 1.0
```

另有 `sayaexice.atk`（强化版）/`scream_saya_*.atk`（zigadvent 联动，同目录 grep 命中）——非本技能，记档。

### 2.4 动画关键帧表（全部实测）

| 动画 | 帧数 | 总时长 | 循环 | 引用 img | 备注 |
|---|---|---|---|---|---|
| `passiveobject\...\animation\saya1.ani` | 8 | 880 ms | ✅ | `Character/Swordman/Effect/GhostSaya1.img` | 鬼神本体层 1 |
| `saya2.ani` | 8 | 880 ms | ✅ | GhostSaya2.img | 本体层 2 |
| `sayaareaappear1.ani` / `2` | 13 | 1040 ms | ❌ | SummonArea.img | 领域出现——**染色 `24 85 165 255`（冰蓝色调，RGBA 已支持）** |
| `sayaareastay.ani` | 4 | 720 ms | ✅ | SummonArea.img | 常驻（同冰蓝） |
| `sayaareadisappear.ani` | 4 | 320 ms | ❌ | SummonArea.img | 消失 |
| 施法侧共用 | summon1/2.ani | 150/600 ms | ❌ | sm_body（帧 75-89） | 全召唤族共用 |

帧数/时长与卡赞逐项相同（025 实测对照）——**同一 SummonArea.img 第三种染色**（卡赞红 `255 30 10` / 普戾蒙绿 `94 106 29` / **萨亚冰蓝 `24 85 165`**），一张 img 三阵复用，RGBA 染色是家族视觉区分的全部秘密。

`.als` 边车：无（两侧 animation 目录 ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Saya.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Saya.skl` | ✅ 实测 | 数值（5 列全解）+ 前置 41 Lv3 + 消耗道具 3037 |
| 注册行 | —（无 pushState/pushScriptFiles） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无 | 纯引擎召唤（F3） |
| 主 nut | —（不存在） | `…\sqr\character\swordman\`（ls 57 项） | ⛔ 无 | 引擎内置 |
| PO 注册 | passiveobject.lst:11158-11159 | `…\pvf\passiveobject\passiveobject.lst` | ✅ 实测 | Saya.obj = 20013 |
| PO 定义 | saya.obj | `…\passiveobject\character\swordman\saya.obj` | ✅ 实测 | §2.3 六相位 + attack info |
| PO .atk | saya.atk | `…\passiveobject\character\swordman\attackinfo\saya.atk` | ✅ 实测 | 周期命中参数 |
| PO .ani | saya×6 | `…\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.4（sayaex\ 子目录 + scream_saya* 为强化/联动技，非本文） |
| 角色 .chr / .ani | [throw motion 2-1/2-2] Summon1/2.ani | `…\pvf\character\swordman\` | ✅ 实测 | 共用召唤姿态 |
| 角色 .atk | — | `…\character\swordman\attackinfo\` | ⛔ 无（伤害在 PO 侧） | — |
| .als | — | 两侧 animation 目录 | ⛔ 无 | — |
| 装备层 | —（共用姿态无专属图层） | `…\equipment\character\swordman\avatar\` | 未查 | 同 khazan 结论 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法姿态 | 必需 | ✅ 已在库 |
| GhostSaya1.img / GhostSaya2.img | sprite_character_swordman_effect.NPK（Effect 根推导） | 鬼神本体双层 8 帧 | 必需（视觉还原） | ❌ |
| SummonArea.img | 同上（**卡赞/普戾蒙共用同一张**——三阵一图，RGBA 分色） | 领域阵冰蓝染色 | 必需（视觉还原） | ❌ |

缺失 img：必需 3 张、同一 NPK 一次提取（与 025/041 共享 SummonArea.img——鬼泣三阵合计 5 张）。

## 5. 实现方案草案

**✅ 直接可实现**（全部有先例，零新机制）：

- **`SayaSkill : SkillLogic`**（同 FireCircleSkill 施法侧 + khazan 结构）——CD 20000、TotalTimeMs=600；`OnCast`：`ctx.PlayAnim(AnimId.Summon2 姿态)` + `ctx.CreateAreaInFront(AreaIds.SayaZone, (FP)45/10)`（static[0]=450px → 4.5 单位）。
- **`SayaZone : AreaDefinition`**（FireCircleArea + 025-Khazan 视觉结构合体）：
  - `TotalTimeMs = 10000`（col1 直用）
  - `TickTimeMs = 1000`（static[3] 直用）、`TickActions = {MeleeHit}`
  - `HalfExtents = (2.5, 0.5, 2.5)`（static[1]=250px）
  - `HitReaction { Damage = 40, HitstunMs = 0, KnockbackX = 30, LaunchY = 10, ProcBuffId = BuffIds.Freeze, ProcChance = 9 }`——push/lift 直译 Saya.atk；**概率冰冻走 ProcBuffId/ProcChance（MonsterIceBreath 先例），TickActions 连 AddFreezeBuffAction 都可省**
  - `ViewAnimId = AnimId.SayaAreaStay`（冰蓝染色常驻层）、`ViewBackAnimId = AnimId.SayaGhost`（鬼神本体，双层同 khazan boomback 用法）、`ViewEndAnimId = AnimId.SayaAreaDisappear`（消失收尾，FireCircleEnd 先例）
- **复用现有 `BuffIds.Freeze`**（3.5s 定身）——DNF 冰冻时长 col4=1.5s(Lv1)：demo 用 FreezeBuff 预设即可；要贴原值则加 `SayaFreezeBuff : BuffDefinition`（TotalTimeMs=1500，ForbidMove 开/关，复制 Freeze 同构）。
- **重复施放替换**：同 CasterId 同 AreaId 先删后建（025 §8 实现惯例，非系统缺口）。
- 注册点：`SkillIds.Saya = 31`；`AreaIds.SayaZone = 31`；`AnimIds 150-155`（SayaGhost=Saya1、SayaGhost2=Saya2、SayaAreaAppear1/2、SayaAreaStay、SayaAreaDisappear 六个 json——**注意三阵 .ani 各自独立不能共用 json**，RGBA 在帧数据里，041 同款提醒）；Appear 相位并入创建即播或跳过（Area 无 ViewStartAnimId，khazan 同款处理）。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000 ms | 20000 直用 |
| 领域时长 | 10 s 恒定 | 10000 直用 |
| 伤害间隔 | 1 s | 1000 直用 |
| 每跳伤害 | 386%→3136% 魔攻（10 跳） | 固定 40/跳（总计 400） |
| 光环半径 | 250 px | 2.5 单位 |
| 召唤落点 | 前 450 px | CreateAreaInFront 4.5 |
| 冰冻概率 | 9%→74.4% | 9%（Lv1 直用，演示明显可调 25%） |
| 冰冻时长 | 1.5 s→6.9 s | FreezeBuff 3.5s 预设 or 新 1.5s 变体 |
| 击退/浮空 | push 30 / lift 10 / 无反应 | KnockbackX 30 / LaunchY 10 / HitstunMs 0 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Saya.skl` | `.skl` 无子命令（含 [consume item]/[pre required skill] 节） | 手抄 6 值；`skl` 子命令同前议（消耗道具关系建议进输出） |
| `saya.obj` | `.obj` 无子命令 | 六相位手工映射 Area 三态（khazan 同款）；本技能带 attack info——`obj` 子命令立项时"相位序列 + atk 配对"建模（064 §8 同议） |
| `saya.atk` | `.atk` 无子命令（含 [active status] freeze 占位 + [hit info] blow + [no blood]） | 手抄 5 值可行；atk 子命令字段设计时 [active status] 运行时参数语义需建模 |
| saya×6.ani | `[SHADOW]`（规则表外） | 跳过无碍；RGBA 染色（冰蓝）已支持——本技能视觉关键 |

结论：动画资源全部可被现有 ani 子命令翻译；实质缺口 `.skl`/`.obj`/`.atk`（重复印证，均手抄量小）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 周期魔法伤害 | **无缺口**（Area Tick + MeleeHit + 固定 Damage） | 直译（主干 ✅） |
| 概率冰冻 | 无缺口（ProcBuffId/ProcChance + LSRng 已落地） | 直译 |
| 冰冻 Lv（对抗抗性） | 缺失档：抗性对抗公式（041 记档） | 概率直判（不查等级） |
| 冰属性（water element） | 缺失档：元素属性系统 | 跳过（无属性白伤） |
| 读条 100ms | 延后档 | 跳过 |
| MP 150~1260 / 道具 3037×1 | 延后档（MP）/无道具系统 | 跳过 |
| 前置技能（普戾蒙 Lv3） | 无技能树 | demo 直接可学 |
| 重复施放替换 | 无（先删后建惯例） | 直译 |
| 六相位视觉 | 无（ViewAnimId/ViewBackAnimId/ViewEndAnimId 现有字段够用） | 直译（Appear 并入创建即播） |
| pvp 参数（CD 30s/读条 500ms） | 无 pvp 场景 | 不用 |

## 8. 存疑与缺口上报

**未考证项**
1. static[2]=70 语义（疑浮空高度/特效偏移——前两阵此槽分别为 70（萨亚）与无对应，家族内不统一）。
2. Appear1 与 Appear2 双版本的选择逻辑（引擎按何条件选一/选二——卡赞同款未考证，两版帧数相同疑镜像/层次差异）。
3. 冰冻 Lv col3 的对抗公式细节（无对抗系统，跳过项）。
4. [skill under cooltime effect] 空节（CD 中特效声明，无值）。

**缺口上报（并入主循环汇总）**
1. **无新系统级缺口**——鬼神召唤族三个样本（buff 阵 ⛔ / debuff 阵 ⛔ / **攻击阵 ✅**）恰好覆盖三类语义边界，可作 00-总览"召唤阵类可实现性判据"的完整对照表：效果落在伤害/控制（✅）还是属性数值（⛔）。
2. **周期 tick 的"无硬直"手感**：Saya.atk damage reaction=none——我们 HitstunMs=0 即等价（受击闪白仍会播，视觉差异小）。记档：tick 类 HitReaction 的 HitstunMs=0 是"不打断"语义的正确参数，非缺省 500。

**翻译工具缺口**：`.skl`/`.obj`/`.atk` 子命令（重复印证）；`[consume item]` 节首见（skl 子命令设计输入）。

**给下轮的经验**：**鬼神族 PO 编号连号 20011-20016 已实证四席**（BlastBloodSub 20011 / Khazan 20012 / **Saya 20013** / Bremen 20014 / TombStoneRain 20015 / TombStone 20016）——后续鬼泣阵技（罗刹/卡洛）按 lst 11150-11170 区间直查。萨亚是"攻击阵"完整直读样本：**.obj 带 [attack info] = 判定全在 PO 侧可直译；无 [attack info] = 纯视觉阵（增益走引擎，撞数值链）**——这个二分法可以直接判定鬼神族的 ✅/⛔，不用再逐个走引擎行为。
