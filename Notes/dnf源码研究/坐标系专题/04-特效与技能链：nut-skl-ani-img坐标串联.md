# 04 · 特效与技能链：nut-skl-ani-img 坐标串联

> **本篇回答**：从按下技能键到特效落点、判定生效，坐标在哪些文件/层之间流转？"特效生成在哪"到底由谁决定？近战判定和投射物判定为什么是两套来源？
> **实证素材**（全部为 pvf 原文 + nut 知识库，路径可点开复核）：
> - `pvf/skill/swordman/normalwave.skl`（波动剑技能配置）
> - `pvf/sqr/character/swordman/wave/wave.nut`（波动系状态脚本）
> - `pvf/sqr/character/swordman/wave/po_wavecut.nut`（波动气被动对象脚本）
> - nut 知识库：`部分前人的文档/nut知识库/02-技能系统.md`、`11-常用函数.md`

---

## 1. 全链路总图（按键 → 伤害的坐标流转）

```
① 键盘 ↓→+技能键
   ↓ （normalwave.skl [command]：{6=DOWN},{8=,},{6=RIGHT},{8=,},{6=SKILL}）
② 技能配置层 .skl
   · [static data] 250 100 -150      ← 数值参数（sq_GetLevelData 按索引读）
   · [skill preloading image] NormalWave1/2.img   ← 资源预加载清单（按需加载的钥匙）
   ↓  进入技能状态
③ 角色状态脚本 .nut（wave.nut，state=Wavesword）
   · 角色播放攻击动画（角色 .ani，含 [SET FLAG] 关键帧标记）
   · onKeyFrameFlag_WaveSword(obj, flagIndex)   ← 动画播到关键帧时被回调！
   ↓  在关键帧上：
④ 生成特效对象
   obj.sq_SendCreatePassiveObjectPacket(24328, 0, 75, 1, 0)
                                          │      │   │  │  └ z=0（地面）
                                          │      │   │  └─── y=±1/±15（纵深道）
                                          │      │   └────── x=75（身前75px）
                                          │      └────────── 0=参数流标记
                                          └───────────────── 被动对象编号
   ↓  相对施法者当前位置（脚底锚点）+ 方向
⑤ 特效对象（passive object, 24328）自己的脚本 po_wavecut.nut
   · 位置：setCurrentPos(posX, yPos, z)      ← 自己管理世界坐标
   · 表现：setCurrentAnimation(icewave.ani)  ← 特效动画（02篇的特效锚点规则）
   · 判定：sq_GetCustomAttackInfo(obj, 103)  ← 盒子来自 attackInfo 配置（不是 .ani！）
   · 前进：每 tick 链式生成下一个 PO（sq_SendCreatePassiveObjectPacketPos(..., posX+dist, ...)）
   ↓
⑥ 命中：PO 的判定盒 × 目标受击盒（03篇三轴盒）→ 伤害/击退（nut 写方向）
```

**"特效在头顶"在这条链上只有两个候选病因：④ 生成点坐标错（x/y/z 给错或挂错参考系），或 ⑤ 特效动画摆位错（没走 02 篇特效锚点公式）。** 分层排查见 06 篇 §1。

## 2. 逐层要点与原文证据

### 2.1 技能层（.skl）：参数与资源清单

- `[static data]` 的数字按索引被 nut 读走：`sq_GetLevelData(obj, 100, 4, level)` = 取技能100的第4个数据（wave.nut:20 读的就是 burn 概率）。**skl 不直接写坐标，坐标逻辑在 nut 里**；skl 只给数值。
- `[skill preloading image]`：该技能要预载的 img 清单——DNF 按需加载策略的一环（进图/学技能才载资源，见 14 篇资源加载文档）。

### 2.2 关键帧回调：[SET FLAG] → onKeyFrameFlag

角色攻击 .ani 的判定帧/特效帧带 `[SET FLAG] n` 标记（02 篇词条表），nut 侧：

```squirrel
function onKeyFrameFlag_WaveSword(obj, flagIndex) {   // wave.nut:5
    // 动画播到打标记的帧 → 这里被调用 → 此刻才生成特效/开启判定
```

**"第几帧出手"是动画数据和脚本代码的连接点**——我们引擎的 onKeyFrameFlag 回调 + 帧驱动攻击盒（LSHitboxComponentSystem:44）就是同一机制的对等实现。

另一个帧驱动例（BloodBoom，nut 知识库 02-技能系统.md:535-560）：

```squirrel
if (currentAniIndex == 20 && Hit == 0) {
    obj.sq_SendCreatePassiveObjectPacket(38001, 0, 0, 0, 0);        // 举手血珠：原点生成
    local z = obj.getObjectHeight() / 2;                            // 半身高！
    obj.sq_SendCreatePassiveObjectPacket(24320, 0, 0, 0, z);        // PO 在半身高处生成
```

→ **生成 z 直接用 `对象高度/2`**：这是"特效生成点"语义的官方样例——身体中部的特效从半身高出。

### 2.3 生成坐标：相对施法者的 (x, y, z)，单位像素

wave.nut 的两组生成调用（wave.nut:46,66 附近）：

```squirrel
obj.sq_SendCreatePassiveObjectPacket(24328, 0, 75, 1, 0);   // 波A
...（参数流写 -15）
obj.sq_SendCreatePassiveObjectPacket(24328, 0, 75, 1, 0);   // 波B（参数里 +15）
```

- x=75：**沿面向**前方 75px（不是世界 x 轴！面左时自动取反——生成 API 吃"方向语义"的 x）
- y=±15：**纵深道偏移**——波动剑双道波就是这么来的（同一 z 高度、两条纵深车道）
- z=0：地面
- 配合 `sq_GetDistancePos(obj.getXPos(), obj.getDirection(), dist)`（po_wavecut.nut:141）—— **direction 参与的横移计算**是 nut 里"向前"的标准写法。

### 2.4 特效对象层：表现与判定分离

`po_wavecut.nut`（波动气本体）证明了投射物的双轨制：

| 轨 | 来源 | 内容 |
|---|---|---|
| 表现 | `setCurrentAnimation(icewave.ani)`（:66） | 特效动画：IMAGE POS 摆位（02篇特效锚点）、LINEARDODGE 发光、RGBA 闪烁 |
| 判定 | `sq_GetCustomAttackInfo(obj, 103); sq_SetCurrentAttackInfo(...)`（:69-70） | **attackInfo 配置**（编号103）定义伤害+判定盒——不是 .ani 的 ATTACK BOX |

**两种判定盒来源总结**：
- 近战（角色动作）：角色 .ani 的 [ATTACK BOX]，帧驱动（03 篇 kneekick 表）
- 投射物/特效（PO）：custom attackInfo 配置，nut 挂接，动画只管表现

我们引擎现状对应：近战=帧驱动盒（✅已实现）；弹=固定 AABB 配置（BulletDefinition.HalfExtents，attackInfo 思路的简化版）。以后做"判定盒随等级成长"时可参考 attackInfo 的配置化思路。

### 2.5 "前进"的实现：链式生成，不是移动单体

po_wavecut.nut onProc（:131-155）：每 10ms 检查，`maxCount > currCount` 时在 `posX = sq_GetDistancePos(x, dir, dist)` 处**生成下一个 PO**，自己到期销毁。视觉上"波在前进"，实际是一串 PO 沿 x 依次生灭。
（我们引擎的弹是"移动的单体"——两种实现皆可，链式生成的好处是每段可独立配置尺寸/命中表，代价是对象量。）

## 3. nut 坐标 API 速查表

| API | 语义 | 出处 |
|---|---|---|
| `obj.getXPos()/getYPos()/getZPos()` | 世界坐标（x横向/y纵深/z高度，px） | 11-常用函数.md:128-133 |
| `obj.setCurrentPos(x,y,z)` | 设世界坐标（人物直接用会卡墙） | 11-常用函数.md:111-113 |
| `obj.sq_SetfindNearLinearMovablePos(x,y,z)` | 不卡墙的坐标设置 | 11-常用函数.md:115 |
| `sq_GetDistancePos(x, direction, dist)` | 沿方向前进 dist 后的 x | po_wavecut.nut:141 |
| `obj.getObjectHeight()` | 对象高度（生成点用 /2） | nut知识库02:558 |
| `obj.sq_SendCreatePassiveObjectPacket(id, 0, x, y, z)` | 相对自身生成 PO | wave.nut:46 |
| `sq_SendCreatePassiveObjectPacketPos(obj, id, 0, x, y, z)` | 绝对坐标生成 PO | po_wavecut.nut:155 |
| `obj.getDirection()` | 朝向（1右/-1左） | wave.nut:141 |
| `sq_GetCenterXPos(boundingBox)` | 盒中心 x | 16-API函数参考.md:50 |
| `obj.sq_SetCameraScrollPosition(x, ...)` | 相机卷动 | 11-常用函数.md:224 |
| `sq_SendHitObjectPacket(obj, enemy, hintX, hintY, hintZ)` | 命中反馈提示点（飘字/受击方向） | 16-API函数参考.md:107 |
| `sq_GetCurrentAnimation()/GetCurrentFrameIndex()` | 当前动画/帧号（帧驱动判定用） | nut知识库02:540 |

## 4. 世界坐标与地图（速查级）

- 世界坐标 = x横向 / y纵深 / z高度，**单位像素**；官方"Y 轴 250 PX"=纵深判定带宽度。
- 相机跟随：`sq_SetCameraScrollPosition(sq_GetDistancePos(obj.getXPos(), obj.getDirection(), 0), ...)`——以角色 x 为主轴卷动（11-常用函数.md:224-226）。
- 地图配置文件（map/stagemap 的房间/出生点/背景层坐标）本期未深挖——现有素材足够技能开发用，需要做地图编辑器时再开专题。
- NPC 朝向由地图调用 `Left` 词条控制；特效自翻转用 .ani [IMAGE RATE] 负值（资料/网络资料摘录.md）。

## 5. 对应我们引擎（ET）的映射表

| DNF 机制 | 我们的对等物 | 状态 |
|---|---|---|
| skl [command] | LSOperaComponent 按键 → SkillIds.ButtonToSkill | ✅ |
| skl [static data]/[level info] | SkillLogic 常量 /（未来 luban 表） | ✅ 简化版 |
| [skill preloading image] | LSAnimResComponent.InitAsync 建图集（按需） | ✅ |
| .ani [SET FLAG] → onKeyFrameFlag | 动画帧标记 → SkillContext/Hitbox 帧驱动判定 | ✅（判定帧=有盒帧） |
| sq_SendCreatePassiveObjectPacket(相对xyz) | ctx.CreateBullet(BulletIds.Xxx)（出生=身前0.8单位） | ✅ |
| PO 表现 ani + 特效锚点 | LSBulletView 同摆位公式 | ✅ |
| attackInfo 判定盒 | BulletDefinition.HalfExtents | ✅ 简化版 |
| PO 链式前进 | 弹移动单体 | 设计取舍不同，均可行 |
| 世界 x/y/z(px) | TSVector(x横向,y高度,z纵深)/100 | ✅ |

## 6. 本篇速查卡

1. 链路：按键→skl(数值/资源)→nut状态脚本→角色ani关键帧([SET FLAG])→生成PO(相对xyz)→PO自管位置+表现ani+attackInfo判定→命中。
2. 生成坐标是**相对施法者、含方向语义**的像素偏移（x=沿面向、y=纵深道、z=高度）；半身高特效用 getObjectHeight()/2。
3. 近战判定=.ani ATTACK BOX 帧驱动；投射物判定=attackInfo 配置——两套来源别混。
4. 波动"前进"=链式生成 PO（每段独立）。
5. 特效位置问题：先查生成点（第4层），再查特效摆位（第5层）——见 06 篇 §1。

---
**下一篇**：[05 · Unity 转换](05-Unity转换：从DNF像素到Unity单位.md) —— 这些数值落到 Unity/TSVector 的公式与代码。
