# 死亡墓碑（TombStoneRain）

> 技能ID 44 | 级别 A | 可实现性 🔶（多块墓碑连砸可直接表达；诅咒/霸体/跳跃中断降级） | 分析日期 2026-08-22 | 批次 A10

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 死亡墓碑（[name2]="Tombstone"） | `skill\Swordman\TombStoneRain.skl [name]` |
| 英文名 | TombStoneRain（取 skl 文件名） | 同上 |
| 职业 | 鬼泣（[skill fitness growtype]=2，L17） | 同上 |
| 学习等级 | 35 | 同上 [required level] |
| 最高等级 | 70（growtype2 段上限 50） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 3） | 同上 [type] |
| 指令 | ↑↓ + Z（指令施法 MP 优惠 10%/20% 档） | 同上 [command] / [skill command advantage] |
| CD | 20000 ms（pvp 同 20000 / 起手 20000） | 同上 [cool time] / [pvp] |
| MP | 165 → 1386（Lv1→Lv70） | 同上 [consume MP] |
| 施法时间 | 500 ms | 同上 [casting time] |
| 特殊消耗 | 无色小晶体 3037×1 | 同上 [consume item] |
| 一句话效果 | 自身周围区域连续落下墓碑砸击敌人，概率施加诅咒；落碑期间自身霸体且不可移动，按跳跃键可中断 | 同上 [explain] |

**static data**：无（skl 内未见该节）。
**level info（8 列，Lv1 → Lv70）**：`180 3000 8 540 36 240 10000 36` → `180 3000 8 4320 174 1068 10000 450`（pvp 行 `180 3000 8 30 36 100 10000 5`）。

| 列 | 值域 | 语义（推断，无 nut 消费可证） |
|---|---|---|
| col0 | 180（恒） | 墓碑掉落间隔 ms（180ms/块） |
| col1 | 3000（恒） | 落碑持续时间 ms（3 秒雨） |
| col2 | 8（恒） | 同屏墓碑上限（生成器节流） |
| col3 | 540→4320 | 每块墓碑魔法攻击力（pvp 30→…印证伤害列身份） |
| col4 | 36→174（+2/级） | 诅咒参数 A（等级或时长） |
| col5 | 240→1068 | 诅咒参数 B（pvp 100→255 缩水，性质同伤害类） |
| col6 | 10000（恒） | 诅咒几率 100.00%（万分率，GoreCross col4=10000 同惯例） |
| col7 | 36→450（+6/级） | 诅咒参数 C（pvp 5→80） |

**feature skill index 225**（与 TP 强化/变体关联，E 类批次另行分析；同目录另有 tombstoneex/tombstoneexexplosion obj 与 atk，以及 .chr etc motion #97 = TombStoneEx.ani）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**load_state 无注册**（实测：按名 grep 无命中；按技能 ID 44 反查第 5 参——仅 Kalla 状态 44/-1，非本技能）。`sqr\character\swordman\` 内无 tombstonerain 目录（仅有 `tombstoneswamp\`——那是**另一个技能 247 墓碑沼泽**，且其 nut 被 mod 作者 XOR compilestring 混淆 + 调用共享打击 PO 24370，与本技能无逻辑关联）。**F3 引擎内置技能**（老一代常态），返回值标"引擎内置 Y"。

资源侧证据链：
- `swordman_header.nut` 有 `CUSTOM_ANI_TOMBSTONEEX <- 97`（Ex 版动画槽位），无基础版常量——基础版施法动画未定位（§8 存疑）。
- 被动对象三件套齐全（§2.3）。

### 2.2 施法时序（引擎内置，按数据反推）

```
casting time 500ms（站桩施法，通用 casting 状态）→
施放：在自身周围随机/环形位置连续创建墓碑 PO（tombstone.obj）：
    间隔 col0=180ms、持续 col1=3000ms（≈16 块）、同屏上限 col2=8 →
每块墓碑：tombstone.ani 落下（60×3ms）→ 落地帧起攻击盒（F1-F5，见 §2.4）→
    站立 1000ms（F3）→ 崩碎（F6-F9）；命中走 tombstone.atk（down/push50 + 诅咒 col4-7）→
期间自身霸体（SUPERARMOR）+ 不可移动；跳跃键中断收尾
```

生成器与节流由 `tombstonerain.obj`（"死亡墓碑生成器"）承担——**该 obj 无 motion/无 attack info，是纯生成器 PO**（首个此形态样本：行为 100% 引擎内置，pvf 只留壳与子对象定义）。

### 2.3 被动对象（墓碑本体）

**tombstone.obj**（`passiveobject\character\swordman\tombstone.obj`，"死亡墓碑"）：

| 节 | 值 | 说明 |
|---|---|---|
| pass type / piercing | pass all / 1000 | 全穿透多目标 |
| basic motion | `Animation/TombStone.ani` | 10 帧 1540ms，F1-F5 攻击盒 |
| etc motion | `TombStoneGlow2.ani`、`TombStoneGlow1.ani`、`TombStoneDust.ani` | 光晕×2 + 落地尘土视觉层（11/11/7 帧） |
| attack info | `AttackInfo/TombStone.atk` | 下表 |
| int data | `2 50` | （引擎参数，未考证） |

**TombStone.atk**（`passiveobject\character\swordman\attackinfo\tombstone.atk`）：

| 字段 | 值 | → 我们 HitReaction |
|---|---|---|
| attack type / elemental | magic / **dark element** | 暗属性（无元素系统，记档） |
| damage reaction / direction | **down** / hit down | 长硬直击倒近似 |
| push aside / lift up | 50 / 0 | Kb=50 / Ly=0 |
| hit info / no blood | blow / 50 1.0 | 表现层 |
| **[active status] curse** | `0 0 0 0 0 0 0`（7 零） | 诅咒参数运行时由引擎按 skl col4-7 填入（静态全零=占位） |

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `passiveobject\character\swordman\animation\tombstone.ani` | 10 | 1540（60×3+**1000**+60×5） | 无 | F1-F5 | 盒见下；引 `Character/Swordman/Effect/TombStone.img` |
| tombstoneglow1.ani / glow2.ani | 11 / 11 | 1540 / 1540 | 无 | 无 | 光晕层（Glow1/2.img） |
| tombstonedust.ani | 7 | 420 | 无 | 无 | 落地尘土（TombStoneDust.img） |
| 角色 .ani | **未定位** | — | — | — | 基础版无专属施法动画（仅 TombStoneEx.ani #97 属强化线；推断用通用 casting 动作，未考证） |

**tombstone.ani 攻击盒**（偏移 x,y,z + 尺寸 w,h,d，DNF 像素）：

| 帧 | 盒 |
|---|---|
| F1（落地） | `-18 -15 56 48 30 131` |
| F2/F3 | `-18 -15 -1 41 30 119`（F3=1000ms 站立期持续判定） |
| F4 | `-23 -15 0 47 30 100` |
| F5 | `-32 -15 -1 64 30 70`（崩碎收缩） |

折算：半尺寸 ≈ (0.24, 0.15, 0.66)~(0.32,0.15,0.655) 单位、中心 x 偏移 ≈ 0~0.1 单位——**判定≈贴碑身的窄高盒**（一块碑只罩住自己脚下）。

`.als`：基础版链路无（仅 `tombstone.ani_ds.als` 为剑鬼 DemonicSwordsman 变体）。角色侧 `attackinfo\` 无 tombstone 条目（实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | TombStoneRain.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\TombStoneRain.skl` | ✅ 实测 | 等级/CD/MP/8 列 level info |
| 注册行 | —（无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | 按 L2 反查技能 44 无命中 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\` | ⛔ 缺失 | 逻辑在引擎 |
| 生成器 PO | tombstonerain.obj | `…\pvf\passiveobject\character\swordman\tombstonerain.obj` | ✅ 实测 | "死亡墓碑生成器"（无 motion/attack 纯壳） |
| 墓碑 PO | tombstone.obj（+tombstone_ds.obj 剑鬼变体） | 同上 | ✅ 实测 | 落碑判定+视觉（etc motion 三视觉层） |
| 墓碑 .ani | tombstone.ani / glow1 / glow2 / dust | `…\passiveobject\character\swordman\animation\` | ✅ 实测 | 1540ms 三段式 + 视觉层 |
| 墓碑 .atk | tombstone.atk | `…\passiveobject\character\swordman\attackinfo\tombstone.atk` | ✅ 实测 | magic/dark/down/push50/lift0/诅咒占位 |
| .chr 条目 | etc motion #97 | `…\pvf\character\swordman\swordman.chr` 行 1070 | ✅ 实测 | Animation/TombStoneEx.ani（强化线，非基础版） |
| .act | — | `…\passiveobject\character\swordman\action\` | ⛔ 无 tombstone act | 行为引擎内置 |
| 关联技能 | tombstoneswamp（247） | `…\pvf\sqr\character\swordman\tombstoneswamp\tombstoneswamp.nut` | ✅ 实测（混淆代码） | 墓碑沼泽=另一技能（用 24370），非本技能链路 |
| 关联强化 | tombstoneex.obj / tombstoneexexplosion.obj / tombstoneex.atk / tombstoneexexplosion.atk / animation\tombstoneex\ | 各对应目录 | ✅ 存在性实测 | E 类批次 |
| 装备层 | tombstone 系 ×18（coat 层抽样） | `…\pvf\equipment\character\swordman\avatar\coat\*\` | ✅ 实测（存在性） | 换装图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/TombStone.img` | sprite_character_swordman_effect_tombstone.NPK | 墓碑本体（tombstone.ani 全帧） | **必需** | ❌ |
| `Character/Swordman/Effect/TombStoneDust.img` | 同上 | 落地尘土 | 可选 | ❌ |
| `Character/Swordman/Effect/TombStoneGlow1.img`、`TombStoneGlow2.img` | 同上 | 光晕层 ×2 | 可选 | ❌ |
| （_ds 变体引 `Character/DemonicSwordsman/...` img） | sprite_character_demonicswordsman_... | 剑鬼变体 | 不需要 | ❌ |

缺失 img：必需 1 张、可选 3 张，**同一个 NPK 一次提取全覆盖**。角色侧无新 img（施法动画未定位；demo 用现有待机/施法姿态）。

## 5. 实现方案草案

**结构映射**：DNF"生成器连续落碑" → 我们"技能 OnUpdate 定时在周围 CreateArea（每碑一个短命 Area）"。每块碑 = 独立 Area = 独立 HitTargets → **天然单次命中/碑，不撞多段命中缺口**（L19 同理）。

### 内容件清单

1. **`DotNet~/Skills/TombStoneRainSkill.cs : SkillLogic`**（FireCircleSkill 范式 + 时间驱动连发）
   - `CooldownMs = 20000`；`TotalTimeMs = 3600`（施法 500 + 落碑 3000 + 余量 100）。
   - 常量：`SpawnIntervalMs = 180`（col0）、`RainDurationMs = 3000`（col1）、`MaxTombStones = 16`（3000/180，col2=8 为同屏上限，demo 不节流）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanIdle)`（施法姿态沿用待机——专属动画未定位）+ `ctx.SetSubState(0)`（已生成计数存 SubState）。
   - `OnUpdate`（t≥500 起，每 180ms 一块）：
     - 生成序号 i = SubState；`offset = SpawnPattern[i % 8] + (i/8) × 环形旋转增量`——**确定性伪随机**：8 个 const 环形偏移（半径 1~2.2 单位）轮转（见 §8 缺口：SkillContext 无随机门面，用 const 表保证帧同步一致）；
     - `ctx.CreateArea(AreaIds.TombStone, casterPos + offset)`（直接世界坐标版 CreateArea；位置取 `ctx.GetTargetPosition()` 同源的施法者位置——实现时经 ctx 现有 API 组合，见 §8 上报②）；
     - `ctx.SetSubState(i + 1)`；到 16 块或 t≥3500 停。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。
2. **`DotNet~/Areas/TombStoneArea.cs : AreaDefinition`**（单碑，BloodBoomArea 范式）
   - `TotalTimeMs = 1540`（tombstone.ani 原时长）、`TickTimeMs = 0`、`EnterActions = { MeleeHit }`；
   - `HalfExtents = (0.35, 0.2, 0.7)`（F1 盒折算放宽：x 取 0.32+落点误差、z 取 131px/100≈0.66 上取 0.7）；
   - `HitReaction { Damage = 55, HitstunMs = 600, KnockbackX = 50, LaunchY = 0 }`（tombstone.atk：down/push50/lift0；Damage=col3 540% 的 demo 折算）；
   - 诅咒（可选档）：`ProcBuffId = BuffIds.Curse, ProcChance = 100`（col6）——见下；
   - `ViewAnimId = AnimId.TombStoneFall`；可选 `ViewBackAnimId = AnimId.TombStoneGlow1`（光晕背层同构用法）。
3. **（可选）`DotNet~/Buffs/CurseBuff.cs : BuffDefinition`**——DNF 诅咒=全属性减益 debuff；无属性消费链 → demo 降级为"暗蚀 tick"（`TotalTimeMs = 8000, TickTimeMs = 2000, TickActions = { CurseDamageTick }`，数值占位）或干脆跳过（推荐 v1 跳过，见 §7）。
4. **无新增 Action**（MeleeHit 现成；若做诅咒 tick 再加 `ActionIds.CurseDamageTick = 10`）。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎内置状态 + casting 500ms | `TotalTimeMs` 前段 + 待机动画（无专属施法动画） |
| 生成器 PO 连续落碑（col0 间隔/col1 时长/col2 上限） | OnUpdate 定时 `CreateArea` + const 环形落点表 |
| 墓碑 PO（tombstone.ani 盒 + tombstone.atk） | `TombStoneArea`（EnterActions 单次命中） |
| [active status] curse（col4-7） | `HitReaction.ProcBuffId/ProcChance`（L6 链路）+ CurseBuff 降级 |
| 落碑期间霸体 | 霸体帧延后 → 跳过（受击照常打断施法，手感差异见 §7） |
| 不可移动 | NumericType.ForbidMove 门面未暴露给技能层 → 沿用现状（不锁移动） |
| 跳跃键中断 | 无跳跃系统 → 跳过（技能自然结束） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.TombStoneRain = 17` + `ButtonToSkill` case 9（新键，如 M） |
| AreaId | `Runtime\AreaDefinition.cs` | `AreaIds.TombStone = 7` |
| AnimId | `AnimConfigRegistry.cs` | `TombStoneFall = 67`、`TombStoneGlow1 = 68`（可选）、`TombStoneGlow2 = 69`（可选）、`TombStoneDust = 70`（可选） |
| BuffId（可选） | `Runtime\BuffDefinition.cs` | `BuffIds.Curse = 7` |
| json 注册 | `LSAnimClipRegistrar.cs` | `RegisterOne` ×1~4（tombstone.json 必需） |
| 图集 | `LSAnimResComponentSystem.cs` | `TombStone.img.bytes`（必需；可选 +Dust/Glow1/Glow2） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 20000 ms | 20000（直用） |
| 施法 | 500 ms | 500（前段空转） |
| 落碑节奏 | 180ms/块 × 3000ms（≈16 块，同屏上限 8） | 同左（不节流 16 块） |
| 单碑判定 | 盒半尺寸 ≈(0.24~0.32, 0.15, 0.66) | HalfExtents (0.35, 0.2, 0.7) |
| 单碑伤害 | col3 540→4320（Lv1→70） | 55（固定） |
| 单碑反应 | down / push50 / lift0 / blow | Hitstun 600 / Kb 50 / Ly 0 |
| 诅咒 | col6=10000（100%）、col4/5/7 参数（未考证） | v1 跳过；可选 CurseBuff 8s/2s tick |
| 碑生命周期 | tombstone.ani 1540ms | Area TotalTimeMs 1540 |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `tombstone.ani` / glow1 / glow2 / dust | 常规节（FRAME/IMAGE/DELAY/ATTACK BOX） | **全部可被现有 ani 子命令翻译**（F3=1000ms 长帧属正常���顿，非 10s 级悬停，不需钳制） |
| `tombstone.atk` | `.atk` 无子命令 + **[active status] curse 7 参数节** | 手抄；atk 子命令设计时 [active status] 需按 (状态类型, 概率, 等级, 时长, 额外…) 建模（比 L6 四参多 curse 特有参数位） |
| `tombstone.obj` / `tombstonerain.obj` | `.obj` 无子命令（生成器壳无任何 motion——obj 子命令要容忍空定义） | 手工映射为 Area 编排（§5 已给） |
| `TombStoneRain.skl` | `.skl` 无子命令（8 列矩阵） | 手抄 |
| `tombstoneswamp.nut` | mod XOR compilestring 混淆（另一技能，记档不译） | 不涉及 |

结论：本技能 .ani 资源**全部可被现有 ani 子命令翻译**（无 .als、无超长 DELAY、无特殊节）；实质缺口 = `.atk`（含 curse 状态节建模）+ `.obj`（空壳容忍）+ `.skl` 三类无子命令，计 3 条。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 落碑期间自身霸体 | 霸体帧延后（AnimFrameData 加 damageType） | 跳过——被打会中断（我们受击不打断施法，见下） |
| 落碑期间不可移动 | ForbidMove 门面未暴露技能层 | 沿用现状不锁（DNF 的"罚站"风险本来就是代价面） |
| 受击-施法互斥缺失（R1-A4 已录） | 我们永不打断施法 → 霸体+罚站的"风险换输出"设计点整体空转 | 数值面平衡（CD/伤害）即可，手感差异：DNF 站桩有风险、我们无 |
| 跳跃键中断收尾 | 跳跃系统缺失（R1-A2） | 跳过（自然结束 3.6s） |
| 诅咒 debuff（属性减益） | **属性数值无伤害消费链**（R1-A4 最重缺口）+ 无 Curse 状态 | v1 跳过（HitReaction.ProcChance 不配）；演示需要时降级为暗蚀 tick Buff |
| 落点随机 | SkillContext 无确定性随机门面（LSRng 未暴露） | **const 环形落点表**（8 点轮转+层旋转）——确定性且视觉近似随机 |
| 同屏上限 8 节流（col2） | 无（Area 生命周期自管理） | 不节流（16 块短命 Area 开销可控） |
| 暗元素属性 | 元素系统缺失 | 记档跳过 |
| 音效/屏震 | 延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①8 列 level info 中 col4/col5/col7 的精确语义（推断表见 §1，无 nut 消费佐证）；②基础版施法动画（仅强化线有 TombStoneEx.ani；推断走通用 casting 动作）；③`tombstone.obj [int data] 2 50` 语义；④生成器落点算法（随机/环形/追踪——引擎内置不可见，demo 用 const 表自定）。
- **新缺口上报**：①**SkillContext 缺世界坐标创建与确定性随机门面**——落点类技能（墓碑/陨石/箭雨族）需要"以施法者为中心、偏移创建"的 API：现仅有 `CreateAreaInFront`（前方直线偏移）与 `CreateArea(position)`（需自算世界坐标，而 ctx 未暴露施法者 Position——实现期需加 `ctx.GetCasterPosition()` 或 `CreateAreaAround(offset)` 门面，属框架层小增量）；确定性随机建议暴露 `ctx.Rand(min,max)`（LSRng 底层已有）；②**诅咒类属性异常状态**（stat-debuff 家族）——与"属性数值无伤害消费链"同根，随该系统一并立项；③**纯生成器形态 .obj**（tombstonerain.obj 无 motion/attack）——obj 子命令需容忍纯壳定义。
- **给下轮的经验**：引擎内置技能若在 passiveobject 侧出现"无 motion 无 attack 的壳 obj + 完整子 obj"，壳=生成器、逻辑全引擎——直接按 .skl 数值 + 子 obj 数据反推时序即可，不必再找脚本。avatar 子树计数用 `ls <单层>` 分层做（本批实测 `*/*/` 通配 2 分钟超时，C4 再证）。
