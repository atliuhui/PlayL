$root = $PSScriptRoot

Get-ChildItem "$root/assets/lyrics/*" -Recurse | Remove-Item -Force -Recurse
Write-Host "removed $root/assets/lyrics/*"

Get-ChildItem "$root/indices/*" -Recurse | Remove-Item -Force -Recurse
Write-Host "removed $root/indices/*"
