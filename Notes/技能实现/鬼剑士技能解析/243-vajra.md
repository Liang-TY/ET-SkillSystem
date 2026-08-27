# 天雷 · 降魔杵（vajra）

> 技能ID 243 | 级别 A | 可实现性 🔶（跟随云/索敌落点/感电增伤消费链三降级；判定主干与节拍可直译） | 分析日期 2026-08-22 | 批次 A17

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 天雷 · 降魔杵 | `skill\Swordman\swordman_vajra.skl [name]` |
| 英文名 | vajra（取 skl 文件名去 swordman_ 前缀） | 同上 |
| 职业 | 阿修罗 · 天帝（二觉；[second growtype maximum level] 槽 9=30 → 按 233 反推公式 槽=growtype×2(+1) 即 growtype 4=阿修罗二觉档，L17 互证） | 同上 + 233 交叉 |
| 学习等级 | 80 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↓→→ + Z；指令施法 MP 优惠 50%/50% 档 | 同上 [command] / [command key explain] / [skill command advantage] |
| CD | 45000 ms（固定） | 同上 [cool time] |
| MP | 800 → 6000 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块（道具 3037）×5 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14 | 同上 [executable states] |
| 一句话效果 | 头顶召唤跟随的天雷云持续最多 50s；每 1s 锁定周围敌人降下 3 支带角度的降魔杵（感电，上限 15 支）；再按技能键或时间到→云移至前方 → 27 支杵的终结雨 → 巨型金刚降魔杵砸地双重爆炸；对感电敌人伤害 ×1.5 | 同上 [explain] + PO 走读 |

**level property（14 列，Lv1 dungeon）**：col0 降魔杵攻击力 **455%**、col1 巨型降魔杵攻击力 **4600%**、
col2 巨型降魔杵爆炸攻击力 **12865%**、col3 感电几率 **100%**、col4 感电Lv **75**（向量源 **-6**，L21 未解族）、
col5 感电持续时间 **1500ms**（×0.001）、col6 感电攻击力 **1348**（固定值）、col7 雾气持续时间 **50000ms**（×0.001）、
col8 降魔杵生成间隔 **1000ms**（×0.001）、col9 数量上限 **15**、col10 降临范围 **430px**、
col11 最后一轮追加个数 **12**、col12 最后一轮爆炸攻击力 **463%**、col13 对感电敌人增伤 **50%**（全等级恒定）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 75（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/vajra/vajra.nut", "swordman_vajra", 243, 243);
// 行 8-13：共享打击 PO 24370（L20/F7；本技能是其最复杂的用例——5 个 subType + 主云状态机 10~14）
```

- `swordman_header.nut`：`STATE/SKILL_SWORDMAN_VAJRA <- 243`（行 75/103）、`CUSTOM_ANI_SWORDMAN_VAJRA{CAST,FINISH} <- 161/162`（行 331/332）。**无角色侧 CUSTOM_ATTACK**（全部伤害走 PO，character\attackinfo\ 无 vajra 文件，实测）。
- .chr 对位（0 基实测吻合）：etc motion #161/162 = 1134/1135 行 `VajraCast.ani` / `VajraFinish.ani`（行号-973 ✓）。
- 主 nut 107 行，mod 混淆（C3 族），语义已还原。

### 2.2 主 nut 逐回调（vajra.nut，两子状态 + 再按终结）

**checkExecutableSkill（核心入口，再按检测在这里）**：
```
若自己的 PO 24370(id 243) 存在且 state==11（雾气持续中）：
    → 给 PO 发 state 12 + 数据（前方 165px 处 x, 角色y）    // 云飞往终结点
    → 自己切 subState 2（终结施法动作）；return false（不重进状态机主流程）
否则 sq_IsUseSkill(243) → (subState 1) 进状态 243
```

**onSetState**：
- subState 1（召唤）：播 #161 VajraCast（22 帧 1280ms，无攻击盒无 flag）；手部闪电视觉（vajra_start_06_hand_lightning_b.ani）；
  **写包创建 PO 24370 at (0,0,350)**——出生即 z=350px 高空：`[243, subType 0, col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13]`（**全部 14 列 level 数据 + addRate=col13**）；音 SM_VAJRA + VAJRA_CLOUD。
- subState 2（终结动作）：播 #162 VajraFinish（17 帧 240ms，F15 flag 65534 取消窗口标记，语义未考证）。

**onEndCurrentAni**：两子状态播完 → STAND（云是独立 PO，继续存在）。

### 2.3 共享 PO 24370（mod 版，case 243 —— 5 subType 家族）

**主云（subType 0，跟随施法者，状态机 10→11→12→13→14）**：

| 状态 | 行为 | 动画/atk（mod obj 0 基直读） |
|---|---|---|
| 10 成云 | 播成云动画一次 → onend→11 | etc motion **#69** `vajra_start_cloud_eff.ani`（12 帧 1120ms） |
| 11 雾气持续 | **setMapFollowParent(type1)**——云跟随角色；**timeEvent 0（30ms）**：向角色缓移（每拍 |dx|/80+1 px），父死亡或 elapsed ≥ col7(50s) → 自动 state 12（云原地）；**timeEvent 1（col8=1000ms）**：`sq_FindFirstTarget(-430,430,430)`（col10 范围内首个敌人）→ 发 ChangeSkillEffect(1, 敌x, 敌y)；临近 50s 大限前补发一次性 timeEvent 1（col8+1000ms） | **#70** `vajra_maincloud_cloud.ani`（15 帧 1200ms **LOOP**） |
| —（消息 1） | onChangeSkillEffect：停 timeEvent 1，云闪 a/b 视觉，存 atkPos(敌x,敌y)，**setTimeEvent 2（200ms × 3）**：每拍若剩余支数 var9>0 → 算云→敌连线的俯角（atan2）**带旋转**生成 subType 1 小杵 PO（写包：col0 攻击力 + 感电 4 参 + 旋转角 + addRate），支数 -1 | — |
| 12 云飞移 | procappend：从云位向目标点（再按时=前方 165px）1000ms 匀速插值 → 13 | — |
| 13 终结雨 | **timeEvent 3（100ms × col9+col11=27 次）**：云周围 ±80/±35 随机位生成 subType 2 小杵（写包：col0 + col12 爆炸% + 感电 4 参 + addRate） | — |
| 14 巨型杵砸地 | 播动画（31 帧 2480ms）；**F18 flag 1** → 震 10 持续 delaySum(18,30)；**F23 flag 2** → 收黑屏 + 白闪；同时创建 subType 3/#4 终结爆炸 PO ×2（atk31 + atk32 双层）；黑屏（delaySum(0,0), 99999, 0→150）；音 VAJRA_CHANGE；播完销毁 | **#71** `vajra_finish_maincloud_lightning_a.ani` |

**小杵 subType 1（索敌型）**：state 10 = 随机动画 #42-44（A/B/C 三变体）+ **atk 29**；F1 delay 被脚本改写为 1000ms（悬停 1s，L23 事件推进族）；procappend 从云位向 atkPos **三轴匀速插值下坠（z→0）**，带 setCustomRotate 旋转角（斜落）；F1 flag 1（落地）→ 震 2/50 + 落雷视觉；onend → state 11：旋转归零，播 #45 `vajra_finish_break.ani` + **atk 30** 落地碎裂 → 销毁。
**小杵 subType 2（终结雨型）**：同 1 但 F1 delay=1300ms、直线 80ms 落地、碎裂时附加 col12 爆炸倍率。
**subType 3/4（巨型杵双层爆炸）**：分别 #46 `vajra_finish_exp_main_vajra.ani` + **atk 31**、#47 `vajra_finish_exp_lightning_b.ani` + **atk 32**（写包带 col1/col2 倍率 + 感电 4 参 + addRate）。

**onAttack（主云/杵每次命中敌人）**：挂 `ap_vajra.nut` 到目标（若未挂）——**感电增伤钩子**：
```
getImmuneTypeDamageRate：目标处于 ACTIVESTATUS_LIGHTNING 且伤害来源 PO 的 skill==243 → 返回 rate × 1.5
```
（×1.5 硬编码 = col13 的 50% 全等级恒定，数值吻合；addRate 存入 ap 变量但乘法用常量——mod 作者偷懒写法）

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒（min/max ÷100=单位） | 备注 |
|---|---|---|---|---|---|
| `character/…/vajracast.ani`（#161） | 22 | 1280ms | 无 | 无 | .als 挂 7 层（start_eff A-F 帧 4-9 起 + 手部云 @帧14）；仅引 sm_body |
| `vajrafinish.ani`（#162） | 17 | 240ms | F15=65534 | 无 | 取消窗口标记（064 同款，语义未考证） |
| PO `vajra_start_cloud_eff.ani`（#69） | 12 | 1120ms | 无 | 无 | 成云；引 Vajra_circlecloud.img |
| PO `vajra_maincloud_cloud.ani`（#70） | 15 | 1200ms | 无 | 无 | **LOOP**；引 Vajra_maincloud.img |
| PO `vajra_bullet_main_a.ani`（#42，B/C 同构） | 2 | 1920ms（F1 被脚本改 1000/1300ms） | F1=1（落地） | **F0/F1**：`-19,-40,-68,39,80,136`（下落杵体） | 引 Vajra_drop.img；带 .als |
| PO `vajra_finish_break.ani`（#45） | 15 | 520ms | 无 | **F2-F5**：`-127,-51,-66,250,103,228` 等（碎裂冲击） | 引 Vajra_Finish_Break.img |
| PO `vajra_finish_maincloud_lightning_a.ani`（#71） | 31 | 2480ms | **F18=1**（巨型杵落）、**F23=2**（白闪） | 无 | 引 Vajra_start_Eff.img；带 .als |
| PO `vajra_finish_exp_main_vajra.ani`（#46） | 29 | 2720ms | 无 | **F20-F27**：`-97,-40,150,196,80,314` 等（高空巨型杵贯穿区） | 引 Vajra_finish_Eff.img |
| PO `vajra_finish_exp_lightning_b.ani`（#47） | 47 | 3600ms | 无 | **F30/F31**：`-272,-80,-1,544,160,331`（**8.16 单位宽终结爆区**） | 引 Vajra_drop.img；带 .als |
| effect/animation/vajra/ 8 个 + PO 目录 68 个 | — | — | — | — | 全视觉层；节名实测全部常规（FRAME/LOOP/SHADOW/[add]/[use animation]） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_vajra.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_vajra.skl` | ✅（268 行） | 14 列等级数据 |
| 注册行 | load_state 行 75（243/243）+ 行 8-13 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 243 + PO 24370 |
| 常量 | swordman_header.nut 行 75/103/331/332 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画槽位 |
| 主 nut | vajra.nut（107 行，混淆已还原） | `…\pvf\sqr\character\swordman\vajra\vajra.nut` | ✅ | 召唤/再按终结 |
| ap nut | ap_vajra.nut（45 行） | `…\vajra\ap_vajra.nut` | ✅ | 感电增伤钩子（getImmuneTypeDamageRate ×1.5） |
| PO 回调 | common_object/share_obj/swordman/ 五个 nut 的 case 243（onAttack/onKeyFrameFlag/onTimeEvent/onChangeSkillEffect/destroy + setcustomdata/setstate/procappend/onendcurrentani） | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅ | 5 subType 全逻辑 |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | etc motion #42-47/#69-71、atk #29-32 对位（本批复验 0 基无错位） |
| PO atk | VajraSmallSword / VajraSmallSwordFinishBreak / VajraCloud / VajraCloudFinish .atk | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ | 小杵/碎裂/巨型杵主爆/终结爆 |
| .chr 条目 | etc motion #161/162（1134/1135 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | 两动画；**无 atk 条目** |
| 角色 .ani | vajracast.ani + .als、vajrafinish.ani（无边车） | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| PO .ani | Vajra/ 68 个（+9 个 .als） | `…\script_sqr_nut_qq506807329\swordman\animation\Vajra\` | ✅ | 云/杵/爆全套 |
| PO 镜像 | passiveobject\character\swordman\animation\vajra\（同构一套） | 同上 | ✅ | 官方部署位副本 |
| 特效 .ani | effect/animation/vajra/ 8 个 | `…\pvf\character\swordman\effect\animation\vajra\` | ✅ | 施法手部特效 |
| 装备层 | *vajra* ×152 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层（存在性） |

atk 关键值（PO 侧，实测）：VajraSmallSword=magic/光/**down**/push100/lift100/blood40；FinishBreak=magic/光/**down**/knuck back -1/push50/lift **250**；VajraCloud=magic/光/damage/无 push；VajraCloudFinish=magic/光/**down**/push100/lift **450**/no blood。

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 两段角色动画 | 必需（共享） | ✅ |
| `Character/Swordman/Effect/Vajra/Vajra_maincloud.img` | sprite_character_swordman_effect_vajra.NPK | **主云视觉**（LOOP 循环） | **必需** | ❌ |
| `…/Vajra/Vajra_drop.img` | 同上 | **下落杵体 + 终结爆层**（108 处引用） | **必需** | ❌ |
| `…/Vajra/Vajra_Finish_Break.img` | 同上 | 小杵碎裂 | **必需** | ❌ |
| `…/Vajra/Vajra_finish_Eff.img` | 同上 | **巨型杵主爆**（224 处引用，本技能最大视觉） | **必需** | ❌ |
| `…/Vajra/Vajra_start_Eff.img` | 同上 | 巨型杵砸落 | **必需** | ❌ |
| `…/Vajra/Vajra_circlecloud.img` | 同上 | 成云 | 可选（成云 1120ms 一闪，主云循环可代） | ❌ |
| `…/Vajra/Vajra_lightning_B.img` | 同上 | 闪电层（143 处） | **必需**（雷视主体） | ❌ |
| `…/Vajra/Vajra_lightningcloud.img`、`Vajra_Break.img` | 同上 | 云雷/碎裂变体 | 可选 | ❌ |

缺失 img：**必需 6 张、可选 3 张——全部同一个 NPK**（sprite_character_swordman_effect_vajra），一次提取全覆盖。
img 版本红线（v2/v4 可 / v5 不可）由提取时把关。

## 5. 实现方案草案

**结构映射**：云 = 长寿命纯视觉 Area（无判定）；每秒落杵 = 技能 OnUpdate 定时 CreateArea（小杵区：EnterActions 单发 = atk29+30 合并）；终结雨 = Tick 100ms × 27 无去重 Area；巨型杵 = 两个顺序 Area（atk31 主爆 + atk32 终结爆）；再按终结 = PeekBufferedButton 窗口（064 追击同构）。
**系统性简化**：云不跟随角色（静态于施放点——Area 无跟随门面）；索敌落点 → 固定前方（目标枚举门面可做但 SubState 存 id 重查 054 式较绕，demo 从简）；感电 = 新增简化 Buff；感电增伤 ×1.5 无消费链 → 跳过（R1-A4 最重缺口第四实证）。

### 内容件清单

1. **`DotNet~/Skills/VajraSkill.cs : SkillLogic`**（SubState 时间编排 + 长技）
   - `CooldownMs = 45000`；`TotalTimeMs = 8000`（demo 云期 6s + 终结 2s；DNF 50s 太长，演示缩短，云期长短为配置项）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanVajraCast)` + `ctx.ClearHitTargets()`；SubState=0。
   - `OnUpdate`：
     - t ≥ 1280（Cast 播完）：`ctx.CreateAreaInFront(AreaIds.VajraCloud, FP.Zero)`（纯视觉云，TotalTime=6720）；`ctx.PlayDefaultAnim()`；SubState=1；记 `nextDrop = 1280+1000`。
     - 云期每 1000ms（共 6 次，DNF 每 1s 三连简化为 1 次单杵）：`ctx.CreateAreaInFront(AreaIds.VajraDrop, 1.5单位)`（前方落杵区）；期间 `ctx.PeekBufferedButton()==<本技能键>` → `ctx.ConsumeBuffer()` 提前跳终结（SubState=2）。
     - 云期满/再按：`ctx.PlayAnim(AnimId.SwordmanVajraFinish)` + `ctx.CreateAreaInFront(AreaIds.VajraRain, 1.5)`（终结雨 Tick 100ms×27）；SubState=2。
     - t 终点-1400：`ctx.CreateAreaInFront(AreaIds.VajraFinish, 1.5)`（巨型杵主爆，atk31）。
     - t 终点-700：`ctx.CreateAreaInFront(AreaIds.VajraFinishBlast, 1.5)`（终结爆，atk32）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/VajraCloudArea.cs`**（纯视觉）：`TotalTimeMs=6720`、无 Actions、`ViewAnimId=AnimId.VajraCloudLoop`（LOOP）。
3. **`DotNet~/Areas/VajraDropArea.cs`**（小杵）：`TotalTimeMs=520`（碎裂 520ms）、`EnterActions={MeleeHit}`、`HalfExtents=(1.9,0.8,1.5)`（碎裂盒 F2 x[-127,250] 折算）、`HitReaction{Damage=40, HitstunMs=500, KnockbackX=50, LaunchY=250, ProcBuffId=BuffIds.VajraShock, ProcChance=100}`（atk29+30 合并；down/lift250；感电 100% 概率挂简化 Buff）。
4. **`DotNet~/Areas/VajraRainArea.cs`**（终结雨，L19 Tick 无去重档）：`TotalTimeMs=2700`、**`TickTimeMs=100`**、`TickActions={MeleeHit}`、`HalfExtents=(1.6,0.5,1.5)`、`HitReaction{Damage=30, HitstunMs=250, KnockbackX=50, LaunchY=100, ProcBuffId=BuffIds.VajraShock, ProcChance=50}`。
5. **`DotNet~/Areas/VajraFinishArea.cs`**（巨型杵主爆，atk31）：`TotalTimeMs=2720`（伴随动画 F20-F27 判定窗）、`EnterActions={MeleeHit}`、`HalfExtents=(2.0,0.6,1.6)`（F20 盒折算）、`HitReaction{Damage=250, HitstunMs=800, KnockbackX=0, LaunchY=0}`（atk31：damage 反应无 push）、`ViewAnimId=AnimId.VajraFinishExpMain`。
6. **`DotNet~/Areas/VajraFinishBlastArea.cs`**（终结爆，atk32）：`TotalTimeMs=600`、`EnterActions={MeleeHit}`、`HalfExtents=(4.1,1.2,1.7)`（**F31 盒 x[-272,544] 8.16 单位宽**折算）、`HitReaction{Damage=300, HitstunMs=1000, KnockbackX=100, LaunchY=450}`（atk32 原值 down/push100/lift450）、`ViewAnimId=AnimId.VajraFinishExpLightning`、`ViewBackAnimId` 可选。
7. **`DotNet~/Buffs/VajraShockBuff.cs : BuffDefinition`**（感电简化——视觉/短硬直向）：`TotalTimeMs=1500`（col5 原值）、`TickTimeMs=500`、`TickActions={ShockDamageTick}` 或简化为 AddActions={MeleeHit 微伤害}——DNF 感电是"受击增幅"异常，无伤害消费链时简化为周期小伤害（同 Bleed 结构，Damage=col6 1348 缩到 demo 值 10）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 主云 PO 跟随角色（setMapFollowParent + 30ms 缓移） | **Area 无跟随门面**（新缺口，见 §8）→ 云静态于施放点（手感差异：走远后云留在原地） |
| 每 1s FindFirstTarget(430px) → 3 连带角度落杵 | 技能 OnUpdate 定时 + 固定前方落点（目标枚举门面 R3-A11 已记档，可后补） |
| 小杵悬停 1000ms 后三轴插值下坠 + 旋转 | 简化为瞬时 Area（下坠演出靠 ViewAnimId 自带 F0 悬停帧） |
| 再按技能键 → 云飞移 + 终结 | PeekBufferedButton 提前终结（064 追击窗口同构；云飞移砍掉） |
| 终结雨 27 支（col9+col11，±80/±35 随机落点） | Tick 100ms × 27 无去重 Area（位置随机缺口 R2-A10——判定等价，视觉单循环） |
| 巨型杵 atk31 主爆 + atk32 终结爆双层 | 两个顺序 Area（064 多相位定案） |
| ACTIVESTATUS_LIGHTNING 感电（L6 链路） | HitReaction.ProcBuffId + ProcChance（FreezeBuff 先例）+ 新 VajraShockBuff |
| ap_vajra 感电增伤 ×1.5（getImmuneTypeDamageRate 钩子） | **受击伤害管线钩子缺失**（R3-A15 记档）→ 跳过（增伤不做） |
| 巨型杵黑屏/白闪/震 10 | 闪屏/屏震延后 → 跳过 |
| 攻速静态化 | 延后 → 固定速度 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.Vajra = 24` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `VajraCloud = 19`、`VajraDrop = 20`、`VajraRain = 21`、`VajraFinish = 22`、`VajraFinishBlast = 23` |
| BuffId | `Runtime\BuffDefinition.cs` | `VajraShock = 10` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanVajraCast = 109`、`SwordmanVajraFinish = 110`、`VajraCloudLoop = 111`、`VajraDrop = 112`、`VajraBreak = 113`、`VajraFinishMain = 114`（可选）、`VajraFinishExpMain = 115`、`VajraFinishExpLightning = 116` |
| json 注册 | `LSAnimClipRegistrar.cs` | 角色 2 + PO 5~7 个（含 .als overlay） |
| 图集 | `LSAnimResComponentSystem.cs` | 必需 6 张（同一 NPK） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000 ms | 45000（直用） |
| 云持续时间 | col7 = 50000 ms | 6000（demo 缩短；配置项） |
| 落杵节拍 | 每 col8=1000ms × 3 支（200ms 间隔），上限 col9=15 支 | 每 1000ms × 1 支 × 6 次 |
| 落杵伤害 | col0 455%（atk29 down/push100/lift100 + atk30 碎裂） | Damage 40 / Hitstun 500 / Kb 50 / Ly 250 |
| 感电 | 100% / Lv75 / 1.5s / 攻击力 1348 | VajraShockBuff 1.5s（简化 tick 10） |
| 感电增伤 | col13 = +50%（×1.5） | 不做（消费链缺失） |
| 终结雨 | 100ms × 27 支（15+12），±80/±35 随机 | Tick 100ms × 27 单区 |
| 巨型杵主爆 | col1 4600%，atk31 damage 无 push | Damage 250 / Hitstun 800 |
| 终结爆 | col2 12865%，atk32 down/push100/lift450；盒宽 8.16 单位 | Damage 300 / Kb 100 / Ly 450 / 半宽 4.1 |
| 终结雨爆炸附加 | col12 463% | 并入 Rain Tick 值 |
| 召唤施法 | VajraCast 1280ms | 直译 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_vajra.skl | `.skl` 无子命令 | **14 列全列技能**（本批最宽表）——skl 子命令高收益样本再证 |
| 4 份 PO .atk | `.atk` 无子命令 | 手抄；`[elemental property] light element`、`[knuck back] -1` 字段并入 atk 立项输入 |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | 本档已给 #42-47/#69-71/#29-32 对位表 |
| 各 .ani/.als | 节名实测全部常规（[use animation]/[add]/[none effect add]/[LOOP]/[SHADOW]） | **无新节缺口** |
| vajra_bullet_main F1 悬停 1000/1300ms | 脚本运行时改写 delay（非文件内容） | 非翻译问题；游戏侧 ViewAnimId 帧表已含原 delay，直接用原 920ms 视觉无碍 |

结论：缺口 = `.skl`/`.atk`/`.obj` 族共性 3 条，无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 云跟随施法者移动（30ms 缓移跟随） | **Area 无跟随/锚定门面（新缺口）** | 云静态于施放点（施放者走远后云不动——手感差异主项，见 §8） |
| 每秒索敌 430px 内首个敌人定向落杵（带角度旋转） | 目标枚举门面（R3-A11）+ 位置类随机（R2-A10） | 固定前方落点；索敌版可后补（GetEnemies + SubState 存 id，054 式） |
| 感电异常（受击增幅语义） | 无元素/异常系统；简化为周期伤害 Buff | VajraShockBuff 1.5s tick（视觉+轻伤害） |
| 感电敌人 ×1.5 增伤 | **属性数值无伤害消费链**（R1-A4 最重缺口，第四实证） | 跳过（记档） |
| 50s 超长云期 + 再按终结 | TotalTimeMs 可表达；再按窗口 = PeekBufferedButton（已有） | 云期缩到 6s（demo）；再按提前终结可通 |
| 悬停 1000ms 后下坠（事件推进帧） | L23 族（超长 DELAY 悬停） | Area 瞬时创建 + ViewAnimId 自带视觉 |
| 27 支独立随机落杵 | 位置随机缺口 | 单雨区 Tick（判定等价；视觉单循环 vs 27 支散布） |
| 黑屏/白闪/震 10 | 延后 | 跳过 |
| 混淆源码 | mod 污染（C3 族） | 本档还原语义为实现依据 |

## 8. 存疑与缺口上报

**未考证项**
1. `-6` level 向量源语义（本技能 = 感电Lv 列；与 234 的 -4=出血攻击力列并存，L21 族继续累积样本）。
2. vajrafinish.ani F15 flag 65534 语义（取消窗口惯例推断）。
3. col4 感电Lv=75 与感电概率 col3=100% 在无元素系统的引擎内如何结算（只作记录）。
4. ap_vajra 的 addRate 变量写入后未消费（×1.5 硬编码）——mod 作者实现瑕疵，数值恰好等价。
5. 镜像两套 Vajra 动画目录未逐文件 diff（同构推断）。

**新系统级缺口（主循环汇总）**
1. **区域/视觉跟随施法者**（follow-owner Area）：本技能主云 30ms 缓移跟随是首个"施放者锚定持续视觉"实证——光环/跟随类技能（暗月降临 R1-A3 同族）都会撞，建议与 Buff 视觉挂接（R1-A5）合并立项评估。
2. **受击伤害管线钩子第四实证**（ap_vajra getImmuneTypeDamageRate ×1.5 无法注入）——R3-A15 已记档，本技能补充"条件增伤（目标异常态 × 来源技能）"用例。
3. 感电（LIGHTNING）异常 Buff 类型缺失——首个需求实证（021 冰冻/燃烧已有，感电需新建）。

**给下轮的经验**：24370 的 case 243 是**多 subType 递归创建**形态（主云 timeEvent 再 CreatePassiveObjectPacket 同 ID 不同 subType——小杵 1/2、终结爆 3/4）；读这类技能先列 subType×state 矩阵再读 timeEvent 表，别按顺序读。主云"每秒找目标→消息回流→200ms×3 落杵"的三段链（timeEvent 1 → ChangeSkillEffect → timeEvent 2）跨三个回调，需拼读。
