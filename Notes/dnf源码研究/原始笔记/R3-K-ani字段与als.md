# R3-K：DNF .ani 剩余字段实证 + .als 边车普查 + [SET FLAG] 号惯例

> 第3轮 Agent K 原始笔记（首次运行卡死后唤醒续跑完成）。
> 抽样范围：pvf 根。总量：.ani 784,593 个 / .als 17,283 个。字段值分布主要在 character/swordman/animation + aicharacter 抽样，部分字段扩到 character/*/animation + passiveobject/character 复核。
> 已综合进：08-动画配置ani与als-总结.md

---

## 一、.ani 剩余字段逐个实证

### 1. [GRAPHIC EFFECT] —— 帧级混合模式（枚举，单值）
| 取值 | 出现次数 | 语义 |
|---|---|---|
| LINEARDODGE | 1300 → 95,811 | 线性减淡 = **加法混合**（发光/剑气/灵体特效标配） |
| MONOCHROME | 13 → 322 | 灰度/单色化 |
| DARK | 0 → 24 | 变暗混合 |
| SPACEDISTORT | 0 → 24 | 空间扭曲（极罕见） |

实例：berserk/berserk.ani（狂暴光环，5 帧全 LINEARDODGE）；passiveobject/character/swordman/animation/attackdustfront_08.ani（挥砍白光）。规律：**角色本体帧从不写 GRAPHIC EFFECT，特效/灵体帧大量写 LINEARDODGE**。缺口：没有枚举源码；SPACEDISTORT 渲染行为未知。

### 2. [RGBA] —— 帧级 RGBA 染色（4 值 0-255）
- 实例 1（淡入）：bladephantom_mist1.ani 帧序列 alpha 76→127→153→178→204，配 IMAGE RATE 2 2 + LINEARDODGE，就是"雾气放大淡入"。
- 实例 2（淡出）：earthbreakout3.ani：255 255 255 128 → 255 255 255 0（末帧透明）。
- 实例 3（染色）：普查 19,106 处中非白色大量存在：0 0 0 255（纯黑剪影，1198 次）、255 0 0 255（染红）、0 185 255 255（青色）。
- 结论：RGB=乘性染色，A=透明度，作用于**整个帧精灵**。写全 255 也常见（显式默认值）。
- 我们要用：半透明帧、隐形帧（alpha=0 占位帧）、残影黑剪影、受击闪白/闪红。

### 3. [DAMAGE TYPE] —— 帧级受击状态（普查共 3 值）
| 取值 | 次数 | 语义 |
|---|---|---|
| SUPERARMOR | 1356 | 霸体帧（受击不硬直不击退） |
| NORMAL | 754 | 普通帧（默认；显式写出） |
| UNBREAKABLE | 49 | 不可打断 |

实例：bloodboom.ani 全 23 帧 SUPERARMOR（读条技全程霸体）；throwenemythrow.ani UNBREAKABLE（投掷敌人时投掷者不可被打断）。推断（标缺口）：NORMAL 与不写字段等价；SUPERARMOR/UNBREAKABLE 语义是推断，无引擎源码佐证。**普查范围内确认只有这 3 个值。**

### 4. [IMAGE RATE] —— 帧级缩放（2 浮点：xScale yScale，独立）
取值集合（swordman/animation 全量）：1 1(90)、0.5 0.5(40)、-1 1(25)、0.6 0.6(24)、0.8 0.8、1.2 1.2、2 2、0.9 0.3…
- **负值=镜像确认**：bloodsnatchspin_body.ani FRAME000 对 sm_body 贴图写 -1 1（回身动作把身体帧水平翻转）。
- x/y 可不等：earthbreakout3.ani 的 0.9 0.3（压扁特效）。
- 结论：就是 (scaleX, scaleY)，scaleX<0 = 水平翻转，与朝向翻转叠加。

### 5. [IMAGE ROTATE] —— 帧级旋转（单浮点，**弧度**）
取值集合：0(118)、±0.785398(=π/4=45°，34 次)、-0.610865(≈-35°)、0.03、-0.87、2.09…
实例：bloodsnatchcatch_01.ani FRAME001 IMAGE RATE 0.6 0.6 + IMAGE ROTATE 1.05（手部特效旋转 ~60°）。
结论：单位弧度（0.785398 铁证）。**缺口：正负方向与旋转中心（IMAGE POS 锚点还是帧中心）无静态证据，需运行时验证。**

### 6. [INTERPOLATION] —— 帧间插值开关（单值，普查恒为 1）
379/379 全是 1（二值开关：1=开）。实例：descentsoul_end_ghostbremen_b.ani：帧 0 IMAGE POS (-85,-210) → 帧 1 (-76,-217)，两帧均 INTERPOLATION 1——相邻帧坐标有微小差值，开启插值让移动平滑。推断（标缺口）：插值对象是**帧间 IMAGE POS（及可能的 RATE/ROTATE）连续化**，DELAY 不变。

### 7. [FLIP TYPE] —— 帧级翻转类型（枚举，3 值）
扩大样本：HORIZON(3477)、ALL(1226)、VERTICAL(97)。推断：HORIZON=水平镜像、VERTICAL=垂直镜像、ALL=双向。swordman+aicharacter 样本里仅 1 个文件用到，**低频字段**。缺口：与角色朝向翻转的叠加关系、为何大多数动画不用它，未解。

### 8. [COORD] —— 顶层坐标模式开关（单值，恒 1，极稀有）
位置：文件顶层（[SHADOW] 旁），非帧内。passiveobject/character 下仅 13 处，全部 1。实例：dimensionrunner_a_ball.ani（投射物）、explosion_11.ani（爆炎粒子）——**全是粒子/投射物类**。推断（缺口）：IMAGE POS 改用另一套坐标（如屏幕/绝对坐标而非挂接对象锚点），供不跟随施法者的特效用。

### 9. [SHADOW] —— 顶层投影开关（0/1）
取值：0(13470) / 1(9)。语义：0=关闭脚底阴影，1=开启。**特效/技能动画几乎都显式写 0**；基础动作（move/stay/attack）通常不写 → 默认有影。实例：berserk.ani（光环特效，SHADOW 0）。

### 10. [LOOP] —— 循环开关（普查恒 1；非帧号）
取值：1（swordman 110 个全 1；扩大样本 2340 个 1 + 1 个 255 异常值）。
- **帧号假说被否**：flowmindstay.ani FRAME MAX=1 且 LOOP=1——若 LOOP 是回跳帧号则越界；且从无 2/3 等值出现。
- 结论：LOOP=布尔循环开关，播到末帧后**回到帧 0** 重播。255（≈0xFF 或 -1 编码异常）标缺口。
- 注意：DNF 技能动作大多**不写 LOOP**（播完即停，由状态机切换），LOOP 用于待机/移动/光环等循环动画。

### 11. [SPECTRUM] —— 残影（顶层，含 5 个子节）
完整结构（rorate.ani、bladephantomex_jump_body.ani）：
```
[SPECTRUM]
	1                            ← 总开关
	[SPECTRUM TERM]       80     ← 采样间隔(ms)：每 80ms 截一个残影
	[SPECTRUM LIFE TIME]  400    ← 单个残影存活(ms)：400/80=同时约5个残影
	[SPECTRUM COLOR]  0 0 0 127  ← 残影 RGBA（0 0 0 127=半透明黑影；145 0 255 153=紫色）
	[SPECTRUM EFFECT] `LINEARDODGE`  ← 残影混合模式（可 NONE）
```
term=0（每帧都采样）+ lifetime=10 的极限用法也存在。脚本侧有对应**运行时**API（appendage 挂的动态残影，参数同构）：sq_AddOcularSpectrum / sq_SetParameterOcularSpectrum(spec, 400, 50, true, RGBA, RGBA, ...)，见 ap_stylish.nut:43、ap_common_burster.nut。18 个文件用 SPECTRUM TERM，是**动画自带残影**。

### 12. [PLAY SOUND] —— 帧级音效（帧内，单反引号字符串）
格式：短资源名，如 R_BLADESPIRIT、SM_BOONGSAN、DOUBLEP_SWORD、SM_BLOOD_RAVE_ATK、R_SM_JUMP_ATK（前缀=角色/技能缩写）。实例：attack_bladespirit1.ani 帧内 [PLAY SOUND] R_BLADESPIRIT。**资源不在 pvf 内**：音频在客户端 sound NPK 里按名字索引。缺口：名字→wav 的映射表在客户端侧。

## 二、.als 边车文件全量普查

### 1. 总量与分布
共 **17,283** 个 .als（占 .ani 总数 2.2%）：passiveobject 7064（投射物/特效对象的分层子特效）、character 6751（技能本体动画+施法圈/剑痕等覆盖特效）、monster 1523、creature 631 / equipment 557（称号动画）/ ui 264 / map 251 / common 175 / npc 59 / dungeon 5 / **aicharacter 仅 2** / appendage 1。

### 2. 完整节结构（全库节名统计）
| 节 | 次数 | 语法与语义 |
|---|---|---|
| [use animation] | 97,323 | 两行：子 .ani 相对路径 + 别名字符串。**注册**（不触发） |
| [none effect add] | 61,865 | `<整数1> <整数2>` + 别名。在父动画上叠加播放该子动画 |
| [add] | 33,632 | 同上格式，普通叠加 |
| [create draw only object] | 1,529 | `<帧?>` + 别名 + `<x> <y> <z>` 偏移；生成独立 draw-only 对象 |
| [static data]/[/static data] | 371 | 包裹一段整数数据（透传给消费者） |
| [create draw only object follow parent] | 80 | 同上，跟随父对象移动 |
| [remove] | 55 | `<帧?>` + `<id>` + 别名：移除已叠加的动画 |
| [mask layer] + [add mask layer] | 55+55 | 蒙版层（ui/preyraid 装饰动画用） |
| [create draw only object random] | 27 | 随机生成变体 |
| [create draw only object bottom object] | 13 | 底层对象 |
| [non effect add] | 10 | [none effect add] 的拼写变体 |
| [create draw only object no apply speed] | 8 | 不受攻速影响的版本 |
| [add force effect] | 7 | [add] 强制变体 |
| [create draw only object auto remove state] | 1 | 自动随状态移除 |
| 内嵌 [LOOP]/[FRAME MAX]/[FRAME000..] | 各 1 | trading_jonathan_all.ani.als 一个文件同时是 .ani+边车（孤例） |

**两个整数的语义**（静态推断，证据充分但非引擎实锤）：
- **整数2 = 绘制层 z 序**：符号决定前后、绝对值决定优先级。铁证：momentaryslashex.ani.als 中 cutBottom → -9999（最底）、cutUp → +9999（最顶）；yin_ready.ani.als 把**同一个**特效文件注册两份分别挂 -10000 和 +10000（角色前后夹层辉光）；常规取值 -1..-8（贴身后）、1000、9999、10000+（身前）。全库范围 -10010..10010。
- **整数1 = 触发帧**（父动画帧号，-1 未见明确解释）：抽样全部满足 0 ≤ n < 父 FRAME MAX。[remove] 的整数1 = 移除时刻帧。反例：bladephantom_mist1 的 n=8 恰等于 FRAME MAX（越界或"末帧后"语义，标缺口）。
- **备选假说**（不能排除）：整数2 是事件 id（供 [remove] 匹配）。两假说对所有数据均自洽；cutBottom/cutUp 与 ±9999 的配对是 z 序假说的最强证据。
- [add] vs [none effect add]：抽样中 [add] 多用于带 LINEARDODGE 的辉光特效、[none effect add] 多用于无混合的尘土/刀光——推断 none 变体**不继承父对象的特效状态**（混合/染色），缺口：无实锤。

### 3. 消费机制
- **同名自动配对**：.als 全部以 `<完整名>.ani.als` 形式与 .ani 并存，连 [pvp] 变体都保持配对（bloodswordcharge.[pvp].ani ↔ bloodswordcharge.[pvp].ani.als）。加载 .ani 时引擎自动找同名边车。
- **sqr 脚本侧无显式 .als 引用**：全 sqr/ 目录 grep `.als` 字符串只有 2 个文件、且是**原生函数 `als_ani(obj, "xxx.ani", x, y, z, ?, alpha, ?, bool, rate)`**（thief/flametogaws.nut:1134 起，传的是 .ani 路径——函数名即"带 als 的动画"）；另一个是注释 `//buster_loop_front_normal.ani.als`（ap_common_burster.nut:152，注释上方正是被注释掉的 sq_CreateDrawOnlyObject 手写代码）——即 .als = **把手写 drawOnlyObject/特效叠加声明化**，引擎读边车代替脚本手挂。
- WebSearch 两次（中文+英文）未找到 .als 格式公开文档；仅 17173/arad.ink 确认 .als 属 NPK 内文件、社区无格式细节。本轮为首证。
- 标缺口：引擎侧加载代码（CALSData 等类名未验证）。

### 4. 适用场景（为什么独立成 .als 而不写进 .ani）
- **内容归属分离**：本体动画（角色骨架帧）与特效（effect/animation 的施法圈/剑痕）由不同工序制作，.als 用相对路径 `../Effect/Animation/...` 把特效"贴"到本体动画的指定帧/层上，**不改动原 .ani 一个字节**。
- **一模多皮肤**：attackdustfront_08.ani.als 注册 11 个尘土变体（00-12）按不同层号挂接；attack_bladespirit1-4.ani（普攻）通过 .als 挂不同武器的剑光，普攻 .ani 保持通用。
- **多职业复用**：ap_common_burster（变身光环）这类公共效果由 appendage 驱动，特效动画带自己的 .als 即插即用。
- 启示：**"动画本体 JSON"与"叠加特效表"分成两个文件、同名配对、引擎自动合并**是已被 DNF 验证的组织方式。

## 三、[SET FLAG] 号分配惯例

### .ani 侧号段分布（swordman/animation + aicharacter，335 处）
| 号段 | 次数 | 用途（与 nut 对照后） |
|---|---|---|
| 0 | 9 | 存在但**未见脚本处理**（缺口） |
| 1-13 | 202 | **技能内小事件**：出刀/震屏/建 PO/发包（bloodboom flag1=创PO+震屏） |
| 100-126 | 44 | **多段序列/受害者挂点**：elbowthrow 100-106=受害者相对位置表、110=收尾发包 |
| 237 | 1 | 孤例 |
| 10001-10009 | 89 | 同"小事件"，新内容偏好用（attack.nut: flagIndex==10001 → resetHitObjectList） |
| **65534** | 29 | **网络同步专用**：bloodyrave.nut 中 65534 触发 sq_SendChangeSkillEffectPacket（连招特效重同步）。aicharacter 侧只有 1 和 65534 两个号 |

### onKeyFrameFlag 实现抽样（12 个 nut 的 case/== 号集合）
| 脚本 | flag 号 |
|---|---|
| swordman/attack/attack.nut | 0-5, 67, 100-101, **120-123**(连段分派), 10001 |
| swordman/5_ghostsword/speedslash | 1, 3, 10001-10003 |
| swordman/5_ghostsword/hellslash | 10002, 10004 |
| swordman/5_ghostsword/ghostdecollation | 0, 1, 3, 10001-10003 |
| swordman/bloodriven | 1, 2, 4 |
| swordman/bloodboom | 1 |
| fighter/elbowthrow | 1(震屏+attackInfo), **100-106(位置表), 110(发包)** |
| fighter/chaindestruction | 1, 2, 3 |
| mage/avatar, chasercluster | 0-2 |
| mage/dragondance | 0-6, 10, 11 |
| priest/avengerattack | 0-3, 100 |
| passiveobject/new/gunner/else | 12, **1001-1004** |

**惯例结论**：
1. 号空间就是"每个技能私有的 int 事件号"，引擎不解释、纯路由到 onKeyFrameFlag(obj, flag)。
2. **小号 1-9 = 通用语义**（每技能 1-3 个：出判定/震屏/创 PO），**两位数 100+ = 同一技能内的序列/查表项**（受害者挂点、连段分派），**1000+/10000+ = 新内容风格**（等价于小号，无特殊引擎语义），**65534 ≈ 0xFFFE 被多个技能复用作"同步帧"标记**。
3. flag 号跨技能无冲突（回调按技能命名空间隔离），**不需要全局注册表**。
4. 对我们 JSON 的映射：帧事件数组 `flags:[int]` + 技能逻辑按值 switch，即可 1:1 复刻 DNF 惯例；建议保留"小号=通用事件、大号=自定义"约定并显式保留一个同步专用号。

## 四、.ani 全字段表 + 序列帧游戏必要性注记

| 字段 | 级别 | 格式 | 影响的行为 | 我们要不要 |
|---|---|---|---|---|
| [FRAME MAX] | 顶层 | int | 帧数 | **必须要** |
| [FRAME000..] | 顶层 | 节 | 帧定义 | 必须要 |
| [IMAGE] | 帧 | 路径模板+帧号 | 贴图来源+图集帧索引 | 必须要（我们=图集 id+帧 index） |
| [IMAGE POS] | 帧 | int x y | 帧图相对锚点摆位（脚底中心原点，y 向下） | 必须要 |
| [DELAY] | 帧 | int ms | 帧时长 | 必须要（我们已用 ms） |
| [LOOP] | 顶层 | 0/1（恒 1） | 播完回帧 0 循环 | 必须要（布尔循环开关；DNF 无"回跳到中间帧"能力——需要中途循环得拆两个动画） |
| [SHADOW] | 顶层 | 0/1 | 脚底阴影开关 | 要（特效动画关影用） |
| [ATTACK BOX] | 帧 | 6 值 | 攻击判定盒（y=纵深，z=高度） | 要（判定核心；已解） |
| [DAMAGE BOX] | 帧 | 6 值同上 | 受击盒 | 要（已解） |
| [SET FLAG] | 帧 | int | 帧事件号→onKeyFrameFlag | 要（hit/cancel/PO 创建全靠它） |
| [PLAY SOUND] | 帧 | 资源名 | 帧触发音效 | 要（打击感） |
| [DAMAGE TYPE] | 帧 | NORMAL/SUPERARMOR/UNBREAKABLE | 该帧受击状态（霸体/不可打断） | **建议要**（帧级霸体比 buff 级更细，DNF 读条技全靠它）；仅 3 值易实现 |
| [RGBA] | 帧 | R G B A (0-255) | 整帧染色+透明（隐形帧/剪影/淡入出） | 要（隐形占位帧、残影黑剪影、闪白都用） |
| [GRAPHIC EFFECT] | 帧 | 枚举 4 值 | 帧混合模式（加法混合=发光） | **建议要**（2D 格斗打击感核心：additive 辉光；MONOCHROME/DARK 可缓） |
| [IMAGE RATE] | 帧 | float sx sy | 缩放；sx<0=水平镜像 | 要（镜像复用贴图省一半资源） |
| [IMAGE ROTATE] | 帧 | float 弧度 | 旋转 | 可选（方向/朝向 hack 用；中心语义需自定） |
| [INTERPOLATION] | 帧 | 0/1（恒 1） | 帧间位置平滑 | 可选（20fps 帧动画 + 插值=顺滑位移，帧同步下需确定性实现） |
| [FLIP TYPE] | 帧 | HORIZON/VERTICAL/ALL | 帧级翻转 | 低优先（可用 IMAGE RATE 负值替代大半场景） |
| [SPECTRUM] | 顶层 | 开关+TERM/LIFE TIME/COLOR/EFFECT | 残影（间隔采样+存活+染色+混合） | 要（疾影系位移的标配；等价我们做"动画级 afterimage 配置"） |
| [COORD] | 顶层 | 恒 1 | 坐标模式切换（投射物/粒子） | 暂不要（语义未实锤，我们无多坐标空间需求） |

## 缺口清单（查不到/未实锤）
1. **.als 两个整数的引擎侧最终解释**（z 序 vs 事件 id；整数1=-1 的含义；bladephantom_mist1 的 8=FRAME MAX 越界）
2. IMAGE ROTATE 的旋转中心与正方向
3. INTERPOLATION 插值的具体属性集合
4. COORD=1 的确切坐标空间切换
5. [none effect add] 与 [add] 的引擎差异
6. flag 0 的消费方（.ani 有用例，脚本未见处理）
7. LOOP=255 孤例的文件定位（全库定位超时）
8. DAMAGE BOX 第 6 值的确切含义（前轮遗留，本轮数据一致：疑似高度/层深扩展）

Sources:
- [17173 DNF 资源分析](https://dnf.17173.com/content/2009-02-11/1234335803.shtml)
- [dnf.arad.ink: PVF 文件与工具](https://dnf.arad.ink/thread-1728-1-1.html)
- [dnf.arad.ink: APD/PVF 格式](https://dnf.arad.ink/thread-5544-1-1.html)（.als 具体格式无公开文档，仅佐证其存在与社区研究空白）
