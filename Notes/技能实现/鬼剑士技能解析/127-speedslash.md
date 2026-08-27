# 鬼连斩（speedslash）

> 技能ID 127 | 级别 A | 可实现性 ✅（基础三连斩） | 分析日期 2026-08-22 | 批次 A5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼连斩 | `skill\Swordman\ghostsword\speedslash.skl [name]` |
| 英文名 | speedslash（skl 文件名；本 skl 无 [name2] 节） | 同上实测 |
| 职业 | 剑影（[skill fitness growtype]=5；5_ghostsword 目录） | 同上 |
| 学习等级 | 15 | 同上 [required level] |
| 最高等级 | 60（各觉醒段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ←↓→ + Z（指令施放 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 5000 ms | 同上 [cool time] 5000 5000 |
| MP | 20 → 210（Lv1 → Lv60） | 同上 [dungeon][consume MP] |
| 特殊消耗 | 无 | 同上 |
| 前置 | 技能 123（鬼影剑 BladeSpirit）Lv1 | 同上 [pre required skill] |
| static data | `100`（= 斩击范围 100%，nut 实证：`sq_GetIntData(obj, SKILL_SPEEDSLASH, 0)` 作攻击盒缩放） | 同上 + speedslash.nut |
| 一句话效果 | 连续斩击前方敌人 3 次造成物理伤害；在鬼步准备姿势下按技能键可发动特殊功能（鬼步终结动作变更为鬼连斩终结动作并对命中敌人额外适用鬼连斩攻击力） | 同上 [explain] |

**level property（3 列，Lv1 → Lv60）**：`516→4104`、`619→4966`、`929→7484`（+52/+63/+95 每级）。
模板：第1/2/3次斩击攻击力 `<int>%%`（3 列一一对应）+ 斩击范围 `<int>%%`（static 100）。
nut 实证取用：PO 攻击信息切换时 `sq_GetBonusRateWithPassive(SKILL_SPEEDSLASH, -1, col, 1.0)`，col=0/1/2 对应三段（见 §2.3）。

**等级外联动（攻速）**：动画播放速度 × `SpeedRate = 1 + 鬼影剑(123) 列0`（普通段）/
`SpeedRateEx = 1 + 列1`（上挑段）——前置被动 123 提供动画加速（speedslash.nut onSetState，实测）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
155: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/speedslash/speedslash.nut", "speedslash", STATE_SPEEDSLASH, SKILL_SPEEDSLASH);
18:  IRDSQRCharacter.pushPassiveObj("shared_passive_object/po_swordman_shared.nut", 24349);
```

- 状态名 `speedslash`（STATE_SPEEDSLASH=112，swordman_header.nut:44 实测），nut = `sqr\character\swordman\5_ghostsword\speedslash\speedslash.nut`（270 行）。
- 判定体 **共用被动对象 24349**（unclebang 共享 PO，7 行壳脚本按 id 分派到 `sqr\shared_passive_object\swordman\*.nut`）；
  对象定义 `passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj`（passiveobject.lst:29375 实测映射）。
- 鬼连斩用到的 PO 子 id：**42**（三连斩本体）/ **41**（鬼步联动终结）/ 43、44（上挑强化段）。

### 2.2 主 nut 逐回调（speedslash.nut）

**onSetState（按子状态分派，setSkillSubState 后 sq_StopMove）**
| 子状态 | 动画（.chr etc motion 槽位） | 动作 | 创建 PO |
|---|---|---|---|
| 0 | CUSTOM_ANI_SPEEDSLASH1=277（speedslash1.ani） | 音效 R_SM_SPEEDSLASH_01；动画速度×SpeedRate | 写包 dword **42** → CreatePassiveObject(24349, 0,0,0,0) |
| 1 | CUSTOM_ANI_SPEEDSLASH2=278（speedslash2.ani） | ×SpeedRate | dword **43**（上挑段，需强化技 118） |
| 2 | CUSTOM_ANI_SPEEDSLASH_CONTACT=279（speedslash_contact.ani） | ×SpeedRateEx；计算 xDistance = 现位置 + **鬼步(126) static[0]=400px** 存 var spiritMove | dword **41**，出生偏移 300px（鬼步联动终结） |
| 3 | CUSTOM_ANI_SPEEDSLASH2（复用） | ×SpeedRateEx | dword **44**（联动后的追击上挑） |

**onKeyFrameFlag（子状态 0 的三个 flag → 纯视觉特效 + 音效 + 屏震）**
- 10001：`als_ani` 播 `effect/animation/speedslashbs/speedslashbs11_00.ani`（z=-80，缩放=斩击范围%）。
- 10002：播 `speedslashbs21_00.ani`（z=-60）。
- 10003：音效 R_SM_SPEEDSLASH_02 + 播 4 组特效：`speedslashbs3effecta_00`（z=-90）、`speedslashbs3effectb_00`（z=0）、
  `speedslashbs3bodynew_00/01/02/04/05`（z=90，剑影残像）。
- 子状态 1/3 的 flag 10001/10002：上挑特效 speedslashupperneweffect 系列 + 音效 + `sq_SetMyShake(5, 300)` 屏震（强化段，本技能 demo 可不做）。

**onProc（子状态 2）**：`sq_GetUniformVelocity(起点x, xDistance, currentT, 500)` → `sq_MoveToNearMovablePos` —— 
500ms 内匀速突进 400px（与鬼步共用逻辑，撞墙可止，见 §7）。

**onProcCon（子状态 1/3）**：动画帧 ≥7 时调 `whiteGhostSlshContact(obj)`——若已学白鬼一闪（trigger 列4>0）且按其键，
立即扣 MP/起 CD 并切 STATE_WHITEGHOSTSLASH（跨技能取消，见 §7/§8）。

**onEndCurrentAni（子状态流转）**
- 0 →（学得强化技 118 鬼连斩上挑 SKILL_SPEEDSLASHUPPER）→ 子状态 1；未学 → 回站立。
- 1 → 回站立。
- 2 →（学得 118）→ 子状态 3；未学 → 回站立。
- 3 → 回站立。
即：**基础版只有子状态 0**（三连斩）；118 是追加第 4 段上挑的 TP/强化技；子状态 2/3 是"鬼步联动"专用入口
（鬼步准备姿势下按鬼连斩键 → `GhostSwordSetState(obj, 127, [2], STATE_SPEEDSLASH)` 直接进入子状态 2，
跳过本体三连斩——jg_swordman_common.nut:745 实测）。

### 2.3 被动对象（24349 子 id 42/41，`sqr\shared_passive_object\swordman\` 各回调）

**setCustomData id=42（三连斩本体）**：
- 动画 = 对象 [etc motion] 槽 51 = `character/swordman/effect/animation/speedslashbs/speedslash1_attack.ani`
  ——**空占位动画**（26 帧全空 IMAGE，L7 同型），只做"与角色动画同步的时间轴 + 攻击盒"载体，不可见；
- 攻击盒尺寸缩放 = 本技能 static[0]=100%（`sq_SetAttackBoundingBoxSizeRate`）。

**onkeyframeflag id=42（伤害核心——按 PO 自身动画的 SET FLAG 切换攻击信息）**：
| PO 帧 | flag | 攻击信息（.chr etc attack info 槽） | 伤害 |
|---|---|---|---|
| F3（150ms） | 10001 | CUSTOM_ATTACK_INFO_SPEEDSLASH1=149（speedslash1.atk） | `sq_GetBonusRateWithPassive(127,-1,col0)` = 516%+ |
| F9（440ms） | 10002 | 槽 150（speedslash2.atk）+ **resetHitObjectList**（重置命中表→第 2 段可再打） | col1 = 619%+ |
| F13（680ms） | 10003 | 槽 151（speedslash3.atk）+ resetHitObjectList | col2 = 929%+ |

PO 动画自带逐帧攻击盒（speedslash1_attack.ani，"偏移+尺寸"格式）：
第 1 段帧 `-56 -35 0 285 70 162`、第 2 段帧 `-76 -35 0 305 70 183`（第 3 段同域，实测 grep）。

**setCustomData id=41（鬼步联动终结段）**：动画 = 槽 50 `spiritmove/spiritmovedasheffect_00.ani`（可见，
15 帧全帧攻击盒 `-200 -60 0 400 120 200`）；攻击信息 = 对象表 30（`attackInfo/spiritmove.atk`）；伤害 = **鬼步(126) 列0**；
多段 = 动画总时长/`鬼步 static[2]=3`（900/3=300ms 定时 resetHitObjectList）——即联动终结段的伤害是鬼步的多段伤害，
鬼连斩本体攻击力"额外适用"的部分未见数据通道（explain 与实现差异，§8 存疑）。

**onattack id=41~44**：命中时 `GhostSword_Attack_Effect`——在受击者身上随机播放剑影命中特效
（`common/hiteffect/animation/swordghosthiteffect/...`，纯视觉）。
**onendcurrentani**：动画播完即销毁（`sq_SendDestroyPacketPassiveObject`）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/speedslash1.ani`（角色本体，槽 277） | 26 | 1170ms | F3=10001, F9=10002, F13=10003 | 无（判定在 PO） | 触发 §2.2 的视觉特效；引 sm_body 图集帧 33-37/211-237 |
| `character/swordman/animation/speedslash2.ani`（槽 278，上挑段） | 22 | 760ms | F0=10001, F5=10002, F6/F7=10003 | 无 | 强化技 118 段用 |
| `character/swordman/animation/speedslash_contact.ani`（槽 279） | 6 | 360ms | 无 | 无 | 鬼步联动终结动作；**含 [SPECTRUM] 残影节**（term 40/life 300/白色 180 半透明） |
| `effect/animation/speedslashbs/speedslash1_attack.ani`（PO id42） | 26 | 1140ms | F3/F9/F13 同上 | 判定帧 F3-5/F9-11/F13-17 | **空占位**（不可见），伤害盒载体 |
| `effect/animation/speedslashbs/speedslash2_attack.ani`（PO id43/44） | 22 | 760ms | F0/F5/F6/F7 | F5-8 | 空占位（上挑段） |
| `effect/animation/speedslashbs/speedslashbs11_00.ani` 等 5 组特效 | 未逐帧统计 | — | — | 无 | 引 .als 边车叠 1-5 层（§3） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | speedslash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\speedslash.skl` | ✅ | 技能数据 |
| 注册行 | swordman_load_state.nut:155 / :18 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态注册 + PO 24349 注册 |
| 主 nut | speedslash.nut | `…\pvf\sqr\character\swordman\5_ghostsword\speedslash\speedslash.nut` | ✅（270 行） | 四子状态机 |
| 常量 | swordman_header.nut:44/149/448-450/540-543 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | STATE 112/SKILL 127/ANI 277-279/ATK 149-152 |
| PO 壳 | po_swordman_shared.nut | `…\pvf\sqr\shared_passive_object\po_swordman_shared.nut` | ✅（7 行） | 按回调分派到 swordman/*.nut |
| PO 逻辑 | setcustomdata/onkeyframeflag/onattack/onendcurrentani/ontimeevent.nut | `…\pvf\sqr\shared_passive_object\swordman\` | ✅ | id 42/41 分支（§2.3） |
| PO 定义 | swordman_shared.obj | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\swordman_shared.obj` | ✅（166 行） | etc motion 槽 50/51/52 映射 |
| 联动公共 | jg_swordman_common.nut（SpiritMoveContact/GhostSwordSetState/whiteGhostSlshContact） | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅（load_state:3 pushScriptFiles 引入） | 鬼步→剑术联动/白鬼取消 |
| .chr 条目 | etc motion #277-279（1248-1252 行）/ etc attack info #149-152（1443-1446 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | 动画与攻击信息映射 |
| 角色 .ani | speedslash1/2/contact.ani | `…\pvf\character\swordman\animation\` | ✅ | 见 §2.4 |
| 角色 .atk | speedslash1/2/3.atk、speedslashupper.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 三段命中反应（upper 段属强化技 118） |
| PO .atk | attackInfo/spiritmove.atk（对象表 30） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\attackInfo\spiritmove.atk` | ✅ | 联动终结段命中反应 |
| 特效 .ani | speedslashbs1_00/21_00/3effecta_00/3effectb_00/3bodynew_00~05 | `…\pvf\character\swordman\effect\animation\speedslashbs\` | ✅ | 三段挥砍视觉 |
| .als 边车 | speedslashbs11_00.ani.als / bs21_00.als / bs3effecta_00.als / bs3effectb_00.als（共 4 个） | 同上目录 | ✅ | 每组特效再叠 1-5 子层（[none effect add]/[add]） |
| 装备层 | speedslash*.ani ×608 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 各 avatar 变体图层（含 VS 系技能文件） |

## 4. 资源需求

img → NPK 规则：`sprite_<img所在路径下划线化>.NPK`。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（%04d 图集，帧 33-37/200-203/211-241） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色本体动画 | **必需** | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` |
| SpeedSlashBS/BSSpeedSlash04.img | sprite_character_swordman_effect_speedslashbs.NPK | 第 1/2 段挥砍特效主层（bs11/bs21/3effecta 共用） | 可选 | ❌ |
| SpeedSlashBS/BSSpeedSlash03.img | 同上 | 第 3 段特效 b 层 | 可选 | ❌ |
| GhostSlashBS/GhostSlashBS05.img | sprite_character_swordman_effect_ghostslashbs.NPK | 第 3 段剑影残像（3bodynew_00）/ 鬼步终结特效共用 | ��选 | ❌ |
| BladeSpirit/001.img、002.img | sprite_character_swordman_effect_bladespirit.NPK | 第 3 段剑影残像（3bodynew_01/02/04/05） | 可选 | ❌ |
| SpiritMove/SpiritMove02.img | sprite_character_swordman_effect_spiritmove.NPK | 鬼步联动终结段 PO 视觉 | 可选（做联动段才需要） | ❌ |

缺失 img：必需 0 张（本体动画图集已在库）；可选 6 张（分属 4 个 NPK）。**最小实现（三连斩本体）零新增 img。**

## 5. 实现方案草案

### 内容件清单（基础版=子状态 0，全部继承真实基类）

1. **`DotNet~/Skills/SpeedSlashSkill.cs : SkillLogic`**（ gorecross/bloodboom 范式：帧号 const + SubState 一次性守卫）
   - `CooldownMs = 5000`（DNF 原值直用）；`TotalTimeMs = 1170`（speedslash1.ani 26 帧总时长）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanSpeedSlash1)` + `ctx.ClearHitTargets()`。
   - `OnUpdate` 三段触发（帧号 const 3/9/13，对应 150/440/680ms；SubState 0→1→2→3 递进）：
     - 每段：`ctx.ClearHitTargets()`（DNF resetHitObjectList 同构）+ `ctx.SetAttackHitbox(前偏, 半尺寸)`
       ——盒取 PO 动画判定帧实测值：第 1 段 `(-56,-35,0)+(285,70,162)` → 前偏 ~0.9、半尺寸 (1.4,0.35,0.8)；
       第 2 段 `(-76,-35,0)+(305,70,183)` → 半尺寸 (1.5,0.35,0.9)；第 3 段同域略放大。
     - `HitActions = { MeleeHit }`（命中走本技能 HitReaction）。
     - 末段后 `ctx.DisableAttackHitbox()`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - 三段 HitReaction 不同（atk1/2/3 仅 push/lift 微差 50/150、50/200、75/170）——**demo 建议统一一档**
     （见下方数值表）；若要逐段精确，可在帧 9/13 各 `ctx.CreateAreaInFront` 一个一次性小 Area
     （Area 自带 HitReaction，BloodBoom 范式）替代第 2/3 段的手动盒——二选一，推荐前者（简单）。
2. **需要新增的 Action/Buff/Bullet/Area**：无（MeleeHit 现成）。
3. 不做（记档 §7）：强化技 118 上挑段（子状态 1/3）、白鬼取消、鬼步联动段（子状态 2）、攻速联动（动画 Speed 层已有
   `LSAnimComponent.Speed`，但内容层无门面——按"等级数值缩放"同类延后）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 112 + speedslash1.ani | `SpeedSlashSkill : SkillLogic` + `AnimId.SwordmanSpeedSlash1` |
| PO 24349 id42 空 PO + 其动画 flag 切 atk | 帧号 const（3/9/13）+ SubState 守卫 + SetAttackHitbox（判定帧数据在 PO ani，我们直接放技能类 const） |
| resetHitObjectList（段间重置） | `ctx.ClearHitTargets()`（帧驱动路径同样消费 HitTargets 去重表） |
| 三份 .atk | 单一 `HitReaction`（统一档）或分段 Area |
| als_ani 特效 + .als 边车 | 视觉层：`AnimOverlayConfig`（effect ani 翻译后挂技能动画 overlay）或暂跳过 |
| [SPECTRUM] 残影 | 无对应系统（§7 记档） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.SpeedSlash = 11` + `ButtonToSkill` 新按键（如 M） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanSpeedSlash1 = 49`（接现有 48 之后顺延；特效层做几个加几个） |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne(swordman_speedslash1.json)`（+可选特效 json） |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 无新增（sm_body 已在库）；做特效时加 speedslashbs 图集 |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 7 |
| 翻译 | DnfConfigTranslation ani/als 子命令 | 角色 1 个 + 特效 5 组（含 4 个 .als）json |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 5000ms | 5000（直用） |
| 总时长 | 1170ms（26 帧） | 1170 |
| 三段触发帧 | F3/F9/F13 = 150/440/680ms | 同（帧号 const） |
| 第 1 段 | atk1：damage 反应/push 50/lift 150；伤害 516%+ | 伤害 80/硬直 500/Kb 50/Ly 150 |
| 第 2 段 | atk2：push 50/lift 200；619%+ | 伤害 95/硬直 550/Kb 50/Ly 200 |
| 第 3 段 | atk3：push 75/lift 170；929%+ | 伤害 140/硬直 600/Kb 75/Ly 170 |
| 攻击盒（第1段） | PO 帧 `-56 -35 0 285 70 162` | 前偏 0.9、半尺寸 (1.4,0.35,0.8) |
| 音效/屏震 | R_SM_SPEEDSLASH_01/02；上挑段 sq_SetMyShake(5,300) | 跳过（延后档） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `speedslash.skl` | `.skl` 无子命令（3 列 level info + static） | 手抄可行；建议后续加 `skl` 子命令（与 064-GoreCross 上报同条） |
| 6 个 `.atk`（speedslash1/2/3/upper + PO spiritmove.atk） | `.atk` 无子命令 | 手抄（每文件 ~8 值）；随批量化提级 |
| `speedslash_contact.ani` | **`[SPECTRUM]`（含 TERM/LIFE TIME/COLOR/EFFECT 子节）不在翻译规则表** | 新缺口：建议 ani 子命令加 `spectrum` 字段（term/life/rgba packed）；
  游戏侧消费=拖影渲染系统（当前无，先跳过整节） |
| `speedslashbs/*.ani` 各空 IMAGE 帧 | 现有规则可处理（path=""） | 无需改（空白帧） |
| 4 个 `.als`（bs11/bs21/3effecta/3effectb） | 全部为 `[use animation]`/`[none effect add]`/`[add]` | **现有 als 子命令全覆盖**，无缺口 |
| PO 定义 `swordman_shared.obj` | `.obj` 无子命令 | 本技能不需直译（空占位 PO，判定数据已在上面手抄）；与 064 上报的 obj 子命令建议合并 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 强化技 118 追加上挑段（子状态 1/3，双 hit resetHitObjectList + speedslashupper.atk down 反应） | 属另一技能（118，不在本批）；跨技能条件流转现有框架无（Buff 查询门面缺失） | demo 只做基础三连斩；上挑段随 118 强化批次处理 |
| 白鬼一闪取消（onProcCon 帧≥7 白鬼键 → 切状态 + 手动扣 MP/起 CD） | **技能取消体系缺失**（064-GoreCross §8 已上报同条） | 不实现 |
| 鬼步联动终结（鬼步姿势按鬼连斩键 → 直接进子状态 2：突进 400px + PO41 鬼步多段伤害） | 跨技能状态跳转 + 鬼步本体（126）联动；多段命中（resetHitObjectList 定时）在延后档 | 本技能 demo 不做；联动版在 126-SpiritMove 文档统一设计 |
| 鬼影剑(123) 攻速加成（动画 setSpeedRate） | 内容层无动画速度门面（延后，等级缩放同类） | 固定 1.0 |
| 挥砍特效 5 组（als_ani + .als 叠层） | 视觉层：overlay 机制已有，但 als_ani 是运行时按 flag 动态播（非 .als 边车挂接） | 翻译成独立 json 后按帧号 const 手动挂 overlay（releasewave 手组装先例），或先跳过 |
| [SPECTRUM] 拖影（speedslash_contact） | 无拖影渲染系统（缺失档新例） | 跳过 |
| 音效/屏震 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. explain 宣称"鬼步命中敌人**额外适用鬼连斩攻击力**"——PO id41 数据包（dword 41）只带鬼步列 0 伤害，
   鬼连斩攻击力的附加通道未见（可能引擎结算或与正式服实现有差异）。
2. `speedslash2_attack.ani` 与角色 speedslash2.ani 帧号差 0（同步）；PO id44 的 size×2 精确语义（整盒还是仅图像）未细究。
3. 强化技 118（SKILL_SPEEDSLASHUPPER）的 skl 不在本批（lst 中存在性未查）。

**新系统级缺口（§6.3 清单外）**
1. **[SPECTRUM] 拖影/残影**：DNF 通用视觉节（term/life/color），冲刺类动作常见——建议记"缺失档"：
   需 AnimFrameData/技能级 spectrum 字段 + 视图层拖尾渲染（保留最近 N 帧半透明快照）。
2. **als_ani 运行时特效播放**（区别于 .als 边车）：nut 在 flag 帧调 `als_ani(obj, ani路径, …, z, size, …)` 动态播特效，
   参数含 z 层/尺寸/速率——翻译管不到（是行为不是资源）。建议：惯例化为"帧号 const → 注册的特效 AnimId"映射，
   在技能/区域视图侧手动挂（releasewave 8 层手组装先例已趟通，不需新框架，但要立命名约定）。

**翻译工具缺口**：`[SPECTRUM]` 节（新）；`.skl`/`.atk`/`.obj` 子命令（与既有上报合并，不重复计新）。
