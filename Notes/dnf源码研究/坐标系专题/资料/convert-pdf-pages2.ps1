Add-Type -AssemblyName System.Drawing
$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
New-Item -ItemType Directory -Force -Path "jpg" | Out-Null
Get-ChildItem -Filter *.png | Where-Object { $_.Name -match "-1\.png$|-4\.png$|-5\.png$" } | ForEach-Object {
    $img = [System.Drawing.Image]::FromFile($_.FullName)
    $dst = Join-Path (Get-Location) ("jpg\" + $_.BaseName + ".jpg")
    $ep = New-Object System.Drawing.Imaging.EncoderParameters(1)
    $ep.Param[0] = [System.Drawing.Imaging.EncoderParameter]::new([System.Drawing.Imaging.Encoder]::Quality, [long]90)
    $img.Save($dst, $codec, $ep)
    $img.Dispose()
    Write-Host ("saved " + $dst)
}
