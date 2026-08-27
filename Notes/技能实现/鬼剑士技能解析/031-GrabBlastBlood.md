# 嗜魂之手（GrabBlastBlood）

> 技能ID 31 | 级别 A | 可实现性 ⛔（抓取/目标控制系统缺失——手册 §6.3 已列档；深简化"定身连招"近似见 §5） | 分析日期 2026-08-22 | 批次 A7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 嗜魂之手 | `skill\Swordman\GrabBlastBlood.skl` [name] |
| 英文名 | GrabBlastBlood（取 skl 文件名；[name2] 实测为英文别名 `Bloodlust`） | 同上 [name2] |
| 职业 | 狂战士（[skill fitness growtype]=3，L17 映射） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 70（六系上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 2） | 同上 [type] |
| 指令 | →→ + Z（指令施法 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 6000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 37 → 290（Lv1→70） | 同上 [consume MP] |
| static data | `1200 500 600 4`（**推断**：500=抓取结束后无敌时长 ms——与 grabhand.nut `sq_PostDelayedMessage(INVINCIBLE,0,0,500)` 硬编码互证；1200/600/4 未解，疑为抓取判定窗口/拖拽距离/可抓目标数上限） | 同上 [static data] + §2.2 |
| 一句话效果 | 抓住前方一名敌人吸其血气再喷发，造成物理伤害并增加自身力量；对出血敌人伤害更高且力量时间更长；可抓霸体/防御态敌人 | 同上 [explain] |
| 屏震 | [shake screen] 2 400（喷发屏震，延后档） | 同上 |

**level property（6 列，Lv1 → Lv70 首末值）**：`352→2242`、`502→3202`、`30000(恒)`、`46→942`、`528→3362`、`30000(恒)`。
列语义**推断**（引擎内置消费，无 nut 佐证；与 explain 逐句对齐）：col0=抓取/吸血段攻击力%、col1=喷发段攻击力%、col2=力量 buff 持续 30000ms、col3=力量增加值、col4=对出血敌人的攻击力%、col5=对出血敌人的力量 buff 延长 30000ms。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 128（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/grabhand/grabhand.nut", "GRABHAND", STATE_GRABBLASTBLOOD, SKILL_GRABBLASTBLOOD);
// swordman_header.nut 行 3/84（实测）：STATE_GRABBLASTBLOOD <- 26，SKILL_GRABBLASTBLOOD <- 31
```

- 状态名 **GRABHAND**、状态号 **26**、技能 ID **31**——L2 第 4/5 参区分的又一实证。
- `grabhand.nut`（122 行）是**共享抓取失败处理**：技能 31（嗜魂之手）与技能 102（嗜魂之手 TP 强化版 GrabBlastBloodEx，lst 实测）共用。
- **抓取成功的主流程不在任何 nut 里**——GRABHAND 状态 26 的抓取机（抓住/拖拽/控住/终结触发）是引擎内置状态机，pvf 只提供失败分支 + 数据文件（F3 引擎内置模式的"半内置"变体：注册行存在，nut 只写边角）。

### 2.2 主 nut 逐回调（grabhand.nut——失败分支与收尾）

- `onSetState_GRABHAND`：把写包向量 (substate, skill, target) 存进 var `value`——引擎切进 26 时带入。
- `onAttack_GRABHAND(obj, damager, ...)`（**抓取判定的命中回调**）：
  ```
  if (substate==0 && skill==31 && target==-1):        // 抓取尝试段命中了一个对象
      if (!sq_IsHoldable || !sq_IsGrabable || sq_IsFixture(damager)):   // 不可抓（建筑/部分 Boss/固定物）
          createGrabBloodHandunGrabEffect(obj, 42, 1, 85)               // 播"抓空"血雾特效（bloodlustgrabcannon_00.ani，前方 42px）
          记录 damager → 切子状态 1（抓空收势段）
          → sq_AddSetStatePacket(26, ...) 带参 (1, 31, -1)
      // 反之（可抓）→ 引擎接管成功分支：控住目标进入抓取演出（无脚本，引擎内置）
  else if (skill==102): 同构失败分支（TP 版）
  ```
  ——**三段判据** `sq_IsHoldable / sq_IsGrabable / sq_IsFixture` 就是 DNF 抓取免疫体系的 API 面（对少数敌人无效 = isGrabable=false）。
- `onAfterSetState_GRABHAND`：进入子状态 1（抓空）时 `sq_SendHitObjectPacket(obj, dama, 0,0,0)`——对不可抓目标仍结算一次普通命中（用 GrabBlastBlood.atk 的 stuck 数据）。
- `onEndCurrentAni_GRABHAND`：
  ```
  sq_SendMessage(obj, OBJECT_MESSAGE_INVINCIBLE, 1, 0);          // 立即开启无敌
  sq_PostDelayedMessage(obj, OBJECT_MESSAGE_INVINCIBLE, 0, 0, 500); // 500ms 后关闭
  ```
  ——**抓取动画结束获得 500ms 无敌**（抓技安全窗口，UNBREAKABLE 家族又一实例，印证缺口累计档）。
- `onEndState_GRABHAND`：离开状态 26 时若习得技能 117（BLOODINKANET，mod 新技能）清除自定义伤害类型（mod 附加逻辑，与原版无关）。

### 2.3 抓取演出与终结（引擎内置 + 数据文件拼合还原）

**① 抓取尝试**：引擎播 `Grab.ani`（.chr etc motion #10，实测 17 帧 640ms）：
- **F7-F14 连续 8 帧攻击盒** `8 -19 47 80 38 67`（偏移+尺寸格式）→ x∈[8,88]/y∈[-19,19]/z∈[47,114]，**身前窄小盒=单目标抓取判定**；
- F8 SET FLAG 100（引擎抓取触发标记）、F15 flag 65534（收尾标记，语义未考证）；
- 命中反应 `GrabBlastBlood.atk`（etc attack info #13，实测）：**damage reaction = none**、push 0、lift 0、`[stuck] -1000`——纯"定住"命中，无打击反应（目标将被整个拖入抓取演出）。

**② 抓取中（成功分支，引擎内置）**：目标被控住（`ap_grabhand.nut` 实测：appendage 持续到施法者回 STATE_STAND 才失效——被抓者期间完全受控）、角色进入吸血演出（引擎控制双方位置同步）。

**③ 终结喷发（引擎创建 PO）**：`passiveobject\character\swordman\grabblastblood.obj`（实测）：
- name `怒气爆发抓取时的打击`（文案沿用怒气爆发，实为本技能终结 PO）；
- `[basic motion]` GrabBlastBlood1.ani（6 帧 480ms，**全帧攻击盒**，如 F0 `2 -37 -67 133 74 132` → x∈[2,135]/y∈[-37,37]/z∈[-67,65]）+ `[etc motion]` 2/3/4.ani 组成 4 相位血气喷发序列；
- `[attack info]` **PO 侧** `GrabBlastBlood.atk`（`passiveobject\...\attackinfo\`，实测——与角色侧同名文件是两份！L3/L9 双印证）：**down 击倒/push 500/lift 100/blow**——喷发终结的击飞手感；
- 大终结变体 `grabblastbloodbig.obj`（6 相位，GrabBlastBloodBig1-6.ani + GrabBlastBloodBig.atk：down/push500/lift100）；
- 无情追击档 `grabblastbloodmercilessness.atk`（PO 侧，实测 down/push500/lift100/blow/weapon damage）——与 gorecrossmercilessness 同命名惯例（064 首见），判定为**对出血敌人的强化终结**（explain"对出血状态敌人增伤"的命中参数载体，触发条件引擎内置未考证）。
- 视觉：PO 侧 `grabblastbloodm_boom.ani`（9 帧 595ms，4 帧攻击盒，引 **bloodboom_boomfront.img——浴血之怒爆炸图，已在库**）+ `m_fluid`（血气流体 420ms）/`m_dustr4`（尘土）；角色侧 `effect\...\grabblastblood\bloodlustgrabcannon_00.ani`（27 帧 944ms 血炮）+ 其 **.als**（[none effect add] 3 层：grabcannon_01@F1/层-1、02@F2/层-2、03@F4/层-3——官方 .als 含此节，L12 又一实证）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\swordman\animation\Grab.ani | 17（F0-16） | 640ms（每帧 40ms，F16=0） | F8=100、F15=65534 | **F7-F14（8 帧）** | 抓取尝试动作；sm_body 图集 |
| passiveobject\...\grabblastblood1.ani | 6 | 480ms | 无 | 全 6 帧 | 终结 PO 相位 1（impact-normal.img） |
| passiveobject\...\grabblastblood2/3/4.ani | 未逐帧（2 号实测 5 帧 300ms 无盒） | — | 无 | 1 号有/后续无 | 相位 2-4（impact-dodge/blood-normal/blood-dodge） |
| passiveobject\...\grabblastbloodbig1-6.ani | 6 号实测 6 帧 480ms | — | 无 | 1 号有 | 大终结 6 相位 |
| passiveobject\...\grabblastblood\grabblastbloodm_boom.ani | 9 | 595ms | 无 | 4 帧 | 爆发主视觉（借 bloodboom 图） |
| character\swordman\effect\...\bloodlustgrabcannon_00.ani | 27 | 944ms | 无 | 无 | 抓空/喷发血炮 + .als 3 层 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GrabBlastBlood.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GrabBlastBlood.skl` | ✅ | 技能数据 |
| 注册行 | load_state 行 128（GRABHAND/26/31） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 3/84 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | STATE=26 / SKILL=31 |
| 主 nut | grabhand.nut | `…\pvf\sqr\character\swordman\grabhand\grabhand.nut` | ✅（122 行） | 失败分支 + 无敌收尾（成功流程引擎内置） |
| ap nut | ap_grabhand.nut | `…\pvf\sqr\character\swordman\grabhand\ap_grabhand.nut` | ✅（41 行） | 抓取持有 appendage（回待机即放） |
| .chr 条目 | etc motion #10（行 983）；etc attack info #12/#13/#71（行 1306/1307/1365） | `…\pvf\character\swordman\swordman.chr` | ✅ | Grab.ani；Grab.atk/GrabBlastBlood.atk/GrabBlastBloodEx.atk |
| 角色 .ani | Grab.ani | `…\pvf\character\swordman\animation\Grab.ani` | ✅ | 抓取尝试动作 |
| 角色 .atk | grabblastblood.atk / grab.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 定住命中 / 抓取基础命中 |
| .als | bloodlustgrabcannon_00.ani.als | `…\pvf\character\swordman\effect\animation\grabblastblood\` | ✅ | 血炮 3 层叠加（[none effect add]） |
| PO 定义 | grabblastblood.obj / _ds / big / big_ds / ex | `…\pvf\passiveobject\character\swordman\` | ✅ | 终结 PO（§2.3） |
| PO .ani | grabblastblood1-4、big1-6、grabblastbloodm_* ×10 | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | 喷发相位与视觉 |
| PO .atk | grabblastblood.atk / big / mercilessness（+ _ds） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 终结/大终结/无情追击命中 |
| 特效 .ani | bloodlustgrabcannon_00-03.ani | `…\pvf\character\swordman\effect\animation\grabblastblood\` | ✅ | 抓空 + 喷发血炮 |
| 装备层 | grab*.ani ×96 | `…\pvf\equipment\character\swordman\avatar\` | ✅（find 计数） | 抓取动作换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | Grab.ani 角色动作 | 必需（共享） | ✅ 已在库 |
| Character\Swordman\Effect\BloodBoom\bloodboom_boomfront.img | sprite_character_swordman_effect_bloodboom.NPK | grabblastbloodm_boom 爆发主视觉（**跨技能复用浴血之怒**） | 必需 | ✅ 已在库（bloodboom_boomfront.img.bytes） |
| Character\Swordman\Effect\GrabBlastBlood\impact-normal.img / impact-dodge.img | sprite_character_swordman_effect_grabblastblood.NPK | PO 相位 1-2（冲击） | 必需 | ❌ |
| Character\Swordman\Effect\GrabBlastBlood\blood-normal.img / blood-dodge.img | 同上 | PO 相位 3-4（血气） | 必需 | ❌ |
| Character\Swordman\Effect\GrabBlastBlood\normal.img / blood.img | 同上 | 大终结相位 5-6 | 可选 | ❌ |
| Character\Swordman\Effect\GrabBlastBlood\GBBM_fluid.img / GBBM_dustR4.img | 同上 | 血气流体内嵌层 / 尘土 | 可选 | ❌ |
| DemonicSwordsman\Effect\GrabBlastBlood\*.img ×6 | sprite_character_demonicswordsman_effect_grabblastblood.NPK | 剑影 _ds 变体（本职业不用） | 不需要 | ❌ |
| Character\Priest\Effect\ChoppingHammer\*.img | — | 本次映射中未被 31 消费（bluntmasterysub 借图，见 108 号文档） | 不涉及 | — |

缺失 img：必需级 4 张、可选级 4 张，全部集中在 sprite_character_swordman_effect_grabblastblood.NPK（一次提取全覆盖）。

## 5. 实现方案草案（⛔ 级正式方案免——以下为"定身连招"深简化近似，供立项评估）

**我们侧若要做，需要什么**（对照两侧）：

| DNF 机制 | 需要的系统 | 现状 |
|---|---|---|
| 抓住目标：目标位置被施法者牵引同步（引擎控双方） | **抓取/目标控制系统**：把目标 LSUnit 挂到施法者的控制流（禁 AI/禁输入/位置跟随/朝向锁定） | ❌ 缺失（手册 §6.3 已列） |
| 抓取判定三判据（Holdable/Grabable/Fixture） | 单位属性位（可抓性标记） | ❌ 无单位属性系统 |
| 抓取演出（角色与目标共用一套时间轴，目标无受击反应） | 双单位演出同步（damaged reaction=none + 受控） | ❌ 依赖上行 |
| 抓取结束 500ms 无敌 | 无敌帧 | ❌ 缺失（R1-A5 记档） |
| 力量 buff（col2/3） | 属性数值消费链（Attack 侧同 R1-A4 缺口） | ❌ 缺失 |
| 对出血敌人增伤（col4/5） | Buff 查询门面 | ❌ 缺失（R1-A3 记档） |

**深简化近似（不做抓取，做"定身连招"）**——机制近似度约 60%（伤害时序全保，控住演出降级为定身）：
- `GrabBlastBloodSkill : SkillLogic`（同 BloodBoomSkill 帧触发范式）：
  - OnCast：`ctx.PlayAnim(AnimId.SwordmanGrab)`（Grab.ani 翻译件）+ `ctx.ClearHitTargets()`；`CooldownMs=6000`、`TotalTimeMs=640`（抓取动画时长）。
  - OnUpdate F7-F14 窗口（帧驱动攻击盒走 Grab.ani 自带 attackBoxes——json 翻译后 LSHitboxComponentSystem 自动激活）：首次命中 → `ctx.AddBuff(target, BuffIds.GrabHold)`（新 Buff：TotalTimeMs=900、AddActions={ForbidMoveOn 动作}——复用 FreezeBuff 的 ForbidMoveOn/Off 范式即"假抓取"）+ `ctx.SetSubState(1)` 锁单目标。
  - OnEnd 前 200ms：`ctx.CreateAreaInFront(AreaIds.GrabBlastBlood, 0.9)` 终结喷发（对定身目标必中）。
- `GrabBlastBloodArea : AreaDefinition`（同 ReleaseWaveArea 一次性爆发范式）：`TotalTimeMs=480`、`EnterActions={MeleeHit, AddBleedBuff}`、`HitReaction{Damage=150(喷发 col1 demo 值), HitstunMs=800, KnockbackX=500, LaunchY=100}`（PO atk 原值 push500/lift100/down）、`ViewAnimId=AnimId.GrabBlastBoom`（grabblastbloodm_boom，图已在库）。
- 手感差异说明：目标定在原地而非被"抓到脸前"，无吸演出，无双人合体时间轴——读者一眼能看出不是抓取；但伤害节奏（抓→定→爆）与击飞手感保留。
- 注册点（草案号段）：SkillIds.GrabBlastBlood=16、AreaIds=4、AnimIds 63-66（Grab/Boom/Cannon/Finish）、BuffIds.GrabHold=8、json ×3、img 4 张必需。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| Grab.ani、grabblastblood*.ani、m_boom/m_fluid/m_dustr4、bloodlustgrabcannon_00-03.ani | 节面常规（本次实测无规则外节） | **现有 ani 子命令全覆盖** |
| bloodlustgrabcannon_00.ani.als | [use animation]+[none effect add]——**已支持**（L12/L15） | 无缺口 |
| grabblastblood.obj / big.obj（多相位 basic+etc motion 序列） | `.obj` 无子命令 | ���入既有 `.obj` 缺口（L9 相位建模建议已在档）；本技能 2 个 obj 手工映射 Area 可接受 |
| GrabBlastBlood.skl（6 列）+ 角色/PO 5 个 .atk | `.skl`/`.atk` 无子命令 | 并入既有缺口；本技能手抄量小（6+~30 值） |

计 3 条既有缺口（.skl/.atk/.obj），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 抓住敌人吸血（目标被控、位置牵引、双人演出） | **抓取/投掷 Grab 系——缺失档**（手册 §6.3 明列；本技能为首个完整走读样本） | §5 定身近似（ForbidMove Buff 替代控制） |
| 可抓性三判据（霸体可抓/建筑不可抓/少数 Boss 免疫） | 单位属性位缺失 | demo 全部可"抓"（全部定身）；判据表后补 |
| 抓取结束 500ms 无敌 | 无敌帧（缺失，R1-A5 记档） | 跳过 |
| 力量 buff 30s（col2/3） | 属性数值消费链（缺失，R1-A4 记档——Attack 侧） | 跳过或做 NumericType.AttackAdd 增减对偶（无消费则只记账面） |
| 对出血敌人增伤+延时（col4/5） | Buff 查询门面（缺失，R1-A3 记档） | 跳过（demo 固定伤害）；mercilessness.atk 记档备用 |
| 喷发屏震 [shake screen] 2 400 | 屏震（延后档） | 跳过 |
| TP 强化版（102）共用状态机 | — | E 批另行分析（基础档为准） |

## 8. 存疑与缺口上报

**未考证项**
1. static data `1200 500 600 4` 除 500（无敌）外的精确语义。
2. level property 6 列语义（§1 推断与 explain 对齐，无 nut 佐证）。
3. 成功分支的引擎演出细节（抓住后角色/目标动画编排——Grab.ani 之外引擎是否切专用"持握"动画，无数据文件可考）。
4. grabblastbloodbig（大终结）与 mercilessness（无情追击）的**触发条件**（疑出血状态相关，引擎内置判定）。
5. flag 100（F8）语义（疑抓取判定生效标记）。

**系统级缺口（非新增，实证补充）**
- **抓取/投掷（Grab 系）**：手册 §6.3 已列，本技能首次给出完整机制拆解——补足定义：需要"目标控制流（禁 AI/禁输入/位置跟随）+ 双人演出同步 + 可抓性属性位"三个子系统，比单一"抓取 API"面更大。建议 00-总览 汇总时引用本档 §5 表格作为立项依据。
- 抓取结束无敌 500ms：无敌帧缺口的第 4 个实例（R1-A5 累计 1→本批 18/31 两例再证）。

**给下轮的经验**：鬼剑士其余抓取技（如 35 怒气爆发 Ex、102/各类投掷）都走 GRABHAND 状态 26——**直接读 grabhand.nut + Grab.ani + passiveobject\...\grabblastblood\* 一套通吃**；"注册行存在但 nut 只写失败分支"= 成功流程引擎内置的判据（F3b 半内置变体，建议补进轮间经验）。
