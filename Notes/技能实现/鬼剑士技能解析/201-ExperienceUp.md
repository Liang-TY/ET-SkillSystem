# 极速成长（ExperienceUp）

> 技能ID 201 | 级别 C（**纠偏：实为 D 类倾向**——公会经验加成，非战斗技能；经验系统不存在） | 可实现性 ⛔（无经验系统） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 极速成长 | `ExperienceUp.skl [name]` |
| 英文名 | ExperienceUp（[name2] 实测 `Advice For Quick Growth`——英文别名） | skl [name2] |
| 职业 | 全系共通（growtype 0-5；**公会技能**） | skl [skill fitness growtype] |
| 学习等级 | 1（**由公会 GSP 购买解锁**：[purchase gsp] 1 / [purchase gold] 100000，与 200 同族） | skl [purchase gsp] / [purchase gold] |
| 最高等级 | 20 | skl [maximum level] |
| 类型 | passive（skill class 4） | skl [type] / [skill class] |
| 一句话效果 | 增加公会成员的经验获得量（只适用于普通以上公会成员） | skl [explain] |
| 图标 | `Character/Common/SkillIcon.img` #44/45（共通表） | skl [icon] |

**level info 1 列**：Lv1 = `1` → Lv20 = `20`（**+1%/级经验加成**，模板
`增加经验获得量 : <int>%%` 向量 `-1 0 1.0` 直读——列语义确定）。

## 2. 技能逻辑走读（极简）

**无任何脚本**（实测同 200：load_state 无、passive_skill 无、白名单 grep `experienceup` 零命中）。
纯数据技能：引擎在经验结算处按此加成（+1% ~ +20%）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ExperienceUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ExperienceUp.skl` | ✅ | 1 列 × 20 级 + 购买消耗 |
| 注册行 / 被动注册 / nut | — | load_state / passive_skill_swordman.nut / swordman 目录 | ⛔ 均无（引擎内置） | — |
| .ani / .atk / PO | — | 各资源目录 | ⛔ 无专属 | — |
| 姊妹技能 | StatusUp.skl（200） | `…\skill\Swordman\StatusUp.skl` | ✅ | 公会四维（200 文档） |

## 4. 资源需求

必需级缺失 img = **0**（无任何资源文件）。

## 5~7. 实现判定（⛔ 级一段话）

**无经验系统**：我方 demo 无角色等级/经验概念（无经验值存储、无升级、无升级奖励）——
"经验获得量 +N%"没有可作用的结算点。叠加公会系统（GSP 购买链）也不存在。
**不实现、不建议立项**；本文档仅存档（与 200 的差异：200 的四维属性尚可做面板试点，
本技能连面板落点都没有——故判定降为 ⛔ / D 类处理）。

## 8. 存疑与缺口上报

- 未考证：无（数据自洽、语义确定）。
- 分类纠偏：C → **D 倾向**（非战斗/系统技能；因批次安排仍按 C 级文档规格存档）。
- 新缺口：**经验/成长系统**——首次显式记档（此前 241 技能尚未撞过；若未来做角色成长，
  本技能与任务/等级系统一并评估）。
- 翻译缺口：`.skl` 子命令（全局已知项）。
