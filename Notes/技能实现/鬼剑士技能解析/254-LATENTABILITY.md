# 唤醒（LATENTABILITY）

> 技能ID 254 | 级别 C | 可实现性 ⛔（元素属性系统缺失；潜在能力觉醒为系统级功能） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 唤醒 | `LATENTABILITY.skl [name]`（全大写文件名照录） |
| 英文名 | LATENTABILITY（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 全系共通（growtype 0-5；各段上限 1） | skl [skill fitness growtype] / [growtype maximum level] |
| 学习等级 | 1 | skl [required level] |
| 最高等级 | 1 | skl [maximum level] |
| 类型 | passive（skill class 4） | skl [type] / [skill class] |
| 图标 | `Character/LatentAbilityIcon.img` #1/#7（独立图标表——系统功能佐证） | skl [icon] |
| explain / basic explain | **空串**（系统技能不进技能说明） | skl |
| 一句话效果 | （走读归纳）潜在能力觉醒开关：按配置/最高元素攻击激活火/水/暗/光元素系状态 | ap_latentability.nut（§2.2） |
| static data | `0 0 0 0 0`（五开关全 0 = 默认全关） | skl |
| level info | 29 列全 0（level property 29 组空模板对应）——**数据全零**，运行时值来自外部 | skl |

## 2. 技能逻辑走读

### 2.1 注册与文件链

`passive_skill_swordman.nut` case 254（实测，本批两个有脚本的被动之二）：

```
case 254:
    append = "appendage/character/ap_latentability.nut"     // 注意：挂在 sqr\appendage\character\（跨职业公共层）
    if (skill_level > 0):  挂 appendage + set_appendage_latentability(obj, appendage, skill_level)
    else:                  摘除
```

（`set_appendage_latentability` 仅此一处调用，函数定义在白名单内未找到——疑在 sqr 公共层/
引擎注册 nut，**未考证**；不影响机制结论，数值注入本就在 proc 里自取。）
load_state 无 pushState（被动，正常）。

### 2.2 appendage（ap_latentability.nut，全 103 行实测）

```
proc（每帧，内部节流 1500ms 一轮）：
    读 static data 五开关：fire/water/dark/light flag + adjustFlag（sq_GetIntData(obj, 254, 0..4)）
    若 adjustFlag > 0（自动档）：取四元素攻击（CHANGE_STATUS_TYPE_ELEMENT_ATTACK_*）最高者
        → 对应 flag 置 1（允许并列多开）
    逐元素（火→4000 / 水→4001 / 暗→4003 / 光→4002）：
        flag>0 且未挂 → sqx_AppendScriptAppendage(obj, id, 600000)   // 挂 10 分钟
        flag=0 且已挂 → 短时(1ms)重挂覆盖移除
```

即：**本技能自身零数值**——它是一个每 1.5s 自检的"元素觉醒状态调度器"，实际增益在
script appendage 4000-4003（火/水/暗/光系，系统级定义，非本技能资源）。
唤醒的意义 = 玩家点开开关后，引擎按角色元素攻击配置周期性续挂对应觉醒 buff。

### 2.3 机制归纳

```
习得（1 级）→ 挂永久调度 appendage → 每 1.5s：
    手动档：static 五开关（默认全 0 = 无效果）
    自动档：四元素攻击取最高 → 挂对应元素觉醒 script appendage（600s，循环续）
```

这是 DNF "潜在能力/觉醒系统"（Latent Ability）的角色侧入口——属于**成长系统**而非战斗技能：
五大属性面板（力/体/智/精神/四元素）之外的隐藏成长轴。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | LATENTABILITY.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\LATENTABILITY.skl` | ✅ | 全零数据 + 五开关 static |
| 被动注册 | passive_skill_swordman.nut case 254 | `…\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | 挂载入口 |
| appendage | ap_latentability.nut | `…\pvf\sqr\appendage\character\ap_latentability.nut`（注册行直接指向，白名单外定点读） | ✅ 实测 103 行 | 元素觉醒调度器（全部逻辑） |
| header 常量 | SKILL_LATENTABILITY <- 254 | `…\sqr\character\swordman\swordman_header.nut:114` | ✅ | ID 对号 |
| script appendage | id 4000/4001/4002/4003 | （系统级定义，白名单外未追） | ⛔ 未考证 | 火水暗光觉醒 buff 本体 |
| .ani / .atk / PO | — | 各资源目录 | ⛔ 无专属 | — |
| 图标 | LatentAbilityIcon.img | `Character/LatentAbilityIcon.img` | ✅（路径） | 独立图标表（不做 UI） |

## 4. 资源需求

必需级缺失 img = **0**（无专属资源文件）。

## 5~7. 实现判定与困难（⛔ 级精简）

- **⛔ 原因（三层全空）**：
  1. **元素属性系统缺失**（§6.3 缺失清单在档）——四元素攻击无键无消费，"取最高者"无输入；
  2. 觉醒增益本体在系统级 script appendage 4000-4003（数值未考证，且依赖元素伤害公式）；
  3. static 五开关由**外部系统**（觉醒 UI/任务/道具）写回——我方无对应功能面。
- 就业面：本技能无战斗逻辑可移植；待元素属性系统立项时，其"每 1.5s 自检 + 取最高元素 +
  周期续挂"的调度模式可整体平移为 `LatentAbilityBuff : BuffDefinition`（`TickTimeMs=1500`，
  TickActions 内查元素面板择高挂子 Buff——Tick 轮询形态与 Buff 系统现架构兼容，届时零新机制）。
- 概念映射：`sqx_AppendScriptAppendage` → `ctx.AddBuff`（Tick 内条件挂载）；`appendage 定时器节流`
  → `TickTimeMs` 原生对应。

## 8. 存疑与缺口上报

- 未考证：①`set_appendage_latentability` 函数定义位置（白名单内无）；②script appendage
  4000-4003 的数值内容（元素伤害加成量，未读到表）；③static 开关的官方写回方（觉醒 UI 推断）；
  ④29 列 level info 的设计用途（全零，疑为觉醒等级扩展位）。
- 新缺口：**元素觉醒调度样本**——若元素系统立项，本技能是"周期性条件挂 Buff"形态的唯一样本
  （Buff Tick 轮询可覆盖，不单独立项）；除此之外无新增。
- 翻译缺口：`.skl` 子命令（全局已知项；本技能数据全零，手抄成本为零）。

**给下轮的经验**：`append = "appendage/..."` 开头（不带 `character/swordman/` 前缀）的注册行
= 跨职业公共 appendage，实际文件在 `sqr\appendage\character\`；`Character/xxxIcon.img` 独立
图标表 = 系统级功能技能的标志（非战斗技能，分析可到此为止）。
