# Script to combine files by type into single files

# Define the directory where the script is located
$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Define the file types to search for
$fileExtensions = @("razor", "cs")

# Loop through each file extension
foreach ($extension in $fileExtensions) {
    # Get all files with the current extension in the script's directory and subdirectories
    $files = Get-ChildItem -Path $ScriptDirectory -Recurse -Filter "*.$extension"

    # Construct the output file name, replacing '.' in the extension for clean file names
    $cleanExtension = $extension -replace '\.', ''
    $outputFile = Join-Path -Path $ScriptDirectory -ChildPath "All${cleanExtension}Files.$extension"

    # Clear the output file if it already exists
    if (Test-Path $outputFile) {
        Remove-Item -Path $outputFile
    }

    # Iterate through each file and append its contents to the output file
    foreach ($file in $files) {
        # Append the file name as a comment (optional, for reference)
        Add-Content -Path $outputFile -Value "`n/* File: $($file.FullName) */`n"

        # Append the file content
        Get-Content -Path $file.FullName | Add-Content -Path $outputFile
    }

    Write-Output "Combined $extension files into $outputFile"
}

Write-Output "File combination completed!"
