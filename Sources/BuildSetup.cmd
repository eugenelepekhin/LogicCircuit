@echo off

if "%VSCMD_VER%" == "" (
	echo This .CMD file must be run from "Developer Command Prompt for VS 2022" or later
	exit /B
)

@echo on

rem Forse building everything, so all the tools are built there.
msbuild -r -p:Configuration=Release LogicCircuit.sln

dotnet publish LogicCircuit\LogicCircuit.csproj --sc -c Release -r win-x64 -o LogicCircuit\bin\Release_64\Publish
dotnet publish LogicCircuit\LogicCircuit.csproj --sc -c Release -r win-x86 -o LogicCircuit\bin\Release_32\Publish

msbuild -r -p:Configuration=Release;Platform=x64 Setup\Setup.wixproj
msbuild -r -p:Configuration=Release;Platform=x86 Setup\Setup.wixproj
