# 鬼神诏令（hero）

> 技能ID 132 | 级别 B | 可实现性 🔶（深简化：降灵循环/选鬼/演出/挂 buff 可落地；四鬼神降灵的战斗效果全部卡在数值消费链与输出管线钩子上） | 分析日期 2026-08-22 | 批次 B6

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼神诏令 | `skill\Swordman\ghost\hero.skl [name]` |
| 英文名 | hero（取 skl 文件名；[name2] 节无） | 同上 |
| 职业 | 鬼泣（[skill fitness second growtype]=2；[second growtype maximum level] 第 6 位=30 → growtype 2 二觉档，087/247 同判据；卡赞/普戾蒙/萨亚主题互证） | 同上 |
| 学习等级 | 75（二觉技） | 同上 [required level] |
| 最高等级 | 30 | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | active（skill class 1，魔法系声明+物理 weapon effect 混写） | 同上 [type] / [weapon effect type] |
| 指令 | ←←→ + Z（[skill command advantage] 节无值） | 同上 [command] |
| CD | **0 ms**（循环施放型——降灵切换器） | 同上 [dungeon][cool time] |
| MP | 146 → 280（Lv1→Lv30） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 可执行状态 | `[executable states] 0 8 14` | 同上 [executable states] |
| static data | `120000`（降灵持续时间 ms = **120 秒**，代码 `sq_SetValidTime` 直读） | 同上 [dungeon][static data] |
| 一句话效果 | 消耗当前在场的鬼神召唤阵，令该鬼神降灵附体 120 秒（卡赞=力智/物魔攻、萨亚=水强+全攻击附带冰冻、普戾蒙=命中降防、罗刹=全攻击附带出血+中毒）；无鬼神在场时默认降灵卡赞 | 同上 [explain]/[basic explain] + nut 实证 |

**level property（14 列，Lv1 → Lv30，向量全 `-1 <列>` = 纯 level 列；模板 14 行对位实测）**：

| 列 | Lv1→Lv30 | 语义 |
|---|---|---|
| col0 | 110→600 | 卡赞：力量智力增加值（代码实挂 **PHYSICAL_ATTACK + MAGICAL_ATTACK** 两条 ChangeStatus） |
| col1 | 6600→36000 | 普戾蒙：防御力减少值（挂接走引擎 PO 12275，见 §2.3） |
| col2 | 55→300 | 萨亚：冰属性强化值（代码挂 **ELEMENT_ATTACK_WATER**） |
| col3 / col4 / col5 | 25% 恒 / 82→180 / 5000 恒 | 萨亚：冰冻机率 / 冰冻等级 / 冰冻时长 5s |
| col6 / col7 / col8 / col9 | 25% 恒 / 82→180 / 2000 恒 / 8312→29872 | 罗刹：出血机率 / 等级 / 时长 2s / **出血伤害** |
| col10 / col11 / col12 / col13 | 25% 恒 / 130 恒 / 2000 恒 / 8312→29872 | 罗刹：中毒机率 / 等级 / 时长 2s / **中毒伤害** |

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 123（实测）
IRDSQRCharacter.pushState(0, "character/swordman/hero/hero.nut", "HERO", STATE_HERO, SKILL_HERO);
// swordman_header.nut 行 619/620：STATE_HERO <- 132，SKILL_HERO <- 132
```

主 nut：`sqr\character\swordman\hero\hero.nut`（216 行，**干净原版无混淆**）+ 同目录 6 个 ap_nut。
常量：`CUSTOM_ANI_THROW=182` / `CUSTOM_ANI_THROW2=183`（.chr etc motion 182/183 = `Animation/Throw1.ani`(6 帧 300ms) / `Throw2.ani`(11 帧 600ms)，通用投掷动作，行 1155/1156 实测）。
被动挂载：`passive_skill_swordman.nut:215 drawHeroGhostIcon`——习得即挂 `ap_hero.nut`（buff 图标绘制壳，无战斗逻辑）。

### 2.2 主 nut 逐回调（hero.nut）

- **checkExecutableSkill**：常规 → 进 STATE_HERO（var state=0）。
- **onSetState**：
  - state 0：`sq_SetCurrentAnimation(182)`（Throw1 300ms 投掷起手）；`isDelete=0`；`ghost=0`（注释实证 **0=卡赞 1=萨亚(saya) 2=普戾蒙(bremen) 3=罗刹(rasa)**）。
  - state 1：`ghostJudge(obj)`（见下）+ `sq_SetCurrentAnimation(183)`（Throw2 600ms 收势）。
- **onProc**（仅 state 0 期间，isDelete 一次性守卫）：枚举自己的四类鬼神阵 PO——
  `20011 卡赞阵(Khazan.obj)` / `20012 萨亚阵(Saya.obj)` / `20013 普戾蒙阵(Bremen.obj)` / `20040 罗刹疫病体(EpidemicRasaCreater.obj)`（passiveobject.lst 实测四条）；
  **找到哪个就销毁哪个并记 ghost 槽**——降灵目标 = 当前在场的鬼神召唤（对应 basic explain"施展降灵术时，在最后会让使用的技能降灵"）；
  一个都没有 → ghost 保持 0 = **默认降灵卡赞**（explain 兜底条款实证）。
- **onEndCurrentAni**：state 0 → state 1；state 1 → 回站立。**总演出 900ms，之后 120s buff 自治**。
- **ghostJudge(obj)**（核心，state 1 入口）：
  1. 视觉：`sq_CreatePooledObject` 播对应鬼神降灵动画（见 §2.4）；
  2. 挂 appendage（四选一）+ `setAppendCauseSkill(对应鬼神技能 25/36/41/75)` + buff 图标 154-157；
  3. `appendage.sq_SetValidTime(static[0]=120000)`——**四鬼神降灵统一 120 秒**。

### 2.3 四降灵 appendage 逐个实测（hero 目录 6 ap_nut）

| ap_nut | 挂接 | 战斗效果（实测代码） | 我们侧判定 |
|---|---|---|---|
| `ap_hero_kazan.nut`（卡赞） | 自身 120s | 纯壳——数值在 hero.nut：ChangeStatus `PHYSICAL_ATTACK`/`MAGICAL_ATTACK` += col0（110→600） | **属性数值无伤害消费链**（R1-A4 最重缺口）：AddNumeric 可存、伤害端不读 |
| `ap_hero_saya.nut`（萨亚） | 自身 120s | **proc 每帧把 FREEZE 注入自身 currentAttackInfo**：机率 col3=25%、等级 col4、时长 col5=5s——**存续期间所有攻击带 25% 冰冻**（L6 链路的"自身 buff 版"） | **输出管线钩子缺失**（新缺口，§8）：HitReaction.ProcBuffId 是技能静态配置，无"buff 修改后续所有攻击"的注入口 |
| `ap_hero_hermen.nut`（普戾蒙） | 自身 120s | `onAttackParent` 每次命中 → `sq_SendCreatePassiveObjectPacket(12275,…)`——**引擎内置 PO（passiveobject.lst 无注册，实测）**施放防御减 debuff（ap_hero_debuff_hermen.nut 为其挂到敌人身上的壳） | 防御 stat 无消费链 + PO 12275 行为不可见（引擎内置） |
| `ap_poistion.nut`（罗刹） | 自身 120s | **proc 每帧注入 BLEEDING（25%/col7/2s/伤害 col9）+ POISON（25%/130/2s/伤害 col13）到自身攻击**——"诏令罗刹"= 全攻击附带双异常 | 同萨亚：输出管线钩子缺失；BleedBuff/BurnBuff 同构可复用但无注入口。（另：该 nut 引用未定义 `skill_level` 变量——mod 层小 bug，四 value 读数存疑） |
| `ap_hero.nut` | 习得即挂 | buff 图标绘制壳（drawHeroGhostIcon 配套） | Buff 视觉挂接缺口（R1-A5） |
| `ap_hero_debuff_hermen.nut` | 敌人（PO 12275 代挂） | 纯壳（def-down 数值在引擎侧） | 同上不可见 |

**basic explain 与代码的差异**：explain 称"降临在**组队员**或是自己身上 / 持续时间=鬼神技能剩余时间 / 逆顺序降灵"——本 pvf 代码只对**自己**挂 appendage、时长固定 120s、无逆序队列（旧版文本/未实现特性，存疑记档）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/Throw1.ani`（#182） | 6 | 300ms | 无 | 无 | 通用投掷起手；sm_body |
| `Throw2.ani`（#183） | 11 | 600ms | 无 | 无 | 通用收势；sm_body |
| `animation/ani_als/necromantic/ghost/descentsoul_end_ghostkhazan_b.ani`（卡赞降灵，pooled） | 19 | 1520ms | 无 | 无 | `GhostKhazan1.img`；**含 [IMAGE RATE]/[INTERPOLATION] 节**；.als 一份 |
| `descentsoul_end_ghostsaya_b.ani`（萨亚） | 19 | 1520ms | 无 | 无 | `GhostSaya1.img` |
| `descentsoul_end_ghostbremen_b.ani`（普戾蒙） | 19 | 1520ms | 无 | 无 | `GhostBremen1.img`；.als 一份 |
| `descentsoul_end_rasa_c.ani`（罗刹） | 19 | 1520ms | 无 | 无 | `EpidemicRasa/rasa.img`；.als 叠 7 层（End_Rasa_A-F + End_Eff_A/B，z -4..10004，`[add]` 标准节） |

（同目录另有 descentsoul_start_* 与 phantom 变体系——属鬼魂释放 131 的视觉族，本技能只用 end 四份。）

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | hero.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghost\hero.skl` | ✅（170 行） | 14 列数值/CD0/static 120000 |
| 注册行 | load_state 行 123（132/132） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 132 |
| 主 nut | hero.nut | `…\pvf\sqr\character\swordman\hero\hero.nut` | ✅（216 行） | 降灵循环/选鬼 |
| ap nut ×6 | ap_hero / ap_hero_kazan / ap_hero_saya / ap_hero_hermen / ap_hero_debuff_hermen / ap_poistion.nut | 同上目录 | ✅（逐个读） | §2.3 |
| 鬼神阵 PO ×4 | Khazan.obj / Saya.obj / Bremen.obj / EpidemicRasaCreater.obj | `…\pvf\passiveobject\passiveobject.lst:20011/20012/20013/20040` 注册 | ✅（lst 行实测，obj 未展开——属技能 25/36/41/75 的分析范围） | 选鬼目标 |
| 引擎 PO | 12275 | passiveobject.lst（grep 无注册） | ⛔ **引擎内置未注册** | 普戾蒙 def-down 代挂体 |
| .chr 条目 | etc motion #182/#183（行 1155/1156） | `…\pvf\character\swordman\swordman.chr` | ✅ | Throw1/Throw2 |
| 角色 .ani | Throw1.ani / Throw2.ani | `…\pvf\character\swordman\animation\` | ✅ | 通用投掷 |
| 降灵 .ani | descentsoul_end_ghost{khazan,saya,bremen}_b / rasa_c.ani(+.als×3) | `…\pvf\character\swordman\animation\ani_als\necromantic\ghost\` | ✅ | 四鬼神显灵 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 不存在 | 纯 buff 技（PO 12275 自带） |
| 被动挂载 | passive_skill_swordman.nut（drawHeroGhostIcon:215 / getGhostSoulReleaseExecultableState:189） | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ | 图标壳 + 鬼魂释放联动 |
| 关联技能 | Khazan.skl(25) / Saya.skl(36) / Bremen.skl(41) / EpidemicRasa.skl(75) / ghostsoulrelease.skl(131) | `…\pvf\skill\Swordman\`（lst 行 65/207/95/233/147 实测） | ✅（存在性） | 鬼神阵四技 + 取消体系（各属其他批次） |
| 装备层 | coat 层 Throw 系 ×50 | `…\pvf\equipment\character\swordman\avatar\coat\` | ✅（计数） | 换装图层（通用投掷动作共享） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | Throw1/Throw2 | 必需（共享） | ✅ 已在库 |
| `Character/Swordman/Effect/GhostKhazan1.img` | sprite_character_swordman_effect.NPK（`Effect` 直属，无子目录） | 卡赞显灵 | **必需** | ❌ |
| `Character/Swordman/Effect/GhostSaya1.img` | 同上 | 萨亚显灵 | **必需** | ❌ |
| `Character/Swordman/Effect/GhostBremen1.img` | 同上 | 普戾蒙显灵 | **必需** | ❌ |
| `Character/Swordman/Effect/EpidemicRasa/rasa.img` | sprite_character_swordman_effect_epidemicrasa.NPK | 罗刹显灵（.als 七层同图集系） | **必需** | ❌ |

缺失 img：必需 4 张、可选 0 张（四份降灵动画单 img 单层——**本批三技能中最轻的资源面**）。

## 5. 实现方案草案（深简化"降灵循环器"：演出+选鬼+占位 buff 全通，四降灵战斗效果砍掉）

### 内容件清单

1. **`DotNet~/Skills/HeroSkill.cs : SkillLogic`**
   - `CooldownMs = 0`（**DNF 原值 0 直用**——SkillCastHelper 对 CooldownMs=0 不记 CD，天然支持循环切换，零新机制）；`TotalTimeMs = 900`（Throw1 300 + Throw2 600）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanThrow1)`；SubState=0（当前降灵鬼神槽存静态枚举：无阵在场恒 0=卡赞）。
   - `OnUpdate`：`GetElapsedMs() ≥ 300 && SubState==0` → `ctx.PlayAnim(AnimId.SwordmanThrow2)` + `ctx.AddBuffToSelf(当前鬼神对应的 BuffId)` + 视觉 `ctx.CreateArea(AreaIds.HeroGhostDescend, ctx.GetTargetPosition())`（显灵区：无判定纯视图载体）或直接借 buff 无视图时的单位层播放（实现期二选一）+ SubState=1。
   - 选鬼简化：鬼神阵四技（25/36/41/75）未实现 → 恒走卡赞兜底（DNF 行为子集，非改设）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Buffs/HeroKazanBuff.cs : BuffDefinition`**（占位范式，四选一示例）
   - `TotalTimeMs = 120000`、`TickTimeMs = 0`、无 Tick——面板占位；可加 `AddActions` 里 `ctx.AddOwnerNumeric(NumericType.PhysicalAttack, 110)`（**存而不用**，消费链补齐日即生效——与远古记忆类同处理）。
3. **`HeroSayaBuff / HeroBremenBuff / HeroRasaBuff`**：同构占位（罗刹版可在 AddActions 挂说明性日志）；效果注入等待输出管线钩子（§8）。
4. **无新增 Action/Area**（显灵视觉走 AnimId + 单位层；MeleeHit 不需要）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 132 + Throw1/Throw2 | `HeroSkill` + 两个 AnimId |
| onProc 选鬼（销毁在场阵） | 鬼神阵四技缺位 → 恒卡赞（子集行为） |
| ghostJudge 挂 appendage 120s | `ctx.AddBuffToSelf` + BuffDefinition(120000ms) |
| ap_hero_saya/poistion 的攻击注入 | **输出管线钩子（新缺口）**——v1 砍掉 |
| 卡赞 ChangeStatus | AddNumeric 面板占位（消费链缺口） |
| PO 12275（def-down） | 引擎内置不可见 + 防御无消费 → 砍掉 |
| ap_hero buff 图标 | Buff 视觉挂接缺口（R1-A5）→ 砍掉 |
| CD 0 循环切换 | `CooldownMs = 0` 原生支持 ✅ |

### 注册点清单（B6 批号段）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.Hero = 29` + ButtonToSkill 新键 |
| BuffId | `Runtime\BuffDefinition.cs` | `HeroKazan = 14`、`HeroSaya = 15`、`HeroBremen = 16`、`HeroRasa = 17` |
| AnimId | `AnimConfigRegistry.cs` | SwordmanThrow1=148、SwordmanThrow2=149、HeroGhostKhazan=150、HeroGhostSaya=151、HeroGhostBremen=152、HeroGhostRasa=153 |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×6（含 3 个 .als overlay）；img 必需 4 张 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 0 | 0（直用） |
| 演出 | 300ms + 600ms | TotalTimeMs 900 |
| 降灵持续 | static 120000ms | 120000（直用） |
| 卡赞力智/物魔攻 | col0 110→600 | 面板 +110（存而不用） |
| 萨亚冰冻注入 | 25% / Lv82→180 / 5s | 砍（管线钩子）；管线落地后 = ProcBuffId=Freeze/25 |
| 罗刹双异常注入 | 25% / 2s / 伤害 8312→29872 | 砍；管线落地后 = 出血+中毒双 Proc（需扩展单 HitReaction 多 Proc 槽） |
| 普戾蒙降防 | col1 6600→36000（PO 12275） | 砍 |
| 选鬼 | 四阵 PO 在场判定 | 恒卡赞（阵四技缺位） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| Throw1/Throw2.ani | 节面常规 | 现有 ani 子命令全覆盖 |
| descentsoul 四 ani | **[IMAGE RATE]**（4 文件）、**[INTERPOLATION]**（2 文件：khazan_b/rasa_c）、[SHADOW] | 三节均已在缺口累计记档（IMAGE RATE 延后/INTERPOLATION R3-A13/SHADOW），无新增 |
| descentsoul rasa_c.ani.als | `[add]` 标准节 | als 子命令全覆盖 |
| `.skl` | `.skl` 无子命令（14 列矩阵——**本批最大列数**，批量化收益例证+1） | 手抄可接受；并入 skl 子命令立项 |
| hero.skl 双 [weapon effect type] 节（magical+physical 各一） | 解析容错点（非缺口） | skl 子命令设计时注意同名节覆盖问题 |
| `.obj`（四鬼神阵） | 属技能 25/36/41/75 批次，本档未展开 | — |

结论：.ani/.als 全覆盖；实质缺口 = `.skl` 既有项 + IMAGE RATE/INTERPOLATION 既有记档，无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 萨亚/罗刹"全攻击附带异常"注入 | **输出伤害管线钩子（新缺口，§8）**：受击侧钩子已有记档（R3-A15），出击侧无注入点——HitReaction 静态 per-skill | v1 砍；管线立项时以"自身 buff → 后续所有 MeleeHit 的 HitReaction 叠加 ProcBuffId"为设计输入 |
| 卡赞物魔攻/萨亚水强/普戾蒙降防 | 属性数值无伤害消费链（R1-A4 最重）+ 元素系统缺失 | 面板占位（AddNumeric 存而不用）；降防直接砍 |
| 选鬼依赖四鬼神阵在场 | 阵四技（25/36/41/75）未实现 | 恒卡赞（DNF 兜底行为的子集，非改设） |
| 降灵到组队员 | 队伍/阵营判定（R1-A3）+ 本 pvf 代码本就只挂自己 | 只挂自己 |
| 逆顺序降灵/剩余时间继承（basic explain） | 本 pvf 代码未实现（文本遗留） | 不做 |
| buff 图标 154-157 | Buff 视觉挂接（R1-A5） | 砍 |
| PO 12275 行为 | 引擎内置未注册，不可走读 | 砍（其效果本就无消费链） |
| ap_poistion 的 skill_level 未定义 bug | mod/原版代码缺陷（读数存疑） | 数值以 skl 列直读为准 |

## 8. 存疑与缺口上报

**未考证项**
1. explain"逆顺序降灵/剩余时间继承"与代码（固定 120s、单鬼神）的差异——疑 basic explain 为旧版文本或未实装特性，无其他佐证。
2. ap_poistion.nut 中 `skill_level` 未定义（Squirrel 动态作用域下取 null）——四 value 读数在引擎运行时是否回落 0 未考证；本文按 skl 列直读。
3. hero.skl 出现两个 [weapon effect type] 节（magical 在前 physical 在后）——引擎取后者还是前者未考证。
4. descentsoul_start_* / phantom 变体系与技能 131 的分工（推断 131 用 start 系，未展开）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **输出（出击）伤害管线钩子**——与 R3-A15"受击伤害管线钩子"互为镜像：需要"自身 buff 修改后续所有攻击的命中参数/异常注入"的注入口（萨亚冰冻、罗刹出血+中毒、各类"攻击时概率 XX"附魔系被动都会撞）。建议形态：MeleeHitAction 执行前查询施法者 buff 列表的可叠加 ProcBuffId 槽（BuffDefinition 加 `virtual int[] OutgoingProcs`），代价小、先例同构（HitReaction.ProcBuffId 既有）。
2. **单 HitReaction 多异常槽**：罗刹同击双异常（出血+中毒）超出 ProcBuffId 单槽——管线立项时一并扩展（数组化）。
3. **引擎内置未注册 PO**（12275 形态）：passiveobject.lst 无条目、行为纯引擎——与 F3 引擎内置技能同类但更隐蔽（"脚本创建但数据不可见"）。记档：遇到 `sq_SendCreatePassiveObjectPacket(未注册ID)` 直接按不可走读处理。

**给下轮的经验**：`ghost\` 目录三件套（hero/ghostsoulrelease/…）是鬼泣二觉的"鬼神指挥"体系，hero 的选鬼对象是 25/36/41/75 四阵 PO 的 **lst 注册 ID**（20011/20012/20013/20040）——分析这四技时记得回填 hero 的联动面；CD=0 循环技能我们框架原生支持（CooldownMs=0 不进 CD 表），无需特判。
