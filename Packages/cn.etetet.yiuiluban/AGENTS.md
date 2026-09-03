# cn.etetet.yiuiluban

## 概述

Luban 工具包，包含 Unity 编辑器窗口、Luban 源码副本、预编译工具和本项目使用的自定义导出模板。

## 修改约束

- `DontNet~/luban/src` 是 Luban 源码副本，修改前先确认是否有 `.ToolsGen/Custom` 覆盖模板。
- 当前项目的 C# 配置代码导出优先使用 `.ToolsGen/Custom/cs-code` 模板。
- 生成到 `CodeMode/Model/**` 的 Luban bean 根类应继承 `ET.Object`，不要额外生成 `[EnableClass]`。
- 不手工生成 `.meta`，需要时由 Unity 刷新生成。
