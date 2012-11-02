@echo off

set config=%1
if "%config%" == "" (
   set config=Debug
)

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Solutionizer.sln /p:Configuration="%config%"
