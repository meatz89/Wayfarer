Get-ChildItem -Path "src" -Recurse -Filter "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $modified = $content `
        -replace 'LocationSpotTypes', 'LocationTypes' `
        -replace 'LocationSpotDTO', 'LocationDTO' `
        -replace 'SpotPropertiesDTO', 'LocationPropertiesDTO' `
        -replace 'LocationSpotParser', 'LocationParser' `
        -replace 'LocationSpotValidator', 'LocationValidator' `
        -replace 'LocationSpotAction', 'LocationAction' `
        -replace 'LocationSpotTags', 'LocationTags' `
        -replace 'LocationSpotActionManager', 'LocationActionManager' `
        -replace 'LocationSpotManager', 'LocationManager' `
        -replace 'SpotDescriptionGenerator', 'LocationDescriptionGenerator' `
        -replace 'SpotLevel', 'LocationLevel' `
        -replace 'SpotPropertyType', 'LocationPropertyType' `
        -replace '\bLocationSpot\b', 'Location' `
        -replace 'ConvertDTOToLocationSpot', 'ConvertDTOToLocation'

    if ($content -ne $modified) {
        Set-Content -Path $_.FullName -Value $modified -NoNewline
        Write-Host "Updated: $($_.FullName)"
    }
}
