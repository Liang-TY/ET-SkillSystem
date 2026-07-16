# ET Async 参考

> 从 ET10 迁移，规则在 ET9 中完全适用。

## 何时保留异步

- RPC、IO、定时器、协程锁、跨 Fiber 结果、异步资源加载
- 调用方明确需要等待结果或错误
- 需要取消边界、超时边界或独立协程生命周期

## 何时不要异步化

- 只是为了接口统一、以后可能扩展、包一层转发
- 调用方不关心返回值，可以改成同步、消息 `Send` 或独立协程

## EntityRef 规则

- 任意 `Entity` 或其子类，在 `await` 后都不能直接复用旧变量
- `await` 前创建 `EntityRef<T>`
- `await` 后每次使用前重新获取对应 Entity
- 函数参数传 Entity；字段、属性、集合中保存 `EntityRef<T>`
- 不要用 `.Entity` 访问，直接赋值：`Unit unit = unitRef;`

## 正确写法

```csharp
public static async ETTask ProcessUpdate(this UpdateCoordinatorComponent self, UpdateTask task)
{
    EntityRef<UpdateCoordinatorComponent> selfRef = self;
    EntityRef<UpdateTask> taskRef = task;

    await SomeAsyncOperation();

    self = selfRef;
    task = taskRef;

    task.UpdateProgress("done");
}
```

```csharp
public class SomeComponent : Entity, IAwake
{
    public Dictionary<int, EntityRef<ProcessInfo>> ProcessDict { get; set; }
}
```

## 错误写法

```csharp
await SomeAsyncOperation();
self.DoSomething();   // ❌ await 后直接用旧 Entity
```

```csharp
ProcessInfo process = processRef.Entity;  // ❌ 用 .Entity 属性
```

```csharp
public List<ProcessInfo> Processes { get; set; }  // ❌ 集合中直接存 Entity
```

## 并发等待

- 为每一路启动独立收集协程，内部直接 `await`
- 外层只负责触发、`ETTask.WaitAll(...)` 和汇总
- 不要先返回多个未消费 `ETTask`，再在远处补 await
- 并发结果需要实体时，收集协程内部同样使用 `EntityRef<T>`

## ETCancellationToken 与 NewContext

- `ETCancellationToken` 只表达取消或超时，不承载业务状态
- `NewContext(...)` 只用于启动独立协程并传递取消边界
- 不要把 Entity 当 context 透传
- 需要跨协程携带实体时传 `EntityRef<T>` 或稳定 Id，再重新查实体

## Handler 与测试

- Handler `Run` 有 `await` 时同样必须用 `EntityRef` 重新获取
- 测试协程、普通系统方法、Helper 方法都遵守同一套规则

## 常见错误

- `await` 后直接访问旧实体变量
- 返回 `ETTask<ETTask<T>>`
- 写 `RunXxx + ETTask.Create + SetResult` 这种中转封装
- 把 Entity 塞进 `NewContext(...)` 或等待结果对象
