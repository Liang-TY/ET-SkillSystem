# 血气旺盛（BloodyVigorous）

> 技能ID 63 | 级别 C | 可实现性 🔶（效果全由十字斩侧消费，064 已给规避方案；交叉引用文档） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 血气旺盛 | `BloodyVigorous.skl [name]` |
| 英文名 | BloodyVigorous（[name2] 实测 `Blood Lust`——英文别名） | skl [name2] |
| 职业 | 狂战士专属（[skill fitness growtype] = 3；上限 1 级） | skl |
| 学习等级 | 15 | skl [required level] |
| 最高等级 | 1 | skl [maximum level] / [growtype maximum level] `0 0 0 1 0 0` |
| 类型 | passive（skill class 2） | skl [type] / [skill class] |
| 指令 / CD | 无（[consume MP] 0 0 为占位） | skl |
| 一句话效果 | 施放十字斩时用 HP 消减替代 MP 消耗，并使十字斩出血机率 +40% | skl [explain] |
| static data | **400** | skl（= 40%×10 内部量纲；explain 互证） |

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无任何脚本**（实测，与 064-GoreCross.md §3 记档一致）：
- `passive_skill_swordman.nut` 无 case 63；
- header 常量表有 `SKILL_SWORDMAN_BLOODYVIGOROUS <- 63`（84-114 行段实测）——但白名单内
  **除定义外零引用**（grep BLOODYVIGOROUS 全白名单仅命中 header 自身）；
- 十字斩（64）为引擎内置技，其"MP→HP 替代"与"出血率+40%"的检查点在引擎内的十字斩消耗/命中
  流程里（读 `sq_GetSkillLevel(63)` 分支）。

### 2.2 机制归纳

```
习得即生效（无等级梯度，1 级封顶）：
  ① 十字斩施放时：消耗从 MP 改为 HP（量值沿用十字斩 MP 列，未考证是否同额）
  ② 十字斩命中时：出血几率 +40 个百分点（static 400）
```

两个效果**都不是血气旺盛自身的行为**——它是纯粹的"被十字斩读取的标记技"。
064-GoreCross.md §1/§7 已引用本技能并给出十字斩侧的落地结论：
demo 十字斩固定 100% 出血（等于本被动满档常开），HP 替代可选做 `ctx.ConsumeCasterHp(常数)`。

### 2.3 被动对象 / appendage

无（§2.1 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodyVigorous.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodyVigorous.skl` | ✅ | static 400 + 职业约束（唯一数据） |
| 常量定义 | swordman_header.nut 87 行 | `…\sqr\character\swordman\swordman_header.nut` | ✅ | SKILL_SWORDMAN_BLOODYVIGOROUS = 63 |
| 注册行 / 被动注册 / nut | — | load_state / passive_skill_swordman.nut / swordman 目录 | ⛔ 均无（引擎内置） | — |
| 消费方 | GoreCross.skl（64）及其引擎流程 | `…\skill\Swordman\GoreCross.skl` | ✅ | 效果注入点（064 文档全量走读） |
| .ani / .atk / PO | — | 各资源目录 | ⛔ 无专属 | — |
| 图标 | SkillIcon.img #168/169 | `Character/Swordman/Effect/SkillIcon.img` | ✅（路径） | 不做 UI |

## 4. 资源需求

必需级缺失 img = **0**（纯数据技能，无任何专属资源文件）。

## 5~7. 实现判定与困难（精简合并）

**判定 🔶 的含义**：本技能自身无需实现任何内容件——它是十字斩（064 已解析、✅/🔶 可实现）的
**参数修饰**。落地路径完全跟随 064 的两个决策点：

| DNF 行为 | 我方现状（064 §5/§7 已定） | 本技能增量 |
|---|---|---|
| MP→HP 替代 | MP 系统延后；BloodBoomSkill 已示范 `ConsumeCasterHp` | 无（十字斩侧可选 `ConsumeCasterHp(固定值)`） |
| 出血率 +40% | 十字斩 PO 出血走 `HitReaction.ProcBuffId/ProcChance` | demo 固定 ProcChance=100 等效满档 |
| "习得才生效"门槛 | 无技能习得系统 | 等效常开；若要还原博弈需**自身技能等级查询门面**（R4-B18 已在档开关技缺口同族） |

- 概念映射：`sq_GetSkillLevel(63) 检查` → 十字斩 HitReaction 参数档位选择（配置层）。
- 无注册点增量（无 SkillId/AnimId/BuffId/img/翻译需求——static 单值 400 手抄）。

## 8. 存疑与缺口上报

- 未考证：HP 替代量与十字斩 MP 列的关系（同额/换算率）；static 400 量纲（40%×10 为 explain
  互证的推断）。
- 新缺口：无新增（064 与本批 012 已上报项覆盖；本文件价值 = 把 064 的"血气旺盛引用"落成
  独立存档，C2 批内闭环）。
- 翻译缺口：`.skl` 子命令（全局已知项）。
