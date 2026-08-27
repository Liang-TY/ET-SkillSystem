# 心眼（StuckUpOnCounter）

> 技能ID 92 | 级别 C | 可实现性 ⛔（破招判定 + 回避率判定双缺失） | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 心眼 | `skill\Swordman\StuckUpOnCounter.skl [name]` |
| 英文名 | StuckUpOnCounter（取 skl 文件名；[name2]="Mind's Eye" DNF 官方英文名） | 同上 |
| 职业 | 阿修罗二觉被动（[second growtype maximum level] 槽 9-10 非零 = 阿修罗段；名称即官方阿修罗二觉被动） | 同上 |
| 学习等级 | 48 | 同上 [required level] |
| 最高等级 | 50（二觉段上限 30×2 槽） | 同上 |
| 类型 | passive（skill class 1） | 同上 [type] |
| 指令 / CD / MP | 无（常驻被动） | 同上 |
| static data | 空（dungeon/pvp 均无） | 同上 [static data] |
| 一句话效果 | 增加攻击力；被破招攻击时增加自身回避率，不易被击中 | 同上 [explain] |

**level property（2 列，Lv1 → Lv50，向量全对位）**：
`增加回避率<float1>%%`←col0×0.1、`基本攻击力和技能攻击力增加<float1>%%`←col1×0.1。
- col0 回避率：60→**6.0%** → 944→**94.4%**（Lv50 mod 数值；pvp 20→2.0% 起）；
- col1 攻击力：100→10.0% → 490→49.0%。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全查实测，无一处命中）

grep `stuckupon`：`swordman_load_state.nut`、`sqr\character\swordman\` 全树
（`passive_skill_swordman.nut` 无 case 92）、`sqr\character\JG_SwordMan\`、
`passiveobject\character\swordman\`、`character\swordman\effect\animation\`——全部无命中。

**结论（F3 引擎内置）**：判定链全在引擎——受击时引擎检测本次攻击是否构成"破招"
（受击者正处于攻击/施法动作中被命中），是则给自身挂回避率增益（engine change-status），
下次受击 roll 时生效。

### 2.2 机制归纳

```
学得即常驻：
  自身基本/技能攻击力 + col1×0.1%（10% → 49%）
  被破招攻击（自己在攻击动作中被敌人命中）→ 自身回避率 + col0×0.1%
    （受击命中 roll 用回避率抵消后续攻击；具体持续/叠层规则未考证，§8）
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | StuckUpOnCounter.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\StuckUpOnCounter.skl` | ✅ 实测 | 2 列，无 static |
| lst 条目 | swordmanskill.lst 275-276 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 92 → 本 skl |
| 注册行 / 主 nut / appendage / PO / 动画 / 特效 | —（全查无，实测） | 同 186 §3 各路径 | ⛔ 缺失（引擎内置） | 破招/回避管线在引擎 |
| 图标 | SkillIcon.img #202/#203 | `Character/Swordman/Effect/SkillIcon.img` | ✅ 实测（路径） | 无 UI 消费 |

## 4. 资源需求

**零资源需求**（无专属视觉文件）。

## 5. 实现方案草案

⛔ 暂缓——双缺失 + 一前置：
1. **回避率判定**：`SkillContext.CheckHit` 实测为纯 AABB 相交（恒命中）——无命中 roll、无回避数值位；
2. **破招判定**：需要"受击者当前处于攻击动作"的目标状态查询（R2-A8"目标状态查询门面"已记档；
   R5-B3 逆转反击的"受击触发窗口"同族——本技能为其第 3 消费方）；
3. **受击管线钩子**（R3-A15 起已累计 6+ 消费方）：增益挂在"被命中"事件上，需受击侧注入点。
4. 攻击力增益部分撞属性消费链（176 §8）。

届时形态：受击钩子判定 caster 施法中 → `AddBuffToSelf(DodgeBuff)`（ForbidMoveOn/Off 同构的
NumericType.Dodge 挂摘）；CheckHit 改为 roll（回避率 vs 命中率——与 090 命中率互为镜像，一对键位一并立项）。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| StuckUpOnCounter.skl | `.skl` 无子命令（2 列×50 级） | 手抄 2 组值；全局已知缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 被破招攻击时回避率 +6%~94% | **缺失：回避率判定**（CheckHit 恒命中，实测无回避 roll；090 命中率镜像） | 命中/回避 roll 随暴击系统一并向伤害公式立项评估 |
| 破招判定（攻击动作中被命中） | **缺失：目标状态查询门面**（R2-A8）+ 受击管线钩子（R3-A15 族） | 并入受击管线钩子立项（消费方 +1） |
| 攻击力 +10%~49% | **缺失：属性伤害消费链**（第 10 实证） | 公式立项前不做 |
| 静态视觉（心眼/洞察） | 引擎内置无文件 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. 回避率增益的持续/叠层规则（explain 一句话无时限；疑触发后限时 or 常驻累积——无脚本可考）。
2. "被破招攻击"的精确窗口（仅攻击动作 or 含施法/读条——DNF 语义为攻击动作中，未在本 pvf 实证）。
3. pvp col0 起点 2.0%（dungeon 6.0%）差异未考证（平衡惯例）。

**新系统级缺口**：**回避率/命中率判定对**（090 与本技能互为镜像消费方——建议在 186 §8 的
伤害公式立项里加"命中 roll"一档：CheckHit 相交后 roll(命中率-回避率)，一次改动双技能族解锁）。
破招判定并入受击管线钩子+目标状态查询既有缺口，消费方各 +1。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
