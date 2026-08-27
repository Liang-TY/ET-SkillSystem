# 极冰·裂波剑（IceWaveEx）

> 技能ID 100 | 级别 E（**预分类纠偏：非 TP 强化被动——[type] active、skill class 1，阿修罗二觉替换型主动技**，按 B 类深度走���；同目录 216-IceWaveExp 才是"强化-冰刃·波动剑"TP 被动） | 可实现性 🔶（链式冰柱群主干可用"技能逐拍建弹/建区"表达（R4-A17 过渡范式）；同柱多段命中重置与柱内 tick 间隔未考证，按固定 tick 简化） | 分析日期 2026-08-22 | 批次 E5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极冰 · 裂波剑 | `skill\Swordman\IceWaveEx.skl` [name] |
| 英文名 | IceWaveEx（取 skl 文件名；[name2] 实测 `Super Ice Wave Sword`） | 同上 |
| 职业 | 阿修罗二觉（[second growtype maximum level] 12 槽第 8/9 位（0 基）= **30 级**——(8,9)=阿修罗对，R6-C4；[skill fitness growtype] 空） | 同上 |
| 学习等级 | 60（[required level range] 2）；前置 **22 爆炎·波动剑 Lv1**（[pre required skill] 实测——⚠ 官方语义疑为冰刃 21，本 pvf 数据如此，存疑） | 同上 |
| 最高等级 | 50（[maximum level]；二觉档实际 30） | 同上 |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | →←↓→ + Z（[skill command advantage] 50/50） | 同上 [command] |
| CD | 30000 ms | 同上 [cool time] |
| MP | 400 → 1120 | 同上 [consume MP] |
| 读条 | 300 ms | 同上 [casting time] |
| 特殊消耗 | 道具 3037 ×1（无色）；耐久 25 | 同上 |
| static data | `45 500 65 1 145`——[0]=45（面板"生成间隔 0.045s"）、[1]=500（僵直 0.5s）、[2]=65（冰柱间隔 65px）、[3]=1（未解）、[4]=145（爆炸大小 145%）——**注意：脚本侧硬编码 10ms/75px/3000ms 与此不同源**（§2.3），static 疑仅面板显示/引擎另路消费 | 同上 + level property |
| 一句话效果 | 发射沿地面推进的冰属性小冰柱群（上下双道 × 各 7 柱），穿透多段伤害并概率冰冻；每柱 3 秒后原地冰爆（基础冰刃的强化重制） | 同上 [explain] + 走读 |
| 与基础技关系 | [pre required skill] 22 单向依赖；基础 IceWave.skl [feature skill index]=216（TP 版）——与 100 无链接 | 两 skl 实测 |

**level property 模板解码（8 列 + 11 向量，L21 法全解，Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 魔法攻击力（每柱穿透伤） | **(-2,3,1.0)** | col3 = 1579 → 17878 % |
| 爆炸攻击力 | **(-2,7,1.0)** | col7 = 1994 → 22552 % |
| 小冰柱数量 | (-1,1,1.0) | col1 = **7 恒定** |
| 冰冻几率 | (-1,4,0.1) | col4 = 300→5200 → **30% → 520%** |
| 冰冻Lv | (-1,5,1.0) | col5 = Lv62 → Lv160 |
| 冰冻持续时间 | (-1,6,0.001) | col6 = 2500→27000 → **2.5 → 27 s** |
| 每个冰柱持续时间 | (-1,0,0.001) | col0 = 800→5700 → 0.8→5.7 s（疑面板列，脚本 maxT=3000 恒定） |
| 每个冰柱生成间隔 | (0,0,0.001) | static[0]=45 → 0.045 s（脚本实测 10ms，见 §2.3 存疑） |
| 施放后自身的僵直时间 | (1,1,0.001) | static[1]=500 → 0.5 s |
| 冰柱之间的间隔 | (2,2,1.0) | static[2]=65 px（脚本实测 75px） |
| 爆炸大小 | (4,4,1.0) | static[4]=145 → 145% |
| （col2 无模板行） | — | 200→690 成长列，**未考证**（疑爆炸参数/多段间隔类） |

-2 源两见（col3/col7——恰是 wave.nut 实取的两列，nut 佐证直读），L21 的 -1/-2 差异本案仍无解但不影响取值。

## 2. 技能逻辑走读

### 2.1 注册与文件链（共用波动剑状态——F1 族）

无独立注册：波动剑系共用一条（load_state:16-17，021 §2.1）——
`pushState(0, "character/Swordman/wave/wave.nut", "WaveSword", 24, -1)` + `pushPassiveObj("character/Swordman/wave/po_wavecut.nut", 24328)`。
施法侧分流按**子状态 var state[0]==100**（技能 ID 即子状态号），弹体按**包内 id==125** 分支——021 示范文档当年走读的就是本技分支（"强化冰刃"即此）。

### 2.2 施法侧（wave.nut onKeyFrameFlag_WaveSword，全读）

```
state[0]==100：
  atk   = sq_GetPowerWithPassive(100, 24, 3, -1, 1.0)   // col3 每柱伤害
  atk2  = sq_GetPowerWithPassive(100, 24, 7, -1, 1.0)   // col7 爆炸伤害
  count = sq_GetLevelData(100, 1, lv)                   // col1 = 7 柱
  dist=75, size=100, maxT=3000                          // 距离/大小/柱寿命（硬编码）
  prob=col4/10, lv=col5, time=col6                      // 冰冻三参
  写包 12 值 → 创建 PO 24328 于 x=75, y=-15 与 y=+15（上下双道，各链 7 柱）
```

### 2.3 弹体侧（po_wavecut.nut id 125 分支，全读——链式冰柱机制核心）

```
setCustomData（每柱出生）：
  播空占位动画 zrr_skill\newswordman\animation\icewave.ani（5 帧全空 IMAGE，L7）
  attackInfo = sq_GetCustomAttackInfo(obj, 103)（自有表，L3；PO attackinfo\ 有 icewave.atk/icewaveex.atk 候选——push 30/lift 100/water/freeze 运行时写入）
  伤害 = 写包 col3；判定盒按 size/100 缩放（sq_SetAttackBoundingBoxSizeRate）
  特效 = createIceWaveExAnimation：按 size 选 icewaveex/1~6.ani（3 帧 240ms，带 .als 加法层）
  冰冻 = sq_SetChangeStatusIntoAttackInfo(..., ACTIVESTATUS_FREEZE, prob, lv, time)（L6 标准链）
procAppend（每帧）：
  currentT ≥ 10ms（一次性）：若 currCount < maxCount → 在前方 dist=75px 处创建下一柱
    （写包同构、currCount+1）——**链式自复制：波列视觉 = 柱逐个接力出生**，非单体移动
  currentT ≥ maxT=3000ms（一次性）：爆炸——
    createIceWaveExExplosionEffect(200)（icewaveexiceexplosionparticle2.ani 池化视觉）
    + sq_createAttackObjectWithPath(icewaveexiceexplosion.ani  [1 帧空图带攻击盒] ,
      icewaveexiceexplosion.atk [magic/water/damage/push200/lift100/blow], 伤害=col7, size)
    → 自毁
onAttack（每次命中）：目标可定身且非霸体 → 挂 ap_wavehold（200ms hold + 2s 有效）——命中即短定身
destroy：清特效对象
```

### 2.4 动画与资源（icewaveex\ 43 文件 + 根目录爆炸件，抽关键实测）

| 动画 | 帧数 | 时长 | 引用 img | 备注 |
|---|---|---|---|---|
| `passiveobject\...\animation\icewaveex\1~6.ani`（柱本体，按 size 档位选） | 3 | 240ms | `Effect/IceWaveEx/ice_apply_dodge.img` + `ice_dodge_middle.img` | LOOP；各带 .als（ice{N}add_eff_a/b 叠层 → **LightningPOW_ADD_A.img**） |
| ice_dodge_middle1-6.ani | 6 | — | + `ice_dodge_up.img` | 柱分层变体（.als 引用） |
| ice_normal_down1-6.ani | 6 | — | `ice_normal_down.img` | 落地层 |
| ice_dust1/2.ani | 7 | — | `Character/Mage/Effect/IceField/dust.img` | 尘土（跨职业借图） |
| light_1/2.ani | 7 | — | `Effect/LightningGod/light.img` | 闪光层 |
| `…\animation\icewaveexiceexplosion.ani`（根目录） | 1 | 400ms | **空**（带攻击盒——爆炸判定体，L7 占位） | 另有 (diff) 变体 |
| icewaveexiceexplosionparticle2.ani（根目录） | — | — | 粒子视觉件 | 爆炸视觉（池化对象播放） |
| zrr_skill\...\icewave.ani（弹体时间轴） | 5 | — | **空**（L7 纯占位） | 判定走 attackInfo 103 |
| 施法姿态 | 共用波动剑姿态（021 §2.4 未定位，共用未考证） | — | sm_body | — |
| 粒子 | icewaveexiceexplosionparticle1-4.ptl、icewaveexparticlesmoke.ptl（5 个） | — | .ptl | L5：翻译+系统双缺口，跳过 |

[skill preloading image]：`IceWave1.img`、`IceWave2.img`、`IceWaveParticle{,Smoke,Star}.img`（**基础冰刃的资产清单**——柱视觉真用图 Effect/IceWaveEx/* 未列入，mod 换皮残留同 075 模式）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | IceWaveEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\IceWaveEx.skl` | ✅（253 行全读） | 8 列全解 |
| lst 条目 | ID 100 | `…\pvf\skill\swordmanskill.lst` 371-372 行 | ✅ | — |
| 注册行 | load_state:16-17（共用 WaveSword/24328） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅（021 已证） | F1 族共用 |
| 施法 nut | wave.nut（state[0]==100 分支） | `…\pvf\sqr\character\swordman\wave\wave.nut` | ✅ 全读 | §2.2 |
| 弹体 nut | po_wavecut.nut（id 125 分支） | `…\pvf\sqr\character\swordman\wave\po_wavecut.nut` | ✅ 全读 | §2.3 |
| hold appendage | ap_wavehold.nut | 同目录 | ✅（021 已证） | 命中定身 |
| PO .atk | icewave.atk / icewaveex.atk / icewaveexiceexplosion.atk / _item.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | 爆炸 atk 直读；103 表与 icewave.atk/icewaveex.atk 的对应关系未考证（两者仅 hit wav 名不同） |
| PO .ani/.als | icewaveex\ 43 文件 + 根目录 icewaveexiceexplosion*.ani | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 空占位 | zrr_skill\newswordman\animation\icewave.ani | `…\pvf\passiveobject\zrr_skill\newswordman\animation\` | ✅（5 帧空图） | L7 |
| 粒子 | icewaveex*.ptl ×5 | `…\pvf\passiveobject\character\swordman\particle\` | ✅ 存在 | L5 跳过 |
| .chr / 角色 .ani/.atk | —（共用波动剑，无 icewaveex 条目） | `…\pvf\character\swordman\` | ⛔ 无 | 021 同结论 |
| 基础技文档 | 021-IceWave.md | 本目录 | ✅ | 结构对照 + 本技分支当年已走读 |
| 同名 TP 技 | 216-IceWaveExp.md | 本目录 | ✅ | TP 版（交叉引用） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/IceWaveEx/ice_apply_dodge.img`、`ice_dodge_middle.img` | sprite_character_swordman_effect_icewaveex.NPK | 冰柱本体（1-6.ani 主图） | **必需** | ❌ |
| `…/IceWaveEx/ice_dodge_up.img`、`ice_normal_down.img` | 同上 | 柱分层/落地层 | 可选 | ❌ |
| `Character/Swordman/Effect/LightningPower/LightningPOW_ADD_A.img` | sprite_character_swordman_effect_lightningpower.NPK | 柱加法混合叠层（.als a/b ×6 全引用） | 可选（视觉增强） | ❌ |
| `Character/Swordman/Effect/LightningGod/light.img` | sprite_character_swordman_effect_lightninggod.NPK | 闪光层 | 可选 | ❌ |
| `Character/Mage/Effect/IceField/dust.img` | sprite_character_mage_effect_icefield.NPK | 尘土（跨职业借图） | 可选 | ❌ |
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法姿态（共用） | 必需（共享） | ✅ |

**缺失 img：必需 2 张（同一 NPK）、可选 5 张（跨 4 NPK）。** AnimRes 实测均未入库。

## 5. 实现方案草案（号段：SkillIds 35 / AnimIds 191-193 / AreaIds 42-43，E5 批内顺延；撞号无妨 L18）

### 内容件清单

1. **`DotNet~/Skills/IceWaveExSkill.cs : SkillLogic`**（WaveSwordSkill 范式 + 逐拍建弹扩展）：
   - `CooldownMs=30000`；`TotalTimeMs=800`（读条跳过、僵直 0.5s + 余量）。
   - `OnCast`：`ctx.PlayAnim(波动剑共用姿态)`；设 SubState=0、计时器清零。
   - `OnUpdate`（**链式出生改技能驱动**——R4-A17"逐拍建区"过渡范式，替代弹体自复制）：
     双道（y±1.5 单位）各自按 **每 50ms 一柱 × 7 柱 × 前进 75px（7.5 单位→每柱 +0.75 单位）** 依次
     `ctx.CreateArea(AreaIds.IceWaveExPillar, 位置)`——柱为静态区（无移动），出生序列还原"波列推进"观感。
   - `HitActions={MeleeHit, AddFreezeBuff}` 挂在柱区（见下）。
2. **`DotNet~/Areas/IceWaveExPillarArea.cs : AreaDefinition`**（每柱=静态多段区）：
   - `TotalTimeMs=3000`（maxT 直用）；`TickTimeMs=500`（**柱内多段 tick 间隔未考证**——hold 200ms 旁证取保守 500ms demo 值）；
     `TickActions={MeleeHit}`；`HalfExtents=(0.8,0.4,0.5)`（icewave.atk push30/lift100 波形 + 判定盒 size 缩放近似）。
   - `HitReaction{Damage=160, HitstunMs=0, KnockbackX=30, LaunchY=100, ProcBuffId=BuffIds.Freeze, ProcChance=30}`（icewave.atk + col4=30% Lv1 档；FreezeBuff 复用）。
   - `ViewAnimId=AnimId.IceWaveExPillar1`（icewaveex/1.ani 240ms 循环）；`ViewEndAnimId` 缺失（见 §7）。
   - **爆炸 = 区中区**：柱到期时由 SkillLogic OnUpdate 同帧对每柱位置补 `ctx.CreateArea(AreaIds.IceWaveExBoom, 柱位)`（Skills 侧有全部柱位记录）——绕开"Area 派生 Area"缺口。
3. **`DotNet~/Areas/IceWaveExBoomArea.cs : AreaDefinition`**（每柱终爆）：
   `TotalTimeMs=400`（爆炸 ani 时长）、`EnterActions={MeleeHit}`、`HalfExtents=(1.4,0.4,1.4)`（爆炸大小 145% 折算）、
   `HitReaction{Damage=200, HitstunMs=500, KnockbackX=200, LaunchY=100}`（icewaveexiceexplosion.atk 直译；Damage=col7 Lv1 1994% demo 折算）、
   `ViewAnimId=AnimId.IceWaveExBoom`（icewaveexiceexplosionparticle2.ani——空图爆炸 ani 不可用，取粒子视觉件替代）。
4. **无需**新 Buff/Action（Freeze 复用；多段走 Tick——同段定时多段 = L19 双同心 Area Tick 已验证形态）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 双道链式柱（PO 24328 自复制 +75px/10ms） | SkillLogic 逐拍建区（14 柱按 50ms 节奏 × 2 道；个体性保留、自复制改为集中调度） |
| 每柱 3s 存续 + 多段命中（引擎重置） | Area Tick 500ms × 6 跳（间隔未考证取 demo 值；L19 同段定时档） |
| 柱终爆（icewaveexiceexplosion.ani + atk） | 独立 BoomArea（柱位已知，技能侧补建） |
| ACTIVESTATUS_FREEZE（prob/lv/time） | `HitReaction.ProcBuffId+ProcChance`（L6 已支持）+ FreezeBuff 复用 |
| ap_wavehold 命中定身 200ms | 并入 Freeze（021 同款简化） |
| 判定盒随 size 缩放（sq_SetAttackBoundingBoxSizeRate） | 固定 HalfExtents（对象整体缩放延后档，021 同结论） |
| 读条 300ms / 僵直 0.5s | 跳过 / TotalTimeMs 吸收 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.IceWaveEx = 35` + 按键 |
| AreaId | `Runtime\AreaDefinition.cs` | `IceWaveExPillar = 42`、`IceWaveExBoom = 43` |
| AnimId | `AnimConfigRegistry.cs` | `IceWaveExPillar1=191、PillarDodge=192、Boom=193` |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | 3 json；icewaveex NPK 2 张必需 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000 ms | 30000 直用 |
| 柱数/道 | 7（col1） | 7 |
| 柱间距/出生间隔 | 75px / 10ms（脚本实测；skl 面板 65px/45ms 不同源） | 0.75 单位 / 50ms |
| 柱寿命 | 3000 ms（脚本 maxT；面板 col0 0.8-5.7s 不同源） | 3000 |
| 每跳伤害 | col3 1578%→17878% | 160 |
| 冰冻 | 30%→520% / Lv62→160 / 2.5→27s | 30% / Freeze 2.5s（等级跳过） |
| 爆炸伤害/反应 | col7 1994%→22552%；push200/lift100/blow | 200 / Kb 200 / Ly 100 / Hitstun 500 |
| 爆炸大小 | 145%（static[4]） | HalfExtents 1.4 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| IceWaveEx.skl | `.skl` 无子命令（8 列 + static 5 值 + 二觉 12 槽） | 手抄 §1 全解；skl 子命令同前议 |
| icewave.atk / icewaveex.atk / icewaveexiceexplosion.atk | `.atk` 无子命令（freeze 运行时写入位） | 手抄（每文件 ≤8 值） |
| po_wavecut 自有表 103 | `sq_GetCustomAttackInfo(obj,103)` 的表→文件映射关系不可走读（引擎内部） | §5 直接采 PO attackinfo 目录候选值（push30/lift100 两候选仅 hit wav 异） |
| icewaveex*.ptl ×5 | `.ptl` 无子命令 | L5 既有缺口：跳过（视觉用 ani 替代） |
| 全部 .ani/.als | FRAME/IMAGE/DELAY/LOOP/SHADOW/[add]/[use animation] | **现有 ani/als 子命令全覆盖**（SHADOW 记档跳过） |

翻译缺口计 3 类（.skl/.atk/.ptl——均常驻缺口，无新节）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 柱自复制链（PO 派生 PO，10ms 接力） | LSActionContext 无 CreateArea（R6-C5，第 3 消费方之后的又一形态——**弹体自复制**） | 技能集中调度逐拍建区（柱位可计算，几何等价；10ms→50ms 节奏差肉眼难辨） |
| 柱内多段命中（引擎重置间隔未考证） | 多段命中重置延后档（L19 同段定时=Tick 可表达） | 500ms tick 固定（间隔未考证记 §8） |
| 爆炸视觉 = 空图 ani + 粒子层 | 粒子系统缺失（L5）+ Bullet/Area 无 ViewEndAnimId（R2-A6 小项） | BoomArea 自带 ViewAnimId（独立区即"收尾动画"），粒子跳过 |
| 判定盒随 size 缩放 | 对象整体缩放（延后档） | 固定盒 |
| 冰冻 Lv62-160 等级对抗 | 状态等级 Lv 系统（R6-C4） | 固定概率+时长 |
| 双道 y±15 | 无（CreateArea 支持 y 偏移或两次创建） | 直译 |

## 8. 存疑与缺口上报

**未考证项**
1. **skl static/col0 面板值与脚本硬编码不同源**（45ms/65px/0.8-5.7s vs 10ms/75px/3000ms）——static 疑仅 TP/等级面板显示用，运行时以 wave.nut 硬编码为准；col2（200→690 成长列）无任何引用，语义不明。
2. 自有表 103 与 PO attackinfo 四个 .atk 的精确对应（icewave.atk 与 icewaveex.atk 仅 hit wav 名不同，push30/lift100 相同——不影响实现取值）。
3. 柱内多段重置间隔（引擎内部；hold 200ms 是旁证非直证）。
4. [pre required skill] 22（爆炎）而非 21（冰刃）——疑 mod 改写或官方数据如此。
5. ice1add_eff_a/b（LightningPOW_ADD_A 叠层）与冰属性的语义关联（021 §4 遗留同疑）。
6. icewave1-6.ani（animation 根目录，含 .als/pvp/add 变体）属于基础冰刃的波列视觉族还是本技复用（未逐一对表）。

**缺口上报**：**"弹体自复制"（PO 派生 PO）**——LSActionContext 无 CreateArea 缺口的新消费形态（R6-C5 已记 Buff Tick 光环/96 结晶两形态后第 3 形态：链式接力弹幕）；本档用技能集中调度绕过，立项时列受益清单。

**预分类纠偏上报（主循环记账）**：98/100/102 同批三 Ex 均为二觉替换主动技（详见 098 §8）；100 即 021 示范文档当年走读的 state[0]==100 分支本体——021 §2.2 标注的"强化冰刃（lst 实测 ID 100）"即本技，非 TP。

**给轮间经验**：F1 波动剑族的 Ex 替换技（100 极冰）与 TP 强化（216）**共用 wave.nut 施法分支编号体系**（子状态=技能 ID）——后续 FireWaveEx 99/BloodBlastEx 101 直查 wave.nut 对应 state 分支即可。
