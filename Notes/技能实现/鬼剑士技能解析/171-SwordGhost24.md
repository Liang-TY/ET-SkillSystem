# 鬼啸（SwordGhost24）

> 技能ID 171 | 级别 C（二觉被动） | 可实现性 ⛔（核心=增伤+僵直双数值列，消费���全卡死；CD/攻击范围可烘焙进宿主技能；幻鬼连携撞取消体系） | 分析日期 2026-08-22 | 批次 C5

**判定口径按"面板可表达 / 消费端卡死"分半**（批次要求）：本技能数值半（增伤 col0、僵直 col1）挂 NumericType 可表达但零消费；机制半（CD -1s、Y 轴范围 +20%）可烘焙；连携半（无施放动作接幻鬼技/被击中施放）撞技能取消体系与受击-施法互斥两个缺失档。

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼啸 | `skill\Swordman\SwordGhost\SwordGhost24.skl` [name] |
| 英文名 | SwordGhost24（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影·夜见罗刹（二觉被动；注册行 gate `growtype==0 \|\| growtype==5`，L17 映射 5=剑影；explain 通篇"夜见罗刹"） | `passive_skill_swordman.nut:49` + skl [explain] |
| 学习等级 | 75 | skl [required level] |
| 最高等级 | 50（[second growtype maximum level] 第 11 槽 = 30） | skl [maximum level] / [second growtype maximum level] |
| 类型 | 被动（[passive]，skill class 1） | skl [type] |
| 指令 | —（纯被动） | — |
| CD / MP | skl 里有 [cool time] 2000 / [consume MP] 1（被动技能的残留字段，无实际施放） | skl 实测 |
| 特殊消耗 | 无 | 同上 |
| 一句话效果 | 增加夜见罗刹基本攻击力与鬼斩/三段斩/转职技攻击力；攻击僵直提升；三段斩/幻鬼步 CD -1s；幻鬼步 Y 轴攻击范围 +20%；普攻/前冲攻击中无施放动作接幻鬼技；站立被击时可用幻鬼技与幻鬼步 | skl [explain] |

**level property（3 列全明，L21 向量法）**：

| # | 模板项 | 向量 | 解读 | 值 |
|---|---|---|---|---|
| 1 | 基本攻击力和技能攻击力增加 `<float1>`% | (-1, 0, 0.1) | level col0 × 0.1 | 10%（Lv1）→ 108%（Lv50，col0=1080）→ 表末 1480=148%（表实为 **70 行**，超出 max level 50，疑 mod 扩表） |
| 2 | 攻击敌人提高的僵直 `<int>` | (-1, 1, 0.1) | level col1 × 0.1 | 150 → 885（Lv50，col1=8850）→ 表末 1185 |
| 3 | [鬼步]Y 轴攻击范围增加率 `<float1>`% | (2, 2, 0.1) | **static[2]** × 0.1 | 200 × 0.1 = **20%（恒定）** |

[static data] `1 0 200`：static[2]=200 已由向量 3 消费；static[0]=1 / static[1]=0 无脚本消费者——结合 explain"三段斩 : 冷却时间 -1秒；幻鬼步 : 冷却时间 -1秒"，**推断 static[0]=1 即 CD 减免秒数**（引擎消费，未考证）。

## 2. 技能逻辑走读

### 2.1 注册与挂载（C 类核心链路）

无 pushState（被动不走状态机）。挂载点在 `sqr\character\swordman\passive_skill_swordman.nut`（引擎回调 `ProcPassiveSkill_Swordman`，习得/升级时触发）：

```
case 171:
    append = "character/jg_swordman/swordghost13/ap_buff_171.nut"
    if (skill_level > 0 && (growtype == 0 || growtype == 5)) {   // 共通或剑影
        sq_AppendAppendage(obj, obj, 171, false, append, true)
        // ★ 本技能唯一实际写入：僵直列
        rigidity = sq_GetLevelData(obj, 171, 1, skill_level) * 0.1   // col1×0.1 = 150→1170
        change_appendage.addParameter(CHANGE_STATUS_TYPE_RIGIDITY, false, rigidity)
    } else {
        sq_RemoveAppendage(obj, append)    // 转职离开/降级时回收
    }
```

### 2.2 appendage 本体：ap_buff_171.nut（空壳标记）

`E:\...\pvf\sqr\character\jg_swordman\swordghost13\ap_buff_171.nut`（61 行，实测全读）：
注册了 proc/onStart/prepareDraw/onEnd/isEnd 五个回调，**函数体全部为空**（`isEnd` 恒 return false = 永久存在）。
即：appendage 只充当"已习得鬼啸"的**标记载体** + 承载注册行写入的 RIGIDITY 参数——与 248 的 ap_stateoflimit 同构（标记型 appendage）。

### 2.3 消费端全景（白名单内逐一实测）

| explain 声明的效果 | 实现载体 | 实测结论 |
|---|---|---|
| 攻击力增加（col0） | 引擎属性系统 | **无脚本消费者**（grep 全白名单零命中）——引擎伤害管线直读，pvf 层不可见 |
| 攻击僵直提升（col1） | 注册行 CHANGE_STATUS_TYPE_RIGIDITY | 唯一有实体的效果；生效端在引擎命中结算 |
| 三段斩/幻鬼步 CD -1s | 疑引擎（static[0]=1） | 无脚本消费者（`startSkillCoolTime` 全树检索仅命中与本技能无关的两处） |
| 幻鬼步 Y 轴范围 +20%（static[2]） | 疑引擎 | `returnspiritmove.nut:117` 有 `local len = 200;` 但**无 171 门禁**（疑巧合的硬编码，未考证） |
| 普攻/前冲中无施放动作接幻鬼技 | `jg_swordman_common.nut` SpiritMoveContact/GhostSwordCommandEnable/GhostSwordSetState（735-788 行） | 连携系统存在且完整（五个幻鬼技 SPEEDSLASH/GHOSTPIERCE/WHITEGHOSTSLASH/GHOSTDECOLLATION/SWORDDANCEBS 的命令使能+MP 扣费+CD 启动+无动画切状态），但**可见代码无 171 等级门禁**——原版应由引擎按"习得鬼啸"放行，mod 层未还原门禁（推断） |
| 站立被击时可用幻鬼技 | 同上 + 受击管线 | 无 171 相关受击钩子命中 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SwordGhost24.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SwordGhost\SwordGhost24.skl` | ✅ 实测（157 行） | 数值（2 列 level + static 3 槽） |
| 注册行 | passive_skill_swordman.nut:47-65 | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | case 171 挂载 + RIGIDITY 写入 |
| appendage | ap_buff_171.nut | `…\pvf\sqr\character\jg_swordman\swordghost13\ap_buff_171.nut` | ✅ 实测（61 行空壳） | 标记载体（C2 定点读，注册行直指） |
| 同名副本 | JG_SwordMan\swordghost13\ap_buff_171.nut | `…\pvf\sqr\character\JG_SwordMan\swordghost13\` | ✅ 存在 | 大小写双目录副本（C5 白名单内） |
| 常量 | SKILL_SWORD_GHOST_24 <- 171 | `…\pvf\sqr\character\swordman\swordman_header.nut:141` | ✅ 实测 | 技能号常量 |
| 连携系统 | jg_swordman_common.nut:735-788 | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅ 实测 | SpiritMoveContact/GhostSword* 函数组（无 171 门禁） |
| 图标 | SkillIcon.img 636/637 | `Character/Swordman/Effect/SkillIcon.img`（skl [icon]） | ✅ 引用实证 | 共享图标图集（demo 不需要） |
| 角色 .ani / .atk / .als | — | `…\character\swordman\` | ⛔ 无 | 纯数值被动，无视觉无判定 |
| 被动对象 | — | `…\passiveobject\character\swordman\` | ⛔ 无 | 同上 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无） | — | — | — | — |

**零资源需求**：纯数值被动（appendage 无 buff 图标、无光环 ani——注册行未调 setBuffIconImage，ap 本体无任何绘制）。

## 5. 实现方案草案

**⛔ 暂缓（消费链）**，与远古记忆/破极兵刃/流心:狂同队列（属性数值无伤害消费链家族）。分半明细：

| DNF 机制 | 我们的现状（代码实测） | 阻断点 |
|---|---|---|
| 攻击力增加 col0×0.1 = 10%→108% | `NumericType.Attack` 键存在，MeleeHit 只读固定 `HitReaction.Damage`（MeleeHitAction.cs 实测，家族第 N 实证） | 属性数值无伤害消费链 |
| 僵直 +150→1170（RIGIDITY） | `HitReaction.HitstunMs` 为每次命中的固定数据，无全局修改器 | 消费链姊妹缺口：**Hitstun 修改器**（与 Attack 键同属"属性→命中结算"管线） |
| CD -1s（三段斩 8/幻鬼步 128） | `CooldownMs` 为各 SkillLogic 虚属性常量 | 无运行时 CD 修改门面——但**可烘焙**：宿主技能常量直减 1000ms |
| 幻鬼步 Y 轴范围 +20% | 判定盒为技能内常量 | **可烘焙**：宿主技能 hitbox 常量放大 |
| 无施放动作接幻鬼技 | 技能取消体系缺失（064 首报） | ⛔ |
| 被击时施放（受击中出技） | 受击-施法互斥缺失（R1-A4） | ⛔ |

**若做占位**（等消费链解锁前的诚实占位，同 OverDrive 模式）：
- 无需 SkillLogic（无施放）；只需 `GhostHowlBuff : BuffDefinition`（`TotalTimeMs=0` 永久 + `AddActions={AddOwnerNumeric(NumericType.Attack, +108)}` 零消费占位）；僵直面占位无键可挂（NumericType 无 RIGIDITY——新增键也是零消费，不建议）。
- 注册点：`BuffIds.GhostHowlBuff = 18`（L18 号段顺延）。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| SwordGhost24.skl | `.skl` 无子命令（2 列 × ~70 行 + static 3 槽） | 本技能手抄 6 值全明；`skl` 子命令同前议（既有记档） |

无 ani/als/atk/obj 资源——本技能翻译环节仅 `.skl` 一项（重复印证）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 增伤 10%→108% | **缺失档：属性数值消费链**（家族再+1 实证） | ⛔ 主因；等 Attack 键进 MeleeHit 结算 |
| 僵直 +150→1170 | 消费链姊妹缺口（Hitstun 无修改器） | 可烘焙：幻鬼技 HitReaction.HitstunMs 直加固定值（demo +300ms） |
| 三段斩/幻鬼步 CD -1s | 无运行时 CD 修改（烘焙可绕） | 宿主技能 CooldownMs 常量 -1000 |
| 幻鬼步 Y 轴范围 +20% | 无运行时判定盒修改（烘焙可绕） | 宿主技能 hitbox 常量放大 |
| 无施放动作接幻鬼技 | **缺失档：技能取消体系** | 跳过（幻鬼技独立按键直发） |
| 被击中施放幻鬼技 | **缺失档：受击-施法互斥** | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①static[0]=1/static[1]=0 语义（CD-1s 推断）；②level info 表 70 行（末行 1480/11850）vs [maximum level] 50（疑 mod 扩表，实测对表）；③returnspiritmove.nut:117 `len=200` 与 static[2]=200 的关系；④连携函数组缺 171 门禁是引擎放行还是 mod 遗漏。
- **新缺口上报**：**Hitstun 修改器**（RIGIDITY 类被动）——属性消费链的第 2 个消费面（伤害端 HitReaction.Damage 之外，硬直端 HitstunMs 同样无修改器注入点）。剑影系被动（本技能 + C2/C4 批的幻鬼之力/鬼连斩·极等）普遍依赖，建议与 Attack 键消费链合并立项（同改 MeleeHitAction 一处）。
- **给下轮的经验**：`passive_skill_swordman.nut` 是剑士被动的统一挂载点（case 171/209/248/252/254/123/119/78 + procSkill_ 三函数），C 类批次**先读这 294 行**再定各技能链路；标记型空 appendage 是二觉被动的标准形态（171/248 双实证），行为全在消费端脚本里 grep appendage 路径字符串。
