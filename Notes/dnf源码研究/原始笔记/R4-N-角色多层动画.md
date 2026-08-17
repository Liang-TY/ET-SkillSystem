# R4-N：DNF 角色序列帧多层合成机制（研究原始笔记）

> 第4轮（补充轮）Agent N 原始笔记（首次运行 API 错误中断后唤醒续跑完成）。
> 任务：角色分层渲染机制（武器/皮肤/上衣/下装各层怎么配、怎么同步、换装怎么换层）。
> 已综合进：09-动画系统整体架构认知.md

---

## 关键文件路径速查
- 角色基础动画：`pvf/character/swordman/animation/*.ani`
- 角色定义：`pvf/character/swordman/swordman.chr`（含 [body image path]）
- 动作映射表：`pvf/equipment/character/swordman.lay`（及 atswordman.lay / dsswordman.lay）
- 装备外观绑定：`pvf/equipment/character/swordman/avatar/<部位>/<id>.equ`
- 武器外观：`pvf/equipment/character/swordman/weapon/<武器系>/<id>.equ`
- 枚举头：`pvf/sqr/dnf_enum_header.nut`

## 一、角色 .ani 每帧多图实证

**结论：每帧只有一个 [IMAGE]，只引用身体皮肤层。武器/衣服的图不在 .ani 里。**

逐帧计数（全部 1 图/帧，且都是 sm_body%04d.img）：
| 文件 | [FRAME MAX] | [IMAGE] 总数 | 帧号范围 |
|---|---|---|---|
| stay.ani | 6 | 6 | 90–95 |
| attack1.ani | 10 | 10 | 0–9 |
| move.ani | 8 | 8 | 180–187 |
| jump.ani | 16 | 16 | — |
| dash.ani | 8 | 8 | — |
| weaponcomboshort1.ani | 6 | 6 | 1–6 |

全 swordman/animation/ 目录图片路径前缀统计：`Character/Swordman/Equipment/Avatar/skin/sm_body` — 2767 次（身体皮肤，唯一的角色层）；没有任何 sm_weapon / sm_coat / wp_ / sswdc 出现在 .ani 里。`[LAYER]` 字段在整个目录的 .ani 中一次都不存在。

**推论：武器层/衣服层的图由引擎按 .equ 的层配置、用同一帧号去各层的 img 文件里取同名帧合成。**

## 二、层定义在哪

### 2.1 .lay 文件结构
`equipment/character/swordman.lay`：
```
[LAYER]
    0
[waiting motion]    `%s/Stay.ani`
[move motion]       `%s/Move.ani`
[attack motion]     `%s/Attack1.ani`  `%s/Attack2.ani`  `%s/Attack3.ani`
[etc motion]        `%s/Guard.ani` ... `%s/WeaponComboShort1.ani` ... （数百个）
```
- [LAYER] 后只有一个值 0，不是多值枚举——整个 .lay 只声明一个动作层。
- .lay 本质 = "状态→.ani 文件"映射表，%s 运行时替换为职业动画目录路径。
- [etc motion] 里按武器系区分普攻连段（WeaponComboShort1-3/Blade1-4/Blunt1-4/Heavy1-2/Light1-3）——不同武器系用不同的身体.ani，但都是身体层动画。

### 2.2 .equ 的 [variation] / [layer variation] 字段语义
以 avatar/coat/10086010.equ（雪人外套）为例：
```
[equipment type]    `[coat avatar]`   0
[animation job]     `[swordman]`
[variation]         185    0
[layer variation]
    2300            `coat_c`
[equipment ani script]   `equipment/character/swordman.lay`
[layer variation]
    1800            `coat_a`
[layer variation]
    500             `coat_d`
[hide equipment]    `[pants avatar]` `[breast avatar]` `[shoes avatar]` ...
[hide layer]    2860 1651 1650 1501 1500 1300 1151 1150 2840 2780 ...
```

| 字段 | 含义 |
|---|---|
| [equipment type] | 装备槽类型（[coat avatar]/[pants avatar]/[weapon] 等） |
| [animation job] | 对哪个职业生效 |
| [variation] N M | N=外观变体号→代入img路径的%04d（coat变体185→coat_a0185.img）；M=子变体（0/1/2，推测染色/等级档） |
| [layer variation] 层号 `资源组名` | 层号=z排序键（小=底层）；资源组名=img文件前缀（如coat_c→coat_c0185.img）。一件装备可声明多个层 |
| [equipment ani script] | 指向.lay动作表（绝大多数都指向swordman.lay；特殊时装可带自定义） |
| [hide equipment] | 穿该件时隐藏哪些装备槽 |
| [hide layer] | 穿该件时隐藏哪些层号 |

皮肤 .equ 特殊：[variation] 0 0，没有 [layer variation]——皮肤是 base 层，路径来自 .chr [body image path]。

### 2.3 层号→部位对应表（从全部 .equ 提取，升序=底→顶）

| 层号 | 资源组名 | 部位 |
|---|---|---|
| 0 | sm_body | 身体皮肤（来自.chr，不在.equ） |
| 20 | hair_f1 | 刘海/前发 |
| 50 | berserker_eye | 狂战觉醒眼瞳光 |
| 100 | face_b | 脸部主层 |
| 290-300 | neck_k/cap_d | 颈饰后/帽后 |
| 400 | hair_d | 后发 |
| 500 | coat_d | 上衣后摆/裙摆 |
| **650** | sswdb/katanab/clubb/lswdb | **武器背后层**（b=back） |
| 670-900 | coat_m/cap_b/hair_b | 帽后/上衣中片/后发 |
| 900-1200 | coat_b/neck_b/belt_b/pants_b/shoes_b | 上衣后背/颈饰/腰带/裤/鞋后片 |
| 1300-1700 | pants_a/shoes_a/belt_a | 裤前/鞋前/腰带前 |
| **1800** | coat_a | **上衣主体**（躯干） |
| 1900-2000 | neck_a/face_c/hair_a | 颈饰前/脸前/前发 |
| 2100-2300 | cap_a/hair_c/coat_c | 帽前/发顶/上衣外层 |
| 2430 | berserker | 觉醒体外覆盖 |
| 2500 | cap_c | 帽顶 |
| **2790** | sswdc/katanac/clubc/lswdc | **武器前层**（c=front/combat） |
| 2791-2792 | beamswdc1/beamswdc2 | 光剑前层（剑身+光束两层） |
| 2850 | coat_f | 二觉上衣覆盖层 |
| 2860 | （疑光环） | 光环/最顶特效 |

层号规律：千位=身体大区（0xxx=身体/脸/后摆/武器背；1xxx=腿/腰/鞋；2xxx=躯干/发/帽/武器前/光环）；b后缀=背后层(~650)；c后缀=前面层(~2790)；f后缀=前片/装饰/二觉附加层(z更高)。

### 2.4 时装完全同一机制
所有可见外观件都是 [xx avatar] 类型：avatar/coat→[coat avatar]、avatar/pants→[pants avatar]…avatar/aura→光环。ENUM_EQUIPMENTTYPE_AVATAR_*（0=HEADGEAR~10=WEAPON）确认槽位枚举。**现代DNF属性防具不可见，外观完全由avatar决定。**

## 三、层间同步机制

**结论：不是"每层一份.ani"，而是引擎按同一帧号对齐取图。**

证据链：
1. .ani 每帧只有1个[IMAGE]，没有武器/衣服的图，也没有[LAYER]字段。
2. 所有动画共用同一张全局帧表：attack1.ani用帧0-9、stay.ani用90-95、move.ani用180-187——每个part img（sm_body/coat_a/sswdc…）都是同一张巨型精灵图集，帧号全局统一。
3. 知识库API证实：animation.setCurrentFrameWithChildLayer(frameIndex)——"设置当前帧（包含子层）"。动画对象有"子层"，设帧时所有子层同步到同一帧号。另有 addLayerAnimation/sq_AniLayerListSize/sq_getAniLayerListObject/removeLayerAnimation。
4. .equ的[equipment ani script]都指向同一swordman.lay——正常时装复用基础动作表。

机制总结：
- 角色状态机→查.lay/.chr得当前.ani→.ani给出帧号N、延时、判定框
- 身体层(sm_body%04d.img，%04d=皮肤变体号)画帧N
- 每件已装备avatar按[layer variation]注册一个子层：img路径=资源组名+变体号%04d+.img，画其帧N
- 全部子层共享帧号N→完美同步，无需每层一份.ani

## 四、图片资源组织

命名规则：`<部位前缀><变体号:04d>.img`
- 皮肤：sm_body%04d.img（sm=swordman，变体0→sm_body0000.img），路径由.chr [body image path]给出
- 二觉皮肤：sg_body%04d.img（sg=swordman-grown），路径ATEquipment/Avatar/skin/（atswordman.chr）
- 上衣：coat_a/b/c/d/e/f/g/h/m/x + 变体号→如coat_a0185.img、coat_c0185.img（一件外套=多个img=多层）
- 裤/鞋/发/帽/脸/腰带/颈饰同规则
- 二觉前缀加at：atcoat_a/athair_a/atpants_a…
- 武器（五系c=前/b=后）：sswdc/sswdb（短剑）、katanac/katanab（太刀）、clubc/clubb（钝器）、lswdc/lswdb（巨剑）、beamswdc1/beamswdc2/beamswdb1/beamswdb2（光剑两层）
- 变体号=%04d=.equ [variation]第一值。同一款装备的多层共用同一变体号。

## 五、渲染合成顺序

底→顶（层号升序=画家算法：小号先画被覆盖）：
1. 皮肤 sm_body(0) — 最底
2. 眼瞳光/脸/刘海(20-150)
3. 后发/后颈/帽后/上衣后摆(260-600)
4. **武器背后层 weapon_b(650)** — 背挎/收刀态
5. 帽后/上衣后背/后发(700-925)
6. 颈饰/腰带/裤/鞋后片(1000-1200)
7. 裤前/鞋前/腰带前(1300-1700)
8. **上衣主体 coat_a(1800)** + 颈饰前/前发(1800-2000)
9. 帽身/发顶/脸前(2100-2400)
10. 上衣外层 coat_c(2300) — 在发之上（领/兜帽/前襟外片）
11. 觉醒体外覆盖(2430)
12. 帽顶(2500)
13. **武器前层 weapon_c(2790)** — 挥刀在身前
14. 光剑额外前层(2791-2792)
15. 二觉上衣覆盖(2850)
16. 光环/最顶特效(2860)

### 武器前后两层
- b层(~650)=背后/收刀态：在身体后摆之上、腿之后。挥击时该层img对应帧为透明（不画）
- c层(~2790)=身前/挥击态：在所有衣物之上。收刀时该层img对应帧为透明
- 两层每帧都画，靠img内"空帧"切换显示——**不是引擎按状态切层，而是img帧内容本身决定哪层可见**

### .als 的 z 序与此体系无关
.als = animation layer script，作用是给.ani关键帧挂技能特效子动画（[use animation]+[none effect add]），属于"帧事件→特效"系统，不参与角色部件z排序。
ENUM_DRAWLAYER_*（CONTACT/NORMAL/BOTTOM/CLOSEBACK…）是场景级/object级绘制层（整个角色作为场景对象坐哪层），与角色内部avatar部件层号(0-2860)是两套独立系统。

## 六、怪物/PO 是否分层

**不分层。怪物用单层.ani。**
- monster .mob 有 [face image]（怪物头像图标，不是分层精灵）；没有[equipment]、没有[layer variation]、没有任何avatar/部件层字段
- 怪物.ani每帧只有1个[IMAGE]（如Monster/Bantu/BantuAmazones.img+帧号），单层
- PO同理，走passiveobject/自己的.ani，不进avatar层体系

## 缺口/未确认项
1. [variation]第二值M(0/1/2)的确切语义（推测染色/等级/觉醒档，待查）
2. 部分层号未取样到资源名（1450/2600/2860等来自hide layer列表或跨目录混入）
3. 自定义动作表时装的帧布局兼容性（.equ [equipment ani script]允许非标准.lay，未找实例验证）
4. 属性防具是否曾带外观层（现代DNF不可见，未遍历equipment.lst全表）
5. 层号→z是纯数值排序还是引擎有固定查表（疑C++内，社区资料未取到）
6. breast avatar（胸饰ENUM=6）目录下无breast/子目录，该槽是否空置未确认

## 机制全景一句话总结
DNF 角色是**纸娃娃式多层合成**：.ani只驱动身体层的帧序列（每帧1个[IMAGE]，无层字段）；引擎读.equ的[layer variation]（层号+资源组名）+[variation]（变体号→%04d）为每件装备注册一个子层img，**所有子层共享同一帧号**对齐取图（setCurrentFrameWithChildLayer），按层号升序从底(皮肤0)到顶(光环2860)画家算法合成；武器分b/c前后两层靠img内空帧切换显示；怪物不走此体系，单层.ani。
