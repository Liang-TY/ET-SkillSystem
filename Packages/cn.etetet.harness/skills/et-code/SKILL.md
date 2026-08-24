---
name: et-code
description: ET9 ECS coding standards. Use when writing or modifying any C# in this project - entity/system structure, layering, package conventions, analyzer red lines.
---

# et-code - ET9 编码规范入口

> 底本：`Notes/架构说明以及约定.md`、`Notes/ET框架分析器规则详解.md`、`Notes/创建包和写包代码的一些约定.md`

## 何时使用

- 新增/修改任何 C# 代码（Entity、Component、System、Handler、编辑器工具）
- 决定"这段代码放哪个包/哪个层"
- 创建新包、配置 asmdef/asmref
- 评审代码是否符合 ECS 架构

## 不要加载

- 纯文本/文档/Excel 操作（et-excel）
- 只涉及异步安全细节（et-async 已覆盖时）

## 默认动作

1. 先定层：数据定义 → Model；无 Unity 的逻辑 → Hotfix；带 UnityEngine 的数据 → ModelView；带 Unity 的逻辑 → HotfixView；纯工具类（无 Entity）→ 包的 Runtime/
2. Entity 只放数据（字段/EntityRef），行为全部写成 `static partial class XxxSystem` 的扩展方法
3. 按红线速查表自检（见下），不确定的规则去补读 references
4. 每个类一个文件；文件名 = 类名

## 红线速查（最高频）

| 红线 | 规则 |
|---|---|
| Entity 纯数据 | 不声明方法/委托；泛型 Entity 禁止 |
| 跨 Entity 引用 | 字段/集合里用 `EntityRef<T>`；参数可直接传 Entity |
| 友元 | 访问其他 Entity 内部必须 `[FriendOf(typeof(X))]` |
| 父子注解 | `AddChild<T>` 需 `[ChildOf(父)]`；`AddComponent<T>` 需 `[ComponentOf(父)]`，二者不可同标 |
| System 形态 | `[EntitySystemOf(typeof(X))]` + `[EntitySystem]` 标注生命周期方法，必须在 static partial class |
| 静态字段 | 必须标 `[StaticField]` |
| Hotfix 纪律 | 只放静态类（或 [EnableClass] 例外）；无非 const 字段 |
| 事件处理器 | 类名 = 事件名（如 `AppStartInitFinish_CreateUILSLogin`） |
| LSEntity | 禁 float/double，用 TSMath/TSVector |
| 日志 | Entity 内用 Fiber 日志（ET0026） |

## 分层职责表

| 目录 | 程序集 | 职责 | 引用 Unity | 热更 |
|---|---|---|---|---|
| Model/ | ET.Model | Entity/Component/接口 定义 | 否 | 否 |
| Hotfix/ | ET.Hotfix | System/Handler 逻辑 | 否 | 是 |
| ModelView/ | ET.ModelView | 需要 UnityEngine 的定义 | 是 | 否 |
| HotfixView/ | ET.HotfixView | 渲染/UI/输入逻辑 | 是 | 是 |

## 包结构速记

- 纯工具放 `Runtime/`（自带 asmdef）；业务代码按四层放 `Scripts/`（只用 asmref，不用 asmdef）
- 所有包的同层代码汇入同一程序集（横切模式，lockstep 包定义的四个 asmdef 全项目共用）
- asmdef 名全局唯一：`ET.包名.层级`
- `package.json` 管 UPM 依赖（可能性），asmdef references 管编译可见性（实际能用）
- 服务端代码放 `Scripts/*/Server/` 或 `Share/`；Server 禁止引用 ET.Client

## 按需补读

- `references/et-code-rules.md`：完整 32 条分析器规则分组 + 源码生成器 + 序列化约束
- `Notes/创建包和写包代码的一些约定.md`：包创建、asmdef/asmref/csproj 关系详解
