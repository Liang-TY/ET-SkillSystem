# 鬼人化（SwordGhost4）

> 技能ID 123 | 级别 C | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼人化 | `SwordGhost/SwordGhost4.skl [name]`（无 [name2]，英文名取文件名） | skl |
| 英文名 | SwordGhost4（header 别名 `SKILL_BLADESPIRIT <- 123`——"BLADESPIRIT/剑鬼形态"即本技能） | skl 文件名 + swordman_header.nut |
| 职业 | 剑影（[skill fitness growtype] 0-5 全列，[growtype maximum level] `50 0 0 0 0 50`——gt0/gt5 可学 50） | skl |
| 学习等级 | 15 | skl `[required level]` |
| 最高等级 | 70（level info 70 行实存） | skl `[maximum level]` |
| 类型 | [passive]（skill class 1） | skl `[type]` |
| 一句话效果 | 与幻鬼之魂融合：剑影基本攻击/前冲攻击变更、前冲攻击后可立即施放技能；[鬼斩][三段斩]变更为剑影特殊形态；[后跳]中可发动空中斩击 | skl `[explain]` |

**level property（2 列 × 70 级，dungeon/pvp 同表）**：
- col0 技能发动速度：100→307%（`-1 0 1.0`）；
- col1 [鬼步]剑术技能发动速度：154→473%（`-1 1 1.0`）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- **`passive_skill_swordman.nut` case 123**：gt 无限制，skill_level>0 即挂 appendage
  `character/jg_swordman/swordghost4/ap_swordghost4.nut`——**空壳**（onStart/onEnd 仅判空）。
- **`passive_skill_swordman.nut:181-184 procSkill_Swordman`**：等级>0 且 gt 0/5 →
  `setGhostSkillEnable(obj, SKILL_SWORD_GHOST_21(59 大回天), STATE_SWORD_GHOST_21(125))`——
  在三段斩等状态中放开技能 59 的指令（"前冲攻击后可以立即施放技能"的实现侧）。
  `setGhostSkillEnable` 定义在 `jg_swordman_common.nut:61`（状态白名单内 setSkillCommandEnable）。
- **真正的形态替换在共享层**（`jg_swordman_common.nut`，实测）：
  - `isSwordSaber(obj)`（:3-11）= **`sq_GetSkillLevel(obj, 123) > 0`**——本技能是全剑影形态的总开关；
  - `getAttackAni_Swordman`（:115）：普攻 index 0-3 → `CUSTOM_ANI_ATTACK_BLADESPIRIT1-4`(266-269)，
    上挑(46) 第 3 段 → `CUSTOM_ANI_UPPERSLASH_BLADESPIRIT`(272)；
  - `getDefaultAttackInfo_Swordman`（:135）：普攻命中 → `CUSTOM_ATTACK_INFO_ATTACK_BLADESPIRIT`(145)；
  - `getRestAni_Swordman`（:102）：待机 → `CUSTOM_ANI_GROWTYPE_GHOSTSWORD`(265)。
- **剑影形态技能变体注册**（load_state:150-153，实测）：
  `hardattack.nut` 同时注册状态 20（hardattack_swordman）与 105（**ghostslash** 鬼斩剑影形态）；
  `tripleslash.nut` 注册 22（tripleslash_swordman）与 138（**tripleslashbs** 三段斩剑影形态）。
- 后跳空中斩：`CUSTOM_ANI_JUMPATTACK_BLADESPIRIT`(271)（jumpattack_bladespirit.ani，7 帧）；
  前冲攻击：`CUSTOM_ANI_DASHATTACK_BLADESPIRIT`(270)。
- col0/col1 发动速度：无脚本消费点——引擎按技能等级加速剑影系技能（推断，面板性质）。

### 2.2 资产清单（全实测存在）

| 资产 | 文件（`character\swordman\` 下） | 说明 |
|---|---|---|
| 普攻 1-4 段 | `animation\attack_bladespirit1~4.ani`(+.als) | 10 帧级；atk=`attackinfo\attack_bladespirit.atk`（物理/无元素/push 30/lift 100/damage bonus 25） |
| 前冲攻击 | `animation\dashattack_bladespirit.ani` | |
| 后跳空中斩 | `animation\jumpattack_bladespirit.ani` | 7 帧 |
| 上挑变体 | `animation\upperslash_bladespirit.ani` | |
| 鬼斩变体 | hardattack.nut 状态 105（ghostslash） | |
| 三段斩变体 | tripleslash.nut 状态 138 + `tripleslash_bladespirit1~3.ani/.atk` | |
| 待机 | chr 槽 265 | |
| 特效 | `effect\animation\BladeSpirit\01a_00~03a_01 等`（attack_bladespirit*.als 挂接，[none effect add] 已支持） | img 000~006J 系列 + 1 张 Mage/ATChainLightning 借用（L14 跨目录常态） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SwordGhost4.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\SwordGhost\SwordGhost4.skl` | ✅（210 行） | 速度数值 |
| 被动注册 | passive_skill_swordman.nut case 123 + procSkill_Swordman:181 | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | 挂 appendage + 开 59 指令 |
| appendage | ap_swordghost4.nut | `…\pvf\sqr\character\jg_swordman\swordghost4\ap_swordghost4.nut` | ✅（28 行空壳） | 纯载体 |
| 形态开关/动画替换 | jg_swordman_common.nut:3/61/102/115/135 | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅ 实测 | isSwordSaber + 普攻/待机替换 |
| 变体状态注册 | load_state:150-153 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 105/138 |
| 变体动画/命中 | attack/dashattack/jumpattack/upperslash/tripleslash _bladespirit 系列 | `…\pvf\character\swordman\{animation,attackinfo}\` | ✅（ls 实测全存） | 形态资产 |

## 4. 资源需求（仅列增量 img；角色本体 sm_body 已入库）

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| BladeSpirit/000~004、005D、006J.img（7 张） | sprite_character_swordman_effect_bladespirit.NPK | 普攻剑气特效 | 可选（⛔ 级不做则不需要） | ❌ |
| 6_lightning_dodge.img | sprite_character_mage_at_chainlightning.NPK（跨树借用，L14） | 特效层闪电 | 可选 | ❌ |

**缺失 img = 8**（均可选）。

## 5~7. 实现/翻译/困难（⛔ 合并）

- **判定 ⛔（多重系统缺口叠加）**——本技能是"一套职业形态"而非一个被动：
  | 环节 | 缺口（在案） |
  |---|---|
  | 基本攻击/前冲/待机动画替换 | 普攻行为替换/技能变体分派门面（R5-B5 卡洛+暗天波动眼双实证——第 3 实证，且本技能是**形态级**替换，影响面最大） |
  | 鬼斩/三段斩剑影变体（状态 105/138） | 同上 + 各技能 SkillLogic 变体分支（可在各技能内 if 学得查询实现，但依赖跨技能查询门面） |
  | 前冲攻击后立即施放技能 | 技能取消体系（R1-A3 起，最大用户族之一） |
  | 后跳中空中斩 | 跳跃系统（R1-A2 在案）+ 后跳状态（169 已析）联动 |
  | col0/col1 发动速度 | 攻速/施放速度系统（R5-B5 四技共撞）——且 NumericType.Speed 移动端零消费（R2-A7） |
- 简化建议：**不实现**；剑影职业 demo 落地时按"整职业形态"统一立项（与 171 鬼啸/209 鬼夜等
  剑影系被动同批评估——它们都 gate 在 isSwordSaber 上）。
- 翻译工具：`.skl` 无子命令（全局已知）；bladespirit .ani/.als 全常规节 +
  [none effect add]（已支持）。无新增缺口。

## 8. 存疑与缺口上报

- 未考证：col1"[鬼步]剑术技能发动速度"的精确消费对象（鬼步系技能 126-128 族的加速，引擎侧）。
- 缺口归档（无新增，但**普攻行为替换档的影响面证据**值得记档）：本技能使"普攻行为替换"缺口
  从"单技能形态"升格为"职业形态级"（普攻 4 段+前冲+后跳斩+待机全套替换）——建议该缺口立项时
  以本技能为最重用例。另：7 张特效 img 含 1 张 Mage 树借用，NPK 推导按 .ani 内路径（L14）。
