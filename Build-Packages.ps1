<#
.SYNOPSIS
    Creates NuGet packages
.DESCRIPTION
    Creates NuGet packages for the projects in this solution.
#>

# Restore project dependencies (e.g., JSON.Net)
dotnet.exe restore
# Create the NuGet packages.
dotnet.exe pack --include-source --include-symbols --configuration Release --output $PSScriptRoot\nupkg