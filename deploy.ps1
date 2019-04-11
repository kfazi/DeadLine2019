param (
    [switch]$runAll
)

If ([System.IO.Directory]::Exists("C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\")) {
    $vcDir = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\"
} ElseIf ([System.IO.Directory]::Exists("C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\")) {
    $vcDir = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\"
} ElseIf ([System.IO.Directory]::Exists("C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\")) {
    $vcDir = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build\"
}

pushd $vcDir
cmd /c "vcvars64.bat&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
  }
}
popd

If ([IO.File]::Exists("$PSScriptRoot\deploy\server0.pid")) {
    $processId = [IO.File]::ReadAllText("$PSScriptRoot\deploy\server0.pid")
    Stop-Process -Id $processId
}

If ([IO.File]::Exists("$PSScriptRoot\deploy\server1.pid")) {
    $processId = [IO.File]::ReadAllText("$PSScriptRoot\deploy\server1.pid")
    Stop-Process -Id $processId
}

If ([IO.File]::Exists("$PSScriptRoot\deploy\server2.pid")) {
    $processId = [IO.File]::ReadAllText("$PSScriptRoot\deploy\server2.pid")
    Stop-Process -Id $processId
}

msbuild $PSScriptRoot\DeadLine2019\DeadLine2019.csproj /p:OutputPath=$PSScriptRoot\deploy /p:Configuration=Release /p:Platform="x64" /p:AllowUnsafeBlocks=true

pushd "$PSScriptRoot\deploy\"
If ($runAll) {
    $processId = (Start-Process -PassThru -FilePath DeadLine2019.exe -ArgumentList "`"$PSScriptRoot`" connection0.json").Id
    [IO.File]::WriteAllText("$PSScriptRoot\deploy\server0.pid", $processId)
}

$processId = (Start-Process -PassThru -FilePath DeadLine2019.exe -ArgumentList "`"$PSScriptRoot`" connection1.json").Id
[IO.File]::WriteAllText("$PSScriptRoot\deploy\server1.pid", $processId)

$processId = (Start-Process -PassThru -FilePath DeadLine2019.exe -ArgumentList "`"$PSScriptRoot`" connection2.json").Id
[IO.File]::WriteAllText("$PSScriptRoot\deploy\server2.pid", $processId)
popd

$branch= &git rev-parse --abbrev-ref HEAD
$time = (Get-Date).ToString('ddHHmmss')
git add -A
git commit -m 'Release'
git tag $branch-$time
# git push --tags
