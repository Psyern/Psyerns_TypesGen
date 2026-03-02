Add-Type -AssemblyName System.Drawing

$bmp = New-Object System.Drawing.Bitmap(256, 256)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.Clear([System.Drawing.Color]::FromArgb(30, 30, 30))

# Blue bars representing XML/JSON data rows
$brush1 = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(0, 150, 255))
$g.FillRectangle($brush1, 20, 40, 216, 28)
$g.FillRectangle($brush1, 20, 80, 180, 28)
$g.FillRectangle($brush1, 20, 120, 150, 28)

# Yellow accent bar
$brush2 = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 200, 0))
$g.FillRectangle($brush2, 20, 160, 120, 28)

# "TH" text
$font = New-Object System.Drawing.Font('Segoe UI', 48, [System.Drawing.FontStyle]::Bold)
$g.DrawString('TH', $font, [System.Drawing.Brushes]::White, 90, 185)

$g.Dispose()

# Convert bitmap to icon
$iconHandle = $bmp.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($iconHandle)

$outPath = Join-Path $PSScriptRoot 'DayZTypesHelper\app.ico'
$fs = [System.IO.File]::Create($outPath)
$icon.Save($fs)
$fs.Close()
$icon.Dispose()
$bmp.Dispose()

Write-Host "Icon created at: $outPath"
