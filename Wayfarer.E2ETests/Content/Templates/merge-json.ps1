# merge-json.ps1

$OutputPath = "merged.json"

# Remove the output file if it exists
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath
}

# Get all JSON files in the current directory
$JsonFiles = Get-ChildItem -Path . -Filter *.json

foreach ($File in $JsonFiles) {
    $Header = "=== $($File.Name) ==="
    $RawContent = Get-Content -Path $File.FullName -Raw

    Add-Content -Path $OutputPath -Value $Header
    Add-Content -Path $OutputPath -Value $RawContent
    Add-Content -Path $OutputPath -Value ""  # Add a blank line between entries
}
