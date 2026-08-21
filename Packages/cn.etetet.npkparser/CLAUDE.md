# cn.etetet.npkparser — NPK/IMG 精灵图解析 + 动画配置 CLAUDE.md

## 包概述

基础库包。解析 Neople IMG 格式的精灵图文件，提供动画配置数据结构和全局注册表。

无 Unity 依赖，无 Entity/Component/System 模式，纯 C# 工具代码。

## 目录结构

```
cn.etetet.npkparser/
├── Runtime/
│   ├── npkParser.asmdef         # 程序集名: ET.NpkParser
│   ├── NpkImgParser.cs          # IMG 二进制解析器 + NpkSprite 结构体
│   ├── AnimClipData.cs          # 动画剪辑数据结构
│   └── AnimConfigRegistry.cs    # 动画配置注册表 + AnimId 常量
├── DotNet~/
│   └── ET.NpkParser.csproj      # 服务端独立编译
├── package.json                 # 依赖 core, sourcegenerator
└── CLAUDE.md
```

## 依赖

| 包 | 程序集 | 用途 |
|---|---|---|
| cn.etetet.core | ET.Core | `[EnableClass]`、`[StaticField]` 特性 |
| cn.etetet.sourcegenerator | ET.SourceGeneratorAttribute | 源码生成器运行时属性 |

## 类型一览

### NpkSprite — 精灵数据

```csharp
public struct NpkSprite
{
    public int Index;                          // 帧序号
    public int Width, Height;                  // 实际像素宽高
    public int X, Y;                           // 在精灵图中的偏移
    public int FrameWidth, FrameHeight;        // 帧画布尺寸
    public int[] ArgbData;                     // ARGB8888 像素数据（int[Width*Height]）
}
```

### NpkImgParser — IMG 解析器

```csharp
// 一次性解析整个 IMG 文件
NpkSprite[] sprites = NpkImgParser.Parse(byte[] imgFileData);
```

支持的格式：
- IMG v2：ARGB1555、ARGB4444、ARGB8888 像素格式（统一小端 ARGB 整型：8888 字节序 = B,G,R,A；按 R,G,B,A 读会红蓝互换，2026-08-21 bloodboom 特效实证）
- IMG v4：索引色 + 调色板
- zlib 压缩帧
- 引用帧（TypeReference，复用已有帧数据）

### AnimClipData — 动画剪辑

```csharp
public class AnimClipData
{
    public bool loop;               // 是否循环
    public int frameMax;            // 最大帧数
    public AnimFrameData[] frames;  // 帧列表
    public int totalDuration;       // 总时长（ms）
}

public struct AnimFrameData
{
    public int index;               // 帧序号
    public AnimFrameImage image;    // 精灵图引用 { path, index }
    public AnimFramePos imagePos;   // 渲染偏移 { x, y }
    public int delay;               // 该帧持续时间（ms）
    public AnimBox damageBox;       // 受击盒（单数 = damageBoxes[0]，兼容旧 JSON/现有采样代码）
    public AnimBox[] damageBoxes;   // 受击盒全量（DNF 一帧可有多个；旧 JSON 无此字段 → null）
    public AnimBox[] attackBoxes;   // 攻击盒（DNF 一帧可有多个，如 kneekick 帧 1-3 各 2 个；无 → null）
}
```

JSON 由 `E:\Projects\cs\parse-img-ani\DnfConfigTranslation`（DNF .ani 翻译工具）生成，盒数据原样直译 DNF 像素（x=横向/y=纵深/z=高度），/100、y/z 轴映射、面左镜像由游戏运行时负责（`LSHitboxComponentSystem.SampleHurtBox`，已实证）。

> **待优化（暂缓）**：当前盒用嵌套 min/max 对象表示太啰嗦（一个盒 16 行），可优化为紧凑数字数组 `"damageBox": [-16,-7,-5,29,14,106]`。没现在改的原因：JsonUtility 不支持自定义转换器，数组→struct 需要把 `AnimFrameData` 字段改成 `int[]` + 语义化访问器（或换序列化方案），且阶段 2 实证的采样链路正在消费 `damageBox`——改动涉及 AnimClipData 结构 + 翻译工具输出 + 采样代码三方联动，放到后面统一做（做的时候翻译工具同步改）。

JSON 示例（配合 Unity JsonUtility 使用）：
```json
{
  "loop": true,
  "frameMax": 6,
  "frames": [
    {
      "index": 0, "image": { "path": "", "index": 0 }, "imagePos": { "x": 0, "y": 0 }, "delay": 50,
      "damageBox":  { "min": { "x": -16, "y": -7, "z": -5 }, "max": { "x": 29, "y": 14, "z": 106 } },
      "damageBoxes": [ { "min": { "x": -16, "y": -7, "z": -5 }, "max": { "x": 29, "y": 14, "z": 106 } } ],
      "attackBoxes": []
    }
  ],
  "totalDuration": 300
}
```

### AnimConfigRegistry — 配置注册表

```csharp
// 注册
AnimConfigRegistry.Register(AnimId.Idle, idleClipData);

// 获取
AnimClipData clip = AnimConfigRegistry.Get(AnimId.Idle);

// .als 特效叠加（DNF .ani 同名边车）：挂在父动画上
AnimConfigRegistry.RegisterOverlay(AnimId.SwordmanBloodboom, overlayConfig);
AnimOverlayConfig cfg = AnimConfigRegistry.GetOverlay(animId);   // 无 = null
```

### AnimOverlayConfig — .als 特效叠加配置

DNF `.als` 边车（如 `bloodboom.ani.als`）的翻译产物（`DnfConfigTranslation` 的 `als` 子命令生成，JsonUtility 反序列化）。父动画播放时，视图层 LSAnimOverlayViewComponent 按配置在指定帧/层叠加特效动画：

```csharp
[Serializable]
public class AnimOverlayConfig { public AnimOverlayEntry[] overlays; }

[Serializable]
public class AnimOverlayEntry
{
    public int startFrame;      // -1 = 全帧生效（DNF 帧号直译）
    public int z;               // 层号直译：负 = 身后；10001+ = 前景标记段（身前）
    public string effectAni;    // 特效动画别名（.als [use animation] 注册名）
    [NonSerialized] public int effectAnimId;   // 注册时由别名解析填充（0=未映射，视图层跳过该层）
}
```

### AnimId — 动画 ID 常量

| 常量 | 值 | 含义 |
|---|---|---|
| None | 0 | 无动画 |
| Idle | 1 | 待机 |
| Walk | 2 | 行走 |
| Attack1 | 3 | 普攻第一段（暂用班图膝踢 kneekick.json，帧 1-3 有攻击盒=判定帧） |
| Hurt | 4 | 受击僵直（damage.json，末帧长 delay 停帧、靠硬直计时切走） |
| NormalWave | 5 | 地裂波动剑投射物 |
| FireCircle | 6 | 火圈持续燃烧 |
| FireCircleEnd | 7 | 火圈熄灭收尾 |
| SwordmanIdle | 10 | 鬼剑士待机 |
| SwordmanWalk | 11 | 鬼剑士行走 |
| SwordmanAttack1~3 | 12-14 | 鬼剑士普攻三段 |
| SwordmanHurt | 15 | 鬼剑士受击 |
| SwordmanBloodboom | 16 | 鬼剑士浴血之怒施法（挂 bloodboom_cast_overlay） |
| BloodboomCastingBack | 17 | 浴血之怒施法蓄力背面层 |
| BloodboomCasting | 18 | 浴血之怒施法蓄力正面层 |
| BloodboomBoomFront | 19 | 浴血之怒爆炸正面（区域视图主层） |
| BloodboomBoomBack | 20 | 浴血之怒爆炸背面（区域视图背层） |

扩展方式：直接在 `AnimId` 类中添加新常量，或业务层自定义 ID。

## 典型调用流程（视图层）

```
1. 加载 .img 文件 → byte[]
2. NpkImgParser.Parse(bytes) → NpkSprite[]
3. NpkSprite → Unity Texture2D + Sprite（视图层负责）
4. 加载 .json 文件 → string
5. JsonUtility.FromJson<AnimClipData>(json)
6. AnimConfigRegistry.Register(animId, data)
```

参考调用方：`lockstep/Scripts/HotfixView/Client/LSAnimResComponentSystem.cs`

## 引用此包

**package.json**:
```json
"dependencies": { "cn.etetet.npkparser": "1.0.0" }
```

**asmdef**:
```json
"references": ["ET.NpkParser"]
```

**DotNet~ csproj**:
```xml
<ProjectReference Include="$(SolutionDir)Packages\cn.etetet.npkparser\DotNet~\ET.NpkParser.csproj" />
```
