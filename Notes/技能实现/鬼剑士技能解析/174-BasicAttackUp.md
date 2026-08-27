# 基础精通（BasicAttackUp）

> 技能ID 174 | 级别 C | 可实现性 ⛔（普攻伤害无属性加成注入口 + 攻击数值零消费） | 分析日期 2026-08-22 | 批次 C1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 基础精通 | `skill\Swordman\BasicAttackUp.skl [name]` |
| 英文名 | BasicAttackUp（[name2]=`Basic Attack Power Up`） | 同上 |
| 职业 | 鬼剑士全系（fitness growtype `0 1 2 3 4 5`；growtype maximum level 全 1——**全系通用的基础被动**） | 同上 |
| 学习等级 | 1（[required level range] 1；无前置） | 同上 |
| 最高等级 | **100**（本批最高等级面；随角色等级成长，自动升级型） | 同上 [maximum level] |
| 类型 | passive（skill class 4），无指令/CD/MP | 同上 |
| static data | `1`（语义未考证） | 同上 |
| feature skill index | `161`（未考证，疑关联特性/TP 系统） | 同上 [feature skill index] |
| 一句话效果 | 增加基本攻击、前冲攻击、跳跃攻击的攻击力 | 同上 [explain] |

**level property（3 列 × 100 级，等差数列，全表实测）**：
- 模板：`基本攻击力增加量/前冲攻击力增加量/跳跃攻击力增加量 : <float1>%%`，三列向量均为 `(-1, col, 0.1)`
  ——**三列同值**，即三类攻击同额加成；
- dungeon：Lv1 = `0 0 0`，此后每级 +48 → Lv100 = `4752`；×0.1 → **0% → 475.2%（+4.8%/级）**；
- pvp：**全等级恒 100**（= +10%，pvp 平衡压平）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测，全空）

- `swordman_load_state.nut` grep `basicattackup` **无命中**；白名单 `sqr\character\swordman\` 全树 grep **无命中**；
- `passive_skill_swordman.nut` switch（全文走读）**无 case 174**；
- `passiveobject\`/角色 `animation\`/`effect\animation\`/`swordman.chr`（grep 计 0）全无条目。

**结论：纯引擎内置被动**——引擎在计算**普攻三件套**（基本/前冲/跳跃攻击）伤害时，
按 `基础精通等级 × 4.8%` 乘算加成（伤害公式引擎侧，无脚本）。这与 DNF "普攻伤害随版本大幅膨胀"
的设计一致（475% 上限说明普攻基值很低、靠本被动抬）。

### 2.2 机制归纳

```
随角色等级自动升级（Lv1 起，100 级封顶——无学习交互，purchase cost 0）：
  基本攻击伤害 + (等级-1)×4.8%   （Lv1=0%，Lv100=475.2%）
  前冲攻击伤害 同额
  跳跃攻击伤害 同额
  pvp 场景恒 +10%
消费端 = 普攻伤害公式（引擎）——非面板属性，直接改三个攻击的伤害基数
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BasicAttackUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BasicAttackUp.skl` | ✅ 实测（274 行） | 3 列 × 100 级数值表 |
| lst 条目 | swordmanskill.lst 175-176 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 174 → 本 skl |
| 注册行 / 被动注册表 | — | `…\pvf\sqr\character\swordman_load_state.nut` / `…\passive_skill_swordman.nut` | ⛔ 均无（引擎内置） | — |
| 主 nut / appendage / PO / 动画 / 特效 / .chr | — | 白名单各目录 | ⛔ 全无 | 纯数据被动 |
| 图标 | SkillIcon.img 76/77 | `Character/Common/SkillIcon.img`（共通表，全系通用技能，175 文档经验一致） | ✅ 路径实测 | 仅 UI |
| 关联强化 | BasicAttackUpEx.skl（ls 实测存在） | `…\pvf\skill\Swordman\` | ✅ 存在 | E 类强化技（他批处理） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无） | — | — | — | — |

**零资源需求**：无任何 .ani/.als/.atk/.obj；缺失 img = 0。

## 5. 实现方案草案（⛔ 级）

**⛔ 暂缓——伤害端无注入口，面板端零消费，两半都不通**：

1. **伤害端**：效果 = 普攻三件套伤害 ×(1+X%)。我们的伤害 = `MeleeHitAction` 读
   `HitReaction.Damage` **固定值**（属性数值无伤害消费链，在案缺口本技第 N 次实证）——
   没有任何"百分比伤害加成"的乘算位。
2. **攻击范围端**：前冲攻击/跳跃攻击两个消费场景本身还分别压在**位移系统/跳跃系统**（在案缺口）上；
   基本/前冲攻击的载体（普攻状态机）在 demo 中以固定伤害技能近似，同样无加成位。
3. **面板端**：`NumericType.Attack`（Base/Add/Pct 三层）**全仓库零消费**（grep 实测）——
   就算把 +475.2% 写进 AttackPct，任何代码都不读它。

**届时形态（伤害消费链立项后，零新机制）**：MeleeHit 改读 source 单位 NumericType.Attack 五层值
（门面 `LSActionContext.GetSourceId` 已在）→ 本被动 = 出厂 `Set(AttackPct, 4.75)`（或等级档位），
一条数值注入即完成。届时不占号段（无施法行为；数值键已有 1003 系）。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BasicAttackUp.skl | `.skl` 尚无子命令（**3 列 × 100 级 = 300 个数据点**——手抄不现实，本批对 skl 子命令需求最迫切的样本） | 等差数列可压缩为 (首项, 公差, 末项) 三元组表达；归入全局 skl 子命令需求 |

（无其他文件——翻译环节仅此 1 条，全局已知项，无新增缺口；但量大，值得在 skl 子命令设计时单列"等差表压缩"。）

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 普攻伤害 +0%→475.2% | **缺失：属性伤害消费链**（MeleeHit 固定值） | 消费链立项后零成本接入；之前不做 |
| 前冲/跳跃攻击加成 | **缺失：跳跃系统**（在案）；前冲攻击载体未定型 | 先只覆盖基本攻击 |
| 100 级等差成长 | 等级缩放延后 | 固定档（如 +48% 演示） |
| pvp 恒 10% | 无 pvp 场景 | — |
| static 1 / feature skill index 161 | 未考证 | — |

## 8. 存疑与缺口上报

**未考证项**
1. static data `1` 与 [feature skill index] `161` 的语义（疑特性/TP 系统关联）。
2. 引擎侧加成作用点（乘算伤害基数 vs 面板攻击力——explain"攻击力"+pvp 压平的形态更像直接乘伤害，无脚本可证）。
3. 升级触发（随角色等级自动 or 系统赠送——无 [required level] ��构支持，purchase cost 0 推断免费自动）。

**系统级缺口增量（主循环汇总）**
1. **属性伤害消费链的第 6 实证**（在案缺口，本技形态最直接：纯百分比、目标明确=普攻三件套）——
   建议该缺口立项时把本技列为第一个验收用例（改动面小、效果可即时观测）。
2. 等差 level info 表（3×100）对 `.skl` 子命令的数据量需求——翻译工具设计输入（见 §6）。

**翻译工具缺口**：`.skl` 子命令（全局在案项；本技为其最大数据量用户）。
