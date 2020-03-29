#!/usr/bin/env pwsh

#Requires -Version 6

param(
    
    [string]$configuration,
    [string]$self_contained,
    [string]$runtime_identifier
)

Push-Location
Set-Location $PSScriptRoot

$metadata_file = "AssemblyMetadata.Generated.cs"
$now = [System.DateTimeOffset]::UtcNow

# cache files in debug release
$cache_warning = ""
if ($build_type -ieq "Debug") {
    $cache_warning = "// GENERATED_VALUES_MAY_BE_CACHED_IN_DEBUG_BUILD"
    $last_write = (Get-Item $metadata_file).LastWriteTimeUtc
    if (($now - $last_write) -lt [timespan]::FromMinutes(5)) {
        Write-Warning "Skipping AssemblyMetadata generation because we're in Debug and the file was generated less than five minutes ago"
        exit 0
    }
}

$self_contained = if ($self_contained -eq 'true') { 'true' } else { 'false' }

$commit_hash = git show -s --format="%H"

$branch = git rev-parse --abbrev-ref HEAD

$describe = git describe --dirty --abbrev --long --always

$describe_tag, $describe_commit_count, $describe_hash, $describe_dirty = $describe.Split("-")
$is_dirty = if ($null -ne $describe_dirty) { 'true' } else { 'false' }
$dirty = if ($is_dirty) { 'DIRTY' } else { '' }

$year = $now.Year
$month = $now.Month
$short_year = $now.ToString("yy")
$short_month = $now.ToString("%M")
$build_date = $now.ToString("O")
$build_number = if ($null -eq ${env:BUILD_BUILDID}) { "000" } else { ${env:BUILD_BUILDID} }

$tags_this_month = git log --tags --simplify-by-decoration --first-parent --pretty="format:%ai %d" --after="$year-$month-01T00:00Z"
$tag_count_this_month = $tags_this_month.Count

$version = "$short_year.$short_month.$tag_count_this_month.$describe_commit_count"
$informational_version = "$version-$branch-$commit_hash-$dirty-CI:$build_number"

$template_file = "AssemblyMetadata.cs.template"
$content = Get-Content -Raw $template_file
$templated = $ExecutionContext.InvokeCommand.ExpandString($content)
$templated | Out-File $metadata_file -Force -Encoding utf8NoBOM

$props = @"
Year=$short_year
Month=$short_month
BuildDate=$build_date
BuildNumber=$build_number
CommitHash=$commit_hash
CommitHashShort=$describe_hash
Branch=$branch
LastTag=$describe_tag
CommitsSinceLastTag=$describe_commit_count
TagsThisMonth=$tag_count_this_month
IsDirty=$is_dirty
Version=$version
InformationalVersion=$informational_version
GeneratedMetadata=$metadata_file
CacheWarning=$cache_warning
MsBuildSelfContained=$self_contained
MsBuildRuntimeIdentifer=$runtime_identifier
MsBuildConfiguration=$configuration
"@

Write-Output $props

Pop-Location