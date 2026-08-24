---
name: et-git
description: ET git workflow for preparing commits and reviewing changes.
---

# et-git - ET Git 入口

> 红线：禁止直接使用 `git pull`。远端同步必须使用 `git pull --rebase`，或先 `git fetch` 再 `git rebase`。禁止 `merge`。

## 何时使用

- 准备提交本次改动
- 检查 `git status`、`git diff`、`git diff --staged`
- 筛除日志、临时文件、自动生成文件和无关改动
- 编写中文 `commit message`
- 拆分提交、整理暂存区、与远端同步

## 默认动作

1. 先看 `git status --short` 与 `git diff --stat`，确认影响范围
2. 只保留本次任务相关改动，排除 `Logs/`、临时文件、无关文件
3. 如果本次任务需要提交 `.meta`、导出结果或生成代码，则保留对应文件
4. 准备提交前，再看 `git diff` 与 `git diff --staged`，确认没有混入无关改动
5. 提交信息统一使用中文，格式：`动作 + 对象 + 影响范围/原因`
6. 提交信息**一行简短**，不写正文清单；**禁止附加任何自动签名**（如 Co-Authored-By 等 trailer）
7. 风险操作（拆分提交、回滚、`cherry-pick`、`rebase`）先说明影响再执行

## 优先入口

- `git status --short`
- `git diff --stat`
- `git diff`
- `git diff --staged`
- `git pull --rebase`

## 按需补读

- `references/et-git-workflow.md`：提交前检查、变更筛选、中文提交信息、rebase 规范
