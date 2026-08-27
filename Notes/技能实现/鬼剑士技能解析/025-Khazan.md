# 刀魂之卡赞（Khazan）

> 技能ID 25 | 级别 B（召唤/光环增益） | 可实现性 ⛔（力量/智力属性数值无消费链 + 队伍/阵营判定双缺口；阵本体与视觉可完整表达） | 分析日期 2026-08-22 | 批次 B1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 刀魂之卡赞 | `skill\Swordman\Khazan.skl` [name] |
| 英文名 | Khazan（取 skl 文件名；[name2]="Summon Khazan"） | 同上 [name2] 实测 |
| 职业 | 鬼剑士全系可学（[skill fitness growtype] 0-5）；升级上限分档 `10 10 20 10 10 20`（鬼泣/剑影 20 级，其余 10 级） | 同上 |
| 学习等级 | 5 | 同上 [required level] |
| 最高等级 | 50 | 同上 [maximum level] |
| 类型 | active（**skill class 3 = 召唤/增益类**） | 同上 [type]/[skill class] |
| 指令 | → + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 10000 ms（pvp 20000） | 同上 [dungeon][cool time] |
| MP | 10 → 400（Lv1→Lv50） | 同上 [dungeon][consume MP] |
| 读条 | casting time 300 ms | 同上 [casting time] |
| 特殊消耗 | 无 | 同上 |
| static data | `250 250`（2 值：static[1]=250 已由 po_khazan.nut 实证为**光环半径 px**；static[0]=250 疑为召唤落点前偏移 px，未考证） | 同上 + po_khazan.nut |
| 一句话效果 | 在前方召唤鬼神卡赞领域，领域内**我方队员**增加力量和智力，持续 120 秒；重复施放则旧的消失、在前面重新召唤 | 同上 [explain] |

**level property 模板解码（3 列全明，L21 向量法）**：
- 持续时间 = col1 × 0.001 = **120s（恒定）**
- 增加力量 = col0 = **31 → 1656**（dungeon；Lv1→表末）
- 增加智力 = col2 = **31 → 1656**（与力量同列同值）

注：level info 表中段（第 28 行起）出现 1270→1147 的回落再爬升（pvp 表同构）——两段分档与 [growtype maximum level] 的 10/20 级上限相关，**分段规则未考证**。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无 pushState**（引擎内置召唤，F3）。load_state 仅注册 PO 行为脚本（124 行）：

```
124: IRDSQRCharacter.pushScriptFiles("character/swordman/ghostrelease/po_khazan.nut");
```

被召唤物：`passiveobject\character\swordman\khazan.obj` = **PO 20012**（passiveobject.lst:11157 实测）；`sqr\character\swordman\ghostrelease\` 目录另有 `ap_ghostrelease.nut`（空脚手架）。

### 2.2 引擎内置行为重建

```
施放（读条 300ms）：
  播召唤姿态（.chr [throw motion 2-1/2-2] = Summon1.ani 150ms / Summon2.ani 600ms，共用动作）
  在前方（static[0] 疑 250px）创建 PO 20012（卡赞领域）
  领域存续 col1 = 120s：周期性对半径 static[1]=250px 内的我方队员挂"力量/智力 +col0/col2"appendage
  （appendage 实体在 pvf\appendage 大树内，无确切子路径不检索——未考证，030 同例）
  领域动画相位：出现（Appear 1040ms）→ 常驻（Stay 720ms 循环）→ 消失（Disappear 320ms）
  重复施放：旧 PO 销毁，重新在前方召唤（explain 明示）
```

### 2.3 被动对象与 mod 附加层

**khazan.obj（PO 20012，纯视觉 + 引擎光环）**——`[etc motion]` 6 个动画、**无 [basic motion]、无任何 attack info**：

| .obj 节 | 值 |
|---|---|
| [floating height] | 0（贴地） |
| [pass type] / [piercing power] | pass all / 1000 |
| [etc motion] | Khazan1.ani、Khazan2.ani（鬼神本体双层循环 880ms）→ KhazanAreaAppear1/2.ani（领域出现 13 帧 1040ms ×2）→ KhazanAreaStay.ani（常驻 4 帧 720ms 循环）→ KhazanAreaDisappear.ani（消失 4 帧 320ms） |
| [name] | `卡赞` |

（etc motion 无 etc attack info 配对 = 纯视觉层，L13/L9 判定——领域**无攻击判定**，增益由引擎光环逻辑挂 appendage。）

**po_khazan.nut（38 行，mod 附加的攻击层）**——函数 `onCreat_Khazan` / `onProc_Khazan`（引擎按 PO 名约定调用）：

```
onCreat：攻击力 = 施法者 sq_GetPowerWithPassive(SKILL_GHOST_RELEASE=技能10, -1, col0) → 写入 PO 攻击信息（一次性）
onProc：每 interval（技能 10 col1）：
  遍历场上对象：isEnemy && 距离 <= size（sq_GetIntData(25,1)=250px）&& 活动对象
  → sq_SendHitObjectPacketWithNoStuck（以技能 10 的攻击力周期性伤害领域内敌人）
```

即 **mod 作者（qq506807329）给卡赞领域叠加了"鬼神解放"（mod 新技能 10）的攻击行为**——原版卡赞无攻击。本技能分析以原版语义为准，mod 层记档不还原。

### 2.4 动画关键帧表（全部实测）

| 动画 | 帧数 | 总时长 | 循环 | SET FLAG | 攻击盒 | 引用 img | 备注 |
|---|---|---|---|---|---|---|---|
| `passiveobject\...\animation\khazan1.ani` | 8 | 880ms | ✅ | 无 | 无 | `Character/Swordman/Effect/GhostKhazan1.img` | 鬼神本体层 1 |
| `khazan2.ani` | 8 | 880ms | ✅ | 无 | 无 | GhostKhazan2.img | 本体层 2（**+GRAPHIC EFFECT LINEARDODGE**，L15 已支持） |
| `khazanareaappear1.ani` / `2` | 13 | 1040ms | ❌ | 无 | 无 | SummonArea.img | 领域出现（SummonArea 帧**染色 `255 30 10 255` 红色调**，RGBA 已支持） |
| `khazanareastay.ani` | 4 | 720ms | ✅ | 无 | 无 | SummonArea.img | 常驻（同红色调） |
| `khazanareadisappear.ani` | 4 | 320ms | ❌ | 无 | 无 | SummonArea.img | 消失 |
| `character\swordman\animation\summon1.ani`（施法侧共用） | 3 | 150ms | ❌ | 无 | — | sm_body（帧 75-77） | [throw motion 2-1] |
| `summon2.ani` | 12 | 600ms | ❌ | F9=65534 | — | sm_body（帧 78-89） | [throw motion 2-2] / [buff motion] |

`.als` 边车：全部无（两侧 animation 目录 ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Khazan.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Khazan.skl` | ✅ 实测（250 行） | 数值（3 列全明） |
| 注册行 | load_state:124 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 仅 pushScriptFiles po_khazan.nut |
| PO 行为 nut | po_khazan.nut | `…\pvf\sqr\character\swordman\ghostrelease\po_khazan.nut` | ✅ 实测（38 行） | mod 攻击层（技能 10 驱动） |
| 空壳 | ap_ghostrelease.nut | `…\pvf\sqr\character\swordman\ghostrelease\` | ✅ 实测（25 行空壳） | 无逻辑 |
| PO 注册 | passiveobject.lst:11156-11157 | `…\pvf\passiveobject\passiveobject.lst` | ✅ 实测 | Khazan.obj = 20012 |
| PO 定义 | khazan.obj | `…\pvf\passiveobject\character\swordman\khazan.obj` | ✅ 实测 | §2.3 纯视觉 6 相位 |
| PO .atk | — | `…\passiveobject\character\swordman\attackinfo\`（grep khazan 仅 hardattackchargeafterkhazan.atk——另一技能的联动攻击） | ⛔ 原版无 | 增益领域无攻击 |
| PO .ani | khazan×6 | `…\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 角色 .chr | [throw motion 2-1/2-2] / [buff motion] | `…\pvf\character\swordman\swordman.chr` 934-938/949-950 行 | ✅ 实测 | Summon1/2.ani 共用召唤姿态 |
| 角色 .ani | summon1.ani / summon2.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 增益 appendage | （未知） | `…\pvf\appendage\`（大树无路径不检索） | 未考证 | 力量/智力 buff 载体 |
| .als | — | 两侧 animation 目录 | ⛔ 无 | — |
| 预载 img | [skill preloading image] | Khazan.skl 内 | ✅ 实测 | GhostKhazan1/2.img + SummonArea.img（与 .ani 引用互证） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89 召唤姿态） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | 必需 | ✅ 已在库 |
| GhostKhazan1.img / GhostKhazan2.img | sprite_character_swordman_effect.NPK（img 直属 `Character/Swordman/Effect/` 根，无子目录——按路径下划线化规则推导） | 鬼神本体双层 8 帧 | 必需（视觉还原） | ❌ 未入库 |
| SummonArea.img | 同上 | 领域阵（出现/常驻/消失，与 Bremen 共用） | 必需（视觉还原） | ❌ 未入库 |

缺失 img：必需 3 张（同一 NPK 一次提取）。技能 ⛔ 期间全部挂起。

## 5. 实现方案草案

**⛔ 暂缓（核心增益）**——两个已记档缺口叠加，030-DarkToleranceUp 同款判定：

1. **属性数值无伤害消费链**（缺失档，R1-A4 起 3 实证）：力量/智力 +31~1656 无 NumericType 键位（现有 Speed/Hp/MaxHp/Attack/Defense/ForbidMove，实测 NumericType.cs）且 MeleeHit 只读固定 HitReaction.Damage——即使挂上"力量 buff"也不改变任何伤害。
2. **队伍/阵营判定**（缺失档，R1-A3 首撞）："我方队员"增益需区分敌我（GetEnemies ��语义=除自己全部；单人 demo 退化为仅自身）。

**可先行落地的部分（视觉壳 + 结构占位）**——若做演示：
- `KhazanSkill : SkillLogic`——CD 10000、TotalTimeMs=600（读条 300 跳过，summon2 600ms）；`OnCast` `ctx.PlayAnim(召唤姿态 AnimId)` + `ctx.CreateAreaInFront(AreaIds.KhazanZone, 2.5 单位)`（static[0] 250px 推断）。
- `KhazanZone : AreaDefinition`——**结构与视觉零缺口**：`TotalTimeMs=120000`（col1 直用）、`HalfExtents=(2.5,0.5,2.5)`（半径 250px）、`TickTimeMs=1000`（光环刷新间隔，引擎内置值未考证，demo 取 1s）、`ViewAnimId=鬼神本体+领域 Stay 循环`、`ViewEndAnimId=领域 Disappear`（AreaDefinition 现有字段全部够用，FireCircle 先例）、重复施放挤掉旧区（Area 同点重建即可近似）。TickActions 留空（等消费链）。
- 增益落地时映射：`BuffIds.KhazanStrInt = 10` 预留（BuffDefinition + AddNumeric(新 STR/INT 键) + TickActions=AddBuff——Burn/Freeze 同构）。
- 注册点：`SkillIds.Khazan = 26`；`AnimIds 111-116`（GhostKhazan1/2、AreaAppear1/2、AreaStay、AreaDisappear 六个 json）；`AreaIds.KhazanZone = 17`。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000ms | 10000 直用 |
| 领域时长 | 120s（恒定） | 120s 直用（或 demo 缩 20s） |
| 光环半径 | 250px（static[1]） | 2.5 单位 |
| 召唤落点 | 前 250px（static[0]，推断） | CreateAreaInFront 2.5 |
| 增益 | 力量/智力 +31→1656 | 等 STR/INT 数值键 + 消费链 |
| 读条 | 300ms | 跳过（延后档） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Khazan.skl` | `.skl` 无子命令（3 列 + 预载 img 清单） | 手抄 3 值；`skl` 子命令同前议 |
| khazan×6.ani | `[SHADOW]`（规则表外）；khazan2 的 `[GRAPHIC EFFECT]`（**已支持**，L15——README 滞后） | SHADOW 跳过无碍；RGBA 染色（SummonArea 红色调）已支持 |
| khazan.obj | `.obj` 无子命令 | 本技能 6 相位纯视觉，手工映射 Area 三态（Appear→创建即播 / Stay→ViewAnimId / Disappear→ViewEndAnimId）即可，无需 obj 子命令 |

结论：**动画资源全部可被现有 ani 子命令翻译**（含 RGBA/GRAPHIC EFFECT）；实质缺口 `.skl`/`.obj` 子命令（重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 力量/智力增益（核心） | **缺失档：属性数值消费链**（NumericType 无 STR/INT 键 + 伤害端零消费） | ⛔ 主因；与远古记忆/不屈意志/流心:狂同队列等数值链立项 |
| "我方队员"光环目标 | **缺失档：队伍/阵营判定** | 单人 demo 可先只做自身（退化为自 buff 光环）——语义偏差需用户定夺 |
| 120s 长驻 + 重复施放替换 | 无（Area TotalTimeMs + 重建即可） | 直译 |
| 读条 300ms | 读条系统（延后档） | 跳过 |
| MP 10-400 | MP 系统（延后档） | 跳过 |
| mod 攻击层（po_khazan） | mod 内容 + 依赖 mod 技能 10 | 不实现 |
| 鬼神本体 2 层 + 领域 3 相位视觉 | 无（ViewAnimId/ViewBackAnimId/ViewEndAnimId 现有字段可配） | 直译 |

## 8. 存疑与缺口上报

**未考证项**
1. static[0]=250 的精确语义（召唤落点前偏移推断）。
2. 光环刷新间隔（引擎内置周期；demo 取 1s）与 appendage 刷新规则（领域内反复进出是否重复叠 buff）。
3. 增益 appendage 实体文件（pvf\appendage 大树无路径）。
4. level info 中段回落（1270→1147）的分段规则。
5. mod 技能 10（GHOSTRELEASE，`SwordmanNewSkill/GHOSTRELEASE.skl`）与 po_khazan 的完整联动（mod 内容，不还原，仅记档）。

**缺口上报（并入主循环汇总）**
1. **属性数值消费链 + 阵营判定双缺口的光环类第 2 实证**（030 首撞、本技能第 2 例、041-Bremen 第 3 例）——建议 00-总览 将"属性键位 + 伤害消费 + 阵营"打包为一个立项提案（光环类技能整体解锁的杠杆点）。
2. **召唤物"同点替换"语义**（重复施放销毁旧的）：AreaDefinition 无"互斥/替换"概念——实现期可用"同 CasterId 同 AreaId 先删后建"约定解决（非系统缺口，记实现惯例）。

**翻译工具缺口**：`.skl`/`.obj` 子命令（重复印证）；`[skill preloading image]` 节（016 首见，本技能第 2 例——预载清单仅客户端性能优化，建议工具忽略即可，记档）。

**给下轮的经验**：鬼泣鬼神召唤族（卡赞/普戾蒙/萨亚/罗刹/卡洛）共用一套结构——引擎内置召唤 + .obj 纯视觉 6 相位（本体 2 层循环 + 领域 Appear1/2/Stay/Disappear）+ **SummonArea.img 靠 RGBA 染色区分色系**（卡赞红 `255 30 10`、普戾蒙绿 `94 106 29`）+ load_state 只有 pushScriptFiles 一行（mod 攻击层）。后续萨亚（20013）/罗刹直接按此链路查，不必再探；PO 编号连号（20012 卡赞/20013 萨亚/20014 普戾蒙）。
