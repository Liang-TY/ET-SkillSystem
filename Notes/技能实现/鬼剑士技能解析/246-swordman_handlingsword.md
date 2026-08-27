# 万剑归宗（swordman_handlingsword）

> 技能ID 246 | 级别 A | 可实现性 🔶（buff 状态+终结乱舞主干可表达：30s 环剑 Buff + 终结"抓取→10 段乱舞→刺入/斩击/爆炸三连"Area 编排；但核心手感"**其他技能命中时**落剑穿云刺"依赖技能命中事件钩子（新缺口）、环绕剑视觉撞 Buff 视觉挂接（R1-A5）、finale 抓取撞 Grab 系统（R2-A7）三处降级） | 分析日期 2026-08-22 | 批次 A18

## 1. 基本信息

| 项 | 值 | 来��� |
|---|---|---|
| 中文名 | 万剑归宗 | `skill\Swordman\swordman_handlingsword.skl` [name] |
| 英文名 | swordman_handlingsword（取 skl 文件名；无 [name2]，实测） | 同上 |
| 职业 | 剑魂（85 级二觉大招；[second growtype maximum level] 第 5 位=30，四技槽位互证见 231 文档；登顶剑神之境=剑魂二觉常识） | 同上 + 技能名常识 |
| 学习等级 | 85 | 同上 [required level] |
| 最高等级 | 40（二觉档上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↓↑→→ + Z | 同上 [command] |
| CD | 180000 ms | 同上 [cool time] |
| MP | 2500 → 5000 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×10 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14 | 同上 [executable states] |
| static data | **无 [static data] 节**（实测） | — |
| 一句话效果 | 30 秒内随心操纵武器：普攻/跳攻/前冲攻与 10 个指定技能**命中时**落剑穿云刺（按技能落 1/2/4/5 把）；再施放/到期/死亡进入最终阶段——按领主>精英>HP最高优先抓取目标乱舞后爆炸 | 同上 [explain]（长文） |

**level property（7 列，Lv1 → Lv40 首末值，7 向量全 -1 源列直读）**：

| 列 | 模板变量 | Lv1 值 | 印证处 |
|---|---|---|---|
| col0 | 持续时间 | 30000×0.001=30 秒（恒） | buff setValidTime |
| col1 | [穿云刺]攻击力 | 972% | onAttackParent 写包 subType1 |
| col2 | 最后多段攻击力 | 3512% | 写包 subType2/3 |
| col3 | 最后爆炸攻击力 | 17999% | 写包 subType3 |
| col4/5/6 | 终结控制范围 最大x/y/z | 150 / 40 / 350（恒） | onendcurrentani 目标扫描 |

落剑数映射（explain 直录）：普攻/跳攻/前冲攻/三段斩/猛龙断空斩=1 把；流心:刺/跃/升=2 把；幻影剑舞/极·神剑术(破空斩)=4 把；破军斩龙击=5 把。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 49（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/handlingsword/handlingsword.nut", "swordman_handlingsword", 246, 246);
// swordman_header.nut 行 78/106/338/616（实测）：STATE/SKILL_SWORDMAN_HANDLINGSWORD <- 246
//   CUSTOM_ANI_SWORDMAN_HANDLINGSWORD_SRART_BODY <- 168；APID_SWORDMAN_HANDLINGSWORD <- 229（appendage 数字 id）
```

F7 变体：主 nut 138 行管"开关 + 状态挂钩"，**技能本体是一个 30 秒 appendage（ap_handlingsword_buff，475 行，全技能最重的 buff 脚本）**；伤害体仍是共享 PO 24370（subType 1/2/3 三模式，L20）。

### 2.2 主 nut 逐回调（handlingsword.nut，138 行）

- **checkExecutableSkill（开关分流，实测）**：已带 buff → 调 `onEndRangeAttack`（立即终结）+ 移除 buff + **return false（本次不施放）**——"再按一次=提前终结"；未带 → SetState 246。
- **onSetState**：播动画 168（Handlingsword_Srart_Body.ani 31 帧 1488ms）。
- **onKeyFrameFlag flag 1（F20 @960ms 实测）**：`ChangeSkillEffect(word=1)` → **onChangeSkillEffect case 1**：挂 `ap_handlingsword_buff`（valid time=col0=30s）+ `AppendAppendageID(229)`（数�� id 附身标记，视觉/状态层）。
- **setState_Swordman（全局状态钩子：buff 期间角色进入任何状态都回调，实测）**：状态→落剑数映射——
  `STATE 7/8/15/22/39 → 1`（跳攻/普攻段/前冲/三段斩22/猛龙断空斩39）；`62/63/64 → 2`（流心:刺/跃/升，F6 互证）；`40/234(substate0) → 4`（幻影剑舞/极·神剑术破空斩）；`58(substate99) → 5`（破军斩龙击）；`STATE_DIE → 终结 + 移除 buff`。写入 buff var[0]=本状态可落剑数。
- **onEndRangeAttack（终结发起点，buff 到期/再按/死亡共用）**：写包 `(246, subType=2, col2 多段%, col3 爆炸%, col4, col5, col6)` → PO 24370 于自身位置。
- **onEndCurrentAni**：回 STAND（施法动画 1.5s 后角色自由，**30 秒 buff 状态自治**）。

### 2.3 被动对象 / appendage

**① ap_handlingsword_buff.nut（475 行，技能本体核心）**：

- **onStart**：初始化 var[0]=0（待命剑计数）；**5 把环剑视觉**——`sq_AddEffectBack` ×5（Handlingsword_Stand_Sword_normal_A-E.ani 循环，挂在角色背后的特效层槽位）。
- **onAttackParent（角色攻击命中时回调，**核心机制**，实测）**：20ms 节流；触发条件=攻击源处于上述映射状态之一（或伤害源是 PO 24370 且 var skill==234，即破空斩的弹体也算）；var[0]>0 且有空闲环剑（帧号 12 的循环动画=待命态）→ 消耗一把（视觉切 Lifting 协成 200% 起飞）+ var[0]-1 + 写包 `(246, subType=1, col1 穿云刺%, 敌group, 敌uniqueId, 敌x/y/z+身高/2)` → **PO 24370 于目标身上落剑**。
- **drawAppend（每帧渲染回调）**：飞剑彗星视觉——ballhead+balltail1-4 拖尾按 Atan2 旋转 + IMAGE RATE(-1,1) 镜像，从环剑位置飞向目标 800ms 弧线（**Buff 视觉挂接缺口的又一重实例，R1-A5**）。
- **proc**：末 5% 时间标记 + 环剑状态机（recoverLoop 待命恢复 / recoverSword 回归动画 Returning_E）。
- **onVaildTimeEnd（30s 到期）**：调 onEndRangeAttack 终结。

**② 共享 PO 24370 case 246（share_obj\swordman\ 实测）**：

| subType | 写包 | 行为 |
|---|---|---|
| 1 穿云刺 | col1 + 目标三参 | state10：Handlingsword_Launch_Eff_G.ani（发射视觉）；state11：**定位到目标**（活→目标位；死→写包快照位）+ anim 58 flyingsword_eff_B.ani（10 帧 820ms，F3-F6 攻击盒）+ atk37 HandlingSwordShoot.atk（damage/push100/lift250/blow）；命中再挂 atk_eff 视觉（随机三角度旋转）；播完销毁 |
| 2 终结·乱舞 | col2/col3/范围三参 | 先移除 buff；state10：handlingsword_finish_start_sword_normal_a.ani（14 帧 960ms，起手黑闪）；state11：**目标抓取**（见下）+ anim 59 Finish_A_SwordNormal_P.ani（37 帧 2960ms，**flag 10 ×9 段 = resetHitObjectList 九连击**，F10-F32 攻击盒）+ atk38 FinishFirst.atk（lift100/blow）；flag1@2400ms → 生成 subType3 + 把抓取目标再拖到地面 |
| 3 终结·三连 | col2/col3 | anim 60 Finish_B_AtkEff_A.ani（58 帧 4640ms）：flag1(F5/F7/F10/F11) 切 **atk39 FinishSecondStuck（刺入，push200/lift150）**；flag10(F18) 重置；flag2(F22) 切 **atk40 FinishSecondCut（斩击，push300/lift100）**；flag3(F32)+flag10(F38)+flag4(F39) 切 **atk41 FinishSecondExplosion（爆炸，down/push300/lift500）**；播完销毁 |

**③ 终结目标选择（onendcurrentani.nut:133 case 246 subType2 state10→11，实测完整算法）**：
```
扫描全部敌方（可抓取 holdable + 可伤害）：优先级 boss(2) > named/精英(1) > 普通(0)——
  同档内取 HP 最高者为主目标；再以主目标为中心，收集 x≤150 / y≤40 / z≤350（col4-6）内其余敌人；
  主目标 + 范围内敌人全部：ap_handlingsword_control（hold）+ sq_HoldAndDelayDie + sq_MoveToAppendage（拖到 PO 位 z=50，500ms）
```
ap_handlingsword_control.nut（20 行）为空壳 hold 标记。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\handlingsword_srart_body.ani（槽168，注意文件名 Srart 拼写） | 31 | 1488ms | **F20=1(@960ms)**→挂 buff | 无 | .als 挂 Wing_First_Light 系 7+ 层（展开双翼光） |
| PO Handlingsword_Launch_Eff_G.ani | 7 | 480ms | 无 | 无 | 穿云刺发射视觉 |
| PO Handlingsword_flyingsword_eff_B.ani | 10 | 820ms | 无 | **F3-F6**（穿云刺判定） | .als 含 [SPECTRUM]（剑光残影） |
| PO Handlingsword_Stand_Sword_normal_A-E.ani ×5 | 12 | 1080ms 循环 | 无 | 无 | 环剑待命（帧 12=待命标记） |
| PO handlingsword_finish_start_sword_normal_a.ani | 14 | 960ms | F2=1/F3=2（无脚本处理） | 无 | 终结起手 |
| PO Handlingsword_Finish_A_SwordNormal_P.ani | 37 | 2960ms | **F9=2；F10/14/16/17/19/20/23/25/27=10（九段重置）；F30=1→生成三连** | **F10-F32** | 乱舞主体 |
| PO Handlingsword_Finish_B_AtkEff_A.ani | 58 | 4640ms | **F5/F7/F10/F11=1（刺入）；F18=10；F22=2（斩击）；F32=3/F38=10/F39=4（爆炸）** | **F1-F14/F19/F22/F23/F25/F40-F46/F48** | 三连终结 |
| ballhead / balltail1-4.ani | — | — | — | — | 飞剑彗星（drawAppend 手绘，非独立 PO） |

`.als` 边车：角色 1 个 + PO 目录 handlingsword_atk_eff_a.ani.als / finish_b_atkeff_a.ani.als 等。mod 目录 332 文件（含 finish_b 系 slash/swordnormal/bg/last 等 ~300 视觉层）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_handlingsword.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_handlingsword.skl` | ✅（263 行） | 7 列数据 |
| 注册行 | load_state 行 49（246/246） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 78/106/338/616 | 同文件 | ✅ | 状态/动画 168/APID 229 |
| 主 nut | handlingsword.nut | `…\pvf\sqr\character\swordman\handlingsword\handlingsword.nut` | ✅（138 行） | 开关+状态挂钩 |
| buff nut | ap_handlingsword_buff.nut | 同目录 | ✅（475 行） | 技能本体（环剑/穿云刺/到期） |
| hold nut | ap_handlingsword_control.nut | 同目录 | ✅（20 行空壳） | 抓取 hold 标记 |
| 共享 PO | share_obj\swordman\ case 246 subType1/2/3 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅（L20） | 穿云刺/乱舞/三连 |
| .chr 条目 | etc motion #168（行 1141） | `…\pvf\character\swordman\swordman.chr` | ✅ | Handlingsword_Srart_Body.ani |
| 角色 .ani/.als | handlingsword_srart_body.ani + .als | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 不存在 | 伤害全在 PO |
| PO .obj | qq506807329new_swordman_24370.obj（etc motion #58-60 / etc attack #37-41） | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | 穿云刺/乱舞/三连动画与 atk 表 |
| PO .ani/.als | handlingsword 目录 332 文件 | `…\passiveobject\script_sqr_nut_qq506807329\swordman\animation\handlingsword\` | ✅ | 全视觉池 |
| PO .atk | HandlingSwordShoot / FinishFirst / FinishSecond{Stuck,Cut,Explosion}.atk | `…\passiveobject\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ | §2.3 |
| 施法特效 | Effect\HandlingSword\ 20 文件 | `…\pvf\character\swordman\effect\animation\HandlingSword\` | ✅ | 施法翼光层 |
| 装备层 | *handlingsword*.ani ×76 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |

## 4. 资源需求

| img（按 NPK 族归并） | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_…avatar_skin.NPK | 角色动画 | 必需（共享） | ✅ |
| handlingsword_stand_normal / stand_dodge.img | sprite_character_swordman_effect_handlingsword_\* | **环剑 5 把**（Stand/Lifting/Returning/finish_start 共用 stand 图集） | 必需 | ❌ |
| handlingsword_shoot_eff.img | 同族 | 穿云刺飞行体（flyingsword_eff_B） | 必需 | ❌ |
| handlingsword_atk_eff_a / atk_eff_b.img | 同族 | 穿云刺命中爆点 | 必需 | ❌ |
| handlingsword_finish_normal.img | 同族 | 乱舞主体（Finish_A） | 必需 | ❌ |
| handlingsword_finish_sword_normal / sword_dodge.img | 同族 | 三连剑体 | 必需 | ❌ |
| handlingsword_finish_eff_a~h（8 张）/ finish_dodge.img | 同族 | 乱舞/三连合成层 | 必需（简版可裁至 2-3 张） | ❌ |
| handlingsword_start / start_b / stand_white / stand_shoot / return.img | 同族 | 施法/回归视觉 | 可选 | ❌ |
| sword_light_tail.img | sprite_character_swordman_effect_hundredsword_sword_light_tail.NPK | 飞剑彗星拖尾（L14 跨技能借图，087 已记） | 必需（穿云刺手感） | ❌ |
| timeslash_boomgas / vajra_finish_eff / atchainkick×2 / legsuplexex（格斗家借图 4 张） | 各源 NPK | 乱舞合成层 | 可选 | ❌ |

缺失 img：必需级约 15 张（handlingsword 族 25 张一次提取全覆盖 + hundredsword 跨族 1 张）；可选级 ~10 张。全部未入库。img 版本红线由提取时把关。

## 5. 实现方案草案（穿云刺触发方式降级：命中钩子 → 按键主动射剑）

### 内容件清单

1. **`DotNet~/Skills/HandlingSwordSkill.cs : SkillLogic`**（BloodBoomSkill 帧触发 + FireCircleSkill 状态技合成）
   - `CooldownMs=180000`（demo 60000）；`TotalTimeMs=1488`（施法动画全长）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanHandlingSwordStart)` + `ctx.AddBuffToSelf(BuffIds.HandlingSwordState)`（30s 环剑状态 Buff，**F20@960ms 的挂接简化为 OnCast 即挂**——flag 精度损失 1s 内，可接受）+ SubState=0。
   - OnUpdate：**状态期间技能自身已结束（TotalTimeMs 到时 OnEnd），30s 状态由 Buff 承载**——但"再按终结"需要 buff 期间监听输入：demo 简化为 **Buff 挂上后本技能不进 CD（ManualCooldown=true），再按=TryCast 新一转** → checkExecutableSkill 语义重建：OnCast 里 `if (ctx.GetNumeric(self, NumericType.Hp)>0 && 已带Buff标记)` 判断……
     ——**此处暴露框架边界**：无"跨施放查询自身 Buff"门面（AddBuffToSelf 可挂、查不可）。给出两条落地路径：
     ① **门面小补**（推荐）：SkillContext 加 `bool HasBuff(int buffId)`（LSBuffComponent 已有查询，纯门面暴露）→ 再按=检测到 Buff 在身 → 不重施，改触发终结（OnUpdate 监听 `PeekBufferedButton`）；
     ② 无门面版：终结绑定到 Buff 的 RemoveActions 之外无法提前触发——只能"到期自动终结"，demo 先做 ① 前的手动版=按第二下没反应、30s 到自动终结（手感降级，§7）。
   - OnEnd：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Buffs/HandlingSwordStateBuff.cs : BuffDefinition`**（30s 环剑状态）
   - `TotalTimeMs=30000`（col0 原值）、`TickTimeMs=0`；AddActions/RemoveActions 暂空（视觉层见 §7）；
     到期终结：Buff 无"到期回调施法"通道（RemoveActions 是 Action 节点，可配 `SpawnFinaleAction`——见下）。
3. **`DotNet~/Actions/SpawnHandlingSwordFinaleAction.cs : LSAction`**（Buff RemoveActions 触发终结区，ForbidMoveOn 同粒度先例）
   - `Run(ctx)`：`ctx.GetSourceId()` 定位施法者 → 于施法者位置创建 `AreaIds.HandlingSwordFinale`（**Action 无"按实体创建 Area"门面**——LSActionContext 现有门面无 CreateArea；同 241 处理：终结区由技能/Action 二选一承载，demo 取**技能 OnCast 时即预排 30s 后时序不可行**（技能早结束）→ **升级路径=LSActionContext 加 CreateAreaAtOwner 门面**（新缺口上报 §8），第一版先做"无 Buff 的常驻版"：技能 OnCast 后 30s 计时挂在…… 无处可挂——**结论：本技能的 30s 延迟终结必须落在框架小补上（Buff 到期创建 Area 或 HasBuff 门面二选一），草案按 ① HasBuff + 技能 ManualCooldown 重施转终结 给出**）。
   - 简化定案（写明取舍）：**第一版砍 30s 状态，做"瞬发终结版"**——OnCast 播施法 → 960ms 后直接进终结编排（抓取/乱舞/三连三 Area），CD 60s；穿云刺与环剑作为 Buff 视觉增强后补。这样零框架改动即可跑通终结链（乱舞/三连的所有数值/判定全保真）。
4. **`DotNet~/Areas/HandlingSwordStrikeArea.cs : AreaDefinition`**（穿云刺，后补版）
   - `TotalTimeMs=820`（flyingsword_eff_B）、`EnterActions={MeleeHit}`、`HalfExtents=(0.8,0.5,0.8)`（目标位小盒）、
     `HitReaction{Damage=40, HitstunMs=400, KnockbackX=100, LaunchY=250}`（atk37 原值 push100/lift250/blow）、`ViewAnimId=AnimId.HandlingSwordStrike`。
   - 创建方：后补版由"带 Buff 期间普攻命中"钩子（新缺口）或按键触发（见 §7）。
5. **`DotNet~/Areas/HandlingSwordBarrageArea.cs : AreaDefinition`**（乱舞九段，FireCircleArea Tick 范式）
   - `TotalTimeMs=2960`（Finish_A 全长）、`TickTimeMs=280`（2960/9 段≈flag10 间隔实测 720→2400ms 均布）、
     `EnterActions={MeleeHit}`、`TickActions={MeleeHit}`、`HalfExtents=(1.5,0.6,3.5)`（col4-6 控制范围÷100）、
     `HitReaction{Damage=70, HitstunMs=400, KnockbackX=0, LaunchY=100}`（atk38 lift100/blow——乱舞浮空连打）、`ViewAnimId=AnimId.HandlingSwordFinishA`。
6. **`DotNet~/Areas/HandlingSwordFinaleArea.cs : AreaDefinition`**（刺入/斩击/爆炸三连→两 Area 近似）
   - 三连的 atk 参数切换（39→40→41）单 Area 表达不了 → 拆两个：
     `HandlingSwordStuckCutArea`：`TotalTimeMs=1760`（F0-F22 刺入+斩击段）、`TickTimeMs=400`（两段）、`HitReaction{Damage=90, HitstunMs=500, KnockbackX=250, LaunchY=120}`（39/40 折中）；
     `HandlingSwordExplosionArea`：由技能 OnUpdate 在乱舞+三连时序点创建、`TotalTimeMs=470`、`EnterActions={MeleeHit}`、
     `HitReaction{Damage=220, HitstunMs=1000, KnockbackX=300, LaunchY=500}`（atk41 原值 down/push300/lift500）、`ViewAnimId=AnimId.HandlingSwordFinishB`。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 246 + 30s appendage | `HandlingSwordStateBuff`（后补）/ 第一版瞬发终结版 SkillLogic |
| 环剑 5 把（sq_AddEffectBack） | Buff 视觉挂接缺失（R1-A5 第 4 实证）——后补：BuffDefinition 视图通道或 overlay 手组装 |
| onAttackParent 落剑（他技命中钩子） | **新缺口���技能命中事件钩子**——后补（§8） |
| 落剑数按状态映射（1/2/4/5） | 同上依赖钩子；后补 |
| drawAppend 飞剑彗星 | 视觉延后（Area ViewAnimId 直出穿云刺飞行体替代） |
| PO subType1 穿云刺（定位到目标） | `HandlingSwordStrikeArea`（CreateArea 于目标位——**目标位读取门面**：CreateAreaInFront 用自身朝向，目标位需 GetEnemies+位置读，见 §8） |
| 目标优先级抓取（boss>精英>HP最高） | `ctx.GetEnemies()` + `ctx.GetNumeric(t, NumericType.Hp)` 可算 HP 档；领主/精英 tier 查询缺失 → 简化 HP 最高 |
| HoldAndDelayDie+MoveToAppendage 拖拽 | Grab 系统（R2-A7 拆解在案）——第一版不抓取，乱舞区原地结算 |
| flag10 ×9 resetHitObjectList | Area `TickTimeMs`（同段定时档，L19） |
| atk39/40/41 三段参数切换 | 两个 Area 拆分（L9 同构） |

### 注册点清单（草案号段，A18 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.HandlingSword=26` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanHandlingSwordStart=120、HandlingSwordOrbit=121（Stand 剑循环）、HandlingSwordLaunch=122、HandlingSwordStrike=123、HandlingSwordFinishA=124、HandlingSwordFinishB=125 |
| AreaId | `AreaDefinition.cs` | HandlingSwordStrike=26、HandlingSwordBarrage=27、HandlingSwordStuckCut=28、HandlingSwordExplosion=29 |
| BuffId | `BuffDefinition.cs` | HandlingSwordState=13 |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×6；img 必需 ~15 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 180000ms | 60000（瞬发终结版） |
| 状态时长 | 30000ms | 第一版砍（瞬发）；后补 30s |
| 穿云刺 | col1 972%（push100/lift250/blow），按状态 1-5 把 | 40 伤小盒区 |
| 乱舞 | col2 3512% ×9 段（lift100/blow） | 70×9 tick |
| 刺入 | atk39（push200/lift150） | 并入 StuckCut 区 |
| 斩击 | atk40（push300/lift100） | 并入 StuckCut 区 |
| 爆炸 | col3 17999%（down/push300/lift500） | 220/硬直1000/推300/浮500 |
| 终结控制范围 | x150/y40/z350 | Barrage 盒 (1.5,0.6,3.5) |
| 终结时序 | 起手960+乱舞2960+三连4640 ≈ 8.6s | 同序（Area 链） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 332 个 PO .ani + 角色 .ani | 节面常规（含 GRAPHIC EFFECT，L15） | 现有 ani 子命令全覆盖 |
| handlingsword_flyingsword_eff_c.ani | `[SPECTRUM]` 族（武器残影，R1-A1 首报后又一实证） | 已记档：跳过（无残影系统） |
| finish 系 .ani | `[FLIP TYPE]`（本批 45 处之一） | 同 231 §6：flipType 字段 |
| 角色 .als + atk_eff_a.ani.als 等 | `[none effect add]`/`[add]` 已支持 | 覆盖 |
| swordman_handlingsword.skl（7 列） | `.skl` 无子命令（既有） | 手抄 7 值可接受 |
| 5 个 PO .atk | `.atk` 无子命令（既有）；`[hit wav]` 记档 | 手抄 |
| 24370 .obj | `.obj` 无子命令（既有） | etc #58-60/#37-41 手工映射 |
| drawAppend 的 IMAGE RATE(-1,1)/sq_SetfRotateAngle | 运行时旋转缩放（非翻译问题） | 视图延后档 |

计 3 条既有缺口 + 1 条新节（[FLIP TYPE]）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **他技命中时自动落剑**（onAttackParent + 状态→剑数映射） | **新缺口：技能命中事件钩子**（现有系统命中结算在 CombatSystem，技能无"我打中了"回调；环剑数=Buff 内状态，LSCast 之外的跨技能联动） | 后补：①LSHitboxComponentSystem 命中处发事件（帧同步安全=直接调 Buff 查表）或 ②demo 降级=带 Buff 期间按专用键手动射剑（输入缓冲现成） |
| 环剑 5 把视觉（effect-back 层 + 待命/起飞/回归状态机） | Buff 视觉挂接（R1-A5 第 4 实证——按宿主渲染层挂动画族） | 后补 BuffDefinition 视图通道；第一版仅技能图标/音效替代 |
| 再按提前终结 / 到期自动终结 / 死亡终结 | 无自身 Buff 查询门面 + Buff 到期无施法回调 + 死亡无技能钩子 | 第一版瞬发终结版（§5 定案）；升级=HasBuff 门面 + RemoveActions 造终结区 |
| 终结抓取+拖拽（boss>精英>HP最高，HoldAndDelayDie） | Grab 系统完整缺失（R2-A7 拆解在案）+ tier 查询缺失 | 不抓取：HP 最高目标位（GetEnemies+GetNumeric 可算）生成乱舞区，敌人原地挨打 |
| 飞剑彗星 drawAppend（弧线+旋转+镜像） | 逐帧绘制回调不存在 | 穿云刺 Area 的 ViewAnimId 直出（直线飞行体） |
| 落剑数 1/2/4/5 按技能差异化 | 同命中钩子缺口 | 第一版固定 1 把/次 |
| 屏震（8/12 级）×多段/黑闪白闪 | 屏震闪屏（延后） | 跳过 |
| 332 文件视觉池 | 非缺口（提取量问题） | 必需 15 张先行，finish 合成层后补 |

## 8. 存疑与缺口上报

**未考证项**
1. `AppendAppendageID(229)`（APID_SWORDMAN_HANDLINGSWORD=229）的数字 id appendage 具体内容（数字 id 表未定位——挂载的视觉/属性标记作用未解）。
2. 穿云刺 20ms 节流 timer 的精确行为（setParameter(20,-1) 语义按 DNF timer 惯例推断）。
3. 环剑"待命=帧号 12 的循环动画"判定在 5 把剑间的分配顺序（getSwordCount 从 0 号起消耗，实测代码但动画细节未逐帧核对）。
4. finish_b 系 ~300 视觉层与 58 帧 Finish_B 的精确挂接（mod 视觉池，仅抽样核对 atkeff_a/bg/last 三族）。

**新缺口上报（主循环汇总）**
1. **技能命中事件钩子**（"技能/普攻命中目标时"回调，供 Buff/状态联动消费）——万剑归宗穿云刺、各类"命中触发"被动的共同前提；建议 LSHitboxComponentSystem 命中结算处加可注册回调（内容层经 BuffDefinition 配置消费，避免内容层直接挂系统事件）。
2. **自身 Buff 查询门面**（`SkillContext.HasBuff(buffId)`）——开关型技能（本技能再按终结、241 ON/OFF）通用；LSBuffComponent 已有查询能力，纯门面暴露，代价极低。
3. **Buff 到期触发效果/施法**（RemoveActions 已有但只能配 Action 节点，而"到期创建 Area"缺 LSActionContext 侧创建门面）——与缺口 2 合并评估。
4. **目标位置读取门面**（"在指定敌人位置创建 Area/Bullet"——CreateAreaInFront 只支持自身朝向偏移）——穿云刺/落点类技能通用，与 R2-A10 施法者位置读取门面同族。

**给下轮的经验**：**"技能本体是 appendage"的形态首见**（主 nut 只是壳 + 全局状态钩子 setState_Swordman）——遇 `sq_AddFunctionName("onAttackParent"/"drawAppend"/"onVaildTimeEnd")` 即此类，行为全在 ap_*.nut；终结类 PO 的"目标优先级扫描"都在 onendcurrentani 的 state 切换处（引擎先播起手动画、动画结束才选目标），别��� setcustomdata 里找。
