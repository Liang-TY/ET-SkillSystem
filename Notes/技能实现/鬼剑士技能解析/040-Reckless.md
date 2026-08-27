# 暴走（Reckless）

> 技能ID 40 | 级别 B（形态/自身增益 buff） | 可实现性 ⛔（力量/移速/攻速/抗性四路数值全部无消费链 + 受击触发钩子缺失；buff 壳与时长可挂但效果全空转） | 分析日期 2026-08-22 | 批次 B3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 暴走 | `skill\Swordman\Reckless.skl` [name] |
| 英文名 | Reckless（取 skl 文件名；[name2]="暴走" 是中文别名，L1） | 同上 [name2] 实测 |
| 职业 | 狂战士（[skill fitness growtype]=**3**，L17 实测映射） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 30（狂战/剑影档 20：growtype maximum level `0 0 0 20 0 20`） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 2） | 同上 [type]/[skill class] |
| 指令 | →→ + Space（BUFF 键） | 同上 [command] / [command key explain] |
| CD | 5000 ms（固定） | 同上 [cool time] |
| MP | 30 → 308（Lv1→Lv30） | 同上 [consume MP] |
| 读条 | casting time 500 ms | 同上 [casting time] |
| 特殊消耗 | 无（狂战系同族"血之狂暴"另有 HP 维持，本技能自身不扣血） | 同上（未见 HP 消耗节） |
| 屏震 | [shake screen] `3 500`（施放时自身屏震） | 同上 |
| static data | `5`（static[0]=5 → **被攻击时力量叠加上限 5 层**，与 explain"最多重叠5次"互证） | 同上 [static data] |
| 一句话效果 | 自身进入暴走：力量/移速/攻速/异常抗性提升，智力/物防/魔防下降，持续 600 秒；被攻击时追加力量（可叠 5 层，每层 15 秒） | 同上 [explain] |

**level property 模板解码（14 列，L21 向量法全解，dungeon 表 41 行，Lv1→表末）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 持续时间 | (-1, 0, ×0.001) | col0 = **600 s 恒定**（pvp 20s） |
| 增加力量 | (-1, 1, ×1.0) | col1 = **55 → 1279** |
| 增加移动速度 | (-1, 5, ×0.1) | col5 = 8→308 → **0.8% → 30.8%** |
| 增加攻击速度 | (-1, 6, ×0.1) | col6 = 8→308 → **0.8% → 30.8%**（与移速同值同列宽） |
| 增加异常状态抗性 | (-1, 7, ×1.0) | col7 = **2 → 74** |
| 减少智力 | (-1, 4, ×1.0) | col4 = **68 → 218** |
| 减少物防 | (-1, 2, ×1.0) | col2 = **50（恒定；系数 1.0 → 推断 50% 减防，pvp 同 50）** |
| 减少魔防 | (-1, 3, ×1.0) | col3 = **50 恒定**（pvp 33） |
| 被攻击时增加力量 | (-1, 12, ×1.0) | col12 = **2 → 98** |
| 被攻击时力量的持续时间 | (-1, 13, ×0.001) | col13 = 15000 → **15 s 恒定** |

列 8/9/10/11（Lv1 行：200/27/5000/48）无模板行对应，**语义未考证**（疑引擎内部：buff 图标/特效参数/叠层间隔之类）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无 pushState、无 nut、无 appendage 脚本**（三方实测）：

- `swordman_load_state.nut` grep `reckless` 无命中（状态号 40 属 IllusionSlash 幻影剑舞，L2 铁证之一）；
- `sqr\character\swordman\` 57 项目录 ls 无 reckless 目录；appendage 子目录 7 个 ap_*.nut 无 reckless；
- 剑影（atswordman）/剑鬼（jg_swordman）load_state 亦无同名注册（F3 ③ 参照源空）。

→ **F3 引擎内置**（老一代 buff 技能常态）：施法流程/属性加减/受击叠层全在客户端引擎，pvf 只留 .skl 数据。施放瞬间进 STATE_THROW（load_state:117 注册 swordman_throw.nut 状态 13，共用 buff/投掷态），但 throw.nut 的 case 分支只有 23/47/82/18/222——**40 连 throw 分支都没有**，纯引擎。

### 2.2 引擎内置行为重建

```
施放（读条 500ms，→→+Space 或技能栏）：
  播增益姿态（共用 [buff motion] = Summon2.ani 600ms / [throw motion 2-1/2-2]，
    sm_body 帧 75-89——与卡赞/普戾蒙/波动刻印全套共用，025 已实测）
  屏震 3 级 500ms（延后特性）
  挂自身 appendage（实体在 pvf\appendage 大树，无确切子路径未检索——未考证）：
    时长 col0 = 600s
    属性面：力量 +col1 / 移速 +col5×0.1% / 攻速 +col6×0.1% / 异常抗性 +col7
            智力 -col4 / 物防 -col2(%) / 魔防 -col3(%)
    （全部由引擎属性系统消费——无脚本可走读）
受击（buff 存续期间）：
  力量追加 col12，持续 col13=15s，最多叠 static[0]=5 层（受击触发由引擎受击管线驱动）
视觉：
  暴走红光/变身氛围——白名单内无 reckless 特效 ani（character\effect\animation ls 实测）
  → 引擎硬编码或 appendage drawAppend（与 ap_sacrifice 的红色 LINEARDODGE 同法），未考证
```

### 2.3 被动对象 / appendage

无被动对象。增益 appendage 实体文件未定位（`pvf\appendage\` 大树，C2 规则不检索）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\summon1.ani` / `summon2.ani`（共用增益姿态） | 3 / 12 | 150 / 600 ms | summon2 F9=65534 | 无 | 025-Khazan 已实测；sm_body 帧 75-89 已入库 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | Reckless.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\Reckless.skl` | ✅ 实测 | 数值（14 列全解） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无（grep 实测） | 引擎内置（F3） |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（ls 57 项） | ⛔ 无 | 逻辑在引擎 |
| appendage nut | —（不存在） | `…\pvf\sqr\character\swordman\appendage\`（7 文件 ls） | ⛔ 无 | 属性 buff 载体在 pvf\appendage 大树，未考证 |
| .chr 条目 | —（共用 [buff motion]/[throw motion 2-1/2-2]） | `…\pvf\character\swordman\swordman.chr` 934-950 行 | ✅ 实测（025） | Summon1/2.ani |
| 角色 .ani | summon1/2.ani（共用） | `…\pvf\character\swordman\animation\` | ✅ 实测 | 增益姿态 |
| 角色 .atk | — | `…\pvf\character\swordman\attackinfo\` | ⛔ 无（buff 无攻击） | — |
| 特效 .ani | —（无 reckless 特效） | `…\pvf\character\swordman\effect\animation\`（ls 实测） | ⛔ 无 | 暴走视觉载体未考证（引擎/drawAppend） |
| 装备层 | —（未查；共用姿态无专属图层） | `…\pvf\equipment\character\swordman\avatar\` | 未查 | 施法动作为共用姿态 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img（帧 75-89 增益姿态） | sprite_character_swordman_equipment_avatar_skin.NPK | 施法动作 | 必需 | ✅ 已在库 |
| （暴走 buff 专属视觉 img 未定位——引擎视觉或 appendage 大树） | — | buff 氛围 | 存疑 | — |

缺失 img：**必需级 0 张**（共用姿态已入库）；专属 buff 视觉未考证。

## 5. 实现方案草案

**⛔ 暂缓（核心增益全空转）**——判定依据（均框架层代码核实）：

1. **属性数值无伤害消费链**（缺失档，R1-A4 起已 3+ 实证）：力量 +55~1279 无 NumericType 键位、MeleeHit 只读固定 HitReaction.Damage——挂了 buff 伤害分毫不变；智力/物防/魔防减益同卡死。
2. **移动速度数值零消费**（R2-A7 实证）：NumericType.Speed 五层公式在库，但移动计算硬编码 6 单位/s——移速 +0.8%~30.8% 空转。
3. **攻击速度系统缺失**：无攻速数值键、无消费管线。值得记档的正向线索：**LSAnimComponent.Speed 字段存在**（LSAnimPlayUtil 每次 Play 重置为 1）——攻速消费有天然落点（动画播放速率），缺的只是"攻速数值键 → anim.Speed"的管线，属消费链缺口而非从零建设。
4. **异常状态抗性系统缺失**（041-Bremen 首撞）：抗性 +2~74 无处安放。
5. **受击触发钩子缺失**（R3-A15 受击伤害管线钩子）：被攻击时叠力量需要"受击事件 → buff 叠层"注入点，现无。

**可先行落地的部分（占位壳）**：`RecklessBuff : BuffDefinition`（TotalTimeMs=600000，AddActions/TickActions 留空）+ `RecklessSkill : SkillLogic`（CD 5000、TotalTimeMs=600、OnCast PlayAnim+AddBuffToSelf）——结构与视觉零缺口，但无任何战斗效果，**不建议单独落地**（与 Khazan/远古记忆/不屈意志同队列，等属性消费链立项一次解锁全家）。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 5000 ms | 5000 直用 |
| buff 时长 | 600 s | 600 s（demo 可缩 60 s） |
| 施法姿态 | Summon2 600ms + 读条 500ms | 600ms（读条跳过） |
| 力量/移速/攻速/抗性 | col1/col5/col6/col7 | 等数值链（键位 + 消费） |
| 受击叠层 | col12/15s/5 层 | 等受击钩子 |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `Reckless.skl` | `.skl` 无子命令（14 列 level info + static data 1 值） | 本技能手抄 10 组显示值可行；`skl` 子命令同前议（重复印证） |

结论：**本技能无 .ani/.als 新增翻译需求**（共用姿态 025 已走通）；缺口仅 `.skl` 一条（重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 力量+55~1279（核心收益） | **缺失档：属性数值消费链**（R1-A4） | ⛔ 主因；等数值链立项 |
| 移速+0.8~30.8% | 缺失档：Speed 零消费（R2-A7） | 同上（数值键已有，补消费端即可，性价比高） |
| 攻速+0.8~30.8% | 缺失档：攻速系统（anim.Speed 落点已在，见 §5-3） | 同上 |
| 异常抗性+2~74 | 缺失档：抗性系统（041 首撞） | 与异常 Buff 体系合并立项 |
| 智力/物防/魔防减（代价面） | 同消费链 | 同上（减益与增益共用一条链） |
| 被攻击叠力量（5 层×15s） | **缺失档：受击伤害管线钩子**（R3-A15） | 先砍掉叠层，只做静态 buff |
| 600s 长驻 | 无（BuffDefinition TotalTimeMs 直用） | 直译 |
| 读条 500ms / 屏震 / MP | 延后档 | 跳过 |
| 暴走红光视觉 | 视觉载体未考证 + Buff 视觉挂接缺失（R1-A5） | 先无视觉或贴特效 ani |

## 8. 存疑与缺口上报

**未考证项**
1. 列 8/9/10/11（200/27/5000/48）语义（无模板行、无 nut 消费方）。
2. 物防/魔防减的"50"读法（系数 1.0 直读=50 点 or 50%——模板显示 `<float1>%%`，倾向 50%，标推断）。
3. 暴走 buff 专属视觉的载体（引擎硬编码 or pvf\appendage 大树内 .apd 的 drawAppend）。
4. 增益 appendage 实体文件（大树无路径）。

**缺口上报（并入主循环汇总）**
1. **属性消费链家族 +1 实证**（狂战形态系首例）：与 Khazan/远古记忆/不屈意志/流心:狂同队列——建议"数值键位 + 消费端"立项时把暴走列为验收用例（四路数值一网打尽）。
2. **攻速消费的天然落点**（新线索）：LSAnimComponent.Speed 已存在，攻速管线=数值键→anim.Speed 映射，比移速（移动计算硬编码）改造成本低——记档供立项排序参考。
3. **受击触发钩子**（R3-A15 已记档，本技能为"受击→自身增益叠层"形态第 2 用例，与"HP 下限钳制"形态互补）。

**翻译工具缺口**：`.skl` 子命令（重复印证）。

**给下轮的经验**：狂战形态 buff 族（暴走/血之狂暴 51）与阿修罗 toggle 族（波动刻印/杀意波动）**全部共用增益姿态 Summon1/2 + STATE_THROW(13)**——但暴走连 throw 分支都没有（纯引擎），toggle 族在 swordman_throw.nut 有 case。判定捷径：先 grep throw.nut，无 case 才判纯引擎。
