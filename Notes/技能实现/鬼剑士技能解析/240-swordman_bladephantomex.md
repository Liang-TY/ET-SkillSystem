# 幽魂降临 : 式（swordman_bladephantomex）

> 技能ID 240 | 级别 E（**预分类纠偏：非 TP——[type] active、skill class 2，幽魂之布雷德（239）的升级型主动技**（pre required 239 Lv1，鬼泣 80 级段）） | 可实现性 🔶（"起势→方向选择跳跃→空中挥斩→裂地两段 PO"主干可三段 PlayAnim + 两相位 Area 直译；跳跃（瞬移 170z/空中施放/落地）与技能中方向输入双撞系统缺口→地面原地降级；col0 判定盒缩放可满级预折算） | 分析日期 2026-08-22 | 批次 E7

## 1. 基本信息

| 项 | 值 | 来源 |
|---|---|---|
| 中文名 | 幽魂降临 : 式 | `skill\Swordman\swordman_bladephantomex.skl` [name] |
| 英文名 | swordman_bladephantomex（取 skl 文件名；无 [name2]） | 同上 |
| 职业 | 鬼泣（[skill fitness second growtype] **单值 2**；[second growtype maximum level] 12 槽**第 5 位单填 30**（第 4 位 0——鬼泣对的"只填后位"，疑三觉树槽位语义，未考证）；[skill fitness growtype] 空） | 同上 |
| 学习等级 | 80（[required level range] 2）；前置 239 幽魂之布雷德 Lv1 | 同上 |
| 最高等级 | 40（二觉档实际 30） | 同上 |
| 类型 | active（skill class 2） | 同上 |
| 指令 | ↓→→ + Z（MP 优惠 50%/50%） | 同上 |
| 可施放状态 | `8 0 6 14`（普攻/站立/**跳跃（6）——空中可施放**/14 未考证） | 同上 [executable states] |
| CD | 45000 ms | 同上 [cool time] |
| MP | 800 → 6000 | 同上 [consume MP] |
| 特殊消耗 | 无色小晶块 ×**5**（本批最重） | 同上 [consume item] |
| 装备损耗 | [durability decrease rate] 315（高损耗） | 同上 |
| static data | `170 100 170`——三向量直取：[0]=170 跳跃高度 z、[1]=100 前后跳距离 x、[2]=170 空中施放最高高度 z（**checkExecutableSkill 的 zPos 门禁实测消费**——static 全列有脚本消费的罕见实证） | 同上 + nut 实测 |
| 一句话效果 | 跳到空中使幽魂布雷德降临己身挥出剑气，剑气劈裂地面后爆炸；施放时按前/后方向键决定跳向，可在空中施放 | 同上 [explain] |

**level property 模板解码（3 列 + 6 向量，L21 法全解，Lv1→Lv70 dungeon）**：

| 显示行 | 向量 | 取值 |
|---|---|---|
| 跳跃高度 | (0,0,1.0) → static[0] | 170 z 恒定 |
| 前后方跳跃距离 | (1,1,1.0) → static[1] | 100 x 恒定 |
| 空中施放时最高高度 | (2,2,1.0) → static[2] | 170 z 恒定 |
| 斩击大小比例 | (-1,0,1.0) | col0 = **100% → 169%**（随等级长大的图像/判定盒缩放） |
| 斩击攻击力 | (-1,1,1.0) | col1 = 14010% → 112099% |
| 最后一击攻击力 | (-1,2,1.0) | col2 = 17540% → 140317% |

pvp 表同构（col0 恒 100%、col1=105→852、col2=158→1263）。

**与基础 239 幽魂之布雷德对照**：239 = 地面召唤幽魂残影持续区 + 终结二连；240 = **本体跳跃演出 + 一击裂地两段 PO**（无持续区）——是"升级版主动技"而非强化被动，机制几乎不重叠（唯一共享：暗属性叙事与 24370 载体）。

## 2. 技能逻辑走读

### 2.1 注册与文件链（**完整 nut 主动技**——本批 8 技中唯一注册型）

```
62: IRDSQRCharacter.pushState(ENUM_CHARACTERJOB_SWORDMAN, "character/swordman/bladephantomex/bladephantomex.nut", "swordman_bladephantomex", 240, 240);
（PO 载体：共享 PO 24370 六回调——行 8-13，F7/L20 既有链路）
// swordman_header.nut：STATE/SKILL_SWORDMAN_BLADEPHANTOMEX <- 240（72/100 行）、
//   CUSTOM_ANI_SWORDMAN_BLADEPHANTOMEX_CAST/JUMP/SLASH_BODY <- 156/157/158（326-328 行）
// .chr 对位：etc motion #156/157/158 = BladePhantomEx_Cast/Jump/Slash_body.ani（1129-1131 行，0 基吻合）
```

### 2.2 主 nut 逐回调（bladephantomex.nut，227 行全读；变量名混淆但语义完整，C3 形态①）

- **checkExecutableSkill**：`sq_GetIntData(obj,240,2)`=170 作 zPos 门禁（**当前 z>170 拒绝施放**）；`sq_IsUseSkill(240)` 后：**STATE_JUMP 且 z>0 → 子状态 1（空中施放，跳过起跳段）**，否则子状态 0（地面完整流程）。
- **onSetState**（子状态 0/1/2）：
  - **0 起势**：播音 SM_BLADE_PHANTOM_01 + 播 #156（Cast_body 160ms）+ 底层特效 `bladephantomex_cast_floor.ani`（draw-only）。
  - **1 跳跃**：播 #157（Jump_body 460ms）+ **接管黑屏 flash**（999990ms 长黑——演出用）+ 若 vector[1]≠-1（地面版）：落地尘 `jump_dust`、`sq_findNearLinearMovableXPos` 求目标 x（输入距离 static[1]）、按 `atan2(|dx|, static[0])` 旋转角生成 `jump_pass01+02` 池化残影、然后 **`sq_setCurrentAxisPos(0, 目标x)` + `sq_setCurrentAxisPos(2, static[0]=170)`——角色瞬移至跳跃落点/顶高**（跳跃=瞬移+镜头+旋转残影的演出组合，非真实抛物线）；空中版（vector[1]==-1）不瞬移。
  - **2 挥斩**：播 #158（Slash_body 260ms）+ 音 SM_BLADE_PHANTOM_02。
- **onKeyFrameFlag**（子状态 2，Slash_body **F2=140ms flag 1**）：写包四 dword——`240 / col0（大小率）/ sq_GetBonusRateWithPassive(240,240,1,1.0)（col1）/ sq_GetBonusRateWithPassive(240,240,2,1.0)（col2）`→ `sq_SendCreatePassiveObjectPacketPos(24370, 0, x, y, 0)` 创建裂地 PO；同帧建 `bladephantomex_crack_floor_main.ani` 池化视觉（**按 col0/100 三重缩放**：setImageRate + setAutoLayerWorkAnimationAddSizeRate）。（**sq_GetBonusRateWithPassive 出现**——R7-E1 引擎折算的脚本侧形态，TP 体系的调用惯例。）
- **onEndCurrentAni**：0→1（**读取 OPTION_HOTKEY_MOVE_LEFT/RIGHT**：同向=+static[1] 前跳 / 逆向=-static[1] 后跳 / 无=原地，写入 vector[1]）；1→2；2→**STATE_JUMP**（[1,0,0] 落下收尾）。
- **getScrollBasisPos**：子状态 1 镜头 200ms 从旧 x lerp 到 施法者+300；子状态 2 固定 +300（镜头演出核心）。
- **onEndState**：离开 240 清 flash 对象。

### 2.3 共享 PO 24370 case 240（裂地判定与演出主体）

- **setcustomdata.nut:305**：接管角色黑屏 flash + 白闪 + 震 8/150；读三 dword（大小率/斩击/最后一击）；**动画 etc motion #36** `BladePhantomEx/BladePhantomEx_Crack_Explosion02.ani`；**攻击信息 etc atk #21** `BladePhantomEx_Crack_Hold.atk`；**图像 + autoLayer + `sq_SetAttackBoundingBoxSizeRate` 三重按 col0/100 缩放（攻击盒缩放 API 实证）**；atk#21 bonus=col1、atk#22 bonus=col2。
- **else.nut:233（onKeyFrameFlag）**：裂地动画 **F10 flag 1 → `sq_SetCurrentAttackInfoFromCustomIndex(obj, 22)` 切换到最后一击 atk** + RemoveAllFlash + 黑白双闪 + 震 9/200——**�� PO 内分段 HitReaction 切换**（R4-B17 缺口族的第 5 消费方，PO 侧原生形态）。
- **onendcurrentani.nut:76**：播完销毁。
- 24370 mod obj 对位（0 基直读，F7 第四/五次复验区段）：etc motion **#36** = `BladePhantomEx_Crack_Explosion02.ani`；etc atk **#21** = `BladePhantomEx_Crack_Hold.atk`、**#22** = `BladePhantomEx_Crack_Exp.atk`。

**两个 .atk 实测**：

| atk | 关键值 | → 我们 HitReaction |
|---|---|---|
| #21 Crack_Hold（裂地主段） | magic / dark / damage 反应 / hit down / blow / no blood 25 1.0 / knuck back -1 / **[force hit stun time] 1000** | Damage=col1；HitstunMs=**1000（强制僵直直译）**；Kb 0 / Ly 0 |
| #22 Crack_Exp（最后一击） | magic / dark / **down** / cut / push 0 / **lift 150** / **[ignore weight] 1** | Damage=col2；击倒=长硬直+浮空 150 |

### 2.4 动画关键帧表

| 动画 | 帧数 | 总时长 | SET FLAG | 攻击盒 | 引用 img |
|---|---|---|---|---|---|
| BladePhantomEx_Cast_body.ani（#156 起势） | 2 | 160ms | 无 | 无 | sm_body（L16） |
| BladePhantomEx_Jump_body.ani（#157 跳跃，+.als） | 7 | 460ms | 无 | 无 | sm_body |
| BladePhantomEx_Slash_body.ani（#158 挥斩，+.als） | 8 | 260ms | **F2=1（140ms，创建裂地 PO）** | 无 | sm_body |
| PO etc#36 BladePhantomEx_Crack_Explosion02.ani（裂地） | 24 | 1830ms | **F10=1（@740ms，切最后一击 atk）** | **F0-F13**（盒见下） | `Effect/BladePhantomEx/ExplosionA.img`（+.als） |
| bladephantomex_cast_floor / jump_dust / jump_pass01+02 / phantom05（nut 直建 draw-only） | — | — | — | — | effect\bladephantomex\ 系 |
| crack_floor_main.ani（nut 池化地面裂纹） | — | — | — | — | Floor_CrackA/B.img |
| PO 目录其余（crack_back/front_explosion、fire、floor_dust/eff、front_light、stonec 等 ~30 文件 + .als ×3） | — | — | — | — | §4 |

**裂地攻击盒**（offset x,y,z + size w,h,d，px）：F0 `9 -55 -5 318 110 466`（前伸高柱 x[0.09,3.27] z[-0.05,4.61]——劈落竖斩）；F5 `14 -55 -29 966 110 183`（**前向 9.66 单位长条地面盒**）；F9-F13 ≈748-967 宽（地面持续段）。全盒随 col0 缩放（§2.3）。

## 3. 关联文件清单（每行实测）

| 层 | 文件 | 完整路径 | 状态 | 作用 |
|---|---|---|---|---|
| .skl | swordman_bladephantomex.skl | `E:\Projects\cs\dnforigin\pvf源码提取部分\pvf\skill\Swordman\swordman_bladephantomex.skl` | ✅（260 行） | 3 列全解 |
| 注册行 | swordman_load_state.nut 行 62（+行 8-13 共享 PO） | `…\sqr\character\swordman_load_state.nut` | ✅ 实测 | 状态 240 + 24370 |
| 主 nut | bladephantomex.nut | `…\sqr\character\swordman\bladephantomex\bladephantomex.nut` | ✅（227 行全读，混淆可读） | §2.2 |
| PO 回调 | share_obj/swordman/ 三处 case 240 | `…\sqr\common_object\share_obj\swordman\{setcustomdata:305, else:233, onendcurrentani:76}.nut` | ✅ 实测 | §2.3 |
| PO 定义（mod） | qq506807329new_swordman_24370.obj | `…\passiveobject\script_sqr_nut_qq506807329\swordman\` | ✅ 实测 | etc #36 / atk #21/22 对位 |
| PO atk ×2 | BladePhantomEx_Crack_Hold.atk / _Exp.atk | `…\script_sqr_nut_qq506807329\swordman\attackinfo\` | ✅ 实测 | §2.3 |
| .chr 条目 | etc motion #156/157/158（行 1129-1131） | `…\character\swordman\swordman.chr` | ✅ 实测 | 三段角色动画 |
| 常量 | swordman_header.nut 72/100/326-328 | `…\sqr\character\swordman\swordman_header.nut` | ✅ 实测 | 状态/技能/动画常量 |
| 角色 .ani | BladePhantomEx_Cast/Jump/Slash_body.ani（+.als ×2） | `…\character\swordman\animation\` | ✅ 实测 | §2.4 |
| 角色 .atk | —（无专属） | `…\character\swordman\attackinfo\` | —（判定全在 PO） | — |
| PO .ani | script_sqr_nut…\animation\bladephantomex\ ~30 文件（.als ×3）+ 官方镜像 `…\passiveobject\character\swordman\animation\bladephantomex\` | 两目录 | ✅ 实测 | 裂地全套演出 |
| 特效 .ani | effect\animation\bladephantomex\（cast_floor/jump_dust/jump_pass/phantom05 等） | `…\character\swordman\effect\animation\bladephantomex\` | ✅ 实测 | nut 直建演出层 |
| 基础技文档 | 239-swordman_bladephantom.md | `Notes\技能实现\鬼剑士技能解析\` | ✅ | 升级链对照 |
| 装备层 | 未查 | `…\equipment\...` | 未查 | sm_body 单图集（L16） |

## 4. 资源需求

| img | 推导 NPK | 用途 | 必要性 | 已入库? |
|---|---|---|---|---|
| `…/Effect/BladePhantomEx/ExplosionA.img` | sprite_character_swordman_effect_bladephantomex.NPK | 裂地主层（etc#36） | **必需** | ❌ |
| `…/BladePhantomEx/Floor_CrackA.img`、`Floor_CrackB.img` | 同上 | 地面裂纹（crack_floor_main） | **必需** | ❌ |
| `…/BladePhantomEx/CastA01.img` | 同上 | 起势底光（cast_floor） | **必需** | ❌ |
| `…/BladePhantomEx/SlashA.img` | 同上 | 挥斩刀光（.als 主层） | **必需** | ❌ |
| `…/BladePhantomEx/Black.img` | 同上 | 黑屏演出层 | 可选（demo 不做黑屏可省） | ❌ |
| `…/BladePhantomEx/{CastA,CastB02,CastC01-04,SlashB-D,StoneA-C,ExplosionB,Floor_Fire,Floor_Light,Floor_ShockA/B}.img` | 同上 | 其余演出层（起势/斩击/碎石/地光） | 可选（~16 张） | ❌ |
| sm_body0000.img | （已入库） | 三段角色动画 | 必需（共享） | ✅ |

**缺失 img：必需 5 张、可选 ~18 张——全部同一个 NPK 一次提取；无跨目录借图。**

## 5. 实现方案草案（号段：SkillIds 39 / AnimIds 200-202 / AreaIds 49-50，E7 批内顺延）

### 内容件清单（地面降级版：跳跃段砍，见 §7）

1. **`DotNet~/Skills/BladePhantomExSkill.cs : SkillLogic`**（三段 PlayAnim + SubState，239 BladePhantomSkill 同范式）：
   - `CooldownMs=45000`；`TotalTimeMs=1200`（起势 160 + 挥斩 260 + 收势余量；裂地区独立 1830ms）。
   - `OnCast`：`ctx.PlayAnim(AnimId.BladePhantomExCast)`；SubState=0。
   - `OnUpdate`：`CurrentFrameIndex()` 达起势尾且 SubState==0 → `ctx.PlayAnim(AnimId.BladePhantomExSlash)`（**跳跃段跳过**，地面版直接挥斩）+ SubState=1；挥斩 140ms（F2 对位）且 SubState==1 → `ctx.CreateAreaInFront(AreaIds.BladePhantomExCrack, (FP)15/10)`（裂地区出生前 1.5 单位）+ SubState=2。
   - `OnEnd`：`ctx.PlayDefaultAnim()`。（方向键前后跳/空中施放/落地不做——§7。）
2. **`DotNet~/Areas/BladePhantomExCrackArea.cs : AreaDefinition`**（裂地两段——**单 PO 分段 atk 切换拆为双区顺序创建**，L9 多相位惯例）：
   - 主段（Hold）：`TotalTimeMs=740`（F0-F9）、`TickTimeMs=185`（10 帧 70ms 窗口内 4 跳近似）、`TickActions={MeleeHit}`、`HalfExtents=(5.0,0.6,1.0)`（F5 长条盒 966px 折算；**col0 满级预折算 ×1.69 → 8.5 单位**——E2 判定盒缩放同款绕过）、`HitReaction{Damage=260, HitstunMs=1000, KnockbackX=0, LaunchY=0}`（atk#21 force stun 1000 直译；Damage=col1 14010% 折算）；
   - 末段（Exp）：`TotalTimeMs=1090`（F10-F23）、`EnterActions={MeleeHit}`、同盒、`HitReaction{Damage=330, HitstunMs=800, KnockbackX=0, LaunchY=150}`（atk#22 down/lift150；Damage=col2 17540%）——技能 OnUpdate 于 740ms 时第二区落点。
   - `ViewAnimId=AnimId.BladePhantomExCrack`（1830ms 全长视图一次挂主段区，末段区无视图——避免双份）。

### 概念映射（对齐 01§3）

| DNF 概念 | 我们的组件 |
|---|---|
| 状态 240 三子状态（起势/跳跃/挥斩） | 三段 PlayAnim（跳跃段砍——地面版两段） |
| **跳跃 = 瞬移 + 镜头 lerp + 旋转残影**（sq_setCurrentAxisPos 实证） | 不做（跳跃系统 R1-A2 + 技能中方向输入 R1-A3 双撞）——地面直挥；跳跃落地后若做：MoveCasterForward 100px + 演出 ani 平移近似 |
| 方向键前/后跳选择（onEndCurrentAni 读键） | 技能中方向输入读取缺口（R1-A3） | 不做 |
| 空中施放（STATE_JUMP z>0 → 免起跳） | 跳跃系统缺失 | 不做（demo 地面 only） |
| static[2]=170 空中高度门禁 | 同上 | 不做 |
| 裂地 PO 24370（两段 atk 切换 + col0 盒缩放） | 双 Area 顺序创建 + 满级预折算（R4-B17 分段 HitReaction 的 Area 化绕过） |
| force hit stun 1000（强制僵直） | HitReaction.HitstunMs=1000 直译 |
| 黑白闪/震 8-9/镜头滚动 | 闪屏/屏震延后 | 跳过 |
| 暗属性 | 元素系统缺失 | 无属性直伤 |
| 无色×5 / 装备损耗 315 | 延后 | 跳过 |

### 注册点清单

| 什么 | 在哪 | 增量 |
|---|---|---|
| SkillId | `Runtime\SkillIdAttribute.cs` | `SkillIds.BladePhantomEx = 39` + 新键 |
| AreaId | `Runtime\AreaDefinition.cs` | `BladePhantomExCrack = 49`、`BladePhantomExCrackEnd = 50` |
| AnimId | `AnimConfigRegistry.cs` | `BladePhantomExCast = 200`、`BladePhantomExSlash = 201`、`BladePhantomExCrack = 202` |
| json/图集 | LSAnimClipRegistrar / BuildAtlas | json ×3；bladephantomex 图集 1 个（必需 5 张） |

### 关键数值表

| 项 | DNF 原值 | demo 建议值 |
|---|---|---|
| CD | 45000 ms | 45000 直用 |
| 全程 | 起势 160 + 跳 460 + 挥斩 260 + 裂地 1830 | 地面版 160+260+裂地区 1830 |
| 裂地主段 | col1 14010%→112099%，force stun 1000 | Damage 260 / Hitstun 1000 |
| 裂地末段 | col2 17540%→140317%，down+lift150 | Damage 330 / Hitstun 800 / Ly 150 |
| 裂地盒 | F5 前向 966px 长条 × col0（100%~169%） | HalfExtents 5.0（满级预折算 8.5） |
| 跳跃 | 瞬移 x±100px、z=170、方向键选择 | 不做（地面版） |
| 分段切换点 | 裂地 F10 @740ms | 第二区 740ms 落点 |

## 6. 翻译工具适配

| 文件 | 不支持的节/字段 | 建议 |
|---|---|---|
| swordman_bladephantomex.skl | `.skl` 无子命令 | 手抄（3 列全解） |
| qq506807329new_swordman_24370.obj | `.obj` 无子命令 | 本档已给 #36/#21/#22 对位表（F7 通用查询表既有） |
| BladePhantomEx_Crack_Hold.atk / _Exp.atk | `.atk` 无子命令；**[force hit stun time]**（239 已记）与 **[ignore weight]**（R2-A8 记档字段族再证） | 手抄；atk 子命令字段表两字段已列输入 |
| Crack_Explosion02.ani | 攻击盒 14 帧 + flag（常规节） | ani 子命令全覆盖 |
| .als ×3（crack_explosion02/crack_floor_main/crack_front_explosion03）+ 角色 .als ×2 | [use animation]/[add] | ✅ 全覆盖（层多但常规） |
| bladephantomex.nut（混淆） | 非翻译问题（C6 形态①） | 走读按语义恢复（本文 §2.2 即是） |

本技能翻译缺口 3 类（.skl/.obj/.atk）。

## 7. 困难与简化

| DNF 原版行为 | 缺口/困难 | 简化建议 |
|---|---|---|
| 跳跃演出（瞬移 + 镜头 + 旋转残影，前/后方向选择） | **跳跃系统（R1-A2）+ 技能中方向输入（R1-A3）双撞** | 地面原地版：起势→挥斩直连（损失位移/背身跳的走位价值；判定主体不损） |
| 空中施放（免起势 + z≤170 门禁） | 跳跃系统 | 不做 |
| 单 PO 内 flag 切 atk（两段 HitReaction） | 技能内分段 HitReaction 切换（R4-B17 第 5 消费方） | 双 Area 顺序创建（PO 化判定既定绕过路线，245 §8 指引） |
| col0 判定盒/图像随等级 ×1.69 | 对象整体缩放延后（已多例） | 满级预折算（E2 绕过惯例） |
| 强制僵直 1000ms / ignore weight | HitstunMs 直译 / 无重量系统忽略 | 直译/忽略 |
| 黑屏长闪 + 白闪 + 震 8-9 + 镜头 lerp | 闪屏/屏震/镜头控制延后 | 跳过 |
| 无色×5 | 道具系统 | 跳过 |

## 8. 存疑与缺口上报

- **未考证**：①[executable states] 中 14 号状态；②[skill fitness second growtype] 单值 2 与 12 槽"第 5 位单填 30"的槽位语义（第 4/5 位=鬼泣对只填后位——疑"三觉树"独立槽，与 96/95 的成对填法差异是职业树代际线索，建议收尾在 R6-C4 捷径表补注"单填后位"变体）；③pvp col0 恒 100%（pvp 不放大判定）；④jump_pass 旋转残影的角度公式消费（纯视觉）；⑤missile 类粒子无（本技无 .ptl——演出全 ani 化）。
- **升级链结论（本批专项⑤）**：240 是 **239 的升级型主动技**（pre required 239 Lv1；239 的 [feature skill index] 无——239 无 TP），机制上"持续区+终结"重构为"跳跃一击+裂地两段"——与 95/97/99/101 同属"Ex 名 ≠ TP"的替换/升级主动技家族（swordman_ 二觉系对应物）。
- **给 239 的回填**：239 §2.3 记录的 24370 etc 表（#31-35 / #18-20）向后延伸三行——**#36 = BladePhantomEx_Crack_Explosion02.ani、atk #21/#22 = BladePhantomEx_Crack_Hold/Exp.atk**（case 240 专用）；F7 共享表查询范围可扩至 #36/#21-22。
- **给轮间经验候选**：①**sq_SetAttackBoundingBoxSizeRate**（setcustomdata case 240）——"攻击盒随数值列缩放"的引擎 API 首证（col0 斩击大小列的真实消费方），对象整体缩放缺口立项时的引擎侧参照；②"跳跃 = 瞬移 + 镜头演出"（sq_setCurrentAxisPos + getScrollBasisPos）——DNF 侧对跳跃系技能的轻量实现形态，跳跃系统立项的降级参照。
