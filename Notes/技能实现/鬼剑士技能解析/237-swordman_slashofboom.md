# 鬼斩 : 狂怒（swordman_slashofboom）

> 技能ID 237 | 级别 A | 可实现性 ✅（直接，基础版：两相位子状态 + 前方爆炸 Area 全现成；霸体/屏震/普攻中施放降级） | 分析日期 2026-08-22 | 批次 A12

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼斩 : 狂怒 | `skill\Swordman\swordman_slashofboom.skl [name]` |
| 英文名 | swordman_slashofboom（取 skl 文件名；**本 skl 无 [name2] 节**，实测） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype]=2；L17 映射 2=鬼泣；鬼斩系变体） | 同上 |
| 学习等级 | 40（前置：技能 6 HardAttackCharge Lv1） | 同上 [required level] / [pre required skill] |
| 最高等级 | 60（鬼泣段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 3） | 同上 [type] |
| 指令 | ←↑→ + Z（指令 MP 优惠 20%/40%） | 同上 [command] |
| CD | 20000 ms（pvp 起手 20000） | 同上 [cool time] |
| MP | 165 → 1386（Lv1→Lv70） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 可执行状态 | `[executable states] 8 0 14`——普攻(8)/站立(0)/状态 14（**未考证**何状态，疑前冲/移动攻击族）中可施放 | 同上 [executable states] |
| static data | 无 | 同上 |
| 一句话效果 | 剑注鬼神之力正面下斩并引发爆炸，被爆炸命中者受暗属性魔法伤害 | 同上 [explain] |

**level property（2 列模板，Lv1 → Lv70）**：大小比例 = col0：`100`（**恒定 100%**）；爆炸攻击力 = col1：`7857% → 148272%`。
同级技能 238 鬼斩 : 炼狱（`238-swordman_slashofhell.md`）前置正是本技能 237。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
53: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/slashofboom/slashofboom.nut", "swordman_slashofboom", 237, 237);
```

状态号 237 = 技能 ID 237（新式一一对应注册）。mod 中断系统同款映射（ap_swordman_comminterrupt.nut case 2 鬼泣区：`EnableSoften(obj, 237, 237); SetSkillState(obj, 237, 237, [0]);`）。
爆炸 PO 走 **24370 共享打击 PO**（L20；现 pvf 中 24370 已被 mod 替换为分发器，见 2.3）。

### 2.2 主 nut 逐回调（slashofboom.nut，91 行实测；局部变量名被 mod 混淆但结构完整可读）

- `checkExecutableSkill_swordman_slashofboom(obj)`：`sq_IsUseSkill(237)` → IntVect [0] → `sq_AddSetStatePacket(237, STATE_PRIORITY_USER, true)`——进入状态 237 子状态 0。
- `checkCommandEnable`：`sq_GetState()==STATE_STAND` → true；`STATE_ATTACK` → `sq_IsCommandEnable(237)`（普攻中可接=取消，对应 [executable states] 8）；其余 true。
- `onSetState_swordman_slashofboom`：读 vector[0] → `setSkillSubState` + `sq_StopMove()`；攻速系数 ×1.0。分派：
  - **子状态 0（蓄力预备）**：`sq_SetCurrentAnimation(153)`（= `SlashOfBoom_Ready.ani`，.chr etc motion 槽 153，实测 1126 行）。
  - **子状态 1（下斩+爆炸）**：`sq_SetCurrentAnimation(154)`（= `SlashOfBoom_Attack.ani`，1127 行）；随后写包：
    ```
    sq_WriteDword(237);                                    // 技能 ID（24370 分发键）
    sq_WriteDword(sq_GetLevelData(obj, 237, 0, level));    // col0 大小比例（100）
    sq_WriteDword(sq_GetBonusRateWithPassive(237, 237, 1, 1.0)); // col1 爆炸攻击力%
    sq_SendCreatePassiveObjectPacket(24370, 0, 170, 0, 0); // 身前 170px 创建 PO
    sq_SetMyShake(obj, 5, 300);                            // 屏震 强度5/300ms
    ```
- `onEndCurrentAni`：子状态 0 → IntVect [1] 重进状态 237（**切下斩段**）；子状态 1 → `STATE_STAND` 回待机。

时序总纲：**Ready 380ms（全帧霸体）→ Attack 500ms（全帧霸体，段首即创建爆炸 PO 于身前 170px）→ 回待机**；爆炸 PO 独立播 720ms。

### 2.3 被动对象：爆炸判定（现行 pvf = mod 共享 PO 24370 分发；vanilla 数据并存）

**现行生效链**（本 pvf 实测）：PO 24370 → `passiveobject.lst:9-10` = `script_sqr_nut_qq506807329/swordman/qq506807329new_swordman_24370.obj`（"剑圣60"，mod 分发器；行为 nut = `sqr\common_object\share_obj\swordman\setcustomdata.nut` 等 5 件，C2 定点读取）：

```
// setcustomdata.nut case 237（首 dword=技能 ID 分发）
sq_SetCurrentAttackInfoFromCustomIndex(obj, 15);  // → etc attack info #15 = AttackInfo/SlashOfBoomExp.atk
setCurrentAnimationFromCutomIndex(obj, 29);       // → etc motion #29 = Animation/SlashOfBoom/SlashOfBoom_Expl_Ghost.ani
rate = 大小比例/100（=1.0）；setImageRate/攻击盒三轴同步缩放；攻击加成 = col1
```

- **SlashOfBoom_Expl_Ghost.ani**（mod 侧副本，9 帧 720ms）：**F0-F4 有攻击盒**（F0 `-132 -40 -13 281 80 165`、F1-4 `-132 -45 -13 281 90 165` → min/max 口径 x∈[-132,281] y∈[-45,80/90] z∈[-13,165]，≈4.1×1.3×1.8 单位大前方爆炸）；引 `Character/Swordman/Effect/SlashOfBoom/Ghost.img`；.als 叠 5 层（Expl/BlackExpl/BlackDust/Glow + [create draw only object] RingDust）。
- **SlashOfBoomExp.atk**（mod 目录 `…\script_sqr_nut_qq506807329\swordman\attackinfo\`）：magic / **dark element**（暗属性魔法，explain 吻合）/ damage reaction **down** / push **120** / lift **450** / hit direction front / wav R_DARK_SWORD_HIT。

**vanilla 数据**（`passiveobject\character\swordman\slashofboom.obj`，**未在 passiveobject.lst 注册**——引擎内置专用）：basic action = `Action/slashofboom.act`（BASE ghost.ani 9 帧 620ms **无攻击盒** + SUB 4 层 expl/blackexpl/blackdust/glow @层 -3/-4/-5/-6），[attack info] 为空串——vanilla 命中参数不可考，**以现行 mod 链数据为实现依据**。slashofboom/if.ani 为空占位（IMAGE 空路径，L7）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/slashofboom_ready.ani` | 8 | 380ms | 无 | 无 | **全帧 SUPERARMOR**；.als 叠 3 剑光层（swordC@F7/swordA@F0/swordB 全帧，[none effect add]） |
| `slashofboom_attack.ani` | 8 | 500ms | 无 | 无 | **全帧 SUPERARMOR**；.als 叠 3 剑光层（均 F0 起） |
| `…\effect\animation\slashofboom\slashofboom_ready_sworda/b/c.ani` | — | — | — | — | .als 挂接的剑光（img SwordA/B/C） |
| `…\slashofboom_attack_sworda/b/c.ani` | — | — | — | — | 同上 |
| mod `SlashOfBoom_Expl_Ghost.ani`（PO） | 9 | 720ms | 无 | **F0-F4**（数值见 2.3） | + .als 5 层 |
| vanilla `passiveobject\...\slashofboom\ghost.ani` | 9 | 620ms | 无 | 无 | vanilla PO 基础层 |
| vanilla `expl/blackexpl/blackdust.ani` | 9 | 840ms | 无 | — | vanilla PO SUB 层 |
| vanilla `glow.ani` | 5 | 520ms | 无 | — | 同上 |

`.als` 边车：角色 2 个（make/charge 式剑光叠加）+ mod PO 1 个（5 层）——全部为已支持节（[use animation]/[none effect add]/[create draw only object] 后者记档缺口）。
角色 .ani 无 [pvp] 变体。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_slashofboom.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_slashofboom.skl` | ✅ | 技能数据 |
| 注册行 | swordman_load_state.nut 53 行 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 237=技能 237 |
| 主 nut | slashofboom.nut | `…\pvf\sqr\character\swordman\slashofboom\slashofboom.nut` | ✅（91 行，mod 混淆但完整） | 两相位状态机 + PO 写包 + 屏震 |
| .chr 条目 | etc motion #153/#154 | `…\pvf\character\swordman\swordman.chr` 1126/1127 行 | ✅ | Ready/Attack.ani（无 etc attack info 条目——角色无 .atk，判定全在 PO） |
| 角色 .ani | slashofboom_ready/attack.ani（各 +.als） | `…\pvf\character\swordman\animation\` | ✅ | 帧表见 2.4 |
| PO（vanilla） | slashofboom.obj + action\slashofboom.act + animation\slashofboom\*.ani | `…\pvf\passiveobject\character\swordman\` | ✅（obj 未注册 lst） | 引擎内置版数据 |
| PO（mod 生效） | qq506807329new_swordman_24370.obj + animation\SlashOfBoom\*.ani + attackinfo\SlashOfBoomExp.atk | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | 现行爆炸判定链 |
| PO 行为 nut | setcustomdata.nut 等 5 件 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅（C2 定点读） | case 237 分发 |
| 特效 .ani | ready/attack_sworda/b/c、expl 族 7 个 | `…\pvf\character\swordman\effect\animation\slashofboom\` | ✅ | .als 挂接剑光/爆炸 |
| 装备层 | slashofboom_ready/attack.ani | `…\pvf\equipment\character\swordman\avatar\`（belt_a 实测 2 件） | ✅ | 换装图层（demo 不需要） |
| 关联技能 | swordman_slashofhell.skl（238） | `…\pvf\skill\Swordman\` | ✅ | 后继技，另见 238 文档 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集 | 必需（共享） | ✅ 已在库 |
| SlashOfBoom/Ghost.img | sprite_character_swordman_effect_slashofboom.NPK | 爆炸 PO 主层（判定+视觉） | **必需** | ❌ |
| SlashOfBoom/Expl.img / BlackExpl.img | 同上 | 爆炸层 2/3 | **必需**（主视觉） | ❌ |
| SlashOfBoom/SwordA/B/C.img | 同上 | 剑光 .als 层（ready/attack） | 可选 | ❌ |
| SlashOfBoom/BlackDust.img / Glow.img | 同上 | 爆炸尘/辉光层 | 可选 | ❌ |
| ATJihad/RingDust.img | sprite_character_swordman_effect_atjihad.NPK | 环尘（[create draw only object]，跨目录 L14） | 可选 | ❌ |
| （vanilla slashofboom/if.ani 空占位） | — | — | — | — |

缺失 img：必需级 3 张（同 1 个 NPK 一次提取全覆盖）、可选级 5 张（跨 2 个 NPK）。img 版本红线由提取时把关。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/SlashOfBoomSkill.cs : SkillLogic`**（BloodBoomSkill 两段 SubState 范式 + slashofboom.nut 逐句同构）
   - `CooldownMs = 20000`（原值直用）；`TotalTimeMs = 900`（ready 380 + attack 500 + 余量）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSlashOfBoomReady)` + `ctx.ClearHitTargets()`（SubState=0，对应 setSkillSubState(0)）。
   - `OnUpdate`：SubState==0 且 ready 动画播完（帧≥7）→ `ctx.PlayAnim(AnimId.SwordmanSlashOfBoomAttack)`、**同帧** `ctx.CreateAreaInFront(AreaIds.SlashOfBoom, (FP)17/10)`（PO 出生 170px 直译；DNF 在 Attack 段 onSetState 即建 PO）、SubState=1。
   - `OnEnd`：`ctx.PlayDefaultAnim()`（onEndCurrentAni → STATE_STAND 同构）。
2. **`DotNet~/Areas/SlashOfBoomArea.cs : AreaDefinition`**（BloodBoomArea 范式）
   - `TotalTimeMs=720`（Expl_Ghost 动画时长）、`TickTimeMs=0`、`EnterActions={MeleeHit}`、
     `HalfExtents=(41/10, 135/100, 89/100)`（盒 F1 x[-132,281] 半宽 2.05+中心前移由 CreateInFront 距离补、y[-45,80]、z[-13,165] 半高 0.89）、
     `HitReaction{Damage=140, HitstunMs=800, KnockbackX=120, LaunchY=450}`（SlashOfBoomExp.atk 原值 down/push120/lift450——lift450+down 反应=击倒浮空爆，releasewave §5.6-3 同构手感）、
     `ViewAnimId=AnimId.SlashOfBoomExplGhost`（Expl_Ghost 译 json，.als 5 层 demo 注册 Ghost+Expl+BlackExpl 3 层）。
3. 无需新 Buff/Action（MeleeHit 现成）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 237 子状态 0/1（setSkillSubState） | LSCast.SubState 段机（`ctx.GetSubState/SetSubState`） |
| onEndCurrentAni 段切换 | `OnUpdate` 帧号 const（7）+ 一次性守卫 |
| sq_SendCreatePassiveObjectPacket(24370,0,170,0,0) | `CreateAreaInFront(AreaIds.SlashOfBoom, 1.7)` |
| PO 大小比例 col0（100%→三轴同步缩放） | demo 固定 100%（等级缩放延后） |
| SlashOfBoomExp.atk（magic/dark/down/push120/lift450） | Area `HitReaction`（暗属性以直伤体现） |
| sq_SetMyShake(5,300) | 屏震延后（开关已有，效果未实现） |
| sq_StopMove / 攻速 ×1.0 | 移动锁不做（现状同其他技能）；攻速无系统 |
| [executable states] 8/0/14（普攻/站立/14 中可放） | 独立施放（技能取消体系缺失） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `…\Runtime\SkillIdAttribute.cs` | `SkillIds.SlashOfBoom = 22` + 按键 case |
| AreaId | `…\Runtime\AreaDefinition.cs` | `AreaIds.SlashOfBoom = 12`（A12 段） |
| AnimId | `…\npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanSlashOfBoomReady=92`、`SwordmanSlashOfBoomAttack=93`、`SlashOfBoomExplGhost=94`（特效层可选 95-97：swordA/B/C 剑光 .als） |
| json/图集/按键 | LSAnimClipRegistrar / LSAnimResComponentSystem / LSOperaComponentSystem | 3+ json（含 2 个 .als overlay json）；图集 3 张必需 img；新按键 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000ms | 20000 |
| 总时长 | ready 380 + attack 500 = 880ms（PO 另 720ms） | TotalTimeMs 900 |
| 爆炸触发 | Attack 段首（onSetState 即建 PO） | Attack 切换帧同步 CreateArea |
| PO 出生位 | 身前 170px | CreateAreaInFront 1.7 |
| 爆炸伤害 | col1 7857%（Lv1）→148272%（Lv70，魔法基数） | MeleeHit 固定 140 |
| 爆炸反应 | down/push120/lift450/hit front（dark） | Kb120/Ly450/Hitstun800 |
| 爆炸盒 | F1 x[-132,281] y[-45,80] z[-13,165] ×大小比例 | HalfExtents (2.05+前偏, 0.63, 0.89) |
| 活动帧 | PO F0-F4（0-370ms） | Area 一次 Enter（TotalTimeMs 720 视觉对齐） |
| 屏震 | 强度5/300ms | 不做 |
| 霸体 | 两段全帧 SUPERARMOR | 不做 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| mod `slashofboom_expl_ghost.ani.als` 的 `[create draw only object]` | 无 follow 后缀变体（R2-A10 记档，本技能第 4 例） | als 子命令按 [add] 同构支持；游戏侧"仅绘制对象"处理 |
| 角色 2 .als 的 `[none effect add]` | **已支持**（README 规则表） | 无缺口 |
| PO/effect .ani 的 `[GRAPHIC EFFECT]`（expl 族 LINEARDODGE）、`[INTERPOLATION]`、`[IMAGE RATE]` | GRAPHIC EFFECT 已支持（L15）；[IMAGE RATE] 延后记档；**[INTERPOLATION] 不在任何清单——本批上报（060 §8 同条）** | 跳过无碍逻辑 |
| `.skl` / `.act` / vanilla `.obj` / mod `.atk` | 尚无子命令（累计记档） | 本技能手抄 ~12 值可行 |
| 各 .ani 的 `[SHADOW]` / `[DAMAGE TYPE] SUPERARMOR` / `[SET FLAG]` | 记档跳过（本技能角色 .ani 实无 SET FLAG，PO 亦无） | 无缺口 |

结论：本技能唯一实质翻译缺口 = `[create draw only object]` 无后缀变体（既有记档追加实证）；.ani/.als 其余全可译。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 两段全程霸体（SUPERARMOR 全帧×2 动画） | 霸体帧延后 | 不做（蓄力 380ms 内可被打断） |
| 屏震 sq_SetMyShake(5,300) | 屏震延后（开关已有） | 跳过 |
| 暗属性魔法伤害 | 元素属性系统缺失 | 直伤固定值 |
| 爆炸大小随 col0 缩放（三轴同步） | 对象整体缩放延后（IMAGE RATE 同族） | 固定 100%（col0 原值本就恒 100，**实��无损**） |
| 普攻/状态14 中可施放 | 技能取消体系缺失（064 上报） | 独立施放 |
| PO 活动帧 F0-F4（370ms）判定、动画播 720ms | Area 一次 Enter 结算 | 保持 TotalTimeMs=720 对齐视觉（判定即入即结，与 DNF 首帧命中差异极小） |
| 音效（R_DARK_SWORD_HIT 等） | 延后 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. [executable states] 第 3 值 `14` 的状态语义（疑前冲/移动攻击族；未在 load_state 72 条 pushState 内定位到 14 号注册）。
2. vanilla slashofboom.obj 的命中参数（[attack info] 空 + ghost.ani 无攻击盒——vanilla 判定不可考；本档以现行 mod 链 SlashOfBoomExp.atk 为实现依据）。
3. 攻速系数 ×1.0（onSetState 显式设置）与 DNF 默认的关系（疑为 mod 模板残留）。
4. mod 24370 分发器的 etc motion/atk 索引配对（本技能 29/15 由 setcustomdata.nut case 237 实证；其余技能索引配对不在本批范围）。
5. 效果层 slashofboom_expl_ringdust.ani 引用 ATJihad/RingDust.img 的跨目录复用关系（已按 img 路径推 NPK，L14 惯例）。

**新系统级缺口上报**：无新增（霸体/屏震/取消体系/元素属性均已在缺口累计中；[create draw only object] 为翻译工具既有缺口第 4 实证）。
