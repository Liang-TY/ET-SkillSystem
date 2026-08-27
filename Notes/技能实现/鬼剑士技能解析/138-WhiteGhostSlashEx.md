# 强化 - 魂破斩（WhiteGhostSlashEx）

> 技能ID 138 | 级别 E | 可实现性 🔶（增量本身 ✅ 级——攻击力乘区 + 范围 +10% 一处 HalfExtents；整体随基础技 071 的 🔶 前提） | 分析日期 2026-08-22 | 批次 E4

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 强化 - 魂破斩 | `skill\Swordman\WhiteGhostSlashEx.skl` [name] |
| 英文名 | WhiteGhostSlashEx（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 剑影（[skill fitness growtype] 全列；[growtype maximum level] `5 0 0 0 0 5` → TP 上限 5） | 同上 |
| 学习等级 | 55（[required level range] 5）；**前置 126（spiritmove 鬼步）Lv1——非基础技 71，数据异常见 §8** | 同上 [pre required skill] |
| 最高等级 | [maximum level] 7（实际可学 5） | 同上 |
| 类型 | passive · 特性技（[feature skill type] 1）· skill class 5（新式 TP）/ 物理 | 同上 |
| TP 消耗 | 2 点/级 | 同上 |
| 一句话增量 | 魂破斩攻击力 +10%/Lv；攻击范围 +10%（Lv1 固定、与等级无关） | 同上 [explain ex] |

## 2. 强化增量（TP 表）

### 2.1 副本 + 新式结构（byte 级实证）

python diff 实证：[static data]（5 值 `100 0 130 190 0`）与 [level info]（71 行 × 1 列，col0 6213→28800+，步进 +630）与 `ghostsword\whiteghostslash.skl` 逐字节相同。[special level up] 空节——增量引擎内部（同 083 分型）：

- **攻击力 +10%/Lv** → ×(1+0.1N) 乘区；
- **范围 +10%（Lv1 固定）** → explain ex 明言"攻击范围增加效果为固定值"——static[0] 副本仍显示 100%（剑气大小），+10% 由引擎内部施加（副本无此数据，记未考证细节）。

level property（6 占位符模板对位，与基础技 071 全同的副本显示）：
- 下劈攻击力 ← (-1,0) col0；剑气大小 ← (0,0) static[0]=100；
- 追加下劈开关 ← static[1]=**0（关）**；追加剑气大小/速度 ← static[2]=130/static[3]=190（备用）；
- 鬼步/剑术互断开关 ← static[4]=**0（关）**——**TP 不开追加下劈与互断**（与基础技本 pvf 配置一致）。

### 2.2 引擎消费（全内置）

load_state 无注册；白名单 grep `whiteghostslashex` 0 命中；无 PO/ani/atk/appendage。基础技 PO 24349 dword 47/48/49 取数 = 父角色 col0（071 文档 §2.3）——TP 加成引擎层并入（sq_GetBonusRateWithPassive 族）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | WhiteGhostSlashEx.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\WhiteGhostSlashEx.skl` | ✅（278 行） | 副本 + 引擎增量 |
| 基础技文档 | 071-whiteghostslash.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 增量挂接点 |
| 基础技 .skl | ghostsword\whiteghostslash.skl | 同 skl 树 | ✅ | diff 对拍相同 |
| 注册行 / nut / PO / .ani / .atk | — | — | ⛔ 无新增（复用 071 全链：PO 24349 d47/48/49） | 纯数据被动 |

## 4. 资源需求

无自有资源（图标位 620/621）。**缺失 img：0 张。**

## 5. 实现方案草案（增量，随 071 一并落地）

- **零新增内容件**。071 草案的 `WhiteGhostSlashArea` 接 `TpLevel`（0-5 常量）：
  - `Damage = 180 × (1 + 0.10 × TpLevel)`（col0 乘区；鬼步接续 d48/d49 变体后补时同乘）；
  - `HalfExtents = (2.05, 0.6, 1.4) × 1.10`（TP≥1 时范围 +10% 固定）+ `CreateAreaInFront` 距离同乘 1.10（盒中心前偏同步放大）；
  - 命中反应（down/push100/lift108）、CD 12000、static[1]/[4]=0 的两开关行为不变。
- **概念映射**：引擎 ×(1+0.1N) → TpLevel 乘区；范围 +10% → HalfExtents/偏移常量 ×1.1（对象整体缩放延后档的常量版替代——固定倍率无需运行时缩放系统）；TP 学习系统缺失（R6-C1）→ 常量。
- **关键数值表**：TP0-5 攻击力 ×1.0 → ×1.5；TP≥1 范围恒 ×1.10（盒 4.1×1.2×2.8 单位 → 4.51×1.32×3.08）。

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| WhiteGhostSlashEx.skl | `.skl` 无子命令（新式 TP，同 083 分型） | 同 083 处理 |

本技能翻译缺口 1 类（.skl）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| TP 学习（2 点/级） | 技能等级系统缺失（R6-C1） | 常量 TpLevel |
| 范围 +10%（Lv1 固定） | 对象整体缩放（延后档）——但**固定倍率可用常量预乘**绕开 | HalfExtents ×1.10 常量版 |
| 鬼步接续链的 TP 结算（sub2/3/4） | 技能取消体系缺失（071 §7 已砍） | 常规路径乘区即可 |

## 8. 存疑与缺口上报

- **数据异常（mod 疑点）**：[pre required skill] = **126（spiritmove 鬼步）** 而非基础技 71——与 106（pre=137）同型异常，学习链数据疑被 mod 改动；实现侧按"前置 71"理解（无功能影响）。
- **未考证**：范围 +10% 的引擎施加点（static 副本无数据、仅 explain 文案——疑引擎按 TP≥1 硬编码 ×1.1）；新式 TP ×(1+0.1N) 公式为推断（同 083）。
