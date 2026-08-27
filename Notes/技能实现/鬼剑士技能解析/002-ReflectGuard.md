# 鬼印珠（ReflectGuard）

> 技能ID 2 | 级别 B（**预分类纠偏：A 类候选 → B**，skill class 1 攻击技但核心机制是"波动印资源消耗+多段弹体"，按 B 类以机制为重点分析） | 可实现性 🔶（深简化：印记资源系统缺失，按 0 印基础版可做） | 分析日期 2026-08-22 | 批次 B1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼印珠 | `skill\Swordman\ReflectGuard.skl` [name] |
| 英文名 | ReflectGuard（取 skl 文件名；[name2]="Ghost gate return"） | 同上 [name2] 实测 |
| 职业 | 阿修罗（[skill fitness growtype] **仅 4**；growtype maximum level `0 0 0 0 50 0`——唯阿修罗可学 50 级） | 同上 |
| 学习等级 | 15 | 同上 [required level] |
| 最高等级 | 70 | 同上 [maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ←→→ + Z | 同上 [command] / [command key explain] |
| CD | **skl 无 [cool time] 节**（实测 grep 无命中）——限频疑由读条 300ms + 引擎内部机制承担（未考证） | 同上 |
| MP | 95 → 980（Lv1→Lv70） | 同上 [consume MP] |
| 读条 | casting time 300 ms | 同上 [casting time] |
| 特殊消耗 | **波动印**（技能 47 WaveMark 波动刻印系统的资源，印数决定威力，见 §2.5） | [explain] + ap_wavemark.nut |
| 屏震 | [shake screen] 2 200（延后档） | 同上 |
| static data | `250 150 10 120`（static[1]=150 已由模板向量实证为多段间隔 ms；static[0]=250 疑弹速 px/s、static[2]/[3] 未考证） | 同上 [dungeon][static data] + [level property] |
| 一句话效果 | 把波动印转化成鬼印珠向敌人发射：珠体缓速前进、穿透多段攻击（间隔 0.15s）；印记越多攻击力/大小/段数/时长越强；珠消失时爆炸击倒周围敌人 | 同上 [explain] |

**level property 模板解码（9 列 + 2 个 static 引用，L21 向量法全解）**：

| 模板行 | 向量 | 来源 | 数值（Lv1→Lv70） |
|---|---|---|---|
| 鬼印珠魔法攻击力 | (-1,1,1.0) | col1 | 135% → 1071% |
| 持续时间 | (-1,0,0.001) | col0 | **3.0s（恒定）** |
| 多段攻击间隔 | (**1**,1,0.001) | **static[1]** | **0.15s（恒定）** |
| 鬼印珠大小比率 | (-1,2,0.1) | col2 | 100% → 207.3% |
| 爆炸魔法攻击力 | (-1,7,1.0) | col7 | 270% → 2142% |
| 每印·攻击力增加比率 | (-1,3,1.0) | col3 | 46% → 358% |
| 每印·增加持续时间 | (-1,4,0.001) | col4 | +0.3s（恒定） |
| 每印·增加多段攻击次数 | (-1,5,1.0) | col5 | +1 次（恒定） |
| 每印·大小增加比率 | (-1,6,0.1) | col6 | +10%（恒定） |
| 每印·爆炸攻击力增加比率 | (-1,8,1.0) | col8 | 90% → 714% |

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无 pushState**（引擎内置，F3）。文件链全在数据侧：

- 施法动画：`swordman.chr` [etc motion] **槽 46 = ReflectGuard.ani**（`swordman_header.nut:216` 常量 `CUSTOM_ANI_REFLECTGUARD <- 46` 实证，槽位逐行数过 0 基吻合）。
- 弹体 PO：`reflectguardbead.obj` = **20033**（passiveobject.lst:11199 实测）；爆炸 PO：`reflectguardexplosion.obj` = **20049**（lst:11231）。`_ds` 变体 20096/20097（demonicswordman 黑暗武士版）。

### 2.2 施法侧引擎内置行为重建

```
施放（读条 300ms）：
  播 ReflectGuard.ani（10 帧 ×60ms = 600ms）
  F8 SET FLAG 65534（取消窗口/特殊标记，064 同款惯例，语义未考证）
  F? 帧（引擎内置，疑 F4-F8 段）：读取当前波动印数量 n（引擎内资源）
    伤害倍率 = col1 + col3×n；时长 = col0 + col4×n；大小 = (col2 + col6×n)×10%；
    多段次数预算 = 时长 / static[1]；爆炸倍率 = col7 + col8×n
  写包 → 创建 PO 20033（虚无之球，出生=身前，向面朝方向缓速前进 250px/s 推断）
```

### 2.3 被动对象一：鬼印珠弹体（reflectguardbead.obj = 20033）

| .obj 节 | 值 |
|---|---|
| [width] | 1 1 |
| [floating height] | 1 |
| [pass type]/[piercing power] | pass all / 1000（**全穿透**） |
| [basic motion] | `Animation/ReflectGuardBead/eg-ball.ani`——2 帧 200ms **循环**，**两帧均有 ATTACK BOX `-44 -20 -46 93 40 94`**（≈x[-0.44,0.93] y[-0.2,0.4] z[-0.46,0.94] 单位，÷100） |
| [etc motion] | `eg-ball-end.ani`（5 帧 350ms 非循环，珠体消失表现） |
| [attack info] | `AttackInfo/ReflectGuardBead.atk`：**魔法**/damage 反应/push **100**/lift **50**/[knuck back] -1/no blood 100 1.5 |
| [name] | `虚无之球` |

多段语义：珠体以 0.15s 间隔对碰到的敌人反复结算（引擎 resetHitObjectList 族），**同一敌人可吃满整段多段**。

`.als` 边车（eg-ball.ani.als，全可译）：

```
[use animation] eg-ball-normal.ani=bottom、light.ani=light、light_2.ani=light_2
[add] 0 -10 bottom（底层珠体视觉层）；0 -9 light；0 -8 light_2（两层闪电缠绕，借 LightningGod 图集，L14 跨目录复用）
```

### 2.4 被动对象二：珠体爆炸（reflectguardexplosion.obj = 20049）

| .obj 节 | 值 |
|---|---|
| [basic motion] | `../../Common/Animation/NoneExplosion.ani`——**1 帧 400ms**，F0 有 ATTACK BOX `-45 -45 -50 130 130 100`（≈x[-0.45,1.3] y[-0.45,1.3] z[-0.5,1.0] 单位，÷100——爆炸盒偏前方），**无 img 引用（空占位，L7）**：纯判定帧，视觉全靠粒子 |
| [attack info] | `AttackInfo/ReflectGuardExplosion.atk`：**魔法/down 击倒反应**/push **200**/lift **200**/no blood 100 1.0 |
| [string data] | NoneExplosionParticle1-4.ptl ×4（**粒子：L5 翻译+系统双缺口**） |
| [name] | `中性爆炸` |

珠体存在到期（col0+col4×n 秒）→ 创建 20049 于珠体当前位置 → 击倒周围敌人 → 消失。

### 2.5 波动印资源系统（本技能的前置机制）

- 挂点：`sqr\character\swordman\appendage\ap_wavemark.nut`（29 行实测）：proc——计时 10s 或施放技能 **47（WaveMark 波动刻印，lst 实证 `47 -> Swordman/WaveMark.skl`**）时 appendage 失效（印记清空规则）。
- 攒印：波动剑系施放时推印（`wave\wave.nut:69-71`：挂有 ap_wavemark 且 WaveMarkPush——每放一道波动剑攒 1 印）。
- 耗印：鬼印珠施放时按当前印数 n 放大参数（引擎内置，无脚本可读——n 的上限与计息规则未考证，DNF 惯例上限随波动刻印等级）。
- **我们侧完全无对应系统**（无技能资源堆叠/消耗机制）——本批新增缺口上报（§8）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ReflectGuard.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ReflectGuard.skl` | ✅ 实测（258 行） | 数值（9 列全解） |
| 注册行 | — | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无（引擎内置） | F2 三招全查 |
| .chr 条目 | etc 槽 46 | `…\pvf\character\swordman\swordman.chr` 1019 行 | ✅ 实测 | Animation/ReflectGuard.ani |
| 常量 | swordman_header.nut:216 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_REFLECTGUARD=46 |
| 角色 .ani | reflectguard.ani | `…\pvf\character\swordman\animation\reflectguard.ani` | ✅ 实测 | 10 帧 600ms，F8=65534 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（grep reflect 无） | ⛔ 无 | 伤害在 PO atk |
| PO 注册 | passiveobject.lst:11198-11199 / 11230-11231 | `…\pvf\passiveobject\passiveobject.lst` | ✅ 实测 | 20033 / 20049 |
| PO 定义 | reflectguardbead.obj / reflectguardexplosion.obj | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | §2.3/§2.4 |
| PO .atk | reflectguardbead.atk / reflectguardexplosion.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | 珠体 push100/lift50；爆炸 down push200/lift200 |
| PO .ani | eg-ball.ani、eg-ball-normal.ani、eg-ball-end.ani、eg-ball-end-normal.ani、light.ani、light_2.ani（+reflectguardbead\ 与 _ds\ 两目录） | `…\pvf\passiveobject\character\swordman\animation\reflectguardbead\` | ✅ 实测 | §2.3 |
| 爆炸 .ani | NoneExplosion.ani | `…\pvf\passiveobject\Common\Animation\NoneExplosion.ani` | ✅ 实测 | 1 帧空占位判定 |
| .als | eg-ball.ani.als、eg-ball-end.ani.als（+_ds 侧 3 个） | 同上目录 | ✅ 实测 | 底层+闪电层（全可译） |
| 粒子 | NoneExplosionParticle1-4.ptl | `…\pvf\passiveobject\Common\Particle\`（.obj 引用，未逐一验存在） | 未逐一验 | 爆炸视觉（L5 缺口） |
| 波动印 | ap_wavemark.nut + wave.nut:69 | `…\pvf\sqr\character\swordman\appendage\` / `wave\` | ✅ 实测 | §2.5 |
| 装备层 | reflectguard.ani ×N | `…\pvf\equipment\character\swordman\avatar\{coat,…}\` | ✅ 实测 | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 0-4 施法动作） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法 600ms | 必需 | ✅ 已在库 |
| eg-ball.img | sprite_character_swordman_effect_reflectguard.NPK（`Effect/ReflectGuard/` 下划线化） | 珠体判定帧视觉（eg-ball + eg-ball-end 共用） | 必需 | ❌ 未入库 |
| eg-ball-normal.img | 同上 | 珠体底层视觉（.als bottom 层） | 必需 | ❌ 未入库 |
| light.img / light_2.img | sprite_character_swordman_effect_lightninggod.NPK（`Effect/LightningGod/`） | 闪电缠绕 2 层 | 可选 | ❌ 未入库 |
| （NoneExplosionParticle*.ptl ×4） | 粒子无 NPK 直取规则 | 爆炸视觉 | 可选（无粒子系统） | ❌ |

缺失 img：必需 2 张（同一 NPK）、可选 2 张（另一 NPK）。爆炸无 img 需求（空占位）——视觉建议用既有爆炸特效 ani 替代（releasewave 占位先例）。

## 5. 实现方案草案

**🔶 深简化可行**（按"0 印基础版"实现，印记系统落地后升格）：

- **内容件清单**：
  - `DotNet~/Skills/ReflectGuardSkill.cs : SkillLogic`——同 `WaveSwordSkill` 范式：
    - `CooldownMs`：DNF 原值无 CD；**demo 建议值 1000**（防连打，读条 300ms 的替代限频）。
    - `TotalTimeMs = 600`（施法动画时长）+ ManualCooldown=false。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanReflectGuard)` + `ctx.CreateBullet(BulletIds.ReflectGuardBead)`。
    - `OnUpdate`：**到期爆炸**——珠速 250px/s × 存续 3s = 前进 7.5 单位（纯函数可算，`ctx.GetElapsedMs() == 300+3000` 时 `ctx.CreateAreaInFront(AreaIds.ReflectGuardExplosion, 7.5)`——不查弹位置，位置确定性推导；简化：珠不减速）。
  - `DotNet~/Bullets/ReflectGuardBead.cs : BulletDefinition`——复制 `NormalWaveBullet` 改：
    - `Speed = (FP)5/2`（250px/s）；`TotalTimeMs = 3000`；`DestroyOnHit = false`（穿透）；
    - `HalfExtents` 按 eg-ball 攻击盒：x[-0.44,0.93]→半长 0.7、y 高 0.3、z 0.7 → `(0.7, 0.3, 0.7)`；
    - `HitActions = { MeleeHit }`；`HitReaction{ Damage=80, HitstunMs=400, KnockbackX=100, LaunchY=50 }`（atk 原值 push100/lift50）；
    - `ViewAnimId = AnimId.EgBall`（+底层/闪电层由弹体 overlay 视图挂 .als 翻译产物，rw_creature 先例）。
  - `DotNet~/Areas/ReflectGuardExplosionArea.cs : AreaDefinition`——同 `BloodBoomArea` 范式：`TotalTimeMs=400`（NoneExplosion 1 帧）、`EnterActions={MeleeHit}`、`HitReaction{Damage=150, HitstunMs=800, KnockbackX=200, LaunchY=200}`（atk down/push200/lift200，击倒手感=releasewave as-built 同构）、`ViewAnimId=` 替代爆炸特效（NoneExplosion 空占位，用库内 rw_burst1 或新提可选 img）。
- **深简化清单**（印记系统缺失砍掉什么）：n 恒取 0 → 攻击 135%（Lv1 基准）、时长 3s、大小 100%、爆炸 270%——**印数增益、珠体放大、多段段数增加、降速机制全部不做**；多段攻击本身见下。
- **多段攻击（0.15s 间隔同敌反复结算）**：BulletDefinition 的 HitTargets 去重是单次语义——**撞"Bullet 侧多段命中 ResetHitIntervalMs"缺口（R1-A5 首记，本技能第 2 例）**。简化二选一：① 珠体改单次穿透伤害（每敌只吃 1 跳，数值上调补偿）；② 珠体退化为 Area（Tick 150ms 无去重特性可直接表达多段，L19/R2-A8 路径）——但 Area 不移动，珠体视觉自挪（DNF 珠速 2.5 单位/s 很慢，3s 共 7.5 单位，Area 固定在施放点的偏差可感知，接受度需定夺）。
- **注册点**：`SkillIds.ReflectGuard = 27`；`AnimIds 117-123`（SwordmanReflectGuard / EgBall / EgBallNormal / EgBallEnd / Light / Light2 / 爆炸替代）；`BulletIds.ReflectGuardBead = 6`；`AreaIds.ReflectGuardExplosion = 18`。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 无（读条 300 限频） | 1000ms |
| 施法 | 600ms（10 帧） | 600 直用 |
| 珠速 | 250px/s（static[0] 推断） | 2.5 单位/s |
| 珠存续 | 3s（0 印） | 3000ms |
| 多段间隔 | 150ms（static[1] 实证） | 简化为单跳（或 Area Tick 150） |
| 珠体命中 | atk：magic/damage 反应/push100/lift50 | Damage 80/Hitstun 400/Kb 100/Ly 50 |
| 爆炸 | atk：down/push200/lift200；倍率 270% | Damage 150/Hitstun 800/Kb 200/Ly 200 |
| 爆炸半径 | NoneExplosion 盒 `-45 -45 -50 130 130 100` | HalfExtents 0.9（盒 x 跨 1.75 的半值），前偏 0.4 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `ReflectGuard.skl` | `.skl` 无子命令（9 列 + 2 static 引用，模板全明） | 手抄 10 值；`skl` 子命令同前议 |
| reflectguardbead.obj / explosion.obj | `.obj` 无子命令（basic+etc motion/attack info 结构） | 本技能手工映射 Bullet+Area（§5）；obj 子命令立项时按"相位序列"建模（064 §8.3 已提） |
| reflectguardbead.atk / explosion.atk | `.atk` 无子命令；**[knuck back] -1**（atk 字段设计新输入：珠体"不使目标位移回弹"语义未解）；[no blood] 双值字段 | 手抄 4-6 值可接受；atk 子命令字段表补记 knuck back/no blood |
| NoneExplosionParticle1-4.ptl | `.ptl` 无子命令 + 无粒子系统（L5） | 爆炸视觉用特效 ani 替代 |
| eg-ball.ani.als 等 5 个 .als | 无（[use animation]/[add] 全支持） | ✅ 直接可译 |
| eg-ball.ani | `[SHADOW]` | 跳过无碍 |
| reflectguard.ani | `[SET FLAG]` 65534（F8）、`[PLAY SOUND]` | 既有约定跳过 |

结论：动画/边车全部可被现有 ani/als 子命令翻译；实质缺口 `.skl`/`.atk`/`.obj`/`.ptl` 四类 + atk 新字段（knuck back）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 波动印资源（攒印/耗印/上限） | **缺失档：技能资源标记系统**（新缺口，§8） | n 恒 0 基础版；印记系统立项后升格（阿修罗全系受益） |
| 0.15s 间隔同敌多段 | **Bullet 多段 ResetHitIntervalMs**（R1-A5 已记，本技第 2 例） | 单跳穿透 或 Area Tick 化（L19 三档口径） |
| 珠消失原地爆炸 | Bullet 无到期回调/位置查询门面（新缺口变体） | 纯函数推位置 CreateAreaInFront（§5，速度恒定前提成立） |
| 印记放大珠体（100%→207%+10%/印） | 对象整体缩放（延后档） | 固定大小 |
| 多段时降低珠���敌人移速 | NumericType.Speed 零消费（R2-A7 已记） | 跳过 |
| 爆炸粒子 | 粒子系统（缺失档） | 特效 ani 替代 |
| 读条 300ms | 读条（延后档） | 跳过 |
| 屏震 [shake screen] | 屏震（延后档） | 跳过 |
| 65534 标记 | 未考证（064 同款惯例） | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. static[0]=250（珠速推断）、static[2]=10、static[3]=120 语义。
2. 波动印上限/攒取规则（引擎内置；ap_wavemark 只见 10s 失效与技能 47 清印）。
3. 施法侧创建珠体的确切帧号（引擎内置，疑 F4-F8 段）。
4. [knuck back] -1 语义（DNF atk 字段，疑"命中不回弹/贴身"标记）。
5. NoneExplosion ATTACK BOX 已补读（-45,-45,-50 → 130,130,100），见 §2.4。
6. CD 缺失的实战限频机制（引擎内部 CD 猜测）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **技能资源标记系统（波动印）**：攒取（施放 A 技能得标记）→ 存量查询 → 消耗（施放 B 技能按存量放大）→ 超时清空。阿修罗波动系（鬼印珠/爆发/心眼）全系依赖；实现建议：LSSkillComponent 或独立 LSResourceComponent 存 `Dictionary<resId, count>`（进快照）+ SkillContext 门面 `AddMark/GetMarkCount/ConsumeMarks`——比属性消费链轻，可先行立项。
2. **Bullet 到期/销毁回调（OnExpire）**：珠体"消失时原地爆炸"类（死亡触发效果）通用；可先以技能侧纯函数定时替代（速度恒定类成立），变速弹（本技能多段降速版）不成立。
3. （追加记档）Bullet 多段 ResetHitIntervalMs 第 2 实证（邪光斩 350ms 首例）——多段框架项落地时 Bullet/Area 一并加（轮间经验已记）。

**翻译工具缺口**：`.skl`/`.atk`/`.obj`/`.ptl`（重复印证）+ atk 新字段 [knuck back] / [no blood] 双值（atk 子命令立项输入）。

**给下轮的经验**：阿修罗"消耗波动印"族技能（鬼印珠 2/爆发 32 等）共用 WaveMark(47) 资源链——读技能先看 `ap_wavemark.nut` + `wave.nut:69` 的推印逻辑；弹体判定盒直接在 eg-ball.ani 的 ATTACK BOX（不查角色 attackinfo，L3 再证）。reflectguardbead 目录与 _ds 目录是三套同名变体（本尊/剑影/黑暗武士），读本尊目录即可。
