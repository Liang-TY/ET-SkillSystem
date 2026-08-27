# 灭魂之手（GrabBlastBloodEx）

> 技能ID 102 | 级别 E（**预分类纠偏：非 TP 强化被动——[type] active、skill class 2 抓取类，狂战士二觉替换型主动技**，按 B 类深度走读；同目录 165-GrabBlastBloodExp 才是"强化-嗜魂之手"TP 被动。031 §2.1 当年"102=TP 强化版"的记法即本技，R7-E3 已纠偏为独立技，本文给全量走读） | 可实现性 ⛔（抓取/目标控制系统缺失——完全继承基础技 031；R7-E3 纠偏的第 4 定性证据） | 分析日期 2026-08-22 | 批次 E5

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 灭魂之手 | `skill\Swordman\GrabBlastBloodEx.skl` [name] |
| 英文名 | GrabBlastBloodEx（取 skl 文件名；[name2] 实测 `Bloodruin`） | 同上 |
| 职业 | 狂战士二觉（[second growtype maximum level] 12 槽第 6/7 位（0 基）= **30 级**——(6,7)=狂战对，R6-C4；[skill fitness growtype] 空） | 同上 |
| 学习等级 | 60（[required level range] 2）；前置 **79 鲜血暴掠 Lv1**（[pre required skill] 实测；官方语义疑为 31 嗜魂之手，本 pvf 数据如此） | 同上 |
| 最高等级 | 50（[maximum level]；二觉档实际 30） | 同上 |
| 类型 | active（**skill class 2 = 抓取类**，与基础嗜魂之手同类） | 同上 [type]/[skill class] |
| 指令 | →←↓→ + Z（[skill command advantage] 50/50） | 同上 [command] |
| CD | 30000 ms | 同上 [dungeon][cool time] |
| MP | 400 → 800（Lv1→Lv50） | 同上 [consume MP] |
| 特殊消耗 | 道具 3037 ×1（无色）；耐久 20；屏震 [shake screen] 2 400 | 同上 |
| static data | `1200 500 200 50 3 400 8`——[0]=1200/[1]=500 与基础技 static 同位（推断：抓取窗口/结束后无敌时长 ms）；[2]=200→**吸入多段间隔 0.2s**、[3]=50→**喷发多段间隔 0.05s**、[4]=3→**喷发次数上限**（模板实证）；[5]=400/[6]=8 未考证 | 同上 + level property |
| 一句话效果 | 抓取前方一名敌人吸血气（多段）再喷发（多段 3 次）造成物理伤害并溅射身后敌人；成功获得力量 Buff；对出血敌人增伤且力量时间更长 | 同上 [explain] |
| 与基础技关系 | [pre required skill] 79 单向依赖；基础 GrabBlastBlood.skl [feature skill index]=165（TP 版）——与 102 无链接 | 两 skl 实测 |

**level property 模板解码（8 列，模板 10 占位符 vs 11 向量——1 向量悬空，L21 法 10/11 解）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 血气吸入持续时间 | (-1,6,0.001) | col6 = 1000 恒 → **1.0 s** |
| 血气吸入物理攻击力 | (-2,0,1.0) | col0 = 450 → 5091 % |
| 血气吸入多段攻击间隔 | (2,2,0.001) | static[2]=200 → 0.2 s |
| 喷发时物理攻击力 | (-2,1,1.0) | col1 = 2250 → 25461 % |
| 喷发时多段攻击间隔 | (3,3,0.001) | static[3]=50 → 0.05 s |
| 喷发时多段攻击次数上限 | (4,4,1.0) | static[4]=3 → 3 次 |
| 增加力量的持续时间 | (-1,2,0.001) | col2 = 10000 恒 → **10 s** |
| 增加力量 | (-1,3,1.0) | col3 = 200 → 690 |
| （抓出血时）物理攻击力 | (-2,4,1.0) | col4 = 225 → 2546 % |
| （抓出血时）力量持续时间 | (-1,5,0.001) | col5 = 20000 恒 → **20 s** |
| **第 11 向量悬空** | (-1,6,0.001) | 与首行同参重复、无占位符对应；col7=500→1480 成长列**无模板引用**（疑喷发溅射/无敌参数，未考证） |

pvp 表：col0/col1 大幅缩水（25→515 / 75→1545），col2-7 同结构。

## 2. 技能逻辑走读

### 2.1 注册与文件链（共用 GRABHAND 状态 26——031 同链）

无独立注册：基础技 31 的注册行（load_state:128）即入口——
`pushState(..., "character/swordman/grabhand/grabhand.nut", "GRABHAND", 26, 31)`（L2：状态 26/技能 31）。
**技能 102 复用同一状态机**，靠包内技能 ID 区分（031 §2.1 早已发现共用，当年误标"TP 版"）。

### 2.2 主 nut（grabhand.nut 102 分支，实测原文语义）

- `onAttack_GRABHAND`（抓取尝试命中回调）：

```
若 (子状态 0 && 技能==102 && 目标==-1) 且目标不可抓（sq_IsHoldable/sq_IsGrabable/sq_IsFixture 三判据）：
    createGrabBloodHandunGrabEffect(obj, 42, 1, 85)   // 播"抓空"血雾特效（复用基础技表现）
    记录 damager → 切子状态 1 → 写包重进状态 26 (1, 102, 34675045)
    // 34675045 = 0x02100005 疑打包参数（基础技 31 分支同位是 -1），未考证
可抓 → 引擎接管成功分支（无脚本，同基础技）
```

- `onAfterSetState_GRABHAND` 子状态 1（抓空收势）：`sq_SendHitObjectPacket` ——对不可抓目标仍结算一次普通命中（用 .chr #71 的 GrabBlastBloodEx.atk：none 反应/push0/lift0/stuck -1000——"定住"命中，与基础技 #13 同构）。
- `onEndCurrentAni_GRABHAND`：无敌开关两消息（500ms 窗口，static[1] 同族）。
- 成功分支全在引擎：抓取演出（双方同步）→ 吸入阶段（1s × 0.2s 间隔多段，col0 伤害）→ 喷发阶段（3 次 × 0.05s 间隔，col1 伤害 + 引擎创建终结 PO）→ 力量 buff（col2/col3；出血敌人走 col4/col5）。

### 2.3 被动对象（grabblastbloodex.obj，实测）

| .obj 节 | 值 | 说明 |
|---|---|---|
| [name] | `怒气爆发抓取时的打击`（文案沿用怒气爆发——与基础技 grabblastblood.obj 同款命名，官方数据如此） | — |
| [floating height]/[pass type]/[piercing power] | 1 / pass all / 1000 | 全穿透多目标 |
| [basic motion] | `Animation/GrabBlastBloodEx/exp_blood_normal.ani`（10 帧 800ms，**全帧攻击盒**） | 喷发打击主体 |
| [etc motion] | `exp_blood_normal_teen.ani`（10 帧 800ms 全帧盒，teen=异界贴图变体） | 第二相位 |
| [attack info] | **PO 侧** `AttackInfo/GrabBlastBloodEx.atk`（与角色侧同名两份，L3/L9 双印证）：**down / push 500 / lift 100 / blow / no blood 50** | 喷发击飞 |

喷发多段（3 次×50ms）由引擎重置命中表实现（PO 800ms 全帧盒 > 3×50ms 窗口）。同目录其余动画（exp_blood_dodge/around_dodge 8 帧、exp_light_back/front_dodge 6 帧，均带攻击盒计数但未注册进 .obj）经 .als [add] 叠层消费（引擎绘制层）。

### 2.4 动画关键帧表（抽关键件实测）

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img | 备注 |
|---|---|---|---|---|---|---|
| `character\swordman\animation\Grab.ani`（.chr etc #10，**复用基础技抓取动作**） | 17 | 640ms | F8=100、F15=65534 | F7-F14（031 已测 `8 -19 47 80 38 67`） | sm_body | 抓取尝试姿态（本技无专属角色动画） |
| `passiveobject\...\grabblastbloodex\exp_blood_normal.ani` | 10 | 800ms | 无 | 全帧 | `Effect/GrabBlastBloodEx/exp_blood_normal.img` | 喷发主体（.als 叠层） |
| exp_blood_normal_teen.ani | 10 | 800ms | 无 | 全帧 | 同图 | teen 变体（.als） |
| exp_blood_dodge / around_dodge.ani | 8 | — | 无 | 有 | exp_blood_dodge / exp_blood_around_dodge.img | .als 叠层 |
| exp_light_back/front_dodge.ani | 6 | — | 无 | 有 | exp_light_back/front_dodge.img | 光效层 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GrabBlastBloodEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GrabBlastBloodEx.skl` | ✅（242 行全读） | 8 列 10/11 解 |
| lst 条目 | ID 102 | `…\pvf\skill\swordmanskill.lst` 373-374 行 | ✅ | — |
| 注册行 | load_state:128（GRABHAND/26，技能 31 共用） | `…\pvf\sqr\character\swordman_load_state.nut` | ✅（031 已证） | 状态机复用 |
| 主 nut | grabhand.nut（102 失败分支） | `…\pvf\sqr\character\swordman\grabhand\grabhand.nut` | ✅ 全读 | §2.2 |
| ap nut | ap_grabhand.nut | 同目录 | ✅（031 已读 41 行） | 抓取持有（回待机即放） |
| .chr 条目 | etc motion #10 Grab.ani（复用）；etc attack info #71 GrabBlastBloodEx.atk（行 1365） | `…\pvf\character\swordman\swordman.chr` | ✅ 实测 | 抓取动作/定住命中 |
| 角色 .atk | grabblastbloodex.atk | `…\pvf\character\swordman\attackinfo\` | ✅ 实测 | none/0/0/stuck -1000 |
| PO 定义 | grabblastbloodex.obj | `…\pvf\passiveobject\character\swordman\` | ✅ 实测 | §2.3 |
| PO .atk | grabblastbloodex.atk（PO 侧） | `…\pvf\passiveobject\character\swordman\attackinfo\` | ✅ 实测 | down/push500/lift100 |
| PO .ani/.als | grabblastbloodex\ 12 文件 | `…\pvf\passiveobject\character\swordman\animation\grabblastbloodex\` | ✅ 实测 | §2.4 |
| 抓空特效 | bloodlustgrabcannon_00.ani + .als | `…\pvf\character\swordman\effect\animation\grabblastblood\` | ✅（031 已读） | 失败分支复用 |
| 基础技文档 | 031-GrabBlastBlood.md | 本目录 | ✅ | 抓取系统拆解基准（§5 立项依据） |
| 同名 TP 技 | 165-GrabBlastBloodExp.md | 本目录 | ✅ | TP 版（交叉引用） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| sm_body0000.img | sprite_character_swordman_equipment_avatar_skin.NPK | Grab.ani 抓取动作 | 必需（共享） | ✅ |
| `Character/Swordman/Effect/GrabBlastBloodEx/exp_blood_normal.img` | sprite_character_swordman_effect_grabblastbloodex.NPK | 喷发打击主体 | **必需** | ❌ |
| `…/GrabBlastBloodEx/exp_blood_dodge.img`、`exp_blood_around_dodge.img` | 同上 | 喷发叠层 | **必需** | ❌ |
| `…/GrabBlastBloodEx/exp_light_back_dodge.img`、`exp_light_front_dodge.img` | 同上 | 光效层 | 可选 | ❌ |
| （抓空血雾 bloodlustgrabcannon 系） | sprite_character_swordman_effect_grabblastblood.NPK | 失败分支（与 031 共享） | 可选 | ❌（031 档已记） |

**缺失 img：必需 3 张（同一 NPK 一次提取）、可选 2+ 张。** ⛔ 期间挂起。AnimRes 实测均未入库。

## 5. 实现方案草案（⛔ 级正式方案免）

完全继承 031 §5"定身连招"深简化近似框架，仅数据面换 Ex：

| DNF 机制 | 我们现状 | 阻断点 |
|---|---|---|
| 抓取目标控制（同 031：控住/牵引/双人演出） | ❌ 抓取/目标控制系统 | 缺失档（031 §5 表格完整立项依据） |
| 吸入 1s × 0.2s 多段 + 喷发 3 次 × 0.05s 多段 | 多段=Area Tick 可表达；但宿主演出被 ⛔ 挡在前面 | 随抓取 |
| 力量 buff 10s/20s（col2/3/5） | 属性数值无消费链（R1-A4） | 缺失档 |
| 对出血敌人增伤（col4） | Buff 查询门面（R1-A3） | 缺失档 |
| 抓空分支 + 500ms 无敌 | 无敌帧（R1-A5）；抓空特效链 ✅ 可复用 | 部分可 |

若按 031 §5 近似落地：`GrabBlastBloodExSkill`（CD 30000、Grab.ani 640ms 定身近似）+ 吸入段（1s 定身 + 5 跳 0.2s tick）+ 喷发区（PO atk down/push500/lift100 直译 + 3 跳 0.05s）——伤害节奏完整，控住演出降级为定身（近似度同 031 的 60% 档）。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| GrabBlastBloodEx.skl | `.skl` 无子命令（8 列 + static 7 值 + 11 向量/10 占位符错配） | 手抄 §1；**skl 子命令解析需容错"向量数 ≠ 占位符数"**（本批首见悬空向量） |
| 角色/PO 两个同名 grabblastbloodex.atk | `.atk` 无子命令 | 手抄（两份各 ≤8 值，L3 双表常态） |
| grabblastbloodex.obj | `.obj` 无子命令（basic+etc 两相位 + 攻击盒全帧） | 手工映射（§2.3 已给；L9 多相位建模） |
| 全部 .ani/.als | 常规节（SHADOW 记档跳过） | **现有 ani/als 子命令全覆盖** |

翻译缺口计 3 条（.skl/.atk/.obj——常驻）+ skl 悬空向量容错 1 条新输入。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 抓住敌人吸血（目标控制/双人演出） | **抓取/投掷 Grab 系——缺失档**（031 §5 完整拆解） | ⛔ 主因；定身连招近似（031 §5） |
| 可抓性三判据 | 单位属性位缺失 | demo 全可抓（031 同结论） |
| 吸入/喷发多段节奏 | 多段命中重置（L19——同段定时可 Tick 表达） | 若做近似版：0.2s/0.05s tick 直用 |
| 喷发溅射身后敌人（explain） | 无方向性多区（可两个 Area） | 近似版可双区（前后各一） |
| 力量 buff/出血增伤 | 属性消费链 + Buff 查询门面（双缺失档） | 跳过 |
| 抓空第三参 34675045 | 未考证（打包参数） | 不实现 |
| 屏震 2 400 / 无色 / MP | 延后档 | 跳过 |

## 8. 存疑与缺口上报

**未考证项**
1. col7（500→1480 成长列）与悬空第 11 向量的关系（疑喷发溅射伤害/无敌时长参数，无占位符无 nut 消费）。
2. static[5]=400、static[6]=8 语义（400 疑无敌时长与 static[1]=500 冲突待辨；8 疑溅射目标数/演出帧参）。
3. 写包第三参 34675045（0x02100005）的打包语义。
4. teen 变体（exp_blood_normal_teen）的选择条件（疑异界/染色模式）。
5. [pre required skill] 79（鲜血暴掠）而非 31（嗜魂之手）——本 pvf 前置树改写（三 Ex 同现象，见批次总评）。

**缺口上报**：无新系统级缺口（抓取系统 031 §5 已完整立项依据；多段 tick 已有 L19 通道）。

**预分类纠偏上报（主循环记账）**：**102 定性第 4 次实证收口**——R7-E3 已把 031 §2.1 的"102=TP 强化"记法纠偏为独立技，本文给出全量走读（skill class 2 抓取类 + 独立 [pre required skill] + 基础 skl feature 链指向 165 不指向 102）。031 文档相应表述按 R7-E3 勘误维持，无需再改。

**给轮间经验**：二觉替换技的 [pre required skill] 指向**同族前一阶大招**（98←68 同名系、100←22 跨系、102←79 跨系）而非被替换技——"前置=同职业 60 级新链"是本 pvf mod 的常态，判替换关系要看技能语义（同名系/同 class）不能只看前置。
