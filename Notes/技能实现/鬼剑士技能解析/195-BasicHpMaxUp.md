# 生命之源（BasicHpMaxUp）

> 技能ID 195 | 级别 C | 可实现性 🔶（面板版——本批唯一非 ⛔ 者） | 分析日期 2026-08-22 | 批次 C1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 生命之源 | `skill\Swordman\BasicHpMaxUp.skl [name]` |
| 英文名 | BasicHpMaxUp（[name2]=`The source of life`） | 同上 |
| 职业 | 鬼剑士全系（fitness growtype `0 1 2 3 4 5`；growtype maximum level 全 20） | 同上 |
| 学习等级 | **0**（[required level range] 1；purchase cost 0——1 级起可学的通用被动） | 同上 |
| 最高等级 | 30（各成长段实际可用 20） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | passive（skill class 4），无指令/CD/MP | 同上 |
| 一句话效果 | 增加 HP 最大值（Lv1 +11% → 高级 +70%） | 同上 [explain] + level info |

**level property（1 列，[dungeon][level info] 44 行全表实测）**：
模板 `HP最大值变化量 : <int>%%`，向量 `(-1, 0, 0.1)` → 显示值 = 列值 × 0.1（百分比）。
- 名义等差 +10/级（+1%/级）：Lv1=110(+11%)、Lv2=120、…；
- **4 个异常格（疑 mod 改值/数据污染，未考证）**：Lv6=`1000`(+100%)、Lv14=`2400`(+240%)、
  Lv15=`560`、Lv16=`570`（打断了 Lv17=`430` 的单调性——非等差插入，疑编辑残留）；
- Lv17 起恢复 +10/级 至 Lv44=`700`(+70%)；最高等级 30 → 实际消费到 Lv30=`560`(+56%) 一档。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测，全空）

- `swordman_load_state.nut` grep `basichpmaxup` **无命中**；白名单 `sqr\character\swordman\` 全树 grep **无命中**；
- `passive_skill_swordman.nut` switch（全文走读）**无 case 195**；
- `passiveobject\`/角色 `animation\`/`effect\animation\`/`swordman.chr`（grep 计 0）全无条目。

**结论：纯引擎内置被动**——引擎把列值 × 0.1 作为百分比加到 HP 最大值面板（五层公式的
MaxHp 百分比位），无任何表现资源。

### 2.2 机制归纳

```
学习（等级 0 起，SP 购买型被动）→ 永久生效：
  HP 最大值 + 列值×0.1 %（Lv1 +11%，名义 +1%/级，至 +56%~70% 档）
无施放、无动画、无刷新——一次性面板改写
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BasicHpMaxUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BasicHpMaxUp.skl` | ✅ 实测（109 行） | 1 列 × 44 行数值表 |
| lst 条目 | swordmanskill.lst 195-196 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 195 → 本 skl |
| 注册行 / 被动注册表 | — | `…\pvf\sqr\character\swordman_load_state.nut` / `…\passive_skill_swordman.nut` | ⛔ 均无（引擎内置） | — |
| 主 nut / appendage / PO / 动画 / 特效 / .chr | — | 白名单各目录 | ⛔ 全无 | 纯数据被动 |
| 图标 | ATSkillIcon.img 112/113 | `Character/Fighter/Effect/ATSkillIcon.img`（**格斗家图标表**——跨职业通用技能借表，175 Common 表之后第二种借表形态） | ✅ 路径实测 | 仅 UI |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无） | — | — | — | — |

**零资源需求**：无任何 .ani/.als/.atk/.obj；缺失 img = 0。

## 5. 实现方案草案（🔶 面板版）

**本批唯一"两半都基本能做"的被动**——因为它的效果只落在 HP 最大值上，而这条链是通的：

- **数值键**：`NumericType.MaxHp = 1002` **五层齐全**（Base/Add/Pct/FinalAdd/FinalPct，NumericType.cs 全文实测）；
- **消费点（真实存在，grep 实测）**：`SkillCastHelper.cs:32-33` 的 HP 施法门禁读
  `NumericType.Hp / MaxHp` 算百分比（MinCastHpPct 的分母）；`LSCastViewComponentSystem.cs:86`
  视图层读 Hp 做 HP 条/伤害数字——MaxHp 抬高后门禁百分比即时变化；
- **挂载点**：被动（非施放技）不走 SkillLogic——**出厂数值注入**：`LSUnitFactory.cs:20-21`
  （现在 `Set(MaxHpBase, 1000)` 的位置）追加 `Set(MaxHpPct, 值)` 即成，或做成常驻
  `BuffDefinition`（无时长、AddActions 注入——届時需 BuffId，从 18 起，L18）。

### 内容件清单（面板版）

| 件 | 做法 |
|---|---|
| 无新 SkillLogic | 被动无施放——不占 SkillId/AnimId 号段 |
| 注入点 | `LSUnitFactory`（玩家单位数值初始化处）加一行 `MaxHpPct` 注入；或常驻 Buff（BuffIds 从 18 顺延占 1 个） |
| 数值 | DNF：Lv1 +11%、+1%/级、至 +56%（Lv30 实消费档）；demo 建议 **+20% 固定档**（演示面板与门禁分母变化足够） |

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎内置 HP%+ 被动 | 出厂 `MaxHpPct` 数值注入（或常驻 BuffDefinition） |
| 44 行等级表 | 固定档（等级系统延后） |
| 异常格（1000/2400/560/570） | 按"数据污染"忽略，取名义等差档（需用户定夺是否照抄 pvf 原值） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| Lv1 | +11% | +20%（固定档演示） |
| Lv30（实际最高消费档） | +56%（名义等差推算） | — |
| 作用 | MaxHp 百分比位 | 同（MaxHpPct 10023） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BasicHpMaxUp.skl | `.skl` 尚无子命令（1 列 × 44 行） | 名义等差可手抄 (110, +10, 700)；异常 4 格需人工决断；归入全局 skl 子命令需求 |

（无其他文件——翻译环节仅此 1 条，全局已知项，无新增缺口。）

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| SP 购买、等级成长 | 无技能学习/等级系统（延后档） | 固定档出厂注入 |
| +11%~56% HP max | **链路完整，无缺口**（键+五层+消费点全在） | 直做 |
| 学习时是否同步治疗现有 HP | 未考证（引擎行为） | 我们注入在出厂时点，天然无此问题 |
| 异常数值格 | pvf 数据疑污染（未考证） | 忽略或照抄由用户定夺 |
| 图标借格斗家表 | 无 UI 消费 | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. [dungeon][level info] 4 个异常格（1000/2400/560/570）成因——mod 改值或原版如此。
2. 最高等级 30 vs 表 44 行 vs growtype 上限 20 三者不一致的原因（多版本残留推断）。
3. 学习/升级瞬间是否按比例治疗现有 HP（引擎侧）。

**系统级缺口增量（主循环汇总）**
1. **被动技能的挂载形态**（本批共性，首次成文）：被动=非施放技，现框架只有 SkillLogic（施放型）
   与 BuffDefinition（限时挂载）两种载体——"学习即永久生效"建议定为**第三种出厂注入形态**
   （LSUnitFactory 数值段或常驻 Buff），供全部 C 类被动复用。**这是本批 12 技共同的实现期决策点**。
2. HP 系现状备注：MaxHp 有消费点（门禁/血条），但 **Hp 无 MaxHp 钳制/出生按 MaxHp 初始化的
   联动未逐处核验**（出厂 HpBase=MaxHpBase=1000 同值写死，LSUnitFactory 实测）——若被动先落地，
   建议顺带把"出厂 Hp=MaxHp final"改成联动（一行改动，面板版完整性受益）。

**翻译工具缺口**：`.skl` 子命令（全局在案项）。
