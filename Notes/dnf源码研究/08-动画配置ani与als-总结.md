# 第三轮总结②：DNF 动画配置 .ani 与 .als（字段全集/边车机制/帧标记惯例）

> 课题：Q1——.ani 和 .ani.als 的区别联系、完整字段语义、帧标记惯例。
> 来源：.ani 78 万文件抽样实证（11 个剩余字段逐个取证）+ .als 全量普查（17,283 个）+ [SET FLAG] 号段统计 + 社区资料交叉。
> 用途：定案我们 AnimClipData 该有哪些字段（attack.json 制作依据）+ 阶段7 特效挂接设计参照。

---

## 1. DNF 怎么做的

### 1.1 .ani 全字段表（含已解字段，"我们要不要"= 序列帧游戏必要性）

| 字段 | 级别 | 格式 | 语义 | 我们要不要 |
|------|------|------|------|-----------|
| [FRAME MAX] / [FRAME000..] | 顶层/帧 | int / 节 | 帧数/帧定义 | ✅ 已有 |
| [IMAGE] | 帧 | 路径模板+帧号 | 贴图（`sm_body%04d.img`+78） | ✅ 已有（图集 index） |
| [IMAGE POS] | 帧 | int x y | 摆位（脚底锚点，y 下正） | ✅ 已有 |
| [DELAY] | 帧 | int ms | 帧时长 | ✅ 已有 |
| [LOOP] | 顶层 | 0/1 | **布尔循环开关**（播完回帧 0；非帧号假说已否） | ✅ 已有（布尔）；注意 DNF 技能动作大多不循环、由状态机切 |
| [DAMAGE BOX]/[ATTACK BOX] | 帧 | 6 值 | 受击/攻击盒（y=纵深 z=高度） | ✅ 已有/待做 |
| [SET FLAG] | 帧 | int | 帧事件号 → onKeyFrameFlag | ✅ 已有（hitFlag/cancelFlag） |
| [PLAY SOUND] | 帧 | 资源名 | 帧触发音效（音频在客户端 NPK，pvf 内无） | 🔶 加（打击感） |
| **[DAMAGE TYPE]** | 帧 | NORMAL/SUPERARMOR/UNBREAKABLE | **帧级受击状态**：霸体帧（血爆读条 23 帧全霸体）/不可打断 | 🔶 **加**（帧级霸体比 buff 级细，仅 3 值易实现） |
| **[RGBA]** | 帧 | R G B A (0-255) | 整帧染色+透明：隐形占位帧(alpha=0)/黑剪影/淡入淡出/闪白 | 🔶 加 |
| **[GRAPHIC EFFECT]** | 帧 | 枚举4值 | 帧混合模式：**LINEARDODGE=加法混合（发光特效标配，9.6万处）**/MONOCHROME/DARK/SPACEDISTORT | 🔶 **加**（2D 打击感视觉核心；先只做 LINEARDODGE） |
| **[IMAGE RATE]** | 帧 | float sx sy | 缩放，**sx<0=水平镜像**（回身动作翻贴图复用） | 🔶 加（省一半贴图） |
| **[SPECTRUM]** | 顶层 | 开关+TERM(采样ms)/LIFE TIME(存活ms)/COLOR(rgba)/EFFECT | 残影：每 TERM 采样一个残影存活 LIFE TIME（80/400≈同时5个） | 🔶 加（疾影位移标配） |
| [SHADOW] | 顶层 | 0/1 | 脚底阴影开关（特效显式关） | 🔶 加 |
| [IMAGE ROTATE] | 帧 | float 弧度 | 旋转（0.785398=45° 铁证）；中心/方向未实锤 | ⏸ 缓 |
| [INTERPOLATION] | 帧 | 0/1 恒1 | 帧间 IMAGE POS 平滑 | ⏸ 缓（帧同步下需确定性实现） |
| [FLIP TYPE] | 帧 | HORIZON/VERTICAL/ALL | 帧级翻转（低频，IMAGE RATE 负值可替代大半） | ⏸ 缓 |
| [COORD] | 顶层 | 恒1 极稀有 | 坐标模式切换（投射物/粒子，语义未实锤） | ❌ 不做 |

### 1.2 .als 边车（17,283 个，全网无公开文档——本轮首证）

**结构**（9 种节，按频次）：

| 节 | 次数 | 语法 | 语义 |
|----|------|------|------|
| `[use animation]` | 97,323 | 子.ani 路径 + 别名 | **注册**子动画（不触发） |
| `[none effect add]` | 61,865 | `<整数1> <整数2>` + 别名 | 在父动画上叠加播放该子动画 |
| `[add]` | 33,632 | 同上 | 普通叠加（推断 [none] 变体不继承父特效状态） |
| `[create draw only object]` | 1,529 | 帧 + 别名 + x,y,z 偏移 | 生成独立绘制对象 |
| `[static data]` | 371 | 整数段 | 透传数据 |
| `[create draw only object follow parent]` | 80 | 同上 | 跟随父对象 |
| `[remove]` | 55 | 帧 + id | 移除已叠加动画 |
| 其余（mask layer/random/bottom/no apply speed…） | <30 | | 变体 |

**两个整数的语义**（静态推断，证据充分非引擎实锤）：**整数1=触发帧号**（父动画帧号）；**整数2=绘制层 z 序**（铁证：同一特效挂 -10000 和 +10000 前后夹层；cutBottom=-9999/cutUp=+9999）。

**消费机制**：**同名自动配对**（`xxx.ani` ↔ `xxx.ani.als`，连 [pvp] 变体都保持配对）——引擎加载 .ani 时自动应用边车；**sqr 脚本侧零显式引用**（仅原生函数 `als_ani(obj,"xxx.ani",...)` 从侧面印证）。.als = 把脚本手写特效叠加**声明化**（被注释掉的 sq_CreateDrawOnlyObject 手写代码上方就是 als 注释）。

**为什么独立成边车**：①**内容归属分离**——本体动画与特效由不同工序制作，.als 用相对路径把特效"贴"到本体指定帧/层，不改原 .ani 一个字节；②**一模多皮肤**——普攻 .ani 通用，.als 挂不同武器的剑光（11 个尘土变体按层号挂接）；③多职业复用公共特效。

### 1.3 [SET FLAG] 号段惯例（335 处 + 12 个脚本抽样）

| 号段 | 用途 |
|------|------|
| 0 | 存在但未见脚本处理（缺口） |
| **1-13** | 通用小事件：出判定/震屏/创 PO（bloodboom flag1=创PO+震屏） |
| **100-126** | 技能内查表/序列：受害者挂点表（elbowthrow 100-106）、连段分派（attack 120-123） |
| 1000+/10000+ | 新内容风格（等价小号，如 10001=resetHitObjectList） |
| **65534** | **网络同步专用帧**（触发 ChangeSkillEffectPacket 重同步，多技能复用） |

flag 号 = 技能私有 int 事件号，引擎不解释纯路由；跨技能无冲突（回调按技能命名空间隔离），**不需要全局注册表**。

---

## 2. 我们怎么对应

| DNF | 我们 | 评价 |
|-----|------|------|
| .ani 帧字段 | AnimClipData（move.json 等） | ✅ 直译（第一轮已证同源） |
| [LOOP] 布尔 | AnimClipData.loop 布尔 | ✅ 同构 |
| hitFlag/cancelFlag | [SET FLAG] 的专用化 | ✅；泛化方案见建议2 |
| [DAMAGE TYPE] 霸体帧 | 无 | ❌ 加（第一轮建议3 的具体格式定案：3 值枚举） |
| [RGBA]/[GRAPHIC EFFECT]/[IMAGE RATE]/[SPECTRUM]/[PLAY SOUND]/[SHADOW] | 无 | ❌ 按必要性分批加（建议1） |
| .als 叠加特效表 | 无（阶段7 特效未设计） | 🔶 阶段7 参照 .als 模式（建议3） |
| flag 号段惯例 | 无约定 | 🔶 采纳号段约定（建议2） |

## 3. 建议改动清单

1. **AnimClipData 字段扩展定案**（attack.json 制作时一并加）：
   - **必加**：`damageType`（0 普通/1 霸体/2 不可打断）、`rgba`（默认 255,255,255,255；隐形帧/闪白用）、`graphicEffect`（0 无/1 加法混合）、`imageRate`（x,y 默认 1,1；负 x=镜像）、`playSound`（资源名，可空）。
   - **后加**：`spectrum`（残影四参数，疾影类技能时）、`shadow`（布尔）。
   - **缓**：`imageRotate`/`interpolation`/`flipType`（用到再加）。
2. **AnimFrameData 帧事件泛化**：保留 `hitFlag`/`cancelFlag` 语义字段（可读性），另加 `flags: int[]` 通用帧事件数组——采纳 DNF 号段约定：**1-9 通用事件、100+ 技能内查表、65534 保留做同步/重播专用**。技能逻辑按值 switch。
3. **阶段7 特效挂接参照 .als 模式**：本体动画 JSON 与"叠加特效表"**分文件同名配对**（如 `attack.json` + `attack.fx.json`），运行时自动合并——特效贴到指定帧/层而不污染本体数据；支持一模多皮肤（不同武器挂不同剑光）。
4. 镜像复用：贴图打图集时支持 `imageRate.x<0` 镜像引用（同一图集帧翻转使用，省资源）。

## 4. 遗留/待验证

- .als 两个整数的引擎侧最终解释（z 序 vs 事件 id——z 序证据强但非实锤）；整数1=-1 与"8=FRAME MAX 越界"用例。
- [IMAGE ROTATE] 旋转中心与正方向（需运行时验证）。
- [INTERPOLATION] 插值属性集合；[COORD]=1 坐标空间切换。
- [none effect add] 与 [add] 的引擎差异。
- flag 0 的消费方；LOOP=255 孤例。
- DAMAGE BOX 第 6 值疑义（前轮遗留，疑似高度/层深扩展）。
- 音效名→资源的映射表在客户端侧（pvf 内无）。
