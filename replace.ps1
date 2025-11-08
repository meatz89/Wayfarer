param(
    [Parameter(Mandatory=$true)]
    [string]$Find,
    
    [Parameter(Mandatory=$true)]
    [string]$Replace
)

function Get-CasePreservedReplacement {
    param([string]$original, [string]$replacement)
    
    $result = ""
    for ($i = 0; $i -lt $replacement.Length; $i++) {
        if ($i -lt $original.Length) {
            if ([char]::IsUpper($original[$i])) {
                $result += [char]::ToUpper($replacement[$i])
            } else {
                $result += [char]::ToLower($replacement[$i])
            }
        } else {
            $result += $replacement[$i]
        }
    }
    return $result
}

function Replace-PreserveCase {
    param([string]$text, [string]$find, [string]$replace)
    
    $pattern = [regex]::Escape($find)
    $regex = [regex]::new($pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    
    return $regex.Replace($text, {
        param($match)
        return Get-CasePreservedReplacement -original $match.Value -replacement $replace
    })
}

$items = Get-ChildItem -Path . -Recurse -Force

$folders = $items | Where-Object { $_.PSIsContainer } | Sort-Object -Property FullName -Descending
$files = $items | Where-Object { -not $_.PSIsContainer }

foreach ($folder in $folders) {
    $newName = Replace-PreserveCase -text $folder.Name -find $Find -replace $Replace
    if ($newName -ne $folder.Name) {
        Rename-Item -Path $folder.FullName -NewName $newName
    }
}

foreach ($file in $files) {
    $newName = Replace-PreserveCase -text $file.Name -find $Find -replace $Replace
    if ($newName -ne $file.Name) {
        Rename-Item -Path $file.FullName -NewName $newName
    }
}