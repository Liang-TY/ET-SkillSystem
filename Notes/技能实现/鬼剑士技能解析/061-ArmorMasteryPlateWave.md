# 阿修罗板甲专精（ArmorMasteryPlateWave）

> 技能ID 61 | 级别 C | 可实现性 ⛔（三重缺失，同族模板 056） | 分析日期 2026-08-22 | 批次 C1
>
> 甲系九兄弟之一。注册链/引擎消费/判定/翻译/资源结论与 **056-ArmorMasteryHeavyBK.md（族模板）完全一致**，
> 本文只列本技能差异；共通部分（load_state 无注册、passive_skill_swordman.nut 无分支、无 nut/appendage/
> PO/动画/特效/.chr、纯引擎内置、⛔ 三重缺失论证）不重复，见模板 §2/§3/§5。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 阿修罗板甲专精 | `ArmorMasteryPlateWave.skl [name]` |
| 英文名 | ArmorMasteryPlateWave（[name2]=`Asura's Plate Armor Mastery`） | 同上 |
| 职业 | 阿修罗（fitness growtype 4；growtype maximum level `0 0 0 0 1 0` 仅本系） | 同上 |
| 学习/最高等级 | 1 / 1（无 level property，数值全在 static data） | 同上 |
| 类型 | passive（skill class 4），无指令/CD/MP，购买代价 0 | 同上 |
| 一句话效果 | 装备板甲时增加智力、精神、MP恢复、MP最大值、HP最大值和施放速度，消除 MP恢复/施放/攻速惩罚；随件数增大 | 同上 [explain] |

## 2. 与族模板的差异

| 项 | 056（狂战重甲专精） | 本技能 061（阿修罗板甲专精） |
|---|---|---|
| 甲系 | 重甲 | 板甲 |
| 加成属性 | 力量/精神/MP恢复/MP最大/HP最大/物防 | 智力/精神/MP恢复/MP最大/HP最大/**施放速度** |
| static data | 17 列 | **26 列**：`0 0 0 0 20 10 8 20 0 50 100 150 0 20 0 0 0 0 100 0 0 0 0 0 0 0`（九兄弟最长；col18=100 为本技独有段，语义未考证，疑施放速度位） |
| pvp 变体 | col9 50→0 | **无 [pvp] 数值差异**（文件有 [pvp] 节但为空） |
| 图标 | SkillIcon.img 126/127 | SkillIcon.img 184/185（`character\swordman\effect\` 下实测路径） |

对照模板 §2.3 推断法：与 187 布甲精通同有 col4/5=`20 10`（两技 explain 均含智力——互证 col4/5=智力位，推断）。

## 3~8. 关联文件 / 资源 / 草案 / 翻译 / 困难 / 上报

全部同族模板 056 对应节：零资源（缺失 img=0）、⛔（装备系统+数值键+消费链三重缺失，
面板无条件版为不推荐的 🔶 降级形态）、翻译缺口仅 `.skl` 子命令 1 条（全局在案）、不占号段。
本技能独有存疑：static col18=100 与尾部 7 个 0 列的语义；无其他新增缺口。
