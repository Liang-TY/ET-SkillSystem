# Demo UI 量产与进度

> 2026-08-25 创建。P1 产线（uibuilder + automation 总线）投产后的第一批真实订单。
> 本文档是唯一驱动源：按批次执行，进度/问题记录在文末。spec 规范见 `Notes/UIBuilder-P1实施方案.md` §3。

## 决策记录（2026-08-25 用户确认）

| 项 | 决策 |
|---|---|
| 覆盖目标 | demo 全流程 UI：登录→选角→城镇→选图→战斗→返回城镇 |
| MainHUD 按钮 | **底部中间**横排；战斗时**保留** |
| 角色血条 | 战斗时**常驻**，**顶部最左**（BattleInfoPanel 内） |
| 怪物血条 | **不打怪就隐藏**，顶部左、角色条下方（同面板） |
| 技能 HUD | **要**，**底部右边**（SkillHUDPanel） |
| 登录 | 接现有流程，**只替换面板**（改 `AppStartInitFinish_CreateUILSLogin` 入口） |
| 背包 | 假数据 20 随机 item 撑格子；item 占位复用现有 TestScrollItem.prefab |
| 伤害飘字 | v1 逻辑层代码创建（Text+DOTween 上飘渐隐），不进 spec 体系 |
| 怪物血条形态 | 并入 BattleInfoPanel（不再独立 prefab），显示/隐藏由逻辑控 |
| 贴图 | 无贴图，自带控件外观（P1 决策延续） |

## 面板总表

| 批次 | 面板 | pkg | 层级 | blockBg | 内容要点 | 状态 |
|---|---|---|---|---|---|---|
| ① | LoginPanel | Login | Panel | ✓ | 账号/密码输入+登录按钮，黑底图 | ✅* |
| ① | LoadingPanel | Loading | Top | ✓ | 全屏黑图 #000000F2 + "加载中…"+副文本 | ✅* |
| ① | RoleCreatePanel | Role | Panel | ✓ | 名字输入+创建按钮（v1 极简） | ✅ |
| ① | RoleSelectPanel | Role | Panel | ✓ | 角色大卡片+进入城镇按钮 | ✅ |
| ① | MainHUDPanel | Lobby | Scene | ✗ | 左上角色名；底部中间 6 按钮横排 | ✅ |
| ① | MapSelectPanel | Battle | Popup | ✓ | 地图竖排按钮列表（占位 2 个）+关闭 | ✅ |
| ① | BattleTipPanel | Battle | Tips | ✗ | 顶部提示文本（战斗结束倒计时） | ✅* |

> \* 结构验收通过（prefab/生成代码与 spec 逐项核对一致），但带 8 位色值（#AARRGGBB）的节点颜色被 builder 解析错位（见问题区 P1），等修复后重建即可闭环。
| ② | SkillHUDPanel | Battle | Scene | ✗ | 底部右边技能按钮组（占位 4） | ⬜ |
| ② | BattleInfoPanel | Battle | Scene | ✗ | 顶部左：角色血条(常驻)+怪物血条(默认隐藏) | ⬜ |
| ② | BagPanel | Lobby | Popup | ✓ | grid 容器，运行时填 20 假 item | ⬜ |
| ② | RoleInfoPanel | Lobby | Popup | ✓ | 文本行：unit 数值 | ⬜ |
| ③ | SettingsPanel / ShopPanel / ActivityPanel | Lobby | Popup | ✓ | 标题+"功能开发中" | ⬜ |

## 逻辑对接清单（面板落地后的代码层工作）

| # | 改动 | 状态 |
|---|---|---|
| L1 | `AppStartInitFinish_CreateUILSLogin` 改开 YIUI LoginPanel（旧 UILSLogin 退役） | ✅ 无需改动——入口本就指向 YIUI LoginPanel（open/close 均已接线，仅引用类型名，同名重建后自动生效） |
| L2 | 登录成功 → LoadingPanel → RoleSelectPanel（demo 恒有占位角色，RoleCreate 暂不入流程） | ⬜ |
| L3 | RoleSelect 进入城镇 → 关流程面板 → 开 MainHUDPanel（+批② SkillHUD） | ⬜ |
| L4 | 选图按钮 → MapSelect 弹窗 → 点地图 → LoadingPanel → 进战斗（与按 N 并存） | ⬜ |
| L5 | 战斗开始 → 开 BattleInfoPanel（角色条常驻；怪条隐藏） | ⬜ |
| L6 | 命中怪 → 怪条显示+血量更新；伤害飘字（逻辑层创建） | ⬜ |
| L7 | 怪死亡 → BattleTip 倒计时文本 → 3 秒回城镇 → 关 BattleInfo | ⬜ |
| L8 | 背包假数据填充（20 随机 item，TestScrollItem 占位） | ⬜ |

## 进度记录

- **08-25 05:1x** 0010 验收合并：wip-0010 干净并入 dev（4f3f3a093，108 文件）。build_all 结果 done（9/9 OK，无编译错误）；无 PNG 回传（build_all 单本身不含 preview 步骤，本轮未补单）。改为静态验收：7 面板 prefab 节点/组件/锚点/位置/尺寸与 spec 逐项核对全部一致；Login 密码框 m_ContentType=7=Password（Unity6 uGUI 枚举序已核实）；MainHUD 横排/MapSelect 竖排布局组件在位。发现 builder 颜色解析缺陷见问题区 P1。
- **08-25 03:1x** 文档创建；批次① 7 个 spec 已写、随 build_all 单 0010 下发。
- **08-25 03:2x** 排雷：旧试验版 LoginPanel（prefab+代码）已清除（防字段名冲突编译错）；发现登录入口已指向 YIUI，L1 免做。
- **08-25 03:1x** TestBroken spec（0009 验收用）使命完成，已删除，避免污染 build_all。

## 问题 / 待办

- **P1 builder 颜色字节序 bug（影响批次①3 面板 4 节点）**：spec 约定 8 位色 #AARRGGBB，而 `PropConfigurator.ParseColor` 直接交给 `ColorUtility.TryParseHtmlString`（Unity 8 位格式为 #RRGGBBAA，alpha 在末尾），通道错位。实测：Login BlackBg `#E6000000`→(0.90,0,0,a=0) 完全透明且染暗红（应为 90% 不透明黑）；Loading BlackBg `#F2000000`→(0.95,0,0,a=0) 同病；Loading 副文本 `#FFAAAAAA`→(1,.67,.67,a=.67) 半透明粉（应为不透明浅灰）；BattleTip 文本 `#FFFFCC44`→(1,1,.8,a=.27) 淡黄 27%（应为不透明黄，倒计时不醒目）。6 位色不受影响。修复建议（二选一，待主会话定）：①改 `Packages/cn.etetet.uibuilder/Editor/Build/PropConfigurator.cs` ParseColor，9 长度串 AA 前置重排为 RRGGBBAA 再解析（推荐，spec 约定不动），随后无需改 spec 直接再下 build_all 重建即闭环；②spec 约定改为 #RRGGBBAA 并重写现有 4 处色值。spec 侧无需改动。
- **P2 文档/spec 不一致（小）**：面板总表 Loading 黑底写 `#E6000000`，spec 实为 `#F2000000`；P1 修复时顺手统一。
- 0010 无预览截图回传：build_all 任务类型不含 preview 步骤。后续如需视觉验收，单独立 preview 单或在 build_all 后补标准链第二步。
