#!/usr/bin/env pwsh

param(
    [Parameter(Position = 0, mandatory = $true)]
    $runtime_identifier
)

$ErrorView = "NormalView"
$ErrorActionPreference = "Stop"

. "$PSScriptRoot/log.ps1"
. "$PSScriptRoot/exec.ps1"


log "Installing dependencies" "Installing dependencies"

if ($IsLinux) {
    log "Installing linux dependencies"

    if ($runtime_identifier -ilike '*arm*') {
        sudo apt install qemu binfmt-support qemu-user-static
        sudo update-binfmts --display
    }

    log "Updating apt sources"
    exec { sudo apt-get update }
    
    log "Installing SoX"
    exec { sudo apt-get install -y libsox-fmt-all sox }

    log "Installing wavpack"
    exec { sudo apt-get install -y wavpack }

}
