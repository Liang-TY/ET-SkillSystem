# 杀气感知（SenseViolentTemper）

> 技能ID 55 | 级别 C | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 杀气感知 | `SenseViolentTemper.skl [name]` |
| 英文名 | SenseViolentTemper（skl 文件名；[name2]=`Rage Sense`——DNF 官方英文名，两版并存） | skl |
| 职业 | 阿修罗（[skill fitness growtype] = 4） | skl |
| 学习等级 | 18 | skl `[required level]` |
| 最高等级 | 1 | skl `[maximum level]` |
| 类型 | [passive]（skill class 1） | skl `[type]` |
| 一句话效果 | 失明状态下可感知敌人杀气：+100% 失明抗性、+3% 背击回避率、减缓防具耐久度下降 | skl `[explain]` |

**static data（dungeon）**：`100 30 20` = 失明抗性 100（%）、背击回避率 30（×0.1=3%）、
耐久下降减缓 20（%，推断）。无 level property（单级）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- `swordman_load_state.nut`：无注册行；
- `passive_skill_swordman.nut`：无 case 55；
- `sqr\character\swordman\` 全树 grep `senseviolent|ragesense`：无命中。

——纯引擎内置属性被动（DNF 设定：阿修罗失明的代价换感知，本技能为其补偿型被动）。

### 2.2 行为重建（引擎惯例，推断）

学习即生效，三路常驻数值：
1. 失明抗性 100%——异常状态判定时豁免失明（blinding）；
2. 背击回避率 3%——被背面攻击命中时概率闪避；
3. 防具耐久下降减缓 20%——受击时耐久损耗折减。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SenseViolentTemper.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SenseViolentTemper.skl` | ✅（64 行） | 全部数据 |
| 注册行/主 nut/appendage | — | `…\pvf\sqr\character\`（grep 实测） | ⛔ 缺失（引擎内置） | 行为在引擎 |
| 图标 | SkillIcon.img #164/165 | `…\pvf\character\swordman\effect\SkillIcon.img` | ✅ | UI |

无 .ani/.atk/.als/.obj（零视觉零判定）。

## 4. 资源需求

无（缺失 img = 0）。

## 5~7. 实现/翻译/困难（⛔ 级合并）

- **判定 ⛔**，三个子效果各撞一档：
  | 子效果 | 缺口 | 档位 |
  |---|---|---|
  | 失明抗性 100% | 异常状态系统仅 Burn/Stun/Bleed/Freeze 四 Buff，无"抗性"概念、无失明状态 | 缺失 |
  | 背击回避 3% | 受击方向判定（在案 R5-B3，逆转反击首撞）+ 回避/闪避系统（无） | 缺失（半在案） |
  | 耐久减缓 20% | 耐久度系统整体缺失（同 39 武器节制） | 缺失 |
- 简化建议：不实现。前两效在"异常状态/受击方向"两个专题落地前无意义；第三效永远无意义
  （我们无耐久概念，也不建议有）。
- 翻译工具：`.skl` 无子命令（全局已知）；3 个 static 值手抄即可。无新增缺口。

## 8. 存疑与缺口上报

- 未考证：static[2]=20 的精确单位（% vs 绝对值）——无消费端，不影响判定。
- 缺口归档：背击回避并入"受击方向判定"（在案）；**耐久度系统**（39/55 两实证）建议直接
  归档为"不实现"类（无战斗意义），不立项。
