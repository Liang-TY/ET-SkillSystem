# 公会：增加角色属性（StatusUp）

> 技能ID 200 | 级别 C | 可实现性 🔶（四属性面板可表达 / 消费端全卡死；公会购买链不做） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 公会 : 增加角色属性 | `StatusUp.skl [name]` |
| 英文名 | StatusUp（[name2] 实测 `Guild Status Up`——英文别名） | skl [name2] |
| 职业 | 全系共通（growtype 0-5；**公会技能**） | skl [skill fitness growtype] |
| 学习等级 | 2（角色等级；**由公会 GSP 购买解锁**：[purchase gsp] 1 / [purchase gold] 100000） | skl [required level] / [purchase gsp] / [purchase gold] |
| 最高等级 | 20 | skl [maximum level] |
| 类型 | passive（skill class 4） | skl [type] / [skill class] |
| 指令 / CD / MP | 无 | skl |
| 一句话效果 | 增加公会成员的力量/体力/智力和精神（只适用于普通以上公会成员） | skl [explain] |
| 图标 | `Character/Common/SkillIcon.img` #42/43（共通表——跨职业标志，175 经验同判据） | skl [icon] |

**level info 4 列（Lv1 / Lv20）**：`4 4 4 4` → `270 270 270 270`（力/体/智/精神四列同值，
+13~22/级递增）。level property 模板四占位（增加力量/体力/智力/精神 `<int>`），向量 4 组
`-1 0~3 1.0` 直读——**列语义确定**（模板与 explain 互证）。

## 2. 技能逻辑走读（极简）

**无任何脚本**（实测）：load_state 无、passive_skill_swordman.nut 无 case 200、白名单 grep
`statusup` 零命中。与 175/176 同形态的**纯数据技能**——引擎读 skl 后把四维属性加进角色面板
（DNF 侧力量→物攻、智力→魔攻、体力→HP、精神→MP，公式全在引擎）。

"公会成员资格（普通以上）"的检查在公会系统侧（GSP 购买/成员等级），与技能数据无关。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | StatusUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\StatusUp.skl` | ✅ | 4 列 × 20 级 + 购买消耗 |
| 注册行 / 被动注册 / nut | — | load_state / passive_skill_swordman.nut / swordman 目录 | ⛔ 均无（引擎内置） | — |
| .ani / .atk / PO | — | 各资源目录 | ⛔ 无专属 | — |
| 图标 | SkillIcon.img #42/43 | `Character/Common/SkillIcon.img` | ✅（路径） | 不做 UI |
| 姊妹技能 | ExperienceUp.skl（201，经验加成） | `…\skill\Swordman\ExperienceUp.skl` | ✅ | 同为公会 GSP 技能（201 文档） |

## 4. 资源需求

必需级缺失 img = **0**（无任何资源文件）。

## 5. 实现方案草案（§5/§6/§7 合并精简）

**判定分半**：

| 半边 | 结论 | 依据 |
|---|---|---|
| 面板可表达 | ✅ 可 | 四维需新增 NumericType 键（Strength/Stamina/Intelligence/Spirit；176 草案已计划 Intelligence=1007，本文补齐另外三个）+ Buff 数值挂摘 |
| 消费端 | ⛔ 全卡死 | 四维→伤害/HP/MP 的引擎公式无对应（属性伤害消费链 5 实证在档；体力→MaxHp 理论上可经 MaxHpAdd 表达一半，精神→MP 无 MP 系统） |
| 公会购买链 | 不做 | 无公会系统（GSP/金币/成员等级整套不存在；与 201 同族） |

**届时形态**：`GuildStatusBuff : BuffDefinition`（永久），AddActions/RemoveActions 数值挂摘对
（四路 AddOwnerNumeric）。demo 视角可作"四维属性面板试点"——与远古记忆（176）同为属性面板
验证件，数值更大更平（+270 满档 vs +300 智力），二选一即可。数值建议取 Lv10 档 +99。

## 6~8. 困难与缺口上报（精简）

| DNF 行为 | 缺口 | 简化 |
|---|---|---|
| 四维→战斗公式 | 属性消费链（在档） | 面板可见即可 |
| GSP/金币购买、成员资格 | 公会系统（无对应，不立项） | 角色生成直接挂 |
| 等级 20 档 | 等级缩放（延后） | 固定档 |

- 未考证：四列同值无差异化（Lv1 起 +4 平铺）是否有隐藏修正。
- 新缺口：无新增（四维键随 176 的 Intelligence 计划扩位即可）。
- 翻译缺口：`.skl` 子命令（全局已知项）。
