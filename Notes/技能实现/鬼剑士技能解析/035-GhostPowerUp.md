# 封印解除（GhostPowerUp）

> 技能ID 35 | 级别 C | 可实现性 ⛔（技能等级系统缺失，效果完全空转） | 分析日期 2026-08-22 | 批次 C1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 封印解除 | `skill\Swordman\GhostPowerUp.skl [name]` |
| 英文名 | GhostPowerUp（取 skl 文件名；[name2]=`The Chain Cancelation` 官方英文名） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype] 2；[growtype maximum level] `0 0 1 0 0 0` 仅鬼泣 1 级） | 同上 |
| 学习等级 | **无 [required level] 节**（未标注；购买代价 0——转职/觉醒赠送型被动的形态） | 同上实测 |
| 最高等级 | 1（无 [level property] 节，无 CD/MP/指令——纯被动） | 同上 |
| 类型 | passive（**skill class 3**——同批被动多为 4，本技为 3，档位差异语义未考证） | 同上 [type] / [skill class] |
| static data | `5 25 36 41 84 75 82`（**7 个技能 ID**，见 §2.2 解码） | 同上 [static data] |
| 一句话效果 | 解除噬魂者左臂封印，使卡赞/冰霜萨亚/侵蚀普戾蒙/残影凯贾/瘟疫罗煞/冥炎卡洛/鬼斩 各 **+1 级** | 同上 [explain] |

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测，全空）

- `swordman_load_state.nut` grep `ghostpowerup` **无命中**（无 pushState）；
- 白名单 `sqr\character\swordman\` 全树 grep `ghostpowerup` **无命中**（无主 nut、无 appendage）；
- `passive_skill_swordman.nut` 的 `ProcPassiveSkill_Swordman` switch（294 行全文走读）**无 case 35**；
- `passiveobject\character\swordman\`、角色 `animation\`/`effect\animation\`/`swordman.chr`（grep 计 0）全无本技条目。

**结论：纯引擎内置被动**（F3 型最彻底档：比 064 还干净——无动画无特效无 PO，仅 .skl 一份数据）。
引擎读 static data 的 7 个技能 ID，对每个在册技能的当前等级 +1（等级加成参与各技 level property 取值）。

### 2.2 static data 解码（lst 反查，全实证）

| static 值 | lst 反查 skl | 技能（中文名） | 本仓库已有解析 |
|---|---|---|---|
| 5 | `Swordman/HardAttack.skl` | 鬼斩 | `005-HardAttack.md` ✅ |
| 25 | `Swordman/Khazan.skl` | 鬼神‧卡赞 | `025-Khazan.md` ✅ |
| 36 | `Swordman/Saya.skl` | 鬼神‧冰霜萨亚 | `036-Saya.md` ✅ |
| 41 | `Swordman/Bremen.skl` | 鬼神‧侵蚀普戾蒙 | `041-Bremen.md` ✅ |
| 84 | `Swordman/Keiga.skl` | 鬼神‧残影凯贾 | `084-Keiga.md` ✅ |
| 75 | `Swordman/EpidemicRasa.skl` | 鬼神‧瘟疫罗煞 | `075-EpidemicRasa.md` ✅ |
| 82 | `Swordman/Kalla.skl` | 鬼神‧冥炎卡洛 | `082-Kalla.md` ✅ |

与 [explain] 列出的七个技能**一一对应、零存疑**（鬼斩排最后的解释顺序与 static 顺序不同而已）。

### 2.3 机制归纳

```
学习（鬼泣，1 级封顶）→ 永久生效：
  卡赞/萨亚/普戾蒙/凯贾/罗煞/卡洛/鬼斩 的技能等级各 +1
  （各技 level property 按"等级+1"档取值——伤害/持续/概率等全线抬一档）
```

无施放、无动画、无数值面板——效果 100% 体现在**其他 7 个技能的等级取值**上。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GhostPowerUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GhostPowerUp.skl` | ✅ 实测（42 行） | 唯一数据源（7 个目标技能 ID） |
| lst 条目 | swordmanskill.lst 35-36 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 35 → 本 skl |
| 注册行 / 被动注册表 | — | `…\pvf\sqr\character\swordman_load_state.nut` / `…\passive_skill_swordman.nut` | ⛔ 均无本技（引擎内置） | — |
| 主 nut / appendage / PO / 动画 / 特效 / .chr | — | `…\pvf\sqr\character\swordman\`、`…\passiveobject\character\swordman\`、`…\character\swordman\` | ⛔ 全无 | 纯数据被动 |
| 图标 | SkillIcon.img 80/81 | `…\pvf\character\swordman\effect\SkillIcon.img` | ✅ 路径实测 | 仅 UI |
| 目标技能 7 份 | 见 §2.2 表 | `…\pvf\skill\Swordman\` | ✅ 全部在册 | 效果载体 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无） | — | — | — | — |

**零资源需求**：无任何 .ani/.als/.atk/.obj；缺失 img = 0。

## 5. 实现方案草案（⛔ 级）

**⛔ 暂缓——阻塞在系统层，且是三技中最"空"的一个**：

1. **技能等级系统缺失**：我方技能 = 固定数值内容件（SkillLogic/HitReaction 写死每档值），
   **没有"技能等级"概念，也没有 level property 缩放机制**（§6.3 延后档"等级数值缩放"的根因形态）。
   "+1 级"在我们的系统里没有落点——7 个目标技能的伤害/数值表无从按档抬升。
2. 就算补齐等级系统，效果也只是"数值档位切换"，无任何新表现——**建议等级缩放系统立项时作为
   零成本附带项**（static 7 个 ID + 1 级偏移即可），不单独实现。

**届时不占号段**：无施法行为（无 SkillId/AnimId）；等级系统若以"角色字段"实现亦无 BuffId 需求。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| GhostPowerUp.skl | `.skl` 尚无子命令（7 个 static 值） | 手抄 7 个 ID 即可；归入全局 skl 子命令需求（在案） |

（无其他文件——翻译环节仅此 1 条，全局已知项，**无新增缺口**。）

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 7 个鬼神技各 +1 级 | **缺失：技能等级系统/等级数值缩放**（本技为最纯样本——效果 100% 是"别的技能的等级"） | 等级缩放立项时附带；单独实现无意义 |
| 学习即永久生效 | 被动挂载机制（数值面板版同款问题，见 195 §5） | 届时随等级系统一并定形态 |
| 无 [required level] | 未考证（觉醒赠送型推断） | — |

## 8. 存疑与缺口上报

**未考证项**
1. [skill class] 3 vs 同批被动 4 的档位语义差。
2. 引擎对"+1 级"的精确实现（技能等级表偏移 vs level property 插值——引擎侧，无脚本可读）。
3. 无 [required level] 节的学习时机（觉醒赠送推断）。

**系统级缺口增量（主循环汇总）**
1. **技能等级系统 / level property 缩放**——§6.3 已列"等级数值缩放"于延后档，本技证明对鬼泣系
   它是**必需品而非可延后项**（+1 级被动是鬼泣Build标配）；建议与"属性消费链"合并评估优先级。

**翻译工具缺口**：`.skl` 子命令（全局在案项）。
