# Delete all 'nul' files created by incorrect bash redirection
# Windows reserves 'nul' as a device name, so these files require special handling
# Run with: powershell -ExecutionPolicy Bypass -File delete_nul.ps1

$files = Get-ChildItem -Path 'C:\Git\Wayfarer' -Filter 'nul' -Recurse -Force
$count = 0
foreach($f in $files) {
    # Use \\?\ prefix to bypass Windows reserved name check
    $path = '\\?\' + $f.FullName
    try {
        [System.IO.File]::Delete($path)
        $count++
    } catch {
        Write-Host "Failed to delete: $($f.FullName)"
    }
}
Write-Host "Deleted $count nul files"
