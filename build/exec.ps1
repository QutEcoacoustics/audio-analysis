. $PSScriptRoot/ci.ps1

function script:exec {
    [CmdletBinding()]

    param(
        [Parameter(Position = 0, Mandatory = 1)][scriptblock]$cmd,
        [Parameter(Position = 1, Mandatory = 0)][string]$errorMessage,

        [switch]$WhatIf = $false
    )
    if ($WhatIf) {
        $InformationPreference = 'Continue'
        Write-Information "Would execute `"$cmd`""
        return;
    }

    if (Test-CI) {
        Write-Information = "Executing `"$cmd`""
    }

    & $cmd
    if ($LASTEXITCODE -ne 0) {
        throw ("Error ($LASTEXITCODE) executing command: {0}" -f $cmd) + ($errorMessage ?? "")
    }
}
