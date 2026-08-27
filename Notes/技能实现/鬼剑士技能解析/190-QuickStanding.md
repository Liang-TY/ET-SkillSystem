# 受身蹲伏（QuickStanding）

> 技能ID 190 | 级别 B | 可实现性 ⛔（缺倒地状态输入窗口 + 无敌帧两个系统级能力；数值/动画资源零缺口） | 分析日期 2026-08-22 | 批次 B1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 受身蹲伏 | `skill\Swordman\QuickStanding.skl` [name] |
| 英文名 | QuickStanding（取 skl 文件名；[name2]="Quick Standing"） | 同上 [name2] 实测 |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5；全职业通用系统技） | 同上 |
| 学习等级 | 1 | 同上 [required level] |
| 最高等级 | 20 | 同上 [maximum level] |
| 类型 | active（**skill class 4 = 系统动作类**） | 同上 [type]/[skill class] |
| 指令 | （倒地状态下）C | 同上 [command] / [command key explain] |
| CD | 5000 ms（pvp 20000） | 同上 [cool time] |
| MP | 1 | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `300 0`（dungeon）/ `500 0`（pvp）——static[0] = 蹲伏**最短**无敌时间 ms（level property 模板向量 `0 0 0.001` 直读，源 0=static 槽，L21 印证） | 同上 [static data] |
| level info | 2 列 20 级：`4000→31500`、`300→933` | 同上 [level info] |
| 一句话效果 | 倒地状态下迅速起身并蹲伏，蹲伏期间无敌（最短 0.3s / 最长随等级 4→31.5s），起身时霸体 | 同上 [explain] + [level property] 模板 |

**level property 模板解码（3 行全明，L21 向量法）**：
- 蹲伏最短无敌时间 = static[0] = **300ms**（pvp 500）
- 蹲伏最长无敌时间 = col0 × 0.001 = **4.0s → 31.5s**（Lv1→20）
- 起身时霸体时间 = col1 × 0.001 = **0.3s → 0.933s**

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无任何注册行**——`swordman_load_state.nut`、`common_load_state.nut`、`atswordman_load_state.nut`、`demonicswordman_load_state.nut`、`JG_SwordMan\` 白名单内 grep `quickstanding|quick_stand|squat` 全空（实测）。受身蹲伏是引擎内置的**全职业通用状态机**（倒地恢复分支），pvf 只给数据文件。

### 2.2 引擎内置行为重建（.skl 数据 + 倒地动画链推断）

```
被击倒落地（Down 状态，down.ani F2/F3 = 10000ms 悬停帧，等待起身决策——L23 事件推进）：
  按 C：
    立即起身 → 蹲伏姿态（蹲伏动画未见独立 .ani，见 §8 未考证）
    蹲伏期间无敌：按住 C 保持蹲伏，最短 300ms 必蹲；最长 4~31.5s（等级）
    松开 C / 到最长时限 → 起身（起身时霸体 0.3~0.933s，期间不可被打断但可被命中？——霸体=可被打不硬直）
  未按 C：down.ani 事件帧超时 → 自然起身（overturn.ani，1 帧 10000ms 悬停）
```

### 2.3 动画关键帧表（倒地-起身链，全部实测）

| 动画 | 帧数 | 总时长 | SET FLAG | 受击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\down.ani`（.chr [down motion]） | 6 | 80+80+10000+10000+70+430 | 无 | 6/6 帧 | 击倒落地：F0/F1 倒地（80ms×2）→ **F2/F3 躺地悬停（10000ms×2，事件推进）** → F4 起身（70ms）→ F5 站立（430ms） |
| `character\swordman\animation\overturn.ani`（.chr [overturn motion]） | 1 | 10000ms（悬停） | 无 | 1/1 帧 | 翻身/起身帧：**[IMAGE ROTATE] 3.140022（≈180°，躺姿=站立精灵旋转）** + 受击盒 `-14 -5 2 30 10 84`/`-28 -4 26 17 8 59`（躺地扁盒） |
| `character\swordman\animation\sit.ani`（.chr [sit motion]） | 1 | 150ms | 无 | 1/1 帧 | 坐姿（非蹲伏，见 §8） |
| `damage1.ani` / `damage2.ani`（.chr [damage motion 1/2]） | 1 / — | 10000ms 悬停 | 无 | — | 受击僵直事件帧（现库 SwordmanHurt 同构） |

**蹲伏姿态专属动画不存在**（animation 目录 ls 按 squat/crouch/wake/quickstand 全查无命中，实测）——蹲伏姿势疑为引擎内置姿态（或复用 down.ani 躺地帧），未考证。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | QuickStanding.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\QuickStanding.skl` | ✅ 实测（125 行） | 数值（无敌/霸体时长） |
| 注册行 | — | `…\pvf\sqr\character\swordman_load_state.nut` 等四处 | ⛔ 全空（引擎内置） | — |
| 主 nut / ap nut | — | `…\pvf\sqr\character\swordman\`（递归 grep） | ⛔ 无 | — |
| .chr 条目 | [down motion]/[overturn motion]/[sit motion] | `…\pvf\character\swordman\swordman.chr` 883-893 行 | ✅ 实测 | Down.ani / Overturn.ani / Sit.ani |
| 角色 .ani | down.ani / overturn.ani / sit.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.3 帧表 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（grep 无） | ⛔ 无 | 系统技无攻击 |
| .als / PO / 装备层 | — | — | ⛔ 无专属（换装层为通用 down/overturn 各层） | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 98-103 down / 97 overturn / 157 sit） | sprite_character_swordman_equipment_avatar_skin.NPK | 倒地/起身动画 | 必需（若立项） | ✅ 已在库 |

缺失 img：**0**。蹲伏姿态动画本身未定位（§8），不产生 img 需求。

## 5. 实现方案草案

**⛔ 暂缓**——核心语义依赖两条缺失系统能力：

1. **倒地状态输入窗口**（致命）：我们倒地 = `LSCombatComponent.HitstunTimer` 纯计时（`LSCombatComponentSystem.LSUpdate` 实测：计满自动回默认动画，**无输入路由**）；且 `SkillCastHelper.TryCast` 在 `HitstunTimer > 0` 时**拒绝一切施放**——受身要求"恰在倒地计时内可施放且打断该计时"，与现门禁方向相反。落地动画链（DownAnimId）只是表现钩子，不是状态机。
2. **无敌帧**：蹲伏 300ms~31.5s 无敌无处安放（无受击豁免通道；169 §8 的"空受击盒帧"修正可作低成本载体，但蹲伏动画缺源）。

**若未来立项**（跳跃/倒地状态机一并做），映射现成：`QuickStandingSkill : SkillLogic`——门禁改"仅倒地状态可放"（倒地状态标记进 LSCombatComponent 或状态机实体）；`TotalTimeMs` 由按键保持驱动（蹲伏=按住 C，无按住检测则固定蹲 1s demo 值）；蹲伏期无敌走空受击盒帧；起身段 0.3s 霸体（霸体帧延后档）；CD 5000 直用。**无新增内容件复杂度，纯缺状态机与无敌通道**。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `QuickStanding.skl` | `.skl` 无子命令 | 手抄 3 值即可（模板全明）；`skl` 子命令同前议 |
| `down.ani` | **超长 DELAY 悬停帧**（F2/F3=10000ms，L23 已记档） | 翻译需钳制或消费侧约定"末段超长 delay=待事件"；直译会永不播完 |
| `overturn.ani` | **[IMAGE ROTATE] 3.14**（躺姿=精灵旋转 180°，语义承重） | 工具整节跳过（README 规则表外）——跳过后起身动画以未旋转的站立精灵渲染，躺姿错误。建议：IMAGE ROTATE 进翻译（AnimFrameData 加 rotate 字段）或消费侧对该帧手工配替代贴图 |
| `[DAMAGE TYPE] SUPERARMOR`（霸体，引擎按 col1 时长赋予） | DAMAGE TYPE 整节跳过（README 明示） | 霸体帧延后档既有记档，非新缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 倒地中按 C 触发 | **缺失档：倒地状态输入窗口**（受击-施法互斥门禁反向需求，R1-A4"受击-施法互斥"缺口的姊妹面） | 不实现；等倒地状态机立项（建议与跳跃系统合并评估） |
| 蹲伏无敌 0.3~31.5s | **缺失档：无敌帧**（R1-A5 已记）；169 §8 空受击盒帧修正可作载体但蹲伏动画无源 | 同上 |
| 按住 C 保持蹲伏 | 按住检测（延后档，无按住输入） | 若立项：固定蹲伏时长（如 1s） |
| 起身霸体 0.3~0.933s | 霸体帧（延后档） | 忽略（落地起身即解控） |
| down/overturn 悬停帧 10000ms | L23 超长 DELAY（翻译缺口已记档） | 钳制 |

## 8. 存疑与缺口上报

**未考证项**
1. **蹲伏姿态动画来源**（无专属 .ani；疑引擎内置姿态或复用 down.ani 躺地帧——纯数据无法判定）。
2. 蹲伏期间是否可被霸体类攻击命中（explain 只说无敌，霸体攻击交互未考证）。
3. pvp 最短无敌 500ms 的对抗性设计（pvp static 500）不适用我们。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **倒地状态输入窗口**：受身蹲伏是"状态前置型技能"（R1-A1 已记该类缺口）的最深案例——前置状态不是"前冲/跳跃"这种主动技能态，而是**受击系统内部子状态**。落地涉及的组件：LSCombatComponent（倒地计时）、SkillCastHelper（门禁反向）、LSInputBufferComponent（倒地期输入路由）。建议并入"受击-施法互斥"缺口立项评估（R1-A4）。
2. （联动提醒）169-BACKSTEP §8 上报的"受击盒空帧语义修正"是本技能蹲伏无敌的现成载体——两技能共用该前提。

**翻译工具缺口**：`[IMAGE ROTATE]`（overturn.ani 躺姿语义承重，历批记档为消费延后，本技能首次成为**阻断级**）+ 超长 DELAY（重复印证）+ `.skl` 子命令（重复印证）。

**给下轮的经验**：受身蹲伏/倒地系查不到脚本属正常（引擎内置全职业通用态）；down.ani 的 F2/F3 悬停帧就是"躺地等待起身输入"的窗口本体——倒地系统的输入窗口设计可直接对标这两帧。sit.ani 是坐姿（休息）不是蹲伏，别混用。
