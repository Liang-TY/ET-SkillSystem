# 暗月降临（DarkToleranceUp）

> 技能ID 30 | 级别 B（**预判纠偏：A → B**，主动状态/增益技，skill class 3，无攻击逻辑） | 可实现性 ⛔（元素属性系统缺失） | 分析日期 2026-08-22 | 批次 A3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 暗月降临 | `skill\Swordman\DarkToleranceUp.skl [name]` |
| 英文名 | DarkToleranceUp（取 skl 文件名）；[name2]="The Curtain of Moon" 为英文别名（本 pvf 少见的真英文 name2） | 同上 [name2] 实测 |
| 职业 | 鬼泣（[skill fitness growtype] 2，仅此一系；各觉醒段上限 10） | 同上 |
| 学习等级 | 15 | 同上 [required level] |
| 最高等级 | 20 | 同上 [maximum level] |
| 类型 | active（**skill class 3 = 增益/状态类**——预判纠偏依据） | 同上 [type] / [skill class] |
| 指令 | ↑↑ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 10000 ms（固定） | 同上 [cool time] |
| 读条 | casting time 400 ms | 同上 [casting time] |
| MP | 60 → 504（Lv1 → Lv20；pvp 减半） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `250 500`（pvp `350 500`。语义未考证：250/350 疑为增益光环半径 px，500 未考证） | 同上 [static data] |
| 一句话效果 | 增加自身及周围**队友**的暗属性抗性，并**减少周围敌人**的暗属性抗性，持续 20 秒 | 同上 [explain] |

**level property（3 列，Lv1 → Lv20 首末值，语义模板直读）**：`20000(恒)`、`3→60`、`3→60`。
col0 = 持续时间 ms（20000 = 20s；pvp 10000）；col1 = 增加暗抗（自身+队友）；col2 = 减少暗抗（周围敌人）。模板 `持续时间<float1>秒 / 增加暗属性抗性<int> / 减少暗属性抗性<int>` 与数据完全对位——**列语义零存疑**（本批少见的全明样本）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

- `swordman_load_state.nut` **无注册行**（grep `darktolerance` 无命中）；白名单 `sqr\character\swordman\` 递归 grep `darktolerance` 亦无（含 appendage 目录）；F2 三招（按数字 ID 反查/按文件名搜/共用状态）均空。
- **完全引擎内置**（F3 型，且比 064 更彻底）：无 nut、无 .chr 条目（grep `moon|tolerance|curtain` 仅命中月光斩/月魂斩等无关技能）、无 .obj、无角色施法 .ani（character\animation 无对应文件，avatar 换装层同样无 darktoler* 文件——老增益技用引擎通用施法姿态，具体动画未考证）。
- pvf 侧数据仅两处：`.skl`（上表）+ 特效动画 5 份（§2.3）。

### 2.2 引擎内置行为重建（.skl 数据 + 特效资源推断）

```
施放（读条 400ms 完成）：
  播通用施法姿态（未考证）
  对自身+周围队友（半径疑 250px）挂增益 appendage：暗抗 +col1，时长 col0（20s）
  对周围敌人挂减抗 appendage：暗抗 -col2，时长同
  播月幕特效（mooncurtain 系列，§2.3）
冷却 10000ms
```
增益/减抗的实现载体为引擎 appendage（`pvf\appendage\` 大树内，无确切子路径不做检索——按红线记"未考证"）。暗抗数值进 DNF 属性抗性公式（元素伤害减免），我们无对应系统。

### 2.3 特效资源（`character\swordman\effect\animation\` 根目录实测 5 份）

| 动画 | 帧数 | 总时长 | 引用 img | 备注 |
|---|---|---|---|---|
| `mooncurtain_upper.ani` | 9 | 720ms（80×9） | `Effect/DarkToleranceUp/mooncurtain_upper.img` | 月幕上层 |
| `mooncurtain_under.ani` | 9 | 720ms | `.../mooncurtain_under.img` | 月幕下层 |
| `mooncurtain_eff_upper.ani` | 9 | 720ms | `.../mooncurtain_eff_upper.img`（F1 交错引用 eff_under） | 上层强化闪 |
| `mooncurtain_floor.ani` | 7 | 560ms | `.../mooncurtain_floor.img` | 地面月阵 |
| `mooncurtain_eff_under.ani` | 9 | 720ms | `.../mooncurtain_eff_under.img` | 下层强化闪 |

全部无 SET FLAG / 无攻击盒 / 无 .als 边车（实���）——纯视觉叠加（上层/下层/地面三层 + 淡入淡出强化层）。挂接方式（帧号/层号）无 .als 可查，推断为引擎施放瞬间一次性播放（720ms ≈ 读条 400ms + 施放动作）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DarkToleranceUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DarkToleranceUp.skl` | ✅ 实测 | 技能数据（3 列全明） |
| 注册行 | — | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | F2/F3 全查 |
| 主 nut / ap nut | — | `…\pvf\sqr\character\swordman\`（递归 grep） | ⛔ 缺失 | 同上 |
| .chr 条目 | — | `…\pvf\character\swordman\swordman.chr` | ⛔ 无相关条目（实测） | 施法动画引擎通用，未考证 |
| 角色 .ani / .atk | — | `…\pvf\character\swordman\animation\` / `attackinfo\` | ⛔ 缺失（增益技无攻击文件） | — |
| 特效 .ani | mooncurtain_*.ani ×5 | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | §2.3 |
| 引擎 appendage | （未知） | `…\pvf\appendage\`（大树，无确切路径不检索） | 未考证 | 增益/减抗数值载体 |
| 装备层 | — | `…\pvf\equipment\character\swordman\avatar\`（find darktol* 无命中；moon* 命中均为月光斩/月魂斩） | ⛔ 无专属换装图层（实测） | 老增益技常态 |
| 关联强化 | —（无 Ex 文件，ls 实测） | `…\pvf\skill\Swordman\` | ⛔ 无 | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| mooncurtain_upper / under / eff_upper / eff_under / floor.img ×5 | sprite_character_swordman_effect_darktoleranceup.NPK（路径下划线化规则） | 月幕三层视觉 | 可选（技能 ⛔，视觉还原时再提） | ❌ 未入库 |

缺失 img：必需级 **0**（⛔ 技能无必需资源）；可选级 5 张（同一 NPK 一次提取）。

## 5. 实现方案草案

**⛔ 暂缓**——核心语义（暗属性抗性增减）依赖两条缺失系统级能力：
1. **元素属性/抗性系统**（§6.3 缺失档：无元素伤害、无抗性数值位、无抗性公式——暗抗 ±60 完全无处生效）；
2. **队伍/阵营判定**（§6.3 缺失档变体：SkillContext.GetEnemies 把"除自己外全部单位"当敌人——"自身及周围队友增益 + 敌人减益"的双向光环无法区分目标阵营）。

若未来元素系统落地，映射现成：`DarkToleranceUpSkill : SkillLogic`（读条跳过）+ `OnCast` 以自身为中心 `CreateArea`（半径 static[0] 疑 2.5 单位、Duration 20000ms）——Area `EnterActions` 按"阵营"分支挂两种 Buff（`BuffDefinition` 各带抗性数值 Action，Burn/Freeze 同构）+ 月幕三层 `ViewAnimId/ViewBackAnimId`。**无新机制需求，纯缺数值系统**。

**临时替身方案（不建议，仅备忘）**：若想先占按键演示视觉，可做成"自身减伤 X% + 敌人受到暗属性伤害增幅占位"——语义已偏离原版，按"方案先行"惯例需用户定夺后才做。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `DarkToleranceUp.skl` | `.skl` 尚无子命令（3 列 level info + casting time + static data） | 手抄 3 值即可（列语义全明）；`skl` 子命令同前议 |
| mooncurtain_*.ani ×5 | 仅 `[SHADOW]`（值 0）在规则表外 | 整节跳过无碍（064 已记 README 补记建议）；其余全为常规节 |

结论：特效资源**全部可被现有 ani 子命令翻译**；实质缺口仅 `.skl` 无子命令，计 1 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 暗抗增减（核心效果） | **缺失档：元素属性系统** | ⛔ 主因；落地前不做 |
| 自身+队友 / 敌人 双向光环 | **缺失档：队伍/阵营判定**（GetEnemies 无阵营过滤） | 元素系统落地时同步补阵营字段；单人 demo 期可先只做敌方减抗侧 |
| 400ms 读条 | 读条系统（延后档） | 跳过 |
| 月幕三层特效挂接（无 .als） | 无声明式来源（064 §8.2 同型，延后档） | 还原时手组装 overlay（releasewave 先例）或 Area 三层视图 |
| MP 60-504 | MP 系统（延后档） | 跳过 |
| static data 250/500 | 未考证 | — |

## 8. 存疑与缺口上报

**未考证项**
1. 引擎施法姿态动画（无 .chr 条目、无角色 .ani——老增益技通用姿态推断）。
2. `[static data] 250 500`（pvp 350 500）逐参语义（250/350 疑为光环半径 px）。
3. 增益/减抗 appendage 的确切文件与刷新规则（20s 内重复施放是否刷新——pvf\appendage 大树无路径不检索）。
4. 月幕特效的层号/挂帧（无 .als；720ms 一次性播放为推断）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **队伍/阵营判定**：`GetEnemies()` 现按"除自己外全部"——所有"队友增益/敌方减益"类光环技（本技能首撞，预计暗影鞭挞/觉醒被动的队伍增益等还会撞）都依赖。建议记缺失档；单人 demo 期可先用"全场敌方"近似敌方侧。
2. **属性抗性数值位**：即便不做元素伤害公式，"抗性 Buff"若要可显示/可查询，也需要 LSNumeric 预留属性键位——与元素属性系统同一立项。

**翻译工具缺口（并入主循环汇总）**：`.skl` 子命令（计 1 条，第 3 次重复印证）。
