---
name: et-excel
description: Excel file operations via Claude Code built-in xlsx skill. Use when creating, reading, editing .xlsx/.csv/.tsv files, adding formulas, formatting, or charts.
---

# et-excel — Excel 操作入口

> ET9 不依赖 ET10 的 `ET.ExcelMcp` 工具。改用 Claude Code 内置 **xlsx skill** 处理 Excel。

## 依赖环境

| 依赖 | 版本 | 安装路径 |
|------|------|----------|
| Python | 3.11.4 | `C:\Users\Liang\AppData\Local\Programs\Python\Python311\python.exe` |
| openpyxl | 最新 | `pip install openpyxl` |
| pandas | 2.1.0 | 已安装 |

> pip 命令：`& "C:\Users\Liang\AppData\Local\Programs\Python\Python311\python.exe" -m pip install openpyxl`

## 何时使用

- 创建新 Excel 文件并写入数据
- 读取 `.xlsx`、`.csv`、`.tsv` 文件
- 编辑现有文件（修改内容、添加公式、格式化）
- 修复损坏的 Excel 文件
- 数据表格式转换（csv ↔ xlsx 等）

## 不要加载

- 只是改 C# 代码、编译、操作 Unity
- 操作的是 Luban 配置表导出（用 `et-luban`）

## 使用方式

### 创建 Excel

在对话中说：创建 Excel 文件、写入数据。AI 会调用 xlsx skill 生成 Python 脚本。

示例 prompt：
```
创建一个 Excel 文件，sheet 名"员工表"，列：姓名/年龄/部门，写入 5 行数据，保存到 Notes/employees.xlsx
```

### 读取 Excel

示例 prompt：
```
读取 Notes/employees.xlsx，打印所有内容
```

### 编辑 Excel

示例 prompt：
```
在 Notes/employees.xlsx 的 C 列后插入一列"薪资"，填入数据，修改表头颜色
```

## 技术细节

xlsx skill 使用 openpyxl（创建/编辑/格式化）和 pandas（数据分析/批量操作）。AI 自动选择合适的库。

### openpyxl 示例（格式化 + 公式）

```python
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill

wb = Workbook()
ws = wb.active
ws['A1'] = '公式测试'
ws['A2'] = 10
ws['A3'] = 20
ws['A4'] = '=SUM(A2:A3)'  # Excel 公式
ws['A1'].font = Font(bold=True, color='FF0000')
wb.save('output.xlsx')
```

### pandas 示例（数据分析）

```python
import pandas as pd
df = pd.read_excel('file.xlsx')
print(df.describe())
df['新列'] = df['年龄'] * 2
df.to_excel('output.xlsx', index=False)
```

## 注意事项

- openpyxl 写入公式后，Excel 打开时会自动计算
- pandas 读取大文件时指定列：`pd.read_excel('f.xlsx', usecols=['A','C'])`
- 日期列用 `parse_dates=['date_col']`
