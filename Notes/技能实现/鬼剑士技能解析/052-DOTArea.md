# 杀意波动（DOTArea）

> 技能ID 52 | 级别 B（持续光环） | 可实现性 🔶（周期伤害光环主干 = FireCircleArea 同构直译；光环不跟随施法者/开关判定/暴击增益/攻击力减益四处降级） | 分析日期 2026-08-22 | 批次 B3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 杀意波动 | `skill\Swordman\DOTArea.skl` [name] |
| 英文名 | DOTArea（取 skl 文件名；[name2]="杀意波动" 中文，L1） | 同上 [name2] 实测 |
| 职业 | 阿修罗（[skill fitness growtype]=**4**） | 同上 |
| 学习等级 | 25（**前置：技能 53 Lv1**，[pre required skill] `53 1`——53 的身份未考证，疑地裂系） | 同上 [required level]/[pre required skill] |
| 最高等级 | 30（阿修罗档 20） | 同上 |
| 类型 | active（skill class 1） | 同上 [type]/[skill class] |
| 指令 | →→ + Space（BUFF 键） | 同上 [command] |
| CD | 5000 ms | 同上 [cool time] |
| MP | 20 → 168 + **维持 MP 10.0~60.0/s** | 同上 [consume MP]/[maintain MP] |
| 读条 | casting time 500 ms | 同上 [casting time] |
| CD 机制 | [auto cooltime apply] 0（开关型） | 同上 |
| static data | `50 10`（两值语义未考证——疑 50=对敌有效高度限/10=MP 不足判定相关） | 同上 [static data] |
| 一句话效果 | 开关型：以自身为中心持续喷射杀意波动，周围敌人每秒受无视防御伤害；周围队员物/魔暴击率提升，自身基本/技能攻击力强化；MP 不足或再次施放则中断 | 同上 [explain] |
| 特殊规则 | 过高位置敌人无效；对玩家角色只造成 25% 伤害 | 同上 [explain] |

**level property 模板解码（6 列，L21 向量法全解，Lv1→Lv41 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 每秒波动伤害 | (-1, 0, ×0.1) | col0 = 624→3206 → **62.4 → 320.6**/秒 |
| 波动领域 | (-1, 1, ×1.0) | col1 = **300 → 507 px**（光环半径） |
| 增加物理暴击率 | (-1, 2, ×0.1) | col2 = 5→150 → **0.5% → 15%**（队员） |
| 增加魔法暴击率 | (-1, 3, ×0.1) | col3 = 5→150 → **0.5% → 15%**（队员） |
| 攻击力减少范围 | (-1, 4, ×1.0) | col4 = **300 px 恒定**（敌方减攻范围） |
| 攻击力减少比率 | (-1, 5, ×1.0) | col5 = **5% 恒定** |

## 2. 技能逻辑走读

### 2.1 注册与文件链

**纯引擎内置（比波动刻印更彻底）**——三方实测：

- load_state 无 pushState（grep dotarea 无命中）；swordman_throw.nut 的 case 表（23/47/82/18/222）**无 52 分支**；
- `sqr\character\swordman\` 无 dotarea 目录、appendage 无 ap_dotarea；
- 无被动对象（passiveobject 两侧 ls 无 dotarea）。

→ toggle 判定/周期伤害/暴击光环/攻击力减益全部引擎实现，pvf 只留 .skl 数值 + 视觉特效 ani。

### 2.2 引擎内置行为重建

```
施放（读条 500ms + 增益姿态）：
  toggle 开启（再次施放关闭；MP 不足自动关——维持 MP 10~60/s 持续扣除）
开启期间（光环跟随自身移动）：
  每 1s（"每秒"模板行；精确 tick 未考证）：
    半径 col1（300~507px）内敌人：col0×0.1 无视防御伤害
      —— 无视防御：正好命中我们"伤害=固定值、无防御公式"的现状（天然的语义等价）
      —— z 过高敌人无效（static[0]=50 推断为高度阈值）；对玩家 25%
    半径 col2/col3（队员）：物/魔暴击率 +0.5~15%（引擎属性系统）
    半径 col4（300px）内敌人：攻击力 -5%（引擎 debuff）
  自身：基本攻击力和技能攻击力强化（explain 文本，数值列未见——疑与暴击/减攻共用引擎内部值，未考证）
视觉：
  光环本体与受击表现由引擎按 effect\animation\dotarea*.ani 编排（无 .als 声明、无脚本引用——
  dotareadamage=受击表现 / dust1-5=环境尘 / quake+rock=地面震裂，播放时机为引擎硬编码）
```

### 2.3 被动对象 / appendage

无。伤害无 .atk 文件（角色/PO 两侧 attackinfo grep dotarea 无命中）——命中参数（硬直/无反应）在引擎内部，**未考证**。

### 2.4 动画关键帧表（全部实测）

| 动画 | 帧数 | 总时长 | 循环 | SET FLAG | 攻击盒 | 引用 img | 备注 |
|---|---|---|---|---|---|---|---|
| `character\swordman\effect\animation\dotareadamage.ani` | 19 | 475 ms | ❌ | 无 | 无 | `Character/Swordman/Effect/DOTAreaDamage.img` | 受击表现（主视觉） |
| `dotareadust1-5.ani` ×5 | 5 | 500 ms | ❌ | 无 | 无 | **空（IMAGE 路径空串，L7 占位）** | 含 [INTERPOLATION] |
| `dotareaquake.ani` | 5 | 400 ms | ❌ | 无 | 无 | `Common/CommonEffect/EarthQuakeRing.img` | 震环（L14 跨目录共用） |
| `dotarearock1/2.ani` | 2 | 200 ms | ✅ | 无 | 无 | **空（L7 占位）** | 含 [INTERPOLATION]/[IMAGE RATE] |
| 施法姿态 | summon1/2.ani 共用 | 150/600 ms | — | — | — | sm_body 帧 75-89 | 同全套 buff 族 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DOTArea.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DOTArea.skl` | ✅ 实测 | 数值（6 列全解）+ [skill preloading image] DOTArea.img |
| 注册行 | — | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无 | 引擎内置（F3） |
| 主 nut | —（throw case 表亦无 52） | `…\sqr\character\swordman\swordman_throw.nut` | ⛔ 无 | 纯引擎 |
| appendage | — | `…\sqr\character\swordman\appendage\` | ⛔ 无 | toggle 态在引擎 |
| .chr / 角色 .ani | summon1/2 共用 | `…\pvf\character\swordman\` | ✅ 实测 | 增益姿态 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\` | ⛔ 无 | 引擎内部命中参数未考证 |
| 特效 .ani | dotarea*.ani ×10（+rock2_ds 剑影变体） | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | §2.4 |
| 被动对象 | — | `…\pvf\passiveobject\character\swordman\` | ⛔ 无 | — |
| .als | — | 两侧 animation 目录 | ⛔ 无 | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法姿态 | 必需 | ✅ 已在库 |
| `Character/Swordman/Effect/DOTAreaDamage.img` | sprite_character_swordman_effect.NPK（Effect 根，下划线化规则） | 受击表现主视觉 | 必需 | ❌ |
| `Common/CommonEffect/EarthQuakeRing.img` | sprite_common_commoneffect.NPK（L14：img 完整路径推导，跨目录共用） | 震环 | 可选 | ❌ |
| （dotareadust1-5/rock1-2 为空占位，无 img 需求；skl 预载清单的 DOTArea.img——目录名与预载名不完全一致，**该 img 在 .ani 引用中未出现**，疑旧版图标/贴图，存疑跳过） | — | — | 存疑 | — |

缺失 img：必需级 1 张、可选 1 张。

## 5. 实现方案草案

**🔶 简化实现（伤害光环主干零新机制）**：

- **`DOTAreaSkill : SkillLogic`**（同 FireCircleSkill 范式）——CD 5000、TotalTimeMs=600；OnCast：`ctx.PlayAnim(增益姿态)` + `ctx.CreateArea(AreaIds.DOTAreaAura, ctx 施法者位置)`（自身中心，不用 InFront）。**开关语义降级为固定时长**（见 §7）。
- **`DOTAreaAura : AreaDefinition`**（复制 FireCircleArea 改）：
  - `TotalTimeMs = 10000`（demo 固定 10s 光环替代 toggle；DNF 无时长上限、靠关）
  - `TickTimeMs = 1000`、`TickActions = {MeleeHit}`（每秒对区域内敌人结算）
  - `HalfExtents = (3.0, 0.5, 3.0)`（col1=300px → 3.0 单位）
  - `HitReaction { Damage = 25, HitstunMs = 0, KnockbackX = 0, LaunchY = 0 }`（纯 tick 伤害无硬直无击退——DNF 杀意 tick 不打断；引擎版硬直未考证，demo 取 0）
  - `ViewAnimId = AnimId.DotAreaDamage`（475ms 不循环 → 视图层循环播放近似 aura 闪动；或把 dotareadamage 当 tick 表现每秒重播）
- **位置语义**：DNF 光环跟随施法者移动；Area 是静态点 → **demo 简化为"原地驻场光环"**（站桩输出场景近似），或按 R4-A17"逐拍建区"过渡范式（每 500ms 在新位置重建短命区，多 Area 叠 GC 压力，实现期二选一）。跟随语义的正式解 = Area 跟随施法者缺口（R4-A17 已记档）。
- 注册点：`SkillIds.DOTArea = 29`、`AreaIds.DOTAreaAura = 30`、`AnimIds.DotAreaDamage = 147`（ quake=148 可选）。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 5000 ms | 5000 直用 |
| 光环半径 | 300 → 507 px | 3.0 单位（Lv1） |
| 每秒伤害 | 62.4 → 320.6（无视防御） | 25/秒（固定值天然"无视防御"） |
| tick 间隔 | 1 s（推断） | 1000 ms |
| 持续 | 无限（toggle） | 10000 ms（降级：自动结束） |
| 队员暴击 | +0.5~15% | 跳过（无暴击系统） |
| 敌方减攻 | -5%（300px 内） | 跳过（无消费链） |
| 读条/维持 MP | 500ms / 10-60 每秒 | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `DOTArea.skl` | `.skl` 无子命令（6 列 + static 2 值 + [skill preloading image] 节） | 手抄可行；`skl` 子命令同前议（预载清单建议工具忽略+记档，025 同议） |
| dotareadamage/quake.ani | 常规节，无缺口 | `ani` 直译 |
| dotareadust1-5 / rock1-2.ani | **[INTERPOLATION]**（规则表外，R3-A13 已记档）+ [IMAGE RATE]（rock，延后档） | 空占位 ani（L7）——直接跳过不翻译，无视觉损失；INTERPOLATION 缺口记档即可 |

结论：实质翻译缺口 `.skl` 子命令 + [INTERPOLATION]（重复印证，且本技能的占位 ani 可整体跳过、不构成阻塞）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 光环跟随施法者移动 | **缺失档：Area 跟随施法者**（R4-A17 双实证） | 原地驻场 or 逐拍建区（§5）；跟随版等缺口 |
| toggle（再按关/MP 不足自动关） | **缺失档：自身 Buff 查询门面**（R4-A18）+ MP 系统（延后） | 固定时长 10s 自动结束 |
| 队员物/魔暴击 +0.5~15% | 缺失档：暴击系统（无暴击数值键与暴击公式） | 跳过 |
| 敌方攻击力 -5% | 缺失档：属性数值消费链（R1-A4） | 跳过 |
| 每秒无视防御伤害 | **无缺口**：TickActions=MeleeHit + 固定 Damage 天然无视防御 | 直译（主干 ✅） |
| z 过高无效 / 对玩家 25% | 无 z 判定门面 / 无阵营区分 | 跳过（demo 全额全高） |
| 维持 MP 10~60/s | 延后档 | 跳过 |
| dust/rock 占位视觉 | L7 空占位 | 跳过（引擎原本也无贴图） |

## 8. 存疑与缺口上报

**未考证项**
1. static data `50 10` 两值语义（高度限/MP 判定推断）。
2. tick 的精确间隔与命中参数（引擎内部无 .atk——硬直/受击表现参数未考证）。
3. "自身基本/技能攻击力强化"的数值来源（explain 有、level property 无对应列——疑引擎固定或与 col2/3 挂钩）。
4. 前置技能 53 的身份。
5. skl 预载清单 `DOTArea.img` 与 .ani 实际引用 `DOTAreaDamage.img` 的关系（疑旧版残留）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **暴击系统**（首次撞上）：杀意波动/阿修罗多个光环都有暴击率增减——需要暴击数值键 + 命中判定公式（暴击倍率）。与属性消费链同族但多一层"判定公式"，建议并入数值链立项设计。
2. **光环跟随（Area 跟随施法者）第 3 实证**（R4-A17 双实证后）——本技能是最典型的"贴身移动光环"，可作该缺口立项的验收用例。

**翻译工具缺口**：`.skl` 子命令 + [skill preloading image] 节（重复印证）；[INTERPOLATION]（重复印证，占位 ani 可跳过）。

**给下轮的经验**：**toggle 型技能的引擎化程度看 swordman_throw.nut case 表**——有 case（47/82/18）= 脚本层 toggle；无 case（52 杀意）= 纯引擎 toggle。杀意波动与波动刻印同指令同职业同期，结构却差一层，判定时别混。空占位 ani（dust/rock）+ INTERPOLATION 组合 = 引擎粒子锚点，一律跳过不翻译。
