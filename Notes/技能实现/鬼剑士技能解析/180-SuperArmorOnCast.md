# 不屈意志（SuperArmorOnCast）

> 技能ID 180 | 级别 B（纠偏：预判 A 存疑已证实——实际为**施法免打断状态技**，无攻击逻辑） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 不屈意志 | `SuperArmorOnCast.skl [name]` |
| 英文名 | SuperArmorOnCast（取 skl 文件名；[name2] 实测为英文 `Great Willpower`——DNF 官方英文名，两版并存） | skl [name2] |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5 全可学，各成长上限 10；DNF 通用系技能） | skl |
| 学习等级 | 15 | skl `[required level]` |
| 最高等级 | 20 | skl `[maximum level]` |
| 类型 | active（skill class 4 = buff 类） | skl `[type]` |
| 指令 | ↓ + Space（`(DOWN)` + `(BUFF)`） | skl `[command]` + `[command key explain]` |
| CD | 30000 → 105000 ms（Lv1 → Lv20，**随等级增长**） | skl `[cool time] 30000 105000` |
| MP | 8 → 80（Lv1 → Lv20） | skl `[consume MP]` |
| 读条 | 200 ms（casting time；pvp 1000 ms） | skl `[casting time]` |
| 特殊消耗 | 无 | skl |
| 一句话效果 | 一定时间内，施放技能时遭受敌人攻击也不会中断施放动作（概率性，高等级 100%） | skl `[explain]` |

**level property（2 列，20 级全表实测）**：
- dungeon：Lv1 `(100, 10000)` → Lv10 `(1000, 55000)` → Lv20 `(1000, 105000)`；
- pvp：Lv1 `(100, 20000)` → Lv10 `(1000, 29000)` → Lv20 `(1000, 30000)`。

列语义**推断**（模板顺序与数据矛盾，见 §8）：
- 列 0 = 不被中断机率，**0.1% 单位**：Lv1 100=10% → Lv10 起 1000=**100% 封顶**；
- 列 1 = 持续时间 ms：Lv1 10000=10s → Lv20 105000=105s（pvp 封顶 30s）。
依据：列 1 若作机率则 Lv20=1050% 不可能；列 0 恰在 Lv10 达 1000 后不再增长（封顶特征）；pvp 持续压缩到 30s 内符合 pvp 平衡惯例。
模板 `持续时间 : <float1>秒 / 不被中断的机率 : <float1>%%` 的先后序与列序**不符**（MagicalAttackUp 等同格式技能是相符的），原因未考证。
向量 col0=`-1 1 0.001`、col1=`-1 0 0.1`，与表值对不上，同样存疑。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无任何脚本存在**（全实测，与 176 同法）：
- `swordman_load_state.nut` grep `superarmor` 无命中（无 pushState）；
- `sqr\character\swordman\` 全树 grep `superarmor|SuperArmorOnCast` 无命中（无主 nut、无 appendage；
  命中的 `sq_SetSuperArmor*` 调用属流心/流星/巨剑等**其他技能**的即时霸体 API，与本技能无关，见 §2.2）；
- `passive_skill_swordman.nut` 无命中。

——纯引擎内置 buff 技能。

### 2.2 行为重建（按引擎惯例 + skl 数据，推断部分已标注）

- **onSetState（推断）**：播 `[buff motion]` 通用动画（Summon2.ani，同 176，chr 949 行段实测）→ 扣 MP → 读条 200ms。
- **读条完成 → 挂引擎 appendage（推断）**：持续列 1（10s~105s），期间**施放技能时被击**按列 0 概率免打断。
- **机制本质（引擎侧）**：DNF 的"施放被打断"= 读条/施法动作被击中断（进受击硬直）；
  本 buff 的效果 = 施法状态中被击时按概率**保持施法动作继续**（等效于"施法期间霸体"，故技能文件名 SuperArmorOnCast）。
  引擎 API 佐证：`sq_SetSuperArmorUntilTime(obj, ms)`（flowmindone/three/two.nut 实测使用——剑魂流心系读技能 248 列 1 时长开即时霸体）、
  `sq_SetSuperArmor/sq_RemoveSuperArmor/sq_IsSuperArmor` 一族。本技能 = 该机制的**概率化、持续化封装**。
- **结束**：到期复原。

### 2.3 被动对象 / appendage

无（§2.1 实测）。无专属视觉文件（character/swordman/animation 与 effect/animation grep `willpower|superarmor|buff` 无命中；
effect 下仅有 buffalofall/ghost_change_buff 两个无关目录）。DNF 本体的蓝色光环/霸体标记为引擎内置绘制。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| summon2.ani（[buff motion] 槽，与 176 共用） | 12 | 600ms | 无 | 无 | 仅 `sm_body%04d.img`；无 .als |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SuperArmorOnCast.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SuperArmorOnCast.skl` | ✅（126 行） | 全部技能数据 |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | — |
| 主 nut / appendage | —（不存在） | `…\pvf\sqr\character\swordman\`（全树 grep 实测） | ⛔ 缺失 | 行为在引擎 |
| 引擎 API 旁证 | flowmindone/three/two.nut 的 sq_SetSuperArmorUntilTime 调用 | `…\pvf\sqr\character\swordman\flowmind\*.nut:10/15` | ✅ 实测 | 即时霸体 API 存在性证明 |
| .chr 条目 | [buff motion] = Summon2.ani | `…\pvf\character\swordman\swordman.chr:949-951` | ✅ 实测 | 通用 buff 施法动画 |
| 角色 .ani | summon2.ani（共用） | `…\pvf\character\swordman\animation\summon2.ani` | ✅ | 施法动作 |
| 角色 .atk / 特效 | —（无） | attackinfo\ / effect\animation\ | ⛔ 无 | buff 无攻击/无特效文件 |
| 装备层 | summon2* ×76 | `…\pvf\equipment\character\swordman\avatar\{…}\` | ✅（find 计数，与 176 共用） | avatar 变体 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动画图集 | 必需（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` |

**缺失 img = 0**（霸体光环视觉引擎内置无文件，demo 无视觉或 tint 近似）。

## 5. 实现方案草案

**先说结论**：本技能的效果消费端（"施放被打断"）在我们系统里**目前不存在**——受击只写 HitstunTimer + 切 Hurt 动画
（LSCombatComponentSystem 实测：不取消活动 LSCast；SkillCastHelper 只对**新施放**做硬直门禁）。
因此"不屈意志"在现系统下**空转成立**（施法本来就不会被打断）。demo 定位 = Buff 体系与"霸体语义"的占位试点。

### 内容件清单

1. **`DotNet~/Skills/GreatWillpowerSkill.cs : SkillLogic`**
   - `CooldownMs = 30000`（Lv1 原值；随等级增长 demo 不做）；`TotalTimeMs = 600`（summon2 动画时长）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanBuffCast)` + `ctx.AddBuffToSelf(BuffIds.GreatWillpower)`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。读条 200ms 简化为瞬发（同 176）。
2. **`DotNet~/Buffs/GreatWillpowerBuff.cs : BuffDefinition`**（最简 Buff：无 Actions 亦可）
   - `TotalTimeMs = 10000`（Lv1 原值；Lv20 105000）；`TickTimeMs = 0`；AddActions/RemoveActions 暂空（无消费者，见上）。
   - 概率（列 0）demo 固定 100%（Lv10+ 档）——有消费者后接 `LSRng.Roll`（HitReaction.ProcChance 同构，已落地）。
3. **未来接线位（记档不实现）**：当"受击打断施法"机制引入时（如施法中被打 → 取消 LSCast + 进硬直），
   打断检查点查询 `LSBuffComponent` 是否持有 GreatWillpower → 概率豁免。需要 **Buff 查询门面**（§6.3 缺失档，
   bloodboom §4.6"血气旺盛增伤联动"同源缺口——凡是"查目标有没有某 Buff"的技能都卡这里）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎内置 buff（skill class 4） | `SkillLogic`（瞬发）+ `AddBuffToSelf` |
| 引擎 appendage（免打断概率/时长） | `GreatWillpowerBuff : BuffDefinition`（挂上即可，消费端见 §7） |
| sq_SetSuperArmorUntilTime 系 API（即时霸体） | 霸体帧延后档（§6.3）；[.ani DAMAGE TYPE=SUPERARMOR] 的 AnimFrameData 通道同属一个专题 |
| 施放被打断 | **现系统无此机制**（受击不打断活动 cast）——效果空转 |
| [buff motion] Summon2.ani | 与 176 共用同一 AnimId（SwordmanBuffCast） |
| 概率判定 | LSRng.Roll（已落地，FreezeBuff/ProcChance 先例）——待消费端出现后接 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.GreatWillpower = 14` + ButtonToSkill 新键 |
| BuffId | `Packages\cn.etetet.skill\Runtime\BuffDefinition.cs` | `BuffIds.GreatWillpower = 6` |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | 无新增（复用 176 的 SwordmanBuffCast/summon2） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | 无新增（176 已注册 summon2.json） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000（Lv1）→ 105000（Lv20） | 10000（demo 缩短） |
| 施法时长 | 读条 200 + 动作 600 | 600 |
| 持续 | 10000ms（Lv1）→ 105000ms（Lv20）；pvp ≤30000 | 10000 |
| 免打断概率 | 10%（Lv1）→ 100%（Lv10+ 封顶，推断读法） | 100% |
| MP | 8 → 80 | 跳过 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| SuperArmorOnCast.skl | `.skl` 无子命令 | 2 列 × 20 级手抄可接受；归全局需求 |
| summon2.ani | 全常规节 | 现有 ani 子命令全覆盖（176 同款共用） |

结论：**无新增翻译缺口**。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 施放技能时被打断 → 概率免打断 | **施法打断机制不存在**（受击不取消 LSCast——效果空转；无消费者） | demo 空转成立：Buff 可挂可到期可叠层，行为验证足够；真实博弈等打断机制专题 |
| 免打断判定 | Buff 查询门面缺失（§6.3 缺失档） | 未来打断检查点直接遍历 LSBuffComponent 或加 `HasBuff` 门面（血气旺盛联动同���） |
| 读条 200ms（被打断风险窗口） | 无读条系统（延后） | 瞬发 |
| 霸体帧（DAMAGE TYPE=SUPERARMOR） | §6.3 延后档（AnimFrameData 加 damageType） | 本技能不依赖帧级霸体（是状态级），但同专题；先跳过 |
| CD 随等级 30s→105s | 等级缩放延后 | 固定 10s |
| 蓝色光环视觉 | 引擎内置无文件（缺失档：无来源） | 跳过或 tint 近似 |

## 8. 存疑与缺口上报

**未考证项**
1. 列 0/列 1 语义对调推断（模板序 vs 数据形态矛盾，§1 已给依据）——**本批次最需要试玩或引擎侧佐证的点**：
   若列 0 实为持续时间（0.1s~1s 量级读法）则技能形态完全不同（瞬时免打断窗）。
2. 向量 col0=`-1 1 0.001`/col1=`-1 0 0.1` 与表值对不上的原因（其他技能如 MagicalAttackUp 向量同样只给显示换算系数）。
3. 读条期间受击的精确行为（打断 or 延迟）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **施法打断机制缺失的反向问题**：DNF 的手感建立在"施法会被打断"上（受击硬直取消读条/长施法），我们目前**永不打断**。
   大量技能（读条系、不屈意志、霸体帧、受击流心取消）都挂在这个机制上。建议归档：**受击-施法交互专题**
   （含：打断规则 + 霸体豁免 + Buff 查询门面），是比单个技能更优先的框架级议题。
2. **Buff 查询门面**（与 bloodboom §4.6、064 §8 重复撞上，第三次实证）：`LSBuffComponent.HasBuff(buffId)` 一类的
   只读门面极小（遍历子实体），但能让"不屈意志/血气旺盛/波动印记"一族全部从缺失档升到可表达——建议单独小任务先做。

**翻译工具缺口**：无新增。
