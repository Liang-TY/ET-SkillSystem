# R4-L：DNF 装备系统整体架构认知（研究原始笔记）

> 第4轮（补充轮）Agent L 原始笔记。任务：装备数据组织/属性管线/武器类型×战斗/套装时装耐久强化。
> 已综合进：12-装备系统整体架构认知.md

---

**总架构一句话**：装备系统是"**明文数据表（.equ）+ 引擎原生 DSL + 少量 nut 脚本**"的三层结构——装备的静态属性、外观绑定、条件特效全部声明在 `pvf\equipment\` 下的 `.equ` 文件里，由引擎（客户端/服务器 C++）直接解析生效；只有"武器类型×技能分支""精通技能图形/buff""快捷栏消耗品"这类需要表现逻辑的部分下沉到 `sqr\` 的 nut 脚本。**装备的穿上/脱下没有客户端脚本事件，静态加成不走 nut**。

---

## 一、装备数据的组织

### 1.1 目录结构（按"职业 → 部位 → 材质/类型"三级）

```
pvf\equipment\
├─ equipment.lst                    装备总注册表（20.7万行 ≈ 5万+件，格式：ID\n`相对路径.equ`）
├─ equipmentnumbersection.txt       ID 分段说明（韩服 1~65535 / 日服 65536~75536）
├─ equipment.kor.str                名称字符串表（22.6万行）
├─ oldequipmentstatinfolist.dat     旧版装备属性表（明文，ID+33列数值）
├─ pricetable.tbl                   价格表
├─ character\                       按职业组织（大头）
│   ├─ common\<部位>\<材质>\         全职业通用防具/首饰：jacket/shoulder/pants/shoes/waist/
│   │                               amulet(项链)/wrist(手镯)/ring/support(辅助)/magicstone(魔法石)/title
│   │   └─ 材质子目录：cloth(布)/leather(皮)/larmor(轻)/harmor(重)/plate(板)
│   ├─ swordman\weapon\<武器类型>\   武器：ssword(短剑)/katana(太刀)/club(钝器)/hsword(巨剑)/beamsword(光剑)
│   ├─ swordman\avatar\<部位>\       时装：cap/hair/face/neck/coat/pants/belt/shoes/skin/aura/weapon
│   ├─ swordman\at_avatar\           觉醒后限定时装
│   ├─ swordman\growtype\            职业觉醒强制外观件（asura.equ 波动印、berserker.equ 红眼+眼特效）
│   ├─ <job>.lay                     该职业的"装备动画脚本"（外观层绑定表，见 1.3）
│   └─ partset / partset2 / partset3 套装（时装套装）定义，681 个文件
├─ creature\artifact_red/blue/green/n 宠物装备（宝珠）
├─ addition\                         武器/皮肤外观附加（染色武器外观 46 件）
└─ weapon\23summer_weapon 等         活动武器
```

### 1.2 .equ 文件格式：全部明文 `#PVF_File`，Tab 缩进 + 反引号字符串，无二进制

抽样精读：短剑 `101000001.equ`、史诗短剑 `ssword_27155.equ`（战争英雄之无双短剑）、布甲上衣 `100050017.equ`、板甲上衣 `amr_dawnsevenstar.equ`（黎明七星）、称号 `100330003.equ`、腰部装饰 `etc\bravery.equ`、时装上衣 `avatar\coat\101500005.equ`、光环时装 `avatar\aura\101590001.equ`、宠物宝珠 `artifact_red`。

**字段全集（按职能分组）**：

| 职能 | 字段 | 示例/说明 |
|---|---|---|
| 身份 | `[name]` `[name2]` `[flavor text]` `[basic explain]` `[explain]` | 名称/英文名/背景故事/说明 |
| 分类 | `[equipment type]` `` `[weapon]` 23 `` | 槽位字符串+数字分类号。字符串：weapon/coat/shoulder/pants/shoes/waist/amulet/wrist/ring/support/magicstone/title/`[coat avatar]`/`[aurora avatar]`/`[artifact red]`…（对应 sqr 的 ENUM_EQUIPMENTTYPE_*）；后缀数字是另一套物品分类号（武器 23/24、上衣 17/21，用于仓库/拍卖分组，语义未完全确证） |
| 子类型 | `[sub type]` | 防具=材质（0布/1皮/2轻/3重/4板，与精通匹配）；武器留 0 |
| 品质 | `[grade]`(等级) `[rarity]`(稀有度：1普通/2稀有/3史诗级) `[icon mark]`(品质角标) `[random option]`(魔法封印随机属性开关) | |
| 穿戴条件 | `[usable job]`…（`[all]`/`[swordman]`…）、`[minimum level]` | 职业与等级门槛；武器系别门槛另由"掌握"技能管（见三.4） |
| 基础属性 | `[equipment physical attack] 621 574`、`[equipment magical attack]`、`[separate attack]`(独立攻击)、`[equipment physical defense] 900 876`、`[physical/magical attack]`(称号/光环用面板值)、`[cast speed]` `[stuck]`(僵直) `[HP MAX]` `[all elemental resistance]` 等 | **双列数值**：比例一致——推断为品级(最上/最下)随机区间上下限（缺口未确证） |
| 技能加成 | `[skill levelup]` `` `[swordman]` 25 2 `` …`[/skill levelup]` | 职业+技能ID+加等级，引擎计入 sq_GetSkillLevel |
| 经济 | `[price]` `[repair price]` `[value]` `[weight]` `[creation rate]` `[need material]` | 售价/修理费/分解值/重量(负重)/合成率/合成材料 |
| 耐久 | `[durability] 35` | 见四 |
| 交易限制 | `[attach type]`(`[sealing]`可封装/`[trade]`)、`[impossible contents]`、`[no random]`、`[possible kiri protect]`(可强化保护) | |
| **外观绑定** | `[animation job]` + `[variation] 层号 0` + `[layer variation]` `层号` `` `sswdc` `` + `[equipment ani script]` `` `equipment/character/swordman.lay` `` | **按职业重复一组**：每个职业指定换装图层号(layer)与图层资源名，`*.lay` 是角色基础动作→动画文件映射表。多职业共用的史诗写三组 |
| 图标 | `[icon]` `[field image]` `[custom animation]` `[aura hud icon]` | |
| 音效 | `[move wav]`(`MINERALSWD_TOUCH`拾取音) | |
| 组归类 | `[item group name]` `` `ssword` `` / `cl coat` | 同族物品分组（"所有短剑"） |
| **条件特效 DSL** | `[if]…[/if]` + `[then]…[/then]`（详见二.2） | |
| 套装 | `[set name]` `[set item]`(全套装成员ID列表) `[set ability]`(套装属性) | 详见二.4 |
| 时装专有 | `[part set index]`(套装索引) `[enable dye]`(染色) `[avatar type select]`(可选属性菜单) `[avatar select ability]`(可选技能+1列表) `[emblem socket default]`(徽章槽 `[M socket]`) | |
| 光环时装专有 | `[aurora graphic effects]`(前/后层 ani) `[aura ability]` `[room list move speed rate]` | |

### 1.3 `.lay` 装备动画脚本
`equipment\character\swordman.lay`：`[LAYER 0]` 起头，内容为 `[waiting motion] %[job]%s/Stay.ani` 等——把"职业目录名(%s)"映射到各基础动作动画。装备件通过 `[equipment ani script]` 引用它，配合 `[layer variation]` 的图层号把武器/防具外观合到角色渲染层上。**这是"装备=可换图层"外观系统的核心**。

---

## 二、装备属性怎么作用到角色（属性���线）

装备影响角色共 **5 条管线**，从"纯引擎直读"到"脚本驱动"：

### 2.1 静态属性 → 引擎直读（不走 nut、不走 appendage）
`[equipment physical attack]`、`[HP MAX]` 等面板字段以及 `[skill levelup]`、`[weight]`、`[durability]` 由引擎在穿装备时汇总进角色属性。证据：`sqr\` 全目录 grep 不到任何 getEquip*/isEquip 类 API（language.dof.character.md 也只有 `getWeaponSubType()/setCarryWeapon()/isCarryWeapon()` 三个武器相关接口）——**nut 层根本拿不到"装备对象"，只能看到武器子类型和最终数值**。脚本侧修改装备攻击力用 `CHANGE_STATUS_TYPE_EQUIPMENT_PHYSICAL_ATTACK`（dnf_enum_header.nut:1028）这类状态通道，佐证装备属性在引擎的状态系统里是一等公民。

### 2.2 条件特效 → `.equ` 内嵌 `[if]/[then]` DSL（引擎原生解释，非 nut）★重要发现
装备特效直接写在 .equ 里，条件动词有：
- `[party count] = N`、`[change status] hp > %75 end`、`[attack success]`、`[event attack success]`、`[my appendage] 38`、`[party death]`、`[cooltime] 30000`、`[time]`、`[stat change]`、`[module] [dungeon type]…[/module]`
- 动作有：`[stat by condition]`(条件属性)、`[increase damage] %30`、`[add absolute damage]`(附加伤害)、`[consume item]`、`[restore] hp %10`、`[appendage] 38`、`[speech on]`(台词气泡)、`[equipment duration]`

实例：
- 称号 `100330003.equ`：攻击 2% 几率 +30 力量 20 秒（`[attack success]`+`[probability] 2`+`[stat]`）。
- 史诗短剑 `sswd_27155.equ`：按队伍人数 1~4 人给 +15~60 双攻（4 组 party count 分支）；队友死亡→`[appendage] 38`（变红+30%攻击，10 秒）+台词；HP<10% 且冷却 60s→消耗灵魂晶石(3187)回 10% HP。

### 2.3 复杂/持续特效 → `[appendage N]` → `appendage.lst` → `.apd`
`.equ` 里的 `[appendage] 38` 指向 `appendage\appendage.lst` 第 38 号 = `equipment/changeWeaponColor/38.apd`。`pvf\appendage\equipment\` 下 356 个 .apd，内容是 `[type] change status` + `[string data]/[int data]/[float data]` 的参数化状态包（如"独立攻击 +50%"）。另有 426 条 equipment 路径注册在 appendage.lst。**简单状态→.apd 参数化；需要逻辑→nut 版 appendage（sq_AddFunctionName + sq_AddChangeStatus），后者是技能/装备共用的同一套 appendage 机制**。

### 2.4 套装（set）效果：数据在每件成员上冗余声明
- 防具套装（邪神之怒 `100080004.equ`）：每件都带完整 `[set name]`+`[set item]` 5件成员ID+`[set ability]`。套装属性内可再嵌 `[fullset basic explain]` 和整段 `[if]/[then]`（如"HP75%以上时附加20%伤害"）以及纯数值。触发=引擎统计已穿成员数（pvf 内只见整套 5/5 生效写法；2/3/5 分段档位未见——缺口）。
- 时装套装：avatar 件带 `[part set index]`，定义在 `character\partset*\`（681 个文件），格式 `[piece set ability] 1 [additional effect index] 198`——指向 `etc\additionaleffectlist.etc` 第 198 号=**走路脚印特效 ani**。即时装成套=纯视觉奖励。

### 2.5 精通技能管线（armor mastery / weapon mastery）
- **防具精通**（`skill\swordman\armormasteryheavy.skl` 等 9 个）：passive，`[skill fitness growtype] 3` 与防具 `[sub type]`（3=重甲）匹配——**引擎按"身上该材质件数"逐件放大精通收益**，纯引擎无脚本。
- **武器精通**（`shortswordmastery.skl` 等 5 个 + `weaponmasteryup.skl`）：passive，`[level info]` 每级一行多列数值（物攻%/魔攻%/命中%/各技能特化%）。**特化逻辑在 sqr 脚本里以"技能等级>0 且武器子类型匹配"为条件挂 appendage**，例：`passive_skill_swordman.nut` case 78——`sq_getGrowType(obj)==0/5 && obj.getWeaponSubType()==1`（太刀）时挂 `ap_blademastery.nut`，写 CHANGE_STATUS_TYPE_EQUIPMENT_PHYSICAL_ATTACK 加成与 stuck 减少；换武器/职业不符则 sq_RemoveAppendage。
- **换装响应是轮询不是事件**��`mysticequip\mysticequip.nut` 的 proc_appendage 每帧比对 `appendage.getVar("weapon").get_vector(0) != parentObj.getWeaponSubType()`，变化则重新挂 buff——DNF 用"appendage proc 轮询武器子类型"实现换装检测。

### 2.6 `sqr\equipment\*.nut` 的真实职责（不是装备属性）
`equipment_main.nut` 只是装载 11 个职业文件。每职业文件三个函数：
- `isUsableItem_<Job>(obj, itemIndex)`：快捷栏物品可用性；
- `drawMainCustomUI_<Job>`：自定义 UI 绘制；
- `procAppend_<Job>`：每帧跑——流心派生驱动 + **检查快捷栏里 100000101~108 号消耗品是否在槽，动态挂/摘药剂类 buff appendage**。
即：该目录是"物品/消耗品相关的职业脚本"，与装备静态属性无关。**知识库 17 篇 grep 不到任何 onEquip/onUnEquip 装备事件——穿脱装备无客户端脚本钩子**（缺口/引擎侧）。

---

## 三、武器类型系统（对战斗影响最大）

### 3.1 类型定义在哪
- **目录层**：`equipment\character\<job>\weapon\{ssword, katana, club, hsword, beamsword}`（鬼剑五系）+ newweapon。
- **枚举层**：`dnf_enum_header.nut` 只有非鬼剑职业的子类型枚举（格斗 KNUCKLE..TONFA 0-5、圣职 CROSS/ROSARY/TOTEM/SCYTHE/BATTLE_AXE 0-4），说明子类型**按职业从 0 重编号**；鬼剑子类型由 `.chr` 内表顺序推出：**0短剑 1太刀 2钝器 3巨剑 4光剑**（佐证：skill 78"剑影太刀精通"配 `getWeaponSubType()==1`；`.chr` 音效块顺序 MINERALSWD/KATANA/STICK/SQUARESWD/(空)/BEAMSWD）。
- **使用门槛**：`.equ [usable job]` 管职业；武器系别门槛是**技能**——`EquipLightSword.skl`（"光剑掌握"，skill 33，passive）"可以使用光剑系武器"。

### 3.2 `.chr` 内的 per-武器类型数值表（`swordman.chr` 尾部）
按武器子类型索引的平行表，**武器类型对战斗的"数据面"全在这**：
- `[etc motion]` 里的 `WeaponComboShort1-3 / Blade1-4 / Blunt1-3 / Heavy1-2 / Light1-3.ani` → 各武器普攻段数与动画（太刀4段、巨剑2段、光剑3段…），配 `[etc attack info]` 里同名 `.atk`；
- `[weapon wav]`×6 块：每系挥砍/命中音效；
- `[weapon hit info]`：每系打击表现（`[cut][blood]` vs 钝器 `[blow][no blood]`、血量倍率、喷射参数）；
- `[weapon skill info]` 24 列系数、`[upgrade weapon attack power rate]` 12 列（**强化等级→武器攻击力倍率**）；
- `[weapon durability decrease rate] 1.0 0.9 1.05 0.9 1.0 1.0`：**各武器系别的耐久消耗倍率**（巨剑 1.05 磨损快）。

### 3.3 "武器类型 × 技能分支"编程模式（4 实例归纳）
所有分支点都是 `obj.getWeaponSubType()`（唯一入口），三种用法：
1. **动画/攻击数据替换**：`attack\attack.nut` `weaponcombo_*`——`switch(weapon){ case 2: switch(段数){ case 2: sq_SetCurrentAnimation(37); sq_SetCurrentAttackInfo(36); …} }`。`jg_swordman_common.nut` 的 `getAttackAni_Swordman/getDashAttackAni_Swordman/...` 整族 getter 同模式。
2. **行为分支**：`badao\badao.nut`（拔刀斩）——`getWeaponSubType()==2`（钝器）时 onBeforeAttack 对可抓取敌人 `sq_HoldAndDelayDie` 强控 + onAfterSetState 加 upForce=200 浮空；`illusionslash.nut` 同样 `==2` 分支。
3. **状态参数传递**：`meteorsword.nut`/`swordofmind.nut`——`obj.sq_IntVectPush(obj.getWeaponSubType())` 把武器类型作为 state 入参，状态机按它选分支（技能动画按武器播放）。
4. **武器变化侦测**：mysticequip appendage 轮询比对（见 2.5）。

**模式总结**：武器类型 = 0..N 整数枚举（getWeaponSubType() 唯一入口），数据面在 `.chr` 平行表按索引对齐；行为面在技能 nut 里 `if (getWeaponSubType()==K)` 散点分支；精通被动负责"持对应武器时挂数值 buff"。

### 3.4 武器精通技能机制
- vanilla：skill 12短剑/13太刀/14巨剑/15钝器精通，光剑无精通（光剑掌握 33 只是使用许可）。攻击加成走脚本 appendage，特化效果走各技能 nut 里的等级判断（短剑精通 Lv3 上挑追加攻击、Lv5 格挡冲击波）。
- 本 PVF（私服魔改）新增：`SwordGhost28.skl`＝"剑影太刀精通"(skill 78)、`SwordGhost4.skl`＝"鬼人化"(skill 123)。**修正前几轮结论**：`isSwordSaber(obj)` 实际检查的是 `sq_GetSkillLevel(obj,123)`＝**鬼人化**（函数名是遗留误导，旧版应指短剑 saber），它切换整套 BladeSpirit 普攻动画/攻击框/取消帧数。

---

## 四、其他子系统概览

**时装/avatar**：外观槽独立于属性槽（enum：AVATAR_HEADGEAR 0~SKIN 8、AURORA 9，10 个外观位）。时装件含：染色开关、`[avatar type select]`（穿时选属性）、`[avatar select ability]`（上衣可选某技能+1）、徽章镶嵌槽（槽型按部位由 `etc\chn_equipmentsockettypelist.etc` 规定：上衣 C×2、头肩 B×2、鞋 D×2…）。角色本体皮肤路径在 `.chr [body image path]`。觉醒外观件（growtype 目录）以隐藏 avatar 槽形式强制穿戴。

**光环 aura（两个不同的"光环"）**：
1. **光环时装槽**（aurora avatar）：纯外观+徽章槽+少量面板值，前后两层 ani。
2. **pvf\aura\ 玩法光环**：`.ora` 文件（aura.lst 注册），定义 `[range] 250`、`[duration]`、周期事件 `[event time] first/repeat/update`、`[target] monster/ally`、`[event] appendage 102`（周期性给范围内目标挂状态）、粒子/地面圈特效。来源如"70级史诗板甲套装"——装备赋予的范围光环（debuff 场）。

**耐久度**：四方协作——`.equ [durability]`（上限）+ `[repair price]`；`.chr [weapon durability decrease rate]`（按武器系别倍率）；`.skl [durability decrease rate]`（**1332 个技能文件都有**，该技能使用时武器耐久消耗倍率，如 blache.skl=200）；史诗特效里 `[equipment duration]`（挂 buff 持续时间，另一义）。消费方均为引擎，pvf 内无修理脚本。

**强化/增幅/随机属性**：
- **强化（Kiri）**：数值表在 `etc\upgrade.etc`（每级成功率/费用/材料 3037 无色小晶块/掉级规则）+ `upgrade_separate.etc`；`.equ [possible kiri protect]`；`.chr [upgrade weapon attack power rate]` 折算攻击力。
- **增幅（红字）**：`etc\amplifyitem.etc`（option 映射、按稀有度权重 common 2.0~epic 3.75、净化材料）+ `amplifyupgrade.etc`。
- **魔法封印随机属性**：`.equ [random option] 1` + `etc\randomoption\`（randomoption.lst 注册 options\RandomOptions_N_效果.etc）。
- **附魔（卡片/宝珠）**：**缺口——pvf 内未找到附魔配置**，推断为服务器侧数据。

**宠物装备**：`equipment\creature\artifact_red|blue|green|n`（红/蓝/绿宝珠），同一套装备 DSL，挂在宠物身上。

---

## 五、缺口清单
1. 穿脱装备的引擎事件、静态属性汇总公式——纯 C++ 侧。
2. `[equipment physical attack]` 双列数值确切语义（推测品级上下限）。
3. 套装 2/3/5 件分段档位配置（pvf 只见整套写法）。
4. 附魔数据——未找到。
5. 徽章具体属性数值表。
6. `[equipment type]` 后缀数字（23/24 等）语义。
7. equipmentnumbersection.txt 为 EUC-KR 乱码。

## 六、关键文件路径清单（相对 pvf\）
- 总表：`equipment\equipment.lst`、`equipment.kor.str`
- 样本：`character\swordman\weapon\ssword\sswd_27155.equ`（史诗+条件DSL）、`character\common\jacket\plate\amr_dawnsevenstar.equ`、`character\common\title\100330003.equ`（称号proc）、`character\swordman\avatar\{coat\101500005,aura\101590001}.equ`
- 外观绑定：`equipment\character\swordman.lay`
- appendage：`appendage\appendage.lst`、`appendage\equipment\*.apd`（356 个）
- 脚本：`sqr\equipment\equipment_{main,swordman}.nut`、`sqr\equipment\ap\*.nut`、`sqr\character\swordman\attack\attack.nut`（weaponcombo）、`badao\badao.nut`（钝器分支）、`mysticequip\mysticequip.nut`（换装轮询）、`passive_skill_swordman.nut`（精通appendage）、`jg_swordman\jg_swordman_common.nut`
- 枚举：`sqr\dnf_enum_header.nut`（ENUM_EQUIPMENTTYPE_* 1136-1170、WEAPON_SUBTYPE_*、CHANGE_STATUS_TYPE_EQUIPMENT_*）
- 职业/武器数据面：`character\swordman\swordman.chr`（尾部 weapon wav/hit info/skill info/durability rate、WeaponCombo 动画表）
- 技能：`skill\swordman\shortswordmastery.skl` 等 5 精通、`armormastery*.skl` 9 个、`EquipLightSword.skl`
- 子系统：`aura\aura.lst` + `aura\equipmentaura\*.ora`、`etc\upgrade.etc`、`etc\amplifyitem.etc`、`etc\randomoption\`、`etc\chn_equipmentsockettypelist.etc`、`equipment\character\partset\`、`equipment\creature\artifact_red\`

**给 2D 格斗网游的可借鉴结论**：DNF 把装备做成"数据驱动的声明式系统"——静态数值/外观层/条件特效全在配置表内由引擎统一解释（一套 `[if]/[then]` 微 DSL 覆盖 90% 装备特效），脚本只在"武器类型改变战斗行为"和"精通 buff"两处介入，且以 `getWeaponSubType()` 单一枚举入口 + `.chr` 平行表 + 脚本散点分支实现武器差异化——"枚举+平行数据表+脚本散点分支"三件套可直接移植。
