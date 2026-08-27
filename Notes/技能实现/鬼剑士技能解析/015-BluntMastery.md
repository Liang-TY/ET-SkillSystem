# 钝器精通（BluntMastery）

> 技能ID 15 | 级别 C | 可实现性 🔶（属性面板可表达 / 消费端卡死，同 012 分半；跳跃攻击冲击波���有跳跃系统缺口） | 分析日期 2026-08-22 | 批次 C2

**同族对照主文档 = `012-ShortSwordMastery.md`**（共性结论全部适用，本文只记钝器差异——
四精通中唯一带判定 PO 的一支）。

## 1. 基本信息（差异项）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名/英文名 | 钝器精通 / BluntMastery（[name2] 实测 `Blunt Mastery`） | skl |
| growtype 上限 | 同族模式（剑魂代管 1 级，其余 30） | skl |
| dungeon static data | `3 7 5 100 7 0 3 300 3 130 7`（11 值） | skl |
| level info | **12 列** × 50 级；Lv1：`12 4 350 112 144 36 72 200 102 12 97 2065` | skl |
| 模板前三占位 | 物攻/魔攻/物理命中增加比率（×0.1 向量；列位 col0 / col9 / col1——见 012 对照表注；钝器命中 col1 起步 4 高于族内 3） | level property 模板实测 |
| 附加效果（Lv3） | 后跳斩浮空冲击波；里鬼对眩晕敌增伤；银光落刃冲击波范围↑ | explain |
| 附加效果（Lv5） | **跳跃攻击命中敌人时生成物理冲击波**（空中连击不适用） | explain |
| 附加效果（Lv7） | 破军追加蓄气捶击+冲击波；幻影剑舞末击剑气改冲击波 | explain |

**level property 12 占位（实测）**：前三属性 → [后跳斩]浮空力 / [后跳斩]冲击波物攻 →
对眩晕状态敌人攻击力增加率% → [银光落刃]冲击波范围增加率% → [破军]冲击波物攻 / 捶击物攻%区间 →
跳跃攻击时的浮空力 / 冲击波物攻 → [幻影剑舞]冲击波物攻%。
向量含 3 处源 **-2**（L21：-2 = level 列引用的另一形态）。

## 2. 走读差异

- 注册/脚本/引擎内置结论同 012 §2.1（实测同批 grep 覆盖）。
- 附加效果消费点分散在 **后跳斩 49 / 里鬼 67 / 银光落刃 16 / 破军 68 / 幻影剑舞 73 / 跳跃攻击**。
- **判定 PO（族内唯一）**：`bluntmasterysub.obj` 实测完整——

| .obj 节 | 值 | 说明 |
|---|---|---|
| [name] | 钝器精通冲击波 | — |
| [layer] | bottom | 地面层 |
| [piercing power] | 1000 | 全穿透 |
| [basic motion] | `Animation/BluntMasterySub/Hit.ani` | 单相位（无 etc motion，L13 的命中判定型 sub） |
| [attack info] | `AttackInfo/BluntMasterySub.atk` | **atk 实测**：物理 / down 击倒反应 / push 50 / lift 100 / blow / 无元素 / hit 音 R_SHOCKWAVE_HIT / no blood |
| [object destroy condition] | on end of animation | 播完即毁 |

  谁创建它：引擎（跳跃攻击命中 / 后跳斩 / 幻影剑舞末击时按精通等级分支）；无 ap nut（PO 行为
  同族惯例在引擎）。动画目录 `BluntMasterySub\` 有 hit/sub1/sub2.ani 三个（变体归属未考证），
  Hit.ani 引用 **`Character/Priest/Effect/ChoppingHammer/bottom.img`**——圣职者贴图跨职业复用（L14 同类）。

## 3. 关联文件清单（差异行）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BluntMastery.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BluntMastery.skl` | ✅ | 12 列 × 50 级 + static 11 值 |
| PO 定义 | bluntmasterysub.obj | `…\pvf\passiveobject\character\swordman\bluntmasterysub.obj` | ✅ 实测 | 冲击波判定 PO |
| PO .atk | bluntmasterysub.atk | `…\passiveobject\character\swordman\attackinfo\bluntmasterysub.atk` | ✅ 实测（值见 §2） | 冲击波命中反应 |
| PO .ani | hit/sub1/sub2.ani | `…\passiveobject\character\swordman\animation\BluntMasterySub\` | ✅ 实测（ls） | 冲击波视觉 |
| 其余 | 同 012 §3 | — | 同 | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| bottom.img（Priest/ChoppingHammer） | sprite_character_priest_effect_choppinghammer.NPK | 冲击波视觉（跨职业借图） | **可选**（仅实现附加效果时） | ❌ |

必需级缺失 img = **0**（主体属性被动无视觉；冲击波属附加效果可选项）。

## 5~7. 实现草案与困难（同 012，差异三条）

1. **跳跃攻击冲击波（Lv5）**：撞跳跃系统缺口（R1-A2 在案）——"跳跃攻击命中时"的触发点在跳跃
   攻击落地命中判定内。届时形态：`BluntMasteryShockArea : AreaDefinition`（EnterActions={MeleeHit}、
   HitReaction{Damage=按列8 档, KnockbackX=50, LaunchY=100, down 手感}，atk 原值直译）——
   数据链完整，机制上 CreateArea 即可，卡点全在跳跃系统与精通等级查询。
2. 后跳斩浮空冲击波：后跳（169）已解析，冲击波 Area 同上；浮空力 = atk lift 直译。
3. 对眩晕状态敌人增伤：目标状态查询门面（R2-A8 在档）+ 属性消费链双卡——随里鬼实现期评估。

## 8. 存疑与缺口上报

- 未考证：static 11 值逐位语义；sub1/sub2.ani 的使用分支（推断对应后跳斩/幻影剑舞变体）；
  source -2 向量 3 处（L21 已记）。
- 新缺口：无新增（跳跃/目标状态查询/属性消费链/被动注入点均在档；012 上报覆盖）。
- 翻译缺口：`.skl` 子命令（全局已知项）；`.obj`/`.atk` 子命令（064 已在档全局项，本技能再 +1 数据点）。

**给下轮的经验**：精通族里只有钝器带 `xxxsub.obj` 判定 PO（跳跃攻击冲击波）；PO 单相位形态
（basic motion + attack info + 播完即毁，无 etc）是最简 PO 样本，可作 Area 直译的最小参照。
