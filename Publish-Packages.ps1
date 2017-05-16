<#
.SYNOPSIS
    Adds all packages to NuGet source
.DESCRIPTION
    Adds all packages in current directory (non recursive) to NuGet source.
.EXAMPLE
    PS C:\> .\Publish-Packages \\my\feed
    Publishes packages to a network location.
.INPUTS
    Path to feed location or the name of a feed registered with nuget source.
#>
param(
    [Parameter(Mandatory=$true, HelpMessage="NuGet source to add feeds to.")]
    [string]
    $source
)
foreach ($item in Get-Item *.nupkg) {
    NuGet.exe add $item -Source $source
}