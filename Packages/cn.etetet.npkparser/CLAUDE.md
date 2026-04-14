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
- IMG v2：ARGB1555、ARGB4444、ARGB8888 像素格式
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
}
```

JSON 示例（配合 Unity JsonUtility 使用）：
```json
{
  "loop": true,
  "frameMax": 6,
  "frames": [
    { "index": 0, "image": { "path": "", "index": 0 }, "imagePos": { "x": 0, "y": 0 }, "delay": 50 },
    { "index": 1, "image": { "path": "", "index": 1 }, "imagePos": { "x": 0, "y": 0 }, "delay": 50 }
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
```

### AnimId — 动画 ID 常量

| 常量 | 值 | 含义 |
|---|---|---|
| None | 0 | 无动画 |
| Idle | 1 | 待机 |
| Walk | 2 | 行走 |

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
