$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'
$version = [System.Reflection.Assembly]::LoadFile("$root\Akka.Persistence.DocumentDb\bin\Release\Akka.Persistence.DocumentDb.dll").GetName().Version
$versionStr = "{0}.{1}.{2}-alpha" -f ($version.Major, $version.Minor, $version.Build)

Write-Host "Setting .nuspec version tag to $versionStr"

$content = (Get-Content $root\Akka.Persistence.DocumentDb\Akka.Persistence.DocumentDb.nuspec) 
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\nuget\Akka.Persistence.DocumentDb.compiled.nuspec

& $root\NuGet\NuGet.exe pack $root\nuget\Akka.Persistence.DocumentDb.compiled.nuspec -OutputDirectory $root\Akka.Persistence.DocumentDb\bin\Release