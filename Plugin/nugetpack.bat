echo off

path %SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\

msbuild.exe ..\PluginMvcWeb.sln /t:Rebuild /p:Configuration=Release /p:VisualStudioVersion=12.0

NuGet.exe pack PluginMvc.Framework.csproj -Prop Configuration=Release
pause