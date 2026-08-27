# 后跳（BACKSTEP）

> 技能ID 169 | 级别 B | 可实现性 🔶（位移主干直接可做；全程无敌帧需 1 处受击盒空帧语义修正） | 分析日期 2026-08-22 | 批次 B1

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 后跳 | `skill\Swordman\BACKSTEP.skl` [name] |
| 英文名 | BACKSTEP（取 skl 文件名；[name2]="Back Step" 是本 pvf 少见的真英文） | 同上 [name2] 实测 |
| 职业 | 鬼剑士共通（[skill fitness growtype] 0-5 全系） | 同上 |
| 学习等级 | 1 | 同上 [required level] |
| 最高等级 | 1（一次性系统技能） | 同上 [maximum level] |
| 类型 | active（**skill class 4 = 系统动作类**） | 同上 [type]/[skill class] |
| 指令 | ↓（按住）+ C（跳跃键）；[command] 为 `{6=(DOWN)}{8=&}{6=(JUMP)}` 组合 | 同上 [command] / [command key explain] |
| CD | **无 [cool time] 节**（skl 实测 grep 无命中）——限频靠 ↓ 按住指令门槛 | 同上 |
| MP | 1 | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| static data | `200 0 0 …`（首值 200 语义未考证；疑为起跳垂直速度或位移参数） | 同上 [static data] |
| level info | 4 列，仅 1 级：`0 -600 0 0`——**col1 = -600 为向后位移速度 px/s（推断**，负号=背向） | 同上 [level info] |
| 一句话效果 | 使自身向后方小跳并避开敌人的攻击 | 同上 [explain] |

## 2. 技能逻辑走读

### 2.1 注册与文件链

load_state 有注册（本批 6 技能中唯一）：

```
114: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/BackStep/BackStep.nut", "swordman_backstep", 7, -1);
```

- 状态号 **7 = 后跳**（L2 老一代技能状态号表实证：6 跳/7 后跳/8 普攻…）；第 5 参 -1 = 不绑定特定技能（由指令触发而非技能键）。
- 状态机 nut：`sqr\character\swordman\BackStep\backstep.nut`（22 行）；appendage：`ap_backstep.nut`（63 行）。

### 2.2 主 nut 逐回调

**backstep.nut 全文仅 1 个函数 `onAfterSetState_swordman_backstep`，且是 mod 注入的混淆代码**（变量名 `JM8H4uzkC2sc8UKfL7` 系随机串，C3/C6 形态；无任何原版状态回调）。提语义：

```
onAfterSetState(obj, state, datas, ...)：
  子状态 datas[0] == 49 时（技能 248 = swordman_stateoflimit 二觉被动的编排序列号）：
    若自身挂有 ap_stateoflimit.nut（beidong/ 目录，二觉被动 appendage）：
      写包(248, 1, bonusRate= sq_GetBonusRateWithPassive(248, -1, 0, 1.0))
      → sq_SendCreatePassiveObjectPacketPos(obj, 24370, 出生=自身当前位置)
      —— 即"剑神二觉被动期间的后跳会附带 24370 打击判定"（mod/二觉联动，L20 共享 PO）
```

**原版后跳逻辑完全引擎内置**（状态 7 的位移/起跳/落地收招全部在客户端引擎内，pvf 无脚本佐证）。

`ap_backstep.nut`：空脚手架（proc/onStart/onEnd/prepareDraw/isEnd 全部空函数体，实测读全文）——不承载任何逻辑。

### 2.3 引擎内置行为重建（.ani 数据 + 注册行 + 常识三方印证）

```
按 ↓+C：
  播 backjump.ani（7 帧 380ms：40×5 + 80 + 100）
  以 -600 px/s（level col1，推断）向背向水平位移（小跳弧线由引擎处理）
  F0-F4：SET FLAG 0（×5，语义未考证）；F5：flag 1；F6：flag 2（落地/收招事件，未考证）
  落地后回待机
```

**关键实证——后跳全程无受击盒**：`backjump.ani` 7 帧中 **DAMAGE BOX 计数 = 0**（grep 实测 + python 逐帧解析双验证），即动画全程角色不可被命中 = **380ms 全程无敌帧**（"避开敌人的攻击"的数据层实现）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 受击盒 | 备注 |
|---|---|---|---|---|---|---|
| `character\swordman\animation\backjump.ani` | 7（F0-6） | 380ms（40×5/80/100） | F0-F4=0，F5=1，F6=2 | 无 | **无（0/7 帧）** | 仅引 `sm_body%04d.img`（帧 188-193） |
| `back_jump_start/run/end.ani`（3+2+1 帧，225/300/50ms） | 6 | 575ms | 无 | 无 | 未验 | 另一组后跳动画（疑前冲后跳/剑影系变体，与本技能挂接未考证） |
| `passiveobject\...\state_of_limit_backstep_00/01/02.ani` | — | — | — | — | — | mod 技能 248 的后跳打击特效（本技能不依赖） |

`.als` 边车：backjump.ani 无（animation 目录 ls 实测）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BACKSTEP.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BACKSTEP.skl` | ✅ 实测（76 行） | 技能数据（-600 位移等） |
| 注册行 | swordman_load_state.nut:114 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 7 + backstep.nut |
| 主 nut | backstep.nut | `…\pvf\sqr\character\swordman\BackStep\backstep.nut` | ✅ 实测（22 行） | **仅 mod 注入的 248 联动钩子**，原版引擎内置 |
| appendage | ap_backstep.nut | `…\pvf\sqr\character\swordman\BackStep\ap_backstep.nut` | ✅ 实测（63 行空壳） | 无逻辑 |
| .chr 条目 | —（无 backstep 条目） | `…\pvf\character\swordman\swordman.chr` | ⛔ 无（实测 grep） | 动画走基础动作体系（见 §2.4 疑点） |
| 角色 .ani | backjump.ani | `…\pvf\character\swordman\animation\backjump.ani` | ✅ 实测 | 后跳动作（7 帧 0 受击盒） |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\`（ls grep backstep 无） | ⛔ 无 | 位移技无攻击 |
| .als | — | `…\pvf\character\swordman\animation\`（ls grep 无） | ⛔ 无边车 | — |
| 装备层 | backjump.ani ×N | `…\pvf\equipment\character\swordman\avatar\{belt,coat,…}\` | ✅ 实测（belt_a/coat_a 命中） | 换装图层 |

> 疑点备注：.chr 基础动作槽（[jump motion] 等）未见后跳专属条目，backjump.ani 的挂接槽位未考证（无 header 常量、无 .chr 条目，疑引擎按状态 7 直呼文件名加载——同族旁证：jump.ani 也直接在 animation 目录按名使用）。

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 188-193） | sprite_character_swordman_equipment_avatar_skin.NPK | 后跳动作 7 帧 | 必需 | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` 已在库（L16 单图集） |

缺失 img：**0**（角色侧零缺口；mod 的 state_of_limit 特效不提取）。

## 5. 实现方案草案

- **内容件清单**：
  - `DotNet~/Skills/BackStepSkill.cs : SkillLogic`——同 `ReleaseWaveSkill` 位移范式：
    - `CooldownMs`：DNF 原值无 CD；**demo 建议值 500**（我们无"↓按住"指令门槛，不设 CD 会按键连打无限后跳）。
    - `TotalTimeMs = 380`（backjump.ani 总时长直用）。
    - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanBackJump)`。
    - `OnUpdate`：位移纯函数（releasewave §5.6-2 同构）——`min(ElapsedMs, 380)/380 × 2.28 单位` 按帧差增量 `ctx.MoveCasterForward(-增量)`（**负距离 = 背向移动**，MoveCasterForward 源码实测负值自然反向）；总位移 = 600px/s × 0.38s ÷100 = 2.28 单位。
    - `OnEnd`：`ctx.PlayDefaultAnim()`。
- **无敌帧（"避开攻击"）**：backjump.ani 翻译后 json 无 damageBoxes → 但当前 `LSHitboxComponentSystem.LSUpdate` 的回退分支会把零值 `frame.damageBox` 采样成一个贴地点盒（源码 line 44-47 实测），角色仍可被贴地攻击命中；`LSAreaSystem` 虽有 `CurrentHurtBoxes.Count == 0` 跳过分支（line 84 实测）但回退分支使 Count 恒 ≥1。**使能修正（约 3 行）**：damageBoxes 为 null 且 damageBox 全零 → 不加入受击盒列表——修完后"空受击盒帧=无敌帧"零成本成立（近战+区域两条管线同时生效）。未修正前先做"仅位移躲判定"版（后跳躲出攻击盒范围），修正后升格完整版。
- **概念映射**：状态 7 + backjump.ani → `BackStepSkill : SkillLogic` + `AnimId.SwordmanBackJump`；引擎 -600px/s 位移 → `MoveCasterForward(负值)` 纯函数；无受击盒帧 → 翻译 json 原样保留空 + 受击盒空帧语义修正。
- **注册点**：`SkillIds.BackStep = 23`（ButtonToSkill 新键）；`AnimId.SwordmanBackJump = 104`（LSAnimClipRegistrar 注册 backjump 翻译 json，帧表无盒照常输出）；BuildAtlas 无增量（sm_body 已在）。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 无（指令门槛限频） | 500ms（防连打） |
| 总时长 | 380ms（7 帧） | 380 直用 |
| 位移速度 | -600 px/s（level col1，推断） | 6 单位/s 背向 |
| 总位移 | ≈228px | 2.28 单位 |
| 无敌 | 全程 380ms（0 受击盒帧实证） | 同（依赖空受击盒语义修正） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `BACKSTEP.skl` | `.skl` 无子命令（4 列 level info） | 本技能手抄 2 值即可；`skl` 子命令同前议（历批已多次记档） |
| `backjump.ani` | `[SET FLAG]`（0×5/1/2）、`[PLAY SOUND]` | 既有约定整节跳过（触发帧不进翻译链路）；本技能位移不依赖 flag，无缺口 |

结论：**ani 资源全部可被现有 ani 子命令翻译**；实质缺口仅 `.skl` 无子命令（1 条，重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| ↓按住+C 指令触发 | 无组合键/修饰键输入（延后档：指令系统） | 独立按键直发；CD 500ms 补限频 |
| 全程无敌帧（0 受击盒帧） | **受击盒空帧语义**：现回退分支把零盒当贴地点盒（见 §5，非系统级缺失，属数据约定修正） | 先发布"仅位移躲判定"版；修正后升格 |
| 小跳弧线（z 轴抛物线） | 跳跃系统（缺失档，R1-A2 已记；水平位移不受影响） | 平移版后跳（视觉略平，可接受） |
| 撞墙检测 | 无地图碰撞（延后档） | 无限后退（demo 场地无墙） |
| mod 的 248 后跳联动打击 | mod 注入内容 + 跨技能取消体系（缺失档） | 不实现（非原版内容） |
| F5/F6 flag 事件 | 引擎内置语义未考证 | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. `[static data] 200` 首值语义（起跳速度/位移参数猜测）。
2. backjump.ani 挂接 .chr 的具体槽位（无条目无常量，推断引擎按状态直呼文件名）。
3. F0-F4 flag 0 连发 + F5=1/F6=2 的语义（值 0 疑为空标记）。
4. back_jump_start/run/end 三件套与本技能的关系（疑前冲后跳变体）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **受击盒空帧语义修正（低成本使能项，非新系统）**：`LSHitboxComponentSystem` 回退分支把"无 DAMAGE BOX 帧"采样成贴地零盒 → 建议约定 `damageBoxes == null && damageBox 全零` = 无受击盒（Area 侧 Count==0 跳过分支已存在，天然配套）。修完后 DNF"无敌帧=空受击盒帧"的通用表达零成本成立——后跳/受身蹲伏/跳跃系全族受益，比立项独立无敌帧系统便宜得多（对 R1-A5"无敌帧"缺口的落地路径建议）。

**翻译工具缺口**：`.skl` 子命令（1 条，重复印证）。

**给下轮的经验**：后跳/受身蹲伏这类"指令触发的系统动作"（skill class 4）在 load_state 的注册行第 5 参是 -1（不绑技能键），按技能 ID 反查不到——按状态名/目录名查。backstep.nut 是 mod 污染壳（C6④形态：文件存在但全是 mod 代码），别当原版逻辑读。
