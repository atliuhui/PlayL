$version = '0.1.0'
$root = Get-Location

$name = 'LyricsAdmin'
Set-Location -Path "src\$name"
# https://docs.microsoft.com/zh-cn/dotnet/core/tools/dotnet-publish
"dotnet publish --configuration Release --output $root\dist\admin -p:PublishProfile=win-x64 -p:Version=$version" | cmd
Set-Location -Path "$root"

$name = 'LyricsAgent'
Set-Location -Path "src\$name"
# https://docs.microsoft.com/zh-cn/dotnet/core/tools/dotnet-publish
"dotnet publish --configuration Release --output $root\dist\agent -p:PublishProfile=win-x64 -p:Version=$version" | cmd
Set-Location -Path "$root"
