# cn.etetet.aabb — 轴对齐包围盒碰撞检测 CLAUDE.md

## 包概述

基础库包。提供基于 TrueSync 定点数的 AABB（Axis-Aligned Bounding Box）碰撞检测，是碰撞系统的**窄相**层。

无 Unity 依赖，无 Entity/Component 模式，纯数学运算，完全确定性。

## 目录结构

```
cn.etetet.aabb/
├── Runtime/
│   ├── aabb.asmdef             # 程序集名: ET.Aabb
│   ├── AABB.cs                 # AABB 结构体
│   └── AABBUtil.cs             # 构造/检测/变换工具
├── DotNet~/
│   └── ET.Aabb.csproj          # 服务端独立编译
├── package.json                # 依赖 core, sourcegenerator, truesync
└── CLAUDE.md
```

## 依赖

| 包 | 程序集 | 用途 |
|---|---|---|
| cn.etetet.truesync | ET.TrueSync | `TSVector`、`FP` 定点数 |
| cn.etetet.core | ET.Core | `[EnableClass]` 特性 |
| cn.etetet.sourcegenerator | ET.SourceGeneratorAttribute | 源码生成器 |

## 类型一览

### AABB — 轴对齐包围盒

```csharp
public struct AABB
{
    public long Id;              // 身份标识，调用方显式赋值（默认 0，不自动分配）
    public TSVector Min;         // 最小角点
    public TSVector Max;         // 最大角点

    public TSVector Center;       // 计算属性: (Min + Max) / 2
    public TSVector Size;         // 计算属性: Max - Min
    public TSVector HalfExtents;  // 计算属性: Size / 2

    public bool Contains(TSVector point);  // 点包含检测
}
```

**Id 的用途**：碰撞检测结果（`List<(long, long)>`）中的每个元素 `(idA, idB)` 对应碰撞双方的 `AABB.Id`。调用方将 `Id` 设为实体 ID、碰撞框类型等任意值，碰撞后直接用 Id 定位原始对象，无需维护"数组索引 → 对象"的映射。

**Id 不自动分配（帧同步铁律）**：构造函数曾用静态计数器自增分配 Id，但静态计数器是进程级状态——回滚重放会重复执行帧逻辑，各端计数器不一致，而 AABB 会存进 LSEntity 快照参与 hash 校验 → 假报 desync。因此 Id 恒默认 0，需要身份时显式 `box.Id = xxx`。

```
攻击框 Id=101  ────相交────  受击框 Id=201
攻击框 Id=101  ────相交────  受击框 Id=202

结果: [(101, 201), (101, 202)]  ← 直接知道谁打了谁
```

### AABBUtil — 静态工具

#### 构造

```csharp
// 从最小/最大点
AABB box = AABBUtil.FromMinMax(min, max);

// 从中心 + 半尺寸
AABB box = AABBUtil.FromCenter(center, halfExtents);
```

#### 检测

```csharp
// AABB 相交（6 次 FP 比较，完全确定性）
bool hit = AABBUtil.Intersects(aabbA, aabbB);

// A 完全包含 B
bool inside = AABBUtil.Contains(outerAabb, innerAabb);
```

#### 变换

```csharp
// 合并两个 AABB（取最小 Min 和最大 Max）
AABB merged = AABBUtil.Merge(aabbA, aabbB);

// 向外扩展（每轴各扩 expansion）
AABB bigger = AABBUtil.Expand(box, expansion);
```

## 确定性保证

所有运算仅使用 TrueSync 的 `FP`（定点数）和 `TSVector`，不涉及 `float`、`System.Random`、`DateTime` 等非确定性类型。

`AABBUtil.Intersects` 的 6 次比较在所有平台上结果一致，适合帧同步逻辑层使用。

## 使用示例

### 基本碰撞检测

```csharp
// 创建两个包围盒，Id 设为实体 ID
AABB playerBox = AABBUtil.FromCenter(playerPos, new TSVector(FP.FromInt(1), FP.FromInt(2), FP.FromInt(1)));
playerBox.Id = playerEntityId;

AABB skillBox  = AABBUtil.FromCenter(skillPos,  new TSVector(FP.FromInt(3), FP.FromInt(1), FP.FromInt(2)));
skillBox.Id = skillEntityId;

// 检测相交
if (AABBUtil.Intersects(playerBox, skillBox))
{
    // 命中处理，直接用 skillBox.Id / playerBox.Id 知道是谁
}
```

### 扩大受击判定

```csharp
// 攻击范围比实际大一圈
AABB attackRange = AABBUtil.Expand(bodyBox, new TSVector(FP.FromInt(2), FP.FromInt(1), FP.FromInt(2)));
```

## 引用此包

**package.json**:
```json
"dependencies": { "cn.etetet.aabb": "1.0.0" }
```

**asmdef**:
```json
"references": ["ET.Aabb"]
```

**DotNet~ csproj**:
```xml
<ProjectReference Include="$(SolutionDir)Packages\cn.etetet.aabb\DotNet~\ET.Aabb.csproj" />
```
