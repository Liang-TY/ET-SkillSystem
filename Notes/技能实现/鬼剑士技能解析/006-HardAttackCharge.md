# 噬灵鬼斩（HardAttackCharge）

> 技能ID 6 | 级别 C（被动·主动技强化型——为[鬼斩]追加蓄力形态，非独立主动技） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 噬灵鬼斩 | `skill\Swordman\HardAttackCharge.skl [name]` |
| 英文名 | HardAttackCharge（取 skl 文件名；[name2]="鬼斩 Drive" 混合名） | 同上 [name2] 实测 |
| 职业 | 鬼泣（[skill fitness growtype] 2） | 同上 |
| 学习等级 | 25（前置：鬼斩 Lv1，pre required skill `5 1`） | 同上 [required level] / [pre required skill] |
| 最高等级 | 50 | 同上 [maximum level] |
| 类型 | passive（skill class 3）——**预判纠偏说明**：名为"鬼斩蓄力"易误判主动技，实为改造[鬼斩](ID 5)施放方式的被动强化技 | 同上 [type] |
| 指令 | Z（按住稍停后松开）——作用于鬼斩的原指令 | 同上 [command key explain] |
| CD / MP | 无（自身无消耗，效果并入鬼斩） | 同上（无相应节） |
| static data | dungeon `0 50 750 -1200 0 300`（pvp 仅第 2 位 50→500 差异）——static[1]=50 → 蓄气时间上限 ×0.001 显示（见下存疑）；750/-1200/300 疑为前冲/位移参数，未考证 | 同上 [static data] |
| 一句话效果 | 按住蓄气使[鬼斩]攻击力大增；蓄满时向前推进斩击并喷射鬼神冲波（卡赞/萨亚/普戾蒙 = 最后召唤的鬼神，未召唤过默认卡赞；不同鬼神附加不同状态） | 同上 [explain] |

**level property（13 列全解，Lv1 → Lv50 首末值）**——14 占位符 ↔ 14 向量一一对位
（4 个 ×0.001 向量恰好对应 4 个"秒"占位符，交叉自洽；L21 解码法）：

| 列 | 语义（模板直读） | 向量 | Lv1 | Lv50 |
|---|---|---|---|---|
| static[1] | 蓄气时间上限（秒） | (1,1,0.001) | 50×0.001 = **0.05s（存疑 §8）** | 同 |
| col0 | 最大蓄气攻击力增加比率 % | (-1,0,1.0) | 20% | 295% |
| col1 | 卡赞魔法攻击力 | (-1,1,1.0) | 772 | 6275 |
| col2 | 萨亚魔法攻击力 | (-1,2,1.0) | 811 | 6588 |
| col3 | 普戾蒙魔法攻击力 | (-1,3,1.0) | 869 | 7059 |
| col4 | 卡赞：增加力量 | (-1,4,1.0) | 19 | 575 |
| col5 | 卡赞：增加智力 | (-1,5,1.0) | 19 | 575 |
| col6 | 卡赞：效果持续时间 ms | (-1,6,0.001) | 10000（10s） | 46316 |
| col7 | 萨亚：冰冻机率 % | (-1,7,1.0) | 10% | 264% |
| col8 | 萨亚：冰冻 Lv | (-1,8,1.0) | 27 | 234 |
| col9 | 萨亚：冰冻持续时间 ms | (-1,9,0.001) | 1000 | 6447 |
| col10 | 普戾蒙：睡眠机率 % | (-1,10,1.0) | 10% | 264% |
| col11 | 普戾蒙：睡眠 Lv | (-1,11,1.0) | 27 | 234 |
| col12 | 普戾蒙：睡眠持续时间 ms | (-1,12,0.001) | 3000 | 19342 |

pvp 段数值全面缩水（如 Lv1 col0=7%、col1-3=158/163/172）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**无 pushState、无同名 nut（引擎内置，F3 型）**——grep `hardattackcharge` 于
`swordman_load_state.nut`、`sqr\character\swordman\` 全树、`passive_skill_swordman.nut` 均无命中
（唯一命中为 `swordman_header.nut:187` 的动画常量）。蓄力状态机在引擎的鬼斩（hardattack）状态内：
学得技能 6 后，鬼斩按键变为"按住蓄气"，松开或蓄满时进入强化演出。引擎行为证据链：
- `swordman_header.nut:187`：`CUSTOM_ANI_HARDATTACKCHARGEAFTER <- 17`（蓄满斩击动画槽位）；
- `load_state:149-150` 注册的 `jg_swordman\hardattack\hardattack.nut` 是**剑影 BladeSpirit 变体**
  （读技能 SKILL_BLADESPIRIT 数据），非本技能逻辑——勿混；
- 攻击信息/PO/特效资源全部在本技能名下的目录树内（§2.2，即 005-HardAttack.md §2.3 预告的清单）。

### 2.2 机制归纳（引擎行为 + 资源树重建）

```
按住 Z → 蓄气（蓄气中反馈特效 hardattackoncharge1/2.ani，蓄满切 hardattackfullcharge1/2.ani）
松开：
  未蓄满 → 普通鬼斩
  蓄满 → 短硬直后向前推进（static[2]=750 / -1200 疑位移参数，未考证）
        → 向前推进斩击（hardattackchargeafter.ani，11 帧 650ms，etc 槽 17）
        → 喷射鬼神冲波（PO 24370 族：hardattackchargeafter.obj 多相位——
           卡赞/萨亚/普戾蒙 各 down/up 相位，命中挂对应状态）：
             卡赞冲波：力量/智力 +col4/col5，持续 col6（作用对象未考证，§8）
             萨亚冲波：冰冻 col7% / col9 ms（冰冻 Lv=col8，状态等级系统）
             普戾蒙冲波：睡眠 col10% / col12 ms
        → 选用哪只鬼神 = 最后召唤者（萨亚36/卡赞25/普戾蒙41 在场记忆），未召唤默认卡赞
```

### 2.3 被动对象（资源树实测，全部存在）

- **PO 定义** `hardattackchargeafter.obj`（`passiveobject\character\swordman\`，L9 多相位结构）；
- **PO 动画** `passiveobject\character\swordman\animation\hardattackcharge\`：
  khazan-down/up、saya-down/up、bremen-down/up（各带 [pvp] 变体）+ bladepantom 系列 3 ani（95 级 mod 追加变体）；
- **PO 命中** `passiveobject\character\swordman\attackinfo\`：hardattackchargeafterkhazan/saya/bremen.atk
  + hardattackchargeafterphantom.atk（bladepantom 变体）；
- **角色侧**：`hardattackchargeafter.ani`（etc 槽 17）+ `HardAttackChargeAfter.atk`（etc atk 槽 21，005 文档实测）；
- **特效** `character\swordman\effect\animation\`：hardattackoncharge1/2.ani（蓄气中）、
  hardattackfullcharge1/2.ani（蓄满）、hardattackchargeafterdust.ani（收尾尘土）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 引用 img | 备注 |
|---|---|---|---|
| hardattackchargeafter.ani（角色推进斩） | 11（650ms，005 文档实测） | sm_body%04d.img | 已入库图集 |
| hardattackoncharge1/2.ani（蓄气反馈） | — | HardAttackOnCharge.img | 循环反馈层 |
| hardattackfullcharge1/2.ani（蓄满反馈） | 5（实测 FRAME MAX） | HardAttackFullCharge.img | 蓄满提示层 |
| PO khazan/saya/bremen-down/up.ani | — | HardAttackCharge\ghost-/saya-/bremen-*.img | 冲波本体 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | HardAttackCharge.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\HardAttackCharge.skl` | ✅（232 行） | 13 列数值全解 |
| lst 条目 | swordmanskill.lst 179-180 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 6 → 本 skl |
| 注册行 / 主 nut | —（引擎内置蓄力） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无注册 | 蓄力状态机在引擎 |
| 常量 | swordman_header.nut:187 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_HARDATTACKCHARGEAFTER=17 |
| 角色 .ani / .atk | hardattackchargeafter.ani / HardAttackChargeAfter.atk | `…\pvf\character\swordman\animation\` / `attackinfo\` | ✅ 实测 | 推进斩（槽 17 / 槽 21） |
| PO 定义 / 动画 / atk | hardattackchargeafter.obj + animation\hardattackcharge\ + 4 atk | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | 鬼神冲波多相位 |
| 特效 | hardattack{oncharge,fullcharge}1/2.ani + chargeafterdust.ani | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | 蓄气反馈三层 |
| 装备层 | —（avatar 内无 hardattackcharge 专属层，L16 惯例 sm_body 共用） | `…\pvf\equipment\character\swordman\avatar\` | ⛔ 无专属 | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| HardAttackCharge\ghost-down / ghost-up / saya-down / saya-up / bremen-down / bremen-up / dust.img（7 张） | sprite_character_swordman_effect_hardattackcharge.NPK | 鬼神冲波本体+尘土 | 必需（PO 视觉即判定体演出） | ❌ 未入库 |
| HardAttackFullCharge.img | sprite_character_swordman_effect_hardattackfullcharge.NPK | 蓄满反馈 | 可选（瞬发简化可砍） | ❌ 未入库 |
| HardAttackOnCharge.img | sprite_character_swordman_effect_hardattackoncharge.NPK | 蓄气中反馈 | 可选（同上） | ❌ 未入库 |
| HardAttack\lv95\BladePantom / BladePantomSlash / BladePantomSmoke.img（3 张） | sprite_character_swordman_effect_hardattack_lv95.NPK | 95 级 mod 追加幻影变体 | 可选（mod 内容，demo 不做） | ❌ 未入库 |
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色推进斩 | 必需（共享） | ✅ 已入库 |

缺失 img：**必需 7 / 可选 5**（AnimRes 目录 ls 实测均未入库）。

## 5. 实现方案草案（蓄力按 L21 共性简化为瞬发满蓄力版）

### 内容件清单（SkillIds 从 34 起顺延，L18 记账）

1. **`DotNet~/Skills/GhostSlashDriveSkill.cs : SkillLogic`**（独立成技，非挂到普攻——我们无技能改造系统，
   简化为"按 U 直发满蓄力版"，同 ReleaseWaveSkill 位移范式）：
   - `CooldownMs = 8000`（demo 建议值；DNF 侧无独立 CD，随鬼斩）；`TotalTimeMs = 650`（chargeafter.ani 11 帧）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanGhostSlashDrive)`；`ctx.CreateBullet(BulletIds.GhostWave)`（鬼神冲波弹体，
     NormalWaveBullet 范式改穿透/射程 750→我方 7.5 单位）。
   - `OnUpdate` 帧推进斩：`ctx.MoveCasterForward(…)` + 移动锁（ReleaseWaveSkill 同构）。
   - `HitReaction`：Damage = 鬼斩伤害 ×（1 + col0 20%）+ 冲波独立伤害（col1 772，demo 固定档）；
     ProcBuffId/ProcChance = 冰冻 10%（萨亚档）/ 睡眠 10%（普戾蒙档）——**两形态二选一由 demo 参数定**，
     鬼神记忆简化掉（§7）。
2. **`DotNet~/Buffs/SleepBuff.cs : BuffDefinition`**：Sleep = ForbidMove 开/关，StunBuff 同构改时长
   （BuffIds.Sleep = 18）。冰冻已有（Freeze=4）直用。
3. **数值表**：推进斩 650ms；冰冻 1000ms/10%；睡眠 3000ms/10%；卡赞力量/智力 buff 部分砍掉（属性消费链缺失，§7）。

### 注册点清单

| 什么 | 增量 |
|---|---|
| SkillId | `GhostSlashDrive = 34`（+ ButtonToSkill 新键） |
| AnimId | `SwordmanGhostSlashDrive = 178`（hardattackchargeafter.ani 译 json，sm_body 已在库）+ PO 冲波动画 179-184（khazan/saya/bremen 各 up/down，需先提 NPK） |
| BuffId | `Sleep = 18` |
| BulletId | `GhostWave = 8` |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| HardAttackCharge.skl | `.skl` 无子命令（13 列×50 级 + static 6 槽） | 本文档已全解（§1 表），实现期直抄；`.skl` 子命令为全局已知缺口 |
| HardAttackChargeAfter.atk + PO 4 atk | `.atk` 无子命令 | 数值量小（5 文件）手抄；`.atk` 子命令全局已记档 |
| hardattackchargeafter.obj | `.obj` 无子命令 | 多相位 PO 按本文档 §2.3 直读动画/atk 组合即可，obj 仅作对位参考；全局已记档 |
| 各 .ani | 全常规节（FRAME/IMAGE/DELAY + [SHADOW]） | 现有 ani 子命令覆盖；[SHADOW] 已记档 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 按住蓄气（按住时长分档） | **按住蓄力输入缺失**（L21 已记共性：demo 统一瞬发） | 瞬发满蓄力版（本方案） |
| 鬼神冲波按"最后召唤的鬼神"分形态 | 鬼神召唤物在场记忆（R2-A6 幻鬼实体同族缺口） | demo 固定一档（建议萨亚：冰冻已支持） |
| 蓄气反馈特效（oncharge/fullcharge） | 无（资源在，纯视觉） | 瞬发版可砍，仅保留蓄满一闪 |
| 卡赞冲波力量/智力 buff（col4-6） | **属性数值无伤害消费链**（176 §8 缺口，第 6 实证） | 砍掉；demo 只保留伤害+冰冻/睡眠 |
| 冰冻/睡眠状态等级（col8/col11 Lv 值） | 状态等级系统缺失（090 鲜血之忆同撞） | 状态无等级，固定命中 |
| static[2..5]（750/-1200/300 疑位移参数） | 未考证 | 推进距离按 750px→7.5 单位试值 |

## 8. 存疑与缺口上报

**未考证项**
1. 蓄气时间上限显示值 0.05s（static[1]=50×0.001）——与 DNF 手感（约 0.5s）不符，疑量纲/系数另有解释；
   瞬发简化下不影响。
2. static data 6 槽中 0/750/-1200/0/300 的精确语义（推断为推进位移参数）。
3. 卡赞冲波"增加力量/智力"的作用对象（自身增益 or 敌方 debuff——DNF 官方语义疑为敌方减益，模板写"增加"存疑）。
4. 蓄力状态机的引擎侧分档（未蓄满松开是否也强化）——无脚本可考。

**新系统级缺口**：无新上报（蓄力输入、属性消费链、状态等级、召唤物记忆均已在档；
本技能是它们的组合消费方）。

**翻译工具缺口**：`.skl`/`.atk`/`.obj` 子命令（全局已知三项，本技能为 .obj 的新消费场景——
**多相位 PO + 独立 atk 命名**（hardattackchargeafterkhazan.atk 等）可供 .obj/.atk 子命令设计时作第 4 个对位样本）。
