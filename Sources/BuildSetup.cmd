@echo off

if "%VSCMD_VER%" == "" (
	echo This .CMD file must be run from "Developer Command Prompt for VS 2022" or later
	exit /B
)

@echo on

rem Force building everything, so all the tools are built there.
msbuild -r -p:Configuration=Release LogicCircuit.sln
@if "%ERRORLEVEL%" NEQ "0" (exit /B)

rem Publish for 64 bits
dotnet publish LogicCircuit\LogicCircuit.csproj --sc -c Release -r win-x64 -o LogicCircuit\bin\Release_64\Publish
@if "%ERRORLEVEL%" NEQ "0" (exit /B)

rem Publish for 32 bits
dotnet publish LogicCircuit\LogicCircuit.csproj --sc -c Release -r win-x86 -o LogicCircuit\bin\Release_32\Publish
@if "%ERRORLEVEL%" NEQ "0" (exit /B)

rem Buiding Setup

msbuild -r -p:Configuration=Release;Platform=x64 Setup\Setup.wixproj
@if "%ERRORLEVEL%" NEQ "0" (exit /B)

msbuild -r -p:Configuration=Release;Platform=x86 Setup\Setup.wixproj
@if "%ERRORLEVEL%" NEQ "0" (exit /B)

rem Buiding Setup Zip files

msbuild -t:ZipSetup Setup -p:Platform=x64
@if "%ERRORLEVEL%" NEQ "0" (exit /B)

msbuild -t:ZipSetup Setup -p:Platform=x86
@if "%ERRORLEVEL%" NEQ "0" (exit /B)
