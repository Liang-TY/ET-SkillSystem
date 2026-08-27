# 武器节制（DurabilityUp）

> 技能ID 39 | 级别 C | 可实现性 ⛔（效果无消费端，建议不实现） | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 武器节制 | `DurabilityUp.skl [name]` |
| 英文名 | DurabilityUp（skl 文件名；[name2]=`The Temperate Attack`） | skl |
| 职业 | 剑魂（[skill fitness growtype] = 1） | skl |
| 学习等级 | 20 | skl `[required level]` |
| 最高等级 | 20（[growtype maximum level] gt1=10） | skl |
| 类型 | [passive]（skill class 1） | skl `[type]` |
| 一句话效果 | 减缓武器耐久度下降速度 4%→80%，[破极兵刃]施放中亦有效 | skl `[explain]` |

**level property**：单列 col0 = 4→80（`-1 0 1.0`，每级 +4%，直读——全批最简单的数值表）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- `swordman_load_state.nut`：无注册行；`passive_skill_swordman.nut`：无 case 39；
- `sqr\character\swordman\` 全树 grep `durability`：无命中（破极兵刃 OverDrive 的 nut 亦不读 39，
  "破极兵刃中亦有效"由引擎耐久管线统一处理，推断）。

——纯引擎内置被动：武器耐久损耗结算时按比率折减。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | DurabilityUp.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\DurabilityUp.skl` | ✅（73 行） | 全部数据 |
| 注册行/主 nut/appendage | — | `…\pvf\sqr\character\`（grep 实测） | ⛔ 缺失（引擎内置） | 行为在引擎 |
| 图标 | SkillIcon.img #72/73 | `…\pvf\character\swordman\effect\SkillIcon.img` | ✅ | UI |

无 .ani/.atk/.als/.obj。

## 4. 资源需求

无（缺失 img = 0）。

## 5~7. 实现/翻译/困难（合并）

- **判定 ⛔（无消费端）**：我们无武器耐久系统，也无任何损耗概念——效果空转到"连面板都无处显示"
  的程度。与 55 杀气感知的耐久子效合同一归档：**建议按"不实现"处理**（近 D 类"无战斗意义"），
  不建议为它立项耐久系统。
- 翻译工具：`.skl` 无子命令（全局已知）；单列 20 值手抄即可。无新增缺口。

## 8. 存疑与缺口上报

无未考证项。缺口归档：耐久度系统（39/55 双实证）→ 建议标记"不实现"，非缺口立项。
