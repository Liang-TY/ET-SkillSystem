# 强化-血气唤醒（PinchPowerUpEx）

> 技能ID 139 | 级别 E | 可实现性 🔶（阈值提升零成本落地；血之狂暴 HP 消耗调制随基础技/关联技缺口跳过） | 分析日期 2026-08-22 | 批次 E3

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 血气唤醒 | `PinchPowerUpEx.skl [name]` |
| 英文名 | PinchPowerUpEx（skl 文件名；[name2]=`血气唤醒 UpGrade`） | 同上 |
| 职业 | 狂战士 + 剑影（[skill fitness growtype]=3；[growtype maximum level] `0 0 0 1 0 1`——狂战/剑影各 1 级） | 同上 |
| 学习等级 | 55（[required level range] 5；**前置：技能 19 血气唤醒 Lv5**，[pre required skill] `19 5`） | 同上 |
| 最高等级 | **1**（TP 1 级封顶，E3 批唯一单级 TP 技） | 同上 [maximum level] |
| TP 消耗 | [special purchase cost] 5 | 同上 |
| 类型 | passive（[feature skill type] 1，skill class 2） | 同上 |
| 指令 / CD / MP | 无（纯被动强化） | 同上 |
| 一句话效果 | 血气唤醒触发阈值 40%→60%；并按 HP 分段调制"血之狂暴"的 HP 变化量（高档大减、低档大增） | 同上 [explain ex] |
| 基础技 | 19 血气唤醒（`019-PinchPowerUp.md`，🔶）；基础 skl [feature skill index]=139 双向链接证 | 两 skl 实测 |

## 2. 强化增量（对照 019-PinchPowerUp.md）

### 2.1 TP 数据表解码（L21 向量法，10 向量全解，零未解）

static data（dungeon）= **`60 25 250 60 10`**，配合 [level property] 向量逐条解出：

| 向量 | 模板行 | 解码值 |
|---|---|---|
| (3,3,×1.0) | `HP在 <int>%% 以上时…` | static[3]=**60%**（高档分界） |
| (4,4,×0.01) | `…血之狂暴效果的HP变化量 : <float1>倍` | static[4]=10→**×0.1 倍**（HP≥60% 时血之狂暴 HP 消耗大减） |
| (1,1,×1.0) | `HP在 <int>%%以下时…` | static[1]=**25%**（低档分界） |
| (2,2,×0.01) | `…HP变化量 : <float1>倍` | static[2]=250→**×2.5 倍**（HP≤25% 时大增） |
| (-1,0/1,×1.0) 等 6 组 | 力量/回避/攻速/移速 min~max | 四路属性列（同基础技 8 列结构，**本技无 [special level up]，TP 不加属性**） |

- static[0]=**60 = 新触发阈值**：基础技 static=40 → TP 后 60%（explain"最低发动 HP 条件增加 20%增" = +20 百分点，直读互证）。
- [level info] 8 列 ×70 行**与基础表不完全一致**（Ex Lv1 力量 24/61 vs 基础 36/92；col4-7 同值）——独立平衡表，差异规则未考证（见 §8）。

### 2.2 增量逐条

| # | 增量 | 数据源 | 落我们侧 |
|---|---|---|---|
| 1 | 触发阈值 40%→60% | static[0] | 两档简化版改一个常量，✅ 零成本 |
| 2 | 血之狂暴（技能 76 Frenzy）HP 变化量：HP≥60% ×0.1 / HP≤25% ×2.5 | static[1..4] | ⛔ Frenzy 本身 ⛔（076 档：普攻派生改造四重缺口），调制无从挂——跳过 |
| 3 | 习得后显示血气唤醒资讯（UI） | explain ex | 不做 |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | PinchPowerUpEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\PinchPowerUpEx.skl` | ✅ | TP 数据（static 5 值 + 8 列表） |
| 基础 skl | PinchPowerUp.skl（[feature skill index] 139） | 同目录 | ✅ | 双向链接 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（基础档已 grep pinch 零命中；passive_skill case 表 248/254/252/123/171/209/…/78 无 139，实测） | ⛔ | TP 增量消费在引擎 feature skill 体系 |
| 基础技文档 | 019-PinchPowerUp.md | 本目录 | ✅ | 基础走读引用 |
| 关联技 | Frenzy.skl（76 血之狂暴） | 同 .skl 目录 | ✅（存在） | 增量 #2 的被调制对象 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| （无——TP 被动零资源；图标用 SkillIcon.img #312/313，不做 UI） | — | — | — | — |

缺失 img：**0**。

## 5. 实现方案草案（增量落地）

- 前提：基础技 019 的"两档简化"（HP<阈值挂 max 值 Buff、回复即摘）。本 TP 增量只需把阈值常量 0.40 → **0.60**（`PinchPowerBuff` Tick 内判定值），无新内容件、无新注册点。
- demo 形态：TP 等级以配置常量注入（TP/技能等级系统是 R6-C1 已记档缺口，不阻塞单值增量）。
- 关键数值：阈值 60%（static[0] 直译）；HP 段调制 ×0.1/×2.5（数值就绪、待 Frenzy 立项，记档不实现）。

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| PinchPowerUpEx.skl | `.skl` 无子命令（static 5 值 + 10 向量） | 手抄 5 值可行；`skl` 子命令同前议 |

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 阈值 40→60% | 无缺口 | 常量直改 |
| 血之狂暴 HP 变化量分段调制 | 关联技 Frenzy ⛔（普攻派生改造四重缺口，076 档）+ HP 消耗管线 | 跳过（数值已解出记档） |
| 四路属性 min~max 插值 | 随基础技（受击管线钩子/属性消费链） | 同基础档两档简化 |

## 8. 存疑与缺口上报

**未考证项**
1. Ex 自有 8 列表与基础表的差异规则（24/61 vs 36/92 @Lv1，col4-7 同值）——疑 TP 版独立平衡表，引擎消费方式未知。
2. static[0]=60 与"基础 static 40 + 引擎加算"的关系（本档按 Ex 覆盖读，Ex [explain] ���本写 60% 互证）。

**新缺口**：无新增系统级缺口。翻译工具：`.skl` 子命令（重复印证）；**[special level up] 节为本批（E 类）首见的新节名**（本技能无此节，节家族记档见批次总评，skl 子命令设计需纳入 TP 增量表）。
