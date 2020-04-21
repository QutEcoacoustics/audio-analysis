

if ($null -eq (Get-Command docfx -ErrorAction SilentlyContinue)) {
    Wite-output "Installing docfx..."
    #dotnet tool install -g docfx --version "3.0.0-*" --add-source https://www.myget.org/F/docfx-v3/api/v2
    choco install docfx -y

}

# if (-not ((docfx --version) -match "^3.0.*")) {
#     Write-Error "We require docfx version 3"
# }

Push-Location

try {
    Set-Location docs
    docfx build --log verbose


    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build docs, skipping deploy"
        exit 1
    }
}
finally {
    Pop-Location
}