# 剑影太刀精通（SwordGhost28）

> 技能ID 78 | 级别 C | 可实现性 🔶（数值面板半通、消费端卡死；主动组件目标状态无注册行） | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 剑影太刀精通 | `SwordGhost/SwordGhost28.skl [name]`（无 [name2]，英文名取文件名） | skl |
| 英文名 | SwordGhost28 | skl 文件名 |
| 职业 | 剑影（[skill fitness growtype] = 5） | skl |
| 学习等级 | 20 | skl `[required level]` |
| 最高等级 | 20（[maximum level]；[growtype maximum level] `10 0 0 0 0 10`——见 §8） | skl |
| 类型 | [passive]（skill class 1） | skl `[type]` |
| 一句话效果 | 使用太刀系武器时增加物攻和命中率 | skl `[explain]` |

**level property（2 列 × 60 级，dungeon，pvp 无表）**：
- col0 物理攻击力增加���88→568（`-1 0 1.0` 直读）；
- col1 命中率增加率：35→335（`-1 1 0.1` → 3.5%→33.5%）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（全实测）

- **`passive_skill_swordman.nut` case 78（本批三注册之一）**：
  ```squirrel
  case 78:
      local append = "character/jg_swordman/appendage/ap_blademastery.nut";
      if (skill_level > 0 && (sq_getGrowType(obj) == 0 || sq_getGrowType(obj) == 5) && obj.getWeaponSubType() == 1) {
          // 挂 appendage，写 change status：
          //   CHANGE_STATUS_TYPE_EQUIPMENT_PHYSICAL_ATTACK += sq_GetLevelData(78, 0, lv)   ← col0 物攻
          //   CHANGE_STATUS_TYPE_STUCK                 -= sq_GetLevelData(78, 1, lv)/10   ← col1 命中(卡定率减)
      } else { /* 摘除 */ }
  ```
- `ap_blademastery.nut`（48 行）：**空壳 carrier**（onStart/proc/onEnd 仅判空）——效果全部由上面
  注册侧的 change status 写入，引擎攻击/命中公式消费。
- **主动组件（意外发现）**：`jg_swordman_common.nut:70 setSwordGhost28Effect`——太刀+已学+未封印时，
  三段斩（tripleslashbs 态，`JG_SwordMan\swordghost_effect\tripleslash.nut:236` 每帧调用）中按
  `OPTION_HOTKEY_SKILL2` → 消耗 1 剑气（setSwordGhostEnergyDec）→ 进 `STATE_SWORD_GHOST_28`(128)
  （含左右方向键合流分支）。**但 load_state 全文无状态 128 注册行、白名单内无处理 nut**——
  该状态行为在本 pvf 不可考（引擎内置或 mod 残缺，见 §8）。

### 2.2 行为重建

数值侧：条件（职业 0/5 + 武器 subType==1 太刀）常驻——装备物攻 +col0、卡定率 -col1/10
（DNF 的"命中率"=降低 Stuck/卡定 miss 概率）。换武器/摘技能时摘除（注册函数双向分支）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | SwordGhost28.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\SwordGhost\SwordGhost28.skl` | ✅（126 行） | 数值表 |
| 注册 | passive_skill_swordman.nut case 78 | `…\pvf\sqr\character\swordman\passive_skill_swordman.nut:120-142` | ✅ 实测 | 挂 appendage+写数值 |
| appendage | ap_blademastery.nut | `…\pvf\sqr\character\jg_swordman\appendage\ap_blademastery.nut` | ✅（48 行空壳） | 数值载体 |
| 主动组件 | jg_swordman_common.nut:70-100 | `…\pvf\sqr\character\jg_swordman\jg_swordman_common.nut` | ✅ 实测 | 三段斩中触发逻辑 |
| 调用点 | tripleslash.nut:236 | `…\pvf\sqr\character\JG_SwordMan\swordghost_effect\tripleslash.nut` | ✅ 实测 | setSwordGhost28Effect 挂点 |
| 状态 128 处理 nut | — | load_state 全文 + 白名单 grep 实测 | ⛔ 缺失 | 行为不可考（§8） |
| 图标 | SkillIcon.img #584/585 | `…\pvf\character\swordman\effect\SkillIcon.img` | ✅ | UI |

无自有 .ani/.atk（数值被动零视觉；主动组件的动画挂在状态 128 侧，无从查起）。

## 4. 资源需求

无（缺失 img = 0）。

## 5~7. 实现/翻译/困难（合并）

- **判定 🔶（分半口径的"面板半通"样本）**：
  - 可表达半：NumericType 实测有 `Attack=1003` 及 `AttackAdd=10032`——太刀条件下物攻 +col0
    完全可挂（BuffDefinition.AddActions + AddOwnerNumeric，ForbidMoveOn/Off 同构）；
  - 卡死半：①"太刀条件"=武器类型差异化（在案 R2-A6，流心:刺首撞）——无武器 subType 读取门面；
    ②命中（STUCK 减）无对应 NumericType 亦无消费公式；③物攻数值进了面板也不改变
    MeleeHitAction 的固定 Damage（属性消费链，在案）。
  - 主动组件：目标状态 128 无注册行——不可还原；即便按 DNF 常识（太刀精通特殊攻击）补做，
    也撞"技能中按键二段交互"（在案 R4-A16）+ 剑气资源系统。
- 实现落点（若做数值半）：`DotNet~/Buffs/BladeMasteryKatanaBuff.cs : BuffDefinition`
  （AddActions 挂 AttackAdd）+ 习得时 AddBuffToSelf；武器条件暂以"无脑常驻"简化（demo 单武器）。
  被动不可施放——不占 SkillIds 号段。
- 翻译工具：`.skl` 无子命令（全局已知）；2 列 × 60 级手抄可接受。无新增缺口。

## 8. 存疑与缺口上报

- 未考证①：[growtype maximum level] `10 0 0 0 0 10` 给 gt1（剑魂）=10——剑魂可学剑影太刀精通？
  与 [skill fitness growtype]=5 矛盾，疑 mod 数据；注册代码确实放行 gt0/gt5（不含 gt1）。
- 未考证②：状态 128 无注册行——触发代码（消耗剑气+进状态）存在但目标行为全无着落，
  判 mod 残缺或引擎默认态（无动画即无表现）。
- 缺口归档（无新增）：武器类型差异化、属性消费链（武器系精通族，见 004 §8 打包建议）。
