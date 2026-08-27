# 暗天波动眼（WaveEye）

> 技能ID 88 | 级别 B | 可实现性 🔶（深简化"领域区+终结天眼爆炸"可表达主干；**40 秒形态改造（普攻魔法化/三系波动剑变异/普攻尾击刺轮）整体撞"普攻行为替换门面"——与 082 卡洛同缺口首档**，降级为压缩时序版） | 分析日期 2026-08-22 | 批次 B5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 暗天波动眼 | `skill\Swordman\WaveEye.skl` [name] |
| 英文名 | WaveEye（取 skl 文件名；[name2] 实测 `Twinkle Eye`） | 同上 [name2] |
| 职业 | 阿修罗（50 级一觉主动；[second growtype maximum level] 第 9/10 位=30/30；杀意波动前置=阿修罗常识） | 同上 |
| 学习等级 | 50 | 同上 [required level] |
| 最高等级 | 50（二觉档 30） | 同上 |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↑↑↓↓ + Z | 同上 [command] |
| CD | 200000 ms（固定）；[league ban] 1 | 同上 [cool time] |
| 施法时间 | 1000 ms（读条） | 同上 [casting time] |
| MP | 1500 → 12600 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×7 | 同上 [consume item] |
| 前置 | 技能 52（杀意波动）Lv1——**只能在杀意波动状态使用** | 同上 [pre required skill] + explain |
| 武器效果 | magical | 同上 [weapon effect type] |
| static data | `0 0 0 50 50 30 3500 0 7000`（9 槽；槽 6=3500/槽 8=7000 有模板印证） | 同上 [dungeon][static data] |
| 一句话效果 | 开启波动之眼 40 秒生成暗黑领域：域内敌人降命中/降攻速；自身普攻变魔法攻击（三段独立倍率）、冰刃/地裂/爆炎三系波动剑变异为天照/光翼/闪枪、普攻末击发射刺轮；最后暗黑领域内天眼大爆炸 | 同上 [explain] |

**level property（14 列，模板 16 行 16 向量，全部自明）**：持续时间 = col13×0.001 = **40 秒**（恒）；
天照冷却 = static[6]×0.001 = **3.5 秒**；光翼冷却 = col12×0.001 = 2→**1.8 秒**（Lv6 起，对应 6 级里程碑"减光翼 CD"✓）；
闪���冷却 = static[8]×0.001 = **7 秒**；降命中范围 = col10（700→850px）；降命中率 = col0×0.1（5.7%→）；
降攻速范围 = col11（200px 恒）；降攻速 = col1×0.1（80%/100% 档）；
普攻一/二/三击魔攻 = col2/col3/col4（1186/1335/1481 →）；天照魔攻 = col5（755→）；
光翼魔攻 = col6（3128→）；闪枪魔攻 = col7（1055→）；刺轮魔攻 = col8（341→）；
**天眼爆炸魔攻 = col9（64978→34 万+，全 241 技能档位数一档的终伤）**。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**load_state 无注册行**（全文件实测，grep waveeye 零命中）——本技能是**纯引擎���置状态机**（F3 形态，
skill class 1 老一觉）。走读法：header 常量 + .chr 槽位 + PO 数据三方印证（F3 三步）：

```
// swordman_header.nut 行 259-266（实测）
CUSTOM_ANI_WAVEEYECAST <- 89    // 施法
CUSTOM_ANI_WAVEEYESTART <- 90   // 开眼
CUSTOM_ANI_WAVEEYEATTACK1/2/3 <- 91/92/93   // 形态期普攻三段
CUSTOM_ANI_WAVEEYECLAW <- 94    // 天照
CUSTOM_ANI_WAVEEYEWING <- 95    // 光翼
CUSTOM_ANI_WAVEEYESPEAR <- 96   // 闪枪
// passiveobject.lst（实测）：20055=WaveEyeSpear，20056=WaveEyeClaw，20057=WaveEyeWheel，20058=WaveEye（终结）
// sqr\character\swordman\ 白名单内 grep waveeye 仅命中 header（attack.nut/common.nut 均无）——无任何行为脚本
```

### 2.2 引擎内置流程重建（explain + 资源数据反推，时序标推断）

```
① 施法：读条 1000ms + WaveEyeCast.ani（6 帧 480ms）→ WaveEyeStart.ani（2 帧 400ms）开眼
② 形态期 40 秒（col13）：
   · 暗黑领域光环跟随自身（effect: aurarange1/2 + bottomlayer + middlebacklayer 四层）
   · 领域 debuff：700px 内敌人 降命中(col0)/降攻速(col1)（引擎施加，无 appendage 文件）
   · 普攻替换：WaveEyeAttack1/2/3.ani（600/650/350ms，各 2 攻击盒帧）+ WaveEyeAttack1/2/3.atk
     （magic/无属性/push30-40/lift30，倍率 col2/3/4）——形态改造核心
   · 冰刃波动剑 → 波动眼·天照：WaveEyeClaw.ani（500ms）+ PO 20056（claw1/claw2 视觉，
     atk claw=push100/lift200/光属性/hit horizon + etc clawfinal）；"把敌人拉向自身"+生成波动印；CD 3.5s
   · 地裂波动剑 → 波动剑·光翼：WaveEyeWing.ani（450ms，2 攻击盒帧）+ WaveEyeWing.atk
     （push200/lift100/cut+blood50）上下大范围；CD 2s/1.8s
   · 爆炎波动剑 → 波动剑·闪枪：WaveEyeSpear.ani（310ms）+ PO 20055（SpearAppear→SpearAttack→SpearFlash，
     atk spear=push200/lift100）敌人背后生成长枪前刺；CD 7s
   · 普攻最后一击 → 波动剑·刺轮：PO 20057（WheelAppear→WheelLoop→WheelSpark，
     atk wheelstay=push0/lift100 + etc wheelmove=push100/lift100）刺轮前移（位移判定体）
③ 终结：暗黑领域内生成无数天眼 → 大爆炸：PO 20058 WaveEye（name"波动眼:终结技"，
   etc=WaveEye_Light/FinishHit.ani + finisheye/glass 系 20+ 视觉层，atk waveeye=push200/lift300），
   伤害 col9（64978%~34 万%）
```

### 2.3 被动对象 / appendage

| PO | .obj 结构 | .atk 关键值 |
|---|---|---|
| **20056 WaveEyeClaw**（天照） | normal 层/pass all 1000/[basic motion] 空 + [attack info] WaveEyeClaw.atk + **[etc attack info] WaveEyeClawFinal.atk**（二段）+ 播完即毁 | magic/**光属性**/damage/push100/**lift200**/hit horizon/blow/no blood 50 |
| **20055 WaveEyeSpear**（闪枪） | normal/pass all/[basic] SpearAppear.ani + [etc] SpearAttack/SpearFlash + [attack info] WaveEyeSpear.atk | magic/push200/lift100 |
| **20057 WaveEyeWheel**（刺轮） | normal/pass all/[etc] WheelAppear/WheelLoop/WheelSpark + [attack info] WheelStay.atk + [etc attack info] WheelMove.atk（静止/前移双段） | magic/push0-100/lift100 |
| **20058 WaveEye**（终结天眼） | **bottom 层**/pass all/[etc] WaveEye_Light/FinishHit.ani + [attack info] WaveEye.atk | magic/push200/**lift300** |

- appendage：无（`appendage\` 目录 7 文件实测无 waveeye 系）——**领域 debuff 与形态切换全部引擎内置**。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备��� |
|---|---|---|---|---|---|
| character\…\waveeyecast.ani（槽 89） | 6 | 480ms | 无 | 无 | 施法 |
| character\…\waveeyestart.ani（槽 90） | 2 | 400ms | 无 | 无 | 开眼 |
| character\…\waveeyeattack1/2/3.ani（槽 91-93） | 10/11/7 | 600/650/350ms | 无 | **各 2 帧** | 形态期普攻三段（帧驱动盒） |
| character\…\waveeyeclaw.ani（槽 94） | 6 | 500ms | 无 | 无 | 天照（判定在 PO） |
| character\…\waveeyewing.ani（槽 95） | 8 | 450ms | 无 | **2 帧** | 光翼（本体判定） |
| character\…\waveeyespear.ani（槽 96） | 5 | 310ms | 无 | 无 | 闪枪（判定在 PO） |
| PO claw1/claw2.ani | — | — | — | — | 天照爪视觉 |
| PO spearappear/attack/flash + tail1-3.ani | — | — | — | — | 长枪视觉（判定在 atk/引擎） |
| PO wheelappear/loop/spark.ani | — | — | — | — | 刺轮视觉 |
| PO waveeye_light\finishhit + finisheye/glass 系 ~25 个 .ani | — | — | — | — | 终结天眼爆炸视觉（20+ 层） |
| effect\…\waveeye\ 9 个（attack1-3/aurarange1-2/bottomlayer/middlebacklayer/wing1-2） | — | — | — | — | 领域光环/普攻特效/光翼层 |

`.als` 边车：无（两侧实测）。角色动画仅引 sm_body（L16 ✓）。
PO 动画目录 **waveeye\ 与 waveeye_light\ 两份同名 34 文件**（光属性变体对，疑雷神之息式元素切换，未考证）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | WaveEye.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\WaveEye.skl` | ✅（311 行） | 14 列全自明 |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | §2.1 |
| 常量 | swordman_header.nut 行 259-266 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 动画 89-96 |
| 主 nut | —（不存在，白名单 grep 实证） | `…\pvf\sqr\character\swordman\` | ⛔ 缺失 | 行为引擎内置 |
| .chr 条目 | etc motion #89-96（行 1062-1069）+ etc attack ×4（行 1357-1360） | `…\pvf\character\swordman\swordman.chr` | ✅ | 8 动画 + 4 atk |
| 角色 .ani | waveeye 系 8 个 | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | waveeyeattack1/2/3.atk + waveeyewing.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 普攻三段（magic/push30-40/lift30）+ 光翼（push200/lift100/cut+blood50） |
| PO lst | 20055-20058（行 11243-11250） | `…\pvf\passiveobject\passiveobject.lst` | ✅ | ID→obj |
| PO 定义 | waveeye/waveeyeclaw/waveeyespear/waveeyewheel.obj | `…\pvf\passiveobject\character\swordman\` | ✅ | §2.3 |
| PO .ani | waveeye\ 与 waveeye_light\ 各 34 个 | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | §2.4 |
| PO .atk | waveeye/waveeyeclaw/clawfinal/spear/wheelstay/wheelmove.atk ×6 | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | §2.3 |
| 施法特效 | waveeye\ 9 个 | `…\pvf\character\swordman\effect\animation\waveeye\` | ✅ | 领域/普攻/光翼层 |
| 装备层 | *waveeye*.ani ×608 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层（8 动作 ×76） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色 8 动画 | 必需（共享） | ✅ 已在库 |
| eye.img / eye_none / eye_ldodge.img | sprite_character_swordman_effect_waveeye.NPK | 天眼本体（终结+领域） | **必需** | ❌ |
| 18eye_none / 18eye_ldodge.img | 同上 | 终结多眼阵 | **必需** | ❌ |
| background.img / slash.img | 同上 | 领域背景/斩击 | **必需** | ❌ |
| wing_up / wing_down.img | 同上 | 光翼上下层 | **必需** | ❌ |
| beam1 / beam2.img | 同上 | 天照地刺光柱 | **必需** | ❌ |
| chakra / chick / light / tail.img | 同上 | 领域轮/终结辅助 | 可选 | ❌ |
| glass_break / glass_part / siva_finish_twinkling / siva_glass_part.img | 同上 | 终结玻璃碎裂系 | 可选 | ❌ |
| A_weapon01.img | 同上 | 形态武器 | 可选 | ❌ |
| floor_circle.img | sprite_character_swordman_effect_hundredsword.NPK（跨技能借图，L14） | 领域地圈 | 可选 | ❌ |
| p-01~p-04.img | sprite_monster_casillas_appear.NPK（**跨 Monster 借图**，L14） | 终结粒子 | 可选 | ❌ |
| supergirl_change_glows.img | sprite_character_mage_effect_bellatrix.NPK（跨职业借图） | 变身光 | 可选 | ❌ |
| MonoLayer.img | sprite_common_etc.NPK | 领域底层 | 可选 | ❌ |

缺失 img：必需级 9 张 + 可选级 12 张（主 NPK 一次提取全覆盖；跨 NPK 借图 7 张）。
img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案（深简化"压缩时序版"：开眼 → 领域区 5 秒 → 天眼终结爆炸；40s 形态改造整体降级，见 §7）

### 内容件清单

1. **`DotNet~/Skills/WaveEyeSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发 + 087 Blache 站桩引导版范式）
   - `CooldownMs=200000`（demo 缩 30000）；`TotalTimeMs=7000`（**时序压缩决策先行**：开眼 880 + 领域 5000 + 终结 1120，40s 压到 7s，同 087 惯例）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanWaveEyeCast)`（480ms）→ 视图自然推进；`ctx.SetSubState(0)`；
     `ctx.AddBuffToSelf(BuffIds.WaveEyeMark)`（40s 形态标记——demo 占位，无消费方，为形态门面预留）。
   - OnUpdate（ElapsedMs + SubState）：
     - `≥880 && SubState==0`：`ctx.PlayAnim(AnimId.SwordmanWaveEyeStart)`（开眼 400ms）+
       `ctx.CreateArea(AreaIds.WaveEyeDomain, ctx.GetTargetPosition())`（领域区自治，不随人）；`SetSubState(1)`。
     - `≥5880 && SubState==1`：终结——`ctx.CreateArea(AreaIds.WaveEyeFinish, 施法点)` + `SetSubState(2)`。
   - OnEnd：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/WaveEyeDomainArea.cs : AreaDefinition`**（暗黑领域：debuff 占位 + 轻伤害，同 FireCircleArea Tick 范式）
   - `TotalTimeMs=5000`、`TickTimeMs=1000`、`EnterActions={MeleeHit}`、`TickActions={MeleeHit}`；
   - `HalfExtents=(7.0,1.0,7.0)`（col10 降命中范围 700px 直译 ÷100）；
   - `HitReaction{Damage=10, HitstunMs=0, KnockbackX=0, LaunchY=0}`——**DNF 是降命中/降攻速 debuff，
     命中率系统不存在 + 攻速系统不存在 → 轻伤害 tick 占位**（087 沼泽同处理）；
   - `ViewAnimId=AnimId.WaveEyeDomain`（bottomlayer 循环层）+ `ViewBackAnimId=AnimId.WaveEyeAura`（aurarange 背层）。
3. **`DotNet~/Areas/WaveEyeFinishArea.cs : AreaDefinition`**（天眼终结爆炸）
   - `TotalTimeMs=1120`、`EnterActions={MeleeHit}`、`HalfExtents=(7.0,1.5,7.0)`（全领域级）；
   - `HitReaction{Damage=500, HitstunMs=1000, KnockbackX=200, LaunchY=300}`（waveeye.atk 原值 push200/lift300）；
   - `ViewAnimId=AnimId.WaveEyeFinishHit` + overlay 手组装 finisheye/glass 层（可选，先主层）。
4. **（可选后补）天照/光翼/闪枪/刺轮四变异技**：各自独立小技能（Area/Bullet 直译，atk 全在库）——
   依赖形态门面（按 WaveEyeMark 切换），第一期不做，见 §7。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎形态状态（40s 开眼） | WaveEyeMark Buff 标记（30/40s）+ 压缩时序演出（7s） |
| 领域 debuff（降命中/降攻速） | 命中率/攻速系统不存在 → DomainArea 轻伤害 tick 占位（087 同） |
| **普攻替换（magic 三段）** | **普攻行为替换门面缺失（本批 082 上报）**——第一期不做 |
| **三系波动剑变异 + 普攻尾击刺轮** | 同上（技能按标记变体分派）——后补件，判定/atk/动画已全录（§2.2） |
| 波动印生成联动 | 无印系统（wave 系联动，021 波动印记同族缺口）——跳过 |
| PO 20058 天眼爆炸 | WaveEyeFinishArea 一次性结算 |
| 杀意波动状态门槛 | Buff 查询门面（R1-A3）——demo 不设门槛 |
| 终结 20+ 层视觉 | 主层 + overlay 渐补 |

### 注册点清单（草案号段，B5 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.WaveEye=32` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanWaveEyeCast=169、SwordmanWaveEyeStart=170、WaveEyeDomain=171、WaveEyeAura=172、WaveEyeFinishHit=173、WaveEyeClaw=174、WaveEyeWing=175、WaveEyeSpear=176、WaveEyeWheel=177（后四个为形态期后补件预留） |
| AreaId | `AreaDefinition.cs` | WaveEyeDomain=36、WaveEyeFinish=37、（WaveEyeClaw=38 预留） |
| BuffId | `BuffDefinition.cs` | WaveEyeMark=17 |
| json / 图集 | LSAnimClipRegistrar / BuildAtlas | json ×5（一期）；img 必需 9 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 200000ms | 200000（演示可缩 30000） |
| 形态持续 | 40s（col13） | 标记 40s 照挂；演出压缩 7s |
| 领域 | 700-850px；降命中 5.7%+/降攻速 80% | 7.0 单位区，tick 10/s 占位 |
| 普攻三段（形态） | col2/3/4 1186/1335/1481%；atk push30-40/lift30 | 一期不做 |
| 天照 | col5 755%~；claw.atk push100/lift200（拉向自身） | 预留 |
| 光翼 | col6 3128%~；wing.atk push200/lift100/cut | 预留 |
| 闪枪 | col7 1055%~；spear.atk push200/lift100 | 预留 |
| 刺轮 | col8 341%~；wheel push0-100/lift100 | 预留 |
| 天眼爆炸 | **col9 64978%~34 万%**；waveeye.atk push200/lift300 | 500/硬直 1000/击退 200/浮空 300 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| WaveEye.skl（14 列 + 9 static 槽） | `.skl` 无子命令（既有） | 16 向量模板行全部自明（本批语义最完整的 skl 之一），手抄零负担 |
| 10 个 .atk（角色 4 + PO 6） | `.atk` 无子命令（既有）；waveeyeattack1 的 **[hit info] 空值行**（`` `` -1 1.5）需容错 | 手抄；空值行与 086 同一解析容错项 |
| 4 个 .obj（其中 claw/wheel 有 etc attack info 二段） | `.obj` 无子命令（既有） | 手工映射；claw 二段（claw/clawfinal）按 L9 多相位→双 Area |
| 全部 .ani（~85 个，含 waveeye/waveeye_light 双目录） | 节面常规 | 现有 ani 子命令全覆盖 |

计 2 条既有缺口 + 1 条解析容错（同 086），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **40s 形态改造：普攻 magic 化 + 三波动剑变异 + 尾击刺轮** | **普攻行为替换门面 + 技能按 Buff 变体分派**（本批 082 上报新缺口的两面） | 一期只做"开眼+领域+终结"；四变异技后补（数据已全录 §2.2，每个都是独立小技能的量） |
| 领域降命中/降攻速 | 命中率系统 + 攻速系统均不存在（087 沼泽同族） | 轻伤害 tick 占位 |
| 杀意波动状态门槛（52 前置） | Buff 查询门面（R1-A3） | 不设门槛 |
| 天照"拉向自身" | 位移他人门面（R2-A8） | 后补件用负击退近似（L22） |
| 领域光环跟随施放者 40s | Buff 视觉挂接（R1-A5）+ Area 跟随施法者（R4-A17） | 定点 Area（不随人）——领域固定于施法点 |
| 变异技独立 CD（3.5s/2s/7s） | 依赖形态门面（形态期才有 CD 槽） | 后补件期再议 |
| 光属性（claw.atk light element）/无属性混用 | 元素属性系统缺失 | 无属性直伤 |
| 40s 长时序 | 技能期间放开控制缺失（087 同） | 压缩 7s 站桩版 |
| 音效/屏震/读条 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static 槽 0-5（0 0 0 50 50 30）语义（槽 6/8 已由模板印证=天照/闪枪 CD；槽 3/4/5=50/50/30 疑与领域/眼数相关）。
2. 终结天眼爆炸的触发时点（形态到期自动爆/再按键手动爆）——引擎内置不可见；explain"最后"语义模糊。
3. waveeye 与 waveeye_light 双动画目录的分工（疑光属性元素切换变体，雷神之息同族，未实证）。
4. 波动印生成的联动消费（"生成一个波动印"——与 wavemark 系（021）的关系未考证）。
5. col1 攻速减 80%/100% 两档与 col0 降命中的实际引擎公式（×0.1 读法为模板行推断）。

**系统级缺口**
- **普攻行为替换/技能变体分派**：与 082 卡洛同一缺口的两面（查询 + 行为注入/变体分派）——本技能是"整技能组按形态切换"的最大用例（1 普攻 + 3 波动剑 + 1 尾击 = 5 个技能受一个形态控制）。建议与 082 的上报合并立项：`SkillLogic 虚属性 IsFormActive(formId)` + SkillCastHelper 分派。
- 领域 debuff：命中率/攻速双系统缺失的又一实证（087 沼泽第 2 例）。

**给下轮的经验**：纯引擎内置一觉（无注册行）先抄 header 的 CUSTOM_ANI_* 常量族——**一个技能的常量族连号（89-96）就是它的全部角色动画清单**；PO 编号 2005x 紧邻段（20055-20058）按 lst 顺序连查可一次拿全。waveeye_light 双目录形态首见，翻译时两份都要入 json（元素变体切换用）。
