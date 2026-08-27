# 投掷精通（ThrowMastery）

> 技能ID 177 | 级别 C | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 投掷精通 | `ThrowMastery.skl [name]` |
| 英文名 | ThrowMastery（skl 文件名；[name2]=`Throw Mastery`） | skl |
| 职业 | 全职业共通（[skill fitness growtype] 0-5 全列，各 20 级） | skl |
| 学习等级 | 20 | skl `[required level]` |
| 最高等级 | 40 | skl `[maximum level]` |
| 类型 | [passive]（skill class 4 通用系） | skl `[type]` |
| 特殊 | 每次投掷消耗 1 MP（[consume MP] 1 1） | skl |
| 图标 | **Character/Common/SkillIcon.img #10/11**（通用系图标库，非剑士专属） | skl `[icon]` |
| 一句话效果 | 投掷时增加投掷道具攻击力 50%→2000%（双段伤害道具的第 2 段不加成） | skl `[explain]` |

**level property**：单列 col0 = 50→2000（`-1 0 1.0` 直读，每级 +50%）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- `swordman_load_state.nut` 无注册；`passive_skill_swordman.nut` 无 case 177；
- `sqr\character\swordman\` 全树 grep `throwmastery`：无命中。
  （注意区分：`swordman_throw.nut` 是**投掷形态技能的状态回调**（23/47/82/18/222 等，实测通读），
  与本技能无关——本技能作用于**投掷道具**（道具栏的燃烧瓶/飞刀类）伤害管线。）

——纯引擎内置：投掷道具伤害结算时查攻方技能 177 等级加成（双段道具第 2 段跳过加成的规则在引擎）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ThrowMastery.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ThrowMastery.skl` | ✅（97 行） | 全部数据 |
| 注册行/主 nut/appendage | — | `…\pvf\sqr\character\`（grep 实测） | ⛔ 缺失（引擎内置） | 道具伤害管线 |
| 排除项 | swordman_throw.nut | `…\pvf\sqr\character\swordman\swordman_throw.nut` | ✅（实测，非本技能） | 投掷形态技能回调 |

无 .ani/.atk/.als/.obj。

## 4. 资源需求

无（缺失 img = 0）。

## 5~7. 实现/翻译/困难（⛔ 合并）

- **判定 ⛔（系统整体缺失）**：我们无投掷道具系统——没有道具栏、没有投掷物品类、没有
  "投掷伤害结算"这一消费端。技能本体（+X% 乘区）连挂载点都不存在。
- 与"飞刀/燃烧瓶"道具相关的还有地图/场景交互、背包系统——均为远期系统，不建议为此立项。
- 简化建议：不实现（通用系技能，与鬼剑士战斗演示无关；真做投掷道具时再连带评估）。
- 翻译工具：`.skl` 无子命令（全局已知）；单列 40 值手抄可接受。无新增缺口。

## 8. 存疑与缺口上报

- 未考证：+2000%（Lv40）的数值量级为 mod 改动痕迹（官服此技能数值低得多）；不影响判定。
- 缺口归档：**投掷道具系统**（新记档：无任何投掷物概念——与"地图/场景交互""背包"同属
  远期系统缺口，建议标"不实现/远期"而非立项）。
