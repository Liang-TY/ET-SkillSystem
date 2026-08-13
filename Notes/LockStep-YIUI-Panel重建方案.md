# LockStep YIUI Panel 重建方案

> 将 `Packages/cn.etetet.lockstep/Bundles/UI/` 下旧的 ET 风格 prefab 迁移到 YIUI 框架。

---

## 一、现状分析

### 1.1 原有 prefab

`Packages/cn.etetet.lockstep/Bundles/UI/` 下有 3 个：

| Prefab | 组件 | 功能 |
|--------|------|------|
| `UILSLobby.prefab` | Canvas, GraphicRaycaster, ReferenceCollector | 大厅：匹配按钮(EnterMap)、回放按钮(Replay)、回放路径输入(ReplayPath) |
| `UILSLogin.prefab` | Canvas, GraphicRaycaster, ReferenceCollector | 登录：账号(Account)、密码(Password)、登录按钮(LoginBtn)、背景(Background) |
| `UILSRoom.prefab` | — | 房间（本次暂不处理） |

它们使用 ET 传统方式：Canvas + ReferenceCollector 绑定子控件，代码通过 `rc.Get<GameObject>("EnterMap")` 获取引用。

### 1.2 现有代码

**ModelView（组件定义）**：
- `UILSLobbyComponent.cs`：enterMap(GameObject), text(Text), replay(Button), replayPath(InputField)
- `UILSLoginComponent.cs`：account(GameObject), password(GameObject), loginBtn(GameObject)

**HotfixView（逻辑+事件）**：
- `UILSLobbyEvent.cs`：从 Bundles/UI 加载 prefab，Instantiate，add Component
- `UILSLobbyComponentSystem.cs`：通过 ReferenceCollector 绑定控件，EnterMap→Match、Replay→读取文件回放
- `UILSLoginEvent.cs`：同上加载方式

**UIType 枚举**：已定义 `UILSLogin`、`UILSLobby`、`UILSRoom`

### 1.3 目标路径已有资源

`Packages/cn.etetet.lockstep/Assets/GameRes/YIUI/`：
- `Common/YIUIRoot.prefab` — YIUI 通用根节点
- `Login/Prefabs/LoginPanel.prefab` — 已有登录面板（含 Button、InputField、UIBlockBG，但不完整）
- `ScrollTest/` — 上次测试创建的 scroll demo

### 1.4 判断：是否复用

| 已有 | 判断 | 原因 |
|------|:---:|------|
| `Login/Prefabs/LoginPanel.prefab` | ❌ 删除重建 | 组件类型是 Legacy(Text/InputField/Button)，无 CDE 绑定，布局不完整 |
| `ScrollTest/` | 保留不动 | 测试用的，不相关 |

---

## 二、目标

用 YIUI 框架创建 2 个 Panel：

| Panel | 目标路径 | 控件 |
|-------|---------|------|
| **LoginPanel** | `Assets/GameRes/YIUI/Login/Prefabs/LoginPanel.prefab` | 账号输入、密码输入、登录按钮 |
| **LobbyPanel** | `Assets/GameRes/YIUI/Lobby/Prefabs/LobbyPanel.prefab`（新建目录） | 匹配按钮、回放按钮、回放路径输入、提示文本 |

---

## 三、布局方案

### 3.1 LoginPanel

```
LoginPanel (VerticalLayoutGroup, stretch full, padding=40, spacing=20, alignment=MiddleCenter)
├── UIBlockBG (已有，全屏阻挡)
├── Panel_Form (Image 背景板, 400x300, LayoutElement)
│   ├── Txt_Title (Text "登录", fontSize=24, Bold)
│   ├── Input_Account (InputField, placeholder="账号")
│   ├── Input_Password (InputField, placeholder="密码", contentType=Password)
│   └── Btn_Login (Button, text="登录")
```

### 3.2 LobbyPanel

```
LobbyPanel (VerticalLayoutGroup, stretch full, padding=30, spacing=16, alignment=MiddleCenter)
├── UIBlockBG (全屏阻挡)
├── Txt_Title (Text "大厅", fontSize=28, Bold)
├── Btn_Match (Button, text="匹配对战", 300x80)
├── Panel_Replay (Image 背景板, horizontal, padding=12, spacing=8)
│   ├── Input_ReplayPath (InputField, placeholder="回放文件路径", flex=1)
│   └── Btn_Replay (Button, text="回放")
├── Txt_Tips (Text "提示：输入回放文件路径后点击回放", fontSize=12, color=gray)
```

---

## 四、实现步骤

### Step 1：删除旧文件
- 删 `Login/Prefabs/LoginPanel.prefab`（重建）

### Step 2：创建 Panel
- `YIUICreatePanel --path "Assets/GameRes/YIUI/Login" --name "LoginPanel"`
- `YIUICreatePanel --path "Assets/GameRes/YIUI/Lobby" --name "LobbyPanel"`

### Step 3：布局（通过 CLI 指令）
- Load → AddControl/YIUIAddControl 加控件 → LayoutSet/ElementSet/FitterSet → TextSet/ImageSet 调样式 → PrefabSaveModified

### Step 4：CDE 绑定
- `YIUIBindComponent` — 绑定控件到 CDE C 表
- `YIUIBindEvent` — 创建事件定义（Sync 类型点击事件）
- `YIUIAttachEvent` — 挂载事件到按钮

### Step 5：生成代码
- `YIUIGenerateCode` — 生成 C# 组件类 + 事件绑定代码

### Step 6：补逻辑代码
- 删除旧的 ModelView/HotfixView 代码或修改为引用新生成的代码

---

## 五、组件映射

| 原 prefab | YIUI Panel | 原 ReferenceCollector key | YIUI 控件名 |
|-----------|-----------|--------------------------|------------|
| UILSLogin | LoginPanel | Account | Input_Account |
| | | Password | Input_Password |
| | | LoginBtn | Btn_Login |
| UILSLobby | LobbyPanel | EnterMap | Btn_Match |
| | | Replay | Btn_Replay |
| | | ReplayPath | Input_ReplayPath |

---

## 六、待确认

1. 登录面板是否需要标题栏/Logo？（目前方案包含但原 prefab 没有）
2. 回放路径输入：是否需要文件名自动补全 .bytes？  （原代码 `File.ReadAllBytes(self.replayPath.text)` 没有补全）
3. LobbyPanel 命名：是否沿用 UILSLobby 还是改为 LobbyPanel？（YIUI 惯例用 Panel 后缀）
