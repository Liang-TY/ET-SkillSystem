# 短剑精通（ShortSwordMastery）

> 技能ID 12 | 级别 C | 可实现性 🔶（属性面板可表达 / 消费端与附加效果卡死，分半见 §5） | 分析日期 2026-08-22 | 批次 C2

**本文档同时是四武器精通同族对照的主文档**（13 太刀 / 14 巨剑 / 15 钝器各自文档只列差异，
对照总表与共性结论以本文为准）。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 短剑精通 | `ShortSwordMastery.skl [name]` |
| 英文名 | Short Sword Mastery（[name2] 实测英文原版） | skl [name2] |
| 职业 | 全鬼剑士系可学（growtype 0-5；**剑魂 growtype 上限 1**——精通等级实际由武器奥义 27 代管，见 §2.3） | skl [growtype maximum level] `30 1 30 30 30 30` |
| 学习等级 | 15 | skl [required level] |
| 最高等级 | 50（各觉醒段内实际可点上限受 growtype 列约束） | skl [maximum level] |
| 类型 | passive（skill class 1） | skl [type] / [skill class] |
| 指令 / CD / MP | 无（纯被动） | skl |
| 一句话效果 | 使用短剑系武器时增加物攻/魔攻/物理命中率；剑魂习得后普攻/上挑/拔刀斩附带剑气、格挡生成眩晕冲击波、猛龙追加攻击 | skl [explain] |

### 四武器精通同族对照总表（C2 批内模板化结论）

| 项 | 12 短剑 | 13 太刀 | 14 巨剑 | 15 钝器 |
|---|---|---|---|---|
| 学习等级 | 15 | 15 | 15 | 15 |
| 最高等级 | 50 | 50 | 50 | 50 |
| 剑魂 growtype 上限 | 1（奥义代管） | 1（奥义代管，实测同列模式） | 1（奥义代管） | 1（奥义代管） |
| 主体属性（模板前三占位） | 物攻/魔攻/物理命中 | 物攻/魔攻/命中 | 物攻/魔攻/物理命中 | 物攻/魔攻/物理命中 |
| dungeon Lv1 属性三值（×0.1 向量） | 物攻 col0=14→+1.4%、命中 col1=3→+0.3%、魔攻 col8=12→+1.2% | 物攻 col0=13→+1.3%、命中 col1=3→+0.3%、魔攻 col9=12→+1.2% | 物攻 col0=12→+1.2%、命中 col1=3→+0.3%、魔攻 col10=12→+1.2% | 物攻 col0=12→+1.2%、命中 col1=4→+0.4%、魔攻 col9=12→+1.2% |

注：属性三值的**列位四族不同**（魔攻在 col8/col9/col10/col9），由各自 level property 向量
（-1 N 0.1）指认；"前三列"字面（col0-2）里 col2 是附加效果参数（如短剑上挑追加 110%）。
四族 growtype maximum level 行实测全部为 `30 1 30 30 30 30`。
| level info 列数 | 10 | 14 | 11 | 12 |
| static data | `7 5 3 7 500 400 7 0 10 12 600 950 3 85 750`（15 值） | `5 3 7 3 350 3`（6 值） | `5 3 7 50 5 3 400 150 1000`（9 值） | `3 7 5 100 7 0 3 300 3 130 7`（11 值） |
| Lv3 门槛附加 | 上挑追加物理攻击 + 剑气（普攻/里鬼/上挑/拔刀） | 空中连斩多段攻击 | 银光落刃霸体+多段；里鬼 2 次斩击+拔刀斩 | 后跳斩浮空冲击波；里鬼对眩晕敌增伤；银光落刃冲击波范围↑ |
| Lv5 门槛附加 | 格挡冲击波+概率眩晕 | 三段斩变 5 段+末击浮空 | 格挡冲击波+概率眩晕 | 跳跃攻击命中生成冲击波 |
| Lv7 门槛附加 | 猛龙追加攻击；拔刀剑气 | 拔刀斩追加攻击+概率出血 | 破军追加蓄气捶击+冲击波 | 破军追加捶击+冲击波；幻影剑舞末击变冲击波 |
| 专属视觉资源 | `effect\animation\shortswordmastery\` 17 个 ani（剑气 dodge 特效，含 [pvp] 变体） | 无 | 无 | `passiveobject\...\bluntmasterysub.obj` + `BluntMasterySub\` 动画/atk（冲击波判定 PO） |
| 引擎内置 | Y（四者同：load_state 无 pushState、passive_skill 无 case、白名单内除图标名外零引用） | Y | Y | Y |

static data 前几位的族内同构（**推断**）：各精通 static 前部 = 附加效果的等级门槛与参数
（短剑 `7 5 3 7…` ↔ explain 的 Lv7/Lv5/Lv3 门槛序；钝器 `3 7 5 100 7…` 同见门槛值）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无任何脚本**（四精通同结论，全部实测）：
- `swordman_load_state.nut` grep `shortsword|blademastery|heavysword|bluntmastery`：无命中（被动无施法状态，正常）；
- `passive_skill_swordman.nut` 294 行通读：**无 case 12/13/14/15**（对照：剑影太刀精通 78 有 case——同族异种不同待遇，因 78 是剑影新版有四路属性注入）；
- `sqr\character\swordman\` 白名单内 grep `Mastery|mastery`：仅命中 passive_skill_swordman.nut 自身（case 78）——
  四精通的**主体属性加成与全部附加效果均在客户端引擎内实现**。

### 2.2 机制归纳

- **主体**：装备短剑系武器时，按等级数据前三列加成物攻/魔攻/命中率（面板属性，进引擎攻击力公式）。
  条件激活 = "当前武器类型 == 短剑"（weaponSubType 判定，引擎内）。
- **剑魂附加效果**（Lv3/5/7 门槛，static data 中的门槛值判定）：不是独立逻辑，而是
  **改写其他技能的行为分支**——上挑/格挡/拔刀斩/猛龙/三段斩/银光落刃等技能的引擎内置流程里
  检查 `sq_GetSkillLevel(精通)` 切换形态（剑气层、冲击波 PO、追加段）。**消费点分散在被改写的
  各主动技能侧，不在精通自身**。
- 短剑精通专属视觉：`character\swordman\effect\animation\shortswordmastery\` 下
  shortattack1-3dodge / shortweaponcombo1-3 / shortweaponupperslash(after) 等 17 个 ani
  ——对应"普攻/里鬼/上挑剑气"的 dodge 混合特效层，由引擎按精通等级在上述技能播放时叠加。

### 2.3 与武器奥义（27）的联动

短剑精通 growtype 上限列 = `30 1 30 30 30 30`：**剑魂只能裸点 1 级**，实际等级由
武器奥义（27，static data `12 13 14 15 4` = 五个精通技能 ID）按其 level info 各列
（`3 3 3 3 2` 起，+1/级）镜像提升——即"精通等级 = 裸点 + 奥义加成"，等级簿记在引擎内完成。
详见 `027-WeaponMasteryUp.md`。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ShortSwordMastery.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ShortSwordMastery.skl` | ✅ | 全部技能数据（10 列 × 50 级 + static 15 值） |
| 注册行 | — | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无（被动，正常） | — |
| 被动注册 | — | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ⛔ 无 case 12（引擎内置） | — |
| 主 nut / appendage | — | `…\pvf\sqr\character\swordman\`（白名单 grep 实测） | ⛔ 不存在 | 行为在引擎 |
| 剑气特效 | shortswordmastery\*.ani ×17 | `…\pvf\character\swordman\effect\animation\shortswordmastery\` | ✅ 实测（ls） | 普攻/里鬼/上挑的剑气 dodge 层（含 [pvp] 变体） |
| PO / .atk / .als | —（本技能无） | `…\passiveobject\character\swordman\` | ⛔ 无（钝器精通才有，见 015） | — |
| 图标 | SkillIcon.img #38/39 | `Character/Swordman/Effect/SkillIcon.img` | ✅（路径） | 技能图标（无 UI 消费，不做） |
| 关联被动 | WeaponMasteryUp.skl（27） | `…\skill\Swordman\WeaponMasteryUp.skl` | ✅ | 剑魂等级镜像（027 文档） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | （无新动画需求，仅背景知识） | — | ✅ 已在库 |
| shortswordmastery\*.ani 引用的 img（剑气层，**未逐个打开提取清单**） | sprite_character_swordman_effect_shortswordmastery 系（按 01§2 规则推导） | 剑气视觉 | **可选**（仅实现剑魂附加效果时需要） | ❌ |

**必需级缺失 img = 0**（主体属性被动无视觉需求；剑气层为附加效果的可选项）。

## 5. 实现方案草案与困难（§5/§6/§7 合并）

**判定分半**（本批 C 类属性被动的统一口径）：

| 半边 | 结论 | 依据 |
|---|---|---|
| 面板可表达 | ✅ 可 | 物攻加成 → `NumericType.Attack(1003)` 已存在；BuffDefinition AddActions/RemoveActions 数值挂摘（176 远古记忆同构）。加成率换算：col0×0.1% 等按 §1 表 |
| 消费端 | ⛔ 卡死 | MeleeHit 伤害 = HitReaction.Damage 固定值，不读 NumericType（176 §8 已证第 1 例、轮间经验"属性数值无伤害消费链"已 5 实证）；命中/魔攻无对应键 |

- **届时形态（主体属性）**：`ShortSwordMasteryBuff : BuffDefinition`（`TotalTimeMs=0` 永久），
  AddActions/RemoveActions 数值挂摘对（`AddOwnerNumeric(AttackPct, +N)` 型，ActionIds 从 15 起顺延、
  BuffIds 从 18 起顺延，撞号无妨）。**激活条件"装备短剑时"无武器类型系统 → demo 简化为无条件常驻**
  或固定 demo 武器类型生效（武器类型差异化缺口 R2-A6 已在档）。
- **附加效果（剑魂 Lv3/5/7）**：⛔ 不随精通实现——消费点在 8+ 个其他技能的形态分支里
  （上挑剑气/格挡冲击波/猛龙追加…），等于"按武器类型+精通等级重写全技能组"，依赖
  ①武器类型系统 ②技能等级查询 ③各技能的多形态支持。建议：精通主体先行（面板试点），
  附加效果随对应主动技能的武器分支立项时逐个补。
- **被动注入点缺口**：我们技能全部由按键驱动（ButtonToSkill），**没有"角色初始化挂常驻 Buff"的入口**——
  需要一个"被动装配"调用点（角色生成时按已习得被动清单挂 Buff）。见 §8 上报。
- **等级数值**：等级缩放延后（§6.3 延后档），demo 取固定档（如 Lv10 值 +2.9% 物攻 / +4.9% 命中档）。

翻译工具：`.skl` 无子命令（全局已知项，本技能 10 列×50 级+static 15 值手抄可行）；剑气 ani 为常规节（未逐个核对，实现附加效果时再核）。

## 6~7. 困难与简化（并入 §5，此处补遗）

| DNF 原版行为 | 缺口档位 | 简化 |
|---|---|---|
| 条件激活（武器类型） | 武器类型差异化（缺失，R2-A6） | 无条件常驻或固定武器 |
| 物攻加成进伤害公式 | 属性伤害消费链（缺失，5 实证） | 仅面板变化，demo 作 Buff 体系试点 |
| 剑魂附加效果（剑气/冲击波/追加段） | 武器分支+等级查询+多形态技能（缺失叠加） | 不做，随主动技能逐个立项 |
| 等级镜像（奥义） | 技能等级系统（缺失） | 见 027 文档 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 15 值的逐位语义（前部门槛值由 explain 旁证推断；500/400/750 等参数值未解）。
2. level info col2=110→633（Lv1→Lv70）随物攻比率列同涨——"物理命中率"用 110 起步与 ×0.1 系数的关系未细究（不影响 demo 固定档）。
3. 剑气特效 ani 与各技能帧位的挂接关系（引擎内置叠加，无 .als 边车可查）。

**新系统级缺口（§6.3 清单外）**
1. **被动装配/常驻属性注入点**：无"角色初始化时按习得被动挂常驻 Buff"的入口（SkillContext 全部
   挂在按键施放的 cast 上）。四精通/幻鬼之力/血气唤醒等全部 C 类属性被动共同需要——建议与
   "属性伤害公式"（176 上报）合并立项为"被动技能系统最小版"：习得表 + 初始化挂 Buff + 数值消费。
2. **武器类型差异化**（R2-A6 已在档）：本批四精通是其最大用户（条件激活 + 附加效果分武器分支），证据 +1。

**翻译工具缺口**：`.skl` 子命令（全局已知，无新增）。

**给下轮的经验**：武器精通类被动（12/13/14/15/4/78）全部引擎内置（除 78），
主体 = skl 前三列 ×0.1 的面板百分比；剑魂 growtype 上限恒 1（奥义 27 代管），
非剑魂上限 30——从 growtype maximum level 行可直读该约束，不必逐个 explain。
