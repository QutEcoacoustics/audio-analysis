#!/usr/bin/pwsh

# builds and pushes a docker file to docker hub
# currently only builds the 'stable' tag, which is applied to whatever version
# is supplied to this script.
# Future work: allow this script to build our 'Weekly' and 'Continuous' lines
# as their own containers.
param(
    # Version tag
    [Parameter(Mandatory=$true)]
    [string]
    $version 
)

# get the current git commit
$GIT_COMMIT=git log -1 --format=%H

docker build `
    -t qutecoacoustics/audio-analysis:stable `
    -t qutecoacoustics/audio-analysis:latest `
    -t qutecoacoustics/audio-analysis:$version `
    . `
    --build-arg GIT_COMMIT=$GIT_COMMIT `
    --build-arg AP_SOURCE="github" `
    --build-arg AP_VERSION=$version

docker push qutecoacoustics/audio-analysis:stable
