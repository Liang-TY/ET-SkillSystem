# DNF 风格 2D 美术资源与帧动画系统方案

> 适用工程：`et9lockStepYIUITest`（ET9 帧同步 + YIUI + Unity URP）
> 目标：用 Neople IMG（`.img.bytes`）压缩序列帧 + JSON 动作配置，做出 DNF 风格的 2D 横版逐帧动画，能创建物并播放走 / 跑 / 攻击。
> 本文结论已对照本仓库实际代码 + Unity 官方文档做了对抗式验证。

---

## 会话恢复速查（新会话先读这段，再按需看下面章节）

- **项目**：`et9lockStepYIUITest`（ET9 帧同步 + YIUI + Unity URP），改成 DNF 风格 2D 横版格斗。美术 = Neople IMG（`.img.bytes`）压缩序列帧 + 每动作 JSON 配置（帧列表 / per-frame delay / damageBox）。
- **已定美术方案（方式 1b）**：`.img.bytes` 进包 → 运行时解码一次 → RectpackSharp 打成**单张运行时图集**（按实际 `bounds` 建大小，别固定 2048）→ `Sprite.Create` 每帧为子区域。不用 SpriteAtlas（构建期专用，喂不进运行时像素）；不用一帧一 Texture2D（破合批）。详见 §3 Q1/Q2、§4.4。
- **已定动画方案**：自写帧动画组件（Mecanim 运行时无法数据驱动，已证，§3 Q3）。**逻辑层** `LSAnimComponent`（AnimId/FrameIndex/FrameTick，ILSUpdate 按 ms 累加器推进帧，余数保留无漂移）+ **视图层** `LSSpriteAnimViewComponent`（diff 到帧变就换 SpriteRenderer.sprite）。动画组件当哑播放器，状态流转交给 AI 行为 FSM（§13）。
- **代码组织**：动画 ECS 写在 `cn.etetet.lockstep` 包的 `LSAnim/` 文件夹（**不单独建包**，与 LSUnit 双向耦合无真边界，§10）；`npkparser` 保持独立（真边界，纯 C# 解析+数据）；RectpackSharp 源码拷进 npkparser/Runtime。
- **当前进度**（§12 详）：解析器(npkparser)✅；`LSAnimResComponent` 半成品且写法错（每帧一 Texture2D，待重写成图集）；**整个动画逻辑层 + 2D 视图层还没开始**；4 个已知 bug 见 §1。目前能进战斗场景。
- **下一步**：跑通最小闭环 §12.2 的 1-5 步（2D prefab + 修 SpriteRenderer 赋值 + 图集打包 + LSAnimComponent + 视图换帧 + Play(Walk)），让怪在场景里循环走路。代码骨架在 §4。
- **要动手实现时，除本文档外再读这几个文件**：`Packages/cn.etetet.npkparser/Runtime/{NpkImgParser,AnimClipData,AnimConfigRegistry}.cs`、`Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/{LSAnimResComponentSystem,LSUnitViewSystem,LSUnitViewComponentSystem}.cs`、`Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSUnitView.cs`。
- **可继续讨论的方向**：落地实现步骤 1-5、怪物刷怪管理、寻路、伤害公式、攻击框多目标命中规则、投射物、buff……

---

## 0. 一句话结论

- **美术**：`.img.bytes` 当可热更小包发；进房间一次性 `NpkImgParser.Parse` 解码，再把一组 IMG 的所有帧用 **RectpackSharp 打包到一张运行时图集 `Texture2D`**（按实际 `bounds` 建大小，不浪费内存），每帧 `Sprite.Create` 为这张图的子区域。**不要一帧一个 Texture2D**（当前写法，破坏合批）。
- **动画**：Unity 的 Mecanim **无法在运行时从数据生成 sprite-swap 动画**（架构死路，详见 Q3）。正统做法是**自己写一个极简帧动画组件**，JSON 驱动 `SpriteRenderer.sprite` 换帧。帧状态放逻辑层（确定性 / 可回滚），视图层只读换图。
- **正统性**：游戏 **UI 拼界面 = 构建期 SpriteAtlas**（你的 YIUI 本就走这条路）；**像素序列帧**默认也是构建期导入（美术给标准 PNG 时），但 **IMG 私有格式 + 要热更小包 = 运行时打包图集（1b）是合理特例**。两者在工程里并存，各管各的。

---

## 1. 当前代码的 4 个真 bug（会直接挡住你）

| # | 位置 | 问题 | 后果 |
|---|------|------|------|
| 1 | `LSUnitViewComponentSystem.cs:33-35` | 硬编码 `demores/Bundles/Unit/Unit.prefab` + `Get<GameObject>("Knight")`，这俩在仓库里**都不存在** | 进战斗场景必崩 / 看不到角色 |
| 2 | `LSUnitView.cs:14` + `Awake` | 声明了 `public SpriteRenderer SpriteRenderer;` **但从未赋值** | `sortingOrder`（第 78 行）静默失效，换帧组件会空引用 |
| 3 | `AnimClipData.cs` 的 `AnimFrameData` | 没有 `damageBox` 字段，但 `stay.json` / `move.json` 里**有** `damageBox` | `JsonUtility` 静默丢弃，碰撞盒永远拿不到 |
| 4 | `LSAnimResComponentSystem.cs:43-68` | 每帧一个 `new Texture2D` + `SetPixels` | 每帧 = 不同纹理 = 不同 draw call 批次键，**合批全坏** |

---

## 2. 资源格式与解析回顾

### 2.1 美术资源

- `bantuamazones.img.bytes`：Neople IMG 格式，一个怪物多个动作的序列帧压缩包。
  - `NpkImgParser.Parse(byte[])` → `NpkSprite[]`，每个 sprite 含 `Index / Width / Height / X / Y / FrameWidth / FrameHeight / ArgbData(int[])`。
  - 支持 IMG v2（ARGB1555/4444/8888）、v4（索引色 + 调色板）、zlib 压缩帧、引用帧。

### 2.2 动作配（AnimClipData）

```json
{
  "loop": true, "frameMax": 6,
  "frames": [
    {
      "index": 0,
      "image": { "path": "BantuAmazones.img", "index": 9 },
      "imagePos": { "x": -249, "y": -301 },
      "delay": 60,
      "damageBox": { "min": { "x": -16, "y": -7, "z": -5 }, "max": { "x": 29, "y": 14, "z": 106 } },
      "damageType": null, "graphicEffect": null, "playSound": null, "setFlag": null
    }
  ],
  "totalDuration": 400
}
```

- `image.index`：引用 IMG 里第几张 sprite —— **这是贯穿全程的唯一 key**。
- `imagePos`：渲染锚点偏移（像素，100ppu）。
- `delay`：该帧持续毫秒。
- `damageBox`：每帧受击 / 攻击盒。**当前 C# 结构体没接收，被 JsonUtility 静默丢弃。**

---

## 3. 逐一回答四个问题

### Q1：在 Unity 中开发 2D 像素游戏，正统使用这些美术资源的方式？

> **先澄清一个最容易混的概念：Unity 里"图集"有两种，运行时能力完全不同。**

| | Unity 官方 `SpriteAtlas` 资源 | 手搓大纹理 `Texture2D` + 多个 `Sprite` 子区域 |
|---|---|---|
| 打包时机 | **仅 build 时**（编辑器里配，打包时拼） | 运行时也能拼 |
| 能喂"运行时解码出来的像素"吗 | ❌ **不能** | ✅ 能 |
| 发布后能新建 / 重新打包吗 | ❌ 只能 Late Bind 加载已打好的 | ✅ 随时建 |
| 编辑器工作流（文件夹规则、变体图集、压缩选项） | ✅ 丰富 | ❌ 没有 |
| 构建期 ASTC/ETC 压缩 | ✅ | ❌ 只能 RGBA32 |
| **运行时最终产物** | `Sprite` 引用一张 `Texture2D` 的子区域 | **完全一样** |

**结论**：Unity 不支持"运行时新建官方 SpriteAtlas 并打包"；但**完全支持"运行时用 `Texture2D` + `Sprite.Create` 手搓一张图集"**，发布后也能用，合批效果一样（dynamic sprite batcher 按底层 `Texture2D` 引用分批，不管它是 build 时还是运行时生成的）。本方案用的就是后者。

具体要点：

- **合批前提**：共享一张 `Texture2D` + 同一材质 → 同屏 N 个怪 N 帧**合为 1 个 draw call**。
- **零拷贝填充**：`GetRawTextureData<Color32>()` 拿 `NativeArray<Color32>`（非托管内存，写它不产生 GC），ARGB int 拆成 R,G,B,A 字节写入，再 `Apply(false, makeNoLongerReadable:true)` 上传 GPU 并释放 CPU 拷贝。
- **TextureFormat 必须 `RGBA32`**：`Color32` 内存布局是 R,G,B,A，只有 `RGBA32` 匹配；`ARGB32` 会通道错位。
- **像素图纹理设置**：`filterMode = FilterMode.Point`（防双线性糊像素）；`wrapMode = TextureWrapMode.Clamp`（图集里 Repeat 会让 UV 精度溢出串到相邻帧，产生接缝渗色）；帧间留 **1px padding**。
- **加载**用 YooAsset：`ResourcesLoaderComponent.LoadAssetAsync<TextAsset>`。`.img.bytes` / `.json` 作为 TextAsset 走 AssetBundle。
- 本工程用 URP，但 **SRP Batcher 不支持 SpriteRenderer**——走老的 dynamic sprite batcher，不影响合批结论。

### Q2：资源导入方式 —— 运行时按需解压 vs 全部搞成大图预导入？

**核心：把"解码 + 拼图集"放在运行时还是构建期，是真正的分水岭。** 有四条路：

| 方式 | 文件怎么进 Unity | 谁拼图集 | 合批 | 美术热更 | 压缩 |
|------|------------------|----------|------|----------|------|
| **1a 当前代码** | `.img.bytes` 运行时解码 | 不拼，每帧一个 Texture2D | ❌ 全坏 | ✅ 换小 bytes | ❌ |
| **1b 推荐** | `.img.bytes` 运行时解码 | **运行时** RectpackSharp 拼 1 张 | ✅ | ✅ 换小 bytes | ❌ RGBA32 |
| **2** 多张单帧 PNG | 离线把 bytes 拆成 N 张 PNG 导入 | Unity SpriteAtlas（build 时） | ✅ | ❌ 重导 N 张 + 重打 bundle | ✅ ASTC |
| **3** 一张拼接大图 PNG | 离线把 bytes 拼成一张大图 + 切片数据导入 | 你离线拼好，Unity 切片 | ✅ | ❌ 重拼 + 重导 + 重打 bundle | ✅ ASTC |

方式 2 / 3 本质是同一类：**把"解码 + 拼图"从运行时挪到离线**，换 Unity 原生 Sprite 工作流 + ASTC 压缩，代价是每次改美术都要重跑离线步骤 + 重打 bundle。

**推荐：现在用 1b。** 理由：怪多、美术频繁迭代，1b 让"改一张图"只换一个 71KB 的 `.bytes`（YooAsset 增量热更）；方式 2/3 每次改动都要重导重打。ASTC 更省包体的优势，等真机包体 / 加载成**实测瓶颈**再迁（那时上编辑器 ScriptedImporter 自动化方式 2/3，不是手动）。

#### Q2.1 方式一的解压开销

`bantuamazones.img.bytes` 只 71KB。`NpkImgParser.Parse` 做的是 zlib 解压 + 位拆分 / 调色板查表，全部发生在**进房间加载那一次**，一个怪约 5~30ms 一次性开销，loading 里感受不到。**解压开销不是放弃方式一的理由**，1a 真正的问题只是"每帧一个 Texture2D + SetPixels"（破合批 + GC），1b 都修掉了。

#### Q2.2 GC 的诚实说法（1b 不是零 GC）

| 分配 | 1a | 1b |
|------|----|----|
| `Parse` 的 `int[] ArgbData`（4 字节/像素，~1-3MB） | ✅ 有 | ✅ **照样有**（一次性） |
| 每帧 `new Color[]`（16 字节/像素，~5-20MB） | ✅ 有 | ❌ 没有 |
| `SetPixels` 内部格式转换拷贝 | ✅ 有 | ❌ 没有 |
| 像素填充本身 | 无（`Color32` 是 struct） | 无（写 `NativeArray`） |

- **1b 没吹"零 GC"**：它照样有解析器无法回避的 `int[]`（1-3MB）。但它砍掉了 1a 那笔最贵的、每像素 `Color[]`（~5-20MB），加载 GC 峰值降到约 1/4 ~ 1/8。
- **两者都只在加载时分配，不是每帧**：游戏跑起来后 `SpriteRenderer.sprite = sprites[idx]` 只改引用，1a / 1b **都不产生任何 GC**。
- **想连 int[] 也省**：改 `NpkImgParser` 直接解码进传入的 `NativeArray<Color32>`（在正确图集偏移上），整条流水线真正零托管分配。第二阶段优化，现在不必。

#### Q2.3 拼完图集怎么索引？

**`image.index`（JSON 里那个数字）是唯一 key，贯穿全程。** 打包位置是一次性施工数据，用完即丢；最终索引就是一个 `Dictionary<int, Sprite>`。

```
NpkSprite[] (每个带 Index/Width/Height/ArgbData)
   │  ① 每个 sprite 的 (Width+1,Height+1) 喂给 RectpackSharp，Id 存 Index
   ▼
打包结果：每个 sprite → 图集坐标 (X,Y)        ← PackingRectangle.Id 扛索引，临时
   │  ② 建一张按 bounds 实际大小的 atlas，按 (X,Y) 拷像素进 NativeArray（注意 Y 翻转）
   ▼
atlas.Apply()
   │  ③ 每个 sprite 用 (X,Y,W,H) 子区域 Sprite.Create
   ▼
Dictionary<int, Sprite> Sprites              ← 运行时索引，key = image.index
   │  运行时 res.Sprites[frame.image.index]   O(1)
   ▼
SpriteRenderer.sprite = ...
```

#### Q2.4 RectpackSharp —— 第三方 MIT 库，不是自己写算法

- **仓库**：[ThomasMiz/RectpackSharp](https://github.com/ThomasMiz/RectpackSharp)，**MIT 许可**（[LICENSE](https://raw.githubusercontent.com/ThomasMiz/RectpackSharp/main/LICENSE) 确认，商用无忧）。
- C# / .NET Standard，**Unity 直接能用**；体积小，作者原话"can even just chuck the files directly onto your project"——拷几个 `.cs` 进工程即可（放 `Packages/cn.etetet.npkparser/Runtime/RectpackSharp/` 或单独建个小包），无需 DLL。
- **API 不是回调式**：填 `PackingRectangle[]` → 调 `RectanglePacker.Pack` → 读回每个 rect 的 X/Y。`PackingRectangle.Id`（uint）**正好存 sprite index**（Pack 不保证顺序，靠 Id 认领）。`maxBoundsWidth/maxBoundsHeight` 用来把图集钉在上限内。

#### Q2.5 内存：按实际 bounds 建大小，别固定 2048

`Texture2D` 一旦按某尺寸创建，就按**完整尺寸**分配内存（CPU 可读副本 + GPU 副本），跟实际写进去几个像素无关。固定建 2048² 而只放一张 100×100 → 浪费 16MB。**修复：先打包拿 `bounds`，再按 `bounds.Width × bounds.Height` 建纹理**（代码就这么写的）：

```csharp
// 2048 只是上限，不是建多大
RectanglePacker.Pack(rects, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
Texture2D atlas = new Texture2D((int)bounds.Width, (int)bounds.Height, TextureFormat.RGBA32, false);
```

- 只有一张 100×100 → `bounds` ≈ 102×102 → ~40KB，不浪费；
- 帧多铺到 800×800 → `bounds` ≈ 800×800 → ~2.5MB，用满；
- 真铺到 2048 → 16MB，花得不冤。
- **NPOT（非 2 的幂）无惩罚**：`bounds` 常是 NPOT（如 800×600），对无压缩 RGBA32 现代平台都没问题。
- **省 CPU 副本**：`Apply(false, makeNoLongerReadable:true)` 上传后释放 CPU 副本，加载后只剩 GPU 那份。

### Q3：Unity 动画系统必须手动配、不能运行时生成，怎么办？

**这是 Unity 的架构死路，不是你不会用。** Mecanim / AnimatorController **无法在运行时从 JSON + Sprite 数据生成 sprite-swap 动画**，两条独立证据（已查 Unity 官方文档）：

1. `AnimationClip.SetCurve` 对非 Legacy 剪辑是 **editor-only**（文档原话："SetCurve will only work at runtime for legacy animation clips"）。
2. sprite 引用（PPtr）要靠 `AnimationUtility.SetObjectReferenceCurve`，它**整个在 `UnityEditor` 命名空间，打包时被剥离**。

所以"运行时从数据生成 AnimationClip"这条路**根本不存在**。**正统做法就是自己写一个极简帧动画组件，用 JSON 配置驱动 `SpriteRenderer.sprite` 换帧**——这正是街机 / DNF / cel 动画的标准写法。（Playables API 的 `PlayableBehaviour.ProcessFrame` 运行时能驱动 SpriteRenderer，但对纯换帧是多余开销，除非以后要 Timeline 混合。）

**两层分离**（你笔记 `2D帧动画游戏组件设计.md` 已写对）：

```
逻辑层 (LSWorld, 确定性, 会回滚 / 快照)        视图层 (Unity, 只读, 不模拟)
  LSAnimComponent                                LSSpriteAnimViewComponent
    AnimId / FrameIndex / FrameTick              读 AnimId / FrameIndex
    ILSUpdate 里按累积时间推进帧                 diff 到变化才换 SpriteRenderer.sprite
  → 决定"第几帧"（影响伤害盒 → 命中 → 结果）    → 只负责"怎么画"
```

**核心原则：帧状态必须在逻辑层。** 每帧 damageBox 决定打没打中 → 伤害 → 血量 → 胜负，必须确定性。视图层**零模拟**，只 diff `LastAnimId` / `LastFrameIndex`。

**时间单位**：JSON 是 ms（`delay: 60/80`），保持 ms。逻辑层用 **FP 定点数累加器**：`FrameTick += UpdateInterval × Speed`，while `FrameTick >= delay` 推进一帧并**保留余数**。完全无漂移，只有确定性的一次性量化抖动（所有客户端一致）。

**删掉 `LSAnimatorComponent`**（Mecanim 那套），不要和新组件并行跑（"并行运行"会双重驱动）。`Skeleton.prefab`（3D 骨骼 Bip001）与此无关，忽略。

### Q4：创建怪物 + 播放走 / 跑 / 攻击（落地步骤）

1. **逻辑层** `LSAnimComponent`：字段 `AnimId/FrameIndex/FrameTick(FP)/Speed(FP)/IsLoop/IsFinished`。在 **LSUnit 创建处**（加 `LSInputComponent` 的同一个地方）给**每个** LSUnit（玩家和怪）都挂上。怪和玩家共用 LSUnit，区别只在于怪挂 AI 组件而非输入组件。
2. **扩展 npkparser**：给 `AnimFrameData` 加 `public AnimBox damageBox;`（+ `AnimBox{AnimVec3 min,max}` / `AnimVec3{x,y,z}` 这些 `[Serializable]` 结构）。给 `AnimId` 加 `Attack` 等常量。
3. **资源层**：重写 `LSAnimResComponentSystem.InitAsync`——解析 IMG → RectpackSharp 打包成一张图集（按 `bounds` 建大小，RGBA32）→ `GetRawTextureData<Color32>` 填充 → 设 Point / Clamp → `Apply(false, true)` → `Sprite.Create` 每帧为子区域 → 缓存 `Dictionary<int,Sprite>`。注册 `stay.json`(Idle) / `move.json`(Walk) / `attack.json`(Attack)。
4. **视图层** `LSSpriteAnimViewComponent`：Awake 里从父 `LSUnitView.SpriteRenderer` 拿引用；Update 里 diff，变了就 `SpriteRenderer.sprite = res.GetSprite(frame.image.index)`，并设 `transform.localPosition = imagePos/100`（100ppu），翻转沿用 `LSUnitViewSystem` 已有的 `localScale.x`（**别再设 `flipX`，会抵消**）。`LSRollback` 把 `LastAnimId/LastFrameIndex` 重置为 -1 强制重新同步。
5. **2D Unit prefab**：新建一个**只含 SpriteRenderer** 的 Unit prefab（无 Animator / SkinnedMesh / bones），加进 AssetBundleCollector。修 `LSUnitViewComponentSystem.InitAsync`：删掉坏的 `demores/...Unit.prefab` + `Get("Knight")`，改用 ResourcesLoader 加载新 prefab；实例化后 **`lsUnitView.SpriteRenderer = unitGo.GetComponentInChildren<SpriteRenderer>()`**（修 bug #2）；把 `AddComponent<LSAnimatorComponent>()` 换成 `AddComponent<LSSpriteAnimViewComponent>()`。
6. **播放控制**：怪创建时 `anim.Play(AnimId.Idle)`；AI 检测到玩家进追击范围 `Play(AnimId.Walk)`；进攻击范围 `Play(AnimId.Attack)`；结束回 Idle/Walk。把 `LSUnitViewSystem.Update` 里那行 `SetFloatValue("Speed",...)` 和它的 TODO 删掉，Idle/Walk 判断**移到逻辑层**。
7. **碰撞**：damageBox 扩展好后，逻辑层每帧从 `frame.damageBox` + `unit.Position` 构造世界空间 AABB，交给 `LSHitboxComponent` / `LSAttackComponent` 做确定性矩形重叠检测。**一次性事件（命中、音效）用"检测帧跳变"判断**，不要发事件——回滚重放时同一跳变会被重新检测并确定性重放。

> 关于"把 `UpdateInterval` 50 → 33 提升动画流畅度"——**别为动画改它**。可见动画帧率由 JSON 的 delay 决定（80ms = 12.5fps），跟 tick 率几乎无关；改 tick 率真正的好处是移动插值更顺、输入延迟更低，但**会增加网络带宽和 CPU**（联网时是硬代价）。

---

## 4. 代码骨架（已按本仓库 ECS 约定 + RectpackSharp 真实 API 核过）

### 4.1 逻辑层组件

```csharp
// Packages/cn.etetet.lockstep/Scripts/Model/Share/LSAnimComponent.cs
using MemoryPack;
using TrueSync;

namespace ET
{
    [ComponentOf(typeof(LSUnit))]
    [MemoryPackable]
    public partial class LSAnimComponent : LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        public int AnimId;            // AnimId.Idle=1, Walk=2, Attack=3...
        public int FrameIndex;        // 当前帧序号
        public FP FrameTick;          // 累积毫秒（FP 定点数，确定性）
        public FP Speed = FP.One;     // 播放倍率
        public bool IsLoop = true;
        public bool IsFinished;
    }
}
```

### 4.2 逻辑层系统（带余数保留的累加器）

```csharp
// Packages/cn.etetet.lockstep/Scripts/Hotfix/Share/LSAnimComponentSystem.cs
namespace ET
{
    [EntitySystemOf(typeof(LSAnimComponent))]
    [LSEntitySystemOf(typeof(LSAnimComponent))]
    public static partial class LSAnimComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAnimComponent self) => self.Play(AnimId.Idle);

        [LSEntitySystem]
        private static void LSUpdate(this LSAnimComponent self)
        {
            if (self.IsFinished) return;
            AnimClipData clip = AnimConfigRegistry.Get(self.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return;

            self.FrameTick += (FP)LSConstValue.UpdateInterval * self.Speed; // 余数保留 -> 无漂移
            int delay = clip.frames[self.FrameIndex].delay;
            if (delay <= 0) delay = LSConstValue.UpdateInterval;

            while (!self.IsFinished && self.FrameTick >= (FP)delay)
            {
                self.FrameTick -= (FP)delay;
                if (++self.FrameIndex >= clip.frames.Length)
                {
                    if (self.IsLoop) self.FrameIndex = 0;
                    else { self.FrameIndex = clip.frames.Length - 1; self.IsFinished = true; break; }
                }
                delay = clip.frames[self.FrameIndex].delay;
                if (delay <= 0) delay = LSConstValue.UpdateInterval;
            }
            // TODO: 本帧 damageBox 采样到世界 AABB，交给 LSHitbox/LSAttack（帧跳变检测一次性事件）
        }

        public static void Play(this LSAnimComponent self, int animId)
        {
            AnimClipData clip = AnimConfigRegistry.Get(animId);
            if (clip == null) return;                       // 未注册就跳过
            self.AnimId = animId; self.FrameIndex = 0; self.FrameTick = FP.Zero;
            self.Speed = FP.One; self.IsLoop = clip.loop; self.IsFinished = false;
        }

        public static AnimFrameData GetCurrentFrame(this LSAnimComponent self)
        {
            AnimClipData clip = AnimConfigRegistry.Get(self.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return default;
            return clip.frames[self.FrameIndex];
        }
    }
}
```

### 4.3 视图层组件 + 系统（diff 换帧）

```csharp
// Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSSpriteAnimViewComponent.cs
using UnityEngine;
namespace ET.Client
{
    [ComponentOf(typeof(LSUnitView))]
    public class LSSpriteAnimViewComponent : Entity, IAwake, IUpdate, ILSRollback
    {
        public SpriteRenderer SpriteRenderer;
        public int LastAnimId = -1;
        public int LastFrameIndex = -1;
    }
}

// Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSSpriteAnimViewComponentSystem.cs
namespace ET.Client
{
    [EntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [LSEntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [FriendOf(typeof(LSSpriteAnimViewComponent))]
    public static partial class LSSpriteAnimViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSSpriteAnimViewComponent self)
            => self.SpriteRenderer = self.GetParent<LSUnitView>().SpriteRenderer;

        [EntitySystem]
        private static void Update(this LSSpriteAnimViewComponent self)
        {
            if (self.SpriteRenderer == null) return;
            LSUnitView view = self.GetParent<LSUnitView>();
            LSAnimComponent anim = view.Unit?.GetComponent<LSAnimComponent>();
            if (anim == null) return;
            if (anim.AnimId == self.LastAnimId && anim.FrameIndex == self.LastFrameIndex) return; // diff

            AnimFrameData frame = anim.GetCurrentFrame();
            LSAnimResComponent res = (self.IScene as Room)?.GetComponent<LSAnimResComponent>();
            Sprite sprite = res?.GetSprite(frame.image.index);
            if (sprite == null) return;

            self.SpriteRenderer.sprite = sprite;
            self.SpriteRenderer.transform.localPosition =
                new Vector3(frame.imagePos.x / 100f, frame.imagePos.y / 100f, 0f);
            self.LastAnimId = anim.AnimId; self.LastFrameIndex = anim.FrameIndex;
        }

        [LSEntitySystem]
        private static void LSRollback(this LSSpriteAnimViewComponent self) { self.LastAnimId = -1; self.LastFrameIndex = -1; }
    }
}
```

### 4.4 资源层图集打包（RectpackSharp 真实 API + 按 bounds 建大小）

```csharp
// LSAnimResComponentSystem.InitAsync 核心片段
using RectpackSharp;
using UnityEngine;
using Unity.Collections;

NpkSprite[] npk = NpkImgParser.Parse(imgAsset.bytes);

// ① 每个 sprite 造一个 PackingRectangle，Id 直接存 Index
const int PAD = 1;
List<PackingRectangle> rects = new();
foreach (NpkSprite s in npk)
    if (s.ArgbData != null)
        rects.Add(new PackingRectangle(0, 0, (uint)(s.Width + PAD), (uint)(s.Height + PAD), (uint)s.Index));

// ② 打包。2048 是上限；bounds 是实际用到的包围盒
RectanglePacker.Pack(rects, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
int atlasW = (int)bounds.Width, atlasH = (int)bounds.Height;

// ③ 按实际大小建图集，NativeArray 零拷贝填充（无托管 GC）
Texture2D atlas = new Texture2D(atlasW, atlasH, TextureFormat.RGBA32, false);
atlas.filterMode = FilterMode.Point;
atlas.wrapMode  = TextureWrapMode.Clamp;
NativeArray<Color32> buf = atlas.GetRawTextureData<Color32>();

foreach (PackingRectangle r in rects)
{
    NpkSprite s = npk[r.Id];                          // Id 直接反查回 sprite
    for (int y = 0; y < s.Height; y++)
    for (int x = 0; x < s.Width;  x++)
    {
        int argb = s.ArgbData[y * s.Width + x];
        // ⚠️ Y 翻转：packer 是 top-left，Unity 纹理原点是 bottom-left
        int dstY = atlasH - 1 - (int)r.Y - y;
        buf[dstY * atlasW + ((int)r.X + x)] = new Color32(
            (byte)((argb >> 16) & 0xFF), (byte)((argb >> 8) & 0xFF),
            (byte)(argb & 0xFF),         (byte)((argb >> 24) & 0xFF));
    }
}
atlas.Apply(false, makeNoLongerReadable: true);
self.Atlas = atlas;

// ④ 每帧 Sprite.Create 为子区域，存进字典 —— 运行时索引（key = image.index）
foreach (PackingRectangle r in rects)
{
    NpkSprite s = npk[r.Id];
    Rect rect = new Rect(r.X, atlasH - r.Y - s.Height, s.Width, s.Height);  // 同样 Y 翻转
    self.Sprites[(int)r.Id] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f);
}
// 打包坐标到此用完即丢；运行时只用 self.Sprites
```

> **Y 翻转是最容易踩的坑**：RectpackSharp 用 top-left 原点，Unity `Texture2D` / `Sprite.Create` 的 Rect 用 bottom-left 原点。先放一个 sprite 进去看是否上下颠倒——颠了就说明翻转方向反了，调一下即可。
>
> **不想引库**：用固定网格拼，索引变纯算术（`cols = ceil(sqrt(N))`，sprite i 的位置 = `(i%cols, i/cols)*cellSize`）。代价是 sprite 大小差异大时浪费空间。别自己实现 MaxRects。

### 4.5 扩展 AnimFrameData（修 bug #3）

```csharp
// Packages/cn.etetet.npkparser/Runtime/AnimClipData.cs 追加
[Serializable] public struct AnimVec3 { public int x, y, z; }
[Serializable] public struct AnimBox  { public AnimVec3 min; public AnimVec3 max; }
// AnimFrameData 增加：public AnimBox damageBox; public string damageType; public string graphicEffect; public string playSound; public string setFlag;
// AnimConfigRegistry.cs 的 AnimId 增加：public const int Attack = 3;
```

---

## 5. 正统性：UI 拼界面 vs 像素序列帧（别混）

| | 游戏 UI（YIUI 面板 / 按钮 / 图标） | 怪物 / 角色序列帧 |
|---|---|---|
| 美术交付 | 标准 PNG | IMG 私有压缩格式（你的情况） |
| 正统方式 | **方式 2：构建期 SpriteAtlas** | 默认也是方式 2；**IMG + 要热更小包 → 方式 1b（合理特例）** |
| 谁打包 | Unity build 时 | 1b = 运行时 RectpackSharp |
| 你工程现在 | 本就走方式 2（标准 Sprite 导入） | 待改成 1b |

**要点：**

- **UI 拼界面：正统 100% 是方式 2（构建期 SpriteAtlas）。** 没有人在运行时打包 UI。你工程的 YIUI 面板本来就是标准 PNG + Sprite 导入，别动它，更别把它改成 1b。
- **像素序列帧：默认也是方式 2**（美术给标准 PNG 时，导入 + SpriteAtlas）。方式 1b 是"私有格式 / 热更小包 / 运行时生成美术"这种特殊情况才用。**你因为用 DNF 的 IMG 格式，踩在这个合理特例上。**
- **两者在工程里并存，各管各的**：UI = 方式 2，怪 = 1b。不要互相套用。
- **什么时候把怪也迁回方式 2**：写编辑器 ScriptedImporter，构建期离线把 IMG 转成原生 Sprite 资源。当"ASTC 压缩省包体 / 省 GPU 显存"的价值**超过**"换一张图只改一个小 bytes 的热更便利"时再做。在那之前，1b 是你"频繁迭代美术 + DNF 格式"场景的合理选择。
- **运行时不管用哪种"图集"，最终产物都是"若干 Sprite 共享一张 Texture2D"**，GPU 合批逻辑一样。SpriteAtlas 的"API 更丰富"丰富在**编辑器 / 构建期工作流**（压缩、变体图集、文件夹规则、Late Binding），**运行时能力跟 1b 等价**，且它不能运行时打包，喂不进运行时解码的像素。

---

## 6. 关键坑与补充

1. **两种"图集"别混**：`SpriteAtlas`（构建期）≠ 手搓 `Texture2D`+`Sprite.Create`（运行时也能）。1b 用的是后者。
2. **内存按 bounds 建大小**：固定建 2048 而没用满会浪费（空部分照样占内存）。先打包拿 `bounds` 再建。
3. **图集纹理必须设 Clamp + Point**：否则相邻帧串色 / 像素被双线性糊掉。
4. **Y 翻转**：RectpackSharp top-left ↔ Unity bottom-left，填充和 `Sprite.Create` 都要翻。先放一个验证方向。
5. **打包留 1px padding**：防 UV 精度溢出渗色。
6. **TextureFormat 必须 RGBA32**（配 `GetRawTextureData<Color32>`）；用 ARGB32 会通道错位。
7. **`makeNoLongerReadable:true`**：上传后不可再 `GetPixel`（帧是一次性写入的，没问题）。
8. **翻转只走 `localScale.x`**：别再设 `SpriteRenderer.flipX`，两者会抵消（`LSUnitViewSystem` 已在用 scale.x）。
9. **一次性事件用帧跳变检测**：不要用事件总线 / lambda 触发命中或音效（回滚重放会重放事件）；在 `LSUpdate` 里判 `oldFrame != newFrame && newFrame == activeFrame`。
10. **`AnimConfigRegistry` 是 `[StaticField]` 全局**：不在 Room 生命周期内，退房不清。多怪多套美术时按 `(unitType, animId)` 索引或退房 `Clear()`。
11. **`Play()` 要 null 守卫**：`AnimConfigRegistry.Get` 对未注册 ID 返回 null，怪专属动作 ID 必须在怪第一次 `LSUpdate` 前注册好。
12. **double-driving**：`LSAnimatorComponent` 和 `LSSpriteAnimViewComponent` 不要并存，视图只留后者。
13. **`Sprite.Create` 运行时只能 FullRect quad**：透明区仍光栅化，无法 Tight 裁剪。
14. **GC**：1b 不是零 GC，仍有一一次性 `int[]`（~1-3MB）；砍掉的是 1a 的每像素 `Color[]`（~5-20MB）。游戏运行中换帧无 GC。

---

## 7. 建议落地顺序

1. 修 bug #1 #2：做 2D Unit prefab + 给 `SpriteRenderer` 赋值（**先让画面能出**）
2. npkparser 扩 `damageBox`（bug #3）
3. 引入 RectpackSharp（拷 `.cs` 进 npkparser 包）
4. 写逻辑层 `LSAnimComponent` + System，在 LSUnit 创建处挂上
5. 重写 `LSAnimResComponentSystem` 为单图集打包（bug #4 + 按 bounds 建大小 + 纹理设置）
6. 写视图层 `LSSpriteAnimViewComponent`，替换 `LSAnimatorComponent`
7. 删 `LSAnimatorComponent`，删 `SetFloatValue("Speed")` 那行，Idle/Walk 判断移到逻辑层
8. 接 damageBox → 碰撞盒；怪 AI 调 `Play`

---

## 8. 相关文件索引

| 用途 | 路径 |
|------|------|
| IMG 解析 | `Packages/cn.etetet.npkparser/Runtime/NpkImgParser.cs` |
| 动作配置结构 | `Packages/cn.etetet.npkparser/Runtime/AnimClipData.cs` |
| 配置注册表 | `Packages/cn.etetet.npkparser/Runtime/AnimConfigRegistry.cs` |
| 当前资源加载（待重写） | `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSAnimResComponentSystem.cs` |
| 当前视图更新（含坐标转换 / 翻转 / 排序） | `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSUnitViewSystem.cs` |
| 当前 Mecanim 动画（待删） | `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSAnimatorComponentSystem.cs` |
| 示例美术 / 配置 | `Packages/cn.etetet.lockstep/Bundles/AnimRes/` |
| 矩形打包库 | [ThomasMiz/RectpackSharp](https://github.com/ThomasMiz/RectpackSharp)（MIT） |
| 已有设计参考 | `Notes/2D帧动画游戏组件设计.md`、`Notes/DNF风格2D表现层实现方案.md`、`Notes/帧同步帧动画技能系统实现方案.md`、`Notes/帧同步碰撞检测策略与实现.md` |

---

## 9. 动画系统架构：两层分离 + 组件该装什么

### 9.1 动画不是单独一层，是被劈成两半

帧同步架构里，"动画层"不是一个干净的词。动画被劈开分居两层：

- **逻辑层**：决定"现在该显示第几帧"
- **视图层**：负责"把这一帧画出来"

三条硬理由（不是风格选择）：

1. **确定性 / 回滚**：第几帧 → 该帧伤害盒 → 命中 → 伤害 → 血量 → 胜负，必须确定。帧状态不进逻辑层，回滚就还原不了。
2. **依赖隔离**：逻辑层（Model/Hotfix Share）不依赖 `SpriteRenderer` / `Texture2D`。
3. **可单跑**：纯逻辑层能脱离视图跑（服务器 / 回放 / 确定性单测），视图只是逻辑的"渲染器"。

### 9.2 正统像素动画系统的 8 块职责

```
① Clip 数据（只读）       一个动作=有序帧列表+每帧(sprite/时长/偏移/伤害盒)+loop   ← AnimClipData
② 动画状态（可变）        当前动作/帧索引/累积时间/倍速/是否播完                  ← LSAnimComponent
③ 时间推进（tick）        按累积时间 vs per-frame delay 推进，保留余数            ← LSAnimComponentSystem.LSUpdate
④ 状态机 / 转换（可选）   条件转换：移动→Walk，攻击→Attack                        ← 不在动画组件！在 AI/输入系统
⑤ 采样（状态→当前帧）     逻辑侧采样伤害盒；视图侧采样 sprite
⑥ 渲染（换 sprite）       ← LSSpriteAnimViewComponent（只读逻辑状态）
⑦ 事件 / 关键帧           命中帧、音效帧 —— 用"帧跳变检测"，不发事件
⑧ 同步 / 回滚集成         逻辑态自动快照回滚；视图失效缓存重同步
```

关键：**④ 状态机不属于动画组件。** 动画组件当"哑播放器"——只管 Play 什么播什么；"该播什么"由更外层的输入 / AI 决定。

### 9.3 三层映射 + 组件该装什么

| 职责 | 归属 | 装在哪 |
|------|------|--------|
| ① Clip 数据 | 配置（只读） | `AnimClipData` + `AnimConfigRegistry`（npkparser） |
| ②③⑦ 帧状态/推进/事件 | 逻辑 | `LSAnimComponent` + System（LSUnit） |
| ④ 转换决策 | 逻辑 | 输入系统 / 怪物 AI 系统（调 Play） |
| ⑤a 采样伤害盒 | 逻辑 | `LSHitboxComponent` / `LSAttackComponent` |
| ⑤b⑥ 采样 sprite + 渲染 | 视图 | `LSSpriteAnimViewComponent`（LSUnitView） |
| 资源（图集/Sprite[]） | 视图资源 | `LSAnimResComponent`（Room） |

**每个组件的字段（精确）：**

```
LSAnimComponent（逻辑，只装"播放状态"）
  AnimId / FrameIndex / FrameTick(FP) / Speed(FP) / IsLoop / IsFinished
  ✗ 不装：sprite 引用、SpriteRenderer、伤害盒（伤害盒是从当前帧+配置实时采样的）

LSSpriteAnimViewComponent（视图，只装"渲染所需 + diff 缓存"）
  SpriteRenderer / LastAnimId / LastFrameIndex
  ✗ 不装：AnimId/FrameIndex（那是逻辑的，只读不存副本，否则两份状态不一致）

LSAnimResComponent（视图资源，挂 Room，只装"解码好的资源"）
  Atlas(Texture2D) / Sprites(Dictionary<int,Sprite>)
```

**一句话原则：决定第几帧 = 逻辑；画出这一帧 = 视图；配置是只读共享数据。动画组件当哑播放器，转换决策留给输入/AI。**

---

## 10. 代码组织：不单独建包，写在 lockstep 文件夹里

### 10.1 结论

动画 ECS **直接写在 `cn.etetet.lockstep` 包的文件夹里**，不单独建包。`npkparser` 保持独立（它有真边界）。

### 10.2 关键洞察：ET 的 asmref 让"包"没有程序集隔离

ET 的 asmref 把每个包按槽位合并进共享游戏程序集：

```
cn.etetet.npkparser/Scripts/Model/Share/*.cs   ─┐
cn.etetet.lockstep/Scripts/Model/Share/*.cs     ─┼──→ 同一个 ET.Model 程序集
cn.etetet.lsanimsystem/Scripts/Model/Share/*.cs ─┘   (LSUnit 和 LSAnimComponent 同程序集)
```

所以"单独建包"在 ET 里**不提供任何程序集隔离或封装强制**——它只是文件夹 + 仪式文件。

### 10.3 决策标准：单向依赖才有真边界

| | 依赖方向 | 真边界？ | 该独立成包？ |
|---|---|---|---|
| **npkparser** | 别人依赖它，它不依赖任何 ET 业务（纯 C#） | ✅ 单向 | ✅（已是） |
| **动画 ECS** | `LSAnimComponent` 是 `[ComponentOf(typeof(LSUnit))]`（依赖 LSUnit），工厂又 `AddComponent<LSAnimComponent>`（依赖动画） | ❌ 双向互耦 | ❌ |

**双向耦合的东西强行拆成两个包，是在撒谎说这里有边界。** 把 LSUnit 和它的动画组件硬分两处，反而不内聚。

### 10.4 文件夹布局（推荐）

```
cn.etetet.lockstep/Scripts/
├── Model/Share/LSAnim/              ← LSAnimComponent.cs
├── Hotfix/Share/LSAnim/             ← LSAnimComponentSystem.cs
├── ModelView/Client/LSAnim/         ← LSSpriteAnimViewComponent.cs, LSAnimResComponent.cs
└── HotfixView/Client/LSAnim/        ← 两者的 System
```

RectpackSharp 的 `.cs` 拷到 `cn.etetet.npkparser/Runtime/RectpackSharp/`（纯 C# 库，放框架无关的包里最合适）。

### 10.5 什么时候才真值得建包

满足任一才抽包：① 确实要复用到另一个项目；② 当独立资产发布；③ 长到很大想用独立程序集做编译隔离。现在都不满足，文件夹够用。真复用那天再剪切+补 asmref，几小时的事。

---

## 11. 完整示例：怪物从刷怪到攻击

### 11.1 数据流（单向，干净）

```
AI / 输入系统 ──Play(animId)──→ LSAnimComponent ──GetCurrentFrame()──→ 碰撞采样(逻辑)
 (决定播什么)                  (AnimId/FrameIndex)                    LSHitbox/LSAttack
                                        │
                                        └──只读 FrameIndex──→ LSSpriteAnimViewComponent
                                                              (换 SpriteRenderer.sprite)
```

AI 不碰 sprite，视图不碰碰撞，碰撞不碰 sprite。

### 11.2 时间线

**① 房间加载（一次性）**：`LSAnimResComponent.InitAsync` 加载 img.bytes → 解析 → RectpackSharp 打图集 → Sprite[] 缓存；加载各动作 json → `AnimConfigRegistry.Register`。

**② 刷怪（逻辑，确定性）**：怪物管理组件建 LSUnit，挂 `LSAnimComponent`(自动 Play Idle) + `LSMonsterAIComponent` + `LSHitbox` + `LSAttack` + `LSHp`。视图镜像建 LSUnitView + `LSSpriteAnimViewComponent`。

**③ 巡逻（每逻辑帧）**：AI 按距离判定调 `anim.Play(Walk)`；`LSAnimComponentSystem.LSUpdate` 按 delay 推进帧；`LSHitboxComponentSystem.LSUpdate` 采样当前帧 damageBox→世界 AABB；视图 diff 到 FrameIndex 变就换 sprite。

**④ 攻击（关键时刻）**：AI 进攻击范围调 `anim.Play(Attack)`（非循环）。逻辑层按 attack.json 推进：起手→蓄力→**挥击判定帧**→收招→IsFinished。`LSAttackComponent` 在判定帧检测帧跳变 + attackBox 非空 → 构造世界攻击 AABB → 与敌人 CurrentHitbox 重叠 → 确定性扣血（记录命中防重复）。视图同步换攻击各帧 sprite。

**⑤ 结束**：AI 见 `anim.IsFinished` → Play(Idle/Walk)。

> 整个攻击判定在逻辑层，跟视图无关。就算画面 lag 一帧，逻辑层判定帧已命中——所有客户端一致。

### 11.3 攻击框 vs 受击框（别混）

| | 受击框（hurt/body） | 攻击框（attack） |
|---|---|---|
| 配置来源 | 每帧的 `damageBox`（**所有动作所有帧都有**，身体） | 攻击 clip 特定帧的 `attackBox`（新增可选字段，判定帧才非空） |
| 归属 | `LSHitboxComponent` | `LSAttackComponent` |
| 何时用 | **别人**打这个怪时做被命中判定 | 这个怪**主动**攻击时碰别人的受击框 |

需给 `AnimFrameData` 加：`public AnimBox damageBox;`（受击，每帧）+ `public AnimBox? attackBox;`（攻击，仅判定帧）。attack.json 给挥击帧填 attackBox，其余留空。

### 11.4 游戏代码只需三件套

```csharp
AnimConfigRegistry.Register(AnimId.Walk, walkClip);           // 1. 注册 clip（房间加载）
unit.AddComponent<LSAnimComponent>();                         // 2. 挂组件（自动 Play Idle）
unit.GetComponent<LSAnimComponent>().Play(AnimId.Attack);     // 3. 让它干啥（AI/输入调）
```

---

## 12. 当前进度审计 + 最短可见路径

> 经代码扫描确认（非凭印象）。

### 12.1 现状

| 层 | 组件 | 状态 |
|----|------|------|
| 配置 | `NpkImgParser` | ✅ 能用 |
| 配置 | `AnimClipData` | ⚠️ 缺 damageBox/attackBox |
| 配置 | `AnimConfigRegistry`/`AnimId` | ⚠️ 只有 Idle/Walk，缺 Attack/Run/Jump |
| 配置 | 动作 JSON | ⚠️ 只有 stay/move，无 attack/run/jump |
| 资源 | `LSAnimResComponent`+System | ⚠️ 写法错（每帧一 Texture2D），需重写成图集 |
| 资源 | RectpackSharp | ❌ 未引入 |
| 逻辑 | `LSAnimComponent`+System | ❌ **不存在** |
| 逻辑 | `LSMonsterAIComponent` | ❌ 不存在 |
| 逻辑 | `LSHitbox`/`LSAttack`/`LSHp` | ❌ 不存在 |
| 逻辑 | 怪物刷怪管理 | ❌ 缺（LSUnitFactory 有，但只建玩家） |
| 视图 | `LSSpriteAnimViewComponent`+System | ❌ **不存在** |
| 视图 | 2D Unit prefab | ❌ 只有 3D Skeleton.prefab（用不了）+ 坏 Knight 引用 |
| 视图 | `LSUnitView.SpriteRenderer` 赋值 | ❌ 字段声明了从不赋值 |
| 视图 | `LSUnitViewSystem` | ⚠️ 坐标转换/翻转/排序都对，只差删 `SetFloatValue("Speed")` |
| 待删 | `LSAnimatorComponent`+System | 🗑 Mecanim 那套，删 |

**定位：能进战斗场景，解析器可用，资源加载要重写，但整个动画逻辑层 + 2D 视图层一行都没写。**

### 12.2 最短可见路径

```
1. 建 2D Unit prefab + 修 SpriteRenderer 赋值        → 能看到一张图
2. 重写 LSAnimResComponent 为图集打包                → 图集 + Sprite[] 就绪
3. 写 LSAnimComponent + System（逻辑推进帧）         → 帧按 delay 往前走
4. 写 LSSpriteAnimViewComponent（视图 diff 换帧）    → 帧变就换图
5. 随便在哪调 anim.Play(Walk)（先硬编码）            → 看到走/待机循环
─────── 以上 = 怪能动 ───────
6. 加 AnimId.Attack + attack.json + Play(Attack)     → 能攻击
7. 写 LSMonsterAIComponent（行为状态机）             → 自动巡逻/追击/攻击
8. 加 damageBox/attackBox + LSHitbox/LSAttack        → 攻击有伤害判定
```

建议先跑通 1-5（怪在场景里循环走路），再补 6-8。第 4 节的代码骨架就是 1-5。

---

## 13. 状态机：要的是行为 FSM，不是动画图

### 13.1 三种"状态机"，只要一种

| 种类 | 要不要 | 原因 |
|------|--------|------|
| ① Mecanim 动画状态图（states/transitions/blend） | ❌ | 数据驱动逐帧动画用不上，运行时也没法生成 |
| ② 动画层自己的状态机（组件内管 Playing/Blend） | ❌ | 动画组件当哑播放器，不该自己决定下一步 |
| ③ **行为/AI 状态机**（Idle/Patrol/Chase/Attack/Hurt/Die） | ✅ | 这才是"流转"——它决定播什么，每次流转=一次 Play() |

**关键认知：你要的"动画状态机流转"本质是行为状态机。** 动画只是行为状态的镜像——行为是 Attack 就 Play(Attack)，转回 Chase 就 Play(Walk)。动画组件不持有流转逻辑。

### 13.2 行为 FSM 长什么样（AI 层，enum + switch）

```csharp
// Model/Share
public enum MonsterState { Idle, Patrol, Chase, Attack, Hurt, Die }
public partial class LSMonsterAIComponent : LSEntity, ILSUpdate, IAwake {
    public MonsterState State = MonsterState.Idle;
    public FP AttackRange, ChaseRange, PatrolRange;
    public long StateTimer;
}

// Hotfix/Share
[LSEntitySystem]
private static void LSUpdate(this LSMonsterAIComponent self) {
    LSUnit me = self.GetParent<LSUnit>();
    LSUnit target = FindPlayer(me);                 // 确定性查找
    LSAnimComponent anim = me.GetComponent<LSAnimComponent>();
    FP dist = target != null ? TSVector.Distance(me.Position, target.Position) : FP.MaxValue;

    switch (self.State) {
        case Idle:    anim.Play(AnimId.Idle);
                      if (dist <= self.ChaseRange) self.Go(Chase);
                      else if (self.StateTimer > 2000) self.Go(Patrol); break;
        case Patrol:  PatrolMove(me); anim.Play(AnimId.Walk);
                      if (dist <= self.ChaseRange) self.Go(Chase); break;
        case Chase:   MoveToward(me, target.Position); anim.Play(AnimId.Walk);
                      if (dist <= self.AttackRange) self.Go(Attack);
                      else if (dist > self.PatrolRange) self.Go(Idle); break;
        case Attack:  if (anim.AnimId != AnimId.Attack) anim.Play(AnimId.Attack);
                      if (anim.IsFinished) self.Go(dist <= self.ChaseRange ? Chase : Idle); break;
        case Hurt:    if (anim.AnimId != AnimId.Hurt) anim.Play(AnimId.Hurt);
                      if (anim.IsFinished) self.Go(Idle); break;
        case Die:     if (anim.AnimId != AnimId.Die) anim.Play(AnimId.Die); break;
    }
    self.StateTimer += LSConstValue.UpdateInterval;
}

public static void OnHurt(this LSMonsterAIComponent self) {  // 被命中时调
    if (self.State != Die) self.Go(Hurt);
}
```

**一个枚举 + 一个 switch，放 AI 组件的 LSUpdate。** `self.Go(newState)` 就是流转，进入新状态第一个 `anim.Play(...)` 把动画切过去。状态机与动画组件的唯一接口是 `Play()` + 读 `AnimId`/`IsFinished`。

### 13.3 可选流转规则（v1 不上，需要再加）

- **攻击承诺**：Attack 未到判定帧不许被打断 → 在 OnHurt 里判 `anim.AnimId==Attack && 未过判定帧` 则忽略。
- **优先级**：Die > Hurt > 其它。上面 OnHurt 已体现。

### 13.4 代码 vs 数据驱动

现在用**代码（switch）**，最直接好调。等有几十种怪、策划要调行为表时，再抽成数据驱动 FSM（每怪一份配置）。别一上来就做数据驱动，过度设计。
