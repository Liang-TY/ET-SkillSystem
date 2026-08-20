Add-Type -AssemblyName System.Drawing
$base = "E:\Projects\cs\et9lockStepYIUITest\Notes\dnf源码研究"
foreach ($i in 1, 4, 5) {
    $src = Join-Path $base "pdf图片\ANI的坐标的逻辑详解-牧野-图片-$i.png"
    $dst = Join-Path $base "坐标系专题\资料\page-$i.jpg"
    $img = [System.Drawing.Image]::FromFile($src)
    $codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
    $ep = New-Object System.Drawing.Imaging.EncoderParameters(1)
    $ep.Param[0] = [System.Drawing.Imaging.EncoderParameter]::new([System.Drawing.Imaging.Encoder]::Quality, [long]90)
    $img.Save($dst, $codec, $ep)
    $img.Dispose()
    Write-Host "saved $dst"
}
