---
name: et-async
description: ET async and EntityRef safety workflow. Use when adding, modifying, or reviewing async/await, ETTask, EntityRef, concurrent waits, or Handler Run async safety.
---

# et-async - ET Async 入口

> 从 ET10 `cn.etetet.harness` 迁移，核心规则在 ET9 中完全适用。

## 何时使用

- 新增、修改或 review `async` / `await` / `ETTask` / `ETTask<T>`
- 判断某段逻辑是否应该异步化
- 处理 `await` 后 `Entity` 访问、`EntityRef<T>`、Handler `Run` 异步安全
- 设计并发等待、`ETCancellationToken`、`NewContext(...)`

## 不要加载

- 代码完全同步，且不涉及异步边界或安全控制
- 只是编译、导出、跑测试、操作 Unity

## 默认动作

1. 先判断是否真的需要异步，并明确谁在等待返回值
2. 只有 RPC、IO、定时器、协程锁等真实异步资源，才保留 `async` / `ETTask`
3. 调用方不依赖返回值时，优先改成同步方法
4. 只要存在 `await` 后继续访问 Entity 的路径，就在 `await` 前创建 `EntityRef<T>`，`await` 后重新获取
5. 函数参数可以传 Entity；字段、属性、集合中保存 `EntityRef<T>`
6. `NewContext(...)` 只承担独立协程与取消边界

## 快速分流

- EntityRef 示例、并发等待、取消控制：补读 `references/et-async-rules.md`
- Handler `Run`、组件契约、包结构：叠加 `et-code`

## 输出要求

- 说明异步是否必要，以及谁在等待结果
- 说明 `await` 后实体访问是否通过 `EntityRef<T>` 保护
- 说明并发组织方式是否避免延后消费 `ETTask`
