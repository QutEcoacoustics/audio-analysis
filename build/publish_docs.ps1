

param(
    [switch]$prod
)

. $PSScriptRoot/ci.ps1
. $PSScriptRoot/exec.ps1
. $PSScriptRoot/log.ps1
$ErrorActionPreference = "Stop"




log "Installing netlify CLI" "Prepare"
npm install netlify-cli -g

$l = $env:NETLIFY_AUTH_TOKEN.Length
log "Environment variable NETLIFY_AUTH_TOKEN is length $l"
if ($l -eq 0) {
    Write-Error "Netlify auth token not available, stopping"
    exit 1
}

$commit_hash = git show -s --format="%H"
log "Start deploy" "Deploy"
try {
    Push-Location
    Set-Location "$PSScriptRoot/../_site"

    Write-Output "Deploying to netlify"

    $prod_arg = if ($prod) { "--prod" } else { "" }
    Write-Output "prod mode is $prod"

    # NETLIFY_AUTH_TOKEN used by this command
    exec { netlify deploy --dir=. --message="Docs deploy for https://github.com/QutEcoacoustics/audio-analysis/commit/$commit_hash" $prod_arg --site="078c0d59-a45a-4458-bd92-2d7c05f44bb6" --json } | Write-Output -OutVariable "deploy_result"

    log "Add github status check" "Status Check"

    $result = $deploy_result | ConvertFrom-Json
    $result | Write-Output

    Set-CiOutput "netlify_deploy_url" $result.deploy_url
    Set-CiOutput "netlify_url" $result.url

}
finally {
    Pop-Location
    finish_log
    Write-Output "Deploying complete"
}