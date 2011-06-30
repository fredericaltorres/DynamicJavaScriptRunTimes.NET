<#
Build Script
#>
function DeleteFolder($file){
	if (Test-Path $file){
		Remove-Item $file -Force -Recurse
	}
}
function DeleteFile($file){
	if (Test-Path $file){
		Remove-Item $file -Force
	}
}
function is64bit() { 
	return ([IntPtr]::Size -eq 8) 
}
function get-programfilesdir() {
  if (is64bit -eq $true) {
    (Get-Item "Env:ProgramFiles(x86)").Value
  }
  else {
    (Get-Item "Env:ProgramFiles").Value
  }
}
function get-X64() {
  if (is64bit -eq $true) {
    return "X64"
  }
  else {
    return ""
  }
}
function LoadFile($file){
	$lines = Get-Content $file
	$text  = ""
	foreach($line in $lines){
		$text += $line + "`r`n"
	}
	return $text
}

#cd "C:\DVT\.NET\DSSharpLibrary"

"Build..."

$CLR_VERSION					= "0.1.10.0"
$OS_VERSION						= "0.1.10.0"
$MSBUILD40						= "C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
$NUGET							= ".\nuget.exe"
$X64							= get-X64

# Clean stuff before we start
DeleteFolder "Output"

# Create version file

$Properties = "/p:Configuration=Debug;X64=$X64"
& $MSBUILD40 Build.xml /v:m /t:BuildDebug $Properties
if($LASTEXITCODE -ne 0){ throw "Debug build failed" }

$Properties = "/p:Configuration=Release;X64=$X64"
& $MSBUILD40 Build.xml /v:m /t:BuildRelease $Properties
if($LASTEXITCODE -ne 0){ throw "Release build failed" }

"Done"
