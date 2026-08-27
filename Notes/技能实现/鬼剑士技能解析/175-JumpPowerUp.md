# 跃翔（JumpPowerUp）

> 技能ID 175 | 级别 B（预判 A 纠偏：实为跳跃增益 buff，无攻击逻辑） | 可实现性 ⛔（缺跳跃系统） | 分析日期 2026-08-22 | 批次 A2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 跃翔 | `skill\Swordman\JumpPowerUp.skl [name]` |
| 英文名 | JumpPowerUp（取 skl 文件名；[name2]="跳跃" 是中文别名，L1 惯例） | 同上 [name2] 实测 |
| 职业 | 鬼剑士全系共通（growtype 0-5；图标在 `Character/Common/SkillIcon.img` 共通表） | 同上 [skill fitness growtype] / [icon] |
| 学习等级 | 10 | 同上 [required level] |
| 最高等级 | 20（各觉醒段上限 10） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 4 = buff 类） | 同上 [type] / [skill class] |
| 指令 | ↑ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 25000 → 105000 ms（Lv1 → Lv20，**随等级增长**；PvP 15000→30000） | 同上 [cool time] / [pvp][cool time] |
| 施放时间 | 400 ms | 同上 [casting time] |
| MP | 13 → 44 | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 一句话效果 | 增加自身 20% 跳跃力，持续 10~105 秒 | 同上 [explain] + [level property] |

**level property（1 列，Lv1 → Lv20）**：`10000 → 105000`。模板 `持续时间 : <float1>秒`（向量 (-1,0,0.001)）→
col0 = 持续时间 ms（10 秒 → 105 秒）。**static data = `200`**：与 explain 的"20%"对应（推断 = 跳跃力 +20%，
200 = 20.0×10 的内部量纲，未考证）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无注册行**（grep `jumppowerup` / `, 175` 均无命中）。
Buff 类老技能：**全部逻辑（挂 buff/加跳跃力/计时移除）在客户端引擎内**，pvf 只有一份 .skl。

- 白名单 `sqr\character\swordman\` 全树 grep `jumppower`：**无命中**（无 nut、无 appendage 脚本；
  `appendage\` 目录仅 8 个 ap_*.nut，无一相关）。
- `passive_skill_swordman.nut`：无 jump 相关（grep 实测）。
- `passiveobject\character\swordman\`：无 jumppower 相关 .obj/.ani/.atk（ls 实测）。
- `character\swordman\animation\`：无 jumppowerup.ani（ls 实测）；`effect\animation\` 无 jumppowerup 目录
  （有 flowmindpowerup 但那是流心系，非本技能）。
- 兄弟职业 load_state（atswordman/demonicswordman/common）：grep 无命中。

**结论**：纯数据技能——引擎读到 skl 后给自身挂"跳跃力 +20%、持续 col0 ms"的内置 buff，
视觉为引擎通用 buff 光效（无专属资源文件，**未考证**具体表现）。

### 2.2 机制归纳

```
施放（400ms 蓄条）→ 自身挂 buff：
    跳跃力 +20%（static data 200，explain 固定 20% 不随等级变）
    持续 10000 → 105000 ms（col0）
    CD = 25000 → 105000 ms（有趣：CD 与持续时间同趋势，Lv20 时 CD=105s=时长，即"无缝续杯"临界）
到时 → buff 移除，跳跃力复原
```

无攻击、无判定体、无目标选择——纯自我增益。PvP 缩水版：持续 4→23 秒。

### 2.3 被动对象 / appendage

无（引擎内置，见 §2.1 四处实测）。

### 2.4 动画关键帧表

无任何专属动画（角色施放动作推测复用通用施法动作，未考证）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | JumpPowerUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\JumpPowerUp.skl` | ✅ 实测 | 全部技能数据（唯一文件） |
| lst 条目 | swordmanskill.lst 71-72 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 175 → 本 skl |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | — |
| 主 nut / ap nut / appendage | —（不存在） | `…\pvf\sqr\character\swordman\`（全树 grep jumppower 无命中） | ⛔ 缺失 | 引擎内置 |
| 角色 .ani / .atk | —（不存在） | `…\pvf\character\swordman\animation\` / `attackinfo\` | ⛔ 缺失 | 无专属动画/命中 |
| PO / .als / 特效 | —（不存在） | `…\pvf\passiveobject\character\swordman\` 等 | ⛔ 缺失 | — |
| 图标 | SkillIcon.img #6/#7 | `Character/Common/SkillIcon.img`（共通表，非 swordman 专属） | ✅ 实测（路径） | 技能图标（我方无 UI 消费，不做） |
| 装备层 | — | `…\pvf\equipment\character\swordman\avatar\` | —（无动画故无图层） | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无） | — | — | — | — |

**零资源需求**：pvf 侧没有任何专属 img/ani/als/atk/obj——是全 241 技能里文件链最短的形态（仅 1 个 .skl）。

## 5. 实现方案草案

⛔ 级——按手册可免；记**前提与届时形态**：

- **前提（系统级）**：跳跃系统落地（跳跃输入 + 玩家 z 物理 + 跳跃力数值消费方——见 017 §8）。
  buff 机制本身已有（BuffDefinition/LSBuffComponent），缺的只是"跳跃力"这个数值没有消费方。
- **届时形态（零障碍）**：
  - `JumpPowerUpSkill : SkillLogic`：`CooldownMs = 25000`（Lv1 档）、`TotalTimeMs = 400`（蓄条时长当施放动作）；
    `OnCast` → `ctx.AddBuffToSelf(BuffIds.JumpPowerUp)`。
  - `JumpPowerUpBuff : BuffDefinition`：`TotalTimeMs = 10000`（Lv1 档）、`AddActions = { JumpPowerOn }`、
    `RemoveActions = { JumpPowerOff }`（同 StunBuff 的 ForbidMoveOn/Off 对称写法，把 NumericType 加减换成跳跃力档位）。
  - 数值表：时长 DNF 10~105s（demo 取 30s 中档）；跳跃力 +20%（static 200 原值）。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `JumpPowerUp.skl` | `.skl` 尚无子命令（1 列 level info + static 200） | 本技能手抄 2 个数即可；随批量化加 `skl` 子命令（同 064 记档） |

（无 .ani/.als/.atk/.obj——翻译环节仅此 1 条。）

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 跳跃力 +20% | **缺失：跳跃系统**（§6.3 清单外新缺口，017 §8 同源上报）——我方无跳跃，"跳跃力"无数值消费方 | 跳跃系统落地前不实现（无意义）；落地后按 §5 零障碍接入 |
| 蓄条 400ms | 无 cast 条（延后） | 瞬发 |
| CD 随等级增长（25s→105s） | 无等级系统（延后） | 固定档 |
| buff 光效 | 无专属资源（引擎通用光效，未考证） | 跳过或复用现有染色/overlay |
| MP 13-44 | 无 MP 系统（延后） | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. static data `200` 的精确量纲（与"20%"的换算关系）。
2. 施放动作动画（推测复用通用施法，无文件可查）。
3. buff 视觉表现（引擎通用光效，pvf 无资源文件）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **跳跃系统**（与 017 合并上报）：本技能是"跳跃系统的最便宜验证件"——建议跳跃系统立项时把它当
   第一个数值消费方一起做（BuffDefinition 全现成，只需跳跃力数值+开关 Action）。

**翻译工具缺口**：`.skl` 子命令（1 条）。

**给下轮的经验**：技能图标在 `Character/Common/SkillIcon.img`（共通表）的技能 = 跨职业技能（本技能全系可用）；
icon 路径可作职业归属的辅助判据（064/058/065 的专属图标都在 `Character/Swordman/Effect/SkillIcon.img`）。
