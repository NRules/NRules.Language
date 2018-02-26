param (
    [string]$target = 'Build',
    [string]$component = 'Core'
)

$version = '0.0.1'
$configuration = 'Release'

if (Test-Path Env:CI) { $version = $Env:APPVEYOR_BUILD_VERSION }
if (Test-Path Env:CI) { $configuration = $Env:CONFIGURATION }

$components = @{
    'NRules.RuleSharp' = @{
        name = 'NRules.RuleSharp'
        restore = @{
            tool = 'nuget'
        }
        build = @{
            tool = 'dotnet'
        }
        test = @{
            location = 'NRules.RuleSharp.IntegrationTests'
            frameworks = @('net46')
        }
        bin = @{
            frameworks = @('net45')
            'net45' = @{
                include = @(
                    "NRules.RuleSharp\bin\$configuration"
                )
            }
        }
    };
}

$core = @('NRules.RuleSharp')

$componentList = @()
if ($component -eq "Core") {
    $componentList += $core
} elseif ($component -eq "All") {
    $componentList += $core
} else {
    $componentList += $component
}

Import-Module .\tools\build\psake.psm1
$baseDir = Resolve-Path .
$componentList | % {
    Invoke-psake .\tools\build\default.ps1 $target -properties @{version=$version;configuration=$configuration;baseDir=$baseDir} -parameters @{component=$components[$_]}
    if (-not $psake.build_success) {
        break
    }
}