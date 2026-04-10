# ET 框架资源加载与配置系统

## 一、资源加载管线

ET 使用 **YooAsset** 作为资源管理方案，封装为两层：

### 1.1 底层：ResourcesComponent（单例）

路径：`Packages/cn.etetet.yooassets/Runtime/ET/ResourcesComponent.cs`

- 框架级单例，用于加载 DLL、Config、AOT DLL 等启动时资源
- 此时 Fiber 还没创建，无法使用 Entity 体系
- 内部直接调用 `YooAssets.LoadAssetAsync<T>(location)`

```csharp
// 加载单个资源
public async ETTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object

// 加载某路径下所有资源
public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
```

### 1.2 业务层：ResourcesLoaderComponent（Entity 组件）

路径：`Packages/cn.etetet.yooassets/Scripts/ModelView/Client/ResourcesLoaderComponent.cs`

- 挂在 Entity 上的组件，生命周期随 Parent
- **游戏运行时资源加载应该用这个**，不用 ResourcesComponent
- 自动管理 Handle 引用计数，Destroy 时统一 Release

```csharp
// 加载单个资源（带协程锁防重入）
public async ETTask<T> LoadAssetAsync<T>(this ResourcesLoaderComponent self, string location)

// 加载所有子资源
public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(this ResourcesLoaderComponent self, string location)

// 加载场景
public async ETTask LoadSceneAsync(this ResourcesLoaderComponent self, string location, LoadSceneMode loadSceneMode)
```

使用方式（参照现有代码）：
```csharp
Room room = ...;
var resourcesLoader = room.GetComponent<ResourcesLoaderComponent>();
GameObject go = await resourcesLoader.LoadAssetAsync<GameObject>("Packages/cn.etetet.demores/Bundles/Unit/Unit.prefab");
```

### 1.3 资源路径规则

- 资源放在 Package 的 `Bundles/` 目录下
- 加载路径格式：`Packages/{包名}/Bundles/{相对路径}`
- Editor 模拟模式下，YooAsset 直接读取包目录文件（EditorSimulateMode）
- 真机构建时走 AssetBundle

现有资源分布：
```
cn.etetet.demores/
├── Bundles/
│   └── Unit/
│       └── Unit.prefab          → 加载路径: "Packages/cn.etetet.demores/Bundles/Unit/Unit.prefab"
└── Unit/Skeleton/
    ├── Skeleton.prefab          → Unit.prefab 内引用的子资源
    ├── SkeletonController.controller
    └── Ani/*.FBX
```

### 1.4 GlobalComponent

路径：`Packages/cn.etetet.loader/Scripts/ModelView/Client/GlobalComponent.cs`

场景中的全局容器，在 Awake 时通过 `GameObject.Find` 获取：
```csharp
self.Global = GameObject.Find("/Global").transform;
self.Unit   = GameObject.Find("/Global/Unit").transform;   // 角色挂载容器
self.UI     = GameObject.Find("/Global/UI").transform;
```

实例化角色时，挂到 `GlobalComponent.Unit` 下：
```csharp
GameObject unitGo = Object.Instantiate(prefab, globalComponent.Unit, true);
```

---

## 二、配置表系统

### 2.1 整体流程

```
Excel (.xlsx)
    ↓ ExcelExporter 工具
C# 代码 (ConfigCategory + Config 类)
    ↓ 编译 + MongoDB BSON 序列化
.bytes 文件
    ↓ ConfigLoader 运行时加载
Singleton 注册到 World
    ↓ 代码中访问
XxxConfigCategory.Instance.Get(id)
```

### 2.2 Excel 源文件

放在各 Package 的 `Excel/` 目录下：
- `AnimConfig@cs.xlsx` → ClientServer 共用
- `UnitConfig@c.xlsx` → Client 独有
- `UnitConfig@s.xlsx` → Server 独有

### 2.3 生成的 C# 代码

通过 Unity 菜单 `ET/Excel/ExcelExporter` 生成，输出到：
```
cn.etetet.excel/CodeMode/Model/Client/UnitConfig.cs
cn.etetet.excel/CodeMode/Model/Server/UnitConfig.cs
cn.etetet.excel/CodeMode/Model/ClientServer/UnitConfig.cs
```

生成的代码模式（以 UnitConfig 为例）：
```csharp
[Config]  // ← 关键特性，ConfigLoader 扫描此特性发现所有配置
public partial class UnitConfigCategory : Singleton<UnitConfigCategory>, IMerge
{
    [BsonElement]
    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
    private Dictionary<int, UnitConfig> dict = new();

    public void Merge(object o) { ... }

    public UnitConfig Get(int id) { ... }
    public bool Contain(int id) { ... }
    public Dictionary<int, UnitConfig> GetAll() { ... }
    public UnitConfig GetOne() { ... }
}

public partial class UnitConfig : ProtoObject, IConfig
{
    public int Id { get; set; }       // IConfig 要求必须有 Id
    public int Type { get; set; }
    public string Name { get; set; }
    // ...
}
```

### 2.4 生成的数据文件

`.bytes` 文件（BSON 格式）：
```
cn.etetet.excel/Config/Bytes/
├── c/   UnitConfigCategory.bytes      ← Client 配置
├── s/   UnitConfigCategory.bytes      ← Server 配置
└── cs/
    ├── UnitConfigCategory.bytes       ← ClientServer 共用
    └── StartConfig/
        ├── Example/   Start*.bytes
        ├── Localhost/ Start*.bytes
        └── Release/   Start*.bytes
```

中间 `.txt` 文件（JSON 格式，用于调试）：
```
cn.etetet.excel/Config/Json/cs/UnitConfig.txt
```

### 2.5 运行时加载

**ConfigLoader**（`cn.etetet.excel/Scripts/Model/Share/ConfigLoader.cs`）：
```csharp
public async ETTask LoadAsync()
{
    // 1. 触发事件 → 获取所有配置的 byte[]
    Dictionary<Type, byte[]> configBytes = await EventSystem.Instance
        .Invoke<GetAllConfigBytes, ETTask<Dictionary<Type, byte[]>>>(new GetAllConfigBytes());

    // 2. 逐个反序列化 + 注册为单例
    foreach (Type type in configBytes.Keys)
        LoadOneConfig(type, configBytes[type]);

    // 3. 后处理
    ConfigProcess();
}

private static void LoadOneConfig(Type configType, byte[] oneConfigBytes)
{
    object category = MongoHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
    World.Instance.AddSingleton((ASingleton)category);
}
```

### 2.6 客户端 ConfigLoaderInvoker

路径：`cn.etetet.lockstep/Scripts/HotfixView/Client/ConfigLoaderInvoker.cs`

```csharp
// Editor 模式：直接读文件系统
if (Define.IsEditor)
{
    // 根据 GlobalConfig.CodeMode 决定读 c/s/cs 目录
    configFilePath = $"{ExcelPackagePath}/Config/Bytes/{ct}/{configType.Name}.bytes";
    output[configType] = File.ReadAllBytes(configFilePath);
}
// 真机模式：通过 YooAsset 加载
else
{
    TextAsset v = await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>(
        $"{ExcelPackagePath}/Config/Bytes/c/{type.Name}.bytes");
    output[type] = v.bytes;
}
```

### 2.7 服务器 ConfigLoaderInvoker

路径：`cn.etetet.lockstep/Scripts/Hotfix/Server/ConfigLoaderInvoker.cs`

只走文件系统：
```csharp
configFilePath = Path.Combine($"{ExcelPackagePath}/Config/Bytes/s/{configType.Name}.bytes");
output[configType] = File.ReadAllBytes(configFilePath);
```

`ExcelPackagePath` 在 LSConstValue 中定义为 `"./Packages/cn.etetet.excel"`。

### 2.8 如何新增配置

1. 在 Package 的 `Excel/` 下创建 `XxxConfig@cs.xlsx`
2. 运行 `ET/Excel/ExcelExporter`
3. 自动生成：C# 代码 + .bytes + .txt
4. 代码中访问：`XxxConfigCategory.Instance.Get(id)`

**不走 Excel 导表的替代方案**（适用于外部 json 等非策划配置）：
- 直接用 `ResourcesLoaderComponent.LoadAssetAsync<TextAsset>()` 加载
- 自行解析 json 注册到全局字典
- 本次 2D 动画的 AnimClipData 就采用这种方式

---

## 三、关键配置文件

| 文件 | 路径 | 说明 |
|------|------|------|
| GlobalConfig | Assets/Resources/GlobalConfig | Unity Resources.Load 加载，CodeMode 设置 |
| YooConfig | Assets/Resources/YooConfig | YooAsset 运行模式配置 |
| StartConfig | Config/Bytes/s/StartConfig/{name}/ | 服务器启动配置 |

---

## 四、Package 依赖关系速查

| 包 | 职责 | 关键类型 |
|----|------|----------|
| cn.etetet.core | 框架核心 | Entity, Fiber, EventSystem, World |
| cn.etetet.lsentity | 帧同步实体 | LSEntity, LSWorld, LSUpdater, ILSUpdate |
| cn.etetet.truesync | 确定性数学 | TSVector, FP, TSQuaternion, TSRandom |
| cn.etetet.memorypack | 序列化 | MemoryPack 序列化器 |
| cn.etetet.yooassets | 资源管理 | ResourcesComponent, ResourcesLoaderComponent |
| cn.etetet.loader | 全局组件 | GlobalComponent (Unit/UI 容器) |
| cn.etetet.excel | 配置表 | ConfigLoader, [Config], IConfig, IMerge |
| cn.etetet.lockstep | 帧同步业务 | Room, LSUnit, LSInput, FrameBuffer |
| cn.etetet.demores | Demo 资源 | 场景、角色 Prefab、动画资源 |
