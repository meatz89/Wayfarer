$outputFile = "combined.json"
$fileTypes = @("*.json")  # Add more file types as needed

$combinedData = @{}

foreach ($fileType in $fileTypes) {
    Get-ChildItem -Path . -Recurse -Filter $fileType | ForEach-Object {
        $fileContent = Get-Content $_.FullName -Raw
        $combinedData[$_.FullName] = "`n$($fileContent)`n`n"
    }
}

# Convert and ensure proper JSON formatting
$formattedJson = ($combinedData | ConvertTo-Json -Depth 10) -replace "\\r\\n", "`n" -replace "\\n", "`n"

Set-Content -Path $outputFile -Value $formattedJson
