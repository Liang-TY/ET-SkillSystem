# 里 · 鬼剑术（WeaponCombo）

> 技能ID 67 | 级别 A（纠偏：仍为 A，但本质是"普攻派生连段系统"而非独立大招） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 里 · 鬼剑术 | `WeaponCombo.skl [name]` |
| 英文名 | WeaponCombo（取 skl 文件名；本技能无 [name2]） | 文件名 |
| 职业 | 剑魂（[skill fitness growtype]=1；另 [growtype maximum level] 第 5 位=5，剑影也可习得，上限语义见 §8） | skl + growtype 实测映射（FlowMind=1 剑魂、Kalla=2 鬼泣、bloodyrave=3 狂战、IceWave=4 阿修罗） |
| 学习等级 | 15 | skl `[required level]` |
| 最高等级 | 30（[growtype maximum level] `0 5 0 0 0 5`——剑魂/剑影列上限 5，与 [maximum level] 30 的关系未考证，疑为觉醒段上限） | skl |
| 类型 | active（skill class 1） | skl `[type]` |
| 指令 | 无（[command] 空——由普攻键在剑魂普攻连段中派生触发） | skl |
| CD | 0 ms（dungeon `[cool time] 0 0` 且 `[auto cooltime apply] 0`，施放不起 CD；pvp CD 2000） | skl |
| MP | 6 → 140（Lv1 → Lv30） | skl `[consume MP]` |
| 特殊消耗 | 无（耐久消耗率 14——武器耐久系统我们无，忽略） | skl |
| static data | `1`（dungeon；语义未考证） | skl |
| 一句话效果 | 剑魂特有的鬼剑术：按武器种类切换的多段派生连击（短剑3段/太刀4段/钝器3~4段/巨剑2段/光剑3~4段），攻击力按等级提升（+10%→+590%）；可与普攻形成连击，最后一击后无法再接普攻或里鬼；可强制中断施放其他转职技能 | skl `[explain]` + 实测资源 |

**level property（1 列，Lv1 → Lv30）**：`10 → 590`（每级 +20）。
模板 `[里 · 鬼剑术]攻击力增加率 : <int>%%`——列 0 = 攻击力增加率 %，叠加在普攻倍率之上。
nut 印证：`obj.sq_SetCurrentAttackBonusRate(sq_GetCurrentAttackBonusRate(obj) + obj.sq_GetBonusRateWithPassive(67, 8, 0, 1.0))`
（attack.nut 行 240——技能 67、状态 8、列 0，**列语义有实证**，本技能罕见的确定项）。
pvp 列：`0 → 87`。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无独立注册**（全文件 172 行 grep `weaponcombo|moonlight…` 无命中）。
它寄生在**普攻共用状态 8**上：

```
120: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/attack/attack.nut", "swordman_attack", 8, -1);
  8: IRDSQRCharacter.pushPassiveObj("common_object/share_obj/share_po_swordman_24370.nut", 24370);
```

- 状态 `swordman_attack`（nut 为 `sqr\character\swordman\attack\attack.nut`，399 行）承载**全部普攻与里鬼派生**；
  进入状态时携带的 datas 元素结构 = (substate, ready)：`ready == 67` 即"本次普攻是里·鬼剑术变体"。
- 共享被动对象 **24370**（`sqr\common_object\share_obj\share_po_swordman_24370.nut` **实测为空文件**——行为全在引擎内）。
  该 PO 是全鬼剑士共享的"打击特效/判定体"：bloodboom（229）、百剑 wavemark（47）、里鬼（67）都写包复用它，引擎按包内首 dword（技能ID）分流。**01§4 文档"24370=ap_bloodboom.nut"的说法据此修正为共享 PO**。
- 引擎内置证据链（F3 三方印证之①）：`swordman_header.nut` 常量 `CUSTOM_ANI_WEAPONCOMBOSHORT1 <- 28`…`WEAPONCOMBOLIGHT3 <- 42`，
  与 `swordman.chr [etc motion]` 槽位一一对应（实测：槽 28 = 973+28=1001 行 `Animation/WeaponComboShort1.ani` ✓）。

**重要定性**：本客户端的 attack.nut 已被国内 mod 作者改造（函数名带 `qq506807329` 水印、变量名混淆）���
里鬼剑术的**本尊**（每武器选哪套动画、攻击信息、连段推进、与普攻互锁）是引擎内置；
mod 附加了：倍率应用、短剑动画替换、太刀/短剑特效图层、钝器第 4 段（Blunt4 重挥）、巨剑剑气 PO。

### 2.2 主 nut 逐回调（attack.nut，仅里鬼相关部分）

`onAfterSetState_swordman_attack(obj, state, datas, isResetTimer)`：
```
substate = datas[0]; ready = datas[1];            // 存 var
wavemark_qq...(obj, ...);                          // 百剑印记联动（非本技能）
weaponcombo_qq506807329_swordman_attack(obj, ...)  // 里鬼入口
```

`weaponcombo_qq506807329_swordman_attack`（mod 注入，读 datas 而非混淆参名）：
```
若 ready == 67:
  攻击倍率 += sq_GetBonusRateWithPassive(67, 8, 0, 1.0)      // 列0：+10%~+590%
  按 getWeaponSubType() 分支:
  case 0 短剑: substate 0/1/2
      当前动画替换为 sq_GetAttackAni(1)/(0)/(1)（引擎普攻槽位复用）
      + sq_AddStateLayerAnimation(1, "…weaponcombo/short_new_01/02/03.ani")   // 状态层特效
      + 写包(67, 3, 倍率) 创建 PO 24370 于(0,0,0)   ×substate 0/1
      substate 2 写包(67, 2, 倍率)（末段类型不同）
  case 1 太刀: substate 0/1/2/3 各挂 katana 效果层（ura_katana_eff / katana_new1/2_under/upper），无 PO、不换动画
  case 2 钝器: substate 2 → sq_SetCurrentAnimation(37)[=Blunt3] + atk 36[=Blunt3.atk]
               substate 3 → sq_SetCurrentAnimation(170)[=Blunt4, mod 新增段] + atk 100[=Blunt4.atk]
  case 3 巨剑: substate 0 → 写包(67, 1, 倍率) 创建 PO 24370 于 x=+100（身前 100px 剑气判定）
  case 4 光剑: 无 mod 改动（引擎默认 3 段）
```

`onKeyFrameFlag_swordman_attack(obj, flagIndex)`（里鬼相关）：
```
switch(sq_getGrowType(obj)):
  case 4（阿修罗）: 若 ready==67:
      flag 100 / 101 → obj.resetHitObjectList()        // 多段命中重置（.ani 内配合每攻击帧）
      flag 1 且 substate==3 → sq_CreateDrawOnlyObject(…reslash_blunt03_dust01.ani)  // 钝器4段尘土
  case 1/2/3: 空（剑魂侧的同类处理推断由引擎原生完成——见 §8 存疑①）
```

`onProcCon_swordman_attack`：若 `isSwordSaber(obj)`（光剑）且 substate==2 且帧 ≥5 且按下攻击键
→ 推 datas(3) 重入状态 8（**光剑第 4 段**输入派生；effect 目录实测存在 `weaponcombolight4.ani` 特效佐证）。

`onSetState` case 3/4（ATTACK_BLADESPIRIT4 / UPPERSLASH_BLADESPIRIT）为剑影（bladespirit）普攻变体，与里鬼无关，记档不展开。

### 2.3 被动对象（PO 24370）

- nut 为空文件（实测 `od -c` 仅 3 个 CRLF）——**行为完全引擎内置**，按写包首 dword 分流：
  - `(67, 3, rate)` / `(67, 2, rate)`：短剑各段斩击判定/特效；
  - `(67, 1, rate)`：巨剑身前 100px 剑气；
  - 另有 `(47, …)`（百剑 wavemark）、`(229, …)`（bloodboom 爆炸）等兄弟用法。
- 无 .obj/.act/专属 ani 可读（`passiveobject\character\swordman\` 与 `passiveobject\zrr_skill\newswordman\animation\` 实测无 weaponcombo 条目）。
- **含义**：里鬼的短剑/巨剑打击判定与视觉走引擎内置 PO，pvf 数据侧只有包参数——行为细节不可考证，重建只能按"身前矩形判定 + 斩击特效"近似。

### 2.4 动画关键帧表（角色侧 16 个 .ani 实测，python 帧表提取）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒（DNF px：x y z min/max 混排） | 备注 |
|---|---|---|---|---|---|
| weaponcomboshort1.ani | 7 | 600ms | 无 | **无**（判定在 PO 24370 / mod 换用普攻动画） | 太刀外全武器末帧 ~300ms 收招停顿 |
| weaponcomboshort2.ani | 7 | 600ms | 无 | 无 | 同上 |
| weaponcomboshort3.ani | 6 | 540ms | 无 | 无 | 同上 |
| weaponcomboblade1.ani（太刀1） | 7 | 700ms | F3=100 | F2:`8 -10 18 95 30 91`，F3:`-63 -5 1 140 30 56`，F4:`-87 -5 3 130 30 53` | 末帧 400ms |
| weaponcomboblade2.ani（太刀2） | 6 | 640ms | F3=100 | F2/F3 | |
| weaponcomboblade3.ani（太刀3） | 7 | 700ms | F3=100 | F2/F3/F4 | |
| weaponcomboblade4.ani（太刀4） | 6 | 640ms | F3=100 | F2/F3 | |
| weaponcomboblunt1.ani（钝器1） | 7 | 705ms | 无 | F2/F3 | |
| weaponcomboblunt2.ani（钝器2） | 7 | 655ms | 无 | F1/F2 | |
| weaponcomboblunt3.ani（钝器3） | 7 | 748ms | F2/F3/F4 各=101 | F2/F3/F4（**同动画 3 次命中重置**） | |
| weaponcomboblunt4.ani（钝器4·mod） | 7 | 740ms | F4=1（尘土特效） | F3/F4 | 重挥+尘土 |
| weaponcomboheavy1.ani（巨剑1） | 8 | 660ms | 无 | **无**（判定走 PO 24370 剑气） | |
| weaponcomboheavy2.ani（巨剑2） | 10 | 780ms | 无 | 无 | 同上 |
| weaponcombolight1.ani（光剑1） | 6 | 540ms | F3=65534（取消/命中标记，同 GoreCross 惯例） | **无** | |
| weaponcombolight2.ani（光剑2） | 6 | 540ms | 无 | 无 | |
| weaponcombolight3.ani（光剑3） | 7 | 600ms | 无 | 无 | |

- flag 语义：**100/101 = resetHitObjectList**（多段命中重置，attack.nut 实证）；1 = 特效触发；65534 = 引擎标记（语义未考证，GoreCross 同款）。
- 伤害盒（DAMAGE BOX）每帧 1~4 个（皮肤/护甲层），翻译规则已覆盖。
- 图像全部为 `sm_body%04d.img`（皮肤图集，已入库）。
- `.als` 边车：**blade1-4 与 blunt1-4 共 8 个**（内容见 §6 翻译节）；short/heavy/light 无。

### 2.5 .atk 命中反应（16 个实测；DNF 原值 → 详见 §5 数值表）

全部 physic / weapon damage apply=1 / no element；hit info：太刀 `[cut]+[blood]`，钝器 `[blow]+[no blood]`，短剑/光剑空。
关键差异：末段普遍加重——太刀4/钝器4/光剑2/光剑3/巨剑2 为 down（击倒）反应，短剑3/太刀4/钝器4 带 lift 400（击飞量级），短剑3 为 damage+hit lift up（浮空不倒地）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | WeaponCombo.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\WeaponCombo.skl` | ✅（146 行） | 技能数据（等级/倍率列） |
| 注册行 | attack.nut 状态 8（共用） | `…\pvf\sqr\character\swordman_load_state.nut:120` + `:8`（PO 24370） | ✅ 共用注册 | 普攻/里鬼共用状态 + 共享 PO |
| 主 nut | attack.nut | `…\pvf\sqr\character\swordman\attack\attack.nut` | ✅（399 行） | 里鬼 mod 入口（onAfterSetState → weaponcombo_qq506…） |
| PO nut | share_po_swordman_24370.nut | `…\pvf\sqr\common_object\share_obj\share_po_swordman_24370.nut` | ✅ 但**空文件** | PO 24370 行为引擎内置（C2 定点读取） |
| 常量表 | swordman_header.nut:198-212 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | CUSTOM_ANI_WEAPONCOMBO* = 28~42 |
| .chr 条目 | etc motion #28-42 + etc attack info #27-41 | `…\pvf\character\swordman\swordman.chr:1001-1015 / 1321-1335` | ✅ 实测 | 15 个动画 + 15 个攻击信息（Blunt4 另在 etc motion #170 / etc attack info #100：1143/1394 行，mod 追加段） |
| 角色 .ani | weaponcombo{short1-3,blade1-4,blunt1-4,heavy1-2,light1-3}.ani | `…\pvf\character\swordman\animation\` | ✅ 16 个 | 各武器连段动作 |
| .als | blade1-4/blunt1-4 共 8 个 | 同上目录 | ✅ | 斩击特效叠加（见 §6） |
| 角色 .atk | 16 个 weaponcombo*.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 各段命中反应 |
| 特效 .ani | 57 个（katana_new*/katananew_*/light*/reslash_blunt*/ura_*/short_new_*/weaponcombolight4 等） | `…\pvf\character\swordman\effect\animation\weaponcombo\` | ✅（含 [pvp] 变体 3 个、.als 2 个） | mod 特效图层与引擎特效 |
| 装备层 | weaponcombo* ×1216 个 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,coat,…}\` | ✅（find 计数） | 各 avatar 变体（只查存在性） |
| 关联 | swordman_attack1/2/3（mod 短剑换用） | `…\pvf\character\swordman\animation\attack1-3.ani` | ✅ | 我们侧已有 json（AnimId 12/13/14） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`。所有角色动画用 `sm_body%04d.img`（已入库 ✅）。

**最小 demo（单武器·太刀 4 段，帧驱动攻击盒全齐）缺失 img = 0 张**——本体动画只用 sm_body0000（已在库）。
特效为可选增强，若做全武器还原则需下列（全部 ❌ 未入库）：

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| ura_katana_plus01/02.img | sprite_character_swordman_effect_weaponcombo.NPK | 太刀斩光（katananew_* 系引用） | 可选 | ❌ |
| ura_club.img / ura_club_new.img | 同上 | 钝器斩光 | 可选 | ❌ |
| ura_beam.img / ura_ls.img / ura_ls_wind.img / sword_light_*.img（6张） | 同上 | 光剑/巨剑剑气与辉光 | 可选 | ❌ |
| ura_ss_plus.img | 同上 | 短剑斩光 | 可选 | ❌ |
| regi_frame_blade01/02.img | 同上 | 太刀残影帧 | 可选 | ❌ |
| do_jumpchainattack_ldodge_under/upper.img（2张） | sprite_character_swordman_effect_jumpattackmulti.NPK | 特效 ani 跨技能借用（跳跃连斩） | 可选 | ❌ |
| 02_dust2_normal.img | sprite_character_priest_effect_ripper.NPK | 钝器4段尘土（**跨职业借用圣职者素材**） | 可选 | ❌ |

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/WeaponComboSkill.cs : SkillLogic`**（NormalAttack 先例的多段扩展；**单 cast 内子状态机**，与 DNF 状态 8 的 substate 模型同构）
   - `CooldownMs = 0`（DNF 原值 0）；`TotalTimeMs = 0`（手动控制：各段动画时长累计）；`ManualCooldown` 不需要。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanBlade1)`（demo 取太刀 4 段，因 .ani 自带攻击盒走帧驱动路径），`ctx.ClearHitTargets()`。
   - `OnUpdate` 段表驱动（`GetElapsedMs()` 对照各段起止 + `SetSubState` 单次守卫，bloodboom §4.7-7 同构）：
     - 段边界（太刀，DNF 原时长）：段1 0-700ms → 段2 700-1340 → 段3 1340-2040 → 段4 2040-2680。
     - 每段开始：`ctx.PlayAnim(下一段 AnimId)` + **`ctx.ClearHitTargets()`**（= DNF resetHitObjectList / flag100 同构——已落地 API，不是缺口）。
     - 段间输入窗口：`CurrentFrameIndex() >= 段取消帧 && ctx.PeekBufferedButton() == <普攻键值1>` → 立即推进下一段（提前切段）；不按则播完自动进下一段（DNF 里鬼为自动连段）。
     - 第 4 段结束 `OnEnd`：`ctx.PlayDefaultAnim()`；末段后**不接**普攻/里鬼（explain 互锁语义——TryCast 冷却外另需短锁，demo 用 200ms 静默窗近似或先不做）。
   - 帧驱动攻击盒：太刀 1-4 段 json 自带 attackBoxes（LSHitboxComponentSystem 判定帧表扩条目即可，同 releasewavedash §5.6-6 先例）。
   - 若选短剑/巨剑/光剑（.ani 无攻击盒）：各段 `ctx.SetAttackHitbox(前偏0.9, 半尺寸(0.7,0.3,0.6))` + `DisableAttackHitbox()`（NormalAttack 同款手动盒，近似引擎 PO 判定）。
2. **`WeaponComboBlade34HitReaction` 等静态参数**——每段独立 `static readonly HitReaction`（同技能单类内按段切换；.atk 原值见下表）。
3. **无需**新 Action / Area / Bullet / Buff（MeleeHit 现成；短剑 PO 判定简化为手动盒）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 8 + datas(67) 里鬼变体 | `WeaponComboSkill : SkillLogic`（独立技能类，按键进入） |
| 状态 substate 0..N 连段推进 | 单 cast 内 `LSCast.SubState` + `GetElapsedMs()` 段表（无需 RestartCurrentSkill——SubState 在同 cast 内持久） |
| resetHitObjectList（flag 100/101） | `ctx.ClearHitTargets()` 每段开始调（**已落地**，非延后档——多段命中的"定时自动重置"才是延后档） |
| sq_GetBonusRateWithPassive(67,8,0) 倍率叠加 | demo 固定值（等级缩放延后） |
| 引擎 PO 24370 斩击判定 | `SetAttackHitbox` 手动盒近似（无 PO 数据可考） |
| sq_AddStateLayerAnimation 状态层特效 | 手组装 overlay（releasewave 先例）或跳过 |
| 与普攻互锁 / 强制中断施放其他技能 | 取消体系缺失（同 064-GoreCross §8 缺口①），demo 不做 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.WeaponCombo = 11`（接现有 10 之后顺延）+ ButtonToSkill 普攻键复用判断（J 连打派生或新键） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | 太刀版 4 个：`SwordmanWeaponComboBlade1~4 = 49~52`（段值接 48 顺延；其他武器族按需扩） |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×4（weaponcomboblade1-4） |
| 帧判定帧表 | LSHitboxComponentSystem 判定帧表 | 扩 `{Attack1, …, SwordmanWeaponComboBlade1~4}`（releasewavedash 先例同位置） |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 太刀最小版无新 img（sm_body 已在）；特效版再加 ura_katana_plus01/02 |
| 翻译 | DnfConfigTranslation | 16 个 .ani + 8 个 .als（.als 的 `[create draw only object follow parent]` 节缺口见 §6） |

### 关键数值表（太刀 4 段示范；DNF 原值 + demo 建议值）

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 0 | 0 |
| 段时长 | 太刀 700/640/700/640 ms | 直用（累计 2680ms 总时长） |
| 倍率 | 普攻倍率 + 10%~590%（列0，Lv1~30） | 各段固定 60/70/80/110 |
| 段1 .atk（Blade1） | damage bonus 17 / push 30 / lift 80 / damage 反应 | 伤害 60/硬直 400/Kb 30/Ly 80 |
| 段2（Blade2） | bonus 40 / push 30 / lift 95 | 70/400/30/95 |
| 段3（Blade3） | bonus 61 / push 30 / lift 105 | 80/400/30/105 |
| 段4（Blade4） | bonus 84 / push 80 / **lift 400 / down 击倒** | 110/硬直 700/Kb 80/Ly 400 |
| 攻击盒 | .ani 帧盒（F2-F4 等，见 §2.4） | 帧驱动直译（json attackBoxes） |
| 取消/推进窗口 | flag 100 帧起（F3） | `CurrentFrameIndex() >= 3` 起接受普攻键提前切段 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| weaponcomboblade1/2/3/4.ani.als | **`[create draw only object follow parent]`**（新节：帧号 + 别名 + 偏移xyz，值结构近同 [add]，语义=跟随父对象的绘制对象） | als 子命令按 [add] 同构解析并加 `followParent:true` 字段；游戏侧 LSAnimOverlayViewComponent 的 overlay GO 本就挂单位根下跟随——翻译成 overlay 即可视觉等价 |
| weaponcomboblade2.als | 悬空别名（`blunt01_wind_back` 引用了未注册名——注册名是 `reslash_blunt01_wind_back`， blunt2.als 的笔误） | 工具已有悬空别名���数提示，照常输出、游戏侧跳过（无需改） |
| WeaponCombo.skl | `.skl` 无子命令 | 手抄列0 倍率表可行；随批量化提级（同 064 建议） |
| 16 个 .atk | `.atk` 无子命令 | 本技能 16 文件 × ~8 值，手抄量大——**.atk 子命令优先级建议上调**（里鬼是 .atk 数量最多技能之一） |
| 部分 .ani `[SHADOW]` | 已知整节跳过（064 记档） | 无需处理 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 5 武器 × 3-4 段的分支（引擎内置选段） | 无武器切换系统（缺失档：换装/武器切换） | demo 只做 1 套（太刀，帧盒最全）；其余武器族逐个加（纯内容件复制） |
| 短剑/巨剑/光剑判定走引擎 PO 24370（空 nut，数据不可考） | 行为黑盒（未考证档） | 手动攻击盒近似（NormalAttack 先例），判定范围取太刀帧盒量级 |
| 多段命中重置（flag 100/101 resetHitObjectList） | 定时自动重置延后；但**段间手动 ClearHitTargets 已落地** | 每段开始 ClearHitTargets；同段内多次重置（钝器3 的 3 连）demo 降为单次 |
| mod 特效层 sq_AddStateLayerAnimation + .als 叠加 | 声明式 overlay 已有（.als 可译）；脚本层动画无对应 | 用 .als 译文注册 overlay（太刀族）；short_new_* 手组装或跳过 |
| 里鬼↔普攻连段互锁 + 强制中断进其他技能 | 取消体系缺失（064 §8 缺口① 同源） | demo：里鬼结束回待机；互锁用静默窗近似或不做 |
| 攻击力倍率随等级（+10%~+590%） | 等级缩放延后 | demo 固定段伤害 |
| MP 消耗 / 武器耐久 | 延后（MP 在延后清单；耐久无系统） | 跳过 |
| 光剑第 4 段（onProcCon 输入派生 + weaponcombolight4 特效） | 光剑族 .ani 无攻击盒且第 4 段动画槽未考证 | 光剑不做第 4 段（demo 不含光剑族） |

## 8. 存疑与缺口上报

**未考证项**
1. `onKeyFrameFlag` 的 flag 100/101 处理挂在 `sq_getGrowType` case 4（阿修罗）分支而剑魂分支为空——与 skl fitness=1（剑魂）矛盾。推断：剑魂侧由引擎原生处理，modder 只为其改版对象补了脚本侧；**不排除本 mod 允许阿修罗使用里鬼变体**（mod 常见玩法），待试玩考证。
2. `[growtype maximum level] 0 5 0 0 0 5` 与 `[maximum level] 30` 的关系（疑为各觉醒段上限，同 GoreCross "70（各觉醒段 50）"句式）。
3. `sq_GetAttackAni(0/1)`（mod 短剑换动画）对应的引擎普攻槽位编号语义。
4. PO 24370 包内 subtype（1/2/3）的引擎侧行为差异（判定形状/特效选择）——nut 为空无法读。
5. static data `1` 语义。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **"普攻派生技能"的输入通道**：DNF 里鬼不是独立按键技能而是普攻连段中的派生（按住/连打方向决定走普攻串还是里鬼串）。我们现有输入面只有 button id + PeekBufferedButton 单值，表达"普攻第 N 段中按下技能键切换到里鬼串"这类**派生触发**需要输入状态机层的小扩展（当前 TryCast 在技中直接拒绝）。建议归档：输入系统扩展位。
2. **24370 共享 PO 模式**（一 PO 多技能按首 dword 分流）：bloodboom/wavemark/weaponcombo 三例实证。对我们无直接影响（各技能自带内容件），但**修正了 01§4 的记载**——bloodboom 爆炸实体并非专属 ap_bloodboom，主循环回填 01 文档时注意。

**翻译工具缺口**：`[create draw only object follow parent]` 新节（4 个 .als 实证）；`.atk` 子命令优先级建议上调（16 文件）。
