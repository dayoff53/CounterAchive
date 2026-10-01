param([string]$ManifestPath = 'Legacy/Carve/2026-09-30/manifest.json')

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -Raw -LiteralPath (Join-Path $root $ManifestPath) | ConvertFrom-Json
$failures = [Collections.Generic.List[string]]::new()

function Assert-Hash($relativePath, $expectedHash) {
    $path = Join-Path $root $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing file: $relativePath")
    } elseif ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $expectedHash) {
        $failures.Add("Changed file: $relativePath")
    }
}

foreach ($entry in $manifest.quarantined) {
    Assert-Hash $entry.archivePath $entry.sha256
    Assert-Hash ($entry.archivePath + '.meta') $entry.metaSha256
    foreach ($path in @($entry.originalPath, ($entry.originalPath + '.meta'))) {
        if (Test-Path -LiteralPath (Join-Path $root $path)) {
            $failures.Add("Quarantined file is active again: $path")
        }
    }
}

# This is a historical preservation check. Later intentional asset edits will
# fail it; review those changes, never silently rewrite the original snapshot.
foreach ($entry in $manifest.preservedAssets) {
    Assert-Hash $entry.path $entry.sha256
}

$retiredGuids = @{}
foreach ($entry in $manifest.quarantined) { $retiredGuids[$entry.guid] = $true }
foreach ($meta in Get-ChildItem -LiteralPath (Join-Path $root 'Assets') -Recurse -Filter '*.cs.meta' -File) {
    $match = [regex]::Match([IO.File]::ReadAllText($meta.FullName), '(?m)^guid: ([0-9a-f]{32})')
    if ($match.Success -and $retiredGuids.ContainsKey($match.Groups[1].Value)) {
        $failures.Add("Retired script GUID reused in Assets: $($meta.FullName)")
    }
}

# Conservative source scan, not a C# compiler: comments are ignored, but string
# literals may be reported. Inspect any result before classifying it as a dependency.
$retiredTypes = @($manifest.quarantined | ForEach-Object { $_.declaredTypes } | Sort-Object -Unique)
$pattern = '\b(?:' + (($retiredTypes | ForEach-Object { [regex]::Escape($_) }) -join '|') + ')\b'
foreach ($source in Get-ChildItem -LiteralPath (Join-Path $root 'Assets') -Recurse -Filter '*.cs' -File) {
    $code = [IO.File]::ReadAllText($source.FullName)
    $code = [regex]::Replace($code, '(?s)/\*.*?\*/|(?m)//[^\r\n]*', '')
    $hits = @([regex]::Matches($code, $pattern) | ForEach-Object { $_.Value } | Sort-Object -Unique)
    if ($hits.Count -gt 0) {
        $failures.Add("Retired type reference: $($source.FullName): $($hits -join ', ')")
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Output $_ }
    throw "Quarantine verification failed: $($failures.Count) issue(s)."
}

[pscustomobject]@{
    QuarantinedScripts = $manifest.quarantined.Count
    PreservedAssetFiles = $manifest.preservedAssets.Count
    ArchiveHashes = 'PASS'
    PreservedAssetHashes = 'PASS'
    RetiredSourceReferences = 'NONE (static scan)'
    UnityCompilation = 'Not tested by this script'
}
