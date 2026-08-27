# 暗之亲和（DarkFriendship）

> 技能ID 29 | 级别 C | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 暗之亲和 | `DarkFriendship.skl [name]` |
| 英文名 | DarkFriendship（skl 文件名；[name2]=`The Friendship of Darkness`） | skl |
| 职业 | 鬼泣（[skill fitness growtype] = 2） | skl |
| 学习等级 | 18 | skl `[required level]` |
| 最高等级 | 1（[growtype maximum level] gt2=1） | skl |
| 类型 | [passive]（skill class 3） | skl `[type]` |
| 一句话效果 | 增加自身 20 点暗属性抗性，减少 10 点光属性抗性 | skl `[explain]` + static data |

**static data**：`20 -10`（暗抗 +20 / 光抗 -10，无 level property——单级被动直读 static）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- `swordman_load_state.nut`：无注册行；
- `passive_skill_swordman.nut`：无 case 29；
- `sqr\character\swordman\` 全树 grep `darkfriendship`：无命中。

——**纯引擎内置属性被动**：抗性数值由引擎元素伤害管线直接消费（元素伤害结算时查攻受双方抗性）。

### 2.2 行为重建（引擎惯例，推断）

学习即生效：角色暗抗 +20、光抗 -10，常驻、无动画无 Buff 外显。DNF 中抗性参与
"元素伤害增减免"公式（受暗属性攻击伤害降低、受光属性攻击伤害提高）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DarkFriendship.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DarkFriendship.skl` | ✅（44 行） | 全部数据 |
| 注册行/主 nut/appendage | — | `…\pvf\sqr\character\`（grep 实测） | ⛔ 缺失（引擎内置） | 行为在引擎 |
| 图标 | SkillIcon.img #60/61 | `…\pvf\character\swordman\effect\SkillIcon.img` | ✅ | UI |

无 .ani/.atk/.als/.obj（零视觉零判定）。

## 4. 资源需求

无（缺失 img = 0）。

## 5~7. 实现/翻译/困难（⛔ 级合并）

- **判定 ⛔**：效果 100% 落在"元素属性系统"（§6.3 缺失档）——我们的伤害管线无元素类型、无抗性
  数值键、无消费公式。NumericType（实测仅 Speed/Hp/MaxHp/Attack/Defense/ForbidMove/ForbidSkill）
  无任何抗性键。面板侧也无法表达（加键容易但全无消费端），两半皆空。
- 若元素系统立项（暗月降临 030、暗/光属性武器技都会受益），本技能是"最薄"的试点：
  单级、static 两值、纯数值。
- 翻译工具：`.skl` 无子命令（全局已知项）；本技能 2 个值手抄 10 秒，无催迫。无新增缺口。
- 简化建议：不实现（对 demo 战斗无影响）；UI 上可做"已习得"图标占位。

## 8. 存疑与缺口上报

无未考证项。缺口归档：元素属性系统（在案），本技能为其第 N 个实证（暗月降临 030 首撞）。
