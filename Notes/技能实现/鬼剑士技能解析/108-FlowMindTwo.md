# 流心：跃（FlowMindTwo）

> 技能ID 108 | 级别 A | 可实现性 🔶（跳跃弧线/连携入口/武器分档三项降级后主干可表达，见 §5/§7） | 分析日期 2026-08-22 | 批次 A7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 流心：跃 | `skill\Swordman\FlowMindTwo.skl` [name] |
| 英文名 | FlowMindTwo（取 skl 文件名；[name2] 实测为英文别名 `Flow Heart : Jump`） | 同上 [name2] |
| 职业 | 剑魂（[skill fitness growtype]=1，L17 映射；流心系专属） | 同上 |
| 学习等级 | 25（前置：流心 105 Lv1，[pre required skill]） | 同上 |
| 最高等级 | 70（growtype 上限：仅剑魂 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | **（流心动作中）C 跳跃键**起手，**（追加操作）X 攻击键**砍击 | 同上 [command] {6=`(JUMP)`} / [command key explain] |
| CD | [cooltime level info]=1 → **CD 取 level info col1**：Lv1 **6500ms** → Lv70 **-177ms**（递减到无，"推断"——col1 单调递减与该节声明吻合） | 同上 [cooltime level info] + [level info] |
| MP | 30 → 350（顶层 [consume MP]） | 同上 |
| static data | **14 值** `300 200 150 -10000 300 100 50 2 100 350 300 10 110 70`——引擎消费的跳跃物理参数组（水平速度/上升初速/Y 轴移动速度/重力…精确对应**未考证**） | 同上 [static data] |
| 一句话效果 | 向前轻轻跳跃，空中追加 X 发出砍击；方向键可 Y 轴移动；不追加则落地回流心态；钝器命中产生冲击波，太刀/短剑为范围更广的横斩 | 同上 [explain] |

**level property（4 列，Lv1 → Lv70 首末值）**：`755→6278`、`6500→-177`、`100→194`、`472→3924`。
**推断**：col0=跃攻击力%（755→6278 成长形态与攻击列一致）；col1=CD ms（[cooltime level info] 指向）；col2/col3 未考证（疑追加砍击攻击力/冲击波攻击力，对应两段 atk 与钝器分支）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（引擎内置状态 + mod 双 nut 现状）

**load_state 无 state 63 的 pushState**（实测全表核对）——流心系状态号 61（流心态）/62（流心:刺）/63（**流心:跃**）/64（流心:升）均引擎内置，pvf 只留回调钩子 nut：

```
// sqr/character/swordman/swordman_common.nut procAppend_Flowmind_Comminterrupt（mod 改写的连携中枢，实测节选）
if (mystate == 0 || mystate == 8 || mystate == 14 || mystate == 62 || mystate == 64 || mystate == 29 || mystate == 147) {
    EnableSoften(obj, 108, 63);
    SetSkillState(obj, 108, 63, [0, 61, 62, 104, 0]);   // 从站街/普攻/流心:刺/流心:升等状态可直入跃（技能108→状态63）
}
```

白名单内存在**两份** flowmindtwo.nut（需区分，C3 坑同款）：
- `sqr\character\swordman\flowmind\flowmindtwo.nut`——**变量名混淆的近原版钩子**（onAfterSetState_swordman_flowmindtwo / onProcCon_swordman_flowmindtwo，状态名前缀与引擎内置注册吻合）；
- `sqr\character\swordman\weaponmaster\flowmind\flowmindtwo.nut`——**mod 重写版**（202 行，拼音变量 weiyi/liuxing/jieshu；把攻击段动画改挂到流心壹落地 ani 176/177/178 与 zigadventcast 179，伤害读技能 **13**（三段斩）col0×2.1——均为 mod 自创，与原版无关，本档只作状态号/子状态号旁证）。

### 2.2 主流程（引擎内置状态 63 的子状态机，钩子 nut + 数据文件拼合还原）

```
子状态 0（跳跃段，引擎内置）:
    播 FlowMindTwoJump.ani（etc motion #108，3 帧 300ms，无攻击盒）
    按 static data 14 参数做前跳抛物线（水平 300 / 上升 200 / 重力 -10000 …推断）；
    空中可按方向键 Y 轴移动（explain 明言）
空中按 X → 子状态 200（砍击段）:
    播 FlowMindTwoAttack1.ani（#110，7 帧 560ms，F0-F3 攻击盒帧驱动）
    atk=FlowMindTwoAttack1.atk（etc attack info #75，down/push50/lift0）
    太刀/短剑 → 更广横斩（Attack2/Attack3 档，见 §2.3）；钝器命中 → 追加冲击波 PO（§2.4）
    （mod 重写版实证了 sq_SetCurrentAttackInfo(75) 与攻击子状态号 200——与引擎档吻合）
落地 → 子状态 201（落地段）:
    播 FlowMindTwoLanding.ani（#109，5 帧 300ms）→ 回站街或流心态（explain："不追加操作则回到流心初始状态"）
连携出口（onProcCon_swordman_flowmindtwo 实测）:
    子状态 0/1 中按技能键且技能 109（流心:升）可用 → 切状态 64（升）
    onAfterSetState: 若挂 ap_stateoflimit（技能 248，疑 mod 霸体）→ 施放瞬间霸体 sq_SetSuperArmorUntilTime
```

### 2.3 武器分档（explain + atk 文件对照）

| 武器 | atk（etc attack info，实测） | 反应 |
|---|---|---|
| 通用砍击 | #75 FlowMindTwoAttack1.atk：physic/武器伤害/down/push **50**/lift 0 | 击倒 |
| 二段/追加档 | #76 FlowMindTwoAttack2.atk：同上 push **100** + **[bounce] 1**（弹墙） | 击倒+弹墙 |
| 升空档 | #77 FlowMindTwoAttack3.atk：damage 反应（非击倒）/**lift 200**/[hit info] ×1.3 | **浮空挑起** |

三份 atk 与武器/段位的精确对应关系引擎内置（未考证）；从数值形态推断：Attack3（lift 200）疑为太刀/短剑广域横斩档，Attack2（bounce）疑为二段追击档。

### 2.4 钝器冲击波 PO（flowmindtwobluntsub.obj，实测）

- name `钝器精通冲击波`——复用**钝器精通**体系的判定 PO（非本技能专属资产）；
- `[basic motion]` `Animation/BluntMasterySub/Hit.ani`（4 帧 400ms，**11 个攻击盒标记**——多盒密集判定）；
- `[attack info]` `AttackInfo/FlowMindTwoBluntSub.atk`（PO 侧，实测：physic/down/push 50/lift **100**/blow/no blood 20）；
- `[object destroy condition] on end of animation`（播完即毁）；
- 视觉借**圣职者** `Character\Priest\Effect\ChoppingHammer\bottom.img`（L14 跨职业借图又一实证）。

### 2.5 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| character\swordman\animation\FlowMindTwoJump.ani（#108） | 3 | 300ms | 无 | 无 | 跳跃动作（sm_body） |
| character\swordman\animation\FlowMindTwoAttack1.ani（#110） | 7 | 560ms（每帧 80ms） | 无 | **F0-F3** | 砍击判定帧 |
| character\swordman\animation\FlowMindTwoAttack2.ani（#111） | 6 | 480ms | F3=65534 | **F0-F2** | 追击/横斩档 |
| character\swordman\animation\FlowMindTwoLanding.ani（#109） | 5 | 300ms | 无 | 无 | 落地收势 |
| passiveobject\...\bluntmasterysub\Hit.ani | 4 | 400ms | 无 | 多盒（11 处标记） | 冲击波判定+视觉 |
| character\swordman\effect\...\flowmindtwo\attack1_normal/dodge 等 ×6 | 未逐帧 | — | 无 | 无 | 砍击弧光（attack1/2_dodge 系 + fallstate_two_air/leap 跳跃弧光） |

`.als` 边车：全部文件均无（两侧 ls 实测）——弧光特效无声明式挂接（064"引擎特效缺源"同档，但此处至少有 effect ani 文件在手，可手组装）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMindTwo.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMindTwo.skl` | ✅ | 技能数据（14 static/4 列） |
| 注册行 | —（引擎内置状态 63） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失 | 连携入口在 swordman_common.nut（mod 改写，§2.1） |
| 钩子 nut | flowmindtwo.nut（混淆近原版） | `…\pvf\sqr\character\swordman\flowmind\flowmindtwo.nut` | ✅（38 行） | 霸体钩子 + 升连携出口 |
| mod 重写 nut | flowmindtwo.nut | `…\pvf\sqr\character\swordman\weaponmaster\flowmind\flowmindtwo.nut` | ✅（202 行，**mod 自创**） | 攻击段子状态 200/201 + atk75 旁证 |
| 流心态 nut | flowmind.nut（61 状态） | `…\pvf\sqr\character\swordman\weaponmaster\flowmind\flowmind.nut` | ✅（43 行，mod 改写） | 流心态入口（本技能前置） |
| appendage | ap_weiyi3.nut（位移）/ ap_liuxing.nut（流心标记） | 同目录 | ✅（18/4 行） | mod 位移与连携标记（原版无） |
| .chr 条目 | etc motion #104-#113（行 1077-1086）；etc attack info #75/76/77（行 1369-1371） | `…\pvf\character\swordman\swordman.chr` | ✅ | 流心系全家动画 + 三档 atk |
| 角色 .ani | FlowMindTwoJump/Attack1/Attack2/Landing.ani | `…\pvf\character\swordman\animation\` | ✅ | §2.5 |
| 角色 .atk | FlowMindTwoAttack1/2/3.atk | `…\pvf\character\swordman\attackinfo\` | ✅ | §2.3 |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失 | — |
| PO 定义 | flowmindtwobluntsub.obj | `…\pvf\passiveobject\character\swordman\flowmindtwobluntsub.obj` | ✅ | 钝器冲击波（§2.4） |
| PO .ani/.atk | bluntmasterysub\Hit.ani + FlowMindTwoBluntSub.atk | `…\pvf\passiveobject\character\swordman\animation\bluntmasterysub\` / `…\attackinfo\` | ✅ | 冲击波判定+视觉+命中 |
| 特效 .ani | flowmindtwo\*.ani ×6（+_ds） | `…\pvf\character\swordman\effect\animation\flowmindtwo\` | ✅ | 弧光/跳跃特效 |
| 装备层 | flowmindtwo*.ani ×304 | `…\pvf\equipment\character\swordman\avatar\` | ✅（find 计数） | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 跳跃/砍击/落地动作图集 | 必需（共享） | ✅ 已在库 |
| Character\Swordman\Effect\FlowMindTwo\attack1_normal.img / attack1_dodge.img | sprite_character_swordman_effect_flowmindtwo.NPK | 砍击弧光 | 可选（视觉增强） | ❌ |
| Character\Swordman\Effect\FlowMindTwo\attack2_dodge1.img / attack2_dodge2.img | 同上 | 横斩档弧光 | 可选 | ❌ |
| Character\Swordman\Effect\FlowMindTwo\fallstate_two_air.img / fallstate_two_leap.img | 同上 | 跳跃弧光 | 可选 | ❌ |
| Character\Priest\Effect\ChoppingHammer\bottom.img | sprite_character_priest_effect_choppinghammer.NPK | 钝器冲击波视觉（L14 跨职业） | 可选（做钝器档才需要） | ❌ |

缺失 img：必需级 **0 张**（角色动作全在已入库的 sm_body；L16 又一实证）、可选级 6 张（2 个 NPK）。

## 5. 实现方案草案（🔶 深简化：独立施放 + 水平化跳跃）

- **内容件清单**（全部继承真实基类）：
  - `FlowMindTwoSkill : SkillLogic`（三段子状态机，同 ReleaseWaveSkill 位移 + NormalAttack 帧驱动混合范式）：
    - `CooldownMs=6500`（col1 Lv1 原值直用）、`TotalTimeMs=1160`（300 跳 + 560 砍窗口 + 300 落地的固定上限；**条件收段依赖 ctx.EndCast()（缺失档，见 §7）**——demo 用固定时间轴兜底，手感=不砍也站满窗口）。
    - OnCast（SubState=0 跳跃段）：`ctx.PlayAnim(AnimId.SwordmanFlowMindTwoJump)` + `ctx.ClearHitTargets()`；OnUpdate 前 300ms 内 `ctx.MoveCasterForward(纯函数插值)` 前移 1.2 单位（DNF 水平 300px/300ms 折算；**z 轴弧线降级为纯水平**，§7）。
    - 跳跃段后 560ms 砍击窗口（SubState=1）：`ctx.PeekBufferedButton()==攻击键` → `ctx.ConsumeBuffer()` + `ctx.PlayAnim(AnimId.SwordmanFlowMindTwoAttack1)` + `ctx.SetSubState(2)`——砍击判定**零代码**：Attack1.ani json 自带 F0-F3 攻击盒，帧驱动自动激活（L19/§2.1 已有先例，判定帧表加该 AnimId 即可）。
    - 末 300ms（SubState=3 落地段）：`ctx.PlayAnim(AnimId.SwordmanFlowMindTwoLanding)`；OnEnd → `ctx.PlayDefaultAnim()`。
    - `HitReaction{Damage=100, HitstunMs=600, KnockbackX=50, LaunchY=0}`（FlowMindTwoAttack1.atk 原值 push50/down → demo 值）。
  - 钝器冲击波（可选二期）：`FlowMindTwoBluntArea : AreaDefinition`——砍击命中帧 `ctx.CreateAreaInFront(AreaIds.Xxx, 1.5)`，`TotalTimeMs=400`、`EnterActions={MeleeHit}`、`HitReaction{Damage=60, HitstunMs=600, KnockbackX=50, LaunchY=100}`（BluntSub.atk 原值）、`ViewAnimId=AnimId.FlowMindTwoBluntShock`。
- **概念映射**：引擎状态 63 子状态机 → LSCast.SubState 三段；static 跳跃物理 → MoveCasterForward 纯函数（z 缺）；X 追加输入 → PeekBufferedButton/ConsumeBuffer（已落地）；F0-F3 帧判定 → json attackBoxes 帧驱动（已落地）；冲击波 PO → AreaDefinition（L9 映射）。
- **注册点**：SkillIds.FlowMindTwo=17、AnimIds 67-70（Jump/Attack1/Landing/BluntShock，71-72 留特效 overlay）、json ×4（三个角色 ani + Hit.ani）、判定帧表 `LSHitboxComponentSystem` 加 FlowMindTwoAttack1、按键 ButtonToSkill case 9。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | col1：6500ms（Lv1）→ 0（Lv70+） | 6500 固定 |
| 跳跃 | static：水平 300/上升 200/重力 -10000（推断） | 水平 1.2 单位/300ms，z 轴不做 |
| 砍击判定 | F0-F3（80ms/帧） | 帧驱动直用 |
| 砍击伤害 | col0 755%（Lv1） | 100 固定 |
| 砍击反应 | atk1：down/push50/lift0 | 伤害 100/硬直 600/Kb 50 |
| 落地 | 5 帧 300ms | 300 直用 |
| 冲击波（钝器） | BluntSub.atk：down/push50/lift100 | 伤害 60/Kb 50/Ly 100 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| FlowMindTwoJump/Attack1/Attack2/Landing.ani、bluntmasterysub\Hit.ani | 节面常规（FRAME/IMAGE/DELAY/ATTACK BOX 多盒/DAMAGE BOX） | **现有 ani 子命令全覆盖**（多盒已支持） |
| FlowMindTwo.skl | `.skl` 无子命令（14 列 static + 4 列 level info——static 列数本批最多） | 并入既有 `.skl` 缺口 |
| FlowMindTwoAttack1/2/3.atk + BluntSub.atk（4 个） | `.atk` 无子命令 | ���入既有缺口；本技能手抄 4×~8 值可接受 |
| Attack2.ani F3 flag 65534 | SET FLAG 按既有约定跳过（帧号 const 进技能类） | 非缺口 |

计 2 条既有缺口（.skl/.atk），无新节、无 .als、无 .ptl。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 跳跃 z 轴抛物线（上升 200/重力 -10000） | **跳跃系统**（缺失档，R1-A2 记档；LSFlightComponent 有 z 物理原语但为受击侧，施法者主动跳跃无入口） | 水平位移近似 + 跳跃动画（300ms 短弧视觉损失小）；若立项跳跃系统则本技能是首批受益者 |
| 空中方向键 Y 轴移动 | **技能中方向输入读取**（缺失档，R1-A3 记档） | 跳跃方向=施放时朝向固定 |
| 流心态（105）中 C 键起手 + 落地回流心态 | **技能取消/连招体系**（缺失档，064 首报）+ 流心态本身未实现 | 独立技能直发（绕过连携入口）；落地回站街 |
| 跃→升（109）连携出口 | 同上 + 技能 109 未实现 | 跳过（109 另行分析） |
| 武器四档分野（太刀/短剑广斩、钝器冲击波、弹墙追击） | 无武器类型系统（换装/武器切换缺失档） | demo 单档（通用砍击）；钝器冲击波做可选二期 Area |
| 施放瞬间霸体（skill 248 联动） | 霸体帧（延后档） | 跳过 |
| 条件收段（不砍则提前结束） | **ctx.EndCast() 自结束门面**（缺失档，R1-A3 记档——TotalTimeMs=0 的技能必撞） | 固定时间轴 1160ms 兜底（站满窗口，手感略僵） |
| [bounce] 弹墙（atk2） | 撞墙检测（延后档） | 追击档不做 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 14 值与跳跃物理参数的逐位对应（§1 仅给形态推断）。
2. level property col2/col3 语义（疑追击/冲击波攻击力）。
3. 三份 atk（#75/76/77）与武器/段位的精确对应（§2.3 为数值形态推断）。
4. col1 CD 递减到负值的引擎截断规则。
5. 钝器冲击波的触发时机（命中帧引擎内置，疑砍击命中瞬间）。
6. 近原版钩子 nut 中技能 248（ap_stateoflimit）身份（疑 mod 霸体技）。

**系统级缺口（非新增，实证补充）**
- 跳跃系统（R1-A2）：本技能 + 18（鬼影步无敌窗口）+ 105-109 流心全家均撞——流心系是跳跃/连招两缺口的最大需求方，建议总览优先级评估时把"跳跃系统（施法者 z 物理）"与流心系实现捆绑考量。
- ctx.EndCast()（R1-A3）：条件分支技能的第二实例（三段斩后），缺口清单再+1 实证。

**给下轮的经验**：流心系（105/107/108/109 + TP110）**全部走引擎内置状态 61-64**，load_state 里只有 61 一条 mod 注册；分析时直接看 `swordman_common.nut` 的 procAppend_Flowmind_Comminterrupt（连携中枢，state 号地图全在此）+ `.chr` etc motion #104-#113（动画槽连号）+ etc attack info #73-#78（atk 连号）——三张连号表一次摸清全家。另：`weaponmaster\flowmind\` 目录是 mod 重写区（C3 坑），原版钩子在 `flowmind\` 目录（函数名混淆但结构近原版），两者要分开引用。
