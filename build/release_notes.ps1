#!/usr/bin/pwsh

# Create a release message and changelog.
# Assumes a tag has not yet been created.
param(
    $tag_name
)

if ($null -eq (Get-Module PowerShellForGitHub)) {
    Install-Module -Name PowerShellForGitHub -Force -Scope CurrentUser
    Import-Module PowerShellForGitHub
}

Set-GitHubConfiguration -DefaultOwnerName QutEcoacoustics -DefaultRepositoryName audio-analysis -SuppressTelemetryReminder
$secureString = ($env:RELEASE_NOTES_GITHUB_TOKEN | ConvertTo-SecureString)
$cred = New-Object System.Management.Automation.PSCredential "username is ignored", $secureString
Set-GitHubAuthentication -Credential $cred

function script:exec {
    [CmdletBinding()]

    param(
        [Parameter(Position = 0, Mandatory = 1)][scriptblock]$cmd,
        [Parameter(Position = 1, Mandatory = 0)][string]$errorMessage = ("Error executing command: {0}" -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw $errorMessage
    }
}

$old_tag_name = exec { git describe --abbrev=0 --always }
#$old_tag_date = exec { git log -1 --format=%ai $old_tag_name }


$compare_message = "[Compare $old_tag_name...$tag_name](https://github.com/QutBioacoustics/audio-analysis/compare/$old_tag_name...$tag_name)"





# list all issues and PRs



# list all commits
#  link author
# cite issues
