#!/usr/bin/pwsh

# builds and pushes a docker file to docker hub
param(
    # Version tag
    [Parameter(Mandatory = $true)]
    [string]
    $version,

    $commit
)
. $PSScriptRoot/log.ps1

log "Resolving commit '$commit'"

# get the current git commit
# this will resolve a name (like master/main) into a hash id
# and will return current commit id if empty reference provided
$commit = git log -1 --format=%H "$commit"

log "Commit resolved to '$commit'"

$env:DOCKER_BUILDKIT=1

log "Building container" "Building"

docker build `
    -t qutecoacoustics/audio-analysis:stable `
    -t qutecoacoustics/audio-analysis:latest `
    -t qutecoacoustics/audio-analysis:$version `
    . `
    --build-arg GIT_COMMIT=$commit `
    --build-arg AP_VERSION=$version `
    --build-arg CREATION_DATE=((Get-Date ).ToString("O"))
    --secret id=GITHUB_AUTH_TOKEN,env=GITHUB_AUTH_TOKEN

log "Pushing container" "Pushing"

docker push qutecoacoustics/audio-analysis:stable
