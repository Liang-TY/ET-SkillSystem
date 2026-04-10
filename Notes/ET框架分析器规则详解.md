# ET 框架分析器规则详解

## 为什么限制 class 创建

核心原因：**ET 是数据驱动的 ECS 框架，Entity 必须是纯粹的数据容器，行为全部在 System 中实现。** 限制 class 是为了强制执行这个架构约束。

### 限制的是什么

不是"不能建 class"，而是**每个程序集只允许声明特定类型的 class**：

| 程序集 | 允许的 class | 分析器 ID |
|--------|-------------|-----------|
| **Model/ModelView** | Entity 及其子类，或标了 `[EnableClass]` 的 | ET0032 |
| **Hotfix** | 静态类，或标了 `[EnableClass]` 子特性的类 | ET0005 |

### struct 不受限制

struct 不受分析器检查（分析器只扫描 `ClassDeclarationSyntax`），所以纯数据容器优先用 struct。

---

## 为什么要限制

### 1. 保证序列化安全（最关键）

ET 的 Entity 要支持 MemoryPack 快照序列化（用于帧同步回滚）。如果随意创建 class 挂在 Entity 上，序列化时会遗漏或出错。分析器强制所有 Entity 遵循统一的序列化规则。

### 2. 强制数据与行为分离

```
Entity = 纯数据（字段/属性）
System = 纯行为（静态扩展方法）
```

- ET0006：Entity 不能声明方法
- ET0010：Entity 不能声明委托
- ET0020：Entity 不能直接引用其他 Entity（必须用 `EntityRef<T>`）
- ET0023：LSEntity 不能用 float（帧同步确定性）

### 3. 热更新安全

- ET0004：Hotfix 程序集不能声明非 const 字段
- ET0005：Hotfix 只允许静态类或框架特性的类

Hotfix 代码运行时热替换，如果有实例字段状态，热更后状态丢失。静态类无状态，热更安全。

### 4. 编译期类型安全

- ET0001：`AddChild<T>()` 检查 T 是否标了 `[ChildOf(父类型)]`
- ET0007：`AddComponent<T>()` 检查 T 是否标了 `[ComponentOf(父类型)]`
- ET0003：Entity 必须直接继承 Entity/LSEntity，不能多层继承

---

## 32 条分析器规则总览

### Entity 结构约束

| ID | 规则 | 目的 |
|----|------|------|
| ET0003 | Entity 不能多层继承 | 保持扁平继承，简化序列化 |
| ET0006 | Entity 不能声明方法 | 数据与行为分离 |
| ET0029 | Entity 不能是泛型 | 确保可序列化 |

### 关系约束

| ID | 规则 | 目的 |
|----|------|------|
| ET0001 | `AddChild` 类型检查 | 编译期验证父子关系 |
| ET0007 | `AddComponent` 类型检查 | 编译期验证组件归属 |
| ET0014 | 不能对 Entity 基类直接操作 | 强制类型安全 |
| ET0028 | 不能同时标记 `[ComponentOf]` 和 `[ChildOf]` | 语义清晰 |

### 字段限制

| ID | 规则 | 目的 |
|----|------|------|
| ET0004 | Hotfix 不能声明非 const 字段 | 热更新安全 |
| ET0010 | Entity 不能声明委托 | 序列化不兼容 |
| ET0020 | Entity 不能直接引用 Entity | 用 EntityRef，支持序列化 |
| ET0023 | LSEntity 不能用 float/double | 帧同步确定性 |
| ET0015 | 静态字段必须标 `[StaticField]` | 显式文档化静态状态 |

### 访问控制

| ID | 规则 | 目的 |
|----|------|------|
| ET0002 | 只有 `[FriendOf]` 能访问 Entity 内部 | 封装，友元模式 |

### 异步安全

| ID | 规则 | 目的 |
|----|------|------|
| ET0008 | 同步方法中 ETTask 必须用 `.Coroutine()` | 防止阻塞 |
| ET0009 | 异步方法中 ETTask 必须 await 或 `.Coroutine()` | 防止 fire-and-forget |
| ET0016 | await 后必须检查 CancelToken | 协作取消 |
| ET0017 | await 调用必须传递同一个 CancelToken | 取消传播 |
| ET0018 | CancelToken 参数不能有默认值 | 强制传递 |
| ET0019 | 不能传 null 给 CancelToken | 空引用安全 |
| ET0021 | async 方法必须返回 ETTask | 可追踪 |

### 架构边界

| ID | 规则 | 目的 |
|----|------|------|
| ET0022 | Server 不能引用 ET.Client 命名空间 | 客户端/服务端分离 |
| ET0032 | Model 只能声明 Entity 或 `[EnableClass]` 类 | Model 只放领域实体 |
| ET0005 | Hotfix 只能声明静态类或 `[EnableClass]` 类 | 热更代码控制 |

### 网络消息

| ID | 规则 | 目的 |
|----|------|------|
| ET0030 | 消息类不能包含 Entity 字段 | 网络消息必须可序列化 |

### 唯一性与代码生成

| ID | 规则 | 目的 |
|----|------|------|
| ET0011 | UniqueId 必须在范围内 | 网络实体 ID 范围校验 |
| ET0012 | UniqueId 不能重复 | 唯一标识 |
| ET0024 | 生命周期方法必须完整生成 | 源码生成器配合 |
| ET0025 | `[EntitySystem]` 方法必须在 `[EntitySystemOf]` 类中 | System 归属正确 |
| ET0026 | Entity 中必须用 Fiber 日志 | 多 Fiber 环境日志隔离 |
| ET0027 | Entity 类全名 HashCode 必须唯一 | 类型识别 |
| ET1001 | ETSystem 方法必须在 static partial class 中 | 源码生成器可扩展 |

### 实例化控制

| ID | 规则 | 目的 |
|----|------|------|
| ET0013 | 静态类之间不能循环依赖 | 初始化顺序可确定 |
| ET0031 | `[DisableNew]` 类不能用 new 创建 | 强制使用工厂/对象池 |

---

## 源码生成器

分析器之外，ET 还有两个源码生成器：

| 生成器 | 作用 |
|--------|------|
| **ETEntitySerializeFormatterGenerator** | 为所有 Entity 生成 MemoryPack 序列化/反序列化代码，使用类型 Hash 做多态分发 |
| **ETGetComponentGenerator** | 为每个 `[ComponentOf]` 生成类型安全的 `GetXxxComponent()` 扩展方法 |

---

## 总结

分析器把架构设计决策从"约定"变成了**编译期强制**，让团队无法写出违反 ECS 模式的代码。`[EnableClass]` 就是"我知道我在做什么，让我破例"的逃生阀。
