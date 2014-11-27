echo off

path %SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\

msbuild.exe Plugin.Mvc.csproj /t:Rebuild /p:Configuration=Release /p:VisualStudioVersion=12.0

..\NuGet.exe pack Plugin.Mvc.csproj -Prop Configuration=Release
pause