# 雷神降世 : 裁决（swordman_lightninggod）

> 技能ID 244 | 级别 A | 可实现性 🔶（主干=固定时序"召唤阵吸附区 + 天雷 + 爆炸"三段 Area 编排，吸附走负击退拉拽（L22 已通）；感电 Buff 可新建，但"感电状态增伤 50%"依赖属性消费链缺失被砍；屏震/闪屏/音效延后） | 分析日期 2026-08-22 | 批次 A18

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 雷神降世 : 裁决 | `skill\Swordman\swordman_lightninggod.skl` [name] |
| 英文名 | swordman_lightninggod（取 skl 文件名；无 [name2]，实测） | 同上 |
| 职业 | 阿修罗（85 级二觉大招；[second growtype maximum level] 第 10 位=30，四技槽位互证见 231 文档） | 同上 + 技能名常识 |
| 学习等级 | 85 | 同上 [required level] |
| 最高等级 | 40（二觉档上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↓↑→→ + Z | 同上 [command] |
| CD | 180000 ms（pvp 起手 600000） | 同上 [cool time] |
| MP | 1500 → 5000 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×10 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14 | 同上 [executable states] |
| static data | **无 [static data] 节**（实测 grep 计数 0） | — |
| 一句话效果 | 召唤雷神，把范围内敌人吸附到中心雷电审判：出现击倒 → 召唤阵周期伤害+感电 → 天雷之裁决 → 爆炸；对感电敌人攻击 +50% | 同上 [explain] |

**level property（9 列，Lv1 → Lv40 首末值，9 向量全 -1 源列直读，实测无歧义）**：

| 列 | 模板变量 | Lv1 值 | 印证处 |
|---|---|---|---|
| col0 | 雷神出现时攻击力 | 3576% | 写包 dword2 → PO atk33 |
| col1 | 召唤阵攻击力 | 930% | 写包 dword3 → PO atk34 |
| col2 | 感电几率 | 100% | 写包 dword6 |
| col3 | 感电Lv | Lv80 | 写包 dword7 |
| col4 | 感电持续时间 | 3000×0.001=3.0 秒 | 写包 dword8 |
| col5 | 感电伤害 | 2147 | 写包 dword9（感电跳伤） |
| col6 | [天雷之裁决]攻击力 | 6143% | 写包 dword4 → PO atk35 |
| col7 | [天雷之裁决]爆炸攻击力 | 22015% | 写包 dword5 → PO atk36 |
| col8 | 攻击感电敌人时攻击力增加率 | 50% | 写包 dword10 → ap_lightninggod |

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 78（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/lightninggod/lightninggod.nut", "swordman_lightninggod", 244, 244);
// swordman_header.nut 行 76/104/333（实测）：STATE/SKILL_SWORDMAN_LIGHTNINGGOD <- 244；CUSTOM_ANI_LIGHTNINGGOD <- 163
```

F7 变体的**极简壳**：主 nut 仅 70 行——播动画 + 一笔写包，**全部演出与判定在共享 PO 24370 case 244 的六态时序里**（比 slashofhell 的 PO 更重度）。

### 2.2 主 nut 逐回调（lightninggod.nut，70 行）

- **onSetState**（唯一实质回调）：播动画 163（LightningGod.ani 8 帧 560ms，无 flag）→ 立即写包
  `(244, subType=1, col0, col1, col6, col7, 感电几率col2, Lv col3, 时长 col4, 伤害 col5, 增伤 col8)` → `sq_SendCreatePassiveObjectPacket(24370, 0, x=350, 0, 0)`（身前 350px）→ 播音效 SM_LIGHTNINGGOD_CAST。
- **onEndCurrentAni**（560ms 后）：回 STAND——**角色即自由，雷神 PO 自治演出**。
- 无 onProc/onKeyFrameFlag/checkCommandEnable 特殊逻辑。

### 2.3 被动对象 / appendage

**共享 PO 24370 case 244 subType 1（雷神时序，setcustomdata:523 + setstate:401 实测）**：

四组攻击信息预设：atk33 出现 / atk34 召唤阵 / atk35 天雷 / atk36 爆炸，**全部注入 ACTIVESTATUS_LIGHTNING 感电**（几率100/Lv80/3s/跳伤2147）；`addRate=col8`（+50%）存 var；→ setState 10。

| state | 动画（custom） | 攻击 | 时长 | 内容（实测） |
|---|---|---|---|---|
| 10 | 48 = lightning_create/1/god/1_god.ani（22 帧 1540ms） | atk33 | 1540ms | **雷神出现**（倒地反应；floor 1_magic 地面阵；flag F8=1@560 无脚本处理） |
| 11 | 49 = 2_god.ani（6 帧 420ms） | atk34 | 420ms | **召唤阵**：`setTimeEvent(0, 120ms, ∞)`——**120ms 间隔持续命中**（else.nut case 244 timeEvent0=resetHitObjectList）；记录 9 个落雷偏移点（±225/±320 等）；timeEvent1 ×5 触发天雷子 PO |
| 12 | 50 = 3_god.ani（14 帧 980ms） | — | 980ms | 黑屏闪屏过渡；timeEvent2=100ms×(场上 24370 数) |
| 13 | 51 = 4_god.ani（5 帧 350ms） | — | 350ms | 蓄势（floor 4_magic） |
| 14 | 52 = 5/god/god_god_normal.ani（7 帧 490ms） | atk35 | 490ms | **[天雷之裁决]落下**（白闪；子 PO 小剑=Small_Fall/Keep_small_sword + Ball 系） |
| 15 | 53 = exp/exp_normal_04.ani（15 帧 1050ms） | atk36 | 1050ms | **终结爆炸**（屏震 10 + 末段黑闪） |

onEndCurrentAni：state≠15 → +1 推进；=15 → 销毁。**全链 ≈ 4730ms 固定时序**。

**subType 2（天雷子 PO，else.nut timeEvent1 逐个创建，实测）**：state 10-13 → anims 54-57（Small_Fall 落剑 290ms / Small_Keep / Ball_start / Ball_Keep 小球），携 col6/col7 伤害在 9 个偏移点落下；state 13 销毁。

**ap_lightninggod.nut（45 行，感电增伤 appendage，PO onAttack 挂载）**：`getImmuneTypeDamageRate` 钩子——受害者带 LIGHTNING 状态且伤害源是本 PO（var skill==244）→ 伤害 += col8（**+50% 对感电**）。proc 仅存活校验。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\lightninggod.ani（槽163） | 8 | 560ms | 无 | 无 | .als：cast（[add] 帧1/层10001）+ cast_floor（层-1） |
| PO 1_god.ani（state10） | 22 | 1540ms | F6=0(@420)/F8=1(@560)（无脚本处理） | **F7-F11**（出现判定） | .als 挂 21 层（god_normal/scarf/hand/elec_1-7/smoke/floor_gather 等）——雷神主体合成 |
| PO 2_god.ani（state11） | 6 | 420ms | 无 | **F0-F5**（全帧召唤阵判定） | |
| PO 3_god.ani（state12） | 14 | 980ms | 无 | **F0-F13** | |
| PO 4_god.ani（state13） | 5 | 350ms | 无 | F0-F4 | |
| PO god_god_normal.ani（state14） | 7 | 490ms | F1=0(@70) | **F1-F5** | |
| PO exp_exp_normal_04.ani（state15） | 15 | 1050ms | 无 | **F0-F8/F10/F12** | 爆炸主判定 |
| PO Small_Fall_small_sword_1-1.ani（subType2） | 5 | 290ms | 无 | 无 | 落剑视觉 |
| PO Ball_start_small_ball.ani | 7 | 490ms | 无 | 无 | 雷球视觉 |

`.als` 边车：角色 1 个 + lightninggod mod 目录 159 文件中多个（1_god.ani.als 等）；节面含 [FLIP TYPE]（见 §6）。

**atk 实测**（PO 侧 4 个，`…\script_sqr_nut_qq506807329\swordman\attackinfo\`）：
- `LightningGod.atk`（atk33）：magic / **dark element（暗——疑似复制模板笔误，雷神出场却标暗属性）** / **down** / lift 150 / [ignore weight] 1 / hit wav R_DARKSHOT_HIT。
- `LightningGodAttract.atk`（atk34）：magic / light / damage 反应 / push 0 / **[knuck back] -1（负值=向心拉拽，L22）** / hit info etc。
- `LightningGodPushSword.atk`（atk35）：magic / light / damage / lift 300 / hit down / blood 40 0.5。
- `LightningGodExp.atk`（atk36）：magic / light / **down** / lift 500 / hit down。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_lightninggod.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_lightninggod.skl` | ✅（258 行） | 9 列数据（无 static） |
| 注册行 | load_state 行 78（244/244） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 76/104/333 | 同文件 | ✅ | 状态 244/动画 163 |
| 主 nut | lightninggod.nut | `…\pvf\sqr\character\swordman\lightninggod\lightninggod.nut` | ✅（70 行） | 极简壳 |
| ap nut | ap_lightninggod.nut | 同目录 | ✅（45 行） | 感电增伤钩子 |
| 共享 PO | share_obj\swordman\ case 244（subType1/2） | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅（L20） | 全演出时序 |
| .chr 条目 | etc motion #163（行 1136） | `…\pvf\character\swordman\swordman.chr` | ✅ | LightningGod.ani |
| 角色 .ani/.als | lightninggod.ani + .als | `…\pvf\character\swordman\animation\` | ✅ | 560ms 施法 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 不存在 | 伤害全在 PO |
| PO .obj | qq506807329new_swordman_24370.obj（etc motion #48-57 / etc attack #33-36） | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ | 雷神动画/atk 表 |
| PO .ani/.als | lightninggod 目录 159 文件（create/1-5/exp/smallfall/smallkeep/ballstart/ballkeep） | `…\passiveobject\script_sqr_nut_qq506807329\swordman\animation\lightninggod\` | ✅ | 演出视觉 |
| PO .atk | LightningGod/Attract/PushSword/Exp.atk | `…\passiveobject\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ | §2.4 |
| 施法特效 | cast.ani / cast_floor.ani（Effect\LightningGod\ 2 文件） | `…\pvf\character\swordman\effect\animation\LightningGod\` | ✅ | .als 层 |
| 装备层 | *lightninggod*.ani ×76 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |

## 4. 资源需求

| img（按 NPK 族归并） | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_…avatar_skin.NPK | 角色动画 | 必需（共享） | ✅ |
| cast.img / cast_floor.img | sprite_character_swordman_effect_lightninggod_{cast,cast_floor}.NPK | 施法（.als 层） | 必需 | ❌ |
| god_normal.img / god_dodge.img / scarf.img / hand.img / fall.img | sprite_…lightninggod_{god_normal,god_dodge,scarf,hand,fall}.NPK | 雷神主体（state10 的 .als 21 层合成源） | 必需 | ❌ |
| floor.img / magic.img / floor_gather.img / floor_elec(_2).img | 同族 | 地面召唤阵（state10-14 各段 floor） | 必需 | ❌ |
| glow.img / glow_2.img / light(_2).img / start.img / start_elec.img / pa.img / shock.img / smoke.img | 同族 | 出现/蓄势特效 | 必需（简版可裁） | ❌ |
| big_sword.img / small_sword_1/2.img / small_ball.img / circle.img / sword 系 | 同族 | 天雷子 PO 视觉 | 必需（天雷段） | ❌ |
| exp_normal / exp_1/2/3 / exp_elec.img | 同族 | 终结爆炸 | 必需 | ❌ |
| timeslash_circle.img | sprite_character_swordman_effect_attimeslash_timeslash_circle.NPK | 跨族借图（L14） | 可选 | ❌ |

缺失 img：必需级约 22 张（30 张 lightninggod 族一次提取全覆盖 + attimeslash 1 张跨族）。全部未入库。img 版本红线由提取时把关。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/LightningGodSkill.cs : SkillLogic`**（同 BlacheSkill 站桩引导版范式）
   - `CooldownMs=180000`（demo 30000）；`TotalTimeMs=4700`（PO 全链 4730ms 原速保留——本技能演出即判定本体，不压缩）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanLightningGod)`（560ms 后停末帧/回默认）+ 站桩；记录施放点 `ctx.GetTargetPosition()`。
   - OnUpdate（ElapsedMs + SubState，五次 Area 创建全部以施放点为锚——`CreateArea(id, GetTargetPosition())` 快照，雷神不随人）：
     - `≥0 && SubState==0`：`ctx.CreateArea(AreaIds.LightningGodAppear, 施放点前 3.5)`——雷神出现击倒区 + SetSubState(1)；
     - `≥1540 && SubState==1`：`ctx.CreateArea(AreaIds.LightningGodCircle, 施放点)` 召唤阵 + SetSubState(2)；
     - `≥2940 && SubState==2`：`ctx.CreateArea(AreaIds.LightningGodSword, 施放点)` 天雷 + SetSubState(3)；
     - `≥3430 && SubState==3`：`ctx.CreateArea(AreaIds.LightningGodExplosion, 施放点)` 爆炸 + SetSubState(4)。
   - OnEnd：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/LightningGodAppearArea.cs : AreaDefinition`**（出现击倒）
   - `TotalTimeMs=1540`、`EnterActions={MeleeHit}`、`HalfExtents=(3.5,1.0,2.0)`（1_god 盒折算+350px 前移）、
     `HitReaction{Damage=60, HitstunMs=600, KnockbackX=0, LaunchY=150, ProcBuffId=BuffIds.Shock, ProcChance=100}`（atk33 down/lift150 + 感电 ProcBuffId，L6 通道）、`ViewAnimId=AnimId.LightningGod1`（1_god 21 层取主层）。
3. **`DotNet~/Areas/LightningGodCircleArea.cs : AreaDefinition`**（召唤阵：吸附+周期伤害，FireCircleArea Tick 范式）
   - `TotalTimeMs=1400`（state11-12 合并）、`TickTimeMs=120`（**DNF resetHitObjectList 间隔原值直用**，L19 同段定时档）、
     `HalfExtents=(3.2,1.0,3.2)`、`EnterActions={MeleeHit}`、`TickActions={MeleeHit}`、
     `HitReaction{Damage=20, HitstunMs=200, KnockbackX=-40, LaunchY=0, ProcBuffId=BuffIds.Shock, ProcChance=100}`——**KnockbackX 负值=向心吸附**（atk34 knuck back -1 拉拽语义，L22 已通）；`ViewAnimId=AnimId.LightningGodFloor`。
4. **`DotNet~/Areas/LightningGodSwordArea.cs : AreaDefinition`**（天雷之裁决）
   - `TotalTimeMs=490`、`EnterActions={MeleeHit}`、`HalfExtents=(3.2,1.5,3.2)`（9 落点外包络）、
     `HitReaction{Damage=120, HitstunMs=500, KnockbackX=0, LaunchY=300}`（atk35 lift300 原值）、`ViewAnimId=AnimId.LightningGodSword`。
5. **`DotNet~/Areas/LightningGodExplosionArea.cs : AreaDefinition`**（终结爆炸）
   - `TotalTimeMs=1050`、`EnterActions={MeleeHit}`、`HalfExtents=(3.5,1.5,3.5)`、
     `HitReaction{Damage=250, HitstunMs=1000, KnockbackX=0, LaunchY=500}`（atk36 down/lift500）、`ViewAnimId=AnimId.LightningGodExplosion`。
6. **`DotNet~/Buffs/ShockBuff.cs : BuffDefinition`**（感电，BleedBuff 同构 DoT）
   - `TotalTimeMs=3000`（col4 原值）、`TickTimeMs=1000`、`TickActions={MeleeHit}` 跳伤 2147/3≈**每秒 15**（demo 对齐 Bleed 档）——DNF 感电语义"带电期间受击附加伤害"简化为定时跳伤。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 244 + 560ms 施法 | `LightningGodSkill` + `AnimId.SwordmanLightningGod` |
| PO subType1 六态时序（动画结束链推进） | OnUpdate ElapsedMs 时序创建 4 Area（L9 多相位→Area 编排；Blache 同构） |
| atk34 吸附（knuck back -1） | `HitReaction.KnockbackX` 负值向心拉拽（L22） |
| 120ms resetHitObjectList 持续命中 | Area `TickTimeMs=120`（同段定时档，L19） |
| 感电 ACTIVESTATUS_LIGHTNING | `ProcBuffId=Shock` 通道 + ShockBuff DoT 简化 |
| 感电敌人增伤 50%（ap getImmuneTypeDamageRate） | 属性数值无伤害消费链（R1-A4）——**砍**，§7 |
| subType2 天雷子 PO（9 落点小剑） | 单个大 Area 一次结算（落点细节为纯视觉差异） |
| 屏震 10/黑闪/白闪/音效 | 延后跳过 |
| 光属性伤害 | 元素属性系统缺失 → 无属性直伤 |

### 注册点清单（草案号段，A18 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.LightningGod=25` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanLightningGod=115、LightningGod1=116、LightningGodFloor=117、LightningGodSword=118、LightningGodExplosion=119 |
| AreaId | `AreaDefinition.cs` | LightningGodAppear=22、LightningGodCircle=23、LightningGodSword=24、LightningGodExplosion=25 |
| BuffId | `BuffDefinition.cs` | Shock=12 |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×5~6；img 必需 ~22 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 180000ms | 30000 |
| 演出总长 | 4730ms（六态 1540+420+980+350+490+1050） | 4700 |
| 出现伤害 | col0 3576%（down/lift150） | 60/硬直 600/浮 150 + 感电 |
| 召唤阵 | col1 930%/120ms 间隔 | 20/tick/吸附 -40 |
| 天雷 | col6 6143%（lift300） | 120/硬直 500/浮 300 |
| 爆炸 | col7 22015%（down/lift500） | 250/硬直 1000/浮 500 |
| 感电 | 100%/Lv80/3s/跳伤 2147 | ShockBuff 3s 每秒 15 |
| 感电增伤 | col8 +50% | 砍（消费链缺失） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 159 个 PO .ani + 角色 .ani | 节面常规（FRAME/DELAY/IMAGE/RGBA/GRAPHIC EFFECT 已支持） | 现有 ani 子命令全覆盖 |
| 1_god.ani.als 等 | `[add]`/`[none effect add]` 已支持；21 层大 overlay（层号 -1~10021） | 现有 als 子命令覆盖；层数多但结构常规 |
| lightninggod 系 .ani（magic/pa/glow 等 45 处） | **`[FLIP TYPE]`（HORIZON 帧镜像）——新节（本批 231 首报）** | 同 231 §6：flipType 字段 |
| swordman_lightninggod.skl（9 列） | `.skl` 无子命令（既有） | 手抄 9 值可接受 |
| 4 个 PO .atk | `.atk` 无子命令（既有）；`[knuck back]`/`[ignore weight]`/`[hit wav]` 为 HitReaction 外字段（R2-A8 记档清单再+2） | 手抄 |
| 24370 .obj | `.obj` 无子命令（既有） | etc #48-57/#33-36 手工映射 |

计 3 条既有缺口（.skl/.atk/.obj）+ 1 条新节（[FLIP TYPE]）+ atk 字段记档 2 项。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 感电状态增伤 50%（对带电目标全伤害加成） | 属性数值无伤害消费链（R1-A4，三实证后再+1） | 砍；感电只保留 DoT 表现 |
| 吸附=向心持续拉拽（knuck back -1，120ms 重复施加） | 负 KnockbackX 拉拽已通（L22），但方向=径向向心 vs DNF 恒向阵心（L24 方向语义差异已知） | 负击退每 tick 重复（阵中心即 Area 中心，径向=向心，本技能恰好无 L24 歧义——判定与拉力同源） |
| 感电"受击附加伤害"语义 | 无受击事件钩子（DoT 近似） | ShockBuff 定时跳伤 |
| subType2 天雷 9 落点各自小判定 | 位置类确定性随机（R2-A10 已记档） | 单个大 Area 一次结算 |
| 屏震（10 级）/黑屏白屏闪屏 ×4/音效 | 屏震闪屏音效（延后） | 跳过 |
| 光/暗属性（atk33 标暗疑笔误） | 元素属性系统缺失 | 无属性直伤 |
| 演出期间角色自由（PO 自治 4.7s） | 技能期间放开控制缺失 | 4.7s 站桩引导版（Blache 同决策） |

## 8. 存疑与缺口上报

**未考证项**
1. state10 的 F6=0/F8=1 两个 flag 与 state14 的 F1=0——无脚本处理，疑引擎音效/特效标记。
2. state11 记录的 9 个偏移点精确坐标语义（成对 (x,z) 推断；落点分布为"环+随机"观感，未逐点解）。
3. state12 的 timeEvent2（100ms×场上 24370 数）作用——疑天雷子 PO 落速控制。
4. LightningGod.atk 暗属性+R_DARKSHOT_HIT（与雷神题材矛盾，疑复制 atk 模板笔误，DNF 原版如���）。

**新缺口上报（主循环汇总）**
1. **[FLIP TYPE] 翻译节**（本批四技 45 处，231 首报）：帧级水平镜像。
2. **atk 记档字段追加**：`[knuck back]`（负值拉拽方向，与 push aside 并列）、`[ignore weight]`（无视体重浮空）——atk 子命令立项时的字段设计输入（并入 R2-A8 清单）。

**给下轮的经验**：`swordman_` 系若主 nut <80 行且 onSetState 只有一笔 24370 写包（无 flag/onProc），**时序全在 setstate case 表里**——直接读 `share_obj\swordman\setstate.nut` 的 `case <技能号>` 拿六态动画/atk 对照表，再回 .obj etc 表查动画文件名，五分钟可出时序表。
