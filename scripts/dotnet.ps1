$ErrorActionPreference = 'Stop'
& "$PSScriptRoot\install-dev-tools.ps1"
if ($LASTEXITCODE) { exit $LASTEXITCODE }
& dotnet @args
exit $LASTEXITCODE
