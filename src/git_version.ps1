#!/usr/bin/env pwsh
#Requires -Version 6

param(
    [string]$configuration,
    [string]$self_contained,
    [string]$runtime_identifier,
    [switch]$set_ci = $false
)

Push-Location
Set-Location $PSScriptRoot

$metadata_file = "AssemblyMetadata.Generated.cs"
$now = [System.DateTimeOffset]::UtcNow

# cache files in debug release
$cache_warning = ""
if ($build_type -ieq "Debug" -and !$set_ci) {
    $cache_warning = "// GENERATED_VALUES_MAY_BE_CACHED_IN_DEBUG_BUILD"
    $last_write = (Get-Item $metadata_file).LastWriteTimeUtc
    if (($now - $last_write) -lt [timespan]::FromMinutes(5)) {
        Write-Warning "Skipping AssemblyMetadata generation because we're in Debug and the file was generated less than five minutes ago"
        exit 0
    }
}

$self_contained = if ($self_contained -eq 'true') { 'true' } else { 'false' }

$commit_hash = git show -s --format="%H"

$branch = ((git show -s --pretty=%D HEAD) -split ',').Trim().TrimStart('origin/') | Where-Object { -not $_.Contains("HEAD") }


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

# if we're on the CI, register these variables
$prefix = ''
$seperator = '='
if ($set_ci) {
    $prefix = "##vso[task.setvariable variable=AP_"
    $seperator = "]"
}

$props = @"
${prefix}Year${seperator}$short_year
${prefix}Month${seperator}$short_month
${prefix}BuildDate${seperator}$build_date
${prefix}BuildNumber${seperator}$build_number
${prefix}CommitHash${seperator}$commit_hash
${prefix}CommitHashShort${seperator}$describe_hash
${prefix}Branch${seperator}$branch
${prefix}LastTag${seperator}$describe_tag
${prefix}CommitsSinceLastTag${seperator}$describe_commit_count
${prefix}TagsThisMonth${seperator}$tag_count_this_month
${prefix}IsDirty${seperator}$is_dirty
${prefix}Version${seperator}$version
${prefix}InformationalVersion${seperator}$informational_version
${prefix}GeneratedMetadata${seperator}$metadata_file
${prefix}CacheWarning${seperator}$cache_warning
${prefix}MsBuildSelfContained${seperator}$self_contained
${prefix}MsBuildRuntimeIdentifer${seperator}$runtime_identifier
${prefix}MsBuildConfiguration${seperator}$configuration
"@

Write-Output $props

Pop-Location