# 鬼连斩 · 极（SwordGhost26）

> 技能ID 118 | 级别 C（被动·主动技强化型——[鬼连斩]第 4 段追加开关，预判"像主动技"已验 [type]=passive 证伪） | 可实现性 ✅ | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼连斩 · 极 | `skill\Swordman\SwordGhost\SwordGhost26.skl [name]` |
| 英文名 | SwordGhost26（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype] 5） | 同上 |
| 学习等级 | 30（前置：鬼连斩 Lv1，pre required skill `127 1`） | 同上 [required level] / [pre required skill] |
| 最高等级 | 60（growtype 段上限：growtype 1 与 5 各可学——`1 0 0 0 0 1`，即剑魂 1 也开放，mod 配置） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | passive（skill class 1） | 同上 [type] |
| 指令 / CD / MP | 无（效果并入鬼连斩） | 同上（无相应节） |
| static data | `100`（冲波范围 100%，见 level property） | 同上 [dungeon][static data] |
| 一句话效果 | 学习后，[鬼连斩]的最后一击后追加一次交叉斩击 | 同上 [explain] |

**level property（1 列，Lv1 → Lv60）**：模板 `追加交叉斩击攻击力 : <int>%% x 3 / 追加交叉斩击范围 : <int>%%`，
向量 col0=`(-1,0,1.0)`、范围=`(0,0,1.0)`→static[0]。
- col0 = 追加攻击力 790% → 6241%（每级 +79）；
- 范围 = static[0] = **100%**（恒定，不随级变）。
- pvp 段同构缩水（Lv1=79 → Lv60=624）。
- "x 3"疑为三段判定（模板字面），无 nut 印证，标推断（§8）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（实装联动，非引擎内置）

本技能**不 pushState、无独立 nut**，但有两条真实脚本联动（均实测）：

1. **等级同步**——`sqr\character\swordman\passive_skill_swordman.nut` case `SKILL_SPEEDSLASHUPPER`：
   ```
   local sourceSkillLevel = sq_GetSkillLevel(obj, SKILL_SPEEDSLASH);
   upperSkill.setLevel(1, sourceSkillLevel);   // 本被动等级恒同步 = 鬼连斩等级
   ```
   常量对位（`swordman_header.nut` 实测）：`SKILL_SPEEDSLASH <- 127`、**`SKILL_SPEEDSLASHUPPER <- 118`**——
   即本技能在脚本侧的正式身份就是"鬼连斩 Upper"。
2. **第 4 段门禁**——鬼连斩（127）实现里，子状态流转在"学得强化技 118"时进入追加段
   （`127-speedslash.md` §2 实证：子状态 0 → 学得 118 → 子状态 1，PO dword **43** = 追加上挑/交叉段，
   用 CUSTOM_ANI_SPEEDSLASH2=278）。未学得则回站立。

### 2.2 机制归纳

```
学得 118（且等级同步=鬼连斩等级）→ 鬼连斩第 3 段收招后不回站立：
  追加段（speedslashupper_body / speedslash2 演出）：
    交叉斩击 ×（模板 x 3 推断三段判定），攻击力 = col0%（790%~6241%），范围 100%
  → 收招回站立
```

### 2.3 动画与资源（实测）

- **角色追加段动画**：`speedslashupper_body.ani`（sm_body 图集直读，L16）+ 边车 `.als`
  引用 `Effect/Animation/SpeedSlashUpper/` 特效族（SpeedSlashUpperNEWEffect01/02 等）；
- `speedslash2.ani`（CUSTOM_ANI 278，127 文档对位 dword 43）同为 sm_body；
- **特效目录** `character\swordman\effect\animation\SpeedSlashUpper\`（实测存在），其 img 引用见 §4。

### 2.4 动画关键帧表

| 动画 | 帧数 | 引用 img | 备注 |
|---|---|---|---|
| speedslashupper_body.ani | —（sm_body） | sm_body%04d.img | 追加段本体（"upper" 命名 = SPEEDSLASHUPPER 对位） |
| speedslash2.ani | —（sm_body） | sm_body%04d.img | dword 43 段演出（127 文档对位） |
| SpeedSlashUpper 特效族 | — | BladeSpirit/GhostSlashBS05/SpeedSlashUpper/Dust imgs | .als overlay 视觉层 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SwordGhost26.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SwordGhost\SwordGhost26.skl` | ✅ 实测 | 1 列数值 + static 100 |
| lst 条目 | swordmanskill.lst 217-218 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 118 → 本 skl |
| 被动注册 | passive_skill_swordman.nut（case SKILL_SPEEDSLASHUPPER） | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | 等级同步逻辑 |
| 常量 | swordman_header.nut:149/168 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | SKILL_SPEEDSLASH=127 / UPPER=118 |
| 基础技联动 | speedslash 子状态 1 + PO dword 43 | `…\pvf\sqr\character\swordman\5_ghostsword\speedslash\`（127 文档实证） | ✅（详见 127-speedslash.md §2） | 追加段门禁与演出 |
| 角色 .ani | speedslashupper_body.ani（+.als）/ speedslash2.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 追加段动画 |
| 特效 | SpeedSlashUpper\*.ani | `…\pvf\character\swordman\effect\animation\SpeedSlashUpper\` | ✅ 实测 | 追加段视觉 |
| .atk | —（追加段命中走共享 PO 24349 的 etc attack info，F5 族） | `…\pvf\passiveobject\unclebang_shared_passive_object\swordman\` | ✅（F5 链路，127 文档已走读） | 命中参数 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 追加段本体动画 | 必需（共享） | ✅ 已入库 |
| SpeedSlashUpper\en01~en09.img（9 张） | sprite_character_swordman_effect_speedslashupper.NPK | 交叉斩特效 | 可选（视觉层） | ❌ 未入库 |
| BladeSpirit\001~004.img（4 张） | sprite_character_swordman_effect_bladespirit.NPK | 剑影特效（特效 ani 跨目录复用，L14 惯例） | 可选 | ❌ 未入库 |
| GhostSlashBS\GhostSlashBS05.img | sprite_character_swordman_effect_ghostslashbs.NPK | 同上 | 可选 | ❌ 未入库 |
| Common\CommonEffect\Dust\Dust01/02.img | sprite_common_commoneffect_dust.NPK | 尘土 | 可选 | ❌ 未入库 |

缺失 img：**必需 0 / 可选 16**（本体动画零缺——追加段可直接落地，特效后补）。

## 5. 实现方案草案

### 内容件清单（SkillIds 从 35 起顺延，L18 记账；与 127 鬼连斩实现联动）

我们无"被动改造已有技能"的系统——**落地形态 = 鬼连斩（127）实现时直接内置第 4 段**
（或出独立"鬼连斩·极"键位变体）。按 127 文档已走读的连段结构：

1. **鬼连斩 SkillLogic 扩展**（若 127 已按连段子状态机实现，本被动 = 追加子状态 4）：
   - 第 3 段结束（`OnUpdate` 段末）→ 不回待机，`ctx.SetSubState(4)` + `ctx.PlayAnim(AnimId.SwordmanSpeedSlashUpper)`；
   - 第 4 段帧触发攻击盒：`ctx.SetAttackHitbox(…)` + `ctx.ClearHitTargets()`（段间重开命中，L19 段间档已通）；
   - 若按"x 3"三段判定：双同心 Area Tick 或段内 3 次帧触发（L19 同段定时档）；
   - HitReaction.Damage = 鬼连斩基准 × col0%（demo 固定档 790%，Lv1 原值）。
2. **独立键位变体**（更省事，推荐）：`GhostSlashExtremeSkill : SkillLogic`（SkillIds 35）——
   "四段完整版鬼连斩"整技直发，数值直抄 127 三段 + 本技能第 4 段。
3. **注册点**：AnimId `SwordmanSpeedSlashUpper = 185`（speedslashupper_body.ani，sm_body 已在库，零 img 前置）。

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| 追加段攻击力 | col0：790%（Lv1）→ 6241%（Lv60） | 790% 固定档 |
| 追加段范围 | static 100% | 与鬼连斩本体段同宽 |
| 段数 | x 3（推断） | 1 段（先验证手感，三段后加） |
| 等级同步 | UPPER.setLevel(=SPEEDSLASH level) | 无等级系统，固定档 |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| SwordGhost26.skl | `.skl` 无子命令（1 列×60 级 + static 1 槽） | 手抄 2 值；全局已知缺口 |
| speedslashupper_body.ani.als | 全为已支持节（[use animation]） | 现有 als 子命令覆盖 |
| 各 .ani | 常规节（+已记档的 [SHADOW] 类） | 现有 ani 子命令覆盖 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 被动学得门禁（学没学 118 决定第 4 段有无） | 无技能树/学得系统（延后档：等级缩放同族） | demo 直接内置第 4 段（或独立键位变体），门禁不做 |
| 等级同步机制（UPPER 等级 = 鬼连斩等级） | 无等级系统（延后档） | 固定档 |
| "x 3"三段判定 | 数值语义未完全印证（§8） | 先做单段，三段作扩展 |
| 特效层 16 张 img 未入库 | 资源提取事务（用户侧） | 本体先行（sm_body 已在库），特效后补 |

## 8. 存疑与缺口上报

**未考证项**
1. `追加交叉斩击攻击力 : <int>%% x 3` 的 "x 3" 语义——三段判定（推断）or 显示文案残留；
   127-speedslash.md 亦记"鬼连斩本体攻击力额外适用的数据通道未见"，两处合并待实现期用 F5 共享 PO
   （swordman_shared.obj dword 43 的 etc attack info）对位验证。
2. speedslashupper_body.ani 与 speedslash2.ani（dword 43）谁是追加段最终演出——两 ani 均为 sm_body 本体，
   不影响资源结论，实现期二选一。

**新系统级缺口**：无（连段段间命中已通 L19；"被动改造主动技"用整技内置规避，不单独立项）。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
