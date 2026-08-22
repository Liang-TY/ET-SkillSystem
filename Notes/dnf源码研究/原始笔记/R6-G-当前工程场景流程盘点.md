# R6-G-当前工程场景流程盘点

# 当前工程场景/房间流程盘点报告

考察范围：`E:\Projects\cs\et9lockStepYIUITest\Packages\cn.etetet.lockstep`（+ 少量关联包 cn.etetet.login / npkparser / yooassets / excel）。未触碰 pvf 树。

## 1. 登录 → 进入战斗的完整流程与消息流

### SceneType 定义（`Packages\cn.etetet.lockstep\Scripts\Model\Share\SceneType.cs`）

```csharp
public const int RoomRoot = PackageType.LockStep * 1000 + 1;
public const int LockStep = PackageType.LockStep * 1000 + 2;
public const int Match = PackageType.LockStep * 1000 + 3;
public const int Map = PackageType.LockStep * 1000 + 4;
public const int LockStepServer = PackageType.LockStep * 1000 + 5;
public const int LockStepClient = PackageType.LockStep * 1000 + 21;   // 客户端主纤程
```

客户端主纤程是 `SceneType.LockStep`；服务端有 Gate/Match/Map/RoomRoot 多种纤程。

### 完整消息流（按时间序）

```
[客户端启动]
Init.unity → Init.cs (DontDestroyOnLoad, /Global 挂点常驻)
  → EntryEvent3_InitClient (YIUI初始化, PlayerComponent, CurrentScenesComponent...)
  → AppStartInitFinish → YIUI OpenPanelAsync<LoginPanelComponent>          ← 登录面板（YIUI）

[登录]
LoginPanel 点击 → LoginHelper.Login
  → ClientSenderComponent.LoginAsync (新建 NetClient 纤程)
  → C2R_Login(Realm) → R2C_Login(拿Gate地址+Key) → C2G_LoginGate(Gate) → G2C_LoginGate(PlayerId)
  → PlayerComponent.MyId = playerId → publish LoginFinish
  → LoginFinish_CloseLoginPanel + LoginFinish_CreateUILSLobby              ← 进"大厅"（唯一的中间界面）

[匹配]
UILSLobby "EnterMap" 按钮 → EnterMapHelper.Match → C2G_Match (ISessionRequest, 走Gate)
  Gate: C2G_MatchHandler → G2Match_Match(Id=player.Id) → Match纤程
  Match: MatchComponentSystem.Match
      MatchCount==1 攒够即开房 → 随机挑一个 Map 纤程 (StartSceneConfig SceneType.Map)
      → Match2Map_GetRoom (IRequest RPC)
  Map纤程: Match2Map_GetRoomHandler
      → FiberManager.Create(... SceneType.RoomRoot, "RoomRoot")   ← 每次匹配新建一个房间纤程
      → RoomManager2Room_Init(PlayerIds) RPC
         RoomRoot纤程: RoomManager2Room_InitHandler → root.AddComponent<Room>(Name="Server")
             + RoomServerComponent(PlayerIds) + LSWorld(LockStepServer)
      → 返回 Map2Match_GetRoom{ ActorId = roomRootActorId }
  Match → Match2G_NotifyMatchSuccess{ActorId} (LocationType.Player 定位发送)
  Gate: Match2G_NotifyMatchSuccessHandler
      → player.AddComponent<PlayerRoomComponent>().RoomActorId = ActorId   ← 之后 IRoomMessage 按此路由
      → session 转发给客户端

[客户端切场景]
客户端 Match2G_NotifyMatchSuccessHandler → LSSceneChangeHelper.SceneChangeTo(root, "Map1", ActorId.InstanceId)
  ① root.RemoveComponent<Room>() + AddComponentWithId<Room>(sceneInstanceId), Name="Map1"
  ② await publish LSSceneChangeStart → LSSceneChangeStart_AddComponent:
       room.AddComponent<ResourcesLoaderComponent>() + UIComponent
       await UIHelper.Create(room, UIType.UILSRoom)          ← 房间调试HUD(帧号/存录像)
       await LoadSceneAsync("Packages/cn.etetet.demores/Scenes/Game.unity", Single)   ← 写死
       await LSAnimClipRegistrar.RegisterAll(clientScene)    ← 注册动画clip(全局registry)
       await SkillContentLoader.Load(clientScene)            ← 加载技能DLL+skillconfig.json
       LSBulletViewComponent / LSAreaViewComponent / LSCastViewComponent
  ③ Send C2Room_ChangeSceneFinish (IRoomMessage, Gate按PlayerRoomComponent.RoomActorId转给房间)
  ④ await ObjectWait.Wait<Wait_Room2C_Start>()

[房间开战]
RoomRoot: C2Room_ChangeSceneFinishHandler
  roomPlayer.Progress=100 → IsAllPlayerProgress100 → WaitAsync(1000)
  组 Room2C_Start{ StartTime, UnitInfo[]{PlayerId,Position=(0,0,0),Rotation=identity} }
  room.Init(服务器) + AddComponent<LSServerUpdater> → BroadCast(Room2C_Start)

[客户端开战]
Room2C_EnterMapHandler → ObjectWait.Notify(Wait_Room2C_Start)
  → SceneChangeTo 继续: room.LSWorld = new LSWorld(LockStepClient)
     room.Init(UnitInfo, StartTime)          ← 建玩家单位 + 硬编码测试怪
     AddComponent<LSClientUpdater> → publish LSSceneInitFinish
  LSSceneInitFinish_Finish:
     LSUnitViewComponent.InitAsync (LSAnimResComponent图集 + Unit2D.prefab实例化 + 渲染层配置)
     LSCameraComponent + LSOperaComponent → Remove(UILSLobby)

[帧循环]
客户端 LSClientUpdater 每帧: FrameMessage{Frame,PlayerId,LSInput} → Gate(FrameMessage分支) → 房间
服务器 LSServerUpdater: 攒齐(缺的用上帧输入) → OneFrameInputs 广播(Loc2GateSession) + room.Update
客户端 OneFrameInputsHandler → 预测/回滚更新；C2Room_CheckHash / Room2C_AdjustUpdateTime 校时校验
```

另有两条支线：断线重连（G2Room_Reconnect → G2C_Reconnect → `SceneChangeToReconnect`）、本地录像回放（`SceneChangeToReplay`，不经服务器）。

## 2. Room 创建参数：LockStepUnitInfo 来源与 PlayerIds 分配

- **PlayerId = Gate 侧 Player 实体 Id**，一路透传：`C2G_MatchHandler` 里 `g2MatchMatch.Id = player.Id` → 匹配池 → `Match2Map_GetRoom.PlayerIds` → `RoomManager2Room_Init.PlayerIds` → `RoomServerComponent.Awake` 按它 `AddChildWithId<RoomPlayer>(id)`（`Packages\cn.etetet.lockstep\Scripts\Hotfix\Server\Map\RoomServerComponentSystem.cs`）。
- **LockStepUnitInfo 在开战时由服务器现造**（`C2Room_ChangeSceneFinishHandler.cs`）：

```csharp
foreach (RoomPlayer rp in roomServerComponent.Children.Values)
{
    LockStepUnitInfo lockStepUnitInfo = LockStepUnitInfo.Create();
    lockStepUnitInfo.PlayerId = rp.Id;
    lockStepUnitInfo.Position = new TSVector(0, 0, 0);   // 出生点写死原点，无职业/角色字段
    lockStepUnitInfo.Rotation = TSQuaternion.identity;
    room2CStart.UnitInfo.Add(lockStepUnitInfo);
}
```

- proto 定义（`Proto\LockStepOuter_C_11001.proto`）只有 3 个字段：`PlayerId / Position / Rotation`——**没有职业、皮肤、地图、怪物列表等任何扩展位**。
- 客户端收到后 `Room.Init` → `LSUnitFactory.Init`：`AddChildWithId<LSUnit>(unitInfo.PlayerId)`（unit Id 即 PlayerId，输入按此 keyed），且玩家外观全部写死鬼剑士（`LSUnitFactory.cs`）：

```csharp
lsUnit.AddComponent<LSAnimComponent>().Play(AnimId.SwordmanIdle);   // 鬼剑士待机（覆盖默认 bantu Idle）
num.Set(NumericType.HpBase, 1000);
```

视图层同样写死（`LSUnitViewComponentSystem.cs`）：有 `LSInputComponent` = 玩家 → `sm_body0000.img + katana_blade/handle` 三层；否则 = 怪物 → `bantuamazones.img` 单层。**无选角色概念**。
- 匹配人数 `LSConstValue.MatchCount = 1`（`Scripts\Model\Share\LSConstValue.cs`）——单人秒匹配，匹配池里从不排队。

## 3. 多战斗房间/地图选择概念：没有

- **服务器**：支持多房间并发——每次匹配 `Match2Map_GetRoomHandler` 都 `FiberManager.Create(...SceneType.RoomRoot, "RoomRoot")` 新建纤程，一房一纤程。但房间内容完全相同，房间没有“地图 ID”参数。
- **Map 纤程**只是承载开房的调度单位，`StartConfig\Localhost\StartSceneConfig.txt` 配了 2 个，随机挑一个，二者无差异：

```
[304, {..."SceneType":"Match","Name":"Match"...}],
[305, {..."SceneType":"Map","Name":"Map1"...}],
[306, {..."SceneType":"Map","Name":"Map2"...}],
```

- **客户端**：Unity 战斗场景写死为 `Game.unity`（`LSSceneChangeStart_AddComponent.cs` 第 21 行 `LoadSceneAsync($"Packages/cn.etetet.demores/Scenes/{"Game"}.unity", Single)`），场景名写死 `"Map1"`（`G2C_ChangeSceneHandler.cs` 里 `SceneChangeTo(root, "Map1", ...)`，纯展示用字符串）。`demores/Scenes` 下虽有 `Map1.unity`/`Map2.unity`（demo 遗留），没有任何代码引用。
- 全工程 grep `Town|城镇|Dungeon|地下城` 零命中（只有 YIUI 框架内部 `InitOwnerUIEntity` 等误匹配）。UI 只有三个：LoginPanel（YIUI 登录）、UILSLobby（大厅，两个按钮：EnterMap 匹配 / Replay 回放）、UILSRoom（战斗内调试 HUD：帧号显示、存/跳录像）。**流程是 登录→大厅→点按钮直接进唯一战斗场景，中间没有选角色、没有选地图、没有城镇**。

## 4. 刷怪机制：RoomSystem.Init 里的怪物桩是纯写死的代码桩

`Packages\cn.etetet.lockstep\Scripts\Hotfix\Share\RoomSystem.cs` `Init()` 在创建完玩家单位后直接硬编码追加 1 只怪（服务器/客户端共用此代码，确定性一致）：

```csharp
// Half B 测试桩：班图女战士（阶段1 技能轮播驱动；阶段2 换 AI）
// 不进 PlayerIds、不加 LSInputComponent（不是玩家：不吃输入、相机不跟）
LSUnit monster = lsUnitComponent.AddChild<LSUnit>();
monster.Position = new TSVector(3, 0, 0);
monster.Forward = new TSVector(-1, 0, 0);
...
monsterNum.Set(NumericType.HpBase, 500);
...
monster.AddComponent<LSMonsterAIComponent, int>(MonsterAiIds.BantuAmazones);
Log.Info($"[Monster] 测试桩怪物 unit{monster.Id} @ {monster.Position}（AI 驱动）");
```

- 数量(1)、坐标(3,0,0)、朝向、HP(500)、动画、AI（`MonsterAiIds.BantuAmazones = 1`，注释“`.mob` 数据驱动，见 02 文档 §10.2”）全部内联写死。**没有刷怪表/波次/怪物配置结构**——注意区分：AI *行为*是 ET.SkillContent 里的配置类驱动，但“房间里出什么怪、出几只、站哪”这层是硬编码。
- 该方法同时是服务器和客户端的初始化入口（同一 `Room.Init`），做 DNF 地图刷怪时这里就是要把“怪物桩”替换为配置驱动的插入点；且怪物不进 `Room2C_Start.UnitInfo`（不走网络，靠共用 Init 代码保证两端一致）。

## 5. LSAnimResComponent.InitAsync：房间级，每次进房全量重建

- **挂载层级**：`[ComponentOf(typeof(Room))]`（`Scripts\ModelView\Client\LSAnimResComponent.cs`），在 `LSSceneInitFinish` 后由 `LSUnitViewComponentSystem.InitAsync` 添加并调用：

```csharp
LSAnimResComponent animRes = room.AddComponent<LSAnimResComponent>();
await animRes.InitAsync();
```

- **不是全局缓存**：`SceneChangeTo` 开头 `root.RemoveComponent<Room>()` 会销毁旧 Room 及其全部组件，`Destroy` 里只清字典不销毁 Texture2D：

```csharp
private static void Destroy(this LSAnimResComponent self)
{
    self.Atlases.Clear();
    self.AtlasCenters.Clear();
    // Texture2D 运行时创建的图集不 Destroy（场景级共享，泄露量恒定可接受）
}
```

- **切场景=全量重跑**：每次进房 `InitAsync` 重新 LoadAsset + `NpkImgParser.Parse` + RectpackSharp 打包约 25 张 img.bytes（bantuamazones/NormalWave1/AT_Up/sm_body0000/katana_*/bloodboom×4/rwi×4/releasewave 等，路径硬编码清单在 `LSAnimResComponentSystem.cs` 第 32-57 行）。旧图集纹理按注释有意泄漏（“泄露量恒定可接受”），Sprite 字典被清空重建。反过来说：**它也不做按地图裁剪——不管进什么图都是同一份全量清单**。
- 相邻资源的对比（同一时机加载，但作用域不同）：
  - `LSAnimClipRegistrar.RegisterAll` / `SkillContentLoader.Load`（在 `LSSceneChangeStart_AddComponent`，room.Init 之前）写入的是 **静态全局** `AnimConfigRegistry`（`[StaticField]` 字典，npkparser 包）与 `Assembly.Load`——每次进房重跑但幂等覆盖，进程级存活；
  - 它们用的是 **root 级** `ResourcesLoaderComponent`（`EntryEvent3` 添加），句柄跨房保留；而 `LSAnimResComponent.InitAsync` 用的是 **room 级** `ResourcesLoaderComponent`（`LSSceneChangeStart_AddComponent` 添加到 Room），Room 销毁时 `Destroy` 释放全部 YooAsset 句柄（含场景句柄 UnloadAsync，见 `Packages\cn.etetet.yooassets\Scripts\HotfixView\Client\ResourcesLoaderComponentSystem.cs`）。
  - 表现层挂点 `/Global/Unit`、`/Global/UI` 来自 `Init.unity`（`Init.cs` 的 `DontDestroyOnLoad(gameObject)` + `GlobalComponent.Awake` 的 `GameObject.Find("/Global/...")`），跨 Unity 场景常驻；`Game.unity` 本体只是一个 SunshineForest 背景预制 + 相机，无逻辑对象。

## 关键结论（对新流程的落点提示）

1. 房间生命周期/消息链已具备（Match→Map→RoomRoot 纤程→ChangeScene 握手→Room2C_Start），但**从匹配到战斗之间没有任何“内容选择”参数通道**：`Match2Map_GetRoom`、`RoomManager2Room_Init`、`Room2C_Start.LockStepUnitInfo` 都需要加字段才能承载地图ID/角色信息。
2. 玩家与怪物均硬编码（鬼剑士/1只班图女战士），出生点 (0,0,0)/(3,0,0)；无角色、无职业、无地图差异。
3. 战斗 Unity 场景写死 `Game.unity`；要做“选地图→不同战斗场景/不同怪物/不同图集”，需把 `LSSceneChangeStart_AddComponent` 的场景路径、`LSAnimResComponent` 的图集清单、`LSAnimClipRegistrar` 的 clip 清单、`RoomSystem.Init` 的怪物桩全部参数化（目前四者均为硬编码清单）。