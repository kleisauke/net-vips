[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$BuildArguments
)

Write-Output "PowerShell $($PSVersionTable.PSEdition) version $($PSVersionTable.PSVersion)"

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }

###########################################################################
# CONFIGURATION
###########################################################################

$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
$BuildProjectFile = "$PSScriptRoot\build\NetVips.Build.csproj"

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

Write-Output "Microsoft (R) .NET Core SDK version $(& dotnet --version)"

ExecSafe { & dotnet build $BuildProjectFile /nodeReuse:false }
ExecSafe { & dotnet run --project $BuildProjectFile --no-build -- $BuildArguments }
