#Requires -Version 7

. $PSScriptRoot/ci.ps1

$script:current_section = $null

function script:log {
    [CmdletBinding()]

    param(
        [Parameter(Position = 0, Mandatory = 1)][string]$message,
        [Parameter(Position = 1, Mandatory = 0)][string]$section = ""
    )


    # $null - reset group
    # ""    - maintain group
    # *     - anything else, new group
    if ($section -ne "") {
        # reset group
        if ((is_CI) -and $null -ne $script:current_section) {
            Write-Output "::endgroup::"
        }

        # update state
        $script:current_section = $section

        # if starting a new group
        if ($null -ne $section -and (is_CI)) {
            # emit new section
            Write-Output "::group::$script:current_section"
        }
    }

    $tag = $null -ne  $script:current_section ? "[$script:current_section] " : ""
    Write-Output "${tag}$message"
}

function script:finish_log {
    [CmdletBinding()]
    param()

    $script:current_section = $null
    if ((is_CI)) {
        Write-Output "::endgroup::"
    }
}