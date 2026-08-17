# 配置方案选型与 DNF 翻译管线（终版）

> 适用工程：`et9lockStepYIUITest`（ET9 + YIUI + Unity URP + HybridCLR + 帧同步）
> 本文档是配置格式的**最终决策**，综合了所有讨论（JSON/C#/Lua/TypeScript/Excel/热更需求/条件分支）。
> 结论已定，后续不再讨论"要不要换格式"。

---

## 0. 最终结论（先看这个）

```
需要发布后可改的东西         格式                  热更方式
────────────────────────────────────────────────────────────
技能逻辑 + 技能数值           C# → SkillDefs.dll    HybridCLR 换 DLL
动画帧数据                   JSON                  YooAsset 换 .json
装备条件特效                 JSON 条件+动作对象      YooAsset 换 .json
怪物/装备基础数据            JSON                  YooAsset 换 .json
结构性常量（AnimId 枚举等）   C# 编译进主程序集       不需要热更
```

**原则：逻辑+数值在一起用 C# DLL（内聚）；纯数据用 JSON（机器翻译/数据驱动组合）；不用 Lua/TypeScript/假C#文本。**

---

## 1. DNF 的配置语法是什么

DNF 用的是**自制的"节式键值格式 + 内嵌 DSL"**——没有标准名字，本质是 INI 的方言（2005 年韩国 MMO 时代产物）。

五种基础构件：
```
① 简单键值     [name] `银光落刃`
② 多值行       [IMAGE POS]  -249  -301          ← 一个键带多个位置参数
③ 重复节       [layer variation] 2300 `coat_c`   ← 同名节出现多次=列表
④ 嵌套节       [maze info] → 里面再套 [greed]    ← 树结构
⑤ 表格行块     [level info] 6                     ← 首行=列数，之后每行一级
               185 185 100
```

三处内嵌 DSL：
- 装备条件特效：`[if] [party count] = 3 [then] [stat] +45 [/if]`
- 技能指令：`[command] {6=(RIGHT)}{8=&}{6=(SKILL)}`（花括号记号+连接符）
- 迷宫形状：`[greed] NN BB FF`（每格 2 字符编码）

**为什么难读**：给机器和内部工具看的（无 schema、无类型、位置语义、无注释）。2005 年为 C++ 手写解析器优化、磁盘紧凑（加密打包走慢网络）、策划用内部可视化工具编辑。**不是给人读的。**

---

## 2. 配置表达力光谱（从轻到重）

```
层级1：纯数据        { "damage": 100, "cd": 5000 }                    ← JSON
层级2：结构化逻辑    { "if": {...}, "then": {...} }（有限动词组合）      ← JSON + 条件/动作对象
层级3：表达式语言    if hp < 10% and cooldown > 60s then consume(...)  ← Lua / TypeScript
层级4：完整脚本     function onCast(ctx) ... end（任意逻辑）            ← C# DLL (HybridCLR)
```

**我们要层级 1、2、4，不要层级 3。**

---

## 3. 所有方案完整对比

| 方案 | 注释 | 类型安全 | Unity 原生 | 热更方式 | 迭代成本 | 适合谁 |
|------|------|---------|-----------|---------|---------|--------|
| **JSON** | ❌ | 弱 | ✅ JsonUtility | YooAsset 换 .json | 直接改文本 | 工具生成/数据驱动组合 |
| **JSON5** | ✅ | 弱 | ❌ 要换 parser | 同上 | 直接改文本 | 人手写（暂不用） |
| **YAML** | ✅ | 弱（隐式类型坑） | ❌ 要外库 | 同上 | 缩进敏感 | 大型管线（不选） |
| **TOML** | ✅ | 中 | ❌ 要外库 | 同上 | 好 | 中小型（不选） |
| **Lua** | ✅ | ❌ | 需 xLua/MoonSharp | 换 .lua | 直接改文本 | 传统游戏（**不选**，见§4） |
| **TypeScript** (puerts) | ✅ | ✅（编译时） | 需 puerts + V8/QuickJS | 换 .ts→编译→热加载 | 要 TS 编译链 | 腾讯级团队（**不选**，见§4） |
| **Excel→C# 代码生成** | ✅ | ✅ | ET 有 excel 包 | 重新生成+编译 | 策划改 Excel | 大批量策划数据（以后上） |
| **C# 代码编译进主程序集** | ✅ | ✅ | ✅ Unity 编译 | ❌ 不能热更 | 改 .cs→Unity 重编译 | 结构性常量 |
| **C# SkillDefs.dll** ★ | ✅ | ✅ | HybridCLR 加载 | **HybridCLR 换 DLL** | 改 .cs→dotnet build 2-5秒 | 技能逻辑+数值 |
| **假C#文本运行时解析** | ✅ | ❌ | ❌ 自写 parser | 换文本 | 直接改文本 | ❌ 不做（见§5） |

---

## 4. 为什么不选 Lua / TypeScript

### Lua 的代价
| 维度 | 评价 |
|------|------|
| 性能 | 解释执行，比 C# DLL 慢一个量级 |
| 类型安全 | 没有——写错字段名运行时才炸 |
| 调试 | VM 内堆栈，C# 断点打不进 |
| 依赖 | 需嵌入 VM（xLua 原生绑定或 MoonSharp 纯C#但慢） |
| 确定性 | **帧同步隐患**——不同平台 Lua VM 浮点行为可能有差异 |
| 团队 | 又多一门语言 |

### TypeScript (puerts) 的代��
| 维度 | 评价 |
|------|------|
| 类型安全 | ✅ 编译时有（tsc）——这是 TS 比 Lua 最大的优势 |
| 调试 | ✅ VS Code 断点——比 Lua 好 |
| 依赖 | ❌ V8 引擎（~30MB 增包体）或 QuickJS + TS 编译管线 + C#↔JS binding |
| 热更 | 换 .ts → tsc 编译 .js → 热加载（比换 DLL 多一步） |
| 生态 | 腾讯自家在用，中文资料多 |

### 不选的共同理由

**你已经有 HybridCLR 热更 C# DLL——这比任何嵌入脚本更强：**
- 表达力：完整 C#（比 Lua/TS 强）
- 类型安全：编译器抓一切（比 Lua 强，与 TS 编译时等价）
- 性能：原生编译（比任何解释执行快）
- 确定性：C# + TrueSync 定点数（帧同步安全）
- 热更：换 DLL（与 Lua 换 .lua / TS 换 .ts 等价）

**加 Lua/TS VM 只增加复杂度，不增加不可替代的能力。** 适用于"没有 C# 热更能力"的项目或"策划独立写逻辑"的中型团队；你是单人程序员，不需要。

---

## 5. "C# 代码配置"的两种模式（最容易混淆的）

| | 模式 A：编译型 C# 代码 ★我们用的 | 模式 B：C# 语法文本运行时解析 ❌不做 |
|---|---|---|
| 写什么 | `.cs` 文件：`public const FP Damage = 100;` | 一个 `.cs` 风格的**文本文件**（不是真 C#） |
| 怎么生效 | 编译进程序集 → 运行时直接读字段 | 需要运行时 parser 读文本 |
| 要 parser 吗 | **不需要**（编译器在 build 时做完了） | **要**——解析 C# 语法比 JSON 还难 |
| 改值后 | 改 .cs → 编译 → 生效 | 改文本 → 运行时 parse |
| 热更 | HybridCLR 换 DLL | 换文本 |

**你现在的 SkillLogic const 区 = 模式 A = 不需要 parser。** 如果要运行时文本，用 JSON（JsonUtility 就是现成 parser），别做假 C#。

---

## 6. Excel→生成 生成什么

**不是中间文本。** Excel 是策划的编辑界面，生成产物是两种之一：

```
Excel（策划编辑）
   ↓ 代码生成工具（build 时，如 ET 的 cn.etetet.excel 包 / Luban）
   ├→ 路径 A：C# partial class + [StaticField] static 字典 → 编译进程序集
   │          运行时：ConfigCategory.Instance.Get(id)  ← 零 parser
   └→ 路径 B：二进制 blob（MemoryPack/protobuf）→ 资源文件
              运行时：反序列化  ← 不是文本 parse
```

适合**大批量策划数值**（装备表/掉落表/怪物表）。等规模上来再上；现在 JSON 够用。

---

## 7. 条件分支怎么解决（装备的 [if]/[then]）

**核心认知：DNF 的 [if]/[then] 本身就不是任意代码，是"有限条件动词×有限动作动词"的配对表。** 可以用结构化 JSON 表达：

```json
{
  "triggers": [
    {
      "event": "onAttackSuccess",
      "probability": 0.02,
      "actions": [
        { "type": "addStat", "stat": "str", "value": 30, "durationMs": 20000 }
      ]
    },
    {
      "event": "onHpBelow",
      "threshold": 0.10,
      "conditions": [
        { "type": "cooldownElapsed", "ms": 60000 }
      ],
      "actions": [
        { "type": "consumeItem", "itemId": 3187 },
        { "type": "restoreHp", "percent": 0.10 }
      ]
    }
  ]
}
```

- `event` = 什么时候评估（有限集合：攻击命中/HP低于/进副本/队伍人数/…）
- `conditions` = 附加条件（AND 语义，有限集合：冷却/已有Buff/…）
- `actions` = 满足后干什么（复用阶段5 Actions 系统）

C# 端实现条件评估器 + 动作分发（跟已有的 ILSAction 同构）：
```csharp
public interface ICondition { bool Evaluate(LSUnit unit); }
// CooldownElapsedCondition / HpBelowCondition / PartyCountCondition ...
```

**限制（诚实）**：
- 改数值/加新触发 → 纯 JSON 热更 ✅
- **新增一种条件类型/动作类型** → 要写 C#（一年加几个，程序的事）

**注意：这套适用于装备特效（动词组合），不适用于技能**——技能有独特行为逻辑（冲撞撞墙截断、命中每个目标只扣一次），写 C# SkillLogic 类比配 JSON 更自然。两层各用各的。

---

## 8. SkillDefs.dll 方案（最终采用的技能配置方式）

这已在技能系统设计文档（`Notes/技能系统/05-阶段4`）里定过，此处确认它正好满足"发布后可改"的需求：

```
开发期（90% 时间）：
  技能代码在 skill 包的 Hotfix 层（asmref → ET.Hotfix）
  改代码 → Unity 自动编译 → Play → 断点调试
  跟写普通 C# 没区别

发布期（热更技能）：
  改 .cs → dotnet build Packages/cn.etetet.skill/DotNet~/SkillDefs
  → 产物 ET.SkillDefs.dll → 放入 HybridCLR 热更目录
  → 重启游戏 → SkillLoader 加载新 DLL → 扫描 [SkillId] 注册 → 生效
```

**对比 JSON 方案的优势**（单人程序员场景）：

| | C# SkillDefs.dll | JSON |
|---|---|---|
| 类型安全 | ✅ 编译器抓一切 | ❌ 打错字段名静默丢 |
| 数据+逻辑 | ✅ 在一起（一个类 = 逻辑 + 数值） | 分离（要维护解析层） |
| IDE | ✅ 补全/重构/跳转 | ❌ |
| 迭代 | dotnet build 2-5 秒 | 直接改文本（快 3 秒，差距不大） |
| 适合 | 程序员 | 策划团队 |

---

## 9. 翻译管线：DNF 配置 → 我们的格式

**关键前提**：第二轮 G 证明 `.skl/.chr/.atk/.ani/.equ` **全是明文 `#PVF_File`**——可以写程序批量翻译。这条路已经走通过：我们的 move.json/stay.json 就是 DNF 原版 `.ani` 的逐值直译（R1-D 实证逐值相同）。

### 9.1 翻译器设计

```
AnimTranslator（编辑器工具菜单 / dotnet CLI）
├── 输入：pvf 源目录路径 + 要翻译的目标列表
├── 核心：line-based 状态机 parser（~100 行 C#）
│   读到 [节名] 进状态 → 按行切值 → 填到目标结构
└── 输出：我们的格式，落到 Bundles/AnimRes/ 等目录
```

### 9.2 各文件类型的映射

| DNF 文件 | 翻译成 | 注意点 |
|---------|--------|--------|
| `.ani` | `AnimClipData JSON` | 归一化 min>max 倒序；引用帧 ArgbData=null 跳过；一帧多盒取数组；IMAGE RATE 负值镜像；DAMAGE TYPE 三值映射 |
| `.atk` | attack.json 顶层受击反应四件套 | 盒几何不在 .atk！在 .ani 帧里 |
| `.skl` | 技能 C# 的 const 区（翻译器吐出数值，贴进 SkillDefs 技能类） | [level info] 列数+每级行→按列名生成常量 |
| `.mob` | 怪物配置 JSON（属性系数/AI 参数/掉落） | 系数×等级基础表关系保留 |
| `.equ` | 装备 JSON + 条件触发器（§7 格式） | [if]/[then] 结构化转 triggers 数组 |
| `.chr` | （参考用）资源清单→注册表 | 不直接翻译，是路径解析参考 |

### 9.3 脏数据处理

- min>max 倒序 → 自动 Math.Min/Max 修正
- 引用帧 ArgbData=null → 跳过
- 一帧多盒 → 输出数组（我们 JSON 目前只留一个→翻译器遇多盒输出 `boxes: [...]`）
- [IMAGE RATE] 负值 → 保留为 imageRate.x<0

### 9.4 落地顺序

1. **现在**：`.ani → AnimClipData JSON` 翻译器（做 attack.json 时直接吃 DNF 原版鬼剑攻击动画）
2. **阶段 4（技能系统）**：`.skl → C# const`（翻译器吐出伤害列数值，贴进技能类）
3. **以后（装备/怪物）**：`.equ → 装备 JSON`、`.mob → 怪物 JSON`——parser 核心共用

---

## 10. 一页速查

```
DNF 配置语法？  自制 INI 方言（[节]/Tab/反引号/+ 内嵌 DSL）；2005年给机器看的

最终采用什么？
  技能逻辑+数值 → C# SkillDefs.dll（HybridCLR 热更换，类型安全，数据逻辑内聚）
  动画帧/装备/怪物 → JSON（YooAsset 热更，机器翻译产物/数据驱动组合）
  装备条件分支 → JSON triggers 数组（有限条件动词×动作动词，结构化对象）
  结构常量     → C# 编译进主程序集

为什么不选 Lua/TS？  已有 HybridCLR 热更 C#（比嵌入 VM 更强），加 VM 是多余依赖

Excel 生成啥？  C# 代码或二进制（编译/反序列化），不是中间文本

翻译管线？  DNF 全明文 → 写 parser（~100行）批量翻译；move.json 已验证通路
```

---

## 11. 补充：项目组织（GameContent 项目）

> 本节是对 §0/§8 结论的**项目结构补充**，开发到一定规模后再细化。

### 11.1 问题

§0 的分工是"技能→C# DLL、装备/怪物→JSON"。但如果项目早期配置量小（几十条），全 JSON 还要维护解析层、两种热更通道（DLL + YooAsset），不如全放一个 C# 项目简单。

### 11.2 早期方案：全 C# 一个项目

项目早期（单人开发、几十条配置量），所有可热更的游戏内容放一个 C# 项目，编译成一个 DLL：

```
Packages/cn.etetet.gamecontent/
├── DotNet~/GameContent.csproj          ← 独立 C# 项目
├── Skills/                             ← 技能逻辑+数值（SkillLogic 子类）
├── Equipment/                          ← 装备数据（C# 静态字典）
├── Monsters/                           ← 怪物数据（C# 静态字典）
└── Conditions/                         ← 装备条件特效（C# 接口实现，不走 JSON triggers）
```

```csharp
// 技能：逻辑+数值在一起
[SkillId(1001)]
public class ChargeSkill : SkillLogic
{
    public override int CooldownMs => 5000;   // 数值
    public override void OnCast(...) { ... }  // 逻辑
}

// 装备数据：C# 静态字典（不用 JSON）
public static class EquipmentConfig
{
    public static readonly Dictionary<int, EquipmentDef> All = new()
    {
        [1001] = new EquipmentDef { Name = "短剑", PhysicalAttack = 621, ... },
    };
}

// 装备条件特效：C# 接口实现（不走 JSON triggers）
public class TitleAttackProcBuff : IConditionTrigger
{
    public float Probability => 0.02f;
    public bool Evaluate(LSUnit unit) => unit.LastEvent == AttackSuccess;
    public void Execute(LSUnit unit) => unit.AddStat(StatType.Str, 30, 20000);
}
```

**优势**：
- 类型安全（编译器抓一切）
- 一个 DLL 热更全生效（不用维护两套热更通道）
- 数据+逻辑内聚（一个文件看完一个技能/装备的全貌）
- IDE 补全/重构
- 不用写 JSON 解析层

**劣势**：
- 数据量大了手写不现实（几百件装备时）
- 改数值要 dotnet build（2-5 秒，对程序员可接受）

### 11.3 什么时候拆分

| 阶段 | 配置量 | 策略 |
|------|--------|------|
| **早期（现在）** | 几十条 | 全 C# 一个 GameContent.dll ✅ 最简单 |
| **中期** | 几百件装备/几十种怪 | 装备/怪物数据迁 JSON（YooAsset），技能逻辑留 DLL |
| **后期** | 上千件+策划团队 | 装备/怪物上 Excel→codegen，技能逻辑留 DLL |

### 11.4 动画帧数据的特殊性

**AnimClipData 始终是 JSON**——它是翻译器从 .ani 吐出来的机器产物（每帧的盒子/标记/延时/渲染效果），一帧就是一行数据、一个动作几十帧、手写 C# 不现实。这条不随项目阶段变。

### 11.5 项目命名

不叫 "SkillDefs"（因为不止技能），叫 **GameContent**——"所有可热更的游戏内容"的家：

```
GameContent 项目
├── 编译产物：ET.GameContent.dll → HybridCLR 热更
├── 包含：技能逻辑+数值 / 装备数据 / 怪物数据 / 装备条件特效
└── 不含：动画帧（JSON/YooAsset）/ 框架代码（主程序集）
```

### 11.6 两套热更通道的关系（早期简化版）

```
GameContent.dll（C# 编译）
  ├── 技能 SkillLogic 子类
  ├── 装备 EquipmentConfig 静态字典
  ├── 怪物 MonsterConfig 静态字典
  └── 条件特效 IConditionTrigger 实现
      ↓ dotnet build
  ET.GameContent.dll
      ↓ HybridCLR 替换
  运行时：SkillLoader 扫描 [SkillId] + Config 静态初始化器 → 全部就绪

AnimClipData JSON（翻译器产物）
  ↓ YooAsset 加载 TextAsset
  运行时：JsonUtility.FromJson → LSAnimResComponent 缓存
```

早期就这两条线：**一个 DLL（逻辑+数值+条件特效）+ JSON（动画帧）**。等数据量大了再拆第三条（装备/怪物 JSON）。
