# 冰刃·波动剑（IceWave）

> 技能ID 21 | 级别 A | 可实现性 ✅（直接，基础版） | 分析日期 2026-08-22 | 批次 示范（主循环执笔，子 agent 参照格式与粒度）

**示范说明**：本文档是《03-鬼剑士全技能解析指导手册》§7 模板的**标准示范**。
子 agent 交卷的格式、走读粒度、表格用法、诚实标注"未考证"的方式都以本文为准。
注意本文也示范了一个重要情况：**技能无独立 pushState、多技能共用一个 nut**（§2.1）。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 冰刃 · 波动剑 | `skill\Swordman\IceWave.skl` [name] |
| 英文名 | IceWave（取 skl 文件名；本 pvf 的 [name2] 是中文别名"冰刃 · 波动剑"，不是英文） | 同上 [name2] 实测 |
| 职业 | 阿修罗（[skill fitness growtype]=4；wave 系常识） | 同上 |
| 学习等级 | 30 | 同上 [required level] |
| 最高等级 | 70 | 同上 [maximum level] |
| 类型 | 主动（active） | 同上 [type] |
| 指令 | —（skl 前 45 行未见 [command]，未全文检索） | 同上 |
| CD | 7000 ms（蓄力版另计 10000 ms，见 §6） | 同上 [cool time] / [skill under cooltime effect] |
| MP | 未提取（未读消耗节全文） | 同上 |
| 特殊消耗 | 无 | 同上 |
| 一句话效果 | 发射地面冰波穿透敌人，概率冰冻；地面升起小冰柱多段打击 | 同上 [explain] + [level property] 模板 |

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无独立注册**（实测 `grep -i icewave` 无命中）。
波动剑系（20 地裂 / 21 冰刃 / 22 爆炎 / 24 血气）共用一条注册：

```
16: IRDSQRCharacter.pushState(0, "character/Swordman/wave/wave.nut", "WaveSword", 24 ,-1 );
17: IRDSQRCharacter.pushPassiveObj("character/Swordman/wave/po_wavecut.nut", 24328);
```

- 状态名 `WaveSword`，nut 为 `sqr\character\swordman\wave\wave.nut`（74 行），job 参数为 0（写法变体）。
- 弹体被动对象 `po_wavecut.nut` 注册 ID **24328**。
- 基础版（技能 21）的施法分流**未考证**（按名检索白名单内无命中；推断走共用状态/引擎默认，
  需要时按数字 ID 追）。已实测到的是**强化版**分支（见下）。

### 2.2 主 nut 逐回调（wave.nut，施法侧）

`onKeyFrameFlag_WaveSword(obj, flagIndex)`——仅一个活跃回调（其余被注释）：

```
if 子状态(var state[0]) == 100:            // 100 = IceWaveEx（强化冰刃，lst 实测 ID 100）
    atk  = sq_GetPowerWithPassive(100, 24, 3, -1, 1.0)   // 伤害倍率（列 3）
    atk2 = sq_GetPowerWithPassive(100, 24, 7, -1, 1.0)   // 二段伤害（列 7）
    count = sq_GetLevelData(100, 1, lv)                  // 小冰柱数量（列 1）
    dist=75, size=100, maxT=3000                         // 距离/大小(100%=1倍)/存在时长
    prob=列4/10, lv=列5, time=列6                         // 冰冻概率/等级/时长
    写包(伤害, 125, count, 1, dist, size, y=±15, maxT, prob, lv, time, atk2)
    → sq_SendCreatePassiveObjectPacket(24328, 0, 75, 1, 0)   ×2 次（y=-15 与 y=+15 → 上下两道波）
```

`onAfterSetState_WaveSword`：记录子状态到 var；若挂有 wavemark appendage 则联动推波（波动印记联动，缺 Buff 查询门面，§6）。

### 2.3 被动对象（po_wavecut.nut，179 行，弹体逻辑核心）

`setCustomData_po_wavecut` 按包内 id 分支，实测到 `id == 125`（IceWaveEx）：

```
播 "passiveobject/zrr_skill/newswordman/animation/icewave.ani"（实测为 6 帧空占位，IMAGE 路径为空）
attackInfo = sq_GetCustomAttackInfo(obj, 103)          // 对象自有攻击信息表（角色 attackinfo/ 无 icewave.atk，实测）
sq_SetCurrentAttackPower(attackInfo, attackBonusRate)  // 伤害来自施法者写入的等级数据
sq_SetAttackBoundingBoxSizeRate(ani, size/100 ×3)      // 判定盒随大小等级缩放
特效：createIceWaveExAnimation → icewaveex/1~6.ani 按大小档位选择（pooled object，含 setImageRateFromOriginal 缩放）
粒子：icewaveexiceexplosionparticle1.ptl 等
状态：sq_SetChangeStatusIntoAttackInfo(attackInfo, 0, ACTIVESTATUS_FREEZE, prob, lv, time)   // 概率冰冻
存 var：伤害/数量/距离/大小/y/maxT/冰冻三参/atk2 + flag 计数 → 多段与生命周期用
```

`onAttack_po_wavecut`（每次命中）：

```
若目标可抓取/非霸体：
    挂 appendage ap_wavehold（200ms 定身 + sq_HoldAndDelayDie 命中硬直表现，有效期 2000ms）
```

——即"命中即短定身 + 概率冰冻"双层控制。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| passiveobject\zrr_skill\newswordman\animation\icewave.ani | 6 | 未逐帧加总（DELAY 未提取） | 无实测命中 | —（弹体判定在 attackInfo） | **空占位**：IMAGE 全为空路径 |
| passiveobject\character\swordman\animation\icewaveex\1~6.ani | 未读 | — | — | — | 强化版特效，按大小档位选用 |
| 角色施法动画 | **未定位** | — | — | — | .chr 与 animation/ 无 icewave 条目（实测 grep）；波动剑共用施法动作，具体条目未考证 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | IceWave.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\IceWave.skl` | ✓ | 技能数据（等级/CD/等级属性） |
| 注册行 | swordman_load_state.nut 行 16/17 | `...\pvf\sqr\character\swordman_load_state.nut` | ✓ | WaveSword 状态 + 24328 对象（共用） |
| 主 nut | wave.nut | `...\pvf\sqr\character\swordman\wave\wave.nut` | ✓ | 施法侧（实测仅强化版 100 分支） |
| 弹体 nut | po_wavecut.nut | `...\pvf\sqr\character\swordman\wave\po_wavecut.nut` | ✓ | 弹体逻辑（冰冻/定身/缩放） |
| hold appendage | ap_wavehold.nut | `...\pvf\sqr\character\swordman\wave\ap_wavehold.nut` | ✓（存在，未细读） | 命中定身 |
| .chr 条目 | — | `...\pvf\character\swordman\swordman.chr` | **无 icewave 条目** | 施法动画共用，条目未考证 |
| 角色 .ani | — | `...\pvf\character\swordman\animation\` | **无 icewave 文件** | 同上 |
| 角色 .atk | — | `...\pvf\character\swordman\attackinfo\` | **无（波动系仅 shockwavearea/standalonewave/waveeye 系列）** | 命中参数在对象表 103 |
| 弹体 .ani | icewave.ani | `...\pvf\passiveobject\zrr_skill\newswordman\animation\icewave.ani` | ✓ | 空占位（6 帧） |
| 特效 .ani | icewaveex\1~6.ani | `...\pvf\passiveobject\character\swordman\animation\icewaveex\` | ✓ | 强化版视觉 |
| 粒子 | icewave*.ptl ×7 | `...\pvf\passiveobject\character\swordman\particle\` | ✓ | 冰爆/烟雾/星屑粒子 |
| 装备层 | — | `...\pvf\equipment\character\swordman\avatar\` | 未查 | 施法动画定位后再查 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （icewave.ani 为空占位，无 img 引用） | — | — | — | — |
| icewaveex\1~6.ani 引用的 img（**未提取清单**，需打开这 6 个 ani 逐一 grep IMAGE） | 待推导（sprite_passiveobject_character_swordman_animation_icewaveex 系） | 强化版冰波视觉 | 可选（先做基础版） | 否 |
| LightningPOW_ADD_A.img（`Character/Swordman/Effect/LightningPower/`，ice1add_eff_a.ani 引用；**与冰柱的关联未考证**） | sprite_character_swordman_effect_lightningpower.NPK | 存疑项 | 存疑 | 否 |
| .ptl 粒子 ×7 | 粒子无 NPK 直取规则（.ptl 引用 img 需打开确认） | 冰爆粒子 | 可选 | 否（且无粒子系统） |

**结论**：基础版若以"自推帧弹体 + 特效 ani"实现，最小资源 = 强化版特效 ani 对应 img 中选一套；
空占位 icewave.ani 证明 DNF 本体视觉也全靠特效层，与 bloodboom 占位动画同一处理（跳过）。

## 5. 实现方案草案

- **内容件清单**（全部有先例，零新机制）：
  - `IceWaveSkill : SkillLogic`——同 `WaveSwordSkill` 范式：OnCast 里 `ctx.CreateBullet(BulletIds.IceWave)`；CD 7000。
  - `IceWaveBullet : BulletDefinition`——复制 `NormalWaveBullet` 改：`DestroyOnHit=false`（穿透）、
    `TotalTimeMs≈1500`、`HalfExtents` 按冰波略放大、`HitActions = { MeleeHit, AddFreezeBuff }`、`ViewAnimId=新 AnimId`。
  - `AddFreezeBuffAction : LSAction`——同 `AddBleedBuffAction`/`AddBurnBuffAction` 同构（新写一个 ~10 行）。
  - **复用** `FreezeBuff`（已存在：3.5s ForbidMoveOn/Off 定身）——概率冰冻若走
    `HitReaction.ProcBuffId/ProcChance`（MonsterIceBreath 先例）则连 Action 都可省。
- **概念映射**：sq_SendCreatePassiveObjectPacket(24328) → `ctx.CreateBullet`；
  sq_SetChangeStatusIntoAttackInfo(FREEZE) → `HitReaction.ProcBuffId+ProcChance`；
  sq_HoldAndDelayDie → 定身简化并入 FreezeBuff（无独立 hold 机制，§6）；
  自推帧弹体动画 → `BulletDefinition.ViewAnimId`。
- **注册点**：SkillIds 加 `IceWave`；AnimId 加冰波特效；LSAnimClipRegistrar 注册翻译后的 json；
  LSAnimResComponentSystem.BuildAtlas 加图集；demo 按键映射在 LSOperaComponentSystem。
- **关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 7000 ms | 7000（直用） |
| 冰冻概率 | 等级数据列 3：基准 4% + 0.1%/级（向量 [-1,4,0.1]，**推断**） | 25%（固定，演示明显） |
| 冰冻时长 | 列 5：基准 5s? + 1.0s/级（向量 [-1,5,1.0]，**推断**） | 3.5s（对齐 FreezeBuff 现值） |
| 小冰柱数量 | 列 1：基准 1 + 1/级 | 1（demo 不做多段，见 §6） |
| 伤害 | atk 列（魔法攻击力%向武器基数结算） | MeleeHit 固定值（demo 惯例） |
| HitReaction 击退/浮空 | 对象表 103 未读取 | 0/0（冰波不击退，冰冻控制） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| icewaveex\*.ani / 弹体 ani | （预期为常规节，未逐个核对——**示范注意：子 agent 此处必须实际核对**） | 常规即可走 `ani` 子命令 |
| icewave*.ptl ×7 | **.ptl 粒子文件无子命令** | 暂跳过（游戏无粒子系统）；视觉用特效 ani 替代 |
| sq_SetAttackBoundingBoxSizeRate / setImageRateFromOriginal | 运行时缩放（非翻译问题） | 游戏侧缺"对象整体缩放"，归 §6.3 延后档 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 蓄力版（按住→强化冰波，CD 10s） | 无按住蓄力输入（延后） | demo 只做瞬发基础版；蓄力版等输入缓冲扩展 |
| 命中 200ms hold 硬直（ap_wavehold） | 无 hold/抓取微控机制（缺失档） | 并入冰冻：命中即 FreezeBuff 定身，手感略强 |
| 判定盒/图像随等级缩放 | 对象整体缩放（延后，IMAGE RATE 同类） | demo 固定 100% |
| 小冰柱多段（count>1） | 多段命中重置（延后） | 单次穿透伤害 + 概率冰冻 |
| 波动印记联动增伤 | Buff 查询门面（缺失） | 跳过 |
| 粒子特效 | 粒子系统（缺失） | 特效 ani 替代或不做 |

## 8. 存疑与缺口上报

- **未考证**：基础版（21）施法侧分流位置（仅实测到强化版 100 分支）；角色施法动画条目；
  对象攻击信息表 103 的具体参数（未读到表）；等级数据向量→公式的精确读法（§5 已标推断）。
- **新缺口上报**：①粒子系统（.ptl，多个技能都会撞上）；②对象整体缩放（随等级尺寸）；
  ③命中 hold 微控（独立于冰冻的 200ms 定身）。均已建议归档档位。
- **给下轮的经验**：波动剑系技能（20/21/22/24 + 全部 Ex）**直接从 `wave\` 目录入手**，
  别按技能名搜 load_state（只有一条共用注册）；弹体参数全在写包顺序里，读 po_wavecut 的
  setCustomData 分支即可。
