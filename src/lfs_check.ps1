#!/usr/bin/env pwsh
$cache_file = "$PSScriptRoot\.lfs_check_last_check"
if ([System.IO.File]::Exists($cache_file) -and [DateTime]::Now.Subtract([System.IO.File]::GetLastWriteTime($cache_file)).TotalSeconds -lt 3600) {
    Write-Output "Skipping lfs check because .lfs_check_last_check exists"
    exit 0
}

$solution_root = Resolve-Path "$PSScriptRoot/.."
$slash = [System.IO.Path]::DirectorySeparatorChar

if ($null -eq (Get-Command "git-lfs" -ErrorAction SilentlyContinue )) {
    Write-Output "$solution_root${slash}src${slash}AP.RequireLfsAssets.targets`:git-lfs error AP0003: git-lfs must be installed and must be available on PATH."
    exit;
}

Push-Location
Set-Location $solution_root
$errored = $false
foreach ($line in (git-lfs ls-files)) {
    # format:
    # 1da5b69f92 * tests/Fixtures/whip bird2.wav
    $status = $line[11]
    if ($status -eq '*') {
        continue
    }

    if ($status -eq '-') {
        $file = $line.SubString(13)
        $status = git status --porcelain $file
        $has_changed = ($null -ne $status)
        if (-not $has_changed) {
            Write-Output "$file`:git-lfs error AP001: Git LFS BLOB has not been restored. The file is empty! It contains only a LFS pointer."
            $errored = $true
        }
    }
}

if ($errored -eq $true) {
    Write-Output "$solution_root${slash}src${slash}AP.RequireLfsAssets.targets`:git-lfs error AP002: AP build cannot continue there are Git LFS assets that have not been restored. Please follow the instructions at https://github.com/QutEcoacoustics/audio-analysis/blob/master/CONTRIBUTING.md#AP001"
}
else {
    $null >> $cache_file
}

Pop-Location


exit 0