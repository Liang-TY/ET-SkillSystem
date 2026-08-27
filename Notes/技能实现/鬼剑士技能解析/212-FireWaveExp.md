# 强化-爆炎波动剑（FireWaveExp）

> 技能ID 212 | 级别 E（TP 强化技） | 可实现性 ✅（=基础版 022 ✅ 双 Area 方案；增量两路攻击力 +10%/级纯数值，零新机制零新资源） | 分析日期 2026-08-22 | 批次 E6

## 1. 基本信息（.skl 实测）

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 爆炎波动剑（[name] 实测带空格"爆炎波动剑"与批次表"爆炎 · 波动剑"微差） | `FireWaveExp.skl [name]` |
| 英文名 | FireWaveExp（skl 文件名；[name2]=`波动剑 暴炎 Upgrade`——中文别名+Upgrade，L1 族） | 同上 |
| 职业 | 阿修罗（[skill fitness growtype]=4） | 同上 |
| 学习等级 | 65（[required level range] 5） | 同上 |
| 最高等级 | 10（TP；[growtype maximum level] `0 0 0 0 5 0`——阿修罗至多 5 级） | 同上 |
| TP 消耗 | 2/级 | 同上 [special purchase cost] |
| 前置 | 技能 22（爆炎 · 波动剑）Lv1 | 同上 [pre required skill] `22 1` |
| 类型 | passive（[feature skill type] 1；skill class 0 波动系） | 同上 |
| 一句话效果 | 火浪与爆炸攻击力各 +10%/级（explain 单条"攻击力 +10%"，数据两路同增） | 同上 [explain ex] |
| 基础技 | 22 爆炎 · 波动剑（`022-FireWave.md` ✅）；基础 skl [feature skill index]=212 双向链接（实测） | 两 skl 实测 |

> **与 E5 批 99 FireWaveEx（极炎 · 裂波剑，active）区分**：99 是主动替换技；212 是 TP。022 记档的 `firewavebig\ex_bead_fire_dodge.ani.als`（"Ex 系，不在基础版链路"）归属未定（疑 99），不属 212 链路。

## 2. 强化增量（对照 022-FireWave.md）

### 2.1 数据侧（E 类通用解码法）

- [level info]（71 行 ×2 列，末行 `23413 42746`）、[static data] dungeon `350` / pvp `450`、[level property] 2 向量（-2 0 魔攻 / -2 1 爆炸魔攻）——**全部与基础 skl 逐字节相同**（python 比对实测）。
- [special level up]（dungeon，2 行）：`-1 0 % 10`（col0 魔法攻击力 +10%/级）、`-1 1 % 10`（col1 爆炸魔法攻击力 +10%/级）。pvp 无该节（不强化）。
- **F1 波动剑族定点核验**：`sqr\character\swordman\wave\` 三 nut（wave.nut / po_wavecut.nut / ap_wavehold.nut）grep `212`/`FireWaveExp` **零命中**（实测）——wave.nut 唯一强化分支仍只读技能 100（IceWaveEx active，E5 批对象）；212 与 216（IceWaveExp TP）同形态：TP 消费全在引擎。

### 2.2 增量明细

| # | 增量 | 落我们侧 |
|---|---|---|
| 1 | 火浪攻击力 +10%/级 | FireWaveArea Damage ×(1+0.1×TP)——✅ |
| 2 | 爆炸攻击力 +10%/级 | FireWaveExplosionArea Damage ×(1+0.1×TP)——✅ |

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | FireWaveExp.skl（259 行） | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\FireWaveExp.skl` | ✅ 全节实测 | 镜像表 + TP 增量 |
| 基础 skl | FireWave.skl（[feature skill index] 212） | 同目录 | ✅ 实测比对 | 双向链接 |
| 脚本 | —（无 212 分支） | `…\pvf\sqr\character\swordman\wave\`（三 nut） | ⛔ 零命中（实测） | TP 消费在引擎 |
| 基础技文档 | 022-FireWave.md | 本目录 | ✅ | 继承源（双 Area 方案 A） |

## 4. 资源需求

**0 新 img / 0 新文件**（随基础档 022 §4 的必需 3 + 可选 4；static 350/450 语义仍随基础未考证）。

## 5. 实现方案草案（增量落地）

零新内容件/注册点，并入 022 §5 方案 A：

| 参数 | 基础版（022 草案） | TP 并入（建议 TP5 定值） |
|---|---|---|
| 火浪伤害 | 100 | ×1.5 → 150 |
| 爆炸伤害 | 150 | ×1.5 → 225 |
| 判定盒/时序 | ATKBOX 折算 (2.0,0.32,1.35) / 770ms + 630ms | 不变（大小类增量本技无） |

## 6. 翻译工具适配（DnfConfigTranslation）

| 文件 | 不支持的节/字段/文件类型 | 建议 |
|---|---|---|
| FireWaveExp.skl | `.skl` 无子命令（2 列镜像表 + 2 行增量 + static 单值 ×2） | 手抄可行；并入 skl 子命令缺口 |

翻译缺口计 1 条（.skl 类型）。

## 7. 困难与简化

| DNF 原版行��� | 缺口/困难 | 简化建议 |
|---|---|---|
| 两路攻击力 +10%/级 | 无缺口 | 常数倍率 |
| 火元素属性 / stuck 吸附 / 波动印联动 / 施法前摇 DELAY | 随基础档（022 §7） | 同 022 |
| pvp 不强化 | 无 PVP 分流 | 不做 |

## 8. 存疑与缺口上报

**未考证项**
1. static 350（dungeon）/450（pvp）语义（随基础档 022 §8 延续——TP 未提供新证据）。

**新缺口**：无。翻译工具：`.skl` 子命令（常驻）。
