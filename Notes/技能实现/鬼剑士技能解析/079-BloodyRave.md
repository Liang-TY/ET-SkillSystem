# 嗜魂封魔斩（BloodyRave）

> 技能ID 79 | 级别 A（维持预判） | 可实现性 ⛔（"吸附拉拽"撞位移他人/抓取双缺口；深简化"定身+斩击"近似见 §5） | 分析日期 2026-08-22 | 批次 A11

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 嗜魂封魔斩 | `skill\Swordman\BloodyRave.skl [name]` |
| 英文名 | BloodyRave（取 skl 文件名；[name2] 实测为英文 `Bloody Rave`） | 同上 [name2] |
| 职业 | 狂战士（[skill fitness growtype]=3，L17） | 同上 |
| 学习等级 | 35（[required level range] 2） | 同上 [required level] |
| 最高等级 | 50（六系上限 0/0/0/30/0/0——仅狂战系 30，growtype0/5 通用上限另有 50 行为） | 同上 [maximum level] / [growtype maximum level] |
| 类型 | active（skill class 2）/ 物理 | 同上 [type] / [weapon effect type] |
| 指令 | ←→→ + Z | 同上 [command] |
| CD | 30000 ms（dungeon）；pvp 20000（[start cool time] 20000） | 同上 [cool time] |
| MP | 180 → 1512（Lv1 → Lv50；pvp 123 → …） | 同上 [consume MP] |
| 特殊消耗 | 施放中每秒削减 HP（等级数据 col6×0.1，Lv1=2.3/秒）；只能在[血之狂暴]状态使用（引擎门禁）；消耗道具 3037×1 | 同上 [explain] / [consume item] |
| 前置 | 技能 24（怒气爆发）Lv1 | 同上 [pre required skill]（24 号 skl [name] 实测=怒气爆发） |
| 一句话效果 | 把前方敌人吸附到近身处，用血气之剑斩击；按住技能键持续吸附（每秒扣自身 HP），吸附数越多斩击伤害越高；头目/稀有/精英/深渊类敌人受创更深 | 同上 [explain] |

**static data**（dungeon `500 3000 180 55 8 250 4 5 6 6 6 0`，pvp `500 3000 180 55 8 500 2 3 4 4 4 0`）：仅 col4 有模板实证——**8 = 达到最高攻击力所必须的吸附总数**（pvp 同 8；其余列语义未考证，推断 500=吸附范围 px、3000=按住最大时长 ms、250=拉拽速度量级、4/5/6/6/6=头目/稀有/精英/深渊等分类增伤档、0=保留）。

**level info（7 列，Lv1 → Lv50 首末值）+ 模板向量解码**（L21 法，8 向量 ↔ 8 占位符）：

| 列 | 向量 | 模板语义 | Lv1 值（换算） |
|---|---|---|---|
| col0 | (-1,0,1.0) | 物理攻击力 % | 2314% |
| static[4] | (4,4,1.0) | 最高攻击力所需吸附总数 | 8 |
| col1 | (-1,1,1.0) | 最高物理攻击力 % | 10428% |
| col2 | (-1,2,0.1) | 出血机率（float） | 228→22.8% |
| col3 | (-1,3,1.0) | 出血 Lv | 45 |
| col4 | (-1,4,0.001) | 出血持续时间 | 5000→5 秒 |
| col5 | **(-4,5,1.0)** | 出血攻击力 | 101（源 -4 **新见**，未考证——L21 只记录过 -1/-2=level 列、≥0=static 槽） |
| col6 | (-1,6,0.1) | 每秒 HP 削减（float） | 23→2.3/秒 |

col0 Lv50=10428 区间值、col1 Lv50=约 4.7 万量级（未逐列抄全，文档只取首值）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
105: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/bloodyrave/bloodyrave.nut", "swordman_bloodyrave", 43, -1);
```

- 状态名 `swordman_bloodyrave`、**状态号 43、技能 ID -1（不绑定特定技能）**——老一代技能：技能 79 → 状态 43 的映射在引擎内（L2 第 4/5 参区分又一例）。
- `swordman_header.nut:233-234`：`CUSTOM_ANI_BLOODYRAVEINHALE <- 63`、`CUSTOM_ANI_BLOODYRAVESLASH <- 64`（白名单内无任何 nut 引用——引擎按槽位直取）。
- ⚠ **本技能是 F3b 半内置 + mod 污染双重特例**：
  1. `bloodyrave.nut`（65 行）**变量名全部混淆**（C3 mod 水印作者风格），且**只有边角回调**（无 onSetState/onProc 主流程）——吸附/按住/拉拽/伤害递增主逻辑全在引擎。
  2. `passiveobject\character\swordman\bloodyrave.obj` 的 [name] 是 **`技能修改:剑圣`**（mod 作者改写），basic motion/attack info 指向 **`../../script_sqr_nut_qq506807329/...`（该目录在 pvf 内不存在——死引用）**。
  3. nut 操作的 PO id 是 **20041**，而 `passiveobject.lst` 实测 `BloodyRave.obj = 20042`（lst 行 11216）；**20041 = EpidemicRasaCreater.obj，其 [name] 竟是 `邪光斩`**（050-GrandWave 同族 mod 混淆命名）——mod 作者把嗜魂封魔斩的收尾与自家邪光斩区域 PO 做了联动，属**跨技能污染**，非原版行为。
  4. **原版数据全部幸存**：动画/atk 原路径文件俱在（§3），vanilla 行为按幸存文件重建。

### 2.2 主 nut 逐回调（bloodyrave.nut，65 行，mod 混淆——语义还原）

- `onAfterSetState_swordman_bloodyrave`：进入状态 43 时若写包向量[0]==1（推断=斩击子段）→ 攻速 ×1.5（`sq_SetStaticSpeedInfo`）。
- `onKeyFrameFlag_swordman_bloodyrave`（flag **65534**）：写 word 1 → `sq_SendChangeSkillEffectPacket` 自发效果包。
- `onChangeSkillEffect_swordman_bloodyrave` case 1：遍历自己名下 **PO 20041**、仍在第 0 帧的 → `setCurrentPos(自身位置)` + 同向 + `setFrameDelay(0,0)` + `sq_Rewind`（把等待中的 PO 瞬移到自己脚下并从头播放）。
- `onEndState_swordman_bloodyrave`：离开状态 43（切到的不是 43）→ 销毁自己名下仍停在第 0 帧的 PO 20041。
- 即：**65534 = 收尾斩触发标记**（与 .als 的 Slash_01@F7 同帧）；mod 版把它接到邪光斩区域 PO 上。原版语义推断：斩击帧把"血气之剑"PO 拉到当前位置起播（未考证）。

### 2.3 引擎主流程重建（数据文件三方印证）

**① 吸附段（inhale）**：引擎播 `BloodyRaveInhale.ani`（etc motion #63，10 帧 870ms）：
- **F6-F9 连续 4 帧攻击盒** `26 -30 17 62 60 113` → x∈[26,62] y∈[-30,60] z∈[17,113]——身前窄小盒=吸附判定（031 Grab.ani 同型：窄盒定住式命中）；
- 命中反应 `BloodyRave.atk`（etc attack info #53，实测）：**damage reaction = none、push 0、lift 0**——纯"定住/标记"命中，被打中者被拉入吸附（无打击反应，与 031 的 [stuck] 思路一致）；
- 命中的敌人被引擎持续**拉向施法者近身**（吸附演出；按住技能键持续、每秒扣 col6×0.1 HP）；
- `.als` 边车 9 层钩子（F0 起台风起势 ×7 + Loop×2）——吸血旋涡视觉（§2.4）。

**② 斩击段（slash）**：松开键/按满后引擎播 `BloodyRaveSlash.ani`（etc motion #64，11 帧 660ms）：
- **F7 = flag 65534**（斩击触发）；`.als` 4 层：Charge_01@F0、Charge_02/03@F1、Slash_01@F7（血气巨剑蓄力+挥斩）；
- 角色 .ani 无攻击盒——伤害判定由 PO 承担（下条）。

**③ 终结 PO（血气之剑斩击，vanilla=20042/BloodyRave.obj）**：幸存数据：
- `bloodyravefinish_03.ani`（8 帧 640ms，**F0-F3 攻击盒** `x∈[-62,303]/[-4,289]/[14,237]`、z 至 248——身前约 3×2.5 单位大范围）+ `.als` 2 层（Finish_01/02 叠加）；
- 另有 `bloodyrave.ani`（6 帧 480ms，F0-F2 攻击盒 x∈[-4,289]）与 `bloodyravelayer.ani`（纯视觉层）——mod 改写前的原始多相位素材（原版 .obj 结构已不可考）；
- PO 侧 atk `passiveobject\...\attackinfo\bloodyrave.atk`（实测）：**down 击倒 / push 500 / lift 300 / cut+blood 60**——终结斩的击飞手感；
- 伤害 = 吸附数加成：base col0（2314%）按吸附数线性升至 col1（10428%）（吸附数 ≥ static[4]=8 时满额；explain 语义+模板对位，引擎内插值公式未考证）；头目/稀有/精英/深渊类另吃 static 分类增伤（未考证）；
- 出血：col2 22.8% 机率 / col3 Lv45 / col4 5 秒 / col5 攻击力（引擎写入 ACTIVESTATUS_BLEED 同构链路，无 nut 佐证）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\animation\BloodyRaveInhale.ani`（etc #63） | 10 | 870ms（150+80×9） | 无 | **F6-F9** | 吸附判定；sm_body 图集；另有 [pvp] 变体；.als 9 层（new_Start1/2@F0/F2、TyphoonStart_01-05@F5-F7、new_Loop1/2@F5） |
| `BloodyRaveSlash.ani`（etc #64） | 11 | 660ms（60×11） | **F7=65534** | 无（判定在 PO） | 斩击；.als 4 层（Charge_01@F0、Charge_02/03@F1、Slash_01@F7） |
| PO `bloodyravefinish_01.ani` | 2 | 480ms | 无 | 无 | 闪光层 |
| PO `bloodyravefinish_02.ani` | 7 | 560ms | 无 | 无 | 中间层 |
| PO `bloodyravefinish_03.ani` | 8 | 640ms | 无 | **F0-F3**（x 至 303） | 主判定层（.als 叠 _01/_02） |
| PO `bloodyrave.ani`（animation 根） | 6 | 480ms | 无 | F0-F2（x 至 289） | 疑原版另一判定相位（mod 前素材，未考证） |
| PO `bloodyravelayer.ani` | 6 | 480ms | 无 | 无 | 纯视觉层 |
| 特效 `new_Loop1.ani` / `new_Start1.ani` / `bloodyravetyphoonstart_01.ani` | 12/8/12 | 960/640/480ms | 无 | 无 | **[LOOP] 循环**——吸附持续视觉（按住期间不停） |
| 特效 `bloodyraveslash_01.ani` / `bloodyravecharge_01.ani` | 4/7 | 240/420ms | 无 | 无 | 斩击/蓄力层 |

`.als` 节面：全部 `[use animation]` + `[none effect add]`（已支持，L12）；无变体节。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BloodyRave.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\BloodyRave.skl` | ✅ 实测 | 7 列等级数据 + static 12 值 |
| 注册行 | swordman_load_state.nut:105 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 43 / 技能 -1 |
| 常量 | swordman_header.nut:233-234 | `…\pvf\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | 槽位 63/64 |
| 主 nut | bloodyrave.nut | `…\pvf\sqr\character\swordman\bloodyrave\bloodyrave.nut` | ✅ 实测（65 行，**mod 混淆**） | 边角回调；主流程引擎内置 |
| PO 定义 | bloodyrave.obj | `…\pvf\passiveobject\character\swordman\bloodyrave.obj` | ⚠ **mod 改写**（name"技能修改:剑圣"、指向死路径） | vanilla id=20042（lst 实测）；nut 里的 20041=邪光斩 PO（污染） |
| PO 定义 | bloodyrave_ds.obj | 同目录 | ✅ 存在（未读） | 剑影变体（本职业不用） |
| .chr 条目 | etc motion #63/#64（行 1036/1037）+ etc attack info #53（行 1347） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 两段动画 + BloodyRave.atk |
| 角色 .ani | BloodyRaveInhale.ani（+[pvp]）/ BloodyRaveSlash.ani | `…\pvf\character\swordman\animation\` | ✅ 实测 | §2.4 帧表 |
| 角色 .atk | bloodyrave.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | 吸附定住命中（none/0/0） |
| 角色 .als | BloodyRaveInhale.ani.als / BloodyRaveSlash.ani.als | `…\pvf\character\swordman\animation\` | ✅ 实测 | 9 层 + 4 层钩子 |
| PO .ani | bloodyravefinish_01/02/03.ani（+_03.als、[pvp] 变体） | `…\pvf\passiveobject\character\swordman\animation\bloodyrave\` | ✅ 实测 | 终结斩三相位 |
| PO .ani | bloodyrave.ani / bloodyravelayer.ani | `…\pvf\passiveobject\character\swordman\animation\` | ✅ 实测 | 疑原版相位/视觉层 |
| PO .atk | bloodyrave.atk | `…\pvf\passiveobject\character\swordman\attackinfo\bloodyrave.atk` | ✅ 实测 | down/push500/lift300（与角色侧同名文件两份，L3） |
| 特效 .ani | 38 个（typhoon/charge/slash/loop/new_* 等） | `…\pvf\character\swordman\effect\animation\bloodyrave\` | ✅ 实测（ls） | 吸附旋涡/蓄力/斩击视觉 |
| 装备层 | bloodyrave* ×152 | `…\pvf\equipment\character\swordman\avatar\`（find 计数） | ✅ 实测（存在性） | avatar 变体图层 |
| 关联强化 | bloodyraveex.skl（feature skill index 158） | `…\pvf\skill\Swordman\bloodyraveex.skl` | ✅ 存在 | E 批另行分析 |

## 4. 资源需求

img 推导 NPK 规则：`sprite_<img所在路径下划线化>.NPK`（01§2 Step 4）。

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body%04d.img | sprite_character_swordman_equipment_avatar_skin.NPK | 两段角色动画（已入库单图集） | 必需（共享） | ✅ |
| `Effect/BloodyRave/start-normal.img`（new_Start2 用；new_Start1 用 start-dodge.img） | sprite_character_swordman_effect_bloodyrave.NPK | 吸附起势 | 必需 | ❌ |
| `Effect/BloodyRave/loop-normal.img`（new_Loop1 用，[LOOP] 循环） | 同上 | 吸附持续旋涡 | **必需**（技能身份视觉） | ❌ |
| `Effect/BloodyRave/slash_normal.img`（bloodyraveslash_01 用） | 同上 | 斩击弧光 | 必需 | ❌ |
| `Effect/BloodyRave/wind_charge.img`（bloodyravecharge_01 用） | 同上 | 蓄力层 1 | 必需 | ❌ |
| `Effect/BloodyRave/sword_charge_normal.img`（charge_03 用）+ `sword_charge_dodge.img`（charge_02 用） | 同上 | 蓄力层 2/3 | 必需 | ❌ |
| `Effect/BloodyRave/finish_normal.img` / `finish_dodge.img`（finish_01/02/03 用） | 同上 | 终结斩 PO 三层 | 必需 | ❌ |
| `Effect/BloodyRave/lslash-normal.img` / `lslash-dodge.img` / `light.img`（bloodyrave.ani/bloodyravelayer.ani 用） | 同上 | 疑原版相位/视觉层 | 可选 | ❌ |
| `start-dodge.img`、`loop-dodge.img`、`line-normal/dodge.img`、`scrach.img`、`particle.img`、`casting_end_dodge.img`、`end-normal.img` | 同上 | 各类叠加/变体层 | 可选 | ❌ |
| `Effect/Frenzy/blood-energy.img` | sprite_character_swordman_effect_frenzy.NPK | 血气能量（effect 目录某 ani 引用，跨技能借图 L14 同型） | 可选 | ❌ |
| img 版本红线（v2/v4 可/v5 不可）由提取时把关 | | | | |

缺失 img：必需级 7 张（逐 ani 实测归属，见行内标注）、可选级 9 张，主体集中在 `sprite_character_swordman_effect_bloodyrave.NPK`（一次提取全覆盖）。

## 5. 实现方案草案（⛔ 级正式方案免——以下为"定身+斩击"深简化近似，供立项评估，同 031 惯例）

**我们侧若要做，需要什么**（对照两侧）：

| DNF 机制 | 需要的系统 | 现状 |
|---|---|---|
| 吸附：命中敌人持续被拉向施法者（按住键维持） | **位移他人门面**（R2-A8 记档：拉拽/牵引类）——受击者位置被施法者牵引 | ❌ 缺失 |
| 吸附数 → 斩击伤害线性加成（col0→col1，≥8 满额） | 目标枚举/计数门面（对区内敌人去重计数） | ❌ 缺失（MeleeHit 只结算不返回名册） |
| 按住技能键持续吸附（channel 输入） | 按住型输入门面（077 §8 同源缺口：技能中读按键状态） | ❌ 缺失 |
| 血之狂暴状态门禁（未开狂暴不可放） | Buff 查询门面 | ❌ 缺失（R1-A3 记档） |
| 头目/稀有/精英/深渊分类增伤 | 单位属性位（稀有度标记） | ❌ 缺失 |
| 吸附每秒扣 HP | ConsumeCasterHp（已有）+ 持续段计时 | ✅ 可表达 |

**深简化近似（不做拉拽，做"定身聚怪+延迟大斩"）**——机制近似度约 50%（伤害节奏与时序保留，"拉到脸前"降级为"原地定住"）：
- `BloodyRaveSkill : SkillLogic`（BloodBoomSkill 帧触发范式 + 008 连段时间轴）：
  - OnCast：`ctx.PlayAnim(AnimId.SwordmanBloodyRaveInhale)` + `ctx.ClearHitTargets()`；`CooldownMs=30000`、`TotalTimeMs=870+660=1530`（demo 把"按住"固定为 870ms 吸附窗）。
  - OnUpdate 吸附窗（F6 起）：`ctx.CreateAreaInFront(AreaIds.BloodyRaveHold, 0.9)`——**吸附区**：`TotalTimeMs=870`、`TickTimeMs=200`、**`TickActions={MeleeHit, AddHoldBuff}`**（AreaDefinition 实测字段：EnterActions/TickActions 并存，Tick 无去重=持续刷新，L19/R2-A8 用法），MeleeHit 用"零伤版 HitReaction"（Damage=1/HitstunMs=200/Kb 0/Ly 0，模拟 BloodyRave.atk 的 none 反应），AddHoldBuff 挂 **BloodyRaveHoldBuff**（新 Buff：800ms、AddActions={ForbidMoveOn}——FreezeBuff 的 ForbidMoveOn/Off 范式复刻，"假吸附"）。
  - OnUpdate t≥870：`ctx.PlayAnim(AnimId.SwordmanBloodyRaveSlash)` + SubState=1（斩击段）。
  - OnUpdate t≥870+420（Slash F7=65534 同时刻）：`ctx.CreateAreaInFront(AreaIds.BloodyRaveFinish, 1.5)`——**终结区**：`TotalTimeMs=640`、`EnterActions={MeleeHit, AddBleedBuff}`、`HalfExtents=(1.8,0.5,1.2)`（finish_03 盒 x至303 折算）、`HitReaction{Damage=260, HitstunMs=800, KnockbackX=500, LaunchY=300, ProcBuffId=BuffIds.Bleed, ProcChance=23}`（PO atk 原值 push500/lift300 + 出血 22.8%）、`ViewAnimId=AnimId.BloodyRaveFinish`（finish_03+als 三层）。
  - OnEnd：`ctx.PlayDefaultAnim()`。
- 吸附数伤害加成：无计数门面 → demo 固定中档伤害（上表 260 即 col0-col1 折中）；立项"目标枚举门面"后回补。
- 注册点（草案号段，A11 批内）：`SkillIds.BloodyRave=19`、`AreaIds.BloodyRaveHold=9 / BloodyRaveFinish=10`、`AnimIds 77-80`（Inhale/Slash/Finish/TyphoonLoop）、`BuffIds.BloodyRaveHold=9`；json：swordman_bloodyrave_inhale/slash + bloodyrave_finish + new_loop1（LOOP 动画）；图集：bloodyrave 必需 7 张（§4）。

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 30000 ms | 30000（直用；demo 可临时降） |
| 吸附窗 | 870ms（inhale.ani；按住可延长，上限未考证疑 static[1]=3000） | 固定 870ms |
| 吸附盒 | F6-F9 x∈[26,62] z∈[17,113]（窄前盒） | Area 前偏 0.45、半尺寸 (0.2,0.45,0.5) 再放宽到 (0.9,0.45,0.9)（可玩性） |
| 吸附命中反应 | BloodyRave.atk：none/push0/lift0 | 零伤 Hitstun 200ms + HoldBuff |
| 每秒 HP 削减 | col6×0.1 = 2.3/秒 | 吸附窗共扣 5（ConsumeCasterHp 两次） |
| 斩击触发 | Slash F7（870+420ms） | 同帧 |
| 终结伤害 | col0 2314% → col1 10428%（吸附数加成） | 固定 260 |
| 终结反应 | PO atk：down/push500/lift300/cut+blood60 | Hitstun 800/Kb 500/Ly 300 |
| 出血 | 22.8%/Lv45/5s/col5 | BleedBuff 现值（ProcChance 23） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BloodyRave.skl | `.skl` 无子命令（7 列 + static 12 值） | 手抄可行；并入既有缺口 |
| 3 份 .atk（角色 1 + PO 1 + 邪光斩 PO 1 不涉及） | `.atk` 无子命令 | 手抄（每份 ~8 值）；并入既有缺口 |
| bloodyrave.obj（mod 改写版） | `.obj` 无子命令 + **指向 pvf 外死路径** | 不采信该 obj（mod 产物）；vanilla 结构已由 finish_01/02/03 + loose 文件反推，手工映射 Area 即可 |
| 全部 .ani（含 [LOOP] 循环的 new_loop1 等） | 常规节（FRAME/DELAY/IMAGE/LOOP），无规则外节 | **现有 ani 子命令全覆盖**（[LOOP] 已译） |
| 两份 .als | [use animation]+[none effect add] 均已支持 | 无缺口 |

本技能翻译缺口：`.skl`/`.atk`/`.obj` 三类既有（计 3 条），无新节。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 吸附拉拽（敌人被持续拉向自身） | **位移他人门面（缺失，R2-A8 记档）+ 抓取系（缺失，031 立项依据）** | §5 定身近似（ForbidMove 假吸附） |
| 按住技能键持续吸附（channel） | 按住型输入门面（缺失，077/067 同源） | 固定 870ms 吸附窗 |
| 吸附数 → 伤害线性加成 | 目标枚举/计数门面（缺失，新记 §8） | 固定中档伤害 |
| 头目/稀有/精英/深渊分类增伤 | 单位属性位（缺失） | 跳过 |
| 血之狂暴状态门禁 | Buff 查询门面（缺失） | demo 无门禁直放 |
| 出血（22.8%/5s） | 已支持（ProcBuffId+ProcChance+LSRng） | 直用 |
| 吸附每秒扣 HP | ConsumeCasterHp 已有 | 直用 |
| 台风旋涡 LOOP 视觉按住期间持续 | Area 视图循环动画（FireCircle 先例） | Hold 区 ViewAnimId 用 LOOP 动画 |
| 65534 收尾标记 / PO 拉回重播（mod 版） | mod 污染行为，非原版 | 不还原（记档） |
| 等级缩放（7 列） | 延后 | 固定值 |
| 音效（MINERALSWDC_HIT_01 等） | 延后 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. static data 12 值中除 col4=8 外的语义（500/3000/180/55/250/4/5/6/6/6/0 全部推断）。
2. level property 向量源 **-4**（col5 出血攻击力）——L21 已知 -1/-2=level 列、≥0=static 槽，-4 为新见，语义未考证（建议主循环并入 L21 收尾统一）。
3. 吸附数→伤害的插值公式（线性？分段？引擎内置）；头目/稀有增伤档位与 static col6-10 的对应。
4. 原版 PO 20042 的多相位结构（.obj 被 mod 改写，finish_01/02/03 与 loose bloodyrave.ani/bloodyravelayer.ani 的原始编排不可考）。
5. 引擎"按住延长"的精确机制（吸附上限时长疑 static[1]=3000ms，未实证）。

**mod 污染记档（C3 补充）**
- `bloodyrave.nut` 变量混淆 + `bloodyrave.obj` 改名"技能修改:剑圣" + 死路径 `script_sqr_nut_qq506807329/`（pvf 内不存在）；**nut 中 PO 20041 实为邪光斩 PO（EpidemicRasaCreater.obj，[name]=`邪光斩`）**——嗜魂封魔斩与邪光斩在 mod 中做了跨技能联动，走读时勿把 20041 当本技能 PO。

**新系统级缺口（§6.3 清单外）**
1. **目标枚举/计数门面**：吸附数伤害加成（本技能）与"命中 N 个敌人触发 Y"类技能都需要"对区域内敌人去重计数"的查询面；MeleeHit 只结算不返回名册。建议随"位移他人门面"一并评估立项。

**翻译工具缺口**：`.skl`/`.atk`/`.obj`（既有三类，无新增）。

**给下轮的经验**：狂战士"吸血/吸附"类技能（本技能、35 怒气爆发系）的判定模式=角色窄盒+none 反应 atk（定住标记）+ PO 大盒终结——与 031 抓取同构；走读先查 `bloodyrave` 目录 + `.chr` 槽 63/64 + `passiveobject.lst` 反查 obj id（本例揭示 **lst id ≠ nut 引用 id 时必有一方是 mod 污染**，lst（11216 行=20042）比混淆 nut 可信）。
