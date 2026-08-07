# Maakt de downloadbestanden voor de cursus.
#
#   static/downloads/shopwave-start-<les>.zip   het startpakket bij de oefeningen
#   static/downloads/shopwave-<les>.zip         de uitgewerkte oplossing
#   static/downloads/shopwave-oplossingen-alle-lessen.zip
#
# Bouwrommel (bin, obj, .vs, *.user, gegenereerde .feature.cs) gaat er niet in.

$ErrorActionPreference = 'Stop'

$root      = Split-Path -Parent $PSScriptRoot
$downloads = Join-Path $root 'static\downloads'
$staging   = Join-Path $env:TEMP ('shopwave-zip-' + [Guid]::NewGuid().ToString('N').Substring(0, 8))

New-Item -ItemType Directory -Force -Path $downloads | Out-Null
New-Item -ItemType Directory -Force -Path $staging   | Out-Null

function Kopieer-Schoon {
    param([string]$Bron, [string]$Doel)

    # robocopy laat bin, obj en .vs meteen links liggen. Copy-Item struikelt over
    # de indexbestanden die Visual Studio openhoudt in .vs.
    $uit = robocopy $Bron $Doel /E `
        /XD bin obj .vs .idea `
        /XF *.user *.suo *.feature.cs `
        /NFL /NDL /NJH /NJS /NP

    # robocopy geeft 0 tot 7 terug bij succes; 8 en hoger is een echte fout.
    if ($LASTEXITCODE -ge 8) {
        throw "robocopy faalde op $Bron met code $LASTEXITCODE`n$uit"
    }

    $global:LASTEXITCODE = 0
}

function Maak-Zip {
    param([string]$Map, [string]$ZipPad)

    if (Test-Path $ZipPad) { Remove-Item -Force $ZipPad }
    Compress-Archive -Path $Map -DestinationPath $ZipPad -CompressionLevel Optimal
}

# Startpakketten
foreach ($map in Get-ChildItem -Path (Join-Path $root 'startpakketten') -Directory) {
    $tijdelijk = Join-Path $staging ('start-' + $map.Name)
    Kopieer-Schoon -Bron $map.FullName -Doel $tijdelijk
    $zip = Join-Path $downloads ('shopwave-start-' + $map.Name + '.zip')
    Maak-Zip -Map $tijdelijk -ZipPad $zip
    $kb = [math]::Round((Get-Item $zip).Length / 1KB)
    Write-Output ("startpakket  {0,-32} {1,6} kB" -f $map.Name, $kb)
}

# Oplossingen
$alle = Join-Path $staging 'oplossingen'
New-Item -ItemType Directory -Force -Path $alle | Out-Null

foreach ($map in Get-ChildItem -Path (Join-Path $root 'solutions') -Directory) {
    $tijdelijk = Join-Path $staging $map.Name
    Kopieer-Schoon -Bron $map.FullName -Doel $tijdelijk
    $zip = Join-Path $downloads ('shopwave-' + $map.Name + '.zip')
    Maak-Zip -Map $tijdelijk -ZipPad $zip
    $kb = [math]::Round((Get-Item $zip).Length / 1KB)
    Write-Output ("oplossing    {0,-32} {1,6} kB" -f $map.Name, $kb)

    Copy-Item -Path $tijdelijk -Destination (Join-Path $alle $map.Name) -Recurse -Force
}

$zipAlle = Join-Path $downloads 'shopwave-oplossingen-alle-lessen.zip'
if (Test-Path $zipAlle) { Remove-Item -Force $zipAlle }
Compress-Archive -Path (Join-Path $alle '*') -DestinationPath $zipAlle -CompressionLevel Optimal
$kb = [math]::Round((Get-Item $zipAlle).Length / 1KB)
Write-Output ("alle lessen  {0,-32} {1,6} kB" -f '', $kb)

Remove-Item -Recurse -Force $staging
