# 魔法暴击（MagicalCriticalHitUp）

> 技能ID 188 | 级别 C | 可实现性 ⛔（暴击系统缺失） | 分析日期 2026-08-22 | 批次 C4

> **同构技能**：与 186 物理暴击 / 189 魔法背击为三件套，全部走读结论（引擎内置判定、文件链实测、
> 系统缺口）见 `186-PhysicalCriticalHitUp.md` §2/§8——本文只记差异，不重复引用链证据。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 魔法暴击 | `skill\Swordman\MagicalCriticalHitUp.skl [name]` |
| 英文名 | MagicalCriticalHitUp（取 skl 文件名；[name2]="Magical Critical Hit" 官方英文名） | 同上 |
| 职业 | 全系共通（growtype 0-5；图标 Common 表 #38/#39） | 同上 |
| 学习等级 / 最高等级 | 20 / 20 | 同上 |
| 类型 | passive（skill class 4） | 同上 |
| 指令 / CD / MP | 无（纯被动） | 同上 |
| 一句话效果 | 增加自身魔法暴击率 +1%/级（Lv1 +1% → Lv20 +20%） | 同上 [explain] + [level property] |

**level property（1 列）**：模板 `增加魔法暴击率 : <float1>%%`，向量 `(-1, 0, 0.1)`；
col0：Lv1=10 → Lv20=200 → **+1.0% → +20.0%**。与 186 数值表完全同构（仅属性位不同）。

## 2. 技能逻辑走读

与 186 完全同构：load_state / passive_skill_swordman.nut / appendage / PO / 角色动画特效**全查无命中**
（本批实测，方法同 186 §2.1）。引擎内置：col0 直读进魔法暴击率 change-status，参与引擎魔法伤害暴击 roll。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MagicalCriticalHitUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MagicalCriticalHitUp.skl` | ✅ 实测 | 全部技能数据（唯一文件） |
| lst 条目 | swordmanskill.lst 157-158 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 188 → 本 skl |
| 注册行 / nut / appendage / PO / 动画 / 特效 | —（全查无，实测） | 同 186 §3 各路径 | ⛔ 缺失（引擎内置） | — |

## 4. 资源需求

零资源需求（同 186）。

## 5. 实现方案草案

⛔ 级免写（同 186 §5：暴击系统落地后仅一条 NumericType 数据行，
届时需区分 `PhysicalCritRate` / `MagicalCritRate` 两个键位）。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| MagicalCriticalHitUp.skl | `.skl` 无子命令（1 列） | 手抄 1 值；全局已知缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 魔法暴击率 +1%~20% | **缺失：暴击系统**（186 §8 上报；另注：我方尚无物理/魔法伤害分型，键位拆分落地时需一并定夺） | 暴击系统落地前不实现 |

## 8. 存疑与缺口上报

**未考证项**：无。

**新系统级缺口**：并入 186 §8 的**暴击系统**上报（本技能为其第 2 实证）。
补充记档：我方伤害无物理/魔法分型（HitReaction 无属性字段）——暴击键位若按 DNF 拆物理/魔法两轨，
伤害公式立项时需同步决策是否引入伤害分型（**伤害分型**与已记档的"元素属性系统"为相邻缺口）。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
