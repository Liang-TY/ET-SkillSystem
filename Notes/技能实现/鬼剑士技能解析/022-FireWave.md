# 爆炎·波动剑（FireWave）

> 技能ID 22 | 级别 A | 可实现性 ✅（直接，判定形状改用实证 Area 方案） | 分析日期 2026-08-22 | 批次 A10

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 爆炎 · 波动剑 | `skill\Swordman\FireWave.skl [name]` |
| 英文名 | FireWave（取 skl 文件名；[name2]="波动剑 暴炎" 是中文别名，L1） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4，L17） | 同上 |
| 学习等级 | 35 | 同上 [required level] |
| 最高等级 | 70（各觉醒段上限：growtype4 = 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | ←↓→ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 15000 ms（pvp 20000 / 起手 20000） | 同上 [dungeon][cool time] / [pvp] |
| MP | 130 → 1092（Lv1→Lv70） | 同上 [consume MP] |
| 施法时间 | 400 ms（casting time，站桩前摇） | 同上 [casting time] |
| 特殊消耗 | 无 | 同上 |
| 前置 | 冰刃·波动剑（21）Lv3 | 同上 [pre required skill] |
| 一句话效果 | 向前发射烈焰波（火属性魔法）击退敌人；烈焰波触地时发生爆炸造成二段魔法伤害；施放时可生成波动印 | 同上 [explain] |

**static data**：dungeon `350` / pvp `450`（单值；语义未考证——基础施法引擎内置，无脚本消费可读；推断与爆炸延迟或波动尺寸相关）。

**level property 模板**（2 占位符，两向量 `(-2,0,1.0)`、`(-2,1,1.0)`）：

```
魔法攻击力 : <int>
爆炸魔法攻击力 : <int>
```

按 L21 解码法 + 本例实证补充：**向量首值 -2 = 指向 [level info] 列**（第二值=列号）——
- level info col0 = 魔法攻击力：2873（Lv1）→ 23413（Lv70）
- level info col1 = 爆炸魔法攻击力：5344（Lv1）→ 42746（Lv70）

（L21 原规则：首值 ≥0 = static data 槽、-1 = 常量基准+成长；本例新增 **-2 = level info 列引用**，与 2 列 level info 一一对应，交叉印证成立。）

**[skill preloading image]** 节：`Character/Swordman/Effect/FireWaveFloor.img`、`Common/CommonEffect/Fire.img`、`Common/CommonEffect/FireLight.img`（预载清单，已记档的翻译缺口节；FireWaveFloor.img 未被任何已读 .ani 引用，用途未考证）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**F1 波动剑族：无独立 pushState**。全族共用一条注册（`sqr\character\swordman_load_state.nut` 行 16-17，021-IceWave.md 已录）：

```
16: IRDSQRCharacter.pushState(0, "character/Swordman/wave/wave.nut", "WaveSword", 24 ,-1 );
17: IRDSQRCharacter.pushPassiveObj("character/Swordman/wave/po_wavecut.nut", 24328);
```

- 施法状态 = WaveSword（状态号 24，技能 ID -1 不绑定）；基础四波（20 地裂/21 冰刃/22 爆炎/24 血气）的施法分流**引擎内置**。
- `wave\wave.nut`（74 行）唯一活跃回调 `onKeyFrameFlag_WaveSword` 只有 **IceWaveEx（技能 100）分支**（写包 id=125）；**FireWave（22）无脚本分支**（实测全文件读毕）。
- `po_wavecut.nut`（179 行）`setCustomData` 同样只有 `id == 125` 分支（021 已录）。
- **新实证（本批次）**：FireWave 的弹体在 mod 共享 PO 24349（F5 unclebang 链路）里另有 id=17 分支（`sqr\shared_passive_object\swordman\setcustomdata.nut` case 17）：
  - 动画 = etc motion #24 → `flame_dodge.ani`（火浪视觉）；攻击信息 = etc attack info #12 → `attackInfo/firewave.atk`（**此段 code=直读 0 基索引，与 windspiritvs 等 VS 族的 -2 错位不同**，见 066 文档 §8）；
  - 伤害 = `sq_GetPowerWithPassive(22, -1, 0)`（= level col0 魔法攻击力）✓；尺寸 = level col1 ÷100（与模板"col1=爆炸魔法攻击力"冲突——见下）；
  - 灼烧 = `sq_SetChangeStatusIntoAttackInfo(..., ACTIVESTATUS_BURN, col5, col6, col7, col8)`——**col5-8 超出本 skl 2 列形状，属 mod 移植死代码**（本 pvf 的 FireWave.skl 只有 2 列，读取必为 0）。基础版 atk（firewavebig.atk）无 [active status]，**基础爆炎波不灼烧**，灼烧只在 Ex/共享变体。
- 基础版（22）的引擎侧创建调用（谁创建 firewavebig PO）不可见——**未考证**（引擎内置）。

### 2.2 主 nut 逐回调（wave.nut 施法侧，对本技能）

无本技能分支。可参照的族行为：
- `onAfterSetState_WaveSword`：若挂有 `ap_wavemark.nut`（波动印记 appendage）→ `sqx_WaveMarkPush(obj,1,1)` 推波动印（"施放时可以生成波动印"的实现；Buff 查询门面缺失，跳过——021 已录同类）。
- 施法动画 = 族共用 `wave.ani`（.chr etc motion #9，见 §2.4）；波在施法结束瞬间由引擎放出（未考证精确帧）。

### 2.3 被动对象（火波动 + 地面爆炸，数据全在 .obj/.act/.atk）

**主判定体 firewavebig.obj**（"火波动"，`passiveobject\character\swordman\firewavebig.obj`）：

| 节 | 值 | 说明 |
|---|---|---|
| [pass type]/[piercing power] | pass all / 1000 | 全穿透多目标 |
| [basic action] | `Action/flame.act` | **行为脚本走 .act**（basic motion 为空） |
| [attack info] | `AttackInfo/FireWaveBig.atk` | 命中反应（下表） |
| [object destroy condition] | on end of animation | 播完即毁 |

**flame.act**（`passiveobject\character\swordman\action\flame.act`，81 行）——每帧行为表：

| 帧 | 行为 |
|---|---|
| F0 | ATTACKRECT RESET + 创建 PO 249951（=flame01.obj，纯视觉，播 flame_dodge.ani）于 x=110 + 屏震 2/150 |
| F1 | 同上，x=210 |
| F2 | 同上，x=310 |

基动画 = `../Animation/ATKBOX.ani`（4 帧 763ms：250/250/250/13，**全帧 RGBA alpha=0 不可见**，LINEARDODGE）——即"行进感"由逐帧铺设的 3 个火焰视觉 PO 拼出，本体只提供扩张判定盒：

| ATKBOX.ani 帧 | 攻击盒（偏移 x,y,z + 尺寸 w,h,d，DNF 像素） |
|---|---|
| F0 | `0 -35 -20 130 64 269` |
| F1 | `0 -35 -20 230 64 269` |
| F2 | `0 -35 -20 400 64 269` |
| F3 | 无（13ms 收尾空帧） |

→ **判定形状实证：前方 750ms 内扩张至宽 400px（≈4 单位）、高 64px、深 269px 的贴地扇盒**，不是高速远射程弹。

**FireWaveBig.atk**（`passiveobject\character\swordman\attackinfo\firewavebig.atk`）：

| 字段 | 值 | → 我们 HitReaction |
|---|---|---|
| attack type / elemental | magic / fire element | 伤害类型（无元素系统，记档） |
| damage reaction | **down** | 长硬直 + 浮空落地的击倒近似 |
| push aside / lift up | 150 / 200 | KnockbackX=150 / LaunchY=200 |
| hit info / no blood | blow / 50 1.9 | 表现层 |

**爆炸体 firewavebigsub.obj**（烈焰波触地/终止时爆炸）：
- basic motion = `Animation/FireWaveBig/blast-front-1.ani`、etc motion = `blast-back-1.ani`——**目录内实际文件是 `blast-front.ani`/`blast-back.ani`（9 帧 630ms / 5 帧 350ms），`-1` 后缀文件不存在**（mod 改名残留或引擎后缀约定���未考证；视觉文件本身在）。
- width `1 1`；atk = `firewavebigsub.atk`：magic/fire/down、**push 300 / lift 200 / stuck -1000**（强吸附定身）。
- 爆炸的创建者引擎侧（未考证触发条件：触地或动画终点）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\wave.ani`（族共用施法，.chr etc motion #9） | 9 | 名义 10500 | F7=65534 | 无 | **F0 delay=10000 超长悬停帧**=施法时间槽（400ms casting time 消耗于此）；实际 ≈400+50×6+150+150 ≈ 1000ms；flag 65534 语义未考证（064 同款） |
| `passiveobject\character\swordman\animation\ATKBOX.ani`（火波动判定） | 4 | 763 | 无 | F0/F1/F2（扩张盒，见 §2.3） | 全帧 RGBA alpha=0（不可见，视觉靠 flame01 铺设）；引 `Flame/flame_dodge.img` |
| `passiveobject\...\flame_dodge.ani`（火焰视觉，flame01.obj 用） | 11 | 770 | 无 | 无 | 引 `Flame/flame_dodge.img` |
| `passiveobject\...\firewavebig\fire-front.ani`（火浪主层） | 7 | 490 | 无 | 无 | 引 `FireWave/fire-front.img` |
| `passiveobject\...\firewavebig\fire-back.ani`（火浪背层） | 6 | 420 | 无 | 无 | 引 `FireWave/fire-back.img` |
| `passiveobject\...\firewavebig\blast-front.ani`（爆炸主层） | 9 | 630 | 无 | 无 | 引 `FireWave/blast-front.img` |
| `passiveobject\...\firewavebig\blast-back.ani`（爆炸背层） | 5 | 350 | 无 | 无 | 引 `FireWave/blast-back.img` |

`.als` 边车：firewavebig 目录仅 `ex_bead_fire_dodge.ani.als`（Ex 系，不在基础版链路）；基础版链路无 .als。
角色侧 `attackinfo\` 无 firewave 条目（实测 grep）——命中参数全在 PO 表（L3 印证）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FireWave.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FireWave.skl` | ✅ 实测 | 等级/CD/MP/2 列 level info |
| 注册行 | swordman_load_state.nut 行 16/17 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | WaveSword 状态 + 24328（族共用；本技能无独立分支） |
| 主 nut | wave.nut | `…\pvf\sqr\character\swordman\wave\wave.nut` | ✅ 实测（74 行全读） | 仅 IceWaveEx 分支；本技能施法引擎内置 |
| 弹体 nut | po_wavecut.nut | `…\pvf\sqr\character\swordman\wave\po_wavecut.nut` | ✅ 实测（179 行全读） | 仅 id=125 分支 |
| 共享 PO 回调 | setcustomdata.nut case 17 | `…\pvf\sqr\shared_passive_object\swordman\setcustomdata.nut` | ✅ 实测 | mod 侧火浪变体（col5-8 死代码） |
| .chr 条目 | etc motion #9 | `…\pvf\character\swordman\swordman.chr` 行 982 | ✅ 实测 | Animation/Wave.ani（族共用施法动画） |
| 角色 .ani | wave.ani | `…\pvf\character\swordman\animation\wave.ani` | ✅ 实测 | 9 帧，F0=10000 悬停 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\` | ⛔ 无 firewave 条目 | 命中参数在 PO 表 |
| PO 定义 | firewavebig.obj / firewavebigsub.obj | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 火波动 + 地面爆炸 |
| PO 行为 | flame.act | `…\pvf\passiveobject\character\swordman\action\flame.act` | ✅ 实测 | 每帧铺 flame01 视觉 + 扩张盒 |
| PO 判定动画 | ATKBOX.ani | `…\pvf\passiveobject\character\swordman\animation\ATKBOX.ani` | ✅ 实测 | 4 帧扩张攻击盒（不可见） |
| PO 视觉 | flame01.obj / flame_dodge.ani | 同上目录 / animation 根 | ✅ 实测（passiveobject.lst 行 18659 定位 249951） | 火焰铺设视觉 |
| PO .atk | firewavebig.atk / firewavebigsub.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | 主波 push150/lift200；爆炸 push300/lift200/stuck-1000 |
| PO .atk（共享表） | firewave.atk | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\attackinfo\firewave.atk` | ✅ 实测 | mod 变体：push400/lift300/lift up 方向 |
| 爆炸 .ani | blast-front.ani / blast-back.ani | `…\passiveobject\character\swordman\animation\firewavebig\` | ✅ 实测 | 630/350ms（obj 引用的 `-1` 后缀名不存在，见 §2.3） |
| 火浪 .ani | fire-front.ani / fire-back.ani | 同上 | ✅ 实测 | 490/420ms |
| 装备层 | wave 系 ani ×162（coat 层抽样） | `…\pvf\equipment\character\swordman\avatar\coat\*\` | ✅ 实测（存在性） | 换装图层（demo 不需要） |

## 4. 资源需求

NPK 推导规则：`sprite_<img 路径下划线化>.NPK`（01§2 Step 4）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/Flame/flame_dodge.img` | sprite_character_swordman_effect_flame.NPK | 火浪视觉（flame01 铺设 + ATKBOX） | **必需** | ❌ |
| `Character/Swordman/Effect/FireWave/fire-front.img` | sprite_character_swordman_effect_firewave.NPK | 火浪主层 | **必需**（视觉主表现） | ❌ |
| `Character/Swordman/Effect/FireWave/blast-front.img` | 同上 | 爆炸主层 | **必需** | ❌ |
| `Character/Swordman/Effect/FireWave/fire-back.img` | 同上 | 火浪背层 | 可选 | ❌ |
| `Character/Swordman/Effect/FireWave/blast-back.img` | 同上 | 爆炸背层（ViewBackAnimId 同构） | 可选 | ❌ |
| `Character/Fighter/Effect/NenGuardEx/Glow.img`、`Spark.img` | sprite_character_fighter_effect_nenguardex.NPK | glow.ani/spark.ani 借用格斗家贴图（L14 跨目录复用常态） | 可选 | ❌ |
| `Character/Swordman/Effect/LightningGod/light.img`、`light_2.img` | sprite_character_swordman_effect_lightninggod.NPK | light_1/2.ani 借用 | 可选 | ❌ |
| `Character/Swordman/Effect/FireWave/fire_dodge.img`、`fire_normal.img` | sprite_character_swordman_effect_firewave.NPK | pvp/子变体视觉 | 不需要（pvp 变体） | ❌ |
| `Character/Swordman/Effect/FireWaveFloor.img` | sprite_character_swordman_effect.NPK（Effect 根） | skl 预载清单，无引用者 | 存疑（未考证） | ❌ |

缺失 img：必需 3 张（flame_dodge / fire-front / blast-front）、可选 4 张，横跨 3 个 NPK（firewave 系一次提取覆盖大半）。img 版本（v2/v4 ✓ / v5 ✗）提取时把关。
角色施法动画 wave.ani 仅引 `sm_body%04d.img`（已入库，L16）——角色侧零缺失。

## 5. 实现方案草案

**判定形状二选一**（§2.3 实证扩张盒 vs 传统行进波手感）：

- **方案 A（推荐，判定同构）**：扩张盒实证 = 前方 ~2 单位贴地宽扇 ×750ms 单次命中，后接爆炸。映射为 2 个 Area 顺序创建，���弹体。
- 方案 B（手感同构）：穿透 Bullet（NormalWave 同构）+ 终点爆炸 Area。DNF 本体更像 A（数据说话），B 留作备选。

### 内容件清单（方案 A，全部继承真实基类）

1. **`DotNet~/Skills/FireWaveSkill.cs : SkillLogic`**（同 FireCircleSkill/ReleaseWaveSkill 范式）
   - `CooldownMs = 15000`（DNF 原值直用）；`TotalTimeMs = 1700`（施法 400 + 火浪 763 + 收势余量；末尾爆炸 Area 寿命 630ms 独立延续，不受技能时长截断——FireCircleSkill 350ms/区 5000ms 同构）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanWaveCast)`（wave.ani json，F0 手改 400ms 见 §6）+ `ctx.ClearHitTargets()`。
   - `OnUpdate`（SubState 单值推进）：
     - t≥400（施法完毕瞬间）：`ctx.CreateAreaInFront(AreaIds.FireWave, (FP)1)`（火浪判定区，出生=身前 1 单位）；SubState=1。
     - t≥1163（火浪 763ms 后触地）：`ctx.CreateAreaInFront(AreaIds.FireWaveExplosion, (FP)2)`（爆炸区，火浪前端位置）；SubState=2。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/FireWaveArea.cs : AreaDefinition`**（BloodBoomArea 范式：EnterActions 单次结算）
   - `TotalTimeMs = 770`、`TickTimeMs = 0`、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (2.0, 0.32, 1.35)`（ATKBOX F2 折算：w400/h64/d269 px → 半尺寸 ÷100）；
   - `HitReaction { Damage = 100, HitstunMs = 700, KnockbackX = 150, LaunchY = 200 }`（firewavebig.atk：down/push150/lift200 → 长硬直+浮空落地近似，releasewave 先例）；
   - `ViewAnimId = AnimId.FireWaveBullet`（flame_dodge.ani 视觉，770ms 恰合）+ `ViewBackAnimId = AnimId.FireWaveFront`（fire-back 背层，可选）。
3. **`DotNet~/Areas/FireWaveExplosionArea.cs : AreaDefinition`**
   - `TotalTimeMs = 630`（blast-front 原时长）、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (1.2, 0.4, 1.2)`（爆炸范围无盒数据，推断值；stuck -1000 的吸附感用 HitstunMs 表达）；
   - `HitReaction { Damage = 150, HitstunMs = 900, KnockbackX = 300, LaunchY = 200 }`（firewavebigsub.atk：down/push300/lift200/stuck-1000）；
   - `ViewAnimId = AnimId.FireWaveBlastFront` + `ViewBackAnimId = AnimId.FireWaveBlastBack`。
4. **无新增 Action**（MeleeHit 现成；基础版不灼烧）。
5. 方案 B 备选：`FireWaveBullet : BulletDefinition`（复制 NormalWaveBullet：Speed 15 / TotalTimeMs 1500 / 穿透 / HalfExtents (0.8,0.3,1.3) / 同上 HitReaction / ViewAnimId=flame_dodge），技能 OnUpdate 于 t=1900 在身前 22 单位 `CreateAreaInFront` 爆炸（TotalTimeMs 相应 2000）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| WaveSword 状态 + wave.ani 施法 | `FireWaveSkill.OnCast` + `AnimId.SwordmanWaveCast` |
| 施法时间 400ms | wave.ani F0 手改 400ms（json 手改项，§6） |
| firewavebig PO + flame.act 扩张盒 | `FireWaveArea`（最大盒折算，单次命中） |
| firewavebigsub 爆炸 PO | `FireWaveExplosionArea`（时间驱动顺序创建） |
| .atk push/lift/down | `HitReaction.KnockbackX/LaunchY/HitstunMs` |
| 火焰铺设视觉（249951×3） | Area ViewAnimId 单层（IMAGE RATE 扩张延后 → 直出原帧） |
| 波动印联动（wavemark） | 缺 Buff 查询门面 → 跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.FireWave = 15` + `ButtonToSkill` case 7（新键，如 N；LSOperaComponentSystem 加分支） |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.FireWave = 4`、`FireWaveExplosion = 5`（方案 B 则 `BulletIds.FireWave = 3`） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanWaveCast = 59`、`FireWaveBullet = 60`（flame_dodge）、`FireWaveFront = 61`（fire-front，区域视图）、`FireWaveBlastFront = 62`、`FireWaveBlastBack = 63`（背层可选） |
| json 注册 | `…\lockstep\Scripts\HotfixView\Client\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×3~5（swordman_wave / firewave_flame / firewave_firefront / firewave_blastfront[/blastback]） |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | `flame_dodge.img.bytes`、`fire-front.img.bytes`、`blast-front.img.bytes`（必需 3；可选 +fire-back/blast-back/Glow/Spark/light） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 15000 ms | 15000（直用） |
| 施法时间 | 400 ms | 400（wave.ani F0 手改） |
| 火浪判定 | ATKBOX 扩张 130→400px 宽 / 64 高 / 269 深，750ms | HalfExtents (2.0,0.32,1.35) 单发 770ms |
| 主波伤害 | level col0（2873 → 23413，Lv1→70） | 100（固定） |
| 主波反应 | down / push150 / lift200 | Hitstun 700 / Kb 150 / Ly 200 |
| 爆炸伤害 | level col1（5344 → 42746） | 150（固定） |
| 爆炸反应 | down / push300 / lift200 / stuck-1000 | Hitstun 900（含 stuck 近似）/ Kb 300 / Ly 200 |
| 爆炸时点 | 引擎内置（触地/终点，未考证） | 施法后 1163ms（火浪播完） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `wave.ani` | `[DELAY] 10000`（F0 施法悬停帧）——工具按 README"原样保留"，游戏侧会真的停 10 秒 | **翻译需钳制或约定手改**（已在缺口累计：R1-A2 jump.ani 首例；本技能为施法前摇型第二例）→ 翻译后手改 json F0=400 |
| `flame.act` / `.act` 文件类型 | 无子命令 | 本技能不依赖：扩张盒数值手抄进 Area（§5 已给）；act 的"逐帧铺视觉"由 Area 视图替代 |
| `firewavebig.obj` / `firewavebigsub.obj` | `.obj` 无子命令 | 手工映射为 Area 编排（§5）；obj 落地 `obj` 子命令时注意 **`blast-front-1.ani` 后缀名引用失配**（实际文件无 -1） |
| `firewavebig.atk` / `firewavebigsub.atk` / `firewave.atk` | `.atk` 无子命令 | 每文件 ~7 值手抄可接受；随批量化提级 |
| `FireWave.skl` | `.skl` 无子命令 + `[skill preloading image]` 节 | 手抄（2 列数值）；预载节已在缺口累计 |
| `ATKBOX.ani` | `[GRAPHIC EFFECT]`（已支持，L15）+ 全帧 RGBA alpha=0 | 翻译无碍；**不要**把它当视觉层用（全透明）——视觉走 flame_dodge.ani |

结论：.ani/.als 常规节全部可译（本技能基础链路无 .als）；实质缺口 = 超长 DELAY 处理约定 + `.act`/`.obj`/`.atk`/`.skl` 四类无子命令，计 5 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 火元素属性伤害（fire element） | 元素属性系统缺失 | 记档跳过（数值同魔法） |
| 命中击退 + down 反应 | 已有（HitReaction + LSFlight） | 直用 |
| 爆炸 stuck -1000（吸附定身） | 无 hold/stuck 微控（缺失档，IceWave hold 同族） | 并入 HitstunMs 900 长硬直近似 |
| 火浪扩张视觉（3 个 flame01 逐帧铺设 + ATKBOX 透明帧） | IMAGE RATE 扩张延后；无铺设型视觉机制 | Area 单层 flame_dodge 视觉直出（视觉略"整块"，可接受） |
| 波动印生成/联动增伤 | Buff 查询门面缺失 | 跳过 |
| mod 灼烧分支（col5-8） | 本 pvf 死代码（skl 无此列） | 不做（忠于基础版） |
| 等级缩放（col0/col1） | 等级数值缩放延后 | demo 固定值 |
| 音效/屏震（flame.act SHAKING 2/150） | 延后 | 跳过 |
| 施法期间站桩 400ms | 无强制站桩机制（现有技能均不锁移动） | 沿用现状（不锁） |

## 8. 存疑与缺口上报

- **未考证**：①基础版（22）引擎侧创建 firewavebig/sub PO 的精确触发（触地 vs 动画终点）；②static 350/450 语义；③wave.ani F7 flag 65534 语义；④`blast-front-1.ani` 的 -1 后缀约定；⑤FireWaveFloor.img 用途（仅预载清单）；⑥firewavebig 与 unclebang 共享 id17 两套火浪并存的取舍（引擎实际用哪套）。
- **新缺口上报**：①**施法前摇型超长 DELAY**（wave.ani F0=10000 作为 casting time 载体）——与 jump.ani 悬停帧同族但语义不同（前者是数据驱动前摇），建议翻译工具加"DELAY>N 钳制/标注"选项；②**`.act` 逐帧行为表**（flame.act 的 CREATE PASSIVEOBJECT 铺设模式）——act 子命令设计时按"帧→创建物+参数"建模可覆盖此类铺设视觉。
- **给下轮的经验**：F1 波动剑族的基础四波施法侧**全部引擎内置**（wave.nut 只有 IceWaveEx 分支）；判定/视觉数据在 `passiveobject\character\swordman\`（firewavebig 系）与 unclebang 共享表（id17）**两处各有一套**，走读时先 .obj 后共享表。level property 向量首值 **-2 = level info 列引用**（本例双列模板实证，L21 补充）。
