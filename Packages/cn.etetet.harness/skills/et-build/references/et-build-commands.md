# ET9 Build 命令参考

## 编译

```bash
dotnet build ET.sln
```

唯一编译命令。不要用 IDE（Rider/VS）单独编译包。

## Proto 导出

```bash
dotnet Packages/cn.etetet.proto/DotNet~/Exe/ET.Proto2CS.dll
```

> 与 ET10 不同：ET10 路径为 `dotnet ./Bin/ET.Proto2CS.dll`

## 服务器启动

```bash
dotnet ./Bin/ET.App.dll --Console=1
```

必须在 Unity 项目根目录执行。启动前清理 `Logs/`。

## UBridge CLI 编译

```bash
dotnet build Packages/cn.etetet.ubridge/DotNet~/ET.UBridge.csproj
```

产物在 `Bin/ET.UBridge.dll`。

## 常见排查

| 问题 | 检查 |
|------|------|
| 编译报错 | 确认使用了 `ET.sln` 而非单独 csproj |
| Proto 生成失败 | 确认 `packages-lock.json` 中存在对应包条目 |
| 服务器启动失败 | 检查权限、端口占用、`Logs/` 目录 |
