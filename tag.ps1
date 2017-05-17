# This script has been modified to work with our CI server

param($version)
$ErrorActionPreference = "Stop"

function script:exec {
    [CmdletBinding()]

	param(
		[Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
		[Parameter(Position=1,Mandatory=0)][string]$errorMessage = ("Error executing command: {0}" -f $cmd)
	)
	& $cmd
	if ($lastexitcode -ne 0)
	{
		throw $errorMessage
	}
}



$tag_name = "v$version"
$env:ApTagName = $tag_name
echo "creating tag '$tag_name'"

$tags = git tag
if ($tags -contains $tag_name) {
    Write-Warning "Tag $tag_name already exists - skipping tag creation"
    exit 0
}

exec { git tag -a -m "Version $tag_name" $tag_name }
echo "pushing tags"
exec { git push origin $tag_name } 2>&1
