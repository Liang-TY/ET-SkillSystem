param(
    [ValidateSet("client", "all")]
    [string]$Target = "client",
    [string]$OutputCodeDir = "Packages/cn.etetet.skill/Runtime/SkillParamsGen"
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")).Path
$lubanDll = Join-Path $repoRoot "Packages\cn.etetet.yiuiluban\.Tools\Luban\Luban.dll"
$configFile = Join-Path $PSScriptRoot "luban.conf"
$customTemplateDir = Join-Path $repoRoot "Packages\cn.etetet.yiuiluban\.ToolsGen\Custom"

if (-not (Test-Path -LiteralPath $lubanDll)) {
    throw "Luban executable was not found: $lubanDll"
}

Push-Location $repoRoot
try {
    $lubanArgs = @(
        $lubanDll,
        "--conf", $configFile,
        "-t", $Target,
        "-c", "cs-code",
        "--customTemplateDir", $customTemplateDir,
        "-f",
        "--validationFailAsError",
        "-x", "outputCodeDir=$OutputCodeDir",
        "-x", "configGroup=SkillParams"
    )

    & dotnet @lubanArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Luban generation failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
