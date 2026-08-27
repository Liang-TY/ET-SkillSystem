# 连突刺（DashAttackMultiHit）

> 技能ID 11 | 级别 A | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 连突刺 | `DashAttackMultiHit.skl [name]` |
| 英文名 | DashAttackMultiHit（skl 文件名；[name2]=`Thrust` 是本技能的英文俗名，非文件名） | 同上 [name2] |
| 职业 | 鬼剑士共通（growtype 0-5） | 同上 |
| 学习等级 | 5 | 同上 [required level] |
| 最高等级 | 70 | 同上 [maximum level]（level info 实际 70 档，口径一致） |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | (前冲攻击中) X | 同上 [command] / [command key explain] |
| CD | 1000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 8 → 70（Lv1 → Lv70） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 武器效果类型 | physical | 同上 |
| static data | `300 -1000 500 -1000 1`（语义未考证；推断 300/500 为窗口或位移毫秒值，-1000 为无效哨兵） | 同上 [static data] |
| 一句话效果 | 前冲攻击时按攻击键，追加一个突刺攻击；突刺提升命中率并击退敌人 | 同上 [explain] |

**level property（3 列，Lv1 → Lv70）**：col0 `185→1748`、col1 `0→690`、col2 `46→437`。
本技能无 nut 参照（白名单内无同名/同型脚本），列语义按模板向量**推断**（L8）：
- col0 = 攻击倍率%（模板 `<int>%%`，向量 `-1 0 1.0`）；
- col1 = 命中率比率（模板 `<float1>%%`，向量 `-1 1 0.1`——explain 明示"可以增加命中率"）；
- col2 = 固定物攻加成（模板第二个 `<int>`，向量 `-2 2 1.0`，同 UpperSlash col3 型）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**`swordman_load_state.nut` 中无注册行，白名单内无同名 nut**——基础版（技能 11）引擎内置（F3 特征：`swordman_header.nut:181` 定义 `CUSTOM_ANI_DASHATTACKMULTIHIT <- 11`，白名单 grep 实证无引用）。且与鬼斩/上挑不同，**连剑鬼区也没有同型脚本**（grep tripleslash/upperslash/hardattack 之外无 thrust 类）——行为重建只剩 .ani/.atk/.obj 数据两方印证。

相邻注册（本技能的前置动作）：`attack.nut` 的 `swordman_attack`（状态 8，普攻链）与 .chr `[dashattack motion]`（前冲攻击动作）均引擎/共用状态处理。

### 2.2 引擎内置状态行为重建（.ani 攻击盒 + .obj 数据印证）

**onSetState（施法瞬间）——推断**
- 前置：角色处于前冲攻击（dashattack，.chr [dashattack motion] = `DashAttack.ani` 10 帧 630ms，攻击盒 F2-F8）中；
- 按攻击键 → 切 `dashattackmultihit.ani`（etc 槽 11），设攻击信息 `DashAttackMultiHIt.atk`（etc atk 槽 14），扣 MP，起 CD。

**命中判定（dashattackmultihit.ani 实测——与鬼斩不同，本技能 .ani 自带攻击盒）**

| 帧号 | 累计时间 | 攻击盒 | 备注 |
|---|---|---|---|
| F2 | 200ms | ✅（4 帧连续有盒） | 突刺判定窗口 = **F2-F6（200~470ms）** |
| F2-F6 | 200-470ms | ✅ | 帧驱动多窗口（引擎按帧施加，单次命中最可能；"MultiHit"之名或来自多帧盒窗口） |
| F0/F1/F7 | — | 无 | 起手/收招 |

无 SET FLAG（无帧触发标记）。

**被动对象：激光剑气（dashattackmultihitsub.obj 完整实测）**——突刺向前延伸的剑气判定体：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [name] | `激光剑气` | 官方命名 |
| [layer] / [floating height] | normal / 1 | 常规层 |
| [pass type] / [piercing power] | pass all / 1000 | 全穿透 |
| [basic motion] | `Animation/DashAttackMultiHitSub.ani` | 6 帧 425ms；**攻击盒 F1-F4**（偏移+尺寸口径：F1 x∈[92,157] → F4 x∈[95,236]——**向前伸出 2.4 单位的增长光束**，z 高度 51→58 段位） |
| [attack info] | `AttackInfo/DashAttackMultiHitSub.atk` | 物理/damage 反应/push30/lift30/cut+blood 60 0.85/knuck back 1 |
| [object destroy condition] | on end of animation | 播完即毁 |

即完整机制 = **角色突刺（本体判定 200-470ms）+ 前方剑气 PO（穿透延伸判定 100-360ms）双层打击**，两者独立结算（PO 是独立伤害源，L3 同型）。

**onEndCurrentAni（推断）**：回待机。

### 2.3 命中反应（.atk 实测）

- `dashattackmultihit.atk`（本体）：物理/damage 反应（普通硬直）/**push 250 / lift 200**/hit horizon（水平击退）/**[knuck back] 1**（反向击退标记）——explain"能击退敌人"实证。
- `dashattackmultihitsub.atk`（剑气）：物理/damage/push 30 / lift 30/cut+blood（砍切出血表现参数 60 0.85）。
- 前置 `dashattack.atk`（前冲攻击本体）：damage bonus 40/damage 反应/push 50 / lift 80/hit horizon。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/dashattack.ani`（前冲攻击，前置动作） | 10（F0-9） | 630ms（100/80/50/100/60×5/30/30） | 无 | F2-F8 | 前冲攻击本体（.chr [dashattack motion]） |
| `character/swordman/animation/dashattackmultihit.ani`（角色突刺） | 8（F0-7） | 500ms（100/100/60×4/30/30） | 无 | **F2-F6** | 帧驱动判定；仅引 `sm_body%04d.img` |
| `passiveobject/.../dashattackmultihitsub.ani`（剑气 PO） | 6 | 425ms（100/65×5） | 无 | F1-F4 | `thrust_beemsword.img`；光束前伸（盒 x 至 236） |
| `character/swordman/effect/animation/dashattackmultihit1.ani`（下身特效） | 2 | 160ms | 无 | 无 | `DashAttackMultiHit/thrust_under.img`；引擎内置绘制（无引用者） |
| `character/swordman/effect/animation/dashattackmultihit2.ani`（上身特效） | 3 | 240ms | 无 | 无 | `DashAttackMultiHit/thrust_upper.img`；同上 |

`.als` 边车：**无**（两侧 animation 目录 ls 实证）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DashAttackMultiHit.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DashAttackMultiHit.skl` | ✅ 实测 | 技能数据（CD1000/3 列等级数据） |
| lst 条目 | ID 11 | `…\pvf\skill\swordmanskill.lst` 59-60 行 | ✅ 实测 | — |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ **缺失（引擎内置状态）** | 白名单内亦无同型参照脚本（与鬼斩/上挑不同） |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\` | ⛔ 缺失 | 行为靠 .ani 盒 + .obj 数据重建 |
| 常量 | swordman_header.nut 181 行 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_DASHATTACKMULTIHIT=11（无引用者） |
| .chr 条目 | [dashattack motion]（944 行）+ etc motion #11（984 行）；etc attack info #14（1308 行）+ [dashattack info]（1285 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | DashAttack.ani / DashAttackMultiHit.ani / 两个 atk 映射 |
| 角色 .ani | dashattack.ani / dashattackmultihit.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 10 帧 630ms / 8 帧 500ms（盒 F2-F8 / F2-F6） |
| 角色 .atk | dashattack.atk / dashattackmultihit.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 前冲攻击 / 突刺命���反应 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失（本技能无边车） | — |
| PO 定义 | dashattackmultihitsub.obj | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 激光剑气（§2.2 表） |
| PO .ani | dashattackmultihitsub.ani | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 6 帧 425ms，盒 F1-F4 |
| PO .atk | dashattackmultihitsub.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | 剑气命中（push30/lift30） |
| 突刺特效 | dashattackmultihit1/2.ani | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | 引擎绘制（无引用者） |
| 装备层 | dashattack.ani ×76 / dashattackmultihit.ani ×76 | `…\pvf\equipment\character\swordman\avatar\{belt,cap,coat,face,hair,neck,pants,shoes}\*\` | ✅ 实测（find 计数各 76） | 各 avatar 变体图层（只查存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img（帧索引集） | sprite_character_swordman_equipment_avatar_skin.NPK | 角色/前冲攻击动画图集 | 必需（共享） | ✅ `sm_body0000.img.bytes` 已在库 |
| DashAttackMultiHit/thrust_beemsword.img | sprite_character_swordman_effect_dashattackmultihit.NPK | 激光剑气 PO 视觉（若做弹体/区域延伸判定则视觉必需） | 可选（建议做——剑气是本技能辨识度主体，且承担延伸判定视觉） | ❌ 未入库 |
| DashAttackMultiHit/thrust_under.img | 同上 | 突刺下身特效（2 帧） | 可选 | ❌ 未入库 |
| DashAttackMultiHit/thrust_upper.img | 同上 | 突刺上身特效（3 帧） | 可选 | ❌ 未入库 |

必需 img **0 张**；可选 3 张同属一个 NPK（一次提取全覆盖）。

## 5. 实现方案草案

- **内容件清单**：
  - `DotNet~/Skills/DashAttackMultiHitSkill.cs : SkillLogic`——同 `BloodBoomSkill` 范式：
    - `CooldownMs = 1000`（原值直用）；`TotalTimeMs = 500`（动画 8 帧）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanDashAttackMultiHit)` + `ctx.ClearHitTargets()`；F2 时点（200ms）`ctx.CreateBullet(BulletIds.ThrustBeam)`（剑气弹体，见下）。
    - **本体攻击盒走帧驱动**——dashattackmultihit.json 自带 F2-F6 attackBox，LSHitboxComponentSystem 自动激活（releasewave F1/F2 同构），不用 SetAttackHitbox。
    - `OnUpdate`：SubState 守卫下在 elapsed≥200ms 时 `CreateBullet`（帧号 const=2）。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
    - `HitReaction`（static readonly）：`Damage=60 / HitstunMs=400 / KnockbackX=250 / LaunchY=80`——atk 原值 push250/lift200/hit horizon（水平击退为主，Ly 取小值防意外浮空）。
  - `DotNet~/Bullets/ThrustBeam.cs : BulletDefinition`——**复制 `NormalWaveBullet` 改**：`Speed≈0`（贴身出生不飞行，或 Speed 8 短程前推模拟光束增长）、`TotalTimeMs=425`、`DestroyOnHit=false`（穿透，pass all 同构）、`HalfExtents=(1.2,0.35,0.3)`（PO 盒 F1-F4 均值折算：x 半幅 ~1.2 单位）、`HitActions={MeleeHit}`、`HitReaction{Damage=40, HitstunMs=300, KnockbackX=30, LaunchY=30}`（dashattackmultihitsub.atk 原值）、`ViewAnimId=AnimId.ThrustBeam`（thrust_beemsword.json 视图自推帧）。
  - **不需要新 Action**（MeleeHit 现成）。
- **前置动作（前冲攻击）的简化**：demo 无前冲状态机——见 §7，做成独立按键技能。
- **概念映射**：引擎状态 + dashattackmultihit.ani → SkillLogic + AnimId；.ani F2-F6 攻击盒 → 帧驱动盒；激光剑气 PO → `BulletDefinition`（穿透短命弹，WaveSword 同构）；.atk 双层命中 → 技能 HitReaction（本体）+ 弹体 HitReaction（剑气）。
- **注册点清单**：

  | 什么 | 在哪 | 增量 |
  |---|---|---|
  | SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.DashAttackMultiHit = 13` + `ButtonToSkill` case 9 |
  | BulletId | `Packages\cn.etetet.skill\Runtime\BulletDefinition.cs` | `BulletIds.ThrustBeam = 2`（接现有 NormalWave=1 之后） |
  | AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanDashAttackMultiHit=49`、`ThrustBeam=50` |
  | json 注册 | `…\LSAnimClipRegistrar.cs` | `RegisterOne` ×2 |
  | 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | 可选 `thrust_beemsword.img.bytes` 等 3 张 |
  | 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 9 |

- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 1000ms | 1000 |
| 总时长 | 500ms（8 帧） | 500 |
| 本体判定窗 | F2-F6（200-470ms，帧驱动盒） | 直用 json 盒 |
| 本体伤害 | col0 185% + col2 46 固定（Lv1，推断） | MeleeHit 固定 60 |
| 本体命中反应 | push250/lift200/hit horizon/knuck back | Kb250/Ly80/Hitstun400 |
| 剑气伤害 | PO 独立结算（atk push30/lift30） | 弹体 Damage40/Kb30/Ly30 |
| 剑气穿透 | pass all / piercing 1000 | DestroyOnHit=false（穿透） |
| 命中率加成 | col1 0→690（%） | 无命中判定系统 → 忽略（§7） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `DashAttackMultiHit.skl` | `.skl` 尚无子命令（3 列 level info + static data） | 手抄可行；随批量化提级 |
| `dashattack.atk` / `dashattackmultihit.atk` / `dashattackmultihitsub.atk` | `.atk` 尚无子命令 | 手抄（每文件 ~7 值） |
| `dashattackmultihitsub.obj` | `.obj` 尚无子命令 | 本例**单相位**（basic motion + 单 attack info + destroy condition），手映射为 1 个 BulletDefinition 即可（比 GoreCross 多相位 PO 简单） |
| `dashattackmultihit.ani`（角色） | 无缺口（F2-F6 attackBox → `attackBoxes[]` 现有规则直译，帧驱动消费已落地） | — |
| `dashattackmultihitsub.ani` / 特效 ani | `[SHADOW]`（值 0） | 整节跳过无碍（GoreCross 先例） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 施放前置=前冲攻击中按 X（dashattack 状态衔接） | 无前冲/跑步状态机（新缺口，见 §8）；输入条件化无从挂接 | demo 做成独立按键技能（瞬发突刺）；CD 1000ms 本就近似连打节奏 |
| 命中率提升（col1） | 数值系统无命中/闪避判定 | 忽略（demo 全命中） |
| 剑气光束随帧增长（PO 盒 F1→F4 前伸） | BulletDefinition 固定 HalfExtents（无生长） | 固定盒（取末端值 2.4 单位的一半）；视觉上 thrust_beemsword 自推帧自带增长观感 |
| [knuck back] 1（反向击退标记） | 击退方向语义未支持（HitReaction 只有正向 KbX） | 忽略（hit horizon 正向击退即可） |
| cut+blood 60 0.85（砍切出血表现参数） | 无砍切表现系统 | 忽略 |
| 突刺特效 thrust_under/upper 引擎内置绘制 | 延后（无声明式翻译源，GoreCross §8-2 同类） | 先跳过；还原时手组装 overlay |
| 等级缩放 / MP | 延后 | 固定值 / 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. col0/col1/col2 列语义（无 nut 参照，模板向量推断；col1=命中率与 explain 互证，置信较高）。
2. `[static data] 300 -1000 500 -1000 1` 语义（推断含窗口/位移毫秒）。
3. "MultiHit"是否真多段（.ani 连续 4 帧盒可能只是长判定窗；若引擎每帧重置命中表则多段——resetHitObjectList 机制未考证，多段命中本就在延后清单）。
4. 剑气 PO 的出生时点（推断 F2 本体判定同步）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **前冲/跑步攻击状态机**：连突刺依赖"前冲攻击中"的前置状态；我们的移动系统无 dash/跑步攻击衔接（NormalAttack 只有站立三段）。银光落刃（跳跃前置）同属"状态前置型技能"——建议在 01§0.4 表增补"状态前置技能（前冲/跳跃中施放）"行（延后档：demo 全部降级为独立按键施放）。
2. 命中率/闪避判定（col1 数据源已出现，冰波等技能或再有）——数值系统扩展项，记档即可。

**给下轮的经验**：连突刺是**无任何 nut 参照**的引擎内置技（连剑鬼区都没有），只能靠 .ani 攻击盒 + .obj 两方印证——这类技能文档里"推断"标注会多，属正常。PO 命名"sub"后缀（xxxsub.obj）= 命中判定型被动对象（ashenforksub 同型），单相位结构可直接映射 Bullet/Area。
