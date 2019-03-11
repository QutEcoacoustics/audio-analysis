#!/usr/bin/pwsh

#Requires -Version 5

# Tests for the download_ap.ps1 script

if (! (Get-Module -ListAvailable -Name Pester)) {
    # warning this may need admin rights
    Install-Module -Name Pester -Force -SkipPublisherCheck
}

Import-Module Pester

function Invoke {
    . $PSScriptRoot\download_ap.ps1 @args
}

Describe 'AP.exe installer script' {
    $expected_gh_version = Get-Date -Format "vyy.*"

    Context 'Testing the default download' {
        It 'downloads the stable release by default' {
            $result = Invoke -WhatIf
            $result.Type | Should -Be "Default"
            $result.Source | Should -Be "Github"
            $result.ResolvedVersion | Should -BeLike $expected_gh_version
            $result.AssetUrl | Should -BeLike '*github.com*'
        }
    }

    Context 'Testing the pre-defined downloads' {
        It 'can download the stable release' {
            $result = Invoke -package Stable -WhatIf
            $result.Type | Should -Be "Default"
            $result.Source | Should -Be "Github"
            $result.ResolvedVersion | Should -BeLike $expected_gh_version
            $result.AssetUrl | Should -BeLike '*github.com*'
        }
        It 'can download the weekly release' {
            $result = Invoke -package weekly -WhatIf
            $result.Type | Should -Be "Default"
            $result.Source | Should -Be "Github"
            $result.ResolvedVersion | Should -BeLike $expected_gh_version
            $result.AssetUrl | Should -BeLike '*github.com*'
        }
        It 'can download the continuous release' {
            $result = Invoke -package conTinuous -WhatIf
            $result.Type | Should -Be "Default"
            $result.Source | Should -Be "AppVeyor"
            $result.ResolvedVersion | Should -BeGreaterThan 500
            $result.AssetUrl | Should -BeLike '*appveyor.com*'
        }
    }

    Context 'Testing the exact version downloader' {
        It 'can download an exact version from GitHub' {
            $result = Invoke -version '18.8.1.2' -WhatIf
            $result.Type | Should -Be "GitHub"
            $result.Source | Should -Be "github"
            $result.ResolvedVersion | Should -Be 'v18.8.1.2'
            $result.AssetUrl | Should -BeLike '*github.com*'
        }
        It 'can download a build from AppVeyor' {
            $result = Invoke -ci_build_number 678 -WhatIf
            $result.Type | Should -Be "AppVeyor"
            $result.Source | Should -Be "appveyor"
            $result.ResolvedVersion | Should -Be '678'
            $result.AssetUrl | Should -BeLike '*appveyor.com*'
        }
    }

    Context 'Installing the files' {
        BeforeAll {
            $here = [System.IO.Path]::GetFullPath("$PSScriptRoot/test_download")
            
            # remove any path modifications from previous test runs
            $env:Path = $env:Path -replace "$here",''
            $paths = [Environment]::GetEnvironmentVariables("User").Path -split [IO.Path]::PathSeparator
            $paths = $paths | Where-Object { $_ -ne $here }
            $user_path = ($paths -join [IO.Path]::PathSeparator)
            [Environment]::SetEnvironmentVariable("Path", $user_path, "User")

            $result_all = Invoke -Destination $here
            
        }
        It 'defaults to installing into /AP' {
            $result = Invoke -WhatIf
            $result.Destination | Should -BeIn ("/AP", "C:\AP")
        }
        It 'updates path on install' {
            $env:Path | Should -BeLike "*$here*"
            # Enable this test after we bootstrap the releases
            #(Get-Command "download_ap.ps1" -erroraction 'silentlycontinue' ) | Should -Not -BeNullOrEmpty
        }
        It 'can install to another directory' {
            $result_all.Destination | Should -Be (Resolve-Path $here).Path
            ("$here/AnalysisPrograms.exe") | Should -Exist
            ("$here/*.Zip") | Should -Not -Exist
        }
        It 'checks the environment when it installs' {
            $result_all.EnvironmentCheck | Should -BeExactly 0
        }
        It 'can update itself' {
            $update_result = Invoke -Destination (Resolve-Path $here).Path
            ("$here/AnalysisPrograms.exe") | Should -Exist
            # Enable this test after we bootstrap the releases
            #("$here/download_ap.ps1") | Should -Exist
            ("$here/*.Zip") | Should -Not -Exist
        }

        AfterAll {
            Remove-Item $here -Recurse
        }
    }
}