# 御鬼之极（GHOSTRELEASE）

> 技能ID 10 | 级别 C | 可实现性 ⛔（属性数值无伤害消费链） | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 御鬼之极（批次清单预注"鬼神解放"——本 pvf 已改名，skl [name] 实测为御鬼之极） | `skill\Swordman\SwordmanNewSkill\GHOSTRELEASE.skl [name]` |
| 英文名 | GHOSTRELEASE（取 skl 文件名；[name2]="Ghost Release" 官方英文名） | 同上 |
| 职业 | 鬼泣二觉段被动（[second growtype maximum level] 槽 6 非零 = 鬼泣二觉段；[skill fitness second growtype] `2`） | 同上 |
| 学习等级 | 75 | 同上 [required level] |
| 最高等级 | 30 | 同上 [maximum level] |
| 类型 | passive（skill class 3） | 同上 [type] |
| 指令 / CD / MP | 无 | 同上 |
| static data | 空 | 同上 [static data] |
| 一句话效果 | 增加基本攻击力和技能攻击力 | 同上 [explain] |

**level property（1 列，Lv1 → Lv30）**：模板 `基本攻击力和技能攻击力增加 : <float1>%%`，向量 `(-1,0,0.1)`。
col0：110→**+11.0%** → 400→**+40.0%**（每级 +10）。pvp 段仅 1 行 Lv1=1（PvP 砍到 0.1%——近乎禁用）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（目录存在但全为空壳/遗留——判引擎内置数据管线）

`skill\Swordman\SwordmanNewSkill\GHOSTRELEASE.skl`（75 级二觉段被动族，与 BLOODINKANET 同目录同构）。
脚本侧存在 `sqr\character\swordman\ghostrelease\` 目录，但逐一实测：

- `ap_ghostrelease.nut`：**空壳**（proc/onStart/onEnd 全 no-op）；`beidong\ap_ghostrelease.nut` 同名副本亦空壳；
- `po_khazan.nut` / `po_bremen.nut`：**遗留死代码**（详见 §2.2——引用未定义常量、读不存在的列）；
- `load_state:124-125` 仅 pushScriptFiles 加载上述两 PO 脚本（**无 pushState**——不是状态注册）；
- `passive_skill_swordman.nut` 无 case 10；无 PO 对象、无专属动画/特效（grep 实测）；
- 注意与 131 鬼魂解放（ghostsoulrelease，`SKILL_GHOST_SOUL_RELEASE <- 131`）是**两个不同技能**——
  131 有完整 ap/区域逻辑（procSkill_ghostSoulRelease），勿混。

**结论**：本 pvf 中御鬼之极 = 纯数值被动，col0 走引擎标准数值管线；脚本目录为早期版本遗留。

### 2.2 遗留死代码样本（po_khazan.nut 实测，供收尾期甄别）

```
onCreat_Khazan: attackPower = sq_GetPowerWithPassive(SKILL_GHOST_RELEASE, -1, 0, -1, 1.0)  // 读 col0
onProc_Khazan:  interval = sq_GetLevelData(sqrChr, SKILL_GHOST_RELEASE, 1, skillLevel)    // 读 col1！
               每 interval 对半径内敌人 sq_SendHitObjectPacketWithNoStuck（卡赞自动攻击光环）
```
两个致命破绽：①`SKILL_GHOST_RELEASE` 常量在全部白名单脚本树中**无定义**（仅此两文件引用）；
②读 col1 作攻击间隔——当前 skl 只有 1 列（col1 不存在）。即这段"鬼神自动攻击光环"逻辑对应的
是**旧版技能数据结构**，现 skl 已改版为纯数值被动，脚本未随之更新 → 判死代码（高置信）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GHOSTRELEASE.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SwordmanNewSkill\GHOSTRELEASE.skl` | ✅ 实测 | 1 列数值 |
| lst 条目 | swordmanskill.lst 437-438 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 10 → 本 skl |
| pushScriptFiles | load_state:124-125 | `…\pvf\sqr\character\swordman_load_state.nut` | ✅ 实测 | 仅加载脚本（非状态注册） |
| appendage | ap_ghostrelease.nut（+beidong 副本） | `…\pvf\sqr\character\swordman\ghostrelease\` / `beidong\` | ✅ 实测（空壳） | 无行为 |
| PO 脚本 | po_khazan.nut / po_bremen.nut | `…\pvf\sqr\character\swordman\ghostrelease\` | ✅ 实测（死代码，§2.2） | 旧版自动攻击光环遗留 |
| PO 对象 / 动画 / 特效 / atk | —（无创建方，白名单全查） | `…\pvf\passiveobject\character\swordman\` 等 | ⛔ 缺失 | 死代码无对位资源 |
| 图标 | SkillIcon.img #440/#441 | `Character/Swordman/Effect/SkillIcon.img` | ✅ 实测（路径） | 无 UI 消费 |

## 4. 资源需求

**零资源需求**。

## 5. 实现方案草案

⛔ 暂缓——单数值撞**属性数值无伤害消费链**（176 §8；攻击力 +11%~40% 无处生效）。
届时形态：一条 `AddNumeric(AttackPct, +X%)` 数据行，零内容件。

（若未来想还原 §2.2 的旧版"鬼神自动攻击光环"语义：= 以在场鬼神 PO 为中心的 Tick Area——
依赖幻鬼/鬼神实体记忆（R2-A6）+ 在场 PO 查询，两缺口已在档；不建议按旧版做，explain 已不承诺该行为。）

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| GHOSTRELEASE.skl | `.skl` 无子命令（1 列×30 级） | 手抄 2 值；全局已知缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 攻击力 +11%~40% | **缺失：属性伤害消费链**（第 12 实证） | 公式立项前不做 |
| （旧版遗留）鬼神自动攻击光环 | 召唤物在场记忆（R2-A6）+ 死代码（§2.2） | 不还原——按现版纯数值被动处理 |
| pvp 0.1% 惩罚档 | 无 PvP 系统 | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. "死代码"判定的残余不确定性：`SKILL_GHOST_RELEASE` 可能定义于引擎预载的公共常量表
   （白名单外，不检索）——若其值=10 且引擎容错 col1 缺省，PO 仍可能被某未发现入口创建
   （白名单内无创建方，故维持高置信死代码判断）。
2. 改版前该技能是否真有"鬼神自动攻击"行为（现 explain 无此承诺）。

**新系统级缺口**：无新上报（属性消费链已在档；本技能新增价值=**改版遗留死代码的甄别样本**：
"脚本引用未定义常量 + 读不存在的 level 列"可作判死代码的两条速判法，供收尾期复用）。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
