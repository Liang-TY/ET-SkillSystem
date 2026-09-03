# P2 Editor：AI 自动化接口

> 本文规定如何让 AI 通过 Unity CLI 操作技能编辑器。目标是让 AI 调用和人工窗口使用同一套
> 编辑服务；AI 只提交语义操作，不模拟鼠标坐标，也不直接执行任意 C#。
> 命令格式以项目现有 Unity.Pipeline.Commands、[CliCommand] 和 [CliArg] 约定为基线。

## 1. 目标和边界

AI 第一版可以完成以下操作：

- 列出、搜索和读取 Skill、Bullet、Area、Buff、Action；
- 对整数 ID 指定的对象修改字段、列表项和时间窗口；
- 在保存前执行结构、引用、时间和动画资源校验；
- 显式保存、重新加载、打开窗口和生成确定性预览；
- 对代表性技能运行 Editor 回归并返回结构化结果。

AI 第一版不可以：

- 通过屏幕坐标点击、键盘录制或依赖窗口当前布局完成操作；
- 写入任意工程外路径、C#、Shell、网络请求或 Unity 场景中的运行时对象；
- 让 Editor 预览驱动 LSCast、LSWorld、碰撞、伤害、联网或回滚；
- 绕过校验直接覆盖 JSON，或在每次字段修改时刷新全局运行时 Loader。

## 2. 入口和分层

所有入口都调用同一个核心服务：

~~~text
SkillEditorOperations
  ├─ UIToolkit Window
  ├─ Unity Pipeline CLI（unity command）
  └─ 可选 batchmode -executeMethod
        |
        v
DocumentStore -> ValidationService -> Command/History -> Preview/Save
~~~

建议的核心类型：

| 类型 | 责任 |
|---|---|
| SkillEditorOperations | 无 UI 的 list/get/validate/patch/save/reload/preview/regression 门面 |
| SkillEditorRequest/Result | 命令输入和结构化 JSON 输出 |
| SkillEditorDocumentStore | 读取 DTO、快照、原子写入、外部变更检测 |
| SkillEditorValidationService | 对 DTO 和目录快照做无副作用校验 |
| SkillEditorSession | 窗口会话、选择、dirty、历史；CLI 请求可创建短生命周期会话 |
| SkillEditorPreviewService | 只读采样并输出预览图或帧摘要 |

CLI 适配器只能做参数解析和结果序列化，不在命令方法中复制编辑逻辑。若 Unity Pipeline
不能在 batchmode 中提供命令发现，batchmode 入口仍调用 SkillEditorOperations。

UI 会话可以跨交互保留选择、dirty 和 Undo/Redo；CLI/Batchmode 默认每次创建短生命周期操作上下文，
不在两条命令之间隐式保留一份可变 DTO。patch 自己完成“读取 → 应用 → 校验 → 可选保存”的原子流程。
若同一资产已在 UI 中 dirty，CLI 写操作返回 conflict，不尝试合并或覆盖。

建议的文件位置：

~~~text
Packages/cn.etetet.skill/Editor/SkillEditor/
  SkillEditorOperations.cs
  SkillEditorCliCommands.cs
  SkillEditorCliModels.cs
~~~

## 3. 第一版命令

命令名保持稳定，参数可在实现探针后增加可选项，但不得改变整数 ID 和保存语义。

| 命令 | 作用 | 主要参数 |
|---|---|---|
| skill_editor_list | 列出资产或按条件搜索 | kind、query、includeInvalid |
| skill_editor_get | 读取一个资产及文件哈希 | kind、id |
| skill_editor_validate | 校验一个资产或全部目录 | kind、id、request |
| skill_editor_patch | 应用受限字段/list 操作 | request、dryRun、expectedHash |
| skill_editor_save | 显式保存当前 UI 会话；无活动会话则报错 | kind、id、expectedHash |
| skill_editor_reload | 重建磁盘目录；目标在 UI 中打开时检查 dirty | kind、id、discardDirty |
| skill_editor_open | 打开窗口并按语义选中资产/字段/时间 | kind、id、fieldPath、timeMs |
| skill_editor_preview | 按指定时间和显示选项输出预览 | kind、id、timeMs、out |
| skill_editor_regression | 执行预先定义的 Editor 回归集 | set、out |

kind 只允许 Skill、Bullet、Area、Buff、Action 五种枚举值。所有内容寻址用整数 id；name
只作为返回信息、搜索和错误诊断。文件路径不能替代 id，也不能由请求自由指定。

AI 的普通数据修改优先使用一次性的 patch(save=true)，不依赖 save 命令保存跨命令状态。
save/open/reload 用于控制人工可见的 Editor 会话：discardDirty 默认为 false；没有明确授权时，
reload 不能丢弃窗口中的未保存修改。AI 需要查看某个时刻时直接设置 timeMs；不依赖实时播放等待。

## 4. patch 请求格式

复杂修改使用工程内 JSON 请求文件，避免把长 JSON 塞进命令行。请求文件本身不是新的数据源，
只描述一次可审计操作。

~~~json
{
  "kind": "Skill",
  "id": 11,
  "operations": [
    {
      "op": "replace",
      "path": "/manualBoxes/0/offMs",
      "value": 600
    },
    {
      "op": "replace",
      "path": "/phases/0/animId",
      "value": 49
    }
  ],
  "dryRun": true,
  "expectedHash": "sha256:...",
  "save": false
}
~~~

首版只支持白名单操作：

- replace：替换已存在的标量或完整对象字段；
- add：向允许的 list 末尾或指定索引加入一个强类型对象；
- remove：删除允许的 list 项；
- move：暂不实现；需要时先补设计和测试。

path 使用 JSON Pointer 风格，但字段名和容器必须经过 schema/DTO 白名单校验。禁止通过 path
访问任意文件、类型名、方法名或反射成员。value 按目标字段的强类型反序列化，不能把字符串
自动转换成整数引用。

## 5. 执行和返回约定

推荐的 AI 工作流：

~~~text
list -> get（取得 baseHash）
     -> patch(dryRun=true，返回诊断和 proposedHash）
     -> patch(save=true, expectedHash=baseHash）
     -> get/reload -> preview/regression
~~~

dryRun 的 patch 必须校验修改后的内存结果，所以不需要依赖下一条命令保存临时状态。validate
单独用于校验磁盘资产/全目录，或用同一 request 文件检查提议变更。

返回值必须是单个结构化 JSON 对象，至少包含：

~~~json
{
  "ok": true,
  "command": "skill_editor_patch",
  "kind": "Skill",
  "id": 11,
  "changed": true,
  "saved": false,
  "baseHash": "sha256:...",
  "resultHash": "sha256:...",
  "errors": [],
  "warnings": [],
  "diagnostics": []
}
~~~

错误时 ok=false，并返回 code、message、path、field、relatedId；不要只返回 Unity Console
中的自然语言。建议错误码包括：

- invalid_request：请求格式或操作不在白名单；
- not_found：kind/id 不存在；
- no_active_session：save 等 UI 会话命令没有匹配的活动文档；
- validation_error：数据或引用不合法；
- conflict：expectedHash、外部文件或 UI dirty 冲突；
- save_failed：原子写入失败；
- preview_failed：资源映射或渲染失败；
- editor_unavailable：Unity/Pipeline 不可用。

命令失败不应部分保存。patch 的默认行为为 dryRun；真正写盘必须显式 save=true，并使用
expectedHash 对 get 时读到的磁盘字节做乐观并发检查。save 命令只保存已打开的 UI 会话，
同样必须经过校验和哈希冲突检测。

## 6. 安全和一致性规则

1. 写入根目录固定为 Packages/cn.etetet.skill/Bundles/SkillParams/，只允许其下的预期
   skills、bullets、areas、buffs、actions 和 index.json。
2. 保存采用临时文件写入、校验、替换的原子流程；失败时保留原文件和 dirty 状态。
3. 保存前重新校验当前 DTO、整数引用、时间窗口和重复 ID；Error 阻止保存，Warning 可带占位
   预览但必须出现在结果中。
4. 文件被外部修改、窗口有未保存修改或哈希不匹配时返回 conflict；不自动覆盖、不自动丢弃。
5. validate/patch(dryRun) 只操作内存快照，不调用 SkillParamEditorLoader.ReloadFromDisk，
   不清空全局 SkillParamLoader。save/reload/regression 才允许显式更新运行时缓存。
6. 预览输出只能写入约定的工程内结果目录（例如 automation/results/ 或 Library/SkillPreview），
   命令不得接受任意绝对输出路径。
7. CLI 不提供 eval 任意代码能力。若未来需要批处理入口，方法名和参数必须是固定白名单。

## 7. Unity CLI 调用示例

交互式 Pipeline 命令遵循现有项目形式：

~~~text
unity command skill_editor_list --kind Skill --query HardAttack --json
unity command skill_editor_get --kind Skill --id 11 --json
unity command skill_editor_patch --request automation/skill-patches/001.json --json
unity command skill_editor_preview --kind Skill --id 11 --timeMs 300 --out automation/results/skill-11.png --json
~~~

是否支持 --json、--timeout 等全局选项由 Unity Pipeline 提供；技能命令本身只保证返回
结构化结果。命令注册参考 Packages/cn.etetet.uibuilder/Editor/Commands/UIBuilderCliCommands.cs，
不另造一套注册器。

batchmode 仅作为无头回归的备用入口，约定为固定的 ExecuteSkillEditorRegression 方法或
同等白名单入口；它必须复用 SkillEditorOperations，不能因为无 UI 就绕过验证和安全限制。

## 8. 实现顺序和最小验证

1. 先探针：确认 Unity.Pipeline.Commands 能发现命令，工程相对路径解析和 JSON 返回正常。
2. 实现 list/get/validate，只读验证 22 Skill、4 Bullet、14 Area、4 Buff、7 Action。
3. 实现 patch dryRun 和冲突检测，用 HardAttack 修改一个数值并确认磁盘未变。
4. 实现显式 save/reload，验证原子写入、哈希变化、错误不会留下半文件。
5. 接入 open/preview/regression；UI、CLI、batchmode 三入口比较同一份结果。
6. 将每条命令的输入、输出和失败样例加入 P2 验收证据，具体门槛见
   04-P2-Editor验收与问题上报.md。

CLI 接口是 Editor 的自动化表面，不改变 SkillParams schema、运行时技能语义或 P2/P3 边界。
若要增加脚本执行、场景战斗模拟、批量迁移或新的写入根目录，必须先按 04 文档上报并重新确认。
