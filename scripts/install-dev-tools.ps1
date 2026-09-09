$ErrorActionPreference = 'Stop'

function Get-LocalDotNetRoot {
    Join-Path $env:LocalAppData 'Microsoft\dotnet'
}

function Get-DotNetRoots {
    @(
        (Get-LocalDotNetRoot)
        'C:\Program Files\dotnet'
    ) | Where-Object { Test-Path (Join-Path $_ 'dotnet.exe') }
}

function Test-RootHasDotNet10([string]$root) {
    $sdks = & (Join-Path $root 'dotnet.exe') --list-sdks 2>$null
    return [bool]($sdks -match '^10\.')
}

function Use-DotNetRoot([string]$root) {
    $env:DOTNET_ROOT = $root
    $env:Path = "$root;$env:Path"
}

function Find-DotNet10Root {
    foreach ($root in Get-DotNetRoots) {
        if (Test-RootHasDotNet10 $root) { return $root }
    }
    return $null
}

function Install-DotNetSdk {
    Write-Host 'Installing .NET 10 SDK...'

    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if ($winget) {
        $previous = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        & winget install --id Microsoft.DotNet.SDK.10 -e --source winget `
            --accept-package-agreements --accept-source-agreements --disable-interactivity --scope user
        if (-not (Find-DotNet10Root)) {
            & winget install --id Microsoft.DotNet.SDK.10 -e --source winget `
                --accept-package-agreements --accept-source-agreements --disable-interactivity
        }
        $ErrorActionPreference = $previous
        $root = Find-DotNet10Root
        if ($root) { return $root }
        Write-Host 'Winget install did not provide a .NET 10 SDK; falling back to dotnet-install.ps1.'
    }

    $installer = Join-Path $env:TEMP 'dotnet-install.ps1'
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
    $installDir = Get-LocalDotNetRoot
    & $installer -Channel 10.0 -InstallDir $installDir
    return $installDir
}

$root = Find-DotNet10Root
if ($root) {
    Use-DotNetRoot $root
    Write-Host "Dev tools OK: $(& dotnet --version) ($root)"
    return
}

$root = Install-DotNetSdk
if (-not $root -or -not (Test-RootHasDotNet10 $root)) {
    throw 'Failed to install the .NET 10 SDK. Install it from https://dot.net/download and reload the window.'
}

Use-DotNetRoot $root
Write-Host "Dev tools installed: $(& dotnet --version) ($root)"
