# 破军斩龙击（ChargeCrashEx）

> 技能ID 98 | 级别 E（**预分类纠偏：文件名以 Ex 结尾但不是 TP 强化被动——[type] active、skill class 1，剑魂二觉替换型主动技**，本文按 B 类深度走读；同目录 218-ChargeCrashExp 才是"强化-破军升龙击"TP 被动） | 可实现性 ✅（三段式与基础技 068 同构直译；眩晕走 L6 ProcBuff 链（FreezeBuff 先例），撞敌停驻用 068 已验证的 CheckHit 轮询） | 分析日期 2026-08-22 | 批次 E5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 破军斩龙击 | `skill\Swordman\ChargeCrashEx.skl` [name] |
| 英文名 | ChargeCrashEx（取 skl 文件名；[name2] 实测 `Charge Bust`） | 同上 |
| 职业 | 剑魂二觉（[second growtype maximum level] 12 槽第 2/3 位（0 基）= **50 级**，R6-C4 职业判定捷径：(0,1)共通/(2,3)剑魂/(4,5)鬼泣/(6,7)狂战/(8,9)阿修罗/(10,11)剑影；[skill fitness growtype] 空 = 一觉树无此技；[skill fitness second growtype] `1 2` 三 Ex 同值、非职业指示） | 同上 |
| 学习等级 | 60（[required level range] 2）；前置 68 破军升龙击 Lv1（[pre required skill]） | 同上 |
| 最高等级 | 50（[maximum level]） | 同上 |
| 类型 | active（skill class 1；[auto cooltime apply] 1 施放即进 CD；[weapon effect type] physical） | 同上 [type] |
| 指令 | →←↓→ + Z（[skill command advantage] 50/50） | 同上 [command] |
| CD | 30000 ms（pvp 同 30000 + start cool 600000） | 同上 [dungeon][cool time] |
| MP | 400 → 1120（Lv1→Lv50） | 同上 [consume MP] |
| 特殊消耗 | 道具 3037 ×1（无色小晶块）；耐久损耗 30 | 同上 [consume item] |
| static data | **无 [static data] 节**（与基础技 068 不同——原 static 冲撞参数疑并入 col4） | 同上 |
| 一句话效果 | 肩撞击退浮空并眩晕 → 快速转身用剑穿刺（将敌人聚在一处）→ 上挑攻击浮空（基础技的强化重制三段式） | 同上 [explain] |
| 与基础技关系 | **[pre required skill] 68 单向依赖**；基础 skl [feature skill index] 指向 218（TP 版）而非 98——98 是独立技能不是 TP（双向链接法第三类形态：无 feature 链） | 两 skl 实测 |

**level property 模板解码（8 列，L21 向量法，7 向量全明，Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 冲撞攻击力 | (-1,0,1.0) | col0 = 1728 → 19552 % |
| 穿刺攻击力 | (-1,1,1.0) | col1 = 2592 → 29330 % |
| 上挑攻击力 | (-1,2,1.0) | col2 = 2160 → 24443 % |
| 上挑浮空力 | (-1,3,1.0) | col3 = **400 恒定** |
| （col4 无模板行） | — | 300 恒定——与基础技 static[2]=300 同值，**疑冲撞距离参数**（推断，无消费者） |
| 眩晕机率 | (-1,5,0.1) | col5 = 400→520 → **40% → 52%** |
| 眩晕Lv | (-1,6,1.0) | col6 = Lv60 → Lv134 |
| 眩晕持续时间 | (-1,7,0.001) | col7 = 2000→5920 → **2.0 → 5.92 s** |

## 2. 技能逻辑走读

### 2.1 注册与文件链（纯引擎内置，F3）

- load_state **无 98/chargecrashex 注册**（grep 实测；仅基础技 90 行状态 37 注册，第 5 参 -1 不绑定）。
- `sqr\character\swordman\` + `JG_SwordMan\` 递归 grep：**仅 swordman_header.nut:269-271 三行常量**——
  `CUSTOM_ANI_CHARGECRASHEXSHOULDER <- 99` / `CHARGECRASHEXPICK <- 100` / `CHARGECRASHEXUPPER <- 101`。
- .chr 实测：etc motion 槽 **99/100/101**（行 1072-1074）= `Animation/ChargeCrashEx{Shoulder,Pick,Upper}.ani`；
  etc attack info 槽 **68/69/70**（行 1362-1364）= `AttackInfo/ChargeCrashEx{Shoulder,Pick,Upper}.atk`。
- 无 PO、无 appendage、无 [skill preloading image]——**全部三段时序由引擎按 .ani/.atk/等级表驱动**（068 引擎状态 37 家族同型，比 068 更干净：连 mod 钩子壳都没有）。

### 2.2 引擎行为重建（.ani + .atk + 等级表三方印证）

```
施放（→←↓→+Z 或技能栏，MP+无色×1，CD 30s）：
第一段·肩撞（ChargeCrashExShoulder.ani 300ms）：
  F0-F4 攻击盒 `-10 -30 8 105 60 68`（贴身窄长撞区，同基础技 dash 盒几何）
  命中反应 ChargeCrashExShoulder.atk：物理/武器、down、lift 200、push 30、hit down
    + [active status] stun 0 0 0 → 运行时写入眩晕 col5 机率/col6 等级/col7 时长（L6 标准链路）
  F7 flag 1（疑段切标记，未考证）
第二段·转身穿刺（ChargeCrashExPick.ani 265ms）：
  F0-F2/F4-F8 长盒 `-60 -30 0 227 60 120`（前后双向长穿刺）+ F3 前向短盒 `82 -20 0 85 40 120`
  命中反应 ChargeCrashExPick.atk：damage 反应/hit horizon/无 push 无 lift——"聚在一处"＝穿刺不产生位移
  F10 flag 1
第三段·上挑（ChargeCrashExUpper.ani 400ms）：
  F1 flag 1 + F1-F2 大盒（`-94 -30 0 174/197 60 94`）+ F3 收窄盒（`-22 -30 1 137 60 138`）
  命中反应 ChargeCrashExUpper.atk：down/lift 0/push 0——浮空由引擎按 col3=400 上挑浮空力施加（atk lift=0）
```

三段帧尾 flag 1 三连现——疑"段完成/转段"引擎标记（未考证；我们按���画时长驱动等价）。

### 2.3 被动对象 / appendage

无。基础技 068 的下捶 PO（ChargeCrashSub.obj）与本技无关（068 第三段是精通条件段；Ex 三段全是本线）。

### 2.4 动画关键帧表（角色侧实测 + 特效层）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\ChargeCrashExShoulder.ani`（etc 99） | 8 | 300ms（25×4+50×4） | F7=1 | F0-F4 | 肩撞；sm_body 图集 |
| `ChargeCrashExPick.ani`（etc 100） | 11 | 265ms | F10=1 | F0-F8（F3 短盒） | 转身穿刺 |
| `ChargeCrashExUpper.ani`（etc 101） | 8 | 400ms | F1=1 | F1-F3 | 上挑；**带 .als**（4 [use animation]+2 [add]+2 [create draw only object]：Upper/UpperChargeUp 叠层 + DustBack/DustFoward 双尘土绘制体） |
| effect\animation\chargecrashex\shoulder.ani | 6 | 430ms | — | — | 肩撞特效（+ .als） |
| effect\...\pick.ani | 4 | 348ms | — | — | 穿刺特效（+ .als） |
| effect\...\upper.ani / upperbody / upperchargeup | 7/6/8 | 350/380/482ms | — | — | 上挑三层特效 |
| effect\...\upperdustback/foward、shoulderbottom、picklinebottom | 4/7/4/1 | 280/460/348/350ms | — | — | 尘土/底线层 |
| effect\...\enemybottom.ani | 1 | **100000ms** | — | — | 悬停帧（L23 待事件型，翻译需钳制） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ChargeCrashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ChargeCrashEx.skl` | ✅（237 行全读） | 8 列全解 |
| lst 条目 | ID 98 | `…\pvf\skill\swordmanskill.lst` 369-370 行 | ✅ 实测 | — |
| 注册行 | —（无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无 | 纯引擎（F3） |
| 常量表 | swordman_header.nut:269-271 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | CUSTOM_ANI 99/100/101 |
| .chr 条目 | etc motion 99-101（行 1072-1074）；etc attack info 68-70（行 1362-1364） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 三段 .ani + 三 .atk |
| 角色 .ani | ChargeCrashExShoulder/Pick/Upper.ani（+Upper.ani.als） | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 帧表 |
| 角色 .atk | ChargeCrashExShoulder/Pick/Upper.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | §2.2 |
| 特效 .ani | chargecrashex\ 15 文件（含 .als ×3） | `…\pvf\character\swordman\effect\animation\chargecrashex\` | ✅ 实测 | §2.4 |
| PO / appendage | —（不存在） | `…\pvf\passiveobject\character\swordman\`（仅基础技 chargecrashsub） | ⛔ 无 | 引擎直查 |
| 装备层 | —（未查；sm_body 单图集动作，L16） | `…\pvf\equipment\character\swordman\avatar\` | 未查 | 同 068 结论 |
| 基础技文档 | 068-ChargeCrash.md | 本目录 | ✅ | 结构对照（状态 37 三段式） |
| 同名 TP 技 | 218-ChargeCrashExp.md | 本目录 | ✅ | TP 版（本批无关，交叉引用） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 三段角色动作 | 必需（共享） | ✅（L16） |
| `Character/Swordman/Effect/ChageCrashEx/Shoulder.img`、`ShoulderLine.img` | sprite_character_swordman_effect_chagecrashex.NPK | 肩撞特效 | 必需 | ❌ |
| `…/ChageCrashEx/Pick.img`、`PickLineNormal.img`、`PickLineDodge.img` | 同上 | 穿刺特效 | 必需 | ❌ |
| `…/ChageCrashEx/Upper.img`、`UpperUpDodge.img`、`UpperBodyUpDodge.img`、`UpperCircleDodge.img` | 同上 | 上挑特效 | 必需 | ❌ |
| `Character/Swordman/Effect/ChargeCrash/dash.img` | sprite_character_swordman_effect_chargecrash.NPK | 肩撞尾焰（**复用基础技贴图**，与 068 共享） | 可选 | ❌（068 必需档同图） |
| `Character/Mage/Effect/RandomPierceStrike/dust.img` | sprite_character_mage_effect_randompiercestrike.NPK | 尘土（跨职业借图，L14） | 可选 | ❌ |
| `Character/Fighter/Effect/StrongestLowKick/dust(normal).img` | sprite_character_fighter_effect_strongestlowkick.NPK | 尘土（跨职业借图） | 可选 | ❌ |

**缺失 img：必需 9 张（同一 NPK 一次提取；⚠ NPK 名按 pvf 原文拼写 ChageCrashEx——原版官方 typo，提取时照抄）、可选 3 张（跨 3 NPK）。**
AnimRes/AnimConfigRegistry 实测：仅 sm_body 在库，其余全未入库。

## 5. 实现方案草案（号段：SkillIds 34 / AnimIds 187-190，E5 批内顺延；撞号无妨 L18）

### 内容件清单

1. **`DotNet~/Skills/ChargeCrashExSkill.cs : SkillLogic`**（068 ChargeCrashSkill 同范式：帧号 const + SubState 段机 + 撞敌停驻轮询）：
   - `CooldownMs=30000`；`TotalTimeMs=965`（肩撞 300 + 穿刺 265 + 上挑 400）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanChargeCrashExShoulder)`、`ctx.ClearHitTargets()`、SubState=0。
   - `OnUpdate`：
     - SubState 0（肩撞 0-300ms）：帧驱动盒（json 自带 attackBox）+ 技能级 `HitReaction{Damage=170, HitstunMs=500, KnockbackX=30, LaunchY=200, ProcBuffId=BuffIds.Stun, ProcChance=40}`（atk 直译 + 眩晕 40% Lv1 档）；撞敌或滑完 → `ctx.ClearHitTargets()` + 切 PlayAnim(Pick)。
     - SubState 1（穿刺 300-565ms）：`ctx.SetAttackHitbox(前偏 0.85, 半尺寸 (1.4,0.3,0.6))`（长盒折算），`HitReaction{Damage=260, HitstunMs=300, KnockbackX=0, LaunchY=0}`（damage 反应、无位移＝聚怪观感）；段末清盒清目标。
     - SubState 2（上挑 565-965ms）：F1 时刻 `ctx.CreateAreaInFront(AreaIds.ChargeCrashExUpper, 0.5)`（§2.2 大盒折算），Area 独立反应吃浮空力。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/ChargeCrashExUpperArea.cs : AreaDefinition`**（068 ChargeCrashUpperArea 直改）：
   `TotalTimeMs=200`、`EnterActions={MeleeHit}`、`HalfExtents=(1.0,0.3,0.5)`、
   `HitReaction{Damage=220, HitstunMs=800, KnockbackX=0, LaunchY=400}`（atk lift=0 + 引擎浮空力 col3=400 → LaunchY 400 直用，068 上挑同构）、
   `ViewAnimId=AnimId.ChargeCrashExUpperEffect`。
3. **`DotNet~/Buffs/StunBuff.cs : BuffDefinition`**：FreezeBuff 同构新写（ForbidMoveOn/Off 定身）——眩晕 2.0-5.9s（demo 取 2s），L6 链路已支持 ProcBuffId+ProcChance，零新机制。
4. **无需**新 Action/Bullet；无 PO。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎三段（.chr 99-101 顺序播放） | SubState 段机 + PlayAnim（帧号 const） |
| 肩撞 stun 三参（col5/6/7 → atk [active status] 运行时写入） | `HitReaction.ProcBuffId/ProcChance` + StunBuff（FreezeBuff 先例，L6） |
| 穿刺"聚在一处"（damage 反应无 push） | 无位移命中（KnockbackX=0）——敌人留在原地自然聚集 |
| 上挑浮空力 col3=400（atk lift=0，引擎施加） | Area HitReaction.LaunchY=400（068 同款折算） |
| 帧尾 flag 1 ×3 | 按动画时长驱动（等价），flag 语义未考证不实现 |
| 转身（explain"快速转身"） | 施法者朝向固定（方向输入缺口 R1-A3，观感差异小） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.ChargeCrashEx = 34` + ButtonToSkill 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `ChargeCrashExUpper = 41` |
| BuffId | `Runtime\BuffDefinition.cs`（或 BuffIds 表） | `Stun = 19`（Freeze 同族） |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanChargeCrashExShoulder=187、Pick=188、Upper=189`（特效 190 预留） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | 角色 3 json + Upper.als 叠层；图集 1 张（chagecrashex NPK 9 张必需） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000 ms | 30000（直用；demo 可缩 10000） |
| 三段时长 | 300/265/400 = 965ms | 直用 |
| 肩撞伤害/反应 | col0 1728%；down/push30/lift200 | Damage 170 / Kb 30 / Ly 200 / Hitstun 500 |
| 眩晕 | 40%→52% / Lv60→134 / 2.0→5.9s | ProcChance 40% / Stun 2s（等级跳过） |
| 穿刺伤害/盒 | col1 2592%；x[-60,227] 长盒 | Damage 260 / 前偏 0.85 半尺寸 (1.4,0.3,0.6) |
| 上挑伤害/浮空 | col2 2160%；浮空力 col3=400 | Area Damage 220 / LaunchY 400 / Hitstun 800 |
| MP/无色/耐久 | 400-1120 / 3037×1 / 30 | 跳过（延后档） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ChargeCrashEx.skl | `.skl` 无子命令（8 列 + 二觉 12 槽节） | 手抄 §1 已全解；skl 子命令同前议 |
| ChargeCrashEx{Shoulder,Pick,Upper}.atk | `.atk` 无子命令（Shoulder 含 [active status] stun 三参运行时写入位） | 手抄（每文件 ≤10 值）；atk 子命令设计时 active status 节纳入（L6 已有消费方） |
| ChargeCrashExUpper.ani.als | `[create draw only object]`（双绘制体） | **已知缺口**（R1-A4/R2-A10 记档）——按 [add] 同构支持，重复印证 |
| enemybottom.ani | `[DELAY] 100000` 悬停帧 | L23 既有缺口：钳制/手改 |
| 全部 .ani | FRAME/IMAGE/DELAY/ATTACK BOX 常规节 | **现有 ani 子命令全覆盖** |

翻译缺口计 4 条（.skl/.atk/.create-draw-only/超长 DELAY——前三者常驻，后两条既有记档再印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 眩晕 Lv60-134（等级对抗） | 状态等级 Lv 系统（R6-C4 缺口，90/6 两消费方后第 3 例） | 跳过等级、固定 2s 定身（机率保留） |
| "聚在一处"聚怪牵引观感 | 无位移他人/牵引门面（R2-A8 拉拽族） | 穿刺零位移命中已得"聚集"观感（敌人不动=聚），不做主动牵引 |
| 转身朝向 | 技能中方向输入（R1-A3） | 固定朝向 |
| 三段帧尾 flag 1 | 未考证（疑段切标记） | 动画时长驱动等价 |
| 无色消耗/耐久/MP | 延后档 | 跳过 |
| 屏震/音效（R_SLESSSWDA_HIT） | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. 三段帧尾 flag 1 的引擎语义（疑段切/转段标记——基础技 068 用 65534，本技用小值 1，两种标记并存）。
2. col4=300 恒定列无模板引用（与基础技 static[2]=300 同值，疑冲撞距离/速度参数）。
3. [skill fitness second growtype] `1 2` 的语义（三 Ex 同值，非职业指示——12 槽才是职业判定）。
4. pvp 表 col7 末值 11800（11.8s 眩晕）与 dungeon 5.92s 的平衡差异是否被引擎钳制。

**缺口上报**：无新系统级缺口（眩晕走 L6 既有链路；状态等级 Lv 系统第 3 消费方记档）。

**预分类纠偏上报（主循环记账）**：**Ex 后缀 ≠ TP**——98/100/102 三技均为 [type] active 的二觉替换型主动技（本 pvf 命名规律：**Ex=二觉替换主动技、Exp=TP 强化被动**，但 145-156 段的老 TP 也用 Ex 后缀，判定仍以 [type]/[feature skill type] 为准）。基础技 skl 的 [feature skill index] 只指向 TP 版（218/216/165），与主动替换技零链接——E3 的"双向链接 10/10"结论只对 TP 形态成立。

**给轮间经验**：二觉替换技的常量入口在 swordman_header.nut 269-271 行（CUSTOM_ANI_CHARGECRASHEX* 三连），.chr etc 槽 99-101 与 attack info 68-70 成对出现——同族技能（FireWaveEx 99/BloodBlastEx 101/MomentarySlashEx 97 等）可按 header 常量名直查。
