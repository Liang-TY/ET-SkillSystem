# 流心（FlowMind）

> 技能ID 105 | 级别 B（预判 A 纠偏：无独立攻击，是连段形态枢纽状态技） | 可实现性 ⛔（本体价值依赖技能取消体系；与 107 合并降级为 🔶，见 §5） | 分析日期 2026-08-22 | 批次 A6

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 流心 | `skill\Swordman\FlowMind.skl [name]` |
| 英文名 | FlowMind（skl 文件名；[name2] 实测为 `Flow Heart`，本技能 name2 恰是英文） | 同上 |
| 职业 | 剑魂（[skill fitness growtype]=1，L17 映射） | 同上 |
| 学习等级 | 20 | 同上 [required level] |
| 最高等级 | 1（growtype maximum level：剑魂位=1，其余 0） | 同上 |
| 类型 | active（skill class 1） | 同上 [type] |
| 指令 | 空（`[command]` 节为空；实际由引擎按"技能键/自定义键位"触发，jump 脚本里有 `setSkillCommandEnable(105, true)` 动态开令） | 同上 + `jump\swordman_jump.nut` |
| CD | 1000 ms（地下城）/ 2000 ms（pvp）；[auto cooltime apply] 0 → CD 由脚本手动 `startSkillCoolTime` 起表 | 同上 |
| MP | 消耗 20-28 + **维持 10.0-60.0/秒**（[maintain MP]，架势持续期间每秒扣） | 同上 |
| 特殊消耗 | 施法时间 500 ms（[casting time]，推断=入架势动画时长） | 同上 |
| static data | `0 0 0 0 0`（全零，无脚本消费点） | 同上 [static data] |
| 一句话效果 | 进入"流心"架势：摆出准备姿势并维持，期间可用 X/C 等后继键派发 流心:刺(107)/跃(108)/升(109)；按住技能2键(SKILL2)可挂 流心:狂(110) 的暴击增益窗口 | 同上 [explain] + 走读 |

## 2. 技能逻辑走读

### 2.1 注册与文件链

`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\sqr\character\swordman_load_state.nut`（实测两行绑定 105）：

```
139: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/flowmind/flowmindonefallstate.nut", "FlowMindOneFallState", STATE_FLOW_MIND_ONE_FALL_STATE, 105);
142: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/weaponmaster/flowmind/flowmind.nut", "FlowMind", 61, 105);
```

- 状态 **61 = FlowMind（本技能本体）**，nut 为 `sqr\character\swordman\weaponmaster\flowmind\flowmind.nut`（43 行）。
- 状态 **147 = FlowMindOneFallState**（流心:刺的空中落下段，属 107 的形态，但注册行第 5 参也写 105——mod 疑点，见 107 文档 §8）。
- **后继派发状态 62（刺）/63（跃）/64（升）无任何 pushState 注册**（全文件 72 条逐一核对）——引擎内置状态号，pvf 只留数据与 .ani（详见 107 文档）。

### 2.2 主 nut 逐回调（flowmind.nut，43 行，实测）

- `onSetState_FlowMind(obj, ...)`：仅读 datas[0] 存局部变量，**无实际行为**——入架势动作/扣消耗全在引擎侧（播 FlowMindStart→FlowMindStay 循环）。
- `onProc_FlowMind(obj)`（每帧）：
  ```
  fmjt = sq_GetIntData(obj, 110, 1)                        // 读技能 110(流心:狂) 的 static data 槽 1
  若按住 SKILL2 键：
      移除旧 ap_liuxing → 重挂 ap_liuxing（validTime = fmjt）  // 流心:狂 暴击增益窗口刷新
  ```
  即**流心(105) 架势中按住技能2键 = 持续维持 流心:狂(110) 的增益标记 appendage**（ap_liuxing 是 4 行标记型 appendage，注册的 onEnd/isEnd 回调名 `*_appendage_BAOJI`（暴击）在白名单内无定义——引擎消费，具体增益数值未考证；110 的 dungeon static 有两块 `(300 0)`/`(300 10000)`，fmjt 推断=10000ms，未考证）。
- 无 onKeyFrameFlag / onEndCurrentAni——架势退出（超时/后继派发/受击）由引擎状态机处理，**未考证**。

**后继派发（引擎 + mod 钩子双轨）**：
- 引擎：架势中按 X → 状态 62（流心:刺，107 的 skl 指令节写明"([流心] 动作中) X"）。
- mod 增强（`sqr\character\swordman\swordman_common.nut` 的 `procAppend_Flowmind_Comminterrupt`，仅 growType≤1 生效）：站立/走路/跃收招等状态直接 EnableSoften+SetSkillState 进 62/63/64，跳过架势——**这层是 mod 加的快捷取消**（原版需 先流心/强制-流心）。另有 `skill\Swordman\cancelflowmind.skl`（强制-流心，不在 241 清单内，存档记名）。

### 2.3 被动对象 / appendage

无攻击被动对象。appendage：`ap_liuxing.nut`（4 行，流心:狂 标记，见 2.2）。同目录 `ap_newdam.nut`/`ap_weiyi1-3.nut` 同为 4-18 行标记壳（BAOJI 同名回调、无脚本引用者，疑似 mod 遗留，未细读）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\FlowMindStart.ani`（.chr etc motion #104） | 4 | 名义 10240ms | 无 | 无 | F0 delay=10000ms 悬停帧（引擎状态切换时切断；同 jump.ani 前例，翻译需钳制），F1-3=80ms；仅引 `sm_body%04d.img` |
| `character\swordman\animation\FlowMindStay.ani`（#105） | 1 | 80ms | F0=65534（取消窗口标记，064 同款） | 无 | **LOOP**（架势维持循环）；仅引 `sm_body%04d.img` |

`.als` 边车：两动画均无（animation 目录 ls 实测）。资源侧结论：**流心架势零新增 img**（纯角色姿态帧，sm_body0000.img 已入库）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FlowMind.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FlowMind.skl` | ✅（84 行） | 技能数据（CD/维持MP/施法时间） |
| 注册行 | swordman_load_state.nut:142 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 61 绑技能 105 |
| 主 nut | flowmind.nut | `…\pvf\sqr\character\swordman\weaponmaster\flowmind\flowmind.nut` | ✅（43 行） | 仅 SKILL2→流心:狂 窗口维持；架势本体引擎侧 |
| appendage | ap_liuxing.nut | `…\pvf\sqr\character\swordman\weaponmaster\flowmind\ap_liuxing.nut` | ✅（4 行标记壳） | 流心:狂 增益标记 |
| 钩子 | swordman_common.nut（procAppend_Flowmind_Comminterrupt） | `…\pvf\sqr\character\swordman\swordman_common.nut` | ✅（1-130 行走读） | mod 快捷取消进 62/63/64/147 |
| 钩子 | swordman_jump.nut | `…\pvf\sqr\character\swordman\jump\swordman_jump.nut` | ✅ | 跳跃中经流心指令进空中刺/跃（107/108 关联） |
| .chr 条目 | etc motion #104/#105 | `…\pvf\character\swordman\swordman.chr` 1077/1078 行 | ✅ | FlowMindStart/Stay.ani |
| 角色 .ani | FlowMindStart.ani / FlowMindStay.ani | `…\pvf\character\swordman\animation\` | ✅ | 见 §2.4（另有同名小写 flowmindstart/stay.ani，与 .chr 无关，推断冗余副本） |
| .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 不存在 | 架势无攻击 |
| .als | —（无） | 同上 animation 目录 | ⛔ 不存在 | — |
| 装备层 | flowmindstart/stay.ani 等流心系 | `…\pvf\equipment\character\swordman\avatar\belt\belt_a\`（15 个流心系文件，ls 实测） | ✅ 存在 | 换装图层（只查存在性） |
| 关联被动 | cancelflowmind.skl（强制-流心） | `…\pvf\skill\Swordman\cancelflowmind.skl` | ✅（不在 241 清单内） | 原版取消被动，记档 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色架势帧（%04d 模板单图集） | 必需（共享） | ✅ 已在 `Bundles\AnimRes\sm_body0000.img.bytes` |

**缺失 img：0 张。**

## 5. 实现方案草案（⛔ 级；此处给降级合并方案）

**独立实现 105 的判定 = ⛔**：本技能的全部价值是"架势→后继技能派发枢纽"。我们现有系统只有同技能 `RestartCurrentSkill` 连段取消（SkillCastHelper.TryCast 在技中直接拒绝、无"结束当前施放并转施另一技能"门面）——**跨技能连携是已记档缺口（064 §8 首报"技能取消体系"），流心族是最大用户**。

**降级方案（🔶 可行）：与 107 合并为单技能状态机**（L19 连段思路）：

- 内容件：`FlowMindComboSkill : SkillLogic`（同 NormalAttack/BloodBoom 范式）：
  - `TotalTimeMs = 0`（自管）；`OnCast`：`ctx.PlayAnim(AnimId.FlowMindStart)` → 完成后切 `AnimId.FlowMindStay`（循环）+ SubState=0（架势中）。
  - `OnUpdate`：SubState=0 期间轮询 `ctx.PeekBufferedButton()==<X键>` → 消费缓冲、`ctx.PlayAnim(AnimId.SwordmanFlowMindOne)`、SubState=1，把控制权交给内嵌"刺"段（107 文档 §5 的段编排原样嵌入）；无输入超时（建议 2000ms，DNF 原版由引擎管，未考证）→ `OnEnd` 回默认动画。
  - MP 维持/流心:狂窗口：跳过（无 MP 系统、无暴击消费链，见 §7）。
- 概念映射：引擎状态 61+62 合并 → 单 LSCast.SubState 状态机；SKILL2 流心:狂 → 跳过。
- 注册点：SkillIds 15（FlowMindCombo，demo 键建议 Space 长按/单击）；AnimIds 59/60（FlowMindStart/Stay）+ 61（复用 107 的刺动画，见 107 文档）。
- 数值表：CD 1000ms 直用；架势维持上限 demo 2000ms（DNF 未考证）；维持 MP 跳过。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| FlowMind.skl | `.skl` 无子命令 | 手抄 CD/MP 可行；随批量化建 `skl` 子命令（累计缺口） |
| FlowMindStart.ani | F0 `[DELAY] 10000` 超长悬停帧 | 翻译侧需钳制或约定手改（jump.ani F7/F14 同款，已在轮间经验记档） |
| FlowMindStay.ani | `[SET FLAG] 65534` | 既有约定跳过（触发帧 const 进技能类），非缺口 |

其余（LOOP/IMAGE）现有 ani 子命令全覆盖。**本技��翻译缺口 1 类（.skl）。**

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 架势→X/C 派发 刺/跃/升（跨技能连携枢纽） | **缺失：技能取消体系**（064 首报；本技能为其最重依赖者） | 与 107 合并单技能状态机（§5）；跃/升（108/109，他批）后续以同法并入或等取消体系立项 |
| mod 快捷取消（站立/走路直接进 62/63/64） | 同上 | 不实现（原版也不允许） |
| 维持 MP 10-60/s（架势计时经济约束） | 延后：无 MP 系统 | 用固定超时 2000ms 替代"MP 尽架势断" |
| 流心:狂(110) 按住 SKILL2 挂暴击窗口 | 缺失：暴击消费链 + Buff 查询门面（属性数值无伤害消费链，R1-A4 首报） | 跳过 |
| 65534 取消标记 | 引擎语义（064 同款，未考证） | 忽略 |

## 8. 存疑与缺口上报

- **未考证**：架势最长维持时长与退出条件（引擎侧；FlowMindStart F0=10000ms 悬停帧疑为上限线索）；fmjt（110 static 槽 1）精确值与两块 static 块的索引规则；ap_liuxing 的实际增益数值（BAOJI 回调无脚本定义）。
- **系统级缺口（重申+补证）**：**技能取消体系/跨技能连携门面**——本批实证其消费面：`swordman_common.nut` 的 EnableSoften/SetSkillState 钩子 + jump 脚本空中入口 + 引擎架势派发，流心族(105/107/108/109)整套手感都压在这上面。建议 01§0.4 该行补"流心族依赖"注记。
- **新坑（供轮间经验）**：流心族存在**双目录**：`sqr\character\swordman\flowmind\`（小写、含空中段完整脚本 + 混淆版 flowmindone.nut——与 backstep/chargecrash 同一 mod 手笔，均检测"极·神剑术(248) appendage 授霸体"）与 `weaponmaster\flowmind\`（原版风格）；检索时两处都要看。
- 给下轮：后继技能 108/109 的状态 63/64 同样无注册（引擎内置），但 `weaponmaster\flowmind\flowmindtwo.nut`（202 行）/`flowmindthree.nut` 有同名回调脚本可走读，加载机制未考证（推断引擎按状态名约定绑定同目录 nut）。
