# 幻鬼步（ReturnSpiritMove）

> 技能ID 128 | 级别 B（预判 A 纠偏：无伤害纯位移技） | 可实现性 ⛔（核心依赖幻鬼实体系统 + 无敌帧 + 位置传送门面，见 §5 前提清单） | 分析日期 2026-08-22 | 批次 A6

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幻鬼步 | `skill\Swordman\ghostsword\returnspiritmove.skl [name]` |
| 英文名 | ReturnSpiritMove（skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype]=5） | 同上 |
| 学习等级 | 20（前置：126 鬼步 Lv1） | 同上 [pre required skill] |
| 最高等级 | 1（growtype maximum：共通 1 / 剑影 1） | 同上 |
| 类型 | active（skill class 1） | 同上 |
| 指令 | Space（`{6=(BUFF)}`，BUFF 键位） | 同上 [command] |
| CD | 2000 ms（地下城，[auto cooltime apply] 缺省）；pvp 起手 CD 10000 + CD 20000 | 同上 |
| MP | 336（pvp 168） | 同上 [consume MP] |
| 特殊消耗 | 无 | 同上 |
| 可施放状态 | 近乎全状态（0-75 连续 + 106/105/117/111/112/115/119/122/127）——**几乎任何动作中可用** | 同上 [executable states] |
| static data | 空（dungeon 无 [static data] 值） | 同上 |
| 一句话效果 | 幻鬼分离状态下，剑影瞬间移动到幻鬼身边：移动全程 + 到达后 0.5 秒无敌；每次幻鬼分离只能用一次；决斗场无到达后无敌 | 同上 [explain] |

**level property**：1 列 ×1 向量：dungeon `500` / pvp `0`，向量 `(-1, 0, 0.001)` → **列 0 × 0.001 = 0.5 秒**（无敌时间，L21 细化读法，nut `sq_GetLevelData(128, 0, level)` 实证同值）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

```
162: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/5_ghostsword/returnspiritmove/returnspiritmove.nut", "returnspiritmove", STATE_RETURNSPIRITMOVE, SKILL_RETURNSPIRITMOVE);
```

（`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\sqr\character\swordman_load_state.nut` 实测；状态 113 / 技能 128。）

**前置依赖**：`getVSObject(obj)`（`…\sqr\character\jg_swordman\jg_swordman_common.nut:892`）——遍历自己名下共享 PO 24349，var id ∈ {61,62,63,65,66,68,70,74} 者为"在场幻鬼"。无幻鬼 = 本技能不可用（对应 [explain] "幻鬼分离状态下使用"）。幻鬼由鬼步(126)/幻鬼:一闪(135) 等技创建。

### 2.2 主 nut 逐回调（returnspiritmove.nut，122 行，实测）

- `checkExecutableSkill_returnspiritmove`：无 VSObject → `startCantUseSkillWarning()` + 消息 71099（"幻鬼未分离"提示），拒绝；有 → 切状态 STATE_RETURNSPIRITMOVE + `als_ani` 两道起始特效（`returnspiritmovestart_00/01.ani`）。
- `onSetState` sub0（**瞬时技能，本体无动画**）：
  1. **立刻切回 STATE_STAND**（本状态不占动画时间）。
  2. `returnSpiritMoveEffect(obj, VSObject)`：在自身位置创建 pooled 特效 `moveeffect_move.ani`（z+67 高度），按两位置连线**运行时拉伸**（`setImageRate(distance/200, 1.0)`）+ 旋转（`sq_SetCustomRotate(angle × distance/200)`）——剑影到幻鬼的残影光束。
  3. 朝向幻鬼方向；`sq_MoveToNearMovablePos(幻鬼x, 幻鬼y, …, 20, -1, 3)` ——**瞬移到幻鬼附近**（寻路可落点）。
  4. 非 PVP：`sq_SendMessage(obj, OBJECT_MESSAGE_UNBREAKABLE, 1, 0)` 立即无敌 + `sq_PostDelayedMessage(UNBREAKABLE 0, lastTime=500ms)` 定时解除——**全程无敌 0.5 秒**（列 0 × 0.001）。
  5. 音效 RETURNSPIRIT_MOVE；`als_ani` 两道到达特效（`returnspiritmoveend_00/01.ani`）。
- `onEndCurrentAni`：回 STATE_STAND（实际早已回）。

无攻击信息、无 .atk、无被动对象、无角色动画——**纯位移 + 无敌 + 特效**技能。

### 2.3 被动对象 / appendage

无。无敌走引擎 OBJECT_MESSAGE_UNBREAKABLE 消息（非 appendage）。

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 备注 |
|---|---|---|---|---|---|
| `character\swordman\effect\animation\returnspiritmove\moveeffect_move.ani` | 10 | 500ms | 无 | 无 | 位移光束（**GRAPHIC EFFECT LINEARDODGE**；nut 运行时拉伸/旋转——帧数据不含） |
| `…\returnspiritmovestart_00.ani` / `_01.ani` | 10 / 8 | 600 / 480ms | 无 | 无 | 起点雾（Mist.img）/ 幻影（Illusion.img） |
| `…\returnspiritmoveend_00.ani` / `_01.ani` | 10 / 8 | 600 / 480ms | 无 | 无 | 终点雾/幻影（与起点共用两张 img） |

角色侧：**无专属 .ani / 无 .chr 条目**（grep 实测无 returnspirit 条目）；.als：无。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | returnspiritmove.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghostsword\returnspiritmove.skl` | ✅（100 行） | 1 列等级数据（无敌时长） |
| 注册行 | swordman_load_state.nut:162 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ | 状态 113 / 技能 128 |
| 主 nut | returnspiritmove.nut | `…\pvf\sqr\character\swordman\5_ghostsword\returnspiritmove\returnspiritmove.nut` | ✅（122 行） | 门禁/瞬移/无敌/特效 |
| 幻鬼判定 | jg_swordman_common.nut:892 | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅ | getVSObject |
| .chr 条目 | — | `…\pvf\character\swordman\swordman.chr` | ⛔ 无条目 | 瞬时技能无动画 |
| 角色 .ani | — | `…\pvf\character\swordman\animation\` | ⛔ 不存在 | 同上 |
| .atk | — | `…\pvf\character\swordman\attackinfo\` | ⛔ 不存在 | 无伤害 |
| 特效 .ani | moveeffect_move / start_00,01 / end_00,01 | `…\pvf\character\swordman\effect\animation\returnspiritmove\` | ✅（5 个，无 .als） | 位移视觉 |
| 装备层 | — | `…\pvf\equipment\character\swordman\avatar\` | ⛔ 不存在 | 无角色动作 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| Character/Swordman/Effect/ReturnSpiritMove/Move.img | sprite_character_swordman_effect_returnspiritmove.NPK | 位移光束 | 可选（⛔ 级，实现时升必需） | ❌ |
| …/ReturnSpiritMove/Mist.img | 同上 | 起点/终点雾 | 可选（同上） | ❌ |
| …/ReturnSpiritMove/Illusion.img | 同上 | 起点/终点幻影 | 可选（同上） | ❌ |

**缺失 img：3 张（全部可选档，随 ⛔ 解锁提取），1 个 NPK。**

## 5. 实现方案草案（⛔ 级——前提清单代替草案）

按 §6.3 判据归 **缺失档**，三项系统级前提：

1. **幻鬼实体系统**（最重）：目标点 = 在场幻鬼位置（VSObject）。需要"玩家可控外的伴随实体"概念（创建/持有/位置查询/销毁）。剑影全族（鬼步 126/一闪 135/幻鬼步 128/贯穿 136…）共享此依赖——建议作为"剑影族"整族立项，而非为 128 单独打洞。已在缺口累计（召唤物实体，135 文档 §8 呼应）。
2. **无敌帧通道**：OBJECT_MESSAGE_UNBREAKABLE ↔ 我方需受击侧无敌判定（MeleeHit/受击系统加免疫窗口）。R1-A5 已记档（位移技通用），本技能再 +1 例（0.5s 定时无敌）。
3. **单位位置设置门面**：`SkillContext` 只有 `MoveCasterForward`（朝向增量位移），无 `SetCasterPosition(x,z)`。瞬移语义需要绝对位置写入（~5 行门面 + LSCast 无状态，帧同步安全——小改非系统级，但单独为它改框架不划算，随剑影族一起）。

**次级缺口**（可降级，不阻塞）：位移光束的运行时拉伸/旋转（setImageRate/setCustomRotate）↔ IMAGE RATE/ROTATE 延后档——若实现，光束改为"固定长度拖尾特效"近似；"每次分离仅一次"计数门禁 ↔ 无幻鬼状态查询（归入前提 1）。

若三前提齐备：内容件仅需一个瞬时 SkillLogic（OnCast 里 SetCasterPosition(幻鬼位) + PlayAnim 无 + AddBuffToSelf(无敌 Buff 500ms) + 两端特效 als_ani 同构 overlay），无 Area/Bullet/新 Action——形态与 returnspiritmove.nut 一一对应，无额外难度。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| returnspiritmove.skl | `.skl` 无子命令 | 单列数值手抄无碍（累计缺口） |
| moveeffect_move.ani | `[GRAPHIC EFFECT] LINEARDODGE` | **工具已支持**（L15：graphicEffect 字段，消费侧 AnimClipData 已接）——非缺口 |
| 运行时 setImageRate/setCustomRotate | 非 .ani 内容（nut 运行时行为） | 归游戏侧"对象整体缩放/旋转"延后档，非翻译缺口 |

**本技能翻译缺口 1 类（.skl）；ani 全覆盖。**

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 传送到幻鬼位置 | **缺失：幻鬼实体系统**（前提 1） | 无简化可保技能身份——暂缓整技能；demo 可做"向前瞬��� 4.5 单位"占位验证传送门面 |
| 全程 + 0.5s 无敌 | **缺失：无敌帧**（R1-A5 记档） | 暂缓；或仅做特效不做无敌（失去主要价值，不推荐） |
| 光束按两点距离拉伸/旋转 | 延后：IMAGE RATE/ROTATE 同类（运行时） | 固定长度拖尾特效近似 |
| 每次分离限用一次 | 缺失：幻鬼状态查询（前提 1 子项） | 暂缓 |
| 近乎全状态可施放 | 延后：施法互斥口径（我们目前技中不能施放） | 到达后/待机时可用即可 |
| pvp 差异（CD 20s/无无敌） | 延后：无 pvp 分支 | 不做 |

## 8. 存疑与缺口上报

- **未考证**：`sq_MoveToNearMovablePos(…, 20, -1, 3)` 的寻路参数语义（20/-1/3 疑为间距/弧度/步长）；消息 71099 文案归属；"移动过程中无敌"的精确起止（推断=状态进入即无敌，500ms 后解除，与到达后无敌合并计时）。
- **系统级缺口（累计补证）**：无敌帧（本批 128 为第 2 例，R1-A5 首报）；召唤物实体记忆（135/128 双技能依赖，剑影族整族共享——建议主循环在 01§0.4 增"伴随实体/召唤物"行时注明剑影族与鬼泣召唤系双用户）。
- **框架小项**：`SkillContext.SetCasterPosition` 传送门面（§5 前提 3，~5 行）——多技能会用到（投掷系/传送系），建议与跳跃系统或剑影族立项搭车。
- 本技能无伤害、无判定体、无角色动画，是 241 技能里少数"纯位移"样本，可作将来"位移技分类"的最小参照。
