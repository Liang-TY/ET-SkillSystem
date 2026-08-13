# RectpackSharp

第三方 2D 矩形打包库（MaxRects 算法），用于把多个不同尺寸的矩形高效塞进一个尽量小的方形容器里。

## 用途（本工程）

`LSAnimResComponentSystem.InitAsync` 里：把 `NpkImgParser.Parse` 解析出来的所有 `NpkSprite`（每个是一张序列帧）按尺寸打包进**单张运行时图集 `Texture2D`**，再 `Sprite.Create` 每帧为这张图的子区域。

为什么需要：所有帧共享一张纹理 → SpriteRenderer 之间合批成 1 个 draw call（一帧一个 Texture2D 会破坏合批）。

关键 API：
- `PackingRectangle`：矩形结构，含 `Id / X / Y / Width / Height`。`Id` 用来认领（Pack 后顺序不保证），本工程存 NpkSprite 的数组下标（= JSON 的 `image.index`）。
- `RectanglePacker.Pack(rects, out bounds, PackingHints.FindBest, 1, 1, maxW, maxH)`：打包。`bounds` 是实际包围盒（按它建图集大小，不浪费内存）；`maxW/maxH` 是上限（用 2048）。

## 来源

- 仓库：https://github.com/ThomasMiz/RectpackSharp
- 许可证：**MIT**（见各文件头部 Copyright）

## 改动（相对上游）

移除了 `PackingRectangle.cs` 对 `System.Drawing` 的依赖——Unity 运行时不带 `System.Drawing`，原库的 `Rectangle` 互转构造和隐式转换操作符会编译失败。删掉的只有那几个 `System.Drawing.Rectangle` 互转的便利成员，核心打包逻辑不受影响。

## 条件编译说明

源码用 `#if NET5_0_OR_GREATER / #elif NETSTANDARD2_0`。Unity 6 + .NET Standard 2.1 按规范累积定义 `NETSTANDARD2_0`，走数组重载分支。若换 Unity 版本后编译报错，看是不是这两个 define 都没命中。
