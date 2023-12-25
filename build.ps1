param (
    [string]$target = 'Build',
    [string]$component = 'Core'
)

$version = '0.0.9'
$configuration = 'Release'

if (Test-Path Env:CI) { $version = $Env:APPVEYOR_BUILD_VERSION }
if (Test-Path Env:CI) { $configuration = $Env:CONFIGURATION }

$components = @{
    'NRules.RuleSharp' = @{
        name = 'NRules.RuleSharp'
        solution_file = 'src\NRules.RuleSharp\NRules.RuleSharp.sln'
        package = @{
            bin = @{
                artifacts = @('netstandard2.0')
                'netstandard2.0' = @{
                    include = @(
                        "NRules.RuleSharp\bin\$configuration\netstandard2.0"
                    )
                }
            }
            nuget = @(
                'NRules.RuleSharp'
            )
        }
    };
    'Samples.GettingStarted' = @{
        name = 'GettingStarted'
        solution_file = 'samples\GettingStarted\GettingStarted.sln'
    };
}

$core = @('NRules.RuleSharp')
$samples = $components.keys | Where-Object { $_.StartsWith("Samples.") }

$componentList = @()
if ($component -eq "Core") {
    $componentList += $core
} elseif ($component -eq "Samples") {
    $componentList += $samples
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