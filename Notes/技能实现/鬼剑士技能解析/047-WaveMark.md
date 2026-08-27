# 波动刻印（WaveMark）

> 技能ID 47 | 级别 B（资源系统/开关增益） | 可实现性 ⛔（刻印→增伤的资源转化链全卡在属性消费链 + 自身 Buff 查询门面；开关壳/刻印计数/剑气弹可拆出单独落地，但核心价值"印越多伤害越高"无法表达） | 分析日期 2026-08-22 | 批次 B3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 波动刻印 | `skill\Swordman\WaveMark.skl` [name] |
| 英文名 | WaveMark（取 skl 文件名；[name2]="Wave Seal"） | 同上 [name2] 实测 |
| 职业 | 阿修罗（[skill fitness growtype]=**4**，L17） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 30（阿修罗档 20：`0 0 0 0 20 0`） | 同上 |
| 类型 | active（skill class 1） | 同上 [type]/[skill class] |
| 指令 | ↑↑ + Space（BUFF 键） | 同上 [command] |
| CD | 10000 ms | 同上 [cool time] |
| MP | 10 → 14 + **维持 MP 1.0~8.0/s**（[maintain MP]，开启期间持续耗蓝） | 同上 [consume MP]/[maintain MP] |
| 读条 | casting time 200 ms | 同上 [casting time] |
| CD 机制 | [auto cooltime apply] 0（不自动进 CD——开关型） | 同上 |
| static data | `500 30000 5 10000`（static[1]=30000→印持续 30s、static[2]=5→上限 5 印，由 level property 向量实证；static[0]=500/static[3]=10000 未直接引用，static[3] 与 col2 同值疑同义） | 同上 [static data] + level property |
| 一句话效果 | 开关型：开启后每 10s 生成一个波动印（上限 5，每印 30s），印越多智力/施放速度越高；普攻/上挑附带波动剑气 | 同上 [explain] |

**level property 模板解码（7 列，L21 向量法全解，Lv1→Lv41 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 增加智力 | (-1, 0, ×1.0) | col0 = **14 → 295**（每印的智力，随印数叠乘在引擎侧） |
| 增加施放速度 | (-1, 1, ×0.1) | col1 = 12→246 → **1.2% → 24.6%**（同上，随印数） |
| 生成时间间隔 | (-1, 2, ×0.001) | col2 = 10000 → **10 s 恒定**（与 static[3] 同值） |
| 波动印持续时间 | **(1, 1, ×0.001)** → static[1] | 30000 → **30 s 恒定** |
| 波动印数量上限 | **(2, 2, ×1.0)** → static[2] | **5 个恒定** |
| 波动剑气魔法攻击力 第一/二/三击 | (-1, 3/4/5, ×1.0) | col3/4/5 = **62/124/186 → 101/163/225**（%向魔法攻击力结算，攻击类列） |
| 基本攻击力和技能攻击力增加 | (-1, 6, ×0.1) | col6 = 100→490 → **10% → 49%**（开启即得的全局增伤，写入 appendage） |

static[0]=500 语义未考证（疑首次施放即产 1 印的初始延迟/参数，explain"首次施放时会生成一个波动印"互证其存在）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无独立 pushState**——开关走共用投掷/增益态（load_state:117）：

```
117: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "Character/swordman/swordman_throw.nut", "swordman_throw", 13, -1);
```

技能脚本链（白名单内全部实测）：

| 文件 | 行数 | 角色 |
|---|---|---|
| `sqr\character\swordman\swordman_throw.nut` case 47 | 67 | **开关本体** |
| `sqr\character\swordman\appendage\ap_wavemark.nut` | 16 | 开关态 buff：10s 后再按 47 自动关（防误触）+ 持印容器 |
| `sqr\character\swordman\attack\ap_wavemark.nut` | 34 | 同名空骨架（onStart/onEnd/proc 全空——另一引用路径，疑原版残留） |
| `sqr\character\swordman\attack\attack.nut` 内 `wavemark_qq506807329_swordman_attack`（mod 注入，C3） | ~25 | **普攻剑气触发**（写包 24370） |
| `sqr\character\swordman\wavemark_jianqi\wavemark_jianqi.nut` + `po_wavemark_jianqi.nut` | 59+77 | 原版风格剑气实现（**未被 load_state 注册，判定死代码/幸存数据**，见 §2.4） |
| `sqr\character\swordman\swordman_common.nut` / `wave\wave.nut` / `indaramang\indaramang.nut` | — | 刻印追加调用点（`sqx_WaveMarkPush`） |

### 2.2 主逻辑逐段（switch 分支走读）

**开关（swordman_throw.nut onAfterSetState_swordman_throw case 47）**：

```
若已挂 ap_wavemark → 移除（关闭）——toggle 语义
否则：
  bonus = sq_GetLevelData(47, 6, level)        // col6 = 基本攻击力和技能攻击力增加
  挂 ap_wavemark.nut（appendage var "ap" 存 bonus——全局增伤的载体）
```

**ap_wavemark.nut（appendage 侧，proc）**：

```
宿主死亡 → 失效
timer < 10000 → return（开启后 10s 内不接受再按关）
若 sq_IsEnterSkill(47) 且 sq_IsUseSkill(47) → setValid(false)   // 10s 后再按 = 手动关
```

**刻印生成（引擎内置 + 脚本加速点）**：

```
引擎侧（不可走读）：每 col2=10s 生成一印；首次施放立即 1 印（explain）
  每印持续 static[1]=30s；上限 static[2]=5
  印数 ×（col0 智力 + col1 施放速度）→ 引擎属性系统（无消费脚本）
脚本加速点（施放波动剑族技能时立刻追加一印，sqx_WaveMarkPush(obj,1,1)）：
  swordman_common.nut addSetStatePacket_Swordman：state 52 / 54 /（skillState 32 = 波动爆发 技能32）
  wave\wave.nut onAfterSetState_WaveSword（地裂/冰刃/爆炎共用态）
  indaramang\indaramang.nut（因陀罗天）
  —— sqx_WaveMarkPush 函数定义在白名单外（mod 全局），未考证；语义按调用面=加印
```

**普攻剑气（attack.nut mod 函数，onAfterSetState_swordman_attack 尾部调用）**：

```
条件：挂有 ap_wavemark（开关开启）
swing = sq_GetVectorData(substate, 0)                     // 普攻第几段（0/1/2 = 三连击）
bonusRate = sq_GetBonusRateWithPassive(47, 8, 3+swing)    // col3/4/5 = 剑气三段倍率
写包：dword 47（技能id）/ bool（sq_GetSkillLevel(92)>0，92 疑强化被动）/ dword swing / dword bonusRate
sq_SendCreatePassiveObjectPacket(obj, 24370, 0, 0, 0, 0, dir)
  —— 24370 = 共享打击 PO（L20：share_obj\swordman\*.nut 六回调，按首 dword=47 分流）
  —— 即"普攻每段挥出一道小剑气"，伤害为技能 47 的等级列、判定体走共享 PO
```

### 2.3 上挑联动与 mod 层辨析

explain 提到"使用普通攻击和[上挑]时会产生波动剑气"。白名单实测：普攻段触发在 attack.nut（mod 重写版，C3）；上挑触发未在白名单命中（上挑为引擎内置 011 系，其钩子应在引擎或 mod 全局——未考证）。**原版幸存数据**：`wavemark_jianqi\` 目录的 `onAfterSetState_ATTACKSWORDMAN`（按 attackIndex 0/1/2 写包 24383，倍率 col3/4/5）与官方形态吻合，但 24383 在 passiveobject.lst 中指向 `changqing_atswordman\ChangQing_751675335_ATSwordman.obj`（lst:27367，mod 换皮 PO）且 load_state 无该 nut 注册——判定为**未被加载的幸存/备用实现**，走读采信其行为语义（三段剑气 × col3/4/5），实现链以 attack.nut mod 版（24370）为准。

`po_wavemark_jianqi.nut`（幸存版弹体逻辑）：按包内 id 0/1/2 设攻击倍率与 customAnimation 0/1；proc 每帧沿朝向 +2px；播完自毁——**弹体行为 = 短距穿透波**，与我们 BulletDefinition 语义一一对应。

### 2.4 动画关键帧表（全部实测）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img | 备注 |
|---|---|---|---|---|---|---|
| `character\swordman\effect\animation\wavemark\add.ani` | 12 | 880 ms | 无 | 无 | `Character/Swordman/Effect/WaveMark/start.img` + `wave.img` | 开启特效 |
| `wavemark\cast.ani` | 7 | 480 ms | 无 | 无 | wave.img | 施法波纹 |
| `wavemark\remove.ani` | 8 | 700 ms | 无 | 无 | destruction.img | 关闭特效 |
| `wavemark\addmark1-5.ani` | 3 | 160 ms | 无 | 无 | font.img | 加印（按当前印数选 1-5） |
| `wavemark\removemark1-5.ani` | 3 | 300 ms | 无 | 无 | font.img | 印消失 |
| `passiveobject\...\animation\addwave\addwave1/2/3.ani`（剑气弹体） | 4/4/6 | 未逐帧加总 | 无 | 无 | `character/swordman/effect/addwave/addwave.img`（light 变体：`addwave\light\addwave.img`） | 幸存版三段剑气视觉 |
| `passiveobject\...\wavemarkattack.obj` 引用的 `Animation/AddWave/light/addwave1-3.ani` | 同上 | — | — | — | 同上 | 光属性变体（wavemarkattack*.obj 为孤儿数据，见 §3） |
| 施法姿态 | summon1/2.ani 共用 | 150/600 ms | — | — | sm_body 帧 75-89 | 开关共用增益姿态 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | WaveMark.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\WaveMark.skl` | ✅ 实测 | 数值（7 列全解） |
| 注册行 | load_state:117（共用 throw 态） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 13 无技能绑定 |
| 开关 nut | swordman_throw.nut case 47 | `…\pvf\sqr\character\swordman\swordman_throw.nut` | ✅ 实测 | toggle + bonus 存 var |
| appendage | ap_wavemark.nut ×2 | `…\sqr\character\swordman\appendage\` + `attack\` | ✅ 实测 | 持印容器/空骨架 |
| 普攻联动 | attack.nut（mod） | `…\sqr\character\swordman\attack\attack.nut` 110/210 行 | ✅ 实测 | 三段剑气写包 24370 |
| 共享 PO 回调 | share_obj\swordman\*.nut | `…\sqr\common_object\share_obj\swordman\` | ✅（L20 已证） | 24370 按首 dword 分流 |
| 幸存实现 | wavemark_jianqi.nut + po_wavemark_jianqi.nut | `…\sqr\character\swordman\wavemark_jianqi\` | ✅ 实测（未注册） | 原版风格剑气（语义采信） |
| 孤儿 PO 定义 | wavemarkattack.obj / wavemarkattack_light.obj | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | lst 无注册；light 版 atk 引用 `AttackInfo/WaveMarkAttack_light.atk` **不存在**（attackinfo ls 实测）——死引用，佐证孤儿判定 |
| 刻印加速点 | swordman_common.nut / wave.nut / indaramang.nut | `…\sqr\character\swordman\` | ✅ 实测 | sqx_WaveMarkPush（定义未考证） |
| .chr / 角色 .ani | summon1/2 共用 | `…\pvf\character\swordman\` | ✅ 实测 | 增益姿态 |
| 特效 .ani | wavemark\*（13 个）+ addwave\*（6 个） | `…\pvf\character\swordman\effect\animation\wavemark\` + `…\passiveobject\character\swordman\animation\addwave\` | ✅ 实测 | §2.4 |
| .atk | — | 两侧 attackinfo 目录 | ⛔ 无（剑气伤害为写包倍率 + 共享 PO 自有表） | — |
| .als | — | 两侧 animation 目录 | ⛔ 无 | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法姿态 | 必需 | ✅ 已在库 |
| `Character/Swordman/Effect/WaveMark/start.img` | sprite_character_swordman_effect_wavemark.NPK | 开启特效 | 必需（视觉还原） | ❌ |
| `…/WaveMark/wave.img` | 同上 | 施法波纹/加印 | 必需 | ❌ |
| `…/WaveMark/destruction.img` | 同上 | 关闭特效 | 可选 | ❌ |
| `…/WaveMark/font.img` | 同上 | 印数计数视觉（1-5） | 可选 | ❌ |
| `character/swordman/effect/addwave/addwave.img` | sprite_character_swordman_effect_addwave.NPK | 剑气弹体 | 必需（剑气落地时） | ❌ |
| `character/swordman/effect/addwave/light/addwave.img` | 同上 | 光属性剑气变体 | 可选 | ❌ |

缺失 img：必需级 4 张（3 NPK）。

## 5. 实现方案草案

**⛔ 暂缓（资源转化链断裂）**——本技能是"**技能资源标记系统**"的样本用例，缺口拆解：

1. **刻印计数存储**：✅ 其实可行——LSNumericComponent 支持任意 int 键 Add/Get 且进快照（SkillContext.AddNumeric 已暴露），加一个"波动印数"自定义键即可回滚安全地存 0-5。这一层**不是**缺口。
2. **刻印驱动增伤（核心价值）**：⛔ 属性数值无伤害消费链（R1-A4）——col6 全局 10%~49% 增伤与"每印智力叠乘"全部卡死；MeleeHit 只读固定 HitReaction.Damage。
3. **开关判定**：⛔ 自身 Buff 查询门面（R4-A18 开关技通用）——再按关/10s 防误触都要"查询我是否挂着 wavemark"。
4. **普攻联动剑气**：⛔ 跨技能状态查询（NormalAttack 的 OnCast/OnUpdate 需感知 wavemark 开关与印数）+ 剑气伤害随 col3/4/5 缩放（同消费链）。若剑气改**固定伤害**则弹体本身零缺口（BulletDefinition 直译 po_wavemark_jianqi：短距穿透 + 三段倍率固化为三档 demo 值），仅剩触发侧查询缺口。
5. **施放速度**：⛔ 无系统（与暴走攻速同族）。
6. **维持 MP**：延后档（无 MP），跳过。

**若未来拆分落地的分层草案**（资源系统立项时的参考结构）：

- `WaveMarkSkill : SkillLogic`——CD 10000、TotalTimeMs=600；OnCast 查询自身 Buff（等门面）→ 已开则关 / 未开则 `AddBuffToSelf(BuffIds.WaveMarkOn)` + 首印计数 +1（`AddNumeric(自身, 波动印键, 1)`）；OnUpdate 里 10s 定时 +1（封顶 5）、每印 30s 到期 -1（**Buff 定时 + 计数衰减并存——现有 BuffDefinition 无"计数资源"形态，需 SkillLogic 自持定时，SubState 不够用，建议 LSCast 加常驻 tick 或用隐藏 Area 驱动**——实现期决策）。
- `WaveMarkOnBuff : BuffDefinition`——TotalTimeMs=0（手动开关移除）；TickActions 留空（等消费链）。
- `WaveMarkJianqiBullet : BulletDefinition`——复制 NormalWaveBullet 改小（Speed 高/射程短/穿透），ViewAnimId=addwave1-3；普攻侧在 NormalAttackSkill OnUpdate 按段触发（需 buff 查询门面）。
- 刻印加速点：WaveSword/IceWave/FireWave 的 OnCast 各 +1 印（跨技能写同一数值键，无缺口）。
- 注册点：`SkillIds.WaveMark=28`、`BuffIds.WaveMarkOn=15`、`BulletIds.WaveMarkJianqi=6`、AnimIds 134-146（cast/add/remove + addmark/removemark×5，demo 可只做 134-136 三个）。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 10000 ms | 10000 直用 |
| 开关防误触 | 10 s（appendage timer） | 10 s |
| 印生成间隔 | 10 s（col2 恒定） | 10 s |
| 印寿命 | 30 s（static[1]） | 30 s |
| 印上限 | 5（static[2]） | 5 |
| 每印智力 | 14→295（col0） | 等数值链 |
| 全局增伤 | 10%→49%（col6） | 等数值链（⛔ 主因） |
| 剑气三段倍率 | 62/124/186%（col3/4/5） | 固定伤害 15/25/35（若拆分落地） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `WaveMark.skl` | `.skl` 无子命令（7 列 + static 4 值） | 手抄可行；`skl` 子命令同前议 |
| wavemark\*.ani ×13、addwave\*.ani ×6 | 常规节（IMAGE/DELAY/GRAPHIC EFFECT/RGBA 均在已支持清单） | 无缺口，`ani` 子命令直译 |
| wavemarkattack*.obj | `.obj` 无子命令 | 孤儿数据不翻译；剑气弹走 BulletDefinition 手工配置 |

结论：动画资源全部可被现有 ani 子命令翻译；实质缺口 `.skl`/`.obj`（重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 印数→智力/施放速度叠乘 | **缺失档：属性数值消费链**（R1-A4） | ⛔ 主因之一 |
| 开启即全局增伤 10~49% | 同上（伤害端零消费） | ⛔ 主因之二——**印攒了没处花** |
| 开关（再按关闭）+ 10s 防误触 | **缺失档：自身 Buff 查询门面**（R4-A18） | 固定时长自动关（丢手感） |
| 普攻/上挑三段剑气 | **缺失档：跨技能状态查询**（NormalAttack 感知 wavemark）+ 倍率缩放（消费链） | 剑气改固定伤害独立弹；触发等查询门面 |
| 印的定时生成/到期 | 无系统级缺口（数值键 + SkillLogic 定时可表达），但 BuffDefinition 无计数资源形态 | 实现期决策：技能自持定时 vs 隐藏驱动 |
| 维持 MP 1~8/s | 延后档（无 MP） | 跳过 |
| 施放速度 +1.2~24.6% | 缺失档：施放速度系统（读条都没有） | 跳过 |
| mod 层（attack.nut 剑气/24370 分流） | mod 内容 | 按 24370 语义等价重写，不搬代码 |

## 8. 存疑与缺口上报

**未考证项**
1. `sqx_WaveMarkPush` 函数定义（白名单外 mod 全局；按调用面推定"立刻加 1 印"）。
2. static[0]=500 与 static[3]=10000 的精确语义（后者与 col2 同值疑冗余）。
3. 上挑的剑气触发点（引擎/上挑共用态——白名单未命中）。
4. 写包第二参 bool 的消费端（共享 PO setcustomdata case 47 分支——share_obj 六回调未逐行展开，技能 92 的身份未考证，疑"强化上挑/强化普攻"系被动）。
5. "每印智力叠乘"的引擎公式（印数 × col0 还是前缀和——无脚本可证）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **技能资源标记系统**（本技能为首个完整样本）：刻印/斗气/杀气值这类"技能攒资源、其他技能花资源"的通用形态——需要 ①可回滚的计数存储（NumericType 自定义键可满足）②**资源→伤害的消费管线**（与属性消费链合并立项）③跨技能读写同一资源的约定（OnCast 写键无缺口）。建议 00-总览 单列"资源标记系统"条目，以本技能 + 未来气力系技能为验收用例。
2. **自身 Buff 查询门面**第 4 实证（R4-A18 首记，开关技通用——本技能是纯开关形态代表）。
3. **普攻段位感知**：NormalAttack 连段的 attackIndex 0/1/2（DNF）在我们侧 = SubState 单值——"按段位差异化触发"（剑气三段不同倍率）需要连段技能暴露段位或段位表驱动。记档供连段体系（里鬼/月光）统一设计。

**翻译工具缺口**：`.skl`/`.obj` 子命令（重复印证）。

**给下轮的经验**：**toggle 型增益技（波动刻印 47/卡洛 82/鬼影步 18）的开关逻辑全在 `swordman_throw.nut` 的 onAfterSetState case 表**——查 toggle 技先读这个文件的 case 列表，一行一个技能，别再全树搜。印/资源类视觉（font.img 计数 1-5）是按"当前印数选 addmark1-5 播放"的——计数视觉=表驱动选动画，值得记为资源系统的通用视觉范式。
