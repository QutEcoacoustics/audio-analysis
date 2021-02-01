#!/usr/bin/env pwsh
#Requires -Version 6

param(
    [string]$configuration,
    [string]$self_contained,
    [string]$runtime_identifier,
    [switch]$set_ci = $false,
    [switch]$json = $false,
    [switch]$env_vars = $false,
    [string]$prefix = ''
)

function construct_string($values) {

    $final += "$prologue"
    $total = $values.Count
    $index = 0
    foreach($key in $values.Keys) {
        $value = $values[$key]
        $thisDelimiter = $index + 1 -eq $total ? "" : $delimiter
        $final += "${prefix}${key}${seperator}${value}${suffix}${thisDelimiter}"

        $index++
    }
    $final += "$epilogue"
    return $final
}

class PathAndValue {
    [string]$Path
    [object]$Value

    PathAndValue(
        [string]$Path,
        [string]$Value
    ){
        $this.Path = $Path
        $this.Value = $Value
    }

    [string]ToString(){
        return $this.Value
    }
}


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

# process "HEAD -> github-actions, origin/github-actions"
$branch = (git show -s --pretty='%D' HEAD) -split ',' | Where-Object { -not $_.Contains("HEAD") } | % { $_ -replace '.*/','' }  | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($branch)) {
    $branch = git rev-parse --abbrev-ref HEAD
}


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


$values =  [ordered]@{
    "Year" = $short_year;
    "Month" = $short_month;
    "BuildDate" = $build_date;
    "BuildNumber" = $build_number;
    "CommitHash" = $commit_hash;
    "CommitHashShort" = $describe_hash;
    "Branch" = $branch;
    "LastTag" = $describe_tag;
    "CommitsSinceLastTag" = $describe_commit_count;
    "TagsThisMonth" = $tag_count_this_month;
    "IsDirty" = $is_dirty;
    "Version" = $version;
    "InformationalVersion" = $informational_version;
    "GeneratedMetadata" = $metadata_file;
    "CacheWarning" = $cache_warning;
    "MsBuildSelfContained" = $self_contained;
    "MsBuildRuntimeIdentifer" = $runtime_identifier;
    "MsBuildConfiguration" = $configuration;
}


$prologue = ''
#$prefix = '' - now a parameter
$seperator = '='
$suffix = ''
$delimiter = "`n"
$epilogue = ""

# if we're on the CI, register these variables
if ($set_ci) {
    $prefix = "##vso[task.setvariable variable=AP_" + $prefix
    $seperator = "]"
    Write-Output (construct_string $values)
}
elseif ($json) {
    $prologue = "{`n"
    $prefix = '    "' + $prefix
    $seperator = '": "'
    $suffix = '"'
    $delimiter = ",`n"
    $epilogue = "`n}"
    Write-Output (construct_string $values)
}
elseif ($env_vars) {
    # https://github.com/PowerShell/PowerShell/issues/5543
    # Due to a bug with set-content, it cna't bind to the property for a value in
    # an object, which makes the pipeline version rather intolerable.
    # Hence we're adding a toString() to our pscustomobject that ensures only
    # the value is represented when Set-Content calls toString
    foreach($key in $values.Keys) {
        $value = $values[$key]
        Write-Output ([PathAndValue]::new("Env:${prefix}$key", $value))
    }

}
else {
    Write-Output (construct_string $values)
}


Pop-Location

