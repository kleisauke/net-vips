[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$BuildArguments
)

Write-Output "PowerShell $($PSVersionTable.PSEdition) version $($PSVersionTable.PSVersion)"

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

###########################################################################
# CONFIGURATION
###########################################################################

$BuildProjectFile = "$PSScriptRoot\build\NetVips.Build.csproj"

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:DOTNET_MULTILEVEL_LOOKUP = 0
$env:NUKE_TELEMETRY_OPTOUT = 1

###########################################################################
# EXECUTION
###########################################################################

function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

if ((Get-Command "dotnet" -ErrorAction SilentlyContinue) -eq $null) {
	Write-Output "Error: dotnet is not installed."
	exit 1
}

Write-Output "Microsoft (R) .NET SDK version $(& dotnet --version)"

ExecSafe { & dotnet build $BuildProjectFile /nodeReuse:false /p:UseSharedCompilation=false /nologo /clp:NoSummary --verbosity quiet }
ExecSafe { & dotnet run --project $BuildProjectFile --no-build -- $BuildArguments }
