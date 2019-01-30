
$streamDeckExePath = "C:\Program Files\Elgato\StreamDeck\StreamDeck.exe"
$bindir = $PSScriptRoot + "\bin\Debug\netcoreapp2.1\win-x64"
$manifestFile = $bindir +"\manifest.json"

$manifestContent = Get-Content $manifestFile | Out-String
$json = ConvertFrom-JSON $manifestcontent
$uuidAction = $json.Actions[0].UUID
$pluginID = $uuidAction.substring(0, $uuidAction.Length - ".action".Length)
$destDir = $env:APPDATA + "\Elgato\StreamDeck\Plugins\" + $pluginID + ".sdPlugin"

$pluginName = split-path $PSScriptRoot -leaf

Get-Process StreamDeck,$pluginName | Stop-Process –force -ErrorAction SilentlyContinue 

Copy-Item -Path $bindir -Exclude '.vs' -Recurse -Destination $destDir -Force

Write-Host "Script Completed"

Start-Process $streamDeckExePath
exit 0
