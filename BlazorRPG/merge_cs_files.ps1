# PowerShell Script to merge all .cs, .cshtml, and .razor files in the current folder and its subfolders
# Define the output file name
$outputFile = "./MergedCode.txt"

# Ensure the output file doesn't already exist
if (Test-Path $outputFile) {
    Remove-Item $outputFile
}

# Get all files with the specified extensions in the folder and subfolders
$files = Get-ChildItem -Path . -Recurse -Include *.cs, *.cshtml, *.razor

# Loop through each file and append its content to the output file
foreach ($file in $files) {
    # Add a file header for better organization
    "//================ ${file} ================" | Out-File -Append -FilePath $outputFile
    Get-Content $file.FullName | Out-File -Append -FilePath $outputFile
    "`n`n" | Out-File -Append -FilePath $outputFile  # Add some spacing between files
}

Write-Host "All .cs, .cshtml, and .razor files have been merged into $outputFile"