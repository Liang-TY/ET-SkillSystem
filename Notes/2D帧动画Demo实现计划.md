# 2D 帧动画帧同步 Demo — 实现计划

## 目标

在现有 ET 帧同步框架上，替换原有 3D Skeleton（Animator）角色为 2D 精灵动画角色，使用 `bantuamazones.img` 资源，`stay.json` 作为 idle 动画，`move.json` 作为移动动画。

---

## 一、资源放置与加载方式

### 资源位置

ET 框架运行时通过 **YooAsset** 加载资源（`ResourcesLoaderComponent.LoadAssetAsync<T>(location)`）。在 Editor 模拟模式下，YooAsset 直接读取包目录下的文件。参照现有 Unit.prefab 的路径 `Packages/cn.etetet.demores/Bundles/Unit/Unit.prefab`，资源应放在 `cn.etetet.demores` 包的 Bundles 目录下。

**资源放置**：
```
Packages/cn.etetet.demores/Bundles/
├── Unit/
│   └── Unit.prefab          ← 已有，需要修改（见第七节）
└── AnimRes/                 ← 新建目录
    ├── bantuamazones.img    ← 精灵图集
    ├── stay.json            ← idle 动画配置
    └── move.json            ← walk 动画配置
```

### 加载方式

视图层通过 `ResourcesLoaderComponent.LoadAssetAsync<TextAsset>(location)` 加载：
- img 文件作为 `TextAsset` 加载 → `textAsset.bytes` 获取原始字节 → 传入 NpkImgParser 解析
- json 文件作为 `TextAsset` 加载 → `textAsset.text` 获取文本 → 解析为 AnimClipData

加载路径格式：
```csharp
string assetsName = "Packages/cn.etetet.demores/Bundles/AnimRes/bantuamazones.img";
TextAsset imgAsset = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<TextAsset>(assetsName);
```

参照 ET 配置表加载模式（`ConfigLoader` + BSON `.bytes`），动画配置不走 Excel 导表流程（因为是外部 json），直接作为 TextAsset 加载解析。

---

## 二、IMG 解析模块（纯 C#，放在 lockstep package 中）

**新建文件**：`Scripts/Model/Share/NpkImgParser.cs`

精简移植 NpkApi 中 **v2 + v4** 版本的解析逻辑：

### 支持的 IMG 版本

| 版本 | 像素格式 | 说明 |
|------|----------|------|
| v2 | ARGB1555/4444/8888 | 直接色彩模式，bantuamazones.img 可能是此版本 |
| v4 | Indexed + 单调色板 | 索引色模式，需额外读取调色板后查表解码 |

### v2 解码流程
1. 读取文件头：magic(16B) + tableLength(4B) + skip(4B) + version(4B) + frameCount(4B)
2. 遍历帧目录：type(4B) + compressedFlag(4B) + width/height/length/x/y/frameWidth/frameHeight(各4B)
3. 读取每帧原始字节 → 若 compressed 则 ZLIB 解压 → 按 type 解码为 `int[]` ARGB 数据
4. ARGB1555: 2B/pixel, ARGB4444: 2B/pixel, ARGB8888: 4B/pixel

### v4 解码流程（在 v2 基础上增加）
1. 文件头读取后，额外读取：colorNum(4B) + paletteData(4B * colorNum)，格式为 R,G,B,A 每色 4 字节
2. 帧目录与 v2 相同
3. 帧数据解码时，每个字节是调色板索引 → 查 palette 得到 ARGB 颜色

### 输出结构

```csharp
public class NpkSprite
{
    public int Index;
    public int Width, Height;
    public int X, Y;             // 帧在图集中的偏移
    public int FrameWidth, FrameHeight;
    public int[] ArgbData;       // ARGB 像素数据
}
```

### 不支持
- v5（DDS 纹理）、v6（多调色板）— 本 demo 不需要
- 写入功能 — 只需要读取

### ZLIB 解压
使用 `System.IO.Compression.DeflateStream`，跳过 ZLIB 头 2 字节后解压（与 NpkApi 中 CompressionHelper 一致）。

---

## 三、动画配置数据结构

**新建文件**：`Scripts/Model/Share/AnimClipData.cs`

对应 stay.json / move.json 的解析结构。由于 `JsonUtility` 不支持 snake_case 字段名，使用 `System.Text.Json` 或手动构造可序列化结构：

```csharp
public class AnimClipData
{
    public bool Loop;
    public int FrameMax;
    public List<AnimFrameData> Frames;
    public int TotalDuration;
}

public class AnimFrameData
{
    public int Index;
    public int ImgIndex;       // image.index → img 文件中的精灵索引
    public int ImagePosX;      // imagePos.x
    public int ImagePosY;      // imagePos.y
    public int Delay;           // 持续时间 ms
}
```

stay.json 结构参考：
- 1 帧，index=0, imgIndex=0, delay=80ms, loop=false
- totalDuration=80

move.json 结构参考：
- 6 帧，index=0..5, imgIndex=9..14, delay=60/60/80/60/60/80ms, loop=true
- totalDuration=400

---

## 四、逻辑层 — LSAnimComponent（帧同步确定性动画）

**新建文件**：`Scripts/Model/Share/LSAnimComponent.cs`

按照 `2D帧动画游戏实现方案-持续时间版.md` 实现：

```csharp
[ComponentOf(typeof(LSUnit))]
[MemoryPackable]
public partial class LSAnimComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
{
    [MemoryPackOrder(0)] public int AnimId;        // 0=None, 1=Idle, 2=Walk
    [MemoryPackOrder(1)] public int FrameIndex;
    [MemoryPackOrder(2)] public long FrameTick;    // 毫秒累积器
    [MemoryPackOrder(3)] public FP Speed;          // 播放速度倍率
    [MemoryPackOrder(4)] public bool IsLoop;
    [MemoryPackOrder(5)] public bool IsFinished;
}
```

**新建文件**：`Scripts/Hotfix/Share/LSAnimComponentSystem.cs`

```csharp
// LSUpdate 核心逻辑
self.FrameTick += (long)(LSConstValue.UpdateInterval * self.Speed);
AnimFrameData frame = config.Frames[self.FrameIndex];
while (self.FrameTick >= frame.Delay) {
    self.FrameTick -= frame.Delay;  // 保留余数
    self.FrameIndex++;
    if (self.FrameIndex >= config.Frames.Count) {
        if (self.IsLoop) self.FrameIndex = 0;
        else { self.IsFinished = true; return; }
    }
    frame = config.Frames[self.FrameIndex];
}
```

AnimId 常量：
```csharp
public static class AnimId
{
    public const int None = 0;
    public const int Idle = 1;   // → stay.json
    public const int Walk = 2;   // → move.json
}
```

---

## 五、动画配置注册

**新建文件**：`Scripts/Hotfix/Share/AnimConfigRegistry.cs`

将 animId 映射到 AnimClipData，供 LSAnimComponentSystem 查询：

```csharp
public static class AnimConfigRegistry
{
    private static readonly Dictionary<int, AnimClipData> configs = new();

    public static void Register(int animId, AnimClipData data) { configs[animId] = data; }
    public static AnimClipData Get(int animId) { return configs[animId]; }
}
```

初始化时机：在视图层资源加载完成后（`NpkSpriteCache` 初始化时一起做），解析 json 并注册。

---

## 六、LSUnitFactory 修改

在 `LSUnitFactory.Init()` 中，创建 LSUnit 后为其添加 `LSAnimComponent`，默认播放 Idle：

```csharp
var animComp = lsUnit.AddComponent<LSAnimComponent>();
// AnimId=1(Idle) 在 LSAnimComponentSystem.LSUpdate 中查询 AnimConfigRegistry
```

注意：不在 Factory 中调用 `Play()`，因为 AnimConfigRegistry 此时可能还未初始化。Play 逻辑由视图层触发，或者 LSAnimComponent.Awake 中设默认 AnimId=Idle。

---

## 七、视图层 — 修改 Unit.prefab + 2D 精灵渲染

### 7.1 修改 Unit.prefab（手动操作，在 Unity Editor 中）

当前的 `Skeleton.prefab` 是 3D 模型 + Animator。需要修改为 2D 用法：

1. 打开 `Packages/cn.etetet.demores/Unit/Skeleton/Skeleton.prefab`
2. **删除** Skeleton 的 SkinnedMeshRenderer 和 Animator 组件（或禁用）
3. **添加** `SpriteRenderer` 组件
4. SpriteRenderer 的 Sprite 留空（运行时由代码设置）
5. 确保 prefab 根对象有一个空 GameObject 结构即可
6. 或者更简单：**新建一个空 prefab**，只挂 SpriteRenderer，替换原来的 Skeleton.prefab 路径

`LSUnitViewComponentSystem.InitAsync` 仍然加载同一个 prefab 路径：
```csharp
string assetsName = "Packages/cn.etetet.demores/Bundles/Unit/Unit.prefab";
```
只是 prefab 的内容从 3D Skeleton 变成了带 SpriteRenderer 的 2D 对象。

### 7.2 精灵图集缓存

**新建文件**：`Scripts/HotfixView/Client/NpkSpriteCache.cs`

- 通过 `ResourcesLoaderComponent.LoadAssetAsync<TextAsset>` 加载 img 文件
- 用 NpkImgParser 解析出所有 NpkSprite
- 将每个 sprite 的 `int[] ArgbData` 转为 Unity `Texture2D` → `Sprite`
- 缓存 `Dictionary<int, Sprite>` (imgIndex → Sprite)
- 同时加载 stay.json / move.json → 解析为 AnimClipData → 注册到 AnimConfigRegistry
- 挂载到 Room 上作为组件，生命周期跟随 Room

### 7.3 新增 LSSpriteAnimViewComponent

**新建文件**：`Scripts/ModelView/Client/LSSpriteAnimViewComponent.cs`

```csharp
[ComponentOf(typeof(LSUnitView))]
public class LSSpriteAnimViewComponent : Entity, IAwake, IUpdate, IDestroy
{
    public SpriteRenderer SpriteRenderer;
    public int LastFrameIndex = -1;  // 用于检测帧变化
}
```

**新建文件**：`Scripts/HotfixView/Client/LSSpriteAnimViewComponentSystem.cs`

Update 逻辑：
1. 获取 LSUnit 的 LSAnimComponent
2. 若 FrameIndex 变化 → 查询 AnimConfigRegistry 获取当前帧的 imgIndex 和 imagePos
3. 从 NpkSpriteCache 获取 Sprite → 设置到 SpriteRenderer
4. 设置 GameObject 的 localPosition 偏移（imagePos 是负值，表示精灵相对于角色锚点的偏移）

### 7.4 修改 LSUnitViewComponentSystem.InitAsync

原来：
```csharp
LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
lsUnitView.AddComponent<LSAnimatorComponent>();
```

改为：
```csharp
LSUnitView lsUnitView = self.AddChildWithId<LSUnitView, GameObject>(lsUnit.Id, unitGo);
lsUnitView.AddComponent<LSSpriteAnimViewComponent>();  // 替换 LSAnimatorComponent
```

### 7.5 修改 LSUnitViewSystem.Update

原来通过 `LSAnimatorComponent.SetFloatValue("Speed", ...)` 控制动画，改为：

```csharp
LSInput input = unit.GetComponent<LSInputComponent>().LSInput;
var animComp = unit.GetComponent<LSAnimComponent>();
if (input.V != TSVector2.zero)
{
    if (animComp.AnimId != AnimId.Walk)
        animComp.Play(AnimId.Walk, FP.One, true);
}
else
{
    if (animComp.AnimId != AnimId.Idle)
        animComp.Play(AnimId.Idle, FP.One, false);
}
```

---

## 八、文件清单汇总

| 文件（相对 lockstep 包根目录） | 类型 | 说明 |
|------|------|------|
| `Packages/cn.etetet.demores/Bundles/AnimRes/bantuamazones.img` | 新增资源 | 精灵图集 |
| `Packages/cn.etetet.demores/Bundles/AnimRes/stay.json` | 新增资源 | idle 动画配置 |
| `Packages/cn.etetet.demores/Bundles/AnimRes/move.json` | 新增资源 | walk 动画配置 |
| `Packages/cn.etetet.demores/Unit/Skeleton/Skeleton.prefab` | 修改 | 改为 SpriteRenderer |
| `Scripts/Model/Share/NpkImgParser.cs` | 新建 | IMG 文件解析（v2 + v4） |
| `Scripts/Model/Share/AnimClipData.cs` | 新建 | 动画配置数据结构 |
| `Scripts/Model/Share/LSAnimComponent.cs` | 新建 | 帧同步动画逻辑组件 |
| `Scripts/Hotfix/Share/LSAnimComponentSystem.cs` | 新建 | 动画逻辑 System（累积器） |
| `Scripts/Hotfix/Share/AnimConfigRegistry.cs` | 新建 | 动画配置注册表 |
| `Scripts/ModelView/Client/LSSpriteAnimViewComponent.cs` | 新建 | 2D 精灵动画视图组件 |
| `Scripts/HotfixView/Client/LSSpriteAnimViewComponentSystem.cs` | 新建 | 视图渲染 System |
| `Scripts/HotfixView/Client/NpkSpriteCache.cs` | 新建 | 精灵图集缓存 + 配置加载 |
| `Scripts/Hotfix/Share/LSUnitFactory.cs` | 修改 | 添加 LSAnimComponent |
| `Scripts/HotfixView/Client/LSUnitViewComponentSystem.cs` | 修改 | 改挂 LSSpriteAnimViewComponent |
| `Scripts/HotfixView/Client/LSUnitViewSystem.cs` | 修改 | 动画切换逻辑 |

---

## 九、执行顺序

1. 复制资源文件到 `cn.etetet.demores/Bundles/AnimRes/`
2. 实现 NpkImgParser（v2 + v4 解析）
3. 实现 AnimClipData + AnimConfigRegistry
4. 实现 LSAnimComponent + LSAnimComponentSystem（帧同步确定性动画）
5. 修改 LSUnitFactory（添加 LSAnimComponent）
6. 实现 NpkSpriteCache（精灵缓存 + json 配置加载注册）
7. 实现 LSSpriteAnimViewComponent + System（视图层）
8. 修改 LSUnitViewComponentSystem + LSUnitViewSystem（接入 2D 渲染）
9. 在 Unity Editor 中修改 Skeleton.prefab（改为 SpriteRenderer）
10. 测试运行

---

## 十、注意事项

- **LSAnimComponent 必须是 LSEntity**，支持回滚和序列化（MemoryPack）
- **NpkImgParser 和 AnimClipData 是纯数据**，不挂在 Entity 上，作为静态工具/配置使用
- **视图层（Sprite 渲染）不影响帧同步确定性**，只读取 LSAnimComponent 的状态做表现
- **资源加载走 YooAsset 管线**：通过 `ResourcesLoaderComponent.LoadAssetAsync<TextAsset>()` 加载，不走 `Resources.Load`
- **IMG 版本自动检测**：NpkImgParser 读取文件头 version 字段后分派到 v2 或 v4 解码逻辑
- bantuamazones.img 中 index 0 是站立帧，index 9-14 是行走帧，与 json 配置对应
- imagePos 是偏移量（负值），需要在渲染时作为精灵锚点偏移处理
- 原有 LSAnimatorComponent（3D Animator）保留不删，新代码用 LSSpriteAnimViewComponent 并行
