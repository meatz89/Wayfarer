Get-ChildItem -Path "src" -Recurse -Filter "*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $modified = $content `
        -replace '\.SpotProperties\b', '.LocationProperties' `
        -replace '\bLocationSpotEntry\b', 'LocationEntry' `
        -replace '\.CurrentLocationSpot\b', '.CurrentLocation' `
        -replace 'LocationEntry\)\.Id\b', 'LocationEntry).LocationId'

    if ($content -ne $modified) {
        Set-Content -Path $_.FullName -Value $modified -NoNewline
        Write-Host "Updated: $($_.FullName)"
    }
}

# Update Razor files too
Get-ChildItem -Path "src" -Recurse -Filter "*.razor" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $modified = $content `
        -replace '\.SpotProperties\b', '.LocationProperties' `
        -replace '\bSpotPropertyType\b', 'LocationPropertyType' `
        -replace '\.CurrentLocationSpot\b', '.CurrentLocation'

    if ($content -ne $modified) {
        Set-Content -Path $_.FullName -Value $modified -NoNewline
        Write-Host "Updated: $($_.FullName)"
    }
}
