$ErrorActionPreference = 'Stop'

$mods = 'C:\Program Files (x86)\Steam\steamapps\common\A Webbing Journey\Mods'
$unityExplorerDll = Join-Path $mods 'UnityExplorer.ML.Mono.dll'
if (-not (Test-Path $unityExplorerDll)) {
    throw 'UnityExplorer.ML.Mono.dll not found in Mods.'
}

$backupPath = $unityExplorerDll + '.bak_' + (Get-Date -Format 'yyyyMMdd_HHmmss')
Copy-Item $unityExplorerDll $backupPath -Force

$cecilCandidates = @(
    "$env:USERPROFILE\\.nuget\\packages\\mono.cecil\\0.11.5\\lib\\net40\\Mono.Cecil.dll",
    "$env:USERPROFILE\\.nuget\\packages\\mono.cecil\\0.10.4\\lib\\net40\\Mono.Cecil.dll",
    "$env:USERPROFILE\\.nuget\\packages\\mono.cecil\\0.11.6\\lib\\netstandard2.0\\Mono.Cecil.dll",
    "$env:USERPROFILE\\.nuget\\packages\\mono.cecil\\0.11.5\\lib\\netstandard2.0\\Mono.Cecil.dll",
    "$env:USERPROFILE\\.nuget\\packages\\mono.cecil\\0.10.4\\lib\\netstandard2.0\\Mono.Cecil.dll",
    "$env:USERPROFILE\\.nuget\\packages\\mono.cecil\\0.10.4\\lib\\net35\\Mono.Cecil.dll"
)
$cecilPath = $cecilCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $cecilPath) {
    throw 'Mono.Cecil.dll not found in NuGet cache.'
}

Add-Type -Path $cecilPath

$readerParams = New-Object Mono.Cecil.ReaderParameters
$readerParams.ReadWrite = $true
$readerParams.InMemory = $true
$asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($unityExplorerDll, $readerParams)
$stringType = $asm.MainModule.TypeSystem.String
$changed = $false

foreach ($attr in $asm.CustomAttributes) {
    if ($attr.AttributeType.FullName -eq 'MelonLoader.MelonGameAttribute') {
        if ($attr.ConstructorArguments.Count -ge 1 -and $null -eq $attr.ConstructorArguments[0].Value) {
            $attr.ConstructorArguments[0] = New-Object Mono.Cecil.CustomAttributeArgument($stringType, 'UNKNOWN')
            $changed = $true
        }

        if ($attr.ConstructorArguments.Count -ge 2 -and $null -eq $attr.ConstructorArguments[1].Value) {
            $attr.ConstructorArguments[1] = New-Object Mono.Cecil.CustomAttributeArgument($stringType, 'UNKNOWN')
            $changed = $true
        }
    }

    if ($attr.AttributeType.FullName -eq 'MelonLoader.MelonInfoAttribute') {
        if ($attr.ConstructorArguments.Count -ge 5 -and $null -eq $attr.ConstructorArguments[4].Value) {
            $attr.ConstructorArguments[4] = New-Object Mono.Cecil.CustomAttributeArgument($stringType, 'https://github.com/sinai-dev/UnityExplorer')
            $changed = $true
        }
    }
}

if ($changed) {
    $writerParams = New-Object Mono.Cecil.WriterParameters
    $asm.Write($unityExplorerDll, $writerParams)
    Write-Output ("Patched UnityExplorer metadata. Backup: " + $backupPath)
}
else {
    Write-Output 'No metadata changes were necessary.'
}

$asm.Dispose()
