# 空中连斩（JumpAttackMulti）

> 技能ID 17 | 级别 A | 可实现性 ⛔（缺跳跃系统） | 分析日期 2026-08-22 | 批次 A2

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 空中连斩 | `skill\Swordman\JumpAttackMulti.skl [name]` |
| 英文名 | JumpAttackMulti（取 skl 文件名；本技能 [name2] 恰为英文 "Aerial Chain Slash"，属例外——L1 惯例 name2 是中文别名，此处不是） | 同上 [name2] 实测 |
| 职业 | 鬼剑士全系共通（[skill fitness growtype]=0-5） | 同上 |
| 学习等级 | 5 | 同上 [required level] |
| 最高等级 | 50（各觉醒段上限 1） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] / [skill class] |
| 指令 | 空中按攻击键（(ATTACK)） | 同上 [command] |
| CD | **无**（skl 全文无 [cool time] 节——空中攻击由跳跃状态门禁，不走 CD） | 同上实测 |
| MP | 5 → 7（Lv1 → Lv50） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 一句话效果 | 在空中向敌人连续斩击；从第二斩开始才适用本技能攻击力（第一击为普通跳跃攻击） | 同上 [explain] |

**level property（1 列，Lv1 → Lv50）**：`180 → 1008`。模板 `物理攻击力 : <int>%%`（向量 (-1,0,1.0)）→
col0 = 物理攻击力 %（跳跃连斩每刀，180% → 1008%）。**static data = `1`**（单值，语义未考证——推断为链斩计数上限或版本开关）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

**本技能在 `swordman_load_state.nut` 中无注册行**（grep `jumpattackmulti` / `, 17` 均无命中）。
空中连斩属最老一代技能：**跳跃与空中攻击链全部逻辑在客户端引擎内**，pvf 只提供数据文件。
与跳跃相关的唯一注册是共用跳跃状态：

```
IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/jump/swordman_jump.nut", "SwordmanJump", 6, -1);
```

`swordman_jump.nut`（61 行，实测全文）**只处理流心系空中取消**（state 6 中检测技能 105/107/108 的
FlowMindOneFallState / FlowMindTwoFallState），不含空中攻击逻辑——空中攻击链（首击=jumpattack、
后续=jumpattackmultislash1↔2 交替）完全是引擎行为。兄弟职业脚本亦无同型实现
（`atswordman_load_state.nut` / `demonicswordman_load_state.nut` / `common_load_state.nut` grep 均无命中）。

引擎内置证据链（F3 ①③ 两腿，无 PO 故无 ② 腿）：

```
// sqr/character/swordman/swordman_header.nut
CUSTOM_ANI_JUMPATTACKMULTISLASH1 <- 23      // = swordman.chr [etc motion] 第 24 项（996 行，实测）
CUSTOM_ANI_JUMPATTACKMULTISLASH2 <- 24      // = 同上第 25 项（997 行）
```

```
// character/swordman/swordman.chr
922:[jumpattack motion]                      // 专用段：普通跳跃攻击动画
923:    `Animation/JumpAttack.ani`
1281:[jumpattack info]                       // 专用段：普通跳跃攻击命中参数
1282:    `AttackInfo/JumpAttack.atk`
```

### 2.2 引擎内置行为重建（.ani 标记 + .atk 数据 + explain 三方印证）

**空中攻击链时序（推断，引擎内置无脚本）**：

| 步骤 | 动画/参数 | 数据依据 |
|---|---|---|
| 跳跃 | `jump.ani` 16 帧：F0-F6 起跳 600ms → F7 delay 10000（滞空悬停帧，物理驱动）→ F8-F13 下落 → F14 delay 10000（下落悬停帧）→ F15 落地 50ms | 实测帧表；受击盒 1-3 个/帧 |
| 空中首击 | `jumpattack.ani` 6 帧 300ms（100/50/50/40/30/30），**F2 有攻击盒** `13 -13 -11 88 26 167`（min/max → x∈[13,88] z∈[-11,167]，高位的空中斩击盒）+ `JumpAttack.atk`（damage bonus 10 / physic / **down 击倒反应** / push 270 / lift 180 / hit down） | 实测 |
| 第 2 击起（链斩） | `jumpattackmultislash1.ani` ↔ `jumpattackmultislash2.ani` 交替，各 5 帧 370ms（50+80×4），**无攻击盒**（引擎施加武器判定，同 GoreCross 模式）；攻击力取 skl col0（explain：从第二斩开始适用技能攻击力）；命中参数沿用引擎空中攻击（无独立 .atk / .chr etc attack info 条目，**推断**共用 JumpAttack.atk） | 实测 + explain |
| 视觉层 | 每刀同步绘制特效：刀光 `jumpchainattackslash1/2_katana_upper/under.ani`（370ms，与角色帧同构 50+80×4）+ 辉光 `jumpchainattackslash1/2_upper/under_effect.ani`（240ms，LINEARDODGE）+ 首击辉光 `jumpchainattacknormal_upper_effect.ani`（220ms） | 实测（effect/animation/jumpattackmulti/） |

**skill 挂接**：`JumpAttackMulti.skl` 的存在使引擎把"第二击起"的攻击力从普攻数值换成 col0（180%→1008%），
这是本技能的全部机制——**没有独立状态机、没有被动对象、没有 Buff**。

### 2.3 被动对象 / appendage

无（白名单内 grep 实证：无 PO 定义、无 .act、无 ap_*.nut）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character/swordman/animation/jump.ani` | 16 | 21150ms（含 F7/F14 两个 10000ms 物理悬停帧） | 无 | 无 | 起跳 600ms/下落/落地；受击盒 1-3 个/帧 |
| `character/swordman/animation/jumpattack.ani` | 6 | 300ms | 无 | **F2**（13,-13,-11 ~ 88,26,167） | 空中首击（普攻数值） |
| `character/swordman/animation/jumpattackmultislash1.ani` | 5 | 370ms | 无 | 无（引擎施加） | 链斩奇数刀；受击盒 1/帧 |
| `character/swordman/animation/jumpattackmultislash2.ani` | 5 | 370ms | 无 | 无（引擎施加） | 链斩偶数刀 |
| `character/swordman/effect/animation/jumpattackmulti/`（9 个特效 ani） | 4-5 | 220-370ms | 无 | 无 | 刀光（katana 系，帧长与角色动画同构）+ LINEARDODGE 辉光 |
| `character/swordman/effect/animation/jumpattackhold.ani` | 2 | 200ms | 无 | 无 | 空中按住蓄力视觉（F1 RGBA 淡出 + IMAGE RATE）——**引用者未考证** |

`.als` 边车：**本技能全部文件均无**（character animation 目录 ls 实测；同目录的 jumpattack_bladespirit.ani.als 是剑影版，非本技能）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | JumpAttackMulti.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\JumpAttackMulti.skl` | ✅ 实测 | 技能数据（等级/MP/攻击力列） |
| lst 条目 | swordmanskill.lst 63-64 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 17 → 本 skl |
| 注册行 | —（无 pushState） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | 仅共用跳跃状态 6（swordman_jump.nut，只管流心取消） |
| 常量表 | swordman_header.nut 193-194 行 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_JUMPATTACKMULTISLASH1/2 = 23/24 |
| .chr 条目 | [etc motion] #23/#24（996/997 行）+ [jumpattack motion]/[jumpattack info]（922/1281 行） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 动画与命中参数注册 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\` | ⛔ 缺失 | 逻辑在引擎；jump/ 目录仅流心取消脚本 |
| 角色 .ani | jump.ani / jumpattack.ani / jumpattackmultislash1/2.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | 见 §2.4 |
| 角色 .atk | jumpattack.atk | `…\pvf\character\swordman\attackinfo\jumpattack.atk` | ✅ 实测 | 空中攻击命中反应（首击；链斩推断共用） |
| .als | —（无） | 两侧 animation 目录 | ⛔ 缺失（本技能无边车） | — |
| 特效 .ani | jumpattackmulti\*.ani ×9 + jumpattackhold.ani | `…\pvf\character\swordman\effect\animation\` | ✅ 实测 | 刀光/辉光视觉（引擎绘制，无脚本引用者） |
| 装备层 | jump.ani / jumpattack.ani 各 86 份、jumpattackmultislash1/2.ani 各 76 份变体 | `…\pvf\equipment\character\swordman\avatar\` | ✅ 实测（find 计数） | 换装图层（只查存在性） |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 角色 4 个动画全部帧（jump/jumpattack/slash1/slash2 仅引此图集，帧号不同） | **必需（共享）** | ✅ `Bundles\AnimRes\sm_body0000.img.bytes` 已在库 |
| do_jumpchainattack_ldodge_upper.img | sprite_character_swordman_effect_jumpattackmulti.NPK | 链斩上侧辉光 | 可选（视觉） | ❌ |
| do_jumpchainattack_ldodge_under.img | 同上 | 链斩下侧辉光 | 可选（视觉） | ❌ |
| do_jumpchainattack_katana_upper.img | 同上 | 链斩上侧刀光（太刀） | 可选（视觉） | ❌ |
| do_jumpchainattack_katana_under.img | 同上 | 链斩下侧刀光（太刀） | 可选（视觉） | ❌ |
| JumpAttackHold.img | sprite_character_swordman_effect.NPK（img 在 Effect 根目录，不在子目录） | 空中按住蓄力视觉 | 可选（引用者未考证，可不做） | ❌ |

缺失 img：必需 0（角色帧全在已入库 sm_body 图集）、可选 5。特效 4 张同属一个 NPK，一次提取全覆盖。

## 5. 实现方案草案

⛔ 级——按手册可免；此处只记**前提与接入形态**（跳跃系统落地后的最小接入路径）：

- **前提（系统级）**：①跳跃输入（如 Space → 新 button）+ ②玩家 z 轴物理（起跳初速/重力/落地——`LSFlightComponent` 已有 z 物理+重力积分原语，可作为实现起点）+ ③空中状态（jump.ani 悬停帧语义 = 动画停在 F7/F14 由物理驱动切走）+ ④空中攻击键处理。
- **内容件（届时）**：`JumpAttackMultiSkill : SkillLogic` 不需要独立类——DNF 语义是"跳跃状态的攻击键增强"。我们侧更自然的形态是**普攻 SkillLogic 扩展**：检测"施放者处于空中"（需暴露 IsAirborne 门面）时，第 2 刀起 PlayAnim 交替 slash1/slash2、伤害取本技能 HitReaction。跳跃本身做成一个非技能的状态（DNF 同构：跳跃不是技能）。
- **资源届时零提取**：角色帧已在库；特效 4 张 img + 各 json 翻译注册即可。
- 关键数值：MP 5-7（无 MP 系统，忽略）；伤害 = 普攻跳跃攻击 demo 值（JumpAttack.atk damage bonus 10 → MeleeHit 固定值惯例）；命中反应 demo：Damage 60 / HitstunMs 500 / KnockbackX 270 / LaunchY 180（.atk 原值 push270/lift180/down）。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| `JumpAttackMulti.skl` | `.skl` 尚无子命令（1 列 level info + static data） | 本技能手抄 1 组数值可行；随批量化加 `skl` 子命令（同 064 记档） |
| 角色/特效全部 .ani | 无缺口：仅 FRAME/IMAGE/IMAGE POS/DELAY/DAMAGE BOX/ATTACK BOX（全部在翻译规则表内）；本技能 ani 无 SET FLAG / 无 .als | **全部可被现有 ani 子命令翻译** |
| jumpattackhold.ani | `[IMAGE RATE]`（延后档）/ `[RGBA]`（已支持） | IMAGE RATE 整节跳过无碍（该 ani 可不做） |
| .atk（jumpattack.atk） | `.atk` 尚无子命令 | 手抄 ~8 值可接受 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 跳跃（起跳/滞空/落地/空中状态机） | **缺失：跳跃系统（§6.3 清单外新缺口，见 §8）**——无跳跃输入、无玩家 z 物理、无空中状态 | 不实现本技能直至跳跃系统立项；z 物理原语已有（LSFlightComponent） |
| 空中连斩链（首击普攻数值、第 2 击起技能数值、交替动画） | 依赖跳跃系统（同上） | 届时按 §5 形态接入 |
| 悬停帧语义（F7/F14 delay 10000 由物理切走） | 我们的动画推进是纯时长驱动（AnimClipData.delay） | 翻译时把 10000 改小或技能逻辑强制切帧；需约定处理方式 |
| 太刀专属刀光（katana 层） | 无武器类型系统 | demo 固定用 katana 层（或跳过特效） |

## 8. 存疑与缺口上报

**未考证项**
1. 链斩命中参数是否真共用 JumpAttack.atk（无独立 .atk/.chr 条目，仅推断）。
2. static data `1` 的语义。
3. jumpattackhold.ani 的引用者（空中按住蓄力？无脚本可查）。
4. 除太刀外其他武器类型的链斩刀光是否有专属层（effect 目录只有 katana + 通用辉光层）。

**新系统级缺口（§6.3 清单外，主循环汇总）**
1. **跳跃系统**：跳跃输入（新按键）+ 玩家可控 z 轴物理 + 空中状态机 + 落地判定 + 空中攻击。
   本批 17/175 两技能直接撞上；后续 JumpAttackHighFast（ID 43）、流心系空中取消等一批技能同源。
   已有资产基础：jump.ani 等角色帧全在已入库 sm_body 图集（零提取）；z 物理/重力积分原语已在
   LSFlightComponent（击退浮空用）——补"可控起跳 + 空中状态"即可，是**性价比很高的系统投资**。

**翻译工具缺口**：`.skl` 子命令、`.atk` 子命令（同 064，2 条；.ani/.als 侧无新缺口）。
