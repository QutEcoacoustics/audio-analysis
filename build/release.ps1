# Create a release message (and optionally create a GitHub release)
#
# depends on hub being on path (`choco install hub`)
# 
# This script has been modified to work with our CI server

param($tag_name, [bool]$ci = $false, [bool]$pre_release = $true)
$ErrorActionPreference = "Continue"



function Check-Command($cmdname) {
	return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}
if (!$ci -and !(Check-Command hub)) {
	throw "Cannot find needed executable dependencies";
}


echo "Creating release message"
# we assumed we've already tagged before describing this release
$old_tag_name = exec { git describe --abbrev=0 --always "$tag_name^" }

$compare_message = "[Compare $old_tag_name...$tag_name](https://github.com/QutBioacoustics/audio-analysis/compare/$old_tag_name...$tag_name)"
$commit_summary = exec { git log --no-merges --pretty=format:"%h %an - %s" "$old_tag_name...$tag_name" -- . ':(exclude,icase)*.r' }
$commit_summary = ($commit_summary | % { "- " + $_ }) -join "`n"
$release_message = "Version $tag_name`n`n$compare_message`n`n$commit_summary"
$env:ApReleaseMessage = $release_message
$release_title = "Ecoacoustics Audio Analysis Software $tag_name"
$env:ApReleaseTitle = $release_title

echo "Release strings:`n$release_title`n$release_message"


if (!$ci) {
	# create and upload a github release
	echo "creating github release"
  
	$artifacts = ((ls .\src\AnalysisPrograms\bin\*.zip) | % { "-a " + $_ }) -join " "
  
	hub release create "$(if($pre_release){"-p"})" $artifacts -m $release_message $tag_name
  
	echo "Release created!"
  
}