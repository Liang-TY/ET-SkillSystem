# 致命血陨（fatalblood）

> 技能ID 245 | 级别 A | 可实现性 🔶（分段位移距离分支/命中分支/分段反应切换三降级；帧驱动判定主干可完整直译——**本批四技中结构最贴合现有框架**） | 分析日期 2026-08-22 | 批次 A17

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 致命血陨 | `skill\Swordman\swordman_fatalblood.skl [name]` |
| 英文名 | fatalblood（取 skl 文件名去 swordman_ 前缀） | 同上 |
| 职业 | 狂战士（狱血弑天二觉段 Lv80 技；[second growtype maximum level] 槽 7=30 → growtype 3=狂战二觉档，与 233/87/48 三方公式互证；血气剑系=狂战常识） | 同上 + 233 交叉 |
| 学习等级 | 80 | 同上 [required level] |
| 最高等级 | 40（二觉段上限 30） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↓→→ + Z；指令���法 MP 优惠 20%/40% 档 | 同上 [command] / [command key explain] / [skill command advantage] |
| CD | 50000 ms（固定） | 同上 [cool time] |
| MP | 848 → 6360 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块（道具 3037）×5 | 同上 [consume item] |
| 可施放状态 | 8 / 0 / 14 | 同上 [executable states] |
| 一句话效果 | 吸血气凝成巨剑：上斩 → 前冲横斩 → 命中/有目标时交叉斩击两连（第二击击倒），挥空则播收空动画；每段间有短前冲（按住前方向键加长）；习得[血气旺盛]后全段附带出血 | 同上 [explain] + nut 走读 |

**level property（10 列，模板↔向量逐占位对位）**：上斩攻击力 = **col7% + col0**（Lv1：4060% + 13257）、
横斩 = **col8% + col1**（5079% + 16570）、交叉斩击 = **col9% + col2**（11171% + 36463，两连击各一次）、
出血概率 col3 **100%**、出血Lv col4 **78**、出血时长 col5 **2300ms**（×0.001）、出血伤害 col6 **1883**（向量源 **-4**，L21 未解族）。
⚠ 列布局是"固定值前（col0-2）/ 异常参中（col3-6）/ 百分比后（col7-9）"的**倒置表**——读表时按向量索引对位，勿按列序直觉读。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 36（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/fatalblood/fatalblood.nut", "swordman_fatalblood", 245, 245);
```

- `swordman_header.nut`：`STATE/SKILL_SWORDMAN_FATALBLOOD <- 245`（行 77/105）、`CUSTOM_ANI_SWORDMAN_FATALBLOOD{1HIT,2HIT,3HIT,3HITFAIL}_BODY <- 164/165/166/167`（行 334-337）、`CUSTOM_ATTACK_SWORDMAN_FATALBLOOD{1,2,3,4}HIT <- 96/97/98/99`（行 487-490）。
- .chr 对位（0 基实测吻合）：etc motion #164-167 = 1137-1140 行 `FatalBlood1Hit/2Hit/3Hit/3HitFail_Body.ani`；etc attack info #96-99 = 1390-1393 行 `AttackInfo/FatalBlood1Hit/2Hit/3Hit/4Hit.atk`（行号-1294 ✓）。
- **本技能不用共享 PO 24370**——全部判定在角色自身帧驱动攻击盒上（四段 .ani 自带 ATTACK BOX）。
- 主 nut 273 行**未混淆**（变量名可读，本批唯一——F7 族也有原版幸存件）。

### 2.2 主 nut 逐回调（fatalblood.nut，四子状态连段）

**checkExecutableSkill / checkCommandEnable**：标准入口（同 236/243）。

**onSetState（每段统一流程）**：

| 子状态 | 动画 | 攻击信息 | 伤害（BonusRate + Power） | 前景/背影特效层（moveWithParent 跟随角色） |
|---|---|---|---|---|
| 0 上斩 | #164（11 帧 551ms） | atk 96 | col7 4060% + col0 13257 | 1hitback_01 / 1hitfront_04 |
| 1 横斩 | #165（7 帧 401ms） | atk 97 | col8 5079% + col1 16570 | 2hitback_02 / 2hitfront_02 |
| 2 交叉斩（命中线） | #166（17 帧 1322ms） | atk 98 → F11 切 **atk 99** | col9 11171% + col2 36463 ×2 | 3hitback_00 / 3hitfront_07 |
| 3 交叉斩（挥空线） | #167（15 帧 1178ms） | atk 98 | 同上（单段） | 3hitfailback_00 / 3hitfailfront_04 |

- 子状态 0 起手：**隐藏武器图层**（setShowEquipmentLayer(WEAPON, false)——血气巨剑替代武器，onEndState 恢复）；bool0=false（命中标记）。
- 子状态 1/2/3 起手：存向量 (起点x, 本段前冲距离 datas[1])。
- **血气旺盛联动**（技能 63 已学时，四个 atk 全部写入）：`sq_SetChangeStatusIntoAttackInfo(BLEEDING, col3 100%, col4 Lv78, col5 2300ms, col6 1883)`——L6 标准链路。
- 特效层播放带攻速补偿率（原时长/静态化时长×100——CreateAniRate）。

**onKeyFrameFlag（仅子状态 2 交叉斩）**：
- F3 flag 1：震 10/350 + 黑闪(60,650,0,191,CLOSEBACK) + **全屏前景特效 directionforeground.ani**（sq_setFullScreenEffect）。
- F11 flag 2：**切攻击信息 atk 99**（down/push150/lift350/stuck -1000）+ 同参数重算 + **resetHitObjectList()**（交叉斩第二击可重复命中）+ 黑闪(0,0,300,191)。

**onProc（分段前冲，纯函数插值）**：
- 子状态 1：帧 ≥1 起，窗口 = delaySum(1,5)=274ms，从起点匀速推进 datas[1] 距离；isMovablePos 撞墙止损。
- 子状态 2/3：窗口 = delaySum(0,3)=386ms，同样推进。

**onAttack（命中标记）**：子状态 0/1 命中敌人 → bool0=true。

**onEndCurrentAni（段间推进 + 分支）**：
- 0 → 1：前冲距离 = 按住前方向键 ? **68px : 47px**（explain 的"按向前方向键增加前进距离"）。
- 1 → 分支：`sq_FindTarget(-140, 350, 60, 350)` 找到敌人 **或** bool0（前段命中过）→ 子状态 2（前冲 54/56px）；否则 → 子状态 3 挥空线（前冲 48/93px）。
- 2/3 → STAND。

**onEndState**：脱离状态 245 → 解绑特效层父节点 + **恢复武器图层显示**。

### 2.3 被动对象 / appendage

**无 PO、无 ap**（角色自身帧驱动连段——与 243 的"全 PO 化"相反的另一个极端）。
effect 目录 45 个特效 ani 全部由 nut 直播（CreateAniRate + moveWithParent 挂角色）：8 个直播层是**空 IMAGE 壳动画**（L7），各自 .als 链挂 00-07 号子层（真正带 img 的层）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒（min/max ÷100=单位） | 备注 |
|---|---|---|---|---|---|
| `fatalblood1hit_body.ani`（#164） | 11 | 551ms | 无 | **F8-F10**：`-128,-48,-12,437,96,228`（上斩，前伸 4.37 单位宽盒） | 仅引 sm_body |
| `fatalblood2hit_body.ani`（#165） | 7 | 401ms | F1=0（空处理器） | **F3-F5**：`-117,-48,-12,411,96,228` 等 | 前冲窗 F1-F5 |
| `fatalblood3hit_body.ani`（#166） | 17 | 1322ms | **F3=1**（震/闪/全屏前景）、**F11=2**（切 atk99+重置命中表） | **F3-F4**：`-125,-48,0,395,96,309`；**F11**：`-125,-48,0,411,96,309` | 交叉斩两拍；前冲窗 F0-F3 |
| `fatalblood3hitfail_body.ani`（#167） | 15 | 1178ms | 无 | **F3**：同 3hit 盒 | 挥空线 |
| effect/animation/fatalblood/ 45 个 | 1-29 | — | 无 | 无 | 8 直播壳层 + .als 链 36 子层 + directionforeground 全屏层 |

`.als` 边车：仅 effect 目录 9 个（壳层链）；4 个角色 .ani 无边车（特效由 nut 直播）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_fatalblood.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_fatalblood.skl` | ✅（258 行） | 10 列等级数据 |
| 注册行 | load_state 行 36（245/245） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 245 |
| 常量 | swordman_header.nut 行 77/105/334-337/487-490 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 状态/动画/攻击槽位 |
| 主 nut | fatalblood.nut（273 行，**未混淆**） | `…\pvf\sqr\character\swordman\fatalblood\fatalblood.nut` | ✅ | 四段连段状态机 |
| PO/ap | —（不存在） | `…\pvf\passiveobject\character\swordman\`（grep 无命中） | ⛔ 不需要 | 判定全在角色帧盒 |
| .chr 条目 | etc motion #164-167（1137-1140 行）+ etc attack info #96-99（1390-1393 行） | `…\pvf\character\swordman\swordman.chr` | ✅ | 四动画 + 四 atk |
| 角色 .ani | fatalblood{1hit,2hit,3hit,3hitfail}_body.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4（帧驱动盒） |
| 角色 .atk | FatalBlood1Hit/2Hit/3Hit/4Hit.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | §2.2 表 |
| 特效 .ani | fatalblood/ 45 个 + .als ×9 | `…\pvf\character\swordman\effect\animation\fatalblood\` | ✅ | 壳层链结构 |
| 关联被动 | BloodyVigorous.skl（技能 63 血气旺盛，064 已记档） | `…\pvf\skill\Swordman\BloodyVigorous.skl` | ✅（引用层面） | 出血开关 |
| 装备层 | *fatalblood* ×304 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层（存在性） |

atk 关键值（实测）：
- 1hit：physic/damage/push **250**/lift **150**/hit down/cut/blood 70/**force hit stun 1000**/front；
- 2hit：同 1hit 但 hit horizon；
- 3hit：physic/damage/push **50**/lift **50**/**ignore weight 1**/horizon/force stun 1000；
- 4hit（交叉第二击）：physic/**down**/push **150**/lift **350**/horizon/knuck back -1/**stuck -1000**。

## 4. 资源需求

img 推导 NPK：`sprite_character_swordman_effect_fatalblood.NPK`（全部同库，一次提取全覆盖）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | （已入库） | 四段角色动画 | 必需（共享） | ✅ |
| `…/FatalBlood/Sword.img` | sprite_character_swordman_effect_fatalblood.NPK | **血气巨剑本体**（48 处引用，1hit 成剑/各段剑体） | **必需** | ❌ |
| `…/MakeSword.img` | 同上 | 成剑特效 | **必需** | ❌ |
| `…/SwordDisappearA.img`、`SwordDisappearB.img` | 同上 | 剑消散 | **必需** | ❌ |
| `…/Blood3HitA/B/C.img` | 同上 | 交叉斩血光 ×3 | **必需** | ❌ |
| `…/SwordTrail3HitA/B/C/D.img`、`SwordTrail1Hit.img`、`SwordTrail2Hit.img` | 同上 | 各段剑光拖尾 ×7 | 可选（视觉层，至少 3Hit 一组建议保留） | ❌ |
| `…/DustFront1Hit/2Hit/3Hit.img`、`DustBehind1Hit/2Hit/3Hit.img` | 同上 | 前后尘土 ×6 | 可选 | ❌ |
| `…/Blood1Hit.img`、`Blood2Hit.img` | 同上 | 前两段血光 | 可选 | ❌ |
| `…/ETC.img`、`GroundBlood.img` | 同上 | 杂项/地血 | 可选 | ❌ |

缺失 img：**必需 7 张、可选 16 张——全部同一 NPK**。img 版本红线（v2/v4 可 / v5 不可）由提取时把关。

## 5. 实现方案草案

**结构映射**：四段连段 = 单 SkillLogic 的 SubState 时间编排（L19 连段段间档：每段 `ctx.ClearHitTargets()` 段间调用即"段间重命中"）；
判定 = **帧驱动攻击盒直译**（四 ani 的 ATTACK BOX 翻译进 json + 判定帧表注册——ReleaseWave F1/F2 同通路）；
出血 = HitReaction.ProcBuffId（血气旺盛固定为已学）。
**降级三处**：分段 HitReaction 切换（atk96→97→98→99）当前框架单 HitReaction 表达不了 → 取折中单值（缺口上报 §8）；
按住前方向键加长前冲 → 固定中档；命中/挥空分支 → 无"我打中了谁"查询门面 → 固定走命中线（挥空线动画可留作未命中表现，或后补门面）。

### 内容件清单

1. **`DotNet~/Skills/FatalBloodSkill.cs : SkillLogic`**（SubState 段间编排 + ReleaseWave 纯函数位移范式）
   - `CooldownMs = 50000`；`TotalTimeMs = 2280`（551 + 401 + 1322 命中线；挥空线 1178ms 更短，以长线为准）。
   - `HitReaction { Damage = 130, HitstunMs = 1000, KnockbackX = 250, LaunchY = 150, ProcBuffId = BuffIds.Bleed, ProcChance = 100 }`
     （四 atk 折中：atk96/97 主值 push250/lift150 + 全段共有的 force stun 1000；出血 100% = 血气旺盛满档直给）。
   - `HitActions = { MeleeHit }`。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanFatalBlood1Hit)` + `ctx.ClearHitTargets()`；SubState=0。
   - `OnUpdate`（时间驱动，段间前冲 + 换段）：
     - t=0~551 段0；t ≥ 551：`ctx.PlayAnim(AnimId.SwordmanFatalBlood2Hit)` + `ctx.ClearHitTargets()`（段间重命中，L19 档）；SubState=1。
     - 551+38~551+312（段1 F1-F5 窗 274ms）：`ctx.MoveCasterForward(47px 换算 0.47 单位 × dt/274)`。
     - t ≥ 952：`ctx.PlayAnim(AnimId.SwordmanFatalBlood3Hit)` + `ctx.ClearHitTargets()`；SubState=2。
     - 952~1338（段2 F0-F3 窗 386ms）：前冲 0.54 单位。
     - t ≥ 1338（段2 F11 对应时刻 ≈ 段内 800ms）：**再 ClearHitTargets 一次**（交叉斩第二击重命中——DNF F11 resetHitObjectList 直译）。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **无新增 Area/Buff/Action**（帧驱动盒 + Bleed 复用——本批四技中唯一零新增内容件技能）。
3. **视觉**：血气巨剑前后层按 releasewave 手组装 overlay 先例（8 直播壳层的 .als 链合并为 2~3 层 overlay 挂到各段动画），或简化只保留 front 主层。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 四子状态连段（onEndCurrentAni 推进） | SubState 时间编排（无输入分支时结构等价） |
| 帧 8-10/3-5/3-4/11 ATTACK BOX | 帧驱动攻击盒直译（json attackBoxes + 判定帧表注册——ReleaseWave 同通路） |
| 段间新攻击信息 + 伤害重算 | 单 HitReaction 折中（差异见 §7；分段切换门面缺口上报） |
| F11 resetHitObjectList（交叉斩第二击重命中） | `ctx.ClearHitTargets()` 段内二次调用（L19 段间/同段定时档） |
| 段间前冲（47/68/54/56/48/93px，方向键分支） | MoveCasterForward 纯函数增量；**方向分支固定中档**（技能中方向输入缺失 R1-A3） |
| 命中/挥空分支（FindTarget or bool0） | 固定命中线（无命中反馈门面）；挥空线动画留作后续增强 |
| 血气旺盛出血（L6 链路） | HitReaction.ProcBuffId=Bleed + ProcChance=100 |
| 武器图层隐藏（血气剑替武器） | demo 单层 sm_body 无武器层——天然无需处理（多图层系统后补时映射 RenderConfig 层开关） |
| 特效层攻速补偿率（CreateAniRate） | 无动画速度门面（延后）——固定 100% |
| 黑闪/震/全屏前景 | 延后 → 跳过 |
| 撞墙止损 | 无地图碰撞（延后）→ 无墙直用 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.FatalBlood = 25` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanFatalBlood1Hit = 121`、`2Hit = 122`、`3Hit = 123`、`3HitFail = 124`（可选）、剑光层 125+（可选） |
| 判定帧表 | `LSHitboxComponentSystem` 采样集 | 四个 FatalBlood 动画 id 加入（帧盒直译的注册点） |
| json 注册 | `LSAnimClipRegistrar.cs` | 角色 4 个 + 特效壳层链（可选 2-3 个） |
| 图集 | `LSAnimResComponentSystem.cs` | 必需 7 张（同一 NPK） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 50000 ms | 50000（直用） |
| 总时长 | 2274ms（命中线 551+401+1322）/ 2130ms（挥空线） | 2280 |
| 上斩 | col7 4060% + col0 13257；atk96 push250/lift150/stun1000 | 并入折中 HitReaction（Damage 130/Hitstun 1000/Kb 250/Ly 150） |
| 横斩 | col8 5079% + col1 16570；atk97 同参数 | 同上 |
| 交叉斩第一拍 | col9 11171% + col2 36463；atk98 push50/lift50 | 同上（差异降级） |
| 交叉斩第二拍 | 同倍率；atk99 down/push150/lift350/stuck-1000 | 同上（击倒手感缺失，§7） |
| 出血 | 100%/Lv78/2.3s/1883 | BleedBuff 预设（ProcBuffId 直挂） |
| 段间前冲 | 47/68px（段0→1）、54/56 或 48/93px（段1→2/3） | 0.47 / 0.54 单位固定 |
| 攻击盒 | 上斩 F8 x[-128,437]；交叉 F11 x[-125,411] | 帧盒直译（json attackBoxes） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| swordman_fatalblood.skl | `.skl` 无子命令 | 手抄 10 列（含倒置表陷阱，见 §1 注） |
| 4 份 .atk | `.atk` 无子命令 | 手抄；`[force hit stun time]`/`[ignore weight]`/`[stuck]`/`[knuck back]` 字段并入 atk 立项输入（R2-A8/R3-A14 已录同族） |
| 4 个角色 .ani | 节面常规（FRAME/DELAY/ATTACK BOX） | **现有 ani 子命令全覆盖**（帧盒是核心数据，翻译直接可用） |
| effect 壳层 .als 链 | [none effect add]（已支持 L12） | 无新节缺口 |
| directionforeground.ani（全屏前景层） | 无特殊节；但属"全屏特效"消费形态 | 游戏侧无全屏层通道——视觉降级为普通 overlay 或跳过（非工具缺口） |

结论：缺口 = `.skl`/`.atk` 族共性 2 条，无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 四段各持独立 .atk（反应参数逐段不同；交叉第二击 down/lift350） | **SkillLogic 单 HitReaction 限制**（新缺口：帧驱动命中经 `logic?.HitReaction` 单值注入——LSHitboxComponentSystem.cs:124 实测） | 折中单值（段 0/1 主值）；若要精确还原则走"分段 Area 各带 HitReaction"（064 范式）但需弃帧盒直译，二选一 |
| 按住前方向键前冲 47→68px 等 | 技能中方向输入读取缺失（R1-A3） | 固定中档 |
| 命中/挥空分支（FindTarget/bool0） | 无命中反馈查询门面（技能不知"刚才打没打中"） | 固定命中线；挥空线 3HitFail 动画入库备用 |
| stuck -1000 钉住（交叉第二击） | 无 stuck 机制 | HitstunMs 1000 近似 |
| ignore weight（无视重量打击） | 无重量系统 | 忽略 |
| 武器图层隐藏/恢复 | demo 无武器图层 | 天然无需处理；多图层后补 |
| 特效攻速补偿（CreateAniRate） | 动画速度门面未暴露（延后） | 固定 100% |
| 黑闪/震 10/全屏前景 | 延后 | 跳过 |
| 撞墙止损 | 无地图碰撞（延后） | 无墙直用 |

## 8. 存疑与缺口上报

**未考证项**
1. `-4` level 向量源（本技能 = 出血伤害列 1883；与 234 出血攻击力、245 感电Lv 的 -6 并存——L21 族已三样本，仍无定论）。
2. 2hit F1 flag 0 空处理器（残留标记）。
3. 段间前冲距离数据 47/68/54/56/48/93 的来源（nut 硬编码，非 skl 列——平衡参数内嵌代码）。
4. level info 70 行 vs 上限 40（超出段未用，同 236/243 现象）。

**新系统级缺口（主循环汇总）**
1. **技能内分段 HitReaction/HitActions 切换门面**：帧驱动命中在 `LSHitboxComponentSystem` 以 `logic?.HitReaction` + `logic?.HitActions` 单值注入（代码实测 :110/:124）——多段技（本技能四段四 atk、064 两刀两 atk 同族）要么折中单值要么弃帧盒走 Area。建议：LSCast.SubState 已存在，给 SkillLogic 加"按 SubState 取 HitReaction"的查表层（或 HitActions 数组按段索引）成本低收益大，连段类技能普遍受益。
2. **命中反馈查询门面**（"刚才这一段打没打中人"）：命中/挥空分支类技能通用（本技能 FindTarget/bool0 双判据）——与 R3-A11"目标枚举/计数门���"同族，可合并立项。

**给下轮的经验**：狂战 Lv80 二觉技 fatalblood 是"纯角色帧驱动连段"样本（无 PO 无 appendage，nut 273 行未混淆——F7 族里也有干净文件，先 cat 再定还原策略别一律按混淆处理）；其 effect 层是"nut 直播空壳 + .als 链挂子层"两段式结构，img 全在子层里——查 img 需求要跟着 .als 链走到叶子。
