param (
    [hashtable] $component
)

properties {
    $version = $null
    $sdkVersion = "8.0.404"
    $sdkRuntimes = @("8.0.0", "6.0.0")
    $configuration = "Release"
    $baseDir = $null
}

include .\buildutils.ps1

task default -depends Build

task Init {
    Assert ($version -ne $null) 'Version should not be null'
    Assert ($component -ne $null) 'Component should not be null'
    Assert ($baseDir -ne $null) 'Base directory should not be null'
    
    Write-Host "Running '$taskList' target for $($component.name) version $version ($configuration)" -ForegroundColor Green
    
    $compName = $component.name
    
    $script:outsideDir = Split-Path $baseDir -Parent
    $script:sourceDir = Join-Path $baseDir src
    $script:buildDir = Join-Path $baseDir build
    $script:binDir = Join-Path $buildDir bin
    $script:pkgDir = Join-Path $buildDir packages
    $script:toolsDir = Join-Path $baseDir tools

    $script:solutionDir = Join-Path $sourceDir $compName
    $script:binOutDir = Join-Path $binDir $compName
    $script:pkgOutDir = Join-Path $pkgDir $compName
    
    $script:solutionFile = Join-Path $solutionDir "$($component.name).sln"
    if ($component.ContainsKey('solution_file')) {
        $script:solutionFile = Join-Path $baseDir "$($component.solution_file)"
        $script:solutionDir = Split-Path $script:solutionFile -Parent
    }
    
    $sdkLocation = Join-Path $toolsDir ".dotnet"
    Install-DotNetCli $sdkLocation $sdkVersion $sdkRuntimes
}

task Clean -depends Init {
    Remove-Directory $pkgOutDir
    Remove-Directory $binOutDir

    if ($component.ContainsKey('solution_file')) {
        try {
            exec { dotnet clean $solutionFile -c $configuration -v minimal } | Out-Null
        }
        catch {
            Write-Host "Clean failed for $solutionFile" -ForegroundColor Yellow
        }
    }

    if ($component.ContainsKey('output')) {
        foreach ($output in $component.output) {
            $outputDir = Join-Path $baseDir $output
            Write-Host "Removing $outputDir"
            Remove-Directory $outputDir
        }
    }
}

task PatchFiles {
    $signingKey = Join-Path $baseDir "SigningKey.snk"
    $secureKey = Join-Path $outsideDir "SecureSigningKey.snk"
    $secureHash = Join-Path $outsideDir "SecureSigningKey.sha1"
    
    if ((Test-Path $secureKey) -and (Test-Path $secureHash)) {
        Write-Host "Using secure signing key." -ForegroundColor Magenta
        $publicKey = Get-Content $secureHash
        $publicKey = $publicKey.Trim()
        Update-InternalsVisible $baseDir $publicKey
        Copy-Item $secureKey -Destination $signingKey -Force
    } else {
        Write-Host "Secure signing key does not exist. Using development key." -ForegroundColor Yellow
    }
}

task ResetPatch {
    $signingKey = Join-Path $baseDir "SigningKey.snk"
    $secureKey = Join-Path $outsideDir "SecureSigningKey.snk"
    $secureHash = Join-Path $outsideDir "SecureSigningKey.sha1"
    $devKey = Join-Path $baseDir "DevSigningKey.snk"
    $devHash = Join-Path $baseDir "DevSigningKey.sha1"
    
    if ((Test-Path $secureKey) -and (Test-Path $secureHash)) {
        $publicKey = Get-Content $devHash
        $publicKey = $publicKey.Trim()
        Update-InternalsVisible $baseDir $publicKey
        Copy-Item $devKey -Destination $signingKey -Force
    }
}

task RestoreTools {
    exec { dotnet tool restore -v minimal }
}

task Restore -precondition { return $component.ContainsKey('solution_file') } {
    New-Directory $pkgDir
    exec { dotnet restore $solutionFile -v minimal }
}

task Compile -depends Init, Restore -precondition { return $component.ContainsKey('solution_file') } { 
    New-Directory $buildDir
    exec { dotnet build $solutionFile --no-restore -c $configuration -p:Version=$version -p:ContinuousIntegrationBuild=true -v minimal }
}

task Test -depends Compile -precondition { return $component.ContainsKey('solution_file') } {
    Get-ChildItem $solutionDir -recurse -filter "*.trx" | % { Remove-File $_.fullname }
    
    $hasError = $false
    try {
        exec { dotnet test $solutionFile --no-restore --no-build -c $configuration -v minimal --logger "trx;LogFilePrefix=testResults" }
    }
    catch {
        $hasError = $true
    }
    
    if (Test-Path Env:CI) {
        Write-Host "Uploading test results to CI server"
        $wc = New-Object 'System.Net.WebClient'
        Get-ChildItem $solutionDir -recurse -filter "*.trx" | % {
            $testFile = $_.fullname
            $wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$($Env:APPVEYOR_JOB_ID)", (Resolve-Path $testFile))
        }
    }
    
    if ($hasError) {
        throw "Test task failed"
    }
}

task PackageNuGet -depends Compile -precondition { $component.package.ContainsKey('nuget') -and $component.ContainsKey('solution_file') } {
    New-Directory $pkgOutDir
    exec { dotnet pack $solutionFile -c $configuration --no-restore --no-build -p:Version=$version -o $pkgOutDir -v minimal }
}

task PackageBin -depends Compile -precondition { $component.package.ContainsKey('bin') } {
    New-Directory $binOutDir
    $bin = $component.package.bin
    foreach ($artifact in $bin.artifacts) {
        $outputDir = $artifact
        if ($bin.$artifact.ContainsKey('output')) {
            $outputDir = $bin.$artifact.output
        }
        $destDir = Join-Path $binOutDir $outputDir
        New-Directory $destDir
        foreach ($item in $bin.$artifact.include) {
            $itemPath = Join-Path $solutionDir $item
            if (Test-Path -Path $itemPath -PathType Container) {
                $itemPath = "$itemPath\**"
            }
            Get-ChildItem $itemPath | Copy-Item -Destination $destDir -Force
        }
    }
}

task Package -depends PackageNuGet, PackageBin -precondition { return $component.ContainsKey('package') } {
}

task CompileDocs -depends RestoreTools -precondition { return $component.ContainsKey('doc') -and $component.doc.ContainsKey('docfx') } {
    $docfx_project_file = Join-Path $baseDir $component.doc.docfx.project_file
    exec { dotnet tool run docfx $docfx_project_file }
}

task Make -depends Init, Restore, Compile, Test, Package, CompileDocs -precondition { return IsCompatibleOs $component.os } {
}

task Build -depends Init, Clean, PatchFiles, Make, ResetPatch {
}

task Bench -depends Build -precondition { return $component.ContainsKey('bench') } {
    $benchRunner = $component.bench.runner
    $categories = $component.bench.categories -join ","
    foreach ($framework in $component.bench.frameworks) {
        $benchRunnerFmkPath = Join-Path $binOutDir $framework
        $benchRunnerPath = Join-Path $benchRunnerFmkPath $benchRunner
        $benchDir = Join-Path $buildDir bench
        $artifacts = Join-Path $benchDir $framework
        Push-Location $solutionDir
        exec { &$benchRunnerPath --join --anyCategories=$categories --artifacts=$artifacts }
        Pop-Location
    }
}

task PushNuGet -precondition { return $component.ContainsKey('package') -and $component.package.ContainsKey('nuget') } {
    $accessKeyFile = Join-Path $outsideDir "Nuget-Access-Key.txt"
    if ( (Test-Path $accessKeyFile) ) {
        $accessKey = Get-Content $accessKeyFile
        $accessKey = $accessKey.Trim()
        
        foreach ($package in $component.package.nuget) {
            $nupkg = Join-Path $pkgOutDir "$($package).$($version).nupkg"
            exec { dotnet nuget push $nupkg --api-key $accessKey }
        }
    } else {
        Write-Host "Nuget-Access-Key.txt does not exist. Cannot publish the nuget package." -ForegroundColor Yellow
    }
}

task Publish -depends Init, PushNuGet {
}
