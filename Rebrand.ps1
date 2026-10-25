# Replaces the repository's brand (PanelSwWix4) with the Brand defined in Signing.props, in every tracked file except
# Signing.props and this script.
$sourceBrand = 'PanelSwWix4'

$brand = ([xml](Get-Content "$PSScriptRoot/Signing.props" -Raw)).Project.PropertyGroup.Brand
if (-not $brand) { throw "Brand is not defined in Signing.props" }

$skip = @('Signing.props', 'Rebrand.ps1')
$files = @(git -C $PSScriptRoot -c core.quotepath=off grep -liw $sourceBrand | Where-Object { $skip -notcontains $_ })
$files | %{ (Get-Content $_) -replace "PanelSwWix4", $brand | Set-Content $_ }
Write-Host "Rebranded $($files.Count) file(s) to $brand"
