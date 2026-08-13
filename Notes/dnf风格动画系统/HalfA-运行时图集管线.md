# Half A 执行手册：运行时图集管线（img.bytes → Texture2D + Sprite[]）

> 本文档是**执行手册**（playbook），给当前或新会话/新 agent 照着做。
> 配套文档（同文件夹）：`DNF风格2D美术资源与帧动画系统方案.md`（主方案，下称"主文档"）、`参考-npkparser包说明.md`。
> 范围：只管"把 `.img.bytes` 运行时解码打包成单张图集 + Sprite[] 字典"这一段（Half A）。动画帧推进/换帧/AI/碰撞是 Half B，不在本文档。

---

## 0. 会话恢复速查（新 agent 先读这段）

- **Half A 目标**：重写 `LSAnimResComponentSystem.InitAsync`，把现在的"每帧一个 Texture2D + SetPixels"（破合批、GC 大）换成"解码 → RectpackSharp 打成**单张运行时图集** → `GetRawTextureData<Color32>` 填充 → `Sprite.Create` 每帧为子区域 → `Dictionary<int,Sprite>`"。产出一个能被 Half B 视图层 `res.GetSprite(image.index)` 消费的 sprite 字典。
- **当前状态**：解析器（npkparser）✅ 能用；`LSAnimResComponent`+System 半成品且**写法错**（每帧一 Texture2D）；2D Unit prefab 不存在（只有 3D Skeleton.prefab + 坏的 Knight 引用）；`LSUnitView.SpriteRenderer` 字段没赋值。**Half A 还没开始实现**。
- **下一步**：按 §5 流程执行；先做任 T1（2D prefab + SpriteRenderer 赋值），否则没法验证图集。
- **验证标准**：§7，在 Unity 里跑，屏幕上能看到那只怪的正确一帧（颜色对、不上下颠倒）。
- **新 agent 操作流程**：详 §10。简版 = 读本文档 → 读主文档 §1/§3Q1/Q2/§4.4/§6 → 读 §11 列的**真实代码文件**（别读副本）→ 按 §5 执行 → 每完成一项更新 §8 → 遇问题记 §9 → 需要在 Unity 里看效果时找用户测（§7）。

---

## 1. Half A 的范围

**做（IN scope）：**
- 引入 RectpackSharp（拷源码进 npkparser 包）。
- 重写 `LSAnimResComponent`（加 `Atlas` 字段）+ `LSAnimResComponentSystem.InitAsync`（图集打包）。
- 扩展 `AnimClipData` 的 `AnimFrameData` 加 `damageBox`（+ 攻击框 `attackBox`），让 JSON 不再静默丢弃。
- 建 2D Unit prefab + 修 `LSUnitViewComponentSystem`（加载 prefab、赋 `SpriteRenderer`）——**这是验证图集的前提，不是 Half B**。
- 最小验证：进场景能看到怪的一帧静态图。

**不做（OUT of scope，属于 Half B / 后续）：**
- 帧推进逻辑层 `LSAnimComponent`（Half B-逻辑）。
- 视图换帧 `LSSpriteAnimViewComponent`（Half B-视图）。
- AI / 行为状态机 / 碰撞 / 伤害。
- 删 `LSAnimatorComponent`（Half B 接好后再删，这里先不动避免编译链断）。

---

## 2. 方案（方式 1b）+ 关键决策

**方案**：`.img.bytes` 原样进包（YooAsset 当 TextAsset 加载）→ 进房间一次性 `NpkImgParser.Parse` → RectpackSharp 把所有帧打进**一张** `Texture2D`（按实际 `bounds` 建大小）→ `GetRawTextureData<Color32>` 零拷贝填充 → `Apply(false, makeNoLongerReadable:true)` → 每帧 `Sprite.Create` 为子区域 → 存进 `Dictionary<int,Sprite>`。详见主文档 §3 Q1/Q2、§4.4。

**必须守的硬决策（踩过/验过的坑，别违反）：**

| # | 决策 | 原因 |
|---|------|------|
| 1 | `TextureFormat.RGBA32`（不是 ARGB32） | `GetRawTextureData<Color32>` 内存布局是 R,G,B,A；ARGB32 会通道错位（颜色错） |
| 2 | `filterMode = Point` | 像素图，否则双线性糊掉 |
| 3 | `wrapMode = Clamp` | 图集必须，否则 UV 精度溢出串到相邻帧渗色 |
| 4 | 按 `bounds` 建大小，不固定 2048 | 固定建大而没用满 = 浪费内存（空部分照样占） |
| 5 | 帧间留 1px padding | 防渗色 |
| 6 | **Y 翻转**：packer top-left ↔ Unity bottom-left | 填充和 Sprite.Create 的 rect 都要翻（最容易踩的坑） |
| 7 | `Apply(false, makeNoLongerReadable:true)` | 上传 GPU 后释放 CPU 副本（帧是写一次的） |
| 8 | `Sprite.Create` 运行时只能 FullRect quad | 已知限制，透明区仍光栅化，非 bug |
| 9 | ARGB int → RGBA 字节拆分：R=`(argb>>16)&0xFF`，G=`(argb>>8)&0xFF`，B=`argb&0xFF`，A=`(argb>>24)&0xFF` | NpkSprite.ArgbData 是 ARGB8888 int |

---

## 3. 起点：现状快照 + 已知 bug

### 3.1 当前 `LSAnimResComponent.cs`（路径：`Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSAnimResComponent.cs`）

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class LSAnimResComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<int, Sprite> Sprites = new();
        public List<Texture2D> Textures = new();   // 半成品遗留，重写后改成单张 Atlas
    }
}
```

**目标改动**：`Textures` 列表换成 `public Texture2D Atlas;`（单张图集）。

### 3.2 当前 `LSAnimResComponentSystem.cs`（路径：同目录 HotfixView/Client）—— **写法错，要重写**

```csharp
// ❌ 当前：每帧一个 Texture2D + SetPixels —— 破合批 + GC
foreach (NpkSprite npkSprite in npkSprites)
{
    if (npkSprite.ArgbData == null) continue;
    Texture2D tex = new Texture2D(npkSprite.Width, npkSprite.Height, TextureFormat.ARGB32, false); // ❌ 应 RGBA32
    Color[] colors = new Color[npkSprite.ArgbData.Length];   // ❌ 16字节/像素托管数组，GC 大
    for (int i = 0; i < npkSprite.ArgbData.Length; i++) { ... colors[i] = new Color32(...); }
    tex.SetPixels(colors); tex.Apply();                       // ❌ 应 GetRawTextureData<Color32>
    Sprite sprite = Sprite.Create(tex, new Rect(0,0,npkSprite.Width,npkSprite.Height), new Vector2(0.5f,0.5f), 100f);
    self.Sprites[npkSprite.Index] = sprite;
    self.Textures.Add(tex);                                    // ❌ 每帧不同 Texture2D = 每帧不同批次键，合批全坏
}
```

### 3.3 Half A 相关的已知 bug（主文档 §1）

| # | 位置 | 问题 | Half A 要修吗 |
|---|------|------|---------------|
| 1 | `LSUnitViewComponentSystem.cs:33-35` | 硬编码不存在的 `demores/...Unit.prefab` + `Get("Knight")` | ✅ 是（验证前提） |
| 2 | `LSUnitView.cs:14` | `SpriteRenderer` 字段从不赋值 | ✅ 是（验证前提） |
| 3 | `AnimClipData.cs` | 没接收 `damageBox`，JsonUtility 丢弃 | ✅ 是 |
| 4 | `LSAnimResComponentSystem.cs:43-68` | 每帧一个 Texture2D + SetPixels | ✅ 是（Half A 核心） |

---

## 4. 任务清单（每完成一项把 `[ ]` 改成 `[x]`，并在 §8 记日期）

- [ ] **T0** 引入 RectpackSharp：把 [ThomasMiz/RectpackSharp](https://github.com/ThomasMiz/RectpackSharp) 的 `RectanglePacker.cs` + `PackingRectangle.cs` 等源码拷进 `Packages/cn.etetet.npkparser/Runtime/RectpackSharp/`，加 asmdef（纯 C#，无 Unity 依赖）。
- [ ] **T1** 建 2D Unit prefab：在 `Packages/cn.etetet.lockstep/Bundles/Unit/` 下新建一个只含 `SpriteRenderer` 的 prefab（无 Animator/SkinnedMesh/bones），加进 AssetBundleCollector。
- [ ] **T2** 修 `LSUnitViewComponentSystem.InitAsync`：把坏的 `demores/...Unit.prefab` + `Get("Knight")` 换成 YooAsset 加载新 prefab；实例化后 `lsUnitView.SpriteRenderer = unitGo.GetComponentInChildren<SpriteRenderer>()`（修 bug #2）。**先别**加 LSAnimComponent（那是 Half B），保留现有 `LSAnimatorComponent` 让编译不断。
- [ ] **T3** 扩展 `AnimClipData`：给 `AnimFrameData` 加 `public AnimBox damageBox;` + `public AnimBox? attackBox;`，加 `[Serializable] struct AnimBox{AnimVec3 min,max;}` / `[Serializable] struct AnimVec3{int x,y,z;}`。
- [ ] **T4** 改 `LSAnimResComponent`：`List<Texture2D> Textures` → `Texture2D Atlas;`。
- [ ] **T5** **重写 `LSAnimResComponentSystem.InitAsync`** 为图集打包（§6.3 目标代码）。`Destroy` 里改成 `Object.Destroy(self.Atlas); Sprites.Clear();`。
- [ ] **T6** 临时验证代码：在 T2 实例化 unit 后，加一行临时测试 `lsUnitView.SpriteRenderer.sprite = animRes.GetSprite(9);`（9 是 move.json 第 0 帧引用的 index），让用户进场景看一眼。
- [ ] **T7** 用户验证通过后，删掉 T6 的临时测试代码（正式留给 Half B 的视图组件换帧）。

---

## 5. 执行流程（按顺序）

```
T0 引入 RectpackSharp
   ↓
T1 2D prefab  ──┐
T2 修 LSUnitViewComponentSystem（加载 prefab + 赋 SpriteRenderer）──┤
T6 临时贴一张 sprite（GetSprite(9)） ──→ 【验证点 V1：进场景能看到一张怪】
   ↓                                                              （此时图集还没改，用旧 loader 的 sprite 也行，先确认 prefab 链路通）
T3 扩展 AnimClipData（damageBox/attackBox）
T4 改 LSAnimResComponent（Atlas 字段）
T5 重写 InitAsync 为图集打包 ──→ 【验证点 V2：屏幕上的怪颜色对、不颠倒、内存合理】
   ↓
T7 删临时测试代码 → Half A 完成，交给 Half B
```

> V1 先确保 prefab+SpriteRenderer 链路通（用旧 loader 的 sprite 就行）；V2 验证新图集管线本身正确。两段验证隔离问题域。

---

## 6. 关键代码参考

### 6.1 NpkSprite 结构 & Parse（读，不改；来自 `NpkImgParser.cs`）

```csharp
public struct NpkSprite {
    public int Index;              // ★ 贯穿全程的 key（= JSON 的 image.index）
    public int Width, Height;      // 实际像素宽高（要打包的尺寸）
    public int X, Y;               // 在帧画布里的偏移
    public int FrameWidth, FrameHeight;
    public int[] ArgbData;         // ARGB8888 像素，int[Width*Height]，可能为 null（引用帧）
}
// 用法：NpkSprite[] sprites = NpkImgParser.Parse(byte[] imgFileData);
```

### 6.2 RectpackSharp 用法（第三方 MIT 库，`using RectpackSharp;`）

```csharp
// 填 PackingRectangle[]，Id 存 sprite.Index（Pack 不保证顺序，靠 Id 认领）
PackingRectangle[] rects = ...;   // 每个 (0, 0, Width+1, Height+1, (uint)Index)，+1 是 padding
RectanglePacker.Pack(rects, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
// 2048 是上限；bounds.Width/Height 是实际包围盒（按它建图集）。Pack 后每个 rect 被赋了 X/Y。
```

### 6.3 目标 `LSAnimResComponentSystem.InitAsync`（图集打包核心）

```csharp
using RectpackSharp;
using UnityEngine;
using Unity.Collections;

public static async ETTask InitAsync(this LSAnimResComponent self)
{
    Room room = self.Room();
    ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();

    // 1. 加载并解析 IMG
    string imgPath = "Packages/cn.etetet.lockstep/Bundles/AnimRes/bantuamazones.img.bytes";
    TextAsset imgAsset = await resLoader.LoadAssetAsync<TextAsset>(imgPath);
    NpkSprite[] npk = NpkImgParser.Parse(imgAsset.bytes);

    // 2. 收集要打包的矩形（+1 padding），Id = sprite.Index
    const int PAD = 1;
    List<PackingRectangle> rects = new();
    foreach (NpkSprite s in npk)
        if (s.ArgbData != null)
            rects.Add(new PackingRectangle(0, 0, (uint)(s.Width + PAD), (uint)(s.Height + PAD), (uint)s.Index));

    // 3. 打包。2048=上限；bounds=实际包围盒
    RectanglePacker.Pack(rects, out PackingRectangle bounds, PackingHints.FindBest, 1, 1, 2048, 2048);
    int atlasW = (int)bounds.Width, atlasH = (int)bounds.Height;

    // 4. 建图集（RGBA32！），设 Point/Clamp，NativeArray 零拷贝填充
    Texture2D atlas = new Texture2D(atlasW, atlasH, TextureFormat.RGBA32, false);
    atlas.filterMode = FilterMode.Point;
    atlas.wrapMode  = TextureWrapMode.Clamp;
    NativeArray<Color32> buf = atlas.GetRawTextureData<Color32>();

    foreach (PackingRectangle r in rects)
    {
        NpkSprite s = npk[r.Id];                       // Id 反查回 sprite
        for (int y = 0; y < s.Height; y++)
        for (int x = 0; x < s.Width;  x++)
        {
            int argb = s.ArgbData[y * s.Width + x];
            int dstY = atlasH - 1 - (int)r.Y - y;      // ★ Y 翻转
            buf[dstY * atlasW + ((int)r.X + x)] = new Color32(
                (byte)((argb >> 16) & 0xFF),            // R
                (byte)((argb >>  8) & 0xFF),            // G
                (byte)( argb        & 0xFF),            // B
                (byte)((argb >> 24) & 0xFF));           // A
        }
    }
    atlas.Apply(false, makeNoLongerReadable: true);
    self.Atlas = atlas;
    Log.Info($"[LSAnimRes] Atlas built: {atlasW}x{atlasH}, {rects.Count} sprites");

    // 5. 每帧 Sprite.Create 为子区域，存进字典（key = image.index）
    foreach (PackingRectangle r in rects)
    {
        NpkSprite s = npk[r.Id];
        Rect rect = new Rect(r.X, atlasH - r.Y - s.Height, s.Width, s.Height);  // ★ Y 翻转
        self.Sprites[(int)r.Id] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f);
    }

    // 6. 注册动作配置（damageBox 扩展后不再被丢弃）
    await RegisterClip(self, resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/stay.json", AnimId.Idle);
    await RegisterClip(self, resLoader, "Packages/cn.etetet.lockstep/Bundles/AnimRes/move.json", AnimId.Walk);
    // attack.json 等 Half B 再加
}

private static async ETTask RegisterClip(this LSAnimResComponent self, ResourcesLoaderComponent res, string path, int animId)
{
    TextAsset asset = await res.LoadAssetAsync<TextAsset>(path);
    AnimClipData data = JsonUtility.FromJson<AnimClipData>(asset.text);
    AnimConfigRegistry.Register(animId, data);
}

public static Sprite GetSprite(this LSAnimResComponent self, int imgIndex)
    => self.Sprites.TryGetValue(imgIndex, out var sp) ? sp : null;
```

> ⚠️ Y 翻转是最容易错的点。V2 验证时如果怪上下颠倒，把填充的 `dstY` 和 rect 的 Y 翻转方向一起调（两个要一致）。

### 6.4 AnimClipData 扩展（`Packages/cn.etetet.npkparser/Runtime/AnimClipData.cs`）

```csharp
// 追加：
[Serializable] public struct AnimVec3 { public int x, y, z; }
[Serializable] public struct AnimBox  { public AnimVec3 min; public AnimVec3 max; }

// AnimFrameData 增加：
//   public AnimBox damageBox;     // 受击/身体盒，每帧都有（对应 JSON 已有的 damageBox）
//   public AnimBox? attackBox;    // 攻击盒，仅攻击 clip 的判定帧非空（JSON 暂没这字段，先留接口）
```

### 6.5 2D Unit prefab（T1）

Unity 里新建 prefab：根 GameObject（空）+ 子 GameObject 挂 `SpriteRenderer`（Sprite 先留空，运行时赋；SortingLayer 自定）。无 Animator / 无 SkinnedMeshRenderer / 无 bones。保存到 `Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab`，在 AssetBundleCollector 里登记。

### 6.6 LSUnitViewComponentSystem.InitAsync 修改要点（T2）

```csharp
// 把
//   string assetsName = $"Packages/cn.etetet.demores/Bundles/Unit/Unit.prefab";
//   GameObject bundleGameObject = await ...LoadAssetAsync<GameObject>(assetsName);
//   GameObject prefab = bundleGameObject.Get<GameObject>("Knight");
// 改成
string prefabPath = "Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab";
GameObject prefab = await room.GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(prefabPath);
// 实例化后赋值（修 bug #2）：
lsUnitView.SpriteRenderer = unitGo.GetComponentInChildren<SpriteRenderer>();
// LSAnimatorComponent 暂时保留（Half B 再换），保证当前能编译能跑
```

---

## 7. 如何测试（需要你在 Unity 里手动跑）

Half A 没法纯代码验证（要看渲染结果），所以每个验证点都得**你进 Unity Play 看一眼**，把结果告诉 agent。

### 验证点 V1（T1+T2+T6 后）：prefab 链路通
1. 打开战斗场景，Play。
2. **预期**：场景里出现一个怪，显示 move.json 第 0 帧那张图（index=9）。
3. **判据**：有图就行（此时还用旧/临时 sprite，颜色/颠倒先不管）。
4. **若没有**：看 Console 报错——常见是 prefab 路径不对、AssetBundle 没打包进、SpriteRenderer 没赋上。

### 验证点 V2（T3+T4+T5 后）：图集管线正确
1. Play。
2. **预期**：怪显示正确，**颜色正常（不红不绿不错位）、上下不颠倒**。
3. **判据**：
   - 颜色错（发红/发绿/黑白）→ 多半是 RGBA32 用成 ARGB32，或 ARGB→RGBA 拆字节错。
   - 上下颠倒 → Y 翻转方向反了，调 `dstY` 和 rect.Y 的翻转（两者一起翻）。
   - 图集里串到别的帧/边缘渗色 → wrapMode 没设 Clamp 或 padding 没有。
   - 内存爆 → 检查是不是固定建了 2048 而不是按 bounds。
4. 可选调试：临时在 InitAsync 末尾把 `self.Atlas` EncodeToPNG 存盘看一眼图集拼得对不对。

### 通过标准
V2 看到"颜色对、不颠倒、单张图集"→ Half A 完成，T7 删临时代码，交给 Half B。

---

## 从 Half A 到 Half B（递进关系 + 联合验收 VT）

Half A 只管"供给"：图集 + sprite 字典 + clip 注册。它的**独立验证止于 V2**（一个 unit 静态显示一张正确的图）。Half A 本身不产生动画——T7 删掉临时静态 sprite 后，玩家身上暂时没动画，这是正常的交付点。

"怪物创建 + 动起来"是 **Half B** 的职责（`HalfB-帧动画驱动与怪物创建.md`）：
- 逻辑层 `LSAnimComponent` 按 clip 的 delay 推进帧
- 视图层 `LSSpriteAnimViewComponent` diff 换帧
- 创建怪物 unit `Play(Walk)`（播 move.json）
- 玩家挂新系统 `Play(Idle)`，删掉 Mecanim

**联合验收 VT**（定义在 Half B 文档 §7）：进战斗场景，看到一个怪物原地循环播放 move.json 的走动画。**VT 的前提是 Half A 的 V2 已通过**（图集渲染正确）。

```
Half A ──产出 sprite 字典 + clip 注册──→ V2：静态一帧显示正确
                                         │  (V2 通过 = 供给没问题)
                                         ▼
Half B ──消费 sprite 驱动动画 + 建怪物──→ VT：怪物原地循环走 move.json
```

两份文档互相咬合：本文档（Half A）末尾 → Half B 的 VT；Half B 文档开头 → 本文档的 V2（列为前提）。新 agent 接手时按"先 A 过 V2，再做 B 到 VT"的顺序走。

---

## 8. 实施进度（agent 每次更���这里）

| 任务 | 状态 | 日期 | 备注 |
|------|------|------|------|
| T0 RectpackSharp 引入 | ✅ 完成 | 2026-08-11 | 下载 4 个 .cs 到 npkparser/Runtime/RectpackSharp/，移除 System.Drawing 依赖（Unity 不支持） |
| T1 2D Unit prefab | ✅ 完成 | 2026-08-11 | 用户已建 Unit2D.prefab 并登记 AssetBundleCollector（commit f739f3d9a） |
| T2 修 LSUnitViewComponentSystem | ✅ 完成 | 2026-08-11 | 加载 Unit2D.prefab + 赋 SpriteRenderer，保留 LSAnimatorComponent |
| T3 AnimClipData 扩 damageBox | ✅ 完成 | 2026-08-11 | 加 damageBox + AnimBox/AnimVec3；attackBox 暂不加（attack.json 还没有） |
| T4 LSAnimResComponent 改 Atlas | ✅ 完成 | 2026-08-11 | List&lt;Texture2D&gt; Textures → Texture2D Atlas |
| T5 重写 InitAsync 图集打包 | ✅ 完成 | 2026-08-11 | RectpackSharp 打包 + GetRawTextureData + Y翻转 + Apply(false,true) |
| T6 临时验证代码 | ✅ 完成 | 2026-08-11 | 玩家 unit 上临时贴 GetSprite(9) |
| T7 删临时代码 | ⬜ 未开始 | | V2 验证通过后删 T6 那行 |
| V1 验证通过 | 🟡 待测 | | 等用户进 Unity Play |
| V2 验证通过 | 🟡 待测 | | 等用户进 Unity Play |

**已知风险（待 Unity 编译验证）**：RectpackSharp 的 `#if NET5_0_OR_GREATER / #elif NETSTANDARD2_0` 条件编译——Unity 6 + .NET Standard 2.1 按规范应累积定义 NETSTANDARD2_0，理论能编。若刷新 Unity 报错，看具体错误反馈给 agent。

状态用：⬜未开始 / 🟡进行中 / ✅完成 / ⚠️阻塞

---

## 9. 问题记录（遇到问题就加一行，解决了改状态）

| # | 问题描述 | 原因 | 解决/对策 | 状态 |
|---|---------|------|----------|------|
| 1 | （示例）怪上下颠倒 | packer top-left vs Unity bottom-left | Y 翻转方向调对 | 待复现 |
| | | | | |

---

## 10. 新会话 / 新 agent 操作指引（playbook）

**开场，按顺序做：**

1. **读本文件夹三份文档**：本文档（Half A）→ 主文档 `DNF风格2D美术资源与帧动画系统方案.md` 的 §1（4 个 bug）/ §3 Q1+Q2（方案）/ §4.4（图集代码）/ §6（坑）→ `参考-npkparser包说明.md`（解析器 API）。
2. **读真实代码文件**（路径见 §11，**读真的，别读副本**）：
   - `Packages/cn.etetet.npkparser/Runtime/NpkImgParser.cs`（NpkSprite + Parse）
   - `Packages/cn.etetet.npkparser/Runtime/AnimClipData.cs`（要改）
   - `Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSAnimResComponent.cs`（要改）
   - `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSAnimResComponentSystem.cs`（要重写）
   - `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSUnitViewComponentSystem.cs`（要改）
   - `Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSUnitView.cs`（SpriteRenderer 字段在这）
3. **看 §8 进度表**，从下一个 ⬜ 任务接着做。
4. **按 §5 流程 + §6 代码执行**。每完成一个任务：把 §8 对应行状态改 ✅ + 填日期；有备注写备注。
5. **遇到问题**：加到 §9 问题记录；阻塞了把 §8 状态改 ⚠️。
6. **到验证点（V1/V2）**：本文档没法自测，**告诉用户去 Unity 里 Play 看效果**（§7），等用户反馈再继续。

**铁律：**

- ✏️ **只改真实文件**（§11 路径），**绝不**改本文件夹的代码副本或快照——快照只是参考，改了不会生效。
- 🔁 **代码以实际文件为准**：本文档的代码快照可能滞后，动手前先读真实文件确认当前内容。
- 🧱 **守 §2 的 9 条硬决策**（尤其 RGBA32、Y 翻转、bounds 建大小、Clamp+Point）——都是验过的坑。
- 🚧 **不越界**：Half A 只做到"图集 + sprite 字典 + 能静态显示一帧"。帧推进/换帧/AI/碰撞是 Half B，别提前做。
- 🗑 **LSAnimatorComponent 先留着**，Half B 接好视图组件再删，避免编译链断。

**完成判据**：V2 验证通过（怪颜色对、不颠倒、单张图集）+ T7 删完临时代码 → Half A 收工，§8 全绿。

---

## 11. 引用（本地 + 真实路径）

**本文件夹（参考文档，只读）：**
- `DNF风格2D美术资源与帧动画系统方案.md` — 主方案（§1 bug、§3 方案、§4.4 图集代码、§6 坑）
- `参考-npkparser包说明.md` — npkparser 包 API 说明

**要读/改的真实代码文件（在仓库 Packages/ 下，改这些别改副本）：**
| 文件 | 操作 |
|------|------|
| `Packages/cn.etetet.npkparser/Runtime/NpkImgParser.cs` | 读（NpkSprite + Parse） |
| `Packages/cn.etetet.npkparser/Runtime/AnimClipData.cs` | 改（加 damageBox/attackBox） |
| `Packages/cn.etetet.npkparser/Runtime/AnimConfigRegistry.cs` | 读（AnimId 常量；Half B 再加 Attack） |
| `Packages/cn.etetet.npkparser/Runtime/RectpackSharp/` | 新增（T0 拷源码进） |
| `Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSAnimResComponent.cs` | 改（Atlas 字段） |
| `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSAnimResComponentSystem.cs` | 重写（图集打包） |
| `Packages/cn.etetet.lockstep/Scripts/HotfixView/Client/LSUnitViewComponentSystem.cs` | 改（prefab + SpriteRenderer 赋值） |
| `Packages/cn.etetet.lockstep/Scripts/ModelView/Client/LSUnitView.cs` | 读（SpriteRenderer 字段位置） |
| `Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab` | 新建（T1） |
| `Packages/cn.etetet.lockstep/Bundles/AnimRes/*.img.bytes` `*.json` | 读（加载用） |
