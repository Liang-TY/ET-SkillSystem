# 恐惧光环（DarkAura）

> 技能ID 89 | 级别 C | 可实现性 ⛔（属性数值无伤害消费链 + 元素抗性/攻速系统缺失） | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 恐惧光环 | `skill\Swordman\DarkAura.skl [name]` |
| 英文名 | DarkAura（取 skl 文件名；[name2]="Fear of Darkness" DNF 官方英文名） | 同上 |
| 职业 | 鬼泣二觉被动（[second growtype maximum level] 槽 5-6 非零 = 鬼泣一觉/二觉段；[skill fitness second growtype] `1 2`；剑魂/鬼泣/狂战/阿修罗/剑影四被动 91/89/90/92/209 槽位 3-12 依次排开，互证成立） | 同上 |
| 学习等级 | 48 | 同上 [required level] |
| 最高等级 | 50（二觉段上限 30×2 槽） | 同上 [maximum level] / [second growtype maximum level] |
| 类型 | passive（skill class 3） | 同上 [type] |
| 指令 / CD / MP | 无（常驻被动光环） | 同上 |
| static data | dungeon `550`（光环半径 550px）；pvp 无 static（半径同 550 推断） | 同上 [static data] |
| 一句话效果 | 增加自身基本/技能攻击力；散发黑暗光环降低周围敌人暗属性抗性与攻击速度 | 同上 [explain] |

**level property（3 列，Lv1 → Lv50，34 级实测）**：模板
`光环范围<int>px / 降低暗属性抗性<int> / 降低攻击速度<float1>%% / 基本攻击力和技能攻击力增加<float1>%%`，向量：
范围=`(0,0,1.0)`→static[0]=**550px 恒定**；col0=暗抗降低 5 → 84；col1=攻速降低 40→700 ×0.1 = **4.0% → 70.0%**；
col2=攻击力增加 100→440 ×0.1 = **10.0% → 44.0%**。
pvp 段仅 2 列（暗抗 4→84、攻速 1.0%→35.5%——攻击力增益列被砍，PvP 平衡惯例）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全查实测，无一处命中）

- `swordman_load_state.nut`：grep `darkaura` 无命中；
- `sqr\character\swordman\` 全树（含 `passive_skill_swordman.nut` 无 case 89、`appendage\` 7 文件、`beidong\`）：无命中；
- `sqr\character\JG_SwordMan\`：唯一命中 `swordghost22\ap_erjue.nut` 的 `"mm_darkaura"` 是**粒子贴图名**
  （借 mage 召唤系路径 `at_mage_summon_nutouwang\...ptl`，剑影二觉演出用），与本技能无关——排除；
- `passiveobject\character\swordman\`、`character\swordman\effect\animation\`（grep darkaura/恐惧）：无资源文件。

**结论（F3 引擎内置）**：光环的目标枚举、debuff 挂载、数值写入全在引擎 appendage 管线内
（`pvf\appendage\` 大树无 skill 名可对——按红线不检索，记未考证）。pvf 侧只有 .skl 数据。

### 2.2 机制归纳

```
学得即常驻：
  自身：基本/技能攻击力 + col2×0.1%（+10% → +44%）
  以自身为中心半径 550px 光环：
    敌方：暗属性抗性 - col0（5 → 84）
          攻击速度 - col1×0.1%（4% → 70%）
    （"恐惧"= 表现文案；实际减益就是暗抗+攻速两项）
```

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DarkAura.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DarkAura.skl` | ✅ 实测 | 3 列 + static 550 |
| lst 条目 | swordmanskill.lst 269-270 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 89 → 本 skl |
| 注册行 / 主 nut / appendage / PO / 动画 / 特效 | —（全查无，实测） | 同 186 §3 各路径 | ⛔ 缺失（引擎内置） | 光环管线在引擎 |
| 图标 | SkillIcon.img #196/#197 | `Character/Swordman/Effect/SkillIcon.img` | ✅ 实测（路径） | 无 UI 消费 |

## 4. 资源需求

**零资源需求**（光环视觉为引擎内置绘制，pvf 无专属文件——与 030 暗月降临"引擎通用光效"同形态）。

## 5. 实现方案草案

⛔ 暂缓——核心语义依赖三条缺失能力：
1. **属性数值无伤害消费链**（176 §8 缺口：攻击力 +10%~44% 无处生效）；
2. **元素属性/抗性系统**（030 §5 同撞：暗抗 -5~-84 无数值位无公式）；
3. **攻速系统**（R5-B5 记档"攻速/施放速度系统"：攻速 -4%~-70% 无消费方；连 NumericType 键位都没有）。

届时形态（供立项参考）：常驻光环 = 以自身为中心、跟随施法者（R4-B17 "Area 跟随施法者"缺口）
的 Tick Area，EnterActions 给敌方挂 DebuffBuff（暗抗/攻速两数值 Action 对）；自身攻击力走 NumericType。
若阵营系统未就绪，PvE 单人 demo 期 GetEnemies 可近似（全场敌方）。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| DarkAura.skl | `.skl` 无子命令（3 列×50 级 + static 550） | 手抄 3 组值；全局已知缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 攻击力 +10%~44% | **缺失：属性伤害消费链**（第 7 实证） | 公式立项前不做 |
| 暗抗 -5~-84 | **缺失：元素属性系统**（030 同撞） | 同上 |
| 攻速 -4%~-70% | **缺失：攻速系统**（R5-B5 记档；本技能为"敌方攻速减益"消费方） | 攻速系统立项时列入消费方清单 |
| 光环半径 550px 常驻跟随 | 延后：Area 跟随施法者（R4-B17 已记档） | 逐拍建区过渡范式可用 |
| 恐惧光环视觉 | 引擎内置无文件（无来源） | 跳过或自做 tint/overlay 近似 |

## 8. 存疑与缺口上报

**未考证项**
1. 光环 debuff 的刷新规则（进出半径是否即时增减；重复进入是否叠加）——引擎 appendage 管线无脚本可考。
2. pvp 无 static 段的光环半径取值（推断沿用 550）。
3. "恐惧"是否另有恐惧状态效果（explain 全文只提暗抗/攻速，判为纯文案；引擎侧未考证）。

**新系统级缺口**：无新上报（三条主缺口均已在档；本技能把"攻速系统"的**减益向**消费方补入清单）。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
