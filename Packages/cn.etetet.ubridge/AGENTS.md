# cn.etetet.ubridge

> 从 ET10 `cn.etetet.unitybridge` v2.0.0 迁移到 ET9

## 概述

Unity 本地文件桥接包，提供：

- `DotNet~` 下的纯命令行 `ET.UBridge`
- `Scripts/ModelView/Client` 下的 Unity Editor 文件宿主，汇入 `ET.ModelView`
- `Scripts/Model/Share` 下的桥接命令、错误码与共享文件协议
- `Proto/` 下的命令消息定义

## UnityBridge skill 入口

- **架构规范**：请查看 `/et-code` skill
- **编译构建**：请查看 `/et-build` skill

本文件只保留 UnityBridge 相关 skill 的轻量路由。命中 UnityBridge 任务后按需读取下面对应 `SKILL.md`，不要一次性加载全部 UnityBridge 规则。

### et-unitybridge - UnityBridge 命令调用入口

**使用场景**：

- 查询 UnityBridge 宿主是否在线、Unity 是否正在编译、PlayMode 状态、CodeMode、Unity 版本。
- AI 操作 Unity Editor：资源、场景、选择集、GameObject、Transform、Inspector、Prefab、菜单、截图、GameView、Editor 测试。
- 执行 `Compile` / `Refresh` / `RegenProject` 等延迟命令。
- 排查 UnityBridge 返回的 `Error` / `Message`。

**补读**：`Packages/cn.etetet.harness/skills/et-unitybridge/SKILL.md`

## 核心目录

| 路径 | 说明 |
|------|------|
| `DotNet~` | `ET.UBridge.csproj` 与命令行入口 |
| `Scripts/ModelView/Client` | Unity Editor 宿主、处理器与分发逻辑 |
| `Scripts/Model/Share` | 桥接命令、错误码、路径与文件存储协议 |
| `Proto/` | 命令消息 protobuf 定义 |

## 当前命令清单

详见 `README.md`。50 个命令可用（系统/场景/选中/资源/GameObject/Transform/Prefab/Inspector/Editor控制/延迟）。
