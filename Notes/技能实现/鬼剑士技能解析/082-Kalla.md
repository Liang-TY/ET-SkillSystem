# 冥炎之卡洛（Kalla）

> 技能ID 82 | 级别 B | 可实现性 🔶（深简化"召唤弹幕+冥炎 debuff+三连击终结"可表达主干；**普攻形态改造（附身期普攻替换/攻击时自动发射分身）依赖普攻行为替换门面——新缺口**，附身 30s 沦为纯计时器） | 分析日期 2026-08-22 | 批次 B5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冥炎之卡洛 | `skill\Swordman\Kalla.skl` [name] |
| 英文名 | Kalla（取 skl 文件名；[name2] 实测 `Kalla`） | 同上 [name2] |
| 职业 | 鬼泣（[skill fitness growtype]=2，L17；卡洛=第四鬼神常识） | 同上 |
| 学习等级 | 45（一觉主动，skill class 3） | 同上 [required level] / [skill class] |
| 最高等级 | 70（一觉档 50） | 同上 [maximum level] / [growtype maximum level] 第 3 位=50 |
| 类型 | 主动（active） | 同上 [type] |
| 指令 | ↑↓↓ + Z | 同上 [command] |
| CD | 60000 ms（pvp 40000/起手 30000） | 同上 [cool time] |
| 施法时间 | 500 ms（读条） | 同上 [casting time] |
| MP | 420 → 3528（Lv1→70） | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×2 | 同上 [consume item] |
| 前置 | 技能 75（瘟疫之罗刹）Lv1 | 同上 [pre required skill] |
| 武器效果 | magical（[weapon effect type] 魔法系） | 同上 |
| static data | `500 100 -300 200 -300`（5 槽，语义未考证，疑分身发射参数） | 同上 [dungeon][static data] |
| 一句话效果 | 召唤卡洛附身 30 秒并向敌人发射分身；命中者受暗属性伤害并被冥炎附身（单敌最多 3 层、每 1s 受无视防御魔法伤害 ×5 次）；再按指令键进入二刀流模式打出右/左/上三连斩；鬼影步状态下普攻/前冲攻击增伤 | 同上 [explain] + level property 模板 |

**level property（13 列，模板 9 行 9 向量，L21 读法）**：持续时间 = col0×0.001 = **30 秒**（恒 30000）；
分身攻击力 = col1（312→4484 魔攻）；右斩击 = col2（624→8968% 武器魔攻）；左斩击 = col3（936→13452%）；
上斩击 = col4（1872→26904%）；冥炎攻击次数 = col5 = **5 次**（恒）；冥炎攻击间隔 = col6×0.001 = **1 秒**（恒 1000）；
冥炎攻击力 = col7（31→224 魔攻）；鬼影步态普攻/前冲攻击力增加量 = col8（90→869）。9 列语义全部由模板行文本直读。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 83（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/Kalla/Kalla.nut", "swordman_kalla", 44, -1);
// swordman_header.nut 行 238-243（实测）：CUSTOM_ANI_KALLALAND..CUSTOM_ANI_KALLAFINISH3 <- 68..73
// passiveobject.lst 行 11223-11227（实测）：20045=KallaShadowLand.obj，20046=KallaShadowAir.obj，20047=KallaFire.obj
```

⚠ 状态 44 **不绑定技能 ID**（第 5 参 -1）——它是"卡洛攻击形态"通用状态，被本技能与其他入口共用；
召唤/附身/分身发射的主流程**引擎内置**（觉醒 skill class 3 一代形态，087 同代），nut 只承担两件事：
鬼影步态普攻链替换 + 附身期技能打断表（见 2.2）。

### 2.2 主 nut 逐回调（kalla.nut，412 行，**变量名混淆的 mod 代码**——C3/C6 形态①，语义仍可读）

- **onAfterSetState（子状态 (0, N)，N=0/1/2）**：按 N 播攻击动画——N=0 → `sq_GetAttackAni(1)`（右斩）、
  N=1 → `sq_GetAttackAni(0)`（左斩）、N=2 → `sq_GetCustomAni(obj, 57)`（= etc motion #57 = **Frenzy1.ani**，
  .chr 槽位实测——上斩借用血之狂暴系二刀流普攻动画）；设攻速。
- **onProcCon（每帧，两段）**：
  ① 若持有 `ap_kalla.nut` **且** `ap_ghoststep.nut` 两标记（卡洛附身 + 鬼影步）且处于三连击 N=0/1 段：
  当前动画帧 ≥ 总帧-8/-7 时开启攻击键输入，再按攻击键 → 切状态 44 子状态 (0, N+1)——**段间推进 0→1→2**。
  ② **附身期技能打断表**（约 300 行）：附身中可按指令直接施放 20+ 个技能（237-241/247 二觉系、87 怖拉修、
  25/41/36/75/96 四鬼神+冰晶萨亚、60 鬼影闪、111 鬼影鞭、5/8/20/22/33/42/55/65/169/249 等）——
  附身不锁操作，鬼泣全技能皆可取消进入。
- **普攻链替换入口在 `swordman_common.nut:248`（实测，procAppend 中断钩子）**：按普攻键时若同时持有
  ap_kalla + ap_ghoststep 两标记 → 拦截普攻、改切状态 44 子状态 (0,0)——**这就是"卡洛附身改写普攻"的实现点**。
- **引擎内置部分（推断，参照 087 定性法）**：施法瞬间播 KallaLand.ani（地面 440ms）/KallaAir.ani（空中 280ms）、
  挂 ap_kalla 标记（空壳 7 行，纯标记）、附身期 kallaappear/stay/disappear 特效层（effect 目录）、
  按攻击节奏发射分身 PO 20045/20046、命中注冥炎（PO 20047）、再按指令键 → KallaFinishReady→1→2→3 三连斩
  （对应 .chr etc attack #KallaFinish1/2/3.atk）——**创建时点/发射条件引擎侧不可见**。

### 2.3 被动对象 / appendage

| PO（passiveobject.lst 实测） | .obj 结构 | .atk 关键值 |
|---|---|---|
| **20045 KallaShadowLand**（地面分身） | normal 层 / pass all / **piercing 0（不穿透单目标）** / [basic motion] ShadowLand.ani（10 帧 800ms，**全部 10 帧攻击盒**）+ [add object effect] ShadowLandDodge.ani / [attack info] KallaShadowLand.atk | magic / **暗属性** / damage 反应 / push 50 / **lift 200** / hit horizon / blow / no blood 50 0.7 |
| **20046 KallaShadowAir**（空中分身） | 同构，ShadowAir.ani（10 帧 800ms 全帧攻击盒） | magic / 暗 / damage / push 50 / lift 0 / hit down / blow / no blood 50 0.7 |
| **20047 KallaFire**（冥炎） | normal 层 / pass all / **piercing 1000（穿透全场）** / [basic motion] FireDodge.ani（10 帧 600ms 无攻击盒）+ [etc motion] FireNormal/FireDodge/FireNormal 循环（火焰持续视觉） | magic / 暗 / **none（无受击反应）** / hit down / no blood 10 0.5 |

- **ap_kalla.nut（7 行）与 ap_ghoststep.nut（4 行）均为空壳标记 appendage**——不携带任何数值逻辑，
  只作 `sq_IsAppendAppendage` 查询标记（241 的 ap_zig_character 同模式：**Buff 当跨系统布尔标记用**）。
- 冥炎"3 层/1s 间隔/5 次/无视防御"的数值消费全在引擎侧（KallaFire PO 挂附 + DoT 结算），pvf 无对应脚本。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\kallaland.ani（槽 68） | 10 | 440ms | 无 | 无 | 地面施法（卡洛现身） |
| character\…\kallaair.ani（槽 69） | 5 | 280ms | 无 | 无 | 空中施法 |
| character\…\kallafinishready.ani（槽 70） | 6 | 660ms | 无 | 无 | 二刀流起势 |
| character\…\kallafinish1.ani（槽 71） | 5 | 300ms | 无 | **F1-F4**：min/max ≈ x[-81,201] z[8,127] | 右斩（前伸 ~2 单位） |
| character\…\kallafinish2.ani（槽 72） | 5 | 300ms | 无 | **F2-F4**：x[-54,187] z[9,113] | 左斩 |
| character\…\kallafinish3.ani（槽 73） | 5 | 540ms | **F3=65534（@240ms）** | **F2-F3**：x[-42,200] z[-35,230] | 上斩（高盒到 2.3 单位；65534=取消/命中标记惯例，语义未考证） |
| PO shadowland / shadowair.ani | 10/10 | 800/800ms | 无 | **全部 10 帧**（分身全程判定） | 分身飞行体 |
| PO shadowlanddodge / shadowairdodge.ani | 5/5 | 400ms | 无 | 无 | 分身辉光叠加层 |
| PO firedodge / firenormal.ani | 10/10 | 600ms | 无 | 无 | 冥炎火焰视觉 |
| effect\…\kalla\（38 个 .ani） | — | — | — | — | appear/stay/disappear（附身幻影）、land/air（施法）、finish1-3 ×6 层（三连斩特效）、exp ×6（爆炎） |

`.als` 边车：**无**（角色与 PO 两侧实测）；角色动画仅引 sm_body（L16 ✓）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Kalla.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Kalla.skl` | ✅（281 行） | 13 列等级数据 |
| 注册行 | load_state 行 83（状态 44/技能 -1） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 238-243 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 动画 68-73 |
| 主 nut | kalla.nut | `…\pvf\sqr\character\swordman\kalla\kalla.nut` | ✅（412 行，混淆） | 三连击链 + 打断表 |
| 普攻拦截 | swordman_common.nut 行 248 | `…\pvf\sqr\character\swordman\swordman_common.nut` | ✅ | 附身+鬼影步 → 普攻改状态 44 |
| ap 标记 ×2 | ap_kalla.nut / ap_ghoststep.nut | `…\pvf\sqr\character\swordman\appendage\` | ✅（空壳 7/4 行） | 跨系统布尔标记 |
| .chr 条目 | etc motion #68-73（行 1041-1046）+ etc attack #KallaFinish1-3（行 1352-1354） | `…\pvf\character\swordman\swordman.chr` | ✅ | 动画/命中注册 |
| 角色 .ani | kallaland / kallaair / kallafinishready / kallafinish1-3（+[pvp] ×4） | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | kallafinish1/2/3.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | 三连斩：damage/push30/lift30 ×2 + down/push40/**lift300 击飞** |
| .als | —（无） | 两侧 animation 目录 | ⛔ 无边车 | — |
| PO lst | 20045/20046/20047（行 11223-11227） | `…\pvf\passiveobject\passiveobject.lst` | ✅ | ID→obj |
| PO 定义 | kallashadowland / kallashadowair / kallafire.obj | `…\pvf\passiveobject\character\swordman\` | ✅ | §2.3 |
| PO .ani | kalla\ 6 个 | `…\pvf\passiveobject\character\swordman\animation\kalla\` | ✅ | §2.4 |
| PO .atk | kallashadowland / kallashadowair / kallafire.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | §2.3 |
| 施法/附身特效 | kalla\ 38 个（+kalla_ds\） | `…\pvf\character\swordman\effect\animation\kalla\` | ✅ | 幻影/爆炎/三连斩特效 |
| 装备层 | *kalla*.ani ×456 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色全部动作 | 必需（共享） | ✅ 已在库 |
| karl_body_d / karl_body_n.img | sprite_character_swordman_effect_kalla.NPK | 卡洛幻影本体（appear/stay/disappear） | **必需** | ❌ |
| shot-dodge / shot-normal.img | 同上 | 分身弹体（shadowland/air） | **必需** | ❌ |
| fire-dodge / fire-normal.img | 同上 | 冥炎火焰 | **必需** | ❌ |
| finish_front_dodge / _normal、finish_back_dodge / _normal.img | 同上 | 三连斩前后特效层 | **必需** | ❌ |
| !sword_dark_under / !sword_upper.img | 同上 | 冥炎剑（二刀流武器形态） | 可选 | ❌ |
| karl_weapon_d/n、karl_weapon_light_d/n、karl_jumpweapon_d/n.img | 同上 | 卡洛武器/跳跃武器 | 可选 | ❌ |
| explo-dodge / explo-normal、scrach-dodge.img | 同上 | 爆炎/爪痕 | 可选 | ❌ |
| dark_explosion_1 / dark_explosion_2.img | sprite_creature_common.NPK（**跨目录借图**，L14） | exp 系爆炎 | 可选 | ❌ |

缺失 img：必需级 9 张 + 可选级 12 张（主 NPK 一次提取全覆盖；跨 NPK 2 张）。img 版本红线由提取时把关。

## 5. 实现方案草案（深简化"独立施放版"：召唤齐射 + 冥炎 debuff + 可选三连斩；附身形态改造整体降级为计时 Buff，见 §7）

### 内容件清单

1. **`DotNet~/Skills/KallaSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发范式 + WaveSwordSkill 弹体范式）
   - `CooldownMs=60000`；`TotalTimeMs=440`（KallaLand 时长）+ 齐射段，取 1200。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanKallaLand)`；`ctx.AddBuffToSelf(BuffIds.KallaPossession)`（30s 附身标记——
     **本 demo 中仅作占位计时器**，无普攻改造消费方）；`ctx.SetSubState(0)`。
   - OnUpdate：`≥400 && SubState==0`：**齐射分身**——`ctx.CreateBullet(BulletIds.KallaShadowLand)`（地面弹）；
     若施法时在空中（我们无空中态，demo 恒地面版）；`SetSubState(1)`。
   - **三连斩入口（demo 变通）**：DNF"再按指令键"撞二段交互门面（R4-A16）——demo 用**独立按键**直连
     `KallaFinishSkill`（见下），或在 KallaSkill 内 `PeekBufferedButton` 窗口判定（仅能消费一次，R4-A16 已知限）。
2. **`DotNet~/Bullets/KallaShadowBullet.cs : BulletDefinition`**（复制 NormalWaveBullet 范式）
   - `TotalTimeMs=800`（shadowland.ani 时长直用）、穿透（pass all）、`HalfExtents=(1.2,0.5,0.8)`（分身判定）、
     `HitActions={MeleeHit, AddDarkFireBuff}`、`HitReaction{Damage=120, HitstunMs=500, KnockbackX=50, LaunchY=200}`（KallaShadowLand.atk 原值：浮空 200）；
     `ViewAnimId=AnimId.KallaShadowLand`。
3. **`DotNet~/Actions/AddDarkFireBuffAction.cs : LSAction`**（同 AddBleedBuffAction 十行范式）
4. **`DotNet~/Buffs/DarkFireBuff.cs : BuffDefinition`**（冥炎 DoT，同 BleedBuff 范式）
   - `TotalTimeMs=5000`（col5 5 次 × col6 1s）、`TickTimeMs=1000`、`TickActions={DarkFireTick}`、
     每跳 31~224（col7，demo 取 40）——"无视防御"在我们无防御系统下天然成立。
     ⚠ DNF 单敌 3 层独立结算：我们 Stack 是"同 buff 叠层刷新时长"简版（LSBuff.Stack 实测），**3 层并行结算不可表达**（§8 上报）。
5. **`DotNet~/Buffs/KallaPossessionBuff.cs : BuffDefinition`**（附身 30s 标记）
   - `TotalTimeMs=30000`、无 Tick（纯标记）——为将来"普攻改造门面"预留挂载点；视觉（卡洛幻影）撞 Buff 视觉挂接缺口（R1-A5）。
6. **`DotNet~/Skills/KallaFinishSkill.cs : SkillLogic`**（三连斩，同 NormalAttack 连段子状态机范式，L19 第一档）
   - `TotalTimeMs=660+300+300+540=1800`；OnCast 播 FinishReady；
   - OnUpdate 帧驱动三段：`≥660` 段1 右斩（`SetAttackHitbox` F1-F4 盒折算 + HitReaction A：col2 + kallafinish1.atk push30/lift30）→ `ClearHitTargets` →
     `≥960` 段2 左斩（同参数，col3）→ `ClearHitTargets` → `≥1260` 段3 上斩（HitReaction B：col4 + kallafinish3.atk **down/push40/lift300 击飞**）；
   - 每段攻击盒由帧号 const + SubState 守卫（bloodboom §4.7-7 同构）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 44（卡洛攻击形态，引擎） | KallaSkill + KallaFinishSkill 两技能拆分（引擎形态不可整体迁移） |
| ap_kalla/ap_ghoststep 标记 + 普攻拦截（common.nut:248） | **普攻行为替换门面缺失**（§8 新缺口）——demo 以独立按键三连斩替代 |
| PO 20045/20046 分身（引擎按攻击节奏发射） | 施放瞬间齐射 1 发 Bullet（发射节奏引擎侧不可见，简化） |
| PO 20047 冥炎 + 引擎 DoT（3 层/1s×5） | `DarkFireBuff`（单层；3 层并行缺口记档） |
| KallaFinishReady/1/2/3 + 三个 atk | KallaFinishSkill 三段 SubState 连段（帧驱动盒） |
| 鬼影步态普攻/前冲增伤（col8） | 属性数值无伤害消费链（R1-A4）——跳过 |
| 附身期全技能打断表（nut 300 行） | 技能取消体系缺失（R1-A4）——跳过（我们不锁操作，天然可"打断"） |
| 暗属性伤害 | 元素属性系统缺失——无属性直伤 |
| kallaappear/stay 幻影 | Buff 视觉挂接缺失（R1-A5）——跳过或 Area 视图贴身跟随（R4-A17 同款缺口） |

### 注册点清单（草案号段，B5 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.Kalla=28`、`SkillIds.KallaFinish=29` + 两个新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanKallaLand=140、SwordmanKallaAir=141、SwordmanKallaFinishReady=142、SwordmanKallaFinish1=143、SwordmanKallaFinish2=144、SwordmanKallaFinish3=145、KallaShadowLand=146、KallaShadowAir=147、KallaFire=148、KallaAppear=149、KallaStay=150 |
| BuffId | `BuffDefinition.cs` | DarkFire=14、KallaPossession=15 |
| BulletId | `BulletDefinition.cs` | KallaShadowLand=6（空中版 7 预留） |
| json / 图集 | LSAnimClipRegistrar / BuildAtlas | json ×8~11；img 必需 9 张 |
| 按键 | LSOperaComponentSystem | 新按键 ×2 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 60000ms | 60000（直用） |
| 附身持续 | 30s（col0） | 30000（Buff 标记） |
| 分身 | col1 312~4484 魔攻；atk push50/lift200/hit horizon | 伤害 120/硬直 500/击退 50/浮空 200/穿透 800ms |
| 冥炎 | 3 层 × 5 次 × 1s × col7 31~224 无视防御 | 单层 5 跳 × 1s × 40 |
| 右斩 | col2 624~8968%；atk push30/lift30 | 伤害 100/硬直 500 |
| 左斩 | col3 936~13452%；同上 | 伤害 110 |
| 上斩 | col4 1872~26904%；atk down/push40/lift300 | 伤害 130/硬直 800/浮空 300 |
| 三连斩窗口 | FinishReady 660ms 起手，段间约 300ms | 660/960/1260ms 帧驱动 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| 角色/PO/effect 全部 .ani（~50 个） | 节面常规；kallafinish3 的 SET FLAG 65534 属惯例跳过项 | 现有 ani 子命令全覆盖 |
| Kalla.skl（13 列 + static 5 槽） | `.skl` 无子命令（既有） | 9 向量模板行全部自明，手抄零负担；static 5 槽语义未考证 |
| 6 个 .atk（角色 3 + PO 3） | `.atk` 无子命令（既有） | 手抄 ~8 值/文件 |
| 3 个 .obj | `.obj` 无子命令（既有） | 手工映射（§5 已给：2 PO→Bullet/Buff、fire→Buff 视觉） |
| kalla.nut 混淆 | 非翻译问题（C6 mod 形态①） | 走读按语义恢复（本文 §2.2 即是） |

计 3 条既有缺口（.skl/.atk/.obj），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **附身期普攻替换为二刀流链**（common.nut 普攻拦截 + 状态 44） | **普攻行为替换门面（新缺口，§8）**：NormalAttack 是独立 SkillLogic，无"按 Buff 条件改写其行为"的注入点 | 附身期不做普攻改造；三连斩独立按键直连（§5.6） |
| **攻击时自动发射分身**（引擎按攻击节奏） | 攻击事件钩子（施法侧，R4-A18 命中事件钩子的姊妹缺口） | 施放瞬间齐射 1 发替代"随攻击发射" |
| 冥炎 3 层并行独立结算 | LSBuff.Stack 是叠层刷新简版（实测），无并行多实例 | 单层 DoT；多实例需 LSBuffComponent 支持同 id 多实例（记档） |
| 再按指令键进二刀流 | 技能二段交互门面（R4-A16 三技共撞） | 独立按键或单次 PeekBufferedButton 窗口 |
| 鬼影步态普攻/前冲增伤（col8） | 属性数值无伤害消费链（R1-A4 三实证同族） | 跳过 |
| 附身期 20+ 技能打断表 | 技能取消体系（R1-A4） | 无锁操作天然可换技能，跳过专门实现 |
| 卡洛幻影跟随视觉 30s | Buff 视觉挂接（R1-A5） | 跳过；后续可用贴身 Area 视图近似 |
| 分身 Land/Air 双版本选择 | 我们无空中态（跳跃系统缺失 R1-A2） | 只做地面版 |
| 暗属性/音效/读条 500ms | 元素系统/音频/读条延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 5 槽（500 100 -300 200 -300）语义——疑分身发射的位置/速度参数，无脚本消费方（引擎侧）。
2. 引擎侧流程细节：ap_kalla 挂载时点、分身发射节奏（每次攻击一发？定时？）、冥炎 3 层的挂载与结算、再按指令键的判定窗口——均引擎内置不可见（087 定性法同款）。
3. 脚本三连击链（attackAni(1)/attackAni(0)/Frenzy1）与引擎 KallaFinish1-3 资源的两套三连击关系——疑 mod 重写普攻链 + 引擎保留原版终结技，demo 采用 Finish 资源（动画/atk/特效齐全）。
4. kallafinish3 F3 的 flag 65534 语义（惯例取消/命中标记，064 同款未考证）。

**新系统级缺口（主循环汇总）**
1. **普攻行为替换门面（形态改造类核心缺口）**："Buff 在身时改写另一技能的行为"（普攻替换/强化）。卡洛、暗天波动眼（88，同批）、血之狂暴系全撞此处。建议形态：SkillLogic 加 `virtual bool IsActiveForm(int buffId)` 或 SkillContext 查 Buff 门面（R4-A18 已记"自身 Buff 查询门面"，本技能是其第一个真实消费场景——查询+行为注入两层都要）。
2. **攻击事件钩子（施法侧）**："每次攻击命中/出手时触发 X"——分身自动发射、冥炎剑附身全靠它；与 R4-A18"技能命中事件钩子"合并立项（命中侧/出手侧两个注入点）。
3. **Buff 同 id 多实例/叠层上限**：冥炎 3 层独立结算需要"并行多实例 + 上限"；当前 Stack 简版（叠层刷新）不覆盖。

**给下轮的经验**：形态改造类（卡洛/暗天波动眼）的"标记 appendage 全是空壳"（ap_kalla/ap_ghoststep 4-7 行）——机制在 engine + common.nut 拦截钩子里；先查 `swordman_common.nut` 的 `sq_IsAppendAppendage` 调用点就能找到形态改写入口，别在 appendage 目录里找数值。kalla.nut 300 行打断表是模板化复制，读 1-2 个 case 即可。
