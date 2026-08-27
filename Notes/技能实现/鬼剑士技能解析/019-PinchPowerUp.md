# 血气唤醒（PinchPowerUp）

> 技能ID 19 | 级别 C | 可实现性 🔶（面板可表达 / 消费端卡死 + 条件触发需简化） | 分析日期 2026-08-22 | 批次 C2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 血气唤醒 | `PinchPowerUp.skl [name]` |
| 英文名 | PinchPowerUp（取 skl 文件名；[name2] 实测 `血之十字`——中文别名，L1 惯例） | skl [name2] |
| 职业 | 狂战士 + 剑影（[skill fitness growtype] = 3；growtype 上限 `0 0 0 20 0 20`——仅 3/5 可学，上限 20） | skl |
| 学习等级 | 15 | skl [required level] |
| 最高等级 | 50（受 growtype 约束实际 20） | skl [maximum level] / [growtype maximum level] |
| 类型 | passive（skill class 2） | skl [type] / [skill class] |
| 指令 / CD / MP | 无（纯被动） | skl |
| 强化版 | feature skill index 139（PinchPowerUpEx，E3 批次） | skl [feature skill index] |
| 一句话效果 | 自身 HP < 40% 时增加力量/回避率/攻击速度/移动速度，HP 越低增加越大 | skl [explain] |

**static data（dungeon）= `40`**：触发阈值 40%（explain 互证，确定）。

**level info 8 列（dungeon Lv1 / Lv20）**：
- col0/col1 = 力量 min~max：`36 / 92` → Lv20 `1707 / 4266`（×1.0 直读）
- col2/col3 = 回避率 min~max：`4 / 30` → `106 / 1064`（×0.1 → 0.4%~3.0% → 10.6%~106.4%）
- col4/col5 = 攻击速度 min~max：`8 / 20` → `155 / 388`（×0.1 → 0.8%~2.0% → 15.5%~38.8%）
- col6/col7 = 移动速度 min~max：与攻速同值同涨（`8 / 20` → `155 / 388`）

level property 模板（实测）：`增加力量 <int>~<int> / 回避率 <float1>~<float1>%% /
攻击速度和移动速度 <float1>~<float1>%%`；向量 6 组系数 1.0/1.0/0.1/0.1/0.1/0.1——
列语义与模板对位自洽（min=HP 刚破 40% 档，max=HP 趋 0 档，**推断**为线性插值，引擎内实现）。
pvp 列独立且大幅缩水（力量 7~532）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无任何脚本**（实测）：
- `swordman_load_state.nut` grep `pinchpower`：无命中（被动无状态）；
- `passive_skill_swordman.nut` 294 行通读：无 case 19；
- `sqr\character\swordman\` 白名单 grep `pinch|Pinch`：**零命中**——
  连常量定义都没有（对照：血气旺盛 63 至少进了 header 常量表）。
  触发监听（HP 变化）、min~max 插值、四路属性写入**全部在客户端引擎内**。

### 2.2 机制归纳

```
常驻监听自身 HP%：
  HP ≥ 40%      → 无增益（buff 关）
  HP < 40%      → 增益开，四路数值 = colMin ~ colMax 按 HP 位置线性插值（推断）
                  HP↓ → 力量/回避/攻速/移速↑（狂战残血爆发定位）
```

- 属性写入方式：引擎按面板属性（力量/回避率/攻速/移速四键）注入——与 passive_skill 注册表
  里 78/119 的 `sq_AddChangeStatus` 四路同构，只是驱动源是 HP 阈值而非习得等级。
- 视觉：狂战残血红光/血气特效为引擎通用表现，白名单内无 pinchpower 命名资源（实测）。

### 2.3 被动对象 / appendage

无（§2.1 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | PinchPowerUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\PinchPowerUp.skl` | ✅ | 8 列 × 50 级（dungeon+pvp 双表）+ static 40 |
| 注册行 / 被动注册 | — | `…\sqr\character\swordman_load_state.nut` / `passive_skill_swordman.nut` | ⛔ 均无（引擎内置） | — |
| 主 nut / appendage | — | `…\sqr\character\swordman\`（grep pinch 零命中） | ⛔ 不存在 | 行为在引擎 |
| .ani / .atk / PO | — | `…\character\swordman\animation\` 等 | ⛔ 无专属 | — |
| 图标 | SkillIcon.img #44/45 | `Character/Swordman/Effect/SkillIcon.img` | ✅（路径） | 不做 UI |
| 关联强化 | PinchPowerUpEx.skl（139） | `…\skill\Swordman\PinchPowerUpEx.skl` | ✅（存在） | E3 批次 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无专属资源） | — | — | — | — |

必需级缺失 img = **0**。

## 5. 实现方案草案（§5/§6/§7 合并精简）

**判定分半**：

| 半边 | 结论 | 依据 |
|---|---|---|
| 面板可表达 | ✅ 可（力量需新增键；其余三路键不存在，见下） | BuffDefinition 数值挂摘 + NumericType 机制在库 |
| 消费端 | ⛔ 全卡死 | 力量→伤害公式无（176 已证）；**攻速**撞攻速系统缺口（R5-B5 四技共撞）；**移速**撞 Speed 零消费（R2-A7 移动硬编码 6 单位/s）；**回避率**无命中/闪避系统 |

**届时形态**：
- 条件常驻 Buff：`PinchPowerBuff : BuffDefinition`，`TotalTimeMs=0`（永久）+ `TickTimeMs=500`——
  Tick 内读 HP% 重算四路数值（挂/摘用 AddActions/RemoveActions 对称减回；Tick 重设采用
  "先减上次增量再加新值"模式）。需要 **LSActionContext 加 `GetOwnerMaxHp/GetOwnerHpPct` 门面**
  （现有 GetOwnerHp 有、MaxHp 无——实测 LSActionContext 仅 GetOwnerHp）。
- **简化档（推荐 demo 首版）**：不做 HP 插值——**分两档**：HP<40% 挂 max 值 Buff、HP≥40% 摘除
  （HP 阈值监听点可借受击管线钩子缺口 R3-A15 的消费方一并立项，或 Tick 500ms 轮询 LSAction 侧自查）。
- 数值（demo 建议取 Lv10 档）：力量 +150 / 回避 +5% / 攻速 +5% / 移速 +5%（DNF 原值列见 §1；
  NumericType 需新增 Strength / EvadeRate / AttackSpeed / MoveSpeedPct 键——Speed(1000) 五层已有
  但移动端不消费，攻速/回避全新）。
- 注册点：BuffIds 从 18 顺延（本技能草案记 22 号段，撞号无妨）；无 SkillId/AnimId/img 需求。

## 6~7. 困难与简化（并入 §5）

| DNF 原版行为 | 缺口档位 | 简化 |
|---|---|---|
| HP 位置 min~max 线性插值 | 无 HP 变化事件钩子（受击管线钩子缺口 R3-A15 同族） | 两档开关（<40% = max 值） |
| 攻速/移速加成 | 攻速系统缺失（R5-B5）/ Speed 零消费（R2-A7） | 面板数值可见即可，行为不变 |
| 回避率 | 无命中/闪避判定系统（新记，见 §8） | 同上 |
| 力量→伤害 | 属性伤害消费链（5 实证在档） | 同上 |
| 残血视觉 | 无资源无来源 | 跳过或 tint 近似 |

## 8. 存疑与缺口上报

**未考证项**
1. min~max 的插值曲线（线性为推断；Lv20 回避 max 106% >100% 的封顶规则未考证）。
2. 触发是"跌破即挂、回复即摘"还是滞回（DNF 手感常识为即时，未实证）。
3. static data 仅 1 值（40）——若引擎还读了未列出的参数无从考证。

**新系统级缺口**
1. **回避率/命中判定系统**：本 pvf 的 CHANGE_STATUS_TYPE 系（回避/命中/暴击/暴伤）是一整族
   面板属性，我方全部无键无消费——本技能首次撞"回避率"。建议与"属性伤害公式"合并记为
   **属性消费链总缺口**的子项清单（力量/智力已有先例上报，回避/命中/暴击/暴伤为新增四键）。
2. **LSActionContext 缺 GetOwnerMaxHp**：Buff 侧 HP 百分比条件技（本技能）无法自查阈值——
   一行门面的小改，随被动系统最小版一并提。

**翻译工具缺口**：`.skl` 子命令（全局已知项，本技能双表 8 列×50 级×2 手抄量大，是 skl 子命令
收益较大的数据点之一）。
