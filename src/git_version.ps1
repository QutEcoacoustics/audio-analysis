#!/usr/bin/env pwsh

#Requires -Version 6

param(
    [string]$configuration = "Release"
)

Push-Location
Set-Location $PSScriptRoot

$metadata_file = "AssemblyMetadata.Generated.cs"
$props_file = "AssemblyMetadata.Generated.targets"
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

$commit_hash = git show -s --format="%H"

$branch = git rev-parse --abbrev-ref HEAD



$describe = git describe --dirty --abbrev --long --always

$describe_tag, $describe_commit_count, $describe_hash, $describe_dirty = $describe.Split("-")
$is_dirty = if ($null -ne $describe_dirty) { 'true' } else { 'false' }
$dirty = if ($is_dirty) { 'DIRTY'} else { '' }


$year = $now.Year
$month = $now.Month
$short_year = $now.ToString("yy")
$short_month = $now.ToString("%M")
$build_date = $now.ToString("O")
$build_number = if ($null -eq  ${env:BUILD_BUILDID}) { "000" } else { ${env:BUILD_BUILDID} }

$tags_this_month = git log --tags --simplify-by-decoration --first-parent --pretty="format:%ai %d" --after="$year-$month-01T00:00Z"
$tag_count_this_month = $tags_this_month.Count

$version = "$short_year.$short_month.$tag_count_this_month.$describe_commit_count"
$informational_version = "$version-$branch-$commit_hash-$dirty-CI:$build_number"

$template_file = "AssemblyMetadata.cs.template"
$content = Get-Content -Raw $template_file
$templated = $ExecutionContext.InvokeCommand.ExpandString($content)
$templated | Out-File $metadata_file -Force -Encoding utf8NoBOM

$props =  @"
<?xml version="1.0" encoding="utf-8"?>
<Project>
    <Target Name="APVersionLoadProperties" AfterTargets="APVersionBeforeBuild">
        <PropertyGroup>
            <Year>$short_year</Year>
            <Month>$short_month</Month>
            <BuildDate>$build_date</BuildDate>
            <BuildNumber>$build_number</BuildNumber>
            <CommitHash>$commit_hash</CommitHash>
            <CommitHashShort>$describe_hash</CommitHashShort>
            <Branch>$branch</Branch>
            <LastTag>$describe_tag</LastTag>
            <CommitsSinceLastTag>$describe_commit_count</CommitsSinceLastTag>
            <TagsThisMonth>$tag_count_this_month</TagsThisMonth>
            <IsDirty>$is_dirty</IsDirty>
            <Version>$version</Version>
            <InformationalVersion>$informational_version</InformationalVersion>
            <GeneratedMetadata>$metadata_file</GeneratedMetadata>
            <GeneratedProps>$props_file</GeneratedProps>
        </PropertyGroup>
    </Target>
</Project>
"@

$props | Out-File $props_file -Force -Encoding utf8NoBOM

Write-Output $props

Pop-Location