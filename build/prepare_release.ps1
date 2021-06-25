
param(
    [Parameter(Mandatory = $true)]
    [string]$temp_path
)
$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

. $PSScriptRoot/ci.ps1
. $PSScriptRoot/log.ps1
. $PSScriptRoot/exec.ps1

log "enable echo" "setup"
Set-EchoOn

log "set git credentials"
exec { git config --global user.email "ecoacoustics@qut.edu.au" }
exec { git config --global user.name "QUT Ecoacoustics" }


log "import build variables" "import build variables"
Get-Content "$temp_path/artifacts/AP_vars*/AP_vars.json" | ConvertFrom-Json | ForEach-Object { $_.Value | Set-Content $_.Path }
Set-CiOutput "AP_Version" "${env:AP_Version}"
Set-CiOutput "AP_CommitHash" "${env:AP_CommitHash}"

log "the latest tag"
git describe --abbrev=0

log "generate release notes" "generate release notes"
./build/release_notes.ps1 "v${env:AP_Version}" "$temp_path/release_notes.txt" -update_changelog

log "tag the release" "tag the release"
# release notes assumes we haven't made the new tag yet,
# we tag the version that the code was built against - not whatever commit is currently checked out!
exec { git tag -a -m "Version ${env:AP_Version}" "v${env:AP_Version}" "${env:AP_CommitHash}" }

log "save changes and push" "save changes and push"
exec { git add CHANGELOG.md }
exec { git commit -m "Update changelog for v${env:AP_Version}" -m "[skip ci]" }

exec { git push --follow-tags }

# debug
log "list artifacts:" "debug"
Get-ChildItem -Recurse "$temp_path/artifacts"

log "print env"
Get-ChildItem Env: | Format-Table | Write-Output

log "git status"
git status