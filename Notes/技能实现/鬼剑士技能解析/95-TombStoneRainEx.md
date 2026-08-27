# 墓碑三绝阵（TombStoneRainEx）

> 技能ID 95 | 级别 E（**预分类纠偏：文件名以 Ex 结尾但不是 TP 强化被动——[type] active、skill class 3，鬼泣二觉替换型主动技**（死亡墓碑的 70 级进化版），本文按"基础技对照 + 增量走读"的 E 级深度，机制走读对齐 096 先例） | 可实现性 🔶（三碑三角落点/落下判定/定时爆炸的 Area 编排主干直译；"降低暗抗+我方转暗属性光环"撞属性消费链+阵营判定双缺口→仅剩视觉；"再按引爆"撞二段交互门面） | 分析日期 2026-08-22 | 批次 E7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 墓碑三绝阵 | `skill\Swordman\TombStoneRainEx.skl` [name] |
| 英文名 | TombStoneRainEx（取 skl 文件名；[name2] 实测 `Tombstone Triangle`——直译"墓碑三角"，与 static 三落点互证） | 同上 |
| 职业 | 鬼泣二觉（[skill fitness second growtype] `1 2`；[second growtype maximum level] 12 槽**第 4/5 位=30**（0 基，R6-C4 职业判定捷径）；[skill fitness growtype] 空 = 一觉树无此技） | 同上 |
| 学习等级 | 70（[required level range] 2）；前置 44 死亡墓碑 Lv5 | 同上 |
| 最高等级 | 50（二觉档实际 30） | 同上 |
| 类型 | active（skill class 3 召唤类）/ 魔法 | 同上 |
| 指令 | →←↑→ + Z（指令 MP 优惠 50%/50%） | 同上 |
| CD | 50000 ms | 同上 [cool time] |
| MP | 1200 → 2520 | 同上 [consume MP] |
| 读条 | 500 ms | 同上 [casting time] |
| 特殊消耗 | 无色小晶块 ×2（道具 3037） | 同上 [consume item] |
| static data | `-150 50 150 50 0 -50`——**三对 (x,y) = 三块墓碑落点**（(-150,50)/(150,50)/(0,-50) 三角阵，与 [name2] "Triangle"及 col2=3 三重互证；推断标注，px÷100≈1.5 单位半径的三角） | 同上 + level property |
| 一句话效果 | 身周落下 3 块墓碑攻击并概率诅咒；墓碑存在期间散发降低敌暗抗、使我方攻击转暗属性的光环；10 秒后或再按技能键，墓碑暗属性爆炸后消失 | 同上 [explain] |

**level property 模板解码（14 列 + 11 向量，L21 法全解，Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 掉落数量 | (-1,2,1.0) | col2 = **3（恒定）** |
| 魔法攻击力 | (-1,3,1.0) | col3 = 940 → 10635 % |
| 诅咒机率 | (-1,5,0.1) | col5 = 100→2550 → **10% → 255%** |
| 诅咒持续时间 | (-1,6,0.001) | col6 = 10000 → **10 s 恒定** |
| 力/体/智/精神各减少 | (-1,7,1.0) | col7 = 210 → 455 |
| 诅咒Lv | (-1,4,1.0) | col4 = Lv72 → Lv170 |
| 爆炸魔法攻击力 % | (-2,12,1.0) | col12 = 3874 → 43825 % |
| 爆炸魔法攻击力 +固定 | (-1,9,1.0) | col9 = 3874→43825（pvp 215→2435——**%% + 固定值双段式**，模板行 `<int>%% + <int>` 两占位） |
| 墓碑存在时间 | (-1,10,0.001) | col10 = 10000 → **10 s 恒定** |
| 降低暗属性抗性 | (-1,11,1.0) | col11 = **-10 → -255**（负值=减抗幅度） |
| 墓碑的光环范围 | (-1,13,1.0) | col13 = **200 px 恒定** |

未引用列：col0/col1=0（死列）、col8=500（恒定，无向量引用——疑引擎直读的爆炸延迟或死列，未考证）。

**与基础 044 死亡墓碑对照**：生成器连落 16 块/3 秒雨 → **定点 3 块三角/各存 10 秒**；单体小伤害×大量 → 大额攻击+独立爆炸；诅咒从"参数未考证的 4 列" → **全列解码**（044 的 col4/5/7 推断表可借本表交叉印证：诅咒Lv/机率‰×0.1/属性减值）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（纯引擎内置，F3——096 同型第 4 例）

- load_state **无 pushState**（grep tombstoneex 无命中）；`sqr\character\swordman\` 无本技 nut/appendage（momentaryslash 空壳目录同批另见，本技无残留）；PO 创建无脚本——**全部行为由引擎按 skl+obj 数据驱动**（基础 044 同型）。
- 被动对象 ×2（passiveobject.lst 实测 11254-11257 行）：
  - **PO 20061 = `Character/Swordman/TombStoneEx.obj`**（墓碑本体+光环）；
  - **PO 20062 = `Character/Swordman/TombStoneExExplosion.obj`**（墓碑爆炸）。
- 施法动画：.chr etc motion **#97** = `Animation/TombStoneEx.ani`（行 1070 实测；`swordman_header.nut:267` 常量 `CUSTOM_ANI_TOMBSTONEEX <- 97` 双证）——**基础版"施法动画未定位"的悬案就此收口**：TombStoneEx.ani 属本技（基础版无专属动画，044 §8 ②的推断"通用 casting"仍成立但资源归属澄清）。

### 2.2 引擎内置行为重建（推断标注，无脚本佐证）

```
施放（读条 500ms，播 TombStoneEx.ani）：
  消耗 MP + 无色×2 → 按 static 三落点创建 PO 20061 ×3（三角阵）
每块墓碑（TombStoneEx.obj，int data `3 50 10`）：
  Start.ani 560ms：F2-F4 攻击盒（高窄柱）→ 落地命中：col3% 魔法伤害
    + 概率诅咒 col5×0.1（10%~255%），Lv col4，10s，力体智精 -col7（atk 无 [active status] 节——参数运行时注入，044 同款）
  → Stay.ani 10000ms 悬停（L23 待事件型；时长=col10）
    期间 Circle.ani 魔法阵光环（视觉 1100ms+消失 300ms）：
    半径 col13=200px 内敌暗抗 -|col11|，我方攻击转暗属性（引擎内置，无脚本/数据可读）
10s 到（或再按技能键）→ End.ani 600ms + 创建 PO 20062 爆炸：
  Explosion.ani 980ms，F1-F6 攻击盒（宽地面盒）→ col12% + col9 固定 暗属性伤害（atk down/push50）
```

- **"再按引爆"**：explain 明示；无脚本可读（引擎监听）——二段交互门面（R4-B16）第 6 例消费方。
- 三碑共享同一 CD/施法，光环三圆 200px 半径在三角边长 ~3 单位布局下基本连片。

### 2.3 被动对象（两个 .obj 完整实测）

**PO 20061 tombstoneex.obj（墓碑+光环）**：

| .obj 节 | 值 |
|---|---|
| [attack info] | `AttackInfo/TombStoneEx.atk`：magic / dark / **down** / push **50** / lift 0 / hit down（与基础 tombstone.atk 同参数族） |
| [etc motion] 五相位 | Start.ani（9 帧 560ms，**F2-F4 攻击盒** `-13~-20 -230 -20` + `61 200 40`——高 2 单位窄柱）→ Stay.ani（1 帧 **10000ms 悬停**）→ End.ani（6 帧 600ms）→ Circle.ani（3 帧 1100ms，魔法阵）→ CircleDisappear.ani（2 帧 300ms） |
| [string data] | `Particle/TombStoneExExCrack.ptl`（粒子：L5 翻译+系统双缺口） |
| [int data] | `3 50 10`（3=数量对位 col2；50/10 未考证） |
| 其余 | width 1 1 / floating 0 / pass all / piercing 1000（同族） |

**PO 20062 tombstoneexexplosion.obj（爆炸）**：

| .obj 节 | 值 |
|---|---|
| [basic motion] | `TombStoneExExplosion/Explosion.ani`（14 帧 980ms，**F1-F6 攻击盒** `-108 -60 -20` + `221 119 253` → x[-1.08,1.13] z[-0.2,2.33] 单位宽地面盒；F1 flag 1） |
| [attack info] | `AttackInfo/TombStoneExExplosion.atk`：magic / dark / down / push 50 / lift 0 |
| [int data] | `3 300`（未考证） |

### 2.4 动画关键帧表（tombstoneex\ 23 文件 + tombstoneexexplosion\ 16 文件，抽关键件实测）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img |
|---|---|---|---|---|---|
| start.ani（落碑） | 9 | 560ms | F4=1 | **F2-F4**（§2.3） | StoneStartBottomNormal.img |
| stay.ani（存在期） | 1 | **10000ms** | 无 | 无 | 同上（L23 超长帧=col10 载体） |
| end.ani（消失） | 6 | 600ms | 无 | 无 | StoneEndUpDodge/Normal.img |
| circle.ani（光环阵） | 3 | 1100ms | 无 | 无 | MagicCircle.img |
| circledisappear.ani | 2 | 300ms | 无 | 无 | MagicCircle.img |
| explosion.ani（爆炸） | 14 | 980ms | F1=1 | **F1-F6**（§2.3） | explosionDodge125.img |
| character tombstoneex.ani（施法） | — | — | — | — | sm_body（.chr #97） |
| .als ×6（start/stay/end/circle/circledisappear/explosion） | — | — | — | — | 挂层视觉（WordUp/BottomDodge、stone*_particle 等） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | TombStoneRainEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\TombStoneRainEx.skl` | ✅（243 行） | 14 列全解 + 二觉归属 |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无 | 纯引擎（F3） |
| 主 nut | —（不存在） | `…\sqr\character\swordman\`（grep 0 命中） | ⛔ 无 | 引擎内置 |
| PO 注册 | passiveobject.lst:11254-11257 | `…\pvf\passiveobject\passiveobject.lst` | ✅ 实测 | 20061 / 20062 |
| PO 定义 | tombstoneex.obj / tombstoneexexplosion.obj | `…\passiveobject\character\swordman\` | ✅ 实测 | §2.3 |
| PO .atk | tombstoneex.atk / tombstoneexexplosion.atk | `…\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | §2.3 |
| PO .ani/.als | tombstoneex\ ×23（.als ×5）+ tombstoneexexplosion\ ×16（.als ×1） | `…\passiveobject\character\swordman\animation\` | ✅ 实测 | §2.4 |
| .chr 条目 | etc motion #97（行 1070） | `…\character\swordman\swordman.chr` | ✅ 实测 | TombStoneEx.ani 施法 |
| 常量 | swordman_header.nut:267 | `…\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_TOMBSTONEEX=97 |
| 角色 .ani | tombstoneex.ani | `…\character\swordman\animation\` | ✅ 实测 | sm_body 单图集（L16） |
| 粒子 | TombStoneExExCrack.ptl / TombStoneExExplosion.ptl | .obj [string data] 引用 | ⛔ L5 双缺口 | 落碑裂纹/爆炸视觉 |
| 基础技文档 | 044-TombStoneRain.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 结构对照基准 + 诅咒列交叉印证 |
| 关联 TP | TombStoneRainExp.skl（技能 225，E6 批） | `…\skill\Swordman\` | ✅ 存在 | 见 §8 新旧关系 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/TombStoneEx/StoneStartBottomNormal.img` | sprite_character_swordman_effect_tombstoneex.NPK | 墓碑本体（start/stay 共用） | **必需** | ❌ |
| `…/TombStoneEx/StoneStartUpNormal.img` | 同上 | 墓碑上层 | **必需** | ❌ |
| `…/TombStoneEx/MagicCircle.img` | 同上 | 光环魔法阵 | **必需** | ❌ |
| `…/TombStoneEx/explosionDodge125.img` | 同上 | 爆炸主层 | **必需** | ❌ |
| `…/TombStoneEx/explosionNormal125.img` | 同上 | 爆炸常态层 | **必需** | ❌ |
| `…/TombStoneEx/StoneEndUpDodge.img`、`StoneEndUpNormal.img`、`WordUpDodge.img`、`WordBottomDodge.img`、`explosionWaveNormal200.img`、`stonesmall_particle.img`、`stone1~3normal_particle.img` | 同上 | 消失/字纹/冲击波/粒子层 | 可选 | ❌ |
| sm_body0000.img | （已入库） | 施法动画 | 必需（共享） | ✅ |

**缺失 img：必需 5 张、可选 10 张——全部同一个 NPK 一次提取；无跨目录借图。** 粒子 2 个 .ptl（L5 缺口，特效 ani 替代/跳过）。

## 5. 实现方案草案（号段：SkillIds 35 / AnimIds 187-191 / AreaIds 41-42，E7 批内顺延；撞号无妨 L18）

### 内容件清单

1. **`DotNet~/Skills/TombStoneTriangleSkill.cs : SkillLogic`**（044 TombStoneRainSkill 的"定点三角"变体，FireCircle 范式）：
   - `CooldownMs=50000`；`TotalTimeMs=700`（读条 500 + 施法 200；三碑是独立 Area，不受技能时长约束——同 044"技能短、区域长"）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanTombStoneEx)`（#97）+ 三次 `ctx.CreateArea(AreaIds.TombStoneEx, casterPos + TriangleOffset[i])`——偏移表 const 三点 `(-1.5,0.5)/(1.5,0.5)/(0,-0.5)` 单位（static 三落点÷100，R2-A10 确定性 const 落点同款）。
   - `OnUpdate`：t≥500+10000（存在期结束）且 SubState==0 → 在三碑位置 `ctx.CreateArea(AreaIds.TombStoneExExplosion, pos_i)`；SubState=1。**再按引爆不做**（时间轴等价，§7）。
2. **`DotNet~/Areas/TombStoneExArea.cs : AreaDefinition`**（单碑，044 TombStoneArea 直改）：
   - `TotalTimeMs=11000`（start 560 + stay 10000）、`TickTimeMs=0`、`EnterActions={MeleeHit}`（落下单次命中，L19 段间档）；
   - `HalfExtents=(0.4,1.0,1.0)`（F2-F4 高窄柱折算：x[-0.2,0.4] y±1.0（柱宽 200px 取整）z 0~2.0）；
   - `HitReaction{Damage=90, HitstunMs=600, KnockbackX=50, LaunchY=0}`（atk down/push50/lift0；Damage=col3 940% demo 折算）；
   - 诅咒：`ProcBuffId=BuffIds.Curse, ProcChance=10`（col5×0.1 @Lv1；044 CurseBuff 同款降级或跳过）；
   - `ViewAnimId=AnimId.TombStoneExStart`（560ms 后自然停在尾帧或切 stay 循环 json——实现期二选一）+ 可选 `ViewBackAnimId=AnimId.TombStoneExCircle`（魔法阵光环视觉层）。
3. **`DotNet~/Areas/TombStoneExExplosionArea.cs : AreaDefinition`**（碑爆）：
   - `TotalTimeMs=980`、`EnterActions={MeleeHit}`、`HalfExtents=(1.1,0.6,1.3)`（F1 盒 x[-1.08,1.13] 折算）；
   - `HitReaction{Damage=250, HitstunMs=800, KnockbackX=50, LaunchY=0}`（atk down/push50；Damage=col12 3874%+col9 3874 双段折算）；
   - `ViewAnimId=AnimId.TombStoneExExplosion`。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎施法 + TombStoneEx.ani（#97） | Skill.PlayAnim（sm_body 已入库，零新增角色资源） |
| static 三落点三角阵 | const 偏移表 ×3 CreateArea（R2-A10 同款确定性落点） |
| 墓碑 PO（Start 落下判定→Stay 悬停→光环） | `TombStoneExArea`（单次落下命中 + 11s 存续视图） |
| 光环（敌暗抗 -10~-255 + 我方转暗属性，半径 200px） | **⛔ 属性消费链（R1-A4）+ 阵营判定（R1-A3）双撞** → 只留 MagicCircle 视觉层（§7） |
| 诅咒（Lv72-170/10s/四维 -210~-455/机率 10-255%） | HitReaction.ProcBuffId+ProcChance（L6 链路）+ CurseBuff 降级（044 同款：属性减值无消费 → v1 跳过或暗蚀 tick） |
| 10s 到/再按 → 爆炸 PO | 技能 OnUpdate 定时第三批 Area；**再按不做**（二段交互门面 R4-B16 第 6 例） |
| 暗属性伤害 | 元素系统缺失 → 无属性直伤（惯例） |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.TombStoneTriangle = 35` + 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `TombStoneEx = 41`、`TombStoneExExplosion = 42` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanTombStoneEx = 187`、`TombStoneExStart = 188`、`TombStoneExCircle = 189`、`TombStoneExExplosion = 190`（+可选 191 End） |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×4；图集 1 个 NPK（必需 5 张） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 50000 ms | 50000 直用 |
| 读条/施法 | 500 ms / TombStoneEx.ani | 500 + 200 |
| 三碑落点 | static (-150,50)/(150,50)/(0,-50) px | 三点 const（±1.5/±0.5 单位） |
| 单碑落下伤害 | col3 940%→10635% 魔攻 | 90 |
| 落下反应 | down / push50 / lift0 | Hitstun 600 / Kb 50 / Ly 0 |
| 诅咒 | 10%→255% / 10s / Lv72→170 / 四维 -210→-455 | 10% + CurseBuff（降级）或跳过 |
| 光环 | 半径 200px，暗抗 -10→-255，转暗属性 | **砍**（仅视觉） |
| 存在期 | col10 = 10 s | Area 11000ms |
| 爆炸 | col12 3874→43825% + col9 3874→43825 | 250 |
| 爆炸盒 | F1 x[-1.08,1.13] z[-0.2,2.33] | HalfExtents (1.1,0.6,1.3) |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| TombStoneRainEx.skl | `.skl` 无子命令（14 列 + 11 向量 + 二觉 12 槽） | 手抄（本档已全解码）；skl 子命令同前议 |
| tombstoneex.obj / tombstoneexexplosion.obj | `.obj` 无子命令 | 手工映射双 Area（§2.3 已给全相位表） |
| tombstoneex.atk / tombstoneexexplosion.atk | `.atk` 无子命令；**无 [active status] 节**（诅咒运行时注入比 7 零占位更彻底） | 手抄 5 值；atk 子命令诅咒建模沿用 044 输入 |
| stay.ani | `[DELAY] 10000` 超长悬停帧 | L23 既有缺口：钳制/约定手改（044 F3=1000ms 那次不需钳制，本例需要） |
| TombStoneExExCrack.ptl 等 ×2 | `.ptl` 无子命令+无粒子系统（L5） | 特效 ani 替代或跳过 |
| 全部 .ani/.als | 常规节 | **现有 ani/als 子命令全覆盖** |

本技能翻译缺口 5 类（.skl/.obj/.atk/超长 DELAY/.ptl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 光环：敌暗抗降低 + 我方攻击转暗属性（半径 200px，持续 10s） | **属性数值无伤害消费链（R1-A4）+ 队伍/阵营判定（R1-A3）双撞** | 只留 MagicCircle 视觉层；数值光环不做（该缺口补齐后回补——受益清单第 4 例） |
| 再按技能键提前引爆 | 技能二段交互门面（R4-B16，第 6 例消费方） | 固定 10s 自动引爆（时间轴等价，损失操作深度） |
| 诅咒（四维减值 debuff） | 属性消费链 + 无 Curse 状态（044 同款） | ProcChance 挂 CurseBuff 降级暗蚀 tick，或 v1 跳过 |
| 三碑独立存在期/爆炸时序 | 无（Area 生命周期自管理） | 直译（11s Area + 定时爆区） |
| 暗属性 | 元素系统缺失 | 无属性直伤 |
| 落碑粒子裂纹 | 粒子系统缺失（L5） | ani 替代/跳过 |
| 读条/无色×2/SP 70 | 延后档 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①光环机制的引擎实现（降抗/转属性均无脚本与数据载体——比 096 结晶子系统更彻底的"纯 explain 机制"）；②int data `3 50 10` 与 `3 300` 中 50/10/300 语义；③col0/col1=0、col8=500 无引用；④"再按"窗口与 CD 交互；⑤TombStoneEx.ani 帧表未逐帧（施法动画，非判定载体）。
- **新旧 TP 并存关系结论（本批专项①）**：**95 与 225（TombStoneRainExp，E6 批）不是两代 TP**——95 是 [type] active 的二觉**替换主动技**（前置=基础技 Lv5、二觉 30 级档、独立 CD/无色消耗）；死亡墓碑的 TP 是 225。**决定性证据：基础技 TombStoneRain.skl 的 [feature skill index] = 225**（非 95）——基础技的功能强化链唯一指向 Exp。95/225 是"替换技与 TP 各司其职"的并存（DNF 二觉后主动技升级 + TP 继续强化新版本）。
- **给 044 的回填**：①044 §1 "feature skill index 225" 已记 ✓；②044 §8 未考证 ②"基础版施法动画仅强化线有 TombStoneEx.ani"——**澄清：TombStoneEx.ani（.chr #97）是墓碑三绝阵（95）的施法动画，不是 044 的"强化线"资源**；044 基础版仍无专属施法动画（通用 casting 推断维持）；③044 §1 诅咒列推断表（col4=Lv/col5=机率/col7=四维减）与本病 14 列全解交叉印证成立，可提升置信。
- **给轮间经验候选**：二觉替换技的 static data = 召唤落点表（成对 x,y）——与 096 的 static[0] 单落点互证，"static 值个数与召唤物数量成对"可作族规律。
