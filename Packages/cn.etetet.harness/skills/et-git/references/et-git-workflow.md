# ET Git 工作流参考

## 提交流程

1. `git status --short` — 确认影响范围
2. `git diff --stat` — 查看变更统计
3. 暂存时精确控制范围，排除：
   - `Logs/`、`Bin/` 构建输出
   - 临时文件、IDE 配置文件
   - 与本任务无关的改动
4. 保留新建/移动文件的 `.meta`
5. 仅在本次任务需要时才暂存 Proto/Luban 生成结果
6. `git diff --staged` — 最终确认

## 中文提交信息格式

```
动作 + 对象 + 影响范围/原因
```

规则：
- **一行简短**，不写正文清单（需要说明的细节放 Notes 文档或 issue）
- **禁止附加任何自动签名**（如 Co-Authored-By 等 trailer）
- 确有未完成事项才在末尾加 `遗留问题：...`

示例：
- `移植unitybridge包:读控制台信息指令`
- `修复 YIUI 框架初始化时相机渲染问题`
- `添加 UBridge ping 指令`

## Rebase 规范

- 禁止 `git pull`，只用 `git pull --rebase`
- 或 `git fetch` + `git rebase origin/main`
- 冲突时只 rebase 解决，禁止 merge
- 风险操作前先说明影响
