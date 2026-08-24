# YamlDotNet（随包内置）

- 来源：NuGet 包 `YamlDotNet` 18.1.0，取 `lib/netstandard2.1/YamlDotNet.dll`
- 许可：MIT（https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt）
- 用途：`ET.UIBuilder.Editor` 程序集通过 asmdef 的
  `overrideReferences: true` + `precompiledReferences: ["YamlDotNet.dll"]` 引用，
  仅本包编辑器代码可见，不泄漏给其他程序集。
- 升级方式：从 NuGet 下载新版本 nupkg，替换本目录 dll（保持 netstandard2.1 目标）。
