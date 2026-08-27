# 王者号令 : 吉格降临（swordman_zigadvent）

> 技能ID 241 | 级别 A | 可实现性 🔶（OFF 冥界拖入模式=固定时序演出 PO，深简化三/四 Area 时序可表达主干；ON 召唤模式=**真 AI 召唤物**（本 pvf 存在完整行为脚本，非引擎演出），撞"召唤物独立 AI"缺失档暂缓；诅咒 debuff/吞食处决/技能镜像联动全部依赖缺失系统） | 分析日期 2026-08-22 | 批次 A18

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 王者号令 : 吉格降临 | `skill\Swordman\swordman_zigadvent.skl` [name] |
| 英文名 | swordman_zigadvent（取 skl 文件名；本 skl 无 [name2]，实测） | 同上 |
| 职业 | 鬼泣（85 级二觉大招；[second growtype maximum level] 第 6 位=30，四技槽位互证见 231 文档；吉格=鬼泣导师常识） | 同上 + 技能名常识 |
| 学习等级 | 85 | 同上 [required level] |
| 最高等级 | 40（二觉档上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 2）；**[seal enable] 1 = ON/OFF 开关技**（封印态施放=OFF 演出，解封态=ON 召唤） | 同上 [type] / [seal enable] |
| 指令 | ↓↑→→ + Z | 同上 [command] |
| CD | 180000 ms（pvp 起手 600000） | 同上 [cool time] |
| MP | 2500 → 5000 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×10 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14 | 同上 [executable states] |
| static data | `270 18 20000 9 -11 19 -50` | 同上 [dungeon][static data] |
| 一句话效果 | ON：召唤神官吉格协助战斗 20 秒，自身施放 9 个鬼气技时吉格镜像联动；OFF：吉格将敌人拖入冥界（魑魅魍魉群袭+诅咒→解放僵直→裂缝→亡者之手拖拽→吞食处决） | 同上 [explain]（长文，两模式） |

**level property（25 列，Lv1 → Lv40 首末值，27 向量逐条对位实测）**：

| 列/槽 | 模板变量 | Lv1 值 | 说明 |
|---|---|---|---|
| col0 | 魑魅魍魉攻击力 | 25328% | 写包模式 1 |
| col1 | 魑魅魍魉攻击间隔 | 150×0.001=0.15 秒 | timeEvent0 间隔印证 |
| col2 | 吉格解放攻击力 | 4695% | 模式 2 state 11 |
| col3 | 冥界裂缝攻击力 | 5869% | 模式 2 state 12 |
| col4 | 最终一击攻击力 | 18780% | 模式 2（atk custom 25） |
| col5-col8 | 诅咒几率/Lv/持续/减属性 | 100% / Lv87 / 3000ms→3s / 50 | 7 参 curse（L6/R2-A10 已记档） |
| col9-col12 | （无模板行——**斩杀线 4 档**） | 30 / 10 / 10 / 20 | ap_zigadvent 按怪物类型取 var[0-3]，见 §2.3 |
| col13 | 降临时魔法攻击力 | 3648% | 吉格登场（24349 case 24） |
| col14 | 鬼影鞭魔法攻击力 | 4521% | 联动 |
| col15/col16 | 三击剑攻击力 | 3281~5361% | 联动 |
| col17 | 鬼影闪魔法攻击力 | 6554% | 联动 |
| col18 | 墓碑魔法攻击力 | 1684% | 联动 |
| static[1] | 墓碑数量 | 18 个 | 联动 |
| col19+static[3] | [刀魂之卡赞]攻击力/加友方攻击力 | 3718% / 9 | 联动 |
| col20+static[4] | [侵蚀之普戾蒙]攻击力/减防 | 6042% / -11×-1=11 | 联动 |
| col21/col22+static[5] | [冰霜/冰晶之萨亚]攻击力/加冰强 | 5113/5113% / 19 | 联动 |
| col23+static[6] | [瘟疫之罗刹]攻击力/减异常抗性 | 3254% / -50×-1=50 | 联动 |
| static[2] | 狂啸之波效果持续时间 | 20000×0.001=20 秒 | ON 模式时长 |
| static[0] | 吉格召唤位（x 前方） | 270 px | nut xPos 印证 |

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 65（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/zigadvent/zigadvent.nut", "swordman_zigadvent", 241, 241);
// swordman_header.nut 行 73/101/329（实测）：STATE/SKILL_SWORDMAN_ZIGADVENT <- 241；CUSTOM_ANI_ZIGADVENT_NEW <- 159
```

**双模式分流**（checkExecutableSkill，实测）：`skill.isSealFunction()`（封印开关，skl [seal enable]）为真 → substate 0（**OFF 冥界拖入**）；为假 → substate 1（**ON 召唤吉格**）。两模式共用动画 159（ZigAdvent_New.ani 15 帧 1240ms，flag 1 @F10=800ms）。

### 2.2 主 nut 逐回调（zigadvent.nut，148 行，无混淆）

- **onSetState**：substate 0/1 均播动��� 159；substate 1 额外挂 `ap_zig_character.nut` appendage（68 行**空壳**，仅存活校验——作为"吉格在场"标记被 ZigAttackWithChr 查询）。
- **onKeyFrameFlag flag 1（F10 @800ms 实测）**：
  - **substate 0（OFF 演出，双 PO 写包）**：
    ```
    写包(241, subType=1, col0 魑魅魍魉%, 诅咒几率col5, Lv col6, 时长 col7, 减属性 col8, 间隔 col1)
      → sq_SendCreatePassiveObjectPacket(24370, 0, x=270, 1, 0)      // 魑魅魍魉风暴
    写包(241, subType=2, col2 解放%, col3 裂缝%, col4 最终一击%, col9, col10, col11, col12)
      → sq_SendCreatePassiveObjectPacket(24370, 0, x=270, -1, 0)     // 吉格本体演出序列
    + 白闪 flashScreen + 池化对象 zigstartback_start_floor.ani（地面暗蚀）
    ```
  - **substate 1（ON 召唤）**：写包 `(24)` → `sq_SendCreatePassiveObjectPacket(24349, 0, zigPos=static[0]=270, 0, 0)`——**F5 共享 PO 24349 新 case 24 = 吉格实体**（unclebang swordman_shared.obj）。
- **onEndCurrentAni**：两模式均回 STAND（角色 1.2 秒后自由行动，演出/吉格自治）。

### 2.3 被动对象 / appendage

**① 共享 PO 24370 case 241（OFF 演出核心，share_obj\swordman\ 实测）**：

- **subType 1 魑魅魍魉**：anim custom 40 = ZigAdvent/GhostWind/GhostWind_1.ani（48 帧 2640ms，F2-F37 全程攻击盒）+ atk custom 26 = ZigGhostWind.atk（magic/暗/damage 反应/**hit direction inner 向心**/push50/lift50）；命中注 7 参 CURSE（col5-8）；`setTimeEvent(0, col1=150ms, ∞)` + else.nut:706 case 241 → 每 150ms resetHitObjectList（**150ms 间隔持续多段+诅咒**）；屏震 2/3500ms。
- **subType 2 吉格本体序列**（setstate 按 state 推进，onEndCurrentAni 自动 +1，**固定时序**）：

| state | 动画（custom） | 攻击（custom） | 时长 | 语义 |
|---|---|---|---|---|
| 10 | 37 ZigStart_Body.ani（17 帧，[CLIP] 节） | — | 875ms | 吉格现身（烟雾特效） |
| 11 | 38 ZigReady_Body.ani（13 帧，F0-F4 攻击盒） | 23 ZigLiberationAttack.atk（down/push50/lift150） | 773ms | **解放：强制僵直+抓取**——else.nut onAttack case 241：对 holdable 敌人挂 ap_zigadvent + `sq_HoldAndDelayDie` + `sq_MoveToAppendage(拖到 PO 中心, 1000ms)` |
| 12 | 39 ZigFinish_Body.ani（59 帧 4760ms，F0-F11 攻击盒，flag 10/1/2/4/3 @1360-3160ms） | 24 ZigFloorHoldAttack.atk（damage/push0/lift0 压制）→ flag 切 25 ZigFinalHandsAttack.atk（down/push50/lift50 最终一击） | 4760ms | 冥界裂缝+亡者之手+终结（zigfinish 系视觉 ×50+） |
| — | 播完销毁 | | | |

OFF 全链 ≈ 800ms（前摇）+ 2640ms（魑魅魍魉并行）+ 875+773+4760ms（本体）≈ **9.2 秒演出**。

- **ap_zigadvent.nut（69 行，吞食处决 appendage，PO 引擎侧挂载）**：onStart 按怪物类型取斩杀线——named→var[1]/hell→var[2]/AI 角色→var[3]/非 boss 非精英→var[0]，boss/精英=9999 不处决；onEnd：`HP ≤ maxHp×斩杀线%` → `setHp(0)` **立即死亡**（var[0-3]=col9-12=30/10/10/20%）。
- **ap_zig_character.nut（68 行空壳）**：在场标记（被 swordman_common.nut 的 ZigAttackWithChr 查询）。

**② 共享 PO 24349 case 24→25（ON 吉格实体，shared_passive_object\swordman\ 实测）——真 AI 召唤**：

- case 24（登场）：custom anim 33（zig 移动系）+ atk 18 zig_darkaura.atk，伤害=col13 降临时魔法攻击力；播完 → 写包 25 自替换。
- case 25（**procappend 每帧行为，完整 AI 状态机**）：`setMapFollowParent`（跟随主人）；state 10=索敌（`sq_FindFirstTarget(-1000,1000,2000,200)`，无敌方则自毁）；state 11=匀速逼近（50000ms 长插值；丢目标→teleport 回主人身后）；50~250px 内→随机 state 12-15（四种自选攻击）；每态调 **`ZigAttackWithChr`**（sqr\character\swordman\swordman_common.nut:309，实测）：
  - **技能镜像联动**：主人施放 鬼影鞭(111)/三击剑(112)/鬼影闪(60)/死亡墓碑(44)/卡赞(25)/普戾蒙(41)/萨亚(36)/冰晶萨亚(96)/罗刹(75) 且不在 CD → 吉格切 state 16-24（各自专属动画+atk，伤害 col14-23）；
  - **再按 241 提前终结**：在场标记有效时 → 就地触发与 OFF 相同的双 PO 演出 + 销毁吉格；
  - 主人死亡（STATE_DIE）→ 同样触发冥界演出收场（procappend case 25 首段，实测）。
- 吉格动画族：`passiveobject\unclebang_shared_passive_object\swordman\animation\zig\`（95 文件：stay/move/auto_attack×4/stonefall/darksword/posssession/soulcutter/scream/darkaura/teleport），atk 9 个（zig_appear/darkaura/darksword/posssession/scream_dark/none/water/soulcutter/stonefall.atk）。贴图为 **sprite_monster_zig_\*（吉格怪物图集，45 张 img）**。
- ⚠ 吉格 20 秒寿命的计时消费方未在六回调中定位（static[2]=20000 无 setTimeEvent 引用）——推断引擎侧 PO 生命周期或未入本 pvf，**未考证**。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\zigadvent_new.ani（槽159） | 15 | 1240ms | **F10=1(@800ms)**→双 PO/召唤 | 无 | .als：Floor_Cast[create draw only object] + Cast_1/2 [none effect add] |
| PO GhostWind_1.ani（魑魅魍魉） | 48 | 2640ms | 无 | **F2-F37**（36 帧连续） | ghostwind 系视觉 ×20+ |
| PO ZigStart_Body.ani | 17 | 875ms | 无 | 无 | **[CLIP] 节（-400 -400 800 400 裁剪窗）** |
| PO ZigReady_Body.ani | 13 | 773ms | F2=1(@138ms) | F0-F4 | [CLIP] 节 |
| PO ZigFinish_Body.ani | 59 | 4760ms | F17=10/F19=1/F25=2/F33=4/F39=3 | F0-F11 | [CLIP] 节；flag 切 atk 24↔25+resetHitObjectList |
| 吉格族（unclebang zig\） | 95 文件 | — | — | — | ON 模式专用 |

`.als` 边车：角色 1 个（zigadvent_new.ani.als）+ zigfinish 等 mod 目录多个；[create draw only object] 已记档缺口第 4 实证。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_zigadvent.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_zigadvent.skl` | ✅（233 行） | 25 列数据 |
| 注册行 | load_state 行 65（241/241） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 73/101/329 | 同文件 | ✅ | 状态 241/动画 159 |
| 主 nut | zigadvent.nut | `…\pvf\sqr\character\swordman\zigadvent\zigadvent.nut` | ✅（148 行） | 双模式分流 |
| ap nut ×9 | ap_zig_character / ap_zigadvent / ap_scream_{bremen,kazan,rasa,saya,sayaex}_{friend,enemy} | 同目录 | ✅ | 在场标记/吞食处决/狂啸之波 buff（scream 系 7 个均为空壳+图标） |
| 镜像联动 | ZigAttackWithChr | `…\pvf\sqr\character\swordman\swordman_common.nut`（行 309） | ✅ | 9 技镜像 + 提前终结 |
| 共享 PO 24370 | share_obj\swordman\ case 241（setcustomdata/setstate/onendcurrentani/else/procappend） | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅ | OFF 演出（L20） |
| 共享 PO 24349 | shared_passive_object\swordman\ case 24/25 六回调 | `…\pvf\sqr\shared_passive_object\swordman\` | ✅（F5） | ON 吉格实体 AI |
| 24349 定义 | swordman_shared.obj（etc motion #31-44 zig 族 / etc attack #15-23） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\` | ✅ | 吉格动画/atk 表 |
| .chr 条目 | etc motion #159（行 1132） | `…\pvf\character\swordman\swordman.chr` | ✅ | ZigAdvent_New.ani |
| 角色 .ani/.als | zigadvent_new.ani + .als | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | —（无，伤害全在 PO atk） | `…\pvf\character\swordman\attackinfo\` | ⛔ 不存在 | — |
| PO .obj | qq506807329new_swordman_24370.obj（etc #37-40 zig 族） | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | 演出动画表 |
| PO .ani | zigadvent 目录 208 文件（zigstart/zigready/zigfinish/ghostwind） | `…\passiveobject\script_sqr_nut_qq506807329\swordman\animation\zigadvent\` | ✅ | 演出视觉 |
| PO .atk | ZigLiberationAttack/ZigFloorHoldAttack/ZigFinalHandsAttack/ZigGhostWind.atk | `…\passiveobject\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ | §2.3 |
| 吉格动画/atk | zig\ 95 文件 + attackinfo zig_*.atk ×9 | `…\passiveobject\unclebang_shared_passive_object\swordman\` | ✅ | ON 模式 |
| 施法特效 | ZigAdvent 目录 28 文件 | `…\pvf\character\swordman\effect\animation\ZigAdvent\` | ✅ | .als 层 |
| 装备层 | *zigadvent*.ani ×152 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |
| 引用缺失 | summon_zig_start_zigbody.ani | `…\pvf\character\swordman\animation\zigadvent\`（目录仅 zigadvent_new/cast 4 文件） | ⛔ **缺失**（swordman_shared.obj etc#31 引用，ON 登场可能降级） |

## 4. 资源需求

| img（按 NPK 族归并） | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_…avatar_skin.NPK | 角色动画 | 必需（共享） | ✅ |
| zig_start_zigbody / zig_start_floor / zig_start_light / zig_start_dodge 等 zig_start 族 | sprite_character_swordman_effect_zigadvent_\*（~20 张） | OFF 现身段 | 必需 | ❌ |
| zig_start_casting / casting_dodge | 同族 | 施法暗幕（zig_start_casting 亦被 238 引用） | 可选 | ❌ |
| GhostWind 族（wind1/wind2_1/ghost1/ghost2/ghostboom1-3/hand*/floorloop*/appearghost 等 ~20 张） | sprite_character_swordman_effect_zigadvent_ghostwind_\* | 魑魅魍魉风暴 | 必需 | ❌ |
| darkaura 族（start/ready/loop ~15 张） | sprite_character_swordman_effect_zigadvent_darkaura_\* | 解放/裂缝段 | 必需 | ❌ |
| zig_finish 族（impact/backdodge/smoke1-3 等 ~5 张） | sprite_character_swordman_effect_zigadvent_zig_\* | 终结段 | 必需 | ❌ |
| sprite_monster_zig_\*（吉格本体 45 张） | monster 侧 NPK | **ON 模式吉格** | ON 必需/OFF 不需要 | ❌ |
| timeslash_circle / bloodboom_shock / bluedragon_exp_nova（跨族借图，L14） | 各源 NPK | 演出合成层 | 可选 | ❌ |

缺失 img：OFF 模式必需 ~40 张（sprite_zigadvent/zig 族一次提取全覆盖）；ON 模式另需 monster_zig 45 张。**全部未入库**（AnimRes 实测无 zig 命名）。img 版本红线由提取时把关。

## 5. 实现方案草案（OFF 演出模式深简化"三 Area + 处决简化"；ON 模式暂缓见 §7）

### 内容件清单

1. **`DotNet~/Skills/ZigAdventSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发 + Blache 站桩引导版范式）
   - `CooldownMs=180000`（demo 30000）；`TotalTimeMs=6600`（演出压缩：800 前摇 + 2600 风暴并行 + 875 现身 + 773 解放 + 1500 终结压缩自 4760——**时序压缩决策先行**，同 087 惯例）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanZigAdvent)`（1240ms 后视图停末帧）+ 站桩。
   - OnUpdate（ElapsedMs + SubState）：
     - `≥800 && SubState==0`：`ctx.CreateAreaInFront(AreaIds.ZigGhostSwarm, 2.7)`（魑魅魍魉风暴区，270px 前方）+ SetSubState(1)；
     - `≥3475 && SubState==1`：`ctx.CreateAreaInFront(AreaIds.ZigLiberation, 2.7)`（解放僵直区）+ SetSubState(2)；
     - `≥4248 && SubState==2`：`ctx.CreateAreaInFront(AreaIds.ZigRift, 2.7)`（裂缝压制区）+ SetSubState(3)；
     - `≥5248 && SubState==3`：`ctx.CreateAreaInFront(AreaIds.ZigFinalHands, 2.7)`（最终一击区）+ SetSubState(4)。
   - OnEnd：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/ZigGhostSwarmArea.cs : AreaDefinition`**（魑魅魍魉：持续多段+诅咒，同 FireCircleArea Tick 范式）
   - `TotalTimeMs=2600`、`TickTimeMs=150`（col1 原值直用）、`HalfExtents=(2.7,0.8,2.0)`、`EnterActions={MeleeHit}`、`TickActions={MeleeHit}`；
     `HitReaction{Damage=25, HitstunMs=300, KnockbackX=-30, LaunchY=0, ProcBuffId=BuffIds.Curse, ProcChance=100}`——**KnockbackX 负值=向心拉拽**（L22；atk26 hit direction inner 同构）；诅咒 Buff 见下。
3. **`DotNet~/Areas/ZigLiberationArea.cs : AreaDefinition`**（解放：强制僵直+聚拢）
   - `TotalTimeMs=773`、`EnterActions={MeleeHit}`、`HalfExtents=(2.7,0.8,2.0)`、`HitReaction{Damage=90, HitstunMs=773, KnockbackX=-50, LaunchY=150}`（ZigLiberation.atk down/lift150 + 负击退=拖向中心，替代 sq_MoveToAppendage）。
4. **`DotNet~/Areas/ZigRiftArea.cs : AreaDefinition`**（裂缝压制）与 **`ZigFinalHandsArea`**（最终一击）
   - Rift：`TotalTimeMs=2000`、`HitReaction{Damage=60, HitstunMs=800, KnockbackX=0, LaunchY=0}`（FloorHold 压制=长硬直贴地）；
   - FinalHands：`TotalTimeMs=470`、`HitReaction{Damage=180, HitstunMs=1000, KnockbackX=50, LaunchY=50}`（ZigFinalHands.atk down/push50/lift50）。
5. **`DotNet~/Buffs/CurseBuff.cs : BuffDefinition`**（诅咒 3s）
   - `TotalTimeMs=3000`、`TickTimeMs=1000`、`TickActions={MeleeHit}` 轻伤害占位——DNF 诅咒=减力智 50 点（col8），**属性数值无伤害消费链（R1-A4）**，与 Blache 沼泽减速同处理：tick 轻伤占位"诅咒在起作用"。
6. **吞食处决**：DNF"HP≤30/10/10/20% 立即死亡"→ 简化为 FinalHands Area `HitReaction{Damage=999}` 对低血量目标近似处决；精确按 HP 阈值斩杀需**受击伤害管线钩子（R3-A15 已记档）**，记 §8。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 241 双模式（seal 开关） | demo 单模式 OFF 演出（ON=SkillLogic 加 toggle 子状态，后补） |
| 模式 1 魑魅魍魉 PO（150ms 重置多段+诅咒） | `ZigGhostSwarmArea`（TickTimeMs=150 直接同构） |
| 模式 2 三态推进（动画结束链） | 技能 OnUpdate 时序创建 3 个 Area（L9 多相位→Area 编排） |
| HoldAndDelayDie+MoveToAppendage 拖拽 | 负 KnockbackX 拉拽（L22）+ 长硬直近似 |
| 吞食处决（按怪物档 HP%） | 简化大伤害；精确版待伤害管线钩子 |
| ON 吉格（跟随/索敌/逼近/随机攻/镜像 9 技） | **召唤物独立 AI（缺失档）——暂缓**，§8 定性 |
| 诅咒减属性 | 属性消费链缺失 → 轻伤 tick 占位 |
| 白闪/屏震/音效 | 延后跳过 |

### 注册点清单（草案号段，A18 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.ZigAdvent=24` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanZigAdvent=110、ZigGhostWind=111、ZigStartBody=112、ZigReadyBody=113、ZigFinishBody=114 |
| AreaId | `AreaDefinition.cs` | ZigGhostSwarm=18、ZigLiberation=19、ZigRift=20、ZigFinalHands=21 |
| BuffId | `BuffDefinition.cs` | Curse=11 |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×5~6；img 必需 ~40 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 180000ms | 30000 |
| 演出总长 | ≈9.2s（前摇 0.8+风暴 2.64 并行+本体 6.4） | 6600（压缩） |
| 魑魅魍魉 | 25328%/150ms 间隔/2600ms | 25 伤/150ms tick/2600ms |
| 诅咒 | 100%/Lv87/3s/减属性 50 | CurseBuff 3s tick 15 |
| 解放 | 4695%（down/lift150） | 90/硬直 773/拉拽 -50/浮 150 |
| 裂缝 | 5869%（压制） | 60/硬直 800 |
| 最终一击 | 18780%（down/push50/lift50） | 180/硬直 1000 |
| 处决线 | 普通 30%/稀有 10%/地狱 10%/AI 20% | 简化大伤害 |
| 吉格（ON） | 20s/降临 3648%/9 技联动 col14-23 | 暂缓 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| zigstart_body / zigready_body / zigfinish_body.ani | **`[CLIP]`（4 值裁剪窗，如 `-400 -400 800 400`）——新节首见**（吉格本体三动画全带） | 建议 ani 子命令加 clip 字段（帧级裁剪矩形）；消费侧视图可先忽略（视觉略溢出）或 renderer 裁剪 |
| zigfinish 系 45 文件 | `[FLIP TYPE]`（HORIZON 帧镜像，本批四技 45 处之一） | 同 231 §6：flipType 字段 |
| zigadvent_new.ani.als | `[create draw only object]`（第 4 实证，已记档） | als 子命令按 [add] 同构输出独立 overlay |
| swordman_zigadvent.skl（25 列） | `.skl` 无子命令（既有） | 25 列手抄偏大，并入既有缺口（批量化例证+1） |
| 4 个 PO .atk + 吉格 9 个 .atk | `.atk` 无子命令（既有） | 手抄 13 文件 ×~8 值可接受 |
| qq506807329new_swordman_24370.obj / swordman_shared.obj | `.obj` 无子命令（既有） | 两 obj 各取 zig 族数对，手工映射 |

计 2 条既有缺口 + 2 条新节（[CLIP]/[FLIP TYPE]）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **ON 模式：吉格随行 AI（跟随/索敌/逼近/随机攻）+ 镜像主人 9 技 + 再按终结** | **召唤物独立 AI（缺失）+ 技能镜像联动（新缺口）+ 在场标记查询（Buff 查询门面缺失）** | ON 模式整体暂缓；若做：吉格=LSUnit+简化 AI（怪物 AI 系统可复用 02 文档），镜像联动=各技能 OnCast 里查标记再召唤（代价大，砍） |
| 亡者之手逐目标拖拽到中心（MoveToAppendage 1000ms） | 位移他人门面（R2-A8） | 负 KnockbackX 拉拽近似（方向=径向向心，L24 已知差异） |
| 吞食处决按怪物档 HP% 斩杀 | 受击伤害管线钩子（R3-A15）+ 怪物 tier 查询 | 大伤害近似或跳过 |
| 诅咒减力智（7 参 active status） | 属性数值无伤害消费链（R1-A4）+ curse 7 参建模（R2-A10 记档） | 轻伤 tick 占位 |
| ON 模式 9 技联动伤害（col14-23 各自成套） | 联动的 9 技需先实现（墓碑 44 已有 044 文档） | 暂缓整体 |
| 演出 9.2 秒站桩 | 技能期间放开控制缺失（同 Blache §7） | 压缩时序 6.6s 站桩版 |
| 白闪/屏震（2-8 级）×多段/音效 | 屏震闪屏音效（延后） | 跳过 |
| summon_zig_start_zigbody.ani 引用缺失 | mod 资源不完整（C6 同族） | ON 模式暂缓无影响；OFF 不用该文件 |

## 8. 存疑与缺口上报

**定性修正（对批次提示的回应，087 怖拉修教训对照）**：吉格降临**两种形态并存**——
- **OFF 模式 = 纯固定时序演出 PO**（模式 2 三态由动画结束链推进，无任何决策分支；模式 1 是定时重置的持续判定场）——与怖拉修同构，深简化 Area 时序可表达（§5 方案即此）；
- **ON 模式 = 真 AI 召唤物**：procappend 里有完整"跟随→索敌→逼近→随机攻击"状态机 + 按主人技能输入镜像联动——**这是"召唤物独立 AI"缺失档的真实用例**（87 的定性修正不适用于本模式）。手册 §6.3"召唤物独立 AI"缺口保留，本技能 ON 模式为其首个剑士侧立项依据。

**未考证项**
1. 吉格 20 秒寿命的计时器实现（static[2]=20000 无脚本消费方；吉格 PO 寿命疑引擎侧）。
2. ON 模式吉格随机攻击 state 12-15 的伤害列映射（state 12 实测用 col14 鬼影鞭值——随机攻击借用联动技能数值，其余态未逐一核对）。
3. curse 7 参中后 4 参（重复 col8）的精确语义（R2-A10 已记档未解）。
4. swordman_shared.obj etc#31 引用的 summon_zig_start_zigbody.ani 缺失对 ON 登场动画的实际影响。

**新缺口上报（主循环汇总）**
1. **[CLIP] 翻译节**（吉格本体三动画，帧级裁剪窗）：ani 子命令 clip 字段；消费侧可选支持。
2. **技能镜像联动**（"主人施放技能 X 时召唤物同步施放"）：无技能施放事件广播门面——召唤系通用，建议随召唤 AI 立项一并评估。
3. ON/OFF 开关技（seal 系统）：SkillLogic 无"技能级开关状态"（跨施放持久布尔）——需 LSCast 之外的持久存储（如挂 Buff 当标记，现有 ap_zig_character 即此模式，可复用 BuffDefinition 表达）。

**给下轮的经验**：zigadvent 的 ap_scream_\* 七件套全是空壳（只 setBuffIconImage）——**别花时间逐个读**；ON 模式行为全在 `shared_passive_object\swordman\procappend.nut` case 24/25 + `swordman_common.nut` 的 ZigAttackWithChr 两处。
