# 汲血之力（BLOODINKANET）

> 技能ID 117 | 级别 C | 可实现性 ⛔（数值主体撞属性消费链；霸体子效果可延后档简化） | 分析日期 2026-08-22 | 批次 C4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 汲血之力 | `skill\Swordman\SwordmanNewSkill\BLOODINKANET.skl [name]` |
| 英文名 | BLOODINKANET（取 skl 文件名；[name2]="Blood Inkanet" 官方英文名） | 同上 |
| 职业 | 狂战士二觉段被动（[second growtype maximum level] 槽 8 非零 = 狂战二觉段；explain 点名嗜魂之手/灭魂之手=狂战技，互证） | 同上 |
| 学习等级 | 75 | 同上 [required level] |
| 最高等级 | 30 | 同上 [maximum level] |
| 类型 | passive（skill class 3） | 同上 [type] |
| 指令 / CD / MP | 无 | 同上 |
| static data | 空 | 同上 [static data] |
| 一句话效果 | 增加基本攻击力和技能攻击力；施放[嗜魂之手]、[灭魂之手]时进入霸体状态 | 同上 [explain] |

**level property（1 列，Lv1 → Lv30）**：模板 `基本攻击力和技能攻击力增加 : <float1>%%`，向量 `(-1,0,0.1)`。
col0：140→**+14.0%** → 720→**+72.0%**（每级 +20）。pvp 段 Lv1=1（同 010 的近乎禁用档）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（空壳 appendage + 基础技内真实引用）

- `sqr\character\swordman\bloodinkanet\ap_bloodinkanet.nut`：**空壳**（proc/onStart/onEnd no-op；
  注册了 `onAttackParent` 回调名但函数体缺失）；`beidong\` 有同名副本；
- **真实引用在基础技**——`sqr\character\swordman\grabhand\grabhand.nut`（嗜魂之手，技能 26/31 状态）两处实测：
  ```
  line 107（抓取命中时）: if (sq_GetSkillLevel(obj, 117) > 0) sq_SetCustomDamageType(obj, true, 1)
  line 65 （离开状态 26）: if (sq_GetSkillLevel(obj, 117) > 0) sq_SetCustomDamageType(obj, false, 0)
  ```
  即学得 117 → 嗜魂之手抓取期间启用"自定义伤害类型 1"（引擎侧受击保护语义——
  与 explain 的"霸体"是相邻但不同的实现面，DNF 抓取期间本就带保护，此为强化；精确语义未考证，§8）；
- 灭魂之手侧：grep `117` 于 bloodsnatch/outbreak/rage 等狂战技 nut **无命中**——
  "施放灭魂之手霸体"在本 pvf 未见脚本支撑（疑引擎通用管线 or 未实装，§8）；
- `passive_skill_swordman.nut` 无 case 117；无 PO/动画/特效（grep 实测）。

**结论**：数值主体（col0）走引擎标准数值管线；霸体部分=基础技脚本内技能等级门禁（引擎 SetCustomDamageType）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | BLOODINKANET.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\SwordmanNewSkill\BLOODINKANET.skl` | ✅ 实测 | 1 列数值 |
| lst 条目 | swordmanskill.lst 439-440 行 | `…\pvf\skill\swordmanskill.lst` | ✅ 实测 | ID 117 → 本 skl |
| appendage | ap_bloodinkanet.nut（+beidong 副本） | `…\pvf\sqr\character\swordman\bloodinkanet\` / `beidong\` | ✅ 实测（空壳） | 无行为 |
| 基础技引用 | grabhand.nut:65/107 | `…\pvf\sqr\character\swordman\grabhand\grabhand.nut` | ✅ 实测 | 抓取期间伤害类型保护（117 门禁） |
| 灭魂之手侧引用 | —（无命中，实测） | `…\pvf\sqr\character\swordman\bloodsnatch\` 等 | ⛔ 缺失 | explain 承诺但无脚本支撑 |
| PO / 动画 / 特效 / atk | —（全查无） | 同 186 §3 各路径 | ⛔ 缺失 | — |
| 图标 | SkillIcon.img #426/#427 | `Character/Swordman/Effect/SkillIcon.img` | ✅ 实测（路径） | 无 UI 消费 |

## 4. 资源需求

**零资源需求**。

## 5. 实现方案草案

⛔ 主体暂缓 + 子效果可先行的拆分记档：

1. **攻击力 +14%~72%（主体）**：撞属性伤害消费链（176 §8）——公式立项前不做；届时一条数据行。
2. **"施放嗜魂之手/灭魂之手时霸体"（子效果）**：我方语义可降级表达为**施法期间免打断/免受击硬直**——
   但注意我们当前"受击-施法互斥"本身缺失（R1-A4：永不打断施法——即霸体在现系统里天然近似成立，
   空转）。若做显式霸体：`GhostHandSkill.OnCast` → `ctx.AddBuffToSelf(SuperArmorBuff)`
   （ForbidMoveOn/Off 同构改受击免疫位）——**霸体帧属延后档**（§6.3），且嗜魂之手本体是抓取系
   （Grab 系统缺失在档，031-GrabBlastBlood §5），故子效果跟随基础技一并暂缓。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| BLOODINKANET.skl | `.skl` 无子命令（1 列×30 级） | 手抄 2 值；全局已知缺口 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（引 §6.3 档位） | 简化建议 |
|---|---|---|
| 攻击力 +14%~72% | **缺失：属性伤害消费链**（第 13 实证） | 公式立项前不做 |
| 施放嗜魂之手时霸体（SetCustomDamageType） | 延后：霸体帧；且基础技嗜魂之手撞**抓取系统**（R2-A7 拆解在案） | 随嗜魂之手移植一并；我方受击本不打断施法，霸体近似天然成立 |
| 灭魂之手侧霸体 | 本 pvf 无脚本支撑（§2.1）——按未实装记档 | 不做 |
| pvp 0.1% 惩罚档 | 无 PvP 系统 | 忽略 |

## 8. 存疑与缺口上报

**未考证项**
1. `sq_SetCustomDamageType(obj, true, 1)` 的精确引擎语义（自定义伤害类型 1 = 免伤/减伤/不可被抓？——
   与"霸体"文案的对应关系为推断）。
2. 灭魂之手侧霸体无脚本支撑的原因（引擎通用管线 vs 本 pvf 未实装——灭魂之手本体技能号待其自身批次考证）。
3. ap_bloodinkanet.nut 注册了 `onAttackParent` 回调名却无函数体——mod 半成品痕迹（与 010 同族形态）。

**新系统级缺口**：无新上报（属性消费链/霸体/抓取系统均已在档）。

**翻译工具缺口**：`.skl` 子命令（全局已知，计 1 条）。
