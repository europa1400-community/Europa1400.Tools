param(
    [string]$EnvFile,
    [switch]$NonInteractive
)

$ErrorActionPreference = 'Stop'
$SolutionRoot = Split-Path $PSScriptRoot -Parent
if (-not $EnvFile) { $EnvFile = Join-Path $SolutionRoot '.env' }

$Quote = [char]34
$Variables = @(
    @{
        Name        = 'GAME_DIRECTORY_PATH'
        Kind        = 'Directory'
        Title       = 'Europa 1400 install folder'
        Description = 'Folder that contains Data, Resources, gfx and sfx (Gold Edition).'
        Markers     = @(
            'Resources\objects.bin'
            'Resources\animations.bin'
            'Data\A_Geb.dat'
        )
    }
)

function Test-Interactive {
    if ($NonInteractive) { return $false }
    if ($env:CI -eq 'true') { return $false }
    return [Environment]::UserInteractive
}

function Read-DotEnv([string]$path) {
    $map = [ordered]@{}
    if (-not (Test-Path $path)) { return $map }
    foreach ($line in Get-Content -LiteralPath $path -Encoding UTF8) {
        $trim = $line.Trim()
        if ($trim -eq '' -or $trim.StartsWith('#')) { continue }
        $eq = $trim.IndexOf('=')
        if ($eq -lt 1) { continue }
        $key = $trim.Substring(0, $eq).Trim()
        $value = $trim.Substring($eq + 1).Trim()
        if ($value.Length -ge 2) {
            $q = $value[0]
            if (($q -eq $Quote -or $q -eq [char]39) -and $value[$value.Length - 1] -eq $q) {
                $value = $value.Substring(1, $value.Length - 2)
            }
        }
        $map[$key] = $value
    }
    return $map
}

function Format-DotEnvValue([string]$value) {
    if ($value -notmatch '[\s#=]') { return $value }
    return $Quote + $value.Replace([string]$Quote, '\' + $Quote) + $Quote
}

function Write-DotEnv([string]$path, $values) {
    $lines = @(
        '# Local Europa 1400 Tools settings. Not committed.'
        "# Created by scripts/ensure-env.ps1 on $(Get-Date -Format 'yyyy-MM-dd')."
    )
    foreach ($key in $values.Keys) {
        $lines += "$key=$(Format-DotEnvValue $values[$key])"
    }
    $utf8 = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllLines($path, $lines, $utf8)
}

function Test-DirectoryVariable($spec, [string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return 'Value is empty.' }
    if (-not (Test-Path -LiteralPath $value -PathType Container)) {
        return "Folder does not exist: $value"
    }
    $missing = @($spec.Markers | Where-Object { -not (Test-Path -LiteralPath (Join-Path $value $_)) })
    if ($missing.Count -gt 0) {
        return "Not an Europa 1400 install (missing $($missing -join ', '))."
    }
    return $null
}

function Test-Variable($spec, [string]$value) {
    switch ($spec.Kind) {
        'Directory' { Test-DirectoryVariable $spec $value }
        default {
            if ([string]::IsNullOrWhiteSpace($value)) { 'Value is empty.' } else { $null }
        }
    }
}

function Ensure-FolderPickerType {
    if ('NativeFolderPicker' -as [type]) { return }
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing
    $refs = @(
        [System.Windows.Forms.OpenFileDialog].Assembly.Location
        [System.Drawing.Point].Assembly.Location
    )
    Add-Type -Path (Join-Path $PSScriptRoot 'NativeFolderPicker.cs') -ReferencedAssemblies $refs
}

function Select-Folder([string]$title, [string]$initialDirectory) {
    Ensure-FolderPickerType
    $start = $null
    if ($initialDirectory) {
        if (Test-Path -LiteralPath $initialDirectory -PathType Container) { $start = $initialDirectory }
        else {
            $parent = Split-Path $initialDirectory -Parent
            if ($parent -and (Test-Path -LiteralPath $parent -PathType Container)) { $start = $parent }
        }
    }
    try { return [NativeFolderPicker]::Pick($title, $start) }
    catch { Write-Host "Folder dialog failed ($($_.Exception.Message)). Type the path instead." }
    return $null
}

function Read-PathFromHost($spec, [string]$current) {
    $hint = if ($current) { " [$current]" } else { '' }
    Write-Host "$($spec.Name): $($spec.Description)"
    $typed = Read-Host "Path$hint"
    if ([string]::IsNullOrWhiteSpace($typed)) { return $current }
    return $typed.Trim($Quote)
}

function Request-Directory($spec, [string]$current) {
    Write-Host ''
    Write-Host "$($spec.Name) - $($spec.Description)"
    Write-Host 'A folder dialog should open. Pick the Europa 1400 install directory.'
    $picked = Select-Folder $spec.Title $current
    if ($picked) { return $picked }
    return Read-PathFromHost $spec $current
}

function Request-Variable($spec, [string]$current) {
    switch ($spec.Kind) {
        'Directory' { Request-Directory $spec $current }
        default {
            $hint = if ($current) { " [$current]" } else { '' }
            Write-Host "$($spec.Name): $($spec.Description)"
            $typed = Read-Host "Value$hint"
            if ([string]::IsNullOrWhiteSpace($typed)) { return $current }
            return $typed
        }
    }
}

$values = Read-DotEnv $EnvFile
$missingFile = -not (Test-Path -LiteralPath $EnvFile)
$changed = $false

foreach ($spec in $Variables) {
    $current = $values[$spec.Name]
    if (-not $current) { $current = [Environment]::GetEnvironmentVariable($spec.Name) }

    $errorMessage = Test-Variable $spec $current
    if (-not $errorMessage) {
        $values[$spec.Name] = $current
        Write-Host "$($spec.Name) OK: $current"
        continue
    }

    if ($missingFile) { Write-Host ".env not found at $EnvFile" }
    else { Write-Host "$($spec.Name) invalid: $errorMessage" }

    if (-not (Test-Interactive)) {
        Write-Host "Set $($spec.Name) in $EnvFile. $($spec.Description)"
        exit 1
    }

    while ($true) {
        $current = Request-Variable $spec $current
        $errorMessage = Test-Variable $spec $current
        if (-not $errorMessage) {
            $values[$spec.Name] = $current
            $changed = $true
            break
        }
        Write-Host $errorMessage
        if (-not $current) {
            Write-Host "Cancelled. Set $($spec.Name) in $EnvFile and re-run."
            exit 1
        }
    }
}

if ($changed -or $missingFile) {
    Write-DotEnv $EnvFile $values
    Write-Host "Wrote $EnvFile"
}
else {
    Write-Host '.env OK'
}
