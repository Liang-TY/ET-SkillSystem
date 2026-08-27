# 雷神之息（FORCEOFTHUNDERER）

> 技能ID 250 | 级别 C（二觉被动，阿修罗） | 可实现性 🔶（分半：数值半 ⛔——增伤 col3 撞属性消费链；光环半 ✅ 可表达——全 pvf 少见的"有完整脚本的二觉被动"，最近敌索敌+定点雷击+感电三件套数据链闭合，GetEnemies/CreateArea API 实测在库；"被动载体"需降级为主动开关型光环技） | 分析日期 2026-08-22 | 批次 C5

**判定口径按"面板可表达 / 消费端卡死"分半**（批次要求）：与 248 的差异在于本技能的 appendage **不是空壳标记而是完整逻辑**（自动索敌+定时召唤），消费端不依赖其他技能——是三个二觉被动里唯一"自带完整可实现实体"的。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 雷神之息 | `skill\Swordman\FORCEOFTHUNDERER.skl` [name] |
| 英文名 | FORCEOFTHUNDERER（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 阿修罗·天帝（二觉被动；procSkill gate `growtype==4`，L17 映射 4=阿修罗；雷/感电主题） | `passive_skill_swordman.nut:159` |
| 学习等级 | 75 | skl [required level] |
| 最高等级 | 30（[second growtype maximum level] 第 9 槽 = 30） | skl [maximum level] / [second growtype maximum level] |
| 类型 | 被动（[passive]，skill class 1） | skl [type] |
| 指令 | —（纯被动，**可 On/Off 开关**：[seal enable] 1，脚本按 isSealFunction 移除/重挂） | skl [seal enable] + procSkill 实测 |
| 特殊消耗 | 无 | skl 实测 |
| 一句话效果 | 增加基本攻击力和技能攻击力；以雷神气息庇护身体，每 1.8 秒自动雷击周围敌人（无距离上限，锁定最近敌），概率感电 | skl [explain] + 脚本实测 |

**level property（7 项全明，L21 向量法；4 列 level info × 29 行 + static 4 槽，Lv1 → 表末第 29 行）**：
（表 29 行 vs [maximum level] 30 差 1 行，疑 mod 截表；下列末值取表末行实测）

| # | 模板项 | 向量 | 解读 | Lv1 | 表末 |
|---|---|---|---|---|---|
| 1 | 雷神之息间隔 | (0, 0, 0.001) | **static[0]**×0.001 | 1.8s（恒定） | 1.8s |
| 2 | 感电概率 | (1, 1, 1.0) | **static[1]** | 100（恒定） | 100 |
| 3 | 感电持续时间 | (2, 2, 0.001) | **static[2]**×0.001 | 3.0s（恒定） | 3.0s |
| 4 | 雷电伤害 | (-2, 0, 1.0) | level col0 | 1322 | 4066 |
| 5 | 感电等级 | (-1, 1, 1.0) | level col1 | 80 | 164 |
| 6 | 感电攻击力 | (**-6**, 2, 1.0) | level col2（源 -6 未解，按索引=level 列读，与 -4 同族推断） | 5652 | 19036 |
| 7 | 基本攻击力和技能攻击力增加 | (-1, 3, 0.1) | level col3×0.1 | 14% | 70% |

[static data]（dungeon）`1800 100 3000 100`：前 3 槽由模板消费；**static[3]=100 无模板项**——脚本实读为**雷击判定/图像缩放百分比**（100%=1.0 倍，§2.2）。

## 2. 技能逻辑走读

### 2.1 注册与挂载（每帧维护型）

无 pushState。挂载点在 `passive_skill_swordman.nut` 的 `procSkill_FORCEOFTHUNDERER`（由 `procSkill_Swordman` 调用，引擎逐帧回调）：

```
if (技能等级 > 0 && growtype == 4) {
    if (skill.isSealFunction())          // 玩家 Off（seal enable）
        sq_RemoveAppendage(obj, "Character/Swordman/ForceOfThunderer/ap_ForceOfThunderer.nut")
    else                                 // On
        若未挂 → sq_AppendAppendage(obj, obj, 250, true, append, true)
}
```

即：**开关两态由逐帧维护**（与 248/171 的"习得事件一次性挂载"不同形态）；转职离开由等级条件自然摘除。

### 2.2 appendage 本体：ap_ForceOfThunderer.nut（完整逻辑，115 行全读）

```
onStart：记 appendage 计时起点。
proc（每帧）：
    if (当前时间 - 起点 > static[0] = 1800ms) {
        summon_FORCEOFTHUNDERER(parentObj);   重置起点
    }

summon_FORCEOFTHUNDERER：
    遍历碰撞对象，滤出敌方且可受击（isEnemy + isInDamagableState + OBJECTTYPE_ACTIVE）；
    平方距离取**最近一个**（⚠ 无最大距离上限——全屏索敌）；
    若存在目标：
        power   = sq_GetPowerWithPassive(250, -1, 0, -1, 1.0)     // col0 雷电伤害 1322→4066
        size    = sq_GetIntData(250, 3)                            // static[3]=100 → 缩放 100%
        gdProc  = sq_GetLevelData(250, 1, level)                   // col1 = 80→164
        gdLeve  = sq_GetIntData(250, 1)                            // static[1] = 100
        gdTime  = sq_GetIntData(250, 2)                            // static[2] = 3000ms
        gdRate  = sq_GetPowerWithPassive(250, -1, 2, -1, 0.18)     // col2 × 0.18 = 1017→3426 感电每跳
        写包(250, power, size, gdProc, gdLeve, gdTime, gdRate)
        → sq_SendCreatePassiveObjectPacketPos(24370, 0, tarX, tarY+1, 0)   // 在目标脚下生成共享 PO
```

⚠ **概率/等级参数与模板命名互换**：按模板项序应"概率=static[1]=100、等级=col1=80→164"；脚本把 col1 装进 prob 位、static[1] 装进 level 位下发。两解都在（数值无损），哪个命名正确未考证——按 PO 消费端字面（prob=col1）则概率 80%→164%（>100% 异常），按模板则概率恒 100%、等级 80→164（更合理），**倾向模板命名正确、脚本传参位互换**，存疑记档。

### 2.3 共享 PO 24370 的 case 250（`sqr\common_object\share_obj\swordman\setcustomdata.nut:8`）

```
attackInfo = sq_GetCustomAttackInfo(obj, 58)      // → AttackInfo/ForceOfThunder.atk（obj 槽位直读）
setCurrentAnimationFromCutomIndex(obj, 73)        // → Animation/ForceOfThunderer/LightningPower_Eff_A.ani
读包：power / size(=100→1.0 缩放) / prob / level / duration / lightDamage
图像与攻击盒按 size 缩放（setImageRateFromOriginal + sq_SetAttackBoundingBoxSizeRate ×3 轴）
sq_SetAddWeaponDamage(true)                        // 附加武器伤害
sq_SetCurrentAttackPower(power)                    // 雷电伤害 col0
sq_SetChangeStatusIntoAttackInfoWithEtc(0, ACTIVESTATUS_LIGHTNING, prob, level, duration, lightDamage, 0)
sq_SetCurrentAttackeHitStunTime(0)                 // 零硬直（纯伤害+感电）
onendcurrentani：播完即毁（单发雷击）
```

### 2.4 判定与视觉（PO 侧实测）

| 件 | 值 | 来源 |
|---|---|---|
| ForceOfThunder.atk（obj 槽 58） | magic / weapon damage apply 1 / **light element** / damage 反应 / push 20 / lift 10 / no blood 5@1.0 / 零硬直（脚本置 0） | 文件实读 |
| LightningPower_Eff_A.ani（obj 槽 73） | 14 帧 840ms；**F3-F7 攻击盒**：F3 `-26,-40,19/51,80,468`（z 高至 468 的**垂直雷柱**）、F4-F7 约 `-62,-50,-8/124,100,87`（min/max） | C1 法提取 |
| .als 边车 | `[use animation]`×6 + `[add]`×6：叠 LightningPower_Eff_B/C/D/E/F/G 六层（E/F 层引 **Character/Mage/Effect/ATLightningWall** 跨职业图集，L14 常态） | 实测全文 |
| 感电（ACTIVESTATUS_LIGHTNING） | prob/level/duration=3s/lightDamage=col2×0.18 每跳 | 写包实测 |

⚠ 元素属性：atk 声明 **light element**——元素属性系统缺失在案（判定口径缺口），对我们仅影响"是否吃光抗"，demo 无元素减伤系统可忽略。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FORCEOFTHUNDERER.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FORCEOFTHUNDERER.skl` | ✅ 实测（107 行） | 7 项数值全明 |
| 注册行 | passive_skill_swordman.nut:156-174（procSkill_FORCEOFTHUNDERER） | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | 开关维护挂载 |
| appendage | ap_ForceOfThunderer.nut | `…\sqr\character\swordman\forceofthunderer\ap_forceofthunderer.nut` | ✅ 实测（115 行全读） | 索敌+定时召唤（完整逻辑） |
| 常量 | SKILL_FORCEOFTHUNDERER <- 250 | `…\swordman\swordman_header.nut:112` | ✅ 实测 | — |
| PO 回调 | setcustomdata.nut:8 / onendcurrentani.nut:5 | `…\sqr\common_object\share_obj\swordman\` | ✅ 实测 | case 250 装配/销毁 |
| PO obj | qq506807329new_swordman_24370.obj（etc motion #73 / etc attack info #58，0 基直读） | `…\passiveobject\script_sqr_nut_qq506807329\swordman\`（passiveobject.lst:9-10） | ✅ 实测 | 槽位对表 |
| PO .atk | forceofthunder.atk | `…\script_sqr_nut_qq506807329\swordman\AttackInfo\` | ✅ 实测 | §2.4 |
| PO 动画 | lightningpower_eff_a~g.ani（7 个 + a 的 .als） | `…\script_sqr_nut_qq506807329\swordman\Animation\ForceOfThunderer\` | ✅ 实测 | 雷击视觉七层 |
| 图标 | SkillIcon.img 442/443 | skl [icon] | ✅ 引用实证 | demo 不需要 |
| 角色 .ani / .atk / .als | — | `…\character\swordman\` | ⛔ 无 | 光环无角色侧姿态 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| Character/Swordman/Effect/LightningPower/LightningPower.img | sprite_character_swordman_effect_lightningpower.NPK | 雷击主层+叠层 a/b/c/d/g（5 个 ani 共引） | **必需** | ❌（021 文档记过同目录 LightningPOW_ADD_A.img 未入库，本表为另一张） |
| Character/Mage/Effect/ATLightningWall/5_el-p_normal.img | sprite_character_mage_effect_atlightningwall.NPK | 叠层 E（3 帧） | **必需**（跨职业复用） | ❌ |
| Character/Mage/Effect/ATLightningWall/6_el-p_dodge.img | 同上 | 叠层 F（4 帧） | **必需** | ❌ |

缺失 img：必需 3 张（三个 NPK——含一个魔法师树 NPK，跨职业提取时注意）。v2/v4 由提取时把关。

## 5. 实现方案草案（光环半；数值半 ⛔ 见 §7）

**被动降级为"主动开关型光环技"**（被动技能系统缺失 + Buff Tick 无世界查询门面，见 §7 第一行；DNF 本体恰好有 On/Off 开关语义 [seal enable]，降级手感无损）：

- **内容件清单**：
  1. `ThunderBreathSkill : SkillLogic`——`CooldownMs = 1000`（开关节奏）、`TotalTimeMs = 30000`（demo 光环期 30s；DNF 原为永久被动）。`OnCast`：`ctx.AddBuffToSelf(BuffIds.ThunderBreath)`（视觉/占位挂 Buff）；`OnUpdate`：`ElapsedMs % 1800 == 0` 守卫（SubState 计次）→ `ctx.GetEnemies()` 取最近（距离平方比较，`LSVector` 运算）→ `ctx.CreateArea(AreaIds.ThunderStrike, 敌人位置)`。**全程只用已核实 API**（SkillContext.cs:105 GetEnemies / :147 CreateArea 实测在库）。
  2. `ThunderStrikeArea : AreaDefinition`——同 BloodBoomArea 范式：`TotalTimeMs=840`（雷击 ani 时长）、`EnterActions={MeleeHit, AddLightningBuff}`、`HalfExtents=(0.93,0.75,2.4)`（雷柱盒 F4 z 轴取半高，垂直感用动画表达）、`HitReaction{Damage=100（col0 demo 折算）, HitstunMs=0（原值零硬直）, KnockbackX=20, LaunchY=10, ProcBuffId=BuffIds.Lightning, ProcChance=100}`、`ViewAnimId=AnimId.LightningPowerEffA`（叠层 B-G 用 RegisterOverlay 手工挂 6 层，.als 同构）。
  3. `LightningBuff : BuffDefinition`——克隆 `BurnBuff`（唯一差异：`TotalTimeMs=3000`、tick 伤害 40≈col2×0.18 demo 折算、视图层若做再议）；`AddLightningBuffAction : LSAction`——克隆 `AddBurnBuffAction`（~10 行）。
- **概念映射**：procSkill 逐帧维护 → SkillLogic.OnUpdate（开关重按 = 重挂）；summon 最近敌 → `GetEnemies()` + 距离比较；PO 24370 case 250 → ThunderStrikeArea；ACTIVESTATUS_LIGHTNING → LightningBuff（L6 标准链路同构）；零硬直 → `HitstunMs=0`。
- **注册点**：`SkillIds.ThunderBreath = 34`（L18 号段顺延注明）；`BuffIds.Lightning = 18`；`AreaIds.ThunderStrike = 39`；`AnimIds.LightningPowerEffA = 178`（叠层 6 个可共用 1 个 AnimId + overlay 配置）；按键映射 `LSOperaComponentSystem` 新键。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 雷击间隔 | static[0] = 1800ms | 1800 直用 |
| 索敌 | 最近敌，无距离上限 | 最近敌，限半径 8 单位（防跨屏瞬移观感） |
| 雷电伤害 | col0 = 1322→4066 | MeleeHit 100 |
| 感电概率 | 100%（模板读法；脚本读法 80%→164% 互换存疑） | ProcChance 100 |
| 感电时长 | static[2] = 3000ms | 3000 直用 |
| 感电每跳 | col2×0.18 = 1017→3426 | 40（BurnBuff 同档） |
| 感电等级 | 80→164（col1） | 砍（无等级对抗系统） |
| 增伤 | col3×0.1 = 14%→70% | 占位 +70 零消费 |
| 击退/浮空 | atk：push 20 / lift 10 | 20/10 直用 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| FORCEOFTHUNDERER.skl | `.skl` 无子命令（4 列 × 29 行 + static 4 槽 + 7 向量） | 本文档已手抄全明；`skl` 子命令同前议 |
| forceofthunder.atk | `.atk` 无子命令 | 手抄 6 值（§2.4 已全列） |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | python 直读槽位（同 248 文档） |
| lightningpower_eff_a~g.ani | 常规节（FRAME/DELAY/IMAGE/ATTACK BOX/SHADOW）全可译；**空 IMAGE 帧存在**（占位帧） | 现有规则可处理（064 先例） |
| lightningpower_eff_a.ani.als | `[use animation]` + `[add]` 常规两节 | **全部可译**（实测无规则外节） |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 常驻被动（习得即生效，永久） | 被动技能系统缺失；`BuffDefinition.TickActions` 的 LSActionContext **无世界查询/CreateArea 门面**（R4-B18 缺口，本批再实证） | 降级为主动开关型光环技（30s 通道，OnUpdate 驱动）——DNF 本体有 On/Off 开关，手感无损 |
| 增伤 14%→70%（col3） | **缺失档：属性数值消费链** | ⛔ 数值半主因；NumericType 占位 |
| 感电 DOT | 无 LIGHTNING Buff（有 Burn/Bleed 同构先例） | 新增 LightningBuff（BurnBuff 克隆，非缺口） |
| light element 元素 | 元素属性系统缺失（在案） | 忽略（无元素减伤消费） |
| 零硬直雷击 | 无（HitstunMs=0 直配） | 直译 |
| 概率/等级传参互换 | 脚本与模板命名矛盾（§2.2） | 按模板命名取值（100% / 80→164），存疑记档 |
| 七层叠视觉 | 常规 overlay（.als [add]） | RegisterOverlay 手工挂或翻译直出 |

## 8. 存疑与缺口上报

**未考证项**：①prob/level 传参位与模板命名互换（§2.2，倾向模板正确）；②static[3]=100 作���缩放而非"光环半径"——**本 pvf 实现无距离上限**（原版雷神之息应有范围限制，疑 mod 简化），demo 建议自加半径；③源 -6 的向量语义（L21 未解族，本批按索引=level 列读并获 0.18 系数运行时交叉印证）；④[second growtype maximum level] 第 9 槽=天帝（与 248 第 3 槽/171 第 11 槽同表反推，无权威映射）；⑤level info 29 行 vs [maximum level] 30（疑 mod 截表）。

**新系统级缺口（消费方增补）**：**LSActionContext 无世界查询/CreateArea**——Buff Tick 驱动的自动索敌类被动（本技能形态）在 BuffDefinition 上表达不了，只能借 SkillLogic.OnUpdate。与 R4-B18"Buff 到期触发施法/LSAction 侧 CreateArea"同根，本批给出**第 2 个具体消费方**（自动光环类被动），建议该缺口立项时一并考虑"LSActionContext.GetEnemies/CreateArea"门面（Buff 驱动技能的一半场景都卡这里）。

**给下轮的经验**：二觉被动三形态现在齐了——①空标记+宿主钩子（171 僵直参数型 / 248 全联动型）；②完整 appendage 自治体（250 索敌召唤型，**挂在 `forceofthunderer\` 独立目录而非 appendage\ 目录**，按 procSkill 函数找）；判形态先读 `passive_skill_swordman.nut` 的 case/procSkill 两区。250 是"被动可玩化"的最佳首发候选（数据链闭合 + API 全在库 + 无宿主依赖）。
