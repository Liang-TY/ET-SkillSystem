# 崩山击（HopSmash）

> 技能ID 65 | 级别 A | 可实现性 🔶（蓄力变距/多段简化） | 分析日期 2026-08-22 | 批次 A2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 崩山击 | `skill\Swordman\HopSmash.skl [name]` |
| 英文名 | HopSmash（取 skl 文件名；[name2]="Hill Breaker" 亦为英文，属例外） | 同上 [name2] 实测 |
| 职业 | 鬼剑士全系可学（growtype 0-5）；冲击波分支限狂战士（[血气旺盛]为狂战特有被动） | 同上 [skill fitness growtype] + [explain] |
| 学习等级 | 10 | 同上 [required level] |
| 最高等级 | 70（各觉醒段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 2） | 同上 [type] / [skill class] |
| 指令 | →↓ + Z（指令施放 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 4000 ms（固定） | 同上 [dungeon][cool time] |
| 施放时间 | 无（skl 无 [casting time] 节，瞬发） | 同上实测 |
| MP | 17 → 150 | 同上 [consume MP] |
| 特殊消耗 | 无（血气旺盛的 MP→HP 替代见 §2.2） | 同上 |
| 一句话效果 | 向前低跃用武器砸击地面，多段攻击判定，最后一击击倒；已学[血气旺盛]则发出冲击波并使敌人出血；跳跃距离和冲击波范围随指令键按放间隔改变 | 同上 [explain] |

**static data（11 值）**：`30 200 50 200 50 120 150 100 2 3 500`。
其中 **static[8]=2、static[9]=3 = 多段攻击次数 2~3 次**（模板行实证，见下）；其余 8 值推断为跳跃物理参数
（跳跃距离/高度/速度的档位——按放间隔越长跳越远，具体映射未考证）。

**level property（8 列，Lv1 → Lv70）**：`125→783`、`374→2339`、`200→1558`、`12→98`、`10000(恒)`、`45→282`、`250→409`、`375→614`。
模板行（实测）逐占位符对照向量行解码（**高置信推断**，方法见 §8 经验）：

| 列 | 模板行 | 值域 | 语义 |
|---|---|---|---|
| col0 | `物理攻击力 : <int>%%` | 125%→783% | 跳跃下砸本体（多段）物理攻击力 |
| static[8]~static[9] | `多段攻击次数 : <int>~<int>次` | 2~3 次 | 多段次数区间（区间由什么浮动未考证——推断与蓄力档位联动，explain 只写了距离/范围随按放间隔变） |
| col1 | `冲击波物理攻击力 : <int>`（无 % 号=固伤） | 374→2339 | 血气旺盛冲击波固伤 |
| col2 | `出血机率 : <float1>%%`（×0.1） | 20.0%→155.8%（封顶 100%） | 冲击波出血率 |
| col3 | `出血Lv : Lv<int>` | Lv12→Lv98 | 异常等级 |
| col4 | `出血持续时间 : <float1>秒`（×0.001） | 10.0 秒（恒定） | 出血时长 |
| col5 | `出血攻击力 : <int>` | 45→282 | 出血每跳 |
| col6/col7 | `冲击波范围 : <int>~<int>px` | 250~375 → 409~614 px | 冲击波范围区间（与 PO 攻击盒实测���合，见 §2.3） |

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无注册行**（grep `hopsmash` / `, 65` 均无命中）。
崩山击属最老一代技能：**角色侧状态逻辑在客户端引擎内**；冲击波是数据驱动被动对象（.obj）。

```
// sqr/character/swordman/swordman_header.nut
CUSTOM_ANI_HOPSMASHREADY <- 26      // = swordman.chr [etc motion] 第 27 项（999 行，实测）
CUSTOM_ANI_HOPSMASH      <- 27      // = 同上第 28 项（1000 行）
```

```
// character/swordman/swordman.chr [etc attack info]（0 计数自 HardAttack.atk）
#25 `AttackInfo/HopSmash.atk`        （1319 行）
#26 `AttackInfo/HopSmashFinal.atk`   （1320 行）
```

兄弟职业脚本无同型实现（atswordman/demonicswordman/common load_state grep 均无命中）。

### 2.2 引擎内置行为重建（.ani 标记 + .atk 数据 + explain 三方印证）

**时序（hopsmash.ani 实测帧表锚定）**：

| 帧区间 | 累计时间 | 行为 | 数据依据 |
|---|---|---|---|
| 蓄力 | 0-400ms | `hopsmashready.ani` 单帧 400ms 蹲姿蓄力；**按住技能键延长蓄力→跳跃距离/冲击波范围增大**（按放间隔机制） | ready.ani 实测 + explain |
| 前跃 | 400-820ms（hopsmash F0-F2） | 前方低跃（跳跃物理参数在 static data，未考证映射） | F0=120/F1=240/F2=60ms |
| 下砸多段 | 820-1120ms（F3-F5） | **每帧攻击盒** `-10 -20 -2 ~ 86 40 49`（min/max → 前方 0.96×0.6×0.51 单位）多段 2-3 次；`HopSmash.atk`：physic / damage 反应 / push 30 / lift 30 / hit down | F3=180/F4=60/F5=60ms；static[8]/[9] |
| 霸体 | F2-F6（760-1300ms，540ms） | **DAMAGE TYPE = SUPERARMOR**（实测 5 帧） | F2-F6 逐帧标记 |
| 标记 | F4 起点（1000ms） | flag **65534**（同 GoreCross F14，取消窗口/命中标记，语义未考证） | 实测 |
| 末击 | F5-F6 | `HopSmashFinal.atk`：physic / damage bonus 40 / **down 击倒** / push 200 / lift 200 / hit stop 1.6 | 最后一击使敌人倒地（explain） |
| 冲击波 | 落地时（条件） | **已学[血气旺盛]（技能 63，狂战被动）才创建**冲击波 PO（见 2.3）+ 出血（col2 率/col3 Lv/col4 时长/col5 攻击力） | explain + 064 对 63 的考证 |

**血气旺盛联动**（064-GoreCross §1 已考证）：技能 63 习得后 MP 消耗由 HP 替代 + 本技能解锁冲击波/出血分支。

### 2.3 被动对象：冲击波（hopsmashsub.obj，完整实测）

`passiveobject/character/swordman/hopsmashsub.obj`（`hopsmashsub_ds.obj` 为剑影变体，引用 Front1_DS/Front2_DS）：

| .obj 节 | 值 | 说明 |
|---|---|---|
| [floating height] | 1 | 悬浮高度 |
| [pass type] / [piercing power] | pass all / 1000 | 全穿透 |
| [basic motion] | `Animation/HopSmashSubFront1.ani` | 冲击波主层：6 帧 480ms（80ms×6），**F0-F3 攻击盒逐帧扩张**：`-43,1~85,39` → `-125,0~250,80` → `-142,0~282,80` → `-145,-1~292,80`（min/max）；F0 播音效 BLOODBOOM_EXP |
| [attack info] | `AttackInfo/HopSmashSub.atk` | 命中：physic / **down 击倒** / push 0 / **lift 300** / cut+blood 50 1.0 / hit horizon / 音效 GORECROSS_HIT |
| [add object effect] | `Animation/HopSmashSubFront2.ani` @层 1 | 叠加辉光层（b_bottom_01_d.img，LINEARDODGE，无攻击盒） |
| [object destroy condition] | on end of animation | 播完即毁（480ms） |

**范围交叉验证**：F3 盒 x∈[-145,292] = 437px 宽，与 level info col6/col7（250~375px@Lv1 → 409~614px@Lv50）
同量级——**col6/col7 是等级缩放的冲击波范围，PO 盒是某一档的实测**，互相印证（高置信）。

**未解**：`hopsmashsubback1/2.ani`（b_bottom_02_n/d.img）不被 .obj 引用（两个 obj 都只引 Front 层），
也未在白名单脚本中找到引用者——推断为引擎按朝向/条件选层的备用层或老版本残留（未考证）。

特效层（引擎绘制，`effect/animation/hopsmash/`，无引用者）：`smash.ani`（7 帧 900ms，d-end.img，落地爆尘）、
`sword.ani`（4 帧 540ms，b-start/b-middle.img，跃击刀光）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/hopsmashready.ani` | 1 | 400ms | 无 | 无 | 蓄力蹲姿；受击盒 2 |
| `character/swordman/animation/hopsmash.ani` | 7 | 900ms（120/240/60/180/60/60/180） | F4=65534 | **F3/F4/F5**（-10,-20,-2~86,40,49） | **F2-F6 SUPERARMOR**；F6 无盒收势 |
| `passiveobject/.../animation/HopSmashSubFront1.ani` | 6 | 480ms | 无 | **F0-F3**（逐帧扩张至 437px 宽） | F0 播 BLOODBOOM_EXP |
| `passiveobject/.../animation/HopSmashSubFront2.ani` | 6 | 480ms | 无 | 无 | 叠加辉光层（LINEARDODGE） |
| `passiveobject/.../animation/HopSmashSubBack1/2.ani` | 6/6 | 480ms | 无 | 无 | **无引用者**（未考证） |
| `effect/animation/hopsmash/smash.ani` | 7 | 900ms | 无 | 无 | 落地爆尘 |
| `effect/animation/hopsmash/sword.ani` | 4 | 540ms | 无 | 无 | 跃击刀光 |

`.als` 边车：**本技能全部文件均无**（两侧 animation 目录 ls 实证）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | HopSmash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\HopSmash.skl` | ✅ 实测 | 技能数据 |
| lst 条目 | swordmanskill.lst 69-70 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 65 → 本 skl |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | — |
| 常量表 | swordman_header.nut 196-197 行 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_HOPSMASHREADY/HOPSMASH = 26/27 |
| .chr 条目 | etc motion #26/#27（999/1000 行）+ etc attack info #25/#26（1319/1320 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 动画 + 两段命中参数 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\` | ⛔ 缺失 | 角色逻辑在引擎 |
| ap nut | —（不存在） | `…\pvf\passiveobject\character\swordman\` | ⛔ 缺失 | PO 行为引擎内置，数据在 .obj |
| 角色 .ani | hopsmashready.ani / hopsmash.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 见 §2.4 |
| 角色 .atk | hopsmash.atk / hopsmashfinal.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 多段/末击 |
| PO 定义 | hopsmashsub.obj（+hopsmashsub_ds.obj 剑影变体） | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 冲击波结构 |
| PO .ani | HopSmashSubFront1/2.ani + Back1/2.ani（+_ds ×4） | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 冲击波视觉 |
| PO .atk | hopsmashsub.atk（+hopsmashsub_ds.atk） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | 冲击波命中（击飞 300） |
| 特效 .ani | smash.ani / sword.ani（+hopsmash_ds\） | `…\pvf\character\swordman\effect\animation\hopsmash\` | ✅ 实测 | 引擎绘制特效 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失 | — |
| 装备层 | hopsmash.ani ×76 / hopsmashready.ani ×76 | `…\pvf\equipment\character\swordman\avatar\` | ✅ 实测（find 计数） | 换装图层 |
| 关联被动 | BloodyVigorous.skl（技能 63） | `…\pvf\skill\Swordman\BloodyVigorous.skl` | ✅ 实测（064 已考证） | 冲击波/出血分支门槛 + MP→HP |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画帧 | 必需（共享） | ✅ 已在库 |
| b_bottom_01_n.img | sprite_character_swordman_effect_hopsmash.NPK | 冲击波主层（PO Front1） | **必需** | ❌ |
| b_bottom_01_d.img | 同上 | 冲击波辉光层（PO Front2） | **必需**（视觉主表现） | ❌ |
| b_bottom_02_n.img | 同上 | 备用层（Back1，无引用者） | 可选（不做） | ❌ |
| b_bottom_02_d.img | 同上 | 备用层（Back2） | 可选（不做） | ❌ |
| d-end.img | 同上 | 落地爆尘（effect smash.ani） | 可选 | ❌ |
| b-start.img / b-middle.img | 同上 | 跃击刀光（effect sword.ani） | 可选 | ❌ |

缺失 img：必需 2、可选 5，同属一个 NPK 一次提取全覆盖。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/HopSmashSkill.cs : SkillLogic`**（同 ReleaseWaveSkill 范式：纯函数位移 + 帧驱动攻击盒）
   - `CooldownMs = 4000`（原值直用）；`TotalTimeMs = 1400`（ready 400 + hopsmash 900 + 余量 100）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanHopSmashReady)` + `ctx.ClearHitTargets()`（蓄力按放间隔机制砍掉=固定中档，§7）。
   - `OnUpdate` 编排（SubState 0→1→2）：
     - t=400ms → SubState=1：`ctx.PlayAnim(AnimId.SwordmanHopSmash)`；随后 420ms 内（对应 smash F0-F2 空中段）匀速前冲 ~1.5 单位
       （`ctx.MoveCasterForward` 纯函数增量，ReleaseWaveSkill.OnUpdate 同构；DNF 跳跃距离 200px 档 ÷100 ≈ 2 单位，demo 取 1.5）。
     - F3-F5（帧驱动攻击盒由 hopsmash json 自带）：多段 2-3 次简化为**单次结算**（多段命中延后），
       `HitReaction { Damage = 80, HitstunMs = 600, KnockbackX = 200, LaunchY = 200 }`
       （DNF 多段=push30/lift30、末击=down/push200/lift200 两档——技能级 HitReaction 单份，**统一取末击 down 档**，硬直 600 表现击倒；差异见 §7）。
     - t=1120ms（F5 末，落地瞬间）→ SubState=2：`ctx.CreateAreaInFront(AreaIds.HopSmashWave, 0.5)` 冲击波（血气旺盛常开，等价已习得）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/HopSmashWaveArea.cs : AreaDefinition`**（冲击波——同 BloodBoomArea 单次结算范式）
   - `TotalTimeMs = 480`（PO 动画时长）、`TickTimeMs = 0`、`EnterActions = { MeleeHit, AddBleedBuff }`；
   - `HalfExtents = (2.2, 0.5, 0.4)`（PO F3 盒 x∈[-145,292]/2 ≈ 2.19 半宽）；出生中心=施法者前方 0.5（DNF 波以落点为中心，盒前后对称）；
   - `HitReaction { Damage = 130, HitstunMs = 800, KnockbackX = 0, LaunchY = 300, ProcBuffId = BuffIds.Bleed, ProcChance = 100 }`
     （hopsmashsub.atk 原值 down/push0/**lift300**；出血率 DNF 20%~155.8% → demo 固定 100% = 血气旺盛满档）；
   - `ViewAnimId = AnimId.HopSmashWaveFront`（HopSmashSubFront1 json）+ 需要辉光层时走 `ViewBackAnimId = AnimId.HopSmashWaveGlow`（Front2）或手组装 overlay。
3. 出血 Buff 复用现有 `BuffIds.Bleed`（BleedBuff 3s/每秒 15；DNF 原值 10s/45-282 每跳——demo 用现值，见 §7）。
4. 无需新 Action。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 65 + ready/smash 两动画 | `HopSmashSkill` + 两个 AnimId 时序切换 |
| 前跃（跳跃物理，static data） | `MoveCasterForward` 纯函数水平位移（无 z 轴跳跃，视觉由动画承担） |
| 多段下砸（F3-F5 盒 × 2-3 次） | 帧驱动攻击盒（json）单次结算（多段延后） |
| 末击击倒（HopSmashFinal.atk） | 技能 `HitReaction`（统一取末击档） |
| 冲击波 PO（血气旺盛条件） | `HopSmashWaveArea`（条件砍掉=常开） |
| PO 出血（col2/3/4/5） | `HitReaction.ProcBuffId/ProcChance` + BleedBuff |
| 霸体 F2-F6 | 延后（不还原） |
| 蓄力按放间隔变距 | 砍掉（固定中档距离） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.HopSmash = 13`（顺延；与并行批次冲突时实现时统一分配）+ `ButtonToSkill` case 9（新键） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanHopSmashReady=58`、`SwordmanHopSmash=59`、`HopSmashWaveFront=60`、`HopSmashWaveGlow=61`（顺延，冲突时统一调） |
| json 注册 | `…\LSAnim\LSAnimClipRegistrar.cs` | `RegisterOne` ×4 |
| 图集 | `…\LSAnimResComponentSystem.cs` BuildAtlas | `b_bottom_01_n.img.bytes`、`b_bottom_01_d.img.bytes`（必需两张） |
| 按键 | `…\LSOperaComponentSystem.cs` | 新按键分支 → button 9 |
| 翻译 | DnfConfigTranslation ani 子命令 | 角色 2 + PO 2 json |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 4000ms | 4000（直用） |
| 总时长 | ready 400 + smash 900 = 1300ms | 1400 |
| 前冲距离 | static 跳跃参数（200px 档推断） | 1.5 单位固定 |
| 下砸攻击盒 | F3-F5 `-10,-20,-2~86,40,49` | json 帧驱动直用 |
| 下砸命中 | 多段 damage/push30/lift30；末击 down/push200/lift200 | 统一 80/600/Kb200/Ly200 单次 |
| 冲击波 | down/push0/lift300 + 出血 | 130/800/Kb0/Ly300 + Bleed 100% |
| 冲击波范围 | 250-375px（Lv1）~409-614px | HalfExtents x 2.2（固定档） |
| 出血 | 率 20-155.8%/10s/每跳 45-282 | BleedBuff 现值 3s/每跳 15 |
| 霸体 | F2-F6（540ms） | 不还原 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `HopSmash.skl` | `.skl` 尚无子命令（8 列 level info + 11 值 static） | 手抄可行；随批量化加 `skl` 子命令 |
| 3 个 `.atk`（hopsmash/hopsmashfinal + PO hopsmashsub） | `.atk` 尚无子命令；`[hit info]` 的 `cut+blood 50 1.0` / hit stop `1.6` 无对应概念 | 手抄核心 8 值/文件；cut/blood→ProcBuffId 语义映射，hit stop 跳过 |
| `hopsmashsub.obj` | `.obj` 尚无子命令 | 手工映射为 Area（本文 §5 已给） |
| 角色/PO 各 .ani | `[DAMAGE TYPE] SUPERARMOR`（整节跳过）、`[PLAY SOUND]`、`[SHADOW]`、`[GRAPHIC EFFECT] LINEARDODGE` | 跳过无碍；SUPERARMOR 若做霸体需 AnimFrameData 加字段（01§0.4 既记）；LINEARDODGE 为消费侧缺口（同 064） |

结论：.ani/.als 侧全部可被现有 ani 子命令翻译（本技能无 .als）；实质缺口 `.skl`/`.atk`/`.obj` 三类无子命令 + GRAPHIC EFFECT 消费通道（计 4 条，与 064 同清单）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 蓄力按放间隔→跳跃距离/冲击波范围浮动 | 无按住蓄力输入（延后，同 IceWave 蓄力版） | 固定中档（前冲 1.5 单位、范围 Lv1 档） |
| 前跃（z 轴低跳） | 无玩家 z 物理（跳跃系统缺失，§8 上报） | 水平位移 + 动画承担视觉（崩山击是低跃，贴地近似损失小） |
| 多段 2-3 次（引擎命中表重置） | 多段命中（延后档） | 单次结算取末击反应；要手感可后续把 F3-F5 拆 3 个短 Area（TickActions 替代） |
| 多段(damage/30/30) 与末击(down/200/200) 反应不同 | 技能级 HitReaction 单份（不算缺失，编排可表达） | 统一取末击档；或末击单独走小 Area |
| 霸体 F2-F6（540ms） | 霸体帧（延后档，01§0.4 既记） | 不还原（受击可打断） |
| 血气旺盛条件分支 | 被动技能系统不存在 + Buff 查询门面（缺失档，064 同） | 常开（等价已习得）；MP→HP 替代不做 |
| 冲击波范围随等级缩放（col6/col7） | 等级数值缩放（延后档） | 固定档 |
| LINEARDODGE / 音效 | 延后档 | 直出原始帧 / 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 前 8 值（`30 200 50 200 50 120 150 100`）与跳跃物理的映射（推断=距离/高度/速度档位）。
2. `HopSmashSubBack1/2.ani`（b_bottom_02 层）的引用者（两 .obj 均不引用，白名单脚本无引用——推断引擎条件选层或残留）。
3. F4 flag 65534 语义（同 064 未考证项）。
4. hopsmashfinal.atk `[hit info]` 的 `-1 1.6`（推断=hit stop 表现参数）。
5. col2 出血率 20.0%→155.8% 超过 100% 的封顶行为（推断引擎封顶）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **跳跃系统**（与 017 同源上报）：本技能"低跃"可用水平位移近似，但跳跃距离随蓄力浮动、空中攻击族
   （17/43/流心系）都依赖同一系统——建议与 017 §8 合并记档。
2. **技能内多份 HitReaction 切换**：DNF 单技能多段不同反应（本技能多段/末击两档）在我方需 Area 编排或
   统一取值——不算缺失但属高频适配点（064 两刀同构），建议实现期沉淀"分段 HitReaction"惯例
   （帧号→Area 或未来 SkillLogic.PhaseReactions）。

**翻译工具缺口**：`.skl` 子命令、`.atk` 子命令、`.obj` 子命令、`[GRAPHIC EFFECT]` 消费通道（计 4 条，同 064 清单）。
