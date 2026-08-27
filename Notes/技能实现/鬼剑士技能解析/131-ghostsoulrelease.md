# 鬼魂解放（ghostsoulrelease）

> 技能ID 131 | 级别 C（确认为**被动**——效果=宿主技能施放中无动作放置鬼阵，非主动技） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 鬼魂解放 | `ghost/ghostsoulrelease.skl [name]`（无 [name2]，英文名取文件名） | skl |
| 英文名 | ghostsoulrelease | skl 文件名 |
| 职业 | 鬼泣（[skill fitness growtype] = 2） | skl |
| 学习等级 | 20 | skl `[required level]` |
| 最高等级 | 50（[growtype maximum level] gt2=1——mod 数据矛盾，见 §8；level info 头=**0 列**：无等级数值，单级开关） | skl |
| 类型 | [passive]（skill class 1） | skl `[type]` |
| 一句话效果 | 裂波斩/鬼斩/月光斩/鬼影鞭/死亡墓碑/鬼影闪/二觉技等**施放过程中**，可无施放动作立即发动鬼阵技；位置随宿主技能前后方偏移；鬼斩/月光斩中发动需先学[噬灵鬼斩]/[满月斩] | skl `[explain]` |

**static data（dungeon）**：`100 -100 200 0` = **放置位置表**（由 ap 脚本 `sq_GetIntData` 消费，
见 §2.2）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测——本批唯一带完整行为脚本的技能）

- `passive_skill_swordman.nut:207-221 procSkill_ghostSoulRelease`：等级>0 → 常挂 appendage
  `character/swordman/ghostsoulrelease/ap_ghostsoulrelease.nut`（等级 0 → 摘）；
- `ap_ghostsoulrelease.nut`（171 行，实测通读）：proc 每帧执行——
  1. `getGhostSoulReleaseExecultableState`（同文件上方 :188）：当前状态 ∈
     {32, 20, 42, 65, 13, 33, 50, SLASHOFBOOM(237), SLASHOFHELL(238), BLADEPHANTOMEX(240), ZIGADVENT(241)}
     才放行（宿主技能白名单——20=鬼斩、42=月光斩已由技能门槛双印证，其余状态号对应裂波斩/鬼影鞭/
     死亡墓碑/鬼影闪/二觉技，个别未逐一考证）；
  2. 状态 20（鬼斩中）额外要求**技能 6（噬灵鬼斩）已学**；状态 42（月光斩中）要求**技能 80（满月斩）已学**
     ——与 explain 末句精确互证；
  3. `getGhostSoulRelease_Area_Distance`：按状态取 static 槽——32/20/42/65/狂怒→IntData(0)=
     **前方 100px**；33→IntData(1)=**后方 -100px**（鬼影闪位）；50/炼狱/幽魂式/吉格→IntData(2)=
     **前方 200px**；13→IntData(3)=**中央 0**（与 level property 文本四档一一吻合，破译实证）；
  4. 对 4 个鬼阵技逐个 `setCommandEnable(true)` + 检测 `sq_IsEnterSkill`：按下即
     `sq_SendCreatePassiveObjectPacketPos` 直接创建阵（无施法动作）：
     | 按下的技能 | 创建 PO | 对象（passiveobject.lst 定点查证） |
     |---|---|---|
     | 25 刀魂之卡赞 | 20011 | `Character/Swordman/Khazan.obj` |
     | 36 冰霜之萨亚 | 20012 | `Character/Swordman/Saya.obj` |
     | 41 侵蚀普戾蒙 | 20013 | `Character/Swordman/Bremen.obj` |
     | 95 墓碑三绝阵 | 20060 ×3（-250/+75/-45 三点三角） | `Character/Swordman/TombStoneEx.obj` |
     重复按下先 `killPassiveObject` 清旧阵（同 id 全灭重建）。
- ⚠ explain 文本列的鬼阵是"普戾蒙/萨亚/**冥祭之沼/幽魂之布雷德**"，脚本实放的是
  **卡赞/萨亚/普戾蒙/墓碑三绝阵**——文本与代码不一致（explain 疑 mod 改写或版本差，见 §8）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | ghostsoulrelease.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\ghost\ghostsoulrelease.skl` | ✅（63 行） | 位置表+说明 |
| 注册 | passive_skill_swordman.nut:207-221 | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut` | ✅ 实测 | 常挂 appendage |
| appendage | ap_ghostsoulrelease.nut | `…\pvf\sqr\character\swordman\ghostsoulrelease\ap_ghostsoulrelease.nut` | ✅（171 行，完整逻辑） | 每帧检测+放阵 |
| 阵对象 | Khazan.obj / Saya.obj / Bremen.obj / TombStoneEx.obj | `…\pvf\passiveobject\character\swordman\`（lst:11155-11253 定点） | ✅ | 被放置的鬼阵（各自技能批已析：025/036/041；95 未析） |

无自有 .ani/.atk（无施法表现；视觉全在各阵对象侧）。

## 4. 资源需求

无增量（缺失 img = 0）——阵本体资源在 025-Khazan/036-Saya/041-Bremen 各文档记账
（TombStoneEx 待 095 批）。

## 5. 实现方案草案（🔶）

- **机制映射**：DNF"技能施放中按另一技能键立即放阵" → 我们的**宿主技能 SkillLogic.OnUpdate 内
  二段输入消费**：
  - 在 058 裂波斩 / 005 鬼斩 / 077 月光斩 / 111 鬼影鞭 / 044 死亡墓碑 / 060 鬼影闪 / 237-241 二觉技
    的 OnUpdate 加"鬼魂解放"分支：`ctx.PeekBufferedButton()` 检测鬼阵键位 →
    `ctx.ConsumeBuffer()` + `ctx.CreateArea(AreaIds.Khazan, 施法者位置 ± 偏移)`；
  - 偏移按宿主技能查 static 表（前方 100 / 后方 -100 / 前方 200 / 中央 0——`CreateArea(areaId, position)`
    面板已有，绝对位置由 `GetTargetPosition`/朝向计算）；
  - 门槛：鬼斩宿主需"噬灵鬼斩已学"、月光斩宿主需"满月斩已学"——**跨技能 level 查询门面**（在案）；
- **前置依赖**：四个阵技本体（025/036/041 已析 ✅、095 未析）；"同 id 重建"用 Area 生命周期天然
  支持（重建=再 CreateArea）。
- **注册点**：无新 SkillId/AnimId（被动+复用阵技资源）；若按键需专属映射则加 ButtonToSkill 键位。
- 不占号段（SkillIds 34 起本技能未用）。

## 6. 翻译工具适配

`.skl` 无子命令（全局已知，本技能零数值列）；无 .ani/.atk 增量。**无新增翻译缺口**。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（在案） | 简化建议 |
|---|---|---|
| 技能施放中消费另一技能键 | 技能二段交互门面（R4-A16 四实证——本技是**第 5 例**且形态不同：消费的不是"再按本技能键"而是**别的技能键**，输入语义更接近技能取消体系 R1-A3） | 宿主技能内硬编码键位检测（PeekBufferedButton 已有，消耗一次可行）；通用化并入"技能取消/二段交互"专题 |
| 鬼斩/月光斩宿主门槛 | 跨技能 level 查询门面（在案 R3-A11） | demo 免门槛或写死 true |
| 位置四档偏移 | 无（CreateArea(position) 已有） | 直译 static 表 |
| explain 与代码的鬼阵清单不一致 | —（存疑项，非缺口） | 以代码 4 技为准 |

## 8. 存疑与缺口上报

- 未考证①：宿主状态号 32/33/65/13 与裂波斩/鬼影鞭/死亡墓碑的对应（20/42 已双印证，其余按
  explain 顺序推断）。
- 未考证②：explain 列"冥祭之沼(247)/幽魂之布雷德(239)"但代码放 95 墓碑三绝阵（PO 20060）——
  判 explain 为跨版本文本（或 mod 改写），**实现以代码为准**。
- 未考证③：[growtype maximum level] gt2=1 与 [maximum level] 50 矛盾（mod 数据，常见形态）。
- 缺口上报（主循环汇总）：**"施放中消费其他技能键"作为二段交互门面的第 5 实证**，建议在该缺口
  立项描述中把"消费语义"从"再按本键"扩为"任意已配置技能键"，本技能是最完整样本（脚本全存）。
