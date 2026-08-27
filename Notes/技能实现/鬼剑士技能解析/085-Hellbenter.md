# 魔狱血刹（Hellbenter）

> 技能ID 85 | 级别 B | 可实现性 🔶（深简化"终结斩+多段爆炸"可表达主干：施放即斩（跳过 120s 吸收蓄能模式）+ 25 段爆炸 Area；吸收模式的攻速/移速成长、血气之刃成长、按血气叠层增伤全部依赖属性消费链/成长型 Buff 缺失） | 分析日期 2026-08-22 | 批次 B5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 魔狱血刹 | `skill\Swordman\Hellbenter.skl` [name] |
| 英文名 | Hellbenter（取 skl 文件名；[name2] 实测 `Extreame Overkill`） | 同上 [name2] |
| 职业 | 狂战士（50 级一觉主动；explain"血之狂暴普通攻击"联动 + 血气系常识；[skill fitness second growtype] 1 2） | 同上 |
| 学习等级 | 50 | 同上 [required level] |
| 最高等级 | 50（二觉档 30，[second growtype maximum level] 第 7/8 位） | 同上 |
| 类型 | 主动（active，skill class 2） | 同上 [type] |
| 指令 | ↑↑↓↓ + Z | 同上 [command] |
| CD | 140000 ms（固定）；[league ban] 1 联赛禁用 | 同上 [cool time] |
| 施法时间 | 100 ms | 同上 [casting time] |
| MP | 900 → 7560 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×5 | 同上 [consume item] |
| 可施放状态 | 状态 8（普攻中可放） | 同上 [executable states] |
| static data | `600 400 0 0 70 105 150 210 280 280 500 640 2 75 100 2500 25 300 1 0 0 3000 0`（23 槽，见 §8） | 同上 [dungeon][static data] |
| 一句话效果 | 发动后进入血气吸收模式（最多 120 秒）：攻击敌人吸收血气 → 攻速/移速递增、血气之刃变大；血气满后释放最强一击——巨剑捶地多段攻击（25 段）+ 无属性爆炸终结，命中特殊出血敌人时按叠层数强化多段 | 同上 [explain] |

**level property（15 列，模板 12 行 12 向量，L21 读法）**：吸收血气时间 = col11×0.001 = **120 秒**（恒 120000）；
血气之刃物理攻击力 = col0~col1（709~1418 → 9138~18276）；多段物理攻击力 = col2~col3（529~1058 → 9138~18276）；
多段攻击次数 = **static 槽 16 = 25 次**（向量 `(16,16,1.0)` 正源=static，L21 正源读法实证）；
爆炸物理攻击力 = col4~col5（2661~2661 → 45687~45687）；增加攻击速度 = col6~col7 ×0.1（1%→78.5%）；
增加移动速度 = col8~col9 ×0.1（同）；强化特殊出血攻击力 = col10（1549→10000）。
col12/13/14（500/25/1000，Lv9 起 450/28/990）无模板行——语义未考证（疑吸收间隔/每次吸收量/上限）。
3/6/9 级里程碑（explain）：3 级攻速移速满档、6 级吸收时间增加、9 级特殊出血攻击力增加。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
// sqr/character/swordman_load_state.nut 行 102（实测）
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/hellbenter/hellbenter.nut", "swordman_hellbenter", 46, -1);
// swordman_header.nut 行 244（实测）：CUSTOM_ANI_HELLBENTERFINISH <- 74
// passiveobject.lst（实测）：20052 = Character/Swordman/HellbenterFinish.obj（行 11237-11238）
```

⚠ 状态 46 不绑定技能 ID（第 5 参 -1）；**主 nut 仅 16 行**（F3b 半内置极端形态，087 同代）——
吸收模式（血气成长/攻速移速增益/血气之刃变大/满气判定）**全部引擎内置**，
nut 只在引擎创建终结 PO 的瞬间做一次补发（见 2.2）。

### 2.2 主 nut 全文语义（hellbenter.nut，16 行）

```
onCreateObject_swordman_hellbenter(obj, createObject)          // 引擎创建对象时回调
  若 createObject 是被动对象且 getCollisionObjectIndex() == 20052（终结 PO）：
      写包(技能ID 85, subType 1)
      sq_SendCreatePassiveObjectPacket(cObject, 24370, 0, 0, 0, 0, 方向)
      // 在终结 PO 位置补发共享 PO 24370（F7/L20 家族）——魔改增强：给引擎演出挂"演出控制器"
```

### 2.3 被动对象 / appendage

**① PO 20052 HellbenterFinish（引擎创建的终结判定体，实测 .obj）**：

- normal 层 / pass all / piercing 1000；[basic motion] 空串（相位 1 无独立动画）；
- [etc motion] 10 项序列（引擎按 state 推进）：`0.ani（空占位）→ startfloor_00 → StartB_00 → 0.ani → StartA_00 → 0.ani → Loop_00 → 0.ani → EndFloor_07 → End_00`——即"起幕地板 → B 相 → A 相 → 循环相 → 终结地板+终结"的**固定时序演出 PO**（87 怖拉修同定性：非独立 AI）；
- [attack info] HellbenterFinish.atk（多段）+ [etc attack info] HellbenterFinishFinal.atk（终结）。

| .atk | 关键值 |
|---|---|
| HellbenterFinish.atk | physic / 无属性 / **down / push 0 / lift 170** / blow / no blood 40 / knuck back -1 |
| HellbenterFinishFinal.atk | physic / 无属性 / **down / push 200 / lift 300** / blow / no blood 40 |

**② 共享 PO 24370 case 85（mod 增补的演出控制器，`share_obj\swordman\` 实测）**：

- setcustomdata case 85：subType=1 → 定位到 parent（即 20052）并 `sq_moveWithParent` 跟随；
- procappend case 85（每帧）：
  - parent 首次 y≠0 → 播 `hellbenterstarta_01.ani` 起斩视觉；
  - parent **state 4（循环相）**：创建 `hellbenterloopfloor_06.ani`（bottom 层循环地板）；
    **若主角仍处状态 46 → 把主角切回 STATE_STAND（释放操作）**；循环动画播完自动 Rewind；
  - parent **state 5（终结相）**：播 `hellbenterendfloor_07/end_00.ani` 终结视觉 → 自毁，parent z 置 1000（防重入）；
- else.nut case 85：销毁时 RemoveAllAni。

即：**角色本体只在施放瞬间与终结瞬间受限，吸收模式期间与终结演出期均为自由行动**（24370 显式把角色切回待机）。

**③ 引擎内置部分（不可见，explain+数据反推）**：施放（读条 100ms）→ 挂吸收模式：攻击命中吸收血气
（每次吸收 +攻速/移速 col6-9、血气之刃视觉变大 sword1/2.ani、血之狂暴普攻 100% 特殊出血）→
血气满 → 终结攻击可用 → 角色播 HellbenterFinish.ani + 引擎创建 20052 → 25 段多段（static[16]）+
爆炸终结（col4/5）；特殊出血叠层 × 段数（col10 强化）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\…\hellbenterfinish.ani（槽 74） | 18 | 2700ms（17×100 + **F17=1000**） | 无 | **F3/F4/F5**：min/max `31 -70 0 420 140 280` 等（前伸 ~4 单位宽盒） | 终结斩（F17 超长收势帧，L23 族） |
| PO hellbenterstartfloor_00.ani | 11 | 660ms | 无 | 无 | 起幕地板（+10 层 als 叠加） |
| PO hellbenterstartb_00.ani | 16 | 1020ms | 无 | 无 | B 相（+4 层） |
| PO hellbenterstarta_00.ani | 7 | 660ms | 无 | **4 帧攻击盒** | A 相起斩（+31 层） |
| PO hellbenterloop_00.ani | 1 | 1200ms | 无 | **1 帧攻击盒**（单帧长驻判定） | 循环相（+29 层） |
| PO hellbenterloopfloor_00.ani | 11 | 900ms | 无 | 无 | 循环地板（+6 层） |
| PO hellbenterend_00.ani | 17 | 1260ms | 无 | **1 帧攻击盒** | 终结相（+33 层） |
| PO hellbenterendfloor_00.ani | 9 | 660ms | 无 | 无 | 终结地板（+7 层） |
| PO 0.ani | 1 | 500ms | 无 | 无 | 空占位（etc motion 分隔帧，L7） |
| effect\…\hellbenter\（30 个 .ani） | — | — | — | — | 吸收模式视觉：sword1/2（血气之刃）、buffmark1-3（增速标记）、targetmark1-3、absorb、bleeding1-2、cast1-2、completeflash/completesword、finish1-3、ballhead/balltail 系 |

`.als` 边车：角色/正式 PO 侧无；mod PO 目录（hellbenterr\）**5 个 .als**——全部是
`[use animation]` ×N + `[none effect add]` ×N 的**多层叠加表**（loop 30 层、starta 31 层、startfloor 10 层、
endfloor 7 层、startb 5 层——数字系列 _01~_33 全是同一相位的层叠贴图流）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Hellbenter.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Hellbenter.skl` | ✅（289 行） | 15 列 + 23 static 槽 |
| 注册行 | load_state 行 102（状态 46/技能 -1） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | §2.1 |
| 常量 | swordman_header.nut 行 244 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ | 动画 74 |
| 主 nut | hellbenter.nut | `…\pvf\sqr\character\swordman\hellbenter\hellbenter.nut` | ✅（16 行） | 终结 PO 补发钩子 |
| 共享 PO 24370 | share_obj\swordman\ setcustomdata/procappend/else case 85 | `…\pvf\sqr\common_object\share_obj\swordman\` | ✅ | 演出控制器（L20/F7） |
| .chr 条目 | etc motion #74（行 1047）+ etc attack（行 1355） | `…\pvf\character\swordman\swordman.chr` | ✅ | Finish.ani/.atk |
| 角色 .ani | hellbenterfinish.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.4 |
| 角色 .atk | hellbenterfinish.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | down/push0/**lift300**/cut+blood50 |
| PO lst | 20052（行 11237-11238） | `…\pvf\passiveobject\passiveobject.lst` | ✅ | ID→obj |
| PO 定义 | hellbenterfinish.obj | `…\pvf\passiveobject\character\swordman\` | ✅ | §2.3① |
| PO .atk | hellbenterfinish.atk / hellbenterfinishfinal.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 多段/终结 |
| PO .ani | hellbenterr\ 167 文件（**mod 目录**，.obj 引用链定点访问） | `…\pvf\passiveobject\script_sqr_nut_qq506807329\swordman\animation\hellbenterr\` | ✅ | 七相位 + 5 als + 辅助视觉 |
| 吸收模式特效 | hellbenter\ 30 .ani | `…\pvf\character\swordman\effect\animation\hellbenter\` | ✅ | §2.4 末行 |
| 装备层 | *hellbenter*.ani ×76 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ | 换装图层 |
| 地牢变体 | KContents3\DarkTemple\Rosenberg_Grey\Hellbenter*.obj ×3 | `…\pvf\passiveobject\`（lst 行 25682/26112/26114） | ✅（存在，未读） | 副本内容物，本技能不用 |

## 4. 资源需求

| img（按 NPK 归并） | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色终结斩 | 必需（共享） | ✅ 已在库 |
| New01-19.img（19 张） | sprite_character_swordman_effect_hellbenter.NPK | 巨剑相位层叠流（starta/loop/end 各层） | **必需** | ❌ |
| sword-dodge / sword-normal.img | 同上 | 血气之刃本体（吸收模式跟随武器） | **必需** | ❌ |
| slash-d / slash-n.img | 同上 | 终结斩弧光 | **必需** | ❌ |
| start-dodge / start-normal.img | 同上 | 起幕 | **必需** | ❌ |
| impact-d / kaaa-d1.img | 同上 | 终结爆点 | **必需** | ❌ |
| NewSmoke01-04、blood-normal、change-d/n、energy、exi-particle、sim1/2-dodge、target-dodge/normal、woong-dodge.img | 同上 | 烟/血/换装光/能量/粒子/标记（13 张） | 可选 | ❌ |
| bloodboom_boomback/boomfront/finish/finish2.img | sprite_character_swordman_effect_bloodboom.NPK（**跨技能借图**，L14） | 爆炸层 | 可选 | ❌ |
| 01floor_D/N（BloodMarble）、riven_bottomblood/circle（BloodRiven）.img | 各源 NPK（跨技能借图） | 地板血渍/血圈 | 可选 | ❌ |
| sword_light_tail.img（HundredSword） | sprite_character_swordman_effect_hundredsword.NPK | 珠尾 | 可选 | ❌ |
| supergirl_change_glows.img（**跨职业** Mage/Bellatrix）、Dust01/02、Smoke08.img（Common） | 各源 NPK | 换装光/尘/烟 | 可选 | ❌ |

缺失 img：必需级 26 张 + 可选级 28 张（主 NPK 39 张一次提取全覆盖；跨 NPK 借图 8 张）。
img 版本红线（v2/v4 可用/v5 不可）由提取时把关。

## 5. 实现方案草案（深简化"终结斩 + 多段爆炸"——跳过 120s 吸收蓄能：吸收模式砍成固定值，见 §7）

### 内容件清单

1. **`DotNet~/Skills/HellbenterSkill.cs : SkillLogic`**（同 BloodBoomSkill 帧触发范式）
   - `CooldownMs=140000`（DNF 原值直用，demo 可缩 30000）；`TotalTimeMs=2700`（hellbenterfinish.ani 时长直用，弃 F17 收势取 1700 亦可，demo 用 2700）。
   - OnCast：`ctx.PlayAnim(AnimId.SwordmanHellbenterFinish)` + `ctx.ClearHitTargets()` + `ctx.SetSubState(0)`。
   - OnUpdate：
     - `≥300 && SubState==0`（F3）：本体斩击 `ctx.SetAttackHitbox(前偏 2.2, 半尺寸 (2.0,1.0,1.4))`（F3 盒 x[31,420] 折算）+
       `HitReaction`（角色 atk：down/lift300/cut+blood50 → 伤害 200/硬直 800/浮空 300 + Bleed 100%）；
       同帧 `ctx.CreateAreaInFront(AreaIds.HellbenterStorm, 1.5)`（多段爆炸区）；`SetSubState(1)`。
   - OnEnd：`ctx.PlayDefaultAnim()`。
   - **吸收模式简化**：不进模式、不吸收——攻速/移速增益/血气之刃成长/满气判定全部砍掉（§7）；
     血之狂暴普攻特殊出血 → 本体斩击直接挂 `ProcBuffId=BuffIds.Bleed, ProcChance=100`（复用现有 BleedBuff 占位"特殊出血"）。
2. **`DotNet~/Areas/HellbenterStormArea.cs : AreaDefinition`**（25 段多段+终结，同 FireCircleArea Tick 范式 + 双相位）
   - 多段相位：`TotalTimeMs=1200`（loop_00 单帧 1200ms）、`TickTimeMs=48`（1200/25 段=static[16]×时长对齐——
     **L19 第二档：Area Tick 无去重 = 天然多段**）、`EnterActions={MeleeHit}`、`TickActions={MeleeHit}`、
     `HalfExtents=(4.0,1.2,2.5)`（巨剑 loop 判定简化）、
     `HitReaction{Damage=40, HitstunMs=250, KnockbackX=0, LaunchY=170}`（HellbenterFinish.atk down/lift170 → 持续浮空连打）；
     `ViewAnimId=AnimId.HellbenterLoop` + `ViewBackAnimId=AnimId.HellbenterLoopFloor`。
   - 终结相位：技能 OnUpdate `≥1500 && SubState==1` → `ctx.CreateAreaInFront(AreaIds.HellbenterFinal, 1.5)`——
     `TotalTimeMs=1260`（end_00）、`EnterActions={MeleeHit}`、`HalfExtents=(4.5,1.5,3.0)`、
     `HitReaction{Damage=400, HitstunMs=1000, KnockbackX=200, LaunchY=300}`（FinishFinal.atk 原值 down/push200/lift300）；
     `ViewAnimId=AnimId.HellbenterEnd` + `ViewEndAnimId=AnimId.HellbenterEndFloor`。
3. **无需新 Buff**（特殊出血复用 BleedBuff；吸收模式增益砍掉后无 Buff 需求）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 46 吸收模式（120s/血气成长） | **砍掉**（属性消费链 + 成长型 Buff 双缺失）；demo 直接满气终结 |
| 血之狂暴普攻 100% 特殊出血 | 斩击 `HitReaction.ProcBuffId=Bleed`（近似，特殊出血≠普通出血，记档） |
| 特殊出血叠层 × 25 段强化（col10） | 砍掉（叠层上限+伤害联动缺失） |
| 角色 HellbenterFinish.ani F3-F5 攻击盒 | 技能 `SetAttackHitbox`（帧驱动，releasewave 同构） |
| PO 20052 多相位演出（startfloor→A/B→loop→end） | 两个 Area 时序创建（L9 多相位→Area 编排） |
| 25 段多段（static[16]） | `TickTimeMs=48` 的 Tick 多段（L19 第二档） |
| 24370 演出控制器（释放角色/循环视觉） | 不需要——我们技能短站桩即可；"释放角色自由行动"砍掉 |
| 攻速/移速增益（col6-9） | NumericType.Speed 移动端零消费（R2-A7）+ 攻速系统无 → 砍 |
| 屏震/音效（ASTORM_HIT_FIRE 等） | 延后档跳过 |

### 注册点清单（草案号段，B5 批）

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `SkillIdAttribute.cs` | `SkillIds.Hellbenter=30` + ButtonToSkill 新键 |
| AnimId | `AnimConfigRegistry.cs` | SwordmanHellbenterFinish=151、HellbenterStartFloor=152、HellbenterStartA=153、HellbenterStartB=154、HellbenterLoop=155、HellbenterLoopFloor=156、HellbenterEnd=157、HellbenterEndFloor=158、HellbenterSword=159 |
| AreaId | `AreaDefinition.cs` | HellbenterStorm=32、HellbenterFinal=33 |
| json / 图集 | LSAnimClipRegistrar / BuildAtlas | json ×9；img 必需 26 张 |
| 按键 | LSOperaComponentSystem | 新按键分支 |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 140000ms | 140000（演示可缩 30000） |
| 总时长 | 角色斩 2700ms；PO 演出 start 660-1020 + loop 1200 + end 1260 ≈ 3.5s | 2700（斩+双 Area 编排） |
| 本体斩 | col0/1 709~1418%；atk down/lift300/cut+blood50 | 伤害 200/硬直 800/浮空 300/Bleed 100% |
| 多段 | col2/3 529~1058% × 25 次；atk down/lift170 | 40 × 25 段（48ms 间隔）/浮空 170 |
| 爆炸终结 | col4/5 2661%；atk down/push200/lift300 | 400/硬直 1000/击退 200/浮空 300 |
| 特殊出血强化 | col10 1549~10000 × 叠层数 | 砍掉 |
| 吸收时间 | 120s（col11） | 砍掉（无模式） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| hellbenterr\ 5 个 .als | `[use animation]`/`[none effect add]` 均已支持（als 子命令）——但 **30+ 层的巨型叠加表**（loop 30 层/starta 31 层） | 可直译；消费侧 LSAnimOverlayViewComponent 每层一 GO，验证 30+ 层性能（一次性演出可接受） |
| hellbenterr\ 167 个 .ani | 节面常规 | 现有 ani 子命令全覆盖 |
| Hellbenter.skl（15 列 + 23 static 槽） | `.skl` 无子命令（既有）——static 23 槽为本批最大 | 手抄 12 向量可接受；static 槽多数未考证 |
| 3 个 .atk（角色 1 + PO 2） | `.atk` 无子命令（既有）；`[knuck back] -1` 为表外字段 | 手抄；knuck back 纳入 atk 子命令字段设计清单（R2-A8 系） |
| hellbenterfinish.obj（etc motion 10 项跨目录引用） | `.obj` 无子命令（既有）；**相对路径跨目录引用**（`../../script_sqr_nut_qq506807329/...`） | obj 子命令立项时需处理相对路径归一（obj 引用文件名失配族又一变体） |

计 3 条既有缺口 + 1 条小适配（obj 跨目录路径），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| **120s 吸收蓄能模式**（攻击吸收→成长） | 攻击事件钩子（出手侧，本批 082 同报）+ 成长型 Buff（数值随叠层变化）+ 属性消费链（R1-A4/R2-A7 双实证） | 整体砍掉：demo 施放即满气终结 |
| 攻速/移速递增（col6-9，1%→78.5%） | NumericType.Speed 零消费 + 攻速系统不存在 | 砍掉 |
| 血气之刃随血气变大（sword1/2 + 对象缩放） | 对象整体缩放（延后，IMAGE RATE 同族） | 固定尺寸视觉 |
| 特殊出血叠层 × 段数强化（col10） | Buff 叠层上限（本批 082 上报）+ 增伤联动消费链 | 普通 BleedBuff 占位 |
| 角色吸收期/演出期自由行动（24370 切 STAND） | 技能期间放开控制（087 同款） | 短站桩 2.7s 版 |
| 终结 25 段多段 | 多段命中——**Area Tick 可表达**（L19 第二档，不算缺口） | TickTimeMs=48 |
| 巨剑 30+ 层 als 叠加 | 翻译可通；性能未验证 | 先译 8 层内核心层（_00 主层+地板），其余层后续补 |
| 音效/屏震/联赛禁用 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 23 槽中仅槽 16（25 段）有模板行印证；槽 0/1/4-11/15/17/21（600/400/70/105/150/210/280/280/500/640/2500/300/3000）语义未知（疑：血气之刃尺寸、吸收单次量/上限、攻速成长步长、满气阈值）。
2. col12/13/14（500/25/1000 → Lv9 起 450/28/990）语义（疑吸收间隔/每次量/上限——与 6 级里程碑"吸收时间增加"的对应关系不明：col11 恒 120000 未增）。
3. 引擎侧吸收模式细节：血气吸收的判定（命中即吸？普攻限定？）、满气阈值、终结攻击的触发方式（自动/再按键）——引擎内置不可见（087 定性法）。
4. PO 20052 etc motion 与引擎 state 的精确对应（state 4=loop、state 5=end 由 24370 控制器反推；其余 state 值未考证）。
5. 地牢变体 3 个 obj（DarkTemple）未读——非本技能主体。

**系统级缺口**
- 无新缺口（吸收模式的三个依赖——攻击出手钩子/成长型 Buff/属性消费链——均已记档，本技能为"三缺叠加"的典型样本；建议 00-总览把魔狱血刹列为"属性数值消费链"立项的第 4 实证）。

**给下轮的经验**：一觉类（state 不绑技能、nut <20 行、CD 140s）先查 `share_obj\swordman\` 六回调里有没有本技能 case——**mod 作者把演出控制器都挂 24370**（85/87/241 全是），引擎本体逻辑反推 + 24370 case 直读是这类技能的固定套路。hellbenterr 目录 167 文件里数字系列全是 als 层叠流，只读 _00 主层帧表即可，别逐个读。
