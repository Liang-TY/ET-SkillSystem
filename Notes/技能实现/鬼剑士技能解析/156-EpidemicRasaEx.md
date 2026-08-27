# 强化-瘟疫之罗煞（EpidemicRasaEx）

> 技能ID 156 | 级别 E（TP 强化被动） | 可实现性 ⛔（=基础版 075 ⛔——唯一增量"单敌鬼神上限 +1/级"落在 Buff Stack 上限预留位（小框架字段），但附身核心三减益/失明仍缺系统，整体随基础技 ⛔） | 分析日期 2026-08-22 | 批次 E5

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 瘟疫之罗煞 | `EpidemicRasaEx.skl [name]` |
| 英文名 | EpidemicRasaEx（skl 文件名；[name2]=`Rhasa of Epidemic Upgrade`） | 同上 |
| 职业 | 鬼泣（[growtype maximum level] `0 0 5 0 0 0`；[skill fitness growtype]=2） | 同上 |
| 学习等级 | 65（[required level range] 5）；前置：技能 75 瘟疫之罗煞 Lv1 | 同上 |
| 最高等级 | 10（TP） | 同上 |
| TP 消耗 | 2/级 | 同上 |
| 类型 | passive（skill class 3；[feature skill type] 1；耐久 70） | 同上 [type] |
| 一句话效果 | 附在敌人身上的鬼神数上限 +1/级 | 同上 [explain ex] |
| 基础技文档 | 075-EpidemicRasa.md（⛔ 三减益消费链 + 失明系统） | 本目录 |

## 2. 强化增量（对照 075-EpidemicRasa.md）

### 2.1 数据表形态：整表镜像 + static[6] 覆写

- [level info] 20 列 × 70 行逐值镜像（python diff 实测 diff=0——本批最宽镜像表）。
- static：dungeon `450 1000 100 100 100 80 350 4 3`——**static[6] 250→350 覆写**（召唤范围 ×2 = 500→**700px**，模板 (6,6,2.0) 实证）；static[8]=3 同基础。基础另有 pvp static `450 1000 80 60 50 50 250 2 3` 而 Ex pvp 无 static 节。
- [special level up]（dungeon）**单行**：`8 8 + 1`（static[8] 附身鬼神上限 +1/级）；pvp 单行 0。

### 2.2 增量明细

| 增量项 | 数据源 | 每 TP 级 | TP10 |
|---|---|---|---|
| 单敌附身鬼神上限（static[8]=3） | `8 8 + 1` | +1 | **13** |
| （static[6] 覆写 250→350） | 无四元组（覆写即生效） | — | 召唤范围 500→700px |

**增量性质**：数量上限档——非纯数值（改变叠层容量=行为容量），但载体是 075 §8-2 已记档的 **BuffDefinition MaxStack 预留位**（小框���字段，非新系统）。

### 2.3 模板解码旁证（20 列 → 19 向量）

19 向量解 19 占位符（持续时间 col0=20s、召唤范围 static[6]×2、三减益 col2/3/4、中毒 col6-9（源 **-3**）、失明 col10-14、出血 col15-18（源 **-4**）、附身持续 col19=10s）——与基础档 075 §1 解码一致；col1=10000/col5=2000 两列仍无引用（075 §8-4 遗留维持）。源 -3/-4（DoT 伤害列专用源语义未解）本批再证（149 col11 同 -4）。

### 2.4 资源增量

**0 新增**（图标 SkillIcon.img；无 [skill preloading image]——基础档预载清单本就是 mod 错列，075 §3 已记）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | EpidemicRasaEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\EpidemicRasaEx.skl` | ✅（275 行全读） | 镜像+覆写表 + 单行 TP 增量 |
| lst 条目 | ID 156 | `…\pvf\skill\swordmanskill.lst` 395-396 行 | ✅ | — |
| 反向链接 | EpidemicRasa.skl [feature skill index]=156 | 同目录 | ✅ 实测 | 双向互指 |
| 脚本 | —（无） | `…\sqr\character\swordman\`（grep epidemicrasaex 零命中） | ⛔ | 引擎消费 |
| 基础技文档 | 075-EpidemicRasa.md | 本目录 | ✅ | 数值/缺口继承源（075 §8-5 的"E 类另行分析"即本档收口） |

## 4. 实现方案增量（并入 075 §5 草案）

075 分层草案（RasaFieldArea + RasaGhostBuff）落地时，TP 即两参：

| 参数 | 基础版 | TP10 并入后 |
|---|---|---|
| RasaGhostBuff MaxStack | 3 | 13 |
| 领域 HalfExtents | 5.0（500px） | **7.0（700px，static[6] 覆写）** |

⚠ BuffDefinition MaxStack 是预留位未实现（075 §8-2 首需求）——本档把上限从 3 抬到 13，**附身叠层的伤害面（每层异常）依赖 Tick 挂 Buff 叠加，上限字段实现后即通**。

## 5. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| EpidemicRasaEx.skl | `.skl` 无子命令（20 列本批最宽 + static 覆写 + 单四元组） | 随 skl 子命令（常驻；宽表样本再证） |

翻译缺口 1 条（.skl，常驻）。

## 6. 困难与简化

| DNF 原版行为 | 缺口/困难（§6.3 档位） | 简化建议 |
|---|---|---|
| 鬼神上限 3→13 | BuffDefinition MaxStack 预留位未实现（075 §8-2，小改） | 字段补齐后直译 |
| 召唤范围 700px（覆写） | 无缺口（HalfExtents） | 直译 |
| 上限内每层附身的减益/异常 | 属性消费链 + 失明系统（075 ⛔ 主因不变） | 随基础档 |
| pvp 不强化 | 无 PVP 分流 | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. static[6] 覆写 350（无四元组）与 static[8] 四元组 +1 并存——覆写型与增量型在同 skl 内混用（引擎合成顺序未考证；实现侧取"覆写基准 + 增量步长"惯例）。
2. Ex pvp 无 static 节（基础有）——pvp 覆写是否回落基础值，未考证。
3. col1=10000/col5=2000 无引用列（075 §8-4 遗留，无新证据）。
4. 源 -3/-4 的语义（DoT 伤害列专用源；L21 未解族——本案与 149 同批两证，建议收尾统一）。

**缺口上报**：**BuffDefinition MaxStack 需求量化输入**——075 首记"上限 3"，本档给出上限变量范围（3→13）——字段设计按 int 即可，非 enum/小位宽；其余无新系统级缺口。

**给轮间经验**：TP 的 **static 覆写型（无四元组直接换值）与增量型（四元组）可混用**于同一 skl（156 的 [6] 覆写 + [8] 增量）——skl 子命令解析时两种生效路径都要建模。
