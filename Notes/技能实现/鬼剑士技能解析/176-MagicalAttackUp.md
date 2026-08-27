# 远古记忆（MagicalAttackUp）

> 技能ID 176 | 级别 B（纠偏：预判 A 存疑已证实——实际为**主动属性增益状态技**，无任何攻击逻辑） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 A4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 远古记忆 | `MagicalAttackUp.skl [name]` |
| 英文名 | MagicalAttackUp（取 skl 文件名；[name2] 实测为英文 `Ancient Memory`——DNF 官方英文名，两版并存） | skl [name2] |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5 全可学，各成长上限 10；DNF 通用系技能，非鬼剑士专属） | skl |
| 学习等级 | 15 | skl `[required level]` |
| 最高等级 | 20 | skl `[maximum level]` |
| 类型 | active（skill class 4 = buff 类） | skl `[type]` |
| 指令 | Space（`(BUFF)` 键） | skl `[command]` + `[command key explain]` |
| CD | 40000 ms（固定） | skl `[cool time] 40000 40000` |
| MP | 15 → 150（Lv1 → Lv20） | skl `[consume MP]` |
| 读条 | 500 ms（casting time；pvp 1500 ms） | skl `[casting time]` |
| 特殊消耗 | 无 | skl |
| 一句话效果 | 增加自身的智力，效果持续一定时间（20 秒 +15 → +300 智力） | skl `[explain]` + level info |

**level property（2 列，20 级全表实测）**：
- 列 0 = 持续时间 ms：**恒 20000**（20 秒；pvp 同 20000）；
- 列 1 = 增加智力：`15 → 300`（每级 +15；pvp 12 → 240，每级 +12）。
模板 `持续时间 : <float1>秒 / 增加智力 : <int>`；向量 col0=`-1 0 0.001`（ms→秒显示换算）、col1=`-1 1 1.0`。
列语义与表值自洽（本技能为少数**列语义确定**的引擎内置 buff：持续时间+数值直读无歧义）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无任何脚本存在**（全实测）：
- `swordman_load_state.nut` grep `magicalattack` 无命中（无 pushState）；
- `sqr\character\swordman\` 全树（含 appendage/ 7 个 ap_*）grep 无命中（无主 nut、无 appendage）；
- `passive_skill_swordman.nut` 无命中（非被动系）。

——纯引擎内置 buff 技能（skill class 4 的标准形态：读条 → 挂引擎状态）。数据消费全在引擎内，
pvf 侧只有 .skl 数值与通用施法动画。

### 2.2 行为重建（按引擎惯例 + skl 数据，推断部分已标注）

- **onSetState（推断）**：进入通用 buff 施法状态 → 播 `[buff motion]` 槽位动画（`swordman.chr` 949 行段实测 = `Animation/Summon2.ani`，12 帧 600ms，仅 sm_body 图像）→ 扣 MP。
- **读条 500ms**：casting time 节——DNF 读条机制（期间可被打断，见不屈意志的互文），我们无读条系统（§7）。
- **读条完成 → 挂引擎 appendage（推断）**：智力 + 列1 值，时长 = 列0（20000ms）。
  引擎侧"智力"参与魔法攻击力公式（五层公式的属性加成）——这部分在引擎内，无脚本可读。
- **结束**：状态到期属性复原（引擎 appendage 标准行为）。

### 2.3 被动对象 / appendage

无（§2.1 实测）。远古记忆的增益光环/全身发光视觉为引擎内置绘制，白名单目录实测无任何
magicalattack/ancient 命名的 .ani/.als/.ptl（character/swordman/animation 与 effect/animation 均查）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| summon2.ani（chr [buff motion] 槽，通用 buff 施法动作） | 12 | 600ms | 无 | 无 | 仅 `sm_body%04d.img`；.damageBox 每帧 1 个；无 .als |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MagicalAttackUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MagicalAttackUp.skl` | ✅（122 行） | 全部技能数据 |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | — |
| 主 nut / appendage | —（不存在） | `…\pvf\sqr\character\swordman\`（全树 grep 实测） | ⛔ 缺失 | 行为在引擎 |
| .chr 条目 | [buff motion] 段 = Summon2.ani | `…\pvf\character\swordman\swordman.chr:949-951` | ✅ 实测 | 通用 buff 施法动画槽 |
| 角色 .ani | summon2.ani | `…\pvf\character\swordman\animation\summon2.ani` | ✅ | 施法动作 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 无（buff 无攻击） | — |
| 特效 | —（无专属文件） | `…\pvf\character\swordman\effect\animation\`（grep 实测） | ⛔ 缺失 | 光环视觉引擎内置 |
| 装备层 | summon2* ×76 | `…\pvf\equipment\character\swordman\avatar\{…}\` | ✅（find 计数） | avatar 变体（只查存在性） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动画图集 | 必需（共享） | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` |

**缺失 img = 0**（无特效需求；引擎光环视觉无文件可提取，demo 无视觉或自做 tint 近似）。

## 5. 实现方案草案

### 内容件清单

1. **`DotNet~/Skills/AncientMemorySkill.cs : SkillLogic`**
   - `CooldownMs = 40000`（DNF 原值直用）；`TotalTimeMs = 600`（summon2.ani 12 帧总时长）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanBuffCast)`（summon2 译 json）；`ctx.AddBuffToSelf(BuffIds.AncientMemory)`。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
   - 读条 500ms：无读条系统——**简化**为瞬发（读条期间被打断的博弈不做，见 §7）。
2. **`DotNet~/Buffs/AncientMemoryBuff.cs : BuffDefinition`**（StunBuff 同构，最简形态）
   - `TotalTimeMs = 20000`（DNF 原值恒定）；`TickTimeMs = 0`。
   - `AddActions = { AncientMemoryOn }`、`RemoveActions = { AncientMemoryOff }`——**新 Action 一对**
     （`AddOwnerNumeric(新 IntelligenceAdd, +值)` / `RemoveActions` 减回；数值挂/摘成对，ForbidMoveOn/Off 同构）。
   - 数值原值：+15（Lv1）/ +300（Lv20）→ demo 固定 **+150**（中位档演示）。
3. **前置小改（框架层数值表，非本技能内容件）**：`NumericType` 无 Intelligence/魔法攻击键——
   需加 `Intelligence = 1007`（+ 五个子键 10071-10075）。这是把"数值系统五层公式"第一次用于属性增益的试点。
   **注意：当前 MeleeHitAction 伤害 = HitReaction.Damage 固定值，不读任何攻击方数值**——加了 Intelligence 也**不影响伤害**（§7 困难①）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎内置 buff 技能（skill class 4） | `SkillLogic`（瞬发）+ `AddBuffToSelf` |
| 引擎 appendage（智力+15~300/20s） | `AncientMemoryBuff : BuffDefinition`（AddActions/RemoveActions 数值挂摘） |
| 智力参与魔法攻击力公式（引擎内） | **无对应**——NumericType 加 Intelligence 后仅有数值本身，无消费公式 |
| 读条 500ms | 无读条系统 → 瞬发 |
| [buff motion] Summon2.ani | 新 AnimId + 译 json（sm_body 已在库） |
| 引擎光环视觉 | 无文件 → 跳过或单位 tint（受击闪白 renderer.color 通道可复用近似，延后） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Packages\cn.etetet.skill\Runtime\SkillIdAttribute.cs` | `SkillIds.AncientMemory = 13` + ButtonToSkill 新键（Space 类，如 N） |
| AnimId | `Packages\cn.etetet.npkparser\Runtime\AnimConfigRegistry.cs` | `SwordmanBuffCast = 59`（summon2，接 58 顺延） |
| BuffId | `Packages\cn.etetet.skill\Runtime\BuffDefinition.cs` | `BuffIds.AncientMemory = 5`（接 Freeze=4 之后） |
| ActionId | `Packages\cn.etetet.skill\Runtime\LSAction.cs` | `AncientMemoryOn/Off = 8/9` |
| NumericType | `Packages\cn.etetet.skill\Scripts\Model\Share\NumericType.cs` | `Intelligence = 1007` + 子键 ×5（框架层，F6） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | summon2.json（无新 img） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 40000 ms | 4000（demo 缩短便于演示 Buff 进出） |
| 施法时长 | 读条 500 + 动作 600 | 600（瞬发直用动画时长） |
| 持续 | 20000 ms（恒定） | 20000（直用） |
| 智力 | +15 → +300（+15/级；pvp +12/级） | +150 固定 |
| Buff 叠加 | 同 Buff 再挂 = Stack+1+刷新时长（引擎 appendage 刷新） | 现有 BuffLoader 叠层简版天然同构 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| MagicalAttackUp.skl | `.skl` 无子命令 | 本技能 2 列 × 20 级手抄 5 分钟，无催迫；归入全局 skl 子命令需求 |
| summon2.ani | 全常规节（FRAME/IMAGE/IMAGE POS/DELAY/DAMAGE BOX） | 现有 ani 子命令全覆盖 |

结论：**无新增翻译缺口**（.skl 为全局已知项）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 智力提升 → 魔法攻击力提升（引擎公式） | **数值伤害公式缺失**：MeleeHit 伤害为 HitReaction.Damage 固定值，不读攻击方任何 NumericType——加智力对战斗数值无影响（新缺口，§8） | demo 定位为"Buff 体系试点"：验证挂/摘/时长/叠层 + 数值面板变化（加 Intelligence 后可在数值系统观察到 Intelligence final 变化），伤害不动 |
| 读条 500ms（期间可被打断，与不屈意志互文） | 无读条/打断系统（延后档） | 瞬发 |
| MP 消耗 | MP 系统延后 | 跳过 |
| 增益光环视觉 | 引擎内置无文件（缺失档：无来源） | 跳过或 tint 近似 |
| 等级数值（+15/级） | 等级缩放延后 | 固定 +150 |

## 8. 存疑与缺口上报

**未考证项**
1. 引擎侧"智力"的精确公式位（独立属性 vs 直接魔攻加成）——无脚本可读；对 demo 无影响（见上）。
2. pvp 列 1 为 +12/级（dungeon +15/级）——差异原因未考证（平衡性调整，常识级）。
3. 读条期间受击是否打断（DNF 通用读条机制如此，未在本 pvf 实证）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **属性数值的伤害消费链**：LSNumericComponent 五层公式齐备，但 MeleeHit/所有伤害节点只读固定 HitReaction.Damage，
   **没有任何"属性 → 伤害"公式位**。远古记忆/不屈意志/所有增伤 Buff（血气旺盛联动、波动印记等）都卡在这一处。
   建议归档：缺失档——"属性伤害公式（NumericType.Attack/Intelligence → HitReaction.Damage 加成）"，
   补齐前提 = MeleeHit 改读 source 的 NumericType（门面已在 LSActionContext.GetSourceId，取 source 单位即可）。
   这是**第一批状态增益类技能撞上的共同缺口**，优先级建议高于单个技能移植。

**翻译工具缺口**：无新增。
