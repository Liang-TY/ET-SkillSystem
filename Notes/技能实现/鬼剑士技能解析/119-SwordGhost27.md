# 幻鬼之力（SwordGhost27）

> 技能ID 119 | 级别 C | 可实现性 🔶（面板可表达 / 四路消费端全卡死；本批数值最干净的可直译样本） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻鬼之力 | `SwordGhost/SwordGhost27.skl [name]`（无 [name2] 节，L1 规则取文件名） | skl |
| 英文名 | SwordGhost27（取 skl 文件名） | 同上 |
| 职业 | 剑影专属（[skill fitness growtype] = 5；上限 20，共通 0 亦可学 20——`20 0 0 0 0 20`） | skl |
| 学习等级 | 15 | skl [required level] |
| 最高等级 | 50（growtype 约束实际 20） | skl [maximum level] |
| 类型 | passive（skill class 1）；skl 内另有 [consume MP] 1 / [cool time] 2000 / [executable states] `0 14 8` / [weapon effect type]——被动带施法三件的占位写法（未考证用途，疑引擎要求） | skl |
| 一句话效果 | 增加剑影的攻击速度、移动速度、物理暴击率和暴击伤害 | skl [explain] |

**level info 4 列（Lv1 / Lv20，×0.1 向量）**：
- col1 攻击速度：`110` → `800`（+11% → +80%，+3.6/级）
- col2 移动速度：`60` → `750`（+6% → +75%）
- col3 物理暴击率：`60` → `750`（+6% → +75%）
- col0 暴击伤害：`55` → `400`（+5.5% → +40%）

列→模板对位（level property 实测）：`攻击速度 <float1>%% / 移动速度 <float1>%% / 物理暴击率
<float1>%% / 暴击伤害 <float1>%%`，向量 `-1 1 0.1 / -1 2 0.1 / -1 3 0.1 / -1 0 0.1`——
四列语义**确定**（模板占位与注册表取列一一互证，见 §2.2）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本批两个"有脚本"的被动之一**（实测）：
`passive_skill_swordman.nut` case 119（约 87-109 行）：

```
case 119:
    append = "character/jg_swordman/swordghost13/ap_swordghost27.nut"
    if (skill_level > 0 && (growType == 0 || growType == 5)):     // 剑影/共通
        挂 appendage
        changeStatus = appendage.sq_AddChangeStatus("swordghost27", ...)
        atspdb = sq_GetLevelData(obj, 119, 1, level) * 0.1        // 攻速   = col1×0.1
        mspdb = sq_GetLevelData(obj, 119, 2, level) * 0.1        // 移速   = col2×0.1
        crb   = sq_GetLevelData(obj, 119, 3, level) * 0.1        // 物暴率 = col3×0.1
        cratkb= sq_GetLevelData(obj, 119, 0, level) * 0.1        // 暴伤   = col0×0.1
        addParameter(CHANGE_STATUS_TYPE_ATTACK_SPEED,                false, atspdb)
        addParameter(CHANGE_STATUS_TYPE_MOVE_SPEED,                  false, mspdb)
        addParameter(CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_HIT_RATE,  false, crb)
        addParameter(CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_DAMAGE_RATE,false, cratkb)
    else: 摘除 appendage
```

——**注册行直接完成全部逻辑**：四路 ChangeStatus 注入，与 skl 模板四列一一对应（C2 批内
唯一"列语义经脚本双向印证"的被动）。

### 2.2 appendage（ap_swordghost27.nut，实测空壳）

`sqr\character\jg_swordman\swordghost13\ap_swordghost27.nut`：六回调（proc/onStart/prepareDraw/
onEnd/isEnd）全部空实现，`isEnd` 恒 `false`（永久）——**纯挂载容器**，数值全在注册表。
（对照同目录 ap_buff_171/209：那两个连数值都在注册表里写 changeStatus，本文件更空。）

### 2.3 机制归纳

```
习得（剑影）→ 永久 appendage → 四路面板属性常驻（攻速/移速/物暴/暴伤 ××%，随等级线性）
无触发条件、无目标、无视觉资源（白名单实测无 swordghost27 命名 ani/als/obj）
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SwordGhost27.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SwordGhost\SwordGhost27.skl` | ✅ | 4 列 × 50 级 |
| 被动注册 | passive_skill_swordman.nut case 119 | `…\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测（§2.1 原文） | 四路 ChangeStatus 注入（全部逻辑） |
| appendage | ap_swordghost27.nut | `…\sqr\character\jg_swordman\swordghost13\ap_swordghost27.nut` | ✅ 实测（空壳） | 永久挂载容器 |
| header 常量 | SKILL_SWORD_GHOST_27 <- 119 | `…\sqr\character\swordman\swordman_header.nut:144` | ✅ | ID 对号 |
| load_state 注册 | —（无 pushState，正常） | `…\swordman_load_state.nut` | ⛔ 无 | 被动无施法状态 |
| .ani / .atk / PO / 特效 | — | 各资源目录（ls/grep 实测） | ⛔ 无专属 | — |
| 图标 | SkillIcon.img #590/591 | `Character/Swordman/Effect/SkillIcon.img` | ✅（路径） | 不做 UI |

## 4. 资源需求

必需级缺失 img = **0**（无任何专属资源文件；特效层无 swordghost27 命名目录）。

## 5. 实现方案草案（§5/§6/§7 合并精简）

**判定分半**：

| 半边 | 结论 | 依据 |
|---|---|---|
| 面板可表达 | ✅ 可 | BuffDefinition 数值挂摘 + NumericType；四键中 Speed(1000) 已有，攻速/物暴率/暴伤需新增 3 键 |
| 消费端 | ⛔ 四路全卡 | 攻速→攻速系统缺失（R5-B5 四技共撞，本文第 5 例）；移速→Speed 零消费（R2-A7）；物暴率/暴伤→无暴击系统（019 回避率同族新增） |

**届时形态（最简被动样板）**：
- `SwordGhostPowerBuff : BuffDefinition`：`TotalTimeMs=0`（永久）、`TickTimeMs=0`、
  AddActions = { `SwordGhostPowerOn` }（一次挂四路：`AddOwnerNumeric(AttackSpeedAdd,+N)` ×4）、
  RemoveActions 对称减回（ForbidMoveOn/Off 同构，一对 Action 搞定）。
- 数值（demo 取 Lv10 档）：攻速 +28% / 移速 +24% / 物暴 +24% / 暴伤 +21%（原值列见 §1 直读）。
- 注册点：BuffIds 从 18 顺延（本草案记 23 号段）、ActionIds 从 15 起（撞号无妨）；无 SkillId/
  AnimId/img。**被动注入点缺口**（012 §8 上报）适用——角色初始化挂载入口是唯一前置。

## 6~7. 困难与简化（并入 §5）

| DNF 原版行为 | 缺口档位 | 简化 |
|---|---|---|
| 攻速 +11~80% | 攻速系统（缺失，第 5 实证） | 面板值可见，动画速度不变 |
| 移速 +6~75% | Speed 零消费（缺失） | 同上 |
| 物暴率/暴伤 | 暴击系统（缺失，本批与 019 合并上报） | 同上 |
| 等级 50 级表 | 等级缩放（延后） | 固定档 |

## 8. 存疑与缺口上报

- 未考证：skl 内 [cool time] 2000/[executable states]/[weapon effect type] 三节在 passive 上的
  用途（疑引擎对 skill class 1 的格式要求，无行为含义）。
- 新缺口：无独立新增（攻速第 5 例、暴击系统与 019 合并、被动注入点随 012）——本文件的主要
  贡献是**注册行直译样本**：passive_skill_swordman.nut 的 case 块 = 完整被动逻辑的标准形态，
  后续 C 批被动（171/209/78 等同型）可照此直读，不必再猜引擎行为。
- 翻译缺口：`.skl` 子命令（全局已知项）。

**给下轮的经验**：剑影系被动（119/123/171/209/78）全部走 `passive_skill_swordman.nut` case +
`jg_swordman\` 下 appendage（多空壳）模式——**先读注册表 case 块再决定要不要读 ap 文件**；
数值全在 case 的 sq_GetLevelData×系数 里，与 skl 模板向量双向可印证。
