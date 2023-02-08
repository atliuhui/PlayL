$version = '0.1.0'
$root = Get-Location

$name = 'LyricsAdmin'
Set-Location -Path "src\$name"
# https://docs.microsoft.com/zh-cn/dotnet/core/tools/dotnet-publish
"dotnet publish --configuration Release --output $root/dist/admin -p:PublishProfile=osx-x64 -p:Version=$version" | bash
Set-Location -Path "$root"

$name = 'LyricsAgent'
Set-Location -Path "src\$name"
# https://docs.microsoft.com/zh-cn/dotnet/core/tools/dotnet-publish
"dotnet publish --configuration Release --output $root/dist/agent -p:PublishProfile=osx-x64 -p:Version=$version" | bash
Set-Location -Path "$root"
