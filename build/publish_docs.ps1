Write-Output "Installing netlify CLI"
npm install netlify-cli -g

$l = $env:NETLIFY_AUTH_TOKEN.Length
Write-Output "Environment variable NETLIFY_AUTH_TOKEN is length $l"
if ($l -eq 0) {
    Write-Error "Netlify auth token not available, stopping"
    exit 1
}

$commit_hash = git show -s --format="%H"

try {
    Push-Location
    Set-Location "$PSScriptRoot/../_site"

    Write-Output "Deploying to netlify"


    netlify deploy --dir=. --message="Docs deploy for https://github.com/QutEcoacoustics/audio-analysis/commit/$commit_hash" --prod --site="078c0d59-a45a-4458-bd92-2d7c05f44bb6" --json

    Write-Output "Deploying complete"
}
finally {
    Pop-Location
}