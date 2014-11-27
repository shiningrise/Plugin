echo off

path %SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\

msbuild.exe Plugin.csproj /t:Rebuild /p:Configuration=Release /p:VisualStudioVersion=12.0

..\NuGet.exe pack Plugin.csproj -Prop Configuration=Release
pause