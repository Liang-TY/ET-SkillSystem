# R1-D：DNF IMG/.ani 坐标系语义研究笔记（受击盒/攻击盒采样依据）

> 第1轮 Agent D 原始笔记。任务：Q14 坐标系与包围盒。
> 已综合进：02-坐标系与包围盒-总结.md

---

## 0. 结论速览

| 问题 | 结论 | 置信度 |
|---|---|---|
| IMG 帧 X/Y | 位图左上角在帧画布（FrameWidth×FrameHeight，"帧域"）内的偏移，画布原点左上、y 向下 | 确凿（源码+实测） |
| [IMAGE POS] | 帧画布左上角相对"角色锚点"的位置；锚点=角色脚底中心线与地面交点；x 右正、y 下正（负 y 在脚上方） | 确凿（实测验证） |
| 摆位公式 | 位图左上角(世界) = 角色原点 + [IMAGE POS] + (IMG帧x, IMG帧y)；y 向下为正 | 确凿（本项目数据验证，误差≤2px） |
| DAMAGE BOX | 该动画播放者的**受击盒**（hurtbox），6 值 = minX minY minZ maxX maxY maxZ | 确凿 |
| 三轴语义 | x=横向（右正，规范朝向=面右）；**y=纵深/地板前后**；**z=高度（上正，0=地面脚底）** | 确凿（多源交叉） |
| 单位 | 全部是像素，与贴图 1:1，无全局缩放 | 确凿（实测） |
| 朝向镜像 | 面左时 x 区间取反（[minX,maxX]→[-maxX,-minX]），y/z 不变，贴图水平翻转 | 高置信推断 |
| 盒子来源 | 策划在动画编辑器里**逐帧手配**，非运行时按位图非透明像素自动算 | 高置信推断（有数据反证自动生成） |

## 1. 本地实证（最强证据，全部可复现）

### 1.1 我们的数据就是 DNF 原版数据
`dnforigin/pvf源码提取部分/pvf/monster/bantu/baanimation/move.ani` / `stay.ani` 与我们的 `move.json` / `stay.json` **逐值相同**（图片路径 Monster/Bantu/BantuAmazones.img、帧号 9-14/0、imagePos -249,-301、damageBox -16 -7 -5 29 14 106）。.ani 是明文 PVF 脚本，帧结构：

```
[FRAME000]
	[IMAGE]
		`Monster/Bantu/BantuAmazones.img`
		9
	[IMAGE POS]
		-249	-301
	[DELAY]
		60
	[DAMAGE BOX]
	-16	-7	-5	29	14	106     ← minX minY minZ maxX maxY maxZ
```

全 pvf 的 .ani 字段全集（character/common/animation 下 213188 帧统计）：`[IMAGE]/[IMAGE POS]/[DELAY]/[GRAPHIC EFFECT]/[RGBA]/[DAMAGE TYPE]/[IMAGE RATE]/[IMAGE ROTATE]/[INTERPOLATION]/[ATTACK BOX]/[DAMAGE BOX]/[FLIP TYPE]/[PLAY SOUND]/[SET FLAG]/[SHADOW]/[LOOP]/[COORD]`。

### 1.2 用 BantuAmazones.img 实测验证摆位公式

对 `Bundles\AnimRes\bantuamazones.img.bytes`（IMG v2，54 帧，帧画布统一 500×500）解码帧目录，与 .ani 的 imagePos 组合：

| 帧 | 位图 w×h | IMG x,y | imagePos | 位图左上=和 | 位图占据（x 左..右，y 上..下） | 该帧 damageBox x / z |
|---|---|---|---|---|---|---|
| 0(stay) | 53×107 | 216,195 | -249,-301 | (-33,-106) | x[-33..20]，y[-106..1] | x[-21..44]，z[-5..106] |
| 9(move0) | 54×101 | 222,201 | -249,-301 | (-27,-100) | x[-27..27]，y[-100..1] | x[-16..29]，z[-5..106] |
| 10 | 53×103 | 226,198 | -254,-301 | (-28,-103) | x[-28..25]，y[-103..0] | x[-10..21]，z[-5..106] |
| 12 | 57×103 | 229,199 | -259,-301 | (-30,-102) | x[-30..27]，y[-102..1] | x[-13..26]，z[-5..106] |

**三条铁律从这张表直接读出：**
1. **位图底边 = imagePos.y + imgY + h ≈ 0~+1 像素**（核了全部 30+ 个实体帧，无一例外）——脚底正好落在原点。DNF 美术是"画布 500×500 固定、精灵悬在画布中部约 200px 高处、.ani 里逐帧用 imagePos 抵消画布偏移"来制作的。imagePos 是 -301 这种大负数，主要在抵消画布内偏移（y≈195..280），不是精灵尺寸。
2. **位图高度 101~107 ≈ 受击盒 z 跨度 111**（0..106 加下探 -5）——受击盒纵向基本罩住整个身体剪影，贴图像素与游戏判定像素 1:1，**没有全局缩放系数**。
3. walk 循环里 imagePos.x 在 -249..-259 摆动而位图左上稳定在 -27..-31——imagePos 是逐帧调好的"抵消量"。

### 1.3 z=高度、y=纵深 的铁证（不靠任何网络信源）
- `fallen_asil/animation/down.ani`（倒地动画）：站立受击帧 z 范围 0..108/113 → 倒下过程 0..34 → 0..19 → **躺平帧 0..5**，同时 x 从 ±30 扩到 **-59..113**（身体放平、横向摊开）、y 恒为 -5..10。只有"z=离地高度"能解释：躺平后高度塌到 5px，厚度（y）不变。
- 地板攻击特效 `aura/20_devil_task/prey_2/pattern01attackfloorb_00.ani` 的 ATTACK BOX：z **0..10**（贴地薄饼），y -19..36，x -59..119——地板波纹在高度上扁平、纵深展宽。
- 攻击动画 `monster/bantu/bashanimation/attack.ani` 第 2-4 帧（攻击判定帧）：ATTACK BOX x[25..61]（前方）、z[26..48]（半身高挥击带）、y[-15..20]；同帧还带 DAMAGE BOX z[0..92]——攻击盒明显比身体盒（z 0..106）矮，是前伸的打击带。
- 交叉信源：DNF 官方公告用"**Y轴：250 PX**"描述技能纵深判定、用"跳跃高度"描述 Z；私服 nut API 有专门的 `sq_SetZVelocity`（跳跃竖直速度）、`sq_SetVelocity(obj, dis, speed)` 的 dis 注释 `0/X轴 1/Y轴 2/Z轴`（16-API函数参考.md）。

⚠️ **信源冲突警告**：GitHub `heihuo000/DNF-PVF-file-Reference`（ANI 问题解答.md）声称"X轴左负右正；**Y轴下负上正；Z轴前负后正**"——与上述全部硬数据矛盾（该库行文带明显 AI 生成痕迹，示例坐标也是编造风格）。**采信硬数据：z=高度。** 该库其余内容（IMAGE POS 语义、DAMAGE/ATTACK BOX 功能描述）与实测一致，可用。

## 2. 坐标系对照表

| 坐标 | 原点 | 单位 | 正方向 | 用途/备注 | 证据 |
|---|---|---|---|---|---|
| IMG 帧 X/Y | 帧画布左上角 | px | x 右正，y 下正（图像坐标） | 位图在画布内的摆放偏移 | OjoDnfExtractor 源码 `set_canvas(cw,ch)+set_texture(x,y,w,h)`；DNF-Porting `NpkTexture{x,y,...}`；实测 |
| IMG FrameWidth/Height | — | px | — | 帧画布尺寸（统一 500×500）。运行时定位**不需要**；属编辑器画布/可能供特效阴影参考 | arad.ink 教程称"帧域"；实测仅作 (x,y) 容器 |
| .ani [IMAGE POS] | **角色锚点 = 脚底中心** | px | x 右正，y 下正（负值在脚上方） | 帧画布左上角相对锚点的位置。社区口诀"从画布左上角量到目标点，取负" | arad.ink 教程（公式 X=-X1-X2/2, Y=-Y1-Y2/2-60 为粗略近似，含经验常数 60，不必采用）；实测底边归零铁律 |
| .ani [DAMAGE BOX] | 角色逻辑位置（脚底中心，z=0 地面） | px | x 右正（规范朝向=面右）；y 纵深；z 上正 | 6 值 minX minY minZ maxX maxY maxZ；受击盒；一帧可写多个盒；[DAMAGE TYPE] 同时段 SUPERARMOR=该帧霸体 | down.ani/地板特效/官方公告；DNF-Porting `vector<array<int,6>> damageBox` |
| .ani [ATTACK BOX] | 同上 | px | 同上 | 同格式 6 值；攻击判定帧才写；nut API `sq_GetAttackBoundRect(ani)`/`sq_AddAttackBox(anim,x,y,z,xSz,ySz,zSz)`/`sq_SetAttackBoundingBoxSizeRate` 直接读写 | 同上 + bantu attack.ani 实例 |
| DNF 世界坐标（nut getXPos 等） | 房间/关卡 | px | x 右正；y=纵深；z=高度 | 官方公告"Y轴：250 PX"即纵深判定范围 | 官方公告、API 文档 |

注意：**DNF 的 y 不是"上下屏幕"而是"地面纵深"，z 才是"上下"。** .ani 的 IMAGE POS 是纯屏幕 2D 坐标（x、竖直 y），与盒子的 3D（x、纵深 y、高度 z）是两套 y。

## 3. 包围盒怎么来的：手配，不是位图自动算

判定：**策划/美术在 Neople 内部动画工具里逐帧手配**（社区有可视化 .ani 编辑器，如"梦太晓 Ani 文件可视化编辑工具"）。证据：
1. **可缺省**：character/common 下 213188 帧里只有 92 处 DAMAGE BOX、53 处 ATTACK BOX——自动剪影算法不会"大部分帧没有盒"。
2. **与位图剪影不一致**：stay 帧位图 x[-33..20] 而盒 x[-21..44]——右侧超出位图 24px（含武器/预判余量）、左侧缩进 12px。这不是任何 alpha-bbox。
3. **脏数据**：Bash 怪攻击盒写成 z "48 26"（min>max，倒序）——生成器不会输出，人手敲会；引擎端显然做了 min/max 归一化容错。
4. **随帧快速变形且只在活跃帧出现**——典型格斗游戏 frame data 手工打磨。
5. 社区文档的设计指导语气（"被击框通常比攻击框稍小""攻击框应覆盖武器范围"）。

（未找到 Neople 内部编辑器是否有"按剪影预填"辅助功能的公开记录——但**运行时不做自动剪影碰撞**是确定的。）

## 4. 我们项目的采样换算（TSVector，1 单位=100px@100ppu，Position=脚底中心）

### 4.1 轴映射与缩放
DNF 像素 → 我们单位：**除以 100**。轴映射（我们 z=纵深，y=高度）：
```
我们.x ← DNF.x            （都是右正；DNF 规范朝向=面右）
我们.y ← DNF.z (高度)     （都是上正，0=脚底地面）
我们.z ← DNF.y (纵深)     （DNF y 正方向未定，盒近对称，取哪边都不影响对称判定）
```

### 4.2 受击盒 AABB 公式（每帧采样，无插值）
```csharp
// dnfBox: min/max 各 (x,y,z) 像素;  facingRight: 当前朝向;  pos: 角色脚底中心 TSVector
long s = 100; // 100px = 1 unit

// 先归一化（DNF 数据存在 min/max 倒序的脏数据）
int minX = Math.Min(dnfBox.min.x, dnfBox.max.x), maxX = Math.Max(...);
// ... minY/maxY/minZ/maxZ 同理

long wx0 = facingRight ? minX : -maxX;      // 面左：x 区间镜像
long wx1 = facingRight ? maxX : -minX;

var worldMin = new TSVector(pos.x + wx0/s, pos.y + minZ/s, pos.z + minY/s);
var worldMax = new TSVector(pos.x + wx1/s, pos.y + maxZ/s, pos.z + maxY/s);
```

move.json 首帧代入（面右）：世界盒 x[-0.16, 0.29]，y[-0.05, 1.06]，z[-0.07, 0.14]——角色高约 1.06 单位，正对着 107px 的贴图（1:1 缩放，Sprite 的 pixelsPerUnit=100 直接成立）。z(minDNF)=-5 是允许脚底略微下探的余量，建议保留（忠实原作）。**纵深跨度 21px≈0.21 单位——我们的 z 轴判定阈值应与此同量级。**

### 4.3 渲染摆位（跟盒子共用同一锚点）
- 每帧 Sprite 的 pivot（纹理像素，从纹理左上、y 向下）：`pivot = (-imagePos.x - imgX, -imagePos.y - imgY)`。stay 帧 0：`(-(-249)-216, -(-301)-195) = (33, 106)`，即 53×107 贴图上 (33,106) 处是脚底锚点。
- Unity：`sprite.pivot = new Vector2(pivotX/w, 1 - pivotY/h)`，SpriteRenderer 挂在角色 Position 上；面左用 flipX（绕 pivot 镜像，与盒子 x 镜像天然一致）。DNF 手游官方也是这个方案（atlas 配置 originWidth/originHeight + offsetX/offsetY 逐帧转 Unity pivot）。
- [IMAGE RATE]（浮点，1.0=原大，-1=水平翻转）、[IMAGE ROTATE]（弧度，顺时针正）目前我们 JSON 未承载，未来做特效动画时需要。

### 4.4 校验清单
跑一帧 stay：贴图底边应贴地面、角色中线过 Position.x；受击盒 Gizmo 应近似罩住剪影（右缘超出武器位置 ~0.24 单位属正常手配余量）。

## 5. 遗留问题（没查明白的，别当定论用）
1. **DNF y（纵深）正方向**：无公开权威信源；盒近对称故不影响我们；只有做"击退纵深方向"类逻辑时才需要，建议到时用台服单机实测。
2. **社区公式里的 -60 常数**（arad.ink）：作者自称粗略近似，疑似典型人体位图的经验补偿值；我们用逐帧精确数据，无关紧要。
3. **FrameWidth/FrameHeight 是否有运行时作用**：无证据；当编辑器画布元数据即可。
4. **玩家技能的攻击盒是否全部来自 .ani**：`sq_GetAttackBoundRect(ani)` 表明几何来自 .ani，.atk 文件提供的是伤害/命中方向等参数；不排除个别技能由 nut 脚本动态加盒（`sq_AddAttackBox`），未逐一验证。
5. **一帧多盒**：二进制格式支持一帧多个 DAMAGE/ATTACK BOX（DNF-Porting 解析为 vector），我们的 JSON 目前只留一个——转 .ani 时若遇到多盒要决定取并集还是数组。
6. CSDN《NPK的那些事儿》系列正文付费墙（第 3 篇 IMGV2 细节）未能读到全文，但字段语义已由 OjoDnfExtractor/DNF-Porting 源码 + 实测覆盖。

## 6. 参考 URL 清单
**网络信源**
- arad.ink《DNF-ANI坐标详解+ani制作器1.6》：https://dnf.arad.ink/thread-9675-1-1.html
- arad.ink《梦太晓Ani文件可视化编辑工具》：https://dnf.arad.ink/thread-3183-1-1.html
- heihuo000/DNF-PVF-file-Reference（⚠️ 轴向描述有误，其余可用）：https://github.com/heihuo000/DNF-PVF-file-Reference
- flwmxd/DNF-Porting（.ani 二进制解析 C++）：https://github.com/flwmxd/DNF-Porting
- HsOjo/OjoDnfExtractor（IMG v1-6 提取器源码）：https://github.com/HsOjo/OjoDnfExtractor
- hooyantsing/npk-api（NPK 解包库）：https://github.com/hooyantsing/npk-api
- Musoucrow《DNFMobile 图片资源提取笔记》：https://musoucrow.github.io/2018/01/20/dnf_mobile_ex/
- CSDN《关于DNF的多媒体包NPK文件的那些事儿》系列：https://blog.csdn.net/u010274704/article/details/77413110 、78113351 、78195956
- DNF 官方机械崛起公告（"Y轴：250 PX"）：https://dnf.qq.com/cp/a20221222version/page02.html
- 灰机wiki 角色技能平衡公告：https://dnfcn.huijiwiki.com/wiki/角色技能、决斗场技能平衡性改版
- 肥猫池塘 49：https://seicing.com/html/fatcatpool/fat49.html
- 知乎"有纵深的横版 2D 游戏坐标"讨论：https://www.zhihu.com/question/59129150
- DNF 开源项目索引：https://github.com/stars/momaek/lists/dnf

**本地关键文件**
- `Packages/cn.etetet.npkparser/Runtime/NpkImgParser.cs`（帧目录字段读取，与 DNF-Porting/OjoDnfExtractor 一致）
- `Packages/cn.etetet.lockstep/Bundles/AnimRes/`（bantuamazones.img.bytes 实测源）
- `pvf/monster/bantu/baanimation/{stay,move}.ani`、`bashanimation/attack.ani`（我方 JSON 的原始出处、攻击盒实例）
- `pvf/aicharacter/arad_aic/2013sao/fallen_asil/animation/{damage1,down}.ani`（z=高度铁证）
- `nut知识库/`（10-攻击系统、16-API函数参考、资源nut函数声明\language.dof.{BoundingBox,globalFunction,animation}.md、14-ACT脚本说明）
