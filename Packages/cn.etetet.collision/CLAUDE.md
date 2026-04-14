# cn.etetet.collision — 碰撞检测策略 CLAUDE.md

## 包概述

基础库包。提供碰撞检测的**宽相**策略，快速排除不可能碰撞的对象对，再交给 aabb 包做精确检测。

无 Unity 依赖，无 Entity/Component 模式，完全确定性。

## 目录结构

```
cn.etetet.collision/
├── Runtime/
│   ├── collision.asmdef            # 程序集名: ET.Collision
│   ├── BruteForceDetector.cs       # 暴力双循环 O(n²)
│   ├── SweepAndPruneDetector.cs    # 扫掠裁剪 O(n·k)
│   └── SpatialHashDetector.cs      # 空间哈希 O(n) 平均
├── DotNet~/
│   └── ET.Collision.csproj         # 服务端独立编译
├── package.json                    # 依赖 aabb, core, sourcegenerator, truesync
└── CLAUDE.md
```

## 依赖关系

```
collision → aabb → truesync
         → core
         → sourcegenerator
```

| 包 | 程序集 | 用途 |
|---|---|---|
| cn.etetet.aabb | ET.Aabb | `AABB` 结构体、`AABBUtil.Intersects` |
| cn.etetet.truesync | ET.TrueSync | `FP` 定点数 |
| cn.etetet.core | ET.Core | `[EnableClass]` 特性 |
| cn.etetet.sourcegenerator | ET.SourceGeneratorAttribute | 源码生成器 |

## 类型一览

### BruteForceDetector — 暴力双循环

**复杂度**: O(n²) — 对象数 < 100 时首选

**适用场景**: 格斗对战（街霸、拳皇）、DNF 普通战斗

```csharp
var pairs = new List<(int, int)>();
BruteForceDetector.Detect(aabbArray, pairs);
// pairs 中每项 (idA, idB) 对应碰撞双方的 AABB.Id
```

**性能估算**:
```
2 角色 × 5 box = 10 个 → 45 次检测
4 角色 × 5 box = 20 个 → 190 次检测
20fps 下每秒不到 4000 次，完全无压力
```

### SweepAndPruneDetector — 扫掠裁剪

**复杂度**: O(n·k)（k 为每对象平均邻居数）

**适用场景**: 横版/2D，角色主要沿 X 轴移动（DNF 刷图）

```csharp
var pairs = new List<(int, int)>();
SweepAndPruneDetector.Detect(aabbArray, pairs);
```

**原理**:
1. 按 `Min.x` 排序（确定性：相同 x 按原始索引排序）
2. 扫掠时 X 轴不再重叠的直接 `break`，跳过后续更远的对象
3. 只对 X 轴重叠的做完整 AABB 三轴检测

**性能估算**:
```
200 个 box，X 轴均匀分布
每对象平均 5-10 个邻居 → 约 800 次检测
比暴力 19,900 次减少 96%
```

### SpatialHash — 空间哈希

**复杂度**: O(n) 平均

**适用场景**: 固定场景、对象均匀分布（帧同步 RTS、大型战场）

```csharp
var hash = new SpatialHash();
hash.Init(cellSize: 5);  // 格子大小（FP 精度下的整数）

// 每帧：清空 → 插入 → 查询
hash.Clear();
foreach (AABB box in hurtBoxes)
    hash.Insert(box);  // 注册 AABB，自动记录 box.Id

// 查询与攻击区域重叠的所有受击对象
var hits = new HashSet<int>();
hash.Query(attackBox, hits);

// hits 中是 hurtBox 的 Id，精确确认后再处理
foreach (int hurtId in hits)
{
    AABB hurtBox = GetById(hurtId);
    if (AABBUtil.Intersects(hurtBox, attackBox))
        ApplyDamage(hurtId);
}
```

**原理**:
- 用质数异或 `(x * 73856093) ^ (y * 19349663) ^ (z * 83492791)` 计算 hash key
- 跨格子的大对象会注册到多个格子
- `HashSet<int>` 自动去重

## 策略选择决策树

```
对象数 < 100？
├─ 是 → BruteForceDetector（格斗对战、DNF 普通战斗）
└─ 否 → 横版/2D？
    ├─ 是 → SweepAndPruneDetector（DNF 刷图）
    └─ 否 → 对象均匀分布？
        ├─ 是 → SpatialHash（帧同步 RTS、大型战场）
        └─ 否 → 考虑八叉树（3D 开放世界，非帧同步典型场景）
```

## 确定性保证

- `BruteForceDetector`: 固定遍历顺序（i 从 0 到 n-1），结果稳定
- `SweepAndPruneDetector`: 按 `Min.x` 排序，相同 x 按原始索引排序，保证确定性
- `SpatialHash`: 质数 hash 消除字典遍历顺序依赖，`HashSet<int>` 去重后遍历结果与插入顺序无关

三种策略在帧同步环境中均可安全使用。

## 引用此包

**package.json**:
```json
"dependencies": { "cn.etetet.collision": "1.0.0" }
```

**asmdef**:
```json
"references": ["ET.Collision"]
```

**DotNet~ csproj**:
```xml
<ProjectReference Include="$(SolutionDir)Packages\cn.etetet.collision\DotNet~\ET.Collision.csproj" />
```
