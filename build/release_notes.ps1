# Create a release message and update the changelog

param(
    [Parameter(Position = 0, mandatory = $true)]
    $tag_name,
    [switch]$update_changelog
)

$ErrorActionPreference = "Stop"
$DebugPreference = 'Continue'

function formatIssueList {
    process {
        #Write-debug $_.Issues.Count #+ ($_.Issues -join ","))
        $issues = $_.Issues

        $issue_string = if ($null -eq $issues -or $issues.Count -eq 0) {
            " - "
        }
        else {
            $issues | Join-String { "[$_](https://github.com/QutEcoacoustics/audio-analysis/issues/" + $_.TrimStart("#") + ")" } -Separator ", " -OutputPrefix " - (" -OutputSuffix ")`n  "
        }

        return "- " + ($_.Title -replace " - ", $issue_string)
    }
}

Write-Output "Creating release message"
# we assumed we've already tagged before describing this release
$old_tag_name = exec { git describe --abbrev=0 --always "HEAD^" }



$commit_summary = exec { git log --no-merges --pretty=format:">>>%h %an - %s%+b" "$old_tag_name...HEAD" -- . }

# look for issue refs in commits
$commits = @()
$current;
foreach ($line in $commit_summary) {
    if ($line.StartsWith(">>>")) {
        if ($null -ne $current) {
            $commits += $current
        }

        $current = [pscustomobject]@{Title = $line.Substring(3); Issues = @() }
    }

    $issue_refs = ($line | Select-String "#\d+" -AllMatches).Matches.Value | Where-Object { $null -ne $_ }
    $current.Issues += $issue_refs
    
}
$commits += $current
#Write-debug ($commits | format-table | out-string)
Write-debug ($commits.Count)
# include references to issues
$commit_list = $commits | formatIssueList | Join-String -Separator "`n"

# get any notes from the change log
$changelog_path = "$PSScriptRoot/../CHANGELOG.md" 
$changelog = Get-Content $changelog_path -Raw
# find notes and insertion point
$changelog_regex = "<!--manual-content-insert-here-->([\s\S]*)<!--generated-content-insert-here-->(\s)"
if (-not ($changelog -match $changelog_regex)) {
    throw "Changelog does not have insertion tokens"
}

$notes = [string]::IsNullOrWhiteSpace($Matches[1]) ? $null : "### Notes`n`n" + $Matches[1].Trim()

$release_title = "Ecoacoustics Audio Analysis Software $tag_name"
$changelog_title = "## $release_title $(Get-Date -Format 'yyyy-MM-dd')"
$compare_message = "[Compare $old_tag_name...$tag_name](https://github.com/QutBioacoustics/audio-analysis/compare/$old_tag_name...$tag_name)"

$release_message = @"
Version $tag_name

$notes

### Details

$compare_message

$commit_list
"@

$changelog_changes = "$changelog_title`n`n$release_message"
if ($update_changelog) {
    $replace = "<!--manual-content-insert-here-->`n`n`n`n<!--generated-con
    tent-insert-here-->`n`n$changelog_changes"
    $changelog -replace $changelog_regex, $replace | Out-File $changelog_path -Encoding utf8NoBOM
}
else {
    Write-Debug "Changelog changes:`n$changelog_changes"
}


# $env:ApReleaseMessage = $release_message
# $env:ApReleaseTitle = $release_title

Write-Debug "Release message:`n$release_title`n$release_message"



Write-Debug "commit count: $($commits.Count)"

Write-Output "##vso[task.setvariable variable=AP_ReleaseTitle]$release_title"
Write-Output "##vso[task.setvariable variable=AP_ReleaseMessage]$release_message"