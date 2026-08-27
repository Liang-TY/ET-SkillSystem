# 逆转反击（HitBack）

> 技能ID 7 | 级别 B（受击触发/反击） | 可实现性 ⛔（背击检测 + 受击触发窗口 + 受击-施法联动三重系统缺失；反击动作与命中参数本身完全可表达，降级为普通主动技则 ✅ 但技能身份丧失） | 分析日期 2026-08-22 | 批次 B3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 逆转反击 | `skill\Swordman\HitBack.skl` [name] |
| 英文名 | HitBack（取 skl 文件名；[name2]="Reverse attack from the backside"） | 同上 [name2] 实测 |
| 职业 | 剑魂（[skill fitness growtype]=**1**，L17；growtype maximum 六系均 20——各系都可学 20 级） | 同上 |
| 学习等级 | 30 | 同上 [required level] |
| 最高等级 | 50 | 同上 [maximum level] |
| 类型 | active（skill class 1） | 同上 [type]/[skill class] |
| 指令 | (被背击时) Z——[command] `{6=(SKILL)}` | 同上 [command] / [command key explain] |
| CD | **15000 → 7000 ms**（Lv1→Lv50，CD 随等级缩短；pvp 25000） | 同上 [cool time] |
| MP | 10 → 84 | 同上 [consume MP] |
| 特殊消耗 | 无（static data `0`） | 同上 [static data] |
| TP 强化 | [steel learning skill] `1 50 200 4`（强化技数据） | 同上 |
| 一句话效果 | 自身背后遭受攻击时可以反击；反击时不减免所受伤害 | 同上 [explain] |

**level property 模板解码（2 列，L21 向量法全解）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 物理攻击力 | (-1, 1, ×1.0) | col1 = **1260 → 6581 %**（Lv1→表末，反击伤害倍率） |

col0 = 500 恒定，无模板行对应——**语义未考证**（疑反击触发窗口 500ms 或反击无敌帧时长）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**引擎内置（F3 三方印证）**：

- load_state 无 pushState（grep hitback 无命中；状态号 7 属 BackStep 后跳，L2）；
- `sqr\character\swordman\` 57 项 ls 无 hitback 目录；白名单内 grep hitback 仅命中 `swordman_header.nut:172` 常量与 fatalblood 的无关文件名；
- atswordman/jg_swordman 两家参照 load_state 亦无同名注册（F3 ③ 空）。

资源链（引擎消费，常量对位实证）：

```
swordman_header.nut:172   CUSTOM_ANI_HITBACK <- 2
swordman.chr  [etc motion]      槽 2 = `Animation/HitBack.ani`   (975 行实测：Guard=0/HardAttack=1/HitBack=2)
swordman.chr  [etc attack info] 槽 1 = `AttackInfo/HitBack.atk`  (1295 行实测：HardAttack=0/HitBack=1)
```

### 2.2 引擎内置行为重建

```
受击（任意伤害来源命中自身）：
  引擎判定受击方向：攻击者位于自身背面（hit direction 判定，引擎内部）
  → 若背面受击：开启反击窗口（时长未考证——col0=500 疑即此值），显示可反击提示
窗口内按 Z（SKILL 键）：
  播 HitBack.ani（9 帧 850ms，无 SET FLAG、无攻击盒——判定与动画时序引擎内挂）
  引擎按 [etc attack info] 槽 1 施加 HitBack.atk 命中（武器判定盒引擎施加）：
    physic / damage reaction [down] 击倒 / push aside 600 / lift up 220 / attack direction [hit lift up]
    伤害 = col1 × 物理攻击力%（1260% → 6427%）
  反击期间不减免所受伤害（explain 明示——反击不是防御）
窗口外/超时：反击机会消失，回正常受击流程
```

**关键时序未考证点**：反击判定的激活帧（.ani 无 SET FLAG，疑帧 0 即判或引擎固定延迟）；反击窗口与受击硬直的关系（被打硬直中能否立即反击）。

### 2.3 被动对象 / appendage

无（反击判定直接由角色攻击信息承担，不创建 PO）。

### 2.4 动画关键帧表（实测）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 受击盒 | 备注 |
|---|---|---|---|---|---|---|
| `character\swordman\animation\hitback.ani` | 9（F0-8） | 850 ms（100/50×7/400） | **无** | **无**（每帧 damageBox，sm_body 模板） | 有 | 挥剑回身反击；末帧 400ms 收招 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | HitBack.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\HitBack.skl` | ✅ 实测 | 数值（2 列全解） |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 无（grep 实测） | 引擎内置（F3） |
| 主 nut | —（不存在） | `…\sqr\character\swordman\`（白名单 grep） | ⛔ 无 | 触发逻辑在引擎 |
| 常量对位 | swordman_header.nut:172 | `…\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_HITBACK=2 |
| .chr 条目 | [etc motion] 槽 2 / [etc attack info] 槽 1 | `…\pvf\character\swordman\swordman.chr` 975/1295 行 | ✅ 实测 | HitBack.ani / HitBack.atk |
| 角色 .ani | hitback.ani | `…\pvf\character\swordman\animation\hitback.ani` | ✅ 实测 | §2.4 |
| 角色 .atk | hitback.atk | `…\pvf\character\swordman\attackinfo\hitback.atk` | ✅ 实测 | 击倒反击参数 |
| .als | — | `…\character\swordman\animation\` | ⛔ 无 | — |
| 特效 .ani | — | `…\character\swordman\effect\animation\`（ls 无 hitback） | ⛔ 无 | 无专属特效 |
| 装备层 | hitback.ani ×76 | `…\pvf\equipment\character\swordman\avatar\{8 层}\*\` | ✅ 实测（find 计数 76） | 各 avatar 变体图层 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | 反击动作帧 | 必需 | ✅ 已在库 |

缺失 img：**0 张**（资源完备，随时可做）。

## 5. 实现方案草案

**⛔ 暂缓（触发链系统缺失）**——反击本体可表达、触发不可表达：

1. **受击方向判定缺失**：LSCombatComponent 只记录 HitstunTimer/HurtAnimId（代码实测），无"攻击者相对受击者方位"记录——"背后受击"无从判起。HitReaction 结算点（MeleeHitAction）也没有方向写入。
2. **受击触发窗口缺失**：受击后需要"可反击窗口 + 窗口内输入监听"——LSInputBufferComponent 的缓冲是按键沿触发，不是"事件授权后的条件输入"；受击-施法互斥（R1-A4 记档：我们永不打断/也无处挂钩子）是这个缺口的姊妹面。
3. **受击伤害管线钩子缺失**（R3-A15）：往受击流程注入"背击→开窗"逻辑无注入点。

**降级方案（若用户接受语义变更）**：普通主动反击技——`HitBackSkill : SkillLogic`（CD 7000、TotalTimeMs=850；OnCast PlayAnim(hitback) + ClearHitTargets；F2 附近 `ctx.SetAttackHitbox(后偏 0.6, 半尺寸 (0.8,0.3,0.6))`——回身斩判定在身后；HitReaction `{Damage=200, HitstunMs=800, KnockbackX=600, LaunchY=220}` 直译 .atk 的 down/push600/lift220）。**判定盒在身后**这一点 SetAttackHitbox 的 offset.x 为负即可表达（沿朝向镜像语义内），零新机制——但"被背击才能用"的身份没了，定位从反击变背袭技。此降级 ✅ 可行，需用户定夺。

**关键数值表**：

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 15000→7000（随等级降） | 7000（取满级值） |
| 动画时长 | 850 ms | TotalTimeMs=850 直用 |
| 伤害 | 1260%→6427% 物理 | 固定 200（MeleeHit 惯例） |
| 反应 | down / push 600 / lift 220 / hit lift up | Damage 200 / HitstunMs 800 / KnockbackX 600 / LaunchY 220 |
| 反击窗口 | col0=500（推断） | —（等触发系统） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `HitBack.skl` | `.skl` 无子命令（2 列） | 手抄 2 值可行；`skl` 子命令同前议 |
| `hitback.atk` | `.atk` 无子命令（down/push600/lift220/hit lift up） | 手抄 4 值；`atk` 子命令立项时 [attack direction]/[damage reaction] 字段设计输入 |
| `hitback.ani` | 常规节（damageBox） | `ani` 直译无缺口 |

结论：实质缺口 `.skl`/`.atk` 子命令（重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 背后受击才可触发（核心身份） | **缺失档：受击方向判定**（HitReaction/LSCombat 无方向数据） | ⛔ 主因；需受击管线加 direction 写入 |
| 受击后反击窗口 + Z 输入 | **缺失档：受击触发窗口**（受击-施法互斥姊妹面 + 无事件授权输入） | ⛔；与背击检测合并立项（"受击反击族"：逆转反击/受身/格挡反击共用） |
| 反击不减免伤害 | 无减免系统（我们本来也不减伤） | 语义天然成立 |
| 击倒反击（down/push600/lift220） | 无缺口（HitReaction + LSFlight 已落地） | 直译 |
| CD 随等级缩短 15s→7s | 延后档（等级缩放） | 固定 7000 |
| TP 强化（steel learning） | 强化技体系（E 批次） | 不在本文范围 |

## 8. 存疑与缺口上报

**未考证项**
1. col0=500 语义（反击窗口推断）。
2. 反击判定的激活时点（.ani 无 SET FLAG——帧 0 即判 or 引擎固定延迟，未考证）。
3. 受击硬直中按 Z 的时序关系（反击是否吃掉硬直）。
4. 装备层 76 变体与 8 个 avatar 层目录的精确对应（只查存在性）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **受击方向判定**（首撞）：HitReaction 结算时把"攻击者→受击者方位（前/后）"写入受击状态——是"背击触发族"（逆转反击、背后破招加成类）与"朝向减伤"类技能的共同前置。建议与受击伤害管线钩子（R3-A15）同一次受击管线改造中落地。
2. **受击触发窗口**（受击-施法互斥 R1-A4 的展开项）：受击事件授权一段"条件输入窗口"——逆转反击/受身类技能的触发形态，建议命名"受击反击族"一并设计。

**翻译工具缺口**：`.skl`/`.atk` 子命令（重复印证）；atk 子命令的 [attack direction] `hit lift up` 语义（击退方向变体）需纳入字段设计。

**给下轮的经验**：老技能 .ani **无 SET FLAG 无 ATTACK BOX** 且 .chr 挂在 [etc attack info] 的，判定时序是引擎按动画帧内部施加——重建时自行设计判定帧（本批 hitback 与 gorecross 的引擎弧光同规律）。反击/受身类技能判定前先看 007 这份——背击检测 + 触发窗口两个缺口是家族共用的，不用逐技能重复论证。
