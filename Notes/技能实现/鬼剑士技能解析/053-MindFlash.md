# 挫折意志（MindFlash）

> 技能ID 53 | 级别 C | 可实现性 ⛔（触发端无注入点；受击钩子落地后升 🔶） | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 挫折意志 | `MindFlash.skl [name]` |
| 英文名 | MindFlash（skl 文件名；[name2] 同名） | skl |
| 职业 | 阿修罗（[skill fitness growtype] = 4） | skl |
| 学习等级 | 20 | skl `[required level]` |
| 最高等级 | 30（[growtype maximum level] gt4=20） | skl |
| 类型 | [passive]（skill class 1） | skl `[type]` |
| 一句话效果 | 受击后暂时+智力（可叠层）；满层再受击 → 转为+MP恢复量 | skl `[explain]` |

**level property（4 列，L21 解码，量级自洽）**：
- col0 智力：5→170（`-1 0 1.0`，每级约 +3）；
- col1 智力持续：恒 20000ms（`-1 1 0.001`）；
- col2 每秒MP恢复：12→425（`-1 2 0.05` → 显示 0.6→21.25/s）；
- col3 MP恢复持续：恒 20000ms；
- 最大重叠：static[0]=5（dungeon）/ 3（pvp）（第 5 向量 `0 0 1.0` = static 槽 0）。

**[skill preloading image]**：`Character/Swordman/Effect/MindFlashHead.img`（受击触发时的头顶视觉）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- `swordman_load_state.nut` 无注册；`passive_skill_swordman.nut` 无 case 53；
- `sqr\character\swordman\` 全树 grep `mindflash`：无命中（受击触发逻辑在引擎受击管线内，推断：
  引擎按"被击事件 → 查技能 53 → 挂智力 appendage/叠层"处理）。

### 2.2 行为重建（引擎惯例 + skl 数据）

- 被击 → 挂"智力+col0"状态 20s；持续期内再被击 → 层数+1 并刷新（上限 5）；
- 已满层再被击 → 改挂"MP恢复+col2/秒"状态 20s（智力不再叠）；
- 头顶播 mindflashhead 视觉 + 层数文字 word0-3（特效层）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | MindFlash.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\MindFlash.skl` | ✅（209 行） | 全部数据 |
| 注册行/主 nut/appendage | — | `…\pvf\sqr\character\`（grep 实测） | ⛔ 缺失（引擎内置） | 受击触发在引擎 |
| 特效 ani | mindflash/word0~3.ani | `…\pvf\character\swordman\effect\animation\mindflash\` | ✅（4 个，实测） | 层数文字视觉 |
| 特效 ani | mindflashhead.ani | `…\pvf\character\swordman\effect\animation\mindflashhead.ani` | ✅（4 帧） | 头顶触发视觉 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| MindFlashWord.img | sprite_character_swordman_effect.NPK | word0~3 层数文字 | 可选 | ❌ |
| MindFlashHead.img | sprite_character_swordman_effect.NPK | 头顶视觉 | 可选 | ❌ |

（两 ani 均 [SHADOW] 节（已知翻译记档项）+ 常规 FRAME 节；角色本体动画无新增。）

## 5~7. 实现/翻译/困难（合并）

- **判定 ⛔ → 受击管线钩子落地后 🔶**：
  | 环节 | 缺口（在案） | 档位 |
  |---|---|---|
  | "被击时触发" | 受击伤害管线钩子（R3-A15 首报，消费方已累计 6：B2 凯贾/自动格挡 + B3 逆转反击等） | 缺失（在案） |
  | 智力数值 | NumericType 无 Intelligence 键（176 草案曾提议 1007 未落地，本次实测确认仍在）+ 属性消费链卡死 | 面板半通 |
  | 叠层 Buff | BuffDefinition 无叠层计数字段（Buff 同 id 多实例/叠层上限——在案 R5-B5 冥炎 3 层同源） | 缺失（在案） |
  | MP恢复 | MP 系统延后 | 延后 |
  | 转换逻辑（满层→MP） | Buff 查询门面（在案）查询当前层数 | 缺失（在案） |
- 简化建议：demo 不实现；受击钩子专题落地后，按"StunBuff 同构 + AddActions 挂 AttackAdd"做
  面板演示（伤害端仍卡死）。
- 翻译工具：`.skl` 无子命令（全局已知）；word/head ani 常规节 + [SHADOW]（已知记档）。无新增缺口。

## 8. 存疑与缺口上报

- 未考证：col2 的 ×0.05 显示换算（MP恢复/秒的真实单位）；word0~3 与 5 层的对应关系（4 文件 vs 5 层）。
- 缺口归档（全部在案，无新增）：受击管线钩子第 7 消费方；Buff 叠层上限；Buff 查询门面；
  属性消费链；MP 系统。
