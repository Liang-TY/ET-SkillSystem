# 冥炎剑（MINGYANJIAN）

> 技能ID 249 | 级别 A（预分类 A，维持；预分类备注"狂战二觉系"**纠偏为鬼泣系**） | 可实现性 ✅（五段纯自身连斩，零 PO 零新机制；暗元素/魔法属性为标注级差异） | 分析日期 2026-08-22 | 批次 A13

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冥炎剑 | `skill\Swordman\MINGYANJIAN.skl [name]` |
| 英文名 | MINGYANJIAN（取 skl 文件名；无 [name2]。资源层真名 = **DarkFlameSlash**，见 §2.1） | 同上 |
| 职业 | **鬼泣**（[skill fitness growtype]=2，L17——**批次预分类备注"狂战二觉系"纠偏**；Kalla 卡洛系） | 同上 |
| 学习等级 | 45 | 同上 [required level] |
| 最高等级 | 50（growtype 段 2=50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class **3**——本批四技中唯一 3，语义未考证） | 同上 [type] / [skill class] |
| 指令 | →→ + Z | 同上 [command] |
| CD | 45000 ms（pvp **300000** + 开场 300000——5 分钟级，二觉技待遇） | 同上 [cool time] |
| MP | 110 → 924 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 2 | 同上 [consume item] |
| 可施放状态 | 0/14/8 | 同上 [executable states] |
| 前置 | 技能 75（EpidemicRasa 瘟疫之罗刹）Lv1（lst 实测对位；卡洛本尊 = 技能 82 Kalla） | 同上 [pre required skill] + swordmanskill.lst 263-266 行 |
| 属性 | [weapon effect type] magical；atk 侧 magic + **dark element（暗属性）** | skl + 4 份 .atk |
| static data | **空**（dungeon [static data] 无值） | 同上 |
| 一句话效果 | 召唤凝聚卡洛之力的冥炎剑，以二刀流动作向前突进连续攻击敌人 | 同上 [explain] |

**level info（1 列，Lv1 → Lv50）**：col0 技能攻击力 1863→15156（level property 模板 `技能攻击力 : <float1>%%` + 向量 `(-1,0,1.0)` 实证；nut 四个攻击态全读 col0，同源同证）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
122: IRDSQRCharacter.pushState(0, "character/swordman/guiqi/mingyanjian/mingyanjian.nut", "MINGYANJIAN", STATE_MINGYANJIAN, SKILL_MINGYANJIAN);
```

- `swordman_header.nut`：`STATE_MINGYANJIAN <- 249`（80 行）、`SKILL_MINGYANJIAN <- 249`（109 行）——**状态号=技能号**（249 是新式注册，与老技能小状态号不同）；`CUSTOM_ANI_MINGYANJIAN_READY/ONE/TWO/THREE/END <- 171/172/173/174/175`（341-345 行）、`ATTACKINFO_MINGYANJIAN_ONE/TWO/THREE/END <- 101/102/103/104`（491-494 行）。
- .chr 0 基对位实测：etc motion **#171-175** = `DarkFlameSlash_Ready/Atk1/Atk2/Atk3/Finish_Body.ani`（1144-1148 行）；etc attack info **#101-104** = `DarkFlameSlash_Atk1/2/3/Finish.atk`（1395-1398 行）。**动画/atk 资源真名是 DarkFlameSlash**——MINGYANJIAN 只是 skl/nut 层拼音名，按技能名搜资源会落空（本批重要分族结论）。
- **无被动对象**（`passiveobject\character\swordman\` 无 darkflame/dfs 条目，实测 ls+grep；卡洛实体 kallafire/kallashadow*.obj 属技能 82 Kalla，本技不引用——nut 全读无 CreatePassiveObject）。

### 2.2 主 nut 逐回调（mingyanjian.nut，115 行全读，原版未混淆）

五子状态顺序链（var "state" 0→1→2→3→4→待机）：

- `checkExecutableSkill`：`sq_IsUseSkill(249)` → 进状态 249 subState 0。
- `onSetState`（每段）：
  - **0 蓄势**：播 #171 `darkflameslash_ready_body.ani`（480ms，无攻击盒）；`sq_SetStaticSpeedInfo` 攻速默认 1.0。
  - **1/2/3 连斩**：播 #172/173/174（300/300/600ms）+ `sq_SetCurrentAttackInfo(101/102/103)`（角色直伤，无 PO）+ `sq_SetCurrentAttackBonusRate(sq_GetBonusRateWithPassive(249, 249, 0, 1.0))`——**四段共用 col0**。
  - **4 终结**：`sq_SetMyShake(5,400)` 屏震 + 播 #175（480ms）+ atk 104 + col0。
- `onEndCurrentAni`：0→1→2→3→4→STATE_STAND。全链总时长 **480+300+300+600+480 = 2160ms**。
- **无 onKeyFrameFlag/onProc/onProcCon**——攻击判定完全由动画攻击盒帧驱动 + atk 参数，位移未在脚本出现（explain 的"突进"疑为动画表现或引擎小位移，**未考证**）。

### 2.3 动画关键帧表（帧表 C1 法实测）

| 动画（.chr 槽） | 帧数 | 总时长 | SET FLAG | 攻击盒（min/max, px） | 对应 atk |
|---|---|---|---|---|---|
| `darkflameslash_ready_body.ani`（#171） | 7 | 480ms | 无 | 无 | —（纯蓄势，.als 三层剑焰） |
| `…atk1_body.ani`（#172） | 5 | 300ms | **F3=1**（消费者未考证，疑音效/特效） | F2/F3：≈281×90×165（x∈[-34,247]） | Atk1.atk |
| `…atk2_body.ani`（#173） | 5 | 300ms | **F2=1** | F1/F2/F3：≈289×90×117 | Atk2.atk |
| `…atk3_body.ani`（#174） | 10 | 600ms | 无 | F2-F6 ×5 帧：≈328×90×198（x∈[-70,258]） | Atk3.atk |
| `…finish_body.ani`（#175） | 8 | 480ms | 无 | F0-F3 ×4 帧：≈394×90×133（x∈[-54,340]） | Finish.atk |

**攻击盒是帧驱动主判定**（角色侧 min/max 格式）——atk1/2 短盒快斩、atk3 长盒×5 帧、finish 最宽盒×4 帧，判定范围逐段扩大。

### 2.4 命中反应（.chr etc attack info #101-104，四份全读）

| atk | 反应 | push | lift | 其它 |
|---|---|---|---|---|
| DarkFlameSlash_Atk1.atk | damage | 30 | 30 | magic/dark element/hit down/blow/blood 10 2.0/DARKFLAME_SLASH_HIT |
| DarkFlameSlash_Atk2.atk | damage | 30 | 30 | 同上 |
| DarkFlameSlash_Atk3.atk | **down** | 40 | **300** | hit **lift up**（第三段挑空） |
| DarkFlameSlash_Finish.atk | **down** | 70 | **300** | hit lift up（终结挑飞收尾） |

即手感：两段轻推平斩 → 第三段挑空 → 终结大范围挑飞。

### 2.5 .als 边车（五份全实测）

每段 body 动画各一册 .als，全部 `[use animation]` + `[none effect add]` 标准节：
- ready：DFSReady_Sword_01-03 三层（帧 1/4/全帧）；
- atk1：DFSAtkABody_Sword_01-03 + 两道刀光（dfsatka01_slash_01_dod/dfsatkb01_slash_02_doda）；
- atk2：DFSAtkBBody_Sword_03 + 刀光/烟雾；atk3：Slash_03_Dod/Nor + Last_Eye/Eff ×2 + Sword_02（挑空爆发）；finish：Slash_04_DodA/B + NorA/B + Last_Eff ×2（终结爆发）。
- 特效真源 = `character\swordman\effect\animation\DarkFlameSlash\`（40 文件含 boom\ 子目录 6 个爆炸特效，全引 Kalla 图集）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MINGYANJIAN.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MINGYANJIAN.skl` | ✅ 实测 | CD/MP/1 列 level info/level property |
| 注册行 | swordman_load_state.nut 行 122 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | guiqi\mingyanjian\mingyanjian.nut，状态 249=技能 249 |
| 主 nut | mingyanjian.nut | `…\pvf\sqr\character\swordman\guiqi\mingyanjian\mingyanjian.nut` | ✅ 实测（115 行全读，原版） | 五子状态链 |
| .chr 条目 | etc motion #171-175（1144-1148 行）+ etc attack info #101-104（1395-1398 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | DarkFlameSlash 五动画 + 四 atk |
| 角色 .ani/.als | darkflameslash_{ready,atk1,atk2,atk3,finish}_body.ani + .als ×5 | `…\pvf\character\swordman\animation\` | ✅ 实测 | 五段动作 + 特效边车 |
| 角色 .atk | DarkFlameSlash_Atk1/2/3/Finish.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测（四份全读） | 各段命中反应 |
| 被动对象 | —（无） | `…\passiveobject\character\swordman\` | ⛔ 本技无 PO（kalla*.obj 属技能 82） | — |
| 特效 | DFS* 系 40 文件 + boom\ ×6 | `…\character\swordman\effect\animation\DarkFlameSlash\` | ✅ 实测（目录 ls） | 剑焰/刀光/爆炸视觉 |
| 关联技 | Kalla.skl（82）/ kalla.nut（**mod 混淆**，变量名乱码实证）/ kalla{fire,shadowair,shadowland}.obj | `…\skill\Swordman\Kalla.skl` 等 | ✅ 存在（未细读） | 前置体系卡洛本尊；本技不直接依赖 |
| 装备层 | darkflame 系 ×380 | `…\pvf\equipment\character\swordman\avatar\{…}\*\` | ✅ 实测（find 计数 380） | 换装图层（只查存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/Kalla/DFS_01.img`（54 处引用） | sprite_character_swordman_effect_kalla.NPK | 剑焰刀光主层 | **必需** | ❌ |
| `…Kalla/DFS_03.img`（104 处引用） | 同上 | 剑焰/爆发主层 | **必需** | ❌ |
| `…Kalla/DFS_02.img`、`DFS_Exp_01.img`、`DFS_Exp_02.img` | 同上 | 第三/终结段与 boom 爆炸 | **必需**（视觉主体） | ❌ |
| `…Kalla/1-normal/dodge.img`、`2-normal/dodge.img`、`3-b-normal/dodge.img`、`3-f-normal/dodge.img`（8 张） | 同上 | 各段剑体/残影层 | 可选 | ❌ |
| `…Kalla/fire-normal.img`、`explo-dodge.img` | 同上 | boom 火焰/爆炸 | 可选 | ❌ |
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色帧 | 必需（共享） | ✅ 已入库 |

**结论**：全部特效 img 共 15 张**同属一个 NPK**（sprite_character_swordman_effect_kalla.NPK，与技能 82 Kalla 共享图集——"撞 Kalla 共享结构"的实相：共享的是 img 不是逻辑）；必需 5 张。AnimRes 无 DFS/Kalla 条目（实测）。

## 5. 实现方案草案

- **内容件清单**（五段自身连斩，无 PO——全批最简）：
  - `MingYanJianSkill : SkillLogic`（同三段斩连段子状态机范式，L19 段间重置）：`CooldownMs=45000`、`TotalTimeMs=2200`（五段 2160ms + 余量）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanDarkFlameReady)`、SubState=0。
    - `OnUpdate` 段切点（480/780/1080/1680ms）→ `ctx.PlayAnim(下一段)` + `ctx.ClearHitTargets()`（段间重置，每段独立可命中）。
    - 每段攻击盒帧窗（atk1 F2-F3 ≈ 100-200ms 段内相对、atk2 F1-F3、atk3 F2-F6 ×100ms、finish F0-F3 ×60ms）→ 用 `ctx.SetAttackHitbox(前偏 ≈1.0-1.4, 半尺寸逐段 (1.4,0.45,0.8)→(2.0,0.45,1.0))` + `ctx.DisableAttackHitbox()` 按帧开关（releasewave 冲刺盒同构，帧表来自 §2.3）；伤害走技能 `HitReaction` 按段切换（见数值表）。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
  - 无 Area/Bullet/Action/Buff 新建——**五类内容件只需 1 个 Skill**。
- **概念映射**：五子状态 var→SubState；sq_SetCurrentAttackInfo 段切换→HitReaction 分段 override（实现上 OnUpdate 按段换 SetAttackHitbox + 每段一次性 EnterHit；或简化为"每段一次 SetAttackHitbox + 单帧 MeleeHit"——demo 推荐后者，帧窗全收为段中一次判定）；sq_SetMyShake→跳过；.als 五册→AnimOverlayConfig 直译（现有 als 子命令）。
- **注册点**：SkillIds 加 `MingYanJian=22`；AnimIds 加 `SwordmanDarkFlameReady=99`、`…Atk1=100`、`…Atk2=101`、`…Atk3=102`、`…Finish=103`（5 个 body json，overlay 由 als 自动）；BuildAtlas 加 kalla 图集（1 个）；按键映射新键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000ms | 45000（直用） |
| 总时长 | 2160ms（480+300+300+600+480） | TotalTimeMs 2200 |
| 攻击力 | col0=1863→15156（186%~1516%，四段同源） | 每段一次判定，伤 70/70/110/160 |
| 段1/2 命中 | atk1/2：damage/push30/lift30 | 伤 70/硬直 300/Kb 30/Ly 30 |
| 段3 命中 | atk3：down/push40/**lift300**/hit lift up | 伤 110/硬直 500/Kb 40/Ly 300 |
| 终结命中 | finish：down/push70/**lift300**/hit lift up | 伤 160/硬直 600/Kb 70/Ly 300 |
| 段切点 | 480/780/1080/1680ms | 直用 |
| 攻击盒 | 281/289/328/394px 宽（§2.3） | 半尺寸 (1.4,0.45,0.8)→(2.0,0.45,1.0) |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| darkflameslash_*.ani ×5（实测节名枚举） | 无缺口节——仅 IMAGE/IMAGE POS/DELAY/DAMAGE BOX/ATTACK BOX + 已知跳过项（DAMAGE TYPE×35/PLAY SOUND×7/SET FLAG×2） | 现有 ani 子命令全覆盖 |
| 五份 .als | 仅 `[use animation]`/`[none effect add]` | 现有 als 子命令全覆盖 |
| MINGYANJIAN.skl / 4 份 .atk | `.skl`/`.atk` 无子命令 | 既有缺口；本技能手抄量极小（1 列数值 + 4×8 行 atk） |

**结论**：**本技能资源全部可被现有 ani/als 子命令翻译**（全批唯一零翻译缺口技）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| magic + dark element 暗属性攻击 | 元素属性系统（缺失） | 无视属性（纯数值伤害），标注差异 |
| 五段帧窗判定（每段 2-5 帧盒） | 无缺口（帧驱动攻击盒已落地，LSHitboxComponent 帧表） | §5 直译；或段中一次判定简化 |
| "向前突进"（explain） | 位移未在脚本/数据发现（**未考证**） | 不做位移（原地连斩；若实测有位移，用 MoveCasterForward 每段 +0.5 补） |
| 屏震 5/400（终结段） | 延后 | 跳过 |
| 攻速静态信息 sq_SetStaticSpeedInfo | 无消费（攻速系统） | 固定 1.0 |
| 血液表现 blood 10 2.0 | 无独立血液系统（BleedBuff 是异常非表现） | 跳过（表现层） |

## 8. 存疑与缺口上报

- **未考证**：①"突进"位移的实现载体（无脚本无数据）；②skill class 3 与其它技（class 1）的差异含义；③atk1 F3=1 / atk2 F2=1 两个 flag 的消费者（疑音效/特效触发）；④pvp CD 300000 的档位依据（数据照录）。
- **分族结论（供主循环回填）**：**资源真名 ≠ skl 名**——MINGYANJIAN 的动画/atk/特效全叫 DarkFlameSlash（.chr #171-175/#101-104 精确对位实证）。新式拼音名技能（guiqi 目录）先查 .chr 槽位常量表拿真名，再按真名搜资源。kalla.nut 亦为 mod 混淆（变量乱码实证，C3 又一实例——guiqi 目录整体可疑）。
- **系统级缺口复证**：元素属性系统（本批首撞"暗属性"具体值——此前记档为泛"元素属性系统"）。
- **无新翻译缺口**。
