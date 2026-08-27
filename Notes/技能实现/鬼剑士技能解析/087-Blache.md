# 第7鬼神 : 邪神之怖拉修（Blache）

> 技能ID 87 | 级别 A | 可实现性 🔶（深简化"定时区域风暴"可表达主干：沼泽持续区 → 爆发浮空 → 咬合击倒三段；沼泽减速/命中降 debuff、吞噬重置鬼神 CD、按击杀加智力全部依赖缺失系统） | 分析日期 2026-08-22 | 批次 A14

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 第7鬼神 : 邪神之怖拉修 | `skill\Swordman\Blache.skl` [name] |
| 英文名 | Blache（skl 文件名；[name2] 实测 `The 7th Ghost : Furious Blache`） | 同上 [name2] |
| 职业 | 鬼泣（[second growtype maximum level] 第 5/6 位=50/50 → growtype 2 鬼泣一觉/二觉档，48/233 两技能三方互证；吞噬七大鬼神=鬼泣一觉常识） | 同上 |
| 学习等级 | 50（觉醒主动） | 同上 [required level] |
| 最高等级 | 70（一觉/二觉档上限 50） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 3） | 同上 [type] |
| 指令 | ↑↑↓↓ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 140000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 600 → 5040（Lv1→70） | 同上 [consume MP] |
| 施法时间 | 500 ms（[casting time]，读条） | 同上 |
| 特殊消耗 | 无色小晶块 ×5；[league ban] 1（联赛禁用） | 同上 [consume item] |
| static data | `0 30000 930 10 930 1000 200 3 6 9`（见 §8 逐槽推断） | 同上 [dungeon][static data] |
| 一句话效果 | 召唤怖拉修：先出现缓缓变大的沼泽（减速+降命中），沼泽满幅时怖拉修现身冲击波（浮空），合上大腭二段伤害（击倒）并按击杀数给召唤者加智力；吞噬周围鬼神加速出场并重置其 CD | 同上 [explain] |

**level property（23 列，模板 8 行 9 向量；Lv1 → Lv70 首末值，L21 读法逐条对位）**：
需要鬼神数量 = static[0]=**0**（此 pvf 置 0——"必须存在鬼神才能召唤"被 mod 解除，推断）；
冲击波魔法攻击力 = col3 `2779→47488%`；合上大腭攻击力 = col4~col5 `10296→176817% ~ 30160→517902%`（min~max，随吞噬鬼神浮动）；
沼泽效果范围 = static[4]=**930px**；降低移动速度 = col0×0.1 `51→247%`（**推断**，超 100% 必有上限钳制，公式无 nut 佐证）；
降低命中率 = col1×0.1 `8.8→160%`（同上推断）；智力 buff 持续 = static[1]=30000ms×0.001=**30 秒**；每杀一敌加智力 = col2 `9→384`。
col6-17 为 3/6/9 级里程碑增益参数（col18-22 从 Lv6 起激活：50/20/11/2000/10000——具体语义未考证）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 145（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN , "character/swordman/blache/blache.nut", "Blache", 50, 87);
// swordman_header.nut 行 257/258（实测）：CUSTOM_ANI_BLACHECAST <- 87，CUSTOM_ANI_BLACHESMASH <- 88
```

状态号 **50** ≠ 技能 ID **87**（L2 又一实证）。⚠ **主 nut 仅 14 行**（F3b 半内置变体的极端形态：注册行存在、nut 只写一个钩子，
**沼泽创建/怖拉修出场/演出编排全部引擎内置**，pvf 只提供数据文件）：

```
// blache.nut 全文（实测）
function onAfterSetState_Blache(obj, state, datas, isResetTimer) {
    local substate = obj.sq_GetVectorData(datas, 0);
    if (substate == 1) {                       // 引擎切子状态 1（推断 = 怖拉修出场/吞噬完成时刻）
        obj.endSkillCoolTime(82); obj.endSkillCoolTime(84); obj.endSkillCoolTime(18);
        obj.endSkillCoolTime(25); obj.endSkillCoolTime(36); obj.endSkillCoolTime(41);
        obj.endSkillCoolTime(75); obj.endSkillCoolTime(96);   // 重置 8 个鬼神技能 CD
    }
}
```

（82/84/18/25/36/41/75/96 = 被吞噬鬼神技能 ID——explain 列卡赞/普戾蒙/萨亚/罗煞/凯贾/鬼影/卡洛 7 鬼神 + 1，具体映射未考证。）

### 2.2 引擎内置编排重建（以 .obj/.atk/.ani 数据 + explain 三方拼合；时序标推断）

```
① 施法（BlacheCast.ani，2 帧 480ms，含 500ms 读条）：站桩召唤
② 沼泽（blacheswamp.obj，[bottom] 层）：Start.ani（4 帧 360ms）起幕 → swampfloor.ani（22 帧 11890ms）
   播放期间沼泽缓缓变大（引擎缩放，无 IMAGE RATE 数据佐证尺寸曲线）；区域内敌人降移速/降命中（引擎施加，无 appendage 文件）
③ 怖拉修现身（blache.obj basic motion = Body1.ani，12 帧 3040ms，F1/F2/F3/F9 攻击盒）：
   BlacheFirst.atk（浮空 200/blow/blood50）——"引发冲击波受暗属性伤害"
④ 演出相位（[etc motion] 11 项：Body2-4/AppearLight/Smoke/Tile/Wave1-5/Bite，引擎按序推进）
⑤ 合上大腭：etc attack info #0 = BlacheFinal.atk（down 击倒/blood70）——第二段伤害
   （按 064 建立的 etc 序列配对规则，etc attack info #0 ↔ etc motion #0 = Body2 相位；Bite.ani 无攻击盒，
   纯视觉相位——两份 .atk 与相位的精确对位是推断，引擎编排不可见）
⑥ 施法者收势（BlacheSmash.ani，4 帧 470ms）+ 按击杀数挂智力 buff（引擎，30s）
```

**重要定性修正（对批次提示的纠偏）**：怖拉修**不是独立 AI 召唤物**——整条链是"沼泽计时 → 出场 → 咬合"的
**固定时序演出 PO**（blache.obj 无行为 nut、无 AI 文件、多相位动画线性推进），手册 §6.3"召唤物独立 AI"缺口
**不适用于本技能**；真正撞上的是下表所列缺口（沼泽 debuff 消费、CD 重置门面、智力消费链）。

### 2.3 被动对象 / appendage

- **blacheswamp.obj**（实测）：name 怖拉修 / [layer] bottom / [floating height] 0 / pass all 1000 /
  [basic motion] 空串（无相位 1 动画）+ [etc motion] Start.ani——沼泽本体的数据面（减速/降命中无对应节，引擎施加）。
- **blache.obj**（实测）：name 怖拉修 / [floating height] 0 / pass all 1000 /
  [basic motion] Body1.ani + [attack info] BlacheFirst.atk；
  [etc motion] 11 项（§2.2 ④）+ [etc attack info] BlacheFinal.atk；
  [object destroy condition] 未列出（播完即毁类，064 同族）。
- **appendage：无**（`sqr\character\swordman\appendage\` 仅 7 个文件，无 blache/swamp，实测）。
- PO 注册：load_state 无 pushPassiveObj（两个 .obj 由引擎直接实例化）。

两份 .atk 实测关键值：
- **BlacheFirst.atk**：damage bonus 100000 / absolute damage 103564 / magic / **暗属性** / damage reaction / push 0 / **lift up 200** / blow / blood 50 / stuck -20
- **BlacheFinal.atk**：absolute damage 13564 / magic / 暗属性 / **down** / push 0 / lift 0 / blood 70 / stuck -20

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒（偏移+尺寸口径） | 备注 |
|---|---|---|---|---|---|
| character\…\blachecast.ani（槽87） | 2 | 480ms（80+400） | 无 | 无 | 施法（sm_body） |
| character\…\blachesmash.ani（槽88） | 4 | 470ms（90×3+200） | 无 | 无 | 收势合腭 |
| passiveobject\…\blache\start.ani | 4 | 360ms | 无 | 无 | 沼泽起幕 |
| swampfloor.ani | 22 | **11890ms** | 无 | 无 | 沼泽铺地主视觉（变大的载体） |
| body1.ani | 12 | 3040ms | 无 | **F1/F2/F3**：偏移(-400,-60,0) 尺寸(800,120,200)；**F9**：(800,120,300) | 出场冲击判定（大盒：8×1.2×2 单位，偏后 4 单位） |
| body2/3/4.ani | 12 | 3040ms | 无 | 无 | 演出相位（同图集） |
| bite.ani | 3 | 270ms | 无 | 无 | 合腭（纯视觉） |
| appearlight / smoke / tile | 4/9/5 | 690/3540/3240ms | 无 | 无 | 现身光/烟/地裂 |
| wave1~5.ani | 7/7/7/5/5 | 540~720/450ms | 无 | 无 | 冲击波五连 |
| bubble1/2、swampsplash、smokeparticle、splashparticle | 7/10/9/1/4 | — | 无 | 无 | 沼泽泡/泥溅/尘 |

全链时长（推断）：480（施法）→ 沼泽 ~11.9s → 出场 3040ms → 咬合/收势 ~470ms ≈ **16 秒级演出**。
`.als` 边车：两侧均无（PO 目录与角色 animation 目录 ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Blache.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Blache.skl` | ✅ | 技能数据（23 列） |
| 注册行 | load_state 行 145（状态 50/技能 87） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 主 nut | blache.nut（14 行） | `…\pvf\sqr\character\swordman\blache\blache.nut` | ✅ | 仅 CD 重置钩子（编排引擎内置） |
| ap nut | — | `…\pvf\sqr\character\swordman\appendage\`（无） | ⛔ 不存在 | 智力 buff 引擎施加 |
| .chr 条目 | etc motion #87/#88（行 1060/1061） | `…\pvf\character\swordman\swordman.chr` | ✅ | BlacheCast/BlacheSmash.ani |
| 角色 .ani | blachecast.ani / blachesmash.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\`（grep 无） | ⛔ 不存在 | 伤害全在 PO atk |
| PO 定义 | blache.obj / blacheswamp.obj | `…\pvf\passiveobject\character\swordman\` | ✅ | §2.3 |
| PO .ani | blache 目录 20 个 | `…\pvf\passiveobject\character\swordman\animation\blache\` | ✅ | §2.4 |
| PO .atk | blachefirst.atk / blachefinal.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 浮空 200 / 击倒 |
| PO 行为 nut | —（无） | sqr 白名单内 grep blache 仅命中壳 nut 与 header | ⛔ 不存在 | 相位推进引擎内置 |
| 施法特效 | ballhead.ani、balltail1-4.ani、smash.ani | `…\pvf\character\swordman\effect\animation\blache\` | ✅ | 鬼神珠/拖尾/收势（引擎绘制，无引用者） |
| 装备层 | *blache*.ani ×152 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法/收势动画 | 必需（共享） | ✅ 已在库 |
| b_01_down01_d.img | sprite_character_swordman_effect_blache.NPK | 沼泽起幕 start | 必需 | ❌ |
| b_01_down02_n.img | 同上 | 沼泽铺地 swampfloor（核心视觉） | 必需 | ❌ |
| monsterbody_03_down_n.img | 同上 | 怖拉修主体 body1-4 | 必需 | ❌ |
| b_02_down_d.img | 同上 | 现身光 appearlight | 必需 | ❌ |
| wave.img | 同上 | 冲击波 wave1-3 | 必需 | ❌ |
| b_03_up02_d.img | 同上 | 合腭 bite | 必需 | ❌ |
| monsterbody_03_up_n.img | 同上 | 地裂 tile | 可选 | ❌ |
| b_02_up_n.img + monstersmoke_03_up_n.img | 同上 | 烟雾 smoke | 可选 | ❌ |
| b_01_up02_n.img | 同上 | 沼泽泥溅 swampsplash | 可选 | ❌ |
| bubble01_03_up_n.img / bubble02_03_up_n.img | 同上 | 沼泽泡 | 可选 | ❌ |
| dust.img / b_d_up.img | 同上 | 粒子尘/泥溅 | 可选 | ❌ |
| b_03_up01_n.img | 同上 | wave4/5 前段 | 可选 | ❌ |
| b_01_up01_d.img | 同上 | 收势 smash | 可选 | ❌ |
| energy_blue.img | 同上 | 鬼神珠 ballhead | 可选 | ❌ |
| sword_light_tail.img | sprite_character_swordman_effect_hundredsword.NPK | 珠拖尾 balltail1-4（**跨技能借图**，L14） | 可选 | ❌ |

缺失 img：必需级 6 张 + 可选级 12 张（本族 NPK 一次提取全覆盖；跨 NPK 1 张）。img 版本红线由提取时把关。

## 5. 实现方案草案（深简化"定时区域风暴"——机制近似度约 70%：三段伤害时序与判定全保，沼泽 debuff/CD 重置/智力加成砍掉）

### 内容件清单

1. **`DotNet~/Skills/BlacheSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发范式；**站桩引导版**——DNF 施法者施法后自由行动，
   我们无"技能期间放开控制"能力，demo 采用 5 秒站桩引导作为第一版，见 §7）
   - `CooldownMs=30000`（DNF 原值 140000，demo 缩短）；`TotalTimeMs=5000`（沼泽压缩为 4s + 爆发，DNF 原链 ~16s）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanBlacheCast)`（480ms 后由视图自然停末帧）+ `ctx.CreateArea(AreaIds.BlacheSwamp, ctx.GetTargetPosition())`——沼泽区在施法点生根（Area 自治，不随人）。
   - OnUpdate（GetElapsedMs 驱动 + SubState 守卫）：
     - `≥4000 && SubState==0`：`ctx.CreateArea(AreaIds.BlacheErupt, 施法点)` 出场冲击 + `ctx.PlayAnim(AnimId.SwordmanBlacheSmash)` + `ctx.SetSubState(1)`；
     - `≥4600 && SubState==1`：`ctx.CreateArea(AreaIds.BlacheBite, 施法点)` 合腭二段 + `ctx.SetSubState(2)`。
   - OnEnd：`ctx.PlayDefaultAnim()`。
   - 施法点记录：`GetTargetPosition()` 是施放瞬间快照（现成门面），回滚安全。
2. **`DotNet~/Areas/BlacheSwampArea.cs : AreaDefinition`**（沼泽持续区，同 FireCircleArea Tick 范式）
   - `TotalTimeMs=4000`、`TickTimeMs=1000`、`HalfExtents=(4.7,1.0,4.7)`（930px 范围折算 ÷100，8 格外圈）、
     `TickActions={MeleeHit}`、`HitReaction{Damage=10, HitstunMs=0, KnockbackX=0, LaunchY=0}`（沼泽侵蚀轻伤害——
     **DNF 是减速/降命中 debuff，我们移速数值零消费（R2-A7），用轻伤害占位"沼泽在起作用"**）；
   - `ViewAnimId=AnimId.BlacheSwampFloor`（swampfloor.ani 22 帧 11.89s，Area 视图播前 4s：速度对齐由 json 帧表天然承担，翻译时不必裁剪）。
3. **`DotNet~/Areas/BlacheEruptArea.cs : AreaDefinition`**（出场冲击，同 ReleaseWaveArea 一次性爆发范式）
   - `TotalTimeMs=3040`（body1 时长）、`EnterActions={MeleeHit}`、`HalfExtents=(4.0,0.6,1.0)`（Body1 盒 x[-800,0] 偏后 4 单位——demo 简化为以施法点为中心 (4.0,1.2,2.0)）、
     `HitReaction{Damage=250, HitstunMs=800, KnockbackX=0, LaunchY=200}`（BlacheFirst 原值 lift200/blow → 浮空冲击波）；
   - `ViewAnimId=AnimId.BlacheBody1`（12 帧主体）+ `ViewBackAnimId=AnimId.BlacheAppearLight`（现身光背层，boomback 同构）。
4. **`DotNet~/Areas/BlacheBiteArea.cs : AreaDefinition`**（合腭终结）
   - `TotalTimeMs=470`、`EnterActions={MeleeHit}`、`HalfExtents=(4.0,1.2,3.0)`（F9 盒 800×120×300）、
     `HitReaction{Damage=400, HitstunMs=1000, KnockbackX=0, LaunchY=0}`（BlacheFinal down → 击倒表现走硬直+贴地，同 064 相位 2 处理）；
   - `ViewAnimId=AnimId.BlacheBite`（270ms 合腭）+ `ViewEndAnimId=AnimId.BlacheWave`（wave 系收尾）。
5. **无需新 Buff/Action**（MeleeHit 现成；智力 buff/减速均砍）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 50 + BlacheCast/Smash | `BlacheSkill` + 两个 AnimId |
| blacheswamp.obj（bottom 层自治 PO） | `BlacheSwampArea`（CreateArea 于施法点） |
| blache.obj 多相位（basic→etc 11 项） | 两个 Area 按时序创建（出场/咬合；L9 多相位→Area 编排同构） |
| BlacheFirst/BlacheFinal.atk | 两个 Area 的 `HitReaction` |
| 沼泽变大（引擎缩放 11.89s） | 固定尺寸 + swampfloor 帧序列自带"蔓延"观感（对象缩放延后档） |
| 吞噬鬼神 → 重置 CD/加速出场/增伤 | **新缺口：跨技能 CD 重置门面**（§8 上报）；demo 跳过（无鬼神技能实现） |
| 按击杀加智力（30s） | 属性数值无消费链（R1-A4）——跳过 |

### 注册点清单（草案号段，A14 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.Blache=19` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanBlacheCast=77、SwordmanBlacheSmash=78、BlacheSwampStart=79、BlacheSwampFloor=80、BlacheBody1=81、BlacheAppearLight=82、BlacheWave=83、BlacheBite=84、BlacheSmoke=85 |
| AreaId | `AreaDefinition.cs` | BlacheSwamp=9、BlacheErupt=10、BlacheBite=11 |
| json / 图集 | LSAnimClipRegistrar / BuildAtlas | json ×6~9；img 必需 6 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 140000ms | 30000 |
| 沼泽范围/时长 | 930px / ~11.9s | 4.7 单位 / 4000ms |
| 出场触发 | 沼泽满幅（引擎） | ElapsedMs 4000 |
| 冲击波伤害 | col3 2779%（lift200/blow/暗） | 250/硬直 800/浮 200 |
| 合腭伤害 | col4-5 10296%~30160%（down） | 400/硬直 1000 |
| 沼泽 debuff | 降移速 51%+ / 降命中 8.8%+ | 轻伤害 tick 10/s 占位 |
| 智力 buff | 30s，每杀 +9~384 | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 角色与 PO 共 27 个 .ani | 节面常规（实测无规则外节；F0 空路径 [IMAGE] 空白帧惯例可处理） | **现有 ani 子命令全覆盖** |
| Blache.skl（23 列 + static 10 槽） | `.skl` 无子命令 | 手抄 9 向量对位值可接受；23 列规模已偏大，并入既有缺口（批量化收益例证） |
| blachefirst/final.atk | `.atk` 无子命令（[absolute damage]/[stuck]/[blood] 等 HitReaction 外字段） | 手抄；[absolute damage] 建议纳入 atk 子命令字段设计（R2-A8 记档清单再+1） |
| blache.obj / blacheswamp.obj（多相位 12 项） | `.obj` 无子命令 | 并入既有缺口（L9 相位建模建议）；本技能 2 个 obj 手工映射 3 Area 可接受 |

计 3 条既有缺口（.skl/.atk/.obj），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 施法者施法后自由行动，怖拉修自治演出 16s | 技能期间放开控制/演出期自治（无此机制） | 5s 站桩引导版（§5）；升级路径 = Area 接续创建门面（见 §8） |
| 沼泽降移速/降命中 | **NumericType.Speed 移动端零消费（R2-A7）+ 命中率系统不存在** | 轻伤害 tick 占位 |
| 沼泽缓缓变大（930px 满幅） | 对象整体缩放（延后，IMAGE RATE 同族） | 固定尺寸；swampfloor 22 帧自带蔓延观��� |
| 吞噬鬼神：重置其 CD/加速出场/增伤 min~max | **跨技能 CD 重置门面（新缺口，§8）+ Buff 查询门面（R1-A3）** | 跳过（无鬼神技能在库；CD 重置待门面） |
| 按击杀数加智力（30s） | 属性数值无伤害消费链（R1-A4） | 跳过 |
| 需要鬼神数量门槛（static[0]） | 同上 + 无技能习得查询 | 跳过（此 pvf 本身已置 0） |
| 出场屏震/闪屏（引擎） | 屏震/闪屏（延后） | 跳过 |
| 暗属性伤害 | 元素属性系统（缺失） | 无属性直伤 |
| 鬼神珠/拖尾（引擎绘制特效无引用者） | 引擎特效无声明式来源（R1-A4 064 上报同族） | 跳过（能量珠演出省略） |

## 8. 存疑与缺口上报

**未考证项**
1. 引擎子状态切换点（substate==1 的确切时刻——推断为怖拉修出场即触发 CD 重置）。
2. 两份 .atk 与 .obj 相位的精确对位（BlacheFinal ↔ etc motion #0=Body2 是按 064 配对规则的推断；Bite 相位无攻击盒与"合腭二段伤害"的对应关系存疑）。
3. static data 槽 2/3/5/6（930/10/1000/200）语义（推断槽 2/4 为沼泽初始/满幅尺寸，槽 5/6 为演出计时参数）。
4. level property col0/col1（降速/降命中）读法为推断（×0.1 后超 100%，必有引擎钳制）。
5. 沼泽变大的尺寸曲线（引擎运行时行为，无数据文件）。
6. 152 张装备层 .ani 仅 find 计数未逐一核对。

**系统级缺口**
- **跨技能 CD 重置门面（新缺口上报）**：`endSkillCoolTime(skillId)` 语义 = 技能 A 的效果重置技能 B 的冷却。现有 LSSkillComponent 只有自身 CD 记录，无 ctx 层"重置他技能 CD"门面；鬼泣鬼神系（本技能吞噬重置 8 技）与各类"重置系"被动都会撞。建议 SkillContext 增 `ResetSkillCooldown(skillId)`（实现代价低：LSSkillComponent 已有 CD 存储）。
- **召唤物定性修正**：怖拉修=固定时序演出 PO，非独立 AI——"召唤物独立 AI"缺口**不适用于本技能**；但"长时序演出 PO（>10s 自治）"暴露姊妹需求：**Area 接续创建**（沼泽到时自动爆出下一区域；现只能靠技能 OnUpdate 长站桩驱动）。可与 064 §7 已记档的"AreaDefinition 加 PhaseActions"合并立项。
- 施法者演出期自治（施法后技能结束、效果继续）：已有 Area 自治可覆盖伤害面，收势动画（470ms 后重演）无通道——记档。

**给下轮的经验**：老一代觉醒技（state 号 ≠ 技能号、nut <20 行）= "引擎内置编排 + 纯数据文件"形态——分析时直接读
`passiveobject\character\swordman\<名>.obj` 的 basic/etc motion 序列 + 同名 attackinfo\*.atk，角色侧只查 .chr etc motion 两三条；
不要再找行为 nut（没有）。CD 140s/演出 16s 的技能，实现草案先做"压缩时序"决策再对数值。
