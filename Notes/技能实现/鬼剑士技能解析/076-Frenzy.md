# 血之狂暴（Frenzy）

> 技能ID 76 | 级别 B（形态/buff 类——狂战士核心形态：双刀流改造） | 可实现性 ⛔（普攻派生改造撞自身 Buff 查询门面 + 属性数值消费链 + 击杀回血/跨技能 CD 门面四重缺口；简化方向=独立双刀连段技能） | 分析日期 2026-08-22 | 批次 B2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 血之狂暴 | `skill\Swordman\Frenzy.skl` [name] |
| 英文名 | Frenzy（取 skl 文件名；[name2]="Frenzy"） | 同上 [name2] 实测 |
| 职业 | 狂战士（[skill fitness growtype]=3；升级上限仅狂战 20 级） | 同上 |
| 学习等级 | 17 | 同上 [required level] |
| 最高等级 | 50 | 同上 [maximum level] |
| 类型 | active（skill class 2）；[auto cooltime apply] 0 | 同上 [type] |
| 指令 | ↓↑ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 10000 ms | 同上 [dungeon][cool time] |
| MP | 10 → 35 | 同上 [consume MP] |
| 特殊消耗 | **施放扣 HP（col0）+ 每 10 秒持续扣 HP（col1）**；[consume item] 3037×1（无色小晶体，道具系统外） | 同上 [level property] / [consume item] |
| static data | `350 10000 110 100`（4 值：static[1]=10000 疑即"每 10 秒"扣血间隔 ms，与模板文案吻合——推断；其余未考证） | 同上 |
| 一句话效果 | 施放后进入双刀流形态：持续耗 HP 换取命中率/硬直提升，普攻变为二刀流攻击并高机率出血；提升[嗜魂之手]/[怒气爆发]/[十字斩]攻击力并减其 CD；杀死出血敌人回复 HP | 同上 [explain] |

**level property 模板解码（L21 向量法，15/16 列全明，col2 无模板引用）**（Lv1 → Lv83 表末）：

| # | 模板项 | 向量 | Lv1 | Lv83 | 备注 |
|---|---|---|---|---|---|
| 1 | 施放时消减的HP | -1,0,1.0 | 220 | 248 | 直减 HP |
| 2 | 每10秒消减的HP | -1,1,1.0 | 75 | 228 | static[1]=10000ms 间隔 |
| 3 | 增加命中率 | -1,3,0.1 | 0.2% | 9.4% | |
| 4 | 增加硬直 | -1,4,1.0 | 10 | 260 | |
| 5 | 出血机率 | -1,5,0.1 | 6.0% | 17.4% | 60→174（×0.1） |
| 6 | 出血Lv | -1,6,1.0 | 36 | 174 | |
| 7 | 出血持续时间 | -1,7,0.001 | 3s | 3s | 恒定 |
| 8 | 出血攻击力 | **-4,8,1.0** | 60 | 219 | **源 -4 第 2 例实证**（R3-A11 首见，语义仍未考证） |
| 9 | [嗜魂之手]增加攻击力 | -1,9,1.0 | 1% | 39% | |
| 10 | [嗜魂之手]减少冷却时间 | -1,10,0.001 | 0.1s | 3.966s | |
| 11 | [怒气爆发]增加攻击力 | -1,11,1.0 | 1% | 39% | |
| 12 | [怒气爆发]减少冷却时间 | -1,12,0.001 | 0.15s | 5.859s | |
| 13 | [十字斩]增加攻击力 | -1,13,1.0 | 1% | 39% | |
| 14 | [十字斩]减少冷却时间 | -1,14,0.001 | 0.05s | 1.953s | |
| 15 | HP最大恢复值 | -1,15,1.0 | 22 | 2633 | 杀出血敌回血上限 |

col2（70→94）无模板行，语义未考证。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能引擎内置（F3）**：load_state 无注册（grep 零命中）；`sqr\character\swordman\` 全树 grep `frenzy` 仅命中
`appendage\ap_swordman_comminterrupt.nut` 的 `STATE_FFRENZY`（另一技能"血魔弑天"系的全局变量，非本技能）与
`swordman_header.nut` 常量；`CUSTOM_ANI_FRENZY1-4`（=57-60，etc 槽）**全 pvf 白名单内无任何消费者**——
普攻切换双刀动画的判定在客户端引擎内（狂战 growtype + Frenzy 开启状态 → 普攻改播 Frenzy1-4）。

**同型参照脚本（F3③，剑魔 ats 版 150）**：`atswordman_load_state.nut:128`：
`IRDSQRCharacter.pushState(10, "character/atswordman/2_demonslayer/atfrenzy/atfrenzy.nut", "Atfrenzy", 150, 150)`——
魔人版（无双刀普攻改造，仅属性增益+光环），结构见 §2.2。

### 2.2 参照实现逐回调（atfrenzy.nut 158 行实读，mod 混淆可读）

```
checkExecutableSkill_Atfrenzy：sq_IsUseSkill(150) → 切状态 150 子状态 0
onSetState 子 0（施放）：播 FRENZY_CAST 音效；读条速度折算；播施法动画（ani 413 = BodyCalldaimus 变体）
onSetState 子 1（读条完，动画 412）：
  挂 ap_atfrenzy.nut appendage，sq_SetValidTime(col2 = 持续时间) + setBuffIconImage(146)
onEndCurrentAni：子 0 → 子 1；子 1 → 回待机 state 0
ap_atfrenzy onStart（buff 生效瞬间）：
  sq_AddChangeStatus("Atfrenzy", type 10, col0) + (type 11, col0)     // 两类属性增益（键值语义未考证）
  sq_AddEffectBack(loop_loop.ani) + sq_AddEffectFront(loop_up.ani)    // 持续光环前后层（挂 buff 期间常驻）
  sq_EffectLayerAppendage(RGB(255,0,0), 75)                            // 角色红色调染色
ap_atfrenzy onVaildTimeEnd/onEnd：删除前后光环层
```

——即 **DNF buff 类标准形态：状态机两段（读条/生效）+ appendage（属性写入 + 持续光环视觉 + 到期回收）**。
狂战士原版在此基础上再加：普攻动画替换（Frenzy1-4）、普攻出血注入、HP 持续消减、击杀出血敌回血、三个技能的增伤减 CD——全部引擎内置。

### 2.3 双刀流普攻动画与命中（Frenzy1-4，本技能对 .chr 的唯一增量）

| 件 | 值 | 说明 |
|---|---|---|
| .chr etc 槽 57-60 | `Animation/Frenzy1-4.ani`（1030-1033 行） | CUSTOM_ANI_FRENZY1-4（header:227-230 实证） |
| .chr etc attack info | `AttackInfo/Frenzy1-4.atk`（1343-1346 行） | 双刀四段命中参数 |

**四段连段结构（实测帧表）**：

| 动画 | 帧数 | 总时长 | sm_body 帧 | SET FLAG | 攻击盒 | 受击盒 |
|---|---|---|---|---|---|---|
| Frenzy1.ani | 6 | 700ms（60×5+400） | 188-193 | 无 | 无（引擎武器判定） | 6/6 |
| Frenzy2.ani | 5 | 640ms（60×4+400） | 194-198 | 无 | 无 | 5/5 |
| Frenzy3.ani | 5 | 640ms（60×4+400） | 199-203 | 无 | 无 | 5/5 |
| Frenzy4.ani | 6 | 600ms（60×5+300） | 204-209 | 无 | 无 | 10/10（双盒） |

装备图层：frenzy1-4.ani **×304**（avatar 全部件 find 计数实测）——四段动作全套换装图层，证明其"普攻级"地位。

**Frenzy1-4.atk 命中参数（实测）**：

| .atk | damage bonus | 反应 | push | lift | 方向 |
|---|---|---|---|---|---|
| frenzy1.atk | -19 | damage | 30 | 30 | hit down |
| frenzy2.atk | -6 | damage | 30 | 30 | hit down |
| frenzy3.atk | +8 | **down** | 40 | **100** | **hit lift up（第 3 段浮空）** |
| frenzy4.atk | +39 | **down** | **100** | **100** | hit down |

即双刀四段=轻推轻推→浮空→击倒，是狂战普攻连段手感的本体。物理/武器伤害适用（[weapon damage apply] 1）。

**效果层**：`effect\animation\frenzy\` 实测：`buff.ani`（8f 800ms，blood-spirits.img——buff 期间光环）、
`cast.ani`（6f 480ms，blood-start.img——施放）、`ball.ani`+`balllayer/tail1-4`（血球系）、
`sword1-1~4-4`（双刀挥砍弧光 ×16）、`xuesetulu_*` ×17（**血色屠戮系——疑他技能共用目录，不属本技能**）。

### 2.4 施放/形态时序（引擎内置重建，推断）

```
施放（↓↑+Space，CD 10s，[auto cooltime apply] 0=CD 起点延迟到结束）：
  扣 MP + 扣 HP col0（施放消减）
  播 buff motion（.chr [buff motion]=Summon2.ani，600ms——buff 类标准姿态，推断；无专属施法动画实测）
  挂"血之狂暴"形态（引擎态，持续到关闭/死亡——skl 无时长列，DNF 实际为开关型形态）
形态期间（引擎内置）：
  每 10000ms（static[1]，推断）扣 HP col1
  普攻（状态 8）改用 Frenzy1-4 动画 + Frenzy1-4.atk + 出血注入（机率 col5/Lv col6/时长 col7/攻击力 col8）
  命中率 +col3、硬直 +col4（面板属性）
  嗜魂之手(31=GrabBlastBlood)/怒气爆发(23)/十字斩(64) 攻击力 +col9/11/13、CD -col10/12/14（跨技能联动）
  击杀出血状态敌人 → 回 HP（上限 col15）
  再施放 → 关闭形态（DNF 开关型技能惯例，引擎内置——skl 无二段数据，未考证）
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Frenzy.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Frenzy.skl` | ✅ 实测（278 行） | 16 列等级数据（15 列全明） |
| 注册行 | —（无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | F2 三招全查 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（全树 grep） | ⛔ 缺失 | 引擎内置 |
| 参照 nut | atfrenzy.nut + ap_atfrenzy.nut | `…\pvf\sqr\character\atswordman\2_demonslayer\atfrenzy\` | ✅ 实测（158+69 行） | ats 版完整实现（C2 定点读） |
| .chr 条目 | Frenzy1-4.ani（etc 57-60）+ Frenzy1-4.atk | `…\pvf\character\swordman\swordman.chr` 1030-1033/1343-1346 行 | ✅ 实测 | 双刀四段动画+命中 |
| 角色 .ani | Frenzy1-4.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.3 帧表 |
| 角色 .atk | Frenzy1-4.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | §2.3 参数 |
| 施法姿态 | Summon2.ani（[buff motion] 共用） | `…\pvf\character\swordman\animation\`（025-Khazan 已验） | ✅ 共用 | 施放姿态（推断） |
| 特效 ani | buff/cast/ball 等 | `…\pvf\character\swordman\effect\animation\frenzy\` | ✅ 实测（56 文件） | §2.3 |
| .als | —（两侧 ls 无） | animation 目录 | ⛔ 无边车 | — |
| 装备层 | Frenzy1-4.ani ×304 | `…\pvf\equipment\character\swordman\avatar\` | ✅ 实测（find 计数） | 换装图层 |
| 增益 appendage | （引擎内置，pvf 无实体） | `…\pvf\appendage\`（无确切子路径不检索） | 未考证 | 狂战版 buff 载体 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 188-209 双刀四段 + 施法姿态） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画 | 必需 | ✅ 已在库 |
| blood-spirits.img | sprite_character_swordman_effect_frenzy.NPK | buff 期间光环（buff.ani 8f） | 必需（视觉还原） | ❌ 未入库 |
| blood-start.img | 同上 | 施放特效（cast.ani 6f） | 必需（视觉还原） | ❌ 未入库 |
| blood-stone-0.img | 同上 | 血球（ball.ani） | 可选 | ❌ 未入库 |
| （sword1-1~4-4 弧光、xuesetulu 系对应 img） | 同上/血色屠戮系 NPK | 双刀弧光/他技能 | 可选 | ❌ 未入库 |

缺失 img：必需 2 张、可选多张（同 NPK 一次提取）。⚠ 双刀四段本体（sm_body 帧 188-209）零缺口——简化版只缺光环两张。

## 5. 实现方案草案

**⛔ 暂缓（完整形态）**——四重缺口叠加，与 Khazan（025）同判 ⛔，但**双刀四段连段部分零缺口**：

| DNF 机制 | 我们的现状（代码实测） | 阻断点 |
|---|---|---|
| 普攻改双刀 Frenzy1-4 | NormalAttackSkill 无"按 buff 切动画"能力；`LSBuffComponent` 无对外查询门面 | **自身 Buff 查询门面**（R4-A18 已记，本技能第 2 消费方）+ 普攻派生改造（技能取消体系姊妹项） |
| 命中率/硬直增益 | NumericType 无对应键（实测 NumericType.cs 仅 Speed/Hp/MaxHp/Attack/Defense/ForbidMove/ForbidSkill）；伤害端零消费 | **属性数值无伤害消费链**（R1-A4 最重缺口，第 4 实证） |
| 三技能增伤/减 CD | MeleeHit 只读固定 HitReaction.Damage（MeleeHitAction.cs:13-14 实测）；无 CD 修改 API | 属性消费链 + **跨技能 CD 重置/修改门面**（R3-A14 已记，"重置"与"减少"略有差异） |
| 杀出血敌回 HP | 无击杀事件钩子、无目标状态查询 | **目标状态查询门面**（R2-A8 记档）+ 击杀事件（新面） |
| 每 10s 扣 HP | ✅ 可表达（BuffDefinition TickTimeMs=10000 + TickActions 自伤） | 无 |
| 出血注入普攻 | ✅ HitReaction.ProcBuffId+ProcChance（L6 已落地） | 无（若普攻能改） |

**可先行落地的部分（简化方向）**：
- **双刀四段独立连段技能**（砍掉形态开关，做"一键四连斩"）：`FrenzySkill : SkillLogic`——
  `TotalTimeMs=0` 自控（四段 700/640/640/600ms），`OnCast` `ctx.PlayAnim(AnimId.Frenzy1)` + 段间
  `ctx.PeekBufferedButton()` 消费 → 下一段（NormalAttackSkill 连段 + L19 段间 ClearHitTargets 惯例）；
  每段 `SetAttackHitbox` + 四段独立 HitReaction（§2.3 atk 原值：50/55/70/110 伤害，段 3 LaunchY 100 浮空、段 4 push100 击倒）+
  `ProcBuffId=BuffIds.Bleed`（机率 col5=17% 建议固定）；施放扣 HP `ctx.ConsumeCasterHp(220→固定 50)`（BloodBoom 先例）。
- **形态壳（若做开关版）**：`FrenzyBuff : BuffDefinition`（TotalTimeMs=0 永久、TickTimeMs=10000、
  TickActions={FrenzyHpDrain(新 Action≈BleedDamageTick 同构 10 行)}）+ `AddBuffToSelf`——buff 可挂可 tick，
  但普攻/伤害端读不到它（缺口如上），demo 意义=光环视觉 + HP 消耗演示。
- 注册点：`SkillIds.Frenzy = 28`；`AnimId.Frenzy1-4 = 132-135`、`FrenzyBuff = 136`（blood-spirits 光环）、
  `FrenzyCast = 137`；`BuffIds.FrenzyForm = 17` 预留；`ActionIds.FrenzyHpDrain = 14` 预留。
- 视觉挂接：光环"buff 期间常驻前后层"撞 **Buff 视觉挂接**缺口（R1-A5 已记；OverDrive/Keiga 同撞，见各文档）——
  简化用 `.als` overlay 单次播放或忽略常驻层。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000ms | 10000 直用（简化连段版可 3000） |
| 施放扣 HP | 220→248（col0） | 固定 50（ConsumeCasterHp） |
| 每 10s 扣 HP | 75→228（col1） | 10/跳（Tick） |
| 四段时长 | 700/640/640/600ms | 直用 |
| 四段命中 | atk：-19/-6/+8/+39；push 30/30/40/100；lift 30/30/100/100 | Damage 50/55/70/110；Kb/Ly 原值直用（段 3 浮空、段 4 击倒） |
| 出血 | 机率 6%→17.4%、3s、攻 60→219 | ProcChance 17% + BleedBuff 预设（3s/15 每秒） |
| 增伤/减CD/回血 | col9-15 | ⛔ 不实现（消费链） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Frenzy.skl` | `.skl` 无子命令（16 列 × 83 行大表） | 本技能手抄 8 组关键值；`skl` 子命令（**大表技能是 skl 子命令最大受益者**） |
| `Frenzy1-4.atk` ×4 | `.atk` 无子命令 | 手抄 4×6 值可接受；随批量化提级 |
| buff/cast/ball.ani | `[SHADOW]`（规则表外，实测 6 处分布） | 整节跳过无碍（已记档） |
| （引擎侧 applyChangeStatus 属性键 10/11/15/50） | 运行时属性写入（非翻译问题） | 归属性消费链立项（§5） |

结论：ani 资源全部可被现有 ani 子命令翻译；实质缺口 `.skl` + `.atk` 子命令（2 条，均重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 普攻→双刀四段改造（形态核心） | **缺失档：自身 Buff 查询门面 + 普攻派生** | 双刀四段做独立连段技能（§5）；形态开关等门面 |
| 命中率/硬直增益 | **缺失档：属性数值消费链**（第 4 实证） | 跳过（面板属性无消费） |
| 三技能增伤+减 CD | 属性消费链 + 跨技能 CD 门面（R3-A14 记档） | 跳过 |
| 杀出血敌回 HP | 目标状态查询 + 击杀事件（缺失档） | 跳过 |
| 出血注入（机率/Lv/时长/攻击力） | ✅ ProcBuffId/ProcChance 落地；出���攻击力随等级缩放延后 | BleedBuff 预设值 |
| buff 常驻光环（前后层） | **缺失档：Buff 视觉挂接**（R1-A5；本批 Keiga/OverDrive 同撞） | .als overlay 单播或跳过 |
| 形态开关（再按关闭） | 技能二段交互门面（R4-A16 已记） | 开关版延后；连段版不需要 |
| 每 10s 扣 HP | ✅ Buff Tick 可表达 | 直译 |
| 无色小晶体消耗 | 道具系统（缺失档） | 跳过 |
| 施法姿态/光环 RGBA 红染 | RGBA 染色已支持（帧级）；常驻红调属 buff 视觉挂接 | 帧级可用 |

## 8. 存疑与缺口上报

**未考证项**
1. `static data 350 10000 110 100` 各值语义（仅 static[1]=10000 与"每 10 秒"文案吻合）。
2. 狂战版增益 appendage 实体（pvf\appendage 大树无路径）；col2（70→94）语义。
3. **源 -4**（col8 出血攻击力）第 2 例（R3-A11 首见 79 列）——两次独立出现，建议 L21 补记"-4=出血攻击力类列（2 例）"。
4. 施放姿态是否为 Summon2（buff motion 共用，推断）。
5. 普攻出血的注入点（引擎在普攻结算时读 Frenzy 状态——机制级推断，无脚本佐证）。

**新系统级缺口（§6.3 清单外/消费方增补）**
1. **普攻派生改造**（形态技能的普攻动画/命中替换）——与"自身 Buff 查询门面"（R4-A18）合并评估：门面落地后普攻类技能读 buff 分支即可。本技能是该缺口**最重的消费方**（狂战形态本体）。
2. **击杀事件钩子**（"杀死出血敌人回 HP"类）——目标状态查询门面（R2-A8）+ 击杀瞬间事件，建议合并立项。
3. Buff 视觉挂接 +2 消费方（OverDrive/Keiga，本批内互证，见 038/084 文档）。
4. 跨技能 CD"减少"（非重置）语义——R3-A14 记档为"重置"，本技能需要"减 X 秒"，建议门面设计时两者一并。

**翻译工具缺口**：`.skl` 子命令（16 列大表最具收益案例）+ `.atk` 子命令（4 文件）；`[SHADOW]` 记档（重复）。

**给下轮的经验**：狂战形态系（血之狂暴/血气唤醒等）的"双刀/出血"视觉与数据全在 `Frenzy1-4.atk/ani` + `effect\animation\frenzy\`，与普攻改造的联动全在引擎——脚本侧只有 ats 参照（atfrenzy）。xuesetulu_* 特效属血色屠戮系勿混入。
