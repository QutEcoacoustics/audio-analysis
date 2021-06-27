# Create a release message and update the changelog

param(
    [Parameter(Position = 0, mandatory = $true)]
    $tag_name,
    [Parameter(Position = 1, mandatory = $true)]
    $output_file,
    [switch]$update_changelog
)

$ErrorActionPreference = "Stop"
$DebugPreference = 'Continue'
. $PSScriptRoot/exec.ps1
. $PSScriptRoot/ci.ps1

Write-Debug "`$tag_name:$tag_name"
Write-Debug "`$output_file:$output_file"
Write-Debug "`$update_changelog:$update_changelog"

function formatIssueList {
    process {
        #Write-debug $_.Issues.Count #+ ($_.Issues -join ","))
        $issues = $_.Issues

        $issue_string = if ($null -eq $issues -or $issues.Count -eq 0) {
            " - "
        }
        else {
            # disabled code "nice" hard wrap because markdown was rendering with a newline in github release notes
            $issues | Join-String { "[$_](https://github.com/QutEcoacoustics/audio-analysis/issues/" + $_.TrimStart("#") + ")" } -Separator ", " -OutputPrefix " - (" -OutputSuffix ") "
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
            $current.Issues = $current.Issues | Select-Object -Unique
        }

        $current = [pscustomobject]@{Title = $line.Substring(3); Issues = @() }
    }

    $issue_refs = $line `
    | Select-String "#\d+" -AllMatches `
    | Select-Object -ExpandProperty Matches `
    | Select-Object -ExpandProperty Value `
    | Where-Object { '' -ne $_ }

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
    $replace = "<!--manual-content-insert-here-->`n`n`n`n<!--generated-content-insert-here-->`n`n$changelog_changes"
    $changelog -replace $changelog_regex, $replace | Out-File $changelog_path -Encoding utf8NoBOM
}
else {
    Write-Debug "Changelog changes:`n$changelog_changes"
}


# $env:ApReleaseMessage = $release_message
# $env:ApReleaseTitle = $release_title

Write-Debug "Release message:`n$release_title`n$release_message"
Write-Debug "commit count: $($commits.Count)"

Set-CiOutput "AP_ReleaseTitle" "$release_title"
Write-Output "Writing release notes to $output_file"
$release_message | Out-File $output_file -Encoding utf8NoBOM