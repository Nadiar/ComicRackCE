# ComicRackCE Ctags Update Wrapper
# Use $PSScriptRoot to avoid hardcoded FQNs
$WorkspaceRoot = (Resolve-Path "$PSScriptRoot\..\..\..").Path
$BaseScript = Join-Path $WorkspaceRoot "scripts\update-ctags-base.ps1"
$ProjectRoot = (Resolve-Path "$PSScriptRoot\..\..").Path

$SourcePaths = @(
    (Join-Path $ProjectRoot "ComicRack"),
    (Join-Path $ProjectRoot "ComicRack.Engine"),
    (Join-Path $ProjectRoot "ComicRack.Plugins")
)
$TagsDir = Join-Path $ProjectRoot ".github\ctags"

if (-not (Test-Path $BaseScript)) {
    Write-Error "Base script not found at $BaseScript"
    exit 1
}

& $BaseScript -SourcePaths $SourcePaths -TagsDir $TagsDir -ProjectRoot $ProjectRoot
