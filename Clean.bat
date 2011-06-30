ECHO OFF
ECHO ----------------------------------
ECHO Clean
ECHO ----------------------------------
Set MSBUILD40=C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe

If Exist Output Echo Delete output Folder
If Exist Output RD /q/s Output

If Exist TestResults Echo Delete TestResults Folder
If Exist TestResults RD /q/s TestResults

%MSBUILD40% Build.xml /v:m /t:CleanProjectsDebugAndRelease /p:Configuration=Debug;X64="%ProgramFiles(x86)%"
%MSBUILD40% Build.xml /v:m /t:CleanProjectsDebugAndRelease /p:Configuration=Release;X64="%ProgramFiles(x86)%"

:END
Echo Clean now
