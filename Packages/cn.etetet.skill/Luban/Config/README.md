# Skill Luban Config

`luban.conf` uses XML schema files and JSON data files. Run the generator from any directory with:

```powershell
pwsh -File Packages/cn.etetet.skill/Luban/Config/LubanGen.ps1
```

The schema inputs are the integer-ID JSON files under `Bundles/SkillParams`. Generated C# files are written to
`Runtime/SkillParamsGen`; do not edit generated files by hand.

The `ProbeData` directory contains the first Luban compatibility fixture and is not part of the production tables.
