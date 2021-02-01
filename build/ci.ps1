

function Test-CI() {
    $env:CI -eq "true"
}

function Set-CiOutput($key, $value) {
    Write-Output "::set-output name=$key::$value"
}