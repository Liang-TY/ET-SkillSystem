# 破空拔刀斩（MomentarySlashEx）

> 技能ID 97 | 级别 E（**预分类纠偏：非 TP——[type] active、skill class 1，剑魂二觉替换型主动技**（拔刀斩的 70 级进化版）） | 可实现性 🔶（瞬发"周身大范围斩 + 前方剑气"可直接表达；按住蓄气（0.5s 上限、满蓄 +100%~198%）撞按住输入缺口→固定档降级） | 分析日期 2026-08-22 | 批次 E7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 破空拔刀斩 | `skill\Swordman\MomentarySlashEx.skl` [name] |
| 英文名 | MomentarySlashEx（取 skl 文件名；[name2] 实测 `Speed Slash`） | 同上 |
| 职业 | 剑魂二觉（[second growtype maximum level] 12 槽**第 2/3 位=30** = 剑魂，R6-C4 捷径；[skill fitness growtype] 空） | 同上 |
| 学习等级 | 70（[required level range] 2）；前置 9 拔刀斩 Lv1 | 同上 |
| 最高等级 | 50（二觉档实际 30） | 同上 |
| 类型 | active（skill class 1）/ 物理（[weapon effect type] physical） | 同上 |
| 指令 | →←↑→ + Z（MP 优惠 50%/50%） | 同上 |
| CD | 50000 ms（pvp [start cool time] 600000——禁用级） | 同上 |
| MP | 800 → 1680 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×2 | 同上 [consume item] |
| static data | `100`（单值；无向量引用，语义未考证——疑剑气飞行/尺寸参数，基础 009 的 6 值蓄力阈值表未继承） | 同上 |
| 一句话效果 | 拔刀向周身大范围强斩，并向前方放出剑气；按住蓄气（最短 0/最长 0.5s）可提升拔刀斩与剑气的攻击力并扩大剑气范围 | 同上 [explain] + level property |

**level property 模板解码（7 列 + 7 向量，L21 法全解，Lv1→Lv50 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 物理攻击力 | (-1,0,1.0) | col0 = 8964 → 101437 % |
| 剑气物理攻击力 | (-2,1,1.0) | col1 = 8964→99551（Lv48 前与 col0 同值，Lv49 起分化为 col0 的 ~46%） |
| 蓄气时间上限 | (-1,2,0.001) | col2 = 500 → **0.5 s 恒定** |
| 剑气移动距离 | (-1,3,1.0) | col3 = **10（恒定）**——疑非像素直读（10px 太小，引擎内部换算未考证） |
| 最大蓄气时移动距离增加比率 | (-1,4,0.1) | col4 = 10000×0.1 = **1000 %（恒定）**（满蓄剑气范围 ×11，与 col3 联动） |
| 最大蓄气时攻击力增加比率 | (-1,5,0.1) | col5 = 1000→1980 → **+100% → +198%** |
| 最短蓄气时间 | (-1,6,0.001) | col6 = 0（dungeon，可瞬放）/ 500ms（pvp） |

pvp 表同构（col0=430→4869% 等，攻击减档）；[skill preloading image] `MomentarySlashBlade.img`（刀光预载，009 同族节）。

**与基础 009 拔刀斩对照**：基础"蓄势 500ms→出刀+拔刀波"；Ex = "**可蓄气 0~500ms 的周身斩 + 独立剑气弹**"，数值规模 ×3.4（col0 8964% vs 基础 2628%），太刀抓取/精通追击等基础分支未继承（explain 无精通条件句）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（引擎内置 + 空壳残留）

- load_state **无 pushState**（grep momentaryslashex 无命中）。
- `sqr\character\swordman\momentaryslash\momentaryslash.nut` 存在但**全文件仅 3 行空白**（实测）——空壳残留（基础 009 的活跃 nut 在 `badao\badao.nut`；本目录疑上一版布局残留）。**判"引擎内置"前先 cat 目标 nut防空壳干扰**（本批新坑）。
- 剑气 PO：`MomentarySlashExWave.obj` = **20065**（passiveobject.lst:11262-11263 实测）。
- 施法动画：.chr etc motion **#98** = `Animation/MomentarySlashEx.ani`（行 1071；`swordman_header.nut:268` `CUSTOM_ANI_MOMENTARYSLASHEX <- 98` 双证）；etc attack info `AttackInfo/MomentarySlashEx.atk`（.chr 行 1361，槽号按 009 #8=行 1302 线性推算 ≈#67）。

### 2.2 引擎内置行为重建（.ani 标记 + PO 数据反推，推断标注）

```
按下技能键 → 播 momentaryslashex.ani：
  F0 = 100000ms 站桩蓄气帧（L23 待事件——按住期间悬停；
    引擎读按住时长 t∈[col6=0, col2=500ms]，松开或到上限进入出刀）
  → F1 转斩（~30ms）→ F2-F5 出刀判定（25+25+50+50ms 窗口）：
    角色周身攻击盒 F2-F5 `-20 -65 35` + `258 95 27` → x[-0.2,2.38] 水平大范围斩
    命中走 MomentarySlashEx.atk（角色侧：damage 反应/push200/lift200）
  → F5 flag=1：创建剑气 PO 20065（引擎侧，推断同帧）
    剑气：new_bigwave.ani（F0=10000ms 悬停 + 尾帧 280ms）F0 盒 x[1.54,2.06]
    向前推进（int data 300 疑=射程 300px；atk down/push0/lift0）
    badao.act（PO 行为层）：F0 创建粒子 PO 53688（missile.Ptl）于 x+50
蓄气加成（引擎折算）：满蓄时 攻击 ×(1+col5×0.001)（+100%~198%）、剑气距离 ×(1+col4×0.001)（×11）
```

### 2.3 被动对象（momentaryslashexwave.obj = 20065）

| .obj 节 | 值 |
|---|---|
| [basic motion] | `animation/momentaryslashex/new_bigwave.ani`（3 帧 10280ms，**F0 攻击盒** `154 -77 -45` + `52 145 94`→x[1.54,2.06]；F0 delay=10000 悬停帧，命中窗口引擎截断——与基础 009 拔刀波同构） |
| [attack info] | `AttackInfo/MomentarySlashExWave.atk`：physic / no element / **down** / hit horizon / push 0 / lift 0（与基础 momentaryslashwave.atk 同参数，伤害基底不同） |
| [int data] | `300`（疑射程/寿命 300px，未考证） |
| 行为层 | `badao.act`：[BASE ANI]=start.ani；F0 TRIGGER → CREATE PASSIVEOBJECT 53688（粒子 missile.Ptl，pos 50,0,0）——.act 数据驱动行为层（R5-B6 族）在 Ex 线复用 |

**角色侧 MomentarySlashEx.atk**（实测）：physic / damage 反应 / push **200** / lift **200** / hit horizon（基础 009 角色 atk 同参数族——周身斩击退浮空）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img |
|---|---|---|---|---|---|
| `character\…\momentaryslashex.ani`（.chr #98） | 12 | 名义 100555 | **F5=1**（释剑气，推断） | **F2-F5**（§2.2 水平斩盒） | sm_body（L16） |
| `momentaryslashex.ani.als` | — | — | — | — | 挂 MomentarySlashBlade 刀光层（预载清单互证） |
| PO new_bigwave.ani（剑气判定） | 3 | 10280（F0=10000 悬停） | 无 | **F0**（§2.3） | `Effect/MomentarySlashEx/New_BigWave.img`（与基础 009 拔刀波同图共享！） |
| PO new_bigwave.ani.als | — | — | — | — | 叠层 |
| PO start.ani / startb.ani（badao.act 基动画） | 3 / 3 | 10280 / 10200 | 无 | F0（start：`-69 -69 -68 220 137 143`——与基础 009 Start.ani 逐值相同） | New_BigWave.img |
| PO new_normal.ani | — | — | — | — | 剑气常态层 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MomentarySlashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MomentarySlashEx.skl` | ✅（234 行） | 7 列全解 |
| 注册行 | —（无） | `…\sqr\character\swordman_load_state.nut` | ⛔ 无 | 引擎内置 |
| 空壳 nut | momentaryslash.nut | `…\sqr\character\swordman\momentaryslash\` | ✅（3 行空白，残留） | 无逻辑 |
| PO 注册 | passiveobject.lst:11262-11263 | `…\passiveobject\passiveobject.lst` | ✅ | 20065 |
| PO 定义 | momentaryslashexwave.obj | `…\passiveobject\character\swordman\` | ✅ 实测 | §2.3 |
| PO 行为 | badao.act | `…\passiveobject\character\swordman\animation\momentaryslashex\` | ✅ 实测 | F0 粒子创建 |
| PO .atk | momentaryslashexwave.atk | `…\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | down/0/0 |
| 角色 .atk | momentaryslashex.atk | `…\character\swordman\attackinfo\` | ✅ 实测 | damage/push200/lift200 |
| .chr 条目 | etc motion #98 + etc attack（行 1361） | `…\character\swordman\swordman.chr` | ✅ 实测 | 动画/攻击注册 |
| 常量 | swordman_header.nut:268 | `…\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | CUSTOM_ANI_MOMENTARYSLASHEX=98 |
| 角色 .ani | momentaryslashex.ani（+.als） | `…\character\swordman\animation\` | ✅ 实测 | §2.4 |
| PO .ani | momentaryslashex\ 7 文件（new_bigwave/start/startb/new_normal + 2 .als + badao.act + missile.ptl） | `…\passiveobject\character\swordman\animation\momentaryslashex\` | ✅ 实测 | 判定+视觉 |
| 粒子 | missile.ptl | 同上目录 | ⛔ L5 | 剑气拖尾 |
| 基础技文档 | 009-MomentarySlash.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 对照基准（New_BigWave 共享） |
| 关联 TP | MomentarySlashExp.skl（154，E5 批） | `…\skill\Swordman\` | ✅ 存在 | 见 §8 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `Character/Swordman/Effect/MomentarySlashEx/New_BigWave.img` | sprite_character_swordman_effect_momentaryslashex.NPK | 剑气判定帧视觉（**与基础 009 拔刀波共享同一张**——009 已列必需） | **必需**（共享） | ❌ |
| `Character/Swordman/Effect/MomentarySlashBlade.img` | sprite_character_swordman_effect_momentaryslashblade.NPK | .als 刀光层（skl 预载清单互证） | 可选 | ❌ |
| sm_body0000.img | （已入库） | 角色斩击 12 帧 | 必需（共享） | ✅ |

**缺失 img：必需 1 张（与 009 共享）、可选 1 张——两个 NPK。** 粒子 missile.ptl（L5）。

## 5. 实现方案草案（号段：SkillIds 36 / AnimIds 192-193 / BulletIds 8，E7 批内顺延）

### 内容件清单

1. **`DotNet~/Skills/SpeedSlashSkill.cs : SkillLogic`**（009 MomentarySlashSkill 的"去蓄力版"直改，BloodBoom 帧触发范式）：
   - `CooldownMs=50000`；`TotalTimeMs=600`（F1-F11 出刀段 ≈455ms + 余量；**F0 蓄气帧不译**——固定档）。
   - `OnCast`：`ctx.PlayAnim(AnimId.SwordmanMomentarySlashEx)`（json 手改 F0=100ms 固定蓄势，§6）+ `ctx.ClearHitTargets()`。
   - `OnUpdate`：`CurrentFrameIndex()` 达 F2 且 SubState==0 → `ctx.SetAttackHitbox(offset (1.1,0,0.5), half (1.3,0.8,0.3))`（F2-F5 盒 x[-0.2,2.38] 折算，中心前偏）+ `ctx.CreateBullet(BulletIds.SpeedSlashWave)`；F5 后 → `DisableAttackHitbox()`。
   - `HitReaction{Damage=200, HitstunMs=500, KnockbackX=200, LaunchY=200}`（角色 atk push200/lift200 直译；Damage=col0 8964% demo 折算）。
2. **`DotNet~/Bullets/SpeedSlashBullet.cs : BulletDefinition`**（复制 NormalWaveBullet：`Speed` 按 int data 300px≈3 单位/s、`TotalTimeMs=300`、穿透、`HalfExtents=(0.3,0.7,0.7)`（F0 盒折算）、`HitActions={MeleeHit}`、`HitReaction{Damage=120, HitstunMs=800, KnockbackX=0, LaunchY=0}`（wave atk down/push0/lift0）、`ViewAnimId=AnimId.SpeedSlashWave`（new_bigwave，json 手改 F0））。
3. 蓄力档（后续）：按住输入缺口落地后——F0 帧时长改引擎驱动 + `Damage ×(1+col5×0.001×蓄满比)` 插值 + 剑气射程 ×(1+col4×0.001)。

### 概念映射

| DNF 概念 | 我们的组件 |
|---|---|
| 引擎状态 + momentaryslashex.ani F0 蓄气帧 | Skill + json F0 固定 100ms（按住缺口，R3-A15 族） |
| F2-F5 周身斩（角色 atk） | SetAttackHitbox + HitReaction（push200/lift200 击退浮空） |
| 剑气 PO 20065（new_bigwave F0 盒 + atk down） | SpeedSlashBullet（穿透短射程） |
| 蓄气加成（+100%~198% / 范围 ×11） | 不做（固定档；缺口同 009 蓄力五段） |
| badao.act 粒子拖尾 | 粒子系统缺失（L5） | Bullet 视图自带尾帧替代 |
| pvp 600s 起手 CD | 无 pvp | 跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.SpeedSlash = 36` + 新键 |
| BulletId | `Runtime\BulletDefinition.cs` | `SpeedSlashWave = 8` |
| AnimId | `AnimConfigRegistry.cs` | `SwordmanMomentarySlashEx = 192`、`SpeedSlashWave = 193` |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×2；New_BigWave.img（与 009 共享一次提取） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 50000 ms | 50000 直用 |
| 总时长 | F0 蓄气（按住 0~500ms）+ 出刀 ≈455ms | 固定 100+455 |
| 周身斩伤害 | col0 8964%→101437%（物理） | 200 |
| 周身斩反应 | damage / push200 / lift200 | Hitstun 500 / Kb 200 / Ly 200 |
| 剑气伤害 | col1 8964%（Lv1，=col0） | 120 |
| 剑气反应 | down / push0 / lift0 | Hitstun 800 / 0 / 0 |
| 剑气射程 | int data 300（疑 px） | Bullet 300ms × 3 单位/s |
| 满蓄加成 | 攻 +100%~198% / 剑气距离 ×11 | **不做**（固定档） |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| MomentarySlashEx.skl | `.skl` 无子命令（7 列 + 预载节） | 手抄（已全解） |
| momentaryslashex.ani | `[DELAY] 100000`（F0 蓄气悬停）——**本技能出刀帧表不受影响，但直译会永播不完** | L23 钳制：翻译后手改 F0=100（固定档） |
| new_bigwave.ani / start.ani | `[DELAY] 10000`（F0 悬停） | 同 009 处理（手改 F0=280） |
| momentaryslashexwave.obj | `.obj` 无子命令 | 手工映射 Bullet（§5 已给） |
| badao.act | `.act` 无子命令（TRIGGER/BEHAVIOR/CREATE PASSIVEOBJECT，R5-B6 已记） | 粒子行为不译（视觉替代） |
| momentaryslashex.atk / momentaryslashexwave.atk | `.atk` 无子命令 | 手抄各 ~7 值 |
| missile.ptl | `.ptl`（L5） | 跳过 |
| .als ×2 | [use animation]/[add] | ✅ 全覆盖 |

本技能翻译缺口 6 类（.skl/.obj/.atk/.act/超长 DELAY×2 处/.ptl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 按住蓄气 0~500ms（松开即出刀，最短 0） | **按住输入缺失**（R3-A15 四技同族；009 五段同族）——第 8 例消费方 | 固定 100ms 蓄势瞬发（损失满蓄 +100%~198% 档；上手差异小因上限仅 0.5s） |
| 满蓄剑气范围 ×11（col4=1000%） | 同上 + 对象缩放延后 | 固定射程 |
| 剑气 F0 10000ms 悬停命中窗 | L23 超长 DELAY | json 手改 + Bullet 寿命表达 |
| 双伤害来源（周身斩 atk + 剑气 atk） | 单 HitReaction 已可表达（技能侧盒 + Bullet 各自 HitReaction） | 直译双来源（比基础 009 的"双来源取一"更干净——本技两载体天然分离） |
| badao.act 粒子拖尾 | 粒子系统（L5） | Bullet 视图尾帧 |
| 无色×2 / SP 70 | 延后 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①static=100 语义；②col3=10"移动距离"的引擎换算（10px 不合理，疑内部倍率）；③int data 300（射程推断）；④剑气创建帧（F5 flag=1 推断）；⑤col1 与 col0 在 Lv48 前同值、Lv49 分化的设计意图（疑满级表中剑气占比刻意下调）；⑥momentaryslash\ 空壳 nut 的来历（疑旧版布局残留——**判 F3 前先 cat nut 内容**，本批新坑记档）。
- **新旧 TP 并存关系结论（本批专项②）**：**97 与 154（MomentarySlashExp，E5 批）不是两代 TP**——97 是 [type] active 二觉替换主动技（前置 9 Lv1、二觉 30 档、独立 50s CD）；拔刀斩的 TP 是 154。**铁证：基础技 MomentarySlash.skl [feature skill index] = 154**（非 97）。
- **给 009 的回填**：009 §3"关联强化 MomentarySlashEx（CUSTOM_ANI=98 / atk / momentaryslashexwave.obj / momentaryslashex.act）"四资产本批全部实测定位（.act 实为 `animation\momentaryslashex\badao.act`，009 记名 momentaryslashex.act 有偏差）；New_BigWave.img 为两技共享必需图（009 已列，无需重复提取）。
