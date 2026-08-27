# 武器奥义（WeaponMasteryUp）

> 技能ID 27 | 级别 C | 可实现性 ⛔（无技能等级系统；demo 侧并入精通 Buff 档位，无独立实现件） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 武器奥义 | `WeaponMasteryUp.skl [name]` |
| 英文名 | WeaponMasteryUp（[name2] 实测 `The Extreme of Weapon`——英文别名） | skl [name2] |
| 职业 | 剑魂专属（[skill fitness growtype] = 1；growtype 上限 `0 30 0 0 0 0`） | skl |
| 学习等级 | 15 | skl [required level] |
| 最高等级 | 30（growtype 1 上限 30，其余 0） | skl [maximum level] / [growtype maximum level] |
| 类型 | passive（skill class 1） | skl [type] / [skill class] |
| 指令 / CD / MP | 无 | skl |
| 一句话效果 | 领悟武器奥义，增加所有武器的精通 Lv | skl [explain] |

**static data = `12 13 14 15 4`**：五个精通技能 ID 列表（短剑/太刀/巨剑/钝器/光剑精通 4——
光剑精通 LightSwordMastery 属 C3 批次，本文按 id 4 记档）。

**level info 5 列 × 50 级**（实测节选）：Lv1 `3 3 3 3 2`、Lv2 `4 4 4 4 3`、… 每级 +1。
即：**短剑/太刀/巨剑/钝器精通 +（N+2）级，光剑精通 +（N+1）级**（N=奥义等级；
col0-3 与 col4 差 1，光剑档位低 1 级）。无 [level property] 模板节（数值即等级，无需展示换算）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无任何脚本**（实测）：
- `swordman_load_state.nut` grep `weaponmasteryup`：无命中；
- `passive_skill_swordman.nut`：无 case 27（对照：同机制的**鬼连斩·极** SKILL_SPEEDSLASHUPPER
  有 case——`upperSkill.setLevel(1, sourceSkillLevel)` 的 nut 写法，正好是本技能引擎行为的
  **同构参照**：把奥义等级写进精通技能的等级簿记）；
- 白名单 grep Mastery：仅 passive_skill_swordman.nut 自身。

### 2.2 机制归纳

```
习得奥义 Lv N → 引擎把 12/13/14/15 的生效等级各 +N+2、4（光剑）+N+1
（等级写入方向 = "奥义 → 精通"；剑魂裸点精通上限 1，故实际等级几乎全由奥义贡献——见 012 §2.3）
```

奥义本身**不提供任何直接属性**——纯等级簿记技。所有战斗效果都经由"精通等级↑ → 精通前三列
属性与附加效果增强"间接产生。

### 2.3 被动对象 / appendage

无（§2.1 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | WeaponMasteryUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\WeaponMasteryUp.skl` | ✅ | 5 列 × 50 级 + static（技能 ID 表） |
| 注册行 / 被动注册 / nut | — | load_state / passive_skill_swordman.nut / swordman 目录 | ⛔ 均无（引擎内置；nut 同构参照 = passive_skill 内 SKILL_SPEEDSLASHUPPER case） | — |
| .ani / .atk / PO / 特效 | — | 各资源目录 | ⛔ 无专属 | — |
| 图标 | SkillIcon.img #66/67 | `Character/Swordman/Effect/SkillIcon.img` | ✅（路径） | 不做 UI |
| 联动精通 | ShortSword/Blade/HeavySword/Blunt/LightSwordMastery.skl（12/13/14/15/4） | `…\skill\Swordman\` | ✅ | 被提升对象（12-15 见本批，4 属 C3） |

## 4. 资源需求

必需级缺失 img = **0**（纯数据技能，无任何专属资源文件——全 241 技能里与 175 同级的零资源形态）。

## 5~7. 实现判定与困难（⛔ 级精简）

- **⛔ 原因**：本技能的**全部语义 = 修改其他技能的等级**。我方无技能等级系统（等级数值缩放
  在 §6.3 延后档；无 sq_GetSkillLevel 对应物——"跨技能 level/static data 查询门面"缺口 R3-A11 已在档，
  本技能是其"写入向"镜像：**跨技能等级写入门面**）。
- **demo 规避（不实现独立件）**：若按 012 §5 草案把精通做成常驻 Buff，奥义无需存在——
  直接把 Buff 数值取更高档位即可等效（"奥义满级"预设）。届时若要还原"奥义 vs 裸点"的加点博弈，
  需技能等级系统先行。
- 概念映射：`setLevel 簿记` → （无对应）；`static 技能 ID 表` → 精通 Buff 档位表（配置层替代）。

## 8. 存疑与缺口上报

- 未考证：col4（光剑）比 col0-3 低 1 级的原因（平衡性常识级）；50 级表 vs growtype 上限 30 的
  超出部分用途。
- 新缺口：**跨技能等级写入门面**（R3-A11"查询门面"的镜像项，本技能唯一增量——但因整条等级
  链都不存在，不单独建议立项，并入"技能等级系统"评估）。
- 翻译缺口：`.skl` 子命令（全局已知项）。

**给下轮的经验**：`static data = 一串技能 ID`（如本例 12 13 14 15 4）= 等级镜像/联动技的标志，
读 skl 时先看 static 是否为 ID 表可省走读；nut 侧同构参照在 passive_skill_swordman.nut 的
SKILL_SPEEDSLASHUPPER case（唯一有脚本的等级镜像样本）。
