# 鬼夜（SwordGhost25）

> 技能ID 209 | 级别 C | 可实现性 ⛔（暴击率 + 攻击力双数值无消费链） | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼夜 | `skill\Swordman\SwordGhost\SwordGhost25.skl [name]` |
| 英文名 | SwordGhost25（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影二觉被动（[second growtype maximum level] 槽 11-12 非零 = 剑影段；explain"夜刀神"= 剑影二觉称号互证） | 同上 |
| 学习等级 | 48 | 同上 [required level] |
| 最高等级 | 50（但二觉段上限 **1/1**——每觉醒段仅 1 级？与 50 行数据矛盾，mod 配置疑点，§8） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | passive（skill class 1） | 同上 [type] |
| 指令 | 无 | 同上 |
| CD / MP | 2000ms / 1（**被动技带 CD/MP/可执行状态（0 14 8）——疑数据驱动的自动触发结构，§8**） | 同上 [cool time] / [consume MP] / [executable states] |
| static data | `0` | 同上 [dungeon][static data] |
| 一句话效果 | 增加夜刀神的基本攻击力、技能攻击力和物理暴击率 | 同上 [explain] |

**level property（2 列，Lv1 → Lv50，向量全对位）**：
`基本攻击力和技能攻击力增加量<float1>%%`←**col1**×0.1、`物理暴击率增加量<float1>%%`←**col0**×0.1
（注意列序与模板行序相反——向量索引为准，L21）。
- col0 物理暴击率：150→**15.0%** → 885→**88.5%**（mod 数值风）；
- col1 攻击力：100→**10.0%** → 590→**59.0%**。
无 pvp 段（整节空）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（有真实脚本挂载——非纯引擎内置）

- `passive_skill_swordman.nut` **case 209 实测存在**（唯一有脚本的二觉被动）：
  ```
  case 209:  // growtype 0 或 5（剑影）才挂
    append = "character/jg_swordman/swordghost13/ap_buff_209.nut"
    sq_AppendAppendage(obj, obj, 209, false, append, true)
    crb = sq_GetLevelData(obj, 209, 0, skill_level) * 0.1     // col0 = 暴击率
    change_appendage.addParameter(CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_HIT_RATE, false, crb)
  ```
  即：**col0 暴击率经 change-status 挂载实证**（×0.1 与我方列解读一致，交叉验证成立）。
- `JG_SwordMan\swordghost13\ap_buff_209.nut`：**空壳**（proc/onStart/onEnd/prepareDraw 全 no-op，
  仅一段读技能 123 gauge 的死代码；isEnd 恒 false 常驻）——挂载点存在但无行为。
- col1 攻击力增益无脚本痕迹 → 引擎标准被动数值管线（未考证，§8）。
- 无 PO、无专属动画/特效（grep 实测）。

### 2.2 机制归纳

```
学得即常驻（剑影限定）：
  基本攻击力/技能攻击力 + col1×0.1%（10% → 59%）
  物理暴击率 + col0×0.1%（15% → 88.5%）——change-status 挂载（脚本实证）
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SwordGhost25.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SwordGhost\SwordGhost25.skl` | ✅ 实测 | 2 列 + static 0 |
| lst 条目 | swordmanskill.lst 277-278 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 209 → 本 skl |
| 被动注册 | passive_skill_swordman.nut case 209 | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | 暴击率挂载（growtype 0/5 门禁） |
| appendage | ap_buff_209.nut | `…\pvf\sqr\character\JG_SwordMan\swordghost13\ap_buff_209.nut` | ✅ 实测（空壳） | 挂载锚点，无行为 |
| 注册行 / 主 nut / PO / 动画 / 特效 | —（全查无，实测） | 同 186 §3 各路径 | ⛔ 缺失 | — |
| 图标 | SkillIcon.img #628/#629 | `Character/Swordman/Effect/SkillIcon.img` | ✅ 实测（路径） | 无 UI 消费 |

## 4. 资源需求

**零资源需求**（无专属视觉文件）。

## 5. 实现方案草案

⛔ 暂缓——双数值全撞消费链：
1. **暴击系统**（186 §8：CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_HIT_RATE 在我方无对应数值位、
   伤害无暴击 roll——挂载侧脚本我们完全可表达（AddNumeric/ChangeStatus→Buff），消费端卡死）；
2. **属性数值无伤害消费链**（攻击力 +10%~59%，176 §8）。

届时形态（暴击系统落地后）：`NumericType.PhysicalCritRate` 键 + 学得时一次性
`ctx.AddNumeric(caster, PhysicalCritRateAdd, +15%)`——无内容件，纯数据行。
（可对照同文件 case 119 swordghost27：攻速/移速/暴击率/暴击伤害四参数同款挂载。）

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| SwordGhost25.skl | `.skl` 无子命令（2 列×50 级 + static 0 + 可执行状态节） | 手抄 2 组值；全局已知缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 物理暴击率 +15%~88.5% | **缺失：暴击系统**（186 §8；本技能脚本挂载实证但消费端零） | 随伤害公式立项 |
| 攻击力 +10%~59% | **缺失：属性伤害消费链**（第 11 实证） | 同上 |
| 二觉段等级上限 1/1 vs 50 行数据 | mod 配置疑点（§8） | demo 固定 1 级档 |

## 8. 存疑与缺口上报

**未考证项**
1. [maximum level] 50 与 [second growtype maximum level] 1/1 的矛盾（疑 mod 想做成"每段 1 级"但没删 50 行数据）。
2. 被动技却带 [cool time] 2000 / [consume MP] 1 / [executable states] 0 14 8——疑配合自动施放/proc 结构的数据残留，
   无脚本可考。
3. col1 攻击力的挂载通道（脚本只见暴击率一路；推断走引擎标准被动管线）。
4. ap_buff_209.nut proc 内读技能 123（幻鬼槽位技）gauge 的死代码——原意疑为剑影 gauge 联动，mod 残留。

**新系统级缺口**：无新上报（暴击系统 186 §8 消费方 +1——本技能提供**脚本侧挂载形态样本**
（change-status addParameter），伤害公式立项时可直接照此映射）。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
