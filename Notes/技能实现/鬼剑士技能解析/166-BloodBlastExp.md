# 强化-怒气爆发（BloodBlastExp）

> 技能ID 166 | 级别 E（TP 强化技） | 可实现性 ✅（=基础版 024 ✅；段数 4→5/8→10 落为 Area Tick 段数调档、攻击力倍率常数——零新机制；TP 视觉另有 Ex PO 族 9 张可选 img） | 分析日期 2026-08-22 | 批次 E6

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 怒气爆发 | `BloodBlastExp.skl [name]` |
| 英文名 | BloodBlastExp（skl 文件名；[name2]=`Raising Fury Upgrade`） | 同上 |
| 职业 | 狂战士（[skill fitness growtype]=3） | 同上 |
| 学习等级 | 65（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；[growtype maximum level] `0 0 0 5 0 0`——狂战至多 5 级） | 同上 |
| TP 消耗 | **`4 2`（双值——本批唯一双值形态，语义未考证：疑 dungeon 4/pvp 2 或基础价/增量价）** | 同上 [special purchase cost] |
| 前置 | 技能 24（怒气爆发）Lv1 | 同上 [pre required skill] |
| 类型 | passive（[feature skill type] 1；skill class 2） | 同上 |
| 一句话效果 | TP1：外圈 4 段→5 段、中心 8 段→10 段（一次性）；每级攻击力 +10%（TP2 起只加攻击力） | 同上 [explain ex]（"从2Lv开始只增加攻击力"） |
| 基础技 | 24 怒气爆发（`024-BloodBlast.md` ✅）；基础 skl [feature skill index]=166 双向链接（实测） | 两 skl 实测 |

## 2. 强化增量（对照 024-BloodBlast.md）

### 2.1 数据侧（E 类通用解码法 + 一处 static 覆写）

- [level info]（71 行 ×3 列）与基础**逐字节相同**；[level property] 2 向量与基础逐值相同。
- **[static data] 覆写变体（本批 4 例之一）**：
  - dungeon：基础 `4 450 450 700 1` → Ex `5 450 450 700`——**static[0] 段数 4→5**（外圈段 +1，TP 习得即生效=explain"TP1 加段、TP2 起只加攻击力"的数据形态）；**尾槽 1 丢失**（基础 static[4]=1 疑"中心双倍段标志"，Ex 表缺值——explain 明示中心 10 段=5×2 双倍仍开，疑引擎默认，存疑）。
  - pvp：基础无 pvp static 节 → Ex 新增 `4 450 450 700 0`（pvp 段数保持 4——决斗场不加段，通例一致）。
- [special level up]（dungeon，2 行）：`-1 0 % 10`（col0 外圈每段物攻 +10%/级）、`-1 2 % 10`（col2 中心冲击波攻击力 +10%/级）。pvp 无该节。

### 2.2 level property 模板行直读——回填 024 存疑

Ex [level property] 模板（`物理攻击力 : <int>` / `冲击波攻击力 : <int>` ↔ 向量 -2 0 / -2 2）：

| 列 | 基础档原判定（024 §1"未考证推断"） | **本档修正（模板直读）** |
|---|---|---|
| col0（645→5259） | "每段攻击力 ‰（推断）" | **外圈每段物理攻击力**（模板第 1 行）✓ |
| col1（500→143 递减） | "多段间隔（推断）" | 不在 Ex 模板 → 间隔推断维持（面板不显示） |
| col2（861→7014） | "中心段攻击力或总量（推断）" | **中心（冲击波）攻击力**（模板第 2 行）✓ |

### 2.3 TP 态 PO 族切换（引擎行为，数据实测）

基础 024 记档的 20102/20103/20104 = **BlastBloodEx/SubEx/PreSubEx.obj**（passiveobject.lst:11334-11338 实测）为 TP 习得后引擎换用的判定体族（推断，高置信：obj 命名+atk 配套+段数 5 的视觉需求）。与基础三件 diff 实测：

| 项 | 基础 | Ex 版 | 说明 |
|---|---|---|---|
| 主 PO basic motion | BlastBlood1.ani（15 帧，攻击盒 F10-F13） | **BlastBlood1Ex.ani（15 帧，攻击盒 F11-F14）** | 判定窗整体后移 1 帧；etc motion 换 BlastBloodEx/sub、sub_dodge1/2、BlastBloodExEnd（Floor3/BlastBlood3 移除） |
| 主 PO atk | BlastBlood.atk | BlastBloodEx.atk | **逐字节相同**（实测 diff） |
| Sub PO | 仅 atk 名换 SubEx | SubEx.atk | 仅音效名差异（FIRE_PILLAR_HIT→FLAMES_HIT） |
| PreSub PO | layer [normal] + PreSub.atk | **layer [bottom] + PreSubEx.atk：lift up 400→100** | **TP 态先手浮空高度 400→100**（唯一行为差异，音效同换） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodBlastExp.skl（255 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodBlastExp.skl` | ✅ 全节实测 | 镜像表 + static 覆写 + TP 增量 |
| 基础 skl | BloodBlast.skl（[feature skill index] 166） | 同目录 | ✅ 实测比对 | 双向链接 |
| 脚本 | —（引擎内置，基础档无 nut） | `…\sqr\character\swordman\` | ⛔（024 已证无 nut） | TP 消费在引擎 |
| PO 定义 | blastbloodex.obj / blastbloodsubex.obj / blastbloodpresubex.obj | `…\pvf\passiveobject\character\swordman\` | ✅ diff 实测 | TP 态判定体三件套（§2.3） |
| PO .atk | blastbloodex.atk / blastbloodsubex.atk / blastbloodpresubex.atk | `…\passiveobject\character\swordman\attackinfo\` | ✅ diff 实测 | 同上 |
| PO .ani | BlastBlood1Ex.ani + BlastBloodExEnd.ani + animation\blastbloodex\（19 个）+ blastbloodexend.ani.als | `…\passiveobject\character\swordman\animation\` | ✅ 实测（ls+帧表） | TP 视觉族（ex_blood1-8 血柱八层等） |
| 基础技文档 | 024-BloodBlast.md | 本目录 | ✅ | 继承源（双同心 Area 方案） |

## 4. 资源需求

TP 视觉族新 img（`Character/Swordman/Effect/BlastBloodEx/`，全推导 `sprite_character_swordman_effect_blastbloodex.NPK`，一次提取全覆盖；血柱 ex_blood1-8 引 blood_red_front/back+small_blood，收尾 end_blood，地面 blood_floor 系，sub/sub_dodge）：

| img | 用途 | 必要性 | 已入库? |
|---|---|---|---|
| blood_red_front.img / blood_red_back.img / small_blood.img | Ex 血柱多层 | **可选**（仅 TP 习得后视觉；demo 不还原 TP 视觉则零需求） | ❌ |
| end_blood.img / sub.img / sub_dodge.img / blood_floor.img / blood_floor_front.img / blood_floor_back.img | 收尾/地面/本体层 | 可选（同上） | ❌ |
| （复用）Effect/BlastBlood/blood-d1.img | BlastBlood1Ex 第 1 帧借用 | 已在基础档 024 §4 清单 | ❌ |

缺失 img：**必需 0 / 可选 9**（TP 视觉增量自身）。

## 5. 实现方案草案（增量落地）

零新内容件——024 §5 的 BloodBlastPreArea/BloodBlastArea/BloodBlastCoreArea 三区方案原样承载：

| 参数 | 基础版（024 草案） | TP 并入（建议 TP5 定值） |
|---|---|---|
| 外圈段数 | Tick 450ms × TotalTime 1800 = 4 段 | **5 段**（TotalTimeMs 2250，Tick 不变）——TP≥1 即 5 段 |
| 中心段数 | 内外双区叠加 = 8 段 | **10 段**（内区同步 5 Tick，双倍逻辑自然成立） |
| 外圈每段伤害 | 80 | ×(1+0.1×TP)，TP5=120 |
| 中心每段伤害 | 90 | ×(1+0.1×TP)，TP5=135 |
| 前段浮空 | PreArea LaunchY=400 | **可选采纳 Ex：TP 态 100**（PreSubEx.atk 实测；不采纳则维持 400） |
| 视觉 | ViewAnimId=BlastBlood1 | TP 态可换 BlastBloodEx 系（需 §4 可选 img） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BloodBlastExp.skl | `.skl` 无子命令（镜像表 + static 覆写 + 2 行增量 + **[special purchase cost] 双值**） | 手抄可行；skl 子命令对 cost 节按"值数组"建模（兼容双值） |
| BlastBloodEx 三件 .obj/.atk | `.obj`/`.atk` 无子命令（既有） | 手工映射已给（§2.3 diff 表即全部差异） |
| blastbloodex 系 .ani | 常规节（含 blastbloodexend.ani.als 常规 [use animation]/[add]） | 现有 ani/als 子命令全覆盖 |

翻译缺口计 1 条（.skl 类型，含 cost 双值新形态）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 外圈 5 段/中心 10 段 | 无缺口（Area Tick 段数=时长/间隔调档） | 直译 |
| 攻击力 +10%/级 | 无缺口 | 常数倍率 |
| TP 态先手浮空 400→100 | 无缺口（数值） | 可选采纳 |
| TP 视觉族 9 img | 资源量问题（可选级） | demo 沿用基���血柱视觉 |
| pvp 不加段不强化 | 无 PVP 分流 | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. [special purchase cost] `4 2` 双值语义（本批唯一；其余 9 技均单值 2）。
2. Ex dungeon static 尾槽缺失（基础 static[4]=1 的"中心双倍标志"若被引擎按位读，Ex 表缺值是否等效 0——但 explain 明示中心 10 段双倍仍开，矛盾，疑引擎对缺省槽另有默认）。
3. 引擎 TP 习得时切换 Ex PO 族的确切条件（任何 TP 等级 vs TP≥1——按 explain 应为 TP≥1，推断）。

**回填**：024 §1 三列语义修正（col0=外圈每段物攻、col2=中心攻击力，模板直读；§2.2 表）——024 §5 demo 数值不受影响。
**新缺口**：无。翻译工具：`.skl` 子命令（cost 双值形态为设计输入）。
