# 修罗邪光斩（GrandWaveCharge）

> 技能ID 51 | 级别 C（确认为**被动强化**——[type] [passive]，pre-required 邪光斩 50；非主动技） | 可实现性 🔶 | 分析日期 2026-08-22 | 批次 C3

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 修罗邪光斩 | `GrandWaveCharge.skl [name]` |
| 英文名 | GrandWaveCharge（skl 文件名；[name2] 同中文） | skl |
| 职业 | 阿修罗（[skill fitness growtype] = 4） | skl |
| 学习等级 | 20（前置：邪光斩 50 Lv1） | skl `[required level]`/`[pre required skill]` |
| 最高等级 | 20 | skl `[maximum level]` |
| 类型 | [passive]（skill class 1） | skl `[type]` |
| 指令 | Z（按住稍停后松开）——挂在邪光斩键位上的蓄力形态 | skl `[command key explain]` |
| CD | 无自有 CD（随宿主技 50，[auto cooltime apply] 0 特殊语义） | 参见 050 文档 |
| 一句话效果 | [邪光斩]升级为[修罗邪光斩]：按住蓄气后发射，增加攻击力/发射速度/射程；满蓄可击退并（若已学波动刻印）+1 波动印 | skl `[explain]` |

**level property（1 列）**：col0 = 40→135（dungeon；`-1 0 1.0` 直读）= 满蓄时增加魔攻 %。
**static data（dungeon）**：`10 200 150 300` = 蓄气上限（static[0]，秒级换算未考证，推断 1.0s）、
满蓄尺寸 200%（static[1]）、射程加成 150%（static[2]）、满蓄多段间隔 300ms（static[3]）——
后三值已在 050-GrandWave.md §2.4 由 PO 写包实证（case 12 修罗邪光斩满蓄波）。

## 2. 技能逻辑走读

### 2.1 注册与文件链

本技能无自有注册/nut（实测：load_state 与 passive_skill_swordman.nut 均无 51/grandwavecharge，
仅 `swordman_header.nut` 无关）。**全部逻辑挂在宿主技 50 邪光斩侧**（引擎内置 F3b 形态：
`attack\grandwave.nut` 门禁壳 + 共享 PO 24349 承担判定）——**完整走读见
`050-GrandWave.md` §2.3-2.4（case 11 未蓄力 / case 12 满蓄波）**，本文不重复。

关键结论摘录（050 实证）：
- 蓄力过程循环播 `grandwaveoncharge1/2.ani`（14 帧循环 910ms，OnCharge.img）；
  满蓄爆发播 `grandwavefullcharge1/2.ani`（6 帧 200ms，FullCharge.img）；
- 满蓄波 case 12：atk `grandwavefullcharge.atk`（对象表 7）、尺寸=51 static[1] 200% 三重同步缩放、
  多段间隔=static[3] 300ms、射程=col0×static[2]；
- [auto cooltime apply] 0：CD 从发射（松开）起算。

## 3. 关联文件清单（每行实测，宿主侧资源从略——见 050 文档 §3）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | GrandWaveCharge.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\GrandWaveCharge.skl` | ✅（126 行） | 蓄力参数 |
| 注册行/主 nut/appendage | —（挂宿主 50） | `…\pvf\sqr\character\`（grep 实测无独立文件） | ⛔ 无独立脚本 | 见 050 |
| 蓄力特效 ani | grandwaveoncharge1/2.ani、grandwavefullcharge1/2.ani | `…\pvf\character\swordman\effect\animation\` | ✅（050 实测） | 蓄力/满蓄视觉 |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| GrandWaveOnCharge.img / GrandWaveFullCharge.img | sprite_character_swordman_effect.NPK | 蓄力循环/满蓄爆发特效 | 可选（做蓄力才需要） | ❌（050 §4 同结论） |

其余资源（挥剑/波判定/atk）已在 050 文档记账，缺失 img 增量 = **2**（均可选）。

## 5~7. 实现/翻译/困难（🔶 合并）

- **判定 🔶（简化绕过）**：
  - 主干（满蓄强化波：更大尺寸/射程/伤害/击退 + 300ms 多段）用现有五件套完全可表达——
    Area/Bullet 缩放参数 + HitReaction（KnockbackX 击退）+ 同心双 Area Tick 多段（L19）；
  - **按住蓄气输入缺失**（在案共性，R3-A15 四技同撞）：demo 简化为**瞬发强化版**——
    习得 51 后邪光斩直接发射 case 12 参数（对齐 050 §5"蓄力整体跳过"建议的增强变体），
    或完全跳过 51（050 原建议）。手感差异：失去"蓄力博弈"（蓄力时被打断的风险收益），
    数值上可取 Lv10 档 col0≈85% 折进 Damage。
  - 满蓄"+1 波动印"：技能资源标记系统（在案，波动刻印 047）——跳过。
- 实现落点：**改 050 草案的 GrandWaveSkill**（SubState/参数分支），不新增 SkillId
  （被动不可施放；SkillIds 34 起号段本技能未占用）。
- 翻译工具：`.skl` 无子命令（全局已知）；蓄力特效 ani 全常规节（050 实测）。无新增缺口。

## 8. 存疑与缺口上报

- 未考证：static[0]=10 蓄气上限的精确换算（×0.1s=1.0s 推断，050 同存疑；不影响"瞬发简化"路径）。
- 缺口归档：按住蓄力输入（在案 R3-A15 共性）；技能资源标记/波动印（在案 047）。
