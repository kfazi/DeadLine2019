param (
    [int]$id
)

If ([IO.File]::Exists("$PSScriptRoot\deploy\server0.pid")) {
    $processId = [IO.File]::ReadAllText("$PSScriptRoot\deploy\server$id.pid")
    Stop-Process -Id $processId
}

pushd "$PSScriptRoot\deploy\"

$processId = (Start-Process -PassThru -FilePath DeadLine2019.exe -ArgumentList "`"$PSScriptRoot`" connection$id.json").Id
[IO.File]::WriteAllText("$PSScriptRoot\deploy\server$id.pid", $processId)

popd
