# ET10 Harness Skills 分析

> 源路径：`D:\Projects\et10initLockStepYIUI-UITest\Packages\cn.etetet.harness\skills\`
> 目的：评估 6 个 AI 技能文档，判断哪些可迁移到 ET9

---

## 总体结构

```
skills/
├── index.md              # 技能索引 + 路由矩阵
├── et-code/              # C# 编码规范
├── et-async/             # 异步编程安全
├── et-build/             # 构建与导出
├── et-git/               # Git 工作流
├── et-excel/             # Excel 操作（MCP 工具）
└── et-luban/             # Luban 配置导出
```

---

## 各技能详细分析

### 1. et-async（异步编程） ⭐ 最值得迁移

**内容：** `await` 之后禁止直接访问 Entity，必须用 `EntityRef<T>` 重新获取。字段/集合/属性必须存 `EntityRef<T>` 而非原始 Entity。并发等待使用 `ETTask.WaitAll`。`ETCancellationToken` 仅用于取消/超时。

**ET9 可迁移性：🟢 高。** ET9 的 `ETTask`、`EntityRef<T>`、`ETCancellationToken` API 与 ET10 几乎一致，原则完全相同。

**需适配：** `NewContext(...)` API 细节可能不同；`EntityRef<T>` 隐式转换行为可能略有差异。

---

### 2. et-git（Git 工作流） ⭐ 值得迁移

**内容：** 禁止 `git pull`，只用 `pull --rebase`。中文提交信息格式：`动作 + 对象 + 范围`。暂存前 `git diff --stat` 审查。排除 `Logs/`、`Bin/`、临时文件。保留 `.meta`。

**ET9 可迁移性：🟢 高。** 纯项目约定，与 ET 版本无关。

**需适配：** 排除路径可能略有差异；提交信息风格依项目习惯。

---

### 3. et-build（构建命令） ⚠️ 部分可迁移

**内容：** `dotnet build ET.sln`（唯一编译命令）、`dotnet ./Bin/ET.Proto2CS.dll`（Proto 导出）、`dotnet ./Bin/ET.App.dll`（服务器启动）。

**ET9 可迁移性：🟡 中。** 命令形式完全相同（`dotnet build ET.sln`），但 DLL 路径需调整。

**需适配：** ET9 的路径是 `Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll` 而非 `./Bin/`；服务器入口可能不同；发布脚本位置不同。

---

### 4. et-luban（配置导出） ⚠️ 部分可迁移

**内容：** `dotnet ./Bin/ET.ExcelExporter.dll` 导出 Luban 表为 C# 代码。

**ET9 可迁移性：🟡 中-高。** 流程相同但路径不同。

**需适配：** `cn.etetet.*` → ET9 包命名；Luban 版本可能不同导致命令行参数差异。

---

### 5. et-code（编码规范） ❌ 重写成本高

**内容：** 5 层包依赖层级、`packagegit.json` 格式、`PackageType.cs`、Module Analyzer、Handler 规范。ET10 专属包名（`cn.etetet.wow`、`cn.etetet.actorlocation` 等）。

**ET9 可迁移性：🔴 高。** 80% 内容引用了 ET10 专属包名和层级，直接复制无效。

**需适配：** 重新为 ET9 的包集合建立层级表；`packagegit.json` 格式校验；Module Analyzer 存在性确认。

---

### 6. et-excel（Excel 操作） ❌ 无法迁移

**内容：** 通过 `ET.ExcelMcp.dll` CLI 工具操作 Excel（读写单元格、样式、公式、图表）。

**ET9 可迁移性：🔴 高。** `ET.ExcelMcp.dll` 是 ET10 专属工具，ET9 不存在。ET9 的 Excel 操作通过 `cn.etetet.excel` 包的 `ET.ExcelExporter` 完成。

---

### 7. index.md（技能索引） ❌ 重写成本高

**内容：** 任务 → 技能路由表。引用了 `cn.etetet.unitybridge`、`cn.etetet.test` 等 ET10 专属包。

**ET9 可迁移性：🔴 高。** 路由表全部引用 ET10 包名和工具。需要根据 ET9 项目实际配置重写。

---

## 迁移优先级建议

| 优先级 | 技能 | 理由 |
|--------|------|------|
| **1** | et-async | ✅ 已迁移 | 核心安全规则，零适配 |
| **2** | et-git | ✅ 已迁移 | 工作流约定，零适配 |
| **3** | et-build | ✅ 已迁移 | 路径已适配 ET9 |
| **4** | et-luban | 待迁移 | 流程相同，路径 + 版本适配 |
| **5** | et-code | 待迁移 | 需为 ET9 重写包层级 + 去掉 ET10 专属引用 |
| **6** | index.md | ✅ 已迁移 | 路由表适配 ET9，et-excel 路由到 Claude Code xlsx skill |
| ❌ | et-excel | 无法迁移 | 依赖 ET10 专属 MCP 工具 |
