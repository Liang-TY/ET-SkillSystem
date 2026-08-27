# 物理背击（PhysicalBackAttackCritical）

> 技能ID 178 | 级别 C | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 物理背击 | `PhysicalBackAttackCritical.skl [name]` |
| 英文名 | PhysicalBackAttackCritical（skl 文件名；[name2]=`Physical Back Attack`） | skl |
| 职业 | 全职业共通（[skill fitness growtype] 0-5 全列，各 10 级） | skl |
| 学习等级 | 20 | skl `[required level]` |
| 最高等级 | 20 | skl `[maximum level]` |
| 类型 | [passive]（skill class 4 通用系） | skl `[type]` |
| 图标 | Character/Common/SkillIcon.img #12/13（通用系图标库） | skl `[icon]` |
| 一句话效果 | 从背后攻击敌人时，增加物理暴击率 1.5%→30% | skl `[explain]` |

**level property**：单列 col0 = 15→300（`-1 0 0.1` → 1.5%→30%，每级 +1.5%）。
（姊妹技能 186 物理暴击/188 魔法暴击/189 魔法背击同族，C4 批。）

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- `swordman_load_state.nut` 无注册；`passive_skill_swordman.nut` 无 case 178；
- `sqr\character\swordman\` 全树 grep `physicalback`：无命中。

——纯引擎内置：物理伤害结算时，若命中方向为背击 → 暴击率 += col0×0.1%
（引擎的暴击判定 = 命中结算内的概率分支，暴击伤害有独立乘区）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | PhysicalBackAttackCritical.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\PhysicalBackAttackCritical.skl` | ✅（73 行） | 全部数据 |
| 注册行/主 nut/appendage | — | `…\pvf\sqr\character\`（grep 实测） | ⛔ 缺失（引擎内置） | 暴击公式在引擎 |

无 .ani/.atk/.als/.obj。

## 4. 资源需求

无（缺失 img = 0）。

## 5~7. 实现/翻译/困难（⛔ 合并）

- **判定 ⛔（两档缺口相乘）**：
  | 环节 | 缺口 | 状态 |
  |---|---|---|
  | 暴击系统 | **新缺口**：skill 包全树（Runtime/DotNet~/Scripts）grep `critical/crit` 零命中——无暴击率、无暴击伤害乘区、MeleeHit/HitReaction 无暴击字段 | 缺失（本批首报） |
  | 背击判定 | 受击方向判定（在案 R5-B3，逆转反击首撞）——攻击方位于受击者背面才成立 | 缺失（在案） |
- 即使只做半（无条件暴击率）也缺第一档；两档齐备后本技能=条件数值，表达容易。
- 简化建议：不实现。若"暴击系统"立项（建议与 186/188/189 + 209 鬼夜/119 幻鬼之力的
  CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_HIT_RATE 一并评估——C2/C4 批将复撞），本技能随后端到端补。
- 翻译工具：`.skl` 无子命令（全局已知）；单列 20 值手抄即可。无新增缺口。

## 8. 存疑与缺口上报

- 无未考证项。
- **新系统级缺口上报：暴击系统缺失**（暴击率面板键 + 伤害结算暴击分支 + 暴击伤害乘区）。
  已知关联技能：本技能 + 186/188/189（C4）+ 209 鬼夜（C4）+ 119 幻鬼之力（C2，代码侧已见
  CHANGE_STATUS_TYPE_PHYSICAL_CRITICAL_HIT_RATE 消费点）+ 78 剑影太刀精通的命中半（Stuck 减
  属"命中/卡定"姊妹概念）。建议主循环汇总时单列"暴击/命中判定系统"条目。
