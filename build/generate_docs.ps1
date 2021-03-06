. $PSScriptRoot/exec.ps1
. $PSScriptRoot/log.ps1
$ErrorActionPreference = "Stop"

Push-Location

Set-Location "$PSScriptRoot/../"

try {
    if ($null -eq (Get-Command docfx -ErrorAction SilentlyContinue)) {
        log "Installing docfx..." "Install tools"
        #dotnet tool install -g docfx --version "3.0.0-*" --add-source https://www.myget.org/F/docfx-v3/api/v2
        exec { choco install docfx wkhtmltopdf -y --limit-output --no-progress }
    }

    # if (-not ((docfx --version) -match "^3.0.*")) {
    #     Write-Error "We require docfx version 3"
    # }

    log "Extracting git version metadata" "Prepare metadata"
    & $PSScriptRoot/../src/git_version.ps1 -json -prefix "AP_" | Tee-Object -FilePath "$PSScriptRoot/../docs/apMetadata.json"
    log "Extracting git version metadata (ENVIRONMENT VARIABLES)"
    & $PSScriptRoot/../src/git_version.ps1 -env_vars -prefix "AP_" | set-content

    Set-Location docs

    log "Prepare API metadata for docs"

    exec { docfx metadata }

    log "Building pdf docs" "Build PDF"
    exec { docfx pdf  --log verbose }

    log "moving pdf" "PDF"
    $pdf_name = "ap_manual_${Env:AP_Version}.pdf"
    Move-Item "ap_manual_pdf.pdf" $pdf_name -Force

    log "generating pdf xref files"
    $xref_content = @"
    {"references":[{"uid":"invariant_ap_manual_ref","name":"Download AP PDF","href":"$pdf_name","fullName":"PDF Download for AP.exe docs version ${Env:AP_Version}"}]}
"@
    $xref_content | Set-Content -Encoding utf8NoBOM -Path "pdf_xrefmap.yml" -Force


    log "Building docs" "Build docs"
    exec { docfx build --log verbose --force }

    log "✅ Doc generation success"
}
catch {
    Write-Output $_
    Write-Error "Failed to build docs."
}
finally {
    Pop-Location
    finish_log
}
