# 地裂 · 波动剑（NormalWave）

> 技能ID 20 | 级别 A（维持预判） | 可实现性 ✅（游戏已有 demo 实现，本文含与原版差异对照） | 分析日期 2026-08-22 | 批次 A3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 地裂 · 波动剑 | `skill\Swordman\NormalWave.skl [name]` |
| 英文名 | NormalWave（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 鬼剑士共通可学（[skill fitness growtype] 0-5）；波动剑系鼻祖（阿修罗主线） | 同上 + F1 分族经验 |
| 学习等级 | 15 | 同上 [required level] |
| 最高等级 | 70（各觉醒段上限 50） | 同上 |
| 类型 | active（skill class 0） | 同上 [type] |
| 指令 | ↓→ + Z（指令施放 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 3000 ms（dungeon 固定；pvp 4500） | 同上 [cool time] |
| 读条 | casting time 200 ms | 同上 [casting time] |
| MP | 17 → 168（Lv1 → Lv70） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `250 100 -150`（dungeon；pvp `350 100 -150`。语义未考证：250/350 疑为波速相关，-150 未考证） | 同上 [static data] |
| 一句话效果 | 向前方快速施放无属性地波，击退并击倒敌人；波大小随等级增大 | 同上 [explain] |

**level property（3 列，Lv1 → Lv70 首末值）**：`225→2366`、`100→159`、`276→2877`。
列语义（模板 + 惯例，高置信）：col0 = 魔法攻击力 %；col1 = **大小比率 %**（100→159%，模板明示）；col2 = 魔法攻击力固定值。pvp 列同构（11→126 等）。
预加载图：skl [skill preloading image] 声明 `Character/Swordman/Effect/NormalWave1.img` + `NormalWave2.img`。

## 2. 技能逻辑走读

### 2.1 注册与文件链（F1 波动剑系入口直取）

无独立 pushState——波动剑系（20/21/22/24 + Ex）共用一条注册（`swordman_load_state.nut:16-17` 实测）：

```
16: IRDSQRCharacter.pushState(0, "character/Swordman/wave/wave.nut", "WaveSword", 24 ,-1 );
17: IRDSQRCharacter.pushPassiveObj("character/Swordman/wave/po_wavecut.nut", 24328);
```

- 状态名 `WaveSword`（状态号 24，技能号 -1）——**基础版（本技能 20）的施法逻辑在客户端引擎内**（老一代技能常态）。
- `wave.nut`（74 行，全读）中**唯一活跃回调** `onKeyFrameFlag_WaveSword` 只处理子状态 100＝IceWaveEx（技能 100 强化冰刃，写包两发 24328）——**本技能 20 不经过任何脚本分支**；`onAfterSetState_WaveSword` 仅记录子状态 + 波动印记联动（ap_wavemark，缺 Buff 查询门面，跳过）。
- 弹体脚本 `po_wavecut.nut`（全读）`setCustomData` 仅有 `id == 125`（IceWaveEx）分支；**基础版弹体同样引擎内置**，数据全在 `normalwave.obj`（见 2.3）。

### 2.2 引擎内置施法行为重建（.ani 标记 + .obj/.atk 数据印证）

- 播 `wave.ani`（.chr [etc motion] #9 = `CUSTOM_ANI_WAVE <- 9`，header.nut:179 实测对位）：**F0 停 10000ms = 读条姿态帧**（引擎按 [casting time] 200ms 切走），F1-F7 ×50ms + F8 ×150ms = 500ms 挥出动作，F7 flag 65534（取消窗口标记，064 同款）。帧号 42-50 → sm_body 单图集。
- 读条完成瞬间创建弹体（普通波动 PO）：按 .obj 定义飞行、穿透、命中结算（见 2.3）。伤害倍率 = skl col0/col2（引擎读取，机制推断——写包细节引擎内置，无脚本可读）。
- 施法动画播完回待机。

### 2.3 被动对象：普通波动（normalwave.obj，全文实测）

`passiveobject\character\swordman\normalwave.obj`（另 `normalwave_ds.obj` 剑影变体，数据同构）：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [name] | 普通波动 | |
| [floating height] | 0 | 贴地飞行 |
| [pass type] / [piercing power] | pass all / 1000 | **全穿透**（可打多目标） |
| [basic motion] | `Animation/NormalWaveFire_Light/light_NormalWaveFire.ani` | 弹体动画（现行视觉） |
| [attack info] | `AttackInfo/NormalWave.atk` | 命中参数 |

**弹体动画（现行，.obj basic motion）** `light_NormalWaveFire.ani`：5 帧 ×65ms=325ms，引 `Character/Swordman/Effect/light_hadouken_hadou.img`（"波动拳"图集），F0-F2 有攻击盒（偏移+尺寸格式，如 F0 `11 -20 -18 / 238 40 120` ≈ x∈[11,249] 判定前伸 ~2.4 单位），全帧 LINEARDODGE。
**候选弹体动画**：同目录上级 `normalwavefire.ani`（同 hadouken 图，无 _light 前缀，5 帧×65ms）与 PO 根 `normalwave.ani`（**NormalWave2.img** 4 帧 ×50ms LOOP、每帧攻击盒 `-33 -14 0 / 66 28 27`、LINEARDODGE）——三者关系未考证（推断 NormalWave2 版为旧版视觉、hadouken 版为改版后现行；`_light` 为光属性变体命名）。

**命中参数** `normalwave.atk`（passiveobject 侧，全文实测）：魔法 / weapon damage apply 1 / 无属性 / **damage reaction=down（击倒）** / **lift up 200 / push aside 300** / attack direction=hit horizon / [hit info] [blow] / [no blood] 10 0.7 / hit wav FLAMES_HIT——与 explain"击退并使其倒地"吻合。
**施法侧 .atk**：角色 `attackinfo\` 无 wave 条目（实测：仅 waveeye/shockwavearea/standalonewave 系）——命中全由弹体结算（轮间经验 L3 同型）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\wave.ani`（角色施法，共用） | 9 | 10500ms 名义（F0=10000 读条停帧 + 500ms 实动作） | F7=65534 | 无（判定在弹体） | 帧号 42-50 → sm_body；各帧 damageBox 皮肤层 |
| `passiveobject\...\normalwavefire_light\light_normalwavefire.ani`（弹体现行） | 5 | 325ms | 无 | F0-F2（偏移+尺寸） | hadouken 图集；LINEARDODGE |
| `passiveobject\...\normalwave.ani`（弹体旧版候选） | 4 | 200ms（LOOP 1，飞行期循环） | 无 | 每帧（偏移+尺寸） | NormalWave2.img；LINEARDODGE |
| `character\swordman\effect\animation\normalwave.ani`（角色侧特效） | 7 | 350ms | 无 | 无 | **NormalWave1.img**（F0 空帧）；= 游戏在用 normalwave.json 的源文件 |

`.als` 边车：本技能全部文件**均无**（character/passiveobject 两侧 animation 目录实测；icewave 系才有）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | NormalWave.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\NormalWave.skl` | ✅ 实测 | 技能数据 |
| 注册行 | swordman_load_state.nut:16-17 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | WaveSword 状态 24 + 24328（共用；基础版逻辑引擎内置） |
| 主 nut | wave.nut | `…\pvf\sqr\character\swordman\wave\wave.nut` | ✅ 实测（74 行全读） | 仅 IceWaveEx 分支 + 印记联动；**本技能不经脚本** |
| 弹体 nut | po_wavecut.nut | `…\pvf\sqr\character\swordman\wave\po_wavecut.nut` | ✅ 实测（全读） | 仅 id=125 分支；基础版弹体引擎内置 |
| PO 定义 | normalwave.obj（+normalwave_ds.obj） | `…\pvf\passiveobject\character\swordman\` | ✅ 实测（全读） | 普通波动弹体结构 |
| PO .ani | light_NormalWaveFire.ani / normalwavefire.ani / normalwave.ani | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.4（三份候选视觉） |
| PO .atk | normalwave.atk（+normalwave_ds.atk） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测（全读） | 击退+击倒命中参数 |
| .chr 条目 | etc motion #9 | `…\pvf\character\swordman\swordman.chr:982` | ✅ 实测 | Animation/Wave.ani（`CUSTOM_ANI_WAVE <- 9` 对位） |
| 角色 .ani | wave.ani | `…\pvf\character\swordman\animation\wave.ani` | ✅ 实测 | 共用施法动画 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 缺失（命中在弹体） | — |
| 角色特效 .ani | effect/animation/normalwave.ani（+normalwaveslash.ani/_ds） | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | NormalWave1.img 波体视觉（游戏在用源） |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失（本技能无边车） | — |
| hold appendage | ap_wavehold.nut | `…\pvf\sqr\character\swordman\wave\` | ✅ 存在（021 已记） | 仅作用于 po_wavecut（IceWaveEx 路径），**本技能���涉及** |
| 装备层 | wave.ani ×76 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ 实测（只查存在性） | 施法动画换装图层 |
| 关联强化 | NormalWaveEx.skl（技能 140，lst 实测） | `…\pvf\skill\Swordman\NormalWaveEx.skl` | ✅ 实测（explain 已读） | 强化-地裂波动剑（大小增大）；E 类另行分析 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| NormalWave1.img（effect/animation/normalwave.ani 引用） | sprite_character_swordman_effect.NPK（Effect 根目录；具体归属以提取时为准��� | 波体视觉（游戏 demo 在用） | **必需** | ✅ `Bundles\AnimRes\NormalWave1.img.bytes` + `normalwave.json` 已在库 |
| sm_body%04d.img（wave.ani 帧号 42-50） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动画皮肤层（若补角色施法动作） | 必需（共享） | ✅ sm_body0000.img.bytes 已在库 |
| light_hadouken_hadou.img | sprite_character_swordman_effect.NPK（推断） | 原版现行弹体视觉（hadouken） | 可选（demo 已有 NormalWave1 视觉可继续用） | ❌ 未入库 |
| NormalWave2.img | 同上 | 旧版循环弹体视觉 | 可选 | ❌ 未入库 |
| （avatar wave.ani 各层 ×76） | sprite_character_swordman_equipment_avatar_<层>.NPK | 换装图层 | 可选（不需要） | ❌ 未入库 |

缺失 img：必需级 **0**（demo 已闭环）；可选级 2 张（hadouken/NormalWave2，换原版视觉时再提）。

## 5. 实现方案草案（本技能已实现，此节为"原版对齐升级清单"）

**现状**（`DotNet~/Skills/WaveSwordSkill.cs` + `Bullets/NormalWaveBullet.cs` + `LSOperaComponentSystem` I 键，代码实测）：
OnCast 即 `ctx.CreateBullet(BulletIds.NormalWave)`，`TotalTimeMs=350`、`CooldownMs=2000`、不切角色动画（注释自述"原地发波"）；弹：`Speed=15` 单位/s、`TotalTimeMs=1500`（射程 ~22 单位）、`DestroyOnHit=false` 穿透、`HalfExtents=(0.5,0.4,0.3)`、`HitActions={MeleeHit}`、**HitReaction 未 override（默认 50 伤害/500 硬直，无击退击倒）**、`ViewAnimId=AnimId.NormalWave`（normalwave.json，NormalWave1.img 7 帧）。

**与 DNF 原版差异对照及升级建议**（全部用现有 API，无新机制）：

| 项 | DNF 原版 | demo 现状 | 升级建议（先例） |
|---|---|---|---|
| CD | 3000ms | 2000ms | 改 `CooldownMs=3000` 直用 |
| 施法动画 | wave.ani（F0 读条停帧 + 500ms 动作，F7 取消标记） | 无动画切换 | `OnCast` 加 `ctx.PlayAnim(AnimId.SwordmanWave)`（sm_body 42-50 已在库，仅需翻译 json + 注册）；读条 200ms 延后不做，动画直接从动作段起播（json 里可裁掉 F0） |
| 弹体创建时机 | 读条完成后 | OnCast 立即 | 保持立即（读条延后；如需手感对齐可在 OnUpdate 延 200ms 创建——SubState 守卫，BloodBoom 同构） |
| 命中反应 | down 击倒 + push 300 + lift 200 | 默认 50/500 无位移 | `NormalWaveBullet.HitReaction = {Damage=50, HitstunMs=800, KnockbackX=300, LaunchY=200}`（normalwave.atk 原值；HitReaction/LSFlight 已落地，ReleaseWaveSkill 同构） |
| 伤害公式 | skl col0%（225→2366）+ col2 固定（276→2877） | MeleeHit 固定 50 | demo 固定值可留；数值化时改读表 |
| 大小缩放 | col1 100→159%（图像+判定同步） | 固定 | 延后（对象整体缩放，064 §8 已记） |
| 穿透 | pass all / 1000 | DestroyOnHit=false ✓ | 已对齐 |
| 波速 | static 250（pvp 350；单位语义未考证） | 15 单位/s | 手感已可，暂不动 |
| 弹体判定盒 | 弹体 ani 帧攻击盒（前伸 ~2.4 单位） | AABB (0.5,0.4,0.3) | 已可（BulletDefinition 无帧盒路径；弹体 ani 的 ATTACK BOX 翻译产物仅视图侧，不消费——记档即可） |
| MP 17-168 / 指令 MP 优惠 | 有 | 无 MP 系统 | 延后 |

**注册点增量**（若做施法动画对齐）：AnimId 加 `SwordmanWave`；LSAnimClipRegistrar 注册 `swordman_wave.json`（wave.ani 翻译产物，F0 长停帧建议裁剪）；无新 SkillId/按键/图集。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `NormalWave.skl` | `.skl` 尚无子命令（3 列 level info + casting time + static data + 预加载图���明） | 本技能手抄 3 组数值可行；`skl` 子命令批量化时建议一并输出 [casting time] |
| `normalwave.atk`（+ _ds） | `.atk` 尚无子命令 | 手抄 ~8 值可接受；随批量化提级 |
| `normalwave.obj` | `.obj` 尚无子命令 | 本技能 PO 结构极简（单 motion + 单 atk + 穿透参数），无需直译——弹体配置手写进 BulletDefinition 即可（现有实现即此路线）；obj 子命令需求记档（064 §8.3 同议） |
| wave.ani / 弹体 ani | `[SET FLAG]` 65534、`[SHADOW]`、`[GRAPHIC EFFECT]` | SET FLAG 按约定跳过（取消窗口 const 进技能类）；SHADOW 跳过；**GRAPHIC EFFECT 实测已支持**（AniParser.cs:140 → graphicEffect 1-4，游戏 normalwave.json 已含 `graphicEffect:1`——README"未识别节"清单滞后） |
| F0 空路径 `[IMAGE]`（effect/normalwave.ani 首帧） | 现有规则可处理 | 无需改工具（在库 json 已验证） |

结论：**.ani 资源全部可被现有 ani 子命令翻译**（本技能无 .als）；实质缺口为 `.skl`/`.atk`/`.obj` 三类无子命令，计 3 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 200ms 读条（casting time） | 无读条/施法条系统（延后档） | 跳过读条直接施放（demo 已如此） |
| 波大小随等级缩放（col1） | 对象整体缩放（延后档，064 §8 已记） | 固定 100% |
| 三份弹体视���（hadouken 现行 / NormalWave2 旧版 / NormalWave1 特效侧）的引擎选用关系 | 未考证 | demo 沿用 NormalWave1（已在库、观感成立）；要还原再提 hadouken |
| 弹体飞行期循环动画 + 消散表现（普通波无消散 ani） | BulletDefinition 视图单动画 | 现状即可（normalwave.json 7 帧不循环，弹视图播完停末帧——已有行为） |
| 波动印记联动（ap_wavemark 增伤推波） | Buff 查询门面（缺失档，021 §8 已记） | 跳过 |
| 音效 FLAMES_HIT | 音频（延后档） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. `[static data] 250 100 -150`（pvp 350 100 -150）逐参语义（250/350 疑为波速或射程参数）。
2. 三份弹体动画的引擎选用关系（.obj basic motion 指向 hadouken 版为数据事实，但 NormalWave2/NormalWave1 的实际播放角色无脚本/引用可查）。
3. 引擎基础版创建弹体的精确帧时机与写包参数（引擎内置，无脚本佐证）。
4. 弹体寿命/射程的引擎数值（.obj 无 destroy condition；demo 1500ms 为自定值）。

**新系统级缺口（§6.3 清单外）**
1. **BulletDefinition 无帧驱动判定路径**：弹体 .ani 自带 ATTACK BOX（本技能 F0-F2 / 旧版每帧），但弹逻辑判定走 AABB 半尺寸（translation 产物的 attackBoxes 不进弹）——当前用 HalfExtents 手配即可，量产后若弹形差异大可考虑"弹判定帧表"扩展。归记档（非本技能阻塞）。
2. （复述印证）`.obj` 子命令需求：本技能是"极简 PO"样本（单相位单 motion），与 gorecross 多相位样本（L9）合起来已覆盖 PO 两种形态，立项 `obj` 子命令时建议按"可选多相位"建模。

**翻译工具缺口（并入主循环汇总）**：`.skl`、`.atk`、`.obj` 无子命令（计 3 条）；README"未识别节"清单应把 `[GRAPHIC EFFECT]` 移出（工具已支持，代码实证）。
