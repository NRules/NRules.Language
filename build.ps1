param (
    [string]$target = 'Build',
    [string]$component = 'Core'
)

$version = '0.0.7'
$configuration = 'Release'

if (Test-Path Env:CI) { $version = $Env:APPVEYOR_BUILD_VERSION }
if (Test-Path Env:CI) { $configuration = $Env:CONFIGURATION }

$components = @{
    'NRules.RuleSharp' = @{
        name = 'NRules.RuleSharp'
        restore = @{
            tool = 'dotnet'
        }
        build = @{
            tool = 'dotnet'
        }
        test = @{
            location = 'NRules.RuleSharp.IntegrationTests'
            frameworks = @('net48', 'netcoreapp3.1')
        }
        bin = @{
            artifacts = @('netstandard2.0')
            'netstandard2.0' = @{
                include = @(
                    "NRules.RuleSharp\bin\$configuration\netstandard2.0"
                )
            }
        }
        package = @{
            nuget = @(
                'NRules.RuleSharp'
            )
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
    Invoke-psake .\tools\build\psakefile.ps1 $target -properties @{version=$version;configuration=$configuration;baseDir=$baseDir} -parameters @{component=$components[$_]}
    if (-not $psake.build_success) {
        break
    }
}