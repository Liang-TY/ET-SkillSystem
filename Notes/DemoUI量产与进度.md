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
| ① | LoginPanel | Login | Panel | ✓ | 账号/密码输入+登录按钮，黑底图 | ✅ |
| ① | LoadingPanel | Loading | Top | ✓ | 全屏黑图 #000000F2 + "加载中…"+副文本 | ✅ |
| ① | RoleCreatePanel | Role | Panel | ✓ | 名字输入+创建按钮（v1 极简） | ✅ |
| ① | RoleSelectPanel | Role | Panel | ✓ | 角色大卡片+进入城镇按钮 | ✅ |
| ① | MainHUDPanel | Lobby | Scene | ✗ | 左上角色名；底部中间 6 按钮横排 | ✅ |
| ① | MapSelectPanel | Battle | Popup | ✓ | 地图竖排按钮列表（占位 2 个）+关闭 | ✅ |
| ① | BattleTipPanel | Battle | Tips | ✗ | 顶部提示文本（战斗结束倒计时） | ✅ |

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

- **08-25 05:3x** 0011 验收合并：颜色修复方案②（spec 改 #RRGGBBAA 序，4 处）后 build_all 重建，结果 done（9/9 OK，0 失败）。wip-0011 快进并入 dev（49e3b30e8，8 prefab，1097/1097 行）。静态验收：8 个变更 prefab 做 fileID 归一化语义 diff，除 4 个颜色节点外全部 0 差异（其余为 builder 重建 fileID 重生成，节点多重集/锚点/位置/尺寸与 0010 版一致）。4 节点颜色逐项确认修复：Login BlackBg (0,0,0,α=0.902) 黑不透明、Loading BlackBg (0,0,0,α=0.949)、Loading 副文本 (0.667,0.667,0.667,α=1) 浅灰、BattleTip 文本 (1,0.8,0.267,α=1) 清晰黄。布局要点复核：MainHUD 按钮根底部中间 1100×56+6 按钮、角色名左上；RoleSelect 大卡片 u_ComBtnRole+进入按钮底部中间；MapSelect 右上 X(48×48)+2 地图钮竖列。**PNG 未回传**（见问题区 P3），本单为静态验收，批次① 颜色缺陷闭环。
- **08-25 05:1x** 0010 验收合并：wip-0010 干净并入 dev（4f3f3a093，108 文件）。build_all 结果 done（9/9 OK，无编译错误）；无 PNG 回传（build_all 单本身不含 preview 步骤，本轮未补单）。改为静态验收：7 面板 prefab 节点/组件/锚点/位置/尺寸与 spec 逐项核对全部一致；Login 密码框 m_ContentType=7=Password（Unity6 uGUI 枚举序已核实）；MainHUD 横排/MapSelect 竖排布局组件在位。发现 builder 颜色解析缺陷见问题区 P1。
- **08-25 03:1x** 文档创建；批次① 7 个 spec 已写、随 build_all 单 0010 下发。
- **08-25 03:2x** 排雷：旧试验版 LoginPanel（prefab+代码）已清除（防字段名冲突编译错）；发现登录入口已指向 YIUI，L1 免做。
- **08-25 03:1x** TestBroken spec（0009 验收用）使命完成，已删除，避免污染 build_all。

## 问题 / 待办

- **P3 从机未按新规则回传 PNG（0011 实证）**：`automation/worker/ui-worker.SKILL.md` 的 build_all 补 PNG 回传规则已随 5d3363b77 更新进仓库（origin/automation），但 Unity 机 watcher 执行 0011 时未产出 `automation/results/0011-*.png`——疑从机本地 skill 副本未同步仓库更新。0011 验收改走静态核验（结果见进度区），视觉验收欠账。需主会话处理：同步从机本地 skill 或确认 watcher 读取路径。
- **P1 builder 颜色字节序 bug —— 已解决（方案②，5d3363b77 + 0011 重建验证）**：spec 8 位色约定由 #AARRGGBB 改为 #RRGGBBAA（Unity `ColorUtility` 原生序），4 处色值重写；0011 重建后 4 节点颜色实测正确（0,0,0,0.902 / 0,0,0,0.949 / 0.667 灰 α=1 / 黄 α=1）。builder `PropConfigurator.ParseColor` 代码未动，如后续再写 #AARRGGBB 仍会错位——spec 侧务必按 #RRGGBBAA 书写。
- **P2 文档/spec 不一致 —— 已解决**：面板总表 Loading 黑底已随 5d3363b77 统一为 `#000000F2`（RRGGBBAA 序）。
- 0010 无预览截图回传：build_all 任务类型不含 preview 步骤。后续如需视觉验收，单独立 preview 单或在 build_all 后补标准链第二步（0011 已补 skill 规则但未生效，见 P3）。
