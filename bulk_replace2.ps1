Get-ChildItem -Path "src" -Recurse -Filter "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $modified = $content `
        -replace '\bSpotId\b', 'LocationId' `
        -replace '\bspot\b', 'location' `
        -replace '\bSpots\b', 'Locations' `
        -replace '\bspots\b', 'locations' `
        -replace '\bCurrentLocationSpot\b', 'CurrentLocation' `
        -replace '\blocationSpots\b', 'locations' `
        -replace '\bLocationSpots\b', 'Locations' `
        -replace '\bGetSpot\b', 'GetLocation' `
        -replace '\bGetSpots\b', 'GetLocations' `
        -replace '\bAddSpot\b', 'AddLocation' `
        -replace '\bSetSpot\b', 'SetLocation' `
        -replace '\bGetSpotsForVenue\b', 'GetLocationsForVenue' `
        -replace '\bGetSpotsForLocation\b', 'GetLocationsForVenue' `
        -replace '\bGetAllLocationSpots\b', 'GetAllLocations' `
        -replace '\bGetLocationSpotsForLocation\b', 'GetLocationsForVenue' `
        -replace '\bGetTravelHubSpot\b', 'GetTravelHubLocation' `
        -replace '\bIsSpotKnown\b', 'IsLocationKnown' `
        -replace '\bMarkSpotDiscovered\b', 'MarkLocationDiscovered' `
        -replace '\bIsSpotDiscovered\b', 'IsLocationDiscovered' `
        -replace '\bGetDefaultEntranceSpot\b', 'GetDefaultEntranceLocation' `
        -replace '\bGetAccessibleSpotsFromCurrent\b', 'GetAccessibleLocationsFromCurrent' `
        -replace '\bGetServiceSpots\b', 'GetServiceLocations' `
        -replace '\bSpotExists\b', 'LocationExists' `
        -replace '\bIsSpotTravelHub\b', 'IsLocationTravelHub' `
        -replace '\bGetActiveSpotProperties\b', 'GetActiveLocationProperties' `
        -replace '\bSpotHasProperty\b', 'LocationHasProperty' `
        -replace '\bGetSpotsWithProperty\b', 'GetLocationsWithProperty' `
        -replace '\bGetSpotDetail\b', 'GetLocationDetail' `
        -replace '\bGenerateSpotDetail\b', 'GenerateLocationDetail' `
        -replace '\bGenerateSpotActions\b', 'GenerateLocationActions' `
        -replace '\bGetNPCsForLocationSpotAndTime\b', 'GetNPCsForLocationAndTime' `
        -replace '\bValidateLocationSpot\b', 'ValidateLocation' `
        -replace 'Venue and spot', 'Venue and location' `
        -replace 'Venue spot', 'Venue location' `
        -replace '\bspotId\b', 'locationId'

    if ($content -ne $modified) {
        Set-Content -Path $_.FullName -Value $modified -NoNewline
        Write-Host "Updated: $($_.FullName)"
    }
}
