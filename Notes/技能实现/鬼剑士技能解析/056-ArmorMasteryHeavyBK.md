# 狂战士重甲专精（ArmorMasteryHeavyBK）——甲系九兄弟模板档

> 技能ID 56 | 级别 C | 可实现性 ⛔（装备系统 + 数值键位 + 属性消费链三重缺失） | 分析日期 2026-08-22 | 批次 C1
>
> **本文档同时是"甲系专精/精通"九兄弟（56/61/93/94/172/173/185/187/196）的族模板**：
> 注册链/引擎消费机制/可实现性判定/翻译与资源结论九技完全同构，其余八份文档只列差异表并引用本文。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 狂战士重甲专精 | `skill\Swordman\ArmorMasteryHeavyBK.skl [name]` |
| 英文名 | ArmorMasteryHeavyBK（取 skl 文件名；[name2]=`Berserker's Heavy Armor Mastery` 为官方英文名） | 同上 |
| 职业 | 狂战士（[skill fitness growtype] 3；[growtype maximum level] `0 0 0 1 0 1` —— 第 5 槽剑影也为 1，疑 mod 改动或引擎惯例，未考证） | 同上 |
| 学习等级 | 1 | 同上 [required level] |
| 最高等级 | 1（无 [level property] 节——1 级被动，全部数值在 static data） | 同上 [maximum level] |
| 类型 | passive（skill class 4） | 同上 [type] / [skill class] |
| 指令 / CD / MP | 无（被动，无施放） | skl 无对应节 |
| 购买代价 | 0（免费被动） | 同上 [purchase cost] |
| static data | 见 §2.3 | 同上 [static data]（pvp 变体同文件 [pvp] 节） |
| 一句话效果 | 装备重甲系防具时增加力量、精神、MP恢复速度、MP最大值、HP最大值和物防，并消除 MP恢复/施放/攻速惩罚；效果随重甲件数增大 | 同上 [explain] |

## 2. 技能逻辑走读（族模板：九兄弟共用）

### 2.1 注册与文件链（三方实测，全空）

- `swordman_load_state.nut` grep `armormastery|mastery|...`（-i）**无命中**——无 pushState；
- 白名单 `sqr\character\swordman\` **全树递归** grep `armormastery|basichpmaxup|ghostpowerup` **无命中**——无主 nut、
  `appendage\` 目录（仅 7 个 ap_*.nut）无甲系 appendage 脚本；
- `passive_skill_swordman.nut`（294 行全文走读）：`ProcPassiveSkill_Swordman` 的 switch 只处理
  **248/254/252/123/171/209/119/78 + speedslashupper** 九个技能 ID——**甲系九兄弟的 ID 一个都不在册**；
- `passiveobject\character\swordman\` grep mastery 仅命中 `bluntmasterysub.obj`（钝器**武器**专精判定体，非甲系）；
- 角色 `animation\`、`effect\animation\`、`swordman.chr`（grep 计 0）均无任何条目——**被动无动画无特效无攻击信息**；
- 兄弟职业参照（F3 惯例）：`atswordman\passive_skill_atswordman.nut` / `demonicswordman\passive_skill_demonicswordman.nut`
  定点查 mastery——前者只有**武器**专精（shortsword/blade/heavysword/blunt 四把 ap_*.nut），后者 0 命中。
  **全职业脚本层面都不存在甲系精通逻辑。**

**结论（族级定性）**：甲系精通/专精是**纯引擎内置（C++）被动**——引擎在装备变化时按
`[static data]` 向量 × 已穿同系甲件数计算加成，直接写入属性面板。pvf 侧唯一数据源就是这份
static data 向量；效果数值与列语义的解码权在引擎，不在脚本。

### 2.2 机制归纳（引擎行为，按 skl + DNF 常识重建，推断已标注）

```
学习（等级 1 即自动生效，无需施放）：
  引擎遍历 5 个防具槽（上衣/头肩/裤子/鞋/腰带），
  每件"重甲系"防具：按 static data 向量累加对应属性（推断：多列 = 多属性，值多为"每件加成"）
  同件数下专精数值 > 通用精通（ClothSB explain 明说"比[布甲精通]更高"，static 对比可证，见 §2.3）
  穿非本系甲：承受引擎固有的 MP恢复/施放/攻速惩罚（精通 explain 的"惩罚效果消失"即抵消位）
换装 → 实时重算（引擎侧，无脚本参与）
```

### 2.3 static data 向量（九兄弟全表，**列语义引擎消费、pvf 无 nut 印证——未考证**）

| ID | 技能 | static data（dungeon） | pvp 差异 |
|---|---|---|---|
| 56 | 狂战重甲专精 | `20 10 0 0 0 0 6 15 0 50 100 150 0 20 200 0 0` | col9 50→0 |
| 61 | 阿修罗板甲专精 | `0 0 0 0 20 10 8 20 0 50 100 150 0 20 0 0 0 0 100 0 0 0 0 0 0 0` | 无 pvp 节差异 |
| 93 | 鬼泣布甲专精 | `0 0 6 15 0 0 0 0 0 100 100 100 0 20 0 500 0 0 24 12 0 0 30` | 同上无 |
| 94 | 剑魂轻甲专精 | `0 0 4 10 0 0 4 10 0 50 100 150 0 20 36 18 100 0 0` | 无 |
| 172 | 轻甲精通 | `0 0 0 0 0 0 0 0 0 0 100 100 0 20 20 10 100` | 无 |
| 173 | 重甲精通 | `10 5 0 0 0 0 0 0 0 0 100 100 0 20 0 0 0 0` | 尾部追加 `-40 -400 -400` |
| 185 | 剑影皮甲专精 | `20 25 6 15 0 0 4 10 80 300 100 100 0 0 200 100 100 0` | 完全相同 |
| 187 | 布甲精通 | `0 0 0 0 20 10 0 0 0 100 100 50 0 20 0 500 0 0 0 0 0 0` | 无 pvp 节差异 |
| 196 | 板甲精通 | `0 0 0 0 0 0 0 0 0 0 0 0 0 0 200 0 10 -10 -100 -100 0 0 0` | 尾部追加 `-50 -500 -500` |

**可交叉印证的两条推断**（对照 explain 属性清单）：
1. Heavy(173) col0/1 = `10 5` → HeavyBK(56) = `20 20`…实为 `20 10`——同系"精通 → 专精"同列翻倍，
   与 ClothSB explain "比布甲精通更高" 互证（专精=精通强化版，方向成立）；
2. Light(172) col14/15 = `20 10` → LightWM(94) = `36 18`：LightWM explain 比 Light 多"体力、精神"
   ——疑 col14/15 = 体力/精神每件加成（推断）。
其余列（含 Heavy/Plate 的 pvp 负值惩罚列、ClothSB 的 `500/24/12/30` 段）**精确映射未考证**。

## 3. 关联文件清单（每行实测；九兄弟通用）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ArmorMasteryHeavyBK.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ArmorMasteryHeavyBK.skl` | ✅ 实测（52 行） | 唯一数据源 |
| lst 条目 | swordmanskill.lst 57-58 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 56 → 本 skl |
| 注册行 | — | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | — |
| 被动注册表 | ProcPassiveSkill_Swordman | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ⛔ 无本 ID 分支（全文走读） | 在册的 9 个 ID 均非甲系 |
| 主 nut / appendage | — | `…\pvf\sqr\character\swordman\`（全树递归 grep） | ⛔ 缺失 | 逻辑在引擎 C++ |
| PO / .obj | —（bluntmasterysub.obj 为武器专精，无关） | `…\pvf\passiveobject\character\swordman\` | ⛔ 无 | — |
| 角色 .ani / .atk / .als / 特效 / .chr | — | `…\pvf\character\swordman\{animation,attackinfo,effect}`、`swordman.chr` | ⛔ 全无（grep 计 0） | 被动无表现资源 |
| 图标 | SkillIcon.img #126/#127 | `…\pvf\character\swordman\effect\SkillIcon.img`（UI 图集，非提取对象） | ✅ 路径实测 | 仅 UI |
| 装备层 | —（效果作用于防具，但属装备系统域） | `…\pvf\equipment\character\swordman\avatar\` | —（不做存在性统计） | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无） | — | — | — | — |

**零资源需求**（九兄弟同）：无任何 .ani/.als/.atk/.obj/.ptl；图标属 UI 图集不提取。缺失 img = 0。

## 5. 实现方案草案（⛔ 级，族级统一记档）

**三重系统级缺失**（引 §6.3 缺失档 + 本批新证）：

1. **装备系统**（§6.3"换装/武器切换"档的更底层形态）：我方**没有防具槽位、没有甲种概念**——
   "效果随重甲件数增加"的联动变量（0~5 件）根本不存在。这是第一阻塞。
2. **数值键位大面积缺失**：`NumericType.cs`（全文实测）仅有 Speed/Hp/MaxHp/Attack/Defense/
   ForbidMove/ForbidSkill——甲系要的力量/智力/体力/精神/MP最大值/MP恢复/攻速/施放速度/
   移动速度/硬直/物理暴击率/物防消费位**一个都没有**。
3. **属性消费链**（在案缺口，本族第 6~14 次实证）：Attack/Defense 数值键存在但**全仓库零消费**
   （grep 实测，伤害=HitReaction.Damage 固定值）；Speed 移动端零消费（R2-A7 在案）。

**能做的半截（面板无条件版，🔶 降级形态，不建议现在做）**：在 `LSNumericComponent` 补键后，
于 `LSUnitFactory`（数值初始化点，`HpBase/MaxHpBase` 在此 Set，实测）无条件注入固定加成——
但"随件数增长"语义全失，等价于一个永久属性 buff，与原版偏离过大。装备系统立项前不实现。

**届时不占号段**：无施法行为 → 不需要 SkillId/AnimId；若做成常驻 Buff 才需 BuffId（从 18 起，L18）。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ArmorMasteryHeavyBK.skl | `.skl` 尚无子命令（static data 向量 + pvp 变体） | 1 级被动手抄 17~26 个数即可；归入全局 skl 子命令需求（在案） |

（无 .ani/.als/.atk/.obj——翻译环节仅此 1 条，且为全局已知项，**无新增缺口**。九兄弟同。）

## 7. 困难与简化（族级统一）

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 穿重甲时多属性加成 | **缺失：装备系统**（无防具槽/甲种） | 装备系统立项前不做 |
| 加成随件数 0→5 增长 | 同上（联动变量不存在） | 无条件版（偏离大，不建议） |
| 力量/智力/体力/精神/MP/速度/暴击/物防 | **缺失：数值键位**（NumericType 无此键） | 与属性消费链合并立项 |
| 数值实际参与面板与伤害/防御公式 | **缺失：属性消费链**（Attack/Defense 零消费实测） | 同上（本族为该缺口最大用户群） |
| 非本系甲惩罚抵消 | 装备系统 + 惩罚数值位双缺 | — |

## 8. 存疑与缺口上报

**未考证项**
1. static data 各列精确语义（引擎 C++ 消费；本文两条交叉推断已标"推断"）。
2. HeavyBK/LightWM 的 [growtype maximum level] 第 5 槽（剑影）为 1——与"专精属单一职业"的原版认知不符，疑 mod 改动。
3. 引擎侧是否为每系专精生成独立 appendage 对象（`pvf\appendage\` 大树无路径不检索，红线）。

**系统级缺口增量（主循环汇总）**
1. **装备系统（防具槽位/甲种/件数联动）**——本族 9 技能的第一阻塞，首次成族撞上；
   建议在 §6.3 缺失档单列"装备系统（防具）"条目，与"属性消费链"分开记账（前者是联动变量缺失，后者是数值出口缺失）。
2. **属性数值键位缺口清单化**：力量/智力/体力/精神/MP 最大值/MP 恢复/攻速/施放速度/移动速度/硬直/物理暴击率/物防
   ——12 个键无一存在；补键是属性消费链立项的子任务。

**翻译工具缺口**：`.skl` 子命令（全局在案项，第 N 次印证；甲系为 17~26 列向量的批量需求方）。
