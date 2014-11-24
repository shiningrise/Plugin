echo off

path %SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\

msbuild.exe ..\Plugin.sln /t:Rebuild /p:Configuration=Release /p:VisualStudioVersion=12.0

..\NuGet.exe pack Plugin.Interfaces.csproj -Prop Configuration=Release
pause