# 巨剑精通（HeavySwordMastery）

> 技能ID 14 | 级别 C | 可实现性 🔶（属性面板可表达 / 消费端卡死，同 012 分半） | 分析日期 2026-08-22 | 批次 C2

**同族对照主文档 = `012-ShortSwordMastery.md`**（共性结论全部适用，本文只记巨剑差异）。

## 1. 基本信息（差异项）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名/英文名 | 巨剑精通 / HeavySwordMastery（[name2] 实测 `Heavy Sword Mastery`） | skl |
| growtype 上限 | 同族模式（剑魂代管 1 级，其余 30） | skl |
| dungeon static data | `5 3 7 50 5 3 400 150 1000`（9 值） | skl |
| level info | **11 列** × 50 级；Lv1：`12 3 108 216 320 18 1080 100 54 97 12` | skl |
| 模板前三占位 | 物攻增加比率 / 魔攻增加比率 / 物理命中增加比率（×0.1 向量；列位 col0 / col10 / col1——见 012 对照表注） | level property 模板实测 |
| 附加效果（Lv3） | 银光落刃自身霸体 + 多段攻击；里·鬼剑术 2 次斩击并可接拔刀斩 | explain |
| 附加效果（Lv5） | 格挡生成冲击波 + 概率眩晕（与短剑同构，参数独立） | explain |
| 附加效果（Lv7） | 破军升龙击后追加蓄气型捶击 + 冲击波 | explain |

**level property 11 占位（实测）**：前三属性 → [格挡]冲击波物攻%/眩晕机率%/眩晕Lv/眩晕时长秒 →
[银光落刃]多段物理% → [破军]捶击物攻% ~~%（区间值）→ [里鬼]蓄气上限攻击力增加率% →
[拔刀斩]蓄气上限攻击力增加率%。

## 2. 走读差异

- 注册/脚本/引擎内置结论同 012 §2.1（实测同批 grep 覆盖）。
- 附加效果消费点分散在 **银光落刃 16 / 里鬼 67 / 拔刀斩 9 / 格挡 1 / 破军 68**（对应 A 批文档
  016/067/009/001/068 已存在）。
- 特有机制：**蓄气型捶击**（破军追加）与**蓄气上限攻击力**（里鬼/拔刀列）——按住蓄力输入缺失
  （L21 记档的共性简化：蓄力技统一瞬发），这两项在 demo 侧先天降档。
- 霸体（银光落刃）撞霸体帧缺口（§6.3 延后档）。
- 无专属视觉资源目录（effect\animation 无 heavyswordmastery 命名，实测 ls）；
  `shortswordmastery\heavyweaponcombochargedodge.ani` 文件名含 heavy 但位于短剑目录（里鬼蓄气
  dodge 层复用，**归属未考证**）。

## 3. 关联文件清单（差异行）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | HeavySwordMastery.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\HeavySwordMastery.skl` | ✅ | 11 列 × 50 级 + static 9 值 |
| 特效 / PO | —（无专属目录） | `…\character\swordman\effect\animation\`（ls 实测） | ⛔ 无 | — |
| 其余 | 同 012 §3 | — | 同 | — |

## 4. 资源需求

必需级缺失 img = **0**（同 012；heavyweaponcombochargedodge.ani 归属存疑不计入）。

## 5~7. 实现草案与困难（同 012，差异两条）

1. 格挡冲击波 + 眩晕：与短剑同构（001-Guard 是被改写方）；冲击波可用 `AreaDefinition`
   （EnterActions={MeleeHit} + ProcBuffId=Stun）表达——**这一族附加效果里机制最可直接还原的一项**，
   前提仍是精通等级查询 + 格挡技能分支。
2. 蓄气型捶击/蓄气上限增伤：按住蓄力输入缺失（L21 共性简化），demo 若做则瞬发取满档值。

## 8. 存疑与缺口上报

- 未考证：static 9 值逐位语义；[破军]捶击"低~高%"区间的运行时取值方式（蓄气时长映射，引擎内）；
  heavyweaponcombochargedodge.ani 的归属技能。
- 新缺口：无新增（012 已上报项全覆盖；蓄力输入/霸体/跳跃均为已知在档缺口）。
- 翻译缺口：`.skl` 子命令（全局已知项）。
