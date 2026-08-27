# 满月斩（MoonlightSlashFull）

> 技能ID 80 | 级别 C（确认为**被动强化**——[type] [passive]，pre-required 月光斩 77；非主动技） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 满月斩 | `MoonlightSlashFull.skl [name]` |
| 英文名 | MoonlightSlashFull（skl 文件名；[name2]=`Fullmoon Moonlight Slash`） | skl |
| 职业 | 鬼泣（[skill fitness growtype] = 2） | skl |
| 学习等级 | 20（前置：月光斩 77 Lv1） | skl `[required level]`/`[pre required skill]` |
| 最高等级 | 1（[growtype maximum level] gt2=1） | skl |
| 类型 | [passive]（skill class 3）+ [weapon effect type] [magical] | skl `[type]` |
| 一句话效果 | [月光斩]的单手上斩后追加一段**暗属性**双手上斩 | skl `[explain]` |

**static data**：空——无任何数值（追加段的伤害全在 MoonlightSlashFull.atk 的
[weapon damage apply]=1 武器伤害结算里，随月光斩等级成长）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

本技能无自有注册/nut（load_state、passive_skill_swordman.nut 均无 80，实测）。效果挂在宿主技
**77 月光斩**（引擎内置 F3，完整走读见 `077-MoonlightSlash.md`）：
- `swordman_header.nut:235`：`CUSTOM_ANI_MOONLIGHTSLASHFULL <- 65`（.chr etc 槽 65）；
- `swordman.chr:1038`：槽 65 = `Animation/MoonlightSlashFull.ani`；
- `swordman.chr:1350`：atk 对象表配对 `AttackInfo/MoonlightSlashFull.atk`；
- 旁证：`ghostsoulrelease/ap_ghostsoulrelease.nut` 实测有 `state == 42（月光斩中）需技能 80 已学`
  的门槛——满月斩形态是引擎在月光斩状态机里按技能 80 分支（推断：两段月光后追播槽 65 动画
  + MoonlightSlashFull.atk 判定）。

### 2.2 动画与命中（追加段资产全实测）

| 文件 | 帧数 | 关键参数 | 备注 |
|---|---|---|---|
| `animation\MoonlightSlashFull.ani` | 10 | 常规 FRAME/ATTACK BOX | 仅 `sm_body%04d.img`（已入库 L16） |
| `attackinfo\moonlightslashfull.atk` | — | [magic]/[dark element]/[damage reaction] down/[lift up] 300/[push aside] 100/[weapon damage apply] 1 | 暗属性魔法伤害 + 击倒挑空 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MoonlightSlashFull.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MoonlightSlashFull.skl` | ✅（53 行） | 开关型被动 |
| 注册行/主 nut | —（挂宿主 77） | `…\pvf\sqr\character\`（grep 实测无） | ⛔ 无独立脚本 | 见 077 |
| .chr 条目 | 槽 65（etc motion）+ atk 表 | `…\pvf\character\swordman\swordman.chr:1038/1350` | ✅ 实测 | 追加段动画/命中 |
| 角色 .ani | MoonlightSlashFull.ani | `…\pvf\character\swordman\animation\moonlightslashfull.ani` | ✅（10 帧） | 双手上斩 |
| 角色 .atk | moonlightslashfull.atk | `…\pvf\character\swordman\attackinfo\moonlightslashfull.atk` | ✅ | 暗属性击倒 |
| 消费旁证 | ap_ghostsoulrelease.nut state 42 门槛 | `…\pvf\sqr\character\swordman\ghostsoulrelease\ap_ghostsoulrelease.nut` | ✅ 实测 | 技能 80 已学判定实证 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 追加段动画图集 | 必需（共享） | ✅ sm_body0000.img.bytes |

**缺失 img = 0**（本批唯一零缺失的技能——追加段纯角色动画，无特效层）。

## 5. 实现方案草案（"基础技 + 强化增量"范式，本批最可实现者）

- **内容件**：不新建 SkillLogic——**扩 077 草案的 MoonlightSlashSkill**：习得 80 时
  `TotalTimeMs` 追加满月段，OnUpdate 在月光末段后 `SetSubState` 进满月段：
  `ctx.PlayAnim(新 AnimId)` + `ctx.ClearHitTargets()`（段间重置，L19）+ 帧到位后
  `ctx.SetAttackHitbox(offset, halfExtents)`，命中走满月段 HitReaction。
- **HitReaction**：Damage=武器伤害折算（demo 固定值，对齐 077 建议表 +20% 档）、
  `LaunchY≈300、KnockbackX≈100`（atk 原值直用 01§4 换算）、击倒=Down 受击动画链（已有）。
- **暗属性**：[dark element] 降级为无属性（元素属性系统缺失，在案）——伤害照常结算。
- **注册点**：`AnimConfigRegistry.cs` 新增 `MoonlightSlashFull = 178`（AnimIds 178 起顺延，L18）；
  json 走 ani 子命令（sm_body 已在库）。不占 SkillIds/BuffIds。
- **关键数值**：追加段动画 10 帧（帧时长另测，0.1s/帧惯例 ≈ 1s）；无 CD/MP 增量（并入月光斩）。

## 6. 翻译工具适配

| 文件 | 不支持项 | 建议 |
|---|---|---|
| MoonlightSlashFull.skl | `.skl` 无子命令 | 单级开关无数据，手抄为零负担 |
| MoonlightSlashFull.ani | 全常规节 | 现有 ani 子命令全覆盖 |
| moonlightslashfull.atk | `.atk` 无子命令（全局已知）；[dark element] 无字段 | 手抄 3 值即可；元素字段归元素系统立项时统一 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 暗属性伤害 | 元素属性系统缺失（在案） | 降级无属性，数值不变 |
| 武器伤害随月光斩等级成长 | 等级缩放延后（在案） | demo 固定值 |
| 追加段与月光的接续取消窗口 | 技能取消体系（在案） | 连段直出（SubState 顺序推进，077 同构） |

## 8. 存疑与缺口上报

- 未考证：追加段的触发细节（月光两段后自动接 vs 按键接续）——引擎分支不可见；
  077 记载月光斩本体为"子状态链自动推进"，demo 按自动接续处理。
- 缺口归档：元素属性系统（在案）。无新增缺口。
