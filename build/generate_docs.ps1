

if ($null -eq (Get-Command docfx -ErrorAction SilentlyContinue)) {
    Write-output "Installing docfx..."
    #dotnet tool install -g docfx --version "3.0.0-*" --add-source https://www.myget.org/F/docfx-v3/api/v2
    choco install docfx -y

}

# if (-not ((docfx --version) -match "^3.0.*")) {
#     Write-Error "We require docfx version 3"
# }

Write-Output "Extracting git version metadata"
. $PSScriptRoot/../src/git_version.ps1 | Split-String "`n", "`r"  -RemoveEmptyStrings | ForEach-Object { $result = @{ } } {
    $key, $value = $_ -split "="
    $result.Add("AP_$key", $value )
} { $result } | ConvertTo-JSON | Out-File "$PSScriptRoot/../docs/apMetadata.json"




try {
    Write-Output "Startign docs build"
    Push-Location
    Set-Location docs

    docfx metadata

    docfx build --log verbose


    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build docs, skipping deploy"
        exit 1
    }
}
finally {
    Pop-Location
}