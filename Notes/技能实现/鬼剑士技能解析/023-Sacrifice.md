# 嗜血（Sacrifice）

> 技能ID 23 | 级别 B | 可实现性 ⛔（核心"血气技能增伤联动"撞属性数值无伤害消费链；HP 自耗/蓄力演出/满蓄冲击波可先行） | 分析日期 2026-08-22 | 批次 B4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 嗜血（[name2] 同名） | `skill\Swordman\Sacrifice.skl [name]` |
| 英文名 | Sacrifice（取 skl 文件名；[name2] 非英文） | 同上 |
| 职业 | 狂战士（[skill fitness growtype] 3） | 同上 |
| 学习等级 | 35（**前置：技能 24 Lv1**，[pre required skill] `24 1` 实测——24 为血气系） | 同上 [required level] / [pre required skill] |
| 最高等级 | 50 | 同上 [maximum level] |
| 类型 | active（skill class 2，增益/状态类） | 同上 [type]/[skill class] |
| 指令 | ↑↓ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 3000 ms | 同上 [dungeon][cool time] |
| MP | 0（本 pvf 数值） | 同上 [dungeon][consume MP] `0 0` |
| 读条 | casting time 100 ms | 同上 |
| 特殊消耗 | 施放自损 HP：static[0]=**20%** 上限 HP（"爆发血气消减大量HP"）；蓄气期间每 0.5 秒再损 static[5]=**5%** | 同上 [static data] + explain + `sqr\character\swordman\swordman_throw.nut` case 23 实证 |
| static data | `20 -1 400 500 6 5 100`（[0]=消减HP%、[4]=6（嗜魂封魔斩吸附总数上限）、[5]=5（蓄气每0.5s HP减%）；[1]/[2]/[3]/[6] 语义未考证） | 同上 [static data] |
| 一句话效果 | 自损大量 HP 爆发血气，持续期间施放者对**出血状态敌人**使用血气系列技能（血之狂暴/怒气爆发/崩山裂地斩/嗜魂封魔斩/血气爆发）时大幅提升效果；按住指令键蓄气，蓄气中每 0.5 秒力量/物攻递增、HP 递减，最大蓄气时触发[嗜血冲击波]（范围出血波） | 同上 [explain] + [level property] 模板 |

**level property（18 列，Lv1 代表值）**：消减 HP=static[0]=20%；持续时间=col1×0.001=**2s**（Lv1→Lv50=3s，本 pvf 数值偏短，未考证是否 mod 改动）；出血持续增加 col3=145s?（血之狂暴联动）；HP 恢复增加 col4=50；怒气爆发/崩山裂地斩/血气爆发对出血增伤 col5/col6/col4=50%（复用列）；每击吸收 HP col7=36；每秒 HP 消减 col8=800；噬魂之手系出血 Lv col9=37、攻击力 col10=24、持续 col11=3s；蓄气力量 col12=9、物攻 col13=1；**最大蓄气冲击波**：出血攻击力 col14=1033（源 -4，L21 未解档）、出血 Lv col15=37、持续 col16=7s、**冲击波范围 col17=500px**。
（col0=600000 与 col2=39 无模板引用，引擎侧消费，未考证；Lv70 行多列数值反常回落，与 041-Bremen 记档的中段重排现象同类。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

**skill 23 在 `swordman_load_state.nut` 中无注册行**（实测 grep sacrifice 仅一行）：

```
121: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/sacrifice/sacrifice.nut", "swordman_sacrifice", 222, SKILL_SWORDMAN_SACRIFICE);
```

⚠ **名称陷阱（本技能最大坑）**：这一行属于**技能 222 = `swordman/swordman_sacrifice.skl`（血气分流，30 级）**，不是本技能（23，`Swordman/Sacrifice.skl` 嗜血）。证据链：
- `swordman_header.nut:88`：`SKILL_SWORDMAN_SACRIFICE <- 222`（与 lst 222 号位吻合，实测）；
- `sacrifice.nut` 内容全为 `SKILL_SWORDMAN_SACRIFICE`（=222）的读数，其 [name]=血气分流（`skill\swordman\swordman_sacrifice.skl` 实测）。

嗜血（23）本体属 **F3 引擎内置**：无 pushState、无同名 nut。它的施放载体是**引擎通用 THROW 状态 13**——`sqr\character\swordman\swordman_throw.nut` 中有 case 23 的专码（实测）：

```
// onAfterSetState_swordman_throw: 进入 THROW 态且 throwIndex==23
case 23:
    obj.sq_IsEnterSkillLastKeyUnits(23);              // 检测按住技能键
    obj.sq_PlaySound("BLOODRIVEN_CAST", 8903);        // 蓄气音效
// onProc_swordman_throw: throwIndex==23 且仍按住(throwState==0)
    每帧扣 0.1% 上限 HP（hpReduce = hpMax*0.001，钳到最少 1，不扣死——newHp<=0 时置 1）
```

即：嗜血 = 引擎把角色置入 THROW(13) 状态、throwIndex=23 的**按住蓄气型增益**；蓄气中持续掉血，松开（throwState 变 1）时结算 buff。

### 2.2 引擎内置行为重建（THROW 状态 + 模板数据反推）

```
施放（读条 100ms）：
  进入 THROW 状态 13（throwIndex=23），播蓄气特效 sacrificecharge1.ani(+als 层叠 charge2)
  播蓄气音效；按住指令键期间每帧自损 0.1% 上限 HP（≈每 0.5s 5%，static[5]）
  蓄气每 0.5 秒效果递增：力量 +col12、物攻 +col13（"蓄气时每0.5秒效果变化"）
  最大蓄气：触发[嗜血冲击波]——以自身为中心、半径 col17=500px 的出血波
    （出血攻击力 col14 / Lv col15 / 持续 col16=7s；表现资源未见专属 PO，未考证载体——推断引擎内置）
松开键（结算）：
  自损 static[0]=20% 上限 HP（explain"施放时爆发血气"）
  获得嗜血 buff：持续 col1×0.001=2~3s；期间对出血状态敌人使用血气系列技能时
    各技能增伤（血之狂暴：出血持续+col3/HP恢复+col4；怒气爆发/崩山裂地斩：+col5/col6%；
    嗜魂封魔斩：吸附上限 static[4]=6、每击吸HP col7；血气爆发：+col4%、每秒HP消减+col8）
  buff 姿态动画 StaySacrifice.ani（16 帧 1550ms，实测无 SET FLAG/攻击盒）
  释放特效 sacrifice1.ani + .als（层-1 叠 sacrifice2.ani）
```

### 2.3 被动对象 / appendage

嗜血本体**无 PO、无 appendage 脚本**（引擎内置状态；`sqr\character\swordman\appendage\` 下只有 `ap_sacrifice.nut`，实测内容为**技能 222 血气分流**的 buff（物攻/攻速/移速/物防/魔防 changeStatus 参数）——与本技能无关，勿混淆）。血气技能增伤联动由引擎在**各血气技能的伤害结算处**查询嗜血 buff 实现（推断，无脚本可证）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\StaySacrifice.ani`（buff 常驻姿态，.chr [etc motion] #114 实测） | 16 | 1550ms（F0-8=50，F9-14=100，F15=500） | 无 | 无 | 仅引 `sm_body%04d.img`（L16：已在库） |
| `character\swordman\effect\animation\sacrifice1.ani`（释放爆发）+ `.als`（层-1 叠 sacrifice2） | 12 | 未逐帧加总 | 无 | 无 | GiveBlood1.img |
| `sacrifice2.ani`（叠层） | 12 | 同上量级 | 无 | 无 | GiveBlood2.img |
| `sacrificecharge1.ani`（蓄气）+ `.als`（层-1 叠 charge2） | 6 | 未逐帧加总 | 无 | 无 | GiveBlood1.img |
| `sacrificecharge2.ani`（蓄气叠层） | 6 | 同上 | 无 | 无 | GiveBlood2.img |
| 嗜血冲击波表现 | — | — | — | — | **未定位**（无专属 PO/ani，推断引擎复用通用资源；§8 存疑） |

`.als` 边车：sacrifice1.ani.als / sacrificecharge1.ani.als 各一条 `[use animation]`+`[add]`（帧0/层-1），实测常规结构。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Sacrifice.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Sacrifice.skl` | ✅（295 行） | 技能数据（18 列等级数据 + static 7 值） |
| 注册行 | —（本技能无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无（121 行属技能 222，§2.1 勘误） | 引擎内置（F3） |
| THROW 载体 | swordman_throw.nut case 23 | `…\pvf\sqr\character\swordman\swordman_throw.nut` | ✅（67 行，case 23 实测） | 蓄气掉血 + 音效 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\sacrifice\sacrifice.nut`（属技能 222） | ⚠ 同名异技 | 勿当本技能逻辑读 |
| appendage | —（ap_sacrifice.nut 属技能 222） | `…\pvf\sqr\character\swordman\appendage\ap_sacrifice.nut` | ⚠ 同名异技 | 血气分流的 buff |
| .chr 条目 | [etc motion] #114 = StaySacrifice.ani | `…\pvf\character\swordman\swordman.chr` 1087 行 | ✅ 实测 | buff 常驻姿态 |
| 角色 .ani | StaySacrifice.ani | `…\pvf\character\swordman\animation\StaySacrifice.ani` | ✅ | §2.4 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\`（grep sacrifice 无） | ⛔ 无 | 增益技无命中 |
| 特效 .ani | sacrifice1/2、sacrificecharge1/2（+2 个 .als） | `…\pvf\character\swordman\effect\animation\` 根目录 | ✅ 实测 | 蓄气/释放视觉 |
| 预载 img | GiveBlood1/2.img | Sacrifice.skl [skill preloading image] | ✅ 实测 | 与特效 ani 引用一致 |
| 装备层 | —（未查；StaySacrifice 若做换装需补） | `…\pvf\equipment\character\swordman\avatar\` | 未考证 | buff 姿态换装层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 姿态动画帧 | 必需（共享） | ✅ 已在库 |
| GiveBlood1.img | sprite_character_swordman_effect.NPK（Effect 根目录） | 蓄气/释放特效 | 必需（视觉还原） | ❌ 未入库 |
| GiveBlood2.img | 同上 | 叠层特效 | 必需（视觉还原） | ❌ 未入库 |

缺失 img：必需 2 张（同一 NPK 一次提取）。

## 5. 实现方案草案（⛔ 级：核心联动不可表达，仅列可先行壳）

**⛔ 主因**：技能的全部价值在"对**出血状态敌人**的**其他技能**增伤"——命中时查询目标是否出血（Buff 查询门面，缺失档）+ 增伤注入其他技能伤害公式（**属性数值无伤害消费链**，R1-A4 已记最重缺口，远古记忆/不屈意志/流心:狂三实证后的第 4 例）+ 蓄力输入（R3-A15 按住蓄力共性缺失）。三者叠加，主体验在当前系统等于空转。

**可先行落地的壳（视觉演示级）**：
- `SacrificeSkill : SkillLogic`——CD 3000、`MinCastHpPct = 21`（static[0]=20% 自损的门槛对齐，BloodBoomSkill 同构）；`OnCast`：`ctx.ConsumeCasterHp(maxHp×20%)` + `ctx.PlayAnim(AnimId.SacrificeStay)` + `ctx.AddBuffToSelf(BuffIds.SacrificeBuff)`。
- `SacrificeBuff : BuffDefinition`——`TotalTimeMs=2000`（col1 Lv1 直用）、TickTimeMs=500（蓄气每 0.5s 演出档）、AddActions/TickActions 留空（数值消费链落地后回填攻/防键位）；Tick 演出可选 `ConsumeCasterHp(maxHp×5%)` 还原蓄气掉血。
- 满蓄冲击波（若做"瞬发最大蓄"简化）：`CreateArea(AreaIds.SacrificeWave, 自身位置)`，`HalfExtents=(5.0,0.5,5.0)`（col17=500px）、`EnterActions={MeleeHit, AddBleedBuff}`、`HitReaction{Damage=80, HitstunMs=500, ProcBuffId=BuffIds.Bleed, ProcChance=100}`——纯增项，机制全现成。
- 注册点：`SkillIds.Sacrifice = 27`；`BuffIds.SacrificeBuff = 14`（预留）；`AnimIds 132-134`（Stay=132、Burst(sacrifice1+als)=133、Charge(sacrificecharge1+als)=134）。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 3000ms | 3000 直用 |
| buff 时长 | col1×0.001 = 2s（Lv1） | 2000ms |
| 施放自损 | static[0]=20% 上限 | 20%（MinCastHpPct=21 挡空血） |
| 蓄气掉血 | 0.1%/帧（≈5%/0.5s） | Tick 500ms×5% |
| 冲击波范围 | col17=500px | 半尺寸 5.0 单位 |
| 冲击波出血 | 攻击力 col14=1033/Lv37/7s | BleedBuff 现值（3s/15每秒） |
| 增伤联动（⛔） | col4-col7 各技能各异 | 等"属性数值消费链"立项 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Sacrifice.skl` | `.skl` 无子命令（18 列 level info + [pre required skill] + [skill preloading image]） | 手抄关键 8 值（§5 已列）；`skl` 子命令为既有建议，本技能 18 列是迄今最宽表，纳入该子命令的矩阵输出设计 |
| sacrifice1/charge1 `.als` | 无缺口（`[use animation]`/`[add]` 常规） | 现有 als 子命令覆盖 |
| StaySacrifice.ani / 4 个特效 .ani | 常规节 | 现有 ani 子命令覆盖 |

结论：动画/边车资源全部可被现有 ani/als 子命令翻译；实质缺口仅 `.skl`（既有）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 对出血敌人血气技能增伤（核心） | **缺失档**：属性数值无伤害消费链 + 目标 Buff 查询门面（R2-A8 记档） | ⛔ 主因；与远古记忆/不屈意志/流心:狂同队列，等消费链立项 |
| 按住蓄气（蓄力中力量/物攻递增、满蓄冲击波） | 缺失档：按住蓄力输入（R3-A15 四技共性） | demo 瞬发：施放即结算 + 直接给满蓄冲击波；蓄力版等输入扩展 |
| 蓄气掉血 0.1%/帧 | 已有（ConsumeCasterHp + 钳不扣死由 MaxHp 比例控制；"不扣死"需自身判断，现门面无下限钳制） | 演出档用 Tick 500ms×5% 近似 |
| buff 期间各血气技能分支加成 | 缺失档：跨技能数值查询（R3-A11 同源） | 一并等消费链 |
| buff 常驻姿态换装（avatar 层 StaySacrifice） | 换装系统（缺失档，未展开） | demo 单层 sm_body |
| 冲击波表现载体未定位 | 未考证（§8） | 用通用圆形 Area 特效占位 |

## 8. 存疑与缺口上报

**未考证项**
1. 嗜血冲击波的判定/表现载体（无专属 PO/atk/ani；推断引擎内置通用波，资源未定位）。
2. col0=600000、col2=39 的引擎消费语义；持续时间 2~3s 与原版认知（20s+ 量级）出入较大——疑 mod 改值，按本 pvf 数据记录。
3. StaySacrifice.ani 的装备层变体是否存在（avatar 树未查）。
4. 蓄气递增（每 0.5s 力量/物攻变化）的精确步进公式（模板只有 col12/col13 终值）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **同名技能双陷阱**：`Sacrifice.skl`(23) 与 `swordman_sacrifice.skl`(222) 同名异技、`sacrifice/sacrifice.nut` 属 222——**"nut 目录名/状态名与技能同名"不可作为归属依据**，必须以 pushState 第 5 参（或 header 常量）对 lst 反查。建议补进 _轮间经验（F 系）。
2. 属性数值无伤害消费链第 4 例实证（嗜血），且是首个"条件增伤（对出血敌人）"形态——比无条件增伤多一层目标 Buff 查询需求，消费链立项时需一并设计。

**翻译工具缺口**：`.skl` 子命令（既有建议，18 列宽表新证）。
