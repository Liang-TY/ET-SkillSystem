# R2-G：DNF 技能系统绑定链笔记（技能脚本常量 → 底层资源，全链已实证）

> 第2轮 Agent G 原始笔记。任务：断点②③——CUSTOM_ANI/ATTACK_INFO 映射表 + .skl/.atk 结构。
> 首要结论：script.pvf 解包后**所有 .skl/.chr/.atk/.ani/.act 全是同一种明文文本格式**——首行 `#PVF_File`，`[节名]` 分节、Tab 缩进、反引号作字符串引号、Tab 分列。原版 script.pvf 只是把这些文本加密打包，解包后无任何二进制。
> 已综合进：04-按键到伤害全链路-总结.md

---

## 〇、绑定链总图

```
【玩家技能】（以三段斩·剑魂版 tripleslashbs 为例，全链实证）

sqr/character/swordman_load_state.nut:153
  IRDSQRCharacter.pushState(职业枚举, "character/jg_swordman/swordghost_effect/tripleslash.nut",
                            "tripleslashbs", STATE_TRIPLESLASH_BLADESPIRIT=138, -1)
      │  注册：状态号 → 脚本文件 + 函数名后缀
      ▼
sqr/character/jg_swordman/swordghost_effect/tripleslash.nut
  obj.sq_SetCurrentAnimation(CUSTOM_ANI_TRIPLESLASH_BLADESPIRIT1)   ← 262
  obj.sq_SetCurrentAttackInfo(CUSTOM_ATTACK_INFO_TRIPLESLASH_BLADESPIRIT1) ← 142
  obj.sq_GetBonusRateWithPassive(8, -1, 0, 1.0)  ← 技能8(TripleSlash) [level info] 第0列
      │           │                        │
      │ 纯数字索引  │ 纯数字索引              │ 技能ID+列号
      ▼           ▼                        ▼
character/swordman/swordman.chr   character/swordman/swordman.chr   skill/swordman/tripleslash.skl
  [etc motion] 第262项             [etc attack info] 第142项          [dungeon][level info] 6列/等级
  (972行节首,973行=第0项)          (1293行节首,1294行=第0项)          + [static data] + [pvp]同构
  = `Animation/tripleslash_       = `AttackInfo/tripleslash_
     bladespirit1.ani`               bladespirit1.atk`
      │ 相对路径(大小写不敏感)        │                              │ 技能ID 8 的来源
      ▼           ▼                        ▼                     ▼
character/swordman/animation/   character/swordman/attackinfo/    skill/skilllist.lst
  tripleslash_bladespirit1.ani    tripleslash_bladespirit1.atk      职业类别0→`SwordmanSkill.lst`
  (明文,每帧贴图/攻击盒/音效)     (明文,命中属性/受击反应)           skill/swordmanskill.lst
      │                                `8` → `Swordman/TripleSlash.skl`
      ▼
[IMAGE] `Character/Swordman/.../sm_body%04d.img` + 105
      → 实际贴图 sm_body0105.img（在客户端 NPK 包里，pvf 内无 .img）
```

**引擎侧入口清单**（最外一层映射）：
- `pvf\character\character.lst`：职业索引 → .chr。`0 → Swordman/Swordman.chr`、`10 → Swordman/ATSwordman.chr`（女鬼剑）、另有 Demonicswordman.chr（黑暗武士）。
- `pvf\skill\skilllist.lst`：职业类别 → 技能清单。`0 → SwordmanSkill.lst`。

## 一、CUSTOM_ANI_*（如 262）→ .ani 路径：映射表 = `.chr` 的 `[etc motion]` 节

1. **swordman_header.nut 结构**（623 行）：**纯常量表**，别无其他。四段：STATE_*（状态号，138=TRIPLESLASH_BLADESPIRIT）、SKILL_*（技能ID）、CUSTOM_ANI_*（0~305，[etc motion] 下标）、CUSTOM_ATTACK_INFO_*/CUSTOM_ATK_*/CUSTOM_ATTACK_*/ATTACKINFO_*（同一 [etc attack info] 空间的下标，命名风格混杂但都指向同一张表）。
2. **映射表本体**：`pvf\character\swordman\swordman.chr` 第 972 行 `[etc motion]` ~ 1279 行，共 306 行反引号路径（=最大索引 305，严丝合缝）。**索引从 0 起算**（第 973 行 `Animation/Guard.ani` 是第 0 项）。
3. **验证（决定性）**：第 973+262=1235 行 = `Animation/tripleslash_bladespirit1.ani`；263→1236 行 bladespirit2、264→1237 行 bladespirit3，与 header 中三个连续常量一一对应。抽查其他段（BLOODRIVENMULTIHIT=87→BloodRivenMultiHit.atk、MINGYANJIAN=101→DarkFlameSlash_Atk1.atk 冥炎=DarkFlame）同样吻合。
4. **官方注释佐证**：`language.dof.character.md:116`——`sq_GetCustomAni(etcIndex)` 参数注释"**obj文件中[etc motion]的Ani指向，从0号开始计算**"；02-技能系统.md:615 也有"0是[etc motion]下面的第一个"。
5. **同族节**（.chr 内，均为"引擎命名槽位→路径"，不走数字索引）：`[waiting motion]`、`[attack motion]`（3项普攻）、`[ghost motion]`、`[jumpattack info]`、`[attack info]`、`[weapon hit info]`、`[weapon wav]`、`[body image path]` 等；另有 `[job]`、六套 `[growtype N]` 数值（各觉醒阶段属性、`[skill]` 初始技能表）。
6. **变体**：女鬼剑 atswordman.chr 的同节引用 `ATAnimation/...` 前缀 → `character/swordman/atanimation/`、`atattackinfo/` 目录。

**回答核心问题："CUSTOM_ANI_X=262 时引擎怎么找到 .ani"——查该角色 .chr 文件 [etc motion] 节的第 262 行（0起）路径，相对 .chr 所在目录解析。**

## 二、CUSTOM_ATTACK_INFO_*（如 142）→ .atk：映射表 = `.chr` 的 `[etc attack info]` 节

- 同文件 1293 行 `[etc attack info]` ~ 1463 行，第 1294+142=1436 行 = `AttackInfo/tripleslash_bladespirit1.atk`（143/144 同理吻合）。API 文档原话（language.dof.character.md:1645）：`sq_SetCurrentAttackInfo(attackInfoIndex)` 参数是".chr中的[etc attack info]攻击信息"。
- **.atk 格式**（明文 PVF 节格式，371 字节~数 KB）。全目录字段普查（165 个文件的字段频次）：`[attack type]`（physic/magic）、`[weapon damage apply]`、`[attack enemy]`、`[elemental property]`、`[damage reaction]`（damage/down/blow/knuck back…）、`[push aside]`(推距)、`[lift up]`(浮空高度)、`[attack direction]`（hit down/hit horizen/hit lift up/hit direction/front…）、`[damage bonus]`、`[blood] 数值 比率`、`[cut]`、`[hit wav]`(命中音效名)、`[ignore weight]`、`[force hit stun time]`、`[pvp]...[/pvp]`（PVP 数值覆盖块）。
- **重要更正（相对第一轮结论）**：攻击盒/受击盒**不在 .atk 里，而在 .ani 的每帧数据里**。.atk 只管"命中后的属性与反应"，.ani 管"哪一帧、多大的盒子"。引擎扫描当前动画当前帧的 [ATTACK BOX] × 已设置的 attackInfo 共同判定命中。

## 三、.skl 文件结构（明文，实读 ashenfork.skl 3826 字节）

- `[name2]`英文名 / `[name]`中文名（"银光落刃"）/ `[explain]`、`[basic explain]`技能说明
- `[purchase cost]`、`[required level]` 5、`[required level range]`、`[type]`(active)、`[skill class]` 1、`[maximum level]` 70、`[growtype maximum level]`（6职业各一列）、`[skill fitness growtype]`
- `[command] {6=`(SKILL)`}` 指令、`[command key explain]`（"跳跃状态下 Z"）、`[icon]` 图标 img+序号、`[durability decrease rate]`、`[skill preloading image]`、`[feature skill index]` 153（TP/强化技能关联）
- **数值区**（脚本真正读的）：
  - `[dungeon]` → `[consume MP] 10 120`（初值/满级值）、`[cool time] 4000 4000`（毫秒）、`[static data] 50 10 50 100`（技能级常量，sq_GetIntData 读它）、`[level info]`：**首数字=列数（此处3），随后每级一行**，列即 sq_GetLevelData(技能ID, 列号, 等级)（如 lv1 行 `185 185 100` = 物理攻击力/冲击波攻击力/冲击波大小%）
  - `[pvp]`：同构的完整数值覆盖块；`[death tower]`、`[warroom]` 为空壳同构
  - `[level property]`：列的 UI 展示模板（`<int>` 占位、正负号格式）
- **技能ID → .skl 的映射**：`pvf\skill\swordmanskill.lst` 每两行一组 = 技能ID + .skl 相对路径（已验证 16→Swordman/AshenFork.skl、8→Swordman/TripleSlash.skl）。上层 skilllist.lst 把职业类别映射到各 *skill.lst。TripleSlash 的 6 列中，脚本取列 0（百分比 158→981%）与列 3（固伤 47），与 nut 中 sq_GetBonusRateWithPassive(8,-1,0,…)/sq_GetPowerWithPassive(8,-1,3,…) 吻合。
- **注意**：社区搜到的".skl=骨骼文件"说法与本 PVF 无关（那是其他引擎的混淆信息）；DNF PVF 内 .skl 就是技能数值文本。

## 四、.ani 结构与技能→动画的最终绑定

**.ani 明文格式**（784,593 个文件；实读 animation/tripleslash_bladespirit1.ani）：
- 顶层：`[FRAME MAX] 7`、`[FRAME000]…[FRAME006]`、`[LOOP]`（循环起始帧）、`[SHADOW]`（0=无影）、`[SPECTRUM]`+`[SPECTRUM TERM]`（残影）
- 每帧：`[IMAGE]`（贴图路径含 %04d 格式符 + 数字105 → 实际 sm_body0105.img，.img 在客户端 NPK 包中，PVF 提取目录内不存在）、`[IMAGE POS] x y`、`[RGBA]`（含半透明 127/隐形 0 的闪帧用法）、`[DELAY]`（毫秒，如 80ms=12.5fps）、`[DAMAGE TYPE]`（NORMAL/SUPERARMOR 霸体帧，普查仅两值）、`[PLAY SOUND]`（帧音效名）、**`[ATTACK BOX] 六个整数`**（例 `-150 -25 0 300 50 80`）、**`[DAMAGE BOX]`**（例 `-9 -5 -6 55 10 100`）。六数按"两对 (x,y,z) 角点"解释与数值量级吻合（x横/-150~300 斩击宽度、y纵深/-25~50、z竖直/0~80 身高），**此解释为推断，格式本身实证**。
- `.als` 边车（如 attack_bladespirit1.ani.als）：`[use animation] 效果ani相对路径 + 触发帧号`、`[none effect add]`——在同播帧挂特效动画，属附加层。

**绑定在哪一层完成（实证最深点）**：技能→动画的最终绑定**就在 .nut 状态脚本运行时**——sq_SetCurrentAnimation(索引) 经 .chr [etc motion] 表解析成 .ani 路径。.skl 里**没有**动画引用（只有图标/预载贴图）；.chr 只提供"角色可用动画/攻击信息的有序清单"，不区分技能。即：**数据(.skl/.chr/.ani/.atk)提供素材，绑定关系由脚本逻辑(每技能的 nut)在运行时拼装**。佐证：ashenfork（银光落刃）没有任何 nut 文件，纯靠引擎内置跳跃攻击状态 + .chr 数据槽驱动——引擎内置状态可只用数据，而自定义技能一律走 nut。

**ACT（.act，52,137 个，明文同格式）**：**与玩家技能无关**，是怪物/AI角色/地图物件的帧级行为脚本（monster/*/action/*.act、aicharacter/*、character/common/action/），语法为 `[MOTION][BASE ANI]相对路径.ani` + `[TRIGGER]`（帧/次数/变量条件）+ `[BEHAVIOR]`（SET DAMAGE BOX ON/OFF、DESTROY、CENTER MSG…）。它的动画绑定是**直接相对路径**，不走数字索引——技能系统之外的另一条链（知识库 14-ACT脚本说明.md 内容与实物吻合）。

## 五、参考 URL
- [最爱午后红茶：DNF DX11 复刻项目（游戏资源篇）](https://www.blackteahouse.com/skills/projects/md/projects/dnf-dx11/1-2)——独立开发者证实"pvf 存几乎所有游戏数值"，与本笔记实地结论一致
- [藏宝湾：DOF PVF 解密解包原理分析（开源）](https://www.iopq.net/thread-17098351-3-1.html)、[DNF台服PVF加密解密之路](https://dnf.arad.ink/thread-3004-1-1.html)、[Zageku/DNF_pvf_python](https://github.com/Zageku/DNF_pvf_python)——script.pvf 容器加密/解密（解密后即 #PVF_File 文本）
- [hooyantsing/npk-api](https://github.com/hooyantsing/npk-api)、[DNF Extractor 使用指南(dfonexus)](https://dfonexus.com/t/dnfe-usage-guide/282)——NPK（.img 贴图实体）解包工具

## 六、没打通/标注推断的环节
1. **[ATTACK BOX]/[DAMAGE BOX] 六整数的字段级含义**：两角点解释高度合理但无源码级证据（标注：推断）。〔注：R1-D 已实证两角点结论，交叉确认〕
2. **.img 内部格式**（NPK 内序列帧图）：PVF 提取目录里没有，需解 NPK 才能继续（未做）。〔注：我们仓库已有 NpkImgParser 解析〕
3. **引擎二进制侧**：无法从数据侧实证，本文引擎行为描述均以"数据结构+API 注释+脚本用例"三角互证。
4. **.ani 的 [IMAGE] 第二参数与 %04d 替换规则**（105→0105）：从模式与量级推断，未见引擎代码。
5. swordman.chr 的基础动画槽由引擎固定语义命名（如 [back motion] → Attack3.ani），各槽完整语义表未逐一考证。
6. ashenfork 无 nut 属引擎内置状态，其"跳跃高度→伤害加成"逻辑在引擎里，数据侧只见 [level info] 结果值（行为细节未打通）。
