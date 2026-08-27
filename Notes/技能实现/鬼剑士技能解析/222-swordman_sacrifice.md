# 血气分流（swordman_sacrifice）

> 技能ID 222 | 级别 B（队友支援/HP 转移） | 可实现性 ⛔（队友目标选择 + 力量/攻速/移速/双防数值消费链双缺失；HP 转移的数值操作本身可行但"给队友"语义无承载——自身退化版=纯力量 buff 空转） | 分析日期 2026-08-22 | 批次 B3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 血气分流 | `skill\swordman\swordman_sacrifice.skl` [name] |
| 英文名 | swordman_sacrifice（取 skl 文件名；[name2]="The Sacrifice"） | 同上 [name2] 实测 |
| 职业 | 狂战士（[skill fitness growtype]=**3**；二觉系 swordman_ 前缀，F7 结构） | 同上 |
| 学习等级 | 30 | 同上 [required level] |
| 最高等级 | 50（狂战档 20：`0 0 0 20 0 0`） | 同上 [maximum level]/[growtype maximum level] |
| 类型 | active（skill class 2） | 同上 [type]/[skill class] |
| 指令 | ↑↓→ + Space（BUFF 键） | 同上 [command] |
| CD | 40000 ms（pvp 60000） | 同上 [cool time] |
| MP | **0**（不耗蓝，代价是 HP） | 同上 [consume MP] `0 0` |
| 读条 | casting time 250 ms | 同上 [casting time] |
| 可施放状态 | [executable states] `8 0 14`（普攻态 8 等可中施放） | 同上 [executable states] |
| 特殊消耗 | **HP：施放扣除自身最大 HP 的 5%**（static[0]=5，checkExecutableSkill 实证） | skl [static data] + sacrifice.nut |
| static data | `5 700 500`（static[0]=5 → HP 门槛/代价 5%；static[1]=700 → 队友搜索范围 700px（推断）；static[2]=500 未考证） | 同上 + sacrifice.nut |
| 一句话效果 | 把自身 5% 最大 HP 按比率转给范围内一名队员并增益其力量；自身施放时不回血但力量增益 ×1.1；习得血气旺盛(63)时附攻速/移速增益与双防减益 | 同上 [explain] |

**level property 模板解码（7 列，L21 向量法全解，Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| HP 转化比率 | (-1, 0, ×1.0) | col0 = **80% → 510%**（转出量 = 5%maxHp × 此比率） |
| 增加力量 | (-1, 1, ×1.0) | col1 = **20 → 510** |
| 持续时间 | (-1, 2, ×0.001) | col2 = 600000 → **600 s 恒定** |
| 增加攻击速度 | (-1, 3, ×0.1) | col3 = 10→500 → **1% → 50%** |
| 增加移动速度 | (-1, 4, ×0.1) | col4 = 10→500 → **1% → 50%** |
| 减少物理防御 | (-1, 5, ×1.0) | col5 = **15%（恒定）** |
| 减少魔法防御 | (-1, 6, ×1.0) | col6 = **15%（恒定）** |

攻速/移速/双防四项仅在习得血气旺盛（技能 63）后生效（nut 实证，§2.2）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**完整 nut 链（F7 变体：有专属 pushState 但实走 STATE_THROW）**：

```
121: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/sacrifice/sacrifice.nut", "swordman_sacrifice", 222, SKILL_SWORDMAN_SACRIFICE);
     （swordman_header.nut:88  SKILL_SWORDMAN_SACRIFICE <- 222 实证；状态号=技能号同值——R3 后新技能惯例）
```

| 文件 | 完整路径 | 行数 | 角色 |
|---|---|---|---|
| sacrifice.nut | `…\sqr\character\swordman\sacrifice\sacrifice.nut` | 101 | 门禁 + 增益结算（**无 onSetState——姿态走 THROW 态**） |
| ap_sacrifice.nut | `…\sqr\character\swordman\appendage\ap_sacrifice.nut` | 65 | buff 容器 + drawAppend 红光染色 |
| swordman_throw.nut case SACRIFICE | `…\sqr\character\swordman\swordman_throw.nut` 42-47 行 | — | 投掷态落点：播特效 + 调 SetMyAppendage |

### 2.2 主 nut 逐函数

**checkExecutableSkill_swordman_sacrifice（施法门禁）**：

```
hpMax5 = maxHp × 0.05                          // static[0]=5
if (hp < hpMax5) → 拒绝 + 警告消息 3599        // 最低 5% HP 门槛（MinCastHpPct 同构！）
isUsed = sq_IsUseSkill(222)：
  立即扣血 newHp = hp - hpMax5（下限 1，不致死）
  组包进 STATE_THROW：substate 0 / type 0 / skill 222 / castTime×2 / … / 1000/2000 参数
  → AddSetStatePacket(STATE_THROW, USER, true)
```

**checkCommandEnable**：STAND 态直接允许；ATTACK 态（普攻中）走通用命令判定——与 [executable states] 8 0 14 互证（普攻/站立/投掷态可施放）。

**swordman_throw.nut onAfterSetState case SACRIFICE（投掷态推进）**：

```
throwState == 1（投出相位）：
  sq_AddDrawOnlyAniFromParent(obj, "character/swordman/effect/animation/sacrifice1.ani", 0, 1, 0)
  SetMyAppendage_swordman_sacrifice(obj, datas)
```

**SetMyAppendage_swordman_sacrifice（增益结算核心）**：

```
目标 = sq_GetObjectByObjectId(obj, datas[1])     // THROW 态瞄准系统选中的单位（700px 内队友，static[1] 推断）
读列：time=col2(600s) / phyAtk=col1(力量) / perHp=col0(转化率) / atkSpeed=col3 / moveSpeed=col4
      phyDef=col5/(-1) / magDef=col6/(-1)（防御为减益，取负）
无目标 → 目标=自身（自施放形态）
newHp = 目标HP + 施法者maxHp × 5% × perHp/100    // HP 转移结算
自施放特例：newHp 不变（不回血），phyAtk ×1.1（力量增益加成）
挂 ap_sacrifice.nut（valid time = 600s，cause=技能222）：
  changeStatus 参数组（引擎属性系统）：
    PHYSICAL_ATTACK +phyAtk                      // 力量
    若血气旺盛(63) 习得：
      +ATTACK_SPEED atkSpeed / +MOVE_SPEED moveSpeed
      -EQUIPMENT_PHYSICAL_DEFENSE phyDef% / -EQUIPMENT_MAGICAL_DEFENSE magDef%
目标 HP 写入（治疗落地）
```

**ap_sacrifice.nut（buff 侧）**：proc 空；**drawAppend** = 全帧把宿主当前动画染红：`GRAPHICEFFECT_LINEARDODGE + RGB(155,0,0) + alpha 30→160（240ms 渐入）`，且递归染色所有动画图层——**"Buff 视觉挂接"缺口（R1-A5）的 drawAppend 类第 3+ 实证**（受血者浑身泛红的效果表现）。

### 2.3 被动对象

无攻击判定体（纯支援技）。视觉特效 sacrifice1/2 + charge1/2 由 throw 态/nut 直接播放（DrawOnlyAni）。

### 2.4 动画关键帧表（全部实测）

| 动画 | 帧数 | 总时长 | 引用 img | 备注 |
|---|---|---|---|---|
| `character\swordman\animation\staysacrifice.ani` | 16 | 1550 ms | sm_body%04d（模板） | 施法者动作（.chr [etc motion] 槽 114 = CUSTOM_ANI_STAYSACRIFICE，header.nut:284 实证） |
| `effect\animation\sacrifice1.ani` | 13 | 未逐帧加总 | `Character/Swordman/Effect/GiveBlood1.img` | 放血主特效（throw 态触发） |
| `effect\animation\sacrifice2.ani` | 12 | 600 ms | GiveBlood2.img | 背层（sacrifice1.ani.als 挂层 -1 双层叠加） |
| `effect\animation\sacrificecharge1.ani` | 6 | 300 ms | GiveBlood1.img | 充能层（sacrificecharge1.ani.als 同构双层） |
| `effect\animation\sacrificecharge2.ani` | 6 | 300 ms | GiveBlood2.img | 充能背层 |

`.als` 边车 ×2（sacrifice1/sacrificecharge1）——标准 `[use animation]` + `[add] 帧号 层号 别名` 结构，现工具直译无缺口。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_sacrifice.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\swordman\swordman_sacrifice.skl` | ✅ 实测 | 数值（7 列全解） |
| 注册行 | load_state:121 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 222 / 技能 222 |
| 主 nut | sacrifice.nut | `…\sqr\character\swordman\sacrifice\sacrifice.nut` | ✅ 实测（101 行） | 门禁 + 结算 |
| THROW 态 | swordman_throw.nut case 222 | `…\sqr\character\swordman\swordman_throw.nut` | ✅ 实测 | 姿态/特效/落点 |
| appendage | ap_sacrifice.nut | `…\sqr\character\swordman\appendage\ap_sacrifice.nut` | ✅ 实测（65 行） | buff + drawAppend 红光 |
| 常量 | swordman_header.nut:88/284 | `…\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | SKILL 222 / CUSTOM_ANI_STAYSACRIFICE 114 |
| .chr 条目 | [etc motion] 槽 114 | `…\pvf\character\swordman\swordman.chr` 1087 行 | ✅ 实测 | StaySacrifice.ani |
| 角色 .ani | staysacrifice.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 特效 .ani/.als | sacrifice1/2 + charge1/2 + 2 个 .als | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | §2.4 |
| 角色 .atk | — | `…\character\swordman\attackinfo\` | ⛔ 无（无攻击） | — |
| 装备层 | staysacrifice.ani ×76 | `…\equipment\character\swordman\avatar\`（find 计数 76） | ✅ 实测 | 只查存在性 |
| 目标选择 | 引擎 THROW 瞄准系统 | — | 未考证 | 700px 队友锁定的引擎侧细节 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作帧 | 必需 | ✅ 已在库 |
| `Character/Swordman/Effect/GiveBlood1.img` | sprite_character_swordman_effect.NPK（Effect 根推导） | 放血主/充能特效 | 必需（视觉还原） | ❌ |
| `Character/Swordman/Effect/GiveBlood2.img` | 同上 | 放血背层 | 必需（双层视觉） | ❌ |

缺失 img：必需 2 张、同一 NPK 一次提取。

## 5. 实现方案草案

**⛔ 暂缓（支援语义无承载）**——判定依据：

1. **队伍/阵营判定缺失**（R1-A3 首撞）：目标 = "范围内**一名队员**"——需要 ①队友识别（GetEnemies 语义=除自己全部，单人 demo 无队友）②目标选择/瞄准门面（THROW 态的 objectId 锁定在我们侧无对应 API，无"最近队友"之类查询）。单人 demo 退化为自施放形态，但自施放 = **不回血 + 纯力量 buff（消费链空转）**——技能两手空空。
2. **属性数值无消费链**（R1-A4）：力量 +20~510、攻速/移速 +1~50%、双防 -15% 四路全卡（同暴走 §5——本技能是暴走的"给队友版"）。
3. **HP 转移数值操作本身 ✅**：`ctx.ConsumeCasterHp(maxHp×5%)` + `ctx.AddNumeric(target, NumericType.Hp, +转移量)` 两行门面现成——**缺的是"target 是谁"**，不是数值能力。血气旺盛(63) 联动（Buff 查询门面）另撞 R4-A18。

**若未来数值链 + 阵营立项后的落地草案**：

- `SacrificeSkill : SkillLogic`——CD 40000、TotalTimeMs=1550（staysacrifice 动画直用）、`MinCastHpPct = 5`（DNF 门禁同构，现成字段）；OnCast：PlayAnim + ConsumeCasterHp(maxHp×5%)；OnUpdate castTime 250ms 后 `ctx.AddNumeric(目标, Hp, maxHp×5%×col0/100)` + `ctx.AddBuff(目标, BuffIds.SacrificeBuff)`。
- `SacrificeBuff : BuffDefinition`——TotalTimeMs=600000；AddActions 挂力量/攻速/移速数值（等键位与消费端）。
- 注册点：`SkillIds.Sacrifice = 32`、`BuffIds.SacrificeBuff = 16`、`AnimIds 156-160`（StaySacrifice + sacrifice1/2 + charge1/2）。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000 ms | 40000 直用 |
| HP 门槛/代价 | maxHp 5%（static[0]） | MinCastHpPct 5 + ConsumeCasterHp 5% |
| 转化率 | 80%→510%（col0） | 等阵营/数值链 |
| 力量增益 | 20→510（自施放 ×1.1） | 等数值链 |
| buff 时长 | 600 s | 600 s |
| 搜索范围 | 700 px（static[1] 推断） | 等目标选择门面 |
| 施法动作 | staysacrifice 1550ms + 读条 250ms | TotalTimeMs 1550（读条跳过） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `swordman_sacrifice.skl` | `.skl` 无子命令（7 列 + [executable states] 节） | 手抄可行；`skl` 子命令同前议 |
| staysacrifice / sacrifice1/2 / charge1/2.ani | 常规节 | `ani` 直译无缺口 |
| sacrifice1.ani.als / sacrificecharge1.ani.als | 标准结构 | `als` 直译无缺口 |
| ap_sacrifice.nut 的 drawAppend | 运行时染色（非翻译问题） | 归"Buff 视觉挂接"缺口（R1-A5），非工具缺口 |

结论：**.ani/.als 全部可被现有子命令翻译**；实质缺口 `.skl`（重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 给队友回血（核心收益） | **缺失档：队伍/阵营 + 目标选择门面**（R1-A3 + 无瞄准 API） | ⛔ 主因；单人无队友语义 |
| 力量/攻速/移速/双防增益 | **缺失档：属性数值消费链**（R1-A4，与暴走同族） | ⛔；等数值链 |
| HP 转移结算 | **无缺口**（ConsumeCasterHp + AddNumeric(Hp) 现成） | 数值侧已可写 |
| 5% HP 门槛 | 无缺口（MinCastHpPct 现成，bloodboom 同构） | 直译 |
| 血气旺盛(63) 条件增益 | 缺失档：Buff 查询门面（R4-A18）+ 被动技能系统 | demo 固定全给（等于满档被动） |
| 自施放 ×1.1 特例 | 无缺口（分支一行） | 直译 |
| drawAppend 红光视觉 | 缺失档：Buff 视觉挂接（R1-A5） | 跳过 or 特效 ani 近似 |
| 普攻态施放（executable states 8） | 技能取消体系（064 上报） | demo 站立施放 |
| 读条 250ms | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static[1]=700（队友搜索范围推断）/ static[2]=500 语义。
2. THROW 态目标锁定的引擎细节（objectId 选择规则——最近？准星？Tab？）。
3. staysacrifice.ani（1550ms）与 THROW 态默认姿态的关系（THROW 用通用 throw motion，staysacrifice 疑为蓄力/维持相位专用——哪个相位播未考证）。
4. charge1/2 的播放时机（读条充能推断——sq_EndDrawCastGauge 同族，nut 未直接引用）。

**缺口上报（并入主循环汇总）**
1. **目标选择/瞄准门面**（新表述，属 R1-A3 阵营缺口的展开）：支援技需要"范围内友方单位枚举 + 选定一个"的 API（GetEnemies 只有敌方语义且单人退化）——与 Khazan（队友 buff 光环）、sacrifice（单体队友）同族，建议"阵营/目标选择"一项统一立项，本技能是单体形态样本。
2. **Buff 视觉挂接第 3+ 实证**（R1-A5 首记）：ap_sacrifice 的 drawAppend 全帧染红是"按 buff 状态持续染色"形态（与光环类 drawAppend 并列的第二形态），buff 视觉通道设计需同时覆盖"挂光环"与"染色宿主"两种。

**翻译工具缺口**：`.skl` 子命令（重复印证）；[executable states] 节第 2 例（skl 设计输入）。

**给下轮的经验**：**swordman_ 前缀二觉系（F7）里也有"非演出型"分支**——222 是门禁+结算型（无 onSetState、姿态借 THROW 态），与 244/246 的"薄壳+共享 PO"形态不同。判定捷径：pushState 后先看 nut 有没有 onSetState——没有就是门禁/结算型（checkExecutableSkill_* 函数组），F7 的"写包 dword 对照 level property"解码法在结算函数里照样适用（本技能 7 列零未考证全解）。
