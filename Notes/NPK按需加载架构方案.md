# NPK 按需加载架构方案

> 2026-08-26 创建。基于 DNF 资源管理模式 + 行业成熟方案（Addressables/O3DE 依赖图/Handle-based 资源系统）设计。
> 本文档是唯一驱动源：按步骤实施，进度/问题记录在文末。

## 1. 目标

抛弃手工提取 .img.bytes 的 demo 管线，改为 **NPK 归档原样进项目 → 运行时挂载 NPK 索引 → 按需从 NPK 提取 IMG → 解析打图集 → 用完卸载**。

### 非目标（暂不做）

- NPK 文件增量更新/版本校验（启动器功能，非游戏引擎功能）
- UI 贴图从 NPK 加载（当前 UI 用 Unity 自带控件，后续 P2/P3 贴图 pass 时再接入）
- 音频资源按需加载（.ogg 条目当前跳过）

## 2. 核心决策记录

| # | 决策 | 理由 |
|---|---|---|
| D1 | **直接加载 NPK，不用 .img.bytes** | 省去手工提取步骤；NPK 原样进项目，管线自动化 |
| D2 | **不生成离线清单文件**（主方案） | DNF 的做法：配置在内存 → 运行时沿引用链查找。避免配置→清单→代码的多层耦合 |
| D3 | **备用方案：离线工具生成清单**（见 §8） | 如果运行时递归查找性能不达标（当前判断不会），退化到预计算 |
| D4 | **七种作用域**（base/character/town/dungeon/feature/event/temporary） | 精确控制生命周期，避免"常驻"变成垃圾桶 |
| D5 | **NPK 解析从 parse-img-ani 的 NpkAccess 改造** | 已有完整实现（魔数/XOR 解密/SHA-256 校验/索引表），改造为 lazy 挂载 |
| D6 | **IMG 解析复用项目内 NpkImgParser** | 已有且验证过，输入从 .img.bytes 字节改为 NPK 提取的字节 |

## 3. 架构总览

```
┌─ 离线准备（一次性）───────────────────────────────────────────┐
│  NPK 原始文件改名为 .npk.bytes 后放入 Bundles/NPK/ 目录（Unity 不认 .NPK 扩展名）                              │
│  （不再手工提取 .img.bytes，NPK 原样扔进去）                       │
└────────────────────────────────���────────────────────────────┘
          ↓
┌─ 运行时 ─────────────────────────────────────────────────────┐
│                                                               │
│  Layer 1: NpkArchive（新写）                                    │
│    Parse(npkBytes) → 读 header + 索引表 → Dictionary<虚拟路径, (offset,size)> │
│    ★ lazy：只建索引，不解析任何 IMG 内容                           │
│    Extract(virtualPath) → 按 offset/size 从字节流取指定条目        │
│    ★ 按需提取：只取字节，不转纹理                                  │
│                                                               │
│  Layer 2: NpkMountManager（新写）                                │
│    Mount(archiveName, npkBytes) × N 个 NPK                     │
│    Read(virtualPath) → 查所有已挂载 NPK → 提取字节                │
│    Unmount(archiveName) → 卸载不再需要的 NPK 归档                 │
│                                                               │
│  Layer 3: 资源作用域管理器 ResourceScopeManager（新写）             │
│    七种作用域（见 §4），每种有明确的触发/加载/卸载规则               │
│    内部沿配置链递归收集 IMG 虚拟路径 → 去重 → 批量提取 → 打图集      │
│                                                               │
│  Layer 4: 现有代码（最小改动）                                    │
│    NpkImgParser.Parse(bytes) ← 输入源从 .img.bytes 改为 NPK 提取  │
│    BuildAtlas / LSAnimResComponent ← 结构不变，数据源换了          │
└───────────────────────────────────────────────────────────���─┘
```

## 4. 七种资源作用域

| 作用域 | 触发时机 | 内容 | 生命周期 | 估算 |
|---|---|---|---|---|
| **base** | 登录成功 | UI 框架贴图、通用数字字体、真公共特效（受击火花/死亡灰化/升级光柱） | 会话期间从不卸载 | 2-5MB |
| **character** | 选角/切角 | 当前职业基础身体+时装+武器+该职业全部技能图标+该职业技能特效 | 选角加载，切角卸旧加新，进出副本不卸 | 10-30MB |
| **town** | 进城镇 | 城镇 tile/背景/NPC 外观/装饰 | 进城加载，离城卸载 | 5-15MB |
| **dungeon** | 进副本 | 该地图怪物 IMG+怪物技能特效+地图 tile/装饰+被动物件 | 进副本加载，出副本卸载 | 5-20MB |
| **feature** | 打开功能界面 | 背包物品图标集、商城商品图、技能书全职业图标 | 打开加载，关闭可���载（可缓存） | 每功能 1-10MB |
| **event** | 服务器下发活动配置 | 活动横幅/NPC/特效/地图装饰 | 活动开启加载，结束/过期卸载 | 每活动 1-20MB |
| **temporary** | 即时操作 | 对话立绘、时装预览大图、结算特效 | 用时加载，用完即卸 | 按需 |

### 作用域切换规则

```
登录成功:       load(base) + load(character)
进城镇:         load(town) → active = {base, character, town}
进副本:         unload(town) + load(dungeon) → active = {base, character, dungeon}
回城镇:         unload(dungeon) + load(town) → active = {base, character, town}
切角色:         unload(character旧) + load(character新) → base/town/dungeon 不动
打开背包:       load(feature.bag) → 用完可 unload(feature.bag)
活动开启:       load(event.spring) → 活动结束 unload(event.spring)
```

### 共享资源防误卸

多个作用域可能引用同一 IMG（如公共特效在 base 和 dungeon 都用到）。卸载 dungeon 时不释放 base 中已加载的同名资源。

实现方式：每个已加载的 IMG 记录其归属作用域集合。卸载作用域 A 时，只释放**仅属于 A** 的 IMG（`img.scopes == {A}`）。属于多个作用域的 IMG 保留，直到最后一个引用它的作用域被卸载。

## 5. 依赖收集规则（核心）

### 5.1 三种请求的收集链路

**请求角色：**

```
输入: 角色数据(职业=鬼剑士, 时装=[头部_X,上衣_X,...], 武器=太刀)
  → 职业 → 基础身体 IMG (sm_body0000.img)           ← 职业决定，同职业共享
  → 每件时装 → 时装部件 IMG (avatar_top_X.img)      ← 时装 ID 决定，同职业不同装扮不同
  → 武器类型 → 武器 IMG (katana_blade.img, ...)      ← 武器类型决定，同类型共享
  → 职业 → 该职业全部技能 → 每个技能的图标 IMG + 技能动画 IMG + 技能特效 IMG
```

**请求城镇：**

```
输入: 城镇配置(TownId)
  → 城镇 tile 路径列表 → 每个 .til → 引用的 .img
  → NPC 列表 → 每个 NPC 配置 → 外观 .img
  → 装饰动画列表 → 每个 .ani → 引用的 .img
```

**请求副本地图：**

```
输入: MapDefinition + 玩家职业
  链路1（地图）: MapDefinition → tile 路径 → .til → .img
                MapDefinition → 装饰动画 → .ani → .img
                MapDefinition → 被动物件 → 物件定义 → .ani → .img
  链路2（怪物）: MapDefinition.MonsterAiIds → MonsterAiDefinition
                → 动画列表 → .ani → .img
                → 技能列表 → SkillDefinition → .ani → .img
                → 特效列表 → .ani → .img
  链路3（玩家技能，因为进副本可能用任何已学技能）:
                玩家职业 → 该职业全部技能 → .ani → .img
                （这部分实际归 character 作用域，不跟地图走）
```

### 5.2 收集器接口

```csharp
/// <summary>资源依赖收集器：沿配置链递归收集 IMG 虚拟路径（纯逻辑，不碰 IO）</summary>
public static class ResourceDependencyCollector
{
    /// <summary>收集角色的全部 IMG 依赖</summary>
    public static HashSet<string> CollectCharacter(int classId, int[] avatarIds, int weaponType);

    /// <summary>收集城镇的全部 IMG 依赖</summary>
    public static HashSet<string> CollectTown(int townId);

    /// <summary>收集副本地图的全部 IMG 依赖（不含玩家，玩家走 character 作用域）</summary>
    public static HashSet<string> CollectDungeon(int mapId);
}
```

收集器内部是**纯内存字典查找**（配置已加载），每次收集 = 沿引用链走 3-4 层字典查找，微秒级。

### 5.3 配置中需要补充的字段

当前部分配置缺少 IMG 路径引用，需要在翻译/配置层补充：

| 配置 | 缺少什么 | 补充方式 |
|---|---|---|
| AnimClipData JSON | 引用了哪个 .img 文件 | DnfConfigTranslation 翻译 .ani 时提取 `.img` 路径写入 JSON |
| MonsterAiDefinition | 该怪物关联哪些动画 | 已有动画名列表，补充动画名→img 路径映射 |
| SkillDefinition | 技能特效引用哪些 .ani | 补充特效动画名列表（翻译 .skl 时提取） |
| 地图 tile | .til 引用了哪些 .img | DnfConfigTranslation 已有 TilParser，补充 .img 路径提取 |

## 6. 实施步骤

### 实现要求

- **一次写对**：每步写完后在本地做完整自查（lint/语法检查/逻辑走查），尽量一次提交编译后就暴露所有报错，一次解决
- **避免反复修改**：写代码前先确认 API/类型/命名空间，避免"写完→编译→报错→修→再编译→再报错"的循环
- **ET 规范前置遵守**：写码时遵守分析器红线（ET00xx），特别是 HotfixView 无状态、ModelView 不引 TrueSync、ET 子命名空间 Object/Scene 陷阱
- **每步有验收标准**：不达验收标准不进下一步

### 步骤 A：NpkArchive（NPK 挂载与按需提取）

**新文件**：`cn.etetet.npkparser/Runtime/NpkArchive.cs`

从 parse-img-ani 的 `NpkAccess.cs` 改造：
- 保留：魔数验证（`NeoplePack_Bill\0`）、XOR 名称解密（`puchikon@neople...` 密钥）、SHA-256 校验
- **关键改造**：`Read()` 只读 header + 索引表 → 建 `Dictionary<string, (int offset, int length)>`，**不 eagerly 解析任何 IMG**
- 新增 `Extract(string virtualPath) → byte[]`：按索引条目的 offset/length 从字节流取指定条目

**验收**：用用户的 `sprite_monster_bantu.NPK` 测试——挂载后能列出所有条目名，能按名称���取指定条目的字节。

### 步骤 B：NpkMountManager（多 NPK 统一管理）

**新文件**：`cn.etetet.npkparser/Runtime/NpkMountManager.cs`

- `Mount(string name, byte[] npkBytes)` → 挂载归档（调 NpkArchive.Parse）
- `Read(string virtualPath)` → 遍历已挂载归档查找 → NpkArchive.Extract
- `Unmount(string name)` → 移除归档（字节流释放）
- `Contains(string virtualPath)` → 查找是否存在
- 线程安全：mount/read 可能在异步加载中调用

**验收**：挂载 2+ 个 NPK，跨归档查找虚拟路径，正确提取。

### 步骤 C：ResourceScopeManager（作用域管理器）

**新文件**：`cn.etetet.lockstep/Scripts/HotfixView/Client/Resource/ResourceScopeManager.cs`（或独立组件）

核心 API：
```csharp
// 加载作用域（收集依赖 → 逐个从 NPK 提取 → 解析 → 打图集 → 注册）
async ETTask LoadScope(string scopeType, string scopeId)

// 卸载作用域（释放仅属于该作用域的图集）
void UnloadScope(string scopeType, string scopeId)

// 查询某个 IMG 是否已加载
bool IsLoaded(string virtualPath)
```

内部结构：
- `Dictionary<string, LoadedImgInfo>` — 已加载的 IMG（虚拟路径 → 图集 + 归属作用域集合）
- `Dictionary<string, HashSet<string>>` — 作用域 → 该作用域加载的虚拟路径集合

与 NpkMountManager 集成：`LoadScope` 内部调 `ResourceDependencyCollector` 收集路径 → 对每个路径调 `NpkMountManager.Read` → `NpkImgParser.Parse` → `BuildAtlas`。

**验收**：手动构造清单调用 LoadScope/UnloadScope，验证图集生成与释放、共享资源不被误卸。

### 步骤 D：改造 LSAnimResComponentSystem（替换硬编码）

改造 `InitAsync`：
- 删除 26 行硬编码的 `BuildAtlas` 调用
- 改为：加载 NPK 文件（从 Bundles/NPK/ 目录）→ 挂载 → `ResourceScopeManager.LoadScope("character", 职业名)` + `LoadScope("dungeon", mapId)`
- 现有的 `BuildAtlas` / `AtlasLookup` / `AdditiveMaterial` 逻辑不变，只是数据源换了

同时需要：启动时挂载所有需要的 NPK（可从 Bundles/NPK/ 目录扫描，或配置列表指定）。

**验收**：删除全部 .img.bytes 文件后游戏仍正常运行（从 NPK 加载）。

### 步骤 E：生命周期挂接

挂接现有场景事件：

| 事件 | 动作 |
|---|---|
| 登录/选角完成 | `LoadScope("base")` + `LoadScope("character", classId)` |
| TownSceneInitFinish | `LoadScope("town", townId)` |
| 进副本（选图确认/Match） | `UnloadScope("town")` + `LoadScope("dungeon", mapId)` |
| 回城镇（BattleEnd→EnterTown） | `UnloadScope("dungeon")` + `LoadScope("town")` |
| 切角色 | `UnloadScope("character", 旧)` + `LoadScope("character", 新)` |

与 Loading 界面集成：资源加载期间显示 Loading，加载完成才进入场景。

**验收**：完整流程（登录→选角→城镇→副本→回城→再进副本）中，内存中只保留当前活跃作用域的资源。可通过日志打印图集数量验证。

### 步骤 F：清理与收尾

- 删除全部 .img.bytes 文件及其 .meta
- 删除 `LSAnimResComponentSystem.InitAsync` 中的旧加载代码
- 更新 YooAsset 配置（.img.bytes 不再打包，NPK 文件加入打包）
- 更新相关文档

## 7. 技术要点与风险

| 要点 | 说明 |
|---|---|
| **NPK 文件加载方式** | 通过 YooAsset `LoadAssetAsync<TextAsset>` 加载 .npk.bytes（TextAsset 包含完整 NPK 字节）。大 NPK（>100MB）如性能不达标，改为 FileStream + 按需 seek |
| **XOR 解密密钥** | 硬编码 `"puchikon@neople dungeon and fighter DNF..."`（DNF 固定密钥），从 NpkAccess.cs 复制 |
| **SHA-256 校验** | 挂载时验证（防篡改），可选跳过（性能考虑） |
| **图集不 Destroy** | 沿用现有策略：Texture2D 运行时创建的图集在作用域卸载时 `Object.Destroy` 释放 |
| **ET 分析器红线** | NpkArchive/NpkMountManager 放 npkparser Runtime（纯 C#，无 Entity）；ResourceScopeManager 放 lockstep HotfixView（Entity 组件） |
| **配置链缺失** | 部分 .ani JSON 可能没有 imgPath 字段——需要 DnfConfigTranslation 补充翻译，或手动配置映射 |

## 8. 备用方案：离线工具生成资源清单

如果运行时递归收集性能不达标（大量配置对象遍历），退化到离线预计算：

- 写一个离线工具（复用 DnfConfigTranslation），扫描全部 PVF 配置
- 沿依赖链递归收集每个 mapId/characterId/townId 的完整 IMG 路径集合
- 输出 `resource_dependencies.json`
- 运行时直接查 JSON，不做递归

**代价**：配置变更后需重新跑工具生成清单（多一步人为环节，有不同步风险）。
**当前判断**：配置在内存中的字典查找是微秒级，不需要退化到此方案。

## 9. 进度记录

- **08-26** 方案讨论完成（收集规则/作用域体系/NPK 挂载/生命周期），文档创建。
- **08-26** 修正 NPK 存放方式为 .npk.bytes（Unity 兼容）。原始 NPK 源目录确认（18 个文件）。

## 10. 问题 / 待办

- （暂无）
