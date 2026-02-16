param (
    [string]$Version = "3.11.9",
    [string]$TargetDir = "ComicRack/Output/Python"
)

$majorMinor = $Version.Substring(0, 4).Replace(".", "")
$dllName = "python$($majorMinor).dll"
$pthName = "python$($majorMinor)._pth"
$targetDllPath = Join-Path $TargetDir $dllName

if (Test-Path $targetDllPath) {
    Write-Host "Portable Python $Version already exists at $TargetDir. Skipping download."
    exit 0
}

Write-Host "Portable Python version mismatch or missing. Cleaning up and downloading $Version..."

if (Test-Path $TargetDir) {
    Remove-Item -Path $TargetDir -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Path $TargetDir -Force


$zipFile = "python_portable.zip"
$downloadUrl = "https://www.python.org/ftp/python/$Version/python-$Version-embed-amd64.zip"

try {
    Invoke-WebRequest -Uri $downloadUrl -OutFile $zipFile
    Write-Host "Exploding archive..."
    Expand-Archive -Path $zipFile -DestinationPath $TargetDir -Force
    Remove-Item $zipFile
    
    # Un-isolate the environment so it can see local scripts like clr_bridge.py
    $pthPath = Join-Path $TargetDir $pthName
    if (Test-Path $pthPath) {
        Write-Host "Patching $pthName for site support..."
        (Get-Content $pthPath) -replace '#import site', 'import site' | Set-Content $pthPath
    }
    
    Write-Host "Portable Python $Version prepared successfully."
}
catch {
    Write-Error "Failed to prepare portable Python: $_"
    exit 1
}
