# 体术逆改（swordman_comminterrupt）

> 技能ID 252 | 级别 D | 可实现性 ⛔ | 分析日期 2026-08-22 | 批次 D1

## 1. 基本信息
| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 体术逆改 | .skl `[name]` |
| 英文名 | swordman_comminterrupt | skl 文件名（`[name2]` 空） |
| 职业 | 全职业（[growtype maximum level] 六槽全 1） | .skl |
| 学习等级 / 最高等级 | 1（range 1）/ 1 | .skl |
| 类型 | `[passive]`，[skill class] 4，`[seal enable]` 1（可封印开关） | .skl `[type]` |
| CD / MP | 无 | .skl |
| 一句话效果 | 施放技能时，可强制中断当前技能施放其它技能 | .skl `[explain]` |

来源文件：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_comminterrupt.skl`（43 行）

## 存档说明
全鬼剑**"强制取消"（cast cancel）系统的总开关技能**——batch 委托猜测它可能是受击打断系统技，实测**不是**：explain 与脚本都指向"施法中主动取消当前技能、立即接续其它技能"（DNF 手感的"强制-后跳/强制-上挑"体系），与受击无关。实现链完整实测：

- 注册：`E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\sqr\character\swordman\passive_skill_swordman.nut` case 252 → 常驻挂 appendage `character/swordman/appendage/ap_swordman_comminterrupt.nut`（159 行）；
- appendage `proc` 每帧执行（技能未 `isSealFunction()` 时）：排除基础状态（0 待机/3/4/5/9/16/7 后跳/25/235/236）与死亡之塔后，对一张**可取消清单**逐项调 `EnableSoften(obj, 技能ID, 状态号)` + `SetSkillState(obj, 技能ID, 状态号, 参数表)`；
- 两个引擎函数（`swordman_common.nut:101/108`）：`EnableSoften` = `setSkillCommandEnable(skillId, true)`——在**当前技能状态中**重新开启目标技能的指令输入；`SetSkillState` = 若玩家按下该键且可用 → `AddSetStatePacket` 硬切状态（带预设 IntVect 参数）；
- 清单结构：共通段=鬼斩(5)/崩山击(65)/裂波斩(58)/十字斩(64)/月光斩(77)/地裂波动剑(20)/三段斩(8,限地面 `sq_GetZPos==0`)；再按 growtype 分职业段——1 剑魂(67/68/98/9/72/73/97/236/235/234，流心架势 61 中另放 67)、2 鬼泣(112/111/60 鬼影闪/95/87/237/238/239/247/240/241)、3 狂战(232/233/31/102/229/245/231，**血之狂暴(76)状态下追加** 79/103/81)、4 阿修罗(57/74)；**剑影(5) 不在 switch 内**（其取消另有 F6 的 comminterrupt 钩子表）。清单注释为乱码+mod 修复注释（修死亡之塔），此 nut 是 mod 手笔，但实现的是官方"强制中断"机制语义。

**与既有缺口的关系（委托要求专项说明）**：本技能不撞"受击-施法互斥"（R1-A4：我们永不打断施法——那是**被动被打**一侧），它撞的是**打断体系的另一半：主动取消**，即缺口累计里的"技能取消体系"（064 首报、F6 流心系"已知最大用户"）。两侧恰好构成完整打断框架：受击侧=被敌人打断/霸体免打断；施法侧=自己主动 cancel 重入新技（本技能）。且 `SetSkillState` 的"施法中消费技能键"与"技能二段交互门面"（R4-B16，PeekBufferedButton 只能消费一次）同根——取消体系立项时必须一并解决"任意技能键的施法中再消费"。若未来立项技能取消体系，本文件的清单（含按职业/血之狂暴子状态/地面限定的分支）就是现成的需求表。

## 8. 一句话结论
⛔ 不实现/远期：鬼剑全职业"强制取消"总开关，依赖技能取消体系（施法中按键再消费+状态硬切）立项；它是该缺口最直接的系统级样本，建议随取消体系一并设计。
