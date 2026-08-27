# 鲜血暴掠（swordman_bloodsnatch）

> 技能ID 233 | 级别 A | 可实现性 ⛔（抓取/目标控制系统缺失，手册 §6.3 已列档；深简化"定身砸地"近似见 §5） | 分析日期 2026-08-22 | 批次 A14

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鲜血暴掠 | `skill\Swordman\swordman_bloodsnatch.skl` [name] |
| 英文名 | swordman_bloodsnatch（取 skl 文件名；本 skl 无 [name2] 节，实测） | 同上 |
| 职业 | 狂战士（[second growtype maximum level] 12 槽位中第 7/8 位=30/30 → 按"growtype×2（一觉,二觉）"排列即 growtype 3=狂战士 一觉/二觉档，与 87/48 两技能三方互证；血气抓取系=狂战常识） | 同上 |
| 学习等级 | 60 | 同上 [required level] |
| 最高等级 | 40（二觉后上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 2） | 同上 [type] |
| 指令 | →←↓→ + Z（指令施法 MP 优惠 20%/40% 档） | 同上 [command] / [skill command advantage] |
| CD | 30000 ms（固定） | 同上 [dungeon][cool time] |
| MP | 344 → 963（Lv1→40） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×2（[consume item] 3037 2 2） | 同上 |
| 可施放状态 | 8 / 0 / 14（普攻/站立/受击系状态可接） | 同上 [executable states] |
| static data | **无 [static data] 节**（实��） | 同上 |
| 一句话效果 | 抓取前方敌人后，跳起将敌人砸向地面，引发剧烈血气爆炸 | 同上 [explain] |

**level property（4 列，模板两行四占位；Lv1 → Lv40）**：`抓起攻击力 : <int>%% + <int>` = col2 + col0 → **5792% + 22650 → 9549% + 37345**；
`血气爆炸攻击力 : <int>%% + <int>` = col3 + col1 → **8726% + 38841 → 16057% + 71464**。
（向量 `(-1,2)(-2,0)(-1,3)(-2,1)`，L21 读法：-1/-2=level 列；百分比列与固定值列成对，与模板逐占位对应，实测无歧义。）

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 33（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/bloodsnatch/bloodsnatch.nut", "swordman_bloodsnatch", 233, 233);
// swordman_header.nut 行 65/93（实测）：STATE_SWORDMAN_BLOODSNATCH <- 233，SKILL_SWORDMAN_BLOODSNATCH <- 233
// 行 305-310：CUSTOM_ANI_SWORDMAN_BLOODSNATCHCAST_BODY <- 135 …FINISH_BODY <- 140
// 行 481：CUSTOM_ATTACK_SWORDMAN_BLOODSNATCH <- 90
```

状态号 233 = 技能 ID 233（新世代技能，状态号=技能号）。**共享打击 PO 24370** 的注册在行 8
（`pushPassiveObj("common_object/share_obj/share_po_swordman_24370.nut", 24370)`，L20 已记档的引擎内置共享 PO，按写包首 dword 分流——本技能写 233）。

### 2.2 主 nut 逐回调（bloodsnatch.nut，269 行；变量名被 mod 作者混淆，语义经反混淆走读）

**checkExecutableSkill**：`sq_IsUseSkill(233)` → 写包 (0) 切状态 233（标准入口）。
**checkCommandEnable**：STAND 直接可；STATE_ATTACK 里走 `sq_IsCommandEnable(233)`（普攻中取消施放）。

**onSetState（六子状态机，setSkillSubState）**——先统一 `sq_StopMove + sq_ZStop`：

| 子状态 | 动画(槽) | 行为 |
|---|---|---|
| 0 起手 | 135（Cast_body，1 帧 80ms） | 仅播动画 |
| 1 前冲 | 136（Dash_body） | 播 `bloodsnatchdash_dust.ani` 挂父尘土特效；存向量 (220, 起点x, 30) |
| 2 抓取 | 137（Catch_body） | 设攻击信息 90 + 倍率 col2（抓起%）+ 攻击力 col0；存向量 (220, 起点x, 70) |
| 3 前冲抓取 | 138（DashCatch_body） | 同 2（攻击信息 90 + col2/col0） |
| 4 旋转跳起 | 139（Spin_body） | 存向量 (当前x, 当前z, 100/2=50, 120, 0, 90)——x 推进 50px、跳跃幅 120、正弦表 0°→90° |
| 5 砸地终结 | 140（Finish_body） | 向量[0]=当前x、[4]=90、[5]=180——正弦表续 90°→180°（下落段） |

子状态 1/2 期间 `sq_SetStaticSpeedInfo(攻速)`（动画随攻速，延后档）。

**onAttack（子状态 1/2/3 的命中回调，抓取核心）**：
```
命中目标为活动单位时：
  if (sq_IsGrabable && sq_IsHoldable && !sq_IsFixture(目标)):     // 可抓三判据（031 同款）
      移除旧 ap_bloodsnatch → 重新 sq_AppendAppendage(ap_bloodsnatch.nut)
      sq_HoldAndDelayDie(目标, obj, …)                            // 控住目标进入合体演出
  if (子状态 != 1):                                               // 冲刺段不放命中特效
      在目标位置创建 bloodsnatchshot1.ani + 层1 叠加 shot2.ani（DRAWONLY）
```

**onKeyFrameFlag（仅子状态 5，帧 4 flag=1）**：
```
写包（233, 倍率=col3 血气爆炸%, 攻击力=col1 固定值）
→ sq_SendCreatePassiveObjectPacket(24370, 0, 100, 0, 0)      // 身前 100px 共享打击 PO（爆炸伤害体）
+ sq_SetMyShake(5, 400) + flashScreen(白 178)
+ 身前 100px 底层 DRAWONLY：bloodsnatchboomb_00.ani + 层1 叠加 _01/_02（爆炸视觉）
```

**onEndCurrentAni（子状态推进）**：
```
0 → 按住前方向键 ? 子状态 1（前冲） : 子状态 3（直接前冲抓取）     // 起手 80ms 内的方向输入分流
2/3 → 没抓到（bool0=false）? 回 STAND（挥空收招） : 子状态 4（抓到，旋转跳起）
5 → 回 STAND
其余 → 子状态 +1
```

**onProc（位移与跳跃弧线，纯函数插值）**：
```
子状态 1/2：x = 起点 + 方向 × uniform(0 → 向量[2]%×220, 当前时间, 动画总长)   // 30%×220=66px / 70%×220=154px
            isMovablePos 撞墙检测（撞墙置 bool1 停止推进）
子状态 4：帧 ≥3 起（帧 3-5 窗口）：x 前推 uniform(0→50)；z = z₀ + 120 × sin(uniform(0°→90°))  // 升到 120px
子状态 5：帧 0-3 窗口：z = z₀ + 120 × sin(uniform(90°→180°))                              // 从顶点落回 0
——4/5 两段拼成正弦半弧跳跃，落地点在身前 50px
```

### 2.3 被动对象 / appendage

**ap_bloodsnatch.nut（84 行，抓取持有 appendage——"目标控制流"子系统的完整第二样本）**：
- 有效性：施法者不在状态 233（或消失）→ 立即失效；
- proc（每帧把被抓者钉在施法者相对位）：
  - 子状态 1/2/3：目标 → 施法者前方 80px（被抓在身前拖着走）；
  - 子状态 4（旋转跳起）：按施法者帧号分档 `-25 → -15 → -20(+z10) → -45(+z75)`——目标被抡到身后并**举过头顶**，且 `setCurrentDirection(反向)`（目标面朝施法者）；
  - 子状态 5（砸地）：帧 <4 保持 `-35(+z75)` 举顶；帧 ≥4 → **前方 80px、z=0（砸到地面）并立即失效释放**；
- onEnd：`sq_SimpleMoveToNearMovablePos(目标, 5000)`（释放后把目标吸附到最近可站位置，防卡墙）。

**爆炸体 = 共享 PO 24370**（引擎内置，L20）：收到写包 (233, col3 倍率, col1 固定值) 后按首 dword=233 分流结算爆炸伤害；本技能无自有 PO .obj/.act（`passiveobject\character\swordman\` 下 grep bloodsnatch 无命中，实测）。爆炸的前景 13 层视觉（bloodsnatchboomf_00~12，末层 .als 链挂 12 子层）无脚本引用者——**播放者未考证**（推断由共享 PO 引擎表现层或引擎直接播放）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒（min/max 口径，÷100=单位） | 备注 |
|---|---|---|---|---|---|
| character\…\bloodsnatchcast_body.ani（槽135） | 1 | 80ms | 无 | 无 | 起手 |
| bloodsnatchdash_body.ani（136） | 4 | 88ms | 无 | F0-F2：x∈[-34,100] y∈[-30,30] z∈[-6,102] | 前冲（盒随行） |
| bloodsnatchcatch_body.ani（137） | 6 | 299ms | 无 | F2/F3：x∈[-79,194] z∈[-2,137] | 抓取判定（宽盒） |
| bloodsnatchdashcatch_body.ani（138） | 6 | 276ms | 无 | F2/F3 同上 | 前冲抓取 |
| bloodsnatchspin_body.ani（139） | 6 | 302ms | 无 | 无 | 旋转跳起 |
| bloodsnatchfinish_body.ani（140） | 10 | 571ms | **F4=1**（爆炸触发） | 无 | 砸地终结 |
| effect\…\bloodsnatchboomb_00.ani | 20 | — | 无 | 无 | 爆炸底层（bloodwave.img） |
| bloodsnatchboomf_00~12.ani | 8/7/7/19/5/5/6/15/9/8/9/10/19 | — | 无 | 无 | 爆炸前景 13 层（_12 有 .als 链挂 12 层） |

全流程时长（走前冲线）：80+88+299+302+571 ≈ **1340ms**。`.als` 边车：仅效果目录 2 个（boomb_02、boomf_12）；角色动画 6 个均无边车（实测 ls）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_bloodsnatch.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_bloodsnatch.skl` | ✅ | 技能数据 |
| 注册行 | load_state 行 33（233/233） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 65/93/305-310/481 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画/攻击信息槽位 |
| 主 nut | bloodsnatch.nut（269 行） | `…\pvf\sqr\character\swordman\bloodsnatch\bloodsnatch.nut` | ✅ | 六子状态机（§2.2，mod 混淆变量名 C3 同族） |
| ap nut | ap_bloodsnatch.nut（84 行） | `…\pvf\sqr\character\swordman\bloodsnatch\ap_bloodsnatch.nut` | ✅ | 抓取持有 appendage（目标位置钉定） |
| 共享 PO | share_po_swordman_24370.nut 注册行 8 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅（引擎内置，L20） | 爆炸伤害体 |
| .chr 条目 | etc motion #135-140（行 1108-1113）；etc attack info #90（行 1384） | `…\pvf\character\swordman\swordman.chr` | ✅ | 6 动画 + BloodSnatch.atk |
| 角色 .ani | bloodsnatch{cast,dash,catch,dashcatch,spin,finish}_body.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | bloodsnatch.atk | `…\pvf\character\swordman\attackinfo\bloodsnatch.atk` | ✅ | physic/推 500/强僵 600ms |
| .als | bloodsnatchboomb_02.ani.als、bloodsnatchboomf_12.ani.als | `…\pvf\character\swordman\effect\animation\bloodsnatch\` | ✅ | 爆炸层链 |
| 特效 .ani | boomb_00-02、boomf_00-12、dash_dust、shot1、shot2（共 20 个） | 同上目录 | ✅ | 冲刺尘土/命中/爆炸 |
| PO 定义 | —（无自有 PO） | `…\pvf\passiveobject\character\swordman\`（grep 无命中） | ⛔ 不存在（用共享 24370） | — |
| 装备层 | *bloodsnatch*.ani ×456 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层（只查存在性） |

bloodsnatch.atk 实测关键值：physic / damage reaction / push aside **500** / lift up 0 / **force hit stun time 600** / hit direction front / knuck back 1 / 血 0 / 音效 R_BLOOD_HIT。

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。跨目录借图是常态（L14）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色动画图集（6 个动作全引） | 必需（共享） | ✅ 已在库 |
| bloodwave.img | sprite_character_swordman_effect_bloodsnatch.NPK | 爆炸底层 boomb_00 | 必需 | ❌ |
| shockwave.img | 同上 | 爆炸次层 boomb_01 | 必需 | ❌ |
| boom1_normal ~ boom8_normal/dodge（8 张） | 同上 | 爆炸前景 boomf_03~12 主素材 | 必需（可减张数做简版） | ❌ |
| dust.img | 同上 | 爆炸尘 boomf_01/02 | 可选 | ❌ |
| shot1.img / shot2.img | 同上 | 抓取命中特效 | 可选 | ❌ |
| crack_dodge.img | sprite_character_knight_effect_dragonburst.NPK | boomb_02 裂地（**跨职业借图**） | 可选 | ❌ |
| rock1.img | 同上 | boomf_10/11 岩石 | 可选 | ❌ |
| 00_dash_dust.img | sprite_character_fighter_effect_chaindestruction.NPK | 冲刺尘土（**跨职业借图**） | 可选 | ❌ |

缺失 img：必需级 10 张（本族 NPK 一次提取全覆盖）、可选级 5 张（跨职业 2 NPK + 本族 3 张）。img 版本红线（v2/v4 可 / v5 不可）由提取时把关。

## 5. 实现方案草案（⛔ 级正式方案免——以下为"定身砸地"深简化近似，供立项评估）

**机制对照**（抓取三子系统拆解引用 `031-GrabBlastBlood.md` §5）：本技能与嗜魂之手共用
"目标控制流（ap_bloodsnatch 每帧钉位+抡举）+ 可抓性三判据（Grabable/Holdable/Fixture）+ 双人演出同步（HoldAndDelayDie）"
三个缺失子系统，且新增**施法者跳跃弧线（正弦半弧 z 位移）**——我们的 MoveCasterForward 只有 x 轴，z 轴跳跃需借用/参照 LSFlightComponent 的 z 物理（当前仅受击方可挂）。

**深简化近似（不做抓取，做"定身砸地"；机制近似度约 55%——伤害节奏保留，抡举演出降级为定身原地）**：

- `BloodSnatchSkill : SkillLogic`（同 031 §5 定身范式 + 本技能六段时序）：
  - `CooldownMs=30000`；`TotalTimeMs=1340`（起手 80 + 前冲 88 + 抓取 299 + 旋转 302 + 终结 571）。
  - OnCast：`ctx.PlayAnim(AnimId.SwordmanBloodSnatchCast)` + `ctx.ClearHitTargets()`；SubState=0。
  - OnUpdate 时序（GetElapsedMs 驱动 + SubState 单向推进）：
    - 80ms：前冲——`ctx.PlayAnim(Dash)`，80~168ms `ctx.MoveCasterForward(66px 换算 0.66 单位增量)`（纯函数插值，同 ReleaseWaveSkill §5.6-2 范式）；
    - 168ms：切 Catch 动画 + `ctx.SetAttackHitbox(前偏 0.6, 半尺寸 (1.4,0.3,0.7))`（F2/F3 盒折算）→ 首个命中 `ctx.AddBuff(target, BuffIds.BloodSnatchHold)` + `ctx.SetSubState(1)` 锁单目标 + `ctx.DisableAttackHitbox()`；
    - 467ms：Spin 动画（**跳跃省略**——施法者留在地面，"跳起"只保留在动画观感）；
    - 769ms：Finish 动画；
    - 769+571×(4/10)≈1000ms（F4 对应时刻）：`ctx.CreateAreaInFront(AreaIds.BloodSnatchBoom, 1.0)` 血气爆炸（对定身目标必中）。
  - OnEnd：`ctx.PlayDefaultAnim()`。
- `BloodSnatchHoldBuff : BuffDefinition`（复用 031 草案同款）：TotalTimeMs≈1000、AddActions={ForbidMoveOn}、RemoveActions={ForbidMoveOff}（FreezeBuff 同构）。
- `BloodSnatchBoomArea : AreaDefinition`（同 ReleaseWaveArea 一次性爆发）：TotalTimeMs=480、EnterActions={MeleeHit, AddBleedBuff}、
  `HitReaction{Damage=200(血气爆炸 demo 值，原值 8726%+38841), HitstunMs=800, KnockbackX=500, LaunchY=0}`（atk 原值 push500）、
  `ViewAnimId=AnimId.BloodSnatchBoom`（boomb_00 层）+ overlay 手组装 boomf 选层。
- 手感差异：目标定在原地不被抡到身后/举顶/砸地——"抓起跳砸"的合体演出完全缺失；伤害节奏（冲→定→爆）与 500 推力保留。读者一眼能看出不是抓取。

**注册点（草案号段，A14 批）**：`SkillIds.BloodSnatch=21`、`BuffIds.BloodSnatchHold=9`、`AreaIds.BloodSnatchBoom=13`、
AnimIds 92-98（Cast/Dash/Catch/DashCatch/Spin/Finish/Boom）、json ×7、img 必需 10 张。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000ms | 30000（直用） |
| 总时长 | ≈1340ms（六段） | 1340 |
| 前冲距离 | 66px（30%×220）/ 抓取段再推 154px | 0.66 单位 / 简化合并为一次 1.5 单位 |
| 抓取盒 | F2/F3 x∈[-79,194] z∈[-2,137] | 前偏 0.6 + 半尺寸 (1.4,0.3,0.7) |
| 抓起伤害 | 5792%+22650（atk：强僵 600ms/push500） | MeleeHit 100/硬直 600/推 500 |
| 爆炸伤害 | 8726%+38841（共享 PO 24370） | 200/硬直 800/推 500 |
| 爆炸点 | 身前 100px | CreateAreaInFront 1.0 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 角色 6 动画 + boomb/boomf/dash_dust/shot 共 26 个 .ani | 节面常规（实测无规则外节） | **现有 ani 子命令全覆盖** |
| bloodsnatchboomb_02.ani.als / boomf_12.ani.als | [use animation]+[none effect add]——已支持（L12） | 无缺口 |
| swordman_bloodsnatch.skl（4 列）+ bloodsnatch.atk | `.skl`/`.atk` 无子命令 | 并入既有缺口；本技能手抄量小（4+14 值） |
| 共享 PO 24370 的行为（写包协议） | 无文件可译（引擎内置） | 游戏侧以 Area 手工重建（§5），非翻译问题 |

计 2 条既有缺口（.skl/.atk），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 抓住敌人抡到身后举顶再砸地（ap_bloodsnatch 每帧钉位） | **抓取/目标控制——缺失档**（031 §5 三子系统第二实证：钉位+方向翻转+释放吸附） | §5 定身近似（ForbidMove Buff 替代） |
| 可抓性三判据 | 单位属性位缺失 | demo 全部可"抓" |
| 施法者正弦半弧跳跃（z 0→120→0） | 施法者 z 轴位移门面缺失（MoveCasterForward 仅 x；LSFlightComponent 仅受击方） | 跳跃省略，动画观感保留 |
| 起手 80ms 内方向输入分流（前冲/直抓两分支） | **技能中方向输入读取——缺失**（R1-A3 记档） | demo 只做前冲分支 |
| 前冲撞墙止损（isMovablePos） | 无地图碰撞（延后） | 无墙环境直用 |
| 爆炸屏震 5/400 + 白闪 178 | 屏震/闪屏（延后） | 跳过 |
| 攻速缩放动画（sq_SetStaticSpeedInfo） | 动画速度门面未暴露（延后） | 固定速度 |
| 挥空分支（没抓到→直接收招） | 可表达（SubState 判定） | 已含在 §5（未命中即不进入爆炸段照播动画） |

## 8. 存疑与缺口上报

**未考证项**
1. 共享 PO 24370 收到 (233, col3, col1) 后的爆炸判定盒/命中参数（引擎内置，pvf 无数据文件；伤害倍率按写包推断）。
2. bloodsnatchboomf_00~12 的播放者（无脚本引用；推断共享 PO 引擎表现层播放）。
3. load_state 行 8 的 share_po_swordman_24370.nut 路径在 `common_object\share_obj\`（白名单外，未读——L20 已定性引擎内置）。
4. 456 张装备层 .ani 未逐一核对（仅 find 计数）。

**系统级缺口（非新增，实证补充）**
- **抓取/投掷（Grab 系）第二完整样本**：ap_bloodsnatch 给出"钉位帧表"（子状态×帧号 → 相对偏移+朝向翻转）与"释放吸附"（SimpleMoveToNearMovablePos）两个新细节，031 §5 立项表可引用本档补充"目标朝向锁定"与"防卡墙释放"两条子需求。
- **施法者 z 轴位移（跳跃弧线）**：新缺口上报——DNF 抓取投掷/跳砸类通用（本技能正弦半弧；R1-A2 跳跃系统记档的姊妹项，建议并入"跳跃系统"立项一并解）。
- 技能中方向输入读取：R1-A3 已记档，本技能第 3 实证（起手分流）。

**给下轮的经验**：狂战 60 级二觉系技能（swordman_ 前缀 skl）全部有完整 nut 状态机（非引擎内置），状态号=技能号；抓取类直接看 ap_*.nut 的钉位帧表即可还原演出结构，不必猜。
