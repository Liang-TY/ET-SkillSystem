# 波动爆发（ReleaseWave）

> 技能ID 32 | 级别 A | 可实现性 ✅（**我们游戏已���现**——本档分析官方原版并与 mod 版/我们实现三方对照） | 分析日期 2026-08-22 | 批次 A7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 波动爆发 | `skill\Swordman\ReleaseWave.skl` [name] |
| 英文名 | ReleaseWave（取 skl 文件名；[name2] 实测为英文别名 `Release Wave`） | 同上 [name2] |
| 职业 | 阿修罗（[skill fitness growtype]=4，L17 映射） | 同上 |
| 学习等级 | 25 | 同上 [required level] |
| 最高等级 | 50（growtype 上限：阿修罗 30） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | 主动（active，skill class 1） | 同上 [type] |
| 指令 | ↑↑ + Z（指令施法 MP 优惠 10%/20% 档） | 同上 [command] |
| CD | 15000 ms（基础值；explain 明言"冷却时间随 Lv 增加而减少"——递减公式引擎内置，skl 数据面只见 `[cool time] 15000 0`，**未考证**） | 同上 [cool time] + [explain] |
| MP | 65 → 630 | 同上 [consume MP] |
| static data | `600`（单值；对照 mod 版为 600 僵直——01§5.5 已记档"施放后僵直 600ms"） | 同上 |
| 可施放状态 | **[executable states] 8** = 普攻状态中可施放（普攻取消用）——**取消体系**（064 上报缺口）的 skl 数据面 | 同上 [executable states] |
| 一句话效果 | 体内波动之力瞬间爆发，把周围敌人朝施放者面朝方向击飞；伤害小但瞬发，常用脱险；施放时生成一枚波动印 | 同上 [explain] |

**level property（1 列，Lv1 → Lv50）**：`4004 → 32024`（单列=独立攻击力，向魔法攻击基数结算——列语义由"1 列+atk 魔法"高置信推断）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（F3 引擎内置 + 空动画侧）

**官方版在 `swordman_load_state.nut` 无 pushState、白名单内无同名 nut**（grep `releasewave` 于 sqr\character\swordman\ 全树无命中，实测）——波动爆发是老一代技能，**角色侧逻辑完全引擎内置**（与 mod 版自注册 state 156 形成对照，01§5.3）。

**角色侧零动画资产**（三方对照关键事实）：
- `character\swordman\animation\` 无 releasewave 动画（ls 实测）；
- `swordman.chr` 无 releasewave 条目（grep 实测）；
- `equipment\...\avatar\` 下 release* 换装图层 **0 个**（find 计数实测——官方版角色没有专属动作，靠通用施法姿态+PO/特效演全部视觉）。

### 2.2 引擎内置流程重建（以 .obj/.atk/.ani 数据文件印证）

```
onSetState（推断，引擎内置）:
    扣 MP；创建 PO releasewave.obj（ID 引擎内置）于自身位置
    → PO [basic motion] ReleaseWave.ani（5 帧 400ms）+ [attack info] ReleaseWave.atk
    → 视觉：PO 层叠 releasewave_eff_bot_00/bot_01/top/ghost 系（见 §2.4）
       + 角色特效 releasewave1.ani（7 帧 490ms）/releasewave2.ani（6 帧 420ms）
onEndCurrentAni: 瞬发即回（static 600 = 施放后僵直）
```

**判定体（ReleaseWave.ani，被动对象动作，实测）**：
- 5 帧、总时长 400ms、**每帧 1 个攻击盒 `-180 -80 0 360 160 230`**（偏移+尺寸格式）→ x∈[-180,180]/y∈[-80,80]/z∈[0,230] ≈ **3.6×1.6×2.3 单位的自身中心盒**（角色原点对称，覆盖贴身一圈）；
- **[IMAGE] 全空**（无贴图引用）——纯判定时间轴占位（L7 同款），视觉全靠特效层。

**命中反应（PO 侧 `passiveobject\...\attackinfo\ReleaseWave.atk`，实测）**：
```
[attack type] magic   [elemental property] light element   （魔法/光属性）
[damage reaction] down   [attack direction] hit horizon   [hit info] blow
[push aside] 400   [lift up] 400   [hit wav] R_SHOCKWAVE_HIT
```
——**mod 版的 releasewave_light.atk（01§5："魔法/击倒/push400/lift400/blow"）与官方 PO atk 数值完全一致**——mod 作者直接抄了官方 atk（"light" 名源是官方的 [light element] 光属性）。

### 2.3 波动印联动（跳过档）

explain："施放技能时， 可以生成波动印"——波动印（wavemark，技能 47）是阿修罗资源系统（swordman_throw.nut case 47 实测挂 ap_wavemark）。本技能只是"施放+1 印"，无独立逻辑。我们无资源系统 → 跳过。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| passiveobject\...\animation\ReleaseWave.ani（PO 判定） | 5 | 400ms | 无 | **全 5 帧**（360×160×230px） | 空贴图占位 |
| …\releasewave\releasewave_eff_bot_00.ani | 4 | 280ms | 无 | 无 | 底层视觉（RELEASEWAVE3.IMG） |
| …\releasewave\releasewave_eff_bot_01.ani（+.als） | 未逐帧 | — | 无 | 无 | 底层 2；**.als**：[add] F0/层 10001 叠 ghost_b_dodge |
| …\releasewave\releasewave_eff_top.ani | 6 | 420ms | 无 | 无 | 顶层视觉 |
| …\releasewave\releasewave_eff_ghost_a/b_*.ani | 未逐帧 | — | 无 | 无 | 残影层（ghost_b_dodge.als 再叠 ghost_b_normal@F2/层-1） |
| character\swordman\effect\animation\releasewave1.ani | 7 | 490ms | 无 | 无 | 冲击波主体（ReleaseWave1.img） |
| character\swordman\effect\animation\releasewave2.ani | 6 | 420ms | 无 | 无 | 冲击波副层（ReleaseWave2.img） |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ReleaseWave.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ReleaseWave.skl` | ✅ | 技能数据 |
| 注册行 | —（无） | `…\pvf\sqr\character\swordman_load_state.nut` | ⛔ 缺失（引擎内置） | F3 走读法适用 |
| 主 nut | —（不存在） | `…\pvf\sqr\character\swordman\`（grep 无） | ⛔ 缺失 | 参照 mod 版 nut（01§5.2） |
| .chr 条目 | —（无） | `…\pvf\character\swordman\swordman.chr` | ⛔ 缺失 | 官方无角色动画 |
| 角色 .ani | —（无） | `…\pvf\character\swordman\animation\` | ⛔ 缺失 | 同上 |
| 角色 .atk | —（无） | `…\pvf\character\swordman\attackinfo\` | ⛔ 缺失 | 命中在 PO 侧 |
| PO 定义 | releasewave.obj | `…\pvf\passiveobject\character\swordman\releasewave.obj` | ✅ | 判定+视觉 PO（§2.2） |
| PO .ani | ReleaseWave.ani + releasewave\*.ani ×7 | `…\pvf\passiveobject\character\swordman\animation\` | ✅ | 判定时间轴 + 特效层 |
| PO .atk | ReleaseWave.atk | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ | 魔法/光/down/push400/lift400 |
| PO .als | releasewave_eff_bot_01.ani.als、releasewave_eff_ghost_b_dodge.ani.als | 同 animation 目录 | ✅ | 特效层叠（标准 [add]） |
| 角色特效 | releasewave1/2.ani | `…\pvf\character\swordman\effect\animation\` | ✅ | 冲击波视觉（ReleaseWave1/2.img） |
| 装备层 | —（0 个） | `…\pvf\equipment\character\swordman\avatar\` | ⛔ 无（官方无角色动作） | — |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| CHARACTER\SWORDMAN\EFFECT\RELEASEWAVE3.IMG | sprite_character_swordman_effect_releasewave.NPK | PO 特效层全家族（bot_00/bot_01/top/ghost ×6 个 ani 共用） | 必需（若做官方视觉） | ✅ **已在库**（`releasewave3.img.bytes`，01§5.4 资源收集时入的） |
| Character\Swordman\Effect\ReleaseWave1.img | 同上 | 角色冲击波主体 | 可选 | ❌ |
| Character\Swordman\Effect\ReleaseWave2.img | 同上 | 角色冲击波副层 | 可选 | ❌ |

缺失 img：必需级 0 张（RELEASEWAVE3 已在库！）、可选级 2 张（同 NPK）。

## 5. 三方对照：官方原版 vs 玩家 mod 版 vs 我们实现（本档核心产出）

| 维度 | 官方原版（本档走读） | mod 重做版（01§5） | 我们实现（ReleaseWaveSkill as-built） |
|---|---|---|---|
| 角色动作 | **无专属动画**（通用姿态，avatar 0 图层） | 自制冲刺动画 releasewavedash_body.ani（3 帧 230ms）+ .als 11 层 | 用 mod 版动画（AnimId.SwordmanReleaseWaveDash=21） |
| 角色位移 | **无位移**（原地爆发） | 前冲 300px（onProc 匀速） | `MoveCasterForward` 纯函数 3 单位 / 230ms |
| 爆炸位置 | 自身位置（中心对称盒 360×160×230px） | 施放点（冲出爆炸，flag 10001@F0） | `CreateAreaInFront(0)` 施放点（同 mod） |
| 判定尺寸 | 3.6×1.6×2.3 单位 | releasewave1.ani 400×100×234px（4×1×2.34） | HalfExtents (2, 1.17, 0.5) |
| 属性/类型 | 魔法 / **光属性** | 魔法（无属性标记） | 无元素系统（固定值） |
| 命中反应 | down/push **400**/lift **400**/blow | releasewave_light.atk 同值（抄官方） | 冲刺 80/800/400/400 + 爆发 150/800/400/400 |
| 伤害 | skl col0 4004→32024（等级成长，单列） | 自定写包倍率 | 固定 80/150 |
| CD | 15000ms 递减（引擎公式未考证） | 自定（01§5 记 15s 基础） | 5000ms（demo） |
| 施放后僵直 | static 600ms | 600ms（沿用） | 无（回待机即解控，01§5.5 记档） |
| 普攻中取消 | [executable states] 8（可取消普攻） | 无此标记 | 无取消体系（缺口档） |
| 波动印 | 施放+1 印（47 联动） | 无 | 无资源系统 |

**结论**：我们的实现 = **mod 版忠实移植**（位移/爆炸点/数值链全同构）；与官方原版的真实差异只有两处——① 官方**无角色位移**（脱险靠击飞敌人而非自己跑）；② 击飞方向语义（见下）。若想更贴官方手感，改法极小：

- **去位移版**：删 OnUpdate 的 MoveCasterForward 段 + TotalTimeMs 改 400（PO 判定时长）即可（其他全不动）——建议做成 skillconfig 或双技能对照 demo。
- **击飞方向语义差异（新发现，值得记档）**：官方"朝**施放者面朝方向**击飞"（explain 明言）= 前后所有敌人都往面朝方向飞；我们 `LSActionContext.LaunchOwner`（实测行 110）`dir = owner.x < source.x ? -1 : +1` = **远离施法者方向**（径向推离）。身前敌人两者一致，**身后敌人方向相反**。官方语义复刻需 LaunchOwner 加"按 source 朝向"模式——一处小改，涉及 LSAction 框架层，本档仅记档不动手。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| ReleaseWave.ani（PO 判定） | 节面常规（含全帧 ATTACK BOX——已支持多盒） | 现有 ani 子命令全覆盖 |
| releasewave_eff_*.ani ×7 + 2 个 .als | [use animation]/[add] 标准 | 现有 ani/als 子命令全覆盖 |
| releasewave1/2.ani（角色特效） | 本次实测无规则外节 | 全覆盖 |
| ReleaseWave.skl / ReleaseWave.atk（PO） | `.skl`/`.atk` 无子命令 | 并入既有缺口（本技能量小：1 列 + 8 值） |
| ReleaseWave.ani 的 [IMAGE] 空路径 | 现有规则可处理（空白帧） | 无缺口 |

计 2 条既有缺口（.skl/.atk），无新节、无 .ptl。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 魔法/光属性结算 | 元素属性系统（缺失档） | 固定伤害（demo 惯例） |
| CD 随等级递减 | 等级数值缩放（延后档） | 固定 CD |
| 施放后僵直 600ms（static） | 无施放后僵直机制（延后档，01§5.5 已记） | 跳过（回待机即解控） |
| 普攻中可取消施放（executable states 8） | 技能取消体系（缺失档，064 首报） | 跳过 |
| 击飞方向=面朝方向 | **LaunchOwner 方向语义**（§5 新发现——径向 vs 面朝，框架层一处小改） | 记档；官方复刻时改 |
| 波动印+1 | 资源/印记系统（无对应档，新记） | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. CD"随等级递减"的引擎公式（skl 数据面 `[cool time] 15000 0` 无递减参数——疑引擎硬编码表或与 level 联动）。
2. 官方版施法瞬间的角色通用姿态具体用哪个动画（无数据文件可考）。
3. PO 创建参数（引擎内置 ID 与出生偏移——按 .obj 自身中心推断）。

**系统级缺口（非新增）**
- LaunchOwner 击退方向语义（径向推离 vs DNF"面朝方向击飞"）：本批新发现的**框架层语义差异**，影响所有依赖"方向性击飞"的技能（波动爆发/裂波斩等），建议主循环在 00-总览 里单列记档并评估是否给 HitReaction/LaunchOwner 加方向模式。

**给下轮的经验**：官方 ReleaseWave（32）与 mod 版（01§5）是**同技能两套实现**的罕见样本——官方走 F3 引擎内置（零 nut/零角色动画/PO 承担一切），mod 走全 nut 化。分析 mod 起源的技能时记得查官方原版 skl/PO（本例 mod atk 数值=官方直抄），三方对照能快速锚定"哪些是 mod 作者自创"。
