# 极 · 神剑术 (破空斩)（swordofmind）

> 技能ID 234 | 级别 A | 可实现性 🔶（蓄力按住/武器五分支/标记追踪简化） | 分析日期 2026-08-22 | 批次 A15

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极 · 神剑术 (破空斩) | `skill\Swordman\swordman_swordofmind.skl [name]` |
| 英文名 | swordofmind（取 skl 文件名去前缀，skl 存剑 = swordman_swordofmind） | 同上 |
| 职业 | 剑魂 · 剑神（二觉 85 系；极·神剑术系列） | 同上 [basic explain] + 常识 |
| 学习等级 | 75 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30） | 同上 [maximum level] / [second growtype maximum level]（索引 3 = 30） |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | →←→ + Z（指令施法 MP 优惠 50%/50% 档） | 同上 [command] / [skill command advantage] |
| CD | 35000 ms | 同上 [cool time] |
| MP | 580 → 4500 | 同上 [consume MP] |
| 特殊消耗 | 道具 3037 × 3 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14（攻击中可取消接技） | 同上 [executable states] |
| 一句话效果 | 蓄力后极速斩击标记范围内敌人（先僵直），随后共享 PO 对全部被标敌人每 120ms 多段连打 + 终结一击，掌握[极·神剑术]时最后在血量最高的被标敌人处引发爆炸；五种武器各有附加效果 | 同上 [explain] + PO 走读 |

**static data**（dungeon）：`1500` = 蓄力时间上限 ms（向量 `(0,0,0.001)` 对位）。

**level info（20 列，Lv1）**：col0 多段攻击次数 5、col1 物理攻击力 2448%、col2 爆炸攻击力 8176%、col3-5 钝器眩晕 100%/Lv87/4.5s、col6-10 光剑感电 100%/87/2399/2.0s/+1 段、col11-15 太刀出血 53%/87/943/2.4s/+1 段、col16-17 短剑吸附 190px/200%、col18-19 巨剑蓄气追加 0.2s/+10%。
**level property 向量**（21 个）：`-4`/`-6` 两个**新源值**首见（-4 标记出血���击力、-6 标记感电攻击力，均在 template 末段）——按两文件交叉（本文件与 meteorsword 同构）推断仍读 level info 对应列、仅为"绝对值（非 %）"显示语义标记，未考证。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
40: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/swordofmind/swordofmind.nut", "swordman_swordofmind", 234, 234);
 8: IRDSQRCharacter.pushPassiveObj("common_object/share_obj/share_po_swordman_24370.nut", 24370);   // 共享打击 PO（本 pvf 为 mod 实现）
 9-13: sq_RunScript("common_object/share_obj/swordman/setcustomdata|setstate|procappend|onendcurrentani|else.nut")   // 24370 的六回调
```

- `swordman_header.nut`：`STATE_SWORDMAN_SWORDOFMIND <- 234`（66 行）、`CUSTOM_ANI_SWORDMAN_SWORDOFMINDCHARGE/CHARGEATTRACT/CHARGEATTACK <- 141/142/143`（311-313 行）、`CUSTOM_ATTACK_SWORDMAN_SWORDOFMINDATTACK <- 91`（482 行）。
- .chr 对位（0 基）：etc motion #141/142/143 = 1114/1115/1116 行（`SwordOfMindCharge/ChargeAttract/ChargeAttack.ani`，行号-973 ✓）；etc attack info #91 = 1385 行 `AttackInfo/SwordOfMindAttack.atk`（行号-1294 ✓，064 偏移公式复验）。
- **24370 对象定义 = mod 版**：`passiveobject.lst:9-10` 指向 `script_sqr_nut_qq506807329/swordman/qq506807329new_swordman_24370.obj`（"剑圣60"，实测存在，155 行 UTF-8 文本）——L20"引擎内置"结论在本 pvf 被 mod 替换为**数据 + 共享回调 nut**实现，本技能因此得以完整走读。
- ⚠ 主 nut `swordofmind.nut`（252 行）被 mod 混淆（变量名乱码，C3 同族），逻辑已人工还原（下文）。

### 2.2 主 nut 逐回调（swordofmind.nut，还原）

**checkExecutableSkill**：使用 → 推 (subState 0, weaponSubType) 进状态 234。**weaponSubType 分支实证**（写包列号 ↔ 模板武器组对位）：**0=短剑（吸附组 col16/17）、1=太刀（出血组 col11-15）、2=钝器（眩晕组 col3-5）、3=巨剑（蓄气组 col18/19）、5=光剑（感电组 col6-10）**。

**onSetState subState 0（蓄力段）**：
- 短剑（subType 0）：`sq_AttractToMe(obj, col16=190, 190, col17=200%)`——蓄力期吸附敌人（���回 appendage 记录，onEndState 失效）；播 ani 142 ChargeAttract（620ms）。
- 其余武器：播 ani 141 Charge（620ms，无攻击盒）。
- 挂 `ap_swordofmind.nut`（蓄力光环视觉三段：charging → charge_fin → charge_repeat，按朝向镜像）；建 draw-only `charge_spin_eff.ani`（底层循环，跟随）。
- time 向量：[static 1500ms 上限, col18 蓄气追加(仅巨剑), col19 蓄力增伤%(仅巨剑)]。

**onProcCon subState 0**：计时 ≥ 1500(+col18) **或松开技能键** → subState 1，蓄力系数 = uniform(0→col19, over col18)——即增伤只产生于巨剑的 0.2s 追加窗（0~+10% 线性）。

**onSetState subState 1（斩击/标记段）**：
- 播 ani 143 ChargeAttack（660ms，**F1/F2 攻击盒** `30 -50 -27 / 349 100 125` → x∈[30,379] y±50 z[-27,98]）；设攻击信息 **#91 SwordOfMindAttack.atk：reaction `none` + `stuck -10000`**——标记斩不产生受击反应、以超长 stuck 把敌人**定在原地**（"记录敌人之前会先进入僵直状态"的实现）。
- 闪屏(0,120,180,102) + 震 4/150 + 音 SM_GUE_FLEETNESS。
- 写包创建 PO 24370：`[234, weaponSubType, 段数=col0(+1 光剑/太刀), col1×(1+蓄力), col2×(1+蓄力), 武器特效 3~4 参, 极·神剑术(stateoflimit)掌握 bool]`。

**onAttack（本体斩击每次命中）**：把命中敌人 push 进 PO 的标记列表（obj_vector）+ `newhit.ani` 命中视觉（pooled，位于敌身高中点）。

**onEndCurrentAni subState 1** → 回站立。**onEndState**：摘除蓄力 appendage、失效吸附 appendage、RemoveAllAni。

### 2.3 共享 PO 24370（mod 版，case 234 三相位）

| 阶段 | 触发 | 行为 | 攻击信息（mod obj 0 基 [etc attack info] 直读实测） |
|---|---|---|---|
| 待机 | 创建后 200ms（procappend） | → state 10 | — |
| state 10 多段 | timeEvent **每 120ms** | 先给全部标记敌人挂 `ap_swordofmind_attack.nut`（受击烟雾视觉，normalhit/dunhit 随机 6 选 1）；每拍对**全部标记敌人** `sq_SendHitObjectPacket`（无视位置直接命中） | **#9 SwordOfMind.atk**：physic/weapon、damage、push 100、lift 50、knuck back -1 |
| 终结击 | 段数计数归零拍 | 换攻击信息再打一轮全部标记敌人 + `last_hit.ani` 视觉 + 摘烟雾 appendage | **#10 SwordOfMindExp.atk**：physic/weapon、**down**、push 400、lift 300、hit down |
| state 11 爆炸 | 掌握[极·神剑术]（stateoflimit appendage 在身）时：找**HP 最高**标记敌人 → 传送到其位置 | 动画 **#19 cross_eff.ani**（etc motion 直读）+ 闪屏 + 震 7/150；播完销毁。未掌握 → 直接销毁 | **#11 SwordOfMindThirdPhase.atk**：physic/weapon、**down**、push 0、lift 150、hit down |

武器特效写入：钝器 → 三份 atk 全写 `ACTIVESTATUS_STUN`(col3/4/5)；光剑 → `LIGHTNING` 4 参（col6-9，含独立感电攻击力）；太刀 → `BLEEDING` 4 参（col11-14）——L6 状态附加链路同构。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/swordofmindcharge.ani`（蓄力，.chr #141；磁盘名小写） | 14 | 620ms | 无 | 无 | .als 挂 charge_attack_base_ready.ani（**该文件两侧目录均不存在——悬空别名**）；仅引 sm_body |
| swordofmindchargeattract.ani（短剑蓄力，#142） | 14 | 620ms | 无 | 无 | .als 挂 charge_pull.ani @帧2 层10003 |
| swordofmindchargeattack.ani（斩击，#143） | 13 | 660ms | 无 | **F1/F2**（见 §2.2） | .als：sword @帧1 层10002 + dash @帧1 层10003（[add]）+ **dust_front/back 走 `[create draw only object]` 无后缀变体**（帧2 + 偏移三元组） |
| effect/animation/swordofmind/：charge_spin_eff、charging、charge_fin、charge_repeat、attack_sword、attack_dash、attack_dust_front/back、charge_pull、newhit、last_hit、cross_eff（+attack_casting、charge、charge_ani、hit、marking_1/2/3——后五组 nut 无引用者，疑引擎内置原版残留，未考证） | — | — | — | — | 全部纯视觉 draw-only；节名实测仅 FRAME/LOOP/SHADOW 常规 |
| marking_2.ani.als | — | — | — | — | [use animation]+[add] 常规可译 |

（PO 侧 `passiveobject\character\swordman\animation\swordofmind\` 存在同构镜像副本一套，引用以 character/effect 路径为准。）

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_swordofmind.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_swordofmind.skl` | ✅ 实测 | 20 列等级数据 |
| 注册行 | swordman_load_state.nut 行 40 / 8-13 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 234 + PO 24370 及其回调 |
| 主 nut | swordofmind.nut | `…\pvf\sqr\character\swordman\swordofmind\swordofmind.nut` | ✅ 实测（252 行，mod 混淆已还原） | 蓄力/斩击/标记 |
| 蓄力视觉 ap | ap_swordofmind.nut | `…\swordofmind\ap_swordofmind.nut` | ✅ 实测 | charging→fin→repeat 三段光环 |
| 受击烟雾 ap | ap_swordofmind_attack.nut | `…\swordofmind\ap_swordofmind_attack.nut` | ✅ 实测 | 标记敌人身上 6 选 1 烟雾 |
| PO 回调 | common_object/share_obj/swordman/{setcustomdata,setstate,procappend,onendcurrentani,else}.nut 的 case 234 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅ 实测（C2：注册行直指） | 多段/终结/爆炸节拍器 |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ 实测 | etc motion #19=cross_eff、atk #9/10/11 对位表 |
| PO atk | SwordOfMind.atk / SwordOfMindExp.atk / SwordOfMindThirdPhase.atk | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ 实测 | 多段/终结/爆炸 |
| .chr 条目 | etc motion #141-143（1114-1116 行）+ etc attack info #91（1385 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 三动画 + 标记斩 atk |
| 角色 .ani | swordofmind{charge,chargeattract,chargeattack}.ani（+各 .als） | `…\pvf\character\swordman\animation\` | ✅ 实测 | 620/620/660ms |
| 角色 .atk | SwordOfMindAttack.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 标记斩（none + stuck -10000） |
| 特效 .ani | swordofmind/ 21 个 + normalhit/、dunhit/ | `…\pvf\character\swordman\effect\animation\swordofmind\` | ✅ 实测 | 全视觉层 |
| 关联被动 | ap_stateoflimit（掌握判定） | `…\pvf\sqr\character\swordman\appendage\ap_stateoflimit.nut` | 引用实证（未细读） | 极·神剑术掌握 buff |
| 装备层 | 未查 | `…\pvf\equipment\...` | 未查 | sm_body 单图集即可（L16） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 三段角色动画 | 必需（共享） | ✅ |
| `…/SwordOfMind/sword.img`、`dash.img` | sprite_character_swordman_effect_swordofmind.NPK | 斩击剑光/冲刺光（.als [add] 层） | **必需** | ❌ |
| `…/SwordOfMind/last_hit.img` | 同上 | 终结击视觉 | **必需** | ❌ |
| `…/SwordOfMind/cross_eff.img` | 同上 | 爆炸十字光 | **必需** | ❌ |
| `…/SwordOfMind/charging.img`、`charge_fin.img`、`charge_repeat.img`、`spin_eff.img`、`newhit.img`、`marking1.img`、`marking2.img`、`casting.img`、`pull.img` | 同上 | 蓄力光环/标记/命中/吸附等 | 可选 | ❌ |
| `Character/Priest/Effect/DarkHowling/soul _normal.img` | sprite_character_priest_effect_darkhowling.NPK | 跨职业借用（具体引用 ani 未定位，存疑） | 可选（存疑） | ❌ |

缺失 img：**必需 4 张、可选 10 张**（含 1 张跨职业）——必需全在一个 NPK（swordofmind）内，一次提取。

## 5. 实现方案草案

**结构映射**：斩击标记 = 帧驱动攻击盒（ChargeAttack 自带盒）；多段 = Tick 无去重 Area（L19/R2-A8 实证：同段定时多段可直接表达）；终结/爆炸 = 顺序 Area。

### 内容件清单

1. **`DotNet~/Skills/SwordOfMindSkill.cs : SkillLogic`**（BloodBoom SubState + releasewave 帧盒范式）
   - `CooldownMs = 35000`；`TotalTimeMs = 900`（斩击 660ms + PO 200ms 延迟起拍）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSwordOfMindAttack)` + `ctx.ClearHitTargets()`（蓄力段不做——按住输入缺失，§7）。
   - `OnUpdate`（时间驱动，SubState 单值推进）：
     - t ≥ 200：`ctx.CreateAreaInFront(AreaIds.SwordOfMindMultiHit, (FP)2)`（多段区，斩击盒 x 前沿 ~3.5 单位的中点）；SubState=1。
     - t ≥ 200+600（5 段 × 120ms 拍完）：`ctx.CreateAreaInFront(AreaIds.SwordOfMindFinish, (FP)2)`（终结区）；SubState=2。
     - （可选第三段）t ≥ 1050：索敌 HP 最高敌人（054 同款 SubState 存 id 重查法）→ `ctx.CreateArea(AreaIds.SwordOfMindBlast, 敌位置)`；demo 可省（无掌握被动系统）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/SwordOfMindMultiHitArea.cs : AreaDefinition`**
   - `TotalTimeMs = 600`、**`TickTimeMs = 120`**、`TickActions = { MeleeHit }`（Tick 无去重 = 5 段独立结算，L19 R2-A8 档）；
   - `HalfExtents = (1.8, 0.5, 0.8)`（斩击盒 x[30,379]/y±50 折算半尺寸）；
   - `HitReaction { Damage = 60, HitstunMs = 400, KnockbackX = 100, LaunchY = 50 }`（SwordOfMind.atk 原值；stuck -10000 定身近似为长硬直——受击者不走不动即"钉在原地"）；
   - `ViewAnimId = AnimId.SwordOfMindNewHit`（标记命中视觉循环，可选）。
3. **`DotNet~/Areas/SwordOfMindFinishArea.cs : AreaDefinition`**（BloodBoomArea 单次范式）
   - `TotalTimeMs = 300`、`TickTimeMs = 0`、`EnterActions = { MeleeHit }`；
   - `HitReaction { Damage = 150, HitstunMs = 800, KnockbackX = 400, LaunchY = 300 }`（SwordOfMindExp.atk：down/push400/lift300）；
   - `ViewAnimId = AnimId.SwordOfMindLastHit`、可选 `ViewEndAnimId = AnimId.SwordOfMindCross`（爆炸视觉并入终结收尾，替代第三段）。
4. **无新增 Buff/Action**（MeleeHit 复用；眩晕/感电/出血武器特效不做——武器分支缺失）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 234 两子状态（蓄力/斩击） | 蓄力砍掉（瞬发），斩击 = PlayAnim + 帧驱动盒 |
| weaponSubType 五分支 | 武器类型差异化缺口（R2-A6）→ 不做，取无分支基础版 |
| 标记斩 stuck -10000（定身） | 斩击 HitReaction.HitstunMs 800 近似 + 多段区钉位 |
| 标记列表 + 120ms 直发命中包 | Tick 无去重 Area（敌人被硬直钉在区内 ≈ 标记集合，手感差异小） |
| 终结击（atk10 down/push400/lift300） | 第二个 Area 单次结算 |
| 掌握被动 → HP 最高标记敌处爆炸 | 索敌定位第三 Area（可选）——被动系统缺失，默认直接给 |
| 蓄力按住 + 巨剑追加 0.2s/+10% | 按住输入缺失（缓冲只有按下沿）→ 瞬发 |
| 短剑吸附（AttractToMe） | 位移他人门面缺失（R2-A8）→ 不做 |
| ap 视觉三件（光环/烟雾/last_hit） | Buff 视觉挂接缺口 + draw-only 无通道 → Area ViewAnimId 承担主干 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SwordOfMind = 21` |
| AreaId | `Runtime\AreaDefinition.cs` | `SwordOfMindMultiHit = 10`、`SwordOfMindFinish = 11` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanSwordOfMindAttack = 80`、`SwordOfMindSword = 81`、`SwordOfMindDash = 82`、`SwordOfMindLastHit = 83`、`SwordOfMindCross = 84`、`SwordOfMindNewHit = 85`（可选） |
| json 注册 | `LSAnimClipRegistrar.cs` | 斩击 + sword/dash 层（.als overlay）+ last_hit/cross ×5~6 |
| 图集 | `LSAnimResComponentSystem.cs` | sword.img/dash.img/last_hit.img/cross_eff.img（必需 4） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 35000 ms | 35000（直用） |
| 蓄力上限 | 1500ms（+巨剑 200ms 追加） | 不做蓄力（瞬发） |
| 斩击窗 | ChargeAttack F1/F2（60~180ms），盒 x[30,379]y±50z[-27,98] | 帧驱动直译 |
| 多段 | 5 段 × 120ms（光剑/太刀 +1） | Tick 120ms × 5 |
| 多段伤害 | level col1 = 2448%（每段） | 60/段 |
| 多段反应 | SwordOfMind.atk：damage/push100/lift50 | Hitstun 400/Kb 100/Ly 50 |
| 终结 | SwordOfMindExp.atk：down/push400/lift300 | Damage 150/Hitstun 800/Kb 400/Ly 300 |
| 爆炸 | ThirdPhase：down/push0/lift150；col2 = 8176% | Damage 200/Hitstun 800/Ly 150（可选段） |
| 总时长 | 蓄力可变 + 斩击 660 + PO 200+600+120+爆炸 | 900（不含可选爆炸） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_swordofmind.skl | `.skl` 无子命令 | 手抄 20 列（本技能列多，skl 子命令收益大） |
| 4 份 .atk（角色 1 + PO 3） | `.atk` 无子命令 | 手抄（每份 ~8 值）；`stuck`/`blow` 字段纳入 atk 立项输入 |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | 本档已给 #9/10/11/#19 对位表，手工映射 |
| swordofmindchargeattack.ani.als | **`[create draw only object]`**（无 "follow parent" 后缀变体；帧号独行 + `别名 x y z` 三元组） | R1-A4/066 已录家族；建议 als 子命令按 [add] 同构支持（帧号+别名+偏移三元组，缺层号列） |
| swordofmindcharge.ani.als | 悬空别名 `charge_attack_base_ready.ani`（文件两侧均不存在） | 工具现有行为（计数提示+游戏侧跳过）即可，非缺口，记档 |
| 各 .ani 的 [SHADOW]/[LOOP] | 已知族 | 无新缺口 |

结论：���口 `.skl`/`.atk`/`.obj` 族共性 3 条 + `[create draw only object]` 变体 1 条，计 4 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 按住技能键蓄力（松手/超时发动，巨剑追加窗增伤） | 输入缓冲只有按下沿，无"按住"读取 | 瞬发（蓄力视觉/增伤全砍；巨剑玩家损失最大，注明） |
| 五种武器分支（吸附/多段+1/眩晕/感电/出血/蓄气） | 武器类型差异化缺失（R2-A6 首撞） | 无分支基础版；异常附加可后续用 HitReaction.ProcBuffId 按武器预设近似 |
| 标记列表逐敌直发（无视位置的定点连打） | 无"标记集合"实体；Area 进出检测是位置语义 | 硬直钉位 + 同位置 Area（敌人不移动时零差异；击退会推出区——多段 atk push100 有推力，DNF 靠 stuck 抵消，我们硬直期间不位移？硬直只锁输入不锁位移）→ **多段区 Kb 建议降 0~50 缓解推出** |
| 终结击/爆炸对"标记集合"结算 | 同上 | 同心 Area 近似 |
| 掌握[极·神剑术]才追加爆炸 | 被动/掌握系统缺失 | demo 直接给第三段（或砍掉） |
| 短剑蓄力吸附 | 位移他人门面缺失（R2-A8） | 不做 |
| stuck -10000 超长定身 | 无 stuck 机制 | HitstunMs 800 近似（时长内钉位） |
| 闪屏/震/音效 | 延后 | 跳过 |
| 乱码混淆源码 | mod 污染（C3 族） | 本档已还原语义，实现以本档为准 |

## 8. 存疑与缺口上报

- **未考证**：①`-4`/`-6` level 向量源值语义（推断"绝对值显示标记"，两文件同构互证）；②effect 目录中 attack_casting/charge/charge_ani/hit/marking_1-3 五组无 nut 引用者（疑引擎内置原版路径残留）；③`Character/Priest/.../soul _normal.img` 的具体引用 ani；④onSetState 末尾 `sq_SetStaticSpeedInfo`（攻速静态化）的副作用细节。
- **L20 修正补充**：本 pvf 的共享打击 PO 24370 **不是纯引擎内置**——`passiveobject.lst` 被 mod 指到 `script_sqr_nut_qq506807329/swordman/qq506807329new_swordman_24370.obj`（存在）+ `common_object/share_obj/swordman/*.nut` 六回调（load_state 9-13 行 sq_RunScript 注册）。凡引用 24370 的技能（血爆/百剑/里鬼/破空斩/流星落/236/239…）**行为走读应直接读这套回调**，勿再按"引擎内置不可考"处理。建议主循环把该目录加入白名单（本档按 C2 注册行直指定点读取完成）。
- **新翻译变体实证**：`[create draw only object]` 值格式为"帧号独行 + `别名 x y z`"（与 [create draw only object follow parent] 不同列）——als 立项时的格式输入。
- **给下轮的经验**：剑神系（234/235/236/239/百剑/雷鸣）共用 24370 且**动画/atk 全在 mod obj 的 [etc motion]/[etc attack info] 0 基直读**（本档 #19 cross_eff、#9/10/11 已验证无错位——**该表与 F5 swordman_shared.obj 不同，无 -2 问题**）；多段节拍全在 `else.nut` 的 timeEvent case，段数/间隔一看便知。
