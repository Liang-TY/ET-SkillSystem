# ET 分析器完整规则（32 条）

> 完整版底本：`Notes/ET框架分析器规则详解.md`。本文件为速查结构，细节以底本为准。

## 为什么限制

ET 是数据驱动 ECS：Entity 纯数据容器，行为全在 System。分析器把架构约定变成编译期强制，核心保障：
1. MemoryPack 序列化安全（帧同步回滚快照）
2. 数据与行为分离
3. HybridCLR 热更安全（Hotfix 无状态）
4. 编译期类型安全

`[EnableClass]` 是"我知道我在做什么"的逃生阀，慎用。

## Entity 结构约束

| ID | 规则 |
|---|---|
| ET0003 | Entity 必须直接继承 Entity/LSEntity，不能多层继承 |
| ET0006 | Entity 不能声明方法 |
| ET0029 | Entity 不能是泛型 |

## 关系约束

| ID | 规则 |
|---|---|
| ET0001 | `AddChild<T>()` 检查 T 是否标 `[ChildOf(父类型)]` |
| ET0007 | `AddComponent<T>()` 检查 T 是否标 `[ComponentOf(父类型)]` |
| ET0014 | 不能对 Entity 基类直接操作 |
| ET0028 | 不能同时标 `[ComponentOf]` 和 `[ChildOf]` |

## 字段限制

| ID | 规则 |
|---|---|
| ET0004 | Hotfix 程序集不能声明非 const 字段 |
| ET0010 | Entity 不能声明委托 |
| ET0020 | Entity 不能直接引用 Entity（用 `EntityRef<T>`） |
| ET0023 | LSEntity 不能用 float/double |
| ET0015 | 静态字段必须标 `[StaticField]` |

## 访问控制

| ID | 规则 |
|---|---|
| ET0002 | 只有 `[FriendOf]` 能访问 Entity 内部 |

## 异步安全（细节叠加 et-async skill）

| ID | 规则 |
|---|---|
| ET0008 | 同步方法中 ETTask 必须 `.Coroutine()` |
| ET0009 | 异步方法中 ETTask 必须 await 或 `.Coroutine()` |
| ET0016 | await 后必须检查 CancelToken |
| ET0017 | await 调用必须传同一个 CancelToken |
| ET0018 | CancelToken 参数不能有默认值 |
| ET0019 | 不能传 null 给 CancelToken |
| ET0021 | async 方法必须返回 ETTask |

## 架构边界

| ID | 规则 |
|---|---|
| ET0022 | Server 不能引用 ET.Client 命名空间 |
| ET0032 | Model 只能声明 Entity 或 `[EnableClass]` 类 |
| ET0005 | Hotfix 只能声明静态类或 `[EnableClass]` 类 |

## 网络消息 / 唯一性 / 生成

| ID | 规则 |
|---|---|
| ET0030 | 消息类不能包含 Entity 字段 |
| ET0011 | UniqueId 必须在范围内 |
| ET0012 | UniqueId 不能重复 |
| ET0024 | 生命周期方法必须完整生成 |
| ET0025 | `[EntitySystem]` 方法必须在 `[EntitySystemOf]` 类中 |
| ET0026 | Entity 中必须用 Fiber 日志 |
| ET0027 | Entity 类全名 HashCode 必须唯一 |
| ET1001 | ETSystem 方法必须在 static partial class 中 |

## 实例化控制

| ID | 规则 |
|---|---|
| ET0013 | 静态类之间不能循环依赖 |
| ET0031 | `[DisableNew]` 类不能用 new 创建 |

## 源码生成器

| 生成器 | 作用 |
|---|---|
| ETEntitySerializeFormatterGenerator | 为 Entity 生成 MemoryPack 序列化代码（类型 Hash 多态分发） |
| ETGetComponentGenerator | 为每个 `[ComponentOf]` 生成 `GetXxxComponent()` 扩展 |

## 命名空间陷阱：ET 子命名空间下的类型解析

在 `ET.xxx` 子命名空间写代码时，C# 会先在父命名空间 `ET` 里解析类型。ET.Core 定义了 `Object`、`Entity` 等与 UnityEngine 同名/近名类型——**裸写 `Object` 会绑到 `ET.Object` 而非 `UnityEngine.Object`**（编译报 CS0029/CS0030，且不易看出原因）。

规则：ET 子命名空间内引用 UnityEngine 的 Object 必须**全限定 `UnityEngine.Object`**；同理警惕 `Scene`、`Random`、`Debug` 等潜在同名类型（`Scene` 已实际踩坑）。

若某类型（如 `Scene`）在文件里高频出现想用别名：**文件级 `using X = ...` 别名会输给父命名空间成员（ET.Scene 优先）**，别名必须声明在 `namespace ET.Xxx { }` **体内**才生效：

```csharp
namespace ET.UIBuilder
{
    using Scene = UnityEngine.SceneManagement.Scene; // 体内别名胜出
    ...
}
```

## 写码自检顺序

1. 这个类是什么？（Entity/System/Handler/工具类）→ 决定放哪层
2. 字段里有 Entity 引用？→ `EntityRef<T>`
3. 要访问别的 Entity 内部？→ `[FriendOf]`
4. 加孩子/组件？→ 注解齐了吗
5. 有 await？→ CancelToken 三连问（传了吗/同 token 吗/查了吗）
6. 静态字段？→ `[StaticField]`
